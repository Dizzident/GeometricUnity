using Gu.Branching;
using Gu.Core;

namespace Gu.Interop;

/// <summary>
/// Result of a GPU Krylov solve.
/// </summary>
public sealed class GpuKrylovResult
{
    /// <summary>Solution vector (edge-valued).</summary>
    public required double[] Solution { get; init; }

    /// <summary>Number of CG iterations performed.</summary>
    public required int Iterations { get; init; }

    /// <summary>Final relative residual norm.</summary>
    public required double FinalRelativeResidual { get; init; }

    /// <summary>Whether the solver converged within tolerance.</summary>
    public required bool Converged { get; init; }

    /// <summary>Whether the solver terminated due to negative curvature (pAp &lt;= 0).</summary>
    public bool TerminatedNegativeCurvature { get; init; }
}

/// <summary>
/// GPU-accelerated conjugate gradient solver for Gauss-Newton normal equations.
/// Solves (J^T J + lambda I) x = rhs entirely on the GPU using native BLAS-like
/// primitives (axpy, inner_product, scale, copy) and matrix-free Jacobian/adjoint
/// actions from CUDA Stage 2.
///
/// All intermediate vectors remain in GPU memory; only the final solution is
/// downloaded to CPU.
/// </summary>
public sealed class GpuKrylovSolver : IDisposable
{
    private readonly INativeBackend _backend;
    private readonly int _maxIterations;
    private readonly double _tolerance;
    private bool _disposed;

    /// <param name="backend">Native backend with GPU primitives.</param>
    /// <param name="maxIterations">Maximum CG iterations. Default: 50.</param>
    /// <param name="tolerance">Relative residual tolerance. Default: 1e-6.</param>
    public GpuKrylovSolver(INativeBackend backend, int maxIterations = 50, double tolerance = 1e-6)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        if (maxIterations < 1)
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Must be >= 1.");
        if (tolerance <= 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Must be > 0.");
        _maxIterations = maxIterations;
        _tolerance = tolerance;
    }

    /// <summary>
    /// Solve (J^T J + lambda I) x = rhs via conjugate gradient on GPU.
    /// </summary>
    /// <param name="omegaBuf">Current omega connection buffer (for J and J^T evaluation).</param>
    /// <param name="rhs">Right-hand side vector (edge-valued, on CPU).</param>
    /// <param name="edgeN">Edge-valued vector dimension (edge_count * dimG).</param>
    /// <param name="faceN">Face-valued vector dimension (face_count * dimG).</param>
    /// <param name="lambda">Gauge penalty / Tikhonov regularization parameter.</param>
    public GpuKrylovResult SolveNormalEquations(
        PackedBuffer omegaBuf,
        double[] rhs,
        int edgeN,
        int faceN,
        double lambda)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var edgeLayout = BufferLayoutDescriptor.CreateSoA("cg-edge", new[] { "c" }, edgeN);
        var faceLayout = BufferLayoutDescriptor.CreateSoA("cg-face", new[] { "c" }, faceN);

        // Allocate GPU buffers for CG vectors
        var xBuf = _backend.AllocateBuffer(edgeLayout);     // solution x
        var rBuf = _backend.AllocateBuffer(edgeLayout);     // residual r
        var pBuf = _backend.AllocateBuffer(edgeLayout);     // search direction p
        var apBuf = _backend.AllocateBuffer(edgeLayout);    // A*p = J^T(J*p) + lambda*p
        var jpBuf = _backend.AllocateBuffer(faceLayout);    // J*p (face-valued, temp)

        try
        {
            // Initialize: x = 0, r = rhs, p = rhs
            _backend.UploadBuffer(xBuf, new double[edgeN]);
            _backend.UploadBuffer(rBuf, rhs);
            _backend.UploadBuffer(pBuf, rhs);

            double rDotR = _backend.InnerProduct(rBuf, rBuf, edgeN);
            double rhsNorm = System.Math.Sqrt(rDotR);
            double tol = _tolerance * rhsNorm;

            if (rhsNorm < 1e-15)
            {
                return new GpuKrylovResult
                {
                    Solution = new double[edgeN],
                    Iterations = 0,
                    FinalRelativeResidual = 0.0,
                    Converged = true,
                };
            }

            int iter = 0;
            bool negCurvature = false;
            for (; iter < _maxIterations; iter++)
            {
                // Compute A*p = J^T(J*p) + lambda*p
                _backend.EvaluateJacobianAction(omegaBuf, pBuf, jpBuf);
                _backend.EvaluateAdjointAction(omegaBuf, jpBuf, apBuf);

                if (lambda > 0)
                    _backend.Axpy(apBuf, lambda, pBuf, edgeN);

                double pAp = _backend.InnerProduct(pBuf, apBuf, edgeN);

                if (pAp <= 0)
                {
                    negCurvature = true;
                    break;
                }

                double alpha = rDotR / pAp;

                // x = x + alpha * p
                _backend.Axpy(xBuf, alpha, pBuf, edgeN);

                // r = r - alpha * ap
                _backend.Axpy(rBuf, -alpha, apBuf, edgeN);

                double rDotRNew = _backend.InnerProduct(rBuf, rBuf, edgeN);

                if (System.Math.Sqrt(rDotRNew) < tol)
                {
                    iter++;
                    rDotR = rDotRNew;
                    break;
                }

                double beta = rDotRNew / rDotR;

                // p = r + beta * p
                _backend.Scale(pBuf, beta, edgeN);
                _backend.Axpy(pBuf, 1.0, rBuf, edgeN);

                rDotR = rDotRNew;
            }

            // Download solution
            var solution = new double[edgeN];
            _backend.DownloadBuffer(xBuf, solution);

            double finalRelRes = rhsNorm > 0 ? System.Math.Sqrt(rDotR) / rhsNorm : 0.0;

            return new GpuKrylovResult
            {
                Solution = solution,
                Iterations = iter,
                FinalRelativeResidual = finalRelRes,
                Converged = finalRelRes < _tolerance,
                TerminatedNegativeCurvature = negCurvature,
            };
        }
        finally
        {
            _backend.FreeBuffer(xBuf);
            _backend.FreeBuffer(rBuf);
            _backend.FreeBuffer(pBuf);
            _backend.FreeBuffer(apBuf);
            _backend.FreeBuffer(jpBuf);
        }
    }

    /// <summary>
    /// Convenience: solve the Gauss-Newton normal equations using semantic types.
    /// Uploads omega, converts rhs, calls the buffer-level solver, and returns a FieldTensor.
    /// </summary>
    public FieldTensor SolveNormalEquations(
        FieldTensor omega,
        FieldTensor rhs,
        GeometryContext geometry,
        int dimG,
        double lambda)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        int edgeN = omega.Coefficients.Length;
        int faceCount = geometry.AmbientSpace.FaceCount
            ?? throw new InvalidOperationException(
                "GeometryContext.AmbientSpace.FaceCount must be set for GPU Krylov solve.");
        int faceN = faceCount * dimG;

        var edgeLayout = BufferLayoutDescriptor.CreateSoA("krylov-omega", new[] { "c" }, edgeN);
        var omegaBuf = _backend.AllocateBuffer(edgeLayout);

        try
        {
            _backend.UploadBuffer(omegaBuf, omega.Coefficients);
            var result = SolveNormalEquations(omegaBuf, rhs.Coefficients, edgeN, faceN, lambda);

            return new FieldTensor
            {
                Label = "krylov_delta",
                Signature = omega.Signature,
                Coefficients = result.Solution,
                Shape = omega.Shape.ToArray(),
            };
        }
        finally
        {
            _backend.FreeBuffer(omegaBuf);
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }
}

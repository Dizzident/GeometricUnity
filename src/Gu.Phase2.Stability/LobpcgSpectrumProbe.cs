using Gu.Branching;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// LOBPCG (Locally Optimal Block Preconditioned Conjugate Gradient)
/// for computing smallest eigenvalues of symmetric operators.
///
/// Implements unpreconditioned block LOBPCG with Rayleigh-Ritz projection.
/// For each iteration, the search space S = [X, R, P] is formed, the small
/// projected eigenproblem S^T H S is solved, and the trial vectors are updated.
/// </summary>
public sealed class LobpcgSpectrumProbe : ISpectrumProbe
{
    public string MethodId => "lobpcg";

    public SpectrumProbeResult ComputeSmallestEigenvalues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        if (op.InputDimension != op.OutputDimension)
            throw new ArgumentException("Eigenvalue computation requires a square operator.");
        if (k < 1)
            throw new ArgumentOutOfRangeException(nameof(k), "Must request at least 1 eigenvalue.");

        int n = op.InputDimension;
        k = System.Math.Min(k, n);

        // Initialize k orthonormal random trial vectors
        var rng = new Random(42);
        var X = InitRandomOrthonormal(n, k, rng, op.InputSignature);
        var HX = ApplyToBlock(op, X, n);

        // Compute initial Rayleigh quotients
        var lambda = new double[k];
        for (int i = 0; i < k; i++)
            lambda[i] = Dot(X, i, HX, i, n);

        // Previous search direction (null on first iteration)
        double[]? P = null;   // k*n flattened
        double[]? HP = null;
        int iter = 0;
        var residualNorms = new double[k];

        for (iter = 0; iter < maxIterations; iter++)
        {
            // Compute residuals R_i = H*X_i - lambda_i * X_i
            var R = new double[k * n];
            bool allConverged = true;
            for (int i = 0; i < k; i++)
            {
                double rNorm2 = 0;
                for (int j = 0; j < n; j++)
                {
                    double val = HX[i * n + j] - lambda[i] * X[i * n + j];
                    R[i * n + j] = val;
                    rNorm2 += val * val;
                }
                residualNorms[i] = System.Math.Sqrt(rNorm2);
                if (residualNorms[i] > tolerance) allConverged = false;
            }
            if (allConverged) break;

            // Build search space: orthonormalize [X, R, P] together
            // First, collect all active vectors
            var activeVecs = new List<double[]>();
            var activeHVecs = new List<double[]>();

            // X block (already orthonormal)
            for (int i = 0; i < k; i++)
            {
                var xi = new double[n];
                var hxi = new double[n];
                Array.Copy(X, i * n, xi, 0, n);
                Array.Copy(HX, i * n, hxi, 0, n);
                activeVecs.Add(xi);
                activeHVecs.Add(hxi);
            }

            // R block: orthogonalize against X, then normalize
            var HR = ApplyToBlock(op, R, n);
            for (int i = 0; i < k; i++)
            {
                var ri = new double[n];
                var hri = new double[n];
                Array.Copy(R, i * n, ri, 0, n);
                Array.Copy(HR, i * n, hri, 0, n);

                // Orthogonalize against all existing active vectors (twice for stability)
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int j = 0; j < activeVecs.Count; j++)
                    {
                        double proj = DotArr(ri, activeVecs[j], n);
                        for (int l = 0; l < n; l++)
                        {
                            ri[l] -= proj * activeVecs[j][l];
                            hri[l] -= proj * activeHVecs[j][l];
                        }
                    }
                }

                double norm = NormArr(ri, n);
                if (norm > 1e-14)
                {
                    for (int l = 0; l < n; l++) { ri[l] /= norm; hri[l] /= norm; }
                    activeVecs.Add(ri);
                    activeHVecs.Add(hri);
                }
            }

            // P block (if available)
            if (P != null && HP != null)
            {
                for (int i = 0; i < k; i++)
                {
                    var pi = new double[n];
                    var hpi = new double[n];
                    Array.Copy(P, i * n, pi, 0, n);
                    Array.Copy(HP, i * n, hpi, 0, n);

                    for (int pass = 0; pass < 2; pass++)
                    {
                        for (int j = 0; j < activeVecs.Count; j++)
                        {
                            double proj = DotArr(pi, activeVecs[j], n);
                            for (int l = 0; l < n; l++)
                            {
                                pi[l] -= proj * activeVecs[j][l];
                                hpi[l] -= proj * activeHVecs[j][l];
                            }
                        }
                    }

                    double norm = NormArr(pi, n);
                    if (norm > 1e-14)
                    {
                        for (int l = 0; l < n; l++) { pi[l] /= norm; hpi[l] /= norm; }
                        activeVecs.Add(pi);
                        activeHVecs.Add(hpi);
                    }
                }
            }

            int sSize = activeVecs.Count;
            if (sSize < k) break; // search space too small

            // Form projected H_small = S^T * H * S (symmetric)
            var Hsmall = new double[sSize * sSize];
            for (int i = 0; i < sSize; i++)
            {
                for (int j = i; j < sSize; j++)
                {
                    double val = DotArr(activeVecs[i], activeHVecs[j], n);
                    Hsmall[i * sSize + j] = val;
                    Hsmall[j * sSize + i] = val;
                }
            }

            // Solve small symmetric eigenproblem via Jacobi iteration
            var (eigvals, eigvecs) = JacobiEigen(Hsmall, sSize);

            // Sort ascending and pick k smallest
            var indices = new int[eigvals.Length];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;
            Array.Sort(eigvals, indices);
            int take = System.Math.Min(k, eigvals.Length);

            // Update X, HX
            var newX = new double[take * n];
            var newHX = new double[take * n];
            P = new double[take * n];
            HP = new double[take * n];

            for (int i = 0; i < take; i++)
            {
                int idx = indices[i];
                lambda[i] = eigvals[i]; // already sorted

                for (int j = 0; j < sSize; j++)
                {
                    double coeff = eigvecs[j * sSize + idx];
                    for (int l = 0; l < n; l++)
                    {
                        newX[i * n + l] += coeff * activeVecs[j][l];
                        newHX[i * n + l] += coeff * activeHVecs[j][l];
                    }
                }

                // P = contribution from non-X blocks only (j >= k)
                for (int j = k; j < sSize; j++)
                {
                    double coeff = eigvecs[j * sSize + idx];
                    for (int l = 0; l < n; l++)
                    {
                        P[i * n + l] += coeff * activeVecs[j][l];
                        HP[i * n + l] += coeff * activeHVecs[j][l];
                    }
                }
            }

            X = newX;
            HX = newHX;
            k = take;
        }

        // Final residual computation
        for (int i = 0; i < k; i++)
        {
            double rNorm2 = 0;
            for (int j = 0; j < n; j++)
            {
                double val = HX[i * n + j] - lambda[i] * X[i * n + j];
                rNorm2 += val * val;
            }
            residualNorms[i] = System.Math.Sqrt(rNorm2);
        }

        // Sort eigenvalues ascending
        var sortIdx = Enumerable.Range(0, k).OrderBy(i => lambda[i]).ToArray();
        var sortedValues = sortIdx.Select(i => lambda[i]).ToArray();
        var sortedResiduals = sortIdx.Select(i => residualNorms[i]).ToArray();
        var sortedVectors = sortIdx.Select(i =>
        {
            var v = new double[n];
            Array.Copy(X, i * n, v, 0, n);
            return MakeFieldTensor(v, op.InputSignature, "lobpcg_eigvec");
        }).ToArray();

        bool converged = sortedResiduals.All(r => r < tolerance);
        string status = converged ? "converged" :
            sortedValues.Length > 0 ? "partially-converged" : "failed";

        return new SpectrumProbeResult
        {
            Values = sortedValues,
            Vectors = sortedVectors,
            ResidualNorms = sortedResiduals,
            Iterations = iter,
            ConvergenceStatus = status,
        };
    }

    public SpectrumProbeResult ComputeSmallestSingularValues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        var normalOp = new NormalOperatorWrapper(op);
        var result = ComputeSmallestEigenvalues(normalOp, k, tolerance * tolerance, maxIterations);

        var singularValues = new double[result.Values.Length];
        for (int i = 0; i < singularValues.Length; i++)
            singularValues[i] = System.Math.Sqrt(System.Math.Max(0, result.Values[i]));

        var residualNorms = new double[result.ResidualNorms.Length];
        for (int i = 0; i < residualNorms.Length; i++)
            residualNorms[i] = System.Math.Sqrt(System.Math.Max(0, result.ResidualNorms[i]));

        return new SpectrumProbeResult
        {
            Values = singularValues,
            Vectors = null,
            ResidualNorms = residualNorms,
            Iterations = result.Iterations,
            ConvergenceStatus = result.ConvergenceStatus,
        };
    }

    private static double[] InitRandomOrthonormal(int n, int k, Random rng, TensorSignature sig)
    {
        var X = new double[k * n];
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < n; j++)
                X[i * n + j] = rng.NextDouble() * 2.0 - 1.0;

            // Orthogonalize against previous vectors
            for (int p = 0; p < i; p++)
            {
                double proj = 0;
                for (int j = 0; j < n; j++)
                    proj += X[i * n + j] * X[p * n + j];
                for (int j = 0; j < n; j++)
                    X[i * n + j] -= proj * X[p * n + j];
            }

            // Normalize
            double norm = 0;
            for (int j = 0; j < n; j++)
                norm += X[i * n + j] * X[i * n + j];
            norm = System.Math.Sqrt(norm);
            if (norm > 1e-14)
                for (int j = 0; j < n; j++)
                    X[i * n + j] /= norm;
        }
        return X;
    }

    private static double[] ApplyToBlock(ILinearOperator op, double[] block, int n)
    {
        int k = block.Length / n;
        var result = new double[k * n];
        var sig = op.InputSignature;
        for (int i = 0; i < k; i++)
        {
            var v = new double[n];
            Array.Copy(block, i * n, v, 0, n);
            var input = MakeFieldTensor(v, sig, "lobpcg_input");
            var output = op.Apply(input);
            Array.Copy(output.Coefficients, 0, result, i * n, n);
        }
        return result;
    }

    private static double Dot(double[] block1, int i1, double[] block2, int i2, int n)
    {
        double sum = 0;
        int off1 = i1 * n, off2 = i2 * n;
        for (int j = 0; j < n; j++)
            sum += block1[off1 + j] * block2[off2 + j];
        return sum;
    }

    private static double DotArr(double[] a, double[] b, int n)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
            sum += a[i] * b[i];
        return sum;
    }

    private static double NormArr(double[] a, int n)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
            sum += a[i] * a[i];
        return System.Math.Sqrt(sum);
    }

    /// <summary>
    /// Jacobi eigenvalue iteration for a small dense symmetric matrix.
    /// Matrix stored as row-major flat array of size n*n.
    /// Returns sorted eigenvalues and eigenvector matrix (column-major in flat row-major layout).
    /// </summary>
    private static (double[] eigenvalues, double[] eigenvectors) JacobiEigen(double[] A, int n)
    {
        var D = (double[])A.Clone();
        var V = new double[n * n];
        for (int i = 0; i < n; i++)
            V[i * n + i] = 1.0;

        for (int sweep = 0; sweep < 200; sweep++)
        {
            double offDiag = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    offDiag += D[i * n + j] * D[i * n + j];
            if (offDiag < 1e-28) break;

            for (int p = 0; p < n; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    double apq = D[p * n + q];
                    if (System.Math.Abs(apq) < 1e-15) continue;

                    double diff = D[q * n + q] - D[p * n + p];
                    double t;
                    if (System.Math.Abs(diff) < 1e-15 * System.Math.Abs(apq))
                    {
                        t = 1.0;
                    }
                    else
                    {
                        double theta = 0.5 * diff / apq;
                        t = System.Math.Sign(theta) /
                            (System.Math.Abs(theta) + System.Math.Sqrt(theta * theta + 1.0));
                    }

                    double c = 1.0 / System.Math.Sqrt(t * t + 1.0);
                    double s = t * c;
                    double tau = s / (1.0 + c);

                    D[p * n + q] = 0;
                    D[q * n + p] = 0;
                    D[p * n + p] -= t * apq;
                    D[q * n + q] += t * apq;

                    for (int r = 0; r < n; r++)
                    {
                        if (r == p || r == q) continue;
                        double drp = D[r * n + p];
                        double drq = D[r * n + q];
                        D[r * n + p] = drp - s * (drq + tau * drp);
                        D[p * n + r] = D[r * n + p];
                        D[r * n + q] = drq + s * (drp - tau * drq);
                        D[q * n + r] = D[r * n + q];
                    }

                    for (int r = 0; r < n; r++)
                    {
                        double vrp = V[r * n + p];
                        double vrq = V[r * n + q];
                        V[r * n + p] = vrp - s * (vrq + tau * vrp);
                        V[r * n + q] = vrq + s * (vrp - tau * vrq);
                    }
                }
            }
        }

        var eigenvalues = new double[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = D[i * n + i];

        return (eigenvalues, V);
    }

    private static FieldTensor MakeFieldTensor(double[] coeffs, TensorSignature sig, string label)
    {
        return new FieldTensor
        {
            Label = label,
            Signature = sig,
            Coefficients = (double[])coeffs.Clone(),
            Shape = new[] { coeffs.Length },
        };
    }

    private sealed class NormalOperatorWrapper : ILinearOperator
    {
        private readonly ILinearOperator _inner;

        public NormalOperatorWrapper(ILinearOperator inner) => _inner = inner;

        public TensorSignature InputSignature => _inner.InputSignature;
        public TensorSignature OutputSignature => _inner.InputSignature;
        public int InputDimension => _inner.InputDimension;
        public int OutputDimension => _inner.InputDimension;

        public FieldTensor Apply(FieldTensor v)
        {
            var av = _inner.Apply(v);
            return _inner.ApplyTranspose(av);
        }

        public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
    }
}

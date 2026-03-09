using Gu.Branching;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// Lanczos iteration for computing extremal eigenvalues of a symmetric operator.
/// Uses the standard three-term recurrence to build a tridiagonal projection,
/// then computes eigenvalues of the small tridiagonal matrix.
///
/// For singular values of a non-square operator A, this computes eigenvalues
/// of A^T A using the Lanczos iteration on the normal operator.
/// </summary>
public sealed class LanczosSpectrumProbe : ISpectrumProbe
{
    public string MethodId => "lanczos";

    public SpectrumProbeResult ComputeSmallestEigenvalues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        if (op.InputDimension != op.OutputDimension)
            throw new ArgumentException("Eigenvalue computation requires a square operator.");
        if (k < 1)
            throw new ArgumentOutOfRangeException(nameof(k), "Must request at least 1 eigenvalue.");

        int n = op.InputDimension;
        int m = System.Math.Min(maxIterations, n); // Lanczos dimension

        // Lanczos vectors (stored as rows)
        var alpha = new double[m]; // diagonal
        var beta = new double[m];  // sub-diagonal

        // Initial random vector
        var rng = new Random(42);
        var v = MakeRandomVector(n, rng, op.InputSignature);
        double norm = L2Norm(v.Coefficients);
        v = ScaleVector(v, 1.0 / norm);

        FieldTensor? vPrev = null;
        var lanczosVectors = new List<FieldTensor> { v };

        for (int j = 0; j < m; j++)
        {
            var w = op.Apply(v);

            alpha[j] = Dot(w.Coefficients, v.Coefficients);

            // w = w - alpha[j]*v - beta[j-1]*v_prev
            var wCoeffs = (double[])w.Coefficients.Clone();
            for (int i = 0; i < n; i++)
                wCoeffs[i] -= alpha[j] * v.Coefficients[i];

            if (j > 0 && vPrev != null)
            {
                for (int i = 0; i < n; i++)
                    wCoeffs[i] -= beta[j - 1] * vPrev.Coefficients[i];
            }

            double betaJ = L2Norm(wCoeffs);

            if (betaJ < 1e-14)
            {
                // Invariant subspace found; stop early
                m = j + 1;
                break;
            }

            if (j + 1 < m)
            {
                beta[j] = betaJ;
                vPrev = v;
                v = MakeFieldTensor(wCoeffs, 1.0 / betaJ, op.InputSignature);
                lanczosVectors.Add(v);
            }
        }

        // Compute eigenvalues of the m x m tridiagonal matrix
        var triAlpha = alpha[..m];
        var triBeta = beta[..(m > 0 ? m - 1 : 0)];
        var eigenvalues = SolveTridiagonalEigenvalues(triAlpha, triBeta);

        // Sort ascending and take k smallest
        Array.Sort(eigenvalues);
        int obtained = System.Math.Min(k, eigenvalues.Length);
        var smallest = eigenvalues[..obtained];

        // Compute residual norms for each Ritz value
        var residualNorms = new double[obtained];
        for (int i = 0; i < obtained; i++)
        {
            // Approximate residual: |beta_m * e_m^T y_i| where y_i is the
            // Ritz vector in the tridiagonal basis. For simplicity, use a
            // bound based on the last beta value.
            residualNorms[i] = m > 1 ? System.Math.Abs(beta[m - 2]) : 0;
        }

        bool allConverged = residualNorms.All(r => r < tolerance);
        string status = allConverged ? "converged" :
            obtained > 0 ? "partially-converged" : "failed";

        return new SpectrumProbeResult
        {
            Values = smallest,
            Vectors = null, // Ritz vector extraction deferred
            ResidualNorms = residualNorms,
            Iterations = m,
            ConvergenceStatus = status,
        };
    }

    public SpectrumProbeResult ComputeSmallestSingularValues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        // Compute singular values of A via eigenvalues of A^T A
        var normalOp = new NormalOperatorWrapper(op);
        var result = ComputeSmallestEigenvalues(normalOp, k, tolerance * tolerance, maxIterations);

        // Singular values = sqrt(eigenvalues of A^T A)
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

    /// <summary>
    /// Solve eigenvalues of a symmetric tridiagonal matrix using the QR algorithm.
    /// Simple implementation suitable for small Lanczos matrices.
    /// </summary>
    private static double[] SolveTridiagonalEigenvalues(double[] alpha, double[] beta)
    {
        int n = alpha.Length;
        if (n == 0) return Array.Empty<double>();
        if (n == 1) return new[] { alpha[0] };

        // Copy to working arrays (QL/QR iteration modifies in place)
        var d = (double[])alpha.Clone();
        var e = new double[n];
        for (int i = 0; i < beta.Length; i++)
            e[i] = beta[i];

        // Implicit QL iteration with shifts
        for (int l = 0; l < n; l++)
        {
            int iter = 0;
            while (true)
            {
                // Find small sub-diagonal element
                int m = l;
                while (m < n - 1)
                {
                    double dd = System.Math.Abs(d[m]) + System.Math.Abs(d[m + 1]);
                    if (System.Math.Abs(e[m]) + dd == dd)
                        break;
                    m++;
                }

                if (m == l) break;

                if (iter++ >= 30 * n)
                    break; // Prevent infinite loop

                // Wilkinson shift
                double g = (d[l + 1] - d[l]) / (2.0 * e[l]);
                double r = System.Math.Sqrt(g * g + 1.0);
                g = d[m] - d[l] + e[l] / (g + System.Math.CopySign(r, g));

                double s = 1.0, c = 1.0, p = 0.0;
                for (int i = m - 1; i >= l; i--)
                {
                    double f = s * e[i];
                    double b = c * e[i];
                    r = System.Math.Sqrt(f * f + g * g);
                    e[i + 1] = r;

                    if (r == 0)
                    {
                        d[i + 1] -= p;
                        e[m] = 0;
                        break;
                    }

                    s = f / r;
                    c = g / r;
                    g = d[i + 1] - p;
                    r = (d[i] - g) * s + 2.0 * c * b;
                    p = s * r;
                    d[i + 1] = g + p;
                    g = c * r - b;
                }

                d[l] -= p;
                e[l] = g;
                e[m] = 0;
            }
        }

        return d;
    }

    private static FieldTensor MakeRandomVector(int n, Random rng, TensorSignature sig)
    {
        var coeffs = new double[n];
        for (int i = 0; i < n; i++)
            coeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        return new FieldTensor
        {
            Label = "lanczos_v0",
            Signature = sig,
            Coefficients = coeffs,
            Shape = new[] { n },
        };
    }

    private static FieldTensor ScaleVector(FieldTensor v, double s)
    {
        var result = new double[v.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = v.Coefficients[i] * s;
        return new FieldTensor
        {
            Label = v.Label,
            Signature = v.Signature,
            Coefficients = result,
            Shape = v.Shape,
        };
    }

    private static FieldTensor MakeFieldTensor(double[] coeffs, double scale, TensorSignature sig)
    {
        var result = new double[coeffs.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = coeffs[i] * scale;
        return new FieldTensor
        {
            Label = "lanczos_v",
            Signature = sig,
            Coefficients = result,
            Shape = new[] { coeffs.Length },
        };
    }

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    /// <summary>
    /// Wrapper that turns any ILinearOperator A into A^T A (normal operator).
    /// Used to compute singular values via eigenvalues.
    /// </summary>
    private sealed class NormalOperatorWrapper : ILinearOperator
    {
        private readonly ILinearOperator _inner;

        public NormalOperatorWrapper(ILinearOperator inner)
        {
            _inner = inner;
        }

        public TensorSignature InputSignature => _inner.InputSignature;
        public TensorSignature OutputSignature => _inner.InputSignature;
        public int InputDimension => _inner.InputDimension;
        public int OutputDimension => _inner.InputDimension;

        public FieldTensor Apply(FieldTensor v)
        {
            var av = _inner.Apply(v);
            return _inner.ApplyTranspose(av);
        }

        public FieldTensor ApplyTranspose(FieldTensor v)
        {
            return Apply(v); // A^T A is self-adjoint
        }
    }
}

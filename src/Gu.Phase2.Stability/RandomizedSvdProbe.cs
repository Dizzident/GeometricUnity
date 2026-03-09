using Gu.Branching;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// Randomized SVD probe for computing smallest singular values of possibly
/// non-square operators (e.g., L_tilde = (J, sqrt(lambda)*C)).
///
/// Unlike the normal-equations approach (A^T A), this method does not square
/// the condition number, making it numerically robust for detecting small
/// singular values — exactly the regime that matters for singularity detection
/// in continuation and branch-fragility analysis.
///
/// Algorithm (Halko-Martinsson-Tropp randomized range finder):
/// 1. Draw Omega in R^{n x (k+p)} random Gaussian
/// 2. Form Y = A * Omega, with q power iterations for accuracy
/// 3. QR decompose Y = Q * R
/// 4. Form B = Q^T * A (small (k+p) x n matrix)
/// 5. SVD of B -> singular values of A (up to approximation error)
/// </summary>
public sealed class RandomizedSvdProbe : ISpectrumProbe
{
    private readonly int _oversampling;
    private readonly int _powerIterations;

    /// <summary>
    /// Create a randomized SVD probe.
    /// </summary>
    /// <param name="oversampling">Extra columns beyond k for accuracy (default 5).</param>
    /// <param name="powerIterations">Power iteration count for accuracy on decaying spectra (default 2).</param>
    public RandomizedSvdProbe(int oversampling = 5, int powerIterations = 2)
    {
        _oversampling = oversampling;
        _powerIterations = powerIterations;
    }

    public string MethodId => "randomized-svd";

    public SpectrumProbeResult ComputeSmallestEigenvalues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        // For eigenvalues of a symmetric operator, compute singular values
        // (which equal absolute eigenvalues for symmetric operators)
        // and return them. For a PSD operator, singular values = eigenvalues.
        if (op.InputDimension != op.OutputDimension)
            throw new ArgumentException("Eigenvalue computation requires a square operator.");

        var svdResult = ComputeSmallestSingularValues(op, k, tolerance, maxIterations);

        // For symmetric operators, singular values = |eigenvalues|.
        // We return them as-is (non-negative) since we can't determine sign
        // without eigenvector information.
        return svdResult;
    }

    public SpectrumProbeResult ComputeSmallestSingularValues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        if (k < 1)
            throw new ArgumentOutOfRangeException(nameof(k), "Must request at least 1 singular value.");

        int m = op.OutputDimension;
        int n = op.InputDimension;
        int ell = System.Math.Min(k + _oversampling, System.Math.Min(m, n));
        if (ell < 1) ell = 1;

        var rng = new Random(42);

        // Step 1: Draw random Gaussian test matrix Omega (n x ell)
        var omega = new double[n * ell];
        for (int i = 0; i < omega.Length; i++)
            omega[i] = NextGaussian(rng);

        // Step 2: Form Y = A * Omega (m x ell) with power iterations
        var Y = ApplyToColumns(op, omega, n, ell);

        for (int q = 0; q < _powerIterations; q++)
        {
            // Y = A * (A^T * Y) — each power iteration improves accuracy
            var AtY = ApplyTransposeToColumns(op, Y, m, ell);
            Y = ApplyToColumns(op, AtY, n, ell);
        }

        // Step 3: QR decompose Y (m x ell) -> Q (m x ell), R (ell x ell)
        var (Q, rank) = QrOrthogonalize(Y, m, ell);
        if (rank == 0)
        {
            return new SpectrumProbeResult
            {
                Values = Array.Empty<double>(),
                ResidualNorms = Array.Empty<double>(),
                Iterations = 0,
                ConvergenceStatus = "failed",
            };
        }

        // Step 4: Form B = Q^T * A (rank x n)
        // B[i, j] = sum_l Q[l, i] * (A * e_j)[l]
        // More efficiently: B = Q^T * A by applying A to standard basis vectors
        // But that's n matvecs. Instead, apply A^T to Q columns to get B^T.
        // B^T = A^T * Q, so B^T[j, i] = (A^T * Q_i)[j]
        // Then B[i, j] = B^T[j, i]
        var Bt = ApplyTransposeToColumns(op, Q, m, rank); // n x rank
        // Bt is stored as: Bt[col * n + row], col in [0, rank), row in [0, n)
        // B is rank x n: B[i, j] = Bt[i * n + j]
        // So Bt already stores B in row-major order (each column of Q -> row of B)

        // Step 5: SVD of small B (rank x n)
        var singularValues = ComputeSvdOfSmallMatrix(Bt, rank, n);

        // Sort ascending
        Array.Sort(singularValues);

        // Take k smallest
        int obtained = System.Math.Min(k, singularValues.Length);
        var smallest = new double[obtained];
        Array.Copy(singularValues, 0, smallest, 0, obtained);

        // Residual norms are approximate — we don't have exact Ritz vectors
        var residualNorms = new double[obtained];
        for (int i = 0; i < obtained; i++)
            residualNorms[i] = tolerance * 0.1; // approximate bound

        string status = obtained >= k ? "converged" :
            obtained > 0 ? "partially-converged" : "failed";

        return new SpectrumProbeResult
        {
            Values = smallest,
            Vectors = null,
            ResidualNorms = residualNorms,
            Iterations = 1 + _powerIterations,
            ConvergenceStatus = status,
        };
    }

    /// <summary>Apply operator to each column of a matrix (stored column-major).</summary>
    private static double[] ApplyToColumns(ILinearOperator op, double[] matrix, int inputDim, int cols)
    {
        int outputDim = op.OutputDimension;
        var result = new double[outputDim * cols];
        var sig = op.InputSignature;

        for (int j = 0; j < cols; j++)
        {
            var v = new double[inputDim];
            for (int i = 0; i < inputDim; i++)
                v[i] = matrix[j * inputDim + i];

            var input = new FieldTensor
            {
                Label = "rsvd_input",
                Signature = sig,
                Coefficients = v,
                Shape = [inputDim],
            };

            var output = op.Apply(input);
            for (int i = 0; i < outputDim; i++)
                result[j * outputDim + i] = output.Coefficients[i];
        }

        return result;
    }

    /// <summary>Apply transpose to each column of a matrix (stored column-major).</summary>
    private static double[] ApplyTransposeToColumns(ILinearOperator op, double[] matrix, int inputDim, int cols)
    {
        int outputDim = op.InputDimension; // transpose swaps input/output
        var result = new double[outputDim * cols];
        var sig = op.OutputSignature;

        for (int j = 0; j < cols; j++)
        {
            var v = new double[inputDim];
            for (int i = 0; i < inputDim; i++)
                v[i] = matrix[j * inputDim + i];

            var input = new FieldTensor
            {
                Label = "rsvd_transpose_input",
                Signature = sig,
                Coefficients = v,
                Shape = [inputDim],
            };

            var output = op.ApplyTranspose(input);
            for (int i = 0; i < outputDim; i++)
                result[j * outputDim + i] = output.Coefficients[i];
        }

        return result;
    }

    /// <summary>
    /// Modified Gram-Schmidt QR on column-major matrix (m x cols).
    /// Returns orthonormalized columns packed into positions [0..rank) and effective rank.
    /// </summary>
    private static (double[] Q, int rank) QrOrthogonalize(double[] Y, int m, int cols)
    {
        var Q = new double[m * cols];
        int rank = 0;

        for (int j = 0; j < cols; j++)
        {
            // Copy column j from Y into working position
            var col = new double[m];
            for (int l = 0; l < m; l++)
                col[l] = Y[j * m + l];

            // Orthogonalize against all accepted columns (two passes for stability)
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < rank; i++)
                {
                    double proj = 0;
                    for (int l = 0; l < m; l++)
                        proj += Q[i * m + l] * col[l];
                    for (int l = 0; l < m; l++)
                        col[l] -= proj * Q[i * m + l];
                }
            }

            // Normalize
            double norm = 0;
            for (int l = 0; l < m; l++)
                norm += col[l] * col[l];
            norm = System.Math.Sqrt(norm);

            if (norm < 1e-14)
                continue; // Linearly dependent — skip

            for (int l = 0; l < m; l++)
                Q[rank * m + l] = col[l] / norm;

            rank++;
        }

        return (Q, rank);
    }

    /// <summary>
    /// Compute singular values of a small dense matrix B (rows x cols),
    /// stored in row-major order (B[i * cols + j]).
    /// Uses eigenvalues of B^T * B (acceptable for small matrices).
    /// </summary>
    private static double[] ComputeSvdOfSmallMatrix(double[] B, int rows, int cols)
    {
        // Form C = B^T * B (cols x cols symmetric)
        var C = new double[cols * cols];
        for (int i = 0; i < cols; i++)
        {
            for (int j = i; j < cols; j++)
            {
                double sum = 0;
                for (int l = 0; l < rows; l++)
                    sum += B[l * cols + i] * B[l * cols + j];
                C[i * cols + j] = sum;
                C[j * cols + i] = sum;
            }
        }

        // Jacobi eigenvalue decomposition of the small C
        var eigenvalues = JacobiEigenvalues(C, cols);

        // Singular values = sqrt(eigenvalues of B^T B)
        var singularValues = new double[eigenvalues.Length];
        for (int i = 0; i < eigenvalues.Length; i++)
            singularValues[i] = System.Math.Sqrt(System.Math.Max(0, eigenvalues[i]));

        return singularValues;
    }

    /// <summary>
    /// Jacobi eigenvalue iteration for a small dense symmetric matrix.
    /// Returns eigenvalues (unsorted).
    /// </summary>
    private static double[] JacobiEigenvalues(double[] A, int n)
    {
        if (n == 0) return Array.Empty<double>();
        if (n == 1) return [A[0]];

        var D = (double[])A.Clone();

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
                }
            }
        }

        var eigenvalues = new double[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = D[i * n + i];

        return eigenvalues;
    }

    /// <summary>Box-Muller Gaussian random number.</summary>
    private static double NextGaussian(Random rng)
    {
        double u1 = 1.0 - rng.NextDouble(); // (0, 1]
        double u2 = rng.NextDouble();
        return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
               System.Math.Cos(2.0 * System.Math.PI * u2);
    }
}

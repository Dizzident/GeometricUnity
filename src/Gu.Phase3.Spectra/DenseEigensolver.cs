using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Dense eigensolver for small generalized eigenproblems: H v = lambda M v.
///
/// Uses Cholesky factorization of M to reduce to standard form:
///   L^{-1} H L^{-T} w = lambda w, then v = L^{-T} w.
///
/// Then solves the standard eigenproblem via Jacobi iteration.
///
/// Only suitable for toy/debug systems (dim &lt;= ~200).
/// </summary>
internal static class DenseEigensolver
{
    /// <summary>
    /// Solve the generalized eigenproblem H v = lambda M v.
    /// Returns (eigenvalues, eigenvectors) sorted ascending by eigenvalue.
    /// Eigenvectors are M-normalized: v_i^T M v_j = delta_ij.
    /// </summary>
    public static (double[] Eigenvalues, double[][] Eigenvectors) Solve(
        LinearizedOperatorBundle bundle,
        int numEigenvalues)
    {
        int n = bundle.StateDimension;
        if (n > 500)
            throw new InvalidOperationException(
                $"Dense eigensolver not suitable for dimension {n}. Use Lanczos instead.");

        // Step 1: Assemble dense H and M matrices
        var H = AssembleDenseMatrix(bundle.ApplySpectral, n, bundle.SpectralOperator.InputSignature);
        var M = AssembleDenseMatrix(bundle.ApplyMass, n, bundle.MassOperator.InputSignature);

        // Step 2: Cholesky factorization of M: M = L L^T
        var L = CholeskyDecompose(M, n);

        // Step 3: Form A = L^{-1} H L^{-T}
        var A = FormReducedMatrix(H, L, n);

        // Step 4: Jacobi eigendecomposition of A
        var eigVecs = new double[n * n];
        for (int i = 0; i < n; i++) eigVecs[i * n + i] = 1.0;
        var eigVals = JacobiEigen(A, eigVecs, n);

        // Step 5: Sort by eigenvalue ascending
        var indices = Enumerable.Range(0, n)
            .OrderBy(i => eigVals[i])
            .ToArray();

        int count = System.Math.Min(numEigenvalues, n);
        var resultEigenvalues = new double[count];
        var resultEigenvectors = new double[count][];

        for (int k = 0; k < count; k++)
        {
            int idx = indices[k];
            resultEigenvalues[k] = eigVals[idx];

            // w_k = eigenvector in reduced space
            var w = new double[n];
            for (int i = 0; i < n; i++)
                w[i] = eigVecs[idx * n + i];

            // v_k = L^{-T} w_k (back-transform to original space)
            var v = SolveUpperTriangular(L, w, n);
            resultEigenvectors[k] = v;
        }

        // Step 6: M-normalize eigenvectors
        MNormalize(resultEigenvectors, bundle, n);

        return (resultEigenvalues, resultEigenvectors);
    }

    /// <summary>
    /// Assemble a dense matrix by probing with unit vectors.
    /// </summary>
    private static double[] AssembleDenseMatrix(
        Func<FieldTensor, FieldTensor> apply, int n, TensorSignature sig)
    {
        var matrix = new double[n * n];
        var ej = new double[n];

        for (int j = 0; j < n; j++)
        {
            ej[j] = 1.0;
            var ft = new FieldTensor
            {
                Label = $"e_{j}",
                Signature = sig,
                Coefficients = (double[])ej.Clone(),
                Shape = new[] { n },
            };
            var col = apply(ft);
            for (int i = 0; i < n; i++)
                matrix[j * n + i] = col.Coefficients[i];
            ej[j] = 0.0;
        }

        return matrix;
    }

    /// <summary>
    /// Cholesky decomposition: M = L L^T. Returns L (lower triangular, column-major).
    /// Falls back to regularized decomposition if M is not positive definite.
    /// </summary>
    private static double[] CholeskyDecompose(double[] M, int n)
    {
        var L = new double[n * n];
        double regularization = 0.0;
        const double regStep = 1e-12;
        const int maxRetries = 5;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            Array.Clear(L);
            bool success = true;

            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int k = 0; k < j; k++)
                    sum += L[j * n + k] * L[j * n + k];

                double diag = M[j * n + j] + regularization - sum;
                if (diag <= 0)
                {
                    success = false;
                    break;
                }
                L[j * n + j] = System.Math.Sqrt(diag);

                for (int i = j + 1; i < n; i++)
                {
                    sum = 0;
                    for (int k = 0; k < j; k++)
                        sum += L[i * n + k] * L[j * n + k];
                    L[i * n + j] = (M[j * n + i] - sum) / L[j * n + j];
                }
            }

            if (success) return L;
            regularization += regStep;
        }

        throw new InvalidOperationException(
            "Cholesky decomposition failed: M is not positive definite even with regularization.");
    }

    /// <summary>
    /// Form A = L^{-1} H L^{-T}.
    /// </summary>
    private static double[] FormReducedMatrix(double[] H, double[] L, int n)
    {
        // First compute B = L^{-1} H
        var B = new double[n * n];
        for (int j = 0; j < n; j++)
        {
            // Solve L * b_j = h_j (column j of H)
            var hCol = new double[n];
            for (int i = 0; i < n; i++)
                hCol[i] = H[j * n + i];
            var bCol = SolveLowerTriangular(L, hCol, n);
            for (int i = 0; i < n; i++)
                B[j * n + i] = bCol[i];
        }

        // Then compute A = B L^{-T} = (L^{-1} H) L^{-T}
        var A = new double[n * n];
        for (int j = 0; j < n; j++)
        {
            // Solve L^T * a_j = b_j (column j of B transposed problem)
            // Equivalently: row j of A = (L^{-T} row_j(B)^T)^T
            // Simpler: A = B * (L^{-1})^T => A[:,j] = B * L^{-T}[:,j]
            // Solve L^T x = e_j, then A[:,j] = B * x
            var ej = new double[n];
            ej[j] = 1.0;
            var x = SolveUpperTriangular(L, ej, n);
            for (int i = 0; i < n; i++)
            {
                double val = 0;
                for (int k = 0; k < n; k++)
                    val += B[k * n + i] * x[k];
                A[j * n + i] = val;
            }
        }

        // Symmetrize to avoid numerical drift
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                double avg = 0.5 * (A[j * n + i] + A[i * n + j]);
                A[j * n + i] = avg;
                A[i * n + j] = avg;
            }

        return A;
    }

    /// <summary>
    /// Solve L x = b where L is lower triangular (stored as L[row * n + col]).
    /// </summary>
    private static double[] SolveLowerTriangular(double[] L, double[] b, int n)
    {
        var x = new double[n];
        for (int i = 0; i < n; i++)
        {
            double sum = b[i];
            for (int j = 0; j < i; j++)
                sum -= L[i * n + j] * x[j];
            x[i] = sum / L[i * n + i];
        }
        return x;
    }

    /// <summary>
    /// Solve L^T x = b where L is lower triangular (so L^T is upper triangular).
    /// </summary>
    private static double[] SolveUpperTriangular(double[] L, double[] b, int n)
    {
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = b[i];
            for (int j = i + 1; j < n; j++)
                sum -= L[j * n + i] * x[j]; // L^T[i,j] = L[j,i]
            x[i] = sum / L[i * n + i];
        }
        return x;
    }

    /// <summary>
    /// M-normalize eigenvectors: v_i^T M v_j = delta_ij.
    /// </summary>
    private static void MNormalize(double[][] vectors, LinearizedOperatorBundle bundle, int n)
    {
        for (int i = 0; i < vectors.Length; i++)
        {
            var ft = new FieldTensor
            {
                Label = $"v_{i}",
                Signature = bundle.MassOperator.InputSignature,
                Coefficients = vectors[i],
                Shape = new[] { n },
            };
            var mv = bundle.ApplyMass(ft);
            double norm = 0;
            for (int k = 0; k < n; k++)
                norm += vectors[i][k] * mv.Coefficients[k];
            norm = System.Math.Sqrt(System.Math.Max(0, norm));
            if (norm > 1e-15)
            {
                for (int k = 0; k < n; k++)
                    vectors[i][k] /= norm;
            }
        }
    }

    /// <summary>
    /// Jacobi eigendecomposition of a symmetric matrix.
    /// Modifies matrix in-place; returns eigenvalues.
    /// </summary>
    private static double[] JacobiEigen(double[] matrix, double[] vectors, int n)
    {
        const int maxIter = 200;
        const double tol = 1e-14;

        for (int iter = 0; iter < maxIter; iter++)
        {
            double offDiagNorm = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    offDiagNorm += matrix[i * n + j] * matrix[i * n + j];

            if (offDiagNorm < tol) break;

            for (int p = 0; p < n; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    double apq = matrix[p * n + q];
                    if (System.Math.Abs(apq) < tol * 0.01) continue;

                    double app = matrix[p * n + p];
                    double aqq = matrix[q * n + q];
                    double tau = (aqq - app) / (2.0 * apq);
                    double t = (tau >= 0 ? 1.0 : -1.0) /
                               (System.Math.Abs(tau) + System.Math.Sqrt(1.0 + tau * tau));
                    double c = 1.0 / System.Math.Sqrt(1.0 + t * t);
                    double s = t * c;

                    matrix[p * n + p] = app - t * apq;
                    matrix[q * n + q] = aqq + t * apq;
                    matrix[p * n + q] = 0;
                    matrix[q * n + p] = 0;

                    for (int r = 0; r < n; r++)
                    {
                        if (r == p || r == q) continue;
                        double mrp = matrix[r * n + p];
                        double mrq = matrix[r * n + q];
                        matrix[r * n + p] = c * mrp - s * mrq;
                        matrix[p * n + r] = matrix[r * n + p];
                        matrix[r * n + q] = s * mrp + c * mrq;
                        matrix[q * n + r] = matrix[r * n + q];
                    }

                    for (int r = 0; r < n; r++)
                    {
                        double vp = vectors[p * n + r];
                        double vq = vectors[q * n + r];
                        vectors[p * n + r] = c * vp - s * vq;
                        vectors[q * n + r] = s * vp + c * vq;
                    }
                }
            }
        }

        var eigenValues = new double[n];
        for (int i = 0; i < n; i++)
            eigenValues[i] = matrix[i * n + i];
        return eigenValues;
    }
}

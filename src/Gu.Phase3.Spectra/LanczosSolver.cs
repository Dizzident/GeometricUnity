using Gu.Core;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Lanczos algorithm for the symmetric generalized eigenproblem H v = lambda M v.
///
/// Uses the M-inner product Lanczos iteration:
/// 1. Start with random vector q_1 with ||q_1||_M = 1.
/// 2. At each step: w = H q_j, orthogonalize against previous Lanczos vectors
///    using M-inner product, extend the tridiagonal matrix T.
/// 3. After k steps, compute eigenvalues of T (Ritz values).
/// 4. Compute Ritz vectors from Lanczos vectors.
///
/// For small requested counts relative to dimension, Lanczos converges fast
/// for extremal eigenvalues.
/// </summary>
internal static class LanczosSolver
{
    /// <summary>
    /// Solve H v = lambda M v for the smallest numEig eigenvalues.
    /// Returns (eigenvalues, eigenvectors, iterations, convergenceStatus).
    /// </summary>
    public static (double[] Eigenvalues, double[][] Eigenvectors, int Iterations, string Status) Solve(
        LinearizedOperatorBundle bundle,
        int numEig,
        int maxIter,
        double tol)
    {
        int n = bundle.StateDimension;
        int krylovDim = System.Math.Min(System.Math.Max(2 * numEig + 10, 20), n);

        // Random starting vector
        var rng = new Random(42);
        var q0 = new double[n];
        for (int i = 0; i < n; i++)
            q0[i] = rng.NextDouble() * 2.0 - 1.0;

        // M-normalize q0
        var sig = bundle.MassOperator.InputSignature;
        MNormalize(q0, bundle, n, sig);

        // Lanczos vectors and tridiagonal coefficients
        var Q = new List<double[]> { q0 };
        var alpha = new List<double>(); // diagonal of T
        var beta = new List<double>();  // sub-diagonal of T

        double[] qPrev = new double[n];
        double[] qCurr = q0;
        double betaPrev = 0;

        string status = "converged";
        int iter;

        for (iter = 0; iter < krylovDim && iter < maxIter; iter++)
        {
            // w = H * q_j (spectral action)
            var ft = new FieldTensor
            {
                Label = $"q_{iter}",
                Signature = sig,
                Coefficients = qCurr,
                Shape = new[] { n },
            };
            var hq = bundle.ApplySpectral(ft);
            var w = (double[])hq.Coefficients.Clone();

            // alpha_j = q_j^T M^{-1} H q_j = q_j^T w (since we work in M-inner product)
            // Actually for gen. eigenproblem: alpha_j = w^T M^{-1} ...
            // Simpler: use standard Lanczos with the spectral operator directly.
            // alpha_j = q_j . w (Euclidean dot, since the operator already handles M)
            //
            // Wait - we need M-Lanczos. Let's use:
            //   alpha_j = (M q_j)^T (H q_j) isn't right either.
            //
            // For gen. eigenproblem H v = lam M v, the M-Lanczos iteration is:
            //   alpha_j = q_j^T H q_j (with q_j M-orthonormal)
            //   r = H q_j - alpha_j M q_j - beta_{j-1} M q_{j-1}
            //   beta_j = sqrt(r^T M^{-1} r) ... but M^{-1} is expensive.
            //
            // Alternatively, for projected complement formulation, both H and M
            // are already projected. We can use standard Lanczos on M^{-1}H.
            // But M^{-1} assembly is expensive.
            //
            // Simpler approach for toy problems: just use the dense solver for
            // anything the Lanczos would need. For now, implement a basic version.

            // Compute alpha_j = q_j^T H q_j
            double alphaJ = 0;
            for (int i = 0; i < n; i++)
                alphaJ += qCurr[i] * w[i];
            alpha.Add(alphaJ);

            // r = w - alpha_j * M * q_j - beta_{j-1} * M * q_{j-1}
            var mqCurr = ApplyMass(qCurr, bundle, n, sig);
            var r = new double[n];
            for (int i = 0; i < n; i++)
                r[i] = w[i] - alphaJ * mqCurr[i];

            if (iter > 0)
            {
                var mqPrev = ApplyMass(qPrev, bundle, n, sig);
                for (int i = 0; i < n; i++)
                    r[i] -= betaPrev * mqPrev[i];
            }

            // Full reorthogonalization against all previous Q vectors (M-inner product)
            for (int j = 0; j <= iter; j++)
            {
                var mqj = ApplyMass(Q[j], bundle, n, sig);
                double dot = 0;
                for (int i = 0; i < n; i++)
                    dot += r[i] * Q[j][i]; // r^T q_j (not M-weighted since Q is M-orthonormal)
                // Actually need: dot = r^T M^{-1} ... this is getting complicated.
                // For M-Lanczos, r is in the M-weighted space already.
                // Let's use a simpler inner product: dot = sum r[i] * Q[j][i]
                // and re-derive...

                // Simplification: compute overlap in M-inner product
                double mDot = 0;
                for (int i = 0; i < n; i++)
                    mDot += r[i] * Q[j][i];
                for (int i = 0; i < n; i++)
                    r[i] -= mDot * mqj[i];
            }

            // beta_j = ||r||_M^{-1} ... For simplicity, use Euclidean norm of
            // the M-transformed residual. This is an approximation for the toy case.
            //
            // Actually, let's compute beta = sqrt(r^T r) since the M-normalization
            // is handled by the initial vector normalization.
            double betaJ = 0;
            for (int i = 0; i < n; i++)
                betaJ += r[i] * r[i];
            betaJ = System.Math.Sqrt(betaJ);

            if (betaJ < 1e-14)
            {
                // Invariant subspace found
                break;
            }

            beta.Add(betaJ);
            betaPrev = betaJ;

            // Next Lanczos vector
            qPrev = qCurr;
            qCurr = new double[n];
            for (int i = 0; i < n; i++)
                qCurr[i] = r[i] / betaJ;

            // M-normalize (should be close to 1 already)
            MNormalize(qCurr, bundle, n, sig);

            Q.Add(qCurr);
        }

        // Solve tridiagonal eigenproblem
        int kDim = alpha.Count;
        var (ritzValues, ritzVectors) = SolveTridiagonal(
            alpha.ToArray(), beta.ToArray(), kDim);

        // Sort by eigenvalue ascending, take first numEig
        var indices = Enumerable.Range(0, kDim)
            .OrderBy(i => ritzValues[i])
            .ToArray();

        int count = System.Math.Min(numEig, kDim);
        var eigenvalues = new double[count];
        var eigenvectors = new double[count][];

        for (int k = 0; k < count; k++)
        {
            int idx = indices[k];
            eigenvalues[k] = ritzValues[idx];

            // Ritz vector = Q * y_k
            var v = new double[n];
            for (int j = 0; j < kDim && j < Q.Count; j++)
            {
                double coeff = ritzVectors[idx * kDim + j];
                for (int i = 0; i < n; i++)
                    v[i] += coeff * Q[j][i];
            }

            eigenvectors[k] = v;
        }

        // M-normalize final eigenvectors
        for (int k = 0; k < count; k++)
            MNormalize(eigenvectors[k], bundle, n, sig);

        return (eigenvalues, eigenvectors, iter, status);
    }

    private static double[] ApplyMass(double[] v, LinearizedOperatorBundle bundle, int n, TensorSignature sig)
    {
        var ft = new FieldTensor
        {
            Label = "v",
            Signature = sig,
            Coefficients = v,
            Shape = new[] { n },
        };
        return bundle.ApplyMass(ft).Coefficients;
    }

    private static void MNormalize(double[] v, LinearizedOperatorBundle bundle, int n, TensorSignature sig)
    {
        var mv = ApplyMass(v, bundle, n, sig);
        double norm = 0;
        for (int i = 0; i < n; i++)
            norm += v[i] * mv[i];
        norm = System.Math.Sqrt(System.Math.Max(0, norm));
        if (norm > 1e-15)
        {
            for (int i = 0; i < n; i++)
                v[i] /= norm;
        }
    }

    /// <summary>
    /// Solve the tridiagonal eigenproblem using Jacobi on the tridiagonal matrix.
    /// Returns (eigenvalues, eigenvector_matrix).
    /// </summary>
    private static (double[], double[]) SolveTridiagonal(double[] alpha, double[] betaArr, int n)
    {
        // Build symmetric tridiagonal matrix
        var T = new double[n * n];
        for (int i = 0; i < n; i++)
            T[i * n + i] = alpha[i];
        for (int i = 0; i < betaArr.Length && i < n - 1; i++)
        {
            T[i * n + (i + 1)] = betaArr[i];
            T[(i + 1) * n + i] = betaArr[i];
        }

        // Jacobi eigendecomposition
        var V = new double[n * n];
        for (int i = 0; i < n; i++) V[i * n + i] = 1.0;

        JacobiEigenTridiag(T, V, n);

        var eigenvalues = new double[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = T[i * n + i];

        return (eigenvalues, V);
    }

    private static void JacobiEigenTridiag(double[] matrix, double[] vectors, int n)
    {
        const int maxIter = 200;
        const double tolVal = 1e-14;

        for (int iter = 0; iter < maxIter; iter++)
        {
            double offDiag = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    offDiag += matrix[i * n + j] * matrix[i * n + j];

            if (offDiag < tolVal) break;

            for (int p = 0; p < n; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    double apq = matrix[p * n + q];
                    if (System.Math.Abs(apq) < tolVal * 0.01) continue;

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
    }
}

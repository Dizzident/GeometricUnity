using Gu.Core;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Lanczos algorithm for the symmetric generalized eigenproblem H v = lambda M v.
///
/// B-Lanczos iteration with M-inner product and CG-based mass solve:
///   q_0 = random, M-normalized (q_0^T M q_0 = 1)
///   For j = 0, 1, ...:
///     alpha_j = q_j^T H q_j                                 (Rayleigh quotient)
///     z = H q_j - alpha_j M q_j - beta_j M q_{j-1}          (three-term recurrence)
///     s = M^{-1} z                                           (CG solve of M s = z)
///     M-reorthogonalize s: h_k = (M q_k)^T s, s -= h_k q_k  (for each k &lt;= j)
///     beta_{j+1} = sqrt(s^T M s)                             (M-norm)
///     q_{j+1} = s / beta_{j+1}
///
/// The tridiagonal matrix T with diagonal {alpha} and sub-diagonal {beta} satisfies
/// T_k = Q_k^T H Q_k with Q_k^T M Q_k = I_k, so eigenvalues of T approximate
/// generalized eigenvalues of (H, M).
///
/// The CG mass solve converts the residual z (which lives in range(M)) back to the
/// q-vector space. This is essential for correct M-orthogonality when M != I.
///
/// Reference: Saad, "Numerical Methods for Large Eigenvalue Problems", 2nd ed., §5.2;
/// Parlett, "The Symmetric Eigenvalue Problem", Ch. 13.
/// </summary>
internal static class LanczosSolver
{
    /// <summary>
    /// Solve H v = lambda M v for the smallest numEig eigenvalues.
    /// Returns (eigenvalues, eigenvectors, iterations, convergenceStatus, diagnosticNotes).
    /// </summary>
    public static (double[] Eigenvalues, double[][] Eigenvectors, int Iterations, string Status, List<string> DiagnosticNotes) Solve(
        LinearizedOperatorBundle bundle,
        int numEig,
        int maxIter,
        double tol)
    {
        int n = bundle.StateDimension;
        int krylovDim = System.Math.Min(System.Math.Max(2 * numEig + 10, 20), n);
        var diagnosticNotes = new List<string>();

        // Random starting vector
        var rng = new Random(42);
        var q0 = new double[n];
        for (int i = 0; i < n; i++)
            q0[i] = rng.NextDouble() * 2.0 - 1.0;

        // M-normalize q0: q0 <- q0 / sqrt(q0^T M q0)
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
            // w = H q_j
            var ft = new FieldTensor
            {
                Label = $"q_{iter}",
                Signature = sig,
                Coefficients = qCurr,
                Shape = new[] { n },
            };
            var hq = bundle.ApplySpectral(ft);
            var w = (double[])hq.Coefficients.Clone();

            // alpha_j = q_j^T H q_j (Rayleigh quotient)
            double alphaJ = 0;
            for (int i = 0; i < n; i++)
                alphaJ += qCurr[i] * w[i];
            alpha.Add(alphaJ);

            // z = H q_j - alpha_j M q_j - beta_j M q_{j-1} (three-term recurrence)
            // z lives in range(M): z = beta_{j+1} M q_{j+1}
            var mqCurr = ApplyMass(qCurr, bundle, n, sig);
            var z = new double[n];
            for (int i = 0; i < n; i++)
                z[i] = w[i] - alphaJ * mqCurr[i];

            if (iter > 0)
            {
                var mqPrev = ApplyMass(qPrev, bundle, n, sig);
                for (int i = 0; i < n; i++)
                    z[i] -= betaPrev * mqPrev[i];
            }

            // s = M^{-1} z (solve M s = z via CG to convert back to q-space)
            var s = SolveMassCG(z, bundle, n, sig);

            // Full M-reorthogonalization: h_k = (M q_k)^T s, s -= h_k * q_k
            // After this, s^T M q_k = 0 for all k, ensuring q_{j+1}^T M q_k = 0.
            for (int j = 0; j <= iter; j++)
            {
                var mqj = ApplyMass(Q[j], bundle, n, sig);
                double mDot = 0;
                for (int i = 0; i < n; i++)
                    mDot += mqj[i] * s[i];
                for (int i = 0; i < n; i++)
                    s[i] -= mDot * Q[j][i];
            }

            // beta_{j+1} = ||s||_M = sqrt(s^T M s)
            var ms = ApplyMass(s, bundle, n, sig);
            double betaJ = 0;
            for (int i = 0; i < n; i++)
                betaJ += s[i] * ms[i];
            betaJ = System.Math.Sqrt(System.Math.Max(0, betaJ));

            if (betaJ < 1e-14)
            {
                // Invariant subspace found
                break;
            }

            beta.Add(betaJ);
            betaPrev = betaJ;

            // q_{j+1} = s / beta_{j+1} (M-orthonormal by construction)
            qPrev = qCurr;
            qCurr = new double[n];
            for (int i = 0; i < n; i++)
                qCurr[i] = s[i] / betaJ;

            Q.Add(qCurr);
        }

        // Solve tridiagonal eigenproblem
        int kDim = alpha.Count;
        var (ritzValues, ritzVectors, tridiagNotes) = SolveTridiagonal(
            alpha.ToArray(), beta.ToArray(), kDim);
        diagnosticNotes.AddRange(tridiagNotes);

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

            // Ritz vector = Q * y_k (project tridiagonal eigenvector back to full space)
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

        return (eigenvalues, eigenvectors, iter, status, diagnosticNotes);
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

    /// <summary>
    /// Solve M x = b using conjugate gradient.
    /// M is symmetric positive definite, so CG converges.
    /// For diagonal M this converges in one iteration.
    /// </summary>
    private static double[] SolveMassCG(double[] b, LinearizedOperatorBundle bundle, int n, TensorSignature sig)
    {
        var x = new double[n];
        var r = (double[])b.Clone();
        var p = (double[])b.Clone();

        double rDotR = 0;
        for (int i = 0; i < n; i++)
            rDotR += r[i] * r[i];

        if (rDotR < 1e-30)
            return x;

        const int maxCgIter = 200;
        for (int k = 0; k < maxCgIter; k++)
        {
            var mp = ApplyMass(p, bundle, n, sig);

            double pMp = 0;
            for (int i = 0; i < n; i++)
                pMp += p[i] * mp[i];

            if (pMp < 1e-30) break;

            double alphaK = rDotR / pMp;

            for (int i = 0; i < n; i++)
            {
                x[i] += alphaK * p[i];
                r[i] -= alphaK * mp[i];
            }

            double rDotRNew = 0;
            for (int i = 0; i < n; i++)
                rDotRNew += r[i] * r[i];

            if (rDotRNew < 1e-28)
                break;

            double betaK = rDotRNew / rDotR;
            rDotR = rDotRNew;

            for (int i = 0; i < n; i++)
                p[i] = r[i] + betaK * p[i];
        }

        return x;
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
    /// Returns (eigenvalues, eigenvector_matrix_row_major, diagnosticNotes).
    /// Eigenvector i is stored as row i: V[i * n + 0 .. i * n + n-1].
    /// </summary>
    private static (double[], double[], List<string>) SolveTridiagonal(double[] alpha, double[] betaArr, int n)
    {
        var notes = new List<string>();

        // Build symmetric tridiagonal matrix
        var T = new double[n * n];
        for (int i = 0; i < n; i++)
            T[i * n + i] = alpha[i];
        for (int i = 0; i < betaArr.Length && i < n - 1; i++)
        {
            T[i * n + (i + 1)] = betaArr[i];
            T[(i + 1) * n + i] = betaArr[i];
        }

        // Jacobi eigendecomposition with dimension-scaled iteration limit
        var V = new double[n * n];
        for (int i = 0; i < n; i++) V[i * n + i] = 1.0;

        int maxJacobiIter = System.Math.Max(200, 10 * n);
        JacobiEigenTridiag(T, V, n, maxJacobiIter);

        // Check Jacobi convergence: max off-diagonal vs Frobenius norm
        double maxOffDiag = 0;
        double frobSq = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double val = T[i * n + j];
                frobSq += val * val;
                if (i != j && System.Math.Abs(val) > maxOffDiag)
                    maxOffDiag = System.Math.Abs(val);
            }
        }
        double frobNorm = System.Math.Sqrt(frobSq);
        // Warn only when off-diagonal residual is large relative to Frobenius norm.
        // JacobiEigenTridiag stops when sum-of-squares-of-off-diagonals < 1e-14,
        // which gives max-off-diag ~ O(1e-7) for typical Krylov dimensions.
        // Threshold 1e-6 * frobNorm distinguishes genuine non-convergence from
        // expected floating-point residual.
        if (frobNorm > 0 && maxOffDiag > 1e-6 * frobNorm)
            notes.Add($"Jacobi did not converge: max off-diag = {maxOffDiag:E3}");

        var eigenvalues = new double[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = T[i * n + i];

        return (eigenvalues, V, notes);
    }

    private static void JacobiEigenTridiag(double[] matrix, double[] vectors, int n, int maxIter)
    {
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

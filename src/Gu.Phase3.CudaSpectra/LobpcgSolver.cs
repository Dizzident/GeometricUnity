namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Configuration for the LOBPCG eigensolver.
/// </summary>
public sealed class LobpcgConfig
{
    /// <summary>Number of eigenvalues to compute.</summary>
    public int NumEigenvalues { get; init; } = 10;

    /// <summary>Maximum number of outer iterations.</summary>
    public int MaxIterations { get; init; } = 200;

    /// <summary>Convergence tolerance for eigenvalue residual norms.</summary>
    public double Tolerance { get; init; } = 1e-8;

    /// <summary>Block size (>= NumEigenvalues for standard LOBPCG).</summary>
    public int BlockSize { get; init; } = 10;

    /// <summary>Random seed for initial block generation.</summary>
    public int Seed { get; init; } = 42;
}

/// <summary>
/// Result from a LOBPCG eigensolver run.
/// </summary>
public sealed class LobpcgResult
{
    /// <summary>Computed eigenvalues, sorted ascending.</summary>
    public required double[] Eigenvalues { get; init; }

    /// <summary>
    /// Eigenvectors stored column-major: eigenvectors[i] has length stateDim.
    /// eigenvectors[k] is the k-th eigenvector.
    /// </summary>
    public required double[][] Eigenvectors { get; init; }

    /// <summary>Residual norms for each eigenpair: ||H*x_k - lambda_k * M*x_k||.</summary>
    public required double[] ResidualNorms { get; init; }

    /// <summary>Number of iterations performed.</summary>
    public required int Iterations { get; init; }

    /// <summary>Whether all requested eigenpairs converged.</summary>
    public required bool Converged { get; init; }

    /// <summary>Number of converged eigenpairs.</summary>
    public int NumConverged => ResidualNorms.Count(r => r < 1e-8);
}

/// <summary>
/// Locally Optimal Block Preconditioned Conjugate Gradient (LOBPCG) eigensolver.
/// Solves the generalized eigenproblem: H * x = lambda * M * x
/// using only operator-vector products (matrix-free).
///
/// This implementation works with ISpectralKernel for both CPU and GPU backends.
/// The same algorithm runs on both; the kernel dispatch is what differs.
/// </summary>
public sealed class LobpcgSolver
{
    private readonly ISpectralKernel _kernel;

    public LobpcgSolver(ISpectralKernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
    }

    /// <summary>
    /// Solve the generalized eigenproblem H*x = lambda * M*x.
    /// Returns the smallest eigenvalues and their eigenvectors.
    /// </summary>
    public LobpcgResult Solve(LobpcgConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        int n = _kernel.StateDimension;
        int k = config.NumEigenvalues;
        int blockSize = System.Math.Max(k, config.BlockSize);

        // Initialize random block X (n x blockSize)
        var rng = new Random(config.Seed);
        var X = new double[blockSize][];
        for (int j = 0; j < blockSize; j++)
        {
            X[j] = new double[n];
            for (int i = 0; i < n; i++)
                X[j][i] = rng.NextDouble() - 0.5;
        }

        // M-orthonormalize initial block
        MOrthonormalize(X, n);

        var eigenvalues = new double[k];
        var residualNorms = new double[k];
        bool converged = false;
        int iter = 0;

        for (iter = 0; iter < config.MaxIterations; iter++)
        {
            // Compute H*X and M*X
            var HX = ApplyToBlock(X, blockSize, n, applySpectral: true);
            var MX = ApplyToBlock(X, blockSize, n, applySpectral: false);

            // Rayleigh quotients: lambda_j = (X_j^T H X_j) / (X_j^T M X_j)
            for (int j = 0; j < System.Math.Min(k, blockSize); j++)
            {
                double xhx = Dot(X[j], HX[j], n);
                double xmx = Dot(X[j], MX[j], n);
                eigenvalues[j] = xmx > 1e-30 ? xhx / xmx : double.PositiveInfinity;
            }

            // Compute residuals: W_j = H*X_j - lambda_j * M*X_j
            var W = new double[blockSize][];
            for (int j = 0; j < blockSize; j++)
            {
                W[j] = new double[n];
                double lam = j < k ? eigenvalues[j] : eigenvalues[k - 1];
                for (int i = 0; i < n; i++)
                    W[j][i] = HX[j][i] - lam * MX[j][i];
            }

            // Check convergence
            bool allConverged = true;
            for (int j = 0; j < k; j++)
            {
                residualNorms[j] = L2Norm(W[j], n);
                if (residualNorms[j] > config.Tolerance)
                    allConverged = false;
            }

            if (allConverged)
            {
                converged = true;
                break;
            }

            // Build search space S = [X, W] (block steepest descent variant)
            int searchSize = 2 * blockSize;
            var S = new double[searchSize][];
            for (int j = 0; j < blockSize; j++)
                S[j] = X[j];
            for (int j = 0; j < blockSize; j++)
                S[blockSize + j] = W[j];

            // Solve projected eigenproblem via Rayleigh-Ritz
            var HS = ApplyToBlock(S, searchSize, n, applySpectral: true);
            var MS = ApplyToBlock(S, searchSize, n, applySpectral: false);

            var projH = new double[searchSize, searchSize];
            var projM = new double[searchSize, searchSize];
            for (int i = 0; i < searchSize; i++)
            for (int j = 0; j <= i; j++)
            {
                double hval = Dot(S[i], HS[j], n);
                double mval = Dot(S[i], MS[j], n);
                projH[i, j] = hval;
                projH[j, i] = hval;
                projM[i, j] = mval;
                projM[j, i] = mval;
            }

            // Regularize projected M for numerical stability
            double mTrace = 0;
            for (int i = 0; i < searchSize; i++)
                mTrace += projM[i, i];
            double regularization = System.Math.Max(1e-10, 1e-10 * mTrace / searchSize);
            for (int i = 0; i < searchSize; i++)
                projM[i, i] += regularization;

            // Solve small projected eigenproblem
            var (projEvals, projEvecs) = SolveSmallGeneralizedEigen(projH, projM, searchSize, blockSize);

            // Check for degenerate result (all infinite eigenvalues)
            bool degenerateResult = true;
            for (int j = 0; j < blockSize; j++)
            {
                if (double.IsFinite(projEvals[j]))
                { degenerateResult = false; break; }
            }
            if (degenerateResult) continue; // skip update, try again

            // Update X = S * projEvecs
            for (int j = 0; j < blockSize; j++)
            {
                X[j] = new double[n];
                for (int s = 0; s < searchSize; s++)
                {
                    double coeff = projEvecs[j, s];
                    if (!double.IsFinite(coeff)) continue;
                    for (int i = 0; i < n; i++)
                        X[j][i] += coeff * S[s][i];
                }
            }

            // M-orthonormalize updated X
            MOrthonormalize(X, n);
        }

        // Extract final eigenvectors (only first k)
        var finalVecs = new double[k][];
        for (int j = 0; j < k; j++)
            finalVecs[j] = (double[])X[j].Clone();

        return new LobpcgResult
        {
            Eigenvalues = eigenvalues,
            Eigenvectors = finalVecs,
            ResidualNorms = residualNorms,
            Iterations = iter + 1,
            Converged = converged,
        };
    }

    private double[][] ApplyToBlock(double[][] block, int count, int n, bool applySpectral)
    {
        var result = new double[count][];
        for (int j = 0; j < count; j++)
        {
            result[j] = new double[n];
            if (applySpectral)
                _kernel.ApplySpectral(block[j], result[j]);
            else
                _kernel.ApplyMass(block[j], result[j]);
        }
        return result;
    }

    private void MOrthonormalize(double[][] X, int n)
    {
        int blockSize = X.Length;
        for (int j = 0; j < blockSize; j++)
        {
            // Orthogonalize against previous vectors (M-inner product)
            var MXj = new double[n];
            _kernel.ApplyMass(X[j], MXj);

            for (int i = 0; i < j; i++)
            {
                double proj = Dot(X[i], MXj, n);
                for (int idx = 0; idx < n; idx++)
                    X[j][idx] -= proj * X[i][idx];
            }

            // Normalize
            _kernel.ApplyMass(X[j], MXj);
            double norm = System.Math.Sqrt(System.Math.Max(0, Dot(X[j], MXj, n)));
            if (norm > 1e-30)
            {
                double invNorm = 1.0 / norm;
                for (int idx = 0; idx < n; idx++)
                    X[j][idx] *= invNorm;
            }
        }
    }

    /// <summary>
    /// Solve small generalized eigenproblem projH * c = lambda * projM * c
    /// via Jacobi-style symmetric eigendecomposition.
    /// Returns eigenvalues sorted ascending and corresponding eigenvectors.
    /// In production, this would call LAPACK dsygv.
    /// </summary>
    internal static (double[] Eigenvalues, double[,] Eigenvectors) SolveSmallGeneralizedEigen(
        double[,] H, double[,] M, int size, int numWanted)
    {
        int k = System.Math.Min(numWanted, size);

        // Reduce to standard eigenproblem via Cholesky of M.
        // M = L L^T, then solve (L^{-1} H L^{-T}) y = lambda y, with c = L^{-T} y.
        // For numerical safety, if M is near-identity, just solve H directly.

        // Compute Cholesky L of M
        var L = new double[size, size];
        bool choleskyOk = true;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = M[i, j];
                for (int p = 0; p < j; p++)
                    sum -= L[i, p] * L[j, p];

                if (i == j)
                {
                    if (sum <= 1e-30) { choleskyOk = false; break; }
                    L[i, j] = System.Math.Sqrt(sum);
                }
                else
                {
                    L[i, j] = sum / L[j, j];
                }
            }
            if (!choleskyOk) break;
        }

        // Build matrix A = L^{-1} H L^{-T}  (or just H if Cholesky failed)
        var A = new double[size, size];
        if (choleskyOk)
        {
            // Solve L^{-1} H -> tmp, then tmp * L^{-T} -> A
            // tmp[i,j] = (L^{-1} H)[i,j]
            var Linv = InvertLowerTriangular(L, size);
            // A = Linv * H * Linv^T
            var tmp = new double[size, size];
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                double s = 0;
                for (int p = 0; p < size; p++)
                    s += Linv[i, p] * H[p, j];
                tmp[i, j] = s;
            }
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                double s = 0;
                for (int p = 0; p < size; p++)
                    s += tmp[i, p] * Linv[j, p]; // Linv^T[p,j] = Linv[j,p]
                A[i, j] = s;
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                A[i, j] = H[i, j];
        }

        // Jacobi eigendecomposition of symmetric A
        var (evals, evecs) = JacobiEigen(A, size);

        // Sort ascending
        var indices = Enumerable.Range(0, size).OrderBy(i => evals[i]).ToArray();

        // Back-transform eigenvectors if Cholesky was used: c = L^{-T} y
        var result = new double[k, size];
        var resultEvals = new double[k];
        for (int j = 0; j < k; j++)
        {
            resultEvals[j] = evals[indices[j]];
            if (choleskyOk)
            {
                // c = L^{-T} * y
                var Linv = InvertLowerTriangular(L, size);
                for (int r = 0; r < size; r++)
                {
                    double s = 0;
                    for (int p = 0; p < size; p++)
                        s += Linv[p, r] * evecs[indices[j], p]; // Linv^T[r,p] = Linv[p,r]
                    result[j, r] = s;
                }
            }
            else
            {
                for (int r = 0; r < size; r++)
                    result[j, r] = evecs[indices[j], r];
            }
        }

        return (resultEvals, result);
    }

    private static double[,] InvertLowerTriangular(double[,] L, int n)
    {
        var inv = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            inv[i, i] = 1.0 / L[i, i];
            for (int j = i + 1; j < n; j++)
            {
                double sum = 0;
                for (int p = i; p < j; p++)
                    sum -= L[j, p] * inv[p, i];
                inv[j, i] = sum / L[j, j];
            }
        }
        return inv;
    }

    /// <summary>
    /// Jacobi eigendecomposition for symmetric matrix.
    /// Returns all eigenvalues and eigenvectors (row-major: evecs[k, :] is k-th eigenvector).
    /// </summary>
    private static (double[] Eigenvalues, double[,] Eigenvectors) JacobiEigen(double[,] A, int n)
    {
        var D = new double[n, n];
        var V = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            V[i, i] = 1.0;
            for (int j = 0; j < n; j++)
                D[i, j] = A[i, j];
        }

        for (int sweep = 0; sweep < 100; sweep++)
        {
            // Find largest off-diagonal element
            double maxOff = 0;
            int p = 0, q = 1;
            for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                if (System.Math.Abs(D[i, j]) > maxOff)
                {
                    maxOff = System.Math.Abs(D[i, j]);
                    p = i; q = j;
                }
            }
            if (maxOff < 1e-15) break;

            // Compute rotation
            double theta = 0.5 * System.Math.Atan2(2.0 * D[p, q], D[p, p] - D[q, q]);
            double c = System.Math.Cos(theta);
            double s = System.Math.Sin(theta);

            // Apply rotation D = G^T D G
            var newD = new double[n, n];
            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                newD[i, j] = D[i, j];

            for (int i = 0; i < n; i++)
            {
                if (i != p && i != q)
                {
                    newD[i, p] = c * D[i, p] + s * D[i, q];
                    newD[p, i] = newD[i, p];
                    newD[i, q] = -s * D[i, p] + c * D[i, q];
                    newD[q, i] = newD[i, q];
                }
            }
            newD[p, p] = c * c * D[p, p] + 2 * s * c * D[p, q] + s * s * D[q, q];
            newD[q, q] = s * s * D[p, p] - 2 * s * c * D[p, q] + c * c * D[q, q];
            newD[p, q] = 0;
            newD[q, p] = 0;

            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                D[i, j] = newD[i, j];

            // Update eigenvector matrix V = V * G
            for (int i = 0; i < n; i++)
            {
                double vip = V[i, p];
                double viq = V[i, q];
                V[i, p] = c * vip + s * viq;
                V[i, q] = -s * vip + c * viq;
            }
        }

        var evals = new double[n];
        var evecs = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            evals[i] = D[i, i];
            for (int j = 0; j < n; j++)
                evecs[i, j] = V[j, i]; // transpose: row i of evecs = column i of V
        }

        return (evals, evecs);
    }

    private static double Dot(double[] a, double[] b, int n)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
            sum += a[i] * b[i];
        return sum;
    }

    private static double L2Norm(double[] a, int n)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
            sum += a[i] * a[i];
        return System.Math.Sqrt(sum);
    }
}

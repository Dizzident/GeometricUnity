using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Orthonormal basis for the gauge image Im(Gamma_*) in connection space.
///
/// Built by applying Gamma_* to a basis of gauge parameters, assembling the
/// gauge Gram matrix under M_state, and orthonormalizing via SVD.
///
/// Handles rank deficiency explicitly: if the gauge image is lower-rank than
/// expected, the defect is recorded rather than silently dropped.
/// </summary>
public sealed class GaugeBasis
{
    /// <summary>Orthonormal gauge basis vectors (each is an edge-valued connection perturbation).</summary>
    public IReadOnlyList<double[]> Vectors { get; }

    /// <summary>Singular values from the SVD of the gauge image.</summary>
    public IReadOnlyList<double> SingularValues { get; }

    /// <summary>All singular values from the SVD, sorted descending (before cutoff filtering).</summary>
    public IReadOnlyList<double> AllSingularValues { get; }

    /// <summary>Number of retained basis vectors (effective gauge rank).</summary>
    public int Rank => Vectors.Count;

    /// <summary>Expected gauge rank (dimG * (VertexCount - 1) for connected mesh).</summary>
    public int ExpectedRank { get; }

    /// <summary>Rank defect = ExpectedRank - Rank.</summary>
    public int RankDefect => ExpectedRank - Rank;

    /// <summary>SVD cutoff tolerance used for rank determination.</summary>
    public double SvdCutoff { get; }

    /// <summary>Background ID this gauge basis was computed for.</summary>
    public string BackgroundId { get; }

    /// <summary>Connection space dimension (EdgeCount * dimG).</summary>
    public int ConnectionDimension { get; }

    private GaugeBasis(
        IReadOnlyList<double[]> vectors,
        IReadOnlyList<double> singularValues,
        IReadOnlyList<double> allSingularValues,
        int expectedRank,
        double svdCutoff,
        string backgroundId,
        int connectionDimension)
    {
        Vectors = vectors;
        SingularValues = singularValues;
        AllSingularValues = allSingularValues;
        ExpectedRank = expectedRank;
        SvdCutoff = svdCutoff;
        BackgroundId = backgroundId;
        ConnectionDimension = connectionDimension;
    }

    /// <summary>
    /// Build an orthonormal gauge basis from the linearized gauge action.
    ///
    /// Algorithm:
    /// 1. Apply Gamma_* to each standard basis vector e_j of gauge parameter space.
    /// 2. Assemble the resulting columns into a matrix G (connectionDim x gaugeDim).
    /// 3. Compute thin SVD: G = U S V^T.
    /// 4. Retain columns of U with singular values above cutoff.
    /// </summary>
    /// <param name="linearization">The linearized gauge action Gamma_*.</param>
    /// <param name="svdCutoff">Relative SVD cutoff for rank determination (default 1e-10).</param>
    public static GaugeBasis Build(
        GaugeActionLinearization linearization,
        double svdCutoff = 1e-10)
    {
        if (linearization == null) throw new ArgumentNullException(nameof(linearization));
        if (svdCutoff < 0) throw new ArgumentException("SVD cutoff must be non-negative.", nameof(svdCutoff));

        int gaugeDim = linearization.GaugeParameterDimension;
        int connDim = linearization.ConnectionDimension;
        var op = linearization.Operator;

        // Build the gauge image matrix: columns are Gamma_*(e_j)
        var columns = new double[gaugeDim][];
        var ej = new double[gaugeDim];

        for (int j = 0; j < gaugeDim; j++)
        {
            ej[j] = 1.0;
            var result = op.Apply(new FieldTensor
            {
                Label = $"e_{j}",
                Signature = op.InputSignature,
                Coefficients = (double[])ej.Clone(),
                Shape = new[] { gaugeDim },
            });
            columns[j] = result.Coefficients;
            ej[j] = 0.0;
        }

        // Assemble into a flat column-major matrix for SVD
        // G[i, j] = columns[j][i]
        var gMatrix = new double[connDim * gaugeDim];
        for (int j = 0; j < gaugeDim; j++)
            for (int i = 0; i < connDim; i++)
                gMatrix[j * connDim + i] = columns[j][i];

        // Compute thin SVD using Householder bidiagonalization
        int minDim = System.Math.Min(connDim, gaugeDim);
        var svdResult = ThinSvd(gMatrix, connDim, gaugeDim, minDim);

        // Determine rank using relative cutoff
        double maxSigma = svdResult.SingularValues.Length > 0 ? svdResult.SingularValues[0] : 0.0;
        double absCutoff = svdCutoff * maxSigma;

        var vectors = new List<double[]>();
        var sigmas = new List<double>();
        var allSigmas = new List<double>();

        for (int k = 0; k < minDim; k++)
        {
            allSigmas.Add(svdResult.SingularValues[k]);
            if (svdResult.SingularValues[k] > absCutoff)
            {
                var col = new double[connDim];
                for (int i = 0; i < connDim; i++)
                    col[i] = svdResult.U[k * connDim + i];
                vectors.Add(col);
                sigmas.Add(svdResult.SingularValues[k]);
            }
        }

        return new GaugeBasis(
            vectors,
            sigmas,
            allSigmas,
            linearization.ExpectedGaugeRank,
            svdCutoff,
            linearization.BackgroundId,
            connDim);
    }

    /// <summary>
    /// Build an M_state-orthonormal gauge basis from the linearized gauge action.
    ///
    /// When M_state != I, the gauge basis must be M_state-orthonormal so that
    /// P_phys is M_state-self-adjoint (physicist constraint #8).
    ///
    /// Algorithm:
    /// 1. Build image matrix G as in standard Build.
    /// 2. Apply M^{1/2} to rows: G_w = M^{1/2} G.
    /// 3. SVD of G_w: G_w = U_w S V^T.
    /// 4. Undo scaling: q_k = M^{-1/2} u_w_k, then M-normalize.
    ///
    /// The resulting vectors satisfy q_i^T M q_j = delta_ij.
    /// </summary>
    /// <param name="linearization">The linearized gauge action Gamma_*.</param>
    /// <param name="massWeights">Diagonal M_state weights (length = ConnectionDimension).</param>
    /// <param name="svdCutoff">Relative SVD cutoff for rank determination (default 1e-10).</param>
    public static GaugeBasis BuildWithMass(
        GaugeActionLinearization linearization,
        double[] massWeights,
        double svdCutoff = 1e-10)
    {
        if (linearization == null) throw new ArgumentNullException(nameof(linearization));
        if (massWeights == null) throw new ArgumentNullException(nameof(massWeights));
        if (svdCutoff < 0) throw new ArgumentException("SVD cutoff must be non-negative.", nameof(svdCutoff));

        int gaugeDim = linearization.GaugeParameterDimension;
        int connDim = linearization.ConnectionDimension;
        if (massWeights.Length != connDim)
            throw new ArgumentException(
                $"massWeights length {massWeights.Length} != connection dimension {connDim}.");

        var op = linearization.Operator;

        // Build the gauge image matrix columns
        var columns = new double[gaugeDim][];
        var ej = new double[gaugeDim];
        for (int j = 0; j < gaugeDim; j++)
        {
            ej[j] = 1.0;
            var result = op.Apply(new FieldTensor
            {
                Label = $"e_{j}",
                Signature = op.InputSignature,
                Coefficients = (double[])ej.Clone(),
                Shape = new[] { gaugeDim },
            });
            columns[j] = result.Coefficients;
            ej[j] = 0.0;
        }

        // Apply M^{1/2} to rows: G_w[i,j] = sqrt(M[i]) * G[i,j]
        var gMatrix = new double[connDim * gaugeDim];
        for (int j = 0; j < gaugeDim; j++)
            for (int i = 0; i < connDim; i++)
            {
                double sqrtM = System.Math.Sqrt(System.Math.Max(0.0, massWeights[i]));
                gMatrix[j * connDim + i] = sqrtM * columns[j][i];
            }

        // SVD of the weighted matrix
        int minDim = System.Math.Min(connDim, gaugeDim);
        var svdResult = ThinSvd(gMatrix, connDim, gaugeDim, minDim);

        double maxSigma = svdResult.SingularValues.Length > 0 ? svdResult.SingularValues[0] : 0.0;
        double absCutoff = svdCutoff * maxSigma;

        var vectors = new List<double[]>();
        var sigmas = new List<double>();
        var allSigmas = new List<double>();

        for (int k = 0; k < minDim; k++)
        {
            allSigmas.Add(svdResult.SingularValues[k]);
            if (svdResult.SingularValues[k] > absCutoff)
            {
                // Undo M^{1/2} scaling: q = M^{-1/2} u_w
                // Since u_w are Euclidean-orthonormal (from SVD), the resulting q
                // are M-orthonormal: q_i^T M q_j = u_i^T u_j = delta_ij.
                var col = new double[connDim];
                for (int i = 0; i < connDim; i++)
                {
                    double sqrtM = System.Math.Sqrt(System.Math.Max(0.0, massWeights[i]));
                    col[i] = sqrtM > 1e-15
                        ? svdResult.U[k * connDim + i] / sqrtM
                        : 0.0;
                }

                vectors.Add(col);
                sigmas.Add(svdResult.SingularValues[k]);
            }
        }

        // Modified Gram-Schmidt with M inner product to ensure M-orthonormality.
        // The SVD-based approach is correct in exact arithmetic, but the Jacobi
        // eigendecomposition may lose orthogonality for near-degenerate singular
        // values, so we re-orthonormalize explicitly.
        for (int i = 0; i < vectors.Count; i++)
        {
            // Orthogonalize against all previous vectors
            for (int j = 0; j < i; j++)
            {
                double dot = 0;
                for (int d = 0; d < connDim; d++)
                    dot += vectors[i][d] * massWeights[d] * vectors[j][d];
                for (int d = 0; d < connDim; d++)
                    vectors[i][d] -= dot * vectors[j][d];
            }

            // M-normalize
            double norm = 0;
            for (int d = 0; d < connDim; d++)
                norm += vectors[i][d] * massWeights[d] * vectors[i][d];
            norm = System.Math.Sqrt(norm);
            if (norm > 1e-15)
            {
                for (int d = 0; d < connDim; d++)
                    vectors[i][d] /= norm;
            }
        }

        return new GaugeBasis(
            vectors,
            sigmas,
            allSigmas,
            linearization.ExpectedGaugeRank,
            svdCutoff,
            linearization.BackgroundId,
            connDim);
    }

    /// <summary>
    /// Thin SVD via Golub-Kahan bidiagonalization.
    /// For small matrices (toy problems), uses explicit Jacobi SVD.
    /// </summary>
    private static SvdResult ThinSvd(double[] a, int m, int n, int k)
    {
        // One-sided Jacobi SVD for small matrices
        // Work on A^T A approach: compute V from eigendecomposition, then U = A V S^{-1}

        // For correctness on toy problems, use a simple approach:
        // Compute Gram matrix G = A^T A, find eigenvectors, derive SVD.
        var ata = new double[n * n];
        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                double dot = 0;
                for (int r = 0; r < m; r++)
                    dot += a[i * m + r] * a[j * m + r];
                ata[i * n + j] = dot;
                ata[j * n + i] = dot;
            }
        }

        // Jacobi eigendecomposition of A^T A
        var eigenVectors = new double[n * n];
        for (int i = 0; i < n; i++) eigenVectors[i * n + i] = 1.0;

        var eigenValues = JacobiEigen(ata, eigenVectors, n);

        // Sort by eigenvalue descending
        var indices = Enumerable.Range(0, n).OrderByDescending(i => eigenValues[i]).ToArray();
        var sortedEigenValues = indices.Select(i => eigenValues[i]).ToArray();
        var sortedV = new double[n * n];
        for (int col = 0; col < n; col++)
            for (int row = 0; row < n; row++)
                sortedV[col * n + row] = eigenVectors[indices[col] * n + row];

        // Singular values = sqrt(eigenvalues)
        var singularValues = new double[k];
        for (int i = 0; i < k; i++)
            singularValues[i] = sortedEigenValues[i] > 0 ? System.Math.Sqrt(sortedEigenValues[i]) : 0.0;

        // U columns: u_i = (1/sigma_i) A v_i
        var uMatrix = new double[k * m];
        for (int i = 0; i < k; i++)
        {
            if (singularValues[i] > 1e-15)
            {
                for (int r = 0; r < m; r++)
                {
                    double val = 0;
                    for (int c = 0; c < n; c++)
                        val += a[c * m + r] * sortedV[i * n + c];
                    uMatrix[i * m + r] = val / singularValues[i];
                }
            }
        }

        return new SvdResult(uMatrix, singularValues);
    }

    /// <summary>
    /// Jacobi eigendecomposition of a symmetric matrix.
    /// Modifies matrix in-place, returns eigenvalues on diagonal.
    /// </summary>
    private static double[] JacobiEigen(double[] matrix, double[] vectors, int n)
    {
        int maxIter = System.Math.Max(200, 10 * n);
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

                    // Rotate matrix
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

                    // Rotate eigenvectors
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

    private sealed class SvdResult
    {
        public double[] U { get; }
        public double[] SingularValues { get; }

        public SvdResult(double[] u, double[] singularValues)
        {
            U = u;
            SingularValues = singularValues;
        }
    }
}

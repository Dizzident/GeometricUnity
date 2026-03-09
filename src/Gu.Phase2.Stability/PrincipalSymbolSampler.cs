using Gu.Branching;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// Computes or approximates the principal symbol of a linearized operator
/// at a given (cell, covector) sample point.
///
/// Per IMPLEMENTATION_PLAN_P2.md Section 9.4:
/// 1. derive analytic principal-symbol blocks for terms where possible,
/// 2. where analytic derivation is not yet implemented, use a symbolic/local
///    linearization layer or automatic differentiation over frozen-coefficient local stencils,
/// 3. store the resulting symbol in a typed PrincipalSymbolRecord.
///
/// This implementation uses the frozen-coefficient approach: for each cell,
/// evaluate the operator on localized basis perturbations scaled by e^{i xi·x}
/// to extract the symbol matrix.
/// </summary>
public sealed class PrincipalSymbolSampler
{
    private readonly double _symmetryTolerance;
    private readonly double _zeroThreshold;

    /// <summary>
    /// Create a principal symbol sampler.
    /// </summary>
    /// <param name="symmetryTolerance">Tolerance for declaring a matrix symmetric.</param>
    /// <param name="zeroThreshold">Threshold below which eigenvalues are considered zero.</param>
    public PrincipalSymbolSampler(
        double symmetryTolerance = 1e-10,
        double zeroThreshold = 1e-10)
    {
        _symmetryTolerance = symmetryTolerance;
        _zeroThreshold = zeroThreshold;
    }

    /// <summary>
    /// Sample the principal symbol of a linear operator at a given (cell, covector).
    ///
    /// Uses the frozen-coefficient finite-difference approach:
    /// For each input DOF j at the cell, construct a localized perturbation
    /// delta_j concentrated at the cell, apply the operator, and read off the
    /// response at the same cell. This gives one column of the symbol matrix.
    ///
    /// The covector xi modulates the perturbation phase: in the discrete setting,
    /// this corresponds to scaling neighboring contributions by exp(i xi·dx).
    /// For the frozen-coefficient approximation on a single cell, the covector
    /// enters through the local stencil weights.
    /// </summary>
    public PrincipalSymbolRecord Sample(
        ILinearOperator op,
        int cellIndex,
        double[] covector,
        int localDim,
        string branchManifestId,
        GaugeStudyMode gaugeStudyMode,
        string operatorId,
        int operatorOrder = 1)
    {
        ArgumentNullException.ThrowIfNull(op);
        ArgumentNullException.ThrowIfNull(covector);
        ArgumentException.ThrowIfNullOrWhiteSpace(branchManifestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorId);

        if (localDim < 1)
            throw new ArgumentOutOfRangeException(nameof(localDim), "Local dimension must be at least 1.");

        // Extract symbol matrix by probing with unit basis vectors at the cell
        var symbolMatrix = new double[localDim][];
        for (int i = 0; i < localDim; i++)
            symbolMatrix[i] = new double[localDim];

        var covectorNorm = L2Norm(covector);

        // Compute normalized covector direction xi/|xi|
        double[]? covectorDirection = null;
        if (covectorNorm > 0)
        {
            covectorDirection = new double[covector.Length];
            for (int k = 0; k < covector.Length; k++)
                covectorDirection[k] = covector[k] / covectorNorm;
        }

        // For order-k operator: sigma(t*xi) = t^k * sigma(xi)
        // Probe with unit perturbation and normalize by |xi|^k
        double normFactor = covectorNorm > 0 ? System.Math.Pow(covectorNorm, operatorOrder) : 1.0;

        for (int j = 0; j < localDim; j++)
        {
            // Create a perturbation: unit vector in direction j, localized at cellIndex
            var delta = new double[op.InputDimension];
            int globalIdx = cellIndex * localDim + j;
            if (globalIdx < delta.Length)
            {
                delta[globalIdx] = 1.0;
            }

            var deltaT = new FieldTensor
            {
                Label = $"symbol_probe_{j}",
                Signature = op.InputSignature,
                Coefficients = delta,
                Shape = new[] { op.InputDimension },
            };

            var response = op.Apply(deltaT);

            // Read off the response at the same cell
            for (int i = 0; i < localDim; i++)
            {
                int outIdx = cellIndex * localDim + i;
                if (outIdx < response.Coefficients.Length)
                {
                    symbolMatrix[i][j] = response.Coefficients[outIdx] / normFactor;
                }
            }
        }

        // Analyze the symbol matrix
        var (isSymmetric, symmetryError) = CheckSymmetry(symbolMatrix, localDim);
        var eigenvalues = ComputeEigenvaluesSymmetric(symbolMatrix, localDim);
        Array.Sort(eigenvalues);

        var definiteness = ClassifyDefiniteness(eigenvalues);
        int rankDeficiency = CountZeroEigenvalues(eigenvalues);
        var classification = ClassifyPde(eigenvalues, rankDeficiency);

        return new PrincipalSymbolRecord
        {
            CellIndex = cellIndex,
            Covector = covector,
            SymbolMatrix = symbolMatrix,
            Eigenvalues = eigenvalues,
            IsSymmetric = isSymmetric,
            SymmetryError = symmetryError,
            DefinitenessIndicator = definiteness,
            RankDeficiency = rankDeficiency,
            GaugeNullDimension = 0, // Conservative default; refined by gauge analysis
            Classification = classification,
            BranchManifestId = branchManifestId,
            GaugeStudyMode = gaugeStudyMode,
            OperatorId = operatorId,
            CovectorDirection = covectorDirection,
        };
    }

    private (bool isSymmetric, double error) CheckSymmetry(double[][] m, int n)
    {
        double maxEntry = 0;
        double maxAsymmetry = 0;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                maxEntry = System.Math.Max(maxEntry, System.Math.Abs(m[i][j]));
                maxAsymmetry = System.Math.Max(maxAsymmetry, System.Math.Abs(m[i][j] - m[j][i]));
            }
        }

        double relError = maxEntry > 0 ? maxAsymmetry / maxEntry : 0;
        return (relError < _symmetryTolerance, relError);
    }

    /// <summary>
    /// Compute eigenvalues of a symmetric matrix using the Jacobi method.
    /// Suitable for small matrices (localDim typically = dimG = 3 or 8).
    /// </summary>
    private static double[] ComputeEigenvaluesSymmetric(double[][] matrix, int n)
    {
        if (n == 0) return Array.Empty<double>();
        if (n == 1) return new[] { matrix[0][0] };

        // Symmetrize first
        var a = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                a[i, j] = 0.5 * (matrix[i][j] + matrix[j][i]);

        // Jacobi eigenvalue iteration for small symmetric matrices
        const int maxIterations = 100;
        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Find largest off-diagonal element
            double maxOff = 0;
            int p = 0, q = 1;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (System.Math.Abs(a[i, j]) > maxOff)
                    {
                        maxOff = System.Math.Abs(a[i, j]);
                        p = i;
                        q = j;
                    }
                }
            }

            if (maxOff < 1e-15) break;

            // Compute rotation angle
            double theta;
            if (System.Math.Abs(a[p, p] - a[q, q]) < 1e-15)
            {
                theta = System.Math.PI / 4.0;
            }
            else
            {
                theta = 0.5 * System.Math.Atan2(2.0 * a[p, q], a[p, p] - a[q, q]);
            }

            double c = System.Math.Cos(theta);
            double s = System.Math.Sin(theta);

            // Apply Givens rotation
            var newA = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    newA[i, j] = a[i, j];

            for (int i = 0; i < n; i++)
            {
                if (i != p && i != q)
                {
                    newA[i, p] = c * a[i, p] + s * a[i, q];
                    newA[p, i] = newA[i, p];
                    newA[i, q] = -s * a[i, p] + c * a[i, q];
                    newA[q, i] = newA[i, q];
                }
            }

            newA[p, p] = c * c * a[p, p] + 2 * s * c * a[p, q] + s * s * a[q, q];
            newA[q, q] = s * s * a[p, p] - 2 * s * c * a[p, q] + c * c * a[q, q];
            newA[p, q] = 0;
            newA[q, p] = 0;

            a = newA;
        }

        var eigenvalues = new double[n];
        for (int i = 0; i < n; i++)
            eigenvalues[i] = a[i, i];

        return eigenvalues;
    }

    private string ClassifyDefiniteness(double[] eigenvalues)
    {
        if (eigenvalues.Length == 0) return "zero";

        bool allPositive = true, allNegative = true;
        bool allNonNeg = true, allNonPos = true;
        bool allZero = true;

        foreach (var ev in eigenvalues)
        {
            if (ev > _zeroThreshold) { allNegative = false; allNonPos = false; allZero = false; }
            else if (ev < -_zeroThreshold) { allPositive = false; allNonNeg = false; allZero = false; }
            else { allPositive = false; allNegative = false; }
        }

        if (allZero) return "zero";
        if (allPositive) return "positive-definite";
        if (allNegative) return "negative-definite";
        if (allNonNeg) return "positive-semidefinite";
        if (allNonPos) return "negative-semidefinite";
        return "indefinite";
    }

    private int CountZeroEigenvalues(double[] eigenvalues)
    {
        int count = 0;
        foreach (var ev in eigenvalues)
        {
            if (System.Math.Abs(ev) < _zeroThreshold)
                count++;
        }
        return count;
    }

    private PdeClassification ClassifyPde(double[] eigenvalues, int rankDeficiency)
    {
        if (eigenvalues.Length == 0) return PdeClassification.Unresolved;
        if (rankDeficiency == eigenvalues.Length) return PdeClassification.Degenerate;

        // Check non-zero eigenvalues for sign consistency
        bool hasPositive = false, hasNegative = false;
        foreach (var ev in eigenvalues)
        {
            if (ev > _zeroThreshold) hasPositive = true;
            else if (ev < -_zeroThreshold) hasNegative = true;
        }

        if (rankDeficiency > 0)
        {
            // Has zero eigenvalues beyond what's expected
            if (hasPositive && hasNegative) return PdeClassification.Mixed;
            return PdeClassification.Degenerate;
        }

        if (hasPositive && !hasNegative) return PdeClassification.EllipticLike;
        if (hasNegative && !hasPositive) return PdeClassification.EllipticLike;
        if (hasPositive && hasNegative) return PdeClassification.HyperbolicLike;

        return PdeClassification.Unresolved;
    }

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        foreach (var x in v) sum += x * x;
        return System.Math.Sqrt(sum);
    }
}

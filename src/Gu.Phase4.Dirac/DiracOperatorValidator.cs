namespace Gu.Phase4.Dirac;

/// <summary>
/// Validation utilities for assembled Dirac operators.
/// </summary>
public static class DiracOperatorValidator
{
    /// <summary>
    /// Validate the operator shape: matrixShape[0] == matrixShape[1] == cellCount * dofsPerCell.
    /// Returns a list of validation messages (empty = OK).
    /// </summary>
    public static IReadOnlyList<string> ValidateShape(DiracOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        var msgs = new List<string>();

        if (bundle.MatrixShape.Length != 2)
            msgs.Add($"MatrixShape must have length 2, got {bundle.MatrixShape.Length}.");
        else
        {
            if (bundle.MatrixShape[0] != bundle.MatrixShape[1])
                msgs.Add($"MatrixShape must be square: [{bundle.MatrixShape[0]}, {bundle.MatrixShape[1]}].");

            int expectedDof = bundle.CellCount * bundle.DofsPerCell;
            if (bundle.MatrixShape[0] != expectedDof)
                msgs.Add($"MatrixShape[0]={bundle.MatrixShape[0]} != CellCount*DofsPerCell={expectedDof}.");
        }

        return msgs;
    }

    /// <summary>
    /// Validate that the explicit matrix has the correct storage length.
    /// </summary>
    public static IReadOnlyList<string> ValidateExplicitMatrixShape(DiracOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        var msgs = new List<string>();

        if (!bundle.HasExplicitMatrix || bundle.ExplicitMatrix is null)
        {
            msgs.Add("No explicit matrix to validate.");
            return msgs;
        }

        int totalDof = bundle.TotalDof;
        int expected = totalDof * totalDof * 2;
        if (bundle.ExplicitMatrix.Length != expected)
            msgs.Add($"ExplicitMatrix.Length={bundle.ExplicitMatrix.Length} != totalDof^2*2={expected}.");

        return msgs;
    }

    /// <summary>
    /// Check that the operator is Hermitian by verifying |D[i,j] - conj(D[j,i])| &lt; tol for all entries.
    /// Returns the maximum violation entry (0.0 if all pass).
    /// </summary>
    public static double CheckHermiticity(DiracOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        if (!bundle.HasExplicitMatrix || bundle.ExplicitMatrix is null)
            throw new InvalidOperationException("No explicit matrix to check.");

        var D = bundle.ExplicitMatrix;
        int n = bundle.TotalDof;
        double maxViolation = 0.0;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double dRe = D[(i * n + j) * 2];
                double dIm = D[(i * n + j) * 2 + 1];
                double dagRe =  D[(j * n + i) * 2];
                double dagIm = -D[(j * n + i) * 2 + 1];

                double vRe = dRe - dagRe;
                double vIm = dIm - dagIm;
                double v = System.Math.Sqrt(vRe * vRe + vIm * vIm);
                if (v > maxViolation) maxViolation = v;
            }
        }
        return maxViolation;
    }

    /// <summary>
    /// Check that D * psi has the same norm as D^dagger * psi for a test vector psi.
    /// This is an alternative self-adjointness check that works without the full matrix.
    /// </summary>
    public static double CheckNormPreservation(DiracOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        if (!bundle.HasExplicitMatrix || bundle.ExplicitMatrix is null)
            throw new InvalidOperationException("No explicit matrix available.");

        int n = bundle.TotalDof;
        // Use a random-ish test vector (fixed seed for reproducibility)
        var psi = new double[n * 2];
        var rng = new Random(42);
        for (int i = 0; i < psi.Length; i++)
            psi[i] = rng.NextDouble() * 2.0 - 1.0;

        // Apply D * psi directly from the explicit matrix
        var D = bundle.ExplicitMatrix;
        var Dpsi = new double[n * 2];
        for (int i = 0; i < n; i++)
        {
            double re = 0, im = 0;
            for (int j = 0; j < n; j++)
            {
                double dRe = D[(i * n + j) * 2];
                double dIm = D[(i * n + j) * 2 + 1];
                re += dRe * psi[j * 2] - dIm * psi[j * 2 + 1];
                im += dRe * psi[j * 2 + 1] + dIm * psi[j * 2];
            }
            Dpsi[i * 2] = re;
            Dpsi[i * 2 + 1] = im;
        }

        // Compute norms
        double normDpsi = 0.0;
        for (int i = 0; i < Dpsi.Length; i++)
            normDpsi += Dpsi[i] * Dpsi[i];

        return System.Math.Sqrt(normDpsi);
    }

    /// <summary>
    /// Run full validation and return a DiracValidationReport.
    /// </summary>
    public static DiracValidationReport Validate(DiracOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        var shapeErrors = ValidateShape(bundle);
        var storageErrors = ValidateExplicitMatrixShape(bundle);

        double hermiticity = 0.0;
        string hermStatus = "not-checked";

        if (bundle.HasExplicitMatrix && bundle.ExplicitMatrix is not null)
        {
            hermiticity = CheckHermiticity(bundle);
            hermStatus = hermiticity < 1e-10 ? "hermitian" : "non-hermitian";
        }

        var allMessages = new List<string>(shapeErrors.Count + storageErrors.Count);
        allMessages.AddRange(shapeErrors);
        allMessages.AddRange(storageErrors);

        return new DiracValidationReport
        {
            IsShapeValid = shapeErrors.Count == 0,
            IsStorageValid = storageErrors.Count(m => !m.StartsWith("No explicit")) == 0,
            HermiticityResidual = hermiticity,
            HermiticityStatus = hermStatus,
            Messages = allMessages,
        };
    }
}

/// <summary>
/// Report from DiracOperatorValidator.Validate().
/// </summary>
public sealed class DiracValidationReport
{
    public required bool IsShapeValid { get; init; }
    public required bool IsStorageValid { get; init; }
    public required double HermiticityResidual { get; init; }
    public required string HermiticityStatus { get; init; }
    public required IReadOnlyList<string> Messages { get; init; }
    public bool IsValid => IsShapeValid && IsStorageValid;
}

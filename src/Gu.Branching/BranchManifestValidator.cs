using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Validates a BranchManifest before a run can start.
/// Ensures all required fields are present and consistent.
/// No run can start without a valid branch manifest.
/// </summary>
public static class BranchManifestValidator
{
    /// <summary>
    /// Validate a branch manifest. Returns a list of validation errors.
    /// An empty list means the manifest is valid.
    /// </summary>
    public static IReadOnlyList<string> Validate(BranchManifest manifest)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(manifest.BranchId))
            errors.Add("BranchId is required.");
        if (string.IsNullOrWhiteSpace(manifest.SchemaVersion))
            errors.Add("SchemaVersion is required.");
        if (string.IsNullOrWhiteSpace(manifest.SourceEquationRevision))
            errors.Add("SourceEquationRevision is required.");
        if (string.IsNullOrWhiteSpace(manifest.CodeRevision))
            errors.Add("CodeRevision is required.");

        // Branch identifiers
        if (string.IsNullOrWhiteSpace(manifest.ActiveGeometryBranch) || manifest.ActiveGeometryBranch == "unset")
            errors.Add("ActiveGeometryBranch must be set to a valid branch identifier.");
        if (string.IsNullOrWhiteSpace(manifest.ActiveObservationBranch) || manifest.ActiveObservationBranch == "unset")
            errors.Add("ActiveObservationBranch must be set to a valid branch identifier.");
        if (string.IsNullOrWhiteSpace(manifest.ActiveTorsionBranch) || manifest.ActiveTorsionBranch == "unset")
            errors.Add("ActiveTorsionBranch must be set to a valid branch identifier.");
        if (string.IsNullOrWhiteSpace(manifest.ActiveShiabBranch) || manifest.ActiveShiabBranch == "unset")
            errors.Add("ActiveShiabBranch must be set to a valid branch identifier.");
        if (string.IsNullOrWhiteSpace(manifest.ActiveGaugeStrategy) || manifest.ActiveGaugeStrategy == "unset")
            errors.Add("ActiveGaugeStrategy must be set.");

        // Dimensions
        if (manifest.BaseDimension <= 0)
            errors.Add("BaseDimension must be positive.");
        if (manifest.AmbientDimension <= 0)
            errors.Add("AmbientDimension must be positive.");
        if (manifest.AmbientDimension <= manifest.BaseDimension)
            errors.Add("AmbientDimension must be greater than BaseDimension.");

        // Convention identifiers
        if (string.IsNullOrWhiteSpace(manifest.LieAlgebraId) || manifest.LieAlgebraId == "unset")
            errors.Add("LieAlgebraId must be set.");
        if (string.IsNullOrWhiteSpace(manifest.BasisConventionId) || manifest.BasisConventionId == "unset")
            errors.Add("BasisConventionId must be set.");
        if (string.IsNullOrWhiteSpace(manifest.ComponentOrderId) || manifest.ComponentOrderId == "unset")
            errors.Add("ComponentOrderId must be set.");
        if (string.IsNullOrWhiteSpace(manifest.AdjointConventionId) || manifest.AdjointConventionId == "unset")
            errors.Add("AdjointConventionId must be set.");
        if (string.IsNullOrWhiteSpace(manifest.PairingConventionId) || manifest.PairingConventionId == "unset")
            errors.Add("PairingConventionId must be set.");
        if (string.IsNullOrWhiteSpace(manifest.NormConventionId) || manifest.NormConventionId == "unset")
            errors.Add("NormConventionId must be set.");
        if (string.IsNullOrWhiteSpace(manifest.DifferentialFormMetricId) || manifest.DifferentialFormMetricId == "unset")
            errors.Add("DifferentialFormMetricId must be set (FIX-M1-3).");

        // Inserted assumptions and choices
        if (manifest.InsertedAssumptionIds == null)
            errors.Add("InsertedAssumptionIds must not be null.");
        if (manifest.InsertedChoiceIds == null)
            errors.Add("InsertedChoiceIds must not be null.");

        return errors;
    }

    /// <summary>
    /// Validate and throw if the manifest is invalid.
    /// This is the gatekeeper: no run can start without a valid manifest.
    /// </summary>
    public static void ValidateOrThrow(BranchManifest manifest)
    {
        var errors = Validate(manifest);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Branch manifest '{manifest.BranchId}' is invalid. " +
                $"Errors: {string.Join("; ", errors)}");
        }
    }
}

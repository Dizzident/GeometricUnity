namespace Gu.Core.Factories;

/// <summary>
/// Factory for creating default and empty branch manifests.
/// </summary>
public static class BranchManifestFactory
{
    /// <summary>Current schema version for branch manifests.</summary>
    public const string CurrentSchemaVersion = "1.0.0";

    /// <summary>
    /// Creates an empty branch manifest with minimal defaults for the active bosonic branch.
    /// </summary>
    public static BranchManifest CreateEmpty(string branchId = "empty-branch")
    {
        return new BranchManifest
        {
            BranchId = branchId,
            SchemaVersion = CurrentSchemaVersion,
            SourceEquationRevision = "unset",
            CodeRevision = "unset",
            ActiveGeometryBranch = "unset",
            ActiveObservationBranch = "unset",
            ActiveTorsionBranch = "unset",
            ActiveShiabBranch = "unset",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "unset",
            BasisConventionId = "unset",
            ComponentOrderId = "unset",
            AdjointConventionId = "unset",
            PairingConventionId = "unset",
            NormConventionId = "unset",
            DifferentialFormMetricId = "unset",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
            Parameters = new Dictionary<string, string>(),
        };
    }
}

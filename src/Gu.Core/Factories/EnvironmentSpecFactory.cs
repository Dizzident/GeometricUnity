namespace Gu.Core.Factories;

/// <summary>
/// Factory for creating default and empty environment specifications.
/// </summary>
public static class EnvironmentSpecFactory
{
    /// <summary>
    /// Creates an empty environment spec with minimal defaults.
    /// </summary>
    public static EnvironmentSpec CreateEmpty(string environmentId = "empty-environment", string branchId = "empty-branch")
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4, Label = "base_X_h" };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14, Label = "ambient_Y_h" };

        return new EnvironmentSpec
        {
            EnvironmentId = environmentId,
            Branch = new BranchRef
            {
                BranchId = branchId,
                SchemaVersion = BranchManifestFactory.CurrentSchemaVersion,
            },
            ScenarioType = "toy-consistency",
            Geometry = new GeometryContext
            {
                BaseSpace = baseSpace,
                AmbientSpace = ambientSpace,
                DiscretizationType = "simplicial",
                QuadratureRuleId = "unset",
                BasisFamilyId = "unset",
                ProjectionBinding = new GeometryBinding
                {
                    BindingType = "projection",
                    SourceSpace = ambientSpace,
                    TargetSpace = baseSpace,
                },
                ObservationBinding = new GeometryBinding
                {
                    BindingType = "observation",
                    SourceSpace = baseSpace,
                    TargetSpace = ambientSpace,
                },
                Patches = Array.Empty<PatchInfo>(),
            },
            BoundaryConditions = new BoundaryConditionBundle
            {
                ConditionType = "unset",
            },
            InitialConditions = new InitialConditionBundle
            {
                ConditionType = "flat-connection",
            },
            GaugeConditions = new GaugeConditionBundle
            {
                StrategyType = "penalty",
                PenaltyCoefficient = 1.0,
            },
            ObservableRequests = Array.Empty<ObservableRequest>(),
            ComparisonTemplateIds = Array.Empty<string>(),
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "unset",
                Branch = new BranchRef
                {
                    BranchId = branchId,
                    SchemaVersion = BranchManifestFactory.CurrentSchemaVersion,
                },
            },
        };
    }
}

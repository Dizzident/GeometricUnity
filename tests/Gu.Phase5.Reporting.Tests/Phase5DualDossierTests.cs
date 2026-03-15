using Gu.Artifacts;
using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

/// <summary>
/// WP-5: Tests that Phase5CampaignRunner.RunFull emits both dossier types.
/// </summary>
public sealed class Phase5DualDossierTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "wp5-test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static Phase5CampaignSpec MakeSpec() => new Phase5CampaignSpec
    {
        CampaignId = "wp5-test-campaign",
        SchemaVersion = "1.0.0",
        BranchFamilySpec = new BranchRobustnessStudySpec
        {
            StudyId = "branch-study",
            BranchVariantIds = ["V1", "V2", "V3"],
            TargetQuantityIds = ["q1"],
        },
        RefinementSpec = new RefinementStudySpec
        {
            StudyId = "refine-study",
            SchemaVersion = "1.0",
            BranchManifestId = "branch-1",
            TargetQuantities = ["q1"],
            RefinementLevels =
            [
                new RefinementLevel { LevelId = "L0", MeshParameterX = 1.0, MeshParameterF = 1.0 },
                new RefinementLevel { LevelId = "L1", MeshParameterX = 0.5, MeshParameterF = 0.5 },
                new RefinementLevel { LevelId = "L2", MeshParameterX = 0.25, MeshParameterF = 0.25 },
            ],
            Provenance = MakeProvenance(),
        },
        EnvironmentCampaignSpec = new EnvironmentCampaignSpec
        {
            CampaignId = "env-campaign",
            SchemaVersion = "1.0",
            EnvironmentIds = ["env-toy-2d", "env-structured-4x4"],
            BranchManifestId = "branch-1",
            TargetQuantities = ["q1"],
            Provenance = MakeProvenance(),
        },
        ExternalTargetTablePath = "targets.json",
        BranchQuantityValuesPath = "config/branch_quantity_values.json",
        RefinementValuesPath = "config/refinement_values.json",
        ObservablesPath = "config/observables.json",
        EnvironmentRecordPaths = ["artifacts/environments/env-toy.json"],
        RegistryPath = "artifacts/registry/unified_registry.json",
        CalibrationPolicy = new CalibrationPolicy
        {
            PolicyId = "test-policy",
            Mode = "sigma",
            SigmaThreshold = 5.0,
        },
        FalsificationPolicy = new FalsificationPolicy(),
        Provenance = MakeProvenance(),
    };

    private static IReadOnlyDictionary<string, double[]> BranchExecutor(string variantId) =>
        new Dictionary<string, double[]> { ["q1"] = [1.0 + variantId.GetHashCode() % 3 * 0.001] };

    private static IReadOnlyDictionary<string, double> RefinementExecutor(RefinementLevel level) =>
        new Dictionary<string, double> { ["q1"] = 2.0 + level.EffectiveMeshParameter * level.EffectiveMeshParameter };

    [Fact]
    public void RunFull_BothDossierTypesPresent()
    {
        // Both typed dossier and provenance dossier must be non-null after RunFull
        var spec = MakeSpec();
        var runner = new Phase5CampaignRunner();
        var observables = new List<QuantitativeObservableRecord>();
        var targetTable = new ExternalTargetTable { TableId = "t1", Targets = [] };

        var result = runner.RunFull(spec, BranchExecutor, RefinementExecutor, observables, targetTable);

        Assert.NotNull(result);
        Assert.NotNull(result.TypedDossier);
        Assert.NotNull(result.ProvenanceDossier);
        Assert.NotNull(result.StudyManifests);
        Assert.NotNull(result.Report);

        // Both dossier types carry distinct IDs
        Assert.NotEqual(result.TypedDossier.DossierId, result.ProvenanceDossier.DossierId);
    }

    [Fact]
    public void RunFull_ProvenanceDossier_AcceptableWhenManifestsHaveReproducibility()
    {
        // ValidationDossier (provenance gate) must be acceptable when manifests carry
        // regenerated reproducibility bundles (as created by the runner).
        var spec = MakeSpec();
        var runner = new Phase5CampaignRunner();
        var targetTable = new ExternalTargetTable { TableId = "t1", Targets = [] };

        var result = runner.RunFull(spec, BranchExecutor, RefinementExecutor, [], targetTable);

        // Study manifests must carry reproducibility metadata
        Assert.Equal(2, result.StudyManifests.Count);
        Assert.All(result.StudyManifests, m =>
        {
            Assert.NotNull(m.Reproducibility);
            Assert.Equal(ArtifactEvidenceTier.RegeneratedCpu, m.EffectiveEvidenceTier);
        });

        // Provenance dossier must be acceptable because all manifests are regenerated
        Assert.True(result.ProvenanceDossier.IsAcceptableAsEvidence);
        Assert.Empty(result.ProvenanceDossier.StaleStudyIds);
    }

    [Fact]
    public void RunFull_TypedDossier_ContainsBranchAndFalsifierContent()
    {
        // Phase5ValidationDossier must contain branch summary and falsifier summary.
        var spec = MakeSpec();
        var runner = new Phase5CampaignRunner();

        // Use a fragile branch to ensure falsifiers are triggered
        IReadOnlyDictionary<string, double[]> fragileBranch(string variantId) =>
            variantId switch
            {
                "V1" => new Dictionary<string, double[]> { ["q1"] = [1.0] },
                "V2" => new Dictionary<string, double[]> { ["q1"] = [500.0] },
                _    => new Dictionary<string, double[]> { ["q1"] = [0.001] },
            };

        var targetTable = new ExternalTargetTable { TableId = "t1", Targets = [] };

        var result = runner.RunFull(spec, fragileBranch, RefinementExecutor, [], targetTable);

        // Typed dossier must carry branch family summary
        Assert.NotNull(result.TypedDossier.BranchFamilySummary);

        // Typed dossier must carry refinement summary
        Assert.NotNull(result.TypedDossier.RefinementSummary);

        // Typed dossier must carry falsifier summary
        Assert.NotNull(result.TypedDossier.FalsifierSummary);
    }
}

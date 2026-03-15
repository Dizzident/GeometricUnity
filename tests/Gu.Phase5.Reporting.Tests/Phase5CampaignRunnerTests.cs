using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase5CampaignRunnerTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static Phase5CampaignSpec MakeSpec() => new Phase5CampaignSpec
    {
        CampaignId = "test-campaign",
        SchemaVersion = "1.0.0",
        BranchFamilySpec = new BranchRobustnessStudySpec
        {
            StudyId = "branch-study",
            BranchVariantIds = ["V1", "V2", "V3"],
            TargetQuantityIds = ["q1", "q2"],
        },
        RefinementSpec = new RefinementStudySpec
        {
            StudyId = "refine-study",
            SchemaVersion = "1.0",
            BranchManifestId = "branch-1",
            TargetQuantities = ["q1"],
            RefinementLevels = [
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
            EnvironmentIds = ["env-toy"],
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

    [Fact]
    public void Run_SyntheticData_ProducesPhase5Report()
    {
        var spec = MakeSpec();
        var runner = new Phase5CampaignRunner();

        // Synthetic branch pipeline: each variant has slightly different q1 and q2
        IReadOnlyDictionary<string, double[]> branchExecutor(string variantId) =>
            variantId switch
            {
                "V1" => new Dictionary<string, double[]> { ["q1"] = [1.0], ["q2"] = [5.0] },
                "V2" => new Dictionary<string, double[]> { ["q1"] = [1.005], ["q2"] = [5.02] },
                _ => new Dictionary<string, double[]> { ["q1"] = [0.998], ["q2"] = [4.99] },
            };

        // Synthetic refinement: second-order convergent
        IReadOnlyDictionary<string, double> refinementExecutor(RefinementLevel level) =>
            new Dictionary<string, double> { ["q1"] = 2.0 + level.EffectiveMeshParameter * level.EffectiveMeshParameter };

        // Observables and target table
        var observables = new List<QuantitativeObservableRecord>
        {
            new QuantitativeObservableRecord
            {
                ObservableId = "boson-eigenvalue-ratio-1",
                Value = 0.1,
                Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.02, BranchVariation = 0.01, RefinementError = 0.01 },
                BranchId = "V1",
                EnvironmentId = "env-toy",
                ExtractionMethod = "eigenvalue-ratio",
                Provenance = MakeProvenance(),
            },
        };

        var targetTable = new ExternalTargetTable
        {
            TableId = "test-targets",
            Targets =
            [
                new ExternalTarget
                {
                    ObservableId = "boson-eigenvalue-ratio-1",
                    Label = "light-boson",
                    Value = 0.1,
                    Uncertainty = 0.05,
                    Source = "synthetic-toy-v1",
                },
            ],
        };

        var report = runner.Run(spec, branchExecutor, refinementExecutor, observables, targetTable);

        Assert.NotNull(report);
        Assert.Equal("test-campaign", report.StudyId);
        Assert.NotEmpty(report.DossierIds);
        Assert.NotNull(report.BranchIndependenceAtlas);
        Assert.NotNull(report.ConvergenceAtlas);
        Assert.Equal(1, report.ConvergenceAtlas!.ConvergentCount);
    }

    [Fact]
    public void Run_ThrowsOnNullSpec()
    {
        var runner = new Phase5CampaignRunner();
        Assert.Throws<ArgumentNullException>(() =>
            runner.Run(null!, _ => new Dictionary<string, double[]>(),
                _ => new Dictionary<string, double>(), [], new ExternalTargetTable
                {
                    TableId = "t", Targets = [],
                }));
    }

    [Fact]
    public void Run_FragileBranch_ProducesFalsificationDashboard()
    {
        var spec = MakeSpec();
        var runner = new Phase5CampaignRunner();

        // q1 is very fragile across variants
        IReadOnlyDictionary<string, double[]> branchExecutor(string variantId) =>
            variantId switch
            {
                "V1" => new Dictionary<string, double[]> { ["q1"] = [1.0], ["q2"] = [5.0] },
                "V2" => new Dictionary<string, double[]> { ["q1"] = [100.0], ["q2"] = [5.0] },
                _ => new Dictionary<string, double[]> { ["q1"] = [0.001], ["q2"] = [5.0] },
            };

        IReadOnlyDictionary<string, double> refinementExecutor(RefinementLevel level) =>
            new Dictionary<string, double> { ["q1"] = 3.0 + level.EffectiveMeshParameter };

        var targetTable = new ExternalTargetTable
        {
            TableId = "empty-targets",
            Targets = [],
        };

        var report = runner.Run(spec, branchExecutor, refinementExecutor, [], targetTable);

        Assert.NotNull(report);
        Assert.NotNull(report.FalsificationDashboard);
        // With fragile branch, we should have at least 1 active falsifier (high severity)
        Assert.True(report.FalsificationDashboard!.DemotionCount >= 1 ||
                    report.FalsificationDashboard.TotalFalsifiers >= 1);
    }
}

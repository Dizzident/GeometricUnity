using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison.Tests;

public sealed class EndToEndCampaignTests
{
    private static ComparisonAsset MakeAsset(string id = "asset-001") => new()
    {
        AssetId = id,
        SourceCitation = "Simulated benchmark data",
        AcquisitionDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        PreprocessingDescription = "Normalized to unit scale",
        AdmissibleUseStatement = "Benchmark comparison only",
        DomainOfValidity = "Flat 3D, weak field",
        UncertaintyModel = new UncertaintyRecord
        {
            Discretization = 0.01,
            Solver = 0.005,
            Branch = 0.02,
            Extraction = 0.01,
            Calibration = 0.0,
            DataAsset = 0.015,
        },
        ComparisonVariables = new Dictionary<string, string>
        {
            ["energy-density"] = "Volume-averaged energy density",
            ["curvature-norm"] = "L2 norm of curvature",
        },
    };

    private static PredictionTestRecord MakeRecord(
        string testId = "test-001",
        ClaimClass claimClass = ClaimClass.ExactStructuralConsequence,
        string numericalStatus = "converged",
        string approximation = "exact",
        ComparisonAsset? asset = null) => new()
    {
        TestId = testId,
        ClaimClass = claimClass,
        FormalSource = "GU Field Equation linearization",
        BranchManifestId = "manifest-standard-branch",
        ObservableMapId = "obs-energy-curvature",
        TheoremDependencyStatus = "closed",
        NumericalDependencyStatus = numericalStatus,
        ApproximationStatus = approximation,
        ExternalComparisonAsset = asset ?? MakeAsset(),
        Falsifier = "Energy density deviates from benchmark by more than 5 sigma",
        ArtifactLinks = ["artifact-run-001", "artifact-extraction-001"],
    };

    private static ComparisonCampaignSpec MakeSpec(
        ComparisonMode mode,
        string campaignId = "e2e-campaign") => new()
    {
        CampaignId = campaignId,
        EnvironmentIds = ["env-flat-3d"],
        BranchSubsetIds = ["branch-standard"],
        ObservedOutputClassIds = ["energy-density", "curvature-norm"],
        ComparisonAssetIds = ["asset-001"],
        Mode = mode,
        CalibrationPolicy = "fixed",
    };

    private static InMemoryDatasetAdapter MakeAdapterWithGoodData()
    {
        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(MakeAsset(), new Dictionary<string, double[]>
        {
            ["energy-density"] = [1.0, 1.005, 0.998, 1.002],
            ["curvature-norm"] = [1.0, 0.999, 1.001],
        });
        return adapter;
    }

    private static InMemoryDatasetAdapter MakeAdapterWithBadData()
    {
        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(MakeAsset(), new Dictionary<string, double[]>
        {
            ["energy-density"] = [50.0, 60.0, 70.0],
            ["curvature-norm"] = [100.0, 200.0],
        });
        return adapter;
    }

    // --- Structural end-to-end ---

    [Fact]
    public void Structural_EndToEnd_AllPass()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithGoodData();
        var predictions = new List<PredictionTestRecord>
        {
            MakeRecord("pred-001"),
            MakeRecord("pred-002", ClaimClass.ApproximateStructuralSurrogate),
        };

        var result = runner.RunWithStrategy(MakeSpec(ComparisonMode.Structural), predictions, adapter);

        Assert.Equal(2, result.CampaignResult.RunRecords.Count);
        Assert.All(result.CampaignResult.RunRecords, r =>
        {
            Assert.Equal(ComparisonMode.Structural, r.Mode);
            Assert.True(r.Score > 0);
        });
        Assert.Equal("e2e-campaign", result.CampaignResult.CampaignId);
    }

    // --- SemiQuantitative end-to-end ---

    [Fact]
    public void SemiQuantitative_EndToEnd_GoodData_Passes()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithGoodData();

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.SemiQuantitative),
            [MakeRecord("pred-001")],
            adapter);

        Assert.Single(result.CampaignResult.RunRecords);
        var run = result.CampaignResult.RunRecords[0];
        Assert.Equal(ComparisonMode.SemiQuantitative, run.Mode);
        Assert.True(run.Score > 0.9);
    }

    [Fact]
    public void SemiQuantitative_EndToEnd_BadData_Fails_NegativeArtifact()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithBadData();

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.SemiQuantitative),
            [MakeRecord("pred-001")],
            adapter);

        // RunRecord is still emitted (score computed), but failure also emitted
        Assert.Single(result.CampaignResult.RunRecords);
        Assert.NotEmpty(result.CampaignResult.Failures);
        Assert.NotEmpty(result.NegativeArtifacts);

        var artifact = result.NegativeArtifacts[0];
        Assert.Equal("pred-001", artifact.OriginalTestId);
        Assert.Equal(ComparisonMode.SemiQuantitative, artifact.AttemptedMode);
        Assert.Equal("manifest-standard-branch", artifact.BranchManifestId);
    }

    // --- Quantitative end-to-end ---

    [Fact]
    public void Quantitative_EndToEnd_GoodData_Passes()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithGoodData();

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord("pred-001")],
            adapter);

        Assert.Single(result.CampaignResult.RunRecords);
        var run = result.CampaignResult.RunRecords[0];
        Assert.Equal(ComparisonMode.Quantitative, run.Mode);
        Assert.True(run.Score > 0.9);

        // No negative artifacts for passing comparison
        Assert.Empty(result.NegativeArtifacts);
    }

    [Fact]
    public void Quantitative_EndToEnd_BadData_EmitsNegativeArtifact()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithBadData();

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord("pred-001")],
            adapter);

        Assert.Single(result.CampaignResult.RunRecords);
        Assert.NotEmpty(result.NegativeArtifacts);

        var artifact = result.NegativeArtifacts[0];
        Assert.Equal("e2e-campaign", artifact.CampaignId);
        Assert.False(string.IsNullOrEmpty(artifact.Failure.FailureReason));
        Assert.Equal("empirical", artifact.Failure.FailureLevel);
    }

    // --- Mixed predictions ---

    [Fact]
    public void Mixed_EndToEnd_GoodAndBadPredictions()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithGoodData();

        var predictions = new List<PredictionTestRecord>
        {
            MakeRecord("good-pred"),
            MakeRecord("bad-pred", numericalStatus: "exploratory"),
            MakeRecord("inadmissible-pred"),
        };
        // Make inadmissible-pred actually inadmissible by removing falsifier
        predictions[2] = new PredictionTestRecord
        {
            TestId = "inadmissible-pred",
            ClaimClass = ClaimClass.ExactStructuralConsequence,
            FormalSource = "Theorem",
            BranchManifestId = "manifest-001",
            ObservableMapId = "obs-001",
            TheoremDependencyStatus = "closed",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "exact",
            ExternalComparisonAsset = MakeAsset(),
            Falsifier = "", // missing!
            ArtifactLinks = ["art-001"],
        };

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.Quantitative),
            predictions,
            adapter);

        // good-pred passes quantitative, bad-pred fails mode guard (exploratory),
        // inadmissible-pred fails validation
        Assert.Single(result.CampaignResult.RunRecords);
        Assert.Equal("good-pred", result.CampaignResult.RunRecords[0].TestId);
        Assert.True(result.CampaignResult.Failures.Count >= 2);
        Assert.True(result.NegativeArtifacts.Count >= 2);
    }

    // --- Uncertainty awareness ---

    [Fact]
    public void EndToEnd_UncertaintyAware_AllRecordsHaveUncertainty()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithGoodData();

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord("pred-001")],
            adapter);

        foreach (var run in result.CampaignResult.RunRecords)
        {
            Assert.NotNull(run.Uncertainty);
            // At least some components should be estimated
            Assert.True(UncertaintyDecomposer.TotalUncertainty(run.Uncertainty) > 0);
        }
    }

    // --- Branch traceability ---

    [Fact]
    public void EndToEnd_BranchTraceable_NegativeArtifactsCarryManifestId()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithBadData();

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord("pred-001")],
            adapter);

        foreach (var artifact in result.NegativeArtifacts)
        {
            Assert.False(string.IsNullOrEmpty(artifact.BranchManifestId));
            Assert.False(string.IsNullOrEmpty(artifact.CampaignId));
        }
    }

    // --- Strategy override ---

    [Fact]
    public void RunWithStrategy_OverridesDefaultStrategy()
    {
        var runner = new CampaignRunner();
        var adapter = MakeAdapterWithGoodData();

        // Use structural strategy even though spec says Quantitative
        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord("pred-001")],
            adapter,
            strategyOverride: new StructuralComparisonStrategy());

        Assert.Single(result.CampaignResult.RunRecords);
        // Mode in the run record comes from the spec, not the strategy
        Assert.Equal(ComparisonMode.Quantitative, result.CampaignResult.RunRecords[0].Mode);
    }

    // --- Null argument checks for RunWithStrategy ---

    [Fact]
    public void RunWithStrategy_NullSpec_Throws()
    {
        var runner = new CampaignRunner();
        Assert.Throws<ArgumentNullException>(() =>
            runner.RunWithStrategy(null!, [], new InMemoryDatasetAdapter()));
    }

    [Fact]
    public void RunWithStrategy_NullPredictions_Throws()
    {
        var runner = new CampaignRunner();
        Assert.Throws<ArgumentNullException>(() =>
            runner.RunWithStrategy(MakeSpec(ComparisonMode.Structural), null!, new InMemoryDatasetAdapter()));
    }

    [Fact]
    public void RunWithStrategy_NullAdapter_Throws()
    {
        var runner = new CampaignRunner();
        Assert.Throws<ArgumentNullException>(() =>
            runner.RunWithStrategy(MakeSpec(ComparisonMode.Structural), [], null!));
    }

    // --- Constructor-injected strategies ---

    [Fact]
    public void ConstructorInjectedStrategy_UsedInsteadOfFactory()
    {
        var strategies = new Dictionary<ComparisonMode, IComparisonStrategy>
        {
            [ComparisonMode.Quantitative] = new StructuralComparisonStrategy(),
        };
        var runner = new CampaignRunner(strategies);
        var adapter = MakeAdapterWithGoodData();

        var result = runner.RunWithStrategy(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord("pred-001")],
            adapter);

        // Structural strategy was used but mode is overridden to Quantitative
        Assert.Single(result.CampaignResult.RunRecords);
        Assert.Equal(ComparisonMode.Quantitative, result.CampaignResult.RunRecords[0].Mode);
        // Structural passes with score 1.0
        Assert.True(result.CampaignResult.RunRecords[0].Passed);
    }
}

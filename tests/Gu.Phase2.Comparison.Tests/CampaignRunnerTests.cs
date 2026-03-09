using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison.Tests;

public sealed class CampaignRunnerTests
{
    private static ComparisonAsset MakeAsset(string id = "asset-001") => new()
    {
        AssetId = id,
        SourceCitation = "Test citation",
        AcquisitionDate = DateTimeOffset.UtcNow,
        PreprocessingDescription = "None",
        AdmissibleUseStatement = "Any",
        DomainOfValidity = "All",
        UncertaintyModel = UncertaintyRecord.Unestimated(),
        ComparisonVariables = new Dictionary<string, string> { ["energy"] = "Total energy" },
    };

    private static PredictionTestRecord MakeRecord(
        string testId = "test-001",
        ClaimClass claimClass = ClaimClass.ExactStructuralConsequence,
        string theoremStatus = "closed",
        string numericalStatus = "converged",
        string approximation = "exact",
        string branchManifestId = "manifest-001",
        string falsifier = "Observable diverges by >5 sigma",
        ComparisonAsset? asset = null,
        bool includeAsset = true) => new()
    {
        TestId = testId,
        ClaimClass = claimClass,
        FormalSource = "Theorem 3.1",
        BranchManifestId = branchManifestId,
        ObservableMapId = "obs-map-001",
        TheoremDependencyStatus = theoremStatus,
        NumericalDependencyStatus = numericalStatus,
        ApproximationStatus = approximation,
        ExternalComparisonAsset = includeAsset ? (asset ?? MakeAsset()) : null,
        Falsifier = falsifier,
        ArtifactLinks = ["artifact-001"],
    };

    private static ComparisonCampaignSpec MakeSpec(
        ComparisonMode mode = ComparisonMode.Structural,
        string campaignId = "campaign-001") => new()
    {
        CampaignId = campaignId,
        EnvironmentIds = ["env-001"],
        BranchSubsetIds = ["branch-001"],
        ObservedOutputClassIds = ["output-001"],
        ComparisonAssetIds = ["asset-001"],
        Mode = mode,
        CalibrationPolicy = "fixed",
    };

    private static IReadOnlyDictionary<string, ComparisonAsset> MakeAssets()
    {
        var a = MakeAsset();
        return new Dictionary<string, ComparisonAsset> { [a.AssetId] = a };
    }

    // --- Structural mode (permissive) ---

    [Fact]
    public void Structural_ValidPrediction_ProducesRunRecord()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.Structural),
            [MakeRecord()],
            MakeAssets());

        Assert.Single(result.RunRecords);
        Assert.Empty(result.Failures);
        Assert.Equal("campaign-001", result.CampaignId);
        Assert.Equal("test-001", result.RunRecords[0].TestId);
        Assert.Equal(ComparisonMode.Structural, result.RunRecords[0].Mode);
    }

    [Fact]
    public void Structural_MissingFalsifier_SkippedAsInadmissible()
    {
        var runner = new CampaignRunner();
        var record = MakeRecord(falsifier: "");
        var result = runner.Run(MakeSpec(), [record], MakeAssets());

        Assert.Empty(result.RunRecords);
        Assert.Single(result.Failures);
        Assert.Contains("Rule 3", result.Failures[0].FailureReason);
        Assert.Equal(ClaimClass.Inadmissible, result.Failures[0].DemotedClaimClass);
    }

    [Fact]
    public void Structural_MissingBranchManifest_SkippedAsInadmissible()
    {
        var runner = new CampaignRunner();
        var record = MakeRecord(branchManifestId: "");
        var result = runner.Run(MakeSpec(), [record], MakeAssets());

        Assert.Empty(result.RunRecords);
        Assert.Single(result.Failures);
        Assert.Contains("Rule 4", result.Failures[0].FailureReason);
    }

    // --- Quantitative mode guards ---

    [Fact]
    public void Quantitative_ValidRecord_Succeeds()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord()],
            MakeAssets());

        Assert.Single(result.RunRecords);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void Quantitative_MissingManifest_Rejected()
    {
        var runner = new CampaignRunner();
        // Record has a falsifier but PredictionValidator marks it Inadmissible for missing manifest
        var record = MakeRecord(branchManifestId: "");
        var result = runner.Run(
            MakeSpec(ComparisonMode.Quantitative),
            [record],
            MakeAssets());

        Assert.Empty(result.RunRecords);
        Assert.Single(result.Failures);
    }

    [Fact]
    public void Quantitative_SurrogateApproximation_Rejected()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord(approximation: "surrogate")],
            MakeAssets());

        Assert.Empty(result.RunRecords);
        Assert.Single(result.Failures);
        Assert.Contains("surrogate", result.Failures[0].FailureReason);
    }

    [Fact]
    public void Quantitative_ExploratoryNumerical_Rejected()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord(numericalStatus: "exploratory")],
            MakeAssets());

        Assert.Empty(result.RunRecords);
        Assert.Single(result.Failures);
        Assert.Contains("converged", result.Failures[0].FailureReason);
    }

    [Fact]
    public void Quantitative_MissingExternalAsset_Rejected()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.Quantitative),
            [MakeRecord(includeAsset: false)],
            MakeAssets());

        Assert.Empty(result.RunRecords);
        Assert.Single(result.Failures);
        Assert.Contains("uncertainty decomposition", result.Failures[0].FailureReason);
    }

    // --- SemiQuantitative mode guards ---

    [Fact]
    public void SemiQuantitative_ValidRecord_Succeeds()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.SemiQuantitative),
            [MakeRecord()],
            MakeAssets());

        Assert.Single(result.RunRecords);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void SemiQuantitative_FailedNumerical_Rejected()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.SemiQuantitative),
            [MakeRecord(numericalStatus: "failed")],
            MakeAssets());

        Assert.Empty(result.RunRecords);
        // Failures may come from both PredictionValidator (Rule 5) and mode guard
        Assert.NotEmpty(result.Failures);
    }

    [Fact]
    public void SemiQuantitative_ExploratoryNumerical_Allowed()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(
            MakeSpec(ComparisonMode.SemiQuantitative),
            [MakeRecord(numericalStatus: "exploratory")],
            MakeAssets());

        // SemiQuantitative allows exploratory (only rejects "failed")
        // But PredictionValidator Rule 5 demotes to PostdictionTarget -- record still passes mode guard
        Assert.Single(result.RunRecords);
    }

    // --- Multiple predictions ---

    [Fact]
    public void MultiplePredictions_MixedResults()
    {
        var runner = new CampaignRunner();
        var predictions = new List<PredictionTestRecord>
        {
            MakeRecord(testId: "good-1"),
            MakeRecord(testId: "bad-1", falsifier: ""),
            MakeRecord(testId: "good-2", claimClass: ClaimClass.ApproximateStructuralSurrogate),
            MakeRecord(testId: "bad-2", branchManifestId: ""),
        };

        var result = runner.Run(MakeSpec(), predictions, MakeAssets());

        Assert.Equal(2, result.RunRecords.Count);
        Assert.Equal(2, result.Failures.Count);
        Assert.Contains(result.RunRecords, r => r.TestId == "good-1");
        Assert.Contains(result.RunRecords, r => r.TestId == "good-2");
        Assert.Contains(result.Failures, f => f.TestId == "bad-1");
        Assert.Contains(result.Failures, f => f.TestId == "bad-2");
    }

    // --- Dry-run campaign (completion criteria) ---

    [Fact]
    public void DryRunCampaign_SyntheticData_FullCycle()
    {
        var asset = MakeAsset("benchmark-asset");
        var predictions = new List<PredictionTestRecord>
        {
            MakeRecord(
                testId: "pred-exact-001",
                claimClass: ClaimClass.ExactStructuralConsequence,
                asset: asset),
            MakeRecord(
                testId: "pred-approx-001",
                claimClass: ClaimClass.ApproximateStructuralSurrogate,
                asset: asset),
            MakeRecord(
                testId: "pred-postdiction-001",
                claimClass: ClaimClass.PostdictionTarget,
                asset: asset),
        };

        var spec = new ComparisonCampaignSpec
        {
            CampaignId = "dry-run-campaign",
            EnvironmentIds = ["env-flat-3d"],
            BranchSubsetIds = ["branch-standard"],
            ObservedOutputClassIds = ["energy-density"],
            ComparisonAssetIds = [asset.AssetId],
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        var assets = new Dictionary<string, ComparisonAsset>
        {
            [asset.AssetId] = asset,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(spec, predictions, assets);

        // All 3 should succeed in Structural mode
        Assert.Equal(3, result.RunRecords.Count);
        Assert.Empty(result.Failures);
        Assert.Equal("dry-run-campaign", result.CampaignId);
        Assert.True(result.CompletedAt <= DateTimeOffset.UtcNow);

        // Every comparison record has explicit claim class and uncertainty
        foreach (var run in result.RunRecords)
        {
            Assert.True(Enum.IsDefined(run.ResolvedClaimClass));
            Assert.NotNull(run.Uncertainty);
            Assert.Equal(ComparisonMode.Structural, run.Mode);
        }
    }

    [Fact]
    public void DryRunCampaign_QuantitativeMode_GatesEnforced()
    {
        var asset = MakeAsset("benchmark-asset");
        var predictions = new List<PredictionTestRecord>
        {
            MakeRecord(testId: "exact-converged", asset: asset),
            MakeRecord(testId: "surrogate-bad", approximation: "surrogate", asset: asset),
            MakeRecord(testId: "exploratory-bad", numericalStatus: "exploratory", asset: asset),
            MakeRecord(testId: "no-asset", includeAsset: false),
        };

        var spec = MakeSpec(ComparisonMode.Quantitative);
        var runner = new CampaignRunner();
        var result = runner.Run(spec, predictions, MakeAssets());

        // Only the first should pass quantitative gates
        Assert.Single(result.RunRecords);
        Assert.Equal("exact-converged", result.RunRecords[0].TestId);
        Assert.Equal(3, result.Failures.Count);
    }

    // --- Null argument checks ---

    [Fact]
    public void Run_NullSpec_Throws()
    {
        var runner = new CampaignRunner();
        Assert.Throws<ArgumentNullException>(() =>
            runner.Run(null!, [], MakeAssets()));
    }

    [Fact]
    public void Run_NullPredictions_Throws()
    {
        var runner = new CampaignRunner();
        Assert.Throws<ArgumentNullException>(() =>
            runner.Run(MakeSpec(), null!, MakeAssets()));
    }

    [Fact]
    public void Run_NullAssets_Throws()
    {
        var runner = new CampaignRunner();
        Assert.Throws<ArgumentNullException>(() =>
            runner.Run(MakeSpec(), [], null!));
    }

    // --- Edge cases ---

    [Fact]
    public void EmptyPredictions_ProducesEmptyResult()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(MakeSpec(), [], MakeAssets());

        Assert.Empty(result.RunRecords);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void RunRecord_HasUncertaintyFromAsset()
    {
        var asset = new ComparisonAsset
        {
            AssetId = "asset-with-uncertainty",
            SourceCitation = "Citation",
            AcquisitionDate = DateTimeOffset.UtcNow,
            PreprocessingDescription = "None",
            AdmissibleUseStatement = "Any",
            DomainOfValidity = "All",
            UncertaintyModel = new UncertaintyRecord
            {
                Discretization = 0.01,
                Solver = 0.02,
                Branch = 0.03,
                Extraction = 0.04,
                Calibration = 0.05,
                DataAsset = 0.06,
            },
            ComparisonVariables = new Dictionary<string, string> { ["x"] = "Position" },
        };

        var record = MakeRecord(asset: asset);
        var runner = new CampaignRunner();
        var result = runner.Run(MakeSpec(), [record], MakeAssets());

        Assert.Single(result.RunRecords);
        var u = result.RunRecords[0].Uncertainty;
        Assert.Equal(0.01, u.Discretization);
        Assert.Equal(0.06, u.DataAsset);
    }

    [Fact]
    public void ComparisonCampaignResult_CalibrationIsOptional()
    {
        var runner = new CampaignRunner();
        var result = runner.Run(MakeSpec(), [MakeRecord()], MakeAssets());

        Assert.Null(result.Calibration);
    }

    [Fact]
    public void CalibrationRecord_Construction()
    {
        var cal = new CalibrationRecord
        {
            CalibrationId = "cal-001",
            Policy = "fixed",
            Parameters = new Dictionary<string, double> { ["alpha"] = 1.0 / 137.0 },
            FitMethod = "a-priori",
        };

        Assert.Equal("cal-001", cal.CalibrationId);
        Assert.Equal("fixed", cal.Policy);
        Assert.Single(cal.Parameters);
        Assert.Equal("a-priori", cal.FitMethod);
    }

    [Fact]
    public void ComparisonCampaignSpec_Construction()
    {
        var spec = MakeSpec(ComparisonMode.Quantitative, "my-campaign");

        Assert.Equal("my-campaign", spec.CampaignId);
        Assert.Equal(ComparisonMode.Quantitative, spec.Mode);
        Assert.Equal("fixed", spec.CalibrationPolicy);
        Assert.Single(spec.EnvironmentIds);
    }

    [Fact]
    public void FailureRecord_PreservesAllFields()
    {
        var runner = new CampaignRunner();
        var record = MakeRecord(falsifier: "");
        var result = runner.Run(MakeSpec(), [record], MakeAssets());

        var failure = Assert.Single(result.Failures);
        Assert.Equal("test-001", failure.TestId);
        Assert.False(string.IsNullOrEmpty(failure.FailureReason));
        Assert.Equal("extraction", failure.FailureLevel);
        Assert.False(failure.FalsifiesRecord);
        Assert.False(failure.BlocksCampaign);
    }
}

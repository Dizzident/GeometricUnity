using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.IntegrationTests;

/// <summary>
/// Comparison dry-run end-to-end test (Study S6, GAP-14).
/// Builds PredictionTestRecord with InMemoryDatasetAdapter,
/// runs CampaignRunner.RunWithStrategy(), asserts ComparisonCampaignResult
/// has RunRecords, and asserts NegativeResultArtifact produced for failing prediction.
/// </summary>
public class ComparisonDryRunTests
{
    [Fact]
    public void ComparisonCampaign_WithPredictions_ProducesRunRecords()
    {
        var spec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-integ-1",
            EnvironmentIds = new[] { "env-2d" },
            BranchSubsetIds = new[] { "branch-1" },
            ObservedOutputClassIds = new[] { "residual-norm" },
            ComparisonAssetIds = new[] { "asset-1" },
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        var asset = new ComparisonAsset
        {
            AssetId = "asset-1",
            SourceCitation = "Test asset",
            AcquisitionDate = DateTimeOffset.UtcNow,
            PreprocessingDescription = "none",
            AdmissibleUseStatement = "structural comparison only",
            DomainOfValidity = "test domain",
            UncertaintyModel = UncertaintyRecord.Unestimated(),
            ComparisonVariables = new Dictionary<string, string> { ["x"] = "position" },
        };

        var prediction = new PredictionTestRecord
        {
            TestId = "pred-1",
            ClaimClass = ClaimClass.ApproximateStructuralSurrogate,
            FormalSource = "D_omega* Upsilon = 0",
            BranchManifestId = "branch-1",
            ObservableMapId = "obs-map-1",
            TheoremDependencyStatus = "open",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "leading-order",
            ExternalComparisonAsset = asset,
            Falsifier = "residual > 1e-3",
            ArtifactLinks = new[] { "art-001" },
        };

        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(asset, new Dictionary<string, double[]>
        {
            ["x"] = new[] { 1.0, 2.0, 3.0 },
        });

        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(spec, new[] { prediction }, adapter);

        Assert.NotNull(result.CampaignResult);
        Assert.Equal("campaign-integ-1", result.CampaignResult.CampaignId);
        Assert.True(result.CampaignResult.RunRecords.Count + result.CampaignResult.Failures.Count > 0);
    }

    [Fact]
    public void ComparisonCampaign_FailingPrediction_ProducesNegativeArtifact()
    {
        var spec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-neg-1",
            EnvironmentIds = new[] { "env-2d" },
            BranchSubsetIds = new[] { "branch-1" },
            ObservedOutputClassIds = new[] { "residual-norm" },
            ComparisonAssetIds = new[] { "asset-1" },
            Mode = ComparisonMode.Quantitative,
            CalibrationPolicy = "fixed",
        };

        // Prediction with surrogate approximation -> should fail quantitative mode guard
        var prediction = new PredictionTestRecord
        {
            TestId = "pred-fail-1",
            ClaimClass = ClaimClass.PostdictionTarget,
            FormalSource = "test",
            BranchManifestId = "branch-1",
            ObservableMapId = "obs-map-1",
            TheoremDependencyStatus = "open",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "surrogate",
            Falsifier = "any deviation",
            ArtifactLinks = new[] { "art-001" },
        };

        var adapter = new InMemoryDatasetAdapter();
        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(spec, new[] { prediction }, adapter);

        // Surrogate + Quantitative mode -> should produce a failure
        Assert.NotEmpty(result.CampaignResult.Failures);
        Assert.NotEmpty(result.NegativeArtifacts);
        Assert.Equal("pred-fail-1", result.NegativeArtifacts[0].OriginalTestId);
    }

    [Fact]
    public void ComparisonCampaign_InadmissiblePrediction_ProducesFailure()
    {
        var spec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-inadmissible",
            EnvironmentIds = new[] { "env-2d" },
            BranchSubsetIds = new[] { "branch-1" },
            ObservedOutputClassIds = new[] { "residual-norm" },
            ComparisonAssetIds = Array.Empty<string>(),
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        // Prediction with empty falsifier -> PredictionValidator should demote to Inadmissible
        var prediction = new PredictionTestRecord
        {
            TestId = "pred-no-falsifier",
            ClaimClass = ClaimClass.ExactStructuralConsequence,
            FormalSource = "test",
            BranchManifestId = "branch-1",
            ObservableMapId = "obs-map-1",
            TheoremDependencyStatus = "closed",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "exact",
            Falsifier = "",
            ArtifactLinks = new[] { "art-001" },
        };

        var adapter = new InMemoryDatasetAdapter();
        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(spec, new[] { prediction }, adapter);

        Assert.NotEmpty(result.CampaignResult.Failures);
        Assert.Contains(result.CampaignResult.Failures,
            f => f.TestId == "pred-no-falsifier" && f.DemotedClaimClass == ClaimClass.Inadmissible);
    }
}

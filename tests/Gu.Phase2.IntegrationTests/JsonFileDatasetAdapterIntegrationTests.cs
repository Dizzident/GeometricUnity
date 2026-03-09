using System.Text.Json;
using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.IntegrationTests;

/// <summary>
/// Integration tests for the file-backed external comparison dataset path.
/// Exercises JsonFileDatasetAdapter -> CampaignRunner end-to-end.
/// Per GAP-6 and DoD criterion #10.
/// </summary>
public sealed class JsonFileDatasetAdapterIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDatasetAdapterIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"gu-json-integ-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void S6_FileBackedCampaign_LoadsAssetFromDisk_ProducesRunRecord()
    {
        // 1. Write a valid ComparisonAsset JSON file to temp directory
        var asset = new ComparisonAsset
        {
            AssetId = "file-asset-1",
            SourceCitation = "Integration test data 2026",
            AcquisitionDate = DateTimeOffset.UtcNow.AddYears(-1),
            PreprocessingDescription = "None",
            AdmissibleUseStatement = "Structural comparison only",
            DomainOfValidity = "Low energy",
            UncertaintyModel = new UncertaintyRecord
            {
                Discretization = 0.01,
                Solver = 0.005,
                Branch = 0.02,
                Extraction = 0.01,
                Calibration = 0.005,
                DataAsset = 0.03,
            },
            ComparisonVariables = new Dictionary<string, string>
            {
                ["mass"] = "Test mass observable",
            },
        };

        var data = new Dictionary<string, double[]>
        {
            ["mass"] = [125.0, 0.5],
        };

        WriteAssetFile("file-asset-1", asset, data);

        // 2. Construct JsonFileDatasetAdapter pointing at temp directory
        var adapter = new JsonFileDatasetAdapter(_tempDir, "file-integ-adapter");

        // 3. Create a PredictionTestRecord referencing the asset
        var prediction = new PredictionTestRecord
        {
            TestId = "pred-file-1",
            ClaimClass = ClaimClass.ApproximateStructuralSurrogate,
            FormalSource = "Shiab identity operator trace",
            BranchManifestId = "branch-1",
            ObservableMapId = "pullback-trace",
            TheoremDependencyStatus = "open",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "leading-order",
            ExternalComparisonAsset = asset,
            Falsifier = "Observable differs from SM prediction by more than 3-sigma",
            ArtifactLinks = ["art-file-1"],
        };

        var campaignSpec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-file-1",
            EnvironmentIds = ["env-1"],
            BranchSubsetIds = ["branch-1"],
            ObservedOutputClassIds = ["mass"],
            ComparisonAssetIds = ["file-asset-1"],
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        // 4. Run CampaignRunner with the file-backed adapter
        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(campaignSpec, [prediction], adapter);

        // 5. Assert ComparisonRunRecord produced
        Assert.NotNull(result.CampaignResult);
        Assert.Equal("campaign-file-1", result.CampaignResult.CampaignId);
        Assert.True(
            result.CampaignResult.RunRecords.Count >= 1,
            "Expected at least one run record from file-backed campaign");

        // Verify the adapter loaded the asset from disk
        Assert.True(adapter.CanProvide("file-asset-1"));
    }

    [Fact]
    public void S6_FileBackedCampaign_MissingFile_ProducesFailureRecord()
    {
        // Point adapter at temp directory (no files written)
        var adapter = new JsonFileDatasetAdapter(_tempDir, "file-integ-adapter");

        // Reference a nonexistent asset
        var prediction = new PredictionTestRecord
        {
            TestId = "pred-missing-1",
            ClaimClass = ClaimClass.ApproximateStructuralSurrogate,
            FormalSource = "Shiab trace",
            BranchManifestId = "branch-1",
            ObservableMapId = "pullback-trace",
            TheoremDependencyStatus = "open",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "leading-order",
            Falsifier = "Mass differs by > 3-sigma",
            ArtifactLinks = ["art-missing-1"],
        };

        var campaignSpec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-missing-1",
            EnvironmentIds = ["env-1"],
            BranchSubsetIds = ["branch-1"],
            ObservedOutputClassIds = ["mass"],
            ComparisonAssetIds = ["nonexistent-asset"],
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        // Run the campaign - should produce failure, not exception
        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(campaignSpec, [prediction], adapter);

        Assert.NotNull(result.CampaignResult);
        // The asset can't be loaded, so either a failure record or negative artifact is produced
        Assert.True(
            result.CampaignResult.Failures.Count >= 1 || result.NegativeArtifacts.Count >= 1,
            "Expected at least one failure record or negative artifact for missing file");
    }

    private void WriteAssetFile(string assetId, ComparisonAsset asset, Dictionary<string, double[]> fileData)
    {
        var doc = new { asset, data = fileData };
        var json = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(_tempDir, $"{assetId}.json"), json);
    }
}

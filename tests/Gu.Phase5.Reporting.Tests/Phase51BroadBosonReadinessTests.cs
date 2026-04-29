using System.Text.Json;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase51BroadBosonReadinessTests
{
    [Fact]
    public void ReadinessMatrix_HasOnlyWzRatioPredicted()
    {
        using var doc = LoadReadiness();
        var root = doc.RootElement;

        Assert.Equal("broad-boson-prediction-blocked", root.GetProperty("terminalStatus").GetString());
        Assert.Equal(1, root.GetProperty("validatedPredictionCount").GetInt32());
        Assert.Equal(5, root.GetProperty("blockedPredictionCount").GetInt32());

        var records = root.GetProperty("records").EnumerateArray().ToArray();
        Assert.Equal(6, records.Length);
        Assert.Single(records, r => r.GetProperty("readinessStatus").GetString() == "predicted");
        Assert.Equal(
            "physical-w-z-mass-ratio",
            records.Single(r => r.GetProperty("readinessStatus").GetString() == "predicted")
                .GetProperty("targetObservableId").GetString());
    }

    [Fact]
    public void AbsoluteWzMasses_AreBlockedOnlyByAbsoluteScaleAndMappings()
    {
        using var doc = LoadReadiness();
        var records = doc.RootElement.GetProperty("records").EnumerateArray().ToArray();

        foreach (var id in new[] { "phase51-w-absolute-mass", "phase51-z-absolute-mass" })
        {
            var record = records.Single(r => r.GetProperty("recordId").GetString() == id);
            Assert.Equal("available", record.GetProperty("targetStatus").GetString());
            Assert.Equal("validated", record.GetProperty("identityRuleStatus").GetString());
            Assert.Equal("missing", record.GetProperty("mappingStatus").GetString());
            Assert.Equal(
                "missing-target-independent-absolute-scale",
                record.GetProperty("calibrationStatus").GetString());
            Assert.Contains(
                record.GetProperty("closureRequirements").EnumerateArray(),
                e => e.GetString()!.Contains("target-independent mass-energy scale", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void HiggsPhotonAndGluon_AreExplicitlyBlockedByMissingSectorContracts()
    {
        using var doc = LoadReadiness();
        var records = doc.RootElement.GetProperty("records").EnumerateArray().ToArray();

        var higgs = records.Single(r => r.GetProperty("particleId").GetString() == "higgs");
        Assert.Equal("missing-scalar-mode", higgs.GetProperty("internalModeStatus").GetString());
        Assert.Equal("missing-higgs-identity-rule", higgs.GetProperty("identityRuleStatus").GetString());

        var photon = records.Single(r => r.GetProperty("particleId").GetString() == "photon");
        Assert.Equal("missing-active-target", photon.GetProperty("targetStatus").GetString());
        Assert.Equal("missing-photon-identity-rule", photon.GetProperty("identityRuleStatus").GetString());

        var gluon = records.Single(r => r.GetProperty("particleId").GetString() == "gluon");
        Assert.Equal("missing-active-target", gluon.GetProperty("targetStatus").GetString());
        Assert.Equal("missing-gluon-identity-rule", gluon.GetProperty("identityRuleStatus").GetString());
    }

    [Fact]
    public void NextRecommendation_StartsWithAbsoluteElectroweakScale()
    {
        using var doc = LoadReadiness();
        var recommendation = doc.RootElement.GetProperty("nextPhaseRecommendations")
            .EnumerateArray()
            .OrderBy(r => r.GetProperty("priority").GetInt32())
            .First();

        Assert.Equal("phase52-electroweak-absolute-scale-calibration", recommendation.GetProperty("phaseCandidate").GetString());
        Assert.Contains(
            recommendation.GetProperty("requiredOutputs").EnumerateArray(),
            e => e.GetString() == "target-independent mass-energy calibration record");
    }

    private static JsonDocument LoadReadiness()
    {
        var path = Path.Combine(
            FindRepoRoot(),
            "studies",
            "phase51_broad_boson_prediction_readiness_001",
            "broad_boson_prediction_readiness.json");
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GeometricUnity.slnx")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}

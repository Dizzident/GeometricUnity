using System.Text.Json;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase53ElectroweakScaleSourceIntakeTests
{
    [Fact]
    public void ScaleSourceIntakeKeepsAbsoluteProjectionBlockedWithoutSelectedScale()
    {
        using var doc = LoadJson("scale_source_intake.json");
        var root = doc.RootElement;

        Assert.Equal("scale-source-intake-blocked", root.GetProperty("terminalStatus").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("selectedScaleSource").ValueKind);

        var status = root.GetProperty("absoluteProjectionStatus");
        Assert.Equal("blocked-no-selected-scale-source", status.GetProperty("physical-w-boson-mass-gev").GetString());
        Assert.Equal("blocked-no-selected-scale-source", status.GetProperty("physical-z-boson-mass-gev").GetString());
    }

    [Fact]
    public void IntakeRejectsWOrZTargetFitScale()
    {
        using var doc = LoadJson("scale_source_intake.json");
        var candidates = doc.RootElement.GetProperty("candidateSources").EnumerateArray().ToArray();

        var rejected = candidates.Single(c => c.GetProperty("scaleId").GetString() == "phase53-w-or-z-target-fit-scale");
        Assert.Equal("rejected", rejected.GetProperty("status").GetString());
        Assert.Equal("target-fitted", rejected.GetProperty("sourceKind").GetString());
        Assert.Contains(
            rejected.GetProperty("blockReasons").EnumerateArray(),
            reason => reason.GetString()!.Contains("cannot be used to claim independent W or Z", StringComparison.Ordinal));
    }

    [Fact]
    public void ExternalLaneRequiresBridgeAndExcludesWzTargets()
    {
        using var doc = LoadJson("scale_source_intake.json");
        var candidates = doc.RootElement.GetProperty("candidateSources").EnumerateArray().ToArray();

        var external = candidates.Single(c => c.GetProperty("scaleId").GetString() == "phase53-disjoint-external-electroweak-input-scale");
        Assert.Equal("blocked", external.GetProperty("status").GetString());

        var excluded = external.GetProperty("excludedTargetObservableIds").EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();
        Assert.Contains("physical-w-boson-mass-gev", excluded);
        Assert.Contains("physical-z-boson-mass-gev", excluded);

        Assert.Contains(
            external.GetProperty("closureRequirements").EnumerateArray(),
            requirement => requirement.GetString()!.Contains("validated internal bridge observable", StringComparison.Ordinal));
    }

    [Fact]
    public void BridgeContractHardRejectsTargetContamination()
    {
        using var doc = LoadJson("scale_bridge_validation_contract.json");
        var rejectConditions = doc.RootElement.GetProperty("hardRejectConditions").EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();

        Assert.Contains("scale source uses physical-w-boson-mass-gev", rejectConditions);
        Assert.Contains("scale source uses physical-z-boson-mass-gev", rejectConditions);
        Assert.Contains("bridge observable is missing or has unestimated uncertainty", rejectConditions);
    }

    private static JsonDocument LoadJson(string fileName)
    {
        var path = Path.Combine(
            FindRepoRoot(),
            "studies",
            "phase53_electroweak_scale_source_intake_001",
            fileName);
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

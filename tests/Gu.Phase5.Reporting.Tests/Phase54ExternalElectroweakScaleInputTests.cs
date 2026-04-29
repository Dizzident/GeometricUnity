using System.Text.Json;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase54ExternalElectroweakScaleInputTests
{
    [Fact]
    public void ExternalFermiInputIsIngestedAndExcludesWzTargets()
    {
        using var doc = LoadJson("external_electroweak_scale_input.json");
        var root = doc.RootElement;

        Assert.Equal("external-scale-input-ingested-bridge-blocked", root.GetProperty("terminalStatus").GetString());

        var input = root.GetProperty("externalInputs").EnumerateArray().Single();
        Assert.Equal("codata-2022-fermi-coupling-constant", input.GetProperty("inputId").GetString());
        Assert.Equal(0.000011663787, input.GetProperty("value").GetDouble(), precision: 15);
        Assert.True(
            System.Math.Abs(6E-12 - input.GetProperty("standardUncertainty").GetDouble()) < 1E-20,
            "Fermi coupling uncertainty should match the CODATA 2022 standard uncertainty.");
        Assert.Equal("ingested", input.GetProperty("status").GetString());

        var excluded = input.GetProperty("excludedTargetObservableIds").EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();
        Assert.Contains("physical-w-boson-mass-gev", excluded);
        Assert.Contains("physical-z-boson-mass-gev", excluded);
    }

    [Fact]
    public void DerivedVacuumScaleIsRecordedButNotSelectedAsInternalCalibration()
    {
        using var doc = LoadJson("external_electroweak_scale_input.json");
        var root = doc.RootElement;

        var scale = root.GetProperty("derivedExternalScaleCandidates").EnumerateArray().Single();
        Assert.Equal("phase54-fermi-derived-electroweak-vacuum-scale", scale.GetProperty("scaleId").GetString());
        Assert.Equal(246.21965079413738, scale.GetProperty("value").GetDouble(), precision: 12);
        Assert.Equal("GeV", scale.GetProperty("unit").GetString());

        Assert.Equal(JsonValueKind.Null, root.GetProperty("selectedScaleSource").ValueKind);
        Assert.Equal("blocked", root.GetProperty("internalBridgeStatus").GetProperty("status").GetString());
    }

    [Fact]
    public void AbsoluteProjectionRemainsBlockedUntilInternalBridgeExists()
    {
        using var doc = LoadJson("external_electroweak_scale_input.json");
        var status = doc.RootElement.GetProperty("absoluteProjectionStatus");

        Assert.Equal("blocked-no-internal-bridge", status.GetProperty("physical-w-boson-mass-gev").GetString());
        Assert.Equal("blocked-no-internal-bridge", status.GetProperty("physical-z-boson-mass-gev").GetString());

        var bridge = doc.RootElement.GetProperty("internalBridgeStatus");
        Assert.Contains(
            bridge.GetProperty("blockReasons").EnumerateArray(),
            reason => reason.GetString()!.Contains("using v directly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BridgeSearchReportDoesNotPromoteExistingInsufficientArtifacts()
    {
        using var doc = LoadJson("internal_bridge_search_report.json");
        var root = doc.RootElement;

        Assert.Equal("bridge-not-found", root.GetProperty("status").GetString());
        var artifacts = root.GetProperty("candidateInternalArtifacts").EnumerateArray().ToArray();
        Assert.NotEmpty(artifacts);
        Assert.All(artifacts, artifact => Assert.Equal("insufficient", artifact.GetProperty("bridgeStatus").GetString()));

        var required = root.GetProperty("requiredNextArtifact");
        Assert.Equal("internal-electroweak-bridge-observable", required.GetProperty("artifactKind").GetString());
        Assert.Equal("validated", required.GetProperty("minimumStatusForProjection").GetString());
    }

    private static JsonDocument LoadJson(string fileName)
    {
        var path = Path.Combine(
            FindRepoRoot(),
            "studies",
            "phase54_external_electroweak_scale_input_001",
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

using System.Text.Json;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase52ElectroweakAbsoluteScaleAuditTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    [Fact]
    public void AbsoluteScaleAuditBlocksAbsolutePredictionUntilIndependentScaleExists()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepoRoot,
            "studies/phase52_electroweak_absolute_scale_audit_001/absolute_scale_audit.json")));

        var root = document.RootElement;
        Assert.Equal("absolute-wz-prediction-blocked", root.GetProperty("terminalStatus").GetString());

        var readiness = root.GetProperty("absolutePredictionReadiness");
        foreach (var observableId in new[] { "physical-w-boson-mass-gev", "physical-z-boson-mass-gev" })
        {
            var record = readiness.GetProperty(observableId);
            Assert.Equal("validated", record.GetProperty("internalModeStatus").GetString());
            Assert.Equal("blocked-missing-target-independent-scale", record.GetProperty("calibrationStatus").GetString());
            Assert.Equal("blocked", record.GetProperty("predictionStatus").GetString());
        }
    }

    [Fact]
    public void TargetFitControlsAreRejectedAndExposeCrossCheckResiduals()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepoRoot,
            "studies/phase52_electroweak_absolute_scale_audit_001/absolute_scale_audit.json")));

        var diagnostics = document.RootElement.GetProperty("targetFitScaleDiagnostics").EnumerateArray().ToArray();
        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.Equal("rejected", diagnostic.GetProperty("status").GetString());
            Assert.Contains("fitted to a target mass", diagnostic.GetProperty("rejectionReason").GetString(), StringComparison.Ordinal);
        });

        var wFit = diagnostics.Single(d => d.GetProperty("diagnosticId").GetString() == "phase52-fit-w-target-cross-check-z");
        Assert.Equal(91.36071050377454, wFit.GetProperty("crossCheckComputedValue").GetDouble(), precision: 12);
        Assert.True(wFit.GetProperty("crossCheckTargetPull").GetDouble() > 80);

        var zFit = diagnostics.Single(d => d.GetProperty("diagnosticId").GetString() == "phase52-fit-z-target-cross-check-w");
        Assert.Equal(80.2172681143632, zFit.GetProperty("crossCheckComputedValue").GetDouble(), precision: 12);
        Assert.True(zFit.GetProperty("crossCheckTargetPull").GetDouble() < -10);
    }

    [Fact]
    public void ProjectionContractForbidsFittingWOrZTargets()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepoRoot,
            "studies/phase52_electroweak_absolute_scale_audit_001/absolute_mass_projection_contract.json")));

        var root = document.RootElement;
        Assert.Equal("defined", root.GetProperty("status").GetString());

        var forbidden = root.GetProperty("forbiddenCalibrationSources").EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();
        Assert.Contains("physical-w-boson-mass-gev", forbidden);
        Assert.Contains("physical-z-boson-mass-gev", forbidden);

        var outputs = root.GetProperty("requiredOutputObservableIds").EnumerateArray()
            .Select(e => e.GetString())
            .ToArray();
        Assert.Contains("physical-w-boson-mass-gev", outputs);
        Assert.Contains("physical-z-boson-mass-gev", outputs);
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

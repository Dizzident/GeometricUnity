using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzSelectorSpectrumIndependenceAuditTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p35-{Guid.NewGuid():N}");

    public WzSelectorSpectrumIndependenceAuditTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Evaluate_ProxyOnlyInvariantSelectorSpectra_BlocksPhysicalPredictionPath()
    {
        WriteSpectrum("phase12-candidate-0", "branch-a__L0__env-a", 8.0, solverBacked: false);
        WriteSpectrum("phase12-candidate-2", "branch-a__L0__env-a", 10.0, solverBacked: false);
        WriteSpectrum("phase12-candidate-0", "branch-b__L1__env-b", 8.8, solverBacked: false);
        WriteSpectrum("phase12-candidate-2", "branch-b__L1__env-b", 11.0, solverBacked: false);

        var result = WzSelectorSpectrumIndependenceAudit.Evaluate(
            MakePathDiagnosticJson(),
            MakeCandidateModeSourcesJson(),
            _tempDir,
            MakeProvenance());

        Assert.Equal("wz-selector-spectrum-independence-blocked", result.TerminalStatus);
        Assert.Equal(2, result.InspectedAlignedCellCount);
        Assert.Equal(2, result.ProxyOnlyCellCount);
        Assert.Equal(0, result.SolverBackedCellCount);
        Assert.True(result.RatioInvariantAcrossSelectors);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("proxy-only", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_SolverBackedVaryingSelectorSpectra_CompletesAudit()
    {
        WriteSpectrum("phase12-candidate-0", "branch-a__L0__env-a", 8.0, solverBacked: true);
        WriteSpectrum("phase12-candidate-2", "branch-a__L0__env-a", 10.0, solverBacked: true);
        WriteSpectrum("phase12-candidate-0", "branch-b__L1__env-b", 8.9, solverBacked: true);
        WriteSpectrum("phase12-candidate-2", "branch-b__L1__env-b", 10.9, solverBacked: true);

        var result = WzSelectorSpectrumIndependenceAudit.Evaluate(
            MakePathDiagnosticJson(),
            MakeCandidateModeSourcesJson(),
            _tempDir,
            MakeProvenance());

        Assert.Equal("wz-selector-spectrum-independent-evidence-present", result.TerminalStatus);
        Assert.Equal(2, result.InspectedAlignedCellCount);
        Assert.Equal(0, result.ProxyOnlyCellCount);
        Assert.Equal(2, result.SolverBackedCellCount);
        Assert.False(result.RatioInvariantAcrossSelectors);
        Assert.Empty(result.ClosureRequirements);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteSpectrum(string candidateId, string selectorKey, double value, bool solverBacked)
    {
        var path = Path.Combine(_tempDir, $"{candidateId}__{selectorKey}_spectrum.json");
        var spectrum = solverBacked
            ? $$"""
                {
                  "spectrumId": "{{candidateId}}__{{selectorKey}}-spectrum",
                  "massLikeValues": [{{value}}],
                  "sourceArtifactPaths": ["test"],
                  "operatorBundleId": "op-test",
                  "solverMethod": "test-eigensolver",
                  "modes": [{ "modeId": "m0" }]
                }
                """
            : $$"""
                {
                  "spectrumId": "{{candidateId}}__{{selectorKey}}-spectrum",
                  "massLikeValues": [{{value}}],
                  "sourceArtifactPaths": ["test"]
                }
                """;
        File.WriteAllText(path, spectrum);
    }

    private static string MakePathDiagnosticJson()
        => """
           {
             "resultId": "phase34-wz-operator-spectrum-path-diagnostic-v1",
             "schemaVersion": "1.0.0",
             "terminalStatus": "wz-operator-spectrum-path-diagnostic-complete",
             "algorithmId": "p34-wz-operator-spectrum-path-diagnostic:v1",
             "selectedPairId": "phase22-phase12-candidate-0/phase22-phase12-candidate-2",
             "targetValue": 0.88136,
             "requiredScaleToTarget": 1.0,
             "layerRatios": [],
             "alignedSpectrumPointCount": 0,
             "diagnosis": [],
             "closureRequirements": [],
             "provenance": {
               "createdAt": "2026-04-28T00:00:00+00:00",
               "codeRevision": "test",
               "branch": { "branchId": "test", "schemaVersion": "1.0" },
               "backend": "cpu"
             }
           }
           """;

    private static string MakeCandidateModeSourcesJson()
        => """
           {
             "tableId": "test",
             "schemaVersion": "1.0.0",
             "terminalStatus": "candidate-source-ready",
             "readySourceCount": 2,
             "candidateModeSources": [
               { "sourceId": "phase22-phase12-candidate-0", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-0", "value": 8.0, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "p20-phase12-internal-vector-boson-source-adapter:v1", "provenance": { "createdAt": "2026-04-28T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } },
               { "sourceId": "phase22-phase12-candidate-2", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-2", "value": 10.0, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "p20-phase12-internal-vector-boson-source-adapter:v1", "provenance": { "createdAt": "2026-04-28T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance()
        => new()
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-28T00:00:00+00:00"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            Backend = "cpu",
        };
}

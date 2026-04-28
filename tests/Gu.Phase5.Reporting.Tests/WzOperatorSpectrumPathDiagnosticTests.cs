using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzOperatorSpectrumPathDiagnosticTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p34-{Guid.NewGuid():N}");

    public WzOperatorSpectrumPathDiagnosticTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Evaluate_WhenAllLayersAgree_CompletesAndPointsToSpectrumPath()
    {
        WriteSpectrum("phase12-candidate-0", "b", "L0", "env", 8.0);
        WriteSpectrum("phase12-candidate-2", "b", "L0", "env", 10.0);

        var result = WzOperatorSpectrumPathDiagnostic.Evaluate(
            MakeClosureJson(),
            MakeCandidateModeSourcesJson(),
            MakeSourceCandidatesJson(),
            MakeModeFamiliesJson(),
            _tempDir,
            MakeProvenance());

        Assert.Equal("wz-operator-spectrum-path-diagnostic-complete", result.TerminalStatus);
        Assert.Equal(3, result.LayerRatios.Count);
        Assert.Equal(1, result.AlignedSpectrumPointCount);
        Assert.Null(result.FirstMismatchLayer);
        Assert.Contains(result.Diagnosis, d => d.Contains("per-point spectrum", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_WithMissingSpectraRoot_IsBlocked()
    {
        var result = WzOperatorSpectrumPathDiagnostic.Evaluate(
            MakeClosureJson(),
            MakeCandidateModeSourcesJson(),
            MakeSourceCandidatesJson(),
            MakeModeFamiliesJson(),
            Path.Combine(_tempDir, "missing"),
            MakeProvenance());

        Assert.Equal("wz-operator-spectrum-path-diagnostic-blocked", result.TerminalStatus);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("spectra root", StringComparison.Ordinal));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteSpectrum(string candidateId, string branch, string refinement, string environment, double value)
    {
        var path = Path.Combine(_tempDir, $"{candidateId}__{branch}__{refinement}__{environment}_spectrum.json");
        File.WriteAllText(path, $$"""
            {
              "sourceCandidateId": "{{candidateId}}",
              "branchVariantId": "{{branch}}",
              "refinementLevel": "{{refinement}}",
              "environmentId": "{{environment}}",
              "massLikeValues": [{{value}}]
            }
            """);
    }

    private static string MakeClosureJson()
        => GuJsonDefaults.Serialize(new WzNormalizationClosureDiagnosticResult
        {
            ResultId = "phase31-wz-normalization-closure-diagnostic-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = "wz-normalization-closure-blocked",
            AlgorithmId = WzNormalizationClosureDiagnostic.AlgorithmId,
            TargetObservableId = "physical-w-z-mass-ratio",
            SelectedPairId = "phase22-phase12-candidate-0/phase22-phase12-candidate-2",
            ComputedRatio = 0.8,
            TargetValue = 0.88,
            RequiredScaleToTarget = 1.1,
            DeclaredScaleFactor = 1.0,
            DeclaredScaleUncertainty = 0.0,
            DeclaredScaleDelta = -0.1,
            DeclaredCalibration = null,
            SelectorVariationExplainsMiss = false,
            DerivationBackedScaleAvailable = true,
            NormalizationChangeAllowed = false,
            Diagnosis = [],
            ClosureRequirements = [],
            Provenance = MakeProvenance(),
        });

    private static string MakeCandidateModeSourcesJson()
        => """
           {
             "tableId": "test",
             "schemaVersion": "1.0.0",
             "terminalStatus": "candidate-source-ready",
             "readySourceCount": 2,
             "candidateModeSources": [
               { "sourceId": "phase22-phase12-candidate-0", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-0", "value": 8.0, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "operator-method", "provenance": { "createdAt": "2026-04-28T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } },
               { "sourceId": "phase22-phase12-candidate-2", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-2", "value": 10.0, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "operator-method", "provenance": { "createdAt": "2026-04-28T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } }
             ]
           }
           """;

    private static string MakeSourceCandidatesJson()
        => """
           {
             "candidates": [
               { "sourceCandidateId": "phase22-phase12-candidate-0", "massLikeValue": 8.0 },
               { "sourceCandidateId": "phase22-phase12-candidate-2", "massLikeValue": 10.0 }
             ]
           }
           """;

    private static string MakeModeFamiliesJson()
        => """
           {
             "families": [
               { "sourceCandidateId": "phase12-candidate-0", "massLikeValue": 8.0 },
               { "sourceCandidateId": "phase12-candidate-2", "massLikeValue": 10.0 }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-28T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase34-wz-operator-spectrum-path-diagnostic", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}

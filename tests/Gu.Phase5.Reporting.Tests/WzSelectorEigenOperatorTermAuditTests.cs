using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzSelectorEigenOperatorTermAuditTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p45-{Guid.NewGuid():N}");

    public WzSelectorEigenOperatorTermAuditTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Evaluate_SolverBackedConnectionOnlySpectra_BlockOperatorTermReadiness()
    {
        WriteSpectrum("phase12-candidate-0", "cell-a", "connection", includeTermEvidence: false);
        WriteSpectrum("phase12-candidate-2", "cell-a", "connection", includeTermEvidence: false);

        var result = WzSelectorEigenOperatorTermAudit.Evaluate(
            MakeRatioDiagnosticJson(),
            MakeSelectorVariationJson(),
            _tempDir,
            MakeProvenance());

        Assert.Equal("wz-selector-eigen-operator-term-blocked", result.TerminalStatus);
        Assert.Equal(2, result.InspectedSpectrumCount);
        Assert.Equal(2, result.SolverBackedSpectrumCount);
        Assert.Equal(0, result.NonTrivialOperatorTermEvidenceCount);
        Assert.Contains("connection", result.ObservedModeBlocks);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("no target-independent electroweak", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_NonConnectionTermEvidence_CompletesAudit()
    {
        WriteSpectrum("phase12-candidate-0", "cell-a", "electroweak-mixing", includeTermEvidence: true);
        WriteSpectrum("phase12-candidate-2", "cell-a", "electroweak-mixing", includeTermEvidence: true);

        var result = WzSelectorEigenOperatorTermAudit.Evaluate(
            MakeRatioDiagnosticJson(),
            MakeSelectorVariationJson(),
            _tempDir,
            MakeProvenance());

        Assert.Equal("wz-selector-eigen-operator-term-ready", result.TerminalStatus);
        Assert.Equal(2, result.NonTrivialOperatorTermEvidenceCount);
        Assert.Empty(result.ClosureRequirements);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteSpectrum(string candidateId, string selectorKey, string blockName, bool includeTermEvidence)
    {
        var extra = includeTermEvidence ? "\"operatorNormalizationDerivationId\": \"operator-term:test\"," : "";
        var spectrum = $$"""
            {
              "spectrumId": "{{candidateId}}__{{selectorKey}}-spectrum",
              "operatorBundleId": "op-test",
              "solverMethod": "explicit-dense",
              "operatorType": "FullHessian",
              {{extra}}
              "modeRecords": [
                {
                  "modeId": "m0",
                  "eigenvalue": 1.0,
                  "blockEnergyFractions": {
                    "{{blockName}}": 1.0
                  }
                }
              ]
            }
            """;
        File.WriteAllText(Path.Combine(_tempDir, $"{candidateId}__{selectorKey}_spectrum.json"), spectrum);
    }

    private static string MakeRatioDiagnosticJson()
        => """
           {
             "selectedPair": {
               "pairId": "phase22-phase12-candidate-0/phase22-phase12-candidate-2",
               "requiredScaleToTarget": 1.021
             }
           }
           """;

    private static string MakeSelectorVariationJson()
        => """
           {
             "ratioMin": 0.861,
             "ratioMax": 0.865
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

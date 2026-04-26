using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class VectorBosonIdentityFeatureExtractorTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p25-{Guid.NewGuid():N}");

    public VectorBosonIdentityFeatureExtractorTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Extract_WithModeSignaturesAndCouplings_ProducesPartialFeaturesWithoutChargeSector()
    {
        WriteSignature("bg-a-mode-0", [1, 0, 0, 0, 2, 0]);
        WriteSignature("bg-b-mode-0", [0, 3, 0, 0, 4, 0]);

        var result = VectorBosonIdentityFeatureExtractor.Extract(
            """
            {
              "families": [
                {
                  "familyId": "phase22-family-phase12-candidate-0",
                  "sourceCandidateId": "phase12-candidate-0"
                }
              ]
            }
            """,
            """
            [
              {
                "familyId": "family-0",
                "memberModeIds": ["bg-a-mode-0", "bg-b-mode-0"]
              }
            ]
            """,
            _tempDir,
            [
                """
                {
                  "couplings": [
                    {
                      "bosonModeId": "candidate-0",
                      "fermionModeIdI": "f0",
                      "fermionModeIdJ": "f0",
                      "couplingProxyImaginary": 0.0,
                      "couplingProxyMagnitude": 2.0
                    },
                    {
                      "bosonModeId": "candidate-0",
                      "fermionModeIdI": "f0",
                      "fermionModeIdJ": "f1",
                      "couplingProxyImaginary": 1.0,
                      "couplingProxyMagnitude": 3.0
                    }
                  ]
                }
                """,
            ],
            MakeProvenance());

        Assert.Equal("identity-features-partial", result.TerminalStatus);
        var record = Assert.Single(result.FeatureRecords);
        Assert.Equal("su2-adjoint-triplet:canonical-basis", record.ElectroweakMultipletId);
        Assert.Null(record.ChargeSector);
        Assert.NotNull(record.CurrentCouplingSignature);
        Assert.Contains("identityFeatures", result.EnrichedModeFamiliesJson);
        Assert.Contains(record.Blockers, b => b.Contains("charged/neutral", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Extract_WithoutCouplings_ReportsCouplingBlocker()
    {
        WriteSignature("bg-a-mode-0", [1, 0, 0]);

        var result = VectorBosonIdentityFeatureExtractor.Extract(
            """
            { "families": [{ "familyId": "phase22-family-phase12-candidate-0", "sourceCandidateId": "phase12-candidate-0" }] }
            """,
            """
            [{ "familyId": "family-0", "memberModeIds": ["bg-a-mode-0"] }]
            """,
            _tempDir,
            ["{ \"couplings\": [] }"],
            MakeProvenance());

        var record = Assert.Single(result.FeatureRecords);
        Assert.Contains(record.Blockers, b => b.Contains("coupling profile", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteSignature(string modeId, double[] coefficients)
    {
        var json = $$"""
        {
          "modeId": "{{modeId}}",
          "observedCoefficients": [{{string.Join(",", coefficients.Select(c => c.ToString(System.Globalization.CultureInfo.InvariantCulture)))}}],
          "observedSignature": {
            "lieAlgebraBasisId": "canonical"
          },
          "observedShape": [{{coefficients.Length / 3}}, 3]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, $"{modeId}.json"), json);
    }

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase25-internal-electroweak-features", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}

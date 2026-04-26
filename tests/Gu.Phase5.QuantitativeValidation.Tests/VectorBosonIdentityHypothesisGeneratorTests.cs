using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class VectorBosonIdentityHypothesisGeneratorTests
{
    [Fact]
    public void Generate_WithTwoReadySources_ProducesProvisionalBlockedHypotheses()
    {
        var result = VectorBosonIdentityHypothesisGenerator.Generate(
            [MakeSource("low", 1.0), MakeSource("high", 2.0)],
            MakeProvenance());

        Assert.Equal("identity-blocked", result.TerminalStatus);
        Assert.Equal(2, result.CandidateModes.Count);
        Assert.All(result.CandidateModes, mode => Assert.Equal("provisional", mode.Status));
        Assert.Single(result.CandidateObservables);
        Assert.All(result.ModeIdentificationEvidence.Evidence, evidence => Assert.Equal("provisional", evidence.Status));
    }

    [Fact]
    public void Generate_WithOneReadySource_RemainsSourceBlocked()
    {
        var result = VectorBosonIdentityHypothesisGenerator.Generate([MakeSource("only", 1.0)], MakeProvenance());

        Assert.Equal("source-blocked", result.TerminalStatus);
        Assert.Empty(result.CandidateModes);
        Assert.Contains(result.ClosureRequirements, c => c.Contains("fewer than two", StringComparison.OrdinalIgnoreCase));
    }

    private static CandidateModeSourceRecord MakeSource(string id, double value) => new()
    {
        SourceId = id,
        SourceOrigin = CandidateModeExtractor.InternalComputedOrigin,
        SourceArtifactKind = CandidateModeExtractor.ComputedObservableArtifactKind,
        SourceArtifactPath = "studies/phase22/source_candidates.json",
        SourceObservableId = id,
        Value = value,
        Uncertainty = 0.01,
        UnitFamily = "mass-energy",
        Unit = "internal-mass-unit",
        EnvironmentId = "env-a",
        BranchId = "branch-a",
        RefinementLevel = "L0",
        SourceExtractionMethod = "test",
        Provenance = MakeProvenance(),
    };

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase23-wz-identity-hypotheses", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}

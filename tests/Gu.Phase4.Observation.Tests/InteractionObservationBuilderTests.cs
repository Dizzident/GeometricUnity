using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.Observation;
using Xunit;

namespace Gu.Phase4.Observation.Tests;

/// <summary>
/// Tests for FermionObservationBuilder interaction-summary methods:
/// BuildInteractionSummary and BuildAllInteractionSummaries.
/// </summary>
public sealed class InteractionObservationBuilderTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static CouplingAtlas MakeAtlas(
        string atlasId,
        params (string bosonId, string fi, string fj, double magnitude)[] couplings)
    {
        var prov = TestProvenance();
        return new CouplingAtlas
        {
            AtlasId = atlasId,
            FermionBackgroundId = "bg-001",
            BosonRegistryVersion = "v1",
            NormalizationConvention = "unit-modes",
            Couplings = couplings.Select(c => new BosonFermionCouplingRecord
            {
                CouplingId = $"coup-{c.bosonId}-{c.fi}-{c.fj}",
                BosonModeId = c.bosonId,
                FermionModeIdI = c.fi,
                FermionModeIdJ = c.fj,
                CouplingProxyReal = c.magnitude,
                CouplingProxyMagnitude = c.magnitude,
                NormalizationConvention = "unit-modes",
                Provenance = prov,
            }).ToList(),
            Provenance = prov,
        };
    }

    [Fact]
    public void BuildInteractionSummary_SingleCoupling_CorrectMagnitudes()
    {
        var atlas = MakeAtlas("atlas-1", ("b-0", "f-0", "f-1", 0.7));
        var summary = FermionObservationBuilder.BuildInteractionSummary(
            "b-0", atlas, 1e-12, TestProvenance());

        Assert.Equal("b-0", summary.BosonModeId);
        Assert.Equal(0.7, summary.MeanCouplingMagnitude, precision: 10);
        Assert.Equal(0.7, summary.MaxCouplingMagnitude, precision: 10);
        Assert.Equal(1, summary.NonZeroCouplingCount);
    }

    [Fact]
    public void BuildInteractionSummary_NoMatchingCouplings_ZeroMagnitudes()
    {
        var atlas = MakeAtlas("atlas-1", ("b-0", "f-0", "f-1", 0.7));
        var summary = FermionObservationBuilder.BuildInteractionSummary(
            "b-99", atlas, 1e-12, TestProvenance());

        Assert.Equal(0.0, summary.MeanCouplingMagnitude, precision: 15);
        Assert.Equal(0.0, summary.MaxCouplingMagnitude, precision: 15);
        Assert.Equal(0, summary.NonZeroCouplingCount);
    }

    [Fact]
    public void BuildInteractionSummary_MultipleCouplings_MeanIsAverage()
    {
        var atlas = MakeAtlas("atlas-2",
            ("b-0", "f-0", "f-1", 0.2),
            ("b-0", "f-1", "f-2", 0.6));

        var summary = FermionObservationBuilder.BuildInteractionSummary(
            "b-0", atlas, 1e-12, TestProvenance());

        Assert.Equal(0.4, summary.MeanCouplingMagnitude, precision: 10);
        Assert.Equal(0.6, summary.MaxCouplingMagnitude, precision: 10);
        Assert.Equal(2, summary.NonZeroCouplingCount);
    }

    [Fact]
    public void BuildInteractionSummary_ZeroThreshold_ExcludesExactlyZero()
    {
        var atlas = MakeAtlas("atlas-3",
            ("b-0", "f-0", "f-1", 0.0),
            ("b-0", "f-1", "f-2", 0.5));

        var summary = FermionObservationBuilder.BuildInteractionSummary(
            "b-0", atlas, 1e-12, TestProvenance());

        Assert.Equal(1, summary.NonZeroCouplingCount);
    }

    [Fact]
    public void BuildInteractionSummary_FermionModeIds_AreDistinct()
    {
        var atlas = MakeAtlas("atlas-4",
            ("b-0", "f-0", "f-1", 0.5),
            ("b-0", "f-0", "f-2", 0.3));  // f-0 appears twice

        var summary = FermionObservationBuilder.BuildInteractionSummary(
            "b-0", atlas, 1e-12, TestProvenance());

        // f-0, f-1, f-2 — f-0 should appear only once
        Assert.Equal(3, summary.FermionModeIds.Count);
        Assert.Equal(summary.FermionModeIds.Distinct().Count(), summary.FermionModeIds.Count);
    }

    [Fact]
    public void BuildInteractionSummary_AtlasIdPreserved()
    {
        var atlas = MakeAtlas("my-unique-atlas", ("b-0", "f-0", "f-1", 1.0));
        var summary = FermionObservationBuilder.BuildInteractionSummary(
            "b-0", atlas, 1e-12, TestProvenance());

        Assert.Equal("my-unique-atlas", summary.AtlasId);
    }

    [Fact]
    public void BuildInteractionSummary_NormalizationConventionFromAtlas()
    {
        var atlas = MakeAtlas("atlas-5", ("b-0", "f-0", "f-1", 0.4));
        var summary = FermionObservationBuilder.BuildInteractionSummary(
            "b-0", atlas, 1e-12, TestProvenance());

        Assert.Equal("unit-modes", summary.NormalizationConvention);
    }

    [Fact]
    public void BuildAllInteractionSummaries_TwoBosonModes_TwoSummaries()
    {
        var atlas = MakeAtlas("atlas-6",
            ("b-0", "f-0", "f-1", 0.5),
            ("b-1", "f-0", "f-1", 0.3));

        var summaries = FermionObservationBuilder.BuildAllInteractionSummaries(
            atlas, 1e-12, TestProvenance());

        Assert.Equal(2, summaries.Count);
    }

    [Fact]
    public void BuildAllInteractionSummaries_EmptyAtlas_ReturnsEmpty()
    {
        var atlas = MakeAtlas("atlas-empty");
        var summaries = FermionObservationBuilder.BuildAllInteractionSummaries(
            atlas, 1e-12, TestProvenance());

        Assert.Empty(summaries);
    }

    [Fact]
    public void BuildAllInteractionSummaries_DuplicateBosonIds_OneSummaryEach()
    {
        var atlas = MakeAtlas("atlas-7",
            ("b-0", "f-0", "f-1", 0.5),
            ("b-0", "f-1", "f-2", 0.3),  // same boson mode
            ("b-1", "f-0", "f-1", 0.2));

        var summaries = FermionObservationBuilder.BuildAllInteractionSummaries(
            atlas, 1e-12, TestProvenance());

        // b-0 and b-1 -> 2 summaries
        Assert.Equal(2, summaries.Count);
    }
}

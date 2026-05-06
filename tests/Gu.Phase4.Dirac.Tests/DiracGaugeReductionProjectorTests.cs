using Gu.Core;
using Gu.Phase4.Dirac;

namespace Gu.Phase4.Dirac.Tests;

public sealed class DiracGaugeReductionProjectorTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-dirac-gauge-projector",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    [Fact]
    public void Project_WithIdentityProjector_PreservesMatrixAndMarksGaugeReduced()
    {
        var source = BuildSourceBundle();
        var projector = DiracGaugeReductionProjector.IdentityProjector(source.TotalDof);
        var reducer = new DiracGaugeReductionProjector();

        var reduced = reducer.Project(source, projector, "identity", TestProvenance());

        Assert.True(reduced.GaugeReductionApplied);
        Assert.True(reduced.IsHermitian);
        Assert.Equal(source.ExplicitMatrix!, reduced.ExplicitMatrix!);
        Assert.Contains("gauge-reduced-identity", reduced.OperatorId);
    }

    [Fact]
    public void Project_WithCoordinateProjector_AppliesSandwich()
    {
        var source = BuildSourceBundle();
        var projector = new[]
        {
            1.0, 0.0,
            0.0, 0.0,
        };
        var reducer = new DiracGaugeReductionProjector();

        var reduced = reducer.Project(source, projector, "keep-first", TestProvenance());

        Assert.True(reduced.GaugeReductionApplied);
        Assert.Equal(
            new[]
            {
                2.0, 0.0,
                0.0, 0.0,
                0.0, 0.0,
                0.0, 0.0,
            },
            reduced.ExplicitMatrix!);
    }

    [Fact]
    public void Project_WithNonIdempotentProjector_Throws()
    {
        var source = BuildSourceBundle();
        var projector = new[]
        {
            0.5, 0.0,
            0.0, 1.0,
        };
        var reducer = new DiracGaugeReductionProjector();

        Assert.Throws<ArgumentException>(() =>
            reducer.Project(source, projector, "bad", TestProvenance()));
    }

    private static DiracOperatorBundle BuildSourceBundle() => new()
    {
        OperatorId = "dirac-test",
        FermionBackgroundId = "bg-test",
        LayoutId = "layout-test",
        SpinConnectionId = "spin-test",
        MatrixShape = new[] { 2, 2 },
        HasExplicitMatrix = true,
        ExplicitMatrix =
        [
            2.0, 0.0,
            1.0, 0.0,
            1.0, 0.0,
            3.0, 0.0,
        ],
        ExplicitMatrixRef = null,
        IsHermitian = true,
        HermiticityResidual = 0.0,
        HermiticityTolerance = 1e-10,
        MassBranchTermIncluded = false,
        CorrectionTermIncluded = false,
        GaugeReductionApplied = false,
        CellCount = 1,
        DofsPerCell = 2,
        DiagnosticNotes = [],
        Provenance = TestProvenance(),
    };
}

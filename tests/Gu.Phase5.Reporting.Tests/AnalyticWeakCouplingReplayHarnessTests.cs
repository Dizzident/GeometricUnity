using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Fermions;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class AnalyticWeakCouplingReplayHarnessTests
{
    [Fact]
    public void ReplayFromAnalyticVariation_WithUnitModes_ProducesValidatedEvidence()
    {
        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        var mode = MakeMode("mode-i", [2.0, 0.0, 0.0, 0.0]);

        var result = AnalyticWeakCouplingReplayHarness.ReplayFromAnalyticVariation(
            mode,
            mode,
            "w-mode",
            identity,
            analyticVariationMatrixIm: null,
            variationEvidenceId: "analytic-variation-w-mode",
            provenance: MakeProvenance());

        Assert.Equal("analytic-weak-coupling-replay-validated", result.TerminalStatus);
        Assert.NotNull(result.CouplingRecord);
        Assert.Equal("unit-modes", result.CouplingRecord!.NormalizationConvention);
        Assert.Equal("analytic-dirac-variation-matrix-element:v1", result.CouplingRecord.VariationMethod);
        Assert.Equal(1.0, result.CouplingRecord.CouplingProxyMagnitude, precision: 12);
        Assert.Equal("raw-weak-coupling-matrix-element-evidence-validated", result.EvidenceBuild!.EvidenceValidation.TerminalStatus);
    }

    [Fact]
    public void ReplayFromAnalyticVariation_WithMissingModeVector_Blocks()
    {
        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };

        var result = AnalyticWeakCouplingReplayHarness.ReplayFromAnalyticVariation(
            MakeMode("mode-i", null),
            MakeMode("mode-j", [1.0, 0.0, 0.0, 0.0]),
            "w-mode",
            identity,
            analyticVariationMatrixIm: null,
            variationEvidenceId: "analytic-variation-w-mode",
            provenance: MakeProvenance());

        Assert.Equal("analytic-weak-coupling-replay-blocked", result.TerminalStatus);
        Assert.Null(result.CouplingRecord);
        Assert.Contains("fermion mode I eigenvector coefficients are missing", result.ClosureRequirements);
    }

    private static FermionModeRecord MakeMode(string id, double[]? eigenvector) => new()
    {
        ModeId = id,
        BackgroundId = "bg",
        BranchVariantId = "branch",
        LayoutId = "layout",
        ModeIndex = 0,
        EigenvalueRe = 0.1,
        EigenvalueIm = 0.0,
        ResidualNorm = 0.0,
        EigenvectorCoefficients = eigenvector,
        ChiralityDecomposition = new ChiralityDecompositionRecord
        {
            LeftFraction = 0.5,
            RightFraction = 0.5,
            MixedFraction = 0.0,
            SignConvention = "left-is-minus",
        },
        ConjugationPairing = new ConjugationPairingRecord
        {
            HasPair = false,
            ConjugationType = "charge-conjugation",
        },
        Backend = "cpu",
        Provenance = MakeProvenance(),
    };

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-29T00:00:00Z"),
        CodeRevision = "test-revision",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}

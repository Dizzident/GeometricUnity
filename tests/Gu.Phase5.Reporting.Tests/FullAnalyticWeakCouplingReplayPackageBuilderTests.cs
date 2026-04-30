using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Fermions;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class FullAnalyticWeakCouplingReplayPackageBuilderTests
{
    [Fact]
    public void Build_WithValidatedReplayAndPhysicalSource_PersistsFullPackage()
    {
        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        var mode = MakeMode("mode-i", [2.0, 0.0, 0.0, 0.0]);
        var replay = AnalyticWeakCouplingReplayHarness.ReplayFromAnalyticVariation(
            mode,
            mode,
            "w-mode",
            identity,
            analyticVariationMatrixIm: null,
            variationEvidenceId: "analytic-variation-w-mode",
            provenance: MakeProvenance());

        var package = FullAnalyticWeakCouplingReplayPackageBuilder.Build(
            "phase81-full-replay-package",
            ProductionAnalyticReplayInputMaterializationAuditor.AcceptedBosonModeSourceKind,
            [1.0, 0.0, 0.0],
            identity,
            analyticVariationMatrixIm: null,
            mode,
            mode,
            replay,
            "phase81:test-revision:cpu");

        Assert.Empty(package.ClosureRequirements);
        Assert.Equal("production-analytic-replay-inputs-materialized", package.MaterializationAudit.TerminalStatus);
        Assert.Equal("raw-weak-coupling-matrix-element-evidence-validated", package.EvidenceBuild.EvidenceValidation.TerminalStatus);
        Assert.Equal(2, package.AnalyticVariationMatrixRe.Count);
        Assert.Equal([2.0, 0.0, 0.0, 0.0], package.FermionModeI.EigenvectorCoefficients);
        Assert.Equal(1.0, package.CouplingRecord.CouplingProxyMagnitude, precision: 12);
    }

    [Fact]
    public void Build_WithSyntheticSource_BlocksMaterialization()
    {
        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        var mode = MakeMode("mode-i", [2.0, 0.0, 0.0, 0.0]);
        var replay = AnalyticWeakCouplingReplayHarness.ReplayFromAnalyticVariation(
            mode,
            mode,
            "boson-synthetic-00",
            identity,
            analyticVariationMatrixIm: null,
            variationEvidenceId: "analytic-variation-synthetic",
            provenance: MakeProvenance());

        var package = FullAnalyticWeakCouplingReplayPackageBuilder.Build(
            "phase81-synthetic-replay-package",
            "synthetic-boson-perturbation",
            [1.0, 0.0, 0.0],
            identity,
            analyticVariationMatrixIm: null,
            mode,
            mode,
            replay,
            "phase81:test-revision:cpu");

        Assert.Equal("production-analytic-replay-inputs-blocked", package.MaterializationAudit.TerminalStatus);
        Assert.Contains(
            $"boson mode source kind must be {ProductionAnalyticReplayInputMaterializationAuditor.AcceptedBosonModeSourceKind}",
            package.ClosureRequirements);
    }

    private static FermionModeRecord MakeMode(string id, double[] eigenvector) => new()
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

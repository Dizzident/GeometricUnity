using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class SourceBackedAnalyticReplayPackageRunnerTests
{
    [Fact]
    public void Run_WithSourceBosonVectorAndFermionEigenvectors_BuildsFullReplayPackage()
    {
        var provenance = MakeProvenance();
        var result = SourceBackedAnalyticReplayPackageRunner.Run(
            packageId: "phase83-source-backed-package",
            sourceArtifactId: "phase12-mode-0.json",
            bosonModeJson:
            """
            {
              "modeId": "phase12-boson-mode-0",
              "modeVector": [1.0]
            }
            """,
            gammas: BuildGammaBundle(provenance),
            cellCount: 2,
            spinorDim: 1,
            dimG: 1,
            edgeLengths: [1.0],
            cellsPerEdge: [[0, 1]],
            edgeDirections: [[1.0]],
            modeI: MakeMode("fermion-cell-0", [1.0, 0.0, 0.0, 0.0], provenance),
            modeJ: MakeMode("fermion-cell-1", [0.0, 0.0, 1.0, 0.0], provenance),
            provenanceId: "phase83:test-revision:cpu",
            provenance: provenance);

        Assert.Equal("source-backed-analytic-replay-package-built", result.TerminalStatus);
        Assert.Empty(result.ClosureRequirements);
        Assert.Equal("boson-perturbation-vector-materialized", result.BosonPerturbationVectorMaterialization.TerminalStatus);
        Assert.Equal("modeVector", result.BosonPerturbationVectorMaterialization.SourceFieldName);

        var package = Assert.IsType<FullAnalyticWeakCouplingReplayPackage>(result.FullReplayPackage);
        Assert.Equal("production-analytic-replay-inputs-materialized", package.MaterializationAudit.TerminalStatus);
        Assert.Equal("raw-weak-coupling-matrix-element-evidence-validated", package.EvidenceBuild.EvidenceValidation.TerminalStatus);
        Assert.Equal("phase12-boson-mode-0", package.BosonModeId);
        Assert.Equal(1.0, package.CouplingRecord.CouplingProxyMagnitude, precision: 12);
    }

    [Fact]
    public void Run_WithWrongBosonVectorLength_BlocksBeforeReplay()
    {
        var provenance = MakeProvenance();
        var result = SourceBackedAnalyticReplayPackageRunner.Run(
            packageId: "phase83-source-backed-package",
            sourceArtifactId: "phase12-mode-0.json",
            bosonModeJson:
            """
            {
              "modeId": "phase12-boson-mode-0",
              "modeVector": [1.0, 2.0]
            }
            """,
            gammas: BuildGammaBundle(provenance),
            cellCount: 2,
            spinorDim: 1,
            dimG: 1,
            edgeLengths: [1.0],
            cellsPerEdge: [[0, 1]],
            edgeDirections: [[1.0]],
            modeI: MakeMode("fermion-cell-0", [1.0, 0.0, 0.0, 0.0], provenance),
            modeJ: MakeMode("fermion-cell-1", [0.0, 0.0, 1.0, 0.0], provenance),
            provenanceId: "phase83:test-revision:cpu",
            provenance: provenance);

        Assert.Equal("source-backed-analytic-replay-package-blocked", result.TerminalStatus);
        Assert.Null(result.FullReplayPackage);
        Assert.Contains("perturbation vector length must be 1", result.ClosureRequirements);
    }

    private static GammaOperatorBundle BuildGammaBundle(ProvenanceMeta provenance)
    {
        var builder = new GammaMatrixBuilder();
        var signature = new CliffordSignature { Positive = 1, Negative = 0 };
        return builder.Build(
            signature,
            new GammaConventionSpec
            {
                ConventionId = "phase83-test-cl1",
                Signature = signature,
                Representation = "standard",
                SpinorDimension = 1,
                HasChirality = false,
            },
            provenance);
    }

    private static FermionModeRecord MakeMode(string id, double[] eigenvector, ProvenanceMeta provenance) => new()
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
        Provenance = provenance,
    };

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-29T00:00:00Z"),
        CodeRevision = "test-revision",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}

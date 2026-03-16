using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;
using Xunit;

namespace Gu.Phase4.Couplings.Tests;

public sealed class FiniteDifferenceDiracVariationBuilderTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-p12-variation",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static BackgroundRecord MakeBackground() => new()
    {
        BackgroundId = "bg-test",
        EnvironmentId = "env-test",
        BranchManifestId = "branch-test",
        GeometryFingerprint = "mesh-test",
        StateArtifactRef = "inline:test",
        ResidualNorm = 0.0,
        StationarityNorm = 0.0,
        AdmissibilityLevel = AdmissibilityLevel.B1,
        Metrics = new BackgroundMetrics
        {
            ResidualNorm = 0.0,
            StationarityNorm = 0.0,
            ObjectiveValue = 0.0,
            GaugeViolation = 0.0,
            SolverIterations = 0,
            SolverConverged = true,
            TerminationReason = "unit-test",
            GaussNewtonValid = true,
        },
        ReplayTierAchieved = "R1",
        Provenance = TestProvenance(),
    };

    private static SpinorRepresentationSpec Dim2Spec() => SpinorSpecResolver.BuildDerivedSpec(2, 2, TestProvenance(), spinorSpecId: "spinor-dim2");

    [Fact]
    public void BuildVariation_NonzeroPerturbation_ProducesFiniteDifferenceMatrix()
    {
        var mesh = SingleTriangle();
        var background = MakeBackground();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildChiralSplitLayout("layout-dim2", spec, 1, TestProvenance());
        var gammas = new GammaMatrixBuilder().Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());
        var builder = new FiniteDifferenceDiracVariationBuilder(new CpuSpinConnectionBuilder(), new CpuDiracOperatorAssembler());
        var baseState = new double[] { 0.0, 0.0, 0.0 };
        var perturbation = new double[] { 1.0, 0.0, 0.0 };

        var result = builder.BuildVariation(
            variationId: "variation-b-0",
            bosonModeId: "boson-0",
            background: background,
            baseBosonicState: baseState,
            bosonPerturbation: perturbation,
            epsilon: 1e-5,
            spinorSpec: spec,
            layout: layout,
            gammas: gammas,
            mesh: mesh,
            provenance: TestProvenance());

        Assert.False(result.Variation.Blocked);
        Assert.NotNull(result.RealMatrix);
        Assert.NotNull(result.ImagMatrix);
        Assert.Contains(result.RealMatrix!.Cast<double>(), value => value != 0.0);
        Assert.Equal("finite-difference", result.Variation.VariationMethod);
        Assert.Equal("variation-b-0", result.Variation.VariationId);
    }

    [Fact]
    public void ExpandExplicitMatrix_RoundTripsFlatStorage()
    {
        var flat = new double[]
        {
            1.0, 0.0,
            2.0, -1.0,
            3.0, 1.0,
            4.0, 0.5,
        };

        var (re, im) = FiniteDifferenceDiracVariationBuilder.ExpandExplicitMatrix(flat, totalDof: 2);

        Assert.Equal(1.0, re[0, 0], 12);
        Assert.Equal(0.0, im[0, 0], 12);
        Assert.Equal(2.0, re[0, 1], 12);
        Assert.Equal(-1.0, im[0, 1], 12);
        Assert.Equal(3.0, re[1, 0], 12);
        Assert.Equal(1.0, im[1, 0], 12);
        Assert.Equal(4.0, re[1, 1], 12);
        Assert.Equal(0.5, im[1, 1], 12);
    }
}

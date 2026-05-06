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
    public void ComputeAnalytical_MatchesFiniteDifferenceAssemblerConvention()
    {
        var mesh = SingleTriangle();
        var background = MakeBackground();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildChiralSplitLayout("layout-dim2", spec, 3, TestProvenance());
        var gammas = new GammaMatrixBuilder().Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());
        var builder = new FiniteDifferenceDiracVariationBuilder(new CpuSpinConnectionBuilder(), new CpuDiracOperatorAssembler());
        var baseState = new double[mesh.EdgeCount * 3];
        var perturbation = new double[mesh.EdgeCount * 3];
        perturbation[0] = 0.7;
        perturbation[4] = -0.2;
        perturbation[8] = 0.5;

        var finiteDifference = builder.BuildVariation(
            variationId: "variation-b-analytic-check",
            bosonModeId: "boson-analytic-check",
            background: background,
            baseBosonicState: baseState,
            bosonPerturbation: perturbation,
            epsilon: 1e-5,
            spinorSpec: spec,
            layout: layout,
            gammas: gammas,
            mesh: mesh,
            provenance: TestProvenance());

        var (edgeLengths, cellsPerEdge, edgeDirections) = GeometryArrays(mesh);
        var analytic = DiracVariationComputer.ComputeAnalytical(
            perturbation,
            gammas,
            mesh.VertexCount,
            spec.SpinorComponents,
            dimG: 3,
            edgeLengths,
            cellsPerEdge,
            edgeDirections);

        Assert.False(finiteDifference.Variation.Blocked);
        Assert.NotNull(finiteDifference.RealMatrix);
        Assert.NotNull(finiteDifference.ImagMatrix);
        AssertMatrixClose(finiteDifference.RealMatrix!, analytic.Re, 1e-9);
        AssertMatrixClose(finiteDifference.ImagMatrix!, analytic.Im, 1e-9);
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

    private static (double[] EdgeLengths, int[][] CellsPerEdge, double[][] EdgeDirections) GeometryArrays(SimplicialMesh mesh)
    {
        var edgeLengths = new double[mesh.EdgeCount];
        var cellsPerEdge = new int[mesh.EdgeCount][];
        var edgeDirections = new double[mesh.EdgeCount][];
        for (int edge = 0; edge < mesh.EdgeCount; edge++)
        {
            edgeLengths[edge] = EdgeLength(mesh, edge);
            cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
            edgeDirections[edge] = EdgeDirection(mesh, edge);
        }

        return (edgeLengths, cellsPerEdge, edgeDirections);
    }

    private static double EdgeLength(SimplicialMesh mesh, int edge)
    {
        var a = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
        var b = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
        var sum = 0.0;
        for (var i = 0; i < a.Length; i++)
        {
            var d = b[i] - a[i];
            sum += d * d;
        }

        return System.Math.Sqrt(sum);
    }

    private static double[] EdgeDirection(SimplicialMesh mesh, int edge)
    {
        var a = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
        var b = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
        var direction = new double[a.Length];
        var norm = 0.0;
        for (var i = 0; i < a.Length; i++)
        {
            direction[i] = b[i] - a[i];
            norm += direction[i] * direction[i];
        }

        norm = System.Math.Sqrt(norm);
        if (norm > 1e-14)
            for (var i = 0; i < direction.Length; i++)
                direction[i] /= norm;
        return direction;
    }

    private static void AssertMatrixClose(double[,] expected, double[,] actual, double tolerance)
    {
        Assert.Equal(expected.GetLength(0), actual.GetLength(0));
        Assert.Equal(expected.GetLength(1), actual.GetLength(1));
        for (var row = 0; row < expected.GetLength(0); row++)
            for (var col = 0; col < expected.GetLength(1); col++)
                Assert.True(
                    System.Math.Abs(expected[row, col] - actual[row, col]) <= tolerance,
                    $"matrix[{row},{col}] expected {expected[row, col]:R}, actual {actual[row, col]:R}");
    }
}

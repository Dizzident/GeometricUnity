using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.GaugeReduction.Tests;

public class GaugeReductionWorkbenchTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    [Fact]
    public void PerformFullReduction_ProducesAllComponents()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var result = workbench.PerformFullReduction(bg);

        Assert.NotNull(result.Linearization);
        Assert.NotNull(result.Basis);
        Assert.NotNull(result.GaugeProjector);
        Assert.NotNull(result.PhysicalProjector);
        Assert.NotNull(result.DefectReport);

        Assert.Equal("bg-flat", result.Linearization.BackgroundId);
        Assert.Equal(6, result.Basis.Rank);
    }

    [Fact]
    public void ConstraintDefectReport_FlatBackground_NoDefect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var report = workbench.GenerateConstraintDefectReport(bg);

        Assert.Equal("bg-flat", report.BackgroundId);
        Assert.Equal(6, report.ExpectedRank);
        Assert.Equal(6, report.ComputedRank);
        Assert.False(report.HasDefect);
        Assert.Equal(0, report.RankDefect);
    }

    [Fact]
    public void ConstraintDefectReport_Serializes()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var report = workbench.GenerateConstraintDefectReport(bg);

        var json = System.Text.Json.JsonSerializer.Serialize(report);
        Assert.Contains("backgroundId", json);
        Assert.Contains("expectedRank", json);
        Assert.Contains("computedRank", json);
    }

    [Fact]
    public void GaugeLeakReport_DetectsGaugeDirections()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var linearization = new GaugeActionLinearization(mesh, algebra, bg);

        // Create gauge directions
        int dimG = algebra.Dimension;
        var eta1 = new double[linearization.GaugeParameterDimension];
        eta1[0] = 1.0;
        var etaTensor1 = new FieldTensor
        {
            Label = "eta1",
            Signature = linearization.Operator.InputSignature,
            Coefficients = eta1,
            Shape = new[] { eta1.Length },
        };
        var gaugeDir1 = linearization.Apply(etaTensor1).Coefficients;

        // Create a physical direction
        var rng = new Random(42);
        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var projector = workbench.BuildGaugeProjector(bg);
        var randomVec = new double[projector.ConnectionDimension];
        for (int i = 0; i < randomVec.Length; i++)
            randomVec[i] = rng.NextDouble() * 2.0 - 1.0;
        var physDir = projector.ApplyPhysical(randomVec);

        var report = workbench.GenerateGaugeLeakReport(bg,
            new[] { gaugeDir1, physDir },
            vectorLabels: new[] { "gauge-dir", "phys-dir" });

        Assert.Equal(2, report.Entries.Count);
        Assert.True(report.Entries[0].LeakScore > 0.99, "Gauge direction should have high leak");
        Assert.True(report.Entries[1].LeakScore < 1e-8, "Physical direction should have low leak");
        Assert.Equal(6, report.GaugeRank);
    }

    [Fact]
    public void GaugeLeakReport_Serializes()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var rng = new Random(42);
        var trial = new double[mesh.EdgeCount * algebra.Dimension];
        for (int i = 0; i < trial.Length; i++)
            trial[i] = rng.NextDouble();

        var report = workbench.GenerateGaugeLeakReport(bg, new[] { trial });
        var json = System.Text.Json.JsonSerializer.Serialize(report);
        Assert.Contains("backgroundId", json);
        Assert.Contains("gaugeRank", json);
        Assert.Contains("leakScore", json);
    }

    [Fact]
    public void PhysicalProjector_AsILinearOperator_WorksCorrectly()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var physProj = workbench.BuildPhysicalProjector(bg);

        Assert.Equal(mesh.EdgeCount * algebra.Dimension, physProj.InputDimension);
        Assert.Equal(mesh.EdgeCount * algebra.Dimension, physProj.OutputDimension);

        var rng = new Random(42);
        var vCoeffs = new double[physProj.InputDimension];
        for (int i = 0; i < vCoeffs.Length; i++)
            vCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        var v = new FieldTensor
        {
            Label = "v",
            Signature = physProj.InputSignature,
            Coefficients = vCoeffs,
            Shape = new[] { vCoeffs.Length },
        };

        var pv = physProj.Apply(v);
        var ppv = physProj.Apply(pv);

        // Idempotent: P(P(v)) = P(v)
        for (int i = 0; i < vCoeffs.Length; i++)
            Assert.Equal(pv.Coefficients[i], ppv.Coefficients[i], 10);
    }

    [Fact]
    public void PhysicalProjector_SelfAdjoint()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var physProj = workbench.BuildPhysicalProjector(bg);

        var rng = new Random(42);
        var xCoeffs = new double[physProj.InputDimension];
        var yCoeffs = new double[physProj.InputDimension];
        for (int i = 0; i < xCoeffs.Length; i++)
        {
            xCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
            yCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        }

        var x = new FieldTensor
        {
            Label = "x",
            Signature = physProj.InputSignature,
            Coefficients = xCoeffs,
            Shape = new[] { xCoeffs.Length },
        };
        var y = new FieldTensor
        {
            Label = "y",
            Signature = physProj.InputSignature,
            Coefficients = yCoeffs,
            Shape = new[] { yCoeffs.Length },
        };

        var px = physProj.Apply(x);
        var py = physProj.Apply(y);

        double lhs = GaugeActionLinearizationTests.Dot(px.Coefficients, yCoeffs);
        double rhs = GaugeActionLinearizationTests.Dot(xCoeffs, py.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }

    [Fact]
    public void BuildLinearization_ReturnsConsistentData()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var lin = workbench.BuildLinearization(bg);

        Assert.Equal("bg-flat", lin.BackgroundId);
        Assert.Equal(mesh.VertexCount * algebra.Dimension, lin.GaugeParameterDimension);
        Assert.Equal(mesh.EdgeCount * algebra.Dimension, lin.ConnectionDimension);
    }
}

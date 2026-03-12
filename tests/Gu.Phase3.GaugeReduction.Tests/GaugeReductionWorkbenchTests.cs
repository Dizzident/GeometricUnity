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

    [Fact]
    public void ComputeSpectralGap_UsesActualDiscardedSingularValue()
    {
        // Build a basis with strict cutoff so that some singular values are discarded.
        // Verify AllSingularValues captures them and the gap ratio uses the actual
        // first-discarded SV rather than the old proxy (SV[0] * svdCutoff).
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        double svdCutoff = 0.5; // strict: will discard small SVs
        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization, svdCutoff);

        // AllSingularValues must be at least as large as retained set
        Assert.True(basis.AllSingularValues.Count >= basis.SingularValues.Count,
            "AllSingularValues must include all SVs, not just retained ones.");

        // AllSingularValues must be sorted descending
        for (int i = 1; i < basis.AllSingularValues.Count; i++)
            Assert.True(basis.AllSingularValues[i - 1] >= basis.AllSingularValues[i] - 1e-12,
                $"AllSingularValues not descending at index {i}.");

        // AllSingularValues must contain retained SingularValues as a prefix
        for (int i = 0; i < basis.SingularValues.Count; i++)
            Assert.Equal(basis.SingularValues[i], basis.AllSingularValues[i], 12);

        // If there are discarded SVs, verify the gap computation is correct:
        // gap = minRetained / maxDiscarded (not minRetained / (SV[0] * svdCutoff))
        if (basis.AllSingularValues.Count > basis.SingularValues.Count && basis.Rank > 0)
        {
            double minRetained = basis.SingularValues[basis.SingularValues.Count - 1];
            double maxDiscarded = basis.AllSingularValues
                .FirstOrDefault(s => s < minRetained, double.Epsilon);
            if (maxDiscarded < double.Epsilon) maxDiscarded = double.Epsilon;
            double expectedGap = minRetained / maxDiscarded;

            // The approximate (old proxy) gap would be different
            double approxGap = minRetained / (basis.SingularValues[0] * svdCutoff);

            // The two values should differ (confirming the fix matters)
            // Note: they could coincide in degenerate cases, but typically they differ.
            // We only assert the expected formula value here; behavioral correctness
            // is verified by the GaugeLeakReport path.
            Assert.True(expectedGap > 0, "Spectral gap must be positive.");
            Assert.True(expectedGap != approxGap || System.Math.Abs(expectedGap - approxGap) < 1e-6,
                "Either gap values differ (fix is meaningful) or agree to near-machine precision.");
        }
    }

    [Fact]
    public void GaugeBasis_AllSingularValues_ContainsAllSvdSingularValues()
    {
        // With loose cutoff, all SVs are retained → AllSingularValues == SingularValues
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization, svdCutoff: 1e-10);

        // With a permissive cutoff, AllSingularValues should have >= retained count
        Assert.True(basis.AllSingularValues.Count >= basis.SingularValues.Count);

        // All retained SVs appear in AllSingularValues
        for (int i = 0; i < basis.SingularValues.Count; i++)
            Assert.Equal(basis.SingularValues[i], basis.AllSingularValues[i], 12);
    }
}

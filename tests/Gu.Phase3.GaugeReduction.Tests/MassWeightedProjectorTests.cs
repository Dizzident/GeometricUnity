using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.GaugeReduction.Tests;

/// <summary>
/// Tests for M_state-weighted gauge projector (physicist constraint #8).
///
/// P_phys must be M_state-self-adjoint:
///   (P_phys x)^T M y = x^T M (P_phys y)
///
/// This requires M_state-orthonormal gauge basis Q:
///   Q_i^T M Q_j = delta_ij
/// </summary>
public class MassWeightedProjectorTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    [Fact]
    public void MassWeightedBasis_IsMOrthonormal()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        // Non-uniform mass weights
        int connDim = mesh.EdgeCount * algebra.Dimension;
        var massWeights = new double[connDim];
        var rng = new Random(42);
        for (int i = 0; i < connDim; i++)
            massWeights[i] = 0.5 + rng.NextDouble() * 2.0; // range [0.5, 2.5]

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var basis = workbench.BuildGaugeBasisWithMass(bg, massWeights);

        // Check M-orthonormality: Q_i^T M Q_j = delta_ij
        for (int i = 0; i < basis.Rank; i++)
        {
            for (int j = 0; j < basis.Rank; j++)
            {
                double dot = 0;
                for (int k = 0; k < connDim; k++)
                    dot += basis.Vectors[i][k] * massWeights[k] * basis.Vectors[j][k];
                double expected = (i == j) ? 1.0 : 0.0;
                Assert.Equal(expected, dot, 6);
            }
        }
    }

    [Fact]
    public void MassWeightedProjector_IsMSelfAdjoint()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        int connDim = mesh.EdgeCount * algebra.Dimension;
        var massWeights = new double[connDim];
        var rng = new Random(42);
        for (int i = 0; i < connDim; i++)
            massWeights[i] = 0.5 + rng.NextDouble() * 2.0;

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var projector = workbench.BuildGaugeProjectorWithMass(bg, massWeights);

        // Check M-self-adjointness: (P x)^T M y = x^T M (P y)
        var x = new double[connDim];
        var y = new double[connDim];
        for (int i = 0; i < connDim; i++)
        {
            x[i] = rng.NextDouble() * 2.0 - 1.0;
            y[i] = rng.NextDouble() * 2.0 - 1.0;
        }

        var px = projector.ApplyPhysical(x);
        var py = projector.ApplyPhysical(y);

        double lhs = MassDot(px, y, massWeights);
        double rhs = MassDot(x, py, massWeights);

        Assert.Equal(lhs, rhs, 8);
    }

    [Fact]
    public void MassWeightedProjector_GaugeDirectionHasHighLeak()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        int connDim = mesh.EdgeCount * algebra.Dimension;
        var massWeights = new double[connDim];
        for (int i = 0; i < connDim; i++)
            massWeights[i] = 1.0 + 0.5 * (i % 3); // non-uniform

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var projector = workbench.BuildGaugeProjectorWithMass(bg, massWeights);

        // Create a gauge direction
        var linearization = workbench.BuildLinearization(bg);
        var eta = new double[linearization.GaugeParameterDimension];
        eta[0] = 1.0;
        eta[4] = 0.7;
        var etaTensor = new FieldTensor
        {
            Label = "eta",
            Signature = linearization.Operator.InputSignature,
            Coefficients = eta,
            Shape = new[] { eta.Length },
        };
        var gaugeDir = linearization.Apply(etaTensor).Coefficients;

        double leak = projector.GaugeLeakScore(gaugeDir);
        Assert.True(leak > 0.99, $"Gauge direction leak should be ~1.0, got {leak:F6}");
    }

    [Fact]
    public void MassWeightedProjector_IsIdempotent()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        int connDim = mesh.EdgeCount * algebra.Dimension;
        var massWeights = new double[connDim];
        for (int i = 0; i < connDim; i++)
            massWeights[i] = 1.0 + i * 0.1;

        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var projector = workbench.BuildGaugeProjectorWithMass(bg, massWeights);

        var rng = new Random(42);
        var v = new double[connDim];
        for (int i = 0; i < connDim; i++)
            v[i] = rng.NextDouble() * 2.0 - 1.0;

        var pv = projector.ApplyPhysical(v);
        var ppv = projector.ApplyPhysical(pv);

        for (int i = 0; i < connDim; i++)
            Assert.Equal(pv[i], ppv[i], 8);
    }

    [Fact]
    public void GaugeLeakReport_ContainsSpectralGap()
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
        Assert.NotNull(report.SpectralGap);
        Assert.True(report.SpectralGap > 0, "Spectral gap should be positive");
    }

    private static double MassDot(double[] a, double[] b, double[] m)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * m[i] * b[i];
        return sum;
    }
}

using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.GaugeReduction.Tests;

public class GaugeBasisTests
{
    [Fact]
    public void Build_FlatBackground_DetectsExpectedRank()
    {
        // For single triangle with su(2): 3 vertices, dimG=3
        // Gauge params = 9, expected rank = 6 (constant transforms in kernel)
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization);

        // Expected rank = dimG * (VertexCount - 1) = 3 * 2 = 6
        Assert.Equal(6, basis.ExpectedRank);
        Assert.Equal(6, basis.Rank);
        Assert.Equal(0, basis.RankDefect);
        Assert.Equal(9, basis.ConnectionDimension);
    }

    [Fact]
    public void Build_FlatBackground_BasisVectorsAreOrthonormal()
    {
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization);

        // Check orthonormality: q_i^T q_j = delta_ij
        for (int i = 0; i < basis.Rank; i++)
        {
            for (int j = 0; j < basis.Rank; j++)
            {
                double dot = GaugeActionLinearizationTests.Dot(basis.Vectors[i], basis.Vectors[j]);
                double expected = (i == j) ? 1.0 : 0.0;
                Assert.Equal(expected, dot, 8);
            }
        }
    }

    [Fact]
    public void Build_SingularValues_AreDescending()
    {
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization);

        for (int i = 1; i < basis.SingularValues.Count; i++)
        {
            Assert.True(basis.SingularValues[i - 1] >= basis.SingularValues[i] - 1e-12,
                $"Singular values not descending: sigma[{i - 1}]={basis.SingularValues[i - 1]} < sigma[{i}]={basis.SingularValues[i]}");
        }
    }

    [Fact]
    public void Build_GaugeBasisSpansGaugeImage()
    {
        // Verify that applying Gamma_* to a random gauge parameter
        // projects fully onto the gauge basis (up to numerical precision)
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization);

        var rng = new Random(42);
        int dimG = algebra.Dimension;
        var etaCoeffs = new double[mesh.VertexCount * dimG];
        for (int i = 0; i < etaCoeffs.Length; i++)
            etaCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        var eta = new FieldTensor
        {
            Label = "eta",
            Signature = linearization.Operator.InputSignature,
            Coefficients = etaCoeffs,
            Shape = new[] { etaCoeffs.Length },
        };

        var gammaEta = linearization.Apply(eta);
        double totalNorm = L2Norm(gammaEta.Coefficients);

        // Project onto gauge basis
        var projected = new double[basis.ConnectionDimension];
        for (int k = 0; k < basis.Rank; k++)
        {
            double dot = GaugeActionLinearizationTests.Dot(basis.Vectors[k], gammaEta.Coefficients);
            for (int i = 0; i < basis.ConnectionDimension; i++)
                projected[i] += dot * basis.Vectors[k][i];
        }

        // Residual after projection should be near zero
        var residual = new double[basis.ConnectionDimension];
        for (int i = 0; i < basis.ConnectionDimension; i++)
            residual[i] = gammaEta.Coefficients[i] - projected[i];
        double residualNorm = L2Norm(residual);

        Assert.True(residualNorm / totalNorm < 1e-8,
            $"Gauge image not fully captured by basis: relative residual = {residualNorm / totalNorm:E6}");
    }

    [Fact]
    public void Build_WithStrictCutoff_ReducesRank()
    {
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);

        // With a very strict cutoff, some directions may be lost
        var basisLoose = GaugeBasis.Build(linearization, svdCutoff: 1e-10);
        var basisStrict = GaugeBasis.Build(linearization, svdCutoff: 0.5);

        // Strict cutoff should give <= rank of loose cutoff
        Assert.True(basisStrict.Rank <= basisLoose.Rank);
    }

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }
}

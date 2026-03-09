using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability.Tests;

public class CoulombSliceOperatorTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    [Fact]
    public void Dimensions_AreCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var gauge = new CoulombGaugePenalty(mesh, dimG, 1.0);
        var sliceOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId);

        Assert.Equal(mesh.EdgeCount * dimG, sliceOp.InputDimension);
        Assert.Equal(mesh.VertexCount * dimG, sliceOp.OutputDimension);
    }

    [Fact]
    public void Transpose_SatisfiesAdjointProperty()
    {
        // Test: <C*v, w> == <v, C^T*w>
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var gauge = new CoulombGaugePenalty(mesh, dimG, 1.0);
        var sliceOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId);

        var rng = new Random(42);

        var vCoeffs = new double[sliceOp.InputDimension];
        for (int i = 0; i < vCoeffs.Length; i++)
            vCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var v = new FieldTensor
        {
            Label = "v",
            Signature = sliceOp.InputSignature,
            Coefficients = vCoeffs,
            Shape = new[] { sliceOp.InputDimension },
        };

        var wCoeffs = new double[sliceOp.OutputDimension];
        for (int i = 0; i < wCoeffs.Length; i++)
            wCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var w = new FieldTensor
        {
            Label = "w",
            Signature = sliceOp.OutputSignature,
            Coefficients = wCoeffs,
            Shape = new[] { sliceOp.OutputDimension },
        };

        var cv = sliceOp.Apply(v);
        var ctw = sliceOp.ApplyTranspose(w);

        double lhs = Dot(cv.Coefficients, w.Coefficients);
        double rhs = Dot(v.Coefficients, ctw.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }

    [Fact]
    public void CStarR_EqualsNegativeGaugeLaplacian_AtFlatBackground()
    {
        // Physicist sanity check: C_* R_{z_*} = d^*(-d) = -Delta_gauge on 0-forms.
        // At flat background (A_*=A0=0), R(xi) = -d(xi), so C_* R(xi) = d^*(-d(xi)) = -Delta(xi).
        // Delta_gauge = d^* d is the graph Laplacian on 0-forms.
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var gauge = new CoulombGaugePenalty(mesh, dimG, 1.0);
        var sliceOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId);

        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var gaugeMap = new InfinitesimalGaugeMap(mesh, algebra, a0, omega);

        var rng = new Random(99);
        var xiCoeffs = new double[mesh.VertexCount * dimG];
        for (int i = 0; i < xiCoeffs.Length; i++)
            xiCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var xi = new FieldTensor
        {
            Label = "xi",
            Signature = gaugeMap.InputSignature,
            Coefficients = xiCoeffs,
            Shape = new[] { mesh.VertexCount, dimG },
        };

        // C_* R(xi) via composition
        var rXi = gaugeMap.Apply(xi);
        var cRxi = sliceOp.Apply(rXi);

        // -Delta(xi) = -d^*(d(xi)) computed directly
        var dXi = gauge.ApplyExteriorDerivative(xiCoeffs);
        var dStarDxi = gauge.ApplyCodifferential(dXi);
        var negLaplacian = new double[dStarDxi.Length];
        for (int i = 0; i < negLaplacian.Length; i++)
            negLaplacian[i] = -dStarDxi[i];

        // Should match: C_* R(xi) == -Delta(xi)
        for (int i = 0; i < cRxi.Coefficients.Length; i++)
        {
            Assert.Equal(negLaplacian[i], cRxi.Coefficients[i], 10);
        }
    }

    [Fact]
    public void GaugeLaplacian_IsInvertibleOnNonConstantModes()
    {
        // The gauge Laplacian Delta = d^* d on 0-forms should have:
        // - Kernel = constant gauge transformations (dim = dimG)
        // - Positive eigenvalues on the complement
        // We verify by checking that Delta applied to a non-constant xi gives non-zero output.
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var gauge = new CoulombGaugePenalty(mesh, dimG, 1.0);

        // Non-constant xi: different values on different vertices
        var xiCoeffs = new double[mesh.VertexCount * dimG];
        xiCoeffs[0 * dimG + 0] = 1.0;  // vertex 0
        xiCoeffs[1 * dimG + 0] = 0.0;  // vertex 1
        xiCoeffs[2 * dimG + 0] = 0.0;  // vertex 2

        var dXi = gauge.ApplyExteriorDerivative(xiCoeffs);
        var laplacianXi = gauge.ApplyCodifferential(dXi);

        double norm = 0;
        for (int i = 0; i < laplacianXi.Length; i++)
            norm += laplacianXi[i] * laplacianXi[i];
        norm = System.Math.Sqrt(norm);

        Assert.True(norm > 1e-10, $"Gauge Laplacian on non-constant xi should be non-zero, got norm = {norm:E6}");

        // Constant xi: same value on all vertices -> should give zero
        var constXi = new double[mesh.VertexCount * dimG];
        for (int v = 0; v < mesh.VertexCount; v++)
            constXi[v * dimG + 0] = 1.0;

        var dConstXi = gauge.ApplyExteriorDerivative(constXi);
        var lapConstXi = gauge.ApplyCodifferential(dConstXi);

        double constNorm = 0;
        for (int i = 0; i < lapConstXi.Length; i++)
            constNorm += lapConstXi[i] * lapConstXi[i];
        constNorm = System.Math.Sqrt(constNorm);

        Assert.True(constNorm < 1e-10, $"Gauge Laplacian on constant xi should be zero, got norm = {constNorm:E6}");
    }

    [Fact]
    public void CoulombSliceOperator_FlatA0_AgreesWithFlatCodifferential()
    {
        // Regression: when A0 is zero, covariant d_{A0}^* == flat d^*
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var gauge = new CoulombGaugePenalty(mesh, dimG, 1.0);

        // Flat operator (no A0)
        var flatOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId);

        // Covariant operator with zero A0
        var zeroA0 = new double[mesh.EdgeCount * dimG];
        var covariantOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId,
            algebra: algebra, backgroundConnection: zeroA0);

        var rng = new Random(123);
        var betaCoeffs = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < betaCoeffs.Length; i++)
            betaCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var beta = new FieldTensor
        {
            Label = "beta",
            Signature = flatOp.InputSignature,
            Coefficients = betaCoeffs,
            Shape = new[] { mesh.EdgeCount * dimG },
        };

        var flatResult = flatOp.Apply(beta);
        var covariantResult = covariantOp.Apply(beta);

        for (int i = 0; i < flatResult.Coefficients.Length; i++)
        {
            Assert.Equal(flatResult.Coefficients[i], covariantResult.Coefficients[i], 12);
        }

        // Also check transpose
        var phiCoeffs = new double[mesh.VertexCount * dimG];
        for (int i = 0; i < phiCoeffs.Length; i++)
            phiCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var phi = new FieldTensor
        {
            Label = "phi",
            Signature = flatOp.OutputSignature,
            Coefficients = phiCoeffs,
            Shape = new[] { mesh.VertexCount * dimG },
        };

        var flatTranspose = flatOp.ApplyTranspose(phi);
        var covariantTranspose = covariantOp.ApplyTranspose(phi);

        for (int i = 0; i < flatTranspose.Coefficients.Length; i++)
        {
            Assert.Equal(flatTranspose.Coefficients[i], covariantTranspose.Coefficients[i], 12);
        }
    }

    [Fact]
    public void CoulombSliceOperator_NonFlatA0_DifferentFromFlatCodifferential()
    {
        // For non-zero A0, the covariant codifferential should differ from flat
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var gauge = new CoulombGaugePenalty(mesh, dimG, 1.0);

        // Flat operator (no A0)
        var flatOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId);

        // Non-zero A0: put a non-trivial su(2) connection on each edge
        var rng = new Random(456);
        var a0 = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < a0.Length; i++)
            a0[i] = rng.NextDouble() * 2.0 - 1.0;

        var covariantOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId,
            algebra: algebra, backgroundConnection: a0);

        // Input beta must be chosen so the bracket [A0, beta] is non-zero.
        // For su(2), [T_1, T_2] = T_3 etc., so random A0 and beta will have non-zero bracket.
        var betaCoeffs = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < betaCoeffs.Length; i++)
            betaCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var beta = new FieldTensor
        {
            Label = "beta",
            Signature = flatOp.InputSignature,
            Coefficients = betaCoeffs,
            Shape = new[] { mesh.EdgeCount * dimG },
        };

        var flatResult = flatOp.Apply(beta);
        var covariantResult = covariantOp.Apply(beta);

        // The results should differ
        double diffNorm = 0;
        for (int i = 0; i < flatResult.Coefficients.Length; i++)
        {
            double d = flatResult.Coefficients[i] - covariantResult.Coefficients[i];
            diffNorm += d * d;
        }
        diffNorm = System.Math.Sqrt(diffNorm);

        Assert.True(diffNorm > 1e-10,
            $"Covariant and flat codifferential should differ for non-zero A0, got diff norm = {diffNorm:E6}");
    }

    [Fact]
    public void Transpose_SatisfiesAdjointProperty_WithNonFlatA0()
    {
        // Test: <C*v, w> == <v, C^T*w> with covariant operator
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        int dimG = algebra.Dimension;
        var gauge = new CoulombGaugePenalty(mesh, dimG, 1.0);

        var rng = new Random(789);
        var a0 = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < a0.Length; i++)
            a0[i] = rng.NextDouble() * 2.0 - 1.0;

        var sliceOp = new CoulombSliceOperator(gauge, mesh, dimG, algebra.BasisOrderId,
            algebra: algebra, backgroundConnection: a0);

        var vCoeffs = new double[sliceOp.InputDimension];
        for (int i = 0; i < vCoeffs.Length; i++)
            vCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var v = new FieldTensor
        {
            Label = "v",
            Signature = sliceOp.InputSignature,
            Coefficients = vCoeffs,
            Shape = new[] { sliceOp.InputDimension },
        };

        var wCoeffs = new double[sliceOp.OutputDimension];
        for (int i = 0; i < wCoeffs.Length; i++)
            wCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var w = new FieldTensor
        {
            Label = "w",
            Signature = sliceOp.OutputSignature,
            Coefficients = wCoeffs,
            Shape = new[] { sliceOp.OutputDimension },
        };

        var cv = sliceOp.Apply(v);
        var ctw = sliceOp.ApplyTranspose(w);

        double lhs = Dot(cv.Coefficients, w.Coefficients);
        double rhs = Dot(v.Coefficients, ctw.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}

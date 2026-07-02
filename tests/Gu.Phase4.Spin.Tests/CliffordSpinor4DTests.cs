using System.Numerics;
using Gu.Geometry;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Spin.Tests;

/// <summary>
/// M2 acceptance batteries for the 4D Clifford / spinor layer:
/// CliffordAlgebraFactory (both signatures), SpinorField layout/arithmetic, and
/// the reduced Spin(4) invariant-slice projector.
///
/// The discrete Dirac operator is NOT a fresh type in this project (design §2.4
/// correction): it is the refined Gu.Phase4.Dirac assembler, tested in
/// tests/Gu.Phase4.Dirac.Tests/DiracEdgeVectorContraction4DTests.cs.
/// </summary>
public sealed class CliffordSpinor4DTests
{
    private const double Tol = 1e-14;

    // ---------------------------------------------------------------
    // 2.2 CliffordAlgebraFactory — both signatures
    // ---------------------------------------------------------------

    [Fact]
    public void CreateClifford4DRiemannian_HasDim4_ChiralityPresent_ValidatorPasses()
    {
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();

        Assert.Equal(4, bundle.Signature.Dimension);
        Assert.Equal(4, bundle.SpinorDimension);
        Assert.True(bundle.HasChiralityMatrix);
        Assert.NotNull(bundle.ChiralityMatrix);
        Assert.NotNull(bundle.ValidationResult);
        Assert.True(bundle.ValidationResult!.Passed, string.Join("; ", bundle.ValidationResult.DiagnosticNotes));
        Assert.True(bundle.ValidationResult.AnticommutationMaxError < 1e-12);
    }

    [Fact]
    public void CreateClifford4DLorentzian_HasDim4_ChiralityPresent_ValidatorPasses()
    {
        var bundle = CliffordAlgebraFactory.CreateClifford4DLorentzian();

        Assert.Equal(4, bundle.Signature.Dimension);
        Assert.Equal(3, bundle.Signature.Positive);
        Assert.Equal(1, bundle.Signature.Negative);
        Assert.Equal(4, bundle.SpinorDimension);
        Assert.True(bundle.HasChiralityMatrix);
        Assert.NotNull(bundle.ValidationResult);
        Assert.True(bundle.ValidationResult!.Passed, string.Join("; ", bundle.ValidationResult.DiagnosticNotes));
    }

    [Fact]
    public void CreateClifford4D_RejectsNon4DSignature()
    {
        Assert.Throws<ArgumentException>(() =>
            CliffordAlgebraFactory.CreateClifford4D(new CliffordSignature { Positive = 5, Negative = 0 }));
    }

    [Fact]
    public void GammaAnticommutation_Exact_Riemannian()
    {
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();
        var g = bundle.GammaMatrices;
        int s = bundle.SpinorDimension;

        for (int a = 0; a < 4; a++)
            for (int b = 0; b < 4; b++)
            {
                double expected = a == b ? 2.0 : 0.0; // eta = diag(+,+,+,+)
                double err = AnticommutatorMinusScaledIdentityNorm(g[a], g[b], expected, s);
                Assert.True(err < Tol, $"{{gamma[{a}], gamma[{b}]}} deviates by {err:E3}");
            }
    }

    [Fact]
    public void GammaAnticommutation_Exact_Lorentzian_WithEta()
    {
        var bundle = CliffordAlgebraFactory.CreateClifford4DLorentzian();
        var g = bundle.GammaMatrices;
        int s = bundle.SpinorDimension;

        // eta = diag(+,+,+,-): directions 0..2 positive, direction 3 negative.
        for (int a = 0; a < 4; a++)
            for (int b = 0; b < 4; b++)
            {
                double expected = 0.0;
                if (a == b) expected = a < bundle.Signature.Positive ? 2.0 : -2.0;
                double err = AnticommutatorMinusScaledIdentityNorm(g[a], g[b], expected, s);
                Assert.True(err < Tol, $"Lorentzian {{gamma[{a}], gamma[{b}]}} deviates by {err:E3}");
            }
    }

    [Fact]
    public void ChiralityMatrix_SquaresToIdentity_AndAnticommutesWithGammas()
    {
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();
        var gc = bundle.ChiralityMatrix!;

        // Gamma_chi^2 = I
        var sq = MatMul(gc, gc);
        Assert.True(FrobeniusDiffFromIdentity(sq) < Tol);

        // {Gamma_chi, gamma_mu} = 0
        for (int mu = 0; mu < 4; mu++)
        {
            var anti = MatAdd(MatMul(gc, bundle.GammaMatrices[mu]), MatMul(bundle.GammaMatrices[mu], gc));
            Assert.True(FrobeniusNorm(anti) < Tol, $"{{Gamma_chi, gamma[{mu}]}} != 0");
        }
    }

    [Fact]
    public void ChiralityMatrix_SquaresToIdentity_Lorentzian()
    {
        // The shared GammaMatrixBuilder gives Gamma_chi^2 = -I for Cl(3,1); the
        // factory re-phases so Gamma_chi^2 = I holds for BOTH signatures (design §2.2).
        var bundle = CliffordAlgebraFactory.CreateClifford4DLorentzian();
        var gc = bundle.ChiralityMatrix!;
        var sq = MatMul(gc, gc);
        Assert.True(FrobeniusDiffFromIdentity(sq) < Tol);
    }

    // ---------------------------------------------------------------
    // 2.3 SpinorField — layout round-trip + arithmetic
    // ---------------------------------------------------------------

    [Fact]
    public void SpinorField_LayoutRoundTrips()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var field = new SpinorField(mesh, spinorDimension: 4, gaugeComponents: 2);
        Assert.Equal(mesh.VertexCount * 2 * 4, field.Values.Length);

        for (int v = 0; v < mesh.VertexCount; v++)
            for (int gc = 0; gc < 2; gc++)
                for (int sp = 0; sp < 4; sp++)
                    field.Values[field.Index(v, gc, sp)] = new Complex(v * 100 + gc * 10 + sp, -(v + 1));

        for (int v = 0; v < mesh.VertexCount; v++)
        {
            var block = field.GetVertexSpinor(v);
            for (int gc = 0; gc < 2; gc++)
                for (int sp = 0; sp < 4; sp++)
                    Assert.Equal(field.Values[field.Index(v, gc, sp)], block[gc * 4 + sp]);
        }
    }

    [Fact]
    public void SpinorField_Arithmetic()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var a = new SpinorField(mesh, 4);
        var b = new SpinorField(mesh, 4);
        for (int i = 0; i < a.Values.Length; i++)
        {
            a.Values[i] = new Complex(i, 1);
            b.Values[i] = new Complex(2 * i, -1);
        }

        var sum = a.Add(b);
        var diff = b.Subtract(a);
        var scaled = a.Scale(new Complex(0, 2));
        for (int i = 0; i < a.Values.Length; i++)
        {
            Assert.Equal(a.Values[i] + b.Values[i], sum.Values[i]);
            Assert.Equal(b.Values[i] - a.Values[i], diff.Values[i]);
            Assert.Equal(a.Values[i] * new Complex(0, 2), scaled.Values[i]);
        }

        var ipp = a.InnerProduct(a);
        Assert.True(System.Math.Abs(ipp.Imaginary) < 1e-9);
        Assert.True(ipp.Real >= 0);
    }

    [Fact]
    public void SpinorField_RejectsWrongLength()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        Assert.Throws<ArgumentException>(() => new SpinorField(mesh, 4, 1, new Complex[3]));
    }

    // ---------------------------------------------------------------
    // SpinorDiracOperator — SpinorField-native reference/probe (delegates the
    // unit edge-gamma to the shared EdgeGammaContraction). NOT the production
    // fermion operator (that is the Gu.Phase4.Dirac assembler).
    // ---------------------------------------------------------------

    [Fact]
    public void FrameGamma_UnitAxisEdge_EqualsGammaMu()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();
        var dirac = new SpinorDiracOperator(mesh, bundle);

        for (int mu = 0; mu < 4; mu++)
        {
            int edge = FindUnitAxisEdge(mesh, mu);
            Assert.True(edge >= 0, $"no unit axis-{mu} edge found");
            var g = dirac.FrameGamma(edge)!;
            Assert.True(FrobeniusNorm(MatSub(g, bundle.GammaMatrices[mu])) < Tol,
                $"Gamma_hat(unit axis-{mu} edge) != gamma[{mu}]");
        }
    }

    [Fact]
    public void FrameGamma_SatisfiesUnitCliffordRelation_OnEveryEdge()
    {
        // With the UNIT contraction ê·Γ, {Gamma_hat, Gamma_hat} = 2 |ê|^2 I = 2 I
        // on every edge (axis-aligned AND diagonal), independent of |e|.
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();
        var dirac = new SpinorDiracOperator(mesh, bundle);
        int s = bundle.SpinorDimension;

        for (int edge = 0; edge < mesh.EdgeCount; edge++)
        {
            var g = dirac.FrameGamma(edge)!;
            var anti = MatAdd(MatMul(g, g), MatMul(g, g)); // {G,G} = 2 G^2
            double err = FrobeniusNormMinusScaledIdentity(anti, 2.0, s);
            Assert.True(err < Tol, $"edge {edge}: {{Gamma_hat,Gamma_hat}} != 2 I (err {err:E3})");
        }
    }

    [Fact]
    public void Apply_ConstantSpinor_ReturnsMassTermOnly()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();

        var psi = new SpinorField(mesh, 4);
        var constBlock = new Complex[] { new(1, 0), new(0, 1), new(-2, 0.5), new(3, -1) };
        for (int v = 0; v < mesh.VertexCount; v++)
            for (int sp = 0; sp < 4; sp++)
                psi.Values[psi.Index(v, 0, sp)] = constBlock[sp];

        var massless = new SpinorDiracOperator(mesh, bundle, mass: 0.0).Apply(psi);
        for (int i = 0; i < massless.Values.Length; i++)
            Assert.True(massless.Values[i].Magnitude < Tol, $"massless hop on constant != 0 at {i}");

        double m = 1.75;
        var massive = new SpinorDiracOperator(mesh, bundle, mass: m).Apply(psi);
        for (int i = 0; i < massive.Values.Length; i++)
            Assert.True((massive.Values[i] - m * psi.Values[i]).Magnitude < Tol, $"massive on constant != m psi at {i}");
    }

    [Fact]
    public void HoppingMatrix_IsAntiHermitian_SoITimesItIsHermitian()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();
        var dirac = new SpinorDiracOperator(mesh, bundle);

        var h = dirac.BuildHoppingMatrix();
        int dim = h.GetLength(0);
        Assert.Equal(mesh.VertexCount * bundle.SpinorDimension, dim);

        double antiHermResidual = 0;
        double hermResidual = 0;
        for (int r = 0; r < dim; r++)
            for (int c = 0; c < dim; c++)
            {
                Complex sumWithAdjoint = h[r, c] + Complex.Conjugate(h[c, r]);
                antiHermResidual = System.Math.Max(antiHermResidual, sumWithAdjoint.Magnitude);

                Complex iH_rc = Complex.ImaginaryOne * h[r, c];
                Complex iH_cr = Complex.ImaginaryOne * h[c, r];
                Complex diff = iH_rc - Complex.Conjugate(iH_cr);
                hermResidual = System.Math.Max(hermResidual, diff.Magnitude);
            }

        Assert.True(antiHermResidual <= Tol, $"hopping matrix not anti-Hermitian: residual {antiHermResidual:E3}");
        Assert.True(hermResidual <= Tol, $"i*hopping not Hermitian: residual {hermResidual:E3}");
    }

    [Fact]
    public void DiracOperator_WorksOnLowerDimMesh()
    {
        // Reference operator must work against ANY SimplicialMesh (design §2.4).
        var mesh = SimplicialMeshGenerator.CreateUniform3D(1);
        var bundle = CliffordAlgebraFactory.CreateClifford4DRiemannian();
        var dirac = new SpinorDiracOperator(mesh, bundle);
        var psi = new SpinorField(mesh, 4);
        for (int i = 0; i < psi.Values.Length; i++) psi.Values[i] = new Complex(i % 5, i % 3);
        var result = dirac.Apply(psi);
        Assert.Equal(psi.Values.Length, result.Values.Length);
    }

    // ---------------------------------------------------------------
    // 2.5 ReducedCliffordSliceProjector
    // ---------------------------------------------------------------

    [Fact]
    public void Projector_HodgeInvolution_SquaresToIdentity()
    {
        var h = ReducedCliffordSliceProjector.HodgeInvolution();
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
            {
                double acc = 0;
                for (int k = 0; k < 6; k++) acc += h[i, k] * h[k, j];
                Assert.Equal(i == j ? 1.0 : 0.0, acc, 12);
            }
    }

    [Theory]
    [InlineData("self-dual")]
    [InlineData("anti-self-dual")]
    public void Projector_IsIdempotent_AndInvariantDimensionIsThree(string channel)
    {
        var proj = new ReducedCliffordSliceProjector(channel);
        Assert.Equal(3, proj.InvariantDimension);
        Assert.Equal(6, proj.RawDimension);

        var rng = new Random(1234);
        var raw = new Complex[6];
        for (int i = 0; i < 6; i++) raw[i] = new Complex(rng.NextDouble(), rng.NextDouble());

        var once = proj.Project(raw);
        var twice = proj.Project(once);
        for (int i = 0; i < 6; i++)
            Assert.True((once[i] - twice[i]).Magnitude < 1e-13, $"P^2 != P at {i}");

        Assert.True(once.Zip(proj.Project(once), (x, y) => (x - y).Magnitude).Max() < 1e-13);
    }

    [Fact]
    public void Projector_SelfAndAntiSelfDual_AreComplementary()
    {
        var pPlus = new ReducedCliffordSliceProjector("self-dual");
        var pMinus = new ReducedCliffordSliceProjector("anti-self-dual");

        var rng = new Random(99);
        var raw = new Complex[6];
        for (int i = 0; i < 6; i++) raw[i] = new Complex(rng.NextDouble(), rng.NextDouble());

        var a = pPlus.Project(raw);
        var b = pMinus.Project(raw);
        for (int i = 0; i < 6; i++)
            Assert.True((a[i] + b[i] - raw[i]).Magnitude < 1e-13, $"P_+ + P_- != I at {i}");
    }

    [Fact]
    public void Projector_MultiComponent_IdempotentAndDimensionScales()
    {
        var proj = new ReducedCliffordSliceProjector("self-dual", componentsPerElement: 3);
        Assert.Equal(9, proj.InvariantDimension);   // 3 per ad component * 3
        Assert.Equal(18, proj.RawDimension);         // 6 * 3

        var rng = new Random(7);
        var raw = new Complex[18];
        for (int i = 0; i < 18; i++) raw[i] = new Complex(rng.NextDouble(), rng.NextDouble());

        var once = proj.Project(raw);
        var twice = proj.Project(once);
        for (int i = 0; i < 18; i++)
            Assert.True((once[i] - twice[i]).Magnitude < 1e-13, $"multi-component P^2 != P at {i}");
    }

    [Fact]
    public void Projector_RejectsBadChannelAndLength()
    {
        Assert.Throws<ArgumentException>(() => new ReducedCliffordSliceProjector("bogus"));
        var proj = new ReducedCliffordSliceProjector("self-dual");
        Assert.Throws<ArgumentException>(() => proj.Project(new Complex[5]));
    }

    // ---------------------------------------------------------------
    // helpers
    // ---------------------------------------------------------------

    private static int FindUnitAxisEdge(SimplicialMesh mesh, int axis)
    {
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            int[] ep = mesh.Edges[e];
            var c0 = mesh.GetVertexCoordinates(ep[0]);
            var c1 = mesh.GetVertexCoordinates(ep[1]);
            bool ok = true;
            for (int mu = 0; mu < mesh.EmbeddingDimension; mu++)
            {
                double dx = c1[mu] - c0[mu];
                double want = mu == axis ? 1.0 : 0.0;
                if (System.Math.Abs(dx - want) > 1e-12) { ok = false; break; }
            }
            if (ok) return e;
        }
        return -1;
    }

    private static double AnticommutatorMinusScaledIdentityNorm(Complex[,] a, Complex[,] b, double scale, int s)
        => FrobeniusNormMinusScaledIdentity(MatAdd(MatMul(a, b), MatMul(b, a)), scale, s);

    private static double FrobeniusNormMinusScaledIdentity(Complex[,] m, double scale, int s)
    {
        double sum = 0;
        for (int i = 0; i < s; i++)
            for (int j = 0; j < s; j++)
            {
                Complex expected = i == j ? new Complex(scale, 0) : Complex.Zero;
                Complex d = m[i, j] - expected;
                sum += d.Real * d.Real + d.Imaginary * d.Imaginary;
            }
        return System.Math.Sqrt(sum);
    }

    private static Complex[,] MatSub(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = a[i, j] - b[i, j];
        return r;
    }

    private static Complex[,] MatMul(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                Complex acc = Complex.Zero;
                for (int k = 0; k < n; k++) acc += a[i, k] * b[k, j];
                r[i, j] = acc;
            }
        return r;
    }

    private static Complex[,] MatAdd(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = a[i, j] + b[i, j];
        return r;
    }

    private static double FrobeniusNorm(Complex[,] m)
    {
        int rows = m.GetLength(0), cols = m.GetLength(1);
        double sum = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                sum += m[i, j].Real * m[i, j].Real + m[i, j].Imaginary * m[i, j].Imaginary;
        return System.Math.Sqrt(sum);
    }

    private static double FrobeniusDiffFromIdentity(Complex[,] m)
    {
        int n = m.GetLength(0);
        double sum = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                Complex d = m[i, j] - (i == j ? Complex.One : Complex.Zero);
                sum += d.Real * d.Real + d.Imaginary * d.Imaginary;
            }
        return System.Math.Sqrt(sum);
    }
}

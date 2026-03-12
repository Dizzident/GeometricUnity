using System.Numerics;
using System.Text.Json;
using Gu.Core;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Spin.Tests;

/// <summary>
/// Tests for the GammaMatrixBuilder and CliffordAlgebraValidator.
/// Validates anticommutation relations, chirality operator, Hermiticity,
/// and serialization round-trips for the spinor convention types.
/// </summary>
public class GammaMatrixBuilderTests
{
    private static ProvenanceMeta TestProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static GammaConventionSpec MakeConvention(int dim, bool hasChirality) =>
        new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = dim, Negative = 0 },
            Representation = "standard",
            SpinorDimension = GammaMatrixBuilder.SpinorDimension(dim),
            HasChirality = hasChirality,
            AnticommutationTolerance = 1e-12,
        };

    // ─── SpinorDimension ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(4, 4)]
    [InlineData(5, 4)]   // toy dimY
    [InlineData(6, 8)]
    [InlineData(14, 128)] // physical dimY
    public void SpinorDimension_IsCorrectPowerOfTwo(int dimY, int expectedSpinorDim)
    {
        Assert.Equal(expectedSpinorDim, GammaMatrixBuilder.SpinorDimension(dimY));
    }

    // ─── Anticommutation for Cl(2,0) ─────────────────────────────────────────

    [Fact]
    public void Cl2_Riemannian_AnticommutationHolds()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 2, Negative = 0 };
        var conv = MakeConvention(2, hasChirality: true);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var validator = new CliffordAlgebraValidator();
        var result = validator.Validate(bundle, 1e-12);

        Assert.True(result.Passed,
            $"Cl(2,0) validation failed: {string.Join("; ", result.DiagnosticNotes)}");
        Assert.Equal(2, bundle.SpinorDimension);
    }

    // ─── Anticommutation for Cl(4,0) ─────────────────────────────────────────

    [Fact]
    public void Cl4_Riemannian_AnticommutationHolds()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 4, Negative = 0 };
        var conv = MakeConvention(4, hasChirality: true);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var validator = new CliffordAlgebraValidator();
        var result = validator.Validate(bundle, 1e-12);

        Assert.True(result.Passed,
            $"Cl(4,0) validation failed: {string.Join("; ", result.DiagnosticNotes)}");
        Assert.Equal(4, bundle.SpinorDimension);
        Assert.Equal(4, bundle.GammaMatrices.Length);
    }

    // ─── Toy dimY=5 ──────────────────────────────────────────────────────────

    [Fact]
    public void Cl5_Riemannian_AnticommutationHolds()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 5, Negative = 0 };
        var conv = MakeConvention(5, hasChirality: false);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var validator = new CliffordAlgebraValidator();
        var result = validator.Validate(bundle, 1e-12);

        Assert.True(result.Passed,
            $"Cl(5,0) validation failed: {string.Join("; ", result.DiagnosticNotes)}");
        Assert.Equal(4, bundle.SpinorDimension);  // 2^floor(5/2) = 4
        Assert.Equal(5, bundle.GammaMatrices.Length);
        // Odd dimension: no chirality matrix
        Assert.Null(bundle.ChiralityMatrix);
        Assert.False(bundle.HasChiralityMatrix);
    }

    [Fact]
    public void Cl5_FifthGamma_SquaresToIdentity()
    {
        // For odd n: Gamma_n^2 = I (it was constructed as the chirality of Cl(n-1))
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 5, Negative = 0 };
        var conv = MakeConvention(5, hasChirality: false);
        var bundle = builder.Build(sig, conv, TestProvenance());

        var g5 = bundle.GammaMatrices[4];
        var g5sq = MatMul(g5, g5);
        var ident = Identity(4);
        double err = FrobNorm(MatSub(g5sq, ident));
        Assert.True(err < 1e-12, $"Gamma_5^2 != I: err={err:E3}");
    }

    // ─── Even dimY: chirality operator ───────────────────────────────────────

    [Fact]
    public void Cl4_ChiralityMatrix_SquaresToIdentity()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 4, Negative = 0 };
        var conv = MakeConvention(4, hasChirality: true);
        var bundle = builder.Build(sig, conv, TestProvenance());

        Assert.NotNull(bundle.ChiralityMatrix);
        var gc = bundle.ChiralityMatrix!;
        var gcSq = MatMul(gc, gc);
        double err = FrobNorm(MatSub(gcSq, Identity(4)));
        Assert.True(err < 1e-12, $"Gamma_chi^2 != I for Cl(4,0): err={err:E3}");
    }

    [Fact]
    public void Cl4_ChiralityMatrix_AnticommutesWithAllGammas()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 4, Negative = 0 };
        var conv = MakeConvention(4, hasChirality: true);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var gc = bundle.ChiralityMatrix!;

        for (int mu = 0; mu < 4; mu++)
        {
            var g = bundle.GammaMatrices[mu];
            var anticomm = MatAdd(MatMul(gc, g), MatMul(g, gc));
            double err = FrobNorm(anticomm);
            Assert.True(err < 1e-12, $"{{Gamma_chi, Gamma_{mu}}} != 0: err={err:E3}");
        }
    }

    [Fact]
    public void Cl4_ChiralityMatrix_IsHermitian()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 4, Negative = 0 };
        var conv = MakeConvention(4, hasChirality: true);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var gc = bundle.ChiralityMatrix!;

        double err = FrobNorm(MatSub(gc, ConjTranspose(gc)));
        Assert.True(err < 1e-12, $"Gamma_chi not Hermitian: err={err:E3}");
    }

    // ─── Hermiticity of individual gammas (Riemannian) ───────────────────────

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    public void Cl_Riemannian_GammasAreHermitian(int dimY)
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = dimY, Negative = 0 };
        var conv = MakeConvention(dimY, hasChirality: dimY % 2 == 0);
        var bundle = builder.Build(sig, conv, TestProvenance());

        for (int mu = 0; mu < dimY; mu++)
        {
            var g = bundle.GammaMatrices[mu];
            double err = FrobNorm(MatSub(g, ConjTranspose(g)));
            Assert.True(err < 1e-12,
                $"Gamma_{mu} not Hermitian for Cl({dimY},0): err={err:E3}");
        }
    }

    // ─── GammaMatrices squared (Riemannian: Gamma_mu^2 = I) ──────────────────

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public void Cl_Riemannian_GammasSquareToIdentity(int dimY)
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = dimY, Negative = 0 };
        var conv = MakeConvention(dimY, hasChirality: dimY % 2 == 0);
        var bundle = builder.Build(sig, conv, TestProvenance());
        int s = GammaMatrixBuilder.SpinorDimension(dimY);

        for (int mu = 0; mu < dimY; mu++)
        {
            var g = bundle.GammaMatrices[mu];
            var gsq = MatMul(g, g);
            double err = FrobNorm(MatSub(gsq, Identity(s)));
            Assert.True(err < 1e-12,
                $"Gamma_{mu}^2 != I for Cl({dimY},0): err={err:E3}");
        }
    }

    // ─── Trace of individual gammas (Riemannian: Tr(Gamma_mu) = 0) ──────────

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public void Cl_Riemannian_GammasHaveZeroTrace(int dimY)
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = dimY, Negative = 0 };
        var conv = MakeConvention(dimY, hasChirality: dimY % 2 == 0);
        var bundle = builder.Build(sig, conv, TestProvenance());

        for (int mu = 0; mu < dimY; mu++)
        {
            var g = bundle.GammaMatrices[mu];
            Complex trace = Complex.Zero;
            int s = g.GetLength(0);
            for (int i = 0; i < s; i++) trace += g[i, i];
            Assert.True(Complex.Abs(trace) < 1e-12,
                $"Gamma_{mu} has nonzero trace {trace} for Cl({dimY},0)");
        }
    }

    // ─── Validator integration ────────────────────────────────────────────────

    [Fact]
    public void Validator_Cl4_Passes()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 4, Negative = 0 };
        var conv = MakeConvention(4, hasChirality: true);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var validator = new CliffordAlgebraValidator();
        var result = validator.Validate(bundle);

        Assert.True(result.Passed);
        Assert.Equal("dirac-tensor-product-v1", result.ConventionId);
        Assert.True(result.AnticommutationMaxError < 1e-12);
        Assert.True(result.ChiralitySquareError < 1e-12);
        Assert.True(result.ConjugationConsistencyError < 1e-12);
    }

    [Fact]
    public void Validator_Cl5_Passes_NoChirality()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 5, Negative = 0 };
        var conv = MakeConvention(5, hasChirality: false);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var validator = new CliffordAlgebraValidator();
        var result = validator.Validate(bundle);

        Assert.True(result.Passed);
        Assert.Equal(0.0, result.ChiralitySquareError);
        Assert.Equal(0.0, result.ChiralityAnticommutationMaxError);
    }

    // ─── Reproducibility ──────────────────────────────────────────────────────

    [Fact]
    public void Builder_IsReproducible_SameOutputForSameInput()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 4, Negative = 0 };
        var conv = MakeConvention(4, hasChirality: true);

        var bundle1 = builder.Build(sig, conv, TestProvenance());
        var bundle2 = builder.Build(sig, conv, TestProvenance());

        for (int mu = 0; mu < 4; mu++)
        {
            int s = bundle1.SpinorDimension;
            for (int r = 0; r < s; r++)
                for (int c = 0; c < s; c++)
                {
                    Assert.Equal(bundle1.GammaMatrices[mu][r, c].Real,
                                 bundle2.GammaMatrices[mu][r, c].Real, 15);
                    Assert.Equal(bundle1.GammaMatrices[mu][r, c].Imaginary,
                                 bundle2.GammaMatrices[mu][r, c].Imaginary, 15);
                }
        }
    }

    // ─── Serialization ────────────────────────────────────────────────────────

    [Fact]
    public void SpinorRepresentationSpec_SerializesAndDeserializes()
    {
        var spec = new SpinorRepresentationSpec
        {
            SpinorSpecId = "spinor-test-001",
            SpacetimeDimension = 5,
            CliffordSignature = new CliffordSignature { Positive = 5, Negative = 0 },
            GammaConvention = MakeConvention(5, hasChirality: false),
            ChiralityConvention = new ChiralityConventionSpec
            {
                ConventionId = "no-chirality-odd-dim",
                SignConvention = "left-is-minus",
                PhaseFactor = "n/a",
                HasChirality = false,
            },
            ConjugationConvention = new ConjugationConventionSpec
            {
                ConventionId = "hermitian-adjoint-v1",
                ConjugationType = "hermitian",
                HasChargeConjugation = true,
            },
            SpinorComponents = 4,
            ChiralitySplit = 0,
            InsertedAssumptionIds = new List<string> { "P4-IA-001", "P4-IA-002" },
            Provenance = TestProvenance(),
        };

        var json = JsonSerializer.Serialize(spec, new JsonSerializerOptions { WriteIndented = true });
        Assert.NotEmpty(json);
        Assert.Contains("spinorSpecId", json);
        Assert.Contains("spinor-test-001", json);

        var deserialized = JsonSerializer.Deserialize<SpinorRepresentationSpec>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("spinor-test-001", deserialized!.SpinorSpecId);
        Assert.Equal(5, deserialized.SpacetimeDimension);
        Assert.Equal(4, deserialized.SpinorComponents);
        Assert.Equal(0, deserialized.ChiralitySplit);
        Assert.Equal(2, deserialized.InsertedAssumptionIds.Count);
    }

    [Fact]
    public void GammaConventionSpec_SerializesAndDeserializes()
    {
        var conv = MakeConvention(4, hasChirality: true);
        var json = JsonSerializer.Serialize(conv);
        var deserialized = JsonSerializer.Deserialize<GammaConventionSpec>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("dirac-tensor-product-v1", deserialized!.ConventionId);
        Assert.Equal(4, deserialized.SpinorDimension);
        Assert.True(deserialized.HasChirality);
    }

    [Fact]
    public void CliffordValidationResult_SerializesAndDeserializes()
    {
        var result = new CliffordValidationResult
        {
            ConventionId = "dirac-tensor-product-v1",
            AnticommutationMaxError = 1e-15,
            ChiralitySquareError = 2e-15,
            ChiralityAnticommutationMaxError = 3e-15,
            ConjugationConsistencyError = 4e-15,
            Passed = true,
            Tolerance = 1e-12,
            DiagnosticNotes = new List<string>(),
        };

        var json = JsonSerializer.Serialize(result);
        var deserialized = JsonSerializer.Deserialize<CliffordValidationResult>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("dirac-tensor-product-v1", deserialized!.ConventionId);
        Assert.True(deserialized.Passed);
        Assert.Equal(1e-15, deserialized.AnticommutationMaxError, 15);
    }

    // ─── Cl(6,0) — larger even case ──────────────────────────────────────────

    [Fact]
    public void Cl6_Riemannian_FullValidation()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 6, Negative = 0 };
        var conv = MakeConvention(6, hasChirality: true);
        var bundle = builder.Build(sig, conv, TestProvenance());
        var validator = new CliffordAlgebraValidator();
        var result = validator.Validate(bundle);

        Assert.True(result.Passed,
            $"Cl(6,0) failed: {string.Join("; ", result.DiagnosticNotes)}");
        Assert.Equal(8, bundle.SpinorDimension);
        Assert.NotNull(bundle.ChiralityMatrix);
    }

    // ─── Matrix helpers (local) ───────────────────────────────────────────────

    private static Complex[,] MatMul(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                Complex s = Complex.Zero;
                for (int k = 0; k < n; k++) s += a[i, k] * b[k, j];
                r[i, j] = s;
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

    private static Complex[,] MatSub(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = a[i, j] - b[i, j];
        return r;
    }

    private static Complex[,] Identity(int n)
    {
        var m = new Complex[n, n];
        for (int i = 0; i < n; i++) m[i, i] = Complex.One;
        return m;
    }

    private static Complex[,] ConjTranspose(Complex[,] a)
    {
        int n = a.GetLength(0);
        var r = new Complex[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                r[i, j] = Complex.Conjugate(a[j, i]);
        return r;
    }

    private static double FrobNorm(Complex[,] m)
    {
        int n = m.GetLength(0);
        double s = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                s += m[i, j].Real * m[i, j].Real + m[i, j].Imaginary * m[i, j].Imaginary;
        return System.Math.Sqrt(s);
    }
}

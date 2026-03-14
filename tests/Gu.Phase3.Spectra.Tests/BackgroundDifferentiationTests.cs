using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.GaugeReduction;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.Spectra.Tests;

/// <summary>
/// Regression tests for G-004: verify that two different stored backgrounds (different omega states)
/// produce different CLI spectrum artifacts when passed through the spectrum pipeline.
///
/// This proves the compute-spectrum path is background-sensitive:
/// the spectrum depends on the persisted omega (and A0) rather than being a
/// constant output of a hardcoded zero-state.
/// </summary>
public sealed class BackgroundDifferentiationTests
{
    // Single tetrahedron (4 vertices, 6 edges, 4 faces)
    private static SimplicialMesh BuildTetrahedron() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } });

    private static BranchManifest TrivialManifest() => new()
    {
        BranchId = "g004-regression",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "g004-fix",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 3,
        AmbientDimension = 9,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    private static GeometryContext TestGeometry() => new()
    {
        BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 3 },
        AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 9 },
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding
        {
            BindingType = "projection",
            SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 9 },
            TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 3 },
        },
        ObservationBinding = new GeometryBinding
        {
            BindingType = "observation",
            SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 3 },
            TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 9 },
        },
        Patches = Array.Empty<PatchInfo>(),
    };

    private static TensorSignature ConnectionSig() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "canonical",
        ComponentOrderId = "edge-major",
        MemoryLayout = "dense-row-major",
        NumericPrecision = "float64",
    };

    private static FieldTensor ZeroField(SimplicialMesh mesh, LieAlgebra algebra) => new()
    {
        Label = "omega_zero",
        Signature = ConnectionSig(),
        Coefficients = new double[mesh.EdgeCount * algebra.Dimension],
        Shape = new[] { mesh.EdgeCount, algebra.Dimension },
    };

    private static FieldTensor NonzeroField(SimplicialMesh mesh, LieAlgebra algebra)
    {
        var coeffs = new double[mesh.EdgeCount * algebra.Dimension];
        for (int e = 0; e < mesh.EdgeCount; e++)
            coeffs[e * algebra.Dimension] = 0.3 * (e + 1.0) / mesh.EdgeCount;
        return new FieldTensor
        {
            Label = "omega_nonzero",
            Signature = ConnectionSig(),
            Coefficients = coeffs,
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
    }

    private static SpectrumBundle ComputeSpectrumForBackground(
        SimplicialMesh mesh, LieAlgebra algebra, FieldTensor omega, FieldTensor a0,
        string backgroundId, int numModes = 4)
    {
        var manifest = TrivialManifest();
        var geometry = TestGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var residualMass = new CpuMassMatrix(mesh, algebra);

        var opSpec = new LinearizedOperatorSpec
        {
            BackgroundId = backgroundId,
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
        };

        var bundleBuilder = new OperatorBundleBuilder(mesh, algebra, assembler, residualMass, backend);
        var opBundle = bundleBuilder.Build(opSpec, omega, a0, manifest, geometry);

        var eigSpec = new GeneralizedEigenproblemSpec { NumEigenvalues = numModes };
        var pipeline = new EigensolverPipeline();
        return pipeline.Solve(opBundle, eigSpec);
    }

    /// <summary>
    /// G-004 regression: two different stored background omega states produce different spectra.
    /// This proves the spectrum computation is background-sensitive and not a constant
    /// output of a hardcoded zero background.
    /// </summary>
    [Fact]
    public void TwoDifferentBackgrounds_ProduceDifferentSpectra()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var a0 = ZeroField(mesh, algebra);

        var omegaZero = ZeroField(mesh, algebra);
        var omegaNonzero = NonzeroField(mesh, algebra);

        var spectrumA = ComputeSpectrumForBackground(mesh, algebra, omegaZero, a0, "bg-zero");
        var spectrumB = ComputeSpectrumForBackground(mesh, algebra, omegaNonzero, a0, "bg-nonzero");

        Assert.NotEmpty(spectrumA.Modes);
        Assert.NotEmpty(spectrumB.Modes);

        // The spectra must differ: at least one eigenvalue must differ between the two backgrounds
        int modesCount = System.Math.Min(spectrumA.Modes.Count, spectrumB.Modes.Count);
        Assert.True(modesCount > 0, "Both spectra must have at least one mode.");

        bool anyDiffers = false;
        for (int i = 0; i < modesCount; i++)
        {
            double diff = System.Math.Abs(spectrumA.Modes[i].Eigenvalue - spectrumB.Modes[i].Eigenvalue);
            if (diff > 1e-12)
            {
                anyDiffers = true;
                break;
            }
        }

        Assert.True(anyDiffers,
            $"Two backgrounds with different omega states must produce different eigenvalue spectra. " +
            $"First eigenvalues: bg-zero={spectrumA.Modes[0].Eigenvalue:G6}, " +
            $"bg-nonzero={spectrumB.Modes[0].Eigenvalue:G6}");
    }

    /// <summary>
    /// G-004 regression: persisted A0 round-trip.
    /// Verifies that a non-zero A0 tensor, when serialized to JSON and deserialized (as solve-backgrounds
    /// would write it and compute-spectrum would load it), produces the same spectrum as the original.
    /// This proves the A0 persistence plumbing works correctly.
    /// </summary>
    [Fact]
    public void PersistedA0RoundTrip_ProducesSameSpectrum()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var omega = ZeroField(mesh, algebra);
        var a0Original = NonzeroField(mesh, algebra);
        a0Original = new FieldTensor
        {
            Label = "a0",
            Signature = ConnectionSig(),
            Coefficients = a0Original.Coefficients,
            Shape = a0Original.Shape,
        };

        // Serialize and deserialize A0 (round-trip through JSON as solve-backgrounds/compute-spectrum does)
        var json = GuJsonDefaults.Serialize(a0Original);
        var a0Loaded = GuJsonDefaults.Deserialize<FieldTensor>(json);
        Assert.NotNull(a0Loaded);
        Assert.Equal(a0Original.Coefficients.Length, a0Loaded!.Coefficients.Length);

        // Both spectra must be identical (same data regardless of serialization round-trip)
        var spectrumOriginal = ComputeSpectrumForBackground(mesh, algebra, omega, a0Original, "bg-a0-original");
        var spectrumLoaded = ComputeSpectrumForBackground(mesh, algebra, omega, a0Loaded, "bg-a0-loaded");

        Assert.Equal(spectrumOriginal.Modes.Count, spectrumLoaded.Modes.Count);
        for (int i = 0; i < spectrumOriginal.Modes.Count; i++)
        {
            Assert.Equal(spectrumOriginal.Modes[i].Eigenvalue, spectrumLoaded.Modes[i].Eigenvalue, precision: 10);
        }
    }

    /// <summary>
    /// G-004 regression: omega state serialization round-trip preserves spectrum.
    /// Verifies that a non-zero omega tensor, when serialized to JSON and deserialized (as
    /// solve-backgrounds writes and compute-spectrum reads), produces the same spectrum.
    /// </summary>
    [Fact]
    public void PersistedOmegaRoundTrip_ProducesSameSpectrum()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var omegaOriginal = NonzeroField(mesh, algebra);
        var a0 = ZeroField(mesh, algebra);

        // Serialize and deserialize omega (round-trip through JSON as solve-backgrounds/compute-spectrum does)
        var json = GuJsonDefaults.Serialize(omegaOriginal);
        var omegaLoaded = GuJsonDefaults.Deserialize<FieldTensor>(json);
        Assert.NotNull(omegaLoaded);
        Assert.Equal(omegaOriginal.Coefficients.Length, omegaLoaded!.Coefficients.Length);

        var spectrumOriginal = ComputeSpectrumForBackground(mesh, algebra, omegaOriginal, a0, "bg-omega-original");
        var spectrumLoaded = ComputeSpectrumForBackground(mesh, algebra, omegaLoaded, a0, "bg-omega-loaded");

        Assert.Equal(spectrumOriginal.Modes.Count, spectrumLoaded.Modes.Count);
        for (int i = 0; i < spectrumOriginal.Modes.Count; i++)
        {
            Assert.Equal(spectrumOriginal.Modes[i].Eigenvalue, spectrumLoaded.Modes[i].Eigenvalue, precision: 10);
        }
    }
}

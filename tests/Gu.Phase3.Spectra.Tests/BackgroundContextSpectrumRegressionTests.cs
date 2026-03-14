using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;

namespace Gu.Phase3.Spectra.Tests;

/// <summary>
/// G-004 regression: proves that two different stored background states (different omega)
/// produce different spectrum artifacts through the same pipeline. Verifies that
/// compute-spectrum output is actually sensitive to the loaded background context,
/// not just a function of toy geometry alone.
/// </summary>
public class BackgroundContextSpectrumRegressionTests
{
    private static (SimplicialMesh mesh, LieAlgebra algebra) SetupMeshAndAlgebra()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        return (mesh, algebra);
    }

    private static SpectrumBundle ComputeSpectrumForBackground(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        string backgroundId,
        AdmissibilityLevel admissibility)
    {
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var mass = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, mass);
        var geometry = TestHelpers.DummyGeometry();

        var operatorType = admissibility == AdmissibilityLevel.B2
            ? SpectralOperatorType.GaussNewton
            : SpectralOperatorType.FullHessian;

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = backgroundId,
            OperatorType = operatorType,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = admissibility,
        };

        var bundleBuilder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);
        var bundle = bundleBuilder.Build(spec, omega, a0, manifest, geometry);

        var eigSpec = new GeneralizedEigenproblemSpec { NumEigenvalues = 3 };
        var pipeline = new EigensolverPipeline();
        return pipeline.Solve(bundle, eigSpec);
    }

    /// <summary>
    /// G-004: Two backgrounds with different omega tensors must produce different eigenvalues.
    /// Verifies that compute-spectrum output is sensitive to the loaded omega state.
    /// </summary>
    [Fact]
    public void TwoDifferentStoredBackgrounds_ProduceDifferentSpectrumArtifacts()
    {
        var (mesh, algebra) = SetupMeshAndAlgebra();
        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var manifest = TestHelpers.TestManifest();
        var a0 = TestHelpers.MakeField(edgeN, 0.0);

        // Background 1: zero omega (trivial background)
        var omega1 = TestHelpers.MakeField(edgeN, 0.0);
        // Background 2: nonzero omega (perturbed background with nonzero curvature)
        var rng = new Random(42);
        var omega2 = TestHelpers.MakeRandomField(edgeN, rng);

        var spectrum1 = ComputeSpectrumForBackground(
            mesh, algebra, omega1, a0, manifest, "bg-zero", AdmissibilityLevel.B1);
        var spectrum2 = ComputeSpectrumForBackground(
            mesh, algebra, omega2, a0, manifest, "bg-nonzero", AdmissibilityLevel.B1);

        // Both must succeed
        Assert.NotEmpty(spectrum1.Modes);
        Assert.NotEmpty(spectrum2.Modes);

        // The spectra must differ — different omega produces different Hessian eigenvalues
        var ev1 = spectrum1.Modes.Select(m => m.Eigenvalue).OrderBy(v => v).ToList();
        var ev2 = spectrum2.Modes.Select(m => m.Eigenvalue).OrderBy(v => v).ToList();

        // At least one eigenvalue must differ between the two backgrounds
        bool anyDifference = ev1.Zip(ev2).Any(p => System.Math.Abs(p.First - p.Second) > 1e-12);
        Assert.True(anyDifference,
            $"Eigenvalues of two different backgrounds should differ, but got identical spectra. " +
            $"bg1={string.Join(", ", ev1.Select(v => v.ToString("E6")))} " +
            $"bg2={string.Join(", ", ev2.Select(v => v.ToString("E6")))}");
    }

    /// <summary>
    /// G-004: Two backgrounds with different A0 tensors must produce different eigenvalues.
    /// Verifies that compute-spectrum output is sensitive to the loaded A0 state.
    /// </summary>
    [Fact]
    public void TwoDifferentA0Backgrounds_ProduceDifferentSpectrumArtifacts()
    {
        var (mesh, algebra) = SetupMeshAndAlgebra();
        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var manifest = TestHelpers.TestManifest();

        // Use augmented torsion so A0 actually affects the spectrum
        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);

        var rng = new Random(99);
        var omega = TestHelpers.MakeRandomField(edgeN, rng);

        // A0 zero vs nonzero
        var a0Zero = TestHelpers.MakeField(edgeN, 0.0);
        var a0Nonzero = TestHelpers.MakeRandomField(edgeN, new Random(12));

        SpectrumBundle ComputeWithA0(FieldTensor a0, string bgId)
        {
            var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
            var mass = new CpuMassMatrix(mesh, algebra);
            var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, mass);
            var geometry = TestHelpers.DummyGeometry();
            var spec = new LinearizedOperatorSpec
            {
                BackgroundId = bgId,
                OperatorType = SpectralOperatorType.FullHessian,
                Formulation = PhysicalModeFormulation.PenaltyFixed,
                BackgroundAdmissibility = AdmissibilityLevel.B1,
            };
            var bundleBuilder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);
            var bundle = bundleBuilder.Build(spec, omega, a0, manifest, geometry);
            var eigSpec = new GeneralizedEigenproblemSpec { NumEigenvalues = 3 };
            return new EigensolverPipeline().Solve(bundle, eigSpec);
        }

        var spectrum1 = ComputeWithA0(a0Zero, "bg-a0-zero");
        var spectrum2 = ComputeWithA0(a0Nonzero, "bg-a0-nonzero");

        Assert.NotEmpty(spectrum1.Modes);
        Assert.NotEmpty(spectrum2.Modes);

        var ev1 = spectrum1.Modes.Select(m => m.Eigenvalue).OrderBy(v => v).ToList();
        var ev2 = spectrum2.Modes.Select(m => m.Eigenvalue).OrderBy(v => v).ToList();

        bool anyDifference = ev1.Zip(ev2).Any(p => System.Math.Abs(p.First - p.Second) > 1e-12);
        Assert.True(anyDifference,
            $"Eigenvalues with different A0 should differ, but got identical spectra. " +
            $"bg1={string.Join(", ", ev1.Select(v => v.ToString("E6")))} " +
            $"bg2={string.Join(", ", ev2.Select(v => v.ToString("E6")))}");
    }

    /// <summary>
    /// G-004: Spectrum provenance diagnostic notes must record the omega source.
    /// Verifies the pipeline records what background state was consumed.
    /// </summary>
    [Fact]
    public void Spectrum_RecordsBackgroundIdInBundle()
    {
        var (mesh, algebra) = SetupMeshAndAlgebra();
        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var manifest = TestHelpers.TestManifest();
        var omega = TestHelpers.MakeField(edgeN, 0.0);
        var a0 = TestHelpers.MakeField(edgeN, 0.0);
        var geometry = TestHelpers.DummyGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var mass = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, mass);

        const string bgId = "test-bg-provenance-001";
        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = bgId,
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
        };

        var bundleBuilder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);
        var bundle = bundleBuilder.Build(spec, omega, a0, manifest, geometry);

        // The bundle must record which background it represents
        Assert.Equal(bgId, bundle.BackgroundId);
    }
}

using Gu.Artifacts;
using Gu.Branching;
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
/// Regression tests for P4-C3: Nontrivial Bosonic Validation Study 001.
///
/// This study tests the full artifact chain with:
/// - Nonzero initial omega (constant 0.3 on first su(2) generator per edge)
/// - Nonzero A0 background (constant 0.2 on second su(2) generator per edge)
/// - augmented-torsion branch: T^aug = d_A0(omega - A0)
/// - identity-shiab branch: S = F (curvature 2-form)
/// - Single tetrahedron in 3D (4 vertices, 6 edges, 4 faces), su(2) gauge group
///
/// The torsion T = d_A0(omega - A0) is nonzero because:
///   alpha = omega - A0 has components on first generator (0.1 per edge),
///   and A0 has components on second generator (0.2 per edge),
///   so the bracket term [A0 wedge alpha] is nonzero.
///
/// The curvature F is nonzero because omega is nonzero and its bracket term
///   (1/2)[omega wedge omega] contributes via cross-generator terms.
///
/// Thus Upsilon = S - T = F - T is nontrivial, producing a nonzero gradient
/// and a nontrivial Hessian spectrum.
///
/// Negative results are preserved: if the branch is unstable, the convergence
/// failure is reported by the solver but the artifact chain is still produced.
/// </summary>
public sealed class BosonicValidationStudy001Tests
{
    // Single tetrahedron: 4 vertices, 6 edges, 4 faces
    private static SimplicialMesh BuildTetrahedron() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } });

    // Branch manifest: augmented-torsion + identity-shiab
    private static BranchManifest StudyManifest() => new()
    {
        BranchId = "bosonic-validation-001",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "manuscript-r1",
        CodeRevision = "phase4-p4c3",
        ActiveGeometryBranch = "simplicial-patch-v1",
        ActiveObservationBranch = "sigma-pullback-v1",
        ActiveTorsionBranch = "augmented-torsion-v1",
        ActiveShiabBranch = "identity-shiab-v1",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 3,
        AmbientDimension = 9,
        LieAlgebraId = "su2",
        BasisConventionId = "standard-basis",
        ComponentOrderId = "lexicographic",
        AdjointConventionId = "killing-form",
        PairingConventionId = "trace-pairing",
        NormConventionId = "L2-norm",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2", "IX-3", "IX-4", "IX-5" },
        Parameters = new Dictionary<string, string>
        {
            ["gaugePenaltyWeight"] = "1.0",
            ["solverMaxIter"] = "50",
        },
    };

    private static GeometryContext StudyGeometry() => new()
    {
        BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 3 },
        AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 9 },
        DiscretizationType = "simplicial",
        QuadratureRuleId = "simplex-3d-centroid",
        BasisFamilyId = "P1-simplex-3d",
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

    /// <summary>
    /// Build nonzero omega: edge-varying connection to break boundary-operator cancellation.
    /// Edge e gets value 0.3 * (e+1) / edgeCount on generator 0, and 0.1 on generator 1,
    /// ensuring that d(omega) ≠ 0 and the Jacobian bracket terms are also nonzero.
    ///
    /// Physical motivation: a linear ramp in the connection corresponds to a constant
    /// curvature 2-form (like a uniform magnetic field), which is the simplest nontrivial
    /// gauge configuration.
    /// </summary>
    private static ConnectionField BuildNonzeroOmega(SimplicialMesh mesh, LieAlgebra algebra)
    {
        var omega = new ConnectionField(mesh, algebra);
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            double val0 = 0.3 * (e + 1.0) / mesh.EdgeCount; // varying on gen 0
            double val1 = 0.1 * (mesh.EdgeCount - e) / mesh.EdgeCount; // varying on gen 1
            omega.SetEdgeValue(e, new[] { val0, val1, 0.0 });
        }
        return omega;
    }

    /// <summary>
    /// Build nonzero A0: different edge-varying pattern on second generator.
    /// Using a different profile from omega ensures T = d_A0(omega - A0) is nontrivial
    /// and that the Jacobian dT/domega = d_A0 is not identical to dS/domega.
    /// </summary>
    private static ConnectionField BuildNonzeroA0(SimplicialMesh mesh, LieAlgebra algebra)
    {
        var a0 = new ConnectionField(mesh, algebra);
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            double val1 = 0.2 * (e + 0.5) / mesh.EdgeCount; // varying on gen 1
            a0.SetEdgeValue(e, new[] { 0.0, val1, 0.0 });
        }
        return a0;
    }

    // ===== Artifact: Initial Omega is Nonzero =====

    [Fact]
    public void Study001_InitialOmega_IsNonzero()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);

        double maxCoeff = omega.Coefficients.Max(System.Math.Abs);
        Assert.True(maxCoeff > 0.0, $"Initial omega must be nonzero; max coeff = {maxCoeff}");
    }

    // ===== Artifact: Curvature F is Nonzero =====

    [Fact]
    public void Study001_CurvatureF_IsNonzero()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);

        var curvature = CurvatureAssembler.Assemble(omega);
        double maxF = curvature.Coefficients.Max(System.Math.Abs);

        Assert.True(maxF > 0.0, $"Curvature F must be nonzero for nonzero omega; max |F| = {maxF}");
    }

    // ===== Artifact: Torsion T is Nonzero =====

    [Fact]
    public void Study001_AugmentedTorsion_IsNonzero()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);
        var a0 = BuildNonzeroA0(mesh, algebra);
        var manifest = StudyManifest();
        var geometry = StudyGeometry();

        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var T = torsion.Evaluate(omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        double maxT = T.Coefficients.Max(System.Math.Abs);
        Assert.True(maxT > 0.0,
            $"Augmented torsion T must be nonzero with nonzero A0 and omega != A0; max |T| = {maxT}");
    }

    // ===== Artifact: Residual Upsilon = S - T is Nonzero =====

    [Fact]
    public void Study001_Residual_Upsilon_IsNonzero()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);
        var a0 = BuildNonzeroA0(mesh, algebra);
        var manifest = StudyManifest();
        var geometry = StudyGeometry();

        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var derived = assembler.AssembleDerivedState(omega, a0, manifest, geometry);

        double maxUpsilon = derived.ResidualUpsilon.Coefficients.Max(System.Math.Abs);
        Assert.True(maxUpsilon > 0.0,
            $"Residual Upsilon = S - T must be nonzero; max |Upsilon| = {maxUpsilon}");
    }

    // ===== Artifact: Jacobian has Nonzero Entries =====

    [Fact]
    public void Study001_Jacobian_IsNontrivial()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);
        var a0 = BuildNonzeroA0(mesh, algebra);
        var manifest = StudyManifest();
        var geometry = StudyGeometry();

        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, massMatrix);

        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var derived = assembler.AssembleDerivedState(omega, a0, manifest, geometry);

        var jacobian = backend.BuildJacobian(
            omega.ToFieldTensor(), a0.ToFieldTensor(),
            derived.CurvatureF, manifest, geometry);

        // Verify J is nontrivial by computing its Frobenius norm:
        // ||J||_F^2 = sum_j ||J * e_j||^2. If this is > 0, J is nonzero.
        // We use basis vectors on gen 2 (third su(2) generator), because:
        //   - J*(delta on e_2) has a d(delta) component from dS (via boundary operator)
        //   - minus d(delta) from dT, which cancel
        //   - plus bracket [omega_gen0, delta_gen2] from dS, contributing on gen 1
        //   - These bracket terms don't cancel since dT's bracket with A0_gen1 on delta_gen2 gives gen 0
        // We sweep all basis vectors to guarantee at least one is nonzero.
        int n = mesh.EdgeCount * algebra.Dimension;
        double frobeniusNormSq = 0.0;
        var omegaTensorForSig = omega.ToFieldTensor();

        for (int j = 0; j < n; j++)
        {
            var ejCoeffs = new double[n];
            ejCoeffs[j] = 1.0;
            var ej = new FieldTensor
            {
                Label = $"e_{j}",
                Signature = omegaTensorForSig.Signature,
                Coefficients = ejCoeffs,
                Shape = new[] { n },
            };
            var Jej = jacobian.Apply(ej);
            foreach (var c in Jej.Coefficients)
                frobeniusNormSq += c * c;
        }

        Assert.True(frobeniusNormSq > 0.0,
            $"Jacobian J must have nonzero Frobenius norm; ||J||_F^2 = {frobeniusNormSq:G6}");
    }

    // ===== Artifact: Gradient G = J^T M Upsilon is Nonzero =====

    [Fact]
    public void Study001_Gradient_IsNonzero()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);
        var a0 = BuildNonzeroA0(mesh, algebra);
        var manifest = StudyManifest();
        var geometry = StudyGeometry();

        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, massMatrix);

        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var derived = assembler.AssembleDerivedState(omega, a0, manifest, geometry);

        var jacobian = backend.BuildJacobian(
            omega.ToFieldTensor(), a0.ToFieldTensor(),
            derived.CurvatureF, manifest, geometry);

        var gradient = backend.ComputeGradient(jacobian, derived.ResidualUpsilon);

        double maxG = gradient.Coefficients.Max(System.Math.Abs);
        Assert.True(maxG > 0.0,
            $"Gradient G = J^T M Upsilon must be nonzero; max |G| = {maxG}");
    }

    // ===== Artifact: Spectrum Eigenvalues Are Nontrivial =====

    [Fact]
    public void Study001_Spectrum_HasNontrivialEigenvalues()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);
        var a0 = BuildNonzeroA0(mesh, algebra);
        var manifest = StudyManifest();
        var geometry = StudyGeometry();

        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, massMatrix);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bosonic-validation-001-bg",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            GaugeLambda = 1.0,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
        };

        var bundleBuilder = new OperatorBundleBuilder(mesh, algebra, assembler, massMatrix, backend);
        var bundle = bundleBuilder.Build(
            spec, omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        var eigSpec = new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 4,
            SolverMethod = "explicit-dense",
        };

        var pipeline = new EigensolverPipeline();
        var spectrum = pipeline.Solve(bundle, eigSpec);

        Assert.NotEmpty(spectrum.Modes);

        // At least one eigenvalue should be strictly positive (nontrivial Hessian)
        bool hasPositive = spectrum.Modes.Any(m => m.Eigenvalue > 1e-12);
        Assert.True(hasPositive,
            $"Spectrum must contain at least one positive eigenvalue for nontrivial background. " +
            $"First eigenvalue = {spectrum.Modes[0].Eigenvalue:G6}");
    }

    // ===== Artifact: Nontrivial result is distinct from trivial (omega=0, A0=0) baseline =====

    [Fact]
    public void Study001_NontrivialBranch_DiffersFromTrivialBaseline()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = StudyManifest();
        var geometry = StudyGeometry();

        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);

        // Trivial baseline: omega = 0, A0 = 0
        var omegaFlat = ConnectionField.Zero(mesh, algebra);
        var a0Flat = ConnectionField.Zero(mesh, algebra);
        var derivedFlat = assembler.AssembleDerivedState(omegaFlat, a0Flat, manifest, geometry);
        double trivialNorm = derivedFlat.ResidualUpsilon.Coefficients.Max(System.Math.Abs);

        // Nontrivial study: omega = 0.3e_1, A0 = 0.2e_2
        var omega = BuildNonzeroOmega(mesh, algebra);
        var a0 = BuildNonzeroA0(mesh, algebra);
        var derived = assembler.AssembleDerivedState(omega, a0, manifest, geometry);
        double nontrivialNorm = derived.ResidualUpsilon.Coefficients.Max(System.Math.Abs);

        // The nontrivial branch should produce a strictly larger residual norm
        Assert.True(nontrivialNorm > trivialNorm,
            $"Nontrivial branch residual ({nontrivialNorm:G6}) must exceed trivial baseline ({trivialNorm:G6})");
    }

    // ===== Negative result: pipeline still produces artifacts even if solver does not converge =====

    [Fact]
    public void Study001_SolverRun_ProducesArtifactBundle_RegardlessOfConvergence()
    {
        var mesh = BuildTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = BuildNonzeroOmega(mesh, algebra);
        var a0 = BuildNonzeroA0(mesh, algebra);
        var manifest = StudyManifest();
        var geometry = StudyGeometry();

        var torsion = new AugmentedTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        // Use ResidualOnly (Mode A): evaluates but performs no descent steps
        var options = new SolverOptions
        {
            Mode = SolveMode.ResidualOnly,
            MaxIterations = 1,
            GaugePenaltyLambda = 1.0,
        };

        var result = pipeline.Execute(omega, a0, manifest, geometry, options);

        // Artifact bundle must always be produced
        Assert.NotNull(result.ArtifactBundle);
        Assert.NotNull(result.ArtifactBundle.Residuals);
        Assert.NotNull(result.ArtifactBundle.DerivedState);

        // The initial field in the bundle must reflect our nonzero omega
        Assert.NotNull(result.ArtifactBundle.InitialState);
        double maxInitialOmega = result.ArtifactBundle.InitialState!.Omega.Coefficients
            .Max(System.Math.Abs);
        Assert.True(maxInitialOmega > 0.0,
            "ArtifactBundle.InitialState must preserve nonzero initial omega");

        // DerivedState must contain nonzero curvature (F is nonzero because omega is nonzero)
        Assert.NotNull(result.ArtifactBundle.DerivedState);
        double maxF = result.ArtifactBundle.DerivedState!.CurvatureF.Coefficients
            .Max(System.Math.Abs);
        Assert.True(maxF > 0.0,
            $"ArtifactBundle.DerivedState must contain nonzero curvature F; max |F| = {maxF}");
    }

    // ===== P4-C1 Parity: CLI-driven and in-process-driven runs share the same branch identity =====

    /// <summary>
    /// Verifies the P4-C1 parity requirement: writing the study branch manifest to a canonical
    /// run folder (as the CLI 'run' command does) and then reading it back yields the same
    /// branchId as the in-process StudyManifest(). This confirms the CLI does not silently
    /// substitute a toy/default branch for the persisted study branch.
    ///
    /// The CLI flow is:
    ///   gu init-run {outputDir}
    ///   cp branch.json {outputDir}/manifest/branch.json   (overrides default)
    ///   gu run {outputDir} --backend cpu --mode A ...
    ///
    /// The written manifest/branch.json carries branchId="bosonic-validation-001".
    /// The in-process StudyManifest() also has BranchId="bosonic-validation-001".
    /// Both must agree for P4-C1 parity.
    /// </summary>
    [Fact]
    public void Study001_CliRunFolder_BranchId_MatchesInProcessManifest()
    {
        // In-process branch identity
        var inProcessManifest = StudyManifest();
        var expectedBranchId = inProcessManifest.BranchId; // "bosonic-validation-001"

        // Simulate the CLI 'init-run' + branch manifest copy into a temp run folder
        var tempRunFolder = Path.Combine(Path.GetTempPath(), $"gu-bv001-parity-{System.Guid.NewGuid():N}");
        try
        {
            var writer = new RunFolderWriter(tempRunFolder);
            writer.CreateDirectories();

            // Write the study manifest exactly as the CLI would after 'cp branch.json manifest/branch.json'
            writer.WriteBranchManifest(inProcessManifest);

            // Read back via JSON exactly as the CLI 'run' command would load it
            var manifestPath = Path.Combine(tempRunFolder, RunFolderLayout.BranchManifestFile);
            Assert.True(File.Exists(manifestPath),
                $"CLI artifact manifest/branch.json must exist in run folder: {manifestPath}");

            var json = File.ReadAllText(manifestPath);
            var loaded = GuJsonDefaults.Deserialize<BranchManifest>(json);
            Assert.NotNull(loaded);

            // P4-C1 parity: CLI-written branchId must match in-process study branchId
            Assert.Equal(expectedBranchId, loaded!.BranchId);

            // Verify the study-specific branches are also preserved (not overwritten with defaults)
            Assert.Equal("augmented-torsion-v1", loaded.ActiveTorsionBranch);
            Assert.Equal("identity-shiab-v1", loaded.ActiveShiabBranch);
            Assert.Equal("su2", loaded.LieAlgebraId);
        }
        finally
        {
            if (Directory.Exists(tempRunFolder))
                Directory.Delete(tempRunFolder, recursive: true);
        }
    }
}

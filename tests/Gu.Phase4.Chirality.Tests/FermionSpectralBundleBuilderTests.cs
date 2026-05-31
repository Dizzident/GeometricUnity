using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Chirality;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Chirality.Tests;

/// <summary>
/// Tests for M38: FermionSpectralBundleBuilder, BackgroundReplayFilter, MassPsiWeightsBuilder.
/// Verifies that the full spectral solve + chirality + conjugation pipeline produces
/// a properly populated FermionSpectralBundle.
/// </summary>
public class FermionSpectralBundleBuilderTests
{
    // -------------------------------------------------------
    // Shared fixtures
    // -------------------------------------------------------

    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m38-bundle",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 3, 2 } });

    private static SpinorRepresentationSpec Dim2Spec() => new()
    {
        SpinorSpecId = "spinor-dim2-v1",
        SpacetimeDimension = 2,
        CliffordSignature = new CliffordSignature { Positive = 2, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 2, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 2,
            HasChirality = true,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-standard-v1",
            SignConvention = "left-is-minus",
            PhaseFactor = "-1",
            HasChirality = true,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = true,
        },
        SpinorComponents = 2,
        ChiralitySplit = 1,
        Provenance = TestProvenance(),
    };

    private static DiracOperatorBundle BuildTrivialDiracBundle(SimplicialMesh mesh, string bgId = "bg-1")
    {
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var bg = new BackgroundRecord
        {
            BackgroundId = bgId,
            EnvironmentId = "test-env",
            BranchManifestId = "m1",
            GeometryFingerprint = "test-fp",
            StateArtifactRef = "test-state-ref",
            ResidualNorm = 0.001,
            StationarityNorm = 0.001,
            AdmissibilityLevel = AdmissibilityLevel.B1,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 0.001,
                StationarityNorm = 0.001,
                ObjectiveValue = 0.1,
                GaugeViolation = 0.0,
                SolverIterations = 10,
                SolverConverged = true,
                TerminationReason = "residual-converged",
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R2",
            Provenance = TestProvenance(),
        };
        var bosonicState = new double[mesh.EdgeCount];
        var connBuilder = new CpuSpinConnectionBuilder();
        var conn = connBuilder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        var gammaBuilder = new GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());
        var assembler = new CpuDiracOperatorAssembler();
        return assembler.Assemble(conn, gammas, layout, mesh, TestProvenance());
    }

    private static GammaOperatorBundle BuildGammas()
    {
        var spec = Dim2Spec();
        var builder = new GammaMatrixBuilder();
        return builder.Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());
    }

    private static FermionSpectralResult SolveSpectral(
        DiracOperatorBundle diracBundle,
        FermionFieldLayout layout,
        int modeCount = 2)
    {
        var config = new FermionSpectralConfig
        {
            TargetRegion = "lowest-magnitude",
            ModeCount = modeCount,
            ConvergenceTolerance = 1e-10,
            MaxIterations = 500,
            Seed = 42,
        };
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());
        return solver.Solve(diracBundle, layout, config, TestProvenance());
    }

    // -------------------------------------------------------
    // FermionSpectralBundleBuilder tests
    // -------------------------------------------------------

    [Fact]
    public void Build_ReturnsBundleWithCorrectModeCount()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh);
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 2);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, TestProvenance());

        Assert.NotNull(bundle);
        Assert.Equal(spectralResult.Modes.Count, bundle.ModeCount);
    }

    [Fact]
    public void Build_ChiralityDecompositionsMatchModeCount()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh);
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 2);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, TestProvenance());

        Assert.Equal(bundle.ModeCount, bundle.ChiralityDecompositions.Count);
    }

    [Fact]
    public void Build_ChiralityAvailableForEvenDimension()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh);
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 2);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, TestProvenance());

        // dim=2 is even, so chirality should be available
        Assert.True(bundle.ChiralityAnalysisAvailable);
    }

    [Fact]
    public void Build_ConjugationAnalysisApplied()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh, "bg-2t");
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 3);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, TestProvenance());

        Assert.True(bundle.ConjugationAnalysisApplied);
        Assert.NotNull(bundle.ConjugationPairs); // may be empty list, but must be non-null
    }

    [Fact]
    public void Build_GaugeLeakSummaryContainsCorrectTotalModes()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh, "bg-leak");
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 3);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, TestProvenance());

        Assert.Equal(bundle.ModeCount, bundle.GaugeLeakSummary.TotalModes);
    }

    [Fact]
    public void Build_BundleIdContainsFermionBackgroundId()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh, "bg-named");
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 2);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, TestProvenance());

        Assert.Contains("bg-named", bundle.BundleId);
        Assert.Equal("bg-named", bundle.FermionBackgroundId);
    }

    [Fact]
    public void Build_ChiralityFractionsInUnitInterval()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh);
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 2);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, TestProvenance());

        foreach (var decomp in bundle.ChiralityDecompositions)
        {
            Assert.InRange(decomp.LeftFraction, 0.0, 1.0 + 1e-9);
            Assert.InRange(decomp.RightFraction, 0.0, 1.0 + 1e-9);
        }
    }

    [Fact]
    public void Build_ProvenancePreserved()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh);
        var spectralResult = SolveSpectral(diracBundle, layout, modeCount: 2);
        var gammas = BuildGammas();
        var bundleBuilder = new FermionSpectralBundleBuilder();
        var prov = TestProvenance();

        var bundle = bundleBuilder.Build(
            spectralResult, gammas,
            spec.ChiralityConvention, spec.ConjugationConvention,
            layout, mesh.VertexCount, prov);

        Assert.Equal("test-m38-bundle", bundle.Provenance.CodeRevision);
    }

    // -------------------------------------------------------
    // BackgroundReplayFilter tests
    // -------------------------------------------------------

    [Fact]
    public void BackgroundReplayFilter_FiltersOutBelowMinTier()
    {
        var r0 = MakeBackground("bg-r0", "R0");
        var r1 = MakeBackground("bg-r1", "R1");
        var r2 = MakeBackground("bg-r2", "R2");
        var r3 = MakeBackground("bg-r3", "R3");

        var filtered = BackgroundReplayFilter.FilterByMinimumTier(
            new[] { r0, r1, r2, r3 }, "R2");

        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, b => Assert.True(
            b.ReplayTierAchieved == "R2" || b.ReplayTierAchieved == "R3"));
    }

    [Fact]
    public void BackgroundReplayFilter_AllPassR0()
    {
        var backgrounds = new[]
        {
            MakeBackground("bg-a", "R0"),
            MakeBackground("bg-b", "R1"),
            MakeBackground("bg-c", "R2"),
        };

        var filtered = BackgroundReplayFilter.FilterByMinimumTier(backgrounds, "R0");

        Assert.Equal(3, filtered.Count);
    }

    [Fact]
    public void BackgroundReplayFilter_MeetsTier_ReturnsTrueForSufficient()
    {
        var bg = MakeBackground("bg-x", "R2");
        Assert.True(BackgroundReplayFilter.MeetsTier(bg, "R2"));
        Assert.True(BackgroundReplayFilter.MeetsTier(bg, "R1"));
        Assert.True(BackgroundReplayFilter.MeetsTier(bg, "R0"));
    }

    [Fact]
    public void BackgroundReplayFilter_MeetsTier_ReturnsFalseForInsufficient()
    {
        var bg = MakeBackground("bg-y", "R1");
        Assert.False(BackgroundReplayFilter.MeetsTier(bg, "R2"));
        Assert.False(BackgroundReplayFilter.MeetsTier(bg, "R3"));
    }

    [Fact]
    public void BackgroundReplayFilter_ThrowsOnUnknownTier()
    {
        var backgrounds = new[] { MakeBackground("bg-z", "R1") };
        Assert.Throws<ArgumentException>(
            () => BackgroundReplayFilter.FilterByMinimumTier(backgrounds, "R99"));
    }

    // -------------------------------------------------------
    // MassPsiWeightsBuilder tests
    // -------------------------------------------------------

    [Fact]
    public void MassPsiWeightsBuilder_BuildIdentity_AllOnes()
    {
        var weights = MassPsiWeightsBuilder.BuildIdentity(cellCount: 3, dofsPerCell: 2);

        Assert.Equal(3 * 2 * 2, weights.Length); // cellCount * dofsPerCell * 2 (real rep)
        Assert.All(weights, w => Assert.Equal(1.0, w));
    }

    [Fact]
    public void MassPsiWeightsBuilder_BuildFromCellVolumes_CorrectLength()
    {
        var volumes = new double[] { 0.5, 1.0, 0.25 }; // 3 cells
        var weights = MassPsiWeightsBuilder.BuildFromCellVolumes(volumes, dofsPerCell: 2);

        Assert.Equal(3 * 2 * 2, weights.Length); // 3 cells * 2 dofs * 2 (real rep)
    }

    [Fact]
    public void MassPsiWeightsBuilder_BuildFromCellVolumes_VolumesRepeatedPerDof()
    {
        var volumes = new double[] { 2.0, 3.0 }; // 2 cells
        int dofsPerCell = 2;
        var weights = MassPsiWeightsBuilder.BuildFromCellVolumes(volumes, dofsPerCell);

        // Cell 0: 2 * dofsPerCell entries all equal 2.0
        for (int k = 0; k < dofsPerCell * 2; k++)
            Assert.Equal(2.0, weights[k]);
        // Cell 1: 2 * dofsPerCell entries all equal 3.0
        for (int k = 0; k < dofsPerCell * 2; k++)
            Assert.Equal(3.0, weights[dofsPerCell * 2 + k]);
    }

    [Fact]
    public void MassPsiWeightsBuilder_BuildFromCellVolumes_ThrowsOnNonPositiveVolume()
    {
        var volumes = new double[] { 1.0, 0.0 }; // zero not allowed
        Assert.Throws<ArgumentException>(() =>
            MassPsiWeightsBuilder.BuildFromCellVolumes(volumes, dofsPerCell: 2));
    }

    [Fact]
    public void MassPsiWeightsBuilder_BuildFromMesh_SingleTrianglePositiveVolume()
    {
        var mesh = SingleTriangle();
        var weights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerCell: 2);

        Assert.Equal(mesh.VertexCount * 2 * 2, weights.Length);
        Assert.All(weights, w => Assert.True(w > 0, $"weight {w} must be positive"));
    }

    // -------------------------------------------------------
    // FermionSpectralSolver with MassPsiWeights tests
    // -------------------------------------------------------

    [Fact]
    public void Solve_WithMassPsiWeights_ProducesModes()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh);

        // Build M_psi from mesh geometry
        int dofsPerCell = diracBundle.DofsPerCell;
        var massPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerCell);

        var config = new FermionSpectralConfig
        {
            TargetRegion = "lowest-magnitude",
            ModeCount = 2,
            ConvergenceTolerance = 1e-10,
            MaxIterations = 500,
            Seed = 42,
            MassPsiWeights = massPsiWeights,
        };
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(diracBundle, layout, config, TestProvenance());

        Assert.NotNull(result);
        Assert.Equal(2, result.Modes.Count);
        Assert.Contains("mpsi", result.Diagnostics.SolverName);
    }

    [Fact]
    public void Solve_WithMassPsiWeights_ModesHaveNonNegativeMagnitude()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh, "bg-mpsi");

        int dofsPerCell = diracBundle.DofsPerCell;
        var massPsiWeights = MassPsiWeightsBuilder.BuildIdentity(mesh.VertexCount, dofsPerCell);

        var config = new FermionSpectralConfig
        {
            TargetRegion = "lowest-magnitude",
            ModeCount = 3,
            ConvergenceTolerance = 1e-10,
            MaxIterations = 500,
            Seed = 7,
            MassPsiWeights = massPsiWeights,
        };
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(diracBundle, layout, config, TestProvenance());

        Assert.All(result.Modes, m => Assert.True(m.EigenvalueMagnitude >= 0.0));
    }

    [Fact]
    public void Solve_WithMassPsiWeights_WrongLengthThrows()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var diracBundle = BuildTrivialDiracBundle(mesh);

        var config = new FermionSpectralConfig
        {
            TargetRegion = "lowest-magnitude",
            ModeCount = 2,
            ConvergenceTolerance = 1e-10,
            MaxIterations = 500,
            Seed = 42,
            MassPsiWeights = new double[] { 1.0 }, // wrong length
        };
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        Assert.Throws<ArgumentException>(() =>
            solver.Solve(diracBundle, layout, config, TestProvenance()));
    }

    [Fact]
    public void Solve_WithNonuniformMassPsiWeights_SatisfiesGeneralizedEigenproblemAndMOrthonormality()
    {
        var diracBundle = BuildSyntheticHermitianDiracBundle();
        double[] massPsiWeights = SyntheticNonuniformMassPsiWeights();

        var result = SolveSyntheticSpectral(diracBundle, massPsiWeights);

        Assert.True(result.Diagnostics.Converged);
        Assert.Equal("cpu-dense-hermitian-b-mpsi-v2", result.Diagnostics.SolverName);
        Assert.Contains(result.Diagnostics.Notes, note =>
            note.Contains("B=M_psi^-1/2 K M_psi^-1/2", StringComparison.Ordinal));
        Assert.Equal(diracBundle.TotalDof, result.Modes.Count);

        foreach (var mode in result.Modes)
        {
            double[] psi = Assert.IsType<double[]>(mode.EigenvectorCoefficients);
            double[] kPsi = ApplyExplicitMatrix(diracBundle.ExplicitMatrix!, diracBundle.TotalDof, psi);
            double[] lambdaMPsi = ApplyScaledWeights(psi, massPsiWeights, mode.EigenvalueRe);
            double relativeResidual = RelativeResidual(kPsi, lambdaMPsi);
            var selfInnerProduct = WeightedInnerProduct(psi, psi, massPsiWeights);

            Assert.True(relativeResidual < 1e-11,
                $"Mode {mode.ModeIndex} generalized relative residual {relativeResidual:E6} is too large.");
            Assert.True(mode.ResidualNorm < 1e-10,
                $"Mode {mode.ModeIndex} reported residual {mode.ResidualNorm:E6} is too large.");
            Assert.True(System.Math.Abs(selfInnerProduct.Real - 1.0) < 1e-12,
                $"Mode {mode.ModeIndex} M-norm squared is {selfInnerProduct.Real:G17}.");
            Assert.True(System.Math.Abs(selfInnerProduct.Imaginary) < 1e-12);
        }

        for (int left = 0; left < result.Modes.Count; left++)
            for (int right = left + 1; right < result.Modes.Count; right++)
            {
                var overlap = WeightedInnerProduct(
                    result.Modes[left].EigenvectorCoefficients!,
                    result.Modes[right].EigenvectorCoefficients!,
                    massPsiWeights);
                Assert.True(
                    System.Math.Sqrt(overlap.Real * overlap.Real + overlap.Imaginary * overlap.Imaginary) < 1e-11,
                    $"Modes {left} and {right} are not M-orthogonal: ({overlap.Real:G17}, {overlap.Imaginary:G17}).");
            }
    }

    [Fact]
    public void Solve_WithIdentityMassPsiWeights_MatchesUnweightedSolve()
    {
        var diracBundle = BuildSyntheticHermitianDiracBundle();
        double[] identityWeights = Enumerable.Repeat(1.0, 2 * diracBundle.TotalDof).ToArray();

        var unweighted = SolveSyntheticSpectral(diracBundle, massPsiWeights: null);
        var weightedIdentity = SolveSyntheticSpectral(diracBundle, identityWeights);

        Assert.Equal("cpu-dense-hermitian-k-v2", unweighted.Diagnostics.SolverName);
        Assert.Equal("cpu-dense-hermitian-b-mpsi-v2", weightedIdentity.Diagnostics.SolverName);
        Assert.Equal(unweighted.Modes.Count, weightedIdentity.Modes.Count);
        for (int i = 0; i < unweighted.Modes.Count; i++)
        {
            Assert.Equal(unweighted.Modes[i].EigenvalueRe, weightedIdentity.Modes[i].EigenvalueRe, 12);
            var overlap = WeightedInnerProduct(
                unweighted.Modes[i].EigenvectorCoefficients!,
                weightedIdentity.Modes[i].EigenvectorCoefficients!,
                identityWeights);
            double overlapMagnitude =
                System.Math.Sqrt(overlap.Real * overlap.Real + overlap.Imaginary * overlap.Imaginary);
            Assert.True(System.Math.Abs(overlapMagnitude - 1.0) < 1e-12,
                $"Mode {i} identity-weight overlap magnitude is {overlapMagnitude:G17}.");
        }
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Solve_WithMassPsiWeights_NonPositiveOrNonFiniteWeightThrows(double invalidWeight)
    {
        var diracBundle = BuildSyntheticHermitianDiracBundle();
        double[] massPsiWeights = SyntheticNonuniformMassPsiWeights();
        massPsiWeights[0] = invalidWeight;
        massPsiWeights[1] = invalidWeight;

        Assert.Throws<ArgumentException>(() =>
            SolveSyntheticSpectral(diracBundle, massPsiWeights));
    }

    [Fact]
    public void Solve_WithMassPsiWeights_UnpairedRealImaginaryWeightsThrows()
    {
        var diracBundle = BuildSyntheticHermitianDiracBundle();
        double[] massPsiWeights = SyntheticNonuniformMassPsiWeights();
        massPsiWeights[1] = 1.5;

        Assert.Throws<ArgumentException>(() =>
            SolveSyntheticSpectral(diracBundle, massPsiWeights));
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private static DiracOperatorBundle BuildSyntheticHermitianDiracBundle()
    {
        (double Real, double Imaginary)[,] stiffness =
        {
            { (6.0, 0.0),  (1.0, 0.4),   (0.5, -0.2), (0.0, 0.1) },
            { (1.0, -0.4), (4.0, 0.0),   (-0.25, 0.3), (0.75, 0.0) },
            { (0.5, 0.2),  (-0.25, -0.3), (3.0, 0.0),  (1.25, -0.5) },
            { (0.0, -0.1), (0.75, 0.0),  (1.25, 0.5),  (2.0, 0.0) },
        };
        int totalDof = stiffness.GetLength(0);
        var explicitMatrix = new double[2 * totalDof * totalDof];
        for (int row = 0; row < totalDof; row++)
            for (int col = 0; col < totalDof; col++)
            {
                var entry = stiffness[row, col];
                explicitMatrix[2 * (row * totalDof + col)] = entry.Real;
                explicitMatrix[2 * (row * totalDof + col) + 1] = entry.Imaginary;
            }

        return new DiracOperatorBundle
        {
            OperatorId = "dirac-synthetic-hermitian-k",
            FermionBackgroundId = "bg-synthetic-hermitian-k",
            LayoutId = "layout-trivial",
            SpinConnectionId = "conn-synthetic-hermitian-k",
            MatrixShape = new[] { totalDof, totalDof },
            HasExplicitMatrix = true,
            ExplicitMatrix = explicitMatrix,
            ExplicitMatrixRef = null,
            IsHermitian = true,
            HermiticityResidual = 0.0,
            HermiticityTolerance = 1e-12,
            MassBranchTermIncluded = false,
            CorrectionTermIncluded = false,
            GaugeReductionApplied = false,
            CellCount = 2,
            DofsPerCell = 2,
            DiagnosticNotes = new List<string>(),
            Provenance = TestProvenance(),
        };
    }

    private static double[] SyntheticNonuniformMassPsiWeights() =>
        new[] { 1.0, 1.0, 2.0, 2.0, 4.0, 4.0, 0.5, 0.5 };

    private static FermionSpectralResult SolveSyntheticSpectral(
        DiracOperatorBundle diracBundle,
        double[]? massPsiWeights)
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = new FermionSpectralConfig
        {
            TargetRegion = "lowest-magnitude",
            ModeCount = diracBundle.TotalDof,
            ConvergenceTolerance = 1e-12,
            MaxIterations = 100,
            Seed = 42,
            MassPsiWeights = massPsiWeights,
        };
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());
        return solver.Solve(diracBundle, layout, config, TestProvenance());
    }

    private static double[] ApplyExplicitMatrix(double[] matrix, int totalDof, double[] vector)
    {
        var result = new double[2 * totalDof];
        for (int row = 0; row < totalDof; row++)
            for (int col = 0; col < totalDof; col++)
            {
                double matrixReal = matrix[2 * (row * totalDof + col)];
                double matrixImaginary = matrix[2 * (row * totalDof + col) + 1];
                double vectorReal = vector[2 * col];
                double vectorImaginary = vector[2 * col + 1];
                result[2 * row] += matrixReal * vectorReal - matrixImaginary * vectorImaginary;
                result[2 * row + 1] += matrixReal * vectorImaginary + matrixImaginary * vectorReal;
            }
        return result;
    }

    private static double[] ApplyScaledWeights(double[] vector, double[] weights, double scale) =>
        vector.Zip(weights, (coefficient, weight) => scale * weight * coefficient).ToArray();

    private static double RelativeResidual(double[] left, double[] right)
    {
        double residualSquared = 0.0;
        double leftSquared = 0.0;
        double rightSquared = 0.0;
        for (int i = 0; i < left.Length; i++)
        {
            double residual = left[i] - right[i];
            residualSquared += residual * residual;
            leftSquared += left[i] * left[i];
            rightSquared += right[i] * right[i];
        }
        return System.Math.Sqrt(residualSquared) /
            System.Math.Max(1e-300, System.Math.Max(System.Math.Sqrt(leftSquared), System.Math.Sqrt(rightSquared)));
    }

    private static (double Real, double Imaginary) WeightedInnerProduct(
        double[] left,
        double[] right,
        double[] weights)
    {
        double real = 0.0;
        double imaginary = 0.0;
        for (int i = 0; i < left.Length; i += 2)
        {
            double weight = weights[i];
            real += weight * (left[i] * right[i] + left[i + 1] * right[i + 1]);
            imaginary += weight * (left[i] * right[i + 1] - left[i + 1] * right[i]);
        }
        return (real, imaginary);
    }

    private static BackgroundRecord MakeBackground(string id, string tier) => new()
    {
        BackgroundId = id,
        EnvironmentId = "test-env",
        BranchManifestId = "m1",
        GeometryFingerprint = "test-fp",
        StateArtifactRef = "test-state-ref",
        ResidualNorm = 0.001,
        StationarityNorm = 0.001,
        AdmissibilityLevel = AdmissibilityLevel.B1,
        Metrics = new BackgroundMetrics
        {
            ResidualNorm = 0.001,
            StationarityNorm = 0.001,
            ObjectiveValue = 0.1,
            GaugeViolation = 0.0,
            SolverIterations = 10,
            SolverConverged = true,
            TerminationReason = "residual-converged",
            GaussNewtonValid = false,
        },
        ReplayTierAchieved = tier,
        Provenance = TestProvenance(),
    };
}

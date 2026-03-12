using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac.Tests;

/// <summary>
/// Tests for M38: FermionSpectralSolver.
/// Minimum 8 tests required.
/// </summary>
public class FermionSpectralSolverTests
{
    // -------------------------------------------------------
    // Test fixtures (shared with CpuSpinConnectionBuilderTests)
    // -------------------------------------------------------

    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m38",
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

    private static BackgroundRecord MakeBackgroundRecord(string id) => new()
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
        ReplayTierAchieved = "R1",
        Provenance = TestProvenance(),
    };

    private static FermionSpectralConfig DefaultConfig(int modeCount = 2) => new()
    {
        TargetRegion = "lowest-magnitude",
        ModeCount = modeCount,
        ConvergenceTolerance = 1e-10,
        MaxIterations = 500,
        Seed = 42,
    };

    /// <summary>
    /// Build a DiracOperatorBundle for a single triangle with trivial gauge.
    /// </summary>
    private static DiracOperatorBundle BuildTrivialBundle(SimplicialMesh mesh, string bgId = "bg-1")
    {
        var spec = Dim2Spec();
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var bg = MakeBackgroundRecord(bgId);
        var bosonicState = new double[mesh.EdgeCount];
        var connectionBuilder = new CpuSpinConnectionBuilder();
        var connection = connectionBuilder.Build(bg, bosonicState, spec, layout, mesh, TestProvenance());

        var gammaBuilder = new GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());
        var assembler = new CpuDiracOperatorAssembler();
        return assembler.Assemble(connection, gammas, layout, mesh, TestProvenance());
    }

    // -------------------------------------------------------
    // Tests
    // -------------------------------------------------------

    [Fact]
    public void Solve_ReturnsModes_CountMatchesConfig()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(modeCount: 2);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        Assert.NotNull(result);
        Assert.Equal(2, result.Modes.Count);
    }

    [Fact]
    public void Solve_ResultId_ContainsFermionBackgroundId()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh, "bg-abc");
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(1);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        Assert.Contains("bg-abc", result.ResultId);
        Assert.Equal("bg-abc", result.FermionBackgroundId);
    }

    [Fact]
    public void Solve_ModesSortedByAscendingMagnitude()
    {
        var mesh = TwoTriangles();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(modeCount: 3);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        for (int i = 1; i < result.Modes.Count; i++)
            Assert.True(result.Modes[i].EigenvalueMagnitude >= result.Modes[i - 1].EigenvalueMagnitude);
    }

    [Fact]
    public void Solve_ModesHaveNonNegativeMagnitude()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(2);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        Assert.All(result.Modes, m => Assert.True(m.EigenvalueMagnitude >= 0.0));
    }

    [Fact]
    public void Solve_DiagnosticsContainsSolverName()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(1);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        Assert.NotEmpty(result.Diagnostics.SolverName);
        Assert.True(result.Diagnostics.Iterations > 0);
    }

    [Fact]
    public void Solve_ProvenancePreserved()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(1);
        var prov = TestProvenance();
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, prov);

        Assert.Equal("test-m38", result.Provenance.CodeRevision);
    }

    [Fact]
    public void Solve_Reproducible_SameSeed()
    {
        var mesh = TwoTriangles();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(2);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var r1 = solver.Solve(bundle, layout, config, TestProvenance());
        var r2 = solver.Solve(bundle, layout, config, TestProvenance());

        Assert.Equal(r1.Modes.Count, r2.Modes.Count);
        for (int i = 0; i < r1.Modes.Count; i++)
            Assert.Equal(r1.Modes[i].EigenvalueMagnitude, r2.Modes[i].EigenvalueMagnitude, 10);
    }

    [Fact]
    public void Solve_ModeCountCappedAtTotalDof()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        // Single triangle has 1 cell, dofsPerCell = spinorDim*dimG = 2*1 = 2, totalDof = 2
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(modeCount: 100); // much larger than totalDof
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        // Modes count must not exceed totalDof
        Assert.True(result.Modes.Count <= bundle.TotalDof);
    }

    [Fact]
    public void Solve_ThrowsWhenNoExplicitMatrix()
    {
        // Create a bundle without explicit matrix (simulate large system)
        var bundle = new DiracOperatorBundle
        {
            OperatorId = "dirac-test",
            FermionBackgroundId = "bg-x",
            LayoutId = "layout-x",
            SpinConnectionId = "conn-x",
            MatrixShape = new[] { 2000, 2000 },
            HasExplicitMatrix = false,
            IsHermitian = true,
            HermiticityResidual = 0.0,
            MassBranchTermIncluded = false,
            CorrectionTermIncluded = false,
            GaugeReductionApplied = false,
            CellCount = 500,
            DofsPerCell = 4,
            DiagnosticNotes = new List<string>(),
            Provenance = TestProvenance(),
        };

        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(1);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        Assert.Throws<InvalidOperationException>(() =>
            solver.Solve(bundle, layout, config, TestProvenance()));
    }

    [Fact]
    public void Solve_ModeIndicesAreContiguous()
    {
        var mesh = TwoTriangles();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(3);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        for (int i = 0; i < result.Modes.Count; i++)
            Assert.Equal(i, result.Modes[i].ModeIndex);
    }

    // -------------------------------------------------------
    // M38 extension tests: chirality wiring, background selection,
    // observation summary, serialization
    // -------------------------------------------------------

    [Fact]
    public void Solve_WithoutChiralityProcessor_UsesNoChiralityConvention()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(2);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        Assert.All(result.Modes, m =>
            Assert.Equal("no-chirality-analysis", m.ChiralityDecomposition.SignConvention));
    }

    [Fact]
    public void Solve_WithChiralityPostProcessor_ProcessorIsApplied()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(2);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        // Post-processor marks all modes as left-chiral (definite-left test)
        FermionModeRecord[] processor(FermionModeRecord[] modes)
        {
            var updated = new FermionModeRecord[modes.Length];
            for (int i = 0; i < modes.Length; i++)
            {
                var m = modes[i];
                updated[i] = new FermionModeRecord
                {
                    ModeId = m.ModeId, BackgroundId = m.BackgroundId,
                    BranchVariantId = m.BranchVariantId, LayoutId = m.LayoutId,
                    ModeIndex = m.ModeIndex, EigenvalueRe = m.EigenvalueRe,
                    EigenvalueIm = m.EigenvalueIm, ResidualNorm = m.ResidualNorm,
                    EigenvectorCoefficients = m.EigenvectorCoefficients,
                    ChiralityDecomposition = new ChiralityDecompositionRecord
                    {
                        LeftFraction = 0.95, RightFraction = 0.05, MixedFraction = 0.0,
                        SignConvention = "left-is-minus",
                    },
                    ConjugationPairing = m.ConjugationPairing,
                    GaugeLeakScore = m.GaugeLeakScore, GaugeReductionApplied = m.GaugeReductionApplied,
                    Backend = m.Backend, ComputedWithUnverifiedGpu = m.ComputedWithUnverifiedGpu,
                    BranchStabilityScore = m.BranchStabilityScore,
                    RefinementStabilityScore = m.RefinementStabilityScore,
                    ReplayTier = m.ReplayTier, Provenance = m.Provenance,
                };
            }
            return updated;
        }

        var result = solver.Solve(bundle, layout, config, TestProvenance(),
            chiralityPostProcessor: processor);

        Assert.All(result.Modes, m =>
            Assert.Equal("left-is-minus", m.ChiralityDecomposition.SignConvention));
        Assert.All(result.Modes, m =>
            Assert.Equal(0.95, m.ChiralityDecomposition.LeftFraction));
    }

    [Fact]
    public void Solve_ObservationSummary_TotalMatchesModeCount()
    {
        var mesh = TwoTriangles();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(3);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        Assert.NotNull(result.ObservationSummary);
        Assert.Equal(result.Modes.Count, result.ObservationSummary!.TotalModes);
    }

    [Fact]
    public void Solve_ObservationSummary_ChiralCountsSumToTotal()
    {
        var mesh = TwoTriangles();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(3);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());
        var obs = result.ObservationSummary!;

        Assert.Equal(obs.TotalModes, obs.LeftChiralCount + obs.RightChiralCount + obs.MixedOrTrivialCount);
    }

    [Fact]
    public void Solve_ObservationSummary_NetChiralityIndexCorrect()
    {
        // Use post-processor to set known chirality fractions
        var mesh = TwoTriangles();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(2);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        // Set mode 0 to left, mode 1 to right
        FermionModeRecord[] processor(FermionModeRecord[] modes)
        {
            var updated = new FermionModeRecord[modes.Length];
            for (int i = 0; i < modes.Length; i++)
            {
                double lf = i == 0 ? 0.95 : 0.05;
                double rf = i == 0 ? 0.05 : 0.95;
                var m = modes[i];
                updated[i] = new FermionModeRecord
                {
                    ModeId = m.ModeId, BackgroundId = m.BackgroundId,
                    BranchVariantId = m.BranchVariantId, LayoutId = m.LayoutId,
                    ModeIndex = m.ModeIndex, EigenvalueRe = m.EigenvalueRe,
                    EigenvalueIm = m.EigenvalueIm, ResidualNorm = m.ResidualNorm,
                    EigenvectorCoefficients = m.EigenvectorCoefficients,
                    ChiralityDecomposition = new ChiralityDecompositionRecord
                    {
                        LeftFraction = lf, RightFraction = rf, MixedFraction = 0.0,
                        SignConvention = "left-is-minus",
                    },
                    ConjugationPairing = m.ConjugationPairing,
                    GaugeLeakScore = m.GaugeLeakScore, GaugeReductionApplied = m.GaugeReductionApplied,
                    Backend = m.Backend, ComputedWithUnverifiedGpu = m.ComputedWithUnverifiedGpu,
                    BranchStabilityScore = m.BranchStabilityScore,
                    RefinementStabilityScore = m.RefinementStabilityScore,
                    ReplayTier = m.ReplayTier, Provenance = m.Provenance,
                };
            }
            return updated;
        }

        var result = solver.Solve(bundle, layout, config, TestProvenance(),
            chiralityPostProcessor: processor);
        var obs = result.ObservationSummary!;

        // 1 left - 1 right = 0
        Assert.Equal(0, obs.NetChiralityIndex);
    }

    [Fact]
    public void Solve_DiagnosticsNotes_IsListSerializable()
    {
        var mesh = SingleTriangle();
        var bundle = BuildTrivialBundle(mesh);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", Dim2Spec(), gaugeDimension: 1, TestProvenance(),
            insertedAssumptionIds: new[] { "P4-IA-003" });
        var config = DefaultConfig(1);
        var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());

        var result = solver.Solve(bundle, layout, config, TestProvenance());

        // Notes should be a List<string> (not IReadOnlyList) for STJ round-trip
        var json = System.Text.Json.JsonSerializer.Serialize(result.Diagnostics);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<FermionSpectralDiagnostics>(json);
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized!.Notes);
    }

    [Fact]
    public void BackgroundSelector_FiltersR2AndAbove()
    {
        var selector = new BackgroundSelector();
        var backgrounds = new[]
        {
            MakeBackgroundRecordWithTier("bg-r0", "R0", 1e-3, 1e-3),
            MakeBackgroundRecordWithTier("bg-r1", "R1", 1e-3, 1e-3),
            MakeBackgroundRecordWithTier("bg-r2", "R2", 1e-3, 1e-3),
            MakeBackgroundRecordWithTier("bg-r3", "R3", 1e-3, 1e-3),
        };

        var selected = selector.Select(backgrounds, minReplayTier: "R2");

        Assert.Equal(2, selected.Count);
        Assert.All(selected, bg => Assert.True(
            bg.ReplayTierAchieved == "R2" || bg.ReplayTierAchieved == "R3"));
    }

    [Fact]
    public void BackgroundSelector_FiltersHighResidual()
    {
        var selector = new BackgroundSelector();
        var backgrounds = new[]
        {
            MakeBackgroundRecordWithTier("bg-good", "R2", 1e-4, 1e-4),
            MakeBackgroundRecordWithTier("bg-bad", "R2", 1.0, 1e-4),
        };

        var selected = selector.Select(backgrounds, minReplayTier: "R2", maxResidualNorm: 1e-2);

        Assert.Single(selected);
        Assert.Equal("bg-good", selected[0].BackgroundId);
    }

    [Fact]
    public void BackgroundSelector_ExcludesRejectedBackgrounds()
    {
        var selector = new BackgroundSelector();
        var backgrounds = new[]
        {
            MakeBackgroundRecordWithTier("bg-admitted", "R2", 1e-4, 1e-4),
            MakeRejectedBackgroundRecord("bg-rejected"),
        };

        var selected = selector.Select(backgrounds, minReplayTier: "R0");

        Assert.Single(selected);
        Assert.Equal("bg-admitted", selected[0].BackgroundId);
    }

    // -------------------------------------------------------
    // Additional fixture helpers for background selection tests
    // -------------------------------------------------------

    private static BackgroundRecord MakeBackgroundRecordWithTier(
        string id, string tier, double residual, double stationarity) =>
        new BackgroundRecord
        {
            BackgroundId = id,
            EnvironmentId = "test-env",
            BranchManifestId = "m1",
            GeometryFingerprint = "test-fp",
            StateArtifactRef = "test-state-ref",
            ResidualNorm = residual,
            StationarityNorm = stationarity,
            AdmissibilityLevel = AdmissibilityLevel.B1,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = residual,
                StationarityNorm = stationarity,
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

    private static BackgroundRecord MakeRejectedBackgroundRecord(string id) =>
        new BackgroundRecord
        {
            BackgroundId = id,
            EnvironmentId = "test-env",
            BranchManifestId = "m1",
            GeometryFingerprint = "test-fp",
            StateArtifactRef = "test-state-ref",
            ResidualNorm = 1e-3,
            StationarityNorm = 1e-3,
            AdmissibilityLevel = AdmissibilityLevel.Rejected,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 1e-3,
                StationarityNorm = 1e-3,
                ObjectiveValue = 0.0,
                GaugeViolation = 0.0,
                SolverIterations = 0,
                SolverConverged = false,
                TerminationReason = "rejected",
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R0",
            RejectionReason = "test-rejection",
            Provenance = TestProvenance(),
        };
}

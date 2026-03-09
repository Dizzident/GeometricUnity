using Gu.Branching;
using Gu.Core;
using Gu.Phase2.Branches;
using Gu.Phase2.Branches.Tests;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.Execution.Tests;

public class Phase2BranchSweepRunnerTests
{
    [Fact]
    public void Sweep_TwoVariants_ProducesTwoRunRecords()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        Assert.Equal(2, result.RunRecords.Count);
        Assert.Equal("env-1", result.EnvironmentId);
        Assert.Equal(family, result.Family);
    }

    [Fact]
    public void Sweep_EachVariant_GetsUniqueArtifactBundle()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        var ids = result.RunRecords.Select(r => r.ArtifactBundle.ArtifactId).ToList();
        Assert.Equal(2, ids.Distinct().Count());
    }

    [Fact]
    public void Sweep_PerBranch_HasCorrectVariantId()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        Assert.Equal("v1", result.RunRecords[0].Variant.Id);
        Assert.Equal("v2", result.RunRecords[1].Variant.Id);
    }

    [Fact]
    public void Sweep_PerBranch_ManifestMatchesVariant()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        Assert.Equal("v1", result.RunRecords[0].Manifest.BranchId);
        Assert.Equal("v2", result.RunRecords[1].Manifest.BranchId);
    }

    [Fact]
    public void Sweep_PerBranch_ArtifactHasProvenance()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        foreach (var record in result.RunRecords)
        {
            Assert.NotNull(record.ArtifactBundle.Provenance);
            Assert.Contains("Phase2Sweep", record.ArtifactBundle.Provenance.Notes ?? "");
            Assert.Equal(record.Variant.Id, record.ArtifactBundle.Branch.BranchId);
        }
    }

    [Fact]
    public void Sweep_PerBranch_ArtifactHasReplayContract()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        foreach (var record in result.RunRecords)
        {
            Assert.True(record.ArtifactBundle.ReplayContract.Deterministic);
            Assert.Equal("R2", record.ArtifactBundle.ReplayContract.ReplayTier);
        }
    }

    [Fact]
    public void Sweep_TracksConvergencePerBranch()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        // Each record has a convergence flag; with stub backend (0 objective/residual),
        // the solver reports not-converged (ResidualOnly mode doesn't converge at 0 iters).
        Assert.All(result.RunRecords, r => Assert.False(r.Converged));
        Assert.Empty(result.ConvergedVariants);
        Assert.Equal(2, result.DivergedVariants.Count);
    }

    [Fact]
    public void Sweep_RecordsSolveMode()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        Assert.Equal(SolveMode.ResidualOnly, result.InnerMode);
        Assert.All(result.RunRecords, r => Assert.Equal(SolveMode.ResidualOnly, r.SolveMode));
    }

    [Fact]
    public void Sweep_WithoutPullback_NoObservedState()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily(includePullback: false);
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        Assert.All(result.RunRecords, r => Assert.Null(r.ObservedState));
        Assert.False(result.AllBranchesHaveObservedOutputs);
    }

    [Fact]
    public void Sweep_HasTimestamps()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        Assert.True(result.SweepCompleted >= result.SweepStarted);
    }

    [Fact]
    public void Constructor_BranchSensitivityMode_Throws()
    {
        var registry = CreateRegistryWithStubs();
        var operatorDispatch = new BranchVariantOperatorDispatch(registry);
        var observationDispatch = new ObservationVariantDispatch();
        var backend = new StubSolverBackend();
        var badOptions = new SolverOptions { Mode = SolveMode.BranchSensitivity };

        Assert.Throws<ArgumentException>(
            () => new Phase2BranchSweepRunner(backend, badOptions, operatorDispatch, observationDispatch));
    }

    [Fact]
    public void Sweep_NullFamily_Throws()
    {
        var (runner, _, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        Assert.Throws<ArgumentNullException>(
            () => runner.Sweep("env-1", omega, a0, null!, baseManifest, MakeGeometry()));
    }

    [Fact]
    public void Sweep_InitialOmegaIsClonedPerBranch()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        // Original omega should be unchanged (it was cloned internally)
        Assert.Equal(0.0, omega.Coefficients[0]);
    }

    [Fact]
    public void Sweep_WithStabilityProbe_PopulatesDiagnostics()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry(),
            stabilityProbe: (_, _, _) => new Gu.Phase2.Stability.HessianSummary
            {
                SmallestEigenvalue = 0.5,
                NegativeModeCount = 0,
                SoftModeCount = 1,
                NearKernelCount = 0,
                StabilityClassification = "soft-modes-present",
                GaugeHandlingMode = "coulomb-slice",
            });

        Assert.All(result.RunRecords, r =>
        {
            Assert.NotNull(r.StabilityDiagnostics);
            Assert.Equal(0.5, r.StabilityDiagnostics!.SmallestEigenvalue);
            Assert.Equal("soft-modes-present", r.StabilityDiagnostics.StabilityClassification);
        });
    }

    [Fact]
    public void Sweep_WithoutStabilityProbe_DiagnosticsNull()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry());

        Assert.All(result.RunRecords, r => Assert.Null(r.StabilityDiagnostics));
    }

    [Fact]
    public void Sweep_TwoBranches_DifferentStability()
    {
        var (runner, family, baseManifest) = CreateTestRunnerAndFamily();
        var omega = MakeZeroField("omega_h", "1");
        var a0 = MakeZeroField("A0_h", "1");

        int callIndex = 0;
        var result = runner.Sweep("env-1", omega, a0, family, baseManifest, MakeGeometry(),
            stabilityProbe: (_, _, _) =>
            {
                callIndex++;
                return new Gu.Phase2.Stability.HessianSummary
                {
                    SmallestEigenvalue = callIndex == 1 ? 1.0 : -0.5,
                    NegativeModeCount = callIndex == 1 ? 0 : 2,
                    SoftModeCount = 0,
                    NearKernelCount = 0,
                    StabilityClassification = callIndex == 1
                        ? "strictly-positive-on-slice"
                        : "negative-modes-saddle",
                    GaugeHandlingMode = "coulomb-slice",
                };
            });

        Assert.Equal(2, result.RunRecords.Count);
        Assert.Equal("strictly-positive-on-slice",
            result.RunRecords[0].StabilityDiagnostics!.StabilityClassification);
        Assert.Equal("negative-modes-saddle",
            result.RunRecords[1].StabilityDiagnostics!.StabilityClassification);
        Assert.NotEqual(
            result.RunRecords[0].StabilityDiagnostics!.SmallestEigenvalue,
            result.RunRecords[1].StabilityDiagnostics!.SmallestEigenvalue);
    }

    // ---- Helpers ----

    private static (Phase2BranchSweepRunner runner, BranchFamilyManifest family, BranchManifest baseManifest) CreateTestRunnerAndFamily(
        bool includePullback = false)
    {
        var registry = CreateRegistryWithStubs();
        var operatorDispatch = new BranchVariantOperatorDispatch(registry);
        var observationDispatch = new ObservationVariantDispatch();
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };

        var runner = new Phase2BranchSweepRunner(
            backend, options, operatorDispatch, observationDispatch,
            pullback: null); // No pullback = no observation extraction

        var v1 = BranchVariantManifestTests.MakeVariant("v1");
        var v2 = BranchVariantManifestTests.MakeVariant("v2");
        var family = BranchVariantManifestTests.MakeFamily("fam-1", v1, v2);
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        return (runner, family, baseManifest);
    }

    private static BranchOperatorRegistry CreateRegistryWithStubs()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterTorsion("local-algebraic", _ => new StubTorsion());
        registry.RegisterShiab("identity-shiab", _ => new StubShiab());
        registry.RegisterBiConnection("simple-a0-omega", _ => new StubBiConnection());
        return registry;
    }

    private static FieldTensor MakeZeroField(string label, string degree)
    {
        return new FieldTensor
        {
            Label = label,
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = degree,
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[6],
            Shape = new[] { 3, 2 },
        };
    }

    private static GeometryContext MakeGeometry()
    {
        return new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            DiscretizationType = "simplicial",
            QuadratureRuleId = "midpoint",
            BasisFamilyId = "P0",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            },
            Patches = Array.Empty<PatchInfo>(),
        };
    }

    // ---- Stubs ----

    private sealed class StubSolverBackend : ISolverBackend
    {
        public DerivedState EvaluateDerived(FieldTensor omega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
        {
            var zeroField = new FieldTensor
            {
                Label = "zero",
                Signature = new TensorSignature
                {
                    AmbientSpaceId = "Y_h",
                    CarrierType = "curvature-2form",
                    Degree = "2",
                    LieAlgebraBasisId = "canonical",
                    ComponentOrderId = "face-major",
                    MemoryLayout = "dense-row-major",
                },
                Coefficients = new double[2],
                Shape = new[] { 1, 2 },
            };
            return new DerivedState
            {
                CurvatureF = zeroField,
                TorsionT = zeroField,
                ShiabS = zeroField,
                ResidualUpsilon = zeroField,
            };
        }

        public double EvaluateObjective(FieldTensor upsilon) => 0.0;

        public ILinearOperator BuildJacobian(FieldTensor omega, FieldTensor a0, FieldTensor curvatureF,
            BranchManifest manifest, GeometryContext geometry)
            => new StubLinearOperator();

        public FieldTensor ComputeGradient(ILinearOperator jacobian, FieldTensor upsilon)
        {
            return new FieldTensor
            {
                Label = "grad",
                Signature = upsilon.Signature,
                Coefficients = new double[upsilon.Coefficients.Length],
                Shape = upsilon.Shape,
            };
        }

        public double ComputeNorm(FieldTensor v) => 0.0;
    }

    private sealed class StubLinearOperator : ILinearOperator
    {
        public FieldTensor Apply(FieldTensor v) => v;
        public FieldTensor ApplyTranspose(FieldTensor v) => v;
        public TensorSignature InputSignature => new()
        {
            AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
            LieAlgebraBasisId = "canonical", ComponentOrderId = "edge-major", MemoryLayout = "dense-row-major",
        };
        public TensorSignature OutputSignature => new()
        {
            AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
            LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major", MemoryLayout = "dense-row-major",
        };
        public int InputDimension => 6;
        public int OutputDimension => 2;
    }

    private sealed class StubTorsion : ITorsionBranchOperator
    {
        public string BranchId => "local-algebraic";
        public string OutputCarrierType => "curvature-2form";
        public TensorSignature OutputSignature => new()
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        };
        public FieldTensor Evaluate(FieldTensor omega, FieldTensor a0,
            BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
        public FieldTensor Linearize(FieldTensor omega, FieldTensor a0,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
    }

    private sealed class StubShiab : IShiabBranchOperator
    {
        public string BranchId => "identity-shiab";
        public string OutputCarrierType => "curvature-2form";
        public TensorSignature OutputSignature => new()
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        };
        public FieldTensor Evaluate(FieldTensor curvatureF, FieldTensor omega,
            BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
        public FieldTensor Linearize(FieldTensor curvatureF, FieldTensor omega,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
    }

    private sealed class StubBiConnection : IBiConnectionStrategy
    {
        public string StrategyId => "simple-a0-omega";
        public BiConnectionResult Evaluate(FieldTensor omega, FieldTensor a0,
            BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
        public BiConnectionResult Linearize(FieldTensor omega, FieldTensor a0,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
    }
}

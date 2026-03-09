using Gu.Branching;
using Gu.Core;
using Gu.Phase2.Branches;
using Gu.Phase2.Branches.Tests;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Execution;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.IntegrationTests;

/// <summary>
/// Full pipeline A0-sweep integration test (Study S1, GAP-14).
/// Creates a BranchFamilyManifest with 2 variants differing only in A0Variant,
/// runs Phase2BranchSweepRunner, asserts BranchRunRecords are produced,
/// runs CanonicityAnalyzer, and asserts PairwiseDistanceMatrix dimensions
/// and CanonicityEvidenceRecord is produced.
/// </summary>
public class FullPipelineSweepTests
{
    [Fact]
    public void A0Sweep_TwoVariants_ProducesBranchRunRecords()
    {
        var (runner, family, baseManifest) = CreateA0SweepSetup();
        var omega = MakeZeroField("omega_h", "1", 6);
        var a0 = MakeZeroField("A0_h", "1", 6);

        var result = runner.Sweep("env-2d", omega, a0, family, baseManifest, MakeGeometry());

        Assert.Equal(2, result.RunRecords.Count);
        Assert.Equal("env-2d", result.EnvironmentId);
        Assert.All(result.RunRecords, r => Assert.NotNull(r.ArtifactBundle));
        Assert.All(result.RunRecords, r => Assert.NotNull(r.Manifest));
    }

    [Fact]
    public void A0Sweep_CanonicityAnalyzer_ProducesPairwiseDistanceMatrix()
    {
        var (runner, family, baseManifest) = CreateA0SweepSetup();
        var omega = MakeZeroField("omega_h", "1", 6);
        var a0 = MakeZeroField("A0_h", "1", 6);

        var sweepResult = runner.Sweep("env-2d", omega, a0, family, baseManifest, MakeGeometry());

        var analyzer = new CanonicityAnalyzer();
        var dObs = analyzer.ComputeObservedDistances(sweepResult, family.DefaultEquivalence);
        var dDyn = analyzer.ComputeDynamicDistances(sweepResult);
        var dConv = analyzer.ComputeConvergenceDistances(sweepResult);

        Assert.Equal(2, dObs.BranchIds.Count);
        Assert.Equal(2, dDyn.BranchIds.Count);
        Assert.Equal(2, dConv.BranchIds.Count);
        Assert.Equal("D_obs_max", dObs.MetricId);
        Assert.Equal("D_dyn", dDyn.MetricId);
        Assert.Equal("D_conv", dConv.MetricId);
    }

    [Fact]
    public void A0Sweep_CanonicityEvidence_IsProduced()
    {
        var (runner, family, baseManifest) = CreateA0SweepSetup();
        var omega = MakeZeroField("omega_h", "1", 6);
        var a0 = MakeZeroField("A0_h", "1", 6);

        var sweepResult = runner.Sweep("env-2d", omega, a0, family, baseManifest, MakeGeometry());

        var analyzer = new CanonicityAnalyzer();
        var evidence = analyzer.Evaluate(sweepResult, family.DefaultEquivalence, "A0-variant");

        Assert.NotNull(evidence);
        Assert.NotEmpty(evidence.EvidenceId);
        Assert.NotEmpty(evidence.StudyId);
        Assert.Contains(evidence.Verdict, new[] { "consistent", "inconsistent", "inconclusive" });
    }

    [Fact]
    public void A0Sweep_AgreementMatrix_MatchesBranchCount()
    {
        var (runner, family, baseManifest) = CreateA0SweepSetup();
        var omega = MakeZeroField("omega_h", "1", 6);
        var a0 = MakeZeroField("A0_h", "1", 6);

        var sweepResult = runner.Sweep("env-2d", omega, a0, family, baseManifest, MakeGeometry());

        var analyzer = new CanonicityAnalyzer();
        var agreement = analyzer.ComputeAgreementMatrix(sweepResult);

        Assert.Equal(2, agreement.BranchIds.Count);
        Assert.Equal(2, agreement.Classifications.Count);
    }

    [Fact]
    public void A0Sweep_ExtractionAgreement_MatchesBranchCount()
    {
        var (runner, family, baseManifest) = CreateA0SweepSetup();
        var omega = MakeZeroField("omega_h", "1", 6);
        var a0 = MakeZeroField("A0_h", "1", 6);

        var sweepResult = runner.Sweep("env-2d", omega, a0, family, baseManifest, MakeGeometry());

        var analyzer = new CanonicityAnalyzer();
        var extraction = analyzer.ComputeExtractionAgreement(sweepResult);
        var admissibility = analyzer.ComputeAdmissibilityAgreement(sweepResult);

        Assert.Equal(2, extraction.BranchIds.Count);
        Assert.Equal(2, admissibility.BranchIds.Count);
    }

    // --- Helpers ---

    private static (Phase2BranchSweepRunner runner, BranchFamilyManifest family, BranchManifest baseManifest) CreateA0SweepSetup()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterTorsion("local-algebraic", _ => new StubTorsion());
        registry.RegisterShiab("identity-shiab", _ => new StubShiab());
        registry.RegisterBiConnection("simple-a0-omega", _ => new StubBiConnection());

        var operatorDispatch = new BranchVariantOperatorDispatch(registry);
        var observationDispatch = new ObservationVariantDispatch();
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };

        var runner = new Phase2BranchSweepRunner(
            backend, options, operatorDispatch, observationDispatch, pullback: null);

        // Two variants differing only in A0Variant
        var v1 = BranchVariantManifestTests.MakeVariant("v-a0-flat", familyId: "fam-a0-sweep");
        var v2 = new BranchVariantManifest
        {
            Id = "v-a0-random",
            ParentFamilyId = "fam-a0-sweep",
            A0Variant = "random-perturbation",
            BiConnectionVariant = v1.BiConnectionVariant,
            TorsionVariant = v1.TorsionVariant,
            ShiabVariant = v1.ShiabVariant,
            ObservationVariant = v1.ObservationVariant,
            ExtractionVariant = v1.ExtractionVariant,
            GaugeVariant = v1.GaugeVariant,
            RegularityVariant = v1.RegularityVariant,
            PairingVariant = v1.PairingVariant,
            ExpectedClaimCeiling = v1.ExpectedClaimCeiling,
        };

        var equivalence = new EquivalenceSpec
        {
            Id = "eq-a0-sweep",
            Name = "A0 sweep equivalence",
            ComparedObjectClasses = new[] { "observed-output" },
            NormalizationProcedure = "none",
            AllowedTransformations = Array.Empty<string>(),
            Metrics = new[] { "D_obs" },
            Tolerances = new Dictionary<string, double> { ["D_obs"] = 1e-6 },
            InterpretationRule = "all-within-tolerance",
        };

        var family = new BranchFamilyManifest
        {
            FamilyId = "fam-a0-sweep",
            Description = "A0 variant sweep for integration test",
            Variants = new[] { v1, v2 },
            DefaultEquivalence = equivalence,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        return (runner, family, baseManifest);
    }

    private static FieldTensor MakeZeroField(string label, string degree, int size)
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
            Coefficients = new double[size],
            Shape = new[] { size / 2, 2 },
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

    // --- Stubs ---

    private sealed class StubSolverBackend : ISolverBackend
    {
        public DerivedState EvaluateDerived(FieldTensor omega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
        {
            var zeroField = new FieldTensor
            {
                Label = "zero",
                Signature = new TensorSignature
                {
                    AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
                    LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major", MemoryLayout = "dense-row-major",
                },
                Coefficients = new double[2],
                Shape = new[] { 1, 2 },
            };
            return new DerivedState
            {
                CurvatureF = zeroField, TorsionT = zeroField,
                ShiabS = zeroField, ResidualUpsilon = zeroField,
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
                Label = "grad", Signature = upsilon.Signature,
                Coefficients = new double[upsilon.Coefficients.Length], Shape = upsilon.Shape,
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
            AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
            LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major", MemoryLayout = "dense-row-major",
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
            AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
            LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major", MemoryLayout = "dense-row-major",
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

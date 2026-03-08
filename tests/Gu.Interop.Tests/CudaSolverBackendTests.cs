using Gu.Core;
using Gu.Interop;
using Gu.Solvers;

namespace Gu.Interop.Tests;

/// <summary>
/// Tests for CudaSolverBackend using CpuReferenceBackend as the native backend.
/// Verifies that the solver backend correctly bridges between semantic types
/// and packed GPU buffers.
/// </summary>
public class CudaSolverBackendTests
{
    private static TensorSignature CreateOmegaSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "basis-standard",
        ComponentOrderId = "order-row-major",
        MemoryLayout = "dense-row-major",
        NumericPrecision = "float64",
    };

    private static BranchManifest CreateManifest() => new()
    {
        BranchId = "test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "abc",
        ActiveGeometryBranch = "simplicial-4d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "basis-standard",
        ComponentOrderId = "order-row-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-killing",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };

    private static GeometryContext CreateGeometry()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 };
        return new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = "midpoint",
            BasisFamilyId = "whitney-0",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = ambientSpace,
                TargetSpace = baseSpace,
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = baseSpace,
                TargetSpace = ambientSpace,
            },
            Patches = new[]
            {
                new PatchInfo { PatchId = "patch-0", ElementCount = 10 },
            },
        };
    }

    private static ManifestSnapshot CreateManifestSnapshot() => new()
    {
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraDimension = 3,
        LieAlgebraId = "su2",
        MeshCellCount = 2,
        MeshVertexCount = 5,
        ComponentOrderId = "order-row-major",
        TorsionBranchId = "trivial",
        ShiabBranchId = "identity",
    };

    private CudaSolverBackend CreateBackend()
    {
        var native = new CpuReferenceBackend();
        var backend = new CudaSolverBackend(native, ownsNative: true);
        backend.Initialize(CreateManifestSnapshot());
        return backend;
    }

    [Fact]
    public void EvaluateDerived_ReturnsAllFields()
    {
        using var backend = CreateBackend();

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 2, 3 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[6],
            Shape = new[] { 2, 3 },
        };

        var derived = backend.EvaluateDerived(omega, a0, CreateManifest(), CreateGeometry());

        Assert.NotNull(derived.CurvatureF);
        Assert.NotNull(derived.TorsionT);
        Assert.NotNull(derived.ShiabS);
        Assert.NotNull(derived.ResidualUpsilon);
        Assert.Equal(6, derived.ResidualUpsilon.Coefficients.Length);
    }

    [Fact]
    public void EvaluateDerived_ResidualIsShiabMinusTorsion()
    {
        using var backend = CreateBackend();

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 3.0, 5.0, 7.0 },
            Shape = new[] { 1, 3 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[3],
            Shape = new[] { 1, 3 },
        };

        var derived = backend.EvaluateDerived(omega, a0, CreateManifest(), CreateGeometry());

        // With identity Shiab (S=omega) and trivial torsion (T=0):
        // Upsilon = S - T = omega - 0 = omega
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(omega.Coefficients[i], derived.ResidualUpsilon.Coefficients[i], precision: 12);
        }
    }

    [Fact]
    public void EvaluateObjective_ReturnsHalfSquaredNorm()
    {
        using var backend = CreateBackend();

        var upsilon = new FieldTensor
        {
            Label = "Upsilon_h",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 3.0, 4.0 },
            Shape = new[] { 1, 2 },
        };

        double objective = backend.EvaluateObjective(upsilon);

        // (1/2) * (9 + 16) = 12.5
        Assert.Equal(12.5, objective, precision: 10);
    }

    [Fact]
    public void ComputeNorm_ReturnsL2Norm()
    {
        using var backend = CreateBackend();

        var v = new FieldTensor
        {
            Label = "v",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 3.0, 4.0 },
            Shape = new[] { 1, 2 },
        };

        double norm = backend.ComputeNorm(v);
        Assert.Equal(5.0, norm, precision: 12);
    }

    [Fact]
    public void BuildJacobian_ReturnsValidOperator()
    {
        using var backend = CreateBackend();

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[3],
            Shape = new[] { 1, 3 },
        };

        // Initialize buffers via EvaluateDerived first
        var derived = backend.EvaluateDerived(omega, a0, CreateManifest(), CreateGeometry());

        var jacobian = backend.BuildJacobian(omega, a0, derived.CurvatureF, CreateManifest(), CreateGeometry());

        Assert.NotNull(jacobian);
        Assert.Equal(3, jacobian.InputDimension);
        Assert.Equal(3, jacobian.OutputDimension);
    }

    [Fact]
    public void SolverOrchestrator_ModeA_WorksWithCudaBackend()
    {
        using var backend = CreateBackend();

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 2, 3 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[6],
            Shape = new[] { 2, 3 },
        };

        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        var orchestrator = new SolverOrchestrator(backend, options);

        var result = orchestrator.Solve(omega, a0, CreateManifest(), CreateGeometry());

        Assert.Equal("Mode A: residual-only evaluation", result.TerminationReason);
        Assert.True(result.FinalObjective > 0, "Objective should be positive for non-zero omega");
        Assert.NotNull(result.FinalDerivedState);
    }

    [Fact]
    public void SolverOrchestrator_ModeB_DecreasesObjective()
    {
        using var backend = CreateBackend();

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[3],
            Shape = new[] { 1, 3 },
        };

        var options = new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 5,
            InitialStepSize = 0.01,
            GaugePenaltyLambda = 0.0,
        };
        var orchestrator = new SolverOrchestrator(backend, options);

        var result = orchestrator.Solve(omega, a0, CreateManifest(), CreateGeometry());

        // With identity Shiab and trivial torsion, the gradient descent
        // should reduce the objective (Upsilon = omega, so minimizing moves omega toward zero).
        Assert.True(result.History.Count > 0);
        if (result.History.Count >= 2)
        {
            Assert.True(result.History[^1].Objective <= result.History[0].Objective,
                "Objective should not increase during optimization");
        }
    }

    [Fact]
    public void Dispose_FreesResources()
    {
        var native = new CpuReferenceBackend();
        var backend = new CudaSolverBackend(native, ownsNative: true);
        backend.Initialize(CreateManifestSnapshot());

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = CreateOmegaSignature(),
            Coefficients = new double[3],
            Shape = new[] { 1, 3 },
        };

        // Force buffer allocation
        backend.EvaluateDerived(omega, a0, CreateManifest(), CreateGeometry());

        // Should not throw
        backend.Dispose();
    }
}

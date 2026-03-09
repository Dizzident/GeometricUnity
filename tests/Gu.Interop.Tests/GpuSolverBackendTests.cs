using Gu.Branching;
using Gu.Core;
using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// Tests for GpuSolverBackend: ISolverBackend implementation that wraps
/// INativeBackend for GPU dispatch. Uses CpuReferenceBackend as the
/// underlying native backend (no actual GPU required).
/// </summary>
public class GpuSolverBackendTests
{
    private static ManifestSnapshot CreateManifest() => new()
    {
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraDimension = 3,
        LieAlgebraId = "su2",
        MeshCellCount = 10,
        MeshVertexCount = 20,
        ComponentOrderId = "order-row-major",
        TorsionBranchId = "trivial",
        ShiabBranchId = "identity",
    };

    private static TensorSignature CreateSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "basis-standard",
        ComponentOrderId = "order-row-major",
        MemoryLayout = "dense-row-major",
        NumericPrecision = "float64",
    };

    private static GpuSolverBackend CreateInitializedBackend()
    {
        var native = new CpuReferenceBackend();
        var backend = new GpuSolverBackend(native, ownsBackend: true);
        backend.Initialize(CreateManifest());
        return backend;
    }

    [Fact]
    public void Initialize_Succeeds()
    {
        using var backend = CreateInitializedBackend();
        // No exception means success
    }

    [Fact]
    public void EvaluateDerived_ProducesAllFields()
    {
        using var backend = CreateInitializedBackend();

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 6 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0_h",
            Signature = CreateSignature(),
            Coefficients = new double[6],
            Shape = new[] { 6 },
        };

        var derived = backend.EvaluateDerived(omega, a0, CreateBranchManifest(), CreateGeometryContext());

        Assert.NotNull(derived.CurvatureF);
        Assert.NotNull(derived.TorsionT);
        Assert.NotNull(derived.ShiabS);
        Assert.NotNull(derived.ResidualUpsilon);

        Assert.Equal("F_h", derived.CurvatureF.Label);
        Assert.Equal("T_h", derived.TorsionT.Label);
        Assert.Equal("S_h", derived.ShiabS.Label);
        Assert.Equal("Upsilon_h", derived.ResidualUpsilon.Label);
    }

    [Fact]
    public void EvaluateDerived_FaceValuedBuffersHaveCorrectSize()
    {
        // Verify that face-valued outputs (F, T, S, Upsilon) have size faceCount*dimG,
        // NOT the same size as edge-valued omega
        var native = new CpuReferenceBackend();
        using var backend = new GpuSolverBackend(native, ownsBackend: true);
        backend.Initialize(CreateManifest());

        // Use a geometry context with 3 faces (nFace = 3*3 = 9)
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4, FaceCount = 3 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14, FaceCount = 3, EdgeCount = 5 };
        var geometry = new GeometryContext
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
            Patches = new[] { new PatchInfo { PatchId = "patch-0", ElementCount = 10 } },
        };

        // omega is edge-valued with 15 elements (5 edges * 3 dimG)
        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateSignature(),
            Coefficients = new double[15],
            Shape = new[] { 15 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0_h",
            Signature = CreateSignature(),
            Coefficients = new double[15],
            Shape = new[] { 15 },
        };

        var derived = backend.EvaluateDerived(omega, a0, CreateBranchManifest(), geometry);

        // Face-valued outputs should be faceCount(3) * dimG(3) = 9, NOT 15
        Assert.Equal(9, derived.CurvatureF.Coefficients.Length);
        Assert.Equal(9, derived.TorsionT.Coefficients.Length);
        Assert.Equal(9, derived.ShiabS.Coefficients.Length);
        Assert.Equal(9, derived.ResidualUpsilon.Coefficients.Length);
    }

    [Fact]
    public void EvaluateDerived_CurvatureMatchesStub()
    {
        using var backend = CreateInitializedBackend();

        // omega is edge-valued (edgeCount * dimG); curvature is face-valued (faceCount * dimG)
        // With FaceCount=2 and dimG=3, nFace=6
        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 6 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0_h",
            Signature = CreateSignature(),
            Coefficients = new double[6],
            Shape = new[] { 6 },
        };

        var derived = backend.EvaluateDerived(omega, a0, CreateBranchManifest(), CreateGeometryContext());

        // Stub (no topology): curvature = copy of min(omega, curvature) elements
        // Both are size 6 (nFace=6, omega=6), so full copy
        Assert.Equal(6, derived.CurvatureF.Coefficients.Length);
        Assert.Equal(omega.Coefficients, derived.CurvatureF.Coefficients);
    }

    [Fact]
    public void EvaluateDerived_TorsionIsZero()
    {
        using var backend = CreateInitializedBackend();

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateSignature(),
            Coefficients = new double[] { 5.0, 10.0, 15.0, 20.0, 25.0, 30.0 },
            Shape = new[] { 6 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0_h",
            Signature = CreateSignature(),
            Coefficients = new double[6],
            Shape = new[] { 6 },
        };

        var derived = backend.EvaluateDerived(omega, a0, CreateBranchManifest(), CreateGeometryContext());

        // Stub: torsion = 0, face-valued (faceCount * dimG = 6)
        Assert.Equal(6, derived.TorsionT.Coefficients.Length);
        Assert.All(derived.TorsionT.Coefficients, v => Assert.Equal(0.0, v));
    }

    [Fact]
    public void EvaluateDerived_ResidualIsShiabMinusTorsion()
    {
        using var backend = CreateInitializedBackend();

        // Use size matching nFace = faceCount(2) * dimG(3) = 6
        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateSignature(),
            Coefficients = new double[] { 3.0, 4.0, 5.0, 6.0, 7.0, 8.0 },
            Shape = new[] { 6 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0_h",
            Signature = CreateSignature(),
            Coefficients = new double[6],
            Shape = new[] { 6 },
        };

        var derived = backend.EvaluateDerived(omega, a0, CreateBranchManifest(), CreateGeometryContext());

        // Stub: S = omega (copied), T = 0, so Upsilon = omega - 0 = omega
        Assert.Equal(6, derived.ResidualUpsilon.Coefficients.Length);
        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(omega.Coefficients[i], derived.ResidualUpsilon.Coefficients[i]);
        }
    }

    [Fact]
    public void EvaluateObjective_ComputesHalfSquaredNorm()
    {
        using var backend = CreateInitializedBackend();

        var upsilon = new FieldTensor
        {
            Label = "Upsilon_h",
            Signature = CreateSignature(),
            Coefficients = new double[] { 3.0, 4.0 },
            Shape = new[] { 2 },
        };

        double objective = backend.EvaluateObjective(upsilon);

        // (1/2) * (9 + 16) = 12.5
        Assert.Equal(12.5, objective, precision: 10);
    }

    [Fact]
    public void ComputeNorm_ReturnsSqrtTwoTimesObjective()
    {
        using var backend = CreateInitializedBackend();

        var v = new FieldTensor
        {
            Label = "v",
            Signature = CreateSignature(),
            Coefficients = new double[] { 3.0, 4.0 },
            Shape = new[] { 2 },
        };

        double norm = backend.ComputeNorm(v);

        // sqrt(2 * 12.5) = sqrt(25) = 5
        Assert.Equal(5.0, norm, precision: 10);
    }

    [Fact]
    public void BuildJacobian_ReturnsOperator()
    {
        using var backend = CreateInitializedBackend();

        var omega = new FieldTensor
        {
            Label = "omega",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 6 },
        };

        var jacobian = backend.BuildJacobian(omega, omega, omega, CreateBranchManifest(), CreateGeometryContext());
        Assert.NotNull(jacobian);
    }

    [Fact]
    public void ComputeGradient_NullJacobian_ThrowsArgumentNull()
    {
        using var backend = CreateInitializedBackend();

        var v = new FieldTensor
        {
            Label = "v",
            Signature = CreateSignature(),
            Coefficients = new double[3],
            Shape = new[] { 3 },
        };

        Assert.Throws<ArgumentNullException>(() =>
            backend.ComputeGradient(null!, v));
    }

    [Fact]
    public void OperationBeforeInitialize_Throws()
    {
        using var backend = new GpuSolverBackend(new CpuReferenceBackend());

        var v = new FieldTensor
        {
            Label = "v",
            Signature = CreateSignature(),
            Coefficients = new double[3],
            Shape = new[] { 3 },
        };

        Assert.Throws<InvalidOperationException>(() => backend.EvaluateObjective(v));
    }

    [Fact]
    public void Dispose_DisposesOwnedBackend()
    {
        var native = new CpuReferenceBackend();
        var backend = new GpuSolverBackend(native, ownsBackend: true);
        backend.Initialize(CreateManifest());
        backend.Dispose();

        // After dispose, the native backend should also be disposed
        Assert.Throws<ObjectDisposedException>(() =>
            native.AllocateBuffer(BufferLayoutDescriptor.CreateSoA("x", new[] { "a" }, 1)));
    }

    [Fact]
    public void Dispose_DoesNotDispose_NonOwnedBackend()
    {
        var native = new CpuReferenceBackend();
        var backend = new GpuSolverBackend(native, ownsBackend: false);
        backend.Initialize(CreateManifest());
        backend.Dispose();

        // Native backend should still be usable
        native.Initialize(CreateManifest());
        var buf = native.AllocateBuffer(BufferLayoutDescriptor.CreateSoA("x", new[] { "a" }, 1));
        Assert.NotNull(buf);
        native.Dispose();
    }

    [Fact]
    public void Disposed_ThrowsOnOperation()
    {
        var backend = CreateInitializedBackend();
        backend.Dispose();

        var v = new FieldTensor
        {
            Label = "v",
            Signature = CreateSignature(),
            Coefficients = new double[3],
            Shape = new[] { 3 },
        };

        Assert.Throws<ObjectDisposedException>(() => backend.EvaluateObjective(v));
    }

    [Fact]
    public void NativeBackend_Property_ReturnsBackend()
    {
        var native = new CpuReferenceBackend();
        using var backend = new GpuSolverBackend(native, ownsBackend: true);

        Assert.Same(native, backend.NativeBackend);
    }

    // Helper methods for creating required context objects

    private static BranchManifest CreateBranchManifest() => new()
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

    private static GeometryContext CreateGeometryContext()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4, FaceCount = 2 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14, FaceCount = 2, EdgeCount = 4 };
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
}

using Gu.Core;
using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// CPU/GPU parity tests for Milestone 9.
///
/// These tests verify that the GPU backend (using CpuReferenceBackend as a
/// stand-in for CUDA) produces identical results to the C# CpuReferenceBackend
/// for all kernel stages. This validates:
///
/// 1. The interop plumbing (pack/upload/compute/download/unpack) is correct
/// 2. The stub kernel math matches the C# reference
/// 3. The ParityChecker framework correctly detects matches and mismatches
/// 4. GpuSolverBackend produces the same DerivedState as direct backend calls
///
/// When real CUDA kernels are available (M10+), these same tests will verify
/// GPU parity by swapping in CudaNativeBackend.
/// </summary>
public class NativeParityTests
{
    private const double Tolerance = 1e-14;

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

    // ---------------------------------------------------------------
    // Stage-by-stage parity: CpuReferenceBackend vs CpuReferenceBackend
    // (validates that ParityChecker + buffer plumbing works correctly)
    // ---------------------------------------------------------------

    [Fact]
    public void Parity_Curvature_TwoIdenticalBackends()
    {
        using var ref1 = new CpuReferenceBackend();
        using var ref2 = new CpuReferenceBackend();
        ref1.Initialize(CreateManifest());
        ref2.Initialize(CreateManifest());

        int n = 12;
        var omegaData = GenerateTestData(n, seed: 42);
        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, n);

        var buf1In = ref1.AllocateBuffer(layout);
        var buf1Out = ref1.AllocateBuffer(layout);
        var buf2In = ref2.AllocateBuffer(layout);
        var buf2Out = ref2.AllocateBuffer(layout);

        ref1.UploadBuffer(buf1In, omegaData);
        ref2.UploadBuffer(buf2In, omegaData);

        ref1.EvaluateCurvature(buf1In, buf1Out);
        ref2.EvaluateCurvature(buf2In, buf2Out);

        var result1 = new double[n];
        var result2 = new double[n];
        ref1.DownloadBuffer(buf1Out, result1);
        ref2.DownloadBuffer(buf2Out, result2);

        var record = ParityChecker.CompareResults(
            "curvature", result1, result2, "cpu-ref-1", "cpu-ref-2", Tolerance);

        Assert.True(record.Passed, $"Curvature parity failed: {record.Message}");
        Assert.Equal(0.0, record.MaxAbsoluteError);
    }

    [Fact]
    public void Parity_Torsion_TwoIdenticalBackends()
    {
        using var ref1 = new CpuReferenceBackend();
        using var ref2 = new CpuReferenceBackend();
        ref1.Initialize(CreateManifest());
        ref2.Initialize(CreateManifest());

        int n = 8;
        var omegaData = GenerateTestData(n, seed: 123);
        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, n);

        var buf1In = ref1.AllocateBuffer(layout);
        var buf1Out = ref1.AllocateBuffer(layout);
        var buf2In = ref2.AllocateBuffer(layout);
        var buf2Out = ref2.AllocateBuffer(layout);

        ref1.UploadBuffer(buf1In, omegaData);
        ref2.UploadBuffer(buf2In, omegaData);

        ref1.EvaluateTorsion(buf1In, buf1Out);
        ref2.EvaluateTorsion(buf2In, buf2Out);

        var result1 = new double[n];
        var result2 = new double[n];
        ref1.DownloadBuffer(buf1Out, result1);
        ref2.DownloadBuffer(buf2Out, result2);

        var record = ParityChecker.CompareResults(
            "torsion", result1, result2, "cpu-ref-1", "cpu-ref-2", Tolerance);

        Assert.True(record.Passed, $"Torsion parity failed: {record.Message}");
    }

    [Fact]
    public void Parity_Residual_TwoIdenticalBackends()
    {
        using var ref1 = new CpuReferenceBackend();
        using var ref2 = new CpuReferenceBackend();
        ref1.Initialize(CreateManifest());
        ref2.Initialize(CreateManifest());

        int n = 6;
        var shiabData = new double[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 };
        var torsionData = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, n);

        var s1 = ref1.AllocateBuffer(layout);
        var t1 = ref1.AllocateBuffer(layout);
        var r1 = ref1.AllocateBuffer(layout);
        var s2 = ref2.AllocateBuffer(layout);
        var t2 = ref2.AllocateBuffer(layout);
        var r2 = ref2.AllocateBuffer(layout);

        ref1.UploadBuffer(s1, shiabData);
        ref1.UploadBuffer(t1, torsionData);
        ref2.UploadBuffer(s2, shiabData);
        ref2.UploadBuffer(t2, torsionData);

        ref1.EvaluateResidual(s1, t1, r1);
        ref2.EvaluateResidual(s2, t2, r2);

        var result1 = new double[n];
        var result2 = new double[n];
        ref1.DownloadBuffer(r1, result1);
        ref2.DownloadBuffer(r2, result2);

        var record = ParityChecker.CompareResults(
            "residual", result1, result2, "cpu-ref-1", "cpu-ref-2", Tolerance);

        Assert.True(record.Passed, $"Residual parity failed: {record.Message}");

        // Verify actual values: Upsilon = S - T
        Assert.Equal(9.0, result1[0]);
        Assert.Equal(18.0, result1[1]);
        Assert.Equal(54.0, result1[5]);
    }

    [Fact]
    public void Parity_Objective_TwoIdenticalBackends()
    {
        using var ref1 = new CpuReferenceBackend();
        using var ref2 = new CpuReferenceBackend();
        ref1.Initialize(CreateManifest());
        ref2.Initialize(CreateManifest());

        int n = 5;
        var residualData = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, n);

        var buf1 = ref1.AllocateBuffer(layout);
        var buf2 = ref2.AllocateBuffer(layout);

        ref1.UploadBuffer(buf1, residualData);
        ref2.UploadBuffer(buf2, residualData);

        double obj1 = ref1.EvaluateObjective(buf1);
        double obj2 = ref2.EvaluateObjective(buf2);

        var record = ParityChecker.CompareScalar(
            "objective", obj1, obj2, "cpu-ref-1", "cpu-ref-2", Tolerance);

        Assert.True(record.Passed, $"Objective parity failed: {record.Message}");

        // Verify value: (1/2)(1 + 4 + 9 + 16 + 25) = 27.5
        Assert.Equal(27.5, obj1, precision: 10);
    }

    // ---------------------------------------------------------------
    // Full pipeline parity via ParityChecker.RunFullResidualParity
    // ---------------------------------------------------------------

    [Fact]
    public void FullPipeline_Parity_AllStages_Pass()
    {
        using var reference = new CpuReferenceBackend();
        using var target = new CpuReferenceBackend();

        var checker = new ParityChecker(reference, target);
        var omega = GenerateTestData(20, seed: 99);

        var records = checker.RunFullResidualParity(CreateManifest(), omega, Tolerance);

        Assert.Equal(5, records.Count);
        Assert.All(records, r =>
        {
            Assert.True(r.Passed, $"Stage '{r.KernelName}' failed: {r.Message}");
            Assert.Equal(0.0, r.MaxAbsoluteError);
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void FullPipeline_Parity_VariousBufferSizes(int n)
    {
        using var reference = new CpuReferenceBackend();
        using var target = new CpuReferenceBackend();

        var checker = new ParityChecker(reference, target);
        var omega = GenerateTestData(n, seed: n);

        var records = checker.RunFullResidualParity(CreateManifest(), omega, Tolerance);

        Assert.Equal(5, records.Count);
        Assert.All(records, r =>
            Assert.True(r.Passed, $"Stage '{r.KernelName}' failed for n={n}: {r.Message}"));
    }

    // ---------------------------------------------------------------
    // GpuSolverBackend parity with direct CpuReferenceBackend
    // ---------------------------------------------------------------

    [Fact]
    public void GpuSolverBackend_EvaluateDerived_MatchesCpuReference()
    {
        using var directBackend = new CpuReferenceBackend();
        directBackend.Initialize(CreateManifest());

        using var gpuBackend = new GpuSolverBackend(new CpuReferenceBackend(), ownsBackend: true);
        gpuBackend.Initialize(CreateManifest());

        int n = 15;
        var omegaData = GenerateTestData(n, seed: 77);
        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, n);

        // Compute via direct backend
        var directOmega = directBackend.AllocateBuffer(layout);
        var directCurv = directBackend.AllocateBuffer(layout);
        var directTorsion = directBackend.AllocateBuffer(layout);
        var directShiab = directBackend.AllocateBuffer(layout);
        var directResidual = directBackend.AllocateBuffer(layout);

        directBackend.UploadBuffer(directOmega, omegaData);
        directBackend.EvaluateCurvature(directOmega, directCurv);
        directBackend.EvaluateTorsion(directOmega, directTorsion);
        directBackend.EvaluateShiab(directOmega, directShiab);
        directBackend.EvaluateResidual(directShiab, directTorsion, directResidual);

        var directResidualData = new double[n];
        directBackend.DownloadBuffer(directResidual, directResidualData);
        double directObjective = directBackend.EvaluateObjective(directResidual);

        // Compute via GpuSolverBackend
        var omegaTensor = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateSignature(),
            Coefficients = omegaData,
            Shape = new[] { n },
        };
        var a0Tensor = new FieldTensor
        {
            Label = "a0_h",
            Signature = CreateSignature(),
            Coefficients = new double[n],
            Shape = new[] { n },
        };

        var derived = gpuBackend.EvaluateDerived(
            omegaTensor, a0Tensor, CreateBranchManifest(), CreateGeometryContext());

        // Compare residuals
        var record = ParityChecker.CompareResults(
            "residual-via-solver",
            directResidualData,
            derived.ResidualUpsilon.Coefficients,
            "cpu-direct",
            "gpu-solver",
            Tolerance);

        Assert.True(record.Passed, $"Residual parity failed: {record.Message}");

        // Compare objective
        double gpuObjective = gpuBackend.EvaluateObjective(derived.ResidualUpsilon);
        var objRecord = ParityChecker.CompareScalar(
            "objective-via-solver",
            directObjective,
            gpuObjective,
            "cpu-direct",
            "gpu-solver",
            Tolerance);

        Assert.True(objRecord.Passed, $"Objective parity failed: {objRecord.Message}");
    }

    [Fact]
    public void GpuSolverBackend_ComputeNorm_MatchesCpuReference()
    {
        using var gpuBackend = new GpuSolverBackend(new CpuReferenceBackend(), ownsBackend: true);
        gpuBackend.Initialize(CreateManifest());

        var v = new FieldTensor
        {
            Label = "v",
            Signature = CreateSignature(),
            Coefficients = new double[] { 3.0, 4.0 },
            Shape = new[] { 2 },
        };

        double norm = gpuBackend.ComputeNorm(v);

        // Expected: sqrt(9 + 16) = 5
        Assert.Equal(5.0, norm, precision: 10);
    }

    // ---------------------------------------------------------------
    // NativeFieldBuffer integration with parity checking
    // ---------------------------------------------------------------

    [Fact]
    public void NativeFieldBuffer_PackedData_ParityWithDirectUpload()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());

        int n = 8;
        var data = GenerateTestData(n, seed: 55);
        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, n);

        // Direct upload path
        var directBuf = backend.AllocateBuffer(layout);
        backend.UploadBuffer(directBuf, data);

        // NativeFieldBuffer path
        using var fieldBuffer = new NativeFieldBuffer(data);
        var pinnedBuf = backend.AllocateBuffer(layout);
        backend.UploadBuffer(pinnedBuf, fieldBuffer.ReadOnlySpan);

        // Download and compare
        var directResult = new double[n];
        var pinnedResult = new double[n];
        backend.DownloadBuffer(directBuf, directResult);
        backend.DownloadBuffer(pinnedBuf, pinnedResult);

        var record = ParityChecker.CompareResults(
            "buffer-upload", directResult, pinnedResult,
            "direct", "pinned", Tolerance);

        Assert.True(record.Passed, $"Buffer upload parity failed: {record.Message}");
    }

    // ---------------------------------------------------------------
    // NativeMeshDescriptor integration
    // ---------------------------------------------------------------

    [Fact]
    public void NativeMeshDescriptor_RoundTrip_ViaManifest()
    {
        var manifest = CreateManifest();
        var desc = NativeMeshDescriptor.FromManifest(manifest, edgeCount: 50, faceCount: 30);
        var roundTripped = desc.ToManifestSnapshot(
            manifest.LieAlgebraId,
            manifest.ComponentOrderId,
            manifest.TorsionBranchId,
            manifest.ShiabBranchId);

        Assert.Equal(manifest.BaseDimension, roundTripped.BaseDimension);
        Assert.Equal(manifest.AmbientDimension, roundTripped.AmbientDimension);
        Assert.Equal(manifest.LieAlgebraDimension, roundTripped.LieAlgebraDimension);
        Assert.Equal(manifest.MeshCellCount, roundTripped.MeshCellCount);
        Assert.Equal(manifest.MeshVertexCount, roundTripped.MeshVertexCount);
    }

    // ---------------------------------------------------------------
    // Parity detection: verify that ParityChecker detects injected errors
    // ---------------------------------------------------------------

    [Fact]
    public void Parity_DetectsInjectedError()
    {
        var reference = new double[] { 1.0, 2.0, 3.0, 4.0 };
        var perturbed = new double[] { 1.0, 2.0, 3.01, 4.0 }; // 1% error on element 2

        var record = ParityChecker.CompareResults(
            "perturbed-test", reference, perturbed,
            "cpu", "gpu", 1e-4); // tight tolerance

        Assert.False(record.Passed);
        Assert.True(record.MaxRelativeError > 0.003); // ~0.33% relative error
    }

    [Fact]
    public void Parity_AcceptsWithinTolerance()
    {
        var reference = new double[] { 1.0, 2.0, 3.0 };
        // Machine-epsilon level perturbation
        var perturbed = new double[] { 1.0 + 1e-15, 2.0, 3.0 - 1e-15 };

        var record = ParityChecker.CompareResults(
            "epsilon-test", reference, perturbed,
            "cpu", "gpu", 1e-12);

        Assert.True(record.Passed);
    }

    // ---------------------------------------------------------------
    // Helper methods
    // ---------------------------------------------------------------

    private static double[] GenerateTestData(int n, int seed)
    {
        var rng = new Random(seed);
        var data = new double[n];
        for (int i = 0; i < n; i++)
            data[i] = rng.NextDouble() * 10.0 - 5.0; // Range [-5, 5]
        return data;
    }

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
}

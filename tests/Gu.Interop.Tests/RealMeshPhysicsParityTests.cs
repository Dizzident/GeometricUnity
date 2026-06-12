using Gu.Core;
using Gu.Geometry;
using Gu.Interop;
using Gu.Math;

namespace Gu.Interop.Tests;

/// <summary>
/// Real-mesh physics parity tests (Phase405 platform follow-up).
///
/// The original CUDA parity suite exercised synthetic buffers only, which let
/// two coupled defects go undetected until a study ran real physics on a real
/// mesh:
///
/// 1. GpuSolverBackend.Initialize re-initialized an already-prepared native
///    backend; gu_initialize performs a full shutdown first, silently
///    discarding the uploaded mesh topology / structure constants /
///    background connection.
/// 2. Without physics data, the native physics kernels silently degrade to
///    identity-stub fallbacks (F = omega, T = 0, Shiab = copy), so the wipe
///    produced deterministic wrong values instead of an error.
///
/// These tests run the exact prepare-then-wrap session pattern the studies
/// use, on real mesh topology from the geometry factory, against both the
/// managed CpuReferenceBackend (always) and the CUDA native backend (when
/// available).
/// </summary>
public class RealMeshPhysicsParityTests
{
    private const int DimG = 3; // su(2)

    // ---------------------------------------------------------------
    // Shared setup helpers
    // ---------------------------------------------------------------

    private static (SimplicialMesh Mesh, FiberBundleMesh Bundle) CreateRealMesh()
    {
        var bundle = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 4, cols: 4);
        return (bundle.AmbientMesh, bundle);
    }

    private static MeshTopologyData BuildTopology(SimplicialMesh mesh) =>
        MeshTopologyData.FromMesh(mesh, DimG);

    private static ManifestSnapshot CreateSnapshot(SimplicialMesh mesh, FiberBundleMesh bundle) => new()
    {
        BaseDimension = bundle.BaseMesh.EmbeddingDimension,
        AmbientDimension = mesh.EmbeddingDimension,
        LieAlgebraDimension = DimG,
        LieAlgebraId = "su2",
        MeshCellCount = mesh.FaceCount,
        MeshVertexCount = mesh.VertexCount,
        ComponentOrderId = "order-row-major",
        TorsionBranchId = "trivial",
        ShiabBranchId = "identity",
    };

    private static TensorSignature CreateSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "canonical",
        ComponentOrderId = "edge-major",
        MemoryLayout = "dense-row-major",
    };

    private static BranchManifest CreateBranchManifest(SimplicialMesh mesh, FiberBundleMesh bundle) => new()
    {
        BranchId = "real-mesh-parity-test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        LieAlgebraId = "su2",
        BaseDimension = bundle.BaseMesh.EmbeddingDimension,
        AmbientDimension = mesh.EmbeddingDimension,
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        PairingConventionId = "pairing-killing",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };

    /// <summary>
    /// An exact (closed) connection 1-form in a single Lie direction:
    /// omega_e = f(v1) - f(v0) for a random vertex potential f. Its discrete
    /// exterior derivative telescopes to zero on every face boundary, and the
    /// single Lie direction makes every bracket term vanish identically, so
    /// the exact curvature is zero.
    /// </summary>
    private static double[] CreateClosedSingleDirectionOmega(SimplicialMesh mesh, int direction, int seed)
    {
        var rng = new Random(seed);
        var potential = new double[mesh.VertexCount];
        for (int v = 0; v < mesh.VertexCount; v++)
            potential[v] = rng.NextDouble() * 2.0 - 1.0;

        var omega = new double[mesh.EdgeCount * DimG];
        for (int e = 0; e < mesh.EdgeCount; e++)
            omega[e * DimG + direction] =
                potential[mesh.Edges[e][1]] - potential[mesh.Edges[e][0]];
        return omega;
    }

    /// <summary>
    /// Reference curvature computed directly from the mesh arrays:
    /// F[face] = sum_i sign_i * omega[edge_i] + (1/2) sum_{i&lt;j} [s_i w_i, s_j w_j].
    /// </summary>
    private static double[] ComputeExpectedCurvature(SimplicialMesh mesh, LieAlgebra algebra, double[] omega)
    {
        var expected = new double[mesh.FaceCount * DimG];
        var wi = new double[DimG];
        var wj = new double[DimG];
        for (int fi = 0; fi < mesh.FaceCount; fi++)
        {
            var edges = mesh.FaceBoundaryEdges[fi];
            var orients = mesh.FaceBoundaryOrientations[fi];
            for (int i = 0; i < edges.Length; i++)
                for (int a = 0; a < DimG; a++)
                    expected[fi * DimG + a] += orients[i] * omega[edges[i] * DimG + a];

            for (int i = 0; i < edges.Length; i++)
                for (int j = i + 1; j < edges.Length; j++)
                {
                    for (int a = 0; a < DimG; a++)
                    {
                        wi[a] = orients[i] * omega[edges[i] * DimG + a];
                        wj[a] = orients[j] * omega[edges[j] * DimG + a];
                    }
                    var bracket = algebra.Bracket(wi, wj);
                    for (int a = 0; a < DimG; a++)
                        expected[fi * DimG + a] += 0.5 * bracket[a];
                }
        }
        return expected;
    }

    /// <summary>
    /// The exact session pattern the studies use: initialize the native
    /// backend, upload physics data, then wrap it in a GpuSolverBackend and
    /// initialize the wrapper.
    /// </summary>
    private static GpuSolverBackend PrepareSession(
        INativeBackend native, SimplicialMesh mesh, FiberBundleMesh bundle, LieAlgebra algebra)
    {
        var snapshot = CreateSnapshot(mesh, bundle);
        native.Initialize(snapshot);
        native.UploadMeshTopology(BuildTopology(mesh));
        native.UploadAlgebraData(AlgebraUploadData.FromLieAlgebra(algebra));
        native.UploadBackgroundConnection(new double[mesh.EdgeCount * DimG], mesh.EdgeCount, DimG);

        var solver = new GpuSolverBackend(native, ownsBackend: false);
        solver.Initialize(snapshot);
        return solver;
    }

    private static DerivedState EvaluateDerived(
        GpuSolverBackend solver, SimplicialMesh mesh, FiberBundleMesh bundle, double[] omega)
    {
        var signature = CreateSignature();
        var omegaTensor = new FieldTensor
        {
            Label = "omega_h",
            Signature = signature,
            Coefficients = omega,
            Shape = new[] { mesh.EdgeCount, DimG },
        };
        var a0Tensor = new FieldTensor
        {
            Label = "a0_h",
            Signature = signature,
            Coefficients = new double[mesh.EdgeCount * DimG],
            Shape = new[] { mesh.EdgeCount, DimG },
        };
        return solver.EvaluateDerived(
            omegaTensor, a0Tensor,
            CreateBranchManifest(mesh, bundle),
            bundle.ToGeometryContext("centroid", "P1"));
    }

    // ---------------------------------------------------------------
    // Lifecycle regression (managed backend; runs everywhere)
    // ---------------------------------------------------------------

    [Fact]
    public void GpuSolverBackend_Initialize_PreservesUploadedPhysicsData()
    {
        var (mesh, bundle) = CreateRealMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        using var native = new CpuReferenceBackend();
        using var solver = PrepareSession(native, mesh, bundle, algebra);

        Assert.True(native.HasPhysicsData,
            "GpuSolverBackend.Initialize must not discard physics data already uploaded to the native backend.");
    }

    [Fact]
    public void GpuSolverBackend_RealMesh_ClosedProfileCurvatureIsZero()
    {
        var (mesh, bundle) = CreateRealMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        using var native = new CpuReferenceBackend();
        using var solver = PrepareSession(native, mesh, bundle, algebra);

        var omega = CreateClosedSingleDirectionOmega(mesh, direction: 1, seed: 42);
        var derived = EvaluateDerived(solver, mesh, bundle, omega);

        // Real physics: curvature of a closed single-direction form is zero.
        // The identity-stub fallback would return omega itself (norm > 1).
        double maxAbs = derived.CurvatureF.Coefficients.Max(System.Math.Abs);
        Assert.True(maxAbs < 1e-12,
            $"Closed-profile curvature must vanish; max |F| = {maxAbs:E3}. " +
            "A non-zero value matching omega indicates the identity-stub fallback ran instead of the physics kernel.");
    }

    [Fact]
    public void GpuSolverBackend_RealMesh_CurvatureMatchesDirectAssembly()
    {
        var (mesh, bundle) = CreateRealMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        using var native = new CpuReferenceBackend();
        using var solver = PrepareSession(native, mesh, bundle, algebra);

        // Random multi-direction omega: exercises both the linear d(omega)
        // term and the structure-constant bracket term.
        var rng = new Random(7);
        var omega = new double[mesh.EdgeCount * DimG];
        for (int k = 0; k < omega.Length; k++)
            omega[k] = rng.NextDouble() * 2.0 - 1.0;

        var derived = EvaluateDerived(solver, mesh, bundle, omega);
        var expected = ComputeExpectedCurvature(mesh, algebra, omega);

        double maxDiff = 0;
        for (int k = 0; k < expected.Length; k++)
            maxDiff = System.Math.Max(maxDiff,
                System.Math.Abs(derived.CurvatureF.Coefficients[k] - expected[k]));
        Assert.True(maxDiff < 1e-12,
            $"Real-mesh curvature must match direct assembly; max diff = {maxDiff:E3}");
    }

    // ---------------------------------------------------------------
    // Internal helpers shared with the CUDA collection class below
    // ---------------------------------------------------------------

    internal static (SimplicialMesh Mesh, FiberBundleMesh Bundle) CreateRealMeshShared() => CreateRealMesh();

    internal static GpuSolverBackend PrepareSessionShared(
        INativeBackend native, SimplicialMesh mesh, FiberBundleMesh bundle, LieAlgebra algebra) =>
        PrepareSession(native, mesh, bundle, algebra);

    internal static DerivedState EvaluateDerivedShared(
        GpuSolverBackend solver, SimplicialMesh mesh, FiberBundleMesh bundle, double[] omega) =>
        EvaluateDerived(solver, mesh, bundle, omega);

    internal static double[] CreateClosedSingleDirectionOmegaShared(SimplicialMesh mesh, int direction, int seed) =>
        CreateClosedSingleDirectionOmega(mesh, direction, seed);

    internal static double[] ComputeExpectedCurvatureShared(SimplicialMesh mesh, LieAlgebra algebra, double[] omega) =>
        ComputeExpectedCurvature(mesh, algebra, omega);
}

/// <summary>
/// CUDA real-mesh parity tests. These join the serialized "GPU" collection
/// because the native library holds a single global session; each test
/// rebuilds its own session and the class restores the shared fixture
/// session afterward so later collection members see their expected state.
/// </summary>
[Collection("GPU")]
[Trait("Category", "GPU")]
public sealed class RealMeshCudaPhysicsParityTests : IDisposable
{
    private const int DimG = 3;

    private readonly CudaTestFixture _fixture;

    public RealMeshCudaPhysicsParityTests(CudaTestFixture fixture)
    {
        _fixture = fixture;
    }

    public void Dispose()
    {
        // Restore the shared global native session for subsequent GPU tests.
        if (!CudaAvailability.IsAvailable)
            return;
        _fixture.GpuBackend.Initialize(_fixture.Manifest);
        _fixture.GpuBackend.UploadMeshTopology(_fixture.Topology);
        _fixture.GpuBackend.UploadAlgebraData(_fixture.AlgebraData);
        _fixture.GpuBackend.UploadBackgroundConnection(
            new double[_fixture.EdgeCount * _fixture.DimG], _fixture.EdgeCount, _fixture.DimG);
    }

    [SkipIfNoCuda]
    public void Cuda_RealMesh_ClosedProfileCurvatureIsZero()
    {
        var (mesh, bundle) = RealMeshPhysicsParityTests.CreateRealMeshShared();
        var algebra = LieAlgebraFactory.CreateSu2();

        var native = new CudaNativeBackend();
        using var solver = RealMeshPhysicsParityTests.PrepareSessionShared(native, mesh, bundle, algebra);

        var omega = RealMeshPhysicsParityTests.CreateClosedSingleDirectionOmegaShared(mesh, direction: 1, seed: 42);
        var derived = RealMeshPhysicsParityTests.EvaluateDerivedShared(solver, mesh, bundle, omega);

        double maxAbs = derived.CurvatureF.Coefficients.Max(System.Math.Abs);
        Assert.True(maxAbs < 1e-12,
            $"CUDA closed-profile curvature must vanish; max |F| = {maxAbs:E3}");
    }

    [SkipIfNoCuda]
    public void Cuda_RealMesh_CurvatureMatchesDirectAssembly()
    {
        var (mesh, bundle) = RealMeshPhysicsParityTests.CreateRealMeshShared();
        var algebra = LieAlgebraFactory.CreateSu2();

        var native = new CudaNativeBackend();
        using var solver = RealMeshPhysicsParityTests.PrepareSessionShared(native, mesh, bundle, algebra);

        var rng = new Random(7);
        var omega = new double[mesh.EdgeCount * DimG];
        for (int k = 0; k < omega.Length; k++)
            omega[k] = rng.NextDouble() * 2.0 - 1.0;

        var derived = RealMeshPhysicsParityTests.EvaluateDerivedShared(solver, mesh, bundle, omega);
        var expected = RealMeshPhysicsParityTests.ComputeExpectedCurvatureShared(mesh, algebra, omega);

        double maxDiff = 0;
        for (int k = 0; k < expected.Length; k++)
            maxDiff = System.Math.Max(maxDiff,
                System.Math.Abs(derived.CurvatureF.Coefficients[k] - expected[k]));
        Assert.True(maxDiff < 1e-12,
            $"CUDA real-mesh curvature must match direct assembly; max diff = {maxDiff:E3}");
    }

    [SkipIfNoCuda]
    public void Cuda_RealMesh_DerivedStateParity_WithManagedReference()
    {
        var (mesh, bundle) = RealMeshPhysicsParityTests.CreateRealMeshShared();
        var algebra = LieAlgebraFactory.CreateSu2();

        // Managed reference first: it holds instance state, so the CUDA
        // session created afterwards cannot disturb it.
        using var cpuNative = new CpuReferenceBackend();
        using var cpuSolver = RealMeshPhysicsParityTests.PrepareSessionShared(cpuNative, mesh, bundle, algebra);

        var rng = new Random(11);
        var omega = new double[mesh.EdgeCount * DimG];
        for (int k = 0; k < omega.Length; k++)
            omega[k] = rng.NextDouble() * 2.0 - 1.0;

        var cpuDerived = RealMeshPhysicsParityTests.EvaluateDerivedShared(cpuSolver, mesh, bundle, omega);

        var cudaNative = new CudaNativeBackend();
        using var cudaSolver = RealMeshPhysicsParityTests.PrepareSessionShared(cudaNative, mesh, bundle, algebra);
        var cudaDerived = RealMeshPhysicsParityTests.EvaluateDerivedShared(cudaSolver, mesh, bundle, omega);

        AssertParity("CurvatureF", cpuDerived.CurvatureF.Coefficients, cudaDerived.CurvatureF.Coefficients);
        AssertParity("TorsionT", cpuDerived.TorsionT.Coefficients, cudaDerived.TorsionT.Coefficients);
        AssertParity("ShiabS", cpuDerived.ShiabS.Coefficients, cudaDerived.ShiabS.Coefficients);
        AssertParity("ResidualUpsilon", cpuDerived.ResidualUpsilon.Coefficients, cudaDerived.ResidualUpsilon.Coefficients);
    }

    private static void AssertParity(string name, double[] reference, double[] actual)
    {
        Assert.Equal(reference.Length, actual.Length);
        double maxDiff = 0;
        for (int k = 0; k < reference.Length; k++)
            maxDiff = System.Math.Max(maxDiff, System.Math.Abs(reference[k] - actual[k]));
        Assert.True(maxDiff < 1e-12,
            $"{name} real-mesh CPU/CUDA parity failed; max diff = {maxDiff:E3}");
    }
}

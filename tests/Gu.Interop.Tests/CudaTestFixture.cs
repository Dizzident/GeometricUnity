using Gu.Geometry;
using Gu.Interop;
using global::Gu.Math;

namespace Gu.Interop.Tests;

/// <summary>
/// Detects whether the CUDA native library (libgu_cuda_core.so) is available
/// on the current machine. Used by test attributes to skip GPU tests gracefully.
/// </summary>
public static class CudaAvailability
{
    public static bool IsAvailable { get; }
    public static string SkipReason { get; }

    static CudaAvailability()
    {
        try
        {
            using var backend = new CudaNativeBackend();
            backend.Initialize(new ManifestSnapshot
            {
                BaseDimension = 4,
                AmbientDimension = 14,
                LieAlgebraDimension = 3,
                LieAlgebraId = "su2",
                MeshCellCount = 1,
                MeshVertexCount = 1,
                ComponentOrderId = "order-row-major",
                TorsionBranchId = "augmented-torsion",
                ShiabBranchId = "identity",
            });
            IsAvailable = true;
            SkipReason = string.Empty;
        }
        catch (DllNotFoundException ex)
        {
            IsAvailable = false;
            SkipReason = $"CUDA native library not found: {ex.Message}";
        }
        catch (EntryPointNotFoundException ex)
        {
            IsAvailable = false;
            SkipReason = $"CUDA entry point not found: {ex.Message}";
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            SkipReason = $"CUDA initialization failed: {ex.Message}";
        }
    }
}

/// <summary>
/// Shared test fixture that initializes both CUDA and CPU reference backends
/// with real physics data from the toy 2D geometry and su(2) Lie algebra.
/// Provides helper methods for generating test data and creating buffer layouts.
/// </summary>
[Trait("Category", "GPU")]
public sealed class CudaTestFixture : IDisposable
{
    public CudaNativeBackend GpuBackend { get; }
    public CpuReferenceBackend CpuBackend { get; }
    public ManifestSnapshot Manifest { get; }
    public MeshTopologyData Topology { get; }
    public AlgebraUploadData AlgebraData { get; }
    public int EdgeCount { get; }
    public int FaceCount { get; }
    public int DimG { get; }

    public CudaTestFixture()
    {
        // Build geometry: toy 2D fiber bundle mesh
        var bundle = ToyGeometryFactory.CreateToy2D();
        var baseMesh = bundle.BaseMesh;

        // Build algebra: su(2) with positive-definite trace pairing
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        DimG = algebra.Dimension; // 3
        EdgeCount = baseMesh.EdgeCount; // 8
        FaceCount = baseMesh.FaceCount; // 4

        // Pack mesh topology and algebra for upload
        Topology = MeshTopologyData.FromMesh(baseMesh, dimG: DimG);
        AlgebraData = AlgebraUploadData.FromLieAlgebra(algebra);

        // Create manifest snapshot
        Manifest = new ManifestSnapshot
        {
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraDimension = DimG,
            LieAlgebraId = "su2",
            MeshCellCount = baseMesh.FaceCount,
            MeshVertexCount = baseMesh.VertexCount,
            ComponentOrderId = "order-row-major",
            TorsionBranchId = "augmented-torsion",
            ShiabBranchId = "identity",
        };

        // Initialize GPU backend
        GpuBackend = new CudaNativeBackend();
        GpuBackend.Initialize(Manifest);
        GpuBackend.UploadMeshTopology(Topology);
        GpuBackend.UploadAlgebraData(AlgebraData);
        GpuBackend.UploadBackgroundConnection(
            new double[EdgeCount * DimG], EdgeCount, DimG);

        // Initialize CPU backend (symmetric setup for parity comparison)
        CpuBackend = new CpuReferenceBackend();
        CpuBackend.Initialize(Manifest);
        CpuBackend.UploadMeshTopology(Topology);
        CpuBackend.UploadAlgebraData(AlgebraData);
        CpuBackend.UploadBackgroundConnection(
            new double[EdgeCount * DimG], EdgeCount, DimG);

        Assert.True(GpuBackend.HasPhysicsData,
            "GPU backend should report HasPhysicsData after uploading mesh, algebra, and A0.");
    }

    /// <summary>
    /// Generates a reproducible random omega vector of size EdgeCount * DimG
    /// with values uniformly distributed in [-1, 1].
    /// </summary>
    public double[] GenerateOmega(int seed)
    {
        var rng = new Random(seed);
        int n = EdgeCount * DimG;
        var omega = new double[n];
        for (int i = 0; i < n; i++)
        {
            omega[i] = rng.NextDouble() * 2.0 - 1.0;
        }
        return omega;
    }

    /// <summary>
    /// Creates a SoA buffer layout for edge-valued Lie-algebra-valued data
    /// (EdgeCount * DimG scalar elements).
    /// </summary>
    public BufferLayoutDescriptor CreateEdgeLayout()
    {
        var componentNames = new string[DimG];
        for (int g = 0; g < DimG; g++)
        {
            componentNames[g] = $"edge_g{g}";
        }
        return BufferLayoutDescriptor.CreateSoA("edge-field", componentNames, EdgeCount);
    }

    /// <summary>
    /// Creates a SoA buffer layout for face-valued Lie-algebra-valued data
    /// (FaceCount * DimG scalar elements).
    /// </summary>
    public BufferLayoutDescriptor CreateFaceLayout()
    {
        var componentNames = new string[DimG];
        for (int g = 0; g < DimG; g++)
        {
            componentNames[g] = $"face_g{g}";
        }
        return BufferLayoutDescriptor.CreateSoA("face-field", componentNames, FaceCount);
    }

    public void Dispose()
    {
        GpuBackend.Dispose();
        CpuBackend.Dispose();
    }
}

/// <summary>
/// xUnit collection definition that ensures all GPU test classes share a single
/// CudaTestFixture instance. This is required because the native library uses
/// global state -- multiple gu_initialize calls would reset each other's buffers.
/// </summary>
[CollectionDefinition("GPU")]
public class GpuTestCollection : ICollectionFixture<CudaTestFixture> { }

/// <summary>
/// Fact attribute that automatically skips the test when no CUDA device is available.
/// </summary>
public sealed class SkipIfNoCuda : FactAttribute
{
    public SkipIfNoCuda()
    {
        if (!CudaAvailability.IsAvailable)
        {
            Skip = CudaAvailability.SkipReason;
        }
    }
}

/// <summary>
/// Theory attribute that automatically skips the test when no CUDA device is available.
/// </summary>
public sealed class SkipIfNoCudaTheory : TheoryAttribute
{
    public SkipIfNoCudaTheory()
    {
        if (!CudaAvailability.IsAvailable)
        {
            Skip = CudaAvailability.SkipReason;
        }
    }
}

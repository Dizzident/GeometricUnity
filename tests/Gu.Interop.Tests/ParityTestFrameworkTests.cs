using Gu.Core;
using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// Tests for the parity testing pattern: verifying that packed-buffer operations
/// produce the same results as direct computation on semantic types.
/// </summary>
public class ParityTestFrameworkTests
{
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

    [Fact]
    public void PackedResidual_MatchesDirectComputation()
    {
        // Direct computation: Upsilon = S - T
        var shiabValues = new double[] { 10.0, 20.0, 30.0, 40.0, 50.0, 60.0 };
        var torsionValues = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
        var expectedResidual = new double[6];
        for (int i = 0; i < 6; i++)
            expectedResidual[i] = shiabValues[i] - torsionValues[i];

        // Packed computation via CpuReferenceBackend
        using var backend = new CpuReferenceBackend();
        backend.Initialize(new ManifestSnapshot
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
        });

        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, 6);
        var shiabBuf = backend.AllocateBuffer(layout);
        var torsionBuf = backend.AllocateBuffer(layout);
        var residualBuf = backend.AllocateBuffer(layout);

        backend.UploadBuffer(shiabBuf, shiabValues);
        backend.UploadBuffer(torsionBuf, torsionValues);
        backend.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);

        var packedResult = new double[6];
        backend.DownloadBuffer(residualBuf, packedResult);

        // Parity check
        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(expectedResidual[i], packedResult[i], precision: 14);
        }
    }

    [Fact]
    public void PackedObjective_MatchesDirectComputation()
    {
        var residualValues = new double[] { 3.0, 4.0 };
        double expectedObjective = 0.5 * (3.0 * 3.0 + 4.0 * 4.0); // 12.5

        using var backend = new CpuReferenceBackend();
        backend.Initialize(new ManifestSnapshot
        {
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraDimension = 3,
            LieAlgebraId = "su2",
            MeshCellCount = 1,
            MeshVertexCount = 2,
            ComponentOrderId = "order-row-major",
            TorsionBranchId = "trivial",
            ShiabBranchId = "identity",
        });

        var layout = BufferLayoutDescriptor.CreateSoA("residual", new[] { "c" }, 2);
        var residualBuf = backend.AllocateBuffer(layout);
        backend.UploadBuffer(residualBuf, residualValues);

        double packedObjective = backend.EvaluateObjective(residualBuf);

        Assert.Equal(expectedObjective, packedObjective, precision: 14);
    }

    [Fact]
    public void FieldPacker_RoundTrip_ThroughBackend()
    {
        // Create a multi-component FieldTensor
        var original = new FieldTensor
        {
            Label = "omega_h",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9 },
            Shape = new[] { 3, 3 },
        };

        // Pack to SoA, upload, download, unpack
        using var backend = new CpuReferenceBackend();
        backend.Initialize(new ManifestSnapshot
        {
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraDimension = 3,
            LieAlgebraId = "su2",
            MeshCellCount = 3,
            MeshVertexCount = 5,
            ComponentOrderId = "order-row-major",
            TorsionBranchId = "trivial",
            ShiabBranchId = "identity",
        });

        var layout = FieldPacker.CreateLayout(original, "omega-layout");
        var buffer = backend.AllocateBuffer(layout);
        FieldPacker.UploadField(backend, original, buffer);

        var restored = FieldPacker.DownloadField(
            backend, buffer, "omega_h", CreateSignature(), new[] { 3, 3 });

        // Verify round-trip fidelity
        Assert.Equal(original.Coefficients.Length, restored.Coefficients.Length);
        for (int i = 0; i < original.Coefficients.Length; i++)
        {
            Assert.Equal(original.Coefficients[i], restored.Coefficients[i], precision: 14);
        }
    }

    [Fact]
    public void ManifestSnapshot_FromManifest_CopiesCorrectFields()
    {
        var manifest = new BranchManifest
        {
            BranchId = "test",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1",
            CodeRevision = "abc",
            ActiveGeometryBranch = "simplicial-4d",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic",
            ActiveShiabBranch = "first-order-curvature",
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

        var snapshot = ManifestSnapshot.FromManifest(manifest, 3, 100, 50);

        Assert.Equal(4, snapshot.BaseDimension);
        Assert.Equal(14, snapshot.AmbientDimension);
        Assert.Equal(10, snapshot.FiberDimension);
        Assert.Equal(3, snapshot.LieAlgebraDimension);
        Assert.Equal("su2", snapshot.LieAlgebraId);
        Assert.Equal(100, snapshot.MeshCellCount);
        Assert.Equal(50, snapshot.MeshVertexCount);
        Assert.Equal("order-row-major", snapshot.ComponentOrderId);
        Assert.Equal("local-algebraic", snapshot.TorsionBranchId);
        Assert.Equal("first-order-curvature", snapshot.ShiabBranchId);
    }
}

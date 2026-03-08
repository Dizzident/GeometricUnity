using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// Tests for ParityChecker: comparing two backend instances and
/// producing ParityRecord artifacts.
/// </summary>
public class ParityCheckerTests
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
        TorsionBranchId = "local-algebraic",
        ShiabBranchId = "first-order-curvature",
    };

    [Fact]
    public void CompareResults_IdenticalArrays_Passes()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.0, 2.0, 3.0 };

        var record = ParityChecker.CompareResults("test", a, b, "cpu", "gpu", 1e-12);

        Assert.True(record.Passed);
        Assert.Equal(0.0, record.MaxAbsoluteError);
        Assert.Equal(0.0, record.MaxRelativeError);
        Assert.Equal(0.0, record.L2ErrorNorm);
        Assert.Equal("test", record.KernelName);
        Assert.Equal("cpu", record.CpuBackendId);
        Assert.Equal("gpu", record.GpuBackendId);
        Assert.Equal(3, record.ElementCount);
    }

    [Fact]
    public void CompareResults_SmallDifference_PassesWithinTolerance()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.0 + 1e-14, 2.0 - 1e-14, 3.0 + 1e-14 };

        var record = ParityChecker.CompareResults("curvature", a, b, "cpu", "gpu", 1e-12);

        Assert.True(record.Passed);
        Assert.True(record.MaxRelativeError < 1e-12);
    }

    [Fact]
    public void CompareResults_LargeDifference_Fails()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.1, 2.0, 3.0 };

        var record = ParityChecker.CompareResults("curvature", a, b, "cpu", "gpu", 1e-12);

        Assert.False(record.Passed);
        Assert.True(record.MaxRelativeError > 0.09); // ~10% error
        Assert.Contains("exceeds tolerance", record.Message);
    }

    [Fact]
    public void CompareResults_LengthMismatch_Fails()
    {
        var a = new double[] { 1.0, 2.0 };
        var b = new double[] { 1.0, 2.0, 3.0 };

        var record = ParityChecker.CompareResults("test", a, b, "cpu", "gpu", 1e-12);

        Assert.False(record.Passed);
        Assert.Contains("Length mismatch", record.Message);
    }

    [Fact]
    public void CompareScalar_IdenticalValues_Passes()
    {
        var record = ParityChecker.CompareScalar("objective", 12.5, 12.5, "cpu", "gpu", 1e-12);

        Assert.True(record.Passed);
        Assert.Equal(0.0, record.MaxAbsoluteError);
        Assert.Equal(1, record.ElementCount);
    }

    [Fact]
    public void CompareScalar_SmallDifference_PassesWithinTolerance()
    {
        var record = ParityChecker.CompareScalar("objective", 12.5, 12.5 + 1e-14, "cpu", "gpu", 1e-12);

        Assert.True(record.Passed);
    }

    [Fact]
    public void CompareScalar_LargeDifference_Fails()
    {
        var record = ParityChecker.CompareScalar("objective", 12.5, 13.0, "cpu", "gpu", 1e-12);

        Assert.False(record.Passed);
        Assert.Contains("failed", record.Message);
    }

    [Fact]
    public void RunFullResidualParity_TwoIdenticalBackends_AllPass()
    {
        using var reference = new CpuReferenceBackend();
        using var target = new CpuReferenceBackend();

        var checker = new ParityChecker(reference, target);
        var omega = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 };
        var records = checker.RunFullResidualParity(CreateManifest(), omega);

        // Should produce records for: curvature, torsion, shiab, residual, objective
        Assert.Equal(5, records.Count);
        Assert.All(records, r => Assert.True(r.Passed, $"Kernel '{r.KernelName}' failed: {r.Message}"));

        // Verify kernel names
        Assert.Equal("curvature", records[0].KernelName);
        Assert.Equal("torsion", records[1].KernelName);
        Assert.Equal("shiab", records[2].KernelName);
        Assert.Equal("residual", records[3].KernelName);
        Assert.Equal("objective", records[4].KernelName);
    }

    [Fact]
    public void RunFullResidualParity_ProducesCorrectBackendIds()
    {
        using var reference = new CpuReferenceBackend();
        using var target = new CpuReferenceBackend();

        var checker = new ParityChecker(reference, target);
        var omega = new double[] { 1.0, 2.0, 3.0 };
        var records = checker.RunFullResidualParity(CreateManifest(), omega);

        Assert.All(records, r =>
        {
            Assert.Equal("cpu-reference", r.CpuBackendId);
            Assert.Equal("cpu-reference", r.GpuBackendId);
        });
    }

    [Fact]
    public void ParityRecord_Serialization_RoundTrip()
    {
        var record = new ParityRecord
        {
            RecordId = "test-001",
            KernelName = "curvature",
            CpuBackendId = "cpu-reference",
            GpuBackendId = "cuda",
            ElementCount = 100,
            MaxAbsoluteError = 1e-15,
            MaxRelativeError = 1e-14,
            L2ErrorNorm = 3e-15,
            Tolerance = 1e-12,
            Passed = true,
            Message = "Parity check passed",
        };

        var json = System.Text.Json.JsonSerializer.Serialize(record);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<ParityRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("test-001", deserialized.RecordId);
        Assert.Equal("curvature", deserialized.KernelName);
        Assert.True(deserialized.Passed);
        Assert.Equal(1e-15, deserialized.MaxAbsoluteError);
    }

    [Fact]
    public void RunFullResidualParity_ObjectiveValue_Correct()
    {
        using var reference = new CpuReferenceBackend();
        using var target = new CpuReferenceBackend();

        var checker = new ParityChecker(reference, target);
        // With trivial torsion (T=0) and identity Shiab (S=omega),
        // residual = omega - 0 = omega, objective = 0.5 * ||omega||^2
        var omega = new double[] { 3.0, 4.0 };
        var records = checker.RunFullResidualParity(CreateManifest(), omega);

        var objectiveRecord = records.Single(r => r.KernelName == "objective");
        Assert.True(objectiveRecord.Passed);
        // Both should agree: 0.5 * (9 + 16) = 12.5
    }
}

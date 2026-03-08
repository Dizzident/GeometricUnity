using Gu.Core;
using Gu.Core.Factories;

namespace Gu.Artifacts.Tests;

/// <summary>
/// Shared helpers for creating test data.
/// </summary>
internal static class TestHelpers
{
    internal static BranchManifest CreateTestManifest() => new()
    {
        BranchId = "test-branch",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "abc123",
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
        InsertedAssumptionIds = new[] { "IA-1", "IA-2", "IA-3" },
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    internal static BranchRef CreateTestBranchRef() => new()
    {
        BranchId = "test-branch",
        SchemaVersion = "1.0.0",
    };

    internal static ProvenanceMeta CreateTestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = CreateTestBranchRef(),
        Backend = "cpu-reference",
    };

    internal static ReplayContract CreateTestReplayContract(
        string tier = "R2",
        bool deterministic = true,
        string backendId = "cpu-reference") => new()
    {
        BranchManifest = CreateTestManifest(),
        Deterministic = deterministic,
        RandomSeed = 42,
        BackendId = backendId,
        ReplayTier = tier,
    };

    internal static ValidationRecord CreateTestRecord(string ruleId, bool passed) => new()
    {
        RuleId = ruleId,
        Category = "test",
        Passed = passed,
        MeasuredValue = passed ? 0.0 : 1.0,
        Tolerance = 0.5,
        Detail = passed ? "passed" : "failed",
        Timestamp = DateTimeOffset.UtcNow,
    };

    internal static ValidationBundle CreateTestValidationBundle(bool allPassed = true) => new()
    {
        Branch = CreateTestBranchRef(),
        Records = new[]
        {
            CreateTestRecord("rule-1", true),
            CreateTestRecord("rule-2", allPassed),
        },
        AllPassed = allPassed,
    };

    internal static IntegrityBundle CreateTestIntegrityBundle() => new()
    {
        ContentHash = "abc123def456",
        HashAlgorithm = "SHA-256",
        ComputedAt = DateTimeOffset.UtcNow,
    };

    internal static ResidualBundle CreateTestResidualBundle(double objectiveValue = 1.5e-6) => new()
    {
        Components = Array.Empty<ResidualComponent>(),
        ObjectiveValue = objectiveValue,
        TotalNorm = System.Math.Sqrt(objectiveValue),
    };

    internal static ArtifactBundle CreateTestArtifactBundle() => new()
    {
        ArtifactId = "artifact-001",
        Branch = CreateTestBranchRef(),
        ReplayContract = CreateTestReplayContract(),
        ValidationBundle = CreateTestValidationBundle(),
        Integrity = CreateTestIntegrityBundle(),
        Provenance = CreateTestProvenance(),
        CreatedAt = DateTimeOffset.UtcNow,
    };

    /// <summary>
    /// Create a temp directory for a test run folder. Caller should delete after test.
    /// </summary>
    internal static string CreateTempRunFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "gu-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }
}

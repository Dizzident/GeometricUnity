using Gu.Core;
using Gu.Core.Factories;
using Gu.Math;

namespace Gu.Validation.Tests;

/// <summary>
/// Shared helpers for creating test data in validation tests.
/// </summary>
internal static class TestHelpers
{
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

    /// <summary>
    /// Create a valid BranchManifest with all fields set (no "unset" values).
    /// </summary>
    internal static BranchManifest CreateValidManifest() => new()
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
        InsertedAssumptionIds = new[] { "IA-1", "IA-2" },
        InsertedChoiceIds = new[] { "IX-1" },
    };

    /// <summary>
    /// Create a BranchManifest with some "unset" fields to test manifest-complete check failure.
    /// </summary>
    internal static BranchManifest CreateIncompleteManifest() => BranchManifestFactory.CreateEmpty("incomplete-branch");

    /// <summary>
    /// Create a valid SU(2) Lie algebra for testing algebraic identities.
    /// SU(2) structure constants: f^c_{ab} = epsilon_{abc} (Levi-Civita)
    /// </summary>
    internal static LieAlgebra CreateSu2Algebra()
    {
        int dim = 3;
        var f = new double[dim * dim * dim];

        // epsilon_{123} = +1 and antisymmetric permutations
        // f^c_{ab} indexed as [a * dim^2 + b * dim + c]
        f[0 * 9 + 1 * 3 + 2] = 1.0;  // f^3_{12} = +1
        f[1 * 9 + 0 * 3 + 2] = -1.0; // f^3_{21} = -1
        f[1 * 9 + 2 * 3 + 0] = 1.0;  // f^1_{23} = +1
        f[2 * 9 + 1 * 3 + 0] = -1.0; // f^1_{32} = -1
        f[2 * 9 + 0 * 3 + 1] = 1.0;  // f^2_{31} = +1
        f[0 * 9 + 2 * 3 + 1] = -1.0; // f^2_{13} = -1

        var metric = new double[dim * dim];
        // Using Killing form for SU(2): g_{ab} = -2 delta_{ab}
        // Use simple diagonal metric for testing
        metric[0] = 1.0; // g_{11}
        metric[4] = 1.0; // g_{22}
        metric[8] = 1.0; // g_{33}

        return new LieAlgebra
        {
            AlgebraId = "su2",
            Dimension = dim,
            Label = "SU(2)",
            BasisLabels = new[] { "T1", "T2", "T3" },
            BasisOrderId = "canonical",
            StructureConstants = f,
            InvariantMetric = metric,
            PairingId = "killing",
        };
    }

    /// <summary>
    /// Create an invalid Lie algebra with broken antisymmetry for testing failures.
    /// </summary>
    internal static LieAlgebra CreateBrokenAlgebra()
    {
        int dim = 3;
        var f = new double[dim * dim * dim];

        // Deliberately break antisymmetry: set f^3_{12} = f^3_{21} = +1
        f[0 * 9 + 1 * 3 + 2] = 1.0;
        f[1 * 9 + 0 * 3 + 2] = 1.0; // Should be -1 for antisymmetry

        var metric = new double[dim * dim];
        // Make metric non-symmetric to test metric-symmetry failure
        metric[0] = 1.0;
        metric[1] = 0.5;
        metric[3] = -0.5; // Asymmetric: g_{12} != g_{21}
        metric[4] = 1.0;
        metric[8] = 1.0;

        return new LieAlgebra
        {
            AlgebraId = "broken",
            Dimension = dim,
            Label = "Broken",
            BasisLabels = new[] { "T1", "T2", "T3" },
            BasisOrderId = "canonical",
            StructureConstants = f,
            InvariantMetric = metric,
            PairingId = "custom",
        };
    }

    /// <summary>
    /// Create a TensorSignature for testing.
    /// </summary>
    internal static TensorSignature CreateTestSignature(string carrierType = "residual-2form") => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = carrierType,
        Degree = "2",
        LieAlgebraBasisId = "su2-canonical",
        ComponentOrderId = "row-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    /// <summary>
    /// Create a FieldTensor with specified shape and coefficients.
    /// </summary>
    internal static FieldTensor CreateTestField(
        string label,
        int[] shape,
        double[]? coefficients = null,
        string carrierType = "residual-2form")
    {
        int totalSize = 1;
        foreach (var s in shape)
            totalSize *= s;

        return new FieldTensor
        {
            Label = label,
            Signature = CreateTestSignature(carrierType),
            Shape = shape,
            Coefficients = coefficients ?? new double[totalSize],
        };
    }

    /// <summary>
    /// Create a valid DerivedState with matching carrier types and correct shapes.
    /// </summary>
    internal static DerivedState CreateValidDerivedState()
    {
        return new DerivedState
        {
            CurvatureF = CreateTestField("F_h", new[] { 3, 4 }, carrierType: "curvature-2form"),
            TorsionT = CreateTestField("T_h", new[] { 3, 4 }, carrierType: "residual-2form"),
            ShiabS = CreateTestField("S_h", new[] { 3, 4 }, carrierType: "residual-2form"),
            ResidualUpsilon = CreateTestField("Upsilon_h", new[] { 3, 4 }, carrierType: "residual-2form"),
        };
    }

    /// <summary>
    /// Create a DerivedState with mismatched carrier types (T_h and S_h differ).
    /// </summary>
    internal static DerivedState CreateMismatchedCarrierDerivedState()
    {
        return new DerivedState
        {
            CurvatureF = CreateTestField("F_h", new[] { 3, 4 }, carrierType: "curvature-2form"),
            TorsionT = CreateTestField("T_h", new[] { 3, 4 }, carrierType: "residual-2form"),
            ShiabS = CreateTestField("S_h", new[] { 3, 4 }, carrierType: "curvature-2form"), // mismatch
            ResidualUpsilon = CreateTestField("Upsilon_h", new[] { 3, 4 }, carrierType: "residual-2form"),
        };
    }

    /// <summary>
    /// Create a DerivedState with shape/coefficient count mismatch.
    /// </summary>
    internal static DerivedState CreateShapeMismatchDerivedState()
    {
        return new DerivedState
        {
            CurvatureF = new FieldTensor
            {
                Label = "F_h",
                Signature = CreateTestSignature("curvature-2form"),
                Shape = new[] { 3, 4 }, // expects 12 coefficients
                Coefficients = new double[10], // only 10 -- mismatch!
            },
            TorsionT = CreateTestField("T_h", new[] { 3, 4 }),
            ShiabS = CreateTestField("S_h", new[] { 3, 4 }),
            ResidualUpsilon = CreateTestField("Upsilon_h", new[] { 3, 4 }),
        };
    }

    /// <summary>
    /// Create a ReplayContract for testing.
    /// </summary>
    internal static ReplayContract CreateTestReplayContract(
        string tier = "R2",
        bool deterministic = true,
        string backendId = "cpu-reference") => new()
    {
        BranchManifest = CreateValidManifest(),
        Deterministic = deterministic,
        RandomSeed = 42,
        BackendId = backendId,
        ReplayTier = tier,
    };

    /// <summary>
    /// Create a test ValidationBundle.
    /// </summary>
    internal static ValidationBundle CreateTestValidationBundle(bool allPassed = true) => new()
    {
        Branch = CreateTestBranchRef(),
        Records = new[]
        {
            new ValidationRecord
            {
                RuleId = "rule-1",
                Category = "test",
                Passed = true,
                Timestamp = DateTimeOffset.UtcNow,
            },
            new ValidationRecord
            {
                RuleId = "rule-2",
                Category = "test",
                Passed = allPassed,
                Timestamp = DateTimeOffset.UtcNow,
            },
        },
        AllPassed = allPassed,
    };

    /// <summary>
    /// Create a test IntegrityBundle.
    /// </summary>
    internal static IntegrityBundle CreateTestIntegrityBundle() => new()
    {
        ContentHash = "abc123def456",
        HashAlgorithm = "SHA-256",
        ComputedAt = DateTimeOffset.UtcNow,
    };

    /// <summary>
    /// Create a test ObservedState.
    /// </summary>
    internal static ObservedState CreateTestObservedState(double[] values) => new()
    {
        ObservationBranchId = "sigma-pullback",
        Observables = new Dictionary<string, ObservableSnapshot>
        {
            ["energy"] = new ObservableSnapshot
            {
                ObservableId = "energy",
                OutputType = OutputType.Quantitative,
                Values = values,
            },
        },
        Provenance = CreateTestProvenance(),
    };

    /// <summary>
    /// Create a test ArtifactBundle.
    /// </summary>
    internal static ArtifactBundle CreateTestArtifactBundle(
        ObservedState? observed = null,
        ValidationBundle? validation = null) => new()
    {
        ArtifactId = "artifact-001",
        Branch = CreateTestBranchRef(),
        ReplayContract = CreateTestReplayContract(),
        ValidationBundle = validation ?? CreateTestValidationBundle(),
        ObservedState = observed,
        Integrity = CreateTestIntegrityBundle(),
        Provenance = CreateTestProvenance(),
        CreatedAt = DateTimeOffset.UtcNow,
    };

    /// <summary>
    /// Create a temp directory for a test run folder. Caller should delete after test.
    /// </summary>
    internal static string CreateTempRunFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "gu-val-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }
}

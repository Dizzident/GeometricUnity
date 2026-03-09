using Gu.Core;
using Gu.Phase2.Comparison;
using Gu.Phase2.Execution;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.Comparison.Tests;

public sealed class UncertaintyDecomposerTests
{
    private static UncertaintyRecord MakeEstimated() => new()
    {
        Discretization = 0.01,
        Solver = 0.02,
        Branch = 0.03,
        Extraction = 0.04,
        Calibration = 0.05,
        DataAsset = 0.06,
    };

    private static PredictionTestRecord MakeRecord(string numericalStatus = "converged") => new()
    {
        TestId = "test-001",
        ClaimClass = ClaimClass.ExactStructuralConsequence,
        FormalSource = "Theorem 3.1",
        BranchManifestId = "manifest-001",
        ObservableMapId = "obs-map-001",
        TheoremDependencyStatus = "closed",
        NumericalDependencyStatus = numericalStatus,
        ApproximationStatus = "exact",
        Falsifier = "Observable diverges by >5 sigma",
        ArtifactLinks = ["artifact-001"],
    };

    private static readonly BranchRef TestBranchRef = new()
    {
        BranchId = "branch-001",
        SchemaVersion = "1.0",
    };

    private static readonly BranchManifest TestManifest = new()
    {
        BranchId = "branch-001",
        SchemaVersion = "1.0",
        SourceEquationRevision = "v1",
        CodeRevision = "abc123",
        ActiveGeometryBranch = "flat-3d",
        ActiveObservationBranch = "default",
        ActiveTorsionBranch = "augmented",
        ActiveShiabBranch = "identity",
        ActiveGaugeStrategy = "coulomb",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "standard",
        ComponentOrderId = "lexicographic",
        AdjointConventionId = "standard",
        PairingConventionId = "trace",
        NormConventionId = "L2",
        DifferentialFormMetricId = "hodge-L2",
        InsertedAssumptionIds = [],
        InsertedChoiceIds = [],
    };

    private static BranchRunRecord MakeBranchRun(
        bool converged = true,
        double residualNorm = 1e-6,
        double finalObjective = 0.001) => new()
    {
        Variant = new BranchVariantManifest
        {
            Id = "variant-001",
            ParentFamilyId = "family-001",
            A0Variant = "flat",
            BiConnectionVariant = "symmetric",
            TorsionVariant = "augmented",
            ShiabVariant = "identity",
            ObservationVariant = "default",
            ExtractionVariant = "default",
            GaugeVariant = "coulomb",
            RegularityVariant = "smooth",
            PairingVariant = "trace",
            ExpectedClaimCeiling = "ExactStructuralConsequence",
        },
        Manifest = TestManifest,
        Converged = converged,
        TerminationReason = converged ? "converged" : "max_iterations",
        FinalObjective = finalObjective,
        FinalResidualNorm = residualNorm,
        Iterations = 100,
        SolveMode = SolveMode.ObjectiveMinimization,
        ArtifactBundle = new ArtifactBundle
        {
            ArtifactId = "artifact-001",
            Branch = TestBranchRef,
            ReplayContract = new ReplayContract
            {
                BranchManifest = TestManifest,
                Deterministic = true,
                BackendId = "cpu-reference",
                ReplayTier = "R2",
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc123",
                Branch = TestBranchRef,
            },
            CreatedAt = DateTimeOffset.UtcNow,
        },
    };

    [Fact]
    public void TotalUncertainty_AllEstimated_QuadratureSum()
    {
        var record = MakeEstimated();
        double total = UncertaintyDecomposer.TotalUncertainty(record);

        double expected = System.Math.Sqrt(
            0.01 * 0.01 + 0.02 * 0.02 + 0.03 * 0.03 +
            0.04 * 0.04 + 0.05 * 0.05 + 0.06 * 0.06);
        Assert.Equal(expected, total, 10);
    }

    [Fact]
    public void TotalUncertainty_AllUnestimated_ReturnsZero()
    {
        var record = UncertaintyRecord.Unestimated();
        Assert.Equal(0.0, UncertaintyDecomposer.TotalUncertainty(record));
    }

    [Fact]
    public void TotalUncertainty_MixedEstimated_SkipsNegative()
    {
        var record = new UncertaintyRecord
        {
            Discretization = 0.1,
            Solver = -1,
            Branch = 0.2,
            Extraction = -1,
            Calibration = -1,
            DataAsset = 0.3,
        };

        double expected = System.Math.Sqrt(0.01 + 0.04 + 0.09);
        Assert.Equal(expected, UncertaintyDecomposer.TotalUncertainty(record), 10);
    }

    [Fact]
    public void UnestimatedCount_AllEstimated_ReturnsZero()
    {
        Assert.Equal(0, UncertaintyDecomposer.UnestimatedCount(MakeEstimated()));
    }

    [Fact]
    public void UnestimatedCount_AllUnestimated_ReturnsSix()
    {
        Assert.Equal(6, UncertaintyDecomposer.UnestimatedCount(UncertaintyRecord.Unestimated()));
    }

    [Fact]
    public void UnestimatedCount_Mixed_ReturnsCorrectCount()
    {
        var record = new UncertaintyRecord
        {
            Discretization = 0.1,
            Solver = -1,
            Branch = 0.2,
            Extraction = -1,
            Calibration = -1,
            DataAsset = 0.3,
        };
        Assert.Equal(3, UncertaintyDecomposer.UnestimatedCount(record));
    }

    [Fact]
    public void IsFullyEstimated_AllEstimated_ReturnsTrue()
    {
        Assert.True(UncertaintyDecomposer.IsFullyEstimated(MakeEstimated()));
    }

    [Fact]
    public void IsFullyEstimated_AnyUnestimated_ReturnsFalse()
    {
        Assert.False(UncertaintyDecomposer.IsFullyEstimated(UncertaintyRecord.Unestimated()));
    }

    [Fact]
    public void Propagate_ConvergedNumerical_PreservesAssetUncertainty()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("converged");

        var propagated = UncertaintyDecomposer.Propagate(assetU, prediction);

        Assert.Equal(assetU.Discretization, propagated.Discretization);
        Assert.Equal(assetU.Solver, propagated.Solver);
        Assert.Equal(assetU.Branch, propagated.Branch);
        Assert.Equal(assetU.Extraction, propagated.Extraction);
        Assert.Equal(assetU.Calibration, propagated.Calibration);
        Assert.Equal(assetU.DataAsset, propagated.DataAsset);
    }

    [Fact]
    public void Propagate_FailedNumerical_MarksDiscretizationAndSolverUnestimated()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("failed");

        var propagated = UncertaintyDecomposer.Propagate(assetU, prediction);

        Assert.Equal(-1, propagated.Discretization);
        Assert.Equal(-1, propagated.Solver);
        Assert.Equal(assetU.Branch, propagated.Branch);
        Assert.Equal(assetU.Extraction, propagated.Extraction);
    }

    [Fact]
    public void Propagate_ExploratoryNumerical_PreservesAll()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("exploratory");

        var propagated = UncertaintyDecomposer.Propagate(assetU, prediction);

        Assert.Equal(assetU.Discretization, propagated.Discretization);
        Assert.Equal(assetU.Solver, propagated.Solver);
    }

    [Fact]
    public void Propagate_NullAsset_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            UncertaintyDecomposer.Propagate(null!, MakeRecord()));
    }

    [Fact]
    public void Propagate_NullPrediction_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            UncertaintyDecomposer.Propagate(MakeEstimated(), null!));
    }

    [Fact]
    public void TotalUncertainty_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            UncertaintyDecomposer.TotalUncertainty(null!));
    }

    // --- Decompose with BranchRunRecords ---

    [Fact]
    public void Decompose_AllConverged_RefinesBranchUncertainty()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("converged");
        var runs = new List<BranchRunRecord>
        {
            MakeBranchRun(converged: true, residualNorm: 0.05),
            MakeBranchRun(converged: true, residualNorm: 0.02),
        };

        var result = UncertaintyDecomposer.Decompose(assetU, prediction, runs);

        // Branch uncertainty should be max(original, maxResidual) = max(0.03, 0.05) = 0.05
        Assert.Equal(0.05, result.Branch);
        Assert.Equal(assetU.Solver, result.Solver);
        Assert.Equal(assetU.Discretization, result.Discretization);
    }

    [Fact]
    public void Decompose_SomeNotConverged_InflatesBranchUncertainty()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("converged");
        var runs = new List<BranchRunRecord>
        {
            MakeBranchRun(converged: true),
            MakeBranchRun(converged: false),
        };

        var result = UncertaintyDecomposer.Decompose(assetU, prediction, runs);

        // convergenceRatio = 0.5, so branch = max(0.03, 1.0 - 0.5) = 0.5
        Assert.Equal(0.5, result.Branch);
    }

    [Fact]
    public void Decompose_EmptyRuns_PreservesOriginal()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("converged");

        var result = UncertaintyDecomposer.Decompose(assetU, prediction, []);

        Assert.Equal(assetU.Branch, result.Branch);
        Assert.Equal(assetU.Solver, result.Solver);
    }

    [Fact]
    public void Decompose_FailedNumerical_MarksUnestimated()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("failed");
        var runs = new List<BranchRunRecord> { MakeBranchRun() };

        var result = UncertaintyDecomposer.Decompose(assetU, prediction, runs);

        Assert.Equal(-1, result.Solver);
        Assert.Equal(-1, result.Discretization);
    }

    [Fact]
    public void Decompose_NullBranchRuns_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            UncertaintyDecomposer.Decompose(MakeEstimated(), MakeRecord(), null!));
    }
}

using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison.Tests;

public sealed class UncertaintyPropagatorTests
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

    [Fact]
    public void TotalUncertainty_AllEstimated_QuadratureSum()
    {
        var record = MakeEstimated();
        double total = UncertaintyPropagator.TotalUncertainty(record);

        // sqrt(0.01^2 + 0.02^2 + 0.03^2 + 0.04^2 + 0.05^2 + 0.06^2)
        double expected = System.Math.Sqrt(
            0.01 * 0.01 + 0.02 * 0.02 + 0.03 * 0.03 +
            0.04 * 0.04 + 0.05 * 0.05 + 0.06 * 0.06);
        Assert.Equal(expected, total, 10);
    }

    [Fact]
    public void TotalUncertainty_AllUnestimated_ReturnsZero()
    {
        var record = UncertaintyRecord.Unestimated();
        Assert.Equal(0.0, UncertaintyPropagator.TotalUncertainty(record));
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
        Assert.Equal(expected, UncertaintyPropagator.TotalUncertainty(record), 10);
    }

    [Fact]
    public void UnestimatedCount_AllEstimated_ReturnsZero()
    {
        Assert.Equal(0, UncertaintyPropagator.UnestimatedCount(MakeEstimated()));
    }

    [Fact]
    public void UnestimatedCount_AllUnestimated_ReturnsSix()
    {
        Assert.Equal(6, UncertaintyPropagator.UnestimatedCount(UncertaintyRecord.Unestimated()));
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
        Assert.Equal(3, UncertaintyPropagator.UnestimatedCount(record));
    }

    [Fact]
    public void IsFullyEstimated_AllEstimated_ReturnsTrue()
    {
        Assert.True(UncertaintyPropagator.IsFullyEstimated(MakeEstimated()));
    }

    [Fact]
    public void IsFullyEstimated_AnyUnestimated_ReturnsFalse()
    {
        Assert.False(UncertaintyPropagator.IsFullyEstimated(UncertaintyRecord.Unestimated()));
    }

    [Fact]
    public void Propagate_ConvergedNumerical_PreservesAssetUncertainty()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("converged");

        var propagated = UncertaintyPropagator.Propagate(assetU, prediction);

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

        var propagated = UncertaintyPropagator.Propagate(assetU, prediction);

        Assert.Equal(-1, propagated.Discretization);
        Assert.Equal(-1, propagated.Solver);
        // Other components preserved
        Assert.Equal(assetU.Branch, propagated.Branch);
        Assert.Equal(assetU.Extraction, propagated.Extraction);
    }

    [Fact]
    public void Propagate_ExploratoryNumerical_PreservesAll()
    {
        var assetU = MakeEstimated();
        var prediction = MakeRecord("exploratory");

        var propagated = UncertaintyPropagator.Propagate(assetU, prediction);

        // Exploratory is not failed, so solver/discretization preserved
        Assert.Equal(assetU.Discretization, propagated.Discretization);
        Assert.Equal(assetU.Solver, propagated.Solver);
    }

    [Fact]
    public void Propagate_NullAsset_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            UncertaintyPropagator.Propagate(null!, MakeRecord()));
    }

    [Fact]
    public void Propagate_NullPrediction_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            UncertaintyPropagator.Propagate(MakeEstimated(), null!));
    }

    [Fact]
    public void TotalUncertainty_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            UncertaintyPropagator.TotalUncertainty(null!));
    }
}

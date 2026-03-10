using Gu.Phase3.Backgrounds;

namespace Gu.Phase3.Backgrounds.Tests;

/// <summary>
/// Dedicated tests for background deduplication logic.
/// Tests threshold sensitivity, different-environment handling, and edge cases.
/// </summary>
public class BackgroundDeduplicatorTests
{
    [Fact]
    public void IdenticalStates_AreDeduplicatedToOne()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7),
            TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(records, 1e-6);
        Assert.Single(deduped);
    }

    [Fact]
    public void DistantStates_AreBothRetained()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-3, stationarityNorm: 1e-3),
            TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1, residualNorm: 1e-9, stationarityNorm: 1e-9),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(records, 1e-6);
        Assert.Equal(2, deduped.Count);
    }

    [Fact]
    public void ThresholdSensitivity_TightThresholdKeepsBoth()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7),
            TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1, residualNorm: 1.1e-5, stationarityNorm: 1.1e-7),
        };

        // Distance ~ 1e-6 + 1e-8 ~ 1.01e-6
        // Tight threshold keeps both
        var dedupedTight = BackgroundAtlasBuilder.Deduplicate(records, 1e-8);
        Assert.Equal(2, dedupedTight.Count);

        // Loose threshold merges them
        var dedupedLoose = BackgroundAtlasBuilder.Deduplicate(records, 1e-4);
        Assert.Single(dedupedLoose);
    }

    [Fact]
    public void DifferentEnvironments_NeverDeduplicated()
    {
        // Even identical norms should not be deduplicated across environments
        var r1 = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7);
        // Override environment for r2
        var r2 = new BackgroundRecord
        {
            BackgroundId = "bg-2",
            EnvironmentId = "env-different",
            BranchManifestId = "branch-1",
            GeometryFingerprint = "test-fingerprint",
            StateArtifactRef = "state-bg-2",
            ResidualNorm = 1e-5,
            StationarityNorm = 1e-7,
            AdmissibilityLevel = AdmissibilityLevel.B1,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 1e-5,
                StationarityNorm = 1e-7,
                ObjectiveValue = 5e-11,
                GaugeViolation = 0,
                SolverIterations = 10,
                SolverConverged = true,
                TerminationReason = "converged",
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R2",
            Provenance = TestHelpers.MakeProvenance(),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(new List<BackgroundRecord> { r1, r2 }, 1e-3);
        Assert.Equal(2, deduped.Count);
    }

    [Fact]
    public void DifferentBranches_NeverDeduplicated()
    {
        var r1 = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7);
        var r2 = new BackgroundRecord
        {
            BackgroundId = "bg-2",
            EnvironmentId = "env-1",
            BranchManifestId = "branch-different",
            GeometryFingerprint = "test-fingerprint",
            StateArtifactRef = "state-bg-2",
            ResidualNorm = 1e-5,
            StationarityNorm = 1e-7,
            AdmissibilityLevel = AdmissibilityLevel.B1,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 1e-5,
                StationarityNorm = 1e-7,
                ObjectiveValue = 5e-11,
                GaugeViolation = 0,
                SolverIterations = 10,
                SolverConverged = true,
                TerminationReason = "converged",
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R2",
            Provenance = TestHelpers.MakeProvenance(),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(new List<BackgroundRecord> { r1, r2 }, 1e-3);
        Assert.Equal(2, deduped.Count);
    }

    [Fact]
    public void SingleRecord_ReturnedAsIs()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B2),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(records, 1e-6);
        Assert.Single(deduped);
        Assert.Equal("bg-1", deduped[0].BackgroundId);
    }

    [Fact]
    public void EmptyList_ReturnsEmpty()
    {
        var deduped = BackgroundAtlasBuilder.Deduplicate(new List<BackgroundRecord>(), 1e-6);
        Assert.Empty(deduped);
    }

    [Fact]
    public void BetterAdmissibilityPreferred_WhenDuplicate()
    {
        // B2 should be kept over B0 when they are duplicates
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-b0", AdmissibilityLevel.B0, residualNorm: 1e-5, stationarityNorm: 1e-7),
            TestHelpers.MakeRecord("bg-b2", AdmissibilityLevel.B2, residualNorm: 1e-5, stationarityNorm: 1e-7),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(records, 1e-3);
        Assert.Single(deduped);
        // B2 is strongest, so B2 sorts first (OrderByDescending)
        Assert.Equal("bg-b2", deduped[0].BackgroundId);
    }

    [Fact]
    public void ThreeCloseStates_DeduplicatedToOne()
    {
        var records = new List<BackgroundRecord>
        {
            TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1, residualNorm: 1e-5, stationarityNorm: 1e-7),
            TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1, residualNorm: 1.001e-5, stationarityNorm: 1.001e-7),
            TestHelpers.MakeRecord("bg-3", AdmissibilityLevel.B1, residualNorm: 1.002e-5, stationarityNorm: 1.002e-7),
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(records, 1e-3);
        Assert.Single(deduped);
    }

    [Fact]
    public void L2StateDistance_IdenticalStates_AreDeduplicated()
    {
        var r1 = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1);
        var r2 = TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1);

        var state = new Gu.Core.FieldTensor
        {
            Label = "omega",
            Signature = new Gu.Core.TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 3 },
        };

        var states = new Dictionary<string, Gu.Core.FieldTensor>
        {
            ["bg-1"] = state,
            ["bg-2"] = state,
        };

        var deduped = BackgroundAtlasBuilder.Deduplicate(
            new List<BackgroundRecord> { r1, r2 }, 0.01, states);
        Assert.Single(deduped);
    }

    [Fact]
    public void L2StateDistance_DifferentStates_AreRetained()
    {
        var r1 = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1);
        var r2 = TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1);

        var sig = new Gu.Core.TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "edge-major",
            MemoryLayout = "dense-row-major",
        };

        var states = new Dictionary<string, Gu.Core.FieldTensor>
        {
            ["bg-1"] = new Gu.Core.FieldTensor
            {
                Label = "omega-1", Signature = sig,
                Coefficients = new double[] { 1.0, 0.0, 0.0 },
                Shape = new[] { 3 },
            },
            ["bg-2"] = new Gu.Core.FieldTensor
            {
                Label = "omega-2", Signature = sig,
                Coefficients = new double[] { 0.0, 0.0, 10.0 },
                Shape = new[] { 3 },
            },
        };

        // Relative distance = ||[1,0,-10]|| / max(1, 1) = ~10.05, well above threshold
        var deduped = BackgroundAtlasBuilder.Deduplicate(
            new List<BackgroundRecord> { r1, r2 }, 0.01, states);
        Assert.Equal(2, deduped.Count);
    }

    [Fact]
    public void L2StateDistance_UsesRelativeDistance()
    {
        var r1 = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1);
        var r2 = TestHelpers.MakeRecord("bg-2", AdmissibilityLevel.B1);

        var sig = new Gu.Core.TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "edge-major",
            MemoryLayout = "dense-row-major",
        };

        // Two states that differ by 0.001 with norm ~100
        // Relative distance = 0.001 / 100 = 1e-5
        var states = new Dictionary<string, Gu.Core.FieldTensor>
        {
            ["bg-1"] = new Gu.Core.FieldTensor
            {
                Label = "omega-1", Signature = sig,
                Coefficients = new double[] { 100.0 },
                Shape = new[] { 1 },
            },
            ["bg-2"] = new Gu.Core.FieldTensor
            {
                Label = "omega-2", Signature = sig,
                Coefficients = new double[] { 100.001 },
                Shape = new[] { 1 },
            },
        };

        // Threshold 1e-4: relative distance 1e-5 < 1e-4 -> deduplicated
        var dedupedLoose = BackgroundAtlasBuilder.Deduplicate(
            new List<BackgroundRecord> { r1, r2 }, 1e-4, states);
        Assert.Single(dedupedLoose);

        // Threshold 1e-6: relative distance 1e-5 > 1e-6 -> retained
        var dedupedTight = BackgroundAtlasBuilder.Deduplicate(
            new List<BackgroundRecord> { r1, r2 }, 1e-6, states);
        Assert.Equal(2, dedupedTight.Count);
    }
}

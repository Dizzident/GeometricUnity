using Gu.Core;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Falsification.Tests;

/// <summary>
/// WP-7: Tests for the four deferred falsifier types activated by WP-7:
///   ObservationInstability, EnvironmentInstability, RepresentationContent, CouplingInconsistency.
///
/// One positive and one negative test per falsifier type (8 total).
/// </summary>
public sealed class FalsifierEvaluatorWp7Tests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "wp7-test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
    };

    private static FalsificationPolicy DefaultPolicy() => new FalsificationPolicy();

    // ------------------------------------------------------------------
    // ObservationInstability (WP-7 new)
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_ObservationInstability_HighSensitivity_ProducesActiveHighFalsifier()
    {
        // Positive: SensitivityScore > ObservationInstabilityThreshold (0.3)
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new ObservationChainRecord
            {
                CandidateId = "cand-obs-A",
                PrimarySourceId = "src-A",
                ObservableId = "obs-ratio-1",
                CompletenessStatus = "complete",
                SensitivityScore = 0.6,          // above default threshold 0.3
                AuxiliaryModelSensitivity = 0.1,
                Passed = true,
                Provenance = MakeProvenance(),
            },
        };

        var summary = evaluator.Evaluate(
            "study-obs-instability",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: null,
            policy: DefaultPolicy(),
            provenance: MakeProvenance(),
            observationRecords: records);

        Assert.Single(summary.Falsifiers);
        var f = summary.Falsifiers[0];
        Assert.Equal(FalsifierTypes.ObservationInstability, f.FalsifierType);
        Assert.Equal(FalsifierSeverity.High, f.Severity);
        Assert.True(f.Active);
        Assert.Equal("cand-obs-A", f.TargetId);
        Assert.Equal(0.6, f.TriggerValue);
        Assert.Equal(1, summary.TotalActiveCount);
        Assert.Equal(1, summary.ActiveHighCount);
    }

    [Fact]
    public void Evaluate_ObservationInstability_LowSensitivity_NoFalsifier()
    {
        // Negative: SensitivityScore <= ObservationInstabilityThreshold (0.3)
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new ObservationChainRecord
            {
                CandidateId = "cand-obs-B",
                PrimarySourceId = "src-B",
                ObservableId = "obs-ratio-2",
                CompletenessStatus = "complete",
                SensitivityScore = 0.1,          // below default threshold 0.3
                AuxiliaryModelSensitivity = 0.05,
                Passed = true,
                Provenance = MakeProvenance(),
            },
        };

        var summary = evaluator.Evaluate(
            "study-obs-ok",
            null, null, null, null,
            DefaultPolicy(),
            MakeProvenance(),
            observationRecords: records);

        Assert.Empty(summary.Falsifiers);
        Assert.Equal(0, summary.TotalActiveCount);
    }

    // ------------------------------------------------------------------
    // EnvironmentInstability (WP-7: RelativeStdDev trigger)
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_EnvironmentVariance_AboveThreshold_ProducesActiveHighFalsifier()
    {
        // Positive: RelativeStdDev > EnvironmentInstabilityThreshold (0.3)
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new EnvironmentVarianceRecord
            {
                RecordId = "ev-001",
                QuantityId = "q-mass",
                EnvironmentTierId = "tier-3d",
                RelativeStdDev = 0.6,  // above default threshold 0.3
                Flagged = true,
            },
        };

        var summary = evaluator.Evaluate(
            "study-env-instability",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: null,
            policy: DefaultPolicy(),
            provenance: MakeProvenance(),
            environmentVarianceRecords: records);

        Assert.Single(summary.Falsifiers);
        var f = summary.Falsifiers[0];
        Assert.Equal(FalsifierTypes.EnvironmentInstability, f.FalsifierType);
        Assert.Equal(FalsifierSeverity.High, f.Severity);
        Assert.True(f.Active);
        Assert.Equal("q-mass", f.TargetId);
        Assert.Equal("tier-3d", f.EnvironmentId);
        Assert.Equal(0.6, f.TriggerValue);
        Assert.Equal(1, summary.TotalActiveCount);
        Assert.Equal(1, summary.ActiveHighCount);
    }

    [Fact]
    public void Evaluate_EnvironmentVariance_BelowThreshold_NoFalsifier()
    {
        // Negative: RelativeStdDev <= EnvironmentInstabilityThreshold (0.3)
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new EnvironmentVarianceRecord
            {
                RecordId = "ev-002",
                QuantityId = "q-mass",
                EnvironmentTierId = "tier-2d",
                RelativeStdDev = 0.1,  // below default threshold 0.3
                Flagged = false,
            },
        };

        var summary = evaluator.Evaluate(
            "study-env-ok",
            null, null, null, null,
            DefaultPolicy(),
            MakeProvenance(),
            environmentVarianceRecords: records);

        Assert.Empty(summary.Falsifiers);
        Assert.Equal(0, summary.TotalActiveCount);
    }

    // ------------------------------------------------------------------
    // RepresentationContent (WP-7: MissingRequiredCount / StructuralMismatchScore)
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_RepresentationContent_MissingRequired_ProducesActiveFatalFalsifier()
    {
        // Positive: MissingRequiredCount > 0 → Fatal
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new RepresentationContentRecord
            {
                RecordId = "rc-001",
                CandidateId = "cand-boson-A",
                ExpectedModeCount = 3,
                ObservedModeCount = 1,
                MissingRequiredCount = 2,        // triggers Fatal
                StructuralMismatchScore = 0.0,
                Consistent = false,
                InconsistencyDescription = "Missing 2 required representation modes.",
            },
        };

        var summary = evaluator.Evaluate(
            "study-rep-content-fatal",
            null, null, null, null,
            DefaultPolicy(),
            MakeProvenance(),
            representationContentRecords: records);

        Assert.Single(summary.Falsifiers);
        var f = summary.Falsifiers[0];
        Assert.Equal(FalsifierTypes.RepresentationContent, f.FalsifierType);
        Assert.Equal(FalsifierSeverity.Fatal, f.Severity);
        Assert.True(f.Active);
        Assert.Equal("cand-boson-A", f.TargetId);
        Assert.Equal(1, summary.ActiveFatalCount);
    }

    [Fact]
    public void Evaluate_RepresentationContent_NoneTriggered_NoFalsifier()
    {
        // Negative: MissingRequiredCount == 0 and StructuralMismatchScore <= threshold (0.2)
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new RepresentationContentRecord
            {
                RecordId = "rc-002",
                CandidateId = "cand-boson-B",
                ExpectedModeCount = 3,
                ObservedModeCount = 3,
                MissingRequiredCount = 0,        // no fatal trigger
                StructuralMismatchScore = 0.05,  // below threshold 0.2 → no high trigger
                Consistent = true,
            },
        };

        var summary = evaluator.Evaluate(
            "study-rep-ok",
            null, null, null, null,
            DefaultPolicy(),
            MakeProvenance(),
            representationContentRecords: records);

        Assert.Empty(summary.Falsifiers);
    }

    // ------------------------------------------------------------------
    // CouplingInconsistency
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_CouplingSpread_AboveThreshold_ProducesActiveHighFalsifier()
    {
        // Positive: RelativeSpread > CouplingInconsistencyThreshold (0.3)
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new CouplingConsistencyRecord
            {
                RecordId = "cc-001",
                CandidateId = "cand-fermion-X",
                CouplingType = "gauge",
                RelativeSpread = 0.5,  // above default threshold 0.3
                Consistent = false,
            },
        };

        var summary = evaluator.Evaluate(
            "study-coupling-inconsistent",
            null, null, null, null,
            DefaultPolicy(),
            MakeProvenance(),
            couplingConsistencyRecords: records);

        Assert.Single(summary.Falsifiers);
        var f = summary.Falsifiers[0];
        Assert.Equal(FalsifierTypes.CouplingInconsistency, f.FalsifierType);
        Assert.Equal(FalsifierSeverity.High, f.Severity);
        Assert.True(f.Active);
        Assert.Equal("cand-fermion-X", f.TargetId);
        Assert.Equal(0.5, f.TriggerValue);
        Assert.Equal(1, summary.TotalActiveCount);
        Assert.Equal(1, summary.ActiveHighCount);
    }

    [Fact]
    public void Evaluate_CouplingSpread_BelowThreshold_NoFalsifier()
    {
        // Negative: RelativeSpread <= CouplingInconsistencyThreshold (0.3)
        var evaluator = new FalsifierEvaluator();
        var records = new[]
        {
            new CouplingConsistencyRecord
            {
                RecordId = "cc-002",
                CandidateId = "cand-fermion-Y",
                CouplingType = "yukawa",
                RelativeSpread = 0.1,  // below default threshold 0.3
                Consistent = true,
            },
        };

        var summary = evaluator.Evaluate(
            "study-coupling-ok",
            null, null, null, null,
            DefaultPolicy(),
            MakeProvenance(),
            couplingConsistencyRecords: records);

        Assert.Empty(summary.Falsifiers);
    }

    // ------------------------------------------------------------------
    // Combined: all four new types simultaneously
    // ------------------------------------------------------------------

    [Fact]
    public void Evaluate_AllFourNewTypes_AggregatesAll()
    {
        var evaluator = new FalsifierEvaluator();

        var obsRecords = new[]
        {
            new ObservationChainRecord
            {
                CandidateId = "cand-obs", PrimarySourceId = "src-all", ObservableId = "obs-1",
                CompletenessStatus = "complete", SensitivityScore = 0.5,
                AuxiliaryModelSensitivity = 0.1, Passed = true, Provenance = MakeProvenance(),
            },
        };
        var envRecords = new[]
        {
            new EnvironmentVarianceRecord
            {
                RecordId = "ev-c01", QuantityId = "q-a", EnvironmentTierId = "t1",
                RelativeStdDev = 0.9, Flagged = true,
            },
        };
        var repRecords = new[]
        {
            new RepresentationContentRecord
            {
                RecordId = "rc-c01", CandidateId = "cand-1",
                ExpectedModeCount = 2, ObservedModeCount = 4,
                MissingRequiredCount = 1, StructuralMismatchScore = 0.0,
                Consistent = false,
            },
        };
        var couplingRecords = new[]
        {
            new CouplingConsistencyRecord
            {
                RecordId = "cc-c01", CandidateId = "cand-2",
                CouplingType = "gauge", RelativeSpread = 0.7, Consistent = false,
            },
        };

        var summary = evaluator.Evaluate(
            "study-all-new",
            null, null, null, null,
            DefaultPolicy(),
            MakeProvenance(),
            observationRecords: obsRecords,
            environmentVarianceRecords: envRecords,
            representationContentRecords: repRecords,
            couplingConsistencyRecords: couplingRecords);

        Assert.Equal(4, summary.Falsifiers.Count);
        Assert.All(summary.Falsifiers, f => Assert.True(f.Active));
        Assert.Equal(4, summary.TotalActiveCount);
        Assert.Equal(1, summary.ActiveFatalCount);   // RepresentationContent (MissingRequired)
        Assert.Equal(3, summary.ActiveHighCount);    // Obs + Env + Coupling are High
    }
}

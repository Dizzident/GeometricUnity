using Gu.Artifacts;
using Gu.Core.Serialization;

namespace Gu.Phase5.Dossiers.Tests;

/// <summary>
/// Tests for M51: ClaimEscalationRecord and ClaimEscalationEvaluator.
/// Phase V rule §12.5: promotion must be rule-based, not narrative-based.
/// </summary>
public sealed class ClaimEscalationRecordTests
{
    private static EscalationGateResult PassGate(string id, string desc = "Test gate") => new()
    {
        GateId = id,
        Description = desc,
        Passed = true,
        Required = true,
        Evidence = "Test evidence: gate passed.",
    };

    private static EscalationGateResult FailGate(string id, string desc = "Test gate") => new()
    {
        GateId = id,
        Description = desc,
        Passed = false,
        Required = true,
        Evidence = "Test evidence: gate failed.",
    };

    private static EscalationGateResult OptionalFailGate(string id) => new()
    {
        GateId = id,
        Description = "Optional gate",
        Passed = false,
        Required = false,
    };

    // ── Escalation ─────────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_AllGatesPass_EscalationApproved()
    {
        var gates = new List<EscalationGateResult>
        {
            PassGate("gate-1-branch-robustness", "Survives branch variation"),
            PassGate("gate-2-refinement", "Survives refinement"),
            PassGate("gate-5-quantitative", "Passes quantitative comparison"),
        };

        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-001", "boson-mode-0", "boson",
            currentClass: "C3", proposedClass: "C4",
            gateResults: gates, sourceStudyId: "study-001");

        Assert.True(record.AllGatesPassed);
        Assert.Equal("escalation", record.Direction);
        Assert.Equal("C4", record.ProposedClaimClass);
        Assert.Null(record.DemotionOrBlockReason);
    }

    [Fact]
    public void Evaluate_RequiredGateFails_EscalationBlocked()
    {
        var gates = new List<EscalationGateResult>
        {
            PassGate("gate-1-branch-robustness"),
            FailGate("gate-2-refinement", "Non-convergent refinement sweep"),
        };

        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-001", "boson-mode-0", "boson",
            currentClass: "C3", proposedClass: "C4",
            gateResults: gates, sourceStudyId: "study-001");

        Assert.False(record.AllGatesPassed);
        Assert.Equal("no-change", record.Direction);
        Assert.Equal("C3", record.ProposedClaimClass); // blocked — stays at C3
        Assert.NotNull(record.DemotionOrBlockReason);
        Assert.Contains("gate-2-refinement", record.DemotionOrBlockReason);
    }

    [Fact]
    public void Evaluate_OptionalGateFails_EscalationStillApproved()
    {
        var gates = new List<EscalationGateResult>
        {
            PassGate("gate-1-branch-robustness"),
            OptionalFailGate("gate-optional-extra"),
        };

        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-001", "boson-mode-0", "boson",
            currentClass: "C2", proposedClass: "C3",
            gateResults: gates, sourceStudyId: "study-001");

        Assert.True(record.AllGatesPassed);
        Assert.Equal("escalation", record.Direction);
        Assert.Equal("C3", record.ProposedClaimClass);
    }

    // ── Demotion ─────────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_ProposedClassLower_ClassifiedAsDemotion()
    {
        var gates = new List<EscalationGateResult>
        {
            FailGate("gate-1-branch-robustness", "Branch fragility detected"),
        };

        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-001", "boson-mode-0", "boson",
            currentClass: "C4", proposedClass: "C2",
            gateResults: gates, sourceStudyId: "study-001");

        Assert.Equal("demotion", record.Direction);
        Assert.Equal("C2", record.ProposedClaimClass);
        Assert.NotNull(record.DemotionOrBlockReason);
    }

    // ── No change ────────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_SameClass_NoChange()
    {
        var gates = new List<EscalationGateResult>
        {
            PassGate("gate-1"),
        };

        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-001", "boson-mode-0", "boson",
            currentClass: "C3", proposedClass: "C3",
            gateResults: gates, sourceStudyId: "study-001");

        Assert.Equal("no-change", record.Direction);
    }

    // ── Gate results are preserved ────────────────────────────────────────────

    [Fact]
    public void Evaluate_GateResults_AllPreserved()
    {
        var gates = new List<EscalationGateResult>
        {
            PassGate("gate-1"),
            PassGate("gate-2"),
            FailGate("gate-3"),
        };

        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-001", "boson-mode-0", "boson",
            currentClass: "C2", proposedClass: "C3",
            gateResults: gates, sourceStudyId: "study-001");

        Assert.Equal(3, record.GateResults.Count);
    }

    // ── Provider identity ─────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_RecordFields_SetCorrectly()
    {
        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-test-id", "fermion-mode-0", "fermion",
            currentClass: "C0", proposedClass: "C1",
            gateResults: new List<EscalationGateResult> { PassGate("g1") },
            sourceStudyId: "source-study");

        Assert.Equal("rec-test-id", record.RecordId);
        Assert.Equal("fermion-mode-0", record.CandidateId);
        Assert.Equal("fermion", record.RegistryType);
        Assert.Equal("C0", record.CurrentClaimClass);
        Assert.Equal("source-study", record.SourceStudyId);
    }

    // ── Serialization round-trip ──────────────────────────────────────────────

    [Fact]
    public void ClaimEscalationRecord_SerializesAndDeserializes()
    {
        var gates = new List<EscalationGateResult>
        {
            PassGate("gate-1-branch-robustness", "Survives admissible branch variations"),
            PassGate("gate-2-refinement", "Survives refinement with bounded uncertainty"),
        };

        var record = ClaimEscalationEvaluator.Evaluate(
            "rec-roundtrip", "boson-mode-1", "boson",
            currentClass: "C3", proposedClass: "C4",
            gateResults: gates, sourceStudyId: "study-round-trip");

        var json = GuJsonDefaults.Serialize(record);
        var roundtrip = GuJsonDefaults.Deserialize<ClaimEscalationRecord>(json);

        Assert.NotNull(roundtrip);
        Assert.Equal("rec-roundtrip", roundtrip!.RecordId);
        Assert.Equal("escalation", roundtrip.Direction);
        Assert.Equal("C4", roundtrip.ProposedClaimClass);
        Assert.Equal(2, roundtrip.GateResults.Count);
        Assert.True(roundtrip.AllGatesPassed);
    }

    [Fact]
    public void EscalationGateResult_SerializesAndDeserializes()
    {
        var gate = PassGate("gate-1-branch-robustness", "Survives branch variations");
        var json = GuJsonDefaults.Serialize(gate);
        var roundtrip = GuJsonDefaults.Deserialize<EscalationGateResult>(json);

        Assert.NotNull(roundtrip);
        Assert.Equal("gate-1-branch-robustness", roundtrip!.GateId);
        Assert.True(roundtrip.Passed);
        Assert.True(roundtrip.Required);
    }

    // ── Claim class ranks ─────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_C0ToC5_FullEscalationPath_WhenAllGatesPass()
    {
        // Verify the full promotion chain C0 -> C1 -> C2 -> C3 -> C4 -> C5
        var classes = new[] { "C0", "C1", "C2", "C3", "C4", "C5" };
        for (int i = 0; i < classes.Length - 1; i++)
        {
            var record = ClaimEscalationEvaluator.Evaluate(
                $"rec-{i}", "boson-mode-0", "boson",
                currentClass: classes[i], proposedClass: classes[i + 1],
                gateResults: new List<EscalationGateResult> { PassGate("g1") },
                sourceStudyId: "study-001");

            Assert.Equal("escalation", record.Direction);
            Assert.Equal(classes[i + 1], record.ProposedClaimClass);
        }
    }
}

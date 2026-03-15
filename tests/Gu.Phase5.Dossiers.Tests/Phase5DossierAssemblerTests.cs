using Gu.Core;
using Gu.Phase4.Registry;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Dossiers.Tests;

/// <summary>
/// Tests for Phase5DossierAssembler (M51+M52): claim escalation and negative results.
/// </summary>
public class Phase5DossierAssemblerTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
    };

    private static UnifiedParticleRecord MakeCandidate(
        string id,
        string claimClass = "C2_ReproducibleMode",
        double obsConfidence = 0.8) => new UnifiedParticleRecord
    {
        ParticleId = id,
        ParticleType = UnifiedParticleType.Boson,
        PrimarySourceId = $"src-{id}",
        ContributingSourceIds = new List<string> { $"src-{id}" },
        BranchVariantSet = new List<string> { "v1", "v2" },
        BackgroundSet = new List<string> { "bg1", "bg2" },
        MassLikeEnvelope = new double[] { 1.0, 2.0 },
        ClaimClass = claimClass,
        ObservationConfidence = obsConfidence,
        RegistryVersion = "1.0.0",
        Provenance = MakeProvenance(),
    };

    private static UnifiedParticleRegistry MakeRegistry(params UnifiedParticleRecord[] candidates)
    {
        var registry = new UnifiedParticleRegistry();
        foreach (var c in candidates)
            registry.Candidates.Add(c);
        return registry;
    }

    private static BranchRobustnessRecord MakeRobustBranchRecord() => new BranchRobustnessRecord
    {
        RecordId = "br-001",
        StudyId = "branch-study",
        BranchVariantIds = new List<string> { "v1", "v2" },
        DistanceMatrices = new Dictionary<string, BranchDistanceMatrix>(),
        EquivalenceClasses = new Dictionary<string, List<BranchEquivalenceClass>>(),
        FragilityRecords = new Dictionary<string, FragilityRecord>
        {
            ["qty-1"] = new FragilityRecord
            {
                TargetQuantityId = "qty-1",
                Classification = "invariant",
                FragilityScore = 0.05,
                MaxDistancePair = new[] { "v1", "v2" },
            },
        },
        InvarianceCandidates = new List<InvarianceCandidateRecord>
        {
            new InvarianceCandidateRecord
            {
                TargetQuantityId = "qty-1",
                SourceStudyId = "branch-study",
                BranchFamilySize = 2,
            },
        },
        OverallSummary = "robust",
        Provenance = MakeProvenance(),
    };

    private static BranchRobustnessRecord MakeFragileBranchRecord() => new BranchRobustnessRecord
    {
        RecordId = "br-002",
        StudyId = "branch-study",
        BranchVariantIds = new List<string> { "v1", "v2" },
        DistanceMatrices = new Dictionary<string, BranchDistanceMatrix>(),
        EquivalenceClasses = new Dictionary<string, List<BranchEquivalenceClass>>(),
        FragilityRecords = new Dictionary<string, FragilityRecord>
        {
            ["qty-1"] = new FragilityRecord
            {
                TargetQuantityId = "qty-1",
                Classification = "fragile",
                FragilityScore = 0.9,
                MaxDistancePair = new[] { "v1", "v2" },
            },
        },
        InvarianceCandidates = new List<InvarianceCandidateRecord>(),
        OverallSummary = "fragile",
        Provenance = MakeProvenance(),
    };

    private static IReadOnlyList<ContinuumEstimateRecord> MakeBoundedConvergence() =>
        new List<ContinuumEstimateRecord>
        {
            new ContinuumEstimateRecord
            {
                QuantityId = "qty-1",
                ExtrapolatedValue = 100.0,
                ErrorBand = 5.0,   // 5% < 10%
                ConvergenceOrder = 2.0,
                ConvergenceClassification = "convergent",
                ConfidenceNote = "Good",
                RunRecords = new List<RefinementRunRecord>(),
            },
        };

    private static ConsistencyScoreCard MakePassingScorecard() => new ConsistencyScoreCard
    {
        StudyId = "qv-study",
        SchemaVersion = "1.0.0",
        Matches = new List<TargetMatchRecord>
        {
            new TargetMatchRecord
            {
                ObservableId = "obs-test-1",
                TargetLabel = "pdg-ref",
                TargetValue = 100.0,
                TargetUncertainty = 1.0,
                ComputedValue = 100.5,
                ComputedUncertainty = 0.5,
                Pull = 0.45,
                Passed = true,
            },
        },
        TotalPassed = 1,
        TotalFailed = 0,
        OverallScore = 1.0,
        CalibrationPolicyId = "default",
        Provenance = MakeProvenance(),
    };

    private static IReadOnlyList<EnvironmentVarianceRecord> MakeEnvironmentVariance(string quantityId = "obs-test-1") =>
        new List<EnvironmentVarianceRecord>
        {
            new EnvironmentVarianceRecord
            {
                RecordId = $"env-var-{quantityId}",
                QuantityId = quantityId,
                EnvironmentTierId = "toy+structured",
                RelativeStdDev = 0.08,
                Flagged = false,
                Provenance = MakeProvenance(),
            },
        };

    private static FalsifierSummary MakeEmptyFalsifiers() => new FalsifierSummary
    {
        StudyId = "falsification-study",
        Falsifiers = new List<FalsifierRecord>(),
        ActiveFatalCount = 0,
        ActiveHighCount = 0,
        TotalActiveCount = 0,
        Provenance = MakeProvenance(),
    };

    private static FalsifierSummary MakeFatalFalsifier(string targetId) => new FalsifierSummary
    {
        StudyId = "falsification-study",
        Falsifiers = new List<FalsifierRecord>
        {
            new FalsifierRecord
            {
                FalsifierId = "f-001",
                FalsifierType = FalsifierTypes.BranchFragility,
                Severity = FalsifierSeverity.Fatal,
                TargetId = targetId,
                BranchId = "test-branch",
                Description = "Branch fragile",
                Evidence = "fragility > 0.9",
                Active = true,
                Provenance = MakeProvenance(),
            },
        },
        ActiveFatalCount = 1,
        ActiveHighCount = 0,
        TotalActiveCount = 1,
        Provenance = MakeProvenance(),
    };

    private static EnvironmentRecord MakeEnvironment(string id, string tier) =>
        new EnvironmentRecord
        {
            EnvironmentId = id,
            GeometryTier = tier,
            GeometryFingerprint = $"fp-{id}",
            BaseDimension = 4,
            AmbientDimension = 14,
            EdgeCount = 100,
            FaceCount = 200,
            Admissibility = new EnvironmentAdmissibilityReport
            {
                Level = tier,
                Checks = new List<AdmissibilityCheck>(),
                Passed = true,
            },
            Provenance = MakeProvenance(),
        };

    private readonly Phase5DossierAssembler _assembler = new();

    [Fact]
    public void Assemble_NoRegistry_ProducesEmptyEscalations()
    {
        var dossier = _assembler.Assemble(
            studyId: "study-001",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: null,
            registry: null,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.Equal("dossier-study-001", dossier.DossierId);
        Assert.Equal("study-001", dossier.StudyId);
        Assert.Empty(dossier.ClaimEscalations);
        Assert.Empty(dossier.NegativeResults);
    }

    [Fact]
    public void Assemble_AllGatesPassed_CandidateEscalated()
    {
        var candidate = MakeCandidate("p-001", claimClass: "C2_ReproducibleMode", obsConfidence: 0.9);
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-002",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: new List<EnvironmentRecord>
            {
                MakeEnvironment("env-1", "coarse"),
                MakeEnvironment("env-2", "medium"),
            },
            scoreCard: MakePassingScorecard(),
            falsifiers: MakeEmptyFalsifiers(),
            registry: registry,
            environmentTiersCovered: new List<string> { "coarse", "medium" },
            freshness: "fresh",
            provenance: MakeProvenance(),
            observationChainRecords: [MakeObsChainRecord("p-001", "src-p-001")],
            environmentVarianceRecords: MakeEnvironmentVariance());

        Assert.Single(dossier.ClaimEscalations);
        var esc = dossier.ClaimEscalations[0];
        Assert.Equal("p-001", esc.CandidateId);
        Assert.Equal("escalation", esc.Direction);
        Assert.True(esc.AllGatesPassed);
        Assert.Equal("C3_BranchStableCandidate", esc.ProposedClaimClass);
        Assert.All(esc.GateResults, gate => Assert.NotNull(gate.EvidenceRecordIds));
    }

    [Fact]
    public void Assemble_FatalFalsifier_CandidateDemotedToC0()
    {
        var candidate = MakeCandidate("p-002", claimClass: "C3_BranchStableCandidate");
        var registry = MakeRegistry(candidate);
        var falsifiers = MakeFatalFalsifier("p-002");

        var dossier = _assembler.Assemble(
            studyId: "study-003",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: new List<EnvironmentRecord>
            {
                MakeEnvironment("env-1", "coarse"),
                MakeEnvironment("env-2", "medium"),
            },
            scoreCard: MakePassingScorecard(),
            falsifiers: falsifiers,
            registry: registry,
            environmentTiersCovered: new List<string> { "coarse", "medium" },
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.Single(dossier.ClaimEscalations);
        var esc = dossier.ClaimEscalations[0];
        Assert.Equal("demotion", esc.Direction);
        Assert.Equal("C0_NumericalMode", esc.ProposedClaimClass);
        Assert.NotNull(esc.DemotionOrBlockReason);
    }

    [Fact]
    public void Assemble_BranchNotRobust_EscalationHeld()
    {
        var candidate = MakeCandidate("p-003", claimClass: "C2_ReproducibleMode", obsConfidence: 0.9);
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-004",
            branchRecord: MakeFragileBranchRecord(),   // fragile — gate 1 fails
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: new List<EnvironmentRecord>
            {
                MakeEnvironment("env-1", "coarse"),
                MakeEnvironment("env-2", "medium"),
            },
            scoreCard: MakePassingScorecard(),
            falsifiers: MakeEmptyFalsifiers(),
            registry: registry,
            environmentTiersCovered: new List<string> { "coarse", "medium" },
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.Single(dossier.ClaimEscalations);
        var esc = dossier.ClaimEscalations[0];
        Assert.Equal("no-change", esc.Direction);
        Assert.False(esc.AllGatesPassed);
        Assert.NotNull(esc.DemotionOrBlockReason);
        Assert.Contains(EscalationGates.BranchRobust, esc.DemotionOrBlockReason!);
    }

    [Fact]
    public void Assemble_OnlyOneEnvironmentTier_MultiEnvironmentGateFails()
    {
        var candidate = MakeCandidate("p-004", claimClass: "C2_ReproducibleMode", obsConfidence: 0.9);
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-005",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: null,
            scoreCard: MakePassingScorecard(),
            falsifiers: MakeEmptyFalsifiers(),
            registry: registry,
            environmentTiersCovered: new List<string> { "coarse" },  // only 1 tier
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.Single(dossier.ClaimEscalations);
        var esc = dossier.ClaimEscalations[0];
        Assert.Equal("no-change", esc.Direction);
        var multiEnvGate = esc.GateResults.First(g => g.GateId == EscalationGates.MultiEnvironment);
        Assert.False(multiEnvGate.Passed);
    }

    [Fact]
    public void Assemble_ZeroObservationConfidence_ObsChainGateFails()
    {
        var candidate = MakeCandidate("p-005", claimClass: "C2_ReproducibleMode", obsConfidence: 0.0);
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-006",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: null,
            scoreCard: MakePassingScorecard(),
            falsifiers: MakeEmptyFalsifiers(),
            registry: registry,
            environmentTiersCovered: new List<string> { "coarse", "medium" },
            freshness: "fresh",
            provenance: MakeProvenance());

        var esc = dossier.ClaimEscalations[0];
        var obsGate = esc.GateResults.First(g => g.GateId == EscalationGates.ObservationChainValid);
        Assert.False(obsGate.Passed);
    }

    [Fact]
    public void Assemble_NoScorecard_QuantitativeMatchGateFails()
    {
        var candidate = MakeCandidate("p-006", claimClass: "C2_ReproducibleMode", obsConfidence: 0.9);
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-007",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: MakeEmptyFalsifiers(),
            registry: registry,
            environmentTiersCovered: new List<string> { "coarse", "medium" },
            freshness: "fresh",
            provenance: MakeProvenance());

        var esc = dossier.ClaimEscalations[0];
        var qmGate = esc.GateResults.First(g => g.GateId == EscalationGates.QuantitativeMatch);
        Assert.False(qmGate.Passed);
    }

    [Fact]
    public void Assemble_CandidateAlreadyC5_NoFurtherEscalation()
    {
        var candidate = MakeCandidate("p-007", claimClass: "C5_StrongIdentificationCandidate", obsConfidence: 1.0);
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-008",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: null,
            scoreCard: MakePassingScorecard(),
            falsifiers: MakeEmptyFalsifiers(),
            registry: registry,
            environmentTiersCovered: new List<string> { "coarse", "medium" },
            freshness: "fresh",
            provenance: MakeProvenance());

        var esc = dossier.ClaimEscalations[0];
        // C5 is already max — no further escalation
        Assert.Equal("no-change", esc.Direction);
        Assert.Equal("C5_StrongIdentificationCandidate", esc.ProposedClaimClass);
    }

    [Fact]
    public void Assemble_NonConvergentFailure_ProducesNegativeResult()
    {
        var failures = new List<ConvergenceFailureRecord>
        {
            new ConvergenceFailureRecord
            {
                QuantityId = "qty-fail",
                FailureType = "non-convergent",
                Description = "oscillating values",
                ObservedValues = new double[] { 1.0, 2.0, 1.5, 2.5 },
                MeshParameters = new double[] { 0.5, 0.25, 0.125, 0.0625 },
            },
        };

        var dossier = _assembler.Assemble(
            studyId: "study-009",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: failures,
            environments: null,
            scoreCard: null,
            falsifiers: null,
            registry: null,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "stale",
            provenance: MakeProvenance());

        Assert.Single(dossier.NegativeResults);
        var nr = dossier.NegativeResults[0];
        Assert.Equal("non-convergence", nr.Category);
        Assert.Contains("qty-fail", nr.Description);
        Assert.False(nr.ImpliesDemotion);
        Assert.Equal("flag-for-review", nr.RecommendedAction);
    }

    [Fact]
    public void Assemble_FatalFalsifierWithoutRegistry_ProducesNegativeResult()
    {
        var falsifiers = new FalsifierSummary
        {
            StudyId = "fs-study",
            Falsifiers = new List<FalsifierRecord>
            {
                new FalsifierRecord
                {
                    FalsifierId = "f-002",
                    FalsifierType = FalsifierTypes.QuantitativeMismatch,
                    Severity = FalsifierSeverity.Fatal,
                    TargetId = "p-unknown",
                    BranchId = "test-branch",
                    Description = "Mismatch fatal",
                    Evidence = "pull=12",
                    Active = true,
                    Provenance = MakeProvenance(),
                },
            },
            ActiveFatalCount = 1,
            ActiveHighCount = 0,
            TotalActiveCount = 1,
            Provenance = MakeProvenance(),
        };

        var dossier = _assembler.Assemble(
            studyId: "study-010",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: falsifiers,
            registry: null,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.Single(dossier.NegativeResults);
        var nr = dossier.NegativeResults[0];
        Assert.True(nr.ImpliesDemotion);
        Assert.Equal("demote-candidate", nr.RecommendedAction);
        Assert.Equal(FalsifierTypes.QuantitativeMismatch, nr.Category);
    }

    [Fact]
    public void Assemble_MultipleNegativeResults_AllCollected()
    {
        var failures = new List<ConvergenceFailureRecord>
        {
            new ConvergenceFailureRecord
            {
                QuantityId = "qty-a",
                FailureType = "non-convergent",
                Description = "oscillating",
                ObservedValues = Array.Empty<double>(),
                MeshParameters = Array.Empty<double>(),
            },
            new ConvergenceFailureRecord
            {
                QuantityId = "qty-b",
                FailureType = "non-convergent",
                Description = "diverging",
                ObservedValues = Array.Empty<double>(),
                MeshParameters = Array.Empty<double>(),
            },
        };

        var falsifiers = new FalsifierSummary
        {
            StudyId = "fs-study",
            Falsifiers = new List<FalsifierRecord>
            {
                new FalsifierRecord
                {
                    FalsifierId = "f-003",
                    FalsifierType = FalsifierTypes.BranchFragility,
                    Severity = FalsifierSeverity.Fatal,
                    TargetId = "p-x",
                    BranchId = "test-branch",
                    Description = "Branch fragile",
                    Evidence = "fragility=0.95",
                    Active = true,
                    Provenance = MakeProvenance(),
                },
            },
            ActiveFatalCount = 1,
            ActiveHighCount = 0,
            TotalActiveCount = 1,
            Provenance = MakeProvenance(),
        };

        var dossier = _assembler.Assemble(
            studyId: "study-011",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: failures,
            environments: null,
            scoreCard: null,
            falsifiers: falsifiers,
            registry: null,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "fresh",
            provenance: MakeProvenance());

        // 2 non-convergent + 1 fatal falsifier = 3 negative results
        Assert.Equal(3, dossier.NegativeResults.Count);
        Assert.All(dossier.NegativeResults, nr => Assert.Matches(@"neg-\d{4}", nr.EntryId));
    }

    [Fact]
    public void Assemble_SixGatesAllEvaluated_PerCandidate()
    {
        var candidate = MakeCandidate("p-009", claimClass: "C1_NumericalHint", obsConfidence: 0.5);
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-012",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: null,
            registry: registry,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.Single(dossier.ClaimEscalations);
        var esc = dossier.ClaimEscalations[0];
        // All 6 gates should be present
        Assert.Equal(6, esc.GateResults.Count);
        Assert.Contains(esc.GateResults, g => g.GateId == EscalationGates.BranchRobust);
        Assert.Contains(esc.GateResults, g => g.GateId == EscalationGates.RefinementBounded);
        Assert.Contains(esc.GateResults, g => g.GateId == EscalationGates.MultiEnvironment);
        Assert.Contains(esc.GateResults, g => g.GateId == EscalationGates.ObservationChainValid);
        Assert.Contains(esc.GateResults, g => g.GateId == EscalationGates.QuantitativeMatch);
        Assert.Contains(esc.GateResults, g => g.GateId == EscalationGates.NoActiveFatalFalsifier);
    }

    [Fact]
    public void Assemble_DossierMetadata_Correct()
    {
        var before = DateTimeOffset.UtcNow;
        var dossier = _assembler.Assemble(
            studyId: "study-meta",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: null,
            registry: null,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "regenerated-current-code",
            provenance: MakeProvenance());
        var after = DateTimeOffset.UtcNow;

        Assert.Equal("dossier-study-meta", dossier.DossierId);
        Assert.Equal("1.0.0", dossier.SchemaVersion);
        Assert.Equal("regenerated-current-code", dossier.Freshness);
        Assert.True(dossier.GeneratedAt >= before);
        Assert.True(dossier.GeneratedAt <= after);
    }

    [Fact]
    public void Assemble_MultipleCandidates_EachGetsEscalationRecord()
    {
        var registry = MakeRegistry(
            MakeCandidate("p-a", claimClass: "C2_ReproducibleMode", obsConfidence: 0.7),
            MakeCandidate("p-b", claimClass: "C1_NumericalHint", obsConfidence: 0.3),
            MakeCandidate("p-c", claimClass: "C3_BranchStableCandidate", obsConfidence: 0.9));

        var dossier = _assembler.Assemble(
            studyId: "study-multi",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: null,
            registry: registry,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.Equal(3, dossier.ClaimEscalations.Count);
        var ids = dossier.ClaimEscalations.Select(e => e.CandidateId).ToList();
        Assert.Contains("p-a", ids);
        Assert.Contains("p-b", ids);
        Assert.Contains("p-c", ids);
    }

    [Fact]
    public void Assemble_CandidateSpecificQuantitativeJoin_DoesNotLeakAcrossCandidates()
    {
        var registry = MakeRegistry(
            MakeCandidate("p-pass", claimClass: "C2_ReproducibleMode"),
            MakeCandidate("p-block", claimClass: "C2_ReproducibleMode"));

        var dossier = _assembler.Assemble(
            studyId: "study-candidate-specific",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: null,
            scoreCard: MakePassingScorecard(),
            falsifiers: MakeEmptyFalsifiers(),
            registry: registry,
            environmentTiersCovered: ["toy", "structured"],
            freshness: "fresh",
            provenance: MakeProvenance(),
            observationChainRecords:
            [
                MakeObsChainRecord("p-pass", "src-p-pass"),
                new ObservationChainRecord
                {
                    CandidateId = "p-block",
                    PrimarySourceId = "src-p-block",
                    ObservableId = "obs-unmatched",
                    CompletenessStatus = "complete",
                    SensitivityScore = 0.1,
                    AuxiliaryModelSensitivity = 0.08,
                    Passed = true,
                    Provenance = MakeProvenance(),
                },
            ],
            environmentVarianceRecords: MakeEnvironmentVariance());

        var passingCandidate = dossier.ClaimEscalations.Single(e => e.CandidateId == "p-pass");
        var blockedCandidate = dossier.ClaimEscalations.Single(e => e.CandidateId == "p-block");

        Assert.True(passingCandidate.GateResults.Single(g => g.GateId == EscalationGates.QuantitativeMatch).Passed);
        Assert.False(blockedCandidate.GateResults.Single(g => g.GateId == EscalationGates.QuantitativeMatch).Passed);
        Assert.NotEmpty(passingCandidate.GateResults.Single(g => g.GateId == EscalationGates.QuantitativeMatch).EvidenceRecordIds!);
        Assert.Empty(blockedCandidate.GateResults.Single(g => g.GateId == EscalationGates.QuantitativeMatch).EvidenceRecordIds!);
    }

    [Fact]
    public void Assemble_NullArgValidation_ThrowsForRequiredParams()
    {
        var prov = MakeProvenance();
        Assert.Throws<ArgumentNullException>(() =>
            _assembler.Assemble(null!, null, null, null, null, null, null, null,
                Array.Empty<string>(), "fresh", prov));
        Assert.Throws<ArgumentNullException>(() =>
            _assembler.Assemble("sid", null, null, null, null, null, null, null,
                null!, "fresh", prov));
        Assert.Throws<ArgumentNullException>(() =>
            _assembler.Assemble("sid", null, null, null, null, null, null, null,
                Array.Empty<string>(), "fresh", null!));
    }

    [Fact]
    public void Phase5ValidationDossier_HasFatalFalsifier_TrueWhenFatalActive()
    {
        var candidate = MakeCandidate("p-x");
        var registry = MakeRegistry(candidate);

        var dossier = _assembler.Assemble(
            studyId: "study-fatal",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: MakeFatalFalsifier("p-x"),
            registry: registry,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "fresh",
            provenance: MakeProvenance());

        Assert.True(dossier.HasFatalFalsifier);
    }

    // ─── WP-6: ObservationChainRecord tests ───

    private static ObservationChainRecord MakeObsChainRecord(
        string candidateId,
        string primarySourceId,
        string completenessStatus = "complete",
        double sensitivityScore = 0.1,
        double auxModelSensitivity = 0.08,
        bool passed = true) => new ObservationChainRecord
    {
        CandidateId = candidateId,
        PrimarySourceId = primarySourceId,
        ObservableId = "obs-test-1",
        CompletenessStatus = completenessStatus,
        SensitivityScore = sensitivityScore,
        AuxiliaryModelSensitivity = auxModelSensitivity,
        Passed = passed,
        Provenance = MakeProvenance(),
    };

    [Fact]
    public void ObservationChainGate_NoRecord_Fails()
    {
        // Gate 4 fails when no observation chain records are provided
        var candidate = MakeCandidate("p-obs-none", obsConfidence: 0.0); // obsConfidence=0 → gate fails
        var registry = MakeRegistry([candidate]);
        var assembler = new Phase5DossierAssembler();

        var dossier = assembler.Assemble(
            studyId: "obs-none-test",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: null,
            registry: registry,
            environmentTiersCovered: ["env-toy", "env-structured"],
            freshness: "regenerated",
            provenance: MakeProvenance(),
            observationChainRecords: []);  // empty list — gate should fail

        Assert.Single(dossier.ClaimEscalations);
        var escalation = dossier.ClaimEscalations[0];
        var obsGate = escalation.GateResults.Single(g => g.GateId == EscalationGates.ObservationChainValid);
        Assert.False(obsGate.Passed);
    }

    [Fact]
    public void ObservationChainGate_CompleteLowSensitivityRecord_Passes()
    {
        // Gate 4 passes when a complete, low-sensitivity, passed record exists for the candidate
        var candidate = MakeCandidate("p-obs-good");
        var registry = MakeRegistry([candidate]);
        var obsRecord = MakeObsChainRecord(
            candidateId: "p-obs-good",
            primarySourceId: "src-p-obs-good",
            completenessStatus: "complete",
            sensitivityScore: 0.12,
            auxModelSensitivity: 0.08,
            passed: true);

        var assembler = new Phase5DossierAssembler();
        var dossier = assembler.Assemble(
            studyId: "obs-good-test",
            branchRecord: MakeRobustBranchRecord(),
            convergenceRecords: MakeBoundedConvergence(),
            convergenceFailures: null,
            environments: null,
            scoreCard: MakePassingScorecard(),
            falsifiers: null,
            registry: registry,
            environmentTiersCovered: ["env-toy", "env-structured"],
            freshness: "regenerated",
            provenance: MakeProvenance(),
            observationChainRecords: [obsRecord]);

        Assert.Single(dossier.ClaimEscalations);
        var escalation = dossier.ClaimEscalations[0];
        var obsGate = escalation.GateResults.Single(g => g.GateId == EscalationGates.ObservationChainValid);
        Assert.True(obsGate.Passed);

        // Observation chain is stored in the dossier
        Assert.NotNull(dossier.ObservationChainSummary);
        Assert.Single(dossier.ObservationChainSummary!);
    }

    [Fact]
    public void ObservationChainGate_HighSensitivityRecord_Fails()
    {
        // Gate 4 fails when sensitivityScore > 0.3
        var candidate = MakeCandidate("p-obs-sensitive");
        var registry = MakeRegistry([candidate]);
        var obsRecord = MakeObsChainRecord(
            candidateId: "p-obs-sensitive",
            primarySourceId: "src-p-obs-sensitive",
            completenessStatus: "complete",
            sensitivityScore: 0.44,   // exceeds 0.3 threshold
            auxModelSensitivity: 0.35, // exceeds 0.3 threshold
            passed: false);

        var assembler = new Phase5DossierAssembler();
        var dossier = assembler.Assemble(
            studyId: "obs-sensitive-test",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: null,
            registry: registry,
            environmentTiersCovered: ["env-toy"],
            freshness: "regenerated",
            provenance: MakeProvenance(),
            observationChainRecords: [obsRecord]);

        Assert.Single(dossier.ClaimEscalations);
        var escalation = dossier.ClaimEscalations[0];
        var obsGate = escalation.GateResults.Single(g => g.GateId == EscalationGates.ObservationChainValid);
        Assert.False(obsGate.Passed);
    }
}

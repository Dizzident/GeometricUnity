using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase526_a5_certificate_reducer_001";
const string ContractPath = Root + "/preregistration/phase526_a5_certificate_reducer_contract_v1.json";
const string OutputPath = Root + "/output/a5_certificate_reducer.json";
const string SummaryPath = Root + "/output/a5_certificate_reducer_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;

Binding[] bindings = contract.GetProperty("exactBindings").EnumerateArray().Select(row =>
{
    string id = row.GetProperty("id").GetString()!;
    string path = row.GetProperty("path").GetString()!;
    string expected = row.GetProperty("sha256").GetString()!;
    string actual = File.Exists(path) ? Sha256(path) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}).ToArray();
var expectedBindingInventory = new (string Id, string Path)[]
{
    ("phase520-summary", "studies/phase520_a5_action_subject_lineage_parity_audit_001/output/a5_action_subject_lineage_parity_audit_summary.json"),
    ("phase520-full", "studies/phase520_a5_action_subject_lineage_parity_audit_001/output/a5_action_subject_lineage_parity_audit.json"),
    ("phase520-contract", "studies/phase520_a5_action_subject_lineage_parity_audit_001/preregistration/phase520_a5_action_subject_lineage_parity_contract_v1.json"),
    ("phase520-program", "studies/phase520_a5_action_subject_lineage_parity_audit_001/Program.cs"),
    ("phase521-summary", "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/output/a5_frozen_reflection_compatible_triangulation_census_summary.json"),
    ("phase521-full", "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/output/a5_frozen_reflection_compatible_triangulation_census.json"),
    ("phase521-contract", "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/preregistration/phase521_frozen_reflection_compatible_triangulation_census_contract_v1.json"),
    ("phase521-program", "studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/Program.cs"),
    ("phase522-summary", "studies/phase522_a5_foundation_candidate_reduction_001/output/a5_foundation_candidate_reduction_summary.json"),
    ("phase522-full", "studies/phase522_a5_foundation_candidate_reduction_001/output/a5_foundation_candidate_reduction.json"),
    ("phase522-contract", "studies/phase522_a5_foundation_candidate_reduction_001/preregistration/phase522_a5_foundation_candidate_reduction_contract_v1.json"),
    ("phase522-program", "studies/phase522_a5_foundation_candidate_reduction_001/Program.cs"),
    ("phase523-summary", "studies/phase523_a5_action_member_universalization_audit_001/output/a5_action_member_universalization_audit_summary.json"),
    ("phase523-full", "studies/phase523_a5_action_member_universalization_audit_001/output/a5_action_member_universalization_audit.json"),
    ("phase523-contract", "studies/phase523_a5_action_member_universalization_audit_001/preregistration/phase523_a5_action_member_universalization_contract_v1.json"),
    ("phase523-program", "studies/phase523_a5_action_member_universalization_audit_001/Program.cs"),
    ("phase524-summary", "studies/phase524_a5_exact_omega_parity_decomposition_001/output/a5_exact_omega_parity_decomposition_summary.json"),
    ("phase524-full", "studies/phase524_a5_exact_omega_parity_decomposition_001/output/a5_exact_omega_parity_decomposition.json"),
    ("phase524-contract", "studies/phase524_a5_exact_omega_parity_decomposition_001/preregistration/phase524_a5_exact_omega_parity_decomposition_contract_v1.json"),
    ("phase524-program", "studies/phase524_a5_exact_omega_parity_decomposition_001/Program.cs"),
    ("phase525-summary", "studies/phase525_a5_survivor_reflection_pullback_boundary_audit_001/output/a5_survivor_reflection_pullback_boundary_audit_summary.json"),
    ("phase525-full", "studies/phase525_a5_survivor_reflection_pullback_boundary_audit_001/output/a5_survivor_reflection_pullback_boundary_audit.json"),
    ("phase525-contract", "studies/phase525_a5_survivor_reflection_pullback_boundary_audit_001/preregistration/phase525_survivor_reflection_pullback_boundary_contract_v1.json"),
    ("phase525-program", "studies/phase525_a5_survivor_reflection_pullback_boundary_audit_001/Program.cs"),
};
bool exactBindingInventoryValid = bindings.Length == expectedBindingInventory.Length
    && bindings.Select(x => (x.Id, x.Path)).SequenceEqual(expectedBindingInventory)
    && bindings.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == bindings.Length
    && bindings.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == bindings.Length;

var documents = new List<JsonDocument>();
JsonElement Load(string id)
{
    JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == id).Path));
    documents.Add(document);
    return document.RootElement;
}

try
{
    string[] precedence = contract.GetProperty("terminalPrecedence").EnumerateArray()
        .Select(x => x.GetString()!).ToArray();
    string[] expectedPrecedence =
    {
        "invalid-or-drifted-input",
        "unresolved-member-dependence-for-named-a5-prerequisites",
        "exact-omega-parity-refuted-or-unresolved",
        "no-survivor-with-complete-finite-pullback-boundary-closure",
        "o1-o2-dependency-or-positive-time-algebra-unresolved",
        "normalized-measure-or-domain-incomplete",
        "candidate-to-target-equality-or-hermiticity-unproved",
        "finite-only",
        "necessity-gluing-or-all-scope-unproved",
        "candidate-package-ready-for-independent-mathematical-review",
    };

    bool contractValid =
        contract.GetProperty("contractId").GetString() == "phase526-a19-dependent-a5-certificate-reducer-v1"
        && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A19"
        && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
        && contract.GetProperty("expectedCurrentVerdict").GetString() == "unresolved-member-dependence-for-named-a5-prerequisites"
        && contract.GetProperty("strongestPermittedTerminal").GetString() == "candidate-package-ready-for-independent-mathematical-review"
        && contract.GetProperty("strongestPermittedClaim").GetString() == "independent-mathematical-review-readiness-only"
        && precedence.SequenceEqual(expectedPrecedence, StringComparer.Ordinal)
        && exactBindingInventoryValid
        && contract.GetProperty("externalReviewPending").GetBoolean()
        && !contract.GetProperty("allExecutionAndPromotionAuthorities").GetBoolean()
        && !contract.GetProperty("selectionRegistrationTheoremCounterexampleAuthority").GetBoolean()
        && !contract.GetProperty("reflectionPositivityAuthority").GetBoolean()
        && !contract.GetProperty("phase458G1OrL8Authority").GetBoolean()
        && !contract.GetProperty("phase515Or516UnlockAuthority").GetBoolean()
        && !contract.GetProperty("phase481WorkAuthority").GetBoolean()
        && !contract.GetProperty("phase458EvaluationAuthority").GetBoolean()
        && !contract.GetProperty("sourceContractApplicationAuthority").GetBoolean()
        && !contract.GetProperty("o4OrHumanRulingAuthority").GetBoolean()
        && !contract.GetProperty("physicalTargetTuningAuthority").GetBoolean()
        && !contract.GetProperty("physicalUnitOrGevClaimAuthority").GetBoolean()
        && !contract.GetProperty("samplingHmcReprocessingBenchmarkAuthority").GetBoolean()
        && !contract.GetProperty("productionAllocationOrProductionAuthority").GetBoolean()
        && !contract.GetProperty("launchOrAccelerationAuthority").GetBoolean()
        && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
    bool exactBindingsValid = bindings.All(x => x.HashMatches);

    JsonElement p520 = Load("phase520-summary");
    JsonElement p521 = Load("phase521-summary");
    JsonElement p522 = Load("phase522-summary");
    JsonElement p523 = Load("phase523-summary");
    JsonElement p524 = Load("phase524-summary");
    JsonElement p525 = Load("phase525-summary");

    bool package520To524IdentityValid = new[]
    {
        (Document: p520, Phase: 520, Verdict: "action-member-unresolved"),
        (Document: p521, Phase: 521, Verdict: "finite-dual-reflection-compatible-candidate-survives"),
        (Document: p522, Phase: 522, Verdict: "action-member-or-omega-parity-unresolved"),
        (Document: p523, Phase: 523, Verdict: "universalization-partial-member-dependence-remains"),
        (Document: p524, Phase: 524, Verdict: "exact-omega-parity-refuted-identity-member"),
    }.All(row => row.Document.GetProperty("phase").GetInt32() == row.Phase
        && row.Document.GetProperty("inputsValid").GetBoolean()
        && row.Document.GetProperty("contractValid").GetBoolean()
        && row.Document.GetProperty("verdictKind").GetString() == row.Verdict);
    string[] allowedPhase525Verdicts =
    {
        "no-survivor-complete-finite-pullback-boundary",
        "partial-survivor-finite-pullback-boundary",
        "finite-dual-survivor-pullback-boundary-closed",
    };
    bool phase525IdentityValid = p525.GetProperty("phase").GetInt32() == 525
        && p525.GetProperty("inputsValid").GetBoolean()
        && p525.GetProperty("contractValid").GetBoolean()
        && allowedPhase525Verdicts.Contains(p525.GetProperty("verdictKind").GetString(), StringComparer.Ordinal)
        && p525.GetProperty("finiteOnly").GetBoolean()
        && p525.GetProperty("positiveResultScope").GetString() == "finite-combinatorial-chain-and-carrier-closure-only"
        && !p525.GetProperty("actionCovarianceEvaluated").GetBoolean()
        && !p525.GetProperty("actionCarrierTransformationEstablished").GetBoolean()
        && Phase525AuthorityFirewallsValid(p525.GetProperty("authorityFirewalls"));

    bool fullSummaryPairsIdentical = Enumerable.Range(520, 6).All(phase =>
        bindings.Single(x => x.Id == $"phase{phase}-full").ActualSha256
            == bindings.Single(x => x.Id == $"phase{phase}-summary").ActualSha256);

    JsonElement p523Inventory = p523.GetProperty("prerequisiteInventory");
    JsonElement p523Universalization = p523.GetProperty("universalizationAudit");
    string[] expectedPrerequisiteKeys =
    {
        "kinematic-carrier-and-complete-family-domain", "executable-action-subject-binding",
        "complete-action-functional-coverage", "exact-omega-parity", "composite-correlation-transport",
        "reflection-positivity-for-unchanged-action", "face-local-interaction-factorization",
        "o1-o2-dependency-and-positive-time-membership",
        "normalized-measure-domain-target-equality-hermiticity", "necessity-gluing-and-all-scope",
    };
    JsonElement[] prerequisiteRows = p523Inventory.GetProperty("rows").EnumerateArray().ToArray();
    bool prerequisiteRowKeysExact = prerequisiteRows.Length == expectedPrerequisiteKeys.Length
        && prerequisiteRows.Select(row => row.GetProperty("PrerequisiteId").GetString())
            .SequenceEqual(expectedPrerequisiteKeys, StringComparer.Ordinal)
        && prerequisiteRows.Select(row => row.GetProperty("PrerequisiteId").GetString())
            .Distinct(StringComparer.Ordinal).Count() == expectedPrerequisiteKeys.Length;
    int namedPrerequisiteCount = prerequisiteRows.Length;
    int invariantPrerequisiteCount = prerequisiteRows.Count(row => row.GetProperty("Classification").GetString() == "member-invariant");
    int memberDependentPrerequisiteCount = prerequisiteRows.Count(row => row.GetProperty("Classification").GetString() == "member-dependent");
    int unresolvedPrerequisiteCount = prerequisiteRows.Count(row => row.GetProperty("Classification").GetString() == "unresolved");
    bool perRowSemanticsValid = prerequisiteRowKeysExact && prerequisiteRows.Select((row, index) =>
    {
        string identityFingerprint = row.GetProperty("IdentitySemanticFingerprint").GetString()!;
        string sd2Fingerprint = row.GetProperty("Sd2SemanticFingerprint").GetString()!;
        bool expectedInvariant = index == 0;
        return row.GetProperty("SemanticEvidenceResolved").GetBoolean()
            && !row.GetProperty("PrerequisiteEstablished").GetBoolean()
            && row.GetProperty("FullScopeIdentical").GetBoolean() == expectedInvariant
            && row.GetProperty("UniversalizationRemovesMemberSelectionDependency").GetBoolean() == expectedInvariant
            && row.GetProperty("Classification").GetString() == (expectedInvariant ? "member-invariant" : "member-dependent")
            && (identityFingerprint == sd2Fingerprint) == expectedInvariant
            && identityFingerprint.Length > 0 && sd2Fingerprint.Length > 0;
    }).All(x => x);
    bool memberDependenceUnresolved = p523Inventory.GetProperty("registeredFiniteInventory").GetBoolean()
        && p523Inventory.GetProperty("registeredInventorySchemaValid").GetBoolean()
        && p523Inventory.GetProperty("registeredInventoryCoverageComplete").GetBoolean()
        && !p523Inventory.GetProperty("exhaustiveMathematicalCompletenessClaimed").GetBoolean()
        && !p523Inventory.GetProperty("classificationsDerivedFromContractDependencyDimensions").GetBoolean()
        && perRowSemanticsValid
        && namedPrerequisiteCount == 10
        && invariantPrerequisiteCount == 1
        && memberDependentPrerequisiteCount == 9
        && unresolvedPrerequisiteCount == 0
        && p523Inventory.GetProperty("count").GetInt32() == namedPrerequisiteCount
        && p523Inventory.GetProperty("invariantCount").GetInt32() == invariantPrerequisiteCount
        && p523Inventory.GetProperty("memberDependentCount").GetInt32() == memberDependentPrerequisiteCount
        && p523Inventory.GetProperty("unresolvedCount").GetInt32() == unresolvedPrerequisiteCount
        && p523Universalization.GetProperty("partialUniversalization").GetBoolean()
        && !p523Universalization.GetProperty("fullActionWideEquivalenceEstablished").GetBoolean()
        && !p523Universalization.GetProperty("prerequisiteSatisfactionEstablished").GetBoolean();

    JsonElement[] memberDecompositions = p524.GetProperty("memberDecompositions").EnumerateArray().ToArray();
    JsonElement identityParity = memberDecompositions.Single(x => x.GetProperty("memberId").GetString() == "identity");
    JsonElement sd2Parity = memberDecompositions.Single(x => x.GetProperty("memberId").GetString() == "sd2-id0/c0.5");
    JsonElement identityWitness = p524.GetProperty("identityExactWitness");
    bool identityParityRefuted =
        p524.GetProperty("sourceFormulaCoverageComplete").GetBoolean()
        && p524.GetProperty("termRequirementsFrozenAndComplete").GetBoolean()
        && p524.GetProperty("precedenceBatteryPassed").GetBoolean()
        && identityParity.GetProperty("completeExecutableFormulaCoverage").GetBoolean()
        && identityParity.GetProperty("exactOddCancellationResolved").GetBoolean()
        && !identityParity.GetProperty("identityExactlyEven").GetBoolean()
        && identityParity.GetProperty("omegaParityStatus").GetString() == "refuted-by-full-finite-action-exact-witness"
        && identityWitness.GetProperty("identityOddTermSurvives").GetBoolean()
        && identityWitness.GetProperty("totalLdotW").GetInt32() == 1
        && identityWitness.GetProperty("actionAtPlusMinusDifferenceNumerator").GetInt32() == 1
        && identityWitness.GetProperty("actionAtPlusMinusDifferenceDenominator").GetInt32() == 1
        && identityWitness.GetProperty("semanticReductionDependency").GetString()!.Length > 0;
    bool sd2ParityUnresolved =
        sd2Parity.GetProperty("completeExecutableFormulaCoverage").GetBoolean()
        && !sd2Parity.GetProperty("exactExecutableCoefficients").GetBoolean()
        && !sd2Parity.GetProperty("exactOddCancellationResolved").GetBoolean()
        && sd2Parity.GetProperty("omegaParityStatus").GetString()
            == "unresolved-non-exact-coefficients-and-no-global-cancellation-identity";
    bool exactOmegaParityRefutedOrUnresolved = identityParityRefuted || sd2ParityUnresolved;
    int actionDifferenceNumerator = identityWitness.GetProperty("actionAtPlusMinusDifferenceNumerator").GetInt32();
    int actionDifferenceDenominator = identityWitness.GetProperty("actionAtPlusMinusDifferenceDenominator").GetInt32();
    string actionDifference = actionDifferenceDenominator == 1
        ? actionDifferenceNumerator.ToString(System.Globalization.CultureInfo.InvariantCulture)
        : $"{actionDifferenceNumerator}/{actionDifferenceDenominator}";

    JsonElement[] pairAudits = p525.GetProperty("pairAudits").EnumerateArray().ToArray();
    string[] sourceSurvivorKeys = p522.GetProperty("candidateReduction").GetProperty("totalCompatibleCandidates")
        .EnumerateArray().Select(x => x.GetString()!).Order(StringComparer.Ordinal).ToArray();
    string[] pairKeys = pairAudits.Select(pair => pair.GetProperty("candidateId").GetString()!).Order(StringComparer.Ordinal).ToArray();
    bool pairMenuLineageExact = sourceSurvivorKeys.SequenceEqual(new[]
        {
            "periodic-cubical-cell-poset-barycentric::link-centered-axis0",
            "periodic-cubical-cell-poset-barycentric::site-centered-axis0",
        }, StringComparer.Ordinal)
        && pairKeys.SequenceEqual(sourceSurvivorKeys, StringComparer.Ordinal)
        && pairKeys.Distinct(StringComparer.Ordinal).Count() == pairKeys.Length
        && pairAudits.All(pair => pair.GetProperty("candidateId").GetString()!.EndsWith(
            "::" + pair.GetProperty("reflectionId").GetString(), StringComparison.Ordinal))
        && pairAudits.All(pair => pair.GetProperty("extentAudits").EnumerateArray()
            .Select(extent => extent.GetProperty("extent").GetInt32()).SequenceEqual(new[] { 3, 4 }))
        && p525.GetProperty("precedenceBatteryPassed").GetBoolean();
    JsonElement[] survivingPairAudits = pairAudits.Where(pair =>
        pair.GetProperty("completeFinitePullbackBoundary").GetBoolean()
        && pair.GetProperty("extentAudits").EnumerateArray().All(extent =>
            extent.GetProperty("completeFinitePullbackBoundary").GetBoolean())).ToArray();
    int completeFinitePairCount = survivingPairAudits.Length;
    int failedFinitePairCount = pairAudits.Length - completeFinitePairCount;
    bool twoOfTwoFiniteClosure = completeFinitePairCount == sourceSurvivorKeys.Length;
    bool partialFiniteClosure = completeFinitePairCount > 0 && !twoOfTwoFiniteClosure;
    bool noCompleteFiniteSurvivor = completeFinitePairCount == 0;
    (string derivedPhase525Verdict, bool derivedPhase525PositiveClosure) =
        DerivePhase525State(completeFinitePairCount, pairAudits.Length);
    bool phase525ReportedStateConsistent = Phase525StateConsistent(
        completeFinitePairCount,
        pairAudits.Length,
        p525.GetProperty("verdictKind").GetString()!,
        p525.GetProperty("completePairCount").GetInt32(),
        p525.GetProperty("failedPairCount").GetInt32(),
        p525.GetProperty("finiteDualSurvivorPullbackBoundaryClosed").GetBoolean(),
        p525.GetProperty("finiteCombinatorialChainAndCarrierClosure").GetBoolean());
    var phase525ConsistencyBattery = new[]
    {
        Phase525ConsistencyCase("consistent-zero", 0, 2, "no-survivor-complete-finite-pullback-boundary", 0, 2, false, false, true),
        Phase525ConsistencyCase("consistent-partial", 1, 2, "partial-survivor-finite-pullback-boundary", 1, 1, false, false, true),
        Phase525ConsistencyCase("consistent-all", 2, 2, "finite-dual-survivor-pullback-boundary-closed", 2, 0, true, true, true),
        Phase525ConsistencyCase("mutated-verdict", 2, 2, "partial-survivor-finite-pullback-boundary", 2, 0, true, true, false),
        Phase525ConsistencyCase("mutated-count", 2, 2, "finite-dual-survivor-pullback-boundary-closed", 1, 1, true, true, false),
        Phase525ConsistencyCase("mutated-positive-flag", 2, 2, "finite-dual-survivor-pullback-boundary-closed", 2, 0, false, true, false),
    };
    bool phase525ConsistencyBatteryPassed = phase525ConsistencyBattery.All(x => x.Passed);

    JsonElement p522Blockers = p522.GetProperty("blockers");
    bool o1O2OrPositiveTimeUnresolved = p522Blockers.GetProperty("o1O2DependencyOrPositiveTimeAlgebraUnresolved").GetBoolean();
    bool measureOrDomainIncomplete = p522Blockers.GetProperty("normalizedMeasureOrDomainIncomplete").GetBoolean();
    bool targetEqualityOrHermiticityUnproved = p522Blockers.GetProperty("candidateTargetEqualityOrHermiticityUnproved").GetBoolean();
    bool finiteOnly = p525.GetProperty("finiteOnly").GetBoolean();
    bool necessityGluingOrAllScopeUnproved = p522Blockers.GetProperty("necessityGluingOrAllScopeUnproved").GetBoolean();

    bool precursorSemanticsValid = package520To524IdentityValid && phase525IdentityValid && fullSummaryPairsIdentical
        && memberDependenceUnresolved && identityParityRefuted && sd2ParityUnresolved
        && pairMenuLineageExact && phase525ReportedStateConsistent && phase525ConsistencyBatteryPassed;
    bool invalidOrDrifted = !contractValid || !exactBindingsValid || !precursorSemanticsValid;
    bool[] blockers =
    {
        invalidOrDrifted,
        memberDependenceUnresolved,
        exactOmegaParityRefutedOrUnresolved,
        noCompleteFiniteSurvivor,
        o1O2OrPositiveTimeUnresolved,
        measureOrDomainIncomplete,
        targetEqualityOrHermiticityUnproved,
        finiteOnly,
        necessityGluingOrAllScopeUnproved,
    };
    string verdict = Evaluate(blockers, precedence);

    PrecedenceBatteryRow[] precedenceBattery = Enumerable.Range(0, precedence.Length).Select(index =>
    {
        bool[] synthetic = new bool[precedence.Length - 1];
        if (index < synthetic.Length)
            synthetic[index] = true;
        string actual = Evaluate(synthetic, precedence);
        return new PrecedenceBatteryRow(index, precedence[index], actual, actual == precedence[index]);
    }).ToArray();
    bool precedenceBatteryPassed = precedenceBattery.All(row => row.Passed);
    invalidOrDrifted = invalidOrDrifted || !precedenceBatteryPassed;
    blockers[0] = invalidOrDrifted;
    verdict = Evaluate(blockers, precedence);
    bool expectedCurrentVerdictMatched = verdict == contract.GetProperty("expectedCurrentVerdict").GetString();
    bool reviewReady = verdict == "candidate-package-ready-for-independent-mathematical-review";

    var result = new
    {
        schemaVersion = 1,
        phase = 526,
        phaseId = "phase526-a5-certificate-reducer",
        terminalStatus = "a5-certificate-reducer-" + verdict,
        verdictKind = verdict,
        inputsValid = !invalidOrDrifted,
        contractValid,
        exactBindingsValid,
        precursorSemanticsValid,
        applicationSubjectKind = "dependent-a5-certificate-reducer",
        planSection = "WAVE2_AMENDMENTS_2026-07-12 A19",
        deterministicZeroSampling = true,
        targetBlindConstruction = true,
        physicalTargetsConsultedForConstruction = false,
        contract = new
        {
            path = ContractPath,
            sha256 = Sha256(ContractPath),
            frozenBeforePrecursorConsumption = true,
        },
        exactInputBindings = bindings,
        exactBindingInventoryValid,
        fullSummaryPairsIdentical,
        precedence,
        precedenceBattery,
        precedenceBatteryPassed,
        expectedCurrentVerdictMatched,
        phase525ConsistencyBattery,
        phase525ConsistencyBatteryPassed,
        blockerAudit = new
        {
            invalidOrDrifted,
            unresolvedMemberDependenceForNamedA5Prerequisites = memberDependenceUnresolved,
            exactOmegaParityRefutedOrUnresolved,
            noSurvivorWithCompleteFinitePullbackBoundaryClosure = noCompleteFiniteSurvivor,
            o1O2DependencyOrPositiveTimeAlgebraUnresolved = o1O2OrPositiveTimeUnresolved,
            normalizedMeasureOrDomainIncomplete = measureOrDomainIncomplete,
            candidateToTargetEqualityOrHermiticityUnproved = targetEqualityOrHermiticityUnproved,
            finiteOnly,
            necessityGluingOrAllScopeUnproved,
        },
        memberDependenceReduction = new
        {
            namedPrerequisiteCount,
            invariantPrerequisiteCount,
            memberDependentPrerequisiteCount,
            universalizationRemovesSelectionForSharedScopeOnly = true,
            memberSensitiveObligationsSeparatelyDischarged = false,
            actionWideEquivalenceEstablished = false,
            finding = "Phase523 leaves nine of ten named A5 prerequisites member-dependent. The reducer therefore preserves the earliest member-dependence terminal.",
        },
        omegaParityReduction = new
        {
            identityMember = new
            {
                status = identityParity.GetProperty("omegaParityStatus").GetString(),
                decisiveExactRefutation = identityParityRefuted,
                witnessScope = identityWitness.GetProperty("scope").GetString(),
                totalLdotW = identityWitness.GetProperty("totalLdotW").GetInt32(),
                actionPlusMinusDifference = actionDifference,
                actionPlusMinusDifferenceNumerator = actionDifferenceNumerator,
                actionPlusMinusDifferenceDenominator = actionDifferenceDenominator,
            },
            sd2Member = new
            {
                status = sd2Parity.GetProperty("omegaParityStatus").GetString(),
                unresolved = sd2ParityUnresolved,
            },
            actionMemberSelected = false,
            refutationScopePromotedBeyondIdentityMember = false,
        },
        finitePullbackBoundaryReduction = new
        {
            sourceSurvivorPairCount = sourceSurvivorKeys.Length,
            sourceSurvivorKeys,
            auditedPairCount = pairAudits.Length,
            pairMenuLineageExact,
            derivedPhase525Verdict,
            reportedPhase525Verdict = p525.GetProperty("verdictKind").GetString(),
            phase525ReportedStateConsistent,
            completeFinitePairCount,
            failedPairCount = failedFinitePairCount,
            twoOfTwoFiniteClosure,
            partialFiniteClosure,
            noCompleteFiniteSurvivor,
            derivedPositiveClosure = derivedPhase525PositiveClosure,
            reportedFiniteDualSurvivorPullbackBoundaryClosed = p525.GetProperty("finiteDualSurvivorPullbackBoundaryClosed").GetBoolean(),
            reportedFiniteCombinatorialChainAndCarrierClosure = p525.GetProperty("finiteCombinatorialChainAndCarrierClosure").GetBoolean(),
            finiteOnly,
            survivingCandidateIds = survivingPairAudits.Select(x => x.GetProperty("candidateId").GetString()).ToArray(),
            survivingRows = survivingPairAudits.Select(pair => new
            {
                candidateId = pair.GetProperty("candidateId").GetString(),
                reflectionId = pair.GetProperty("reflectionId").GetString(),
                extentMenu = pair.GetProperty("extentAudits").EnumerateArray()
                    .Select(extent => extent.GetProperty("extent").GetInt32()).ToArray(),
                completeFinitePullbackBoundary = pair.GetProperty("completeFinitePullbackBoundary").GetBoolean(),
            }).ToArray(),
            failedCandidateIds = pairAudits.Where(pair => !survivingPairAudits.Any(survivor =>
                survivor.GetProperty("candidateId").GetString() == pair.GetProperty("candidateId").GetString()))
                .Select(x => x.GetProperty("candidateId").GetString()).ToArray(),
            candidateSelected = false,
            candidateRegistered = false,
            geometryInstalled = false,
            allVolumeEmbeddingOrGluingEstablished = false,
        },
        nextExactMissingCertificates = new[]
        {
            "separate discharge or authorized selection for each member-dependent A5 prerequisite",
            "exact sd2 omega-parity resolution, while retaining the identity-member refutation",
            "exact O1/O2 dependency and positive-time-algebra membership",
            "normalized measure/domain and candidate-to-target equality/Hermiticity",
            "allowed-volume embedding, necessity, shared-edge gluing, and all-coupling/all-volume closure",
        },
        strongestPermittedTerminal = "candidate-package-ready-for-independent-mathematical-review",
        strongestPermittedClaim = "independent-mathematical-review-readiness-only",
        candidatePackageIndependentMathematicalReviewReady = reviewReady,
        externalReviewPending = true,
        actionMemberSelected = false,
        candidateSelected = false,
        actionOrGeometryRegistered = false,
        theoremClaimed = false,
        targetCounterexampleClaimed = false,
        reflectionPositivityEstablished = false,
        reflectionPositivityRefuted = false,
        phase458G1Satisfied = false,
        closesLimbL8 = false,
        phase515MayBeCreated = false,
        phase516MayBeCreated = false,
        phase515Unlocked = false,
        phase516Unlocked = false,
        phase481WorkPerformed = false,
        phase458EvaluationPerformed = false,
        samplingPerformed = false,
        samplingOrReprocessingRun = false,
        hmcPerformed = false,
        hmcRun = false,
        reprocessingPerformed = false,
        benchmarkPerformed = false,
        benchmarkRun = false,
        productionAllocationAuthorized = false,
        productionAuthorized = false,
        launchAuthorized = false,
        accelerationAuthorized = false,
        sourceContractApplicationAllowed = false,
        sourceContractApplicationPerformed = false,
        o4OrHumanRulingAuthoredOrInferred = false,
        physicalTargetTuningPerformed = false,
        physicalUnitClaimAllowed = false,
        gevClaimAllowed = false,
        routePromotesWzMasses = false,
        routePromotesHiggsMass = false,
        routeCompletesBosonPredictions = false,
        precursorArtifactsMutated = false,
        allExecutionAndPromotionAuthorities = false,
        promotedPhysicalMassClaimCount = 0,
    };

    Require(exactBindingInventoryValid, "Phase526 exact 24-binding ID/path inventory is invalid.");
    Require(precedenceBatteryPassed, "Phase526 precedence reachability battery failed.");
    Require(phase525ConsistencyBatteryPassed, "Phase526 Phase525 inconsistency mutation battery failed.");
    Require(phase525ReportedStateConsistent, "Phase526 Phase525 row-local and reported aggregate states disagree.");
    Require(expectedCurrentVerdictMatched, "Phase526 current verdict drifted from the frozen contract.");
    Require(!invalidOrDrifted, "Phase526 inputs are invalid or drifted.");

    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    var options = new JsonSerializerOptions { WriteIndented = true };
    string serialized = JsonSerializer.Serialize(result, options) + Environment.NewLine;
    File.WriteAllText(OutputPath, serialized);
    File.WriteAllText(SummaryPath, serialized);
    Console.WriteLine($"Phase526 verdict: {verdict}");
    Console.WriteLine($"Precedence battery: {(precedenceBatteryPassed ? "PASS" : "FAIL")}");
    Console.WriteLine($"Phase524 carry-forward: identity refuted={identityParityRefuted}, sd2 unresolved={sd2ParityUnresolved}");
    Console.WriteLine($"Phase525 carry-forward: finite closure={completeFinitePairCount}/2");
    Console.WriteLine($"Wrote {OutputPath}");
    Console.WriteLine($"Wrote {SummaryPath}");
}
finally
{
    foreach (JsonDocument document in documents)
        document.Dispose();
}

static string Evaluate(bool[] blockers, string[] precedence)
{
    int index = Array.FindIndex(blockers, blocked => blocked);
    return index >= 0 ? precedence[index] : precedence[^1];
}

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static (string Verdict, bool PositiveClosure) DerivePhase525State(int completeCount, int totalCount)
{
    if (totalCount <= 0 || completeCount < 0 || completeCount > totalCount)
        return ("invalid", false);
    if (completeCount == 0)
        return ("no-survivor-complete-finite-pullback-boundary", false);
    if (completeCount < totalCount)
        return ("partial-survivor-finite-pullback-boundary", false);
    return ("finite-dual-survivor-pullback-boundary-closed", true);
}

static bool Phase525StateConsistent(
    int derivedCompleteCount,
    int derivedTotalCount,
    string reportedVerdict,
    int reportedCompleteCount,
    int reportedFailedCount,
    bool reportedDualClosure,
    bool reportedFiniteClosure)
{
    (string derivedVerdict, bool derivedPositiveClosure) = DerivePhase525State(derivedCompleteCount, derivedTotalCount);
    return derivedVerdict != "invalid"
        && reportedVerdict == derivedVerdict
        && reportedCompleteCount == derivedCompleteCount
        && reportedFailedCount == derivedTotalCount - derivedCompleteCount
        && reportedDualClosure == derivedPositiveClosure
        && reportedFiniteClosure == derivedPositiveClosure;
}

static Phase525ConsistencyBatteryRow Phase525ConsistencyCase(
    string caseId,
    int derivedCompleteCount,
    int derivedTotalCount,
    string reportedVerdict,
    int reportedCompleteCount,
    int reportedFailedCount,
    bool reportedDualClosure,
    bool reportedFiniteClosure,
    bool expectedConsistent)
{
    bool actualConsistent = Phase525StateConsistent(
        derivedCompleteCount, derivedTotalCount, reportedVerdict, reportedCompleteCount,
        reportedFailedCount, reportedDualClosure, reportedFiniteClosure);
    return new Phase525ConsistencyBatteryRow(caseId, expectedConsistent, actualConsistent,
        actualConsistent == expectedConsistent);
}

static bool Phase525AuthorityFirewallsValid(JsonElement firewalls)
{
    string[] expectedNames =
    {
        "candidateSelectionAllowed", "candidateRegistrationAllowed", "productionGeometrySelectionAllowed",
        "allVolumeEmbeddingOrGluingClaimAllowed", "positiveTimeAlgebraClaimAllowed",
        "reflectionPositivityRulingAllowed", "theoremOrCounterexampleAllowed",
        "phase515CreationOrUnlockAllowed", "phase516CreationOrUnlockAllowed",
        "phase458G1SatisfactionAllowed", "limbL8ClosureAllowed", "phase458EvaluationAllowed",
        "phase481PackWorkAllowed", "actionOrObservableEvaluationAllowed", "normalizedMeasureEvaluationAllowed",
        "samplingOrReprocessingAllowed", "hmcAllowed", "benchmarkAllowed", "productionAllowed",
        "launchAllowed", "accelerationAllowed", "o4OrHumanRulingAllowed",
        "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
    };
    JsonProperty[] properties = firewalls.EnumerateObject().ToArray();
    return properties.Select(x => x.Name).SequenceEqual(expectedNames, StringComparer.Ordinal)
        && properties.All(x => x.Value.ValueKind == JsonValueKind.False);
}

sealed class Binding(string id, string path, string expectedSha256, string actualSha256, bool hashMatches)
{
    public string Id { get; } = id;
    public string Path { get; } = path;
    public string ExpectedSha256 { get; } = expectedSha256;
    public string ActualSha256 { get; } = actualSha256;
    public bool HashMatches { get; } = hashMatches;
}

sealed class PrecedenceBatteryRow(int index, string expected, string actual, bool passed)
{
    public int Index { get; } = index;
    public string Expected { get; } = expected;
    public string Actual { get; } = actual;
    public bool Passed { get; } = passed;
}

sealed class Phase525ConsistencyBatteryRow(
    string caseId, bool expectedConsistent, bool actualConsistent, bool passed)
{
    public string CaseId { get; } = caseId;
    public bool ExpectedConsistent { get; } = expectedConsistent;
    public bool ActualConsistent { get; } = actualConsistent;
    public bool Passed { get; } = passed;
}

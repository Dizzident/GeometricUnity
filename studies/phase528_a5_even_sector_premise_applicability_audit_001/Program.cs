using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase528_a5_even_sector_premise_applicability_audit_001";
const string ContractPath = Root + "/preregistration/phase528_a5_even_sector_premise_applicability_contract_v1.json";
const string OutputPath = Root + "/output/a5_even_sector_premise_applicability_audit.json";
const string SummaryPath = Root + "/output/a5_even_sector_premise_applicability_audit_summary.json";

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

var expectedInventory = new (string Id, string Path)[]
{
    ("a5-generator", "studies/phase456_consolidated_n4_launch_001/preregistration/a4a5_pack_generator/Program.cs"),
    ("serialized-a5", "studies/phase456_consolidated_n4_launch_001/preregistration/a5_gaussian_domination_stage_a.json"),
    ("phase520-program", "studies/phase520_a5_action_subject_lineage_parity_audit_001/Program.cs"),
    ("phase520-contract", "studies/phase520_a5_action_subject_lineage_parity_audit_001/preregistration/phase520_a5_action_subject_lineage_parity_contract_v1.json"),
    ("phase520-summary", "studies/phase520_a5_action_subject_lineage_parity_audit_001/output/a5_action_subject_lineage_parity_audit_summary.json"),
    ("phase523-program", "studies/phase523_a5_action_member_universalization_audit_001/Program.cs"),
    ("phase523-contract", "studies/phase523_a5_action_member_universalization_audit_001/preregistration/phase523_a5_action_member_universalization_contract_v1.json"),
    ("phase523-summary", "studies/phase523_a5_action_member_universalization_audit_001/output/a5_action_member_universalization_audit_summary.json"),
    ("phase524-program", "studies/phase524_a5_exact_omega_parity_decomposition_001/Program.cs"),
    ("phase524-contract", "studies/phase524_a5_exact_omega_parity_decomposition_001/preregistration/phase524_a5_exact_omega_parity_decomposition_contract_v1.json"),
    ("phase524-summary", "studies/phase524_a5_exact_omega_parity_decomposition_001/output/a5_exact_omega_parity_decomposition_summary.json"),
    ("phase527-program", "studies/phase527_a5_sd2_theta_zero_exact_parity_audit_001/Program.cs"),
    ("phase527-contract", "studies/phase527_a5_sd2_theta_zero_exact_parity_audit_001/preregistration/phase527_a5_sd2_theta_zero_exact_parity_contract_v1.json"),
    ("phase527-summary", "studies/phase527_a5_sd2_theta_zero_exact_parity_audit_001/output/a5_sd2_theta_zero_exact_parity_audit_summary.json"),
};
bool exactBindingInventoryValid = bindings.Length == expectedInventory.Length
    && bindings.Select(x => (x.Id, x.Path)).SequenceEqual(expectedInventory)
    && bindings.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == bindings.Length
    && bindings.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == bindings.Length;
bool exactBindingsValid = exactBindingInventoryValid && bindings.All(x => x.HashMatches);

string[] expectedPrecedence =
[
    "invalid-or-drifted-input", "both-registered-members-outside-frozen-even-sector-premise",
    "member-scoped-premise-mismatch", "premise-applicability-unresolved",
];
string[] expectedMemberIds = ["identity", "sd2-id0/c0.5"];
string[] expectedFirewallKeys =
[
    "actionMemberSelectionAllowed", "actionOrGeometryRegistrationAllowed",
    "phase515Or516UnlockAllowed", "phase458EvaluationOrG1Allowed",
    "limbL8ClosureAllowed", "generalTheoremOrTargetCounterexampleAllowed",
    "reflectionPositivityRulingAllowed", "samplingOrReprocessingAllowed",
    "hmcOrBenchmarkAllowed", "productionOrLaunchAllowed",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
];
bool contractValid =
    contract.GetProperty("contractId").GetString() == "phase528-a20-even-sector-premise-applicability-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A20"
    && contract.GetProperty("frozenBeforeEvaluation").GetBoolean()
    && contract.GetProperty("registeredMemberIds").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedMemberIds)
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedPrecedence)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

string generator = File.ReadAllText(bindings.Single(x => x.Id == "a5-generator").Path);
using var serializedDoc = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "serialized-a5").Path));
using var p520Doc = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase520-summary").Path));
using var p523Doc = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase523-summary").Path));
using var p524Doc = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase524-summary").Path));
using var p527Doc = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase527-summary").Path));
JsonElement serialized = serializedDoc.RootElement;
JsonElement p520 = p520Doc.RootElement;
JsonElement p523 = p523Doc.RootElement;
JsonElement p524 = p524Doc.RootElement;
JsonElement p527 = p527Doc.RootElement;

bool frozenPremisePresent =
    generator.Contains("exact Z_2 symmetry omega -> -omega", StringComparison.Ordinal)
    && generator.Contains("even (omega -> -omega Z_2 invariant) sector of the exactly-quartic S_B", StringComparison.Ordinal)
    && generator.Contains("S_B(omega) quartic, even under omega -> -omega", StringComparison.Ordinal)
    && serialized.GetProperty("sector").GetString() == "even (omega -> -omega Z_2 invariant) sector of the exactly-quartic S_B"
    && serialized.GetProperty("sbStructure").GetString()!.Contains("S_B(omega) quartic, even under omega -> -omega", StringComparison.Ordinal);

JsonElement memberDomain = p523.GetProperty("memberDomainAudit");
string[] registeredMemberIds = memberDomain.GetProperty("members").EnumerateArray()
    .Select(x => x.GetProperty("memberId").GetString()!).ToArray();
bool familyDomainComplete =
    p520.GetProperty("verdictKind").GetString() == "action-member-unresolved"
    && p520.GetProperty("actionMemberAudit").GetProperty("actionMemberInventoryComplete").GetBoolean()
    && p520.GetProperty("actionMemberAudit").GetProperty("registeredMemberCount").GetInt32() == 2
    && p520.GetProperty("actionMemberAudit").GetProperty("familyExplicitlyHasNoCanonicalMember").GetBoolean()
    && p523.GetProperty("verdictKind").GetString() == "universalization-partial-member-dependence-remains"
    && memberDomain.GetProperty("completeTwoMemberDomainEstablished").GetBoolean()
    && memberDomain.GetProperty("registeredMemberCount").GetInt32() == 2
    && registeredMemberIds.SequenceEqual(expectedMemberIds, StringComparer.Ordinal)
    && !memberDomain.GetProperty("actionMemberSelected").GetBoolean();

JsonElement identity = p524.GetProperty("memberDecompositions").EnumerateArray()
    .Single(x => x.GetProperty("memberId").GetString() == "identity");
JsonElement identityWitness = p524.GetProperty("identityExactWitness");
bool identityOutsidePremise =
    p524.GetProperty("verdictKind").GetString() == "exact-omega-parity-refuted-identity-member"
    && p524.GetProperty("exactIdentityReductionCertified").GetBoolean()
    && p524.GetProperty("proofPolicy").GetProperty("everyFaceContributionIncluded").GetBoolean()
    && identity.GetProperty("completeExecutableFormulaCoverage").GetBoolean()
    && identity.GetProperty("exactOddCancellationResolved").GetBoolean()
    && !identity.GetProperty("identityExactlyEven").GetBoolean()
    && identityWitness.GetProperty("identityOddTermSurvives").GetBoolean()
    && identityWitness.GetProperty("actionAtPlusMinusDifferenceNumerator").GetInt32() == 1
    && identityWitness.GetProperty("actionAtPlusMinusDifferenceDenominator").GetInt32() == 1;

JsonElement sd2Member = p527.GetProperty("member");
JsonElement sd2Witness = p527.GetProperty("witness");
bool sd2OutsidePremise =
    p527.GetProperty("verdictKind").GetString() == "exact-sd2-theta-zero-odd-witness"
    && p527.GetProperty("exactReductionAvailable").GetBoolean()
    && p527.GetProperty("exactRationalArithmeticOnly").GetBoolean()
    && !p527.GetProperty("binary64ReplayUsedAsEvidence").GetBoolean()
    && sd2Member.GetProperty("memberId").GetString() == "sd2-id0/c0.5"
    && sd2Member.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && sd2Member.GetProperty("omegaParityStatus").GetString() == "refuted-by-exact-theta-zero-full-action-witness"
    && sd2Witness.GetProperty("actionPlusMinusDifference").GetString() == "13/180"
    && sd2Witness.GetProperty("cubicCoefficient").GetString() == "13/360"
    && sd2Witness.GetProperty("allFacesIncluded").GetBoolean();

bool inputsValid = contractValid && exactBindingsValid && frozenPremisePresent && familyDomainComplete;
bool bothOutsidePremise = inputsValid && identityOutsidePremise && sd2OutsidePremise;
bool exactlyOneOutsidePremise = inputsValid && identityOutsidePremise != sd2OutsidePremise;
string verdict = EvaluateVerdict(!inputsValid, bothOutsidePremise, exactlyOneOutsidePremise);
var precedenceBattery = new[]
{
    Case("invalid", true, true, false, "invalid-or-drifted-input"),
    Case("both-outside", false, true, false, "both-registered-members-outside-frozen-even-sector-premise"),
    Case("identity-only", false, false, true, "member-scoped-premise-mismatch"),
    Case("unresolved", false, false, false, "premise-applicability-unresolved"),
};
bool precedenceBatteryPassed = precedenceBattery.All(x => x.Passed);

var result = new
{
    schemaVersion = 1,
    phase = 528,
    phaseId = "phase528-a5-even-sector-premise-applicability-audit",
    contractId = contract.GetProperty("contractId").GetString(),
    planSection = contract.GetProperty("planSection").GetString(),
    inputsValid,
    contractValid,
    exactBindingInventoryValid,
    exactBindingsValid,
    frozenPremisePresent,
    familyDomainComplete,
    registeredMemberIds,
    identityOutsidePremise,
    sd2OutsidePremise,
    bothRegisteredMembersOutsideFrozenEvenSectorPremise = bothOutsidePremise,
    exactCounterexampleLogicApplied = true,
    precedenceBatteryPassed,
    precedenceBattery,
    verdictKind = verdict,
    terminalStatus = "a5-even-sector-premise-applicability-audit-" + verdict,
    premiseAudit = new
    {
        premise = "the committed A5 Stage-A pack treats S_B as exactly even under omega -> -omega and restricts to its even observable sector",
        quantifierScope = "both exact-bound executable Phase452 members",
        rows = new object[]
        {
            new { memberId = "identity", exactWitness = "action(+omega)-action(-omega)=1", premiseApplies = !identityOutsidePremise },
            new { memberId = "sd2-id0/c0.5", exactWitness = "theta-zero action(+omega)-action(-omega)=13/180", premiseApplies = !sd2OutsidePremise },
        },
        currentPackApplicability = bothOutsidePremise ? "inapplicable-to-every-registered-member" : "unresolved-or-member-scoped",
    },
    decision = bothOutsidePremise
        ? "Both registered executable action members have exact finite full-action omega-odd witnesses. The frozen A5 pack's exactly-even action premise is therefore inapplicable to every member of this registered family. This is only a current-pack applicability result."
        : "The applicability of the frozen A5 exactly-even action premise is not closed for the complete registered family.",
    scopeFirewalls = new
    {
        generalActionTheoremClaimed = false,
        alternativeA5TheoremRuledOut = false,
        reflectionPositivityEstablished = false,
        reflectionPositivityRefuted = false,
        targetTheoremCounterexampleClaimed = false,
        geometryCandidateInvalidatedGenerally = false,
        thetaNonzeroSd2Claimed = false,
    },
    bindings,
    deterministicZeroSampling = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    externalReviewPending = true,
    actionMemberSelected = false,
    actionOrGeometryRegistered = false,
    phase515MayBeCreated = false,
    phase516MayBeCreated = false,
    phase458EvaluationPerformed = false,
    phase458G1Satisfied = false,
    closesLimbL8 = false,
    theoremClaimed = false,
    targetCounterexampleClaimed = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    samplingPerformed = false,
    reprocessingPerformed = false,
    hmcPerformed = false,
    benchmarkPerformed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    sourceContractApplicationPerformed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    allExecutionAndPromotionAuthorities = false,
    promotedPhysicalMassClaimCount = 0,
};

Require(contractValid, "Phase528 frozen contract invalid.");
Require(inputsValid, "Phase528 inputs invalid or drifted.");
Require(precedenceBatteryPassed, "Phase528 precedence battery failed.");
Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase528 verdict differs from frozen expected branch.");
Require("a5-even-sector-premise-applicability-audit-" + verdict == contract.GetProperty("expectedCurrentTerminalStatus").GetString(), "Phase528 terminal drifted.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase528 verdict: {verdict}");
Console.WriteLine($"registered members outside premise: {(identityOutsidePremise ? 1 : 0) + (sd2OutsidePremise ? 1 : 0)}/2");

static string EvaluateVerdict(bool invalid, bool bothOutside, bool oneOutside) =>
    invalid ? "invalid-or-drifted-input" :
    bothOutside ? "both-registered-members-outside-frozen-even-sector-premise" :
    oneOutside ? "member-scoped-premise-mismatch" :
    "premise-applicability-unresolved";

static BatteryCase Case(string id, bool invalid, bool bothOutside, bool oneOutside, string expected)
{
    string actual = EvaluateVerdict(invalid, bothOutside, oneOutside);
    return new BatteryCase(id, expected, actual, actual == expected);
}

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record BatteryCase(string Id, string Expected, string Actual, bool Passed);

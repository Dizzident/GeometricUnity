using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase530_o4_g4_authentication_admissibility_audit_001";
const string ContractPath = Root + "/preregistration/phase530_o4_g4_authentication_admissibility_audit_contract_v1.json";
const string OutputPath = Root + "/output/o4_g4_authentication_admissibility_audit.json";
const string SummaryPath = Root + "/output/o4_g4_authentication_admissibility_audit_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;

string[] expectedPrecedence =
[
    "invalid-or-drifted-input",
    "authentication-only-g4-predicate-counterexample",
    "no-counterexample-under-frozen-battery",
];
string[] expectedFirewallKeys =
[
    "humanMemoConsumedOrModified", "reviewerRegistered", "rulingAuthoredOrInferred",
    "o4Satisfied", "phase458G3Satisfied", "phase458G4Satisfied", "phase458G5Satisfied",
    "phase458EvaluationAllowed", "phase481ConstructionOrMutationAllowed",
    "samplingOrReprocessingAllowed", "hmcOrBenchmarkAllowed", "productionOrLaunchAllowed",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
];

JsonElement frozenBattery = contract.GetProperty("syntheticStateBattery");
bool frozenBatteryShapeValid = frozenBattery.GetArrayLength() == 6
    && frozenBattery.EnumerateArray().Select(x => x.GetProperty("caseId").GetString()).SequenceEqual(new[]
    {
        "invalid-authentication", "authenticated-incomplete-defense", "authenticated-all-defer",
        "authenticated-one-defer", "authenticated-all-supporting", "authenticated-one-adverse",
    }, StringComparer.Ordinal)
    && frozenBattery.EnumerateArray().Select(x => x.GetProperty("caseId").GetString()).Distinct(StringComparer.Ordinal).Count() == 6;

var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(entry => new BindingSpec(
    entry.GetProperty("id").GetString()!,
    entry.GetProperty("path").GetString()!,
    entry.GetProperty("sha256").GetString()!)).ToArray();
var bindings = bindingSpecs.Select(spec => new Binding(
    spec.Id, spec.Path, spec.ExpectedSha256, Sha256(spec.Path),
    StringComparer.Ordinal.Equals(spec.ExpectedSha256, Sha256(spec.Path)))).ToArray();
bool exactBindingInventoryValid = bindings.Length == 11
    && bindings.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 11;
bool exactBindingsValid = exactBindingInventoryValid && bindings.All(x => x.HashMatches);
var sources = bindingSpecs.ToDictionary(x => x.Id, x => File.ReadAllText(x.Path), StringComparer.Ordinal);

bool contractValid =
    contract.GetProperty("contractId").GetString() == "phase530-a21-o4-g4-authentication-admissibility-audit-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A21"
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && frozenBatteryShapeValid
    && contract.GetProperty("primaryCounterexampleCaseId").GetString() == "authenticated-all-defer"
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedPrecedence)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

using var phase477Document = JsonDocument.Parse(sources["phase477-summary"]);
using var phase478ContractDocument = JsonDocument.Parse(sources["phase478-contract"]);
using var phase478SummaryDocument = JsonDocument.Parse(sources["phase478-summary"]);
using var phase480ContractDocument = JsonDocument.Parse(sources["phase480-contract"]);
using var memoSchemaDocument = JsonDocument.Parse(sources["memo-schema"]);
using var memoTemplateDocument = JsonDocument.Parse(sources["memo-template"]);
using var phase480SummaryDocument = JsonDocument.Parse(sources["phase480-summary"]);
using var dependencyMapDocument = JsonDocument.Parse(sources["o4-dependency-map"]);
JsonElement phase477 = phase477Document.RootElement;
JsonElement phase478Contract = phase478ContractDocument.RootElement;
JsonElement phase478Summary = phase478SummaryDocument.RootElement;
JsonElement phase480Contract = phase480ContractDocument.RootElement;
JsonElement memoSchema = memoSchemaDocument.RootElement;
JsonElement memoTemplate = memoTemplateDocument.RootElement;
JsonElement phase480Summary = phase480SummaryDocument.RootElement;
JsonElement dependencyMap = dependencyMapDocument.RootElement;

string phase478Program = sources["phase478-program"];
string phase480Verifier = sources["phase480-verifier"];
string phase480Program = sources["phase480-program"];
int mandatoryRulingCount = phase478Contract.GetProperty("gates").GetProperty("G4").GetProperty("requiredRulingIds").GetArrayLength();
bool precursorSemanticsValid =
    phase477.GetProperty("infrastructureReady").GetBoolean()
    && phase477.GetProperty("exactCoveragePassed").GetBoolean()
    && phase477.GetProperty("mandatoryRulingIdCount").GetInt32() == 13
    && mandatoryRulingCount == 13
    && memoSchema.GetProperty("properties").GetProperty("rulings").GetProperty("minItems").GetInt32() == 13
    && memoSchema.GetProperty("properties").GetProperty("rulings").GetProperty("maxItems").GetInt32() == 13
    && memoTemplate.GetProperty("rulings").GetArrayLength() == 13
    && memoTemplate.GetProperty("rulings").EnumerateArray().All(x =>
        x.GetProperty("disposition").GetString() == "insufficient-basis"
        && x.GetProperty("selectedOption").GetString() == "defer")
    && phase480Verifier.Contains("(ruling.selectedOption === \"defer\") === (ruling.disposition === \"insufficient-basis\")", StringComparison.Ordinal)
    && phase480Program.Contains("externalMemoValidated = valid", StringComparison.Ordinal)
    && phase478Program.Contains("bool phase480GenuineMemoGreen = B(p480, \"externalMemoValidated\")", StringComparison.Ordinal)
    && !G4PredicateBlock(phase478Program).Contains("normalizedRulings", StringComparison.Ordinal)
    && !G4PredicateBlock(phase478Program).Contains("disposition", StringComparison.Ordinal)
    && !G4PredicateBlock(phase478Program).Contains("selectedOption", StringComparison.Ordinal)
    && phase478Summary.GetProperty("g4").GetProperty("state").GetString() == "missing"
    && !phase478Summary.GetProperty("g4").GetProperty("phase480GenuineMemoGreen").GetBoolean()
    && phase480Contract.GetProperty("machineAuthoredOrInferredRulingAccepted").ValueKind == JsonValueKind.False
    && phase480Summary.GetProperty("verdictKind").GetString() == "awaiting-external-physicist-ruling"
    && !phase480Summary.GetProperty("externalMemoValidated").GetBoolean()
    && dependencyMap.GetProperty("rulingIds").GetArrayLength() == 13;

var battery = frozenBattery.EnumerateArray().Select(entry =>
{
    string caseId = entry.GetProperty("caseId").GetString()!;
    bool authenticationValid = entry.GetProperty("authenticationValid").GetBoolean();
    bool complete = entry.GetProperty("completeRulingPresence").GetBoolean();
    bool resolved = entry.GetProperty("allRulingsResolved").GetBoolean();
    bool adverse = entry.GetProperty("adverseDispositionPresent").GetBoolean();
    string actualG4 = CurrentAuthenticationOnlyG4(authenticationValid);
    string actualClass = ContentClass(authenticationValid, complete, resolved, adverse);
    string expectedG4 = entry.GetProperty("expectedAuthenticationOnlyG4").GetString()!;
    string expectedClass = entry.GetProperty("expectedContentClass").GetString()!;
    return new BatteryCase(caseId, authenticationValid, complete, resolved, adverse,
        expectedG4, actualG4, expectedClass, actualClass,
        expectedG4 == actualG4 && expectedClass == actualClass);
}).ToArray();
bool syntheticBatteryPassed = battery.All(x => x.Passed);
BatteryCase primary = battery.Single(x => x.CaseId == "authenticated-all-defer");
BatteryCase supporting = battery.Single(x => x.CaseId == "authenticated-all-supporting");
BatteryCase adverseCase = battery.Single(x => x.CaseId == "authenticated-one-adverse");
bool authenticationAdmissibilityCounterexample =
    primary.ActualAuthenticationOnlyG4 == "available-true"
    && primary.ActualContentClass == "unresolved"
    && primary.ActualAuthenticationOnlyG4 == supporting.ActualAuthenticationOnlyG4
    && supporting.ActualContentClass == "resolved-supporting"
    && adverseCase.ActualAuthenticationOnlyG4 == supporting.ActualAuthenticationOnlyG4
    && adverseCase.ActualContentClass == "resolved-adverse";

bool inputsValid = contractValid && exactBindingsValid && precursorSemanticsValid && syntheticBatteryPassed;
string verdict = !inputsValid ? "invalid-or-drifted-input"
    : authenticationAdmissibilityCounterexample ? "authentication-only-g4-predicate-counterexample"
    : "no-counterexample-under-frozen-battery";

var result = new
{
    schemaVersion = 1,
    phase = 530,
    phaseId = "phase530-o4-g4-authentication-admissibility-audit",
    contractId = contract.GetProperty("contractId").GetString(),
    planSection = contract.GetProperty("planSection").GetString(),
    inputsValid,
    contractValid,
    frozenBatteryShapeValid,
    exactBindingInventoryValid,
    exactBindingsValid,
    precursorSemanticsValid,
    syntheticBatteryPassed,
    authenticationAdmissibilityCounterexample,
    primaryCounterexampleCaseId = primary.CaseId,
    verdictKind = verdict,
    terminalStatus = "o4-g4-authentication-admissibility-audit-" + verdict,
    decision = authenticationAdmissibilityCounterexample
        ? "The current Phase478 G4 predicate distinguishes authentication but not scientific resolution or adverse content. A synthetic authenticated complete all-defer state reaches available-true under that predicate while remaining unresolved. This scopes a consumer defect only; Phase480 authentication remains valid and no ruling is authored."
        : "The frozen battery did not establish the preregistered authentication/admissibility counterexample.",
    audit = new
    {
        currentPredicateInputs = new[] { "phase477 infrastructure readiness", "Phase480 authentication/provenance", "Phase480 pending flag" },
        currentPredicateReadsNormalizedRulings = false,
        currentPredicateReadsDisposition = false,
        currentPredicateReadsSelectedOption = false,
        phase480AuthenticationInvalidated = false,
        futureConsumerCorrectionRequired = authenticationAdmissibilityCounterexample,
        scientificResolutionSemanticsDefinedByThisPhase = false,
        syntheticStateUsedAsHumanEvidence = false,
    },
    battery,
    bindings,
    zeroPhysicsCompute = true,
    deterministicZeroSampling = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    externalReviewPending = true,
    humanMemoConsumed = false,
    humanMemoModified = false,
    reviewerRegistered = false,
    rulingAuthored = false,
    rulingInferred = false,
    o4Satisfied = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    phase458Evaluated = false,
    phase481Constructed = false,
    phase481Mutated = false,
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

Require(contractValid, "Phase530 frozen contract invalid.");
Require(inputsValid, "Phase530 inputs invalid or drifted.");
Require(syntheticBatteryPassed, "Phase530 synthetic state battery failed.");
Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase530 verdict differs from the frozen expected branch.");
Require("o4-g4-authentication-admissibility-audit-" + verdict == contract.GetProperty("expectedCurrentTerminalStatus").GetString(), "Phase530 terminal drifted.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase530 verdict: {verdict}");
Console.WriteLine($"synthetic battery: {battery.Length} cases, passed={syntheticBatteryPassed}");
Console.WriteLine($"all-defer counterexample: {authenticationAdmissibilityCounterexample}");

static string CurrentAuthenticationOnlyG4(bool authenticationValid) => authenticationValid ? "available-true" : "missing";

static string ContentClass(bool authenticationValid, bool complete, bool resolved, bool adverse) =>
    !authenticationValid ? "authentication-invalid" :
    !complete ? "incomplete" :
    !resolved ? "unresolved" :
    adverse ? "resolved-adverse" :
    "resolved-supporting";

static string G4PredicateBlock(string program)
{
    int start = program.IndexOf("bool phase480GenuineMemoGreen", StringComparison.Ordinal);
    int end = program.IndexOf("var g5 =", start, StringComparison.Ordinal);
    return start >= 0 && end > start ? program[start..end] : string.Empty;
}

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
sealed record BindingSpec(string Id, string Path, string ExpectedSha256);
sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record BatteryCase(
    string CaseId,
    bool AuthenticationValid,
    bool CompleteRulingPresence,
    bool AllRulingsResolved,
    bool AdverseDispositionPresent,
    string ExpectedAuthenticationOnlyG4,
    string ActualAuthenticationOnlyG4,
    string ExpectedContentClass,
    string ActualContentClass,
    bool Passed);

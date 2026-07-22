using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase532_phase458_g4_consumer_correction_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase532_phase458_g4_consumer_correction_adjudicator_contract_v1.json";
const string OutputPath = Root + "/output/phase458_g4_consumer_correction_adjudicator.json";
const string SummaryPath = Root + "/output/phase458_g4_consumer_correction_adjudicator_summary.json";

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string S(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString()! : "";
static bool B(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.True;
static string[] Strings(JsonElement e, string name) => e.GetProperty(name).EnumerateArray().Select(x => x.GetString()!).ToArray();
static bool ExactSequence(IEnumerable<string> actual, IEnumerable<string> expected) => actual.SequenceEqual(expected, StringComparer.Ordinal);
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

using var contractDoc = JsonDocument.Parse(File.ReadAllText(ContractPath));
JsonElement contract = contractDoc.RootElement;

var bindingRows = contract.GetProperty("exactBindings").EnumerateArray().ToArray();
var expectedBindings = new[]
{
    ("phase478-program", "studies/phase478_phase458_gate_specification_closure_001/Program.cs"),
    ("phase478-contract", "studies/phase478_phase458_gate_specification_closure_001/preregistration/phase458_gate_contract_v1.json"),
    ("phase478-summary", "studies/phase478_phase458_gate_specification_closure_001/output/phase458_gate_specification_closure_summary.json"),
    ("phase480-program", "studies/phase480_o4_physicist_adjudication_intake_001/Program.cs"),
    ("phase480-summary", "studies/phase480_o4_physicist_adjudication_intake_001/output/o4_physicist_adjudication_intake_summary.json"),
    ("phase530-program", "studies/phase530_o4_g4_authentication_admissibility_audit_001/Program.cs"),
    ("phase530-contract", "studies/phase530_o4_g4_authentication_admissibility_audit_001/preregistration/phase530_o4_g4_authentication_admissibility_audit_contract_v1.json"),
    ("phase530-summary", "studies/phase530_o4_g4_authentication_admissibility_audit_001/output/o4_g4_authentication_admissibility_audit_summary.json"),
    ("phase531-program", "studies/phase531_o4_g4_disposition_resolution_semantics_001/Program.cs"),
    ("phase531-contract", "studies/phase531_o4_g4_disposition_resolution_semantics_001/preregistration/phase531_o4_g4_disposition_resolution_semantics_contract_v1.json"),
    ("phase531-full", "studies/phase531_o4_g4_disposition_resolution_semantics_001/output/o4_g4_disposition_resolution_semantics.json"),
    ("phase531-summary", "studies/phase531_o4_g4_disposition_resolution_semantics_001/output/o4_g4_disposition_resolution_semantics_summary.json"),
};
var bindings = bindingRows.Select(row =>
{
    string path = S(row, "path");
    string expected = S(row, "sha256");
    string actual = File.Exists(path) ? Sha256(path) : "missing";
    return new { id = S(row, "id"), path, expectedSha256 = expected, actualSha256 = actual, hashMatches = actual == expected };
}).ToArray();

bool bindingInventoryValid = bindings.Length == expectedBindings.Length
    && bindings.Select(x => (x.id, x.path)).SequenceEqual(expectedBindings)
    && bindings.All(x => x.path.Length > 0 && x.expectedSha256.Length == 64);
bool exactBindingsValid = bindingInventoryValid && bindings.All(x => x.hashMatches);

using var phase478Doc = JsonDocument.Parse(File.ReadAllText(bindings.Single(x => x.id == "phase478-summary").path));
using var phase480Doc = JsonDocument.Parse(File.ReadAllText(bindings.Single(x => x.id == "phase480-summary").path));
using var phase530Doc = JsonDocument.Parse(File.ReadAllText(bindings.Single(x => x.id == "phase530-summary").path));
using var phase531Doc = JsonDocument.Parse(File.ReadAllText(bindings.Single(x => x.id == "phase531-summary").path));
JsonElement p478 = phase478Doc.RootElement;
JsonElement p480 = phase480Doc.RootElement;
JsonElement p530 = phase530Doc.RootElement;
JsonElement p531 = phase531Doc.RootElement;

string[] expectedPrecedence =
{
    "invalid-or-drifted-input",
    "missing",
    "resolved-adverse-upstream-invalidation",
    "available-true",
};
string[] expectedCases = { "all-defer", "one-defer", "all-supporting", "resolved-adverse" };
string[] expectedFirewalls =
{
    "humanRulingConsumptionAllowed", "humanRulingAuthorshipOrInferenceAllowed",
    "o4Satisfied", "phase458G3Satisfied", "phase458G4Satisfied",
    "phase458G5Satisfied", "phase458EvaluationAllowed",
    "phase481ConstructionOrMutationAllowed", "samplingOrReprocessingAllowed",
    "hmcOrBenchmarkAllowed", "productionOrLaunchAllowed",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
};
JsonElement firewalls = contract.GetProperty("authorityFirewalls");
bool contractValid =
    S(contract, "contractId") == "phase532-a21-phase458-g4-consumer-correction-adjudicator-v1"
    && S(contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A21"
    && B(contract, "frozenBeforeConsumption")
    && ExactSequence(Strings(contract, "consumerPrecedence"), expectedPrecedence)
    && ExactSequence(Strings(contract, "requiredSyntheticCases"), expectedCases)
    && S(contract, "expectedCurrentVerdict") == "consumer-correction-closed-current-g4-missing"
    && S(contract, "expectedCurrentTerminalStatus") == "phase458-g4-consumer-correction-adjudicator-consumer-correction-closed-current-g4-missing"
    && B(contract, "externalReviewPending")
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && firewalls.EnumerateObject().Select(x => x.Name).Order(StringComparer.Ordinal)
        .SequenceEqual(expectedFirewalls.Order(StringComparer.Ordinal), StringComparer.Ordinal)
    && expectedFirewalls.All(name => firewalls.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.False);

bool phase478SemanticsValid =
    S(p478, "verdictKind") == "gate-specification-closed-phase458-inputs-incomplete"
    && !B(p478, "phase458Evaluated")
    && S(p478.GetProperty("g4"), "state") == "missing"
    && !B(p478.GetProperty("g4"), "phase480GenuineMemoGreen");
bool phase480CurrentStateValid =
    S(p480, "verdictKind") == "awaiting-external-physicist-ruling"
    && !B(p480, "externalMemoValidated")
    && B(p480, "physicistReviewPending")
    && !B(p480, "rulingConsumed");
bool phase530SemanticsValid =
    S(p530, "verdictKind") == "authentication-only-g4-predicate-counterexample"
    && B(p530, "authenticationAdmissibilityCounterexample")
    && B(p530, "syntheticBatteryPassed")
    && !B(p530.GetProperty("audit"), "currentPredicateReadsNormalizedRulings")
    && B(p530.GetProperty("audit"), "futureConsumerCorrectionRequired")
    && !B(p530, "phase458Evaluated");
bool phase531SemanticsValid =
    S(p531, "verdictKind") == "resolution-semantics-closed-current-g4-missing"
    && B(p531, "batteryPassed")
    && B(p531, "allDeferRemainsUnresolved")
    && B(p531, "oneDeferRemainsUnresolved")
    && B(p531, "allSupportingDistinguished")
    && B(p531, "oneAdverseDistinguished")
    && B(p531, "mixedAdversePreserved")
    && B(p531, "notApplicableNeverSupports")
    && S(p531.GetProperty("currentState"), "phase458G4State") == "missing"
    && !B(p531.GetProperty("currentState"), "phase458G4Satisfied")
    && !B(p531, "phase458EvaluationPerformed");

bool inputsValid = contractValid && exactBindingsValid && phase478SemanticsValid
    && phase480CurrentStateValid && phase530SemanticsValid && phase531SemanticsValid;

static ConsumerDecision Resolve(bool valid, bool authenticated, string contentClass)
{
    if (!valid) return new("invalid-or-drifted-input", "invalid-or-drifted", false, false);
    if (!authenticated || contentClass is "unresolved-content" or "invalid-authentication" or "invalid-ruling-set")
        return new("missing", "blocked-inputs-incomplete", false, false);
    if (contentClass == "resolved-adverse")
        return new("available-false", "blocked-upstream-invalidated-by-o4", true, false);
    if (contentClass == "resolved-supporting")
        return new("available-true", "continue-phase458-precedence", false, true);
    return new("invalid-or-drifted-input", "invalid-or-drifted", false, false);
}

var syntheticCases = new[]
{
    Case("all-defer", true, true, "unresolved-content", "missing", "blocked-inputs-incomplete", false),
    Case("one-defer", true, true, "unresolved-content", "missing", "blocked-inputs-incomplete", false),
    Case("all-supporting", true, true, "resolved-supporting", "available-true", "continue-phase458-precedence", false),
    Case("resolved-adverse", true, true, "resolved-adverse", "available-false", "blocked-upstream-invalidated-by-o4", true),
};
bool syntheticBatteryPassed = syntheticCases.All(x => x.Passed)
    && syntheticCases.Where(x => x.Id is "all-defer" or "one-defer").All(x => x.ActualG4State == "missing")
    && syntheticCases.Single(x => x.Id == "resolved-adverse").ActualG4State != "available-true";

ConsumerDecision current = Resolve(inputsValid, B(p480, "externalMemoValidated"), "unresolved-content");
string verdict = !inputsValid ? "invalid-or-drifted-input"
    : current.G4State == "missing" ? "consumer-correction-closed-current-g4-missing"
    : "invalid-or-drifted-input";
string terminalStatus = "phase458-g4-consumer-correction-adjudicator-" + verdict;

var result = new
{
    schemaVersion = 1,
    phase = 532,
    phaseId = "phase532-phase458-g4-consumer-correction-adjudicator",
    contractId = S(contract, "contractId"),
    planSection = S(contract, "planSection"),
    inputsValid,
    contractValid,
    bindingInventoryValid,
    exactBindingsValid,
    phase478SemanticsValid,
    phase480CurrentStateValid,
    phase530SemanticsValid,
    phase531SemanticsValid,
    authenticationOnlyPhase478G4PredicateUnsafeForFutureEvaluation = true,
    phase478ArtifactInvalidatedOrMutated = false,
    phase480AuthenticationInvalidatedOrMutated = false,
    futureConsumerRequiresPhase531ResolutionAndAdversitySemantics = true,
    consumerPrecedence = expectedPrecedence,
    syntheticBatteryPassed,
    syntheticCases,
    currentState = new
    {
        phase480AuthenticatedMemoPresent = false,
        phase531ResolutionClass = "unresolved-content",
        phase458G4State = current.G4State,
        phase458G4Satisfied = false,
        upstreamInvalidationPresent = false,
        phase458RestingState = "blocked-inputs-incomplete",
    },
    futureRules = new
    {
        authenticatedAllDeferG4State = "missing",
        authenticatedOneDeferG4State = "missing",
        authenticatedResolvedSupportingG4State = "available-true",
        authenticatedResolvedAdverseG4State = "available-false",
        authenticatedResolvedAdverseOutcome = "blocked-upstream-invalidated-by-o4",
        resolvedAdverseNeverPositive = true,
        authenticationAloneNeverSatisfiesG4 = true,
    },
    verdictKind = verdict,
    terminalStatus,
    decision = "The authentication-only Phase478 G4 predicate is unsafe for future Phase458 evaluation. Future consumption must use Phase531 resolution and adversity semantics: defer remains missing, supporting resolved content may become available-true, and adverse resolved content remains non-positive and routes to the existing upstream-invalidation outcome. Current G4 remains missing and Phase458 is not evaluated.",
    bindings,
    zeroPhysicsCompute = true,
    deterministicZeroSampling = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    externalReviewPending = true,
    humanMemoConsumed = false,
    humanRulingAuthoredOrInferred = false,
    o4Satisfied = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    phase458EvaluationPerformed = false,
    phase481ConstructedOrMutated = false,
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

Require(inputsValid, "Phase532 inputs are invalid or drifted.");
Require(syntheticBatteryPassed, "Phase532 synthetic consumer battery failed.");
Require(verdict == S(contract, "expectedCurrentVerdict"), "Phase532 verdict drifted.");
Require(terminalStatus == S(contract, "expectedCurrentTerminalStatus"), "Phase532 terminal drifted.");
Require(result.currentState.phase458G4State == "missing", "Phase532 current G4 must remain missing.");
Require(!result.phase458EvaluationPerformed && result.promotedPhysicalMassClaimCount == 0, "Phase532 authority firewall failed.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(OutputPath, json);
File.WriteAllText(SummaryPath, json);
Console.WriteLine(terminalStatus);
Console.WriteLine($"syntheticBatteryPassed={syntheticBatteryPassed} currentG4={result.currentState.phase458G4State} phase458Evaluated=False promotedPhysicalMassClaimCount=0");

static SyntheticCase Case(string id, bool valid, bool authenticated, string contentClass, string expectedG4, string expectedRoute, bool expectedAdverse)
{
    ConsumerDecision actual = Resolve(valid, authenticated, contentClass);
    return new SyntheticCase(
        id, valid, authenticated, contentClass, expectedG4, actual.G4State,
        expectedRoute, actual.Route, expectedAdverse, actual.Adverse, actual.Supporting,
        actual.G4State == expectedG4 && actual.Route == expectedRoute && actual.Adverse == expectedAdverse);
}

sealed class ConsumerDecision
{
    public ConsumerDecision(string g4State, string route, bool adverse, bool supporting) =>
        (G4State, Route, Adverse, Supporting) = (g4State, route, adverse, supporting);

    public string G4State { get; }
    public string Route { get; }
    public bool Adverse { get; }
    public bool Supporting { get; }
}

sealed class SyntheticCase
{
    public SyntheticCase(
        string id, bool valid, bool authenticated, string contentClass,
        string expectedG4State, string actualG4State, string expectedRoute,
        string actualRoute, bool expectedAdverse, bool actualAdverse,
        bool actualSupporting, bool passed) =>
        (Id, Valid, Authenticated, ContentClass, ExpectedG4State, ActualG4State,
            ExpectedRoute, ActualRoute, ExpectedAdverse, ActualAdverse,
            ActualSupporting, Passed) =
        (id, valid, authenticated, contentClass, expectedG4State, actualG4State,
            expectedRoute, actualRoute, expectedAdverse, actualAdverse,
            actualSupporting, passed);

    public string Id { get; }
    public bool Valid { get; }
    public bool Authenticated { get; }
    public string ContentClass { get; }
    public string ExpectedG4State { get; }
    public string ActualG4State { get; }
    public string ExpectedRoute { get; }
    public string ActualRoute { get; }
    public bool ExpectedAdverse { get; }
    public bool ActualAdverse { get; }
    public bool ActualSupporting { get; }
    public bool Passed { get; }
}

using System.Security.Cryptography;
using System.Text.Json;

const string ContractPath = "studies/phase531_o4_g4_disposition_resolution_semantics_001/preregistration/phase531_o4_g4_disposition_resolution_semantics_contract_v1.json";
const string OutputPath = "studies/phase531_o4_g4_disposition_resolution_semantics_001/output/o4_g4_disposition_resolution_semantics.json";
const string SummaryPath = "studies/phase531_o4_g4_disposition_resolution_semantics_001/output/o4_g4_disposition_resolution_semantics_summary.json";

using JsonDocument contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var bindings = contract.GetProperty("exactBindings").EnumerateArray().Select(row =>
{
    string id = row.GetProperty("id").GetString()!;
    string path = row.GetProperty("path").GetString()!;
    string expected = row.GetProperty("sha256").GetString()!;
    string actual = File.Exists(path) ? Sha256(path) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}).ToArray();

string[] expectedBindingIds =
[
    "phase530-program", "phase530-contract", "phase530-full", "phase530-summary",
    "o4-schema", "o4-dependency-map",
];
bool bindingInventoryValid = bindings.Select(x => x.Id).SequenceEqual(expectedBindingIds, StringComparer.Ordinal)
    && bindings.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == expectedBindingIds.Length
    && bindings.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == expectedBindingIds.Length;
bool exactBindingsValid = bindingInventoryValid && bindings.All(x => x.HashMatches);
bool phase530MirrorsValid = exactBindingsValid
    && bindings.Single(x => x.Id == "phase530-full").ActualSha256
        == bindings.Single(x => x.Id == "phase530-summary").ActualSha256;

string[] expectedPrecedence =
[
    "invalid-authentication", "invalid-ruling-set", "unresolved-content",
    "resolved-adverse", "resolved-supporting",
];
string[] expectedSupport = ["accept-at-declared-scope", "accept-with-registered-caveats"];
string[] expectedAdverse = ["reject"];
string[] expectedUnresolved = ["insufficient-basis", "not-applicable"];
string[] expectedCases =
[
    "all-defer", "one-defer", "all-supporting", "one-adverse", "mixed-adverse",
    "duplicate-ruling", "missing-ruling", "invalid-authentication", "not-applicable-nonsupport",
];
string[] expectedFirewallKeys =
[
    "humanOptionSelectionAllowed", "humanRationaleInterpretationAllowed", "humanRulingAuthorshipAllowed",
    "o4Satisfied", "phase458G3Satisfied", "phase458G4Satisfied", "phase458G5Satisfied",
    "phase458EvaluationAllowed", "phase481ConstructionOrMutationAllowed",
    "samplingOrReprocessingAllowed", "hmcOrBenchmarkAllowed", "productionOrLaunchAllowed",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
];
bool contractValid =
    contract.GetProperty("contractId").GetString() == "phase531-a21-o4-g4-disposition-resolution-semantics-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A21"
    && contract.GetProperty("frozenBeforeSyntheticEvaluation").GetBoolean()
    && Strings(contract, "taxonomyPrecedence").SequenceEqual(expectedPrecedence)
    && Strings(contract, "supportingDispositions").SequenceEqual(expectedSupport)
    && Strings(contract, "adverseDispositions").SequenceEqual(expectedAdverse)
    && Strings(contract, "unresolvedDispositions").SequenceEqual(expectedUnresolved)
    && contract.GetProperty("deferOption").GetString() == "defer"
    && Strings(contract, "requiredSyntheticCases").SequenceEqual(expectedCases)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

using JsonDocument schemaDocument = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "o4-schema").Path));
using JsonDocument mapDocument = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "o4-dependency-map").Path));
JsonElement schema = schemaDocument.RootElement;
JsonElement map = mapDocument.RootElement;
string[] schemaRulingIds = schema.GetProperty("$defs").GetProperty("ruling").GetProperty("properties")
    .GetProperty("rulingId").GetProperty("enum").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] mapRulingIds = map.GetProperty("rulingIds").EnumerateArray()
    .Select(x => x.GetProperty("id").GetString()!).ToArray();
string[] schemaDispositions = schema.GetProperty("$defs").GetProperty("disposition").GetProperty("enum")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string schemaText = File.ReadAllText(bindings.Single(x => x.Id == "o4-schema").Path);
bool unchangedO4SurfacesValid = schemaRulingIds.Length == 13
    && schemaRulingIds.SequenceEqual(mapRulingIds, StringComparer.Ordinal)
    && expectedSupport.Concat(expectedAdverse).Concat(expectedUnresolved)
        .All(schemaDispositions.Contains);

bool phase530IdentityValid = false;
if (exactBindingsValid)
{
    using JsonDocument p530Document = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase530-summary").Path));
    JsonElement p530 = p530Document.RootElement;
    phase530IdentityValid = p530.TryGetProperty("phase", out JsonElement phase) && phase.GetInt32() == 530
        && p530.TryGetProperty("inputsValid", out JsonElement inputs) && inputs.GetBoolean()
        && p530.GetProperty("verdictKind").GetString() == "authentication-only-g4-predicate-counterexample"
        && p530.TryGetProperty("authenticationAdmissibilityCounterexample", out JsonElement defect) && defect.GetBoolean()
        && !p530.GetProperty("audit").GetProperty("phase480AuthenticationInvalidated").GetBoolean()
        && p530.GetProperty("audit").GetProperty("futureConsumerCorrectionRequired").GetBoolean()
        && !p530.GetProperty("audit").GetProperty("scientificResolutionSemanticsDefinedByThisPhase").GetBoolean()
        && !p530.GetProperty("humanMemoConsumed").GetBoolean()
        && !p530.GetProperty("phase458G4Satisfied").GetBoolean();
}

var supportingOptions = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["O4-F1-INVARIANT-RAYS"] = "translation-invariant-rays-valid-at-declared-constant-field-scope",
    ["O4-F1-COLLECTIVE-COORDINATE"] = "gauge-invariant-ray-coordinate-admissible",
    ["O4-F1-FP-NORMALIZATION"] = "within-member-only-caveat-sufficient",
    ["O4-F2-POSITIVE-MODE-IR"] = "positive-nonzero-subspace-diagnostic-only",
    ["O4-F3-THETA-HAAR"] = "unit-quaternion-haar-axis-angle-boundary-admissible",
    ["O4-F4-SADDLE-BACKGROUNDS"] = "local-diagnostic-only",
    ["O4-E1-P447-SOFT-FLOOR"] = "default-1e-4-plus-sweep-diagnostic-only",
    ["O4-E2-P453-UNIFORM-LADDER"] = "uniform-stiffness-required",
    ["O4-P455-ZERO-MODE"] = "Za-symmetric-k0-exclusion",
    ["O4-P455-SB-MODEL"] = "accept-w430-as-phase455-workbench-only",
    ["O4-C1-COMPACT-REAL-FORM"] = "phase467-direct-so64-arm-discharges-finite-scope",
    ["O4-C2-YHALF-BOOKKEEPING"] = "accept-phase404-lepton-doublet-normalization-as-bookkeeping-only",
    ["O4-C3-WS3-MPROBE-SCOPE"] = "accept-labeled-probe-only-no-lineage",
};
var adverseOptions = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["O4-F1-INVARIANT-RAYS"] = "neither-admissible",
    ["O4-F1-COLLECTIVE-COORDINATE"] = "inadmissible",
    ["O4-F3-THETA-HAAR"] = "inadmissible",
};
bool syntheticOptionPinsValid = supportingOptions.Keys.SequenceEqual(mapRulingIds, StringComparer.Ordinal)
    && supportingOptions.Values.Concat(adverseOptions.Values).Distinct(StringComparer.Ordinal)
        .All(option => schemaText.Contains($"\"{option}\"", StringComparison.Ordinal));
Ruling[] AllDefer() => mapRulingIds.Select(id => new Ruling(id, "insufficient-basis", "defer")).ToArray();
Ruling[] AllSupporting() => mapRulingIds.Select(id => new Ruling(id, "accept-at-declared-scope", supportingOptions[id])).ToArray();
Ruling[] Replace(Ruling[] rows, int index, string disposition, string option)
{
    Ruling[] copy = rows.ToArray();
    copy[index] = copy[index] with { Disposition = disposition, SelectedOption = option };
    return copy;
}

Ruling[] allSupporting = AllSupporting();
Ruling[] allDefer = AllDefer();
Ruling[] mixedAdverse = allSupporting.ToArray();
mixedAdverse[0] = mixedAdverse[0] with { Disposition = "reject", SelectedOption = adverseOptions[mixedAdverse[0].RulingId] };
mixedAdverse[1] = mixedAdverse[1] with { Disposition = "reject", SelectedOption = adverseOptions[mixedAdverse[1].RulingId] };
mixedAdverse[2] = mixedAdverse[2] with { Disposition = "accept-with-registered-caveats" };
Ruling[] duplicate = allSupporting.Concat([allSupporting[0]]).ToArray();
Ruling[] missing = allSupporting.SkipLast(1).ToArray();

var battery = new[]
{
    Test("all-defer", true, allDefer, "unresolved-content", false, false, true),
    Test("one-defer", true, Replace(allSupporting, 0, "insufficient-basis", "defer"), "unresolved-content", false, false, true),
    Test("all-supporting", true, allSupporting, "resolved-supporting", true, false, false),
    Test("one-adverse", true, Replace(allSupporting, 0, "reject", adverseOptions[allSupporting[0].RulingId]), "resolved-adverse", false, true, false),
    Test("mixed-adverse", true, mixedAdverse, "resolved-adverse", false, true, false),
    Test("duplicate-ruling", true, duplicate, "invalid-ruling-set", false, false, false),
    Test("missing-ruling", true, missing, "invalid-ruling-set", false, false, false),
    Test("invalid-authentication", false, allSupporting, "invalid-authentication", false, false, false),
    Test("not-applicable-nonsupport", true, Replace(allSupporting, 0, "not-applicable", allSupporting[0].SelectedOption), "unresolved-content", false, false, true),
};
bool batteryInventoryValid = battery.Select(x => x.Id).SequenceEqual(expectedCases, StringComparer.Ordinal);
bool batteryPassed = batteryInventoryValid && battery.All(x => x.Passed);
bool allDeferRemainsUnresolved = battery.Single(x => x.Id == "all-defer").Actual == "unresolved-content";
bool oneDeferRemainsUnresolved = battery.Single(x => x.Id == "one-defer").Actual == "unresolved-content";
bool allSupportingDistinguished = battery.Single(x => x.Id == "all-supporting").FullyResolvedSupporting;
bool oneAdverseDistinguished = battery.Single(x => x.Id == "one-adverse").AdversePresent;
bool mixedAdversePreserved = battery.Single(x => x.Id == "mixed-adverse").AdversePresent;
bool notApplicableNeverSupports = !battery.Single(x => x.Id == "not-applicable-nonsupport").FullyResolvedSupporting;

bool inputsValid = contractValid && exactBindingsValid && phase530MirrorsValid
    && unchangedO4SurfacesValid && phase530IdentityValid && syntheticOptionPinsValid;
string verdict = inputsValid && batteryPassed
    ? "resolution-semantics-closed-current-g4-missing"
    : "invalid-or-drifted-input";

var result = new
{
    schemaVersion = 1,
    phase = 531,
    phaseId = "phase531-o4-g4-disposition-resolution-semantics",
    contractId = contract.GetProperty("contractId").GetString(),
    planSection = contract.GetProperty("planSection").GetString(),
    inputsValid,
    contractValid,
    bindingInventoryValid,
    exactBindingsValid,
    phase530MirrorsValid,
    phase530IdentityValid,
    unchangedO4SurfacesValid,
    syntheticOptionPinsValid,
    rulingCount = mapRulingIds.Length,
    rulingIds = mapRulingIds,
    taxonomy = new
    {
        precedence = expectedPrecedence,
        supportingDispositions = expectedSupport,
        adverseDispositions = expectedAdverse,
        unresolvedDispositions = expectedUnresolved,
        deferOption = "defer",
        insufficientBasisOrDeferIsUnresolved = true,
        notApplicableCountsAsSupport = false,
        adverseFlagSurvivesMixedContent = true,
    },
    batteryInventoryValid,
    batteryPassed,
    syntheticBattery = battery,
    allDeferRemainsUnresolved,
    oneDeferRemainsUnresolved,
    allSupportingDistinguished,
    oneAdverseDistinguished,
    mixedAdversePreserved,
    notApplicableNeverSupports,
    verdictKind = verdict,
    terminalStatus = "o4-g4-disposition-resolution-semantics-" + verdict,
    currentState = new
    {
        phase480AuthenticatedMemoPresent = false,
        humanRulingConsumed = false,
        phase458G4State = "missing",
        phase458G4Satisfied = false,
        externalReviewPending = true,
    },
    decision = verdict == "resolution-semantics-closed-current-g4-missing"
        ? "Generic authentication, completeness, resolution, and adversity semantics are frozen without selecting or interpreting a human ruling. Current Phase458 G4 remains missing."
        : "The Phase531 semantics contract or an exact-bound input is invalid or drifted; no consumer may use this output.",
    bindings,
    deterministicZeroPhysicsComputation = true,
    syntheticCasesAreHumanEvidence = false,
    humanOptionSelected = false,
    humanRationaleInterpreted = false,
    humanRulingAuthored = false,
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
    externalReviewPending = true,
    promotedPhysicalMassClaimCount = 0,
};

Require(contractValid, "Phase531 frozen contract invalid.");
Require(inputsValid, "Phase531 exact-bound inputs invalid or drifted.");
Require(batteryPassed, "Phase531 synthetic resolution battery failed.");
Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase531 verdict differs from frozen expected branch.");
Require("o4-g4-disposition-resolution-semantics-" + verdict == contract.GetProperty("expectedCurrentTerminalStatus").GetString(), "Phase531 terminal drifted.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase531 verdict: {verdict}");
Console.WriteLine($"synthetic resolution battery: {battery.Count(x => x.Passed)}/{battery.Length}");
Console.WriteLine("current Phase458 G4: missing");
Console.WriteLine("promotedPhysicalMassClaimCount=0");

static Resolution Classify(bool authenticationValid, IReadOnlyCollection<string> expectedIds, IReadOnlyCollection<Ruling> rulings)
{
    if (!authenticationValid) return new("invalid-authentication", false, false, false, false, false);
    bool completeUnique = rulings.Count == expectedIds.Count
        && rulings.Select(x => x.RulingId).Distinct(StringComparer.Ordinal).Count() == expectedIds.Count
        && rulings.Select(x => x.RulingId).OrderBy(x => x, StringComparer.Ordinal)
            .SequenceEqual(expectedIds.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal);
    if (!completeUnique) return new("invalid-ruling-set", false, false, false, false, false);

    bool adverse = rulings.Any(x => x.Disposition == "reject");
    bool unresolved = rulings.Any(x => x.Disposition is "insufficient-basis" or "not-applicable" || x.SelectedOption == "defer");
    bool allSupporting = rulings.All(x => x.Disposition is "accept-at-declared-scope" or "accept-with-registered-caveats") && !unresolved;
    if (unresolved) return new("unresolved-content", true, false, adverse, true, false);
    if (adverse) return new("resolved-adverse", true, false, true, false, true);
    if (allSupporting) return new("resolved-supporting", true, true, false, false, true);
    return new("unresolved-content", true, false, adverse, true, false);
}

BatteryCase Test(string id, bool authenticationValid, Ruling[] rulings, string expected,
    bool expectedSupporting, bool expectedAdverse, bool expectedUnresolved)
{
    Resolution actual = Classify(authenticationValid, mapRulingIds, rulings);
    bool passed = actual.Kind == expected
        && actual.FullyResolvedSupporting == expectedSupporting
        && actual.AdversePresent == expectedAdverse
        && actual.Unresolved == expectedUnresolved;
    return new(id, expected, actual.Kind, actual.RulingSetCompleteUnique,
        actual.FullyResolvedSupporting, actual.AdversePresent, actual.Unresolved,
        actual.ScientificallyResolved, passed);
}

static string[] Strings(JsonElement parent, string property) => parent.GetProperty(property)
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record Ruling(string RulingId, string Disposition, string SelectedOption);
sealed record Resolution(string Kind, bool RulingSetCompleteUnique, bool FullyResolvedSupporting,
    bool AdversePresent, bool Unresolved, bool ScientificallyResolved);
sealed record BatteryCase(string Id, string Expected, string Actual, bool RulingSetCompleteUnique,
    bool FullyResolvedSupporting, bool AdversePresent, bool Unresolved, bool ScientificallyResolved, bool Passed);

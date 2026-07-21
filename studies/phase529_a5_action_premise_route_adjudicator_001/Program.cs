using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase529_a5_action_premise_route_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase529_a5_action_premise_route_adjudicator_contract_v1.json";
const string OutputPath = Root + "/output/a5_action_premise_route_adjudicator.json";
const string SummaryPath = Root + "/output/a5_action_premise_route_adjudicator_summary.json";

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

string[] phases = ["phase524", "phase525", "phase526", "phase527", "phase528"];
string[] kinds = ["program", "contract", "full", "summary"];
string[] expectedIds = phases.SelectMany(phase => kinds.Select(kind => $"{phase}-{kind}")).ToArray();
bool exactBindingInventoryValid = bindings.Length == 20
    && bindings.Select(x => x.Id).SequenceEqual(expectedIds, StringComparer.Ordinal)
    && bindings.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 20
    && bindings.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 20;
bool exactBindingsValid = exactBindingInventoryValid && bindings.All(x => x.HashMatches);
bool fullSummaryMirrorsValid = phases.All(phase =>
    bindings.Single(x => x.Id == $"{phase}-full").ActualSha256
        == bindings.Single(x => x.Id == $"{phase}-summary").ActualSha256);

string[] expectedPrecedence =
[
    "invalid-or-drifted-input", "exact-reduction-unavailable",
    "member-scoped-premise-status-unresolved",
    "current-a5-route-inapplicable-to-registered-action-family",
    "action-premise-survives-geometry-next",
];
string[] expectedFirewallKeys =
[
    "actionMemberSelectionAllowed", "actionOrGeometryRegistrationAllowed",
    "phase515Or516UnlockAllowed", "phase458EvaluationOrG1Allowed",
    "limbL8GeneralClosureAllowed", "generalTheoremOrTargetCounterexampleAllowed",
    "reflectionPositivityRulingAllowed", "samplingOrReprocessingAllowed",
    "hmcOrBenchmarkAllowed", "productionOrLaunchAllowed",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
];
bool contractValid =
    contract.GetProperty("contractId").GetString() == "phase529-a20-dependent-action-premise-route-adjudicator-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A20"
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedPrecedence)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

var documents = new List<JsonDocument>();
JsonElement Load(string id)
{
    JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == id).Path));
    documents.Add(document);
    return document.RootElement;
}

try
{
    JsonElement p524 = Load("phase524-summary");
    JsonElement p525 = Load("phase525-summary");
    JsonElement p526 = Load("phase526-summary");
    JsonElement p527 = Load("phase527-summary");
    JsonElement p528 = Load("phase528-summary");

    bool precursorIdentityValid =
        p524.GetProperty("phase").GetInt32() == 524
        && p524.GetProperty("inputsValid").GetBoolean()
        && p524.GetProperty("verdictKind").GetString() == "exact-omega-parity-refuted-identity-member"
        && p525.GetProperty("phase").GetInt32() == 525
        && p525.GetProperty("inputsValid").GetBoolean()
        && p525.GetProperty("verdictKind").GetString() == "finite-dual-survivor-pullback-boundary-closed"
        && p526.GetProperty("phase").GetInt32() == 526
        && p526.GetProperty("inputsValid").GetBoolean()
        && p526.GetProperty("verdictKind").GetString() == "unresolved-member-dependence-for-named-a5-prerequisites"
        && p527.GetProperty("phase").GetInt32() == 527
        && p527.GetProperty("inputsValid").GetBoolean()
        && p527.GetProperty("verdictKind").GetString() == "exact-sd2-theta-zero-odd-witness"
        && p528.GetProperty("phase").GetInt32() == 528
        && p528.GetProperty("inputsValid").GetBoolean()
        && p528.GetProperty("verdictKind").GetString() == "both-registered-members-outside-frozen-even-sector-premise";

    bool exactReductionAvailable =
        p524.GetProperty("exactIdentityReductionCertified").GetBoolean()
        && p527.GetProperty("exactReductionAvailable").GetBoolean()
        && p527.GetProperty("exactRationalArithmeticOnly").GetBoolean()
        && !p527.GetProperty("binary64ReplayUsedAsEvidence").GetBoolean();
    string[] p528Members = p528.GetProperty("registeredMemberIds").EnumerateArray()
        .Select(x => x.GetString()!).ToArray();
    bool completeMemberStatus = p528Members.SequenceEqual(new[] { "identity", "sd2-id0/c0.5" }, StringComparer.Ordinal)
        && p528.GetProperty("identityOutsidePremise").GetBoolean()
        && p528.GetProperty("sd2OutsidePremise").GetBoolean();
    bool everyRegisteredMemberOutside = completeMemberStatus
        && p528.GetProperty("bothRegisteredMembersOutsideFrozenEvenSectorPremise").GetBoolean()
        && p528.GetProperty("premiseAudit").GetProperty("currentPackApplicability").GetString()
            == "inapplicable-to-every-registered-member";
    bool finiteGeometryResultRetained =
        p525.GetProperty("finiteOnly").GetBoolean()
        && p525.GetProperty("positiveResultScope").GetString() == "finite-combinatorial-chain-and-carrier-closure-only"
        && !p525.GetProperty("actionCovarianceEvaluated").GetBoolean()
        && !p525.GetProperty("actionCarrierTransformationEstablished").GetBoolean();

    bool inputsValid = contractValid && exactBindingsValid && fullSummaryMirrorsValid && precursorIdentityValid;
    string verdict = EvaluateVerdict(!inputsValid, exactReductionAvailable, completeMemberStatus, everyRegisteredMemberOutside);
    var precedenceBattery = new[]
    {
        Case("invalid", true, true, true, true, "invalid-or-drifted-input"),
        Case("reduction-missing", false, false, true, true, "exact-reduction-unavailable"),
        Case("member-missing", false, true, false, false, "member-scoped-premise-status-unresolved"),
        Case("all-outside", false, true, true, true, "current-a5-route-inapplicable-to-registered-action-family"),
        Case("premise-survives", false, true, true, false, "action-premise-survives-geometry-next"),
        Case("duplicate-member-equivalent", false, true, false, true, "member-scoped-premise-status-unresolved"),
    };
    bool precedenceBatteryPassed = precedenceBattery.All(x => x.Passed);

    var result = new
    {
        schemaVersion = 1,
        phase = 529,
        phaseId = "phase529-a5-action-premise-route-adjudicator",
        contractId = contract.GetProperty("contractId").GetString(),
        planSection = contract.GetProperty("planSection").GetString(),
        inputsValid,
        contractValid,
        exactBindingInventoryValid,
        exactBindingsValid,
        fullSummaryMirrorsValid,
        precursorIdentityValid,
        exactReductionAvailable,
        completeMemberStatus,
        everyRegisteredMemberOutside,
        finiteGeometryResultRetained,
        precedenceBatteryPassed,
        precedenceBattery,
        verdictKind = verdict,
        terminalStatus = "a5-action-premise-route-adjudicator-" + verdict,
        adjudication = new
        {
            registeredFamily = new[] { "identity", "sd2-id0/c0.5" },
            registeredFamilyCoverageComplete = completeMemberStatus,
            identityPremiseStatus = "outside-by-exact-full-action-witness-difference-1",
            sd2PremiseStatus = "outside-by-exact-theta-zero-full-action-witness-difference-13/180",
            currentA5PackRouteClosedAtActionPremise = verdict == "current-a5-route-inapplicable-to-registered-action-family",
            geometryWorkIsNextForCurrentA5Pack = verdict == "action-premise-survives-geometry-next",
            phase525FiniteResultRetained = finiteGeometryResultRetained,
            reason = "A dependent proof route cannot be repaired by later geometry evidence when its earlier frozen exact-evenness premise is false for every registered action-family member.",
        },
        decision = verdict == "current-a5-route-inapplicable-to-registered-action-family"
            ? "The current frozen A5 pack route is inapplicable to the complete registered action family at its exact-evenness premise. Further all-volume geometry work is not the next step for this pack. Alternative actions or analytic routes require a new frozen phase."
            : "The action-premise route is not closed for the complete registered family under this adjudicator.",
        scopeFirewalls = new
        {
            currentPackOnly = true,
            generalGaussianDominationNoGoClaimed = false,
            alternativeActionOrRouteRuledOut = false,
            generalActionTheoremClaimed = false,
            targetTheoremCounterexampleClaimed = false,
            reflectionPositivityEstablished = false,
            reflectionPositivityRefuted = false,
            phase525FiniteGeometryInvalidated = false,
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
        limbL8GenerallyClosed = false,
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

    Require(contractValid, "Phase529 frozen contract invalid.");
    Require(inputsValid, "Phase529 inputs invalid or drifted.");
    Require(precedenceBatteryPassed, "Phase529 precedence battery failed.");
    Require(finiteGeometryResultRetained, "Phase529 failed to retain Phase525's finite-only result.");
    Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase529 verdict differs from frozen expected branch.");
    Require("a5-action-premise-route-adjudicator-" + verdict == contract.GetProperty("expectedCurrentTerminalStatus").GetString(), "Phase529 terminal drifted.");

    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
    File.WriteAllBytes(OutputPath, json);
    File.WriteAllBytes(SummaryPath, json);
    Console.WriteLine($"Phase529 verdict: {verdict}");
    Console.WriteLine("current A5 pack route closed at action premise: " + (verdict == "current-a5-route-inapplicable-to-registered-action-family"));
}
finally
{
    foreach (JsonDocument document in documents) document.Dispose();
}

static string EvaluateVerdict(bool invalid, bool reductionAvailable, bool memberStatusComplete, bool everyMemberOutside) =>
    invalid ? "invalid-or-drifted-input" :
    !reductionAvailable ? "exact-reduction-unavailable" :
    !memberStatusComplete ? "member-scoped-premise-status-unresolved" :
    everyMemberOutside ? "current-a5-route-inapplicable-to-registered-action-family" :
    "action-premise-survives-geometry-next";

static BatteryCase Case(string id, bool invalid, bool reductionAvailable, bool memberStatusComplete, bool everyMemberOutside, string expected)
{
    string actual = EvaluateVerdict(invalid, reductionAvailable, memberStatusComplete, everyMemberOutside);
    return new BatteryCase(id, expected, actual, actual == expected);
}

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record BatteryCase(string Id, string Expected, string Actual, bool Passed);

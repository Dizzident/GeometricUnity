using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase479 freezes post-ruling readiness. It consumes no ruling and runs no Arm Q/Stage B.
var stopwatch = Stopwatch.StartNew();
const string slug = "phase457_post_ruling_readiness";
const string outputDir = "studies/phase479_phase457_post_ruling_readiness_001/output";
const string contractPath = "studies/phase479_phase457_post_ruling_readiness_001/preregistration/phase457_post_ruling_contract_v1.json";
const string phase457Path = "studies/phase457_upsilon_portal_stage_a_001/output/upsilon_portal_stage_a_summary.json";
const string phase477Path = "studies/phase477_o4_adjudication_infrastructure_001/output/o4_adjudication_infrastructure_summary.json";
const string phase480Path = "studies/phase480_o4_physicist_adjudication_intake_001/output/o4_physicist_adjudication_intake_summary.json";

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string Sha256Text(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
static string S(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString()! : "";
static bool B(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.True;
static int I(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.TryGetInt32(out var v) ? v : int.MinValue;
static string[] Strings(JsonElement e, string name) => e.GetProperty(name).EnumerateArray().Select(x => x.GetString()!).ToArray();

using var contractDoc = JsonDocument.Parse(File.ReadAllText(contractPath));
using var phase457Doc = JsonDocument.Parse(File.ReadAllText(phase457Path));
using var phase477Doc = JsonDocument.Parse(File.ReadAllText(phase477Path));
using var phase480Doc = JsonDocument.Parse(File.ReadAllText(phase480Path));
var contract = contractDoc.RootElement;
var p457 = phase457Doc.RootElement;
var p477 = phase477Doc.RootElement;
var p480 = phase480Doc.RootElement;
var phase466Pin = contract.GetProperty("phase466Pin");
string phase466Path = S(phase466Pin, "summaryPath");
using var phase466Doc = JsonDocument.Parse(File.ReadAllText(phase466Path));
var p466 = phase466Doc.RootElement;
var schema466 = p466.GetProperty("schema");

bool phase466PinSatisfied =
    S(p466, "phaseId") == S(phase466Pin, "requiredPhaseId") &&
    S(p466, "terminalStatus") == "ws3-vev-completion-contract-schema-committed" &&
    S(p466, "verdictKind") == "schema-committed" &&
    B(p466, "ws3VevCompletionContractSchemaCommitted") &&
    S(schema466, "schemaId") == S(phase466Pin, "schemaId") &&
    S(schema466, "schemaHash") == S(phase466Pin, "schemaHash") &&
    Sha256Text(S(schema466, "schemaCanonicalPreimage")) == S(schema466, "schemaHash") &&
    B(schema466.GetProperty("pins"), "phase434HashMatches") &&
    B(schema466.GetProperty("pins"), "templateIdMatches") &&
    B(schema466.GetProperty("partition"), "partitionMatches") &&
    I(schema466.GetProperty("partition"), "extractionRowCount") == 6 &&
    I(schema466.GetProperty("partition"), "conditionalRowCount") == 7 &&
    I(schema466.GetProperty("partition"), "amplitudeBlockedRowCount") == 4 &&
    I(schema466.GetProperty("partition"), "lineageBlockedRowCount") == 9 &&
    I(schema466.GetProperty("partition"), "templateFieldCount") == 20 &&
    B(schema466.GetProperty("fixtureBattery"), "fixtureSetComplete") &&
    B(schema466.GetProperty("fixtureBattery"), "allFixturesPassed") &&
    B(schema466, "o8CapAssertedOnAllLineage") &&
    schema466.GetProperty("lineageImpossibilityCap").GetArrayLength() == 9 &&
    schema466.GetProperty("lineageImpossibilityCap").EnumerateArray().All(x => B(x, "wsThreeCannotComplete")) &&
    schema466.GetProperty("o5Guard").GetProperty("sigmaThreshold").GetDouble() == 3.0 &&
    I(schema466, "templateMutationCount") == 0 && !B(schema466, "phase256TemplateMutated") &&
    B(p466, "noGevPromotion") && !B(p466, "sourceContractApplicationAllowed");

var rulingContract = contract.GetProperty("rulingConsumption");
bool memoBindingPathsExist =
    File.Exists("scripts/o4_register/o4_human_memo_schema_v1.json") &&
    File.Exists("scripts/o4_register/coverage_contract.json") &&
    File.Exists("scripts/o4_register/dependency_map.json") &&
    S(rulingContract, "requiredMemoSchemaSha256") == Sha256("scripts/o4_register/o4_human_memo_schema_v1.json");
bool phase477InfrastructureGreen = B(p477, "infrastructureReady") && B(p477, "exactCoveragePassed") &&
    B(p477, "coverageSchemaTemplateRulingIdsEqual");

static bool C3DerivativeAccepted(JsonElement root, JsonElement rulingContract)
{
    bool envelopeGreen =
        S(root, "phaseId") == S(rulingContract, "requiredPhaseId") &&
        S(root, "terminalStatus") == S(rulingContract, "requiredTerminalStatus") &&
        B(root, "externalMemoValidated") && B(root, "humanAuthorshipValidated") &&
        B(root, "repositoryBindingValidated") && B(root, "signedPayloadHashValidated") &&
        B(root, "signatureProvenanceValidated") &&
        S(root, "signatureVerificationStatus") is "cryptographically-verified" or "witnessed-document-verified" &&
        S(root, "memoSchemaSha256") == S(rulingContract, "requiredMemoSchemaSha256") &&
        !B(root, "syntheticOrTemplateInput") && !B(root, "rulingContentMachineAuthoredOrInferred");
    if (!envelopeGreen || !root.TryGetProperty("normalizedRulings", out var rulings) || rulings.ValueKind != JsonValueKind.Array) return false;
    var c3 = rulings.EnumerateArray().Where(r => S(r, "rulingId") == S(rulingContract, "requiredRulingId")).ToArray();
    if (c3.Length != 1) return false;
    var ruling = c3[0];
    var acceptedDispositions = Strings(rulingContract, "acceptedDispositions");
    if (!acceptedDispositions.Contains(S(ruling, "disposition")) || S(ruling, "selectedOption") != S(rulingContract, "requiredSelectedOption")) return false;
    if (!ruling.TryGetProperty("scopeAssertions", out var scope)) return false;
    return !B(scope, "mIsSourceDefined") && B(scope, "singletGapOnly") && B(scope, "noNamedScalarIdentification") && B(scope, "noLineageFill") && B(scope, "latticeUnitsOnly");
}

bool c3DerivativeAccepted = C3DerivativeAccepted(p480, rulingContract);
bool phase457LegacyInterfaceRetired =
    p457.GetProperty("firewall").TryGetProperty("legacyCandidatePathsAccepted", out var oldPaths) && oldPaths.ValueKind == JsonValueKind.False &&
    p457.GetProperty("firewall").TryGetProperty("legacyTwoFieldShapeAccepted", out var oldShape) && oldShape.ValueKind == JsonValueKind.False &&
    S(p457.GetProperty("firewall"), "phase480DerivativePath") == S(rulingContract, "onlySourcePath");
bool verdictFirewallConjunctionSatisfied = phase466PinSatisfied && c3DerivativeAccepted;

// Synthetic derivative battery. These fixtures exist only in memory and never count as rulings.
object MakeDerivative(string disposition = "accept-at-declared-scope", string option = "accept-labeled-probe-only-no-lineage",
    bool scopeGreen = true, bool signatureGreen = true, bool synthetic = false) => new
{
    phaseId = "phase480-o4-physicist-adjudication-intake",
    terminalStatus = "o4-physicist-adjudication-intake-validated-external-physicist-ruling",
    externalMemoValidated = true,
    humanAuthorshipValidated = true,
    repositoryBindingValidated = true,
    signedPayloadHashValidated = true,
    signatureProvenanceValidated = true,
    signatureVerificationStatus = signatureGreen ? "cryptographically-verified" : "shape-valid-signature-unverified",
    memoSchemaSha256 = S(rulingContract, "requiredMemoSchemaSha256"),
    syntheticOrTemplateInput = synthetic,
    rulingContentMachineAuthoredOrInferred = false,
    normalizedRulings = new[]
    {
        new
        {
            rulingId = "O4-C3-WS3-MPROBE-SCOPE",
            disposition,
            selectedOption = option,
            scopeAssertions = new { mIsSourceDefined = !scopeGreen, singletGapOnly = true, noNamedScalarIdentification = true, noLineageFill = true, latticeUnitsOnly = true },
        },
    },
};
bool Eval(object fixture)
{
    using var doc = JsonDocument.Parse(JsonSerializer.Serialize(fixture));
    return C3DerivativeAccepted(doc.RootElement, rulingContract);
}
var legacyFixture = new { wsThreeMProbeScopeSignedOff = true, signer = "synthetic" };
bool rulingBatteryPassed =
    !Eval(legacyFixture) && Eval(MakeDerivative()) &&
    !Eval(MakeDerivative(disposition: "reject")) &&
    !Eval(MakeDerivative(option: "defer")) &&
    !Eval(MakeDerivative(scopeGreen: false)) &&
    !Eval(MakeDerivative(signatureGreen: false)) &&
    !Eval(MakeDerivative(synthetic: true));

bool phase466MutationBatteryPassed =
    phase466PinSatisfied &&
    !SchemaPin("wrong", S(schema466, "schemaHash"), S(schema466, "schemaCanonicalPreimage")) &&
    !SchemaPin(S(schema466, "schemaId"), new string('0', 64), S(schema466, "schemaCanonicalPreimage")) &&
    !SchemaPin(S(schema466, "schemaId"), S(schema466, "schemaHash"), S(schema466, "schemaCanonicalPreimage") + "tamper");
bool SchemaPin(string id, string hash, string preimage) =>
    id == S(phase466Pin, "schemaId") && hash == S(phase466Pin, "schemaHash") && Sha256Text(preimage) == hash;

var armQ = contract.GetProperty("armQ");
var gq2 = armQ.GetProperty("G-Q2");
var gq3 = armQ.GetProperty("G-Q3");
bool armQContractExact =
    Strings(armQ, "launchGateOrder").SequenceEqual(new[] { "G-Q1", "G-Q2", "G-Q3" }) &&
    S(gq2, "requiredFormatId") == "phase457-arm-q-ensemble-v1" &&
    Strings(gq2, "requiredPerSampleFields").SequenceEqual(new[] { "S_B", "Phi", "M_squared" }) &&
    B(gq2, "thetaHaarMeasureUsed") && I(gq2, "environmentOverrideCount") == 0 &&
    !B(gq2, "phase456WindowReuseAllowed") && !B(gq2, "syntheticSelfTestCountsAsEnsemble") &&
    Strings(gq3, "allowedModes").SequenceEqual(new[] { "stream-neutrality", "fresh-independent" }) &&
    I(gq3.GetProperty("streamNeutrality"), "minimumAlignedBlockCount") == 20 &&
    gq3.GetProperty("streamNeutrality").GetProperty("sigmaThresholdInclusive").GetDouble() == 4.0 &&
    S(gq3.GetProperty("freshIndependent"), "requiredLabel") == "FRESH" &&
    !B(gq3.GetProperty("freshIndependent"), "piggybackAllowed") &&
    !B(armQ.GetProperty("actionCellGate"), "unboundedCellAllowed") &&
    !B(armQ.GetProperty("actionCellGate"), "tabledAllCellsUnboundedRelaxationAllowed") &&
    B(armQ.GetProperty("motivationPreregistration"), "requiredBeforeRealEnsembleGeneration") &&
    B(armQ.GetProperty("motivationPreregistration"), "currentPhase457IidStandardErrorMayNotBeUsedForRealHmc");

bool eligibleRealArmQEnsemblePresent = false;
bool rngNeutralityOrFreshProvenanceGreen = false;
bool motivationPreregistrationPresent = false;
bool armQReady = verdictFirewallConjunctionSatisfied && eligibleRealArmQEnsemblePresent &&
    rngNeutralityOrFreshProvenanceGreen && motivationPreregistrationPresent;

var teamC = contract.GetProperty("teamCCosignature");
bool teamCCosignatureSemanticsExact =
    B(teamC, "requiredOnlyForDeadlineTerminals") &&
    Strings(teamC, "deadlineTerminals").Length == 3 &&
    B(teamC, "deadlineNeverClosesL7") && B(teamC, "deadlineNeverSatisfiesPhase458G5") &&
    !B(teamC, "deadlineAuthorizesStageB") && !B(teamC, "deadlineFillsSourceContract") &&
    !B(teamC, "nonDeadlineScientificTerminalRequiresTeamCCosignature");
string DeadlineTerminal(bool deadlineRequested, bool validCosignature) => !deadlineRequested ? "not-deadline"
    : validCosignature ? "deadline-co-signed-l7-withheld" : S(teamC, "deadlineWithoutValidCosignatureTerminal");
bool teamCBatteryPassed =
    DeadlineTerminal(false, false) == "not-deadline" &&
    DeadlineTerminal(true, false) == "deadline-withheld-awaiting-team-c-cosignature" &&
    DeadlineTerminal(true, true) == "deadline-co-signed-l7-withheld";

bool contractClosed =
    contract.GetProperty("schemaVersion").GetInt32() == 1 &&
    S(contract, "contractId") == "phase457-post-ruling-contract-v1" &&
    phase466PinSatisfied && memoBindingPathsExist && phase477InfrastructureGreen &&
    phase457LegacyInterfaceRetired && rulingBatteryPassed && phase466MutationBatteryPassed &&
    armQContractExact && teamCCosignatureSemanticsExact && teamCBatteryPassed;
string terminal = contractClosed ? "post-ruling-readiness-contract-closed-hold-remains" : "post-ruling-readiness-contract-invalid-or-drifted";

var result = new
{
    phaseId = "phase479-phase457-post-ruling-readiness",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = $"phase457-post-ruling-readiness-{terminal}",
    interimTerminal = terminal,
    verdictKind = terminal,
    applicationSubjectKind = "phase457-post-ruling-readiness",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A4",
    waveOrder = 3,
    skeletonBuilt = false,
    zeroPhysicsCompute = true,
    awaitingImplementation = false,
    contractClosed,
    contractPath,
    contractSha256 = Sha256(contractPath),
    phase466 = new { summaryPath = phase466Path, summarySha256 = Sha256(phase466Path), schemaId = S(schema466, "schemaId"), schemaHash = S(schema466, "schemaHash"), canonicalPreimageHashRecomputed = Sha256Text(S(schema466, "schemaCanonicalPreimage")), phase466PinSatisfied },
    memoInterface = new
    {
        onlySourcePath = S(rulingContract, "onlySourcePath"),
        memoBindingPathsExist,
        requiredMemoSchemaSha256 = S(rulingContract, "requiredMemoSchemaSha256"),
        phase477InfrastructureGreen,
        phase457LegacyInterfaceRetired,
        legacyCandidatePathsAccepted = false,
        legacyTwoFieldShapeAccepted = false,
        rawMemoAccepted = false,
        riskAcceptanceCountsAsRuling = false,
        registerCurrencyCountsAsRuling = false,
        c3DerivativeAccepted,
        rulingConsumed = false,
        humanRulingReadOrAuthored = false,
    },
    hold = new
    {
        phase466SchemaConjunctSatisfied = phase466PinSatisfied,
        humanMProbeConjunctSatisfied = c3DerivativeAccepted,
        verdictFirewallConjunctionSatisfied,
        holdLifted = false,
        currentState = c3DerivativeAccepted ? "conjunction-satisfied-readiness-pending" : "closed-human-ruling-absent",
        tabledAllCellsUnboundedRelaxationUsed = false,
    },
    armQReadiness = new
    {
        armQContractExact,
        eligibleRealArmQEnsemblePresent,
        phase456WindowReuseAllowed = false,
        syntheticSelfTestCountsAsEnsemble = false,
        rngNeutralityOrFreshProvenanceGreen,
        motivationPreregistrationPresent,
        currentIidEstimatorEligibleForRealHmc = false,
        armQReady,
        armQRun = false,
        armQMeasurementRecorded = false,
        stageBPreregistrationPresent = false,
        stageBBuiltOrRun = false,
        portalVerdictEligible = false,
    },
    teamC = new
    {
        teamCCosignatureSemanticsExact,
        signerRegistryPresent = false,
        appliesOnlyToDeadlineTerminal = true,
        deadlineTerminalRequested = false,
        deadlineTerminalEmitted = false,
        deadlineClosesL7 = false,
    },
    batteries = new
    {
        rulingBatteryPassed,
        rulingCaseCount = 7,
        phase466MutationBatteryPassed,
        phase466CaseCount = 4,
        teamCBatteryPassed,
        teamCCaseCount = 3,
        canonicalArtifactMutationCount = 0,
        candidatePathWriteCount = 0,
    },
    stageBBuiltOrRun = false,
    holdLifted = false,
    l7Closure = "WITHHELD",
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    physicistReviewPending = true,
    scaleIsWorkbenchRelativeCandidateOnly = true,
    physicalCouplingProvided = false,
    routeProvidesPhysicalEffectiveActionHessian = false,
    routeProvidesVevOrSourceScaleLineage = false,
    routeProvidesPoleExtractionAndGeVNormalization = false,
    sourceContractApplicationAllowed = false,
    phase201TemplateMutated = false,
    fieldsAppliedToPhase201TemplateCount = 0,
    acceptedContractFieldCount = 0,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = "The Phase457 post-ruling interface is frozen and the unsafe legacy two-field parser is retired. Phase466 is exact, but no verified Phase480 C3 derivative, real Arm-Q ensemble, RNG/FRESH provenance, or motivation preregistration exists. The hold stays closed; no ruling is consumed and no Arm Q, Stage B, L7 closure, or claim is authorized.",
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};

Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, $"{slug}.json"), json);
File.WriteAllText(Path.Combine(outputDir, $"{slug}_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"contractClosed={contractClosed} phase466Pin={phase466PinSatisfied} c3Derivative={c3DerivativeAccepted} armQReady={armQReady}");
Console.WriteLine("holdLifted=False stageBBuiltOrRun=False promotedPhysicalMassClaimCount=0");

using System.Security.Cryptography;
using System.Text.Json;

const string phaseDir = "studies/phase507_phase456_selective_inference_pack_readiness_adjudicator_001";
const string phase503Path = "studies/phase503_phase456_adaptive_calibration_protocol_validation_001/output/phase456_adaptive_calibration_protocol_validation_summary.json";
const string phase505Path = "studies/phase505_phase503_frozen_failure_localization_001/output/phase503_frozen_failure_localization_summary.json";
const string phase506Path = "studies/phase506_phase456_selective_inference_protocol_validation_001/output/phase456_selective_inference_protocol_validation_summary.json";
const string expectedPhase503ByteSha256 = "9f5492b11f71cb2a62305837e68d0de6402f4d167f2199d33be31c62119e1d29";
const string expectedPhase505ByteSha256 = "956beedf3cb166f92529fa3f577962e905a88e715cff7be132a3661b6a3e5a61";
const string expectedPhase506ByteSha256 = "d2ac471d5ef36c1351d680bef6d00b34b4fa846144c7e0d7d42f3fb9801046cd";

var contract = BindContract(Path.Combine(phaseDir, "preregistration/phase507_selective_inference_readiness_contract_v1.json"));
var phase503 = BindPhase503(phase503Path);
var phase505 = BindPrecursor(phase505Path, expectedPhase505ByteSha256,
    "phase505-phase503-frozen-failure-localization");
var phase506 = BindPrecursor(phase506Path, expectedPhase506ByteSha256,
    "phase506-phase456-selective-inference-protocol-validation");
var precedenceBattery = SelectiveInferenceReadinessAdjudicator.RunPrecedenceBattery();
bool exactPrecursorsValid = contract.Valid && phase503.Valid && phase505.Valid && phase506.Valid &&
    precedenceBattery.AllCasesPassed;

bool phase503BaselinePreserved = phase503.Classification == "assumption-conditional-protocol";
bool failureLocalizationComplete = phase505.Classification is
    "complete-negative-preserved" or "failure-localization-complete" or
    "frozen-failure-localization-complete";
bool protocolValidationFailed = phase506.Classification == "selective-protocol-validation-failed";
bool assumptionConditionalRepair = phase506.Classification == "assumption-conditional-selective-protocol";
bool completeMenuPassed = phase506.Classification == "selective-protocol-validation-passed" &&
    phase506.WholeFrozenMenuPassed;

var evidence = new ReadinessEvidence(exactPrecursorsValid, phase503BaselinePreserved,
    failureLocalizationComplete, protocolValidationFailed, assumptionConditionalRepair, completeMenuPassed);
var decision = SelectiveInferenceReadinessAdjudicator.Decide(evidence);
string readinessClassification = decision.Classification;
string terminalStatus = "phase456-selective-inference-pack-readiness-adjudicator-" + readinessClassification;

var result = new
{
    phaseId = "phase507-phase456-selective-inference-pack-readiness-adjudicator",
    terminalStatus,
    verdictKind = readinessClassification,
    readinessClassification,
    readinessTaxonomy = SelectiveInferenceReadinessAdjudicator.Taxonomy,
    inputsValid = decision.InputsValid,
    applicationSubjectKind = "phase456-a13-selective-inference-pack-readiness-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A13",
    explorationOnly = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    zeroPhysicsCompute = true,
    zeroNewSampling = true,
    noHmcRun = true,
    noBenchmarkRun = true,
    noReprocessingRun = true,
    frozenBeforePrecursorConsumption = true,
    contractBinding = contract,
    precursorBindings = new { phase503, phase505, phase506 },
    precedenceBattery,
    evidence = new
    {
        phase503BaselinePreserved,
        failureLocalizationComplete,
        protocolValidationFailed,
        assumptionConditionalRepair,
        completeMenuPassed,
        decision.ContradictoryValidationState,
    },
    precedenceTrace = SelectiveInferenceReadinessAdjudicator.Taxonomy.Select((classification, index) => new
    {
        order = index + 1,
        classification,
        selected = classification == readinessClassification,
    }).ToArray(),
    phase503NegativeArtifactPreserved = true,
    phase503ThresholdsRetuned = false,
    ambiguousCaseCountedAsSuccessfulDecisiveSelection = false,
    writtenPermission = new
    {
        providedByUser = true,
        providedDate = "2026-07-15",
        permissionElementPresent = true,
        isPhase507LaunchAuthority = false,
        doesNotBypassIndependentPackOrRefuseToRunGate = true,
    },
    readyResultMayInformLaterIndependentPhase481PackConstruction = readinessClassification ==
        "selective-inference-protocol-ready-for-independent-phase481-pack-construction",
    readyResultIsPlanningEvidenceOnly = true,
    laterPhase481PackMustIndependentlyFreeze = new[]
    {
        "exact-default-configuration-and-implementation-fingerprints",
        "storage-and-chain-topology",
        "rng-policy",
        "convergence-effective-sample-size-and-cost-refusal-rules",
        "selective-inference-truth-set-and-abstention-scoring-rules",
        "still-binding-upstream-authorizations",
    },
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    samplingRun = false,
    reprocessingRun = false,
    samplingAuthorized = false,
    productionAuthorized = false,
    launchAuthorized = false,
    phase456Reinterpreted = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    phase458EvaluationAuthorized = false,
    o4Discharged = false,
    humanRulingAuthored = false,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a13BoundaryHeld = true,
    decision = DecisionText(readinessClassification),
};

string outputDir = Path.Combine(phaseDir, "output");
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "phase456_selective_inference_pack_readiness_adjudicator.json"), json);
File.WriteAllText(Path.Combine(outputDir, "phase456_selective_inference_pack_readiness_adjudicator_summary.json"), json);

Require(precedenceBattery.AllCasesPassed && precedenceBattery.InvalidCasesPassed &&
    precedenceBattery.FailureCasePassed && precedenceBattery.ConditionalCasePassed &&
    precedenceBattery.ReadyCasePassed && precedenceBattery.UnresolvedCasePassed,
    "precedence battery failed");
Require(decision.InputsValid, terminalStatus);
Require(!result.phase481PackCreated && !result.phase481PackMutated && !result.launchAuthorized &&
    !result.samplingAuthorized && !result.productionAuthorized, "pack or launch authority firewall failed");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.o4Discharged,
    "upstream gate firewall failed");
Require(!result.sourceContractApplicationAllowed && result.promotedPhysicalMassClaimCount == 0,
    "claim firewall failed");
Console.WriteLine(terminalStatus);

static ContractBinding BindContract(string path)
{
    byte[] bytes = File.ReadAllBytes(path);
    using var document = JsonDocument.Parse(bytes);
    var root = document.RootElement;
    bool valid = S(root, "contractId") == "phase507-a13-selective-inference-pack-readiness-v1" &&
        B(root, "frozenBeforePrecursorConsumption") &&
        Strings(root, "precedence").SequenceEqual(SelectiveInferenceReadinessAdjudicator.Taxonomy) &&
        S(root, "requiredPhase503Baseline") == "assumption-conditional-protocol" &&
        root.TryGetProperty("writtenPermission", out var permission) &&
        S(permission, "date") == "2026-07-15" && B(permission, "permissionElementPresent") &&
        !B(permission, "isLaunchAuthorityForThisPhase") &&
        !B(root, "phase481PackCreationAllowed") && !B(root, "samplingOrReprocessingLaunchAllowed") &&
        !B(root, "o4OrPhase458RulingAllowed") && !B(root, "sourceContractApplicationAllowed") &&
        !B(root, "physicalClaimAllowed");
    return new ContractBinding(path, Sha(bytes), valid);
}

static PrecursorBinding BindPhase503(string path)
{
    if (!File.Exists(path)) return new(path, "", false, "missing", "", false);
    byte[] bytes = File.ReadAllBytes(path);
    try
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        string classification = S(root, "verdictKind");
        bool valid = Sha(bytes) == expectedPhase503ByteSha256 &&
            S(root, "phaseId") == "phase503-phase456-adaptive-calibration-protocol-validation" &&
            B(root, "inputsValid") && B(root, "a12BoundaryHeld") && B(root, "zeroNewSampling") &&
            !B(root, "samplingAuthorized") && !B(root, "productionAuthorized") &&
            I(root, "promotedPhysicalMassClaimCount") == 0 && classification.Length > 0;
        return new(path, Sha(bytes), valid, valid ? "exact" : "binding-or-boundary-drift",
            classification, B(root, "wholeFrozenMenuPassed") || B(root, "completeFrozenMenuPassed"));
    }
    catch (JsonException) { return new(path, Sha(bytes), false, "invalid-json", "", false); }
}

static PrecursorBinding BindPrecursor(string path, string expectedHash, string expectedPhaseId)
{
    if (!File.Exists(path)) return new(path, "", false, "missing", "", false);
    byte[] bytes = File.ReadAllBytes(path);
    try
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        string classification = S(root, "verdictKind");
        bool valid = Sha(bytes) == expectedHash && S(root, "phaseId") == expectedPhaseId &&
            B(root, "inputsValid") && B(root, "a13BoundaryHeld") && B(root, "zeroNewSampling") &&
            !B(root, "samplingAuthorized") && !B(root, "productionAuthorized") &&
            I(root, "promotedPhysicalMassClaimCount") == 0 && classification.Length > 0;
        return new(path, Sha(bytes), valid, valid ? "exact" : "binding-or-boundary-drift",
            classification, B(root, "wholeFrozenMenuPassed") || B(root, "completeFrozenMenuPassed"));
    }
    catch (JsonException) { return new(path, Sha(bytes), false, "invalid-json", "", false); }
}

static string DecisionText(string classification) => classification switch
{
    "invalid-precursor" => "At least one exact binding, schema, boundary, baseline-preservation, localization, or precedence check failed. No readiness conclusion is available.",
    "protocol-validation-failure" => "The complete frozen selective-inference protocol-validation menu failed. The negative result is preserved and no pack or launch is authorized.",
    "assumption-conditional-repair" => "The selective-inference repair survives only a named subset or named assumptions. A later independent pack must preserve those conditions.",
    "selective-inference-protocol-ready-for-independent-phase481-pack-construction" => "The complete frozen menu supports later independent Phase481 pack construction. This result is planning evidence only and cannot launch sampling or reprocessing.",
    _ => "The exact evidence is valid but does not resolve selective-inference pack readiness.",
};
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string S(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : "";
static bool B(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
static int I(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.TryGetInt32(out int parsed) ? parsed : int.MinValue;
static string[] Strings(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(item => item.GetString() ?? "").ToArray() : Array.Empty<string>();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

internal sealed class ContractBinding
{
    internal ContractBinding(string path, string byteSha256, bool valid) { Path = path; ByteSha256 = byteSha256; Valid = valid; }
    public string Path { get; }
    public string ByteSha256 { get; }
    public bool Valid { get; }
}
internal sealed class PrecursorBinding
{
    internal PrecursorBinding(string path, string byteSha256, bool valid, string status,
        string classification, bool wholeFrozenMenuPassed)
    {
        Path = path; ByteSha256 = byteSha256; Valid = valid; Status = status;
        Classification = classification; WholeFrozenMenuPassed = wholeFrozenMenuPassed;
    }
    public string Path { get; }
    public string ByteSha256 { get; }
    public bool Valid { get; }
    public string Status { get; }
    public string Classification { get; }
    public bool WholeFrozenMenuPassed { get; }
}

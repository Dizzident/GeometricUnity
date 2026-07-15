using System.Security.Cryptography;
using System.Text.Json;

const string phaseDir = "studies/phase504_phase456_calibration_repair_pack_readiness_adjudicator_001";
const string phase501Path = "studies/phase501_phase456_robust_sampling_readiness_adjudicator_001/output/phase456_robust_sampling_readiness_adjudicator_summary.json";
const string phase502Path = "studies/phase502_phase456_adaptive_calibration_protocol_specification_001/output/phase456_adaptive_calibration_protocol_specification_summary.json";
const string phase503Path = "studies/phase503_phase456_adaptive_calibration_protocol_validation_001/output/phase456_adaptive_calibration_protocol_validation_summary.json";
const string expectedPhase501ByteSha256 = "600f35315b9c93de65de9c499f7168eb01c92bb5f0931387b7bd92c4ca172fbb";
const string expectedPhase502ByteSha256 = "a53456b965c09814701f506d84e3cba9d785654e55143d518c9d281ec47d8b53";
const string expectedPhase503ByteSha256 = "9f5492b11f71cb2a62305837e68d0de6402f4d167f2199d33be31c62119e1d29";

var contract = BindContract(Path.Combine(phaseDir, "preregistration/phase504_calibration_repair_readiness_contract_v1.json"));
var phase501 = BindPhase501(phase501Path);
var phase502 = BindPrecursor(phase502Path, expectedPhase502ByteSha256,
    "phase502-phase456-adaptive-calibration-protocol-specification", "protocolClassification", false);
var phase503 = BindPrecursor(phase503Path, expectedPhase503ByteSha256,
    "phase503-phase456-adaptive-calibration-protocol-validation", "validationClassification", true);
var precedenceBattery = CalibrationRepairReadinessAdjudicator.RunPrecedenceBattery();
bool exactPrecursorsValid = contract.Valid && phase501.Valid && phase502.Valid && phase503.Valid && precedenceBattery.AllCasesPassed;

bool protocolSpecificationReady = phase502.Classification == "protocol-specification-ready";
bool protocolValidationFailed = phase503.Classification == "protocol-validation-failed";
bool assumptionConditionalProtocol = phase503.Classification == "assumption-conditional-protocol";
bool protocolValidationPassed = phase503.Classification == "protocol-validation-passed";
var evidence = new ReadinessEvidence(exactPrecursorsValid, protocolSpecificationReady,
    protocolValidationFailed, assumptionConditionalProtocol, protocolValidationPassed);
var decision = CalibrationRepairReadinessAdjudicator.Decide(evidence);
string readinessClassification = decision.Classification;
string terminalStatus = "phase456-calibration-repair-pack-readiness-adjudicator-" + readinessClassification;

var result = new
{
    phaseId = "phase504-phase456-calibration-repair-pack-readiness-adjudicator",
    terminalStatus,
    verdictKind = readinessClassification,
    readinessClassification,
    readinessTaxonomy = CalibrationRepairReadinessAdjudicator.Taxonomy,
    inputsValid = decision.InputsValid,
    applicationSubjectKind = "phase456-a12-calibration-repair-pack-readiness-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A12",
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
    precursorBindings = new { phase501, phase502, phase503 },
    precedenceBattery,
    evidence = new
    {
        protocolSpecificationReady,
        protocolValidationFailed,
        assumptionConditionalProtocol,
        protocolValidationPassed,
        decision.ContradictoryValidationState,
    },
    precedenceTrace = CalibrationRepairReadinessAdjudicator.Taxonomy.Select((classification, index) => new
    {
        order = index + 1,
        classification,
        selected = classification == readinessClassification,
    }).ToArray(),
    writtenPermission = new
    {
        providedByUser = true,
        providedDate = "2026-07-15",
        permissionElementPresent = true,
        permissionScope = "sampling-and-reprocessing-necessary-to-validate-results",
        isPhase504LaunchAuthority = false,
        doesNotBypassIndependentPackOrRefuseToRunGate = true,
    },
    readyResultMayInformLaterIndependentPhase481PackDesign = readinessClassification ==
        "adaptive-calibration-protocol-ready-for-independent-pack",
    laterPhase481PackMustIndependentlyFreeze = new[]
    {
        "exact-input-implementation-and-default-configuration-fingerprints",
        "storage-and-configuration-level-retention-schema",
        "independent-chain-batch-and-convergence-rules",
        "covariance-autocorrelation-and-effective-sample-size-estimators",
        "t16-stop-and-conditional-t32-escalation-rules",
        "cost-ceilings-and-overrun-refuse-rule",
        "rng-seed-policy-and-execution-topology",
    },
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    samplingRun = false,
    reprocessingRun = false,
    samplingAuthorized = false,
    productionAuthorized = false,
    launchAuthorized = false,
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
    a12BoundaryHeld = true,
    decision = DecisionText(readinessClassification),
};

string outputDir = Path.Combine(phaseDir, "output");
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "phase456_calibration_repair_pack_readiness_adjudicator.json"), json);
File.WriteAllText(Path.Combine(outputDir, "phase456_calibration_repair_pack_readiness_adjudicator_summary.json"), json);

Require(precedenceBattery.AllCasesPassed && precedenceBattery.InvalidPrecedencePassed &&
    precedenceBattery.ValidationFailurePrecedencePassed && precedenceBattery.ConditionalPrecedencePassed,
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
    bool valid = S(root, "contractId") == "phase504-a12-calibration-repair-pack-readiness-v1" &&
        B(root, "frozenBeforePrecursorConsumption") &&
        Strings(root, "precedence").SequenceEqual(CalibrationRepairReadinessAdjudicator.Taxonomy) &&
        root.TryGetProperty("writtenPermission", out var permission) &&
        S(permission, "date") == "2026-07-15" && B(permission, "freshWrittenPermissionElementPresent") &&
        !B(permission, "isLaunchAuthorityForThisPhase") &&
        !B(root, "phase481PackCreationAllowed") && !B(root, "samplingOrProductionLaunchAllowed") &&
        !B(root, "o4OrPhase458RulingAllowed") && !B(root, "sourceContractApplicationAllowed") &&
        !B(root, "physicalClaimAllowed");
    return new ContractBinding(path, Sha(bytes), valid);
}

static PrecursorBinding BindPhase501(string path)
{
    if (!File.Exists(path)) return new(path, "", false, "missing", "");
    byte[] bytes = File.ReadAllBytes(path);
    try
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        bool valid = Sha(bytes) == expectedPhase501ByteSha256 &&
            S(root, "phaseId") == "phase501-phase456-robust-sampling-readiness-adjudicator" &&
            B(root, "inputsValid") && B(root, "a11BoundaryHeld") &&
            !B(root, "samplingAuthorized") && !B(root, "productionAuthorized") &&
            I(root, "promotedPhysicalMassClaimCount") == 0;
        return new(path, Sha(bytes), valid, valid ? "exact" : "binding-or-boundary-drift",
            S(root, "readinessClassification"));
    }
    catch (JsonException) { return new(path, Sha(bytes), false, "invalid-json", ""); }
}

static PrecursorBinding BindPrecursor(string path, string expectedHash, string expectedPhaseId,
    string classificationProperty, bool requireExplicitZeroNewSampling)
{
    if (!File.Exists(path)) return new(path, "", false, "missing", "");
    byte[] bytes = File.ReadAllBytes(path);
    try
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        string classification = S(root, classificationProperty);
        if (classification.Length == 0) classification = S(root, "verdictKind");
        bool valid = Sha(bytes) == expectedHash && S(root, "phaseId") == expectedPhaseId &&
            B(root, "inputsValid") && B(root, "a12BoundaryHeld") && !B(root, "samplingRun") &&
            (!requireExplicitZeroNewSampling || B(root, "zeroNewSampling")) &&
            !B(root, "samplingAuthorized") && !B(root, "productionAuthorized") &&
            I(root, "promotedPhysicalMassClaimCount") == 0 && classification.Length > 0;
        return new(path, Sha(bytes), valid, valid ? "exact" : "binding-or-boundary-drift", classification);
    }
    catch (JsonException) { return new(path, Sha(bytes), false, "invalid-json", ""); }
}

static string DecisionText(string classification) => classification switch
{
    "invalid-precursor" => "At least one exact binding, schema, boundary, or precedence check failed. No pack-readiness conclusion is available.",
    "protocol-validation-failure" => "The frozen prospective protocol-validation battery failed. The negative result is preserved and no pack or launch is authorized.",
    "assumption-conditional-protocol" => "The adaptive protocol survives only under named assumptions. A later independent pack must preserve those conditions and all refuse-to-run gates.",
    "adaptive-calibration-protocol-ready-for-independent-pack" => "The exact-bound specification and prospective validation support designing a later independently frozen Phase481 pack. This result is not that pack and cannot launch sampling.",
    _ => "The exact evidence is valid but does not resolve adaptive-calibration pack readiness.",
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
    internal PrecursorBinding(string path, string byteSha256, bool valid, string status, string classification)
    { Path = path; ByteSha256 = byteSha256; Valid = valid; Status = status; Classification = classification; }
    public string Path { get; }
    public string ByteSha256 { get; }
    public bool Valid { get; }
    public string Status { get; }
    public string Classification { get; }
}

using System.Security.Cryptography;
using System.Text.Json;

const string phaseDir = "studies/phase501_phase456_robust_sampling_readiness_adjudicator_001";
const string phase499Path = "studies/phase499_phase456_retained_empirical_noise_information_audit_001/output/phase456_retained_empirical_noise_information_audit_summary.json";
const string phase500Path = "studies/phase500_phase456_adversarial_prospective_acquisition_stress_test_001/output/phase456_adversarial_prospective_acquisition_stress_test_summary.json";
const string phase456Path = "studies/phase456_consolidated_n4_launch_001/output/consolidated_n4_launch_summary.json";
const string expectedPhase499ByteSha256 = "992a96c4f79b9975b2cb5de178522f813a1e4334ae8f58fcdf4fb96f795c6f57";
const string expectedPhase500ByteSha256 = "d472bd7510be14d0a71e90128f0d98ccba965a6fcf4c1043bf24fd4f161885ab";
const string expectedPhase456ByteSha256 = "1668cf2e7b477c24181afa04fcd76089d974c407c08257a62d72a81b8cb9138b";
const double expectedMeasuredCpuWeeks = 0.11371015556660466;

var contract = BindContract(Path.Combine(phaseDir, "preregistration/phase501_robust_readiness_contract_v1.json"));
var phase499 = BindPrecursor(phase499Path, expectedPhase499ByteSha256, "phase499-phase456-retained-empirical-noise-information-audit");
var phase500 = BindPrecursor(phase500Path, expectedPhase500ByteSha256, "phase500-phase456-adversarial-prospective-acquisition-stress-test");
var phase456 = BindPhase456(phase456Path);
var precedenceBattery = RobustReadinessAdjudicator.RunPrecedenceBattery();
bool exactPrecursorsValid = contract.Valid && phase499.Valid && phase500.Valid && phase456.Valid && precedenceBattery.AllCasesPassed;

string calibrationClassification = phase499.Classification;
string stressClassification = phase500.Classification;
bool retainedCalibrationSufficient = calibrationClassification is
    "retained-calibration-sufficient" or "retained-calibration-sufficient-for-frozen-t16-envelope";
bool robustSpecification = stressClassification == "robust-specification-identified";
bool conditionalSpecification = stressClassification is
    "assumption-conditional-specification" or "conditional-specification-survives";
bool noViableSpecification = stressClassification is
    "no-viable-specification-within-audited-budget" or "no-viable-specification-within-audited-grid";

var evidence = new ReadinessEvidence(exactPrecursorsValid, retainedCalibrationSufficient,
    robustSpecification, conditionalSpecification, noViableSpecification);
var decision = RobustReadinessAdjudicator.Decide(evidence);
string readinessClassification = decision.Classification;
string terminalStatus = "phase456-robust-sampling-readiness-adjudicator-" + readinessClassification;

var result = new
{
    phaseId = "phase501-phase456-robust-sampling-readiness-adjudicator",
    terminalStatus,
    verdictKind = readinessClassification,
    readinessClassification,
    readinessTaxonomy = RobustReadinessAdjudicator.Taxonomy,
    inputsValid = decision.InputsValid,
    applicationSubjectKind = "phase456-a11-robust-sampling-readiness-planning-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A11",
    explorationOnly = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    zeroPhysicsCompute = true,
    noHmcRun = true,
    noBenchmarkRun = true,
    frozenBeforePrecursorConsumption = true,
    contractBinding = contract,
    precursorBindings = new { phase499, phase500, phase456 },
    precedenceBattery,
    evidence = new
    {
        retainedCalibrationSufficient,
        robustSpecification,
        conditionalSpecification,
        noViableSpecificationWithinAuditedBudget = noViableSpecification,
        decision.ContradictoryStressState,
    },
    precedenceTrace = RobustReadinessAdjudicator.Taxonomy.Select((classification, index) => new
    {
        order = index + 1,
        classification,
        selected = classification == readinessClassification,
    }).ToArray(),
    phase456MeasuredOperationalCost = new
    {
        measuredCpuWeeks = phase456.MeasuredCpuWeeks,
        sourceIsExactBoundPhase456Summary = phase456.Valid,
        notAnExtrapolatedPhase481Cost = true,
    },
    forwardedRequirementsForLaterIndependentPhase481Pack = new
    {
        storage = new[] { "per-site-unsummed-correlators", "per-face-type-resolution", "complete-temporal-separations", "configuration-or-block-level-resampling-inputs" },
        chainsAndAutocorrelation = new[] { "independent-chain-count", "chain-convergence-gate", "frozen-autocorrelation-estimator", "effective-sample-size-stop-and-refuse-rule" },
        cost = new[] { "trajectory-to-cpu-week-measurement", "audited-cost-scaling-model", "budget-ceiling", "cost-overrun-refuse-rule" },
        refuseToRun = new[] { "exact-input-hashes", "implementation-hash", "default-configuration-hash", "rng-and-seed-policy", "execution-topology", "output-schema" },
    },
    forwardedRequirementsAreNotAPhase481Pack = true,
    planningEvidenceOnly = true,
    mayInformLaterIndependentPhase481PackDesign = readinessClassification is
        "robust-specification-identified" or "assumption-conditional-specification",
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    newWrittenSamplingAuthorizationStillRequired = true,
    samplingRun = false,
    samplingAuthorized = false,
    productionAuthorized = false,
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
    a11BoundaryHeld = true,
    decision = DecisionText(readinessClassification),
};

string outputDir = Path.Combine(phaseDir, "output");
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "phase456_robust_sampling_readiness_adjudicator.json"), json);
File.WriteAllText(Path.Combine(outputDir, "phase456_robust_sampling_readiness_adjudicator_summary.json"), json);

Require(precedenceBattery.AllCasesPassed && precedenceBattery.InvalidPrecedencePassed && precedenceBattery.InsufficientCalibrationPrecedencePassed, "precedence battery failed");
Require(decision.InputsValid, terminalStatus);
Require(!result.phase481PackCreated && !result.phase481PackMutated && !result.samplingAuthorized && !result.productionAuthorized, "pack or run authority firewall failed");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.o4Discharged, "upstream gate firewall failed");
Require(!result.sourceContractApplicationAllowed && result.promotedPhysicalMassClaimCount == 0, "claim firewall failed");
Console.WriteLine(terminalStatus);

static ContractBinding BindContract(string path)
{
    byte[] bytes = File.ReadAllBytes(path);
    using var document = JsonDocument.Parse(bytes);
    var root = document.RootElement;
    bool valid = S(root, "contractId") == "phase501-a11-robust-sampling-readiness-v1" &&
        B(root, "frozenBeforePrecursorConsumption") &&
        Strings(root, "precedence").SequenceEqual(RobustReadinessAdjudicator.Taxonomy) &&
        !B(root, "phase481PackCreationAllowed") && !B(root, "samplingOrProductionAuthorizationAllowed") &&
        !B(root, "physicalClaimAllowed");
    return new ContractBinding(path, Sha(bytes), valid);
}

static PrecursorBinding BindPrecursor(string path, string expectedHash, string expectedPhaseId)
{
    if (!File.Exists(path)) return new(path, "", false, "missing", "");
    byte[] bytes = File.ReadAllBytes(path);
    try
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        string classification = S(root, "classification");
        if (classification.Length == 0) classification = S(root, "verdictKind");
        bool valid = Sha(bytes) == expectedHash && S(root, "phaseId") == expectedPhaseId &&
            B(root, "inputsValid") && B(root, "a11BoundaryHeld") &&
            !B(root, "samplingAuthorized") && !B(root, "productionAuthorized") &&
            I(root, "promotedPhysicalMassClaimCount") == 0 && classification.Length > 0;
        return new(path, Sha(bytes), valid, valid ? "exact" : "binding-or-boundary-drift", classification);
    }
    catch (JsonException) { return new(path, Sha(bytes), false, "invalid-json", ""); }
}

static Phase456Binding BindPhase456(string path)
{
    if (!File.Exists(path)) return new(path, "", false, double.NaN);
    byte[] bytes = File.ReadAllBytes(path);
    using var document = JsonDocument.Parse(bytes);
    var root = document.RootElement;
    double cost = root.TryGetProperty("productionAnalysis", out var analysis) &&
        analysis.TryGetProperty("measuredCpuWeeks", out var value) && value.TryGetDouble(out double parsed) ? parsed : double.NaN;
    bool valid = Sha(bytes) == expectedPhase456ByteSha256 &&
        S(root, "phaseId") == "phase456-consolidated-n4-launch" &&
        double.IsFinite(cost) && Math.Abs(cost - expectedMeasuredCpuWeeks) <= 1e-15 &&
        I(root, "promotedPhysicalMassClaimCount") == 0;
    return new(path, Sha(bytes), valid, cost);
}

static string DecisionText(string classification) => classification switch
{
    "invalid-precursor" => "At least one exact binding, schema, boundary, cost, or precedence check failed. No readiness conclusion is available.",
    "insufficient-retained-calibration" => "The retained Phase456 information cannot calibrate the prospective robustness claim. Synthetic survival cannot override this result.",
    "robust-specification-identified" => "The frozen adversarial menu supports a robust planning specification. A later Phase481 pack must independently freeze every forwarded rule and obtain new written authorization.",
    "assumption-conditional-specification" => "A specification survives only under named assumptions. Those assumptions must remain explicit in any later independently frozen pack.",
    "no-viable-specification-within-audited-budget" => "No specification survives the frozen menu within the audited budget. The negative result authorizes no run.",
    _ => "The exact evidence is valid but does not resolve robust sampling readiness.",
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
internal sealed class Phase456Binding
{
    internal Phase456Binding(string path, string byteSha256, bool valid, double measuredCpuWeeks)
    { Path = path; ByteSha256 = byteSha256; Valid = valid; MeasuredCpuWeeks = measuredCpuWeeks; }
    public string Path { get; }
    public string ByteSha256 { get; }
    public bool Valid { get; }
    public double MeasuredCpuWeeks { get; }
}

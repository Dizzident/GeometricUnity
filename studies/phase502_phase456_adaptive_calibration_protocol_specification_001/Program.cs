using System.Security.Cryptography;
using System.Text.Json;

const string phaseDir = "studies/phase502_phase456_adaptive_calibration_protocol_specification_001";
const string contractPath = phaseDir + "/preregistration/phase502_adaptive_calibration_protocol_v1.json";
const string phase501Path = "studies/phase501_phase456_robust_sampling_readiness_adjudicator_001/output/phase456_robust_sampling_readiness_adjudicator_summary.json";
const string phase456Path = "studies/phase456_consolidated_n4_launch_001/output/consolidated_n4_launch_summary.json";
const string expectedPhase501Sha256 = "600f35315b9c93de65de9c499f7168eb01c92bb5f0931387b7bd92c4ca172fbb";
const string expectedPhase456Sha256 = "1668cf2e7b477c24181afa04fcd76089d974c407c08257a62d72a81b8cb9138b";
const double expectedMeasuredCpuWeeks = 0.11371015556660466;

// Contract consumption deliberately precedes all precursor reads.
var contract = BindContract(contractPath);
var phase501 = BindPhase501(phase501Path);
var phase456 = BindPhase456(phase456Path);
bool inputsValid = contract.Valid && phase501.Valid && phase456.Valid;
string classification = inputsValid ? "protocol-specification-ready" : "invalid-precursor-or-contract";
string terminalStatus = "phase456-adaptive-calibration-protocol-specification-" + classification;

var result = new
{
    phaseId = "phase502-phase456-adaptive-calibration-protocol-specification",
    terminalStatus,
    verdictKind = classification,
    protocolClassification = classification,
    inputsValid,
    applicationSubjectKind = "phase456-a12-adaptive-calibration-protocol-specification",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A12",
    explorationOnly = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    zeroPhysicsCompute = true,
    noHmcRun = true,
    noBenchmarkRun = true,
    noReprocessingRun = true,
    frozenBeforePrecursorConsumption = contract.Valid,
    contractBinding = contract,
    precursorBindings = new { phase501, phase456 },
    protocol = contract.Protocol,
    measuredCostBinding = new
    {
        phase456.MeasuredCpuWeeks,
        exactBound = phase456.Valid,
        calibrationCpuWeekCeiling = contract.Protocol.CostRules.CalibrationCpuWeekCeiling,
        measuredCostIsNotProspectiveCostForecast = true,
    },
    sequentialDecision = new
    {
        initialStage = "t16-calibration",
        conditionalStage = "t32-calibration",
        t32RunsOnlyAfterValidT16EscalationTrigger = true,
        allQuantitiesUnknownUntilFutureMeasurement = true,
        syntheticValuesRelabeledEmpirical = false,
    },
    futureExecutionPermissionRecord = new
    {
        freshWrittenPermissionRecordedByA12 = true,
        permissionDate = "2026-07-15",
        permissionAloneIsNotAnExecutablePack = true,
        independentlyFrozenHashPinnedPackStillRequired = true,
    },
    outputSchema = new
    {
        schemaVersion = 1,
        twinFilesRequired = true,
        futureCalibrationMustEmit = new[]
        {
            "exact-fingerprint-bindings", "per-chain-and-batch-retention-counts",
            "invalid-row-census", "covariance-and-autocorrelation-estimates",
            "per-observable-and-per-chain-ess", "split-rhat",
            "checkpoint-stability-metrics", "cost-ledger-and-forecast",
            "precedence-trace-and-terminal",
        },
    },
    planningEvidenceOnly = true,
    mayInformLaterIndependentPhase481PackDesign = inputsValid,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    samplingRun = false,
    samplingAuthorized = false,
    samplingAuthorizedByThisPhase = false,
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
    a12BoundaryHeld = true,
    decision = inputsValid
        ? "The target-blind sequential T16/conditional-T32 calibration protocol is fully specified for later independent pack construction. This phase launches nothing and grants no run or claim authority."
        : "A contract or exact precursor binding failed; the protocol specification is invalid and grants no authority.",
};

string outputDir = Path.Combine(phaseDir, "output");
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "phase456_adaptive_calibration_protocol_specification.json"), json);
File.WriteAllText(Path.Combine(outputDir, "phase456_adaptive_calibration_protocol_specification_summary.json"), json);

Require(inputsValid, terminalStatus);
Require(contract.Protocol.RetainedConfigurationSchema.MissingNonfiniteOrDuplicateRowAction == "invalidate-stage-no-imputation", "invalid-row rule drift");
Require(contract.Protocol.ChainAndBatchDesign.IndependentChains == 4 && !contract.Protocol.ChainAndBatchDesign.CrossChainStateSharingAllowed, "chain independence drift");
Require(contract.Protocol.T16Rules.TemporalExtent == 16 && contract.Protocol.T32Rules.TemporalExtent == 32, "stage order drift");
Require(contract.Protocol.CostRules.CalibrationCpuWeekCeiling == 2.0, "cost ceiling drift");
Require(!result.phase481PackCreated && !result.samplingRun && !result.samplingAuthorized && !result.samplingAuthorizedByThisPhase && !result.productionAuthorized, "pack/run firewall failed");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.o4Discharged, "upstream firewall failed");
Require(!result.sourceContractApplicationAllowed && result.promotedPhysicalMassClaimCount == 0, "claim firewall failed");
Console.WriteLine(terminalStatus);

static ContractBinding BindContract(string path)
{
    byte[] bytes = File.ReadAllBytes(path);
    using var document = JsonDocument.Parse(bytes);
    var root = document.RootElement;
    bool valid = S(root, "contractId") == "phase502-a12-adaptive-calibration-protocol-v1" &&
        B(root, "frozenBeforePrecursorConsumption") &&
        Strings(root, "requiredExactBindings").SequenceEqual(new[] { "phase501-summary-bytes", "phase456-summary-bytes-and-measured-operational-cost" }) &&
        Strings(root, "stageOrder").SequenceEqual(new[] { "t16-calibration", "conditional-t32-calibration" }) &&
        B(root, "refuseToRunOnAnyFingerprintMismatch") && B(root, "unknownCalibrationQuantitiesRemainUnknown") &&
        !B(root, "syntheticValuesMayBeRelabeledEmpirical") && !B(root, "phase481PackCreationAllowed") &&
        !B(root, "samplingLaunchAllowed") && !B(root, "productionAuthorizationAllowed") &&
        !B(root, "phase458InputAllowed") && !B(root, "physicalClaimAllowed");
    var protocol = JsonSerializer.Deserialize<ProtocolContract>(bytes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("contract deserialization failed");
    valid &= protocol.ChainAndBatchDesign.ChainSeeds.SequenceEqual(new[] { 5021601, 5021602, 5021603, 5021604 }) &&
        protocol.T16Rules.MinimumTotalEss == 2048 && protocol.T32Rules.MinimumTotalEss == 8192 &&
        protocol.Estimators.SplitRhat.StartsWith("rank-normalized split-Rhat", StringComparison.Ordinal) &&
        protocol.T16Rules.EscalateToT32Predicate.Contains(" AND (", StringComparison.Ordinal) &&
        protocol.T16Rules.BoundaryWindowContaminationDefinition.Length > 0 &&
        protocol.T16Rules.BoundaryModelAmbiguityDefinition.Length > 0 &&
        Math.Abs(protocol.CostRules.Phase456MeasuredCpuWeeks - expectedMeasuredCpuWeeks) <= 1e-15 &&
        protocol.RetainedConfigurationSchema.RequiredIdentityFields.Length == 9 &&
        protocol.RetainedConfigurationSchema.RequiredObservables.Length == 5 &&
        protocol.FutureExecutableFingerprintMustExactBind.Length == 8 &&
        protocol.TerminalTaxonomyInPrecedenceOrder.SequenceEqual(new[]
        {
            "invalid-precursor-or-fingerprint", "invalid-calibration-data", "cost-overrun",
            "convergence-failure", "t16-calibration-ready", "t32-calibration-ready", "unresolved",
        });
    return new ContractBinding(path, Sha(bytes), valid, protocol);
}

static PrecursorBinding BindPhase501(string path)
{
    if (!File.Exists(path)) return new(path, "", false, "missing", "");
    byte[] bytes = File.ReadAllBytes(path);
    using var document = JsonDocument.Parse(bytes);
    var root = document.RootElement;
    string verdict = S(root, "verdictKind");
    bool valid = Sha(bytes) == expectedPhase501Sha256 &&
        S(root, "phaseId") == "phase501-phase456-robust-sampling-readiness-adjudicator" &&
        B(root, "inputsValid") && B(root, "a11BoundaryHeld") && verdict == "insufficient-retained-calibration" &&
        !B(root, "samplingAuthorized") && !B(root, "productionAuthorized") && I(root, "promotedPhysicalMassClaimCount") == 0;
    return new(path, Sha(bytes), valid, valid ? "exact" : "binding-or-boundary-drift", verdict);
}

static Phase456Binding BindPhase456(string path)
{
    if (!File.Exists(path)) return new(path, "", false, double.NaN);
    byte[] bytes = File.ReadAllBytes(path);
    using var document = JsonDocument.Parse(bytes);
    var root = document.RootElement;
    double cost = root.TryGetProperty("productionAnalysis", out var analysis) &&
        analysis.TryGetProperty("measuredCpuWeeks", out var value) && value.TryGetDouble(out double parsed) ? parsed : double.NaN;
    bool valid = Sha(bytes) == expectedPhase456Sha256 && S(root, "phaseId") == "phase456-consolidated-n4-launch" &&
        S(root, "verdictKind") == "production-analysis-invalid" && double.IsFinite(cost) &&
        Math.Abs(cost - expectedMeasuredCpuWeeks) <= 1e-15 && I(root, "promotedPhysicalMassClaimCount") == 0;
    return new(path, Sha(bytes), valid, cost);
}

static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string S(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : "";
static bool B(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
static int I(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.TryGetInt32(out int parsed) ? parsed : int.MinValue;
static string[] Strings(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(item => item.GetString() ?? "").ToArray() : Array.Empty<string>();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

internal sealed record ContractBinding(string Path, string ByteSha256, bool Valid, ProtocolContract Protocol);
internal sealed record PrecursorBinding(string Path, string ByteSha256, bool Valid, string Status, string VerdictKind);
internal sealed record Phase456Binding(string Path, string ByteSha256, bool Valid, double MeasuredCpuWeeks);

internal sealed class ProtocolContract
{
    public int SchemaVersion { get; set; }
    public string ContractId { get; set; } = "";
    public RetainedConfigurationSchema RetainedConfigurationSchema { get; set; } = new();
    public ChainAndBatchDesign ChainAndBatchDesign { get; set; } = new();
    public EstimatorRules Estimators { get; set; } = new();
    public StageRules T16Rules { get; set; } = new();
    public StageRules T32Rules { get; set; } = new();
    public CostRules CostRules { get; set; } = new();
    public string[] TerminalTaxonomyInPrecedenceOrder { get; set; } = Array.Empty<string>();
    public string[] FutureExecutableFingerprintMustExactBind { get; set; } = Array.Empty<string>();
}
internal sealed class RetainedConfigurationSchema
{
    public string[] RequiredIdentityFields { get; set; } = Array.Empty<string>();
    public string[] RequiredObservables { get; set; } = Array.Empty<string>();
    public string Serialization { get; set; } = "";
    public string MissingNonfiniteOrDuplicateRowAction { get; set; } = "";
}
internal sealed class ChainAndBatchDesign
{
    public int IndependentChains { get; set; }
    public int WarmupTrajectoriesPerChain { get; set; }
    public int MeasurementStrideTrajectories { get; set; }
    public int RetainedBatchSizePerChain { get; set; }
    public int CheckpointBatchMultiplePerChain { get; set; }
    public int MinimumPostWarmupBatchesPerChainBeforeDecision { get; set; }
    public int MaximumT16RetainedConfigurationsPerChain { get; set; }
    public int MaximumT32RetainedConfigurationsPerChain { get; set; }
    public int[] ChainSeeds { get; set; } = Array.Empty<int>();
    public bool SeedDerivationAllowed { get; set; }
    public bool CrossChainStateSharingAllowed { get; set; }
}
internal sealed class EstimatorRules
{
    public string Covariance { get; set; } = "";
    public string Autocorrelation { get; set; } = "";
    public double IntegratedAutocorrelationTimeFloor { get; set; }
    public string EffectiveSampleSize { get; set; } = "";
    public string SplitRhat { get; set; } = "";
    public string Uncertainty { get; set; } = "";
    public string CovarianceRegularization { get; set; } = "";
    public bool TargetValuesUsed { get; set; }
}
internal sealed class StageRules
{
    public int TemporalExtent { get; set; }
    public int MinimumTotalEss { get; set; }
    public int MinimumPerChainEss { get; set; }
    public double MaximumSplitRhat { get; set; }
    public double MaximumRelativeEssDriftAcrossLastTwoCheckpoints { get; set; }
    public double MaximumCovarianceFrobeniusDriftAcrossLastTwoCheckpoints { get; set; }
    public int RequiredConsecutivePassingCheckpoints { get; set; }
    public bool StopReadyWhenAllRulesPass { get; set; }
    public string BoundaryWindowContaminationDefinition { get; set; } = "";
    public string BoundaryModelAmbiguityDefinition { get; set; } = "";
    public string EscalateToT32Predicate { get; set; } = "";
    public string StopWithoutEscalationWhen { get; set; } = "";
    public string FailWhen { get; set; } = "";
}
internal sealed class CostRules
{
    public double Phase456MeasuredCpuWeeks { get; set; }
    public double CalibrationCpuWeekCeiling { get; set; }
    public string CheckpointCostMeasurement { get; set; } = "";
    public string Forecast { get; set; } = "";
    public double ForecastSafetyFactor { get; set; }
    public string RefuseNextBatchWhen { get; set; } = "";
    public string CostOverrunAction { get; set; } = "";
}

using System.Security.Cryptography;
using System.Text.Json;

const string outputDir = "studies/phase498_phase456_acquisition_repair_readiness_adjudicator_001/output";
const string contractPath = "studies/phase498_phase456_acquisition_repair_readiness_adjudicator_001/preregistration/phase498_readiness_contract_v1.json";
const string phase496Path = "studies/phase496_phase456_retained_data_information_census_001/output/phase456_retained_data_information_census_summary.json";
const string phase497Path = "studies/phase497_phase456_prospective_estimator_acquisition_oracle_001/output/phase456_prospective_estimator_acquisition_oracle_summary.json";
const string expectedPhase496ByteSha256 = "f87307724ec69e239b1b1354376274e7beced92559f00a7b5ef517012d92a9a9";
const string expectedPhase496DecisionContractSha256 = "fbc2e9358564d3de41b02ba577e92db3af39144b76553138415fe74e5e247815";
const string expectedPhase497ByteSha256 = "c5594d5db2b89a8c9bdce0cde2a7090c162a15ca3471cdca5f711af811f7259c";
const string expectedPhase497FrozenContractSha256 = "836c0055b0a53c76b1d001c294b41a5eba4c54ae0b8c5daba41e2bc68c845734";

var precedenceBattery = ReadinessAdjudicator.RunPrecedenceBattery();
var contractBinding = BindContract(contractPath);
var phase496 = BindPhase496(phase496Path);
var phase497 = BindPhase497(phase497Path);
bool exactPrecursorsValid = contractBinding.Valid && phase496.Valid && phase497.Valid &&
    precedenceBattery.AllCasesPassed && precedenceBattery.InvalidPrecedencePassed &&
    precedenceBattery.ReadinessRequiresWholeMenuPassed;

bool retainedDataSufficient = exactPrecursorsValid &&
    phase496.Bool("retainedDataSufficientForProspectiveEstimatorDesign");
bool newAcquisitionRequired = exactPrecursorsValid &&
    phase496.Classification == "retained-data-insufficient-for-identifiable-repair";
bool viableSpecificationFound = exactPrecursorsValid && phase497.Bool("viableSpecificationFound");
bool oracleBatteryValid = exactPrecursorsValid && phase497.Bool("oracleBatteryValid");
bool selectedSpecificationPresent = exactPrecursorsValid &&
    phase497.Kind("selectedSpecification") == JsonValueKind.Object;
bool newAcquisitionSpecificationReady = viableSpecificationFound && oracleBatteryValid &&
    selectedSpecificationPresent && phase497.Classification == "acquisition-specification-viable";
bool noViableSpecificationWithinAuditedGrid = exactPrecursorsValid && oracleBatteryValid &&
    !viableSpecificationFound && phase497.Classification == "no-viable-specification-within-audited-grid";

var evidence = new ReadinessEvidence(
    exactPrecursorsValid,
    retainedDataSufficient,
    newAcquisitionRequired,
    oracleBatteryValid && viableSpecificationFound && selectedSpecificationPresent,
    newAcquisitionSpecificationReady,
    oracleBatteryValid,
    noViableSpecificationWithinAuditedGrid);
var decision = ReadinessAdjudicator.Decide(evidence);
string readinessClassification = decision.Classification;
string terminalStatus = "phase456-acquisition-repair-readiness-adjudicator-" + readinessClassification;

var result = new
{
    phaseId = "phase498-phase456-acquisition-repair-readiness-adjudicator",
    terminalStatus,
    verdictKind = readinessClassification,
    readinessClassification,
    readinessTaxonomy = ReadinessAdjudicator.Taxonomy,
    inputsValid = decision.InputsValid,
    applicationSubjectKind = "phase456-acquisition-repair-readiness-planning-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A10",
    explorationOnly = true,
    zeroPhysicsCompute = true,
    noHmcRun = true,
    noBenchmarkRun = true,
    frozenBeforePrecursorConsumption = true,
    contractBinding = contractBinding.Output(),
    precursorBindings = new
    {
        phase496 = phase496.Output(),
        phase497 = phase497.Output(),
        bothExactBytesSchemasAndContractsValid = phase496.Valid && phase497.Valid,
    },
    precedenceBattery,
    evidence = new
    {
        retainedDataSufficient,
        newAcquisitionRequired,
        oracleBatteryValid,
        viableSpecificationFound,
        selectedSpecificationPresent,
        newAcquisitionSpecificationReady,
        noViableSpecificationWithinAuditedGrid,
        contradictoryCensusState = decision.ContradictoryCensusState,
        contradictoryOracleState = decision.ContradictoryOracleState,
    },
    precedenceTrace = ReadinessAdjudicator.Taxonomy.Select((classification, index) => new
    {
        order = index + 1,
        classification,
        conditionSupported = classification switch
        {
            "invalid-precursor" => !decision.InputsValid,
            "retained-data-sufficient" => decision.RetainedDataSufficientCondition,
            "new-acquisition-specification-ready" => decision.NewAcquisitionSpecificationReadyCondition,
            "no-viable-specification-within-audited-grid" => decision.NoViableSpecificationCondition,
            _ => decision.InputsValid && !decision.RetainedDataSufficientCondition &&
                !decision.NewAcquisitionSpecificationReadyCondition && !decision.NoViableSpecificationCondition,
        },
        selected = classification == readinessClassification,
    }).ToArray(),
    planningEvidenceOnly = true,
    mayInformLaterPhase481PackDesign = readinessClassification is "retained-data-sufficient" or "new-acquisition-specification-ready",
    selectedAcquisitionSpecification = newAcquisitionSpecificationReady ? phase497.Element("selectedSpecification") : null,
    futurePhase481MustIndependentlyFreezeAllA10ListedAcquisitionRules = true,
    futurePhase481RequiredIndependentFreezeItems = new[]
    {
        "observables",
        "temporal-extent",
        "effective-sample-size-and-power-floors",
        "chain-and-autocorrelation-rules",
        "estimator",
        "covariance-treatment",
        "terminal-taxonomy",
        "refuse-to-run-fingerprint",
    },
    newWrittenSamplingAuthorizationStillRequired = true,
    phase456TerminalChanged = false,
    phase456InvalidRowsReinterpreted = false,
    phase456CommittedArtifactsMutated = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    phase481PackAuthorized = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    phase458EvaluationAuthorized = false,
    binderLaunchAuthorized = false,
    samplingRun = false,
    samplingAuthorized = false,
    productionAuthorized = false,
    externalReviewStillRequired = true,
    humanRulingAuthored = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a10BoundaryHeld = true,
    decision = readinessClassification switch
    {
        "invalid-precursor" => "At least one exact precursor byte, schema, frozen contract, boundary, or precedence-battery check failed. No acquisition or repair-readiness conclusion is available.",
        "retained-data-sufficient" => "The exact retained-data and oracle gates support later prospective estimator-pack design without a new acquisition. This is planning evidence only; no pack or run is authorized.",
        "new-acquisition-specification-ready" => "A frozen synthetic oracle specification survives the whole admitted menu and the retained bytes are insufficient. The result may inform a later independently frozen Phase481 pack, but creates no pack and authorizes no run.",
        "no-viable-specification-within-audited-grid" => "No candidate in the frozen audited acquisition grid survives the oracle gates. The negative result authorizes no run.",
        _ => "The exact precursor evidence does not resolve acquisition readiness under the frozen precedence. No pack or run is authorized.",
    },
};

Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "phase456_acquisition_repair_readiness_adjudicator.json"), json);
File.WriteAllText(Path.Combine(outputDir, "phase456_acquisition_repair_readiness_adjudicator_summary.json"), json);

Require(ReadinessAdjudicator.Taxonomy.Contains(readinessClassification, StringComparer.Ordinal), "readiness taxonomy drifted");
Require(precedenceBattery.AllCasesPassed && precedenceBattery.InvalidPrecedencePassed, "precedence battery failed");
Require(decision.InputsValid, terminalStatus);
Require(readinessClassification == "new-acquisition-specification-ready", "expected conservative A10 readiness terminal drifted");
Require(!result.phase481PackCreated && !result.phase481RepairPackConstructed && !result.samplingAuthorized && !result.productionAuthorized, "pack or run authority firewall failed");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.phase458EvaluationAuthorized, "Phase458 firewall failed");
Require(!result.o4Discharged && !result.sourceContractApplicationAllowed && result.noGevPromotion && result.promotedPhysicalMassClaimCount == 0, "review, source-contract, or claim firewall failed");
Console.WriteLine(terminalStatus);

static ContractBinding BindContract(string path)
{
    if (!File.Exists(path)) return ContractBinding.Invalid(path, "missing-contract");
    byte[] bytes = File.ReadAllBytes(path);
    try
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        bool valid = GetInt(root, "schemaVersion") == 1 &&
            GetString(root, "contractId") == "phase498-a10-acquisition-repair-readiness-v1" &&
            GetBool(root, "frozenBeforePrecursorConsumption") &&
            Strings(root, "precedence").SequenceEqual(ReadinessAdjudicator.Taxonomy) &&
            GetBool(root, "readyResultIsPlanningEvidenceOnly") &&
            !GetBool(root, "phase481PackCreationAllowed") &&
            !GetBool(root, "samplingOrProductionAuthorizationAllowed") &&
            !GetBool(root, "physicalClaimAllowed");
        return new ContractBinding(path, Sha256(bytes), valid, valid ? "exact" : "schema-or-boundary-drift");
    }
    catch (JsonException)
    {
        return ContractBinding.Invalid(path, "invalid-json");
    }
}

static PrecursorBinding BindPhase496(string path)
{
    string[] taxonomy =
    {
        "invalid-precursor",
        "retained-data-insufficient-for-identifiable-repair",
        "retained-data-sufficient-for-prospective-estimator-design",
    };
    var binding = PrecursorBinding.Load(path, expectedPhase496ByteSha256, "censusClassification");
    bool schemaValid = binding.Has("phaseId", JsonValueKind.String) &&
        binding.Has("terminalStatus", JsonValueKind.String) && binding.Has("verdictKind", JsonValueKind.String) &&
        binding.Has("inputsValid", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("censusClassification", JsonValueKind.String) &&
        binding.Has("taxonomy", JsonValueKind.Array) &&
        binding.Has("decisionContractSha256", JsonValueKind.String) &&
        binding.Has("retainedDataSufficientForProspectiveEstimatorDesign", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("a10BoundaryHeld", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("promotedPhysicalMassClaimCount", JsonValueKind.Number);
    binding.FinalizeValidity(schemaValid &&
        binding.String("phaseId") == "phase496-phase456-retained-data-information-census" &&
        binding.String("decisionContractSha256") == expectedPhase496DecisionContractSha256 &&
        binding.Strings("taxonomy").SequenceEqual(taxonomy) &&
        taxonomy.Contains(binding.Classification, StringComparer.Ordinal) &&
        binding.String("verdictKind") == binding.Classification && binding.Bool("inputsValid") &&
        binding.Bool("a10BoundaryHeld") && !binding.Bool("phase456InvalidRowsReinterpreted") &&
        binding.Bool("zeroNewSampling") && !binding.Bool("phase458G3Satisfied") &&
        !binding.Bool("samplingAuthorized") && !binding.Bool("productionAuthorized") &&
        binding.Int("promotedPhysicalMassClaimCount") == 0);
    return binding;
}

static PrecursorBinding BindPhase497(string path)
{
    string[] taxonomy =
    {
        "invalid-oracle-battery",
        "acquisition-specification-viable",
        "no-viable-specification-within-audited-grid",
    };
    var binding = PrecursorBinding.Load(path, expectedPhase497ByteSha256, "verdictKind");
    bool schemaValid = binding.Has("phaseId", JsonValueKind.String) &&
        binding.Has("terminalStatus", JsonValueKind.String) && binding.Has("verdictKind", JsonValueKind.String) &&
        binding.Has("taxonomy", JsonValueKind.Array) &&
        binding.Has("inputsValid", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("oracleBatteryValid", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("viableSpecificationFound", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("selectedSpecification", JsonValueKind.Object, JsonValueKind.Null) &&
        binding.Has("frozenContractSha256", JsonValueKind.String) &&
        binding.Has("frozenBeforeEvaluation", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("syntheticPlanningEvidenceOnly", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("a10BoundaryHeld", JsonValueKind.True, JsonValueKind.False) &&
        binding.Has("promotedPhysicalMassClaimCount", JsonValueKind.Number);
    binding.FinalizeValidity(schemaValid &&
        binding.String("phaseId") == "phase497-phase456-prospective-estimator-acquisition-oracle" &&
        binding.String("frozenContractSha256") == expectedPhase497FrozenContractSha256 &&
        binding.Strings("taxonomy").SequenceEqual(taxonomy) &&
        taxonomy.Contains(binding.Classification, StringComparer.Ordinal) && binding.Bool("inputsValid") &&
        binding.Bool("frozenBeforeEvaluation") && binding.Bool("syntheticPlanningEvidenceOnly") && binding.Bool("a10BoundaryHeld") &&
        !binding.Bool("phase458G3Satisfied") && !binding.Bool("phase458G5Satisfied") &&
        !binding.Bool("samplingAuthorized") && !binding.Bool("productionAuthorized") &&
        binding.Int("promotedPhysicalMassClaimCount") == 0);
    return binding;
}

static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string GetString(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : "";
static bool GetBool(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
static int GetInt(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int result) ? result : int.MinValue;
static string[] Strings(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(item => item.GetString() ?? "").ToArray() : Array.Empty<string>();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

internal sealed class ContractBinding
{
    internal ContractBinding(string path, string byteSha256, bool valid, string reason)
    {
        Path = path; ByteSha256 = byteSha256; Valid = valid; Reason = reason;
    }
    internal string Path { get; }
    internal string ByteSha256 { get; }
    internal bool Valid { get; }
    internal string Reason { get; }
    internal static ContractBinding Invalid(string path, string reason) => new(path, "", false, reason);
    internal object Output() => new { path = Path, byteSha256 = ByteSha256, valid = Valid, reason = Reason };
}

internal sealed class PrecursorBinding
{
    private readonly JsonElement root;
    private bool schemaAndBoundaryValid;
    private PrecursorBinding(string path, string expectedByteSha256, string actualByteSha256, bool byteHashValid,
        string classificationProperty, JsonElement root, string reason)
    {
        Path = path; ExpectedByteSha256 = expectedByteSha256; ActualByteSha256 = actualByteSha256;
        ByteHashValid = byteHashValid; ClassificationProperty = classificationProperty; this.root = root;
        Reason = reason;
    }
    internal string Path { get; }
    internal string ExpectedByteSha256 { get; }
    internal string ActualByteSha256 { get; }
    internal bool ByteHashValid { get; }
    internal string ClassificationProperty { get; }
    internal string Reason { get; private set; }
    internal bool Valid => ByteHashValid && schemaAndBoundaryValid;
    internal string Classification => String(ClassificationProperty);
    internal static PrecursorBinding Load(string path, string expectedByteSha256, string classificationProperty)
    {
        if (!File.Exists(path)) return new(path, expectedByteSha256, "", false, classificationProperty, default, "missing-input");
        byte[] bytes = File.ReadAllBytes(path); string actual = Sha256Local(bytes);
        try
        {
            using var document = JsonDocument.Parse(bytes);
            return new(path, expectedByteSha256, actual, actual == expectedByteSha256, classificationProperty, document.RootElement.Clone(), actual == expectedByteSha256 ? "loaded" : "byte-hash-mismatch");
        }
        catch (JsonException)
        {
            return new(path, expectedByteSha256, actual, false, classificationProperty, default, "invalid-json");
        }
    }
    internal void FinalizeValidity(bool valid)
    {
        schemaAndBoundaryValid = valid;
        if (ByteHashValid && !valid) Reason = "schema-contract-or-boundary-drift";
        else if (Valid) Reason = "exact";
    }
    internal bool Has(string name, params JsonValueKind[] kinds) => root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var value) && kinds.Contains(value.ValueKind);
    internal string String(string name) => Has(name, JsonValueKind.String) ? root.GetProperty(name).GetString()! : "";
    internal bool Bool(string name) => Has(name, JsonValueKind.True);
    internal int Int(string name) => Has(name, JsonValueKind.Number) && root.GetProperty(name).TryGetInt32(out int value) ? value : int.MinValue;
    internal string[] Strings(string name) => Has(name, JsonValueKind.Array) ? root.GetProperty(name).EnumerateArray().Select(item => item.GetString() ?? "").ToArray() : Array.Empty<string>();
    internal JsonElement? Element(string name) => root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var value) ? value.Clone() : null;
    internal JsonValueKind Kind(string name) => root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var value) ? value.ValueKind : JsonValueKind.Undefined;
    internal object Output() => new { path = Path, expectedByteSha256 = ExpectedByteSha256, actualByteSha256 = ActualByteSha256, byteHashValid = ByteHashValid, schemaAndBoundaryValid, valid = Valid, classification = Classification, reason = Reason };
    private static string Sha256Local(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

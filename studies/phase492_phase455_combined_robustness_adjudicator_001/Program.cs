using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string Phase490Path = "studies/phase490_zero_mode_quotient_audit_001/output/zero_mode_quotient_audit_summary.json";
const string Phase491Path = "studies/phase491_committed_bosonic_model_family_sensitivity_001/output/committed_bosonic_model_family_sensitivity_summary.json";
const string OutputDir = "studies/phase492_phase455_combined_robustness_adjudicator_001/output";
const string Slug = "phase455_combined_robustness_adjudicator";
const string DecisionContractCanonical =
    "phase492-a7-v1|precedence=invalid-precursor>stable-null>stable-candidate-well>localized-assumption-dependence>underdetermined-external-interpretation|" +
    "stable-null=unique-admissible-quotient&&all-admissible-branches-null|" +
    "stable-candidate-well=unique-admissible-quotient&&all-admissible-branches-candidate-well|" +
    "localized=named-assumption-partitions-outcomes|otherwise=underdetermined|exploration-only-no-gate-translation";

string[] phase490RequiredFields =
{
    "phaseId", "terminalStatus", "verdictKind", "inputsValid", "quotientClassification",
    "uniqueAdmissibleQuotient", "decisionContractSha256",
};
string[] phase491RequiredFields =
{
    "phaseId", "terminalStatus", "verdictKind", "inputsValid", "sensitivityClassification",
    "admissibleBranchCount", "allAdmissibleBranchesNull", "allAdmissibleBranchesCandidateWell",
    "namedAssumptionPartitionsOutcomes", "namedOutcomePartitionAssumptions", "decisionContractSha256",
    "frozenSensitivityContractSha256", "sensitivityTable", "summary",
};
string[] quotientTaxonomy =
{
    "unique-quotient-derived", "quotient-family-derived", "quotient-underdetermined", "invalid-committed-inputs",
};
string[] sensitivityTaxonomy =
{
    "stable-null", "stable-candidate-well", "model-convention-fragile", "invalid-inputs",
};

var errors = new List<string>();
PrecursorRead phase490 = ReadAndFreeze(Phase490Path, errors);
PrecursorRead phase491 = ReadAndFreeze(Phase491Path, errors);

bool phase490SchemaValid = ValidatePhase490(phase490.Document, errors, out Phase490Semantic p490);
bool phase491SchemaValid = ValidatePhase491(phase491.Document, errors, out Phase491Semantic p491);

// Re-hash after semantic consumption. Any concurrent or accidental precursor mutation fails closed.
string? phase490HashAfter = HashFileIfPresent(Phase490Path);
string? phase491HashAfter = HashFileIfPresent(Phase491Path);
bool phase490HashStable = phase490.Sha256 is not null && phase490.Sha256 == phase490HashAfter;
bool phase491HashStable = phase491.Sha256 is not null && phase491.Sha256 == phase491HashAfter;
if (!phase490HashStable) errors.Add("Phase490 precursor bytes changed or disappeared after hash freeze.");
if (!phase491HashStable) errors.Add("Phase491 precursor bytes changed or disappeared after hash freeze.");

bool invalidPrecursor = errors.Count != 0 || !phase490SchemaValid || !phase491SchemaValid ||
    !phase490HashStable || !phase491HashStable || !p490.InputsValid || !p491.InputsValid ||
    p490.QuotientClassification == "invalid-committed-inputs" || p491.SensitivityClassification == "invalid-inputs";
bool uniqueAdmissibleQuotient = !invalidPrecursor && p490.UniqueAdmissibleQuotient &&
    p490.QuotientClassification == "unique-quotient-derived";
bool stableNull = uniqueAdmissibleQuotient && p491.AdmissibleBranchCount > 0 &&
    p491.AllAdmissibleBranchesNull && !p491.AllAdmissibleBranchesCandidateWell;
bool stableCandidateWell = uniqueAdmissibleQuotient && p491.AdmissibleBranchCount > 0 &&
    p491.AllAdmissibleBranchesCandidateWell && !p491.AllAdmissibleBranchesNull;
bool namedAssumptionPartition = !invalidPrecursor && p491.NamedAssumptionPartitionsOutcomes &&
    p491.NamedOutcomePartitionAssumptions.Length > 0;

string verdictKind = invalidPrecursor ? "invalid-precursor"
    : stableNull ? "stable-null"
    : stableCandidateWell ? "stable-candidate-well"
    : namedAssumptionPartition ? "localized-assumption-dependence"
    : "underdetermined-external-interpretation";

bool decisionPrecedenceExact = verdictKind switch
{
    "invalid-precursor" => invalidPrecursor,
    "stable-null" => !invalidPrecursor && stableNull,
    "stable-candidate-well" => !invalidPrecursor && !stableNull && stableCandidateWell,
    "localized-assumption-dependence" => !invalidPrecursor && !stableNull && !stableCandidateWell && namedAssumptionPartition,
    "underdetermined-external-interpretation" => !invalidPrecursor && !stableNull && !stableCandidateWell && !namedAssumptionPartition,
    _ => false,
};

const int upstreamMutationCount = 0;
const bool phase455TerminalMutated = false;
const bool phase455T1OrT2Emitted = false;
const bool phase458G3Satisfied = false;
const bool phase458G5Satisfied = false;
const bool humanRulingAuthored = false;
const bool o4Discharged = false;
const bool phase458EvaluationAuthorized = false;
const bool binderLaunchAuthorized = false;
const bool samplingAuthorized = false;
const bool productionAuthorized = false;
const bool sourceContractApplicationAllowed = false;
const bool noGevPromotion = true;
const int promotedPhysicalMassClaimCount = 0;
const int acceptedContractFieldCount = 0;
const int fieldsAppliedToPhase201TemplateCount = 0;
bool a7BoundaryHeld = upstreamMutationCount == 0 && !phase455TerminalMutated && !phase455T1OrT2Emitted &&
    !phase458G3Satisfied && !phase458G5Satisfied && !humanRulingAuthored && !o4Discharged &&
    !phase458EvaluationAuthorized && !binderLaunchAuthorized && !samplingAuthorized && !productionAuthorized &&
    !sourceContractApplicationAllowed && noGevPromotion && promotedPhysicalMassClaimCount == 0 &&
    acceptedContractFieldCount == 0 && fieldsAppliedToPhase201TemplateCount == 0;

var result = new
{
    phaseId = "phase492-phase455-combined-robustness-adjudicator",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = $"phase455-combined-robustness-adjudicator-{verdictKind}",
    verdictKind,
    applicationSubjectKind = "phase455-combined-robustness-adjudicator",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A7; PHASE455_CONVENTION_CLOSURE_PLAN_2026-07-15 Phase492",
    zeroPhysicsCompute = true,
    prospectiveAdjudicator = true,
    explorationLane = true,
    explorationOnly = true,
    confirmationEvidence = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    decisionContract = new
    {
        schemaId = "phase492-a7-combined-adjudicator-v1",
        canonical = DecisionContractCanonical,
        sha256 = Sha256Text(DecisionContractCanonical),
        precedence = new[]
        {
            "invalid-precursor", "stable-null", "stable-candidate-well",
            "localized-assumption-dependence", "underdetermined-external-interpretation",
        },
        phase490RequiredFields,
        phase491RequiredFields,
        quotientTaxonomy,
        sensitivityTaxonomy,
        decisionPrecedenceExact,
    },
    frozenPrecursors = new
    {
        phase490 = new
        {
            path = Phase490Path,
            present = phase490.Present,
            sha256FrozenBeforeConsumption = phase490.Sha256,
            sha256AfterConsumption = phase490HashAfter,
            byteCount = phase490.ByteCount,
            hashStable = phase490HashStable,
            schemaValid = phase490SchemaValid,
            decisionContractSha256 = p490.DecisionContractSha256,
        },
        phase491 = new
        {
            path = Phase491Path,
            present = phase491.Present,
            sha256FrozenBeforeConsumption = phase491.Sha256,
            sha256AfterConsumption = phase491HashAfter,
            byteCount = phase491.ByteCount,
            hashStable = phase491HashStable,
            schemaValid = phase491SchemaValid,
            frozenSensitivityContractSha256 = p491.DecisionContractSha256,
        },
    },
    precursorSemantics = new
    {
        phase490 = p490,
        phase491 = p491,
        invalidPrecursor,
        uniqueAdmissibleQuotient,
        stableNull,
        stableCandidateWell,
        namedAssumptionPartition,
        inputErrors = errors,
    },
    adjudication = new
    {
        classification = verdictKind,
        namedAssumptionPartitions = verdictKind == "localized-assumption-dependence"
            ? p491.NamedOutcomePartitionAssumptions : Array.Empty<string>(),
        externalInterpretationStillRequired = verdictKind is "localized-assumption-dependence" or "underdetermined-external-interpretation",
        gateBearingUseRequiresNewProspectiveConfirmation = true,
        phase455Mapping = "forbidden",
        phase458Mapping = "forbidden",
    },
    upstreamMutationCount,
    phase455TerminalMutated,
    phase455T1OrT2Emitted,
    phase458G3Satisfied,
    phase458G5Satisfied,
    humanRulingAuthored,
    o4Discharged,
    phase458EvaluationAuthorized,
    binderLaunchAuthorized,
    phase458GateSatisfied = false,
    samplingAuthorized,
    productionAuthorized,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    acceptedContractFieldCount,
    fieldsAppliedToPhase201TemplateCount,
    noGevPromotion,
    promotedPhysicalMassClaimCount,
    a7BoundaryHeld,
    decision = verdictKind switch
    {
        "invalid-precursor" => "At least one exact precursor is missing, malformed, invalid, mutated, or schema-drifted. The adjudicator fails closed and authorizes nothing.",
        "stable-null" => "A unique admissible quotient and null outcome on every admissible branch survive the exploratory table. This remains exploration-only and is not Phase455 T1 or Phase458 evidence.",
        "stable-candidate-well" => "A unique admissible quotient and candidate-well outcome on every admissible branch survive the exploratory table. This remains exploration-only and is not Phase455 T2 or Phase458 evidence.",
        "localized-assumption-dependence" => "The exploratory outcome changes across explicitly named assumption partitions. The dependence is localized but not externally adjudicated.",
        _ => "The committed exploratory evidence does not select a unique robust interpretation. External interpretation or a new prospective confirmation design is required.",
    },
};

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, $"{Slug}.json"), json);
File.WriteAllText(Path.Combine(OutputDir, $"{Slug}_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"decisionPrecedenceExact={decisionPrecedenceExact} a7BoundaryHeld={a7BoundaryHeld} promotedPhysicalMassClaimCount=0");

static PrecursorRead ReadAndFreeze(string path, List<string> errors)
{
    if (!File.Exists(path))
    {
        errors.Add($"Missing precursor: {path}");
        return new PrecursorRead(false, null, 0, null);
    }
    try
    {
        byte[] bytes = File.ReadAllBytes(path);
        string hash = Sha256Bytes(bytes);
        var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { AllowTrailingCommas = false, CommentHandling = JsonCommentHandling.Disallow });
        return new PrecursorRead(true, hash, bytes.LongLength, document);
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
    {
        errors.Add($"Unreadable precursor {path}: {ex.GetType().Name}");
        return new PrecursorRead(true, null, 0, null);
    }
}

static bool ValidatePhase490(JsonDocument? document, List<string> errors, out Phase490Semantic semantic)
{
    semantic = Phase490Semantic.Empty;
    if (document is null) return false;
    JsonElement root = document.RootElement;
    string phaseId = String(root, "phaseId");
    string terminal = String(root, "terminalStatus");
    string verdict = String(root, "verdictKind");
    string classification = String(root, "quotientClassification");
    bool valid = Bool(root, "inputsValid");
    bool unique = Bool(root, "uniqueAdmissibleQuotient");
    string contractHash = String(root, "decisionContractSha256");
    bool schema = phaseId == "phase490-zero-mode-quotient-audit" && verdict == classification &&
        terminal == $"zero-mode-quotient-audit-{verdict}" &&
        new[] { "unique-quotient-derived", "quotient-family-derived", "quotient-underdetermined", "invalid-committed-inputs" }.Contains(classification) &&
        contractHash.Length == 64 && contractHash.All(Uri.IsHexDigit) &&
        unique == (classification == "unique-quotient-derived");
    if (!schema) errors.Add("Phase490 required identity, taxonomy, quotient, or contract-hash fields are missing or inconsistent.");
    semantic = new Phase490Semantic(valid, classification, unique, contractHash);
    return schema;
}

static bool ValidatePhase491(JsonDocument? document, List<string> errors, out Phase491Semantic semantic)
{
    semantic = Phase491Semantic.Empty;
    if (document is null) return false;
    JsonElement root = document.RootElement;
    string phaseId = String(root, "phaseId");
    string terminal = String(root, "terminalStatus");
    string verdict = String(root, "verdictKind");
    string classification = String(root, "sensitivityClassification");
    bool valid = Bool(root, "inputsValid");
    string contractHash = String(root, "decisionContractSha256");
    string frozenContractHash = String(root, "frozenSensitivityContractSha256");
    JsonElement table = root.TryGetProperty("sensitivityTable", out var tableValue) ? tableValue : default;
    JsonElement summary = root.TryGetProperty("summary", out var summaryValue) ? summaryValue : default;
    int totalRows = Integer(summary, "totalRows");
    int validRows = Integer(summary, "validRows");
    int invalidRows = Integer(summary, "invalidRows");
    int nullRows = Integer(summary, "nullRows");
    int candidateRows = Integer(summary, "candidateWellRows");
    bool tableIsArray = table.ValueKind == JsonValueKind.Array;
    int tableRows = tableIsArray ? table.GetArrayLength() : -1;
    bool rowStatusesValid = tableIsArray && table.EnumerateArray().All(row =>
        row.ValueKind == JsonValueKind.Object &&
        String(row, "status") is "null" or "candidate-well" or "invalid");
    int derivedNullRows = tableIsArray ? table.EnumerateArray().Count(row => String(row, "status") == "null") : -1;
    int derivedCandidateRows = tableIsArray ? table.EnumerateArray().Count(row => String(row, "status") == "candidate-well") : -1;
    int derivedInvalidRows = tableIsArray ? table.EnumerateArray().Count(row => String(row, "status") == "invalid") : -1;
    bool countsExact = tableRows == totalRows && validRows == nullRows + candidateRows && invalidRows == derivedInvalidRows &&
        nullRows == derivedNullRows && candidateRows == derivedCandidateRows && totalRows == validRows + invalidRows;
    int branchCount = Integer(root, "admissibleBranchCount");
    bool allNull = Bool(root, "allAdmissibleBranchesNull");
    bool allCandidate = Bool(root, "allAdmissibleBranchesCandidateWell");
    bool namedPartitions = Bool(root, "namedAssumptionPartitionsOutcomes");
    string[] names = Strings(root, "namedOutcomePartitionAssumptions");
    bool aggregatesExact = branchCount == validRows &&
        allNull == (validRows > 0 && nullRows == validRows && candidateRows == 0) &&
        allCandidate == (validRows > 0 && candidateRows == validRows && nullRows == 0) &&
        namedPartitions == (validRows > 0 && nullRows > 0 && candidateRows > 0);
    bool classificationSemantics = classification switch
    {
        "stable-null" => branchCount > 0 && allNull && !allCandidate && !namedPartitions,
        "stable-candidate-well" => branchCount > 0 && allCandidate && !allNull && !namedPartitions,
        "model-convention-fragile" => branchCount > 0 && !allNull && !allCandidate && namedPartitions && names.Length > 0,
        "invalid-inputs" => !valid,
        _ => false,
    };
    bool schema = phaseId == "phase491-committed-bosonic-model-family-sensitivity" && verdict == classification &&
        terminal == $"committed-bosonic-model-family-sensitivity-{verdict}" &&
        contractHash.Length == 64 && contractHash.All(Uri.IsHexDigit) && contractHash == frozenContractHash &&
        rowStatusesValid && countsExact && aggregatesExact && classificationSemantics;
    if (!schema) errors.Add("Phase491 required identity, taxonomy, branch-table, partition, or contract-hash fields are missing or inconsistent.");
    semantic = new Phase491Semantic(valid, classification, branchCount, allNull, allCandidate, namedPartitions, names, contractHash);
    return schema;
}

static string String(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
    ? value.GetString() ?? string.Empty : string.Empty;
static bool Bool(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
static int Integer(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.TryGetInt32(out int result) ? result : -1;
static string[] Strings(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array &&
    value.EnumerateArray().All(x => x.ValueKind == JsonValueKind.String)
        ? value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => x.Length > 0).Distinct(StringComparer.Ordinal).ToArray()
        : Array.Empty<string>();
static string? HashFileIfPresent(string path) => File.Exists(path) ? Sha256Bytes(File.ReadAllBytes(path)) : null;
static string Sha256Text(string text) => Sha256Bytes(Encoding.UTF8.GetBytes(text));
static string Sha256Bytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

sealed class PrecursorRead(bool present, string? sha256, long byteCount, JsonDocument? document)
{
    public bool Present { get; } = present;
    public string? Sha256 { get; } = sha256;
    public long ByteCount { get; } = byteCount;
    public JsonDocument? Document { get; } = document;
}

sealed class Phase490Semantic(bool inputsValid, string quotientClassification, bool uniqueAdmissibleQuotient, string decisionContractSha256)
{
    public static Phase490Semantic Empty { get; } = new(false, "missing", false, string.Empty);
    public bool InputsValid { get; } = inputsValid;
    public string QuotientClassification { get; } = quotientClassification;
    public bool UniqueAdmissibleQuotient { get; } = uniqueAdmissibleQuotient;
    public string DecisionContractSha256 { get; } = decisionContractSha256;
}

sealed class Phase491Semantic(bool inputsValid, string sensitivityClassification, int admissibleBranchCount,
    bool allAdmissibleBranchesNull, bool allAdmissibleBranchesCandidateWell, bool namedAssumptionPartitionsOutcomes,
    string[] namedOutcomePartitionAssumptions, string decisionContractSha256)
{
    public static Phase491Semantic Empty { get; } = new(false, "missing", -1, false, false, false, Array.Empty<string>(), string.Empty);
    public bool InputsValid { get; } = inputsValid;
    public string SensitivityClassification { get; } = sensitivityClassification;
    public int AdmissibleBranchCount { get; } = admissibleBranchCount;
    public bool AllAdmissibleBranchesNull { get; } = allAdmissibleBranchesNull;
    public bool AllAdmissibleBranchesCandidateWell { get; } = allAdmissibleBranchesCandidateWell;
    public bool NamedAssumptionPartitionsOutcomes { get; } = namedAssumptionPartitionsOutcomes;
    public string[] NamedOutcomePartitionAssumptions { get; } = namedOutcomePartitionAssumptions;
    public string DecisionContractSha256 { get; } = decisionContractSha256;
}

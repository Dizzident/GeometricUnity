using System.Text.Json;

const string DefaultOutputDir = "studies/phase150_all_boson_prediction_prerequisite_execution_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase149Path = "studies/phase149_known_boson_predictability_contracts_001/output/known_boson_predictability_contracts.json";
const string PhysicalModeRecordsPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/physical_mode_records.json";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string BosonAtlasPath = "studies/phase12_joined_calculation_001/output/background_family/reports/boson_atlas.json";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase146Path = "studies/phase146_fermion_sector_evidence_census_001/output/fermion_sector_evidence_census.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE150_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase149 = JsonDocument.Parse(File.ReadAllText(Phase149Path));
using var physicalModes = JsonDocument.Parse(File.ReadAllText(PhysicalModeRecordsPath));
using var registry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var atlas = JsonDocument.Parse(File.ReadAllText(BosonAtlasPath));
using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase146 = JsonDocument.Parse(File.ReadAllText(Phase146Path));

var physicalModeParticles = physicalModes.RootElement.EnumerateArray()
    .Select(mode => RequiredString(mode, "particleId"))
    .Distinct(StringComparer.Ordinal)
    .OrderBy(x => x, StringComparer.Ordinal)
    .ToArray();
var registryCandidates = registry.RootElement.GetProperty("candidates").EnumerateArray()
    .Select(candidate => new
    {
        candidateId = RequiredString(candidate, "candidateId"),
        claimClass = JsonString(candidate, "claimClass"),
        branchStabilityScore = JsonDouble(candidate, "branchStabilityScore"),
        massLikeEnvelope = candidate.TryGetProperty("massLikeEnvelope", out var envelope)
            ? envelope.Clone()
            : default,
    })
    .ToArray();
bool hasPromotableNonWzBosonIdentity = physicalModeParticles.Any(particle => particle is not "w-boson" and not "z-boson");
bool hasStableCandidateBeyondWz = registryCandidates.Any(candidate =>
    !string.Equals(candidate.claimClass, "C0_NumericalMode", StringComparison.Ordinal)
    && candidate.branchStabilityScore >= 0.5);
bool correctedWzProjectionReady = string.Equals(
    JsonString(phase122.RootElement, "terminalStatus"),
    "corrected-operator-selection-rule-sweep-found-projection-candidate",
    StringComparison.Ordinal);
bool fermionSectorEvidencePresent = JsonBool(phase146.RootElement, "currentEvidencePresent") is true;

var comparisonRows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray()
    .Select(row => new PredictionRow(
        ParticleId: RequiredString(row, "particleId"),
        ObservableId: RequiredString(row, "observableId"),
        Status: RequiredString(row, "status"),
        PredictedValue: JsonDouble(row, "predictedValue"),
        PredictedUncertainty: JsonDouble(row, "predictedUncertainty"),
        TargetValue: JsonDouble(row, "targetValue"),
        TargetUncertainty: JsonDouble(row, "targetUncertainty"),
        Unit: JsonString(row, "unit"),
        PullOrSigmaResidual: JsonDouble(row, "pullOrSigmaResidual"),
        Passed: JsonBool(row, "passed"),
        PromotionAllowed: string.Equals(RequiredString(row, "status"), "predicted", StringComparison.Ordinal),
        ClosureRequirements: StringArray(row, "closureRequirements")))
    .ToArray();

var prerequisiteAttempts = new[]
{
    new
    {
        prerequisiteId = "validated-physical-mode-identities",
        status = hasPromotableNonWzBosonIdentity ? "non-wz-identities-present" : "wz-only",
        diagnosis = hasPromotableNonWzBosonIdentity
            ? "physical mode records include non-W/Z identities"
            : "physical mode records only validate W and Z identities",
    },
    new
    {
        prerequisiteId = "stable-boson-registry-candidates",
        status = hasStableCandidateBeyondWz ? "stable-candidates-present" : "no-stable-nonpromoted-candidates",
        diagnosis = hasStableCandidateBeyondWz
            ? "at least one non-C0 registry candidate exists"
            : "registry candidates remain branch-fragile C0 numerical modes",
    },
    new
    {
        prerequisiteId = "corrected-wz-transition-or-bridge",
        status = correctedWzProjectionReady ? "ready" : "blocked",
        diagnosis = correctedWzProjectionReady
            ? "corrected W/Z operator sweep found a projection candidate"
            : "corrected W/Z operator sweep found no projection candidate",
    },
    new
    {
        prerequisiteId = "fermion-sector-evidence",
        status = fermionSectorEvidencePresent ? "present" : "absent",
        diagnosis = fermionSectorEvidencePresent
            ? "P146 found a local evidence candidate"
            : "P146 found no non-synthetic local fermion-sector evidence candidate",
    },
};

bool allRowsPromotable = comparisonRows.All(row => row.PromotionAllowed);
string terminalStatus = allRowsPromotable
    ? "all-boson-predictions-promotable"
    : "all-boson-prediction-prerequisites-executed-partial";

var result = new
{
    phaseId = "phase150-all-boson-prediction-prerequisite-execution",
    terminalStatus,
    requestedAllBosonPredictionsPromotable = allRowsPromotable,
    predictionRowCount = comparisonRows.Length,
    promotablePredictionCount = comparisonRows.Count(row => row.PromotionAllowed),
    failedAttemptCount = comparisonRows.Count(row => row.Status == "failed-comparison-attempt-not-promoted"),
    blockedPredictionCount = comparisonRows.Count(row => row.Status.StartsWith("blocked-", StringComparison.Ordinal)),
    predictionRows = comparisonRows,
    prerequisiteAttempts,
    contractReadiness = new
    {
        phase149Status = JsonString(phase149.RootElement, "terminalStatus"),
        allContractsReady = JsonBool(phase149.RootElement, "allContractsReady"),
        readyContractCount = JsonInt(phase149.RootElement, "readyContractCount"),
        openContractCount = JsonInt(phase149.RootElement, "openContractCount"),
    },
    localIdentityInventory = new
    {
        physicalModeParticles,
        registryCandidateCount = registryCandidates.Length,
        stableCandidateCount = registryCandidates.Count(candidate =>
            !string.Equals(candidate.claimClass, "C0_NumericalMode", StringComparison.Ordinal)
            && candidate.branchStabilityScore >= 0.5),
        atlasConvergenceSheets = atlas.RootElement.TryGetProperty("spectrumSheets", out var sheets) && sheets.ValueKind == JsonValueKind.Array
            ? sheets.GetArrayLength()
            : 0,
    },
    cannotProvidePromotedAllBosonPredictionsReason = allRowsPromotable
        ? null
        : "current local artifacts only support the W/Z mass-ratio prediction; other rows lack promotable target-independent identity, transition, source, or benchmark evidence",
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase149Path = Phase149Path,
        physicalModeRecordsPath = PhysicalModeRecordsPath,
        bosonRegistryPath = BosonRegistryPath,
        bosonAtlasPath = BosonAtlasPath,
        phase122Path = Phase122Path,
        phase146Path = Phase146Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "all_boson_prediction_prerequisite_execution.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "all_boson_prediction_prerequisite_execution_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.requestedAllBosonPredictionsPromotable,
        result.predictionRowCount,
        result.promotablePredictionCount,
        result.failedAttemptCount,
        result.blockedPredictionCount,
        result.predictionRows,
        result.prerequisiteAttempts,
        result.contractReadiness,
        result.localIdentityInventory,
        result.cannotProvidePromotedAllBosonPredictionsReason,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"promotablePredictionCount={result.promotablePredictionCount}");
Console.WriteLine($"failedAttemptCount={result.failedAttemptCount}");
Console.WriteLine($"blockedPredictionCount={result.blockedPredictionCount}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .ToArray()
        : Array.Empty<string>();

sealed record PredictionRow(
    string ParticleId,
    string ObservableId,
    string Status,
    double? PredictedValue,
    double? PredictedUncertainty,
    double? TargetValue,
    double? TargetUncertainty,
    string? Unit,
    double? PullOrSigmaResidual,
    bool? Passed,
    bool PromotionAllowed,
    IReadOnlyList<string> ClosureRequirements);

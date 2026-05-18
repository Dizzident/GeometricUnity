using System.Text.Json;

const string DefaultOutputDir = "studies/phase174_protected_massless_subspace_audit_001/output";
const string AtlasPath = "studies/phase12_joined_calculation_001/output/background_family/reports/boson_atlas.json";
const string RegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string Phase173Path = "studies/phase173_next_prediction_route_selection_001/output/next_prediction_route_selection.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE174_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var atlas = JsonDocument.Parse(File.ReadAllText(AtlasPath));
using var registry = JsonDocument.Parse(File.ReadAllText(RegistryPath));
using var phase173 = File.Exists(Phase173Path) ? JsonDocument.Parse(File.ReadAllText(Phase173Path)) : null;

double masslessTolerance = 1e-12;
double gaugeLeakTolerance = 1e-12;
double branchStabilityThreshold = 0.5;

var sheets = atlas.RootElement.GetProperty("spectrumSheets").EnumerateArray()
    .Select(sheet =>
    {
        var eigenvalues = NumberArray(sheet, "eigenvalues");
        var gaugeLeaks = NumberArray(sheet, "gaugeLeaks");
        int masslessCount = eigenvalues.Count(value => Math.Abs(value) <= masslessTolerance);
        int gaugeLeakFreeCount = gaugeLeaks.Count(value => Math.Abs(value) <= gaugeLeakTolerance);
        return new SpectrumSheetAudit(
            RequiredString(sheet, "backgroundId"),
            JsonString(sheet, "convergenceStatus") ?? "unknown",
            JsonInt(sheet, "modeCount") ?? eigenvalues.Length,
            masslessCount,
            gaugeLeakFreeCount,
            eigenvalues.Select(Math.Abs).DefaultIfEmpty(0.0).Max(),
            gaugeLeaks.Select(Math.Abs).DefaultIfEmpty(0.0).Max());
    })
    .ToArray();
var candidates = registry.RootElement.GetProperty("candidates").EnumerateArray()
    .Select(candidate => new CandidateAudit(
        RequiredString(candidate, "candidateId"),
        JsonString(candidate, "claimClass") ?? "unknown",
        JsonDouble(candidate, "branchStabilityScore") ?? 0.0,
        NumberArray(candidate, "massLikeEnvelope").Select(Math.Abs).DefaultIfEmpty(0.0).Max(),
        NumberArray(candidate, "gaugeLeakEnvelope").Select(Math.Abs).DefaultIfEmpty(0.0).Max()))
    .ToArray();

bool allConverged = sheets.All(sheet => string.Equals(sheet.ConvergenceStatus, "converged", StringComparison.Ordinal));
bool allModesMassless = sheets.All(sheet => sheet.MasslessModeCount == sheet.ModeCount);
bool allModesGaugeLeakFree = sheets.All(sheet => sheet.GaugeLeakFreeModeCount == sheet.ModeCount);
bool subspaceDimensionStable = sheets.Select(sheet => sheet.MasslessModeCount).Distinct().Count() == 1;
int protectedSubspaceDimension = subspaceDimensionStable ? sheets.First().MasslessModeCount : 0;
int branchStableCandidateCount = candidates.Count(candidate =>
    candidate.BranchStabilityScore >= branchStabilityThreshold
    && !string.Equals(candidate.ClaimClass, "C0_NumericalMode", StringComparison.Ordinal));
bool identitySplitPresent = false;
bool benchmarkContractsPresent = false;
bool subspaceDiagnosticReady = allConverged && allModesMassless && allModesGaugeLeakFree && subspaceDimensionStable;
bool knownPhotonGluonPredictionAllowed = subspaceDiagnosticReady && identitySplitPresent && benchmarkContractsPresent && branchStableCandidateCount > 0;

string terminalStatus = knownPhotonGluonPredictionAllowed
    ? "protected-massless-subspace-known-boson-prediction-ready"
    : subspaceDiagnosticReady
        ? "protected-massless-subspace-diagnostic-ready-identity-split-blocked"
        : "protected-massless-subspace-diagnostic-failed";

var result = new
{
    phaseId = "phase174-protected-massless-subspace-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    masslessTolerance,
    gaugeLeakTolerance,
    branchStabilityThreshold,
    allConverged,
    allModesMassless,
    allModesGaugeLeakFree,
    subspaceDimensionStable,
    protectedSubspaceDimension,
    subspaceDiagnosticReady,
    identitySplitPresent,
    benchmarkContractsPresent,
    branchStableCandidateCount,
    knownPhotonGluonPredictionAllowed,
    diagnosticPrediction = subspaceDiagnosticReady
        ? new
        {
            observableId = "protected-massless-gauge-subspace-dimension",
            predictedValue = protectedSubspaceDimension,
            unit = "dimension",
            externalTargetsUsed = false,
            promotionAllowedForKnownPhotonGluon = false,
        }
        : null,
    sheetAudits = sheets,
    candidateAuditSummary = new
    {
        candidateCount = candidates.Length,
        branchStableCandidateCount,
        maxCandidateMassLikeAbs = candidates.Select(candidate => candidate.MaxMassLikeAbs).DefaultIfEmpty(0.0).Max(),
        maxCandidateGaugeLeakAbs = candidates.Select(candidate => candidate.MaxGaugeLeakAbs).DefaultIfEmpty(0.0).Max(),
        claimClasses = candidates.Select(candidate => candidate.ClaimClass).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
    },
    blockers = knownPhotonGluonPredictionAllowed
        ? Array.Empty<string>()
        : new[]
        {
            "protected massless subspace is only a sector-level diagnostic, not a photon/gluon identity split",
            "no U(1) photon identity rule is present",
            "no color-octet gluon identity rule is present",
            "no active photon/gluon benchmark contracts are present",
            "individual registry candidates remain branch-fragile C0 numerical modes",
        },
    nextWork = knownPhotonGluonPredictionAllowed
        ? "wire the protected massless subspace into photon/gluon prediction rows"
        : "derive the U(1) photon and color-octet gluon identity split and benchmark contracts from the protected massless subspace before claiming photon/gluon predictions",
    sourceEvidence = new
    {
        atlasPath = AtlasPath,
        registryPath = RegistryPath,
        phase173Path = File.Exists(Phase173Path) ? Phase173Path : null,
        phase173Status = phase173 is null ? null : JsonString(phase173.RootElement, "terminalStatus"),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "protected_massless_subspace_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "protected_massless_subspace_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.protectedSubspaceDimension,
        result.subspaceDiagnosticReady,
        result.knownPhotonGluonPredictionAllowed,
        result.diagnosticPrediction,
        result.candidateAuditSummary,
        result.blockers,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"protectedSubspaceDimension={protectedSubspaceDimension}");
Console.WriteLine($"subspaceDiagnosticReady={subspaceDiagnosticReady}");
Console.WriteLine($"knownPhotonGluonPredictionAllowed={knownPhotonGluonPredictionAllowed}");

static double[] NumberArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Number).Select(item => item.GetDouble()).ToArray()
        : Array.Empty<double>();
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record SpectrumSheetAudit(
    string BackgroundId,
    string ConvergenceStatus,
    int ModeCount,
    int MasslessModeCount,
    int GaugeLeakFreeModeCount,
    double MaxAbsEigenvalue,
    double MaxAbsGaugeLeak);
sealed record CandidateAudit(
    string CandidateId,
    string ClaimClass,
    double BranchStabilityScore,
    double MaxMassLikeAbs,
    double MaxGaugeLeakAbs);

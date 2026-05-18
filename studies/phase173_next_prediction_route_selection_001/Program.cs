using System.Text.Json;

const string DefaultOutputDir = "studies/phase173_next_prediction_route_selection_001/output";
const string Phase51Path = "studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json";
const string Phase117Path = "studies/phase117_wz_repaired_pair_sweep_001/output/wz_repaired_pair_sweep.json";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census.json";
const string Phase182Path = "studies/phase182_wz_operator_algebra_bridge_source_001/output/wz_operator_algebra_bridge_source.json";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";
const string Phase175Path = "studies/phase175_massless_gauge_identity_split_feasibility_001/output/massless_gauge_identity_split_feasibility.json";
const string Phase176Path = "studies/phase176_massless_cartan_line_stability_001/output/massless_cartan_line_stability.json";
const string Phase177Path = "studies/phase177_massless_benchmark_contracts_001/output/massless_benchmark_contracts.json";
const string Phase178Path = "studies/phase178_protected_massless_subspace_transport_001/output/protected_massless_subspace_transport.json";
const string Phase179Path = "studies/phase179_gauge_frame_aligned_subspace_transport_001/output/gauge_frame_aligned_subspace_transport.json";
const string Phase183Path = "studies/phase183_massless_sector_invariant_prediction_001/output/massless_sector_invariant_prediction.json";
const string RegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string AtlasPath = "studies/phase12_joined_calculation_001/output/background_family/reports/boson_atlas.json";
const string TargetsPath = "studies/phase18_experimental_targets_001/physical_targets.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE173_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase51 = JsonDocument.Parse(File.ReadAllText(Phase51Path));
using var phase117 = JsonDocument.Parse(File.ReadAllText(Phase117Path));
using var phase172 = JsonDocument.Parse(File.ReadAllText(Phase172Path));
using var phase182 = File.Exists(Phase182Path) ? JsonDocument.Parse(File.ReadAllText(Phase182Path)) : null;
using var phase174 = File.Exists(Phase174Path) ? JsonDocument.Parse(File.ReadAllText(Phase174Path)) : null;
using var phase175 = File.Exists(Phase175Path) ? JsonDocument.Parse(File.ReadAllText(Phase175Path)) : null;
using var phase176 = File.Exists(Phase176Path) ? JsonDocument.Parse(File.ReadAllText(Phase176Path)) : null;
using var phase177 = File.Exists(Phase177Path) ? JsonDocument.Parse(File.ReadAllText(Phase177Path)) : null;
using var phase178 = File.Exists(Phase178Path) ? JsonDocument.Parse(File.ReadAllText(Phase178Path)) : null;
using var phase179 = File.Exists(Phase179Path) ? JsonDocument.Parse(File.ReadAllText(Phase179Path)) : null;
using var phase183 = File.Exists(Phase183Path) ? JsonDocument.Parse(File.ReadAllText(Phase183Path)) : null;
using var registry = JsonDocument.Parse(File.ReadAllText(RegistryPath));
using var atlas = JsonDocument.Parse(File.ReadAllText(AtlasPath));
using var targets = JsonDocument.Parse(File.ReadAllText(TargetsPath));

var readinessRecords = phase51.RootElement.GetProperty("records").EnumerateArray()
    .ToDictionary(record => RequiredString(record, "particleId"), record => record.Clone(), StringComparer.Ordinal);
var targetObservableIds = targets.RootElement.GetProperty("targets").EnumerateArray()
    .Select(target => RequiredString(target, "observableId"))
    .ToHashSet(StringComparer.Ordinal);
var candidates = registry.RootElement.GetProperty("candidates").EnumerateArray()
    .Select(candidate => new BosonCandidate(
        RequiredString(candidate, "candidateId"),
        JsonString(candidate, "claimClass") ?? "unknown",
        JsonDouble(candidate, "branchStabilityScore") ?? 0.0,
        JsonDouble(candidate, "refinementStabilityScore") ?? 0.0,
        NumberArray(candidate, "massLikeEnvelope"),
        NumberArray(candidate, "gaugeLeakEnvelope")))
    .ToArray();

double masslessTolerance = 1e-12;
double gaugeLeakTolerance = 1e-12;
double branchStabilityThreshold = 0.5;
bool allSpectrumSheetsConverged = atlas.RootElement.GetProperty("spectrumSheets").EnumerateArray()
    .All(sheet => string.Equals(JsonString(sheet, "convergenceStatus"), "converged", StringComparison.Ordinal));
bool allCandidatesNumericallyMassless = candidates.All(candidate =>
    candidate.MassLikeEnvelope.Length > 0 && candidate.MassLikeEnvelope.All(value => Math.Abs(value) <= masslessTolerance));
bool allCandidatesGaugeLeakFree = candidates.All(candidate =>
    candidate.GaugeLeakEnvelope.Length > 0 && candidate.GaugeLeakEnvelope.All(value => Math.Abs(value) <= gaugeLeakTolerance));
int branchStableCandidateCount = candidates.Count(candidate =>
    candidate.BranchStabilityScore >= branchStabilityThreshold
    && !string.Equals(candidate.ClaimClass, "C0_NumericalMode", StringComparison.Ordinal));
bool photonTargetContractPresent = targetObservableIds.Contains("physical-photon-masslessness");
bool gluonTargetContractPresent = targetObservableIds.Contains("physical-gluon-masslessness");
if (phase177 is not null && phase177.RootElement.TryGetProperty("contracts", out var masslessContracts))
{
    foreach (var contract in masslessContracts.EnumerateArray())
    {
        if (JsonBool(contract, "benchmarkContractPresent") is not true)
            continue;
        string observableId = RequiredString(contract, "observableId");
        if (observableId == "physical-photon-masslessness")
            photonTargetContractPresent = true;
        if (observableId == "physical-gluon-masslessness")
            gluonTargetContractPresent = true;
    }
}
bool photonIdentityPresent = false;
bool gluonIdentityPresent = false;
bool colorSectorPresent = false;
bool u1SectorPresent = false;
bool protectedMasslessSubspaceReady = phase174 is not null && JsonBool(phase174.RootElement, "subspaceDiagnosticReady") is true;
int? protectedMasslessSubspaceDimension = phase174 is null ? null : JsonInt(phase174.RootElement, "protectedSubspaceDimension");
bool identitySplitAttempted = phase175 is not null;
int? gaugeComponentCount = phase175 is null ? null : JsonInt(phase175.RootElement, "gaugeComponentCount");
bool cartanLineAttempted = phase176 is not null;
bool cartanLineCandidatePresent = phase176 is not null && JsonBool(phase176.RootElement, "u1CartanLineCandidatePresent") is true;
double? cartanLinePairwiseDot = phase176 is null ? null : JsonDouble(phase176.RootElement, "pairwiseLineDot");
bool subspaceTransportAttempted = phase178 is not null;
bool subspaceTransportStable = phase178 is not null && JsonBool(phase178.RootElement, "transportStable") is true;
bool alignedSubspaceTransportAttempted = phase179 is not null;
bool alignedSubspaceTransportStable = phase179 is not null && JsonBool(phase179.RootElement, "alignedTransportStable") is true;
bool masslessSectorInvariantAttempted = phase183 is not null;
bool masslessSectorInvariantValidated = phase183 is not null && JsonBool(phase183.RootElement, "sectorInvariantValidated") is true;
bool masslessSectorInvariantPredictionAllowed = phase183 is not null && JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is true;
double? subspaceTransportMinSingularValue = null;
double? alignedSubspaceTransportMinSingularValue = null;
if (phase178 is not null
    && phase178.RootElement.TryGetProperty("pairwiseAudits", out var transportAudits)
    && transportAudits.ValueKind == JsonValueKind.Array)
{
    subspaceTransportMinSingularValue = transportAudits.EnumerateArray()
        .Select(audit => JsonDouble(audit, "minSingularValue"))
        .Where(value => value is not null)
        .DefaultIfEmpty(null)
        .Min();
}
if (phase179 is not null
    && phase179.RootElement.TryGetProperty("alignedAudit", out var alignedAudit)
    && alignedAudit.ValueKind == JsonValueKind.Object)
{
    alignedSubspaceTransportMinSingularValue = JsonDouble(alignedAudit, "minSingularValue");
}
if (phase175 is not null && phase175.RootElement.TryGetProperty("identitySplitAudit", out var identitySplitAudit))
{
    photonIdentityPresent = JsonBool(identitySplitAudit, "u1IdentityCandidatePresent") is true;
    gluonIdentityPresent = JsonBool(identitySplitAudit, "colorOctetIdentityCandidatePresent") is true;
    u1SectorPresent = photonIdentityPresent;
    colorSectorPresent = gluonIdentityPresent;
}
if (cartanLineCandidatePresent)
{
    photonIdentityPresent = true;
    u1SectorPresent = true;
}

var routeAssessments = new[]
{
    new RouteAssessment(
        RouteId: "wz-absolute-mass",
        ParticleIds: new[] { "w-boson", "z-boson" },
        DiagnosticObservablePresent: true,
        TargetContractPresent: true,
        IdentityEvidencePresent: true,
        SourceEvidencePresent: false,
        StabilityEvidencePresent: false,
        PredictionAttemptAllowed: false,
        PromotionAllowed: false,
        DiagnosticPrediction: new
        {
            phase117StrongestRawToTargetRatio = JsonDouble(phase117.RootElement.GetProperty("strongestByRaw"), "maxRawToTargetRatio"),
            phase172BestSubspaceRawToTargetRatio = JsonDouble(phase172.RootElement.GetProperty("bestAssessment"), "minRawToTargetRatio"),
            phase182Attempted = phase182 is not null,
            phase182BestOperatorAlgebraRawToTargetRatio = phase182 is null || !phase182.RootElement.TryGetProperty("bestCandidate", out var phase182Best) ? null : JsonDouble(phase182Best, "minRawToTargetRatio"),
            phase182StableRawGatePassingCandidateCount = phase182 is null ? null : JsonInt(phase182.RootElement, "stableRawGatePassingCandidateCount"),
        },
        Blockers: new[]
        {
            "P172 finds no raw-amplitude-clearing source in the full current Phase12 variation subspace",
            phase182 is null
                ? "operator-algebra bridge-source construction has not been attempted"
                : "P182 finds no Hilbert-Schmidt-normalized operator-algebra bridge source clears the W/Z raw-amplitude gate",
            "P117 repaired-pair sweep is far below the target-independent raw-amplitude gate",
            "absolute W/Z prediction would require a new bridge source or derivation-backed analytic source-shape law",
        },
        NextWork: "derive a W/Z bridge source outside the current Phase12 variation subspace or a derivation-backed analytic source-shape law"),
    new RouteAssessment(
        RouteId: "higgs-scalar-mass",
        ParticleIds: new[] { "higgs" },
        DiagnosticObservablePresent: false,
        TargetContractPresent: targetObservableIds.Contains("physical-higgs-mass-gev"),
        IdentityEvidencePresent: false,
        SourceEvidencePresent: false,
        StabilityEvidencePresent: false,
        PredictionAttemptAllowed: false,
        PromotionAllowed: false,
        DiagnosticPrediction: null,
        Blockers: StringArray(readinessRecords["higgs"], "closureRequirements").ToArray(),
        NextWork: "derive and solve a scalar-sector source/operator with Higgs identity sidecars before any Higgs mass prediction"),
    new RouteAssessment(
        RouteId: "massless-gauge-photon-gluon",
        ParticleIds: new[] { "photon", "gluon" },
        DiagnosticObservablePresent: protectedMasslessSubspaceReady || (allSpectrumSheetsConverged && allCandidatesNumericallyMassless && allCandidatesGaugeLeakFree),
        TargetContractPresent: photonTargetContractPresent && gluonTargetContractPresent,
        IdentityEvidencePresent: photonIdentityPresent && gluonIdentityPresent,
        SourceEvidencePresent: u1SectorPresent && colorSectorPresent,
        StabilityEvidencePresent: branchStableCandidateCount > 0 || subspaceTransportStable || alignedSubspaceTransportStable || masslessSectorInvariantValidated,
        PredictionAttemptAllowed: masslessSectorInvariantPredictionAllowed,
        PromotionAllowed: masslessSectorInvariantPredictionAllowed,
        DiagnosticPrediction: new
        {
            allSpectrumSheetsConverged,
            allCandidatesNumericallyMassless,
            allCandidatesGaugeLeakFree,
            protectedMasslessSubspaceReady,
            protectedMasslessSubspaceDimension,
            identitySplitAttempted,
            gaugeComponentCount,
            cartanLineAttempted,
            cartanLineCandidatePresent,
            cartanLinePairwiseDot,
            masslessBenchmarkContractsAttempted = phase177 is not null,
            subspaceTransportAttempted,
            subspaceTransportStable,
            subspaceTransportMinSingularValue,
            alignedSubspaceTransportAttempted,
            alignedSubspaceTransportStable,
            alignedSubspaceTransportMinSingularValue,
            masslessSectorInvariantAttempted,
            masslessSectorInvariantValidated,
            masslessSectorInvariantPredictionAllowed,
            candidateCount = candidates.Length,
            branchStableCandidateCount,
            maxAbsMassLike = candidates.SelectMany(candidate => candidate.MassLikeEnvelope).Select(Math.Abs).DefaultIfEmpty(0.0).Max(),
            maxGaugeLeak = candidates.SelectMany(candidate => candidate.GaugeLeakEnvelope).Select(Math.Abs).DefaultIfEmpty(0.0).Max(),
            diagnosticValue = 0.0,
            diagnosticUnit = "masslessness-indicator",
            diagnosticPromotionAllowed = false,
        },
        Blockers: new[]
        {
            masslessSectorInvariantPredictionAllowed
                ? "P183 promotes only the zero masslessness-indicator invariant; photon U(1) and color-octet identity splits remain unpromoted"
                : "Phase12 boson modes are numerically massless and gauge-leak-free, but registry candidates are branch-fragile C0 numerical modes",
            masslessSectorInvariantPredictionAllowed
                ? "identity split is still required for non-masslessness photon/gluon claims"
                : "P174 proves only a protected massless subspace diagnostic, not a photon/gluon identity split",
            masslessSectorInvariantPredictionAllowed
                ? "P183 zero-sector invariant supplies the contracted masslessness observable without target fitting"
                : "P175 finds the current mode signatures expose a 3-component gauge basis, not U(1) plus color-octet known-boson identity sectors",
            masslessSectorInvariantPredictionAllowed
                ? "P176/P178/P179 identity and transport blockers remain recorded for future identity claims"
                : "P176 finds no stable emergent Cartan-like U(1) line across sibling backgrounds",
        },
        NextWork: masslessSectorInvariantPredictionAllowed
            ? "rerun the validated boson prediction generator; continue U(1)/color identity work only for stronger photon/gluon claims"
            : "materialize U(1) and color-sector identity/benchmark contracts, or prove a branch-stable protected massless subspace before photon/gluon prediction"),
};

var promotableRoutes = routeAssessments.Where(route => route.PromotionAllowed).ToArray();
var allowedAttempts = routeAssessments.Where(route => route.PredictionAttemptAllowed).ToArray();
var bestNextRoute = routeAssessments
    .OrderBy(route => route.PromotionAllowed ? 0 : route.PredictionAttemptAllowed ? 1 : 2)
    .ThenBy(route => route.RouteId == "massless-gauge-photon-gluon" && route.DiagnosticObservablePresent ? 0 : 1)
    .ThenBy(route => route.RouteId, StringComparer.Ordinal)
    .First();

string terminalStatus = promotableRoutes.Length > 0
    ? "next-prediction-route-promotable"
    : allowedAttempts.Length > 0
        ? "next-prediction-route-attempt-ready"
        : "next-prediction-route-no-defensible-attempt";

var result = new
{
    phaseId = "phase173-next-prediction-route-selection",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    masslessTolerance,
    gaugeLeakTolerance,
    branchStabilityThreshold,
    bestNextRoute,
    routeAssessments,
    diagnosticMasslessGaugeObservation = routeAssessments.Single(route => route.RouteId == "massless-gauge-photon-gluon").DiagnosticPrediction,
    predictionAttemptMade = false,
    predictionAttemptMadeReason = "No route has the target-independent identity/source/stability evidence required for a promoted prediction attempt.",
    nextWork = bestNextRoute.NextWork,
    sourceEvidence = new
    {
        phase51Path = Phase51Path,
        phase117Path = Phase117Path,
        phase172Path = Phase172Path,
        phase182Path = File.Exists(Phase182Path) ? Phase182Path : null,
        phase174Path = File.Exists(Phase174Path) ? Phase174Path : null,
        phase175Path = File.Exists(Phase175Path) ? Phase175Path : null,
        phase176Path = File.Exists(Phase176Path) ? Phase176Path : null,
        phase177Path = File.Exists(Phase177Path) ? Phase177Path : null,
        phase178Path = File.Exists(Phase178Path) ? Phase178Path : null,
        phase179Path = File.Exists(Phase179Path) ? Phase179Path : null,
        phase183Path = File.Exists(Phase183Path) ? Phase183Path : null,
        registryPath = RegistryPath,
        atlasPath = AtlasPath,
        targetsPath = TargetsPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "next_prediction_route_selection.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "next_prediction_route_selection_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.bestNextRoute,
        result.diagnosticMasslessGaugeObservation,
        result.predictionAttemptMade,
        result.predictionAttemptMadeReason,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"bestNextRoute={bestNextRoute.RouteId}");
Console.WriteLine($"predictionAttemptMade={result.predictionAttemptMade}");
Console.WriteLine($"masslessDiagnosticPresent={routeAssessments.Single(route => route.RouteId == "massless-gauge-photon-gluon").DiagnosticObservablePresent}");
Console.WriteLine($"promotableRouteCount={promotableRoutes.Length}");

static double[] NumberArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Number)
            .Select(item => item.GetDouble())
            .ToArray()
        : Array.Empty<double>();
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .ToArray()
        : Array.Empty<string>();
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record BosonCandidate(
    string CandidateId,
    string ClaimClass,
    double BranchStabilityScore,
    double RefinementStabilityScore,
    double[] MassLikeEnvelope,
    double[] GaugeLeakEnvelope);
sealed record RouteAssessment(
    string RouteId,
    IReadOnlyList<string> ParticleIds,
    bool DiagnosticObservablePresent,
    bool TargetContractPresent,
    bool IdentityEvidencePresent,
    bool SourceEvidencePresent,
    bool StabilityEvidencePresent,
    bool PredictionAttemptAllowed,
    bool PromotionAllowed,
    object? DiagnosticPrediction,
    IReadOnlyList<string> Blockers,
    string NextWork);

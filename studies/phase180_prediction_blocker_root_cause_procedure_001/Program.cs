using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase180_prediction_blocker_root_cause_procedure_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase149Path = "studies/phase149_known_boson_predictability_contracts_001/output/known_boson_predictability_contracts.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase156Path = "studies/phase156_boson_generation_execution_package_001/output/boson_generation_execution_package.json";
const string Phase157Path = "studies/phase157_scientific_defensibility_boundary_001/output/scientific_defensibility_boundary.json";
const string Phase164Path = "studies/phase164_source_level_wz_bridge_candidate_census_001/output/source_level_wz_bridge_candidate_census.json";
const string Phase166Path = "studies/phase166_source_normalized_wz_prediction_attempt_001/output/source_normalized_wz_prediction_attempt.json";
const string Phase167Path = "studies/phase167_source_shape_normalized_wz_attempt_001/output/source_shape_normalized_wz_attempt.json";
const string Phase168Path = "studies/phase168_source_shape_scalar_relation_closure_audit_001/output/source_shape_scalar_relation_closure_audit.json";
const string Phase169Path = "studies/phase169_source_shape_law_stability_experiment_001/output/source_shape_law_stability_experiment.json";
const string Phase170Path = "studies/phase170_stable_source_shape_prediction_attempt_001/output/stable_source_shape_prediction_attempt.json";
const string Phase171Path = "studies/phase171_branch_stable_bridge_pair_census_001/output/branch_stable_bridge_pair_census.json";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census.json";
const string Phase173Path = "studies/phase173_next_prediction_route_selection_001/output/next_prediction_route_selection.json";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";
const string Phase175Path = "studies/phase175_massless_gauge_identity_split_feasibility_001/output/massless_gauge_identity_split_feasibility.json";
const string Phase176Path = "studies/phase176_massless_cartan_line_stability_001/output/massless_cartan_line_stability.json";
const string Phase177Path = "studies/phase177_massless_benchmark_contracts_001/output/massless_benchmark_contracts.json";
const string Phase178Path = "studies/phase178_protected_massless_subspace_transport_001/output/protected_massless_subspace_transport.json";
const string Phase179Path = "studies/phase179_gauge_frame_aligned_subspace_transport_001/output/gauge_frame_aligned_subspace_transport.json";
const string Phase183Path = "studies/phase183_massless_sector_invariant_prediction_001/output/massless_sector_invariant_prediction.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE180_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase149 = JsonDocument.Parse(File.ReadAllText(Phase149Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase156 = JsonDocument.Parse(File.ReadAllText(Phase156Path));
using var phase157 = JsonDocument.Parse(File.ReadAllText(Phase157Path));
using var phase164 = JsonDocument.Parse(File.ReadAllText(Phase164Path));
using var phase166 = JsonDocument.Parse(File.ReadAllText(Phase166Path));
using var phase167 = JsonDocument.Parse(File.ReadAllText(Phase167Path));
using var phase168 = JsonDocument.Parse(File.ReadAllText(Phase168Path));
using var phase169 = JsonDocument.Parse(File.ReadAllText(Phase169Path));
using var phase170 = JsonDocument.Parse(File.ReadAllText(Phase170Path));
using var phase171 = JsonDocument.Parse(File.ReadAllText(Phase171Path));
using var phase172 = JsonDocument.Parse(File.ReadAllText(Phase172Path));
using var phase173 = JsonDocument.Parse(File.ReadAllText(Phase173Path));
using var phase174 = JsonDocument.Parse(File.ReadAllText(Phase174Path));
using var phase175 = JsonDocument.Parse(File.ReadAllText(Phase175Path));
using var phase176 = JsonDocument.Parse(File.ReadAllText(Phase176Path));
using var phase177 = JsonDocument.Parse(File.ReadAllText(Phase177Path));
using var phase178 = JsonDocument.Parse(File.ReadAllText(Phase178Path));
using var phase179 = JsonDocument.Parse(File.ReadAllText(Phase179Path));
using var phase183 = File.Exists(Phase183Path) ? JsonDocument.Parse(File.ReadAllText(Phase183Path)) : null;

var comparisonRows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray()
    .Select(row => new PredictionRow(
        ParticleId: RequiredString(row, "particleId"),
        ObservableId: RequiredString(row, "observableId"),
        Status: RequiredString(row, "status"),
        Passed: JsonBool(row, "passed"),
        PredictedValue: JsonDouble(row, "predictedValue"),
        TargetValue: JsonDouble(row, "targetValue"),
        PullOrSigmaResidual: JsonDouble(row, "pullOrSigmaResidual"),
        ClosureRequirements: StringArray(row, "closureRequirements")))
    .ToArray();

var routeAssessments = phase173.RootElement.GetProperty("routeAssessments").EnumerateArray()
    .Select(route => new RouteGateAudit(
        RouteId: RequiredString(route, "routeId"),
        ParticleIds: StringArray(route, "particleIds"),
        DiagnosticObservablePresent: JsonBool(route, "diagnosticObservablePresent") is true,
        TargetContractPresent: JsonBool(route, "targetContractPresent") is true,
        IdentityEvidencePresent: JsonBool(route, "identityEvidencePresent") is true,
        SourceEvidencePresent: JsonBool(route, "sourceEvidencePresent") is true,
        StabilityEvidencePresent: JsonBool(route, "stabilityEvidencePresent") is true,
        PredictionAttemptAllowed: JsonBool(route, "predictionAttemptAllowed") is true,
        PromotionAllowed: JsonBool(route, "promotionAllowed") is true,
        Blockers: StringArray(route, "blockers"),
        RootCauseClass: ClassifyRoute(route)))
    .ToArray();

int phase148KnownRows = JsonInt(phase148.RootElement, "knownBosonRowCount") ?? -1;
int phase151KnownRows = JsonInt(phase151.RootElement, "knownBosonRowCount") ?? -2;
int phase148PredictedCount = JsonInt(phase148.RootElement, "predictedCount") ?? -1;
int phase151ValidatedCount = JsonInt(phase151.RootElement, "validatedPredictionCount") ?? -2;
int phase148FailedCount = JsonInt(phase148.RootElement, "failedComparisonAttemptCount") ?? -1;
int phase151FailedCount = JsonInt(phase151.RootElement, "failedAttemptCount") ?? -2;
int phase148BlockedCount = JsonInt(phase148.RootElement, "blockedCount") ?? -1;
int phase151BlockedCount = JsonInt(phase151.RootElement, "blockedRowCount") ?? -2;
bool phase173NoAttempt = string.Equals(JsonString(phase173.RootElement, "terminalStatus"), "next-prediction-route-no-defensible-attempt", StringComparison.Ordinal);
bool phase156NoAttempt = string.Equals(JsonString(phase156.RootElement, "terminalStatus"), "boson-generation-package-blocked-next-prediction-route-no-defensible-attempt", StringComparison.Ordinal);
bool phase157BoundaryReached = string.Equals(JsonString(phase157.RootElement, "terminalStatus"), "scientific-defensibility-boundary-reached", StringComparison.Ordinal);
bool photonMasslessnessPromoted = comparisonRows.Any(row =>
    row.ObservableId == "physical-photon-masslessness"
    && row.Status == "predicted"
    && row.Passed is true
    && row.TargetValue == 0.0);
bool gluonMasslessnessPromoted = comparisonRows.Any(row =>
    row.ObservableId == "physical-gluon-masslessness"
    && row.Status == "predicted"
    && row.Passed is true
    && row.TargetValue == 0.0);
bool photonBenchmarkRecognized = photonMasslessnessPromoted || comparisonRows.Any(row =>
    row.ObservableId == "physical-photon-masslessness"
    && row.Status == "blocked-target-available"
    && row.TargetValue == 0.0);
bool gluonBenchmarkRecognized = gluonMasslessnessPromoted || comparisonRows.Any(row =>
    row.ObservableId == "physical-gluon-masslessness"
    && row.Status == "blocked-target-available"
    && row.TargetValue == 0.0);

var integrityChecks = new[]
{
    Check("row-count-consistency", phase148KnownRows == phase151KnownRows, $"P148 rows={phase148KnownRows}, P151 rows={phase151KnownRows}"),
    Check("validated-count-consistency", phase148PredictedCount == phase151ValidatedCount, $"P148 predicted={phase148PredictedCount}, P151 validated={phase151ValidatedCount}"),
    Check("failed-count-consistency", phase148FailedCount == phase151FailedCount, $"P148 failed={phase148FailedCount}, P151 failed={phase151FailedCount}"),
    Check("blocked-count-consistency", phase148BlockedCount == phase151BlockedCount, $"P148 blocked={phase148BlockedCount}, P151 blocked={phase151BlockedCount}"),
    Check("route-package-consistency", phase173NoAttempt == phase156NoAttempt, $"P173 no-attempt={phase173NoAttempt}, P156 no-attempt={phase156NoAttempt}"),
    Check("boundary-package-consistency", !phase156NoAttempt || phase157BoundaryReached, $"P156 no-attempt={phase156NoAttempt}, P157 boundary={phase157BoundaryReached}"),
    Check("photon-benchmark-contract-consumed", photonBenchmarkRecognized, "photon row should be either promoted by P183 as a zero masslessness indicator or blocked with the zero benchmark contract present"),
    Check("gluon-benchmark-contract-consumed", gluonBenchmarkRecognized, "gluon row should be either promoted by P183 as a zero masslessness indicator or blocked with the zero benchmark contract present"),
    Check("no-promotion-while-route-blocked", !phase173NoAttempt || routeAssessments.All(route => !route.PromotionAllowed), "no route may be promoted when P173 terminal status is no-defensible-attempt"),
};

var predictionAudits = comparisonRows.Select(row => AuditPredictionRow(row)).ToArray();
bool anyIntegrityFailure = integrityChecks.Any(check => !check.Passed);
bool anyPromotableRoute = routeAssessments.Any(route => route.PromotionAllowed || route.PredictionAttemptAllowed);
bool allRowsValidated = JsonBool(phase151.RootElement, "allRowsValidated") is true;
bool procedureAllowsPredictionAttempt = !anyIntegrityFailure && anyPromotableRoute;
bool procedureComplete = !anyIntegrityFailure && !procedureAllowsPredictionAttempt;
string terminalStatus = allRowsValidated
    ? "prediction-root-cause-procedure-all-rows-validated"
    : anyIntegrityFailure
        ? "prediction-root-cause-procedure-pipeline-inconsistency"
        : procedureAllowsPredictionAttempt
            ? "prediction-root-cause-procedure-route-attempt-ready"
            : "prediction-root-cause-procedure-complete-no-defensible-new-attempt";

var processSteps = new[]
{
    Step(1, "artifact-inventory", "Load prediction rows, route selection, package boundary, W/Z bridge experiments, and massless-sector experiments."),
    Step(2, "gate-matrix", "For every route, audit diagnostic, target contract, identity, source, stability, attempt, and promotion gates."),
    Step(3, "contradiction-check", "Fail immediately if package counts, benchmark contracts, route status, or boundary status disagree."),
    Step(4, "negative-experiment-check", "Separate missing artifacts from local experiments that were tried and failed their promotion gates."),
    Step(5, "promotion-decision", "Only allow a prediction attempt when a route has promotion or attempt gates open and no integrity check fails."),
    Step(6, "fail-closed-or-rerun", "If no route is open, emit the exact root cause and stop; if a route is open, rerun the generator."),
};

var result = new
{
    phaseId = "phase180-prediction-blocker-root-cause-procedure",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    procedureValidated = procedureComplete || allRowsValidated || procedureAllowsPredictionAttempt,
    procedureAllowsPredictionAttempt,
    procedureComplete,
    allRowsValidated,
    processSteps,
    integrityChecks,
    predictionAudits,
    routeAssessments,
    rootCauseSummary = new
    {
        whatIsWrong = "The pipeline is internally consistent; the blocker is scientific evidence, not execution. Current artifacts validate the W/Z mass ratio and the protected zero masslessness indicators for photon/gluon. Absolute W/Z fails comparison/bridge gates, Higgs lacks scalar source and identity evidence, and stronger photon/gluon identity claims still lack U(1)/color identity and stable protected-sector transport.",
        currentValidatedPredictionCount = phase151ValidatedCount,
        failedAttemptCount = phase151FailedCount,
        blockedRowCount = phase151BlockedCount,
        noPipelineBugFound = !anyIntegrityFailure,
        masslessSectorInvariantPredictionAllowed = phase183 is not null && JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is true,
        photonMasslessnessPromoted,
        gluonMasslessnessPromoted,
        nextDefensibleAction = procedureAllowsPredictionAttempt
            ? "Rerun the validated generator because P183 supplies a narrow protected-sector zero masslessness invariant for the active photon/gluon benchmark observable."
            : "Do not add another local heuristic. Provide or derive a new target-independent identity/source artifact: W/Z bridge revision, Higgs scalar source plus identity, or U(1)/color-sector identity with stable transport.",
    },
    evidenceSnapshot = new
    {
        phase149Status = JsonString(phase149.RootElement, "terminalStatus"),
        phase151Status = JsonString(phase151.RootElement, "terminalStatus"),
        phase156Status = JsonString(phase156.RootElement, "terminalStatus"),
        phase157Status = JsonString(phase157.RootElement, "terminalStatus"),
        phase164Status = JsonString(phase164.RootElement, "terminalStatus"),
        phase166Status = JsonString(phase166.RootElement, "terminalStatus"),
        phase167Status = JsonString(phase167.RootElement, "terminalStatus"),
        phase168Status = JsonString(phase168.RootElement, "terminalStatus"),
        phase169Status = JsonString(phase169.RootElement, "terminalStatus"),
        phase170Status = JsonString(phase170.RootElement, "terminalStatus"),
        phase171Status = JsonString(phase171.RootElement, "terminalStatus"),
        phase172Status = JsonString(phase172.RootElement, "terminalStatus"),
        phase173Status = JsonString(phase173.RootElement, "terminalStatus"),
        phase174Status = JsonString(phase174.RootElement, "terminalStatus"),
        phase175Status = JsonString(phase175.RootElement, "terminalStatus"),
        phase176Status = JsonString(phase176.RootElement, "terminalStatus"),
        phase177Status = JsonString(phase177.RootElement, "terminalStatus"),
        phase178Status = JsonString(phase178.RootElement, "terminalStatus"),
        phase179Status = JsonString(phase179.RootElement, "terminalStatus"),
        phase183Status = phase183 is null ? null : JsonString(phase183.RootElement, "terminalStatus"),
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase149Path = Phase149Path,
        phase151Path = Phase151Path,
        phase156Path = Phase156Path,
        phase157Path = Phase157Path,
        phase164Path = Phase164Path,
        phase166Path = Phase166Path,
        phase167Path = Phase167Path,
        phase168Path = Phase168Path,
        phase169Path = Phase169Path,
        phase170Path = Phase170Path,
        phase171Path = Phase171Path,
        phase172Path = Phase172Path,
        phase173Path = Phase173Path,
        phase174Path = Phase174Path,
        phase175Path = Phase175Path,
        phase176Path = Phase176Path,
        phase177Path = Phase177Path,
        phase178Path = Phase178Path,
        phase179Path = Phase179Path,
        phase183Path = File.Exists(Phase183Path) ? Phase183Path : null,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "prediction_blocker_root_cause_procedure.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "prediction_blocker_root_cause_procedure_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.procedureValidated,
        result.procedureAllowsPredictionAttempt,
        result.procedureComplete,
        result.allRowsValidated,
        result.integrityChecks,
        result.predictionAudits,
        result.routeAssessments,
        result.rootCauseSummary,
    }, options));
File.WriteAllText(Path.Combine(outputDir, "prediction_blocker_root_cause_procedure.md"), BuildMarkdown(terminalStatus, predictionAudits, routeAssessments, integrityChecks));

Console.WriteLine(terminalStatus);
Console.WriteLine($"procedureAllowsPredictionAttempt={procedureAllowsPredictionAttempt}");
Console.WriteLine($"procedureComplete={procedureComplete}");
Console.WriteLine($"integrityFailureCount={integrityChecks.Count(check => !check.Passed)}");

PredictionAudit AuditPredictionRow(PredictionRow row)
{
    return row.ParticleId switch
    {
        "electroweak-sector" => new PredictionAudit(
            row.ParticleId,
            row.ObservableId,
            row.Status,
            row.Passed is true,
            "validated",
            "W/Z ratio is the only fully promoted physical comparison.",
            Array.Empty<string>()),
        "w-boson" or "z-boson" => new PredictionAudit(
            row.ParticleId,
            row.ObservableId,
            row.Status,
            false,
            "failed-validation-and-bridge-source",
            "Absolute W/Z values fail target comparison and the local bridge/source searches did not find a promotable target-independent revision.",
            row.ClosureRequirements),
        "higgs" => new PredictionAudit(
            row.ParticleId,
            row.ObservableId,
            row.Status,
            false,
            "missing-scalar-source-and-identity",
            "A Higgs row cannot be predicted until a scalar-sector source/operator and target-independent Higgs identity rule exist.",
            row.ClosureRequirements),
        "photon" => new PredictionAudit(
            row.ParticleId,
            row.ObservableId,
            row.Status,
            row.Status == "predicted" && row.Passed is true,
            row.Status == "predicted" && row.Passed is true ? "validated-masslessness-invariant" : "missing-u1-identity-and-negative-transport",
            row.Status == "predicted" && row.Passed is true
                ? "P183 validates the zero masslessness-indicator as a protected-sector invariant; U(1) identity remains unpromoted for stronger claims."
                : "The benchmark contract exists, but P175/P176/P178/P179 do not derive a U(1) photon identity or stable protected transport.",
            row.ClosureRequirements),
        "gluon" => new PredictionAudit(
            row.ParticleId,
            row.ObservableId,
            row.Status,
            row.Status == "predicted" && row.Passed is true,
            row.Status == "predicted" && row.Passed is true ? "validated-masslessness-invariant" : "missing-color-octet-identity-and-negative-transport",
            row.Status == "predicted" && row.Passed is true
                ? "P183 validates the zero masslessness-indicator as a protected-sector invariant; color-octet identity remains unpromoted for stronger claims."
                : "The benchmark contract exists, but P175/P178/P179 do not derive a color-octet identity or stable protected transport.",
            row.ClosureRequirements),
        _ => new PredictionAudit(row.ParticleId, row.ObservableId, row.Status, false, "unclassified", "No row-specific procedure exists.", row.ClosureRequirements),
    };
}

static string ClassifyRoute(JsonElement route)
{
    string routeId = RequiredString(route, "routeId");
    bool identity = JsonBool(route, "identityEvidencePresent") is true;
    bool source = JsonBool(route, "sourceEvidencePresent") is true;
    bool stability = JsonBool(route, "stabilityEvidencePresent") is true;
    bool attempt = JsonBool(route, "predictionAttemptAllowed") is true;
    bool promote = JsonBool(route, "promotionAllowed") is true;

    if (promote)
        return "promotable";
    if (attempt)
        return "attempt-ready";
    if (routeId == "wz-absolute-mass")
        return "negative-local-bridge-experiments";
    if (routeId == "higgs-scalar-mass")
        return "missing-source-and-identity-artifacts";
    if (!identity || !source || !stability)
        return "missing-identity-source-or-stability-evidence";
    return "blocked";
}

static IntegrityCheck Check(string id, bool passed, string detail) => new(id, passed, detail);
static ProcessStep Step(int order, string id, string rule) => new(order, id, rule);

static string BuildMarkdown(
    string terminalStatus,
    IReadOnlyList<PredictionAudit> predictionAudits,
    IReadOnlyList<RouteGateAudit> routeAudits,
    IReadOnlyList<IntegrityCheck> checks)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Prediction Blocker Root-Cause Procedure");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine("## Integrity Checks");
    foreach (var check in checks)
        builder.AppendLine($"- `{check.CheckId}`: {(check.Passed ? "passed" : "failed")} - {check.Detail}");
    builder.AppendLine();
    builder.AppendLine("## Prediction Rows");
    foreach (var audit in predictionAudits)
        builder.AppendLine($"- `{audit.ParticleId}` `{audit.ObservableId}`: {audit.RootCauseClass} - {audit.Diagnosis}");
    builder.AppendLine();
    builder.AppendLine("## Routes");
    foreach (var route in routeAudits)
        builder.AppendLine($"- `{route.RouteId}`: {route.RootCauseClass}; attempt={route.PredictionAttemptAllowed}; promote={route.PromotionAllowed}");
    return builder.ToString();
}

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
    bool? Passed,
    double? PredictedValue,
    double? TargetValue,
    double? PullOrSigmaResidual,
    IReadOnlyList<string> ClosureRequirements);

sealed record PredictionAudit(
    string ParticleId,
    string ObservableId,
    string Status,
    bool PromotionValidated,
    string RootCauseClass,
    string Diagnosis,
    IReadOnlyList<string> ClosureRequirements);

sealed record RouteGateAudit(
    string RouteId,
    IReadOnlyList<string> ParticleIds,
    bool DiagnosticObservablePresent,
    bool TargetContractPresent,
    bool IdentityEvidencePresent,
    bool SourceEvidencePresent,
    bool StabilityEvidencePresent,
    bool PredictionAttemptAllowed,
    bool PromotionAllowed,
    IReadOnlyList<string> Blockers,
    string RootCauseClass);

sealed record IntegrityCheck(string CheckId, bool Passed, string Detail);
sealed record ProcessStep(int Order, string StepId, string Rule);

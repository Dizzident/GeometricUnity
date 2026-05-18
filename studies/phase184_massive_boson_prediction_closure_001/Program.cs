using System.Text.Json;

const string DefaultOutputDir = "studies/phase184_massive_boson_prediction_closure_001/output";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase173Path = "studies/phase173_next_prediction_route_selection_001/output/next_prediction_route_selection.json";
const string Phase74Path = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase112Path = "studies/phase112_scalar_sector_relation_revision_attempt_001/output/scalar_sector_relation_revision_attempt.json";
const string Phase168Path = "studies/phase168_source_shape_scalar_relation_closure_audit_001/output/source_shape_scalar_relation_closure_audit.json";
const string Phase182Path = "studies/phase182_wz_operator_algebra_bridge_source_001/output/wz_operator_algebra_bridge_source.json";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase72Path = "studies/phase72_wz_absolute_scale_calibration_001/wz_absolute_scale_calibration.json";
const string Phase185Path = "studies/phase185_wz_operator_unit_scale_materialization_001/output/wz_operator_unit_scale_materialization.json";
const string Phase186Path = "studies/phase186_wz_source_shape_law_closure_001/output/wz_source_shape_law_closure.json";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold.json";
const string Phase188Path = "studies/phase188_wz_direct_geometric_bridge_source_readiness_001/output/wz_direct_geometric_bridge_source_readiness.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE184_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase173 = JsonDocument.Parse(File.ReadAllText(Phase173Path));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74Path));
using var phase112 = JsonDocument.Parse(File.ReadAllText(Phase112Path));
using var phase168 = JsonDocument.Parse(File.ReadAllText(Phase168Path));
using var phase182 = JsonDocument.Parse(File.ReadAllText(Phase182Path));
using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase72 = JsonDocument.Parse(File.ReadAllText(Phase72Path));
using var phase185 = File.Exists(Phase185Path) ? JsonDocument.Parse(File.ReadAllText(Phase185Path)) : null;
using var phase186 = File.Exists(Phase186Path) ? JsonDocument.Parse(File.ReadAllText(Phase186Path)) : null;
using var phase187 = File.Exists(Phase187Path) ? JsonDocument.Parse(File.ReadAllText(Phase187Path)) : null;
using var phase188 = File.Exists(Phase188Path) ? JsonDocument.Parse(File.ReadAllText(Phase188Path)) : null;
using var phase189 = File.Exists(Phase189Path) ? JsonDocument.Parse(File.ReadAllText(Phase189Path)) : null;

var rows = phase151.RootElement.GetProperty("failedAttempts").EnumerateArray()
    .Concat(phase151.RootElement.GetProperty("blockedRows").EnumerateArray())
    .Select(row => new MassiveRow(
        RequiredString(row, "particleId"),
        RequiredString(row, "observableId"),
        RequiredString(row, "status"),
        JsonDouble(row, "predictedValue"),
        JsonDouble(row, "predictedUncertainty"),
        JsonDouble(row, "targetValue"),
        JsonDouble(row, "targetUncertainty"),
        JsonString(row, "unit"),
        JsonDouble(row, "pullOrSigmaResidual"),
        JsonBool(row, "passed"),
        JsonBool(row, "promotionAllowed") is true,
        StringArray(row, "closureRequirements")))
    .Where(row => row.ParticleId is "w-boson" or "z-boson" or "higgs")
    .OrderBy(row => row.ParticleId, StringComparer.Ordinal)
    .ToArray();

var routeAssessments = phase173.RootElement.GetProperty("routeAssessments").EnumerateArray()
    .ToDictionary(route => RequiredString(route, "routeId"), route => route.Clone(), StringComparer.Ordinal);

bool wzBridgeRevisionValidated = JsonBool(routeAssessments["wz-absolute-mass"], "sourceEvidencePresent") is true
    && JsonBool(routeAssessments["wz-absolute-mass"], "stabilityEvidencePresent") is true
    && (JsonBool(routeAssessments["wz-absolute-mass"], "predictionAttemptAllowed") is true
        || JsonBool(routeAssessments["wz-absolute-mass"], "promotionAllowed") is true);
bool higgsScalarSourceValidated = JsonBool(routeAssessments["higgs-scalar-mass"], "diagnosticObservablePresent") is true
    && JsonBool(routeAssessments["higgs-scalar-mass"], "identityEvidencePresent") is true
    && JsonBool(routeAssessments["higgs-scalar-mass"], "stabilityEvidencePresent") is true
    && (JsonBool(routeAssessments["higgs-scalar-mass"], "predictionAttemptAllowed") is true
        || JsonBool(routeAssessments["higgs-scalar-mass"], "promotionAllowed") is true);
bool p72ScaleValidated = string.Equals(JsonString(phase72.RootElement, "status"), "validated", StringComparison.Ordinal);
bool scalarBridgeDerived = string.Equals(JsonString(phase70.RootElement, "terminalStatus"), "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal);
bool scalarRevisionIndependent = JsonBool(phase112.RootElement, "independentRevisionEvidencePresent") is true
    && JsonBool(phase112.RootElement, "repairAccepted") is true;
bool targetImpliedScalarRejected = JsonBool(phase112.RootElement.GetProperty("diagnosticOnlyTargetImpliedRevision"), "mayBeUsedAsCalibration") is false
    && JsonBool(phase168.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "targetIndependent") is false;
int p182StableRawGatePassingCandidateCount = JsonInt(phase182.RootElement, "stableRawGatePassingCandidateCount") ?? 0;
double? p182BestRawRatio = phase182.RootElement.TryGetProperty("bestCandidate", out var bestCandidate)
    ? JsonDouble(bestCandidate, "minRawToTargetRatio")
    : null;
bool p185UnitScaleMaterialized = phase185 is not null && JsonBool(phase185.RootElement, "unitScaleArtifactMaterialized") is true;
bool p185PredictiveReplayPromotable = phase185 is not null
    && phase185.RootElement.TryGetProperty("predictiveReplay", out var p185Replay)
    && JsonBool(p185Replay, "predictiveReplayPromotable") is true;
bool p186SourceShapeLawPromotable = phase186 is not null
    && JsonBool(phase186.RootElement, "sourceShapeLawPromotable") is true;
bool p187ScaffoldMaterialized = phase187 is not null && JsonBool(phase187.RootElement, "scaffoldMaterialized") is true;
bool p187HiggsSourceIdentityValidated = phase187 is not null && JsonBool(phase187.RootElement, "higgsSourceIdentityValidated") is true;
bool p188DirectBridgeSourceValidated = phase188 is not null && JsonBool(phase188.RootElement, "directBridgeSourceValidated") is true;
bool p188ReplayAllowed = phase188 is not null && JsonBool(phase188.RootElement, "replayAllowed") is true;
bool p189CensusPromotable = phase189 is not null && JsonBool(phase189.RootElement, "censusPromotable") is true;
bool p189PredictionAttemptAllowed = phase189 is not null && JsonBool(phase189.RootElement, "predictionAttemptAllowed") is true;
bool wComparisonPassed = phase74.RootElement.GetProperty("comparisons").EnumerateArray()
    .Where(comparison => RequiredString(comparison, "observableId") == "physical-w-boson-mass-gev")
    .All(comparison => JsonBool(comparison, "passed") is true);
bool zComparisonPassed = phase74.RootElement.GetProperty("comparisons").EnumerateArray()
    .Where(comparison => RequiredString(comparison, "observableId") == "physical-z-boson-mass-gev")
    .All(comparison => JsonBool(comparison, "passed") is true);

var checks = new[]
{
    new GateCheck("wz-target-independent-scale", p72ScaleValidated, "P72 external electroweak scale calibration is validated and disjoint from W/Z mass targets."),
    new GateCheck("wz-bridge-revision", wzBridgeRevisionValidated, "W/Z absolute masses require a source and stability validated bridge revision; P182 found no stable raw-gate passer."),
    new GateCheck("wz-operator-unit-scale", p185UnitScaleMaterialized, "P185 materializes the target-independent dimensional unit-scale lead from P118."),
    new GateCheck("wz-operator-unit-scale-replay", p185PredictiveReplayPromotable, "P185/P166/P170 downstream replay must pass raw, common-scale, and target-comparison gates before W/Z promotion."),
    new GateCheck("wz-source-shape-law-closure", p186SourceShapeLawPromotable, "P186 requires a derivation-backed stable source-shape law passing common-scale and target-comparison gates."),
    new GateCheck("wz-direct-geometric-bridge-source", p188DirectBridgeSourceValidated, "P188 requires an explicit target-independent geometric W/Z bridge-source law outside exhausted families."),
    new GateCheck("wz-direct-geometric-bridge-replay", p188ReplayAllowed, "P188 must allow replay before another W/Z promotion attempt is defensible."),
    new GateCheck("wz-target-comparison", wComparisonPassed && zComparisonPassed, "Existing W/Z absolute comparison remains outside promotion tolerance."),
    new GateCheck("scalar-revision-independent", scalarRevisionIndependent, "P112/P168 reject target-implied scalar factors as diagnostic-only."),
    new GateCheck("higgs-scalar-bridge", scalarBridgeDerived, "P70 derives only the electroweak VEV bridge, not a Higgs scalar excitation/source identity."),
    new GateCheck("higgs-source-identity-scaffold", p187ScaffoldMaterialized, "P187 materializes the fail-closed Higgs source/identity scaffold."),
    new GateCheck("higgs-source-identity-scaffold-validated", p187HiggsSourceIdentityValidated, "P187 requires scalar source/operator, identity features, and stability sidecars before Higgs promotion."),
    new GateCheck("higgs-scalar-source-operator-census", p189CensusPromotable, "P189 requires a solved scalar source/operator census with target-independent identity features and stability sidecars."),
    new GateCheck("higgs-scalar-source-operator-prediction-attempt", p189PredictionAttemptAllowed, "P189/P187 must allow a Higgs prediction attempt before physical mass comparison."),
    new GateCheck("higgs-source-identity-stability", higgsScalarSourceValidated, "P173 has no Higgs diagnostic source, identity, or stability evidence."),
};

bool anyMassivePredictionPromoted = rows.Any(row => row.PromotionAllowed && row.Passed is true);
bool anyNewMassiveAttemptAllowed = wzBridgeRevisionValidated || higgsScalarSourceValidated || p188ReplayAllowed || p189PredictionAttemptAllowed;
string terminalStatus = anyMassivePredictionPromoted
    ? "massive-boson-prediction-closure-promoted"
    : anyNewMassiveAttemptAllowed
        ? "massive-boson-prediction-closure-attempt-ready"
        : "massive-boson-prediction-closure-blocked-no-defensible-revision";

var result = new
{
    phaseId = "phase184-massive-boson-prediction-closure",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    anyMassivePredictionPromoted,
    anyNewMassiveAttemptAllowed,
    rows,
    checks,
    diagnostics = new
    {
        p72ScaleValidated,
        scalarBridgeDerived,
        scalarRevisionIndependent,
        targetImpliedScalarRejected,
        p182StableRawGatePassingCandidateCount,
        p182BestRawRatio,
        p185UnitScaleMaterialized,
        p185PredictiveReplayPromotable,
        p186SourceShapeLawPromotable,
        p187ScaffoldMaterialized,
        p187HiggsSourceIdentityValidated,
        p188DirectBridgeSourceValidated,
        p188ReplayAllowed,
        p189CensusPromotable,
        p189PredictionAttemptAllowed,
        wComparisonPassed,
        zComparisonPassed,
    },
    decision = anyNewMassiveAttemptAllowed
        ? "A massive-boson route has validated source/identity/stability evidence; rerun prediction comparison for that route."
        : "Do not promote W/Z/Higgs predictions. W/Z absolute masses need a target-independent bridge/source revision; Higgs needs a scalar excitation/source identity with stability evidence.",
    nextRequiredArtifact = "Either a W/Z bridge source outside the exhausted current variation/operator-algebra families, or a Higgs scalar-sector source/operator with target-independent identity and stability sidecars.",
    sourceEvidence = new
    {
        phase151Path = Phase151Path,
        phase173Path = Phase173Path,
        phase74Path = Phase74Path,
        phase112Path = Phase112Path,
        phase168Path = Phase168Path,
        phase182Path = Phase182Path,
        phase70Path = Phase70Path,
        phase72Path = Phase72Path,
        phase185Path = File.Exists(Phase185Path) ? Phase185Path : null,
        phase186Path = File.Exists(Phase186Path) ? Phase186Path : null,
        phase187Path = File.Exists(Phase187Path) ? Phase187Path : null,
        phase188Path = File.Exists(Phase188Path) ? Phase188Path : null,
        phase189Path = File.Exists(Phase189Path) ? Phase189Path : null,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "massive_boson_prediction_closure.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "massive_boson_prediction_closure_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.anyMassivePredictionPromoted,
        result.anyNewMassiveAttemptAllowed,
        result.rows,
        result.checks,
        result.diagnostics,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"anyMassivePredictionPromoted={anyMassivePredictionPromoted}");
Console.WriteLine($"anyNewMassiveAttemptAllowed={anyNewMassiveAttemptAllowed}");
Console.WriteLine($"p182StableRawGatePassingCandidateCount={p182StableRawGatePassingCandidateCount}");

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

sealed record MassiveRow(
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

sealed record GateCheck(string CheckId, bool Passed, string Detail);

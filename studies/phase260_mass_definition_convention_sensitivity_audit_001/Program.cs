using System.Text.Json;

const string DefaultOutputDir = "studies/phase260_mass_definition_convention_sensitivity_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase259Path = "studies/phase259_recent_target_value_sensitivity_audit_001/output/recent_target_value_sensitivity_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE260_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase259 = JsonDocument.Parse(File.ReadAllText(Phase259Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wRow = FindRow(rows, "w-boson");
var zRow = FindRow(rows, "z-boson");
var higgsRow = FindRow(rows, "higgs");

var wTarget = TargetFromRow(wRow);
var zTarget = TargetFromRow(zRow);
var higgsTarget = TargetFromRow(higgsRow);
var wPrediction = PredictionFromRow(wRow);
var zPrediction = PredictionFromRow(zRow);

var wWidthGeV = 2.085;
var zWidthGeV = 2.4955;
var higgsWidthGeV = 0.0037;

var wPoleConventionTargetGeV = BreitWignerMassDependentWidthToPoleMass(wTarget.Value, wWidthGeV);
var zPoleConventionTargetGeV = BreitWignerMassDependentWidthToPoleMass(zTarget.Value, zWidthGeV);
var higgsPoleConventionTargetGeV = BreitWignerMassDependentWidthToPoleMass(higgsTarget.Value, higgsWidthGeV);
var wConventionShiftGeV = wTarget.Value - wPoleConventionTargetGeV;
var zConventionShiftGeV = zTarget.Value - zPoleConventionTargetGeV;
var higgsConventionShiftGeV = higgsTarget.Value - higgsPoleConventionTargetGeV;

var wCurrentGapGeV = Math.Abs(wPrediction.Value - wTarget.Value);
var zCurrentGapGeV = Math.Abs(zPrediction.Value - zTarget.Value);
var wResidualAgainstPoleConvention = Residual(wPrediction.Value, wPrediction.Uncertainty, wPoleConventionTargetGeV, wTarget.Uncertainty);
var zResidualAgainstPoleConvention = Residual(zPrediction.Value, zPrediction.Uncertainty, zPoleConventionTargetGeV, zTarget.Uncertainty);
var wFailurePersistsUnderPoleConvention = wResidualAgainstPoleConvention > 5.0;
var zFailurePersistsUnderPoleConvention = zResidualAgainstPoleConvention > 5.0;
var higgsStillHasNoPrediction = JsonNull(higgsRow, "predictedValue");
var wConventionShiftFractionOfGap = wConventionShiftGeV / wCurrentGapGeV;
var zConventionShiftFractionOfGap = zConventionShiftGeV / zCurrentGapGeV;
var conventionShiftPromotesBosonMasses = false;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var phase259TargetValueSensitivityPassed = JsonBool(phase259.RootElement, "targetValueSensitivityAuditPassed") is true;
var phase259RecentTargetUpdatePromotesMasses = JsonBool(phase259.RootElement, "recentTargetUpdatePromotesBosonMasses") is true;

var massDefinitionConventionSensitivityAuditPassed = !conventionShiftPromotesBosonMasses
    && wFailurePersistsUnderPoleConvention
    && zFailurePersistsUnderPoleConvention
    && higgsStillHasNoPrediction
    && wConventionShiftFractionOfGap < 0.01
    && zConventionShiftFractionOfGap < 0.01
    && phase259TargetValueSensitivityPassed
    && !phase259RecentTargetUpdatePromotesMasses
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "wz-mass-definition-shifts-are-small-compared-to-current-gaps",
        wConventionShiftFractionOfGap < 0.01 && zConventionShiftFractionOfGap < 0.01,
        $"wConventionShiftGeV={wConventionShiftGeV:R}; zConventionShiftGeV={zConventionShiftGeV:R}; wCurrentGapGeV={wCurrentGapGeV:R}; zCurrentGapGeV={zCurrentGapGeV:R}; wConventionShiftFractionOfGap={wConventionShiftFractionOfGap:R}; zConventionShiftFractionOfGap={zConventionShiftFractionOfGap:R}"),
    new Check(
        "wz-failed-comparisons-persist-under-pole-convention",
        wFailurePersistsUnderPoleConvention && zFailurePersistsUnderPoleConvention,
        $"wResidualAgainstPoleConvention={wResidualAgainstPoleConvention:R}; zResidualAgainstPoleConvention={zResidualAgainstPoleConvention:R}; wFailurePersistsUnderPoleConvention={wFailurePersistsUnderPoleConvention}; zFailurePersistsUnderPoleConvention={zFailurePersistsUnderPoleConvention}"),
    new Check(
        "higgs-mass-definition-cannot-fill-missing-prediction",
        higgsStillHasNoPrediction && higgsConventionShiftGeV < 1e-6,
        $"higgsStillHasNoPrediction={higgsStillHasNoPrediction}; higgsConventionShiftGeV={higgsConventionShiftGeV:R}"),
    new Check(
        "mass-definition-convention-does-not-close-source-lineage",
        !conventionShiftPromotesBosonMasses && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"conventionShiftPromotesBosonMasses={conventionShiftPromotesBosonMasses}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "target-value-sensitivity-boundary-inherited",
        phase259TargetValueSensitivityPassed && !phase259RecentTargetUpdatePromotesMasses,
        $"phase259TargetValueSensitivityPassed={phase259TargetValueSensitivityPassed}; phase259RecentTargetUpdatePromotesMasses={phase259RecentTargetUpdatePromotesMasses}"),
};

var terminalStatus = massDefinitionConventionSensitivityAuditPassed
    ? "mass-definition-convention-sensitivity-audit-no-promotion-change"
    : "mass-definition-convention-sensitivity-audit-review-required";

var result = new
{
    phaseId = "phase260-mass-definition-convention-sensitivity-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    massDefinitionConventionSensitivityAuditPassed,
    conventionShiftPromotesBosonMasses,
    failedComparisonsPersistUnderPoleConvention = wFailurePersistsUnderPoleConvention && zFailurePersistsUnderPoleConvention && higgsStillHasNoPrediction,
    conventionInputs = new
    {
        convention = "mass-dependent-width Breit-Wigner to complex-pole approximation",
        formula = "M_pole = M_BW / sqrt(1 + (Gamma_BW / M_BW)^2)",
        wWidthGeV,
        zWidthGeV,
        higgsWidthGeV,
    },
    conventionShift = new
    {
        wTargetMassDependentWidthGeV = wTarget.Value,
        wPoleConventionTargetGeV,
        wConventionShiftGeV,
        zTargetMassDependentWidthGeV = zTarget.Value,
        zPoleConventionTargetGeV,
        zConventionShiftGeV,
        higgsTargetMassGeV = higgsTarget.Value,
        higgsPoleConventionTargetGeV,
        higgsConventionShiftGeV,
    },
    predictionSensitivity = new
    {
        wPrediction,
        zPrediction,
        wCurrentGapGeV,
        zCurrentGapGeV,
        wResidualAgainstPoleConvention,
        zResidualAgainstPoleConvention,
        wFailurePersistsUnderPoleConvention,
        zFailurePersistsUnderPoleConvention,
        higgsStillHasNoPrediction,
        wConventionShiftFractionOfGap,
        zConventionShiftFractionOfGap,
    },
    currentBlockerEvidence = new
    {
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase259 = new
        {
            phase259TargetValueSensitivityPassed,
            phase259RecentTargetUpdatePromotesMasses,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "pdg-2025-w-listing-mass-definition",
            "https://pdg.lbl.gov/2025/listings/rpp2025-list-w-boson.pdf",
            "PDG notes the listed W mass is the mass parameter in a Breit-Wigner distribution with mass-dependent width."),
        new ExternalSource(
            "pdg-2025-gauge-higgs-summary",
            "https://pdgweb.lbl.gov/2025/tables/rpp2025-sum-gauge-higgs-bosons.pdf",
            "PDG 2025 gauge/Higgs summary gives Z mass and width plus Higgs mass and width references."),
        new ExternalSource(
            "unstable-particle-pole-definition",
            "https://link.springer.com/article/10.1140/epjp/s13360-024-05301-0",
            "Recent review context: unstable-particle mass and width can be defined by the complex pole; convention shifts are separate from source-lineage prediction."),
    },
    checks,
    decision = massDefinitionConventionSensitivityAuditPassed
        ? "Do not treat pole/Breit-Wigner mass-definition conventions as a route to W/Z/H prediction completion. The convention shifts are tens of MeV for W/Z and negligible for Higgs, while current W/Z gaps are about 10-12 GeV and Higgs has no predicted row. Source-lineage blockers remain active."
        : "Review mass-definition convention sensitivity before relying on current physical mass comparison.",
    nextRequiredArtifact = new[]
    {
        "A W/Z absolute-scale source row whose physical-mass convention is declared before target comparison.",
        "A Higgs scalar-source row with a declared physical-mass convention before target comparison.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase259Path = Phase259Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "mass_definition_convention_sensitivity_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "mass_definition_convention_sensitivity_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.massDefinitionConventionSensitivityAuditPassed,
        result.conventionShiftPromotesBosonMasses,
        result.failedComparisonsPersistUnderPoleConvention,
        result.conventionInputs,
        result.conventionShift,
        result.predictionSensitivity,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"massDefinitionConventionSensitivityAuditPassed={massDefinitionConventionSensitivityAuditPassed}");
Console.WriteLine($"conventionShiftPromotesBosonMasses={conventionShiftPromotesBosonMasses}");
Console.WriteLine($"wResidualAgainstPoleConvention={wResidualAgainstPoleConvention}");

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double BreitWignerMassDependentWidthToPoleMass(double mass, double width) =>
    mass / Math.Sqrt(1.0 + (width / mass) * (width / mass));

static double Residual(double prediction, double predictionSigma, double target, double targetSigma) =>
    Math.Abs(prediction - target) / Math.Sqrt(predictionSigma * predictionSigma + targetSigma * targetSigma);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool JsonNull(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Null;

static Target TargetFromRow(JsonElement row) => new(
    JsonString(row, "particleId") ?? "unknown",
    JsonString(row, "observableId") ?? "unknown",
    row.GetProperty("targetValue").GetDouble(),
    row.GetProperty("targetUncertainty").GetDouble(),
    JsonString(row, "unit") ?? "unknown");

static Prediction PredictionFromRow(JsonElement row) => new(
    JsonString(row, "particleId") ?? "unknown",
    JsonString(row, "observableId") ?? "unknown",
    row.GetProperty("predictedValue").GetDouble(),
    row.GetProperty("predictedUncertainty").GetDouble(),
    JsonString(row, "unit") ?? "unknown");

sealed record Target(string ParticleId, string ObservableId, double Value, double Uncertainty, string Unit);
sealed record Prediction(string ParticleId, string ObservableId, double Value, double Uncertainty, string Unit);
sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExternalSource(string SourceId, string Url, string Finding);

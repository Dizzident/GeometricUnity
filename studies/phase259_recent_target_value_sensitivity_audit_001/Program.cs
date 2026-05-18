using System.Text.Json;

const string DefaultOutputDir = "studies/phase259_recent_target_value_sensitivity_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase258Path = "studies/phase258_recent_electroweak_relation_source_audit_001/output/recent_electroweak_relation_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE259_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase258 = JsonDocument.Parse(File.ReadAllText(Phase258Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wRow = FindRow(rows, "w-boson");
var zRow = FindRow(rows, "z-boson");
var higgsRow = FindRow(rows, "higgs");

var currentWTarget = TargetFromRow(wRow);
var currentZTarget = TargetFromRow(zRow);
var currentHiggsTarget = TargetFromRow(higgsRow);
var currentWPrediction = PredictionFromRow(wRow);
var currentZPrediction = PredictionFromRow(zRow);

var recentWTarget = new Target("cms-2026-nature-w-mass", "w-boson", "physical-w-boson-mass-gev", 80.3602, 0.0099, "GeV");
var currentBestZTarget = new Target("pdg-2025-z-mass", "z-boson", "physical-z-boson-mass-gev", 91.1876, 0.0021, "GeV");
var currentBestHiggsTarget = new Target("pdg-2025-higgs-mass", "higgs", "physical-higgs-mass-gev", 125.20, 0.11, "GeV");

var wTargetDriftSigma = SigmaDistance(currentWTarget.Value, currentWTarget.Uncertainty, recentWTarget.Value, recentWTarget.Uncertainty);
var zTargetDriftSigma = SigmaDistance(currentZTarget.Value, currentZTarget.Uncertainty, currentBestZTarget.Value, currentBestZTarget.Uncertainty);
var higgsTargetDriftSigma = SigmaDistance(currentHiggsTarget.Value, currentHiggsTarget.Uncertainty, currentBestHiggsTarget.Value, currentBestHiggsTarget.Uncertainty);

var currentWResidualAgainstRecentTarget = Residual(currentWPrediction.Value, currentWPrediction.Uncertainty, recentWTarget.Value, recentWTarget.Uncertainty);
var currentZResidualAgainstBestTarget = Residual(currentZPrediction.Value, currentZPrediction.Uncertainty, currentBestZTarget.Value, currentBestZTarget.Uncertainty);
var wFailurePersistsUnderRecentTarget = currentWResidualAgainstRecentTarget > 5.0;
var zFailurePersistsUnderBestTarget = currentZResidualAgainstBestTarget > 5.0;
var higgsStillHasNoPrediction = JsonNull(higgsRow, "predictedValue");

var empiricalRelationWithCurrentTargets = currentHiggsTarget.Value * currentZTarget.Value * currentZTarget.Value / (2.0 * currentWTarget.Value * currentWTarget.Value * currentWTarget.Value);
var empiricalRelationWithRecentWTarget = currentBestHiggsTarget.Value * currentBestZTarget.Value * currentBestZTarget.Value / (2.0 * recentWTarget.Value * recentWTarget.Value * recentWTarget.Value);
var empiricalRelationShift = empiricalRelationWithRecentWTarget - empiricalRelationWithCurrentTargets;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var phase258RelationPromotesMasses = JsonBool(phase258.RootElement, "recentElectroweakRelationPromotesBosonMasses") is true;
var phase258HypotheticalRemainingNullity = phase258.RootElement.TryGetProperty("rankEffect", out var p258RankEffect)
    ? JsonInt(p258RankEffect, "hypotheticalRemainingNullityIfAccepted")
    : null;

var recentTargetUpdatePromotesBosonMasses = false;
var currentTargetsConsistentWithRecentReferences = wTargetDriftSigma < 2.0
    && zTargetDriftSigma < 2.0
    && higgsTargetDriftSigma < 2.0;
var failedComparisonsPersistUnderRecentTargets = wFailurePersistsUnderRecentTarget
    && zFailurePersistsUnderBestTarget
    && higgsStillHasNoPrediction;
var targetValueSensitivityAuditPassed = currentTargetsConsistentWithRecentReferences
    && failedComparisonsPersistUnderRecentTargets
    && !recentTargetUpdatePromotesBosonMasses
    && !phase258RelationPromotesMasses
    && phase258HypotheticalRemainingNullity == 1
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "current-targets-are-consistent-with-recent-experimental-references",
        currentTargetsConsistentWithRecentReferences,
        $"wTargetDriftSigma={wTargetDriftSigma:R}; zTargetDriftSigma={zTargetDriftSigma:R}; higgsTargetDriftSigma={higgsTargetDriftSigma:R}"),
    new Check(
        "wz-failed-comparisons-persist-under-recent-targets",
        wFailurePersistsUnderRecentTarget && zFailurePersistsUnderBestTarget,
        $"currentWResidualAgainstRecentTarget={currentWResidualAgainstRecentTarget:R}; currentZResidualAgainstBestTarget={currentZResidualAgainstBestTarget:R}; wFailurePersistsUnderRecentTarget={wFailurePersistsUnderRecentTarget}; zFailurePersistsUnderBestTarget={zFailurePersistsUnderBestTarget}"),
    new Check(
        "higgs-remains-blocked-not-target-sensitive",
        higgsStillHasNoPrediction,
        $"higgsStillHasNoPrediction={higgsStillHasNoPrediction}; currentHiggsTarget={currentHiggsTarget.Value:R}; currentBestHiggsTarget={currentBestHiggsTarget.Value:R}"),
    new Check(
        "recent-targets-do-not-close-source-lineage",
        !recentTargetUpdatePromotesBosonMasses && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"recentTargetUpdatePromotesBosonMasses={recentTargetUpdatePromotesBosonMasses}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "target-refresh-does-not-rescue-empirical-relation-lead",
        !phase258RelationPromotesMasses && phase258HypotheticalRemainingNullity == 1,
        $"phase258RelationPromotesMasses={phase258RelationPromotesMasses}; phase258HypotheticalRemainingNullity={phase258HypotheticalRemainingNullity}; empiricalRelationWithCurrentTargets={empiricalRelationWithCurrentTargets:R}; empiricalRelationWithRecentWTarget={empiricalRelationWithRecentWTarget:R}; empiricalRelationShift={empiricalRelationShift:R}"),
};

var terminalStatus = targetValueSensitivityAuditPassed
    ? "recent-target-value-sensitivity-audit-no-promotion-change"
    : "recent-target-value-sensitivity-audit-review-required";

var result = new
{
    phaseId = "phase259-recent-target-value-sensitivity-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetValueSensitivityAuditPassed,
    recentTargetUpdatePromotesBosonMasses,
    currentTargetsConsistentWithRecentReferences,
    failedComparisonsPersistUnderRecentTargets,
    targetRows = new
    {
        current = new[] { currentWTarget, currentZTarget, currentHiggsTarget },
        recentReferences = new[] { recentWTarget, currentBestZTarget, currentBestHiggsTarget },
        drift = new
        {
            wTargetDriftSigma,
            zTargetDriftSigma,
            higgsTargetDriftSigma,
        },
    },
    predictionSensitivity = new
    {
        currentWPrediction,
        currentZPrediction,
        currentWResidualAgainstRecentTarget,
        currentZResidualAgainstBestTarget,
        wFailurePersistsUnderRecentTarget,
        zFailurePersistsUnderBestTarget,
        higgsStillHasNoPrediction,
    },
    empiricalRelationSensitivity = new
    {
        empiricalRelationWithCurrentTargets,
        empiricalRelationWithRecentWTarget,
        empiricalRelationShift,
        phase258RelationPromotesMasses,
        phase258HypotheticalRemainingNullity,
    },
    currentBlockerEvidence = new
    {
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "pdg-2025-w-world-average",
            "https://pdg.lbl.gov/2025/listings/rpp2025-list-w-boson.pdf",
            "PDG 2025 lists a W-boson mass evaluation of 80.3692 +/- 0.0133 GeV."),
        new ExternalSource(
            "cms-2026-nature-w-mass",
            "https://www.nature.com/articles/s41586-026-10168-5",
            "CMS reports m_W = 80.3602 +/- 0.0099 GeV in Nature 652, 321-327 (2026)."),
        new ExternalSource(
            "pdg-2025-gauge-higgs-summary",
            "https://pdgweb.lbl.gov/2025/tables/rpp2025-sum-gauge-higgs-bosons.pdf",
            "PDG 2025 gauge/Higgs summary gives the reference Z and Higgs mass scale used for comparison."),
    },
    checks,
    decision = targetValueSensitivityAuditPassed
        ? "Do not treat recent target-value updates as a route to W/Z/H prediction completion. The current target rows are consistent with recent references, W/Z failed comparisons remain many-sigma failures under the CMS 2026 W value and PDG Z value, Higgs has no predicted value, and source-lineage blockers remain open."
        : "Review target-value sensitivity before relying on current comparison targets.",
    nextRequiredArtifact = new[]
    {
        "A source-lineage theorem for the W/Z absolute scale, not a target-table update.",
        "A solved Higgs scalar-source/self-coupling row, not a target-table update.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase258Path = Phase258Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "recent_target_value_sensitivity_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "recent_target_value_sensitivity_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetValueSensitivityAuditPassed,
        result.recentTargetUpdatePromotesBosonMasses,
        result.currentTargetsConsistentWithRecentReferences,
        result.failedComparisonsPersistUnderRecentTargets,
        result.targetRows,
        result.predictionSensitivity,
        result.empiricalRelationSensitivity,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"targetValueSensitivityAuditPassed={targetValueSensitivityAuditPassed}");
Console.WriteLine($"recentTargetUpdatePromotesBosonMasses={recentTargetUpdatePromotesBosonMasses}");
Console.WriteLine($"currentWResidualAgainstRecentTarget={currentWResidualAgainstRecentTarget}");

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double SigmaDistance(double a, double aSigma, double b, double bSigma) =>
    Math.Abs(a - b) / Math.Sqrt(aSigma * aSigma + bSigma * bSigma);

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
        "current-phase148-target",
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

sealed record Target(string SourceId, string ParticleId, string ObservableId, double Value, double Uncertainty, string Unit);
sealed record Prediction(string ParticleId, string ObservableId, double Value, double Uncertainty, string Unit);
sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExternalSource(string SourceId, string Url, string Finding);

using System.Text.Json;

const string DefaultOutputDir = "studies/phase285_recent_qtp_weak_geometry_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase261Path = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output/electroweak_scheme_radiative_source_audit_summary.json";
const string Phase284Path = "studies/phase284_predicted_ratio_alpha_gf_external_closure_diagnostic_001/output/predicted_ratio_alpha_gf_external_closure_diagnostic_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE285_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase261 = JsonDocument.Parse(File.ReadAllText(Phase261Path));
using var phase284 = JsonDocument.Parse(File.ReadAllText(Phase284Path));

var comparisonRows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wTargetRow = FindRow(comparisonRows, "w-boson");
var zTargetRow = FindRow(comparisonRows, "z-boson");
var higgsTargetRow = FindRow(comparisonRows, "higgs");
var targetWGeV = RequiredDouble(wTargetRow, "targetValue");
var targetZGeV = RequiredDouble(zTargetRow, "targetValue");
var targetHiggsGeV = RequiredDouble(higgsTargetRow, "targetValue");
var targetHiggsUncertaintyGeV = RequiredDouble(higgsTargetRow, "targetUncertainty");
var targetWzRatio = targetWGeV / targetZGeV;

const double QtpWeakAlpha = 1.0 / 30.0;
var qtpWeakGfFromRepoWTarget = Math.PI * QtpWeakAlpha / (Math.Sqrt(2.0) * targetWGeV * targetWGeV);
var qtpHiggsFromRepoWTargetGeV = Math.PI * targetWGeV / 2.0;
var qtpHiggsResidualGeV = qtpHiggsFromRepoWTargetGeV - targetHiggsGeV;
var qtpHiggsPullIfWTargetAllowed = Math.Abs(qtpHiggsResidualGeV) / targetHiggsUncertaintyGeV;

var qtpWeakGeometryLeadPresent = true;
var recentExternalPreprint = new ExternalSource(
    "qtp-weak-geometric-foundations-preprint",
    "Geometric Foundations of the Weak Interaction: Deriving the Fermi Constant and Mixing Angle from Mass-Charge Constraints",
    "https://sciety.org/articles/activity/10.21203/rs.3.rs-8408746/v1",
    "Research Square preprint listed by Sciety; version published December 23, 2025 and latest activity shown January 7, 2026.");
var qtpFrameworkIsGeometricUnity = false;
var qtpUsesMeasuredWzMassesForMixingAngle = true;
var qtpUsesMeasuredWMassForFermiConstant = true;
var qtpUsesMeasuredWMassForHiggsProjection = true;
var qtpDoesNotModifyStandardModelDynamics = true;
var qtpProvidesGuSourceLineage = false;
var qtpProvidesObservedFieldExtraction = false;
var qtpProvidesLowEnergyRgTransport = false;
var qtpProvidesHiggsScalarSourceOperator = false;
var qtpProvidesIndependentVevSource = false;
var qtpPromotesWzMasses = false;
var qtpPromotesHiggsMass = false;
var qtpCompletesBosonPredictions = false;

var phase261ExternalInputsUsed = JsonBool(phase261.RootElement, "schemeInputsAreExternalElectroweakInputs") is true;
var phase284ExternalClosurePasses = JsonBool(phase284.RootElement, "predictedRatioAlphaGfExternalClosureDiagnosticPassed") is true
    && JsonBool(phase284.RootElement, "promotesBosonMasses") is false;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "recent-qtp-weak-geometry-lead-recorded",
        qtpWeakGeometryLeadPresent && !qtpFrameworkIsGeometricUnity,
        $"qtpWeakGeometryLeadPresent={qtpWeakGeometryLeadPresent}; qtpFrameworkIsGeometricUnity={qtpFrameworkIsGeometricUnity}; source={recentExternalPreprint.Url}"),
    new Check(
        "qtp-weak-angle-and-fermi-constant-are-target-or-external-input-dependent",
        qtpUsesMeasuredWzMassesForMixingAngle && qtpUsesMeasuredWMassForFermiConstant && qtpUsesMeasuredWMassForHiggsProjection,
        $"qtpUsesMeasuredWzMassesForMixingAngle={qtpUsesMeasuredWzMassesForMixingAngle}; qtpUsesMeasuredWMassForFermiConstant={qtpUsesMeasuredWMassForFermiConstant}; qtpUsesMeasuredWMassForHiggsProjection={qtpUsesMeasuredWMassForHiggsProjection}; targetWzRatio={targetWzRatio:R}"),
    new Check(
        "qtp-higgs-projection-is-numerical-lead-not-scalar-source",
        qtpHiggsFromRepoWTargetGeV > 0.0 && qtpHiggsPullIfWTargetAllowed > 1.0 && !qtpProvidesHiggsScalarSourceOperator,
        $"qtpHiggsFromRepoWTargetGeV={qtpHiggsFromRepoWTargetGeV:R}; qtpHiggsPullIfWTargetAllowed={qtpHiggsPullIfWTargetAllowed:R}; qtpProvidesHiggsScalarSourceOperator={qtpProvidesHiggsScalarSourceOperator}"),
    new Check(
        "qtp-lead-does-not-fill-gu-source-lineage",
        !qtpProvidesGuSourceLineage
            && !qtpProvidesObservedFieldExtraction
            && !qtpProvidesLowEnergyRgTransport
            && !qtpProvidesIndependentVevSource
            && !qtpPromotesWzMasses
            && !qtpPromotesHiggsMass
            && !qtpCompletesBosonPredictions,
        $"qtpProvidesGuSourceLineage={qtpProvidesGuSourceLineage}; qtpProvidesObservedFieldExtraction={qtpProvidesObservedFieldExtraction}; qtpProvidesLowEnergyRgTransport={qtpProvidesLowEnergyRgTransport}; qtpProvidesIndependentVevSource={qtpProvidesIndependentVevSource}; qtpPromotesWzMasses={qtpPromotesWzMasses}; qtpPromotesHiggsMass={qtpPromotesHiggsMass}"),
    new Check(
        "current-external-alpha-gf-closure-remains-nonpromotional",
        phase261ExternalInputsUsed && phase284ExternalClosurePasses,
        $"phase261ExternalInputsUsed={phase261ExternalInputsUsed}; phase284ExternalClosurePasses={phase284ExternalClosurePasses}"),
    new Check(
        "source-lineage-blockers-remain-binding",
        !unlockContractFilled && newSourceEvidenceStillRequired && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var recentQtpWeakGeometrySourceAuditPassed = checks.All(check => check.Passed)
    && !qtpPromotesWzMasses
    && !qtpPromotesHiggsMass
    && !qtpCompletesBosonPredictions;
var terminalStatus = recentQtpWeakGeometrySourceAuditPassed
    ? "recent-qtp-weak-geometry-source-audit-target-dependent-not-promotion"
    : "recent-qtp-weak-geometry-source-audit-review-required";

var result = new
{
    phaseId = "phase285-recent-qtp-weak-geometry-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    recentQtpWeakGeometrySourceAuditPassed,
    qtpWeakGeometryLeadPresent,
    qtpFrameworkIsGeometricUnity,
    qtpUsesMeasuredWzMassesForMixingAngle,
    qtpUsesMeasuredWMassForFermiConstant,
    qtpUsesMeasuredWMassForHiggsProjection,
    qtpDoesNotModifyStandardModelDynamics,
    qtpProvidesGuSourceLineage,
    qtpProvidesObservedFieldExtraction,
    qtpProvidesLowEnergyRgTransport,
    qtpProvidesHiggsScalarSourceOperator,
    qtpProvidesIndependentVevSource,
    qtpPromotesWzMasses,
    qtpPromotesHiggsMass,
    qtpCompletesBosonPredictions,
    qtpNumericalSnapshot = new
    {
        targetWGeV,
        targetZGeV,
        targetWzRatio,
        qtpWeakAlpha = QtpWeakAlpha,
        qtpWeakGfFromRepoWTarget,
        qtpHiggsFromRepoWTargetGeV,
        targetHiggsGeV,
        targetHiggsUncertaintyGeV,
        qtpHiggsResidualGeV,
        qtpHiggsPullIfWTargetAllowed,
        numericalSnapshotUsesRepoTargets = true,
    },
    currentBlockerEvidence = new
    {
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase245 = new
        {
            unlockContractFilled,
            newSourceEvidenceStillRequired,
        },
        phase261 = new
        {
            phase261ExternalInputsUsed,
        },
        phase284 = new
        {
            phase284ExternalClosurePasses,
        },
    },
    recentExternalPreprint,
    researchFindings = new[]
    {
        "The preprint introduces a mass-charge identity involving e/g and MW/MZ, but its public abstract states that experimentally measured W and Z masses are used to obtain the mixing angle.",
        "Its Fermi-constant estimate substitutes a W-boson mass into a standard electroweak expression, so it does not provide the independent GU v or g source row required by Phase245.",
        "Its Higgs relation MH ~= (pi/2) MW is a W-scale projection lead, not a solved Higgs scalar source/operator, self-coupling lineage, or stability package.",
        "The source is a recent external QTP preprint, not a Geometric Unity source-lineage artifact integrated with this repository's observed-field extraction and promotion gates.",
    },
    checks,
    decision = recentQtpWeakGeometrySourceAuditPassed
        ? "Do not promote W/Z/H masses from the recent QTP weak-geometry lead. It is relevant research for the alpha/GF blockage, but it is external to GU, uses measured W/Z or W mass inputs for the advertised weak-angle/Fermi/Higgs relations, and supplies none of the Phase201/245 source-lineage fields."
        : "Review the QTP weak-geometry source audit before relying on the research-lead classification.",
    nextRequiredArtifact = new[]
    {
        "A GU-local target-independent source row for the low-energy electroweak coupling/scale, or a direct source for log(v g), not a relation constructed from measured W/Z masses.",
        "A GU-local observed-field extraction bridge for separate W and Z source rows.",
        "A solved GU Higgs scalar-source/operator and self-coupling or excitation relation independent of measured W or Higgs masses.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase261Path = Phase261Path,
        phase284Path = Phase284Path,
        qtpScietyUrl = recentExternalPreprint.Url,
        qtpResearchSquareDoi = "https://doi.org/10.21203/rs.3.rs-8408746/v1",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "recent_qtp_weak_geometry_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "recent_qtp_weak_geometry_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.recentQtpWeakGeometrySourceAuditPassed,
        result.qtpWeakGeometryLeadPresent,
        result.qtpFrameworkIsGeometricUnity,
        result.qtpUsesMeasuredWzMassesForMixingAngle,
        result.qtpUsesMeasuredWMassForFermiConstant,
        result.qtpUsesMeasuredWMassForHiggsProjection,
        result.qtpProvidesGuSourceLineage,
        result.qtpProvidesObservedFieldExtraction,
        result.qtpProvidesLowEnergyRgTransport,
        result.qtpProvidesHiggsScalarSourceOperator,
        result.qtpProvidesIndependentVevSource,
        result.qtpPromotesWzMasses,
        result.qtpPromotesHiggsMass,
        result.qtpCompletesBosonPredictions,
        result.qtpNumericalSnapshot,
        result.currentBlockerEvidence,
        result.recentExternalPreprint,
        result.researchFindings,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"recentQtpWeakGeometrySourceAuditPassed={recentQtpWeakGeometrySourceAuditPassed}");
Console.WriteLine($"qtpPromotesWzMasses={qtpPromotesWzMasses}");
Console.WriteLine($"qtpPromotesHiggsMass={qtpPromotesHiggsMass}");
Console.WriteLine($"qtpHiggsFromRepoWTargetGeV={qtpHiggsFromRepoWTargetGeV:R}");

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record ExternalSource(string SourceId, string Title, string Url, string Finding);
sealed record Check(string CheckId, bool Passed, string Detail);

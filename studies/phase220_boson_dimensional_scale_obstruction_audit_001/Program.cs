using System.Text.Json;

const string DefaultOutputDir = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output";
const string Phase101Path = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package_summary.json";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase195Path = "studies/phase195_electroweak_vev_wz_absolute_closure_audit_001/output/electroweak_vev_wz_absolute_closure_audit_summary.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase198Path = "studies/phase198_weak_coupling_source_lineage_closure_audit_001/output/weak_coupling_source_lineage_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";
const string Phase222Path = "studies/phase222_wz_raw_amplitude_source_obstruction_audit_001/output/wz_raw_amplitude_source_obstruction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE220_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase101 = JsonDocument.Parse(File.ReadAllText(Phase101Path));
using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase195 = JsonDocument.Parse(File.ReadAllText(Phase195Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase198 = JsonDocument.Parse(File.ReadAllText(Phase198Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));
using var phase221 = File.Exists(Phase221Path) ? JsonDocument.Parse(File.ReadAllText(Phase221Path)) : null;
using var phase222 = File.Exists(Phase222Path) ? JsonDocument.Parse(File.ReadAllText(Phase222Path)) : null;

var wzMissingFields = JsonStringArray(phase213.RootElement, "wzMissingFields");
var higgsMissingFields = JsonStringArray(phase213.RootElement, "higgsMissingFields");
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? wzMissingFields.Length;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? higgsMissingFields.Length;
var currentDefensibleCount = JsonInt(phase101.RootElement, "defensibleValueCount") ?? 0;
var predictionSetComplete = JsonBool(phase101.RootElement, "predictionSetComplete") is true;

var directCandidateConstructed = JsonBool(phase190.RootElement, "candidateLawConstructed") is true;
var directCandidateTargetIndependent = JsonBool(phase190.RootElement, "targetObservablesUsed") is false;
var theoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
var directPredictionComplete = JsonBool(phase191.RootElement, "canCompleteSuccessfulPrediction") is true;
var vevClosurePromotable = JsonBool(phase195.RootElement, "canPromoteWzAbsoluteFromVevScale") is true;
var weakCouplingClosurePromotable = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation") is true;
var weakCouplingSourcePromotable = JsonBool(phase198.RootElement, "canPromoteAnyWeakCouplingSourceForWzAbsolute") is true;
var higgsSourcePromotable = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var externalElectroweakShortcutPromotable = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is true;
var targetImpliedHiggsShortcutPromotable = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is true;
var officialDraftProvidesDirectWzLaw = JsonBool(phase218.RootElement, "officialDraftProvidesDirectWzLaw") is true;
var officialDraftProvidesSolvedHiggsSource = JsonBool(phase218.RootElement, "officialDraftProvidesSolvedHiggsSource") is true;
var casimirProbeMaterialized = phase221 is not null;
var casimirProbeNumericallySuccessful = casimirProbeMaterialized && JsonBool(phase221!.RootElement, "numericalTargetComparisonPassed") is true;
var casimirProbeSourceLineagePromotable = casimirProbeMaterialized && JsonBool(phase221!.RootElement, "sourceLineagePromotable") is true;
var rawAmplitudeObstructionMaterialized = phase222 is not null;
var rawAmplitudeObstructionCertified = rawAmplitudeObstructionMaterialized && JsonBool(phase222!.RootElement, "rawAmplitudeSourceObstructionCertified") is true;

var obstructionChecks = new[]
{
    new Check(
        "dimensionless-ratio-allowed-but-absolute-scale-not-complete",
        currentDefensibleCount >= 3 && !predictionSetComplete,
        $"defensibleValueCount={currentDefensibleCount}; predictionSetComplete={predictionSetComplete}"),
    new Check(
        "direct-wz-candidate-is-evidence-not-theorem",
        directCandidateConstructed && directCandidateTargetIndependent && !theoremClaimed && !directPredictionComplete,
        $"candidateLawConstructed={directCandidateConstructed}; targetIndependent={directCandidateTargetIndependent}; theoremClaimed={theoremClaimed}; canCompleteSuccessfulPrediction={directPredictionComplete}; decision={JsonString(phase191.RootElement, "decision")}"),
    new Check(
        "electroweak-scale-input-does-not-fill-source-shape",
        !vevClosurePromotable && !weakCouplingClosurePromotable && !weakCouplingSourcePromotable,
        $"canPromoteWzAbsoluteFromVevScale={vevClosurePromotable}; canPromoteWzFromWeakCouplingMassRelation={weakCouplingClosurePromotable}; canPromoteAnyWeakCouplingSourceForWzAbsolute={weakCouplingSourcePromotable}"),
    new Check(
        "higgs-excitation-source-is-absent",
        !higgsSourcePromotable,
        $"canPromoteAnyHiggsScalarSourceLineage={higgsSourcePromotable}; decision={JsonString(phase199.RootElement, "decision")}"),
    new Check(
        "external-or-target-implied-inputs-are-not-gu-predictions",
        !externalElectroweakShortcutPromotable && !targetImpliedHiggsShortcutPromotable,
        $"canPromoteExternalElectroweakBridge={externalElectroweakShortcutPromotable}; canPromoteTargetImpliedHiggsSelfCoupling={targetImpliedHiggsShortcutPromotable}"),
    new Check(
        "official-public-gu-sources-do-not-supply-missing-law",
        !officialDraftProvidesDirectWzLaw && !officialDraftProvidesSolvedHiggsSource,
        $"officialDraftProvidesDirectWzLaw={officialDraftProvidesDirectWzLaw}; officialDraftProvidesSolvedHiggsSource={officialDraftProvidesSolvedHiggsSource}"),
    new Check(
        "source-lineage-contracts-remain-unfilled",
        !allRequiredLineagesPromotable && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "su2-casimir-numerical-lead-is-not-source-lineage",
        casimirProbeMaterialized && casimirProbeNumericallySuccessful && !casimirProbeSourceLineagePromotable,
        casimirProbeMaterialized
            ? $"numericalTargetComparisonPassed={casimirProbeNumericallySuccessful}; sourceLineagePromotable={casimirProbeSourceLineagePromotable}; decision={JsonString(phase221!.RootElement, "decision")}"
            : "Phase221 SU(2) Casimir normalization probe is not materialized."),
    new Check(
        "wz-raw-amplitude-source-obstruction-certified",
        rawAmplitudeObstructionCertified,
        rawAmplitudeObstructionMaterialized
            ? $"rawAmplitudeSourceObstructionCertified={rawAmplitudeObstructionCertified}; bestRawToTargetRatio={JsonDouble(phase222!.RootElement.GetProperty("bestProductionReplay"), "bestRawToTargetRatio")}; decision={JsonString(phase222.RootElement, "decision")}"
            : "Phase222 W/Z raw-amplitude source obstruction audit is not materialized."),
};

var obstructionAuditPassed = obstructionChecks.All(check => check.Passed);
var terminalStatus = obstructionAuditPassed
    ? "boson-dimensional-scale-obstruction-audit-ready-new-source-required"
    : "boson-dimensional-scale-obstruction-audit-inconsistent-review-required";

var result = new
{
    phaseId = "phase220-boson-dimensional-scale-obstruction-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    obstructionAuditPassed,
    obstructionKind = "dimensionful-scale-and-source-lineage-missing",
    physicalReason = "The current artifacts can defensibly support dimensionless/protected claims, but W/Z absolute masses require a target-independent dimensionful electroweak scale plus W/Z particle-specific source-shape/coupling lineage, and Higgs mass requires a solved scalar excitation source/operator. Standard electroweak formulae and PDG masses are comparison/input context, not GU source evidence.",
    standardElectroweakReference = new
    {
        wMassRelation = "m_W = g v / 2",
        zMassRelation = "m_Z = v sqrt(g^2 + g'^2) / 2",
        higgsMassRequirement = "requires a scalar potential/self-coupling or excitation relation, not only an order-parameter/VEV bridge",
        promotionBoundary = "These relations are not sufficient for GU prediction unless g, g', v, and Higgs excitation data are derived from target-independent GU source-lineage artifacts.",
    },
    directWzStatus = new
    {
        directCandidateConstructed,
        directCandidateTargetIndependent,
        theoremClaimed,
        directPredictionComplete,
        phase190Status = JsonString(phase190.RootElement, "terminalStatus"),
        phase191Status = JsonString(phase191.RootElement, "terminalStatus"),
        phase191Decision = JsonString(phase191.RootElement, "decision"),
    },
    electroweakScaleStatus = new
    {
        vevClosurePromotable,
        weakCouplingClosurePromotable,
        weakCouplingSourcePromotable,
        phase195Status = JsonString(phase195.RootElement, "terminalStatus"),
        phase197Status = JsonString(phase197.RootElement, "terminalStatus"),
        phase198Status = JsonString(phase198.RootElement, "terminalStatus"),
        currentWeakCoupling = JsonDouble(phase197.RootElement, "currentWeakCoupling"),
        targetImpliedWeakCoupling = JsonDouble(phase197.RootElement, "targetImpliedWeakCoupling"),
    },
    higgsStatus = new
    {
        higgsSourcePromotable,
        phase199Status = JsonString(phase199.RootElement, "terminalStatus"),
        phase199Decision = JsonString(phase199.RootElement, "decision"),
        targetImpliedHiggsShortcutPromotable,
        targetImpliedQuartic = JsonDouble(phase215.RootElement, "targetImpliedQuartic"),
    },
    publicSourceStatus = new
    {
        officialDraftProvidesDirectWzLaw,
        officialDraftProvidesSolvedHiggsSource,
        phase218Status = JsonString(phase218.RootElement, "terminalStatus"),
        standardModelContext = JsonString(phase218.RootElement, "standardModelContext"),
    },
    su2CasimirNormalizationProbe = casimirProbeMaterialized
        ? new
        {
            numericalTargetComparisonPassed = casimirProbeNumericallySuccessful,
            sourceLineagePromotable = casimirProbeSourceLineagePromotable,
            casimirWeakCoupling = JsonDouble(phase221!.RootElement, "casimirWeakCoupling"),
            decision = JsonString(phase221!.RootElement, "decision"),
        }
        : null,
    rawAmplitudeSourceObstruction = rawAmplitudeObstructionMaterialized
        ? new
        {
            rawAmplitudeSourceObstructionCertified = rawAmplitudeObstructionCertified,
            targetImpliedRawMatrixElementMagnitude = JsonDouble(phase222!.RootElement, "targetImpliedRawMatrixElementMagnitude"),
            bestRawToTargetRatio = JsonDouble(phase222.RootElement.GetProperty("bestProductionReplay"), "bestRawToTargetRatio"),
            decision = JsonString(phase222.RootElement, "decision"),
        }
        : null,
    missingSourceLineage = new
    {
        allRequiredLineagesPromotable,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        wzMissingFields,
        higgsMissingFields,
    },
    obstructionChecks,
    decision = obstructionAuditPassed
        ? "Do not promote W/Z or Higgs absolute masses. The immediate scientific blocker is not a code defect in the generator; it is the absence of target-independent dimensionful source-lineage evidence for W/Z absolute scale and Higgs scalar excitation."
        : "Review upstream artifacts before relying on the dimensional obstruction diagnosis; at least one guard is inconsistent.",
    nextRequiredArtifact = new[]
    {
        "W/Z: a theorem-backed target-independent source lineage deriving v/scale and weak source-shape/coupling with separate W and Z rows, raw gates, common bridge gates, stability sidecars, and target comparison only after source construction.",
        "Higgs: a solved target-independent scalar excitation source/operator with identity envelope, massive scalar profile, potential/self-coupling or excitation relation, stability sidecars, and a passing prediction row.",
    },
    sourceEvidence = new
    {
        phase101Path = Phase101Path,
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase195Path = Phase195Path,
        phase197Path = Phase197Path,
        phase198Path = Phase198Path,
        phase199Path = Phase199Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase214Path = Phase214Path,
        phase215Path = Phase215Path,
        phase218Path = Phase218Path,
        phase221Path = Phase221Path,
        phase222Path = Phase222Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_dimensional_scale_obstruction_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_dimensional_scale_obstruction_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.obstructionAuditPassed,
        result.obstructionKind,
        result.physicalReason,
        result.standardElectroweakReference,
        result.directWzStatus,
        result.electroweakScaleStatus,
        result.higgsStatus,
        result.publicSourceStatus,
        result.su2CasimirNormalizationProbe,
        result.rawAmplitudeSourceObstruction,
        result.missingSourceLineage,
        result.obstructionChecks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"obstructionAuditPassed={obstructionAuditPassed}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

sealed record Check(string CheckId, bool Passed, string Detail);

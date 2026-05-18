using System.Text.Json;

const string DefaultOutputDir = "studies/phase236_low_energy_rg_transport_source_audit_001/output";
const string Phase204Path = "studies/phase204_boson_source_lineage_candidate_scan_001/output/boson_source_lineage_candidate_scan_summary.json";
const string Phase205Path = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output/boson_source_lineage_text_evidence_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string LatestCompletionNotesPath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";

var outputDir = Environment.GetEnvironmentVariable("PHASE236_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase204 = JsonDocument.Parse(File.ReadAllText(Phase204Path));
using var phase205 = JsonDocument.Parse(File.ReadAllText(Phase205Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));

var completionNotes = File.Exists(LatestCompletionNotesPath)
    ? File.ReadAllText(LatestCompletionNotesPath)
    : "";

var sourceLineageCandidateIntakeReadyCount = JsonInt(phase204.RootElement, "intakeReadyCandidateCount") ?? 0;
var sourceLineageTextIntakeReadyCount = JsonInt(phase205.RootElement, "intakeReadyFindingCount") ?? 0;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(closure, "zAbsoluteMassParameterClosure") is true;
var patiSalamWeakMixingNormalizationAuditPassed = JsonBool(phase235.RootElement, "patiSalamWeakMixingNormalizationAuditPassed") is true;
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;

var localCompletionNotesMentionRenormalizationAsAuxiliary = completionNotes.Contains("renormalization prescription", StringComparison.OrdinalIgnoreCase)
    || completionNotes.Contains("renormalization prescriptions", StringComparison.OrdinalIgnoreCase);
var localCompletionNotesWarnHiddenAuxiliaryInvalidatesComparison = completionNotes.Contains("Hidden auxiliary assumptions invalidate the comparison", StringComparison.OrdinalIgnoreCase);
var localCompletionNotesMentionLowEnergyReduction = completionNotes.Contains("low-energy reductions", StringComparison.OrdinalIgnoreCase)
    || completionNotes.Contains("low-energy truncation", StringComparison.OrdinalIgnoreCase);
var guBreakingScaleSourcePresent = false;
var rgTransportOperatorSourcePresent = false;
var thresholdCorrectionSourcePresent = false;
var lowEnergyHyperchargeSourcePresent = false;
var lowEnergyRgTransportSourcePromotable =
    guBreakingScaleSourcePresent
    && rgTransportOperatorSourcePresent
    && thresholdCorrectionSourcePresent
    && lowEnergyHyperchargeSourcePresent
    && sourceLineageCandidateIntakeReadyCount > 0
    && sourceLineageTextIntakeReadyCount > 0
    && wAbsoluteMassParameterClosure
    && zAbsoluteMassParameterClosure
    && patiSalamNormalizationPromotableForLowEnergyWz;

var requirements = new[]
{
    new SourceRequirement("gu-breaking-scale-source", guBreakingScaleSourcePresent, "No GU-derived Pati-Salam/left-right breaking-scale source artifact was found."),
    new SourceRequirement("rg-transport-operator-source", rgTransportOperatorSourcePresent, "No RG transport operator from high-scale normalization to electroweak scale was found."),
    new SourceRequirement("threshold-correction-source", thresholdCorrectionSourcePresent, "No heavy-threshold or scalar-sector threshold correction source was found."),
    new SourceRequirement("low-energy-hypercharge-source", lowEnergyHyperchargeSourcePresent, "No target-independent low-energy g_Y or weak-mixing source row was found."),
};

var checks = new[]
{
    new Check("local-source-scans-have-no-intake-ready-rg-source", sourceLineageCandidateIntakeReadyCount == 0 && sourceLineageTextIntakeReadyCount == 0, $"sourceLineageCandidateIntakeReadyCount={sourceLineageCandidateIntakeReadyCount}; sourceLineageTextIntakeReadyCount={sourceLineageTextIntakeReadyCount}"),
    new Check("completion-notes-treat-renormalization-as-required-auxiliary", localCompletionNotesMentionRenormalizationAsAuxiliary && localCompletionNotesMentionLowEnergyReduction, $"renormalizationMentioned={localCompletionNotesMentionRenormalizationAsAuxiliary}; lowEnergyReductionMentioned={localCompletionNotesMentionLowEnergyReduction}"),
    new Check("completion-notes-warn-hidden-auxiliary-invalidates-comparison", localCompletionNotesWarnHiddenAuxiliaryInvalidatesComparison, $"hiddenAuxiliaryWarning={localCompletionNotesWarnHiddenAuxiliaryInvalidatesComparison}"),
    new Check("required-rg-transport-sources-remain-unfilled", requirements.All(row => !row.Filled), $"missingRequirementCount={requirements.Count(row => !row.Filled)}"),
    new Check("phase235-high-scale-boundary-still-nonpromotable", patiSalamWeakMixingNormalizationAuditPassed && !patiSalamNormalizationPromotableForLowEnergyWz, $"patiSalamWeakMixingNormalizationAuditPassed={patiSalamWeakMixingNormalizationAuditPassed}; patiSalamNormalizationPromotableForLowEnergyWz={patiSalamNormalizationPromotableForLowEnergyWz}"),
    new Check("wz-absolute-parameter-closure-still-blocked", !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && wzMissingFieldCount > 0, $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; wzMissingFieldCount={wzMissingFieldCount}"),
    new Check("low-energy-rg-transport-source-not-promotable", !lowEnergyRgTransportSourcePromotable, $"lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}"),
};

var lowEnergyRgTransportSourceAuditPassed = checks.All(check => check.Passed)
    && !lowEnergyRgTransportSourcePromotable;
var terminalStatus = lowEnergyRgTransportSourceAuditPassed
    ? "low-energy-rg-transport-source-audit-no-promotable-source"
    : "low-energy-rg-transport-source-audit-review-required";

var result = new
{
    phaseId = "phase236-low-energy-rg-transport-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    lowEnergyRgTransportSourcePromotable,
    lowEnergyRgTransportSourceAuditPassed,
    objective = "Audit whether the repository already contains a promotable low-energy RG/threshold transport source for W/Z electroweak parameters.",
    localTheoryContext = new
    {
        latestCompletionNotesPath = LatestCompletionNotesPath,
        localCompletionNotesMentionRenormalizationAsAuxiliary,
        localCompletionNotesWarnHiddenAuxiliaryInvalidatesComparison,
        localCompletionNotesMentionLowEnergyReduction,
        interpretation = "Local completion notes treat low-energy reduction and renormalization prescription as declared auxiliary ingredients, not as silently available source-lineage evidence.",
    },
    scanEvidence = new
    {
        phase204Path = Phase204Path,
        sourceLineageCandidateIntakeReadyCount,
        phase205Path = Phase205Path,
        sourceLineageTextIntakeReadyCount,
    },
    requirements,
    currentRepoEvidence = new
    {
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            wzMissingFieldCount,
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
        },
        phase235 = new
        {
            status = JsonString(phase235.RootElement, "terminalStatus"),
            patiSalamWeakMixingNormalizationAuditPassed,
            patiSalamNormalizationPromotableForLowEnergyWz,
        },
    },
    checks,
    decision = lowEnergyRgTransportSourceAuditPassed
        ? "Do not promote W/Z absolute masses from local RG language. The repository has references to required renormalization/low-energy-reduction auxiliaries, but no promotable source artifact for GU breaking scale, RG transport, thresholds, or low-energy hypercharge/weak-mixing values."
        : "Review local RG transport source evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived breaking-scale source artifact.",
        "A declared RG transport operator from the high-scale normalization to the electroweak comparison scale.",
        "Threshold-correction source rows for heavy gauge/scalar sectors.",
        "A target-independent low-energy g_Y or weak-mixing source row satisfying Phase209/Phase201 gates.",
    },
    sourceEvidence = new
    {
        phase204Path = Phase204Path,
        phase205Path = Phase205Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase235Path = Phase235Path,
        latestCompletionNotesPath = LatestCompletionNotesPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "low_energy_rg_transport_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "low_energy_rg_transport_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.lowEnergyRgTransportSourcePromotable,
        result.lowEnergyRgTransportSourceAuditPassed,
        result.localTheoryContext,
        result.scanEvidence,
        result.requirements,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}");
Console.WriteLine($"lowEnergyRgTransportSourceAuditPassed={lowEnergyRgTransportSourceAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record SourceRequirement(string RequirementId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);

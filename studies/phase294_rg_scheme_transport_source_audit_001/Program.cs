using System.Text.Json;

const string DefaultOutputDir = "studies/phase294_rg_scheme_transport_source_audit_001/output";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase261Path = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output/electroweak_scheme_radiative_source_audit_summary.json";
const string Phase284Path = "studies/phase284_predicted_ratio_alpha_gf_external_closure_diagnostic_001/output/predicted_ratio_alpha_gf_external_closure_diagnostic_summary.json";
const string Phase286Path = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output/alpha_running_threshold_source_viability_audit_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";
const string Phase288Path = "studies/phase288_parameter_source_contract_candidate_scan_001/output/parameter_source_contract_candidate_scan_summary.json";
const string Phase289Path = "studies/phase289_phase288_coverage_false_negative_audit_001/output/phase288_coverage_false_negative_audit_summary.json";
const string Phase292Path = "studies/phase292_electromagnetic_alpha_source_audit_001/output/electromagnetic_alpha_source_audit_summary.json";
const string Phase293Path = "studies/phase293_fermi_vev_source_audit_001/output/fermi_vev_source_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE294_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase261 = JsonDocument.Parse(File.ReadAllText(Phase261Path));
using var phase284 = JsonDocument.Parse(File.ReadAllText(Phase284Path));
using var phase286 = JsonDocument.Parse(File.ReadAllText(Phase286Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));
using var phase288 = JsonDocument.Parse(File.ReadAllText(Phase288Path));
using var phase289 = JsonDocument.Parse(File.ReadAllText(Phase289Path));
using var phase292 = JsonDocument.Parse(File.ReadAllText(Phase292Path));
using var phase293 = JsonDocument.Parse(File.ReadAllText(Phase293Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var phase286Boundary = phase286.RootElement.GetProperty("sourceLineageBoundary");
var leptonicRunningNumericallyClosesWz = JsonBool(phase286.RootElement, "leptonicRunningNumericallyClosesWz") is true;
var importedAlphaMzNumericallyClosesWz = JsonBool(phase286.RootElement, "importedAlphaMzNumericallyClosesWz") is true;
var phase286AlphaRunningSourcePromotable = JsonBool(phase286.RootElement, "alphaRunningSourcePromotable") is true;
var phase286AlphaRunningThresholdRoutePromotesWzMasses = JsonBool(phase286.RootElement, "alphaRunningThresholdRoutePromotesWzMasses") is true;
var guRunningOperatorSourceFound = JsonBool(phase286Boundary, "guRunningOperatorSourceFound") is true;
var guHadronicVacuumPolarizationSourceFound = JsonBool(phase286Boundary, "guHadronicVacuumPolarizationSourceFound") is true;
var guRenormalizationSchemeSourceFound = JsonBool(phase286Boundary, "guRenormalizationSchemeSourceFound") is true;
var phase286ExternalAlphaZeroUsed = JsonBool(phase286Boundary, "externalAlphaZeroUsed") is true;
var phase286ExternalLeptonMassesUsed = JsonBool(phase286Boundary, "externalLeptonMassesUsed") is true;
var phase286ExternalVevUsed = JsonBool(phase286Boundary, "externalVevUsed") is true;

var lowEnergyRgTransportSourceAuditPassed = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourceAuditPassed") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;
var electroweakSchemeRadiativeSourceAuditPassed = JsonBool(phase261.RootElement, "electroweakSchemeRadiativeSourceAuditPassed") is true;
var schemeChoicePromotesBosonMasses = JsonBool(phase261.RootElement, "schemeChoicePromotesBosonMasses") is true;
var anySchemeNearTargetWeakCoupling = JsonBool(phase261.RootElement, "anySchemeNearTargetWeakCoupling") is true;
var schemeInputsAreExternalElectroweakInputs = JsonBool(phase261.RootElement, "schemeInputsAreExternalElectroweakInputs") is true;
var schemeChoiceProvidesGuSourceLineage = JsonBool(phase261.RootElement, "schemeChoiceProvidesGuSourceLineage") is true;
var schemeChoiceProvidesObservedFieldExtraction = JsonBool(phase261.RootElement, "schemeChoiceProvidesObservedFieldExtraction") is true;

var phase284ExternalInputsUsed = JsonBool(phase284.RootElement, "externalInputsUsed") is true;
var phase284LowEnergyRgTransportSourcePresent = JsonBool(phase284.RootElement, "lowEnergyRgTransportSourcePresent") is true;
var phase284PromotesBosonMasses = JsonBool(phase284.RootElement, "promotesBosonMasses") is true;
var phase284SourceContractsFilled = JsonBool(phase284.RootElement, "sourceContractsFilled") is true;

var officialDraftProvidesRgTransportSource = JsonBool(phase287.RootElement, "officialDraftProvidesRgTransportSource") is true;
var officialDraftProvidesHadronicSchemeClosure = JsonBool(phase287.RootElement, "officialDraftProvidesHadronicSchemeClosure") is true;
var officialDraftFillsPhase286Gaps = JsonBool(phase287.RootElement, "officialDraftFillsPhase286Gaps") is true;
var localRgRequirement = FindRequirement(phase288.RootElement, "gu-rg-transport-and-scheme");
var excludedRgRequirement = FindRequirement(phase289.RootElement, "gu-rg-transport-and-scheme");
var localParameterScanRgCandidateCount = RequiredInt(localRgRequirement, "candidateLineCount");
var localParameterScanRgIntakeReadyCount = RequiredInt(localRgRequirement, "intakeReadyCandidateCount");
var localParameterScanRgRequirementFilled = JsonBool(localRgRequirement, "filled") is true;
var excludedCorpusRgCandidateCount = RequiredInt(excludedRgRequirement, "candidateLineCount");
var excludedCorpusRgIntakeReadyCount = RequiredInt(excludedRgRequirement, "intakeReadyCandidateCount");
var excludedCorpusRgRequirementFilled = JsonBool(excludedRgRequirement, "filled") is true;

var electromagneticAlphaSourceAuditPassed = JsonBool(phase292.RootElement, "electromagneticAlphaSourceAuditPassed") is true;
var alphaSourcePromotesBosonPredictions = JsonBool(phase292.RootElement, "alphaSourcePromotesBosonPredictions") is true;
var fermiVevSourceAuditPassed = JsonBool(phase293.RootElement, "fermiVevSourceAuditPassed") is true;
var fermiVevSourcePromotesBosonPredictions = JsonBool(phase293.RootElement, "fermiVevSourcePromotesBosonPredictions") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = RequiredInt(phase213.RootElement, "wzMissingFieldCount");
var higgsMissingFieldCount = RequiredInt(phase213.RootElement, "higgsMissingFieldCount");

var rgSchemeInputsAreExternal = schemeInputsAreExternalElectroweakInputs || phase284ExternalInputsUsed;
var rgSchemeTransportPromotesWzMasses = false;
var rgSchemeTransportPromotesBosonPredictions = false;
var sourceContractsFilled = false;

var checks = new[]
{
    new Check(
        "external-running-diagnostics-numerically-close-wz",
        leptonicRunningNumericallyClosesWz
            && importedAlphaMzNumericallyClosesWz
            && anySchemeNearTargetWeakCoupling
            && rgSchemeInputsAreExternal,
        $"leptonicRunningNumericallyClosesWz={leptonicRunningNumericallyClosesWz}; importedAlphaMzNumericallyClosesWz={importedAlphaMzNumericallyClosesWz}; anySchemeNearTargetWeakCoupling={anySchemeNearTargetWeakCoupling}; rgSchemeInputsAreExternal={rgSchemeInputsAreExternal}"),
    new Check(
        "gu-running-operator-hadronic-and-scheme-sources-absent",
        !guRunningOperatorSourceFound
            && !guHadronicVacuumPolarizationSourceFound
            && !guRenormalizationSchemeSourceFound
            && !phase286AlphaRunningSourcePromotable
            && !phase286AlphaRunningThresholdRoutePromotesWzMasses,
        $"guRunningOperatorSourceFound={guRunningOperatorSourceFound}; guHadronicVacuumPolarizationSourceFound={guHadronicVacuumPolarizationSourceFound}; guRenormalizationSchemeSourceFound={guRenormalizationSchemeSourceFound}; phase286AlphaRunningSourcePromotable={phase286AlphaRunningSourcePromotable}; phase286AlphaRunningThresholdRoutePromotesWzMasses={phase286AlphaRunningThresholdRoutePromotesWzMasses}"),
    new Check(
        "phase236-and-phase261-transport-remain-nonpromotable",
        lowEnergyRgTransportSourceAuditPassed
            && !lowEnergyRgTransportSourcePromotable
            && electroweakSchemeRadiativeSourceAuditPassed
            && !schemeChoicePromotesBosonMasses
            && !schemeChoiceProvidesGuSourceLineage
            && !schemeChoiceProvidesObservedFieldExtraction,
        $"lowEnergyRgTransportSourceAuditPassed={lowEnergyRgTransportSourceAuditPassed}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}; electroweakSchemeRadiativeSourceAuditPassed={electroweakSchemeRadiativeSourceAuditPassed}; schemeChoicePromotesBosonMasses={schemeChoicePromotesBosonMasses}; schemeChoiceProvidesGuSourceLineage={schemeChoiceProvidesGuSourceLineage}; schemeChoiceProvidesObservedFieldExtraction={schemeChoiceProvidesObservedFieldExtraction}"),
    new Check(
        "official-draft-and-scans-do-not-supply-rg-source",
        !officialDraftProvidesRgTransportSource
            && !officialDraftProvidesHadronicSchemeClosure
            && !officialDraftFillsPhase286Gaps
            && localParameterScanRgCandidateCount > 0
            && localParameterScanRgIntakeReadyCount == 0
            && !localParameterScanRgRequirementFilled
            && excludedCorpusRgCandidateCount > 0
            && excludedCorpusRgIntakeReadyCount == 0
            && !excludedCorpusRgRequirementFilled,
        $"officialDraftProvidesRgTransportSource={officialDraftProvidesRgTransportSource}; officialDraftProvidesHadronicSchemeClosure={officialDraftProvidesHadronicSchemeClosure}; officialDraftFillsPhase286Gaps={officialDraftFillsPhase286Gaps}; localRgCandidateCount={localParameterScanRgCandidateCount}; localRgIntakeReadyCount={localParameterScanRgIntakeReadyCount}; excludedRgCandidateCount={excludedCorpusRgCandidateCount}; excludedRgIntakeReadyCount={excludedCorpusRgIntakeReadyCount}"),
    new Check(
        "high-scale-boundary-and-external-inputs-do-not-fill-low-energy-transport",
        !patiSalamNormalizationPromotableForLowEnergyWz
            && phase284ExternalInputsUsed
            && !phase284LowEnergyRgTransportSourcePresent
            && !phase284PromotesBosonMasses
            && !phase284SourceContractsFilled,
        $"patiSalamNormalizationPromotableForLowEnergyWz={patiSalamNormalizationPromotableForLowEnergyWz}; phase284ExternalInputsUsed={phase284ExternalInputsUsed}; phase284LowEnergyRgTransportSourcePresent={phase284LowEnergyRgTransportSourcePresent}; phase284PromotesBosonMasses={phase284PromotesBosonMasses}; phase284SourceContractsFilled={phase284SourceContractsFilled}"),
    new Check(
        "neighboring-alpha-and-vev-audits-remain-nonpromotional",
        electromagneticAlphaSourceAuditPassed
            && !alphaSourcePromotesBosonPredictions
            && fermiVevSourceAuditPassed
            && !fermiVevSourcePromotesBosonPredictions
            && phase286ExternalAlphaZeroUsed
            && phase286ExternalLeptonMassesUsed
            && phase286ExternalVevUsed,
        $"electromagneticAlphaSourceAuditPassed={electromagneticAlphaSourceAuditPassed}; alphaSourcePromotesBosonPredictions={alphaSourcePromotesBosonPredictions}; fermiVevSourceAuditPassed={fermiVevSourceAuditPassed}; fermiVevSourcePromotesBosonPredictions={fermiVevSourcePromotesBosonPredictions}; externalInputs={phase286ExternalAlphaZeroUsed},{phase286ExternalLeptonMassesUsed},{phase286ExternalVevUsed}"),
    new Check(
        "remaining-source-contracts-stay-unfilled",
        !unlockContractFilled
            && newSourceEvidenceStillRequired
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0
            && !sourceContractsFilled
            && !rgSchemeTransportPromotesWzMasses
            && !rgSchemeTransportPromotesBosonPredictions,
        $"unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; rgSchemeTransportPromotesBosonPredictions={rgSchemeTransportPromotesBosonPredictions}"),
};

var rgSchemeTransportSourceAuditPassed = checks.All(check => check.Passed)
    && rgSchemeInputsAreExternal
    && !guRunningOperatorSourceFound
    && !guHadronicVacuumPolarizationSourceFound
    && !guRenormalizationSchemeSourceFound
    && !lowEnergyRgTransportSourcePromotable
    && !schemeChoicePromotesBosonMasses
    && !schemeChoiceProvidesGuSourceLineage
    && !officialDraftProvidesRgTransportSource
    && !officialDraftProvidesHadronicSchemeClosure
    && localParameterScanRgIntakeReadyCount == 0
    && excludedCorpusRgIntakeReadyCount == 0
    && !rgSchemeTransportPromotesWzMasses
    && !rgSchemeTransportPromotesBosonPredictions
    && !sourceContractsFilled;
var terminalStatus = rgSchemeTransportSourceAuditPassed
    ? "rg-scheme-transport-source-audit-external-transport-not-gu-source"
    : "rg-scheme-transport-source-audit-review-required";

var result = new
{
    phaseId = "phase294-rg-scheme-transport-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    rgSchemeTransportSourceAuditPassed,
    leptonicRunningNumericallyClosesWz,
    importedAlphaMzNumericallyClosesWz,
    rgSchemeInputsAreExternal,
    guRunningOperatorSourceFound,
    guHadronicVacuumPolarizationSourceFound,
    guRenormalizationSchemeSourceFound,
    lowEnergyRgTransportSourceAuditPassed,
    lowEnergyRgTransportSourcePromotable,
    electroweakSchemeRadiativeSourceAuditPassed,
    schemeChoicePromotesBosonMasses,
    anySchemeNearTargetWeakCoupling,
    schemeInputsAreExternalElectroweakInputs,
    schemeChoiceProvidesGuSourceLineage,
    schemeChoiceProvidesObservedFieldExtraction,
    officialDraftProvidesRgTransportSource,
    officialDraftProvidesHadronicSchemeClosure,
    localParameterScanRgCandidateCount,
    localParameterScanRgIntakeReadyCount,
    localParameterScanRgRequirementFilled,
    excludedCorpusRgCandidateCount,
    excludedCorpusRgIntakeReadyCount,
    excludedCorpusRgRequirementFilled,
    rgSchemeTransportPromotesWzMasses,
    rgSchemeTransportPromotesBosonPredictions,
    sourceContractsFilled,
    inputBoundary = new
    {
        phase286ExternalAlphaZeroUsed,
        phase286ExternalLeptonMassesUsed,
        phase286ExternalVevUsed,
        phase284ExternalInputsUsed,
        phase284LowEnergyRgTransportSourcePresent,
        phase284PromotesBosonMasses,
        alphaRunningInterpretation = "External alpha(0), lepton thresholds, selected scheme inputs, and alpha(MZ) diagnostics can be numerically useful, but they do not supply a GU-local running operator, hadronic vacuum-polarization source, or renormalization-scheme closure.",
    },
    inheritedEvidence = new
    {
        phase235 = new
        {
            patiSalamNormalizationPromotableForLowEnergyWz,
        },
        phase236 = new
        {
            lowEnergyRgTransportSourceAuditPassed,
            lowEnergyRgTransportSourcePromotable,
        },
        phase261 = new
        {
            electroweakSchemeRadiativeSourceAuditPassed,
            schemeChoicePromotesBosonMasses,
            anySchemeNearTargetWeakCoupling,
            schemeInputsAreExternalElectroweakInputs,
            schemeChoiceProvidesGuSourceLineage,
            schemeChoiceProvidesObservedFieldExtraction,
        },
        phase284 = new
        {
            phase284ExternalInputsUsed,
            phase284LowEnergyRgTransportSourcePresent,
            phase284PromotesBosonMasses,
            phase284SourceContractsFilled,
        },
        phase286 = new
        {
            leptonicRunningNumericallyClosesWz,
            importedAlphaMzNumericallyClosesWz,
            phase286AlphaRunningSourcePromotable,
            phase286AlphaRunningThresholdRoutePromotesWzMasses,
            guRunningOperatorSourceFound,
            guHadronicVacuumPolarizationSourceFound,
            guRenormalizationSchemeSourceFound,
            phase286ExternalAlphaZeroUsed,
            phase286ExternalLeptonMassesUsed,
            phase286ExternalVevUsed,
        },
        phase287 = new
        {
            officialDraftProvidesRgTransportSource,
            officialDraftProvidesHadronicSchemeClosure,
            officialDraftFillsPhase286Gaps,
        },
        phase288 = new
        {
            localParameterScanRgCandidateCount,
            localParameterScanRgIntakeReadyCount,
            localParameterScanRgRequirementFilled,
            intakeReadyParameterSourceCandidateCount = JsonInt(phase288.RootElement, "intakeReadyParameterSourceCandidateCount"),
            anyParameterSourceCandidateFillsContract = JsonBool(phase288.RootElement, "anyParameterSourceCandidateFillsContract"),
        },
        phase289 = new
        {
            excludedCorpusRgCandidateCount,
            excludedCorpusRgIntakeReadyCount,
            excludedCorpusRgRequirementFilled,
            intakeReadyExcludedCorpusCandidateCount = JsonInt(phase289.RootElement, "intakeReadyExcludedCorpusCandidateCount"),
            anyExcludedCorpusCandidateFillsContract = JsonBool(phase289.RootElement, "anyExcludedCorpusCandidateFillsContract"),
        },
        phase292 = new
        {
            electromagneticAlphaSourceAuditPassed,
            alphaSourcePromotesBosonPredictions,
        },
        phase293 = new
        {
            fermiVevSourceAuditPassed,
            fermiVevSourcePromotesBosonPredictions,
        },
        phase245 = new
        {
            unlockContractFilled,
            newSourceEvidenceStillRequired,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "pdg-2025-electroweak-model-review",
            "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
            "Precision electroweak formulas and fits rely on declared coupling, weak-mixing, radiative-correction, and scheme inputs; those inputs are not GU source-lineage evidence."),
        new ExternalSource(
            "hadronic-running-electroweak-couplings-lattice-qcd",
            "https://arxiv.org/abs/2211.11401",
            "Hadronic vacuum polarization contributes nonperturbatively to running electroweak couplings, so a complete transport source needs more than a lepton-only QED log."),
        new ExternalSource(
            "hadronic-contribution-running-alpha-and-weak-angle",
            "https://arxiv.org/abs/1910.09525",
            "The electromagnetic coupling and electroweak mixing angle running depend on hadronic vacuum-polarization functions that are external Standard Model/QCD inputs here."),
    },
    checks,
    decision = rgSchemeTransportSourceAuditPassed
        ? "Do not promote W/Z masses through external low-energy RG, scheme, alpha(MZ), or lepton-running transport. The diagnostics are numerically useful, but the repository still lacks a GU running operator, hadronic vacuum-polarization source, renormalization-scheme closure, observed-field extraction, and the W/Z/H source-lineage contracts remain unfilled."
        : "Review the RG/scheme transport source audit before relying on its non-promotional boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local low-energy RG transport theorem from any high-scale or bare GU normalization to the electroweak comparison scale.",
        "Target-independent charged-threshold, hadronic vacuum-polarization, and renormalization-scheme source rows.",
        "A declared observed-field extraction and physical convention before W/Z target comparison.",
        "The existing alpha and VEV source blockers must also be filled; RG/scheme transport alone cannot complete boson predictions.",
    },
    sourceEvidence = new
    {
        phase235Path = Phase235Path,
        phase236Path = Phase236Path,
        phase261Path = Phase261Path,
        phase284Path = Phase284Path,
        phase286Path = Phase286Path,
        phase287Path = Phase287Path,
        phase288Path = Phase288Path,
        phase289Path = Phase289Path,
        phase292Path = Phase292Path,
        phase293Path = Phase293Path,
        phase245Path = Phase245Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "rg_scheme_transport_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "rg_scheme_transport_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.rgSchemeTransportSourceAuditPassed,
        result.leptonicRunningNumericallyClosesWz,
        result.importedAlphaMzNumericallyClosesWz,
        result.rgSchemeInputsAreExternal,
        result.guRunningOperatorSourceFound,
        result.guHadronicVacuumPolarizationSourceFound,
        result.guRenormalizationSchemeSourceFound,
        result.lowEnergyRgTransportSourcePromotable,
        result.schemeChoicePromotesBosonMasses,
        result.schemeInputsAreExternalElectroweakInputs,
        result.schemeChoiceProvidesGuSourceLineage,
        result.schemeChoiceProvidesObservedFieldExtraction,
        result.officialDraftProvidesRgTransportSource,
        result.officialDraftProvidesHadronicSchemeClosure,
        result.localParameterScanRgCandidateCount,
        result.localParameterScanRgIntakeReadyCount,
        result.excludedCorpusRgCandidateCount,
        result.excludedCorpusRgIntakeReadyCount,
        result.rgSchemeTransportPromotesWzMasses,
        result.rgSchemeTransportPromotesBosonPredictions,
        result.sourceContractsFilled,
        result.inputBoundary,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rgSchemeTransportSourceAuditPassed={rgSchemeTransportSourceAuditPassed}");
Console.WriteLine($"leptonicRunningNumericallyClosesWz={leptonicRunningNumericallyClosesWz}");
Console.WriteLine($"importedAlphaMzNumericallyClosesWz={importedAlphaMzNumericallyClosesWz}");
Console.WriteLine($"rgSchemeInputsAreExternal={rgSchemeInputsAreExternal}");
Console.WriteLine($"guRunningOperatorSourceFound={guRunningOperatorSourceFound}");
Console.WriteLine($"guHadronicVacuumPolarizationSourceFound={guHadronicVacuumPolarizationSourceFound}");
Console.WriteLine($"guRenormalizationSchemeSourceFound={guRenormalizationSchemeSourceFound}");
Console.WriteLine($"localParameterScanRgIntakeReadyCount={localParameterScanRgIntakeReadyCount}");
Console.WriteLine($"excludedCorpusRgIntakeReadyCount={excludedCorpusRgIntakeReadyCount}");
Console.WriteLine($"rgSchemeTransportPromotesBosonPredictions={rgSchemeTransportPromotesBosonPredictions}");

static JsonElement FindRequirement(JsonElement root, string requirementId)
{
    foreach (var row in root.GetProperty("requirementResults").EnumerateArray())
    {
        if (string.Equals(JsonString(row, "requirementId"), requirementId, StringComparison.Ordinal))
        {
            return row;
        }
    }

    throw new InvalidOperationException($"Missing requirement row {requirementId}.");
}

static int RequiredInt(JsonElement element, string propertyName) =>
    JsonInt(element, propertyName) ?? throw new InvalidOperationException($"Missing required integer property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property))
    {
        return null;
    }

    return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
}

sealed record ExternalSource(string SourceId, string Url, string Finding);

sealed record Check(string CheckId, bool Passed, string Detail);

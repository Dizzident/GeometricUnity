using System.Text.Json;

const string DefaultOutputDir = "studies/phase292_electromagnetic_alpha_source_audit_001/output";
const string Phase286Path = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output/alpha_running_threshold_source_viability_audit_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";
const string Phase288Path = "studies/phase288_parameter_source_contract_candidate_scan_001/output/parameter_source_contract_candidate_scan_summary.json";
const string Phase289Path = "studies/phase289_phase288_coverage_false_negative_audit_001/output/phase288_coverage_false_negative_audit_summary.json";
const string Phase291Path = "studies/phase291_koide_charged_lepton_threshold_source_audit_001/output/koide_charged_lepton_threshold_source_audit_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE292_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase286 = JsonDocument.Parse(File.ReadAllText(Phase286Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));
using var phase288 = JsonDocument.Parse(File.ReadAllText(Phase288Path));
using var phase289 = JsonDocument.Parse(File.ReadAllText(Phase289Path));
using var phase291 = JsonDocument.Parse(File.ReadAllText(Phase291Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var phase286InputScalars = phase286.RootElement.GetProperty("inputScalars");
var phase286Boundary = phase286.RootElement.GetProperty("sourceLineageBoundary");
var phase286AlphaZeroInverse = RequiredDouble(phase286InputScalars, "alphaZeroInverse");
var phase286AlphaMzInverse = RequiredDouble(phase286InputScalars, "alphaMzInverse");
var externalAlphaZeroUsed = JsonBool(phase286Boundary, "externalAlphaZeroUsed") is true;
var guAlphaZeroSourceFound = JsonBool(phase286Boundary, "guAlphaZeroSourceFound") is true;
var phase286ExternalLeptonMassesUsed = JsonBool(phase286Boundary, "externalLeptonMassesUsed") is true;
var phase286ExternalVevUsed = JsonBool(phase286Boundary, "externalVevUsed") is true;
var phase286GuChargedLeptonThresholdSourceFound = JsonBool(phase286Boundary, "guChargedLeptonThresholdSourceFound") is true;
var phase286GuRunningOperatorSourceFound = JsonBool(phase286Boundary, "guRunningOperatorSourceFound") is true;
var phase286GuHadronicVacuumPolarizationSourceFound = JsonBool(phase286Boundary, "guHadronicVacuumPolarizationSourceFound") is true;
var phase286GuRenormalizationSchemeSourceFound = JsonBool(phase286Boundary, "guRenormalizationSchemeSourceFound") is true;

var externalAlphaInputsNumericallyCloseWz = JsonBool(phase286.RootElement, "leptonicRunningNumericallyClosesWz") is true
    && JsonBool(phase286.RootElement, "importedAlphaMzNumericallyClosesWz") is true;
var alphaRunningSourcePromotable = JsonBool(phase286.RootElement, "alphaRunningSourcePromotable") is true;
var alphaRunningThresholdRoutePromotesWzMasses = JsonBool(phase286.RootElement, "alphaRunningThresholdRoutePromotesWzMasses") is true;

var officialDraftProvidesAlphaSource = JsonBool(phase287.RootElement, "officialDraftProvidesAlphaSource") is true;
var officialDraftProvidesChargeNormalizationSource = JsonBool(phase287.RootElement, "officialDraftProvidesChargeNormalizationSource") is true;
var officialDraftProvidesRgTransportSource = JsonBool(phase287.RootElement, "officialDraftProvidesRgTransportSource") is true;
var officialDraftProvidesHadronicSchemeClosure = JsonBool(phase287.RootElement, "officialDraftProvidesHadronicSchemeClosure") is true;
var officialDraftFillsPhase286Gaps = JsonBool(phase287.RootElement, "officialDraftFillsPhase286Gaps") is true;

var localAlphaRequirement = FindRequirement(phase288.RootElement, "gu-alpha-or-charge-source");
var excludedAlphaRequirement = FindRequirement(phase289.RootElement, "gu-alpha-or-charge-source");
var localParameterScanAlphaCandidateCount = RequiredInt(localAlphaRequirement, "candidateLineCount");
var localParameterScanAlphaIntakeReadyCount = RequiredInt(localAlphaRequirement, "intakeReadyCandidateCount");
var localParameterScanAlphaRequirementFilled = JsonBool(localAlphaRequirement, "filled") is true;
var excludedCorpusAlphaCandidateCount = RequiredInt(excludedAlphaRequirement, "candidateLineCount");
var excludedCorpusAlphaIntakeReadyCount = RequiredInt(excludedAlphaRequirement, "intakeReadyCandidateCount");
var excludedCorpusAlphaRequirementFilled = JsonBool(excludedAlphaRequirement, "filled") is true;

var patiSalamHighScaleNormalizationPresent = JsonBool(phase235.RootElement, "highScaleWeakMixingBoundaryPresent") is true
    && JsonDouble(phase235.RootElement, "canonicalHighScaleSin2ThetaW") is not null;
var canonicalHighScaleSin2ThetaW = JsonDouble(phase235.RootElement, "canonicalHighScaleSin2ThetaW");
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var koideProvidesIndependentAlphaSource = JsonBool(phase291.RootElement, "koideProvidesIndependentAlphaSource") is true;
var koidePromotesBosonPredictions = JsonBool(phase291.RootElement, "koidePromotesBosonPredictions") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = RequiredInt(phase213.RootElement, "wzMissingFieldCount");
var higgsMissingFieldCount = RequiredInt(phase213.RootElement, "higgsMissingFieldCount");

var alphaSourcePromotesWzMasses = false;
var alphaSourcePromotesBosonPredictions = false;
var sourceContractsFilled = false;

var checks = new[]
{
    new Check(
        "external-alpha-inputs-numerically-close-wz",
        externalAlphaInputsNumericallyCloseWz && externalAlphaZeroUsed,
        $"leptonicRunningNumericallyClosesWz={JsonBool(phase286.RootElement, "leptonicRunningNumericallyClosesWz")}; importedAlphaMzNumericallyClosesWz={JsonBool(phase286.RootElement, "importedAlphaMzNumericallyClosesWz")}; externalAlphaZeroUsed={externalAlphaZeroUsed}; phase286AlphaZeroInverse={phase286AlphaZeroInverse}; phase286AlphaMzInverse={phase286AlphaMzInverse}"),
    new Check(
        "alpha-input-is-external-not-gu-source",
        externalAlphaZeroUsed
            && !guAlphaZeroSourceFound
            && !alphaRunningSourcePromotable
            && !alphaRunningThresholdRoutePromotesWzMasses,
        $"externalAlphaZeroUsed={externalAlphaZeroUsed}; guAlphaZeroSourceFound={guAlphaZeroSourceFound}; alphaRunningSourcePromotable={alphaRunningSourcePromotable}; alphaRunningThresholdRoutePromotesWzMasses={alphaRunningThresholdRoutePromotesWzMasses}"),
    new Check(
        "official-draft-does-not-supply-alpha-or-charge-source",
        !officialDraftProvidesAlphaSource
            && !officialDraftProvidesChargeNormalizationSource
            && !officialDraftProvidesRgTransportSource
            && !officialDraftProvidesHadronicSchemeClosure
            && !officialDraftFillsPhase286Gaps,
        $"officialDraftProvidesAlphaSource={officialDraftProvidesAlphaSource}; officialDraftProvidesChargeNormalizationSource={officialDraftProvidesChargeNormalizationSource}; officialDraftProvidesRgTransportSource={officialDraftProvidesRgTransportSource}; officialDraftProvidesHadronicSchemeClosure={officialDraftProvidesHadronicSchemeClosure}; officialDraftFillsPhase286Gaps={officialDraftFillsPhase286Gaps}"),
    new Check(
        "local-and-excluded-alpha-scans-have-no-intake-ready-source",
        localParameterScanAlphaCandidateCount > 0
            && localParameterScanAlphaIntakeReadyCount == 0
            && !localParameterScanAlphaRequirementFilled
            && excludedCorpusAlphaCandidateCount > 0
            && excludedCorpusAlphaIntakeReadyCount == 0
            && !excludedCorpusAlphaRequirementFilled,
        $"localCandidateCount={localParameterScanAlphaCandidateCount}; localIntakeReadyCount={localParameterScanAlphaIntakeReadyCount}; localFilled={localParameterScanAlphaRequirementFilled}; excludedCandidateCount={excludedCorpusAlphaCandidateCount}; excludedIntakeReadyCount={excludedCorpusAlphaIntakeReadyCount}; excludedFilled={excludedCorpusAlphaRequirementFilled}"),
    new Check(
        "high-scale-normalization-is-not-low-energy-alpha-source",
        patiSalamHighScaleNormalizationPresent
            && !patiSalamNormalizationPromotableForLowEnergyWz
            && !lowEnergyRgTransportSourcePromotable,
        $"patiSalamHighScaleNormalizationPresent={patiSalamHighScaleNormalizationPresent}; canonicalHighScaleSin2ThetaW={canonicalHighScaleSin2ThetaW}; patiSalamNormalizationPromotableForLowEnergyWz={patiSalamNormalizationPromotableForLowEnergyWz}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}"),
    new Check(
        "neighboring-threshold-koide-route-does-not-supply-alpha",
        !koideProvidesIndependentAlphaSource && !koidePromotesBosonPredictions,
        $"koideProvidesIndependentAlphaSource={koideProvidesIndependentAlphaSource}; koidePromotesBosonPredictions={koidePromotesBosonPredictions}"),
    new Check(
        "remaining-source-contracts-stay-unfilled",
        phase286ExternalLeptonMassesUsed
            && phase286ExternalVevUsed
            && !phase286GuChargedLeptonThresholdSourceFound
            && !phase286GuRunningOperatorSourceFound
            && !phase286GuHadronicVacuumPolarizationSourceFound
            && !phase286GuRenormalizationSchemeSourceFound
            && !unlockContractFilled
            && newSourceEvidenceStillRequired
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0
            && !sourceContractsFilled
            && !alphaSourcePromotesWzMasses
            && !alphaSourcePromotesBosonPredictions,
        $"externalLeptonMassesUsed={phase286ExternalLeptonMassesUsed}; externalVevUsed={phase286ExternalVevUsed}; guChargedLeptonThresholdSourceFound={phase286GuChargedLeptonThresholdSourceFound}; guRunningOperatorSourceFound={phase286GuRunningOperatorSourceFound}; guHadronicVacuumPolarizationSourceFound={phase286GuHadronicVacuumPolarizationSourceFound}; guRenormalizationSchemeSourceFound={phase286GuRenormalizationSchemeSourceFound}; unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var electromagneticAlphaSourceAuditPassed = checks.All(check => check.Passed)
    && externalAlphaInputsNumericallyCloseWz
    && externalAlphaZeroUsed
    && !guAlphaZeroSourceFound
    && !officialDraftProvidesAlphaSource
    && !officialDraftProvidesChargeNormalizationSource
    && localParameterScanAlphaIntakeReadyCount == 0
    && excludedCorpusAlphaIntakeReadyCount == 0
    && !patiSalamNormalizationPromotableForLowEnergyWz
    && !lowEnergyRgTransportSourcePromotable
    && !koideProvidesIndependentAlphaSource
    && !alphaSourcePromotesWzMasses
    && !alphaSourcePromotesBosonPredictions
    && !sourceContractsFilled;
var terminalStatus = electromagneticAlphaSourceAuditPassed
    ? "electromagnetic-alpha-source-audit-external-input-not-gu-source"
    : "electromagnetic-alpha-source-audit-review-required";

var result = new
{
    phaseId = "phase292-electromagnetic-alpha-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    electromagneticAlphaSourceAuditPassed,
    externalAlphaInputsNumericallyCloseWz,
    externalAlphaZeroUsed,
    guAlphaZeroSourceFound,
    officialDraftProvidesAlphaSource,
    officialDraftProvidesChargeNormalizationSource,
    officialDraftProvidesRgTransportSource,
    officialDraftProvidesHadronicSchemeClosure,
    localParameterScanAlphaCandidateCount,
    localParameterScanAlphaIntakeReadyCount,
    localParameterScanAlphaRequirementFilled,
    excludedCorpusAlphaCandidateCount,
    excludedCorpusAlphaIntakeReadyCount,
    excludedCorpusAlphaRequirementFilled,
    patiSalamHighScaleNormalizationPresent,
    canonicalHighScaleSin2ThetaW,
    patiSalamNormalizationPromotableForLowEnergyWz,
    lowEnergyRgTransportSourcePromotable,
    koideProvidesIndependentAlphaSource,
    koideDoesNotProvideAlpha = !koideProvidesIndependentAlphaSource,
    alphaSourcePromotesWzMasses,
    alphaSourcePromotesBosonPredictions,
    sourceContractsFilled,
    inputBoundary = new
    {
        phase286AlphaZeroInverse,
        phase286AlphaMzInverse,
        codata2022AlphaInverse = 137.035999177,
        phase286ExternalLeptonMassesUsed,
        phase286ExternalVevUsed,
        phase286GuChargedLeptonThresholdSourceFound,
        phase286GuRunningOperatorSourceFound,
        phase286GuHadronicVacuumPolarizationSourceFound,
        phase286GuRenormalizationSchemeSourceFound,
        alphaInputInterpretation = "Phase286's numerically close W/Z diagnostics import electromagnetic alpha inputs and related running context. Phase292 does not alter those numbers; it audits whether a GU-local alpha/electric-charge source row exists.",
    },
    inheritedEvidence = new
    {
        phase286 = new
        {
            leptonicRunningNumericallyClosesWz = JsonBool(phase286.RootElement, "leptonicRunningNumericallyClosesWz"),
            importedAlphaMzNumericallyClosesWz = JsonBool(phase286.RootElement, "importedAlphaMzNumericallyClosesWz"),
            alphaRunningSourcePromotable,
            alphaRunningThresholdRoutePromotesWzMasses,
            externalAlphaZeroUsed,
            guAlphaZeroSourceFound,
            phase286ExternalLeptonMassesUsed,
            phase286ExternalVevUsed,
            phase286GuChargedLeptonThresholdSourceFound,
            phase286GuRunningOperatorSourceFound,
            phase286GuHadronicVacuumPolarizationSourceFound,
            phase286GuRenormalizationSchemeSourceFound,
        },
        phase287 = new
        {
            officialDraftProvidesAlphaSource,
            officialDraftProvidesChargeNormalizationSource,
            officialDraftProvidesRgTransportSource,
            officialDraftProvidesHadronicSchemeClosure,
            officialDraftFillsPhase286Gaps,
        },
        phase288 = new
        {
            localParameterScanAlphaCandidateCount,
            localParameterScanAlphaIntakeReadyCount,
            localParameterScanAlphaRequirementFilled,
            intakeReadyParameterSourceCandidateCount = JsonInt(phase288.RootElement, "intakeReadyParameterSourceCandidateCount"),
            anyParameterSourceCandidateFillsContract = JsonBool(phase288.RootElement, "anyParameterSourceCandidateFillsContract"),
        },
        phase289 = new
        {
            excludedCorpusAlphaCandidateCount,
            excludedCorpusAlphaIntakeReadyCount,
            excludedCorpusAlphaRequirementFilled,
            intakeReadyExcludedCorpusCandidateCount = JsonInt(phase289.RootElement, "intakeReadyExcludedCorpusCandidateCount"),
            anyExcludedCorpusCandidateFillsContract = JsonBool(phase289.RootElement, "anyExcludedCorpusCandidateFillsContract"),
        },
        phase235 = new
        {
            patiSalamHighScaleNormalizationPresent,
            canonicalHighScaleSin2ThetaW,
            patiSalamNormalizationPromotableForLowEnergyWz,
        },
        phase236 = new
        {
            lowEnergyRgTransportSourcePromotable,
        },
        phase291 = new
        {
            koideProvidesIndependentAlphaSource,
            koidePromotesBosonPredictions,
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
            "The electroweak review presents W/Z tree-level masses in terms of e, sin(theta_W), cos(theta_W), and v, and treats alpha(0), GF, and MZ as precision-scheme inputs rather than derived GU quantities."),
        new ExternalSource(
            "nist-codata-2022-fundamental-constants",
            "https://www.nist.gov/pml/fundamental-physical-constants",
            "NIST publishes CODATA recommended physical constants from a 2022 least-squares adjustment, including alpha as an externally measured/recommended constant."),
        new ExternalSource(
            "nist-fine-structure-constant-running",
            "https://physics.nist.gov/cuu/Constants/alpha.html",
            "NIST describes alpha as the electromagnetic coupling whose effective value runs with energy because of vacuum polarization."),
        new ExternalSource(
            "hadronic-running-electroweak-couplings",
            "https://arxiv.org/abs/1910.09525",
            "Hadronic vacuum polarization is a nonperturbative contribution to the running of alpha and the electroweak mixing angle, reinforcing that a lepton-only log is not a complete GU-local running source."),
    },
    checks,
    decision = electromagneticAlphaSourceAuditPassed
        ? "Do not promote W/Z masses through external electromagnetic alpha, imported alpha(MZ), or high-scale charge normalization. Phase286's external-alpha diagnostics are numerically useful, but the repository still lacks a GU-local alpha/electric-charge source row, low-energy RG/scheme transport, charged thresholds, VEV source, and Higgs scalar-source lineage."
        : "Review the electromagnetic alpha source audit before relying on its non-promotional boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local electromagnetic charge or alpha source row at a declared reference scale.",
        "A GU-local low-energy RG transport theorem with thresholds, hadronic vacuum-polarization, and scheme closure.",
        "A target-independent GU VEV or direct W/Z absolute-scale source replacing the external Fermi-scale input.",
        "A solved GU Higgs scalar-source/self-coupling lineage; alpha alone does not fill the Higgs blocker.",
    },
    sourceEvidence = new
    {
        phase286Path = Phase286Path,
        phase287Path = Phase287Path,
        phase288Path = Phase288Path,
        phase289Path = Phase289Path,
        phase291Path = Phase291Path,
        phase235Path = Phase235Path,
        phase236Path = Phase236Path,
        phase245Path = Phase245Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "electromagnetic_alpha_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electromagnetic_alpha_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.electromagneticAlphaSourceAuditPassed,
        result.externalAlphaInputsNumericallyCloseWz,
        result.externalAlphaZeroUsed,
        result.guAlphaZeroSourceFound,
        result.officialDraftProvidesAlphaSource,
        result.officialDraftProvidesChargeNormalizationSource,
        result.localParameterScanAlphaCandidateCount,
        result.localParameterScanAlphaIntakeReadyCount,
        result.excludedCorpusAlphaCandidateCount,
        result.excludedCorpusAlphaIntakeReadyCount,
        result.patiSalamHighScaleNormalizationPresent,
        result.patiSalamNormalizationPromotableForLowEnergyWz,
        result.lowEnergyRgTransportSourcePromotable,
        result.koideProvidesIndependentAlphaSource,
        result.koideDoesNotProvideAlpha,
        result.alphaSourcePromotesWzMasses,
        result.alphaSourcePromotesBosonPredictions,
        result.sourceContractsFilled,
        result.inputBoundary,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"electromagneticAlphaSourceAuditPassed={electromagneticAlphaSourceAuditPassed}");
Console.WriteLine($"externalAlphaInputsNumericallyCloseWz={externalAlphaInputsNumericallyCloseWz}");
Console.WriteLine($"externalAlphaZeroUsed={externalAlphaZeroUsed}");
Console.WriteLine($"guAlphaZeroSourceFound={guAlphaZeroSourceFound}");
Console.WriteLine($"localParameterScanAlphaIntakeReadyCount={localParameterScanAlphaIntakeReadyCount}");
Console.WriteLine($"excludedCorpusAlphaIntakeReadyCount={excludedCorpusAlphaIntakeReadyCount}");
Console.WriteLine($"patiSalamNormalizationPromotableForLowEnergyWz={patiSalamNormalizationPromotableForLowEnergyWz}");
Console.WriteLine($"alphaSourcePromotesBosonPredictions={alphaSourcePromotesBosonPredictions}");

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

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidOperationException($"Missing required numeric property {propertyName}.");

static int RequiredInt(JsonElement element, string propertyName) =>
    JsonInt(element, propertyName) ?? throw new InvalidOperationException($"Missing required integer property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number ? property.GetDouble() : null;

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

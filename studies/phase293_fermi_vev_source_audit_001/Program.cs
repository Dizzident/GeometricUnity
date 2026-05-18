using System.Text.Json;

const string DefaultOutputDir = "studies/phase293_fermi_vev_source_audit_001/output";
const string Phase195Path = "studies/phase195_electroweak_vev_wz_absolute_closure_audit_001/output/electroweak_vev_wz_absolute_closure_audit_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase253Path = "studies/phase253_global_observed_sector_vacuum_scan_001/output/global_observed_sector_vacuum_scan_summary.json";
const string Phase284Path = "studies/phase284_predicted_ratio_alpha_gf_external_closure_diagnostic_001/output/predicted_ratio_alpha_gf_external_closure_diagnostic_summary.json";
const string Phase286Path = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output/alpha_running_threshold_source_viability_audit_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";
const string Phase288Path = "studies/phase288_parameter_source_contract_candidate_scan_001/output/parameter_source_contract_candidate_scan_summary.json";
const string Phase289Path = "studies/phase289_phase288_coverage_false_negative_audit_001/output/phase288_coverage_false_negative_audit_summary.json";
const string Phase292Path = "studies/phase292_electromagnetic_alpha_source_audit_001/output/electromagnetic_alpha_source_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE293_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase195 = JsonDocument.Parse(File.ReadAllText(Phase195Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase253 = JsonDocument.Parse(File.ReadAllText(Phase253Path));
using var phase284 = JsonDocument.Parse(File.ReadAllText(Phase284Path));
using var phase286 = JsonDocument.Parse(File.ReadAllText(Phase286Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));
using var phase288 = JsonDocument.Parse(File.ReadAllText(Phase288Path));
using var phase289 = JsonDocument.Parse(File.ReadAllText(Phase289Path));
using var phase292 = JsonDocument.Parse(File.ReadAllText(Phase292Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var phase286InputScalars = phase286.RootElement.GetProperty("inputScalars");
var phase286Boundary = phase286.RootElement.GetProperty("sourceLineageBoundary");
var externalVevGeV = RequiredDouble(phase286InputScalars, "externalVevGeV");
var externalVevUncertaintyGeV = RequiredDouble(phase286InputScalars, "externalVevUncertaintyGeV");
var derivedFermiCouplingGeVMinus2 = 1.0 / (Math.Sqrt(2.0) * externalVevGeV * externalVevGeV);
var externalVevUsed = JsonBool(phase286Boundary, "externalVevUsed") is true;
var phase286ExternalAlphaZeroUsed = JsonBool(phase286Boundary, "externalAlphaZeroUsed") is true;
var phase286ExternalLeptonMassesUsed = JsonBool(phase286Boundary, "externalLeptonMassesUsed") is true;
var targetMassesUsedForLeptonicRunningConstruction = JsonBool(phase286Boundary, "targetMassesUsedForLeptonicRunningConstruction") is true;

var externalVevParticipatesInNumericalWzClosure = JsonBool(phase284.RootElement, "anyRowPassesWzTargetComparison") is true
    && JsonBool(phase284.RootElement, "externalInputsUsed") is true;
var alphaGfExternalClosurePromotesBosonMasses = JsonBool(phase284.RootElement, "promotesBosonMasses") is true;
var phase284CompleteGuSourceLineagePresent = JsonBool(phase284.RootElement, "completeGuSourceLineagePresent") is true;
var phase284VevSourceLineagePresent = JsonBool(phase284.RootElement, "vevSourceLineagePresent") is true;
var phase284AlphaSourceLineagePresent = JsonBool(phase284.RootElement, "alphaSourceLineagePresent") is true;

var canPromoteWzAbsoluteFromVevScale = JsonBool(phase195.RootElement, "canPromoteWzAbsoluteFromVevScale") is true;
var canPromoteExternalElectroweakBridge = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is true;
var targetIndependentGuVevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var electroweakVevSourceLineageObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;
var globalObservedSectorVacuumCandidateFound = JsonBool(phase253.RootElement, "globalObservedSectorVacuumCandidateFound") is true;
var productionObservedSectorVacuumCandidateCount = JsonInt(phase253.RootElement, "productionObservedSectorVacuumCandidateCount") ?? -1;
var globalScanFillsVacuumMassMatrixUnlock = JsonBool(phase253.RootElement, "globalScanFillsVacuumMassMatrixUnlock") is true;

var officialDraftProvidesTargetIndependentVevSource = JsonBool(phase287.RootElement, "officialDraftProvidesTargetIndependentVevSource") is true;
var officialDraftProvidesHiggsScalarExtraction = JsonBool(phase287.RootElement, "officialDraftProvidesHiggsScalarExtraction") is true;
var officialDraftFillsPhase286Gaps = JsonBool(phase287.RootElement, "officialDraftFillsPhase286Gaps") is true;
var localVevRequirement = FindRequirement(phase288.RootElement, "gu-vev-source");
var excludedVevRequirement = FindRequirement(phase289.RootElement, "gu-vev-source");
var localParameterScanVevCandidateCount = RequiredInt(localVevRequirement, "candidateLineCount");
var localParameterScanVevIntakeReadyCount = RequiredInt(localVevRequirement, "intakeReadyCandidateCount");
var localParameterScanVevRequirementFilled = JsonBool(localVevRequirement, "filled") is true;
var excludedCorpusVevCandidateCount = RequiredInt(excludedVevRequirement, "candidateLineCount");
var excludedCorpusVevIntakeReadyCount = RequiredInt(excludedVevRequirement, "intakeReadyCandidateCount");
var excludedCorpusVevRequirementFilled = JsonBool(excludedVevRequirement, "filled") is true;

var electromagneticAlphaSourceAuditPassed = JsonBool(phase292.RootElement, "electromagneticAlphaSourceAuditPassed") is true;
var alphaSourcePromotesBosonPredictions = JsonBool(phase292.RootElement, "alphaSourcePromotesBosonPredictions") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var wzMissingFieldCount = RequiredInt(phase213.RootElement, "wzMissingFieldCount");
var higgsMissingFieldCount = RequiredInt(phase213.RootElement, "higgsMissingFieldCount");

var guVevSourceFound = targetIndependentGuVevSourcePromotable;
var fermiVevSourcePromotesWzMasses = false;
var fermiVevSourcePromotesBosonPredictions = false;
var sourceContractsFilled = false;

var checks = new[]
{
    new Check(
        "external-fermi-vev-input-materialized",
        externalVevUsed
            && externalVevGeV > 0.0
            && externalVevUncertaintyGeV > 0.0
            && derivedFermiCouplingGeVMinus2 > 0.0,
        $"externalVevUsed={externalVevUsed}; externalVevGeV={externalVevGeV}; externalVevUncertaintyGeV={externalVevUncertaintyGeV}; derivedFermiCouplingGeVMinus2={derivedFermiCouplingGeVMinus2}"),
    new Check(
        "external-alpha-gf-diagnostic-can-numerically-close-but-not-promote",
        externalVevParticipatesInNumericalWzClosure
            && !alphaGfExternalClosurePromotesBosonMasses
            && !phase284CompleteGuSourceLineagePresent
            && !phase284VevSourceLineagePresent,
        $"externalVevParticipatesInNumericalWzClosure={externalVevParticipatesInNumericalWzClosure}; alphaGfExternalClosurePromotesBosonMasses={alphaGfExternalClosurePromotesBosonMasses}; completeGuSourceLineagePresent={phase284CompleteGuSourceLineagePresent}; vevSourceLineagePresent={phase284VevSourceLineagePresent}"),
    new Check(
        "vev-scale-alone-and-external-electroweak-loophole-remain-blocked",
        !canPromoteWzAbsoluteFromVevScale && !canPromoteExternalElectroweakBridge,
        $"canPromoteWzAbsoluteFromVevScale={canPromoteWzAbsoluteFromVevScale}; canPromoteExternalElectroweakBridge={canPromoteExternalElectroweakBridge}"),
    new Check(
        "gu-vev-vacuum-source-lineage-is-missing",
        electroweakVevSourceLineageObstructionCertified
            && !targetIndependentGuVevSourcePromotable
            && !globalObservedSectorVacuumCandidateFound
            && productionObservedSectorVacuumCandidateCount == 0
            && !globalScanFillsVacuumMassMatrixUnlock,
        $"electroweakVevSourceLineageObstructionCertified={electroweakVevSourceLineageObstructionCertified}; targetIndependentGuVevSourcePromotable={targetIndependentGuVevSourcePromotable}; globalObservedSectorVacuumCandidateFound={globalObservedSectorVacuumCandidateFound}; productionObservedSectorVacuumCandidateCount={productionObservedSectorVacuumCandidateCount}; globalScanFillsVacuumMassMatrixUnlock={globalScanFillsVacuumMassMatrixUnlock}"),
    new Check(
        "official-draft-and-scans-do-not-supply-vev-source",
        !officialDraftProvidesTargetIndependentVevSource
            && !officialDraftProvidesHiggsScalarExtraction
            && !officialDraftFillsPhase286Gaps
            && localParameterScanVevCandidateCount > 0
            && localParameterScanVevIntakeReadyCount == 0
            && !localParameterScanVevRequirementFilled
            && excludedCorpusVevCandidateCount > 0
            && excludedCorpusVevIntakeReadyCount == 0
            && !excludedCorpusVevRequirementFilled,
        $"officialDraftProvidesTargetIndependentVevSource={officialDraftProvidesTargetIndependentVevSource}; officialDraftProvidesHiggsScalarExtraction={officialDraftProvidesHiggsScalarExtraction}; officialDraftFillsPhase286Gaps={officialDraftFillsPhase286Gaps}; localVevCandidateCount={localParameterScanVevCandidateCount}; localVevIntakeReadyCount={localParameterScanVevIntakeReadyCount}; excludedVevCandidateCount={excludedCorpusVevCandidateCount}; excludedVevIntakeReadyCount={excludedCorpusVevIntakeReadyCount}"),
    new Check(
        "alpha-audit-does-not-hide-vev-source",
        electromagneticAlphaSourceAuditPassed
            && !alphaSourcePromotesBosonPredictions
            && phase286ExternalAlphaZeroUsed
            && phase286ExternalLeptonMassesUsed
            && !targetMassesUsedForLeptonicRunningConstruction,
        $"electromagneticAlphaSourceAuditPassed={electromagneticAlphaSourceAuditPassed}; alphaSourcePromotesBosonPredictions={alphaSourcePromotesBosonPredictions}; phase286ExternalAlphaZeroUsed={phase286ExternalAlphaZeroUsed}; phase286ExternalLeptonMassesUsed={phase286ExternalLeptonMassesUsed}; targetMassesUsedForLeptonicRunningConstruction={targetMassesUsedForLeptonicRunningConstruction}"),
    new Check(
        "remaining-source-contracts-stay-unfilled",
        !guVevSourceFound
            && !unlockContractFilled
            && newSourceEvidenceStillRequired
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0
            && !sourceContractsFilled
            && !fermiVevSourcePromotesWzMasses
            && !fermiVevSourcePromotesBosonPredictions,
        $"guVevSourceFound={guVevSourceFound}; unlockContractFilled={unlockContractFilled}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; fermiVevSourcePromotesBosonPredictions={fermiVevSourcePromotesBosonPredictions}"),
};

var fermiVevSourceAuditPassed = checks.All(check => check.Passed)
    && externalVevUsed
    && externalVevParticipatesInNumericalWzClosure
    && !guVevSourceFound
    && !officialDraftProvidesTargetIndependentVevSource
    && localParameterScanVevIntakeReadyCount == 0
    && excludedCorpusVevIntakeReadyCount == 0
    && !targetIndependentGuVevSourcePromotable
    && !globalObservedSectorVacuumCandidateFound
    && !canPromoteExternalElectroweakBridge
    && !canPromoteWzAbsoluteFromVevScale
    && !fermiVevSourcePromotesWzMasses
    && !fermiVevSourcePromotesBosonPredictions
    && !sourceContractsFilled;
var terminalStatus = fermiVevSourceAuditPassed
    ? "fermi-vev-source-audit-external-input-not-gu-vacuum"
    : "fermi-vev-source-audit-review-required";

var result = new
{
    phaseId = "phase293-fermi-vev-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    fermiVevSourceAuditPassed,
    externalVevUsed,
    externalVevGeV,
    externalVevUncertaintyGeV,
    derivedFermiCouplingGeVMinus2,
    externalVevParticipatesInNumericalWzClosure,
    alphaGfExternalClosurePromotesBosonMasses,
    canPromoteWzAbsoluteFromVevScale,
    canPromoteExternalElectroweakBridge,
    guVevSourceFound,
    targetIndependentGuVevSourcePromotable,
    electroweakVevSourceLineageObstructionCertified,
    globalObservedSectorVacuumCandidateFound,
    productionObservedSectorVacuumCandidateCount,
    globalScanFillsVacuumMassMatrixUnlock,
    officialDraftProvidesTargetIndependentVevSource,
    officialDraftProvidesHiggsScalarExtraction,
    officialDraftFillsPhase286Gaps,
    localParameterScanVevCandidateCount,
    localParameterScanVevIntakeReadyCount,
    localParameterScanVevRequirementFilled,
    excludedCorpusVevCandidateCount,
    excludedCorpusVevIntakeReadyCount,
    excludedCorpusVevRequirementFilled,
    fermiVevSourcePromotesWzMasses,
    fermiVevSourcePromotesBosonPredictions,
    sourceContractsFilled,
    inputBoundary = new
    {
        externalVevGeV,
        externalVevUncertaintyGeV,
        derivedFermiCouplingGeVMinus2,
        codataFermiCouplingReferenceGeVMinus2 = 1.1663787e-5,
        externalVevSourceClass = "Fermi-derived external electroweak vacuum scale imported as diagnostic/order-parameter context, not a GU vacuum solution.",
        phase286ExternalAlphaZeroUsed,
        phase286ExternalLeptonMassesUsed,
        phase284AlphaSourceLineagePresent,
        phase284VevSourceLineagePresent,
    },
    inheritedEvidence = new
    {
        phase195 = new
        {
            canPromoteWzAbsoluteFromVevScale,
        },
        phase214 = new
        {
            canPromoteExternalElectroweakBridge,
        },
        phase229 = new
        {
            targetIndependentGuVevSourcePromotable,
            electroweakVevSourceLineageObstructionCertified,
        },
        phase253 = new
        {
            globalObservedSectorVacuumCandidateFound,
            productionObservedSectorVacuumCandidateCount,
            globalScanFillsVacuumMassMatrixUnlock,
        },
        phase284 = new
        {
            externalVevParticipatesInNumericalWzClosure,
            alphaGfExternalClosurePromotesBosonMasses,
            phase284CompleteGuSourceLineagePresent,
            phase284VevSourceLineagePresent,
            phase284AlphaSourceLineagePresent,
        },
        phase286 = new
        {
            externalVevUsed,
            phase286ExternalAlphaZeroUsed,
            phase286ExternalLeptonMassesUsed,
            targetMassesUsedForLeptonicRunningConstruction,
        },
        phase287 = new
        {
            officialDraftProvidesTargetIndependentVevSource,
            officialDraftProvidesHiggsScalarExtraction,
            officialDraftFillsPhase286Gaps,
        },
        phase288 = new
        {
            localParameterScanVevCandidateCount,
            localParameterScanVevIntakeReadyCount,
            localParameterScanVevRequirementFilled,
            intakeReadyParameterSourceCandidateCount = JsonInt(phase288.RootElement, "intakeReadyParameterSourceCandidateCount"),
            anyParameterSourceCandidateFillsContract = JsonBool(phase288.RootElement, "anyParameterSourceCandidateFillsContract"),
        },
        phase289 = new
        {
            excludedCorpusVevCandidateCount,
            excludedCorpusVevIntakeReadyCount,
            excludedCorpusVevRequirementFilled,
            intakeReadyExcludedCorpusCandidateCount = JsonInt(phase289.RootElement, "intakeReadyExcludedCorpusCandidateCount"),
            anyExcludedCorpusCandidateFillsContract = JsonBool(phase289.RootElement, "anyExcludedCorpusCandidateFillsContract"),
        },
        phase292 = new
        {
            electromagneticAlphaSourceAuditPassed,
            alphaSourcePromotesBosonPredictions,
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
            "The electroweak review relates W/Z masses to v, g, and the weak mixing structure; the relation identifies required inputs but does not derive a GU vacuum source."),
        new ExternalSource(
            "nist-codata-2022-fundamental-constants",
            "https://www.nist.gov/publications/codata-recommended-values-fundamental-physical-constants-2022",
            "NIST publishes the CODATA recommended Fermi coupling constant as an externally measured/recommended physical constant."),
        new ExternalSource(
            "nist-constants-database",
            "https://physics.nist.gov/constants",
            "NIST's constants database is the reference source for CODATA constants; importing those values does not create GU-local source lineage."),
    },
    checks,
    decision = fermiVevSourceAuditPassed
        ? "Do not promote W/Z masses through the external Fermi-derived VEV. The external VEV participates in alpha/GF numerical closure diagnostics, but the repository still lacks a GU vacuum solution, source-derived vacuum selection rule, VEV source-lineage derivation, physical mass-matrix extraction, and Higgs scalar-source closure."
        : "Review the Fermi/VEV source audit before relying on its non-promotional boundary.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU vacuum/background solution and source-derived vacuum-selection rule.",
        "A replayable GU VEV source-lineage derivation independent of W/Z/H target masses and independent of the Phase54 Fermi scale.",
        "A quadratic bosonic mass-matrix extraction around that vacuum with W/Z/photon/Higgs eigenstate projection and unit normalization.",
        "A solved GU Higgs scalar-source/self-coupling lineage; importing v alone does not solve the Higgs blocker.",
    },
    sourceEvidence = new
    {
        phase195Path = Phase195Path,
        phase214Path = Phase214Path,
        phase229Path = Phase229Path,
        phase253Path = Phase253Path,
        phase284Path = Phase284Path,
        phase286Path = Phase286Path,
        phase287Path = Phase287Path,
        phase288Path = Phase288Path,
        phase289Path = Phase289Path,
        phase292Path = Phase292Path,
        phase245Path = Phase245Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "fermi_vev_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermi_vev_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.fermiVevSourceAuditPassed,
        result.externalVevUsed,
        result.externalVevGeV,
        result.derivedFermiCouplingGeVMinus2,
        result.externalVevParticipatesInNumericalWzClosure,
        result.guVevSourceFound,
        result.targetIndependentGuVevSourcePromotable,
        result.electroweakVevSourceLineageObstructionCertified,
        result.globalObservedSectorVacuumCandidateFound,
        result.productionObservedSectorVacuumCandidateCount,
        result.globalScanFillsVacuumMassMatrixUnlock,
        result.officialDraftProvidesTargetIndependentVevSource,
        result.localParameterScanVevCandidateCount,
        result.localParameterScanVevIntakeReadyCount,
        result.excludedCorpusVevCandidateCount,
        result.excludedCorpusVevIntakeReadyCount,
        result.canPromoteWzAbsoluteFromVevScale,
        result.canPromoteExternalElectroweakBridge,
        result.fermiVevSourcePromotesWzMasses,
        result.fermiVevSourcePromotesBosonPredictions,
        result.sourceContractsFilled,
        result.inputBoundary,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fermiVevSourceAuditPassed={fermiVevSourceAuditPassed}");
Console.WriteLine($"externalVevUsed={externalVevUsed}");
Console.WriteLine($"externalVevGeV={externalVevGeV}");
Console.WriteLine($"derivedFermiCouplingGeVMinus2={derivedFermiCouplingGeVMinus2}");
Console.WriteLine($"externalVevParticipatesInNumericalWzClosure={externalVevParticipatesInNumericalWzClosure}");
Console.WriteLine($"guVevSourceFound={guVevSourceFound}");
Console.WriteLine($"localParameterScanVevIntakeReadyCount={localParameterScanVevIntakeReadyCount}");
Console.WriteLine($"excludedCorpusVevIntakeReadyCount={excludedCorpusVevIntakeReadyCount}");
Console.WriteLine($"fermiVevSourcePromotesBosonPredictions={fermiVevSourcePromotesBosonPredictions}");

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

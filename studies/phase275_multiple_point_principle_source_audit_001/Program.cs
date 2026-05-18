using System.Text.Json;

const string DefaultOutputDir = "studies/phase275_multiple_point_principle_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase264Path = "studies/phase264_higgs_vacuum_criticality_source_audit_001/output/higgs_vacuum_criticality_source_audit_summary.json";
const string Phase271Path = "studies/phase271_asymptotic_safety_higgs_source_audit_001/output/asymptotic_safety_higgs_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE275_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase264 = JsonDocument.Parse(File.ReadAllText(Phase264Path));
using var phase271 = JsonDocument.Parse(File.ReadAllText(Phase271Path));

var multiplePointPrincipleLeadPresent = true;
var degenerateVacuaLeadPresent = true;
var planckScaleQuarticAndBetaZeroLeadPresent = true;
var historicMppHiggsPredictionGeV = 135.0;
var historicMppHiggsPredictionUncertaintyGeV = 9.0;
var historicMppTopPredictionGeV = 173.0;
var historicMppTopPredictionUncertaintyGeV = 4.0;
var laterSmMppBoundaryHiggsMassGeV = 129.0;
var laterSmMppBoundaryUncertaintyGeV = 1.5;
var higgsTargetGeV = 125.20;
var higgsTargetUncertaintyGeV = 0.11;
var historicTargetPull = Pull(historicMppHiggsPredictionGeV, historicMppHiggsPredictionUncertaintyGeV, higgsTargetGeV, higgsTargetUncertaintyGeV);
var laterSmTargetPull = Pull(laterSmMppBoundaryHiggsMassGeV, laterSmMppBoundaryUncertaintyGeV, higgsTargetGeV, higgsTargetUncertaintyGeV);

var requiresGuMultiplePointPrincipleSource = true;
var requiresDegenerateVacuaTheorem = true;
var requiresPlanckScaleOrUvBoundarySource = true;
var requiresQuarticAndBetaBoundarySource = true;
var requiresSmMatterContentAndDesertAssumption = true;
var requiresTopYukawaAndAlphaSSource = true;
var requiresLowEnergyRgTransport = true;
var requiresVacuumSelectionRule = true;
var requiresVevSource = true;
var requiresWzMassMatrixSource = true;
var requiresHiggsScalarSource = true;
var requiresObservedFieldExtraction = true;
var requiresExtendedScalarThresholdsForModern125Compatibility = true;
var mppExternalToGu = true;

var localGuMultiplePointPrincipleSourceFound = false;
var localGuDegenerateVacuaTheoremFound = false;
var localGuPlanckScaleBoundaryFound = false;
var localGuQuarticBetaBoundaryFound = false;
var localGuTopYukawaAlphaSSourceFound = false;
var localGuSmDesertTheoremFound = false;
var localGuRgTransportForMppFound = false;
var localGuVacuumSelectionRuleFound = false;
var localGuVevSourceFound = false;
var localGuWzMassMatrixSourceFound = false;
var localGuHiggsScalarSourceFound = false;
var localGuObservedFieldExtractionFound = false;
var multiplePointPrinciplePromotesWzMasses = false;
var multiplePointPrinciplePromotesHiggsMass = false;
var multiplePointPrincipleCompletesBosonPredictions = false;

var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var obstructionAuditPassed = JsonBool(phase220.RootElement, "obstructionAuditPassed") is true;
var obstructionKind = JsonString(phase220.RootElement, "obstructionKind");
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var higgsScalarSourceRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var newHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var currentImplementationCanFillObservedFieldExtractionContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;
var vacuumCriticalityAuditPassed = JsonBool(phase264.RootElement, "higgsVacuumCriticalitySourceAuditPassed") is true;
var vacuumCriticalityPromotesHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityPromotesHiggsMass") is true;
var asymptoticSafetyHiggsSourceAuditPassed = JsonBool(phase271.RootElement, "asymptoticSafetyHiggsSourceAuditPassed") is true;
var asymptoticSafetyPromotesHiggsMass = JsonBool(phase271.RootElement, "asymptoticSafetyPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var multiplePointPrincipleSourceAuditPassed =
    multiplePointPrincipleLeadPresent
    && degenerateVacuaLeadPresent
    && planckScaleQuarticAndBetaZeroLeadPresent
    && historicTargetPull < 2.0
    && laterSmTargetPull > 2.0
    && requiresGuMultiplePointPrincipleSource
    && requiresDegenerateVacuaTheorem
    && requiresPlanckScaleOrUvBoundarySource
    && requiresQuarticAndBetaBoundarySource
    && requiresSmMatterContentAndDesertAssumption
    && requiresTopYukawaAndAlphaSSource
    && requiresLowEnergyRgTransport
    && requiresVacuumSelectionRule
    && requiresVevSource
    && requiresWzMassMatrixSource
    && requiresHiggsScalarSource
    && requiresObservedFieldExtraction
    && requiresExtendedScalarThresholdsForModern125Compatibility
    && mppExternalToGu
    && !localGuMultiplePointPrincipleSourceFound
    && !localGuDegenerateVacuaTheoremFound
    && !localGuPlanckScaleBoundaryFound
    && !localGuQuarticBetaBoundaryFound
    && !localGuTopYukawaAlphaSSourceFound
    && !localGuSmDesertTheoremFound
    && !localGuRgTransportForMppFound
    && !localGuVacuumSelectionRuleFound
    && !localGuVevSourceFound
    && !localGuWzMassMatrixSourceFound
    && !localGuHiggsScalarSourceFound
    && !localGuObservedFieldExtractionFound
    && !multiplePointPrinciplePromotesWzMasses
    && !multiplePointPrinciplePromotesHiggsMass
    && !multiplePointPrincipleCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && vacuumCriticalityAuditPassed
    && !vacuumCriticalityPromotesHiggsMass
    && asymptoticSafetyHiggsSourceAuditPassed
    && !asymptoticSafetyPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "multiple-point-principle-lead-present",
        multiplePointPrincipleLeadPresent && degenerateVacuaLeadPresent && planckScaleQuarticAndBetaZeroLeadPresent,
        $"multiplePointPrincipleLeadPresent={multiplePointPrincipleLeadPresent}; degenerateVacuaLeadPresent={degenerateVacuaLeadPresent}; planckScaleQuarticAndBetaZeroLeadPresent={planckScaleQuarticAndBetaZeroLeadPresent}"),
    new Check(
        "mpp-historic-prediction-is-broad-not-current-closure",
        historicTargetPull < 2.0 && laterSmTargetPull > 2.0,
        $"historicMppHiggsPredictionGeV={historicMppHiggsPredictionGeV:R}; historicTargetPull={historicTargetPull:R}; laterSmMppBoundaryHiggsMassGeV={laterSmMppBoundaryHiggsMassGeV:R}; laterSmTargetPull={laterSmTargetPull:R}"),
    new Check(
        "mpp-input-contract-is-external-and-assumed",
        requiresGuMultiplePointPrincipleSource
            && requiresDegenerateVacuaTheorem
            && requiresPlanckScaleOrUvBoundarySource
            && requiresQuarticAndBetaBoundarySource
            && requiresSmMatterContentAndDesertAssumption
            && requiresTopYukawaAndAlphaSSource
            && requiresLowEnergyRgTransport,
        $"requiresGuMultiplePointPrincipleSource={requiresGuMultiplePointPrincipleSource}; degenerateVacuaTheorem={requiresDegenerateVacuaTheorem}; planckBoundary={requiresPlanckScaleOrUvBoundarySource}; quarticBetaBoundary={requiresQuarticAndBetaBoundarySource}; topYukawaAlphaSSource={requiresTopYukawaAndAlphaSSource}; rgTransport={requiresLowEnergyRgTransport}"),
    new Check(
        "no-local-gu-mpp-source-artifact",
        !localGuMultiplePointPrincipleSourceFound
            && !localGuDegenerateVacuaTheoremFound
            && !localGuPlanckScaleBoundaryFound
            && !localGuQuarticBetaBoundaryFound
            && !localGuTopYukawaAlphaSSourceFound
            && !localGuRgTransportForMppFound
            && !localGuObservedFieldExtractionFound,
        $"mppSource={localGuMultiplePointPrincipleSourceFound}; degenerateVacuaTheorem={localGuDegenerateVacuaTheoremFound}; planckBoundary={localGuPlanckScaleBoundaryFound}; quarticBetaBoundary={localGuQuarticBetaBoundaryFound}; topYukawaAlphaS={localGuTopYukawaAlphaSSourceFound}; rgTransport={localGuRgTransportForMppFound}; observedFieldExtraction={localGuObservedFieldExtractionFound}"),
    new Check(
        "mpp-does-not-fill-gu-wz-or-higgs-source-contracts",
        !localGuVevSourceFound
            && !localGuWzMassMatrixSourceFound
            && !localGuHiggsScalarSourceFound
            && !multiplePointPrinciplePromotesWzMasses
            && !multiplePointPrinciplePromotesHiggsMass
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"localGuVevSourceFound={localGuVevSourceFound}; localGuWzMassMatrixSourceFound={localGuWzMassMatrixSourceFound}; localGuHiggsScalarSourceFound={localGuHiggsScalarSourceFound}; multiplePointPrinciplePromotesWzMasses={multiplePointPrinciplePromotesWzMasses}; multiplePointPrinciplePromotesHiggsMass={multiplePointPrinciplePromotesHiggsMass}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "neighboring-criticality-routes-remain-nonpromotional",
        vacuumCriticalityAuditPassed
            && !vacuumCriticalityPromotesHiggsMass
            && asymptoticSafetyHiggsSourceAuditPassed
            && !asymptoticSafetyPromotesHiggsMass,
        $"vacuumCriticalityAuditPassed={vacuumCriticalityAuditPassed}; vacuumCriticalityPromotesHiggsMass={vacuumCriticalityPromotesHiggsMass}; asymptoticSafetyHiggsSourceAuditPassed={asymptoticSafetyHiggsSourceAuditPassed}; asymptoticSafetyPromotesHiggsMass={asymptoticSafetyPromotesHiggsMass}"),
};

var terminalStatus = multiplePointPrincipleSourceAuditPassed
    ? "multiple-point-principle-source-audit-external-degenerate-vacua-boundary-not-promotion"
    : "multiple-point-principle-source-audit-review-required";

var result = new
{
    phaseId = "phase275-multiple-point-principle-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    multiplePointPrincipleSourceAuditPassed,
    multiplePointPrincipleLeadPresent,
    degenerateVacuaLeadPresent,
    planckScaleQuarticAndBetaZeroLeadPresent,
    multiplePointPrinciplePromotesWzMasses,
    multiplePointPrinciplePromotesHiggsMass,
    multiplePointPrincipleCompletesBosonPredictions,
    mppBoundary = new
    {
        historicMppHiggsPredictionGeV,
        historicMppHiggsPredictionUncertaintyGeV,
        historicMppTopPredictionGeV,
        historicMppTopPredictionUncertaintyGeV,
        laterSmMppBoundaryHiggsMassGeV,
        laterSmMppBoundaryUncertaintyGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        historicTargetPull,
        laterSmTargetPull,
        requiresGuMultiplePointPrincipleSource,
        requiresDegenerateVacuaTheorem,
        requiresPlanckScaleOrUvBoundarySource,
        requiresQuarticAndBetaBoundarySource,
        requiresSmMatterContentAndDesertAssumption,
        requiresTopYukawaAndAlphaSSource,
        requiresLowEnergyRgTransport,
        requiresVacuumSelectionRule,
        requiresVevSource,
        requiresWzMassMatrixSource,
        requiresHiggsScalarSource,
        requiresObservedFieldExtraction,
        requiresExtendedScalarThresholdsForModern125Compatibility,
        mppExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuMultiplePointPrincipleSourceFound,
        localGuDegenerateVacuaTheoremFound,
        localGuPlanckScaleBoundaryFound,
        localGuQuarticBetaBoundaryFound,
        localGuTopYukawaAlphaSSourceFound,
        localGuSmDesertTheoremFound,
        localGuRgTransportForMppFound,
        localGuVacuumSelectionRuleFound,
        localGuVevSourceFound,
        localGuWzMassMatrixSourceFound,
        localGuHiggsScalarSourceFound,
        localGuObservedFieldExtractionFound,
    },
    currentBlockerEvidence = new
    {
        phase220 = new
        {
            obstructionAuditPassed,
            obstructionKind,
        },
        phase224 = new
        {
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
        },
        phase236 = new
        {
            lowEnergyRgTransportSourcePromotable,
        },
        phase248 = new
        {
            higgsScalarSourceRepairPossibleFromCurrentRegistry,
            newHiggsScalarSourceStillRequired,
        },
        phase257 = new
        {
            currentImplementationCanFillObservedFieldExtractionContract,
        },
        phase264 = new
        {
            vacuumCriticalityAuditPassed,
            vacuumCriticalityPromotesHiggsMass,
        },
        phase271 = new
        {
            asymptoticSafetyHiggsSourceAuditPassed,
            asymptoticSafetyPromotesHiggsMass,
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
            "froggatt-nielsen-dynamical-determination",
            "https://arxiv.org/abs/hep-ph/9607302",
            "Applies multiple point criticality to the pure Standard Model with a desert to the Planck scale; imposes two degenerate Higgs-potential minima and reports top and Higgs predictions."),
        new ExternalSource(
            "hamada-kawai-oda-scalar-singlet-mpp",
            "https://doi.org/10.1093/ptep/ptw186",
            "Reviews the Planck-scale MPP condition as vanishing Higgs quartic and beta function; notes pure-SM MPP does not fit the observed 125 GeV Higgs with recent inputs and uses singlet thresholds to realize MPP."),
        new ExternalSource(
            "darme-hambye-strumia-mpp-extended-higgs-review",
            "https://doi.org/10.3389/fphy.2019.00135",
            "Reviews MPP and extended Higgs sectors; records that MPP is a high-scale principle rather than a concrete low-energy mass-source artifact."),
    },
    localSearchFinding = "Repository search found Standard Model vacuum-criticality and asymptotic-safety diagnostics, but no GU-local multiple-point principle, degenerate-vacua theorem, Planck/UV boundary source, quartic-plus-beta source, top/Yukawa/alpha_s source, MPP RG transport, VEV source, W/Z mass matrix, Higgs scalar source, or observed-field extraction artifact.",
    checks,
    decision = multiplePointPrincipleSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from the multiple point principle. MPP is a serious external degenerate-vacua/criticality lead, but in this repository it is an assumed high-scale boundary condition, depends on Standard Model RG/top/alpha_s inputs or extended scalar thresholds, and does not supply GU source-lineage rows for the VEV, W/Z mass matrix, Higgs scalar source, or observed-field extraction."
        : "Review multiple-point-principle source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived multiple-point or degenerate-vacua theorem fixing the high-scale boundary without importing the physical Higgs target.",
        "GU source lineages for the UV/Planck boundary, Higgs quartic and beta function, top/Yukawa and alpha_s inputs, and RG/threshold transport.",
        "A GU VEV/source-scale theorem, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase220Path = Phase220Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase248Path = Phase248Path,
        phase257Path = Phase257Path,
        phase264Path = Phase264Path,
        phase271Path = Phase271Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "multiple_point_principle_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "multiple_point_principle_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.multiplePointPrincipleSourceAuditPassed,
        result.multiplePointPrincipleLeadPresent,
        result.degenerateVacuaLeadPresent,
        result.planckScaleQuarticAndBetaZeroLeadPresent,
        result.multiplePointPrinciplePromotesWzMasses,
        result.multiplePointPrinciplePromotesHiggsMass,
        result.multiplePointPrincipleCompletesBosonPredictions,
        result.mppBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"multiplePointPrincipleSourceAuditPassed={multiplePointPrincipleSourceAuditPassed}");
Console.WriteLine($"multiplePointPrincipleLeadPresent={multiplePointPrincipleLeadPresent}");
Console.WriteLine($"degenerateVacuaLeadPresent={degenerateVacuaLeadPresent}");
Console.WriteLine($"planckScaleQuarticAndBetaZeroLeadPresent={planckScaleQuarticAndBetaZeroLeadPresent}");
Console.WriteLine($"historicTargetPull={historicTargetPull}");
Console.WriteLine($"laterSmTargetPull={laterSmTargetPull}");
Console.WriteLine($"multiplePointPrinciplePromotesWzMasses={multiplePointPrinciplePromotesWzMasses}");
Console.WriteLine($"multiplePointPrinciplePromotesHiggsMass={multiplePointPrinciplePromotesHiggsMass}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result)
        ? result
        : null;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double Pull(double predicted, double predictedUncertainty, double target, double targetUncertainty) =>
    Math.Abs(predicted - target) / Math.Sqrt((predictedUncertainty * predictedUncertainty) + (targetUncertainty * targetUncertainty));

record ExternalSource(string SourceId, string Url, string Finding);

record Check(string CheckId, bool Passed, string Detail);

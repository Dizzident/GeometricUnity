using System.Text.Json;

const string DefaultOutputDir = "studies/phase271_asymptotic_safety_higgs_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase264Path = "studies/phase264_higgs_vacuum_criticality_source_audit_001/output/higgs_vacuum_criticality_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE271_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase264 = JsonDocument.Parse(File.ReadAllText(Phase264Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var higgsRow = FindRow(rows, "higgs");
var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");

var asymptoticSafetyGravityLeadPresent = true;
var asymptoticSafetyHiggsPredictionLeadPresent = true;
var asymptoticSafetyPredictedHiggsMassGeV = 126.0;
var asymptoticSafetyPredictionUncertaintyGeV = 3.0;
var asymptoticSafetyPredictionPull = Math.Abs(asymptoticSafetyPredictedHiggsMassGeV - higgsTargetGeV)
    / Math.Sqrt(asymptoticSafetyPredictionUncertaintyGeV * asymptoticSafetyPredictionUncertaintyGeV + higgsTargetUncertaintyGeV * higgsTargetUncertaintyGeV);
var targetInsideAsymptoticSafetyPredictionBand = asymptoticSafetyPredictionPull < 1.0;
var asymptoticSafetyRequiresGravityFixedPointSource = true;
var asymptoticSafetyRequiresPositiveQuarticAnomalousDimensionSource = true;
var asymptoticSafetyRequiresQuarticFixedPointBoundary = true;
var asymptoticSafetyRequiresNoIntermediateScalesAssumption = true;
var asymptoticSafetyRequiresSmPlusGravityMatterContentSource = true;
var asymptoticSafetyRequiresPlanckMatchingAndRgTransport = true;
var asymptoticSafetyRequiresTopYukawaAndAlphaSSource = true;
var asymptoticSafetyRequiresHiggsMassSchemeSource = true;
var asymptoticSafetyExternalToGu = true;

var localGuAsymptoticSafetyFixedPointSourceFound = false;
var localGuGravityMatterBetaFunctionsFound = false;
var localGuQuarticAnomalousDimensionSourceFound = false;
var localGuQuarticFixedPointBoundaryFound = false;
var localGuNoIntermediateScaleTheoremFound = false;
var localGuPlanckMatchingRgTransportFound = false;
var localGuTopYukawaAlphaSSourceFound = false;
var localGuHiggsScalarSourceFound = false;
var localGuVevSourceFound = false;
var localGuObservedFieldExtractionFound = false;
var localGuWzMassMatrixSourceFound = false;
var asymptoticSafetyPromotesWzMasses = false;
var asymptoticSafetyPromotesHiggsMass = false;
var asymptoticSafetyCompletesBosonPredictions = false;

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
var higgsVacuumCriticalitySourceAuditPassed = JsonBool(phase264.RootElement, "higgsVacuumCriticalitySourceAuditPassed") is true;
var vacuumCriticalityPromotesHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityPromotesHiggsMass") is true;
var vacuumCriticalityBoundaryNumericallyNearHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityBoundaryNumericallyNearHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var asymptoticSafetyHiggsSourceAuditPassed =
    asymptoticSafetyGravityLeadPresent
    && asymptoticSafetyHiggsPredictionLeadPresent
    && targetInsideAsymptoticSafetyPredictionBand
    && asymptoticSafetyRequiresGravityFixedPointSource
    && asymptoticSafetyRequiresPositiveQuarticAnomalousDimensionSource
    && asymptoticSafetyRequiresQuarticFixedPointBoundary
    && asymptoticSafetyRequiresNoIntermediateScalesAssumption
    && asymptoticSafetyRequiresSmPlusGravityMatterContentSource
    && asymptoticSafetyRequiresPlanckMatchingAndRgTransport
    && asymptoticSafetyRequiresTopYukawaAndAlphaSSource
    && asymptoticSafetyRequiresHiggsMassSchemeSource
    && asymptoticSafetyExternalToGu
    && !localGuAsymptoticSafetyFixedPointSourceFound
    && !localGuGravityMatterBetaFunctionsFound
    && !localGuQuarticAnomalousDimensionSourceFound
    && !localGuQuarticFixedPointBoundaryFound
    && !localGuNoIntermediateScaleTheoremFound
    && !localGuPlanckMatchingRgTransportFound
    && !localGuTopYukawaAlphaSSourceFound
    && !localGuHiggsScalarSourceFound
    && !localGuVevSourceFound
    && !localGuObservedFieldExtractionFound
    && !localGuWzMassMatrixSourceFound
    && !asymptoticSafetyPromotesWzMasses
    && !asymptoticSafetyPromotesHiggsMass
    && !asymptoticSafetyCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && higgsVacuumCriticalitySourceAuditPassed
    && !vacuumCriticalityPromotesHiggsMass
    && vacuumCriticalityBoundaryNumericallyNearHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "asymptotic-safety-higgs-mass-lead-is-near-target",
        asymptoticSafetyGravityLeadPresent
            && asymptoticSafetyHiggsPredictionLeadPresent
            && targetInsideAsymptoticSafetyPredictionBand,
        $"asymptoticSafetyPredictedHiggsMassGeV={asymptoticSafetyPredictedHiggsMassGeV:R}; asymptoticSafetyPredictionUncertaintyGeV={asymptoticSafetyPredictionUncertaintyGeV:R}; higgsTargetGeV={higgsTargetGeV:R}; asymptoticSafetyPredictionPull={asymptoticSafetyPredictionPull:R}"),
    new Check(
        "asymptotic-safety-input-contract-is-not-gu-sourced",
        asymptoticSafetyRequiresGravityFixedPointSource
            && asymptoticSafetyRequiresPositiveQuarticAnomalousDimensionSource
            && asymptoticSafetyRequiresQuarticFixedPointBoundary
            && asymptoticSafetyRequiresNoIntermediateScalesAssumption
            && asymptoticSafetyRequiresSmPlusGravityMatterContentSource,
        $"fixedPointSource={asymptoticSafetyRequiresGravityFixedPointSource}; positiveQuarticAnomalousDimension={asymptoticSafetyRequiresPositiveQuarticAnomalousDimensionSource}; quarticFixedPointBoundary={asymptoticSafetyRequiresQuarticFixedPointBoundary}; noIntermediateScales={asymptoticSafetyRequiresNoIntermediateScalesAssumption}; smPlusGravityMatterContent={asymptoticSafetyRequiresSmPlusGravityMatterContentSource}"),
    new Check(
        "no-local-gu-asymptotic-safety-source-artifact",
        !localGuAsymptoticSafetyFixedPointSourceFound
            && !localGuGravityMatterBetaFunctionsFound
            && !localGuQuarticAnomalousDimensionSourceFound
            && !localGuQuarticFixedPointBoundaryFound
            && !localGuPlanckMatchingRgTransportFound
            && !localGuObservedFieldExtractionFound,
        $"fixedPointSource={localGuAsymptoticSafetyFixedPointSourceFound}; gravityMatterBetaFunctions={localGuGravityMatterBetaFunctionsFound}; quarticAnomalousDimensionSource={localGuQuarticAnomalousDimensionSourceFound}; quarticFixedPointBoundary={localGuQuarticFixedPointBoundaryFound}; planckMatchingRgTransport={localGuPlanckMatchingRgTransportFound}; observedFieldExtraction={localGuObservedFieldExtractionFound}"),
    new Check(
        "current-repo-source-blockers-still-active",
        obstructionAuditPassed
            && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !lowEnergyRgTransportSourcePromotable
            && newHiggsScalarSourceStillRequired
            && !currentImplementationCanFillObservedFieldExtractionContract,
        $"obstructionAuditPassed={obstructionAuditPassed}; obstructionKind={obstructionKind}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}; currentImplementationCanFillObservedFieldExtractionContract={currentImplementationCanFillObservedFieldExtractionContract}"),
    new Check(
        "asymptotic-safety-does-not-complete-boson-predictions",
        !asymptoticSafetyPromotesWzMasses
            && !asymptoticSafetyPromotesHiggsMass
            && !asymptoticSafetyCompletesBosonPredictions
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"asymptoticSafetyPromotesWzMasses={asymptoticSafetyPromotesWzMasses}; asymptoticSafetyPromotesHiggsMass={asymptoticSafetyPromotesHiggsMass}; asymptoticSafetyCompletesBosonPredictions={asymptoticSafetyCompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = asymptoticSafetyHiggsSourceAuditPassed
    ? "asymptotic-safety-higgs-source-audit-external-quantum-gravity-boundary-not-promotion"
    : "asymptotic-safety-higgs-source-audit-review-required";

var result = new
{
    phaseId = "phase271-asymptotic-safety-higgs-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    asymptoticSafetyHiggsSourceAuditPassed,
    asymptoticSafetyGravityLeadPresent,
    asymptoticSafetyHiggsPredictionLeadPresent,
    targetInsideAsymptoticSafetyPredictionBand,
    asymptoticSafetyPromotesWzMasses,
    asymptoticSafetyPromotesHiggsMass,
    asymptoticSafetyCompletesBosonPredictions,
    asymptoticSafetyBoundary = new
    {
        asymptoticSafetyPredictedHiggsMassGeV,
        asymptoticSafetyPredictionUncertaintyGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        asymptoticSafetyPredictionPull,
        asymptoticSafetyRequiresGravityFixedPointSource,
        asymptoticSafetyRequiresPositiveQuarticAnomalousDimensionSource,
        asymptoticSafetyRequiresQuarticFixedPointBoundary,
        asymptoticSafetyRequiresNoIntermediateScalesAssumption,
        asymptoticSafetyRequiresSmPlusGravityMatterContentSource,
        asymptoticSafetyRequiresPlanckMatchingAndRgTransport,
        asymptoticSafetyRequiresTopYukawaAndAlphaSSource,
        asymptoticSafetyRequiresHiggsMassSchemeSource,
        asymptoticSafetyExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuAsymptoticSafetyFixedPointSourceFound,
        localGuGravityMatterBetaFunctionsFound,
        localGuQuarticAnomalousDimensionSourceFound,
        localGuQuarticFixedPointBoundaryFound,
        localGuNoIntermediateScaleTheoremFound,
        localGuPlanckMatchingRgTransportFound,
        localGuTopYukawaAlphaSSourceFound,
        localGuHiggsScalarSourceFound,
        localGuVevSourceFound,
        localGuObservedFieldExtractionFound,
        localGuWzMassMatrixSourceFound,
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
            higgsVacuumCriticalitySourceAuditPassed,
            vacuumCriticalityPromotesHiggsMass,
            vacuumCriticalityBoundaryNumericallyNearHiggsMass,
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
            "shaposhnikov-wetterich-asymptotic-safety-higgs-mass",
            "https://arxiv.org/abs/0912.0208",
            "Asymptotic safety plus a positive gravity-induced anomalous dimension for the Higgs quartic gives a fixed point at zero and an m_H near 126 GeV, assuming no intermediate scales."),
        new ExternalSource(
            "pawlowski-reichert-wetterich-yamada-higgs-potential-asqg",
            "https://journals.aps.org/prd/abstract/10.1103/PhysRevD.99.086010",
            "The Higgs quartic is argued to be irrelevant at the asymptotically safe quantum-gravity fixed point; predictions still rely on the quantum-gravity flow and SM approximation below the Planck scale."),
        new ExternalSource(
            "wetterich-effective-scalar-potential-asqg",
            "https://arxiv.org/abs/1911.06100",
            "Effective scalar-potential work strengthens arguments for a top/Higgs mass-ratio prediction but remains a quantum-gravity scaling-potential framework, not a GU source artifact."),
        new ExternalSource(
            "eichhorn-pauly-ray-dark-portal-asymptotic-safety",
            "https://arxiv.org/abs/2107.07949",
            "Adding a dark portal changes the predicted Higgs mass through extra parameters, showing the prediction depends on UV matter content and assumptions."),
    },
    localSearchFinding = "Repository search found vacuum-criticality/Planck-scale-near-zero-quartic diagnostics, but no GU-local asymptotic-safety fixed point, gravity-matter beta functions, quartic anomalous-dimension source, Planck matching, or observed-field extraction artifact.",
    checks,
    decision = asymptoticSafetyHiggsSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from asymptotic-safety quantum-gravity Higgs-mass literature. It is a serious and numerically close external UV-boundary lead, but the repository lacks a GU-local gravity fixed point, matter content theorem, quartic anomalous dimension, fixed-point boundary, no-intermediate-scale theorem, Planck matching/RG transport, top/Yukawa/alpha_s source, VEV source, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem."
        : "Review asymptotic-safety Higgs source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local asymptotic-safety or equivalent UV fixed-point theorem for the gravity-matter system.",
        "A GU-derived Higgs quartic anomalous dimension and fixed-point boundary with Planck matching and low-energy RG transport.",
        "A top/Yukawa/alpha_s source, no-intermediate-scale theorem, VEV/source-scale row, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase220Path = Phase220Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase248Path = Phase248Path,
        phase257Path = Phase257Path,
        phase264Path = Phase264Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "asymptotic_safety_higgs_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "asymptotic_safety_higgs_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.asymptoticSafetyHiggsSourceAuditPassed,
        result.asymptoticSafetyGravityLeadPresent,
        result.asymptoticSafetyHiggsPredictionLeadPresent,
        result.targetInsideAsymptoticSafetyPredictionBand,
        result.asymptoticSafetyPromotesWzMasses,
        result.asymptoticSafetyPromotesHiggsMass,
        result.asymptoticSafetyCompletesBosonPredictions,
        result.asymptoticSafetyBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"asymptoticSafetyHiggsSourceAuditPassed={asymptoticSafetyHiggsSourceAuditPassed}");
Console.WriteLine($"asymptoticSafetyGravityLeadPresent={asymptoticSafetyGravityLeadPresent}");
Console.WriteLine($"asymptoticSafetyHiggsPredictionLeadPresent={asymptoticSafetyHiggsPredictionLeadPresent}");
Console.WriteLine($"asymptoticSafetyPredictionPull={asymptoticSafetyPredictionPull:R}");
Console.WriteLine($"asymptoticSafetyPromotesWzMasses={asymptoticSafetyPromotesWzMasses}");
Console.WriteLine($"asymptoticSafetyPromotesHiggsMass={asymptoticSafetyPromotesHiggsMass}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");

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

record ExternalSource(string SourceId, string Url, string Finding);

record Check(string CheckId, bool Passed, string Detail);

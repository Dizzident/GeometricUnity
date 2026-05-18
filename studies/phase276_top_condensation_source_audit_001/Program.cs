using System.Text.Json;

const string DefaultOutputDir = "studies/phase276_top_condensation_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase270Path = "studies/phase270_composite_higgs_pngb_source_audit_001/output/composite_higgs_pngb_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE276_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase270 = JsonDocument.Parse(File.ReadAllText(Phase270Path));

var topCondensationLeadPresent = true;
var njlFourFermionLeadPresent = true;
var compositeTopHiggsLeadPresent = true;
var topSeesawLeadPresent = true;
var topcolorLeadPresent = true;
var higgsTargetGeV = 125.20;
var higgsTargetUncertaintyGeV = 0.11;
var topMassInputGeV = 172.52;
var simpleNjlCompositeHiggsMassGeV = 2.0 * topMassInputGeV;
var simpleNjlCompositeHiggsPull = Pull(simpleNjlCompositeHiggsMassGeV, 5.0, higgsTargetGeV, higgsTargetUncertaintyGeV);
var minimalTopCondensationOverpredictsHiggsMass = simpleNjlCompositeHiggsPull > 40.0;
var modernTopCondensationRequiresExtendedDynamics = true;
var modernTopCondensationRequiresAdditionalScalarsOrTopPartners = true;

var requiresGuFourFermionOperatorSource = true;
var requiresGuStrongTopcolorOrBindingSource = true;
var requiresCriticalCouplingGapEquationSource = true;
var requiresCompositenessCutoffSource = true;
var requiresTopCondensateOrderParameterSource = true;
var requiresTopYukawaAndMassSource = true;
var requiresBottomOrTopSeesawMixingSource = true;
var requiresLowEnergyRgTransport = true;
var requiresVevSource = true;
var requiresWzMassMatrixSource = true;
var requiresCompositeHiggsScalarSource = true;
var requiresObservedFieldExtraction = true;
var topCondensationExternalToGu = true;

var localGuFourFermionOperatorSourceFound = false;
var localGuStrongTopcolorOrBindingSourceFound = false;
var localGuCriticalCouplingGapEquationFound = false;
var localGuCompositenessCutoffSourceFound = false;
var localGuTopCondensateOrderParameterFound = false;
var localGuTopYukawaAndMassSourceFound = false;
var localGuBottomOrTopSeesawMixingSourceFound = false;
var localGuTopCondensationRgTransportFound = false;
var localGuVevSourceFound = false;
var localGuWzMassMatrixSourceFound = false;
var localGuCompositeHiggsScalarSourceFound = false;
var localGuObservedFieldExtractionFound = false;
var topCondensationPromotesWzMasses = false;
var topCondensationPromotesHiggsMass = false;
var topCondensationCompletesBosonPredictions = false;

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
var compositeHiggsPngbSourceAuditPassed = JsonBool(phase270.RootElement, "compositeHiggsPngbSourceAuditPassed") is true;
var compositeHiggsPromotesHiggsMass = JsonBool(phase270.RootElement, "compositeHiggsPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var topCondensationSourceAuditPassed =
    topCondensationLeadPresent
    && njlFourFermionLeadPresent
    && compositeTopHiggsLeadPresent
    && topSeesawLeadPresent
    && topcolorLeadPresent
    && minimalTopCondensationOverpredictsHiggsMass
    && modernTopCondensationRequiresExtendedDynamics
    && modernTopCondensationRequiresAdditionalScalarsOrTopPartners
    && requiresGuFourFermionOperatorSource
    && requiresGuStrongTopcolorOrBindingSource
    && requiresCriticalCouplingGapEquationSource
    && requiresCompositenessCutoffSource
    && requiresTopCondensateOrderParameterSource
    && requiresTopYukawaAndMassSource
    && requiresBottomOrTopSeesawMixingSource
    && requiresLowEnergyRgTransport
    && requiresVevSource
    && requiresWzMassMatrixSource
    && requiresCompositeHiggsScalarSource
    && requiresObservedFieldExtraction
    && topCondensationExternalToGu
    && !localGuFourFermionOperatorSourceFound
    && !localGuStrongTopcolorOrBindingSourceFound
    && !localGuCriticalCouplingGapEquationFound
    && !localGuCompositenessCutoffSourceFound
    && !localGuTopCondensateOrderParameterFound
    && !localGuTopYukawaAndMassSourceFound
    && !localGuBottomOrTopSeesawMixingSourceFound
    && !localGuTopCondensationRgTransportFound
    && !localGuVevSourceFound
    && !localGuWzMassMatrixSourceFound
    && !localGuCompositeHiggsScalarSourceFound
    && !localGuObservedFieldExtractionFound
    && !topCondensationPromotesWzMasses
    && !topCondensationPromotesHiggsMass
    && !topCondensationCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && compositeHiggsPngbSourceAuditPassed
    && !compositeHiggsPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "top-condensation-electroweak-breaking-lead-present",
        topCondensationLeadPresent
            && njlFourFermionLeadPresent
            && compositeTopHiggsLeadPresent
            && topSeesawLeadPresent
            && topcolorLeadPresent,
        $"topCondensationLeadPresent={topCondensationLeadPresent}; njlFourFermionLeadPresent={njlFourFermionLeadPresent}; compositeTopHiggsLeadPresent={compositeTopHiggsLeadPresent}; topSeesawLeadPresent={topSeesawLeadPresent}; topcolorLeadPresent={topcolorLeadPresent}"),
    new Check(
        "minimal-njl-top-condensation-overpredicts-higgs",
        minimalTopCondensationOverpredictsHiggsMass
            && modernTopCondensationRequiresExtendedDynamics
            && modernTopCondensationRequiresAdditionalScalarsOrTopPartners,
        $"simpleNjlCompositeHiggsMassGeV={simpleNjlCompositeHiggsMassGeV:R}; simpleNjlCompositeHiggsPull={simpleNjlCompositeHiggsPull:R}; modernTopCondensationRequiresExtendedDynamics={modernTopCondensationRequiresExtendedDynamics}; modernTopCondensationRequiresAdditionalScalarsOrTopPartners={modernTopCondensationRequiresAdditionalScalarsOrTopPartners}"),
    new Check(
        "top-condensation-input-contract-is-external-and-model-dependent",
        requiresGuFourFermionOperatorSource
            && requiresGuStrongTopcolorOrBindingSource
            && requiresCriticalCouplingGapEquationSource
            && requiresCompositenessCutoffSource
            && requiresTopCondensateOrderParameterSource
            && requiresTopYukawaAndMassSource
            && requiresBottomOrTopSeesawMixingSource
            && requiresLowEnergyRgTransport,
        $"fourFermionOperator={requiresGuFourFermionOperatorSource}; bindingSource={requiresGuStrongTopcolorOrBindingSource}; gapEquation={requiresCriticalCouplingGapEquationSource}; compositenessCutoff={requiresCompositenessCutoffSource}; condensateOrderParameter={requiresTopCondensateOrderParameterSource}; topYukawaMass={requiresTopYukawaAndMassSource}; topSeesawMixing={requiresBottomOrTopSeesawMixingSource}; rgTransport={requiresLowEnergyRgTransport}"),
    new Check(
        "no-local-gu-top-condensation-source-artifact",
        !localGuFourFermionOperatorSourceFound
            && !localGuStrongTopcolorOrBindingSourceFound
            && !localGuCriticalCouplingGapEquationFound
            && !localGuCompositenessCutoffSourceFound
            && !localGuTopCondensateOrderParameterFound
            && !localGuTopYukawaAndMassSourceFound
            && !localGuTopCondensationRgTransportFound
            && !localGuObservedFieldExtractionFound,
        $"fourFermionOperator={localGuFourFermionOperatorSourceFound}; bindingSource={localGuStrongTopcolorOrBindingSourceFound}; gapEquation={localGuCriticalCouplingGapEquationFound}; compositenessCutoff={localGuCompositenessCutoffSourceFound}; condensateOrderParameter={localGuTopCondensateOrderParameterFound}; topYukawaMass={localGuTopYukawaAndMassSourceFound}; rgTransport={localGuTopCondensationRgTransportFound}; observedFieldExtraction={localGuObservedFieldExtractionFound}"),
    new Check(
        "top-condensation-does-not-fill-gu-wz-or-higgs-source-contracts",
        !localGuVevSourceFound
            && !localGuWzMassMatrixSourceFound
            && !localGuCompositeHiggsScalarSourceFound
            && !topCondensationPromotesWzMasses
            && !topCondensationPromotesHiggsMass
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"localGuVevSourceFound={localGuVevSourceFound}; localGuWzMassMatrixSourceFound={localGuWzMassMatrixSourceFound}; localGuCompositeHiggsScalarSourceFound={localGuCompositeHiggsScalarSourceFound}; topCondensationPromotesWzMasses={topCondensationPromotesWzMasses}; topCondensationPromotesHiggsMass={topCondensationPromotesHiggsMass}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "neighboring-composite-higgs-route-remains-nonpromotional",
        compositeHiggsPngbSourceAuditPassed && !compositeHiggsPromotesHiggsMass,
        $"compositeHiggsPngbSourceAuditPassed={compositeHiggsPngbSourceAuditPassed}; compositeHiggsPromotesHiggsMass={compositeHiggsPromotesHiggsMass}"),
};

var terminalStatus = topCondensationSourceAuditPassed
    ? "top-condensation-source-audit-external-dynamical-breaking-model-not-promotion"
    : "top-condensation-source-audit-review-required";

var result = new
{
    phaseId = "phase276-top-condensation-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    topCondensationSourceAuditPassed,
    topCondensationLeadPresent,
    njlFourFermionLeadPresent,
    compositeTopHiggsLeadPresent,
    topSeesawLeadPresent,
    topcolorLeadPresent,
    topCondensationPromotesWzMasses,
    topCondensationPromotesHiggsMass,
    topCondensationCompletesBosonPredictions,
    topCondensationBoundary = new
    {
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        topMassInputGeV,
        simpleNjlCompositeHiggsMassGeV,
        simpleNjlCompositeHiggsPull,
        minimalTopCondensationOverpredictsHiggsMass,
        modernTopCondensationRequiresExtendedDynamics,
        modernTopCondensationRequiresAdditionalScalarsOrTopPartners,
        requiresGuFourFermionOperatorSource,
        requiresGuStrongTopcolorOrBindingSource,
        requiresCriticalCouplingGapEquationSource,
        requiresCompositenessCutoffSource,
        requiresTopCondensateOrderParameterSource,
        requiresTopYukawaAndMassSource,
        requiresBottomOrTopSeesawMixingSource,
        requiresLowEnergyRgTransport,
        requiresVevSource,
        requiresWzMassMatrixSource,
        requiresCompositeHiggsScalarSource,
        requiresObservedFieldExtraction,
        topCondensationExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuFourFermionOperatorSourceFound,
        localGuStrongTopcolorOrBindingSourceFound,
        localGuCriticalCouplingGapEquationFound,
        localGuCompositenessCutoffSourceFound,
        localGuTopCondensateOrderParameterFound,
        localGuTopYukawaAndMassSourceFound,
        localGuBottomOrTopSeesawMixingSourceFound,
        localGuTopCondensationRgTransportFound,
        localGuVevSourceFound,
        localGuWzMassMatrixSourceFound,
        localGuCompositeHiggsScalarSourceFound,
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
        phase270 = new
        {
            compositeHiggsPngbSourceAuditPassed,
            compositeHiggsPromotesHiggsMass,
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
            "wells-top-condensation-electroweak-higgs",
            "https://arxiv.org/abs/hep-ph/9612292",
            "Reviews top-quark condensation as an NJL mechanism triggered by a strongly coupled gauge sector; natural hierarchy constraints make top condensation a spectator unless another sector completes electroweak breaking."),
        new ExternalSource(
            "dobrescu-hill-top-condensation-seesaw",
            "https://arxiv.org/abs/hep-ph/9712319",
            "Uses topcolor interactions plus a top seesaw with an additional isosinglet quark to drive electroweak symmetry breaking while keeping the top mass acceptable."),
        new ExternalSource(
            "chivukula-dobrescu-georgi-hill-top-seesaw",
            "https://arxiv.org/abs/hep-ph/9809470",
            "Develops quark-condensation seesaw models with composite scalars and effective-potential spectra."),
        new ExternalSource(
            "osipov-hiller-blin-palanca-moreira-sampaio-top-condensation",
            "https://arxiv.org/abs/1906.09579",
            "Studies an effective four-Fermi top-bottom condensation model that can move beyond the simple NJL m_H=2m_t overestimate, but requires a specified model and spectrum."),
    },
    localSearchFinding = "Repository search found pNGB composite-Higgs diagnostics, but no GU-local topcolor or NJL four-fermion operator, critical coupling/gap equation, compositeness cutoff, top-condensate order parameter, top-seesaw mixing, low-energy RG transport, VEV source, W/Z mass matrix, composite Higgs scalar source, or observed-field extraction artifact.",
    checks,
    decision = topCondensationSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from top-condensation literature. The route is a serious dynamical electroweak-breaking lead, but it requires a model-specific strong/four-fermion sector, critical gap equation, compositeness scale, top condensate, topcolor or top-seesaw dynamics, RG transport, VEV source, W/Z mass matrix, composite Higgs scalar source, and observed-field extraction theorem that the repository does not contain."
        : "Review top-condensation source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local topcolor/NJL or equivalent four-fermion binding source with critical-coupling and gap-equation derivation.",
        "A GU-derived compositeness cutoff, top-condensate order parameter, top/Yukawa source, top-seesaw or extended-sector mixing, and low-energy RG/threshold transport.",
        "A GU VEV/source-scale theorem, W/Z mass matrix, composite Higgs scalar source, and observed-field extraction theorem.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase220Path = Phase220Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase248Path = Phase248Path,
        phase257Path = Phase257Path,
        phase270Path = Phase270Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "top_condensation_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "top_condensation_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.topCondensationSourceAuditPassed,
        result.topCondensationLeadPresent,
        result.njlFourFermionLeadPresent,
        result.compositeTopHiggsLeadPresent,
        result.topSeesawLeadPresent,
        result.topcolorLeadPresent,
        result.topCondensationPromotesWzMasses,
        result.topCondensationPromotesHiggsMass,
        result.topCondensationCompletesBosonPredictions,
        result.topCondensationBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"topCondensationSourceAuditPassed={topCondensationSourceAuditPassed}");
Console.WriteLine($"topCondensationLeadPresent={topCondensationLeadPresent}");
Console.WriteLine($"njlFourFermionLeadPresent={njlFourFermionLeadPresent}");
Console.WriteLine($"compositeTopHiggsLeadPresent={compositeTopHiggsLeadPresent}");
Console.WriteLine($"topSeesawLeadPresent={topSeesawLeadPresent}");
Console.WriteLine($"topcolorLeadPresent={topcolorLeadPresent}");
Console.WriteLine($"simpleNjlCompositeHiggsMassGeV={simpleNjlCompositeHiggsMassGeV}");
Console.WriteLine($"simpleNjlCompositeHiggsPull={simpleNjlCompositeHiggsPull}");
Console.WriteLine($"topCondensationPromotesWzMasses={topCondensationPromotesWzMasses}");
Console.WriteLine($"topCondensationPromotesHiggsMass={topCondensationPromotesHiggsMass}");
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

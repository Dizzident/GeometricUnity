using System.Text.Json;

const string DefaultOutputDir = "studies/phase269_coleman_weinberg_scale_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase264Path = "studies/phase264_higgs_vacuum_criticality_source_audit_001/output/higgs_vacuum_criticality_source_audit_summary.json";
const string Phase268Path = "studies/phase268_spectral_action_boson_source_audit_001/output/spectral_action_boson_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE269_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase264 = JsonDocument.Parse(File.ReadAllText(Phase264Path));
using var phase268 = JsonDocument.Parse(File.ReadAllText(Phase268Path));

var colemanWeinbergScaleLeadPresent = true;
var radiativeSymmetryBreakingLeadPresent = true;
var dimensionalTransmutationLeadPresent = true;
var colemanWeinbergCanGenerateScaleInExternalQft = true;
var standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut = true;
var colemanWeinbergRequiresClassicallyMasslessScalarPotential = true;
var colemanWeinbergRequiresRenormalizationScaleBoundary = true;
var colemanWeinbergRequiresBetaFunctionAndThresholdSource = true;
var colemanWeinbergRequiresScalarSectorSource = true;
var colemanWeinbergRequiresFlatDirectionOrQuarticBoundary = true;
var colemanWeinbergRequiresLowEnergyRgTransport = true;
var colemanWeinbergExternalToGu = true;

var localGuColemanWeinbergPotentialSourceFound = false;
var localGuColemanWeinbergRenormalizationScaleSourceFound = false;
var localGuColemanWeinbergBetaFunctionSourceFound = false;
var localGuColemanWeinbergFlatDirectionSourceFound = false;
var localGuColemanWeinbergQuarticBoundarySourceFound = false;
var localGuColemanWeinbergTopYukawaThresholdSourceFound = false;
var localGuColemanWeinbergVevSourceFound = false;
var localGuColemanWeinbergObservedFieldExtractionFound = false;
var localGuColemanWeinbergWzMassMatrixSourceFound = false;
var localGuColemanWeinbergHiggsScalarSourceFound = false;
var colemanWeinbergPromotesWzMasses = false;
var colemanWeinbergPromotesHiggsMass = false;
var colemanWeinbergCompletesBosonPredictions = false;

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
var vacuumCriticalityPromotesHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityPromotesHiggsMass") is true;
var spectralActionPromotesWzMasses = JsonBool(phase268.RootElement, "spectralActionPromotesWzMasses") is true;
var spectralActionPromotesHiggsMass = JsonBool(phase268.RootElement, "spectralActionPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var colemanWeinbergScaleSourceAuditPassed =
    colemanWeinbergScaleLeadPresent
    && radiativeSymmetryBreakingLeadPresent
    && dimensionalTransmutationLeadPresent
    && colemanWeinbergCanGenerateScaleInExternalQft
    && standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut
    && colemanWeinbergRequiresClassicallyMasslessScalarPotential
    && colemanWeinbergRequiresRenormalizationScaleBoundary
    && colemanWeinbergRequiresBetaFunctionAndThresholdSource
    && colemanWeinbergRequiresScalarSectorSource
    && colemanWeinbergRequiresFlatDirectionOrQuarticBoundary
    && colemanWeinbergRequiresLowEnergyRgTransport
    && colemanWeinbergExternalToGu
    && !localGuColemanWeinbergPotentialSourceFound
    && !localGuColemanWeinbergRenormalizationScaleSourceFound
    && !localGuColemanWeinbergBetaFunctionSourceFound
    && !localGuColemanWeinbergFlatDirectionSourceFound
    && !localGuColemanWeinbergQuarticBoundarySourceFound
    && !localGuColemanWeinbergTopYukawaThresholdSourceFound
    && !localGuColemanWeinbergVevSourceFound
    && !localGuColemanWeinbergObservedFieldExtractionFound
    && !localGuColemanWeinbergWzMassMatrixSourceFound
    && !localGuColemanWeinbergHiggsScalarSourceFound
    && !colemanWeinbergPromotesWzMasses
    && !colemanWeinbergPromotesHiggsMass
    && !colemanWeinbergCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && !vacuumCriticalityPromotesHiggsMass
    && !spectralActionPromotesWzMasses
    && !spectralActionPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "coleman-weinberg-route-is-radiative-scale-generation-lead",
        colemanWeinbergScaleLeadPresent
            && radiativeSymmetryBreakingLeadPresent
            && dimensionalTransmutationLeadPresent
            && colemanWeinbergCanGenerateScaleInExternalQft,
        $"colemanWeinbergScaleLeadPresent={colemanWeinbergScaleLeadPresent}; radiativeSymmetryBreakingLeadPresent={radiativeSymmetryBreakingLeadPresent}; dimensionalTransmutationLeadPresent={dimensionalTransmutationLeadPresent}; canGenerateScaleInExternalQft={colemanWeinbergCanGenerateScaleInExternalQft}"),
    new Check(
        "minimal-standard-model-route-is-not-promotable",
        standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut
            && !colemanWeinbergPromotesHiggsMass
            && !colemanWeinbergCompletesBosonPredictions,
        $"standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut={standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut}; colemanWeinbergPromotesHiggsMass={colemanWeinbergPromotesHiggsMass}; colemanWeinbergCompletesBosonPredictions={colemanWeinbergCompletesBosonPredictions}"),
    new Check(
        "coleman-weinberg-input-contract-not-gu-sourced",
        colemanWeinbergRequiresClassicallyMasslessScalarPotential
            && colemanWeinbergRequiresRenormalizationScaleBoundary
            && colemanWeinbergRequiresBetaFunctionAndThresholdSource
            && colemanWeinbergRequiresScalarSectorSource
            && colemanWeinbergRequiresFlatDirectionOrQuarticBoundary
            && colemanWeinbergRequiresLowEnergyRgTransport,
        $"masslessPotential={colemanWeinbergRequiresClassicallyMasslessScalarPotential}; renormalizationScaleBoundary={colemanWeinbergRequiresRenormalizationScaleBoundary}; betaFunctionAndThresholdSource={colemanWeinbergRequiresBetaFunctionAndThresholdSource}; scalarSectorSource={colemanWeinbergRequiresScalarSectorSource}; flatDirectionOrQuarticBoundary={colemanWeinbergRequiresFlatDirectionOrQuarticBoundary}; lowEnergyRgTransport={colemanWeinbergRequiresLowEnergyRgTransport}"),
    new Check(
        "no-local-gu-coleman-weinberg-source-artifact",
        !localGuColemanWeinbergPotentialSourceFound
            && !localGuColemanWeinbergRenormalizationScaleSourceFound
            && !localGuColemanWeinbergBetaFunctionSourceFound
            && !localGuColemanWeinbergFlatDirectionSourceFound
            && !localGuColemanWeinbergQuarticBoundarySourceFound
            && !localGuColemanWeinbergVevSourceFound
            && !localGuColemanWeinbergObservedFieldExtractionFound,
        $"potentialSource={localGuColemanWeinbergPotentialSourceFound}; renormalizationScaleSource={localGuColemanWeinbergRenormalizationScaleSourceFound}; betaFunctionSource={localGuColemanWeinbergBetaFunctionSourceFound}; flatDirectionSource={localGuColemanWeinbergFlatDirectionSourceFound}; quarticBoundarySource={localGuColemanWeinbergQuarticBoundarySourceFound}; vevSource={localGuColemanWeinbergVevSourceFound}; observedFieldExtraction={localGuColemanWeinbergObservedFieldExtractionFound}"),
    new Check(
        "current-repo-scale-rg-and-field-extraction-blockers-still-active",
        obstructionAuditPassed
            && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
            && !lowEnergyRgTransportSourcePromotable
            && newHiggsScalarSourceStillRequired
            && !currentImplementationCanFillObservedFieldExtractionContract,
        $"obstructionAuditPassed={obstructionAuditPassed}; obstructionKind={obstructionKind}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}; currentImplementationCanFillObservedFieldExtractionContract={currentImplementationCanFillObservedFieldExtractionContract}"),
    new Check(
        "coleman-weinberg-does-not-complete-boson-predictions",
        !colemanWeinbergPromotesWzMasses
            && !colemanWeinbergPromotesHiggsMass
            && !colemanWeinbergCompletesBosonPredictions
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"colemanWeinbergPromotesWzMasses={colemanWeinbergPromotesWzMasses}; colemanWeinbergPromotesHiggsMass={colemanWeinbergPromotesHiggsMass}; colemanWeinbergCompletesBosonPredictions={colemanWeinbergCompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = colemanWeinbergScaleSourceAuditPassed
    ? "coleman-weinberg-scale-source-audit-external-radiative-route-not-promotion"
    : "coleman-weinberg-scale-source-audit-review-required";

var result = new
{
    phaseId = "phase269-coleman-weinberg-scale-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    colemanWeinbergScaleSourceAuditPassed,
    colemanWeinbergScaleLeadPresent,
    radiativeSymmetryBreakingLeadPresent,
    dimensionalTransmutationLeadPresent,
    colemanWeinbergPromotesWzMasses,
    colemanWeinbergPromotesHiggsMass,
    colemanWeinbergCompletesBosonPredictions,
    colemanWeinbergBoundary = new
    {
        colemanWeinbergCanGenerateScaleInExternalQft,
        standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut,
        colemanWeinbergRequiresClassicallyMasslessScalarPotential,
        colemanWeinbergRequiresRenormalizationScaleBoundary,
        colemanWeinbergRequiresBetaFunctionAndThresholdSource,
        colemanWeinbergRequiresScalarSectorSource,
        colemanWeinbergRequiresFlatDirectionOrQuarticBoundary,
        colemanWeinbergRequiresLowEnergyRgTransport,
        colemanWeinbergExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuColemanWeinbergPotentialSourceFound,
        localGuColemanWeinbergRenormalizationScaleSourceFound,
        localGuColemanWeinbergBetaFunctionSourceFound,
        localGuColemanWeinbergFlatDirectionSourceFound,
        localGuColemanWeinbergQuarticBoundarySourceFound,
        localGuColemanWeinbergTopYukawaThresholdSourceFound,
        localGuColemanWeinbergVevSourceFound,
        localGuColemanWeinbergObservedFieldExtractionFound,
        localGuColemanWeinbergWzMassMatrixSourceFound,
        localGuColemanWeinbergHiggsScalarSourceFound,
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
            vacuumCriticalityPromotesHiggsMass,
        },
        phase268 = new
        {
            spectralActionPromotesWzMasses,
            spectralActionPromotesHiggsMass,
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
            "coleman-weinberg-original-radiative-breaking",
            "https://journals.aps.org/prd/abstract/10.1103/PhysRevD.7.1888",
            "Coleman and Weinberg show that radiative corrections can produce spontaneous symmetry breaking in classically massless scalar electrodynamics and compute mass ratios in that model."),
        new ExternalSource(
            "gildener-weinberg-flat-direction-method",
            "https://journals.aps.org/prd/abstract/10.1103/PhysRevD.13.3333",
            "Gildener and Weinberg generalize radiative symmetry breaking to theories with multiple massless weakly coupled elementary scalar fields and flat directions."),
        new ExternalSource(
            "next-to-minimal-coleman-weinberg-model",
            "https://arxiv.org/abs/hep-ph/9604278",
            "Hempfling records that the minimal Standard Model Coleman-Weinberg version is phenomenologically ruled out and introduces extra singlet/U(1) structure to make the route viable."),
        new ExternalSource(
            "minimal-b-l-coleman-weinberg-model",
            "https://arxiv.org/abs/0909.0128",
            "Iso, Okada, and Orikasa use a B-L extension with classical conformal invariance to realize Coleman-Weinberg-type electroweak breaking, again requiring extra model structure."),
    },
    checks,
    decision = colemanWeinbergScaleSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from Coleman-Weinberg/radiative symmetry-breaking literature. It is a serious dimensional-transmutation lead, but the repository lacks a GU-local massless-potential source, renormalization-scale boundary, beta-function/threshold source, scalar-sector/flat-direction source, VEV source, W/Z mass-matrix source, Higgs scalar source, and observed-field extraction theorem."
        : "Review Coleman-Weinberg scale-source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local Coleman-Weinberg effective-potential or equivalent radiative scale theorem.",
        "A renormalization-scale/boundary source plus beta-function and threshold derivation tied to GU fields.",
        "A scalar-sector flat-direction/quartic boundary source, electroweak VEV source, observed-field extraction theorem, and W/Z/H source rows.",
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
        phase268Path = Phase268Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "coleman_weinberg_scale_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "coleman_weinberg_scale_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.colemanWeinbergScaleSourceAuditPassed,
        result.colemanWeinbergScaleLeadPresent,
        result.radiativeSymmetryBreakingLeadPresent,
        result.dimensionalTransmutationLeadPresent,
        result.colemanWeinbergPromotesWzMasses,
        result.colemanWeinbergPromotesHiggsMass,
        result.colemanWeinbergCompletesBosonPredictions,
        result.colemanWeinbergBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"colemanWeinbergScaleSourceAuditPassed={colemanWeinbergScaleSourceAuditPassed}");
Console.WriteLine($"colemanWeinbergScaleLeadPresent={colemanWeinbergScaleLeadPresent}");
Console.WriteLine($"radiativeSymmetryBreakingLeadPresent={radiativeSymmetryBreakingLeadPresent}");
Console.WriteLine($"dimensionalTransmutationLeadPresent={dimensionalTransmutationLeadPresent}");
Console.WriteLine($"standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut={standardModelColemanWeinbergMinimalVersionPhenomenologicallyRuledOut}");
Console.WriteLine($"colemanWeinbergPromotesWzMasses={colemanWeinbergPromotesWzMasses}");
Console.WriteLine($"colemanWeinbergPromotesHiggsMass={colemanWeinbergPromotesHiggsMass}");
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

record ExternalSource(string SourceId, string Url, string Finding);

record Check(string CheckId, bool Passed, string Detail);

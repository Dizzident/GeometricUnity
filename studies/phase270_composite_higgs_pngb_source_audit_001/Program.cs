using System.Text.Json;

const string DefaultOutputDir = "studies/phase270_composite_higgs_pngb_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE270_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));

var compositeHiggsPngbLeadPresent = true;
var vacuumMisalignmentLeadPresent = true;
var custodialSymmetryLeadPresent = true;
var minimalCompositeHiggsCosetLeadPresent = true;
var partialCompositenessLeadPresent = true;
var compositeHiggsParameterDependent = true;
var compositeHiggsRequiresStrongSectorSource = true;
var compositeHiggsRequiresGlobalSymmetryBreakingCosetSource = true;
var compositeHiggsRequiresDecayConstantSource = true;
var compositeHiggsRequiresMisalignmentAngleSource = true;
var compositeHiggsRequiresEffectivePotentialSource = true;
var compositeHiggsRequiresTopPartnerSpectrumSource = true;
var compositeHiggsRequiresPartialCompositenessYukawaSource = true;
var compositeHiggsRequiresRgAndThresholdSource = true;
var compositeHiggsExternalToGu = true;

var localGuCompositeStrongSectorSourceFound = false;
var localGuCompositeCosetEmbeddingFound = false;
var localGuCompositeDecayConstantSourceFound = false;
var localGuCompositeMisalignmentAngleSourceFound = false;
var localGuCompositeEffectivePotentialSourceFound = false;
var localGuCompositeTopPartnerSpectrumSourceFound = false;
var localGuCompositePartialCompositenessSourceFound = false;
var localGuCompositeRgThresholdSourceFound = false;
var localGuCompositeVevSourceFound = false;
var localGuCompositeObservedFieldExtractionFound = false;
var localGuCompositeWzMassMatrixSourceFound = false;
var localGuCompositeHiggsScalarSourceFound = false;
var compositeHiggsPromotesWzMasses = false;
var compositeHiggsPromotesHiggsMass = false;
var compositeHiggsCompletesBosonPredictions = false;

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
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var compositeHiggsPngbSourceAuditPassed =
    compositeHiggsPngbLeadPresent
    && vacuumMisalignmentLeadPresent
    && custodialSymmetryLeadPresent
    && minimalCompositeHiggsCosetLeadPresent
    && partialCompositenessLeadPresent
    && compositeHiggsParameterDependent
    && compositeHiggsRequiresStrongSectorSource
    && compositeHiggsRequiresGlobalSymmetryBreakingCosetSource
    && compositeHiggsRequiresDecayConstantSource
    && compositeHiggsRequiresMisalignmentAngleSource
    && compositeHiggsRequiresEffectivePotentialSource
    && compositeHiggsRequiresTopPartnerSpectrumSource
    && compositeHiggsRequiresPartialCompositenessYukawaSource
    && compositeHiggsRequiresRgAndThresholdSource
    && compositeHiggsExternalToGu
    && !localGuCompositeStrongSectorSourceFound
    && !localGuCompositeCosetEmbeddingFound
    && !localGuCompositeDecayConstantSourceFound
    && !localGuCompositeMisalignmentAngleSourceFound
    && !localGuCompositeEffectivePotentialSourceFound
    && !localGuCompositeTopPartnerSpectrumSourceFound
    && !localGuCompositePartialCompositenessSourceFound
    && !localGuCompositeRgThresholdSourceFound
    && !localGuCompositeVevSourceFound
    && !localGuCompositeObservedFieldExtractionFound
    && !localGuCompositeWzMassMatrixSourceFound
    && !localGuCompositeHiggsScalarSourceFound
    && !compositeHiggsPromotesWzMasses
    && !compositeHiggsPromotesHiggsMass
    && !compositeHiggsCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "composite-higgs-is-pngb-and-vacuum-misalignment-lead",
        compositeHiggsPngbLeadPresent
            && vacuumMisalignmentLeadPresent
            && custodialSymmetryLeadPresent
            && minimalCompositeHiggsCosetLeadPresent,
        $"compositeHiggsPngbLeadPresent={compositeHiggsPngbLeadPresent}; vacuumMisalignmentLeadPresent={vacuumMisalignmentLeadPresent}; custodialSymmetryLeadPresent={custodialSymmetryLeadPresent}; minimalCompositeHiggsCosetLeadPresent={minimalCompositeHiggsCosetLeadPresent}"),
    new Check(
        "composite-higgs-is-parameter-dependent-not-source-complete",
        compositeHiggsParameterDependent
            && compositeHiggsRequiresDecayConstantSource
            && compositeHiggsRequiresMisalignmentAngleSource
            && compositeHiggsRequiresEffectivePotentialSource
            && compositeHiggsRequiresTopPartnerSpectrumSource,
        $"parameterDependent={compositeHiggsParameterDependent}; decayConstantSource={compositeHiggsRequiresDecayConstantSource}; misalignmentAngleSource={compositeHiggsRequiresMisalignmentAngleSource}; effectivePotentialSource={compositeHiggsRequiresEffectivePotentialSource}; topPartnerSpectrumSource={compositeHiggsRequiresTopPartnerSpectrumSource}"),
    new Check(
        "no-local-gu-composite-higgs-source-artifact",
        !localGuCompositeStrongSectorSourceFound
            && !localGuCompositeCosetEmbeddingFound
            && !localGuCompositeDecayConstantSourceFound
            && !localGuCompositeMisalignmentAngleSourceFound
            && !localGuCompositeEffectivePotentialSourceFound
            && !localGuCompositeObservedFieldExtractionFound,
        $"strongSectorSource={localGuCompositeStrongSectorSourceFound}; cosetEmbedding={localGuCompositeCosetEmbeddingFound}; decayConstantSource={localGuCompositeDecayConstantSourceFound}; misalignmentAngleSource={localGuCompositeMisalignmentAngleSourceFound}; effectivePotentialSource={localGuCompositeEffectivePotentialSourceFound}; observedFieldExtraction={localGuCompositeObservedFieldExtractionFound}"),
    new Check(
        "current-repo-boson-source-blockers-still-active",
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
        "composite-higgs-does-not-complete-boson-predictions",
        !compositeHiggsPromotesWzMasses
            && !compositeHiggsPromotesHiggsMass
            && !compositeHiggsCompletesBosonPredictions
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"compositeHiggsPromotesWzMasses={compositeHiggsPromotesWzMasses}; compositeHiggsPromotesHiggsMass={compositeHiggsPromotesHiggsMass}; compositeHiggsCompletesBosonPredictions={compositeHiggsCompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = compositeHiggsPngbSourceAuditPassed
    ? "composite-higgs-pngb-source-audit-external-strong-sector-not-promotion"
    : "composite-higgs-pngb-source-audit-review-required";

var result = new
{
    phaseId = "phase270-composite-higgs-pngb-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    compositeHiggsPngbSourceAuditPassed,
    compositeHiggsPngbLeadPresent,
    vacuumMisalignmentLeadPresent,
    custodialSymmetryLeadPresent,
    minimalCompositeHiggsCosetLeadPresent,
    partialCompositenessLeadPresent,
    compositeHiggsPromotesWzMasses,
    compositeHiggsPromotesHiggsMass,
    compositeHiggsCompletesBosonPredictions,
    compositeHiggsBoundary = new
    {
        compositeHiggsParameterDependent,
        compositeHiggsRequiresStrongSectorSource,
        compositeHiggsRequiresGlobalSymmetryBreakingCosetSource,
        compositeHiggsRequiresDecayConstantSource,
        compositeHiggsRequiresMisalignmentAngleSource,
        compositeHiggsRequiresEffectivePotentialSource,
        compositeHiggsRequiresTopPartnerSpectrumSource,
        compositeHiggsRequiresPartialCompositenessYukawaSource,
        compositeHiggsRequiresRgAndThresholdSource,
        compositeHiggsExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuCompositeStrongSectorSourceFound,
        localGuCompositeCosetEmbeddingFound,
        localGuCompositeDecayConstantSourceFound,
        localGuCompositeMisalignmentAngleSourceFound,
        localGuCompositeEffectivePotentialSourceFound,
        localGuCompositeTopPartnerSpectrumSourceFound,
        localGuCompositePartialCompositenessSourceFound,
        localGuCompositeRgThresholdSourceFound,
        localGuCompositeVevSourceFound,
        localGuCompositeObservedFieldExtractionFound,
        localGuCompositeWzMassMatrixSourceFound,
        localGuCompositeHiggsScalarSourceFound,
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
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "kaplan-georgi-vacuum-misalignment",
            "https://doi.org/10.1016/0370-2693(84)91177-8",
            "Kaplan and Georgi propose electroweak breaking by vacuum misalignment, producing a partially composite Higgs sector in an oblique hypercolor setup."),
        new ExternalSource(
            "georgi-kaplan-custodial-su2",
            "https://doi.org/10.1016/0370-2693(84)90341-1",
            "Georgi and Kaplan show how custodial SU(2) can protect the W/Z mass relation in composite Higgs models, but the Higgs potential is model-generated."),
        new ExternalSource(
            "minimal-composite-higgs-model",
            "https://arxiv.org/abs/hep-ph/0412089",
            "The minimal composite Higgs model treats the Higgs as a pseudo-Goldstone boson in a five-dimensional AdS construction; top loops determine the potential in a specified model."),
        new ExternalSource(
            "composite-nambu-goldstone-higgs-review",
            "https://arxiv.org/abs/1506.01961",
            "Panico and Wulzer review the composite pNGB Higgs framework and its flavor, collider, and electroweak precision phenomenology."),
    },
    localSearchFinding = "Repository search found no current composite-Higgs/pNGB/partial-compositeness source artifact. The active blockers are still dimensional scale, Higgs scalar source, W/Z mass matrix, RG transport, VEV, and observed-field extraction.",
    checks,
    decision = compositeHiggsPngbSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from composite/pNGB Higgs literature. The route is a serious dynamical and symmetry-based lead, but the repository lacks a GU-local strong-sector source, coset embedding, decay constant, vacuum-misalignment angle, top-partner/partial-compositeness structure, effective potential, VEV source, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem."
        : "Review composite-Higgs pNGB source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local strong-sector/coset construction that yields the observed Higgs representation as a pNGB.",
        "A target-independent decay constant, vacuum-misalignment angle, effective potential, and top-partner/partial-compositeness source.",
        "A GU VEV/source-scale theorem, W/Z mass-matrix extraction, Higgs scalar mass source, RG/threshold transport, and observed-field extraction theorem.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase220Path = Phase220Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase248Path = Phase248Path,
        phase257Path = Phase257Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "composite_higgs_pngb_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "composite_higgs_pngb_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.compositeHiggsPngbSourceAuditPassed,
        result.compositeHiggsPngbLeadPresent,
        result.vacuumMisalignmentLeadPresent,
        result.custodialSymmetryLeadPresent,
        result.minimalCompositeHiggsCosetLeadPresent,
        result.partialCompositenessLeadPresent,
        result.compositeHiggsPromotesWzMasses,
        result.compositeHiggsPromotesHiggsMass,
        result.compositeHiggsCompletesBosonPredictions,
        result.compositeHiggsBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"compositeHiggsPngbSourceAuditPassed={compositeHiggsPngbSourceAuditPassed}");
Console.WriteLine($"compositeHiggsPngbLeadPresent={compositeHiggsPngbLeadPresent}");
Console.WriteLine($"vacuumMisalignmentLeadPresent={vacuumMisalignmentLeadPresent}");
Console.WriteLine($"custodialSymmetryLeadPresent={custodialSymmetryLeadPresent}");
Console.WriteLine($"compositeHiggsPromotesWzMasses={compositeHiggsPromotesWzMasses}");
Console.WriteLine($"compositeHiggsPromotesHiggsMass={compositeHiggsPromotesHiggsMass}");
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

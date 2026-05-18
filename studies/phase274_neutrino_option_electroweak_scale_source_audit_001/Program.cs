using System.Text.Json;

const string DefaultOutputDir = "studies/phase274_neutrino_option_electroweak_scale_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE274_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));

var neutrinoOptionLeadPresent = true;
var radiativeSeesawHiggsPotentialLeadPresent = true;
var simultaneousElectroweakAndNeutrinoMassScaleLeadPresent = true;
var expectedMajoranaScaleMinPeV = 10.0;
var expectedMajoranaScaleMaxPeV = 500.0;
var expectedSeesawYukawaMinLog10 = -6.0;
var expectedSeesawYukawaMaxLog10 = -4.5;
var requiresMajoranaScaleSource = true;
var requiresSeesawYukawaMatrixSource = true;
var requiresNeutrinoMassAndMixingInput = true;
var requiresCasasIbarraOrEquivalentParameterChoice = true;
var requiresZeroTreeLevelHiggsPotentialBoundary = true;
var requiresThresholdMatching = true;
var requiresLowEnergyRgTransport = true;
var requiresUvCompletionOrMajoranaMassOrigin = true;
var requiresAdditionalSingletScalarsInConformalRealization = true;
var noGoConstraintsOnSimpleUvCompletionPresent = true;
var neutrinoOptionExternalToGu = true;

var localGuMajoranaScaleSourceFound = false;
var localGuRightHandedNeutrinoSectorSourceFound = false;
var localGuSeesawYukawaSourceFound = false;
var localGuNeutrinoMixingSourceFound = false;
var localGuZeroTreeHiggsPotentialBoundaryFound = false;
var localGuThresholdMatchingSourceFound = false;
var localGuNeutrinoOptionRgTransportFound = false;
var localGuVevSourceFound = false;
var localGuWzMassMatrixSourceFound = false;
var localGuHiggsScalarSourceFound = false;
var localGuObservedFieldExtractionFound = false;
var neutrinoOptionPromotesWzMasses = false;
var neutrinoOptionPromotesHiggsMass = false;
var neutrinoOptionCompletesBosonPredictions = false;

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

var neutrinoOptionElectroweakScaleSourceAuditPassed =
    neutrinoOptionLeadPresent
    && radiativeSeesawHiggsPotentialLeadPresent
    && simultaneousElectroweakAndNeutrinoMassScaleLeadPresent
    && requiresMajoranaScaleSource
    && requiresSeesawYukawaMatrixSource
    && requiresNeutrinoMassAndMixingInput
    && requiresCasasIbarraOrEquivalentParameterChoice
    && requiresZeroTreeLevelHiggsPotentialBoundary
    && requiresThresholdMatching
    && requiresLowEnergyRgTransport
    && requiresUvCompletionOrMajoranaMassOrigin
    && requiresAdditionalSingletScalarsInConformalRealization
    && noGoConstraintsOnSimpleUvCompletionPresent
    && neutrinoOptionExternalToGu
    && !localGuMajoranaScaleSourceFound
    && !localGuRightHandedNeutrinoSectorSourceFound
    && !localGuSeesawYukawaSourceFound
    && !localGuNeutrinoMixingSourceFound
    && !localGuZeroTreeHiggsPotentialBoundaryFound
    && !localGuThresholdMatchingSourceFound
    && !localGuNeutrinoOptionRgTransportFound
    && !localGuVevSourceFound
    && !localGuWzMassMatrixSourceFound
    && !localGuHiggsScalarSourceFound
    && !localGuObservedFieldExtractionFound
    && !neutrinoOptionPromotesWzMasses
    && !neutrinoOptionPromotesHiggsMass
    && !neutrinoOptionCompletesBosonPredictions
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
        "neutrino-option-electroweak-scale-lead-present",
        neutrinoOptionLeadPresent
            && radiativeSeesawHiggsPotentialLeadPresent
            && simultaneousElectroweakAndNeutrinoMassScaleLeadPresent,
        $"neutrinoOptionLeadPresent={neutrinoOptionLeadPresent}; radiativeSeesawHiggsPotentialLeadPresent={radiativeSeesawHiggsPotentialLeadPresent}; simultaneousElectroweakAndNeutrinoMassScaleLeadPresent={simultaneousElectroweakAndNeutrinoMassScaleLeadPresent}"),
    new Check(
        "neutrino-option-input-contract-is-external-and-parameterized",
        requiresMajoranaScaleSource
            && requiresSeesawYukawaMatrixSource
            && requiresNeutrinoMassAndMixingInput
            && requiresCasasIbarraOrEquivalentParameterChoice
            && requiresZeroTreeLevelHiggsPotentialBoundary
            && requiresThresholdMatching
            && requiresLowEnergyRgTransport
            && requiresUvCompletionOrMajoranaMassOrigin,
        $"majoranaScalePeVRange={expectedMajoranaScaleMinPeV:R}-{expectedMajoranaScaleMaxPeV:R}; seesawYukawaLog10Range={expectedSeesawYukawaMinLog10:R}..{expectedSeesawYukawaMaxLog10:R}; thresholdMatching={requiresThresholdMatching}; rgTransport={requiresLowEnergyRgTransport}; uvCompletionOrMajoranaOrigin={requiresUvCompletionOrMajoranaMassOrigin}"),
    new Check(
        "no-local-gu-neutrino-option-source-artifact",
        !localGuMajoranaScaleSourceFound
            && !localGuRightHandedNeutrinoSectorSourceFound
            && !localGuSeesawYukawaSourceFound
            && !localGuZeroTreeHiggsPotentialBoundaryFound
            && !localGuThresholdMatchingSourceFound
            && !localGuNeutrinoOptionRgTransportFound
            && !localGuObservedFieldExtractionFound,
        $"majoranaScale={localGuMajoranaScaleSourceFound}; rightHandedNeutrinoSector={localGuRightHandedNeutrinoSectorSourceFound}; seesawYukawa={localGuSeesawYukawaSourceFound}; zeroTreeHiggsBoundary={localGuZeroTreeHiggsPotentialBoundaryFound}; thresholdMatching={localGuThresholdMatchingSourceFound}; rgTransport={localGuNeutrinoOptionRgTransportFound}; observedFieldExtraction={localGuObservedFieldExtractionFound}"),
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
        "neutrino-option-does-not-complete-boson-predictions",
        !neutrinoOptionPromotesWzMasses
            && !neutrinoOptionPromotesHiggsMass
            && !neutrinoOptionCompletesBosonPredictions
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"neutrinoOptionPromotesWzMasses={neutrinoOptionPromotesWzMasses}; neutrinoOptionPromotesHiggsMass={neutrinoOptionPromotesHiggsMass}; neutrinoOptionCompletesBosonPredictions={neutrinoOptionCompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = neutrinoOptionElectroweakScaleSourceAuditPassed
    ? "neutrino-option-electroweak-scale-source-audit-external-seesaw-threshold-not-promotion"
    : "neutrino-option-electroweak-scale-source-audit-review-required";

var result = new
{
    phaseId = "phase274-neutrino-option-electroweak-scale-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    neutrinoOptionElectroweakScaleSourceAuditPassed,
    neutrinoOptionLeadPresent,
    radiativeSeesawHiggsPotentialLeadPresent,
    simultaneousElectroweakAndNeutrinoMassScaleLeadPresent,
    neutrinoOptionPromotesWzMasses,
    neutrinoOptionPromotesHiggsMass,
    neutrinoOptionCompletesBosonPredictions,
    neutrinoOptionBoundary = new
    {
        expectedMajoranaScaleMinPeV,
        expectedMajoranaScaleMaxPeV,
        expectedSeesawYukawaMinLog10,
        expectedSeesawYukawaMaxLog10,
        requiresMajoranaScaleSource,
        requiresSeesawYukawaMatrixSource,
        requiresNeutrinoMassAndMixingInput,
        requiresCasasIbarraOrEquivalentParameterChoice,
        requiresZeroTreeLevelHiggsPotentialBoundary,
        requiresThresholdMatching,
        requiresLowEnergyRgTransport,
        requiresUvCompletionOrMajoranaMassOrigin,
        requiresAdditionalSingletScalarsInConformalRealization,
        noGoConstraintsOnSimpleUvCompletionPresent,
        neutrinoOptionExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuMajoranaScaleSourceFound,
        localGuRightHandedNeutrinoSectorSourceFound,
        localGuSeesawYukawaSourceFound,
        localGuNeutrinoMixingSourceFound,
        localGuZeroTreeHiggsPotentialBoundaryFound,
        localGuThresholdMatchingSourceFound,
        localGuNeutrinoOptionRgTransportFound,
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
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "brivio-trott-neutrino-option-prl",
            "https://arxiv.org/abs/1703.10924",
            "The minimal seesaw can radiatively generate the Higgs potential and derived electroweak scale from an underlying Majorana scale; expected singlet states sit around 10-500 PeV for small seesaw Yukawa couplings."),
        new ExternalSource(
            "brivio-trott-examining-neutrino-option",
            "https://arxiv.org/abs/1809.03450",
            "A systematic SMEFT matching and numerical study keeps the neutrino option viable but dependent on seesaw matching, neutrino data, and Standard Model particle masses."),
        new ExternalSource(
            "brdar-emonds-helmboldt-lindner-conformal-realization",
            "https://arxiv.org/abs/1807.11490",
            "A conformal realization requires additional real scalar singlets plus right-handed neutrinos, with RG-triggered Majorana masses and constrained parameter space."),
        new ExternalSource(
            "brivio-talbert-trott-no-go-neutrino-option-uv",
            "https://arxiv.org/abs/2010.15428",
            "No-go constraints limit simple perturbative UV generation of the Majorana masses required by the neutrino option."),
    },
    localSearchFinding = "Repository search found B-L and Coleman-Weinberg/classical-conformal diagnostics, but no GU-local right-handed-neutrino sector, Majorana scale, seesaw Yukawa matrix, zero-tree-Higgs-potential boundary, threshold-matching package, low-energy RG transport, W/Z mass matrix, Higgs scalar source, or observed-field extraction artifact.",
    checks,
    decision = neutrinoOptionElectroweakScaleSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from neutrino-option literature. The route is a real external radiative electroweak-scale lead, but it requires a target-independent Majorana scale, seesaw Yukawa matrix, neutrino-mixing input, zero-tree-Higgs-potential boundary, threshold matching, RG transport, UV completion or Majorana-mass origin, VEV source, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem that the repository does not contain."
        : "Review neutrino-option electroweak-scale source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local right-handed-neutrino or equivalent singlet sector with target-independent Majorana scale source.",
        "A GU-derived seesaw Yukawa matrix, neutrino mixing source, zero-tree-Higgs-potential boundary, threshold matching, and low-energy RG transport.",
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
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "neutrino_option_electroweak_scale_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "neutrino_option_electroweak_scale_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.neutrinoOptionElectroweakScaleSourceAuditPassed,
        result.neutrinoOptionLeadPresent,
        result.radiativeSeesawHiggsPotentialLeadPresent,
        result.simultaneousElectroweakAndNeutrinoMassScaleLeadPresent,
        result.neutrinoOptionPromotesWzMasses,
        result.neutrinoOptionPromotesHiggsMass,
        result.neutrinoOptionCompletesBosonPredictions,
        result.neutrinoOptionBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"neutrinoOptionElectroweakScaleSourceAuditPassed={neutrinoOptionElectroweakScaleSourceAuditPassed}");
Console.WriteLine($"neutrinoOptionLeadPresent={neutrinoOptionLeadPresent}");
Console.WriteLine($"radiativeSeesawHiggsPotentialLeadPresent={radiativeSeesawHiggsPotentialLeadPresent}");
Console.WriteLine($"simultaneousElectroweakAndNeutrinoMassScaleLeadPresent={simultaneousElectroweakAndNeutrinoMassScaleLeadPresent}");
Console.WriteLine($"neutrinoOptionPromotesWzMasses={neutrinoOptionPromotesWzMasses}");
Console.WriteLine($"neutrinoOptionPromotesHiggsMass={neutrinoOptionPromotesHiggsMass}");
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

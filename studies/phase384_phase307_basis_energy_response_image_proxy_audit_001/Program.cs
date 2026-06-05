using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase384_phase307_basis_energy_response_image_proxy_audit_001/output";
const string Phase27Path = "studies/phase27_charge_sector_convention_001/identity_features_with_charge_sectors.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit.json";
const string Phase307SummaryPath = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase379Path = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase381Path = "studies/phase381_phase302_307_response_image_selector_compatibility_audit_001/output/phase302_307_response_image_selector_compatibility_audit_summary.json";
const string Phase382Path = "studies/phase382_response_image_observed_projection_requirement_audit_001/output/response_image_observed_projection_requirement_audit_summary.json";
const string Phase383Path = "studies/phase383_phase307_suppressed_axis_counterfactual_selector_audit_001/output/phase307_suppressed_axis_counterfactual_selector_audit_summary.json";
const int ExpectedWzMissingFieldCount = 15;
const double ConventionalLowSuppressedBasisEnergyThreshold = 0.10;

var outputDir = Environment.GetEnvironmentVariable("PHASE384_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase27 = JsonDocument.Parse(File.ReadAllText(Phase27Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase307Summary = JsonDocument.Parse(File.ReadAllText(Phase307SummaryPath));
using var phase379 = JsonDocument.Parse(File.ReadAllText(Phase379Path));
using var phase381 = JsonDocument.Parse(File.ReadAllText(Phase381Path));
using var phase382 = JsonDocument.Parse(File.ReadAllText(Phase382Path));
using var phase383 = JsonDocument.Parse(File.ReadAllText(Phase383Path));

int suppressedGaugeAxis = JsonInt(phase379.RootElement, "stableSuppressedGaugeAxis") ?? -1;
double phase379MaxSuppressedProjectorFraction = JsonDouble(phase379.RootElement, "maxSuppressedGaugeAxisProjectorFraction") ?? double.NaN;
var phase379DominantGaugeAxes = JsonIntArray(phase379.RootElement, "phase379DominantGaugeAxes");
if (phase379DominantGaugeAxes.Length == 0 && phase379.RootElement.TryGetProperty("carrier", out var carrier))
{
    int gaugeDimension = JsonInt(carrier, "gaugeDimension") ?? 3;
    phase379DominantGaugeAxes = Enumerable.Range(0, gaugeDimension).Where(axis => axis != suppressedGaugeAxis).ToArray();
}

var candidateFeatures = phase27.RootElement.GetProperty("featureRecords")
    .EnumerateArray()
    .Select(record => new CandidateBasisEnergy(
        NormalizeCandidateId(RequiredString(record, "sourceCandidateId")),
        RequiredString(record, "sourceCandidateId"),
        JsonString(record, "chargeSector"),
        JsonString(record, "algebraBasisSector"),
        JsonInt(record, "dominantBasisIndex") ?? -1,
        JsonDoubleArray(record, "basisEnergyFractions")))
    .OrderBy(candidate => CandidateOrdinal(candidate.CandidateId))
    .ThenBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
    .ToArray();
var candidateById = candidateFeatures.ToDictionary(candidate => candidate.CandidateId, StringComparer.Ordinal);

var sourceDefinitionAudits = phase307.RootElement.GetProperty("sourceDefinitions")
    .EnumerateArray()
    .Select(definition =>
    {
        var wCandidateIds = JsonStringArray(definition, "wCandidateIds");
        var zCandidateIds = JsonStringArray(definition, "zCandidateIds");
        var wFractions = AverageBasisFractions(wCandidateIds);
        var zFractions = AverageBasisFractions(zCandidateIds);
        double wSuppressed = AxisFraction(wFractions, suppressedGaugeAxis);
        double zSuppressed = AxisFraction(zFractions, suppressedGaugeAxis);
        return new SourceDefinitionBasisEnergyAudit(
            RequiredString(definition, "definitionId"),
            wCandidateIds,
            zCandidateIds,
            wFractions,
            zFractions,
            wSuppressed,
            DominantPairFraction(wFractions, phase379DominantGaugeAxes),
            zSuppressed,
            DominantPairFraction(zFractions, phase379DominantGaugeAxes),
            wSuppressed <= phase379MaxSuppressedProjectorFraction,
            wSuppressed <= ConventionalLowSuppressedBasisEnergyThreshold,
            JsonString(definition, "description"));
    })
    .OrderBy(definition => definition.DefinitionId, StringComparer.Ordinal)
    .ToArray();
var definitionById = sourceDefinitionAudits.ToDictionary(definition => definition.DefinitionId, StringComparer.Ordinal);

var selectionLawAudits = phase307.RootElement.GetProperty("targetIndependentSelectionLawAssessments")
    .EnumerateArray()
    .Select(selectionLaw =>
    {
        var selectedAssessment = selectionLaw.GetProperty("selectedAssessment");
        string wDefinitionId = RequiredString(selectedAssessment.GetProperty("wRow"), "definitionId");
        string zDefinitionId = RequiredString(selectedAssessment.GetProperty("zRow"), "definitionId");
        var wDefinition = definitionById[wDefinitionId];
        var zDefinition = definitionById[zDefinitionId];
        return new SelectionLawBasisEnergyAudit(
            RequiredString(selectionLaw, "lawId"),
            JsonBool(selectionLaw, "selectedRawStableCommonPassed") is true,
            JsonBool(selectionLaw, "selectedP302ScaledStableCommonPassed") is true,
            RequiredString(selectedAssessment, "assessmentId"),
            wDefinitionId,
            zDefinitionId,
            wDefinition.WSuppressedBasisEnergyFraction,
            wDefinition.WDominantPairBasisEnergyFraction,
            zDefinition.ZSuppressedBasisEnergyFraction,
            zDefinition.ZDominantPairBasisEnergyFraction,
            wDefinition.WSuppressedBasisEnergyAtOrBelowPhase379ProjectorFraction,
            wDefinition.WLowSuppressedBasisEnergyProxyPassed);
    })
    .OrderBy(selection => selection.LawId, StringComparer.Ordinal)
    .ToArray();

var lowestSuppressedWDefinitions = sourceDefinitionAudits
    .OrderBy(definition => definition.WSuppressedBasisEnergyFraction)
    .ThenBy(definition => definition.DefinitionId, StringComparer.Ordinal)
    .Take(10)
    .ToArray();
var selectedP302ScaledStableCommonSelection = selectionLawAudits.SingleOrDefault(selection => selection.SelectedP302ScaledStableCommonPassed);
double minWSuppressedBasisEnergyFraction = sourceDefinitionAudits.Min(definition => definition.WSuppressedBasisEnergyFraction);
double maxWDominantPairBasisEnergyFraction = sourceDefinitionAudits.Max(definition => definition.WDominantPairBasisEnergyFraction);
int wDefinitionsAtOrBelowPhase379SuppressedProjectorFractionCount = sourceDefinitionAudits.Count(definition => definition.WSuppressedBasisEnergyAtOrBelowPhase379ProjectorFraction);
int wDefinitionsLowSuppressedBasisEnergyProxyCount = sourceDefinitionAudits.Count(definition => definition.WLowSuppressedBasisEnergyProxyPassed);
int selectorsAtOrBelowPhase379SuppressedProjectorFractionCount = selectionLawAudits.Count(selection => selection.SelectedWSuppressedBasisEnergyAtOrBelowPhase379ProjectorFraction);
int selectorsLowSuppressedBasisEnergyProxyCount = selectionLawAudits.Count(selection => selection.SelectedWLowSuppressedBasisEnergyProxyPassed);

bool phase307Materialized = JsonBool(phase307Summary.RootElement, "targetIndependentDecoupledWzRowSelectionLawAuditPassed") is true
    && JsonBool(phase307Summary.RootElement, "targetObservablesUsedForConstruction") is false
    && JsonInt(phase307Summary.RootElement, "definitionCount") == sourceDefinitionAudits.Length
    && JsonInt(phase307Summary.RootElement, "selectionLawCount") == selectionLawAudits.Length;
bool phase379ContextMaterialized = JsonBool(phase379.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true
    && JsonBool(phase379.RootElement, "targetBlindConstruction") is true
    && JsonBool(phase379.RootElement, "physicalTargetsConsultedForConstruction") is false
    && suppressedGaugeAxis == 1
    && phase379DominantGaugeAxes.SequenceEqual(new[] { 0, 2 })
    && JsonBool(phase379.RootElement, "canFillPhase201WzContract") is false;
bool phase381382383BlockerMaterialized =
    JsonBool(phase381.RootElement, "phase302307ResponseImageSelectorCompatibilityAuditPassed") is true
    && JsonBool(phase381.RootElement, "responseImageSidecarConflictPresent") is true
    && JsonBool(phase382.RootElement, "responseImageObservedProjectionRequirementAuditPassed") is true
    && JsonBool(phase382.RootElement, "observedCarrierAxisNamespaceSeparationMapPresent") is false
    && JsonBool(phase383.RootElement, "phase307SuppressedAxisCounterfactualSelectorAuditPassed") is true
    && JsonBool(phase383.RootElement, "noPredeclaredSelectorAvoidsSuppressedAxis") is true;
bool phase27BasisEnergyMaterialized = candidateFeatures.Length == 12
    && candidateFeatures.All(candidate => candidate.BasisEnergyFractions.Count == 3)
    && candidateFeatures.All(candidate => Math.Abs(candidate.BasisEnergyFractions.Sum() - 1.0) < 1e-9);
bool noWDefinitionHasLowSuppressedBasisEnergyProxy = wDefinitionsLowSuppressedBasisEnergyProxyCount == 0
    && wDefinitionsAtOrBelowPhase379SuppressedProjectorFractionCount == 0
    && minWSuppressedBasisEnergyFraction > ConventionalLowSuppressedBasisEnergyThreshold;
bool noPredeclaredSelectorHasLowSuppressedBasisEnergyProxy = selectorsLowSuppressedBasisEnergyProxyCount == 0
    && selectorsAtOrBelowPhase379SuppressedProjectorFractionCount == 0
    && selectionLawAudits.All(selection => selection.SelectedWSuppressedBasisEnergyFraction > ConventionalLowSuppressedBasisEnergyThreshold);
bool selectedP302ScaledStableCommonStillBasisEnergyConflicted = selectedP302ScaledStableCommonSelection is not null
    && selectedP302ScaledStableCommonSelection.SelectedWDefinitionId == "charged-ladder-all-axis-neutral-coherent-plus"
    && selectedP302ScaledStableCommonSelection.SelectedWSuppressedBasisEnergyFraction > ConventionalLowSuppressedBasisEnergyThreshold
    && selectedP302ScaledStableCommonSelection.SelectedWLowSuppressedBasisEnergyProxyPassed is false;

bool sourceContractApplicationAllowed = false;
bool canFillPhase201WzContract = false;
bool canFillPhase201HiggsContract = false;
bool canFillPhase256ObservedFieldExtractionContract = false;
bool routePromotesWzMasses = false;
bool routePromotesHiggsMass = false;
bool routeCompletesBosonPredictions = false;
bool phase201TemplateMutated = false;
int fieldsAppliedToPhase201TemplateCount = 0;
int acceptedContractFieldCount = 0;
int blockedContractFieldCount = ExpectedWzMissingFieldCount;
int phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? ExpectedWzMissingFieldCount;

var checks = new[]
{
    Check(
        "phase27-basis-energy-and-phase307-selector-space-materialized",
        phase27BasisEnergyMaterialized && phase307Materialized,
        $"candidateFeatureCount={candidateFeatures.Length}; sourceDefinitionCount={sourceDefinitionAudits.Length}; selectionLawCount={selectionLawAudits.Length}; phase307Materialized={phase307Materialized}"),
    Check(
        "phase379-381-382-383-response-image-blocker-preserved",
        phase379ContextMaterialized && phase381382383BlockerMaterialized,
        $"phase379Context={phase379ContextMaterialized}; suppressedAxis={suppressedGaugeAxis}; dominantAxes={string.Join(",", phase379DominantGaugeAxes)}; phase381382383Blocker={phase381382383BlockerMaterialized}"),
    Check(
        "no-phase307-w-definition-has-low-suppressed-basis-energy-proxy",
        noWDefinitionHasLowSuppressedBasisEnergyProxy,
        $"minWSuppressedBasisEnergyFraction={minWSuppressedBasisEnergyFraction:R}; lowThreshold={ConventionalLowSuppressedBasisEnergyThreshold:R}; phase379MaxSuppressedProjectorFraction={phase379MaxSuppressedProjectorFraction:R}; wDefinitionsLowSuppressedBasisEnergyProxyCount={wDefinitionsLowSuppressedBasisEnergyProxyCount}; wDefinitionsAtOrBelowPhase379SuppressedProjectorFractionCount={wDefinitionsAtOrBelowPhase379SuppressedProjectorFractionCount}"),
    Check(
        "no-phase307-predeclared-selector-has-low-suppressed-basis-energy-proxy",
        noPredeclaredSelectorHasLowSuppressedBasisEnergyProxy && selectedP302ScaledStableCommonStillBasisEnergyConflicted,
        $"selectorsLowSuppressedBasisEnergyProxyCount={selectorsLowSuppressedBasisEnergyProxyCount}; selectorsAtOrBelowPhase379SuppressedProjectorFractionCount={selectorsAtOrBelowPhase379SuppressedProjectorFractionCount}; selectedP302ScaledStableCommonWDefinition={selectedP302ScaledStableCommonSelection?.SelectedWDefinitionId}; selectedP302ScaledStableCommonWSuppressedBasisEnergyFraction={selectedP302ScaledStableCommonSelection?.SelectedWSuppressedBasisEnergyFraction:R}"),
    Check(
        "basis-energy-proxy-does-not-supply-observed-projection-or-contract-fill",
        !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0
            && blockedContractFieldCount == ExpectedWzMissingFieldCount
            && phase213WzMissingFieldCount == ExpectedWzMissingFieldCount,
        $"sourceContractApplicationAllowed={sourceContractApplicationAllowed}; canFillPhase201WzContract={canFillPhase201WzContract}; canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}; acceptedContractFieldCount={acceptedContractFieldCount}; blockedContractFieldCount={blockedContractFieldCount}; phase213WzMissingFieldCount={phase213WzMissingFieldCount}"),
};

bool phase307BasisEnergyResponseImageProxyAuditPassed = checks.All(check => check.Passed);
string terminalStatus = phase307BasisEnergyResponseImageProxyAuditPassed
    ? "phase307-basis-energy-response-image-proxy-audit-no-suppressed-axis-escape"
    : "phase307-basis-energy-response-image-proxy-audit-review-required";
string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase384-phase307-basis-energy-response-image-proxy-audit-v1",
    $"phase27Path={Phase27Path}",
    $"phase307Path={Phase307Path}",
    $"phase379Path={Phase379Path}",
    $"phase383Path={Phase383Path}",
    $"suppressedGaugeAxis={suppressedGaugeAxis}",
    $"phase379MaxSuppressedProjectorFraction={phase379MaxSuppressedProjectorFraction:R}",
    "physicalTargetsConsultedForConstruction=false"));
string decision = phase307BasisEnergyResponseImageProxyAuditPassed
    ? $"Phase27 basis-energy metadata does not provide a finer suppressed-axis escape for Phase307. Every current W source definition has suppressed-axis basis energy above {ConventionalLowSuppressedBasisEnergyThreshold:R}; the minimum is {minWSuppressedBasisEnergyFraction:R}, and the P302-scaled stable common selector has {selectedP302ScaledStableCommonSelection?.SelectedWSuppressedBasisEnergyFraction:R}. This proxy is not an observed projection map or a source-row theorem, so it cannot fill Phase201."
    : "Review the Phase307 basis-energy response-image proxy audit before using Phase27 basis-energy fractions as sidecar evidence.";

var result = new
{
    phaseId = "phase384-phase307-basis-energy-response-image-proxy-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    phase307BasisEnergyResponseImageProxyAuditPassed,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    targetBlindConstructionHash,
    applicationSubjectKind = "phase27-basis-energy-proxy-for-phase307-selector-space-under-phase379-response-image-sidecar",
    proxyBoundary = "Phase27 basis-energy fractions are target-blind internal SU(2) metadata, not observed electroweak projection rows and not a physical response-image projector on W/Z fields.",
    thresholds = new
    {
        phase379MaxSuppressedProjectorFraction,
        conventionalLowSuppressedBasisEnergyThreshold = ConventionalLowSuppressedBasisEnergyThreshold,
    },
    phase27BasisEnergy = new
    {
        phase27Path = Phase27Path,
        phase27BasisEnergyMaterialized,
        candidateFeatures,
    },
    phase307SelectorSpace = new
    {
        phase307Path = Phase307Path,
        phase307SummaryPath = Phase307SummaryPath,
        phase307Materialized,
        sourceDefinitionCount = sourceDefinitionAudits.Length,
        selectionLawCount = selectionLawAudits.Length,
    },
    responseImageContext = new
    {
        phase379Path = Phase379Path,
        phase381Path = Phase381Path,
        phase382Path = Phase382Path,
        phase383Path = Phase383Path,
        suppressedGaugeAxis,
        dominantGaugeAxes = phase379DominantGaugeAxes,
        phase379ContextMaterialized,
        phase381382383BlockerMaterialized,
    },
    sourceDefinitionBasisEnergyAudit = new
    {
        sourceDefinitionAudits,
        lowestSuppressedWDefinitions,
        minWSuppressedBasisEnergyFraction,
        maxWDominantPairBasisEnergyFraction,
        wDefinitionsAtOrBelowPhase379SuppressedProjectorFractionCount,
        wDefinitionsLowSuppressedBasisEnergyProxyCount,
        noWDefinitionHasLowSuppressedBasisEnergyProxy,
    },
    selectionLawBasisEnergyAudit = new
    {
        selectionLawAudits,
        selectedP302ScaledStableCommonSelection,
        selectorsAtOrBelowPhase379SuppressedProjectorFractionCount,
        selectorsLowSuppressedBasisEnergyProxyCount,
        noPredeclaredSelectorHasLowSuppressedBasisEnergyProxy,
        selectedP302ScaledStableCommonStillBasisEnergyConflicted,
    },
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    blockedContractFieldCount,
    phase213WzMissingFieldCount,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    checks,
    sourceEvidence = new
    {
        phase27Path = Phase27Path,
        phase213Path = Phase213Path,
        phase307Path = Phase307Path,
        phase307SummaryPath = Phase307SummaryPath,
        phase379Path = Phase379Path,
        phase381Path = Phase381Path,
        phase382Path = Phase382Path,
        phase383Path = Phase383Path,
    },
    nextRequiredArtifact = new[]
    {
        "A GU-native observed electroweak projection map with source-lineage eligibility.",
        "A theorem deriving a W source-definition family compatible with the Phase379 response image.",
        "A theorem explaining why the Phase307 charged-ladder W row physically requires substantial suppressed-axis content.",
        "Separate W and Z Phase201 source rows with source-lineage ids, raw/common/target gates, stability sidecars, derivation ids, and GeV normalization.",
    },
    decision,
};

File.WriteAllText(
    Path.Combine(outputDir, "phase307_basis_energy_response_image_proxy_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase307_basis_energy_response_image_proxy_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase307BasisEnergyResponseImageProxyAuditPassed,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.applicationSubjectKind,
        result.proxyBoundary,
        phase27BasisEnergyMaterialized,
        phase307Materialized,
        phase379ContextMaterialized,
        phase381382383BlockerMaterialized,
        suppressedGaugeAxis,
        phase379DominantGaugeAxes,
        phase379MaxSuppressedProjectorFraction,
        conventionalLowSuppressedBasisEnergyThreshold = ConventionalLowSuppressedBasisEnergyThreshold,
        sourceDefinitionCount = sourceDefinitionAudits.Length,
        selectionLawCount = selectionLawAudits.Length,
        minWSuppressedBasisEnergyFraction,
        maxWDominantPairBasisEnergyFraction,
        wDefinitionsAtOrBelowPhase379SuppressedProjectorFractionCount,
        wDefinitionsLowSuppressedBasisEnergyProxyCount,
        noWDefinitionHasLowSuppressedBasisEnergyProxy,
        selectorsAtOrBelowPhase379SuppressedProjectorFractionCount,
        selectorsLowSuppressedBasisEnergyProxyCount,
        noPredeclaredSelectorHasLowSuppressedBasisEnergyProxy,
        selectedP302ScaledStableCommonSelectionLawId = selectedP302ScaledStableCommonSelection?.LawId,
        selectedP302ScaledStableCommonWDefinitionId = selectedP302ScaledStableCommonSelection?.SelectedWDefinitionId,
        selectedP302ScaledStableCommonWSuppressedBasisEnergyFraction = selectedP302ScaledStableCommonSelection?.SelectedWSuppressedBasisEnergyFraction,
        selectedP302ScaledStableCommonWDominantPairBasisEnergyFraction = selectedP302ScaledStableCommonSelection?.SelectedWDominantPairBasisEnergyFraction,
        selectedP302ScaledStableCommonStillBasisEnergyConflicted,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.phase201TemplateMutated,
        result.fieldsAppliedToPhase201TemplateCount,
        result.acceptedContractFieldCount,
        result.blockedContractFieldCount,
        result.phase213WzMissingFieldCount,
        result.phase201FieldsDefensiblyFilled,
        result.checks,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"phase307BasisEnergyResponseImageProxyAuditPassed={phase307BasisEnergyResponseImageProxyAuditPassed}");
Console.WriteLine($"sourceDefinitionCount={sourceDefinitionAudits.Length}");
Console.WriteLine($"selectionLawCount={selectionLawAudits.Length}");
Console.WriteLine($"suppressedGaugeAxis={suppressedGaugeAxis}");
Console.WriteLine($"phase379MaxSuppressedProjectorFraction={phase379MaxSuppressedProjectorFraction:R}");
Console.WriteLine($"minWSuppressedBasisEnergyFraction={minWSuppressedBasisEnergyFraction:R}");
Console.WriteLine($"wDefinitionsLowSuppressedBasisEnergyProxyCount={wDefinitionsLowSuppressedBasisEnergyProxyCount}");
Console.WriteLine($"selectorsLowSuppressedBasisEnergyProxyCount={selectorsLowSuppressedBasisEnergyProxyCount}");
Console.WriteLine($"selectedP302ScaledStableCommonSelectionLawId={selectedP302ScaledStableCommonSelection?.LawId}");
Console.WriteLine($"selectedP302ScaledStableCommonWSuppressedBasisEnergyFraction={selectedP302ScaledStableCommonSelection?.SelectedWSuppressedBasisEnergyFraction:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

IReadOnlyList<double> AverageBasisFractions(IReadOnlyList<string> candidateIds)
{
    if (candidateIds.Count == 0)
        return Array.Empty<double>();

    var sums = new double[3];
    foreach (var candidateId in candidateIds)
    {
        if (!candidateById.TryGetValue(candidateId, out var candidate))
            throw new InvalidDataException($"Missing Phase27 basis-energy feature for {candidateId}.");
        if (candidate.BasisEnergyFractions.Count != 3)
            throw new InvalidDataException($"Expected three basis-energy fractions for {candidateId}.");
        for (int axis = 0; axis < sums.Length; axis++)
            sums[axis] += candidate.BasisEnergyFractions[axis];
    }
    return sums.Select(sum => sum / candidateIds.Count).ToArray();
}

static double AxisFraction(IReadOnlyList<double> fractions, int axis) =>
    axis >= 0 && axis < fractions.Count ? fractions[axis] : double.NaN;

static double DominantPairFraction(IReadOnlyList<double> fractions, IReadOnlyList<int> dominantAxes) =>
    dominantAxes.Where(axis => axis >= 0 && axis < fractions.Count).Sum(axis => fractions[axis]);

static Check Check(string checkId, bool passed, string details) => new(checkId, passed, details);

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray()
        : Array.Empty<string>();

static int[] JsonIntArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out _))
            .Select(item => item.GetInt32())
            .ToArray()
        : Array.Empty<int>();

static double[] JsonDoubleArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Number && item.TryGetDouble(out _))
            .Select(item => item.GetDouble())
            .ToArray()
        : Array.Empty<double>();

static string NormalizeCandidateId(string sourceCandidateId) =>
    sourceCandidateId.StartsWith("phase12-", StringComparison.Ordinal)
        ? sourceCandidateId["phase12-".Length..]
        : sourceCandidateId;

static int CandidateOrdinal(string candidateId)
{
    var marker = "candidate-";
    var index = candidateId.IndexOf(marker, StringComparison.Ordinal);
    return index >= 0 && int.TryParse(candidateId[(index + marker.Length)..], out var value) ? value : int.MaxValue;
}

static string HashText(string text)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed record Check(string CheckId, bool Passed, string Details);
sealed record CandidateBasisEnergy(
    string CandidateId,
    string SourceCandidateId,
    string? ChargeSector,
    string? AlgebraBasisSector,
    int DominantBasisIndex,
    IReadOnlyList<double> BasisEnergyFractions);
sealed record SourceDefinitionBasisEnergyAudit(
    string DefinitionId,
    IReadOnlyList<string> WCandidateIds,
    IReadOnlyList<string> ZCandidateIds,
    IReadOnlyList<double> WBasisEnergyFractions,
    IReadOnlyList<double> ZBasisEnergyFractions,
    double WSuppressedBasisEnergyFraction,
    double WDominantPairBasisEnergyFraction,
    double ZSuppressedBasisEnergyFraction,
    double ZDominantPairBasisEnergyFraction,
    bool WSuppressedBasisEnergyAtOrBelowPhase379ProjectorFraction,
    bool WLowSuppressedBasisEnergyProxyPassed,
    string? Description);
sealed record SelectionLawBasisEnergyAudit(
    string LawId,
    bool SelectedRawStableCommonPassed,
    bool SelectedP302ScaledStableCommonPassed,
    string SelectedAssessmentId,
    string SelectedWDefinitionId,
    string SelectedZDefinitionId,
    double SelectedWSuppressedBasisEnergyFraction,
    double SelectedWDominantPairBasisEnergyFraction,
    double SelectedZSuppressedBasisEnergyFraction,
    double SelectedZDominantPairBasisEnergyFraction,
    bool SelectedWSuppressedBasisEnergyAtOrBelowPhase379ProjectorFraction,
    bool SelectedWLowSuppressedBasisEnergyProxyPassed);

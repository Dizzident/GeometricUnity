using System.Text.Json;

const string DefaultOutputDir = "studies/phase268_spectral_action_boson_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";
const string Phase267Path = "studies/phase267_completion_revision_direct_bridge_source_audit_001/output/completion_revision_direct_bridge_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE268_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase247 = JsonDocument.Parse(File.ReadAllText(Phase247Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));
using var phase267 = JsonDocument.Parse(File.ReadAllText(Phase267Path));

var spectralActionGeometricLeadPresent = true;
var spectralActionHighScaleBoundaryLeadPresent = true;
var originalSpectralHiggsMassRangeGeV = new[] { 160.0, 180.0 };
var originalSpectralHiggsMassMidpointGeV = 170.0;
var lowHiggsCompatibilityRequiresSingletOrExtendedScalar = true;
var spectralActionRequiresFiniteNoncommutativeGeometry = true;
var spectralActionRequiresCutoffScaleBoundary = true;
var spectralActionRequiresRgTransportToLowEnergy = true;
var spectralActionRequiresYukawaAndMajoranaInputs = true;
var spectralActionUsesModelSpecificSpectralTriple = true;
var spectralActionExternalToGu = true;

var localGuSpectralTripleArtifactFound = false;
var localGuFiniteAlgebraMappingFound = false;
var localGuSpectralActionCutoffSourceFound = false;
var localGuSpectralBoundaryConditionSourceFound = false;
var localGuSpectralRgTransportSourceFound = false;
var localGuSpectralYukawaMajoranaSourceFound = false;
var localGuSpectralObservedFieldExtractionFound = false;
var localGuSpectralVevSourceFound = false;
var localGuSpectralHiggsSingletSourceFound = false;
var spectralActionPromotesWzMasses = false;
var spectralActionPromotesHiggsMass = false;
var spectralActionCompletesBosonPredictions = false;

var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var newDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var higgsScalarSourceRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var newHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var currentImplementationCanFillObservedFieldExtractionContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;
var gaugeHiggsBoundaryPromotesHiggsMass = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryPromotesHiggsMass") is true;
var latestCompletionPromotesWzMasses = JsonBool(phase267.RootElement, "latestCompletionPromotesWzMasses") is true;
var latestCompletionPromotesHiggsMass = JsonBool(phase267.RootElement, "latestCompletionPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var spectralActionBosonSourceAuditPassed =
    spectralActionGeometricLeadPresent
    && spectralActionHighScaleBoundaryLeadPresent
    && lowHiggsCompatibilityRequiresSingletOrExtendedScalar
    && spectralActionRequiresFiniteNoncommutativeGeometry
    && spectralActionRequiresCutoffScaleBoundary
    && spectralActionRequiresRgTransportToLowEnergy
    && spectralActionRequiresYukawaAndMajoranaInputs
    && spectralActionUsesModelSpecificSpectralTriple
    && spectralActionExternalToGu
    && !localGuSpectralTripleArtifactFound
    && !localGuFiniteAlgebraMappingFound
    && !localGuSpectralActionCutoffSourceFound
    && !localGuSpectralBoundaryConditionSourceFound
    && !localGuSpectralRgTransportSourceFound
    && !localGuSpectralYukawaMajoranaSourceFound
    && !localGuSpectralObservedFieldExtractionFound
    && !localGuSpectralVevSourceFound
    && !localGuSpectralHiggsSingletSourceFound
    && !spectralActionPromotesWzMasses
    && !spectralActionPromotesHiggsMass
    && !spectralActionCompletesBosonPredictions
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && newDirectBridgeTheoremStillRequired
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && !gaugeHiggsBoundaryPromotesHiggsMass
    && !latestCompletionPromotesWzMasses
    && !latestCompletionPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "spectral-action-lead-is-geometric-high-scale-boundary",
        spectralActionGeometricLeadPresent
            && spectralActionHighScaleBoundaryLeadPresent
            && spectralActionRequiresFiniteNoncommutativeGeometry
            && spectralActionRequiresCutoffScaleBoundary,
        $"spectralActionGeometricLeadPresent={spectralActionGeometricLeadPresent}; highScaleBoundary={spectralActionHighScaleBoundaryLeadPresent}; finiteNoncommutativeGeometry={spectralActionRequiresFiniteNoncommutativeGeometry}; cutoffScaleBoundary={spectralActionRequiresCutoffScaleBoundary}"),
    new Check(
        "spectral-action-original-higgs-value-not-current-promotion",
        originalSpectralHiggsMassMidpointGeV > 150.0
            && lowHiggsCompatibilityRequiresSingletOrExtendedScalar
            && !spectralActionPromotesHiggsMass,
        $"originalSpectralHiggsMassRangeGeV=[{originalSpectralHiggsMassRangeGeV[0]:R},{originalSpectralHiggsMassRangeGeV[1]:R}]; lowHiggsCompatibilityRequiresSingletOrExtendedScalar={lowHiggsCompatibilityRequiresSingletOrExtendedScalar}; spectralActionPromotesHiggsMass={spectralActionPromotesHiggsMass}"),
    new Check(
        "no-local-gu-spectral-action-source-artifact",
        !localGuSpectralTripleArtifactFound
            && !localGuFiniteAlgebraMappingFound
            && !localGuSpectralActionCutoffSourceFound
            && !localGuSpectralBoundaryConditionSourceFound,
        $"localGuSpectralTripleArtifactFound={localGuSpectralTripleArtifactFound}; localGuFiniteAlgebraMappingFound={localGuFiniteAlgebraMappingFound}; localGuSpectralActionCutoffSourceFound={localGuSpectralActionCutoffSourceFound}; localGuSpectralBoundaryConditionSourceFound={localGuSpectralBoundaryConditionSourceFound}"),
    new Check(
        "spectral-action-auxiliary-inputs-not-gu-sourced",
        !localGuSpectralRgTransportSourceFound
            && !localGuSpectralYukawaMajoranaSourceFound
            && !localGuSpectralObservedFieldExtractionFound
            && !localGuSpectralVevSourceFound,
        $"rgTransportSource={localGuSpectralRgTransportSourceFound}; yukawaMajoranaSource={localGuSpectralYukawaMajoranaSourceFound}; observedFieldExtraction={localGuSpectralObservedFieldExtractionFound}; vevSource={localGuSpectralVevSourceFound}"),
    new Check(
        "current-repo-blockers-still-active",
        !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !lowEnergyRgTransportSourcePromotable
            && newDirectBridgeTheoremStillRequired
            && newHiggsScalarSourceStillRequired
            && !currentImplementationCanFillObservedFieldExtractionContract,
        $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}; newDirectBridgeTheoremStillRequired={newDirectBridgeTheoremStillRequired}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}; currentImplementationCanFillObservedFieldExtractionContract={currentImplementationCanFillObservedFieldExtractionContract}"),
    new Check(
        "spectral-action-does-not-complete-boson-predictions",
        !spectralActionPromotesWzMasses
            && !spectralActionPromotesHiggsMass
            && !spectralActionCompletesBosonPredictions
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"spectralActionPromotesWzMasses={spectralActionPromotesWzMasses}; spectralActionPromotesHiggsMass={spectralActionPromotesHiggsMass}; spectralActionCompletesBosonPredictions={spectralActionCompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = spectralActionBosonSourceAuditPassed
    ? "spectral-action-boson-source-audit-external-boundary-not-promotion"
    : "spectral-action-boson-source-audit-review-required";

var result = new
{
    phaseId = "phase268-spectral-action-boson-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    spectralActionBosonSourceAuditPassed,
    spectralActionGeometricLeadPresent,
    spectralActionPromotesWzMasses,
    spectralActionPromotesHiggsMass,
    spectralActionCompletesBosonPredictions,
    spectralActionBoundary = new
    {
        spectralActionHighScaleBoundaryLeadPresent,
        originalSpectralHiggsMassRangeGeV,
        originalSpectralHiggsMassMidpointGeV,
        lowHiggsCompatibilityRequiresSingletOrExtendedScalar,
        spectralActionRequiresFiniteNoncommutativeGeometry,
        spectralActionRequiresCutoffScaleBoundary,
        spectralActionRequiresRgTransportToLowEnergy,
        spectralActionRequiresYukawaAndMajoranaInputs,
        spectralActionUsesModelSpecificSpectralTriple,
        spectralActionExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuSpectralTripleArtifactFound,
        localGuFiniteAlgebraMappingFound,
        localGuSpectralActionCutoffSourceFound,
        localGuSpectralBoundaryConditionSourceFound,
        localGuSpectralRgTransportSourceFound,
        localGuSpectralYukawaMajoranaSourceFound,
        localGuSpectralObservedFieldExtractionFound,
        localGuSpectralVevSourceFound,
        localGuSpectralHiggsSingletSourceFound,
    },
    currentBlockerEvidence = new
    {
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
        phase247 = new
        {
            newDirectBridgeTheoremStillRequired,
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
        phase265 = new
        {
            gaugeHiggsBoundaryPromotesHiggsMass,
        },
        phase267 = new
        {
            latestCompletionPromotesWzMasses,
            latestCompletionPromotesHiggsMass,
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
            "spectral-action-standard-model-neutrino-mixing",
            "https://arxiv.org/abs/hep-th/0610241",
            "The original spectral-action model derives Standard Model structure from a finite noncommutative geometry and predicts high-scale relations; its low-energy Higgs estimate was around 170 GeV."),
        new ExternalSource(
            "resilience-of-spectral-standard-model",
            "https://arxiv.org/abs/1208.1030",
            "A real scalar singlet modifies the RG analysis and invalidates the older 160-180 GeV Higgs prediction, restoring compatibility with the low Higgs mass."),
        new ExternalSource(
            "higgs-mass-in-noncommutative-geometry",
            "https://arxiv.org/abs/1403.7567",
            "The Higgs mass can be made compatible with about 126 GeV by adding or generating an extra scalar field associated with Majorana neutrino structure."),
        new ExternalSource(
            "new-scalar-fields-in-noncommutative-geometry",
            "https://arxiv.org/abs/0901.4676",
            "Spectral-action relations among gauge, quartic, and Yukawa couplings are imposed at a high cutoff and require RG flow plus extra model fields."),
    },
    checks,
    decision = spectralActionBosonSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from noncommutative-geometry spectral-action literature. It is a valuable geometric high-scale boundary lead, but the repository lacks a GU-local spectral triple/finite algebra, cutoff source, RG transport, Yukawa/Majorana source, observed-field extraction, VEV source, and W/Z/H source rows."
        : "Review spectral-action boson source audit before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local spectral-action or equivalent source theorem if this route is to be used.",
        "A finite-algebra/representation map from GU fields to observed W/Z/H rows.",
        "A cutoff/boundary-scale source, low-energy RG transport, Yukawa/Majorana source, VEV source, and observed-field extraction theorem.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase247Path = Phase247Path,
        phase248Path = Phase248Path,
        phase257Path = Phase257Path,
        phase265Path = Phase265Path,
        phase267Path = Phase267Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "spectral_action_boson_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "spectral_action_boson_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.spectralActionBosonSourceAuditPassed,
        result.spectralActionGeometricLeadPresent,
        result.spectralActionPromotesWzMasses,
        result.spectralActionPromotesHiggsMass,
        result.spectralActionCompletesBosonPredictions,
        result.spectralActionBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"spectralActionBosonSourceAuditPassed={spectralActionBosonSourceAuditPassed}");
Console.WriteLine($"spectralActionGeometricLeadPresent={spectralActionGeometricLeadPresent}");
Console.WriteLine($"spectralActionPromotesWzMasses={spectralActionPromotesWzMasses}");
Console.WriteLine($"spectralActionPromotesHiggsMass={spectralActionPromotesHiggsMass}");
Console.WriteLine($"originalSpectralHiggsMassMidpointGeV={originalSpectralHiggsMassMidpointGeV:R}");
Console.WriteLine($"lowHiggsCompatibilityRequiresSingletOrExtendedScalar={lowHiggsCompatibilityRequiresSingletOrExtendedScalar}");
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

record ExternalSource(string SourceId, string Url, string Finding);

record Check(string CheckId, bool Passed, string Detail);

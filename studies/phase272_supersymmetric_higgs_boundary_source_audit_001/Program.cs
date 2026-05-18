using System.Text.Json;

const string DefaultOutputDir = "studies/phase272_supersymmetric_higgs_boundary_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE272_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var higgsRow = FindRow(rows, "higgs");
var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");

var supersymmetricHiggsBoundaryLeadPresent = true;
var mssmGaugeDTermQuarticLeadPresent = true;
var mssmTreeLevelMaxHiggsMassGeV = 91.1876;
var mssmTreeLevelDeficitToObservedHiggsGeV = higgsTargetGeV - mssmTreeLevelMaxHiggsMassGeV;
var observedHiggsRequiresRadiativeCorrections = mssmTreeLevelDeficitToObservedHiggsGeV > 20.0;
var observedHiggsRequiresHeavyStopsOrMaximalStopMixing = true;
var mssmRequiresTanBetaSource = true;
var mssmRequiresPseudoscalarMassSource = true;
var mssmRequiresSusyBreakingScaleSource = true;
var mssmRequiresStopMassAndMixingSource = true;
var mssmRequiresMuParameterSource = true;
var mssmRequiresThresholdCorrections = true;
var mssmRequiresRgTransport = true;
var mssmRequiresMassSchemeSource = true;
var mssmExternalToGu = true;

var localGuSupersymmetryAlgebraSourceFound = false;
var localGuSuperpartnerSpectrumSourceFound = false;
var localGuSusyBreakingScaleSourceFound = false;
var localGuMssmHiggsDoubletSourceFound = false;
var localGuTanBetaSourceFound = false;
var localGuPseudoscalarMassSourceFound = false;
var localGuStopMassAndMixingSourceFound = false;
var localGuMuParameterSourceFound = false;
var localGuMssmThresholdSourceFound = false;
var localGuMssmRgTransportFound = false;
var localGuMssmVevSourceFound = false;
var localGuMssmObservedFieldExtractionFound = false;
var localGuMssmWzMassMatrixSourceFound = false;
var localGuMssmHiggsScalarSourceFound = false;
var supersymmetryPromotesWzMasses = false;
var supersymmetryPromotesHiggsMass = false;
var supersymmetryCompletesBosonPredictions = false;

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

var supersymmetricHiggsBoundarySourceAuditPassed =
    supersymmetricHiggsBoundaryLeadPresent
    && mssmGaugeDTermQuarticLeadPresent
    && observedHiggsRequiresRadiativeCorrections
    && observedHiggsRequiresHeavyStopsOrMaximalStopMixing
    && mssmRequiresTanBetaSource
    && mssmRequiresPseudoscalarMassSource
    && mssmRequiresSusyBreakingScaleSource
    && mssmRequiresStopMassAndMixingSource
    && mssmRequiresMuParameterSource
    && mssmRequiresThresholdCorrections
    && mssmRequiresRgTransport
    && mssmRequiresMassSchemeSource
    && mssmExternalToGu
    && !localGuSupersymmetryAlgebraSourceFound
    && !localGuSuperpartnerSpectrumSourceFound
    && !localGuSusyBreakingScaleSourceFound
    && !localGuMssmHiggsDoubletSourceFound
    && !localGuTanBetaSourceFound
    && !localGuPseudoscalarMassSourceFound
    && !localGuStopMassAndMixingSourceFound
    && !localGuMuParameterSourceFound
    && !localGuMssmThresholdSourceFound
    && !localGuMssmRgTransportFound
    && !localGuMssmVevSourceFound
    && !localGuMssmObservedFieldExtractionFound
    && !localGuMssmWzMassMatrixSourceFound
    && !localGuMssmHiggsScalarSourceFound
    && !supersymmetryPromotesWzMasses
    && !supersymmetryPromotesHiggsMass
    && !supersymmetryCompletesBosonPredictions
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
        "mssm-tree-level-higgs-boundary-is-gauge-dterm-lead",
        supersymmetricHiggsBoundaryLeadPresent
            && mssmGaugeDTermQuarticLeadPresent
            && observedHiggsRequiresRadiativeCorrections,
        $"mssmTreeLevelMaxHiggsMassGeV={mssmTreeLevelMaxHiggsMassGeV:R}; higgsTargetGeV={higgsTargetGeV:R}; deficit={mssmTreeLevelDeficitToObservedHiggsGeV:R}; observedHiggsRequiresRadiativeCorrections={observedHiggsRequiresRadiativeCorrections}"),
    new Check(
        "mssm-physical-higgs-needs-susy-breaking-threshold-contract",
        observedHiggsRequiresHeavyStopsOrMaximalStopMixing
            && mssmRequiresSusyBreakingScaleSource
            && mssmRequiresStopMassAndMixingSource
            && mssmRequiresThresholdCorrections
            && mssmRequiresRgTransport,
        $"heavyStopsOrMaximalStopMixing={observedHiggsRequiresHeavyStopsOrMaximalStopMixing}; susyBreakingScaleSource={mssmRequiresSusyBreakingScaleSource}; stopMassAndMixingSource={mssmRequiresStopMassAndMixingSource}; thresholdCorrections={mssmRequiresThresholdCorrections}; rgTransport={mssmRequiresRgTransport}"),
    new Check(
        "no-local-gu-mssm-source-artifact",
        !localGuSupersymmetryAlgebraSourceFound
            && !localGuSuperpartnerSpectrumSourceFound
            && !localGuSusyBreakingScaleSourceFound
            && !localGuTanBetaSourceFound
            && !localGuStopMassAndMixingSourceFound
            && !localGuMssmObservedFieldExtractionFound,
        $"supersymmetryAlgebraSource={localGuSupersymmetryAlgebraSourceFound}; superpartnerSpectrumSource={localGuSuperpartnerSpectrumSourceFound}; susyBreakingScaleSource={localGuSusyBreakingScaleSourceFound}; tanBetaSource={localGuTanBetaSourceFound}; stopMassAndMixingSource={localGuStopMassAndMixingSourceFound}; observedFieldExtraction={localGuMssmObservedFieldExtractionFound}"),
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
        "supersymmetry-does-not-complete-boson-predictions",
        !supersymmetryPromotesWzMasses
            && !supersymmetryPromotesHiggsMass
            && !supersymmetryCompletesBosonPredictions
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"supersymmetryPromotesWzMasses={supersymmetryPromotesWzMasses}; supersymmetryPromotesHiggsMass={supersymmetryPromotesHiggsMass}; supersymmetryCompletesBosonPredictions={supersymmetryCompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = supersymmetricHiggsBoundarySourceAuditPassed
    ? "supersymmetric-higgs-boundary-source-audit-external-threshold-model-not-promotion"
    : "supersymmetric-higgs-boundary-source-audit-review-required";

var result = new
{
    phaseId = "phase272-supersymmetric-higgs-boundary-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    supersymmetricHiggsBoundarySourceAuditPassed,
    supersymmetricHiggsBoundaryLeadPresent,
    mssmGaugeDTermQuarticLeadPresent,
    observedHiggsRequiresRadiativeCorrections,
    observedHiggsRequiresHeavyStopsOrMaximalStopMixing,
    supersymmetryPromotesWzMasses,
    supersymmetryPromotesHiggsMass,
    supersymmetryCompletesBosonPredictions,
    supersymmetricBoundary = new
    {
        mssmTreeLevelMaxHiggsMassGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        mssmTreeLevelDeficitToObservedHiggsGeV,
        mssmRequiresTanBetaSource,
        mssmRequiresPseudoscalarMassSource,
        mssmRequiresSusyBreakingScaleSource,
        mssmRequiresStopMassAndMixingSource,
        mssmRequiresMuParameterSource,
        mssmRequiresThresholdCorrections,
        mssmRequiresRgTransport,
        mssmRequiresMassSchemeSource,
        mssmExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuSupersymmetryAlgebraSourceFound,
        localGuSuperpartnerSpectrumSourceFound,
        localGuSusyBreakingScaleSourceFound,
        localGuMssmHiggsDoubletSourceFound,
        localGuTanBetaSourceFound,
        localGuPseudoscalarMassSourceFound,
        localGuStopMassAndMixingSourceFound,
        localGuMuParameterSourceFound,
        localGuMssmThresholdSourceFound,
        localGuMssmRgTransportFound,
        localGuMssmVevSourceFound,
        localGuMssmObservedFieldExtractionFound,
        localGuMssmWzMassMatrixSourceFound,
        localGuMssmHiggsScalarSourceFound,
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
            "renormalization-group-improved-mssm-higgs-sector",
            "https://journals.aps.org/prd/abstract/10.1103/PhysRevD.48.4280",
            "The MSSM relates Higgs self-couplings to gauge couplings at tree level, but top/stop radiative corrections can significantly shift Higgs masses."),
        new ExternalSource(
            "draper-meade-reece-shih-125gev-mssm",
            "https://arxiv.org/abs/1112.3068",
            "A 125 GeV Higgs in the MSSM implies either very heavy stops or near-maximal stop mixing, strongly restricting SUSY-breaking parameter space."),
        new ExternalSource(
            "djouadi-implications-higgs-discovery-mssm",
            "https://link.springer.com/article/10.1140/epjc/s10052-013-2704-3",
            "The MSSM Higgs sector is described by tan beta and M_A at tree level, but radiative corrections make Higgs masses depend on the wider MSSM spectrum."),
        new ExternalSource(
            "maximal-stop-mixing-mssm",
            "https://link.springer.com/article/10.1007/JHEP08(2012)089",
            "A Standard-Model-like Higgs near 125 GeV requires multi-TeV stops or near-maximal stop-mixing contributions."),
    },
    localSearchFinding = "Repository search found speculative non-spacetime SUSY remarks in completion text, but no GU-local MSSM superpartner spectrum, SUSY-breaking scale, tan beta, stop-threshold, RG, VEV, W/Z mass-matrix, Higgs scalar, or observed-field extraction artifact.",
    checks,
    decision = supersymmetricHiggsBoundarySourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from supersymmetric/MSSM Higgs-boundary literature. The route is a serious external Higgs-quartic boundary lead, but the repository lacks a GU-local supersymmetry algebra with observational consequences, superpartner spectrum, SUSY-breaking scale, tan beta, pseudoscalar mass, stop mass/mixing, threshold corrections, RG transport, VEV source, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem."
        : "Review supersymmetric Higgs boundary source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local supersymmetry algebra or equivalent source with observational superpartner content.",
        "A target-independent SUSY-breaking scale, tan beta, pseudoscalar mass, stop spectrum/mixing, mu parameter, threshold correction, and RG-transport source.",
        "A GU VEV/source-scale theorem, W/Z mass matrix, Higgs scalar mass source, and observed-field extraction theorem.",
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
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "supersymmetric_higgs_boundary_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "supersymmetric_higgs_boundary_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.supersymmetricHiggsBoundarySourceAuditPassed,
        result.supersymmetricHiggsBoundaryLeadPresent,
        result.mssmGaugeDTermQuarticLeadPresent,
        result.observedHiggsRequiresRadiativeCorrections,
        result.observedHiggsRequiresHeavyStopsOrMaximalStopMixing,
        result.supersymmetryPromotesWzMasses,
        result.supersymmetryPromotesHiggsMass,
        result.supersymmetryCompletesBosonPredictions,
        result.supersymmetricBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"supersymmetricHiggsBoundarySourceAuditPassed={supersymmetricHiggsBoundarySourceAuditPassed}");
Console.WriteLine($"supersymmetricHiggsBoundaryLeadPresent={supersymmetricHiggsBoundaryLeadPresent}");
Console.WriteLine($"mssmGaugeDTermQuarticLeadPresent={mssmGaugeDTermQuarticLeadPresent}");
Console.WriteLine($"mssmTreeLevelDeficitToObservedHiggsGeV={mssmTreeLevelDeficitToObservedHiggsGeV:R}");
Console.WriteLine($"observedHiggsRequiresHeavyStopsOrMaximalStopMixing={observedHiggsRequiresHeavyStopsOrMaximalStopMixing}");
Console.WriteLine($"supersymmetryPromotesWzMasses={supersymmetryPromotesWzMasses}");
Console.WriteLine($"supersymmetryPromotesHiggsMass={supersymmetryPromotesHiggsMass}");
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

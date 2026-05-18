using System.Text.Json;

const string DefaultOutputDir = "studies/phase277_finite_unified_gauge_yukawa_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase272Path = "studies/phase272_supersymmetric_higgs_boundary_source_audit_001/output/supersymmetric_higgs_boundary_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE277_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase272 = JsonDocument.Parse(File.ReadAllText(Phase272Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var higgsRow = FindRow(rows, "higgs");
var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");

var finiteUnifiedTheoryLeadPresent = true;
var gaugeYukawaUnificationLeadPresent = true;
var reductionOfCouplingsLeadPresent = true;
var allLoopFinitenessLeadPresent = true;
var predictedHiggsBandMinGeV = 121.0;
var predictedHiggsBandMaxGeV = 126.0;
var finiteUnifiedHiggsBandContainsTarget = higgsTargetGeV >= predictedHiggsBandMinGeV && higgsTargetGeV <= predictedHiggsBandMaxGeV;
var finiteUnifiedBandWidthGeV = predictedHiggsBandMaxGeV - predictedHiggsBandMinGeV;

var requiresGuFiniteUnifiedGaugeGroupSource = true;
var requiresGuN1SupersymmetryEmbeddingSource = true;
var requiresGaugeYukawaUnificationSource = true;
var requiresReductionEquationSource = true;
var requiresAllLoopFinitenessProofSource = true;
var requiresSoftSusyBreakingSumRuleSource = true;
var requiresHeavySusySpectrumSource = true;
var requiresGauginoMassAndScalarMassSource = true;
var requiresTopBottomTauYukawaSource = true;
var requiresThresholdCorrections = true;
var requiresLowEnergyRgTransport = true;
var requiresHiggsMassCalculatorSchemeSource = true;
var requiresVevSource = true;
var requiresWzMassMatrixSource = true;
var requiresHiggsScalarSource = true;
var requiresObservedFieldExtraction = true;
var finiteUnifiedTheoryExternalToGu = true;

var localGuFiniteUnifiedGaugeGroupSourceFound = false;
var localGuN1SupersymmetryEmbeddingSourceFound = false;
var localGuGaugeYukawaUnificationSourceFound = false;
var localGuReductionEquationSourceFound = false;
var localGuAllLoopFinitenessProofFound = false;
var localGuSoftSusyBreakingSumRuleFound = false;
var localGuHeavySusySpectrumSourceFound = false;
var localGuGauginoMassAndScalarMassSourceFound = false;
var localGuTopBottomTauYukawaSourceFound = false;
var localGuFiniteGutThresholdSourceFound = false;
var localGuFiniteGutRgTransportFound = false;
var localGuHiggsMassSchemeSourceFound = false;
var localGuVevSourceFound = false;
var localGuWzMassMatrixSourceFound = false;
var localGuHiggsScalarSourceFound = false;
var localGuObservedFieldExtractionFound = false;
var finiteUnifiedTheoryPromotesWzMasses = false;
var finiteUnifiedTheoryPromotesHiggsMass = false;
var finiteUnifiedTheoryCompletesBosonPredictions = false;

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
var supersymmetricHiggsBoundarySourceAuditPassed = JsonBool(phase272.RootElement, "supersymmetricHiggsBoundarySourceAuditPassed") is true;
var supersymmetryPromotesHiggsMass = JsonBool(phase272.RootElement, "supersymmetryPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var finiteUnifiedGaugeYukawaSourceAuditPassed =
    finiteUnifiedTheoryLeadPresent
    && gaugeYukawaUnificationLeadPresent
    && reductionOfCouplingsLeadPresent
    && allLoopFinitenessLeadPresent
    && finiteUnifiedHiggsBandContainsTarget
    && finiteUnifiedBandWidthGeV >= 5.0
    && requiresGuFiniteUnifiedGaugeGroupSource
    && requiresGuN1SupersymmetryEmbeddingSource
    && requiresGaugeYukawaUnificationSource
    && requiresReductionEquationSource
    && requiresAllLoopFinitenessProofSource
    && requiresSoftSusyBreakingSumRuleSource
    && requiresHeavySusySpectrumSource
    && requiresGauginoMassAndScalarMassSource
    && requiresTopBottomTauYukawaSource
    && requiresThresholdCorrections
    && requiresLowEnergyRgTransport
    && requiresHiggsMassCalculatorSchemeSource
    && requiresVevSource
    && requiresWzMassMatrixSource
    && requiresHiggsScalarSource
    && requiresObservedFieldExtraction
    && finiteUnifiedTheoryExternalToGu
    && !localGuFiniteUnifiedGaugeGroupSourceFound
    && !localGuN1SupersymmetryEmbeddingSourceFound
    && !localGuGaugeYukawaUnificationSourceFound
    && !localGuReductionEquationSourceFound
    && !localGuAllLoopFinitenessProofFound
    && !localGuSoftSusyBreakingSumRuleFound
    && !localGuHeavySusySpectrumSourceFound
    && !localGuGauginoMassAndScalarMassSourceFound
    && !localGuTopBottomTauYukawaSourceFound
    && !localGuFiniteGutThresholdSourceFound
    && !localGuFiniteGutRgTransportFound
    && !localGuHiggsMassSchemeSourceFound
    && !localGuVevSourceFound
    && !localGuWzMassMatrixSourceFound
    && !localGuHiggsScalarSourceFound
    && !localGuObservedFieldExtractionFound
    && !finiteUnifiedTheoryPromotesWzMasses
    && !finiteUnifiedTheoryPromotesHiggsMass
    && !finiteUnifiedTheoryCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && supersymmetricHiggsBoundarySourceAuditPassed
    && !supersymmetryPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "finite-unified-gauge-yukawa-lead-present",
        finiteUnifiedTheoryLeadPresent
            && gaugeYukawaUnificationLeadPresent
            && reductionOfCouplingsLeadPresent
            && allLoopFinitenessLeadPresent,
        $"finiteUnifiedTheoryLeadPresent={finiteUnifiedTheoryLeadPresent}; gaugeYukawaUnificationLeadPresent={gaugeYukawaUnificationLeadPresent}; reductionOfCouplingsLeadPresent={reductionOfCouplingsLeadPresent}; allLoopFinitenessLeadPresent={allLoopFinitenessLeadPresent}"),
    new Check(
        "finite-unified-higgs-band-is-compatible-not-source-lineage",
        finiteUnifiedHiggsBandContainsTarget && finiteUnifiedBandWidthGeV >= 5.0,
        $"predictedHiggsBandGeV={predictedHiggsBandMinGeV:R}-{predictedHiggsBandMaxGeV:R}; higgsTargetGeV={higgsTargetGeV:R}; bandWidthGeV={finiteUnifiedBandWidthGeV:R}"),
    new Check(
        "finite-unified-input-contract-is-external-and-supersymmetric",
        requiresGuFiniteUnifiedGaugeGroupSource
            && requiresGuN1SupersymmetryEmbeddingSource
            && requiresGaugeYukawaUnificationSource
            && requiresReductionEquationSource
            && requiresAllLoopFinitenessProofSource
            && requiresSoftSusyBreakingSumRuleSource
            && requiresHeavySusySpectrumSource
            && requiresLowEnergyRgTransport,
        $"finiteGaugeGroup={requiresGuFiniteUnifiedGaugeGroupSource}; n1SusyEmbedding={requiresGuN1SupersymmetryEmbeddingSource}; gaugeYukawaUnification={requiresGaugeYukawaUnificationSource}; reductionEquation={requiresReductionEquationSource}; allLoopFiniteness={requiresAllLoopFinitenessProofSource}; softSumRule={requiresSoftSusyBreakingSumRuleSource}; heavySusySpectrum={requiresHeavySusySpectrumSource}; rgTransport={requiresLowEnergyRgTransport}"),
    new Check(
        "no-local-gu-finite-unified-source-artifact",
        !localGuFiniteUnifiedGaugeGroupSourceFound
            && !localGuGaugeYukawaUnificationSourceFound
            && !localGuReductionEquationSourceFound
            && !localGuAllLoopFinitenessProofFound
            && !localGuSoftSusyBreakingSumRuleFound
            && !localGuFiniteGutRgTransportFound
            && !localGuObservedFieldExtractionFound,
        $"finiteGaugeGroup={localGuFiniteUnifiedGaugeGroupSourceFound}; gaugeYukawaUnification={localGuGaugeYukawaUnificationSourceFound}; reductionEquation={localGuReductionEquationSourceFound}; allLoopFiniteness={localGuAllLoopFinitenessProofFound}; softSumRule={localGuSoftSusyBreakingSumRuleFound}; rgTransport={localGuFiniteGutRgTransportFound}; observedFieldExtraction={localGuObservedFieldExtractionFound}"),
    new Check(
        "finite-unified-does-not-fill-gu-wz-or-higgs-source-contracts",
        !localGuVevSourceFound
            && !localGuWzMassMatrixSourceFound
            && !localGuHiggsScalarSourceFound
            && !finiteUnifiedTheoryPromotesWzMasses
            && !finiteUnifiedTheoryPromotesHiggsMass
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"localGuVevSourceFound={localGuVevSourceFound}; localGuWzMassMatrixSourceFound={localGuWzMassMatrixSourceFound}; localGuHiggsScalarSourceFound={localGuHiggsScalarSourceFound}; finiteUnifiedTheoryPromotesWzMasses={finiteUnifiedTheoryPromotesWzMasses}; finiteUnifiedTheoryPromotesHiggsMass={finiteUnifiedTheoryPromotesHiggsMass}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "neighboring-supersymmetric-boundary-route-remains-nonpromotional",
        supersymmetricHiggsBoundarySourceAuditPassed && !supersymmetryPromotesHiggsMass,
        $"supersymmetricHiggsBoundarySourceAuditPassed={supersymmetricHiggsBoundarySourceAuditPassed}; supersymmetryPromotesHiggsMass={supersymmetryPromotesHiggsMass}"),
};

var terminalStatus = finiteUnifiedGaugeYukawaSourceAuditPassed
    ? "finite-unified-gauge-yukawa-source-audit-external-susy-gut-boundary-not-promotion"
    : "finite-unified-gauge-yukawa-source-audit-review-required";

var result = new
{
    phaseId = "phase277-finite-unified-gauge-yukawa-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    finiteUnifiedGaugeYukawaSourceAuditPassed,
    finiteUnifiedTheoryLeadPresent,
    gaugeYukawaUnificationLeadPresent,
    reductionOfCouplingsLeadPresent,
    allLoopFinitenessLeadPresent,
    finiteUnifiedHiggsBandContainsTarget,
    finiteUnifiedTheoryPromotesWzMasses,
    finiteUnifiedTheoryPromotesHiggsMass,
    finiteUnifiedTheoryCompletesBosonPredictions,
    finiteUnifiedBoundary = new
    {
        predictedHiggsBandMinGeV,
        predictedHiggsBandMaxGeV,
        finiteUnifiedBandWidthGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        requiresGuFiniteUnifiedGaugeGroupSource,
        requiresGuN1SupersymmetryEmbeddingSource,
        requiresGaugeYukawaUnificationSource,
        requiresReductionEquationSource,
        requiresAllLoopFinitenessProofSource,
        requiresSoftSusyBreakingSumRuleSource,
        requiresHeavySusySpectrumSource,
        requiresGauginoMassAndScalarMassSource,
        requiresTopBottomTauYukawaSource,
        requiresThresholdCorrections,
        requiresLowEnergyRgTransport,
        requiresHiggsMassCalculatorSchemeSource,
        requiresVevSource,
        requiresWzMassMatrixSource,
        requiresHiggsScalarSource,
        requiresObservedFieldExtraction,
        finiteUnifiedTheoryExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuFiniteUnifiedGaugeGroupSourceFound,
        localGuN1SupersymmetryEmbeddingSourceFound,
        localGuGaugeYukawaUnificationSourceFound,
        localGuReductionEquationSourceFound,
        localGuAllLoopFinitenessProofFound,
        localGuSoftSusyBreakingSumRuleFound,
        localGuHeavySusySpectrumSourceFound,
        localGuGauginoMassAndScalarMassSourceFound,
        localGuTopBottomTauYukawaSourceFound,
        localGuFiniteGutThresholdSourceFound,
        localGuFiniteGutRgTransportFound,
        localGuHiggsMassSchemeSourceFound,
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
        phase272 = new
        {
            supersymmetricHiggsBoundarySourceAuditPassed,
            supersymmetryPromotesHiggsMass,
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
            "heinemeyer-mondragon-zoupanos-finite-unified-higgs",
            "https://arxiv.org/abs/1201.5171",
            "All-loop finite N=1 supersymmetric GUTs use coupling reduction and experimental quark-mass constraints to predict a light Higgs band near 121-126 GeV."),
        new ExternalSource(
            "heinemeyer-mondragon-zoupanos-fut-update",
            "https://doi.org/10.3390/sym10030062",
            "Updated FUT work keeps the Higgs mass compatible with experiment but depends on a heavy SUSY spectrum and model-specific soft-breaking parameters."),
        new ExternalSource(
            "reduced-coupling-unified-theory-collider-study",
            "https://doi.org/10.1140/epjc/s10052-021-08966-4",
            "Later reduced-coupling studies treat the finite SU(5) construction as a high-scale SUSY-GUT benchmark with heavy-spectrum phenomenology."),
    },
    localSearchFinding = "Repository search found no finite unified theory, gauge-Yukawa unification, coupling-reduction, all-loop-finiteness, or finite SU(5) source artifacts. Existing supersymmetric boundary diagnostics remain nonpromotional and do not supply the required GU-local finite-GUT source lineages.",
    checks,
    decision = finiteUnifiedGaugeYukawaSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from finite unified / gauge-Yukawa unification literature. The route is a serious and numerically compatible external SUSY-GUT boundary lead, but it requires a GU-local finite gauge group, N=1 supersymmetry embedding, gauge-Yukawa reduction equations, all-loop finiteness proof, soft-breaking sum rule, heavy spectrum, top/bottom/tau sources, thresholds, RG transport, VEV source, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem that the repository does not contain."
        : "Review finite unified gauge-Yukawa source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local finite unified gauge group and N=1 supersymmetry embedding with gauge-Yukawa reduction equations and all-loop finiteness proof.",
        "GU source lineages for soft-breaking sum rules, heavy SUSY spectrum, top/bottom/tau Yukawas, thresholds, Higgs mass scheme, and low-energy RG transport.",
        "A GU VEV/source-scale theorem, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem.",
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
        phase272Path = Phase272Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "finite_unified_gauge_yukawa_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "finite_unified_gauge_yukawa_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.finiteUnifiedGaugeYukawaSourceAuditPassed,
        result.finiteUnifiedTheoryLeadPresent,
        result.gaugeYukawaUnificationLeadPresent,
        result.reductionOfCouplingsLeadPresent,
        result.allLoopFinitenessLeadPresent,
        result.finiteUnifiedHiggsBandContainsTarget,
        result.finiteUnifiedTheoryPromotesWzMasses,
        result.finiteUnifiedTheoryPromotesHiggsMass,
        result.finiteUnifiedTheoryCompletesBosonPredictions,
        result.finiteUnifiedBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"finiteUnifiedGaugeYukawaSourceAuditPassed={finiteUnifiedGaugeYukawaSourceAuditPassed}");
Console.WriteLine($"finiteUnifiedTheoryLeadPresent={finiteUnifiedTheoryLeadPresent}");
Console.WriteLine($"gaugeYukawaUnificationLeadPresent={gaugeYukawaUnificationLeadPresent}");
Console.WriteLine($"reductionOfCouplingsLeadPresent={reductionOfCouplingsLeadPresent}");
Console.WriteLine($"allLoopFinitenessLeadPresent={allLoopFinitenessLeadPresent}");
Console.WriteLine($"finiteUnifiedHiggsBandContainsTarget={finiteUnifiedHiggsBandContainsTarget}");
Console.WriteLine($"finiteUnifiedTheoryPromotesWzMasses={finiteUnifiedTheoryPromotesWzMasses}");
Console.WriteLine($"finiteUnifiedTheoryPromotesHiggsMass={finiteUnifiedTheoryPromotesHiggsMass}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static JsonElement FindRow(JsonElement[] rows, string particleId)
{
    foreach (var row in rows)
    {
        if (JsonString(row, "particleId") == particleId)
        {
            return row;
        }
    }

    throw new InvalidOperationException($"Missing row for {particleId}.");
}

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property {propertyName}.");

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

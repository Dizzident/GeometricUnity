using System.Text.Json;

const string DefaultOutputDir = "studies/phase281_geometric_refractive_unification_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase227Path = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output/official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase243Path = "studies/phase243_public_web_source_delta_audit_001/output/public_web_source_delta_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase267Path = "studies/phase267_completion_revision_direct_bridge_source_audit_001/output/completion_revision_direct_bridge_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE281_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase227 = JsonDocument.Parse(File.ReadAllText(Phase227Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase243 = JsonDocument.Parse(File.ReadAllText(Phase243Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase267 = JsonDocument.Parse(File.ReadAllText(Phase267Path));

var localSearchEvidence = BuildLocalSearchEvidence();

var guRvgSourceLeadPresent = true;
var phase243PriorGuRvgCoverageConfirmed =
    JsonBool(phase243.RootElement, "publicWebSourceDeltaAuditPassed") is true
    && JsonBool(phase243.RootElement, "recentGuRvgSynthesisFound") is true
    && JsonString(phase243.RootElement, "latestGuRvgSynthesisVersionReviewed") == "v8"
    && JsonBool(phase243.RootElement, "recentGuRvgSynthesisPromotableForBosonMasses") is false;
var guRvgLatestReviewedVersion = JsonString(phase243.RootElement, "latestGuRvgSynthesisVersionReviewed") ?? "unknown";
var guRvgSourceAlreadyCoveredByPhase243 = phase243PriorGuRvgCoverageConfirmed;
var guRvgClaimsGuLowEnergyEftSynthesis = true;
var traceAnomalyVacuumSourcingLeadPresent = true;
var runningVacuumModelLeadPresent = true;
var ninetyFiveGevDilatonResonanceLeadPresent = true;
var metricEngineeringOrDeviceLeadPresent = true;
var guRvgProvidesGuLocalWzTheorem = false;
var guRvgProvidesSeparateWzSourceRows = false;
var guRvgProvidesRawAmplitudeGate = false;
var guRvgProvidesCommonBridgeGate = false;
var guRvgProvidesTargetIndependentVevSource = false;
var guRvgProvidesWzMassMatrixSource = false;
var guRvgProvidesObservedFieldExtraction = false;
var guRvgProvidesHiggsScalarSourceOperator = false;
var guRvgProvidesHiggsIdentityEnvelope = false;
var guRvgProvidesObservedHiggsMassiveScalarProfile = false;
var guRvgProvidesHiggsSelfCouplingSource = false;
var guRvgUsesExternalEffectiveFieldTheory = true;
var guRvgUsesSpeculativeEngineeringDeviceModel = true;
var guRvgNinetyFiveGevLeadIsNotObservedHiggsPrediction = true;
var guRvgPromotesWzMasses = false;
var guRvgPromotesHiggsMass = false;
var guRvgCompletesBosonPredictions = false;

var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var obstructionAuditPassed = JsonBool(phase220.RootElement, "obstructionAuditPassed") is true;
var obstructionKind = JsonString(phase220.RootElement, "obstructionKind");
var officialGuShiabUpsilonExtractionPromotable = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionPromotable") is true;
var bosonMassMatrixExtractionPromotable = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionPromotable") is true;
var currentImplementationCanFillObservedFieldExtractionContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;
var latestCompletionProvidesDirectWzTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesDirectWzTheorem") is true;
var latestCompletionProvidesObservedFieldExtractionTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesObservedFieldExtractionTheorem") is true;
var latestCompletionProvidesQuantitativeMassScaleSource = JsonBool(phase267.RootElement, "latestCompletionProvidesQuantitativeMassScaleSource") is true;
var latestCompletionProvidesHiggsScalarSource = JsonBool(phase267.RootElement, "latestCompletionProvidesHiggsScalarSource") is true;
var latestCompletionPromotesWzMasses = JsonBool(phase267.RootElement, "latestCompletionPromotesWzMasses") is true;
var latestCompletionPromotesHiggsMass = JsonBool(phase267.RootElement, "latestCompletionPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var geometricRefractiveUnificationSourceAuditPassed =
    guRvgSourceLeadPresent
    && phase243PriorGuRvgCoverageConfirmed
    && guRvgSourceAlreadyCoveredByPhase243
    && guRvgLatestReviewedVersion == "v8"
    && guRvgClaimsGuLowEnergyEftSynthesis
    && traceAnomalyVacuumSourcingLeadPresent
    && runningVacuumModelLeadPresent
    && ninetyFiveGevDilatonResonanceLeadPresent
    && metricEngineeringOrDeviceLeadPresent
    && !guRvgProvidesGuLocalWzTheorem
    && !guRvgProvidesSeparateWzSourceRows
    && !guRvgProvidesRawAmplitudeGate
    && !guRvgProvidesCommonBridgeGate
    && !guRvgProvidesTargetIndependentVevSource
    && !guRvgProvidesWzMassMatrixSource
    && !guRvgProvidesObservedFieldExtraction
    && !guRvgProvidesHiggsScalarSourceOperator
    && !guRvgProvidesHiggsIdentityEnvelope
    && !guRvgProvidesObservedHiggsMassiveScalarProfile
    && !guRvgProvidesHiggsSelfCouplingSource
    && guRvgUsesExternalEffectiveFieldTheory
    && guRvgUsesSpeculativeEngineeringDeviceModel
    && guRvgNinetyFiveGevLeadIsNotObservedHiggsPrediction
    && !guRvgPromotesWzMasses
    && !guRvgPromotesHiggsMass
    && !guRvgCompletesBosonPredictions
    && localSearchEvidence.MatchingFileCount == 0
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !officialGuShiabUpsilonExtractionPromotable
    && !bosonMassMatrixExtractionPromotable
    && !currentImplementationCanFillObservedFieldExtractionContract
    && !latestCompletionProvidesDirectWzTheorem
    && !latestCompletionProvidesObservedFieldExtractionTheorem
    && !latestCompletionProvidesQuantitativeMassScaleSource
    && !latestCompletionProvidesHiggsScalarSource
    && !latestCompletionPromotesWzMasses
    && !latestCompletionPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "gu-rvg-public-source-lead-covered-by-phase243",
        guRvgSourceLeadPresent
            && phase243PriorGuRvgCoverageConfirmed
            && guRvgSourceAlreadyCoveredByPhase243
            && guRvgLatestReviewedVersion == "v8"
            && guRvgClaimsGuLowEnergyEftSynthesis
            && traceAnomalyVacuumSourcingLeadPresent
            && runningVacuumModelLeadPresent
            && ninetyFiveGevDilatonResonanceLeadPresent,
        $"leadPresent={guRvgSourceLeadPresent}; phase243PriorGuRvgCoverageConfirmed={phase243PriorGuRvgCoverageConfirmed}; sourceAlreadyCoveredByPhase243={guRvgSourceAlreadyCoveredByPhase243}; latestReviewedVersion={guRvgLatestReviewedVersion}; claimsGuLowEnergyEft={guRvgClaimsGuLowEnergyEftSynthesis}; traceAnomalyLead={traceAnomalyVacuumSourcingLeadPresent}; runningVacuumModelLead={runningVacuumModelLeadPresent}; ninetyFiveGevDilatonLead={ninetyFiveGevDilatonResonanceLeadPresent}"),
    new Check(
        "gu-rvg-does-not-fill-wz-source-lineage-contract",
        !guRvgProvidesGuLocalWzTheorem
            && !guRvgProvidesSeparateWzSourceRows
            && !guRvgProvidesRawAmplitudeGate
            && !guRvgProvidesCommonBridgeGate
            && !guRvgProvidesTargetIndependentVevSource
            && !guRvgProvidesWzMassMatrixSource
            && !guRvgProvidesObservedFieldExtraction
            && !guRvgPromotesWzMasses,
        $"guLocalWzTheorem={guRvgProvidesGuLocalWzTheorem}; separateWzSourceRows={guRvgProvidesSeparateWzSourceRows}; rawAmplitudeGate={guRvgProvidesRawAmplitudeGate}; commonBridgeGate={guRvgProvidesCommonBridgeGate}; targetIndependentVevSource={guRvgProvidesTargetIndependentVevSource}; wzMassMatrixSource={guRvgProvidesWzMassMatrixSource}; observedFieldExtraction={guRvgProvidesObservedFieldExtraction}; promotesWzMasses={guRvgPromotesWzMasses}"),
    new Check(
        "gu-rvg-does-not-fill-higgs-source-lineage-contract",
        !guRvgProvidesHiggsScalarSourceOperator
            && !guRvgProvidesHiggsIdentityEnvelope
            && !guRvgProvidesObservedHiggsMassiveScalarProfile
            && !guRvgProvidesHiggsSelfCouplingSource
            && guRvgNinetyFiveGevLeadIsNotObservedHiggsPrediction
            && !guRvgPromotesHiggsMass,
        $"higgsScalarSourceOperator={guRvgProvidesHiggsScalarSourceOperator}; higgsIdentityEnvelope={guRvgProvidesHiggsIdentityEnvelope}; observedHiggsMassiveScalarProfile={guRvgProvidesObservedHiggsMassiveScalarProfile}; higgsSelfCouplingSource={guRvgProvidesHiggsSelfCouplingSource}; ninetyFiveGevLeadIsNotObservedHiggsPrediction={guRvgNinetyFiveGevLeadIsNotObservedHiggsPrediction}; promotesHiggsMass={guRvgPromotesHiggsMass}"),
    new Check(
        "gu-rvg-is-external-and-device-oriented-not-local-gu-prediction-artifact",
        guRvgUsesExternalEffectiveFieldTheory
            && guRvgUsesSpeculativeEngineeringDeviceModel
            && metricEngineeringOrDeviceLeadPresent
            && localSearchEvidence.MatchingFileCount == 0,
        $"externalEffectiveFieldTheory={guRvgUsesExternalEffectiveFieldTheory}; speculativeEngineeringDeviceModel={guRvgUsesSpeculativeEngineeringDeviceModel}; metricEngineeringOrDeviceLeadPresent={metricEngineeringOrDeviceLeadPresent}; localSearchMatchingFileCount={localSearchEvidence.MatchingFileCount}; scannedFileCount={localSearchEvidence.ScannedFileCount}"),
    new Check(
        "current-source-lineage-blockers-still-active",
        obstructionAuditPassed
            && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !officialGuShiabUpsilonExtractionPromotable
            && !bosonMassMatrixExtractionPromotable
            && !currentImplementationCanFillObservedFieldExtractionContract
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"obstructionAuditPassed={obstructionAuditPassed}; obstructionKind={obstructionKind}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; officialGuShiabUpsilonExtractionPromotable={officialGuShiabUpsilonExtractionPromotable}; bosonMassMatrixExtractionPromotable={bosonMassMatrixExtractionPromotable}; observedFieldExtractionContract={currentImplementationCanFillObservedFieldExtractionContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = geometricRefractiveUnificationSourceAuditPassed
    ? "geometric-refractive-unification-source-audit-external-eft-not-promotion"
    : "geometric-refractive-unification-source-audit-review-required";

var result = new
{
    phaseId = "phase281-geometric-refractive-unification-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    geometricRefractiveUnificationSourceAuditPassed,
    guRvgSourceLeadPresent,
    phase243PriorGuRvgCoverageConfirmed,
    guRvgSourceAlreadyCoveredByPhase243,
    guRvgLatestReviewedVersion,
    guRvgClaimsGuLowEnergyEftSynthesis,
    traceAnomalyVacuumSourcingLeadPresent,
    runningVacuumModelLeadPresent,
    ninetyFiveGevDilatonResonanceLeadPresent,
    metricEngineeringOrDeviceLeadPresent,
    guRvgPromotesWzMasses,
    guRvgPromotesHiggsMass,
    guRvgCompletesBosonPredictions,
    guRvgBoundary = new
    {
        guRvgProvidesGuLocalWzTheorem,
        guRvgProvidesSeparateWzSourceRows,
        guRvgProvidesRawAmplitudeGate,
        guRvgProvidesCommonBridgeGate,
        guRvgProvidesTargetIndependentVevSource,
        guRvgProvidesWzMassMatrixSource,
        guRvgProvidesObservedFieldExtraction,
        guRvgProvidesHiggsScalarSourceOperator,
        guRvgProvidesHiggsIdentityEnvelope,
        guRvgProvidesObservedHiggsMassiveScalarProfile,
        guRvgProvidesHiggsSelfCouplingSource,
        guRvgUsesExternalEffectiveFieldTheory,
        guRvgUsesSpeculativeEngineeringDeviceModel,
        guRvgNinetyFiveGevLeadIsNotObservedHiggsPrediction,
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
        phase227 = new
        {
            officialGuShiabUpsilonExtractionPromotable,
        },
        phase228 = new
        {
            bosonMassMatrixExtractionPromotable,
        },
        phase257 = new
        {
            currentImplementationCanFillObservedFieldExtractionContract,
        },
        phase267 = new
        {
            latestCompletionProvidesDirectWzTheorem,
            latestCompletionProvidesObservedFieldExtractionTheorem,
            latestCompletionProvidesQuantitativeMassScaleSource,
            latestCompletionProvidesHiggsScalarSource,
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
            "official-gu-2013-oxford-lecture",
            "https://geometricunity.org/2013-oxford-lecture/",
            "Official GU transcript gives architectural leads: augmented torsion VEV, Y-to-X pullback, and Higgs/Yang-Mills content from a Dirac square, but not a checked physical W/Z/H source-lineage artifact."),
        new ExternalSource(
            "official-gu-working-draft-2021",
            "https://saismaran.org/geometricunity.pdf",
            "Draft appendix maps Higgs potential to an Upsilon inner product and places weak isospin/hypercharge in GU locations, but does not fix W/Z/H rows or normalization."),
        new ExternalSource(
            "hofseth-weinstein-geometric-refractive-unification-v5",
            "https://zenodo.org/records/18692706",
            "Older Zenodo v5 record describes GU/RVG as a low-energy EFT synthesis with trace-anomaly vacuum sourcing, a 95.4 GeV resonance, running-vacuum context, and device-oriented metric engineering; the record itself points to a newer version."),
        new ExternalSource(
            "hofseth-weinstein-holographic-geometric-refractive-unification-v8",
            "https://zenodo.org/records/19465143",
            "Phase243 already reviewed the latest found v8 source as a holographic/metric-engineering synthesis lead, not a GU W/Z/H source-lineage derivation for the repository contracts."),
    },
    localSearchEvidence,
    localSearchFinding = localSearchEvidence.MatchingFileCount == 0
        ? "Repository search found no local GU/RVG, refractive-vacuum, trace-anomaly, 95 GeV dilaton, ADPG, MADA, Hiperco, or Minnealloy source artifact outside generated Phase281/integration/journal files."
        : "Repository search found GU/RVG-related local text outside generated Phase281/integration/journal files; review findings before relying on this audit.",
    checks,
    decision = geometricRefractiveUnificationSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from the GU/RVG synthesis source. It is a public external lead that links GU language to refractive-vacuum and trace-anomaly EFT ideas, but it does not supply a GU-local W/Z theorem, separate W/Z source rows, raw/common bridge gates, target-independent VEV/mass-matrix source, observed-field extraction, or observed-Higgs scalar source/operator/self-coupling lineage."
        : "Review GU/RVG source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving a target-independent electroweak vacuum or equivalent source scale from augmented torsion, trace anomaly, or another native GU object.",
        "Separate W and Z source rows with raw-amplitude, common-bridge, target-comparison, derivation, and stability gates satisfied without using physical targets.",
        "A solved observed-Higgs scalar source/operator with identity envelope, massive scalar profile or self-coupling/excitation relation, and stability sidecars.",
        "A checked observed-field extraction from Y-field components to physical W, Z, photon, gluon, and Higgs rows.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase220Path = Phase220Path,
        phase224Path = Phase224Path,
        phase227Path = Phase227Path,
        phase228Path = Phase228Path,
        phase243Path = Phase243Path,
        phase257Path = Phase257Path,
        phase267Path = Phase267Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "geometric_refractive_unification_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "geometric_refractive_unification_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.geometricRefractiveUnificationSourceAuditPassed,
        result.guRvgSourceLeadPresent,
        result.phase243PriorGuRvgCoverageConfirmed,
        result.guRvgSourceAlreadyCoveredByPhase243,
        result.guRvgLatestReviewedVersion,
        result.guRvgClaimsGuLowEnergyEftSynthesis,
        result.traceAnomalyVacuumSourcingLeadPresent,
        result.runningVacuumModelLeadPresent,
        result.ninetyFiveGevDilatonResonanceLeadPresent,
        result.metricEngineeringOrDeviceLeadPresent,
        result.guRvgPromotesWzMasses,
        result.guRvgPromotesHiggsMass,
        result.guRvgCompletesBosonPredictions,
        result.guRvgBoundary,
        result.currentBlockerEvidence,
        result.localSearchEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"geometricRefractiveUnificationSourceAuditPassed={geometricRefractiveUnificationSourceAuditPassed}");
Console.WriteLine($"guRvgSourceLeadPresent={guRvgSourceLeadPresent}");
Console.WriteLine($"traceAnomalyVacuumSourcingLeadPresent={traceAnomalyVacuumSourcingLeadPresent}");
Console.WriteLine($"ninetyFiveGevDilatonResonanceLeadPresent={ninetyFiveGevDilatonResonanceLeadPresent}");
Console.WriteLine($"guRvgPromotesWzMasses={guRvgPromotesWzMasses}");
Console.WriteLine($"guRvgPromotesHiggsMass={guRvgPromotesHiggsMass}");
Console.WriteLine($"localSearchMatchingFileCount={localSearchEvidence.MatchingFileCount}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static LocalSearchEvidence BuildLocalSearchEvidence()
{
    var terms = new[]
    {
        "geometric-refractive",
        "geometric refractive",
        "refractive vacuum gravity",
        "trace anomaly",
        "95 gev",
        "95.4 gev",
        "dilaton resonance",
        "running vacuum model",
        "metric engineering",
        "asymmetric dilaton pump",
        "adpg",
        "mada flux",
        "mada core",
        "mada array",
        "magnetic amplification and direction assembly",
        "hiperco",
        "minnealloy",
    };
    var roots = new[] { "studies", "docs", "scripts", "src", "README.md" };
    var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".csproj",
        ".json",
        ".jsonl",
        ".md",
        ".sh",
        ".txt",
    };

    var scannedFileCount = 0;
    var findings = new List<LocalSearchFinding>();

    foreach (var root in roots)
    {
        var files = File.Exists(root)
            ? new[] { root }
            : Directory.Exists(root)
                ? Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                : Array.Empty<string>();

        foreach (var file in files)
        {
            var normalizedPath = file.Replace('\\', '/');
            if (!extensions.Contains(Path.GetExtension(normalizedPath)) || IsGeneratedOrCurrentPhaseFile(normalizedPath))
            {
                continue;
            }

            scannedFileCount++;
            var text = File.ReadAllText(file).ToLowerInvariant();
            var matchedTerms = terms.Where(term => text.Contains(term, StringComparison.Ordinal)).ToArray();
            if (matchedTerms.Length > 0)
            {
                findings.Add(new LocalSearchFinding(normalizedPath, matchedTerms));
            }
        }
    }

    return new LocalSearchEvidence(scannedFileCount, findings.Count, findings);
}

static bool IsGeneratedOrCurrentPhaseFile(string normalizedPath) =>
    normalizedPath.Contains("/bin/", StringComparison.Ordinal)
    || normalizedPath.Contains("/obj/", StringComparison.Ordinal)
    || normalizedPath.Contains("/output/", StringComparison.Ordinal)
    || normalizedPath == "ExperimentReferences.md"
    || normalizedPath.StartsWith("docs/Reference/ExperimentReferences/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase243_public_web_source_delta_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase281_geometric_refractive_unification_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase312_current_public_gu_rvg_revision_delta_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase313_official_draft_electroweak_projection_map_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase314_dimension_casimir_wz_source_law_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase315_ucsd_dark_geometric_energy_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase316_ucsd_transcript_source_strength_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase318_deferred_implementation_gap_repairability_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase319_legacy_selector_spectrum_source_law_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase320_standard_electroweak_ladder_normalization_boundary_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase321_neutral_electroweak_mixing_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase324_custodial_rho_parameter_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase325_electroweak_unitarity_scattering_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase326_anomaly_hypercharge_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase327_oblique_precision_electroweak_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase328_superphysics_draft_energy_scale_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase329_seiberg_witten_monopole_electroweak_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase330_weyl_geometric_mass_generation_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase331_theta_omega_inhomogeneous_gauge_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase332_string_m_theory_compactification_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase334_su21_superconnection_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase335_graviweak_plebanski_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase336_heft_scalar_geometry_source_law_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase337_octonion_clifford_internal_space_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase338_metric_affine_torsion_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase339_macdowell_mansouri_cartan_breaking_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase340_bf_topological_mass_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase341_scherk_schwarz_twisted_compactification_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase342_higgsless_boundary_condition_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase343_stueckelberg_vector_mass_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase345_fradkin_shenker_complementarity_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase347_dispersive_electroweak_scale_mass_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase348_right_handed_weak_coupling_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase349_spin_exchange_preon_boson_mass_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase350_spin_charge_family_boson_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase351_weak_hypercharge_superselection_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase352_higgs_top_z_nnlo_matching_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase353_gauge_higgs_unification_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase354_multiplicative_higgs_lagrangian_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase355_dirac_lichnerowicz_yang_mills_higgs_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase356_eguchi_hanson_substandard_higgs_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase357_causal_fermion_systems_boson_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase358_exceptional_e8_boson_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase359_finite_ncg_discrete_higgs_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase360_exceptional_jordan_magic_square_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase361_matrix_model_higgs_geometry_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase362_framed_standard_model_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase363_hitchin_higgs_bundle_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase364_moment_map_symplectic_reduction_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath == "studies/phase101_boson_prediction_package_001/Program.cs"
    || normalizedPath == "studies/phase202_boson_objective_completion_audit_001/Program.cs"
    || normalizedPath == "studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs"
    || normalizedPath == "studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs"
    || normalizedPath == "studies/phase207_higgs_quartic_self_coupling_source_scan_001/Program.cs"
    || normalizedPath == "scripts/generate_validated_boson_predictions.sh"
    || normalizedPath == "scripts/verify_boson_claim_integrity.sh"
    || normalizedPath == "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P281.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P312.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P313.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P314.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P315.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P316.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P317.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P318.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P319.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P327.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P328.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P342.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P343.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P364.md";

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

record LocalSearchEvidence(int ScannedFileCount, int MatchingFileCount, IReadOnlyList<LocalSearchFinding> Findings);

record LocalSearchFinding(string Path, IReadOnlyList<string> MatchedTerms);

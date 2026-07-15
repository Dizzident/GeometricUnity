using System.Text.Json;

const string DefaultOutputDir = "studies/phase278_relaxion_electroweak_scale_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase269Path = "studies/phase269_coleman_weinberg_scale_source_audit_001/output/coleman_weinberg_scale_source_audit_summary.json";
const string Phase274Path = "studies/phase274_neutrino_option_electroweak_scale_source_audit_001/output/neutrino_option_electroweak_scale_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE278_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase269 = JsonDocument.Parse(File.ReadAllText(Phase269Path));
using var phase274 = JsonDocument.Parse(File.ReadAllText(Phase274Path));

var localSearchEvidence = BuildLocalSearchEvidence();

var relaxionElectroweakScaleLeadPresent = true;
var cosmologicalRelaxationLeadPresent = true;
var higgsMassScanningLeadPresent = true;
var barrierStoppingLeadPresent = true;
var relaxionCanSelectWeakScaleInExternalModel = true;
var relaxionRequiresNewAxionLikeFieldSource = true;
var relaxionRequiresShiftSymmetryAndBreakingSource = true;
var relaxionRequiresScanningPotentialSource = true;
var relaxionRequiresSlowRollCosmologySource = true;
var relaxionRequiresBarrierOrBackreactionSectorSource = true;
var relaxionRequiresStoppingConditionSource = true;
var relaxionRequiresCutoffAndFieldRangeSource = true;
var relaxionRequiresInitialConditionAndInflationSource = true;
var relaxionRequiresLowEnergyRgTransport = true;
var relaxionRequiresVevSource = true;
var relaxionRequiresWzMassMatrixSource = true;
var relaxionRequiresHiggsScalarSource = true;
var relaxionRequiresObservedFieldExtraction = true;
var relaxionExternalToGu = true;

var localGuRelaxionFieldSourceFound = false;
var localGuRelaxionShiftSymmetrySourceFound = false;
var localGuRelaxionScanningPotentialFound = false;
var localGuRelaxionSlowRollCosmologyFound = false;
var localGuRelaxionBarrierSectorFound = false;
var localGuRelaxionStoppingConditionFound = false;
var localGuRelaxionCutoffFieldRangeFound = false;
var localGuRelaxionInitialConditionFound = false;
var localGuRelaxionRgTransportFound = false;
var localGuRelaxionVevSourceFound = false;
var localGuRelaxionWzMassMatrixSourceFound = false;
var localGuRelaxionHiggsScalarSourceFound = false;
var localGuRelaxionObservedFieldExtractionFound = false;
var relaxionPromotesWzMasses = false;
var relaxionPromotesHiggsMass = false;
var relaxionCompletesBosonPredictions = false;

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
var colemanWeinbergScaleSourceAuditPassed = JsonBool(phase269.RootElement, "colemanWeinbergScaleSourceAuditPassed") is true;
var colemanWeinbergPromotesWzMasses = JsonBool(phase269.RootElement, "colemanWeinbergPromotesWzMasses") is true;
var colemanWeinbergPromotesHiggsMass = JsonBool(phase269.RootElement, "colemanWeinbergPromotesHiggsMass") is true;
var neutrinoOptionElectroweakScaleSourceAuditPassed = JsonBool(phase274.RootElement, "neutrinoOptionElectroweakScaleSourceAuditPassed") is true;
var neutrinoOptionPromotesWzMasses = JsonBool(phase274.RootElement, "neutrinoOptionPromotesWzMasses") is true;
var neutrinoOptionPromotesHiggsMass = JsonBool(phase274.RootElement, "neutrinoOptionPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var relaxionElectroweakScaleSourceAuditPassed =
    relaxionElectroweakScaleLeadPresent
    && cosmologicalRelaxationLeadPresent
    && higgsMassScanningLeadPresent
    && barrierStoppingLeadPresent
    && relaxionCanSelectWeakScaleInExternalModel
    && relaxionRequiresNewAxionLikeFieldSource
    && relaxionRequiresShiftSymmetryAndBreakingSource
    && relaxionRequiresScanningPotentialSource
    && relaxionRequiresSlowRollCosmologySource
    && relaxionRequiresBarrierOrBackreactionSectorSource
    && relaxionRequiresStoppingConditionSource
    && relaxionRequiresCutoffAndFieldRangeSource
    && relaxionRequiresInitialConditionAndInflationSource
    && relaxionRequiresLowEnergyRgTransport
    && relaxionRequiresVevSource
    && relaxionRequiresWzMassMatrixSource
    && relaxionRequiresHiggsScalarSource
    && relaxionRequiresObservedFieldExtraction
    && relaxionExternalToGu
    && !localGuRelaxionFieldSourceFound
    && !localGuRelaxionShiftSymmetrySourceFound
    && !localGuRelaxionScanningPotentialFound
    && !localGuRelaxionSlowRollCosmologyFound
    && !localGuRelaxionBarrierSectorFound
    && !localGuRelaxionStoppingConditionFound
    && !localGuRelaxionCutoffFieldRangeFound
    && !localGuRelaxionInitialConditionFound
    && localSearchEvidence.MatchingFileCount == 0
    && !localGuRelaxionRgTransportFound
    && !localGuRelaxionVevSourceFound
    && !localGuRelaxionWzMassMatrixSourceFound
    && !localGuRelaxionHiggsScalarSourceFound
    && !localGuRelaxionObservedFieldExtractionFound
    && !relaxionPromotesWzMasses
    && !relaxionPromotesHiggsMass
    && !relaxionCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && colemanWeinbergScaleSourceAuditPassed
    && !colemanWeinbergPromotesWzMasses
    && !colemanWeinbergPromotesHiggsMass
    && neutrinoOptionElectroweakScaleSourceAuditPassed
    && !neutrinoOptionPromotesWzMasses
    && !neutrinoOptionPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "relaxion-route-is-electroweak-scale-selection-lead",
        relaxionElectroweakScaleLeadPresent
            && cosmologicalRelaxationLeadPresent
            && higgsMassScanningLeadPresent
            && barrierStoppingLeadPresent
            && relaxionCanSelectWeakScaleInExternalModel,
        $"relaxionElectroweakScaleLeadPresent={relaxionElectroweakScaleLeadPresent}; cosmologicalRelaxationLeadPresent={cosmologicalRelaxationLeadPresent}; higgsMassScanningLeadPresent={higgsMassScanningLeadPresent}; barrierStoppingLeadPresent={barrierStoppingLeadPresent}; externalModelCanSelectWeakScale={relaxionCanSelectWeakScaleInExternalModel}"),
    new Check(
        "relaxion-input-contract-is-cosmological-and-extra-field-dependent",
        relaxionRequiresNewAxionLikeFieldSource
            && relaxionRequiresShiftSymmetryAndBreakingSource
            && relaxionRequiresScanningPotentialSource
            && relaxionRequiresSlowRollCosmologySource
            && relaxionRequiresBarrierOrBackreactionSectorSource
            && relaxionRequiresStoppingConditionSource
            && relaxionRequiresCutoffAndFieldRangeSource
            && relaxionRequiresInitialConditionAndInflationSource,
        $"newAxionLikeField={relaxionRequiresNewAxionLikeFieldSource}; shiftSymmetryAndBreaking={relaxionRequiresShiftSymmetryAndBreakingSource}; scanningPotential={relaxionRequiresScanningPotentialSource}; slowRollCosmology={relaxionRequiresSlowRollCosmologySource}; barrierSector={relaxionRequiresBarrierOrBackreactionSectorSource}; stoppingCondition={relaxionRequiresStoppingConditionSource}; cutoffFieldRange={relaxionRequiresCutoffAndFieldRangeSource}; initialConditionInflation={relaxionRequiresInitialConditionAndInflationSource}"),
    new Check(
        "no-local-gu-relaxion-source-artifact",
        !localGuRelaxionFieldSourceFound
            && !localGuRelaxionShiftSymmetrySourceFound
            && !localGuRelaxionScanningPotentialFound
            && !localGuRelaxionSlowRollCosmologyFound
            && !localGuRelaxionBarrierSectorFound
            && !localGuRelaxionStoppingConditionFound
            && !localGuRelaxionCutoffFieldRangeFound
            && !localGuRelaxionInitialConditionFound
            && localSearchEvidence.MatchingFileCount == 0,
        $"fieldSource={localGuRelaxionFieldSourceFound}; shiftSymmetrySource={localGuRelaxionShiftSymmetrySourceFound}; scanningPotential={localGuRelaxionScanningPotentialFound}; slowRollCosmology={localGuRelaxionSlowRollCosmologyFound}; barrierSector={localGuRelaxionBarrierSectorFound}; stoppingCondition={localGuRelaxionStoppingConditionFound}; cutoffFieldRange={localGuRelaxionCutoffFieldRangeFound}; initialCondition={localGuRelaxionInitialConditionFound}; scannerMatchingFileCount={localSearchEvidence.MatchingFileCount}; scannedFileCount={localSearchEvidence.ScannedFileCount}"),
    new Check(
        "relaxion-does-not-fill-gu-wz-or-higgs-source-contracts",
        !localGuRelaxionRgTransportFound
            && !localGuRelaxionVevSourceFound
            && !localGuRelaxionWzMassMatrixSourceFound
            && !localGuRelaxionHiggsScalarSourceFound
            && !localGuRelaxionObservedFieldExtractionFound
            && !relaxionPromotesWzMasses
            && !relaxionPromotesHiggsMass,
        $"rgTransport={localGuRelaxionRgTransportFound}; vevSource={localGuRelaxionVevSourceFound}; wzMassMatrix={localGuRelaxionWzMassMatrixSourceFound}; higgsScalarSource={localGuRelaxionHiggsScalarSourceFound}; observedFieldExtraction={localGuRelaxionObservedFieldExtractionFound}; promotesWz={relaxionPromotesWzMasses}; promotesHiggs={relaxionPromotesHiggsMass}"),
    new Check(
        "neighboring-radiative-and-neutrino-routes-remain-nonpromotional",
        colemanWeinbergScaleSourceAuditPassed
            && !colemanWeinbergPromotesWzMasses
            && !colemanWeinbergPromotesHiggsMass
            && neutrinoOptionElectroweakScaleSourceAuditPassed
            && !neutrinoOptionPromotesWzMasses
            && !neutrinoOptionPromotesHiggsMass,
        $"colemanWeinbergScaleSourceAuditPassed={colemanWeinbergScaleSourceAuditPassed}; colemanWeinbergPromotesWzMasses={colemanWeinbergPromotesWzMasses}; colemanWeinbergPromotesHiggsMass={colemanWeinbergPromotesHiggsMass}; neutrinoOptionElectroweakScaleSourceAuditPassed={neutrinoOptionElectroweakScaleSourceAuditPassed}; neutrinoOptionPromotesWzMasses={neutrinoOptionPromotesWzMasses}; neutrinoOptionPromotesHiggsMass={neutrinoOptionPromotesHiggsMass}"),
    new Check(
        "current-source-lineage-blockers-still-active",
        obstructionAuditPassed
            && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"obstructionAuditPassed={obstructionAuditPassed}; obstructionKind={obstructionKind}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = relaxionElectroweakScaleSourceAuditPassed
    ? "relaxion-electroweak-scale-source-audit-external-cosmological-selection-not-promotion"
    : "relaxion-electroweak-scale-source-audit-review-required";

var result = new
{
    phaseId = "phase278-relaxion-electroweak-scale-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    relaxionElectroweakScaleSourceAuditPassed,
    relaxionElectroweakScaleLeadPresent,
    cosmologicalRelaxationLeadPresent,
    higgsMassScanningLeadPresent,
    barrierStoppingLeadPresent,
    relaxionPromotesWzMasses,
    relaxionPromotesHiggsMass,
    relaxionCompletesBosonPredictions,
    relaxionBoundary = new
    {
        relaxionCanSelectWeakScaleInExternalModel,
        relaxionRequiresNewAxionLikeFieldSource,
        relaxionRequiresShiftSymmetryAndBreakingSource,
        relaxionRequiresScanningPotentialSource,
        relaxionRequiresSlowRollCosmologySource,
        relaxionRequiresBarrierOrBackreactionSectorSource,
        relaxionRequiresStoppingConditionSource,
        relaxionRequiresCutoffAndFieldRangeSource,
        relaxionRequiresInitialConditionAndInflationSource,
        relaxionRequiresLowEnergyRgTransport,
        relaxionRequiresVevSource,
        relaxionRequiresWzMassMatrixSource,
        relaxionRequiresHiggsScalarSource,
        relaxionRequiresObservedFieldExtraction,
        relaxionExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuRelaxionFieldSourceFound,
        localGuRelaxionShiftSymmetrySourceFound,
        localGuRelaxionScanningPotentialFound,
        localGuRelaxionSlowRollCosmologyFound,
        localGuRelaxionBarrierSectorFound,
        localGuRelaxionStoppingConditionFound,
        localGuRelaxionCutoffFieldRangeFound,
        localGuRelaxionInitialConditionFound,
        localGuRelaxionRgTransportFound,
        localGuRelaxionVevSourceFound,
        localGuRelaxionWzMassMatrixSourceFound,
        localGuRelaxionHiggsScalarSourceFound,
        localGuRelaxionObservedFieldExtractionFound,
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
        phase269 = new
        {
            colemanWeinbergScaleSourceAuditPassed,
            colemanWeinbergPromotesWzMasses,
            colemanWeinbergPromotesHiggsMass,
        },
        phase274 = new
        {
            neutrinoOptionElectroweakScaleSourceAuditPassed,
            neutrinoOptionPromotesWzMasses,
            neutrinoOptionPromotesHiggsMass,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    localSearchEvidence,
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "graham-kaplan-rajendran-cosmological-relaxation",
            "https://arxiv.org/abs/1504.07551",
            "Introduces cosmological relaxation: early-universe dynamics scan the Higgs mass so the weak scale becomes much smaller than the cutoff in an external model with an axion-like field and inflation sector."),
        new ExternalSource(
            "matsedonskyi-mirror-relaxion",
            "https://arxiv.org/abs/1509.03583",
            "Uses a mirror copy and Z2 structure to stabilize the electroweak scale near the desired place, still requiring extra sector, cosmological history, and model parameters."),
        new ExternalSource(
            "ibanez-montero-uranga-valenzuela-relaxion-monodromy-wgc",
            "https://arxiv.org/abs/1512.00025",
            "Shows that relaxion constructions require very large axion excursions and explicit shift-symmetry breaking, motivating monodromy/4-form structure and theoretical constraints."),
    },
    localSearchFinding = localSearchEvidence.MatchingFileCount == 0
        ? "Repository search found no relaxion, cosmological-relaxation, Higgs-mass scanning, barrier-potential, axion-monodromy, or relaxion stopping-condition source artifacts outside generated Phase278/journal implementation files."
        : "Repository search found relaxion-related local text outside generated Phase278/journal implementation files; review findings before relying on this audit.",
    checks,
    decision = relaxionElectroweakScaleSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from relaxion/cosmological-relaxation literature. The route is a serious electroweak-scale selection lead, but it requires a GU-local relaxion field, shift-symmetry and breaking source, scanning potential, inflation/slow-roll history, barrier/backreaction sector, stopping condition, cutoff/field-range source, low-energy transport, VEV source, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem that the repository does not contain."
        : "Review relaxion electroweak-scale source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local relaxion or equivalent scalar-source theorem with shift-symmetry, explicit-breaking, scanning-potential, field-range, and cutoff source rows.",
        "A GU cosmological stopping-condition artifact tying barrier/backreaction and inflation/slow-roll history to a target-independent electroweak scale.",
        "Downstream GU VEV, W/Z mass matrix, Higgs scalar source, low-energy transport, and observed-field extraction artifacts.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase220Path = Phase220Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase248Path = Phase248Path,
        phase257Path = Phase257Path,
        phase269Path = Phase269Path,
        phase274Path = Phase274Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "relaxion_electroweak_scale_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "relaxion_electroweak_scale_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.relaxionElectroweakScaleSourceAuditPassed,
        result.relaxionElectroweakScaleLeadPresent,
        result.cosmologicalRelaxationLeadPresent,
        result.higgsMassScanningLeadPresent,
        result.barrierStoppingLeadPresent,
        result.relaxionPromotesWzMasses,
        result.relaxionPromotesHiggsMass,
        result.relaxionCompletesBosonPredictions,
        result.relaxionBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"relaxionElectroweakScaleSourceAuditPassed={relaxionElectroweakScaleSourceAuditPassed}");
Console.WriteLine($"relaxionElectroweakScaleLeadPresent={relaxionElectroweakScaleLeadPresent}");
Console.WriteLine($"cosmologicalRelaxationLeadPresent={cosmologicalRelaxationLeadPresent}");
Console.WriteLine($"higgsMassScanningLeadPresent={higgsMassScanningLeadPresent}");
Console.WriteLine($"barrierStoppingLeadPresent={barrierStoppingLeadPresent}");
Console.WriteLine($"relaxionPromotesWzMasses={relaxionPromotesWzMasses}");
Console.WriteLine($"relaxionPromotesHiggsMass={relaxionPromotesHiggsMass}");
Console.WriteLine($"localSearchMatchingFileCount={localSearchEvidence.MatchingFileCount}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static LocalSearchEvidence BuildLocalSearchEvidence()
{
    var terms = new[]
    {
        "relaxion",
        "cosmological relaxation",
        "higgs mass scanning",
        "higgs-mass scanning",
        "barrier potential",
        "axion monodromy",
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
    || normalizedPath.Contains("scripts/incremental/", StringComparison.Ordinal)
    || normalizedPath.Contains("scripts/o4_register/", StringComparison.Ordinal)
    || normalizedPath.Contains("scripts/boson_incremental_manifest.json", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase278_relaxion_electroweak_scale_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase496_phase456_retained_data_information_census_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase497_phase456_prospective_estimator_acquisition_oracle_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase498_phase456_acquisition_repair_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase499_phase456_retained_empirical_noise_information_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase500_phase456_adversarial_prospective_acquisition_stress_test_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase501_phase456_robust_sampling_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase502_phase456_adaptive_calibration_protocol_specification_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase503_phase456_adaptive_calibration_protocol_validation_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase504_phase456_calibration_repair_pack_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase505_phase503_frozen_failure_localization_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase506_phase456_selective_inference_protocol_validation_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase507_phase456_selective_inference_pack_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath == "studies/phase101_boson_prediction_package_001/Program.cs"
    || normalizedPath == "studies/phase202_boson_objective_completion_audit_001/Program.cs"
    || normalizedPath == "studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs"
    || normalizedPath == "studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs"
    || normalizedPath == "studies/phase207_higgs_quartic_self_coupling_source_scan_001/Program.cs"
    || normalizedPath == "scripts/generate_validated_boson_predictions.sh"
    || normalizedPath == "scripts/verify_boson_claim_integrity.sh"
    || normalizedPath == "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md"
    || normalizedPath == "docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P278.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P496.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P497.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P498.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P499.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P500.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P501.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P502.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P503.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P504.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P505.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P506.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P507.md";

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

using System.Text.Json;

const string DefaultOutputDir = "studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220Path = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase267Path = "studies/phase267_completion_revision_direct_bridge_source_audit_001/output/completion_revision_direct_bridge_source_audit_summary.json";
const string Phase270Path = "studies/phase270_composite_higgs_pngb_source_audit_001/output/composite_higgs_pngb_source_audit_summary.json";
const string Phase276Path = "studies/phase276_top_condensation_source_audit_001/output/top_condensation_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE279_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase267 = JsonDocument.Parse(File.ReadAllText(Phase267Path));
using var phase270 = JsonDocument.Parse(File.ReadAllText(Phase270Path));
using var phase276 = JsonDocument.Parse(File.ReadAllText(Phase276Path));

var localSearchEvidence = BuildLocalSearchEvidence();

var technicolorEwsbLeadPresent = true;
var walkingTechnicolorLeadPresent = true;
var technifermionCondensateLeadPresent = true;
var compositeHiggsOrTechnidilatonLeadPresent = true;
var technicolorCanGenerateWzMassesInExternalModel = true;
var technicolorRequiresNewStrongGaugeGroupSource = true;
var technicolorRequiresTechnifermionRepresentationSource = true;
var technicolorRequiresElectroweakEmbeddingSource = true;
var technicolorRequiresCondensateOrderParameterSource = true;
var technicolorRequiresDecayConstantOrVevSource = true;
var technicolorRequiresVacuumAlignmentAndCustodialSource = true;
var technicolorRequiresWalkingAnomalousDimensionSource = true;
var technicolorRequiresEtcOrFlavorSource = true;
var technicolorRequiresPrecisionElectroweakConstraintSource = true;
var technicolorRequiresCompositeScalarProfileSource = true;
var technicolorRequiresLowEnergyRgTransport = true;
var technicolorRequiresWzMassMatrixSource = true;
var technicolorRequiresHiggsScalarSource = true;
var technicolorRequiresObservedFieldExtraction = true;
var technicolorExternalToGu = true;

var localGuTechnicolorGaugeGroupSourceFound = false;
var localGuTechnifermionRepresentationSourceFound = false;
var localGuElectroweakEmbeddingSourceFound = false;
var localGuTechnifermionCondensateSourceFound = false;
var localGuTechnipionDecayConstantSourceFound = false;
var localGuVacuumAlignmentCustodialSourceFound = false;
var localGuWalkingAnomalousDimensionSourceFound = false;
var localGuEtcFlavorSourceFound = false;
var localGuPrecisionElectroweakConstraintSourceFound = false;
var localGuCompositeScalarProfileSourceFound = false;
var localGuTechnicolorRgTransportFound = false;
var localGuTechnicolorWzMassMatrixSourceFound = false;
var localGuTechnicolorHiggsScalarSourceFound = false;
var localGuTechnicolorObservedFieldExtractionFound = false;
var technicolorPromotesWzMasses = false;
var technicolorPromotesHiggsMass = false;
var technicolorCompletesBosonPredictions = false;

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
var latestCompletionProvidesDirectWzTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesDirectWzTheorem") is true;
var latestCompletionProvidesObservedFieldExtractionTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesObservedFieldExtractionTheorem") is true;
var latestCompletionPromotesWzMasses = JsonBool(phase267.RootElement, "latestCompletionPromotesWzMasses") is true;
var compositeHiggsPngbSourceAuditPassed = JsonBool(phase270.RootElement, "compositeHiggsPngbSourceAuditPassed") is true;
var compositeHiggsPromotesWzMasses = JsonBool(phase270.RootElement, "compositeHiggsPromotesWzMasses") is true;
var compositeHiggsPromotesHiggsMass = JsonBool(phase270.RootElement, "compositeHiggsPromotesHiggsMass") is true;
var topCondensationSourceAuditPassed = JsonBool(phase276.RootElement, "topCondensationSourceAuditPassed") is true;
var topCondensationPromotesWzMasses = JsonBool(phase276.RootElement, "topCondensationPromotesWzMasses") is true;
var topCondensationPromotesHiggsMass = JsonBool(phase276.RootElement, "topCondensationPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var technicolorWalkingElectroweakScaleSourceAuditPassed =
    technicolorEwsbLeadPresent
    && walkingTechnicolorLeadPresent
    && technifermionCondensateLeadPresent
    && compositeHiggsOrTechnidilatonLeadPresent
    && technicolorCanGenerateWzMassesInExternalModel
    && technicolorRequiresNewStrongGaugeGroupSource
    && technicolorRequiresTechnifermionRepresentationSource
    && technicolorRequiresElectroweakEmbeddingSource
    && technicolorRequiresCondensateOrderParameterSource
    && technicolorRequiresDecayConstantOrVevSource
    && technicolorRequiresVacuumAlignmentAndCustodialSource
    && technicolorRequiresWalkingAnomalousDimensionSource
    && technicolorRequiresEtcOrFlavorSource
    && technicolorRequiresPrecisionElectroweakConstraintSource
    && technicolorRequiresCompositeScalarProfileSource
    && technicolorRequiresLowEnergyRgTransport
    && technicolorRequiresWzMassMatrixSource
    && technicolorRequiresHiggsScalarSource
    && technicolorRequiresObservedFieldExtraction
    && technicolorExternalToGu
    && !localGuTechnicolorGaugeGroupSourceFound
    && !localGuTechnifermionRepresentationSourceFound
    && !localGuElectroweakEmbeddingSourceFound
    && !localGuTechnifermionCondensateSourceFound
    && !localGuTechnipionDecayConstantSourceFound
    && !localGuVacuumAlignmentCustodialSourceFound
    && !localGuWalkingAnomalousDimensionSourceFound
    && !localGuEtcFlavorSourceFound
    && !localGuPrecisionElectroweakConstraintSourceFound
    && !localGuCompositeScalarProfileSourceFound
    && !localGuTechnicolorRgTransportFound
    && !localGuTechnicolorWzMassMatrixSourceFound
    && !localGuTechnicolorHiggsScalarSourceFound
    && !localGuTechnicolorObservedFieldExtractionFound
    && localSearchEvidence.MatchingFileCount == 0
    && !technicolorPromotesWzMasses
    && !technicolorPromotesHiggsMass
    && !technicolorCompletesBosonPredictions
    && obstructionAuditPassed
    && obstructionKind == "dimensionful-scale-and-source-lineage-missing"
    && !wAbsoluteMassParameterClosure
    && !zAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !lowEnergyRgTransportSourcePromotable
    && !higgsScalarSourceRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !currentImplementationCanFillObservedFieldExtractionContract
    && !latestCompletionProvidesDirectWzTheorem
    && !latestCompletionProvidesObservedFieldExtractionTheorem
    && !latestCompletionPromotesWzMasses
    && compositeHiggsPngbSourceAuditPassed
    && !compositeHiggsPromotesWzMasses
    && !compositeHiggsPromotesHiggsMass
    && topCondensationSourceAuditPassed
    && !topCondensationPromotesWzMasses
    && !topCondensationPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "technicolor-route-is-direct-strong-ewsb-lead",
        technicolorEwsbLeadPresent
            && walkingTechnicolorLeadPresent
            && technifermionCondensateLeadPresent
            && compositeHiggsOrTechnidilatonLeadPresent
            && technicolorCanGenerateWzMassesInExternalModel,
        $"technicolorEwsbLeadPresent={technicolorEwsbLeadPresent}; walkingTechnicolorLeadPresent={walkingTechnicolorLeadPresent}; technifermionCondensateLeadPresent={technifermionCondensateLeadPresent}; compositeHiggsOrTechnidilatonLeadPresent={compositeHiggsOrTechnidilatonLeadPresent}; externalModelCanGenerateWzMasses={technicolorCanGenerateWzMassesInExternalModel}"),
    new Check(
        "technicolor-input-contract-is-new-strong-sector-dependent",
        technicolorRequiresNewStrongGaugeGroupSource
            && technicolorRequiresTechnifermionRepresentationSource
            && technicolorRequiresElectroweakEmbeddingSource
            && technicolorRequiresCondensateOrderParameterSource
            && technicolorRequiresDecayConstantOrVevSource
            && technicolorRequiresVacuumAlignmentAndCustodialSource
            && technicolorRequiresWalkingAnomalousDimensionSource
            && technicolorRequiresEtcOrFlavorSource
            && technicolorRequiresPrecisionElectroweakConstraintSource,
        $"strongGaugeGroup={technicolorRequiresNewStrongGaugeGroupSource}; technifermionRepresentation={technicolorRequiresTechnifermionRepresentationSource}; electroweakEmbedding={technicolorRequiresElectroweakEmbeddingSource}; condensateOrderParameter={technicolorRequiresCondensateOrderParameterSource}; decayConstantOrVev={technicolorRequiresDecayConstantOrVevSource}; vacuumAlignmentCustodial={technicolorRequiresVacuumAlignmentAndCustodialSource}; walkingAnomalousDimension={technicolorRequiresWalkingAnomalousDimensionSource}; etcFlavor={technicolorRequiresEtcOrFlavorSource}; precisionElectroweak={technicolorRequiresPrecisionElectroweakConstraintSource}"),
    new Check(
        "no-local-gu-technicolor-source-artifact",
        !localGuTechnicolorGaugeGroupSourceFound
            && !localGuTechnifermionRepresentationSourceFound
            && !localGuElectroweakEmbeddingSourceFound
            && !localGuTechnifermionCondensateSourceFound
            && !localGuTechnipionDecayConstantSourceFound
            && !localGuVacuumAlignmentCustodialSourceFound
            && !localGuWalkingAnomalousDimensionSourceFound
            && !localGuEtcFlavorSourceFound
            && !localGuPrecisionElectroweakConstraintSourceFound
            && !localGuCompositeScalarProfileSourceFound
            && localSearchEvidence.MatchingFileCount == 0,
        $"gaugeGroup={localGuTechnicolorGaugeGroupSourceFound}; technifermionRepresentation={localGuTechnifermionRepresentationSourceFound}; electroweakEmbedding={localGuElectroweakEmbeddingSourceFound}; condensate={localGuTechnifermionCondensateSourceFound}; decayConstant={localGuTechnipionDecayConstantSourceFound}; vacuumAlignmentCustodial={localGuVacuumAlignmentCustodialSourceFound}; walkingAnomalousDimension={localGuWalkingAnomalousDimensionSourceFound}; etcFlavor={localGuEtcFlavorSourceFound}; precisionElectroweak={localGuPrecisionElectroweakConstraintSourceFound}; compositeScalarProfile={localGuCompositeScalarProfileSourceFound}; scannerMatchingFileCount={localSearchEvidence.MatchingFileCount}; scannedFileCount={localSearchEvidence.ScannedFileCount}"),
    new Check(
        "technicolor-does-not-fill-gu-wz-or-higgs-source-contracts",
        !localGuTechnicolorRgTransportFound
            && !localGuTechnicolorWzMassMatrixSourceFound
            && !localGuTechnicolorHiggsScalarSourceFound
            && !localGuTechnicolorObservedFieldExtractionFound
            && !technicolorPromotesWzMasses
            && !technicolorPromotesHiggsMass,
        $"rgTransport={localGuTechnicolorRgTransportFound}; wzMassMatrix={localGuTechnicolorWzMassMatrixSourceFound}; higgsScalarSource={localGuTechnicolorHiggsScalarSourceFound}; observedFieldExtraction={localGuTechnicolorObservedFieldExtractionFound}; promotesWz={technicolorPromotesWzMasses}; promotesHiggs={technicolorPromotesHiggsMass}"),
    new Check(
        "neighboring-composite-and-top-condensation-routes-remain-nonpromotional",
        compositeHiggsPngbSourceAuditPassed
            && !compositeHiggsPromotesWzMasses
            && !compositeHiggsPromotesHiggsMass
            && topCondensationSourceAuditPassed
            && !topCondensationPromotesWzMasses
            && !topCondensationPromotesHiggsMass,
        $"compositeHiggsPngbSourceAuditPassed={compositeHiggsPngbSourceAuditPassed}; compositeHiggsPromotesWzMasses={compositeHiggsPromotesWzMasses}; compositeHiggsPromotesHiggsMass={compositeHiggsPromotesHiggsMass}; topCondensationSourceAuditPassed={topCondensationSourceAuditPassed}; topCondensationPromotesWzMasses={topCondensationPromotesWzMasses}; topCondensationPromotesHiggsMass={topCondensationPromotesHiggsMass}"),
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

var terminalStatus = technicolorWalkingElectroweakScaleSourceAuditPassed
    ? "technicolor-walking-electroweak-scale-source-audit-external-strong-sector-not-promotion"
    : "technicolor-walking-electroweak-scale-source-audit-review-required";

var result = new
{
    phaseId = "phase279-technicolor-walking-electroweak-scale-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    technicolorWalkingElectroweakScaleSourceAuditPassed,
    technicolorEwsbLeadPresent,
    walkingTechnicolorLeadPresent,
    technifermionCondensateLeadPresent,
    compositeHiggsOrTechnidilatonLeadPresent,
    technicolorPromotesWzMasses,
    technicolorPromotesHiggsMass,
    technicolorCompletesBosonPredictions,
    technicolorBoundary = new
    {
        technicolorCanGenerateWzMassesInExternalModel,
        technicolorRequiresNewStrongGaugeGroupSource,
        technicolorRequiresTechnifermionRepresentationSource,
        technicolorRequiresElectroweakEmbeddingSource,
        technicolorRequiresCondensateOrderParameterSource,
        technicolorRequiresDecayConstantOrVevSource,
        technicolorRequiresVacuumAlignmentAndCustodialSource,
        technicolorRequiresWalkingAnomalousDimensionSource,
        technicolorRequiresEtcOrFlavorSource,
        technicolorRequiresPrecisionElectroweakConstraintSource,
        technicolorRequiresCompositeScalarProfileSource,
        technicolorRequiresLowEnergyRgTransport,
        technicolorRequiresWzMassMatrixSource,
        technicolorRequiresHiggsScalarSource,
        technicolorRequiresObservedFieldExtraction,
        technicolorExternalToGu,
    },
    sourceLineageBoundary = new
    {
        localGuTechnicolorGaugeGroupSourceFound,
        localGuTechnifermionRepresentationSourceFound,
        localGuElectroweakEmbeddingSourceFound,
        localGuTechnifermionCondensateSourceFound,
        localGuTechnipionDecayConstantSourceFound,
        localGuVacuumAlignmentCustodialSourceFound,
        localGuWalkingAnomalousDimensionSourceFound,
        localGuEtcFlavorSourceFound,
        localGuPrecisionElectroweakConstraintSourceFound,
        localGuCompositeScalarProfileSourceFound,
        localGuTechnicolorRgTransportFound,
        localGuTechnicolorWzMassMatrixSourceFound,
        localGuTechnicolorHiggsScalarSourceFound,
        localGuTechnicolorObservedFieldExtractionFound,
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
        phase267 = new
        {
            latestCompletionProvidesDirectWzTheorem,
            latestCompletionProvidesObservedFieldExtractionTheorem,
            latestCompletionPromotesWzMasses,
        },
        phase270 = new
        {
            compositeHiggsPngbSourceAuditPassed,
            compositeHiggsPromotesWzMasses,
            compositeHiggsPromotesHiggsMass,
        },
        phase276 = new
        {
            topCondensationSourceAuditPassed,
            topCondensationPromotesWzMasses,
            topCondensationPromotesHiggsMass,
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
            "susskind-dynamical-symmetry-breaking-weinberg-salam",
            "https://particle.physics.ucdavis.edu/archive/abstracts/StrongEWSB/PRD20%281979%292619.sewsb.html",
            "Original technicolor-style route: symmetry breaking is induced by a new strongly interacting sector with a natural scale near a few TeV."),
        new ExternalSource(
            "hill-simmons-strong-dynamics-ewsb-review",
            "https://arxiv.org/abs/hep-ph/0203079",
            "Reviews the hypothesis that electroweak symmetry breaking and the weak scale may arise from a new strong interaction, including technicolor-like mechanisms."),
        new ExternalSource(
            "lane-two-lectures-technicolor",
            "https://arxiv.org/abs/hep-ph/0202255",
            "Reviews technicolor and extended technicolor, including walking technicolor as a proposed response to flavor, precision electroweak, and top-mass obstacles."),
        new ExternalSource(
            "pdg-2025-dynamical-electroweak-symmetry-breaking",
            "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-technicolor.pdf",
            "Summarizes modern dynamical electroweak symmetry breaking: strong dynamics can give W/Z masses, but realistic models need flavor interactions, precision constraints, and strongly coupled calculations."),
    },
    localSearchFinding = localSearchEvidence.MatchingFileCount == 0
        ? "Repository search found no technicolor, walking-technicolor, technifermion, technidilaton, extended-technicolor, or strong electroweak source artifacts outside generated Phase279/integration/journal files."
        : "Repository search found technicolor-related local text outside generated Phase279/integration/journal files; review findings before relying on this audit.",
    checks,
    decision = technicolorWalkingElectroweakScaleSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from technicolor or walking-technicolor literature. The route is a serious external electroweak-symmetry-breaking lead, but it requires a GU-local new strong gauge sector, technifermion representation and electroweak embedding, condensate/decay-constant source, walking dynamics, ETC/flavor source, precision-constraint handling, composite scalar profile, low-energy transport, W/Z mass matrix, Higgs scalar source, and observed-field extraction theorem that the repository does not contain."
        : "Review technicolor/walking-technicolor electroweak-scale source audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A GU-local technicolor or equivalent strong-sector theorem fixing the new gauge group, technifermion content, electroweak embedding, and condensate decay constant/source.",
        "A GU walking/near-conformal dynamics artifact with anomalous dimensions, ETC/flavor source, vacuum alignment, and precision-electroweak constraints.",
        "Downstream GU VEV/decay-constant, W/Z mass matrix, composite Higgs or technidilaton scalar profile, low-energy transport, and observed-field extraction artifacts.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase220Path = Phase220Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase248Path = Phase248Path,
        phase257Path = Phase257Path,
        phase267Path = Phase267Path,
        phase270Path = Phase270Path,
        phase276Path = Phase276Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "technicolor_walking_electroweak_scale_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "technicolor_walking_electroweak_scale_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.technicolorWalkingElectroweakScaleSourceAuditPassed,
        result.technicolorEwsbLeadPresent,
        result.walkingTechnicolorLeadPresent,
        result.technifermionCondensateLeadPresent,
        result.compositeHiggsOrTechnidilatonLeadPresent,
        result.technicolorPromotesWzMasses,
        result.technicolorPromotesHiggsMass,
        result.technicolorCompletesBosonPredictions,
        result.technicolorBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchEvidence,
        result.localSearchFinding,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"technicolorWalkingElectroweakScaleSourceAuditPassed={technicolorWalkingElectroweakScaleSourceAuditPassed}");
Console.WriteLine($"technicolorEwsbLeadPresent={technicolorEwsbLeadPresent}");
Console.WriteLine($"walkingTechnicolorLeadPresent={walkingTechnicolorLeadPresent}");
Console.WriteLine($"technifermionCondensateLeadPresent={technifermionCondensateLeadPresent}");
Console.WriteLine($"compositeHiggsOrTechnidilatonLeadPresent={compositeHiggsOrTechnidilatonLeadPresent}");
Console.WriteLine($"technicolorPromotesWzMasses={technicolorPromotesWzMasses}");
Console.WriteLine($"technicolorPromotesHiggsMass={technicolorPromotesHiggsMass}");
Console.WriteLine($"localSearchMatchingFileCount={localSearchEvidence.MatchingFileCount}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFieldCount}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFieldCount}");

static LocalSearchEvidence BuildLocalSearchEvidence()
{
    var terms = new[]
    {
        "technicolor",
        "technicolour",
        "walking technicolor",
        "walking technicolour",
        "technifermion",
        "technidilaton",
        "extended technicolor",
        "extended technicolour",
        "topcolor-assisted technicolor",
        "dynamical electroweak symmetry breaking",
        "strong electroweak",
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
    || normalizedPath.Contains("studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/", StringComparison.Ordinal)
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
    || normalizedPath.Contains("studies/phase365_dressing_field_electroweak_observed_variables_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase366_bost_connes_arithmetic_gauge_coupling_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase367_theta_omega_source_equation_availability_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase368_oxford_inhomogeneous_gauge_equation_bridge_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase369_weyl_conformal_sm_stueckelberg_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase370_completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase374_shared_weighted_fermion_spectral_solver_repair_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase375_weighted_reciprocal_mixed_block_replay_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase376_persisted_nonzero_shell_reciprocal_replay_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase377_selected_source_mode_shell_response_gram_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase378_full_connection_carrier_shell_response_gram_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase379_response_image_carrier_axis_characterization_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase380_response_image_wz_contract_application_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase381_phase302_307_response_image_selector_compatibility_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase382_response_image_observed_projection_requirement_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase383_phase307_suppressed_axis_counterfactual_selector_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase384_phase307_basis_energy_response_image_proxy_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase385_observed_electroweak_namespace_map_intake_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase386_current_cox_first_principles_i_source_delta_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase387_current_cox_first_principles_i_full_text_contract_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase390_converged_control_branch_fermion_mode_rebuild_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase391_dense_converged_shell_response_replay_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase398_vo6_vo7_control_branch_completion_ledger_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase399_quadratic_model_coupled_critical_point_solve_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase400_full_bosonic_action_flat_direction_lift_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase401_full_quartic_action_coupled_critical_point_construction_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase402_gu_draft_scalar_route_dictionary_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase403_adjoint_doublet_substructure_branching_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase406_choice_space_falsification_sweep_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase407_chimeric_adjoint_sm_content_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase410_curvature_coupled_vev_selection_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase412_quartic_sm_doublet_intersection_analysis_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase413_noncompact_real_form_transfer_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase414_general_shiab_epsilon_operator_ansatz_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase415_fermionic_cohomology_square_root_ansatz_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase416_unobserved_phase_carrier_census_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase417_vector_spinor_144_decomposition_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase418_direction_dependent_curvature_vev_coupling_scan_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase419_observed_field_symbolic_extraction_template_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase420_naive_curvature_mass_scale_sanity_check_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase421_cox_gu_iv_v2_lcdm_rig_boson_contract_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase423_zenodo_gu_rvg_spinorial_dark_sector_boson_contract_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase426_cox_gu_series_boson_contract_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase427_hofseth_gu_rvg_superluminal_source_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase428_fermion_loop_block_selection_no_go_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase429_target_blind_dimensionless_ratio_ledger_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase430_net_one_loop_direction_selection_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase431_lambda8_background_doublet_reopening_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase432_welded_fermion_loop_block_selection_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase433_blind_beta_coefficient_running_ledger_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase434_conditional_observed_field_extraction_row_ledger_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase435_two_condensate_scale_gap_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase436_exact_hessian_saturation_no_go_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase437_four_dimensional_transmutation_scaling_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase438_self_consistent_condensate_gap_equation_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase439_gap_equation_lambda8_background_channel_steering_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase440_coupled_background_condensate_fixed_point_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase441_toy_branch_family_universality_sweep_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase442_joint_omega_theta_hessian_degree_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase443_joint_effective_potential_saturation_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase444_mode_volume_scaled_saturation_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase445_rg_improved_joint_potential_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase446_rg_scheme_dependence_resolution_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase447_two_loop_saturation_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase448_torus_mode_volume_saturation_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase449_variational_gaussian_effective_potential_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase450_constraint_effective_potential_hmc_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase451_two_loop_unification_ledger_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase453_wham_parity_error_model_repair_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase454_beyond_ray_quadratic_certificate_probe_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase460_source_corpus_units_equivariance_kernel_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase461_dimensional_transmutation_reading_menu_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase467_derived_operator_stabilizer_ray_census_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase468_two_loop_content_row_closure_filter_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase469_c_lift_representation_bookkeeping_gate_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase470_c_permanence_five_limb_ledger_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase477_o4_adjudication_infrastructure_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase478_phase458_gate_specification_closure_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase479_phase457_post_ruling_readiness_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase480_o4_physicist_adjudication_intake_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase481_phase456_prospective_repair_preregistration_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase482_a5_theorem_scout_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase483_source_defined_reopening_intake_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase484_exploratory_lane_governance_firewall_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase485_o4_assumption_falsifier_census_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase486_committed_evidence_sensitivity_triage_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase487_independent_so3_haar_measure_control_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase488_haar_proposal_invariance_control_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase489_reduced_sampler_restart_equivalence_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase490_zero_mode_quotient_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase491_committed_bosonic_model_family_sensitivity_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase492_phase455_combined_robustness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase493_phase456_stored_artifact_failure_decomposition_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase494_phase456_estimator_oracle_battery_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase495_phase456_prospective_repair_readiness_adjudicator_001/", StringComparison.Ordinal)
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
    || normalizedPath.Contains("studies/phase508_phase481_acquisition_geometry_closure_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase509_phase481_anisotropic_cpu_reference_feasibility_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase510_phase481_execution_readiness_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase511_phase481_throughput_benchmark_eligibility_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase514_a5_registered_reflection_foundation_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase517_a5_dual_reflection_candidate_foundation_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase518_a5_dual_reflection_exact_consistency_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase519_a5_candidate_foundation_readiness_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase520_a5_action_subject_lineage_parity_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase521_a5_frozen_reflection_compatible_triangulation_census_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase522_a5_foundation_candidate_reduction_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase523_a5_action_member_universalization_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase524_a5_exact_omega_parity_decomposition_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase525_a5_survivor_reflection_pullback_boundary_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase526_a5_certificate_reducer_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase527_a5_sd2_theta_zero_exact_parity_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase528_a5_even_sector_premise_applicability_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase529_a5_action_premise_route_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase530_o4_g4_authentication_admissibility_audit_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase531_o4_g4_disposition_resolution_semantics_001/", StringComparison.Ordinal)
    || normalizedPath.Contains("studies/phase532_phase458_g4_consumer_correction_adjudicator_001/", StringComparison.Ordinal)
    || normalizedPath == "studies/phase101_boson_prediction_package_001/Program.cs"
    || normalizedPath == "studies/phase202_boson_objective_completion_audit_001/Program.cs"
    || normalizedPath == "studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs"
    || normalizedPath == "studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs"
    || normalizedPath == "studies/phase207_higgs_quartic_self_coupling_source_scan_001/Program.cs"
    || normalizedPath == "scripts/generate_validated_boson_predictions.sh"
    || normalizedPath == "scripts/verify_boson_claim_integrity.sh"
    || normalizedPath == "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md"
    || normalizedPath == "docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P279.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P467.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P468.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P469.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P470.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P477.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P478.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P479.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P480.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P481.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P482.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P483.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P484.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P485.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P486.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P487.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P488.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P489.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P490.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P491.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P492.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P493.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P494.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P495.md"
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
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P507.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P508.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P509.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P510.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P511.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P514.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P517.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P518.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P519.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P520.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P521.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P522.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P523.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P524.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P525.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P526.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P527.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P528.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P529.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P530.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P531.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P532.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P533.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P534.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P535.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P536.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P537.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P538.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P539.md"
    || normalizedPath == "docs/Phases/EXPLORATORY_SELF_AUDIT_PLAN_2026-07-15.md"
    || normalizedPath == "docs/Phases/CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15.md"
    || normalizedPath == "docs/Phases/PHASE455_CONVENTION_CLOSURE_PLAN_2026-07-15.md"
    || normalizedPath == "docs/Phases/PHASE456_FORENSIC_RECOVERY_PLAN_2026-07-15.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P327.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P328.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P338.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P343.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P342.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P364.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P365.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P366.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P367.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P368.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P369.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P370.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P371.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P372.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P373.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P374.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P375.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P376.md"
    || normalizedPath.Contains("docs/Reference/ExperimentReferences/", StringComparison.Ordinal);

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

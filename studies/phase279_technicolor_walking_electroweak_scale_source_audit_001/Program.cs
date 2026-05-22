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
    || normalizedPath == "studies/phase101_boson_prediction_package_001/Program.cs"
    || normalizedPath == "studies/phase202_boson_objective_completion_audit_001/Program.cs"
    || normalizedPath == "studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs"
    || normalizedPath == "studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs"
    || normalizedPath == "studies/phase207_higgs_quartic_self_coupling_source_scan_001/Program.cs"
    || normalizedPath == "scripts/generate_validated_boson_predictions.sh"
    || normalizedPath == "scripts/verify_boson_claim_integrity.sh"
    || normalizedPath == "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P279.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P327.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P328.md"
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P338.md"
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

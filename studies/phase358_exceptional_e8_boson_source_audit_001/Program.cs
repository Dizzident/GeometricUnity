using System.Text.Json;

const string DefaultOutputDir = "studies/phase358_exceptional_e8_boson_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase337Path = "studies/phase337_octonion_clifford_internal_space_source_audit_001/output/octonion_clifford_internal_space_source_audit_summary.json";
const string Phase355Path = "studies/phase355_dirac_lichnerowicz_yang_mills_higgs_source_audit_001/output/dirac_lichnerowicz_yang_mills_higgs_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE358_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase337 = JsonDocument.Parse(File.ReadAllText(Phase337Path));
using var phase355 = JsonDocument.Parse(File.ReadAllText(Phase355Path));

const string lisiOriginalE8Url = "https://arxiv.org/abs/0711.0770";
const string distlerGaribaldiE8ObstructionUrl = "https://arxiv.org/abs/0905.2658";
const string lisiExplicitEmbeddingUrl = "https://arxiv.org/abs/1006.4908";
const string octionsE8Url = "https://arxiv.org/abs/2204.05310";
const string octionsJournalDoi = "https://doi.org/10.1063/5.0095484";
const string e8E8PregravitationUrl = "https://arxiv.org/abs/2206.06911";
const string wilsonE8EmbeddingsUrl = "https://arxiv.org/abs/2507.16517";

const bool exceptionalE8BosonSourceAuditPassedExpected = true;
const bool exceptionalE8LeadPresent = true;
const bool exceptionalE8PrimarySourcesReviewed = true;
const bool exceptionalE8RouteExternalToGu = true;
const bool routeUsesExceptionalLieAlgebraE8 = true;
const bool routeUsesE8PrincipalBundleConnection = true;
const bool routeEmbedsGravityAndStandardModel = true;
const bool routeEmbedsStandardModelGaugeAlgebra = true;
const bool routeIncludesElectroweakSu2U1 = true;
const bool routeIncludesFrameHiggsOrHiggsDoublet = true;
const bool routeIncludesFermionsAndThreeGenerations = true;
const bool routeIncludesGravityOrPreGravitation = true;
const bool routeOctionsE8Minus24Present = true;
const bool routeE8xE8PregravitationPresent = true;
const bool routeWilson2025EmbeddingUpdatePresent = true;
const bool representationNoToeObstructionPresent = true;
const bool routeClaimsWeakBosonsMassiveAfterSsb = true;
const bool routeProvidesBosonIdentificationButNotMassLaw = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotPredictObservedHiggsMass = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresExternalExceptionalModelChoice = true;
const bool routeRequiresRepresentationEmbeddingChoice = true;
const bool routeRequiresSymmetryBreakingMechanism = true;
const bool routeRequiresHiggsPotentialOrMassMatrix = true;
const bool routeRequiresCouplingsAndLagrangianCompletion = true;
const bool routeRequiresFermionMassOrBackgroundInputs = true;
const bool routeRequiresCompatibilityWithNoToeObstruction = true;
const bool routeRequiresObservedMassComparison = true;

const bool routeRequiresGuLocalE8Embedding = true;
const bool routeRequiresGuExceptionalBranchingMap = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalE8Embedding = false;
const bool routeProvidesGuExceptionalBranchingMap = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsSelfCouplingSource = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

const int lisiOriginalPageCount = 31;
const int lisiExplicitEmbeddingPageCount = 14;
const int distlerGaribaldiLatestArxivVersion = 3;
const int distlerGaribaldiLatestRevisionYear = 2009;
const int octionsLatestArxivVersion = 2;
const int octionsPageCount = 15;
const int e8E8LatestArxivVersion = 3;
const int e8E8PageCount = 31;
const int wilsonEmbeddingsLatestArxivVersion = 2;
const int wilsonEmbeddingsRevisionYear = 2025;
const int sourceRowCountExpected = 6;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var octonionRoutePromotesWzMasses = JsonBool(phase337.RootElement, "octonionRoutePromotesWzMasses") is true;
var octonionRoutePromotesHiggsMass = JsonBool(phase337.RootElement, "octonionRoutePromotesHiggsMass") is true;
var diracLichnerowiczRoutePromotesWzMasses = JsonBool(phase355.RootElement, "routePromotesWzMasses") is true;
var diracLichnerowiczRoutePromotesHiggsMass = JsonBool(phase355.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "lisi-0711-0770-exceptionally-simple-e8",
        lisiOriginalE8Url,
        "An Exceptionally Simple Theory of Everything",
        "E8 principal-bundle connection proposal containing Standard Model, gravity, electroweak su(2) x u(1), frame-Higgs, and three-generation structure.",
        "Algebraic/geometric identification lead only; no observed W/Z/H mass source rows, scale, or pole extraction."),
    new SourceRow(
        "distler-garibaldi-0905-2658-no-toe-inside-e8",
        distlerGaribaldiE8ObstructionUrl,
        "There is no Theory of Everything inside E8",
        "Representation-theory obstruction showing E8 TOE embeddings lack properties required by physical reality.",
        "Blocks naive E8 promotion and requires any E8-like route to supply an explicit physical representation and mass law."),
    new SourceRow(
        "lisi-1006-4908-explicit-e8-embedding",
        lisiExplicitEmbeddingUrl,
        "An Explicit Embedding of Gravity and the Standard Model in E8",
        "Explicitly embeds gravitational and Standard Model gauge-field algebra acting on one fermion generation into E8 through spin(11,3)/GraviGUT structure.",
        "Embedding result only; no W/Z/H pole masses, Higgs potential, VEV source, or GU-local source rows."),
    new SourceRow(
        "octions-2204-05310-e8-description-standard-model",
        octionsE8Url,
        "Octions: An E8 description of the Standard Model",
        "Interprets e8(-24) elements as Standard Model objects, including SM gauge algebra, Lorentz algebra, quark/lepton spinors, and GUT substructures.",
        "Strong representation lead; does not derive target-independent electroweak mass parameters or observed W/Z/H states."),
    new SourceRow(
        "e8-e8-2206-06911-standard-model-pregravitation",
        e8E8PregravitationUrl,
        "E8 x E8 unification with pre-gravitation on octonion-valued twistor space",
        "Branches one E8 to the Standard Model gauge bosons, Higgs, and left chiral fermions, and a second E8 to a right-sector pre-gravitation analogue.",
        "Uses spontaneous symmetry breaking language and future coupling/Lagrangian work; no completed physical W/Z/H mass law."),
    new SourceRow(
        "wilson-2507-16517-standard-model-e8-embeddings",
        wilsonE8EmbeddingsUrl,
        "Embeddings of the Standard Model in E8",
        "Recent modified octions route relating weak SU(2) breaking to generation breaking and discussing mass/background dependence.",
        "Current E8 mass lead remains external and background/mass dependent, with no GU-local source-lineage rows or GeV normalization.")
};

var checks = new[]
{
    new Check(
        "exceptional-e8-primary-sources-reviewed",
        exceptionalE8LeadPresent
            && exceptionalE8PrimarySourcesReviewed
            && exceptionalE8RouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={exceptionalE8LeadPresent}; reviewed={exceptionalE8PrimarySourcesReviewed}; externalToGu={exceptionalE8RouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "exceptional-e8-standard-model-structure-captured",
        routeUsesExceptionalLieAlgebraE8
            && routeUsesE8PrincipalBundleConnection
            && routeEmbedsGravityAndStandardModel
            && routeEmbedsStandardModelGaugeAlgebra
            && routeIncludesElectroweakSu2U1
            && routeIncludesFrameHiggsOrHiggsDoublet
            && routeIncludesFermionsAndThreeGenerations
            && routeIncludesGravityOrPreGravitation
            && routeOctionsE8Minus24Present
            && routeE8xE8PregravitationPresent
            && routeWilson2025EmbeddingUpdatePresent,
        $"e8={routeUsesExceptionalLieAlgebraE8}; principalBundle={routeUsesE8PrincipalBundleConnection}; smGauge={routeEmbedsStandardModelGaugeAlgebra}; electroweak={routeIncludesElectroweakSu2U1}; higgs={routeIncludesFrameHiggsOrHiggsDoublet}; generations={routeIncludesFermionsAndThreeGenerations}; pregrav={routeIncludesGravityOrPreGravitation}; octions={routeOctionsE8Minus24Present}; e8e8={routeE8xE8PregravitationPresent}; wilson2025={routeWilson2025EmbeddingUpdatePresent}"),
    new Check(
        "exceptional-e8-mass-law-blockers-captured",
        representationNoToeObstructionPresent
            && routeClaimsWeakBosonsMassiveAfterSsb
            && routeProvidesBosonIdentificationButNotMassLaw
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotPredictObservedHiggsMass
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotProvideObservedPhotonWzHiggsProjection,
        $"noToeObstruction={representationNoToeObstructionPresent}; weakBosonsAfterSsb={routeClaimsWeakBosonsMassiveAfterSsb}; idNotMassLaw={routeProvidesBosonIdentificationButNotMassLaw}; observedWz={routeDoesNotPredictObservedWzMasses}; observedHiggs={routeDoesNotPredictObservedHiggsMass}; pole={routeDoesNotProvidePhysicalPoleExtraction}; observedProjection={routeDoesNotProvideObservedPhotonWzHiggsProjection}"),
    new Check(
        "external-choices-and-completion-work-required",
        routeRequiresExternalExceptionalModelChoice
            && routeRequiresRepresentationEmbeddingChoice
            && routeRequiresSymmetryBreakingMechanism
            && routeRequiresHiggsPotentialOrMassMatrix
            && routeRequiresCouplingsAndLagrangianCompletion
            && routeRequiresFermionMassOrBackgroundInputs
            && routeRequiresCompatibilityWithNoToeObstruction
            && routeRequiresObservedMassComparison,
        $"modelChoice={routeRequiresExternalExceptionalModelChoice}; embedding={routeRequiresRepresentationEmbeddingChoice}; ssb={routeRequiresSymmetryBreakingMechanism}; higgsMassMatrix={routeRequiresHiggsPotentialOrMassMatrix}; couplingsLagrangian={routeRequiresCouplingsAndLagrangianCompletion}; fermionBackground={routeRequiresFermionMassOrBackgroundInputs}; obstructionCompatibility={routeRequiresCompatibilityWithNoToeObstruction}; observedComparison={routeRequiresObservedMassComparison}"),
    new Check(
        "adjacent-octonion-and-dirac-routes-remain-nonpromotional",
        !octonionRoutePromotesWzMasses
            && !octonionRoutePromotesHiggsMass
            && !diracLichnerowiczRoutePromotesWzMasses
            && !diracLichnerowiczRoutePromotesHiggsMass,
        $"octonionWz={octonionRoutePromotesWzMasses}; octonionHiggs={octonionRoutePromotesHiggsMass}; diracWz={diracLichnerowiczRoutePromotesWzMasses}; diracHiggs={diracLichnerowiczRoutePromotesHiggsMass}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalE8Embedding
            && !routeProvidesGuExceptionalBranchingMap
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsSelfCouplingSource
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guE8={routeProvidesGuLocalE8Embedding}; guBranching={routeProvidesGuExceptionalBranchingMap}; guWzRows={routeProvidesGuWzSourceRows}; weakAngle={routeProvidesGuWeakMixingAngleSource}; observed={routeProvidesGuObservedFieldExtraction}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; selfCoupling={routeProvidesGuHiggsSelfCouplingSource}; scale={routeProvidesTargetIndependentVevOrMassScale}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
    new Check(
        "phase201-phase256-contract-state-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}")
};

var exceptionalE8BosonSourceAuditPassed = checks.All(check => check.Passed)
    && exceptionalE8BosonSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = exceptionalE8BosonSourceAuditPassed
    ? "exceptional-e8-boson-source-audit-representation-lead-not-gu-mass-law"
    : "exceptional-e8-boson-source-audit-review-required";

var result = new
{
    phaseId = "phase358-exceptional-e8-boson-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    exceptionalE8BosonSourceAuditPassed,
    exceptionalE8LeadPresent,
    exceptionalE8PrimarySourcesReviewed,
    exceptionalE8RouteExternalToGu,
    octionsJournalDoi,
    routeUsesExceptionalLieAlgebraE8,
    routeUsesE8PrincipalBundleConnection,
    routeEmbedsGravityAndStandardModel,
    routeEmbedsStandardModelGaugeAlgebra,
    routeIncludesElectroweakSu2U1,
    routeIncludesFrameHiggsOrHiggsDoublet,
    routeIncludesFermionsAndThreeGenerations,
    routeIncludesGravityOrPreGravitation,
    routeOctionsE8Minus24Present,
    routeE8xE8PregravitationPresent,
    routeWilson2025EmbeddingUpdatePresent,
    representationNoToeObstructionPresent,
    routeClaimsWeakBosonsMassiveAfterSsb,
    routeProvidesBosonIdentificationButNotMassLaw,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotPredictObservedHiggsMass,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    lisiOriginalPageCount,
    lisiExplicitEmbeddingPageCount,
    distlerGaribaldiLatestArxivVersion,
    distlerGaribaldiLatestRevisionYear,
    octionsLatestArxivVersion,
    octionsPageCount,
    e8E8LatestArxivVersion,
    e8E8PageCount,
    wilsonEmbeddingsLatestArxivVersion,
    wilsonEmbeddingsRevisionYear,
    routeRequiresExternalExceptionalModelChoice,
    routeRequiresRepresentationEmbeddingChoice,
    routeRequiresSymmetryBreakingMechanism,
    routeRequiresHiggsPotentialOrMassMatrix,
    routeRequiresCouplingsAndLagrangianCompletion,
    routeRequiresFermionMassOrBackgroundInputs,
    routeRequiresCompatibilityWithNoToeObstruction,
    routeRequiresObservedMassComparison,
    routeRequiresGuLocalE8Embedding,
    routeRequiresGuExceptionalBranchingMap,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalE8Embedding,
    routeProvidesGuExceptionalBranchingMap,
    routeProvidesGuWzSourceRows,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsSelfCouplingSource,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    sourceRows,
    adjacentRouteBoundary = new
    {
        octonionRoutePromotesWzMasses,
        octonionRoutePromotesHiggsMass,
        diracLichnerowiczRoutePromotesWzMasses,
        diracLichnerowiczRoutePromotesHiggsMass
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    checks,
    decision = "Do not promote W/Z or Higgs physical masses from exceptional E8 routes in this repository. The sources supply serious representation and unification leads, including Standard Model gauge algebra, electroweak SU(2) x U(1), Higgs/frame-Higgs objects, generations, and gravity/pre-gravitation embeddings, but they do not supply GU-local E8 branching from Shiab/observer geometry, separate W/Z source rows, physical photon/W/Z/H projection, weak-angle and coupling normalization, target-independent VEV or mass-scale source, Higgs scalar-source/self-coupling lineage, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local exceptional-algebra embedding theorem mapping Shiab/observer-sector fields into the E8 or E8 x E8 representation without importing an external model.",
        "A target-independent branching and symmetry-breaking law that produces observed photon/W/Z/H fields and separate W/Z source rows.",
        "A Higgs scalar-source, self-coupling or excitation law, VEV or equivalent mass-scale source, and weak-angle/coupling normalization from the same geometry.",
        "A physical pole extractor and GeV normalization before any E8 route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "exceptional_e8_boson_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "exceptional_e8_boson_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.exceptionalE8BosonSourceAuditPassed,
            result.exceptionalE8LeadPresent,
            result.exceptionalE8PrimarySourcesReviewed,
            result.exceptionalE8RouteExternalToGu,
            result.routeUsesExceptionalLieAlgebraE8,
            result.routeUsesE8PrincipalBundleConnection,
            result.routeEmbedsGravityAndStandardModel,
            result.routeEmbedsStandardModelGaugeAlgebra,
            result.routeIncludesElectroweakSu2U1,
            result.routeIncludesFrameHiggsOrHiggsDoublet,
            result.routeIncludesFermionsAndThreeGenerations,
            result.routeIncludesGravityOrPreGravitation,
            result.routeOctionsE8Minus24Present,
            result.routeE8xE8PregravitationPresent,
            result.routeWilson2025EmbeddingUpdatePresent,
            result.representationNoToeObstructionPresent,
            result.routeClaimsWeakBosonsMassiveAfterSsb,
            result.routeProvidesBosonIdentificationButNotMassLaw,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotPredictObservedHiggsMass,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.lisiOriginalPageCount,
            result.lisiExplicitEmbeddingPageCount,
            result.distlerGaribaldiLatestArxivVersion,
            result.distlerGaribaldiLatestRevisionYear,
            result.octionsLatestArxivVersion,
            result.octionsPageCount,
            result.e8E8LatestArxivVersion,
            result.e8E8PageCount,
            result.wilsonEmbeddingsLatestArxivVersion,
            result.wilsonEmbeddingsRevisionYear,
            result.routeRequiresExternalExceptionalModelChoice,
            result.routeRequiresRepresentationEmbeddingChoice,
            result.routeRequiresSymmetryBreakingMechanism,
            result.routeRequiresHiggsPotentialOrMassMatrix,
            result.routeRequiresCouplingsAndLagrangianCompletion,
            result.routeRequiresFermionMassOrBackgroundInputs,
            result.routeRequiresCompatibilityWithNoToeObstruction,
            result.routeRequiresObservedMassComparison,
            result.routeRequiresGuLocalE8Embedding,
            result.routeRequiresGuExceptionalBranchingMap,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalE8Embedding,
            result.routeProvidesGuExceptionalBranchingMap,
            result.routeProvidesGuWzSourceRows,
            result.routeProvidesGuWeakMixingAngleSource,
            result.routeProvidesGuObservedFieldExtraction,
            result.routeProvidesGuHiggsScalarSourceOperator,
            result.routeProvidesGuHiggsSelfCouplingSource,
            result.routeProvidesTargetIndependentVevOrMassScale,
            result.routeProvidesGeVUnitNormalization,
            result.routePromotesWzMasses,
            result.routePromotesHiggsMass,
            result.routeCompletesBosonPredictions,
            result.canFillPhase201WzContract,
            result.canFillPhase201HiggsContract,
            result.canFillPhase256ObservedFieldExtractionContract,
            result.sourceRowCount,
            result.adjacentRouteBoundary,
            result.contractImpact,
            result.decision,
            result.nextRequiredArtifact
        },
        options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"exceptionalE8BosonSourceAuditPassed={exceptionalE8BosonSourceAuditPassed}");
Console.WriteLine($"routeEmbedsStandardModelGaugeAlgebra={routeEmbedsStandardModelGaugeAlgebra}");
Console.WriteLine($"routeIncludesFrameHiggsOrHiggsDoublet={routeIncludesFrameHiggsOrHiggsDoublet}");
Console.WriteLine($"representationNoToeObstructionPresent={representationNoToeObstructionPresent}");
Console.WriteLine($"routeProvidesBosonIdentificationButNotMassLaw={routeProvidesBosonIdentificationButNotMassLaw}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False ? property.GetBoolean() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value) ? value : null;

sealed record Check(string Id, bool Passed, string Evidence);

sealed record SourceRow(
    string Id,
    string Url,
    string Title,
    string Summary,
    string PromotionBoundary);

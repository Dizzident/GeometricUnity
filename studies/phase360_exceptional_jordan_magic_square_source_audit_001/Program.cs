using System.Text.Json;

const string DefaultOutputDir = "studies/phase360_exceptional_jordan_magic_square_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase337Path = "studies/phase337_octonion_clifford_internal_space_source_audit_001/output/octonion_clifford_internal_space_source_audit_summary.json";
const string Phase358Path = "studies/phase358_exceptional_e8_boson_source_audit_001/output/exceptional_e8_boson_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE360_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase337 = JsonDocument.Parse(File.ReadAllText(Phase337Path));
using var phase358 = JsonDocument.Parse(File.ReadAllText(Phase358Path));

const string exceptionalQuantumGeometryUrl = "https://arxiv.org/abs/1604.01247";
const string standardModelSymmetryFromJordanUrl = "https://arxiv.org/abs/1806.09450";
const string exceptionalQuantumGeometryIiUrl = "https://arxiv.org/abs/1808.08110";
const string jordanGeometryUrl = "https://arxiv.org/abs/1910.11888";
const string exceptionalQuantumAlgebraUrl = "https://arxiv.org/abs/1911.13124";
const string exceptionalJordanTrialityUrl = "https://arxiv.org/abs/2006.16265";
const string dixonRosenfeldLinesUrl = "https://arxiv.org/abs/2303.11334";
const string fermionMassRatiosJordanUrl = "https://arxiv.org/abs/2508.10131";

const bool exceptionalJordanMagicSquareSourceAuditPassedExpected = true;
const bool exceptionalJordanLeadPresent = true;
const bool exceptionalJordanPrimarySourcesReviewed = true;
const bool exceptionalJordanRouteExternalToGu = true;
const bool routeUsesExceptionalJordanAlgebra = true;
const bool routeUsesAlbertAlgebra = true;
const bool routeUsesPeirceSlotsOrTriality = true;
const bool routeUsesFreudenthalTitsMagicSquare = true;
const bool routeEncodesStandardModelSymmetry = true;
const bool routeIncludesElectroweakSubgroup = true;
const bool routeIncludesHiggsYukawaOrScalarLead = true;
const bool routeIncludesFermionGenerationStructure = true;
const bool routeIncludesJordanGeometryPatiSalamOrBMinusLScalar = true;
const bool routeIncludesCurrentFermionMassRatioLead = true;
const bool currentFermionMassRatioLeadRevisedIn2026 = true;
const int currentFermionMassRatioLatestArxivVersion = 5;
const int currentFermionMassRatioRevisionYear = 2026;
const int dixonRosenfeldLatestArxivVersion = 2;
const int sourceRowCountExpected = 8;

const bool routeMassLeadScopeFermionOnly = true;
const bool routeProvidesRepresentationOrSymmetryLeadNotMassLaw = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotProvideTargetIndependentObservedHiggsMass = true;
const bool routeDoesNotProvideGaugeBosonMassMatrix = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresExternalJordanModelChoice = true;
const bool routeRequiresGuLocalJordanAlgebraMap = true;
const bool routeRequiresGuPeirceTrialityBranchingMap = true;
const bool routeRequiresGuMagicSquareOrExceptionalEmbeddingMap = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuGaugeCouplingNormalization = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresLowEnergyRgAndThresholdTransport = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalJordanAlgebraMap = false;
const bool routeProvidesGuPeirceTrialityBranchingMap = false;
const bool routeProvidesGuMagicSquareOrExceptionalEmbeddingMap = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsSelfCouplingSource = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesLowEnergyRgAndThresholdTransport = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var octonionCliffordSourceAuditPassed = JsonBool(phase337.RootElement, "octonionCliffordSourceAuditPassed") is true;
var octonionRoutePromotesWzMasses = JsonBool(phase337.RootElement, "octonionRoutePromotesWzMasses") is true;
var octonionRoutePromotesHiggsMass = JsonBool(phase337.RootElement, "octonionRoutePromotesHiggsMass") is true;
var exceptionalE8BosonSourceAuditPassed = JsonBool(phase358.RootElement, "exceptionalE8BosonSourceAuditPassed") is true;
var exceptionalE8RoutePromotesWzMasses = JsonBool(phase358.RootElement, "routePromotesWzMasses") is true;
var exceptionalE8RoutePromotesHiggsMass = JsonBool(phase358.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "dubois-violette-1604-01247-exceptional-quantum-geometry",
        exceptionalQuantumGeometryUrl,
        "Exceptional quantum geometry and particle physics",
        "Argues that the Euclidean Albert algebra is relevant for particle internal space, with triality associated to three Standard Model generations and Jordan-module connections.",
        "Internal-space and connection lead only; no observed W/Z/H mass rows, VEV, pole extraction, or GeV normalization."),
    new SourceRow(
        "dubois-violette-todorov-1806-09450-sm-symmetry-from-jordan",
        standardModelSymmetryFromJordanUrl,
        "Deducing the symmetry of the standard model from the automorphism and structure groups of the exceptional Jordan algebra",
        "Uses automorphism/structure groups of J3^8 and Borel-de Siebenthal subgroup theory to derive Standard Model symmetry structure.",
        "Symmetry derivation lead; it does not provide a GU-local electroweak mass matrix or target-independent W/Z/H masses."),
    new SourceRow(
        "dubois-violette-todorov-1808-08110-exceptional-quantum-geometry-ii",
        exceptionalQuantumGeometryIiUrl,
        "Exceptional quantum geometry and particle physics II",
        "Develops J3^8/J2^8 internal-space descriptions for three generations and one generation without introducing new fundamental fermions or electroweak-symmetry trouble.",
        "Generation/electroweak representation lead; no physical W/Z source rows, scalar potential, or mass-scale lineage."),
    new SourceRow(
        "boyle-farnsworth-1910-11888-jordan-geometry",
        jordanGeometryUrl,
        "The standard model, the Pati-Salam model, and Jordan geometry",
        "Presents a Jordan-algebra framework that most nearly describes the Standard Model but actually yields a viable B-L/scalar extension and a Pati-Salam extension.",
        "Geometric model-building lead; extra scalar/B-L structure is external and no observed W/Z/H mass law is supplied."),
    new SourceRow(
        "todorov-1911-13124-exceptional-quantum-algebra-sm",
        exceptionalQuantumAlgebraUrl,
        "Exceptional quantum algebra for the standard model of particle physics",
        "Uses the exceptional Euclidean Jordan algebra, F4 subgroups, S(U(3)xU(2)), primitive idempotents, and triality to describe Standard Model internal structure and Higgs Yukawa coupling.",
        "Higgs/Yukawa structural lead; does not derive Higgs self-coupling, W/Z mass matrix, observed projection, or GeV scale."),
    new SourceRow(
        "boyle-2006-16265-exceptional-jordan-triality",
        exceptionalJordanTrialityUrl,
        "The Standard Model, The Exceptional Jordan Algebra, and Triality",
        "Relates the complexified exceptional Jordan algebra to the Standard Model, left-right symmetry, Spin(10), octonionic projective geometry, and three-generation triality.",
        "Representation and generation lead; no target-independent boson-mass source lineage."),
    new SourceRow(
        "chester-marrani-corradetti-aschheim-irwin-2303-11334-dixon-rosenfeld",
        dixonRosenfeldLinesUrl,
        "Dixon-Rosenfeld Lines and the Standard Model",
        "Uses Freudenthal-Tits construction and Jordan-algebra tensor products to uplift Standard Model interactions over Dixon-Rosenfeld projective-line analogues.",
        "Magic-square/Jordan uplift lead; the paper says further representation work remains and Higgs interactions are future work."),
    new SourceRow(
        "singh-2508-10131-fermion-mass-ratios-exceptional-jordan",
        fermionMassRatiosJordanUrl,
        "Fermion mass ratios from the exceptional Jordan algebra",
        "Current 2026-revised exceptional-Jordan mass-ratio program using Peirce slots, triality, Sym^3 flavor structure, a cubic determinant, and an E6-invariant Yukawa.",
        "Current mass-ratio lead is fermion-scoped; it does not provide W/Z absolute rows or a target-independent observed Higgs mass.")
};

var checks = new[]
{
    new Check(
        "exceptional-jordan-primary-sources-reviewed",
        exceptionalJordanLeadPresent
            && exceptionalJordanPrimarySourcesReviewed
            && exceptionalJordanRouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={exceptionalJordanLeadPresent}; reviewed={exceptionalJordanPrimarySourcesReviewed}; externalToGu={exceptionalJordanRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "exceptional-jordan-standard-model-structure-captured",
        routeUsesExceptionalJordanAlgebra
            && routeUsesAlbertAlgebra
            && routeUsesPeirceSlotsOrTriality
            && routeUsesFreudenthalTitsMagicSquare
            && routeEncodesStandardModelSymmetry
            && routeIncludesElectroweakSubgroup
            && routeIncludesHiggsYukawaOrScalarLead
            && routeIncludesFermionGenerationStructure
            && routeIncludesJordanGeometryPatiSalamOrBMinusLScalar
            && routeIncludesCurrentFermionMassRatioLead,
        $"jordan={routeUsesExceptionalJordanAlgebra}; albert={routeUsesAlbertAlgebra}; triality={routeUsesPeirceSlotsOrTriality}; magicSquare={routeUsesFreudenthalTitsMagicSquare}; sm={routeEncodesStandardModelSymmetry}; electroweak={routeIncludesElectroweakSubgroup}; higgsLead={routeIncludesHiggsYukawaOrScalarLead}; generations={routeIncludesFermionGenerationStructure}; jordanGeometryExtension={routeIncludesJordanGeometryPatiSalamOrBMinusLScalar}; currentFermionMassLead={routeIncludesCurrentFermionMassRatioLead}"),
    new Check(
        "current-jordan-fermion-mass-lead-version-captured",
        currentFermionMassRatioLeadRevisedIn2026
            && currentFermionMassRatioLatestArxivVersion == 5
            && currentFermionMassRatioRevisionYear == 2026
            && dixonRosenfeldLatestArxivVersion == 2,
        $"fermionMassLeadRevised2026={currentFermionMassRatioLeadRevisedIn2026}; latestVersion={currentFermionMassRatioLatestArxivVersion}; revisionYear={currentFermionMassRatioRevisionYear}; dixonRosenfeldVersion={dixonRosenfeldLatestArxivVersion}"),
    new Check(
        "exceptional-jordan-mass-law-blockers-captured",
        routeMassLeadScopeFermionOnly
            && routeProvidesRepresentationOrSymmetryLeadNotMassLaw
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotProvideTargetIndependentObservedHiggsMass
            && routeDoesNotProvideGaugeBosonMassMatrix
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotProvideObservedPhotonWzHiggsProjection,
        $"fermionOnly={routeMassLeadScopeFermionOnly}; representationNotMassLaw={routeProvidesRepresentationOrSymmetryLeadNotMassLaw}; wzMasses={routeDoesNotPredictObservedWzMasses}; higgsMass={routeDoesNotProvideTargetIndependentObservedHiggsMass}; massMatrix={routeDoesNotProvideGaugeBosonMassMatrix}; pole={routeDoesNotProvidePhysicalPoleExtraction}; observedProjection={routeDoesNotProvideObservedPhotonWzHiggsProjection}"),
    new Check(
        "adjacent-exceptional-and-octonion-routes-remain-nonpromotional",
        octonionCliffordSourceAuditPassed
            && !octonionRoutePromotesWzMasses
            && !octonionRoutePromotesHiggsMass
            && exceptionalE8BosonSourceAuditPassed
            && !exceptionalE8RoutePromotesWzMasses
            && !exceptionalE8RoutePromotesHiggsMass,
        $"octonionPassed={octonionCliffordSourceAuditPassed}; octonionWz={octonionRoutePromotesWzMasses}; octonionHiggs={octonionRoutePromotesHiggsMass}; e8Passed={exceptionalE8BosonSourceAuditPassed}; e8Wz={exceptionalE8RoutePromotesWzMasses}; e8Higgs={exceptionalE8RoutePromotesHiggsMass}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        routeRequiresExternalJordanModelChoice
            && routeRequiresGuLocalJordanAlgebraMap
            && routeRequiresGuPeirceTrialityBranchingMap
            && routeRequiresGuMagicSquareOrExceptionalEmbeddingMap
            && routeRequiresGuWzSourceRows
            && routeRequiresGuWeakMixingAngleSource
            && routeRequiresGuGaugeCouplingNormalization
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGuHiggsSelfCouplingSource
            && routeRequiresTargetIndependentVevOrMassScale
            && routeRequiresLowEnergyRgAndThresholdTransport
            && routeRequiresGeVUnitNormalization
            && !routeProvidesGuLocalJordanAlgebraMap
            && !routeProvidesGuPeirceTrialityBranchingMap
            && !routeProvidesGuMagicSquareOrExceptionalEmbeddingMap
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuGaugeCouplingNormalization
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsSelfCouplingSource
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesLowEnergyRgAndThresholdTransport
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"externalModel={routeRequiresExternalJordanModelChoice}; requiresJordanMap={routeRequiresGuLocalJordanAlgebraMap}; requiresTriality={routeRequiresGuPeirceTrialityBranchingMap}; requiresMagicSquare={routeRequiresGuMagicSquareOrExceptionalEmbeddingMap}; requiresWzRows={routeRequiresGuWzSourceRows}; requiresWeakAngle={routeRequiresGuWeakMixingAngleSource}; requiresGaugeCoupling={routeRequiresGuGaugeCouplingNormalization}; requiresObserved={routeRequiresGuObservedFieldExtraction}; requiresHiggsOperator={routeRequiresGuHiggsScalarSourceOperator}; requiresSelfCoupling={routeRequiresGuHiggsSelfCouplingSource}; requiresScale={routeRequiresTargetIndependentVevOrMassScale}; requiresRg={routeRequiresLowEnergyRgAndThresholdTransport}; requiresGev={routeRequiresGeVUnitNormalization}; providesJordanMap={routeProvidesGuLocalJordanAlgebraMap}; providesWzRows={routeProvidesGuWzSourceRows}; providesHiggsOperator={routeProvidesGuHiggsScalarSourceOperator}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var exceptionalJordanMagicSquareSourceAuditPassed = checks.All(check => check.Passed)
    && exceptionalJordanMagicSquareSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = exceptionalJordanMagicSquareSourceAuditPassed
    ? "exceptional-jordan-magic-square-source-audit-representation-lead-not-gu-mass-law"
    : "exceptional-jordan-magic-square-source-audit-review-required";

var result = new
{
    phaseId = "phase360-exceptional-jordan-magic-square-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    exceptionalJordanMagicSquareSourceAuditPassed,
    exceptionalJordanLeadPresent,
    exceptionalJordanPrimarySourcesReviewed,
    exceptionalJordanRouteExternalToGu,
    routeUsesExceptionalJordanAlgebra,
    routeUsesAlbertAlgebra,
    routeUsesPeirceSlotsOrTriality,
    routeUsesFreudenthalTitsMagicSquare,
    routeEncodesStandardModelSymmetry,
    routeIncludesElectroweakSubgroup,
    routeIncludesHiggsYukawaOrScalarLead,
    routeIncludesFermionGenerationStructure,
    routeIncludesJordanGeometryPatiSalamOrBMinusLScalar,
    routeIncludesCurrentFermionMassRatioLead,
    currentFermionMassRatioLeadRevisedIn2026,
    currentFermionMassRatioLatestArxivVersion,
    currentFermionMassRatioRevisionYear,
    dixonRosenfeldLatestArxivVersion,
    routeMassLeadScopeFermionOnly,
    routeProvidesRepresentationOrSymmetryLeadNotMassLaw,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotProvideTargetIndependentObservedHiggsMass,
    routeDoesNotProvideGaugeBosonMassMatrix,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    routeRequiresExternalJordanModelChoice,
    routeRequiresGuLocalJordanAlgebraMap,
    routeRequiresGuPeirceTrialityBranchingMap,
    routeRequiresGuMagicSquareOrExceptionalEmbeddingMap,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuGaugeCouplingNormalization,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresLowEnergyRgAndThresholdTransport,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalJordanAlgebraMap,
    routeProvidesGuPeirceTrialityBranchingMap,
    routeProvidesGuMagicSquareOrExceptionalEmbeddingMap,
    routeProvidesGuWzSourceRows,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuGaugeCouplingNormalization,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsSelfCouplingSource,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesLowEnergyRgAndThresholdTransport,
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
        octonionCliffordSourceAuditPassed,
        octonionRoutePromotesWzMasses,
        octonionRoutePromotesHiggsMass,
        exceptionalE8BosonSourceAuditPassed,
        exceptionalE8RoutePromotesWzMasses,
        exceptionalE8RoutePromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs physical masses from exceptional-Jordan, Albert-algebra, triality, or Freudenthal-Tits magic-square routes in this repository. The sources provide serious geometric and algebraic Standard Model structure leads, including internal-space models, electroweak subgroup embeddings, Higgs/Yukawa or scalar-extension clues, three-generation triality, and current fermion mass-ratio work. They do not provide a GU-local Jordan algebra map from Shiab/observer geometry, a target-independent electroweak mass matrix, separate W/Z source rows, observed photon/W/Z/H projection, Higgs scalar-source/self-coupling lineage, VEV or mass-scale source, RG/threshold and pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local derivation mapping Shiab/observer-sector fields into an exceptional-Jordan, Albert, or magic-square internal geometry without importing an external representation choice.",
        "A target-independent Peirce/triality/branching theorem that produces observed electroweak SU(2) x U(1), photon/W/Z projection, and separate W/Z source rows.",
        "A Higgs scalar-source operator plus self-coupling or excitation law from the same Jordan geometry, not only a Higgs/Yukawa representation or external B-L scalar extension.",
        "A shared VEV or mass-scale source, low-energy RG/threshold transport, physical-pole extractor, and GeV normalization before the route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "exceptional_jordan_magic_square_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "exceptional_jordan_magic_square_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.exceptionalJordanMagicSquareSourceAuditPassed,
            result.exceptionalJordanLeadPresent,
            result.exceptionalJordanPrimarySourcesReviewed,
            result.exceptionalJordanRouteExternalToGu,
            result.routeUsesExceptionalJordanAlgebra,
            result.routeUsesAlbertAlgebra,
            result.routeUsesPeirceSlotsOrTriality,
            result.routeUsesFreudenthalTitsMagicSquare,
            result.routeEncodesStandardModelSymmetry,
            result.routeIncludesElectroweakSubgroup,
            result.routeIncludesHiggsYukawaOrScalarLead,
            result.routeIncludesFermionGenerationStructure,
            result.routeIncludesJordanGeometryPatiSalamOrBMinusLScalar,
            result.routeIncludesCurrentFermionMassRatioLead,
            result.currentFermionMassRatioLeadRevisedIn2026,
            result.currentFermionMassRatioLatestArxivVersion,
            result.currentFermionMassRatioRevisionYear,
            result.dixonRosenfeldLatestArxivVersion,
            result.routeMassLeadScopeFermionOnly,
            result.routeProvidesRepresentationOrSymmetryLeadNotMassLaw,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotProvideTargetIndependentObservedHiggsMass,
            result.routeDoesNotProvideGaugeBosonMassMatrix,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.routeRequiresExternalJordanModelChoice,
            result.routeRequiresGuLocalJordanAlgebraMap,
            result.routeRequiresGuPeirceTrialityBranchingMap,
            result.routeRequiresGuMagicSquareOrExceptionalEmbeddingMap,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuGaugeCouplingNormalization,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresLowEnergyRgAndThresholdTransport,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalJordanAlgebraMap,
            result.routeProvidesGuPeirceTrialityBranchingMap,
            result.routeProvidesGuMagicSquareOrExceptionalEmbeddingMap,
            result.routeProvidesGuWzSourceRows,
            result.routeProvidesGuWeakMixingAngleSource,
            result.routeProvidesGuGaugeCouplingNormalization,
            result.routeProvidesGuObservedFieldExtraction,
            result.routeProvidesGuHiggsScalarSourceOperator,
            result.routeProvidesGuHiggsSelfCouplingSource,
            result.routeProvidesTargetIndependentVevOrMassScale,
            result.routeProvidesLowEnergyRgAndThresholdTransport,
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
Console.WriteLine($"exceptionalJordanMagicSquareSourceAuditPassed={exceptionalJordanMagicSquareSourceAuditPassed}");
Console.WriteLine($"routeUsesExceptionalJordanAlgebra={routeUsesExceptionalJordanAlgebra}");
Console.WriteLine($"routeUsesFreudenthalTitsMagicSquare={routeUsesFreudenthalTitsMagicSquare}");
Console.WriteLine($"routeIncludesCurrentFermionMassRatioLead={routeIncludesCurrentFermionMassRatioLead}");
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

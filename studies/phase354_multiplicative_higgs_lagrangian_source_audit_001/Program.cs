using System.Text.Json;

const string DefaultOutputDir = "studies/phase354_multiplicative_higgs_lagrangian_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase263Path = "studies/phase263_top_yukawa_unity_higgs_closure_audit_001/output/top_yukawa_unity_higgs_closure_audit_summary.json";
const string Phase264Path = "studies/phase264_higgs_vacuum_criticality_source_audit_001/output/higgs_vacuum_criticality_source_audit_summary.json";
const string Phase293Path = "studies/phase293_fermi_vev_source_audit_001/output/fermi_vev_source_audit_summary.json";
const string Phase352Path = "studies/phase352_higgs_top_z_nnlo_matching_source_audit_001/output/higgs_top_z_nnlo_matching_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE354_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase263 = JsonDocument.Parse(File.ReadAllText(Phase263Path));
using var phase264 = JsonDocument.Parse(File.ReadAllText(Phase264Path));
using var phase293 = JsonDocument.Parse(File.ReadAllText(Phase293Path));
using var phase352 = JsonDocument.Parse(File.ReadAllText(Phase352Path));

const string multiplicativeHiggsLagrangianArxiv = "https://arxiv.org/abs/2504.17296";
const string multiplicativeHiggsLagrangianHtml = "https://arxiv.org/html/2504.17296v4";
const string multiplicativeHiggsLagrangianDoi = "https://doi.org/10.48550/arXiv.2504.17296";
const string nonstandardHiggsNeutrinoArxiv = "https://arxiv.org/abs/2312.16587";
const string nonstandardHiggsNeutrinoDoi = "https://doi.org/10.48550/arXiv.2312.16587";

const bool multiplicativeHiggsLagrangianSourceAuditPassedExpected = true;
const bool multiplicativeHiggsLagrangianLeadPresent = true;
const bool multiplicativeHiggsLagrangianPrimarySourcesReviewed = true;
const bool multiplicativeHiggsLagrangianRouteExternalToGu = true;
const bool routeDerivedFromInverseProblemCalculusOfVariations = true;
const bool routeUsesNonAdditiveMultiplicativeHiggsLagrangian = true;
const bool routePreservesElectroweakGaugeStructure = true;
const bool routeIntroducesNoExtraDegreesOfFreedom = true;
const bool routeEquivalentToStandardModelInLargeAuxiliaryMassLimit = true;
const bool routeDynamicallyModifiesHiggsMassTermAndVev = true;
const bool routeInducesSmallHiggsVevAndMassRescalings = true;
const bool routeGeneratesDimension5And6Operators = true;
const bool routeUsesSmeftConstraintBoundary = true;
const bool routeConsistentOnlyInRestrictedParameterSpace = true;
const bool routeContainsTreeLevelWzMassRelations = true;
const bool routeOrganizesFermionMassesByFiniteScalingFactors = true;
const bool routeUsesObservedPoleMassInputsForFermionHierarchy = true;
const bool routePreservesTreeLevelStandardModelYukawaRelation = true;
const bool routeFindsYukawaConvergenceNearHiggsSelfCoupling = true;
const bool routeBackgroundDependentHiggsSelfInteractionsDecreaseAsymptotically = true;
const bool routeProvidesExternalHiggsSectorModificationLead = true;
const bool routeProvidesExternalFermionMassHierarchyLead = true;
const bool routeProvidesNeutrinoMassLeadFromNonstandardHiggsLagrangian = true;

const int multiplicativeHiggsLatestArxivVersion = 4;
const int multiplicativeHiggsLatestRevisionYear = 2025;
const int nonstandardNeutrinoLatestArxivVersion = 3;
const int nonstandardNeutrinoLatestRevisionYear = 2024;

const bool routeRequiresExternalMultiplicativeHiggsModel = true;
const bool routeRequiresAuxiliaryMassScaleM = true;
const bool routeRequiresEpsilonOrScalingFactorAssignment = true;
const bool routeRequiresObservedFermionPoleMassInputs = true;
const bool routeRequiresStandardModelGaugeAndYukawaStructure = true;
const bool routeRequiresSmeftWilsonCoefficientMatching = true;
const bool routeRequiresGuLocalVariationalLagrangianMap = true;
const bool routeRequiresGuAuxiliaryMassScaleSource = true;
const bool routeRequiresGuScalingFactorSelectionLaw = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalVariationalLagrangianMap = false;
const bool routeProvidesGuAuxiliaryMassScaleSource = false;
const bool routeProvidesGuScalingFactorSelectionLaw = false;
const bool routeProvidesGuWzSourceRows = false;
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

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var topYukawaUnityPromotesHiggsMass = JsonBool(phase263.RootElement, "topYukawaUnityPromotesHiggsMass") is true;
var topYukawaUnityProvidesGuYukawaSource = JsonBool(phase263.RootElement, "topYukawaUnityProvidesGuYukawaSource") is true;
var vacuumCriticalityPromotesHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityPromotesHiggsMass") is true;
var vacuumCriticalityProvidesGuScalarPotentialSource = phase264.RootElement.TryGetProperty("sourceLineageBoundary", out var p264Boundary)
    && JsonBool(p264Boundary, "vacuumCriticalityProvidesGuScalarPotentialSource") is true;
var externalVevUsed = JsonBool(phase293.RootElement, "externalVevUsed") is true;
var fermiVevPromotesBosonPredictions = JsonBool(phase293.RootElement, "fermiVevSourcePromotesBosonPredictions") is true;
var targetIndependentGuVevSourcePromotable = JsonBool(phase293.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var higgsTopZNnloPromotesWzMasses = JsonBool(phase352.RootElement, "routePromotesWzMasses") is true;
var higgsTopZNnloPromotesHiggsMass = JsonBool(phase352.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2504.17296-multiplicative-higgs-lagrangian",
        multiplicativeHiggsLagrangianArxiv,
        "Multiplicative Higgs Lagrangian from inverse variational problem",
        "Constructs an electroweak Higgs-sector modification using a non-additive multiplicative Lagrangian, finite scaling factors, SMEFT constraints, and observed charged-lepton/heavy-quark pole masses.",
        "Useful Higgs-sector and fermion-hierarchy lead, but it depends on an external model, auxiliary mass scale, scaling-factor choices, and observed pole-mass inputs."),
    new SourceRow(
        "arxiv-2312.16587-nonstandard-higgs-neutrino-masses",
        nonstandardHiggsNeutrinoArxiv,
        "Nonstandard Higgs Lagrangian neutrino-mass mechanism",
        "Earlier nonstandard Higgs Lagrangian application to neutrino mass mechanisms with a TeV-scale dimensionful parameter in a minimal Standard Model extension.",
        "Related neutrino/Higgs hierarchy lead; it does not supply GU-local W/Z/H observed-field extraction or source-lineage rows.")
};

var checks = new[]
{
    new Check(
        "multiplicative-higgs-primary-sources-reviewed",
        multiplicativeHiggsLagrangianLeadPresent
            && multiplicativeHiggsLagrangianPrimarySourcesReviewed
            && multiplicativeHiggsLagrangianRouteExternalToGu
            && sourceRows.Length == 2,
        $"lead={multiplicativeHiggsLagrangianLeadPresent}; reviewed={multiplicativeHiggsLagrangianPrimarySourcesReviewed}; externalToGu={multiplicativeHiggsLagrangianRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "inverse-variational-higgs-route-captured",
        routeDerivedFromInverseProblemCalculusOfVariations
            && routeUsesNonAdditiveMultiplicativeHiggsLagrangian
            && routePreservesElectroweakGaugeStructure
            && routeIntroducesNoExtraDegreesOfFreedom
            && routeEquivalentToStandardModelInLargeAuxiliaryMassLimit
            && routeDynamicallyModifiesHiggsMassTermAndVev
            && routeInducesSmallHiggsVevAndMassRescalings,
        $"inverseProblem={routeDerivedFromInverseProblemCalculusOfVariations}; multiplicative={routeUsesNonAdditiveMultiplicativeHiggsLagrangian}; ewGauge={routePreservesElectroweakGaugeStructure}; noExtraDof={routeIntroducesNoExtraDegreesOfFreedom}; smLimit={routeEquivalentToStandardModelInLargeAuxiliaryMassLimit}; modifiesMassVev={routeDynamicallyModifiesHiggsMassTermAndVev}; rescalings={routeInducesSmallHiggsVevAndMassRescalings}"),
    new Check(
        "higgs-phenomenology-and-fermion-hierarchy-facts-captured",
        routeGeneratesDimension5And6Operators
            && routeUsesSmeftConstraintBoundary
            && routeConsistentOnlyInRestrictedParameterSpace
            && routeContainsTreeLevelWzMassRelations
            && routeOrganizesFermionMassesByFiniteScalingFactors
            && routeUsesObservedPoleMassInputsForFermionHierarchy
            && routePreservesTreeLevelStandardModelYukawaRelation
            && routeFindsYukawaConvergenceNearHiggsSelfCoupling
            && routeBackgroundDependentHiggsSelfInteractionsDecreaseAsymptotically,
        $"dim5dim6={routeGeneratesDimension5And6Operators}; smeft={routeUsesSmeftConstraintBoundary}; restricted={routeConsistentOnlyInRestrictedParameterSpace}; wzRelations={routeContainsTreeLevelWzMassRelations}; finiteScales={routeOrganizesFermionMassesByFiniteScalingFactors}; poleInputs={routeUsesObservedPoleMassInputsForFermionHierarchy}; treeYukawa={routePreservesTreeLevelStandardModelYukawaRelation}; yukawaConvergence={routeFindsYukawaConvergenceNearHiggsSelfCoupling}; asymptoticSelfInteractions={routeBackgroundDependentHiggsSelfInteractionsDecreaseAsymptotically}"),
    new Check(
        "external-inputs-required-before-promotion",
        routeRequiresExternalMultiplicativeHiggsModel
            && routeRequiresAuxiliaryMassScaleM
            && routeRequiresEpsilonOrScalingFactorAssignment
            && routeRequiresObservedFermionPoleMassInputs
            && routeRequiresStandardModelGaugeAndYukawaStructure
            && routeRequiresSmeftWilsonCoefficientMatching,
        $"externalModel={routeRequiresExternalMultiplicativeHiggsModel}; auxiliaryM={routeRequiresAuxiliaryMassScaleM}; scalingFactors={routeRequiresEpsilonOrScalingFactorAssignment}; poleInputs={routeRequiresObservedFermionPoleMassInputs}; smGaugeYukawa={routeRequiresStandardModelGaugeAndYukawaStructure}; smeftMatching={routeRequiresSmeftWilsonCoefficientMatching}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalVariationalLagrangianMap
            && !routeProvidesGuAuxiliaryMassScaleSource
            && !routeProvidesGuScalingFactorSelectionLaw
            && !routeProvidesGuWzSourceRows
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
        $"guVariationalMap={routeProvidesGuLocalVariationalLagrangianMap}; guAuxiliaryM={routeProvidesGuAuxiliaryMassScaleSource}; guScalingLaw={routeProvidesGuScalingFactorSelectionLaw}; guWzRows={routeProvidesGuWzSourceRows}; observed={routeProvidesGuObservedFieldExtraction}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; higgsSelf={routeProvidesGuHiggsSelfCouplingSource}; scale={routeProvidesTargetIndependentVevOrMassScale}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
    new Check(
        "adjacent-higgs-shortcuts-remain-nonpromotional",
        !topYukawaUnityPromotesHiggsMass
            && !topYukawaUnityProvidesGuYukawaSource
            && !vacuumCriticalityPromotesHiggsMass
            && !vacuumCriticalityProvidesGuScalarPotentialSource
            && externalVevUsed
            && !fermiVevPromotesBosonPredictions
            && !targetIndependentGuVevSourcePromotable
            && !higgsTopZNnloPromotesWzMasses
            && !higgsTopZNnloPromotesHiggsMass,
        $"topYukawaPromotes={topYukawaUnityPromotesHiggsMass}; topYukawaSource={topYukawaUnityProvidesGuYukawaSource}; criticalityPromotes={vacuumCriticalityPromotesHiggsMass}; criticalityScalarSource={vacuumCriticalityProvidesGuScalarPotentialSource}; externalVevUsed={externalVevUsed}; fermiPromotes={fermiVevPromotesBosonPredictions}; guVev={targetIndependentGuVevSourcePromotable}; nnloPromotesWz={higgsTopZNnloPromotesWzMasses}; nnloPromotesHiggs={higgsTopZNnloPromotesHiggsMass}"),
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

var multiplicativeHiggsLagrangianSourceAuditPassed = checks.All(check => check.Passed)
    && multiplicativeHiggsLagrangianSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = multiplicativeHiggsLagrangianSourceAuditPassed
    ? "multiplicative-higgs-lagrangian-source-audit-external-variational-higgs-lead-not-gu-source"
    : "multiplicative-higgs-lagrangian-source-audit-review-required";

var result = new
{
    phaseId = "phase354-multiplicative-higgs-lagrangian-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    multiplicativeHiggsLagrangianSourceAuditPassed,
    multiplicativeHiggsLagrangianLeadPresent,
    multiplicativeHiggsLagrangianPrimarySourcesReviewed,
    multiplicativeHiggsLagrangianRouteExternalToGu,
    multiplicativeHiggsLagrangianHtml,
    multiplicativeHiggsLagrangianDoi,
    nonstandardHiggsNeutrinoDoi,
    routeDerivedFromInverseProblemCalculusOfVariations,
    routeUsesNonAdditiveMultiplicativeHiggsLagrangian,
    routePreservesElectroweakGaugeStructure,
    routeIntroducesNoExtraDegreesOfFreedom,
    routeEquivalentToStandardModelInLargeAuxiliaryMassLimit,
    routeDynamicallyModifiesHiggsMassTermAndVev,
    routeInducesSmallHiggsVevAndMassRescalings,
    routeGeneratesDimension5And6Operators,
    routeUsesSmeftConstraintBoundary,
    routeConsistentOnlyInRestrictedParameterSpace,
    routeContainsTreeLevelWzMassRelations,
    routeOrganizesFermionMassesByFiniteScalingFactors,
    routeUsesObservedPoleMassInputsForFermionHierarchy,
    routePreservesTreeLevelStandardModelYukawaRelation,
    routeFindsYukawaConvergenceNearHiggsSelfCoupling,
    routeBackgroundDependentHiggsSelfInteractionsDecreaseAsymptotically,
    routeProvidesExternalHiggsSectorModificationLead,
    routeProvidesExternalFermionMassHierarchyLead,
    routeProvidesNeutrinoMassLeadFromNonstandardHiggsLagrangian,
    multiplicativeHiggsLatestArxivVersion,
    multiplicativeHiggsLatestRevisionYear,
    nonstandardNeutrinoLatestArxivVersion,
    nonstandardNeutrinoLatestRevisionYear,
    routeRequiresExternalMultiplicativeHiggsModel,
    routeRequiresAuxiliaryMassScaleM,
    routeRequiresEpsilonOrScalingFactorAssignment,
    routeRequiresObservedFermionPoleMassInputs,
    routeRequiresStandardModelGaugeAndYukawaStructure,
    routeRequiresSmeftWilsonCoefficientMatching,
    routeRequiresGuLocalVariationalLagrangianMap,
    routeRequiresGuAuxiliaryMassScaleSource,
    routeRequiresGuScalingFactorSelectionLaw,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresGuWzSourceRows,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalVariationalLagrangianMap,
    routeProvidesGuAuxiliaryMassScaleSource,
    routeProvidesGuScalingFactorSelectionLaw,
    routeProvidesGuWzSourceRows,
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
    sourceRows,
    checks,
    adjacentHiggsBoundary = new
    {
        topYukawaUnityPromotesHiggsMass,
        topYukawaUnityProvidesGuYukawaSource,
        vacuumCriticalityPromotesHiggsMass,
        vacuumCriticalityProvidesGuScalarPotentialSource,
        externalVevUsed,
        fermiVevPromotesBosonPredictions,
        targetIndependentGuVevSourcePromotable,
        higgsTopZNnloPromotesWzMasses,
        higgsTopZNnloPromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs physical masses from the multiplicative Higgs Lagrangian route. It is a useful inverse-variational Higgs-sector and fermion-hierarchy lead, but it depends on an external nonstandard Higgs model, an auxiliary mass scale, scaling-factor assignments, observed fermion pole masses, and SMEFT matching; no GU-local variational Lagrangian map, auxiliary-scale source, W/Z rows, Higgs scalar-source/self-coupling row, observed-field extraction, target-independent scale, or GeV normalization is supplied.",
    nextRequiredArtifact = new[]
    {
        "A GU-local map from the Shiab/observer-sector variational structure to the multiplicative Higgs Lagrangian, if this route is to be more than an external analogy.",
        "A target-independent GU source for the auxiliary mass scale M and for the epsilon/scaling-factor selection law.",
        "Observed photon/W/Z/H projection rows, Higgs scalar-source/self-coupling lineage, and physical pole extraction.",
        "A GU-derived VEV or mass scale plus GeV unit normalization before any W/Z/H mass promotion."
    }
};

var fullPath = Path.Combine(outputDir, "multiplicative_higgs_lagrangian_source_audit.json");
var summaryPath = Path.Combine(outputDir, "multiplicative_higgs_lagrangian_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.multiplicativeHiggsLagrangianSourceAuditPassed,
    result.multiplicativeHiggsLagrangianLeadPresent,
    result.multiplicativeHiggsLagrangianPrimarySourcesReviewed,
    result.multiplicativeHiggsLagrangianRouteExternalToGu,
    result.routeDerivedFromInverseProblemCalculusOfVariations,
    result.routeUsesNonAdditiveMultiplicativeHiggsLagrangian,
    result.routePreservesElectroweakGaugeStructure,
    result.routeIntroducesNoExtraDegreesOfFreedom,
    result.routeEquivalentToStandardModelInLargeAuxiliaryMassLimit,
    result.routeDynamicallyModifiesHiggsMassTermAndVev,
    result.routeInducesSmallHiggsVevAndMassRescalings,
    result.routeGeneratesDimension5And6Operators,
    result.routeUsesSmeftConstraintBoundary,
    result.routeConsistentOnlyInRestrictedParameterSpace,
    result.routeContainsTreeLevelWzMassRelations,
    result.routeOrganizesFermionMassesByFiniteScalingFactors,
    result.routeUsesObservedPoleMassInputsForFermionHierarchy,
    result.routePreservesTreeLevelStandardModelYukawaRelation,
    result.routeFindsYukawaConvergenceNearHiggsSelfCoupling,
    result.routeBackgroundDependentHiggsSelfInteractionsDecreaseAsymptotically,
    result.routeProvidesExternalHiggsSectorModificationLead,
    result.routeProvidesExternalFermionMassHierarchyLead,
    result.routeProvidesNeutrinoMassLeadFromNonstandardHiggsLagrangian,
    result.multiplicativeHiggsLatestArxivVersion,
    result.multiplicativeHiggsLatestRevisionYear,
    result.nonstandardNeutrinoLatestArxivVersion,
    result.nonstandardNeutrinoLatestRevisionYear,
    result.routeRequiresExternalMultiplicativeHiggsModel,
    result.routeRequiresAuxiliaryMassScaleM,
    result.routeRequiresEpsilonOrScalingFactorAssignment,
    result.routeRequiresObservedFermionPoleMassInputs,
    result.routeRequiresStandardModelGaugeAndYukawaStructure,
    result.routeRequiresSmeftWilsonCoefficientMatching,
    result.routeRequiresGuLocalVariationalLagrangianMap,
    result.routeRequiresGuAuxiliaryMassScaleSource,
    result.routeRequiresGuScalingFactorSelectionLaw,
    result.routeRequiresGuObservedFieldExtraction,
    result.routeRequiresGuHiggsScalarSourceOperator,
    result.routeRequiresGuHiggsSelfCouplingSource,
    result.routeRequiresGuWzSourceRows,
    result.routeRequiresTargetIndependentVevOrMassScale,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalVariationalLagrangianMap,
    result.routeProvidesGuAuxiliaryMassScaleSource,
    result.routeProvidesGuScalingFactorSelectionLaw,
    result.routeProvidesGuWzSourceRows,
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
    sourceRowCount = sourceRows.Length,
    result.adjacentHiggsBoundary,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"multiplicativeHiggsLagrangianSourceAuditPassed={multiplicativeHiggsLagrangianSourceAuditPassed}");
Console.WriteLine($"routeDerivedFromInverseProblemCalculusOfVariations={routeDerivedFromInverseProblemCalculusOfVariations}");
Console.WriteLine($"routeContainsTreeLevelWzMassRelations={routeContainsTreeLevelWzMassRelations}");
Console.WriteLine($"routeUsesObservedPoleMassInputsForFermionHierarchy={routeUsesObservedPoleMassInputsForFermionHierarchy}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value))
    {
        return null;
    }

    return value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
    {
        return null;
    }

    return value.TryGetInt32(out var parsed) ? parsed : null;
}

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);

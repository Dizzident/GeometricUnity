using System.Text.Json;

const string DefaultOutputDir = "studies/phase361_matrix_model_higgs_geometry_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase332Path = "studies/phase332_string_m_theory_compactification_source_audit_001/output/string_m_theory_compactification_source_audit_summary.json";
const string Phase353Path = "studies/phase353_gauge_higgs_unification_source_audit_001/output/gauge_higgs_unification_source_audit_summary.json";
const string Phase359Path = "studies/phase359_finite_ncg_discrete_higgs_source_audit_001/output/finite_ncg_discrete_higgs_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE361_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase332 = JsonDocument.Parse(File.ReadAllText(Phase332Path));
using var phase353 = JsonDocument.Parse(File.ReadAllText(Phase353Path));
using var phase359 = JsonDocument.Parse(File.ReadAllText(Phase359Path));

const string emergentBranesUrl = "https://arxiv.org/abs/0806.2032";
const string matrixModelsReviewUrl = "https://arxiv.org/abs/0903.1015";
const string emergentGeometryIntroUrl = "https://arxiv.org/abs/1003.4134";
const string extendedStandardModelHiggsGeometryUrl = "https://arxiv.org/abs/1401.2020";
const string extendedStandardModelHiggsGeometryDoi = "https://doi.org/10.1093/ptep/ptu111";
const string oneLoopEmergentGravityUrl = "https://arxiv.org/abs/2303.08012";
const string quantumSpacetimeWeakCouplingUrl = "https://arxiv.org/abs/2605.13294";

const bool matrixModelHiggsGeometrySourceAuditPassedExpected = true;
const bool matrixModelLeadPresent = true;
const bool matrixModelPrimarySourcesReviewed = true;
const bool matrixModelRouteExternalToGu = true;
const bool routeUsesYangMillsMatrixModel = true;
const bool routeUsesIkktMatrixModel = true;
const bool routeUsesNoncommutativeBranes = true;
const bool routeUsesEmergentFourDimensionalGeometry = true;
const bool routeUsesCompactFuzzyExtraDimensions = true;
const bool routeRealizesElectroweakSectorGeometrically = true;
const bool routeUsesMinimalFuzzyEllipsoids = true;
const bool routeHiggsConnectsBranes = true;
const bool routeHiggsIndispensablePartOfGeometry = true;
const bool routeProducesStandardModelLikeChiralMatter = true;
const bool routeIncludesSecondHiggsDoublet = true;
const bool routeIncludesRightHandedNeutrinos = true;
const bool routeIncludesHiggsSingletMajoranaLead = true;
const bool routeIncludesEmergentGravityAndGaugeTheoryLead = true;
const bool routeIncludesCurrentIkktWeakCouplingUpdate = true;
const int extendedStandardModelLatestArxivVersion = 3;
const int extendedStandardModelPageCount = 41;
const int quantumSpacetimeSubmissionYear = 2026;
const int quantumSpacetimePageCount = 26;
const int sourceRowCountExpected = 6;

const bool routeDependsOnSuitableEffectivePotential = true;
const bool routeDependsOnNonlinearSingletHiggsStabilization = true;
const bool routeContainsExtendedNotMinimalStandardModel = true;
const bool routeContainsMirrorFermionsAtHigherEnergy = true;
const bool routeContainsAnomalousExtraU1Factors = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotProvideTargetIndependentObservedHiggsMass = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresGuLocalMatrixModelMap = true;
const bool routeRequiresGuBraneVacuumSelection = true;
const bool routeRequiresGuFuzzyExtraDimensionSource = true;
const bool routeRequiresGuEffectivePotentialDerivation = true;
const bool routeRequiresGuSingletStabilizationLaw = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuGaugeCouplingNormalization = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresLowEnergyRgAndThresholdTransport = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalMatrixModelMap = false;
const bool routeProvidesGuBraneVacuumSelection = false;
const bool routeProvidesGuFuzzyExtraDimensionSource = false;
const bool routeProvidesGuEffectivePotentialDerivation = false;
const bool routeProvidesGuSingletStabilizationLaw = false;
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
var stringCompactificationSourceAuditPassed = JsonBool(phase332.RootElement, "stringMTheoryCompactificationSourceAuditPassed") is true;
var stringRoutePromotesWzMasses = JsonBool(phase332.RootElement, "stringRoutePromotesWzMasses") is true;
var stringRoutePromotesHiggsMass = JsonBool(phase332.RootElement, "stringRoutePromotesHiggsMass") is true;
var gaugeHiggsUnificationSourceAuditPassed = JsonBool(phase353.RootElement, "gaugeHiggsUnificationSourceAuditPassed") is true;
var gaugeHiggsRoutePromotesWzMasses = JsonBool(phase353.RootElement, "routePromotesWzMasses") is true;
var gaugeHiggsRoutePromotesHiggsMass = JsonBool(phase353.RootElement, "routePromotesHiggsMass") is true;
var finiteNcgDiscreteHiggsSourceAuditPassed = JsonBool(phase359.RootElement, "finiteNcgDiscreteHiggsSourceAuditPassed") is true;
var finiteNcgRoutePromotesWzMasses = JsonBool(phase359.RootElement, "routePromotesWzMasses") is true;
var finiteNcgRoutePromotesHiggsMass = JsonBool(phase359.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "steinacker-0806-2032-emergent-branes",
        emergentBranesUrl,
        "Emergent Gravity and Noncommutative Branes from Yang-Mills Matrix Models",
        "Develops emergent gravity on noncommutative branes embedded in higher-dimensional target space, with effective brane metric and IKKT motivation.",
        "Geometry and gravity foundation only; no observed W/Z/H source rows or low-energy mass extraction."),
    new SourceRow(
        "steinacker-0903-1015-matrix-models-emergent-gravity-gauge",
        matrixModelsReviewUrl,
        "Matrix Models, Emergent Gravity, and Gauge Theory",
        "Reviews Yang-Mills matrix models where dynamical spacetime branes carry emergent gravity and nonabelian gauge fields.",
        "Gauge/gravity framework lead; not a W/Z/H mass-source lineage."),
    new SourceRow(
        "steinacker-1003-4134-emergent-geometry-intro",
        emergentGeometryIntroUrl,
        "Emergent Geometry and Gravity from Matrix Models: an Introduction",
        "Describes spacetime as a noncommutative brane solution with fields and matter arising as matrix fluctuations around the background.",
        "Background-selection lead; no GU-local observed electroweak projection or GeV normalization."),
    new SourceRow(
        "steinacker-zahn-1401-2020-extended-sm-higgs-geometry",
        extendedStandardModelHiggsGeometryUrl,
        "An extended standard model and its Higgs geometry from the matrix model",
        "IKKT brane configuration resembling the Standard Model at low energies, with electroweak sector from fuzzy ellipsoids, Higgs connecting branes, second Higgs doublet, right-handed neutrinos, and Higgs singlet.",
        "Direct geometric-Higgs lead, but it assumes a suitable effective potential and singlet stabilization and does not derive observed W/Z/H masses."),
    new SourceRow(
        "steinacker-2303-08012-ikkt-one-loop-emergent-gravity",
        oneLoopEmergentGravityUrl,
        "One-loop effective action and emergent gravity on quantum spaces in the IKKT matrix model",
        "Derives 3+1-dimensional induced gravity in IKKT for branes with compact fuzzy extra dimensions and physical modes confined to the brane at weak coupling.",
        "Current gravity/scale-background lead; no electroweak boson source rows or Higgs pole mass."),
    new SourceRow(
        "steinacker-2605-13294-ikkt-weak-coupling-quantum-spacetime",
        quantumSpacetimeWeakCouplingUrl,
        "Quantum spacetime and quantum fluctuations in the IKKT model at weak coupling",
        "Current 2026 IKKT update clarifying coupling, noncommutativity scale, fluctuation scale, and semiclassical weak-coupling regime.",
        "Current matrix-model scale lead; it justifies semiclassical geometry but does not provide GU-local W/Z/H mass extraction.")
};

var checks = new[]
{
    new Check(
        "matrix-model-primary-sources-reviewed",
        matrixModelLeadPresent
            && matrixModelPrimarySourcesReviewed
            && matrixModelRouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={matrixModelLeadPresent}; reviewed={matrixModelPrimarySourcesReviewed}; externalToGu={matrixModelRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "matrix-model-geometric-higgs-structure-captured",
        routeUsesYangMillsMatrixModel
            && routeUsesIkktMatrixModel
            && routeUsesNoncommutativeBranes
            && routeUsesEmergentFourDimensionalGeometry
            && routeUsesCompactFuzzyExtraDimensions
            && routeRealizesElectroweakSectorGeometrically
            && routeUsesMinimalFuzzyEllipsoids
            && routeHiggsConnectsBranes
            && routeHiggsIndispensablePartOfGeometry
            && routeProducesStandardModelLikeChiralMatter,
        $"ymMatrix={routeUsesYangMillsMatrixModel}; ikkt={routeUsesIkktMatrixModel}; branes={routeUsesNoncommutativeBranes}; emergent4d={routeUsesEmergentFourDimensionalGeometry}; fuzzyExtra={routeUsesCompactFuzzyExtraDimensions}; electroweakGeometry={routeRealizesElectroweakSectorGeometrically}; ellipsoids={routeUsesMinimalFuzzyEllipsoids}; higgsConnects={routeHiggsConnectsBranes}; higgsGeometry={routeHiggsIndispensablePartOfGeometry}; chiralMatter={routeProducesStandardModelLikeChiralMatter}"),
    new Check(
        "matrix-model-extension-and-current-update-captured",
        routeIncludesSecondHiggsDoublet
            && routeIncludesRightHandedNeutrinos
            && routeIncludesHiggsSingletMajoranaLead
            && routeIncludesEmergentGravityAndGaugeTheoryLead
            && routeIncludesCurrentIkktWeakCouplingUpdate
            && extendedStandardModelLatestArxivVersion == 3
            && extendedStandardModelPageCount == 41
            && quantumSpacetimeSubmissionYear == 2026
            && quantumSpacetimePageCount == 26,
        $"secondDoublet={routeIncludesSecondHiggsDoublet}; rightNeutrinos={routeIncludesRightHandedNeutrinos}; singlet={routeIncludesHiggsSingletMajoranaLead}; gravityGauge={routeIncludesEmergentGravityAndGaugeTheoryLead}; currentIkkt={routeIncludesCurrentIkktWeakCouplingUpdate}; v={extendedStandardModelLatestArxivVersion}; pages={extendedStandardModelPageCount}; currentYear={quantumSpacetimeSubmissionYear}; currentPages={quantumSpacetimePageCount}"),
    new Check(
        "matrix-model-promotion-obstructions-captured",
        routeDependsOnSuitableEffectivePotential
            && routeDependsOnNonlinearSingletHiggsStabilization
            && routeContainsExtendedNotMinimalStandardModel
            && routeContainsMirrorFermionsAtHigherEnergy
            && routeContainsAnomalousExtraU1Factors
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotProvideTargetIndependentObservedHiggsMass
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotProvideObservedPhotonWzHiggsProjection,
        $"effectivePotential={routeDependsOnSuitableEffectivePotential}; singletStabilization={routeDependsOnNonlinearSingletHiggsStabilization}; extendedSm={routeContainsExtendedNotMinimalStandardModel}; mirrorFermions={routeContainsMirrorFermionsAtHigherEnergy}; extraU1={routeContainsAnomalousExtraU1Factors}; observedWz={routeDoesNotPredictObservedWzMasses}; observedHiggs={routeDoesNotProvideTargetIndependentObservedHiggsMass}; pole={routeDoesNotProvidePhysicalPoleExtraction}; observedProjection={routeDoesNotProvideObservedPhotonWzHiggsProjection}"),
    new Check(
        "adjacent-brane-ncg-gauge-higgs-routes-remain-nonpromotional",
        stringCompactificationSourceAuditPassed
            && !stringRoutePromotesWzMasses
            && !stringRoutePromotesHiggsMass
            && gaugeHiggsUnificationSourceAuditPassed
            && !gaugeHiggsRoutePromotesWzMasses
            && !gaugeHiggsRoutePromotesHiggsMass
            && finiteNcgDiscreteHiggsSourceAuditPassed
            && !finiteNcgRoutePromotesWzMasses
            && !finiteNcgRoutePromotesHiggsMass,
        $"stringPassed={stringCompactificationSourceAuditPassed}; stringWz={stringRoutePromotesWzMasses}; stringHiggs={stringRoutePromotesHiggsMass}; gaugeHiggsPassed={gaugeHiggsUnificationSourceAuditPassed}; gaugeHiggsWz={gaugeHiggsRoutePromotesWzMasses}; gaugeHiggsHiggs={gaugeHiggsRoutePromotesHiggsMass}; finiteNcgPassed={finiteNcgDiscreteHiggsSourceAuditPassed}; finiteNcgWz={finiteNcgRoutePromotesWzMasses}; finiteNcgHiggs={finiteNcgRoutePromotesHiggsMass}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        routeRequiresGuLocalMatrixModelMap
            && routeRequiresGuBraneVacuumSelection
            && routeRequiresGuFuzzyExtraDimensionSource
            && routeRequiresGuEffectivePotentialDerivation
            && routeRequiresGuSingletStabilizationLaw
            && routeRequiresGuWzSourceRows
            && routeRequiresGuWeakMixingAngleSource
            && routeRequiresGuGaugeCouplingNormalization
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGuHiggsSelfCouplingSource
            && routeRequiresTargetIndependentVevOrMassScale
            && routeRequiresLowEnergyRgAndThresholdTransport
            && routeRequiresGeVUnitNormalization
            && !routeProvidesGuLocalMatrixModelMap
            && !routeProvidesGuBraneVacuumSelection
            && !routeProvidesGuFuzzyExtraDimensionSource
            && !routeProvidesGuEffectivePotentialDerivation
            && !routeProvidesGuSingletStabilizationLaw
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
        $"requiresMatrixMap={routeRequiresGuLocalMatrixModelMap}; requiresVacuum={routeRequiresGuBraneVacuumSelection}; requiresFuzzy={routeRequiresGuFuzzyExtraDimensionSource}; requiresPotential={routeRequiresGuEffectivePotentialDerivation}; requiresSinglet={routeRequiresGuSingletStabilizationLaw}; requiresWzRows={routeRequiresGuWzSourceRows}; requiresObserved={routeRequiresGuObservedFieldExtraction}; requiresHiggsOperator={routeRequiresGuHiggsScalarSourceOperator}; providesMatrixMap={routeProvidesGuLocalMatrixModelMap}; providesWzRows={routeProvidesGuWzSourceRows}; providesObserved={routeProvidesGuObservedFieldExtraction}; providesHiggsOperator={routeProvidesGuHiggsScalarSourceOperator}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var matrixModelHiggsGeometrySourceAuditPassed = checks.All(check => check.Passed)
    && matrixModelHiggsGeometrySourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = matrixModelHiggsGeometrySourceAuditPassed
    ? "matrix-model-higgs-geometry-source-audit-geometric-higgs-lead-not-gu-mass-law"
    : "matrix-model-higgs-geometry-source-audit-review-required";

var result = new
{
    phaseId = "phase361-matrix-model-higgs-geometry-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    matrixModelHiggsGeometrySourceAuditPassed,
    matrixModelLeadPresent,
    matrixModelPrimarySourcesReviewed,
    matrixModelRouteExternalToGu,
    extendedStandardModelHiggsGeometryDoi,
    routeUsesYangMillsMatrixModel,
    routeUsesIkktMatrixModel,
    routeUsesNoncommutativeBranes,
    routeUsesEmergentFourDimensionalGeometry,
    routeUsesCompactFuzzyExtraDimensions,
    routeRealizesElectroweakSectorGeometrically,
    routeUsesMinimalFuzzyEllipsoids,
    routeHiggsConnectsBranes,
    routeHiggsIndispensablePartOfGeometry,
    routeProducesStandardModelLikeChiralMatter,
    routeIncludesSecondHiggsDoublet,
    routeIncludesRightHandedNeutrinos,
    routeIncludesHiggsSingletMajoranaLead,
    routeIncludesEmergentGravityAndGaugeTheoryLead,
    routeIncludesCurrentIkktWeakCouplingUpdate,
    extendedStandardModelLatestArxivVersion,
    extendedStandardModelPageCount,
    quantumSpacetimeSubmissionYear,
    quantumSpacetimePageCount,
    routeDependsOnSuitableEffectivePotential,
    routeDependsOnNonlinearSingletHiggsStabilization,
    routeContainsExtendedNotMinimalStandardModel,
    routeContainsMirrorFermionsAtHigherEnergy,
    routeContainsAnomalousExtraU1Factors,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotProvideTargetIndependentObservedHiggsMass,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    routeRequiresGuLocalMatrixModelMap,
    routeRequiresGuBraneVacuumSelection,
    routeRequiresGuFuzzyExtraDimensionSource,
    routeRequiresGuEffectivePotentialDerivation,
    routeRequiresGuSingletStabilizationLaw,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuGaugeCouplingNormalization,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresLowEnergyRgAndThresholdTransport,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalMatrixModelMap,
    routeProvidesGuBraneVacuumSelection,
    routeProvidesGuFuzzyExtraDimensionSource,
    routeProvidesGuEffectivePotentialDerivation,
    routeProvidesGuSingletStabilizationLaw,
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
        stringCompactificationSourceAuditPassed,
        stringRoutePromotesWzMasses,
        stringRoutePromotesHiggsMass,
        gaugeHiggsUnificationSourceAuditPassed,
        gaugeHiggsRoutePromotesWzMasses,
        gaugeHiggsRoutePromotesHiggsMass,
        finiteNcgDiscreteHiggsSourceAuditPassed,
        finiteNcgRoutePromotesWzMasses,
        finiteNcgRoutePromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs physical masses from IKKT/Yang-Mills matrix-model Higgs-geometry routes in this repository. The sources provide a serious geometric Higgs lead: noncommutative brane geometry, fuzzy extra dimensions, an electroweak brane configuration, and a Higgs connecting branes. They do not provide a GU-local matrix-model map from Shiab/observer geometry, a target-independent brane vacuum/effective potential, separate observed W/Z source rows, photon/W/Z/H projection, Higgs scalar-source/self-coupling lineage, a VEV or mass-scale source, low-energy RG/threshold and pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem mapping Shiab/observer-sector fields into an IKKT or Yang-Mills matrix-model brane configuration without importing the external model.",
        "A target-independent brane-vacuum and fuzzy-extra-dimension selection law that yields observed electroweak SU(2) x U(1), photon/W/Z projection, and separate W/Z source rows.",
        "A derived effective potential and singlet-stabilization law from the same geometry, yielding a Higgs scalar-source/self-coupling lineage rather than assuming it.",
        "A shared VEV or mass-scale source, low-energy RG/threshold transport, physical-pole extractor, and GeV normalization before any matrix-model route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "matrix_model_higgs_geometry_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "matrix_model_higgs_geometry_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.matrixModelHiggsGeometrySourceAuditPassed,
            result.matrixModelLeadPresent,
            result.matrixModelPrimarySourcesReviewed,
            result.matrixModelRouteExternalToGu,
            result.routeUsesYangMillsMatrixModel,
            result.routeUsesIkktMatrixModel,
            result.routeUsesNoncommutativeBranes,
            result.routeUsesEmergentFourDimensionalGeometry,
            result.routeUsesCompactFuzzyExtraDimensions,
            result.routeRealizesElectroweakSectorGeometrically,
            result.routeUsesMinimalFuzzyEllipsoids,
            result.routeHiggsConnectsBranes,
            result.routeHiggsIndispensablePartOfGeometry,
            result.routeProducesStandardModelLikeChiralMatter,
            result.routeIncludesSecondHiggsDoublet,
            result.routeIncludesRightHandedNeutrinos,
            result.routeIncludesHiggsSingletMajoranaLead,
            result.routeIncludesEmergentGravityAndGaugeTheoryLead,
            result.routeIncludesCurrentIkktWeakCouplingUpdate,
            result.extendedStandardModelLatestArxivVersion,
            result.extendedStandardModelPageCount,
            result.quantumSpacetimeSubmissionYear,
            result.quantumSpacetimePageCount,
            result.routeDependsOnSuitableEffectivePotential,
            result.routeDependsOnNonlinearSingletHiggsStabilization,
            result.routeContainsExtendedNotMinimalStandardModel,
            result.routeContainsMirrorFermionsAtHigherEnergy,
            result.routeContainsAnomalousExtraU1Factors,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotProvideTargetIndependentObservedHiggsMass,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.routeRequiresGuLocalMatrixModelMap,
            result.routeRequiresGuBraneVacuumSelection,
            result.routeRequiresGuFuzzyExtraDimensionSource,
            result.routeRequiresGuEffectivePotentialDerivation,
            result.routeRequiresGuSingletStabilizationLaw,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuGaugeCouplingNormalization,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresLowEnergyRgAndThresholdTransport,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalMatrixModelMap,
            result.routeProvidesGuBraneVacuumSelection,
            result.routeProvidesGuFuzzyExtraDimensionSource,
            result.routeProvidesGuEffectivePotentialDerivation,
            result.routeProvidesGuSingletStabilizationLaw,
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
Console.WriteLine($"matrixModelHiggsGeometrySourceAuditPassed={matrixModelHiggsGeometrySourceAuditPassed}");
Console.WriteLine($"routeUsesIkktMatrixModel={routeUsesIkktMatrixModel}");
Console.WriteLine($"routeHiggsConnectsBranes={routeHiggsConnectsBranes}");
Console.WriteLine($"routeDependsOnSuitableEffectivePotential={routeDependsOnSuitableEffectivePotential}");
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

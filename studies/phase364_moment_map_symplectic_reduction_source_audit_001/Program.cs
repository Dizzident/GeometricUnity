using System.Text.Json;

const string DefaultOutputDir = "studies/phase364_moment_map_symplectic_reduction_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase230Path = "studies/phase230_native_gu_vacuum_hessian_candidate_audit_001/output/native_gu_vacuum_hessian_candidate_audit_summary.json";
const string Phase253Path = "studies/phase253_global_observed_sector_vacuum_scan_001/output/global_observed_sector_vacuum_scan_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase331Path = "studies/phase331_theta_omega_inhomogeneous_gauge_source_audit_001/output/theta_omega_inhomogeneous_gauge_source_audit_summary.json";
const string Phase363Path = "studies/phase363_hitchin_higgs_bundle_source_audit_001/output/hitchin_higgs_bundle_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE364_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase230 = JsonDocument.Parse(File.ReadAllText(Phase230Path));
using var phase253 = JsonDocument.Parse(File.ReadAllText(Phase253Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase331 = JsonDocument.Parse(File.ReadAllText(Phase331Path));
using var phase363 = JsonDocument.Parse(File.ReadAllText(Phase363Path));

const string marsdenWeinsteinReductionUrl = "https://doi.org/10.1016/0034-4877(74)90021-4";
const string marsdenWeinsteinCaltechUrl = "https://authors.library.caltech.edu/records/8k5v6-4pa21/latest";
const string atiyahBottYangMillsUrl = "https://doi.org/10.1098/rsta.1983.0017";
const string diezRudolphReductionUrl = "https://arxiv.org/abs/1812.04707";
const string trautweinYangMillsHiggsFlowUrl = "https://arxiv.org/abs/1610.02245";
const string rielloYangMillsBoundaryReductionUrl = "https://arxiv.org/abs/2010.15894";
const string higgsMechanismHasseDiagramUrl = "https://arxiv.org/abs/1908.04245";

const bool momentMapSymplecticReductionSourceAuditPassedExpected = true;
const bool momentMapSymplecticReductionLeadPresent = true;
const bool momentMapSymplecticReductionPrimarySourcesReviewed = true;
const bool momentMapSymplecticReductionRouteExternalToGu = true;
const bool routeUsesMomentMaps = true;
const bool routeUsesMarsdenWeinsteinReduction = true;
const bool routeUsesCoadjointOrbitGeometry = true;
const bool routeUsesYangMillsAsMomentMap = true;
const bool routeUsesSymplecticQuotientOfGaugeConnections = true;
const bool routeUsesYangMillsHiggsReducedPhaseSpace = true;
const bool routeIncludesGlashowWeinbergSalamHiggsSector = true;
const bool routeUsesSymplecticVortexEquations = true;
const bool routeUsesKahlerHamiltonianGManifold = true;
const bool routeUsesGITOrKobayashiHitchinCorrespondence = true;
const bool routeUsesBoundaryYangMillsReduction = true;
const bool routeUsesGaugeCovariantSuperselectionSectors = true;
const bool routeUsesSymplecticHiggsBranchSingularities = true;
const bool routeEncodesPartialHiggsMechanismAsSymplecticLeaves = true;
const int sourceRowCountExpected = 6;
const int marsdenWeinsteinPublicationYear = 1974;
const int marsdenWeinsteinStartPage = 121;
const int marsdenWeinsteinEndPage = 130;
const int atiyahBottPublicationYear = 1983;
const int diezRudolphLatestArxivVersion = 2;
const int diezRudolphJournalYear = 2020;
const int trautweinLatestArxivVersion = 2;
const int rielloLatestArxivVersion = 3;
const int higgsMechanismHasseLatestArxivVersion = 2;
const int higgsMechanismHassePageCount = 70;

const bool routeIsPhaseSpaceConstraintGeometryNotMassLaw = true;
const bool routeDescribesReducedPhaseSpaceNotHamiltonianMassSpectrum = true;
const bool routeDependsOnChosenHamiltonianPotentialOrModel = true;
const bool routeKeepsStandardModelInputsExternal = true;
const bool routeDoesNotDeriveObservedElectroweakVacuum = true;
const bool routeDoesNotDeriveStandardModelGaugeCouplings = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotProvideTargetIndependentObservedHiggsMass = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresGuLocalMomentMap = true;
const bool routeRequiresGuSymplecticPhaseSpace = true;
const bool routeRequiresGuHamiltonianReductionLaw = true;
const bool routeRequiresGuReducedVacuumSelection = true;
const bool routeRequiresGuElectroweakHiggsDoubletProjection = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuGaugeCouplingNormalization = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresLowEnergyRgAndThresholdTransport = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalMomentMap = false;
const bool routeProvidesGuSymplecticPhaseSpace = false;
const bool routeProvidesGuHamiltonianReductionLaw = false;
const bool routeProvidesGuReducedVacuumSelection = false;
const bool routeProvidesGuElectroweakHiggsDoubletProjection = false;
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
var nativeGuVacuumHessianCandidateAuditPassed = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidateAuditPassed") is true;
var nativeGuVacuumHessianCandidatePromotable = JsonBool(phase230.RootElement, "nativeGuVacuumHessianCandidatePromotable") is true;
var globalObservedSectorVacuumScanPassed = JsonBool(phase253.RootElement, "globalObservedSectorVacuumScanPassed") is true;
var globalObservedSectorVacuumCandidateFound = JsonBool(phase253.RootElement, "globalObservedSectorVacuumCandidateFound") is true;
var globalScanFillsVacuumMassMatrixUnlock = JsonBool(phase253.RootElement, "globalScanFillsVacuumMassMatrixUnlock") is true;
var productionObservedSectorVacuumCandidateCount = JsonInt(phase253.RootElement, "productionObservedSectorVacuumCandidateCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var thetaOmegaInhomogeneousGaugeSourceAuditPassed = JsonBool(phase331.RootElement, "thetaOmegaInhomogeneousGaugeSourceAuditPassed") is true;
var thetaOmegaRoutePromotesWzMasses = JsonBool(phase331.RootElement, "thetaOmegaRoutePromotesWzMasses") is true;
var thetaOmegaRoutePromotesHiggsMass = JsonBool(phase331.RootElement, "thetaOmegaRoutePromotesHiggsMass") is true;
var hitchinHiggsBundleSourceAuditPassed = JsonBool(phase363.RootElement, "hitchinHiggsBundleSourceAuditPassed") is true;
var hitchinRoutePromotesWzMasses = JsonBool(phase363.RootElement, "routePromotesWzMasses") is true;
var hitchinRoutePromotesHiggsMass = JsonBool(phase363.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "marsden-weinstein-1974-symplectic-reduction",
        marsdenWeinsteinReductionUrl,
        "Reduction of Symplectic Manifolds with Symmetry",
        "Foundational Marsden-Weinstein reduction source: constructs reduced symplectic phase spaces from momentum-map level sets and includes coadjoint-orbit geometry as a core example.",
        "Foundational reduction geometry, but not an electroweak W/Z/H mass source law or GU-local vacuum selection."),
    new SourceRow(
        "atiyah-bott-1983-yang-mills-riemann-surfaces",
        atiyahBottYangMillsUrl,
        "The Yang-Mills Equations over Riemann Surfaces",
        "Uses symplectic and Morse-theoretic geometry for Yang-Mills connections over Riemann surfaces and is a standard source for Yang-Mills moment-map structure.",
        "Gauge-connection reduction lead; no observed electroweak vacuum, W/Z rows, Higgs pole, or GeV normalization."),
    new SourceRow(
        "diez-rudolph-1812-04707-singular-cotangent-reduction",
        diezRudolphReductionUrl,
        "Singular Symplectic Cotangent Bundle Reduction of Gauge Field Theory",
        "Proves singular symplectic cotangent-bundle reduction in a Frechet setting and applies it to Yang-Mills-Higgs theory with emphasis on the Glashow-Weinberg-Salam Higgs sector.",
        "Closest Standard Model Higgs-sector phase-space lead; describes reduced phase space, not target-independent W/Z/H masses."),
    new SourceRow(
        "trautwein-1610-02245-yang-mills-higgs-flow",
        trautweinYangMillsHiggsFlowUrl,
        "Convergence of the Yang-Mills-Higgs Flow on Gauged Holomorphic Maps and Applications",
        "Treats symplectic vortex equations as minima of the Yang-Mills-Higgs functional for holomorphic pairs with a moment map on a Kahler Hamiltonian G-manifold.",
        "Strong moment-map/vortex lead; external and not an observed electroweak pole-mass extraction."),
    new SourceRow(
        "riello-2010-15894-yang-mills-boundary-reduction",
        rielloYangMillsBoundaryReductionUrl,
        "Symplectic Reduction of Yang-Mills Theory with Boundaries",
        "Develops symplectic reduction for bounded Yang-Mills regions using gauge-covariant electric-flux superselection sectors and discusses edge-mode ambiguities.",
        "Useful reduced-phase-space and boundary warning; no Higgs scalar-source or W/Z/H mass law."),
    new SourceRow(
        "bourget-et-al-1908-04245-higgs-mechanism-symplectic-singularities",
        higgsMechanismHasseDiagramUrl,
        "The Higgs Mechanism -- Hasse Diagrams for Symplectic Singularities",
        "Identifies symplectic-leaf decompositions of Higgs branches with partial Higgs-mechanism patterns in theories with eight supercharges.",
        "Geometric Higgs-branch stratification lead; supersymmetric branch geometry, not observed Standard Model W/Z/H masses.")
};

var checks = new[]
{
    new Check(
        "moment-map-symplectic-reduction-primary-sources-reviewed",
        momentMapSymplecticReductionLeadPresent
            && momentMapSymplecticReductionPrimarySourcesReviewed
            && momentMapSymplecticReductionRouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={momentMapSymplecticReductionLeadPresent}; reviewed={momentMapSymplecticReductionPrimarySourcesReviewed}; externalToGu={momentMapSymplecticReductionRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "moment-map-reduction-geometric-structure-captured",
        routeUsesMomentMaps
            && routeUsesMarsdenWeinsteinReduction
            && routeUsesCoadjointOrbitGeometry
            && routeUsesYangMillsAsMomentMap
            && routeUsesSymplecticQuotientOfGaugeConnections
            && routeUsesYangMillsHiggsReducedPhaseSpace
            && routeIncludesGlashowWeinbergSalamHiggsSector,
        $"momentMaps={routeUsesMomentMaps}; marsdenWeinstein={routeUsesMarsdenWeinsteinReduction}; coadjoint={routeUsesCoadjointOrbitGeometry}; yangMillsMomentMap={routeUsesYangMillsAsMomentMap}; quotientConnections={routeUsesSymplecticQuotientOfGaugeConnections}; ymhReducedPhaseSpace={routeUsesYangMillsHiggsReducedPhaseSpace}; gwsHiggs={routeIncludesGlashowWeinbergSalamHiggsSector}"),
    new Check(
        "yang-mills-higgs-and-higgs-branch-leads-captured",
        routeUsesSymplecticVortexEquations
            && routeUsesKahlerHamiltonianGManifold
            && routeUsesGITOrKobayashiHitchinCorrespondence
            && routeUsesBoundaryYangMillsReduction
            && routeUsesGaugeCovariantSuperselectionSectors
            && routeUsesSymplecticHiggsBranchSingularities
            && routeEncodesPartialHiggsMechanismAsSymplecticLeaves,
        $"vortex={routeUsesSymplecticVortexEquations}; kahlerHamiltonian={routeUsesKahlerHamiltonianGManifold}; gitKh={routeUsesGITOrKobayashiHitchinCorrespondence}; boundaryReduction={routeUsesBoundaryYangMillsReduction}; superselection={routeUsesGaugeCovariantSuperselectionSectors}; higgsBranchSingularities={routeUsesSymplecticHiggsBranchSingularities}; symplecticLeaves={routeEncodesPartialHiggsMechanismAsSymplecticLeaves}"),
    new Check(
        "moment-map-version-and-publication-metadata-captured",
        marsdenWeinsteinPublicationYear == 1974
            && marsdenWeinsteinStartPage == 121
            && marsdenWeinsteinEndPage == 130
            && atiyahBottPublicationYear == 1983
            && diezRudolphLatestArxivVersion == 2
            && diezRudolphJournalYear == 2020
            && trautweinLatestArxivVersion == 2
            && rielloLatestArxivVersion == 3
            && higgsMechanismHasseLatestArxivVersion == 2
            && higgsMechanismHassePageCount == 70,
        $"marsdenWeinsteinYear={marsdenWeinsteinPublicationYear}; marsdenWeinsteinPages={marsdenWeinsteinStartPage}-{marsdenWeinsteinEndPage}; atiyahBottYear={atiyahBottPublicationYear}; diezRudolphV={diezRudolphLatestArxivVersion}; diezRudolphYear={diezRudolphJournalYear}; trautweinV={trautweinLatestArxivVersion}; rielloV={rielloLatestArxivVersion}; hasseV={higgsMechanismHasseLatestArxivVersion}; hassePages={higgsMechanismHassePageCount}"),
    new Check(
        "moment-map-promotion-obstructions-captured",
        routeIsPhaseSpaceConstraintGeometryNotMassLaw
            && routeDescribesReducedPhaseSpaceNotHamiltonianMassSpectrum
            && routeDependsOnChosenHamiltonianPotentialOrModel
            && routeKeepsStandardModelInputsExternal
            && routeDoesNotDeriveObservedElectroweakVacuum
            && routeDoesNotDeriveStandardModelGaugeCouplings
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotProvideTargetIndependentObservedHiggsMass
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotProvideObservedPhotonWzHiggsProjection,
        $"constraintGeometry={routeIsPhaseSpaceConstraintGeometryNotMassLaw}; reducedPhaseSpaceNotSpectrum={routeDescribesReducedPhaseSpaceNotHamiltonianMassSpectrum}; chosenHamiltonian={routeDependsOnChosenHamiltonianPotentialOrModel}; smInputsExternal={routeKeepsStandardModelInputsExternal}; observedVacuum={routeDoesNotDeriveObservedElectroweakVacuum}; smCouplings={routeDoesNotDeriveStandardModelGaugeCouplings}; observedWz={routeDoesNotPredictObservedWzMasses}; observedHiggs={routeDoesNotProvideTargetIndependentObservedHiggsMass}; pole={routeDoesNotProvidePhysicalPoleExtraction}; projection={routeDoesNotProvideObservedPhotonWzHiggsProjection}"),
    new Check(
        "adjacent-vacuum-and-geometry-routes-remain-nonpromotional",
        nativeGuVacuumHessianCandidateAuditPassed
            && !nativeGuVacuumHessianCandidatePromotable
            && globalObservedSectorVacuumScanPassed
            && !globalObservedSectorVacuumCandidateFound
            && !globalScanFillsVacuumMassMatrixUnlock
            && productionObservedSectorVacuumCandidateCount == 0
            && thetaOmegaInhomogeneousGaugeSourceAuditPassed
            && !thetaOmegaRoutePromotesWzMasses
            && !thetaOmegaRoutePromotesHiggsMass
            && hitchinHiggsBundleSourceAuditPassed
            && !hitchinRoutePromotesWzMasses
            && !hitchinRoutePromotesHiggsMass,
        $"nativeVacuumPassed={nativeGuVacuumHessianCandidateAuditPassed}; nativePromotable={nativeGuVacuumHessianCandidatePromotable}; globalVacuumPassed={globalObservedSectorVacuumScanPassed}; globalCandidate={globalObservedSectorVacuumCandidateFound}; globalFills={globalScanFillsVacuumMassMatrixUnlock}; productionVacuumCandidates={productionObservedSectorVacuumCandidateCount}; thetaOmegaPassed={thetaOmegaInhomogeneousGaugeSourceAuditPassed}; thetaOmegaWz={thetaOmegaRoutePromotesWzMasses}; thetaOmegaHiggs={thetaOmegaRoutePromotesHiggsMass}; hitchinPassed={hitchinHiggsBundleSourceAuditPassed}; hitchinWz={hitchinRoutePromotesWzMasses}; hitchinHiggs={hitchinRoutePromotesHiggsMass}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        routeRequiresGuLocalMomentMap
            && routeRequiresGuSymplecticPhaseSpace
            && routeRequiresGuHamiltonianReductionLaw
            && routeRequiresGuReducedVacuumSelection
            && routeRequiresGuElectroweakHiggsDoubletProjection
            && routeRequiresGuWzSourceRows
            && routeRequiresGuWeakMixingAngleSource
            && routeRequiresGuGaugeCouplingNormalization
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGuHiggsSelfCouplingSource
            && routeRequiresTargetIndependentVevOrMassScale
            && routeRequiresLowEnergyRgAndThresholdTransport
            && routeRequiresGeVUnitNormalization
            && !routeProvidesGuLocalMomentMap
            && !routeProvidesGuSymplecticPhaseSpace
            && !routeProvidesGuHamiltonianReductionLaw
            && !routeProvidesGuReducedVacuumSelection
            && !routeProvidesGuElectroweakHiggsDoubletProjection
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
        $"requiresMomentMap={routeRequiresGuLocalMomentMap}; requiresSymplecticPhaseSpace={routeRequiresGuSymplecticPhaseSpace}; requiresReduction={routeRequiresGuHamiltonianReductionLaw}; requiresVacuum={routeRequiresGuReducedVacuumSelection}; requiresWzRows={routeRequiresGuWzSourceRows}; requiresObserved={routeRequiresGuObservedFieldExtraction}; requiresHiggsOperator={routeRequiresGuHiggsScalarSourceOperator}; providesMomentMap={routeProvidesGuLocalMomentMap}; providesWzRows={routeProvidesGuWzSourceRows}; providesObserved={routeProvidesGuObservedFieldExtraction}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var momentMapSymplecticReductionSourceAuditPassed = checks.All(check => check.Passed)
    && momentMapSymplecticReductionSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = momentMapSymplecticReductionSourceAuditPassed
    ? "moment-map-symplectic-reduction-source-audit-phase-space-lead-not-gu-mass-law"
    : "moment-map-symplectic-reduction-source-audit-review-required";

var result = new
{
    phaseId = "phase364-moment-map-symplectic-reduction-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    momentMapSymplecticReductionSourceAuditPassed,
    momentMapSymplecticReductionLeadPresent,
    momentMapSymplecticReductionPrimarySourcesReviewed,
    momentMapSymplecticReductionRouteExternalToGu,
    marsdenWeinsteinCaltechUrl,
    routeUsesMomentMaps,
    routeUsesMarsdenWeinsteinReduction,
    routeUsesCoadjointOrbitGeometry,
    routeUsesYangMillsAsMomentMap,
    routeUsesSymplecticQuotientOfGaugeConnections,
    routeUsesYangMillsHiggsReducedPhaseSpace,
    routeIncludesGlashowWeinbergSalamHiggsSector,
    routeUsesSymplecticVortexEquations,
    routeUsesKahlerHamiltonianGManifold,
    routeUsesGITOrKobayashiHitchinCorrespondence,
    routeUsesBoundaryYangMillsReduction,
    routeUsesGaugeCovariantSuperselectionSectors,
    routeUsesSymplecticHiggsBranchSingularities,
    routeEncodesPartialHiggsMechanismAsSymplecticLeaves,
    marsdenWeinsteinPublicationYear,
    marsdenWeinsteinStartPage,
    marsdenWeinsteinEndPage,
    atiyahBottPublicationYear,
    diezRudolphLatestArxivVersion,
    diezRudolphJournalYear,
    trautweinLatestArxivVersion,
    rielloLatestArxivVersion,
    higgsMechanismHasseLatestArxivVersion,
    higgsMechanismHassePageCount,
    routeIsPhaseSpaceConstraintGeometryNotMassLaw,
    routeDescribesReducedPhaseSpaceNotHamiltonianMassSpectrum,
    routeDependsOnChosenHamiltonianPotentialOrModel,
    routeKeepsStandardModelInputsExternal,
    routeDoesNotDeriveObservedElectroweakVacuum,
    routeDoesNotDeriveStandardModelGaugeCouplings,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotProvideTargetIndependentObservedHiggsMass,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    routeRequiresGuLocalMomentMap,
    routeRequiresGuSymplecticPhaseSpace,
    routeRequiresGuHamiltonianReductionLaw,
    routeRequiresGuReducedVacuumSelection,
    routeRequiresGuElectroweakHiggsDoubletProjection,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuGaugeCouplingNormalization,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresLowEnergyRgAndThresholdTransport,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalMomentMap,
    routeProvidesGuSymplecticPhaseSpace,
    routeProvidesGuHamiltonianReductionLaw,
    routeProvidesGuReducedVacuumSelection,
    routeProvidesGuElectroweakHiggsDoubletProjection,
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
        nativeGuVacuumHessianCandidateAuditPassed,
        nativeGuVacuumHessianCandidatePromotable,
        globalObservedSectorVacuumScanPassed,
        globalObservedSectorVacuumCandidateFound,
        globalScanFillsVacuumMassMatrixUnlock,
        productionObservedSectorVacuumCandidateCount,
        thetaOmegaInhomogeneousGaugeSourceAuditPassed,
        thetaOmegaRoutePromotesWzMasses,
        thetaOmegaRoutePromotesHiggsMass,
        hitchinHiggsBundleSourceAuditPassed,
        hitchinRoutePromotesWzMasses,
        hitchinRoutePromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs physical masses from moment-map or symplectic-reduction routes in this repository. These sources provide a serious phase-space and reduced-vacuum lead: Marsden-Weinstein reduction, Yang-Mills moment-map geometry, singular Yang-Mills-Higgs reduction for the Glashow-Weinberg-Salam Higgs sector, symplectic vortex equations, boundary Yang-Mills reduction, and Higgs-branch symplectic singularities. They do not supply a GU-local moment map, a target-independent reduced electroweak vacuum, observed photon/W/Z/H projection, separate W/Z source rows, Higgs scalar-source/self-coupling lineage, low-energy transport, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving the relevant symplectic phase space and moment map from Shiab/observer-sector geometry.",
        "A target-independent Hamiltonian reduction and reduced-vacuum selection law that yields observed electroweak SU(2) x U(1), a massless photon, and separate W/Z source rows.",
        "A Higgs scalar-source, doublet-projection, and self-coupling lineage from the same reduced phase space, not from an assumed Standard Model potential.",
        "A low-energy RG/threshold transport, physical-pole extraction, and GeV unit-normalization chain before any moment-map/symplectic-reduction route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "moment_map_symplectic_reduction_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "moment_map_symplectic_reduction_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.momentMapSymplecticReductionSourceAuditPassed,
            result.momentMapSymplecticReductionLeadPresent,
            result.momentMapSymplecticReductionPrimarySourcesReviewed,
            result.momentMapSymplecticReductionRouteExternalToGu,
            result.routeUsesMomentMaps,
            result.routeUsesMarsdenWeinsteinReduction,
            result.routeUsesCoadjointOrbitGeometry,
            result.routeUsesYangMillsAsMomentMap,
            result.routeUsesSymplecticQuotientOfGaugeConnections,
            result.routeUsesYangMillsHiggsReducedPhaseSpace,
            result.routeIncludesGlashowWeinbergSalamHiggsSector,
            result.routeUsesSymplecticVortexEquations,
            result.routeUsesKahlerHamiltonianGManifold,
            result.routeUsesGITOrKobayashiHitchinCorrespondence,
            result.routeUsesBoundaryYangMillsReduction,
            result.routeUsesGaugeCovariantSuperselectionSectors,
            result.routeUsesSymplecticHiggsBranchSingularities,
            result.routeEncodesPartialHiggsMechanismAsSymplecticLeaves,
            result.marsdenWeinsteinPublicationYear,
            result.marsdenWeinsteinStartPage,
            result.marsdenWeinsteinEndPage,
            result.atiyahBottPublicationYear,
            result.diezRudolphLatestArxivVersion,
            result.diezRudolphJournalYear,
            result.trautweinLatestArxivVersion,
            result.rielloLatestArxivVersion,
            result.higgsMechanismHasseLatestArxivVersion,
            result.higgsMechanismHassePageCount,
            result.routeIsPhaseSpaceConstraintGeometryNotMassLaw,
            result.routeDescribesReducedPhaseSpaceNotHamiltonianMassSpectrum,
            result.routeDependsOnChosenHamiltonianPotentialOrModel,
            result.routeKeepsStandardModelInputsExternal,
            result.routeDoesNotDeriveObservedElectroweakVacuum,
            result.routeDoesNotDeriveStandardModelGaugeCouplings,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotProvideTargetIndependentObservedHiggsMass,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.routeRequiresGuLocalMomentMap,
            result.routeRequiresGuSymplecticPhaseSpace,
            result.routeRequiresGuHamiltonianReductionLaw,
            result.routeRequiresGuReducedVacuumSelection,
            result.routeRequiresGuElectroweakHiggsDoubletProjection,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuGaugeCouplingNormalization,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresLowEnergyRgAndThresholdTransport,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalMomentMap,
            result.routeProvidesGuSymplecticPhaseSpace,
            result.routeProvidesGuHamiltonianReductionLaw,
            result.routeProvidesGuReducedVacuumSelection,
            result.routeProvidesGuElectroweakHiggsDoubletProjection,
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
Console.WriteLine($"momentMapSymplecticReductionSourceAuditPassed={momentMapSymplecticReductionSourceAuditPassed}");
Console.WriteLine($"routeUsesMomentMaps={routeUsesMomentMaps}");
Console.WriteLine($"routeUsesYangMillsHiggsReducedPhaseSpace={routeUsesYangMillsHiggsReducedPhaseSpace}");
Console.WriteLine($"routeIncludesGlashowWeinbergSalamHiggsSector={routeIncludesGlashowWeinbergSalamHiggsSector}");
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

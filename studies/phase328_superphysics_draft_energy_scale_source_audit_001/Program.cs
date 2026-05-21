using System.Text.Json;

const string DefaultOutputDir = "studies/phase328_superphysics_draft_energy_scale_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE328_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));

const string officialGuSiteUrl = "https://geometricunity.org/";
const string officialDraftPdfUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";
const string superphysicsIndexUrl = "https://www.superphysics.org/research/weinstein/unity/";
const string superphysicsPart2bUrl = "https://www.superphysics.org/research/weinstein/unity/part-02b/";
const string superphysicsPart4Url = "https://www.superphysics.org/research/weinstein/unity/part-04/";
const string superphysicsPart11Url = "https://www.superphysics.org/research/weinstein/unity/part-12b/";
const string superphysicsPart12Url = "https://www.superphysics.org/research/weinstein/unity/part-12/";
const string superphysicsPart12dUrl = "https://www.superphysics.org/research/weinstein/unity/part-12d/";

const bool officialSiteIdentifiesApril2021Draft = true;
const bool superphysicsReadableDraftMirrorPresent = true;
const bool superphysicsMirrorTreatedAsSearchAidNotPrimaryPromotionSource = true;
const bool superphysicsPart2bFramesHiggsAsGeometricallyUnmotivated = true;
const bool superphysicsPart4PlacesStandardModelGroupInReductionIntersection = true;
const bool superphysicsPart11DiracSquareRootProgramPresent = true;
const bool superphysicsPart12YangMillsHiggsSecondOrderProgramPresent = true;
const bool superphysicsPart12dInternalQuantumNumbersExplicit = true;
const bool superphysicsPart12dEnergyScaleHelpStillNeeded = true;
const bool superphysicsPart12dLocationsWithinGuAppendixPresent = true;

const bool mirrorProvidesTargetIndependentWzEnergyScale = false;
const bool mirrorProvidesSeparateWzSourceRows = false;
const bool mirrorProvidesWzRawAmplitudeGates = false;
const bool mirrorProvidesWzCommonBridgeGate = false;
const bool mirrorProvidesWeakMixingAngleSource = false;
const bool mirrorProvidesLowEnergyGaugeCouplingNormalization = false;
const bool mirrorProvidesTargetIndependentVevSource = false;
const bool mirrorProvidesObservedPhotonWzProjectionRows = false;
const bool mirrorProvidesGaugeFixedMassMatrixDiagonalization = false;
const bool mirrorProvidesHiggsScalarSourceOperator = false;
const bool mirrorProvidesHiggsMassiveScalarProfile = false;
const bool mirrorProvidesHiggsQuarticOrExcitationSource = false;
const bool mirrorProvidesGeVUnitNormalization = false;
const bool mirrorPromotesWzMasses = false;
const bool mirrorPromotesHiggsMass = false;
const bool mirrorCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var officialPublicSourceAuditMaterialized = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true;
var officialDraftProvidesDirectWzLaw = JsonBool(phase218.RootElement, "officialDraftProvidesDirectWzLaw") is true;
var officialDraftProvidesSolvedHiggsSource = JsonBool(phase218.RootElement, "officialDraftProvidesSolvedHiggsSource") is true;
var officialDraftProvidesCompletionSource = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesWeakIsospinLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true;
var officialDraftProvidesWeakHyperchargeLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;
var officialDraftProjectionMapPromotesWzMasses = JsonBool(phase313.RootElement, "officialDraftProjectionMapPromotesWzMasses") is true;
var officialDraftProjectionMapPromotesHiggsMass = JsonBool(phase313.RootElement, "officialDraftProjectionMapPromotesHiggsMass") is true;
var officialDraftProjectionMapCompletesObservedFieldExtraction = JsonBool(phase313.RootElement, "officialDraftProjectionMapCompletesObservedFieldExtraction") is true;

var higgsUpsilonScalarSourceBoundaryAuditPassed = JsonBool(phase322.RootElement, "higgsUpsilonScalarSourceBoundaryAuditPassed") is true;
var officialGuSourcesProvideFixedScalarSourceOperator = JsonBool(phase322.RootElement, "officialGuSourcesProvideFixedScalarSourceOperator") is true;
var officialGuSourcesProvideMassiveScalarProfile = JsonBool(phase322.RootElement, "officialGuSourcesProvideMassiveScalarProfile") is true;
var officialGuSourcesProvideQuarticSelfCouplingValue = JsonBool(phase322.RootElement, "officialGuSourcesProvideQuarticSelfCouplingValue") is true;
var officialGuSourcesPromoteHiggsMass = JsonBool(phase322.RootElement, "officialGuSourcesPromoteHiggsMass") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialDraftProvidesHiggsFieldLocation = JsonBool(phase323.RootElement, "officialDraftAppendixLocatesHiggsField") is true;
var officialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var officialPublicSourcesProvideGeVUnitNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGeVUnitNormalization") is true;
var officialPublicSourcesProvideHiggsScalarSelfCouplingSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHiggsScalarSelfCouplingSource") is true;
var officialPublicSourcesProvideCompleteMassEigenstateExtraction = JsonBool(phase323.RootElement, "officialPublicSourcesProvideCompleteMassEigenstateExtraction") is true;

var sourceRows = new[]
{
    new SourceRow(
        "official-gu-site-draft-release",
        officialGuSiteUrl,
        "lines 25-28",
        "The official site identifies the April 1, 2021 Geometric Unity manuscript draft.",
        "Primary release context only; it does not itself fill W/Z or Higgs source-lineage fields."),
    new SourceRow(
        "official-gu-draft-pdf",
        officialDraftPdfUrl,
        "April 1, 2021 public draft",
        "Primary draft artifact already audited by Phase218, Phase313, Phase322, and Phase323.",
        "Primary draft remains binding; prior official-draft gates report no W/Z direct law or solved Higgs source."),
    new SourceRow(
        "superphysics-readable-draft-index",
        superphysicsIndexUrl,
        "readable public transcription index",
        "The mirror exposes a navigable text rendering of the public GU draft parts.",
        "Useful for searching the draft, but not stronger than the primary PDF for promotion."),
    new SourceRow(
        "superphysics-part-2b-higgs-context",
        superphysicsPart2bUrl,
        "2.3 Higgs Sector Remains Geometrically Unmotivated",
        "The readable draft frames the spin-0 Higgs sector and quartic potential as geometrically unmotivated.",
        "This is motivation for a scalar-source program, not a solved Higgs scalar-source or self-coupling row."),
    new SourceRow(
        "superphysics-part-4-observed-quantum-numbers",
        superphysicsPart4Url,
        "4.1 maximal compact and complex subgroup reductions",
        "The readable draft records Standard Model group/representation structure from reductions.",
        "This supports quantum-number/representation leads, not W/Z/H energy scales."),
    new SourceRow(
        "superphysics-part-11-dirac-square-root",
        superphysicsPart11Url,
        "12.5 Dirac square-root unification",
        "The readable draft places Yang-Mills and Klein-Gordon/Higgs equations in a second-order stratum related to first-order structure.",
        "This is a programmatic equation-family relation, not mass-eigenstate extraction or GeV normalization."),
    new SourceRow(
        "superphysics-part-12-equations",
        superphysicsPart12Url,
        "12.1 equations",
        "The readable draft presents first-order and second-order GU equations involving Upsilon and Yang-Mills-Higgs structure.",
        "No W/Z particle rows, Higgs scalar self-coupling value, weak angle source, or physical target gate is supplied."),
    new SourceRow(
        "superphysics-part-12d-energy-scale-boundary",
        superphysicsPart12dUrl,
        "Appendix: Thoughts on Method",
        "The readable draft distinguishes internal quantum-number predictions from the further work needed to sharpen them into energy thresholds.",
        "This directly blocks using the mirror as a W/Z/H mass-energy source."),
};

var checks = new[]
{
    new Check(
        "readable-draft-route-reviewed",
        officialSiteIdentifiesApril2021Draft
            && superphysicsReadableDraftMirrorPresent
            && superphysicsMirrorTreatedAsSearchAidNotPrimaryPromotionSource
            && sourceRows.Length == 8,
        $"officialSiteDraft={officialSiteIdentifiesApril2021Draft}; mirrorPresent={superphysicsReadableDraftMirrorPresent}; searchAidOnly={superphysicsMirrorTreatedAsSearchAidNotPrimaryPromotionSource}; sourceRowCount={sourceRows.Length}"),
    new Check(
        "mirror-preserves-programmatic-gu-leads",
        superphysicsPart2bFramesHiggsAsGeometricallyUnmotivated
            && superphysicsPart4PlacesStandardModelGroupInReductionIntersection
            && superphysicsPart11DiracSquareRootProgramPresent
            && superphysicsPart12YangMillsHiggsSecondOrderProgramPresent
            && superphysicsPart12dLocationsWithinGuAppendixPresent,
        $"higgsMotivation={superphysicsPart2bFramesHiggsAsGeometricallyUnmotivated}; smReduction={superphysicsPart4PlacesStandardModelGroupInReductionIntersection}; diracSquareRoot={superphysicsPart11DiracSquareRootProgramPresent}; ymHiggsProgram={superphysicsPart12YangMillsHiggsSecondOrderProgramPresent}; locationsAppendix={superphysicsPart12dLocationsWithinGuAppendixPresent}"),
    new Check(
        "mirror-energy-scale-boundary",
        superphysicsPart12dInternalQuantumNumbersExplicit
            && superphysicsPart12dEnergyScaleHelpStillNeeded
            && !mirrorProvidesTargetIndependentWzEnergyScale
            && !mirrorProvidesGeVUnitNormalization
            && !mirrorPromotesWzMasses
            && !mirrorPromotesHiggsMass,
        $"internalQuantumNumbers={superphysicsPart12dInternalQuantumNumbersExplicit}; energyScaleHelpNeeded={superphysicsPart12dEnergyScaleHelpStillNeeded}; mirrorWzEnergyScale={mirrorProvidesTargetIndependentWzEnergyScale}; mirrorGeV={mirrorProvidesGeVUnitNormalization}; promotesWz={mirrorPromotesWzMasses}; promotesHiggs={mirrorPromotesHiggsMass}"),
    new Check(
        "official-draft-prior-audits-remain-binding",
        officialPublicSourceAuditMaterialized
            && !officialDraftProvidesDirectWzLaw
            && !officialDraftProvidesSolvedHiggsSource
            && !officialDraftProvidesCompletionSource
            && officialDraftElectroweakProjectionMapAuditPassed
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && officialDraftProvidesHiggsFieldLocation
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding
            && !officialDraftProjectionMapPromotesWzMasses
            && !officialDraftProjectionMapPromotesHiggsMass
            && !officialDraftProjectionMapCompletesObservedFieldExtraction,
        $"p218Materialized={officialPublicSourceAuditMaterialized}; directWz={officialDraftProvidesDirectWzLaw}; solvedHiggs={officialDraftProvidesSolvedHiggsSource}; p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; weakIsospin={officialDraftProvidesWeakIsospinLocation}; weakHypercharge={officialDraftProvidesWeakHyperchargeLocation}; higgsLocation={officialDraftProvidesHiggsFieldLocation}; photonZ={officialDraftProvidesPhotonZWeinbergRotation}; weakAngle={officialDraftProvidesWeakMixingAngleSource}; observedEmbedding={officialDraftProvidesObservedElectroweakGaugeEmbedding}"),
    new Check(
        "official-higgs-and-mass-extraction-audits-remain-binding",
        higgsUpsilonScalarSourceBoundaryAuditPassed
            && !officialGuSourcesProvideFixedScalarSourceOperator
            && !officialGuSourcesProvideMassiveScalarProfile
            && !officialGuSourcesProvideQuarticSelfCouplingValue
            && !officialGuSourcesPromoteHiggsMass
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideGeVUnitNormalization
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !officialPublicSourcesProvideCompleteMassEigenstateExtraction,
        $"p322Passed={higgsUpsilonScalarSourceBoundaryAuditPassed}; scalarOperator={officialGuSourcesProvideFixedScalarSourceOperator}; massiveProfile={officialGuSourcesProvideMassiveScalarProfile}; quartic={officialGuSourcesProvideQuarticSelfCouplingValue}; p323Passed={coupledYangMillsHiggsMassExtractionAuditPassed}; quadraticExpansion={officialPublicSourcesProvideGaugeFixedQuadraticExpansion}; projectionRows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; gev={officialPublicSourcesProvideGeVUnitNormalization}; completeExtraction={officialPublicSourcesProvideCompleteMassEigenstateExtraction}"),
    new Check(
        "mirror-does-not-fill-source-lineage-or-observed-extraction-contracts",
        !mirrorProvidesSeparateWzSourceRows
            && !mirrorProvidesWzRawAmplitudeGates
            && !mirrorProvidesWzCommonBridgeGate
            && !mirrorProvidesWeakMixingAngleSource
            && !mirrorProvidesLowEnergyGaugeCouplingNormalization
            && !mirrorProvidesTargetIndependentVevSource
            && !mirrorProvidesObservedPhotonWzProjectionRows
            && !mirrorProvidesGaugeFixedMassMatrixDiagonalization
            && !mirrorProvidesHiggsScalarSourceOperator
            && !mirrorProvidesHiggsMassiveScalarProfile
            && !mirrorProvidesHiggsQuarticOrExcitationSource
            && !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"separateWzRows={mirrorProvidesSeparateWzSourceRows}; rawGates={mirrorProvidesWzRawAmplitudeGates}; commonBridge={mirrorProvidesWzCommonBridgeGate}; weakAngle={mirrorProvidesWeakMixingAngleSource}; gaugeCoupling={mirrorProvidesLowEnergyGaugeCouplingNormalization}; vev={mirrorProvidesTargetIndependentVevSource}; observedRows={mirrorProvidesObservedPhotonWzProjectionRows}; massMatrix={mirrorProvidesGaugeFixedMassMatrixDiagonalization}; higgsOperator={mirrorProvidesHiggsScalarSourceOperator}; higgsProfile={mirrorProvidesHiggsMassiveScalarProfile}; higgsCoupling={mirrorProvidesHiggsQuarticOrExcitationSource}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}"),
};

var superphysicsDraftEnergyScaleSourceAuditPassed = checks.All(check => check.Passed)
    && !mirrorProvidesTargetIndependentWzEnergyScale
    && !mirrorProvidesSeparateWzSourceRows
    && !mirrorProvidesWzRawAmplitudeGates
    && !mirrorProvidesWzCommonBridgeGate
    && !mirrorProvidesWeakMixingAngleSource
    && !mirrorProvidesLowEnergyGaugeCouplingNormalization
    && !mirrorProvidesTargetIndependentVevSource
    && !mirrorProvidesObservedPhotonWzProjectionRows
    && !mirrorProvidesGaugeFixedMassMatrixDiagonalization
    && !mirrorProvidesHiggsScalarSourceOperator
    && !mirrorProvidesHiggsMassiveScalarProfile
    && !mirrorProvidesHiggsQuarticOrExcitationSource
    && !mirrorProvidesGeVUnitNormalization
    && !mirrorPromotesWzMasses
    && !mirrorPromotesHiggsMass
    && !mirrorCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = superphysicsDraftEnergyScaleSourceAuditPassed
    ? "superphysics-draft-energy-scale-source-audit-quantum-numbers-not-mass-source"
    : "superphysics-draft-energy-scale-source-audit-review-required";

var result = new
{
    phaseId = "phase328-superphysics-draft-energy-scale-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    superphysicsDraftEnergyScaleSourceAuditPassed,
    officialSiteIdentifiesApril2021Draft,
    superphysicsReadableDraftMirrorPresent,
    superphysicsMirrorTreatedAsSearchAidNotPrimaryPromotionSource,
    superphysicsPart2bFramesHiggsAsGeometricallyUnmotivated,
    superphysicsPart4PlacesStandardModelGroupInReductionIntersection,
    superphysicsPart11DiracSquareRootProgramPresent,
    superphysicsPart12YangMillsHiggsSecondOrderProgramPresent,
    superphysicsPart12dInternalQuantumNumbersExplicit,
    superphysicsPart12dEnergyScaleHelpStillNeeded,
    superphysicsPart12dLocationsWithinGuAppendixPresent,
    mirrorProvidesTargetIndependentWzEnergyScale,
    mirrorProvidesSeparateWzSourceRows,
    mirrorProvidesWzRawAmplitudeGates,
    mirrorProvidesWzCommonBridgeGate,
    mirrorProvidesWeakMixingAngleSource,
    mirrorProvidesLowEnergyGaugeCouplingNormalization,
    mirrorProvidesTargetIndependentVevSource,
    mirrorProvidesObservedPhotonWzProjectionRows,
    mirrorProvidesGaugeFixedMassMatrixDiagonalization,
    mirrorProvidesHiggsScalarSourceOperator,
    mirrorProvidesHiggsMassiveScalarProfile,
    mirrorProvidesHiggsQuarticOrExcitationSource,
    mirrorProvidesGeVUnitNormalization,
    mirrorPromotesWzMasses,
    mirrorPromotesHiggsMass,
    mirrorCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    upstreamOfficialDraftBoundary = new
    {
        officialPublicSourceAuditMaterialized,
        officialDraftProvidesDirectWzLaw,
        officialDraftProvidesSolvedHiggsSource,
        officialDraftProvidesCompletionSource,
        officialDraftElectroweakProjectionMapAuditPassed,
        officialDraftProvidesWeakIsospinLocation,
        officialDraftProvidesWeakHyperchargeLocation,
        officialDraftProvidesHiggsFieldLocation,
        officialDraftProvidesPhotonZWeinbergRotation,
        officialDraftProvidesWeakMixingAngleSource,
        officialDraftProvidesObservedElectroweakGaugeEmbedding,
        officialDraftProjectionMapPromotesWzMasses,
        officialDraftProjectionMapPromotesHiggsMass,
        officialDraftProjectionMapCompletesObservedFieldExtraction,
        higgsUpsilonScalarSourceBoundaryAuditPassed,
        officialGuSourcesProvideFixedScalarSourceOperator,
        officialGuSourcesProvideMassiveScalarProfile,
        officialGuSourcesProvideQuarticSelfCouplingValue,
        officialGuSourcesPromoteHiggsMass,
        coupledYangMillsHiggsMassExtractionAuditPassed,
        officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
        officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
        officialPublicSourcesProvideGeVUnitNormalization,
        officialPublicSourcesProvideHiggsScalarSelfCouplingSource,
        officialPublicSourcesProvideCompleteMassEigenstateExtraction,
    },
    contractImpact = new
    {
        phase201AllRequiredLineagesPromotable,
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
    },
    decision = superphysicsDraftEnergyScaleSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from the Superphysics-readable GU draft path. It is useful searchable context for official-draft claims and explicitly preserves internal quantum-number/structure guidance, but it does not supply W/Z energy scales, separate source rows, replay gates, weak-angle or VEV source lineage, observed-field extraction, Higgs scalar-source/self-coupling lineage, or GeV normalization."
        : "Review the readable draft source path before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A primary-source, derivation-backed, target-independent W/Z absolute source lineage with separate W and Z source rows and all P209 gates true.",
        "A solved, target-independent Higgs scalar-source/operator lineage with identity envelope, massive profile, coupling or excitation source, stability sidecars, and a passing prediction row.",
    },
    sourceEvidence = new
    {
        officialGuSiteUrl,
        officialDraftPdfUrl,
        superphysicsIndexUrl,
        superphysicsPart2bUrl,
        superphysicsPart4Url,
        superphysicsPart11Url,
        superphysicsPart12Url,
        superphysicsPart12dUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase218Path = Phase218Path,
        phase256Path = Phase256Path,
        phase313Path = Phase313Path,
        phase322Path = Phase322Path,
        phase323Path = Phase323Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "superphysics_draft_energy_scale_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "superphysics_draft_energy_scale_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.superphysicsDraftEnergyScaleSourceAuditPassed,
        result.officialSiteIdentifiesApril2021Draft,
        result.superphysicsReadableDraftMirrorPresent,
        result.superphysicsMirrorTreatedAsSearchAidNotPrimaryPromotionSource,
        result.superphysicsPart2bFramesHiggsAsGeometricallyUnmotivated,
        result.superphysicsPart4PlacesStandardModelGroupInReductionIntersection,
        result.superphysicsPart11DiracSquareRootProgramPresent,
        result.superphysicsPart12YangMillsHiggsSecondOrderProgramPresent,
        result.superphysicsPart12dInternalQuantumNumbersExplicit,
        result.superphysicsPart12dEnergyScaleHelpStillNeeded,
        result.mirrorProvidesTargetIndependentWzEnergyScale,
        result.mirrorProvidesSeparateWzSourceRows,
        result.mirrorProvidesWeakMixingAngleSource,
        result.mirrorProvidesTargetIndependentVevSource,
        result.mirrorProvidesObservedPhotonWzProjectionRows,
        result.mirrorProvidesHiggsScalarSourceOperator,
        result.mirrorProvidesHiggsQuarticOrExcitationSource,
        result.mirrorProvidesGeVUnitNormalization,
        result.mirrorPromotesWzMasses,
        result.mirrorPromotesHiggsMass,
        result.mirrorCompletesBosonPredictions,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.sourceRows,
        result.checks,
        result.contractImpact,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"superphysicsDraftEnergyScaleSourceAuditPassed={superphysicsDraftEnergyScaleSourceAuditPassed}");
Console.WriteLine($"mirrorPromotesWzMasses={mirrorPromotesWzMasses}");
Console.WriteLine($"mirrorPromotesHiggsMass={mirrorPromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);

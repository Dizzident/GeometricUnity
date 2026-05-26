using System.Text.Json;

const string DefaultOutputDir = "studies/phase367_theta_omega_source_equation_availability_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase315Path = "studies/phase315_ucsd_dark_geometric_energy_source_audit_001/output/ucsd_dark_geometric_energy_source_audit_summary.json";
const string Phase316Path = "studies/phase316_ucsd_transcript_source_strength_audit_001/output/ucsd_transcript_source_strength_audit_summary.json";
const string Phase329Path = "studies/phase329_seiberg_witten_monopole_electroweak_source_audit_001/output/seiberg_witten_monopole_electroweak_source_audit_summary.json";
const string Phase331Path = "studies/phase331_theta_omega_inhomogeneous_gauge_source_audit_001/output/theta_omega_inhomogeneous_gauge_source_audit_summary.json";
const string Phase365Path = "studies/phase365_dressing_field_electroweak_observed_variables_audit_001/output/dressing_field_electroweak_observed_variables_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE367_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase315 = JsonDocument.Parse(File.ReadAllText(Phase315Path));
using var phase316 = JsonDocument.Parse(File.ReadAllText(Phase316Path));
using var phase329 = JsonDocument.Parse(File.ReadAllText(Phase329Path));
using var phase331 = JsonDocument.Parse(File.ReadAllText(Phase331Path));
using var phase365 = JsonDocument.Parse(File.ReadAllText(Phase365Path));

const string portalGroupThetaOmegaAbstractUrl = "https://theportal.group/from-dark-to-geometric-energy-a-sector-of-geometric-unity/";
const string portalWikiThetaOmegaPageUrl = "https://theportal.wiki/wiki/From_Dark_to_Geometric_Energy_-_A_Sector_of_Geometric_Unity_%28YouTube_Content%29";
const string officialOxfordLectureUrl = "https://geometricunity.org/2013-oxford-lecture/";
const string officialDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";

const bool currentPublicSearchPerformed = true;
const string currentPublicSearchDate = "2026-05-26";
const bool currentPublicSearchFoundNewPrimaryEquationSource = false;
const bool currentPublicSearchFoundNewPrimaryTranscript = false;
const bool currentPublicSearchFoundNewPrimarySlideDeck = false;
const bool currentPublicSearchFoundOnlyExistingPortalAbstractAndWikiState = true;
const bool thirdPartyPointersFoundButNotUsedAsSourceEvidence = true;

const bool officialPortalAbstractAvailable = true;
const bool officialPortalAbstractMentionsThetaOmega = true;
const bool officialPortalAbstractMentionsInhomogeneousGauge = true;
const bool officialPortalAbstractMentionsDiracSpinorBundle = true;
const bool officialPortalAbstractMentionsFourteenDimensionalLorentzianMetrics = true;
const bool officialPortalAbstractMentionsSupersymmetricEinsteinDirac = true;
const bool officialPortalAbstractMentionsPatiSalamGenerations = true;
const bool officialPortalAbstractMentionsSeibergWittenMonopoleEquations = true;
const bool officialPortalAbstractProvidesEquationRows = false;
const bool officialPortalAbstractProvidesWzMassLaw = false;
const bool officialPortalAbstractProvidesHiggsMassLaw = false;

const bool portalWikiPageAvailable = true;
const bool portalWikiEditedTranscriptAvailable = false;
const bool portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess = true;
const bool publicCaptionOrTranscriptUsableAsSourceLineage = false;
const bool publicUcsdSlidesFound = false;
const bool publicFullTranscriptFound = false;
const bool publicEquationSourceFound = false;

const bool officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable = true;
const bool officialOxfordTranscriptGivesInhomogeneousGaugeGroupDefinition = true;
const bool officialOxfordTranscriptGivesBiConnectionAndTiltedMapContext = true;
const bool officialOxfordTranscriptGivesRepresentationTheoryLead = true;
const bool officialOxfordTranscriptProvidesThetaOmegaEquation = false;
const bool officialOxfordTranscriptProvidesDirectWzMassLaw = false;
const bool officialOxfordTranscriptProvidesHiggsMassLaw = false;
const bool officialOxfordTranscriptProvidesObservedPhotonWzHiggsProjectionRows = false;

const bool currentAvailableThetaOmegaMaterialProvidesEquationAvailabilityOnly = false;
const bool currentAvailableThetaOmegaMaterialIsAbstractLevelForUcsdUpdate = true;
const bool routeProvidesDirectWzBridgeSourceLaw = false;
const bool routeProvidesGuLocalWzTheorem = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesTargetIndependentVevSource = false;
const bool routeProvidesWeakAngleOrCouplingNormalization = false;
const bool routeProvidesWzRawAmplitudeGates = false;
const bool routeProvidesWzCommonBridgeGate = false;
const bool routeProvidesWzStabilitySidecars = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesHiggsQuarticOrExcitationSource = false;
const bool routeProvidesHiggsMassiveScalarProfile = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var ucsdDarkGeometricEnergySourceAuditPassed = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergySourceAuditPassed") is true;
var ucsdDarkGeometricEnergyMentionsThetaOmega = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyMentionsThetaOmega") is true;
var ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup") is true;
var ucsdDarkGeometricEnergyEditedTranscriptAvailable = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyEditedTranscriptAvailable") is true;
var ucsdDarkGeometricEnergyPromotesWzMasses = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesWzMasses") is true;
var ucsdDarkGeometricEnergyPromotesHiggsMass = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesHiggsMass") is true;

var ucsdTranscriptSourceStrengthAuditPassed = JsonBool(phase316.RootElement, "ucsdTranscriptSourceStrengthAuditPassed") is true;
var phase316PortalWikiEditedTranscriptAvailable = JsonBool(phase316.RootElement, "portalWikiEditedTranscriptAvailable") is true;
var phase316CaptionOrTranscriptUsableAsSourceLineage = JsonBool(phase316.RootElement, "captionOrTranscriptUsableAsSourceLineage") is true;
var phase316PublicSearchExactVideoTranscriptFound = JsonBool(phase316.RootElement, "publicSearchExactVideoTranscriptFound") is true;
var phase316TranscriptAuditPromotesWzMasses = JsonBool(phase316.RootElement, "transcriptAuditPromotesWzMasses") is true;
var phase316TranscriptAuditPromotesHiggsMass = JsonBool(phase316.RootElement, "transcriptAuditPromotesHiggsMass") is true;

var seibergWittenMonopoleElectroweakSourceAuditPassed = JsonBool(phase329.RootElement, "seibergWittenMonopoleElectroweakSourceAuditPassed") is true;
var seibergWittenPromotesWzMasses = JsonBool(phase329.RootElement, "seibergWittenPromotesWzMasses") is true;
var seibergWittenPromotesHiggsMass = JsonBool(phase329.RootElement, "seibergWittenPromotesHiggsMass") is true;

var thetaOmegaInhomogeneousGaugeSourceAuditPassed = JsonBool(phase331.RootElement, "thetaOmegaInhomogeneousGaugeSourceAuditPassed") is true;
var phase331ThetaOmegaRouteGivesResearchLeadForSourceLaw = JsonBool(phase331.RootElement, "thetaOmegaRouteGivesResearchLeadForSourceLaw") is true;
var phase331ThetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw") is true;
var phase331ThetaOmegaRouteProvidesSeparateWzSourceRows = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesSeparateWzSourceRows") is true;
var phase331ThetaOmegaRouteProvidesTargetIndependentVevSource = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesTargetIndependentVevSource") is true;
var phase331ThetaOmegaRouteProvidesWeakMixingAngleSource = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesWeakMixingAngleSource") is true;
var phase331ThetaOmegaRouteProvidesHiggsScalarSourceOperator = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesHiggsScalarSourceOperator") is true;
var phase331ThetaOmegaRoutePromotesWzMasses = JsonBool(phase331.RootElement, "thetaOmegaRoutePromotesWzMasses") is true;
var phase331ThetaOmegaRoutePromotesHiggsMass = JsonBool(phase331.RootElement, "thetaOmegaRoutePromotesHiggsMass") is true;

var dressingFieldElectroweakObservedVariablesAuditPassed = JsonBool(phase365.RootElement, "dressingFieldElectroweakObservedVariablesAuditPassed") is true;
var dressingFieldRoutePromotesWzMasses = JsonBool(phase365.RootElement, "routePromotesWzMasses") is true;
var dressingFieldRoutePromotesHiggsMass = JsonBool(phase365.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "portal-group-ucsd-theta-omega-abstract-current-check",
        portalGroupThetaOmegaAbstractUrl,
        "Official Portal Group page, checked 2026-05-26",
        "The public abstract remains the current official UCSD theta_omega lead and mentions an inhomogeneous gauge group over the Dirac spinor bundle of a 14-dimensional Lorentzian-metric space.",
        "Strong research lead, but the page exposes no equations, source rows, W/Z bridge law, Higgs mass law, or observed-field extraction."),
    new SourceRow(
        "portal-wiki-ucsd-transcript-state-current-check",
        portalWikiThetaOmegaPageUrl,
        "Portal Wiki page, checked 2026-05-26",
        "The wiki page records metadata and still states that the content does not have an edited transcript; machine-generated transcript access is private.",
        "Confirms that no public transcript/caption artifact is currently available for source-lineage use."),
    new SourceRow(
        "official-oxford-lecture-inhomogeneous-gauge-equation-context",
        officialOxfordLectureUrl,
        "2013 Oxford transcript, inhomogeneous gauge group and tilted-gauge sections",
        "The transcript supplies public inhomogeneous-gauge-group, bi-connection, tilted-map, and representation-theory context.",
        "Useful GU-native equation context, but it does not supply a theta_omega equation or W/Z/H source-lineage rows."),
    new SourceRow(
        "official-gu-draft-context",
        officialDraftUrl,
        "2021 GU working draft, already audited by Phase331",
        "The draft remains the primary GU context source for electroweak/Higgs notation and field-location checks.",
        "The draft has not supplied separate W/Z source rows, a weak-angle/VEV source, observed projection rows, Higgs scalar-source lineage, or GeV normalization."),
    new SourceRow(
        "public-search-theta-omega-equation-refresh",
        "search: theta_omega Geometric Unity equations; From Dark to Geometric Energy transcript; site:geometricunity.org theta_omega",
        "Current public search, 2026-05-26",
        "Search refreshed the availability state and found the existing Portal abstract/wiki state rather than a new primary equation, transcript, or slide artifact.",
        "Search result is a provenance record only; it is not source-lineage evidence.")
};

var checks = new[]
{
    new Check(
        "current-public-source-availability-refresh-recorded",
        currentPublicSearchPerformed
            && currentPublicSearchFoundOnlyExistingPortalAbstractAndWikiState
            && !currentPublicSearchFoundNewPrimaryEquationSource
            && !currentPublicSearchFoundNewPrimaryTranscript
            && !currentPublicSearchFoundNewPrimarySlideDeck
            && thirdPartyPointersFoundButNotUsedAsSourceEvidence
            && sourceRows.Length == 5,
        $"searchDate={currentPublicSearchDate}; existingPortalState={currentPublicSearchFoundOnlyExistingPortalAbstractAndWikiState}; newPrimaryEquation={currentPublicSearchFoundNewPrimaryEquationSource}; newTranscript={currentPublicSearchFoundNewPrimaryTranscript}; newSlides={currentPublicSearchFoundNewPrimarySlideDeck}; thirdPartyPointersNotEvidence={thirdPartyPointersFoundButNotUsedAsSourceEvidence}; sourceRows={sourceRows.Length}"),
    new Check(
        "portal-theta-omega-abstract-is-available-but-non-equational",
        officialPortalAbstractAvailable
            && officialPortalAbstractMentionsThetaOmega
            && officialPortalAbstractMentionsInhomogeneousGauge
            && officialPortalAbstractMentionsDiracSpinorBundle
            && officialPortalAbstractMentionsFourteenDimensionalLorentzianMetrics
            && officialPortalAbstractMentionsSupersymmetricEinsteinDirac
            && officialPortalAbstractMentionsPatiSalamGenerations
            && officialPortalAbstractMentionsSeibergWittenMonopoleEquations
            && !officialPortalAbstractProvidesEquationRows
            && !officialPortalAbstractProvidesWzMassLaw
            && !officialPortalAbstractProvidesHiggsMassLaw,
        $"portalAbstract={officialPortalAbstractAvailable}; thetaOmega={officialPortalAbstractMentionsThetaOmega}; inhomogeneousGauge={officialPortalAbstractMentionsInhomogeneousGauge}; diracSpinorBundle={officialPortalAbstractMentionsDiracSpinorBundle}; metrics14D={officialPortalAbstractMentionsFourteenDimensionalLorentzianMetrics}; equationRows={officialPortalAbstractProvidesEquationRows}; wzLaw={officialPortalAbstractProvidesWzMassLaw}; higgsLaw={officialPortalAbstractProvidesHiggsMassLaw}"),
    new Check(
        "public-transcript-and-slide-artifacts-remain-unavailable",
        portalWikiPageAvailable
            && !portalWikiEditedTranscriptAvailable
            && portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess
            && !publicCaptionOrTranscriptUsableAsSourceLineage
            && !publicFullTranscriptFound
            && !publicUcsdSlidesFound
            && !publicEquationSourceFound
            && ucsdTranscriptSourceStrengthAuditPassed
            && !phase316PortalWikiEditedTranscriptAvailable
            && !phase316CaptionOrTranscriptUsableAsSourceLineage
            && !phase316PublicSearchExactVideoTranscriptFound,
        $"wikiPage={portalWikiPageAvailable}; editedTranscript={portalWikiEditedTranscriptAvailable}; privateMachineTranscript={portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess}; publicTranscript={publicFullTranscriptFound}; publicSlides={publicUcsdSlidesFound}; publicEquations={publicEquationSourceFound}; p316Passed={ucsdTranscriptSourceStrengthAuditPassed}; p316UsableTranscript={phase316CaptionOrTranscriptUsableAsSourceLineage}"),
    new Check(
        "oxford-inhomogeneous-gauge-equations-do-not-fill-theta-omega-source-law",
        officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable
            && officialOxfordTranscriptGivesInhomogeneousGaugeGroupDefinition
            && officialOxfordTranscriptGivesBiConnectionAndTiltedMapContext
            && officialOxfordTranscriptGivesRepresentationTheoryLead
            && !officialOxfordTranscriptProvidesThetaOmegaEquation
            && !officialOxfordTranscriptProvidesDirectWzMassLaw
            && !officialOxfordTranscriptProvidesHiggsMassLaw
            && !officialOxfordTranscriptProvidesObservedPhotonWzHiggsProjectionRows,
        $"oxfordEquations={officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable}; inhomogeneousGaugeDefinition={officialOxfordTranscriptGivesInhomogeneousGaugeGroupDefinition}; biConnection={officialOxfordTranscriptGivesBiConnectionAndTiltedMapContext}; thetaOmegaEquation={officialOxfordTranscriptProvidesThetaOmegaEquation}; wzLaw={officialOxfordTranscriptProvidesDirectWzMassLaw}; higgsLaw={officialOxfordTranscriptProvidesHiggsMassLaw}; observedRows={officialOxfordTranscriptProvidesObservedPhotonWzHiggsProjectionRows}"),
    new Check(
        "prior-theta-omega-and-transcript-boundaries-remain-binding",
        ucsdDarkGeometricEnergySourceAuditPassed
            && ucsdDarkGeometricEnergyMentionsThetaOmega
            && ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup
            && !ucsdDarkGeometricEnergyEditedTranscriptAvailable
            && !ucsdDarkGeometricEnergyPromotesWzMasses
            && !ucsdDarkGeometricEnergyPromotesHiggsMass
            && thetaOmegaInhomogeneousGaugeSourceAuditPassed
            && phase331ThetaOmegaRouteGivesResearchLeadForSourceLaw
            && !phase331ThetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw
            && !phase331ThetaOmegaRouteProvidesSeparateWzSourceRows
            && !phase331ThetaOmegaRouteProvidesTargetIndependentVevSource
            && !phase331ThetaOmegaRouteProvidesWeakMixingAngleSource
            && !phase331ThetaOmegaRouteProvidesHiggsScalarSourceOperator
            && !phase331ThetaOmegaRoutePromotesWzMasses
            && !phase331ThetaOmegaRoutePromotesHiggsMass,
        $"p315Passed={ucsdDarkGeometricEnergySourceAuditPassed}; p315ThetaOmega={ucsdDarkGeometricEnergyMentionsThetaOmega}; p315Transcript={ucsdDarkGeometricEnergyEditedTranscriptAvailable}; p331Passed={thetaOmegaInhomogeneousGaugeSourceAuditPassed}; p331ResearchLead={phase331ThetaOmegaRouteGivesResearchLeadForSourceLaw}; p331DirectBridge={phase331ThetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw}; p331Rows={phase331ThetaOmegaRouteProvidesSeparateWzSourceRows}; p331Vev={phase331ThetaOmegaRouteProvidesTargetIndependentVevSource}; p331WeakAngle={phase331ThetaOmegaRouteProvidesWeakMixingAngleSource}; p331HiggsSource={phase331ThetaOmegaRouteProvidesHiggsScalarSourceOperator}"),
    new Check(
        "adjacent-seiberg-witten-and-observed-variable-boundaries-remain-binding",
        seibergWittenMonopoleElectroweakSourceAuditPassed
            && !seibergWittenPromotesWzMasses
            && !seibergWittenPromotesHiggsMass
            && dressingFieldElectroweakObservedVariablesAuditPassed
            && !dressingFieldRoutePromotesWzMasses
            && !dressingFieldRoutePromotesHiggsMass,
        $"p329Passed={seibergWittenMonopoleElectroweakSourceAuditPassed}; swPromotesWz={seibergWittenPromotesWzMasses}; swPromotesHiggs={seibergWittenPromotesHiggsMass}; p365Passed={dressingFieldElectroweakObservedVariablesAuditPassed}; dressingPromotesWz={dressingFieldRoutePromotesWzMasses}; dressingPromotesHiggs={dressingFieldRoutePromotesHiggsMass}"),
    new Check(
        "theta-omega-current-availability-does-not-fill-gu-contracts",
        !currentAvailableThetaOmegaMaterialProvidesEquationAvailabilityOnly
            && currentAvailableThetaOmegaMaterialIsAbstractLevelForUcsdUpdate
            && !routeProvidesDirectWzBridgeSourceLaw
            && !routeProvidesGuLocalWzTheorem
            && !routeProvidesSeparateWzSourceRows
            && !routeProvidesTargetIndependentVevSource
            && !routeProvidesWeakAngleOrCouplingNormalization
            && !routeProvidesWzRawAmplitudeGates
            && !routeProvidesWzCommonBridgeGate
            && !routeProvidesWzStabilitySidecars
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesHiggsScalarSourceOperator
            && !routeProvidesHiggsQuarticOrExcitationSource
            && !routeProvidesHiggsMassiveScalarProfile
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"abstractLevel={currentAvailableThetaOmegaMaterialIsAbstractLevelForUcsdUpdate}; directBridge={routeProvidesDirectWzBridgeSourceLaw}; wzRows={routeProvidesSeparateWzSourceRows}; vev={routeProvidesTargetIndependentVevSource}; coupling={routeProvidesWeakAngleOrCouplingNormalization}; observedRows={routeProvidesObservedPhotonWzHiggsProjectionRows}; higgsSource={routeProvidesHiggsScalarSourceOperator}; geV={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}"),
    new Check(
        "phase213-and-phase256-blocker-state-preserved",
        !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}")
};

var thetaOmegaSourceEquationAvailabilityAuditPassed = checks.All(check => check.Passed)
    && currentPublicSearchPerformed
    && !currentPublicSearchFoundNewPrimaryEquationSource
    && !portalWikiEditedTranscriptAvailable
    && !publicEquationSourceFound
    && !routeProvidesDirectWzBridgeSourceLaw
    && !routeProvidesSeparateWzSourceRows
    && !routeProvidesTargetIndependentVevSource
    && !routeProvidesHiggsScalarSourceOperator
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions;

var terminalStatus = thetaOmegaSourceEquationAvailabilityAuditPassed
    ? "theta-omega-source-equation-availability-audit-no-public-equation-source-law"
    : "theta-omega-source-equation-availability-audit-review-required";

var result = new
{
    phaseId = "phase367-theta-omega-source-equation-availability-audit",
    terminalStatus,
    thetaOmegaSourceEquationAvailabilityAuditPassed,
    currentPublicSearchPerformed,
    currentPublicSearchDate,
    currentPublicSearchFoundNewPrimaryEquationSource,
    currentPublicSearchFoundNewPrimaryTranscript,
    currentPublicSearchFoundNewPrimarySlideDeck,
    currentPublicSearchFoundOnlyExistingPortalAbstractAndWikiState,
    thirdPartyPointersFoundButNotUsedAsSourceEvidence,
    officialPortalAbstractAvailable,
    officialPortalAbstractMentionsThetaOmega,
    officialPortalAbstractMentionsInhomogeneousGauge,
    officialPortalAbstractMentionsDiracSpinorBundle,
    officialPortalAbstractMentionsFourteenDimensionalLorentzianMetrics,
    officialPortalAbstractMentionsSupersymmetricEinsteinDirac,
    officialPortalAbstractMentionsPatiSalamGenerations,
    officialPortalAbstractMentionsSeibergWittenMonopoleEquations,
    officialPortalAbstractProvidesEquationRows,
    officialPortalAbstractProvidesWzMassLaw,
    officialPortalAbstractProvidesHiggsMassLaw,
    portalWikiPageAvailable,
    portalWikiEditedTranscriptAvailable,
    portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess,
    publicCaptionOrTranscriptUsableAsSourceLineage,
    publicUcsdSlidesFound,
    publicFullTranscriptFound,
    publicEquationSourceFound,
    officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable,
    officialOxfordTranscriptGivesInhomogeneousGaugeGroupDefinition,
    officialOxfordTranscriptGivesBiConnectionAndTiltedMapContext,
    officialOxfordTranscriptGivesRepresentationTheoryLead,
    officialOxfordTranscriptProvidesThetaOmegaEquation,
    officialOxfordTranscriptProvidesDirectWzMassLaw,
    officialOxfordTranscriptProvidesHiggsMassLaw,
    officialOxfordTranscriptProvidesObservedPhotonWzHiggsProjectionRows,
    currentAvailableThetaOmegaMaterialProvidesEquationAvailabilityOnly,
    currentAvailableThetaOmegaMaterialIsAbstractLevelForUcsdUpdate,
    routeProvidesDirectWzBridgeSourceLaw,
    routeProvidesGuLocalWzTheorem,
    routeProvidesSeparateWzSourceRows,
    routeProvidesTargetIndependentVevSource,
    routeProvidesWeakAngleOrCouplingNormalization,
    routeProvidesWzRawAmplitudeGates,
    routeProvidesWzCommonBridgeGate,
    routeProvidesWzStabilitySidecars,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesHiggsQuarticOrExcitationSource,
    routeProvidesHiggsMassiveScalarProfile,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    adjacentRouteBoundary = new
    {
        ucsdDarkGeometricEnergySourceAuditPassed,
        ucsdDarkGeometricEnergyMentionsThetaOmega,
        ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup,
        ucsdDarkGeometricEnergyEditedTranscriptAvailable,
        ucsdDarkGeometricEnergyPromotesWzMasses,
        ucsdDarkGeometricEnergyPromotesHiggsMass,
        ucsdTranscriptSourceStrengthAuditPassed,
        phase316CaptionOrTranscriptUsableAsSourceLineage,
        phase316TranscriptAuditPromotesWzMasses,
        phase316TranscriptAuditPromotesHiggsMass,
        thetaOmegaInhomogeneousGaugeSourceAuditPassed,
        phase331ThetaOmegaRouteGivesResearchLeadForSourceLaw,
        phase331ThetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw,
        phase331ThetaOmegaRouteProvidesSeparateWzSourceRows,
        phase331ThetaOmegaRouteProvidesTargetIndependentVevSource,
        phase331ThetaOmegaRouteProvidesWeakMixingAngleSource,
        phase331ThetaOmegaRouteProvidesHiggsScalarSourceOperator,
        phase331ThetaOmegaRoutePromotesWzMasses,
        phase331ThetaOmegaRoutePromotesHiggsMass,
        seibergWittenMonopoleElectroweakSourceAuditPassed,
        seibergWittenPromotesWzMasses,
        seibergWittenPromotesHiggsMass,
        dressingFieldElectroweakObservedVariablesAuditPassed,
        dressingFieldRoutePromotesWzMasses,
        dressingFieldRoutePromotesHiggsMass
    },
    contractImpact = new
    {
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract
    },
    decision = "Do not promote W/Z or Higgs masses from the currently public theta_omega material. The current public source refresh still finds only an official abstract and wiki metadata for the UCSD update; the public Oxford transcript supplies inhomogeneous-gauge equation context, but not a theta_omega equation, W/Z bridge law, target-independent VEV, weak-angle/coupling normalization, observed photon/W/Z/H projection rows, Higgs scalar-source/self-coupling lineage, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A public primary theta_omega equation source, full transcript, slide deck, or paper that exposes calculable equations rather than only an abstract.",
        "A GU-local W/Z theorem with separate W and Z source rows, raw-amplitude gates, common-bridge gates, target-comparison gates, derivation IDs, and stability sidecars.",
        "A GU-local target-independent electroweak scale or VEV source and weak-angle/coupling normalization independent of observed W/Z/H targets.",
        "A GU-local observed-field extraction artifact mapping geometry to photon, W, Z, and Higgs operators before pole-mass extraction.",
        "A GU-local Higgs scalar-source and self-coupling or excitation lineage with GeV unit normalization."
    }
};

var summary = new
{
    result.phaseId,
    result.terminalStatus,
    result.thetaOmegaSourceEquationAvailabilityAuditPassed,
    result.currentPublicSearchPerformed,
    result.currentPublicSearchDate,
    result.currentPublicSearchFoundNewPrimaryEquationSource,
    result.currentPublicSearchFoundNewPrimaryTranscript,
    result.currentPublicSearchFoundNewPrimarySlideDeck,
    result.currentPublicSearchFoundOnlyExistingPortalAbstractAndWikiState,
    result.thirdPartyPointersFoundButNotUsedAsSourceEvidence,
    result.officialPortalAbstractAvailable,
    result.officialPortalAbstractMentionsThetaOmega,
    result.officialPortalAbstractMentionsInhomogeneousGauge,
    result.officialPortalAbstractMentionsDiracSpinorBundle,
    result.officialPortalAbstractMentionsFourteenDimensionalLorentzianMetrics,
    result.officialPortalAbstractMentionsSupersymmetricEinsteinDirac,
    result.officialPortalAbstractMentionsPatiSalamGenerations,
    result.officialPortalAbstractMentionsSeibergWittenMonopoleEquations,
    result.officialPortalAbstractProvidesEquationRows,
    result.officialPortalAbstractProvidesWzMassLaw,
    result.officialPortalAbstractProvidesHiggsMassLaw,
    result.portalWikiPageAvailable,
    result.portalWikiEditedTranscriptAvailable,
    result.portalWikiMachineGeneratedTranscriptRequiresPrivateDiscordAccess,
    result.publicCaptionOrTranscriptUsableAsSourceLineage,
    result.publicUcsdSlidesFound,
    result.publicFullTranscriptFound,
    result.publicEquationSourceFound,
    result.officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable,
    result.officialOxfordTranscriptGivesInhomogeneousGaugeGroupDefinition,
    result.officialOxfordTranscriptGivesBiConnectionAndTiltedMapContext,
    result.officialOxfordTranscriptGivesRepresentationTheoryLead,
    result.officialOxfordTranscriptProvidesThetaOmegaEquation,
    result.officialOxfordTranscriptProvidesDirectWzMassLaw,
    result.officialOxfordTranscriptProvidesHiggsMassLaw,
    result.officialOxfordTranscriptProvidesObservedPhotonWzHiggsProjectionRows,
    result.currentAvailableThetaOmegaMaterialProvidesEquationAvailabilityOnly,
    result.currentAvailableThetaOmegaMaterialIsAbstractLevelForUcsdUpdate,
    result.routeProvidesDirectWzBridgeSourceLaw,
    result.routeProvidesGuLocalWzTheorem,
    result.routeProvidesSeparateWzSourceRows,
    result.routeProvidesTargetIndependentVevSource,
    result.routeProvidesWeakAngleOrCouplingNormalization,
    result.routeProvidesWzRawAmplitudeGates,
    result.routeProvidesWzCommonBridgeGate,
    result.routeProvidesWzStabilitySidecars,
    result.routeProvidesObservedPhotonWzHiggsProjectionRows,
    result.routeProvidesHiggsScalarSourceOperator,
    result.routeProvidesHiggsQuarticOrExcitationSource,
    result.routeProvidesHiggsMassiveScalarProfile,
    result.routeProvidesGeVUnitNormalization,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.adjacentRouteBoundary,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "theta_omega_source_equation_availability_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "theta_omega_source_equation_availability_audit_summary.json"),
    JsonSerializer.Serialize(summary, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"thetaOmegaSourceEquationAvailabilityAuditPassed={thetaOmegaSourceEquationAvailabilityAuditPassed}");
Console.WriteLine($"currentPublicSearchFoundNewPrimaryEquationSource={currentPublicSearchFoundNewPrimaryEquationSource}");
Console.WriteLine($"portalWikiEditedTranscriptAvailable={portalWikiEditedTranscriptAvailable}");
Console.WriteLine($"officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable={officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable}");
Console.WriteLine($"routeProvidesDirectWzBridgeSourceLaw={routeProvidesDirectWzBridgeSourceLaw}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var property))
    {
        return null;
    }

    return property.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var property))
    {
        return null;
    }

    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
    {
        return value;
    }

    return null;
}

record Check(string CheckId, bool Passed, string Detail);
record SourceRow(string SourceId, string UrlOrQuery, string Locator, string Finding, string PredictionImpact);

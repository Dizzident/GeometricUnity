using System.Text.Json;

const string DefaultOutputDir = "studies/phase370_completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_001/output";
const string CompletionRevisionPath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase226Path = "studies/phase226_official_gu_higgs_potential_notation_audit_001/output/official_gu_higgs_potential_notation_audit_summary.json";
const string Phase227Path = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output/official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json";
const string Phase237Path = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output/cox_ii_higgs_yukawa_texture_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase267Path = "studies/phase267_completion_revision_direct_bridge_source_audit_001/output/completion_revision_direct_bridge_source_audit_summary.json";
const string Phase273Path = "studies/phase273_boson_fermion_coupling_proxy_source_audit_001/output/boson_fermion_coupling_proxy_source_audit_summary.json";
const string Phase310Path = "studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output/completion_variational_branch_to_wz_normalization_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase355Path = "studies/phase355_dirac_lichnerowicz_yang_mills_higgs_source_audit_001/output/dirac_lichnerowicz_yang_mills_higgs_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE370_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var completionLines = File.ReadAllLines(CompletionRevisionPath);
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase226 = JsonDocument.Parse(File.ReadAllText(Phase226Path));
using var phase227 = JsonDocument.Parse(File.ReadAllText(Phase227Path));
using var phase237 = JsonDocument.Parse(File.ReadAllText(Phase237Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase267 = JsonDocument.Parse(File.ReadAllText(Phase267Path));
using var phase273 = JsonDocument.Parse(File.ReadAllText(Phase273Path));
using var phase310 = JsonDocument.Parse(File.ReadAllText(Phase310Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase355 = JsonDocument.Parse(File.ReadAllText(Phase355Path));

const string officialGuDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";
const string officialOxfordTranscriptUrl = "https://geometricunity.org/2013-oxford-lecture/";
const string diracYukawaOperatorUrl = "https://arxiv.org/abs/hep-th/9612149";
const string generalizedLichnerowiczUrl = "https://arxiv.org/abs/hep-th/9503153";

var completionEvidence = new[]
{
    LineEvidence(4772, "Yukawa-like part is still only a target, not a completed geometric derivation", "The completion revision classifies the Yukawa-like layer as a target rather than a completed geometric derivation."),
    LineEvidence(4782, "Without a mass-generation mechanism", "The completion revision says the fermionic sector remains kinematic without a mass-generation mechanism."),
    LineEvidence(12362, "Fermionic variational placeholder with typed obligations", "The completion revision labels the fermionic variational layer as a placeholder."),
    LineEvidence(12371, "collects Yukawa-like or lower-order coupling terms", "The completion revision introduces a Yukawa-like term container without deriving its contents."),
    LineEvidence(12380, "typed placeholder rather than a completed derivation", "The completion revision explicitly withholds completed-derivation status."),
    LineEvidence(12396, "Complete the fermionic variational branch", "VO-6 leaves the fermionic operator domain, adjoint convention, and coupling terms open."),
    LineEvidence(12398, "precise mixed linearization blocks", "VO-7 leaves coupled boson-fermion mixed linearization and gauge identities open."),
    LineEvidence(12465, "Fermionic coupling", "The first executable branch explicitly excludes fermionic coupling."),
};

const bool officialDraftFermionicSectorArchitecturePresent = true;
const bool officialDraftLocatesHiggsPotentialInUpsilonNorm = true;
const bool officialDraftLocatesYukawaCouplingsAsObservedVev = true;
const bool officialOxfordTranscriptDiracSquareProgramPresent = true;
var completionTypedPlaceholderEvidencePresent = completionEvidence.All(row => row.Found);
const bool completionProvidesTypedFermionicActionTemplate = true;
const bool completionRecordsYukawaLikeLowerOrderTermPlaceholder = true;
const bool completionRecordsVo6FermionicBranchObligation = true;
const bool completionRecordsVo7CoupledMixedLinearizationObligation = true;
const bool completionExecutableBranchIncludesFermionicCoupling = false;

const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesFixedFermionicOperatorBranch = false;
const bool routeProvidesExplicitYukawaFunctional = false;
const bool routeProvidesSolvedYukawaCouplingMap = false;
const bool routeProvidesCoupledResidual = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesTargetIndependentGuVevSource = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesScalarProjectionTheorem = false;
const bool routeProvidesScalarNormalizationSource = false;
const bool routeProvidesHiggsIdentityEnvelope = false;
const bool routeProvidesMassiveScalarProfile = false;
const bool routeProvidesHiggsQuarticOrExcitationSource = false;
const bool routeProvidesTargetIndependentHiggsPredictionRow = false;
const bool routeProvidesPoleMassExtraction = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
var phase201HiggsFieldsDefensiblyFilled = Array.Empty<string>();

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var p207CanPromoteHiggsQuarticSelfCouplingSource = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var p207IntakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? -1;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var p226OfficialGuHiggsPotentialNotationObstructionCertified = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationObstructionCertified") is true;
var p226OfficialGuHiggsPotentialNotationPromotable = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationPromotable") is true;
var p227OfficialGuShiabUpsilonExtractionObstructionCertified = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionObstructionCertified") is true;
var p227OfficialGuShiabUpsilonExtractionPromotable = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionPromotable") is true;
var p237CoxIiHiggsYukawaTextureDependencyAuditPassed = JsonBool(phase237.RootElement, "coxIiHiggsYukawaTextureDependencyAuditPassed") is true;
var p237CoxIiYukawaTextureLeadPresent = JsonBool(phase237.RootElement, "coxIiYukawaTextureLeadPresent") is true;
var p237CoxIiHiggsYukawaTexturePromotableForHiggsMass = JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true;
var p256ObservedFieldExtractionIntakeContractPassed = JsonBool(phase256.RootElement, "observedFieldExtractionIntakeContractPassed") is true;
var p256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var p256ObservedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var p267CompletionRevisionDirectBridgeSourceAuditPassed = JsonBool(phase267.RootElement, "completionRevisionDirectBridgeSourceAuditPassed") is true;
var p267LatestCompletionProvidesHiggsScalarSource = JsonBool(phase267.RootElement, "latestCompletionProvidesHiggsScalarSource") is true;
var p273CouplingProxySourceAuditPassed = JsonBool(phase273.RootElement, "couplingProxySourceAuditPassed") is true;
var p273Phase12FiniteDifferenceOnly = JsonBool(phase273.RootElement, "phase12FiniteDifferenceOnly") is true;
var p273CouplingProxyPromotesHiggsMass = JsonBool(phase273.RootElement, "couplingProxyPromotesHiggsMass") is true;
var p310CompletionVariationalBranchToWzNormalizationAuditPassed = JsonBool(phase310.RootElement, "completionVariationalBranchToWzNormalizationAuditPassed") is true;
var p310CompletionDraftCanPromotePhase302Lead = JsonBool(phase310.RootElement, "completionDraftCanPromotePhase302Lead") is true;
var p322HiggsUpsilonScalarSourceBoundaryAuditPassed = JsonBool(phase322.RootElement, "higgsUpsilonScalarSourceBoundaryAuditPassed") is true;
var p322OfficialGuSourcesProvideFixedScalarSourceOperator = JsonBool(phase322.RootElement, "officialGuSourcesProvideFixedScalarSourceOperator") is true;
var p322OfficialGuSourcesProvideMassiveScalarProfile = JsonBool(phase322.RootElement, "officialGuSourcesProvideMassiveScalarProfile") is true;
var p322OfficialGuSourcesProvideQuarticSelfCouplingValue = JsonBool(phase322.RootElement, "officialGuSourcesProvideQuarticSelfCouplingValue") is true;
var p323CoupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var p323OfficialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var p323OfficialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var p355DiracLichnerowiczYangMillsHiggsSourceAuditPassed = JsonBool(phase355.RootElement, "diracLichnerowiczYangMillsHiggsSourceAuditPassed") is true;
var p355RouteUsesDiracYukawaOperator = JsonBool(phase355.RootElement, "routeUsesDiracYukawaOperator") is true;
var p355RouteProvidesGuLocalDiracOperatorMap = JsonBool(phase355.RootElement, "routeProvidesGuLocalDiracOperatorMap") is true;
var p355RouteProvidesGuHiggsScalarSourceOperator = JsonBool(phase355.RootElement, "routeProvidesGuHiggsScalarSourceOperator") is true;

var sourceRows = new[]
{
    new SourceRow(
        "official-gu-draft-2021-fermionic-sector-and-appendix",
        officialGuDraftUrl,
        "section 9.3 and Appendix locations",
        "The official draft contains a fermionic sector and locates Higgs potential and Yukawa-coupling targets inside the GU architecture.",
        "GU-native source direction, but not a solved scalar-source or mixed-linearization derivation."),
    new SourceRow(
        "official-oxford-transcript-dirac-square-program",
        officialOxfordTranscriptUrl,
        "Higgs and Dirac-square discussion",
        "The Oxford transcript motivates obtaining Yang-Mills/Higgs-like second-order structure by squaring a first-order geometric object.",
        "GU-native equation-program context, but not a replayable Higgs mass row."),
    new SourceRow(
        "local-completion-v29-fermionic-placeholder",
        CompletionRevisionPath,
        "lines 4772, 12362-12380, 12396-12398, and 12465",
        "The local completion revision types a fermionic action template and makes the missing Yukawa map and mixed linearization blocks auditable.",
        "Local completion scaffold, not primary GU evidence and not a solved source lineage."),
    new SourceRow(
        "external-dirac-yukawa-action-template",
        diracYukawaOperatorUrl,
        "abstract",
        "Generalized Dirac-operator literature shows that gravity/Yang-Mills/Higgs action structures can be organized using a Dirac-Yukawa operator.",
        "External template only; Phase355 records that it does not supply a GU-local operator map or scalar source."),
};

var checks = new[]
{
    new Check(
        "official-gu-fermionic-yukawa-direction-recorded",
        officialDraftFermionicSectorArchitecturePresent
            && officialDraftLocatesHiggsPotentialInUpsilonNorm
            && officialDraftLocatesYukawaCouplingsAsObservedVev
            && officialOxfordTranscriptDiracSquareProgramPresent,
        $"fermionicSector={officialDraftFermionicSectorArchitecturePresent}; higgsPotential={officialDraftLocatesHiggsPotentialInUpsilonNorm}; yukawaVev={officialDraftLocatesYukawaCouplingsAsObservedVev}; diracSquare={officialOxfordTranscriptDiracSquareProgramPresent}"),
    new Check(
        "completion-v29-placeholder-and-proof-obligations-recorded",
        completionTypedPlaceholderEvidencePresent
            && completionProvidesTypedFermionicActionTemplate
            && completionRecordsYukawaLikeLowerOrderTermPlaceholder
            && completionRecordsVo6FermionicBranchObligation
            && completionRecordsVo7CoupledMixedLinearizationObligation
            && !completionExecutableBranchIncludesFermionicCoupling,
        $"lineEvidence={completionEvidence.Count(row => row.Found)}/{completionEvidence.Length}; template={completionProvidesTypedFermionicActionTemplate}; yukawaPlaceholder={completionRecordsYukawaLikeLowerOrderTermPlaceholder}; vo6={completionRecordsVo6FermionicBranchObligation}; vo7={completionRecordsVo7CoupledMixedLinearizationObligation}; executableIncludesFermions={completionExecutableBranchIncludesFermionicCoupling}"),
    new Check(
        "prior-gu-higgs-and-coupled-extraction-boundaries-remain-binding",
        p226OfficialGuHiggsPotentialNotationObstructionCertified
            && !p226OfficialGuHiggsPotentialNotationPromotable
            && p227OfficialGuShiabUpsilonExtractionObstructionCertified
            && !p227OfficialGuShiabUpsilonExtractionPromotable
            && p237CoxIiHiggsYukawaTextureDependencyAuditPassed
            && p237CoxIiYukawaTextureLeadPresent
            && !p237CoxIiHiggsYukawaTexturePromotableForHiggsMass
            && p322HiggsUpsilonScalarSourceBoundaryAuditPassed
            && !p322OfficialGuSourcesProvideFixedScalarSourceOperator
            && !p322OfficialGuSourcesProvideMassiveScalarProfile
            && !p322OfficialGuSourcesProvideQuarticSelfCouplingValue
            && p323CoupledYangMillsHiggsMassExtractionAuditPassed
            && !p323OfficialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !p323OfficialPublicSourcesProvidePhotonWzHiggsProjectionRows,
        $"p226={p226OfficialGuHiggsPotentialNotationObstructionCertified}; p227={p227OfficialGuShiabUpsilonExtractionObstructionCertified}; p237={p237CoxIiHiggsYukawaTextureDependencyAuditPassed}; p237YukawaLead={p237CoxIiYukawaTextureLeadPresent}; p237Promotable={p237CoxIiHiggsYukawaTexturePromotableForHiggsMass}; p322={p322HiggsUpsilonScalarSourceBoundaryAuditPassed}; scalarOperator={p322OfficialGuSourcesProvideFixedScalarSourceOperator}; massiveProfile={p322OfficialGuSourcesProvideMassiveScalarProfile}; quartic={p322OfficialGuSourcesProvideQuarticSelfCouplingValue}; p323={p323CoupledYangMillsHiggsMassExtractionAuditPassed}; quadraticExpansion={p323OfficialPublicSourcesProvideGaugeFixedQuadraticExpansion}; projectionRows={p323OfficialPublicSourcesProvidePhotonWzHiggsProjectionRows}"),
    new Check(
        "completion-and-proxy-routes-still-nonpromotional",
        p267CompletionRevisionDirectBridgeSourceAuditPassed
            && !p267LatestCompletionProvidesHiggsScalarSource
            && p273CouplingProxySourceAuditPassed
            && p273Phase12FiniteDifferenceOnly
            && !p273CouplingProxyPromotesHiggsMass
            && p310CompletionVariationalBranchToWzNormalizationAuditPassed
            && !p310CompletionDraftCanPromotePhase302Lead,
        $"p267={p267CompletionRevisionDirectBridgeSourceAuditPassed}; p267HiggsSource={p267LatestCompletionProvidesHiggsScalarSource}; p273={p273CouplingProxySourceAuditPassed}; finiteDifferenceOnly={p273Phase12FiniteDifferenceOnly}; p273PromotesHiggs={p273CouplingProxyPromotesHiggsMass}; p310={p310CompletionVariationalBranchToWzNormalizationAuditPassed}; p310Promotes={p310CompletionDraftCanPromotePhase302Lead}"),
    new Check(
        "external-dirac-yukawa-template-does-not-fill-gu-lineage",
        p355DiracLichnerowiczYangMillsHiggsSourceAuditPassed
            && p355RouteUsesDiracYukawaOperator
            && !p355RouteProvidesGuLocalDiracOperatorMap
            && !p355RouteProvidesGuHiggsScalarSourceOperator,
        $"p355={p355DiracLichnerowiczYangMillsHiggsSourceAuditPassed}; diracYukawa={p355RouteUsesDiracYukawaOperator}; guDiracMap={p355RouteProvidesGuLocalDiracOperatorMap}; guScalarSource={p355RouteProvidesGuHiggsScalarSourceOperator}"),
    new Check(
        "fermionic-placeholder-does-not-fill-source-contracts",
        !routeProvidesCompletedFermionicAction
            && !routeProvidesFixedFermionicOperatorBranch
            && !routeProvidesExplicitYukawaFunctional
            && !routeProvidesSolvedYukawaCouplingMap
            && !routeProvidesCoupledResidual
            && !routeProvidesCompletedMixedLinearizationBlocks
            && !routeProvidesMixedLinearizationGaugeCompatibilityIdentities
            && !routeProvidesDirectTargetIndependentWzBridgeSourceLaw
            && !routeProvidesSeparateWzSourceRows
            && !routeProvidesTargetIndependentGuVevSource
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesHiggsScalarSourceOperator
            && !routeProvidesScalarProjectionTheorem
            && !routeProvidesScalarNormalizationSource
            && !routeProvidesHiggsIdentityEnvelope
            && !routeProvidesMassiveScalarProfile
            && !routeProvidesHiggsQuarticOrExcitationSource
            && !routeProvidesTargetIndependentHiggsPredictionRow
            && !routeProvidesPoleMassExtraction
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && phase201HiggsFieldsDefensiblyFilled.Length == 0,
        $"fermionicAction={routeProvidesCompletedFermionicAction}; fixedOperator={routeProvidesFixedFermionicOperatorBranch}; explicitYukawa={routeProvidesExplicitYukawaFunctional}; yukawaMap={routeProvidesSolvedYukawaCouplingMap}; coupledResidual={routeProvidesCoupledResidual}; mixedBlocks={routeProvidesCompletedMixedLinearizationBlocks}; scalarSource={routeProvidesHiggsScalarSourceOperator}; scalarProjection={routeProvidesScalarProjectionTheorem}; scalarNormalization={routeProvidesScalarNormalizationSource}; identityEnvelope={routeProvidesHiggsIdentityEnvelope}; massiveProfile={routeProvidesMassiveScalarProfile}; quartic={routeProvidesHiggsQuarticOrExcitationSource}; higgsFilled={phase201HiggsFieldsDefensiblyFilled.Length}; promotesHiggs={routePromotesHiggsMass}"),
    new Check(
        "phase201-phase207-phase213-blocker-state-preserved",
        !phase201AllRequiredLineagesPromotable
            && !p207CanPromoteHiggsQuarticSelfCouplingSource
            && p207IntakeReadyFindingCount == 0
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && p256ObservedFieldExtractionIntakeContractPassed
            && p256FilledRequiredFieldCount == 0
            && !p256ObservedFieldExtractionContractPromotable,
        $"phase201AllLineages={phase201AllRequiredLineagesPromotable}; p207Promotable={p207CanPromoteHiggsQuarticSelfCouplingSource}; p207IntakeReady={p207IntakeReadyFindingCount}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; p256={p256ObservedFieldExtractionIntakeContractPassed}; p256Filled={p256FilledRequiredFieldCount}; p256Promotable={p256ObservedFieldExtractionContractPromotable}"),
};

var completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed = checks.All(check => check.Passed);
var terminalStatus = completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed
    ? "completion-fermionic-yukawa-higgs-mixed-linearization-source-audit-placeholder-not-source-lineage"
    : "completion-fermionic-yukawa-higgs-mixed-linearization-source-audit-review-required";

var result = new
{
    phaseId = "phase370-completion-fermionic-yukawa-higgs-mixed-linearization-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed,
    officialDraftFermionicSectorArchitecturePresent,
    officialDraftLocatesHiggsPotentialInUpsilonNorm,
    officialDraftLocatesYukawaCouplingsAsObservedVev,
    officialOxfordTranscriptDiracSquareProgramPresent,
    completionTypedPlaceholderEvidencePresent,
    completionProvidesTypedFermionicActionTemplate,
    completionRecordsYukawaLikeLowerOrderTermPlaceholder,
    completionRecordsVo6FermionicBranchObligation,
    completionRecordsVo7CoupledMixedLinearizationObligation,
    completionExecutableBranchIncludesFermionicCoupling,
    routeProvidesCompletedFermionicAction,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesSeparateWzSourceRows,
    routeProvidesTargetIndependentGuVevSource,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesScalarNormalizationSource,
    routeProvidesHiggsIdentityEnvelope,
    routeProvidesMassiveScalarProfile,
    routeProvidesHiggsQuarticOrExcitationSource,
    routeProvidesTargetIndependentHiggsPredictionRow,
    routeProvidesPoleMassExtraction,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201HiggsFieldsDefensiblyFilled,
    sourceRowCount = sourceRows.Length,
    sourceRows,
    completionRevision = new
    {
        path = CompletionRevisionPath,
        totalLineCount = completionLines.Length,
        completionEvidence,
    },
    adjacentRouteBoundary = new
    {
        phase226 = new { p226OfficialGuHiggsPotentialNotationObstructionCertified, p226OfficialGuHiggsPotentialNotationPromotable },
        phase227 = new { p227OfficialGuShiabUpsilonExtractionObstructionCertified, p227OfficialGuShiabUpsilonExtractionPromotable },
        phase237 = new { p237CoxIiHiggsYukawaTextureDependencyAuditPassed, p237CoxIiYukawaTextureLeadPresent, p237CoxIiHiggsYukawaTexturePromotableForHiggsMass },
        phase256 = new { p256ObservedFieldExtractionIntakeContractPassed, p256FilledRequiredFieldCount, p256ObservedFieldExtractionContractPromotable },
        phase267 = new { p267CompletionRevisionDirectBridgeSourceAuditPassed, p267LatestCompletionProvidesHiggsScalarSource },
        phase273 = new { p273CouplingProxySourceAuditPassed, p273Phase12FiniteDifferenceOnly, p273CouplingProxyPromotesHiggsMass },
        phase310 = new { p310CompletionVariationalBranchToWzNormalizationAuditPassed, p310CompletionDraftCanPromotePhase302Lead },
        phase322 = new { p322HiggsUpsilonScalarSourceBoundaryAuditPassed, p322OfficialGuSourcesProvideFixedScalarSourceOperator, p322OfficialGuSourcesProvideMassiveScalarProfile, p322OfficialGuSourcesProvideQuarticSelfCouplingValue },
        phase323 = new { p323CoupledYangMillsHiggsMassExtractionAuditPassed, p323OfficialPublicSourcesProvideGaugeFixedQuadraticExpansion, p323OfficialPublicSourcesProvidePhotonWzHiggsProjectionRows },
        phase355 = new { p355DiracLichnerowiczYangMillsHiggsSourceAuditPassed, p355RouteUsesDiracYukawaOperator, p355RouteProvidesGuLocalDiracOperatorMap, p355RouteProvidesGuHiggsScalarSourceOperator },
    },
    contractImpact = new
    {
        phase201AllRequiredLineagesPromotable,
        p207CanPromoteHiggsQuarticSelfCouplingSource,
        p207IntakeReadyFindingCount,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201HiggsFieldsDefensiblyFilled,
        observedFieldExtractionFilledRequiredFieldCount = p256FilledRequiredFieldCount,
    },
    checks,
    decision = "Do not promote W/Z or Higgs masses from the v29 fermionic variational / Yukawa-Higgs mixed-linearization route. The official GU material and the completion revision identify a meaningful GU-native research direction, but v29 explicitly leaves the Yukawa map as a target, the fermionic action as a typed placeholder, the coupled mixed-linearization blocks as VO-7, and fermionic coupling outside the first executable branch. No solved Higgs scalar-source operator, identity envelope, massive profile, quartic or excitation relation, observed projection, W/Z theorem, target-independent scale, pole extraction, or GeV normalization is supplied.",
    nextRequiredArtifact = new[]
    {
        "A GU-native completed fermionic action with a fixed operator branch and solved Yukawa or lower-order coupling map.",
        "A coupled boson-fermion Hessian or deformation package with explicit mixed-linearization blocks and corrected-gauge compatibility identities.",
        "A scalar-sector extraction theorem mapping the solved mixed branch to a Higgs source operator, identity envelope, massive scalar profile, and quartic or excitation relation.",
        "Observed photon/W/Z/H projection rows, target-independent VEV or mass scale, pole extraction, and GeV unit lineage before any physical mass promotion.",
    },
    sourceEvidence = new
    {
        officialGuDraftUrl,
        officialOxfordTranscriptUrl,
        generalizedLichnerowiczUrl,
        diracYukawaOperatorUrl,
        completionRevisionPath = CompletionRevisionPath,
        phase201Path = Phase201Path,
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase226Path = Phase226Path,
        phase227Path = Phase227Path,
        phase237Path = Phase237Path,
        phase256Path = Phase256Path,
        phase267Path = Phase267Path,
        phase273Path = Phase273Path,
        phase310Path = Phase310Path,
        phase322Path = Phase322Path,
        phase323Path = Phase323Path,
        phase355Path = Phase355Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "completion_fermionic_yukawa_higgs_mixed_linearization_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed,
        result.officialDraftFermionicSectorArchitecturePresent,
        result.officialDraftLocatesHiggsPotentialInUpsilonNorm,
        result.officialDraftLocatesYukawaCouplingsAsObservedVev,
        result.officialOxfordTranscriptDiracSquareProgramPresent,
        result.completionTypedPlaceholderEvidencePresent,
        result.completionProvidesTypedFermionicActionTemplate,
        result.completionRecordsYukawaLikeLowerOrderTermPlaceholder,
        result.completionRecordsVo6FermionicBranchObligation,
        result.completionRecordsVo7CoupledMixedLinearizationObligation,
        result.completionExecutableBranchIncludesFermionicCoupling,
        result.routeProvidesCompletedFermionicAction,
        result.routeProvidesFixedFermionicOperatorBranch,
        result.routeProvidesExplicitYukawaFunctional,
        result.routeProvidesSolvedYukawaCouplingMap,
        result.routeProvidesCoupledResidual,
        result.routeProvidesCompletedMixedLinearizationBlocks,
        result.routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
        result.routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
        result.routeProvidesSeparateWzSourceRows,
        result.routeProvidesTargetIndependentGuVevSource,
        result.routeProvidesObservedPhotonWzHiggsProjectionRows,
        result.routeProvidesGuObservedFieldExtraction,
        result.routeProvidesHiggsScalarSourceOperator,
        result.routeProvidesScalarProjectionTheorem,
        result.routeProvidesScalarNormalizationSource,
        result.routeProvidesHiggsIdentityEnvelope,
        result.routeProvidesMassiveScalarProfile,
        result.routeProvidesHiggsQuarticOrExcitationSource,
        result.routeProvidesTargetIndependentHiggsPredictionRow,
        result.routeProvidesPoleMassExtraction,
        result.routeProvidesGeVUnitNormalization,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.phase201HiggsFieldsDefensiblyFilled,
        result.sourceRowCount,
        result.sourceRows,
        result.completionRevision,
        result.adjacentRouteBoundary,
        result.contractImpact,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed={completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed}");
Console.WriteLine($"completionTypedPlaceholderEvidencePresent={completionTypedPlaceholderEvidencePresent}");
Console.WriteLine($"completionRecordsVo7CoupledMixedLinearizationObligation={completionRecordsVo7CoupledMixedLinearizationObligation}");
Console.WriteLine($"routeProvidesHiggsScalarSourceOperator={routeProvidesHiggsScalarSourceOperator}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201HiggsContract={canFillPhase201HiggsContract}");
Console.WriteLine($"phase201HiggsFieldsDefensiblyFilledCount={phase201HiggsFieldsDefensiblyFilled.Length}");

LineEvidenceRecord LineEvidence(int lineNumber, string requiredCue, string auditFinding)
{
    var excerpt = lineNumber >= 1 && lineNumber <= completionLines.Length
        ? completionLines[lineNumber - 1].Trim()
        : "line missing";
    return new LineEvidenceRecord(
        CompletionRevisionPath,
        lineNumber,
        excerpt.Contains(requiredCue, StringComparison.Ordinal),
        requiredCue,
        auditFinding,
        excerpt);
}

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record LineEvidenceRecord(string Path, int LineNumber, bool Found, string RequiredCue, string AuditFinding, string Excerpt);
sealed record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);

using System.Text.Json;

const string DefaultOutputDir = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold_summary.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";
const string Phase226Path = "studies/phase226_official_gu_higgs_potential_notation_audit_001/output/official_gu_higgs_potential_notation_audit_summary.json";
const string Phase227Path = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output/official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase318Path = "studies/phase318_deferred_implementation_gap_repairability_audit_001/output/deferred_implementation_gap_repairability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE322_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase187 = JsonDocument.Parse(File.ReadAllText(Phase187Path));
using var phase189 = JsonDocument.Parse(File.ReadAllText(Phase189Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));
using var phase226 = JsonDocument.Parse(File.ReadAllText(Phase226Path));
using var phase227 = JsonDocument.Parse(File.ReadAllText(Phase227Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase318 = JsonDocument.Parse(File.ReadAllText(Phase318Path));

const bool officialGuSiteCurrentDraftReferencePresent = true;
const string officialGuSiteUrl = "https://geometricunity.org/";
const string officialGuDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";
const string officialGuLectureTranscriptUrl = "https://geometricunity.org/2013-oxford-lecture/";
const bool officialGuDraftAppendixMapsHiggsPotentialToUpsilonNorm = true;
const bool officialGuLectureFramesHiggsSectorAsGeometricProblem = true;
const bool officialGuLectureDiracSquareRootProgramForSecondOrderEquationsPresent = true;
const bool officialGuLectureShiabUpsilonActionStructurePresent = true;
const bool officialGuSourcesProvideFixedScalarSourceOperator = false;
const bool officialGuSourcesProvideFixedKappaOrInnerProductNormalization = false;
const bool officialGuSourcesProvideUpsilonComponentExtractionTheorem = false;
const bool officialGuSourcesProvideObserverSectorProjection = false;
const bool officialGuSourcesProvideMassiveScalarProfile = false;
const bool officialGuSourcesProvideQuarticSelfCouplingValue = false;
const bool officialGuSourcesPromoteHiggsMass = false;
const bool higgsUpsilonRoutePromotesHiggsMass = false;
const bool higgsUpsilonRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var p187ScaffoldMaterialized = JsonBool(phase187.RootElement, "scaffoldMaterialized") is true;
var p187HiggsSourceIdentityValidated = JsonBool(phase187.RootElement, "higgsSourceIdentityValidated") is true;
var p187PredictionAttemptAllowed = JsonBool(phase187.RootElement, "predictionAttemptAllowed") is true;

var p189CensusPromotable = JsonBool(phase189.RootElement, "censusPromotable") is true;
var p189PredictionAttemptAllowed = JsonBool(phase189.RootElement, "predictionAttemptAllowed") is true;
var p189CandidateSummary = phase189.RootElement.GetProperty("candidateSummary");
var p189ScalarFeatureEnvelopeCount = JsonInt(p189CandidateSummary, "scalarFeatureEnvelopeCount") ?? -1;
var p189BranchStableNonC0Count = JsonInt(p189CandidateSummary, "branchStableNonC0Count") ?? -1;
var p189MassiveScalarProfileCount = JsonInt(p189CandidateSummary, "massiveScalarProfileCount") ?? -1;

var p196CanPromoteHiggsFromPotentialOrSelfCoupling = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var p199CanPromoteAnyHiggsScalarSourceLineage = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var p207CanPromoteHiggsQuarticSelfCouplingSource = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var p207IntakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? -1;

var p215CanPromoteTargetImpliedHiggsSelfCoupling = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is true;
var p223NumericalLeadPresent = JsonBool(phase223.RootElement, "numericalLeadPresent") is true;
var p223CanPromoteHiggsCasimirQuarticLead = JsonBool(phase223.RootElement, "canPromoteHiggsCasimirQuarticLead") is true;
var p223BestFactor = phase223.RootElement.TryGetProperty("bestProbe", out var p223BestProbe)
    ? JsonString(p223BestProbe, "factorExpression")
    : null;

var p226OfficialGuHiggsPotentialNotationObstructionCertified = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationObstructionCertified") is true;
var p226OfficialGuHiggsPotentialNotationPromotable = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationPromotable") is true;

var p227OfficialGuShiabUpsilonExtractionObstructionCertified = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionObstructionCertified") is true;
var p227OfficialGuShiabUpsilonExtractionPromotable = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionPromotable") is true;
var p227UnresolvedExtractionBlockerCount = JsonArray(phase227.RootElement, "unresolvedExtractionBlockers").Count(row => JsonBool(row, "filled") is false);

var p248HiggsScalarRepairabilityAuditPassed = JsonBool(phase248.RootElement, "higgsScalarRepairabilityAuditPassed") is true;
var p248CurrentHiggsNumericalLeadPromotable = JsonBool(phase248.RootElement, "currentHiggsNumericalLeadPromotable") is true;
var p248HiggsScalarSourceRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var p248ThreeTenthsFactorDerivableFromCurrentScalarSource = JsonBool(phase248.RootElement, "threeTenthsFactorDerivableFromCurrentScalarSource") is true;
var p248CurrentRegistryHasScalarIdentityFeatures = JsonBool(phase248.RootElement, "currentRegistryHasScalarIdentityFeatures") is true;
var p248CurrentRegistryHasMassiveScalarProfile = JsonBool(phase248.RootElement, "currentRegistryHasMassiveScalarProfile") is true;
var p248NewHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;

var phase318DeferredImplementationGapRepairabilityAuditPassed = JsonBool(phase318.RootElement, "deferredImplementationGapRepairabilityAuditPassed") is true;
var phase318LaunchableCodeOnlyPredictionFixFound = JsonBool(phase318.RootElement, "launchableCodeOnlyPredictionFixFound") is true;
var phase318QuarticInteractionProxyDeferred = JsonBool(phase318.RootElement, "quarticInteractionProxyDeferred") is true;
var phase318QuarticProxyImplementationPromotesHiggsMass = phase318.RootElement.TryGetProperty("contractImpact", out var phase318ContractImpact)
    && JsonBool(phase318ContractImpact, "quarticProxyImplementationPromotesHiggsMass") is true;
var phase318CanDeriveHiggsScalarSource = phase318.RootElement.TryGetProperty("contractImpact", out phase318ContractImpact)
    && JsonBool(phase318ContractImpact, "codeOnlyFixCanDeriveHiggsScalarSource") is true;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var higgsSourceRequirements = new[]
{
    new Requirement("official-higgs-potential-upsilon-location", "Official GU source locations for Higgs field/potential and Upsilon equations.", officialGuDraftAppendixMapsHiggsPotentialToUpsilonNorm && officialGuLectureShiabUpsilonActionStructurePresent),
    new Requirement("fixed-scalar-source-operator", "A fixed scalar source/operator identity suitable for Higgs extraction.", officialGuSourcesProvideFixedScalarSourceOperator),
    new Requirement("fixed-kappa-inner-product-normalization", "A target-independent kappa or inner-product normalization for the scalar sector.", officialGuSourcesProvideFixedKappaOrInnerProductNormalization),
    new Requirement("upsilon-component-extraction-theorem", "A checked theorem decomposing Upsilon components into physical scalar source rows.", officialGuSourcesProvideUpsilonComponentExtractionTheorem),
    new Requirement("observer-sector-projection", "A checked observer-pullback and observed-sector projection for the Higgs row.", officialGuSourcesProvideObserverSectorProjection),
    new Requirement("higgs-identity-envelope", "A target-independent Higgs identity envelope.", p187HiggsSourceIdentityValidated),
    new Requirement("massive-scalar-profile", "A target-independent massive scalar profile.", p189MassiveScalarProfileCount > 0 && p248CurrentRegistryHasMassiveScalarProfile),
    new Requirement("quartic-self-coupling-source", "A source-derived quartic/self-coupling value or scalar excitation relation.", officialGuSourcesProvideQuarticSelfCouplingValue || p207CanPromoteHiggsQuarticSelfCouplingSource),
    new Requirement("higgs-contract-lineage", "Phase201 Higgs source-lineage intake fields and gates filled by the Upsilon/scalar route.", canFillPhase201HiggsContract),
};

var checks = new[]
{
    new Check(
        "official-gu-higgs-upsilon-source-locations-recorded",
        officialGuSiteCurrentDraftReferencePresent
            && officialGuDraftAppendixMapsHiggsPotentialToUpsilonNorm
            && officialGuLectureFramesHiggsSectorAsGeometricProblem
            && officialGuLectureDiracSquareRootProgramForSecondOrderEquationsPresent
            && officialGuLectureShiabUpsilonActionStructurePresent
            && p226OfficialGuHiggsPotentialNotationObstructionCertified
            && !p226OfficialGuHiggsPotentialNotationPromotable
            && p227OfficialGuShiabUpsilonExtractionObstructionCertified
            && !p227OfficialGuShiabUpsilonExtractionPromotable,
        $"officialDraft={officialGuSiteCurrentDraftReferencePresent}; higgsPotentialUpsilon={officialGuDraftAppendixMapsHiggsPotentialToUpsilonNorm}; geometricHiggsProblem={officialGuLectureFramesHiggsSectorAsGeometricProblem}; diracProgram={officialGuLectureDiracSquareRootProgramForSecondOrderEquationsPresent}; shiabUpsilon={officialGuLectureShiabUpsilonActionStructurePresent}; p226Promotable={p226OfficialGuHiggsPotentialNotationPromotable}; p227Promotable={p227OfficialGuShiabUpsilonExtractionPromotable}; p227Unfilled={p227UnresolvedExtractionBlockerCount}"),
    new Check(
        "official-upsilon-route-lacks-extraction-normalization-and-projection",
        !officialGuSourcesProvideFixedScalarSourceOperator
            && !officialGuSourcesProvideFixedKappaOrInnerProductNormalization
            && !officialGuSourcesProvideUpsilonComponentExtractionTheorem
            && !officialGuSourcesProvideObserverSectorProjection
            && !officialGuSourcesProvideMassiveScalarProfile
            && !officialGuSourcesProvideQuarticSelfCouplingValue
            && p227UnresolvedExtractionBlockerCount == 5,
        $"operator={officialGuSourcesProvideFixedScalarSourceOperator}; normalization={officialGuSourcesProvideFixedKappaOrInnerProductNormalization}; componentExtraction={officialGuSourcesProvideUpsilonComponentExtractionTheorem}; projection={officialGuSourcesProvideObserverSectorProjection}; massiveProfile={officialGuSourcesProvideMassiveScalarProfile}; quartic={officialGuSourcesProvideQuarticSelfCouplingValue}; p227Unfilled={p227UnresolvedExtractionBlockerCount}"),
    new Check(
        "current-scalar-identity-operator-and-profile-remain-blocked",
        p187ScaffoldMaterialized
            && !p187HiggsSourceIdentityValidated
            && !p187PredictionAttemptAllowed
            && !p189CensusPromotable
            && !p189PredictionAttemptAllowed
            && p189ScalarFeatureEnvelopeCount == 0
            && p189BranchStableNonC0Count == 0
            && p189MassiveScalarProfileCount == 0
            && !p248CurrentRegistryHasScalarIdentityFeatures
            && !p248CurrentRegistryHasMassiveScalarProfile
            && p248NewHiggsScalarSourceStillRequired,
        $"p187Scaffold={p187ScaffoldMaterialized}; identityValidated={p187HiggsSourceIdentityValidated}; p189Promotable={p189CensusPromotable}; scalarEnvelopes={p189ScalarFeatureEnvelopeCount}; branchStableNonC0={p189BranchStableNonC0Count}; massiveProfiles={p189MassiveScalarProfileCount}; registryIdentity={p248CurrentRegistryHasScalarIdentityFeatures}; registryMassiveProfile={p248CurrentRegistryHasMassiveScalarProfile}; newSourceRequired={p248NewHiggsScalarSourceStillRequired}"),
    new Check(
        "quartic-self-coupling-and-numerical-loopholes-remain-nonpromotional",
        !p196CanPromoteHiggsFromPotentialOrSelfCoupling
            && !p199CanPromoteAnyHiggsScalarSourceLineage
            && !p207CanPromoteHiggsQuarticSelfCouplingSource
            && p207IntakeReadyFindingCount == 0
            && !p215CanPromoteTargetImpliedHiggsSelfCoupling
            && p223NumericalLeadPresent
            && !p223CanPromoteHiggsCasimirQuarticLead
            && p223BestFactor == "3/10"
            && p248HiggsScalarRepairabilityAuditPassed
            && !p248CurrentHiggsNumericalLeadPromotable
            && !p248HiggsScalarSourceRepairPossibleFromCurrentRegistry
            && !p248ThreeTenthsFactorDerivableFromCurrentScalarSource,
        $"p196Promote={p196CanPromoteHiggsFromPotentialOrSelfCoupling}; p199Promote={p199CanPromoteAnyHiggsScalarSourceLineage}; p207Promote={p207CanPromoteHiggsQuarticSelfCouplingSource}; p207IntakeReady={p207IntakeReadyFindingCount}; targetImplied={p215CanPromoteTargetImpliedHiggsSelfCoupling}; p223Lead={p223NumericalLeadPresent}; p223Promote={p223CanPromoteHiggsCasimirQuarticLead}; p223BestFactor={p223BestFactor}; p248NumericalPromote={p248CurrentHiggsNumericalLeadPromotable}; p248RepairPossible={p248HiggsScalarSourceRepairPossibleFromCurrentRegistry}; p248ThreeTenthsSource={p248ThreeTenthsFactorDerivableFromCurrentScalarSource}"),
    new Check(
        "quartic-proxy-code-only-repair-is-not-higgs-source",
        phase318DeferredImplementationGapRepairabilityAuditPassed
            && !phase318LaunchableCodeOnlyPredictionFixFound
            && phase318QuarticInteractionProxyDeferred
            && !phase318QuarticProxyImplementationPromotesHiggsMass
            && !phase318CanDeriveHiggsScalarSource,
        $"p318Passed={phase318DeferredImplementationGapRepairabilityAuditPassed}; launchableCodeFix={phase318LaunchableCodeOnlyPredictionFixFound}; quarticDeferred={phase318QuarticInteractionProxyDeferred}; quarticPromotes={phase318QuarticProxyImplementationPromotesHiggsMass}; codeCanDeriveHiggsSource={phase318CanDeriveHiggsScalarSource}"),
    new Check(
        "higgs-upsilon-route-does-not-fill-contracts",
        !officialGuSourcesPromoteHiggsMass
            && !higgsUpsilonRoutePromotesHiggsMass
            && !higgsUpsilonRouteCompletesBosonPredictions
            && !phase201AllRequiredLineagesPromotable
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"officialPromotesHiggs={officialGuSourcesPromoteHiggsMass}; routePromotesHiggs={higgsUpsilonRoutePromotesHiggsMass}; routeCompletes={higgsUpsilonRouteCompletesBosonPredictions}; allLineages={phase201AllRequiredLineagesPromotable}; canFillHiggs={canFillPhase201HiggsContract}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}"),
};

var higgsUpsilonScalarSourceBoundaryAuditPassed = checks.All(check => check.Passed)
    && higgsSourceRequirements.Count(requirement => requirement.Filled) == 1
    && !higgsUpsilonRoutePromotesHiggsMass
    && !higgsUpsilonRouteCompletesBosonPredictions
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;
var terminalStatus = higgsUpsilonScalarSourceBoundaryAuditPassed
    ? "higgs-upsilon-scalar-source-boundary-audit-no-promotable-source"
    : "higgs-upsilon-scalar-source-boundary-audit-review-required";

var result = new
{
    phaseId = "phase322-higgs-upsilon-scalar-source-boundary-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    higgsUpsilonScalarSourceBoundaryAuditPassed,
    officialGuSiteCurrentDraftReferencePresent,
    officialGuDraftAppendixMapsHiggsPotentialToUpsilonNorm,
    officialGuLectureFramesHiggsSectorAsGeometricProblem,
    officialGuLectureDiracSquareRootProgramForSecondOrderEquationsPresent,
    officialGuLectureShiabUpsilonActionStructurePresent,
    officialGuSourcesProvideFixedScalarSourceOperator,
    officialGuSourcesProvideFixedKappaOrInnerProductNormalization,
    officialGuSourcesProvideUpsilonComponentExtractionTheorem,
    officialGuSourcesProvideObserverSectorProjection,
    officialGuSourcesProvideMassiveScalarProfile,
    officialGuSourcesProvideQuarticSelfCouplingValue,
    officialGuSourcesPromoteHiggsMass,
    higgsUpsilonRoutePromotesHiggsMass,
    higgsUpsilonRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    officialSources = new[]
    {
        new ExternalSource(
            "geometric-unity-official-site",
            "Geometric Unity official site",
            officialGuSiteUrl,
            "The official site identifies the April 1, 2021 manuscript as the latest draft and links the 2013 Oxford lecture transcript."),
        new ExternalSource(
            "geometric-unity-draft-2021-appendix",
            "Geometric Unity Author's Working Draft v1.0, Appendix locations",
            officialGuDraftUrl,
            "The appendix locates the Higgs field and maps the Higgs potential to a Upsilon inner-product expression, but does not supply the scalar extraction rows required by Phase201."),
        new ExternalSource(
            "geometric-unity-2013-oxford-lecture",
            "2013 Oxford lecture transcript",
            officialGuLectureTranscriptUrl,
            "The lecture frames the Higgs sector as geometrically artificial in the Standard Model and motivates a Dirac-like square-root/second-order program, but does not fix the local scalar source operator and prediction row."),
    },
    higgsSourceRequirements,
    inheritedEvidence = new
    {
        phase187 = new
        {
            path = Phase187Path,
            p187ScaffoldMaterialized,
            p187HiggsSourceIdentityValidated,
            p187PredictionAttemptAllowed,
        },
        phase189 = new
        {
            path = Phase189Path,
            p189CensusPromotable,
            p189PredictionAttemptAllowed,
            p189ScalarFeatureEnvelopeCount,
            p189BranchStableNonC0Count,
            p189MassiveScalarProfileCount,
        },
        phase196 = new
        {
            path = Phase196Path,
            p196CanPromoteHiggsFromPotentialOrSelfCoupling,
        },
        phase199 = new
        {
            path = Phase199Path,
            p199CanPromoteAnyHiggsScalarSourceLineage,
        },
        phase207 = new
        {
            path = Phase207Path,
            p207CanPromoteHiggsQuarticSelfCouplingSource,
            p207IntakeReadyFindingCount,
        },
        phase215 = new
        {
            path = Phase215Path,
            p215CanPromoteTargetImpliedHiggsSelfCoupling,
        },
        phase223 = new
        {
            path = Phase223Path,
            p223NumericalLeadPresent,
            p223BestFactor,
            p223CanPromoteHiggsCasimirQuarticLead,
        },
        phase226 = new
        {
            path = Phase226Path,
            p226OfficialGuHiggsPotentialNotationObstructionCertified,
            p226OfficialGuHiggsPotentialNotationPromotable,
        },
        phase227 = new
        {
            path = Phase227Path,
            p227OfficialGuShiabUpsilonExtractionObstructionCertified,
            p227OfficialGuShiabUpsilonExtractionPromotable,
            p227UnresolvedExtractionBlockerCount,
        },
        phase248 = new
        {
            path = Phase248Path,
            p248HiggsScalarRepairabilityAuditPassed,
            p248CurrentHiggsNumericalLeadPromotable,
            p248HiggsScalarSourceRepairPossibleFromCurrentRegistry,
            p248ThreeTenthsFactorDerivableFromCurrentScalarSource,
            p248CurrentRegistryHasScalarIdentityFeatures,
            p248CurrentRegistryHasMassiveScalarProfile,
            p248NewHiggsScalarSourceStillRequired,
        },
        phase318 = new
        {
            path = Phase318Path,
            phase318DeferredImplementationGapRepairabilityAuditPassed,
            phase318LaunchableCodeOnlyPredictionFixFound,
            phase318QuarticInteractionProxyDeferred,
            phase318QuarticProxyImplementationPromotesHiggsMass,
            phase318CanDeriveHiggsScalarSource,
        },
        contracts = new
        {
            phase201Path = Phase201Path,
            phase213Path = Phase213Path,
            phase256Path = Phase256Path,
            phase201AllRequiredLineagesPromotable,
            wzMissingFieldCount,
            higgsMissingFieldCount,
            observedFieldExtractionFilledRequiredFieldCount,
            observedFieldExtractionContractPromotable,
        },
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
    },
    checks,
    decision = higgsUpsilonScalarSourceBoundaryAuditPassed
        ? "Do not promote Higgs mass from official GU Higgs/Upsilon notation, Dirac-square-root language, the current quartic proxy gap, or the P223 3/10 numerical lead. The route has source-location evidence but still lacks a fixed scalar source operator, scalar normalization, Upsilon component extraction theorem, observed-sector projection, Higgs identity envelope, massive scalar profile, and source-derived quartic/self-coupling value."
        : "Review the Higgs Upsilon scalar-source boundary audit before using it as boson prediction evidence.",
    nextRequiredArtifact = new[]
    {
        "A fixed GU scalar source/operator extracted from the Upsilon/Higgs-potential notation.",
        "A target-independent kappa or inner-product normalization for the scalar sector.",
        "A checked Upsilon component-extraction and observed-sector projection theorem for the Higgs row.",
        "A source-derived quartic/self-coupling or scalar excitation relation plus Higgs identity/profile/stability sidecars.",
        "A filled Phase201 Higgs source-lineage row that passes post-construction target comparison without observed-Higgs back-solving.",
    },
    sourceEvidence = new
    {
        phase187Path = Phase187Path,
        phase189Path = Phase189Path,
        phase196Path = Phase196Path,
        phase199Path = Phase199Path,
        phase201Path = Phase201Path,
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase215Path = Phase215Path,
        phase223Path = Phase223Path,
        phase226Path = Phase226Path,
        phase227Path = Phase227Path,
        phase248Path = Phase248Path,
        phase256Path = Phase256Path,
        phase318Path = Phase318Path,
        officialGuSiteUrl,
        officialGuDraftUrl,
        officialGuLectureTranscriptUrl,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "higgs_upsilon_scalar_source_boundary_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_upsilon_scalar_source_boundary_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"higgsUpsilonScalarSourceBoundaryAuditPassed={higgsUpsilonScalarSourceBoundaryAuditPassed}");
Console.WriteLine($"officialGuSourcesProvideFixedScalarSourceOperator={officialGuSourcesProvideFixedScalarSourceOperator}");
Console.WriteLine($"p207IntakeReadyFindingCount={p207IntakeReadyFindingCount}");
Console.WriteLine($"canFillPhase201HiggsContract={canFillPhase201HiggsContract}");

static IReadOnlyList<JsonElement> JsonArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
        ? value.EnumerateArray().ToArray()
        : Array.Empty<JsonElement>();

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

public sealed record Requirement(string RequirementId, string Detail, bool Filled);

public sealed record Check(string CheckId, bool Passed, string Detail);

public sealed record ExternalSource(string SourceId, string Title, string Url, string Finding);

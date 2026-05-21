using System.Text.Json;

const string DefaultOutputDir = "studies/phase329_seiberg_witten_monopole_electroweak_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase315Path = "studies/phase315_ucsd_dark_geometric_energy_source_audit_001/output/ucsd_dark_geometric_energy_source_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase324Path = "studies/phase324_custodial_rho_parameter_source_audit_001/output/custodial_rho_parameter_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE329_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase315 = JsonDocument.Parse(File.ReadAllText(Phase315Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase324 = JsonDocument.Parse(File.ReadAllText(Phase324Path));

const string ucsdDarkGeometricEnergyUrl = "https://theportal.group/2025-ucsd-astrophysics-and-cosmology-seminar-from-dark-to-geometric-energy/";
const string wittenMonopolesFourManifoldsUrl = "https://arxiv.org/abs/hep-th/9411102";
const string seibergWittenN2SymUrl = "https://arxiv.org/abs/hep-th/9407087";
const string seibergWittenN2QcdUrl = "https://arxiv.org/abs/hep-th/9408099";

const bool ucsdSeibergWittenLeadPresent = true;
const bool wittenMonopolesFourManifoldsReviewed = true;
const bool seibergWittenN2DualitySourcesReviewed = true;
const bool seibergWittenEquationsAreAbelianSpinCMonopoleSystem = true;
const bool seibergWittenTheoryProvidesFourManifoldInvariantRoute = true;
const bool seibergWittenTheoryProvidesN2SupersymmetricGaugeTheoryModuliRoute = true;
const bool seibergWittenTheoryHasMonopoleCondensationAndDualityLead = true;

const bool seibergWittenProvidesStandardModelElectroweakGaugeEmbedding = false;
const bool seibergWittenProvidesLowEnergyWeakMixingAngleSource = false;
const bool seibergWittenProvidesGaugeCouplingNormalization = false;
const bool seibergWittenProvidesTargetIndependentVevSource = false;
const bool seibergWittenProvidesSeparateWzSourceRows = false;
const bool seibergWittenProvidesWzRawAmplitudeGates = false;
const bool seibergWittenProvidesWzCommonBridgeGate = false;
const bool seibergWittenProvidesPhotonWzProjectionRows = false;
const bool seibergWittenProvidesNeutralMassMatrixDiagonalization = false;
const bool seibergWittenProvidesGuObservedFieldExtraction = false;
const bool seibergWittenProvidesHiggsScalarSourceOperator = false;
const bool seibergWittenProvidesHiggsMassiveScalarProfile = false;
const bool seibergWittenProvidesHiggsQuarticOrExcitationSource = false;
const bool seibergWittenProvidesGeVUnitNormalization = false;
const bool seibergWittenPromotesWzMasses = false;
const bool seibergWittenPromotesHiggsMass = false;
const bool seibergWittenCompletesBosonPredictions = false;
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

var ucsdDarkGeometricEnergySourceAuditPassed = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergySourceAuditPassed") is true;
var ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations") is true;
var ucsdDarkGeometricEnergyEditedTranscriptAvailable = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyEditedTranscriptAvailable") is true;
var ucsdDarkGeometricEnergyPromotesWzMasses = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesWzMasses") is true;
var ucsdDarkGeometricEnergyPromotesHiggsMass = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesHiggsMass") is true;

var neutralElectroweakMixingSourceAuditPassed = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true;
var lowEnergyHyperchargeSourcePresent = JsonBool(phase321.RootElement, "lowEnergyHyperchargeSourcePresent") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase321.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var neutralMixingCanFillPhase201WzContract = JsonBool(phase321.RootElement, "canFillPhase201WzContract") is true;

var higgsUpsilonScalarSourceBoundaryAuditPassed = JsonBool(phase322.RootElement, "higgsUpsilonScalarSourceBoundaryAuditPassed") is true;
var officialGuSourcesProvideFixedScalarSourceOperator = JsonBool(phase322.RootElement, "officialGuSourcesProvideFixedScalarSourceOperator") is true;
var officialGuSourcesProvideMassiveScalarProfile = JsonBool(phase322.RootElement, "officialGuSourcesProvideMassiveScalarProfile") is true;
var officialGuSourcesProvideQuarticSelfCouplingValue = JsonBool(phase322.RootElement, "officialGuSourcesProvideQuarticSelfCouplingValue") is true;
var officialGuSourcesPromoteHiggsMass = JsonBool(phase322.RootElement, "officialGuSourcesPromoteHiggsMass") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var officialPublicSourcesProvideCompleteMassEigenstateExtraction = JsonBool(phase323.RootElement, "officialPublicSourcesProvideCompleteMassEigenstateExtraction") is true;
var officialPublicSourcesProvideGeVUnitNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGeVUnitNormalization") is true;

var custodialRhoParameterSourceAuditPassed = JsonBool(phase324.RootElement, "custodialRhoParameterSourceAuditPassed") is true;
var rhoRelationProvidesAbsoluteWzScale = JsonBool(phase324.RootElement, "rhoRelationProvidesAbsoluteWzScale") is true;
var rhoRelationProvidesWeakMixingAngleSource = JsonBool(phase324.RootElement, "rhoRelationProvidesWeakMixingAngleSource") is true;
var rhoCanFillPhase201WzContract = JsonBool(phase324.RootElement, "canFillPhase201WzContract") is true;

var sourceRows = new[]
{
    new SourceRow(
        "ucsd-dark-geometric-energy-seiberg-witten-lead",
        ucsdDarkGeometricEnergyUrl,
        "public abstract source already materialized by Phase315",
        "The UCSD GU abstract mentions Seiberg-Witten monopole alignment as a dark/geometric-energy lead.",
        "The public abstract has no edited transcript or source-lineage row and Phase315 already marks it non-promotional."),
    new SourceRow(
        "witten-monopoles-and-four-manifolds",
        wittenMonopolesFourManifoldsUrl,
        "abstract and setup",
        "Witten's monopole-equation route defines four-manifold invariants by counting solutions of an abelian monopole equation.",
        "This is a topological/geometric gauge-theory invariant route, not a low-energy electroweak mass-source row."),
    new SourceRow(
        "seiberg-witten-n2-sym-duality",
        seibergWittenN2SymUrl,
        "abstract and source context",
        "The Seiberg-Witten N=2 Yang-Mills result is an exact duality/moduli-space framework involving monopoles.",
        "It does not supply the Standard Model SU(2)_L x U(1)_Y breaking inputs, observed field rows, or GeV normalization."),
    new SourceRow(
        "seiberg-witten-n2-qcd-duality",
        seibergWittenN2QcdUrl,
        "abstract and source context",
        "The N=2 QCD extension studies monopoles, duality, and chiral symmetry breaking in a supersymmetric gauge-theory setting.",
        "It is not a GU-local derivation of W/Z/H source rows, Higgs self-coupling, or the physical electroweak scale."),
};

var checks = new[]
{
    new Check(
        "seiberg-witten-lead-reviewed",
        ucsdDarkGeometricEnergySourceAuditPassed
            && ucsdSeibergWittenLeadPresent
            && ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations
            && !ucsdDarkGeometricEnergyEditedTranscriptAvailable
            && !ucsdDarkGeometricEnergyPromotesWzMasses
            && !ucsdDarkGeometricEnergyPromotesHiggsMass
            && sourceRows.Length == 4,
        $"p315Passed={ucsdDarkGeometricEnergySourceAuditPassed}; leadPresent={ucsdSeibergWittenLeadPresent}; p315MentionsSeibergWitten={ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations}; transcriptAvailable={ucsdDarkGeometricEnergyEditedTranscriptAvailable}; p315PromotesWz={ucsdDarkGeometricEnergyPromotesWzMasses}; p315PromotesHiggs={ucsdDarkGeometricEnergyPromotesHiggsMass}; sourceRowCount={sourceRows.Length}"),
    new Check(
        "primary-seiberg-witten-scope-classified",
        wittenMonopolesFourManifoldsReviewed
            && seibergWittenN2DualitySourcesReviewed
            && seibergWittenEquationsAreAbelianSpinCMonopoleSystem
            && seibergWittenTheoryProvidesFourManifoldInvariantRoute
            && seibergWittenTheoryProvidesN2SupersymmetricGaugeTheoryModuliRoute
            && seibergWittenTheoryHasMonopoleCondensationAndDualityLead
            && !seibergWittenProvidesStandardModelElectroweakGaugeEmbedding,
        $"wittenFourManifolds={wittenMonopolesFourManifoldsReviewed}; n2DualitySources={seibergWittenN2DualitySourcesReviewed}; abelianSpinC={seibergWittenEquationsAreAbelianSpinCMonopoleSystem}; fourManifoldInvariant={seibergWittenTheoryProvidesFourManifoldInvariantRoute}; n2Moduli={seibergWittenTheoryProvidesN2SupersymmetricGaugeTheoryModuliRoute}; monopoleCondensation={seibergWittenTheoryHasMonopoleCondensationAndDualityLead}; smElectroweakEmbedding={seibergWittenProvidesStandardModelElectroweakGaugeEmbedding}"),
    new Check(
        "seiberg-witten-does-not-supply-electroweak-bridge",
        neutralElectroweakMixingSourceAuditPassed
            && !lowEnergyHyperchargeSourcePresent
            && !officialDraftProvidesWeakMixingAngleSource
            && !neutralMixingCanFillPhase201WzContract
            && custodialRhoParameterSourceAuditPassed
            && !rhoRelationProvidesAbsoluteWzScale
            && !rhoRelationProvidesWeakMixingAngleSource
            && !rhoCanFillPhase201WzContract
            && !seibergWittenProvidesLowEnergyWeakMixingAngleSource
            && !seibergWittenProvidesGaugeCouplingNormalization
            && !seibergWittenProvidesTargetIndependentVevSource
            && !seibergWittenProvidesSeparateWzSourceRows
            && !seibergWittenProvidesPhotonWzProjectionRows
            && !seibergWittenProvidesNeutralMassMatrixDiagonalization,
        $"p321Passed={neutralElectroweakMixingSourceAuditPassed}; hyperchargeSource={lowEnergyHyperchargeSourcePresent}; weakAngle={officialDraftProvidesWeakMixingAngleSource}; p321CanFill={neutralMixingCanFillPhase201WzContract}; p324Passed={custodialRhoParameterSourceAuditPassed}; rhoAbsoluteScale={rhoRelationProvidesAbsoluteWzScale}; rhoWeakAngle={rhoRelationProvidesWeakMixingAngleSource}; swWeakAngle={seibergWittenProvidesLowEnergyWeakMixingAngleSource}; swCoupling={seibergWittenProvidesGaugeCouplingNormalization}; swVev={seibergWittenProvidesTargetIndependentVevSource}; swRows={seibergWittenProvidesSeparateWzSourceRows}"),
    new Check(
        "seiberg-witten-does-not-supply-higgs-source",
        higgsUpsilonScalarSourceBoundaryAuditPassed
            && !officialGuSourcesProvideFixedScalarSourceOperator
            && !officialGuSourcesProvideMassiveScalarProfile
            && !officialGuSourcesProvideQuarticSelfCouplingValue
            && !officialGuSourcesPromoteHiggsMass
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideCompleteMassEigenstateExtraction
            && !officialPublicSourcesProvideGeVUnitNormalization
            && !seibergWittenProvidesHiggsScalarSourceOperator
            && !seibergWittenProvidesHiggsMassiveScalarProfile
            && !seibergWittenProvidesHiggsQuarticOrExcitationSource,
        $"p322Passed={higgsUpsilonScalarSourceBoundaryAuditPassed}; scalarOperator={officialGuSourcesProvideFixedScalarSourceOperator}; massiveProfile={officialGuSourcesProvideMassiveScalarProfile}; quartic={officialGuSourcesProvideQuarticSelfCouplingValue}; p323Passed={coupledYangMillsHiggsMassExtractionAuditPassed}; quadraticExpansion={officialPublicSourcesProvideGaugeFixedQuadraticExpansion}; projectionRows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; completeExtraction={officialPublicSourcesProvideCompleteMassEigenstateExtraction}; swHiggsOperator={seibergWittenProvidesHiggsScalarSourceOperator}; swHiggsQuartic={seibergWittenProvidesHiggsQuarticOrExcitationSource}"),
    new Check(
        "source-lineage-contracts-remain-unfilled",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !seibergWittenProvidesWzRawAmplitudeGates
            && !seibergWittenProvidesWzCommonBridgeGate
            && !seibergWittenProvidesGuObservedFieldExtraction
            && !seibergWittenProvidesGeVUnitNormalization
            && !seibergWittenPromotesWzMasses
            && !seibergWittenPromotesHiggsMass
            && !seibergWittenCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidenceFound={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; swRawGates={seibergWittenProvidesWzRawAmplitudeGates}; swCommonBridge={seibergWittenProvidesWzCommonBridgeGate}; swObservedExtraction={seibergWittenProvidesGuObservedFieldExtraction}; swGeV={seibergWittenProvidesGeVUnitNormalization}; promotesWz={seibergWittenPromotesWzMasses}; promotesHiggs={seibergWittenPromotesHiggsMass}"),
};

var seibergWittenMonopoleElectroweakSourceAuditPassed = checks.All(check => check.Passed)
    && !seibergWittenProvidesStandardModelElectroweakGaugeEmbedding
    && !seibergWittenProvidesLowEnergyWeakMixingAngleSource
    && !seibergWittenProvidesGaugeCouplingNormalization
    && !seibergWittenProvidesTargetIndependentVevSource
    && !seibergWittenProvidesSeparateWzSourceRows
    && !seibergWittenProvidesWzRawAmplitudeGates
    && !seibergWittenProvidesWzCommonBridgeGate
    && !seibergWittenProvidesPhotonWzProjectionRows
    && !seibergWittenProvidesNeutralMassMatrixDiagonalization
    && !seibergWittenProvidesGuObservedFieldExtraction
    && !seibergWittenProvidesHiggsScalarSourceOperator
    && !seibergWittenProvidesHiggsMassiveScalarProfile
    && !seibergWittenProvidesHiggsQuarticOrExcitationSource
    && !seibergWittenProvidesGeVUnitNormalization
    && !seibergWittenPromotesWzMasses
    && !seibergWittenPromotesHiggsMass
    && !seibergWittenCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = seibergWittenMonopoleElectroweakSourceAuditPassed
    ? "seiberg-witten-monopole-electroweak-source-audit-topological-monopole-not-wzh-source"
    : "seiberg-witten-monopole-electroweak-source-audit-review-required";

var result = new
{
    phaseId = "phase329-seiberg-witten-monopole-electroweak-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    seibergWittenMonopoleElectroweakSourceAuditPassed,
    ucsdSeibergWittenLeadPresent,
    wittenMonopolesFourManifoldsReviewed,
    seibergWittenN2DualitySourcesReviewed,
    seibergWittenEquationsAreAbelianSpinCMonopoleSystem,
    seibergWittenTheoryProvidesFourManifoldInvariantRoute,
    seibergWittenTheoryProvidesN2SupersymmetricGaugeTheoryModuliRoute,
    seibergWittenTheoryHasMonopoleCondensationAndDualityLead,
    seibergWittenProvidesStandardModelElectroweakGaugeEmbedding,
    seibergWittenProvidesLowEnergyWeakMixingAngleSource,
    seibergWittenProvidesGaugeCouplingNormalization,
    seibergWittenProvidesTargetIndependentVevSource,
    seibergWittenProvidesSeparateWzSourceRows,
    seibergWittenProvidesWzRawAmplitudeGates,
    seibergWittenProvidesWzCommonBridgeGate,
    seibergWittenProvidesPhotonWzProjectionRows,
    seibergWittenProvidesNeutralMassMatrixDiagonalization,
    seibergWittenProvidesGuObservedFieldExtraction,
    seibergWittenProvidesHiggsScalarSourceOperator,
    seibergWittenProvidesHiggsMassiveScalarProfile,
    seibergWittenProvidesHiggsQuarticOrExcitationSource,
    seibergWittenProvidesGeVUnitNormalization,
    seibergWittenPromotesWzMasses,
    seibergWittenPromotesHiggsMass,
    seibergWittenCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    upstreamBoundaries = new
    {
        phase315 = new
        {
            ucsdDarkGeometricEnergySourceAuditPassed,
            ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations,
            ucsdDarkGeometricEnergyEditedTranscriptAvailable,
            ucsdDarkGeometricEnergyPromotesWzMasses,
            ucsdDarkGeometricEnergyPromotesHiggsMass,
        },
        phase321 = new
        {
            neutralElectroweakMixingSourceAuditPassed,
            lowEnergyHyperchargeSourcePresent,
            officialDraftProvidesWeakMixingAngleSource,
            neutralMixingCanFillPhase201WzContract,
        },
        phase322 = new
        {
            higgsUpsilonScalarSourceBoundaryAuditPassed,
            officialGuSourcesProvideFixedScalarSourceOperator,
            officialGuSourcesProvideMassiveScalarProfile,
            officialGuSourcesProvideQuarticSelfCouplingValue,
            officialGuSourcesPromoteHiggsMass,
        },
        phase323 = new
        {
            coupledYangMillsHiggsMassExtractionAuditPassed,
            officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
            officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
            officialPublicSourcesProvideCompleteMassEigenstateExtraction,
            officialPublicSourcesProvideGeVUnitNormalization,
        },
        phase324 = new
        {
            custodialRhoParameterSourceAuditPassed,
            rhoRelationProvidesAbsoluteWzScale,
            rhoRelationProvidesWeakMixingAngleSource,
            rhoCanFillPhase201WzContract,
        },
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
    decision = seibergWittenMonopoleElectroweakSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from the Seiberg-Witten monopole route. The route is a legitimate GU-adjacent geometry/gauge-theory lead from the UCSD abstract and primary Seiberg-Witten sources, but it supplies topological or N=2 supersymmetric moduli/duality structure rather than a GU-local electroweak embedding, weak angle, VEV, physical W/Z source rows, observed-field extraction, Higgs scalar-source/self-coupling lineage, or GeV normalization."
        : "Review the Seiberg-Witten source route before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem connecting any Seiberg-Witten/monopole alignment to the observed electroweak SU(2)_L x U(1)_Y breaking sector with target-independent W and Z source rows.",
        "A solved target-independent Higgs scalar-source/operator and self-coupling or excitation source compatible with observed-field extraction and Phase201/209/213 gates.",
    },
    sourceEvidence = new
    {
        ucsdDarkGeometricEnergyUrl,
        wittenMonopolesFourManifoldsUrl,
        seibergWittenN2SymUrl,
        seibergWittenN2QcdUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase256Path = Phase256Path,
        phase315Path = Phase315Path,
        phase321Path = Phase321Path,
        phase322Path = Phase322Path,
        phase323Path = Phase323Path,
        phase324Path = Phase324Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "seiberg_witten_monopole_electroweak_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "seiberg_witten_monopole_electroweak_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.seibergWittenMonopoleElectroweakSourceAuditPassed,
        result.ucsdSeibergWittenLeadPresent,
        result.wittenMonopolesFourManifoldsReviewed,
        result.seibergWittenN2DualitySourcesReviewed,
        result.seibergWittenEquationsAreAbelianSpinCMonopoleSystem,
        result.seibergWittenTheoryProvidesFourManifoldInvariantRoute,
        result.seibergWittenTheoryProvidesN2SupersymmetricGaugeTheoryModuliRoute,
        result.seibergWittenTheoryHasMonopoleCondensationAndDualityLead,
        result.seibergWittenProvidesStandardModelElectroweakGaugeEmbedding,
        result.seibergWittenProvidesLowEnergyWeakMixingAngleSource,
        result.seibergWittenProvidesGaugeCouplingNormalization,
        result.seibergWittenProvidesTargetIndependentVevSource,
        result.seibergWittenProvidesSeparateWzSourceRows,
        result.seibergWittenProvidesWzRawAmplitudeGates,
        result.seibergWittenProvidesWzCommonBridgeGate,
        result.seibergWittenProvidesPhotonWzProjectionRows,
        result.seibergWittenProvidesNeutralMassMatrixDiagonalization,
        result.seibergWittenProvidesGuObservedFieldExtraction,
        result.seibergWittenProvidesHiggsScalarSourceOperator,
        result.seibergWittenProvidesHiggsMassiveScalarProfile,
        result.seibergWittenProvidesHiggsQuarticOrExcitationSource,
        result.seibergWittenProvidesGeVUnitNormalization,
        result.seibergWittenPromotesWzMasses,
        result.seibergWittenPromotesHiggsMass,
        result.seibergWittenCompletesBosonPredictions,
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
Console.WriteLine($"seibergWittenMonopoleElectroweakSourceAuditPassed={seibergWittenMonopoleElectroweakSourceAuditPassed}");
Console.WriteLine($"seibergWittenPromotesWzMasses={seibergWittenPromotesWzMasses}");
Console.WriteLine($"seibergWittenPromotesHiggsMass={seibergWittenPromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);

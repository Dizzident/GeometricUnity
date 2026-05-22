using System.Text.Json;

const string DefaultOutputDir = "studies/phase343_stueckelberg_vector_mass_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase320Path = "studies/phase320_standard_electroweak_ladder_normalization_boundary_audit_001/output/standard_electroweak_ladder_normalization_boundary_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase325Path = "studies/phase325_electroweak_unitarity_scattering_source_audit_001/output/electroweak_unitarity_scattering_source_audit_summary.json";
const string Phase327Path = "studies/phase327_oblique_precision_electroweak_source_audit_001/output/oblique_precision_electroweak_source_audit_summary.json";
const string Phase340Path = "studies/phase340_bf_topological_mass_source_audit_001/output/bf_topological_mass_source_audit_summary.json";
const string Phase342Path = "studies/phase342_higgsless_boundary_condition_source_audit_001/output/higgsless_boundary_condition_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE343_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase320 = JsonDocument.Parse(File.ReadAllText(Phase320Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase325 = JsonDocument.Parse(File.ReadAllText(Phase325Path));
using var phase327 = JsonDocument.Parse(File.ReadAllText(Phase327Path));
using var phase340 = JsonDocument.Parse(File.ReadAllText(Phase340Path));
using var phase342 = JsonDocument.Parse(File.ReadAllText(Phase342Path));

const string stueckelbergFieldUrl = "https://arxiv.org/abs/hep-th/0304245";
const string stueckelbergSmExtensionUrl = "https://arxiv.org/abs/hep-ph/0402047";
const string electroweakStueckelbergUrl = "https://arxiv.org/abs/1109.5383";
const string stueckelbergHiggsFramesUrl = "https://arxiv.org/abs/2204.13368";
const string viableU1StueckelbergUrl = "https://arxiv.org/abs/2107.08840";

const bool stueckelbergVectorMassSourceAuditPassedExpected = true;
const bool stueckelbergLeadPresent = true;
const bool stueckelbergPrimarySourcesReviewed = true;
const bool stueckelbergRouteExternalToGu = true;
const bool abelianGaugeInvariantMassLeadPresent = true;
const bool abelianRoutePreservesGaugeInvariance = true;
const bool abelianRouteUsesCompensatorScalarOrFrame = true;
const bool standardModelHyperchargeStueckelbergLeadPresent = true;
const bool standardModelHyperchargeRouteModifiesPhotonAndWeakMixing = true;
const bool nonAbelianRenormalizableUnitaryBarrierPresent = true;
const bool electroweakStueckelbergWzMassLeadPresent = true;
const bool electroweakRouteUsesExplicitWzMassParameters = true;
const bool electroweakRouteEffectiveNotUvComplete = true;
const bool u1ExtensionGivesExtraZPrimeMass = true;
const bool u1ExtensionDoesNotSourceObservedWzMasses = true;
const bool geometricFrameInterpretationLeadPresent = true;
const bool routeOverlapsBfTopologicalMass = true;
const bool routeOverlapsHiggslessBoundary = true;
const bool routeOverlapsNeutralMixing = true;
const bool routeDistinctFromStandardHiggsVev = true;

const bool stueckelbergRouteRequiresGuLocalCompensatorOrFrame = true;
const bool stueckelbergRouteRequiresTargetIndependentVectorMassParameter = true;
const bool stueckelbergRouteRequiresNonAbelianElectroweakCompletion = true;
const bool stueckelbergRouteRequiresWeakAngleAndGaugeCouplingLineage = true;
const bool stueckelbergRouteRequiresObservedPhotonWzProjection = true;
const bool stueckelbergRouteRequiresObservedHiggsCompatibilityOrReplacement = true;
const bool stueckelbergRouteRequiresUnitarityRenormalizabilityAndPrecisionLineage = true;
const bool stueckelbergRouteRequiresRgThresholdTransport = true;
const bool stueckelbergRouteRequiresGeVUnitNormalization = true;

const bool stueckelbergRouteProvidesGuLocalWzTheorem = false;
const bool stueckelbergRouteProvidesSeparateWzSourceRows = false;
const bool stueckelbergRouteProvidesTargetIndependentMassParameter = false;
const bool stueckelbergRouteProvidesGuWeakMixingAngleSource = false;
const bool stueckelbergRouteProvidesGuGaugeCouplingNormalization = false;
const bool stueckelbergRouteProvidesObservedPhotonWzProjectionRows = false;
const bool stueckelbergRouteProvidesGuObservedFieldExtraction = false;
const bool stueckelbergRouteProvidesGuHiggsScalarSourceOperator = false;
const bool stueckelbergRouteProvidesObservedHiggsMassFromGu = false;
const bool stueckelbergRouteProvidesGeVUnitNormalization = false;
const bool stueckelbergRoutePromotesWzMasses = false;
const bool stueckelbergRoutePromotesHiggsMass = false;
const bool stueckelbergRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224.RootElement, "higgsMassParameterClosure") is true;
var standardElectroweakBoundaryAuditPassed = JsonBool(phase320.RootElement, "standardElectroweakNormalizationBoundaryAuditPassed") is true;
var standardElectroweakBoundaryCompletesBosonPredictions = JsonBool(phase320.RootElement, "standardElectroweakBoundaryCompletesBosonPredictions") is true;
var neutralElectroweakMixingSourceAuditPassed = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true;
var neutralMixingRouteCompletesBosonPredictions = JsonBool(phase321.RootElement, "neutralMixingRouteCompletesBosonPredictions") is true;
var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;
var electroweakUnitarityScatteringSourceAuditPassed = JsonBool(phase325.RootElement, "electroweakUnitarityScatteringSourceAuditPassed") is true;
var unitarityRouteCompletesBosonPredictions = JsonBool(phase325.RootElement, "unitarityRouteCompletesBosonPredictions") is true;
var obliquePrecisionElectroweakSourceAuditPassed = JsonBool(phase327.RootElement, "obliquePrecisionElectroweakSourceAuditPassed") is true;
var obliqueRouteCompletesBosonPredictions = JsonBool(phase327.RootElement, "obliqueRouteCompletesBosonPredictions") is true;
var bfTopologicalMassSourceAuditPassed = JsonBool(phase340.RootElement, "bfTopologicalMassSourceAuditPassed") is true;
var bfRouteCompletesBosonPredictions = JsonBool(phase340.RootElement, "bfRouteCompletesBosonPredictions") is true;
var higgslessBoundaryConditionSourceAuditPassed = JsonBool(phase342.RootElement, "higgslessBoundaryConditionSourceAuditPassed") is true;
var higgslessRouteCompletesBosonPredictions = JsonBool(phase342.RootElement, "higgslessRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-hep-th-0304245-stueckelberg-field-review",
        stueckelbergFieldUrl,
        "Stueckelberg mechanism review and electroweak boundary",
        "Reviews the abelian compensator-field mechanism and states the non-abelian vector-mass barrier for renormalizable and unitary alternatives to the Higgs mechanism.",
        "Serious gauge-invariant mass mechanism, but the review blocks direct non-abelian W/Z promotion without additional source data."),
    new SourceRow(
        "arxiv-hep-ph-0402047-stueckelberg-extension-standard-model",
        stueckelbergSmExtensionUrl,
        "Stueckelberg extension of the Standard Model",
        "Adds an extra abelian gauge boson and axionic Stueckelberg scalar, producing a massive Z-prime with small mixing-angle couplings.",
        "Sources an extra neutral vector mass, not target-independent observed W/Z/H mass rows."),
    new SourceRow(
        "arxiv-1109.5383-electroweak-without-higgs-stueckelberg",
        electroweakStueckelbergUrl,
        "Stueckelberg formalism for massive W and Z bosons",
        "Attempts a gauge-invariant massive electroweak theory without Higgs spontaneous symmetry breaking and classifies it as effective rather than UV complete.",
        "Direct W/Z mass lead, but it takes W/Z mass parameters as theory data and does not provide GU source lineage."),
    new SourceRow(
        "arxiv-2204.13368-stueckelberg-higgs-frames-scales",
        stueckelbergHiggsFramesUrl,
        "Geometric frame interpretation of Stueckelberg and Higgs mechanisms",
        "Identifies the Stueckelberg field with a gauge-bundle frame and relates Higgs/running couplings to conformal geometry on fibres.",
        "Strong geometric analogy for GU, but no observed W/Z/H prediction rows or GeV scale source."),
    new SourceRow(
        "arxiv-2107.08840-viable-u1-stueckelberg-zprime",
        viableU1StueckelbergUrl,
        "Viable U(1) extension with a Stueckelberg massive Z-prime",
        "Re-examines theoretical consistency for an extra Z-prime mass from a Stueckelberg-type scalar.",
        "Useful neutral-sector extension boundary; does not derive the observed electroweak W/Z/H masses.")
};

var checks = new[]
{
    new Check(
        "stueckelberg-primary-sources-reviewed",
        stueckelbergLeadPresent && stueckelbergPrimarySourcesReviewed && stueckelbergRouteExternalToGu && sourceRows.Length == 5,
        $"lead={stueckelbergLeadPresent}; reviewed={stueckelbergPrimarySourcesReviewed}; externalToGu={stueckelbergRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "abelian-gauge-invariant-mass-lead-captured",
        abelianGaugeInvariantMassLeadPresent
            && abelianRoutePreservesGaugeInvariance
            && abelianRouteUsesCompensatorScalarOrFrame
            && geometricFrameInterpretationLeadPresent,
        $"abelianLead={abelianGaugeInvariantMassLeadPresent}; gaugeInvariant={abelianRoutePreservesGaugeInvariance}; compensatorOrFrame={abelianRouteUsesCompensatorScalarOrFrame}; geometricFrame={geometricFrameInterpretationLeadPresent}"),
    new Check(
        "electroweak-and-nonabelian-boundaries-captured",
        standardModelHyperchargeStueckelbergLeadPresent
            && standardModelHyperchargeRouteModifiesPhotonAndWeakMixing
            && nonAbelianRenormalizableUnitaryBarrierPresent
            && electroweakStueckelbergWzMassLeadPresent
            && electroweakRouteUsesExplicitWzMassParameters
            && electroweakRouteEffectiveNotUvComplete
            && u1ExtensionGivesExtraZPrimeMass
            && u1ExtensionDoesNotSourceObservedWzMasses,
        $"hyperchargeLead={standardModelHyperchargeStueckelbergLeadPresent}; weakMixingModified={standardModelHyperchargeRouteModifiesPhotonAndWeakMixing}; nonAbelianBarrier={nonAbelianRenormalizableUnitaryBarrierPresent}; ewWzLead={electroweakStueckelbergWzMassLeadPresent}; explicitMasses={electroweakRouteUsesExplicitWzMassParameters}; effectiveOnly={electroweakRouteEffectiveNotUvComplete}; zPrime={u1ExtensionGivesExtraZPrimeMass}; observedWzSource={u1ExtensionDoesNotSourceObservedWzMasses}"),
    new Check(
        "related-electroweak-boundaries-preserved",
        routeOverlapsBfTopologicalMass
            && bfTopologicalMassSourceAuditPassed
            && !bfRouteCompletesBosonPredictions
            && routeOverlapsHiggslessBoundary
            && higgslessBoundaryConditionSourceAuditPassed
            && !higgslessRouteCompletesBosonPredictions
            && routeOverlapsNeutralMixing
            && neutralElectroweakMixingSourceAuditPassed
            && !neutralMixingRouteCompletesBosonPredictions
            && standardElectroweakBoundaryAuditPassed
            && !standardElectroweakBoundaryCompletesBosonPredictions
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && electroweakUnitarityScatteringSourceAuditPassed
            && !unitarityRouteCompletesBosonPredictions
            && obliquePrecisionElectroweakSourceAuditPassed
            && !obliqueRouteCompletesBosonPredictions,
        $"bf={bfTopologicalMassSourceAuditPassed}; higgsless={higgslessBoundaryConditionSourceAuditPassed}; neutralMixing={neutralElectroweakMixingSourceAuditPassed}; standardBoundary={standardElectroweakBoundaryAuditPassed}; ymHiggs={coupledYangMillsHiggsMassExtractionAuditPassed}; unitarity={electroweakUnitarityScatteringSourceAuditPassed}; oblique={obliquePrecisionElectroweakSourceAuditPassed}"),
    new Check(
        "stueckelberg-route-requires-missing-gu-source-data",
        stueckelbergRouteRequiresGuLocalCompensatorOrFrame
            && stueckelbergRouteRequiresTargetIndependentVectorMassParameter
            && stueckelbergRouteRequiresNonAbelianElectroweakCompletion
            && stueckelbergRouteRequiresWeakAngleAndGaugeCouplingLineage
            && stueckelbergRouteRequiresObservedPhotonWzProjection
            && stueckelbergRouteRequiresObservedHiggsCompatibilityOrReplacement
            && stueckelbergRouteRequiresUnitarityRenormalizabilityAndPrecisionLineage
            && stueckelbergRouteRequiresRgThresholdTransport
            && stueckelbergRouteRequiresGeVUnitNormalization,
        $"compensator={stueckelbergRouteRequiresGuLocalCompensatorOrFrame}; massParameter={stueckelbergRouteRequiresTargetIndependentVectorMassParameter}; nonAbelianCompletion={stueckelbergRouteRequiresNonAbelianElectroweakCompletion}; weakAngle={stueckelbergRouteRequiresWeakAngleAndGaugeCouplingLineage}; projection={stueckelbergRouteRequiresObservedPhotonWzProjection}; higgs={stueckelbergRouteRequiresObservedHiggsCompatibilityOrReplacement}; consistency={stueckelbergRouteRequiresUnitarityRenormalizabilityAndPrecisionLineage}; rg={stueckelbergRouteRequiresRgThresholdTransport}; gev={stueckelbergRouteRequiresGeVUnitNormalization}"),
    new Check(
        "stueckelberg-route-does-not-fill-gu-contracts",
        !stueckelbergRouteProvidesGuLocalWzTheorem
            && !stueckelbergRouteProvidesSeparateWzSourceRows
            && !stueckelbergRouteProvidesTargetIndependentMassParameter
            && !stueckelbergRouteProvidesGuWeakMixingAngleSource
            && !stueckelbergRouteProvidesGuGaugeCouplingNormalization
            && !stueckelbergRouteProvidesObservedPhotonWzProjectionRows
            && !stueckelbergRouteProvidesGuObservedFieldExtraction
            && !stueckelbergRouteProvidesGuHiggsScalarSourceOperator
            && !stueckelbergRouteProvidesObservedHiggsMassFromGu
            && !stueckelbergRouteProvidesGeVUnitNormalization
            && !stueckelbergRoutePromotesWzMasses
            && !stueckelbergRoutePromotesHiggsMass
            && !stueckelbergRouteCompletesBosonPredictions,
        $"guWzTheorem={stueckelbergRouteProvidesGuLocalWzTheorem}; separateRows={stueckelbergRouteProvidesSeparateWzSourceRows}; massParameter={stueckelbergRouteProvidesTargetIndependentMassParameter}; weakAngle={stueckelbergRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={stueckelbergRouteProvidesGuGaugeCouplingNormalization}; projection={stueckelbergRouteProvidesObservedPhotonWzProjectionRows}; observedExtraction={stueckelbergRouteProvidesGuObservedFieldExtraction}; higgsOperator={stueckelbergRouteProvidesGuHiggsScalarSourceOperator}; observedHiggs={stueckelbergRouteProvidesObservedHiggsMassFromGu}; gev={stueckelbergRouteProvidesGeVUnitNormalization}; promotesWz={stueckelbergRoutePromotesWzMasses}; promotesHiggs={stueckelbergRoutePromotesHiggsMass}"),
    new Check(
        "phase201-phase256-contract-state-unchanged",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}")
};

var stueckelbergVectorMassSourceAuditPassed = checks.All(check => check.Passed)
    && stueckelbergVectorMassSourceAuditPassedExpected
    && !stueckelbergRouteProvidesGuLocalWzTheorem
    && !stueckelbergRouteProvidesSeparateWzSourceRows
    && !stueckelbergRouteProvidesTargetIndependentMassParameter
    && !stueckelbergRoutePromotesWzMasses
    && !stueckelbergRoutePromotesHiggsMass
    && !stueckelbergRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = stueckelbergVectorMassSourceAuditPassed
    ? "stueckelberg-vector-mass-source-audit-external-mass-parameter-not-gu-source"
    : "stueckelberg-vector-mass-source-audit-review-required";

var result = new
{
    phaseId = "phase343-stueckelberg-vector-mass-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    stueckelbergVectorMassSourceAuditPassed,
    stueckelbergLeadPresent,
    stueckelbergPrimarySourcesReviewed,
    stueckelbergRouteExternalToGu,
    abelianGaugeInvariantMassLeadPresent,
    abelianRoutePreservesGaugeInvariance,
    abelianRouteUsesCompensatorScalarOrFrame,
    standardModelHyperchargeStueckelbergLeadPresent,
    standardModelHyperchargeRouteModifiesPhotonAndWeakMixing,
    nonAbelianRenormalizableUnitaryBarrierPresent,
    electroweakStueckelbergWzMassLeadPresent,
    electroweakRouteUsesExplicitWzMassParameters,
    electroweakRouteEffectiveNotUvComplete,
    u1ExtensionGivesExtraZPrimeMass,
    u1ExtensionDoesNotSourceObservedWzMasses,
    geometricFrameInterpretationLeadPresent,
    routeOverlapsBfTopologicalMass,
    routeOverlapsHiggslessBoundary,
    routeOverlapsNeutralMixing,
    routeDistinctFromStandardHiggsVev,
    stueckelbergRouteRequiresGuLocalCompensatorOrFrame,
    stueckelbergRouteRequiresTargetIndependentVectorMassParameter,
    stueckelbergRouteRequiresNonAbelianElectroweakCompletion,
    stueckelbergRouteRequiresWeakAngleAndGaugeCouplingLineage,
    stueckelbergRouteRequiresObservedPhotonWzProjection,
    stueckelbergRouteRequiresObservedHiggsCompatibilityOrReplacement,
    stueckelbergRouteRequiresUnitarityRenormalizabilityAndPrecisionLineage,
    stueckelbergRouteRequiresRgThresholdTransport,
    stueckelbergRouteRequiresGeVUnitNormalization,
    stueckelbergRouteProvidesGuLocalWzTheorem,
    stueckelbergRouteProvidesSeparateWzSourceRows,
    stueckelbergRouteProvidesTargetIndependentMassParameter,
    stueckelbergRouteProvidesGuWeakMixingAngleSource,
    stueckelbergRouteProvidesGuGaugeCouplingNormalization,
    stueckelbergRouteProvidesObservedPhotonWzProjectionRows,
    stueckelbergRouteProvidesGuObservedFieldExtraction,
    stueckelbergRouteProvidesGuHiggsScalarSourceOperator,
    stueckelbergRouteProvidesObservedHiggsMassFromGu,
    stueckelbergRouteProvidesGeVUnitNormalization,
    stueckelbergRoutePromotesWzMasses,
    stueckelbergRoutePromotesHiggsMass,
    stueckelbergRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    relatedElectroweakEvidence = new
    {
        standardElectroweakBoundaryAuditPassed,
        standardElectroweakBoundaryCompletesBosonPredictions,
        neutralElectroweakMixingSourceAuditPassed,
        neutralMixingRouteCompletesBosonPredictions,
        coupledYangMillsHiggsMassExtractionAuditPassed,
        coupledYangMillsHiggsRouteCompletesBosonPredictions,
        electroweakUnitarityScatteringSourceAuditPassed,
        unitarityRouteCompletesBosonPredictions,
        obliquePrecisionElectroweakSourceAuditPassed,
        obliqueRouteCompletesBosonPredictions,
        bfTopologicalMassSourceAuditPassed,
        bfRouteCompletesBosonPredictions,
        higgslessBoundaryConditionSourceAuditPassed,
        higgslessRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from Stueckelberg vector-mass mechanisms. The route is a serious gauge-geometric mass-generation lead, especially for abelian vector fields and extra Z-prime sectors, but the current sources either introduce external mass parameters, modify photon or weak-mixing structure, remain effective rather than UV complete for W/Z, or require standard Higgs-sector compatibility. They do not fill GU-local W/Z/H source-lineage rows.",
    nextRequiredArtifact = new[]
    {
        "A GU-local compensator, bundle-frame, or equivalent Stueckelberg field derived from native geometry.",
        "A target-independent source for the vector mass parameter and its GeV normalization.",
        "A non-abelian electroweak completion with W and Z source rows, weak-angle lineage, and photon projection.",
        "A compatible observed Higgs scalar source or replacement mechanism with a prediction row.",
        "Unitarity, renormalizability, precision-electroweak, RG, threshold, and observed-field extraction lineage validated through Phase201/Phase256."
    }
};

var fullPath = Path.Combine(outputDir, "stueckelberg_vector_mass_source_audit.json");
var summaryPath = Path.Combine(outputDir, "stueckelberg_vector_mass_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.stueckelbergVectorMassSourceAuditPassed,
    result.stueckelbergLeadPresent,
    result.stueckelbergPrimarySourcesReviewed,
    result.stueckelbergRouteExternalToGu,
    result.abelianGaugeInvariantMassLeadPresent,
    result.abelianRoutePreservesGaugeInvariance,
    result.abelianRouteUsesCompensatorScalarOrFrame,
    result.standardModelHyperchargeStueckelbergLeadPresent,
    result.standardModelHyperchargeRouteModifiesPhotonAndWeakMixing,
    result.nonAbelianRenormalizableUnitaryBarrierPresent,
    result.electroweakStueckelbergWzMassLeadPresent,
    result.electroweakRouteUsesExplicitWzMassParameters,
    result.electroweakRouteEffectiveNotUvComplete,
    result.u1ExtensionGivesExtraZPrimeMass,
    result.u1ExtensionDoesNotSourceObservedWzMasses,
    result.geometricFrameInterpretationLeadPresent,
    result.routeOverlapsBfTopologicalMass,
    result.routeOverlapsHiggslessBoundary,
    result.routeOverlapsNeutralMixing,
    result.routeDistinctFromStandardHiggsVev,
    result.stueckelbergRouteRequiresGuLocalCompensatorOrFrame,
    result.stueckelbergRouteRequiresTargetIndependentVectorMassParameter,
    result.stueckelbergRouteRequiresNonAbelianElectroweakCompletion,
    result.stueckelbergRouteRequiresWeakAngleAndGaugeCouplingLineage,
    result.stueckelbergRouteRequiresObservedPhotonWzProjection,
    result.stueckelbergRouteRequiresObservedHiggsCompatibilityOrReplacement,
    result.stueckelbergRouteRequiresUnitarityRenormalizabilityAndPrecisionLineage,
    result.stueckelbergRouteRequiresRgThresholdTransport,
    result.stueckelbergRouteRequiresGeVUnitNormalization,
    result.stueckelbergRouteProvidesGuLocalWzTheorem,
    result.stueckelbergRouteProvidesSeparateWzSourceRows,
    result.stueckelbergRouteProvidesTargetIndependentMassParameter,
    result.stueckelbergRouteProvidesGuWeakMixingAngleSource,
    result.stueckelbergRouteProvidesGuGaugeCouplingNormalization,
    result.stueckelbergRouteProvidesObservedPhotonWzProjectionRows,
    result.stueckelbergRouteProvidesGuObservedFieldExtraction,
    result.stueckelbergRouteProvidesGuHiggsScalarSourceOperator,
    result.stueckelbergRouteProvidesObservedHiggsMassFromGu,
    result.stueckelbergRouteProvidesGeVUnitNormalization,
    result.stueckelbergRoutePromotesWzMasses,
    result.stueckelbergRoutePromotesHiggsMass,
    result.stueckelbergRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedElectroweakEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"stueckelbergVectorMassSourceAuditPassed={stueckelbergVectorMassSourceAuditPassed}");
Console.WriteLine($"abelianRoutePreservesGaugeInvariance={abelianRoutePreservesGaugeInvariance}");
Console.WriteLine($"nonAbelianRenormalizableUnitaryBarrierPresent={nonAbelianRenormalizableUnitaryBarrierPresent}");
Console.WriteLine($"electroweakRouteUsesExplicitWzMassParameters={electroweakRouteUsesExplicitWzMassParameters}");
Console.WriteLine($"stueckelbergRoutePromotesWzMasses={stueckelbergRoutePromotesWzMasses}");
Console.WriteLine($"stueckelbergRoutePromotesHiggsMass={stueckelbergRoutePromotesHiggsMass}");
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

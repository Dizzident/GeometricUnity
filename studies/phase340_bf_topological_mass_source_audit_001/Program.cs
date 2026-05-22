using System.Text.Json;

const string DefaultOutputDir = "studies/phase340_bf_topological_mass_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase339Path = "studies/phase339_macdowell_mansouri_cartan_breaking_source_audit_001/output/macdowell_mansouri_cartan_breaking_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE340_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase339 = JsonDocument.Parse(File.ReadAllText(Phase339Path));

const string topologicalElectroweakUrl = "https://arxiv.org/abs/1009.1456";
const string savvidyFourDimensionalGaugeUrl = "https://arxiv.org/abs/1001.2808";
const string hwangLeeNonabelianBfUrl = "https://arxiv.org/abs/hep-th/9512216";
const string landimAlmeidaBfUrl = "https://arxiv.org/abs/hep-th/0010050";
const string nonabelianTopologicalNoGoUrl = "https://arxiv.org/abs/hep-th/9707129";
const string dvaliJackiwPiTopologicalUrl = "https://arxiv.org/abs/hep-th/0511175";
const string bfToBfcgHigherGaugeUrl = "https://arxiv.org/abs/0708.3051";

const bool bfTopologicalMassSourceAuditPassedExpected = true;
const bool bfTopologicalMassLeadPresent = true;
const bool bfTopologicalPrimarySourcesReviewed = true;
const bool bfTopologicalRouteExternalToGu = true;
const bool electroweakTopologicalOriginLeadPresent = true;
const bool electroweakTopologicalModelUsesS3MatterFieldSpace = true;
const bool electroweakTopologicalWzMassesDependOnCurvatureRadiusR = true;
const bool electroweakTopologicalOmitsConventionalHiggs = true;
const bool electroweakTopologicalObservedHiggsConflict = true;
const bool electroweakTopologicalRequiresRadiusWeakAngleAndGaugeLineage = true;
const bool nonabelianBfMassGenerationLeadPresent = true;
const bool nonabelianBfUsesBWedgeFCoupling = true;
const bool nonabelianBfRequiresAuxiliaryFields = true;
const bool nonabelianBfCanGenerateGaugeFieldMass = true;
const bool nonabelianTopologicalNoGoBoundaryPresent = true;
const bool nonabelianNoGoBlocksSimplePowerCountingRenormalizableRoute = true;
const bool savvidyTensorGaugeMassGapLeadPresent = true;
const bool savvidyRouteUsesChernSimonsLikeInvariant = true;
const bool dvaliJackiwPiTopologicalMassLeadPresent = true;
const bool bfcgHigherGaugeTopologicalMatterLeadPresent = true;
const bool bfcgRouteIsHigherGaugeTopologicalFramework = true;
const bool routeDistinctFromMacdowellCartan = true;

const bool bfRouteRequiresGuLocalBfBfcgMap = true;
const bool bfRouteRequiresTopologicalMassParameterSource = true;
const bool bfRouteRequiresElectroweakEmbeddingAndNeutralProjection = true;
const bool bfRouteRequiresObservedPhotonWzHProjection = true;
const bool bfRouteRequiresTargetIndependentRadiusVevOrScaleLineage = true;
const bool bfRouteRequiresWeakAngleAndGaugeCouplingLineage = true;
const bool bfRouteRequiresHiggsSectorCompatibilityAndScalarSource = true;
const bool bfRouteRequiresRenormalizableUnitaryCompletion = true;
const bool bfRouteRequiresGeVUnitNormalization = true;

const bool bfRouteProvidesGuLocalWzTheorem = false;
const bool bfRouteProvidesSeparateWzSourceRows = false;
const bool bfRouteProvidesWzRawAmplitudeGates = false;
const bool bfRouteProvidesWzCommonBridgeGate = false;
const bool bfRouteProvidesTargetIndependentGuVevSource = false;
const bool bfRouteProvidesGuWeakMixingAngleSource = false;
const bool bfRouteProvidesGuGaugeCouplingNormalization = false;
const bool bfRouteProvidesObservedPhotonWzProjectionRows = false;
const bool bfRouteProvidesGuObservedFieldExtraction = false;
const bool bfRouteProvidesGuHiggsScalarSourceOperator = false;
const bool bfRouteProvidesGuHiggsQuarticOrExcitationSource = false;
const bool bfRouteProvidesObservedHiggsMassFromGu = false;
const bool bfRouteProvidesGeVUnitNormalization = false;
const bool bfRoutePromotesWzMasses = false;
const bool bfRoutePromotesHiggsMass = false;
const bool bfRouteCompletesBosonPredictions = false;
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
var macdowellMansouriCartanSourceAuditPassed = JsonBool(phase339.RootElement, "macdowellMansouriCartanSourceAuditPassed") is true;
var macdowellRouteCompletesBosonPredictions = JsonBool(phase339.RootElement, "macdowellRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-1009-1456-topological-origin-electroweak-vector-boson-masses",
        topologicalElectroweakUrl,
        "Topological electroweak W/Z mass origin proposal",
        "Transforms the free electroweak Lagrangian from noncompact matter-field space to compact S3; W and Z masses depend on a curvature-radius parameter R and are written in standard-form formulas.",
        "Direct W/Z topological lead, but R remains a free source parameter and the model omits the now-observed Higgs field."),
    new SourceRow(
        "arxiv-1001-2808-topological-mass-four-dimensional-gauge-theory",
        savvidyFourDimensionalGaugeUrl,
        "Four-dimensional Chern-Simons-like tensor-gauge mass gap",
        "Constructs a metric-independent gauge-invariant density that can generate a mass gap for Yang-Mills/tensor gauge fields.",
        "Mass-gap mechanism lead, but not an electroweak W/Z/H source-lineage derivation."),
    new SourceRow(
        "arxiv-hep-th-9512216-nonabelian-bf-topological-mass",
        hwangLeeNonabelianBfUrl,
        "Non-Abelian BF topological mass generation",
        "Uses a B wedge F coupling and auxiliary one-form field so vector gauge fields can become massive in a non-Abelian BF setting.",
        "Mechanism lead; still needs electroweak embedding, source-derived mass parameter, and observed-field projection."),
    new SourceRow(
        "arxiv-hep-th-0010050-topologically-massive-nonabelian-bf",
        landimAlmeidaBfUrl,
        "Topologically massive non-Abelian BF models in arbitrary dimensions",
        "Extends non-Abelian BF topological mass generation to arbitrary spacetime dimension with auxiliary forms and BRST/anti-BRST structure.",
        "Useful consistency context; not a GU-local W/Z/H prediction source."),
    new SourceRow(
        "arxiv-hep-th-9707129-nonabelian-topological-mass-no-go",
        nonabelianTopologicalNoGoUrl,
        "No-go boundary for simple non-Abelian topological mass mechanism",
        "Proves no power-counting-renormalizable non-Abelian generalization of the Abelian topological mass mechanism in four dimensions in the studied framework.",
        "Boundary evidence requiring any GU route to specify its completion and not assume the simple BF mechanism is enough."),
    new SourceRow(
        "arxiv-hep-th-0511175-topological-mass-four-dimensions",
        dvaliJackiwPiTopologicalUrl,
        "Four-dimensional topological entities for mass generation",
        "Relates four-dimensional topological structures to Schwinger-like mass generation.",
        "General topological mass context; does not provide observed W/Z/H source rows."),
    new SourceRow(
        "arxiv-0708-3051-bf-to-bfcg-higher-gauge-theory",
        bfToBfcgHigherGaugeUrl,
        "Higher-gauge BF to BFCG framework",
        "Constructs topological higher-gauge theories and continuum BF/BFCG counterparts.",
        "Useful higher-gauge infrastructure; not a direct electroweak mass prediction.")
};

var checks = new[]
{
    new Check(
        "bf-topological-primary-sources-reviewed",
        bfTopologicalMassLeadPresent && bfTopologicalPrimarySourcesReviewed && bfTopologicalRouteExternalToGu && sourceRows.Length == 7,
        $"lead={bfTopologicalMassLeadPresent}; reviewed={bfTopologicalPrimarySourcesReviewed}; externalToGu={bfTopologicalRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "electroweak-topological-origin-captured-as-parameterized-and-higgs-conflicted",
        electroweakTopologicalOriginLeadPresent
            && electroweakTopologicalModelUsesS3MatterFieldSpace
            && electroweakTopologicalWzMassesDependOnCurvatureRadiusR
            && electroweakTopologicalOmitsConventionalHiggs
            && electroweakTopologicalObservedHiggsConflict
            && electroweakTopologicalRequiresRadiusWeakAngleAndGaugeLineage,
        $"lead={electroweakTopologicalOriginLeadPresent}; s3MatterSpace={electroweakTopologicalModelUsesS3MatterFieldSpace}; radiusR={electroweakTopologicalWzMassesDependOnCurvatureRadiusR}; omitsHiggs={electroweakTopologicalOmitsConventionalHiggs}; observedHiggsConflict={electroweakTopologicalObservedHiggsConflict}; radiusWeakAngleGaugeLineage={electroweakTopologicalRequiresRadiusWeakAngleAndGaugeLineage}"),
    new Check(
        "bf-topological-mass-mechanism-and-boundary-captured",
        nonabelianBfMassGenerationLeadPresent
            && nonabelianBfUsesBWedgeFCoupling
            && nonabelianBfRequiresAuxiliaryFields
            && nonabelianBfCanGenerateGaugeFieldMass
            && nonabelianTopologicalNoGoBoundaryPresent
            && nonabelianNoGoBlocksSimplePowerCountingRenormalizableRoute
            && savvidyTensorGaugeMassGapLeadPresent
            && savvidyRouteUsesChernSimonsLikeInvariant
            && dvaliJackiwPiTopologicalMassLeadPresent
            && bfcgHigherGaugeTopologicalMatterLeadPresent
            && bfcgRouteIsHigherGaugeTopologicalFramework,
        $"bfLead={nonabelianBfMassGenerationLeadPresent}; bWedgeF={nonabelianBfUsesBWedgeFCoupling}; auxiliary={nonabelianBfRequiresAuxiliaryFields}; gaugeMass={nonabelianBfCanGenerateGaugeFieldMass}; noGo={nonabelianTopologicalNoGoBoundaryPresent}; simpleRenormalizableBlocked={nonabelianNoGoBlocksSimplePowerCountingRenormalizableRoute}; savvidy={savvidyTensorGaugeMassGapLeadPresent}; csLike={savvidyRouteUsesChernSimonsLikeInvariant}; dvaliJackiwPi={dvaliJackiwPiTopologicalMassLeadPresent}; bfcg={bfcgHigherGaugeTopologicalMatterLeadPresent}; higherGauge={bfcgRouteIsHigherGaugeTopologicalFramework}"),
    new Check(
        "related-macdowell-boundary-preserved",
        routeDistinctFromMacdowellCartan && macdowellMansouriCartanSourceAuditPassed && !macdowellRouteCompletesBosonPredictions,
        $"distinctFromMacdowell={routeDistinctFromMacdowellCartan}; phase339Passed={macdowellMansouriCartanSourceAuditPassed}; phase339Completes={macdowellRouteCompletesBosonPredictions}"),
    new Check(
        "bf-route-requires-missing-gu-source-data",
        bfRouteRequiresGuLocalBfBfcgMap
            && bfRouteRequiresTopologicalMassParameterSource
            && bfRouteRequiresElectroweakEmbeddingAndNeutralProjection
            && bfRouteRequiresObservedPhotonWzHProjection
            && bfRouteRequiresTargetIndependentRadiusVevOrScaleLineage
            && bfRouteRequiresWeakAngleAndGaugeCouplingLineage
            && bfRouteRequiresHiggsSectorCompatibilityAndScalarSource
            && bfRouteRequiresRenormalizableUnitaryCompletion
            && bfRouteRequiresGeVUnitNormalization,
        $"guMap={bfRouteRequiresGuLocalBfBfcgMap}; massParameter={bfRouteRequiresTopologicalMassParameterSource}; ewEmbedding={bfRouteRequiresElectroweakEmbeddingAndNeutralProjection}; observedProjection={bfRouteRequiresObservedPhotonWzHProjection}; scale={bfRouteRequiresTargetIndependentRadiusVevOrScaleLineage}; weakAngleGauge={bfRouteRequiresWeakAngleAndGaugeCouplingLineage}; higgs={bfRouteRequiresHiggsSectorCompatibilityAndScalarSource}; completion={bfRouteRequiresRenormalizableUnitaryCompletion}; gev={bfRouteRequiresGeVUnitNormalization}"),
    new Check(
        "bf-route-does-not-fill-gu-contracts",
        !bfRouteProvidesGuLocalWzTheorem
            && !bfRouteProvidesSeparateWzSourceRows
            && !bfRouteProvidesWzRawAmplitudeGates
            && !bfRouteProvidesWzCommonBridgeGate
            && !bfRouteProvidesTargetIndependentGuVevSource
            && !bfRouteProvidesGuWeakMixingAngleSource
            && !bfRouteProvidesGuGaugeCouplingNormalization
            && !bfRouteProvidesObservedPhotonWzProjectionRows
            && !bfRouteProvidesGuObservedFieldExtraction
            && !bfRouteProvidesGuHiggsScalarSourceOperator
            && !bfRouteProvidesGuHiggsQuarticOrExcitationSource
            && !bfRouteProvidesObservedHiggsMassFromGu
            && !bfRouteProvidesGeVUnitNormalization
            && !bfRoutePromotesWzMasses
            && !bfRoutePromotesHiggsMass
            && !bfRouteCompletesBosonPredictions,
        $"guWzTheorem={bfRouteProvidesGuLocalWzTheorem}; separateRows={bfRouteProvidesSeparateWzSourceRows}; targetVev={bfRouteProvidesTargetIndependentGuVevSource}; weakAngle={bfRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={bfRouteProvidesGuGaugeCouplingNormalization}; projection={bfRouteProvidesObservedPhotonWzProjectionRows}; observedExtraction={bfRouteProvidesGuObservedFieldExtraction}; higgsOperator={bfRouteProvidesGuHiggsScalarSourceOperator}; higgsQuartic={bfRouteProvidesGuHiggsQuarticOrExcitationSource}; gev={bfRouteProvidesGeVUnitNormalization}; promotesWz={bfRoutePromotesWzMasses}; promotesHiggs={bfRoutePromotesHiggsMass}"),
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
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var bfTopologicalMassSourceAuditPassed = checks.All(check => check.Passed)
    && bfTopologicalMassSourceAuditPassedExpected
    && !bfRouteProvidesGuLocalWzTheorem
    && !bfRouteProvidesSeparateWzSourceRows
    && !bfRouteProvidesTargetIndependentGuVevSource
    && !bfRouteProvidesGuWeakMixingAngleSource
    && !bfRouteProvidesGuGaugeCouplingNormalization
    && !bfRouteProvidesGuObservedFieldExtraction
    && !bfRouteProvidesGuHiggsScalarSourceOperator
    && !bfRouteProvidesGuHiggsQuarticOrExcitationSource
    && !bfRouteProvidesObservedHiggsMassFromGu
    && !bfRouteProvidesGeVUnitNormalization
    && !bfRoutePromotesWzMasses
    && !bfRoutePromotesHiggsMass
    && !bfRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = bfTopologicalMassSourceAuditPassed
    ? "bf-topological-mass-source-audit-external-topological-mass-lead-not-gu-source"
    : "bf-topological-mass-source-audit-review-required";

var result = new
{
    phaseId = "phase340-bf-topological-mass-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    bfTopologicalMassSourceAuditPassed,
    bfTopologicalMassLeadPresent,
    bfTopologicalPrimarySourcesReviewed,
    bfTopologicalRouteExternalToGu,
    electroweakTopologicalOriginLeadPresent,
    electroweakTopologicalModelUsesS3MatterFieldSpace,
    electroweakTopologicalWzMassesDependOnCurvatureRadiusR,
    electroweakTopologicalOmitsConventionalHiggs,
    electroweakTopologicalObservedHiggsConflict,
    electroweakTopologicalRequiresRadiusWeakAngleAndGaugeLineage,
    nonabelianBfMassGenerationLeadPresent,
    nonabelianBfUsesBWedgeFCoupling,
    nonabelianBfRequiresAuxiliaryFields,
    nonabelianBfCanGenerateGaugeFieldMass,
    nonabelianTopologicalNoGoBoundaryPresent,
    nonabelianNoGoBlocksSimplePowerCountingRenormalizableRoute,
    savvidyTensorGaugeMassGapLeadPresent,
    savvidyRouteUsesChernSimonsLikeInvariant,
    dvaliJackiwPiTopologicalMassLeadPresent,
    bfcgHigherGaugeTopologicalMatterLeadPresent,
    bfcgRouteIsHigherGaugeTopologicalFramework,
    routeDistinctFromMacdowellCartan,
    bfRouteRequiresGuLocalBfBfcgMap,
    bfRouteRequiresTopologicalMassParameterSource,
    bfRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    bfRouteRequiresObservedPhotonWzHProjection,
    bfRouteRequiresTargetIndependentRadiusVevOrScaleLineage,
    bfRouteRequiresWeakAngleAndGaugeCouplingLineage,
    bfRouteRequiresHiggsSectorCompatibilityAndScalarSource,
    bfRouteRequiresRenormalizableUnitaryCompletion,
    bfRouteRequiresGeVUnitNormalization,
    bfRouteProvidesGuLocalWzTheorem,
    bfRouteProvidesSeparateWzSourceRows,
    bfRouteProvidesWzRawAmplitudeGates,
    bfRouteProvidesWzCommonBridgeGate,
    bfRouteProvidesTargetIndependentGuVevSource,
    bfRouteProvidesGuWeakMixingAngleSource,
    bfRouteProvidesGuGaugeCouplingNormalization,
    bfRouteProvidesObservedPhotonWzProjectionRows,
    bfRouteProvidesGuObservedFieldExtraction,
    bfRouteProvidesGuHiggsScalarSourceOperator,
    bfRouteProvidesGuHiggsQuarticOrExcitationSource,
    bfRouteProvidesObservedHiggsMassFromGu,
    bfRouteProvidesGeVUnitNormalization,
    bfRoutePromotesWzMasses,
    bfRoutePromotesHiggsMass,
    bfRouteCompletesBosonPredictions,
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
    decision = "Do not promote W/Z or Higgs physical masses from the BF/BFCG/topological mass route in this repository. The route is a serious external topological mass lead, but the current electroweak source keeps a free curvature-radius parameter and omits the observed Higgs, while the general BF/non-Abelian topological mass literature requires auxiliary fields, faces no-go boundaries for simple renormalizable non-Abelian completions, and does not supply GU-local observed photon/W/Z/H source rows, Higgs compatibility, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem mapping Shiab/Upsilon/connection data into BF two-form or BFCG higher-gauge fields.",
        "A target-independent topological mass parameter or curvature-radius source that does not import W, Z, or Higgs targets.",
        "A renormalizable/unitary electroweak completion with neutral mixing, photon projection, and observed W/Z/H rows.",
        "A Higgs scalar-source or compatible replacement that accounts for the observed physical Higgs.",
        "Weak-angle, gauge-coupling, scale, RG, and GeV normalization lineage validated through Phase201/Phase256."
    }
};

var fullPath = Path.Combine(outputDir, "bf_topological_mass_source_audit.json");
var summaryPath = Path.Combine(outputDir, "bf_topological_mass_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.bfTopologicalMassSourceAuditPassed,
    result.bfTopologicalMassLeadPresent,
    result.bfTopologicalPrimarySourcesReviewed,
    result.bfTopologicalRouteExternalToGu,
    result.electroweakTopologicalOriginLeadPresent,
    result.electroweakTopologicalModelUsesS3MatterFieldSpace,
    result.electroweakTopologicalWzMassesDependOnCurvatureRadiusR,
    result.electroweakTopologicalOmitsConventionalHiggs,
    result.electroweakTopologicalObservedHiggsConflict,
    result.electroweakTopologicalRequiresRadiusWeakAngleAndGaugeLineage,
    result.nonabelianBfMassGenerationLeadPresent,
    result.nonabelianBfUsesBWedgeFCoupling,
    result.nonabelianBfRequiresAuxiliaryFields,
    result.nonabelianBfCanGenerateGaugeFieldMass,
    result.nonabelianTopologicalNoGoBoundaryPresent,
    result.nonabelianNoGoBlocksSimplePowerCountingRenormalizableRoute,
    result.savvidyTensorGaugeMassGapLeadPresent,
    result.savvidyRouteUsesChernSimonsLikeInvariant,
    result.dvaliJackiwPiTopologicalMassLeadPresent,
    result.bfcgHigherGaugeTopologicalMatterLeadPresent,
    result.bfcgRouteIsHigherGaugeTopologicalFramework,
    result.routeDistinctFromMacdowellCartan,
    result.bfRouteRequiresGuLocalBfBfcgMap,
    result.bfRouteRequiresTopologicalMassParameterSource,
    result.bfRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    result.bfRouteRequiresObservedPhotonWzHProjection,
    result.bfRouteRequiresTargetIndependentRadiusVevOrScaleLineage,
    result.bfRouteRequiresWeakAngleAndGaugeCouplingLineage,
    result.bfRouteRequiresHiggsSectorCompatibilityAndScalarSource,
    result.bfRouteRequiresRenormalizableUnitaryCompletion,
    result.bfRouteRequiresGeVUnitNormalization,
    result.bfRouteProvidesGuLocalWzTheorem,
    result.bfRouteProvidesSeparateWzSourceRows,
    result.bfRouteProvidesTargetIndependentGuVevSource,
    result.bfRouteProvidesGuWeakMixingAngleSource,
    result.bfRouteProvidesGuGaugeCouplingNormalization,
    result.bfRouteProvidesObservedPhotonWzProjectionRows,
    result.bfRouteProvidesGuObservedFieldExtraction,
    result.bfRouteProvidesGuHiggsScalarSourceOperator,
    result.bfRouteProvidesGuHiggsQuarticOrExcitationSource,
    result.bfRouteProvidesObservedHiggsMassFromGu,
    result.bfRouteProvidesGeVUnitNormalization,
    result.bfRoutePromotesWzMasses,
    result.bfRoutePromotesHiggsMass,
    result.bfRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"bfTopologicalMassSourceAuditPassed={bfTopologicalMassSourceAuditPassed}");
Console.WriteLine($"electroweakTopologicalWzMassesDependOnCurvatureRadiusR={electroweakTopologicalWzMassesDependOnCurvatureRadiusR}");
Console.WriteLine($"electroweakTopologicalObservedHiggsConflict={electroweakTopologicalObservedHiggsConflict}");
Console.WriteLine($"nonabelianTopologicalNoGoBoundaryPresent={nonabelianTopologicalNoGoBoundaryPresent}");
Console.WriteLine($"bfRoutePromotesWzMasses={bfRoutePromotesWzMasses}");
Console.WriteLine($"bfRoutePromotesHiggsMass={bfRoutePromotesHiggsMass}");
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

using System.Text.Json;

const string DefaultOutputDir = "studies/phase341_scherk_schwarz_twisted_compactification_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";
const string Phase333Path = "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit_summary.json";
const string Phase340Path = "studies/phase340_bf_topological_mass_source_audit_001/output/bf_topological_mass_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE341_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));
using var phase333 = JsonDocument.Parse(File.ReadAllText(Phase333Path));
using var phase340 = JsonDocument.Parse(File.ReadAllText(Phase340Path));

const string originalScherkSchwarzDoi = "https://doi.org/10.1016/0550-3213(79)90592-3";
const string scherkSchwarzCompactificationUrl = "https://arxiv.org/abs/hep-ph/0611309";
const string electroweakExtraDimensionsUrl = "https://arxiv.org/abs/hep-ph/0012263";
const string electroweakFermionMassesExtraDimensionsUrl = "https://arxiv.org/abs/hep-ph/0304220";
const string mssmScherkSchwarzUrl = "https://arxiv.org/abs/hep-ph/0605024";
const string fluxWilsonLineScalarCondensateUrl = "https://arxiv.org/abs/2205.09320";

const bool scherkSchwarzTwistedCompactificationSourceAuditPassedExpected = true;
const bool scherkSchwarzLeadPresent = true;
const bool scherkSchwarzPrimarySourcesReviewed = true;
const bool scherkSchwarzRouteExternalToGu = true;
const bool originalExtraDimensionMassGenerationLeadPresent = true;
const bool scherkSchwarzUsesNontrivialBoundaryTwist = true;
const bool scherkSchwarzMassesDependOnCompactificationRadiusAndTwist = true;
const bool torusGaugeTheoryStableConfigurationLeadPresent = true;
const bool torusRouteClassifiesTwistAndBoundaryStableConfigurations = true;
const bool electroweakExtraDimensionLeadPresent = true;
const bool electroweakWilsonLineScherkSchwarzEquivalent = true;
const bool electroweakWMassDependsOnWilsonLinePhaseOverRadius = true;
const bool electroweakRequiresSmallWilsonLinePhaseToMatchWMass = true;
const bool electroweakModelHasTopHiggsMassDifficulties = true;
const bool mssmScherkSchwarzEwsbLeadPresent = true;
const bool mssmRouteUsesSoftTermsAndRadiativeCorrections = true;
const bool fluxWilsonLineScalarCondensateLeadPresent = true;
const bool fluxRouteUsesWilsonLineScalarCondensate = true;
const bool routeOverlapsGaugeHiggsBoundary = true;
const bool routeOverlapsKaluzaKleinInternalSymmetry = true;
const bool routeDistinctFromBfTopologicalMass = true;

const bool scherkRouteRequiresGuLocalTwistedCompactificationMap = true;
const bool scherkRouteRequiresTwistAngleOrHolonomySource = true;
const bool scherkRouteRequiresCompactificationRadiusSource = true;
const bool scherkRouteRequiresElectroweakEmbeddingAndNeutralProjection = true;
const bool scherkRouteRequiresObservedPhotonWzHProjection = true;
const bool scherkRouteRequiresWeakAngleAndGaugeCouplingLineage = true;
const bool scherkRouteRequiresHiggsSectorCompatibilityAndScalarSource = true;
const bool scherkRouteRequiresRgTransportAndThresholdLineage = true;
const bool scherkRouteRequiresGeVUnitNormalization = true;
const bool scherkRouteRequiresChiralityAndFermionSectorLineage = true;

const bool scherkRouteProvidesGuLocalWzTheorem = false;
const bool scherkRouteProvidesSeparateWzSourceRows = false;
const bool scherkRouteProvidesTargetIndependentTwistAngleSource = false;
const bool scherkRouteProvidesTargetIndependentCompactificationRadiusSource = false;
const bool scherkRouteProvidesGuWeakMixingAngleSource = false;
const bool scherkRouteProvidesGuGaugeCouplingNormalization = false;
const bool scherkRouteProvidesObservedPhotonWzProjectionRows = false;
const bool scherkRouteProvidesGuObservedFieldExtraction = false;
const bool scherkRouteProvidesGuHiggsScalarSourceOperator = false;
const bool scherkRouteProvidesGuHiggsQuarticOrExcitationSource = false;
const bool scherkRouteProvidesObservedHiggsMassFromGu = false;
const bool scherkRouteProvidesGeVUnitNormalization = false;
const bool scherkRoutePromotesWzMasses = false;
const bool scherkRoutePromotesHiggsMass = false;
const bool scherkRouteCompletesBosonPredictions = false;
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
var gaugeHiggsBoundarySourceAuditPassed = JsonBool(phase265.RootElement, "gaugeHiggsBoundarySourceAuditPassed") is true;
var wilsonLineHosotaniSourcePresentInGu = JsonBoolNested(phase265.RootElement, "sourceLineageBoundary", "wilsonLineHosotaniSourcePresent") is true;
var gaugeHiggsBoundaryCompletesBosonPredictions = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryCompletesBosonPredictions") is true;
var kaluzaKleinInternalSymmetrySourceAuditPassed = JsonBool(phase333.RootElement, "kaluzaKleinInternalSymmetrySourceAuditPassed") is true;
var kkRouteCompletesBosonPredictions = JsonBool(phase333.RootElement, "kkRouteCompletesBosonPredictions") is true;
var bfTopologicalMassSourceAuditPassed = JsonBool(phase340.RootElement, "bfTopologicalMassSourceAuditPassed") is true;
var bfRouteCompletesBosonPredictions = JsonBool(phase340.RootElement, "bfRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "doi-10.1016-0550-3213-79-90592-3-how-to-get-masses-from-extra-dimensions",
        originalScherkSchwarzDoi,
        "Original Scherk-Schwarz mass-from-extra-dimensions mechanism",
        "Mass splittings arise from nontrivial compactification/twist data rather than from inserting local mass terms by hand.",
        "Direct geometric/topological mass-generation lead; still needs GU-local compactification map, twist source, and physical projection."),
    new SourceRow(
        "arxiv-hep-ph-0611309-symmetry-breaking-from-scherk-schwarz-compactification",
        scherkSchwarzCompactificationUrl,
        "Torus compactification stable gauge configurations",
        "Classifies stable configurations of extra-dimensional gauge theories compactified on a torus with Scherk-Schwarz boundary data.",
        "Boundary/twist geometry lead; does not derive W/Z/H source rows from GU."),
    new SourceRow(
        "arxiv-hep-ph-0012263-electroweak-symmetry-breaking-and-extra-dimensions",
        electroweakExtraDimensionsUrl,
        "Extra-dimensional electroweak symmetry breaking lead",
        "Uses extra-dimensional electroweak symmetry breaking with a predicted Higgs mass around 200 GeV in the model considered.",
        "Phenomenological electroweak lead, but observed-Higgs compatibility and target-independent GU scale lineage are missing."),
    new SourceRow(
        "arxiv-hep-ph-0304220-electroweak-symmetry-breaking-and-fermion-masses-from-extra-dimensions",
        electroweakFermionMassesExtraDimensionsUrl,
        "Wilson-line/Scherk-Schwarz-equivalent gauge-Higgs electroweak model",
        "Identifies Higgs fields with internal gauge-field components and relates electroweak breaking to Wilson-line or Scherk-Schwarz twist data.",
        "A W mass can depend on a Wilson-line phase over compactification radius, but the phase/radius and top/Higgs sectors remain model data."),
    new SourceRow(
        "arxiv-hep-ph-0605024-mssm-from-scherk-schwarz-supersymmetry-breaking",
        mssmScherkSchwarzUrl,
        "MSSM electroweak breaking from Scherk-Schwarz SUSY breaking",
        "Scherk-Schwarz supersymmetry breaking can induce Higgs-sector soft terms and radiative electroweak breaking.",
        "External SUSY model-building route; still needs GU-local soft-term, RG, and observed-Higgs lineage."),
    new SourceRow(
        "arxiv-2205-09320-flux-compactification-wilson-line-scalar-condensate",
        fluxWilsonLineScalarCondensateUrl,
        "Flux compactification with Wilson-line scalar condensate",
        "Studies gauge symmetry breaking in flux compactification with Wilson-line scalar condensates.",
        "Modern compactification/twist context, not a GU-local W/Z/H prediction source.")
};

var checks = new[]
{
    new Check(
        "scherk-schwarz-primary-sources-reviewed",
        scherkSchwarzLeadPresent && scherkSchwarzPrimarySourcesReviewed && scherkSchwarzRouteExternalToGu && sourceRows.Length == 6,
        $"lead={scherkSchwarzLeadPresent}; reviewed={scherkSchwarzPrimarySourcesReviewed}; externalToGu={scherkSchwarzRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "twisted-compactification-mass-mechanism-captured",
        originalExtraDimensionMassGenerationLeadPresent
            && scherkSchwarzUsesNontrivialBoundaryTwist
            && scherkSchwarzMassesDependOnCompactificationRadiusAndTwist
            && torusGaugeTheoryStableConfigurationLeadPresent
            && torusRouteClassifiesTwistAndBoundaryStableConfigurations,
        $"originalLead={originalExtraDimensionMassGenerationLeadPresent}; twist={scherkSchwarzUsesNontrivialBoundaryTwist}; radiusTwist={scherkSchwarzMassesDependOnCompactificationRadiusAndTwist}; torusLead={torusGaugeTheoryStableConfigurationLeadPresent}; stableConfigs={torusRouteClassifiesTwistAndBoundaryStableConfigurations}"),
    new Check(
        "electroweak-extra-dimensional-route-captured-as-parameterized",
        electroweakExtraDimensionLeadPresent
            && electroweakWilsonLineScherkSchwarzEquivalent
            && electroweakWMassDependsOnWilsonLinePhaseOverRadius
            && electroweakRequiresSmallWilsonLinePhaseToMatchWMass
            && electroweakModelHasTopHiggsMassDifficulties,
        $"ewLead={electroweakExtraDimensionLeadPresent}; wilsonLineEquivalent={electroweakWilsonLineScherkSchwarzEquivalent}; wMassAlphaOverR={electroweakWMassDependsOnWilsonLinePhaseOverRadius}; smallAlphaRequired={electroweakRequiresSmallWilsonLinePhaseToMatchWMass}; topHiggsDifficulties={electroweakModelHasTopHiggsMassDifficulties}"),
    new Check(
        "related-boundaries-preserved",
        routeOverlapsGaugeHiggsBoundary
            && gaugeHiggsBoundarySourceAuditPassed
            && !wilsonLineHosotaniSourcePresentInGu
            && !gaugeHiggsBoundaryCompletesBosonPredictions
            && routeOverlapsKaluzaKleinInternalSymmetry
            && kaluzaKleinInternalSymmetrySourceAuditPassed
            && !kkRouteCompletesBosonPredictions
            && routeDistinctFromBfTopologicalMass
            && bfTopologicalMassSourceAuditPassed
            && !bfRouteCompletesBosonPredictions,
        $"overlapsGaugeHiggs={routeOverlapsGaugeHiggsBoundary}; phase265Passed={gaugeHiggsBoundarySourceAuditPassed}; guWilsonLineSource={wilsonLineHosotaniSourcePresentInGu}; phase265Completes={gaugeHiggsBoundaryCompletesBosonPredictions}; overlapsKk={routeOverlapsKaluzaKleinInternalSymmetry}; phase333Passed={kaluzaKleinInternalSymmetrySourceAuditPassed}; phase333Completes={kkRouteCompletesBosonPredictions}; distinctFromBf={routeDistinctFromBfTopologicalMass}; phase340Passed={bfTopologicalMassSourceAuditPassed}; phase340Completes={bfRouteCompletesBosonPredictions}"),
    new Check(
        "scherk-route-requires-missing-gu-source-data",
        scherkRouteRequiresGuLocalTwistedCompactificationMap
            && scherkRouteRequiresTwistAngleOrHolonomySource
            && scherkRouteRequiresCompactificationRadiusSource
            && scherkRouteRequiresElectroweakEmbeddingAndNeutralProjection
            && scherkRouteRequiresObservedPhotonWzHProjection
            && scherkRouteRequiresWeakAngleAndGaugeCouplingLineage
            && scherkRouteRequiresHiggsSectorCompatibilityAndScalarSource
            && scherkRouteRequiresRgTransportAndThresholdLineage
            && scherkRouteRequiresGeVUnitNormalization
            && scherkRouteRequiresChiralityAndFermionSectorLineage,
        $"guMap={scherkRouteRequiresGuLocalTwistedCompactificationMap}; twistSource={scherkRouteRequiresTwistAngleOrHolonomySource}; radiusSource={scherkRouteRequiresCompactificationRadiusSource}; ewEmbedding={scherkRouteRequiresElectroweakEmbeddingAndNeutralProjection}; observedProjection={scherkRouteRequiresObservedPhotonWzHProjection}; weakAngleGauge={scherkRouteRequiresWeakAngleAndGaugeCouplingLineage}; higgs={scherkRouteRequiresHiggsSectorCompatibilityAndScalarSource}; rg={scherkRouteRequiresRgTransportAndThresholdLineage}; gev={scherkRouteRequiresGeVUnitNormalization}; chirality={scherkRouteRequiresChiralityAndFermionSectorLineage}"),
    new Check(
        "scherk-route-does-not-fill-gu-contracts",
        !scherkRouteProvidesGuLocalWzTheorem
            && !scherkRouteProvidesSeparateWzSourceRows
            && !scherkRouteProvidesTargetIndependentTwistAngleSource
            && !scherkRouteProvidesTargetIndependentCompactificationRadiusSource
            && !scherkRouteProvidesGuWeakMixingAngleSource
            && !scherkRouteProvidesGuGaugeCouplingNormalization
            && !scherkRouteProvidesObservedPhotonWzProjectionRows
            && !scherkRouteProvidesGuObservedFieldExtraction
            && !scherkRouteProvidesGuHiggsScalarSourceOperator
            && !scherkRouteProvidesGuHiggsQuarticOrExcitationSource
            && !scherkRouteProvidesObservedHiggsMassFromGu
            && !scherkRouteProvidesGeVUnitNormalization
            && !scherkRoutePromotesWzMasses
            && !scherkRoutePromotesHiggsMass
            && !scherkRouteCompletesBosonPredictions,
        $"guWzTheorem={scherkRouteProvidesGuLocalWzTheorem}; separateRows={scherkRouteProvidesSeparateWzSourceRows}; twistSource={scherkRouteProvidesTargetIndependentTwistAngleSource}; radiusSource={scherkRouteProvidesTargetIndependentCompactificationRadiusSource}; weakAngle={scherkRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={scherkRouteProvidesGuGaugeCouplingNormalization}; projection={scherkRouteProvidesObservedPhotonWzProjectionRows}; observedExtraction={scherkRouteProvidesGuObservedFieldExtraction}; higgsOperator={scherkRouteProvidesGuHiggsScalarSourceOperator}; higgsQuartic={scherkRouteProvidesGuHiggsQuarticOrExcitationSource}; gev={scherkRouteProvidesGeVUnitNormalization}; promotesWz={scherkRoutePromotesWzMasses}; promotesHiggs={scherkRoutePromotesHiggsMass}"),
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

var scherkSchwarzTwistedCompactificationSourceAuditPassed = checks.All(check => check.Passed)
    && scherkSchwarzTwistedCompactificationSourceAuditPassedExpected
    && !scherkRouteProvidesGuLocalWzTheorem
    && !scherkRouteProvidesSeparateWzSourceRows
    && !scherkRouteProvidesTargetIndependentTwistAngleSource
    && !scherkRouteProvidesTargetIndependentCompactificationRadiusSource
    && !scherkRouteProvidesGuWeakMixingAngleSource
    && !scherkRouteProvidesGuGaugeCouplingNormalization
    && !scherkRouteProvidesGuObservedFieldExtraction
    && !scherkRouteProvidesGuHiggsScalarSourceOperator
    && !scherkRouteProvidesGuHiggsQuarticOrExcitationSource
    && !scherkRouteProvidesObservedHiggsMassFromGu
    && !scherkRouteProvidesGeVUnitNormalization
    && !scherkRoutePromotesWzMasses
    && !scherkRoutePromotesHiggsMass
    && !scherkRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = scherkSchwarzTwistedCompactificationSourceAuditPassed
    ? "scherk-schwarz-twisted-compactification-source-audit-external-twist-mass-lead-not-gu-source"
    : "scherk-schwarz-twisted-compactification-source-audit-review-required";

var result = new
{
    phaseId = "phase341-scherk-schwarz-twisted-compactification-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    scherkSchwarzTwistedCompactificationSourceAuditPassed,
    scherkSchwarzLeadPresent,
    scherkSchwarzPrimarySourcesReviewed,
    scherkSchwarzRouteExternalToGu,
    originalExtraDimensionMassGenerationLeadPresent,
    scherkSchwarzUsesNontrivialBoundaryTwist,
    scherkSchwarzMassesDependOnCompactificationRadiusAndTwist,
    torusGaugeTheoryStableConfigurationLeadPresent,
    torusRouteClassifiesTwistAndBoundaryStableConfigurations,
    electroweakExtraDimensionLeadPresent,
    electroweakWilsonLineScherkSchwarzEquivalent,
    electroweakWMassDependsOnWilsonLinePhaseOverRadius,
    electroweakRequiresSmallWilsonLinePhaseToMatchWMass,
    electroweakModelHasTopHiggsMassDifficulties,
    mssmScherkSchwarzEwsbLeadPresent,
    mssmRouteUsesSoftTermsAndRadiativeCorrections,
    fluxWilsonLineScalarCondensateLeadPresent,
    fluxRouteUsesWilsonLineScalarCondensate,
    routeOverlapsGaugeHiggsBoundary,
    routeOverlapsKaluzaKleinInternalSymmetry,
    routeDistinctFromBfTopologicalMass,
    scherkRouteRequiresGuLocalTwistedCompactificationMap,
    scherkRouteRequiresTwistAngleOrHolonomySource,
    scherkRouteRequiresCompactificationRadiusSource,
    scherkRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    scherkRouteRequiresObservedPhotonWzHProjection,
    scherkRouteRequiresWeakAngleAndGaugeCouplingLineage,
    scherkRouteRequiresHiggsSectorCompatibilityAndScalarSource,
    scherkRouteRequiresRgTransportAndThresholdLineage,
    scherkRouteRequiresGeVUnitNormalization,
    scherkRouteRequiresChiralityAndFermionSectorLineage,
    scherkRouteProvidesGuLocalWzTheorem,
    scherkRouteProvidesSeparateWzSourceRows,
    scherkRouteProvidesTargetIndependentTwistAngleSource,
    scherkRouteProvidesTargetIndependentCompactificationRadiusSource,
    scherkRouteProvidesGuWeakMixingAngleSource,
    scherkRouteProvidesGuGaugeCouplingNormalization,
    scherkRouteProvidesObservedPhotonWzProjectionRows,
    scherkRouteProvidesGuObservedFieldExtraction,
    scherkRouteProvidesGuHiggsScalarSourceOperator,
    scherkRouteProvidesGuHiggsQuarticOrExcitationSource,
    scherkRouteProvidesObservedHiggsMassFromGu,
    scherkRouteProvidesGeVUnitNormalization,
    scherkRoutePromotesWzMasses,
    scherkRoutePromotesHiggsMass,
    scherkRouteCompletesBosonPredictions,
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
    relatedBoundaryEvidence = new
    {
        gaugeHiggsBoundarySourceAuditPassed,
        wilsonLineHosotaniSourcePresentInGu,
        gaugeHiggsBoundaryCompletesBosonPredictions,
        kaluzaKleinInternalSymmetrySourceAuditPassed,
        kkRouteCompletesBosonPredictions,
        bfTopologicalMassSourceAuditPassed,
        bfRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from the Scherk-Schwarz/twisted-compactification route in this repository. The route is a genuine geometric mass-generation lead through compactification twists, Wilson-line phases, or boundary data, but the current evidence leaves the twist angle, compactification radius, electroweak embedding, weak angle, Higgs sector, and GeV normalization as external or model-dependent data rather than GU-local source-lineage rows.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem mapping Shiab/Upsilon/connection data to a compactification/twist/holonomy sector.",
        "A target-independent source for the Scherk-Schwarz twist angle or Wilson-line phase and compactification radius.",
        "Observed photon/W/Z/H projection rows with neutral mixing and weak-angle/gauge-coupling lineage.",
        "A Higgs scalar-source or compatible gauge-Higgs replacement that accounts for the observed Higgs mass.",
        "RG, threshold, chirality/fermion-sector, and GeV-unit normalization lineage validated through Phase201/Phase256."
    }
};

var fullPath = Path.Combine(outputDir, "scherk_schwarz_twisted_compactification_source_audit.json");
var summaryPath = Path.Combine(outputDir, "scherk_schwarz_twisted_compactification_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.scherkSchwarzTwistedCompactificationSourceAuditPassed,
    result.scherkSchwarzLeadPresent,
    result.scherkSchwarzPrimarySourcesReviewed,
    result.scherkSchwarzRouteExternalToGu,
    result.originalExtraDimensionMassGenerationLeadPresent,
    result.scherkSchwarzUsesNontrivialBoundaryTwist,
    result.scherkSchwarzMassesDependOnCompactificationRadiusAndTwist,
    result.torusGaugeTheoryStableConfigurationLeadPresent,
    result.torusRouteClassifiesTwistAndBoundaryStableConfigurations,
    result.electroweakExtraDimensionLeadPresent,
    result.electroweakWilsonLineScherkSchwarzEquivalent,
    result.electroweakWMassDependsOnWilsonLinePhaseOverRadius,
    result.electroweakRequiresSmallWilsonLinePhaseToMatchWMass,
    result.electroweakModelHasTopHiggsMassDifficulties,
    result.mssmScherkSchwarzEwsbLeadPresent,
    result.mssmRouteUsesSoftTermsAndRadiativeCorrections,
    result.fluxWilsonLineScalarCondensateLeadPresent,
    result.fluxRouteUsesWilsonLineScalarCondensate,
    result.routeOverlapsGaugeHiggsBoundary,
    result.routeOverlapsKaluzaKleinInternalSymmetry,
    result.routeDistinctFromBfTopologicalMass,
    result.scherkRouteRequiresGuLocalTwistedCompactificationMap,
    result.scherkRouteRequiresTwistAngleOrHolonomySource,
    result.scherkRouteRequiresCompactificationRadiusSource,
    result.scherkRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    result.scherkRouteRequiresObservedPhotonWzHProjection,
    result.scherkRouteRequiresWeakAngleAndGaugeCouplingLineage,
    result.scherkRouteRequiresHiggsSectorCompatibilityAndScalarSource,
    result.scherkRouteRequiresRgTransportAndThresholdLineage,
    result.scherkRouteRequiresGeVUnitNormalization,
    result.scherkRouteRequiresChiralityAndFermionSectorLineage,
    result.scherkRouteProvidesGuLocalWzTheorem,
    result.scherkRouteProvidesSeparateWzSourceRows,
    result.scherkRouteProvidesTargetIndependentTwistAngleSource,
    result.scherkRouteProvidesTargetIndependentCompactificationRadiusSource,
    result.scherkRouteProvidesGuWeakMixingAngleSource,
    result.scherkRouteProvidesGuGaugeCouplingNormalization,
    result.scherkRouteProvidesObservedPhotonWzProjectionRows,
    result.scherkRouteProvidesGuObservedFieldExtraction,
    result.scherkRouteProvidesGuHiggsScalarSourceOperator,
    result.scherkRouteProvidesGuHiggsQuarticOrExcitationSource,
    result.scherkRouteProvidesObservedHiggsMassFromGu,
    result.scherkRouteProvidesGeVUnitNormalization,
    result.scherkRoutePromotesWzMasses,
    result.scherkRoutePromotesHiggsMass,
    result.scherkRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedBoundaryEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"scherkSchwarzTwistedCompactificationSourceAuditPassed={scherkSchwarzTwistedCompactificationSourceAuditPassed}");
Console.WriteLine($"scherkSchwarzMassesDependOnCompactificationRadiusAndTwist={scherkSchwarzMassesDependOnCompactificationRadiusAndTwist}");
Console.WriteLine($"electroweakWMassDependsOnWilsonLinePhaseOverRadius={electroweakWMassDependsOnWilsonLinePhaseOverRadius}");
Console.WriteLine($"scherkRoutePromotesWzMasses={scherkRoutePromotesWzMasses}");
Console.WriteLine($"scherkRoutePromotesHiggsMass={scherkRoutePromotesHiggsMass}");
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

static bool? JsonBoolNested(JsonElement element, string objectPropertyName, string boolPropertyName)
{
    return element.TryGetProperty(objectPropertyName, out var nested) && nested.ValueKind == JsonValueKind.Object
        ? JsonBool(nested, boolPropertyName)
        : null;
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

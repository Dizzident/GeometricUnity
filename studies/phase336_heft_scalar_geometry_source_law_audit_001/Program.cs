using System.Text.Json;

const string DefaultOutputDir = "studies/phase336_heft_scalar_geometry_source_law_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase325Path = "studies/phase325_electroweak_unitarity_scattering_source_audit_001/output/electroweak_unitarity_scattering_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE336_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase325 = JsonDocument.Parse(File.ReadAllText(Phase325Path));

const string alonsoJenkinsManoharHeftGeometryUrl = "https://arxiv.org/abs/1511.00724";
const string cohenCraigLuSutherlandHeftUnitarityUrl = "https://arxiv.org/abs/2108.03240";
const string krauseBuchallaCataCelisKnechtEwchlUrl = "https://arxiv.org/abs/1907.07605";

const bool heftScalarGeometryLeadPresent = true;
const bool heftPrimarySourcesReviewed = true;
const bool heftRouteExternalToGu = true;
const bool heftDescribesHiggsAndGoldstonesAsScalarManifoldCoordinates = true;
const bool heftSMatrixInvariantUnderScalarFieldRedefinitions = true;
const bool heftCurvatureControlsHiggsAndLongitudinalGaugeObservables = true;
const bool heftGoldstonesBecomeLongitudinalWzModes = true;
const bool heftWzMassesDependOnVevGaugeCouplingsAndScalarMetric = true;
const bool heftHiggsMassDependsOnPotentialHessianAtVacuum = true;
const bool heftScalarManifoldCanBeFlatOrCurved = true;
const bool heftUnitarityGeometryLeadPresent = true;
const bool heftEwChiralLagrangianAssumesMassiveWzPattern = true;
const bool heftGeometricBridgeTemplateMaterialized = true;

const bool heftRequiresVacuumPoint = true;
const bool heftRequiresScalarManifoldMetric = true;
const bool heftRequiresGaugedIsometryKillingVectors = true;
const bool heftRequiresGaugeCouplings = true;
const bool heftRequiresPotentialAndHessian = true;
const bool heftRequiresCanonicalNormalization = true;
const bool heftRequiresObservedPhotonWzHiggsProjection = true;
const bool heftRequiresVevOrUnitNormalization = true;
const bool heftRequiresValidityOrUvCompletionScale = true;

const bool heftProvidesGuLocalWzTheorem = false;
const bool heftProvidesSeparateWzSourceRows = false;
const bool heftProvidesWzRawAmplitudeGates = false;
const bool heftProvidesWzCommonBridgeGate = false;
const bool heftProvidesTargetIndependentGuVevSource = false;
const bool heftProvidesGuWeakMixingAngleSource = false;
const bool heftProvidesGuGaugeCouplingNormalization = false;
const bool heftProvidesGuObservedFieldExtraction = false;
const bool heftProvidesGuHiggsScalarSourceOperator = false;
const bool heftProvidesGuHiggsQuarticOrExcitationSource = false;
const bool heftProvidesObservedHiggsMassFromGu = false;
const bool heftProvidesGeVUnitNormalization = false;
const bool heftPromotesWzMasses = false;
const bool heftPromotesHiggsMass = false;
const bool heftCompletesBosonPredictions = false;
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
var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var officialPublicSourcesProvideHiggsScalarSelfCouplingSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHiggsScalarSelfCouplingSource") is true;
var officialPublicSourcesProvideGeVUnitNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGeVUnitNormalization") is true;
var electroweakUnitarityScatteringSourceAuditPassed = JsonBool(phase325.RootElement, "electroweakUnitarityScatteringSourceAuditPassed") is true;
var unitarityRoutePromotesWzMasses = JsonBool(phase325.RootElement, "unitarityRoutePromotesWzMasses") is true;
var unitarityRoutePromotesHiggsMass = JsonBool(phase325.RootElement, "unitarityRoutePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-1511-00724-heft-scalar-geometry",
        alonsoJenkinsManoharHeftGeometryUrl,
        "HEFT scalar-manifold geometry",
        "Higgs and Goldstone fields are treated as coordinates on a scalar manifold; observables are invariant under field redefinitions and depend on scalar-manifold geometry.",
        "This supplies a geometric dependency template, not GU-local W/Z/H mass source rows."),
    new SourceRow(
        "arxiv-2108-03240-heft-unitarity-geometry",
        cohenCraigLuSutherlandHeftUnitarityUrl,
        "unitarity violation and HEFT geometry",
        "Goldstone/Higgs scattering amplitudes are expressed in basis-independent covariant quantities involving the potential and scalar-manifold curvature near the physical vacuum.",
        "This sharpens the need for a vacuum-local potential and curvature source, but does not predict W/Z/H masses by itself."),
    new SourceRow(
        "arxiv-1907-07605-electroweak-chiral-lagrangian",
        krauseBuchallaCataCelisKnechtEwchlUrl,
        "EW chiral Lagrangian with light Higgs-like scalar",
        "The EFT assumes the electroweak symmetry-breaking pattern leading to three Goldstone bosons, massive W/Z, and a Higgs-like scalar.",
        "This is a general EFT framework with assumed inputs, not a GU target-independent prediction."),
};

var geometricBridgeTemplate = new[]
{
    new TemplateRow(
        "wz-geometric-mass-source",
        "Gauge-boson mass matrix from scalar metric evaluated on gauged isometry/Killing directions at a chosen vacuum point.",
        "Requires GU-local scalar metric, vacuum point, gauged electroweak embedding, gauge-coupling normalization, and observed photon/W/Z projection."),
    new TemplateRow(
        "higgs-geometric-mass-source",
        "Higgs mass from the canonically normalized Hessian of the scalar potential at the same vacuum point.",
        "Requires GU-local potential or Upsilon norm expansion, scalar-source operator, quartic/excitation lineage, and GeV unit normalization."),
    new TemplateRow(
        "field-redefinition-invariant-acceptance",
        "Use curvature, covariant derivatives, Killing norms, and Hessians rather than coordinate-dependent scalar parametrizations.",
        "Requires a concrete GU artifact; external HEFT covariance alone is not source lineage."),
};

var checks = new[]
{
    new Check(
        "heft-primary-sources-reviewed",
        heftScalarGeometryLeadPresent
            && heftPrimarySourcesReviewed
            && heftRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={heftScalarGeometryLeadPresent}; primaryReviewed={heftPrimarySourcesReviewed}; externalToGu={heftRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "heft-geometric-dependency-template-captured",
        heftDescribesHiggsAndGoldstonesAsScalarManifoldCoordinates
            && heftSMatrixInvariantUnderScalarFieldRedefinitions
            && heftCurvatureControlsHiggsAndLongitudinalGaugeObservables
            && heftGoldstonesBecomeLongitudinalWzModes
            && heftWzMassesDependOnVevGaugeCouplingsAndScalarMetric
            && heftHiggsMassDependsOnPotentialHessianAtVacuum
            && heftGeometricBridgeTemplateMaterialized
            && geometricBridgeTemplate.Length == 3,
        $"coordinates={heftDescribesHiggsAndGoldstonesAsScalarManifoldCoordinates}; invariant={heftSMatrixInvariantUnderScalarFieldRedefinitions}; curvature={heftCurvatureControlsHiggsAndLongitudinalGaugeObservables}; goldstonesToWz={heftGoldstonesBecomeLongitudinalWzModes}; wzMetric={heftWzMassesDependOnVevGaugeCouplingsAndScalarMetric}; higgsHessian={heftHiggsMassDependsOnPotentialHessianAtVacuum}; templateRows={geometricBridgeTemplate.Length}"),
    new Check(
        "heft-requires-geometric-source-data-not-present-in-gu",
        heftRequiresVacuumPoint
            && heftRequiresScalarManifoldMetric
            && heftRequiresGaugedIsometryKillingVectors
            && heftRequiresGaugeCouplings
            && heftRequiresPotentialAndHessian
            && heftRequiresCanonicalNormalization
            && heftRequiresObservedPhotonWzHiggsProjection
            && heftRequiresVevOrUnitNormalization
            && heftRequiresValidityOrUvCompletionScale,
        $"vacuum={heftRequiresVacuumPoint}; metric={heftRequiresScalarManifoldMetric}; killing={heftRequiresGaugedIsometryKillingVectors}; couplings={heftRequiresGaugeCouplings}; potentialHessian={heftRequiresPotentialAndHessian}; canonical={heftRequiresCanonicalNormalization}; projection={heftRequiresObservedPhotonWzHiggsProjection}; units={heftRequiresVevOrUnitNormalization}; validity={heftRequiresValidityOrUvCompletionScale}"),
    new Check(
        "current-electroweak-parameter-closures-remain-failed",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure,
        $"p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}"),
    new Check(
        "current-gu-coupled-yang-mills-higgs-extraction-remains-unfilled",
        coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !officialPublicSourcesProvideGeVUnitNormalization,
        $"p323={coupledYangMillsHiggsMassExtractionAuditPassed}; vev={officialPublicSourcesProvideTargetIndependentVevSource}; quadratic={officialPublicSourcesProvideGaugeFixedQuadraticExpansion}; projection={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; higgsSelf={officialPublicSourcesProvideHiggsScalarSelfCouplingSource}; gev={officialPublicSourcesProvideGeVUnitNormalization}"),
    new Check(
        "heft-unitarity-geometry-is-not-mass-prediction",
        heftScalarManifoldCanBeFlatOrCurved
            && heftUnitarityGeometryLeadPresent
            && electroweakUnitarityScatteringSourceAuditPassed
            && !unitarityRoutePromotesWzMasses
            && !unitarityRoutePromotesHiggsMass,
        $"flatOrCurved={heftScalarManifoldCanBeFlatOrCurved}; unitarityGeometry={heftUnitarityGeometryLeadPresent}; p325={electroweakUnitarityScatteringSourceAuditPassed}; promotesWz={unitarityRoutePromotesWzMasses}; promotesHiggs={unitarityRoutePromotesHiggsMass}"),
    new Check(
        "heft-route-does-not-fill-wz-higgs-contracts",
        heftEwChiralLagrangianAssumesMassiveWzPattern
            && !heftProvidesGuLocalWzTheorem
            && !heftProvidesSeparateWzSourceRows
            && !heftProvidesWzRawAmplitudeGates
            && !heftProvidesWzCommonBridgeGate
            && !heftProvidesTargetIndependentGuVevSource
            && !heftProvidesGuWeakMixingAngleSource
            && !heftProvidesGuGaugeCouplingNormalization
            && !heftProvidesGuObservedFieldExtraction
            && !heftProvidesGuHiggsScalarSourceOperator
            && !heftProvidesGuHiggsQuarticOrExcitationSource
            && !heftProvidesObservedHiggsMassFromGu
            && !heftProvidesGeVUnitNormalization
            && !heftPromotesWzMasses
            && !heftPromotesHiggsMass
            && !heftCompletesBosonPredictions,
        $"ewChiralAssumesWz={heftEwChiralLagrangianAssumesMassiveWzPattern}; guWzTheorem={heftProvidesGuLocalWzTheorem}; separateRows={heftProvidesSeparateWzSourceRows}; targetVev={heftProvidesTargetIndependentGuVevSource}; weakAngle={heftProvidesGuWeakMixingAngleSource}; gaugeNorm={heftProvidesGuGaugeCouplingNormalization}; observedExtraction={heftProvidesGuObservedFieldExtraction}; higgsOperator={heftProvidesGuHiggsScalarSourceOperator}; higgsQuartic={heftProvidesGuHiggsQuarticOrExcitationSource}; gev={heftProvidesGeVUnitNormalization}; promotesWz={heftPromotesWzMasses}; promotesHiggs={heftPromotesHiggsMass}"),
    new Check(
        "phase201-phase256-contract-state-unchanged",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var heftScalarGeometrySourceLawAuditPassed = checks.All(check => check.Passed)
    && !heftProvidesGuLocalWzTheorem
    && !heftProvidesSeparateWzSourceRows
    && !heftProvidesTargetIndependentGuVevSource
    && !heftProvidesGuWeakMixingAngleSource
    && !heftProvidesGuGaugeCouplingNormalization
    && !heftProvidesGuObservedFieldExtraction
    && !heftProvidesGuHiggsScalarSourceOperator
    && !heftProvidesGuHiggsQuarticOrExcitationSource
    && !heftProvidesObservedHiggsMassFromGu
    && !heftProvidesGeVUnitNormalization
    && !heftPromotesWzMasses
    && !heftPromotesHiggsMass
    && !heftCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = heftScalarGeometrySourceLawAuditPassed
    ? "heft-scalar-geometry-source-law-audit-template-not-gu-source"
    : "heft-scalar-geometry-source-law-audit-review-required";

var result = new
{
    phaseId = "phase336-heft-scalar-geometry-source-law-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    heftScalarGeometrySourceLawAuditPassed,
    heftScalarGeometryLeadPresent,
    heftPrimarySourcesReviewed,
    heftRouteExternalToGu,
    heftDescribesHiggsAndGoldstonesAsScalarManifoldCoordinates,
    heftSMatrixInvariantUnderScalarFieldRedefinitions,
    heftCurvatureControlsHiggsAndLongitudinalGaugeObservables,
    heftGoldstonesBecomeLongitudinalWzModes,
    heftWzMassesDependOnVevGaugeCouplingsAndScalarMetric,
    heftHiggsMassDependsOnPotentialHessianAtVacuum,
    heftScalarManifoldCanBeFlatOrCurved,
    heftUnitarityGeometryLeadPresent,
    heftEwChiralLagrangianAssumesMassiveWzPattern,
    heftGeometricBridgeTemplateMaterialized,
    heftRequiresVacuumPoint,
    heftRequiresScalarManifoldMetric,
    heftRequiresGaugedIsometryKillingVectors,
    heftRequiresGaugeCouplings,
    heftRequiresPotentialAndHessian,
    heftRequiresCanonicalNormalization,
    heftRequiresObservedPhotonWzHiggsProjection,
    heftRequiresVevOrUnitNormalization,
    heftRequiresValidityOrUvCompletionScale,
    heftProvidesGuLocalWzTheorem,
    heftProvidesSeparateWzSourceRows,
    heftProvidesWzRawAmplitudeGates,
    heftProvidesWzCommonBridgeGate,
    heftProvidesTargetIndependentGuVevSource,
    heftProvidesGuWeakMixingAngleSource,
    heftProvidesGuGaugeCouplingNormalization,
    heftProvidesGuObservedFieldExtraction,
    heftProvidesGuHiggsScalarSourceOperator,
    heftProvidesGuHiggsQuarticOrExcitationSource,
    heftProvidesObservedHiggsMassFromGu,
    heftProvidesGeVUnitNormalization,
    heftPromotesWzMasses,
    heftPromotesHiggsMass,
    heftCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    geometricBridgeTemplate,
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
    decision = "Do not promote W/Z or Higgs masses from HEFT scalar-manifold geometry. HEFT gives the coordinate-invariant geometric form a successful bridge would need, but the repository still lacks GU-local scalar metric, vacuum point, gauged electroweak Killing directions, gauge-coupling and weak-angle normalization, potential Hessian or scalar-source lineage, observed photon/W/Z/H projection, and GeV unit normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local scalar-manifold metric and vacuum point for the observed electroweak sector.",
        "Gauged electroweak Killing/isometry directions with target-independent SU(2) and U(1) coupling normalization.",
        "A gauge-fixed quadratic expansion or equivalent covariant mass matrix with photon/W/Z/H projection rows.",
        "A GU Higgs scalar-source operator or potential Hessian/quartic/excitation lineage at the same vacuum.",
        "Physical-unit normalization and low-energy transport that do not import W, Z, or Higgs target values."
    }
};

var fullPath = Path.Combine(outputDir, "heft_scalar_geometry_source_law_audit.json");
var summaryPath = Path.Combine(outputDir, "heft_scalar_geometry_source_law_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.heftScalarGeometrySourceLawAuditPassed,
    result.heftScalarGeometryLeadPresent,
    result.heftPrimarySourcesReviewed,
    result.heftRouteExternalToGu,
    result.heftDescribesHiggsAndGoldstonesAsScalarManifoldCoordinates,
    result.heftSMatrixInvariantUnderScalarFieldRedefinitions,
    result.heftCurvatureControlsHiggsAndLongitudinalGaugeObservables,
    result.heftGoldstonesBecomeLongitudinalWzModes,
    result.heftWzMassesDependOnVevGaugeCouplingsAndScalarMetric,
    result.heftHiggsMassDependsOnPotentialHessianAtVacuum,
    result.heftScalarManifoldCanBeFlatOrCurved,
    result.heftUnitarityGeometryLeadPresent,
    result.heftEwChiralLagrangianAssumesMassiveWzPattern,
    result.heftGeometricBridgeTemplateMaterialized,
    result.heftRequiresVacuumPoint,
    result.heftRequiresScalarManifoldMetric,
    result.heftRequiresGaugedIsometryKillingVectors,
    result.heftRequiresGaugeCouplings,
    result.heftRequiresPotentialAndHessian,
    result.heftRequiresCanonicalNormalization,
    result.heftRequiresObservedPhotonWzHiggsProjection,
    result.heftRequiresVevOrUnitNormalization,
    result.heftRequiresValidityOrUvCompletionScale,
    result.heftProvidesGuLocalWzTheorem,
    result.heftProvidesSeparateWzSourceRows,
    result.heftProvidesTargetIndependentGuVevSource,
    result.heftProvidesGuWeakMixingAngleSource,
    result.heftProvidesGuGaugeCouplingNormalization,
    result.heftProvidesGuObservedFieldExtraction,
    result.heftProvidesGuHiggsScalarSourceOperator,
    result.heftProvidesGuHiggsQuarticOrExcitationSource,
    result.heftProvidesObservedHiggsMassFromGu,
    result.heftProvidesGeVUnitNormalization,
    result.heftPromotesWzMasses,
    result.heftPromotesHiggsMass,
    result.heftCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    templateRowCount = geometricBridgeTemplate.Length,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"heftScalarGeometrySourceLawAuditPassed={heftScalarGeometrySourceLawAuditPassed}");
Console.WriteLine($"heftGeometricBridgeTemplateMaterialized={heftGeometricBridgeTemplateMaterialized}");
Console.WriteLine($"heftPromotesWzMasses={heftPromotesWzMasses}");
Console.WriteLine($"heftPromotesHiggsMass={heftPromotesHiggsMass}");
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

public sealed record TemplateRow(string TemplateId, string GeometricStatement, string RequiredGuSourceLineage);

public sealed record Check(string CheckId, bool Passed, string Detail);

using System.Text.Json;

const string DefaultOutputDir = "studies/phase320_standard_electroweak_ladder_normalization_boundary_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase308Path = "studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json";
const string Phase309Path = "studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output/source_mode_vector_length_measure_normalization_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE320_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase308 = JsonDocument.Parse(File.ReadAllText(Phase308Path));
using var phase309 = JsonDocument.Parse(File.ReadAllText(Phase309Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));

const bool standardElectroweakReferenceMaterialized = true;
const string standardElectroweakReferenceId = "pdg-2025-electroweak-model-review";
const string standardElectroweakReferenceUrl = "https://pdg.lbl.gov/2025/reviews/rpp2024-rev-standard-model.pdf";
const bool standardWChargedLadderDefinitionAvailable = true;
const bool standardWChargedLadderNormalizationIsSqrtTwo = true;
const bool standardZRequiresNeutralSu2U1Mixing = true;
const bool standardPhotonZRotationRequiresWeakMixingAngle = true;
const bool standardTreeLevelMassesRequireGgv = true;
const bool standardTreeLevelHiggsRequiresLambdaAndV = true;
const bool standardElectroweakAlgebraProvidesPhase302ScaleLaw = false;
const bool standardElectroweakAlgebraProvidesSourceModeVectorLengthScale = false;
const bool standardElectroweakAlgebraJustifiesW416Z156Scale = false;
const bool standardElectroweakAlgebraPromotesDecoupledSelector = false;
const bool standardElectroweakBoundaryPromotesWzMasses = false;
const bool standardElectroweakBoundaryPromotesHiggsMass = false;
const bool standardElectroweakBoundaryCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase307ChargedOperator = JsonString(phase307.RootElement, "canonicalChargedOperator") ?? "";
var phase307ChargedOperatorMatchesStandardShape =
    phase307ChargedOperator.Contains("T", StringComparison.OrdinalIgnoreCase)
    && phase307ChargedOperator.Contains("axis0", StringComparison.OrdinalIgnoreCase)
    && phase307ChargedOperator.Contains("axis1", StringComparison.OrdinalIgnoreCase)
    && phase307ChargedOperator.Contains("sqrt(2)", StringComparison.OrdinalIgnoreCase);
var phase307NearPassPresent = JsonBool(phase307.RootElement, "numericalP302ScaledDecoupledNearPassPresent") is true;
var phase307TargetIndependentConstruction = JsonBool(phase307.RootElement, "targetObservablesUsedForConstruction") is false;
var phase307RawStableCommonSelectionLawCount = JsonInt(phase307.RootElement, "rawStableCommonSelectionLawCount") ?? -1;
var phase307P302ScaledStableCommonSelectionLawCount = JsonInt(phase307.RootElement, "p302ScaledStableCommonSelectionLawCount") ?? -1;
var phase307SelectionLawCanFillContractCount = JsonInt(phase307.RootElement, "selectionLawCanFillPhase201WzContractCount") ?? -1;
var phase307TheoremClaimed = JsonBool(phase307.RootElement, "theoremClaimed") is true;
var phase307CanFillContract = JsonBool(phase307.RootElement, "canFillPhase201WzContract") is true;

var phase308ScaleTransferAllowed = JsonBool(phase308.RootElement, "scaleTransferAllowed") is true;
var phase308CommonScaleApplicationTheoremPresent = JsonBool(phase308.RootElement, "p302CommonScaleApplicationTheoremPresent") is true;
var phase308ParticleLawApplicationTheoremPresent = JsonBool(phase308.RootElement, "p302ParticleLawApplicationTheoremPresent") is true;
var phase308P302WTotalScale = JsonDouble(phase308.RootElement, "p302WTotalScale") ?? double.NaN;
var phase308P302ZTotalScale = JsonDouble(phase308.RootElement, "p302ZTotalScale") ?? double.NaN;
var phase308CanFillContract = JsonBool(phase308.RootElement, "canFillPhase201WzContract") is true;

var phase309VectorLengthScaleIsCoordinateCount = JsonBool(phase309.RootElement, "vectorLengthScaleIsCoordinateCount") is true;
var phase309VectorLengthScaleIsNotL2MeasureConversion = JsonBool(phase309.RootElement, "vectorLengthScaleIsNotL2MeasureConversion") is true;
var phase309ApplicationTheoremPresent = JsonBool(phase309.RootElement, "sourceModeVectorLengthApplicationTheoremPresent") is true;
var phase309ScalePromotable = JsonBool(phase309.RootElement, "sourceModeVectorLengthScalePromotable") is true;
var phase309CanFillContract = JsonBool(phase309.RootElement, "canFillPhase201WzContract") is true;

var phase313ProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var phase313PhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var phase313WeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var phase313NeutralMassMatrixDiagonalization = JsonBool(phase313.RootElement, "officialDraftProvidesNeutralMassMatrixDiagonalization") is true;
var phase313ObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;
var phase313CanFillContract = JsonBool(phase313.RootElement, "canFillPhase201WzContract") is true;

var phase317MassMatrixAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var phase317SmDefinesChargedWCombination = JsonBool(phase317.RootElement, "smDefinesChargedWCombination") is true;
var phase317SmDefinesPhotonZWeinbergRotation = JsonBool(phase317.RootElement, "smDefinesPhotonZWeinbergRotation") is true;
var phase317SmMassMatrixProvidesExternalDependencyMap = JsonBool(phase317.RootElement, "smMassMatrixProvidesExternalDependencyMap") is true;
var phase317SmMassMatrixPromotesWz = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var phase317SmMassMatrixPromotesHiggs = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var phase317SmMassMatrixCompletes = JsonBool(phase317.RootElement, "smMassMatrixCompletesBosonPredictions") is true;
var phase317ContractImpact = phase317.RootElement.GetProperty("contractImpact");
var phase317CanFillWz = JsonBool(phase317ContractImpact, "canFillPhase201WzContract") is true;
var phase317CanFillHiggs = JsonBool(phase317ContractImpact, "canFillPhase201HiggsContract") is true;
var phase317CanFillObserved = JsonBool(phase317ContractImpact, "canFillPhase256ObservedFieldExtractionContract") is true;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var checks = new[]
{
    new Check(
        "standard-electroweak-reference-boundary-materialized",
        standardElectroweakReferenceMaterialized
            && standardWChargedLadderDefinitionAvailable
            && standardWChargedLadderNormalizationIsSqrtTwo
            && standardZRequiresNeutralSu2U1Mixing
            && standardPhotonZRotationRequiresWeakMixingAngle
            && standardTreeLevelMassesRequireGgv
            && standardTreeLevelHiggsRequiresLambdaAndV,
        $"referenceId={standardElectroweakReferenceId}; chargedW={standardWChargedLadderDefinitionAvailable}; sqrt2={standardWChargedLadderNormalizationIsSqrtTwo}; zMixing={standardZRequiresNeutralSu2U1Mixing}; weakAngle={standardPhotonZRotationRequiresWeakMixingAngle}; ggv={standardTreeLevelMassesRequireGgv}; lambdaV={standardTreeLevelHiggsRequiresLambdaAndV}"),
    new Check(
        "phase307-charged-ladder-shape-is-only-w-like",
        phase307ChargedOperatorMatchesStandardShape
            && phase317MassMatrixAuditPassed
            && phase317SmDefinesChargedWCombination
            && phase307TargetIndependentConstruction
            && phase307NearPassPresent
            && phase307RawStableCommonSelectionLawCount == 0
            && phase307P302ScaledStableCommonSelectionLawCount > 0
            && phase307SelectionLawCanFillContractCount == 0
            && !phase307TheoremClaimed
            && !phase307CanFillContract,
        $"chargedOperator={phase307ChargedOperator}; smChargedW={phase317SmDefinesChargedWCombination}; targetIndependentConstruction={phase307TargetIndependentConstruction}; nearPass={phase307NearPassPresent}; rawStableCommonSelectionLawCount={phase307RawStableCommonSelectionLawCount}; p302ScaledStableCommonSelectionLawCount={phase307P302ScaledStableCommonSelectionLawCount}; theoremClaimed={phase307TheoremClaimed}; canFill={phase307CanFillContract}"),
    new Check(
        "standard-z-boundary-requires-missing-observed-neutral-map",
        phase313ProjectionMapAuditPassed
            && phase317SmDefinesPhotonZWeinbergRotation
            && !phase313PhotonZWeinbergRotation
            && !phase313WeakMixingAngleSource
            && !phase313NeutralMassMatrixDiagonalization
            && !phase313ObservedElectroweakGaugeEmbedding
            && !phase313CanFillContract,
        $"phase313Passed={phase313ProjectionMapAuditPassed}; smPhotonZ={phase317SmDefinesPhotonZWeinbergRotation}; draftPhotonZ={phase313PhotonZWeinbergRotation}; weakMixingAngleSource={phase313WeakMixingAngleSource}; neutralMassMatrixDiagonalization={phase313NeutralMassMatrixDiagonalization}; observedEmbedding={phase313ObservedElectroweakGaugeEmbedding}; canFill={phase313CanFillContract}"),
    new Check(
        "phase302-scale-is-not-standard-electroweak-normalization",
        !standardElectroweakAlgebraProvidesPhase302ScaleLaw
            && !standardElectroweakAlgebraProvidesSourceModeVectorLengthScale
            && !standardElectroweakAlgebraJustifiesW416Z156Scale
            && phase308P302WTotalScale == 416.0
            && phase308P302ZTotalScale == 156.0
            && !phase308CommonScaleApplicationTheoremPresent
            && !phase308ParticleLawApplicationTheoremPresent
            && !phase308ScaleTransferAllowed
            && !phase308CanFillContract
            && phase309VectorLengthScaleIsCoordinateCount
            && phase309VectorLengthScaleIsNotL2MeasureConversion
            && !phase309ApplicationTheoremPresent
            && !phase309ScalePromotable
            && !phase309CanFillContract,
        $"standardPhase302Scale={standardElectroweakAlgebraProvidesPhase302ScaleLaw}; p302WTotalScale={phase308P302WTotalScale:R}; p302ZTotalScale={phase308P302ZTotalScale:R}; commonScaleTheorem={phase308CommonScaleApplicationTheoremPresent}; particleLawTheorem={phase308ParticleLawApplicationTheoremPresent}; transferAllowed={phase308ScaleTransferAllowed}; vectorLengthCoordinateCount={phase309VectorLengthScaleIsCoordinateCount}; vectorLengthNotL2={phase309VectorLengthScaleIsNotL2MeasureConversion}; phase309ScalePromotable={phase309ScalePromotable}"),
    new Check(
        "standard-model-mass-matrix-remains-external-dependency-map",
        phase317MassMatrixAuditPassed
            && phase317SmMassMatrixProvidesExternalDependencyMap
            && !phase317SmMassMatrixPromotesWz
            && !phase317SmMassMatrixPromotesHiggs
            && !phase317SmMassMatrixCompletes
            && !phase317CanFillWz
            && !phase317CanFillHiggs
            && !phase317CanFillObserved,
        $"phase317Passed={phase317MassMatrixAuditPassed}; externalDependencyMap={phase317SmMassMatrixProvidesExternalDependencyMap}; promotesWz={phase317SmMassMatrixPromotesWz}; promotesHiggs={phase317SmMassMatrixPromotesHiggs}; completes={phase317SmMassMatrixCompletes}; canFillWz={phase317CanFillWz}; canFillHiggs={phase317CanFillHiggs}; canFillObserved={phase317CanFillObserved}"),
    new Check(
        "source-contracts-remain-unfilled-after-standard-boundary",
        !standardElectroweakBoundaryPromotesWzMasses
            && !standardElectroweakBoundaryPromotesHiggsMass
            && !standardElectroweakBoundaryCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionPromotable,
        $"promotesWz={standardElectroweakBoundaryPromotesWzMasses}; promotesHiggs={standardElectroweakBoundaryPromotesHiggsMass}; completes={standardElectroweakBoundaryCompletesBosonPredictions}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}; allLineages={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}"),
};

var standardElectroweakNormalizationBoundaryAuditPassed = checks.All(check => check.Passed)
    && !standardElectroweakAlgebraPromotesDecoupledSelector
    && !standardElectroweakBoundaryPromotesWzMasses
    && !standardElectroweakBoundaryPromotesHiggsMass
    && !standardElectroweakBoundaryCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;
var terminalStatus = standardElectroweakNormalizationBoundaryAuditPassed
    ? "standard-electroweak-ladder-normalization-boundary-audit-no-promotion"
    : "standard-electroweak-ladder-normalization-boundary-audit-review-required";

var result = new
{
    phaseId = "phase320-standard-electroweak-ladder-normalization-boundary-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    standardElectroweakNormalizationBoundaryAuditPassed,
    standardElectroweakReferenceMaterialized,
    standardElectroweakReferenceId,
    standardElectroweakReferenceUrl,
    standardWChargedLadderDefinitionAvailable,
    standardWChargedLadderNormalizationIsSqrtTwo,
    standardZRequiresNeutralSu2U1Mixing,
    standardPhotonZRotationRequiresWeakMixingAngle,
    standardTreeLevelMassesRequireGgv,
    standardTreeLevelHiggsRequiresLambdaAndV,
    standardElectroweakAlgebraProvidesPhase302ScaleLaw,
    standardElectroweakAlgebraProvidesSourceModeVectorLengthScale,
    standardElectroweakAlgebraJustifiesW416Z156Scale,
    standardElectroweakAlgebraPromotesDecoupledSelector,
    standardElectroweakBoundaryPromotesWzMasses,
    standardElectroweakBoundaryPromotesHiggsMass,
    standardElectroweakBoundaryCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase307Boundary = new
    {
        phase307ChargedOperator,
        phase307ChargedOperatorMatchesStandardShape,
        phase307NearPassPresent,
        phase307TargetIndependentConstruction,
        phase307RawStableCommonSelectionLawCount,
        phase307P302ScaledStableCommonSelectionLawCount,
        phase307SelectionLawCanFillContractCount,
        phase307TheoremClaimed,
        phase307CanFillContract,
    },
    phase308ScaleBoundary = new
    {
        phase308P302WTotalScale,
        phase308P302ZTotalScale,
        phase308CommonScaleApplicationTheoremPresent,
        phase308ParticleLawApplicationTheoremPresent,
        phase308ScaleTransferAllowed,
        phase308CanFillContract,
    },
    phase309MeasureBoundary = new
    {
        phase309VectorLengthScaleIsCoordinateCount,
        phase309VectorLengthScaleIsNotL2MeasureConversion,
        phase309ApplicationTheoremPresent,
        phase309ScalePromotable,
        phase309CanFillContract,
    },
    phase313ObservedMapBoundary = new
    {
        phase313ProjectionMapAuditPassed,
        phase313PhotonZWeinbergRotation,
        phase313WeakMixingAngleSource,
        phase313NeutralMassMatrixDiagonalization,
        phase313ObservedElectroweakGaugeEmbedding,
        phase313CanFillContract,
    },
    phase317ExternalMassMatrixBoundary = new
    {
        phase317MassMatrixAuditPassed,
        phase317SmDefinesChargedWCombination,
        phase317SmDefinesPhotonZWeinbergRotation,
        phase317SmMassMatrixProvidesExternalDependencyMap,
        phase317SmMassMatrixPromotesWz,
        phase317SmMassMatrixPromotesHiggs,
        phase317SmMassMatrixCompletes,
        phase317CanFillWz,
        phase317CanFillHiggs,
        phase317CanFillObserved,
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionPromotable,
    },
    externalSources = new[]
    {
        new ExternalSource(
            standardElectroweakReferenceId,
            "Particle Data Group, Electroweak Model and Constraints on New Physics",
            standardElectroweakReferenceUrl,
            "Records the standard electroweak SU(2)xU(1) field definitions, W charged combinations, photon/Z mixing, and tree-level mass dependencies."),
    },
    checks,
    decision = standardElectroweakNormalizationBoundaryAuditPassed
        ? "Do not promote the Phase302/307 charged-ladder near pass from standard electroweak algebra. The PDG/SM boundary supports the W charged-ladder shape, but Z requires an observed neutral SU(2)-U(1) mixing map and the masses still require v, g, g-prime, and Higgs-potential inputs. Standard algebra does not derive the repo-specific source-mode vector-length scale, W=416/Z=156 transfer, decoupled row selector, or Phase201 source-lineage fields."
        : "Review the standard electroweak ladder normalization boundary audit before using it as claim evidence.",
    nextRequiredArtifact = new[]
    {
        "A GU-local observed electroweak embedding theorem deriving W, photon, and Z rows before target comparison.",
        "A source-backed normalization theorem replacing or deriving the Phase302 156/416 scale in the physical W/Z source-row family.",
        "W/Z source-lineage rows that pass unscaled raw/common gates and remain branch-stable without target-fitted scaling.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase256Path = Phase256Path,
        phase307Path = Phase307Path,
        phase308Path = Phase308Path,
        phase309Path = Phase309Path,
        phase313Path = Phase313Path,
        phase317Path = Phase317Path,
        standardElectroweakReferenceUrl,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "standard_electroweak_ladder_normalization_boundary_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "standard_electroweak_ladder_normalization_boundary_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"standardElectroweakNormalizationBoundaryAuditPassed={standardElectroweakNormalizationBoundaryAuditPassed}");
Console.WriteLine($"standardElectroweakAlgebraProvidesPhase302ScaleLaw={standardElectroweakAlgebraProvidesPhase302ScaleLaw}");
Console.WriteLine($"standardElectroweakAlgebraPromotesDecoupledSelector={standardElectroweakAlgebraPromotesDecoupledSelector}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

public sealed record Check(string CheckId, bool Passed, string Detail);

public sealed record ExternalSource(string SourceId, string Title, string Url, string Finding);

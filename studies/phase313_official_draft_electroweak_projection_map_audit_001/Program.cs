using System.Text.Json;

const string DefaultOutputDir = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output";
const string Phase27ReadinessPath = "studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json";
const string Phase46PromotionPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/promotion_result.json";
const string Phase46SelectorAuditPath = "studies/phase46_electroweak_term_wz_physical_prediction_001/selector_eigen_operator_term_audit.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase311Path = "studies/phase311_completion_observed_sector_wz_row_selector_audit_001/output/completion_observed_sector_wz_row_selector_audit_summary.json";
const string Phase312Path = "studies/phase312_current_public_gu_rvg_revision_delta_audit_001/output/current_public_gu_rvg_revision_delta_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE313_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase27 = JsonDocument.Parse(File.ReadAllText(Phase27ReadinessPath));
using var phase46 = JsonDocument.Parse(File.ReadAllText(Phase46PromotionPath));
using var phase46Selector = JsonDocument.Parse(File.ReadAllText(Phase46SelectorAuditPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase311 = JsonDocument.Parse(File.ReadAllText(Phase311Path));
using var phase312 = JsonDocument.Parse(File.ReadAllText(Phase312Path));

var officialDraftParameterSourceGapAuditPassed = JsonBool(phase287.RootElement, "officialDraftParameterSourceGapAuditPassed") is true;
var officialGuParameterLocationLeadPresent = JsonBool(phase287.RootElement, "officialGuParameterLocationLeadPresent") is true;
var officialDraftFillsParameterGaps = JsonBool(phase287.RootElement, "officialDraftFillsPhase286Gaps") is true;
var officialDraftPromotesWzMasses = JsonBool(phase287.RootElement, "officialDraftPromotesWzMasses") is true;
var officialDraftPromotesHiggsMass = JsonBool(phase287.RootElement, "officialDraftPromotesHiggsMass") is true;

var phase27InternalCartanMixingConventionReady = JsonString(phase27.RootElement, "terminalStatus") == "mixing-convention-ready"
    && phase27.RootElement.TryGetProperty("convention", out var convention)
    && JsonString(convention, "status") == "validated"
    && JsonBool(convention, "externalTargetValuesUsed") is false;
var phase27NeutralAxisIndex = phase27.RootElement.TryGetProperty("convention", out convention)
    ? JsonInt(convention, "neutralBasisAxisIndex")
    : null;
var phase27ChargedAxisCount = phase27.RootElement.TryGetProperty("convention", out convention)
    && convention.TryGetProperty("chargedBasisAxisIndices", out var chargedBasisAxisIndices)
    && chargedBasisAxisIndices.ValueKind == JsonValueKind.Array
        ? chargedBasisAxisIndices.GetArrayLength()
        : 0;

var phase46WzRatioPhysicalClaimAllowed = phase46.RootElement.TryGetProperty("observableClassifications", out var classificationsTable)
    && classificationsTable.TryGetProperty("classifications", out var classifications)
    && classifications.ValueKind == JsonValueKind.Array
    && classifications.EnumerateArray().Any(classification =>
        JsonString(classification, "observableId") == "physical-w-z-mass-ratio"
        && JsonBool(classification, "physicalClaimAllowed") is true);
var phase46OnlyRatioObservableMapped = phase46.RootElement.TryGetProperty("physicalObservableMappings", out var mappingsTable)
    && mappingsTable.TryGetProperty("mappings", out var mappings)
    && mappings.ValueKind == JsonValueKind.Array
    && mappings.GetArrayLength() == 1
    && mappings.EnumerateArray().All(mapping => JsonString(mapping, "targetPhysicalObservableId") == "physical-w-z-mass-ratio");
var phase46SelectorOperatorTermReady = JsonString(phase46Selector.RootElement, "terminalStatus") == "wz-selector-eigen-operator-term-ready"
    && JsonStringArray(phase46Selector.RootElement, "observedModeBlocks").Contains("electroweak-mixing", StringComparer.Ordinal);

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase295IntakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var phase295AnyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var phase311CompletionDraftProvidesCanonicalWzRowSelector = JsonBool(phase311.RootElement, "completionDraftProvidesCanonicalWzRowSelector") is true;
var phase311CompletionDraftProvidesPhotonWzEigenstateProjectionRows = JsonBool(phase311.RootElement, "completionDraftProvidesPhotonWzEigenstateProjectionRows") is true;
var phase311CompletionDraftProvidesPhysicalWzObservableMap = JsonBool(phase311.RootElement, "completionDraftProvidesPhysicalWzObservableMap") is true;
var phase311CanFillPhase201WzContract = JsonBool(phase311.RootElement, "canFillPhase201WzContract") is true;
var phase312CurrentPublicGuRvgPromotesWzMasses = JsonBool(phase312.RootElement, "currentPublicGuRvgPromotesWzMasses") is true;
var phase312CurrentPublicGuRvgProvidesPhotonWzProjection = phase312.RootElement.TryGetProperty("currentPublicGuRvgBoundary", out var p312Boundary)
    && JsonBool(p312Boundary, "currentPublicGuRvgProvidesPhotonWzEigenstateProjectionRows") is true;

var officialDraftProvidesWeakIsospinLocation = true;
var officialDraftProvidesWeakHyperchargeLocation = true;
var officialDraftProvidesHiggsFieldLocation = true;
var officialDraftProvidesHiggsPotentialLocation = true;
var officialDraftProvidesPhotonZWeinbergRotation = false;
var officialDraftProvidesElectromagneticUnbrokenGenerator = false;
var officialDraftProvidesWeakMixingAngleSource = false;
var officialDraftProvidesNeutralMassMatrixDiagonalization = false;
var officialDraftProvidesPhotonMasslessProjectionRow = false;
var officialDraftProvidesWChargedProjectionRows = false;
var officialDraftProvidesZSourceRowProjection = false;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = false;
var officialDraftProjectionMapCompletesObservedFieldExtraction = false;
var officialDraftProjectionMapPromotesWzMasses = false;
var officialDraftProjectionMapPromotesHiggsMass = false;
var canFillPhase256ObservedFieldExtractionContract = false;
var canFillPhase201WzContract = false;

var projectionRequirements = new[]
{
    new ProjectionRequirement("weak-isospin-location", "Official draft location for weak isospin / SU(2)-like electroweak structure.", officialDraftProvidesWeakIsospinLocation),
    new ProjectionRequirement("weak-hypercharge-location", "Official draft location for weak hypercharge / U(1)-like electroweak structure.", officialDraftProvidesWeakHyperchargeLocation),
    new ProjectionRequirement("internal-cartan-charge-sector-convention", "Repository internal Cartan convention assigning charged axes and one neutral axis without W/Z/photon targets.", phase27InternalCartanMixingConventionReady),
    new ProjectionRequirement("photon-z-weinberg-rotation", "A source-derived neutral rotation from internal neutral and hypercharge directions into photon and Z eigenstates.", officialDraftProvidesPhotonZWeinbergRotation),
    new ProjectionRequirement("unbroken-electromagnetic-generator", "A target-independent unbroken U(1)_em generator with photon masslessness sidecar.", officialDraftProvidesElectromagneticUnbrokenGenerator),
    new ProjectionRequirement("weak-mixing-angle-or-coupling-source", "A GU-local weak mixing angle or g/g prime coupling source before target comparison.", officialDraftProvidesWeakMixingAngleSource),
    new ProjectionRequirement("neutral-electroweak-mass-matrix-diagonalization", "A source-derived neutral quadratic mass matrix and diagonalization into photon/Z rows.", officialDraftProvidesNeutralMassMatrixDiagonalization),
    new ProjectionRequirement("physical-w-projection-rows", "Charged W projection/source rows with branch, normalization, and source-lineage provenance.", officialDraftProvidesWChargedProjectionRows),
    new ProjectionRequirement("physical-z-projection-row", "Physical Z projection/source row with branch, normalization, and source-lineage provenance.", officialDraftProvidesZSourceRowProjection),
    new ProjectionRequirement("observed-electroweak-gauge-embedding", "A branch-stable observed-sector electroweak gauge embedding tying GU fields to physical W/Z/photon observables.", officialDraftProvidesObservedElectroweakGaugeEmbedding),
};

var checks = new[]
{
    new Check(
        "official-draft-electroweak-location-leads-present",
        officialDraftParameterSourceGapAuditPassed
            && officialGuParameterLocationLeadPresent
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && officialDraftProvidesHiggsFieldLocation
            && officialDraftProvidesHiggsPotentialLocation,
        $"officialDraftParameterSourceGapAuditPassed={officialDraftParameterSourceGapAuditPassed}; officialGuParameterLocationLeadPresent={officialGuParameterLocationLeadPresent}; officialDraftFillsParameterGaps={officialDraftFillsParameterGaps}"),
    new Check(
        "internal-cartan-convention-is-ratio-only-not-observed-projection",
        phase27InternalCartanMixingConventionReady
            && phase27NeutralAxisIndex == 2
            && phase27ChargedAxisCount == 2
            && phase46WzRatioPhysicalClaimAllowed
            && phase46OnlyRatioObservableMapped
            && phase46SelectorOperatorTermReady,
        $"phase27InternalCartanMixingConventionReady={phase27InternalCartanMixingConventionReady}; phase27NeutralAxisIndex={phase27NeutralAxisIndex}; phase27ChargedAxisCount={phase27ChargedAxisCount}; phase46WzRatioPhysicalClaimAllowed={phase46WzRatioPhysicalClaimAllowed}; phase46OnlyRatioObservableMapped={phase46OnlyRatioObservableMapped}; phase46SelectorOperatorTermReady={phase46SelectorOperatorTermReady}"),
    new Check(
        "official-draft-does-not-supply-photon-z-projection-map",
        !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesElectromagneticUnbrokenGenerator
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesNeutralMassMatrixDiagonalization
            && !officialDraftProvidesPhotonMasslessProjectionRow
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding,
        $"photonZWeinbergRotation={officialDraftProvidesPhotonZWeinbergRotation}; electromagneticGenerator={officialDraftProvidesElectromagneticUnbrokenGenerator}; weakMixingAngleSource={officialDraftProvidesWeakMixingAngleSource}; neutralMassMatrixDiagonalization={officialDraftProvidesNeutralMassMatrixDiagonalization}; photonProjection={officialDraftProvidesPhotonMasslessProjectionRow}; zProjection={officialDraftProvidesZSourceRowProjection}; observedElectroweakGaugeEmbedding={officialDraftProvidesObservedElectroweakGaugeEmbedding}"),
    new Check(
        "completion-and-current-public-deltas-do-not-fill-projection-map",
        !phase311CompletionDraftProvidesCanonicalWzRowSelector
            && !phase311CompletionDraftProvidesPhotonWzEigenstateProjectionRows
            && !phase311CompletionDraftProvidesPhysicalWzObservableMap
            && !phase311CanFillPhase201WzContract
            && !phase312CurrentPublicGuRvgPromotesWzMasses
            && !phase312CurrentPublicGuRvgProvidesPhotonWzProjection,
        $"phase311CanonicalWzRowSelector={phase311CompletionDraftProvidesCanonicalWzRowSelector}; phase311PhotonWzEigenstateProjectionRows={phase311CompletionDraftProvidesPhotonWzEigenstateProjectionRows}; phase311PhysicalWzObservableMap={phase311CompletionDraftProvidesPhysicalWzObservableMap}; phase311CanFillPhase201WzContract={phase311CanFillPhase201WzContract}; phase312PromotesWz={phase312CurrentPublicGuRvgPromotesWzMasses}; phase312PhotonWzProjection={phase312CurrentPublicGuRvgProvidesPhotonWzProjection}"),
    new Check(
        "observed-field-extraction-contract-remains-unfilled",
        observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && phase295IntakeReadyObservedFieldExtractionCandidateCount == 0
            && !phase295AnyObservedFieldExtractionCandidateFillsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"requiredFieldCount={observedFieldExtractionRequiredFieldCount}; filledRequiredFieldCount={observedFieldExtractionFilledRequiredFieldCount}; observedFieldExtractionContractPromotable={observedFieldExtractionContractPromotable}; phase295IntakeReadyCount={phase295IntakeReadyObservedFieldExtractionCandidateCount}; phase295AnyCandidateFillsContract={phase295AnyObservedFieldExtractionCandidateFillsContract}; canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}"),
    new Check(
        "source-contracts-remain-unfilled",
        wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && !officialDraftPromotesWzMasses
            && !officialDraftPromotesHiggsMass
            && !officialDraftProjectionMapPromotesWzMasses
            && !officialDraftProjectionMapPromotesHiggsMass
            && !canFillPhase201WzContract,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; officialDraftPromotesWzMasses={officialDraftPromotesWzMasses}; officialDraftPromotesHiggsMass={officialDraftPromotesHiggsMass}; officialDraftProjectionMapPromotesWzMasses={officialDraftProjectionMapPromotesWzMasses}; officialDraftProjectionMapPromotesHiggsMass={officialDraftProjectionMapPromotesHiggsMass}; canFillPhase201WzContract={canFillPhase201WzContract}"),
};

var officialDraftElectroweakProjectionMapAuditPassed = checks.All(check => check.Passed)
    && !officialDraftProjectionMapCompletesObservedFieldExtraction
    && !officialDraftProjectionMapPromotesWzMasses
    && !officialDraftProjectionMapPromotesHiggsMass
    && !canFillPhase201WzContract;
var terminalStatus = officialDraftElectroweakProjectionMapAuditPassed
    ? "official-draft-electroweak-projection-map-audit-symbolic-placement-not-observed-map"
    : "official-draft-electroweak-projection-map-audit-review-required";

var result = new
{
    phaseId = "phase313-official-draft-electroweak-projection-map-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    officialDraftElectroweakProjectionMapAuditPassed,
    officialGuParameterLocationLeadPresent,
    officialDraftProvidesWeakIsospinLocation,
    officialDraftProvidesWeakHyperchargeLocation,
    officialDraftProvidesHiggsFieldLocation,
    officialDraftProvidesHiggsPotentialLocation,
    phase27InternalCartanMixingConventionReady,
    phase46WzRatioPhysicalClaimAllowed,
    phase46OnlyRatioObservableMapped,
    phase46SelectorOperatorTermReady,
    officialDraftProvidesPhotonZWeinbergRotation,
    officialDraftProvidesElectromagneticUnbrokenGenerator,
    officialDraftProvidesWeakMixingAngleSource,
    officialDraftProvidesNeutralMassMatrixDiagonalization,
    officialDraftProvidesPhotonMasslessProjectionRow,
    officialDraftProvidesWChargedProjectionRows,
    officialDraftProvidesZSourceRowProjection,
    officialDraftProvidesObservedElectroweakGaugeEmbedding,
    officialDraftProjectionMapCompletesObservedFieldExtraction,
    officialDraftProjectionMapPromotesWzMasses,
    officialDraftProjectionMapPromotesHiggsMass,
    canFillPhase256ObservedFieldExtractionContract,
    canFillPhase201WzContract,
    projectionRequirements,
    standardElectroweakProjectionDependency = new
    {
        reference = "Particle Data Group electroweak-review dependency structure",
        url = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        summary = "Physical photon and Z fields require a neutral W3/B rotation fixed by electroweak couplings and the Higgs/VEV mass matrix; an internal Cartan neutral-axis convention is not by itself a physical photon/Z eigenstate projection.",
    },
    inheritedEvidence = new
    {
        phase27 = new
        {
            phase27InternalCartanMixingConventionReady,
            phase27NeutralAxisIndex,
            phase27ChargedAxisCount,
        },
        phase46 = new
        {
            phase46WzRatioPhysicalClaimAllowed,
            phase46OnlyRatioObservableMapped,
            phase46SelectorOperatorTermReady,
        },
        phase287 = new
        {
            officialDraftParameterSourceGapAuditPassed,
            officialGuParameterLocationLeadPresent,
            officialDraftFillsParameterGaps,
            officialDraftPromotesWzMasses,
            officialDraftPromotesHiggsMass,
        },
        phase295 = new
        {
            phase295IntakeReadyObservedFieldExtractionCandidateCount,
            phase295AnyObservedFieldExtractionCandidateFillsContract,
        },
        phase311 = new
        {
            phase311CompletionDraftProvidesCanonicalWzRowSelector,
            phase311CompletionDraftProvidesPhotonWzEigenstateProjectionRows,
            phase311CompletionDraftProvidesPhysicalWzObservableMap,
            phase311CanFillPhase201WzContract,
        },
        phase312 = new
        {
            phase312CurrentPublicGuRvgPromotesWzMasses,
            phase312CurrentPublicGuRvgProvidesPhotonWzProjection,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase256 = new
        {
            observedFieldExtractionRequiredFieldCount,
            observedFieldExtractionFilledRequiredFieldCount,
            observedFieldExtractionContractPromotable,
        },
    },
    checks,
    decision = officialDraftElectroweakProjectionMapAuditPassed
        ? "Do not promote W/Z or Higgs masses from the official draft's electroweak placement language plus the repository's internal Cartan convention. The combination supports the existing dimensionless W/Z ratio lane, but it does not supply a physical photon/Z Weinberg rotation, unbroken electromagnetic generator, weak-mixing/coupling source, neutral mass-matrix diagonalization, or branch-stable observed W/Z/photon projection rows."
        : "Review the official-draft electroweak projection-map audit before relying on package boundaries.",
    nextRequiredArtifact = new[]
    {
        "A target-independent observed electroweak gauge-embedding theorem mapping GU fields to SU(2)_L x U(1)_Y and U(1)_em.",
        "A source-derived neutral mass matrix and photon/Z eigenstate projection with branch-stable rows.",
        "A GU-local weak-mixing/coupling source and W/Z source rows that pass Phase201/P209/P210/P213 before target comparison.",
        "A Higgs scalar-source/operator extraction if the same observed-sector map is meant to promote Higgs mass.",
    },
    sourceEvidence = new
    {
        phase27ReadinessPath = Phase27ReadinessPath,
        phase46PromotionPath = Phase46PromotionPath,
        phase46SelectorAuditPath = Phase46SelectorAuditPath,
        phase213Path = Phase213Path,
        phase256Path = Phase256Path,
        phase287Path = Phase287Path,
        phase295Path = Phase295Path,
        phase311Path = Phase311Path,
        phase312Path = Phase312Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "official_draft_electroweak_projection_map_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "official_draft_electroweak_projection_map_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.officialDraftElectroweakProjectionMapAuditPassed,
        result.officialGuParameterLocationLeadPresent,
        result.officialDraftProvidesWeakIsospinLocation,
        result.officialDraftProvidesWeakHyperchargeLocation,
        result.phase27InternalCartanMixingConventionReady,
        result.phase46WzRatioPhysicalClaimAllowed,
        result.phase46OnlyRatioObservableMapped,
        result.officialDraftProvidesPhotonZWeinbergRotation,
        result.officialDraftProvidesElectromagneticUnbrokenGenerator,
        result.officialDraftProvidesWeakMixingAngleSource,
        result.officialDraftProvidesNeutralMassMatrixDiagonalization,
        result.officialDraftProvidesPhotonMasslessProjectionRow,
        result.officialDraftProvidesWChargedProjectionRows,
        result.officialDraftProvidesZSourceRowProjection,
        result.officialDraftProvidesObservedElectroweakGaugeEmbedding,
        result.officialDraftProjectionMapCompletesObservedFieldExtraction,
        result.officialDraftProjectionMapPromotesWzMasses,
        result.officialDraftProjectionMapPromotesHiggsMass,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.canFillPhase201WzContract,
        result.projectionRequirements,
        result.inheritedEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"officialDraftElectroweakProjectionMapAuditPassed={officialDraftElectroweakProjectionMapAuditPassed}");
Console.WriteLine($"phase27InternalCartanMixingConventionReady={phase27InternalCartanMixingConventionReady}");
Console.WriteLine($"phase46WzRatioPhysicalClaimAllowed={phase46WzRatioPhysicalClaimAllowed}");
Console.WriteLine($"officialDraftProjectionMapPromotesWzMasses={officialDraftProjectionMapPromotesWzMasses}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static IReadOnlyList<string> JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()!).ToArray()
        : Array.Empty<string>();

sealed record ProjectionRequirement(string RequirementId, string Detail, bool Filled);
sealed record Check(string CheckId, bool Passed, string Detail);

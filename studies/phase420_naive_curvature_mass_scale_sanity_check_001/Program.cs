using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase420: naive curvature mass scale sanity check.
//
// The restart prompt asks for a target-blind check of naive scale readings,
// especially the Superphysics/GU-draft stylized m = R(y)/4 curvature-mass
// relation. This phase does only dimensional and contract bookkeeping. It
// compares no W/Z/H targets, fills no source-lineage field, and mutates no
// Phase201/Phase256 template.

const string DefaultOutputDir = "studies/phase420_naive_curvature_mass_scale_sanity_check_001/output";
const string Phase201SummaryPath = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213SummaryPath = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase220SummaryPath = "studies/phase220_boson_dimensional_scale_obstruction_audit_001/output/boson_dimensional_scale_obstruction_audit_summary.json";
const string Phase256SummaryPath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase328SummaryPath = "studies/phase328_superphysics_draft_energy_scale_source_audit_001/output/superphysics_draft_energy_scale_source_audit_summary.json";
const string Phase410SummaryPath = "studies/phase410_curvature_coupled_vev_selection_probe_001/output/curvature_coupled_vev_selection_probe_summary.json";
const string Phase418SummaryPath = "studies/phase418_direction_dependent_curvature_vev_coupling_scan_001/output/direction_dependent_curvature_vev_coupling_scan_summary.json";
const string Phase419SummaryPath = "studies/phase419_observed_field_symbolic_extraction_template_001/output/observed_field_symbolic_extraction_template_summary.json";
const string ApplicationSubjectKind = "naive-curvature-mass-scale-sanity-check";

var outputDir = Environment.GetEnvironmentVariable("PHASE420_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201SummaryPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213SummaryPath));
using var phase220 = JsonDocument.Parse(File.ReadAllText(Phase220SummaryPath));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
using var phase328 = JsonDocument.Parse(File.ReadAllText(Phase328SummaryPath));
using var phase410 = JsonDocument.Parse(File.ReadAllText(Phase410SummaryPath));
using var phase418 = JsonDocument.Parse(File.ReadAllText(Phase418SummaryPath));
using var phase419 = JsonDocument.Parse(File.ReadAllText(Phase419SummaryPath));

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool literalScalarCurvatureMassReadingDimensionallyConsistent = false;
const bool squaredMassCurvatureReadingDimensionallyConsistent = true;
const bool squaredMassReadingProvidesOnlySymbolicScaleShell = true;
const bool sourceProvidesCurvatureValueOrUnitAnchor = false;
const bool sourceProvidesSignConvention = false;
const bool sourceProvidesMassCoefficientNormalization = false;
const bool sourceProvidesElectroweakVevMap = false;
const bool sourceProvidesWeakAngleOrCouplingLineage = false;
const bool sourceProvidesSeparateWzRows = false;
const bool sourceProvidesPhotonZMixingRows = false;
const bool sourceProvidesHiggsScalarExcitationRow = false;
const bool sourceProvidesPoleExtraction = false;
const bool sourceProvidesGeVUnitNormalization = false;
const bool singleCurvatureScalarCanOnlySetCommonScale = true;
const bool naiveCurvatureLawCanDistinguishWzHRows = false;
const bool naiveCurvatureLawPromotable = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool sourceContractApplicationAllowed = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;

var dimensionalReadings = new[]
{
    new DimensionalReading(
        "literal-m-equals-R-over-4",
        "m = R/4",
        CurvatureMassDimension: 2,
        RequiredMassDimension: 1,
        DimensionallyConsistent: false,
        RepairNeeded: "Interpret R as something other than scalar curvature, or supply a square root/unit conversion.",
        Verdict: "closed-literal-reading"),
    new DimensionalReading(
        "lichnerowicz-squared-mass-reading",
        "m^2 = R/4, so m = sqrt(R)/2",
        CurvatureMassDimension: 2,
        RequiredMassDimension: 1,
        DimensionallyConsistent: true,
        RepairNeeded: "Still requires sign, curvature value, normalization, unit conversion, and particle-specific rows.",
        Verdict: "symbolic-scale-shell-only"),
    new DimensionalReading(
        "single-curvature-common-scale-reading",
        "m_i = c_i sqrt(R)/2",
        CurvatureMassDimension: 2,
        RequiredMassDimension: 1,
        DimensionallyConsistent: true,
        RepairNeeded: "Every coefficient c_i, projection row, weak-angle relation, and Higgs excitation law must be source-derived.",
        Verdict: "not-a-complete-boson-source-law"),
};

var requiredScaleSpecificationFields = new[]
{
    new RequiredScaleField("curvature-scalar-definition", "Which scalar, operator eigenvalue, or effective curvature R is used.", Provided: true),
    new RequiredScaleField("mass-vs-mass-squared-convention", "Whether the curvature term enters m or m^2.", Provided: false),
    new RequiredScaleField("sign-convention-and-positivity-domain", "A source-defined sign and allowed curvature domain for positive physical poles.", Provided: false),
    new RequiredScaleField("coefficient-normalization", "The 1/4 coefficient and any representation factors fixed by source, not targets.", Provided: false),
    new RequiredScaleField("curvature-value-or-dynamical-equation", "A target-independent value/equation for R on the observed vacuum.", Provided: false),
    new RequiredScaleField("electroweak-vev-map", "A map from curvature scale to the observed electroweak VEV/order parameter.", Provided: false),
    new RequiredScaleField("weak-angle-and-coupling-lineage", "Source lineage for g, gPrime, and theta_W at the mass shell.", Provided: false),
    new RequiredScaleField("particle-specific-projection-rows", "Separate photon, W, Z, and Higgs rows tied to the same operator.", Provided: false),
    new RequiredScaleField("pole-extraction", "Gauge-invariant pole extraction or equivalent spectral prescription.", Provided: false),
    new RequiredScaleField("gev-unit-normalization", "A source-derived conversion to physical energy units.", Provided: false),
};

var providedRequiredScaleSpecificationFieldCount = requiredScaleSpecificationFields.Count(field => field.Provided);
var missingScaleSpecificationFields = requiredScaleSpecificationFields
    .Where(field => !field.Provided)
    .Select(field => field.FieldId)
    .ToArray();

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var obstructionAuditPassed = JsonBool(phase220.RootElement, "obstructionAuditPassed") is true;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var superphysicsDraftEnergyScaleSourceAuditPassed = JsonBool(phase328.RootElement, "superphysicsDraftEnergyScaleSourceAuditPassed") is true;
var mirrorProvidesTargetIndependentWzEnergyScale = JsonBool(phase328.RootElement, "mirrorProvidesTargetIndependentWzEnergyScale") is true;
var mirrorProvidesTargetIndependentVevSource = JsonBool(phase328.RootElement, "mirrorProvidesTargetIndependentVevSource") is true;
var mirrorProvidesGeVUnitNormalization = JsonBool(phase328.RootElement, "mirrorProvidesGeVUnitNormalization") is true;
var mirrorPromotesWzMasses = JsonBool(phase328.RootElement, "mirrorPromotesWzMasses") is true;
var mirrorPromotesHiggsMass = JsonBool(phase328.RootElement, "mirrorPromotesHiggsMass") is true;
var curvatureCoupledVevSelectionProbePassed = JsonBool(phase410.RootElement, "curvatureCoupledVevSelectionProbePassed") is true;
var curvatureCouplingProducesRunawayAlongFlatRays = JsonBool(phase410.RootElement, "curvatureCouplingProducesRunawayAlongFlatRays") is true;
var phase410RouteProvidesPoleExtractionAndGeVNormalization = JsonBool(phase410.RootElement, "routeProvidesPoleExtractionAndGeVNormalization") is true;
var directionDependentCurvatureVevCouplingScanPassed = JsonBool(phase418.RootElement, "directionDependentCurvatureVevCouplingScanPassed") is true;
var directionDependentCouplingSourceLawStillMissing = JsonBool(phase418.RootElement, "directionDependentCouplingSourceLawStillMissing") is true;
var finiteVevScaleStillExternal = JsonBool(phase418.RootElement, "finiteVevScaleStillExternal") is true;
var phase418RouteProvidesPoleExtractionAndGeVNormalization = JsonBool(phase418.RootElement, "routeProvidesPoleExtractionAndGeVNormalization") is true;
var observedFieldSymbolicExtractionTemplatePassed = JsonBool(phase419.RootElement, "observedFieldSymbolicExtractionTemplatePassed") is true;
var phase419RequiredGuInputProvidedCount = JsonInt(phase419.RootElement, "requiredGuInputProvidedCount") ?? -1;
var phase419SourceDefinedPhase256FieldCount = JsonInt(phase419.RootElement, "sourceDefinedPhase256FieldCount") ?? -1;
var phase419CanFillObserved = JsonBool(phase419.RootElement, "canFillPhase256ObservedFieldExtractionContract") is true;

var targetBlindConstructionHash = Sha256Hex(JsonSerializer.Serialize(new
{
    ApplicationSubjectKind,
    dimensionalReadings,
    requiredScaleSpecificationFields,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
}, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

var checks = new[]
{
    new Check(
        "literal-curvature-mass-reading-fails-dimensional-analysis",
        !literalScalarCurvatureMassReadingDimensionallyConsistent
            && dimensionalReadings.Single(reading => reading.ReadingId == "literal-m-equals-R-over-4").CurvatureMassDimension == 2,
        $"literalConsistent={literalScalarCurvatureMassReadingDimensionallyConsistent}; RDimension=mass^2; mDimension=mass"),
    new Check(
        "squared-mass-reading-is-symbolic-only",
        squaredMassCurvatureReadingDimensionallyConsistent
            && squaredMassReadingProvidesOnlySymbolicScaleShell
            && !sourceProvidesCurvatureValueOrUnitAnchor
            && !sourceProvidesMassCoefficientNormalization
            && !sourceProvidesGeVUnitNormalization,
        $"squaredMassConsistent={squaredMassCurvatureReadingDimensionallyConsistent}; unitAnchor={sourceProvidesCurvatureValueOrUnitAnchor}; coefficient={sourceProvidesMassCoefficientNormalization}; gev={sourceProvidesGeVUnitNormalization}"),
    new Check(
        "single-curvature-scale-cannot-fill-particle-rows",
        singleCurvatureScalarCanOnlySetCommonScale
            && !naiveCurvatureLawCanDistinguishWzHRows
            && !sourceProvidesSeparateWzRows
            && !sourceProvidesPhotonZMixingRows
            && !sourceProvidesHiggsScalarExcitationRow,
        $"commonScaleOnly={singleCurvatureScalarCanOnlySetCommonScale}; distinguishesRows={naiveCurvatureLawCanDistinguishWzHRows}; wzRows={sourceProvidesSeparateWzRows}; photonZRows={sourceProvidesPhotonZMixingRows}; higgsRow={sourceProvidesHiggsScalarExcitationRow}"),
    new Check(
        "required-scale-specification-mostly-absent",
        requiredScaleSpecificationFields.Length == 10
            && providedRequiredScaleSpecificationFieldCount == 1
            && missingScaleSpecificationFields.Length == 9,
        $"required={requiredScaleSpecificationFields.Length}; provided={providedRequiredScaleSpecificationFieldCount}; missing={missingScaleSpecificationFields.Length}"),
    new Check(
        "superphysics-energy-scale-boundary-preserved",
        superphysicsDraftEnergyScaleSourceAuditPassed
            && !mirrorProvidesTargetIndependentWzEnergyScale
            && !mirrorProvidesTargetIndependentVevSource
            && !mirrorProvidesGeVUnitNormalization
            && !mirrorPromotesWzMasses
            && !mirrorPromotesHiggsMass,
        $"p328Passed={superphysicsDraftEnergyScaleSourceAuditPassed}; wzScale={mirrorProvidesTargetIndependentWzEnergyScale}; vev={mirrorProvidesTargetIndependentVevSource}; gev={mirrorProvidesGeVUnitNormalization}; promotesWz={mirrorPromotesWzMasses}; promotesHiggs={mirrorPromotesHiggsMass}"),
    new Check(
        "curvature-vev-precursor-boundaries-preserved",
        curvatureCoupledVevSelectionProbePassed
            && curvatureCouplingProducesRunawayAlongFlatRays
            && !phase410RouteProvidesPoleExtractionAndGeVNormalization
            && directionDependentCurvatureVevCouplingScanPassed
            && directionDependentCouplingSourceLawStillMissing
            && finiteVevScaleStillExternal
            && !phase418RouteProvidesPoleExtractionAndGeVNormalization,
        $"p410Passed={curvatureCoupledVevSelectionProbePassed}; p410Runaway={curvatureCouplingProducesRunawayAlongFlatRays}; p410PoleGev={phase410RouteProvidesPoleExtractionAndGeVNormalization}; p418Passed={directionDependentCurvatureVevCouplingScanPassed}; sourceLawMissing={directionDependentCouplingSourceLawStillMissing}; finiteScaleExternal={finiteVevScaleStillExternal}; p418PoleGev={phase418RouteProvidesPoleExtractionAndGeVNormalization}"),
    new Check(
        "observed-field-template-boundary-preserved",
        observedFieldSymbolicExtractionTemplatePassed
            && phase419RequiredGuInputProvidedCount == 0
            && phase419SourceDefinedPhase256FieldCount == 0
            && !phase419CanFillObserved
            && phase256RequiredFieldCount == 20
            && phase256FilledRequiredFieldCount == 0
            && !phase256ContractPromotable,
        $"p419Passed={observedFieldSymbolicExtractionTemplatePassed}; p419Inputs={phase419RequiredGuInputProvidedCount}; sourceDefinedP256={phase419SourceDefinedPhase256FieldCount}; p256Filled={phase256FilledRequiredFieldCount}; p256Promotable={phase256ContractPromotable}"),
    new Check(
        "source-lineage-and-promotion-boundary-preserved",
        targetBlindConstruction
            && !physicalTargetsConsultedForConstruction
            && targetBlindConstructionHash.Length == 64
            && obstructionAuditPassed
            && !phase201AllRequiredLineagesPromotable
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && !naiveCurvatureLawPromotable
            && !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !phase201TemplateMutated
            && fieldsAppliedToPhase201TemplateCount == 0
            && acceptedContractFieldCount == 0,
        $"targetBlind={targetBlindConstruction}; consultedTargets={physicalTargetsConsultedForConstruction}; hashLength={targetBlindConstructionHash.Length}; p220Passed={obstructionAuditPassed}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; promotable={naiveCurvatureLawPromotable}; canFillWz={canFillPhase201WzContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var naiveCurvatureMassScaleSanityCheckPassed = checks.All(check => check.Passed);
var terminalStatus = naiveCurvatureMassScaleSanityCheckPassed
    ? "naive-curvature-mass-scale-sanity-check-fail-closed"
    : "naive-curvature-mass-scale-sanity-check-review-required";

var result = new
{
    phaseId = "phase420-naive-curvature-mass-scale-sanity-check",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    naiveCurvatureMassScaleSanityCheckPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    motivatingSourceClue = new
    {
        referenceId = "SUPERPHYSICS-GU-DRAFT-MIRROR-20250530",
        locator = "part-12c",
        stylizedRelation = "m = R(y)/4",
        useBoundary = "search clue only; not primary promotion evidence",
    },
    dimensionalReadings,
    literalScalarCurvatureMassReadingDimensionallyConsistent,
    squaredMassCurvatureReadingDimensionallyConsistent,
    squaredMassReadingProvidesOnlySymbolicScaleShell,
    sourceProvidesCurvatureValueOrUnitAnchor,
    sourceProvidesSignConvention,
    sourceProvidesMassCoefficientNormalization,
    sourceProvidesElectroweakVevMap,
    sourceProvidesWeakAngleOrCouplingLineage,
    sourceProvidesSeparateWzRows,
    sourceProvidesPhotonZMixingRows,
    sourceProvidesHiggsScalarExcitationRow,
    sourceProvidesPoleExtraction,
    sourceProvidesGeVUnitNormalization,
    singleCurvatureScalarCanOnlySetCommonScale,
    naiveCurvatureLawCanDistinguishWzHRows,
    requiredScaleSpecificationFieldCount = requiredScaleSpecificationFields.Length,
    providedRequiredScaleSpecificationFieldCount,
    missingScaleSpecificationFieldCount = missingScaleSpecificationFields.Length,
    requiredScaleSpecificationFields,
    missingScaleSpecificationFields,
    upstreamBoundaries = new
    {
        phase220 = new
        {
            obstructionAuditPassed,
            terminalStatus = JsonString(phase220.RootElement, "terminalStatus"),
        },
        phase328 = new
        {
            superphysicsDraftEnergyScaleSourceAuditPassed,
            mirrorProvidesTargetIndependentWzEnergyScale,
            mirrorProvidesTargetIndependentVevSource,
            mirrorProvidesGeVUnitNormalization,
            mirrorPromotesWzMasses,
            mirrorPromotesHiggsMass,
        },
        phase410 = new
        {
            curvatureCoupledVevSelectionProbePassed,
            curvatureCouplingProducesRunawayAlongFlatRays,
            routeProvidesPoleExtractionAndGeVNormalization = phase410RouteProvidesPoleExtractionAndGeVNormalization,
        },
        phase418 = new
        {
            directionDependentCurvatureVevCouplingScanPassed,
            directionDependentCouplingSourceLawStillMissing,
            finiteVevScaleStillExternal,
            routeProvidesPoleExtractionAndGeVNormalization = phase418RouteProvidesPoleExtractionAndGeVNormalization,
        },
        phase419 = new
        {
            observedFieldSymbolicExtractionTemplatePassed,
            requiredGuInputProvidedCount = phase419RequiredGuInputProvidedCount,
            sourceDefinedPhase256FieldCount = phase419SourceDefinedPhase256FieldCount,
            canFillPhase256ObservedFieldExtractionContract = phase419CanFillObserved,
        },
    },
    checks,
    physicalCouplingProvided = false,
    naiveCurvatureLawPromotable,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
        phase256FieldsDefensiblyFilled = Array.Empty<string>(),
        wzMissingFieldCount,
        higgsMissingFieldCount,
        phase256FilledRequiredFieldCount,
    },
    explicitNonclaims = new[]
    {
        "the literal scalar-curvature m = R/4 reading is dimensionally invalid for mass",
        "the m^2 = R/4 repair is only a symbolic one-scale shell",
        "no source-defined curvature value, sign, coefficient normalization, VEV map, weak-angle lineage, particle rows, pole extraction, or GeV unit conversion is supplied",
        "no Phase201 or Phase256 field is filled",
        "no W/Z/H physical mass is promoted",
    },
    decision = naiveCurvatureMassScaleSanityCheckPassed
        ? "The naive curvature-mass route is closed as a direct scale law. Any viable curvature route must add a nontrivial source-defined unit/normalization anchor, electroweak VEV map, particle-specific projection/source rows, weak-angle/coupling lineage, and pole/GeV normalization."
        : "Review the curvature scale bookkeeping before using Phase420 as a boundary.",
    nextRequiredArtifact = new[]
    {
        "A source-defined curvature-to-electroweak-VEV equation with sign, coefficient, unit, and vacuum normalization.",
        "A source-defined observed-field extraction theorem tying that scale to photon/W/Z/H projection rows and pole extraction.",
        "A source-defined weak-angle/coupling lineage and separate W/Z/H source rows that can pass Phase201/209/210/213/256.",
    },
    sourceEvidence = new
    {
        phase201SummaryPath = Phase201SummaryPath,
        phase213SummaryPath = Phase213SummaryPath,
        phase220SummaryPath = Phase220SummaryPath,
        phase256SummaryPath = Phase256SummaryPath,
        phase328SummaryPath = Phase328SummaryPath,
        phase410SummaryPath = Phase410SummaryPath,
        phase418SummaryPath = Phase418SummaryPath,
        phase419SummaryPath = Phase419SummaryPath,
        referenceNote = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "naive_curvature_mass_scale_sanity_check.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "naive_curvature_mass_scale_sanity_check_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"naiveCurvatureMassScaleSanityCheckPassed={naiveCurvatureMassScaleSanityCheckPassed}");
Console.WriteLine($"literalScalarCurvatureMassReadingDimensionallyConsistent={literalScalarCurvatureMassReadingDimensionallyConsistent}");
Console.WriteLine($"squaredMassCurvatureReadingDimensionallyConsistent={squaredMassCurvatureReadingDimensionallyConsistent}");
Console.WriteLine($"providedRequiredScaleSpecificationFieldCount={providedRequiredScaleSpecificationFieldCount}");
Console.WriteLine($"missingScaleSpecificationFieldCount={missingScaleSpecificationFields.Length}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

static string Sha256Hex(string text)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record DimensionalReading(
    string ReadingId,
    string Formula,
    int CurvatureMassDimension,
    int RequiredMassDimension,
    bool DimensionallyConsistent,
    string RepairNeeded,
    string Verdict);

sealed record RequiredScaleField(string FieldId, string Description, bool Provided);

sealed record Check(string CheckId, bool Passed, string Detail);

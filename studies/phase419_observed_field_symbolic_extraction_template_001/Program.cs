using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase419: observed-field symbolic extraction template.
//
// Phase256 already materializes the intake contract, Phase295 verifies that no
// local artifact fills it, and Phases344/365 record FMS/dressing-field methods
// as external observed-field templates rather than GU source laws. This phase
// performs the next target-blind step requested by the restart prompt: assume a
// scalar doublet exists and spell out the symbolic FMS/dressing dependency
// graph needed before photon, W, Z, and Higgs projection rows could be applied
// to Phase256.
//
// Fail-closed: all projection rows are template rows, not source-defined rows.
// The phase writes no Phase201/Phase256 field and promotes no physical mass.

const string DefaultOutputDir = "studies/phase419_observed_field_symbolic_extraction_template_001/output";
const string Phase256SummaryPath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase256TemplatePath = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_template.json";
const string Phase295SummaryPath = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase317SummaryPath = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase344SummaryPath = "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output/fms_gauge_invariant_spectrum_source_audit_summary.json";
const string Phase365SummaryPath = "studies/phase365_dressing_field_electroweak_observed_variables_audit_001/output/dressing_field_electroweak_observed_variables_audit_summary.json";
const string Phase418SummaryPath = "studies/phase418_direction_dependent_curvature_vev_coupling_scan_001/output/direction_dependent_curvature_vev_coupling_scan_summary.json";
const string ApplicationSubjectKind = "fms-dressing-field-observed-extraction-template";

var outputDir = Environment.GetEnvironmentVariable("PHASE419_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256SummaryPath));
using var phase256Template = JsonDocument.Parse(File.ReadAllText(Phase256TemplatePath));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295SummaryPath));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317SummaryPath));
using var phase344 = JsonDocument.Parse(File.ReadAllText(Phase344SummaryPath));
using var phase365 = JsonDocument.Parse(File.ReadAllText(Phase365SummaryPath));
using var phase418 = JsonDocument.Parse(File.ReadAllText(Phase418SummaryPath));

var phase256TemplateFieldIds = ReadTemplateFieldIds(phase256Template.RootElement);

var symbolicMapRows = new[]
{
    new SymbolicMapRow(
        "native-scalar-doublet-carrier",
        "carrier",
        "Provide a GU-native field Phi transforming as an observed SU(2)L doublet with hypercharge convention Y(Phi).",
        new[]
        {
            "observedFieldExtractionTheoremId",
            "sourceReferenceIds",
            "higgsScalarSourceOperatorId",
            "higgsMassiveScalarProfileId",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "A source-derived doublet-equivalent carrier and scalar source/operator, not a Standard Model import."),
    new SymbolicMapRow(
        "observed-vacuum-and-dressing-field",
        "dressing",
        "For Phi != 0, construct a normalized dressing U(Phi) and a radial scalar h = ||Phi|| - v from the source vacuum.",
        new[]
        {
            "canonicalOrDeclaredShiabBranchId",
            "branchNormalizationSourceId",
            "fourDimensionalObservedVacuumArtifactId",
            "targetBlindConstructionHash",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "A GU-derived observed vacuum, branch normalization, and dressing rule fixed before target comparison."),
    new SymbolicMapRow(
        "dressed-charged-weak-fields",
        "projection",
        "Form dressed weak variables W^d_a from U(Phi); define W+ = (W^d_1 - i W^d_2)/sqrt(2), W- = (W^d_1 + i W^d_2)/sqrt(2).",
        new[]
        {
            "electroweakGaugeEmbeddingId",
            "wBosonSourceRowId",
            "wBosonRawAmplitudeGatePassed",
            "stabilitySidecarIds",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "Separate charged W projection/source rows with source replay and stability sidecars."),
    new SymbolicMapRow(
        "dressed-neutral-basis-and-weak-angle",
        "neutral-mixing",
        "Keep the dressed neutral basis (W^d_3, B) and require a source weak-angle relation tan(theta_W)=gPrime/g before defining A and Z.",
        new[]
        {
            "electroweakGaugeEmbeddingId",
            "quadraticElectroweakMassOperatorId",
            "zBosonSourceRowId",
            "zBosonRawAmplitudeGatePassed",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "A GU weak-angle/coupling lineage and neutral quadratic mass operator, not an observed-mass fit."),
    new SymbolicMapRow(
        "photon-projection-row",
        "projection",
        "Define A = cos(theta_W) B + sin(theta_W) W^d_3 and require an exactly massless source row before W/Z promotion.",
        new[]
        {
            "photonEigenstateProjectionId",
            "targetComparisonAfterConstructionGatePassed",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "A massless photon projection row derived from the same neutral mass operator and weak-angle convention."),
    new SymbolicMapRow(
        "z-projection-row",
        "projection",
        "Define Z = cos(theta_W) W^d_3 - sin(theta_W) B and require the Z row to diagonalize the same source mass operator.",
        new[]
        {
            "zBosonSourceRowId",
            "zBosonRawAmplitudeGatePassed",
            "wzCommonBridgeGatePassed",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "A Z projection/source row and common W/Z bridge from source normalization."),
    new SymbolicMapRow(
        "higgs-radial-row",
        "projection",
        "Identify the scalar radial excitation h and its quadratic pole from the source potential or Upsilon pairing.",
        new[]
        {
            "higgsScalarSourceOperatorId",
            "higgsMassiveScalarProfileId",
            "higgsPotentialSelfCouplingRelationId",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "A Higgs scalar source/operator, massive profile, and self-coupling/excitation relation fixed without target masses."),
    new SymbolicMapRow(
        "pole-and-unit-normalization",
        "physical-output",
        "Extract poles of gauge-invariant two-point functions or an equivalent source spectral operator, then attach GeV/unit normalization.",
        new[]
        {
            "quadraticElectroweakMassOperatorId",
            "branchNormalizationSourceId",
            "stabilitySidecarIds",
            "phase201And209ApplicationReady",
        },
        SourceDefined: false,
        ExternalTemplateOnly: true,
        Acceptance: "Pole extraction, unit normalization, and application readiness sidecars for Phase201/209/256."),
};

var projectionRows = new[]
{
    new ProjectionRow(
        "photon",
        "A = cos(theta_W) B + sin(theta_W) W^d_3",
        "massless photon eigenstate; weak-angle and neutral mass-matrix lineage",
        "photonEigenstateProjectionId",
        RequiresPoleExtraction: true,
        SourceDefined: false),
    new ProjectionRow(
        "w-boson",
        "W+/- = (W^d_1 -/+ i W^d_2)/sqrt(2)",
        "charged source rows; raw W amplitude gate; common W/Z bridge",
        "wBosonSourceRowId",
        RequiresPoleExtraction: true,
        SourceDefined: false),
    new ProjectionRow(
        "z-boson",
        "Z = cos(theta_W) W^d_3 - sin(theta_W) B",
        "neutral source row; raw Z amplitude gate; same weak-angle convention as photon",
        "zBosonSourceRowId",
        RequiresPoleExtraction: true,
        SourceDefined: false),
    new ProjectionRow(
        "higgs",
        "h = ||Phi|| - v",
        "scalar source operator; massive profile; self-coupling or excitation relation",
        "higgsMassiveScalarProfileId",
        RequiresPoleExtraction: true,
        SourceDefined: false),
};

var requiredGuInputs = new[]
{
    new RequiredGuInput("gu-native-doublet-carrier", "Native scalar/doublet-equivalent carrier with observed electroweak representation.", Provided: false),
    new RequiredGuInput("source-observed-vacuum", "Four-dimensional observed-sector vacuum and nonzero VEV scale from GU.", Provided: false),
    new RequiredGuInput("native-dressing-field", "Dressing field U(Phi) or equivalent gauge-invariant composite map from GU variables.", Provided: false),
    new RequiredGuInput("electroweak-embedding", "Observed SU(2)L x U(1)Y embedding and hypercharge convention.", Provided: false),
    new RequiredGuInput("weak-angle-coupling-lineage", "Target-independent g, gPrime, and weak-angle relation with transport to the mass shell.", Provided: false),
    new RequiredGuInput("quadratic-mass-operator", "Gauge-invariant quadratic mass/pole operator around the observed vacuum.", Provided: false),
    new RequiredGuInput("wz-source-rows", "Separate W and Z source rows plus common bridge normalization.", Provided: false),
    new RequiredGuInput("photon-source-row", "Massless photon projection row from the same neutral operator.", Provided: false),
    new RequiredGuInput("higgs-source-row", "Scalar source/operator, massive profile, and self-coupling/excitation lineage.", Provided: false),
    new RequiredGuInput("pole-and-gev-normalization", "Pole extraction, units, stability sidecars, and GeV normalization.", Provided: false),
};

var coveredPhase256Fields = symbolicMapRows
    .SelectMany(row => row.RequiredPhase256FieldIds)
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();
var uncoveredPhase256FieldIds = phase256TemplateFieldIds
    .Except(coveredPhase256Fields, StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();
var extraPhase256FieldIds = coveredPhase256Fields
    .Except(phase256TemplateFieldIds, StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();

int sourceDefinedPhase256FieldCount = symbolicMapRows
    .Where(row => row.SourceDefined)
    .SelectMany(row => row.RequiredPhase256FieldIds)
    .Distinct(StringComparer.Ordinal)
    .Count();
int externalTemplateOnlyRowCount = symbolicMapRows.Count(row => row.ExternalTemplateOnly);
int requiredGuInputProvidedCount = requiredGuInputs.Count(input => input.Provided);
int projectionRowsWithPoleExtractionRequirementCount = projectionRows.Count(row => row.RequiresPoleExtraction);
bool allPhase256FieldsMappedBySymbolicTemplate = uncoveredPhase256FieldIds.Length == 0
    && extraPhase256FieldIds.Length == 0
    && coveredPhase256Fields.Length == phase256TemplateFieldIds.Length;
bool allProjectionRowsPresent = projectionRows.Select(row => row.ParticleId).Order(StringComparer.Ordinal).SequenceEqual(new[] { "higgs", "photon", "w-boson", "z-boson" });
bool allProjectionRowsRequirePoleExtraction = projectionRowsWithPoleExtractionRequirementCount == projectionRows.Length;
bool anyProjectionRowSourceDefined = projectionRows.Any(row => row.SourceDefined);

var phase256RequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var phase256FilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var phase256ContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase295CandidateScanPassed = JsonBool(phase295.RootElement, "observedFieldExtractionContractCandidateScanPassed") is true;
var phase295IntakeReadyCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var phase295AnyCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var phase317MassMatrixBridgePassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var phase317ExternalDependencyMap = JsonBool(phase317.RootElement, "smMassMatrixProvidesExternalDependencyMap") is true;
var phase317PromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var phase317PromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var phase344FmsPassed = JsonBool(phase344.RootElement, "fmsGaugeInvariantSpectrumSourceAuditPassed") is true;
var phase344FmsTemplate = JsonBool(phase344.RootElement, "fmsProvidesObservedFieldExtractionTemplate") is true;
var phase344PromotesObservedExtraction = JsonBool(phase344.RootElement, "fmsRoutePromotesObservedFieldExtraction") is true;
var phase365DressingPassed = JsonBool(phase365.RootElement, "dressingFieldElectroweakObservedVariablesAuditPassed") is true;
var phase365ExternalTemplate = JsonBool(phase365.RootElement, "routeProvidesExternalObservedFieldExtractionTemplate") is true;
var phase365PromotesObservedExtraction = JsonBool(phase365.RootElement, "routePromotesObservedFieldExtraction") is true;
var phase418Passed = JsonBool(phase418.RootElement, "directionDependentCurvatureVevCouplingScanPassed") is true;
var phase418CanFillObserved = JsonBool(phase418.RootElement, "canFillPhase256ObservedFieldExtractionContract") is true;

bool fmsDressingExternalTemplateSupport = phase317MassMatrixBridgePassed
    && phase317ExternalDependencyMap
    && phase344FmsPassed
    && phase344FmsTemplate
    && phase365DressingPassed
    && phase365ExternalTemplate;
bool fmsDressingExternalTemplatesPromotional = phase317PromotesWzMasses
    || phase317PromotesHiggsMass
    || phase344PromotesObservedExtraction
    || phase365PromotesObservedExtraction;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
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
const bool symbolicTemplatePromotable = false;

var targetBlindConstructionHash = Sha256Hex(JsonSerializer.Serialize(new
{
    ApplicationSubjectKind,
    symbolicMapRows,
    projectionRows,
    requiredGuInputs,
    phase256TemplateFieldIds,
}, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

var checks = new[]
{
    new Check(
        "phase256-contract-loaded-and-still-unfilled",
        phase256RequiredFieldCount == phase256TemplateFieldIds.Length
            && phase256RequiredFieldCount == 20
            && phase256FilledRequiredFieldCount == 0
            && !phase256ContractPromotable,
        $"phase256Required={phase256RequiredFieldCount}; templateFields={phase256TemplateFieldIds.Length}; phase256Filled={phase256FilledRequiredFieldCount}; phase256Promotable={phase256ContractPromotable}"),
    new Check(
        "phase295-scan-preserves-no-intake-ready-artifact",
        phase295CandidateScanPassed
            && phase295IntakeReadyCandidateCount == 0
            && !phase295AnyCandidateFillsContract,
        $"phase295Passed={phase295CandidateScanPassed}; intakeReady={phase295IntakeReadyCandidateCount}; anyFills={phase295AnyCandidateFillsContract}"),
    new Check(
        "fms-dressing-external-template-support-loaded",
        fmsDressingExternalTemplateSupport
            && !fmsDressingExternalTemplatesPromotional,
        $"support={fmsDressingExternalTemplateSupport}; promotional={fmsDressingExternalTemplatesPromotional}; p317ExternalDependency={phase317ExternalDependencyMap}; fmsTemplate={phase344FmsTemplate}; dressingTemplate={phase365ExternalTemplate}"),
    new Check(
        "phase418-boundary-preserved",
        phase418Passed && !phase418CanFillObserved,
        $"phase418Passed={phase418Passed}; phase418CanFillObserved={phase418CanFillObserved}"),
    new Check(
        "symbolic-map-covers-phase256-template",
        allPhase256FieldsMappedBySymbolicTemplate
            && symbolicMapRows.Length == 8
            && coveredPhase256Fields.Length == 20,
        $"rows={symbolicMapRows.Length}; covered={coveredPhase256Fields.Length}; missing={uncoveredPhase256FieldIds.Length}; extra={extraPhase256FieldIds.Length}"),
    new Check(
        "projection-rows-present-but-source-undefined",
        allProjectionRowsPresent
            && allProjectionRowsRequirePoleExtraction
            && !anyProjectionRowSourceDefined
            && projectionRows.Length == 4,
        $"projectionRows={projectionRows.Length}; allRequirePole={allProjectionRowsRequirePoleExtraction}; anySourceDefined={anyProjectionRowSourceDefined}"),
    new Check(
        "required-gu-inputs-not-supplied-by-template",
        requiredGuInputs.Length == 10
            && requiredGuInputProvidedCount == 0
            && sourceDefinedPhase256FieldCount == 0
            && externalTemplateOnlyRowCount == symbolicMapRows.Length,
        $"requiredGuInputs={requiredGuInputs.Length}; provided={requiredGuInputProvidedCount}; sourceDefinedPhase256FieldCount={sourceDefinedPhase256FieldCount}; externalTemplateRows={externalTemplateOnlyRowCount}"),
    new Check(
        "no-contract-application-or-promotion",
        targetBlindConstruction
            && !physicalTargetsConsultedForConstruction
            && targetBlindConstructionHash.Length == 64
            && !symbolicTemplatePromotable
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
        $"targetBlind={targetBlindConstruction}; consultedTargets={physicalTargetsConsultedForConstruction}; hashLength={targetBlindConstructionHash.Length}; promotable={symbolicTemplatePromotable}; canFillWz={canFillPhase201WzContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}; phase201Mutated={phase201TemplateMutated}; fieldsApplied={fieldsAppliedToPhase201TemplateCount}"),
};

var observedFieldSymbolicExtractionTemplatePassed = checks.All(check => check.Passed);
var terminalStatus = observedFieldSymbolicExtractionTemplatePassed
    ? "observed-field-symbolic-extraction-template-materialized-contract-unfilled"
    : "observed-field-symbolic-extraction-template-review-required";

var template = new
{
    templateId = "observed-field-symbolic-fms-dressing-template-v1",
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    status = "symbolic-template-only",
    symbolicMapRows,
    projectionRows,
    requiredGuInputs,
    coveredPhase256Fields,
    uncoveredPhase256FieldIds,
    extraPhase256FieldIds,
    applicationInstructions = new[]
    {
        "Treat every row as a template row until a GU-native source supplies the corresponding field.",
        "Do not apply this template to Phase201 or Phase256 without source-defined carrier, vacuum, embedding, weak-angle, source rows, pole extraction, and unit normalization.",
        "After a source artifact fills the rows, run Phase201/209/210/213/256 and the full boson generator before any target comparison.",
    },
};

var result = new
{
    phaseId = "phase419-observed-field-symbolic-extraction-template",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    observedFieldSymbolicExtractionTemplatePassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    symbolicTemplatePromotable,
    fmsDressingExternalTemplateSupport,
    fmsDressingExternalTemplatesPromotional,
    symbolicMapRowCount = symbolicMapRows.Length,
    projectionRowCount = projectionRows.Length,
    allProjectionRowsPresent,
    allProjectionRowsRequirePoleExtraction,
    anyProjectionRowSourceDefined,
    requiredGuInputCount = requiredGuInputs.Length,
    requiredGuInputProvidedCount,
    phase256TemplateFieldCount = phase256TemplateFieldIds.Length,
    phase256RequiredFieldCount,
    phase256FilledRequiredFieldCount,
    phase256ContractPromotable,
    coveredPhase256FieldCount = coveredPhase256Fields.Length,
    sourceDefinedPhase256FieldCount,
    externalTemplateOnlyRowCount,
    allPhase256FieldsMappedBySymbolicTemplate,
    uncoveredPhase256FieldIds,
    extraPhase256FieldIds,
    phase295CandidateScanPassed,
    phase295IntakeReadyCandidateCount,
    phase295AnyCandidateFillsContract,
    phase317MassMatrixBridgePassed,
    phase317ExternalDependencyMap,
    phase317PromotesWzMasses,
    phase317PromotesHiggsMass,
    phase344FmsPassed,
    phase344FmsTemplate,
    phase344PromotesObservedExtraction,
    phase365DressingPassed,
    phase365ExternalTemplate,
    phase365PromotesObservedExtraction,
    phase418Passed,
    phase418CanFillObserved,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    checks,
    decision = observedFieldSymbolicExtractionTemplatePassed
        ? "The FMS/dressing-style observed-field map is now explicit as a target-blind symbolic template, but it supplies zero source-defined Phase256 fields and cannot promote W/Z/H predictions."
        : "Review the symbolic extraction template before relying on it.",
    nextRequiredArtifact = new[]
    {
        "A GU-native scalar/doublet-equivalent carrier or dressing field derived from source geometry.",
        "A source-derived observed vacuum, electroweak embedding, weak-angle/coupling lineage, and quadratic mass operator.",
        "Photon/W/Z/H projection rows with pole extraction, stability sidecars, target-independent scale, and GeV/unit normalization.",
    },
    templatePath = Path.Combine(outputDir, "observed_field_symbolic_extraction_template_map.json"),
    sourceEvidence = new
    {
        phase256SummaryPath = Phase256SummaryPath,
        phase256TemplatePath = Phase256TemplatePath,
        phase295SummaryPath = Phase295SummaryPath,
        phase317SummaryPath = Phase317SummaryPath,
        phase344SummaryPath = Phase344SummaryPath,
        phase365SummaryPath = Phase365SummaryPath,
        phase418SummaryPath = Phase418SummaryPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "observed_field_symbolic_extraction_template_map.json"), JsonSerializer.Serialize(template, options));
File.WriteAllText(Path.Combine(outputDir, "observed_field_symbolic_extraction_template.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "observed_field_symbolic_extraction_template_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.observedFieldSymbolicExtractionTemplatePassed,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        result.physicalTargetsConsultedForConstruction,
        result.targetBlindConstructionHash,
        result.symbolicTemplatePromotable,
        result.fmsDressingExternalTemplateSupport,
        result.fmsDressingExternalTemplatesPromotional,
        result.symbolicMapRowCount,
        result.projectionRowCount,
        result.allProjectionRowsPresent,
        result.allProjectionRowsRequirePoleExtraction,
        result.anyProjectionRowSourceDefined,
        result.requiredGuInputCount,
        result.requiredGuInputProvidedCount,
        result.phase256TemplateFieldCount,
        result.phase256RequiredFieldCount,
        result.phase256FilledRequiredFieldCount,
        result.phase256ContractPromotable,
        result.coveredPhase256FieldCount,
        result.sourceDefinedPhase256FieldCount,
        result.externalTemplateOnlyRowCount,
        result.allPhase256FieldsMappedBySymbolicTemplate,
        result.uncoveredPhase256FieldIds,
        result.extraPhase256FieldIds,
        result.phase295CandidateScanPassed,
        result.phase295IntakeReadyCandidateCount,
        result.phase295AnyCandidateFillsContract,
        result.phase317MassMatrixBridgePassed,
        result.phase317ExternalDependencyMap,
        result.phase317PromotesWzMasses,
        result.phase317PromotesHiggsMass,
        result.phase344FmsPassed,
        result.phase344FmsTemplate,
        result.phase344PromotesObservedExtraction,
        result.phase365DressingPassed,
        result.phase365ExternalTemplate,
        result.phase365PromotesObservedExtraction,
        result.phase418Passed,
        result.phase418CanFillObserved,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.phase201TemplateMutated,
        result.fieldsAppliedToPhase201TemplateCount,
        result.acceptedContractFieldCount,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
        result.templatePath,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"observedFieldSymbolicExtractionTemplatePassed={observedFieldSymbolicExtractionTemplatePassed}");
Console.WriteLine($"symbolicMapRowCount={symbolicMapRows.Length}");
Console.WriteLine($"projectionRowCount={projectionRows.Length}");
Console.WriteLine($"allPhase256FieldsMappedBySymbolicTemplate={allPhase256FieldsMappedBySymbolicTemplate}");
Console.WriteLine($"sourceDefinedPhase256FieldCount={sourceDefinedPhase256FieldCount}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

static string[] ReadTemplateFieldIds(JsonElement template)
{
    if (!template.TryGetProperty("requirementRows", out var rows) || rows.ValueKind != JsonValueKind.Array)
        return Array.Empty<string>();

    return rows.EnumerateArray()
        .Select(row => JsonString(row, "fieldId"))
        .OfType<string>()
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static string Sha256Hex(string input)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexString(bytes).ToLowerInvariant();
}

sealed record SymbolicMapRow(
    string RowId,
    string Stage,
    string AlgebraicExpression,
    string[] RequiredPhase256FieldIds,
    bool SourceDefined,
    bool ExternalTemplateOnly,
    string Acceptance);

sealed record ProjectionRow(
    string ParticleId,
    string SymbolicField,
    string RequiredInputs,
    string PrimaryPhase256FieldId,
    bool RequiresPoleExtraction,
    bool SourceDefined);

sealed record RequiredGuInput(string InputId, string Acceptance, bool Provided);

sealed record Check(string CheckId, bool Passed, string Detail);

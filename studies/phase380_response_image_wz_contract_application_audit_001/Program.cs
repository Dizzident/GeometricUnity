using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase380_response_image_wz_contract_application_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase209WzRequestPath = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/wz_absolute_source_lineage_evidence_request.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase311Path = "studies/phase311_completion_observed_sector_wz_row_selector_audit_001/output/completion_observed_sector_wz_row_selector_audit_summary.json";
const string Phase379Path = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const int ExpectedWzMissingFieldCount = 15;

var outputDir = Environment.GetEnvironmentVariable("PHASE380_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase209WzRequest = JsonDocument.Parse(File.ReadAllText(Phase209WzRequestPath));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase311 = JsonDocument.Parse(File.ReadAllText(Phase311Path));
using var phase379 = JsonDocument.Parse(File.ReadAllText(Phase379Path));

var p201WzValidation = phase201.RootElement.GetProperty("wzValidation");
var p210WzApplication = phase210.RootElement.GetProperty("wzApplication");
var p379Carrier = phase379.RootElement.GetProperty("carrier");
var p379PredictionContractImpact = phase379.RootElement.GetProperty("predictionContractImpact");
var p209AcceptanceGates = JsonStringArray(phase209WzRequest.RootElement, "acceptanceGates");
var p213MissingFields = JsonStringArray(phase213.RootElement, "wzMissingFields");

bool phase379Passed = JsonBool(phase379.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true;
bool phase379TargetBlind = JsonBool(phase379.RootElement, "targetBlindConstruction") is true
    && JsonBool(phase379.RootElement, "physicalTargetsConsultedForConstruction") is false;
bool phase379RankThreeCarrier = JsonInt(p379Carrier, "carrierDimension") == 156
    && JsonInt(p379Carrier, "edgeCount") == 52
    && JsonInt(p379Carrier, "gaugeDimension") == 3
    && JsonInt(phase379.RootElement, "backgroundPassedCount") == 2
    && JsonInt(phase379.RootElement, "backgroundCount") == 2;
int suppressedAxis = JsonInt(phase379.RootElement, "stableSuppressedGaugeAxis") ?? -1;
int[] dominantAxes = Enumerable.Range(0, 3).Where(axis => axis != suppressedAxis).ToArray();

bool phase379StrictTransportPassed = JsonBool(phase379.RootElement, "strictBackgroundImageTransportPassed") is true;
bool phase379LooseTransportPassed = JsonBool(phase379.RootElement, "looseBackgroundImageTransportPassed") is true;
bool phase379ProvidesCanonicalGaugeAxisSelector = JsonBool(phase379.RootElement, "routeProvidesCanonicalGaugeAxisSelector") is true;
bool phase379ProvidesObservedElectroweakFieldMap = JsonBool(phase379.RootElement, "routeProvidesObservedElectroweakFieldMap") is true;
bool phase379ProvidesPhysicalEffectiveActionHessian = JsonBool(phase379.RootElement, "routeProvidesPhysicalEffectiveActionHessian") is true;
bool phase379ProvidesDirectWzLaw = JsonBool(phase379.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") is true;
bool phase379ProvidesSeparateWzRows = JsonBool(phase379.RootElement, "routeProvidesSeparateWzSourceRows") is true;
bool phase379ProvidesGeVNormalization = JsonBool(phase379.RootElement, "routeProvidesGeVUnitNormalization") is true;
bool phase379CanFillPhase201WzContract = JsonBool(phase379.RootElement, "canFillPhase201WzContract") is true;
bool phase379PromotesWzMasses = JsonBool(phase379.RootElement, "routePromotesWzMasses") is true;
bool phase379SourceLawPromotionBlocked = JsonBool(phase379.RootElement, "sourceLawPromotionBlocked") is true;

bool phase201WzPromotable = JsonBool(p201WzValidation, "promotable") is true;
bool phase210WzReady = JsonBool(p210WzApplication, "readyForApplication") is true;
int phase213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int phase209WzCurrentMissingFieldCount = JsonInt(phase209WzRequest.RootElement, "currentMissingFieldCount") ?? -1;
bool phase295AnyObservedCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
bool phase307SourceRowsPromotable = JsonBool(phase307.RootElement, "sourceRowsPromotable") is true;
bool phase307CanFillPhase201WzContract = JsonBool(phase307.RootElement, "canFillPhase201WzContract") is true;
bool phase311CanFillPhase201WzContract = JsonBool(phase311.RootElement, "canFillPhase201WzContract") is true;

var targetBlindHash = HashText(string.Join(
    "\n",
    "phase380-response-image-wz-contract-application-audit-v1",
    $"phase379Hash={JsonString(phase379.RootElement, "targetBlindConstructionHash")}",
    $"phase379Path={Phase379Path}",
    $"phase201Path={Phase201Path}",
    "applicationSubject=Phase379 positive response image diagnostic",
    "application=Phase201 W/Z source-lineage contract",
    "templateMutation=false",
    "physicalTargetsConsultedForConstruction=false"));

var contractFieldAssessments = new[]
{
    Field(
        "externalTargetValuesUsed=false",
        CandidateSignalPresent: phase379TargetBlind,
        AcceptedForContract: false,
        Evidence: "Phase379 construction is target-blind and declares physicalTargetsConsultedForConstruction=false.",
        Blocker: "The Phase201 field is not applied because target blindness alone is not a complete source-lineage artifact with theorem, source rows, and gates."),
    Field(
        "theoremOrDerivationId",
        CandidateSignalPresent: false,
        AcceptedForContract: false,
        Evidence: $"phase379ProvidesDirectWzLaw={phase379ProvidesDirectWzLaw}; phase379SourceLawPromotionBlocked={phase379SourceLawPromotionBlocked}.",
        Blocker: "Phase379 characterizes an eigenspace but supplies no theorem or derivation turning it into a physical W/Z bridge-source law."),
    Field(
        "sourceLineageId",
        CandidateSignalPresent: false,
        AcceptedForContract: false,
        Evidence: "The Phase379 artifact is a diagnostic source-evidence path, not a promoted W/Z source lineage.",
        Blocker: "A sourceLineageId cannot be minted from a non-promotional response-image diagnostic."),
    RowField("w-boson", "sourceRowId", phase379ProvidesSeparateWzRows, "Phase379 has no separate W row.", "No W sourceRowId is derived from the response image."),
    RowField("w-boson", "rawAmplitudeGatePassed=true", false, "Phase379 computes projector capture and transport, not raw W amplitudes.", "No W raw-amplitude gate is available."),
    RowField("w-boson", "commonBridgeGatePassed=true", false, "Phase379 lacks source-derived common W/Z bridge normalization.", "No W common-bridge gate is available."),
    RowField("w-boson", "targetComparisonGatePassed=true", false, "Phase379 never constructs a W mass row for post-construction target comparison.", "No W target-comparison gate is runnable."),
    RowField("w-boson", "stabilitySidecarsPresent=true", phase379LooseTransportPassed, $"Phase379 loose transport passed but strict transport={phase379StrictTransportPassed}.", "Loose carrier-image transport is not the Phase201 branch/refinement/environment/representation/coupling sidecar set."),
    RowField("w-boson", "derivationId", false, "No W derivation id is present.", "A derivation id requires a derived W source row."),
    RowField("z-boson", "sourceRowId", phase379ProvidesSeparateWzRows, "Phase379 has no separate Z row.", "No Z sourceRowId is derived from the response image."),
    RowField("z-boson", "rawAmplitudeGatePassed=true", false, "Phase379 computes projector capture and transport, not raw Z amplitudes.", "No Z raw-amplitude gate is available."),
    RowField("z-boson", "commonBridgeGatePassed=true", false, "Phase379 lacks source-derived common W/Z bridge normalization.", "No Z common-bridge gate is available."),
    RowField("z-boson", "targetComparisonGatePassed=true", false, "Phase379 never constructs a Z mass row for post-construction target comparison.", "No Z target-comparison gate is runnable."),
    RowField("z-boson", "stabilitySidecarsPresent=true", phase379LooseTransportPassed, $"Phase379 loose transport passed but strict transport={phase379StrictTransportPassed}.", "Loose carrier-image transport is not the Phase201 branch/refinement/environment/representation/coupling sidecar set."),
    RowField("z-boson", "derivationId", false, "No Z derivation id is present.", "A derivation id requires a derived Z source row."),
};

int acceptedContractFieldCount = contractFieldAssessments.Count(row => row.AcceptedForContract);
int diagnosticSignalButNotAppliedFieldCount = contractFieldAssessments.Count(row => row.CandidateSignalPresent && !row.AcceptedForContract);
int blockedContractFieldCount = contractFieldAssessments.Count(row => !row.AcceptedForContract);
bool phase201TemplateMutated = false;
int fieldsAppliedToPhase201TemplateCount = 0;
bool canFillPhase201WzContract = false;
bool sourceContractApplicationAllowed = false;

var checks = new[]
{
    Check(
        "phase379-response-image-present-target-blind-rank-three",
        phase379Passed && phase379TargetBlind && phase379RankThreeCarrier,
        $"phase379Passed={phase379Passed}; targetBlind={phase379TargetBlind}; rankThreeCarrier={phase379RankThreeCarrier}; suppressedAxis={suppressedAxis}; dominantAxes={string.Join(",", dominantAxes)}"),
    Check(
        "phase379-is-diagnostic-not-source-law",
        !phase379ProvidesCanonicalGaugeAxisSelector
            && !phase379ProvidesObservedElectroweakFieldMap
            && !phase379ProvidesPhysicalEffectiveActionHessian
            && !phase379ProvidesDirectWzLaw
            && !phase379ProvidesSeparateWzRows
            && !phase379ProvidesGeVNormalization
            && phase379SourceLawPromotionBlocked,
        $"canonicalGaugeAxisSelector={phase379ProvidesCanonicalGaugeAxisSelector}; observedElectroweakFieldMap={phase379ProvidesObservedElectroweakFieldMap}; physicalHessian={phase379ProvidesPhysicalEffectiveActionHessian}; directWzLaw={phase379ProvidesDirectWzLaw}; separateRows={phase379ProvidesSeparateWzRows}; gevNormalization={phase379ProvidesGeVNormalization}; sourceLawPromotionBlocked={phase379SourceLawPromotionBlocked}"),
    Check(
        "phase379-background-identity-not-strict",
        !phase379StrictTransportPassed && phase379LooseTransportPassed,
        $"strictTransport={phase379StrictTransportPassed}; looseTransport={phase379LooseTransportPassed}; minSingular={JsonDouble(phase379.RootElement, "interBackgroundMinimumSingularValue")}"),
    Check(
        "observed-row-and-selector-blockers-preserved",
        !phase295AnyObservedCandidateFillsContract
            && !phase307SourceRowsPromotable
            && !phase307CanFillPhase201WzContract
            && !phase311CanFillPhase201WzContract,
        $"phase295AnyObservedCandidateFillsContract={phase295AnyObservedCandidateFillsContract}; phase307SourceRowsPromotable={phase307SourceRowsPromotable}; phase307CanFillPhase201WzContract={phase307CanFillPhase201WzContract}; phase311CanFillPhase201WzContract={phase311CanFillPhase201WzContract}"),
    Check(
        "phase201-wz-contract-remains-unfilled",
        !phase201WzPromotable
            && !phase210WzReady
            && phase213WzMissingFieldCount == ExpectedWzMissingFieldCount
            && phase209WzCurrentMissingFieldCount == ExpectedWzMissingFieldCount,
        $"phase201WzPromotable={phase201WzPromotable}; phase210WzReady={phase210WzReady}; phase213WzMissingFieldCount={phase213WzMissingFieldCount}; phase209WzCurrentMissingFieldCount={phase209WzCurrentMissingFieldCount}"),
    Check(
        "phase201-template-not-mutated",
        !phase201TemplateMutated && fieldsAppliedToPhase201TemplateCount == 0,
        $"phase201TemplateMutated={phase201TemplateMutated}; fieldsAppliedToPhase201TemplateCount={fieldsAppliedToPhase201TemplateCount}"),
    Check(
        "no-contract-fields-accepted-from-response-image",
        acceptedContractFieldCount == 0
            && blockedContractFieldCount == ExpectedWzMissingFieldCount
            && !sourceContractApplicationAllowed
            && !canFillPhase201WzContract
            && !phase379CanFillPhase201WzContract
            && !phase379PromotesWzMasses,
        $"acceptedContractFieldCount={acceptedContractFieldCount}; blockedContractFieldCount={blockedContractFieldCount}; sourceContractApplicationAllowed={sourceContractApplicationAllowed}; canFillPhase201WzContract={canFillPhase201WzContract}; phase379CanFillPhase201WzContract={phase379CanFillPhase201WzContract}; phase379PromotesWzMasses={phase379PromotesWzMasses}"),
};

bool responseImageWzContractApplicationAuditPassed = checks.All(check => check.Passed)
    && p213MissingFields.Length == ExpectedWzMissingFieldCount
    && p209AcceptanceGates.Length >= 8
    && ArrayFromJson(p379PredictionContractImpact, "phase201FieldsDefensiblyFilled").Length == 0;

string terminalStatus = responseImageWzContractApplicationAuditPassed
    ? "response-image-wz-contract-application-audit-blocked-no-source-rows"
    : "response-image-wz-contract-application-audit-review-required";

string decision = responseImageWzContractApplicationAuditPassed
    ? "The Phase379 rank-three response image was stress-tested against the Phase201 W/Z source-lineage contract and cannot be applied. It remains a diagnostic eigenspace, not a source-row candidate: it lacks a theorem, strict background identity, observed electroweak field map, separate W/Z rows, raw-amplitude gates, common bridge, target-comparison rows, derivation ids, and GeV normalization. The Phase201 template remains unmutated."
    : "Review the Phase379-to-Phase201 application attempt before using response-image data in W/Z source-lineage work.";

var result = new
{
    phaseId = "phase380-response-image-wz-contract-application-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    responseImageWzContractApplicationAuditPassed,
    applicationAttempted = true,
    applicationSubjectKind = "phase379-response-image-diagnostic",
    targetBlindConstruction = phase379TargetBlind,
    physicalTargetsConsultedForConstruction = false,
    targetBlindConstructionHash = targetBlindHash,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    contractFieldCount = contractFieldAssessments.Length,
    acceptedContractFieldCount,
    diagnosticSignalButNotAppliedFieldCount,
    blockedContractFieldCount,
    phase209WzAcceptanceGateCount = p209AcceptanceGates.Length,
    phase209WzCurrentMissingFieldCount,
    phase213WzMissingFieldCount,
    phase213WzMissingFields = p213MissingFields,
    applicationSubject = new
    {
        phase379Path = Phase379Path,
        phase379Passed,
        phase379TargetBlind,
        phase379RankThreeCarrier,
        suppressedGaugeAxis = suppressedAxis,
        dominantGaugeAxes = dominantAxes,
        maxSuppressedGaugeAxisProjectorFraction = JsonDouble(phase379.RootElement, "maxSuppressedGaugeAxisProjectorFraction"),
        minDominantGaugePairProjectorFraction = JsonDouble(phase379.RootElement, "minDominantGaugePairProjectorFraction"),
        strictBackgroundImageTransportPassed = phase379StrictTransportPassed,
        looseBackgroundImageTransportPassed = phase379LooseTransportPassed,
        interBackgroundMinimumSingularValue = JsonDouble(phase379.RootElement, "interBackgroundMinimumSingularValue"),
        canonicalGaugeAxisSelectorPresent = phase379ProvidesCanonicalGaugeAxisSelector,
        observedElectroweakFieldMapPresent = phase379ProvidesObservedElectroweakFieldMap,
        physicalEffectiveActionHessianPresent = phase379ProvidesPhysicalEffectiveActionHessian,
        directTargetIndependentWzBridgeSourceLawPresent = phase379ProvidesDirectWzLaw,
        separateWzSourceRowsPresent = phase379ProvidesSeparateWzRows,
        rawAmplitudeGateAvailable = false,
        commonBridgeGateAvailable = false,
        targetComparisonRowsAvailable = false,
        derivationIdsAvailable = false,
        gevNormalizationPresent = phase379ProvidesGeVNormalization,
    },
    inheritedBlockers = new
    {
        phase201WzPromotable,
        phase210WzReady,
        phase213WzMissingFieldCount,
        phase295AnyObservedCandidateFillsContract,
        phase307SourceRowsPromotable,
        phase307CanFillPhase201WzContract,
        phase311CanFillPhase201WzContract,
    },
    contractFieldAssessments,
    checks,
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase209WzRequestPath = Phase209WzRequestPath,
        phase210Path = Phase210Path,
        phase213Path = Phase213Path,
        phase295Path = Phase295Path,
        phase307Path = Phase307Path,
        phase311Path = Phase311Path,
        phase379Path = Phase379Path,
    },
    decision,
};

string fullPath = Path.Combine(outputDir, "response_image_wz_contract_application_audit.json");
string summaryPath = Path.Combine(outputDir, "response_image_wz_contract_application_audit_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    result.terminalStatus,
    responseImageWzContractApplicationAuditPassed,
    result.applicationAttempted,
    result.applicationSubjectKind,
    result.targetBlindConstruction,
    result.physicalTargetsConsultedForConstruction,
    result.targetBlindConstructionHash,
    result.sourceContractApplicationAllowed,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.phase201TemplateMutated,
    result.fieldsAppliedToPhase201TemplateCount,
    result.phase201FieldsDefensiblyFilled,
    result.contractFieldCount,
    result.acceptedContractFieldCount,
    result.diagnosticSignalButNotAppliedFieldCount,
    result.blockedContractFieldCount,
    result.phase209WzAcceptanceGateCount,
    result.phase209WzCurrentMissingFieldCount,
    result.phase213WzMissingFieldCount,
    result.phase213WzMissingFields,
    result.applicationSubject,
    result.inheritedBlockers,
    result.decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"responseImageWzContractApplicationAuditPassed={responseImageWzContractApplicationAuditPassed}");
Console.WriteLine($"phase379Passed={phase379Passed}");
Console.WriteLine($"targetBlindConstruction={phase379TargetBlind}");
Console.WriteLine($"acceptedContractFieldCount={acceptedContractFieldCount}");
Console.WriteLine($"blockedContractFieldCount={blockedContractFieldCount}");
Console.WriteLine($"phase213WzMissingFieldCount={phase213WzMissingFieldCount}");
Console.WriteLine($"phase201TemplateMutated={phase201TemplateMutated}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static ContractFieldAssessment Field(string field, bool CandidateSignalPresent, bool AcceptedForContract, string Evidence, string Blocker) =>
    new(field, CandidateSignalPresent, AcceptedForContract, Evidence, Blocker);

static ContractFieldAssessment RowField(string particleId, string field, bool CandidateSignalPresent, string Evidence, string Blocker) =>
    Field($"{particleId}.{field}", CandidateSignalPresent, AcceptedForContract: false, Evidence, Blocker);

static Check Check(string id, bool passed, string evidence) => new(id, passed, evidence);

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

static string[] ArrayFromJson(JsonElement element, string propertyName) => JsonStringArray(element, propertyName);

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) &&
    property.ValueKind == JsonValueKind.Number &&
    property.TryGetInt32(out int value)
        ? value
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) &&
    property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : null;

static string JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? ""
        : "";

static string HashText(string text) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

public sealed record ContractFieldAssessment(
    string Field,
    bool CandidateSignalPresent,
    bool AcceptedForContract,
    string Evidence,
    string Blocker);

public sealed record Check(string Id, bool Passed, string Evidence);

using System.Text.Json;

const string DefaultOutputDir = "studies/phase213_boson_source_lineage_blocker_matrix_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase204Path = "studies/phase204_boson_source_lineage_candidate_scan_001/output/boson_source_lineage_candidate_scan_summary.json";
const string Phase205Path = "studies/phase205_boson_source_lineage_text_evidence_scan_001/output/boson_source_lineage_text_evidence_scan_summary.json";
const string Phase208Path = "studies/phase208_boson_local_route_exhaustion_certificate_001/output/boson_local_route_exhaustion_certificate_summary.json";
const string Phase209Path = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/boson_source_lineage_evidence_request_package_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE213_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase204 = JsonDocument.Parse(File.ReadAllText(Phase204Path));
using var phase205 = JsonDocument.Parse(File.ReadAllText(Phase205Path));
using var phase208 = JsonDocument.Parse(File.ReadAllText(Phase208Path));
using var phase209 = JsonDocument.Parse(File.ReadAllText(Phase209Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));

var wzTemplatePath = RequiredString(phase201.RootElement, "wzTemplatePath");
var higgsTemplatePath = RequiredString(phase201.RootElement, "higgsTemplatePath");
var wzRequestPath = RequiredString(phase209.RootElement, "wzRequestPath");
var higgsRequestPath = RequiredString(phase209.RootElement, "higgsRequestPath");

using var wzTemplate = JsonDocument.Parse(File.ReadAllText(wzTemplatePath));
using var higgsTemplate = JsonDocument.Parse(File.ReadAllText(higgsTemplatePath));
using var wzRequest = JsonDocument.Parse(File.ReadAllText(wzRequestPath));
using var higgsRequest = JsonDocument.Parse(File.ReadAllText(higgsRequestPath));

var wzApplication = RequiredObject(phase210.RootElement, "wzApplication");
var higgsApplication = RequiredObject(phase210.RootElement, "higgsApplication");
var jsonIntakeReadyCount = JsonInt(phase204.RootElement, "intakeReadyCandidateCount") ?? 0;
var textIntakeReadyCount = JsonInt(phase205.RootElement, "intakeReadyFindingCount") ?? 0;
var localRouteExhaustionCertified = JsonBool(phase208.RootElement, "localRouteExhaustionCertified") is true;
var rerunPromotionAllowed = JsonBool(phase210.RootElement, "rerunPromotionAllowed") is true;

var wzMissingFields = MissingWzFields(wzTemplate.RootElement);
var higgsMissingFields = MissingHiggsFields(higgsTemplate.RootElement);
var existingEvidenceFound = jsonIntakeReadyCount > 0 || textIntakeReadyCount > 0;
var blockerMatrixReady = !rerunPromotionAllowed
    && !existingEvidenceFound
    && localRouteExhaustionCertified
    && wzMissingFields.Length > 0
    && higgsMissingFields.Length > 0;

var terminalStatus = blockerMatrixReady
    ? "boson-source-lineage-blocker-matrix-ready-new-evidence-required"
    : "boson-source-lineage-blocker-matrix-indeterminate";

var result = new
{
    phaseId = "phase213-boson-source-lineage-blocker-matrix",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    blockerMatrixReady,
    rerunPromotionAllowed,
    existingEvidenceFound,
    localRouteExhaustionCertified,
    wzMissingFieldCount = wzMissingFields.Length,
    higgsMissingFieldCount = higgsMissingFields.Length,
    scans = new
    {
        jsonIntakeReadyCount,
        textIntakeReadyCount,
        jsonScanDecision = JsonString(phase204.RootElement, "decision"),
        textScanDecision = JsonString(phase205.RootElement, "decision"),
    },
    matrix = new[]
    {
        new
        {
            artifactKind = "wz-absolute-source-lineage",
            readyForApplication = JsonBool(wzApplication, "readyForApplication") is true,
            acceptanceGateCount = JsonInt(wzApplication, "acceptanceGateCount"),
            requiredEvidence = JsonStringArray(wzTemplate.RootElement, "requiredEvidence"),
            requestAcceptanceGates = JsonStringArray(wzRequest.RootElement, "acceptanceGates"),
            missingFields = wzMissingFields,
            gateBlockers = JsonStringArray(wzApplication, "blockers"),
            nextRequiredArtifact = "A derivation-backed, target-independent W/Z absolute source lineage with separate W and Z source rows and all P209 gates true.",
        },
        new
        {
            artifactKind = "higgs-scalar-source-lineage",
            readyForApplication = JsonBool(higgsApplication, "readyForApplication") is true,
            acceptanceGateCount = JsonInt(higgsApplication, "acceptanceGateCount"),
            requiredEvidence = JsonStringArray(higgsTemplate.RootElement, "requiredEvidence"),
            requestAcceptanceGates = JsonStringArray(higgsRequest.RootElement, "acceptanceGates"),
            missingFields = higgsMissingFields,
            gateBlockers = JsonStringArray(higgsApplication, "blockers"),
            nextRequiredArtifact = "A solved, target-independent Higgs scalar source/operator lineage with identity envelope, massive profile, coupling or excitation source, stability sidecars, and a passing prediction row.",
        },
    },
    decision = blockerMatrixReady
        ? "No downstream promotion rerun is scientifically allowed from current artifacts. The exact remaining work is to supply new W/Z and Higgs source-lineage evidence satisfying the listed fields and acceptance gates."
        : "Do not rely on this blocker matrix until upstream scans, claim boundary, and application gates are reconciled.",
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase204Path = Phase204Path,
        phase205Path = Phase205Path,
        phase208Path = Phase208Path,
        phase209Path = Phase209Path,
        phase210Path = Phase210Path,
        wzTemplatePath,
        higgsTemplatePath,
        wzRequestPath,
        higgsRequestPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_source_lineage_blocker_matrix.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_blocker_matrix_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.blockerMatrixReady,
        result.rerunPromotionAllowed,
        result.existingEvidenceFound,
        result.localRouteExhaustionCertified,
        result.scans,
        wzMissingFieldCount = wzMissingFields.Length,
        higgsMissingFieldCount = higgsMissingFields.Length,
        wzMissingFields,
        higgsMissingFields,
        missingEvidenceMap = new[]
        {
            new
            {
                artifactKind = "wz-absolute-source-lineage",
                missingFieldCount = wzMissingFields.Length,
                missingFields = wzMissingFields,
                nextRequiredArtifact = "A derivation-backed, target-independent W/Z absolute source lineage with separate W and Z source rows and all P209 gates true.",
            },
            new
            {
                artifactKind = "higgs-scalar-source-lineage",
                missingFieldCount = higgsMissingFields.Length,
                missingFields = higgsMissingFields,
                nextRequiredArtifact = "A solved, target-independent Higgs scalar source/operator lineage with identity envelope, massive profile, coupling or excitation source, stability sidecars, and a passing prediction row.",
            },
        },
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"blockerMatrixReady={blockerMatrixReady}");
Console.WriteLine($"wzMissingFieldCount={wzMissingFields.Length}");
Console.WriteLine($"higgsMissingFieldCount={higgsMissingFields.Length}");

static JsonElement RequiredObject(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Object
        ? property
        : throw new InvalidOperationException($"Missing object property {propertyName}.");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing string property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

static string[] MissingWzFields(JsonElement template)
{
    var missing = new List<string>();
    if (JsonBool(template, "externalTargetValuesUsed") is not false)
        missing.Add("externalTargetValuesUsed=false");
    if (string.IsNullOrWhiteSpace(JsonString(template, "theoremOrDerivationId")))
        missing.Add("theoremOrDerivationId");
    if (string.IsNullOrWhiteSpace(JsonString(template, "sourceLineageId")))
        missing.Add("sourceLineageId");
    foreach (var row in template.GetProperty("particleRows").EnumerateArray())
    {
        var particleId = JsonString(row, "particleId") ?? "unknown";
        if (string.IsNullOrWhiteSpace(JsonString(row, "sourceRowId")))
            missing.Add($"{particleId}.sourceRowId");
        if (JsonBool(row, "rawAmplitudeGatePassed") is not true)
            missing.Add($"{particleId}.rawAmplitudeGatePassed=true");
        if (JsonBool(row, "commonBridgeGatePassed") is not true)
            missing.Add($"{particleId}.commonBridgeGatePassed=true");
        if (JsonBool(row, "targetComparisonGatePassed") is not true)
            missing.Add($"{particleId}.targetComparisonGatePassed=true");
        if (JsonBool(row, "stabilitySidecarsPresent") is not true)
            missing.Add($"{particleId}.stabilitySidecarsPresent=true");
        if (string.IsNullOrWhiteSpace(JsonString(row, "derivationId")))
            missing.Add($"{particleId}.derivationId");
    }
    return missing.ToArray();
}

static string[] MissingHiggsFields(JsonElement template)
{
    var missing = new List<string>();
    if (JsonBool(template, "externalTargetValuesUsed") is not false)
        missing.Add("externalTargetValuesUsed=false");
    foreach (var key in new[] { "sourceLineageId", "scalarSourceOperatorId", "higgsIdentityEnvelopeId", "massiveScalarProfileId" })
    {
        if (string.IsNullOrWhiteSpace(JsonString(template, key)))
            missing.Add(key);
    }
    if (string.IsNullOrWhiteSpace(JsonString(template, "potentialOrSelfCouplingSourceId"))
        && string.IsNullOrWhiteSpace(JsonString(template, "excitationRelationId")))
        missing.Add("potentialOrSelfCouplingSourceId-or-excitationRelationId");
    var sidecars = template.GetProperty("stabilitySidecars");
    foreach (var key in new[] { "branch", "refinement", "environment", "representation", "coupling" })
    {
        if (JsonBool(sidecars, key) is not true)
            missing.Add($"stabilitySidecars.{key}=true");
    }
    var predictionRow = template.GetProperty("predictionRow");
    if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "sourceRowId")))
        missing.Add("predictionRow.sourceRowId");
    if (JsonBool(predictionRow, "targetComparisonGatePassed") is not true)
        missing.Add("predictionRow.targetComparisonGatePassed=true");
    if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "derivationId")))
        missing.Add("predictionRow.derivationId");
    return missing.ToArray();
}

using System.Text.Json;

const string DefaultOutputDir = "studies/phase209_boson_source_lineage_evidence_request_package_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase208Path = "studies/phase208_boson_local_route_exhaustion_certificate_001/output/boson_local_route_exhaustion_certificate_summary.json";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE209_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase208 = JsonDocument.Parse(File.ReadAllText(Phase208Path));
using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
var wzTemplatePath = RequiredString(phase201.RootElement, "wzTemplatePath");
var higgsTemplatePath = RequiredString(phase201.RootElement, "higgsTemplatePath");
using var wzTemplate = JsonDocument.Parse(File.ReadAllText(wzTemplatePath));
using var higgsTemplate = JsonDocument.Parse(File.ReadAllText(higgsTemplatePath));

var localRouteExhaustionCertified = JsonBool(phase208.RootElement, "localRouteExhaustionCertified") is true;
var anyCurrentLocalRouteActionable = JsonBool(phase208.RootElement, "anyCurrentLocalRouteActionable") is true;
var intakeContractMaterialized = JsonBool(phase201.RootElement, "intakeContractMaterialized") is true;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFields = MissingWzFields(wzTemplate.RootElement);
var higgsMissingFields = MissingHiggsFields(higgsTemplate.RootElement);

var wzRequest = new
{
    requestId = "phase209-wz-absolute-source-lineage-evidence-request",
    requestKind = "wz-absolute-source-lineage-evidence",
    status = "awaiting-new-source-artifact",
    intakeTemplatePath = wzTemplatePath,
    targetObservables = new[]
    {
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    },
    minimumArtifactFields = new[]
    {
        "externalTargetValuesUsed=false",
        "theoremOrDerivationId",
        "sourceLineageId",
        "two particleRows exactly: w-boson and z-boson",
        "sourceRowId for each row",
        "rawAmplitudeGatePassed=true for each row",
        "commonBridgeGatePassed=true for each row",
        "targetComparisonGatePassed=true for each row after source construction",
        "stabilitySidecarsPresent=true for each row",
        "derivationId for each row",
    },
    currentMissingFieldCount = wzMissingFields.Length,
    currentMissingFields = wzMissingFields,
    acceptanceGates = new[]
    {
        "source construction does not use physical W/Z target values or target-implied repair factors",
        "direct bridge/source theorem or derivation is stated and linked to sourceLineageId",
        "W and Z are separate rows, not a single aggregate W/Z-like pair",
        "raw-amplitude gate clears before target comparison",
        "common W/Z bridge-scale gate clears",
        "physical target comparison clears under repository sigma policy",
        "branch, refinement, environment, representation, and coupling stability sidecars are present",
        "rerunning P201, P192, P193, P202, and P101 promotes the rows",
    },
    rejectionRules = new[]
    {
        "target-implied weak coupling or scalar factor is diagnostic only",
        "P190-style stable branch-local candidate without theorem promotion is insufficient",
        "single W/Z-like pair without particle-specific W and Z source rows is insufficient",
        "normalization that clears raw scale but fails common-scale or target comparison is insufficient",
        "draft prose, theorem obligations, or requirements text are not source evidence",
    },
};

var higgsRequest = new
{
    requestId = "phase209-higgs-scalar-source-lineage-evidence-request",
    requestKind = "higgs-scalar-source-lineage-evidence",
    status = "awaiting-new-source-artifact",
    intakeTemplatePath = higgsTemplatePath,
    targetObservable = "physical-higgs-mass-gev",
    minimumArtifactFields = new[]
    {
        "externalTargetValuesUsed=false",
        "sourceLineageId",
        "scalarSourceOperatorId",
        "higgsIdentityEnvelopeId",
        "massiveScalarProfileId",
        "potentialOrSelfCouplingSourceId or excitationRelationId",
        "stabilitySidecars.branch=true",
        "stabilitySidecars.refinement=true",
        "stabilitySidecars.environment=true",
        "stabilitySidecars.representation=true",
        "stabilitySidecars.coupling=true",
        "predictionRow.particleId=higgs",
        "predictionRow.observableId=physical-higgs-mass-gev",
        "predictionRow.sourceRowId",
        "predictionRow.targetComparisonGatePassed=true after source construction",
        "predictionRow.derivationId",
    },
    currentMissingFieldCount = higgsMissingFields.Length,
    currentMissingFields = higgsMissingFields,
    acceptanceGates = new[]
    {
        "source construction does not use physical Higgs mass target or target-implied repair factors",
        "solved scalar-sector source/operator is identified",
        "Higgs identity envelope is independent of physical Higgs mass",
        "massive scalar profile or excitation relation is present",
        "potential/self-coupling or scalar excitation source is explicit",
        "branch, refinement, environment, representation, and coupling stability sidecars are present",
        "physical target comparison clears under repository sigma policy",
        "rerunning P201, P192, P193, P202, and P101 promotes the Higgs row",
    },
    rejectionRules = new[]
    {
        "VEV/order-parameter bridge alone is not a Higgs excitation source",
        "scalar-relation repair derived from target values is diagnostic only",
        "open issue, approximate draft, postdiction, or proof-obligation text is insufficient",
        "generic gauge-lambda or eigenvalue notation is not a Higgs self-coupling source",
        "scaffold-only identity records without solved source/operator and stability sidecars are insufficient",
    },
};

var evidenceRequestPackageMaterialized = localRouteExhaustionCertified
    && !anyCurrentLocalRouteActionable
    && intakeContractMaterialized;
var terminalStatus = evidenceRequestPackageMaterialized
    ? "boson-source-lineage-evidence-request-package-materialized"
    : "boson-source-lineage-evidence-request-package-blocked";

var result = new
{
    phaseId = "phase209-boson-source-lineage-evidence-request-package",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    evidenceRequestPackageMaterialized,
    localRouteExhaustionCertified,
    anyCurrentLocalRouteActionable,
    intakeContractMaterialized,
    allRequiredLineagesPromotable,
    currentDefensibleManifestStatus = JsonString(phase203.RootElement, "terminalStatus"),
    currentDefensibleValueCount = JsonInt(phase203.RootElement, "defensibleValueCount"),
    wzRequestPath = Path.Combine(outputDir, "wz_absolute_source_lineage_evidence_request.json"),
    higgsRequestPath = Path.Combine(outputDir, "higgs_scalar_source_lineage_evidence_request.json"),
    wzRequest,
    higgsRequest,
    decision = evidenceRequestPackageMaterialized
        ? "Use these request artifacts as the next external/new-source work package. Do not rerun local prediction promotion until one or both Phase201 intake templates are filled with evidence satisfying these gates."
        : "The evidence request package cannot be trusted until route exhaustion and intake contracts are current.",
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase208Path = Phase208Path,
        phase203Path = Phase203Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(result.wzRequestPath, JsonSerializer.Serialize(wzRequest, options));
File.WriteAllText(result.higgsRequestPath, JsonSerializer.Serialize(higgsRequest, options));
File.WriteAllText(Path.Combine(outputDir, "boson_source_lineage_evidence_request_package.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_evidence_request_package_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.evidenceRequestPackageMaterialized,
        result.localRouteExhaustionCertified,
        result.anyCurrentLocalRouteActionable,
        result.intakeContractMaterialized,
        result.allRequiredLineagesPromotable,
        result.currentDefensibleManifestStatus,
        result.currentDefensibleValueCount,
        result.wzRequestPath,
        result.higgsRequestPath,
        wzAcceptanceGateCount = wzRequest.acceptanceGates.Length,
        higgsAcceptanceGateCount = higgsRequest.acceptanceGates.Length,
        wzCurrentMissingFieldCount = wzMissingFields.Length,
        higgsCurrentMissingFieldCount = higgsMissingFields.Length,
        wzCurrentMissingFields = wzMissingFields,
        higgsCurrentMissingFields = higgsMissingFields,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"evidenceRequestPackageMaterialized={evidenceRequestPackageMaterialized}");
Console.WriteLine($"wzRequestPath={result.wzRequestPath}");
Console.WriteLine($"higgsRequestPath={result.higgsRequestPath}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing string property {propertyName}.");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

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

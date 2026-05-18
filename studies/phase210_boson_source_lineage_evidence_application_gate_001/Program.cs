using System.Text.Json;

const string DefaultOutputDir = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase209Path = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/boson_source_lineage_evidence_request_package_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE210_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase209 = JsonDocument.Parse(File.ReadAllText(Phase209Path));

var wzTemplatePath = RequiredString(phase201.RootElement, "wzTemplatePath");
var higgsTemplatePath = RequiredString(phase201.RootElement, "higgsTemplatePath");
var wzRequestPath = RequiredString(phase209.RootElement, "wzRequestPath");
var higgsRequestPath = RequiredString(phase209.RootElement, "higgsRequestPath");

using var wzTemplate = JsonDocument.Parse(File.ReadAllText(wzTemplatePath));
using var higgsTemplate = JsonDocument.Parse(File.ReadAllText(higgsTemplatePath));
using var wzRequest = JsonDocument.Parse(File.ReadAllText(wzRequestPath));
using var higgsRequest = JsonDocument.Parse(File.ReadAllText(higgsRequestPath));

var evidenceRequestPackageMaterialized = JsonBool(phase209.RootElement, "evidenceRequestPackageMaterialized") is true;
var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;

var wzApplication = ValidateWzApplication(wzTemplate.RootElement, wzRequest.RootElement);
var higgsApplication = ValidateHiggsApplication(higgsTemplate.RootElement, higgsRequest.RootElement);
var anyEvidenceReadyForApplication = wzApplication.ReadyForApplication || higgsApplication.ReadyForApplication;
var allEvidenceReadyForApplication = wzApplication.ReadyForApplication && higgsApplication.ReadyForApplication;
var rerunPromotionAllowed = evidenceRequestPackageMaterialized && anyEvidenceReadyForApplication;

var terminalStatus = rerunPromotionAllowed
    ? allEvidenceReadyForApplication
        ? "boson-source-lineage-evidence-application-gate-ready-all"
        : "boson-source-lineage-evidence-application-gate-ready-partial"
    : "boson-source-lineage-evidence-application-gate-awaiting-evidence";

var result = new
{
    phaseId = "phase210-boson-source-lineage-evidence-application-gate",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    evidenceRequestPackageMaterialized,
    anyEvidenceReadyForApplication,
    allEvidenceReadyForApplication,
    rerunPromotionAllowed,
    allRequiredLineagesPromotable,
    wzApplication,
    higgsApplication,
    decision = rerunPromotionAllowed
        ? "At least one source-lineage intake is filled enough to rerun downstream boson promotion gates."
        : "Do not rerun boson promotion gates for remaining W/Z or Higgs values. The Phase201 source-lineage templates have not been filled with evidence satisfying the P209 request package.",
    requiredNextAction = rerunPromotionAllowed
        ? "Run P201 and then the full boson generator; require P192/P193/P202/P101 to decide promotion."
        : "Fill the W/Z and/or Higgs Phase201 intake templates using the P209 request artifacts before another promotion attempt.",
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase209Path = Phase209Path,
        wzTemplatePath,
        higgsTemplatePath,
        wzRequestPath,
        higgsRequestPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_source_lineage_evidence_application_gate.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_evidence_application_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.evidenceRequestPackageMaterialized,
        result.anyEvidenceReadyForApplication,
        result.allEvidenceReadyForApplication,
        result.rerunPromotionAllowed,
        result.allRequiredLineagesPromotable,
        result.wzApplication,
        result.higgsApplication,
        result.decision,
        result.requiredNextAction,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rerunPromotionAllowed={rerunPromotionAllowed}");
Console.WriteLine($"wzReadyForApplication={wzApplication.ReadyForApplication}");
Console.WriteLine($"higgsReadyForApplication={higgsApplication.ReadyForApplication}");

static ApplicationReadiness ValidateWzApplication(JsonElement template, JsonElement request)
{
    var blockers = new List<string>();
    var acceptanceGates = request.GetProperty("acceptanceGates").GetArrayLength();
    if (JsonBool(template, "externalTargetValuesUsed") is not false)
        blockers.Add("W/Z externalTargetValuesUsed must be false");
    if (string.IsNullOrWhiteSpace(JsonString(template, "theoremOrDerivationId")))
        blockers.Add("W/Z theoremOrDerivationId is missing");
    AddForbiddenShortcutBlocker(blockers, "W/Z theoremOrDerivationId", JsonString(template, "theoremOrDerivationId"), ForbiddenShortcutTokens.Wz);
    if (string.IsNullOrWhiteSpace(JsonString(template, "sourceLineageId")))
        blockers.Add("W/Z sourceLineageId is missing");
    AddForbiddenShortcutBlocker(blockers, "W/Z sourceLineageId", JsonString(template, "sourceLineageId"), ForbiddenShortcutTokens.Wz);
    if (!template.TryGetProperty("particleRows", out var rows) || rows.ValueKind != JsonValueKind.Array)
    {
        blockers.Add("W/Z particleRows array is missing");
    }
    else
    {
        var rowArray = rows.EnumerateArray().ToArray();
        if (rowArray.Length != 2)
            blockers.Add("W/Z particleRows must contain exactly two rows");
        foreach (var row in rowArray)
        {
            var particleId = JsonString(row, "particleId") ?? "unknown";
            if (string.IsNullOrWhiteSpace(JsonString(row, "sourceRowId")))
                blockers.Add($"{particleId} sourceRowId is missing");
            AddForbiddenShortcutBlocker(blockers, $"{particleId} sourceRowId", JsonString(row, "sourceRowId"), ForbiddenShortcutTokens.Wz);
            if (JsonBool(row, "rawAmplitudeGatePassed") is not true)
                blockers.Add($"{particleId} rawAmplitudeGatePassed must be true");
            if (JsonBool(row, "commonBridgeGatePassed") is not true)
                blockers.Add($"{particleId} commonBridgeGatePassed must be true");
            if (JsonBool(row, "targetComparisonGatePassed") is not true)
                blockers.Add($"{particleId} targetComparisonGatePassed must be true");
            if (JsonBool(row, "stabilitySidecarsPresent") is not true)
                blockers.Add($"{particleId} stabilitySidecarsPresent must be true");
            if (string.IsNullOrWhiteSpace(JsonString(row, "derivationId")))
                blockers.Add($"{particleId} derivationId is missing");
            AddForbiddenShortcutBlocker(blockers, $"{particleId} derivationId", JsonString(row, "derivationId"), ForbiddenShortcutTokens.Wz);
        }
    }

    return new ApplicationReadiness("wz-absolute-source-lineage", blockers.Count == 0, acceptanceGates, blockers.Distinct().ToArray());
}

static ApplicationReadiness ValidateHiggsApplication(JsonElement template, JsonElement request)
{
    var blockers = new List<string>();
    var acceptanceGates = request.GetProperty("acceptanceGates").GetArrayLength();
    if (JsonBool(template, "externalTargetValuesUsed") is not false)
        blockers.Add("Higgs externalTargetValuesUsed must be false");
    foreach (var key in new[] { "sourceLineageId", "scalarSourceOperatorId", "higgsIdentityEnvelopeId", "massiveScalarProfileId" })
    {
        if (string.IsNullOrWhiteSpace(JsonString(template, key)))
            blockers.Add($"Higgs {key} is missing");
        AddForbiddenShortcutBlocker(blockers, $"Higgs {key}", JsonString(template, key), ForbiddenShortcutTokens.Higgs);
    }
    if (string.IsNullOrWhiteSpace(JsonString(template, "potentialOrSelfCouplingSourceId"))
        && string.IsNullOrWhiteSpace(JsonString(template, "excitationRelationId")))
        blockers.Add("Higgs potentialOrSelfCouplingSourceId or excitationRelationId is missing");
    AddForbiddenShortcutBlocker(blockers, "Higgs potentialOrSelfCouplingSourceId", JsonString(template, "potentialOrSelfCouplingSourceId"), ForbiddenShortcutTokens.Higgs);
    AddForbiddenShortcutBlocker(blockers, "Higgs excitationRelationId", JsonString(template, "excitationRelationId"), ForbiddenShortcutTokens.Higgs);
    if (!template.TryGetProperty("stabilitySidecars", out var sidecars) || sidecars.ValueKind != JsonValueKind.Object)
    {
        blockers.Add("Higgs stabilitySidecars object is missing");
    }
    else
    {
        foreach (var key in new[] { "branch", "refinement", "environment", "representation", "coupling" })
        {
            if (JsonBool(sidecars, key) is not true)
                blockers.Add($"Higgs stabilitySidecars.{key} must be true");
        }
    }
    if (!template.TryGetProperty("predictionRow", out var predictionRow) || predictionRow.ValueKind != JsonValueKind.Object)
    {
        blockers.Add("Higgs predictionRow is missing");
    }
    else
    {
        if (JsonString(predictionRow, "particleId") != "higgs")
            blockers.Add("Higgs predictionRow.particleId must be higgs");
        if (JsonString(predictionRow, "observableId") != "physical-higgs-mass-gev")
            blockers.Add("Higgs predictionRow.observableId must be physical-higgs-mass-gev");
        if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "sourceRowId")))
            blockers.Add("Higgs predictionRow.sourceRowId is missing");
        AddForbiddenShortcutBlocker(blockers, "Higgs predictionRow.sourceRowId", JsonString(predictionRow, "sourceRowId"), ForbiddenShortcutTokens.Higgs);
        if (JsonBool(predictionRow, "targetComparisonGatePassed") is not true)
            blockers.Add("Higgs predictionRow.targetComparisonGatePassed must be true");
        if (string.IsNullOrWhiteSpace(JsonString(predictionRow, "derivationId")))
            blockers.Add("Higgs predictionRow.derivationId is missing");
        AddForbiddenShortcutBlocker(blockers, "Higgs predictionRow.derivationId", JsonString(predictionRow, "derivationId"), ForbiddenShortcutTokens.Higgs);
    }

    return new ApplicationReadiness("higgs-scalar-source-lineage", blockers.Count == 0, acceptanceGates, blockers.Distinct().ToArray());
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing string property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static void AddForbiddenShortcutBlocker(List<string> blockers, string fieldName, string? value, IReadOnlyList<string> forbiddenTokens)
{
    if (string.IsNullOrWhiteSpace(value))
        return;

    var matched = forbiddenTokens
        .Where(token => value.Contains(token, StringComparison.OrdinalIgnoreCase))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    if (matched.Length > 0)
        blockers.Add($"{fieldName} references non-promotable shortcut evidence: {string.Join(", ", matched)}");
}

sealed record ApplicationReadiness(string ArtifactKind, bool ReadyForApplication, int AcceptanceGateCount, IReadOnlyList<string> Blockers);

static class ForbiddenShortcutTokens
{
    public static readonly string[] Wz =
    [
        "target-implied",
        "external",
        "fermi",
        "codata",
        "phase75",
        "phase214",
        "diagnostic",
        "shortcut",
        "physical-w-boson-mass",
        "physical-z-boson-mass",
    ];

    public static readonly string[] Higgs =
    [
        "target-implied",
        "external",
        "observed",
        "fermi",
        "codata",
        "phase215",
        "diagnostic",
        "shortcut",
        "physical-higgs-mass",
        "125.2",
    ];
}

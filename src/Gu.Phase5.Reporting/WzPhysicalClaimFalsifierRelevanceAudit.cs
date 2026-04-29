using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Reporting;

public sealed class WzPhysicalClaimFalsifierRelevanceAuditResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("targetObservableId")]
    public required string TargetObservableId { get; init; }

    [JsonPropertyName("targetComparisonPassed")]
    public required bool TargetComparisonPassed { get; init; }

    [JsonPropertyName("selectorVariationPassed")]
    public required bool SelectorVariationPassed { get; init; }

    [JsonPropertyName("selectedModeIds")]
    public required IReadOnlyList<string> SelectedModeIds { get; init; }

    [JsonPropertyName("selectedSourceCandidateIds")]
    public required IReadOnlyList<string> SelectedSourceCandidateIds { get; init; }

    [JsonPropertyName("activeSevereFalsifierCount")]
    public required int ActiveSevereFalsifierCount { get; init; }

    [JsonPropertyName("targetRelevantSevereFalsifierCount")]
    public required int TargetRelevantSevereFalsifierCount { get; init; }

    [JsonPropertyName("globalSidecarSevereFalsifierCount")]
    public required int GlobalSidecarSevereFalsifierCount { get; init; }

    [JsonPropertyName("falsifierAudits")]
    public required IReadOnlyList<WzPhysicalClaimFalsifierRelevanceRecord> FalsifierAudits { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    public string ToJson(bool indented = true)
        => GuJsonDefaults.Serialize(this);
}

public sealed class WzPhysicalClaimFalsifierRelevanceRecord
{
    [JsonPropertyName("falsifierId")]
    public required string FalsifierId { get; init; }

    [JsonPropertyName("falsifierType")]
    public required string FalsifierType { get; init; }

    [JsonPropertyName("severity")]
    public required string Severity { get; init; }

    [JsonPropertyName("targetId")]
    public required string TargetId { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("relevance")]
    public required string Relevance { get; init; }

    [JsonPropertyName("scope")]
    public required string Scope { get; init; }

    [JsonPropertyName("reason")]
    public required string Reason { get; init; }
}

public static class WzPhysicalClaimFalsifierRelevanceAudit
{
    public const string AlgorithmId = "p47-wz-physical-claim-falsifier-relevance-audit:v1";
    private const string TargetRelevant = "target-relevant";
    private const string GlobalSidecar = "global-sidecar";

    public static WzPhysicalClaimFalsifierRelevanceAuditResult Evaluate(
        string falsifierSummaryJson,
        string consistencyScorecardJson,
        string selectorVariationDiagnosticJson,
        string physicalModeRecordsJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(falsifierSummaryJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(consistencyScorecardJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectorVariationDiagnosticJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalModeRecordsJson);

        var falsifiers = FalsifierSummary.FromJson(falsifierSummaryJson);
        using var scorecardDoc = JsonDocument.Parse(consistencyScorecardJson);
        using var selectorDoc = JsonDocument.Parse(selectorVariationDiagnosticJson);
        using var modesDoc = JsonDocument.Parse(physicalModeRecordsJson);

        var targetObservableId = ReadTargetObservableId(scorecardDoc.RootElement);
        var targetPassed = ReadTargetPassed(scorecardDoc.RootElement, targetObservableId);
        var selectedModeIds = ReadSelectedModeIds(modesDoc.RootElement);
        var selectedSourceIds = selectedModeIds.Select(SourceCandidateId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        var targetIds = new HashSet<string>(StringComparer.Ordinal) { targetObservableId };
        foreach (var modeId in selectedModeIds)
            targetIds.Add(modeId);
        foreach (var sourceId in selectedSourceIds)
            targetIds.Add(sourceId);

        var selectorPassed = ReadSelectorVariationPassed(selectorDoc.RootElement);
        var activeSevere = falsifiers.Falsifiers
            .Where(f => f.Active && IsSevere(f.Severity))
            .OrderBy(f => f.FalsifierId, StringComparer.Ordinal)
            .ToList();

        var records = activeSevere
            .Select(f => AuditFalsifier(f, targetIds, targetPassed, selectorPassed))
            .ToList();
        var targetRelevantCount = records.Count(r => r.Relevance == TargetRelevant);
        var globalSidecarCount = records.Count(r => r.Relevance == GlobalSidecar);
        var closure = BuildClosureRequirements(targetPassed, selectorPassed, targetRelevantCount, globalSidecarCount);

        return new WzPhysicalClaimFalsifierRelevanceAuditResult
        {
            ResultId = "phase47-wz-physical-claim-falsifier-relevance-audit-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = targetRelevantCount > 0
                ? "wz-physical-claim-target-blocked"
                : globalSidecarCount > 0
                    ? "wz-physical-claim-target-clear-global-sidecars-blocked"
                    : "wz-physical-claim-falsifier-clear",
            AlgorithmId = AlgorithmId,
            TargetObservableId = targetObservableId,
            TargetComparisonPassed = targetPassed,
            SelectorVariationPassed = selectorPassed,
            SelectedModeIds = selectedModeIds,
            SelectedSourceCandidateIds = selectedSourceIds,
            ActiveSevereFalsifierCount = activeSevere.Count,
            TargetRelevantSevereFalsifierCount = targetRelevantCount,
            GlobalSidecarSevereFalsifierCount = globalSidecarCount,
            FalsifierAudits = records,
            Diagnosis = BuildDiagnosis(targetObservableId, targetPassed, selectorPassed, records),
            ClosureRequirements = closure,
            Provenance = provenance,
        };
    }

    private static WzPhysicalClaimFalsifierRelevanceRecord AuditFalsifier(
        FalsifierRecord falsifier,
        IReadOnlySet<string> targetIds,
        bool targetPassed,
        bool selectorPassed)
    {
        if (targetIds.Contains(falsifier.TargetId))
        {
            return MakeRecord(
                falsifier,
                TargetRelevant,
                "wz-target",
                $"falsifier target '{falsifier.TargetId}' directly matches the W/Z target observable, selected mode, or selected source candidate id");
        }

        if (string.Equals(falsifier.FalsifierType, "branch-fragility", StringComparison.Ordinal) &&
            targetPassed &&
            selectorPassed)
        {
            return MakeRecord(
                falsifier,
                GlobalSidecar,
                "branch-diagnostic",
                $"branch-fragility target '{falsifier.TargetId}' is not the W/Z target observable or selected W/Z source; the W/Z target comparison and selector-variation diagnostics both pass");
        }

        if (string.Equals(falsifier.FalsifierType, "representation-content", StringComparison.Ordinal))
        {
            return MakeRecord(
                falsifier,
                GlobalSidecar,
                "fermion-registry",
                $"representation-content target '{falsifier.TargetId}' does not match the selected W/Z modes or physical W/Z ratio observable");
        }

        return MakeRecord(
            falsifier,
            TargetRelevant,
            "unclassified-severe",
            $"severe falsifier target '{falsifier.TargetId}' could not be proven unrelated to the W/Z physical claim");
    }

    private static WzPhysicalClaimFalsifierRelevanceRecord MakeRecord(
        FalsifierRecord falsifier,
        string relevance,
        string scope,
        string reason)
        => new()
        {
            FalsifierId = falsifier.FalsifierId,
            FalsifierType = falsifier.FalsifierType,
            Severity = falsifier.Severity,
            TargetId = falsifier.TargetId,
            BranchId = falsifier.BranchId,
            Relevance = relevance,
            Scope = scope,
            Reason = reason,
        };

    private static string ReadTargetObservableId(JsonElement root)
    {
        if (root.TryGetProperty("matches", out var matches) &&
            matches.ValueKind == JsonValueKind.Array &&
            matches.GetArrayLength() > 0 &&
            matches[0].TryGetProperty("observableId", out var id) &&
            id.ValueKind == JsonValueKind.String)
        {
            return id.GetString() ?? string.Empty;
        }

        return "physical-w-z-mass-ratio";
    }

    private static bool ReadTargetPassed(JsonElement root, string targetObservableId)
    {
        if (!root.TryGetProperty("matches", out var matches) || matches.ValueKind != JsonValueKind.Array)
            return false;

        foreach (var match in matches.EnumerateArray())
        {
            if (!StringPropertyEquals(match, "observableId", targetObservableId))
                continue;

            return match.TryGetProperty("passed", out var passed) &&
                   passed.ValueKind is JsonValueKind.True or JsonValueKind.False &&
                   passed.GetBoolean();
        }

        return false;
    }

    private static IReadOnlyList<string> ReadSelectedModeIds(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
            return [];

        return root.EnumerateArray()
            .Where(m => StringPropertyEquals(m, "status", "validated"))
            .Select(m => TryGetString(m, "modeId") ?? TryGetString(m, "observableId"))
            .OfType<string>()
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
    }

    private static bool ReadSelectorVariationPassed(JsonElement root)
    {
        var complete = StringPropertyEquals(root, "terminalStatus", "selector-variation-diagnostic-complete");
        var aligned = TryGetInt(root, "alignedPointCount") ?? 0;
        var passing = TryGetInt(root, "passingPointCount") ?? -1;
        var targetInside = root.TryGetProperty("targetInsideObservedEnvelope", out var inside) &&
                           inside.ValueKind is JsonValueKind.True or JsonValueKind.False &&
                           inside.GetBoolean();
        return complete && aligned > 0 && passing == aligned && targetInside;
    }

    private static IReadOnlyList<string> BuildClosureRequirements(
        bool targetPassed,
        bool selectorPassed,
        int targetRelevantCount,
        int globalSidecarCount)
    {
        var closure = new List<string>();
        if (!targetPassed)
            closure.Add("W/Z physical target comparison must pass before target-specific falsifier relevance can clear the physical claim");
        if (!selectorPassed)
            closure.Add("W/Z selector-variation diagnostic must pass before branch-fragility sidecars can be scoped away from the W/Z target");
        if (targetRelevantCount > 0)
            closure.Add("resolve all active severe falsifiers that directly target the W/Z observable or selected W/Z modes");
        if (globalSidecarCount > 0)
            closure.Add("resolve global sidecar falsifiers or adopt a documented target-scoped physical-claim policy before allowing unrestricted physical boson prediction language");
        return closure;
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        string targetObservableId,
        bool targetPassed,
        bool selectorPassed,
        IReadOnlyList<WzPhysicalClaimFalsifierRelevanceRecord> records)
    {
        var diagnosis = new List<string>
        {
            $"audited active fatal/high falsifiers against W/Z target observable '{targetObservableId}'",
            targetPassed
                ? "W/Z quantitative target comparison passes"
                : "W/Z quantitative target comparison does not pass",
            selectorPassed
                ? "W/Z selector variation passes at every aligned point and contains the target inside the observed envelope"
                : "W/Z selector variation does not fully pass",
        };
        diagnosis.Add($"{records.Count(r => r.Relevance == TargetRelevant)} active severe falsifier(s) are target-relevant");
        diagnosis.Add($"{records.Count(r => r.Relevance == GlobalSidecar)} active severe falsifier(s) are global sidecars");
        return diagnosis;
    }

    private static string SourceCandidateId(string sourceId)
        => sourceId.StartsWith("phase22-", StringComparison.Ordinal)
            ? sourceId["phase22-".Length..]
            : sourceId;

    private static bool IsSevere(string severity)
        => string.Equals(severity, FalsifierSeverity.Fatal, StringComparison.Ordinal) ||
           string.Equals(severity, FalsifierSeverity.High, StringComparison.Ordinal);

    private static bool StringPropertyEquals(JsonElement root, string propertyName, string expected)
        => TryGetString(root, propertyName) is { } actual &&
           string.Equals(actual, expected, StringComparison.Ordinal);

    private static string? TryGetString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? TryGetInt(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var result)
            ? result
            : null;
}

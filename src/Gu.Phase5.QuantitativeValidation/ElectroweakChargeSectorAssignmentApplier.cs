using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class ElectroweakChargeSectorApplicationResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("assignmentCount")]
    public required int AssignmentCount { get; init; }

    [JsonPropertyName("updatedFeatureRecordCount")]
    public required int UpdatedFeatureRecordCount { get; init; }

    [JsonPropertyName("updatedModeFamilyCount")]
    public required int UpdatedModeFamilyCount { get; init; }

    [JsonPropertyName("chargedCount")]
    public required int ChargedCount { get; init; }

    [JsonPropertyName("neutralCount")]
    public required int NeutralCount { get; init; }

    [JsonPropertyName("unassignedCount")]
    public required int UnassignedCount { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    [JsonIgnore]
    public string UpdatedIdentityFeaturesJson { get; init; } = string.Empty;

    [JsonIgnore]
    public string UpdatedModeFamiliesJson { get; init; } = string.Empty;
}

public static class ElectroweakChargeSectorAssignmentApplier
{
    public const string AlgorithmId = "p27-electroweak-charge-sector-assignment-applier:v1";

    private const string ChargeSectorBlocker =
        "charged/neutral sector remains unassigned because no electromagnetic or U(1)-mixing convention is present in the internal artifacts.";

    public static ElectroweakChargeSectorApplicationResult Apply(
        string identityFeaturesJson,
        string modeFamiliesJson,
        string mixingReadinessJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityFeaturesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(modeFamiliesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(mixingReadinessJson);

        var readiness = Gu.Core.Serialization.GuJsonDefaults.Deserialize<ElectroweakMixingConventionReadinessResult>(mixingReadinessJson)
            ?? throw new InvalidDataException("Failed to deserialize electroweak mixing convention readiness.");
        var closure = new List<string>();
        if (!string.Equals(readiness.TerminalStatus, "mixing-convention-ready", StringComparison.Ordinal))
            closure.Add("provide a mixing-convention-ready electroweak mixing readiness artifact before applying charge sectors");
        if (readiness.ChargeSectorAssignments.Count == 0)
            closure.Add("provide charge-sector assignments from the electroweak mixing convention evaluator");
        if (readiness.ChargeSectorAssignments.Any(a => a.ChargeSector is not ("charged" or "neutral")))
            closure.Add("charge-sector assignments must be fully resolved to charged or neutral sectors");

        var identityRoot = JsonNode.Parse(identityFeaturesJson)?.AsObject()
            ?? throw new InvalidDataException("Identity features JSON is empty or not an object.");
        var familiesRoot = JsonNode.Parse(modeFamiliesJson)?.AsObject()
            ?? throw new InvalidDataException("Mode-family JSON is empty or not an object.");

        var assignments = readiness.ChargeSectorAssignments
            .Where(a => a.ChargeSector is "charged" or "neutral")
            .ToDictionary(a => a.SourceCandidateId, a => a, StringComparer.Ordinal);

        var updatedFeatures = closure.Count == 0
            ? ApplyToFeatureRecords(identityRoot, assignments)
            : 0;
        var updatedFamilies = closure.Count == 0
            ? ApplyToModeFamilies(familiesRoot, assignments)
            : 0;

        if (closure.Count == 0 && updatedFeatures == 0)
            closure.Add("no identity feature records matched the charge-sector assignments");
        if (closure.Count == 0 && updatedFamilies == 0)
            closure.Add("no mode families matched the charge-sector assignments");

        UpdateFeatureSummary(identityRoot);

        var chargedCount = readiness.ChargeSectorAssignments.Count(a => a.ChargeSector == "charged");
        var neutralCount = readiness.ChargeSectorAssignments.Count(a => a.ChargeSector == "neutral");
        var unassignedCount = readiness.ChargeSectorAssignments.Count(a => a.ChargeSector == "unassigned");

        return new ElectroweakChargeSectorApplicationResult
        {
            ResultId = "phase27-electroweak-charge-sector-application-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = closure.Count == 0 ? "charge-sectors-applied" : "charge-sector-application-blocked",
            AlgorithmId = AlgorithmId,
            AssignmentCount = readiness.ChargeSectorAssignments.Count,
            UpdatedFeatureRecordCount = updatedFeatures,
            UpdatedModeFamilyCount = updatedFamilies,
            ChargedCount = chargedCount,
            NeutralCount = neutralCount,
            UnassignedCount = unassignedCount,
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
            UpdatedIdentityFeaturesJson = identityRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            UpdatedModeFamiliesJson = familiesRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
        };
    }

    private static int ApplyToFeatureRecords(
        JsonObject identityRoot,
        IReadOnlyDictionary<string, ElectroweakChargeSectorAssignmentRecord> assignments)
    {
        var records = identityRoot["featureRecords"]?.AsArray()
            ?? throw new InvalidDataException("Identity features JSON must contain a 'featureRecords' array.");

        var updated = 0;
        foreach (var record in records.OfType<JsonObject>())
        {
            var sourceCandidateId = StringValue(record, "sourceCandidateId");
            if (sourceCandidateId is null || !assignments.TryGetValue(sourceCandidateId, out var assignment))
                continue;

            record["chargeSector"] = assignment.ChargeSector;
            RemoveChargeSectorBlocker(record);
            UpdateFeatureStatus(record);
            updated++;
        }

        return updated;
    }

    private static int ApplyToModeFamilies(
        JsonObject familiesRoot,
        IReadOnlyDictionary<string, ElectroweakChargeSectorAssignmentRecord> assignments)
    {
        var families = familiesRoot["families"]?.AsArray()
            ?? throw new InvalidDataException("Mode-family JSON must contain a 'families' array.");

        var updated = 0;
        foreach (var family in families.OfType<JsonObject>())
        {
            var sourceCandidateId = StringValue(family, "sourceCandidateId");
            if (sourceCandidateId is null || !assignments.TryGetValue(sourceCandidateId, out var assignment))
                continue;

            var features = family["identityFeatures"]?.AsObject();
            if (features is null)
                continue;

            features["chargeSector"] = assignment.ChargeSector;
            RemoveChargeSectorBlocker(features);
            UpdateFeatureStatus(features);
            updated++;
        }

        return updated;
    }

    private static void RemoveChargeSectorBlocker(JsonObject node)
    {
        var blockers = node["blockers"]?.AsArray();
        if (blockers is null)
            return;

        var remaining = new JsonArray();
        foreach (var blocker in blockers)
        {
            var text = blocker?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(text))
                continue;
            if (!string.Equals(text, ChargeSectorBlocker, StringComparison.Ordinal))
                remaining.Add(text);
        }

        node["blockers"] = remaining;
    }

    private static void UpdateFeatureStatus(JsonObject node)
    {
        var blockers = node["blockers"]?.AsArray();
        var hasBlockers = blockers is not null && blockers.Count > 0;
        var complete =
            !hasBlockers &&
            !string.IsNullOrWhiteSpace(StringValue(node, "electroweakMultipletId")) &&
            !string.IsNullOrWhiteSpace(StringValue(node, "chargeSector")) &&
            !string.IsNullOrWhiteSpace(StringValue(node, "currentCouplingSignature"));

        node["featureStatus"] = complete ? "complete" : "partial";
    }

    private static void UpdateFeatureSummary(JsonObject identityRoot)
    {
        var records = identityRoot["featureRecords"]?.AsArray();
        if (records is null)
            return;

        var blockers = records
            .OfType<JsonObject>()
            .SelectMany(record => record["blockers"]?.AsArray().Select(b => b?.GetValue<string>() ?? string.Empty) ?? [])
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var summary = new JsonArray();
        foreach (var blocker in blockers)
            summary.Add(blocker);
        identityRoot["summaryBlockers"] = summary;

        var allComplete = records.OfType<JsonObject>()
            .All(record => string.Equals(StringValue(record, "featureStatus"), "complete", StringComparison.Ordinal));
        identityRoot["terminalStatus"] = records.Count > 0 && allComplete
            ? "identity-features-complete"
            : "identity-features-partial";
    }

    private static string? StringValue(JsonObject node, string propertyName)
        => node.TryGetPropertyValue(propertyName, out var value) && value is not null
            ? value.GetValue<string>()
            : null;
}

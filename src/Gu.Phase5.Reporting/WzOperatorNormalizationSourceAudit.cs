using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WzOperatorNormalizationSourceAuditResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("selectedPairId")]
    public string? SelectedPairId { get; init; }

    [JsonPropertyName("wSourceCandidateId")]
    public string? WSourceCandidateId { get; init; }

    [JsonPropertyName("zSourceCandidateId")]
    public string? ZSourceCandidateId { get; init; }

    [JsonPropertyName("requiredScaleToTarget")]
    public double? RequiredScaleToTarget { get; init; }

    [JsonPropertyName("sourceCount")]
    public required int SourceCount { get; init; }

    [JsonPropertyName("promotableSourceCount")]
    public required int PromotableSourceCount { get; init; }

    [JsonPropertyName("auditOnlySourceCount")]
    public required int AuditOnlySourceCount { get; init; }

    [JsonPropertyName("blockedSourceCount")]
    public required int BlockedSourceCount { get; init; }

    [JsonPropertyName("bestPromotableSource")]
    public WzOperatorNormalizationSourceRecord? BestPromotableSource { get; init; }

    [JsonPropertyName("sources")]
    public required IReadOnlyList<WzOperatorNormalizationSourceRecord> Sources { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzOperatorNormalizationSourceRecord
{
    [JsonPropertyName("sourceId")]
    public required string SourceId { get; init; }

    [JsonPropertyName("artifactPath")]
    public required string ArtifactPath { get; init; }

    [JsonPropertyName("artifactKind")]
    public required string ArtifactKind { get; init; }

    [JsonPropertyName("referencedSourceCandidateIds")]
    public required IReadOnlyList<string> ReferencedSourceCandidateIds { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public string? NormalizationConvention { get; init; }

    [JsonPropertyName("proxyOnly")]
    public required bool ProxyOnly { get; init; }

    [JsonPropertyName("operatorDerivationId")]
    public string? OperatorDerivationId { get; init; }

    [JsonPropertyName("dimensionlessWzScale")]
    public double? DimensionlessWzScale { get; init; }

    [JsonPropertyName("targetIndependent")]
    public required bool TargetIndependent { get; init; }

    [JsonPropertyName("referencesSelectedPair")]
    public required bool ReferencesSelectedPair { get; init; }

    [JsonPropertyName("promotionStatus")]
    public required string PromotionStatus { get; init; }

    [JsonPropertyName("promotionBlockers")]
    public required IReadOnlyList<string> PromotionBlockers { get; init; }
}

public static class WzOperatorNormalizationSourceAudit
{
    public const string AlgorithmId = "p32-wz-operator-normalization-source-audit:v1";

    public static WzOperatorNormalizationSourceAuditResult Evaluate(
        string p31NormalizationClosureJson,
        IReadOnlyList<string> artifactRoots,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(p31NormalizationClosureJson);
        ArgumentNullException.ThrowIfNull(artifactRoots);

        var p31 = GuJsonDefaults.Deserialize<WzNormalizationClosureDiagnosticResult>(p31NormalizationClosureJson)
            ?? throw new InvalidDataException("Failed to deserialize P31 W/Z normalization closure diagnostic.");

        var closure = new List<string>();
        if (artifactRoots.Count == 0)
            closure.Add("provide at least one artifact root to audit");

        var wSource = SourceCandidateId(p31.SelectedPairId?.Split('/', StringSplitOptions.TrimEntries).FirstOrDefault());
        var zSource = SourceCandidateId(p31.SelectedPairId?.Split('/', StringSplitOptions.TrimEntries).Skip(1).FirstOrDefault());
        var records = new List<WzOperatorNormalizationSourceRecord>();

        foreach (var root in artifactRoots.Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            if (File.Exists(root))
            {
                TryAuditFile(root, wSource, zSource, records);
                continue;
            }

            if (!Directory.Exists(root))
            {
                closure.Add($"artifact root does not exist: {root}");
                continue;
            }

            foreach (var path in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories)
                         .OrderBy(p => p, StringComparer.Ordinal))
            {
                TryAuditFile(path, wSource, zSource, records);
            }
        }

        if (records.Count == 0)
            closure.Add("no JSON operator/normalization candidate artifacts were found");

        var promotable = records
            .Where(r => r.PromotionStatus == "promotable")
            .OrderBy(r => ScaleDistance(r.DimensionlessWzScale, p31.RequiredScaleToTarget))
            .ThenBy(r => r.ArtifactPath, StringComparer.Ordinal)
            .ToList();
        if (promotable.Count == 0)
            closure.Add("no promotable target-independent operator normalization source was found for the selected W/Z pair");

        return new WzOperatorNormalizationSourceAuditResult
        {
            ResultId = "phase32-wz-operator-normalization-source-audit-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = closure.Count == 0 ? "wz-operator-normalization-source-ready" : "wz-operator-normalization-source-blocked",
            AlgorithmId = AlgorithmId,
            SelectedPairId = p31.SelectedPairId,
            WSourceCandidateId = wSource,
            ZSourceCandidateId = zSource,
            RequiredScaleToTarget = p31.RequiredScaleToTarget,
            SourceCount = records.Count,
            PromotableSourceCount = promotable.Count,
            AuditOnlySourceCount = records.Count(r => r.PromotionStatus == "audit-only"),
            BlockedSourceCount = records.Count(r => r.PromotionStatus == "blocked"),
            BestPromotableSource = promotable.FirstOrDefault(),
            Sources = records,
            Diagnosis = BuildDiagnosis(records, promotable, p31.RequiredScaleToTarget),
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static void TryAuditFile(string path, string? wSource, string? zSource, List<WzOperatorNormalizationSourceRecord> records)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            records.Add(AuditRoot(path, doc.RootElement, wSource, zSource));
        }
        catch (JsonException)
        {
            // Ignore non-JSON files that happen to match the extension badly.
        }
    }

    private static WzOperatorNormalizationSourceRecord AuditRoot(string path, JsonElement root, string? wSource, string? zSource)
    {
        var text = root.GetRawText();
        var kind = InferKind(path, root, text);
        var referenced = ExtractReferencedSourceCandidateIds(root, text);
        var normalization = FindString(root, "normalizationConvention");
        var scale = FindDouble(root, "dimensionlessWzScale") ??
                    FindDouble(root, "wzNormalizationScale") ??
                    FindDouble(root, "normalizationScaleFactor");
        var derivation = FindString(root, "operatorNormalizationDerivationId") ??
                         FindString(root, "normalizationDerivationId") ??
                         FindString(root, "derivationId");
        var explicitProxyOnly = FindBool(root, "proxyOnly");
        var proxyOnly = explicitProxyOnly ?? IsProxyOnly(kind, text);
        var explicitTargetIndependent = FindBool(root, "targetIndependent");
        var targetIndependent = scale is not null &&
            (explicitTargetIndependent == true || !ContainsPhysicalTargetInput(root));
        var referencesSelectedPair = !string.IsNullOrWhiteSpace(wSource) &&
            !string.IsNullOrWhiteSpace(zSource) &&
            referenced.Contains(wSource, StringComparer.Ordinal) &&
            referenced.Contains(zSource, StringComparer.Ordinal);
        var blockers = new List<string>();
        if (scale is null)
            blockers.Add("source does not provide a dimensionless W/Z normalization scale");
        if (!referencesSelectedPair)
            blockers.Add("source does not reference both selected W/Z source candidates");
        if (string.IsNullOrWhiteSpace(derivation))
            blockers.Add("source does not provide an operator/normalization derivation id");
        if (proxyOnly)
            blockers.Add("source is marked or inferred as proxy-only evidence");
        if (!targetIndependent)
            blockers.Add("source is not proven target-independent");

        var status = blockers.Count == 0
            ? "promotable"
            : kind is "coupling-atlas" or "dirac-operator-bundle" or "dirac-variation-bundle" or "electroweak-feature-table"
                ? "audit-only"
                : "blocked";

        return new WzOperatorNormalizationSourceRecord
        {
            SourceId = Path.GetFileNameWithoutExtension(path),
            ArtifactPath = path,
            ArtifactKind = kind,
            ReferencedSourceCandidateIds = referenced,
            NormalizationConvention = normalization,
            ProxyOnly = proxyOnly,
            OperatorDerivationId = derivation,
            DimensionlessWzScale = scale,
            TargetIndependent = targetIndependent,
            ReferencesSelectedPair = referencesSelectedPair,
            PromotionStatus = status,
            PromotionBlockers = blockers,
        };
    }

    private static string InferKind(string path, JsonElement root, string text)
    {
        if (root.ValueKind == JsonValueKind.Array)
            return path.Contains("coupling", StringComparison.OrdinalIgnoreCase) ? "coupling-json" : "json-array";
        if (root.ValueKind != JsonValueKind.Object)
            return "json";
        if (root.TryGetProperty("atlasId", out _) && root.TryGetProperty("couplings", out _))
            return "coupling-atlas";
        if (root.TryGetProperty("variationId", out _))
            return "dirac-variation-bundle";
        if (root.TryGetProperty("operatorId", out _) && root.TryGetProperty("matrixShape", out _))
            return "dirac-operator-bundle";
        if (root.TryGetProperty("featureRecords", out _))
            return "electroweak-feature-table";
        if (text.Contains("dimensionlessWzScale", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("wzNormalizationScale", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("normalizationScaleFactor", StringComparison.OrdinalIgnoreCase))
            return "operator-normalization-scale";
        return path.Contains("coupling", StringComparison.OrdinalIgnoreCase) ? "coupling-json" : "json";
    }

    private static bool IsProxyOnly(string kind, string text)
        => kind is "coupling-atlas" or "coupling-json" or "electroweak-feature-table" ||
           text.Contains("couplingProxy", StringComparison.OrdinalIgnoreCase) ||
           text.Contains("proxy", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsPhysicalTargetInput(JsonElement root)
        => ContainsProperty(root, "targetValue") ||
           ContainsProperty(root, "physicalTargetValue") ||
           ContainsProperty(root, "requiredScaleToTarget");

    private static IReadOnlyList<string> ExtractReferencedSourceCandidateIds(JsonElement root, string text)
    {
        var ids = new SortedSet<string>(StringComparer.Ordinal);
        CollectStringArray(root, "sourceCandidateIds", ids);
        CollectStringArray(root, "referencedSourceCandidateIds", ids);
        CollectStringValues(root, "sourceCandidateId", ids);
        CollectStringValues(root, "bosonModeId", ids);

        foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(text, @"phase12-candidate-\d+|candidate-\d+"))
            ids.Add(SourceCandidateId(match.Value) ?? match.Value);

        return ids.ToList();
    }

    private static void CollectStringArray(JsonElement element, string propertyName, ISet<string> values)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String && SourceCandidateId(item.GetString()) is { } id)
                            values.Add(id);
                    }
                }
                CollectStringArray(property.Value, propertyName, values);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                CollectStringArray(item, propertyName, values);
        }
    }

    private static void CollectStringValues(JsonElement element, string propertyName, ISet<string> values)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.String &&
                    SourceCandidateId(property.Value.GetString()) is { } id)
                {
                    values.Add(id);
                }
                CollectStringValues(property.Value, propertyName, values);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                CollectStringValues(item, propertyName, values);
        }
    }

    private static string? FindString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.String)
                    return property.Value.GetString();
                var child = FindString(property.Value, propertyName);
                if (!string.IsNullOrWhiteSpace(child))
                    return child;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var child = FindString(item, propertyName);
                if (!string.IsNullOrWhiteSpace(child))
                    return child;
            }
        }
        return null;
    }

    private static double? FindDouble(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.Number &&
                    property.Value.TryGetDouble(out var value))
                {
                    return value;
                }
                var child = FindDouble(property.Value, propertyName);
                if (child is not null)
                    return child;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var child = FindDouble(item, propertyName);
                if (child is not null)
                    return child;
            }
        }
        return null;
    }

    private static bool? FindBool(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    return property.Value.GetBoolean();
                var child = FindBool(property.Value, propertyName);
                if (child is not null)
                    return child;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var child = FindBool(item, propertyName);
                if (child is not null)
                    return child;
            }
        }
        return null;
    }

    private static bool ContainsProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals(propertyName) || ContainsProperty(property.Value, propertyName))
                    return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (ContainsProperty(item, propertyName))
                    return true;
            }
        }
        return false;
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        IReadOnlyList<WzOperatorNormalizationSourceRecord> records,
        IReadOnlyList<WzOperatorNormalizationSourceRecord> promotable,
        double? requiredScale)
    {
        var diagnosis = new List<string>
        {
            $"audited {records.Count} JSON artifact(s) for target-independent W/Z operator normalization sources",
        };
        if (requiredScale is not null)
            diagnosis.Add($"required P31 scale remains {requiredScale.Value:R}");
        if (promotable.Count == 0)
            diagnosis.Add("no audited artifact satisfies the promotion rule for a physical W/Z normalization scale");
        else
            diagnosis.Add($"best promotable source is '{promotable[0].SourceId}' with scale {promotable[0].DimensionlessWzScale:R}");

        var proxyCount = records.Count(r => r.ProxyOnly);
        if (proxyCount > 0)
            diagnosis.Add($"{proxyCount} artifact(s) were treated as proxy-only evidence and not promoted");
        return diagnosis;
    }

    private static double ScaleDistance(double? scale, double? required)
        => scale is null || required is null ? double.PositiveInfinity : System.Math.Abs(scale.Value - required.Value);

    private static string? SourceCandidateId(string? sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return null;
        var trimmed = sourceId.Trim();
        if (trimmed.StartsWith("phase22-", StringComparison.Ordinal))
            trimmed = trimmed["phase22-".Length..];
        if (System.Text.RegularExpressions.Regex.Match(trimmed, @"candidate-\d+") is { Success: true } match)
            return $"phase12-{match.Value}";
        return trimmed.StartsWith("phase12-candidate-", StringComparison.Ordinal) ? trimmed : null;
    }
}

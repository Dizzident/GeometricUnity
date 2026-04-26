using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class ElectroweakMixingConventionReadinessResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("sourceFeatureCount")]
    public required int SourceFeatureCount { get; init; }

    [JsonPropertyName("convention")]
    public ElectroweakMixingConventionRecord? Convention { get; init; }

    [JsonPropertyName("chargeSectorAssignments")]
    public required IReadOnlyList<ElectroweakChargeSectorAssignmentRecord> ChargeSectorAssignments { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class ElectroweakMixingConventionRecord
{
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("electroweakMultipletId")]
    public required string ElectroweakMultipletId { get; init; }

    [JsonPropertyName("u1GeneratorId")]
    public required string U1GeneratorId { get; init; }

    [JsonPropertyName("chargeOperatorDerivationId")]
    public required string ChargeOperatorDerivationId { get; init; }

    [JsonPropertyName("neutralBasisAxisIndex")]
    public required int NeutralBasisAxisIndex { get; init; }

    [JsonPropertyName("chargedBasisAxisIndices")]
    public required IReadOnlyList<int> ChargedBasisAxisIndices { get; init; }

    [JsonPropertyName("externalTargetValuesUsed")]
    public required bool ExternalTargetValuesUsed { get; init; }

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }
}

public sealed class ElectroweakChargeSectorAssignmentRecord
{
    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("electroweakMultipletId")]
    public required string ElectroweakMultipletId { get; init; }

    [JsonPropertyName("dominantBasisIndex")]
    public required int DominantBasisIndex { get; init; }

    [JsonPropertyName("chargeSector")]
    public required string ChargeSector { get; init; }

    [JsonPropertyName("derivationId")]
    public required string DerivationId { get; init; }
}

public static class ElectroweakMixingConventionReadinessEvaluator
{
    public const string AlgorithmId = "p26-electroweak-mixing-convention-readiness:v1";

    public static ElectroweakMixingConventionReadinessResult Evaluate(
        string identityFeaturesJson,
        string? mixingConventionJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityFeaturesJson);

        var features = LoadFeatures(identityFeaturesJson);
        var closure = new List<string>();
        if (features.Count == 0)
            closure.Add("provide internal electroweak identity feature records");
        if (features.Any(f => string.IsNullOrWhiteSpace(f.ElectroweakMultipletId)))
            closure.Add("compute electroweak multiplet identifiers before evaluating a mixing convention");
        if (features.Any(f => f.DominantBasisIndex is null))
            closure.Add("compute dominant SU(2)-adjoint basis sectors before evaluating a mixing convention");

        ElectroweakMixingConventionRecord? convention = null;
        if (string.IsNullOrWhiteSpace(mixingConventionJson))
        {
            closure.Add("provide a validated internal electroweak mixing convention artifact");
            closure.Add("derive a U(1) generator and charge operator independent of external target values");
            closure.Add("declare which SU(2)-adjoint basis axis is neutral and which axes are charged");
        }
        else
        {
            convention = LoadConvention(mixingConventionJson);
            closure.AddRange(ValidateConvention(convention));
        }

        var assignments = closure.Count == 0 && convention is not null
            ? AssignChargeSectors(features, convention)
            : [];

        if (convention is not null &&
            assignments.Count > 0 &&
            assignments.All(a => a.ChargeSector is "charged" or "neutral") &&
            !assignments.Any(a => a.ChargeSector == "unassigned"))
        {
            return new ElectroweakMixingConventionReadinessResult
            {
                ResultId = "phase26-electroweak-mixing-convention-readiness-v1",
                SchemaVersion = "1.0.0",
                TerminalStatus = "mixing-convention-ready",
                AlgorithmId = AlgorithmId,
                SourceFeatureCount = features.Count,
                Convention = convention,
                ChargeSectorAssignments = assignments,
                ClosureRequirements = [],
                Provenance = provenance,
            };
        }

        return new ElectroweakMixingConventionReadinessResult
        {
            ResultId = "phase26-electroweak-mixing-convention-readiness-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = "mixing-convention-blocked",
            AlgorithmId = AlgorithmId,
            SourceFeatureCount = features.Count,
            Convention = convention,
            ChargeSectorAssignments = assignments,
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static IReadOnlyList<ElectroweakChargeSectorAssignmentRecord> AssignChargeSectors(
        IReadOnlyList<FeatureInfo> features,
        ElectroweakMixingConventionRecord convention)
    {
        var charged = convention.ChargedBasisAxisIndices.ToHashSet();
        return features.Select(feature =>
        {
            var axis = feature.DominantBasisIndex!.Value;
            var sector = axis == convention.NeutralBasisAxisIndex
                ? "neutral"
                : charged.Contains(axis)
                    ? "charged"
                    : "unassigned";
            return new ElectroweakChargeSectorAssignmentRecord
            {
                SourceCandidateId = feature.SourceCandidateId,
                ElectroweakMultipletId = feature.ElectroweakMultipletId!,
                DominantBasisIndex = axis,
                ChargeSector = sector,
                DerivationId = $"{AlgorithmId}:{convention.ConventionId}:{convention.ChargeOperatorDerivationId}",
            };
        }).ToList();
    }

    private static IReadOnlyList<string> ValidateConvention(ElectroweakMixingConventionRecord convention)
    {
        var errors = new List<string>();
        if (!string.Equals(convention.Status, "validated", StringComparison.OrdinalIgnoreCase))
            errors.Add($"mixing convention status is '{convention.Status}', not 'validated'");
        if (string.IsNullOrWhiteSpace(convention.ElectroweakMultipletId))
            errors.Add("mixing convention must declare electroweakMultipletId");
        if (string.IsNullOrWhiteSpace(convention.U1GeneratorId))
            errors.Add("mixing convention must declare u1GeneratorId");
        if (string.IsNullOrWhiteSpace(convention.ChargeOperatorDerivationId))
            errors.Add("mixing convention must declare chargeOperatorDerivationId");
        if (convention.ExternalTargetValuesUsed)
            errors.Add("mixing convention must not use external physical target values");
        if (convention.NeutralBasisAxisIndex is < 0 or > 2)
            errors.Add("neutralBasisAxisIndex must be one of the canonical SU(2)-adjoint axes 0, 1, or 2");
        if (convention.ChargedBasisAxisIndices.Count == 0)
            errors.Add("chargedBasisAxisIndices must not be empty");
        if (convention.ChargedBasisAxisIndices.Any(i => i is < 0 or > 2))
            errors.Add("chargedBasisAxisIndices must use canonical SU(2)-adjoint axes 0, 1, or 2");
        if (convention.ChargedBasisAxisIndices.Contains(convention.NeutralBasisAxisIndex))
            errors.Add("neutral basis axis must not also be charged");
        if (convention.ChargedBasisAxisIndices.Distinct().Count() != convention.ChargedBasisAxisIndices.Count)
            errors.Add("chargedBasisAxisIndices must be unique");
        return errors;
    }

    private static ElectroweakMixingConventionRecord LoadConvention(string json)
        => Gu.Core.Serialization.GuJsonDefaults.Deserialize<ElectroweakMixingConventionRecord>(json)
           ?? throw new InvalidDataException("Failed to deserialize electroweak mixing convention.");

    private static IReadOnlyList<FeatureInfo> LoadFeatures(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var records = doc.RootElement.TryGetProperty("featureRecords", out var featureRecords) &&
            featureRecords.ValueKind == JsonValueKind.Array
                ? featureRecords.EnumerateArray()
                : throw new InvalidDataException("Identity features JSON must contain a 'featureRecords' array.");

        return records.Select(record => new FeatureInfo(
            RequiredString(record, "sourceCandidateId"),
            OptionalString(record, "electroweakMultipletId"),
            OptionalInt(record, "dominantBasisIndex"))).ToList();
    }

    private static string RequiredString(JsonElement element, string propertyName)
        => OptionalString(element, propertyName) ?? throw new InvalidDataException($"Missing required property '{propertyName}'.");

    private static string? OptionalString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static int? OptionalInt(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;

    private sealed record FeatureInfo(
        string SourceCandidateId,
        string? ElectroweakMultipletId,
        int? DominantBasisIndex);
}

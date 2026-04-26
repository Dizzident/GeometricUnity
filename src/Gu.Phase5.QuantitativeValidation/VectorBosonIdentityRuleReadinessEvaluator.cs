using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class VectorBosonIdentityRuleReadinessResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("coverage")]
    public required IReadOnlyList<VectorBosonIdentityFeatureCoverageRecord> Coverage { get; init; }

    [JsonPropertyName("derivedRules")]
    public required IReadOnlyList<VectorBosonIdentityRuleRecord> DerivedRules { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class VectorBosonIdentityFeatureCoverageRecord
{
    [JsonPropertyName("sourceId")]
    public required string SourceId { get; init; }

    [JsonPropertyName("sourceObservableId")]
    public required string SourceObservableId { get; init; }

    [JsonPropertyName("sourceCandidateId")]
    public string? SourceCandidateId { get; init; }

    [JsonPropertyName("familyId")]
    public string? FamilyId { get; init; }

    [JsonPropertyName("selectorStable")]
    public required bool SelectorStable { get; init; }

    [JsonPropertyName("hasElectroweakMultipletId")]
    public required bool HasElectroweakMultipletId { get; init; }

    [JsonPropertyName("hasChargeSector")]
    public required bool HasChargeSector { get; init; }

    [JsonPropertyName("hasCurrentCouplingSignature")]
    public required bool HasCurrentCouplingSignature { get; init; }

    [JsonPropertyName("electroweakMultipletId")]
    public string? ElectroweakMultipletId { get; init; }

    [JsonPropertyName("chargeSector")]
    public string? ChargeSector { get; init; }

    [JsonPropertyName("currentCouplingSignature")]
    public string? CurrentCouplingSignature { get; init; }

    [JsonPropertyName("identityRuleEligible")]
    public required bool IdentityRuleEligible { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}

public sealed class VectorBosonIdentityRuleRecord
{
    [JsonPropertyName("ruleId")]
    public required string RuleId { get; init; }

    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("sourceId")]
    public required string SourceId { get; init; }

    [JsonPropertyName("sourceObservableId")]
    public required string SourceObservableId { get; init; }

    [JsonPropertyName("derivationId")]
    public required string DerivationId { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }
}

public static class VectorBosonIdentityRuleReadinessEvaluator
{
    public const string AlgorithmId = "p24-wz-identity-feature-readiness:v1";

    public static VectorBosonIdentityRuleReadinessResult Evaluate(
        IReadOnlyList<CandidateModeSourceRecord> sources,
        string modeFamiliesJson,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentException.ThrowIfNullOrWhiteSpace(modeFamiliesJson);

        var families = LoadFamilies(modeFamiliesJson);
        var coverage = sources
            .Where(s => s.Value > 0 && s.Uncertainty >= 0)
            .Select(source => EvaluateSource(source, families))
            .ToList();

        var charged = coverage.FirstOrDefault(c =>
            c.IdentityRuleEligible &&
            string.Equals(c.ChargeSector, "charged", StringComparison.OrdinalIgnoreCase));
        var neutral = coverage.FirstOrDefault(c =>
            c.IdentityRuleEligible &&
            string.Equals(c.ChargeSector, "neutral", StringComparison.OrdinalIgnoreCase));

        var rules = new List<VectorBosonIdentityRuleRecord>();
        if (charged is not null)
            rules.Add(CreateRule(charged, "w-boson"));
        if (neutral is not null)
            rules.Add(CreateRule(neutral, "z-boson"));

        var closure = BuildClosureRequirements(coverage, charged, neutral);

        return new VectorBosonIdentityRuleReadinessResult
        {
            ResultId = "phase24-wz-identity-rule-readiness-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = rules.Count == 2 ? "identity-rule-ready" : "identity-feature-blocked",
            AlgorithmId = AlgorithmId,
            Coverage = coverage,
            DerivedRules = rules,
            ClosureRequirements = closure,
            Provenance = provenance,
        };
    }

    private static VectorBosonIdentityFeatureCoverageRecord EvaluateSource(
        CandidateModeSourceRecord source,
        IReadOnlyList<IdentityFamilyInfo> families)
    {
        var family = families.FirstOrDefault(f =>
            !string.IsNullOrWhiteSpace(f.SourceCandidateId) &&
            (string.Equals(f.SourceCandidateId, source.SourceId, StringComparison.Ordinal) ||
             source.SourceId.EndsWith(f.SourceCandidateId, StringComparison.Ordinal) ||
             string.Equals(f.SourceCandidateId, source.SourceObservableId, StringComparison.Ordinal) ||
             source.SourceObservableId.EndsWith(f.SourceCandidateId, StringComparison.Ordinal)));

        var blockers = new List<string>();
        var selectorStable = family is not null &&
            family.AmbiguityCount == 0 &&
            family.BranchVariantIds.Count > 0 &&
            family.RefinementLevels.Count > 0 &&
            family.EnvironmentIds.Count > 0 &&
            family.BranchStabilityScore >= 0.99 &&
            family.RefinementStabilityScore >= 0.99 &&
            family.EnvironmentStabilityScore >= 0.99;

        if (family is null)
        {
            blockers.Add("no selector-aware mode family matches this ready source");
        }
        else if (!selectorStable)
        {
            blockers.Add("matched mode family is not selector-stable across branch, refinement, and environment axes");
        }

        if (string.IsNullOrWhiteSpace(family?.ElectroweakMultipletId))
            blockers.Add("missing electroweak multiplet identifier");
        if (string.IsNullOrWhiteSpace(family?.ChargeSector))
            blockers.Add("missing charged/neutral sector signature");
        if (string.IsNullOrWhiteSpace(family?.CurrentCouplingSignature))
            blockers.Add("missing current-coupling signature");

        return new VectorBosonIdentityFeatureCoverageRecord
        {
            SourceId = source.SourceId,
            SourceObservableId = source.SourceObservableId,
            SourceCandidateId = family?.SourceCandidateId,
            FamilyId = family?.FamilyId,
            SelectorStable = selectorStable,
            HasElectroweakMultipletId = !string.IsNullOrWhiteSpace(family?.ElectroweakMultipletId),
            HasChargeSector = !string.IsNullOrWhiteSpace(family?.ChargeSector),
            HasCurrentCouplingSignature = !string.IsNullOrWhiteSpace(family?.CurrentCouplingSignature),
            ElectroweakMultipletId = family?.ElectroweakMultipletId,
            ChargeSector = family?.ChargeSector,
            CurrentCouplingSignature = family?.CurrentCouplingSignature,
            IdentityRuleEligible = blockers.Count == 0,
            Blockers = blockers,
        };
    }

    private static VectorBosonIdentityRuleRecord CreateRule(
        VectorBosonIdentityFeatureCoverageRecord coverage,
        string particleId)
        => new()
        {
            RuleId = $"validated-{particleId}-identity-rule-from-internal-features",
            ParticleId = particleId,
            SourceId = coverage.SourceId,
            SourceObservableId = coverage.SourceObservableId,
            DerivationId = $"{AlgorithmId}:{coverage.ElectroweakMultipletId}:{coverage.ChargeSector}:{coverage.CurrentCouplingSignature}",
            Status = "validated",
            Assumptions =
            [
                "identity rule was derived from internal electroweak feature records",
                "external physical target values were not used to select this source",
            ],
        };

    private static IReadOnlyList<string> BuildClosureRequirements(
        IReadOnlyList<VectorBosonIdentityFeatureCoverageRecord> coverage,
        VectorBosonIdentityFeatureCoverageRecord? charged,
        VectorBosonIdentityFeatureCoverageRecord? neutral)
    {
        var requirements = new List<string>();
        if (coverage.Count == 0)
            requirements.Add("provide at least two ready internal candidate-mode sources");
        if (coverage.Any(c => !c.SelectorStable))
            requirements.Add("repair selector-stability failures before deriving W/Z identity rules");
        if (coverage.Any(c => !c.HasElectroweakMultipletId))
            requirements.Add("compute electroweak multiplet identifiers for candidate mode families");
        if (coverage.Any(c => !c.HasChargeSector))
            requirements.Add("compute charged/neutral sector signatures for candidate mode families");
        if (coverage.Any(c => !c.HasCurrentCouplingSignature))
            requirements.Add("compute current-coupling signatures independent of physical target values");
        if (charged is null)
            requirements.Add("derive at least one charged-sector vector mode identity candidate for W");
        if (neutral is null)
            requirements.Add("derive at least one neutral-sector vector mode identity candidate for Z");

        return requirements.Distinct(StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<IdentityFamilyInfo> LoadFamilies(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var familyElements = root.ValueKind == JsonValueKind.Array
            ? root.EnumerateArray()
            : root.TryGetProperty("families", out var familiesProperty) && familiesProperty.ValueKind == JsonValueKind.Array
                ? familiesProperty.EnumerateArray()
                : throw new InvalidDataException("Mode-family JSON must be an array or contain a 'families' array.");

        return familyElements.Select(ReadFamily).ToList();
    }

    private static IdentityFamilyInfo ReadFamily(JsonElement element)
    {
        var features = element.TryGetProperty("identityFeatures", out var featureElement) &&
            featureElement.ValueKind == JsonValueKind.Object
                ? featureElement
                : default;

        return new IdentityFamilyInfo(
            OptionalString(element, "familyId"),
            OptionalString(element, "sourceCandidateId"),
            ReadStringArray(element, "branchVariantIds"),
            ReadStringArray(element, "refinementLevels"),
            ReadStringArray(element, "environmentIds"),
            OptionalInt(element, "ambiguityCount") ?? int.MaxValue,
            OptionalDouble(element, "branchStabilityScore") ?? -1,
            OptionalDouble(element, "refinementStabilityScore") ?? -1,
            OptionalDouble(element, "environmentStabilityScore") ?? -1,
            OptionalString(features, "electroweakMultipletId"),
            OptionalString(features, "chargeSector"),
            OptionalString(features, "currentCouplingSignature"));
    }

    private static string? OptionalString(JsonElement element, string propertyName)
        => element.ValueKind == JsonValueKind.Object &&
           element.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static double? OptionalDouble(JsonElement element, string propertyName)
        => element.ValueKind == JsonValueKind.Object &&
           element.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.Number
            ? property.GetDouble()
            : null;

    private static int? OptionalInt(JsonElement element, string propertyName)
        => element.ValueKind == JsonValueKind.Object &&
           element.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
        => element.ValueKind == JsonValueKind.Object &&
           element.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .ToList()
            : [];

    private sealed record IdentityFamilyInfo(
        string? FamilyId,
        string? SourceCandidateId,
        IReadOnlyList<string> BranchVariantIds,
        IReadOnlyList<string> RefinementLevels,
        IReadOnlyList<string> EnvironmentIds,
        int AmbiguityCount,
        double BranchStabilityScore,
        double RefinementStabilityScore,
        double EnvironmentStabilityScore,
        string? ElectroweakMultipletId,
        string? ChargeSector,
        string? CurrentCouplingSignature);
}

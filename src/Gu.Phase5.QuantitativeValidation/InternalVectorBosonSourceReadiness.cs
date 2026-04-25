using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class InternalVectorBosonSourceReadinessCampaignSpec
{
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("branchVariantIds")]
    public required IReadOnlyList<string> BranchVariantIds { get; init; }

    [JsonPropertyName("refinementLevels")]
    public required IReadOnlyList<string> RefinementLevels { get; init; }

    [JsonPropertyName("environmentIds")]
    public required IReadOnlyList<string> EnvironmentIds { get; init; }

    [JsonPropertyName("sourceQuantityIds")]
    public required IReadOnlyList<string> SourceQuantityIds { get; init; }

    [JsonPropertyName("readinessPolicy")]
    public required InternalVectorBosonSourceReadinessPolicy ReadinessPolicy { get; init; }

    [JsonPropertyName("identityScope")]
    public required string IdentityScope { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class InternalVectorBosonSourceReadinessPolicy
{
    [JsonPropertyName("policyId")]
    public required string PolicyId { get; init; }

    [JsonPropertyName("minimumClaimClass")]
    public required string MinimumClaimClass { get; init; }

    [JsonPropertyName("minimumBranchStabilityScore")]
    public required double MinimumBranchStabilityScore { get; init; }

    [JsonPropertyName("minimumRefinementStabilityScore")]
    public required double MinimumRefinementStabilityScore { get; init; }

    [JsonPropertyName("maximumAmbiguityCount")]
    public required int MaximumAmbiguityCount { get; init; }

    [JsonPropertyName("requireBranchSelectors")]
    public bool RequireBranchSelectors { get; init; } = true;

    [JsonPropertyName("requireRefinementCoverage")]
    public bool RequireRefinementCoverage { get; init; } = true;

    [JsonPropertyName("requireEnvironmentSelectors")]
    public bool RequireEnvironmentSelectors { get; init; } = true;

    [JsonPropertyName("requireCompleteUncertainty")]
    public bool RequireCompleteUncertainty { get; init; } = true;

    [JsonPropertyName("allowedClaimClasses")]
    public required IReadOnlyList<string> AllowedClaimClasses { get; init; }
}

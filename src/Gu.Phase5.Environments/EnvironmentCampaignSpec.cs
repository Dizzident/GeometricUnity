using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Environments;

public sealed class EnvironmentCampaignSpec
{
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("environmentIds")]
    public required IReadOnlyList<string> EnvironmentIds { get; init; }

    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    [JsonPropertyName("targetQuantities")]
    public required IReadOnlyList<string> TargetQuantities { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

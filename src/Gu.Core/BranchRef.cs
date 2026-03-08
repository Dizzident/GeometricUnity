using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Lightweight reference to a branch manifest by ID and schema version.
/// </summary>
public sealed class BranchRef
{
    /// <summary>The branch identifier.</summary>
    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    /// <summary>Schema version of the branch manifest.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }
}

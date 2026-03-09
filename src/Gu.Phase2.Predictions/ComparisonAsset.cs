using System.Text.Json.Serialization;

namespace Gu.Phase2.Predictions;

/// <summary>
/// External comparison asset with full metadata (Section 13.2).
/// Every external asset must carry source citation, preprocessing, admissible use,
/// domain of validity, uncertainty model, and comparison variable definitions.
/// </summary>
public sealed class ComparisonAsset
{
    /// <summary>Unique asset identifier.</summary>
    [JsonPropertyName("assetId")]
    public required string AssetId { get; init; }

    /// <summary>Source citation / origin metadata.</summary>
    [JsonPropertyName("sourceCitation")]
    public required string SourceCitation { get; init; }

    /// <summary>Date the data was acquired.</summary>
    [JsonPropertyName("acquisitionDate")]
    public required DateTimeOffset AcquisitionDate { get; init; }

    /// <summary>Description of preprocessing applied to the raw data.</summary>
    [JsonPropertyName("preprocessingDescription")]
    public required string PreprocessingDescription { get; init; }

    /// <summary>Statement of how this asset may be used in comparisons.</summary>
    [JsonPropertyName("admissibleUseStatement")]
    public required string AdmissibleUseStatement { get; init; }

    /// <summary>Regime or domain of validity for this data.</summary>
    [JsonPropertyName("domainOfValidity")]
    public required string DomainOfValidity { get; init; }

    /// <summary>Uncertainty model for this asset.</summary>
    [JsonPropertyName("uncertaintyModel")]
    public required UncertaintyRecord UncertaintyModel { get; init; }

    /// <summary>Comparison variable definitions (variable-name -> description).</summary>
    [JsonPropertyName("comparisonVariables")]
    public required IReadOnlyDictionary<string, string> ComparisonVariables { get; init; }
}

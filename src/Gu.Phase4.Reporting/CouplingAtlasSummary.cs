using System.Text.Json.Serialization;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Summary of the boson-fermion coupling atlas for reporting purposes.
/// </summary>
public sealed class CouplingAtlasSummary
{
    /// <summary>Unique summary identifier.</summary>
    [JsonPropertyName("summaryId")]
    public required string SummaryId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Per-boson-mode coupling matrix summaries.</summary>
    [JsonPropertyName("couplingMatrices")]
    public required List<CouplingMatrixSummary> CouplingMatrices { get; init; }

    /// <summary>Total number of (boson-mode, fermion-pair) coupling entries.</summary>
    [JsonPropertyName("totalCouplings")]
    public required int TotalCouplings { get; init; }

    /// <summary>Number of coupling entries with magnitude above numerical noise floor.</summary>
    [JsonPropertyName("nonzeroCouplings")]
    public required int NonzeroCouplings { get; init; }

    /// <summary>Maximum absolute coupling magnitude observed.</summary>
    [JsonPropertyName("maxCouplingMagnitude")]
    public required double MaxCouplingMagnitude { get; init; }
}

/// <summary>
/// Summary statistics for the coupling matrix of a single bosonic mode.
/// </summary>
public sealed class CouplingMatrixSummary
{
    /// <summary>Boson mode identifier.</summary>
    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    /// <summary>Number of fermion pairs with non-negligible coupling to this boson mode.</summary>
    [JsonPropertyName("fermionPairCount")]
    public required int FermionPairCount { get; init; }

    /// <summary>Maximum absolute entry in the coupling matrix for this boson mode.</summary>
    [JsonPropertyName("maxEntry")]
    public required double MaxEntry { get; init; }

    /// <summary>Frobenius norm of the coupling matrix for this boson mode.</summary>
    [JsonPropertyName("frobeniusNorm")]
    public required double FrobeniusNorm { get; init; }
}

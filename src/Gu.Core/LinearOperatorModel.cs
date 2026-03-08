using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Describes a linear operator (e.g., Jacobian J_h) in the linearization path.
/// </summary>
public sealed class LinearOperatorModel
{
    /// <summary>Operator label (e.g., "jacobian", "adjoint").</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Realization type (e.g., "sparse-csr", "matrix-free", "dense").</summary>
    [JsonPropertyName("realizationType")]
    public required string RealizationType { get; init; }

    /// <summary>Number of rows.</summary>
    [JsonPropertyName("rows")]
    public required int Rows { get; init; }

    /// <summary>Number of columns.</summary>
    [JsonPropertyName("cols")]
    public required int Cols { get; init; }

    /// <summary>Optional sparse data (CSR values). Null for matrix-free operators.</summary>
    [JsonPropertyName("values")]
    public double[]? Values { get; init; }

    /// <summary>Optional row pointers for CSR format.</summary>
    [JsonPropertyName("rowPointers")]
    public int[]? RowPointers { get; init; }

    /// <summary>Optional column indices for CSR format.</summary>
    [JsonPropertyName("colIndices")]
    public int[]? ColIndices { get; init; }
}

using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Derived quantities computed from the independent state (Section 10.4):
/// curvature F_h, torsion T_h, Shiab S_h, residual Upsilon_h.
/// </summary>
public sealed class DerivedState
{
    /// <summary>Curvature field F_h.</summary>
    [JsonPropertyName("curvatureF")]
    public required FieldTensor CurvatureF { get; init; }

    /// <summary>Torsion field T_h.</summary>
    [JsonPropertyName("torsionT")]
    public required FieldTensor TorsionT { get; init; }

    /// <summary>Shiab field S_h.</summary>
    [JsonPropertyName("shiabS")]
    public required FieldTensor ShiabS { get; init; }

    /// <summary>Residual field Upsilon_h = S_h - T_h.</summary>
    [JsonPropertyName("residualUpsilon")]
    public required FieldTensor ResidualUpsilon { get; init; }

    /// <summary>Optional diagnostic fields.</summary>
    [JsonPropertyName("diagnostics")]
    public IReadOnlyDictionary<string, FieldTensor>? Diagnostics { get; init; }
}

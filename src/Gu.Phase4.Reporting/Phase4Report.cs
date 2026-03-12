using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Top-level Phase IV report aggregating fermion atlas, coupling atlas, and unified
/// registry summaries.
///
/// PhysicsNote: This report emits evidence summaries, NOT confirmed physical particles.
/// All claim classes carry explicit uncertainty.
/// </summary>
public sealed class Phase4Report
{
    /// <summary>Unique report identifier.</summary>
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Study identifier that produced this report.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Fermion family atlas summary.</summary>
    [JsonPropertyName("fermionAtlas")]
    public required FermionAtlasSummary FermionAtlas { get; init; }

    /// <summary>Coupling atlas summary.</summary>
    [JsonPropertyName("couplingAtlas")]
    public required CouplingAtlasSummary CouplingAtlas { get; init; }

    /// <summary>Unified registry summary.</summary>
    [JsonPropertyName("registrySummary")]
    public required UnifiedRegistrySummary RegistrySummary { get; init; }

    /// <summary>
    /// Structured negative result dashboard: unstable chirality, fragile couplings,
    /// broken family clusters. Negative results are first-class outputs.
    /// Null only for legacy/stub reports; Phase4ReportBuilder always populates this.
    /// </summary>
    [JsonPropertyName("negativeResultDashboard")]
    public NegativeResultDashboard? NegativeResultDashboard { get; init; }

    /// <summary>Additional freeform negative result descriptions.</summary>
    [JsonPropertyName("negativeResults")]
    public required List<string> NegativeResults { get; init; }

    /// <summary>Provenance of this report.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Timestamp when this report was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Serialize to JSON.</summary>
    public string ToJson(bool indented = true)
        => JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = indented,
            Converters = { new JsonStringEnumConverter() },
        });

    /// <summary>Deserialize from JSON.</summary>
    public static Phase4Report FromJson(string json)
        => JsonSerializer.Deserialize<Phase4Report>(json,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } })
            ?? throw new InvalidOperationException("Deserialized Phase4Report was null.");
}

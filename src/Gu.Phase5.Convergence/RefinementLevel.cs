using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase5.Convergence;

/// <summary>
/// A single refinement level in a convergence study (M47).
/// Identified by levelId and characterized by mesh parameters h_X (base space)
/// and h_F (fiber space). The effective mesh parameter is max(h_X, h_F).
/// </summary>
[JsonConverter(typeof(RefinementLevelConverter))]
public sealed class RefinementLevel
{
    /// <summary>Unique level identifier (e.g., "level-0", "level-1").</summary>
    [JsonPropertyName("levelId")]
    public required string LevelId { get; init; }

    /// <summary>
    /// Mesh parameter h_X for the base space X.
    /// Smaller h = finer mesh.
    /// </summary>
    [JsonPropertyName("meshParameterX")]
    public required double MeshParameterX { get; init; }

    /// <summary>
    /// Mesh parameter h_F for the fiber space (field space).
    /// Smaller h = finer mesh.
    /// </summary>
    [JsonPropertyName("meshParameterF")]
    public required double MeshParameterF { get; init; }

    /// <summary>
    /// Effective mesh parameter: max(h_X, h_F).
    /// Used for Richardson extrapolation ordering and convergence analysis.
    /// </summary>
    [JsonIgnore]
    public double EffectiveMeshParameter => System.Math.Max(MeshParameterX, MeshParameterF);

    /// <summary>Human-readable description of this level.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

/// <summary>
/// Custom JSON converter that supports legacy single-field "meshParameter"
/// input (sets both X and F to the same value) while always emitting
/// the new "meshParameterX" / "meshParameterF" fields on write.
/// </summary>
internal sealed class RefinementLevelConverter : JsonConverter<RefinementLevel>
{
    public override RefinementLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for RefinementLevel.");

        string? levelId = null;
        double? meshParameterX = null;
        double? meshParameterF = null;
        double? legacyMeshParameter = null;
        string? description = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName in RefinementLevel.");

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "levelId":
                    levelId = reader.GetString();
                    break;
                case "meshParameterX":
                    meshParameterX = reader.GetDouble();
                    break;
                case "meshParameterF":
                    meshParameterF = reader.GetDouble();
                    break;
                case "meshParameter":
                    // Legacy field: sets both X and F to the same value
                    legacyMeshParameter = reader.GetDouble();
                    break;
                case "description":
                    description = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (levelId is null)
            throw new JsonException("RefinementLevel is missing required field 'levelId'.");

        // Apply legacy fallback: when old meshParameter is present and new fields are absent
        if (legacyMeshParameter.HasValue)
        {
            meshParameterX ??= legacyMeshParameter.Value;
            meshParameterF ??= legacyMeshParameter.Value;
        }

        if (!meshParameterX.HasValue)
            throw new JsonException("RefinementLevel is missing required field 'meshParameterX' (or legacy 'meshParameter').");
        if (!meshParameterF.HasValue)
            throw new JsonException("RefinementLevel is missing required field 'meshParameterF' (or legacy 'meshParameter').");

        return new RefinementLevel
        {
            LevelId = levelId,
            MeshParameterX = meshParameterX.Value,
            MeshParameterF = meshParameterF.Value,
            Description = description,
        };
    }

    public override void Write(Utf8JsonWriter writer, RefinementLevel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("levelId", value.LevelId);
        writer.WriteNumber("meshParameterX", value.MeshParameterX);
        writer.WriteNumber("meshParameterF", value.MeshParameterF);
        if (value.Description is not null)
            writer.WriteString("description", value.Description);
        writer.WriteEndObject();
    }
}

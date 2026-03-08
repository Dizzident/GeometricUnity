using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Core.Serialization;

/// <summary>
/// Default JSON serialization options for all GU types.
/// </summary>
public static class GuJsonDefaults
{
    /// <summary>
    /// Standard JSON options for GU serialization: indented, camelCase, with enums as strings.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    /// <summary>
    /// Serialize an object to JSON string using GU defaults.
    /// </summary>
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options);

    /// <summary>
    /// Deserialize a JSON string to an object using GU defaults.
    /// </summary>
    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options);
}

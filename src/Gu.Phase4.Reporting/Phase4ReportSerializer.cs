using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Handles JSON serialization and deserialization of <see cref="Phase4Report"/> instances.
///
/// Uses System.Text.Json with enum string conversion.
/// All collection properties use <see cref="List{T}"/> to guarantee round-trip safety.
/// </summary>
public static class Phase4ReportSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Serialize a <see cref="Phase4Report"/> to a JSON string.
    /// </summary>
    /// <param name="report">The report to serialize.</param>
    /// <param name="indented">Whether to use indented (pretty-printed) formatting. Default: true.</param>
    /// <returns>JSON string representation of the report.</returns>
    public static string Serialize(Phase4Report report, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, indented ? Options : CompactOptions);
    }

    /// <summary>
    /// Deserialize a <see cref="Phase4Report"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="Phase4Report"/>.</returns>
    /// <exception cref="InvalidOperationException">If the JSON does not represent a valid report.</exception>
    public static Phase4Report Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<Phase4Report>(json, Options)
            ?? throw new InvalidOperationException("Deserialized Phase4Report was null.");
    }

    /// <summary>
    /// Attempt to deserialize, returning null on failure.
    /// </summary>
    public static Phase4Report? TryDeserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<Phase4Report>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Write a report to a file.
    /// </summary>
    public static void WriteToFile(Phase4Report report, string path, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(path);
        File.WriteAllText(path, Serialize(report, indented));
    }

    /// <summary>
    /// Read a report from a file.
    /// </summary>
    public static Phase4Report ReadFromFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var json = File.ReadAllText(path);
        return Deserialize(json);
    }
}

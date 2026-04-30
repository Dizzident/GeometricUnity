using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class BosonPerturbationVectorMaterializationResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("sourceArtifactId")]
    public required string SourceArtifactId { get; init; }

    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    [JsonPropertyName("sourceFieldName")]
    public string? SourceFieldName { get; init; }

    [JsonPropertyName("expectedLength")]
    public required int ExpectedLength { get; init; }

    [JsonPropertyName("perturbationVector")]
    public required IReadOnlyList<double> PerturbationVector { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class BosonPerturbationVectorMaterializer
{
    public const string AlgorithmId = "phase82-boson-perturbation-vector-materializer-v1";
    public const string PreferredModeVectorFieldName = "modeVector";
    public const string LegacyEigenvectorFieldName = "eigenvectorCoefficients";

    public static BosonPerturbationVectorMaterializationResult MaterializeFromModeJson(
        string sourceArtifactId,
        string modeJson,
        int expectedLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceArtifactId);
        ArgumentException.ThrowIfNullOrWhiteSpace(modeJson);

        using var doc = JsonDocument.Parse(modeJson);
        var mode = doc.RootElement;
        var modeId = mode.TryGetProperty("modeId", out var modeIdElement)
            ? modeIdElement.GetString() ?? string.Empty
            : string.Empty;

        string? sourceFieldName = null;
        double[]? vector = null;
        if (mode.TryGetProperty(PreferredModeVectorFieldName, out var modeVectorElement))
        {
            sourceFieldName = PreferredModeVectorFieldName;
            vector = modeVectorElement.Deserialize<double[]>();
        }
        else if (mode.TryGetProperty(LegacyEigenvectorFieldName, out var eigenvectorElement))
        {
            sourceFieldName = LegacyEigenvectorFieldName;
            vector = eigenvectorElement.Deserialize<double[]>();
        }

        return Materialize(sourceArtifactId, modeId, sourceFieldName, vector, expectedLength);
    }

    private static BosonPerturbationVectorMaterializationResult Materialize(
        string sourceArtifactId,
        string modeId,
        string? sourceFieldName,
        IReadOnlyList<double>? vector,
        int expectedLength)
    {
        var closure = new List<string>();
        if (string.IsNullOrWhiteSpace(modeId))
            closure.Add("mode id is missing");
        if (expectedLength <= 0)
            closure.Add("expected perturbation vector length must be positive");
        if (sourceFieldName is null || vector is null)
            closure.Add($"mode JSON must contain '{PreferredModeVectorFieldName}' or '{LegacyEigenvectorFieldName}'");
        if (vector is { Count: 0 })
            closure.Add("perturbation vector is empty");
        if (vector is not null && expectedLength > 0 && vector.Count != expectedLength)
            closure.Add($"perturbation vector length must be {expectedLength}");
        if (vector is not null && vector.Any(v => !double.IsFinite(v)))
            closure.Add("perturbation vector contains a non-finite value");

        return new BosonPerturbationVectorMaterializationResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "boson-perturbation-vector-materialized"
                : "boson-perturbation-vector-blocked",
            SourceArtifactId = sourceArtifactId,
            ModeId = modeId,
            SourceFieldName = sourceFieldName,
            ExpectedLength = expectedLength,
            PerturbationVector = vector?.ToArray() ?? [],
            ClosureRequirements = closure,
        };
    }
}

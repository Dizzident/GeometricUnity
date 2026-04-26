using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzCanonicalOperatorNormalizationDerivationResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("operatorNormalizationDerivationId")]
    public required string OperatorNormalizationDerivationId { get; init; }

    [JsonPropertyName("selectedPairId")]
    public string? SelectedPairId { get; init; }

    [JsonPropertyName("sourceCandidateIds")]
    public required IReadOnlyList<string> SourceCandidateIds { get; init; }

    [JsonPropertyName("wUnitFamily")]
    public string? WUnitFamily { get; init; }

    [JsonPropertyName("zUnitFamily")]
    public string? ZUnitFamily { get; init; }

    [JsonPropertyName("wUnit")]
    public string? WUnit { get; init; }

    [JsonPropertyName("zUnit")]
    public string? ZUnit { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("dimensionlessWzScale")]
    public double? DimensionlessWzScale { get; init; }

    [JsonPropertyName("targetIndependent")]
    public required bool TargetIndependent { get; init; }

    [JsonPropertyName("proxyOnly")]
    public required bool ProxyOnly { get; init; }

    [JsonPropertyName("derivedCalibration")]
    public PhysicalCalibrationRecord? DerivedCalibration { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public static class WzCanonicalOperatorNormalizationDeriver
{
    public const string AlgorithmId = "p33-wz-canonical-operator-normalization-deriver:v1";
    public const string DerivationId = "operator-normalization:shared-internal-mass-operator-unit:v1";

    public static WzCanonicalOperatorNormalizationDerivationResult Derive(
        string p31NormalizationClosureJson,
        string candidateModeSourcesJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(p31NormalizationClosureJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateModeSourcesJson);

        var p31 = GuJsonDefaults.Deserialize<WzNormalizationClosureDiagnosticResult>(p31NormalizationClosureJson)
            ?? throw new InvalidDataException("Failed to deserialize P31 W/Z normalization closure diagnostic.");
        var sources = GuJsonDefaults.Deserialize<CandidateModeSourceBridgeTable>(candidateModeSourcesJson)
            ?? throw new InvalidDataException("Failed to deserialize candidate-mode source bridge table.");

        var closure = new List<string>();
        var wSourceId = p31.SelectedPairId?.Split('/', StringSplitOptions.TrimEntries).FirstOrDefault();
        var zSourceId = p31.SelectedPairId?.Split('/', StringSplitOptions.TrimEntries).Skip(1).FirstOrDefault();
        var w = FindSource(sources.CandidateModeSources, wSourceId);
        var z = FindSource(sources.CandidateModeSources, zSourceId);
        if (w is null)
            closure.Add("selected W source is absent from candidate-mode sources");
        if (z is null)
            closure.Add("selected Z source is absent from candidate-mode sources");

        if (w is not null && z is not null)
        {
            if (!string.Equals(w.UnitFamily, z.UnitFamily, StringComparison.Ordinal))
                closure.Add("selected W/Z sources do not share a unit family");
            if (!string.Equals(w.Unit, z.Unit, StringComparison.Ordinal))
                closure.Add("selected W/Z sources do not share an internal operator unit");
            if (!string.Equals(w.SourceExtractionMethod, z.SourceExtractionMethod, StringComparison.Ordinal))
                closure.Add("selected W/Z sources were not extracted by the same operator method");
        }

        var sourceIds = new[] { SourceCandidateId(wSourceId), SourceCandidateId(zSourceId) }
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var scale = closure.Count == 0 ? 1.0 : (double?)null;
        var calibration = scale is null ? null : CreateCalibration(scale.Value);

        return new WzCanonicalOperatorNormalizationDerivationResult
        {
            ResultId = "phase33-wz-canonical-operator-normalization-derivation-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = closure.Count == 0
                ? "wz-canonical-operator-normalization-derived"
                : "wz-canonical-operator-normalization-blocked",
            AlgorithmId = AlgorithmId,
            OperatorNormalizationDerivationId = DerivationId,
            SelectedPairId = p31.SelectedPairId,
            SourceCandidateIds = sourceIds,
            WUnitFamily = w?.UnitFamily,
            ZUnitFamily = z?.UnitFamily,
            WUnit = w?.Unit,
            ZUnit = z?.Unit,
            NormalizationConvention = "shared-internal-mass-operator-unit",
            DimensionlessWzScale = scale,
            TargetIndependent = true,
            ProxyOnly = false,
            DerivedCalibration = calibration,
            Diagnosis = BuildDiagnosis(p31, w, z, scale),
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static CandidateModeSourceRecord? FindSource(IReadOnlyList<CandidateModeSourceRecord> sources, string? sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return null;
        var sourceCandidateId = SourceCandidateId(sourceId);
        return sources.FirstOrDefault(s =>
            string.Equals(s.SourceId, sourceId, StringComparison.Ordinal) ||
            string.Equals(s.SourceObservableId, sourceId, StringComparison.Ordinal) ||
            string.Equals(SourceCandidateId(s.SourceId), sourceCandidateId, StringComparison.Ordinal));
    }

    private static PhysicalCalibrationRecord CreateCalibration(double scale)
        => new()
        {
            CalibrationId = "phase33-canonical-operator-wz-normalization",
            MappingId = "phase28-w-z-vector-mode-ratio-to-physical-mass-ratio",
            SourceComputedObservableId = "physical-w-z-mass-ratio",
            SourceUnitFamily = "dimensionless",
            TargetUnitFamily = "dimensionless",
            TargetUnit = "dimensionless",
            ScaleFactor = scale,
            ScaleUncertainty = 0,
            Status = "validated",
            Method = "operator-normalization-closure:shared-internal-mass-operator-unit",
            Source = AlgorithmId,
            Assumptions =
            [
                "W and Z source modes are eigenvalues of the same internal mass-like operator unit",
                "dimensionless W/Z ratios inherit no additional physical target scale from the shared internal operator unit",
                "physical target values were not used to derive this scale",
            ],
            ClosureRequirements = [],
        };

    private static IReadOnlyList<string> BuildDiagnosis(
        WzNormalizationClosureDiagnosticResult p31,
        CandidateModeSourceRecord? w,
        CandidateModeSourceRecord? z,
        double? scale)
    {
        var diagnosis = new List<string>();
        if (w is not null && z is not null)
            diagnosis.Add($"selected W/Z sources share unit '{w.UnitFamily}:{w.Unit}' and extraction method '{w.SourceExtractionMethod}'");
        if (scale is not null)
            diagnosis.Add($"canonical shared-operator normalization scale is {scale.Value:R}");
        if (p31.RequiredScaleToTarget is not null && scale is not null)
            diagnosis.Add($"canonical scale differs from P31 target-required scale by {(scale.Value - p31.RequiredScaleToTarget.Value):R}");
        diagnosis.Add("derivation is target-independent and proxy-free; it does not use coupling proxy magnitudes");
        return diagnosis;
    }

    private static string? SourceCandidateId(string? sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return null;
        var trimmed = sourceId.Trim();
        if (trimmed.StartsWith("phase22-", StringComparison.Ordinal))
            trimmed = trimmed["phase22-".Length..];
        if (System.Text.RegularExpressions.Regex.Match(trimmed, @"candidate-\d+") is { Success: true } match)
            return $"phase12-{match.Value}";
        return trimmed.StartsWith("phase12-candidate-", StringComparison.Ordinal) ? trimmed : null;
    }
}

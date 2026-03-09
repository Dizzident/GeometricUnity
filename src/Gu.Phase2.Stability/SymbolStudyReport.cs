using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Aggregated report from a principal-symbol/PDE classification study.
/// Contains symbol samples across multiple (cell, covector) points and
/// a summary PDE classification.
///
/// Per IMPLEMENTATION_PLAN_P2.md Section 9.4 and Milestone 17 completion criteria:
/// "symbol probes run on declared benchmark families,
///  outputs distinguish elliptic-like, hyperbolic-like, mixed, degenerate,
///  or unresolved local behavior in a typed way."
/// </summary>
public sealed class SymbolStudyReport
{
    /// <summary>Unique study report identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Branch manifest ID for this study.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Background state ID around which the symbol was computed.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>Operator probed: "J", "L_tilde", "H".</summary>
    [JsonPropertyName("operatorId")]
    public required string OperatorId { get; init; }

    /// <summary>Gauge study mode used.</summary>
    [JsonPropertyName("gaugeStudyMode")]
    public required GaugeStudyMode GaugeStudyMode { get; init; }

    /// <summary>Individual symbol samples at (cell, covector) points.</summary>
    [JsonPropertyName("samples")]
    public required IReadOnlyList<PrincipalSymbolRecord> Samples { get; init; }

    /// <summary>
    /// Summary PDE classification across all sample points.
    /// If all samples agree, this is the unanimous classification.
    /// Otherwise "Mixed" or "Unresolved".
    /// </summary>
    [JsonPropertyName("summaryClassification")]
    public required PdeClassification SummaryClassification { get; init; }

    /// <summary>Count of samples classified as elliptic-like.</summary>
    [JsonPropertyName("ellipticCount")]
    public required int EllipticCount { get; init; }

    /// <summary>Count of samples classified as hyperbolic-like.</summary>
    [JsonPropertyName("hyperbolicCount")]
    public required int HyperbolicCount { get; init; }

    /// <summary>Count of samples classified as mixed.</summary>
    [JsonPropertyName("mixedCount")]
    public required int MixedCount { get; init; }

    /// <summary>Count of samples classified as degenerate.</summary>
    [JsonPropertyName("degenerateCount")]
    public required int DegenerateCount { get; init; }

    /// <summary>Count of samples classified as unresolved.</summary>
    [JsonPropertyName("unresolvedCount")]
    public required int UnresolvedCount { get; init; }

    /// <summary>Total number of samples.</summary>
    [JsonPropertyName("totalSamples")]
    public required int TotalSamples { get; init; }

    /// <summary>Timestamp when this study was produced.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Build a SymbolStudyReport from a collection of samples.
    /// </summary>
    public static SymbolStudyReport FromSamples(
        string studyId,
        string branchManifestId,
        string backgroundStateId,
        string operatorId,
        GaugeStudyMode gaugeStudyMode,
        IReadOnlyList<PrincipalSymbolRecord> samples)
    {
        int elliptic = 0, hyperbolic = 0, mixed = 0, degenerate = 0, unresolved = 0;

        foreach (var s in samples)
        {
            switch (s.Classification)
            {
                case PdeClassification.EllipticLike: elliptic++; break;
                case PdeClassification.HyperbolicLike: hyperbolic++; break;
                case PdeClassification.Mixed: mixed++; break;
                case PdeClassification.Degenerate: degenerate++; break;
                case PdeClassification.Unresolved: unresolved++; break;
            }
        }

        var summary = DetermineSummary(elliptic, hyperbolic, mixed, degenerate, unresolved, samples.Count);

        return new SymbolStudyReport
        {
            StudyId = studyId,
            BranchManifestId = branchManifestId,
            BackgroundStateId = backgroundStateId,
            OperatorId = operatorId,
            GaugeStudyMode = gaugeStudyMode,
            Samples = samples,
            SummaryClassification = summary,
            EllipticCount = elliptic,
            HyperbolicCount = hyperbolic,
            MixedCount = mixed,
            DegenerateCount = degenerate,
            UnresolvedCount = unresolved,
            TotalSamples = samples.Count,
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    private static PdeClassification DetermineSummary(
        int elliptic, int hyperbolic, int mixed, int degenerate, int unresolved, int total)
    {
        if (total == 0) return PdeClassification.Unresolved;
        if (elliptic == total) return PdeClassification.EllipticLike;
        if (hyperbolic == total) return PdeClassification.HyperbolicLike;
        if (degenerate == total) return PdeClassification.Degenerate;
        if (unresolved == total) return PdeClassification.Unresolved;
        return PdeClassification.Mixed;
    }
}

namespace Gu.Phase2.Comparison;

/// <summary>
/// Extended result from CampaignRunner.RunWithStrategy, including negative-result artifacts.
/// </summary>
public sealed class CampaignRunnerResult
{
    /// <summary>The campaign result with run records and failures.</summary>
    public required ComparisonCampaignResult CampaignResult { get; init; }

    /// <summary>First-class negative-result artifacts for all failures.</summary>
    public required IReadOnlyList<NegativeResultArtifact> NegativeArtifacts { get; init; }
}

using System.Text.Json.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Report-level terminal status for physical prediction campaigns.
/// Low-level prediction records say whether projection is possible; this record
/// combines projection, claim gate, and target comparison into a campaign result.
/// </summary>
public sealed class PhysicalPredictionTerminalStatus
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }

    public static PhysicalPredictionTerminalStatus Evaluate(
        PhysicalClaimGate? gate,
        IReadOnlyList<PhysicalPredictionRecord>? predictions,
        ConsistencyScoreCard? scoreCard)
    {
        var lines = new List<string>();

        if (gate?.PhysicalBosonPredictionAllowed != true)
        {
            if (gate?.TargetScopedPhysicalComparisonAllowed == true)
            {
                lines.Add("Physical prediction terminal status: target-scoped.");
                lines.Add(
                    $"Target-scoped physical comparison is allowed for {gate.TargetScopedObservableId}; unrestricted physical prediction remains blocked.");
                return new PhysicalPredictionTerminalStatus { Status = "target-scoped", SummaryLines = lines };
            }

            lines.Add("Physical prediction terminal status: blocked.");
            lines.Add("Physical claim gate is blocked.");
            return new PhysicalPredictionTerminalStatus { Status = "blocked", SummaryLines = lines };
        }

        var predicted = predictions?
            .Where(p => string.Equals(p.Status, "predicted", StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];
        var blocked = predictions?
            .Where(p => !string.Equals(p.Status, "predicted", StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];

        if (predicted.Count == 0)
        {
            lines.Add("Physical prediction terminal status: blocked.");
            lines.Add("No projected physical prediction record is available.");
            return new PhysicalPredictionTerminalStatus { Status = "blocked", SummaryLines = lines };
        }

        if (blocked.Count > 0)
        {
            lines.Add("Physical prediction terminal status: blocked.");
            lines.Add($"{blocked.Count} physical projection record(s) remain blocked.");
            return new PhysicalPredictionTerminalStatus { Status = "blocked", SummaryLines = lines };
        }

        var physicalMatches = scoreCard?.Matches
            .Where(IsPhysicalTargetMatch)
            .ToList() ?? [];
        if (physicalMatches.Count == 0)
        {
            lines.Add("Physical prediction terminal status: blocked.");
            lines.Add("No physical target comparison exists for the projected prediction.");
            return new PhysicalPredictionTerminalStatus { Status = "blocked", SummaryLines = lines };
        }

        var failed = physicalMatches.Where(m => !m.Passed).ToList();
        if (failed.Count > 0)
        {
            lines.Add("Physical prediction terminal status: failed.");
            foreach (var match in failed)
                lines.Add($"{match.TargetLabel} failed with pull {match.Pull:G6}.");

            return new PhysicalPredictionTerminalStatus { Status = "failed", SummaryLines = lines };
        }

        lines.Add("Physical prediction terminal status: predicted.");
        lines.Add($"{physicalMatches.Count} physical target comparison(s) passed.");
        return new PhysicalPredictionTerminalStatus { Status = "predicted", SummaryLines = lines };
    }

    private static bool IsPhysicalTargetMatch(TargetMatchRecord match)
        => string.Equals(match.TargetEvidenceTier, "physical-prediction", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(match.TargetBenchmarkClass, "physical-observable", StringComparison.OrdinalIgnoreCase);
}

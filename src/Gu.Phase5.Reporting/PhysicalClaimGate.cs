using System.Text.Json.Serialization;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Fail-closed gate for language that would imply real particle prediction.
/// </summary>
public sealed class PhysicalClaimGate
{
    [JsonPropertyName("gateId")]
    public required string GateId { get; init; }

    [JsonPropertyName("physicalBosonPredictionAllowed")]
    public required bool PhysicalBosonPredictionAllowed { get; init; }

    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }

    public static PhysicalClaimGate Evaluate(
        IReadOnlyList<PhysicalObservableMapping>? mappings,
        ObservableClassificationTable? classifications,
        FalsifierSummary? falsifiers,
        bool calibrationAvailable,
        bool physicalTargetEvidenceAvailable)
    {
        var lines = new List<string>();
        var validatedMappings = mappings?.Where(m =>
            string.Equals(m.Status, "validated", StringComparison.OrdinalIgnoreCase)).ToList() ?? [];
        var physicalClassifications = classifications?.Classifications.Where(c =>
            string.Equals(c.Classification, "physical-observable", StringComparison.OrdinalIgnoreCase) &&
            c.PhysicalClaimAllowed).ToList() ?? [];
        var activeSevereFalsifiers = (falsifiers?.Falsifiers ?? [])
            .Where(f => f.Active && (f.Severity == FalsifierSeverity.Fatal || f.Severity == FalsifierSeverity.High))
            .ToList();

        if (validatedMappings.Count == 0)
            lines.Add("- Physical mapping: blocked; no validated physical observable mapping is present.");
        else
            lines.Add($"- Physical mapping: {validatedMappings.Count} validated mapping(s) present.");

        if (physicalClassifications.Count == 0)
            lines.Add("- Observable classification: blocked; no computed observable is classified as a physical observable.");
        else
            lines.Add($"- Observable classification: {physicalClassifications.Count} physical observable classification(s) present.");

        lines.Add(calibrationAvailable
            ? "- Calibration: present."
            : "- Calibration: blocked; no physical unit or scale calibration is present.");

        lines.Add(physicalTargetEvidenceAvailable
            ? "- Physical target evidence: present."
            : "- Physical target evidence: blocked; no physical target evidence table is active.");

        if (activeSevereFalsifiers.Count > 0)
            lines.Add($"- Falsifiers: blocked by {activeSevereFalsifiers.Count} active fatal/high falsifier(s).");
        else
            lines.Add("- Falsifiers: no active fatal/high falsifier supplied to this gate.");

        var allowed = validatedMappings.Count > 0 &&
                      physicalClassifications.Count > 0 &&
                      calibrationAvailable &&
                      physicalTargetEvidenceAvailable &&
                      activeSevereFalsifiers.Count == 0;

        lines.Insert(0, allowed
            ? "- Physical boson prediction: passed."
            : "- Physical boson prediction: blocked.");

        return new PhysicalClaimGate
        {
            GateId = "phase16-physical-claim-gate",
            PhysicalBosonPredictionAllowed = allowed,
            SummaryLines = lines,
        };
    }
}

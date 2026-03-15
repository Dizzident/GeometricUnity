using System.Text;
using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Falsification;
using Phase3Reporting = Gu.Phase3.Reporting;
using Phase5Dossiers = Gu.Phase5.Dossiers;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Builds Phase5Report from dossiers and study artifacts, and renders markdown (M53).
/// </summary>
public static class Phase5ReportGenerator
{
    /// <summary>
    /// Generate a Phase5Report from a list of dossiers and optional study artifacts.
    /// </summary>
    public static Phase5Report Generate(
        string studyId,
        IReadOnlyList<Phase5Dossiers.ValidationDossier> dossiers,
        ProvenanceMeta provenance,
        BranchRobustnessRecord? branchRecord = null,
        RefinementStudyResult? refinementResult = null,
        BridgeManifest? refinementBridgeManifest = null,
        FalsifierSummary? falsifiers = null,
        IReadOnlyList<Phase3Reporting.NegativeResultEntry>? negativeResults = null)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        ArgumentNullException.ThrowIfNull(dossiers);
        ArgumentNullException.ThrowIfNull(provenance);

        var dossierIds = dossiers.Select(d => d.DossierId).ToList();

        BranchIndependenceAtlas? branchAtlas = branchRecord is not null
            ? BuildBranchAtlas(branchRecord)
            : null;

        ConvergenceAtlas? convergenceAtlas = refinementResult is not null
            ? BuildConvergenceAtlas(refinementResult, refinementBridgeManifest)
            : null;

        FalsificationDashboard? falsificationDashboard = falsifiers is not null
            ? BuildFalsificationDashboard(falsifiers)
            : null;

        return new Phase5Report
        {
            ReportId = $"report-{studyId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            SchemaVersion = "1.0.0",
            StudyId = studyId,
            DossierIds = dossierIds,
            BranchIndependenceAtlas = branchAtlas,
            ConvergenceAtlas = convergenceAtlas,
            FalsificationDashboard = falsificationDashboard,
            NegativeResultSummary = negativeResults,
            Provenance = provenance,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Render a Phase5Report as a markdown document.
    /// </summary>
    public static string ToMarkdown(Phase5Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();
        sb.AppendLine($"# Phase V Validation Report: {report.StudyId}");
        sb.AppendLine();
        sb.AppendLine($"**Report ID**: {report.ReportId}");
        sb.AppendLine($"**Generated**: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"**Schema**: {report.SchemaVersion}");
        sb.AppendLine();

        sb.AppendLine("## Dossiers");
        if (report.DossierIds.Count == 0)
            sb.AppendLine("No dossiers included.");
        else
            foreach (var id in report.DossierIds)
                sb.AppendLine($"- {id}");
        sb.AppendLine();

        if (report.BranchIndependenceAtlas is { } bia)
        {
            sb.AppendLine("## Branch Independence");
            foreach (var line in bia.SummaryLines)
                sb.AppendLine(line);
            sb.AppendLine();
        }

        if (report.ConvergenceAtlas is { } ca)
        {
            sb.AppendLine("## Convergence");
            foreach (var line in ca.SummaryLines)
                sb.AppendLine(line);
            sb.AppendLine();
        }

        if (report.FalsificationDashboard is { } fd)
        {
            sb.AppendLine("## Falsification");
            foreach (var line in fd.SummaryLines)
                sb.AppendLine(line);
            sb.AppendLine();
        }

        if (report.NegativeResultSummary is { Count: > 0 } nr)
        {
            sb.AppendLine("## Negative Results");
            foreach (var entry in nr)
                sb.AppendLine($"- [{entry.Description} (evidence: {entry.Evidence})");
            sb.AppendLine();
        }

        sb.AppendLine("## Provenance");
        sb.AppendLine($"- Code revision: {report.Provenance.CodeRevision}");
        sb.AppendLine($"- Backend: {report.Provenance.Backend ?? "cpu"}");

        return sb.ToString();
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static BranchIndependenceAtlas BuildBranchAtlas(BranchRobustnessRecord record)
    {
        int total = record.FragilityRecords.Count;
        int invariant = record.FragilityRecords.Values.Count(
            f => f.Classification is "invariant" or "robust");
        int fragile = record.FragilityRecords.Values.Count(
            f => f.Classification == "fragile");
        int eqClasses = record.EquivalenceClasses.Values.Sum(list => list.Count);

        var lines = new List<string>
        {
            $"- Overall summary: **{record.OverallSummary}**",
            $"- Quantities analyzed: {total}",
            $"- Invariant/robust: {invariant}",
            $"- Fragile: {fragile}",
            $"- Equivalence classes: {eqClasses}",
        };
        if (record.DiagnosticNotes.Count > 0)
            foreach (var note in record.DiagnosticNotes)
                lines.Add($"  - Note: {note}");

        return new BranchIndependenceAtlas
        {
            TotalQuantities = total,
            InvariantCount = invariant,
            FragileCount = fragile,
            EquivalenceClassCount = eqClasses,
            SummaryLines = lines,
        };
    }

    private static ConvergenceAtlas BuildConvergenceAtlas(
        RefinementStudyResult result,
        BridgeManifest? refinementBridgeManifest)
    {
        int total = result.ContinuumEstimates.Count + result.FailureRecords.Count;
        int convergent = result.ContinuumEstimates.Count(e =>
            e.ConvergenceClassification == "convergent");
        int weakly = result.ContinuumEstimates.Count(e =>
            e.ConvergenceClassification == "weakly-convergent");
        int nonConvergent = result.FailureRecords.Count(f =>
            f.FailureType == "non-convergent");
        int insufficientData = result.FailureRecords.Count(f =>
            f.FailureType == "insufficient-data");

        var lines = new List<string>
        {
            $"- Total quantities: {total}",
            $"- Convergent: {convergent}",
            $"- Weakly convergent: {weakly}",
            $"- Non-convergent: {nonConvergent}",
            $"- Insufficient data: {insufficientData}",
        };

        if (refinementBridgeManifest is not null)
        {
            int sourceCount = refinementBridgeManifest.SourceRecordIds.Count;
            lines.Add($"- Evidence source: bridge-derived from {sourceCount} admitted background record(s).");
            lines.Add(sourceCount >= 2
                ? "- Refinement seed family: multi-variant admitted atlas."
                : "- Refinement seed family: single admitted background (still realism-limited).");
            lines.Add("- Direct solver-backed refinement family: no.");
        }
        else
        {
            lines.Add("- Evidence source: direct/unspecified refinement inputs.");
        }

        return new ConvergenceAtlas
        {
            TotalQuantities = total,
            ConvergentCount = convergent + weakly,
            NonConvergentCount = nonConvergent,
            InsufficientDataCount = insufficientData,
            SummaryLines = lines,
        };
    }

    private static FalsificationDashboard BuildFalsificationDashboard(FalsifierSummary summary)
    {
        var lines = new List<string>
        {
            $"- Total falsifiers evaluated: {summary.Falsifiers.Count}",
            $"- Active fatal: {summary.ActiveFatalCount}",
            $"- Active high: {summary.ActiveHighCount}",
            $"- Total active: {summary.TotalActiveCount}",
        };

        return new FalsificationDashboard
        {
            TotalFalsifiers = summary.Falsifiers.Count,
            ActiveFatalCount = summary.ActiveFatalCount,
            ActiveHighCount = summary.ActiveHighCount,
            PromotionCount = 0, // promotions tracked in Dossiers, not Falsification
            DemotionCount = summary.Falsifiers.Count(f => f.Active),
            SummaryLines = lines,
        };
    }
}

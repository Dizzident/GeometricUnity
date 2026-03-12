using System.Text;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Produces human-readable text summaries from a <see cref="Phase4Report"/>.
///
/// The output is plain text suitable for console output, markdown bodies,
/// or inclusion in broader pipeline reports.
///
/// Scope disclaimer: every formatted summary includes the required note that
/// the report reflects branch-local computation only.
/// </summary>
public sealed class ReportSummaryFormatter
{
    /// <summary>
    /// The required scope disclaimer appended to every report summary.
    /// </summary>
    public const string ScopeDisclaimer =
        "This report reflects branch-local computation. " +
        "Physical interpretation requires additional validation steps not included here.";

    /// <summary>
    /// Format a full human-readable summary of the given report.
    /// </summary>
    /// <param name="report">The report to summarize.</param>
    /// <returns>A multi-line text summary string.</returns>
    public string Format(Phase4Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();

        sb.AppendLine("=== Phase IV Fermionic Sector Report ===");
        sb.AppendLine();
        sb.AppendLine($"Report ID : {report.ReportId}");
        sb.AppendLine($"Study ID  : {report.StudyId}");
        sb.AppendLine($"Generated : {report.GeneratedAt:O}");
        sb.AppendLine($"Schema    : {report.SchemaVersion}");
        sb.AppendLine();

        AppendFermionAtlasSummary(sb, report.FermionAtlas);
        AppendCouplingAtlasSummary(sb, report.CouplingAtlas);
        AppendRegistrySummary(sb, report.RegistrySummary);
        AppendNegativeResults(sb, report.NegativeResults);
        AppendDiagnosticNotes(sb, report.NegativeResults);

        sb.AppendLine("--- SCOPE DISCLAIMER ---");
        sb.AppendLine(ScopeDisclaimer);

        return sb.ToString();
    }

    /// <summary>
    /// Format a brief one-line status line for the report.
    /// </summary>
    public string FormatOneLiner(Phase4Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return $"[{report.StudyId}] " +
               $"families={report.FermionAtlas.TotalFamilies} " +
               $"(stable={report.FermionAtlas.StableFamilies}) " +
               $"couplings={report.CouplingAtlas.TotalCouplings} " +
               $"bosons={report.RegistrySummary.TotalBosons} " +
               $"fermions={report.RegistrySummary.TotalFermions} " +
               $"interactions={report.RegistrySummary.TotalInteractions}";
    }

    // -----------------------------------------------------------------------
    // Private section formatters
    // -----------------------------------------------------------------------

    private static void AppendFermionAtlasSummary(StringBuilder sb, FermionAtlasSummary atlas)
    {
        sb.AppendLine("--- Fermion Atlas ---");
        sb.AppendLine($"  Total families     : {atlas.TotalFamilies}");
        sb.AppendLine($"  Stable families    : {atlas.StableFamilies}");
        sb.AppendLine($"  Ambiguous families : {atlas.AmbiguousFamilies}");
        sb.AppendLine();

        if (atlas.FamilySheets.Count > 0)
        {
            sb.AppendLine("  Family Sheets:");
            foreach (var sheet in atlas.FamilySheets)
            {
                sb.AppendLine($"    [{sheet.FamilyId}] " +
                              $"lambda_mean={sheet.MeanEigenvalue:E4} " +
                              $"spread={sheet.EigenvalueSpread:E4} " +
                              $"members={sheet.MemberCount} " +
                              $"stable={sheet.IsStable} " +
                              $"class={sheet.ClaimClass}");
            }
            sb.AppendLine();
        }

        if (atlas.ChiralitySummaries.Count > 0)
        {
            sb.AppendLine("  Chirality Summaries:");
            foreach (var c in atlas.ChiralitySummaries)
            {
                sb.AppendLine($"    [{c.FamilyId}] " +
                              $"L={c.LeftProjection:F3} R={c.RightProjection:F3} " +
                              $"status={c.ChiralityStatus} type={c.ChiralityType}");
            }
            sb.AppendLine();
        }
    }

    private static void AppendCouplingAtlasSummary(StringBuilder sb, CouplingAtlasSummary coupling)
    {
        sb.AppendLine("--- Coupling Atlas ---");
        sb.AppendLine($"  Total couplings    : {coupling.TotalCouplings}");
        sb.AppendLine($"  Nonzero couplings  : {coupling.NonzeroCouplings}");
        sb.AppendLine($"  Max |coupling|     : {coupling.MaxCouplingMagnitude:E4}");
        sb.AppendLine();

        if (coupling.CouplingMatrices.Count > 0)
        {
            sb.AppendLine("  Coupling Matrices:");
            foreach (var m in coupling.CouplingMatrices)
            {
                sb.AppendLine($"    boson={m.BosonModeId} pairs={m.FermionPairCount} " +
                              $"maxEntry={m.MaxEntry:E4} frob={m.FrobeniusNorm:E4}");
            }
            sb.AppendLine();
        }
    }

    private static void AppendRegistrySummary(StringBuilder sb, UnifiedRegistrySummary registry)
    {
        sb.AppendLine("--- Unified Registry ---");
        sb.AppendLine($"  Bosons       : {registry.TotalBosons}");
        sb.AppendLine($"  Fermions     : {registry.TotalFermions}");
        sb.AppendLine($"  Interactions : {registry.TotalInteractions}");
        sb.AppendLine();

        if (registry.ClaimClassCounts.Count > 0)
        {
            sb.AppendLine("  Claim Class Distribution:");
            foreach (var (cls, count) in registry.ClaimClassCounts.OrderBy(kv => kv.Key))
                sb.AppendLine($"    {cls} : {count}");
            sb.AppendLine();
        }

        if (registry.TopCandidates.Count > 0)
        {
            sb.AppendLine("  Top Candidates:");
            foreach (var c in registry.TopCandidates.Take(10))
            {
                sb.AppendLine($"    [{c.CandidateId}] type={c.ParticleType} " +
                              $"class={c.ClaimClass} mass~={c.MassLikeValue:E4} " +
                              $"demotions={c.DemotionCount}");
            }
            sb.AppendLine();
        }
    }

    private static void AppendNegativeResults(StringBuilder sb, List<string> negativeResults)
    {
        if (negativeResults.Count == 0)
            return;

        sb.AppendLine("--- Negative Results ---");
        foreach (var neg in negativeResults)
            sb.AppendLine($"  {neg}");
        sb.AppendLine();
    }

    private static void AppendDiagnosticNotes(StringBuilder sb, List<string> negativeResults)
    {
        // Diagnostic notes are propagated via negative results entries tagged [demotion] or [ambiguity].
        // This section is intentionally minimal; full diagnostic context lives in the JSON report.
        _ = negativeResults; // used via AppendNegativeResults
    }
}

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
    /// <param name="shiabFamilyScope">
    /// Optional Shiab family scope record (P11-M9). When provided, the report explicitly
    /// states which Shiab operators were exercised and the artifact-backed boundary.
    /// Per D-P11-009: neither current operator is the canonical draft operator.
    /// </param>
    /// <param name="geometryEvidenceLabel">
    /// Optional geometry evidence label (P11-M7). When "toy-control" (or null, which
    /// defaults to "toy-control"), the report emits a mechanical block on X^4/Observerse
    /// recovery claims. Per D-P11-007: current evidence is toy-control only.
    /// </param>
    public static Phase5Report Generate(
        string studyId,
        IReadOnlyList<Phase5Dossiers.ValidationDossier> dossiers,
        ProvenanceMeta provenance,
        BranchRobustnessRecord? branchRecord = null,
        RefinementStudyResult? refinementResult = null,
        RefinementEvidenceManifest? refinementEvidenceManifest = null,
        FalsifierSummary? falsifiers = null,
        IReadOnlyList<Phase3Reporting.NegativeResultEntry>? negativeResults = null,
        ShiabFamilyScopeRecord? shiabFamilyScope = null,
        string? geometryEvidenceLabel = null,
        ObservableClassificationTable? observableClassifications = null,
        IReadOnlyList<PhysicalObservableMapping>? physicalObservableMappings = null,
        IReadOnlyList<PhysicalPredictionRecord>? physicalPredictions = null,
        bool physicalCalibrationAvailable = false,
        bool physicalTargetEvidenceAvailable = false)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        ArgumentNullException.ThrowIfNull(dossiers);
        ArgumentNullException.ThrowIfNull(provenance);

        var dossierIds = dossiers.Select(d => d.DossierId).ToList();

        BranchIndependenceAtlas? branchAtlas = branchRecord is not null
            ? BuildBranchAtlas(branchRecord)
            : null;

        ConvergenceAtlas? convergenceAtlas = refinementResult is not null
            ? BuildConvergenceAtlas(refinementResult, refinementEvidenceManifest)
            : null;

        FalsificationDashboard? falsificationDashboard = falsifiers is not null
            ? BuildFalsificationDashboard(falsifiers)
            : null;

        // P11-M7: default to toy-control if not specified (current repo context)
        string effectiveGeomLabel = geometryEvidenceLabel ?? GeometryEvidenceClassifier.ToyControl;
        string? observerseBlock = GeometryEvidenceClassifier.GetBlockStatement(effectiveGeomLabel);
        var physicalClaimGate = PhysicalClaimGate.Evaluate(
            physicalObservableMappings,
            observableClassifications,
            falsifiers,
            calibrationAvailable: physicalCalibrationAvailable,
            physicalTargetEvidenceAvailable: physicalTargetEvidenceAvailable);

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
            ShiabFamilyScope = shiabFamilyScope,
            GeometryEvidenceLabel = effectiveGeomLabel,
            ObserverseRecoveryBlock = observerseBlock,
            ObservableClassifications = observableClassifications,
            PhysicalClaimGate = physicalClaimGate,
            PhysicalPredictions = physicalPredictions,
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

        // P11-M9: Shiab family scope section
        if (report.ShiabFamilyScope is { } scope)
        {
            sb.AppendLine("## Shiab Family Scope");
            sb.AppendLine($"- Standard path operators: {string.Join(", ", scope.StandardPathShiabIds)}");
            sb.AppendLine($"- Paired path operators: {string.Join(", ", scope.PairedPathShiabIds.Count > 0 ? scope.PairedPathShiabIds : new[] { "(none)" })}");
            sb.AppendLine($"- Operators mathematically distinct: {(scope.OperatorsAreMathematicallyDistinct ? "yes" : "no")}");
            sb.AppendLine($"- Standard path branch result: {scope.StandardPathBranchResult}");
            sb.AppendLine($"- Paired path branch result: {scope.PairedPathBranchResult}");
            sb.AppendLine($"- Paired path changes conclusion: {(scope.PairedPathChangesConclusion ? "yes" : "no")}");
            if (scope.ExpansionBlockedReason is not null)
                sb.AppendLine($"- Expansion blocked: {scope.ExpansionBlockedReason}");
            sb.AppendLine($"- {scope.FamilyOpenStatement}");
            sb.AppendLine();
        }

        // P11-M7: Geometry evidence label and Observerse recovery block
        sb.AppendLine("## Geometry Evidence");
        sb.AppendLine($"- Geometry evidence label: **{report.GeometryEvidenceLabel ?? GeometryEvidenceClassifier.ToyControl}**");
        if (report.ObserverseRecoveryBlock is not null)
            sb.AppendLine($"- {report.ObserverseRecoveryBlock}");
        sb.AppendLine();

        if (report.ObservableClassifications is { } classifications)
        {
            sb.AppendLine("## Observable Classifications");
            foreach (var classification in classifications.Classifications)
            {
                sb.AppendLine(
                    $"- {classification.ObservableId}: {classification.Classification}; physical claim allowed: {(classification.PhysicalClaimAllowed ? "yes" : "no")}. {classification.Rationale}");
            }
            sb.AppendLine();
        }

        if (report.PhysicalClaimGate is { } gate)
        {
            sb.AppendLine("## Physical Claim Gate");
            foreach (var line in gate.SummaryLines)
                sb.AppendLine(line);
            sb.AppendLine();
        }

        if (report.PhysicalPredictions is { Count: > 0 } predictions)
        {
            sb.AppendLine("## Physical Prediction Records");
            foreach (var prediction in predictions)
            {
                if (string.Equals(prediction.Status, "predicted", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(
                        $"- {prediction.TargetPhysicalObservableId}: {prediction.Value:G6} +/- {prediction.Uncertainty:G6} {prediction.Unit}; source {prediction.SourceComputedObservableId}; mapping {prediction.MappingId}.");
                }
                else
                {
                    sb.AppendLine(
                        $"- {prediction.MappingId}: blocked. {string.Join(" ", prediction.BlockReasons)}");
                }
            }
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
        RefinementEvidenceManifest? refinementEvidenceManifest)
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

        if (refinementEvidenceManifest is not null)
        {
            int sourceCount = refinementEvidenceManifest.SourceRecordIds.Count;
            if (string.Equals(refinementEvidenceManifest.EvidenceSource, "direct-solver-backed", StringComparison.Ordinal))
            {
                lines.Add($"- Evidence source: direct solver-backed from {sourceCount} executed background record(s).");
                lines.Add("- Refinement seed family: direct solver-backed ladder.");
                lines.Add("- Direct solver-backed refinement family: yes.");
            }
            else
            {
                lines.Add($"- Evidence source: bridge-derived from {sourceCount} admitted background record(s).");
                lines.Add(sourceCount >= 2
                    ? "- Refinement seed family: multi-variant admitted atlas."
                    : "- Refinement seed family: single admitted background (still realism-limited).");
                lines.Add("- Direct solver-backed refinement family: no.");
            }
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

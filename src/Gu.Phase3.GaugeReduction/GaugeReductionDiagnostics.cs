namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Builds diagnostic reports for gauge reduction: constraint defect reports
/// and gauge leak reports.
/// </summary>
public static class GaugeReductionDiagnostics
{
    /// <summary>
    /// Build a gauge leak report by testing trial vectors against the gauge projector.
    /// </summary>
    /// <param name="projector">Gauge projector built from gauge basis.</param>
    /// <param name="trialVectors">Trial vectors to test for gauge contamination.</param>
    /// <param name="backgroundId">Background state ID.</param>
    /// <param name="vectorLabels">Optional labels for each trial vector.</param>
    public static GaugeLeakReport BuildGaugeLeakReport(
        GaugeProjector projector,
        IReadOnlyList<double[]> trialVectors,
        string backgroundId,
        IReadOnlyList<string>? vectorLabels = null)
    {
        ArgumentNullException.ThrowIfNull(projector);
        ArgumentNullException.ThrowIfNull(trialVectors);

        var entries = new GaugeLeakEntry[trialVectors.Count];
        double maxLeak = 0;
        double sumLeak = 0;

        for (int i = 0; i < trialVectors.Count; i++)
        {
            double score = projector.GaugeLeakScore(trialVectors[i]);
            var gaugeComp = projector.ApplyGauge(trialVectors[i]);
            double gaugeNorm = L2Norm(gaugeComp);
            double totalNorm = L2Norm(trialVectors[i]);

            string label = vectorLabels != null && i < vectorLabels.Count
                ? vectorLabels[i]
                : $"trial-{i}";

            entries[i] = new GaugeLeakEntry
            {
                VectorLabel = label,
                LeakScore = score,
                GaugeNorm = gaugeNorm,
                TotalNorm = totalNorm,
            };

            if (score > maxLeak) maxLeak = score;
            sumLeak += score;
        }

        double meanLeak = trialVectors.Count > 0 ? sumLeak / trialVectors.Count : 0.0;

        return new GaugeLeakReport
        {
            BackgroundId = backgroundId,
            GaugeRank = projector.GaugeRank,
            Entries = entries,
            MaxLeakScore = maxLeak,
            MeanLeakScore = meanLeak,
        };
    }

    /// <summary>
    /// Build a constraint defect report from a gauge basis.
    /// </summary>
    /// <param name="basis">The gauge basis.</param>
    /// <param name="branchManifestId">Branch manifest ID for the report.</param>
    /// <param name="dimG">Lie algebra dimension (for expected kernel = dimG).</param>
    public static ConstraintDefectReport BuildConstraintDefectReport(
        GaugeBasis basis,
        string branchManifestId,
        int dimG)
    {
        ArgumentNullException.ThrowIfNull(basis);

        var report = ConstraintDefectReport.FromGaugeBasis(basis);
        return report;
    }

    /// <summary>
    /// Overload that accepts trial vectors as array, background ID, and branch manifest ID.
    /// </summary>
    public static GaugeLeakReport BuildGaugeLeakReport(
        GaugeProjector projector,
        double[][] trialVectors,
        string backgroundId,
        string branchManifestId)
    {
        return BuildGaugeLeakReport(projector, (IReadOnlyList<double[]>)trialVectors, backgroundId);
    }

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }
}

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Builds gauge leak reports by projecting trial vectors onto the gauge subspace.
/// </summary>
public static class GaugeLeakDiagnostics
{
    /// <summary>
    /// Compute a gauge leak report for a set of labeled vectors.
    /// </summary>
    /// <param name="projector">The gauge projector.</param>
    /// <param name="vectors">Named vectors to test.</param>
    public static GaugeLeakReport ComputeLeakReport(
        GaugeProjector projector,
        IReadOnlyList<(string Label, double[] Vector)> vectors)
    {
        if (projector == null) throw new ArgumentNullException(nameof(projector));
        if (vectors == null) throw new ArgumentNullException(nameof(vectors));

        var entries = new List<GaugeLeakEntry>();
        double maxLeak = 0;
        double sumLeak = 0;

        foreach (var (label, vec) in vectors)
        {
            double totalNormSq = 0;
            for (int i = 0; i < vec.Length; i++)
                totalNormSq += vec[i] * vec[i];
            double totalNorm = System.Math.Sqrt(totalNormSq);

            var gaugeProj = projector.ApplyGauge(vec);
            double gaugeNormSq = 0;
            for (int i = 0; i < gaugeProj.Length; i++)
                gaugeNormSq += gaugeProj[i] * gaugeProj[i];
            double gaugeNorm = System.Math.Sqrt(gaugeNormSq);

            double leak = totalNorm > 1e-30 ? gaugeNorm / totalNorm : 0.0;

            entries.Add(new GaugeLeakEntry
            {
                VectorLabel = label,
                LeakScore = leak,
                GaugeNorm = gaugeNorm,
                TotalNorm = totalNorm,
            });

            if (leak > maxLeak) maxLeak = leak;
            sumLeak += leak;
        }

        double meanLeak = entries.Count > 0 ? sumLeak / entries.Count : 0.0;

        return new GaugeLeakReport
        {
            BackgroundId = projector.BackgroundId,
            GaugeRank = projector.GaugeRank,
            Entries = entries,
            MaxLeakScore = maxLeak,
            MeanLeakScore = meanLeak,
        };
    }
}

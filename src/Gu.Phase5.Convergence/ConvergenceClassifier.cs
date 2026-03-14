namespace Gu.Phase5.Convergence;

/// <summary>
/// Classifies convergence behavior from multi-level refinement data.
///
/// Classifications (physicist-confirmed):
///   "convergent"        -- successive |deltas| decrease monotonically and p > 0
///   "weakly-convergent" -- deltas decrease on average but p &lt; 0.5 or non-monotone
///   "non-convergent"    -- deltas do not decrease (diverging or stagnant)
///   "insufficient-data" -- fewer than 3 refinement levels
/// </summary>
public static class ConvergenceClassifier
{
    /// <summary>
    /// Classify convergence from mesh parameters and corresponding quantity values.
    /// meshParameters and values must be the same length.
    /// meshParameters should be ordered coarsest to finest (decreasing h).
    /// </summary>
    /// <returns>Classification string and confidence note.</returns>
    public static (string Classification, string ConfidenceNote) Classify(
        double[] meshParameters, double[] values)
    {
        int n = meshParameters.Length;
        if (n < 3)
            return ("insufficient-data",
                $"Only {n} refinement level(s); need >= 3 for reliable classification.");

        // Compute successive |delta Q| = |Q(h_{i+1}) - Q(h_i)|
        var deltas = new double[n - 1];
        for (int i = 0; i < n - 1; i++)
            deltas[i] = System.Math.Abs(values[i + 1] - values[i]);

        // Check monotone decrease
        bool monotoneDecreasing = true;
        for (int i = 0; i < deltas.Length - 1; i++)
        {
            if (deltas[i + 1] >= deltas[i] * 1.05) // 5% tolerance
            {
                monotoneDecreasing = false;
                break;
            }
        }

        // Check overall trend: last delta vs first delta
        double firstDelta = deltas[0];
        double lastDelta = deltas[^1];
        bool overallDecreasing = lastDelta < firstDelta * 0.95;

        // Try Richardson fit to estimate convergence order
        double estimatedOrder = EstimateOrder(meshParameters, values);

        if (monotoneDecreasing && estimatedOrder >= 0.5)
            return ("convergent",
                $"Monotone decrease; estimated order p={estimatedOrder:F2}.");

        if (overallDecreasing && estimatedOrder > 0.0)
            return ("weakly-convergent",
                $"Overall decreasing but non-monotone or p={estimatedOrder:F2} < 0.5.");

        // Non-convergent: deltas not decreasing
        return ("non-convergent",
            $"Successive deltas not consistently decreasing (first={firstDelta:G4}, last={lastDelta:G4}).");
    }

    private static double EstimateOrder(double[] hs, double[] qs)
    {
        int n = hs.Length;
        if (n < 3) return 1.0;

        // Use last 3 points classical formula
        double hA = hs[n - 3], qA = qs[n - 3];
        double hB = hs[n - 2], qB = qs[n - 2];
        double hC = hs[n - 1], qC = qs[n - 1];

        double dqAB = qA - qB;
        double dqBC = qB - qC;
        if (System.Math.Abs(dqAB) < 1e-15 || System.Math.Abs(dqBC) < 1e-15)
            return 0.0;
        if (System.Math.Sign(dqAB) != System.Math.Sign(dqBC))
            return 0.0;

        double logH = System.Math.Log(hA / hB);
        if (System.Math.Abs(logH) < 1e-15) return 0.0;

        double p = System.Math.Log(System.Math.Abs(dqAB / dqBC)) / logH;
        return System.Math.Max(0.0, p);
    }
}

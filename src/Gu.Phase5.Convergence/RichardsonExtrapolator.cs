namespace Gu.Phase5.Convergence;

/// <summary>
/// Richardson extrapolation: given Q(h) at multiple h values,
/// estimate Q(0) and convergence order p.
///
/// For three levels h1 > h2 > h3 with Q1, Q2, Q3 (classical 3-point):
///   p = log((Q1-Q2)/(Q2-Q3)) / log(h1/h2)   [assumes h2/h3 = h1/h2]
///   Q_extrap = Q3 + (Q3-Q2) / (r^p - 1)       where r = h2/h3
///
/// For more than 3 levels or non-uniform ratios, least-squares fit of
///   Q(h) = Q_0 + C * h^p
/// using grid-search over p in [0.25, 6.0].
/// </summary>
public static class RichardsonExtrapolator
{
    /// <summary>
    /// Extrapolate the continuum limit from multi-level data.
    /// meshParameters and values must be the same length (>= 2).
    /// meshParameters should be ordered coarsest to finest (decreasing h).
    /// </summary>
    public static RichardsonFitRecord Extrapolate(
        string quantityId,
        double[] meshParameters,
        double[] values)
    {
        if (meshParameters.Length != values.Length)
            throw new ArgumentException("meshParameters and values must be the same length.");
        if (meshParameters.Length < 2)
            throw new ArgumentException("At least 2 refinement levels are required.");

        int n = meshParameters.Length;

        if (n == 2)
        {
            // With only 2 points: assume p=1, estimate limit by linear extrapolation.
            double h1 = meshParameters[0]; double q1 = values[0];
            double h2 = meshParameters[1]; double q2 = values[1];
            // Q(h) = Q0 + C*h^1 => Q0 = Q2 - (Q1-Q2)*h2/(h1-h2)
            double dh = h1 - h2;
            double q0 = System.Math.Abs(dh) < 1e-15 ? q2 : q2 - (q1 - q2) * h2 / dh;
            double residual = System.Math.Abs(q1 - (q0 + (q1 - q0) * (h1 / h2)));
            return new RichardsonFitRecord
            {
                QuantityId = quantityId,
                EstimatedLimit = q0,
                EstimatedOrder = 1.0,
                Residual = residual,
                MeshParameters = (double[])meshParameters.Clone(),
                Values = (double[])values.Clone(),
            };
        }

        // Try classical 3-point formula using the last 3 points.
        double hA = meshParameters[n - 3]; double qA = values[n - 3];
        double hB = meshParameters[n - 2]; double qB = values[n - 2];
        double hC = meshParameters[n - 1]; double qC = values[n - 1];

        double dqAB = qA - qB;
        double dqBC = qB - qC;

        double p3 = 1.0; // fallback
        double q0_3;
        if (System.Math.Abs(dqAB) > 1e-15 && System.Math.Abs(dqBC) > 1e-15
            && System.Math.Sign(dqAB) == System.Math.Sign(dqBC))
        {
            double logRatio = System.Math.Log(System.Math.Abs(dqAB / dqBC));
            double logH = System.Math.Log(hA / hB);
            if (System.Math.Abs(logH) > 1e-15)
                p3 = logRatio / logH;
            // clamp to reasonable range
            p3 = System.Math.Max(0.1, System.Math.Min(10.0, p3));
        }

        double r = hB / hC;
        double rp = System.Math.Pow(r, p3);
        q0_3 = System.Math.Abs(rp - 1.0) > 1e-15
            ? qC + (qC - qB) / (rp - 1.0)
            : qC;

        if (n == 3)
        {
            double res3 = ComputeFitResidual(meshParameters, values, q0_3, p3);
            return new RichardsonFitRecord
            {
                QuantityId = quantityId,
                EstimatedLimit = q0_3,
                EstimatedOrder = p3,
                Residual = res3,
                MeshParameters = (double[])meshParameters.Clone(),
                Values = (double[])values.Clone(),
            };
        }

        // n >= 4: use least-squares grid-search over p.
        double bestP = p3;
        double bestQ0 = q0_3;
        double bestRes = ComputeFitResidual(meshParameters, values, q0_3, p3);

        int gridSteps = 200;
        double pMin = 0.25, pMax = 6.0;
        for (int i = 0; i <= gridSteps; i++)
        {
            double pTry = pMin + (pMax - pMin) * i / gridSteps;
            double q0Try = LeastSquaresLimit(meshParameters, values, pTry);
            double resTry = ComputeFitResidual(meshParameters, values, q0Try, pTry);
            if (resTry < bestRes)
            {
                bestRes = resTry;
                bestQ0 = q0Try;
                bestP = pTry;
            }
        }

        return new RichardsonFitRecord
        {
            QuantityId = quantityId,
            EstimatedLimit = bestQ0,
            EstimatedOrder = bestP,
            Residual = bestRes,
            MeshParameters = (double[])meshParameters.Clone(),
            Values = (double[])values.Clone(),
        };
    }

    /// <summary>
    /// Given fixed p, find Q0 by linear least-squares on Q(h) = Q0 + C * h^p.
    /// </summary>
    private static double LeastSquaresLimit(double[] hs, double[] qs, double p)
    {
        int n = hs.Length;
        // Normal equations for [Q0, C]:
        // [n,  sum(h^p)] [Q0]   [sum(Q)]
        // [sum(h^p), sum(h^2p)] [C] = [sum(Q*h^p)]
        double sumHp = 0, sumH2p = 0, sumQ = 0, sumQHp = 0;
        for (int i = 0; i < n; i++)
        {
            double hp = System.Math.Pow(hs[i], p);
            sumHp += hp;
            sumH2p += hp * hp;
            sumQ += qs[i];
            sumQHp += qs[i] * hp;
        }
        double det = n * sumH2p - sumHp * sumHp;
        if (System.Math.Abs(det) < 1e-15)
            return sumQ / n;
        return (sumQ * sumH2p - sumQHp * sumHp) / det;
    }

    private static double ComputeFitResidual(double[] hs, double[] qs, double q0, double p)
    {
        double c = EstimateC(hs, qs, q0, p);
        double sumSq = 0;
        for (int i = 0; i < hs.Length; i++)
        {
            double predicted = q0 + c * System.Math.Pow(hs[i], p);
            double diff = qs[i] - predicted;
            sumSq += diff * diff;
        }
        return System.Math.Sqrt(sumSq / hs.Length);
    }

    private static double EstimateC(double[] hs, double[] qs, double q0, double p)
    {
        double num = 0, den = 0;
        for (int i = 0; i < hs.Length; i++)
        {
            double hp = System.Math.Pow(hs[i], p);
            num += (qs[i] - q0) * hp;
            den += hp * hp;
        }
        return den < 1e-30 ? 0.0 : num / den;
    }
}

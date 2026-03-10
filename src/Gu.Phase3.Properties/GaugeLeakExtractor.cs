using Gu.Core;
using Gu.Math;
using Gu.Phase3.GaugeReduction;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties;

/// <summary>
/// Extracts gauge contamination score for a mode.
/// gaugeLeak(v) = || P_gauge v || / ||v||
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.5)
///
/// This is a first-class property because it controls claim demotion.
/// </summary>
public sealed class GaugeLeakExtractor
{
    private readonly GaugeProjector _projector;

    public GaugeLeakExtractor(GaugeProjector projector)
    {
        _projector = projector ?? throw new ArgumentNullException(nameof(projector));
    }

    /// <summary>
    /// Compute gauge leak score for a mode vector.
    /// Returns || P_gauge v || / ||v||, or 0 if ||v|| ~ 0.
    /// </summary>
    public double ComputeLeakScore(double[] modeVector)
    {
        ArgumentNullException.ThrowIfNull(modeVector);

        double normV = 0;
        for (int i = 0; i < modeVector.Length; i++)
            normV += modeVector[i] * modeVector[i];
        normV = System.Math.Sqrt(normV);

        if (normV < 1e-30) return 0;

        // Apply gauge projector to get the gauge component
        var field = new FieldTensor
        {
            Label = "mode",
            Signature = _projector.InputSignature,
            Coefficients = modeVector,
            Shape = new[] { modeVector.Length },
        };

        // P_gauge = I - P_phys, but the GaugeProjector applies P_phys (complement).
        // We want the gauge part: P_gauge v = v - P_phys v
        var physPart = _projector.Apply(field);
        double normGauge = 0;
        for (int i = 0; i < modeVector.Length; i++)
        {
            double gaugeComp = modeVector[i] - physPart.Coefficients[i];
            normGauge += gaugeComp * gaugeComp;
        }
        normGauge = System.Math.Sqrt(normGauge);

        return normGauge / normV;
    }

    /// <summary>
    /// Compute gauge leak for a ModeRecord (uses stored gauge leak if consistent).
    /// </summary>
    public double ComputeLeakScore(ModeRecord mode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        return ComputeLeakScore(mode.ModeVector);
    }
}

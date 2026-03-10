using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties;

/// <summary>
/// Extracts polarization / tensor class from mode vectors.
/// Measures how the mode energy distributes among state-space blocks.
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.3)
///
/// For the single-connection-block case (Phase I compatible),
/// energy is computed per edge group or per Lie algebra component.
/// </summary>
public sealed class PolarizationExtractor
{
    private readonly int _edgeCount;
    private readonly int _dimG;

    public PolarizationExtractor(SimplicialMesh mesh, LieAlgebra algebra)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(algebra);
        _edgeCount = mesh.EdgeCount;
        _dimG = algebra.Dimension;
    }

    /// <summary>
    /// Extract polarization descriptor from a mode vector.
    /// For single-block connection modes, decomposes energy by Lie algebra component.
    /// </summary>
    public PolarizationDescriptor Extract(ModeRecord mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        var v = mode.ModeVector;
        int total = _edgeCount * _dimG;

        // Energy per Lie algebra component
        var componentEnergy = new double[_dimG];
        double totalEnergy = 0;

        for (int e = 0; e < _edgeCount && e * _dimG < v.Length; e++)
        {
            for (int a = 0; a < _dimG && e * _dimG + a < v.Length; a++)
            {
                double val = v[e * _dimG + a];
                double energy = val * val;
                componentEnergy[a] += energy;
                totalEnergy += energy;
            }
        }

        var fractions = new Dictionary<string, double>();
        if (totalEnergy > 0)
        {
            for (int a = 0; a < _dimG; a++)
                fractions[$"algebra-component-{a}"] = componentEnergy[a] / totalEnergy;
        }
        else
        {
            for (int a = 0; a < _dimG; a++)
                fractions[$"algebra-component-{a}"] = 1.0 / _dimG;
        }

        // Also compute connection block fraction (always 1.0 for single-block)
        fractions["connection"] = 1.0;

        // Determine dominant class
        double maxFrac = 0;
        string dominantClass = "mixed";

        // Check if energy is concentrated in one algebra component
        for (int a = 0; a < _dimG; a++)
        {
            if (totalEnergy > 0 && componentEnergy[a] / totalEnergy > maxFrac)
                maxFrac = componentEnergy[a] / totalEnergy;
        }

        if (maxFrac > 0.8)
            dominantClass = "connection-dominant";
        else if (_dimG > 1 && maxFrac < 1.0 / _dimG + 0.1)
            dominantClass = "scalar-like"; // Evenly distributed
        else
            dominantClass = "mixed";

        return new PolarizationDescriptor
        {
            ModeId = mode.ModeId,
            BlockEnergyFractions = fractions,
            DominantClass = dominantClass,
            DominanceFraction = maxFrac,
            BackgroundId = mode.BackgroundId,
        };
    }
}

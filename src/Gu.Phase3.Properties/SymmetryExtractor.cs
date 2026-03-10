using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties;

/// <summary>
/// Extracts symmetry content from mode vectors.
/// Measures how a mode transforms under available discrete symmetries
/// by computing overlap with symmetry sectors.
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.4)
///
/// For the basic case (no explicit symmetry group supplied),
/// computes parity eigenvalue by checking v(-x) vs v(x) overlap,
/// and distributes energy by Lie algebra component sectors.
/// </summary>
public sealed class SymmetryExtractor
{
    private readonly int _edgeCount;
    private readonly int _dimG;

    public SymmetryExtractor(int edgeCount, int dimG)
    {
        if (edgeCount < 1) throw new ArgumentOutOfRangeException(nameof(edgeCount));
        if (dimG < 1) throw new ArgumentOutOfRangeException(nameof(dimG));
        _edgeCount = edgeCount;
        _dimG = dimG;
    }

    /// <summary>
    /// Extract symmetry descriptor from a mode record.
    /// </summary>
    public SymmetryDescriptor Extract(ModeRecord mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        var v = mode.ModeVector;
        int total = _edgeCount * _dimG;

        // Compute parity eigenvalue: check if mode is mostly even or odd
        // under index reversal (proxy for spatial parity on simple meshes).
        double? parityEigenvalue = ComputeParityEigenvalue(v, total);

        // Compute sector overlaps: fraction of energy in each Lie algebra component
        var sectorOverlaps = ComputeSectorOverlaps(v, total);

        // Assign symmetry labels based on dominant sectors and parity
        var labels = new List<string>();
        if (parityEigenvalue.HasValue)
        {
            labels.Add(parityEigenvalue.Value > 0 ? "parity-even" : "parity-odd");
        }

        // Find dominant algebra component
        string? dominantSector = null;
        double maxOverlap = 0;
        foreach (var (key, value) in sectorOverlaps)
        {
            if (value > maxOverlap)
            {
                maxOverlap = value;
                dominantSector = key;
            }
        }

        if (dominantSector != null && maxOverlap > 0.5)
        {
            labels.Add($"dominant-{dominantSector}");
        }

        return new SymmetryDescriptor
        {
            ModeId = mode.ModeId,
            ParityEigenvalue = parityEigenvalue,
            SymmetryLabels = labels,
            SectorOverlaps = sectorOverlaps,
            BackgroundId = mode.BackgroundId,
        };
    }

    /// <summary>
    /// Compute parity eigenvalue by measuring overlap of v with its index-reversed copy.
    /// Returns +1 if mostly even, -1 if mostly odd, null if neither dominates.
    /// </summary>
    internal static double? ComputeParityEigenvalue(double[] v, int effectiveLength)
    {
        int n = System.Math.Min(v.Length, effectiveLength);
        if (n < 2) return null;

        // Compute overlap: sum v[i]*v[n-1-i] vs sum v[i]^2
        double evenOverlap = 0;
        double normSq = 0;

        for (int i = 0; i < n; i++)
        {
            normSq += v[i] * v[i];
            evenOverlap += v[i] * v[n - 1 - i];
        }

        if (normSq < 1e-30) return null;

        double ratio = evenOverlap / normSq;

        // If |ratio| > threshold, assign parity
        if (ratio > 0.5) return 1.0;
        if (ratio < -0.5) return -1.0;
        return null; // Mixed parity
    }

    /// <summary>
    /// Compute energy overlap with each Lie algebra component sector.
    /// </summary>
    internal Dictionary<string, double> ComputeSectorOverlaps(double[] v, int effectiveLength)
    {
        var overlaps = new Dictionary<string, double>();
        double totalEnergy = 0;
        var componentEnergy = new double[_dimG];

        int n = System.Math.Min(v.Length, effectiveLength);
        for (int e = 0; e < _edgeCount && e * _dimG < n; e++)
        {
            for (int a = 0; a < _dimG && e * _dimG + a < n; a++)
            {
                double val = v[e * _dimG + a];
                double energy = val * val;
                componentEnergy[a] += energy;
                totalEnergy += energy;
            }
        }

        for (int a = 0; a < _dimG; a++)
        {
            string sectorLabel = $"algebra-{a}";
            overlaps[sectorLabel] = totalEnergy > 0
                ? componentEnergy[a] / totalEnergy
                : 1.0 / _dimG;
        }

        return overlaps;
    }
}

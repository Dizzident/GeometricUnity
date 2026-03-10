using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.GaugeReduction;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties;

/// <summary>
/// Assembles a <see cref="BosonPropertyVector"/> for each mode by running all extractors.
/// </summary>
public sealed class PropertyVectorBuilder
{
    private readonly PolarizationExtractor _polarization;
    private readonly GaugeLeakExtractor? _gaugeLeak;
    private readonly IReadOnlyDictionary<string, int> _clusterMultiplicities;

    /// <param name="mesh">Simplicial mesh for polarization extraction.</param>
    /// <param name="algebra">Lie algebra for polarization extraction.</param>
    /// <param name="gaugeProjector">Gauge projector for leak scores (null to use stored values).</param>
    /// <param name="clusterMultiplicities">Cluster multiplicities keyed by cluster ID.</param>
    public PropertyVectorBuilder(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        GaugeProjector? gaugeProjector = null,
        IReadOnlyDictionary<string, int>? clusterMultiplicities = null)
    {
        _polarization = new PolarizationExtractor(mesh, algebra);
        _gaugeLeak = gaugeProjector is not null ? new GaugeLeakExtractor(gaugeProjector) : null;
        _clusterMultiplicities = clusterMultiplicities ?? new Dictionary<string, int>();
    }

    /// <summary>
    /// Build a property vector for a single mode.
    /// </summary>
    public BosonPropertyVector Build(ModeRecord mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        var massScale = MassLikeScaleExtractor.Extract(mode);
        var polarization = _polarization.Extract(mode);

        double gaugeLeakScore = _gaugeLeak is not null
            ? _gaugeLeak.ComputeLeakScore(mode)
            : mode.GaugeLeakScore;

        int multiplicity = 1;
        if (mode.MultiplicityClusterId is not null &&
            _clusterMultiplicities.TryGetValue(mode.MultiplicityClusterId, out int clusterMult))
        {
            multiplicity = clusterMult;
        }

        var symmetry = new SymmetryDescriptor
        {
            ModeId = mode.ModeId,
            BackgroundId = mode.BackgroundId,
        };

        return new BosonPropertyVector
        {
            ModeId = mode.ModeId,
            BackgroundId = mode.BackgroundId,
            MassLikeScale = massScale,
            Polarization = polarization,
            Symmetry = symmetry,
            GaugeLeakScore = gaugeLeakScore,
            Multiplicity = multiplicity,
        };
    }

    /// <summary>
    /// Build property vectors for all modes.
    /// </summary>
    public IReadOnlyList<BosonPropertyVector> BuildAll(IEnumerable<ModeRecord> modes)
    {
        ArgumentNullException.ThrowIfNull(modes);
        return modes.Select(Build).ToList();
    }
}

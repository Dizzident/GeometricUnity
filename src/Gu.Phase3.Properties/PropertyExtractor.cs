using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties;

/// <summary>
/// End-to-end property extraction pipeline.
/// Takes a ModeRecord (and optional context) and produces a BosonPropertyVector
/// aggregating all extracted properties.
/// (See IMPLEMENTATION_PLAN_P3.md Section 7.7)
///
/// Pipeline:
/// 1. Extract mass-like scale from eigenvalue
/// 2. Extract polarization descriptor from mode vector
/// 3. Extract symmetry descriptor from mode vector
/// 4. Read gauge leak score from mode record
/// 5. Determine multiplicity from cluster (if available)
/// 6. Optionally attach stability scores and interaction proxies
/// </summary>
public sealed class PropertyExtractor
{
    private readonly PolarizationExtractor _polarizationExtractor;
    private readonly SymmetryExtractor _symmetryExtractor;
    private readonly int _defaultMultiplicity;

    /// <summary>
    /// Create a property extractor.
    /// </summary>
    /// <param name="mesh">The ambient mesh.</param>
    /// <param name="algebra">The gauge Lie algebra.</param>
    /// <param name="defaultMultiplicity">Default multiplicity when no cluster is available.</param>
    public PropertyExtractor(SimplicialMesh mesh, LieAlgebra algebra, int defaultMultiplicity = 1)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(algebra);

        _polarizationExtractor = new PolarizationExtractor(mesh, algebra);
        _symmetryExtractor = new SymmetryExtractor(mesh.EdgeCount, algebra.Dimension);
        _defaultMultiplicity = defaultMultiplicity;
    }

    /// <summary>
    /// Extract a full BosonPropertyVector from a mode record.
    /// </summary>
    /// <param name="mode">The mode record to extract properties from.</param>
    /// <param name="cluster">
    /// Optional mode cluster for multiplicity.
    /// If null, multiplicity defaults to <see cref="_defaultMultiplicity"/>.
    /// </param>
    /// <param name="stability">Optional pre-computed stability score card.</param>
    /// <param name="interactionProxies">Optional pre-computed interaction proxies.</param>
    /// <returns>Aggregated property vector.</returns>
    public BosonPropertyVector Extract(
        ModeRecord mode,
        ModeCluster? cluster = null,
        StabilityScoreCard? stability = null,
        IReadOnlyList<InteractionProxyRecord>? interactionProxies = null)
    {
        ArgumentNullException.ThrowIfNull(mode);

        var massScale = MassLikeScaleExtractor.Extract(mode);
        var polarization = _polarizationExtractor.Extract(mode);
        var symmetry = _symmetryExtractor.Extract(mode);

        int multiplicity = cluster?.Multiplicity ?? _defaultMultiplicity;

        return new BosonPropertyVector
        {
            ModeId = mode.ModeId,
            BackgroundId = mode.BackgroundId,
            MassLikeScale = massScale,
            Polarization = polarization,
            Symmetry = symmetry,
            GaugeLeakScore = mode.GaugeLeakScore,
            Multiplicity = multiplicity,
            Stability = stability,
            InteractionProxies = interactionProxies ?? Array.Empty<InteractionProxyRecord>(),
        };
    }

    /// <summary>
    /// Extract property vectors for all modes in a spectrum.
    /// </summary>
    /// <param name="modes">Mode records.</param>
    /// <param name="clusters">Optional clusters keyed by cluster ID.</param>
    /// <returns>List of property vectors, one per mode.</returns>
    public IReadOnlyList<BosonPropertyVector> ExtractAll(
        IEnumerable<ModeRecord> modes,
        IReadOnlyDictionary<string, ModeCluster>? clusters = null)
    {
        ArgumentNullException.ThrowIfNull(modes);

        var results = new List<BosonPropertyVector>();

        foreach (var mode in modes)
        {
            ModeCluster? cluster = null;
            if (clusters != null && mode.MultiplicityClusterId != null)
            {
                clusters.TryGetValue(mode.MultiplicityClusterId, out cluster);
            }
            results.Add(Extract(mode, cluster));
        }

        return results;
    }
}

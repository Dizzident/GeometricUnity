using Gu.Core;
using Gu.Observation;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches;

/// <summary>
/// Resolves observation/extraction pipeline components for a given branch variant.
/// Maps ObservationVariant and ExtractionVariant to pipeline configuration.
/// </summary>
public sealed class ObservationVariantDispatch
{
    private readonly Dictionary<string, Func<BranchManifest, IReadOnlyList<IDerivedObservableTransform>>> _transformFactories = new();
    private readonly Dictionary<string, Func<BranchManifest, INormalizationPolicy>> _normalizationFactories = new();
    private readonly Dictionary<string, Func<BranchManifest, IReadOnlyList<ObservableRequest>>> _requestFactories = new();

    /// <summary>
    /// Register a factory for derived observable transforms keyed by observation variant ID.
    /// </summary>
    public void RegisterTransforms(string observationVariantId, Func<BranchManifest, IReadOnlyList<IDerivedObservableTransform>> factory)
    {
        ArgumentNullException.ThrowIfNull(observationVariantId);
        ArgumentNullException.ThrowIfNull(factory);
        _transformFactories[observationVariantId] = factory;
    }

    /// <summary>
    /// Register a factory for normalization policy keyed by extraction variant ID.
    /// </summary>
    public void RegisterNormalization(string extractionVariantId, Func<BranchManifest, INormalizationPolicy> factory)
    {
        ArgumentNullException.ThrowIfNull(extractionVariantId);
        ArgumentNullException.ThrowIfNull(factory);
        _normalizationFactories[extractionVariantId] = factory;
    }

    /// <summary>
    /// Register a factory for observable requests keyed by observation variant ID.
    /// </summary>
    public void RegisterRequests(string observationVariantId, Func<BranchManifest, IReadOnlyList<ObservableRequest>> factory)
    {
        ArgumentNullException.ThrowIfNull(observationVariantId);
        ArgumentNullException.ThrowIfNull(factory);
        _requestFactories[observationVariantId] = factory;
    }

    /// <summary>
    /// Resolve observation configuration for a variant.
    /// </summary>
    public ResolvedObservationConfig Resolve(
        BranchVariantManifest variant,
        BranchManifest baseManifest)
    {
        var manifest = BranchVariantResolver.Resolve(variant, baseManifest);
        return Resolve(manifest, variant.ObservationVariant, variant.ExtractionVariant);
    }

    /// <summary>
    /// Resolve observation configuration from variant IDs and a Phase I manifest.
    /// </summary>
    public ResolvedObservationConfig Resolve(
        BranchManifest manifest,
        string observationVariantId,
        string extractionVariantId)
    {
        var transforms = ResolveTransforms(observationVariantId, manifest);
        var normalization = ResolveNormalization(extractionVariantId, manifest);
        var requests = ResolveRequests(observationVariantId, manifest);

        return new ResolvedObservationConfig
        {
            Manifest = manifest,
            ObservationVariantId = observationVariantId,
            ExtractionVariantId = extractionVariantId,
            Transforms = transforms,
            Normalization = normalization,
            Requests = requests,
        };
    }

    private IReadOnlyList<IDerivedObservableTransform> ResolveTransforms(string observationVariantId, BranchManifest manifest)
    {
        if (_transformFactories.TryGetValue(observationVariantId, out var factory))
            return factory(manifest);

        // Default: empty transforms list (direct field passthrough only)
        return Array.Empty<IDerivedObservableTransform>();
    }

    private INormalizationPolicy ResolveNormalization(string extractionVariantId, BranchManifest manifest)
    {
        if (_normalizationFactories.TryGetValue(extractionVariantId, out var factory))
            return factory(manifest);

        // Default: dimensionless normalization
        return new DimensionlessNormalizationPolicy();
    }

    private IReadOnlyList<ObservableRequest> ResolveRequests(string observationVariantId, BranchManifest manifest)
    {
        if (_requestFactories.TryGetValue(observationVariantId, out var factory))
            return factory(manifest);

        // Default: standard set of observable requests
        return DefaultObservableRequests();
    }

    /// <summary>
    /// Standard observable requests matching Phase I direct fields.
    /// </summary>
    public static IReadOnlyList<ObservableRequest> DefaultObservableRequests()
    {
        return new ObservableRequest[]
        {
            new()
            {
                ObservableId = "curvature",
                OutputType = OutputType.ExactStructural,
            },
            new()
            {
                ObservableId = "torsion",
                OutputType = OutputType.ExactStructural,
            },
            new()
            {
                ObservableId = "shiab",
                OutputType = OutputType.ExactStructural,
            },
            new()
            {
                ObservableId = "residual",
                OutputType = OutputType.ExactStructural,
            },
        };
    }
}

/// <summary>
/// Fully resolved observation/extraction configuration for a branch variant.
/// </summary>
public sealed class ResolvedObservationConfig
{
    /// <summary>Phase I manifest for pipeline execution.</summary>
    public required BranchManifest Manifest { get; init; }

    /// <summary>Observation variant identifier.</summary>
    public required string ObservationVariantId { get; init; }

    /// <summary>Extraction variant identifier.</summary>
    public required string ExtractionVariantId { get; init; }

    /// <summary>Derived observable transforms.</summary>
    public required IReadOnlyList<IDerivedObservableTransform> Transforms { get; init; }

    /// <summary>Normalization policy.</summary>
    public required INormalizationPolicy Normalization { get; init; }

    /// <summary>Observable requests to extract.</summary>
    public required IReadOnlyList<ObservableRequest> Requests { get; init; }
}

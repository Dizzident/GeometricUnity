using Gu.Branching;
using Gu.Core;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches;

/// <summary>
/// Resolves Phase I branch operators for a given variant.
/// Ensures no solver path depends on hidden global defaults:
/// every operator is resolved from the variant's explicit configuration.
/// </summary>
public sealed class BranchVariantOperatorDispatch
{
    private readonly BranchOperatorRegistry _registry;

    public BranchVariantOperatorDispatch(BranchOperatorRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <summary>
    /// Resolve all operators for a variant using BranchVariantResolver to bridge to Phase I.
    /// </summary>
    public ResolvedBranchOperators Resolve(
        BranchVariantManifest variant,
        BranchManifest baseManifest)
    {
        var manifest = BranchVariantResolver.Resolve(variant, baseManifest);
        return Resolve(manifest);
    }

    /// <summary>
    /// Resolve all operators from a Phase I manifest directly.
    /// </summary>
    public ResolvedBranchOperators Resolve(BranchManifest manifest)
    {
        var torsion = _registry.ResolveTorsion(manifest);
        var shiab = _registry.ResolveShiab(manifest);
        var biConnection = _registry.ResolveBiConnection(manifest);

        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);

        return new ResolvedBranchOperators
        {
            Manifest = manifest,
            Torsion = torsion,
            Shiab = shiab,
            BiConnection = biConnection,
        };
    }
}

/// <summary>
/// A fully resolved set of branch operators ready for execution.
/// </summary>
public sealed class ResolvedBranchOperators
{
    /// <summary>The Phase I manifest these operators were resolved from.</summary>
    public required BranchManifest Manifest { get; init; }

    /// <summary>Resolved torsion operator.</summary>
    public required ITorsionBranchOperator Torsion { get; init; }

    /// <summary>Resolved Shiab operator.</summary>
    public required IShiabBranchOperator Shiab { get; init; }

    /// <summary>Resolved bi-connection strategy.</summary>
    public required IBiConnectionStrategy BiConnection { get; init; }
}

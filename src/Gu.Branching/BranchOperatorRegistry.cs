using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Registry for branch operators. Implementations register at startup.
/// Resolves operators based on manifest branch IDs.
/// </summary>
public sealed class BranchOperatorRegistry
{
    private readonly Dictionary<string, Func<BranchManifest, ITorsionBranchOperator>> _torsionFactories = new();
    private readonly Dictionary<string, Func<BranchManifest, IShiabBranchOperator>> _shiabFactories = new();
    private readonly Dictionary<string, Func<BranchManifest, IBiConnectionStrategy>> _biConnectionFactories = new();

    public void RegisterTorsion(string branchId, Func<BranchManifest, ITorsionBranchOperator> factory)
    {
        ArgumentNullException.ThrowIfNull(branchId);
        ArgumentNullException.ThrowIfNull(factory);
        _torsionFactories[branchId] = factory;
    }

    public void RegisterShiab(string branchId, Func<BranchManifest, IShiabBranchOperator> factory)
    {
        ArgumentNullException.ThrowIfNull(branchId);
        ArgumentNullException.ThrowIfNull(factory);
        _shiabFactories[branchId] = factory;
    }

    public void RegisterBiConnection(string strategyId, Func<BranchManifest, IBiConnectionStrategy> factory)
    {
        ArgumentNullException.ThrowIfNull(strategyId);
        ArgumentNullException.ThrowIfNull(factory);
        _biConnectionFactories[strategyId] = factory;
    }

    /// <summary>
    /// Resolve a torsion operator from manifest.ActiveTorsionBranch.
    /// </summary>
    public ITorsionBranchOperator ResolveTorsion(BranchManifest manifest)
    {
        if (!_torsionFactories.TryGetValue(manifest.ActiveTorsionBranch, out var factory))
            throw new InvalidOperationException(
                $"No torsion operator registered for branch '{manifest.ActiveTorsionBranch}'. " +
                $"Available: [{string.Join(", ", _torsionFactories.Keys)}]");
        return factory(manifest);
    }

    /// <summary>
    /// Resolve a Shiab operator from manifest.ActiveShiabBranch.
    /// </summary>
    public IShiabBranchOperator ResolveShiab(BranchManifest manifest)
    {
        if (!_shiabFactories.TryGetValue(manifest.ActiveShiabBranch, out var factory))
            throw new InvalidOperationException(
                $"No Shiab operator registered for branch '{manifest.ActiveShiabBranch}'. " +
                $"Available: [{string.Join(", ", _shiabFactories.Keys)}]");
        return factory(manifest);
    }

    /// <summary>
    /// Resolve a bi-connection strategy. Uses manifest.Parameters["biConnectionStrategy"]
    /// if present, otherwise defaults to "simple-a0-omega".
    /// </summary>
    public IBiConnectionStrategy ResolveBiConnection(BranchManifest manifest)
    {
        var strategyId = "simple-a0-omega";
        if (manifest.Parameters != null &&
            manifest.Parameters.TryGetValue("biConnectionStrategy", out var customId))
        {
            strategyId = customId;
        }

        if (!_biConnectionFactories.TryGetValue(strategyId, out var factory))
            throw new InvalidOperationException(
                $"No bi-connection strategy registered for '{strategyId}'. " +
                $"Available: [{string.Join(", ", _biConnectionFactories.Keys)}]");
        return factory(manifest);
    }

    /// <summary>
    /// Validate that resolved torsion and Shiab operators have strictly identical
    /// output TensorSignatures. All fields must match (CarrierType, AmbientSpaceId,
    /// Degree, LieAlgebraBasisId, ComponentOrderId, NumericPrecision, MemoryLayout)
    /// for Upsilon = S - T to be well-defined.
    /// Per physicist confirmation: strict signature identity is physically required.
    /// </summary>
    public static void ValidateCarrierMatch(
        ITorsionBranchOperator torsion,
        IShiabBranchOperator shiab)
    {
        var ts = torsion.OutputSignature;
        var ss = shiab.OutputSignature;
        var mismatches = new List<string>();

        if (ts.CarrierType != ss.CarrierType)
            mismatches.Add($"CarrierType ('{ts.CarrierType}' vs '{ss.CarrierType}')");
        if (ts.AmbientSpaceId != ss.AmbientSpaceId)
            mismatches.Add($"AmbientSpaceId ('{ts.AmbientSpaceId}' vs '{ss.AmbientSpaceId}')");
        if (ts.Degree != ss.Degree)
            mismatches.Add($"Degree ('{ts.Degree}' vs '{ss.Degree}')");
        if (ts.LieAlgebraBasisId != ss.LieAlgebraBasisId)
            mismatches.Add($"LieAlgebraBasisId ('{ts.LieAlgebraBasisId}' vs '{ss.LieAlgebraBasisId}')");
        if (ts.ComponentOrderId != ss.ComponentOrderId)
            mismatches.Add($"ComponentOrderId ('{ts.ComponentOrderId}' vs '{ss.ComponentOrderId}')");
        if (ts.NumericPrecision != ss.NumericPrecision)
            mismatches.Add($"NumericPrecision ('{ts.NumericPrecision}' vs '{ss.NumericPrecision}')");
        if (ts.MemoryLayout != ss.MemoryLayout)
            mismatches.Add($"MemoryLayout ('{ts.MemoryLayout}' vs '{ss.MemoryLayout}')");

        if (mismatches.Count > 0)
            throw new InvalidOperationException(
                $"TensorSignature mismatch between torsion '{torsion.BranchId}' and " +
                $"Shiab '{shiab.BranchId}': {string.Join(", ", mismatches)}. " +
                $"Both must produce strictly identical TensorSignatures for Upsilon = S - T.");
    }
}

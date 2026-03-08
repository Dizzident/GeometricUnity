using Gu.Branching;
using Gu.Core;

namespace Gu.ReferenceCpu;

/// <summary>
/// Simplest bi-connection strategy: A = A0, B = omega.
/// Per physicist clarification: this makes d_{B_omega} = d_omega (covariant exterior derivative).
/// v29 Section 32.2 confirms: B_omega = omega.
/// </summary>
public sealed class SimpleBiConnectionCpu : IBiConnectionStrategy
{
    public string StrategyId => "simple-a0-omega";

    /// <summary>
    /// A = A0, B = omega.
    /// </summary>
    public BiConnectionResult Evaluate(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        return new BiConnectionResult
        {
            ConnectionA = new FieldTensor
            {
                Label = "A_omega",
                Signature = a0.Signature,
                Coefficients = (double[])a0.Coefficients.Clone(),
                Shape = a0.Shape,
            },
            ConnectionB = new FieldTensor
            {
                Label = "B_omega",
                Signature = omega.Signature,
                Coefficients = (double[])omega.Coefficients.Clone(),
                Shape = omega.Shape,
            },
        };
    }

    /// <summary>
    /// Linearization: dA/domega = 0, dB/domega = delta_omega.
    /// </summary>
    public BiConnectionResult Linearize(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        return new BiConnectionResult
        {
            ConnectionA = FieldTensorOps.ZerosLike(a0),
            ConnectionB = new FieldTensor
            {
                Label = "dB_omega",
                Signature = deltaOmega.Signature,
                Coefficients = (double[])deltaOmega.Coefficients.Clone(),
                Shape = deltaOmega.Shape,
            },
        };
    }
}

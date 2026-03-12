using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Fermions;

/// <summary>
/// A discrete primal fermionic state psi on Y_h.
///
/// The state is stored as a flat interleaved complex array:
/// coefficients[2*(cell*dofsPerCell + blockOffset + localIdx)    ] = Re(psi)
/// coefficients[2*(cell*dofsPerCell + blockOffset + localIdx) + 1] = Im(psi)
///
/// PhysicsNote: This is a test-space spinor field; physical comparison
/// must go through the observation chain (sigma_h pullback) before any
/// particle-physics interpretation.
/// </summary>
public sealed class DiscreteFermionState
{
    /// <summary>Unique identifier for this state.</summary>
    [JsonPropertyName("stateId")]
    public required string StateId { get; init; }

    /// <summary>Layout ID of the fermionic field layout.</summary>
    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    /// <summary>Number of Y-space cells (mesh cell count on Y_h).</summary>
    [JsonPropertyName("cellCount")]
    public required int CellCount { get; init; }

    /// <summary>Number of primal DOFs per cell (from layout.PrimalDofsPerCell).</summary>
    [JsonPropertyName("dofsPerCell")]
    public required int DofsPerCell { get; init; }

    /// <summary>
    /// Flat interleaved (Re, Im) array.
    /// Length = 2 * CellCount * DofsPerCell.
    /// </summary>
    [JsonPropertyName("coefficients")]
    public required double[] Coefficients { get; init; }

    /// <summary>Total complex DOF count = CellCount * DofsPerCell.</summary>
    [JsonIgnore]
    public int ComplexDofCount => CellCount * DofsPerCell;

    /// <summary>Provenance of this state.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>
    /// Get the real part of the spinor component at (cell, dofIndex).
    /// </summary>
    public double GetRe(int cell, int dofIndex)
    {
        int idx = 2 * (cell * DofsPerCell + dofIndex);
        return Coefficients[idx];
    }

    /// <summary>
    /// Get the imaginary part of the spinor component at (cell, dofIndex).
    /// </summary>
    public double GetIm(int cell, int dofIndex)
    {
        int idx = 2 * (cell * DofsPerCell + dofIndex);
        return Coefficients[idx + 1];
    }

    /// <summary>
    /// Compute the L2 norm: sqrt(sum |psi|^2).
    /// </summary>
    public double L2Norm()
    {
        double sum = 0;
        for (int i = 0; i < Coefficients.Length; i++)
            sum += Coefficients[i] * Coefficients[i];
        return System.Math.Sqrt(sum);
    }
}

/// <summary>
/// A discrete dual fermionic state psi_bar on Y_h.
///
/// Stores the conjugate / dual spinor used for bilinear evaluation.
/// For Riemannian signature: psi_bar = psi^dagger (Hermitian conjugate).
/// For Lorentzian: psi_bar = psi^dagger * Gamma_0 (Dirac adjoint).
/// The convention is fixed by the ConjugationConventionSpec in the layout.
///
/// Storage convention: same flat interleaved (Re, Im) layout as DiscreteFermionState.
/// </summary>
public sealed class DiscreteDualFermionState
{
    /// <summary>Unique identifier for this dual state.</summary>
    [JsonPropertyName("stateId")]
    public required string StateId { get; init; }

    /// <summary>ID of the primal state this was derived from (nullable if standalone).</summary>
    [JsonPropertyName("primalStateId")]
    public string? PrimalStateId { get; init; }

    /// <summary>Layout ID.</summary>
    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    /// <summary>Number of Y-space cells.</summary>
    [JsonPropertyName("cellCount")]
    public required int CellCount { get; init; }

    /// <summary>Number of primal DOFs per cell.</summary>
    [JsonPropertyName("dofsPerCell")]
    public required int DofsPerCell { get; init; }

    /// <summary>
    /// Flat interleaved (Re, Im) array for the dual spinor.
    /// Length = 2 * CellCount * DofsPerCell.
    /// </summary>
    [JsonPropertyName("coefficients")]
    public required double[] Coefficients { get; init; }

    /// <summary>Conjugation convention used to produce this dual state.</summary>
    [JsonPropertyName("conjugationType")]
    public required string ConjugationType { get; init; }

    /// <summary>Provenance of this dual state.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Total complex DOF count.</summary>
    [JsonIgnore]
    public int ComplexDofCount => CellCount * DofsPerCell;

    /// <summary>
    /// Evaluate the real part of the bilinear pairing &lt;psi_bar, psi&gt; = sum psi_bar^dagger * psi.
    /// The imaginary part should vanish for self-dual forms but is also computed.
    /// Returns (real, imaginary).
    /// </summary>
    public (double Re, double Im) BilinearPairing(DiscreteFermionState primal)
    {
        if (primal.CellCount != CellCount || primal.DofsPerCell != DofsPerCell)
            throw new ArgumentException("State dimensions do not match for bilinear pairing.");

        double sumRe = 0, sumIm = 0;
        for (int i = 0; i < primal.Coefficients.Length / 2; i++)
        {
            // psi_bar[i] = (a + ib), primal[i] = (c + id)
            // psi_bar * primal = (a + ib)(c + id) = (ac - bd) + i(ad + bc)
            // For Hermitian dual psi_bar = psi^†: a = Re(psi), b = -Im(psi)
            // so (a + ib)(c + id) = (ac + bd) + i(ad + bc - 2bd) -- but simpler:
            // psi_bar * psi = (Re - i*Im)(Re + i*Im) = Re^2 + Im^2 (real, non-negative)
            double a = Coefficients[2 * i];
            double b = Coefficients[2 * i + 1];
            double c = primal.Coefficients[2 * i];
            double d = primal.Coefficients[2 * i + 1];
            // Standard complex multiplication: (a + ib)(c + id)
            sumRe += a * c - b * d;
            sumIm += a * d + b * c;
        }
        return (sumRe, sumIm);
    }
}

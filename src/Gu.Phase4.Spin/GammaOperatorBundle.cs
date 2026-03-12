using System.Numerics;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Spin;

/// <summary>
/// Precomputed gamma operator bundle for a given Clifford algebra convention.
/// Stores all gamma matrices (and optionally the chirality matrix) as
/// complex dense matrices. These are the same for every mesh cell —
/// they depend only on the Clifford signature, not the geometry.
///
/// GammaMatrices[mu] is the SpinorDimension x SpinorDimension complex matrix
/// for the mu-th gamma (mu = 0..n-1).
/// </summary>
public sealed class GammaOperatorBundle
{
    /// <summary>Convention ID (e.g. "dirac-tensor-product-v1").</summary>
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    /// <summary>Clifford signature used.</summary>
    [JsonPropertyName("signature")]
    public required CliffordSignature Signature { get; init; }

    /// <summary>Spinor dimension = 2^floor(dim/2).</summary>
    [JsonPropertyName("spinorDimension")]
    public required int SpinorDimension { get; init; }

    /// <summary>
    /// Gamma matrices: GammaMatrices[mu][row, col] is a complex entry.
    /// Array length = Signature.Dimension (one matrix per spacetime direction).
    /// Each matrix is SpinorDimension x SpinorDimension.
    /// Not serialized to JSON (large dense matrices); reconstruct from convention.
    /// </summary>
    [JsonIgnore]
    public required Complex[][,] GammaMatrices { get; init; }

    /// <summary>
    /// Chirality matrix Gamma_chi = i^(n/2) * Gamma_1 * ... * Gamma_n.
    /// Null if dimension is odd (chirality not well-defined).
    /// Not serialized to JSON; reconstruct from convention.
    /// </summary>
    [JsonIgnore]
    public Complex[,]? ChiralityMatrix { get; init; }

    /// <summary>
    /// True if the chirality matrix is available (even-dimensional Y).
    /// Serialized so consumers can check without the full matrix.
    /// </summary>
    [JsonPropertyName("hasChiralityMatrix")]
    public bool HasChiralityMatrix => ChiralityMatrix != null;

    /// <summary>Validation result (null if not yet validated).</summary>
    [JsonPropertyName("validationResult")]
    public CliffordValidationResult? ValidationResult { get; init; }

    /// <summary>Provenance of this bundle.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

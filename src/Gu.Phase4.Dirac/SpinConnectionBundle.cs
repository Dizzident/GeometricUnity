using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Precomputed spin connection data derived from a bosonic background on Y_h.
///
/// Contains two distinct parts (see ARCH_P4.md §11.4):
/// 1. LeviCivitaCoefficients: from Y_h metric geometry (discrete LC connection on spin bundle).
///    Currently set to zero under inserted assumption P4-IA-003 (flat spin connection).
/// 2. GaugeCouplingCoefficients: from bosonic omega acting on spinors via rho(T_a).
///    Shape: [edgeIndex * dimG * spinorDim * spinorDim * 2] -- complex, per-edge.
///
/// This is the input object for the Dirac operator assembler (M36).
/// </summary>
public sealed class SpinConnectionBundle
{
    /// <summary>Unique connection identifier.</summary>
    [JsonPropertyName("connectionId")]
    public required string ConnectionId { get; init; }

    /// <summary>Phase III bosonic background ID this connection was derived from.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Branch variant ID.</summary>
    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    /// <summary>SpinorRepresentationSpec ID this connection is compatible with.</summary>
    [JsonPropertyName("spinorSpecId")]
    public required string SpinorSpecId { get; init; }

    /// <summary>
    /// Levi-Civita spin connection coefficients from Y_h geometry.
    /// Under assumption P4-IA-003 this array is all zeros.
    /// Shape: [edgeCount * dim * spinorDim * spinorDim * 2] (real/imag interleaved complex).
    /// </summary>
    [JsonPropertyName("leviCivitaCoefficients")]
    public required double[] LeviCivitaCoefficients { get; init; }

    /// <summary>
    /// Gauge coupling coefficients: omega_mu^a * rho(T_a) per edge per direction.
    /// These enter D_h as Gamma^mu * omega_mu^a * rho(T_a).
    /// Shape: [edgeCount * spinorDim * spinorDim * 2] (complex, real/imag interleaved).
    /// The gauge algebra sum (over a) has already been performed.
    /// </summary>
    [JsonPropertyName("gaugeCouplingCoefficients")]
    public required double[] GaugeCouplingCoefficients { get; init; }

    /// <summary>Gauge representation used: "adjoint", "fundamental", or "trivial".</summary>
    [JsonPropertyName("gaugeRepresentationId")]
    public required string GaugeRepresentationId { get; init; }

    /// <summary>Assembly method identifier (e.g., "cpu-flat-v1").</summary>
    [JsonPropertyName("assemblyMethod")]
    public required string AssemblyMethod { get; init; }

    /// <summary>
    /// True if the Levi-Civita part is identically zero (P4-IA-003).
    /// Must be recorded so downstream code can warn appropriately.
    /// </summary>
    [JsonPropertyName("flatLeviCivitaAssumption")]
    public bool FlatLeviCivitaAssumption { get; init; }

    /// <summary>Spacetime dimension of Y (= spinorSpec.SpacetimeDimension).</summary>
    [JsonPropertyName("spacetimeDimension")]
    public required int SpacetimeDimension { get; init; }

    /// <summary>Spinor dimension 2^floor(dimY/2).</summary>
    [JsonPropertyName("spinorDimension")]
    public required int SpinorDimension { get; init; }

    /// <summary>Number of mesh edges in Y_h.</summary>
    [JsonPropertyName("edgeCount")]
    public required int EdgeCount { get; init; }

    /// <summary>Gauge algebra dimension (e.g., 3 for su(2), 8 for su(3)).</summary>
    [JsonPropertyName("gaugeDimension")]
    public required int GaugeDimension { get; init; }

    /// <summary>Provenance of this bundle.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

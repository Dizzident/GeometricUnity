using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Fermions;

/// <summary>
/// Records a fermionic background state around a selected bosonic background.
///
/// In Phase IV the fermionic "background" is typically zero (psi_* = 0) for
/// the leading-order computation: the fermion modes are extracted from the
/// linearization of D_h(z_*) around the bosonic background z_*.
/// Non-zero fermionic backgrounds can be declared here for branch variants
/// that include condensate-like configurations.
///
/// PhysicsNote: This record encodes z_* in the fermion sector. The bosonic
/// part of z_* lives in Phase III background artifacts; this record only
/// describes the fermionic component.
/// </summary>
public sealed class FermionBackgroundRecord
{
    /// <summary>Unique identifier for this fermionic background.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Phase III bosonic background ID this is built on top of.</summary>
    [JsonPropertyName("bosonicBackgroundId")]
    public required string BosonicBackgroundId { get; init; }

    /// <summary>Branch variant ID.</summary>
    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    /// <summary>Fermionic field layout ID.</summary>
    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    /// <summary>
    /// Whether the fermionic background is identically zero.
    /// True for the standard Phase IV computation where only bosonic z_* is nonzero.
    /// </summary>
    [JsonPropertyName("isZeroBackground")]
    public bool IsZeroBackground { get; init; } = true;

    /// <summary>
    /// Optional primal background state. Null if IsZeroBackground == true.
    /// </summary>
    [JsonPropertyName("primalState")]
    public DiscreteFermionState? PrimalState { get; init; }

    /// <summary>
    /// Whether this background was selected from Phase III (replay-tier check passed).
    /// </summary>
    [JsonPropertyName("selectedFromPhase3")]
    public bool SelectedFromPhase3 { get; init; }

    /// <summary>Minimum replay tier of the underlying bosonic background (R0-R3).</summary>
    [JsonPropertyName("bosonicReplayTier")]
    public string BosonicReplayTier { get; init; } = "R0";

    /// <summary>Notes on why this background was selected or rejected.</summary>
    [JsonPropertyName("selectionNotes")]
    public string? SelectionNotes { get; init; }

    /// <summary>Provenance of this fermionic background record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

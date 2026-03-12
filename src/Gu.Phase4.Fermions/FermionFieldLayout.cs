using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Fermions;

/// <summary>
/// Describes one spinor block within a fermion field layout.
/// Each block carries its own local spinor dimension and gauge-index dimension.
/// </summary>
public sealed class SpinorBlockSpec
{
    /// <summary>Unique identifier for this block within the layout.</summary>
    [JsonPropertyName("blockId")]
    public required string BlockId { get; init; }

    /// <summary>
    /// Role of this block: "primal" (psi), "dual" (psi_bar), "auxiliary" (eta_aux),
    /// or "chiral-left" / "chiral-right" for pre-projected Weyl blocks.
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    /// <summary>Clifford spinor dimension for this block (= SpinorComponents from the spec).</summary>
    [JsonPropertyName("spinorDimension")]
    public required int SpinorDimension { get; init; }

    /// <summary>
    /// Internal gauge-representation dimension for this block.
    /// 1 means no gauge index; >1 means the fermion transforms in a gauge representation.
    /// </summary>
    [JsonPropertyName("gaugeDimension")]
    public required int GaugeDimension { get; init; }

    /// <summary>Total block size = spinorDimension * gaugeDimension.</summary>
    [JsonIgnore]
    public int TotalDimension => SpinorDimension * GaugeDimension;

    /// <summary>Whether this block uses complex arithmetic (always true in Phase IV).</summary>
    [JsonPropertyName("isComplex")]
    public bool IsComplex { get; init; } = true;

    /// <summary>Optional human-readable description of what this block represents.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

/// <summary>
/// Describes a conjugation / duality rule that maps one spinor block to another.
/// Serialized so branch variants can use different conjugation conventions.
/// </summary>
public sealed class ConjugationRule
{
    /// <summary>ID of the source block (e.g. "psi").</summary>
    [JsonPropertyName("sourceBlockId")]
    public required string SourceBlockId { get; init; }

    /// <summary>ID of the dual block (e.g. "psi-bar").</summary>
    [JsonPropertyName("dualBlockId")]
    public required string DualBlockId { get; init; }

    /// <summary>
    /// Convention: "hermitian", "dirac-adjoint", or "majorana".
    /// Must match the ConjugationConventionSpec.ConjugationType.
    /// </summary>
    [JsonPropertyName("convention")]
    public required string Convention { get; init; }
}

/// <summary>
/// Describes a bilinear form allowed between a dual block and a primal block.
/// E.g. psi_bar * Gamma_mu * psi or psi_bar * psi.
/// </summary>
public sealed class AllowedBilinear
{
    /// <summary>ID of the dual (left) block.</summary>
    [JsonPropertyName("leftBlockId")]
    public required string LeftBlockId { get; init; }

    /// <summary>ID of the primal (right) block.</summary>
    [JsonPropertyName("rightBlockId")]
    public required string RightBlockId { get; init; }

    /// <summary>
    /// Type of the bilinear: "scalar", "vector", "tensor", "pseudoscalar",
    /// "axial-vector", or "general".
    /// </summary>
    [JsonPropertyName("bilinearType")]
    public required string BilinearType { get; init; }

    /// <summary>Whether this bilinear is admissible for observation/comparison.</summary>
    [JsonPropertyName("observationEligible")]
    public bool ObservationEligible { get; init; } = true;
}

/// <summary>
/// Declares which blocks are eligible for the observation chain (sigma_h pullback to X_h).
/// Blocks not listed here may not be directly compared to physical observables.
/// </summary>
public sealed class ObservationEligibilitySpec
{
    /// <summary>Block IDs eligible for direct observation pullback.</summary>
    [JsonPropertyName("eligibleBlockIds")]
    public required List<string> EligibleBlockIds { get; init; }

    /// <summary>Reason for eligibility or ineligibility (provenance note).</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}

/// <summary>
/// Complete specification for the fermionic field layout on Y_h.
///
/// The layout is manifest-driven: not every branch includes every block.
/// PhysicsNote: psi lives in sections of S(Y) ⊗ V_rho where V_rho is the
/// gauge representation bundle. The gauge index (GaugeDimension) is tracked
/// per-block; the Clifford index (SpinorDimension) comes from the SpinorSpec.
/// Both must be explicit — neither can be silently hardcoded.
/// </summary>
public sealed class FermionFieldLayout
{
    /// <summary>Unique identifier for this layout.</summary>
    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Spinor representation spec ID this layout is based on.</summary>
    [JsonPropertyName("spinorSpecId")]
    public required string SpinorSpecId { get; init; }

    /// <summary>All spinor blocks in this layout.</summary>
    [JsonPropertyName("spinorBlocks")]
    public required List<SpinorBlockSpec> SpinorBlocks { get; init; }

    /// <summary>Conjugation rules mapping primal blocks to dual blocks.</summary>
    [JsonPropertyName("conjugationRules")]
    public required List<ConjugationRule> ConjugationRules { get; init; }

    /// <summary>Admissible bilinear forms.</summary>
    [JsonPropertyName("allowedBilinears")]
    public required List<AllowedBilinear> AllowedBilinears { get; init; }

    /// <summary>Observation eligibility spec.</summary>
    [JsonPropertyName("observationEligibility")]
    public required ObservationEligibilitySpec ObservationEligibility { get; init; }

    /// <summary>
    /// Total number of fermionic degrees of freedom per Y-space cell.
    /// = sum of TotalDimension across all primal SpinorBlocks (role == "primal").
    /// </summary>
    [JsonIgnore]
    public int PrimalDofsPerCell =>
        SpinorBlocks.Where(b => b.Role == "primal").Sum(b => b.TotalDimension);

    /// <summary>Total fermionic DOFs on Y_h = PrimalDofsPerCell * cellCount.</summary>
    public int TotalPrimalDofs(int cellCount) => PrimalDofsPerCell * cellCount;

    /// <summary>IDs of inserted assumptions that narrow the physics to this layout.</summary>
    [JsonPropertyName("insertedAssumptionIds")]
    public List<string> InsertedAssumptionIds { get; init; } = new();

    /// <summary>Provenance of this layout.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

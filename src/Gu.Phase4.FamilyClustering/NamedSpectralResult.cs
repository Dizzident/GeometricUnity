using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// A labeled collection of FermionModeRecords from one spectral context
/// (e.g., a branch variant or refinement level).
///
/// ContextId distinguishes this result from others in a tracking run.
/// BackgroundId links to the bosonic background this spectrum was computed over.
/// </summary>
public sealed class NamedSpectralResult
{
    /// <summary>
    /// Unique context identifier (e.g., branch variant ID, refinement level tag).
    /// </summary>
    public required string ContextId { get; init; }

    /// <summary>Background ID from the bosonic background record.</summary>
    public required string BackgroundId { get; init; }

    /// <summary>The modes from this spectral context, sorted by ascending |lambda|.</summary>
    public required List<FermionModeRecord> Modes { get; init; }
}

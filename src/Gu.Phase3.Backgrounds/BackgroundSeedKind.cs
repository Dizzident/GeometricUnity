namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Kind of seed used to initialize a background solve (Section 7.1).
/// </summary>
public enum BackgroundSeedKind
{
    /// <summary>Zero or near-zero initial state.</summary>
    Trivial,

    /// <summary>Symmetric ansatz initialization.</summary>
    SymmetricAnsatz,

    /// <summary>Continuation from a previously solved background.</summary>
    Continuation,

    /// <summary>Transfer from a coarser-grid solve.</summary>
    CoarseGridTransfer,

    /// <summary>User-supplied explicit state.</summary>
    Explicit,
}

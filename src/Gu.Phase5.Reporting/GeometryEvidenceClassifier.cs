namespace Gu.Phase5.Reporting;

/// <summary>
/// Classifies the geometry evidence tier for a Phase V campaign report (P11-M7).
///
/// Per D-P11-007: the current executable evidence path uses toy/structured low-dimensional
/// geometry and must not be summarized as direct evidence for draft-level X^4/Observerse
/// recovery.
///
/// The draft (April 1, 2021) starts from a topological X^4 and treats observation as
/// recovery from the Observerse via pullback. The current runtime uses:
///   - base mesh X_h: 2D toy square triangulation (dim=2, not dim=4)
///   - ambient mesh Y_h: 5D toy fiber construction (not full Observerse)
///
/// Until the executable branch operates on a 4-dimensional base with proper Observerse
/// geometry, all geometry evidence is classified as "toy-control".
/// </summary>
public static class GeometryEvidenceClassifier
{
    /// <summary>
    /// The label for evidence produced from toy/structured low-dimensional geometry.
    /// This is the only label valid for the current Phase XI repository context.
    /// </summary>
    public const string ToyControl = "toy-control";

    /// <summary>
    /// The label for evidence produced from geometry consistent with the draft X^4/Observerse.
    /// Not achievable in the current repository context.
    /// </summary>
    public const string DraftAligned = "draft-aligned";

    /// <summary>
    /// The standard block text emitted when geometry evidence label is "toy-control".
    /// Per D-P11-007: must state mechanically that current evidence is toy-control only.
    /// </summary>
    public const string ToyControlBlockStatement =
        "OBSERVERSE-RECOVERY-BLOCKED: Current geometry evidence is toy-control only. " +
        "The base mesh uses dim(X)=2 (not dim(X)=4) and the fiber construction is a 5D toy " +
        "structure (not the full Observerse). The draft's X^4/Observerse recovery program " +
        "requires a 4-dimensional base with proper Observerse geometry. Current campaign " +
        "outputs do not constitute evidence for draft-level X^4 recovery.";

    /// <summary>
    /// Classify the geometry evidence tier from the base and ambient dimensions used
    /// in a campaign.
    /// </summary>
    /// <param name="baseDimension">dim(X) of the base manifold used in the campaign.</param>
    /// <param name="ambientDimension">dim(Y) of the ambient space used in the campaign.</param>
    /// <returns>"toy-control" unless both dimensions match draft requirements.</returns>
    public static string Classify(int baseDimension, int ambientDimension)
    {
        // The draft requires dim(X)=4 and dim(Y)=14 (independent per physicist review).
        // Any other combination is toy-control by definition.
        if (baseDimension == 4 && ambientDimension == 14)
            return DraftAligned;

        return ToyControl;
    }

    /// <summary>
    /// Classify from a BranchManifest-like dimension pair. Returns "toy-control" for any
    /// current repository standard geometry (2D base, 5D fiber = 7D ambient).
    /// </summary>
    public static string ClassifyFromManifest(int baseDimension, int ambientDimension)
        => Classify(baseDimension, ambientDimension);

    /// <summary>
    /// Get the block statement for a given label.
    /// Returns the standard block text when label is "toy-control", null otherwise.
    /// </summary>
    public static string? GetBlockStatement(string label)
        => string.Equals(label, ToyControl, StringComparison.Ordinal)
            ? ToyControlBlockStatement
            : null;
}

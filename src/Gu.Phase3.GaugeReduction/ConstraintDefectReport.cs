using System.Text.Json.Serialization;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Report on gauge constraint defects: rank deficiency in the gauge image,
/// near-zero singular values, and diagnostic information.
///
/// A defect indicates that the gauge action Gamma_* does not span the
/// full expected gauge dimension. This can occur at:
/// - reducible connections (stabilizer group non-trivial),
/// - boundary effects on finite meshes,
/// - numerical artifacts from ill-conditioning.
/// </summary>
public sealed class ConstraintDefectReport
{
    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Expected gauge rank (dimG * (VertexCount - 1) for connected mesh).</summary>
    [JsonPropertyName("expectedRank")]
    public required int ExpectedRank { get; init; }

    /// <summary>Computed gauge rank after SVD truncation.</summary>
    [JsonPropertyName("computedRank")]
    public required int ComputedRank { get; init; }

    /// <summary>Rank defect = ExpectedRank - ComputedRank.</summary>
    [JsonPropertyName("rankDefect")]
    public int RankDefect => ExpectedRank - ComputedRank;

    /// <summary>Whether there is a rank defect.</summary>
    [JsonPropertyName("hasDefect")]
    public bool HasDefect => RankDefect > 0;

    /// <summary>All singular values from the SVD (descending order).</summary>
    [JsonPropertyName("singularValues")]
    public required IReadOnlyList<double> SingularValues { get; init; }

    /// <summary>SVD cutoff tolerance used.</summary>
    [JsonPropertyName("svdCutoff")]
    public required double SvdCutoff { get; init; }

    /// <summary>Ratio of smallest retained singular value to largest.</summary>
    [JsonPropertyName("conditionNumber")]
    public double ConditionNumber { get; init; }

    /// <summary>Human-readable diagnostic notes.</summary>
    [JsonPropertyName("diagnosticNotes")]
    public IReadOnlyList<string> DiagnosticNotes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Build a constraint defect report from a gauge basis.
    /// </summary>
    public static ConstraintDefectReport FromGaugeBasis(GaugeBasis basis)
    {
        if (basis == null) throw new ArgumentNullException(nameof(basis));

        double condNum = 0;
        if (basis.SingularValues.Count >= 2 && basis.SingularValues[0] > 1e-30)
        {
            condNum = basis.SingularValues[basis.SingularValues.Count - 1] / basis.SingularValues[0];
        }
        else if (basis.SingularValues.Count == 1)
        {
            condNum = 1.0;
        }

        var notes = new List<string>();
        if (basis.RankDefect > 0)
        {
            notes.Add($"Rank defect of {basis.RankDefect}: expected {basis.ExpectedRank}, got {basis.Rank}.");
            notes.Add("Possible causes: reducible connection, boundary effects, numerical ill-conditioning.");
        }
        if (condNum < 1e-8 && basis.Rank > 0)
        {
            notes.Add($"Near-singular gauge basis: condition ratio = {condNum:E3}.");
        }

        return new ConstraintDefectReport
        {
            BackgroundId = basis.BackgroundId,
            ExpectedRank = basis.ExpectedRank,
            ComputedRank = basis.Rank,
            SingularValues = basis.SingularValues.ToArray(),
            SvdCutoff = basis.SvdCutoff,
            ConditionNumber = condNum,
            DiagnosticNotes = notes,
        };
    }
}

using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Dirac;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Bundles a solved FermionSpectralResult together with its chirality decompositions
/// and conjugation pair records.
///
/// This is the primary M38 output artifact. It is produced by FermionSpectralBundleBuilder
/// after calling FermionSpectralSolver and running the chirality/conjugation pipeline.
///
/// Architecture note: FermionSpectralBundle lives in Gu.Phase4.Chirality (not Gu.Phase4.Dirac)
/// because Gu.Phase4.Chirality already depends on Gu.Phase4.Dirac and is the natural consumer.
/// </summary>
public sealed class FermionSpectralBundle
{
    /// <summary>Unique bundle identifier.</summary>
    [JsonPropertyName("bundleId")]
    public required string BundleId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Fermionic background ID this bundle was computed for.</summary>
    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    /// <summary>The underlying spectral solve result.</summary>
    [JsonIgnore]
    public required FermionSpectralResult SpectralResult { get; init; }

    /// <summary>
    /// Chirality decomposition for each mode, in the same order as SpectralResult.Modes.
    /// Length equals SpectralResult.Modes.Count.
    /// </summary>
    [JsonPropertyName("chiralityDecompositions")]
    public required List<ChiralityDecomposition> ChiralityDecompositions { get; init; }

    /// <summary>
    /// Conjugation pairs found among the modes.
    /// Each mode appears in at most one pair.
    /// </summary>
    [JsonPropertyName("conjugationPairs")]
    public required List<ConjugationPairRecord> ConjugationPairs { get; init; }

    /// <summary>
    /// Gauge leak summary across all modes.
    /// </summary>
    [JsonPropertyName("gaugeLeakSummary")]
    public required GaugeLeakSummary GaugeLeakSummary { get; init; }

    /// <summary>
    /// Whether chirality analysis was available (false for odd-dimensional Y).
    /// </summary>
    [JsonPropertyName("chiralityAnalysisAvailable")]
    public required bool ChiralityAnalysisAvailable { get; init; }

    /// <summary>
    /// Whether conjugation analysis was applied.
    /// </summary>
    [JsonPropertyName("conjugationAnalysisApplied")]
    public required bool ConjugationAnalysisApplied { get; init; }

    /// <summary>Number of modes.</summary>
    [JsonPropertyName("modeCount")]
    public int ModeCount => SpectralResult.Modes.Count;

    /// <summary>Provenance of this bundle.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

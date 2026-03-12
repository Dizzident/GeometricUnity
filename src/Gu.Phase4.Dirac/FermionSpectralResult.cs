using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Observed-space summary for a fermionic spectral result.
/// Captures how many modes are left/right/mixed chiral and
/// how many conjugation pairs were resolved.
/// </summary>
public sealed class FermionObservationSummary
{
    /// <summary>Background ID.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Total modes in the spectrum.</summary>
    [JsonPropertyName("totalModes")]
    public required int TotalModes { get; init; }

    /// <summary>Number of modes tagged "definite-left".</summary>
    [JsonPropertyName("leftChiralCount")]
    public required int LeftChiralCount { get; init; }

    /// <summary>Number of modes tagged "definite-right".</summary>
    [JsonPropertyName("rightChiralCount")]
    public required int RightChiralCount { get; init; }

    /// <summary>Number of modes tagged "mixed" or "trivial".</summary>
    [JsonPropertyName("mixedOrTrivialCount")]
    public required int MixedOrTrivialCount { get; init; }

    /// <summary>Number of resolved conjugation pairs.</summary>
    [JsonPropertyName("conjugationPairCount")]
    public required int ConjugationPairCount { get; init; }

    /// <summary>
    /// Net chirality index = (leftChiralCount - rightChiralCount).
    /// Nonzero indicates a chiral asymmetry in the spectrum.
    /// </summary>
    [JsonPropertyName("netChiralityIndex")]
    public int NetChiralityIndex => LeftChiralCount - RightChiralCount;

    /// <summary>Notes (e.g. trivial dimension disclaimer).</summary>
    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();
}

/// <summary>
/// Diagnostics for a fermionic spectral solve.
/// </summary>
public sealed class FermionSpectralDiagnostics
{
    [JsonPropertyName("solverName")]
    public required string SolverName { get; init; }

    [JsonPropertyName("iterations")]
    public required int Iterations { get; init; }

    [JsonPropertyName("converged")]
    public required bool Converged { get; init; }

    [JsonPropertyName("maxResidual")]
    public required double MaxResidual { get; init; }

    [JsonPropertyName("meanResidual")]
    public required double MeanResidual { get; init; }

    [JsonPropertyName("gaugeReductionApplied")]
    public bool GaugeReductionApplied { get; init; }

    [JsonPropertyName("nullspaceDeflationApplied")]
    public bool NullspaceDeflationApplied { get; init; }

    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();
}

/// <summary>
/// Result of the fermionic spectral solve:
///   D_h(z_*) phi_i = lambda_i M_psi phi_i
///
/// FermionModeRecord entries are sorted by ascending |lambda_i|.
/// </summary>
public sealed class FermionSpectralResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("operatorId")]
    public required string OperatorId { get; init; }

    [JsonPropertyName("modes")]
    public required List<FermionModeRecord> Modes { get; init; }

    [JsonPropertyName("diagnostics")]
    public required FermionSpectralDiagnostics Diagnostics { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Observation summary (chirality counts, pairing stats).</summary>
    [JsonPropertyName("observationSummary")]
    public FermionObservationSummary? ObservationSummary { get; init; }
}

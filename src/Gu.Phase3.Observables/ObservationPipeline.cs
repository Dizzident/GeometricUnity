using Gu.Phase3.Spectra;

namespace Gu.Phase3.Observables;

/// <summary>
/// Orchestrator: background + modes -> observed signatures + overlap matrix.
///
/// Combines ObservedModeMapper and ObservedOverlapComputer into a single
/// pipeline that transforms spectral analysis results into observed-space data.
/// </summary>
public sealed class ObservationPipeline
{
    private readonly ObservedModeMapper _mapper;

    public ObservationPipeline(LinearizedObservationOperator observationOperator)
    {
        if (observationOperator == null) throw new ArgumentNullException(nameof(observationOperator));
        _mapper = new ObservedModeMapper(observationOperator);
    }

    /// <summary>
    /// Run the full observation pipeline on a set of modes.
    /// </summary>
    /// <param name="modes">Spectral modes to observe.</param>
    /// <returns>Observed signatures and overlap analysis.</returns>
    public ObservationPipelineResult Run(IReadOnlyList<ModeRecord> modes)
    {
        if (modes == null) throw new ArgumentNullException(nameof(modes));

        // Step 1: Map modes through observation operator
        var signatures = _mapper.MapAll(modes);

        // Step 2: Compute overlap matrix
        var overlap = ObservedOverlapComputer.Compute(signatures);

        return new ObservationPipelineResult
        {
            Signatures = signatures,
            Overlap = overlap,
            ModeCount = modes.Count,
        };
    }
}

/// <summary>
/// Result of the full observation pipeline.
/// </summary>
public sealed class ObservationPipelineResult
{
    /// <summary>Observed signatures for each mode.</summary>
    public required IReadOnlyList<ObservedModeSignature> Signatures { get; init; }

    /// <summary>Overlap analysis between observed signatures.</summary>
    public required OverlapResult Overlap { get; init; }

    /// <summary>Number of modes processed.</summary>
    public required int ModeCount { get; init; }
}

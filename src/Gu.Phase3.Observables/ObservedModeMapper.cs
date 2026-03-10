using Gu.Phase3.Spectra;

namespace Gu.Phase3.Observables;

/// <summary>
/// Maps a list of ModeRecords through the linearized observation operator
/// to produce ObservedModeSignatures.
///
/// This is the primary entry point for converting spectral analysis results
/// (modes in Y_h) into observed-space signatures (fields on X_h).
/// </summary>
public sealed class ObservedModeMapper
{
    private readonly LinearizedObservationOperator _operator;

    public ObservedModeMapper(LinearizedObservationOperator observationOperator)
    {
        _operator = observationOperator ?? throw new ArgumentNullException(nameof(observationOperator));
    }

    /// <summary>
    /// Map a single ModeRecord through the observation operator.
    /// </summary>
    public ObservedModeSignature Map(ModeRecord mode)
    {
        if (mode == null) throw new ArgumentNullException(nameof(mode));
        return _operator.Apply(mode.ModeVector, mode.ModeId);
    }

    /// <summary>
    /// Map a batch of ModeRecords through the observation operator.
    /// </summary>
    public IReadOnlyList<ObservedModeSignature> MapAll(IReadOnlyList<ModeRecord> modes)
    {
        if (modes == null) throw new ArgumentNullException(nameof(modes));

        var results = new ObservedModeSignature[modes.Count];
        for (int i = 0; i < modes.Count; i++)
            results[i] = Map(modes[i]);
        return results;
    }
}

using Gu.Core;
using Gu.Phase4.Observation;

namespace Gu.Phase4.Comparison;

/// <summary>
/// Comparison adapter for fermionic candidates: compares FermionObservationSummary
/// records against FermionCandidateReference entries.
///
/// Algorithm (M43):
/// 1. Match each observation summary to a reference by chirality.
/// 2. Check mass-like scale ratio against tolerance.
/// 3. Assign outcome: compatible / incompatible / underdetermined / not-applicable.
///
/// Conservative output: when in doubt, outcome = "underdetermined".
/// </summary>
public sealed class FermionComparisonAdapter
{
    private readonly double _massLikeScaleTolerance;

    public FermionComparisonAdapter(double massLikeScaleTolerance = 0.5)
    {
        if (massLikeScaleTolerance <= 0 || massLikeScaleTolerance > 10)
            throw new ArgumentOutOfRangeException(nameof(massLikeScaleTolerance));
        _massLikeScaleTolerance = massLikeScaleTolerance;
    }

    /// <summary>
    /// Compare one observation summary against one reference candidate.
    /// </summary>
    public FermionComparisonRecord Compare(
        FermionObservationSummary observation,
        FermionCandidateReference reference,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(observation);
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(provenance);

        string comparisonId = $"cmp-{observation.ClusterId}-{reference.ReferenceId}";
        var notes = new List<string>();

        // Trivial observation: not-applicable
        if (observation.IsTrivial)
        {
            return new FermionComparisonRecord
            {
                ComparisonId = comparisonId,
                ClusterId = observation.ClusterId,
                ReferenceCandidateId = reference.ReferenceId,
                Outcome = "not-applicable",
                ChiralityMatch = "not-applicable",
                MassLikeScaleTolerance = _massLikeScaleTolerance,
                Notes = new List<string> { "Trivial observation — not compared." },
                Provenance = provenance,
            };
        }

        // Chirality match check
        bool chiralityMatches = observation.ObservedChirality == reference.ExpectedChirality;
        string chiralityMatch = chiralityMatches ? "match" : "mismatch";
        if (!chiralityMatches)
            notes.Add($"Chirality mismatch: observed={observation.ObservedChirality}, expected={reference.ExpectedChirality}.");

        // Mass-like scale check
        double? massLikeScaleRatio = null;
        bool massLikeScaleWithinTolerance = true;

        if (reference.ExpectedMassLikeEnvelope is not null && observation.MassLikeEnvelope.Length >= 2)
        {
            double observedMean = observation.MassLikeEnvelope[1];
            double referenceMean = reference.ExpectedMassLikeEnvelope[1];

            if (referenceMean > 1e-30)
            {
                massLikeScaleRatio = observedMean / referenceMean;
                double ratio = massLikeScaleRatio.Value;
                massLikeScaleWithinTolerance = ratio >= (1.0 - _massLikeScaleTolerance)
                    && ratio <= (1.0 + _massLikeScaleTolerance);

                if (!massLikeScaleWithinTolerance)
                    notes.Add($"Mass-like scale ratio {ratio:F3} outside tolerance [{1 - _massLikeScaleTolerance:F2}, {1 + _massLikeScaleTolerance:F2}].");
            }
        }

        // Conjugation pair check
        if (reference.ExpectsConjugatePair && !observation.HasConjugatePair)
            notes.Add("Reference expects conjugate pair, but none found in observation.");

        // Determine overall outcome
        string outcome;
        if (!chiralityMatches || !massLikeScaleWithinTolerance)
            outcome = "incompatible";
        else if (reference.ExpectedMassLikeEnvelope is null)
            outcome = "underdetermined";
        else
            outcome = "compatible";

        return new FermionComparisonRecord
        {
            ComparisonId = comparisonId,
            ClusterId = observation.ClusterId,
            ReferenceCandidateId = reference.ReferenceId,
            Outcome = outcome,
            ChiralityMatch = chiralityMatch,
            MassLikeScaleRatio = massLikeScaleRatio,
            MassLikeScaleWithinTolerance = massLikeScaleWithinTolerance,
            MassLikeScaleTolerance = _massLikeScaleTolerance,
            Notes = notes,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Compare all observations against all references (cross-product).
    /// Each observation is compared against every reference.
    /// </summary>
    public List<FermionComparisonRecord> CompareAll(
        IReadOnlyList<FermionObservationSummary> observations,
        IReadOnlyList<FermionCandidateReference> references,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(references);
        ArgumentNullException.ThrowIfNull(provenance);

        var results = new List<FermionComparisonRecord>();
        foreach (var obs in observations)
            foreach (var reference in references)
                results.Add(Compare(obs, reference, provenance));
        return results;
    }
}

using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Builds FermionModeFamily records from a set of spectral results across
/// multiple branch variants or refinement levels.
///
/// Algorithm:
/// 1. Start with modes from the first (reference) spectral result as seeds.
/// 2. For each subsequent result, run FermionModeTracker to match modes.
/// 3. Accumulate matched modes into families; unmatched modes start new families.
/// 4. Compute per-family statistics: persistence scores, chirality envelope,
///    conjugation pairing, gauge-leak average.
/// </summary>
public sealed class FermionFamilyBuilder
{
    private readonly FermionTrackingConfig _config;

    public FermionFamilyBuilder(FermionTrackingConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Build families from a sequence of spectral results.
    /// Each result should correspond to a distinct branch variant or refinement level.
    /// The first result is used as the reference seed.
    /// </summary>
    public IReadOnlyList<FermionModeFamily> Build(
        IReadOnlyList<NamedSpectralResult> results,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(provenance);

        if (results.Count == 0)
            return Array.Empty<FermionModeFamily>();

        var tracker = new FermionModeTracker(_config);

        // Seed families from the first result
        // Each seed mode starts its own family
        var families = new List<FamilyAccumulator>();
        var first = results[0];
        foreach (var mode in first.Modes)
        {
            families.Add(new FamilyAccumulator(mode, first.ContextId, first.BackgroundId));
        }

        // Track across subsequent results
        for (int r = 1; r < results.Count; r++)
        {
            var current = results[r];
            var currentModes = current.Modes;

            // Collect representative modes (last added per family)
            var representatives = families
                .Select(f => f.LastMode)
                .ToList();

            var matches = tracker.Match(representatives, currentModes);
            var usedCurrentIndices = new HashSet<int>();

            for (int fi = 0; fi < families.Count; fi++)
            {
                var match = matches[fi];
                if (match.IsMatch && !string.IsNullOrEmpty(match.TargetModeId))
                {
                    var matchedMode = currentModes.FirstOrDefault(m => m.ModeId == match.TargetModeId);
                    if (matchedMode is not null)
                    {
                        int idx = currentModes.ToList().IndexOf(matchedMode);
                        if (!usedCurrentIndices.Contains(idx))
                        {
                            usedCurrentIndices.Add(idx);
                            families[fi].AddMatch(matchedMode, current.ContextId, current.BackgroundId, match);
                        }
                    }
                }
            }

            // Modes in currentModes with no match start new families
            for (int j = 0; j < currentModes.Count; j++)
            {
                if (!usedCurrentIndices.Contains(j))
                    families.Add(new FamilyAccumulator(currentModes[j], current.ContextId, current.BackgroundId));
            }
        }

        // Convert accumulators to FermionModeFamily records
        int totalContexts = results.Count;
        var output = new List<FermionModeFamily>(families.Count);
        int idx2 = 0;
        foreach (var acc in families)
        {
            output.Add(acc.Build($"ferm-family-{idx2:D4}", totalContexts, provenance));
            idx2++;
        }

        return output;
    }

    // ------------------------------------------------------------------
    // Private helper
    // ------------------------------------------------------------------

    private sealed class FamilyAccumulator
    {
        private readonly List<FermionModeRecord> _modes = new();
        private readonly List<string> _contextIds = new();
        private readonly List<string> _backgroundIds = new();
        private readonly List<FermionModeMatchRecord> _matches = new();

        public FamilyAccumulator(FermionModeRecord seed, string contextId, string backgroundId)
        {
            _modes.Add(seed);
            _contextIds.Add(contextId);
            _backgroundIds.Add(backgroundId);
        }

        public FermionModeRecord LastMode => _modes[_modes.Count - 1];

        public void AddMatch(
            FermionModeRecord mode,
            string contextId,
            string backgroundId,
            FermionModeMatchRecord match)
        {
            _modes.Add(mode);
            _contextIds.Add(contextId);
            _backgroundIds.Add(backgroundId);
            _matches.Add(match);
        }

        public FermionModeFamily Build(string familyId, int totalContexts, ProvenanceMeta provenance)
        {
            var modeIds = _modes.Select(m => m.ModeId).ToList();
            var bgIds = _backgroundIds.Distinct().ToList();
            var ctxIds = _contextIds.Distinct().ToList();

            // Eigenvalue magnitude envelope [min, mean, max]
            var mags = _modes.Select(m => m.EigenvalueMagnitude).ToArray();
            double minMag = mags.Min();
            double maxMag = mags.Max();
            double meanMag = mags.Average();

            // Dominant chirality profile
            var netChiralities = _modes.Select(m => m.ChiralityDecomposition.NetChirality).ToArray();
            double meanNet = netChiralities.Average();
            string dominantChirality = meanNet > 0.3 ? "left"
                : meanNet < -0.3 ? "right"
                : System.Math.Abs(meanNet) < 0.1 ? "mixed"
                : "undetermined";

            // Conjugation pairing
            bool hasPair = _modes.Any(m => m.ConjugationPairing.HasPair);

            // Persistence scores
            double branchPersistence = (double)_modes.Count / System.Math.Max(totalContexts, 1);

            // Ambiguity notes
            var ambiguityNotes = new List<string>();
            foreach (var match in _matches)
                ambiguityNotes.AddRange(match.AmbiguityNotes);

            // Average gauge leak
            double avgGaugeLeak = _modes.Average(m => m.GaugeLeakScore);

            return new FermionModeFamily
            {
                FamilyId = familyId,
                MemberModeIds = modeIds,
                BackgroundIds = bgIds,
                BranchVariantIds = ctxIds,
                EigenvalueMagnitudeEnvelope = new[] { minMag, meanMag, maxMag },
                DominantChiralityProfile = dominantChirality,
                HasConjugationPair = hasPair,
                ConjugateFamilyId = null, // resolved in later passes
                BranchPersistenceScore = System.Math.Min(branchPersistence, 1.0),
                RefinementPersistenceScore = 0.0, // set by caller if refinement tracking
                AverageGaugeLeakScore = avgGaugeLeak,
                AmbiguityNotes = ambiguityNotes.Distinct().ToList(),
                Provenance = provenance,
            };
        }
    }
}

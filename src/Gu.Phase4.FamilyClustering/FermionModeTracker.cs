using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Tracks fermion modes across two or more spectral contexts (different branch variants
/// or refinement levels) and produces match records and family records.
///
/// Algorithm (two-context Match):
/// 1. For each source mode, compute a match score against each target mode.
/// 2. Use greedy best-match assignment (ties flagged as ambiguous).
/// 3. Modes with no match above threshold are recorded as births/deaths.
///
/// Algorithm (multi-context TrackAcrossContexts):
/// 1. Use the first context as the seed.
/// 2. For each subsequent context, run Match against the seed modes.
/// 3. Build FermionModeFamilyRecord entries by chaining matches.
///
/// Match score combines:
///   - eigenvalue band similarity (weight = config.EigenvalueWeight)
///   - chirality profile similarity (weight = config.ChiralityWeight)
///   - eigenspace overlap if available (weight = config.EigenspaceWeight)
/// </summary>
public sealed class FermionModeTracker
{
    private readonly FermionTrackingConfig _config;

    public FermionModeTracker(FermionTrackingConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Match modes between source and target spectral results.
    /// Returns one match record per source mode (with IsMatch=false for unmatched modes).
    /// </summary>
    public IReadOnlyList<FermionModeMatchRecord> Match(
        IReadOnlyList<FermionModeRecord> sourceModes,
        IReadOnlyList<FermionModeRecord> targetModes)
    {
        ArgumentNullException.ThrowIfNull(sourceModes);
        ArgumentNullException.ThrowIfNull(targetModes);

        if (sourceModes.Count == 0)
            return Array.Empty<FermionModeMatchRecord>();

        var results = new List<FermionModeMatchRecord>(sourceModes.Count);
        var usedTargets = new HashSet<int>();

        // Compute full score matrix
        var scores = new double[sourceModes.Count, targetModes.Count];
        var eigenSim = new double[sourceModes.Count, targetModes.Count];
        var chiralSim = new double[sourceModes.Count, targetModes.Count];
        var eigenOverlap = new double?[sourceModes.Count, targetModes.Count];

        for (int i = 0; i < sourceModes.Count; i++)
        {
            var src = sourceModes[i];
            for (int j = 0; j < targetModes.Count; j++)
            {
                var tgt = targetModes[j];
                eigenSim[i, j] = ComputeEigenvalueSimilarity(src, tgt);
                chiralSim[i, j] = ComputeChiralitySimilarity(src, tgt);
                eigenOverlap[i, j] = ComputeEigenspaceOverlap(src, tgt);

                double evW = _config.EigenvalueWeight;
                double chW = _config.ChiralityWeight;
                double esW = _config.EigenspaceWeight;

                if (eigenOverlap[i, j].HasValue)
                {
                    scores[i, j] = evW * eigenSim[i, j]
                                 + chW * chiralSim[i, j]
                                 + esW * eigenOverlap[i, j]!.Value;
                }
                else
                {
                    // Redistribute eigenspace weight to eigenvalue and chirality
                    double total = evW + chW;
                    scores[i, j] = (evW / total) * eigenSim[i, j]
                                 + (chW / total) * chiralSim[i, j];
                }
            }
        }

        // Greedy assignment: for each source mode, pick best available target
        for (int i = 0; i < sourceModes.Count; i++)
        {
            var src = sourceModes[i];
            int bestJ = -1;
            double bestScore = -1.0;
            double secondBest = -1.0;

            for (int j = 0; j < targetModes.Count; j++)
            {
                if (usedTargets.Contains(j)) continue;
                if (scores[i, j] > bestScore)
                {
                    secondBest = bestScore;
                    bestScore = scores[i, j];
                    bestJ = j;
                }
                else if (scores[i, j] > secondBest)
                {
                    secondBest = scores[i, j];
                }
            }

            bool isMatch = bestJ >= 0 && bestScore >= _config.MatchThreshold;
            bool isAmbiguous = isMatch && (bestScore - secondBest) < _config.AmbiguityMargin;
            var notes = new List<string>();
            if (isAmbiguous)
                notes.Add($"Ambiguous: best={bestScore:F3}, second={secondBest:F3}, margin={bestScore - secondBest:F3}");

            if (isMatch)
                usedTargets.Add(bestJ);

            results.Add(new FermionModeMatchRecord
            {
                SourceModeId = src.ModeId,
                TargetModeId = isMatch ? targetModes[bestJ].ModeId : string.Empty,
                EigenvalueSimilarity = bestJ >= 0 ? eigenSim[i, bestJ] : 0.0,
                ChiralitySimilarity = bestJ >= 0 ? chiralSim[i, bestJ] : 0.0,
                EigenspaceOverlap = bestJ >= 0 ? eigenOverlap[i, bestJ] : null,
                AggregateScore = System.Math.Max(bestScore, 0.0),
                IsMatch = isMatch,
                IsAmbiguous = isAmbiguous,
                AmbiguityNotes = notes,
            });
        }

        return results;
    }

    private double ComputeEigenvalueSimilarity(FermionModeRecord src, FermionModeRecord tgt)
    {
        double srcMag = src.EigenvalueMagnitude;
        double tgtMag = tgt.EigenvalueMagnitude;
        double scale = System.Math.Max(System.Math.Max(srcMag, tgtMag), 1e-14);
        double relDiff = System.Math.Abs(srcMag - tgtMag) / scale;
        // Decay: similarity = exp(-relDiff / tol)
        return System.Math.Exp(-relDiff / _config.EigenvalueRelTol);
    }

    private static double ComputeChiralitySimilarity(FermionModeRecord src, FermionModeRecord tgt)
    {
        double srcNet = src.ChiralityDecomposition.NetChirality;
        double tgtNet = tgt.ChiralityDecomposition.NetChirality;
        // Both in [-1, 1], so max difference is 2
        double diff = System.Math.Abs(srcNet - tgtNet) / 2.0;
        return 1.0 - diff;
    }

    private static double? ComputeEigenspaceOverlap(FermionModeRecord src, FermionModeRecord tgt)
    {
        var sv = src.EigenvectorCoefficients;
        var tv = tgt.EigenvectorCoefficients;
        if (sv is null || tv is null || sv.Length != tv.Length || sv.Length == 0)
            return null;

        // |<phi_src, phi_tgt>|^2 using real flat interleaved representation
        double re = 0.0, im = 0.0, normSq = 0.0, normTq = 0.0;
        for (int k = 0; k < sv.Length; k += 2)
        {
            double sRe = sv[k], sIm = sv[k + 1];
            double tRe = tv[k], tIm = tv[k + 1];
            re += sRe * tRe + sIm * tIm;
            im += sRe * tIm - sIm * tRe;
            normSq += sRe * sRe + sIm * sIm;
            normTq += tRe * tRe + tIm * tIm;
        }

        double denom = System.Math.Sqrt(normSq * normTq);
        if (denom < 1e-14) return 0.0;
        return System.Math.Min(1.0, (re * re + im * im) / (denom * denom));
    }

    /// <summary>
    /// Track modes across multiple spectral contexts and produce family records.
    ///
    /// Each entry in <paramref name="contexts"/> is a (contextId, modes) pair.
    /// The first context seeds the families; subsequent contexts are matched against it.
    ///
    /// Returns one FermionModeFamilyRecord per seed mode.
    /// Modes not found in a context contribute to reduced persistence scores.
    /// </summary>
    public IReadOnlyList<FermionModeFamilyRecord> TrackAcrossContexts(
        IReadOnlyList<(string ContextId, IReadOnlyList<FermionModeRecord> Modes)> contexts,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(provenance);

        if (contexts.Count == 0)
            return Array.Empty<FermionModeFamilyRecord>();

        var (seedContextId, seedModes) = contexts[0];
        int nSeed = seedModes.Count;

        // Per-seed: track which contextIds and modeIds each seed matched in
        var memberModeIds = new List<string>[nSeed];
        var memberContextIds = new List<string>[nSeed];
        var ambiguityCounts = new int[nSeed];

        for (int i = 0; i < nSeed; i++)
        {
            memberModeIds[i] = new List<string> { seedModes[i].ModeId };
            memberContextIds[i] = new List<string> { seedContextId };
        }

        // Match each subsequent context against the seed
        for (int c = 1; c < contexts.Count; c++)
        {
            var (targetContextId, targetModes) = contexts[c];
            var matches = Match(seedModes, targetModes);

            for (int i = 0; i < nSeed; i++)
            {
                var match = matches[i];
                if (match.IsMatch)
                {
                    memberModeIds[i].Add(match.TargetModeId);
                    memberContextIds[i].Add(targetContextId);
                    if (match.IsAmbiguous) ambiguityCounts[i]++;
                }
            }
        }

        int nContexts = contexts.Count;
        var families = new List<FermionModeFamilyRecord>(nSeed);

        for (int i = 0; i < nSeed; i++)
        {
            var seed = seedModes[i];
            var modeIds = memberModeIds[i];
            var ctxIds = memberContextIds[i];

            // Compute mean |eigenvalue| and spread from seed only
            // (other contexts may not have eigenvectors available)
            double mean = seed.EigenvalueMagnitude;
            double spread = 0.0;

            // Dominant chirality from seed (single-context fallback)
            string dominantChirality = seed.ChiralityDecomposition.LeftFraction > 0.9 ? "left"
                : seed.ChiralityDecomposition.RightFraction > 0.9 ? "right"
                : "mixed";

            double persistenceScore = (double)modeIds.Count / nContexts;
            bool isStable = persistenceScore >= 1.0;

            families.Add(new FermionModeFamilyRecord
            {
                FamilyId = $"family-{seedContextId}-{seed.ModeId}",
                MemberModeIds = modeIds,
                ContextIds = ctxIds,
                MeanEigenvalue = mean,
                EigenvalueSpread = spread,
                DominantChirality = dominantChirality,
                ConjugationPairFamilyId = null,
                BranchPersistenceScore = persistenceScore,
                RefinementPersistenceScore = persistenceScore,
                IsStable = isStable,
                AmbiguityCount = ambiguityCounts[i],
                Provenance = provenance,
            });
        }

        return families;
    }
}

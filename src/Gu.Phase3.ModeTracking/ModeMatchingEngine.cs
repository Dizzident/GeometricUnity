using Gu.Branching;
using Gu.Core;
using Gu.Phase3.Observables;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Matches modes across two spectra from different contexts.
///
/// Algorithm:
/// 1. Compute overlap matrix (native O1 when possible, feature O3 always).
/// 2. Compute aggregate score matrix.
/// 3. Solve assignment problem via Hungarian algorithm.
/// 4. Detect births, deaths, splits, merges, avoided crossings.
/// 5. Flag ambiguous matches (when best and second-best are close).
///
/// MANDATORY RULE: If matching is ambiguous, record ambiguity.
/// Do not force one-to-one matches without evidence.
/// </summary>
public sealed class ModeMatchingEngine
{
    private readonly TrackingConfig _config;

    public ModeMatchingEngine(TrackingConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Match modes between two spectra.
    /// Returns alignment records for each source mode.
    /// </summary>
    public IReadOnlyList<ModeAlignmentRecord> Match(
        SpectrumBundle source,
        SpectrumBundle target,
        ILinearOperator? massOperator = null,
        IReadOnlyList<ObservedModeSignature>? signatures = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        int nSource = source.Modes.Count;
        int nTarget = target.Modes.Count;

        if (nSource == 0 || nTarget == 0)
            return Array.Empty<ModeAlignmentRecord>();

        // Build signature lookup by mode ID (if provided)
        Dictionary<string, ObservedModeSignature>? sigLookup = null;
        if (signatures != null && signatures.Count > 0)
        {
            sigLookup = new Dictionary<string, ObservedModeSignature>();
            foreach (var sig in signatures)
                sigLookup[sig.ModeId] = sig;
        }

        // Build feature vectors
        var sourceFeatures = BuildFeatures(source);
        var targetFeatures = BuildFeatures(target);

        // Compute score matrix
        var scoreMatrix = new double[nSource, nTarget];
        var o2Matrix = new double?[nSource, nTarget];
        for (int i = 0; i < nSource; i++)
        {
            for (int j = 0; j < nTarget; j++)
            {
                // O3: Feature distance
                double featDist = ModeFeatureVector.Distance(sourceFeatures[i], targetFeatures[j]);
                double featScore = System.Math.Exp(-featDist / _config.FeatureDistanceScale);

                // O1: Native overlap (if mass operator is available and dimensions match)
                double? nativeOverlap = null;
                if (massOperator != null &&
                    source.Modes[i].ModeVector.Length == target.Modes[j].ModeVector.Length)
                {
                    nativeOverlap = ComputeNativeOverlap(
                        source.Modes[i].ModeVector,
                        target.Modes[j].ModeVector,
                        massOperator);
                }

                // O2: Observed signature overlap (if signatures provided)
                double? observedOverlap = null;
                if (sigLookup != null &&
                    sigLookup.TryGetValue(source.Modes[i].ModeId, out var sigI) &&
                    sigLookup.TryGetValue(target.Modes[j].ModeId, out var sigJ))
                {
                    observedOverlap = ObservedOverlapMetrics.L2Overlap(sigI, sigJ);
                }
                o2Matrix[i, j] = observedOverlap;

                // Aggregate: use three-weight formula when all metrics available
                double aggregate = ComputeAggregate(nativeOverlap, observedOverlap, featScore);

                scoreMatrix[i, j] = aggregate;
            }
        }

        // Solve assignment (negate scores for cost minimization)
        var costMatrix = new double[nSource, nTarget];
        for (int i = 0; i < nSource; i++)
            for (int j = 0; j < nTarget; j++)
                costMatrix[i, j] = 1.0 - scoreMatrix[i, j];

        var assignment = HungarianAlgorithm.Solve(costMatrix);

        // Build alignment records
        var alignments = new List<ModeAlignmentRecord>();
        var matchedTargets = new HashSet<int>();

        for (int i = 0; i < nSource; i++)
        {
            int j = assignment[i];
            if (j < 0 || j >= nTarget)
            {
                // Death: source mode has no match
                alignments.Add(new ModeAlignmentRecord
                {
                    SourceModeId = source.Modes[i].ModeId,
                    TargetModeId = "",
                    Metrics = new ModeMatchMetricSet
                    {
                        FeatureDistance = double.MaxValue,
                        AggregateScore = 0,
                        IsMatch = false,
                    },
                    AlignmentType = "death",
                    Confidence = 1.0,
                });
                continue;
            }

            matchedTargets.Add(j);
            double score = scoreMatrix[i, j];
            bool isMatch = score >= _config.MatchThreshold;

            // Check ambiguity: is there a second-best match close to the best?
            double secondBest = 0;
            for (int k = 0; k < nTarget; k++)
            {
                if (k == j) continue;
                if (scoreMatrix[i, k] > secondBest)
                    secondBest = scoreMatrix[i, k];
            }

            bool isAmbiguous = score > 0 && secondBest / score > _config.AmbiguityRatio;
            double confidence = isAmbiguous ? 0.5 : (isMatch ? 1.0 : 0.3);

            // Compute native overlap for metrics
            double? nativeOvlp = null;
            if (massOperator != null &&
                source.Modes[i].ModeVector.Length == target.Modes[j].ModeVector.Length)
            {
                nativeOvlp = ComputeNativeOverlap(
                    source.Modes[i].ModeVector,
                    target.Modes[j].ModeVector,
                    massOperator);
            }

            double featDist = ModeFeatureVector.Distance(sourceFeatures[i], targetFeatures[j]);

            string alignType = isMatch ? "matched" : "death";
            string? ambiguityNote = isAmbiguous
                ? $"Ambiguous: best={score:F4}, second-best={secondBest:F4}"
                : null;

            alignments.Add(new ModeAlignmentRecord
            {
                SourceModeId = source.Modes[i].ModeId,
                TargetModeId = target.Modes[j].ModeId,
                Metrics = new ModeMatchMetricSet
                {
                    NativeOverlap = nativeOvlp,
                    ObservedSignatureOverlap = o2Matrix[i, j],
                    FeatureDistance = featDist,
                    AggregateScore = score,
                    IsMatch = isMatch,
                },
                AlignmentType = alignType,
                Confidence = confidence,
                AmbiguityNotes = ambiguityNote,
            });
        }

        // --- Split detection ---
        // After Hungarian matching, detect cases where one source maps to multiple targets.
        // For each unmatched target, check if its best source is already matched elsewhere.
        var sourceToTarget = new Dictionary<int, int>();
        for (int i = 0; i < nSource; i++)
        {
            int j = assignment[i];
            if (j >= 0 && j < nTarget && matchedTargets.Contains(j))
                sourceToTarget[i] = j;
        }

        var splitTargets = new HashSet<int>();
        for (int j = 0; j < nTarget; j++)
        {
            if (matchedTargets.Contains(j)) continue;

            // Find best source for this unmatched target
            int bestSource = -1;
            double bestScore = -1;
            for (int i = 0; i < nSource; i++)
            {
                if (scoreMatrix[i, j] > bestScore)
                {
                    bestScore = scoreMatrix[i, j];
                    bestSource = i;
                }
            }

            if (bestSource < 0 || bestScore < _config.SplitThreshold) continue;
            if (!sourceToTarget.TryGetValue(bestSource, out int matchedTarget)) continue;

            double matchedScore = scoreMatrix[bestSource, matchedTarget];
            if (matchedScore < _config.SplitThreshold) continue;

            // Split detected: source bestSource maps to both matchedTarget and j
            splitTargets.Add(j);

            // Re-label the existing matched alignment as split
            var existingIdx = alignments.FindIndex(a =>
                a.SourceModeId == source.Modes[bestSource].ModeId &&
                a.TargetModeId == target.Modes[matchedTarget].ModeId);
            if (existingIdx >= 0 && alignments[existingIdx].AlignmentType != "split")
            {
                var ea = alignments[existingIdx];
                alignments[existingIdx] = new ModeAlignmentRecord
                {
                    SourceModeId = ea.SourceModeId,
                    TargetModeId = ea.TargetModeId,
                    Metrics = ea.Metrics,
                    AlignmentType = "split",
                    Confidence = ea.Confidence,
                    AmbiguityNotes = $"Split: source {source.Modes[bestSource].ModeId} maps to multiple targets",
                };
            }

            // Add new split alignment for the unmatched target
            double splitFeatDist = ModeFeatureVector.Distance(
                sourceFeatures[bestSource], targetFeatures[j]);
            alignments.Add(new ModeAlignmentRecord
            {
                SourceModeId = source.Modes[bestSource].ModeId,
                TargetModeId = target.Modes[j].ModeId,
                Metrics = new ModeMatchMetricSet
                {
                    FeatureDistance = splitFeatDist,
                    AggregateScore = bestScore,
                    IsMatch = true,
                },
                AlignmentType = "split",
                Confidence = 0.5,
                AmbiguityNotes = $"Split: source {source.Modes[bestSource].ModeId} maps to multiple targets",
            });
        }

        // --- Merge detection ---
        // Detect cases where multiple sources map to one target.
        // For each unmatched source (death), check if its best target is already matched from another source.
        var targetToSource = new Dictionary<int, int>();
        for (int i = 0; i < nSource; i++)
        {
            int j = assignment[i];
            if (j >= 0 && j < nTarget && matchedTargets.Contains(j))
                targetToSource[j] = i;
        }

        for (int i = 0; i < nSource; i++)
        {
            int j = assignment[i];
            if (j >= 0 && j < nTarget && matchedTargets.Contains(j)) continue;

            // Find best target for this unmatched source
            int bestTarget = -1;
            double bestScore = -1;
            for (int jj = 0; jj < nTarget; jj++)
            {
                if (scoreMatrix[i, jj] > bestScore)
                {
                    bestScore = scoreMatrix[i, jj];
                    bestTarget = jj;
                }
            }

            if (bestTarget < 0 || bestScore < _config.SplitThreshold) continue;
            if (!targetToSource.TryGetValue(bestTarget, out int matchedSource)) continue;

            double matchedScore = scoreMatrix[matchedSource, bestTarget];
            if (matchedScore < _config.SplitThreshold) continue;

            // Merge detected: sources matchedSource and i both map to target bestTarget
            var existingIdx = alignments.FindIndex(a =>
                a.SourceModeId == source.Modes[matchedSource].ModeId &&
                a.TargetModeId == target.Modes[bestTarget].ModeId);
            if (existingIdx >= 0 && alignments[existingIdx].AlignmentType != "merge")
            {
                var ea = alignments[existingIdx];
                alignments[existingIdx] = new ModeAlignmentRecord
                {
                    SourceModeId = ea.SourceModeId,
                    TargetModeId = ea.TargetModeId,
                    Metrics = ea.Metrics,
                    AlignmentType = "merge",
                    Confidence = ea.Confidence,
                    AmbiguityNotes = $"Merge: multiple sources map to target {target.Modes[bestTarget].ModeId}",
                };
            }

            // Replace the death alignment for this unmatched source with a merge alignment
            var deathIdx = alignments.FindIndex(a =>
                a.SourceModeId == source.Modes[i].ModeId &&
                a.AlignmentType == "death");
            if (deathIdx >= 0)
            {
                double mergeFeatDist = ModeFeatureVector.Distance(
                    sourceFeatures[i], targetFeatures[bestTarget]);
                alignments[deathIdx] = new ModeAlignmentRecord
                {
                    SourceModeId = source.Modes[i].ModeId,
                    TargetModeId = target.Modes[bestTarget].ModeId,
                    Metrics = new ModeMatchMetricSet
                    {
                        FeatureDistance = mergeFeatDist,
                        AggregateScore = bestScore,
                        IsMatch = true,
                    },
                    AlignmentType = "merge",
                    Confidence = 0.5,
                    AmbiguityNotes = $"Merge: multiple sources map to target {target.Modes[bestTarget].ModeId}",
                };
            }
        }

        // Detect births: target modes with no source match and not already handled as splits
        for (int j = 0; j < nTarget; j++)
        {
            if (!matchedTargets.Contains(j) && !splitTargets.Contains(j))
            {
                alignments.Add(new ModeAlignmentRecord
                {
                    SourceModeId = "",
                    TargetModeId = target.Modes[j].ModeId,
                    Metrics = new ModeMatchMetricSet
                    {
                        FeatureDistance = double.MaxValue,
                        AggregateScore = 0,
                        IsMatch = false,
                    },
                    AlignmentType = "birth",
                    Confidence = 1.0,
                });
            }
        }

        return alignments;
    }

    /// <summary>
    /// Build mode families from a sequence of spectra along a path.
    /// Each consecutive pair is matched; families accumulate members.
    /// After building, runs avoided-crossing post-processing.
    /// </summary>
    public IReadOnlyList<ModeFamilyRecord> BuildFamilies(
        IReadOnlyList<SpectrumBundle> spectraPath,
        ILinearOperator? massOperator = null)
    {
        ArgumentNullException.ThrowIfNull(spectraPath);
        if (spectraPath.Count < 2)
            throw new ArgumentException("Need at least 2 spectra to build families.", nameof(spectraPath));

        // Track families as lists of mode IDs
        var families = new Dictionary<string, FamilyBuilder>();
        int familyCounter = 0;

        // Initialize families from first spectrum
        foreach (var mode in spectraPath[0].Modes)
        {
            string fid = $"family-{familyCounter++}";
            families[mode.ModeId] = new FamilyBuilder
            {
                FamilyId = fid,
                ModeIds = new List<string> { mode.ModeId },
                ContextIds = new List<string> { spectraPath[0].BackgroundId },
                Eigenvalues = new List<double> { mode.Eigenvalue },
                Alignments = new List<ModeAlignmentRecord>(),
                AmbiguityCount = 0,
            };
        }

        // Map from mode ID to family ID
        var modeToFamily = new Dictionary<string, string>();
        foreach (var (modeId, fb) in families)
            modeToFamily[modeId] = fb.FamilyId;

        // Store all step-alignments for avoided-crossing post-processing
        var stepAlignments = new List<IReadOnlyList<ModeAlignmentRecord>>();

        // Match consecutive spectra
        for (int step = 0; step < spectraPath.Count - 1; step++)
        {
            var alignments = Match(spectraPath[step], spectraPath[step + 1], massOperator);
            stepAlignments.Add(alignments);

            var newModeToFamily = new Dictionary<string, string>();

            foreach (var align in alignments)
            {
                if ((align.AlignmentType == "matched" || align.AlignmentType == "split")
                    && align.Metrics.IsMatch)
                {
                    // Extend existing family
                    if (modeToFamily.TryGetValue(align.SourceModeId, out var fid))
                    {
                        var fb = families.Values.First(f => f.FamilyId == fid);
                        fb.ModeIds.Add(align.TargetModeId);
                        fb.ContextIds.Add(spectraPath[step + 1].BackgroundId);
                        var targetMode = spectraPath[step + 1].Modes
                            .FirstOrDefault(m => m.ModeId == align.TargetModeId);
                        if (targetMode != null)
                            fb.Eigenvalues.Add(targetMode.Eigenvalue);
                        fb.Alignments.Add(align);
                        if (align.AmbiguityNotes != null ||
                            align.AlignmentType == "split" ||
                            align.AlignmentType == "merge")
                            fb.AmbiguityCount++;
                        newModeToFamily[align.TargetModeId] = fid;
                    }
                }
                else if (align.AlignmentType == "merge" && align.Metrics.IsMatch)
                {
                    // Merge: the additional source joins its target's family
                    if (modeToFamily.TryGetValue(align.SourceModeId, out var sourceFid))
                    {
                        var fb = families.Values.First(f => f.FamilyId == sourceFid);
                        fb.ModeIds.Add(align.TargetModeId);
                        fb.ContextIds.Add(spectraPath[step + 1].BackgroundId);
                        var targetMode = spectraPath[step + 1].Modes
                            .FirstOrDefault(m => m.ModeId == align.TargetModeId);
                        if (targetMode != null)
                            fb.Eigenvalues.Add(targetMode.Eigenvalue);
                        fb.Alignments.Add(align);
                        fb.AmbiguityCount++;
                        newModeToFamily[align.TargetModeId] = sourceFid;
                    }
                }
                else if (align.AlignmentType == "birth")
                {
                    // New family
                    string fid = $"family-{familyCounter++}";
                    var targetMode = spectraPath[step + 1].Modes
                        .FirstOrDefault(m => m.ModeId == align.TargetModeId);
                    var fb = new FamilyBuilder
                    {
                        FamilyId = fid,
                        ModeIds = new List<string> { align.TargetModeId },
                        ContextIds = new List<string> { spectraPath[step + 1].BackgroundId },
                        Eigenvalues = new List<double> { targetMode?.Eigenvalue ?? 0 },
                        Alignments = new List<ModeAlignmentRecord> { align },
                        AmbiguityCount = 0,
                    };
                    families[align.TargetModeId] = fb;
                    newModeToFamily[align.TargetModeId] = fid;
                }
            }

            modeToFamily = newModeToFamily;
        }

        // --- Avoided-crossing post-processing ---
        // Requires at least 2 consecutive matching steps (3 spectra).
        // Detects cases where two families swap eigenvalue ordering between steps.
        if (spectraPath.Count >= 3 && stepAlignments.Count >= 2)
        {
            DetectAvoidedCrossings(spectraPath, stepAlignments, families);
        }

        // Build final family records
        return families.Values
            .Select(fb =>
            {
                double mean = fb.Eigenvalues.Count > 0 ? fb.Eigenvalues.Average() : 0;
                double spread = fb.Eigenvalues.Count > 1
                    ? fb.Eigenvalues.Max() - fb.Eigenvalues.Min()
                    : 0;
                bool stable = fb.AmbiguityCount == 0 && fb.ModeIds.Count >= 2;

                return new ModeFamilyRecord
                {
                    FamilyId = fb.FamilyId,
                    MemberModeIds = fb.ModeIds.Distinct().ToList(),
                    ContextIds = fb.ContextIds.Distinct().ToList(),
                    MeanEigenvalue = mean,
                    EigenvalueSpread = spread,
                    IsStable = stable,
                    AmbiguityCount = fb.AmbiguityCount,
                    Alignments = fb.Alignments,
                };
            })
            .ToList();
    }

    /// <summary>
    /// Detect avoided crossings by analyzing consecutive matching steps.
    /// An avoided crossing is flagged when two tracked mode families have
    /// near-degenerate eigenvalues at an intermediate step, and the family
    /// eigenvalue trajectories are converging (approaching each other).
    ///
    /// Near-degeneracy with converging trajectories is the hallmark of both
    /// real crossings and avoided crossings. Both require attention: a real
    /// crossing means the tracker may swap mode identities, and an avoided
    /// crossing means the physical mode character swaps despite eigenvalue
    /// ordering being preserved.
    /// </summary>
    private void DetectAvoidedCrossings(
        IReadOnlyList<SpectrumBundle> spectraPath,
        List<IReadOnlyList<ModeAlignmentRecord>> stepAlignments,
        Dictionary<string, FamilyBuilder> families)
    {
        double degThreshold = _config.AvoidedCrossingDegeneracyThreshold;

        for (int step = 0; step < stepAlignments.Count - 1; step++)
        {
            var prevAlignments = stepAlignments[step];
            var nextAlignments = stepAlignments[step + 1];
            var midSpectrum = spectraPath[step + 1];

            // Build map: for step k, source mode -> target mode (matched only)
            var prevSourceToTarget = new Dictionary<string, string>();
            foreach (var a in prevAlignments)
            {
                if (a.AlignmentType == "matched" && a.Metrics.IsMatch)
                    prevSourceToTarget[a.SourceModeId] = a.TargetModeId;
            }

            // Build map: for step k+1, source mode -> target mode (matched only)
            var nextSourceToTarget = new Dictionary<string, string>();
            foreach (var a in nextAlignments)
            {
                if (a.AlignmentType == "matched" && a.Metrics.IsMatch)
                    nextSourceToTarget[a.SourceModeId] = a.TargetModeId;
            }

            // Look at all pairs of modes in the mid-spectrum that are near-degenerate
            for (int a = 0; a < midSpectrum.Modes.Count; a++)
            {
                for (int b = a + 1; b < midSpectrum.Modes.Count; b++)
                {
                    double eigA = midSpectrum.Modes[a].Eigenvalue;
                    double eigB = midSpectrum.Modes[b].Eigenvalue;
                    double scale = System.Math.Max(
                        System.Math.Max(System.Math.Abs(eigA), System.Math.Abs(eigB)),
                        1e-10);
                    double relDiff = System.Math.Abs(eigA - eigB) / scale;

                    if (relDiff >= degThreshold) continue;

                    string midA = midSpectrum.Modes[a].ModeId;
                    string midB = midSpectrum.Modes[b].ModeId;

                    // Check if both have matched continuations in both directions
                    if (!nextSourceToTarget.TryGetValue(midA, out string? nextA)) continue;
                    if (!nextSourceToTarget.TryGetValue(midB, out string? nextB)) continue;

                    string? prevSourceA = null;
                    string? prevSourceB = null;
                    foreach (var kvp in prevSourceToTarget)
                    {
                        if (kvp.Value == midA) prevSourceA = kvp.Key;
                        if (kvp.Value == midB) prevSourceB = kvp.Key;
                    }

                    if (prevSourceA == null || prevSourceB == null) continue;

                    var prevSpectrum = spectraPath[step];
                    var prevModeA = prevSpectrum.Modes.FirstOrDefault(m => m.ModeId == prevSourceA);
                    var prevModeB = prevSpectrum.Modes.FirstOrDefault(m => m.ModeId == prevSourceB);

                    if (prevModeA == null || prevModeB == null) continue;

                    // Avoided crossing detection: the two families were farther apart
                    // at the previous step than they are at the mid-step (they converged
                    // toward the near-degeneracy). This is the signature of an eigenvalue
                    // crossing/avoided-crossing event.
                    double prevSeparation = System.Math.Abs(
                        prevModeA.Eigenvalue - prevModeB.Eigenvalue);
                    double midSeparation = System.Math.Abs(eigA - eigB);

                    // The families must have been farther apart before approaching
                    if (midSeparation >= prevSeparation) continue;

                    // Avoided crossing detected!
                    string note = $"Avoided crossing: modes {midA} and {midB} swap eigenvalue ordering";

                    foreach (var fb in families.Values)
                    {
                        if (fb.ModeIds.Contains(midA) || fb.ModeIds.Contains(midB))
                        {
                            bool alreadyMarked = fb.Alignments.Any(al =>
                                al.AlignmentType == "avoided-crossing" &&
                                al.AmbiguityNotes != null &&
                                al.AmbiguityNotes.Contains(midA) &&
                                al.AmbiguityNotes.Contains(midB));

                            if (!alreadyMarked)
                            {
                                string sourceModeId = fb.ModeIds.Contains(midA) ? midA : midB;
                                string targetModeId = fb.ModeIds.Contains(midA) ? nextA : nextB;
                                fb.Alignments.Add(new ModeAlignmentRecord
                                {
                                    SourceModeId = sourceModeId,
                                    TargetModeId = targetModeId,
                                    Metrics = new ModeMatchMetricSet
                                    {
                                        FeatureDistance = 0,
                                        AggregateScore = 1.0,
                                        IsMatch = true,
                                    },
                                    AlignmentType = "avoided-crossing",
                                    Confidence = 0.5,
                                    AmbiguityNotes = note,
                                });
                                fb.AmbiguityCount++;
                            }
                        }
                    }
                }
            }
        }
    }

    private static ModeFeatureVector[] BuildFeatures(SpectrumBundle spectrum)
    {
        return spectrum.Modes.Select(m => new ModeFeatureVector
        {
            ModeId = m.ModeId,
            Eigenvalue = m.Eigenvalue,
            GaugeLeakScore = m.GaugeLeakScore,
            Multiplicity = 1, // Will be enriched by clustering later
        }).ToArray();
    }

    /// <summary>
    /// Compute aggregate matching score from available metrics.
    /// When all three metrics (O1, O2, O3) are available:
    ///   score = w1*O1 + w2*O2 + w3*O3_feature_score
    /// When O2 is absent:
    ///   score = w1/(w1+w3)*O1 + w3/(w1+w3)*O3_feature_score
    /// When only O3 is available:
    ///   score = O3_feature_score
    /// </summary>
    private double ComputeAggregate(double? nativeOverlap, double? observedOverlap, double featScore)
    {
        double w1 = _config.NativeOverlapWeight;
        double w2 = _config.ObservedOverlapWeight;
        double w3 = _config.FeatureDistanceWeight;

        if (nativeOverlap.HasValue && observedOverlap.HasValue)
        {
            // All three metrics available: full weighted sum
            return w1 * nativeOverlap.Value + w2 * observedOverlap.Value + w3 * featScore;
        }
        else if (nativeOverlap.HasValue)
        {
            // O1 and O3 available, no O2: renormalize w1 and w3
            double wSum = w1 + w3;
            return (w1 / wSum) * nativeOverlap.Value + (w3 / wSum) * featScore;
        }
        else if (observedOverlap.HasValue)
        {
            // O2 and O3 available, no O1: renormalize w2 and w3
            double wSum = w2 + w3;
            return (w2 / wSum) * observedOverlap.Value + (w3 / wSum) * featScore;
        }
        else
        {
            // Only O3 available
            return featScore;
        }
    }

    private static double ComputeNativeOverlap(
        double[] v1, double[] v2, ILinearOperator massOp)
    {
        int n = v1.Length;
        var ft2 = new FieldTensor
        {
            Label = "v2",
            Signature = massOp.InputSignature,
            Coefficients = v2,
            Shape = new[] { n },
        };
        var mv2 = massOp.Apply(ft2);
        double dot = 0;
        for (int i = 0; i < n; i++)
            dot += v1[i] * mv2.Coefficients[i];
        return System.Math.Abs(dot);
    }

    private sealed class FamilyBuilder
    {
        public required string FamilyId { get; init; }
        public required List<string> ModeIds { get; init; }
        public required List<string> ContextIds { get; init; }
        public required List<double> Eigenvalues { get; init; }
        public required List<ModeAlignmentRecord> Alignments { get; init; }
        public int AmbiguityCount { get; set; }
    }
}

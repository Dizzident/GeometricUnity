using Gu.Core;
using Gu.Phase3.Observables;
using Gu.Phase3.Spectra;
using Gu.Phase3.ModeTracking;
using System.Text.Json;

namespace Gu.Phase3.ModeTracking.Tests;

public class ModeTrackingTests
{
    /// <summary>
    /// Build a synthetic spectrum with known eigenvalues and identity eigenvectors.
    /// </summary>
    private static SpectrumBundle MakeSpectrum(
        string bgId, double[] eigenvalues, double[][]? vectors = null)
    {
        int n = eigenvalues.Length;
        var modes = new ModeRecord[n];
        for (int i = 0; i < n; i++)
        {
            double[] v;
            if (vectors != null)
                v = vectors[i];
            else
            {
                v = new double[n];
                v[i] = 1.0; // identity eigenvector
            }

            modes[i] = new ModeRecord
            {
                ModeId = $"{bgId}-mode-{i}",
                BackgroundId = bgId,
                OperatorType = SpectralOperatorType.GaussNewton,
                Eigenvalue = eigenvalues[i],
                ResidualNorm = 0,
                NormalizationConvention = "unit-M-norm",
                GaugeLeakScore = 0,
                ModeVector = v,
                ModeIndex = i,
            };
        }

        return new SpectrumBundle
        {
            SpectrumId = $"spectrum-{bgId}",
            BackgroundId = bgId,
            OperatorBundleId = $"bundle-{bgId}",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            SolverMethod = "explicit-dense",
            StateDimension = n,
            Modes = modes,
            Clusters = Array.Empty<ModeCluster>(),
            ConvergenceStatus = "converged",
            IterationsUsed = 0,
            MaxOrthogonalityDefect = 0,
        };
    }

    [Fact]
    public void MatchIdenticalSpectra_AllMatched()
    {
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0, 9.0 });
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.0, 4.0, 9.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(spectrum1, spectrum2);

        var matched = alignments.Where(a => a.AlignmentType == "matched").ToList();
        Assert.Equal(3, matched.Count);
        Assert.All(matched, a => Assert.True(a.Metrics.IsMatch));
    }

    [Fact]
    public void MatchShiftedSpectrum_SmallShiftMatches()
    {
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0, 9.0 });
        // Small perturbation of eigenvalues
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.01, 4.02, 8.98 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(spectrum1, spectrum2);

        var matched = alignments.Where(a => a.AlignmentType == "matched").ToList();
        Assert.Equal(3, matched.Count);
    }

    [Fact]
    public void MatchWithBirth_DetectsNewMode()
    {
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0 });
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.0, 4.0, 9.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(spectrum1, spectrum2);

        // The extra target mode should be detected as either a birth or a split
        var birthsOrSplits = alignments.Where(a =>
            a.AlignmentType == "birth" || a.AlignmentType == "split").ToList();
        Assert.True(birthsOrSplits.Count >= 1, "Should detect at least one birth or split");
    }

    [Fact]
    public void MatchWithDeath_DetectsLostMode()
    {
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0, 9.0 });
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.0, 9.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(spectrum1, spectrum2);

        // One source mode should not match (death, merge, or unmatched)
        var unmatched = alignments.Where(a =>
            a.AlignmentType == "death" ||
            a.AlignmentType == "merge" ||
            (a.AlignmentType == "matched" && !a.Metrics.IsMatch)).ToList();
        Assert.True(unmatched.Count >= 1, "Should detect at least one death/merge/unmatched");
    }

    [Fact]
    public void AmbiguousMatch_IsReported()
    {
        // Two very similar eigenvalues that create ambiguity
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0, 1.001, 9.0 });
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.0005, 1.0005, 9.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            AmbiguityRatio = 0.95, // Very sensitive to ambiguity
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(spectrum1, spectrum2);

        // At least one alignment should be flagged as ambiguous
        var ambiguous = alignments.Where(a => a.AmbiguityNotes != null).ToList();
        Assert.True(ambiguous.Count >= 1, "Should detect ambiguity for near-degenerate eigenvalues");
    }

    [Fact]
    public void BuildFamilies_ContinuationPath_PersistsFamilies()
    {
        // Three spectra along a continuation path with gradually shifting eigenvalues
        var s1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0, 9.0 });
        var s2 = MakeSpectrum("bg-2", new[] { 1.1, 4.1, 9.1 });
        var s3 = MakeSpectrum("bg-3", new[] { 1.2, 4.2, 9.2 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var families = engine.BuildFamilies(new[] { s1, s2, s3 });

        // Should have families that persist across all 3 contexts
        var persistent = families.Where(f => f.ContextIds.Count == 3).ToList();
        Assert.True(persistent.Count >= 2,
            $"Expected at least 2 persistent families, got {persistent.Count}");
    }

    [Fact]
    public void BuildFamilies_BranchSweep_DetectsBirthsAndDeaths()
    {
        var s1 = MakeSpectrum("branch-A", new[] { 1.0, 4.0, 9.0 });
        var s2 = MakeSpectrum("branch-B", new[] { 1.0, 4.0, 16.0, 25.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.BranchSweep,
            MatchThreshold = 0.3,
        };

        var engine = new ModeMatchingEngine(config);
        var families = engine.BuildFamilies(new[] { s1, s2 });

        // Some families should have members in both contexts, some only in one
        Assert.True(families.Count >= 3, "Should have families for matched + births");
    }

    [Fact]
    public void ModeFamilyRecord_SerializesCleanly()
    {
        var s1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0, 9.0 });
        var s2 = MakeSpectrum("bg-2", new[] { 1.1, 4.1, 9.1 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var families = engine.BuildFamilies(new[] { s1, s2 });

        var json = JsonSerializer.Serialize(families, new JsonSerializerOptions { WriteIndented = true });
        Assert.NotEmpty(json);
        Assert.Contains("familyId", json);
        Assert.Contains("memberModeIds", json);
        Assert.Contains("meanEigenvalue", json);
    }

    [Fact]
    public void ContinuationModeTrack_SerializesCleanly()
    {
        var s1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0 });
        var s2 = MakeSpectrum("bg-2", new[] { 1.1, 4.1 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var families = engine.BuildFamilies(new[] { s1, s2 });

        var track = new ContinuationModeTrack
        {
            TrackId = "track-1",
            PathBackgroundIds = new[] { "bg-1", "bg-2" },
            Families = families,
        };

        var json = JsonSerializer.Serialize(track, new JsonSerializerOptions { WriteIndented = true });
        Assert.NotEmpty(json);
        Assert.Contains("trackId", json);
        Assert.Contains("families", json);
        Assert.Contains("persistentFamilyCount", json);
    }

    [Fact]
    public void CrossBranchModeMap_SerializesCleanly()
    {
        var s1 = MakeSpectrum("branch-A", new[] { 1.0, 4.0 });
        var s2 = MakeSpectrum("branch-B", new[] { 1.1, 4.1 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.BranchSweep,
            MatchThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(s1, s2);

        var map = new CrossBranchModeMap
        {
            SourceBranchId = "branch-A",
            TargetBranchId = "branch-B",
            Alignments = alignments,
        };

        var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
        Assert.NotEmpty(json);
        Assert.Contains("sourceBranchId", json);
        Assert.Contains("matchedCount", json);
    }

    [Fact]
    public void HungarianAlgorithm_SquareMatrix_FindsOptimal()
    {
        // Cost matrix: optimal assignment should be (0->0, 1->1, 2->2) = cost 3
        var cost = new double[3, 3]
        {
            { 1, 100, 100 },
            { 100, 1, 100 },
            { 100, 100, 1 },
        };
        var assignment = HungarianAlgorithm.Solve(cost);

        Assert.Equal(0, assignment[0]);
        Assert.Equal(1, assignment[1]);
        Assert.Equal(2, assignment[2]);
    }

    [Fact]
    public void HungarianAlgorithm_RectangularMatrix_Works()
    {
        // More columns than rows
        var cost = new double[2, 3]
        {
            { 1, 100, 50 },
            { 100, 1, 50 },
        };
        var assignment = HungarianAlgorithm.Solve(cost);

        Assert.Equal(0, assignment[0]); // row 0 -> col 0 (cost 1)
        Assert.Equal(1, assignment[1]); // row 1 -> col 1 (cost 1)
    }

    [Fact]
    public void HungarianAlgorithm_NearDegenerateSpectrum_AssignmentIsOptimal()
    {
        // 4x4 cost matrix where a greedy assignment gives suboptimal total cost.
        // cost[i,j] = [[1,2,3,4],[2,4,6,8],[3,6,9,12],[4,1,2,3]]
        //
        // Greedy (pick min in row order, avoiding used columns):
        //   row 0 -> col 0 (cost 1)
        //   row 1 -> col 1 (cost 4)  [col 0 taken]
        //   row 2 -> col 2 (cost 9)  [cols 0,1 taken]
        //   row 3 -> col 3 (cost 3)  [cols 0,1,2 taken]
        //   greedy total = 1+4+9+3 = 17
        //
        // Optimal assignment (verified by exhaustive check):
        //   row 0 -> col 0 (cost 1)
        //   row 1 -> col 2 (cost 6)  -- wait, let's verify
        //
        // Exhaustive check of all 24 permutations:
        //   (0,1,2,3): 1+4+9+3 = 17
        //   (0,1,3,2): 1+4+12+2 = 19
        //   (0,2,1,3): 1+6+6+3 = 16
        //   (0,2,3,1): 1+6+12+1 = 20
        //   (0,3,1,2): 1+8+6+2 = 17
        //   (0,3,2,1): 1+8+9+1 = 19
        //   (1,0,2,3): 2+2+9+3 = 16
        //   (1,0,3,2): 2+2+12+2 = 18
        //   (1,2,0,3): 2+6+3+3 = 14  <-- candidate
        //   (1,2,3,0): 2+6+12+4 = 24
        //   (1,3,0,2): 2+8+3+2 = 15
        //   (1,3,2,0): 2+8+9+4 = 23
        //   (2,0,1,3): 3+2+6+3 = 14  <-- candidate
        //   (2,0,3,1): 3+2+12+1 = 18
        //   (2,1,0,3): 3+4+3+3 = 13  <-- candidate
        //   (2,1,3,0): 3+4+12+4 = 23
        //   (2,3,0,1): 3+8+3+1 = 15
        //   (2,3,1,0): 3+8+6+4 = 21
        //   (3,0,1,2): 4+2+6+2 = 14
        //   (3,0,2,1): 4+2+9+1 = 16
        //   (3,1,0,2): 4+4+3+2 = 13  <-- candidate
        //   (3,1,2,0): 4+4+9+4 = 21
        //   (3,2,0,1): 4+6+3+1 = 14
        //   (3,2,1,0): 4+6+6+4 = 20
        //
        // Optimal total cost = 13, achieved by (2,1,0,3) or (3,1,0,2).
        var costMatrix = new double[4, 4]
        {
            {  1,  2,  3,  4 },
            {  2,  4,  6,  8 },
            {  3,  6,  9, 12 },
            {  4,  1,  2,  3 },
        };

        var assignment = HungarianAlgorithm.Solve(costMatrix);

        // All 4 rows must be assigned to distinct columns
        Assert.Equal(4, assignment.Length);
        Assert.Equal(4, assignment.Distinct().Count()); // all unique
        Assert.All(assignment, col => Assert.InRange(col, 0, 3));

        // Compute total cost of returned assignment
        double totalCost = 0;
        for (int i = 0; i < 4; i++)
            totalCost += costMatrix[i, assignment[i]];

        // Must be strictly better than the greedy result (17) and equal to optimal (13)
        Assert.True(totalCost < 17,
            $"Expected cost < greedy cost 17, got {totalCost} with assignment [{string.Join(",", assignment)}]");
        Assert.Equal(13.0, totalCost, 1e-10);
    }

    [Fact]
    public void ModeFeatureVector_Distance_SimilarModesAreClose()
    {
        var a = new ModeFeatureVector
        {
            ModeId = "a", Eigenvalue = 1.0, GaugeLeakScore = 0.01, Multiplicity = 1,
        };
        var b = new ModeFeatureVector
        {
            ModeId = "b", Eigenvalue = 1.01, GaugeLeakScore = 0.02, Multiplicity = 1,
        };
        var c = new ModeFeatureVector
        {
            ModeId = "c", Eigenvalue = 100.0, GaugeLeakScore = 0.9, Multiplicity = 3,
        };

        double distAB = ModeFeatureVector.Distance(a, b);
        double distAC = ModeFeatureVector.Distance(a, c);

        Assert.True(distAB < distAC, "Similar modes should be closer than dissimilar modes");
        Assert.True(distAB < 0.1, $"Near-identical modes should have small distance, got {distAB}");
    }

    // --- GAP-2: O2 observed-signature overlap metric tests ---

    private static TensorSignature TestSignature => new TensorSignature
    {
        AmbientSpaceId = "test",
        CarrierType = "observed",
        Degree = "0",
        LieAlgebraBasisId = "trivial",
        ComponentOrderId = "default",
        MemoryLayout = "dense-row-major",
    };

    private static ObservedModeSignature MakeSignature(string modeId, string bgId, double[] coeffs)
    {
        return new ObservedModeSignature
        {
            ModeId = modeId,
            BackgroundId = bgId,
            ObservedCoefficients = coeffs,
            ObservedSignature = TestSignature,
            ObservedShape = new[] { coeffs.Length },
            LinearizationMethod = LinearizationMethod.Analytic,
        };
    }

    [Fact]
    public void O2_IdenticalSignatures_OverlapIsOne()
    {
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0 });
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.0, 4.0 });

        var signatures = new[]
        {
            MakeSignature("bg-1-mode-0", "bg-1", new[] { 1.0, 0.0, 0.0 }),
            MakeSignature("bg-1-mode-1", "bg-1", new[] { 0.0, 1.0, 0.0 }),
            MakeSignature("bg-2-mode-0", "bg-2", new[] { 1.0, 0.0, 0.0 }),  // identical to mode-0
            MakeSignature("bg-2-mode-1", "bg-2", new[] { 0.0, 1.0, 0.0 }),  // identical to mode-1
        };

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            NativeOverlapWeight = 0.3,
            ObservedOverlapWeight = 0.4,
            FeatureDistanceWeight = 0.3,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(spectrum1, spectrum2, signatures: signatures);

        var matched = alignments.Where(a => a.AlignmentType == "matched").ToList();
        Assert.Equal(2, matched.Count);

        // Identical signatures should give O2 = 1.0
        foreach (var m in matched)
        {
            Assert.NotNull(m.Metrics.ObservedSignatureOverlap);
            Assert.Equal(1.0, m.Metrics.ObservedSignatureOverlap!.Value, 1e-10);
        }
    }

    [Fact]
    public void O2_OrthogonalSignatures_OverlapIsZero()
    {
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0 });
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.0 });

        var signatures = new[]
        {
            MakeSignature("bg-1-mode-0", "bg-1", new[] { 1.0, 0.0, 0.0 }),
            MakeSignature("bg-2-mode-0", "bg-2", new[] { 0.0, 1.0, 0.0 }),  // orthogonal
        };

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.0,  // low threshold so matching still happens
            NativeOverlapWeight = 0.3,
            ObservedOverlapWeight = 0.4,
            FeatureDistanceWeight = 0.3,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(spectrum1, spectrum2, signatures: signatures);

        var matched = alignments.Where(a => a.AlignmentType == "matched" || a.Metrics.ObservedSignatureOverlap.HasValue).ToList();
        Assert.True(matched.Count >= 1);

        foreach (var m in matched)
        {
            Assert.NotNull(m.Metrics.ObservedSignatureOverlap);
            Assert.True(m.Metrics.ObservedSignatureOverlap!.Value < 1e-10,
                $"Orthogonal signatures should have O2 near 0, got {m.Metrics.ObservedSignatureOverlap.Value}");
        }
    }

    [Fact]
    public void O2_AggregateScoreDiffers_WithAndWithoutSignatures()
    {
        // Use slightly different eigenvalues so feature scores are not exactly 1.0
        var spectrum1 = MakeSpectrum("bg-1", new[] { 1.0, 4.0 });
        var spectrum2 = MakeSpectrum("bg-2", new[] { 1.05, 4.05 });

        // Signatures with partial overlap (not identical, not orthogonal)
        var signatures = new[]
        {
            MakeSignature("bg-1-mode-0", "bg-1", new[] { 1.0, 0.3, 0.0 }),
            MakeSignature("bg-1-mode-1", "bg-1", new[] { 0.0, 1.0, 0.2 }),
            MakeSignature("bg-2-mode-0", "bg-2", new[] { 0.9, 0.4, 0.1 }),
            MakeSignature("bg-2-mode-1", "bg-2", new[] { 0.1, 0.8, 0.5 }),
        };

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            NativeOverlapWeight = 0.3,
            ObservedOverlapWeight = 0.4,
            FeatureDistanceWeight = 0.3,
        };

        var engine = new ModeMatchingEngine(config);

        // Without signatures
        var alignmentsNoSig = engine.Match(spectrum1, spectrum2);
        // With signatures
        var alignmentsWithSig = engine.Match(spectrum1, spectrum2, signatures: signatures);

        var matchedNoSig = alignmentsNoSig.Where(a => a.AlignmentType == "matched").ToList();
        var matchedWithSig = alignmentsWithSig.Where(a => a.AlignmentType == "matched").ToList();

        Assert.Equal(matchedNoSig.Count, matchedWithSig.Count);
        Assert.True(matchedNoSig.Count > 0, "Should have at least one match");

        // Without signatures: O2 is null; with signatures: O2 is set
        bool anyDiffers = false;
        foreach (var m in matchedNoSig)
            Assert.Null(m.Metrics.ObservedSignatureOverlap);
        foreach (var m in matchedWithSig)
            Assert.NotNull(m.Metrics.ObservedSignatureOverlap);

        // At least one pair should have a different aggregate score
        for (int i = 0; i < matchedNoSig.Count; i++)
        {
            if (System.Math.Abs(matchedNoSig[i].Metrics.AggregateScore - matchedWithSig[i].Metrics.AggregateScore) > 1e-10)
                anyDiffers = true;
        }

        Assert.True(anyDiffers, "Aggregate scores should differ when O2 is supplied vs absent");
    }

    [Fact]
    public void ModeMatchMetricSet_O2_SerializesNullable()
    {
        // With O2 set
        var metricWithO2 = new ModeMatchMetricSet
        {
            NativeOverlap = 0.9,
            ObservedSignatureOverlap = 0.85,
            FeatureDistance = 0.1,
            AggregateScore = 0.88,
            IsMatch = true,
        };

        var jsonWithO2 = JsonSerializer.Serialize(metricWithO2);
        Assert.Contains("observedSignatureOverlap", jsonWithO2);
        Assert.Contains("0.85", jsonWithO2);

        // Without O2 (null) — should be omitted due to JsonIgnoreCondition.WhenWritingNull
        var metricWithoutO2 = new ModeMatchMetricSet
        {
            NativeOverlap = 0.9,
            FeatureDistance = 0.1,
            AggregateScore = 0.7,
            IsMatch = true,
        };

        var jsonWithoutO2 = JsonSerializer.Serialize(metricWithoutO2);
        Assert.DoesNotContain("observedSignatureOverlap", jsonWithoutO2);

        // Round-trip with O2
        var deserialized = JsonSerializer.Deserialize<ModeMatchMetricSet>(jsonWithO2);
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized!.ObservedSignatureOverlap);
        Assert.Equal(0.85, deserialized.ObservedSignatureOverlap!.Value, 1e-10);
    }

    // --- GAP-3: Split/merge/avoided-crossing detection tests ---

    [Fact]
    public void Split_OneSourceToMultipleTargets_DetectedAsSplit()
    {
        // Source: 2 modes. Target: 3 modes where both target 0 and target 2
        // are close to source 0 (a split).
        // Source mode 0 (eigenvalue 1.0) splits into target 0 (1.0) and target 2 (1.05).
        // Source mode 1 (eigenvalue 10.0) matches target 1 (10.0).
        var source = MakeSpectrum("src", new[] { 1.0, 10.0 });
        var target = MakeSpectrum("tgt", new[] { 1.0, 10.0, 1.05 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            SplitThreshold = 0.4,
            FeatureDistanceScale = 1.0,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(source, target);

        var splits = alignments.Where(a => a.AlignmentType == "split").ToList();
        Assert.True(splits.Count >= 2,
            $"Expected at least 2 split alignment records, got {splits.Count}. " +
            $"Types: {string.Join(", ", alignments.Select(a => $"{a.SourceModeId}->{a.TargetModeId}:{a.AlignmentType}"))}");

        // Both split records should reference the same source mode
        var splitSources = splits.Select(s => s.SourceModeId).Distinct().ToList();
        Assert.Single(splitSources);
    }

    [Fact]
    public void Merge_MultipleSourcesToOneTarget_DetectedAsMerge()
    {
        // Source: 3 modes. Target: 2 modes.
        // Source 0 (1.0) and Source 2 (1.05) both merge into target 0 (1.02).
        // Source 1 (10.0) matches target 1 (10.0).
        var source = MakeSpectrum("src", new[] { 1.0, 10.0, 1.05 });
        var target = MakeSpectrum("tgt", new[] { 1.02, 10.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            SplitThreshold = 0.4,
            FeatureDistanceScale = 1.0,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(source, target);

        var merges = alignments.Where(a => a.AlignmentType == "merge").ToList();
        Assert.True(merges.Count >= 2,
            $"Expected at least 2 merge alignment records, got {merges.Count}. " +
            $"Types: {string.Join(", ", alignments.Select(a => $"{a.SourceModeId}->{a.TargetModeId}:{a.AlignmentType}"))}");

        // Both merge records should reference the same target mode
        var mergeTargets = merges.Select(m => m.TargetModeId).Distinct().ToList();
        Assert.Single(mergeTargets);
    }

    [Fact]
    public void Split_AppearsInCrossBranchModeMapSummary()
    {
        var source = MakeSpectrum("branch-A", new[] { 1.0, 10.0 });
        var target = MakeSpectrum("branch-B", new[] { 1.0, 10.0, 1.05 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.BranchSweep,
            MatchThreshold = 0.3,
            SplitThreshold = 0.4,
            FeatureDistanceScale = 1.0,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(source, target);

        var map = new CrossBranchModeMap
        {
            SourceBranchId = "branch-A",
            TargetBranchId = "branch-B",
            Alignments = alignments,
        };

        Assert.True(map.SplitCount >= 2,
            $"Expected split count >= 2, got {map.SplitCount}");

        // Serialize and check that splitCount appears
        var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
        Assert.Contains("splitCount", json);
    }

    [Fact]
    public void Merge_AppearsInCrossBranchModeMapSummary()
    {
        var source = MakeSpectrum("branch-A", new[] { 1.0, 10.0, 1.05 });
        var target = MakeSpectrum("branch-B", new[] { 1.02, 10.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.BranchSweep,
            MatchThreshold = 0.3,
            SplitThreshold = 0.4,
            FeatureDistanceScale = 1.0,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(source, target);

        var map = new CrossBranchModeMap
        {
            SourceBranchId = "branch-A",
            TargetBranchId = "branch-B",
            Alignments = alignments,
        };

        Assert.True(map.MergeCount >= 2,
            $"Expected merge count >= 2, got {map.MergeCount}");

        var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
        Assert.Contains("mergeCount", json);
    }

    [Fact]
    public void SplitFamily_HasAmbiguityCountGreaterThanZero()
    {
        // Build families with a split at step 2
        var s1 = MakeSpectrum("bg-1", new[] { 1.0, 10.0 });
        var s2 = MakeSpectrum("bg-2", new[] { 1.0, 10.0, 1.05 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            SplitThreshold = 0.4,
            FeatureDistanceScale = 1.0,
        };

        var engine = new ModeMatchingEngine(config);
        var families = engine.BuildFamilies(new[] { s1, s2 });

        // The family containing the split source should have AmbiguityCount > 0
        var splitFamilies = families.Where(f =>
            f.Alignments.Any(a => a.AlignmentType == "split")).ToList();
        Assert.True(splitFamilies.Count >= 1,
            "Should have at least one family with split alignment");
        Assert.All(splitFamilies, f =>
            Assert.True(f.AmbiguityCount > 0,
                $"Family {f.FamilyId} with split should have AmbiguityCount > 0, got {f.AmbiguityCount}"));
    }

    [Fact]
    public void MergeFamily_HasAmbiguityCountGreaterThanZero()
    {
        var s1 = MakeSpectrum("bg-1", new[] { 1.0, 10.0, 1.05 });
        var s2 = MakeSpectrum("bg-2", new[] { 1.02, 10.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            SplitThreshold = 0.4,
            FeatureDistanceScale = 1.0,
        };

        var engine = new ModeMatchingEngine(config);
        var families = engine.BuildFamilies(new[] { s1, s2 });

        var mergeFamilies = families.Where(f =>
            f.Alignments.Any(a => a.AlignmentType == "merge")).ToList();
        Assert.True(mergeFamilies.Count >= 1,
            "Should have at least one family with merge alignment");
        Assert.All(mergeFamilies, f =>
            Assert.True(f.AmbiguityCount > 0,
                $"Family {f.FamilyId} with merge should have AmbiguityCount > 0, got {f.AmbiguityCount}"));
    }

    [Fact]
    public void AvoidedCrossing_EigenvalueSwap_DetectedInFamilies()
    {
        // Three-step continuation where two modes undergo an avoided crossing.
        // The eigenvalues approach, nearly touch at step 2, then diverge.
        // The tracked eigenvalue trajectories reverse direction through the near-degeneracy.
        var s1 = MakeSpectrum("bg-1", new[] { 1.0, 3.0 });
        var s2 = MakeSpectrum("bg-2", new[] { 1.95, 2.05 });
        var s3 = MakeSpectrum("bg-3", new[] { 3.0, 1.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.1,
            AvoidedCrossingDegeneracyThreshold = 0.15,
            FeatureDistanceScale = 2.0,
        };

        var engine = new ModeMatchingEngine(config);

        // Verify step matching first
        var step12 = engine.Match(s1, s2);
        var step23 = engine.Match(s2, s3);

        var diagInfo = $"Step1->2: {string.Join("; ", step12.Select(a => $"{a.SourceModeId}->{a.TargetModeId}:{a.AlignmentType}(score={a.Metrics.AggregateScore:F4})"))}. " +
                       $"Step2->3: {string.Join("; ", step23.Select(a => $"{a.SourceModeId}->{a.TargetModeId}:{a.AlignmentType}(score={a.Metrics.AggregateScore:F4})"))}";

        var families = engine.BuildFamilies(new[] { s1, s2, s3 });

        var avoidedCrossingFamilies = families.Where(f =>
            f.Alignments.Any(a => a.AlignmentType == "avoided-crossing")).ToList();
        Assert.True(avoidedCrossingFamilies.Count >= 1,
            $"Expected at least 1 family with avoided-crossing alignment, " +
            $"got {avoidedCrossingFamilies.Count}. {diagInfo}. " +
            $"Families: {string.Join(", ", families.Select(f => $"{f.FamilyId}:modes=[{string.Join(",", f.MemberModeIds)}],alignments=[{string.Join(",", f.Alignments.Select(a => a.AlignmentType))}]"))}");

        Assert.All(avoidedCrossingFamilies, f =>
            Assert.True(f.AmbiguityCount > 0,
                $"Family {f.FamilyId} with avoided-crossing should have AmbiguityCount > 0"));
    }

    [Fact]
    public void AvoidedCrossing_AppearsInCrossBranchModeMapCount()
    {
        // For CrossBranchModeMap, avoided crossings only make sense in a continuation context.
        // We build alignments that include an avoided-crossing from BuildFamilies and
        // verify that the count property works.
        var alignments = new List<ModeAlignmentRecord>
        {
            new ModeAlignmentRecord
            {
                SourceModeId = "m1",
                TargetModeId = "m2",
                Metrics = new ModeMatchMetricSet
                {
                    FeatureDistance = 0.1,
                    AggregateScore = 0.9,
                    IsMatch = true,
                },
                AlignmentType = "avoided-crossing",
                Confidence = 0.5,
                AmbiguityNotes = "Avoided crossing: modes m1 and m2 swap",
            },
            new ModeAlignmentRecord
            {
                SourceModeId = "m3",
                TargetModeId = "m4",
                Metrics = new ModeMatchMetricSet
                {
                    FeatureDistance = 0.05,
                    AggregateScore = 0.95,
                    IsMatch = true,
                },
                AlignmentType = "matched",
                Confidence = 1.0,
            },
        };

        var map = new CrossBranchModeMap
        {
            SourceBranchId = "branch-A",
            TargetBranchId = "branch-B",
            Alignments = alignments,
        };

        Assert.Equal(1, map.AvoidedCrossingCount);
        Assert.Equal(1, map.MatchedCount);

        var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
        Assert.Contains("avoidedCrossingCount", json);
    }

    // --- GAP-10: ModeTracking test coverage gaps ---

    [Fact]
    public void BranchMatchStability_ThreeVariants_ConsistentFamilies()
    {
        // Build 3 branch variant spectra with slightly perturbed eigenvalues
        var s1 = MakeSpectrum("branch-A", new[] { 1.0, 4.0, 9.0 });
        var s2 = MakeSpectrum("branch-B", new[] { 1.02, 4.05, 8.97 });
        var s3 = MakeSpectrum("branch-C", new[] { 1.01, 3.98, 9.03 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.BranchSweep,
            MatchThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);

        // Match each pair: all 3 modes should match in each pair
        var alignAB = engine.Match(s1, s2);
        var alignAC = engine.Match(s1, s3);
        var alignBC = engine.Match(s2, s3);

        Assert.Equal(3, alignAB.Count(a => a.AlignmentType == "matched"));
        Assert.Equal(3, alignAC.Count(a => a.AlignmentType == "matched"));
        Assert.Equal(3, alignBC.Count(a => a.AlignmentType == "matched"));

        // Build families across all 3: mode family IDs should be consistent
        var families = engine.BuildFamilies(new[] { s1, s2, s3 });

        // Families that persist across all 3 contexts should be stable
        var persistent = families.Where(f => f.ContextIds.Count == 3).ToList();
        Assert.True(persistent.Count >= 2,
            $"Expected at least 2 persistent families across 3 branch variants, got {persistent.Count}");
        Assert.All(persistent, f => Assert.True(f.IsStable,
            $"Family {f.FamilyId} persisting across all branches should be IsStable=true"));
    }

    [Fact]
    public void SplitBookkeeping_FamilyRecordsAmbiguity()
    {
        // Source spectrum with 2 modes, target with 3 modes where source mode 0 splits.
        // Use explicit vectors to ensure overlap-based split detection.
        var sourceVecs = new double[][]
        {
            new double[] { 1.0, 0.0, 0.0 },  // mode-0
            new double[] { 0.0, 0.0, 1.0 },  // mode-1
        };
        var targetVecs = new double[][]
        {
            new double[] { 1.0, 0.0, 0.0 },  // matches source mode-0
            new double[] { 0.9, 0.1, 0.0 },  // also overlaps source mode-0 (split)
            new double[] { 0.0, 0.0, 1.0 },  // matches source mode-1
        };

        var source = MakeSpectrum("bg-src", new[] { 1.0, 9.0 }, sourceVecs);
        var target = MakeSpectrum("bg-tgt", new[] { 1.0, 1.01, 9.0 }, targetVecs);

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            SplitThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(source, target);

        // At least one alignment should be "split"
        var splits = alignments.Where(a => a.AlignmentType == "split").ToList();
        Assert.True(splits.Count >= 1,
            $"Expected split alignment. Types: {string.Join(", ", alignments.Select(a => a.AlignmentType))}");

        // Build families and verify AmbiguityCount > 0 for split families
        var families = engine.BuildFamilies(new[] { source, target });
        var splitFamilies = families.Where(f =>
            f.Alignments.Any(a => a.AlignmentType == "split")).ToList();
        if (splitFamilies.Count > 0)
        {
            Assert.All(splitFamilies, f => Assert.True(f.AmbiguityCount > 0,
                $"Split family {f.FamilyId} should have AmbiguityCount > 0"));
        }

        // CrossBranchModeMap split count
        var map = new CrossBranchModeMap
        {
            SourceBranchId = "bg-src",
            TargetBranchId = "bg-tgt",
            Alignments = alignments,
        };
        Assert.True(map.SplitCount >= 1);
    }

    [Fact]
    public void MergeBookkeeping_FamilyRecordsAmbiguity()
    {
        // Symmetric to split: 3 source modes, 2 target modes.
        var sourceVecs = new double[][]
        {
            new double[] { 1.0, 0.0, 0.0 },
            new double[] { 0.9, 0.1, 0.0 },  // similar to mode-0
            new double[] { 0.0, 0.0, 1.0 },
        };
        var targetVecs = new double[][]
        {
            new double[] { 1.0, 0.0, 0.0 },  // absorbs source modes 0 and 1
            new double[] { 0.0, 0.0, 1.0 },
        };

        var source = MakeSpectrum("bg-src", new[] { 1.0, 1.01, 9.0 }, sourceVecs);
        var target = MakeSpectrum("bg-tgt", new[] { 1.0, 9.0 }, targetVecs);

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            SplitThreshold = 0.3,
            FeatureDistanceScale = 0.5,
        };

        var engine = new ModeMatchingEngine(config);
        var alignments = engine.Match(source, target);

        var merges = alignments.Where(a => a.AlignmentType == "merge").ToList();
        Assert.True(merges.Count >= 1,
            $"Expected merge alignment. Types: {string.Join(", ", alignments.Select(a => a.AlignmentType))}");

        // CrossBranchModeMap merge count
        var map = new CrossBranchModeMap
        {
            SourceBranchId = "bg-src",
            TargetBranchId = "bg-tgt",
            Alignments = alignments,
        };
        Assert.True(map.MergeCount >= 1);
    }

    [Fact]
    public void AvoidedCrossing_ThreeStepPath_SwapDetected()
    {
        // 3-step continuation path where two modes swap eigenvalue ordering.
        // Step 1: mode 0 at 1.0, mode 1 at 3.0
        // Step 2: mode 0 at 2.0, mode 1 at 2.1 (near-degenerate, within threshold)
        // Step 3: mode 0 at 3.0, mode 1 at 1.0 (swapped!)
        var s1 = MakeSpectrum("step-1", new[] { 1.0, 3.0 });
        var s2 = MakeSpectrum("step-2", new[] { 2.0, 2.1 });
        var s3 = MakeSpectrum("step-3", new[] { 3.0, 1.0 });

        var config = new TrackingConfig
        {
            ContextType = TrackingContextType.Continuation,
            MatchThreshold = 0.3,
            FeatureDistanceScale = 1.0,
            AvoidedCrossingDegeneracyThreshold = 0.2,
        };

        var engine = new ModeMatchingEngine(config);
        var families = engine.BuildFamilies(new[] { s1, s2, s3 });

        // BuildFamilies should detect the swap and mark avoided-crossing
        var acAlignments = families
            .SelectMany(f => f.Alignments)
            .Where(a => a.AlignmentType == "avoided-crossing")
            .ToList();
        Assert.True(acAlignments.Count >= 1,
            $"Expected avoided-crossing detection. " +
            $"Alignment types: {string.Join(", ", families.SelectMany(f => f.Alignments).Select(a => a.AlignmentType))}");

        // Families with avoided-crossing should have AmbiguityCount > 0
        var acFamilies = families.Where(f =>
            f.Alignments.Any(a => a.AlignmentType == "avoided-crossing")).ToList();
        Assert.All(acFamilies, f => Assert.True(f.AmbiguityCount > 0));
    }
}

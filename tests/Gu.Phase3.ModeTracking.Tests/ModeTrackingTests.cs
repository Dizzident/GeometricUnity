using Gu.Core;
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

        var births = alignments.Where(a => a.AlignmentType == "birth").ToList();
        Assert.True(births.Count >= 1, "Should detect at least one birth");
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

        // One source mode should not match (death or unmatched)
        var unmatched = alignments.Where(a =>
            a.AlignmentType == "death" ||
            (a.AlignmentType == "matched" && !a.Metrics.IsMatch)).ToList();
        Assert.True(unmatched.Count >= 1, "Should detect at least one death/unmatched");
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
}

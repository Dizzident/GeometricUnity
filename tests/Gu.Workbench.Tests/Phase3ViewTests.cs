using Gu.Core;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.ModeTracking;
using Gu.Phase3.Registry;
using Gu.Phase3.Spectra;
using Gu.VulkanViewer;

namespace Gu.Workbench.Tests;

/// <summary>
/// Unit tests for the 8 Phase III diagnostic view payload types (GAP-2).
/// All tests construct minimal in-memory Phase III artifacts — no file I/O.
/// </summary>
public sealed class Phase3ViewTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helper builders
    // ─────────────────────────────────────────────────────────────────────────

    private static BackgroundRecord MakeBackgroundRecord(string id)
    {
        return new BackgroundRecord
        {
            BackgroundId = id,
            EnvironmentId = "env-test",
            BranchManifestId = "branch-test",
            GeometryFingerprint = "fp-abc",
            StateArtifactRef = "state/omega.json",
            ResidualNorm = 1e-6,
            StationarityNorm = 1e-7,
            AdmissibilityLevel = AdmissibilityLevel.B2,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 1e-6,
                StationarityNorm = 1e-7,
                ObjectiveValue = 5e-13,
                GaugeViolation = 1e-9,
                SolverIterations = 42,
                SolverConverged = true,
                TerminationReason = "residual_tolerance",
                GaussNewtonValid = true,
            },
            ReplayTierAchieved = "T1",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc123",
                Branch = new BranchRef { BranchId = "branch-test", SchemaVersion = "1.0" },
            },
        };
    }

    private static BackgroundAtlas MakeAtlas(int admittedCount = 2, int rejectedCount = 1)
    {
        var admitted = Enumerable.Range(0, admittedCount)
            .Select(i => MakeBackgroundRecord($"bg-{i:000}"))
            .ToList();
        var rejected = Enumerable.Range(0, rejectedCount)
            .Select(i => MakeBackgroundRecord($"bg-rej-{i:000}"))
            .ToList();

        return new BackgroundAtlas
        {
            AtlasId = "atlas-test",
            StudyId = "study-test",
            Backgrounds = admitted,
            RejectedBackgrounds = rejected,
            RankingCriteria = "residual",
            TotalAttempts = admittedCount + rejectedCount,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc123",
                Branch = new BranchRef { BranchId = "branch-test", SchemaVersion = "1.0" },
            },
            AdmissibilityCounts = new Dictionary<string, int> { ["B2"] = admittedCount, ["Rejected"] = rejectedCount },
        };
    }

    private static ModeRecord MakeMode(string modeId, string bgId, int index, double eigenvalue)
    {
        return new ModeRecord
        {
            ModeId = modeId,
            BackgroundId = bgId,
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = eigenvalue,
            ResidualNorm = 1e-9,
            NormalizationConvention = "unit-M-norm",
            GaugeLeakScore = 0.01,
            ModeVector = new[] { 0.5, -0.3, 0.8, 0.1 },
            ModeIndex = index,
            TensorEnergyFractions = new Dictionary<string, double> { ["connection-1form"] = 0.7, ["curvature-2form"] = 0.3 },
            BlockEnergyFractions = new Dictionary<string, double> { ["block-0"] = 0.6, ["block-1"] = 0.4 },
        };
    }

    private static SpectrumBundle MakeBundle(string bgId, int modeCount = 3)
    {
        var modes = Enumerable.Range(0, modeCount)
            .Select(i => MakeMode($"mode-{bgId}-{i}", bgId, i, (i + 1) * 1.5))
            .ToList();
        return new SpectrumBundle
        {
            SpectrumId = $"spec-{bgId}",
            BackgroundId = bgId,
            OperatorBundleId = "op-bundle-1",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            SolverMethod = "lanczos",
            StateDimension = 100,
            Modes = modes,
            Clusters = Array.Empty<ModeCluster>(),
            ConvergenceStatus = "converged",
            IterationsUsed = 30,
            MaxOrthogonalityDefect = 1e-12,
        };
    }

    private static ModeFamilyRecord MakeFamily(string familyId, int memberCount = 2)
    {
        var alignments = Enumerable.Range(0, memberCount - 1).Select(i =>
            new ModeAlignmentRecord
            {
                SourceModeId = $"mode-bg-{i}",
                TargetModeId = $"mode-bg-{i + 1}",
                Metrics = new ModeMatchMetricSet
                {
                    FeatureDistance = 0.05,
                    AggregateScore = 0.92,
                    IsMatch = true,
                },
                AlignmentType = "matched",
                Confidence = 0.92,
            }).ToList();

        return new ModeFamilyRecord
        {
            FamilyId = familyId,
            MemberModeIds = Enumerable.Range(0, memberCount).Select(i => $"mode-bg-{i}").ToList(),
            ContextIds = Enumerable.Range(0, memberCount).Select(i => $"bg-{i}").ToList(),
            MeanEigenvalue = 3.0,
            EigenvalueSpread = 0.2,
            IsStable = true,
            AmbiguityCount = 0,
            Alignments = alignments,
        };
    }

    private static BosonRegistry MakeRegistry(int candidateCount = 2)
    {
        var registry = new BosonRegistry();
        for (int i = 0; i < candidateCount; i++)
        {
            registry.Register(new CandidateBosonRecord
            {
                CandidateId = $"cand-{i:000}",
                PrimaryFamilyId = $"fam-{i:000}",
                ContributingModeIds = new[] { $"mode-bg-0-{i}" },
                BackgroundSet = new[] { "bg-000", "bg-001" },
                MassLikeEnvelope = new[] { 1.0, 1.5, 2.0 },
                MultiplicityEnvelope = new[] { 1, 1, 1 },
                GaugeLeakEnvelope = new[] { 0.005, 0.01, 0.015 },
                ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
                RegistryVersion = "1.0.0",
            });
        }
        return registry;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 1 — BackgroundAtlasBrowserView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void BackgroundAtlasBrowserView_FromAtlas_HasAdmittedEntries()
    {
        var atlas = MakeAtlas(admittedCount: 2, rejectedCount: 1);
        var view = BackgroundAtlasBrowserView.FromAtlas(atlas);

        Assert.Equal("atlas-test", view.AtlasId);
        Assert.Equal(2, view.Admitted.Count);
        Assert.Single(view.Rejected);
        Assert.All(view.Admitted, e => Assert.NotEmpty(e.BackgroundId));
    }

    [Fact]
    public void BackgroundAtlasBrowserView_Print_IsNonEmpty()
    {
        var atlas = MakeAtlas();
        var view = BackgroundAtlasBrowserView.FromAtlas(atlas);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("atlas-test", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 2 — SpectralLadderView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void SpectralLadderView_FromBundles_HasRungs()
    {
        var bundles = new[] { MakeBundle("bg-000", 3), MakeBundle("bg-001", 2) };
        var view = SpectralLadderView.FromBundles(bundles);

        Assert.Equal(5, view.Rungs.Count);
        Assert.All(view.Rungs, r => Assert.NotEmpty(r.ModeId));
    }

    [Fact]
    public void SpectralLadderView_Print_IsNonEmpty()
    {
        var bundles = new[] { MakeBundle("bg-000") };
        var view = SpectralLadderView.FromBundles(bundles);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("Spectral Ladder", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 3 — EigenModeAmplitudeView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void EigenModeAmplitudeView_FromBundles_HasEntries()
    {
        var bundles = new[] { MakeBundle("bg-000", 3) };
        var view = EigenModeAmplitudeView.FromBundles(bundles);

        Assert.Equal(3, view.Entries.Count);
        Assert.All(view.Entries, e =>
        {
            Assert.True(e.L2Norm > 0);
            Assert.True(e.VectorLength > 0);
        });
    }

    [Fact]
    public void EigenModeAmplitudeView_Print_IsNonEmpty()
    {
        var bundles = new[] { MakeBundle("bg-000") };
        var view = EigenModeAmplitudeView.FromBundles(bundles);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("Amplitude", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 4 — GaugeLeakView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GaugeLeakView_FromBundles_HasEntries()
    {
        var bundles = new[] { MakeBundle("bg-000", 4), MakeBundle("bg-001", 2) };
        var view = GaugeLeakView.FromBundles(bundles);

        Assert.Equal(2, view.Entries.Count);
        Assert.All(view.Entries, e =>
        {
            Assert.Equal("converged", e.ConvergenceStatus);
            Assert.True(e.ModeCount > 0);
        });
    }

    [Fact]
    public void GaugeLeakView_Print_IsNonEmpty()
    {
        var bundles = new[] { MakeBundle("bg-000") };
        var view = GaugeLeakView.FromBundles(bundles);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("Gauge Leak", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 5 — BranchModeTrackView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void BranchModeTrackView_FromFamilies_HasTracks()
    {
        var families = new[] { MakeFamily("fam-000", 3), MakeFamily("fam-001", 2) };
        var view = BranchModeTrackView.FromFamilies(families);

        Assert.Equal(2, view.Tracks.Count);
        Assert.All(view.Tracks, t => Assert.True(t.MemberCount > 0));
    }

    [Fact]
    public void BranchModeTrackView_Print_IsNonEmpty()
    {
        var families = new[] { MakeFamily("fam-000") };
        var view = BranchModeTrackView.FromFamilies(families);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("Branch Mode Tracking", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 6 — BosonFamilyCardView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void BosonFamilyCardView_FromRegistry_HasCards()
    {
        var registry = MakeRegistry(3);
        var view = BosonFamilyCardView.FromRegistry(registry);

        Assert.Equal(3, view.Cards.Count);
        Assert.All(view.Cards, c =>
        {
            Assert.NotEmpty(c.CandidateId);
            Assert.NotEmpty(c.ClaimClass);
            Assert.True(c.MassLikeScaleMean > 0);
        });
    }

    [Fact]
    public void BosonFamilyCardView_Print_IsNonEmpty()
    {
        var registry = MakeRegistry(2);
        var view = BosonFamilyCardView.FromRegistry(registry);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("Boson Family", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 7 — ObservedSignatureView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ObservedSignatureView_FromBundles_HasEntries()
    {
        var bundles = new[] { MakeBundle("bg-000", 3) };
        var view = ObservedSignatureView.FromBundles(bundles);

        Assert.Equal(3, view.Entries.Count);
        Assert.All(view.Entries, e =>
        {
            Assert.NotEmpty(e.ModeId);
            Assert.NotNull(e.TensorEnergyFractions);
            Assert.NotNull(e.BlockEnergyFractions);
        });
    }

    [Fact]
    public void ObservedSignatureView_Print_IsNonEmpty()
    {
        var bundles = new[] { MakeBundle("bg-000") };
        var view = ObservedSignatureView.FromBundles(bundles);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("Observed Mode Signatures", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 8 — AmbiguityHeatmapView
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AmbiguityHeatmapView_FromFamilies_HasCells()
    {
        var families = new[] { MakeFamily("fam-000", 3) };
        // All alignments have AggregateScore=0.92 > threshold 0.4
        var view = AmbiguityHeatmapView.FromFamilies(families, ambiguityThreshold: 0.4);

        Assert.True(view.Cells.Count > 0);
        Assert.All(view.Cells, c => Assert.True(c.AggregateScore >= 0.4));
    }

    [Fact]
    public void AmbiguityHeatmapView_HighThreshold_HasNoCells()
    {
        var families = new[] { MakeFamily("fam-000", 2) };
        var view = AmbiguityHeatmapView.FromFamilies(families, ambiguityThreshold: 0.99);

        Assert.Empty(view.Cells);
    }

    [Fact]
    public void AmbiguityHeatmapView_Print_IsNonEmpty()
    {
        var families = new[] { MakeFamily("fam-000", 3) };
        var view = AmbiguityHeatmapView.FromFamilies(families, 0.4);
        var output = view.Print();
        Assert.NotEmpty(output);
        Assert.Contains("Ambiguity Heatmap", output);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phase3ArtifactLoader.PreparePhase3Views integration test (in-memory)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Phase3ArtifactLoader_PrepareViews_FromSnapshot_ReturnsAllEightViews()
    {
        var snapshot = new Phase3WorkbenchSnapshot
        {
            RunFolderPath = "/tmp/test",
            BackgroundAtlas = MakeAtlas(),
            Spectra = new[] { MakeBundle("bg-000", 2) },
            ModeFamilies = new[] { MakeFamily("fam-000", 2) },
            BosonRegistry = MakeRegistry(2),
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var loader = new Phase3ArtifactLoader();
        var views = loader.PreparePhase3Views(snapshot);

        // BackgroundAtlasBrowserView + SpectralLadder + EigenModeAmplitude + GaugeLeak
        // + ObservedSignature + BranchModeTrack + AmbiguityHeatmap + BosonFamilyCard
        Assert.Equal(8, views.Count);
        Assert.Contains(views, v => v is BackgroundAtlasBrowserView);
        Assert.Contains(views, v => v is SpectralLadderView);
        Assert.Contains(views, v => v is EigenModeAmplitudeView);
        Assert.Contains(views, v => v is GaugeLeakView);
        Assert.Contains(views, v => v is BranchModeTrackView);
        Assert.Contains(views, v => v is BosonFamilyCardView);
        Assert.Contains(views, v => v is ObservedSignatureView);
        Assert.Contains(views, v => v is AmbiguityHeatmapView);
    }
}

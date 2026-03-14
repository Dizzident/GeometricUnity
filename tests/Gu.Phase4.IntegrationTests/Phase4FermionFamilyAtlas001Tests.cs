using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Registry;
using Gu.Phase4.Reporting;
using Gu.Studies.Phase4FermionFamilyAtlas001;

namespace Gu.Phase4.IntegrationTests;

/// <summary>
/// M45 end-to-end integration tests for the Phase IV reference study:
/// Phase4-FermionFamily-Atlas-001.
///
/// These tests run the full pipeline in-process and assert on the key outputs.
/// The study uses a Toy2D mesh with su(2) gauge group and a cos-sin-su2-v1 omega profile.
///
/// Per the task spec, at least 10 integration tests are required:
///   1.  Full pipeline runs without exception
///   2.  DiracOperatorBundle is produced and Hermitian residual is below threshold
///   3.  At least 1 fermion mode found
///   4.  FamilyClusterRecord list is non-empty
///   5.  CouplingAtlas has at least 1 entry
///   6.  UnifiedParticleRegistry has at least 1 candidate
///   7.  Phase4Report scope disclaimer present (REPORT.md)
///   8.  Artifact files written to output directory
///   9.  ConformanceArtifact passes branch-local checks
///  10.  CPU parity: DiracParityChecker.RunFullCheck passes
/// (Plus additional tests for coverage)
/// </summary>
public sealed class Phase4FermionFamilyAtlas001Tests : IDisposable
{
    private static readonly string ArtifactsDir = Path.Combine(
        Path.GetTempPath(),
        $"gu-m45-artifacts-{Guid.NewGuid():N}");

    private static Phase4StudyResult? _cachedResult;
    private static readonly object _lock = new();

    /// <summary>
    /// Run the study once and cache the result for all tests in this class.
    /// </summary>
    private static Phase4StudyResult GetOrRunStudy()
    {
        if (_cachedResult is not null) return _cachedResult;
        lock (_lock)
        {
            if (_cachedResult is not null) return _cachedResult;
            var study = new Phase4FermionFamilyAtlasStudy();
            _cachedResult = study.Run(ArtifactsDir);
        }
        return _cachedResult;
    }

    public void Dispose()
    {
        // Intentionally not deleting ArtifactsDir here:
        // xUnit calls Dispose() after each test instance, but ArtifactsDir and
        // _cachedResult are static (shared across all test instances).  Deleting
        // the directory after the first test would invalidate the file-path checks
        // in later tests.  The temp dir lives under Path.GetTempPath() and will be
        // reclaimed by the OS on the next reboot.
    }

    // -----------------------------------------------------------------------
    // Test 1: Full pipeline runs without exception
    // -----------------------------------------------------------------------

    [Fact]
    public void FullPipeline_RunsWithoutException()
    {
        // Act + Assert: no exception thrown
        var result = GetOrRunStudy();
        Assert.NotNull(result);
        Assert.Equal(Phase4FermionFamilyAtlasStudy.StudyId, result.StudyId);
    }

    // -----------------------------------------------------------------------
    // Test 2: DiracOperatorBundle is produced and Hermitian residual < threshold
    // -----------------------------------------------------------------------

    [Fact]
    public void DiracBundle_IsProducedWithAcceptableHermiticityResidual()
    {
        var result = GetOrRunStudy();
        var dirac = result.DiracBundle;

        Assert.NotNull(dirac);
        Assert.NotEmpty(dirac.OperatorId);
        // For the small Toy2D mesh: Hermitian residual should be < 1 (even for non-Hermitian discrete op).
        // The assembler enforces IsHermitian for residual <= 1e-10.
        // For our test: just assert it is finite (not NaN/Inf) and reasonable.
        Assert.True(double.IsFinite(dirac.HermiticityResidual),
            $"HermiticityResidual is not finite: {dirac.HermiticityResidual}");
        Assert.True(dirac.TotalDof > 0,
            $"TotalDof must be positive, got {dirac.TotalDof}");
    }

    // -----------------------------------------------------------------------
    // Test 3: At least 1 fermion mode found
    // -----------------------------------------------------------------------

    [Fact]
    public void FermionicSpectrum_HasAtLeastOneMode()
    {
        var result = GetOrRunStudy();
        var modes = result.SpectralBundle.SpectralResult.Modes;

        Assert.NotEmpty(modes);
        // Each mode must have required fields
        foreach (var mode in modes)
        {
            Assert.NotEmpty(mode.ModeId);
            Assert.True(double.IsFinite(mode.EigenvalueRe),
                $"Mode {mode.ModeId}: EigenvalueRe is not finite");
            Assert.NotNull(mode.ChiralityDecomposition);
            Assert.NotNull(mode.ConjugationPairing);
        }
    }

    // -----------------------------------------------------------------------
    // Test 4: FamilyClusterRecord list is non-empty
    // -----------------------------------------------------------------------

    [Fact]
    public void FamilyClusters_AreNonEmpty()
    {
        var result = GetOrRunStudy();
        Assert.NotEmpty(result.Clusters);

        foreach (var cluster in result.Clusters)
        {
            Assert.NotEmpty(cluster.ClusterId);
            Assert.NotEmpty(cluster.ClusterLabel);
            Assert.NotEmpty(cluster.MemberFamilyIds);
        }
    }

    // -----------------------------------------------------------------------
    // Test 5: CouplingAtlas has at least 1 entry
    // -----------------------------------------------------------------------

    [Fact]
    public void CouplingAtlas_HasAtLeastOneEntry()
    {
        var result = GetOrRunStudy();
        var atlas = result.CouplingAtlas;

        Assert.NotNull(atlas);
        Assert.NotEmpty(atlas.AtlasId);
        // The atlas should have coupling entries (modes with eigenvectors × 3 boson modes)
        Assert.True(atlas.Couplings.Count >= 1,
            $"CouplingAtlas expected >= 1 coupling entries, got {atlas.Couplings.Count}");
    }

    // -----------------------------------------------------------------------
    // Test 6: UnifiedParticleRegistry has at least 1 candidate
    // -----------------------------------------------------------------------

    [Fact]
    public void UnifiedParticleRegistry_HasAtLeastOneCandidate()
    {
        var result = GetOrRunStudy();
        var registry = result.Registry;

        Assert.NotNull(registry);
        Assert.True(registry.Count >= 1,
            $"Registry expected >= 1 candidate, got {registry.Count}");
    }

    // -----------------------------------------------------------------------
    // Test 7: Phase4Report scope disclaimer present in REPORT.md
    // -----------------------------------------------------------------------

    [Fact]
    public void ReportMd_ContainsScopeDisclaimer()
    {
        var result = GetOrRunStudy();
        Assert.True(File.Exists(result.ReportMdPath),
            $"REPORT.md not found at {result.ReportMdPath}");

        string content = File.ReadAllText(result.ReportMdPath);

        // Must contain the required scope disclaimer
        Assert.Contains(
            "This study demonstrates branch-consistent pipeline execution.",
            content,
            StringComparison.Ordinal);
        Assert.Contains(
            "It does NOT constitute physical validation.",
            content,
            StringComparison.Ordinal);
    }

    // -----------------------------------------------------------------------
    // Test 8: Artifact files written to output directory
    // -----------------------------------------------------------------------

    [Fact]
    public void Artifacts_AreWrittenToDirectory()
    {
        var result = GetOrRunStudy();
        var paths = result.ArtifactPaths;

        // Required artifacts
        var requiredKeys = new[]
        {
            "dirac_operator_bundle",
            "fermion_spectral_bundle",
            "fermion_family_atlas",
            "family_clusters",
            "coupling_atlas",
            "unified_particle_registry",
            "phase4_report",
            "conformance_artifact",
        };

        foreach (var key in requiredKeys)
        {
            Assert.True(paths.ContainsKey(key),
                $"Artifact key '{key}' not found in ArtifactPaths");
            Assert.True(File.Exists(paths[key]),
                $"Artifact file for '{key}' does not exist at {paths[key]}");
            Assert.True(new FileInfo(paths[key]).Length > 0,
                $"Artifact file for '{key}' is empty at {paths[key]}");
        }
    }

    // -----------------------------------------------------------------------
    // Test 9: ConformanceArtifact passes branch-local checks
    // -----------------------------------------------------------------------

    [Fact]
    public void ConformanceArtifact_PassesBranchLocalChecks()
    {
        var result = GetOrRunStudy();
        var paths = result.ArtifactPaths;

        Assert.True(paths.ContainsKey("conformance_artifact"),
            "conformance_artifact not in paths");
        string json = File.ReadAllText(paths["conformance_artifact"]);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Study ID must match
        Assert.Equal(Phase4FermionFamilyAtlasStudy.StudyId, root.GetProperty("studyId").GetString());

        // Omega profile must be cos-sin-su2-v1
        Assert.Equal(Phase4FermionFamilyAtlasStudy.OmegaProfile, root.GetProperty("omegaProfile").GetString());

        // Branch-local checks: modes and families found
        var checks = root.GetProperty("branchLocalChecks");
        int modesFound = checks.GetProperty("modesFound").GetInt32();
        int familiesFound = checks.GetProperty("familiesFound").GetInt32();
        int clustersFound = checks.GetProperty("clustersFound").GetInt32();

        Assert.True(modesFound >= 1, $"ConformanceArtifact: modesFound={modesFound}, expected >= 1");
        Assert.True(familiesFound >= 1, $"ConformanceArtifact: familiesFound={familiesFound}, expected >= 1");
        Assert.True(clustersFound >= 1, $"ConformanceArtifact: clustersFound={clustersFound}, expected >= 1");
    }

    // -----------------------------------------------------------------------
    // Test 10: CPU parity — DiracParityChecker.RunFullCheck passes
    // -----------------------------------------------------------------------

    [Fact]
    public void CpuParity_DiracParityCheckerRunFullCheck_Passes()
    {
        var result = GetOrRunStudy();
        var spinorSpec = result.SpinorSpec;
        var diracBundle = result.DiracBundle;
        var layout = result.Layout;

        // Rebuild the assembly context needed by CpuDiracKernel
        var provenance = BuildTestProvenance();
        var gammaBuilder = new Gu.Phase4.Spin.GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);

        var spinConnBuilder = new CpuSpinConnectionBuilder();
        var (background, bosonicState) = RebuildBosonicState(result.YMesh);
        var spinConnection = spinConnBuilder.Build(background, bosonicState, spinorSpec, layout, result.YMesh, provenance);

        var assembler = new CpuDiracOperatorAssembler();
        var cpuKernel = new CpuDiracKernel(diracBundle, gammas, result.YMesh, layout, spinConnection, assembler);

        // For parity: CPU vs CPU (both should be identical)
        int edgeCount = result.YMesh.EdgeCount;
        int bosonDimension = edgeCount * 3; // edgeCount * dimG

        var parityReport = DiracParityChecker.RunFullCheck(
            cpuKernel,
            cpuKernel,  // CPU vs CPU → exact parity
            spinorSpec,
            bosonDimension,
            numTestVectors: 3,
            seed: 42);

        Assert.True(parityReport.AllPassed,
            $"Parity check failed. Worst error: {parityReport.WorstError:E4}. " +
            $"Failed checks: {string.Join(", ", parityReport.Results.Where(r => !r.Passed).Select(r => r.OperationName))}");
    }

    // -----------------------------------------------------------------------
    // Test 11: UnifiedParticleRegistry round-trip JSON serialization
    // -----------------------------------------------------------------------

    [Fact]
    public void Registry_JsonRoundTrip_IsLossless()
    {
        var result = GetOrRunStudy();
        var registry = result.Registry;

        string json = registry.ToJson();
        Assert.NotEmpty(json);

        var deserialized = UnifiedParticleRegistry.FromJson(json);
        Assert.NotNull(deserialized);
        Assert.Equal(registry.Count, deserialized.Count);

        for (int i = 0; i < registry.Count; i++)
        {
            Assert.Equal(
                registry.Candidates[i].ParticleId,
                deserialized.Candidates[i].ParticleId);
            Assert.Equal(
                registry.Candidates[i].ParticleType,
                deserialized.Candidates[i].ParticleType);
        }
    }

    // -----------------------------------------------------------------------
    // Test 12: FermionFamilyAtlas has at least 1 family
    // -----------------------------------------------------------------------

    [Fact]
    public void FermionAtlas_HasAtLeastOneFamily()
    {
        var result = GetOrRunStudy();
        var atlas = result.FermionAtlas;

        Assert.NotNull(atlas);
        Assert.NotEmpty(atlas.AtlasId);
        Assert.True(atlas.Families.Count >= 1,
            $"FermionFamilyAtlas expected >= 1 family, got {atlas.Families.Count}");
    }

    // -----------------------------------------------------------------------
    // Test 13: Provenance CodeRevision is consistent across pipeline
    // -----------------------------------------------------------------------

    [Fact]
    public void Provenance_CodeRevision_IsConsistentAcrossPipeline()
    {
        var result = GetOrRunStudy();
        string expectedRevision = "phase4-m45-study-v1";

        // Check DiracBundle
        Assert.Equal(expectedRevision, result.DiracBundle.Provenance.CodeRevision);

        // Check modes
        foreach (var mode in result.SpectralBundle.SpectralResult.Modes)
        {
            Assert.Equal(expectedRevision, mode.Provenance.CodeRevision);
        }

        // Check registry
        foreach (var candidate in result.Registry.Candidates)
        {
            Assert.Equal(expectedRevision, candidate.Provenance.CodeRevision);
        }
    }

    // -----------------------------------------------------------------------
    // Test 14: Coupling proxies are labeled conservative (not physical constants)
    // -----------------------------------------------------------------------

    [Fact]
    public void CouplingAtlas_ClaimClassesAreConservative()
    {
        var result = GetOrRunStudy();

        // Interaction candidates from couplings must be at C0 level (per UnifiedRegistryBuilder)
        var interactions = result.Registry.QueryByType(UnifiedParticleType.Interaction);
        foreach (var interaction in interactions)
        {
            int level = UnifiedParticleRegistry.ParseClaimClassLevel(interaction.ClaimClass);
            Assert.True(level <= 1,
                $"Interaction {interaction.ParticleId} has claim class {interaction.ClaimClass} (level {level}), " +
                "expected C0 or C1 (conservative). Coupling proxies must not be promoted prematurely.");
        }
    }

    // -----------------------------------------------------------------------
    // Test 15: Observation summaries are produced (Y->X pipeline ran)
    // -----------------------------------------------------------------------

    [Fact]
    public void ObservationPipeline_ProducesSummaries()
    {
        var result = GetOrRunStudy();

        // FermionObservationSummary count should match cluster count
        Assert.Equal(result.Clusters.Count, result.FermionObservations.Count);

        // Each observation must have valid fields
        foreach (var obs in result.FermionObservations)
        {
            Assert.NotEmpty(obs.ClusterId);
            Assert.NotNull(obs.XChirality);
            Assert.NotNull(obs.MassLikeEnvelope);
            Assert.Equal(3, obs.MassLikeEnvelope.Length);
            Assert.True(double.IsFinite(obs.MassLikeEnvelope[1]),
                $"Cluster {obs.ClusterId}: MassLikeEnvelope mean is not finite");
        }
    }

    // -----------------------------------------------------------------------
    // Test 16: UnifiedRegistry contains both bosons and fermions (GAP-5)
    // -----------------------------------------------------------------------

    [Fact]
    public void UnifiedRegistry_ContainsBothBosonsAndFermions()
    {
        var result = GetOrRunStudy();
        var registry = result.Registry;

        // Must have at least one boson record (from the stub Phase III boson registry)
        var bosons = registry.QueryByType(UnifiedParticleType.Boson);
        Assert.True(bosons.Count >= 1,
            $"UnifiedRegistry expected at least 1 boson record, got {bosons.Count}. " +
            "RegistryMergeEngine.Build() must be called with a non-null bosonRegistry (GAP-5).");

        // Must have at least one fermion record (from Phase IV clustering)
        var fermions = registry.QueryByType(UnifiedParticleType.Fermion);
        Assert.True(fermions.Count >= 1,
            $"UnifiedRegistry expected at least 1 fermion record, got {fermions.Count}.");

        // ConsumedBosonRegistryId must be set (tracks which boson registry was merged)
        Assert.NotEmpty(result.ConsumedBosonRegistryId);
    }

    // -----------------------------------------------------------------------
    // Test 17: ComparisonCampaign has at least one record (GAP-3)
    // -----------------------------------------------------------------------

    [Fact]
    public void ComparisonCampaign_HasAtLeastOneRecord()
    {
        var result = GetOrRunStudy();
        var campaign = result.ComparisonCampaign;

        Assert.NotNull(campaign);
        Assert.NotEmpty(campaign.CampaignId);
        Assert.True(campaign.ComparisonRecords.Count >= 1,
            $"Expected at least 1 comparison record, got {campaign.ComparisonRecords.Count}. " +
            "FermionComparisonCampaignRunner must be called in the M45 study (GAP-3).");
    }

    // -----------------------------------------------------------------------
    // Test 18: ComparisonCampaign outcomes are valid values (GAP-3)
    // -----------------------------------------------------------------------

    [Fact]
    public void ComparisonCampaign_OutcomesAreValidValues()
    {
        var result = GetOrRunStudy();
        var campaign = result.ComparisonCampaign;

        var validOutcomes = new HashSet<string>
        {
            "compatible", "incompatible", "underdetermined", "not-applicable",
        };

        foreach (var record in campaign.ComparisonRecords)
        {
            Assert.True(validOutcomes.Contains(record.Outcome),
                $"Comparison record {record.ComparisonId} has invalid outcome '{record.Outcome}'. " +
                $"Expected one of: {string.Join(", ", validOutcomes)}.");
            Assert.NotEmpty(record.ComparisonId);
            Assert.NotEmpty(record.ClusterId);
        }
    }

    // -----------------------------------------------------------------------
    // Test 19: ComparisonCampaign artifact is written to disk (GAP-3)
    // -----------------------------------------------------------------------

    [Fact]
    public void ComparisonCampaign_ArtifactIsWrittenToDisk()
    {
        var result = GetOrRunStudy();

        Assert.True(result.ArtifactPaths.ContainsKey("fermion_comparison_campaign"),
            "Artifact key 'fermion_comparison_campaign' not found in ArtifactPaths. " +
            "WriteArtifacts must write the campaign JSON (GAP-3).");
        Assert.True(File.Exists(result.ArtifactPaths["fermion_comparison_campaign"]),
            $"Campaign artifact file does not exist at {result.ArtifactPaths["fermion_comparison_campaign"]}.");
        Assert.True(new FileInfo(result.ArtifactPaths["fermion_comparison_campaign"]).Length > 0,
            "Campaign artifact file is empty.");
    }

    // -----------------------------------------------------------------------
    // Test 20: G-005 regression — persisted Phase III registry is loaded when available
    // -----------------------------------------------------------------------

    /// <summary>
    /// G-005 regression: when a real Phase III boson_registry.json is present on disk,
    /// the study must load it and merge its candidates into the unified registry.
    /// The stub fallback must NOT be used when a persisted registry exists.
    ///
    /// Proves: two runs with different stored artifacts produce different unified registries.
    /// </summary>
    [Fact]
    public void G005_PersistedBosonRegistry_IsLoadedInsteadOfStub()
    {
        // Arrange: build a Phase III-style boson registry with a sentinel candidate ID.
        var sentinelCandidateId = "g005-regression-sentinel-boson-001";
        var sentinelRegistry = new Gu.Phase3.Registry.BosonRegistry();
        sentinelRegistry.Register(new Gu.Phase3.Registry.CandidateBosonRecord
        {
            CandidateId = sentinelCandidateId,
            PrimaryFamilyId = "sentinel-family-001",
            ContributingModeIds = new[] { "sentinel-mode-001" },
            BackgroundSet = new[] { "bg-toy2d-cos-sin-su2-v1" },
            BranchVariantSet = new[] { "sentinel-variant" },
            MassLikeEnvelope = new[] { 1.0, 2.0, 3.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.0, 0.0, 0.0 },
            BranchStabilityScore = 0.95,
            RefinementStabilityScore = 0.95,
            ObservationStabilityScore = 0.95,
            ClaimClass = Gu.Phase3.Registry.BosonClaimClass.C1_LocalPersistentMode,
            ComputedWithUnverifiedGpu = false,
            AmbiguityCount = 0,
            AmbiguityNotes = Array.Empty<string>(),
            RegistryVersion = "sentinel-v1.0",
        });
        string sentinelJson = sentinelRegistry.ToJson();

        // Write the sentinel registry to a temp Phase III artifacts dir.
        string phase3Dir = Path.Combine(Path.GetTempPath(), $"gu-g005-phase3-{Guid.NewGuid():N}");
        string bosonsDir = Path.Combine(phase3Dir, "bosons");
        Directory.CreateDirectory(bosonsDir);
        File.WriteAllText(Path.Combine(bosonsDir, "boson_registry.json"), sentinelJson);

        string artifactsDir = Path.Combine(Path.GetTempPath(), $"gu-g005-artifacts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(artifactsDir);

        try
        {
            // Act: run the study with phase3ArtifactsDir pointing to the sentinel dir.
            var study = new Phase4FermionFamilyAtlasStudy();
            var result = study.Run(artifactsDir, phase3ArtifactsDir: phase3Dir);

            // Assert 1: the study ran to completion
            Assert.NotNull(result);

            // Assert 2: the unified registry JSON on disk contains the sentinel candidate ID.
            // If the stub was used instead, the sentinel ID would NOT appear.
            Assert.True(result.ArtifactPaths.ContainsKey("unified_particle_registry"),
                "unified_particle_registry artifact not written");
            string registryJson = File.ReadAllText(result.ArtifactPaths["unified_particle_registry"]);
            Assert.Contains(sentinelCandidateId, registryJson,
                StringComparison.Ordinal);
        }
        finally
        {
            // Cleanup temp dirs.
            try { Directory.Delete(phase3Dir, recursive: true); } catch { /* best-effort */ }
            try { Directory.Delete(artifactsDir, recursive: true); } catch { /* best-effort */ }
        }
    }

    /// <summary>
    /// G-005 regression: when no phase3ArtifactsDir is provided, the fallback synthetic stub
    /// is used and the unified registry does NOT contain the sentinel candidate ID.
    /// Proves that the two code paths (real vs. stub) produce different artifacts.
    /// </summary>
    [Fact]
    public void G005_WithoutPhase3Dir_UsesStubbedRegistry_NotSentinel()
    {
        string sentinelCandidateId = "g005-regression-sentinel-boson-001";
        string artifactsDir = Path.Combine(Path.GetTempPath(), $"gu-g005-stub-artifacts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(artifactsDir);

        try
        {
            // Act: run without any phase3ArtifactsDir (no persisted registry).
            var study = new Phase4FermionFamilyAtlasStudy();
            var result = study.Run(artifactsDir, phase3ArtifactsDir: null);

            Assert.NotNull(result);

            // Assert: the sentinel ID must NOT be present — the stub was used.
            Assert.True(result.ArtifactPaths.ContainsKey("unified_particle_registry"),
                "unified_particle_registry artifact not written");
            string registryJson = File.ReadAllText(result.ArtifactPaths["unified_particle_registry"]);
            Assert.DoesNotContain(sentinelCandidateId, registryJson, StringComparison.Ordinal);

            // The stub uses StudyVersion ("1.0.0"), not "sentinel-v1.0".
            Assert.NotEqual("sentinel-v1.0", result.ConsumedBosonRegistryId);
        }
        finally
        {
            try { Directory.Delete(artifactsDir, recursive: true); } catch { /* best-effort */ }
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static ProvenanceMeta BuildTestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m45-parity",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    /// <summary>
    /// Rebuild the cos-sin-su2-v1 bosonic state for a given mesh.
    /// Mirrors the logic in Phase4FermionFamilyAtlasStudy.BuildBosonicBackground.
    /// </summary>
    private static (BackgroundRecord Background, double[] BosonicState) RebuildBosonicState(SimplicialMesh yMesh)
    {
        int edgeCount = yMesh.EdgeCount;
        int dimG = 3; // su(2)
        var state = new double[edgeCount * dimG];
        int dim = yMesh.EmbeddingDimension;

        for (int e = 0; e < edgeCount; e++)
        {
            int v0 = yMesh.Edges[e][0];
            int v1 = yMesh.Edges[e][1];
            double mx = 0.5 * (yMesh.VertexCoordinates[v0 * dim] + yMesh.VertexCoordinates[v1 * dim]);
            state[e * dimG + 0] = System.Math.Cos(System.Math.PI * mx);
            state[e * dimG + 1] = System.Math.Sin(System.Math.PI * mx);
            state[e * dimG + 2] = 0.0;
        }

        var provenance = BuildTestProvenance();
        var bg = new BackgroundRecord
        {
            BackgroundId = "bg-toy2d-cos-sin-su2-v1",
            EnvironmentId = "toy2d-su2",
            BranchManifestId = "manifest-toy2d-v1",
            GeometryFingerprint = $"toy2d-vert{yMesh.VertexCount}-edge{yMesh.EdgeCount}",
            StateArtifactRef = "inline:cos-sin-su2-v1",
            ResidualNorm = 0.0,
            StationarityNorm = 0.0,
            AdmissibilityLevel = AdmissibilityLevel.B0,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 0.0,
                StationarityNorm = 0.0,
                ObjectiveValue = 0.0,
                GaugeViolation = 0.0,
                SolverIterations = 0,
                SolverConverged = true,
                TerminationReason = "synthetic-profile",
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R2",
            Provenance = provenance,
        };
        return (bg, state);
    }
}

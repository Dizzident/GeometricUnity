using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.Registry;
using Gu.Phase4.Chirality;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Comparison;
using Gu.Phase4.Observation;
using Gu.Phase4.Registry;
using Gu.Phase4.Reporting;
using Gu.Phase4.Spin;

namespace Gu.Studies.Phase4FermionFamilyAtlas001;

/// <summary>
/// M45 end-to-end Phase IV reference study: Phase4-FermionFamily-Atlas-001.
///
/// Runs the full Phase IV fermionic pipeline in-process on a Toy2D mesh with su(2) gauge group:
///   1. Build Toy2D mesh (dimX=2, dimY=5, su(2))
///   2. Construct cos-sin-su2-v1 omega (edge-varying connection)
///   3. Build SpinConnectionBundle (CpuSpinConnectionBuilder)
///   4. Assemble DiracOperatorBundle (CpuDiracOperatorAssembler)
///   5. Solve fermionic spectrum (FermionSpectralBundleBuilder + FermionSpectralSolver)
///   6. Chirality analysis (ChiralityAnalyzer, ConjugationAnalyzer)
///   7. Track modes (FermionModeTracker, FermionFamilyAtlasBuilder)
///   8. Cluster families (FamilyClusteringEngine)
///   9. Build coupling atlas (CouplingProxyEngine + DiracVariationComputer)
///  10. Build registry (RegistryMergeEngine with stub boson registry — GAP-5)
///  11. Generate report (Phase4ReportGenerator)
///  12. Write artifacts to studies/phase4_fermion_family_atlas_001/artifacts/
///
/// IMPORTANT: This study demonstrates branch-consistent pipeline execution.
/// It does NOT constitute physical validation.
///
/// The cos-sin-su2-v1 omega profile ensures a spatially varying, non-constant bosonic
/// background, which guarantees nontrivial curvature in all su(2) directions.
/// A constant omega produces zero curvature and trivial Jacobian contributions.
/// </summary>
public sealed class Phase4FermionFamilyAtlasStudy
{
    public const string StudyId = "Phase4-FermionFamily-Atlas-001";
    public const string StudyVersion = "1.0.0";
    public const string OmegaProfile = "cos-sin-su2-v1";
    private const int DimY = 5;
    private const int DimX = 2;
    private const int DimG = 3; // su(2)
    private const int SpinorDim = 4; // 2^floor(5/2) = 4
    private const int ModeCount = 6;
    private const double Pi = System.Math.PI;

    // -----------------------------------------------------------------------
    // Public entry points
    // -----------------------------------------------------------------------

    /// <summary>
    /// Run the full pipeline and return the study result.
    /// </summary>
    /// <param name="artifactsDir">Output directory for artifacts.</param>
    /// <param name="phase3ArtifactsDir">
    /// Optional path to a Phase III run folder containing persisted bosonic spectra and registry.
    /// When provided, <see cref="BuildCouplingAtlas"/> uses real bosonic eigenvectors and
    /// <see cref="LoadOrBuildBosonRegistry"/> loads the persisted registry rather than a stub.
    /// G-005: primary path prefers real persisted Phase III artifacts over synthetic stand-ins.
    /// </param>
    public Phase4StudyResult Run(string artifactsDir, string? phase3ArtifactsDir = null)
    {
        ArgumentNullException.ThrowIfNull(artifactsDir);
        Directory.CreateDirectory(artifactsDir);

        var provenance = BuildProvenance();

        // Step 1: Build geometry
        var (yMesh, xMesh) = BuildGeometry();

        // Step 2: Build bosonic background record with cos-sin-su2-v1 omega
        var (background, bosonicState) = BuildBosonicBackground(yMesh, provenance);

        // Step 3: Build spinor spec, layout, gammas
        var spinorSpec = BuildSpinorSpec(provenance);
        var gammas = BuildGammaBundle(spinorSpec, provenance);
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-toy2d-su2-v1",
            spinorSpec,
            DimG,
            provenance,
            new List<string> { "P4-IA-003" });

        // Step 4: Build spin connection
        var spinConnBuilder = new CpuSpinConnectionBuilder();
        var spinConnection = spinConnBuilder.Build(background, bosonicState, spinorSpec, layout, yMesh, provenance);

        // Step 5: Assemble Dirac operator
        var diracAssembler = new CpuDiracOperatorAssembler();
        var diracBundle = diracAssembler.Assemble(spinConnection, gammas, layout, yMesh, provenance);

        // Step 6: Solve fermionic spectrum with chirality analysis baked in
        var spectralSolver = new FermionSpectralSolver(diracAssembler);
        var spectralConfig = new FermionSpectralConfig
        {
            TargetRegion = "lowest-magnitude",
            ModeCount = ModeCount,
            ConvergenceTolerance = 1e-8,
            MaxIterations = 2000,
            Seed = 42,
        };
        var chiralityConv = spinorSpec.ChiralityConvention;
        var conjugationConv = spinorSpec.ConjugationConvention;
        var spectralBundleBuilder = new FermionSpectralBundleBuilder();
        var spectralBundle = spectralBundleBuilder.SolveAndBuild(
            spectralSolver, diracBundle, layout, spectralConfig,
            gammas, chiralityConv, conjugationConv,
            yMesh.VertexCount, provenance);

        var modes = spectralBundle.SpectralResult.Modes;

        // Step 7: Build fermion family atlas.
        // G-005: Use a single authoritative context. Do NOT fabricate a perturbed duplicate.
        // Real branch sweeps would supply multiple contexts from distinct solved backgrounds.
        var namedResults = BuildNamedResults(modes, background.BackgroundId);
        var trackingConfig = new FermionTrackingConfig();
        var atlasBuilder = new FermionFamilyAtlasBuilder(trackingConfig);
        var fermionAtlas = atlasBuilder.Build(
            "fermion-atlas-001",
            "branch-family-toy2d-v1",
            namedResults,
            provenance);

        // Step 8: Cluster families
        var clusteringConfig = FamilyClusteringConfig.Default;
        var clusteringEngine = new FamilyClusteringEngine(clusteringConfig);
        var clusters = clusteringEngine.Cluster(fermionAtlas.Families, provenance);

        // Step 9: Build coupling atlas (G-005: pass phase3ArtifactsDir for real boson perturbations)
        var couplingAtlas = BuildCouplingAtlas(
            diracBundle, gammas, layout, yMesh, modes, background.BackgroundId, provenance,
            phase3ArtifactsDir);

        // Step 10: Observation pipeline (fermion clusters -> X_h summaries)
        // Run BEFORE registry so observation confidence feeds into claim class assignment.
        var observationPipeline = new FermionObservationPipeline();
        IReadOnlyList<Gu.Phase4.Observation.FermionObservationSummary> fermionObservations =
            observationPipeline.ObserveClusters(clusters, provenance);
        var interactionObservations = observationPipeline.ObserveInteractions(couplingAtlas, provenance);

        // Build observation confidence lookups for registry (GAP-2)
        var observationConfidenceByClusterId = fermionObservations
            .ToDictionary(obs => obs.ClusterId, obs => obs.BranchPersistenceScore);
        var interactionConfidenceByBosonModeId = interactionObservations
            .ToDictionary(obs => obs.BosonModeId, obs => obs.MeanCouplingMagnitude);

        // Step 11: Build unified registry using RegistryMergeEngine (GAP-5 fix).
        // G-005: primary path loads from persisted Phase III artifacts; falls back to stub with warning.
        var stubBosonRegistry = LoadOrBuildBosonRegistry(background.BackgroundId, provenance, phase3ArtifactsDir);
        var registryMergeConfig = RegistryMergeConfig.Default;
        var registryMergeEngine = new RegistryMergeEngine(registryMergeConfig);
        var registry = registryMergeEngine.Build(
            stubBosonRegistry,
            clusters,
            fermionAtlas,
            couplingAtlas,
            provenance,
            observationConfidenceByClusterId,
            interactionConfidenceByBosonModeId);

        // Step 12: Comparison campaign (GAP-3): compare observed fermion clusters against
        // placeholder references. Two references: one left-chiral, one right-chiral.
        // This fulfils M43 DoD: "at least one comparison campaign involving fermionic candidates."
        var candidateReferences = BuildCandidateReferences();
        var comparisonCampaign = FermionComparisonCampaignRunner.Run(
            campaignId: $"campaign-{StudyId}-v1",
            clusters: clusters,
            references: candidateReferences,
            massLikeScaleTolerance: 0.5,
            provenance: provenance);

        // Step 13: Generate report
        var reportGenerator = new Phase4ReportGenerator();
        var report = reportGenerator.Generate(
            StudyId,
            registry,
            fermionAtlas,
            couplingAtlas,
            provenance);

        // Step 14: Write artifacts
        var artifactPaths = WriteArtifacts(
            artifactsDir, diracBundle, spectralBundle, fermionAtlas,
            clusters, couplingAtlas, registry, comparisonCampaign, report, provenance);

        // Step 15: Write REPORT.md
        string reportMdPath = WriteReportMd(artifactsDir, report, fermionAtlas, registry, clusters);

        return new Phase4StudyResult
        {
            StudyId = StudyId,
            YMesh = yMesh,
            Background = background,
            SpinorSpec = spinorSpec,
            Layout = layout,
            DiracBundle = diracBundle,
            SpectralBundle = spectralBundle,
            FermionAtlas = fermionAtlas,
            Clusters = clusters,
            CouplingAtlas = couplingAtlas,
            Registry = registry,
            ConsumedBosonRegistryId = stubBosonRegistry.RegistryVersion,
            Report = report,
            ComparisonCampaign = comparisonCampaign,
            FermionObservations = fermionObservations.ToList<Gu.Phase4.Observation.FermionObservationSummary>(),
            InteractionObservations = interactionObservations.ToList(),
            ArtifactPaths = artifactPaths,
            ReportMdPath = reportMdPath,
        };
    }

    // -----------------------------------------------------------------------
    // Step implementations
    // -----------------------------------------------------------------------

    /// <summary>
    /// Build Toy2D fiber bundle mesh. Returns (yMesh, xMesh).
    /// The Dirac operator is assembled on Y_h (the full observerse mesh).
    /// </summary>
    private static (SimplicialMesh YMesh, SimplicialMesh XMesh) BuildGeometry()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        return (bundle.AmbientMesh, bundle.BaseMesh);
    }

    /// <summary>
    /// Build the bosonic background record and the cos-sin-su2-v1 omega state vector.
    ///
    /// omega_e = cos(pi * mx_e) * T_1 + sin(pi * mx_e) * T_2
    ///
    /// where mx_e is the x-coordinate of the midpoint of edge e.
    /// T_1, T_2, T_3 are the three su(2) generators.
    ///
    /// This profile is spatially varying and non-constant, guaranteeing nontrivial
    /// curvature [T_1, T_2] = i*T_3 in all su(2) directions.
    /// </summary>
    private static (BackgroundRecord Background, double[] BosonicState) BuildBosonicBackground(
        SimplicialMesh yMesh,
        ProvenanceMeta provenance)
    {
        int edgeCount = yMesh.EdgeCount;
        var state = new double[edgeCount * DimG];

        // Build the cos-sin-su2-v1 connection:
        // omega_e^0 = cos(pi * mx_e), omega_e^1 = sin(pi * mx_e), omega_e^2 = 0
        for (int e = 0; e < edgeCount; e++)
        {
            int v0 = yMesh.Edges[e][0];
            int v1 = yMesh.Edges[e][1];
            int dim = yMesh.EmbeddingDimension;

            // Midpoint x-coordinate (index 0)
            double mx = 0.5 * (yMesh.VertexCoordinates[v0 * dim] + yMesh.VertexCoordinates[v1 * dim]);

            state[e * DimG + 0] = System.Math.Cos(Pi * mx);  // T_1 component
            state[e * DimG + 1] = System.Math.Sin(Pi * mx);  // T_2 component
            state[e * DimG + 2] = 0.0;                        // T_3 component
        }

        // Build a minimal BackgroundRecord for use with CpuSpinConnectionBuilder.
        // This is a synthetic background (the full bosonic solve is not reproduced
        // in this in-process study; the cos-sin profile is provided directly).
        var background = new BackgroundRecord
        {
            BackgroundId = "bg-toy2d-cos-sin-su2-v1",
            EnvironmentId = "toy2d-su2",
            BranchManifestId = "manifest-toy2d-v1",
            GeometryFingerprint = $"toy2d-vert{yMesh.VertexCount}-edge{yMesh.EdgeCount}",
            StateArtifactRef = "inline:cos-sin-su2-v1",
            ResidualNorm = 0.0,   // synthetic — not solved
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

        return (background, state);
    }

    /// <summary>
    /// Build SpinorRepresentationSpec for Cl(5,0), dimY=5, spinorDim=4.
    ///
    /// Per ARCH_P4.md §11.3: odd dimY => no Y-chirality; but X-chirality (dimX=2) is available.
    /// GammaConvention.HasChirality = false for the full 5D spec (odd dimension).
    /// ChiralityConvention.HasChirality = false.
    /// </summary>
    private static SpinorRepresentationSpec BuildSpinorSpec(ProvenanceMeta provenance)
    {
        // dimY=5 is odd => no Y-space chirality operator
        // dimX=2 is even => X-chirality exists (but we use dimY=5 gamma matrices)
        var sig = new CliffordSignature { Positive = DimY, Negative = 0 };

        return new SpinorRepresentationSpec
        {
            SpinorSpecId = "spinor-cl5-riem-v1",
            SpacetimeDimension = DimY,
            CliffordSignature = sig,
            GammaConvention = new GammaConventionSpec
            {
                ConventionId = "dirac-tensor-product-v1",
                Signature = sig,
                Representation = "standard",
                SpinorDimension = SpinorDim,
                HasChirality = false,  // odd dimY: no chirality matrix
            },
            ChiralityConvention = new ChiralityConventionSpec
            {
                ConventionId = "chirality-trivial-v1",
                SignConvention = "left-is-minus",
                PhaseFactor = "trivial",
                HasChirality = false,   // odd dimY: trivial chirality
                FullChiralityOperator = null,
                BaseChiralityOperator = "X-chirality",  // dimX=2 is even
                FiberChiralityOperator = null,
                BaseDimension = DimX,
                FiberDimension = DimY - DimX,
            },
            ConjugationConvention = new ConjugationConventionSpec
            {
                ConventionId = "hermitian-v1",
                ConjugationType = "hermitian",
                HasChargeConjugation = true,
            },
            SpinorComponents = SpinorDim,
            ChiralitySplit = 0,  // odd dim: no chirality split
            InsertedAssumptionIds = new List<string> { "P4-IA-003", "P4-IA-001" },
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Build GammaOperatorBundle for Cl(5,0) using GammaMatrixBuilder.
    /// </summary>
    private static GammaOperatorBundle BuildGammaBundle(
        SpinorRepresentationSpec spec,
        ProvenanceMeta provenance)
    {
        var builder = new GammaMatrixBuilder();
        return builder.Build(spec.CliffordSignature, spec.GammaConvention, provenance);
    }

    /// <summary>
    /// Build NamedSpectralResult list for atlas construction.
    ///
    /// G-005: Returns a single authoritative context from the primary solve.
    /// Do NOT fabricate a perturbed duplicate as a stand-in for a second branch variant.
    /// Real multi-context atlases must be driven by distinct solved backgrounds.
    /// </summary>
    private static List<NamedSpectralResult> BuildNamedResults(
        IReadOnlyList<FermionModeRecord> modes,
        string backgroundId)
    {
        var result1 = new NamedSpectralResult
        {
            ContextId = "ctx-primary",
            BackgroundId = backgroundId,
            Modes = modes,
        };

        // G-005: Single-context is the correct primary path when no real second branch
        // variant is available. Do NOT add a fabricated perturbed duplicate.
        return new List<NamedSpectralResult> { result1 };
    }

    /// <summary>
    /// Build the boson-fermion coupling atlas using analytical Dirac variation.
    ///
    /// For this study, the "boson modes" are represented by a synthetic set of
    /// delta-omega perturbations (one per spatial direction × gauge component).
    /// In a full Phase IV study they would come from Phase III bosonic spectral output.
    ///
    /// The coupling proxy g_ijk = &lt;phi_i^†, delta_D[b_k] phi_j&gt; is computed
    /// for all (mode_i, mode_j, boson_k) triples where modes have eigenvectors.
    /// </summary>
    private static CouplingAtlas BuildCouplingAtlas(
        DiracOperatorBundle diracBundle,
        GammaOperatorBundle gammas,
        FermionFieldLayout layout,
        SimplicialMesh yMesh,
        IReadOnlyList<FermionModeRecord> modes,
        string backgroundId,
        ProvenanceMeta provenance,
        string? phase3ArtifactsDir = null)
    {
        int edgeCount = yMesh.EdgeCount;
        int vertexCount = yMesh.VertexCount;
        int dim = yMesh.EmbeddingDimension;

        // Build edge geometry arrays needed by DiracVariationComputer
        var edgeLengths = new double[edgeCount];
        var edgeDirections = new double[edgeCount][];
        // cellsPerEdge: in vertex-based Dirac, each edge connects two vertices (endpoints)
        var cellsPerEdge = new int[edgeCount][];
        for (int e = 0; e < edgeCount; e++)
        {
            int v0 = yMesh.Edges[e][0];
            int v1 = yMesh.Edges[e][1];
            edgeLengths[e] = ComputeEdgeLength(yMesh, e);
            edgeDirections[e] = ComputeEdgeDirection(yMesh, e);
            cellsPerEdge[e] = new[] { v0, v1 };
        }

        var modesWithVectors = modes.Where(m => m.EigenvectorCoefficients != null).ToList();
        if (modesWithVectors.Count == 0)
        {
            // If no eigenvectors are available, return an empty atlas
            return new CouplingAtlas
            {
                AtlasId = "coupling-atlas-001-empty",
                FermionBackgroundId = backgroundId,
                BosonRegistryVersion = "1.0.0",
                Couplings = new List<BosonFermionCouplingRecord>(),
                NormalizationConvention = "unit-modes",
                Provenance = provenance,
            };
        }

        int spinorDim = gammas.SpinorDimension;
        var variationMatrices = new Dictionary<string, (double[,] Re, double[,]? Im)>();

        // G-005: Primary path — try to load real bosonic spectral eigenvectors from Phase III.
        // Each bosonic mode eigenvector lives in connection space (edgeCount * dimG) and
        // represents a real perturbation direction from the solved bosonic background.
        bool loadedRealBosons = TryLoadBosonPerturbations(
            phase3ArtifactsDir, edgeCount, DimG, gammas, vertexCount, spinorDim,
            edgeLengths, cellsPerEdge, edgeDirections, variationMatrices);

        if (!loadedRealBosons)
        {
            // G-005 fallback: use synthetic delta-omega perturbations.
            // These are NOT real bosonic modes. Labeled with "-synthetic-" prefix.
            Console.WriteLine(
                "  G-005 WARNING: No real Phase III bosonic spectral data found. " +
                "Coupling atlas will use synthetic perturbation directions as fallback. " +
                "Provide phase3ArtifactsDir pointing to a Phase III run folder for real results.");
            for (int bIdx = 0; bIdx < 3; bIdx++)
            {
                var deltaOmega = new double[edgeCount * DimG];
                for (int e = 0; e < edgeCount; e++)
                {
                    int v0 = yMesh.Edges[e][0];
                    double mx = yMesh.VertexCoordinates[v0 * dim];
                    if (bIdx == 0)
                        deltaOmega[e * DimG + 0] = System.Math.Cos(Pi * mx);
                    else if (bIdx == 1)
                        deltaOmega[e * DimG + 1] = System.Math.Sin(Pi * mx);
                    else
                        deltaOmega[e * DimG + 2] = 1.0;
                }

                var (reVar, imVar) = DiracVariationComputer.ComputeAnalytical(
                    deltaOmega, gammas, vertexCount, spinorDim, DimG,
                    edgeLengths, cellsPerEdge, edgeDirections);

                variationMatrices[$"boson-synthetic-{bIdx:D2}"] = (reVar, imVar);
            }
        }

        var couplingEngine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());
        return couplingEngine.BuildAtlas(
            "coupling-atlas-001",
            backgroundId,
            modesWithVectors,
            variationMatrices,
            "unit-modes",
            "1.0.0",
            provenance);
    }

    /// <summary>
    /// Try to load real bosonic eigenvectors from Phase III spectra/ directory.
    /// Returns true if at least one real bosonic perturbation was loaded.
    /// </summary>
    private static bool TryLoadBosonPerturbations(
        string? phase3ArtifactsDir,
        int edgeCount,
        int dimG,
        GammaOperatorBundle gammas,
        int vertexCount,
        int spinorDim,
        double[] edgeLengths,
        int[][] cellsPerEdge,
        double[][] edgeDirections,
        Dictionary<string, (double[,] Re, double[,]? Im)> variationMatrices)
    {
        if (string.IsNullOrEmpty(phase3ArtifactsDir))
            return false;

        var spectraDir = Path.Combine(phase3ArtifactsDir, "spectra");
        if (!Directory.Exists(spectraDir))
            return false;

        bool loaded = false;
        foreach (var specFile in Directory.EnumerateFiles(spectraDir, "*.json").Take(3))
        {
            try
            {
                var specJson = File.ReadAllText(specFile);
                using var doc = JsonDocument.Parse(specJson);
                if (!doc.RootElement.TryGetProperty("modes", out var modesEl))
                    continue;
                foreach (var modeEl in modesEl.EnumerateArray().Take(3))
                {
                    var arr = TryReadBosonModeVector(modeEl);
                    if (arr is null || arr.Length != edgeCount * dimG)
                        continue;
                    var modeId = modeEl.TryGetProperty("modeId", out var midEl)
                        ? midEl.GetString() ?? "boson-real-mode"
                        : "boson-real-mode";
                    var (reVar, imVar) = DiracVariationComputer.ComputeAnalytical(
                        arr, gammas, vertexCount, spinorDim, dimG,
                        edgeLengths, cellsPerEdge, edgeDirections);
                    variationMatrices[modeId] = (reVar, imVar);
                    loaded = true;
                }
            }
            catch (Exception)
            {
                // Skip unreadable spectral files.
            }
        }
        return loaded;
    }

    private static double[]? TryReadBosonModeVector(JsonElement modeEl)
    {
        if (modeEl.TryGetProperty("modeVector", out var modeVectorEl))
            return modeVectorEl.Deserialize<double[]>();
        if (modeEl.TryGetProperty("eigenvectorCoefficients", out var eigenvectorEl))
            return eigenvectorEl.Deserialize<double[]>();
        return null;
    }

    /// <summary>
    /// Load a Phase III boson registry from persisted artifacts, or build a minimal
    /// synthetic stub when no persisted data is available (G-005 fix).
    ///
    /// Primary path: loads from &lt;phase3ArtifactsDir&gt;/bosons/boson_registry.json.
    /// Fallback: builds one synthetic C1-class record with an explicit warning.
    /// </summary>
    private static BosonRegistry LoadOrBuildBosonRegistry(
        string backgroundId,
        ProvenanceMeta provenance,
        string? phase3ArtifactsDir)
    {
        // G-005: Primary path — try to load from persisted Phase III artifacts.
        if (!string.IsNullOrEmpty(phase3ArtifactsDir))
        {
            var registryPath = Path.Combine(phase3ArtifactsDir, "bosons", "boson_registry.json");
            if (File.Exists(registryPath))
            {
                try
                {
                    var loaded = BosonRegistry.FromJson(File.ReadAllText(registryPath));
                    Console.WriteLine(
                        $"  G-005: Loaded real Phase III boson registry from {registryPath} " +
                        $"({loaded.Candidates.Count} candidates).");
                    return loaded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"  G-005 WARNING: Failed to load boson registry from {registryPath}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine(
                    $"  G-005 WARNING: No boson_registry.json at {registryPath}. " +
                    "Using synthetic stub. Run 'build-boson-registry' on a Phase III run folder first.");
            }
        }
        else
        {
            Console.WriteLine(
                "  G-005 WARNING: No phase3ArtifactsDir provided. " +
                "Unified registry will use a synthetic stub boson record as fallback. " +
                "Pass a Phase III run folder path to use real persisted bosons.");
        }

        // Fallback: build minimal synthetic stub.
        var stubBoson = new CandidateBosonRecord
        {
            CandidateId = $"stub-boson-{backgroundId}-001",
            PrimaryFamilyId = $"stub-family-{backgroundId}-001",
            ContributingModeIds = new[] { $"stub-mode-{backgroundId}-001" },
            BackgroundSet = new[] { backgroundId },
            BranchVariantSet = new[] { "stub-variant" },
            MassLikeEnvelope = new[] { 0.5, 1.0, 1.5 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.0, 0.0, 0.0 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            ObservationStabilityScore = 0.9,
            ClaimClass = Gu.Phase3.Registry.BosonClaimClass.C1_LocalPersistentMode,
            ComputedWithUnverifiedGpu = false,
            AmbiguityCount = 0,
            AmbiguityNotes = Array.Empty<string>(),
            RegistryVersion = StudyVersion,
        };

        var registry = new BosonRegistry();
        registry.Register(stubBoson);
        return registry;
    }

    /// <summary>
    /// Build placeholder FermionCandidateReference records for the comparison campaign (GAP-3).
    ///
    /// These are intentionally minimal placeholder references — NOT real physical particle data.
    /// One left-chiral reference and one right-chiral reference are provided to ensure the
    /// comparison pipeline runs for a variety of cluster chiralities.
    ///
    /// MassLikeEnvelope is null so these always produce "underdetermined" outcomes unless
    /// chirality explicitly matches, keeping the comparison conservative.
    /// </summary>
    private static IReadOnlyList<FermionCandidateReference> BuildCandidateReferences()
    {
        return new List<FermionCandidateReference>
        {
            new FermionCandidateReference
            {
                ReferenceId = "ref-left-chiral-cluster-placeholder-001",
                Label = "placeholder-left-chiral-cluster",
                ExpectedChirality = "left",
                ExpectedMassLikeEnvelope = null, // unconstrained → underdetermined outcome
                ExpectsConjugatePair = false,
                Notes = new List<string>
                {
                    "Placeholder reference for testing comparison pipeline (GAP-3).",
                    "NOT a physical particle reference — no mass-like scale constraint applied.",
                },
            },
            new FermionCandidateReference
            {
                ReferenceId = "ref-right-chiral-cluster-placeholder-001",
                Label = "placeholder-right-chiral-cluster",
                ExpectedChirality = "right",
                ExpectedMassLikeEnvelope = null, // unconstrained → underdetermined outcome
                ExpectsConjugatePair = false,
                Notes = new List<string>
                {
                    "Placeholder reference for testing comparison pipeline (GAP-3).",
                    "NOT a physical particle reference — no mass-like scale constraint applied.",
                },
            },
        };
    }

    // -----------------------------------------------------------------------
    // Artifact writing
    // -----------------------------------------------------------------------

    /// <summary>
    /// Write all pipeline artifacts to the output directory.
    /// Returns a dictionary of artifact name -> file path.
    /// </summary>
    private static Dictionary<string, string> WriteArtifacts(
        string artifactsDir,
        DiracOperatorBundle diracBundle,
        FermionSpectralBundle spectralBundle,
        FermionFamilyAtlas fermionAtlas,
        IReadOnlyList<FamilyClusterRecord> clusters,
        CouplingAtlas couplingAtlas,
        UnifiedParticleRegistry registry,
        FermionComparisonCampaign comparisonCampaign,
        Phase4Report report,
        ProvenanceMeta provenance)
    {
        var opts = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        var paths = new Dictionary<string, string>();

        void WriteJson(string fileName, object value)
        {
            string path = Path.Combine(artifactsDir, fileName);
            File.WriteAllText(path, JsonSerializer.Serialize(value, opts));
            paths[Path.GetFileNameWithoutExtension(fileName)] = path;
        }

        // Dirac operator summary (not the full matrix — too large for artifact)
        WriteJson("dirac_operator_bundle.json", new
        {
            operatorId = diracBundle.OperatorId,
            fermionBackgroundId = diracBundle.FermionBackgroundId,
            matrixShape = diracBundle.MatrixShape,
            isHermitian = diracBundle.IsHermitian,
            hermiticityResidual = diracBundle.HermiticityResidual,
            totalDof = diracBundle.TotalDof,
            hasExplicitMatrix = diracBundle.HasExplicitMatrix,
        });

        // Spectral result summary (eigenvectors excluded from JSON for size)
        WriteJson("fermion_spectral_bundle.json", new
        {
            bundleId = spectralBundle.BundleId,
            fermionBackgroundId = spectralBundle.FermionBackgroundId,
            modeCount = spectralBundle.SpectralResult.Modes.Count,
            chiralityAnalysisAvailable = spectralBundle.ChiralityAnalysisAvailable,
            conjugationAnalysisApplied = spectralBundle.ConjugationAnalysisApplied,
            gaugeLeakSummary = spectralBundle.GaugeLeakSummary,
            modes = spectralBundle.SpectralResult.Modes.Select(m => new
            {
                m.ModeId,
                m.EigenvalueRe,
                m.EigenvalueIm,
                m.ResidualNorm,
                m.ChiralityDecomposition,
                m.ConjugationPairing,
                m.GaugeLeakScore,
            }).ToList(),
        });

        WriteJson("fermion_family_atlas.json", new
        {
            atlasId = fermionAtlas.AtlasId,
            branchFamilyId = fermionAtlas.BranchFamilyId,
            totalFamilies = fermionAtlas.Families.Count,
            summary = fermionAtlas.Summary,
            families = fermionAtlas.Families.Select(f => new
            {
                f.FamilyId,
                f.EigenvalueMagnitudeEnvelope,
                f.DominantChiralityProfile,
                f.HasConjugationPair,
                f.ConjugateFamilyId,
                f.BranchPersistenceScore,
                f.MemberModeIds,
            }).ToList(),
        });

        WriteJson("family_clusters.json", new
        {
            totalClusters = clusters.Count,
            clusters = clusters.Select(c => new
            {
                c.ClusterId,
                c.ClusterLabel,
                c.MemberFamilyIds,
                c.DominantChirality,
                c.HasConjugatePair,
                c.EigenvalueMagnitudeEnvelope,
                c.AmbiguityScore,
                c.MeanBranchPersistence,
                c.ClusteringMethod,
            }).ToList(),
        });

        WriteJson("coupling_atlas.json", new
        {
            atlasId = couplingAtlas.AtlasId,
            fermionBackgroundId = couplingAtlas.FermionBackgroundId,
            normalizationConvention = couplingAtlas.NormalizationConvention,
            couplingCount = couplingAtlas.Couplings.Count,
            nonzeroCouplings = couplingAtlas.Couplings.Count(c => c.CouplingProxyMagnitude > 1e-12),
            maxMagnitude = couplingAtlas.Couplings.Count > 0
                ? couplingAtlas.Couplings.Max(c => c.CouplingProxyMagnitude)
                : 0.0,
            topCouplings = couplingAtlas.Couplings
                .OrderByDescending(c => c.CouplingProxyMagnitude)
                .Take(10)
                .Select(c => new
                {
                    c.CouplingId,
                    c.BosonModeId,
                    c.FermionModeIdI,
                    c.FermionModeIdJ,
                    c.CouplingProxyMagnitude,
                }).ToList(),
        });

        WriteJson("unified_particle_registry.json", registry);

        WriteJson("fermion_comparison_campaign.json", comparisonCampaign);

        WriteJson("phase4_report.json", report);

        // Conformance artifact
        WriteJson("conformance_artifact.json", new
        {
            studyId = StudyId,
            studyVersion = StudyVersion,
            omegaProfile = OmegaProfile,
            pipelineStagesCompleted = new[]
            {
                "mesh-construction",
                "bosonic-background",
                "spinor-spec",
                "spin-connection",
                "dirac-operator",
                "fermionic-spectrum",
                "chirality-analysis",
                "fermion-family-atlas",
                "family-clustering",
                "coupling-atlas",
                "observation-pipeline",
                "unified-registry",
                "comparison-campaign",
                "report-generation",
            },
            branchLocalChecks = new
            {
                diracIsHermitian = diracBundle.IsHermitian,
                hermiticityResidual = diracBundle.HermiticityResidual,
                modesFound = spectralBundle.SpectralResult.Modes.Count,
                familiesFound = fermionAtlas.Families.Count,
                clustersFound = clusters.Count,
                couplingEntriesFound = couplingAtlas.Couplings.Count,
                registryCandidates = registry.Count,
                comparisonRecordCount = comparisonCampaign.ComparisonRecords.Count,
            },
            provenanceCodeRevision = provenance.CodeRevision,
            generatedAt = DateTimeOffset.UtcNow,
        });

        return paths;
    }

    /// <summary>
    /// Write the study REPORT.md with the required scope disclaimer.
    /// </summary>
    private static string WriteReportMd(
        string artifactsDir,
        Phase4Report report,
        FermionFamilyAtlas fermionAtlas,
        UnifiedParticleRegistry registry,
        IReadOnlyList<FamilyClusterRecord> clusters)
    {
        string path = Path.Combine(artifactsDir, "REPORT.md");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Phase IV Reference Study Report: {StudyId}");
        sb.AppendLine();
        sb.AppendLine("## Scope Disclaimer");
        sb.AppendLine();
        sb.AppendLine("> **This study demonstrates branch-consistent pipeline execution. " +
                      "It does NOT constitute physical validation.**");
        sb.AppendLine();
        sb.AppendLine("The results below are candidate outputs from a discrete computation on a Toy2D mesh " +
                      "with su(2) gauge group. No claims are made about their correspondence to physical " +
                      "particles, masses, or Standard Model representations. All candidates carry explicit " +
                      "claim classes (C0–C5) that reflect the degree of evidence, not confirmed physics.");
        sb.AppendLine();
        sb.AppendLine("## Study Parameters");
        sb.AppendLine();
        sb.AppendLine($"- **Study ID:** {StudyId}");
        sb.AppendLine($"- **Omega profile:** `{OmegaProfile}`");
        sb.AppendLine($"- **dimY:** {DimY} (odd → no Y-space chirality)");
        sb.AppendLine($"- **dimX:** {DimX} (even → X-chirality available)");
        sb.AppendLine($"- **Gauge group:** su(2), dimG={DimG}");
        sb.AppendLine($"- **Spinor dimension:** {SpinorDim} (Cl(5,0))");
        sb.AppendLine($"- **Report generated:** {report.GeneratedAt:O}");
        sb.AppendLine();
        sb.AppendLine("## Pipeline Results");
        sb.AppendLine();
        sb.AppendLine("### Fermionic Spectrum");
        sb.AppendLine($"- Modes solved: {fermionAtlas.Summary.TotalFamilies}");
        sb.AppendLine($"- Families with conjugation pair: {fermionAtlas.Summary.FamiliesWithConjugatePair}");
        sb.AppendLine($"- Mean branch persistence: {fermionAtlas.Summary.MeanBranchPersistence:F3}");
        sb.AppendLine();
        sb.AppendLine("### Family Clusters");
        sb.AppendLine($"- Clusters found: {clusters.Count}");
        foreach (var c in clusters)
        {
            sb.AppendLine($"  - `{c.ClusterLabel}` ({c.ClusteringMethod}): " +
                          $"chirality={c.DominantChirality}, conjugatePair={c.HasConjugatePair}");
        }
        sb.AppendLine();
        sb.AppendLine("### Unified Registry");
        sb.AppendLine($"- Total candidates: {registry.Count}");
        sb.AppendLine($"- Fermions: {registry.QueryByType(UnifiedParticleType.Fermion).Count}");
        sb.AppendLine($"- Interactions: {registry.QueryByType(UnifiedParticleType.Interaction).Count}");
        sb.AppendLine();
        sb.AppendLine("## Negative Results");
        sb.AppendLine();
        if (report.NegativeResults.Count == 0)
        {
            sb.AppendLine("No demotions or ambiguities recorded.");
        }
        else
        {
            foreach (var n in report.NegativeResults.Take(20))
            {
                sb.AppendLine($"- {n}");
            }
        }
        sb.AppendLine();
        sb.AppendLine("## Physical Interpretation Disclaimer");
        sb.AppendLine();
        sb.AppendLine("The chirality labels (trivial/mixed) arise because dim(Y)=5 is **odd**: " +
                      "the Clifford algebra Cl(5,0) does not admit a nontrivial chirality operator. " +
                      "Chirality on X (dim(X)=2, even) is in principle available but requires a " +
                      "full X-space chirality analysis that is beyond this in-process study.");
        sb.AppendLine();
        sb.AppendLine("Coupling proxies are computed via finite-difference variation of the Dirac operator. " +
                      "They are NOT physical coupling constants, scattering amplitudes, or measured quantities.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine($"*Generated by {StudyId} v{StudyVersion}*");

        File.WriteAllText(path, sb.ToString());
        return path;
    }

    // -----------------------------------------------------------------------
    // Provenance
    // -----------------------------------------------------------------------

    private static ProvenanceMeta BuildProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase4-m45-study-v1",
        Branch = new BranchRef
        {
            BranchId = "phase4-fermion-atlas-001",
            SchemaVersion = "1.0.0",
        },
        Backend = "cpu-reference",
    };

    private static double ComputeEdgeLength(SimplicialMesh mesh, int edgeIdx)
    {
        int v0 = mesh.Edges[edgeIdx][0];
        int v1 = mesh.Edges[edgeIdx][1];
        var coords0 = mesh.GetVertexCoordinates(v0);
        var coords1 = mesh.GetVertexCoordinates(v1);
        double norm = 0.0;
        for (int k = 0; k < coords0.Length; k++)
        {
            double d = coords1[k] - coords0[k];
            norm += d * d;
        }
        return System.Math.Sqrt(norm);
    }

    private static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edgeIdx)
    {
        int v0 = mesh.Edges[edgeIdx][0];
        int v1 = mesh.Edges[edgeIdx][1];
        int dim = mesh.EmbeddingDimension;
        var coords0 = mesh.GetVertexCoordinates(v0);
        var coords1 = mesh.GetVertexCoordinates(v1);
        var dir = new double[dim];
        double norm = 0.0;
        for (int k = 0; k < dim; k++)
        {
            dir[k] = coords1[k] - coords0[k];
            norm += dir[k] * dir[k];
        }
        norm = System.Math.Sqrt(norm);
        if (norm > 1e-14)
            for (int k = 0; k < dim; k++)
                dir[k] /= norm;
        return dir;
    }
}

/// <summary>
/// Return type for Phase4FermionFamilyAtlasStudy.Run().
/// Contains all pipeline outputs for programmatic inspection.
/// </summary>
public sealed class Phase4StudyResult
{
    public required string StudyId { get; init; }
    public required SimplicialMesh YMesh { get; init; }
    public required BackgroundRecord Background { get; init; }
    public required SpinorRepresentationSpec SpinorSpec { get; init; }
    public required FermionFieldLayout Layout { get; init; }
    public required DiracOperatorBundle DiracBundle { get; init; }
    public required FermionSpectralBundle SpectralBundle { get; init; }
    public required FermionFamilyAtlas FermionAtlas { get; init; }
    public required IReadOnlyList<FamilyClusterRecord> Clusters { get; init; }
    public required CouplingAtlas CouplingAtlas { get; init; }
    public required UnifiedParticleRegistry Registry { get; init; }
    /// <summary>
    /// Tracks which Phase III boson registry was consumed in the unified registry merge (GAP-5).
    /// For the in-process study this is the stub registry version; for full pipeline runs,
    /// this would be the version string from the loaded Phase III registry.
    /// </summary>
    public required string ConsumedBosonRegistryId { get; init; }
    public required Phase4Report Report { get; init; }
    public required Gu.Phase4.Comparison.FermionComparisonCampaign ComparisonCampaign { get; init; }
    public required List<Gu.Phase4.Observation.FermionObservationSummary> FermionObservations { get; init; }
    public required List<InteractionObservationSummary> InteractionObservations { get; init; }
    public required Dictionary<string, string> ArtifactPaths { get; init; }
    public required string ReportMdPath { get; init; }
}

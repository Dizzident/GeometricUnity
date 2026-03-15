using Gu.Core;
using Gu.Core.Serialization;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Orchestrates background atlas construction (Section 7.1).
/// For each spec: solves for a background, grades admissibility,
/// deduplicates, ranks, and assembles the atlas.
/// </summary>
public sealed class BackgroundAtlasBuilder
{
    private readonly ISolverBackend? _backend;
    private readonly Func<BackgroundSpec, BranchManifest, GeometryContext, ISolverBackend>? _backendFactory;

    public BackgroundAtlasBuilder(ISolverBackend backend)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
    }

    public BackgroundAtlasBuilder(Func<BackgroundSpec, BranchManifest, GeometryContext, ISolverBackend> backendFactory)
    {
        _backendFactory = backendFactory ?? throw new ArgumentNullException(nameof(backendFactory));
    }

    /// <summary>
    /// Build a background atlas from a study specification.
    /// </summary>
    /// <param name="study">Study specification listing all background specs.</param>
    /// <param name="manifests">Branch manifests keyed by BranchManifestId.</param>
    /// <param name="geometries">Geometry contexts keyed by EnvironmentId.</param>
    /// <param name="a0s">Background connections keyed by EnvironmentId.</param>
    /// <param name="provenance">Provenance metadata for the atlas.</param>
    public BackgroundAtlas Build(
        BackgroundStudySpec study,
        IReadOnlyDictionary<string, BranchManifest> manifests,
        IReadOnlyDictionary<string, GeometryContext> geometries,
        IReadOnlyDictionary<string, FieldTensor> a0s,
        ProvenanceMeta provenance)
        => Build(study, manifests, geometries, a0s, provenance, out _);

    /// <summary>
    /// Build a background atlas from a study specification, also returning
    /// the solved omega state tensors keyed by BackgroundId for persistence.
    /// </summary>
    /// <param name="study">Study specification listing all background specs.</param>
    /// <param name="manifests">Branch manifests keyed by BranchManifestId.</param>
    /// <param name="geometries">Geometry contexts keyed by EnvironmentId.</param>
    /// <param name="a0s">Background connections keyed by EnvironmentId.</param>
    /// <param name="provenance">Provenance metadata for the atlas.</param>
    /// <param name="solvedStates">Output: solved omega tensors keyed by BackgroundId.</param>
    public BackgroundAtlas Build(
        BackgroundStudySpec study,
        IReadOnlyDictionary<string, BranchManifest> manifests,
        IReadOnlyDictionary<string, GeometryContext> geometries,
        IReadOnlyDictionary<string, FieldTensor> a0s,
        ProvenanceMeta provenance,
        out IReadOnlyDictionary<string, FieldTensor> solvedStates)
    {
        ArgumentNullException.ThrowIfNull(study);
        ArgumentNullException.ThrowIfNull(manifests);
        ArgumentNullException.ThrowIfNull(geometries);
        ArgumentNullException.ThrowIfNull(a0s);
        ArgumentNullException.ThrowIfNull(provenance);

        var allRecords = new List<BackgroundRecord>();
        var allStates = new Dictionary<string, FieldTensor>();

        foreach (var spec in study.Specs)
        {
            var (record, state) = SolveOneBackground(spec, manifests, geometries, a0s, provenance, study.EnvironmentTier);
            allRecords.Add(record);
            if (state is not null)
                allStates[record.BackgroundId] = state;
        }

        // Deduplicate using state-space L2 distance
        var deduped = Deduplicate(allRecords, study.DeduplicationThreshold, allStates);

        // Separate admitted from rejected
        var admitted = deduped
            .Where(r => r.AdmissibilityLevel != AdmissibilityLevel.Rejected)
            .ToList();
        var rejected = deduped
            .Where(r => r.AdmissibilityLevel == AdmissibilityLevel.Rejected)
            .ToList();

        // Rank admitted backgrounds
        var ranked = Rank(admitted, study.RankingCriteria);

        // Count by admissibility level
        var counts = new Dictionary<string, int>();
        foreach (var level in Enum.GetValues<AdmissibilityLevel>())
        {
            int count = deduped.Count(r => r.AdmissibilityLevel == level);
            if (count > 0)
                counts[level.ToString()] = count;
        }

        solvedStates = allStates;
        return new BackgroundAtlas
        {
            AtlasId = $"atlas-{study.StudyId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StudyId = study.StudyId,
            Backgrounds = ranked,
            RejectedBackgrounds = rejected,
            RankingCriteria = study.RankingCriteria,
            TotalAttempts = allRecords.Count,
            Provenance = provenance,
            AdmissibilityCounts = counts,
            EnvironmentTier = study.EnvironmentTier,
        };
    }

    /// <summary>
    /// Solve a single background spec and produce a record.
    /// </summary>
    internal (BackgroundRecord Record, FieldTensor? State) SolveOneBackground(
        BackgroundSpec spec,
        IReadOnlyDictionary<string, BranchManifest> manifests,
        IReadOnlyDictionary<string, GeometryContext> geometries,
        IReadOnlyDictionary<string, FieldTensor> a0s,
        ProvenanceMeta provenance,
        string? environmentTier = null)
    {
        if (!manifests.TryGetValue(spec.BranchManifestId, out var manifest))
            return (CreateRejectedRecord(spec, provenance, $"Branch manifest '{spec.BranchManifestId}' not found.", environmentTier), null);

        if (!geometries.TryGetValue(spec.EnvironmentId, out var geometry))
            return (CreateRejectedRecord(spec, provenance, $"Geometry for environment '{spec.EnvironmentId}' not found.", environmentTier), null);

        if (!a0s.TryGetValue(spec.EnvironmentId, out var a0))
            return (CreateRejectedRecord(spec, provenance, $"Background connection for environment '{spec.EnvironmentId}' not found.", environmentTier), null);

        // Build initial omega from seed
        var omega = BuildInitialOmega(spec.Seed, a0);

        // Solve
        var solverOptions = spec.SolveOptions.ToSolverOptions();
        var backend = _backendFactory?.Invoke(spec, manifest, geometry)
            ?? _backend
            ?? throw new InvalidOperationException("No solver backend or backend factory was configured.");
        SolverResult solverResult;
        try
        {
            var orchestrator = new SolverOrchestrator(backend, solverOptions);
            solverResult = orchestrator.Solve(omega, a0, manifest, geometry);
        }
        catch (Exception ex)
        {
            return (CreateRejectedRecord(spec, provenance, $"Solver failed: {ex.Message}"), null);
        }

        // Compute stationarity norm if solver didn't provide it
        double stationarityNorm = solverResult.FinalGradientNorm;

        // Grade admissibility
        var grader = new AdmissibilityGrader(spec.SolveOptions);
        var (level, rejectionReason) = grader.Grade(solverResult.FinalResidualNorm, stationarityNorm);

        var metrics = new BackgroundMetrics
        {
            ResidualNorm = solverResult.FinalResidualNorm,
            StationarityNorm = stationarityNorm,
            ObjectiveValue = solverResult.FinalObjective,
            GaugeViolation = solverResult.FinalGaugeViolation,
            SolverIterations = solverResult.Iterations,
            SolverConverged = solverResult.Converged,
            TerminationReason = solverResult.TerminationReason,
            GaussNewtonValid = level == AdmissibilityLevel.B2,
        };

        var geoFingerprint = ComputeGeometryFingerprint(geometry);

        var record = new BackgroundRecord
        {
            BackgroundId = $"bg-{spec.SpecId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            EnvironmentId = spec.EnvironmentId,
            BranchManifestId = spec.BranchManifestId,
            ContinuationCoordinates = spec.ContinuationCoordinates,
            GeometryFingerprint = geoFingerprint,
            GaugeChoice = spec.GaugeChoice,
            StateArtifactRef = $"state-{spec.SpecId}",
            ResidualNorm = solverResult.FinalResidualNorm,
            StationarityNorm = stationarityNorm,
            AdmissibilityLevel = level,
            Metrics = metrics,
            ReplayTierAchieved = "R2",
            Provenance = provenance,
            RejectionReason = rejectionReason,
            EnvironmentTier = environmentTier,
        };

        return (record, solverResult.FinalOmega);
    }

    private static BackgroundRecord CreateRejectedRecord(
        BackgroundSpec spec, ProvenanceMeta provenance, string reason, string? environmentTier = null)
    {
        return new BackgroundRecord
        {
            BackgroundId = $"bg-{spec.SpecId}-rejected",
            EnvironmentId = spec.EnvironmentId,
            BranchManifestId = spec.BranchManifestId,
            ContinuationCoordinates = spec.ContinuationCoordinates,
            GeometryFingerprint = "unknown",
            GaugeChoice = spec.GaugeChoice,
            StateArtifactRef = "none",
            ResidualNorm = double.PositiveInfinity,
            StationarityNorm = double.PositiveInfinity,
            AdmissibilityLevel = AdmissibilityLevel.Rejected,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = double.PositiveInfinity,
                StationarityNorm = double.PositiveInfinity,
                ObjectiveValue = double.PositiveInfinity,
                GaugeViolation = double.PositiveInfinity,
                SolverIterations = 0,
                SolverConverged = false,
                TerminationReason = reason,
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R0",
            Provenance = provenance,
            RejectionReason = reason,
            EnvironmentTier = environmentTier,
        };
    }

    private static FieldTensor BuildInitialOmega(BackgroundSeed seed, FieldTensor a0)
    {
        if (seed.Kind == BackgroundSeedKind.Explicit)
        {
            if (seed.InitialState is not null)
                return seed.InitialState;
        }

        if (seed.Kind == BackgroundSeedKind.SymmetricAnsatz)
        {
            if (seed.InitialState is not null)
                return seed.InitialState;

            return BuildSymmetricAnsatzOmega(seed, a0);
        }

        // Trivial / Continuation / CoarseGridTransfer: start from zero
        return new FieldTensor
        {
            Label = "omega_initial",
            Signature = a0.Signature,
            Coefficients = new double[a0.Coefficients.Length],
            Shape = a0.Shape.ToArray(),
        };
    }

    private static FieldTensor BuildSymmetricAnsatzOmega(BackgroundSeed seed, FieldTensor a0)
    {
        var coefficients = new double[a0.Coefficients.Length];
        int labelHash = System.Math.Abs((seed.Label ?? "symmetric-ansatz").GetHashCode());
        double amplitude = 0.02 + (labelHash % 5) * 0.005;
        double phase = 0.15 * ((labelHash % 7) + 1);

        for (int i = 0; i < coefficients.Length; i++)
        {
            double angle = phase + 0.21 * (i + 1);
            coefficients[i] = amplitude * System.Math.Sin(angle);
        }

        return new FieldTensor
        {
            Label = "omega_symmetric_ansatz",
            Signature = a0.Signature,
            Coefficients = coefficients,
            Shape = a0.Shape.ToArray(),
        };
    }

    /// <summary>
    /// Remove duplicate backgrounds within the state-space L2 distance threshold.
    /// Uses ||omega_1 - omega_2|| / max(||omega_1||, 1) as the distance metric.
    /// Keeps the one with better admissibility / lower residual.
    /// Falls back to metric-based heuristic when state tensors are not available.
    /// </summary>
    internal static List<BackgroundRecord> Deduplicate(
        List<BackgroundRecord> records, double threshold,
        IReadOnlyDictionary<string, FieldTensor>? states = null)
    {
        if (records.Count <= 1)
            return records;

        var result = new List<BackgroundRecord>();
        var used = new bool[records.Count];

        // Sort by admissibility level then residual (best first)
        var sorted = records
            .Select((r, i) => (Record: r, Index: i))
            .OrderByDescending(x => x.Record.AdmissibilityLevel)
            .ThenBy(x => x.Record.ResidualNorm)
            .ToList();

        foreach (var (record, idx) in sorted)
        {
            if (used[idx]) continue;
            result.Add(record);
            used[idx] = true;

            // Mark duplicates
            for (int j = 0; j < records.Count; j++)
            {
                if (used[j]) continue;
                if (AreStatesDuplicate(record, records[j], threshold, states))
                    used[j] = true;
            }
        }

        return result;
    }

    private static bool AreStatesDuplicate(
        BackgroundRecord a, BackgroundRecord b, double threshold,
        IReadOnlyDictionary<string, FieldTensor>? states)
    {
        // Same environment and branch required for comparison
        if (a.EnvironmentId != b.EnvironmentId || a.BranchManifestId != b.BranchManifestId)
            return false;

        // Use state-space L2 distance when state tensors are available
        if (states is not null &&
            states.TryGetValue(a.BackgroundId, out var stateA) &&
            states.TryGetValue(b.BackgroundId, out var stateB) &&
            stateA.Coefficients.Length == stateB.Coefficients.Length)
        {
            double diffNormSq = 0;
            double normASq = 0;
            for (int i = 0; i < stateA.Coefficients.Length; i++)
            {
                double d = stateA.Coefficients[i] - stateB.Coefficients[i];
                diffNormSq += d * d;
                normASq += stateA.Coefficients[i] * stateA.Coefficients[i];
            }
            double normA = System.Math.Sqrt(normASq);
            double diffNorm = System.Math.Sqrt(diffNormSq);
            double relativeDistance = diffNorm / System.Math.Max(normA, 1.0);
            return relativeDistance < threshold;
        }

        // Fallback: metric-based heuristic when states are unavailable
        double distance = System.Math.Abs(a.ResidualNorm - b.ResidualNorm)
                        + System.Math.Abs(a.StationarityNorm - b.StationarityNorm);
        return distance < threshold;
    }

    /// <summary>
    /// Rank backgrounds by the specified criteria.
    /// </summary>
    internal static List<BackgroundRecord> Rank(
        List<BackgroundRecord> records, string criteria)
    {
        return criteria switch
        {
            "residual-then-stationarity" => records
                .OrderBy(r => r.ResidualNorm)
                .ThenBy(r => r.StationarityNorm)
                .ToList(),
            "stationarity-then-residual" => records
                .OrderBy(r => r.StationarityNorm)
                .ThenBy(r => r.ResidualNorm)
                .ToList(),
            _ => records // "admissibility-then-residual" (default, B2 first)
                .OrderByDescending(r => r.AdmissibilityLevel)
                .ThenBy(r => r.ResidualNorm)
                .ToList(),
        };
    }

    private static string ComputeGeometryFingerprint(GeometryContext geometry)
    {
        // Simple fingerprint from geometry context fields
        return $"{geometry.DiscretizationType}-{geometry.BaseSpace.SpaceId}-{geometry.AmbientSpace.SpaceId}" +
               $"-{geometry.QuadratureRuleId}-{geometry.BasisFamilyId}";
    }
}

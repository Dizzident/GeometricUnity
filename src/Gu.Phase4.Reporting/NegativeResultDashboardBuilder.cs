using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Registry;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Builds a <see cref="NegativeResultDashboard"/> from Phase IV analysis outputs.
///
/// Negative results are first-class outputs: they constrain the claim space and
/// distinguish branch-consistent execution from physical validation.
///
/// Categories populated:
///   - UnstableChiralityEntries: families with mixed/trivial chirality or high gauge leakage
///   - FragileCouplingEntries: coupling matrices at or below the noise floor
///   - BrokenFamilyClusterEntries: clusters with low persistence or high ambiguity
///   - AdditionalNotes: demotion log from registry + manually added freeform notes
/// </summary>
public sealed class NegativeResultDashboardBuilder
{
    private readonly string _studyId;
    private readonly List<FamilyClusterRecord> _clusters = new();
    private readonly List<CouplingMatrixSummary> _couplings = new();
    private readonly List<string> _additionalNotes = new();

    /// <summary>Persistence below this threshold → cluster is "broken".</summary>
    public const double DefaultBrokenPersistenceThreshold = 0.3;

    /// <summary>Ambiguity above this threshold → cluster is "broken".</summary>
    public const double DefaultBrokenAmbiguityThreshold = 0.6;

    /// <summary>Noise floor below which a coupling entry is considered zero.</summary>
    public const double DefaultCouplingNoiseFloor = 1e-10;

    /// <summary>Gauge leakage norm above this is "high" (chirality contaminated).</summary>
    public const double DefaultLeakageThreshold = 0.1;

    public NegativeResultDashboardBuilder(string studyId)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        _studyId = studyId;
    }

    /// <summary>Add a family cluster for broken-cluster analysis.</summary>
    public NegativeResultDashboardBuilder AddFamilyCluster(FamilyClusterRecord cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        _clusters.Add(cluster);
        return this;
    }

    /// <summary>Add a coupling matrix for fragile-coupling analysis.</summary>
    public NegativeResultDashboardBuilder AddCouplingMatrix(CouplingMatrixSummary matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        _couplings.Add(matrix);
        return this;
    }

    /// <summary>Add a freeform negative result note.</summary>
    public NegativeResultDashboardBuilder AddNote(string note)
    {
        ArgumentNullException.ThrowIfNull(note);
        _additionalNotes.Add(note);
        return this;
    }

    /// <summary>
    /// Build the dashboard. Pass <paramref name="fermionSummary"/> from <see cref="FermionAtlasSummary"/>
    /// and optionally a <see cref="UnifiedParticleRegistry"/> to collect demotion log entries.
    /// </summary>
    public NegativeResultDashboard Build(
        FermionAtlasSummary fermionSummary,
        UnifiedParticleRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(fermionSummary);

        var unstableChirality = BuildUnstableChirality(fermionSummary);
        var fragileCouplings = BuildFragileCouplings();
        var brokenClusters = BuildBrokenClusters();

        var additionalNotes = new List<string>(_additionalNotes);
        if (registry is not null)
        {
            foreach (var candidate in registry.Candidates)
            {
                foreach (var demotion in candidate.Demotions)
                {
                    additionalNotes.Add(
                        $"[demotion] {candidate.ParticleId} ({candidate.ParticleType}): " +
                        $"{demotion.Reason} — {demotion.Details} ({demotion.FromClaimClass}→{demotion.ToClaimClass})");
                }
                foreach (var note in candidate.AmbiguityNotes)
                {
                    additionalNotes.Add($"[ambiguity] {candidate.ParticleId}: {note}");
                }
            }
        }

        return new NegativeResultDashboard
        {
            DashboardId = $"neg-dashboard-{_studyId}",
            StudyId = _studyId,
            UnstableChiralityEntries = unstableChirality,
            FragileCouplingEntries = fragileCouplings,
            BrokenFamilyClusterEntries = brokenClusters,
            AdditionalNotes = additionalNotes,
        };
    }

    private static List<UnstableChiralityEntry> BuildUnstableChirality(FermionAtlasSummary fermionSummary)
    {
        var entries = new List<UnstableChiralityEntry>();

        foreach (var chirality in fermionSummary.ChiralitySummaries)
        {
            bool isUnstable = chirality.ChiralityStatus is "mixed" or "trivial"
                || chirality.LeakageNorm > DefaultLeakageThreshold;

            if (!isUnstable) continue;

            string reason = chirality.ChiralityStatus switch
            {
                "mixed" => $"No definite chirality assignment — projection weights (L={chirality.LeftProjection:F2}, R={chirality.RightProjection:F2})",
                "trivial" => $"Trivial (non-chiral) geometry — dimY is odd or symmetry-forbidden; L={chirality.LeftProjection:F2}, R={chirality.RightProjection:F2}",
                _ when chirality.LeakageNorm > DefaultLeakageThreshold => $"High gauge leakage norm {chirality.LeakageNorm:F3} contaminates chirality assignment",
                _ => $"Chirality status '{chirality.ChiralityStatus}' is ambiguous",
            };

            entries.Add(new UnstableChiralityEntry
            {
                FamilyId = chirality.FamilyId,
                ChiralityStatus = chirality.ChiralityStatus,
                LeftProjection = chirality.LeftProjection,
                RightProjection = chirality.RightProjection,
                LeakageNorm = chirality.LeakageNorm,
                Reason = reason,
            });
        }

        return entries;
    }

    private List<FragileCouplingEntry> BuildFragileCouplings()
    {
        var entries = new List<FragileCouplingEntry>();

        foreach (var matrix in _couplings)
        {
            if (matrix.MaxEntry > DefaultCouplingNoiseFloor) continue;

            entries.Add(new FragileCouplingEntry
            {
                BosonModeId = matrix.BosonModeId,
                MaxMagnitude = matrix.MaxEntry,
                FrobeniusNorm = matrix.FrobeniusNorm,
                FermionPairCount = matrix.FermionPairCount,
                FragilityReason = "below-noise-floor",
            });
        }

        return entries;
    }

    private List<BrokenFamilyClusterEntry> BuildBrokenClusters()
    {
        var entries = new List<BrokenFamilyClusterEntry>();

        foreach (var cluster in _clusters)
        {
            string? failureMode = null;
            string demotionReason = "none";

            if (cluster.MeanBranchPersistence < DefaultBrokenPersistenceThreshold)
            {
                failureMode = "low-persistence";
                demotionReason = "LowPersistence";
            }
            else if (cluster.AmbiguityScore > DefaultBrokenAmbiguityThreshold)
            {
                failureMode = "high-ambiguity";
                demotionReason = "AmbiguousMatching";
            }
            else if (cluster.ClusteringNotes.Any(n => n.Contains("split", StringComparison.OrdinalIgnoreCase)))
            {
                failureMode = "split-detected";
                demotionReason = "AmbiguousMatching";
            }
            else if (cluster.ClusteringNotes.Any(n => n.Contains("merge", StringComparison.OrdinalIgnoreCase)))
            {
                failureMode = "merge-detected";
                demotionReason = "AmbiguousMatching";
            }
            else if (cluster.ClusteringNotes.Any(n => n.Contains("crossing", StringComparison.OrdinalIgnoreCase)))
            {
                failureMode = "avoided-crossing";
                demotionReason = "AmbiguousMatching";
            }

            if (failureMode is null) continue;

            entries.Add(new BrokenFamilyClusterEntry
            {
                ClusterId = cluster.ClusterId,
                MeanPersistence = cluster.MeanBranchPersistence,
                AmbiguityScore = cluster.AmbiguityScore,
                MemberCount = cluster.MemberCount,
                FailureMode = failureMode,
                DemotionReason = demotionReason,
            });
        }

        return entries;
    }
}

using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Deterministic generation-like clustering of fermionic mode families.
///
/// Algorithm (IMPL_PLAN_P4.md §8.6):
///   Pass 1 — Conjugation-rule clustering:
///     Pair families that have already been identified as conjugation partners
///     (ConjugateFamilyId != null) into two-member clusters labeled "conjugate-pair".
///
///   Pass 2 — Eigenvalue-proximity clustering:
///     Group remaining (unpaired) families whose mean eigenvalue magnitudes are
///     within EigenvalueProximityRelTol of each other, using single-linkage
///     hierarchical clustering.
///     Two families are "proximate" if |mean_i - mean_j| / max(mean_i, mean_j, eps) <= tol.
///
///   Pass 3 — Singleton sweep:
///     Any family not claimed by either pass becomes its own singleton cluster.
///
/// Ambiguity scoring:
///   A cluster is ambiguous if any member family has ambiguity notes.
///   AmbiguityScore = fraction of member families with non-empty AmbiguityNotes.
///
/// Output: sorted by mean EigenvalueMagnitudeEnvelope[1] (ascending).
/// Cluster labels: "cluster-0", "cluster-1", ... in sorted order.
/// </summary>
public sealed class FamilyClusteringEngine
{
    private readonly FamilyClusteringConfig _config;

    public FamilyClusteringEngine(FamilyClusteringConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Cluster a list of FermionModeFamily records into FamilyClusterRecords.
    /// Returns empty list if families is empty.
    /// </summary>
    public IReadOnlyList<FamilyClusterRecord> Cluster(
        IReadOnlyList<FermionModeFamily> families,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(families);
        ArgumentNullException.ThrowIfNull(provenance);

        if (families.Count == 0)
            return Array.Empty<FamilyClusterRecord>();

        var familyList = families.ToList();
        var clustered = new HashSet<string>(); // by FamilyId
        var clusters = new List<ClusterAccumulator>();

        // Pass 1: Conjugation-rule clustering
        // Build a lookup to resolve conjugate family IDs
        var byId = familyList.ToDictionary(f => f.FamilyId);

        foreach (var family in familyList)
        {
            if (clustered.Contains(family.FamilyId)) continue;
            if (family.ConjugateFamilyId is null) continue;
            if (!byId.TryGetValue(family.ConjugateFamilyId, out var conjugate)) continue;
            if (clustered.Contains(conjugate.FamilyId)) continue;

            var acc = new ClusterAccumulator("conjugation-rule");
            acc.Add(family);
            acc.Add(conjugate);
            clustered.Add(family.FamilyId);
            clustered.Add(conjugate.FamilyId);
            clusters.Add(acc);
        }

        // Pass 2: Eigenvalue-proximity clustering (single-linkage)
        var remaining = familyList
            .Where(f => !clustered.Contains(f.FamilyId))
            .OrderBy(f => f.EigenvalueMagnitudeEnvelope[1])
            .ToList();

        // Union-find approach: merge families whose eigenvalue bands are proximate
        while (remaining.Count > 0)
        {
            var seed = remaining[0];
            remaining.RemoveAt(0);

            var acc = new ClusterAccumulator("eigenvalue-proximity");
            acc.Add(seed);
            clustered.Add(seed.FamilyId);

            // Single-linkage: keep expanding as long as any cluster member is proximate to unprocessed
            bool expanded = true;
            while (expanded)
            {
                expanded = false;
                for (int i = remaining.Count - 1; i >= 0; i--)
                {
                    var candidate = remaining[i];
                    if (acc.IsProximate(candidate, _config.EigenvalueProximityRelTol))
                    {
                        acc.Add(candidate);
                        clustered.Add(candidate.FamilyId);
                        remaining.RemoveAt(i);
                        expanded = true;
                    }
                }
            }

            // Only treat as eigenvalue-proximity cluster if it actually has > 1 member;
            // otherwise mark as singleton during pass 3.
            if (acc.Count == 1)
                acc.SetMethod("singleton");

            clusters.Add(acc);
        }

        // Sort clusters by mean eigenvalue magnitude
        clusters.Sort((a, b) =>
            a.MeanEigenvalue.CompareTo(b.MeanEigenvalue));

        // Build output records
        var result = new List<FamilyClusterRecord>(clusters.Count);
        for (int i = 0; i < clusters.Count; i++)
        {
            result.Add(clusters[i].Build($"cluster-{i}", i, provenance));
        }

        return result;
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private sealed class ClusterAccumulator
    {
        private readonly List<FermionModeFamily> _members = new();
        private string _method;

        public ClusterAccumulator(string method) => _method = method;

        public int Count => _members.Count;

        public double MeanEigenvalue =>
            _members.Count == 0 ? 0.0 : _members.Average(f => f.EigenvalueMagnitudeEnvelope[1]);

        public void Add(FermionModeFamily family) => _members.Add(family);
        public void SetMethod(string method) => _method = method;

        /// <summary>True if candidate's eigenvalue mean is proximate to any current member.</summary>
        public bool IsProximate(FermionModeFamily candidate, double relTol)
        {
            double cMean = candidate.EigenvalueMagnitudeEnvelope[1];
            foreach (var member in _members)
            {
                double mMean = member.EigenvalueMagnitudeEnvelope[1];
                double scale = System.Math.Max(System.Math.Max(cMean, mMean), 1e-14);
                if (System.Math.Abs(cMean - mMean) / scale <= relTol)
                    return true;
            }
            return false;
        }

        public FamilyClusterRecord Build(string clusterId, int index, ProvenanceMeta provenance)
        {
            var memberIds = _members.Select(f => f.FamilyId).ToList();

            // Dominant chirality
            var chiralities = _members.Select(f => f.DominantChiralityProfile).ToList();
            bool hasLeft = chiralities.Any(c => c == "left");
            bool hasRight = chiralities.Any(c => c == "right");
            string dominantChirality;
            if (hasLeft && hasRight)
                dominantChirality = "conjugate-pair";
            else if (hasLeft)
                dominantChirality = "left";
            else if (hasRight)
                dominantChirality = "right";
            else
                dominantChirality = chiralities.Count > 0 ? chiralities[0] : "undetermined";

            // Eigenvalue envelope across all members
            var allMeans = _members.SelectMany(f =>
                new[] { f.EigenvalueMagnitudeEnvelope[0], f.EigenvalueMagnitudeEnvelope[2] }).ToArray();
            double minEv = allMeans.Length > 0 ? allMeans.Min() : 0.0;
            double maxEv = allMeans.Length > 0 ? allMeans.Max() : 0.0;
            double meanEv = _members.Count > 0 ? _members.Average(f => f.EigenvalueMagnitudeEnvelope[1]) : 0.0;

            // Ambiguity scoring
            int ambiguousCount = _members.Count(f => f.AmbiguityNotes.Count > 0);
            double ambiguityScore = _members.Count > 0 ? (double)ambiguousCount / _members.Count : 0.0;

            // Persistence
            double meanPersistence = _members.Count > 0
                ? _members.Average(f => f.BranchPersistenceScore)
                : 0.0;

            bool hasConjugatePair = hasLeft && hasRight;

            // Clustering notes
            var notes = new List<string>();
            if (ambiguousCount > 0)
                notes.Add($"{ambiguousCount}/{_members.Count} member families have ambiguous tracking assignments.");
            if (_method == "singleton")
                notes.Add("Singleton cluster: no matching family found within proximity threshold.");
            if (dominantChirality == "mixed" || dominantChirality == "undetermined")
                notes.Add("Dominant chirality is undetermined; physical identification requires M43 observation.");

            return new FamilyClusterRecord
            {
                ClusterId = clusterId,
                ClusterLabel = $"cluster-{index}",
                MemberFamilyIds = memberIds,
                DominantChirality = dominantChirality,
                HasConjugatePair = hasConjugatePair,
                EigenvalueMagnitudeEnvelope = new[] { minEv, meanEv, maxEv },
                AmbiguityScore = ambiguityScore,
                MeanBranchPersistence = meanPersistence,
                ClusteringMethod = _method,
                ClusteringNotes = notes,
                Provenance = provenance,
            };
        }
    }
}

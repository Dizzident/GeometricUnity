using Gu.Core;

namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Engine for running branch-robustness studies (M46).
///
/// Given a study spec and a set of quantity values per branch variant,
/// computes pairwise distance matrices, equivalence classes, fragility records,
/// and invariance candidates.
///
/// The engine is quantity-agnostic: callers supply the quantity values as
/// IReadOnlyDictionary&lt;string, double[]&gt; (key = quantityId, value = per-variant values).
/// This keeps the engine decoupled from Phase III/IV physics.
/// </summary>
public sealed class BranchRobustnessEngine
{
    private readonly BranchRobustnessStudySpec _spec;

    public BranchRobustnessEngine(BranchRobustnessStudySpec spec)
    {
        _spec = spec;
    }

    /// <summary>
    /// Run the branch-robustness study.
    /// </summary>
    /// <param name="quantityValues">
    /// Values to analyze. Key = target quantity ID; value = array of length equal to
    /// spec.BranchVariantIds.Count, one entry per variant in the same order.
    /// </param>
    /// <param name="provenance">Provenance metadata to attach to the record.</param>
    /// <returns>A fully populated <see cref="BranchRobustnessRecord"/>.</returns>
    public BranchRobustnessRecord Run(
        IReadOnlyDictionary<string, double[]> quantityValues,
        ProvenanceMeta provenance)
    {
        var variants = _spec.BranchVariantIds;
        int n = variants.Count;

        var distanceMatrices = new Dictionary<string, BranchDistanceMatrix>();
        var equivalenceClassesMap = new Dictionary<string, List<BranchEquivalenceClass>>();
        var fragilityMap = new Dictionary<string, FragilityRecord>();
        var invarianceCandidates = new List<InvarianceCandidateRecord>();
        var notes = new List<string>();

        if (n < 2)
            notes.Add($"Warning: fewer than 2 branch variants ({n}) — study is inconclusive.");

        foreach (var qid in _spec.TargetQuantityIds)
        {
            if (!quantityValues.TryGetValue(qid, out var vals) || vals.Length != n)
            {
                notes.Add($"Missing or mismatched values for quantity '{qid}'; skipping.");
                continue;
            }

            var matrix = BranchDistanceMatrix.Build(qid, variants, vals);
            distanceMatrices[qid] = matrix;

            var classes = ComputeEquivalenceClasses(qid, variants, vals);
            equivalenceClassesMap[qid] = classes;

            var fragility = ComputeFragility(qid, matrix, variants, vals);
            fragilityMap[qid] = fragility;

            if (fragility.Classification is "invariant" or "robust")
            {
                invarianceCandidates.Add(new InvarianceCandidateRecord
                {
                    TargetQuantityId = qid,
                    BranchFamilySize = n,
                    MeanValue = vals.Length > 0 ? vals.Average() : 0.0,
                    MaxAbsDeviation = matrix.MaxDistance,
                    FragilityScore = fragility.FragilityScore,
                    AbsoluteTolerance = _spec.AbsoluteTolerance,
                    RelativeTolerance = _spec.RelativeTolerance,
                    SourceStudyId = _spec.StudyId,
                });
            }
        }

        string summary = ComputeOverallSummary(fragilityMap, n);

        return new BranchRobustnessRecord
        {
            RecordId = $"{_spec.StudyId}-result",
            StudyId = _spec.StudyId,
            BranchVariantIds = variants,
            DistanceMatrices = distanceMatrices,
            EquivalenceClasses = equivalenceClassesMap,
            FragilityRecords = fragilityMap,
            InvarianceCandidates = invarianceCandidates,
            OverallSummary = summary,
            DiagnosticNotes = notes,
            Provenance = provenance,
        };
    }

    private List<BranchEquivalenceClass> ComputeEquivalenceClasses(
        string qid,
        List<string> variants,
        double[] vals)
    {
        int n = variants.Count;
        double absTol = _spec.AbsoluteTolerance;
        double relTol = _spec.RelativeTolerance;

        // Union-find approach: merge pairs within tolerance
        var parent = Enumerable.Range(0, n).ToArray();

        double refVal = vals.Length > 0 ? System.Math.Abs(vals[0]) : 0.0;
        double threshold = System.Math.Max(absTol, relTol * (refVal + 1e-30));

        for (int i = 0; i < n; i++)
        for (int j = i + 1; j < n; j++)
        {
            if (System.Math.Abs(vals[i] - vals[j]) <= threshold)
                Union(parent, i, j);
        }

        // Group by root
        var groups = new Dictionary<int, List<int>>();
        for (int i = 0; i < n; i++)
        {
            int root = Find(parent, i);
            if (!groups.TryGetValue(root, out var group))
                groups[root] = group = new List<int>();
            group.Add(i);
        }

        var classes = new List<BranchEquivalenceClass>();
        foreach (var kv in groups)
        {
            var indices = kv.Value;
            var memberIds = indices.Select(i => variants[i]).ToList();
            var memberVals = indices.Select(i => vals[i]).ToArray();
            double mean = memberVals.Average();
            double maxDev = memberVals.Max(v => System.Math.Abs(v - mean));

            classes.Add(new BranchEquivalenceClass
            {
                TargetQuantityId = qid,
                MemberBranchVariantIds = memberIds,
                MeanValue = mean,
                MaxDeviationFromMean = maxDev,
                AbsoluteTolerance = absTol,
                RelativeTolerance = relTol,
            });
        }

        return classes;
    }

    private FragilityRecord ComputeFragility(
        string qid,
        BranchDistanceMatrix matrix,
        List<string> variants,
        double[] vals)
    {
        int n = variants.Count;
        if (n < 2)
        {
            return new FragilityRecord
            {
                TargetQuantityId = qid,
                FragilityScore = 0.0,
                MaxDistanceToNeighbor = 0.0,
                MeanDistanceToFamily = 0.0,
                Classification = "indeterminate",
                MaxDistancePair = n >= 1 ? new[] { variants[0], variants[0] } : Array.Empty<string>(),
                VariantCount = n,
            };
        }

        // Find the max-distance pair (for MaxDistancePair field)
        double maxDist = 0.0;
        int maxI = 0, maxJ = 0;
        for (int i = 0; i < n; i++)
        for (int j = i + 1; j < n; j++)
        {
            var d = matrix.GetDistance(i, j);
            if (d > maxDist)
            {
                maxDist = d;
                maxI = i;
                maxJ = j;
            }
        }

        // Use precomputed mean from matrix (mean of all off-diagonal entries / 2 = mean of upper triangle)
        double meanDist = matrix.MeanDistance;
        const double epsilon = 1e-14; // division-by-zero guard only (matches distance metric epsilon)
        // Physicist formula: score = maxDistanceToNeighbor / (meanDistanceToFamily + epsilon)
        double fragilityScore = maxDist / (meanDist + epsilon);

        // Classification: first gate on absolute/relative tolerance, then use ratio threshold.
        // If maxDist is within tolerance, the quantity is invariant across branches.
        double absTol = _spec.AbsoluteTolerance;
        double relTol = _spec.RelativeTolerance;
        double meanVal = vals.Length > 0 ? vals.Average() : 0.0;
        string classification;
        if (maxDist <= absTol || maxDist <= relTol * System.Math.Abs(meanVal))
            classification = "invariant";
        else if (fragilityScore <= 0.5)
            classification = "robust";
        else
            classification = "fragile";

        return new FragilityRecord
        {
            TargetQuantityId = qid,
            FragilityScore = fragilityScore,
            MaxDistanceToNeighbor = maxDist,
            MeanDistanceToFamily = meanDist,
            Classification = classification,
            MaxDistancePair = new[] { variants[maxI], variants[maxJ] },
            VariantCount = n,
        };
    }

    private static string ComputeOverallSummary(
        Dictionary<string, FragilityRecord> fragility,
        int variantCount)
    {
        if (variantCount < 2) return "inconclusive";
        if (fragility.Count == 0) return "inconclusive";

        int invariantCount = fragility.Values.Count(f => f.Classification is "invariant" or "robust");
        int fragileCount = fragility.Values.Count(f => f.Classification == "fragile");

        if (fragileCount == 0) return "robust";
        if (invariantCount == 0) return "fragile";
        return "mixed";
    }

    private static int Find(int[] parent, int i)
    {
        while (parent[i] != i)
        {
            parent[i] = parent[parent[i]]; // path compression
            i = parent[i];
        }
        return i;
    }

    private static void Union(int[] parent, int i, int j)
    {
        int ri = Find(parent, i);
        int rj = Find(parent, j);
        if (ri != rj) parent[ri] = rj;
    }
}

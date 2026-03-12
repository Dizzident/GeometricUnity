using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase3.Registry;

/// <summary>
/// The boson registry: a collection of candidate boson records with
/// querying, serialization, and demotion support.
/// </summary>
public sealed class BosonRegistry
{
    /// <summary>Registry version string.</summary>
    [JsonPropertyName("registryVersion")]
    public string RegistryVersion { get; init; } = "1.0.0";

    /// <summary>All registered candidates.</summary>
    [JsonPropertyName("candidates")]
    public List<CandidateBosonRecord> Candidates { get; init; } = new();

    /// <summary>
    /// Register a new candidate boson.
    /// </summary>
    public CandidateBosonRecord Register(CandidateBosonRecord candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        Candidates.Add(candidate);
        return candidate;
    }

    /// <summary>
    /// Query candidates by claim class.
    /// </summary>
    public IReadOnlyList<CandidateBosonRecord> QueryByClaimClass(BosonClaimClass minClass)
    {
        return Candidates.Where(c => c.ClaimClass >= minClass).ToList();
    }

    /// <summary>
    /// Query candidates by background ID.
    /// </summary>
    public IReadOnlyList<CandidateBosonRecord> QueryByBackground(string backgroundId)
    {
        return Candidates.Where(c => c.BackgroundSet.Contains(backgroundId)).ToList();
    }

    /// <summary>
    /// Query candidates by family ID.
    /// </summary>
    public CandidateBosonRecord? QueryByFamily(string familyId)
    {
        return Candidates.FirstOrDefault(c => c.PrimaryFamilyId == familyId);
    }

    /// <summary>
    /// Total number of candidates.
    /// </summary>
    public int Count => Candidates.Count;

    /// <summary>
    /// Count of candidates at or above a given claim class.
    /// </summary>
    public int CountAboveClass(BosonClaimClass minClass)
    {
        return Candidates.Count(c => c.ClaimClass >= minClass);
    }

    /// <summary>
    /// Serialize the registry to JSON.
    /// </summary>
    public string ToJson(bool indented = true)
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = indented,
            Converters = { new JsonStringEnumConverter() },
        });
    }

    /// <summary>
    /// Deserialize a registry from JSON.
    /// </summary>
    public static BosonRegistry FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };
        var result = JsonSerializer.Deserialize<BosonRegistry>(json, options);
        return result ?? throw new InvalidOperationException("Failed to deserialize BosonRegistry.");
    }

    /// <summary>
    /// Compute the diff from this (base) registry to another registry.
    /// </summary>
    public RegistryDiff Diff(BosonRegistry other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var baseIds = new HashSet<string>(Candidates.Select(c => c.CandidateId));
        var otherIds = new HashSet<string>(other.Candidates.Select(c => c.CandidateId));

        var newIds = otherIds.Except(baseIds).ToList();
        var removedIds = baseIds.Except(otherIds).ToList();

        var baseLookup = Candidates.ToDictionary(c => c.CandidateId);
        var otherLookup = other.Candidates.ToDictionary(c => c.CandidateId);

        var claimChanges = new List<ClaimClassChange>();
        var demotionChanges = new List<DemotionChange>();

        foreach (var id in baseIds.Intersect(otherIds))
        {
            var baseRecord = baseLookup[id];
            var otherRecord = otherLookup[id];

            if (baseRecord.ClaimClass != otherRecord.ClaimClass)
            {
                claimChanges.Add(new ClaimClassChange
                {
                    CandidateId = id,
                    Before = baseRecord.ClaimClass,
                    After = otherRecord.ClaimClass,
                });
            }

            // Use (Reason, Details) tuple counts to detect duplicate demotion reasons.
            // A plain HashSet<DemotionReason> silently drops entries when the same
            // reason appears more than once in a single candidate's demotion list.
            var baseReasonCounts = baseRecord.Demotions
                .GroupBy(d => (d.Reason, d.Details))
                .ToDictionary(g => g.Key, g => g.Count());
            var addedDemotions = new List<BosonDemotionRecord>();
            foreach (var d in otherRecord.Demotions)
            {
                var key = (d.Reason, d.Details);
                baseReasonCounts.TryGetValue(key, out var baseCount);
                if (baseCount > 0)
                    baseReasonCounts[key] = baseCount - 1;
                else
                    addedDemotions.Add(d);
            }

            if (addedDemotions.Count > 0)
            {
                demotionChanges.Add(new DemotionChange
                {
                    CandidateId = id,
                    Added = addedDemotions,
                });
            }
        }

        return new RegistryDiff
        {
            NewCandidateIds = newIds,
            RemovedCandidateIds = removedIds,
            ClaimClassChanges = claimChanges,
            DemotionChanges = demotionChanges,
        };
    }
}

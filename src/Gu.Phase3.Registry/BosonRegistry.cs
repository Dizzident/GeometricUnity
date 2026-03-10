using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase3.Registry;

/// <summary>
/// The boson registry: a collection of candidate boson records with
/// querying, serialization, and demotion support.
/// </summary>
public sealed class BosonRegistry
{
    private readonly List<CandidateBosonRecord> _candidates = new();

    /// <summary>Registry version string.</summary>
    [JsonPropertyName("registryVersion")]
    public string RegistryVersion { get; init; } = "1.0.0";

    /// <summary>All registered candidates.</summary>
    [JsonPropertyName("candidates")]
    public IReadOnlyList<CandidateBosonRecord> Candidates => _candidates;

    /// <summary>
    /// Register a new candidate boson.
    /// </summary>
    public CandidateBosonRecord Register(CandidateBosonRecord candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        _candidates.Add(candidate);
        return candidate;
    }

    /// <summary>
    /// Query candidates by claim class.
    /// </summary>
    public IReadOnlyList<CandidateBosonRecord> QueryByClaimClass(BosonClaimClass minClass)
    {
        return _candidates.Where(c => c.ClaimClass >= minClass).ToList();
    }

    /// <summary>
    /// Query candidates by background ID.
    /// </summary>
    public IReadOnlyList<CandidateBosonRecord> QueryByBackground(string backgroundId)
    {
        return _candidates.Where(c => c.BackgroundSet.Contains(backgroundId)).ToList();
    }

    /// <summary>
    /// Query candidates by family ID.
    /// </summary>
    public CandidateBosonRecord? QueryByFamily(string familyId)
    {
        return _candidates.FirstOrDefault(c => c.PrimaryFamilyId == familyId);
    }

    /// <summary>
    /// Total number of candidates.
    /// </summary>
    public int Count => _candidates.Count;

    /// <summary>
    /// Count of candidates at or above a given claim class.
    /// </summary>
    public int CountAboveClass(BosonClaimClass minClass)
    {
        return _candidates.Count(c => c.ClaimClass >= minClass);
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
}

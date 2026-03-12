using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Observation;

namespace Gu.Phase4.Comparison;

/// <summary>
/// A minimal comparison campaign for fermionic candidates (M43).
///
/// Runs a FermionComparisonAdapter against a list of FamilyClusterRecords
/// and a list of FermionCandidateReferences, producing FermionComparisonRecords.
///
/// M43 completion criteria: at least one comparison campaign completes.
/// </summary>
public sealed class FermionComparisonCampaign
{
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    [JsonPropertyName("clusterCount")]
    public int ClusterCount { get; init; }

    [JsonPropertyName("referenceCount")]
    public int ReferenceCount { get; init; }

    [JsonPropertyName("compatibleCount")]
    public int CompatibleCount { get; init; }

    [JsonPropertyName("incompatibleCount")]
    public int IncompatibleCount { get; init; }

    [JsonPropertyName("underdeterminedCount")]
    public int UnderdeterminedCount { get; init; }

    [JsonPropertyName("notApplicableCount")]
    public int NotApplicableCount { get; init; }

    [JsonPropertyName("comparisonRecords")]
    public required List<FermionComparisonRecord> ComparisonRecords { get; init; }

    [JsonPropertyName("observationSummaries")]
    public required List<FermionObservationSummary> ObservationSummaries { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

/// <summary>
/// Runner for FermionComparisonCampaign.
/// </summary>
public static class FermionComparisonCampaignRunner
{
    /// <summary>
    /// Run a complete fermion comparison campaign.
    ///
    /// Steps:
    /// 1. Build FermionObservationSummary for each cluster.
    /// 2. Compare all observations against all references.
    /// 3. Aggregate outcome counts.
    /// </summary>
    public static FermionComparisonCampaign Run(
        string campaignId,
        IReadOnlyList<FamilyClusterRecord> clusters,
        IReadOnlyList<FermionCandidateReference> references,
        double massLikeScaleTolerance,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(campaignId);
        ArgumentNullException.ThrowIfNull(clusters);
        ArgumentNullException.ThrowIfNull(references);
        ArgumentNullException.ThrowIfNull(provenance);

        var observations = FermionObservationBuilder.BuildAll(clusters, provenance);
        var adapter = new FermionComparisonAdapter(massLikeScaleTolerance);
        var records = adapter.CompareAll(observations, references, provenance);

        int compatible = records.Count(r => r.Outcome == "compatible");
        int incompatible = records.Count(r => r.Outcome == "incompatible");
        int underdetermined = records.Count(r => r.Outcome == "underdetermined");
        int notApplicable = records.Count(r => r.Outcome == "not-applicable");

        return new FermionComparisonCampaign
        {
            CampaignId = campaignId,
            ClusterCount = clusters.Count,
            ReferenceCount = references.Count,
            CompatibleCount = compatible,
            IncompatibleCount = incompatible,
            UnderdeterminedCount = underdetermined,
            NotApplicableCount = notApplicable,
            ComparisonRecords = records,
            ObservationSummaries = observations,
            Provenance = provenance,
        };
    }
}

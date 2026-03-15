using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase4.Registry;
using Gu.Phase5.Environments;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Generates sidecar evidence files from registry, observable, and environment inputs (P6-M3).
///
/// Outputs written to outDir:
///   observation_chain.json        — ObservationChainRecord[] (one per candidate/observable pair)
///   environment_variance.json     — EnvironmentVarianceRecord[]
///   representation_content.json   — RepresentationContentRecord[]
///   coupling_consistency.json     — CouplingConsistencyRecord[]
///   sidecar_summary.json          — SidecarSummary (per-channel coverage report)
///
/// Channel status rules:
///   "evaluated" — at least one input record was supplied (InputCount &gt; 0); file written.
///   "skipped"   — inputs explicitly passed as empty list (InputCount == 0); file written with empty array.
///   "absent"    — inputs were null; file NOT written.
/// </summary>
public static class SidecarGenerator
{
    /// <summary>
    /// Generate all sidecar files and return a SidecarSummary.
    /// Pass null for a channel to mark it "absent" (no file written).
    /// Pass an empty list to mark it "skipped" (file written with empty array).
    /// </summary>
    public static SidecarSummary GenerateSidecars(
        UnifiedParticleRegistry registry,
        IReadOnlyList<QuantitativeObservableRecord>? observables,
        IReadOnlyList<EnvironmentRecord>? envRecords,
        string outDir,
        string studyId,
        ProvenanceMeta provenance,
        IReadOnlyList<ObservationChainRecord>? observationChainRecords = null,
        IReadOnlyList<EnvironmentVarianceRecord>? environmentVarianceRecords = null,
        IReadOnlyList<RepresentationContentRecord>? representationContentRecords = null,
        IReadOnlyList<CouplingConsistencyRecord>? couplingConsistencyRecords = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(outDir);
        ArgumentNullException.ThrowIfNull(studyId);
        ArgumentNullException.ThrowIfNull(provenance);

        Directory.CreateDirectory(outDir);

        var channels = new List<SidecarChannelStatus>();

        // 1. Observation chain
        channels.Add(WriteChannel(
            outDir,
            "observation_chain.json",
            "observation-chain",
            observationChainRecords,
            provenance));

        // 2. Environment variance
        channels.Add(WriteChannel(
            outDir,
            "environment_variance.json",
            "environment-variance",
            environmentVarianceRecords,
            provenance));

        // 3. Representation content
        channels.Add(WriteChannel(
            outDir,
            "representation_content.json",
            "representation-content",
            representationContentRecords,
            provenance));

        // 4. Coupling consistency
        channels.Add(WriteChannel(
            outDir,
            "coupling_consistency.json",
            "coupling-consistency",
            couplingConsistencyRecords,
            provenance));

        // 5. Write sidecar_summary.json
        var summary = new SidecarSummary
        {
            StudyId = studyId,
            Channels = channels,
            Provenance = provenance,
        };

        var summaryJson = GuJsonDefaults.Serialize(summary);
        File.WriteAllText(Path.Combine(outDir, "sidecar_summary.json"), summaryJson);

        return summary;
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static SidecarChannelStatus WriteChannel<T>(
        string outDir,
        string fileName,
        string channelId,
        IReadOnlyList<T>? records,
        ProvenanceMeta provenance)
    {
        if (records is null)
        {
            // Absent: no file written
            return new SidecarChannelStatus
            {
                ChannelId = channelId,
                Status = "absent",
                InputCount = 0,
                OutputCount = 0,
            };
        }

        // Evaluated or skipped: write file
        var json = GuJsonDefaults.Serialize(records);
        File.WriteAllText(Path.Combine(outDir, fileName), json);

        string status = records.Count > 0 ? "evaluated" : "skipped";
        return new SidecarChannelStatus
        {
            ChannelId = channelId,
            Status = status,
            InputCount = records.Count,
            OutputCount = records.Count,
        };
    }
}

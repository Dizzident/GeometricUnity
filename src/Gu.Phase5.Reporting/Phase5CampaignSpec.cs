using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Specification for a Phase V end-to-end campaign (M53).
/// Drives the full pipeline: branch independence, convergence, environments,
/// quantitative validation, falsification, claim escalation, dossier assembly.
/// </summary>
public sealed class Phase5CampaignSpec
{
    /// <summary>Unique campaign identifier.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    /// <summary>Branch-robustness study specification (M46).</summary>
    [JsonPropertyName("branchFamilySpec")]
    public required BranchRobustnessStudySpec BranchFamilySpec { get; init; }

    /// <summary>Refinement study specification (M47).</summary>
    [JsonPropertyName("refinementSpec")]
    public required RefinementStudySpec RefinementSpec { get; init; }

    /// <summary>Environment campaign specification (M48).</summary>
    [JsonPropertyName("environmentCampaignSpec")]
    public required EnvironmentCampaignSpec EnvironmentCampaignSpec { get; init; }

    /// <summary>Path to the external target table JSON file (M49).</summary>
    [JsonPropertyName("externalTargetTablePath")]
    public required string ExternalTargetTablePath { get; init; }

    /// <summary>Calibration policy for quantitative validation (M49).</summary>
    [JsonPropertyName("calibrationPolicy")]
    public required CalibrationPolicy CalibrationPolicy { get; init; }

    /// <summary>Falsification policy (M50).</summary>
    [JsonPropertyName("falsificationPolicy")]
    public required FalsificationPolicy FalsificationPolicy { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    // D-010: Additional artifact paths for analysis-first campaign runs

    /// <summary>Path to the branch quantity values JSON file (RefinementQuantityValueTable format).</summary>
    [JsonPropertyName("branchQuantityValuesPath")]
    public required string BranchQuantityValuesPath { get; init; }

    /// <summary>Path to the refinement values JSON file (RefinementQuantityValueTable format).</summary>
    [JsonPropertyName("refinementValuesPath")]
    public required string RefinementValuesPath { get; init; }

    /// <summary>Path to the observables JSON file (array of QuantitativeObservableRecord).</summary>
    [JsonPropertyName("observablesPath")]
    public required string ObservablesPath { get; init; }

    /// <summary>Paths to environment record JSON files.</summary>
    [JsonPropertyName("environmentRecordPaths")]
    public required IReadOnlyList<string> EnvironmentRecordPaths { get; init; }

    /// <summary>Path to the unified registry JSON file.</summary>
    [JsonPropertyName("registryPath")]
    public required string RegistryPath { get; init; }

    /// <summary>Optional path to the observation chain JSON file (array of ObservationChainRecord).</summary>
    [JsonPropertyName("observationChainPath")]
    public string? ObservationChainPath { get; init; }

    /// <summary>Optional path to the environment variance JSON file (array of EnvironmentVarianceRecord).</summary>
    [JsonPropertyName("environmentVariancePath")]
    public string? EnvironmentVariancePath { get; init; }

    /// <summary>Optional path to the representation content JSON file (array of RepresentationContentRecord).</summary>
    [JsonPropertyName("representationContentPath")]
    public string? RepresentationContentPath { get; init; }

    /// <summary>Optional path to the coupling consistency JSON file (array of CouplingConsistencyRecord).</summary>
    [JsonPropertyName("couplingConsistencyPath")]
    public string? CouplingConsistencyPath { get; init; }

    /// <summary>Optional path to explicit target coverage blocker records.</summary>
    [JsonPropertyName("targetCoverageBlockersPath")]
    public string? TargetCoverageBlockersPath { get; init; }
}

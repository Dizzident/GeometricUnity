using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// Top-level Phase V validation dossier aggregating all Phase V study outputs (M51+M52).
///
/// This is the richly-typed dossier produced by Phase5DossierAssembler, incorporating
/// M46-M50 study results, claim escalation records, and negative result ledger entries.
///
/// The simpler <see cref="ValidationDossier"/> tracks study manifests and G-006 compliance.
/// This type carries the full technical content of a Phase V study.
/// </summary>
public sealed class Phase5ValidationDossier
{
    /// <summary>Unique dossier identifier.</summary>
    [JsonPropertyName("dossierId")]
    public required string DossierId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Study identifier that produced this dossier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>
    /// Branch robustness record from M46 (may be null if study not run).
    /// </summary>
    [JsonPropertyName("branchFamilySummary")]
    public BranchRobustnessRecord? BranchFamilySummary { get; init; }

    /// <summary>
    /// Continuum estimate records from M47 (may be null or empty).
    /// </summary>
    [JsonPropertyName("refinementSummary")]
    public IReadOnlyList<ContinuumEstimateRecord>? RefinementSummary { get; init; }

    /// <summary>
    /// Convergence failure records from M47 (may be null or empty).
    /// </summary>
    [JsonPropertyName("convergenceFailures")]
    public IReadOnlyList<ConvergenceFailureRecord>? ConvergenceFailures { get; init; }

    /// <summary>
    /// Environment records from M48 (may be null or empty).
    /// </summary>
    [JsonPropertyName("environmentSummary")]
    public IReadOnlyList<EnvironmentRecord>? EnvironmentSummary { get; init; }

    /// <summary>
    /// Quantitative consistency scorecard from M49 (may be null if not run).
    /// </summary>
    [JsonPropertyName("quantitativeComparison")]
    public ConsistencyScoreCard? QuantitativeComparison { get; init; }

    /// <summary>
    /// Falsifier summary from M50 (may be null if not run).
    /// </summary>
    [JsonPropertyName("falsifierSummary")]
    public FalsifierSummary? FalsifierSummary { get; init; }

    /// <summary>
    /// Claim escalation records (M51), one per registry candidate evaluated.
    /// </summary>
    [JsonPropertyName("claimEscalations")]
    public required IReadOnlyList<ClaimEscalationRecord> ClaimEscalations { get; init; }

    /// <summary>
    /// Negative result entries (M52) from falsifiers and convergence failures.
    /// </summary>
    [JsonPropertyName("negativeResults")]
    public required IReadOnlyList<NegativeResultEntry> NegativeResults { get; init; }

    /// <summary>
    /// Observation chain summary records (WP-6, D-007).
    /// One record per candidate/observable pair evaluated.
    /// Gate ObservationChainValid: at least one record with
    ///   CompletenessStatus="complete", Passed=true, SensitivityScore &lt;= 0.3,
    ///   AuxiliaryModelSensitivity &lt;= 0.3.
    /// </summary>
    [JsonPropertyName("observationChainSummary")]
    public IReadOnlyList<ObservationChainRecord>? ObservationChainSummary { get; init; }

    /// <summary>
    /// Freshness descriptor: "regenerated-current-code" if all studies were run
    /// from the current tree; "mixed" if some studies are stale.
    /// </summary>
    [JsonPropertyName("freshness")]
    public required string Freshness { get; init; }

    /// <summary>When this dossier was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>
    /// True if all escalation gates passed for at least one candidate.
    /// </summary>
    [JsonIgnore]
    public bool HasAnyEscalation => ClaimEscalations.Any(e => e.Direction == "escalation");

    /// <summary>
    /// True if any fatal falsifier was active.
    /// </summary>
    [JsonIgnore]
    public bool HasFatalFalsifier => FalsifierSummary?.ActiveFatalCount > 0;

    /// <summary>
    /// Count of negative results implying candidate demotion.
    /// </summary>
    [JsonIgnore]
    public int DemotionCount => NegativeResults.Count(n => n.ImpliesDemotion);
}

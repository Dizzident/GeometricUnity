using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase3.Reporting;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Top-level Phase V validation report (M53).
/// Aggregates branch independence, convergence, falsification, and negative-result
/// summaries from a complete Phase V campaign.
/// </summary>
public sealed class Phase5Report
{
    /// <summary>Unique report identifier.</summary>
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    /// <summary>Study identifier this report covers.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Dossier IDs included in this report.</summary>
    [JsonPropertyName("dossierIds")]
    public required IReadOnlyList<string> DossierIds { get; init; }

    /// <summary>Branch-independence atlas (null if not computed).</summary>
    [JsonPropertyName("branchIndependenceAtlas")]
    public BranchIndependenceAtlas? BranchIndependenceAtlas { get; init; }

    /// <summary>Convergence atlas (null if not computed).</summary>
    [JsonPropertyName("convergenceAtlas")]
    public ConvergenceAtlas? ConvergenceAtlas { get; init; }

    /// <summary>Falsification dashboard (null if not computed).</summary>
    [JsonPropertyName("falsificationDashboard")]
    public FalsificationDashboard? FalsificationDashboard { get; init; }

    /// <summary>Negative results preserved from all phases.</summary>
    [JsonPropertyName("negativeResultSummary")]
    public IReadOnlyList<NegativeResultEntry>? NegativeResultSummary { get; init; }

    /// <summary>
    /// Shiab family scope record (P11-M9).
    /// Documents which Shiab operators were exercised, whether they are mathematically
    /// distinct, and the artifact-backed reason why expansion is blocked in repository context.
    /// Per D-P11-009: neither current operator is the canonical draft operator.
    /// Null when not computed for this report.
    /// </summary>
    [JsonPropertyName("shiabFamilyScope")]
    public ShiabFamilyScopeRecord? ShiabFamilyScope { get; init; }

    /// <summary>
    /// Geometry evidence label (P11-M7).
    /// "toy-control" when the campaign uses toy/structured low-dimensional geometry.
    /// "draft-aligned" only when the campaign uses geometry consistent with the draft X^4/Observerse.
    /// Per D-P11-007: current evidence is toy-control only.
    /// Null when not set.
    /// </summary>
    [JsonPropertyName("geometryEvidenceLabel")]
    public string? GeometryEvidenceLabel { get; init; }

    /// <summary>
    /// Explicit statement blocking X^4/Observerse recovery claims (P11-M7).
    /// Populated when GeometryEvidenceLabel is "toy-control".
    /// </summary>
    [JsonPropertyName("observerseRecoveryBlock")]
    public string? ObserverseRecoveryBlock { get; init; }

    /// <summary>Phase XVI observable classifications included in the report.</summary>
    [JsonPropertyName("observableClassifications")]
    public ObservableClassificationTable? ObservableClassifications { get; init; }

    /// <summary>Phase XVI fail-closed gate for real boson-prediction language.</summary>
    [JsonPropertyName("physicalClaimGate")]
    public PhysicalClaimGate? PhysicalClaimGate { get; init; }

    /// <summary>Candidate physical prediction records. Blocked records are diagnostics only.</summary>
    [JsonPropertyName("physicalPredictions")]
    public IReadOnlyList<PhysicalPredictionRecord>? PhysicalPredictions { get; init; }

    /// <summary>Terminal physical prediction status: blocked, predicted, or failed.</summary>
    [JsonPropertyName("physicalPredictionTerminalStatus")]
    public PhysicalPredictionTerminalStatus? PhysicalPredictionTerminalStatus { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Timestamp when this report was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Serialize to JSON.</summary>
    public string ToJson(bool indented = true)
        => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = indented });

    /// <summary>Deserialize from JSON.</summary>
    public static Phase5Report FromJson(string json)
        => JsonSerializer.Deserialize<Phase5Report>(json,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
           ?? throw new InvalidOperationException("Failed to deserialize Phase5Report.");
}

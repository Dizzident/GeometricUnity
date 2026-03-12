using System.Text.Json.Serialization;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Summary of the fermion family atlas for reporting purposes.
/// Mirrors the Phase III BosonAtlasReport pattern for the fermionic sector.
///
/// Note: This is distinct from <see cref="Gu.Phase4.FamilyClustering.FermionAtlasSummary"/>
/// which is a statistical summary on the atlas artifact itself. This type is the
/// reporting-layer narrative summary.
/// </summary>
public sealed class FermionAtlasSummary
{
    /// <summary>Unique summary identifier.</summary>
    [JsonPropertyName("summaryId")]
    public required string SummaryId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Per-family spectral sheets.</summary>
    [JsonPropertyName("familySheets")]
    public required List<FermionFamilySheet> FamilySheets { get; init; }

    /// <summary>Per-family chirality summaries.</summary>
    [JsonPropertyName("chiralitySummaries")]
    public required List<ChiralitySummaryEntry> ChiralitySummaries { get; init; }

    /// <summary>Per-family conjugation summaries.</summary>
    [JsonPropertyName("conjugationSummaries")]
    public required List<ConjugationSummaryEntry> ConjugationSummaries { get; init; }

    /// <summary>Total number of fermion families tracked.</summary>
    [JsonPropertyName("totalFamilies")]
    public required int TotalFamilies { get; init; }

    /// <summary>Number of families with branch persistence score above 0.5.</summary>
    [JsonPropertyName("stableFamilies")]
    public required int StableFamilies { get; init; }

    /// <summary>Number of families with ambiguity score above 0.</summary>
    [JsonPropertyName("ambiguousFamilies")]
    public required int AmbiguousFamilies { get; init; }
}

/// <summary>
/// Spectral sheet for a single fermion mode family.
/// </summary>
public sealed class FermionFamilySheet
{
    /// <summary>Family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>Mean eigenvalue magnitude across member modes.</summary>
    [JsonPropertyName("meanEigenvalue")]
    public required double MeanEigenvalue { get; init; }

    /// <summary>Spread (max-min) of eigenvalue magnitudes within this family.</summary>
    [JsonPropertyName("eigenvalueSpread")]
    public required double EigenvalueSpread { get; init; }

    /// <summary>Number of member modes.</summary>
    [JsonPropertyName("memberCount")]
    public required int MemberCount { get; init; }

    /// <summary>Whether this family has branch persistence score above 0.5.</summary>
    [JsonPropertyName("isStable")]
    public required bool IsStable { get; init; }

    /// <summary>Claim class for this family (C0-C5).</summary>
    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    /// <summary>IDs of member modes.</summary>
    [JsonPropertyName("memberModeIds")]
    public required List<string> MemberModeIds { get; init; }
}

/// <summary>
/// Chirality summary for a single fermion family.
/// </summary>
public sealed class ChiralitySummaryEntry
{
    /// <summary>Family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>Aggregate left-chiral projection weight.</summary>
    [JsonPropertyName("leftProjection")]
    public required double LeftProjection { get; init; }

    /// <summary>Aggregate right-chiral projection weight.</summary>
    [JsonPropertyName("rightProjection")]
    public required double RightProjection { get; init; }

    /// <summary>Gauge leakage norm (0 = no leakage).</summary>
    [JsonPropertyName("leakageNorm")]
    public required double LeakageNorm { get; init; }

    /// <summary>Chirality type label: "Y", "X", or "F".</summary>
    [JsonPropertyName("chiralityType")]
    public required string ChiralityType { get; init; }

    /// <summary>
    /// Chirality status: "definite-left", "definite-right", "mixed",
    /// "trivial" (odd dimY), or "not-applicable".
    /// </summary>
    [JsonPropertyName("chiralityStatus")]
    public required string ChiralityStatus { get; init; }

    /// <summary>Optional diagnostic notes from chirality analysis.</summary>
    [JsonPropertyName("diagnosticNotes")]
    public List<string>? DiagnosticNotes { get; init; }
}

/// <summary>
/// Conjugation pair summary for a single fermion family.
/// </summary>
public sealed class ConjugationSummaryEntry
{
    /// <summary>Family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>Whether this family has a conjugate pair family.</summary>
    [JsonPropertyName("hasConjugatePair")]
    public required bool HasConjugatePair { get; init; }

    /// <summary>ID of the paired family, or null if no pair.</summary>
    [JsonPropertyName("pairedFamilyId")]
    public required string? PairedFamilyId { get; init; }

    /// <summary>Pairing score [0, 1]: 0 = no match, 1 = perfect conjugate pair.</summary>
    [JsonPropertyName("pairingScore")]
    public required double PairingScore { get; init; }
}

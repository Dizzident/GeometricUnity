using Gu.Phase2.Semantics;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Factory for creating canonicity dockets for the minimum required object classes.
/// </summary>
public static class CanonicityDocketBuilder
{
    /// <summary>Minimum required object classes per Section 5.5.</summary>
    public static readonly IReadOnlyList<string> RequiredObjectClasses = new[]
    {
        "A0",
        "torsion",
        "shiab",
        "observation-extraction",
        "regularity",
        "gauge-fixing",
    };

    /// <summary>
    /// Create an initial open docket for a given object class.
    /// All parameters are explicit -- no hidden defaults.
    /// </summary>
    public static CanonicityDocket CreateOpen(
        string objectClass,
        string activeRepresentative,
        string equivalenceRelationId,
        string admissibleComparisonClass)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectClass);
        ArgumentException.ThrowIfNullOrWhiteSpace(activeRepresentative);
        ArgumentException.ThrowIfNullOrWhiteSpace(equivalenceRelationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(admissibleComparisonClass);

        return new CanonicityDocket
        {
            ObjectClass = objectClass,
            ActiveRepresentative = activeRepresentative,
            EquivalenceRelationId = equivalenceRelationId,
            AdmissibleComparisonClass = admissibleComparisonClass,
            DownstreamClaimsBlockedUntilClosure = Array.Empty<string>(),
            CurrentEvidence = Array.Empty<CanonicityEvidenceRecord>(),
            KnownCounterexamples = Array.Empty<string>(),
            PendingTheorems = Array.Empty<string>(),
            StudyReports = Array.Empty<string>(),
            Status = DocketStatus.Open,
        };
    }

    /// <summary>
    /// Create the minimum set of open dockets for all required object classes.
    /// Uses "pending-assignment" for fields that must be filled in by study configuration.
    /// </summary>
    public static IReadOnlyList<CanonicityDocket> CreateMinimumSet()
    {
        return RequiredObjectClasses
            .Select(oc => CreateOpen(
                oc,
                activeRepresentative: "pending-assignment",
                equivalenceRelationId: "pending-assignment",
                admissibleComparisonClass: "pending-assignment"))
            .ToList();
    }
}

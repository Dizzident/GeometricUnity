using Gu.Phase2.Semantics;

namespace Gu.Phase2.Recovery;

/// <summary>
/// Enforces the physical-identification gate (Section 9.8).
/// Every attempt to classify an extraction output as physics must pass through this gate.
///
/// Gate rules:
/// - Missing Falsifier (empty/whitespace) -> Inadmissible
/// - Missing FormalSource (empty/whitespace) -> Inadmissible
/// - SupportStatus == "conjectural" -> max SpeculativeInterpretation
/// - ApproximationStatus not declared (empty/whitespace) -> demote one level
/// - All fields present + SupportStatus == "theorem-supported" -> allow ExactStructuralConsequence
///
/// Evaluate returns a NEW record with ResolvedClaimClass potentially demoted.
/// It never mutates the input. It never throws -- it demotes.
/// </summary>
public static class IdentificationGate
{
    /// <summary>
    /// Evaluate a physical-identification record and return a new record
    /// with ResolvedClaimClass set according to gate rules.
    /// </summary>
    public static PhysicalIdentificationRecord Evaluate(PhysicalIdentificationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var resolvedClass = ComputeClaimClass(record);
        return CopyWithResolvedClass(record, resolvedClass);
    }

    private static ClaimClass ComputeClaimClass(PhysicalIdentificationRecord record)
    {
        // Missing Falsifier -> Inadmissible
        if (string.IsNullOrWhiteSpace(record.Falsifier))
            return ClaimClass.Inadmissible;

        // Missing FormalSource -> Inadmissible
        if (string.IsNullOrWhiteSpace(record.FormalSource))
            return ClaimClass.Inadmissible;

        // Missing ObservationExtractionMap -> Inadmissible
        if (string.IsNullOrWhiteSpace(record.ObservationExtractionMap))
            return ClaimClass.Inadmissible;

        // Missing SupportStatus -> Inadmissible
        if (string.IsNullOrWhiteSpace(record.SupportStatus))
            return ClaimClass.Inadmissible;

        // Missing ComparisonTarget -> Inadmissible
        if (string.IsNullOrWhiteSpace(record.ComparisonTarget))
            return ClaimClass.Inadmissible;

        // SupportStatus == "conjectural" -> max SpeculativeInterpretation
        if (record.SupportStatus == "conjectural")
        {
            if (string.IsNullOrWhiteSpace(record.ApproximationStatus))
                return Demote(ClaimClass.SpeculativeInterpretation);
            return ClaimClass.SpeculativeInterpretation;
        }

        // ApproximationStatus not declared -> demote one level from ceiling
        if (string.IsNullOrWhiteSpace(record.ApproximationStatus))
        {
            var ceiling = record.SupportStatus == "theorem-supported"
                ? ClaimClass.ExactStructuralConsequence
                : ClaimClass.ApproximateStructuralSurrogate;
            return Demote(ceiling);
        }

        // All fields present + SupportStatus == "theorem-supported" -> ExactStructuralConsequence
        if (record.SupportStatus == "theorem-supported")
            return ClaimClass.ExactStructuralConsequence;

        // All fields present + SupportStatus == "numerical-only" -> ApproximateStructuralSurrogate
        if (record.SupportStatus == "numerical-only")
            return ClaimClass.ApproximateStructuralSurrogate;

        // Default: PostdictionTarget for unrecognized but present support status
        return ClaimClass.PostdictionTarget;
    }

    private static ClaimClass Demote(ClaimClass current)
    {
        return current switch
        {
            ClaimClass.ExactStructuralConsequence => ClaimClass.ApproximateStructuralSurrogate,
            ClaimClass.ApproximateStructuralSurrogate => ClaimClass.PostdictionTarget,
            ClaimClass.PostdictionTarget => ClaimClass.SpeculativeInterpretation,
            ClaimClass.SpeculativeInterpretation => ClaimClass.Inadmissible,
            _ => ClaimClass.Inadmissible,
        };
    }

    private static PhysicalIdentificationRecord CopyWithResolvedClass(
        PhysicalIdentificationRecord source, ClaimClass resolvedClass)
    {
        return new PhysicalIdentificationRecord
        {
            IdentificationId = source.IdentificationId,
            FormalSource = source.FormalSource,
            ObservationExtractionMap = source.ObservationExtractionMap,
            SupportStatus = source.SupportStatus,
            ApproximationStatus = source.ApproximationStatus,
            ComparisonTarget = source.ComparisonTarget,
            Falsifier = source.Falsifier,
            ResolvedClaimClass = resolvedClass,
        };
    }
}

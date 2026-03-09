using Gu.Phase2.Semantics;

namespace Gu.Phase2.Predictions;

/// <summary>
/// Enforces the 5 reading rules from Section 9.9.
/// Returns a new PredictionTestRecord with ClaimClass potentially demoted.
/// Never throws -- it demotes or marks Inadmissible.
/// </summary>
public static class PredictionValidator
{
    /// <summary>
    /// Validate a prediction test record against the 5 reading rules.
    /// Returns a new record with ClaimClass potentially demoted.
    /// </summary>
    public static PredictionValidationResult Validate(PredictionTestRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var violations = new List<string>();
        var claimClass = record.ClaimClass;

        // Rule 3: missing falsifier -> Inadmissible (hard reject, check first)
        if (string.IsNullOrWhiteSpace(record.Falsifier))
        {
            violations.Add("Rule 3: Missing falsifier -- invalid prediction record");
            return new PredictionValidationResult
            {
                Record = CopyWithClaimClass(record, ClaimClass.Inadmissible),
                IsValid = false,
                Violations = violations,
            };
        }

        // Rule 4: incomplete branch manifest -> Inadmissible
        if (string.IsNullOrWhiteSpace(record.BranchManifestId))
        {
            violations.Add("Rule 4: Incomplete branch manifest -- invalid reproducible evidence");
            return new PredictionValidationResult
            {
                Record = CopyWithClaimClass(record, ClaimClass.Inadmissible),
                IsValid = false,
                Violations = violations,
            };
        }

        // Rule 1: open theorem dependency -> cannot be ExactStructuralConsequence
        if (record.TheoremDependencyStatus == "open" &&
            claimClass == ClaimClass.ExactStructuralConsequence)
        {
            violations.Add("Rule 1: Open theorem dependency -- demoted from ExactStructuralConsequence");
            claimClass = ClaimClass.ApproximateStructuralSurrogate;
        }

        // Rule 2: missing external asset -> internal result only (no comparison)
        if (record.ExternalComparisonAsset == null)
        {
            violations.Add("Rule 2: Missing external asset -- internal result only, not comparison-ready");
        }

        // Rule 5: exploratory numerical status -> not comparison-ready
        if (record.NumericalDependencyStatus == "exploratory" ||
            record.NumericalDependencyStatus == "failed")
        {
            violations.Add($"Rule 5: Numerical status '{record.NumericalDependencyStatus}' -- not comparison-ready");
            if (claimClass < ClaimClass.PostdictionTarget)
            {
                claimClass = ClaimClass.PostdictionTarget;
            }
        }

        bool isValid = violations.Count == 0;
        return new PredictionValidationResult
        {
            Record = claimClass != record.ClaimClass
                ? CopyWithClaimClass(record, claimClass)
                : record,
            IsValid = isValid,
            Violations = violations,
        };
    }

    private static PredictionTestRecord CopyWithClaimClass(
        PredictionTestRecord source, ClaimClass claimClass)
    {
        return new PredictionTestRecord
        {
            TestId = source.TestId,
            ClaimClass = claimClass,
            FormalSource = source.FormalSource,
            BranchManifestId = source.BranchManifestId,
            ObservableMapId = source.ObservableMapId,
            TheoremDependencyStatus = source.TheoremDependencyStatus,
            NumericalDependencyStatus = source.NumericalDependencyStatus,
            ApproximationStatus = source.ApproximationStatus,
            ExternalComparisonAsset = source.ExternalComparisonAsset,
            Falsifier = source.Falsifier,
            ArtifactLinks = source.ArtifactLinks,
            Notes = source.Notes,
        };
    }
}

/// <summary>
/// Result of PredictionValidator.Validate().
/// </summary>
public sealed class PredictionValidationResult
{
    /// <summary>The (possibly demoted) prediction test record.</summary>
    public required PredictionTestRecord Record { get; init; }

    /// <summary>Whether the record passed all reading rules without demotion.</summary>
    public required bool IsValid { get; init; }

    /// <summary>List of rule violations encountered.</summary>
    public required IReadOnlyList<string> Violations { get; init; }
}

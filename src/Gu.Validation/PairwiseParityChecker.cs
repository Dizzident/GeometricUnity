using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Validation;

/// <summary>
/// Compares two runs (e.g., CPU vs CUDA) for numerical parity.
/// Produces a ValidationBundle with per-field comparison results.
/// </summary>
public sealed class PairwiseParityChecker
{
    /// <summary>Default tolerance for parity checks.</summary>
    public const double DefaultTolerance = 1e-10;

    private readonly double _tolerance;

    /// <summary>
    /// Create a parity checker with the given tolerance.
    /// </summary>
    public PairwiseParityChecker(double tolerance = DefaultTolerance)
    {
        _tolerance = tolerance;
    }

    /// <summary>
    /// Compare two ArtifactBundles for numerical parity.
    /// Compares validation bundle results and observed state values.
    /// Returns a ValidationBundle with per-field parity results.
    /// </summary>
    public ValidationBundle Compare(ArtifactBundle bundleA, ArtifactBundle bundleB, BranchRef branch)
    {
        var records = new List<ValidationRecord>();

        // Compare validation outcomes
        if (bundleA.ValidationBundle is not null && bundleB.ValidationBundle is not null)
        {
            records.Add(CompareValidationOutcomes(bundleA.ValidationBundle, bundleB.ValidationBundle));
        }

        // Compare observed state values
        if (bundleA.ObservedState is not null && bundleB.ObservedState is not null)
        {
            records.AddRange(CompareObservedStates(bundleA.ObservedState, bundleB.ObservedState));
        }

        // Compare integrity hashes
        if (bundleA.Integrity is not null && bundleB.Integrity is not null)
        {
            records.Add(CompareIntegrity(bundleA.Integrity, bundleB.Integrity));
        }

        return new ValidationBundle
        {
            Branch = branch,
            Records = records,
            AllPassed = records.All(r => r.Passed),
        };
    }

    /// <summary>
    /// Compare two FieldTensors for numerical parity.
    /// Returns a ValidationRecord with max deviation and pass/fail.
    /// </summary>
    public ValidationRecord CompareFields(FieldTensor fieldA, FieldTensor fieldB, string fieldLabel)
    {
        if (fieldA.Coefficients.Length != fieldB.Coefficients.Length)
        {
            return new ValidationRecord
            {
                RuleId = $"parity-{fieldLabel}",
                Category = ValidationRuleRegistry.Parity,
                Passed = false,
                Detail = $"Coefficient length mismatch: {fieldA.Coefficients.Length} vs {fieldB.Coefficients.Length}",
                Timestamp = DateTimeOffset.UtcNow,
            };
        }

        double maxDiff = 0;
        for (int i = 0; i < fieldA.Coefficients.Length; i++)
        {
            double diff = System.Math.Abs(fieldA.Coefficients[i] - fieldB.Coefficients[i]);
            if (diff > maxDiff)
                maxDiff = diff;
        }

        bool passed = maxDiff <= _tolerance;
        return new ValidationRecord
        {
            RuleId = $"parity-{fieldLabel}",
            Category = ValidationRuleRegistry.Parity,
            Passed = passed,
            MeasuredValue = maxDiff,
            Tolerance = _tolerance,
            Detail = passed
                ? $"Field '{fieldLabel}' matches within tolerance (maxDiff={maxDiff:E6})"
                : $"Field '{fieldLabel}' exceeds tolerance (maxDiff={maxDiff:E6}, tolerance={_tolerance:E3})",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Compare two DerivedStates for numerical parity across all derived fields.
    /// </summary>
    public ValidationBundle CompareDerivedStates(DerivedState stateA, DerivedState stateB, BranchRef branch)
    {
        var records = new List<ValidationRecord>
        {
            CompareFields(stateA.CurvatureF, stateB.CurvatureF, "curvatureF"),
            CompareFields(stateA.TorsionT, stateB.TorsionT, "torsionT"),
            CompareFields(stateA.ShiabS, stateB.ShiabS, "shiabS"),
            CompareFields(stateA.ResidualUpsilon, stateB.ResidualUpsilon, "residualUpsilon"),
        };

        return new ValidationBundle
        {
            Branch = branch,
            Records = records,
            AllPassed = records.All(r => r.Passed),
        };
    }

    private ValidationRecord CompareValidationOutcomes(ValidationBundle a, ValidationBundle b)
    {
        bool match = a.AllPassed == b.AllPassed;
        return new ValidationRecord
        {
            RuleId = "parity-validation-outcome",
            Category = ValidationRuleRegistry.Parity,
            Passed = match,
            Detail = match
                ? $"Validation outcomes match: AllPassed={a.AllPassed}"
                : $"Validation outcome mismatch: A.AllPassed={a.AllPassed}, B.AllPassed={b.AllPassed}",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    private IReadOnlyList<ValidationRecord> CompareObservedStates(ObservedState a, ObservedState b)
    {
        var records = new List<ValidationRecord>();

        // Find common observable IDs
        var commonIds = a.Observables.Keys.Intersect(b.Observables.Keys).ToList();
        var onlyInA = a.Observables.Keys.Except(b.Observables.Keys).ToList();
        var onlyInB = b.Observables.Keys.Except(a.Observables.Keys).ToList();

        foreach (var id in commonIds)
        {
            var snapshotA = a.Observables[id];
            var snapshotB = b.Observables[id];
            records.Add(CompareObservableValues(id, snapshotA.Values, snapshotB.Values));
        }

        if (onlyInA.Count > 0 || onlyInB.Count > 0)
        {
            records.Add(new ValidationRecord
            {
                RuleId = "parity-observable-coverage",
                Category = ValidationRuleRegistry.Parity,
                Passed = false,
                Detail = $"Observable set mismatch. Only in A: [{string.Join(", ", onlyInA)}]; Only in B: [{string.Join(", ", onlyInB)}]",
                Timestamp = DateTimeOffset.UtcNow,
            });
        }

        return records;
    }

    private ValidationRecord CompareObservableValues(string observableId, double[] valuesA, double[] valuesB)
    {
        if (valuesA.Length != valuesB.Length)
        {
            return new ValidationRecord
            {
                RuleId = $"parity-observable-{observableId}",
                Category = ValidationRuleRegistry.Parity,
                Passed = false,
                Detail = $"Observable '{observableId}' length mismatch: {valuesA.Length} vs {valuesB.Length}",
                Timestamp = DateTimeOffset.UtcNow,
            };
        }

        double maxDiff = 0;
        for (int i = 0; i < valuesA.Length; i++)
        {
            double diff = System.Math.Abs(valuesA[i] - valuesB[i]);
            if (diff > maxDiff)
                maxDiff = diff;
        }

        bool passed = maxDiff <= _tolerance;
        return new ValidationRecord
        {
            RuleId = $"parity-observable-{observableId}",
            Category = ValidationRuleRegistry.Parity,
            Passed = passed,
            MeasuredValue = maxDiff,
            Tolerance = _tolerance,
            Detail = passed
                ? $"Observable '{observableId}' matches within tolerance (maxDiff={maxDiff:E6})"
                : $"Observable '{observableId}' exceeds tolerance (maxDiff={maxDiff:E6})",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    private ValidationRecord CompareIntegrity(IntegrityBundle a, IntegrityBundle b)
    {
        bool match = string.Equals(a.ContentHash, b.ContentHash, StringComparison.OrdinalIgnoreCase);
        return new ValidationRecord
        {
            RuleId = "parity-integrity-hash",
            Category = ValidationRuleRegistry.Parity,
            Passed = match,
            Detail = match
                ? "Content hashes match"
                : $"Content hash mismatch: {a.ContentHash} vs {b.ContentHash}",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }
}

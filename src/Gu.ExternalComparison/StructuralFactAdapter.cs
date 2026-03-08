using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Adapter for GU-specific structural consistency checks.
/// Handles falsifier conditions like Bianchi identity, carrier mismatch,
/// gauge covariance, and other structural facts that don't compare against
/// external reference data but verify internal consistency.
/// </summary>
public sealed class StructuralFactAdapter : IComparisonAdapter
{
    public string AdapterType => "structural_fact";

    public bool CanHandle(ComparisonTemplate template)
        => template.AdapterType == AdapterType;

    public ComparisonRecord Compare(
        ObservableSnapshot observable,
        ComparisonTemplate template,
        BranchManifest branch)
    {
        ArgumentNullException.ThrowIfNull(observable);
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(branch);

        var branchRef = new BranchRef
        {
            BranchId = branch.BranchId,
            SchemaVersion = branch.SchemaVersion,
        };

        var provenance = observable.Provenance!;

        return template.ComparisonRule switch
        {
            "structural_match" => EvaluateStructuralMatch(observable, template, branchRef, provenance, branch.CodeRevision),
            "norm_bound" => EvaluateNormBound(observable, template, branchRef, provenance, branch.CodeRevision),
            "integer_check" => EvaluateIntegerCheck(observable, template, branchRef, provenance, branch.CodeRevision),
            "count_match" => EvaluateCountMatch(observable, template, branchRef, provenance, branch.CodeRevision),
            _ => ComparisonRecord.CreateInvalid(
                template, branchRef,
                new ProvenanceMeta
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    CodeRevision = branch.CodeRevision,
                    Branch = branchRef,
                },
                $"Unknown comparison rule '{template.ComparisonRule}' for structural_fact adapter."),
        };
    }

    private static ComparisonRecord EvaluateStructuralMatch(
        ObservableSnapshot observable,
        ComparisonTemplate template,
        BranchRef branchRef,
        ObservationProvenance provenance,
        string codeRevision)
    {
        // For structural match, we check that observed values are all near zero
        // (e.g., Bianchi identity residual, gauge covariance deviation)
        double maxDeviation = 0.0;
        double l2Norm = 0.0;

        foreach (var v in observable.Values)
        {
            double abs = System.Math.Abs(v);
            if (abs > maxDeviation)
                maxDeviation = abs;
            l2Norm += v * v;
        }
        l2Norm = System.Math.Sqrt(l2Norm);

        bool passed = EvaluateTolerance(maxDeviation, template.Tolerance);
        var metrics = new Dictionary<string, double>
        {
            ["maxDeviation"] = maxDeviation,
            ["l2Norm"] = l2Norm,
            ["elementCount"] = observable.Values.Length,
        };

        return CreateRecord(template, branchRef, provenance, codeRevision,
            passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail,
            metrics,
            passed
                ? $"Structural match passed: max deviation {maxDeviation:E3}"
                : $"Structural match failed: max deviation {maxDeviation:E3} exceeds tolerance");
    }

    private static ComparisonRecord EvaluateNormBound(
        ObservableSnapshot observable,
        ComparisonTemplate template,
        BranchRef branchRef,
        ObservationProvenance provenance,
        string codeRevision)
    {
        double l2Norm = 0.0;
        foreach (var v in observable.Values)
            l2Norm += v * v;
        l2Norm = System.Math.Sqrt(l2Norm);

        bool passed = EvaluateTolerance(l2Norm, template.Tolerance);
        var metrics = new Dictionary<string, double>
        {
            ["l2Norm"] = l2Norm,
            ["elementCount"] = observable.Values.Length,
        };

        return CreateRecord(template, branchRef, provenance, codeRevision,
            passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail,
            metrics,
            passed
                ? $"Norm bound satisfied: ||v|| = {l2Norm:E3}"
                : $"Norm bound violated: ||v|| = {l2Norm:E3} exceeds tolerance");
    }

    private static ComparisonRecord EvaluateIntegerCheck(
        ObservableSnapshot observable,
        ComparisonTemplate template,
        BranchRef branchRef,
        ObservationProvenance provenance,
        string codeRevision)
    {
        // Check that each value is close to an integer
        double maxFractionalPart = 0.0;
        foreach (var v in observable.Values)
        {
            double fractional = System.Math.Abs(v - System.Math.Round(v));
            if (fractional > maxFractionalPart)
                maxFractionalPart = fractional;
        }

        bool passed = EvaluateTolerance(maxFractionalPart, template.Tolerance);
        var metrics = new Dictionary<string, double>
        {
            ["maxFractionalPart"] = maxFractionalPart,
            ["elementCount"] = observable.Values.Length,
        };

        return CreateRecord(template, branchRef, provenance, codeRevision,
            passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail,
            metrics,
            passed
                ? $"Integer check passed: max fractional part {maxFractionalPart:E3}"
                : $"Integer check failed: max fractional part {maxFractionalPart:E3} exceeds tolerance");
    }

    private static ComparisonRecord EvaluateCountMatch(
        ObservableSnapshot observable,
        ComparisonTemplate template,
        BranchRef branchRef,
        ObservationProvenance provenance,
        string codeRevision)
    {
        // Values[0] = observed count, tolerance.BaseValue = expected count
        double observedCount = observable.Values.Length > 0 ? observable.Values[0] : 0.0;
        double expectedCount = template.Tolerance.BaseValue;
        double deviation = System.Math.Abs(observedCount - expectedCount);

        bool passed = deviation < 0.5; // count must be exact (within rounding)
        var metrics = new Dictionary<string, double>
        {
            ["observedCount"] = observedCount,
            ["expectedCount"] = expectedCount,
            ["deviation"] = deviation,
        };

        return CreateRecord(template, branchRef, provenance, codeRevision,
            passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail,
            metrics,
            passed
                ? $"Count match passed: observed {observedCount}, expected {expectedCount}"
                : $"Count match failed: observed {observedCount}, expected {expectedCount}");
    }

    private static bool EvaluateTolerance(double measuredValue, TolerancePolicy tolerance)
    {
        return tolerance.BaseToleranceType switch
        {
            "RelativeDeviation" => measuredValue <= tolerance.BaseValue,
            "FactorBound" => measuredValue <= tolerance.BaseValue,
            "OrderEstimate" => measuredValue <= System.Math.Pow(10, tolerance.BaseValue),
            _ => measuredValue <= tolerance.BaseValue,
        };
    }

    private static ComparisonRecord CreateRecord(
        ComparisonTemplate template,
        BranchRef branchRef,
        ObservationProvenance provenance,
        string codeRevision,
        ComparisonOutcome outcome,
        Dictionary<string, double> metrics,
        string message)
    {
        return new ComparisonRecord
        {
            ComparisonId = Guid.NewGuid().ToString("N"),
            TemplateId = template.TemplateId,
            ObservableId = template.ObservableId,
            ReferenceSourceId = template.ReferenceSourceId,
            ReferenceVersion = "structural",
            Branch = branchRef,
            ComparisonRule = template.ComparisonRule,
            ComparisonScope = template.ComparisonScope,
            Outcome = outcome,
            Metrics = metrics,
            Message = message,
            PullbackOperatorId = provenance.PullbackOperatorId,
            ObservationBranchId = provenance.ObservationBranchId,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = codeRevision,
                Branch = branchRef,
            },
            ExecutedAt = DateTimeOffset.UtcNow,
        };
    }
}

using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Adapter for comparing observables against curated reference tables
/// (e.g., PDG data, known analytical results, literature values).
/// Reference data is provided via IReferenceDataSource.
/// </summary>
public sealed class CuratedReferenceTableAdapter : IComparisonAdapter
{
    private readonly IReferenceDataSource _referenceData;

    public CuratedReferenceTableAdapter(IReferenceDataSource referenceData)
    {
        _referenceData = referenceData ?? throw new ArgumentNullException(nameof(referenceData));
    }

    public string AdapterType => "curated_table";

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

        // Look up reference values
        var reference = _referenceData.GetReference(template.ReferenceSourceId, template.ObservableId);
        if (reference is null)
        {
            return ComparisonRecord.CreateInvalid(
                template, branchRef,
                new ProvenanceMeta
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    CodeRevision = branch.CodeRevision,
                    Branch = branchRef,
                },
                $"Reference data not found: source='{template.ReferenceSourceId}', observable='{template.ObservableId}'.");
        }

        return template.ComparisonRule switch
        {
            "relative_error" => EvaluateRelativeError(observable, reference, template, branchRef, provenance, branch.CodeRevision),
            "order_of_magnitude" => EvaluateOrderOfMagnitude(observable, reference, template, branchRef, provenance, branch.CodeRevision),
            _ => ComparisonRecord.CreateInvalid(
                template, branchRef,
                new ProvenanceMeta
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    CodeRevision = branch.CodeRevision,
                    Branch = branchRef,
                },
                $"Unknown comparison rule '{template.ComparisonRule}' for curated_table adapter."),
        };
    }

    private static ComparisonRecord EvaluateRelativeError(
        ObservableSnapshot observable,
        ReferenceDataEntry reference,
        ComparisonTemplate template,
        BranchRef branchRef,
        ObservationProvenance provenance,
        string codeRevision)
    {
        int obsLen = observable.Values.Length;
        int refLen = reference.Values.Length;
        int count = System.Math.Min(obsLen, refLen);
        double maxRelError = 0.0;
        double sumRelError = 0.0;

        for (int i = 0; i < count; i++)
        {
            double refVal = reference.Values[i];
            double obsVal = observable.Values[i];
            double relError = System.Math.Abs(refVal) > 1e-15
                ? System.Math.Abs(obsVal - refVal) / System.Math.Abs(refVal)
                : System.Math.Abs(obsVal - refVal);
            if (relError > maxRelError)
                maxRelError = relError;
            sumRelError += relError;
        }

        double avgRelError = count > 0 ? sumRelError / count : 0.0;
        bool passed = maxRelError <= template.Tolerance.BaseValue;

        // Detect length mismatch
        var (outcome, message) = BuildLengthAwareResult(
            obsLen, refLen, count, passed,
            passed
                ? $"Relative error check passed: max={maxRelError:E3}, avg={avgRelError:E3}"
                : $"Relative error check failed: max={maxRelError:E3} exceeds tolerance {template.Tolerance.BaseValue:E3}");

        var metrics = new Dictionary<string, double>
        {
            ["maxRelativeError"] = maxRelError,
            ["avgRelativeError"] = avgRelError,
            ["comparedElements"] = count,
            ["observableLength"] = obsLen,
            ["referenceLength"] = refLen,
        };

        return new ComparisonRecord
        {
            ComparisonId = Guid.NewGuid().ToString("N"),
            TemplateId = template.TemplateId,
            ObservableId = template.ObservableId,
            ReferenceSourceId = template.ReferenceSourceId,
            ReferenceVersion = reference.Version,
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

    private static ComparisonRecord EvaluateOrderOfMagnitude(
        ObservableSnapshot observable,
        ReferenceDataEntry reference,
        ComparisonTemplate template,
        BranchRef branchRef,
        ObservationProvenance provenance,
        string codeRevision)
    {
        int obsLen = observable.Values.Length;
        int refLen = reference.Values.Length;
        int count = System.Math.Min(obsLen, refLen);
        double maxOrderDiff = 0.0;

        for (int i = 0; i < count; i++)
        {
            double refVal = System.Math.Abs(reference.Values[i]);
            double obsVal = System.Math.Abs(observable.Values[i]);
            if (refVal > 1e-30 && obsVal > 1e-30)
            {
                double orderDiff = System.Math.Abs(System.Math.Log10(obsVal / refVal));
                if (orderDiff > maxOrderDiff)
                    maxOrderDiff = orderDiff;
            }
        }

        bool passed = maxOrderDiff <= template.Tolerance.BaseValue;

        // Detect length mismatch
        var (outcome, message) = BuildLengthAwareResult(
            obsLen, refLen, count, passed,
            passed
                ? $"Order-of-magnitude check passed: max difference {maxOrderDiff:F2} orders"
                : $"Order-of-magnitude check failed: max difference {maxOrderDiff:F2} orders exceeds tolerance {template.Tolerance.BaseValue:F2}");

        var metrics = new Dictionary<string, double>
        {
            ["maxOrderDifference"] = maxOrderDiff,
            ["comparedElements"] = count,
            ["observableLength"] = obsLen,
            ["referenceLength"] = refLen,
        };

        return new ComparisonRecord
        {
            ComparisonId = Guid.NewGuid().ToString("N"),
            TemplateId = template.TemplateId,
            ObservableId = template.ObservableId,
            ReferenceSourceId = template.ReferenceSourceId,
            ReferenceVersion = reference.Version,
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

    /// <summary>
    /// Helper to handle length mismatch between observable and reference arrays.
    /// If ratio > 2x, outcome is Invalid. Otherwise, adds a note to the message.
    /// </summary>
    private static (ComparisonOutcome outcome, string message) BuildLengthAwareResult(
        int obsLen, int refLen, int comparedCount, bool passed, string baseMessage)
    {
        if (obsLen != refLen)
        {
            double ratio = obsLen > 0 && refLen > 0
                ? (double)System.Math.Max(obsLen, refLen) / System.Math.Min(obsLen, refLen)
                : double.PositiveInfinity;

            if (ratio > 2.0)
            {
                return (ComparisonOutcome.Invalid,
                    $"Invalid: array length mismatch ratio {ratio:F1}x (observable={obsLen}, reference={refLen}), compared {comparedCount} elements");
            }

            return (passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail,
                $"{baseMessage} [note: length mismatch observable={obsLen}, reference={refLen}, compared {comparedCount}]");
        }

        return (passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail, baseMessage);
    }
}

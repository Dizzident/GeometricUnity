using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Adapter for comparing observables against previously computed benchmark data.
/// Uses the same IReferenceDataSource as CuratedReferenceTableAdapter but is
/// semantically distinct: benchmarks are computed (not curated) references.
/// </summary>
public sealed class SimulatedBenchmarkAdapter : IComparisonAdapter
{
    private readonly IReferenceDataSource _referenceData;

    public SimulatedBenchmarkAdapter(IReferenceDataSource referenceData)
    {
        _referenceData = referenceData ?? throw new ArgumentNullException(nameof(referenceData));
    }

    public string AdapterType => "simulated_benchmark";

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
                $"Benchmark data not found: source='{template.ReferenceSourceId}', observable='{template.ObservableId}'.");
        }

        // For simulated benchmarks, we compute element-wise relative error
        int obsLen = observable.Values.Length;
        int refLen = reference.Values.Length;
        int count = System.Math.Min(obsLen, refLen);
        double maxRelError = 0.0;
        double maxAbsError = 0.0;

        for (int i = 0; i < count; i++)
        {
            double refVal = reference.Values[i];
            double obsVal = observable.Values[i];
            double absError = System.Math.Abs(obsVal - refVal);

            if (absError > maxAbsError)
                maxAbsError = absError;

            double relError = System.Math.Abs(refVal) > 1e-15
                ? absError / System.Math.Abs(refVal)
                : absError;
            if (relError > maxRelError)
                maxRelError = relError;
        }

        bool passed = template.ComparisonRule switch
        {
            "relative_error" => maxRelError <= template.Tolerance.BaseValue,
            "absolute_error" => maxAbsError <= template.Tolerance.BaseValue,
            _ => maxRelError <= template.Tolerance.BaseValue,
        };

        // Detect length mismatch
        string baseMessage = passed
            ? $"Benchmark comparison passed: maxRel={maxRelError:E3}, maxAbs={maxAbsError:E3}"
            : $"Benchmark comparison failed: maxRel={maxRelError:E3}, maxAbs={maxAbsError:E3}";

        ComparisonOutcome outcome;
        string message;

        if (obsLen != refLen)
        {
            double ratio = obsLen > 0 && refLen > 0
                ? (double)System.Math.Max(obsLen, refLen) / System.Math.Min(obsLen, refLen)
                : double.PositiveInfinity;

            if (ratio > 2.0)
            {
                outcome = ComparisonOutcome.Invalid;
                message = $"Invalid: array length mismatch ratio {ratio:F1}x (observable={obsLen}, reference={refLen}), compared {count} elements";
            }
            else
            {
                outcome = passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail;
                message = $"{baseMessage} [note: length mismatch observable={obsLen}, reference={refLen}, compared {count}]";
            }
        }
        else
        {
            outcome = passed ? ComparisonOutcome.Pass : ComparisonOutcome.Fail;
            message = baseMessage;
        }

        var metrics = new Dictionary<string, double>
        {
            ["maxRelativeError"] = maxRelError,
            ["maxAbsoluteError"] = maxAbsError,
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
                CodeRevision = branch.CodeRevision,
                Branch = branchRef,
            },
            ExecutedAt = DateTimeOffset.UtcNow,
        };
    }
}

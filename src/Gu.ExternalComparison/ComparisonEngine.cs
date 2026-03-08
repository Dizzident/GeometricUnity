using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Orchestrates comparison execution: given an ObservedState and a set of
/// ComparisonTemplates, dispatches each to the appropriate adapter and collects results.
///
/// Enforces three layers of provenance:
///   1. Type-level: IComparisonAdapter.Compare() only accepts ObservableSnapshot
///   2. Provenance: rejects snapshots where ObservationProvenance.IsVerified != true
///   3. OutputType: rejects comparisons where observable OutputType &lt; template MinimumOutputType
///
/// Failed comparisons are never filtered -- they are first-class results (Section 18.4).
/// </summary>
public sealed class ComparisonEngine
{
    private readonly IReadOnlyList<IComparisonAdapter> _adapters;

    public ComparisonEngine(IReadOnlyList<IComparisonAdapter> adapters)
    {
        _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));
    }

    /// <summary>
    /// Run all comparisons defined by the templates against the observed state.
    /// Returns one ComparisonRecord per template -- never skips or filters.
    /// </summary>
    public IReadOnlyList<ComparisonRecord> RunComparisons(
        ObservedState observed,
        IReadOnlyList<ComparisonTemplate> templates,
        BranchManifest branch)
    {
        ArgumentNullException.ThrowIfNull(observed);
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(branch);

        var branchRef = new BranchRef
        {
            BranchId = branch.BranchId,
            SchemaVersion = branch.SchemaVersion,
        };

        var provenance = observed.Provenance;
        var records = new List<ComparisonRecord>(templates.Count);

        foreach (var template in templates)
        {
            var record = RunSingleComparison(observed, template, branch, branchRef, provenance);
            records.Add(record);
        }

        return records;
    }

    private ComparisonRecord RunSingleComparison(
        ObservedState observed,
        ComparisonTemplate template,
        BranchManifest branch,
        BranchRef branchRef,
        ProvenanceMeta provenance)
    {
        // Step 1: Look up the observable snapshot
        if (!observed.Observables.TryGetValue(template.ObservableId, out var snapshot))
        {
            return ComparisonRecord.CreateInvalid(
                template, branchRef, provenance,
                $"Observable '{template.ObservableId}' not found in ObservedState.");
        }

        // Step 2: Verify observation provenance (IA-6 enforcement)
        if (snapshot.Provenance is not { IsVerified: true })
        {
            return ComparisonRecord.CreateInvalid(
                template, branchRef, provenance,
                $"Observable '{template.ObservableId}' lacks verified ObservationProvenance. " +
                "Data must pass through sigma_h* pipeline before comparison.");
        }

        // Step 3: OutputType guard (Section 17.4)
        if (snapshot.OutputType < template.MinimumOutputType)
        {
            return ComparisonRecord.CreateInvalid(
                template, branchRef, provenance,
                $"Output type mismatch: template requires {template.MinimumOutputType} " +
                $"but observable '{template.ObservableId}' is {snapshot.OutputType}.");
        }

        // Step 4: Find a matching adapter
        var adapter = FindAdapter(template);
        if (adapter is null)
        {
            return ComparisonRecord.CreateInvalid(
                template, branchRef, provenance,
                $"No adapter found for type '{template.AdapterType}'.");
        }

        // Step 5: Dispatch comparison
        return adapter.Compare(snapshot, template, branch);
    }

    private IComparisonAdapter? FindAdapter(ComparisonTemplate template)
    {
        foreach (var adapter in _adapters)
        {
            if (adapter.CanHandle(template))
                return adapter;
        }
        return null;
    }
}

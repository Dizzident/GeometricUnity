using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Interface for comparison adapters that evaluate an observable against a reference.
/// Defined in Gu.ExternalComparison per architect ruling -- no dependency on Gu.Observation.
/// Only accepts ObservableSnapshot (type-level guard for observation discipline IA-6).
/// </summary>
public interface IComparisonAdapter
{
    /// <summary>Adapter type identifier (e.g., "curated_table", "simulated_benchmark").</summary>
    string AdapterType { get; }

    /// <summary>
    /// Execute a comparison of an observed quantity against a reference.
    /// The observable must carry verified ObservationProvenance.
    /// </summary>
    ComparisonRecord Compare(
        ObservableSnapshot observable,
        ComparisonTemplate template,
        BranchManifest branch);

    /// <summary>
    /// Whether this adapter can handle the given template.
    /// </summary>
    bool CanHandle(ComparisonTemplate template);
}

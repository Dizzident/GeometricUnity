namespace Gu.ExternalComparison;

/// <summary>
/// Provides reference data for comparison adapters.
/// Implementations may load from JSON files, databases, or hardcoded tables.
/// </summary>
public interface IReferenceDataSource
{
    /// <summary>
    /// Look up reference values for a specific source and observable.
    /// Returns null if not found.
    /// </summary>
    ReferenceDataEntry? GetReference(string referenceSourceId, string observableId);
}

/// <summary>
/// A single reference data entry: the expected values and their provenance.
/// </summary>
public sealed class ReferenceDataEntry
{
    /// <summary>Reference source identifier.</summary>
    public required string SourceId { get; init; }

    /// <summary>Version or revision of the reference data.</summary>
    public required string Version { get; init; }

    /// <summary>Observable identifier this reference applies to.</summary>
    public required string ObservableId { get; init; }

    /// <summary>Reference values.</summary>
    public required double[] Values { get; init; }

    /// <summary>Optional citation or formal source.</summary>
    public string? Citation { get; init; }
}

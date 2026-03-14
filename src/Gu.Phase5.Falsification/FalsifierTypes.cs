namespace Gu.Phase5.Falsification;

/// <summary>
/// String constants for falsifier type classification (M50).
/// String constants are used instead of enum to allow forward-compatible extension
/// and interop with JSON schemas.
/// </summary>
public static class FalsifierTypes
{
    /// <summary>A branch variant has fragility score above threshold.</summary>
    public const string BranchFragility = "branch-fragility";

    /// <summary>A target quantity failed to converge under refinement.</summary>
    public const string NonConvergence = "non-convergence";

    /// <summary>Observation pipeline produces unstable results across runs.</summary>
    public const string ObservationInstability = "observation-instability";

    /// <summary>Quantity varies excessively across environment tiers.</summary>
    public const string EnvironmentInstability = "environment-instability";

    /// <summary>Computed observable fails quantitative match against external target.</summary>
    public const string QuantitativeMismatch = "quantitative-mismatch";

    /// <summary>Structural representation mismatch (e.g., wrong mode count).</summary>
    public const string RepresentationContent = "representation-content";

    /// <summary>Coupling proxy inconsistency across branches.</summary>
    public const string CouplingInconsistency = "coupling-inconsistency";
}

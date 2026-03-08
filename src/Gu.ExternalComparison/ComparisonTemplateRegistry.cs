using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Provides default ComparisonTemplates for standard falsifier checks.
/// Hard structural templates are automatic on every validation-grade run.
/// Soft physics templates are opt-in per EnvironmentSpec.
/// </summary>
public static class ComparisonTemplateRegistry
{
    /// <summary>
    /// Bianchi identity: d_omega(F) = 0 to discretization accuracy.
    /// </summary>
    public static readonly ComparisonTemplate BianchiIdentity = new()
    {
        TemplateId = "structural-bianchi-identity",
        AdapterType = "structural_fact",
        ObservableId = "bianchi-residual",
        ReferenceSourceId = "structural",
        ComparisonRule = "structural_match",
        ComparisonScope = "NumericalImplementation",
        Tolerance = FalsifierRegistry.BianchiViolation.Tolerance,
        FalsifierCondition = "F-HARD-01",
        MinimumOutputType = OutputType.Quantitative,
    };

    /// <summary>
    /// Topological charge integrality: tr(F wedge F) must be integer.
    /// </summary>
    public static readonly ComparisonTemplate TopologicalCharge = new()
    {
        TemplateId = "structural-topological-charge",
        AdapterType = "structural_fact",
        ObservableId = "topological-charge",
        ReferenceSourceId = "structural",
        ComparisonRule = "integer_check",
        ComparisonScope = "RawBranchStructure",
        Tolerance = FalsifierRegistry.NonIntegerTopologicalCharge.Tolerance,
        FalsifierCondition = "F-HARD-02",
        MinimumOutputType = OutputType.Quantitative,
    };

    /// <summary>
    /// Gauge zero mode count: must equal dim(G).
    /// </summary>
    public static readonly ComparisonTemplate GaugeModeCount = new()
    {
        TemplateId = "structural-gauge-mode-count",
        AdapterType = "structural_fact",
        ObservableId = "gauge-zero-mode-count",
        ReferenceSourceId = "structural",
        ComparisonRule = "count_match",
        ComparisonScope = "RawBranchStructure",
        Tolerance = FalsifierRegistry.WrongGaugeModeCount.Tolerance,
        FalsifierCondition = "F-HARD-03",
        MinimumOutputType = OutputType.ExactStructural,
    };

    /// <summary>
    /// Carrier type compatibility: T_h and S_h must have the same tensor signature.
    /// </summary>
    public static readonly ComparisonTemplate CarrierCompatibility = new()
    {
        TemplateId = "structural-carrier-compatibility",
        AdapterType = "structural_fact",
        ObservableId = "carrier-mismatch-residual",
        ReferenceSourceId = "structural",
        ComparisonRule = "structural_match",
        ComparisonScope = "RawBranchStructure",
        Tolerance = FalsifierRegistry.CarrierMismatch.Tolerance,
        FalsifierCondition = "F-HARD-04",
        MinimumOutputType = OutputType.ExactStructural,
    };

    /// <summary>
    /// Gauge covariance: residual transforms covariantly under exact finite gauge transforms.
    /// </summary>
    public static readonly ComparisonTemplate GaugeCovariance = new()
    {
        TemplateId = "structural-gauge-covariance",
        AdapterType = "structural_fact",
        ObservableId = "gauge-covariance-deviation",
        ReferenceSourceId = "structural",
        ComparisonRule = "norm_bound",
        ComparisonScope = "NumericalImplementation",
        Tolerance = FalsifierRegistry.GaugeCovarianceFailure.Tolerance,
        FalsifierCondition = "F-HARD-05",
        MinimumOutputType = OutputType.Quantitative,
    };

    /// <summary>
    /// All hard structural templates that run automatically.
    /// </summary>
    public static IReadOnlyList<ComparisonTemplate> HardStructuralTemplates { get; } = new[]
    {
        BianchiIdentity,
        TopologicalCharge,
        GaugeModeCount,
        CarrierCompatibility,
        GaugeCovariance,
    };

    /// <summary>
    /// Get all template IDs suitable for inclusion in EnvironmentSpec.ComparisonTemplateIds.
    /// </summary>
    public static IReadOnlyList<string> HardStructuralTemplateIds { get; } =
        HardStructuralTemplates.Select(t => t.TemplateId).ToArray();

    /// <summary>
    /// Look up a template by ID. Returns null if not a registered default template.
    /// </summary>
    public static ComparisonTemplate? GetById(string templateId)
    {
        foreach (var template in HardStructuralTemplates)
        {
            if (template.TemplateId == templateId)
                return template;
        }
        return null;
    }
}

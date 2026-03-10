namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Result of a full gauge reduction at a background state.
/// Contains all constructed components for downstream use by the spectral solver.
/// </summary>
public sealed class GaugeReductionResult
{
    /// <summary>The linearized gauge action Gamma_*.</summary>
    public required GaugeActionLinearization Linearization { get; init; }

    /// <summary>Orthonormalized gauge basis.</summary>
    public required GaugeBasis Basis { get; init; }

    /// <summary>Projector onto the gauge image.</summary>
    public required GaugeProjector GaugeProjector { get; init; }

    /// <summary>Projector onto the physical complement (ILinearOperator).</summary>
    public required PhysicalProjector PhysicalProjector { get; init; }

    /// <summary>Constraint defect diagnostic report.</summary>
    public required ConstraintDefectReport DefectReport { get; init; }
}

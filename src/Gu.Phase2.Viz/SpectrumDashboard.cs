using Gu.Phase2.Stability;

namespace Gu.Phase2.Viz;

/// <summary>
/// Renders eigenvalue spectrum bar chart from <see cref="SpectrumRecord"/>
/// as a HUD overlay. Produces a <see cref="SpectrumDashboardPayload"/>.
/// </summary>
public sealed class SpectrumDashboard : IVulkanDiagnosticPanel
{
    public string PanelId => "spectrum-dashboard";
    public string Title => "Hessian Spectrum Dashboard";

    /// <summary>
    /// Prepare a spectrum dashboard payload from a SpectrumRecord.
    /// </summary>
    public SpectrumDashboardPayload Prepare(SpectrumRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var (_, coercive, soft, nearKernel, negative) =
            LinearizationWorkbench.ClassifySpectrum(record.Values, StabilityStudySpec.Default);

        return new SpectrumDashboardPayload
        {
            BackgroundStateId = record.BackgroundStateId,
            OperatorId = record.OperatorId,
            Values = (double[])record.Values.Clone(),
            StabilityInterpretation = record.StabilityInterpretation,
            GaugeHandlingMode = record.GaugeHandlingMode,
            ProbeMethod = record.ProbeMethod,
            NegativeModeCount = negative,
            SoftModeCount = soft,
            NearKernelCount = nearKernel,
            CoerciveModeCount = coercive,
        };
    }

    /// <summary>
    /// Prepare a spectrum dashboard from a HessianRecord (mode counts only, no eigenvalues).
    /// </summary>
    public SpectrumDashboardPayload PrepareFromHessian(HessianRecord record, double[]? eigenvalues = null)
    {
        ArgumentNullException.ThrowIfNull(record);

        // Infer stability interpretation from mode counts
        string? interpretation = null;
        if (record.NegativeModeCount > 0)
            interpretation = "negative-modes-saddle";
        else if (record.NearKernelCount > 0)
            interpretation = "near-zero-kernel";
        else if (record.SoftModeCount > 0)
            interpretation = "soft-modes-present";
        else if (record.CoerciveModeCount.HasValue)
            interpretation = "strictly-positive-on-slice";

        return new SpectrumDashboardPayload
        {
            BackgroundStateId = record.BackgroundStateId,
            OperatorId = "H",
            Values = eigenvalues ?? Array.Empty<double>(),
            StabilityInterpretation = interpretation,
            GaugeHandlingMode = record.GaugeHandlingMode,
            ProbeMethod = "hessian-record",
            NegativeModeCount = record.NegativeModeCount ?? 0,
            SoftModeCount = record.SoftModeCount ?? 0,
            NearKernelCount = record.NearKernelCount ?? 0,
            CoerciveModeCount = record.CoerciveModeCount ?? 0,
        };
    }
}

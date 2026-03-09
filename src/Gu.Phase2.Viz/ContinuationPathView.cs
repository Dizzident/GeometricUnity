using Gu.Phase2.Continuation;
using Gu.VulkanViewer;

namespace Gu.Phase2.Viz;

/// <summary>
/// Renders lambda vs. objective or lambda vs. smallest eigenvalue
/// as a 2D line plot on the HUD. Produces a <see cref="ContinuationPathPayload"/>.
/// </summary>
public sealed class ContinuationPathView : IVulkanDiagnosticPanel
{
    public string PanelId => "continuation-path";
    public string Title => "Continuation Path";

    /// <summary>
    /// Prepare a continuation path payload from a ContinuationResult.
    /// Extracts lambda vs. residual-norm and lambda vs. corrector-iterations series.
    /// </summary>
    public ContinuationPathPayload Prepare(ContinuationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var path = result.Path;
        int n = path.Count;

        double[] lambdas = new double[n];
        double[] residuals = new double[n];
        double[] correctorIters = new double[n];
        double[] arclengths = new double[n];

        for (int i = 0; i < n; i++)
        {
            lambdas[i] = path[i].Lambda;
            residuals[i] = path[i].ResidualNorm;
            correctorIters[i] = path[i].CorrectorIterations;
            arclengths[i] = path[i].Arclength;
        }

        var series = new List<PlotSeries>
        {
            new PlotSeries
            {
                Label = "Residual Norm",
                X = lambdas,
                Y = residuals,
                LogScaleRecommended = true,
            },
            new PlotSeries
            {
                Label = "Corrector Iterations",
                X = lambdas,
                Y = correctorIters,
                LogScaleRecommended = false,
            },
            new PlotSeries
            {
                Label = "Arclength",
                X = lambdas,
                Y = arclengths,
                LogScaleRecommended = false,
            },
        };

        return new ContinuationPathPayload
        {
            ParameterName = result.Spec.ParameterName,
            BranchManifestId = result.Spec.BranchManifestId,
            Series = series,
            TotalSteps = result.TotalSteps,
            TerminationReason = result.TerminationReason,
            LambdaMin = result.LambdaMin,
            LambdaMax = result.LambdaMax,
        };
    }
}

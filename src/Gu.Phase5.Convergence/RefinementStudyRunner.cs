using Gu.Core;

namespace Gu.Phase5.Convergence;

/// <summary>
/// Orchestrates a refinement convergence study (M47).
///
/// For each target quantity:
///   1. Collect quantity values from each refinement level run.
///   2. Classify convergence behavior.
///   3. Attempt Richardson extrapolation.
///   4. Emit ContinuumEstimateRecord (success) or ConvergenceFailureRecord (failure).
///
/// The pipelineExecutor is provided externally so this class has no direct
/// dependency on the solver stack — it only processes the results.
/// </summary>
public sealed class RefinementStudyRunner
{
    /// <summary>
    /// Run a refinement study.
    /// </summary>
    /// <param name="spec">Study specification with levels and target quantities.</param>
    /// <param name="pipelineExecutor">
    /// Called for each refinement level; returns extracted quantity values
    /// keyed by quantity ID.
    /// </param>
    /// <returns>
    /// Continuum estimate records for quantities that converged.
    /// Non-converging quantities are tracked in FailureRecords.
    /// </returns>
    public RefinementStudyResult Run(
        RefinementStudySpec spec,
        Func<RefinementLevel, IReadOnlyDictionary<string, double>> pipelineExecutor)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(pipelineExecutor);

        // Run pipeline at each level
        var runRecords = new List<RefinementRunRecord>();
        var defaultProvenance = spec.Provenance;

        foreach (var level in spec.RefinementLevels)
        {
            IReadOnlyDictionary<string, double> quantities;
            bool converged = true;
            double residual = 0.0;
            try
            {
                quantities = pipelineExecutor(level);
            }
            catch (Exception ex)
            {
                // Solver failure: record empty quantities
                quantities = new Dictionary<string, double>();
                converged = false;
                residual = double.PositiveInfinity;
                _ = ex; // logged in description below
            }

            runRecords.Add(new RefinementRunRecord
            {
                LevelId = level.LevelId,
                MeshParameter = level.EffectiveMeshParameter,
                Quantities = quantities,
                Converged = converged,
                ResidualNorm = residual,
                Provenance = defaultProvenance,
            });
        }

        // For each target quantity, collect values across levels and extrapolate
        var estimates = new List<ContinuumEstimateRecord>();
        var failures = new List<ConvergenceFailureRecord>();

        foreach (var quantityId in spec.TargetQuantities)
        {
            var levelRecords = runRecords
                .Where(r => r.Quantities.ContainsKey(quantityId) && r.Converged)
                .OrderByDescending(r => r.MeshParameter) // coarsest first
                .ToList();

            var allMeshParams = runRecords
                .OrderByDescending(r => r.MeshParameter)
                .Select(r => r.MeshParameter)
                .ToArray();

            // Check for solver failures
            if (levelRecords.Count < runRecords.Count)
            {
                var failedLevels = runRecords
                    .Where(r => !r.Converged || !r.Quantities.ContainsKey(quantityId))
                    .Select(r => r.LevelId)
                    .ToList();
                failures.Add(new ConvergenceFailureRecord
                {
                    QuantityId = quantityId,
                    FailureType = "solver-failure",
                    Description = $"Solver did not converge or quantity missing at levels: {string.Join(", ", failedLevels)}.",
                    ObservedValues = levelRecords.Select(r => r.Quantities[quantityId]).ToArray(),
                    MeshParameters = levelRecords.Select(r => r.MeshParameter).ToArray(),
                });
                continue;
            }

            if (levelRecords.Count < 3)
            {
                failures.Add(new ConvergenceFailureRecord
                {
                    QuantityId = quantityId,
                    FailureType = "insufficient-data",
                    Description = $"Only {levelRecords.Count} usable refinement level(s); need >= 3.",
                    ObservedValues = levelRecords.Select(r => r.Quantities[quantityId]).ToArray(),
                    MeshParameters = levelRecords.Select(r => r.MeshParameter).ToArray(),
                });
                continue;
            }

            double[] meshParams = levelRecords.Select(r => r.MeshParameter).ToArray();
            double[] vals = levelRecords.Select(r => r.Quantities[quantityId]).ToArray();

            var (classification, confidenceNote) = ConvergenceClassifier.Classify(meshParams, vals);

            if (classification == "non-convergent")
            {
                failures.Add(new ConvergenceFailureRecord
                {
                    QuantityId = quantityId,
                    FailureType = "non-convergent",
                    Description = confidenceNote,
                    ObservedValues = vals,
                    MeshParameters = meshParams,
                });
                continue;
            }

            RichardsonFitRecord fit = RichardsonExtrapolator.Extrapolate(quantityId, meshParams, vals);

            double finestValue = vals[^1];
            double errorBand = System.Math.Abs(finestValue - fit.EstimatedLimit);

            estimates.Add(new ContinuumEstimateRecord
            {
                QuantityId = quantityId,
                ExtrapolatedValue = fit.EstimatedLimit,
                ErrorBand = errorBand,
                ConvergenceOrder = fit.EstimatedOrder,
                ConvergenceClassification = classification,
                ConfidenceNote = confidenceNote,
                RunRecords = levelRecords,
            });
        }

        return new RefinementStudyResult
        {
            StudyId = spec.StudyId,
            RunRecords = runRecords,
            ContinuumEstimates = estimates,
            FailureRecords = failures,
        };
    }
}

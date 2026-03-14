using Gu.Artifacts;
using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Dossiers;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Orchestrates the full M53 reference study (Phase V campaign).
///
/// Steps:
///   1. Branch robustness study (M46)
///   2. Refinement/continuum study (M47)
///   3. Quantitative validation (M49)
///   4. Falsifier evaluation (M50)
///   5. Dossier assembly (M51/M52)
///   6. Report generation (M53)
///
/// Callback delegates keep this runner decoupled from specific solver implementations.
/// </summary>
public sealed class Phase5CampaignRunner
{
    /// <summary>
    /// Run the full Phase V campaign.
    /// </summary>
    /// <param name="spec">Campaign specification.</param>
    /// <param name="branchPipelineExecutor">
    /// Called for each branch variant ID; returns extracted target quantities keyed by
    /// quantity ID (each value is an array — take first element per quantity per variant).
    /// </param>
    /// <param name="refinementPipelineExecutor">
    /// Called for each refinement level; returns extracted target quantities.
    /// </param>
    /// <param name="observablesSource">
    /// Computed quantitative observables for comparison against external targets.
    /// </param>
    /// <param name="targetTable">External target table for quantitative validation.</param>
    /// <returns>Final Phase5Report.</returns>
    public Phase5Report Run(
        Phase5CampaignSpec spec,
        Func<string, IReadOnlyDictionary<string, double[]>> branchPipelineExecutor,
        Func<RefinementLevel, IReadOnlyDictionary<string, double>> refinementPipelineExecutor,
        IReadOnlyList<QuantitativeObservableRecord> observablesSource,
        ExternalTargetTable targetTable)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(branchPipelineExecutor);
        ArgumentNullException.ThrowIfNull(refinementPipelineExecutor);
        ArgumentNullException.ThrowIfNull(observablesSource);
        ArgumentNullException.ThrowIfNull(targetTable);

        var provenance = spec.Provenance;

        // Step 1: Branch-robustness study (M46)
        var quantityValues = new Dictionary<string, double[]>();
        int nVariants = spec.BranchFamilySpec.BranchVariantIds.Count;
        foreach (var qid in spec.BranchFamilySpec.TargetQuantityIds)
        {
            var values = new double[nVariants];
            for (int i = 0; i < nVariants; i++)
            {
                var variantId = spec.BranchFamilySpec.BranchVariantIds[i];
                var pipelineResult = branchPipelineExecutor(variantId);
                if (pipelineResult.TryGetValue(qid, out var arr) && arr.Length > 0)
                    values[i] = arr[0];
            }
            quantityValues[qid] = values;
        }

        var branchEngine = new BranchRobustnessEngine(spec.BranchFamilySpec);
        var branchRecord = branchEngine.Run(quantityValues, provenance);

        // Step 2: Refinement/continuum study (M47)
        var refinementRunner = new RefinementStudyRunner();
        var refinementResult = refinementRunner.Run(spec.RefinementSpec, refinementPipelineExecutor);

        // Step 3: Quantitative validation (M49)
        var qvRunner = new QuantitativeValidationRunner();
        var scoreCard = qvRunner.Run(
            spec.CampaignId,
            observablesSource,
            targetTable,
            spec.CalibrationPolicy,
            provenance);

        // Step 4: Falsifier evaluation (M50)
        var falsifierEvaluator = new FalsifierEvaluator();
        var falsifiers = falsifierEvaluator.Evaluate(
            spec.CampaignId,
            branchRecord,
            refinementResult.ContinuumEstimates,
            refinementResult.FailureRecords,
            scoreCard,
            spec.FalsificationPolicy,
            provenance);

        // Step 5: Dossier assembly (M51/M52)
        var posStudyManifest = new StudyManifest
        {
            StudyId = $"{spec.CampaignId}-positive",
            Description = $"Phase V campaign {spec.CampaignId} — positive evidence dossier",
            RunFolder = "artifacts",
            Reproducibility = ReproducibilityBundle.CreateRegeneratedCpu(
                codeRevision: provenance.CodeRevision,
                reproductionCommands: ["dotnet run --project apps/Gu.Cli -- run-phase5-campaign"]),
            Provenance = provenance,
        };

        var negStudyManifest = new StudyManifest
        {
            StudyId = $"{spec.CampaignId}-negative",
            Description = $"Phase V campaign {spec.CampaignId} — negative result dossier",
            RunFolder = "artifacts",
            Reproducibility = ReproducibilityBundle.CreateRegeneratedCpu(
                codeRevision: provenance.CodeRevision,
                reproductionCommands: ["dotnet run --project apps/Gu.Cli -- run-phase5-campaign"]),
            Provenance = provenance,
        };

        var positiveDossier = DossierAssembler.Assemble(
            $"{spec.CampaignId}-positive-dossier",
            $"Phase V Positive/Mixed Dossier: {spec.CampaignId}",
            [posStudyManifest],
            provenance);

        var negativeDossier = DossierAssembler.Assemble(
            $"{spec.CampaignId}-negative-dossier",
            $"Phase V Negative Result Dossier: {spec.CampaignId}",
            [negStudyManifest],
            provenance);

        var dossiers = new List<ValidationDossier> { positiveDossier, negativeDossier };

        // Step 6: Generate final report (M53)
        return Phase5ReportGenerator.Generate(
            studyId: spec.CampaignId,
            dossiers: dossiers,
            provenance: provenance,
            branchRecord: branchRecord,
            refinementResult: refinementResult,
            falsifiers: falsifiers);
    }
}

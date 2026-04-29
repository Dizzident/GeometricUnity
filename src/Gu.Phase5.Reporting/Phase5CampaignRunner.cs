using Gu.Artifacts;
using Gu.Core;
using Gu.Phase4.Registry;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Dossiers;
using Gu.Phase5.Environments;
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
///   5. Dossier assembly (M51/M52) — both typed and provenance dossiers (WP-5/D-006)
///   6. Report generation (M53)
///
/// Callback delegates keep this runner decoupled from specific solver implementations.
/// </summary>
public sealed class Phase5CampaignRunner
{
    /// <summary>
    /// Run the full Phase V campaign and return both dossier types plus the report (WP-5).
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
    /// <param name="registry">Optional unified particle registry for claim escalation.</param>
    /// <returns>Phase5CampaignResult containing both dossier types and the final report.</returns>
    public Phase5CampaignResult RunFull(
        Phase5CampaignSpec spec,
        Func<string, IReadOnlyDictionary<string, double[]>> branchPipelineExecutor,
        Func<RefinementLevel, IReadOnlyDictionary<string, double>> refinementPipelineExecutor,
        IReadOnlyList<QuantitativeObservableRecord> observablesSource,
        ExternalTargetTable targetTable,
        UnifiedParticleRegistry? registry = null,
        IReadOnlyList<CandidateProvenanceLinkRecord>? candidateProvenanceLinks = null,
        IReadOnlyList<ObservationChainRecord>? observationChainRecords = null,
        IReadOnlyList<EnvironmentRecord>? environmentRecords = null,
        IReadOnlyList<EnvironmentVarianceRecord>? environmentVarianceRecords = null,
        IReadOnlyList<RepresentationContentRecord>? representationContentRecords = null,
        IReadOnlyList<CouplingConsistencyRecord>? couplingConsistencyRecords = null,
        SidecarSummary? sidecarSummary = null,
        RefinementEvidenceManifest? refinementEvidenceManifest = null,
        ObservableClassificationTable? observableClassifications = null,
        IReadOnlyList<PhysicalObservableMapping>? physicalObservableMappings = null,
        IReadOnlyList<PhysicalCalibrationRecord>? physicalCalibrations = null,
        IReadOnlyList<IdentifiedPhysicalModeRecord>? physicalModeRecords = null,
        ModeIdentificationEvidenceTable? modeIdentificationEvidence = null,
        WzPhysicalClaimFalsifierRelevanceAuditResult? physicalClaimFalsifierRelevanceAudit = null)
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
            provenance,
            environmentRecords,
            failClosedTargetCoverage: true);

        // Step 4: Falsifier evaluation (M50)
        var falsifierEvaluator = new FalsifierEvaluator();
        var falsifiers = falsifierEvaluator.Evaluate(
            spec.CampaignId,
            branchRecord,
            refinementResult.ContinuumEstimates,
            refinementResult.FailureRecords,
            scoreCard,
            spec.FalsificationPolicy,
            provenance,
            observationRecords: observationChainRecords,
            environmentVarianceRecords: environmentVarianceRecords,
            representationContentRecords: representationContentRecords,
            couplingConsistencyRecords: couplingConsistencyRecords,
            sidecarSummary: sidecarSummary);

        // Step 5a: Study manifests — positive and negative (D-006)
        var posStudyManifest = new StudyManifest
        {
            StudyId = $"{spec.CampaignId}-positive",
            Description = $"Phase V campaign {spec.CampaignId} — positive evidence dossier",
            RunFolder = "artifacts",
            Reproducibility = ReproducibilityBundle.CreateRegeneratedCpu(
                codeRevision: provenance.CodeRevision,
                reproductionCommands:
                [
                    $"dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec <campaign.json> --out-dir <dir>",
                ]),
            Provenance = provenance,
        };

        var negStudyManifest = new StudyManifest
        {
            StudyId = $"{spec.CampaignId}-negative",
            Description = $"Phase V campaign {spec.CampaignId} — negative result dossier",
            RunFolder = "artifacts",
            Reproducibility = ReproducibilityBundle.CreateRegeneratedCpu(
                codeRevision: provenance.CodeRevision,
                reproductionCommands:
                [
                    $"dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec <campaign.json> --out-dir <dir>",
                ]),
            Provenance = provenance,
        };

        var studyManifests = new List<StudyManifest> { posStudyManifest, negStudyManifest };

        // Step 5b: Provenance/freshness dossier (ValidationDossier) — G-006 gate
        var provenanceDossier = DossierAssembler.Assemble(
            $"{spec.CampaignId}-provenance-dossier",
            $"Phase V Provenance Dossier: {spec.CampaignId}",
            studyManifests,
            provenance);

        // Step 5c: Typed technical dossier (Phase5ValidationDossier) — scientific content
        var effectiveRegistry = registry is null
            ? null
            : CandidateProvenanceLinker.Apply(registry, candidateProvenanceLinks);

        var typedDossierAssembler = new Phase5DossierAssembler();
        var typedDossier = typedDossierAssembler.Assemble(
            studyId: spec.CampaignId,
            branchRecord: branchRecord,
            convergenceRecords: refinementResult.ContinuumEstimates,
            convergenceFailures: refinementResult.FailureRecords,
            environments: environmentRecords,
            scoreCard: scoreCard,
            falsifiers: falsifiers,
            registry: effectiveRegistry,
            environmentTiersCovered: spec.EnvironmentCampaignSpec.EnvironmentIds,
            freshness: "regenerated-current-code",
            provenance: provenance,
            observationChainRecords: observationChainRecords,
            sidecarSummary: sidecarSummary,
            environmentVarianceRecords: environmentVarianceRecords,
            representationContentRecords: representationContentRecords);

        var physicalPredictions = PhysicalPredictionProjector.Project(
            observablesSource,
            physicalObservableMappings,
            observableClassifications,
            physicalCalibrations is null
                ? null
                : new PhysicalCalibrationTable
                {
                    TableId = "campaign-physical-calibrations",
                    Calibrations = physicalCalibrations,
                },
            physicalModeRecords,
            modeIdentificationEvidence?.Evidence);

        // Step 6: Generate final report (M53)
        var report = Phase5ReportGenerator.Generate(
            studyId: spec.CampaignId,
            dossiers: [provenanceDossier],
            provenance: provenance,
            branchRecord: branchRecord,
            refinementResult: refinementResult,
            refinementEvidenceManifest: refinementEvidenceManifest,
            falsifiers: falsifiers,
            observableClassifications: observableClassifications,
            physicalObservableMappings: physicalObservableMappings,
            physicalPredictions: physicalPredictions,
            scoreCard: scoreCard,
            physicalCalibrationAvailable: physicalCalibrations?.Any(c =>
                string.Equals(c.Status, "validated", StringComparison.OrdinalIgnoreCase)) == true,
            physicalTargetEvidenceAvailable: targetTable.Targets.Any(t =>
                !string.IsNullOrWhiteSpace(t.ParticleId) ||
                !string.IsNullOrWhiteSpace(t.PhysicalObservableType)),
            physicalClaimFalsifierRelevanceAudit: physicalClaimFalsifierRelevanceAudit);

        // Step 7: P11-M5 representation-content stabilization record.
        // Produced whenever a fatal representation-content falsifier is active.
        var repContentStabilization = BuildRepContentStabilization(
            falsifiers, representationContentRecords, provenance);

        return new Phase5CampaignResult
        {
            TypedDossier = typedDossier,
            ProvenanceDossier = provenanceDossier,
            StudyManifests = studyManifests,
            Report = report,
            RepresentationContentStabilization = repContentStabilization,
        };
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Produces a RepresentationContentStabilizationRecord when a fatal
    /// representation-content falsifier is active (P11-M5).
    /// Returns null if no such falsifier is present.
    /// </summary>
    private static RepresentationContentStabilizationRecord? BuildRepContentStabilization(
        FalsifierSummary falsifiers,
        IReadOnlyList<RepresentationContentRecord>? representationContentRecords,
        ProvenanceMeta provenance)
    {
        var fatalRepFalsifier = falsifiers.Falsifiers
            .FirstOrDefault(f =>
                f.Active &&
                f.Severity == FalsifierSeverity.Fatal &&
                f.FalsifierType == FalsifierTypes.RepresentationContent);

        if (fatalRepFalsifier is null)
            return null;

        var matchingRecord = representationContentRecords?
            .FirstOrDefault(r => string.Equals(r.CandidateId, fatalRepFalsifier.TargetId, StringComparison.Ordinal));

        return new RepresentationContentStabilizationRecord
        {
            RecordId = $"rep-content-stabilization-{fatalRepFalsifier.TargetId}",
            Status = "preserved-as-blocker",
            CandidateId = fatalRepFalsifier.TargetId,
            FalsifierId = fatalRepFalsifier.FalsifierId,
            BlockerReason =
                matchingRecord is not null
                    ? $"Candidate '{fatalRepFalsifier.TargetId}' is a singleton cluster with " +
                      $"{matchingRecord.ObservedModeCount} observed family source(s) against a required " +
                      $"minimum of {matchingRecord.ExpectedModeCount}. The Phase IV fermion-family atlas " +
                      $"was searched; no second persistent family exists for this candidate in the current " +
                      $"repository context. This is a genuine negative result confirmed by the physicist: " +
                      $"the toy geometry normal bundle cannot match the draft's predicted dimension."
                    : $"Fatal representation-content falsifier on candidate '{fatalRepFalsifier.TargetId}' " +
                      $"cannot be closed without new physics results or a revised fermion family atlas.",
            SearchedArtifactRefs = matchingRecord?.SourceArtifactRefs,
            ClosureRequirement =
                "New branch variants that produce a second persistent fermion family for this candidate, " +
                "or a revised fermion family atlas from a higher-fidelity study using draft-level geometry.",
            BindingDecision = "D-P11-004",
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Run the full Phase V campaign and return the final Phase5Report.
    /// For dual-dossier output, use <see cref="RunFull"/>.
    /// </summary>
    /// <returns>Final Phase5Report.</returns>
    public Phase5Report Run(
        Phase5CampaignSpec spec,
        Func<string, IReadOnlyDictionary<string, double[]>> branchPipelineExecutor,
        Func<RefinementLevel, IReadOnlyDictionary<string, double>> refinementPipelineExecutor,
        IReadOnlyList<QuantitativeObservableRecord> observablesSource,
        ExternalTargetTable targetTable)
    {
        return RunFull(spec, branchPipelineExecutor, refinementPipelineExecutor,
            observablesSource, targetTable, registry: null).Report;
    }
}

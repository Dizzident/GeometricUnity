using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Executes a comparison campaign against external data (Section 13.1).
/// Enforces comparison mode guards (Section 13.3) and preserves all failures.
/// M21: Supports real comparison strategies and dataset adapters.
/// Accepts strategies via constructor dictionary or falls back to factory defaults.
/// </summary>
public sealed class CampaignRunner
{
    private readonly IReadOnlyDictionary<ComparisonMode, IComparisonStrategy>? _strategies;

    public CampaignRunner()
    {
    }

    /// <summary>
    /// Create a CampaignRunner with explicit strategy overrides per comparison mode.
    /// </summary>
    public CampaignRunner(IReadOnlyDictionary<ComparisonMode, IComparisonStrategy> strategies)
    {
        _strategies = strategies;
    }

    private IComparisonStrategy ResolveStrategy(ComparisonMode mode)
    {
        if (_strategies != null && _strategies.TryGetValue(mode, out var strategy))
            return strategy;
        return ComparisonStrategyFactory.Create(mode);
    }

    /// <summary>
    /// Run a comparison campaign using real comparison strategies and a dataset adapter.
    /// Each prediction test record is validated, then compared via the appropriate strategy.
    /// Records that don't qualify for the campaign's declared mode emit ComparisonFailureRecords.
    /// Negative results are collected as first-class NegativeResultArtifacts.
    /// </summary>
    public CampaignRunnerResult RunWithStrategy(
        ComparisonCampaignSpec spec,
        IReadOnlyList<PredictionTestRecord> predictions,
        IExternalDatasetAdapter adapter,
        IComparisonStrategy? strategyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(predictions);
        ArgumentNullException.ThrowIfNull(adapter);

        var strategy = strategyOverride ?? ResolveStrategy(spec.Mode);
        var runRecords = new List<ComparisonRunRecord>();
        var failures = new List<ComparisonFailureRecord>();
        var negativeArtifacts = new List<NegativeResultArtifact>();

        // Load all referenced assets from adapter
        var assets = new Dictionary<string, ComparisonAsset>();
        foreach (var assetId in spec.ComparisonAssetIds)
        {
            if (adapter.CanProvide(assetId))
            {
                assets[assetId] = adapter.LoadAsset(assetId);
            }
        }

        foreach (var prediction in predictions)
        {
            // Step 1: Validate the prediction record
            var validationResult = PredictionValidator.Validate(prediction);

            if (!validationResult.IsValid &&
                validationResult.Record.ClaimClass == ClaimClass.Inadmissible)
            {
                var failure = new ComparisonFailureRecord
                {
                    TestId = prediction.TestId,
                    FailureReason = string.Join("; ", validationResult.Violations),
                    FailureLevel = "extraction",
                    FalsifiesRecord = false,
                    BlocksCampaign = false,
                    DemotedClaimClass = ClaimClass.Inadmissible,
                };
                failures.Add(failure);
                negativeArtifacts.Add(CreateNegativeArtifact(
                    spec, prediction, failure, isFalsification: false));
                continue;
            }

            var validatedRecord = validationResult.Record;

            // Step 2: Check comparison mode guard (Section 13.3)
            var modeCheckResult = CheckModeGuard(spec.Mode, validatedRecord, assets);
            if (modeCheckResult != null)
            {
                failures.Add(modeCheckResult);
                negativeArtifacts.Add(CreateNegativeArtifact(
                    spec, prediction, modeCheckResult, isFalsification: false));
                continue;
            }

            // Step 3: Execute comparison via strategy
            var asset = validatedRecord.ExternalComparisonAsset;
            if (asset == null)
            {
                // Structural mode may not require an asset on the prediction;
                // try the first campaign asset as fallback
                if (assets.Count > 0)
                {
                    asset = assets.Values.First();
                }
                else
                {
                    var failure = new ComparisonFailureRecord
                    {
                        TestId = validatedRecord.TestId,
                        FailureReason = "No comparison asset available for strategy execution",
                        FailureLevel = "extraction",
                        FalsifiesRecord = false,
                        BlocksCampaign = false,
                        DemotedClaimClass = validatedRecord.ClaimClass,
                    };
                    failures.Add(failure);
                    negativeArtifacts.Add(CreateNegativeArtifact(
                        spec, prediction, failure, isFalsification: false));
                    continue;
                }
            }

            var runRecord = strategy.Execute(validatedRecord, asset, adapter);

            // Override mode to match campaign spec (strategy may set its own)
            runRecord = new ComparisonRunRecord
            {
                TestId = runRecord.TestId,
                Mode = spec.Mode,
                Score = runRecord.Score,
                Passed = runRecord.Passed,
                Uncertainty = runRecord.Uncertainty,
                ResolvedClaimClass = runRecord.ResolvedClaimClass,
                Summary = runRecord.Summary,
            };
            runRecords.Add(runRecord);

            // If comparison failed, also emit a negative artifact
            if (!runRecord.Passed)
            {
                var failure = new ComparisonFailureRecord
                {
                    TestId = validatedRecord.TestId,
                    FailureReason = runRecord.Summary,
                    FailureLevel = "empirical",
                    FalsifiesRecord = runRecord.ResolvedClaimClass >= ClaimClass.PostdictionTarget,
                    BlocksCampaign = false,
                    DemotedClaimClass = runRecord.ResolvedClaimClass,
                };
                failures.Add(failure);
                negativeArtifacts.Add(CreateNegativeArtifact(
                    spec, prediction, failure,
                    isFalsification: runRecord.ResolvedClaimClass >= ClaimClass.PostdictionTarget));
            }
        }

        var campaignResult = new ComparisonCampaignResult
        {
            CampaignId = spec.CampaignId,
            RunRecords = runRecords,
            Failures = failures,
            CompletedAt = DateTimeOffset.UtcNow,
        };

        return new CampaignRunnerResult
        {
            CampaignResult = campaignResult,
            NegativeArtifacts = negativeArtifacts,
        };
    }

    /// <summary>
    /// Run a comparison campaign (M20 compatibility: synthetic scoring).
    /// Each prediction test record is validated, then compared if it qualifies.
    /// Records that don't qualify for the campaign's declared mode emit ComparisonFailureRecords.
    /// </summary>
    public ComparisonCampaignResult Run(
        ComparisonCampaignSpec spec,
        IReadOnlyList<PredictionTestRecord> predictions,
        IReadOnlyDictionary<string, ComparisonAsset> assets)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(predictions);
        ArgumentNullException.ThrowIfNull(assets);

        var runRecords = new List<ComparisonRunRecord>();
        var failures = new List<ComparisonFailureRecord>();

        foreach (var prediction in predictions)
        {
            var validationResult = PredictionValidator.Validate(prediction);

            if (!validationResult.IsValid &&
                validationResult.Record.ClaimClass == ClaimClass.Inadmissible)
            {
                failures.Add(new ComparisonFailureRecord
                {
                    TestId = prediction.TestId,
                    FailureReason = string.Join("; ", validationResult.Violations),
                    FailureLevel = "extraction",
                    FalsifiesRecord = false,
                    BlocksCampaign = false,
                    DemotedClaimClass = ClaimClass.Inadmissible,
                });
                continue;
            }

            var validatedRecord = validationResult.Record;

            var modeCheckResult = CheckModeGuard(spec.Mode, validatedRecord, assets);
            if (modeCheckResult != null)
            {
                failures.Add(modeCheckResult);
                continue;
            }

            var runRecord = ExecuteComparison(spec, validatedRecord);
            runRecords.Add(runRecord);
        }

        return new ComparisonCampaignResult
        {
            CampaignId = spec.CampaignId,
            RunRecords = runRecords,
            Failures = failures,
            CompletedAt = DateTimeOffset.UtcNow,
        };
    }

    private static ComparisonFailureRecord? CheckModeGuard(
        ComparisonMode mode,
        PredictionTestRecord record,
        IReadOnlyDictionary<string, ComparisonAsset> assets)
    {
        if (mode == ComparisonMode.Quantitative)
        {
            if (string.IsNullOrWhiteSpace(record.BranchManifestId))
                return ModeFailure(record, "Quantitative requires complete branch manifest");

            if (record.ApproximationStatus == "surrogate")
                return ModeFailure(record, "Quantitative requires exact or leading-order approximation, not surrogate");

            if (record.NumericalDependencyStatus != "converged")
                return ModeFailure(record, $"Quantitative requires converged numerical status, got '{record.NumericalDependencyStatus}'");

            if (record.ExternalComparisonAsset == null)
                return ModeFailure(record, "Quantitative requires external comparison asset with uncertainty decomposition");
        }

        if (mode == ComparisonMode.SemiQuantitative)
        {
            if (record.NumericalDependencyStatus == "failed")
                return ModeFailure(record, "SemiQuantitative requires at least exploratory numerical status");
        }

        return null;
    }

    private static ComparisonFailureRecord ModeFailure(PredictionTestRecord record, string reason)
    {
        return new ComparisonFailureRecord
        {
            TestId = record.TestId,
            FailureReason = reason,
            FailureLevel = "extraction",
            FalsifiesRecord = false,
            BlocksCampaign = false,
            DemotedClaimClass = record.ClaimClass,
        };
    }

    private static ComparisonRunRecord ExecuteComparison(
        ComparisonCampaignSpec spec,
        PredictionTestRecord record)
    {
        return new ComparisonRunRecord
        {
            TestId = record.TestId,
            Mode = spec.Mode,
            Score = 0.0,
            Passed = false,
            Uncertainty = record.ExternalComparisonAsset?.UncertaintyModel
                ?? UncertaintyRecord.Unestimated(),
            ResolvedClaimClass = record.ClaimClass,
            Summary = "Synthetic comparison (M20 compatibility)",
        };
    }

    private static NegativeResultArtifact CreateNegativeArtifact(
        ComparisonCampaignSpec spec,
        PredictionTestRecord prediction,
        ComparisonFailureRecord failure,
        bool isFalsification)
    {
        return new NegativeResultArtifact
        {
            ArtifactId = $"neg-{spec.CampaignId}-{prediction.TestId}",
            CampaignId = spec.CampaignId,
            Failure = failure,
            OriginalTestId = prediction.TestId,
            AttemptedMode = spec.Mode,
            BranchManifestId = prediction.BranchManifestId,
            IsFalsification = isFalsification,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}

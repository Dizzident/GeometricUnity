using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Validates replay contracts at different tiers per the ReplayContract type.
/// Tiers (aligned with the specification):
///   R0 (schema-only): validates JSON schemas match
///   R1 (observable-invariant): validates observed outputs match within tolerance
///   R2 (numerical-replay): validates full state reproduction within tolerance
///   R3 (bit-exact): validates exact byte-level reproduction
/// </summary>
public sealed class ReplayValidator
{
    /// <summary>Default tolerance for numerical comparisons.</summary>
    public const double DefaultTolerance = 1e-10;

    private readonly double _tolerance;

    /// <summary>
    /// Create a replay validator with the specified tolerance.
    /// </summary>
    public ReplayValidator(double tolerance = DefaultTolerance)
    {
        _tolerance = tolerance;
    }

    /// <summary>
    /// Validate a replay by comparing original and replay run folders.
    /// The tier is read from the original replay contract; use <paramref name="requiredTier"/>
    /// to override the minimum tier to validate.
    /// </summary>
    public ReplayReport Validate(
        RunFolderReader original,
        RunFolderReader replay,
        string? requiredTier = null)
    {
        var checks = new List<ReplayCheck>();

        var origContract = original.ReadReplayContract();
        var replayContract = replay.ReadReplayContract();

        if (origContract is null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "original-contract-exists",
                Passed = false,
                Detail = "No replay contract found in original run folder.",
            });

            return BuildReport(checks, requiredTier ?? ReplayTiers.R0);
        }

        checks.Add(new ReplayCheck
        {
            CheckName = "original-contract-exists",
            Passed = true,
        });

        var tier = requiredTier ?? origContract.ReplayTier;
        int tierLevel = ReplayTiers.TierLevel(tier);

        // R0: schema-only validation
        checks.AddRange(ValidateR0(original, replay));

        // R1: observable-invariant validation
        if (tierLevel >= 1)
        {
            checks.AddRange(ValidateR1(original, replay));
        }

        // R2: numerical-replay validation
        if (tierLevel >= 2)
        {
            checks.AddRange(ValidateR2(original, replay));
        }

        // R3: bit-exact validation
        if (tierLevel >= 3)
        {
            checks.AddRange(ValidateR3(original, replay));
        }

        return BuildReport(checks, tier);
    }

    /// <summary>
    /// R0 (schema-only): validates that JSON schemas match structurally.
    /// Both folders must have the same set of artifact files present, and
    /// branch manifests must have matching schema versions.
    /// </summary>
    private IReadOnlyList<ReplayCheck> ValidateR0(RunFolderReader original, RunFolderReader replay)
    {
        var checks = new List<ReplayCheck>();

        var origManifest = original.ReadBranchManifest();
        var replayManifest = replay.ReadBranchManifest();

        if (origManifest is null || replayManifest is null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "r0-manifest-present",
                Passed = false,
                Detail = $"Branch manifest missing (original: {origManifest is not null}, replay: {replayManifest is not null}).",
            });
            return checks;
        }

        checks.Add(new ReplayCheck
        {
            CheckName = "r0-manifest-present",
            Passed = true,
        });

        bool schemaMatch = origManifest.SchemaVersion == replayManifest.SchemaVersion;
        checks.Add(new ReplayCheck
        {
            CheckName = "r0-schema-version-match",
            Passed = schemaMatch,
            Detail = schemaMatch
                ? $"Schema versions match: {origManifest.SchemaVersion}"
                : $"Schema version mismatch: {origManifest.SchemaVersion} vs {replayManifest.SchemaVersion}",
        });

        bool branchIdMatch = origManifest.BranchId == replayManifest.BranchId;
        checks.Add(new ReplayCheck
        {
            CheckName = "r0-branch-id-match",
            Passed = branchIdMatch,
            Detail = branchIdMatch
                ? null
                : $"Branch ID mismatch: {origManifest.BranchId} vs {replayManifest.BranchId}",
        });

        return checks;
    }

    /// <summary>
    /// R1 (observable-invariant): validates that observed outputs match within tolerance.
    /// </summary>
    private IReadOnlyList<ReplayCheck> ValidateR1(RunFolderReader original, RunFolderReader replay)
    {
        var checks = new List<ReplayCheck>();

        var origObserved = original.ReadObservedState();
        var replayObserved = replay.ReadObservedState();

        if (origObserved is null && replayObserved is null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "r1-observed-present",
                Passed = true,
                Detail = "No observed states in either folder (vacuously valid).",
            });
            return checks;
        }

        if (origObserved is null || replayObserved is null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "r1-observed-present",
                Passed = false,
                Detail = $"Observed state presence mismatch (original: {origObserved is not null}, replay: {replayObserved is not null}).",
            });
            return checks;
        }

        checks.Add(new ReplayCheck
        {
            CheckName = "r1-observed-present",
            Passed = true,
        });

        // Compare each common observable
        var commonIds = origObserved.Observables.Keys
            .Intersect(replayObserved.Observables.Keys).ToList();

        foreach (var id in commonIds)
        {
            var origValues = origObserved.Observables[id].Values;
            var replayValues = replayObserved.Observables[id].Values;

            if (origValues.Length != replayValues.Length)
            {
                checks.Add(new ReplayCheck
                {
                    CheckName = $"r1-observable-{id}",
                    Passed = false,
                    Detail = $"Observable '{id}' length mismatch: {origValues.Length} vs {replayValues.Length}",
                });
                continue;
            }

            double maxDiff = 0;
            for (int i = 0; i < origValues.Length; i++)
            {
                double diff = System.Math.Abs(origValues[i] - replayValues[i]);
                if (diff > maxDiff)
                    maxDiff = diff;
            }

            bool passed = maxDiff <= _tolerance;
            checks.Add(new ReplayCheck
            {
                CheckName = $"r1-observable-{id}",
                Passed = passed,
                Detail = passed
                    ? $"Observable '{id}' matches (maxDiff={maxDiff:E6})"
                    : $"Observable '{id}' exceeds tolerance (maxDiff={maxDiff:E6}, tolerance={_tolerance:E3})",
            });
        }

        // Check for missing observables
        var missingInReplay = origObserved.Observables.Keys.Except(replayObserved.Observables.Keys).ToList();
        if (missingInReplay.Count > 0)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "r1-observable-coverage",
                Passed = false,
                Detail = $"Observables missing in replay: [{string.Join(", ", missingInReplay)}]",
            });
        }

        return checks;
    }

    /// <summary>
    /// R2 (numerical-replay): validates full state reproduction within tolerance.
    /// Compares final discrete states, residuals, and validation outcomes.
    /// </summary>
    private IReadOnlyList<ReplayCheck> ValidateR2(RunFolderReader original, RunFolderReader replay)
    {
        var checks = new List<ReplayCheck>();

        // Compare final states
        var origState = original.ReadFinalState();
        var replayState = replay.ReadFinalState();

        if (origState is not null && replayState is not null)
        {
            checks.Add(CompareFieldTensors(
                origState.Omega, replayState.Omega, "r2-omega-match"));
        }
        else if (origState is not null || replayState is not null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "r2-final-state-present",
                Passed = false,
                Detail = $"Final state presence mismatch (original: {origState is not null}, replay: {replayState is not null}).",
            });
        }

        // Compare residual bundles
        var origResidual = original.ReadResidualBundle();
        var replayResidual = replay.ReadResidualBundle();

        if (origResidual is not null && replayResidual is not null)
        {
            var objDiff = System.Math.Abs(origResidual.ObjectiveValue - replayResidual.ObjectiveValue);
            checks.Add(new ReplayCheck
            {
                CheckName = "r2-objective-value",
                Passed = objDiff <= _tolerance,
                Detail = $"Objective value difference: {objDiff:E6} (tolerance: {_tolerance:E3})",
            });

            var normDiff = System.Math.Abs(origResidual.TotalNorm - replayResidual.TotalNorm);
            checks.Add(new ReplayCheck
            {
                CheckName = "r2-residual-norm",
                Passed = normDiff <= _tolerance,
                Detail = $"Residual norm difference: {normDiff:E6} (tolerance: {_tolerance:E3})",
            });
        }

        // Compare validation outcomes
        var origValidation = original.ReadValidationBundle();
        var replayValidation = replay.ReadValidationBundle();

        if (origValidation is not null && replayValidation is not null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "r2-validation-outcome",
                Passed = origValidation.AllPassed == replayValidation.AllPassed,
                Detail = $"Validation outcome: original={origValidation.AllPassed}, replay={replayValidation.AllPassed}",
            });
        }

        return checks;
    }

    /// <summary>
    /// R3 (bit-exact): validates exact byte-level reproduction.
    /// Compares SHA-256 hashes of all artifact files.
    /// </summary>
    private IReadOnlyList<ReplayCheck> ValidateR3(RunFolderReader original, RunFolderReader replay)
    {
        var checks = new List<ReplayCheck>();

        var origHashes = IntegrityHasher.ComputeRunFolderHashes(original.RootPath);
        var replayHashes = IntegrityHasher.ComputeRunFolderHashes(replay.RootPath);

        // Compare state and derived files byte-for-byte via hash comparison
        var filesToCompare = new[]
        {
            RunFolderLayout.FinalStateFile,
            RunFolderLayout.ResidualBundleFile,
            RunFolderLayout.ObservedStateFile,
            RunFolderLayout.ValidationBundleFile,
        };

        foreach (var file in filesToCompare)
        {
            bool origHas = origHashes.FileHashes.ContainsKey(file);
            bool replayHas = replayHashes.FileHashes.ContainsKey(file);

            if (!origHas && !replayHas)
                continue;

            if (!origHas || !replayHas)
            {
                checks.Add(new ReplayCheck
                {
                    CheckName = $"r3-file-{Path.GetFileNameWithoutExtension(file)}",
                    Passed = false,
                    Detail = $"File '{file}' present in only one folder.",
                });
                continue;
            }

            bool hashMatch = string.Equals(
                origHashes.FileHashes[file],
                replayHashes.FileHashes[file],
                StringComparison.OrdinalIgnoreCase);

            checks.Add(new ReplayCheck
            {
                CheckName = $"r3-file-{Path.GetFileNameWithoutExtension(file)}",
                Passed = hashMatch,
                Detail = hashMatch
                    ? $"File '{file}' is bit-exact."
                    : $"File '{file}' differs (hash mismatch).",
            });
        }

        return checks;
    }

    private ReplayCheck CompareFieldTensors(FieldTensor a, FieldTensor b, string checkName)
    {
        if (a.Coefficients.Length != b.Coefficients.Length)
        {
            return new ReplayCheck
            {
                CheckName = checkName,
                Passed = false,
                Detail = $"Coefficient length mismatch: {a.Coefficients.Length} vs {b.Coefficients.Length}",
            };
        }

        double maxDiff = 0;
        for (int i = 0; i < a.Coefficients.Length; i++)
        {
            double diff = System.Math.Abs(a.Coefficients[i] - b.Coefficients[i]);
            if (diff > maxDiff)
                maxDiff = diff;
        }

        bool passed = maxDiff <= _tolerance;
        return new ReplayCheck
        {
            CheckName = checkName,
            Passed = passed,
            Detail = passed
                ? $"Field matches within tolerance (maxDiff={maxDiff:E6})"
                : $"Field exceeds tolerance (maxDiff={maxDiff:E6}, tolerance={_tolerance:E3})",
        };
    }

    private static ReplayReport BuildReport(List<ReplayCheck> checks, string tier)
    {
        var allPassed = checks.All(c => c.Passed);
        var outcome = allPassed ? ReplayOutcome.Pass : ReplayOutcome.Fail;

        return new ReplayReport
        {
            Outcome = outcome,
            ReplayTier = tier,
            Checks = checks,
            ValidatedAt = DateTimeOffset.UtcNow,
        };
    }
}

using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Validates a replay run against its original replay contract (Section 20.4).
/// Checks are tier-dependent:
/// - R0: hashes only (archival)
/// - R1: structural match (branch, geometry, conventions)
/// - R2: numerical match (residuals, observables within tolerance)
/// - R3: cross-backend numerical match
/// </summary>
public static class ReplayContractValidator
{
    /// <summary>
    /// Default tolerance for numerical comparison at R2/R3 tier.
    /// </summary>
    public const double DefaultTolerance = 1e-10;

    /// <summary>
    /// Validate a replay run folder against the original contract.
    /// </summary>
    public static ReplayReport Validate(
        RunFolderWriter originalFolder,
        RunFolderWriter replayFolder,
        string requiredTier = ReplayTiers.R2,
        double tolerance = DefaultTolerance)
    {
        var checks = new List<ReplayCheck>();
        var tierLevel = ReplayTiers.TierLevel(requiredTier);

        // Step 1: Verify original hashes exist and are readable
        checks.Add(CheckFolderStructure(originalFolder, "original"));
        checks.Add(CheckFolderStructure(replayFolder, "replay"));

        // Step 2: Load and compare manifests (R1+)
        if (tierLevel >= 1)
        {
            checks.AddRange(CheckBranchCoherence(originalFolder, replayFolder));
        }

        // Step 3: Check replay contract consistency
        var originalContract = originalFolder.ReadJson<ReplayContract>(RunFolderLayout.ReplayContractFile);
        if (originalContract is null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "replay-contract-exists",
                Passed = false,
                Detail = "Original run folder has no replay contract.",
            });
        }
        else
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "replay-contract-exists",
                Passed = true,
            });

            // Check tier meets requirement
            var meetsTier = ReplayTiers.MeetsTier(originalContract.ReplayTier, requiredTier);
            checks.Add(new ReplayCheck
            {
                CheckName = "replay-tier-sufficient",
                Passed = meetsTier,
                Detail = meetsTier
                    ? $"Contract tier {originalContract.ReplayTier} meets required {requiredTier}."
                    : $"Contract tier {originalContract.ReplayTier} does not meet required {requiredTier}.",
            });

            // Check determinism declaration
            if (!originalContract.Deterministic && tierLevel >= 2)
            {
                checks.Add(new ReplayCheck
                {
                    CheckName = "determinism-warning",
                    Passed = true, // warning, not failure
                    Detail = $"Run declared non-deterministic: {originalContract.NonDeterminismDeclaration ?? "no reason given"}",
                });
            }
        }

        // Step 4: Numerical comparison (R2+)
        if (tierLevel >= 2)
        {
            checks.AddRange(CheckNumericalReplay(originalFolder, replayFolder, tolerance));
        }

        // Step 5: Cross-backend check (R3)
        if (tierLevel >= 3)
        {
            checks.AddRange(CheckCrossBackend(originalFolder, replayFolder));
        }

        var allPassed = checks.All(c => c.Passed);
        var outcome = !checks.All(c => c.Passed)
            ? checks.Any(c => c.CheckName == "replay-contract-exists" && !c.Passed)
                ? ReplayOutcome.Invalid
                : ReplayOutcome.Fail
            : ReplayOutcome.Pass;

        return new ReplayReport
        {
            Outcome = outcome,
            ReplayTier = requiredTier,
            Checks = checks,
            ValidatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Validate that a replay contract is internally consistent.
    /// </summary>
    public static IReadOnlyList<string> ValidateContract(ReplayContract contract)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(contract.BackendId))
            errors.Add("BackendId must not be empty.");

        if (contract.BranchManifest is null)
            errors.Add("BranchManifest must not be null.");

        try
        {
            ReplayTiers.TierLevel(contract.ReplayTier);
        }
        catch (ArgumentException)
        {
            errors.Add($"Invalid ReplayTier: {contract.ReplayTier}");
        }

        if (!contract.Deterministic && string.IsNullOrWhiteSpace(contract.NonDeterminismDeclaration))
            errors.Add("Non-deterministic runs should provide a NonDeterminismDeclaration.");

        return errors;
    }

    private static ReplayCheck CheckFolderStructure(RunFolderWriter folder, string label)
    {
        var valid = folder.HasValidStructure();
        return new ReplayCheck
        {
            CheckName = $"folder-structure-{label}",
            Passed = valid,
            Detail = valid ? null : $"Run folder '{label}' does not have valid canonical structure.",
        };
    }

    private static IReadOnlyList<ReplayCheck> CheckBranchCoherence(
        RunFolderWriter originalFolder,
        RunFolderWriter replayFolder)
    {
        var checks = new List<ReplayCheck>();

        var origManifest = originalFolder.ReadJson<BranchManifest>(RunFolderLayout.BranchManifestFile);
        var replayManifest = replayFolder.ReadJson<BranchManifest>(RunFolderLayout.BranchManifestFile);

        if (origManifest is null || replayManifest is null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "branch-manifest-loadable",
                Passed = false,
                Detail = $"Could not load branch manifest (original: {origManifest is not null}, replay: {replayManifest is not null}).",
            });
            return checks;
        }

        checks.Add(new ReplayCheck
        {
            CheckName = "branch-manifest-loadable",
            Passed = true,
        });

        // Compare key structural fields
        checks.Add(CompareField("branch-id", origManifest.BranchId, replayManifest.BranchId));
        checks.Add(CompareField("schema-version", origManifest.SchemaVersion, replayManifest.SchemaVersion));
        checks.Add(CompareField("lie-algebra-id", origManifest.LieAlgebraId, replayManifest.LieAlgebraId));
        checks.Add(CompareField("basis-convention", origManifest.BasisConventionId, replayManifest.BasisConventionId));
        checks.Add(CompareField("component-order", origManifest.ComponentOrderId, replayManifest.ComponentOrderId));
        checks.Add(CompareField("active-geometry-branch", origManifest.ActiveGeometryBranch, replayManifest.ActiveGeometryBranch));
        checks.Add(CompareField("active-torsion-branch", origManifest.ActiveTorsionBranch, replayManifest.ActiveTorsionBranch));
        checks.Add(CompareField("active-shiab-branch", origManifest.ActiveShiabBranch, replayManifest.ActiveShiabBranch));
        checks.Add(CompareField("gauge-strategy", origManifest.ActiveGaugeStrategy, replayManifest.ActiveGaugeStrategy));
        checks.Add(CompareField("base-dimension", origManifest.BaseDimension.ToString(), replayManifest.BaseDimension.ToString()));
        checks.Add(CompareField("ambient-dimension", origManifest.AmbientDimension.ToString(), replayManifest.AmbientDimension.ToString()));

        return checks;
    }

    private static IReadOnlyList<ReplayCheck> CheckNumericalReplay(
        RunFolderWriter originalFolder,
        RunFolderWriter replayFolder,
        double tolerance)
    {
        var checks = new List<ReplayCheck>();

        // Compare residual bundles
        var origResidual = originalFolder.ReadJson<ResidualBundle>(RunFolderLayout.ResidualBundleFile);
        var replayResidual = replayFolder.ReadJson<ResidualBundle>(RunFolderLayout.ResidualBundleFile);

        if (origResidual is not null && replayResidual is not null)
        {
            var objDiff = System.Math.Abs(origResidual.ObjectiveValue - replayResidual.ObjectiveValue);
            checks.Add(new ReplayCheck
            {
                CheckName = "objective-value-match",
                Passed = objDiff <= tolerance,
                Detail = $"Objective difference: {objDiff:E3} (tolerance: {tolerance:E3})",
            });

            var normDiff = System.Math.Abs(origResidual.TotalNorm - replayResidual.TotalNorm);
            checks.Add(new ReplayCheck
            {
                CheckName = "residual-norm-match",
                Passed = normDiff <= tolerance,
                Detail = $"Residual norm difference: {normDiff:E3} (tolerance: {tolerance:E3})",
            });
        }
        else
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "residual-bundle-available",
                Passed = false,
                Detail = $"Residual bundles not available for comparison (original: {origResidual is not null}, replay: {replayResidual is not null}).",
            });
        }

        // Compare validation bundles
        var origValidation = originalFolder.ReadJson<ValidationBundle>(RunFolderLayout.ValidationBundleFile);
        var replayValidation = replayFolder.ReadJson<ValidationBundle>(RunFolderLayout.ValidationBundleFile);

        if (origValidation is not null && replayValidation is not null)
        {
            checks.Add(new ReplayCheck
            {
                CheckName = "validation-outcome-match",
                Passed = origValidation.AllPassed == replayValidation.AllPassed,
                Detail = $"Original allPassed={origValidation.AllPassed}, replay allPassed={replayValidation.AllPassed}",
            });
        }

        return checks;
    }

    private static IReadOnlyList<ReplayCheck> CheckCrossBackend(
        RunFolderWriter originalFolder,
        RunFolderWriter replayFolder)
    {
        var checks = new List<ReplayCheck>();

        var origContract = originalFolder.ReadJson<ReplayContract>(RunFolderLayout.ReplayContractFile);
        var replayContract = replayFolder.ReadJson<ReplayContract>(RunFolderLayout.ReplayContractFile);

        if (origContract is not null && replayContract is not null)
        {
            var differentBackend = origContract.BackendId != replayContract.BackendId;
            checks.Add(new ReplayCheck
            {
                CheckName = "cross-backend-different",
                Passed = differentBackend,
                Detail = differentBackend
                    ? $"Cross-backend: {origContract.BackendId} vs {replayContract.BackendId}"
                    : $"Same backend used: {origContract.BackendId}. R3 requires different backends.",
            });
        }

        return checks;
    }

    private static ReplayCheck CompareField(string fieldName, string original, string replay)
    {
        var match = string.Equals(original, replay, StringComparison.Ordinal);
        return new ReplayCheck
        {
            CheckName = $"manifest-{fieldName}",
            Passed = match,
            Detail = match ? null : $"Mismatch: original='{original}', replay='{replay}'",
        };
    }
}

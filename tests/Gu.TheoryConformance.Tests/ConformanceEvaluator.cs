using Gu.Branching;
using Gu.Core;

namespace Gu.TheoryConformance.Tests;

/// <summary>
/// Evaluates branch-local theory conformance for a runtime execution.
///
/// This evaluator checks that:
/// 1. Runtime torsion/Shiab/pairing/observation/geometry branch IDs match
///    the declared artifact provenance (branch-identity checks).
/// 2. Execution did not silently fall back to trivial or zero-state paths
///    (trivial-state checks).
/// 3. All results are explicitly scoped as branch-local validation, never
///    theory-level validation (scope-boundary checks).
///
/// A passing conformance artifact means the run was internally consistent
/// with its declared branch. It does NOT mean the branch is canonical or
/// that the output matches physical reality.
/// </summary>
public sealed class ConformanceEvaluator
{
    private const string BranchLocalValidationType = "branch-local";
    private const string BranchIdentityCategory = "branch-identity";
    private const string TrivialStateCategory = "trivial-state";
    private const string ScopeBoundaryCategory = "scope-boundary";

    private const string ScopeDisclaimer =
        "All checks in this conformance artifact are branch-local. " +
        "A passing result means the runtime execution was internally consistent with its declared branch. " +
        "It does NOT mean the branch is the canonical realization of Geometric Unity, " +
        "that the lowering choices are unique, or that the outputs match physical reality. " +
        "See ASSUMPTIONS.md for the full list of branch-local assumptions in effect.";

    /// <summary>
    /// Evaluate conformance of a runtime execution against declared branch assumptions.
    /// </summary>
    /// <param name="manifest">The declared branch manifest.</param>
    /// <param name="runtimeTorsion">The torsion operator that was actually used.</param>
    /// <param name="runtimeShiab">The Shiab operator that was actually used.</param>
    /// <param name="artifactProvenance">Provenance from the emitted artifact.</param>
    /// <param name="observedBranchId">The observation branch ID recorded in the artifact.</param>
    /// <param name="geometryBranchId">The geometry branch ID used at runtime.</param>
    /// <param name="omegaCoefficients">The final omega coefficients (for trivial-state detection).</param>
    /// <returns>A machine-readable conformance artifact.</returns>
    public ConformanceArtifact Evaluate(
        BranchManifest manifest,
        ITorsionBranchOperator runtimeTorsion,
        IShiabBranchOperator runtimeShiab,
        ProvenanceMeta artifactProvenance,
        string observedBranchId,
        string geometryBranchId,
        double[]? omegaCoefficients = null)
    {
        var checks = new List<ConformanceCheck>();

        // --- Branch identity checks ---
        checks.Add(CheckTorsionBranchId(manifest, runtimeTorsion));
        checks.Add(CheckShiabBranchId(manifest, runtimeShiab));
        checks.Add(CheckProvenanceBranchId(manifest, artifactProvenance));
        checks.Add(CheckObservationBranchId(manifest, observedBranchId));
        checks.Add(CheckGeometryBranchId(manifest, geometryBranchId));
        checks.Add(CheckPairingBranchId(manifest, runtimeShiab));

        // --- Trivial-state checks ---
        checks.Add(CheckTrivialTorsion(runtimeTorsion));
        checks.Add(CheckTrivialShiab(runtimeShiab));
        if (omegaCoefficients != null)
            checks.Add(CheckNonZeroOmega(omegaCoefficients));

        // --- Scope boundary checks ---
        checks.Add(CheckBranchScopeDeclaration(manifest));

        bool overallPass = checks.All(c => c.Passed);

        return new ConformanceArtifact
        {
            ConformanceId = $"conformance-{manifest.BranchId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}",
            BranchId = manifest.BranchId,
            EvaluatedAt = DateTimeOffset.UtcNow,
            OverallPass = overallPass,
            Checks = checks,
            ValidationScope = BranchLocalValidationType,
            ScopeDisclaimer = ScopeDisclaimer,
        };
    }

    // --- Branch identity check implementations ---

    private static ConformanceCheck CheckTorsionBranchId(
        BranchManifest manifest, ITorsionBranchOperator torsion)
    {
        bool passed = torsion.BranchId == manifest.ActiveTorsionBranch;
        return new ConformanceCheck
        {
            CheckId = "torsion-branch-id-match",
            Category = BranchIdentityCategory,
            Passed = passed,
            Detail = passed
                ? $"Runtime torsion branch '{torsion.BranchId}' matches manifest '{manifest.ActiveTorsionBranch}'."
                : $"Runtime torsion branch '{torsion.BranchId}' does NOT match manifest '{manifest.ActiveTorsionBranch}'. " +
                  "The artifact provenance claims a different torsion operator was used.",
            ValidationType = BranchLocalValidationType,
        };
    }

    private static ConformanceCheck CheckShiabBranchId(
        BranchManifest manifest, IShiabBranchOperator shiab)
    {
        bool passed = shiab.BranchId == manifest.ActiveShiabBranch;
        return new ConformanceCheck
        {
            CheckId = "shiab-branch-id-match",
            Category = BranchIdentityCategory,
            Passed = passed,
            Detail = passed
                ? $"Runtime Shiab branch '{shiab.BranchId}' matches manifest '{manifest.ActiveShiabBranch}'."
                : $"Runtime Shiab branch '{shiab.BranchId}' does NOT match manifest '{manifest.ActiveShiabBranch}'. " +
                  "The artifact may be mislabeled.",
            ValidationType = BranchLocalValidationType,
        };
    }

    private static ConformanceCheck CheckProvenanceBranchId(
        BranchManifest manifest, ProvenanceMeta provenance)
    {
        bool passed = provenance.Branch.BranchId == manifest.BranchId;
        return new ConformanceCheck
        {
            CheckId = "provenance-branch-id-match",
            Category = BranchIdentityCategory,
            Passed = passed,
            Detail = passed
                ? $"Artifact provenance branch ID '{provenance.Branch.BranchId}' matches manifest '{manifest.BranchId}'."
                : $"Artifact provenance branch ID '{provenance.Branch.BranchId}' does NOT match manifest '{manifest.BranchId}'. " +
                  "The artifact was emitted under a different branch than the declared manifest.",
            ValidationType = BranchLocalValidationType,
        };
    }

    private static ConformanceCheck CheckObservationBranchId(
        BranchManifest manifest, string observedBranchId)
    {
        bool passed = observedBranchId == manifest.ActiveObservationBranch;
        return new ConformanceCheck
        {
            CheckId = "observation-branch-id-match",
            Category = BranchIdentityCategory,
            Passed = passed,
            Detail = passed
                ? $"Observed state branch '{observedBranchId}' matches manifest '{manifest.ActiveObservationBranch}'."
                : $"Observed state branch '{observedBranchId}' does NOT match manifest '{manifest.ActiveObservationBranch}'. " +
                  "The observation pipeline used a different branch than declared.",
            ValidationType = BranchLocalValidationType,
        };
    }

    private static ConformanceCheck CheckGeometryBranchId(
        BranchManifest manifest, string geometryBranchId)
    {
        bool passed = geometryBranchId == manifest.ActiveGeometryBranch;
        return new ConformanceCheck
        {
            CheckId = "geometry-branch-id-match",
            Category = BranchIdentityCategory,
            Passed = passed,
            Detail = passed
                ? $"Runtime geometry branch '{geometryBranchId}' matches manifest '{manifest.ActiveGeometryBranch}'."
                : $"Runtime geometry branch '{geometryBranchId}' does NOT match manifest '{manifest.ActiveGeometryBranch}'. " +
                  "The geometry used does not correspond to the declared branch.",
            ValidationType = BranchLocalValidationType,
        };
    }

    private static ConformanceCheck CheckPairingBranchId(
        BranchManifest manifest, IShiabBranchOperator shiab)
    {
        // The pairing convention is declared in the manifest. We check that
        // the manifest explicitly declares a non-empty pairing convention ID,
        // which is a prerequisite for branch-consistent pairing.
        bool passed = !string.IsNullOrWhiteSpace(manifest.PairingConventionId)
                      && manifest.PairingConventionId != "unset";
        return new ConformanceCheck
        {
            CheckId = "pairing-convention-declared",
            Category = BranchIdentityCategory,
            Passed = passed,
            Detail = passed
                ? $"Pairing convention '{manifest.PairingConventionId}' is explicitly declared in manifest."
                : $"Pairing convention is not declared (value: '{manifest.PairingConventionId}'). " +
                  "The manifest must declare a pairing convention for results to be branch-local.",
            ValidationType = BranchLocalValidationType,
        };
    }

    // --- Trivial-state check implementations ---

    private static ConformanceCheck CheckTrivialTorsion(ITorsionBranchOperator torsion)
    {
        // "trivial-torsion" means T_h = 0 for all omega. This is a valid branch (A-009),
        // but we explicitly flag it so consumers know the execution was on the simplest branch.
        bool isTrivial = torsion.BranchId == "trivial";
        return new ConformanceCheck
        {
            CheckId = "torsion-trivial-branch-flag",
            Category = TrivialStateCategory,
            Passed = true, // trivial is allowed — we just flag it
            Detail = isTrivial
                ? $"Torsion operator is the trivial branch (T=0). " +
                  "This is the simplest executable branch (A-009). " +
                  "Results are branch-local to the zero-torsion assumption."
                : $"Torsion operator '{torsion.BranchId}' is non-trivial.",
            ValidationType = BranchLocalValidationType,
        };
    }

    private static ConformanceCheck CheckTrivialShiab(IShiabBranchOperator shiab)
    {
        // "identity-shiab" (S=F) is the simplest branch. Flag it explicitly.
        bool isIdentity = shiab.BranchId == "identity-shiab" || shiab.BranchId == "identity";
        return new ConformanceCheck
        {
            CheckId = "shiab-identity-branch-flag",
            Category = TrivialStateCategory,
            Passed = true, // identity is allowed — we just flag it
            Detail = isIdentity
                ? $"Shiab operator is the identity branch (S=F). " +
                  "This is the simplest Shiab branch (A-009). " +
                  "Upsilon = F - T in this branch."
                : $"Shiab operator '{shiab.BranchId}' is non-identity.",
            ValidationType = BranchLocalValidationType,
        };
    }

    private static ConformanceCheck CheckNonZeroOmega(double[] omegaCoefficients)
    {
        double normSq = omegaCoefficients.Sum(x => x * x);
        bool isZero = normSq < 1e-24;
        return new ConformanceCheck
        {
            CheckId = "omega-zero-state-flag",
            Category = TrivialStateCategory,
            Passed = true, // zero is allowed — we flag it for awareness
            Detail = isZero
                ? "Final omega is zero (or near-zero). " +
                  "This is the trivial connection branch (A-011). " +
                  "Zero-omega solves land on the trivial zero-residual branch and are weak validation tests."
                : $"Final omega is non-zero (||omega||^2 = {normSq:E4}). Non-trivial connection.",
            ValidationType = BranchLocalValidationType,
        };
    }

    // --- Scope boundary check implementations ---

    private static ConformanceCheck CheckBranchScopeDeclaration(BranchManifest manifest)
    {
        // Conformance is only meaningful when the manifest explicitly lists
        // inserted assumptions. An empty list is not a failure — but the
        // manifest must have the field populated (not null).
        bool hasDeclaredAssumptions = manifest.InsertedAssumptionIds != null;
        string detail = hasDeclaredAssumptions
            ? $"Manifest declares {manifest.InsertedAssumptionIds!.Count} inserted assumption(s): " +
              $"[{string.Join(", ", manifest.InsertedAssumptionIds)}]. " +
              "All results are branch-local to these declared assumptions."
            : "Manifest InsertedAssumptionIds is null. Cannot confirm branch-scope declaration.";

        return new ConformanceCheck
        {
            CheckId = "branch-scope-declared",
            Category = ScopeBoundaryCategory,
            Passed = hasDeclaredAssumptions,
            Detail = detail,
            ValidationType = BranchLocalValidationType,
        };
    }
}

# Phase V — Open Issues for Future Work

**Date:** 2026-03-14
**Phase V baseline:** 3016 tests passing, all 8 milestones (M46-M53) complete + 6 entry gaps (G-001 through G-006) closed.

This document captures known limitations, deferred work, and open questions from Phase V,
following the convention of PHASE_3_OPEN_ISSUES.md. These are inputs to any Phase VI
or gap-closure planning.

---

## Summary Table

| Issue    | Area                         | Status         |
|----------|------------------------------|----------------|
| ISSUE-1  | Observation/Env/Rep/Coupling Falsifiers | Deferred |
| ISSUE-2  | h_X / h_F Separate Mesh Parameters | Deferred |
| ISSUE-3  | Real External Targets        | Out of Scope   |
| ISSUE-4  | Imported Environment Tier    | Deferred       |
| ISSUE-5  | Branch Variant A0 from Phase III | Deferred  |
| ISSUE-6  | Shiab Variation in Convergence | Deferred     |
| ISSUE-7  | GPU Compute in Phase V       | Out of Scope   |
| ISSUE-8  | Non-Gaussian Pull Statistics | Deferred       |
| ISSUE-9  | solve-backgrounds Manifest Loading | Partial  |
| ISSUE-10 | solve-backgrounds Run Classification Persistence | Partial |

---

## ISSUE-1: Four Falsifier Types Not Actively Evaluated
**Area:** Falsification engine (M50)
**Status:** Deferred
**Description:** `ObservationInstability`, `EnvironmentInstability`, `RepresentationContent`, and `CouplingInconsistency` falsifier types are defined as string constants in `FalsifierTypes` but are not connected to any input data source in `FalsifierEvaluator`. The evaluator only actively fires `BranchFragility`, `NonConvergence`, and `QuantitativeMismatch`.
**Why deferred:** The input data types for these falsifiers (observation replay records, per-environment variance records, representation diffs, coupling matrices across branch variants) are not yet available as standardized types from upstream pipelines.
**Future work:** Wire each type to an appropriate Phase III/IV output record. Define the threshold policy for each. Add evaluator tests.

---

## ISSUE-2: Single Mesh Parameter Instead of Separate h_X and h_F
**Area:** Convergence (M47)
**Status:** Deferred
**Description:** `RichardsonExtrapolator` uses a single `MeshParameter` as `h`. The ARCH_P5.md specification calls for `h = max(h_X, h_F)` where `h_X` is the X-space mesh spacing and `h_F` is the fiber-space mesh spacing. These are conflated into one field.
**Why deferred:** The current mesh infrastructure does not distinguish X-space and fiber-space resolution. Implementing the distinction requires changes to `RefinementLevel` and downstream solvers.
**Future work:** Add `MeshParameterX` and `MeshParameterF` fields to `RefinementLevel`. Update `RichardsonExtrapolator` to compute `h = max(h_X, h_F)`.

---

## ISSUE-3: External Quantitative Targets Use Toy-Placeholder Data
**Area:** Quantitative Validation (M49)
**Status:** Out of Scope (current evidence tier)
**Description:** All external targets in the reference study use `targetProvenance = "synthetic-toy-v1"` with `evidenceTier = "toy-placeholder"`. Target values are constructed to be consistent with toy-geometry solve outputs, not from experimental data.
**Why deferred:** No real experimental comparison data is in scope for the current platform. Upgrading requires physically-motivated observables derived from real backgrounds.
**Future work:** Once "structured" or "imported" environment backgrounds are available, replace synthetic targets with targets derived from physical predictions (e.g., mass ratios, coupling constants).

---

## ISSUE-4: Imported Environment Tier Produces Structured Environments
**Area:** Environments (M48)
**Status:** Deferred
**Description:** `EnvironmentImporter` is implemented but currently generates structured simplex meshes rather than truly importing external data. The "imported" evidence tier is declared but the import pathway (from, e.g., a lattice gauge theory dataset or cosmological simulation) is not connected.
**Why deferred:** No external dataset format has been agreed upon.
**Future work:** Define an import schema for external geometry data. Implement a real import reader in `EnvironmentImporter`.

---

## ISSUE-5: Branch Variants in Reference Study Use Analytic A0 Forms
**Area:** Reference Study (M53)
**Status:** Deferred
**Description:** The Phase V reference study (`phase5_su2_branch_refinement_env_validation`) defines branch variants V1-V4 using analytic `A0` fields (`A0_e = 0.3*cos(π*m*x_e)*T_1 + 0.3*sin(π*m*x_e)*T_2`). These are not backed by Phase III background solver outputs.
**Why deferred:** Connecting Phase III solved backgrounds to Phase V branch variant specs requires a bridge layer that was deferred to keep the reference study self-contained.
**Future work:** Replace analytic A0 with solved Phase III backgrounds. Add a bridge type that exports `BackgroundRecord` → `BranchVariantSpec`.

---

## ISSUE-6: Shiab Variation Not Explored in Convergence Studies
**Area:** Convergence (M47), Reporting (M53)
**Status:** Deferred
**Description:** All Phase V convergence studies use the identity Shiab operator (S=F). The Phase V plan called for exploring whether convergence rates depend on the Shiab choice. This variation is not implemented.
**Why deferred:** Non-identity Shiab operators are not yet implemented in the main pipeline. The `ShiabVariationScope` is set to "identity-only" in ASSUMPTIONS.md.
**Future work:** Once non-identity Shiab is available, add a `ShibaVariantId` parameter to `RefinementStudySpec` and repeat convergence studies.

---

## ISSUE-7: No New GPU Kernels in Phase V
**Area:** CUDA/GPU (M46-M53)
**Status:** Out of Scope (by design)
**Description:** Phase V is a post-processing validation layer. No new CUDA kernels were added. All Phase V computation runs on CPU. GPU verification status (`ComputedWithBackend`, `ComputedWithUnverifiedGpu`) is propagated through all Phase V record types from the underlying Phase III/IV computations.
**Why deferred:** GPU acceleration of the validation layer would primarily benefit large-scale production runs, not the current research platform mode.
**Future work:** If Phase V runs become a bottleneck (e.g., large-N branch families), batch the branch executor calls through a GPU-accelerated eigenvalue solver.

---

## ISSUE-8: Pull Statistic Assumes Gaussian Tails
**Area:** Quantitative Validation (M49)
**Status:** Deferred
**Description:** The pull statistic `p = |Q_comp - Q_target| / sqrt(σ_c² + σ_t²)` is derived from Gaussian error propagation. The pass threshold of 5.0 corresponds to the Gaussian 5-sigma convention. Non-Gaussian uncertainties (e.g., systematic biases, discrete discretization errors) are not modeled.
**Why deferred:** Gaussian is the standard convention for toy-placeholder evidence. Non-Gaussian treatment requires a richer uncertainty model.
**Future work:** Add a `DistributionModel` field to `ExternalTarget` and `QuantitativeObservableRecord`. Implement non-Gaussian pull alternatives (e.g., asymmetric uncertainties, Student-t).

---

## ISSUE-9: `solve-backgrounds` Manifest Loading Requires Explicit Flag
**Area:** Entry Gap G-001 (CLI branch selection), `apps/Gu.Cli/Program.cs`
**Status:** Partial — warning added, explicit override available
**Description:** `solve-backgrounds` now accepts `--manifest <path>` and `--manifest-dir <dir>` to load declared branch manifests per spec. Without these flags, the command silently uses an inline default manifest (`ActiveTorsionBranch=trivial`, `ActiveShiabBranch=identity-shiab`) regardless of what `BranchManifestId` the study spec declares. A `[G-001] WARNING` is emitted but the default behavior still bypasses the declared manifest.
**Why deferred:** The `BranchManifestId` in `BackgroundSpec` is a string key, not a file path. No convention for co-locating manifest files with study JSON files currently exists.
**Future work:** Add a `ManifestSearchPaths` field to `BackgroundStudySpec` or a `manifest-registry.json` convention. Change the fallback from "silent inline default" to "hard error" once all study files carry manifest file paths.

---

## ISSUE-10: `solve-backgrounds` Run Classification Written to Console Only
**Area:** Entry Gap G-002 (trivial validation path), `apps/Gu.Cli/Program.cs`
**Status:** Partial — per-spec classification emitted to stdout/stderr but not persisted
**Description:** For `gu run` / `gu solve`, a `SolveRunClassification` record is written to `logs/solve_run_classification.json` in the run folder. For `solve-backgrounds`, per-spec classification is logged to the console only; no per-spec classification artifact is written to the output directory.
**Why deferred:** `solve-backgrounds` writes a shared output directory (not per-spec run folders), so the destination for per-spec classification records is unclear.
**Future work:** Write a `classifications.json` alongside `atlas.json` in the output directory containing the per-spec run classifications. Alternatively, embed `RunClassification` in `BackgroundRecord`.

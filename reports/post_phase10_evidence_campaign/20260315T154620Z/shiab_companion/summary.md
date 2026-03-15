# Phase XI Shiab Companion Summary

- Output root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion`
- Atlas: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/atlas.json`
- Bridge artifact root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/artifacts`

## Purpose

This paired run keeps the `first-order-curvature` Shiab path scientifically comparable
to the standard Phase XI path across all evidence categories updated in Phase XI.

## Phase X Results (Unchanged)

- The paired branch study remains evaluable and mixed. `3/5` quantities are invariant, and `gauge-violation` plus `solver-iterations` remain fragile.
- The paired bridge-derived refinement ladder remains `5/5` convergent.
- The paired direct solver-backed control ladder (Phase X) is still `5/5` convergent (zero-invariant).

## Phase XI Update (P11-M6): Nontrivial Direct Refinement for Paired Path

Phase XI added a nontrivial direct solver-backed refinement ladder for the paired `first-order-curvature` Shiab path.

- New background study configs use `SymmetricAnsatz` seed with `GaussNewton` solver on
  the structured fiber-bundle environments (env-refinement-2x2, 4x4, 8x8).
- All three levels produced admitted backgrounds (B2/B1) with nontrivial nonzero metrics.
- Residual norms across levels: L0=1.77e-9, L1=8.54e-9, L2=2.53e-8 (non-constant, growing with refinement).
- Gauge violations across levels: L0=0.201, L1=0.301, L2=0.752 (non-constant, growing with refinement).
- solver-iterations: constant 2 across all levels (trivially convergent).
- Refinement study result: `artifacts/nontrivial_direct_refinement_study_result.json`
- Convergence: solver-iterations is trivially convergent (constant=2); residual/stationarity/gauge-violation are non-convergent (values grow with refinement).
- This is genuine nontrivial solver-backed evidence but not convergence evidence.

Background configs: `studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase11_direct_nontrivial_shiab_l{0,1,2}.json`
Background records: `studies/phase5_su2_branch_refinement_env_validation/upstream/phase11_direct_nontrivial_shiab_l{0,1,2}/`
Refinement spec: `studies/phase5_su2_branch_refinement_env_validation/config/refinement_study_phase11_direct_nontrivial_shiab.json`
Refinement values: `studies/phase5_su2_branch_refinement_env_validation/config/phase11_direct_nontrivial_shiab/`

## Standard vs. Paired Comparison (Phase XI)

| Evidence Category | Standard Path (identity-shiab) | Paired Path (first-order-curvature) |
|---|---|---|
| Branch result | mixed (gauge-violation, solver-iterations fragile) | mixed (same: gauge-violation, solver-iterations fragile) |
| Branch admitted family | 4/7 method sweep, 1/8 label sweep | 4/7 method sweep, 1/8 label sweep |
| Bridge-derived refinement | 5/5 convergent | 5/5 convergent |
| Direct control ladder (Phase X) | 5/5 convergent (zero-invariant) | 5/5 convergent (zero-invariant) |
| Nontrivial direct ladder (Phase XI) | non-constant, growing (negative convergence evidence) | non-constant, growing (negative convergence evidence) |
| Residuals (nontrivial ladder) | L0=1.24e-9, L1=4.04e-8, L2=1.06e-7 | L0=1.77e-9, L1=8.54e-9, L2=2.53e-8 |
| Gauge violations (nontrivial ladder) | L0=0.197, L1=0.422, L2=0.996 | L0=0.201, L1=0.301, L2=0.752 |
| Representation-content fatal | still active | still active |

## Conclusion: Broader Shiab Scope Does Not Change Conclusions

The Phase XI nontrivial direct refinement ladder for the paired path reproduces
the same pattern as the standard path:
- Both paths produce nontrivial non-zero admitted backgrounds on all three mesh levels.
- Both paths show residuals and gauge violations growing with refinement (negative convergence evidence).
- Both paths retain the same mixed branch boundary (gauge-violation and solver-iterations fragile).
- The paired path residuals are slightly larger and gauge violations grow less steeply,
  but neither path demonstrates continuum limit convergence.
- The broader first-order-curvature Shiab scope does not eliminate the mixed branch result
  or produce convergence evidence where the standard path lacks it.

## Boundary

Phase XI closes the paired nontrivial direct refinement execution gap, but it does not show
paired branch robustness, candidate-linked evidence IDs, or external imported evidence.
All remaining open items (P11-001, P11-002, P11-004, P11-006) apply equally to both paths.

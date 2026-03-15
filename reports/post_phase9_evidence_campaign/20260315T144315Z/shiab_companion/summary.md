# Phase IX Shiab Companion Summary

- Output root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion`
- Atlas: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase9_first_order_shiab_real_atlas/atlas.json`
- Bridge manifest: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase9_evidence_campaign/20260315T144315Z/shiab_companion/artifacts/bridge_manifest.json`

## Purpose

This paired run extends the standard Phase IX evidence path beyond `identity-shiab`
to `first-order-curvature` on the same nontrivial objective-solve study design.

## Results

- Atlas classification: the paired run remains nontrivial. All admitted and rejected records are `objective-solve`, and the admitted family includes one zero-seed solve plus three symmetric-ansatz solves.
- Atlas admissibility: `4/7` attempted backgrounds are admitted and `3/7` are rejected under the original tolerances.
- Branch robustness record: `overallSummary = mixed`, not `inconclusive`.
- Branch quantities: `residual-norm`, `stationarity-norm`, and `objective-value` classify as invariant/robust across the paired family. `gauge-violation` and `solver-iterations` remain fragile.
- Refinement study: five continuum estimates, zero failure records, all classified convergent.
- Evidence boundary: the refinement study is still bridge-derived from the admitted paired atlas. It is not yet a direct solver-backed refinement family.

## Boundary

This closes the old Phase VIII gap where the non-identity Shiab run only had a
single admitted variant and therefore could not support a branch comparison.
It still does not show Shiab robustness, because the paired branch study is now
evaluable but mixed rather than robust.

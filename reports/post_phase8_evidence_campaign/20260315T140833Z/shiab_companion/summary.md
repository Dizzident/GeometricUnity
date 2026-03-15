# Phase VIII Shiab Companion Summary

- Output root: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion`
- Atlas: `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/upstream/phase8_first_order_shiab_real_atlas/atlas.json`
- Bridge manifest: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase8_evidence_campaign/20260315T140833Z/shiab_companion/artifacts/bridge_manifest.json`

## Purpose

This paired run extends the standard Phase VIII evidence path beyond `identity-shiab`
to `first-order-curvature` on the same nontrivial objective-solve study design.

## Results

- Atlas classification: the paired run is no longer trivial-control. The admitted and rejected records are all `objective-solve`, and the symmetric-ansatz cases are explicitly labeled `seedSource = symmetric-ansatz`.
- Atlas admissibility: only `1/4` attempted backgrounds is admitted; the remaining `3/4` symmetric-ansatz solves fail the same admissibility tolerances as the standard path.
- Branch robustness record: `overallSummary = inconclusive`
- Branch quantities: all five tracked quantities are indeterminate because only one paired branch variant survives admission.
- Refinement study: five continuum estimates, zero failure records, all classified convergent.

## Boundary

This closes the old Phase VII gap where the non-identity Shiab run only existed on a
trivial residual-inspection path. It does not show broad Shiab robustness yet, because
the paired nontrivial atlas still collapses to a single admitted background and leaves
the branch comparison inconclusive.

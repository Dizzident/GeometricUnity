# Implementation P302: Identity-Split Particle Normalization Audit

## Purpose

Phase302 audits a loophole left open by Phase300 and Phase301. Phase300 ruled out a single target-independent common normalization for the Phase299 identity-split W/Z production replay rows. Phase301 ruled out changing the promoted fermion transition. Phase302 checks whether a common source scale combined with particle-specific SU(2) multiplicity or Casimir factors can repair the identity-split rows.

## Inputs

- Phase24 identity-rule readiness.
- Phase26 electroweak mixing-convention readiness.
- Phase213 source-lineage blocker matrix.
- Phase225 SU(2) normalization representation compatibility audit.
- Phase249 invariant-origin search.
- Phase299 identity-split production W/Z replay.
- Phase300 common-normalization audit.
- Phase301 transition sweep.

## Behavior

The audit forms a grid of Phase300 common scale candidates against W/Z particle laws:

- no particle-specific factor;
- charged-axis multiplicity factors;
- SU(2) adjoint axis-count factors;
- adjoint/fundamental dimension factors;
- adjoint/fundamental Casimir factors;
- one post-hoc replay-row equalizer kept as a non-source diagnostic control.

The search does not use target observables to construct candidate laws. Target values are used only after candidate construction to evaluate raw/common gates.

## Result

Phase302 intentionally does not promote a W/Z mass claim. It records that a source-invariant raw/common numerical lead can exist, but the lead remains non-promotable because:

- W and Z branch-stability gates are still not passed;
- the particle-specific factor has no application theorem;
- Phase225 and Phase249 still block the SU(2) invariant from being source-backed for W/Z application;
- Phase201/P209 source-lineage rows remain unfilled.

## Output

- `studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit.json`
- `studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json`

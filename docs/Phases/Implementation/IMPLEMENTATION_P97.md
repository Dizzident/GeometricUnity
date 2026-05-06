# Implementation P97: Identity Fermion Lift Quotient Derivation

## Objective

Clear the blocker requiring a derivation of the identity fermion-space lift
against the implemented Phase3 connection-space gauge quotient.

## Changes

- Added `studies/phase97_identity_fermion_lift_quotient_derivation_001/derive_identity_fermion_lift.py`.
- The script emits a machine-checkable derivation artifact covering:
  - Phase12 branch A;
  - Phase12 branch B;
  - Phase94 `2x2` exact refinement modes;
  - Phase94 `4x4` exact refinement modes.
- The derivation claim is deliberately narrow:
  - the implemented Phase3 quotient acts on connection perturbation coefficients;
  - the current fermion coefficient space is a distinct domain;
  - therefore the compact identity fermion projector is admissible for the current executable quotient.

## Validation

```bash
python3 studies/phase97_identity_fermion_lift_quotient_derivation_001/derive_identity_fermion_lift.py
jq -e . studies/phase97_identity_fermion_lift_quotient_derivation_001/output/identity_fermion_lift_connection_quotient_derivation.json
jq -e . studies/phase97_identity_fermion_lift_quotient_derivation_001/output/identity_fermion_lift_quotient_derivation_summary.json
```

Result:

- Derivation status: `identity-fermion-lift-derived-for-current-connection-quotient`.
- The artifact verifies that connection and fermion projector domains are distinct.
- The artifact verifies that all current fermion projectors are full-rank compact identities.
- The artifact explicitly does not claim a nontrivial gauge quotient on fermion states.

## Outcome

The identity fermion-space lift blocker is cleared for the current implemented
connection-space quotient. A future nontrivial fermion gauge quotient remains a
separate theory and implementation extension.

## Remaining Blocker

- The Phase96 materialized `4x4` boson vector is source-backed and replay-compatible, but it is not yet tied to a selector eigenmode identification.

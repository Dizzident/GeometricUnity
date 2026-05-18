# Implementation P160: Phase-Sensitive Transition Rule Materialization

## Status

Implemented `studies/phase160_phase_sensitive_transition_rule_materialization_001`.

## Purpose

P160 turns P159's clean branch projectors into a concrete P140 transition-rule candidate by using the complex coupling phase of the strongest projector-supported off-diagonal transition.

## Result

The phase writes `phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_or_transition_rule_intake_template.json` when the transition rule passes its target-independent gates.

## Gate

The materialized rule requires:

- P159 projectors promotable
- a complex-conjugate off-diagonal pair
- low conjugacy residual
- nontrivial phase separation
- `externalTargetValuesUsed=false`

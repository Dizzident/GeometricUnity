# Implementation P128: Fermion SU(2) Generator Sector Observable

## Status

Implemented `studies/phase128_fermion_su2_generator_sector_observable_001`.

## Purpose

P127 materialized gauge-basis energy fractions but found the repaired quality modes were mixed. P128 checks a stronger target-blind observable: SU(2) generator-sector content using adjoint spin-1 generator moments and `T3` eigenbasis fractions.

## Method

The study reads the P125 source-family-enriched modes, P126/P127 gates, and the Phase12 spinor representation. It computes generator expectations using:

`(T_a)_{bc} = -i epsilon_{abc}`

It also computes `T3` eigenbasis fractions in the `m=-1,0,+1` basis. Promotion requires a dominant `T3` fraction of at least `0.80`, a dominance gap of at least `0.20`, and coherent generator polarization.

## Result

Terminal status:

`fermion-su2-generator-sector-observable-mixed-sector-blocked`

The generator-sector observable is materialized, but it is not promotable to a W/Z transition rule. The repaired quality modes remain mixed in the `T3` eigenbasis and have near-zero SU(2) generator polarization. Phase12 chirality is still trivial.

## Next Work

Derive a different target-blind fermion sector identity source. The remaining requirement is a stable non-mixed fermion charge/weak-sector label, or an equivalent nontrivial chirality/conjugation transition table, before rerunning the corrected W/Z transition sweep.

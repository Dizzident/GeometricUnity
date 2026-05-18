# Implementation P164

P164 adds a source-level W/Z bridge candidate census.

## Motivation

P163 showed that candidate-8 is already far below the W/Z raw-amplitude gate on the Phase91 source-selected pair. That ruled out Phase94 repair/matching as the cause of the absolute bridge failure.

## Implementation

- Added `studies/phase164_source_level_wz_bridge_candidate_census_001`.
- Scans all existing Phase12 fermion variation matrices.
- Computes forward and reverse source-pair matrix elements using the same normalization as P163.
- Records whether any existing source matrix clears the P110 raw-amplitude gate.

## Result

Terminal status: `source-level-wz-bridge-candidate-census-no-existing-source-clears-gate`.

- candidate count: 24
- promotable candidate count: 0
- best candidate: `candidate-4`
- best raw-to-target ratio: `2.3787683544405456E-14`

No existing Phase12 variation matrix clears the source-level W/Z raw-amplitude gate on the fixed Phase91 source-selected pair. The next blocker is a new target-independent W/Z bridge source or source-level amplitude normalization derivation.

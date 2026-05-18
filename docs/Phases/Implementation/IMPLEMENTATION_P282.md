# Phase 282 - Branch-Local Direct Invariant Census

Phase282 searches the repaired branch-local Phase12 finite-difference coupling matrices for target-independent local direct W/Z bridge invariants beyond the single P190 matrix-element law.

The census evaluates three invariant families without using W/Z target values for search ordering or stability decisions:

- single-candidate branch-local finite-difference matrix-element magnitude;
- single-candidate contribution share within the full branch-local variation subspace;
- full branch-local variation-subspace root-sum-square norm.

Target-implied raw amplitude is reported only as a post-construction comparison gate, matching the existing fail-closed policy.

Current result:

- `terminalStatus=branch-local-direct-invariant-census-no-promotable-local-source`
- `branchLocalInvariantCensusPassed=true`
- `targetObservablesUsedForSearch=false`
- `directInvariantPromotesWzMasses=false`
- `newLocalDirectInvariantSourceFound=false`
- no single-candidate or full-subspace invariant clears the post-construction raw-scale gate

Artifacts:

- `studies/phase282_branch_local_direct_invariant_census_001/output/branch_local_direct_invariant_census.json`
- `studies/phase282_branch_local_direct_invariant_census_001/output/branch_local_direct_invariant_census_summary.json`

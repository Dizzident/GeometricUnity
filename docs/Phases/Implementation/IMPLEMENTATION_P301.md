# Implementation P301: Identity-Split Production Transition Sweep

Phase301 tests whether the Phase299 identity-split replay failure is caused by the fixed promoted fermion transition `4 -> 6`.

The phase materializes the same production analytic W and Z source rows used by Phase299, but sweeps every ordered pair of Phase91 promoted fermion modes on each sibling background. It avoids writing hundreds of full replay packages; instead it computes the accepted `unit-modes` analytic Dirac-variation matrix elements directly from the source-backed perturbation vectors.

Inputs:

- Phase299 identity-selected W/Z production replay rows.
- Phase300 common-normalization audit.
- Phase282 all-local-invariant census.
- Phase91 promoted fermion mode files.
- Phase12 spinor/source-mode metadata.
- Phase213 source-lineage blocker matrix.

Result:

- `terminalStatus=identity-split-production-transition-sweep-no-promotable-transition`
- `identitySplitProductionTransitionSweepPassed=true`
- `pairCount=132`
- `bothRawGatePassingPairCount=0`
- `rawAndCommonPassingPairCount=0`
- `stableRawCommonPassingPairCount=0`

Decision:

Do not repair the Phase27 identity split by changing the promoted fermion transition. A production analytic sweep over every promoted ordered transition found no transition that clears both W/Z raw gates, and none clears the raw/common/stability gate stack.

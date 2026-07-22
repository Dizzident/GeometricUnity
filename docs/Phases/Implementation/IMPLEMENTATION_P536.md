# Phase 536 — Trajectory-forensics replay

Phase536 implements Amendment A23's unchanged-behavior replay of the Phase534
reduced scalar HMC. The original v1 preregistration contract was written before
the first execution. Cross-review then required stronger telemetry and contract
checks, so v1 is preserved and a v2 repair contract was frozen before the
repaired rerun. Both bind the Phase533 contract and summary, Phase534 contract,
program, and summary, and Phase535 summary.

The phase reuses the frozen polynomial, seed tables, seed offset, initial
scales, initial step size, leapfrog count, acceptance rule, and adaptation
schedule. Instrumentation consumes no RNG values and records bounded telemetry:
warmup and retained nonfinite/divergence counts, twenty adaptation windows per
chain, one first failure per chain with stage and iteration, and finite maximum
magnitudes for state, momentum, gradient, and energy. Proposed state is captured
before any rejection rollback and is distinct from post-decision state.

Fail-closed source-matched replay checks require exact Phase534 aggregate acceptance counts,
nonfinite counts, divergence counts, and final step sizes. The output cannot
repair or relabel Phase534, reopen Phase535, validate the complete lattice or
production, or support physical-unit claims. Phase534 has no immutable
per-trajectory trace, so Phase536 does not claim exact trajectory identity.

`promotedPhysicalMassClaimCount=0`.

## Targeted result

The Release replay returned
`aggregate-failures-observed-warmup-and-retained`. It matched both Phase534
table acceptance counts, all 108 nonfinite trajectories, all 137 divergent
trajectories, and all eight final adapted step sizes. The partition was:

- warmup: 1 nonfinite and 3 divergent trajectories;
- retained: 107 nonfinite and 134 divergent trajectories.

The first recorded failure was an energy divergence in warmup iteration 774.
Across chains, finite state, momentum, gradient, and energy magnitudes grew by
many orders before nonfinite arithmetic; one chain's first nonfinite event was
observed at the final-gradient stage. These observations localize aggregate
failures to both execution segments under the source-matched replay. They do
not resolve the cause, diagnose a particular defect, or select or execute a
repair.

Frozen/result hashes:

- preserved pre-review v1 contract: `e20698b88c43abb120491eee9c985889d6996e2f43736931981ca4d6c725b225`;
- repaired-run v2 contract: `0ea6a4bfc780ded43d2533255becf6b3445e102adf0606b8c42f3fe30a292a8f`;
- program: `2983d7126bdd9ac6c98d9b8a14ad441f92bf7aa3daeb569a423df12ea7335aa4`;
- project: `cdd6f0b4e4b9c72196431282fc7f42508e52e0f6a10b88a948a157bbdabd220b`;
- full and summary output: `1296ed7826071a6fdccf3dd2182461882055bb1153af1c198e9d7b9b7c6ed9ae`.

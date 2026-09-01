# Phase571 bounded reset checkpoint transition probe

## Status

Executed v4 terminal
`longer-trajectory-local-kernel-lever-supported`. The exact-bound Phase570 v7
result has verdict `invariant-directional-under-resolution-not-localized` and
`phase571TransitionProbeGateOpen=true`. The v1 contract binds the Phase570 v7
contract/program/project/full+summary package, all direct scientific inputs,
and Phase571's program/project/lineage. The pending scaffold program and
contract are preserved byte-for-byte under `lineage/pending/`. The complete
unexecuted v1 program, project, contract, and lineage package is preserved
under `lineage/v1/`; the complete unexecuted v2 package is preserved under
`lineage/v2/`. V3 and its byte-exact pre-RNG resource-refusal full and summary
outputs are preserved under `lineage/v3/`. All are exact-bound by v4.

V3 failed closed before RNG construction because IEEE-754 multiplication
produced `176.64000000000001` while the contract froze an exact comparison to
`176.64`. V4 changes no ceiling or scientific control: it freezes `40`
milliseconds per force evaluation and derives exactly `4416*40=176640`
milliseconds against the unchanged `300000` millisecond ceiling. Seconds are
reported only as the exact integer result divided by `1000.0`.

## Implementation

The study references the exact-bound public Phase545 deterministic proposal
kernel and reconstructs the registered Phase548 SD2/Id0, coefficient `0.5`,
theta-zero extent-three target. The six Phase548 final checkpoint files are
immutable initial positions.

Each disclosed momentum is reused across all checkpoints and proposal arms,
with its exact antithetic negative. Every row clones its checkpoint again.
The accepted state is used only to compute jump diagnostics and is discarded.
The original checkpoint vector is hashed before and after every row; any hash
change or rejected-state mismatch selects the reset-integrity terminal.

The four proposal arms are `(0.06,8)`, `(0.03,16)`, `(0.015,32)`, and
`(0.06,32)`. The first three keep trajectory length `0.48`; the last has
length `1.92`. Forward energy error, Metropolis acceptance, a reverse proposal,
and position/momentum reversal errors are recorded. RMS energy-error order is
computed only across the paired same-length arms.

The mesh incidence construction produces Euclidean scalar projectors onto
`E=im(d0)`, `W=ker(d1) intersect E-perp`, `C=E direct-sum W`, and `Cperp`.
For every jump it reports squared projection norms, decomposition leakage,
ESJD per forward force evaluation, and the three eigenvalues of the closed
coefficient Gram matrix. The latter give a basis-invariant rank-one alignment
and distance to the rank-one closed-coordinate set. These are coordinate
diagnostics only and carry no gauge, quotient, redundancy, or physical-mode
interpretation.

The external resource check derives both forward and reverse proposal work
from the exact arm/checkpoint/seed/sign menu. CPU seconds are derived from the
frozen per-force-evaluation estimate, and peak bytes are derived from an exact
eight-row allocation menu. All ceilings must pass before RNG construction or
proposal allocation. Phase548 raw, execution-offset, and protected seed
namespaces are derived from the exact-bound Phase548 contract and must be
pairwise distinct and disjoint from Phase571's disclosed momentum seeds. RNG
use is real, but no chain,
warmup, adaptation, target sampling, or configuration retention occurs.

The prospective terminal classification compares the long arm to baseline at
each of the six immutable endpoints in all four fixed projector panels. Each
ratio is the paired mean accepted ESJD per forward force evaluation. The local
kernel lever is supported only when at least five endpoints have at least
three ratios `>=1.25` and no ratio `<0.8`. Conclusive responses are mixed when
improvement-threshold responses coexist, or degradation coexists with
non-degradation; remaining conclusive responses are not supported.
Zero baselines and non-finite ratios are inconclusive. A prospective battery
constructs endpoint/panel observations and reaches supported, mixed, and
not-supported through the production aggregation function. A zero-baseline
fixture passes through that same function and must produce `Conclusive=false`,
`Supported=false`, `Mixed=false`, then route to the frozen inconclusive
terminal. Same-tau energy order,
force accounting, reversal, antithetic construction, and projector leakage
all fail earlier, as do reset-integrity and non-finite/divergent transitions.

## Executed outcome

V4 passed its exact bindings, upstream gate, prospective classification
battery, seed-namespace controls, derived resource checks, projector controls,
immutable-reset controls, reversal checks, and same-tau energy-order check.
The run produced 96 reset-only proposal rows. Observed forward and reverse
force evaluations were exactly 2208 each, matching the frozen 4416 total;
derived CPU cost was 176640 milliseconds and derived peak allocation was
8521036 bytes.

The observed same-tau RMS absolute delta-H values were approximately
`0.1873427`, `0.0475886`, and `0.0119484` for the baseline, fine, and finer
arms. Their observed orders were `1.9769921` and `1.9938011`, inside the
frozen `[1.25, 2.75]` band. Reversal relative errors stayed below
`3.52e-16`, projector leakage below `4.54e-17`, and no accepted state was
carried between proposals.

All six immutable endpoints qualified under the frozen movement rule. Every
endpoint improved in all four preregistered panels; long-to-baseline accepted
ESJD-per-forward-force ratios ranged from about `2.015` to `4.915`, with none
below the `0.8` degradation floor. This selected
`longer-trajectory-local-kernel-lever-supported`.

The result supports only a local, reset-from-checkpoint kernel-feasibility
lever: longer trajectories improved this fixed paired proposal diagnostic.
It is not a Markov-chain run, target sample, mixing or stationarity result,
configuration-retention result, default selection, or production
authorization. It does not reinterpret the invariant directions as gauge,
quotient, redundant, or physical modes, and it does not change the Phase548
or Phase570 terminals.

## Outputs and exact hashes

- `output/bounded_reset_checkpoint_transition_probe.json`
- `output/bounded_reset_checkpoint_transition_probe_summary.json`
- v4 program `Program.cs`:
  `79150999c12d0832f58189c8cc6a4c0b066d2d0f3b4d0876a7ce920aa2d3a62f`
- v4 project:
  `09e950d97b94d4063a06f308160ed368c02c646322b6a92bdca01618f690debc`
- v4 contract:
  `83416598aef3e072d33049b98c5319f5b79a96c08e41ddfcd1ae391573dc591d`
- v4 lineage record:
  `4188db8a45f68385eeea25422d8bc20399aa45a1dfe066a21029413e2c3b608f`
- final full and summary outputs, byte-identical:
  `fc5ff726ac4ccdf534be7ba5a9f800557bdb85f58ecb931f66fbc4451172807d`
- preserved v3 pre-RNG `resource-refusal` full and summary outputs,
  byte-identical:
  `9bd3aa94eb1a829d97dc14bed9f0b9ec056b21e97e3404e5040ec0b4efb1c1ae`

External review remains pending. No physical-unit or GeV claim is permitted;
`promotedPhysicalMassClaimCount=0`.

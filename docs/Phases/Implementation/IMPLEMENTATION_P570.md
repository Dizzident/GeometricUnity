# Phase570 implementation — registered-target directional-resolution replay

Phase570 is a deterministic, retrospective replay of the six committed
Phase548 chains. Its contract exact-binds its own numerical program, the
Phase548 configuration and retained artifacts, the Phase549-552 adjudication
lineage, the Phase569 lane boundary, and every directly used core source.

The scalar edge space is decomposed without thresholds into:

- `E = im(d0)`, dimension 80;
- `W = ker(d1) intersect E-perp`, dimension 4;
- `C = ker(d1) = E direct-sum W`, dimension 84;
- `C-perp`, dimension 1131.

Tensoring with the three algebra axes gives dimensions 240, 12, 252, and
3393. The program verifies closure, orthonormality, and cross-orthogonality
before replay: closure is first checked exactly on integer generators, while
the orthonormal floating-point basis uses a separately declared `2e-13`
tolerance and must agree with Phase550's certified dimension 252. Every
retained position is reduced to basis-invariant projector
norms. Its closed component is represented as an `84 x 3` coefficient matrix;
the eigenvalues of its `3 x 3` Gram matrix determine rank-one alignment and
the within-`C` squared distance to the closed rank-one valley. The separately
reported full-space distance adds the `C-perp` norm squared.

The replay independently reconstructs the Phase548 HMC transitions and
requires every accept/reject decision, relative delta-H, retained count, and
final-position bit pattern to agree. Table-level diagnostics reuse the frozen
Phase548 split-rank/folded R-hat and bulk/tail ESS thresholds. Tail ESS is
implemented as the minimum ESS of the pooled-rank lower-5% and upper-5%
indicator series, rather than folded-rank ESS. Per-chain
lag-one correlations, batch drift statistics, and per-subspace movements are
also recorded. IID/AR(1), drift, projector rotation/permutation, rank-one and
transverse, checksum, and replay-decoy batteries exercise the actual
diagnostic routines before audited numerical records are parsed. Frozen drift
aggregation across the decision series has precedence over enrichment.
Rank-one enrichment requires stable closed norm and largest Gram eigenvalue,
plus failures of both absolute and relative within-`C` distance diagnostics.

The v1, v2, and v3 contracts are retained byte-for-byte and marked unexecuted.
The exact v1 program bytes were recovered by replaying the original `Add File`
and two pre-freeze patches from local tool history; their SHA-256 exactly
matches the v1 contract. The v2 and v3 programs are preserved at their live
paths and in byte-identical lineage copies. V4 exact-binds all predecessor
programs, contracts, and lineage records. V4 additionally exact-binds the project
file, disables default compile items, and makes `ProgramV4.cs` the sole
compiled study source; predecessor sources remain evidence, not build inputs.

V4 derives the replay cost as
`6*400*(8+1) = 21600` force evaluations, and refuses resource-invalid work
before mesh, replay, or RNG allocation (contract bytes and small battery arrays
may already exist). Its checked allocation menu derives a conservative
208,786,812-byte peak from the declared DOF, retained series, replay vectors,
incidence projectors, diagnostics scratch, topology and evaluator reserves.
The exact menu must match the contract and structurally excludes the forbidden
`dof*dof` dense-Hessian shape. No dense Hessian is allocated. RNG reporting
depends on whether the committed replay RNG was reached, as detailed below.

The impossible combined closed/rank-one terminal was removed because the two
frozen predicates require opposite closed-norm outcomes. A classification
truth-table battery and production call the exact same selector. It applies
invalid/control failure precedence, then drift, then contradictory combined
state and enrichment. The battery explicitly verifies that drift wins over a
closed-plus-rank-one input; a contradiction without drift fails closed. IID
and AR(1) diagnostics must also
fall within frozen absolute R-hat/ESS bands, with frozen AR(1)-to-IID ESS
degradation bands. A successful clean
scientific terminal may open `phase571TransitionProbeGateOpen`, which permits
only registration of a separate prospective transition probe.

RNG reporting is path-sensitive. Contract, battery, lane, resource, and
incidence failures occur before replay RNG allocation and report
`rngUsed=false`. Replay-identity or later diagnostic failures report
`rngUsed=true`, as do successful replay terminals. These terminal sets and
semantics are contract-bound.

## Executed outcome

The final v7 preregistration executed successfully. All six committed
Phase548 chains reproduced every accept/reject decision and the final
position bit pattern, with worst relative delta-H deviation zero. The
known-answer battery, exact incidence closure, numerical projector controls,
and resource checks passed. None of the frozen decision series triggered the
cross-chain drift rule. Neither closed-sector enrichment nor rank-one-valley
enrichment was established, so the verdict is
`invariant-directional-under-resolution-not-localized` and the terminal is
`registered-target-directional-resolution-replay-invariant-directional-under-resolution-not-localized`.
Both `phase571TransitionProbeGateOpen` and
`phase572AdjudicationGateOpen` are true.

This is a replay of already committed trajectories, not new sampling. It
supports the narrow conclusion that the measured invariant-directional
diagnostics did not localize the Phase548 failure to either preregistered
subspace hypothesis. It does not establish convergence or sampler causality,
does not label any direction gauge or redundant, does not rehabilitate the
Phase548 terminal, and does not select or authorize a production
configuration.

V4 executed only its pre-audited known-answer battery and terminated
`known-answer-battery-failed`. Its full and summary outputs were byte-identical
and are preserved as first-class lineage artifacts before v5 can overwrite
the live paths. All actual Diagnose/Drift absolute bands passed. The auxiliary
sinusoidal innovation failed two unchanged controls: `phi=0.5` produced lag
about `0.1368` instead of greater than `0.25`, and `phi=-0.5` produced lag
about `-0.1159` instead of less than `-0.2`.

V5 changes only that auxiliary fixture. It uses 256 deterministic
stateless-normal white values with frozen key 17 and index offset 1000, then
applies the same AR coefficients and unchanged lag thresholds. The v5 contract
asserts those construction constants and thresholds. It also exact-binds the
v4 contract, program, historical project file, and both immutable v4 output
copies, while the current exact-bound project compiles only `ProgramV5.cs`.

V6 corrects only that exact-bound lineage diagnosis. It preserves and binds
the complete v5 program, contract, project, and lineage bytes, retains output
schema 5, and changes no fixture, threshold, production logic, or output. The
current exact-bound project compiles only `ProgramV6.cs`.

V7 is a binding-integrity-only repair. V6 declared 53 rows but only 52 unique
IDs because `phase570-v5-lineage-record` appeared twice. V7 removes the
duplicate, preserves the distinct v5 lineage-copy path, and machine-checks
both ID and path uniqueness. Its 52-row repaired base plus five new lineage
bindings yields 57 unique exact bindings. Complete v6 program, project,
contract, and lineage bytes are retained under `lineage/v6`; the exact-bound
project compiles only `ProgramV7.cs`. Scientific configuration, fixture,
thresholds, output schema, and outputs are unchanged.

## Exact final and failed-attempt hashes

- v7 program `ProgramV7.cs`:
  `058c29aa3c3d04b02b14ec707e34007ed5201f1be94da0c8393d5a25a1da0f58`
- v7 project:
  `722aa80cc0fe7ec9eac33522ec3cb93109e5a143b2a99382a5b429a2c440c145`
- v7 contract:
  `650f5f3191c10b3f2ba2ed2fdaf2be48501ddeeb5f948d6674f1093cd75a1bd0`
- v7 lineage record:
  `e7e06067e6c3816e2df3290adafbdffc609ee71db44c34f39d13f0ded252c6fd`
- final full and summary outputs, byte-identical:
  `37c1b815bb7b8af0cbda7bcb6f5bab4cafc69cd38f4dcd6cfa496651c23f809c`
- preserved v4 `known-answer-battery-failed` full and summary outputs,
  byte-identical:
  `f7a750818402e77e9efd53ab05efa6a96eaec53cdbb05d1d3683b0a06ffc6274`

The output is not a new sample and is not a convergence assessment. It does
not alter an earlier terminal, identify a gauge sector, apply a quotient,
select an intervention, or authorize any production or physical claim.

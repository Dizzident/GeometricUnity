# Complete-Lattice Flat-Sector Census Plan (Amendment A30)

## Status

Planning only. Phases550-555 are allocated in
`docs/Phases/PHASE_NUMBER_REGISTRY.md` and are NOT implemented, frozen,
registered, or executed. Nothing in this document authorizes sampling, creates
a Phase481 pack, selects a production default, reopens Phase535, satisfies
Phase458 G3/G4/G5, discharges O4, applies a source contract, or supports a
physical-unit claim. Phase548's preserved terminal
`pilot-executed-diagnostics-invalid` and Phase549's
`adjudication-confirms-reported-terminal` are immutable and are not reread,
rehabilitated, or reinterpreted by any phase below.

## Why this block, and not a sampler change

A29 left the lane with a healthy-looking execution and failing convergence
gates, and attributed the gap to the spectrum. Four facts re-read from the
committed sources change which next step is decidable. Each is checkable from
the repository rather than from the A29 narrative.

1. **The smallest-eigenvalue number is one-sided.** Phase548's probe returns
   `shift - dot(v, shift*v - Hv)`, which is the Rayleigh quotient of the unit
   iterate. For a symmetric operator that is an UPPER bound on the smallest
   eigenvalue, so the committed record establishes only
   `lambdaMin <= 1.2273204745838484e-06`. The condition number is therefore a
   lower bound, and `slowestModeTrajectoryLengthEstimate = 2835.769...` is pi
   over the square root of an upper bound on a quantity that may be exactly
   zero. It is a diagnostic artifact. No successor may cite it as a
   measurement, and the derived "short by a factor of fifteen" reading inherits
   the same one-sided error.
2. **Extent three is the enforced floor.** `CreateUniform4DPeriodic` throws
   below three, so no smaller well-formed periodic target exists. The counts
   asserted in `tests/Gu.Geometry.Tests/Mesh4DTests.cs` are 81 vertices,
   `15*n^4 = 1215` edges, `50*n^4 = 4050` faces, Euler characteristic zero.
3. **`beta` is a recorded label in this execution path.** Phase548 reads it
   from the contract and writes it into the summary and checkpoint header, but
   no factor of `beta` enters the executed value, which is exactly
   `op.ComputeJointGradient(omega, thetaZero, massMatrix).Objective`. Changing
   `beta` would change nothing without changing the operator, and any
   `beta`-dependence claim off the current source would be fabricated.
4. **The frozen pack cannot buy the missing path length.** Under the Phase546
   rule at extent three with three chains, `trajectories * leapfrogSteps <=
   3551` per chain, so total path length per chain is at most `3551 * stepSize`
   — about 213 at `0.06` and about 284 at `0.08`, against the 192 the pilot
   spent. At extent four the memory rule admits only two chains
   (`1048576 + 11520*80 = 1970176` bytes per chain; three chains reach
   `5910528` against the `5360704` ceiling and are refused).

Together these say the decisive next measurement is deterministic, costs no
seeds, and can be made on bytes that are already committed.

## Structural expectation, recorded with its falsifier

Recorded so that a later reader can see what was expected before the
measurement, and explicitly NOT assumed by any gate below.

At the origin the second-order form is exactly `(C.d)^T M (C.d)`, because a
single-edge basis vector has vanishing self-bracket and therefore
`F(e_i) = d e_i` exactly. Its kernel therefore contains the kernel of the
discrete exterior derivative on 1-forms, whose dimension on this complex is
`(243 - 3) + 4*3 = 252`: 240 exact plus 12 harmonic, from first Betti number
four and algebra dimension three. Stronger: if a 1-form is closed and takes all
its edge values along one fixed su(2) axis, its self-bracket vanishes
identically, the discrete curvature vanishes, and the value is exactly zero
along the entire ray — an 84-dimensional exactly flat subspace for each axis
choice (80 exact plus 4 harmonic scalar closed 1-forms).

Falsifiers: a measured nullity below 252 would indicate that the discrete
`d.d = 0` identity fails under the committed orientation conventions; a nullity
above 252 would indicate that the registered contraction annihilates part of
the image of `d`, which is a property of the action family and not of gauge.
Either outcome is a first-class result. **Phase550's terminal is keyed to
certification quality, never to which outcome the data support**, so there is
no favorable answer to steer toward.

Note also that flatness of this kind is flatness through the origin only: at a
general point the same directions acquire a first-order response, so the
transverse scale is what sets the equilibrium extent of the flat coordinates.
That transverse normalization is exactly the reserved O4 item; see the
firewalls below.

## Phase550 — deterministic flat-sector and spectral census

**Question.** What is the exact structure of the second-order form of the
registered action, at the origin and at the six preserved pilot positions, and
does the action possess exactly flat unbounded directions?

**Decidable now** because the second-order form at the origin is exact with no
finite differences; because the gradient is an exact cubic along any ray, so
exact second-order products are available at any base point via a four-point
antisymmetric formula; because the checkpoint schema retains `position`, giving
six committed non-origin configurations; and because 3645 admits dense linear
algebra.

**Exact-binds.** `src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs`,
`CurvatureAssembler.cs`, `CpuMassMatrix.cs`,
`src/Gu.Geometry/SimplicialMeshGenerator.cs`, the Phase548 contract and
summary, and all six Phase548 checkpoint files.

**Arms, in order, each a hard precheck for the next.**

- **A. Structural prechecks (exact/integer).** `d.d = 0` on the actual
  incidence and orientations; the rank of the incidence coboundary in exact
  fraction-free integer arithmetic, giving a threshold-free integer;
  `F(e_i) = d e_i` for single-edge basis vectors; symmetry of the assembled
  form within a frozen tolerance; and agreement of the four-point cubic
  extraction with a six-point formula at roundoff level, which TESTS the
  polynomiality premise instead of assuming it.
- **B. Exact-flatness test with a negative control.** For a frozen menu of
  closed forms tensored with a fixed algebra axis, assert nonzero norm, assert
  `d v = 0` and vanishing self-bracket SEPARATELY, then evaluate along a frozen
  ladder up to `t = 1e3`. The negative control is a closed but non-parallel
  direction, which MUST be strictly positive and scale as the fourth power.
  Without the negative control this arm is fail-open and a zero could mean only
  that the constructed vector was zero.
- **C. Nullity, two-sided.** Lower bound by an explicit orthonormalized null
  basis with reported residuals; this rests on arm A and is the load-bearing
  number. Upper bound by dense LDL^T inertia counts over a frozen shift ladder
  whose smallest rung sits ABOVE the roundoff floor `n*eps*norm ~ 1.6e-12`
  (proposed smallest rung `1e-9`). Every floating-point rung is recorded as
  threshold-conditional; no rung may be described as proving an exact zero.
- **D. Conditioning of the complement.** Largest eigenvalue with a residual
  bound; smallest eigenvalue on the orthogonal complement of the measured null
  basis by deflated Lanczos with the a-posteriori bound
  `|lambda - rho(v)| <= norm(Hv - rho v)`. Intervals only, never point
  estimates.
- **E. Non-origin forms.** Repeat C and D at the preserved checkpoint
  positions, since the pilot's chains sat near squared norm 2250 and no
  origin-only statement governs behaviour there.
- **F. Exact homogeneous decomposition.** At each preserved position solve
  `S(t*x) = t^2*S2 + t^3*S3 + t^4*S4` exactly from three evaluations. No
  fitting, no tolerance. This measures directly what the equilibrium virial
  arithmetic can only suggest.
- **G. Transverse scale along a flat ray (indicative).** Log-determinant of the
  second-order form along a ray from arm B. This is a Laplace-approximation
  quantity and must be labelled model-based, never a certified property.
- **H. Measured observable invariance.** For each of `actionDensity`,
  `forceNormSquared`, and `configurationNormSquared`, evaluate the deviation
  along the measured flat basis at the origin and at each preserved position.
  This converts the Phase548 `gaugeInvarianceClassification` from a prose
  declaration into a measurement, and can show that the declared classes were
  wrong.

**Terminals, precedence order, failures above successes.**

1. `invalid-or-drifted-input`
2. `resource-refusal`
3. `structural-precheck-failed`
4. `matvec-or-factorization-unstable`
5. `spectrum-bounds-only-nullity-uncertified`
6. `origin-and-configuration-spectrum-characterized`

**Sampling.** None. No RNG is constructed, no registered seed is touched, and
the two unused frozen seeds stay blind.

**Resource.** A new prospectively frozen ceiling in the Phase541 idiom
(`maximumEstimatedAggregateCpuSeconds`, `maximumEstimatedPeakBytes`,
refuse-before-allocation, deterministic control cost counted). Arithmetic:
3645 forward assemblies at roughly 10 ms is about 40 s; the dense form is
`3645^2 * 8 = 106 MB`; each dense LDL^T is about `3645^3/3 = 1.6e10` flops,
20-60 s, times about eight rungs; one non-origin dense form is
`3645 * 4` gradient evaluations at about 21 ms, about 306 s. Proposed ceiling
**1800 CPU-seconds and 768 MiB**, roughly twice the arithmetic. If the arms do
not fit, SCOPE is cut prospectively in the contract; the ceiling is not raised
at run time. The Phase546 sampler ceiling is untouched and no sampler runs
under this one.

**A clean terminal WOULD establish** exact and certified properties of the
second-order form of the committed operator at named points, an exact integer
lower bound on the flat-sector dimension, an exact decomposition of the value
into homogeneous degrees at six committed configurations, and a measured rather
than declared invariance classification of the three pilot observables.

**It would NOT establish** stationarity, sampling correctness, mixing,
convergence, transfer to a larger extent, any gauge INTERPRETATION of a
measured null direction, a quotient, a measure normalization, a production
default, a Phase458 gate, an O4 discharge, or any physical or unit-carrying
quantity. A measured nullity is a fact about a matrix; calling it gauge volume
is a ruling this phase does not make.

**Independent adjudication.** Required — Phase551. Phase548 shipped two
defects in its own reporting and diagnostic code, and this phase likewise
computes its own numbers and its own verdict.

## Phase551 — independent spectral adjudication

**Question.** Are Phase550's certified numbers reproducible by an
independently implemented, algorithmically distinct route?

**Design.** No project reference to Phase550 and no shared code. A different
algorithm family per quantity: randomized subspace iteration rather than
Lanczos, a different factorization ordering for the inertia counts, an
independent exact-integer incidence rank, and a null basis rebuilt from the
mesh rather than read from Phase550's output. A known-answer battery on
synthetic matrices with planted nullity and planted gaps runs BEFORE the
estimators touch any Phase550 datum, and must include a case that the specific
failure modes of arms C and D would fail.

**Terminals.** `invalid-or-drifted-input`, `resource-refusal`,
`estimator-battery-failed`, `adjudication-contradicts-reported-values`,
`adjudication-inconclusive-tolerance-exceeded`,
`adjudication-confirms-reported-values`.

**Sampling.** None. **Resource.** Its own ceiling, same idiom, about 1800 s and
768 MiB.

**Scope.** Confirming a measurement grants no authority. It does not upgrade a
fact about a matrix into a fact about the theory. No adjudicator of the
adjudicator: the recursion stops here, as it did at Phase549.

## Phase552 — committed-chain stationarity re-analysis

**Question.** Were the six pilot chains stationary but under-resolved, or not
stationary at all? Does the squared configuration norm drift across the
retained window, and how do the three observable series behave once decomposed
into the measured flat sector and its complement?

**Decidable now** because Phase549 already showed that replaying the frozen
seeds reproduces the chains bit for bit, and because the per-draw series is a
deterministic function of already-committed inputs. Recomputing a different
function of an already-determined dataset consumes no blind currency.

**Honesty constraint, stated in the contract.** This is re-analysis of data
whose summary is already known, so every statistic and threshold is frozen
BEFORE the replay runs, the artifact carries an explicit
`analysisIsRetrospectiveOnKnownData: true` field in the spirit of Phase548's
`pristineSeedBlindPreregistration: false`, and the result is diagnostic only.
It cannot change Phase548's terminal and must never be described as a
convergence assessment of the pilot.

**Content.** Preregistered drift test on the squared norm over the retained
window (half-window difference and regression slope, both with frozen
thresholds); the same on the projection onto the Phase550 flat basis and onto
its complement, separately; per-draw series for all three observables; split
R-hat recomputed on the decomposed series.

**Terminals.** `invalid-or-drifted-input`, `replay-not-bit-identical` (a hard
failure, since it would contradict Phase549), `resource-refusal`,
`drift-test-inconclusive`, `non-stationary-drift-detected`,
`stationary-under-resolved-consistent`. The last two are both clean scientific
outcomes and the taxonomy prefers neither.

**Sampling.** No new sampling: it re-executes six already-committed,
already-adjudicated chains under the identical frozen configuration and seeds.
About 22069 force evaluations, roughly 470 CPU-seconds, satisfiable under the
existing Phase546 tick rule with no ceiling change.

**Scope.** It would establish whether six committed chains' series drift. It
would not establish stationarity of the target, rehabilitate any failed gate,
select a default, or touch Phase481, Phase458, or O4.

**Independent adjudication.** Not separately, since the replay is already
corroborated by Phase549 and the added statistics can carry two estimator
implementations internally. If `non-stationary-drift-detected` fires, an
adjudicator becomes mandatory, because that finding would be load-bearing for
stopping the lane.

## Phase553 — constant-budget trajectory-length reallocation scan (conditional)

Register only if Phase550 finds either an exactly flat sector or a genuinely
stiff nondegenerate spectrum, AND Phase552 returns
`stationary-under-resolved-consistent`.

**Question.** At a FIXED aggregate tick budget and with no ceiling change, does
reallocating from many short trajectories to fewer longer ones improve
resolution of the soft sector, and where does acceptance break?

**Why this is not ceiling inflation.** The frozen rule constrains
`trajectories * leapfrogSteps <= 3551` per chain at three chains; a grid over
step size and leapfrog count with `trajectories = floor(3551/L)` sits inside
the existing ceiling at every row. The lever is that for a near-flat coordinate
the accumulated variance at fixed budget is `budget * stepSize^2 * L`, linear
in trajectory length, while the frozen `L = 8` was selected on effective sample
size per unit work of `actionDensity` — an observable in the sector that was
not failing. Acceptance is the empirical bound and is a fail-closed gate.

**Seeds.** Disclosed non-blind tuning seeds from a NEW table disjoint from the
Phase546 families, recorded with `pristineSeedBlindPreregistration: false`
exactly as Phase548 did. The two remaining blind seeds are not touched.

**Selection rule frozen before execution.** Carry forward the row with maximum
measured effective sample size per tick of the observable class Phase550
MEASURED to be invariant, ties broken by lowest grid index. No post-hoc row
choice, and the scan reports every row including the rejected ones.

**Terminals.** `invalid-or-drifted-input`, `resource-refusal`,
`precheck-failed`, `scan-nonfinite-or-divergent`,
`no-row-improves-soft-sector-resolution`, `scan-executed-row-selected`. The
clean terminal is explicitly not a convergence claim and not a production
default.

## Phase554 — blind confirmation (conditional, not proposed for registration now)

The only phase permitted to consume the remaining blind seeds, using the row
frozen by Phase553 plus a newly registered blind table, with its own
independent adjudicator. It is deliberately left unregistered: it should never
be registered before the flat sector is measured and the observable classes are
known, because those two seeds are the last prospectively blind currency in the
pack.

## Phase555 — external-review escalation packet (zero compute, may run in parallel)

Assembles the two reserved questions in a form a reviewer can answer, equipped
with the measured inputs from Phases550-552: the measured flat-sector dimension
and its topological versus operator decomposition, the exact homogeneous
decomposition at real configurations, the transverse scale along a flat ray,
and the measured observable invariance. It authors no ruling, consumes no memo,
verifies no signature, and changes no pending flag; Phase480 semantics are
unchanged.

**Terminals.** `packet-incomplete-inputs-missing`,
`packet-assembled-awaiting-external-ruling`.

## Standing firewalls for the whole block

- **No quotient and no gauge fixing.** Deciding that a measured null direction
  is gauge volume, and normalizing by the corresponding transverse factor, is a
  measure convention, and the register carries exactly those two items as
  pending review. Phase490 already recorded `quotient-underdetermined` when this
  program attempted the analogous classification. Every contract in this block
  carries `nullSpaceInterpretedAsGaugeVolume: false` and `quotientApplied:
  false`. Measuring is permitted; interpreting is not.
- **No ceiling inflation.** Phases550/551 declare a NEW deterministic ceiling
  for a deterministic zero-sampling computation, in the Phase541 idiom; the
  Phase546 sampler ceiling is untouched and no sampler may run under the new
  one. Phases552/553 change no ceiling at all. Raising a sampler ceiling to buy
  a passing convergence gate on the same configuration is forbidden.
- **No re-adjudication.** Phase548 and Phase549 are immutable. Nothing here
  reruns, repairs, rereads, or supersedes them, and the preserved negative
  stands whatever the block finds.
- **No gate progress.** G3, G4, and G5 are keyed to the Phase455/456 and
  register artifacts. Nothing in this lane can supply them at any level of
  success. This lane establishes only whether the registered operator's
  complete-lattice target is samplable at all, which is a prerequisite for any
  future pack and is not a gate input.
- **No claim.** Every quantity here is a property of a discrete operator on a
  3645-dimensional lattice-unit configuration space.

## Deliberately excluded routes, and why

- **Quotient or gauge fixing as a repair.** Reserved, as above.
- **Preconditioned or state-dependent-metric sampling.** Motivated only if the
  softness is a stiff nondegenerate spectrum, which rests on the one-sided
  smallest-eigenvalue number; it cannot fix an exact zero; a fixed metric needs
  a factor of about 106 MB against a `5360704`-byte per-chain ceiling, so it
  requires a new pack, which is this plan's largest ceiling-integrity risk; and
  a state-dependent metric changes the target silently if the generalized
  integrator is wrong. Revisit only after Phase550, and then fixed-metric only,
  with exact-Gaussian controls and moment agreement against unpreconditioned
  chains.
- **Restricting the frozen observable set on its own.** Dropping the failing
  row is textbook post-selection. Phase550 arm H makes invariance measured;
  only then, and only prospectively for new runs, may a gating class change.
- **Raising a ceiling to reach the A29 mixing-length figure.** That figure is
  derived from an upper bound (fact 1), and inflating a ceiling to reach it is
  the exact failure mode this program forbids.
- **Smaller extent or a different `beta`.** Unavailable as stated (facts 2 and
  3). What remains is a different target, and Phases541/542 recorded why that
  transfer fails.

## Wiring checklist for each registered phase in this block

Consult the restart prompt for the authoritative list rather than this summary,
then run a `--incremental` pass before committing.

- run line in `scripts/generate_validated_boson_predictions.sh`
- item in `scripts/BosonPhasesTraversal.proj`
- exclusion registration in the whole-repo scanners, including phase253's
  four-dimension exclusions, where and only where the new text actually trips
  a scanner
- prediction-package block in phase101
- checklist item in phase202
- asserts in `scripts/verify_boson_claim_integrity.sh`
- `docs/Phases/Implementation/IMPLEMENTATION_P5NN.md`

Every branch above is target-blind, workbench-relative, lattice-unit,
source-contract-ineligible, and retains `promotedPhysicalMassClaimCount=0`.

# Implementation P548: Bounded Complete-Lattice Pilot Execution

Phase548 is Amendment A29's separately registered, prospectively frozen
execution phase. Phase547 authorized registering it and explicitly did not
authorize execution; Phase548 carries its own frozen contract, and that
contract is the only execution authority it claims.

Its contract was committed before the first registered seed was drawn. It
exact-binds thirteen artifacts across the Phase533/543/545/546/547 chain plus
the registered operator source, and it re-derives the Phase546 telemetry
schema, observable schema, convergence thresholds, canonical checkpoint format
and checked resource rule from the frozen upstream bytes rather than restating
them, refusing to run on any mismatch. The Phase545 kernel is reused by project
reference, so the executed proposal code is the hash-bound kernel itself.

## Why the inherited row was not used

The Phase533 pilot row could not be used and the contract records why. Its 64
retained draws per chain cannot reach the Phase546 bulk effective-sample-size
floor of 100, and its 0.0125 step size sits about a hundredfold below the
measured leapfrog stability bound. The executed default was selected by a
scratch calibration that ran outside the repository on non-registered seeds,
wrote no repository byte, and is disclosed in the contract with
`pristineSeedBlindPreregistration` set false. The registered seeds were touched
only by the frozen execution. This is tune-then-confirm, not seed-blind
preregistration, and it is labelled as such.

## The ceiling binds at three chains

The frozen Phase546 rule refuses four chains at extent three, and at that width
both of its limits bind: peak bytes land exactly on the ceiling at `5360704`,
and the rule refuses on equality. At this phase's trajectory count the
aggregate CPU-tick limit is also crossed, and since CPU is checked first the
recorded refusal reason is `cpu-boundary-or-limit-exceeded`. The pilot
therefore ran three chains per seed table, taking the first three seeds of each
frozen table in frozen order, with the fourth excluded by a contract rule fixed
before execution. The run recomputes the four-chain assessment and records both
the reason and the peak-byte equality as a witness. No ceiling, formula, or
threshold was modified.

## Deterministic prechecks

All zero-sampling prechecks pass. Action and gradient vanish exactly at the
origin. The directional finite-difference gradient error is `1.36e-9` against a
`3e-7` tolerance, and forward/reverse integration closes to `1.55e-16` against
a `2e-9` tolerance.

The frozen spectral probe returns a largest Hessian eigenvalue of `1.9598`,
giving a leapfrog stability bound of `1.4287`; the frozen `0.06` step is about
twenty-four times below it. The smallest eigenvalue estimate is `1.23e-6`, a
condition number near `1.6e6`. These are iterative estimates, not an
eigendecomposition, and the record says so.

That spectrum is the quantitative reason this pilot is bounded rather than
conclusive. The slowest-mode trajectory length implied by the smallest
eigenvalue is about `2836`, while the whole frozen budget buys `192` units of
trajectory length per chain - roughly seven percent of a single slow-mode
correlation time.

## Live restart equivalence

Phase547 recorded that codec round-trip is not live restart equivalence.
Phase548 closes that specific gap on a reduced prefix: an eight-trajectory run
of a registered chain is reproduced bit for bit by a four-trajectory run,
canonical checksummed checkpoint, decode, and four-trajectory resume, in both
position and RNG state. This is a reduced-prefix result and is not a
full-length chain equivalence claim.

## Execution and terminal

Six chains ran to completion with zero non-finite and zero divergent
trajectories against a frozen zero tolerance. Acceptance ranges `0.9125` to
`0.95` and the largest absolute energy error is `0.885`. All six chains left
their overdispersed starts and agree closely on both a gauge-invariant and a
gauge-variant location, with mean action density between `0.3534` and `0.3570`
and mean squared configuration norm between `2239` and `2271`.

The frozen convergence gates nevertheless do not all pass, so the terminal is
`pilot-executed-diagnostics-invalid`:

| table | observable | class | split R-hat | bulk ESS | tail ESS |
|---|---|---|---|---|---|
| a | actionDensity | invariant | 1.0074 | 212.3 | 622.3 |
| a | forceNormSquared | invariant | 1.0026 | 393.5 | 602.4 |
| a | configurationNormSquared | variant | **1.0387** | **72.2** | 101.8 |
| b | actionDensity | invariant | **1.0124** | 287.0 | 656.2 |
| b | forceNormSquared | invariant | **1.0141** | 420.5 | 710.6 |
| b | configurationNormSquared | variant | **1.0151** | **99.7** | 182.9 |

Both gauge-invariant observables pass every gate in table a. In table b all
three exceed the `1.01` R-hat threshold, marginally. The gauge-variant
observable is the worst row in both tables and is the only one to miss the bulk
effective-sample-size floor.

The contract declared the gauge classification of each observable **before**
execution, precisely so that a split between invariant and variant sectors
would be read against a prospective expectation. That expectation is only
partly met: the split holds in table a and fails in table b, so
`gaugeSectorSplitObserved` is recorded false. The honest reading is that the
bounded budget leaves marginal cross-chain disagreement in every sector, worst
in the gauge-variant one - not that a clean sector separation was demonstrated.

## Two preserved repairs

Both are recorded under `output/incident/` and neither is citable evidence.

The first execution attempt completed all six chains and then aborted while
serializing a non-computable diagnostic, which strict JSON cannot encode. The
repair confines itself to output encoding: a diagnostic that cannot be computed
is now reported as null, which never means "passed".

The second attempt reported no tail effective sample size for any
gauge-invariant observable. That was a defect in this phase's own estimator,
not a property of the chains: the Geyer pair sum omitted the leading `rho_0 = 1`
term, biasing tau low by exactly two, so a well-mixed sequence returned a
negative tau and was reported as not computable. The corrected estimator
restores the standard definition.

Neither repair touched the frozen contract, configuration, seeds, thresholds,
or gates, and the chains are unchanged across all attempts because the run is
deterministic in its frozen seeds. The correction moved the tail rows from
"not computable" to large finite values and left every split R-hat unchanged.
The terminal was `pilot-executed-diagnostics-invalid` before and after.

## What this does not establish

The result is workbench-relative and in lattice units. It does not establish
stationarity of the registered target, sampling correctness, transfer to any
larger extent, or any spectral or physical quantity, and it is not a production
benchmark. It creates no Phase481 pack, selects no production default,
satisfies no Phase458 gate, discharges no O4 review, fills no source contract,
and authorizes no production or launch. Phase535 remains closed and unchanged,
external review remains pending, and `promotedPhysicalMassClaimCount=0`.

Because Phase548 computes its own chains and its own verdict, its assessment
surface is not independent of its execution - a point the two repairs above
make concrete. An independently registered adjudicator over the preserved
telemetry and checkpoints is the appropriate successor.

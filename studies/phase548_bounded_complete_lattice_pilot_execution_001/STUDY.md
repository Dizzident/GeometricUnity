# Phase548 - Bounded Complete-Lattice Pilot Execution

Amendment A29. This is the separately registered, prospectively frozen
**execution** phase that Phase547 authorized registering (and only
registering). Phase547 explicitly did not authorize execution; this phase
carries its own frozen contract, and its own contract is the authority for
what it runs.

## What this phase does

It executes one bounded Hamiltonian Monte Carlo pilot on the complete
extent-three periodic four-dimensional lattice, under the registered
theta-identically-zero SD2/Id0 action member, using:

- the hash-bound Phase545 injectable deterministic proposal kernel, reused by
  project reference rather than copied;
- the Phase546 frozen telemetry schema, observable schema, convergence
  thresholds, canonical checksummed checkpoint format, prospective seed
  tables, and checked resource-refusal rule;
- the Phase547 readiness terminal as its registration premise.

Everything the run may do is fixed in
`preregistration/phase548_bounded_complete_lattice_pilot_execution_contract_v1.json`
before the first trajectory. The program refuses to run on any binding,
premise, schema, or resource mismatch.

## Why the default row was not inherited from Phase533

Phase533's refused pilot row was `stepSize 0.0125 x 6 leapfrog steps`, with 24
warmup and 64 retained trajectories per chain. Two frozen facts make that row
unusable here, and both were established before any registered seed was drawn:

1. **It cannot satisfy the Phase546 diagnostics.** 64 retained draws per chain
   cannot produce a bulk effective sample size of 100, which the Phase546
   convergence block requires. The row is arithmetically incapable of passing
   the gate it would be judged against.
2. **It is far below the integrator stability scale.** A deterministic
   zero-sampling spectral probe of the action Hessian at the origin gives a
   largest eigenvalue near `1.96`, so the leapfrog stability bound is near
   `1.43` - about a hundred times the Phase533 step size.

The default row was therefore selected by an explicit calibration described
below, and frozen before execution.

## Default-selection provenance (disclosed, not hidden)

The default `stepSize`, `leapfrogSteps`, and per-chain trajectory counts were
chosen using a scratch calibration probe that ran **outside the repository**,
wrote no repository byte, and used **non-registered seeds** (`900001`,
`900101`) that are disjoint from every A22-A24 seed family and from both
Phase546 prospective tables. The contract records this provenance, the
non-registered seeds, and the measured quantities that drove the choice.

This is the Phase539 pattern: tune on throwaway seeds, then confirm
prospectively on fresh registered seeds. It is **not** pristine
seed-blind preregistration, and the contract says so in its own fields. The
registered Phase546 seeds are touched only by the frozen execution.

## The binding constraint: three chains, not four

The Phase546 resource rule refuses on equality, and at four chains **both** its
limits are reached. Peak bytes land exactly on the ceiling:

```
perChainBytes = 1048576 + 3645 * 80 = 1340176
4 * 1340176   = 5360704 = maximumPeakBytes   -> at the limit
3 * 1340176   = 4020528                      -> allowed
```

At this phase's trajectory count the aggregate CPU-tick limit is crossed too
(`4 * 400 * 8 * 3645 * 12 = 559872000` against a ceiling of `466093440`), and
because the rule checks CPU before memory it reports
`cpu-boundary-or-limit-exceeded` as the refusal reason. The run records both the
reason and the peak-byte figure so the equality is visible.

The pilot therefore runs **three chains per seed table**, taking the first
three seeds of each frozen Phase546 table in frozen order, and runs the two
tables as two separately assessed resource requests. The fourth seed of each
table is excluded by the frozen ceiling, not by any run-time observation. No
ceiling, formula, or threshold was modified to make room.

The same rule caps aggregate work at `trajectories x leapfrogSteps <= 3551`
per chain, which is the dominant limit on what this pilot can resolve.

## What a pass or a failure here would and would not mean

This is a workbench-relative experiment in lattice units. Whatever the
diagnostics say:

- no observable here is a physical W, Z, Higgs, or photon property;
- no unit calibration to GeV exists, and none is created;
- nothing here fills a source-lineage contract, satisfies Phase458 G3/G4/G5,
  discharges the outstanding O4 physicist review, creates or modifies a
  Phase481 pack, selects a production default, or authorizes production;
- `promotedPhysicalMassClaimCount` stays `0`.

Phase535 remains closed and is neither reopened nor reinterpreted.

A clean diagnostics outcome would establish only that this bounded pilot
configuration ran within its own frozen gates on this lattice. It would not
establish stationarity of the registered target, correctness of the sampled
distribution, transfer to any larger extent, or any spectral or physical
quantity.

## Layout

- `preregistration/` - the frozen contract, written before the first run.
- `output/bounded_complete_lattice_pilot_execution.json` and `_summary.json` -
  the deterministic adjudicated result. These contain no timing and are
  byte-reproducible across runs.
- `output/telemetry/` - the full per-trajectory telemetry required by the
  Phase546 schema, including the mandated `proposalElapsedCpuTicks` field.
  These files carry wall-clock measurements and are therefore **not**
  byte-reproducible; the deterministic projection of each is hashed into the
  main output so the scientific content stays pinned while the timing does not.
- `output/checkpoints/` - the canonical checksummed end-of-chain checkpoints.
- `output/incident/` - the two preserved, non-citable repair records described
  below.

Failed and negative outcomes in these directories are first-class artifacts and
are preserved, never overwritten with a favorable rerun.

## Result

The terminal is `pilot-executed-diagnostics-invalid`. Six chains ran to
completion with zero non-finite and zero divergent trajectories, acceptance
between `0.9125` and `0.95`, and largest absolute energy error `0.885`. Both
gauge-invariant observables pass every frozen gate in table a; in table b all
three observables exceed the `1.01` split R-hat threshold marginally, and the
gauge-variant observable misses the bulk effective-sample-size floor in both
tables. The prospectively declared gauge-sector split is therefore only partly
observed, and `gaugeSectorSplitObserved` is recorded false.

The deterministic spectral probe explains the bound: the condition number is
near `1.6e6`, implying a slowest-mode trajectory length near `2836`, while the
entire frozen budget buys `192` units of trajectory length per chain.

## Two preserved repairs

Both live under `output/incident/` and neither is citable evidence.

1. **Serialization abort.** The first attempt completed all six chains and then
   aborted while writing a non-computable diagnostic, which strict JSON cannot
   encode. The fix is confined to output encoding: a diagnostic that cannot be
   computed is reported as null, which never means "passed".
2. **Defective tail effective-sample-size estimator.** The second attempt
   reported no tail effective sample size for any gauge-invariant observable.
   That was this phase's own estimator, not a property of the chains: the Geyer
   pair sum omitted the leading `rho_0 = 1` term, biasing tau low by exactly
   two, so a well-mixed sequence returned a negative tau. The corrected
   estimator restores the standard definition.

Neither repair touched the frozen contract, configuration, seeds, thresholds,
or gates. The chains are identical across all attempts because the run is
deterministic in its frozen seeds, every split R-hat is unchanged, and the
terminal was `pilot-executed-diagnostics-invalid` before and after.

Because this phase computes its own chains and its own verdict, its assessment
surface is not independent of its execution - a point these two repairs make
concrete. An independently registered adjudicator over the preserved telemetry
and checkpoints is the appropriate successor.

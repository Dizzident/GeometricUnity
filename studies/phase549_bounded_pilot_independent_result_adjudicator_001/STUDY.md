# Phase549 - Bounded Pilot Independent Result Adjudicator

Amendment A29. Phase548 computed its own chains and its own verdict, so its
assessment surface was not independent of its execution - and two defects in
its own reporting and diagnostic code had to be caught by hand. This phase is
the isolated assessment surface that situation calls for.

## What independence means here

Phase549 does not reference the Phase545 kernel and reuses no Phase548 code.
The sampler, the convergence estimators, and the checkpoint reader are
re-implemented here from the frozen contracts alone. A defect in Phase548
therefore cannot hide inside its own verdict.

The contract exact-binds sixteen artifacts: the Phase548 contract, program and
result, all six preserved telemetry files, all six checkpoints, and the
registered operator source.

## The four independent checks

1. **Estimator known-answer battery, run before the estimators touch any pilot
   draw.** This is aimed squarely at the Phase548 failure mode, where a
   defective tail estimator reported "not computable" for well-mixed
   sequences. Independent draws give R-hat `1.0001` with bulk and tail
   effective-sample-size fractions `0.990` and `0.981`; an AR(1) sequence with
   `rho = 0.9` gives a bulk fraction of `0.051` against the analytic
   `(1-rho)/(1+rho) = 0.0526`; chains with separated means give R-hat `1.535`.
2. **Telemetry integrity.** Every required field is present in all six files,
   row counts and indices are as frozen, `deltaH` equals the Hamiltonian
   difference, and both the acceptance rule and the divergence label are
   re-derived from the recorded thresholds rather than trusted.
3. **Independent replay.** Phase548 did not retain the per-draw observable
   series, so its convergence numbers cannot be recomputed from preserved
   bytes alone. This phase therefore replays all six chains from the frozen
   seeds with its own sampler and re-derives the observables itself. The worst
   relative `deltaH` deviation against the recorded telemetry is `0` - the
   replay is bit-identical - and every acceptance decision matches.
4. **Checkpoint audit.** Payload checksums are recomputed, headers are checked
   against the frozen configuration, and each stored final position is required
   to be bit-identical to the independently replayed one.

## Result

The terminal is `adjudication-confirms-reported-terminal`. Every reported split
R-hat is reproduced to the sixth decimal, every per-observable gate outcome
matches, and the independently derived terminal is
`pilot-executed-diagnostics-invalid`, the same one Phase548 reported.

Confirming a negative result grants no authority. This phase does not
reinterpret Phase548, and it establishes no stationarity, sampling
correctness, transfer to a larger extent, or any spectral or physical
quantity. It is workbench-relative and in lattice units. Phase535 remains
closed, Phase458 G3/G4/G5 and O4 remain unsatisfied, external review remains
pending, and `promotedPhysicalMassClaimCount` remains `0`.

One defect was found and fixed in this phase's own first attempt: a placeholder
comparison row emitted non-finite numbers, which strict JSON cannot encode. The
fix is confined to output encoding and touched no check, threshold, or gate.

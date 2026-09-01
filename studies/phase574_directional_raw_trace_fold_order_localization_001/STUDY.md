# Phase 574: directional raw-trace fold-order localization

Phase574 is the A38-designed, prospectively frozen raw-trace experiment that
Phase573 opened. Both committed Phase548 chains replay bit-for-bit from frozen
contracts and seeds, so the raw directional series the upstream phases did not
retain are deterministic and recoverable at zero new sampling cost. Phase574
replays the chains once, applies both committed observable paths - the
Phase570 v7 scalar edge-space basis with its 40-sweep descending unclamped
Jacobi, and the Phase572 full-dof sign-canonicalized directions with their
32-sweep ascending zero-clamped Jacobi - to the shared position stream, and
compares everything through the single estimator kernel Phase573 proved
bit-for-bit identical across implementations.

The pre-registered H-fold hypothesis: ulp-scale raw differences between the
two observable paths flip near-tied folded-rank pairs without reordering the
raw series, so only R-hat's folded component can move while bulk ESS, tail
ESS, and classifications stay bit-identical.

## Executed result

The v1 terminal is `fold-order-flip-confirmed-source-attributed`. Every
H-fold clause held: 30 of 36 rows carry raw ulp-scale differences (maximum
scaled difference 2.96e-15), ordinary rank vectors agree on all 36 rows, all
five committed mismatch rows show fold-order flips of exactly 2-3 folded-rank
entries, and each side's committed R-hat, bulk ESS, tail ESS, and pass flags
reproduce bit-for-bit from its own recovered series. Attribution is measured,
not guessed: three mismatch rows diverge at projection accumulation and two
at the assembled Gram matrix; thirteen additional fold flips in non-mismatch
rows were masked by R-hat's max rule, exactly as the hypothesis anticipated.
The recovered raw series for both sides are retained in the output.

This closes the Phase573 diagnostic question only. Phase572 remains
fail-closed at its unchanged `2e-10` tolerance, the Phase571 lever remains
not independently confirmed, prospective sampler-pack planning remains
closed, and any re-adjudication under an agreed rank/fold convention requires
its own separately registered prospective phase. No new sampling, Markov
advance, configuration retention, protected-seed access, target change,
quotient, gauge fixing, measure normalization, production action,
physical-unit claim, or GeV claim occurred.

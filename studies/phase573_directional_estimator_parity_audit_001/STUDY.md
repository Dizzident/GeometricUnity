# Phase 573: directional estimator parity audit

Phase573 is the zero-new-sampling first step after Phase572's fail-closed
directional-diagnostics disagreement. It exact-binds the executed Phase570 v7
and Phase572 v9 programs and outputs, implements the diagnostic kernel twice,
and tests both implementations on identical synthetic inputs before reading
the audited numerical rows.

The scientific comparison asks only whether the reported disagreement is
confined to rank-normalized R-hat while bulk ESS, tail ESS, and pass/fail
classification remain identical. The upstream artifacts do not retain the
raw directional time series, so this phase cannot distinguish ordinary-rank,
folded-rank, median, projector, or eigensolver rounding effects. A localized
result opens design of a separate raw-trace phase only; it does not authorize
that phase, relax Phase572's tolerance, or open prospective sampler planning.

No replay, RNG for scientific data, HMC, new sampling, Markov advance,
configuration retention, protected seed access, target change, quotient,
gauge fixing, measure normalization, production action, physical-unit claim,
or GeV claim is permitted.

## Executed result

The v2 terminal is
`rhat-only-disagreement-localized-input-trace-required`. Two local kernels
matched bit-for-bit on identical inputs. Across the 36 frozen upstream rows,
exactly five R-hat values differed while bulk ESS, tail ESS, and pass/fail
classification remained identical. Phase574 trace design is open; Phase574
execution and every sampling or promotion authority remain false.

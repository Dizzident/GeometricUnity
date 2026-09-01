# Phase 573 implementation: directional estimator parity audit

## Status

Phase573 v2 executed under Amendment A37 and returned
`rhat-only-disagreement-localized-input-trace-required`. It is a deterministic,
retrospective audit of already committed Phase570 and Phase572 outputs.

The frozen v1 attempt stopped at `known-answer-battery-failed` before reading
upstream numeric rows. Its two estimator implementations already matched
bit-for-bit, but an over-strong composite sign/affine-invariance requirement
failed for finite-sample tail-indicator ESS. V2 preserves that output and
source lineage, keeps exact kernel parity mandatory, makes the composite
invariance row descriptive, and changes no Phase572 comparison threshold.

## Method

The phase exact-binds both upstream diagnostic implementations, their frozen
contracts, and byte-identical full/summary outputs. Two study-local estimator
implementations are exercised on IID, autocorrelated, anti-correlated,
separated, drifting, and tied synthetic chains. Sign and affine invariance are
also required before upstream numeric rows are parsed.

It then compares the exact 36 table rows by table and series. The frozen
hypothesis is that exactly five rows disagree, only in R-hat, while every bulk
ESS, tail ESS, and diagnostic pass classification remains identical. Because
the upstream outputs do not retain raw directional series, a matching result
localizes rather than resolves the remaining cause and opens only Phase574
raw-trace design.

That hypothesis was confirmed exactly. The mismatch keys are table A
`closedGramLargest`, `closedMovementSquared`, `eMovementSquared`, and
`eNormSquared`, plus table B `closedGramSmallest`. All 36 bulk ESS and tail ESS
values are bit-identical across Phase570/572, all pass classifications agree,
and the maximum scaled R-hat difference reproduces
`4.302812022446892e-05`. The remaining distinction is therefore in upstream
raw-series/rank construction; the absent raw series are required to separate
ordinary rank, folded rank, median, projector, and eigensolver rounding.

Program v2 SHA256:
`efed9f7f1c606bad5972ea40bb10e49f6636ee55646d53bdaa2c96f39284b753`.
Project SHA256:
`af48a5eb8994926788f62ba31cfc356f0183a40314ba4633cfb9f79c6a0167f4`.
Contract v2 SHA256:
`245e9ae11f3281228e4ac8a30b714a77efaa1d0be0e92b33f7b8c7365115901c`.
Both v2 outputs have SHA256
`e0851f8ee4a17637d49dbd62c706b4f9a776ad46328f169d7265c93347d16f47`.

## Firewalls

Phase573 does not reinterpret Phase570 or Phase572, relax a threshold, open
prospective-pack planning, replay a chain, use a scientific RNG, sample,
advance Markov state, retain configurations, read protected Phase554 seeds,
change the target, apply a quotient, gauge fix, normalize a measure, authorize
production or launch, or support a physical-unit or GeV claim. External review
remains pending and `promotedPhysicalMassClaimCount=0`.

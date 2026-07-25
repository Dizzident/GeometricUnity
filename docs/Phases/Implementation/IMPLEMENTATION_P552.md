# Implementation P552: Committed-Chain Stationarity Re-Analysis

Amendment A30. A re-analysis of the six already-determined Phase548 chains. No
new sample is drawn: the phase re-executes six committed chains under the
identical frozen configuration and seeds and computes a different function of an
already-determined dataset.

## The honesty constraint, stated first

The summary of this dataset was already known when these statistics were chosen.
Every statistic and threshold is frozen in the contract before the replay runs,
and the artifact carries `analysisIsRetrospectiveOnKnownData: true` in the spirit
of Phase548's `pristineSeedBlindPreregistration: false`. The result is
diagnostic only. It cannot change Phase548's terminal and is not a convergence
assessment of the pilot.

## Replay

The sampler is re-implemented from the frozen Phase548 contract; the Phase545
kernel is not referenced and no Phase548 or Phase549 code is reused. All six
chains replay bit-identically: every acceptance decision matches the recorded
telemetry, the worst relative `deltaH` deviation is within the frozen tolerance,
and every stored final position is bit-identical to the replayed one. Phase549
had already established this, so a failure here would have been a hard
contradiction with its own terminal.

## The decomposition

The 252-dimensional flat basis is rebuilt from the mesh by the same deterministic
construction rule rather than read from the Phase550 output, and its dimension
is checked against the reported one. Each retained position is split into its
flat-sector projection and the remainder, giving two new series alongside the
three pilot observables.

## Frozen drift statistics

Twenty batches of seventeen draws per chain. Two standardized statistics per
chain and series: the half-window difference of batch means, standardized by the
batch-mean standard errors of the two halves, and the ordinary least squares
slope of the batch means against the batch index, standardized by its residual
standard error. Drift is declared when at least three of the six chains exceed
`|z| = 3` on either statistic for a decision series.

No series drifts. The worst standardized statistic anywhere is `2.62`, on the
complement series, and the worst half-window statistic is `1.93`. The terminal is
`stationary-under-resolved-consistent`, which is one of two clean scientific
outcomes the taxonomy does not prefer between.

## What the decomposed diagnostics show

Recomputed split rank-normalized R-hat and effective sample sizes localize the
resolution deficit rather than removing it. In both seed tables `actionDensity`
and `forceNormSquared` mix well, with bulk effective sample sizes from `212` to
`420`. The squared configuration norm and its flat-sector projection are the
worst, at bulk `72` and `81` in table a and `100` and `70` in table b, with
split R-hat up to `1.047`. The complement component sits between them.

This is diagnosis, not rehabilitation. It rehabilitates no Phase548 gate,
selects no configuration, and establishes no stationarity of the target.

## Scope

No new sampling, no registered blind seed touched, no ceiling changed, no
Phase481 pack, no production default, no Phase458 or O4 movement, external review
still pending, and `promotedPhysicalMassClaimCount = 0`. Because the terminal is
not `non-stationary-drift-detected`, the plan's mandatory independent adjudicator
for that finding is not triggered.

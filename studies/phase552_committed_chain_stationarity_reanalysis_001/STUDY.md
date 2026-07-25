# Phase552 - Committed-Chain Stationarity Re-Analysis

Amendment A30. Were the six Phase548 chains stationary but under-resolved, or
not stationary at all? Phase549 already showed that replaying the frozen seeds
reproduces the chains bit for bit, so the per-draw series is a deterministic
function of already-committed inputs and recomputing a different function of it
consumes no blind currency.

## The disclosure that comes first

The summary of this dataset was already known when these statistics were chosen.
Every statistic and threshold is frozen in the contract before the replay runs,
and the artifact records `analysisIsRetrospectiveOnKnownData: true` in the spirit
of Phase548's `pristineSeedBlindPreregistration: false`. This is diagnostic only.
It does not change Phase548's terminal, and it is not a convergence assessment of
the pilot.

## What was run

The sampler is re-implemented from the frozen Phase548 contract; the Phase545
kernel is not referenced and no Phase548 or Phase549 code is reused. All six
chains replay bit-identically - every acceptance decision matches the recorded
telemetry and every stored final position is bit-identical to the replayed one.

The 252-dimensional flat basis is rebuilt from the mesh by the same deterministic
construction rule rather than read from the Phase550 output; its dimension is
checked against the reported one and agrees. Each retained position is split into
its flat-sector projection and the remainder.

## The frozen drift test

Twenty batches of seventeen draws per chain, two standardized statistics per
chain and series - the half-window difference of batch means and the ordinary
least squares slope of the batch means against the batch index - with drift
declared when at least three of six chains exceed `|z| = 3` on a decision series.

Nothing drifts. The worst standardized statistic anywhere is `2.62` and the worst
half-window statistic is `1.93`. The terminal is
`stationary-under-resolved-consistent`, one of two clean scientific outcomes the
taxonomy does not prefer between.

## Where the resolution deficit actually sits

The recomputed diagnostics localize it rather than remove it. In both tables
`actionDensity` and `forceNormSquared` mix well, at bulk effective sample sizes
`212` to `420`. The squared configuration norm and its flat-sector projection are
the worst - bulk `72` and `81` in table a, `100` and `70` in table b, with split
R-hat up to `1.047`. The complement sits between them.

Read together with Phase550, the picture is consistent: the origin flat sector is
lifted at these configurations, but the slowest-mixing coordinate the pilot
measured is still the one that projects onto it.

## What this does not establish

Stationarity of the target, rehabilitation of any failed Phase548 gate, a
production default, or anything touching Phase481, Phase458 or O4. No new
sampling occurred, no registered blind seed was touched, no ceiling changed,
external review remains pending, and `promotedPhysicalMassClaimCount = 0`.
Because the terminal is not `non-stationary-drift-detected`, the plan's mandatory
adjudicator for that finding is not triggered.

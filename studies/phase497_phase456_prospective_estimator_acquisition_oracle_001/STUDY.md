# Phase497: Phase456 prospective estimator/acquisition oracle

## Scope

This phase performs a deterministic, target-blind synthetic planning battery.
It freezes candidate estimators, periodic-correlator oracles, correlated-noise
laws and seeds, an acquisition grid, recovery and model-selection thresholds,
and invalid-row behavior before evaluating any grid point.

The admitted menu contains positive one- and two-component periodic
correlators. A sign-indefinite control must be rejected. Full-rank correlated
covariance, invalid-row propagation, recovery, multi-component discrimination,
and false-selection controls are all required. A low residual by itself cannot
make a specification viable.

## Verdict taxonomy

- `invalid-oracle-battery`
- `acquisition-specification-viable`
- `no-viable-specification-within-audited-grid`

If several grid points survive, selection uses the prospectively frozen
`temporalExtent * effectiveSampleSize` cost and deterministic tie breakers.

## Boundaries

This phase does not read or reinterpret Phase456 or Phase496, run HMC, launch a
benchmark, construct or authorize a Phase481 pack, authorize sampling, satisfy
Phase458 G3/G5, discharge O4, fill a source contract, or support a physical-unit
claim. Its selected specification, if any, is synthetic planning evidence only.
`promotedPhysicalMassClaimCount=0` on every branch.

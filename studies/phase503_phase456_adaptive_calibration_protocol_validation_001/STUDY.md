# Phase503: Phase456 adaptive-calibration protocol validation

## Scope

This phase prospectively freezes a deterministic synthetic
operating-characteristic battery before reading Phase502. It tests the
adaptive protocol's decision rules, not the Phase456 physics result. The
contract fixes seeds, covariance and autocorrelation families, positive
single- and two-component spectral cases, invalid controls, coverage and
model-selection thresholds, convergence/ESS decisions, escalation cases, and
cost-overrun behavior.

The Phase502 summary is exact-hash pinned. Synthetic rows cannot calibrate a
production power calculation or become empirical evidence. Negative and
assumption-conditional outcomes are retained without threshold tuning.

## Verdict taxonomy

- `invalid-precursor-or-validation-battery`
- `protocol-validation-passed`
- `assumption-conditional-protocol`
- `protocol-validation-failed`

## Boundaries

Phase503 runs no HMC, benchmark, reprocessing, or fresh acquisition. It does
not construct Phase481, authorize sampling, reinterpret Phase456, satisfy
Phase458 G3/G5, discharge O4, fill a source contract, or support a physical-
unit claim. `promotedPhysicalMassClaimCount=0` on every branch.

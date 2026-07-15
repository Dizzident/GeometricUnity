# Phase506: Phase456 selective-inference protocol validation

## Scope

Phase506 freezes and hashes its complete deterministic synthetic contract
before reading Phase505. The protocol returns a singleton model only when its
frozen evidence score crosses a decisive threshold; otherwise it returns the
set `{single, double}` and records `unresolved`. Truth-set coverage means the
returned set contains the oracle truth. An unresolved set is never counted as
a decisive success.

Every covariance/autocorrelation row requires decisive correct calls for the
identifiable single and separated-pair controls. Near-degenerate and weak-
secondary cases may remain correctly unresolved, but any wrong decisive call
fails the complete-menu gate. Signed, nonfinite, rank-deficient, convergence,
ESS, escalation, and cost-refusal controls are mandatory. No nominal subset
can substitute for the complete menu.

## Verdict taxonomy

- `invalid-precursor-or-validation-battery`
- `selective-protocol-validation-passed`
- `assumption-conditional-selective-protocol`
- `selective-protocol-validation-failed`

## Boundaries

This is deterministic synthetic/oracle planning evidence. It runs no HMC,
benchmark, reprocessing, or sampling; it does not reinterpret Phase456,
construct or authorize Phase481, satisfy Phase458 G3/G5, discharge O4, fill a
source contract, or support a physical-unit claim.
`promotedPhysicalMassClaimCount=0` on every branch.

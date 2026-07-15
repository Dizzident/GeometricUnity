# Phase502 implementation

Phase502 implements Amendment A12's target-blind adaptive-calibration protocol
specification. It exact-binds the Phase501 insufficiency terminal and the
Phase456 measured-cost record, and consumes its frozen contract before those
precursors.

The protocol retains configuration-level observables, uses four independent
chains and explicit batches, fixes covariance, autocorrelation, ESS,
split-R-hat, stability, invalid-data, and cost rules, and starts at `T=16`
with only predicate-driven escalation to `T=32`. The calibration ceiling is
2 CPU-weeks. Unknown empirical quantities remain unknown.

The terminal is `protocol-specification-ready`. The phase runs no sampler,
benchmark, or reprocessing job, creates no Phase481 pack, and grants no launch,
Phase458, O4, source-contract, physical-unit, or promotion authority.
`promotedPhysicalMassClaimCount=0`.

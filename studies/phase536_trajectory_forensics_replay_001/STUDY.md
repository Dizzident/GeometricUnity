# Phase536 trajectory-forensics replay

Phase536 is the Amendment A23 read-only behavioral replay of the Phase534
reduced scalar HMC. It exact-binds the Phase533 contract and summary, the
Phase534 contract, implementation, and summary, and the Phase535 summary.
The pre-review v1 contract remains preserved; the cross-review repairs are
frozen in v2 before the repaired replay.

The source-matched replay preserves Phase534's polynomial coefficients, scalar-HMC update
order, seeds and offset, initial scales, initial step size, leapfrog count,
warmup and retained lengths, acceptance decision, and windowed adaptation.
Additional observations do not consume random numbers or change a trajectory.
Because Phase534 did not preserve an immutable per-trajectory trace, Phase536
checks exact aggregate table metrics and makes no exact-trajectory identity
claim.

Telemetry is bounded to one first-failure record and twenty adaptation events
per chain, separate warmup and retained failure counts, and maximum absolute
finite state, momentum, gradient, and energy magnitudes. A rejected proposal's
state is recorded separately from the post-decision rollback state. Exact aggregate
acceptance counts, failure counts, and final step sizes must reproduce the
bound Phase534 summary before localization is reported.

The terminal reports only which execution segment contains aggregate failures
under this source-matched replay; their cause remains unresolved. This phase
does not repair or relabel Phase534, reopen the Phase535 pilot,
validate a complete lattice or production execution, or permit a physical-unit
claim. `promotedPhysicalMassClaimCount=0`.

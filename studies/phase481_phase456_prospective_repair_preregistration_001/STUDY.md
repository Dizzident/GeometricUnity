# Phase481: Phase456 Prospective-Repair Pre-registration (A4 skeleton)

Phase481 is execution-priority 5 after Phases477-480. Its initial terminal is
`preregistration-skeleton-awaiting-implementation`.

The implemented phase may create a new, hash-pinned, prospective repair pack
only after the amendment's upstream gates are satisfied. The pack must freeze
diagnostic hypotheses, estimators, sampler/control gates, power rules, and
terminal taxonomy before any fresh sampling.

This phase may never mutate the frozen Phase456 pack or raw artifacts,
reinterpret the invalid Phase456 rows, or treat the invalid artifact as rerun
authorization. This skeleton creates no pack, runs no sampler, and authorizes
nothing. Any future sampling also requires new written authorization or
applicable O4 coverage.

No measurement or source-contract field is emitted and
`promotedPhysicalMassClaimCount=0`.

The 2026-07-16 planning artifact records a construction-ready sequence and
the exact requirements forwarded by Phases502, 506, and 507. It is not a
pre-registration pack. It also records the unresolved geometry, RNG,
implementation-backend, resource-envelope, and Phase480 authorization gates;
therefore the skeleton terminal and every no-launch boundary remain unchanged.

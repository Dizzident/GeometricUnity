# Implementation P481 — Phase456 Prospective-Repair Pre-registration

Phase481 is the zero-compute A4 skeleton at execution priority 5, ordered after
Phases477-480. It emits `preregistration-skeleton-awaiting-implementation`.

The future implementation may produce a fresh prospective, hash-pinned pack
only. It cannot alter the frozen Phase456 pack or artifacts, interpret invalid
rows, sample, or authorize a rerun. The skeleton fills no contract and
preserves `promotedPhysicalMassClaimCount=0`.

## 2026-07-16 construction planning

Phase507 permits independent pack planning, so the construction sequence is
now recorded in
`studies/phase481_phase456_prospective_repair_preregistration_001/planning/phase481_pack_construction_plan_v1.json`.
The plan exact-binds the preserved Phase456 result and Phases502, 506, 507,
and 480. It forwards the four-chain adaptive-calibration, storage,
convergence, cost, and selective-inference requirements without creating any
pre-registration pack bytes.

Planning exposed four construction blockers that must not be guessed away:
the acquisition geometry, an explicit portable RNG and restart contract, a
matching CPU reference implementation, and a static resource envelope. The
current Phase452/456 implementation is isotropic `n^4`, whereas the forwarded
protocol names temporal extents 16 and 32 without freezing whether the future
geometry is `4^3 x T` or `T^4`. Phase480 also remains awaiting a genuine
signed external memo. Consequently the plan is complete, but pack
construction and every sampling action remain fail-closed.

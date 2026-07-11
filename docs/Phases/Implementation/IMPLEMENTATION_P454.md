# Implementation P454 - Beyond-Invariant-Ray Certificate Probe, Arms A/B (Team B, Wave-1 rank-2)

Phase454 executes the sampling-free arms of Team B's Wave-1 rank-2 item of
the committed three-team elimination program
(`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md` §1 item 7): exact
certificates, at second and third order, that decide whether a lower
configuration could hide OFF the translation-invariant rays the 443-453 line
probed. Arm C (structure factors on released sampling columns) is
pre-registered inside phase453 and is NOT part of this phase.

Directory: `studies/phase454_beyond_ray_quadratic_certificate_probe_001/`.
Deterministic, no sampling, ~47 s wall at the 4-core throttle
(`PHASE454_MAXDOP`, default 4).

## Construction

- **Arm A.** The joint `(omega, theta)` Hessian's 48x48 Hermitian momentum
  blocks from the 48 orbit-representative Hessian-vector products (phase448
  machinery copied verbatim: `HvStep = 1e-5`, IR zero-mode convention,
  Jacobi eigenvalue path - bit-compatibility enforced by a lineage battery).
  Per momentum sector the gauge kernel `ker L = ker d` is RECOMPUTED PER
  BACKGROUND as the kernel of the background-covariant curvature
  linearization `D_{omega0} = d + [omega0 ^ .]` (exact central difference of
  the exactly-quadratic discrete curvature; a canonical-chain fSign face
  gauge extends the oSign lattice-gauge discipline to face space), extracted
  per sector from `D(k)^dag D(k)` with a fail-closed spectral-gap rule,
  projected out by orthonormal complement restriction, and the complement
  eigensolved. The projected-out kernel sub-block spectra are recorded too.
- **Arm B.** `S_B(t v)` at `theta = 0` is exactly `a2 t^2 + a3 t^3 + a4 t^4`;
  the coefficients are solved exactly at `t = +-0.1, +-0.2` (the
  phase447-proven stencil scale) with independent `+-0.15` and 5-point-stencil
  cross-checks and a held-out `t = 0.3` exactness gate, over a pre-registered
  deterministic menu: 15 canonical 0/1 momenta x {cos, sin} x {45
  single-(type,Lie) probes + the 3 audited ray coefficient sets} = 1440 unit
  directions x 3 members. Degeneracy classifier: `a3^2 >= 4 a2 a4` (since
  `S_B >= 0` exactly, a degenerate second zero is the only
  first-order-transition-style opening at this order).
- **Backgrounds.** Trivial + phase448 rays (seeds 0/1; `Random(20260704 +
  31*seed)`) at the committed grid points t in {0.1875, 0.9375, 1.6875, 3.0}
  + the phase450/453 ray (`Random(20260703)`, su(2)-zero-mode-projected) at
  t in {0.5, 1.25, 2.375}; theta* projected-Newton warm-started walk
  (phase448 verbatim); identity control at theta = 0. The only RNG uses are
  these fixed-seed reproductions of committed conventions.

## Committed run record (all batteries green; phase PASSES)

`verdictKind = beyond-ray-instability-candidate-found` (pre-registered
terminal), `beyondRayQuadraticCertificateProbePassed = true`.

- **Trivial background - CLEAN.** Gauge kernel exactly **252 = 12 (k=0) +
  3 x 80** (machine-verified); **0 negative complement directions** for all
  three members (3393 positive / 243 zero = the exact theta-flat sector). At
  the one stationary configuration on the menu - the candidate vacuum - no
  beyond-ray quadratic descent direction exists.
- **Ray backgrounds - the transverse negatives are REAL and NAMED.** At all
  11 ray backgrounds the recomputed covariant kernel is EMPTY (dim 0 in
  every sector - the ker-d soft modes lift at t > 0), so the certificate
  finds **2016 named negative complement directions** (identity 660 / sd2
  668 / asd2 688, across 34 distinct momentum sectors; min complement
  eigenvalue **-1.42e-01**, identity, p448-seed1, t = 3.0; some directions
  theta-dominated, weight up to 0.97). This PROVES the phase448 negatives
  (58-68 per point) are not gauge artifacts. Honest reading, recorded in the
  output: they are Hessian negatives at NON-stationary ray points (S_B >= 0
  pins the global minimum at the origin) - the transverse instabilities the
  2026-07-04 review board flagged, now sector-resolved; exactly what the
  non-perturbative CEP/HMC line integrates.
- **Arm B - cubic-order exclusion.** 4320 exact solves; nonzero cubics on
  exactly **270 rows** (all and only the audited-ray-modulated directions;
  every single-type probe has a3 = 0 exactly). Max |a3|/a2 = **3.45e-03**;
  max degeneracy ratio a3^2/(4 a2 a4) = **5.46e-03 << 1**: NO menu direction
  is degeneracy-capable. Cross-estimators 2.6e-13; held-out exactness
  1.2e-13; on-ray parity |a3| <= 4.1e-15 (exact Z2).
- **Battery panel.** Covariance 8.0e-15; face covariance 0.0; objective
  consistency 1.2e-14; analytic-vs-FD gradient 8.9e-10; H-block
  reconstruction 1.0e-11; D-block reconstruction 4.7e-16; theta* residual
  4.4e-04 rel with abs-floor pass + equivariance 5.6e-05 (the phase448
  dual-gate convention); kernel gaps clean; partition trace 9.4e-16;
  S(0) = 0 and theta-flatness-at-origin exactly 0.0; **phase448 lineage
  24/24 exact-count matches** (bit-level reproduction of the committed
  spectra pipeline).

## What this narrows (and what it does not)

B2 frontier update: no beyond-ray descent direction exists at the trivial
background through cubic order on the certified menu, and the invariant-ray
transverse negatives are certified non-gauge and individually named for the
closure theorem-of-record (item 22). NOT claimed: any vacuum or scale, in
either direction - the certificates are finite-menu, n=3, quadratic+cubic
order; the 445-452 conventions carry `physicistReviewPending` (Wave-0 item
0.3 open); `promotedPhysicalMassClaimCount = 0`.

## Wiring

Built and run in a worktree while a full pass occupied the main checkout.
The exact wiring snippets for the main checkpoint (generator line, traversal
item, 8 scanner exclusions, phase101 block, phase202 checklist item,
verify asserts pinned to the ACTUAL values above) are in
`docs/Phases/Implementation/WIRING_P454.md`. Validate with a ~3-min
`--incremental` pass before committing; `--full` before any
promotion-relevant citation.

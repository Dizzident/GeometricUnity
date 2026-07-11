# Phase454: Beyond-Invariant-Ray Quadratic/Cubic Certificate Probe (Arms A/B)

Team B's Wave-1 rank-2 item of the committed three-team elimination program
(`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md` §1 item 7), Arms A and B
— the exact, sampling-free arms. Arm C (structure factors on released
sampling columns) is pre-registered inside phase453 and is NOT in scope here.

## The question

The 443–453 line probed the dynamical-scale question ON translation-invariant
rays (phase453's Stage-0 committed the repair of the one gate standing before
the on-ray null). The residual B2 opening this phase attacks: could a lower
configuration hide OFF the invariant rays? Two exact certificates answer at
quadratic and cubic order around a finite menu of audited backgrounds.

## Arm A — positive-semidefiniteness on the gauge complement per momentum sector

At the trivial background `(omega=0, theta=0)` and at the audited
invariant-ray backgrounds used by phases 448/450/453 (n=3 lattice-canonical
torus), the joint `(omega, theta)` Hessian is block-circulant: its full
spectrum comes from 48 orbit-representative Hessian-vector products,
DFT-assembled into Hermitian 48×48 momentum blocks (the phase448 machinery,
copied verbatim — the committed `HvStep = 1e-5`, IR zero-mode convention, and
Jacobi eigenvalue path are bit-identical, enforced by a lineage battery).

Per momentum sector the **gauge kernel `ker L = ker d` is RECOMPUTED PER
BACKGROUND**: `d` is the background-covariant curvature linearization
`D_{omega0} delta = d delta + [omega0 ∧ delta]`, obtained EXACTLY as a
central difference of the exactly-quadratic discrete curvature map (exact for
any step; roundoff only). The trivial-background kernel is never reused. Per
sector the kernel of `D(k)† D(k)` is extracted with a fail-closed
spectral-gap rule, projected out by orthonormal complement restriction (plain
Euclidean coefficient inner product — the recorded phase450 convention), and
the complement block is eigensolved.

- Any NEGATIVE complement direction ⇒ a **named beyond-ray instability
  candidate** (sector k, background, member, eigenvalue, direction profile).
- All-nonnegative across all sectors and backgrounds ⇒ the quadratic
  certificate that no beyond-ray descent direction exists at these
  backgrounds.
- The projected-out kernel sub-block spectrum is ALSO recorded (diagnostic —
  the projection hides nothing).

Kernel dimension asserts: at the n=3 trivial background
`dim ker d = 3·(V − 1 + b1) = 3·(81 − 1 + 4) = 252`, split as 12 at k=0 (the
harmonic sector, b1 = 4 × su(2)) plus 3 per nonzero momentum (the exact
sector) — machine-verified, and the recomputed per-background dimensions are
recorded.

Background menu (finite, pre-registered):

| Background | t values | theta |
|---|---|---|
| trivial | 0 | 0 |
| phase448 ray, seed 0 (`Random(20260704)`) | 0.1875, 0.9375, 1.6875, 3.0 (committed grid pts) | theta\* projected-Newton walk (phase448 verbatim) |
| phase448 ray, seed 1 (`Random(20260704+31)`) | same | same |
| phase450/453 ray (`Random(20260703)`, su(2)-zero-mode-projected) | 0.5, 1.25, 2.375 (spans the committed CEP window range incl. the antisymmetry-max bin) | same |

Identity control at theta = 0 throughout.

## Arm B — the directional-cubic a3(v) battery

On invariant rays `a3 ≡ 0` by the reviewed omega → −omega Z2; OFF-ray that
argument fails. `S_B(t·v)` at `theta = 0` is EXACTLY
`a2 t² + a3 t³ + a4 t⁴` (S_B is exactly quartic in omega), so the
coefficients are solved exactly from evaluations at `t = ±0.1, ±0.2` (the
phase447-proven stencil scale), cross-checked by the independent `±0.15`
odd-part estimator and the 5-point third-derivative stencil, and gated by a
held-out `t = 0.3` quartic-exactness test.

Pre-registered deterministic menu (no RNG): all 15 canonical 0/1 momenta of
`(Z_3)^4` (shells 1–4, the lowest nonzero momentum shells) × {cos, sin}
waves × {45 single-(edge-type, Lie) probes + the 3 audited ray coefficient
sets} = 1440 unit directions, × 3 members.

Classifier: since `S_B ≥ 0` exactly with `S_B(0) = 0`, no strictly LOWER
beyond-ray configuration can exist; the only first-order-transition-style
opening at this order is a DEGENERATE second zero on the ray, which requires
`a3² ≥ 4·a2·a4`. `a3² < 4·a2·a4` on the whole menu is the cubic-order
exclusion certificate. On-ray `a3 = 0` parity controls, `S(0) = 0`, and
theta-flatness-at-origin batteries run alongside.

## Batteries (fail-closed)

S_B translation covariance; curvature face-space covariance (the oSign
lattice-gauge lesson extended to a canonical-chain fSign face gauge);
objective-path consistency; analytic-vs-FD gradient; H-block reconstruction
vs direct Hv (the phase448 2.9e-11 discipline); D-block reconstruction;
theta\* residual + full-gradient equivariance; identity theta-independence;
kernel dimension/pattern/spectral-gap/eigenvector-residual gates; complement
construction and partition-trace consistency; **phase448 lineage** (the
full-block positive/zero/negative counts at the 24 shared (member, seed, t)
points must equal the committed phase448 values EXACTLY); Arm B
cross-estimator, held-out exactness, positivity, and parity gates.
Seed-free determinism: the only `System.Random` uses are the fixed-seed
verbatim reproductions of the committed phase448/phase450 ray conventions;
every new menu object and battery vector is closed-form deterministic.

## Pre-registered verdict taxonomy

- `beyond-ray-excluded-at-certified-backgrounds` — Arm A all-nonnegative on
  the gauge complement AND Arm B degeneracy-free on the menu.
- `beyond-ray-instability-candidate-found` — any named negative complement
  direction (sector, background) or degeneracy-capable cubic.
- `inconclusive-gates-failed` — any battery red; no physics reading.

The phase PASSES on internal consistency regardless of which verdict the
computation produces.

## Result (committed run — 46.6 s wall, every battery green)

VERDICT: **beyond-ray-instability-candidate-found** (pre-registered terminal;
the phase PASSES — `beyondRayQuadraticCertificateProbePassed = true`).

- **Arm A, trivial background — CLEAN.** The gauge kernel is exactly
  **252 = 12 (k=0) + 3 × 80 nonzero-k** (machine-verified pattern); the
  complement spectrum has **0 negative directions** for all three members
  (3393 positive / 243 zero — the theta-flat sector, since Upsilon vanishes
  identically at omega = 0). At the one stationary configuration on the
  table — the candidate vacuum — no beyond-ray quadratic descent direction
  exists.
- **Arm A, ray backgrounds — the transverse negatives are REAL and now
  NAMED.** At every one of the 11 ray backgrounds the recomputed covariant
  kernel `ker d_{omega0}` is **EMPTY** (dim 0 in every sector — the ker-d
  soft modes lift at t > 0, sharpening the review-board record), so the
  gauge complement is the full block space and the certificate finds
  **2016 named negative directions** (660 identity / 668 sd2 / 688 asd2
  across 34 distinct momentum sectors; min complement eigenvalue
  **−1.42e-01** at identity, p448-seed1, t=3.0). This PROVES the phase448
  negatives (58–68 per point, reproduced EXACTLY by the 24/24 lineage
  battery) are not gauge artifacts. Honest reading: these are Hessian
  negatives at NON-stationary ray points (the S_B gradient does not vanish
  there; S_B ≥ 0 pins the global minimum at 0), i.e. the transverse
  instabilities the 2026-07-04 review board said the retired convention
  discarded — now with per-sector names, eigenvalues, and direction
  profiles (some are theta-dominated, weight up to 0.97). They are exactly
  the directions the non-perturbative CEP/HMC line (450/453) integrates.
- **Arm B — cubic-order exclusion.** 4320 exact cubic solves; nonzero
  cubics on exactly **270 rows** — all 90 audited-ray-modulated directions
  per member; every single-(type,Lie) probe has a3 = 0 exactly (the off-ray
  cubic needs cross-type structure). Max |a3|/a2 = **3.45e-03**, max
  degeneracy ratio a3²/(4·a2·a4) = **5.46e-03 « 1**: NO menu direction
  satisfies the degenerate-second-zero precondition. Cross-estimators agree
  to 2.6e-13, held-out quartic exactness 1.2e-13, on-ray parity |a3| ≤
  4.1e-15 (exact Z2 confirmed).

Battery panel: covariance 8.0e-15, face covariance 0.0, objective
consistency 1.2e-14, H-block reconstruction 1.0e-11 (the phase448
discipline), D-block reconstruction 4.7e-16, partition trace 9.4e-16,
S(0) = 0 and theta-flat-at-origin exactly 0.0, lineage 24/24.

B2 narrowing recorded: no beyond-ray descent at the origin through cubic
order on the certified menu; the ray-point transverse negatives are real
(non-gauge) named candidates feeding the closure theorem-of-record — not a
vacuum claim in either direction.

## Scope honesty

Certificates at a FINITE background menu, ONE volume (n=3), and
quadratic+cubic order. This narrows B2 and feeds the B-closure
theorem-of-record (program item 22); it does NOT close B2 alone. All
eigenvalues/coefficients are lattice-unit, workbench-relative structure data
of the su(2) reduced Spin(4) slice — no GeV, no poles, no promotion; the
445–452 conventions carry `physicistReviewPending` explicitly;
`promotedPhysicalMassClaimCount = 0`.

## Run

```
dotnet run --no-build -c Release --project \
  studies/phase454_beyond_ray_quadratic_certificate_probe_001/Phase454BeyondRayQuadraticCertificateProbe.csproj
```

Env knobs: `PHASE454_OUTPUT_DIR`, `PHASE454_MAXDOP` (default 4 — the phase
throttles all `Parallel.For` loops). Deterministic; ~47 s wall at 4 cores.

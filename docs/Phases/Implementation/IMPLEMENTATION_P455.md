# Implementation P455 - Exact Fermionic Backreaction Probe (Team B, Wave-2)

Phase455 implements plan item 1 of `docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`.
Registry number 455 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`. The STEP 0
skeleton has been replaced by the full pre-registered T0-T3 implementation; the
pre-registered design is fixed in
`studies/phase455_exact_fermionic_backreaction_probe_001/STUDY.md`.

## What this phase computes

The exact one-loop backreaction functional `V(t) = S_B(t*u) + N_f*V_f(t)` along
constant rank-1 rays `omega = t*u`, on the COMMITTED phase428 closed-form ray
spectrum `lambda_i^2(t) = (s1 + t*u_c)^2 + (s2 + t*u_c)^2`. It decides, with EXACT
arithmetic over `Q(sqrt 3)` (BigInteger `Rational` + `Q3 = a + b*sqrt3` fields +
dense `Poly` arithmetic) and Sturm-certified real-root isolation, whether a
derived-content ray develops a below-origin radiative well on `(0, t_max]`.

Committed conventions (cited, realized exactly, `physicistReviewPending`):

- `S_B(t*u)` := committed phase430 bosonic one-loop determinant
  `W_B(t) = sum log(eps^2 + t^2 m^2)` (phase430 Program.cs 230-244). Recorded
  workbench model; the `S_B := W_B` identification is a pre-registered modeling
  choice. **Load-bearing** (see the flag below).
- `V_f(t)` := committed `W_F(t) = -(1/2) sum log lambda^2` (phase428 line 334,
  phase430 line 225). Sign negative.
- Per-mode coefficients: bosonic `+1` per (momentum, nonzero adjoint `m^2`);
  fermionic `-2*N_f` per (momentum, nonzero gauge eigenvalue).
- Zero-mode axis: Z-a (symmetric `k=0` exclusion) PRIMARY; Z-b (keep `k=0`,
  phase447 soft floor); Z-c (keep `k=0`, exclude only exact zeros).
- Derived verdict content `N_f = 4` fundamental (Phase404 single family = 4
  su(3)-active fundamentals). Phase433's `n_f=3` (`N_f=12`) is IMPORTED and
  non-gating. Occupancy sweep robustness-only. Critical-coupling column in NO
  terminal conjunct.

## Result

**Terminal: `exact-fermionic-backreaction-probe-convention-fragile` (T3).** All
seven batteries g0-g6 are green.

| arm | derived N_f=4 result |
|-----|----------------------|
| Z-a PRIMARY (k=0 excluded) | perturbative (g1 max ratio ~0.23); **no below-origin well** (interior minima ABOVE origin: T/D `t~1.619` `V~+1.92`; S `t~1.418` `V~+9.15`). Reads T1. |
| Z-b / Z-c (k=0 included) | T/D develop a **below-origin well** (`t~1.758`, `V~-18.4`); S does not. |
| verdict | **flips with the zero-mode convention => T3**; flip axis routed to O4. |

Imported 3-family (`N_f=12`, non-gating): non-perturbative (g1 ratio ~1.44),
below-origin wells on all axes — outside the derived content and the controlled
regime, decides nothing.

L5 is NOT closed (only T1 closes it). No `t*` forwarded to the phase456 pack
(forwarding is a T2-only action).

## Batteries (all green)

- g0 committed-phase428 spectra hash pin (verdict + `targetBlindConstructionHash`
  match; exact eigenvalue multisets and ray spectrum reproduced, residual ~4e-16).
- g1 perturbativity `|N_f*V_f| <= 2*S_B` at C=2 over the derived rows (satisfied,
  non-marginal); imported 3-family violates it (non-gating).
- g2 exact-vs-float cross-check (residual ~4e-16).
- g3 set-wise evenness of the +/-k-summed `V_f` (all reps/axes even).
- g4 synthetic positive control (a spectrum that DOES produce a below-origin well;
  pipeline detects exactly one interior min + one max => T2 reachable).
- g5 Sturm battery on known polynomials (root counts and sqrt3 isolation exact).
- g6 convention-flip matrix Z-a/Z-b/Z-c (records the flip).

## Framing

Zero physical claims. Every quantity is a workbench-relative structure of the
reduced su(3) slice in lattice units; no GeV/pole/VEV promotion; no Phase201 or
Phase256 field filled; `promotedPhysicalMassClaimCount = 0`;
`physicistReviewPending = true`; lattice-unit quantities stay in lattice units.

## Load-bearing flag for the wiring lead

`S_B` was NOT given a committed closed form for the phase428/430 su(3) workbench
by the plan. The only committed bosonic functional there is phase430's one-loop
determinant `W_B`, which this phase adopts as `S_B` (documented, workbench model,
`physicistReviewPending`). This choice is what makes the g1 perturbativity gate
meaningful and the T3 flip physical. If the referee intends a tree `S_B` that
dominates `V_f`, the g1 gate and the flip would need re-derivation. Recorded here
and in the report so the terminal/asserts can be pinned with the choice explicit.

## Verify-assert expectations (for the wiring lead to pin)

The skeleton fields `exactFermionicBackreactionProbeSkeletonBuilt` and
`interimTerminal` are GONE; the current asserts in
`scripts/verify_boson_claim_integrity.sh` (~L5512), the phase202 checklist
(~L6456), and the phase101 display (~L8390) must be updated to the implemented
fields:

- `terminalStatus === "exact-fermionic-backreaction-probe-convention-fragile"`
- `verdictKind === "convention-fragile"`
- `exactFermionicBackreactionProbePassed === true` (batteries green)
- `anyBatteryRed === false`
- `applicationSubjectKind === "exact-fermionic-backreaction-probe"`
- `targetBlindConstruction === true`, `physicistReviewPending === true`
- `promotedPhysicalMassClaimCount === 0`
- `criticalCouplingColumnInTerminalConjunct === false`
- `noGevPromotion === true`, `scaleIsWorkbenchRelativeCandidateOnly === true`
- `o4QueueItems` contains `"zero-mode-convention-flip-axis"` and `"sb-workbench-model-choice-w430"`

## Rulings (team lead, 2026-07-12)

T3 is the terminal as computed (reframing to Z-a-primary-T1 after seeing the flip
would be a forbidden post-hoc move; L5 stays open honestly). The `S_B := phase430
W_B` identification is accepted as a labeled workbench-model convention with
`physicistReviewPending`, on condition that BOTH review items are named to the O4
register — done via the `o4QueueItems` field. The `-(1/2)` `V_f` prefactor from the
committed record (over the plan shorthand `-1`) is confirmed correct.

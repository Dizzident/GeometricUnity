# Phase457: Upsilon Portal Stage A

Team B, plan item 10 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`);
`TEAM_ELIMINATION_PROGRAM_2026-07-10.md` item 15 (WS3 Upsilon-portal on the
dynamical `(omega, theta, M)` measure; `M` is a **labeled probe-field
convention**, never sourced). This phase advances the Step-0 skeleton
(`awaiting-stage-a`) to committed **Stage A**: boundedness certificates, the
null-hash firewall, and the (gated-off) Arm Q measurement code.

## Terminal

`stage-a-certificates-committed` — all pre-registered cells carry a definite
certificate. The portal VERDICT stays **withheld** (the firewall is closed);
nothing is measured or promoted; `physicistReviewPending = true`.

## The hold (binding)

Verdict emission is gated on a **conjunction**, both required:

1. the **phase466** WS3-completion schema-pin (`schemaId`,`schemaHash`), AND
2. the **O4 M-probe ruling**.

Pre-unlock the **only reachable summary terminal is
`measurement-recorded-verdict-withheld`**: a measurement may be recorded, a
verdict may not be emitted. The stricter methodology hard-gate governs; the
physics referee's all-cells-unbounded relaxation is banked as a bulletin
excerpt for the next adversarial round.

## (1) Stage A — exhaustive-cell boundedness certificates (EXACT mathematics)

Portal action class (reduced su(2) Spin(4) slice / lattice-canonical torus, the
phase450 setting):

```
S_portal(omega,theta,M) = S_B(omega,theta)
                        + kappa * sum_edges ||D_omega M||^2
                        + sum_x [ lambda|M_x|^4 + (mu2 + g*Omega_x)|M_x|^2 ]
```

`S_B = (1/2)||Upsilon||^2 >= 0`; the covariant kinetic `||D_omega M||^2 >= 0`
is the `|D M|^2`-type portal proper (couples `M` to `(omega,theta)`, only helps
boundedness); `Omega_x >= 0` is a curvature-invariant density — the direct
curvature portal (the phase410 curvature-coupling class), convention `MAG`
(`Omega = ||F||`, `~ t^2` on rays) or `DEN` (`Omega = ||F||^2`, `~ t^4`).

Certified arm `kappa = 0`; `kappa > 0` adds a positive-semidefinite kinetic
form that only raises the floor, so BOUNDED certificates hold for all
`kappa >= 0`. The certificates rest on already-committed structural facts —
`S_B >= 0`; control-branch `Upsilon = F` so `||F||^2 = 2 S_B` **exactly**
(`rho = 1`); `S_B ~ t^4`, `||F|| ~ t^2` on non-flat rays; `F = 0` on the flat
vacuum (phase405/410) — via elementary quartic-completion / sum-of-squares /
growth-rate lemmas. No sampling; no HMC.

The sign-cell menu is disjoint and exhaustive over
`sign(lambda) x sign(g) x sign(mu2) x convention` (machine-checked, 24/24):

| cell | predicate | verdict | lemma |
|------|-----------|---------|-------|
| CELL-Q-NEG | `lambda<0` | UNBOUNDED | L-Q quartic runaway |
| CELL-Z-MASSNEG | `lambda=0 & mu2<0` | UNBOUNDED | L-MASS mass runaway on flat vacuum |
| CELL-Z-PORTALNEG | `lambda=0 & mu2>=0 & g<0` | UNBOUNDED | L-PNQ portal runaway, no quartic |
| CELL-Z-SOS | `lambda=0 & mu2>=0 & g>=0` | BOUNDED (floor 0) | L-SOS sum of squares |
| CELL-P-GNONNEG | `lambda>0 & g>=0` | BOUNDED | L-CS complete the square |
| CELL-P-DEN | `lambda>0 & g<0 & DEN` | UNBOUNDED | L-SUP `-t^8` dominates `S_B~t^4` |
| CELL-P-MAG | `lambda>0 & g<0 & MAG` | CONDITIONAL-CRITICAL | L-CRIT `g_crit=sqrt(2 lambda/rho)` |

CELL-P-MAG carries the exact critical coupling `g_crit = sqrt(2 lambda)` at
`rho = 1` (control): `|g|<g_crit` BOUNDED, `|g|>g_crit` UNBOUNDED (`kappa=0`),
`|g|=g_crit` decided by `sign(mu2)`. The unbounded cells are named certificates
that **constrain the probe design**, not physics claims.

## (2) Null-hash firewall (keyed to phase466)

No verdict-bearing summary field is emitted unless the on-disk phase466 schema
`{schemaId, schemaHash}` matches the pinned pair
(`ws3-vev-completion-contract-schema-v1`,
`7159ea49a45e3044c4393542b24a5db596f5d1423150020b072849ec8cb322b9`) AND the
single Phase480 derivative contains a verified, accepted C3 probe-only ruling.
The derivative does **not** exist, so the portal verdict is withheld. The old
three-candidate-path interface and its two-field boolean/signer shape are
retired and cannot open the conjunction.

## (3) Arm Q — motivation-gate measurement code, IMPLEMENTED and GATED OFF

Arm Q measures `<|M|^2>` and its connected susceptibility on a seed-regenerated
`(omega,theta,M)` ensemble (phase450/453 HMC format), guarded by the
RNG-stream-neutrality battery (advancing the `M`-sector stream must leave the
`(omega,theta)`-marginal invariant within MC error; "piggyback" language
deleted; FRESH-labeled arm is the alternative). Execution is gated behind
`G-Q1` firewallOpen, `G-Q2` a committed ensemble with green precursors, `G-Q3`
the neutrality battery passing, `G-Q4` Team C co-signature for any deadline
terminal (never closes L7). The committed default runs Arm Q once `G-Q1..G-Q3`
and the Phase479 prospective motivation pack hold. Here they do not, so only a
synthetic plumbing self-test runs (zero
physics; positive control neutral, negative control contaminated — the battery
has teeth).

## Post-unlock terminals (not reachable pre-unlock)

`portal-measure-nonexistent-at-workbench-scope` / `portal-null-measured` close
limb L7 with named reopening conditions; `portal-scale-candidate` is a
convention-scoped dynamical-scale candidate feeding anchor-free ratios only
(su(2)-honest scope: a gap proves a singlet, never a labelled physical scalar).
Any deadline terminal requires Team C co-signature and never closes L7.

## Framing

Stage A is exact mathematics about the action class; it constrains the probe
design and is not a physics claim about any target. Lattice-unit quantities
stay in lattice units; no GeV/pole/VEV promotion; `M` is a labeled probe
convention (O8); `promotedPhysicalMassClaimCount = 0`.

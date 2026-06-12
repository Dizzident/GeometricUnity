# Phase412: Quartic SM-Doublet Intersection Analysis

## Question

Phase411 left the quartic SM-stable analysis as the named remaining
order (welded-singlet dimensions 9856 / 9632 per channel - too large for
the blockwise stable-subspace machinery). USER DIRECTIVE (2026-06-12):
execute it. Does any quartic spinor composite carry a welded spacetime
scalar with the SM-doublet pattern?

## Construction

AMBIENT-INTERSECTION formulation, strictly stronger than the
stable-subspace one: if any quartic composite were a welded scalar
carrying the doublet pattern, the ambient 16^(x4) would contain a state
simultaneously in (i) the welded (a,b)-isotypic sum allowed by the
channel's spacetime 2-leg factors and (ii) the SM-doublet isotypic
(color-singlet, j_L = 1/2, |Y| = 1/2). Method (exact, deterministic):
the SM-diagonal basis of the 16 makes all SM Cartans diagonal on
16^(x4) by index arithmetic; the doublet-pattern candidates live in the
weight-filtered sector V_w (|Y_tot| = 1/2, color weight 0,
|mL_tot| = 1/2); the doublet isotypic D is the joint kernel of C_color
and (C_L - (3/4)kappaL)^2 restricted to V_w (dense Jacobi,
residual-verified); the welded side uses product-form spectral
projectors in the commuting sub-Casimirs C_A, C_B (grid j(j+1)kappa,
j = 0..6, nearest-root-first, idempotency-verified), with channel label
sets from exact 2-leg su(2) character arithmetic; the intersection is
the eigenvalue-1 count of the Hermitian Gram <d_k, P_allowed d_l>.

## Results

- Candidate sector V_w: 896 of 65536 states. SM-doublet isotypic D:
  exactly 480 real dimensions (kernel residual 8.0e-15).
- Exactness: weight-sector leakage 3.8e-16; Cartan residuals 7.9e-17;
  projector idempotency 9.4e-15; per-channel quartic singlet dimensions
  reproduce Phase411 (LLLL = 9856, LLRR = 9632).
- NEW CHANNEL DATA: the odd-mixed channels LLLR and LRRR have ZERO
  welded singlets at all (their Gram is identically zero).
- **THE INTERSECTION IS ZERO IN EVERY CHANNEL** - with decisive margins:
  top Gram eigenvalues 0.146 (LLLL), 0.000 (LLLR), 0.479 (LLRR),
  0.000 (LRRR), 0.113 (RRRR), and 0.604 for the union of all labels;
  an intersection state would require eigenvalue exactly 1.
- **Verdict**: `quartic-order-closed-no-welded-scalar-sm-doublet-in-
  any-channel`. Since the unrestricted tensor power contains every
  statistics-projected composite space, the closure covers
  antisymmetrized composites too. Together with Phases 408-411, every
  internal composite-extraction route on every probed carrier is now
  closed through QUARTIC order. Remaining named routes: the draft's
  unobserved-phase fields, a noncompact real-form evasion (Nguyen-Polya
  caveat carried), or a new primary-source specification.

## Status

Fail-closed. The ambient-intersection argument is one-directional (zero
closes outright; nonzero would have been necessary-only); complex/
compact arithmetic; no dynamics, no scales; nothing promoted; zero
contract fields.

## Reproduce

```bash
dotnet run --project studies/phase412_quartic_sm_doublet_intersection_analysis_001/Phase412QuarticSmDoubletIntersectionAnalysis.csproj
```

(~8 minutes; parallel over cores.)

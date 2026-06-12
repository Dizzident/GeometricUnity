# Phase411: Quartic / Dirac-Squared Spinor-Sector Composite Probe

## Question

Three converging motivations point at spinor-sector composites: Phase409
(quartic = lowest open order; frame-cross block cannot carry the
doublet), Phase410 (the bosonic curvature-coaxing realization is closed;
the claim's own wording places the VEV "in a Dirac-like operator"
coupling "two chiral halves"), and the primary heuristic that the
"quartic Higgs piece" arises from "Dirac squaring of a quadratic". Can a
spinor BILINEAR (the quadratic) carry a welded-spin-0 SM doublet, so
that its Dirac square supplies the quartic Higgs invariant?

## Construction

Chimeric chiral carriers S_L = 2_L (x) 16 and S_R = 2_R (x) 16 (draft
section 9: S_g(C) = S(V) (x) S(H*)). The Weyl halves come from a Cl(4)
tensor-string construction (homomorphism machine-verified, residual <=
1e-10, labels machine-discovered not assumed); the internal 16 from the
Phase404 Cl(10) construction; the weld pi: so(4) -> so(10) from
Phase408. Welded action D(M) = sigma(M) (x) I + I (x) Sigma(pi(M)); SM
chain (su(2)_L, su(2)_R Cartan, Y, full color su(3)) in the spinor
representation on the 16 factor. All kernels blockwise over sub-Casimir
eigenspaces; every content claim double-checked by character arithmetic.

## Results

- **Welded branching (exact)**: the internal 16 = (1/2,3/2) + (3/2,1/2)
  (realified multiplicity 2 each). S_L and S_R contents recovered and
  character-matched.
- **THE DIRAC MASS CHANNEL THEOREM**: S_L (x) S_R - the Yukawa/mass
  channel, exactly where the SM Higgs couples - contains ZERO welded
  scalars. Its welded content pairs (half-integer, integer) types
  against (integer, half-integer) types, so no singlet can form, for ANY
  alignment. A "VEV in a Dirac-like operator coupling two chiral halves"
  cannot be a welded spacetime scalar on this chain.
- **Majorana channels**: S_L (x) S_L and S_R (x) S_R each carry exactly
  16 welded scalars (realified; direct blockwise kernel = character
  count). The largest SM-stable subspace of that spin-0 sector contains
  ZERO SM-doublet-pattern states (color-singlet, j_L = 1/2, |Y| = 1/2).
- **Quartic counts** (character arithmetic, data): (LR)^2 spin-0
  dimension 9632; (LL)^2 9856. The quartic SM-stable analysis is the
  named remaining order.
- **Verdict**: `spinor-bilinear-channels-carry-no-welded-scalar-sm-
  doublet`. The Dirac-squared composite reading of the heuristics is
  CLOSED at its natural (bilinear) channel level, alongside the
  Phase409 frame-cross closure and the Phase410 bosonic closure. Named
  remaining: the quartic SM-stable analysis, the draft's
  unobserved-phase fields, and a noncompact real-form evasion (all
  arithmetic here is complex/compact - the Nguyen-Polya complexification
  caveat is carried).

## Status

Fail-closed. Exact representation arithmetic only; composites are not
the draft's fundamental scalar; Pin-parity classification not attempted
(named); no dynamics, no scales; nothing promoted; zero contract fields.

## Reproduce

```bash
dotnet run --project studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/Phase411QuarticDiracSquaredSpinorCompositeProbe.csproj
```

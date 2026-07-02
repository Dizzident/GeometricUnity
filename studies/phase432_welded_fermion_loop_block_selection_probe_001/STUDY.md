# Phase432: Welded-Fermion-Loop Block-Selection Probe

Phase428 established a no-go: any su(N)-invariant fermion sector whose one-loop
determinant is a *class function* of the gauge direction cannot distinguish
mutually-conjugate internal rays. This phase asks whether that no-go survives
the GU draft's actual fermion structure, which is **welded**: the carrier
`S = 2 (x) 16` ties the spacetime Weyl factor and the internal 16 through the
chimeric weld `pi: so(4) -> so(10)` (the Sym^2 embedding of Phase408/417),
acting diagonally as the spacetime spin generators on the 2 **and**
`Sigma16(pi_q)` on the 16.

## Setup

Two carriers on a 4x4 periodic lattice, 4-dim Euclidean Dirac spinors, internal
16 of SO(10). The 4-dim spinor is the Cl(4) Dirac spinor whose `gamma4[0]`,
`gamma4[1]` are **exactly** the Phase428 lattice gammas `sigma_x (x) I2`,
`sigma_y (x) I2` (battery: residual 0), and whose full so(4) rep `sigma4(M4_q)`
supplies the weld coupling.

- **Carrier A** (internal-only reference, Phase428 logic with gauge factor 16):
  `D_A(t,u) = sum_mu gamma_mu (x) (i hop_mu (x) I16 + t I_V (x) Herm16(u))`,
  `Herm16(u) = -i Sigma16Plus(u)`. Ray spectrum is the closed form
  `lambda^2 = (s1 + t u_c)^2 + (s2 + t u_c)^2` over lattice momenta and `Herm16`
  eigenvalues `u_c` (verified against a per-momentum dense solve, residual
  6e-15), so it is a class function of the `Herm16(u)` eigenvalue multiset.
- **Carrier B** (welded): `D_A` plus the recorded weld coupling
  `t_w sum_q c_q [ sigma4(M4_q) (x) I_V (x) Sigma16Plus(pi_q) ]`, `c_q = 1`,
  with `pi_q = SymEmbedX(M4_q)`. The coupling is a tensor of two anti-Hermitian
  factors, hence Hermitian (battery: residual 0). `t_w = 0` reproduces carrier A
  exactly (battery: su(2)_L-axis functional split 5e-13). `t_w in {0, 0.5, 1.0}`.

Constant fields make the operator momentum-diagonal; each momentum block is
`4 x 16 = 64` complex. The block-diagonalization (with weld and gauge present)
was cross-checked against a full dense solve: numpy 2D 4x4 residual 1.7e-14
(recorded prototype battery), C# 1D 4-site residual 6e-13.

## Result (machine-verified)

- **internalOnlyLoopDegeneraciesRecorded = true**: at `t_w = 0` the three
  su(2)_L axes are exactly loop-degenerate (Herm16 multiset residual 1e-16;
  each axis carries `{-1/2 x4, 0 x8, +1/2 x4}`); hypercharge Y is on a distinct
  multiset (distance 0.500); the two color Cartans are distinct (distance 0.30).
- **weldBreaksInternalLoopDegeneracies = true**: turning on the weld splits the
  previously-degenerate su(2)_L axes — max functional split `0.000` (t_w=0),
  `0.347` (t_w=0.5), `3.399` (t_w=1.0). The su(2)_L conjugator that made the
  axes loop-degenerate no longer commutes with the welded operator because
  su(2)_L overlaps the weld's `pi` image in the 6789 block.
- **weldChangesSu2LVsHyperchargeOrdering = false**: su(2)_L stays deeper than Y
  at every `t_w`, though the relative gap changes strongly (`W_su2L, W_Y` =
  `226.99, 525.55` at t_w=0; `4.35, 15.50` at t_w=1.0).

Conclusion: the Phase428 internal-only class-function no-go does **not** survive
intact for the GU-draft's welded fermion structure. The weld is exactly the
su(N)-invariance-breaking structure the no-go named as the only escape, and here
it is **source-pinned** rather than invented.

## Fail-closed

The weld ITSELF is source-pinned (Phase408/417). Its coupling normalization
(`t_w` grid, `c_q = 1`), the 4x4 lattice, the 4-dim spinors, and the naive Dirac
are recorded workbench conventions. Splitting the su(2)_L-axis loop degeneracy
is a representation/one-loop distinguishability statement, **not** a doublet-VEV
selection, a scalar source, or a mass prediction. No welded-carrier action, VEV
selection, observed electroweak rows, weak-angle lineage, pole extraction, or
GeV normalization is supplied. No Phase201 or Phase256 field is filled; nothing
is promoted.

## Run (Release, ~22 s)

```bash
dotnet run -c Release --project studies/phase432_welded_fermion_loop_block_selection_probe_001/Phase432WeldedFermionLoopBlockSelectionProbe.csproj
```

# Implementation P428 - Fermion-Loop Block-Selection No-Go Probe

Phase428 adds `studies/phase428_fermion_loop_block_selection_no_go_probe_001`,
the first experiment of the 2026-07-01 beyond-the-literature directive,
deciding the fermionic-backreaction mechanism class that Phases 405/410/418
left open.

## What It Computes

- Builds su(3) exactly (Gell-Mann fundamental, structure constants,
  adjoint representation) with exactness batteries (trace normalization,
  reconstruction residual <= 1e-13).
- Records the conjugacy witness: the color-swap permutation `g` maps
  `lambda_4` to `lambda_1` exactly, placing all seven T/D axes on ONE
  adjoint orbit; the eigenvalue multisets of axes 1-7 agree to 3.3e-16 in
  both representations, while the `lambda_8` axis lies on a distinct orbit.
- Evaluates the one-loop fermion determinant along the constant rank-1
  block rays of the Phase418 menu on a recorded workbench (4x4 periodic
  lattice, 4-dim Euclidean spinors, naive central-difference Dirac,
  fermions in the fundamental 3 and adjoint 8), using the exact
  closed-form ray spectrum
  `lambda^2 = (s1 + t*u_c)^2 + (s2 + t*u_c)^2` (verified against a dense
  192-dim Hermitian solve, residual 1.6e-13). Zero-crossing `t` values
  along rays are enumerated exactly and the small-t windows avoid them.

## Exact Results

- `tripletDoubletFermionLoopExactlyDegenerate=True` (potential residual
  1.1e-13 across all seven T/D axes, both representations).
- `singletAxisPotentialDistinct=True` (the only distinction the loop can
  make is orbit type).
- Large-t log slopes match the coupled-mode counts exactly:
  fundamental T/D -128, S -192; adjoint T/D -384, S -256 - the
  functional falls without bound along every ray, so
  `fermionLoopProvidesPositiveQuarticStabilizer=False`.
- `doubletSelectedByFermionLoop=False`,
  `fermionLoopBlockSelectionMechanismClosed=True`.

## Scientific Boundary

The class-function argument is representation- and discretization-
independent for any su(3)-invariant fermionic sector on constant rank-1
rays; the workbench choices are recorded conventions. The no-go is scoped
to one-loop determinants on constant rank-1 rays. The only escape -
su(3)-breaking fermionic structure (background masses, chemical
potentials, non-invariant occupations) - is not defined by any reviewed
source. No Phase201 or Phase256 field is filled; nothing is promoted.

With Phase428 the named internal mechanism classes for doublet VEV
selection on the control branch are all closed: bare bosonic objective
(Phase405), uniform curvature coupling (Phase410), direction-dependent
quadratics without imported stabilizers (Phase418), the action's own
quartic (2026-07-01 Phase410 post-processing), and one-loop fermionic
backreaction (this phase).

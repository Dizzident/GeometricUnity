# Phase403: Adjoint Doublet-Substructure Branching Probe

## Question

Phase402 sharpened the scalar-sector gap to: exhibit a doublet-equivalent
substructure inside the pulled-back (Lie-algebra-valued) connection
component, since an adjoint-triplet VEV cannot produce the SM neutral
sector. Can an adjoint-valued scalar contain a doublet at all, and what
does that mechanism imply?

## Construction

Pure group theory from the repo's own structure constants
(`LieAlgebraFactory.CreateSu2WithTracePairing` / `CreateSu3`): ad-matrices,
the embedded-su(2) j-Casimir `-sum_a (ad T_a)^2`, the hypercharge-squared
operator `-(ad lambda_8)^2`, simultaneous eigenspace decomposition; then
the gauge-boson mass pattern `M^2_{AB} = <[T_A, phi0],[T_B, phi0]>` over
(T1,T2,T3,lambda_8) for a VEV phi0 in the doublet block (non-physical
magnitude; the pattern is scale-invariant). Target-blind throughout: no
measured electroweak value appears anywhere.

## Results

- **The su(2) adjoint is a pure j = 1 triplet - no doublet block.** The
  existing su(2)-only control branch CANNOT realize the doublet route,
  which explains the Phase397 photon/Z underdetermination at the algebra
  level: the toy algebra is too small.
- **The su(3) adjoint branches as 3_0 + 1_0 + (2_{+Y0} + 2_{-Y0})** under
  the canonical su(2) x u(1) subalgebra (the conjugate doublet pair
  appears as one 4-real-dim j = 1/2 sector with |Y0| = sqrt(3)/2):
  doublet blocks DO exist inside larger adjoints - the
  gauge-Higgs-unification mechanism - so the GU route (Lie-algebra-valued
  scalar from a large algebra) is NOT structurally excluded.
- **A VEV in the doublet block produces the SM-shaped custodial pattern
  EXACTLY**: one massless neutral, T3-Y mixing present, degenerate charged
  pair, and the custodial identity
  m_charged^2 = m_neutral^2 cos^2(theta_emb) at residual 0.0, with the
  mixing angle READ OFF the same mass matrix.
- **Embedding-derived coupling ratio**: because all gauge fields share the
  single larger-algebra coupling, tan^2(theta_emb) is DERIVED from the
  embedding - for the su(3) toy it computes to 3 (recorded blind, no
  comparison to measurement). This is a theorem-level, target-independent
  MECHANISM for the hypercharge/coupling-ratio lineage row: in any
  candidate GU embedding chain the ratio is fixed by group theory, not
  fitted.

## Interpretation

The scalar-sector sub-gap (a) decomposes cleanly: the doublet-equivalent
substructure exists generically in larger-adjoint routes, with the
coupling ratio coming derived rather than input. What remains: the
GU-SPECIFIC embedding chain (spin(6,4)/su(3,2) -> SM) with ITS derived
ratio, the vacuum-manifold mechanism that selects the doublet-block VEV
(welds to the Upsilon = 0 locus geometry from Phase402), and the entire
quantitative chain. su(3) is a study toy, NOT GU's algebra.

## Status

Fail-closed. Nothing promoted; zero contract fields; no scales.

## Reproduce

```bash
dotnet run --project studies/phase403_adjoint_doublet_substructure_branching_probe_001/Phase403AdjointDoubletSubstructureBranchingProbe.csproj
```

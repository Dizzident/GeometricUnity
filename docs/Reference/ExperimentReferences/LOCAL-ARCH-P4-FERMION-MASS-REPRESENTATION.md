# LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION

## Source

- Ref ID: `LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION`
- Title: Phase 4 fermion inner-product and spectral architecture
- Path: `docs/Architecture/ARCH_P4.md`
- Source type: Local architecture workbench, not a primary GU source
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

The Phase 4 architecture defines a mesh-volume fermion mass matrix `M_psi`, the
weighted inner product `<x,y>_M=x^dagger M_psi y`, a generalized fermion
eigenproblem, and the branch-local coupling proxy
`<phi_i, (dD/domega)[b_k] phi_j>_M`.

The implemented discrete Dirac assembler persists an ordinarily Hermitian
matrix. Phase372 showed that directly interpreting that matrix as an
`M_psi`-self-adjoint weighted-space operator is inconsistent on the current toy
mesh. The local diagnosis is to distinguish:

- the Euclidean-Hermitian stiffness representation `K`;
- the weighted-space operator `A=M_psi^-1 K`;
- the Euclidean-Hermitian representative
  `B=M_psi^-1/2 K M_psi^-1/2`.

This distinction is a discrete implementation contract. It is not a derivation
of the physical fermionic branch of Geometric Unity.

## How It Was Used

- Used in Phase372 to compare the identity-weight reciprocal mixed-block control
  against the mesh-volume diagnostic branch.
- Used after Phase372 to interpret the nonzero mesh-volume adjoint residuals as
  a representation mismatch rather than immediate evidence that the discrete
  edge assembly must be rebuilt.
- Used to scope Phase373 as a phase-local representation-transform audit before
  any shared solver repair or physical weighted-pairing claim.
- Used in Phase374 to repair the shared weighted spectral path so it solves
  `B=M_psi^-1/2 K M_psi^-1/2`, transforms modes back with
  `psi=M_psi^-1/2 u`, and reports generalized `K psi=lambda M_psi psi`
  residuals. The persisted Phase12 replay selects kernel modes, so Phase374
  also requires an independent nonuniform-`M_psi` nonzero-spectrum benchmark.

## Prediction Relevance

A consistent `M_psi` representation is required before a branch-local
fermionic bilinear or reciprocal mixed block can be interpreted physically. It
is one prerequisite for the larger coupled-action program, not a W/Z/H
bridge-source law.

The architecture note does not supply a fixed GU fermionic action, explicit
Yukawa functional, solved coupling map, observed-field extraction, Higgs scalar
source row, target-independent mass scale, pole extraction, or GeV
normalization.

## Limitation

Treat this as a local implementation reference. The mesh-volume weights remain
a provisional discretization choice. Phase374 validates a bounded dense shared
solver repair at the discrete level, but does not establish that the weights
or operator branch are physically canonical. No physical boson mass can be
promoted from this convention alone.

## Follow-Up

- Preserve Phase374's nonzero-spectrum benchmark when replacing the bounded
  dense reference solver with a scalable implementation.
- Add explicit representation metadata before enabling mesh-volume weighting
  in production branches.
- Derive a fixed GU fermionic action and physical `M_psi` branch before using
  weighted reciprocal blocks as source-law evidence.

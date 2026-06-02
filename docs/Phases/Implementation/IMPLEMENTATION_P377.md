# Phase377 Selected Source-Mode Shell-Response Gram Audit

## Purpose

Phase377 composes the validated Phase376 shell-restricted perturbation blocks
into a target-blind Hilbert-Schmidt pullback metric on the persisted selected
source-mode subspace:

```text
G_a = Psi_shell^dagger deltaK[b_a] Psi_shell
Q_ab = Re Tr(G_a^dagger G_b)
c^T Q c = ||sum_a c_a G_a||_F^2 >= 0
```

## Source Boundary

- The official GU draft motivates a mixed deformation program but does not
  stabilize the physical coupled blocks.
- Alon and Cederbaum, `https://doi.org/10.1103/PhysRevB.68.033105`, show why an
  exactly degenerate perturbation problem requires diagonalizing the derivative
  inside the degenerate subspace. Phase377 uses an absolute-value spectral shell
  and does not claim an energy-slope theorem.
- Langmann, `https://arxiv.org/abs/math-ph/0104011`, treats a physical
  fermion-induced gauge action through a regularized Dirac determinant.
  Phase377 does not construct that effective action.

## Expected Boundary

The bounded audit must remain discrete-only. `Q` is a selected-source-mode
response precursor, not a full connection-carrier operator, GU action Hessian,
regularized fermion-determinant Hessian, gauge-reduced operator, W/Z mass
matrix, Higgs scalar-source row, or prediction.

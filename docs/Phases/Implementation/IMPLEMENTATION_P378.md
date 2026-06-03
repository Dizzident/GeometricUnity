# Phase378 Full Connection-Carrier Shell-Response Gram Audit

Phase378 extends Phase377 from the persisted selected source-mode subspace to
the full `156`-coordinate Phase12 connection-`1`-form carrier.

The audited discrete object is:

```text
G_i = Psi_shell^dagger deltaK[e_i] Psi_shell
Q_ij = Re Tr(G_i^dagger G_j)
```

where `e_i` is a coordinate basis vector in the fixed connection carrier and
`Psi_shell` is the Phase376 target-blind lowest nonzero four-mode shell.

Validation:

- Re-solves both persisted Phase12 weighted fermion shells.
- Builds all `156` analytic coordinate perturbation blocks per background.
- Checks the full-carrier response Gram is positive semidefinite and nonzero.
- Reconstructs each selected Phase376/377 source-mode block as
  `sum_i b_a[i] G_i`.
- Verifies the reconstructed selected Gram matches Phase377.

Observed result:

- Full-carrier response rank: `3` on both backgrounds.
- Full-carrier response nullity: `153` on both backgrounds.
- Max selected-block reconstruction residual:
  `9.168013570925023E-13`.
- Max selected Gram residual versus Phase377:
  `1.084828202293451E-14`.

Boundary:

Phase378 is a discrete shell-response precursor. It is not a GU action Hessian,
not a regularized fermion-determinant Hessian, not gauge reduced, not an
observed W/Z/H projection, not a Higgs scalar row, not GeV-normalized, and not
a boson prediction.

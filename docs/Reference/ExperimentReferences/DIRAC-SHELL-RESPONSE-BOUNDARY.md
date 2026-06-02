# DIRAC-SHELL-RESPONSE-BOUNDARY

## Sources

- Geometric Unity Author's Working Draft v1.0:
  https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf
- Ofir E. Alon and L. S. Cederbaum, *Hellmann-Feynman theorem at degeneracies*:
  https://doi.org/10.1103/PhysRevB.68.033105
- Edwin Langmann, *Generalized Yang-Mills actions from Dirac operator
  determinants*:
  https://arxiv.org/abs/math-ph/0104011
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

The GU draft motivates a mixed boson-fermion deformation program but does not
stabilize a physical coupled Hessian. Alon and Cederbaum show that derivative
information at an exact degeneracy is obtained by diagonalizing the derivative
operator within the degenerate subspace. Langmann treats a fermion-induced
gauge action as a regularized Dirac-determinant effective action.

Together these sources define a useful boundary for the local experiments:
shell-restricted Dirac perturbation blocks can support a discrete response
diagnostic, but they cannot be promoted into a physical effective-action
Hessian without the missing action, regularization, branch, and gauge
identities.

## How It Was Used

- Used in Phase377 to define the bounded response metric
  `Q_ab = Re Tr(G_a^dagger G_b)` from Phase376 projected shell blocks.
- Used to keep the Phase377 response metric distinct from a physical
  fermion-determinant effective-action Hessian.
- Used to record that the Phase376 absolute-value spectral shell is not an
  exact-degeneracy energy-slope theorem.

## Prediction Relevance

Phase377 can test whether selected connection directions induce a stable,
nonzero discrete shell response. These sources do not provide W/Z source rows,
Higgs scalar-source lineage, observed-field extraction, or GeV normalization.

## Limitation

The response metric is study-defined and selected-subspace scoped. It is not a
GU-native physical mass operator.

## Follow-Up

- Materialize the full connection-carrier response map before interpreting
  selected-subspace null directions.
- Supply a fixed GU fermionic action and gauge-compatible mixed blocks before
  attempting a physical effective-action Hessian.

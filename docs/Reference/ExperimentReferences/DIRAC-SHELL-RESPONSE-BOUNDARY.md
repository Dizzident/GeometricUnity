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
- Used in Phase378 to lift the same response construction to the full
  `156`-coordinate connection-`1`-form carrier and verify that Phase377 is its
  selected-source restriction.
- Used in Phase379 to characterize the positive rank-`3` response-image
  projector in the full carrier while preserving the distinction from a
  physical W/Z source law.
- Used to keep the Phase377 response metric distinct from a physical
  fermion-determinant effective-action Hessian.
- Used to record that the Phase376 absolute-value spectral shell is not an
  exact-degeneracy energy-slope theorem.

## Prediction Relevance

Phase377, Phase378, and Phase379 can test whether connection directions induce
a stable, nonzero discrete shell response and can characterize its carrier
image. Phase379 finds a strong two-axis carrier diagnostic, but these sources
do not provide W/Z source rows, Higgs scalar-source lineage, observed-field
extraction, or GeV normalization.

## Limitation

The response metric is study-defined and shell-scoped. Phase378 removes the
selected-subspace limitation for the current `156`-coordinate carrier, but it
is still not a GU-native physical mass operator. Phase379 characterizes the
carrier image, not a physical observed-field projection.

## Follow-Up

- Supply a fixed GU fermionic action and gauge-compatible mixed blocks before
  attempting a physical effective-action Hessian.
- Supply observed-field extraction, pole extraction, and unit normalization
  before any W/Z/H prediction attempt.

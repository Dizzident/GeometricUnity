# HEFT-SCALAR-MANIFOLD-GEOMETRY

## Sources

- A Geometric Formulation of Higgs Effective Field Theory: Measuring the
  Curvature of Scalar Field Space: https://arxiv.org/abs/1511.00724
- Unitarity Violation and the Geometry of Higgs EFTs:
  https://arxiv.org/abs/2108.03240
- Complete One-Loop Renormalization of the Higgs-Electroweak Chiral
  Lagrangian: https://arxiv.org/abs/1907.07605
- Source type: External physics source cluster
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

This source cluster gives a coordinate-invariant geometric formulation of the
electroweak scalar sector. HEFT treats the physical Higgs scalar and the three
Goldstone modes as coordinates on a scalar manifold. Observables are invariant
under field redefinitions and can be expressed in terms of scalar-manifold
geometry, including curvature, covariant derivatives, and the potential near
the physical vacuum.

The route is relevant because it states the mathematical shape of a successful
geometric W/Z/H bridge: W/Z masses arise from gauging broken directions of the
scalar geometry at a vacuum point, while the Higgs mass requires the
canonically normalized potential Hessian or equivalent scalar-source expansion
at the same point.

## How It Was Used

- Added Phase336 to test whether HEFT scalar-manifold geometry supplies a
  direct source law for the repository's W/Z/H prediction contracts.
- Used the sources to translate the blocker into geometric source data:
  scalar metric, vacuum point, gauged isometries or Killing vectors, gauge
  couplings, potential/Hessian, observed photon/W/Z/H projection, and physical
  unit normalization.
- Preserved HEFT as a geometric dependency template while preventing it from
  being treated as GU-local evidence.

## Prediction Relevance

HEFT geometry is useful because it shows that "geometric" is not enough by
itself. The needed artifact must provide the actual geometric objects at the
observed vacuum, not just a coordinate-invariant formalism. In this repository,
that means a GU-local scalar-manifold metric, a target-independent vacuum/VEV,
electroweak gauged directions, weak-angle and coupling normalization, a Higgs
potential Hessian or source operator, and observed-field extraction rows.

The HEFT route does not complete those requirements. It assumes or parametrizes
the electroweak symmetry-breaking pattern, gauge couplings, VEV scale, scalar
potential, and EFT validity scale. Those are dependency data, not GU-derived
W/Z/H predictions.

## Limitation

Treat as a bridge-template source, not as prediction evidence. It does not
derive GU-local W/Z source rows, a target-independent VEV, a weak-mixing angle,
low-energy gauge-coupling normalization, Higgs self-coupling lineage, or GeV
normalization.

## Follow-Up

- Search for or derive a GU-local scalar-manifold metric and vacuum point.
- Require gauged electroweak Killing directions and a neutral photon/Z rotation
  before any W/Z mass promotion.
- Require a GU potential Hessian or scalar-source operator at the same vacuum
  before any Higgs mass promotion.

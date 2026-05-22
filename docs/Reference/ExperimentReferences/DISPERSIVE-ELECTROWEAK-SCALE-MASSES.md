# DISPERSIVE-ELECTROWEAK-SCALE-MASSES

## Sources

- Dispersive determination of electroweak-scale masses:
  https://doi.org/10.1103/PhysRevD.108.054020
- ArXiv record for the same paper:
  https://arxiv.org/abs/2304.05921
- Inverse Problem Approach for Non-Perturbative QCD: Foundation:
  https://arxiv.org/abs/2211.13753
- Source type: External physics source cluster
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

This cluster tracks Hsiang-nan Li's dispersive route for electroweak-scale mass
estimates. The paper applies dispersion relations to bottom-quark scalar and
vector current correlators, using perturbative QCD input and a bottom-quark mass
input. It reports a Higgs mass near 114 GeV, a Z mass near 90.8 GeV, and a top
mass near 176 GeV, with the Z solution constrained by W/Z proportionality.

The inverse-problem foundation paper is included as method context because it
emphasizes that dispersion-relation inverse problems are ill-posed and require
regularization for stable approximate solutions.

## How It Was Used

- Added before implementing Phase347, a bounded dispersive electroweak-scale
  mass source audit.
- Compared against the source-lineage blocker matrix, observed-field extraction
  contract, mass-definition convention audit, alpha-running route, Standard
  Model mass-matrix dependency audit, Higgs/Upsilon scalar boundary, and
  Nielsen pole-mass boundary.

## Prediction Relevance

The route is relevant because it directly claims numerical electroweak-scale
masses from formal consistency rather than from a fitted Higgs potential. It is
therefore a real candidate to check against the W/Z/H blocker matrix.

It does not fill the contract. The route imports an external bottom-quark mass,
Standard Model/QCD current choices, perturbative inputs, and regularized
dispersion-relation machinery. It gives a Z estimate but not an independent W
source row, and its Higgs value is not the observed Higgs mass. It also does not
derive GU-local observed photon/W/Z/H operators, a GU Higgs scalar source, or
GeV unit lineage.

## Limitation

Treat this as an external numerical lead, not promotion evidence. A GU
prediction would need a native derivation of the relevant correlators, input
scale, W/Z/H observed-field projections, inverse-problem stability, and
target-independent mass rows before target comparison.

## Follow-Up

- Revisit only if a source derives the dispersive current correlators from
  GU-native fields and supplies a GU source for the bottom-mass/current input or
  a replacement dimensionful source.
- Require independent W and Z source rows plus observed-field extraction before
  using any dispersive estimate in promotion gates.

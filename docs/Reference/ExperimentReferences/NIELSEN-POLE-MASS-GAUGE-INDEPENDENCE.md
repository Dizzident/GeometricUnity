# NIELSEN-POLE-MASS-GAUGE-INDEPENDENCE

## Sources

- Nielsen identities of the SM and the definition of mass:
  https://doi.org/10.1103/PhysRevD.62.076002
- Width and Partial Widths of Unstable Particles in the Light of the Nielsen
  Identities: https://doi.org/10.1103/PhysRevD.65.085001
- ArXiv record for the same unstable-particle Nielsen-identity analysis:
  https://arxiv.org/abs/hep-ph/0109228
- The complex-mass scheme and unitarity in perturbative quantum field theory:
  https://doi.org/10.1140/epjc/s10052-015-3579-2
- Gauge Invariance and Unstable Particles:
  https://doi.org/10.1103/PhysRevLett.75.3060
- Practical Algebraic Renormalization:
  https://doi.org/10.1006/aphy.2001.6117
- Source type: External physics source cluster
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

This cluster tracks the gauge-independence boundary for physical masses and
resonance extraction. Nielsen identities describe how gauge-fixed Green
functions vary with gauge parameters. In the Standard Model they support an
all-orders proof that complex-pole masses of physical fields are gauge
independent, including cases with mixing. Related work applies Nielsen
identities to unstable-particle residues and partial widths, and the
complex-mass and pinch-technique literature gives practical frameworks for
gauge-invariant resonance calculations.

The practical lesson for the W/Z/H project is sharp: physical W, Z, and Higgs
mass claims must be tied to gauge-invariant pole or resonance data, not to
gauge-fixed intermediate fields or convention-dependent mass parameters.

## How It Was Used

- Added before implementing Phase346, a bounded Nielsen/complex-pole
  gauge-independence source audit.
- Compared against the mass-definition convention audit, FMS observed-spectrum
  audit, Fradkin-Shenker complementarity audit, observed-field extraction
  contract, and source-lineage blocker matrix.

## Prediction Relevance

The route is relevant because it states what a defensible observed physical
mass extractor must prove: gauge-parameter independence of the pole location,
stable treatment of mixing, and a physical resonance definition.

It still does not fill the contract. These sources do not derive GU-local
BRST/Slavnov-Taylor identities, observed W/Z/H operators, correlation matrices,
pole equations, mass-scale/coupling lineage, the Higgs scalar-source operator,
or GeV normalization.

## Limitation

Treat this as an observed-mass extraction boundary, not prediction evidence.
Gauge-independent pole definitions can certify a mass once a theory supplies
the two-point functions and parameters; they do not supply the GU source law or
the numerical W/Z/H values.

## Follow-Up

- Search for a GU-local BRST or equivalent identity that controls gauge
  dependence of observed-sector correlation functions.
- Revisit only if a source derives photon/W/Z/H pole equations and residues
  from GU-native operators together with target-independent scale/coupling and
  unit lineage.

# FRADKIN-SHENKER-COMPLEMENTARITY

## Sources

- Phase diagrams of lattice gauge theories with Higgs fields:
  https://doi.org/10.1103/PhysRevD.19.3682
- Impossibility of spontaneously breaking local symmetries:
  https://doi.org/10.1103/PhysRevD.12.3978
- Gauge field theories on a lattice:
  https://doi.org/10.1016/0003-4916(78)90039-8
- Phase diagram of the lattice SU(2) Higgs model:
  https://arxiv.org/abs/0911.1721
- A confinement criterion for gauge theories with matter fields:
  https://arxiv.org/abs/1708.08979
- The Higgs phase as a spin glass, and the transition between varieties of
  confinement: https://arxiv.org/abs/2001.03068
- Gauge-independent transition separating confinement and Higgs phases in
  lattice SU(2) gauge theory with a scalar field in the fundamental
  representation: https://arxiv.org/abs/2308.13430
- Source type: External physics source cluster
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

This cluster tracks the gauge-Higgs complementarity boundary around the Higgs
mechanism. Elitzur's theorem blocks treating spontaneous breaking of a local
gauge symmetry as a gauge-invariant physical order parameter. The
Fradkin-Shenker and Osterwalder-Seiler line shows that, for fundamental Higgs
fields in lattice gauge-Higgs theories, confinement and Higgs regions can be
analytically connected rather than sharply separated by a thermodynamic phase
boundary.

Modern lattice work adds gauge-invariant diagnostics for varieties of
confinement, custodial/global symmetry behavior, and possible gauge-independent
transitions. Those diagnostics are useful because they indicate what kind of
operator-level evidence a physical electroweak source law must use. They also
reinforce the negative boundary: phase labels and gauge-fixed Higgs-language
cannot by themselves supply W/Z/H source-lineage rows.

## How It Was Used

- Added before implementing Phase345, a bounded Fradkin-Shenker
  complementarity source audit.
- Compared against the FMS observed-spectrum audit, observed-field extraction
  contract, observed-field candidate scan, and source-lineage blocker matrix.

## Prediction Relevance

The route is relevant because the missing W/Z/H bridge-source law must not rely
on a gauge-fixed local symmetry-breaking story as if it were a physical
observable. Complementarity says that a viable GU route needs gauge-invariant
operators, a physical observed-sector Hilbert space, and correlation or spectral
pole extraction for photon, W, Z, and Higgs states.

It still does not fill the contract. These sources do not derive GU-local
operator maps, the electroweak VEV or equivalent target-independent scale,
weak-mixing and gauge-coupling normalization, the Higgs scalar-source operator,
or GeV units.

## Limitation

Treat this as a source-law boundary and observed-field extraction constraint,
not promotion-ready evidence. Gauge-invariant phase diagnostics can help state
the problem correctly, but they do not predict the W, Z, or Higgs masses unless
connected to GU-local source data.

## Follow-Up

- Search for GU-native gauge-invariant order parameters or composite operators
  that can be shown to have W/Z/H poles.
- Revisit only if a source derives the observed electroweak sector, physical
  pole extraction, scale/coupling lineage, and unit normalization without using
  observed W/Z/H target values as inputs.

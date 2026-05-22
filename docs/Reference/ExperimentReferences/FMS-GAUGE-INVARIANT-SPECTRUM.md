# FMS-GAUGE-INVARIANT-SPECTRUM

## Sources

- Higgs phenomenon without symmetry breaking order parameter:
  https://doi.org/10.1016/0550-3213(81)90448-X
- The Froehlich-Morchio-Strocchi mechanism: An underestimated legacy:
  https://arxiv.org/abs/2305.01960
- On the observable spectrum of theories with a Brout-Englert-Higgs effect:
  https://arxiv.org/abs/1709.07477
- A study of how the particle spectra of SU(N) gauge theories with a
  fundamental Higgs emerge: https://arxiv.org/abs/1710.01941
- Gauge invariance and the physical spectrum in the two-Higgs-doublet model:
  https://arxiv.org/abs/1601.02006
- The Froehlich-Morchio-Strocchi mechanism and quantum gravity:
  https://arxiv.org/abs/1908.02140
- Source type: External physics source cluster
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

This cluster tracks the FMS mechanism: the physical spectrum of a gauge theory
is described by gauge-invariant composite operators, and in the Standard Model
regime those composite states can map to the familiar gauge-fixed elementary
Higgs, W, Z, and fermion fields. The route directly addresses a core blocker in
the repository: observed-field extraction.

FMS is especially relevant because it offers a disciplined way to avoid
treating local gauge symmetry breaking as an observable fact. It says how to
build physical particle operators and, under Standard Model-like assumptions,
why the usual elementary perturbative particles can still be a good description
of observed states.

## How It Was Used

- Added before implementing Phase344, a bounded FMS gauge-invariant spectrum
  source audit.
- Compared against the existing observed-field extraction contract, observation
  pipeline capability audit, observed-field candidate scan, coupled
  Yang-Mills-Higgs extraction audit, and Higgsless boundary-condition audit.

## Prediction Relevance

This route is one of the best external templates for filling the observed-field
extraction side of the W/Z/H problem. It can explain how physical W/Z/H states
should be represented by gauge-invariant operators and how those operators
reduce to ordinary elementary fields in a suitable expansion.

It still does not fill the contract. The repository has no GU-local FMS-like
theorem defining physical photon/W/Z/H composite operators from native fields,
no source-derived observed-sector vacuum and expansion rule, no correlation
function or spectral-pole extraction rows, no target-independent mass scale or
coupling lineage, and no GeV normalization.

## Limitation

Treat this as an observed-field extraction template, not promotion-ready
evidence. FMS can say what kind of physical operator map is needed; it does not
derive the GU map or the W/Z/H mass values by itself.

## Follow-Up

- Search specifically for GU-native gauge-invariant composite operators whose
  poles can be identified with photon, W, Z, and Higgs states.
- Revisit only if a source derives the GU observed-sector vacuum, expansion,
  W/Z/H projection rows, and physical-unit normalization without importing
  Standard Model target masses.

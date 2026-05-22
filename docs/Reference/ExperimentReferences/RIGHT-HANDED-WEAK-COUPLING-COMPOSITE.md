# RIGHT-HANDED-WEAK-COUPLING-COMPOSITE

## Sources

- W boson mass tension caused by its right-handed gauge coupling at high
  energies:
  https://doi.org/10.1016/j.nuclphysb.2022.115992
- ArXiv record for the same paper:
  https://arxiv.org/abs/2205.14957
- Vectorlike W-boson coupling at TeV and third family fermion masses:
  https://arxiv.org/abs/1506.05994
- Source type: External physics source cluster
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

This cluster tracks She-Sheng Xue's right-handed weak-coupling route. The 2022
paper proposes that SM-gauge-symmetric four-fermion interactions induce
right-handed W and Z couplings near a TeV-scale parity-restoration/composite
transition. It writes W and Z mass corrections using coefficients `c_w` and
`c_z` and a transition scale around 5.1 TeV.

The route is tied to top-condensate/composite-Higgs dynamics. It uses measured
top and Higgs masses, the Fermi-constant electroweak VEV, Standard Model
high-order mass baselines, a `c_w` range fitted to the W-mass anomaly, and a
`c_z` bound constrained by the observed Z mass. The earlier 2015 paper provides
background for vectorlike W coupling at TeV scales and third-family fermion
masses.

## How It Was Used

- Added before implementing Phase348, a bounded right-handed weak-coupling
  source audit.
- Compared against the source-lineage blocker matrix, observed-field extraction
  contract, top-condensation audit, Standard Model mass-matrix dependency
  audit, Higgs/Upsilon scalar boundary, Nielsen pole-mass boundary, and
  dispersive electroweak-scale mass audit.

## Prediction Relevance

The route is relevant because it directly addresses W and Z boson masses rather
than only Higgs-sector structure. It supplies a concrete correction template:
right-handed W/Z vertex functions alter the running W and Z mass-shell
relations.

It does not fill the GU prediction contract. The route imports an external
four-fermion operator, critical-coupling/gap-equation dynamics, RG transport,
measured top/Higgs/VEV inputs, SM high-order baselines, and fitted or
constrained `c_w`/`c_z` values. It does not derive GU-local observed photon,
W, Z, or Higgs operators, independent W/Z source rows, a GU Higgs scalar source,
or GeV unit lineage.

## Limitation

Treat this as an external W/Z correction lead, not promotion evidence. It is not
target-independent because the coefficient range and constraints are tied to
measured W/Z data, and it is not GU-local because the source interaction and
transition scale are imported from the external composite/top-condensation
model.

## Follow-Up

- Revisit only if a source derives the four-fermion interaction or equivalent
  right-handed W/Z vertex functions from GU-native fields.
- Require target-independent `c_w` and `c_z` coefficients, an independent
  transition-scale derivation, observed-field extraction, a Higgs scalar-source
  row, and GeV normalization before promotion.

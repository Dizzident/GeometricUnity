# MACDOWELL-MANSOURI-CARTAN-BREAKING

## Sources

- The gravitational and electroweak interactions unified as a gauge theory of
  the de Sitter group: https://arxiv.org/abs/hep-th/9605217
- MacDowell-Mansouri Gravity and Cartan Geometry:
  https://arxiv.org/abs/gr-qc/0611154
- Gravity and electroweak sector from symmetry breaking of an SO(3,3) BF
  theory: https://arxiv.org/abs/2602.19151
- Source type: External physics source cluster
- Reference index: [ExperimentReferences.md](../../../ExperimentReferences.md)

## Summary

This cluster tracks the MacDowell-Mansouri / Stelle-West / Cartan-geometry
route after the metric-affine torsion audit. The common idea is to replace an
ordinary Yang-Mills-style connection with a larger gauge or Cartan connection,
then recover gravity, weak-isospin structure, or both through a symmetry
breaking or projection.

The Cartan-geometry source gives the clean gravitational baseline. It explains
why the MacDowell-Mansouri connection can combine a Lorentz connection and a
coframe into a single de Sitter or anti-de Sitter Cartan connection. The
MacDowell-Mansouri action is then a broken-symmetry rewriting of Palatini
gravity, with a BF reformulation available. This is important geometric
infrastructure, but by itself it is a gravity construction rather than a W/Z/H
mass-source law.

The de Sitter electroweak paper is the most direct W/Z lead in this cluster. It
claims that a complexified de Sitter gauge theory can recover the electroweak
gauge-boson spectrum, with mass terms arising from the geometric breaking data
rather than from a conventional Higgs sector. The paper introduces a mass matrix
depending on the breaking scale, orientation data, and weak angle. It then
recovers the standard electroweak spectrum after choosing parameters such as
rho and trading the scale for an observed W or Z mass.

The SO(3,3) BF source is a newer higher-gauge / BF-style route. It uses a
MacDowell-Mansouri-type symmetry reduction to obtain chiral sectors, then
embeds an electroweak branch. Its electroweak W mass relation uses the
conventional Higgs VEV after the SU(2)L x U(1)Y Higgs mechanism is introduced.
The same source explicitly labels its proposed hierarchy relation as a
speculative phenomenological ansatz rather than a derived result.

## How It Was Used

- Added to the reference tracker before implementing a bounded
  MacDowell-Mansouri / Cartan-breaking source audit.
- Used as the next non-duplicative geometric route after the metric-affine
  torsion source audit preserved a serious but external electroweak
  symmetry-breaking lead.
- Compared against the repository's W/Z and Higgs source-lineage contracts to
  test whether Cartan/de Sitter/BF breaking can supply target-independent
  source rows rather than replaying Standard Model inputs.

## Prediction Relevance

This route is relevant because it is genuinely geometric: the electroweak lead
tries to derive mass assignments from de Sitter breaking data, while the BF
lead puts gravity and weak-isospin structure into a common symmetry-breaking
framework. It is therefore a plausible place to look for a direct bridge-source
law.

It does not yet complete the prediction contract. The de Sitter electroweak
route still depends on free breaking data and can trade its scale for an
observed W or Z mass, which makes it unsuitable as a target-independent
prediction. It also predicts no conventional Higgs sector, which conflicts with
the observed physical Higgs. The SO(3,3) BF route recovers an electroweak branch
through the standard Higgs mechanism and still needs the VEV, weak coupling,
mixing, physical-field projection, and unit normalization as inputs or later
matching data.

## Limitation

Treat this as an external geometric gauge-breaking lead, not promotion-ready
W/Z/H evidence. The current sources do not provide a GU-local theorem mapping
GU fields to the MacDowell-Mansouri or de Sitter breaking data, do not extract
observed photon/W/Z/H fields from GU, do not derive the weak angle or VEV from a
target-independent source, and do not supply a Higgs scalar-source operator
compatible with the observed 125 GeV Higgs.

## Follow-Up

- Implement a fail-closed audit for this route and require it to state whether
  any Phase201 W/Z or Higgs contract field is actually supplied.
- Search specifically for a GU-local Cartan/de Sitter bridge theorem that maps
  the draft's field content, observer projection, and Shiab/operator data to
  physical photon/W/Z/H rows.
- Revisit only if a source supplies target-independent breaking-scale,
  weak-angle, VEV, neutral-mixing, observed-field projection, Higgs-source, and
  GeV-normalization lineage without using measured W/Z/H masses as inputs.

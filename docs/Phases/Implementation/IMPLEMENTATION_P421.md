# Implementation P421 - Cox GU IV v2 LCDM Rig Boson Contract Audit

Phase421 adds
`studies/phase421_cox_gu_iv_v2_lcdm_rig_boson_contract_audit_001`, a
source-level full-text audit for the restart prompt's GU IV v2 follow-up.

## What It Checks

- Uses Zenodo record `17402261`, DOI `10.5281/zenodo.17402261`.
- Records the exact PDF artifact identity:
  `GUT.4.1.pdf`, `702258` bytes,
  `md5:1d51f99a44cf51c8023dbc500e58ed3c`.
- Records the full-text extraction method and line count:
  `pdftotext`, `3305` lines.
- Verifies that the source scope is a LambdaCDM/cosmology testing rig:
  BRST/BV guardrails, projector-variation commutation, Etherington
  reciprocity, boundaries, corridor checks, and seven data hooks.
- Checks that contract-critical electroweak terms are absent from the extracted
  text: electroweak, weak mixing, weak angle, Weinberg, hypercharge, Higgs, W
  boson, Z boson, Standard Model, GeV, VEV, pole, Dirac, and Yang-Mills.
- Classifies positive words such as photon, mass, curvature, and observed as
  cosmology/optics/corridor usage, not electroweak observed-field rows.
- Preserves the Phase419 observed-field template boundary and Phase420 scale
  boundary.

## Scientific Boundary

The phase reports:

- `coxGuIvV2LcdmRigBosonContractAuditPassed=True`
- `lcdmRigScopeConfirmed=True`
- `sourceProvidesBosonContractEvidence=False`
- `sourceProvidesObservedElectroweakNamespaceMap=False`
- `sourceProvidesPhotonWzHProjectionRows=False`
- `sourceProvidesWzSourceRows=False`
- `sourceProvidesHiggsScalarSourceRow=False`
- `sourceProvidesElectroweakVevMap=False`
- `sourceProvidesWeakAngleOrCouplingLineage=False`
- `sourceProvidesCurvatureToElectroweakScaleLaw=False`
- `sourceProvidesPoleExtraction=False`
- `sourceProvidesGeVUnitNormalization=False`
- `canFillPhase201WzContract=False`
- `canFillPhase201HiggsContract=False`
- `canFillPhase256ObservedFieldExtractionContract=False`
- `routePromotesWzMasses=False`
- `routePromotesHiggsMass=False`

GU IV v2 is useful as a reproducible cosmology guardrail/hook source. It does
not supply the theorem-level scalar sector, observed electroweak namespace map,
curvature-to-electroweak scale law, or pole/GeV lineage required for W/Z/H
prediction promotion.

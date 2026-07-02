# Implementation P427 - Hofseth GU-RVG Superluminal Source Audit

Phase427 adds `studies/phase427_hofseth_gu_rvg_superluminal_source_audit_001`,
the source audit for the Hofseth GU-RVG "Superluminal Metric Engineering"
lead named by the 2026-07-01 literature sweep.

## What It Checks

- Records the deletion lineage: the originally-catalogued record
  `10.5281/zenodo.21056575` was tombstoned on 2026-07-01T22:51:40Z
  (reason "duplicate", grace-period-v1 policy) hours after the sweep
  catalogued it; the live successor `10.5281/zenodo.21117379` carries the
  same title and claimed authorship (published 2026-06-18).
- Records the live artifact identity: PDF `3084101` bytes,
  `md5:90be901bc227bc90e493c295aa276046`, pdftotext `6465` lines.
- Records the externally-unverified "Weinstein, Eric R. (Harvard
  University)" co-authorship attribution, matching the fabricated-
  attribution pattern documented in arXiv:2606.02184.
- Classifies the full-text electroweak content as IMPORTED: the paper
  uses `v = 246 GeV` as an explicit input (eqs. 5-6) to derive a 27.2 TeV
  dilaton decay constant, treats the 95.4 GeV diphoton excess as an
  external collider signal, and marks its condensate amplitude as "fixed
  by observation rather than computation" in its own derived/input/open
  table.
- Confirms W boson, Z boson, weak mixing, Weinberg angle, hypercharge,
  pole mass, and vector-spinor projection terms are absent.

## Scientific Boundary

- `sourceUsesExternalElectroweakVev246Gev=True` (imported, not derived)
- `sourceProvidesWzSourceRows=False`
- `sourceProvidesHiggsScalarSourceRow=False`
- `sourceProvidesWeakAngleOrCouplingLineage=False`
- `sourceProvidesPoleExtraction=False`
- `sourceProvidesGeVUnitNormalization=False`
- `canFillPhase201WzContract=False`,
  `canFillPhase201HiggsContract=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`
- `routePromotesWzMasses=False`, `routePromotesHiggsMass=False`

Both NEW-LEAD items from the 2026-07-01 sweep are now discharged
(Phase426: Cox GU I-V series; Phase427: this record). The negative
boundary stands.

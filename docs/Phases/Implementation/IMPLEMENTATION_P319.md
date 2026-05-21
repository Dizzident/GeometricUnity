# Phase319 Legacy Selector Spectrum Source-Law Audit

Phase319 audits whether the older Phase42/43/73 selector-spectrum route can be
promoted as the missing W/Z direct target-independent bridge-source law.

The result is negative. Phase42/43 are useful history: they materialized
bundle-backed and selector-eigen W/Z spectra and moved from an invariant ratio
problem to calibration and target comparison. Phase73 then emitted absolute W/Z
mass projections. But Phase74 showed those projections fail physical W/Z target
comparison, Phase75/76 diagnosed a coherent weak-coupling normalization miss,
and Phase80 still lacks production analytic replay inputs.

Key result:

- `legacySelectorSpectrumSourceLawAuditPassed=true`
- `legacySelectorSpectrumSourceLawFound=false`
- `legacySelectorRoutePromotableForBosonMasses=false`
- `legacySelectorRouteCanFillPhase201WzContract=false`
- `legacySelectorRouteCanFillPhase201HiggsContract=false`
- `legacySelectorRouteCanFillPhase256ObservedFieldExtractionContract=false`
- `legacySelectorRouteCompletesBosonPredictions=false`

The current Phase201/P252/P313 gates remain decisive: the legacy selector route
does not supply current source-lineage fields, a target-independent
normalization theorem, a physical photon/Z/W projection map, or observed
electroweak embedding.

Next required work remains source-level:

- derive target-independent W/Z source rows and normalization before target
  comparison;
- derive observed electroweak projection, including photon/Z rotation and W
  charged rows;
- derive Higgs scalar-source/self-coupling lineage separately.

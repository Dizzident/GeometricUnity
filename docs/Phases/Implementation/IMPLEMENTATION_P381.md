# Implementation P381 - Phase302/307 Response-Image Selector Compatibility Audit

Phase381 compares the strongest Phase302/307 W/Z numerical near-pass route
against the Phase379 response-image sidecar.

Inputs:

- Phase302 identity-split particle normalization audit.
- Phase307 target-independent decoupled W/Z row selector.
- Phase308, Phase309, and Phase310 normalization blocker audits.
- Phase379 response-image carrier-axis characterization.
- Phase380 response-image W/Z contract application audit.

Decision rule:

- Treat Phase379 only as a diagnostic sidecar, not source evidence.
- Materialize which carrier gauge axes the Phase307 selected W and Z rows use.
- Record a compatibility boundary when the selected W row requires the
  Phase379-suppressed gauge axis or when the Phase379 dominant axes do not
  match the Phase307 charged-ladder axis pair.
- Preserve `canFillPhase201WzContract=false` unless a theorem-backed W/Z
  source law, observed electroweak field map, separate source rows, raw/common
  gates, and GeV normalization are present.

Boundary:

- This audit does not construct a mass prediction.
- It must not use physical targets for construction.
- It must not mutate the Phase201 template.

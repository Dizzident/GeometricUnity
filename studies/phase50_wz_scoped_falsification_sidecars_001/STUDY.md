# Phase L - W/Z-Scoped Falsification Sidecars

## Purpose

This study provides W/Z-specific falsification sidecar inputs for a W/Z-only
physical campaign. It keeps the selected Phase XLVI W/Z physical prediction
artifacts, but replaces the broad Phase V reference sidecars with sidecars
scoped to the selected W and Z mass-mode records:

- `phase22-phase12-candidate-0`
- `phase22-phase12-candidate-2`

## Scope

The sidecars in this study are target-scoped to the physical W/Z mass-ratio
campaign. They do not import the Phase IV toy fermion registry sidecars and do
not evaluate `fermion-registry-phase4-toy-v1-0000`.

## Artifacts

- `config/campaign.json`: W/Z-only campaign input wiring that references the
  existing Phase XLVI W/Z physical artifacts and the Phase L scoped sidecars.
- `config/sidecar_summary.json`: sidecar coverage summary for the scoped
  W/Z falsification track.
- `wz_scoped_registry.json`: minimal registry containing only the selected
  W and Z boson candidates.
- `observation_chain.json`: observation-chain sidecar records for the W/Z
  physical ratio extraction.
- `environment_variance.json`: W/Z physical ratio environment-sensitivity
  sidecar record.
- `representation_content.json`: representation-content checks for the two
  selected W/Z modes.
- `coupling_consistency.json`: W/Z identity-coupling sidecar records for the
  selected modes.
- `sidecar_scope.json`: explicit inclusion and exclusion scope for this
  target-scoped sidecar track.

## Interpretation

The Phase IV toy fermion representation-content blocker is out of scope for
this W/Z-only campaign because it targets `fermion-registry-phase4-toy-v1-0000`,
not `physical-w-z-mass-ratio`, `phase22-phase12-candidate-0`, or
`phase22-phase12-candidate-2`. It remains a valid blocker for campaigns that
claim the Phase IV toy fermion registry or unrestricted global registry health.

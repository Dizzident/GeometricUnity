# Phase327 Oblique Precision Electroweak Source Audit

Phase327 audits the precision-electroweak oblique-parameter route. The
Peskin-Takeuchi S, T, and U parameters are real and useful: they summarize
new-physics contributions to electroweak gauge-boson vacuum-polarization
corrections and are constrained by W/Z precision data.

The audit keeps those fit constraints separate from boson-mass promotion.
Oblique parameters can constrain loop corrections, custodial breaking, and
new-physics sectors, but they do not by themselves derive a GU-local
electroweak VEV, low-energy weak-mixing angle, gauge-coupling normalization,
W/Z absolute scale, observed photon/W/Z/H projection rows, or Higgs scalar
self-coupling/source lineage.

## Outputs

- `studies/phase327_oblique_precision_electroweak_source_audit_001/output/oblique_precision_electroweak_source_audit.json`
- `studies/phase327_oblique_precision_electroweak_source_audit_001/output/oblique_precision_electroweak_source_audit_summary.json`

## Decision

Phase327 is expected to pass only as a negative boundary audit. Precision
oblique parameters are fit/constraint variables for radiative corrections, not
exact target-independent GU source rows for W, Z, or Higgs masses.

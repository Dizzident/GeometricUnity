# Implementation P339: MacDowell-Mansouri Cartan-Breaking Source Audit

## Purpose

Phase339 records the MacDowell-Mansouri / Stelle-West / Cartan-breaking route
as a bounded source audit. It checks whether Cartan/de Sitter/BF symmetry
breaking supplies a GU-local, target-independent W/Z/H bridge-source law, or
only an external geometric gauge-breaking lead.

## Sources

- `https://arxiv.org/abs/hep-th/9605217`
- `https://arxiv.org/abs/gr-qc/0611154`
- `https://arxiv.org/abs/2602.19151`

## Result Contract

The phase must pass only when it preserves the non-promotional boundary:

- The route is recorded as serious geometric gauge-breaking evidence.
- The de Sitter electroweak lead is recorded as depending on free breaking
  scale, orientation, weak-angle, and rho data, with the scale tradeable for
  observed W or Z mass.
- The no-conventional-Higgs claim is recorded as incompatible with the
  observed physical Higgs.
- The SO(3,3) BF branch is recorded as using the conventional Higgs mechanism
  with VEV and weak-coupling inputs.
- All GU source-lineage, observed-field extraction, Higgs-source,
  physical-unit, and promotion flags remain false.

## Outputs

- `studies/phase339_macdowell_mansouri_cartan_breaking_source_audit_001/output/macdowell_mansouri_cartan_breaking_source_audit.json`
- `studies/phase339_macdowell_mansouri_cartan_breaking_source_audit_001/output/macdowell_mansouri_cartan_breaking_source_audit_summary.json`

## Decision

Do not promote physical W/Z/H masses from this route. It remains useful because
it is a direct geometric symmetry-breaking lead, but the current sources either
trade the electroweak scale for observed W/Z masses, remove the Higgs sector, or
recover the standard Higgs mechanism with VEV and coupling inputs. The missing
artifact remains a GU-local theorem and source-lineage chain for the observed
photon, W, Z, and Higgs fields.

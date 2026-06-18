# Phase421 Cox GU IV v2 LCDM Rig Boson Contract Audit

## Purpose

Phase421 audits the source-level follow-up named in the restart prompt:
Zenodo DOI `10.5281/zenodo.17402261`, Cox's `Geometric Unity IV (v2): The
Testing Rig for LambdaCDM`. The phase checks whether the full text supplies any
of the missing boson-prediction contract fields.

## Result

The study is target-blind and fail-closed. It records the Zenodo artifact and
full-text extraction metadata, then preserves the boundary that this source is a
cosmology rig, not an electroweak W/Z/H source-law paper.

- The PDF is identified as `GUT.4.1.pdf`, size `702258`, checksum
  `md5:1d51f99a44cf51c8023dbc500e58ed3c`.
- `pdftotext` extraction produced `3305` lines.
- Positive content is BRST/BV projection-variation, admissible boundaries,
  Etherington reciprocity, and seven cosmology hooks.
- Contract-critical terms are absent: electroweak, weak mixing, weak angle,
  Weinberg, hypercharge, Higgs, W boson, Z boson, Standard Model, GeV, VEV,
  pole, Dirac, and Yang-Mills.
- Existing positive hits such as photon, mass, curvature, and observed occur
  only in cosmology/optics/corridor contexts and are not W/Z/H observed-field
  rows.

No Phase201 or Phase256 field is filled, and no W/Z/H mass claim is promoted.

## Output

The study writes:

- `output/cox_gu_iv_v2_lcdm_rig_boson_contract_audit.json`
- `output/cox_gu_iv_v2_lcdm_rig_boson_contract_audit_summary.json`

Terminal status:
`cox-gu-iv-v2-lcdm-rig-boson-contract-audit-no-electroweak-source`.

# IMPLEMENTATION_P402: GU-Draft Scalar-Route Dictionary Audit

## Scope

Audits the TOE-GU-ICEBERG-20250423 scalar-route ansatz against the
PRIMARY 2021 GU draft (text now stored in-repo, PDF hash pinned),
machine-checking the three catalogued items: representation assignment,
normalization claim, negative-mass-squared origin.

## Artifacts

- Study: `studies/phase402_gu_draft_scalar_route_dictionary_audit_001`
- Project: `Phase402GuDraftScalarRouteDictionaryAudit.csproj`
- Outputs: `output/gu_draft_scalar_route_dictionary_audit.json` and
  `..._summary.json`
- New stored primary text:
  `docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt`
  (pdftotext extraction; PDF SHA256
  `3f28d742234a9841fc8e51ff172053200aa3eddf3ece38154a3328b9ebd186d4`)

## Method and results

| Check | Result |
| --- | --- |
| Eq. 12.28 dictionary rows (Higgs potential/KG, Yukawa-as-VEV, hypercharge component) | all present in primary text (11/11 evidence items) |
| Higgs potential = `<U, M U>` correspondence at backgrounds | exact (residual 0.0) |
| Backgrounds on Upsilon ~ 0 vacuum manifold | 5.0e-10 / 1.8e-9 |
| Discrete D^* Upsilon at persisted solve scale | 8.5e-10 / 3.1e-9 (Phase401 polish: 2.7e-17) |
| Adjoint-triplet VEV reproduces SM neutral pattern | FALSE (2 massless neutrals, no W3-B mixing) |
| Doublet VEV reproduces SM neutral pattern | TRUE (custodial identity to 1e-16) |
| "GeV"/"246" occurrences in primary | 0 / 0 |
| "doublet" applied to Higgs in primary | never (fermionic only) |

Structural conclusions: (1) the repo's Mode-B objective IS the
draft-claimed Higgs potential and the Phase393-401 program has been
probing its vacuum manifold; (2) symmetry breaking in the primary must
come from the Upsilon = 0 locus geometry, not a negative mass term;
(3) the binding scalar-sector gap is sharpened to exhibiting a
doublet-equivalent substructure inside the pulled-back connection
component - which the primary does not provide - plus the entirely
absent quantitative chain.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`guDraftScalarRouteDictionaryAudit` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `gu-draft-scalar-route-dictionary-audit-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase402 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296 (plus the new stored text directory)

## Validation

- Targeted Phase402 run passes (11/11 text evidence, correspondence
  verified, discriminator computed).
- Phase101, Phase202 (checklist 194 -> 195 passed), claim-integrity
  verifier re-run with Phase402 included; objective remains incomplete by
  design.

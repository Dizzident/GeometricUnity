# Implementation P347: Dispersive Electroweak-Scale Mass Source Audit

## Purpose

Phase347 records Li's dispersive electroweak-scale mass route as a bounded
source-lineage audit. It checks whether bottom-current dispersion relations and
inverse-problem machinery can fill the missing W/Z/H prediction contracts, or
only provide an external Standard Model/QCD numerical lead.

## Sources

- `https://doi.org/10.1103/PhysRevD.108.054020`
- `https://arxiv.org/abs/2304.05921`
- `https://arxiv.org/abs/2211.13753`

## Result Contract

The phase must pass only when it preserves the non-promotional boundary:

- The reported dispersive Higgs, Z, and top mass estimates are recorded.
- The W row remains only proportionally constrained, not independently derived.
- External bottom-mass, Standard Model/QCD current, perturbative-input, and
  inverse-problem dependencies are explicit.
- Phase201 and Phase256 blockers remain unfilled.
- All GU-local source-law, observed-field extraction, W/Z/H mass-promotion, and
  completion flags remain false.

## Outputs

- `studies/phase347_dispersive_electroweak_scale_mass_source_audit_001/output/dispersive_electroweak_scale_mass_source_audit.json`
- `studies/phase347_dispersive_electroweak_scale_mass_source_audit_001/output/dispersive_electroweak_scale_mass_source_audit_summary.json`

## Decision

Do not promote W/Z or Higgs masses from the dispersive electroweak-scale mass
route. It is a direct external numerical lead, but it imports the bottom-quark
mass, Standard Model/QCD current correlators, perturbative inputs, and a
regularized inverse-problem solution; reports a Higgs value around 114 GeV; and
does not supply independent W/Z source rows, GU observed-field extraction,
Higgs scalar-source lineage, or GeV-unit normalization.

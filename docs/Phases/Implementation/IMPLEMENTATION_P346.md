# Implementation P346: Nielsen Pole-Mass Gauge-Independence Source Audit

## Purpose

Phase346 records Nielsen-identity, complex-pole, complex-mass-scheme, and
pinch-technique sources as a bounded observed-mass extraction audit. It checks
whether gauge-independent pole-mass machinery can fill the missing W/Z/H
source-law or observed-field extraction contracts, or only states the physical
mass convention that a GU-local source law must satisfy.

## Sources

- `https://doi.org/10.1103/PhysRevD.62.076002`
- `https://doi.org/10.1103/PhysRevD.65.085001`
- `https://arxiv.org/abs/hep-ph/0109228`
- `https://doi.org/10.1140/epjc/s10052-015-3579-2`
- `https://doi.org/10.1103/PhysRevLett.75.3060`
- `https://doi.org/10.1006/aphy.2001.6117`

## Result Contract

The phase must pass only when it preserves the non-promotional boundary:

- Nielsen identities and complex-pole gauge independence are recorded as
  physical mass-extraction constraints.
- Pole residues, complex-mass scheme, algebraic identity control, and
  pinch-technique resonance leads are recorded.
- Phase201, Phase213, Phase256, Phase260, Phase295, Phase344, and Phase345
  blockers remain unfilled.
- All GU-local source-law, observed-field extraction, W/Z/H mass-promotion, and
  completion flags remain false.

## Outputs

- `studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output/nielsen_pole_mass_gauge_independence_source_audit.json`
- `studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output/nielsen_pole_mass_gauge_independence_source_audit_summary.json`

## Decision

Do not promote physical W/Z/H masses from Nielsen identities or complex-pole
gauge independence. They define a rigorous physical pole-mass boundary, but
they do not derive GU-local observed operators, pole equations, mass scales,
weak mixing, couplings, Higgs scalar-source lineage, or GeV units.

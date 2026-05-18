# Phase260 Mass-Definition Convention Sensitivity Audit

Phase260 checks whether pole-mass versus Breit-Wigner/running-width conventions could explain the W/Z/H prediction failure.

The audit applies the standard mass-dependent-width Breit-Wigner to pole-mass approximation:

`M_pole = M_BW / sqrt(1 + (Gamma_BW / M_BW)^2)`

## Result

Mass-definition conventions do not repair the package:

- W/Z convention shifts are tens of MeV.
- Current W/Z failed prediction gaps are about 10-12 GeV.
- Higgs has no predicted value, so a convention shift cannot promote it.
- Source-lineage blockers remain open.

## Outputs

- `studies/phase260_mass_definition_convention_sensitivity_audit_001/output/mass_definition_convention_sensitivity_audit.json`
- `studies/phase260_mass_definition_convention_sensitivity_audit_001/output/mass_definition_convention_sensitivity_audit_summary.json`

## Boundary

Phase260 does not change physical targets or promote predictions. It records that mass-definition conventions are not the active W/Z/H blocker.

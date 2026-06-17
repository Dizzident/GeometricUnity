# Phase420 Naive Curvature Mass Scale Sanity Check

## Purpose

Phase420 tests the restart prompt's highest-priority post-Phase419 branch:
whether a naive Superphysics/GU-draft-style curvature mass reading, especially
the stylized `m = R(y)/4` relation, can provide the missing W/Z/H scale law.

## Result

The study is target-blind and fail-closed. It does not compare to W/Z/H masses
and does not fill Phase201 or Phase256.

- The literal scalar-curvature reading `m = R/4` fails dimensional analysis:
  scalar curvature has mass dimension 2 in natural units, while mass has
  dimension 1.
- The Lichnerowicz-style squared-mass repair `m^2 = R/4` is dimensionally
  coherent only as a symbolic one-scale shell; it still lacks sign,
  normalization, curvature value, electroweak VEV map, weak-angle/coupling
  lineage, particle rows, pole extraction, and GeV/unit conversion.
- A single curvature scalar supplies at most a common scale parameter. It
  cannot by itself produce separate W/Z/H rows, photon/Z mixing, Higgs
  excitation data, or the Phase419 observed-field template fields.

## Output

The study writes:

- `output/naive_curvature_mass_scale_sanity_check.json`
- `output/naive_curvature_mass_scale_sanity_check_summary.json`

Terminal status:
`naive-curvature-mass-scale-sanity-check-fail-closed`.

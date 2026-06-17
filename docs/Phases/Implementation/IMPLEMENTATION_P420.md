# Implementation P420 - Naive Curvature Mass Scale Sanity Check

Phase420 adds
`studies/phase420_naive_curvature_mass_scale_sanity_check_001`, a fail-closed
study for the restart prompt's post-Phase419 scale sanity-check branch.

## What It Checks

- Treats the Superphysics/GU-draft stylized `m = R(y)/4` relation as a search
  clue, not promotion evidence.
- Tests three target-blind readings:
  - literal scalar-curvature mass, `m = R/4`;
  - Lichnerowicz-style squared-mass repair, `m^2 = R/4`;
  - single-curvature common-scale shell, `m_i = c_i sqrt(R)/2`.
- Confirms the literal reading is dimensionally invalid for a mass because
  scalar curvature has mass dimension 2.
- Confirms the squared-mass repair is only a symbolic scale shell.
- Requires the missing scale fields to stay explicit: sign, coefficient
  normalization, curvature value, VEV map, weak-angle/coupling lineage,
  particle-specific rows, pole extraction, and GeV/unit normalization.
- Preserves the Phase419 observed-field boundary and the Phase201 source
  lineage boundary.

## Scientific Boundary

The phase does not compare W/Z/H target masses and derives no physical scale.
It reports:

- `literalScalarCurvatureMassReadingDimensionallyConsistent=False`
- `squaredMassCurvatureReadingDimensionallyConsistent=True`
- `squaredMassReadingProvidesOnlySymbolicScaleShell=True`
- `providedRequiredScaleSpecificationFieldCount=1`
- `missingScaleSpecificationFieldCount=9`
- `sourceProvidesGeVUnitNormalization=False`
- `canFillPhase201WzContract=False`
- `canFillPhase201HiggsContract=False`
- `canFillPhase256ObservedFieldExtractionContract=False`
- `routePromotesWzMasses=False`
- `routePromotesHiggsMass=False`

Any viable curvature route now needs a source-defined curvature-to-electroweak
VEV equation with sign, coefficient, unit, and vacuum normalization, plus
observed-field/pole rows and weak-angle/coupling lineage.

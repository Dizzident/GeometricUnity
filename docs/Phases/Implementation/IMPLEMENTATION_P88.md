# Phase 88 - Explicit Fermion Dirac Gauge Projection Path

## Goal

Close the next executable blocker toward physical boson prediction by making a
gauge-reduced fermion Dirac bundle representable in production code instead of
only auditable as a missing artifact.

## Completed

- Added `DiracGaugeReductionProjector` in `Gu.Phase4.Dirac`.
- The projector validates a dense fermion-space projector for:
  - square dimension compatibility with the explicit Dirac bundle;
  - symmetry;
  - idempotence.
- The projector materializes `P D P` for explicit complex Dirac matrices and
  emits a new `DiracOperatorBundle` with `GaugeReductionApplied = true`.
- Updated `FermionSpectralSolver` so solved fermion modes and diagnostics inherit
  `GaugeReductionApplied` from the input Dirac bundle.
- Added tests for identity projection, coordinate projection, invalid
  non-idempotent projectors, and solver propagation of the gauge-reduction flag.

## Result

The codebase can now carry a concrete gauge-reduced fermion Dirac operator
through the solver once a scientifically justified fermion-space projector is
provided. This removes the previous hard-coded metadata blocker where solved
fermion modes were always marked unreduced.

## Physical Prediction Status

Still blocked. This phase adds the executable projected-operator path but does
not yet derive the physical Phase12 fermion-space projector from the
connection-space gauge projector or rerun the branch-pair replay with stable
reduced modes.

## Next Step

Build the Phase12 fermion-compatible projector artifact and apply it to the
persisted Phase12 Dirac bundles:

1. derive or materialize the fermion-space projector for the 2x2 Phase12
   geometry and SU(2) gauge layout;
2. project the A/B explicit Dirac matrices with `DiracGaugeReductionProjector`;
3. exact-solve the projected bundles;
4. rerun the branch-pair replay;
5. require the Phase87 gauge-reduction readiness audit to pass before comparing
   against external boson values.

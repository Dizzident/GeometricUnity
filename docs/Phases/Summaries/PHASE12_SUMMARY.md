# Phase XII Summary

Phase XII closed the main execution blockers that were preventing one repository run folder from producing boson and fermion artifacts from the same persisted structured background family.

## Closed

- `assemble-dirac` is now background-aware and environment-aware on the main path.
- Spinor specs are now derived from persisted run context and validated instead of silently defaulting.
- `compute-spectrum` now accepts the canonical `solve-backgrounds` output layout.
- `extract-couplings` now builds and persists finite-difference Dirac variation artifacts from real boson mode vectors and persisted Dirac matrices.
- Per-background fermion outputs and coupling outputs are now namespaced so one family run does not overwrite itself.
- `build-unified-registry` now ingests the per-background coupling atlases emitted by Phase XII.
- The unified registry schema now matches the emitted registry shape and validates successfully.

## Demonstrated by artifacts

The successful joined run is:

- `studies/phase12_joined_calculation_001/output/background_family`

That run contains:

- solver-backed structured backgrounds,
- boson spectra and mode families,
- a boson registry and boson atlas report,
- background-aware Dirac bundles and fermion mode bundles,
- chirality and conjugation reports,
- finite-difference variation matrices and coupling atlases,
- a fermion family atlas and cluster report,
- and a schema-valid unified particle registry.

## Not closed

- Boson comparison is still internal-target-profile only; there is still no honest external known-value comparison path here.
- Fermion observation is still proxy-level; full pullback-level observation is still absent.
- The structured geometry used here is still a low-dimensional control geometry, not evidence for real physical comparison.
- Fermion mode quality is still weak in the executed run: the solver produced large residuals and only trivial chirality outcomes.

## Proven blocked for later honest physical comparison

Later honest comparison against real known boson and fermion values is still blocked by all of the following:

- no external boson descriptor campaign wired into the executed CLI path,
- no non-proxy fermion observation path,
- no physically justified high-dimensional geometry/evidence label beyond structured control,
- and no numerically credible fermion spectrum in the executed run.

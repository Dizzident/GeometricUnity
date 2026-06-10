# Phase394: Positive Bosonic Spectrum Recomputation and Backreaction Construction

## Question

Phase393 identified the concrete missing artifact for the coupled-critical-
point program: the persisted Phase12 bosonic Gauss-Newton spectrum holds only
12 numerical-kernel modes, so the asymmetric first-order backreaction
`delta_omega = -kappa H_B^+ J` was not constructible. Can the full positive
spectrum be recomputed with production provenance, and what does the
constructed backreaction look like?

## Construction

1. Copy the Phase12 `background_family` artifacts (excluding the fermion and
   registry trees) into a study-local working directory - the persisted
   Phase12 artifacts are never mutated.
2. Re-run the repo's own production pipeline,
   `Gu.Cli compute-spectrum <workdir> <backgroundId> --num-modes 156`, per
   background (same OperatorBundleBuilder / EigensolverPipeline path that
   produced the persisted kernel modes).
3. Verify PSD, kernel dimension, spectral gap, su(2) triplet clustering, and
   containment of every persisted kernel mode in the recomputed kernel span.
4. Construct the per-mode first-order backreaction
   `delta_omega^(s) = -sum_{mu_i > tol} m_i (m_i . J^(s)) / mu_i` per unit
   coupling, with `J^(s)` the Phase393 source currents on the converged
   shell, plus each source's unabsorbable kernel component and the
   second-order bosonic relaxation energy.

## Results

- Full 156-mode spectrum per background: PSD (zero negative), kernel
  dimension exactly **18** (the persisted 12 modes are contained in it to
  containment 1.000000), spectral gap **0.06294** (both backgrounds nearly
  identical; max eigenvalue 6.017 both).
- **su(2) triplet clustering fraction 1.0**: every positive eigenvalue
  belongs to an exact triplet cluster (adjoint degeneracy).
- Source kernel fractions 0.121 / 0.134 per mode (the unabsorbable
  first-order obstruction for asymmetric occupation).
- Backreaction norms 0.4425 / 0.4349 per unit coupling (identical across the
  four shell modes per background); relaxation energies 0.0280 / 0.0295 per
  unit coupling squared.
- **The backreaction direction exhibits the suppressed gauge axis**: axis
  fractions [0.5426, 0.0008, 0.4566] (bg-a) and [0.5296, 0.0007, 0.4697]
  (bg-b) - nearly identical to the Phase379 Gram fractions. Because the
  source currents lie exactly in the rank-3 Gram image (Phase393) and the
  bosonic Hessian inverse preserves the suppression, the suppressed-axis
  structure RE-EMERGES in a genuinely action-derived dynamical object,
  refining the Phase392 metric-dependence conclusion: the fermion-loop
  response operator is isotropic, but the first-order backreaction direction
  is not.

## Status

Fail-closed. The Phase393 artifact gap is closed; the coupling is not
physical, no coupled critical point is solved, and no Phase201/Phase256
contract field is filled. `canFillPhase201WzContract=False`.

## Reproduce

```bash
dotnet run --project studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj
```

(The study re-stages its working copy and re-runs the CLI solves on every
run; the recomputed spectrum bundles live under `output/family_workdir/` and
are reproducible rather than committed.)

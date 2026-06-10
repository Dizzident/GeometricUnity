# IMPLEMENTATION_P394: Positive Bosonic Spectrum Recomputation and Backreaction Construction

## Scope

Phase394 closes the Phase393 artifact gap: it recomputes the full 156-mode
bosonic Gauss-Newton spectrum at both Phase12 backgrounds through the
production `Gu.Cli compute-spectrum` pipeline (on a study-local copy of the
family, never mutating persisted artifacts) and constructs the previously
blocked first-order asymmetric backreaction.

## Artifacts

- Study: `studies/phase394_positive_bosonic_spectrum_backreaction_construction_001`
- Project: `Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj`
- Outputs:
  - `output/positive_bosonic_spectrum_backreaction_construction.json`
  - `output/positive_bosonic_spectrum_backreaction_construction_summary.json`
  - `output/family_workdir/` (recomputed spectrum bundles; reproducible,
    not committed - the embedded spectrum ids are timestamped)

## Method

The study stages a copy of the family tree (atlas, background records/states,
bosons, campaigns, manifest, modes, observables, reports, spectra; the
51 MB fermion tree and registry are not read by compute-spectrum), invokes
`dotnet run --project apps/Gu.Cli -- compute-spectrum <workdir> <bg>
--num-modes 156` per background, and analyzes the resulting 156 mode records
alongside the Phase393 source currents (recomputed via the dense converged
shell path).

Backreaction per shell mode `s`, per unit coupling:

```
delta_omega^(s) = -sum_{mu_i > tol} m_i (m_i . J^(s)) / mu_i
E^(s)           =  sum_{mu_i > tol} (m_i . J^(s))^2 / mu_i
```

with `m_i` the recomputed M-orthonormal bosonic modes and the kernel
(|eig| <= 1e-10 max|eig|) excluded.

## Results

| Quantity | bg-a | bg-b |
| --- | --- | --- |
| Kernel dimension | 18 | 18 |
| Positive count / negative count | 138 / 0 | 138 / 0 |
| Spectral gap | 0.062942 | 0.062942 |
| Max eigenvalue | 6.017 | 6.017 |
| Triplet cluster fraction | 1.0 | 1.0 |
| Persisted kernel containment | 1.000000 | 1.000000 |
| Source kernel fraction (per mode) | 0.1208 | 0.1336 |
| Backreaction norm (per unit kappa) | 0.4425 | 0.4349 |
| Relaxation energy (per kappa^2) | 0.0280 | 0.0295 |
| Backreaction axis fractions | [0.5426, 0.0008, 0.4566] | [0.5296, 0.0007, 0.4697] |

Key diagnosis refinement: the backreaction DIRECTION reproduces the
suppressed gauge axis (axis-1 fractions 0.0008 / 0.0007, nearly identical to
the Phase379 Gram fractions). The Phase392 conclusion stands - the
fermion-loop response operator is isotropic - but the first-order
backreaction for asymmetric occupation inherits the Gram-image structure of
the source currents (Phase393), so the suppressed axis re-emerges in an
action-derived dynamical object. The two backgrounds also share nearly
identical bosonic spectra with exact su(2) triplet degeneracy throughout.

## Fail-closed boundary

No physical coupling, no coupled critical point, all route flags false, zero
Phase201/Phase256 fields, target-blind construction hash persisted.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`positiveBosonicSpectrumBackreactionConstruction` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `positive-bosonic-spectrum-backreaction-construction-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase394 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase394 run passes (CLI recomputation + analysis).
- Phase101, Phase202 (checklist 186 -> 187 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase394 included; objective remains
  incomplete by design.

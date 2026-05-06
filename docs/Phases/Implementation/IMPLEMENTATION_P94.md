# Implementation P94: Exact Refinement Projected Dirac Modes

## Objective

Materialize refinement-level projected fermion modes with residuals low enough to serve as numerical evidence for the physical prediction gate.

Phase93 showed that `4x4` replay needs:

- boson perturbation vector length `576`
- fermion eigenvector length `1800`

This phase uses existing source-backed Phase11 refinement backgrounds to produce exact projected fermion eigenvectors at `2x2` and `4x4`.

## Changes

- Added `studies/phase94_refinement_projected_dirac_exact_modes_001/materialize_exact_refinement_modes.py`.
- Assembled source-backed Phase11 Dirac bundles:
  - `bg-phase11-direct-nontrivial-shiab-l0-gn-20260315181820` (`env-refinement-2x2`)
  - `bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830` (`env-refinement-4x4`)
- Materialized identity-projected Dirac bundles and exact non-null fermion modes using `numpy.linalg.eigh`.
- Recorded the identity fermion-space projector explicitly for each refinement level.

## Validation

Assembled `2x2` Dirac:

```bash
dotnet run --project apps/Gu.Cli/Gu.Cli.csproj --no-restore -- assemble-dirac \
  studies/phase5_su2_branch_refinement_env_validation/config \
  --background-id bg-phase11-direct-nontrivial-shiab-l0-gn-20260315181820 \
  --out studies/phase94_refinement_projected_dirac_exact_modes_001/output/phase11_l0_2x2/dirac_bundle_bg-phase11-direct-nontrivial-shiab-l0-gn-20260315181820.json
```

Result:

- `TotalDof`: `324`
- replay eigenvector length: `648`
- explicit matrix: `true`
- Hermitian: `true`

Assembled `4x4` Dirac:

```bash
dotnet run --project apps/Gu.Cli/Gu.Cli.csproj --no-restore -- assemble-dirac \
  studies/phase5_su2_branch_refinement_env_validation/config \
  --background-id bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830 \
  --out studies/phase94_refinement_projected_dirac_exact_modes_001/output/phase11_l1_4x4/dirac_bundle_bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830.json
```

Result:

- `TotalDof`: `900`
- replay eigenvector length: `1800`
- explicit matrix: `true`
- Hermitian: `true`

Ran exact materialization:

```bash
python3 studies/phase94_refinement_projected_dirac_exact_modes_001/materialize_exact_refinement_modes.py
```

Result:

- `2x2` max residual: `2.6629139943694494e-14`
- `4x4` max residual: `5.686441086977053e-14`
- both projected mode bundles have `gaugeReductionApplied: true`

Focused Dirac tests still pass:

```bash
dotnet test tests/Gu.Phase4.Dirac.Tests/Gu.Phase4.Dirac.Tests.csproj --no-restore --verbosity minimal
```

Result: 94 passed, 0 failed.

## Outcome

The numerical fermion refinement data blocker is partially cleared:

- We now have exact `2x2` projected fermion modes with replay-compatible eigenvector length `648`.
- We now have exact `4x4` projected fermion modes with replay-compatible eigenvector length `1800`.
- The `4x4` vectors match the Phase93 replay validator requirement.

## Remaining Blockers

- The identity fermion-space lift still needs a derivation against the connection-space gauge quotient.
- The selected Phase91 fermion pair must be matched target-blind into the Phase94 refinement mode bundles before refinement stability can be promoted.
- A source-backed refinement boson `modeVector` with length `576` is still missing. Phase40/43 selector records identify refinement cells and mass-like values, but the inspected Phase43 mode records do not carry replay-compatible `modeVector` arrays.

## Next Phase

Implement target-blind mode matching between the selected Phase91 `2x2` exact modes and the Phase94 `2x2`/`4x4` exact refinement modes. Keep the result as evidence only if matching is based on internal eigenvalue/vector structure and does not use external boson target values.

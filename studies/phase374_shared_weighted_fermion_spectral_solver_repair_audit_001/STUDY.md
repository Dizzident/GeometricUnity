# Phase 374 Shared Weighted Fermion Spectral Solver Repair Audit

## Purpose

This audit follows the Phase373 local `K/A/B` representation experiment. It
checks the repaired shared `FermionSpectralSolver` path directly:

```text
K psi = lambda M_psi psi
B = M_psi^-1/2 K M_psi^-1/2
psi = M_psi^-1/2 u
```

For each persisted Phase12 background, it solves the stiffness matrix `K` with
nonuniform mesh-volume `M_psi`, verifies generalized residuals and
`M_psi`-orthonormality, and compares the identity-weight path against the
unweighted path. The selected persisted modes are exact kernel modes, so the
audit also requires an independent nonuniform-`M_psi` four-mode
nonzero-spectrum benchmark through the same shared solver API.

## Run

```bash
dotnet run --project studies/phase374_shared_weighted_fermion_spectral_solver_repair_audit_001
```

## Outputs

- Summary: `output/shared_weighted_fermion_spectral_solver_repair_audit_summary.json`
- Full audit: `output/shared_weighted_fermion_spectral_solver_repair_audit.json`
- Per-background evidence: `output/backgrounds/*.json`

## Boundary

This audit validates a shared discrete solver repair only. It does not prove
that mesh-volume lumping is the physical GU fermion inner product, establish a
fixed GU fermionic branch, complete the coupled action, derive a W/Z bridge law
or Higgs scalar row, normalize GeV units, or promote boson predictions.

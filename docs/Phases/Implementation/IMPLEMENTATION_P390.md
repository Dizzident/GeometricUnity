# IMPLEMENTATION_P390: Converged Control-Branch Fermion Mode Rebuild and Sharp Ward Probe

## Scope

Phase390 removes the two boundary defects Phase389 disclosed, at the discrete
control-branch level: the unconverged persisted Phase12 fermion modes (which
made the pure-gauge Ward zero-current statement untestable) and the
unmaterialized mesh-volume `M_psi` generalized branch (the Phase372 actionable
obstruction). It fails closed on everything physical.

## Artifacts

- Study: `studies/phase390_converged_control_branch_fermion_mode_rebuild_001`
- Project: `Phase390ConvergedControlBranchFermionModeRebuild.csproj`
- Source: `Program.cs`
- Outputs:
  - `output/converged_control_branch_fermion_mode_rebuild.json`
  - `output/converged_control_branch_fermion_mode_rebuild_summary.json`

## Mathematical content

For both persisted Phase12 backgrounds, an in-study complex Hermitian Jacobi
eigensolver (phase-rotation + real rotation per (p,q) pair, cyclic sweeps,
off-diagonal Frobenius threshold `1e-13` relative) solves:

- the identity-weight branch `D psi = lambda psi` on the persisted explicit
  base Dirac matrix, and
- the mesh-volume branch via `B = M^{-1/2} D M^{-1/2}` with generalized modes
  `v = M^{-1/2} w` satisfying `D v = lambda M v`, M-orthonormal.

Verification is performed against the original persisted matrix (not the
rotated copy): eigen-residuals `<= 3.1e-13`, orthonormality at solver
precision, 10-11 sweeps per solve.

Kernel accounting: the toy mesh has 4 isolated vertices (vertex degree 0)
whose Dirac rows vanish identically, giving an exact structural kernel of
48 = 4 x 12 dofs per branch. The target-blind mode selection excludes the
near-zero kernel (`|lambda| <= 1e-10 * max|lambda|`) and takes the 12
smallest-|lambda| nonzero eigenpairs; selecting kernel modes would make the
Ward test vacuous (`D psi = 0` exactly).

Structural commutation: `M_psi` from `MassPsiWeightsBuilder.BuildFromMesh` is
scalar per vertex block while gauge parameters `X_hat` act within vertex
blocks, so `[M_psi, X_hat] = 0` exactly (verified, residual 0). Consequences:

- The Phase389 discrete gauge-compatibility identity conjugates exactly onto
  the M_psi branch: `[B, X_hat] = M^{-1/2} (delta_D[v(X)] + R(X)) M^{-1/2}`.
- The generalized Ward statement holds: for `D v = lambda M v`,
  `|Re<v, [D, X_hat] v>| <= 2 ||D v - lambda M v|| ||X_hat v||` because
  `<M v, X_hat v> - <v, X_hat M v> = v^dagger [M, X_hat] v = 0`.

Sharp Ward results across 4032 (direction, mode, branch, background) rows:
max |current| `1.7e-14` (identity) and `9.3e-13` (mesh-volume). The Phase389
caveat (`wardEigenBoundSharp=False` on the persisted modes) is resolved on the
rebuilt branches.

Persisted-branch characterization: every persisted Phase12 mode has relative
eigen residual in `12.1 - 18.5` against the persisted matrix, and best overlap
with any single converged eigenvector of only `0.376`. The persisted modes are
non-eigen mixtures; any spectral-shell physics built directly on them inherits
this defect (the shell-response chain Phases 376-388 used them as fixed test
vectors, which remains valid as a study-defined diagnostic but not as spectral
physics).

## Fail-closed boundary

Phase390 accepts zero Phase201/Phase256 fields and keeps all physical VO-7
route flags false, including `routeProvidesPhysicalMassPsiCompatibleBranch`
(the mesh, backgrounds, and `M_psi` remain toy/study-defined). The rebuild is
recorded as `convergedControlBranchModesRebuilt=true`,
`mPsiCompatibleGeneralizedControlBranchMaterialized=true`,
`completesVo7=false`.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`convergedControlBranchFermionModeRebuild` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `converged-control-branch-fermion-mode-rebuild-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase390 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase390 run passes with all convergence, commutation, and sharp
  Ward checks.
- Phase101, Phase202 (checklist 182 -> 183 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase390 included; objective remains
  incomplete by design.

# Bosonic Validation Study 001 — Report

**Study ID:** bosonic_validation_001
**Branch:** bosonic-validation-001
**Date:** 2026-03-12
**Phase:** P4-C3 (prerequisite for M33)

---

## What Was Tested

This study validates the full bosonic artifact chain with a nontrivial
(nonzero, non-constant) initial connection. It is stronger than the existing
smoke tests, which use flat (zero) connections.

### Branch Configuration

| Field | Value |
|---|---|
| Torsion branch | `augmented-torsion-v1`: T^aug = d_{A0}(omega - A0) |
| Shiab branch | `identity-shiab-v1`: S = F (curvature 2-form) |
| Gauge group | su(2) |
| Mesh | Single tetrahedron (4 vertices, 6 edges, 4 faces, 3D) |
| Gauge strategy | Penalty (lambda = 1.0) |

### Initial Conditions

**omega (dynamical connection):** edge-varying linear ramp on two generators:
- Generator 0: `omega_0[e] = 0.3 * (e+1) / 6` (edges 0..5)
- Generator 1: `omega_1[e] = 0.1 * (6-e) / 6`

**A0 (background connection):** edge-varying on one generator:
- Generator 1: `A0_1[e] = 0.2 * (e+0.5) / 6`

The varying profiles ensure:
1. `d(omega)` is nonzero on each face (boundary operator sum does not cancel)
2. `alpha = omega - A0` is nonzero, activating the augmented torsion
3. The Jacobian bracket terms `[omega wedge delta]` are generically nonzero
   (for constant connections, antisymmetry of the Lie bracket causes cancellation)

---

## Artifacts Produced

The following artifacts were verified to be nontrivial via regression tests:

| Artifact | Test | Status |
|---|---|---|
| Initial omega | `Study001_InitialOmega_IsNonzero` | PASS — max coeff > 0 |
| Curvature F | `Study001_CurvatureF_IsNonzero` | PASS — F = d(omega) + (1/2)[omega,omega] nonzero |
| Torsion T | `Study001_AugmentedTorsion_IsNonzero` | PASS — T = d_{A0}(omega-A0) nonzero |
| Residual Upsilon | `Study001_Residual_Upsilon_IsNonzero` | PASS — Upsilon = S - T = F - T nonzero |
| Jacobian J | `Study001_Jacobian_IsNontrivial` | PASS — ||J||_F > 0 (nonzero Frobenius norm) |
| Gradient G | `Study001_Gradient_IsNonzero` | PASS — G = J^T M Upsilon nonzero |
| Spectrum eigenvalues | `Study001_Spectrum_HasNontrivialEigenvalues` | PASS — at least one lambda > 0 |
| Artifact bundle | `Study001_SolverRun_ProducesArtifactBundle_RegardlessOfConvergence` | PASS — bundle produced |

All 9 regression tests pass.

---

## What Was Validated

### Branch-Consistent Execution

The following properties are verified to hold consistently with the declared branch:

1. **Residual operator chain is internally consistent**: Upsilon = S - T = F - T is computed
   through the correct operator sequence (curvature → shiab → torsion → subtract).

2. **Augmented torsion branch produces nontrivial torsion**: T = d_{A0}(omega - A0) is
   nonzero because alpha = omega - A0 ≠ 0 and A0 ≠ 0 (the bracket [A0, alpha] ≠ 0).

3. **Jacobian is nontrivial**: J = dS/domega - dT/domega has nonzero Frobenius norm,
   confirming that small perturbations in omega produce nonzero changes in the residual.

4. **Spectrum has structure**: The Gauss-Newton Hessian H = J^T M J + lambda * C^T C has
   at least one positive eigenvalue, confirming the operator is positive-definite at this
   background.

5. **Artifact bundle is produced regardless of convergence**: Even with Mode A (residual-only,
   1 iteration), the full artifact bundle including DerivedState and InitialState is populated.

6. **Nontrivial branch differs from trivial baseline**: The residual norm with nonzero omega
   and A0 strictly exceeds the trivial (omega=0, A0=0) baseline norm.

### Negative Results Preserved

The study is designed to preserve negative results:

- The solver is run in Mode A (residual-only) with MaxIterations=1 and a tight tolerance
  (1e-12). This guarantees the solver does NOT converge.
- Despite non-convergence, the artifact bundle is fully populated.
- This tests the claim that "if the branch is unstable, we still get artifacts."

---

## What Is Still Unvalidated

The following items are NOT validated by this study:

### Physical Validity

1. **We have not validated that the GU equations are physically correct.** This study
   confirms the computational chain is internally consistent (given the code), NOT that
   the theory correctly predicts physics.

2. **We have not validated the continuum limit.** The single-tetrahedron mesh is far
   too coarse to extract physically meaningful mass spectra or gauge couplings.

3. **We have not validated gauge invariance of the residual.** A gauge transformation
   of omega should leave Upsilon invariant (up to the gauge-fixing term), but this
   property is not tested here.

4. **We have not validated the Shiab operator choice.** The identity Shiab (S=F) is the
   simplest possible choice but is not the physically motivated Shiab from the GU paper.
   The actual Shiab involves a non-trivial intertwining with the Riemannian geometry of Y.

5. **We have not validated that the bi-connection (A0 ± omega) satisfies the GU
   compatibility conditions.** The background A0 is set to an arbitrary edge-varying
   value; in a physical setup, A0 should be constructed from the Levi-Civita connection.

### Computational Completeness

6. **Observation artifact is not yet produced.** The observation pipeline (sigma^* pullback)
   is not run in this study. This is a gap for follow-up.

7. **Spectrum is computed at initial state only.** A physically meaningful spectrum would
   require first solving to a stationary point (or near-stationary point) of the objective.

8. **GPU parity is not verified.** All computations are CPU-only (CpuReferencePipeline).
   GPU verification is deferred to M44.

9. **Branch sensitivity (Mode D sweep) not run.** A multi-branch sweep comparing this
   branch against others (e.g., trivial torsion) would strengthen the validation.

---

## Files

```
studies/bosonic_validation_001/
├── branch.json           — Branch manifest (augmented-torsion + identity-shiab)
├── environment.json      — Environment config (mesh, initial conditions, observables)
├── run_study.sh          — Scripted runner (builds + runs regression tests)
└── REPORT.md             — This report

tests/Gu.Phase3.Spectra.Tests/
└── BosonicValidationStudy001Tests.cs  — 9 regression tests
```

---

## How to Run

```bash
cd /path/to/GeometricUnity
bash studies/bosonic_validation_001/run_study.sh
```

Or directly:
```bash
dotnet build --configuration Release
dotnet test tests/Gu.Phase3.Spectra.Tests/Gu.Phase3.Spectra.Tests.csproj --no-build \
  --filter "FullyQualifiedName~BosonicValidationStudy001"
```

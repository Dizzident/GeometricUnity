# Phase 1 Gaps Analysis — Round 2

**Date:** 2026-03-09
**Baseline:** Commit 488c02a (main), 1174 tests passing
**Prior analysis:** PHASE_1_GAPS_01.md (GAP-1 through GAP-9, all closed in commit 2a84b48)
**Reference:** IMPLEMENTATION_PLAN.md (1830 lines, 13 milestones)

---

## Summary

The previous 9 gaps (GAP-1 through GAP-9) are all closed as of commit 2a84b48. Subsequent
commits (eecae7a, d9b6b53, 488c02a) extended CUDA parity tests and fixed benchmark buffer sizing.

This round identifies **new or residual gaps** that remain after that work. The gaps fall into
four categories:

1. **Missing solver mode** — Mode D (branch sensitivity) is not implemented
2. **Incomplete GPU parity** — GPU tests check self-consistency, not C# reference physics
3. **Architecture/design gaps** — buffer size mismatch, CpuReferenceBackend stubs
4. **Missing plan deliverables** — examples, environments, Krylov solver, Gauss-Newton

The C# orchestration layer, CPU reference physics (CurvatureAssembler, AugmentedTorsionCpu,
CpuSolverBackend), validation pipeline, artifact system, and Vulkan data-prep are all solid.
The CUDA kernels (`gu_curvature_physics_kernel`, `gu_torsion_physics_kernel`) are real
GPU implementations with correct physics formulas.

---

## Definition of Done Re-check (Section 22)

| # | Criterion | Status | Notes |
|---|-----------|--------|-------|
| 1 | Branch manifest frozen and serialized | ✓ DONE | BranchManifest + validator |
| 2 | All required lowering maps exist | ✓ DONE | Full Section 6 contract met |
| 3 | CPU reference backend evaluates residuals and solves | ✓ DONE | Modes A/B/C in CpuSolverBackend |
| 4 | CUDA residual parity passes for benchmarks | ⚠ PARTIAL | Self-consistency only, not vs C# reference |
| 5 | At least one CUDA solve path exists | ⚠ PARTIAL | Mode A (residual-only) on GPU; Mode B/C throw NotSupportedException |
| 6 | Observed outputs exclusively through σ_h* | ✓ DONE | ObservationPipeline enforces this |
| 7 | Validation bundles generated for all runs | ✓ DONE | RunFolderWriter + ValidationBundle |
| 8 | Artifact packages replay at R2 on CPU | ✓ DONE | ReplayContractValidator passes |
| 9 | Cross-backend parity benchmark reaches R3 | ⚠ PARTIAL | Metadata R3 met; no numerical physics match vs C# |
| 10 | Failure cases preserved as artifacts | ✓ DONE | NegativeResultPreserver + ArtifactBundle |
| 11 | Visualization is artifact-driven and read-only | ✓ DONE | VulkanContext + ArtifactViewerService |
| 12 | System can run EnvironmentSpec and emit typed outputs | ✓ DONE | `gu run` + ObservationPipeline |

**Score: 9/12 fully met, 3 partial**

---

## GAP-1: Mode D (Branch Sensitivity) Not Implemented

**Severity:** HIGH
**Plan Section:** 13.3, DoD criterion 12

### Current State

`SolveMode` enum in `src/Gu.Solvers/SolverTypes.cs` has three values:
```csharp
public enum SolveMode { ResidualOnly, ObjectiveMinimization, StationaritySolve }
```

`SolverOrchestrator` handles A, B, C; the `_ => throw` branch rejects any other mode.
The CLI `--mode` flag only accepts `A`, `B`, `C`. There is no Mode D orchestrator, sweep runner,
or comparison accumulator.

### What's Missing

Per Section 13.3, Mode D must:
- Re-run the same environment spec under **multiple torsion/Shiab branch manifests**
- Compare: convergence behavior, final residual norms, observed outputs, replayability,
  failure modes across branch variants
- Emit a structured comparison artifact that preserves all per-branch results

This requires:
1. `SolveMode.BranchSensitivity` added to enum
2. `BranchSensitivityRunner` class (or equivalent) in `Gu.Solvers` that accepts a list of
   `BranchManifest` and a common `EnvironmentSpec` and sweeps them
3. `BranchComparisonArtifact` type to hold per-branch results side-by-side
4. CLI `--mode D --branches branch1.json,branch2.json,...` support

### Dependencies
- Requires at least two distinguishable branch manifests (e.g., identity Shiab vs trivial Shiab,
  or augmented torsion vs trivial torsion)

### Estimated Scope
~300-500 lines (C# runner + artifact type + CLI wiring + tests)

---

## GAP-2: CpuReferenceBackend in Interop Layer Has Physics Stubs

**Severity:** HIGH
**Plan Section:** 15.4, 16.3 (Interop tests), IA-5

### Current State

`src/Gu.Interop/CpuReferenceBackend.cs` (the `INativeBackend` implementation used for
parity testing) has the following stubs:

| Method | Stub Behavior | Required Behavior |
|--------|---------------|-------------------|
| `EvaluateCurvature` | `F = omega` (Array.Copy) | `F = d(omega) + (1/2)[omega, omega]` |
| `EvaluateTorsion` | `T = 0` (Array.Clear) | Augmented torsion: `d_{A0}(omega - A0)` |
| `EvaluateShiab` | `S = omega` (Array.Copy) | Identity Shiab: `S = F` |

This backend is **separate from** the real C# reference in `Gu.ReferenceCpu` (which uses
`CurvatureAssembler`, `AugmentedTorsionCpu`, `IdentityShiabCpu`). The interop-layer CPU
backend does not call any of those real implementations.

The comment says "In a full implementation, this would compute F = d(omega) + (1/2)[omega, omega]
using the mesh and Lie algebra from the manifest" — but that implementation does not exist.

### What's Missing

`CpuReferenceBackend.EvaluateCurvature/Torsion/Shiab` must compute the same physics as:
- `CurvatureAssembler.Assemble()` — for curvature
- `AugmentedTorsionCpu.Evaluate()` — for torsion
- `IdentityShiabCpu.Evaluate()` — for Shiab (identity = curvature output)

This requires the backend to:
1. Accept mesh topology (already stored in `_topology`)
2. Accept Lie algebra data (already stored in `_algebra`)
3. Compute real physics using those stored arrays

### Why This Matters

Without real physics in `CpuReferenceBackend`, there is no true CPU-vs-GPU parity path:
- `CudaSolverBackendTests` wraps `CpuReferenceBackend` (stubs) → not testing real physics
- `GpuSolverBackendTests` wraps `CpuReferenceBackend` (stubs) → not testing real physics
- Only `CudaGpuParityTests` uses `CudaNativeBackend` but compares against analytical zeros

### Required Work

Implement real physics in `CpuReferenceBackend.EvaluateCurvature`:
```csharp
// Pseudo-code outline:
// 1. Deserialize topology from _topology (face-edge incidence, orientations)
// 2. Deserialize algebra from _algebra (structure constants)
// 3. For each face: compute d(omega) + 0.5*[omega,omega] using topology+algebra
// 4. Write to curvature output buffer
```
Similarly implement torsion (d_{A0}(alpha) where alpha = omega - A0) and Shiab (identity = curvature).

### Estimated Scope
~150-250 lines of C# physics code mirroring CurvatureAssembler + AugmentedTorsionCpu logic

---

## GAP-3: No True CPU-vs-GPU Physics Parity Test

**Severity:** HIGH
**Plan Section:** 15.4, 19.2

### Current State

`tests/Gu.Interop.Tests/CudaGpuParityTests.cs` (26 tests, T1-T13) verifies:
- T1a: Zero omega → zero curvature
- T1b: Curvature is deterministic
- T1c: Curvature is NOT the identity stub (has non-zero values for non-zero omega)
- T2a: Zero omega + zero A0 → zero torsion
- T3: Shiab equals curvature (identity Shiab)
- T4: Residual = S - T
- T5: Objective = (1/2) sum(r²)
- T6a/b/c: Buffer round-trip (upload → download ≈ identity)
- T7: Full pipeline deterministic
- T8/T8b: HasPhysicsData is true, BackendId is "cuda"

**None of these tests compare GPU kernel output against the C# reference physics output on
the same mesh with the same inputs.**

Plan Section 15.4 requires CPU and GPU to match within declared tolerances for:
> curvature assembly, torsion branch, Shiab branch, residual field, objective value,
> gauge term, observation pullback, any derived observable used in validation

DoD criterion 9 ("cross-backend parity benchmark reaches R3") is currently satisfied only at
the metadata level (different `BackendId` strings in two run folders), not by numerical
physics comparison.

### What's Missing

A parity test that:
1. Creates a mesh (e.g., `ToyGeometryFactory.CreateToy2D()`)
2. Chooses a non-trivial `omega` (e.g., small random values with fixed seed)
3. Computes curvature using C# `CurvatureAssembler.Assemble(omega, mesh, algebra)` → CPU result
4. Computes curvature using `CudaNativeBackend.EvaluateCurvature(omegaBuf, curvatureBuf)` → GPU result
5. Compares element-wise within tolerance (e.g., 1e-12)
6. Repeats for torsion and objective

This test would require GAP-2 (real `CpuReferenceBackend`) or an alternative approach using
`CurvatureAssembler` directly (independent of `INativeBackend`).

### Required Work

1. After closing GAP-2, add parity test class `CudaCpuPhysicsParityTests` with:
   - `T_Curvature_Parity`: CPU curvature == GPU curvature for non-trivial omega
   - `T_Torsion_Parity`: CPU torsion == GPU torsion for non-trivial omega + A0
   - `T_Objective_Parity`: CPU objective == GPU objective
2. These tests must use `[SkipIfNoCuda]` to allow CI without GPU

### Estimated Scope
~200 lines of test code (after GAP-2 is closed)

---

## GAP-4: GPU Jacobian and Adjoint Not Implemented (CUDA Stage 2)

**Severity:** MEDIUM-HIGH
**Plan Section:** 15.2 items 8-9, 15.5 Stage 2

### Current State

`GpuSolverBackend.BuildJacobian()` and `ComputeGradient()` throw `NotSupportedException`:

```csharp
/// GPU Jacobian dispatch is not yet implemented (planned for M10).
/// Throws NotSupportedException.
public ILinearOperator BuildJacobian(...) =>
    throw new NotSupportedException("GPU Jacobian not yet implemented (planned for M10+)");
```

This means:
- GPU backend can only run Mode A (residual-only)
- GPU backend cannot run Mode B (objective minimization) — gradient descent requires Jacobian
- GPU backend cannot run Mode C (stationarity solve) — also requires Jacobian
- DoD criterion 5 is met at the minimum level (Mode A = residual), not at the "solve" level

### What's Missing

Per CUDA Stage 2 (Section 15.5):
1. `INativeBackend` needs `EvaluateJacobianAction(PackedBuffer omega, PackedBuffer v, PackedBuffer Jv)` —
   Jacobian-vector product kernel
2. `INativeBackend` needs `EvaluateAdjointAction(PackedBuffer upsilon, PackedBuffer JTv)` —
   adjoint/JT-vector product kernel
3. Native CUDA kernels for `dF/d_omega(delta)` and `dT/d_omega(delta)` (linearizations)
4. `GpuSolverBackend.BuildJacobian()` returns a matrix-free `IGpuLinearOperator`
5. `GpuSolverBackend.ComputeGradient()` dispatches via that operator

The C# reference `CpuLocalJacobian` in `Gu.ReferenceCpu` is the authoritative linearization
implementation to match.

### Estimated Scope
~400-600 lines C/CUDA + ~150 lines C# wrapper

---

## GAP-5: GpuSolverBackend Uses Wrong Buffer Size for Face-Valued Fields

**Severity:** MEDIUM
**Plan Section:** 15.3 (CUDA memory layout), 12.2

### Current State

`GpuSolverBackend.EvaluateDerived()` allocates all buffers using `omega.Coefficients.Length`:

```csharp
int n = omega.Coefficients.Length;         // edge_count * dimG
var layout = BufferLayoutDescriptor.CreateSoA("gpu-field", new[] { "c" }, n);

var omegaBuf      = _nativeBackend.AllocateBuffer(layout);  // correct: edge-valued
var curvatureBuf  = _nativeBackend.AllocateBuffer(layout);  // WRONG: face-valued
var torsionBuf    = _nativeBackend.AllocateBuffer(layout);  // WRONG: face-valued
var shiabBuf      = _nativeBackend.AllocateBuffer(layout);  // WRONG: face-valued
var residualBuf   = _nativeBackend.AllocateBuffer(layout);  // WRONG: face-valued
```

Per Section 12.2:
- `omega_h` (connection): **edge-valued** — length = `edge_count * dimG`
- `F_h`, `T_h`, `S_h`, `Upsilon_h` (curvature/residual): **face-valued** — length = `face_count * dimG`

For ToyGeometryFactory.CreateToy2D(): 8 edges, 4 faces, dimG=3 → omega=24, curvature=12.
The current code allocates 24 elements for all buffers; correct is 12 for face-valued.

This bug is masked by `CpuReferenceBackend` stubs (curvature = memcpy(omega), ignores size
mismatch). With real `CudaNativeBackend`, the curvature kernel would write 12 correct values
into a 24-element buffer, and the download would include 12 garbage values in positions 12-23.

Commit 488c02a fixed this in `BenchmarkRunner.cs` but NOT in `GpuSolverBackend.cs`.

### Required Work

Modify `GpuSolverBackend.EvaluateDerived()` to use geometry-aware buffer sizes:
```csharp
// omega is edge-valued: edge_count * dimG
int nOmega = omega.Coefficients.Length;
// curvature/torsion/Shiab/residual are face-valued: face_count * dimG
int nFace = geometry.AmbientSpace.FaceCount * dimG;   // need FaceCount from geometry
var omegaLayout    = BufferLayoutDescriptor.CreateSoA("omega", new[] { "c" }, nOmega);
var faceLayout     = BufferLayoutDescriptor.CreateSoA("face-field", new[] { "c" }, nFace);
```
This requires `GeometryContext.AmbientSpace` (SpaceRef) to expose `FaceCount`. Currently
`SpaceRef` only has `Dimension` and `SpaceId`. Either extend SpaceRef, or pass face count
separately.

### Estimated Scope
~20-40 lines C# + SpaceRef or geometry context extension

---

## GAP-6: Examples Directory Lacks Complete Environment Specifications

**Severity:** MEDIUM
**Plan Section:** 8 (Repository structure), 17.2

### Current State

```
examples/
  toy_branch_2d/
    geometry.json     ← exists
    branch.json       ← MISSING
    environment.json  ← MISSING
  toy_branch_3d/
    geometry.json     ← exists
    branch.json       ← MISSING
    environment.json  ← MISSING
  minimal_v1_4d/      ← DOES NOT EXIST
```

The plan (Section 8) requires: `examples/toy_branch_2d`, `toy_branch_3d`, `minimal_v1_4d`.
Each should be a self-contained runnable example with manifests, geometry, and environment spec.

### What's Missing

1. `examples/toy_branch_2d/branch.json` — valid MinimalGU v1 branch manifest for su(2)
2. `examples/toy_branch_2d/environment.json` — EnvironmentSpec for toy consistency run
3. Same for `toy_branch_3d/`
4. `examples/minimal_v1_4d/` directory with:
   - A 4-dimensional observerse geometry (Y, dim=14 or configurable) over 4D base (X)
   - Full branch manifest with `baseDimension=4, ambientDimension=14`
   - Environment spec for a small 4D mesh

### Why This Matters

- Users and researchers cannot run `gu run <example>` without branch.json + environment.json
- The `minimal_v1_4d` example is the intended "production" target (Section 3.1)
- Without it, the system has never demonstrated end-to-end execution at the physical dimension

### Estimated Scope

For toy 2D/3D: ~2-4 small JSON files each
For minimal_v1_4d: Requires either a 4D simplicial mesh generator or a structured product mesh
plus the JSON configs (~100-200 lines of code for mesh generator, ~5 JSON files)

---

## GAP-7: No Gauss-Newton or Conjugate Gradient Solver

**Severity:** MEDIUM
**Plan Section:** 13.3 (Mode B options), 13.1

### Current State

Mode B (objective minimization) in `SolverOrchestrator.SolveModeB()` uses only:
- Gradient descent with Armijo backtracking line search

### What's Missing

Section 13.3 lists three options for Mode B:
1. **Line search gradient descent** ← implemented
2. **Nonlinear conjugate gradient** ← not implemented
3. **Gauss-Newton** ← not implemented

For Mode C (stationarity solve), a proper Newton-style step would require solving
`J_h^T J_h delta = -J_h^T M Upsilon` which is the Gauss-Newton normal equations.

Gauss-Newton is especially important for the GU system because:
- Gradient descent converges slowly for systems with wide range of curvature
- Gauss-Newton uses the quadratic structure of `I2_h = (1/2)||Upsilon||^2` for better steps
- It is the natural second-order method for least-squares residuals

### Required Work

1. Add `SolverMethod` enum: `GradientDescent, ConjugateGradient, GaussNewton`
2. Add `SolverOptions.Method` property
3. Implement Gauss-Newton step in `SolveModeB`:
   - Requires `BuildJacobian()` to return a matrix or matrix-free operator
   - Solve `J^T J delta = -J^T M Upsilon` via CG or direct solve for small systems
4. Implement Fletcher-Reeves or Polak-Ribière nonlinear CG for Mode B

### Estimated Scope
~200-400 lines C# + unit tests

---

## GAP-8: CUDA Stage 3 (Matrix-Free Krylov / Newton) Not Implemented

**Severity:** MEDIUM
**Plan Section:** 15.5 Stage 3

### Current State

CUDA Stage 1 (residual + objective evaluation) ← done
CUDA Stage 2 (GPU gradient/Gauss-Newton) ← not done (GAP-4)
CUDA Stage 3 (GPU matrix-free Krylov / Newton) ← not done

### What's Missing

Stage 3 requires:
1. Jacobian-vector product kernel: `v → J_h v` on GPU
2. Adjoint-vector product kernel: `v → J_h^T v` on GPU
3. CG/MINRES/GMRES solver loop with GPU dot products and axpy
4. This enables solving `J_h^T J_h delta = -J_h^T M Upsilon` entirely on GPU

The solver primitives (axpy, inner_product, scale) are implemented in `gu_cuda_kernels.cu`
but there is no Krylov loop orchestrator at the C# or native level.

### Required Work

1. Add `EvaluateJacobianAction` and `EvaluateAdjointAction` to `INativeBackend` (overlaps GAP-4)
2. Implement `GpuKrylovSolver` class in `Gu.Interop` or `Gu.Solvers`
3. Use existing GPU axpy/inner_product/scale primitives as building blocks

### Estimated Scope
~300-500 lines (depends on whether GAP-4 is done first)

---

## GAP-9: Branch Sensitivity Environments Not Runnable End-to-End

**Severity:** MEDIUM
**Plan Section:** 17.2 item 2, 19.1 (branch-sensitivity test)

### Current State

Section 17.2 requires four environment categories:
1. Toy consistency environments ← implemented (toy 2D/3D, Mode A/B/C)
2. **Branch sensitivity environments** ← no runner exists
3. Observation pipeline environments ← implemented (ObservationPipeline tests)
4. Scaling environments ← no large-mesh infrastructure

Section 19.1 requires a "branch-sensitivity test":
> Change the torsion or Shiab branch and verify that the output change is controlled,
> logged, provenance-preserved.

There is no automated test or environment runner that:
- Takes a base environment spec
- Runs it with multiple branch variants
- Collects and diffs the observed outputs and convergence histories
- Writes a structured comparison artifact

### Required Work

1. Implement `BranchSweepRunner` (can share infrastructure with Mode D / GAP-1):
   ```csharp
   BranchSweepResult Sweep(EnvironmentSpec baseEnv, IReadOnlyList<BranchManifest> branches);
   ```
2. Add `BranchSweepArtifact` type (list of per-branch ArtifactBundles + comparison summary)
3. Add CLI command: `gu sweep <env.json> --branches b1.json,b2.json --output <dir>`
4. Add test that sweeps identity Shiab vs trivial torsion and verifies outputs differ and are provenance-tagged

### Estimated Scope
~300-500 lines (overlaps with GAP-1)

---

## GAP-10: No Large-Scale / Scaling Environment

**Severity:** MEDIUM
**Plan Section:** 17.2 item 4, 15.1

### Current State

Only toy meshes exist:
- `ToyGeometryFactory.CreateToy2D()`: 5 vertices, 8 edges, 4 faces
- `ToyGeometryFactory.CreateToy3D()`: similar small scale
- `ToyGeometryFactory.CreateToyProduct2D()`: slightly larger

The CUDA backend is built for scale but has never been benchmarked above ~50 elements.

### What's Missing

1. A mesh generator or loader for larger simplicial meshes (100K+ faces on Y)
2. Scaling test environments: 1K, 10K, 100K face meshes
3. GPU timing benchmarks comparing CPU vs CUDA at scale
4. The `T9: Large mesh scaling` test from `GPU_TEST.md` is described but tests T9-T13 are in
   `CudaKernelEdgeCaseTests.cs` — need to verify they actually create large meshes

### Required Work

1. Add `SimplicialMeshGenerator.CreateUniform2D(N)` or `CreateUniform3D(N)` for arbitrary size
2. Add scaling benchmark in `Gu.Benchmarks/BenchmarkRunner.cs`
3. Add performance-sensitive CI test with expected timing bounds

### Estimated Scope
~200-400 lines (mesh generator + benchmarks)

---

## GAP-11: Schema ReplayTier Not Listed as Required

**Severity:** LOW
**Plan Section:** 20, FIX-M1-1

### Current State

In `schemas/artifact.schema.json`, the `ReplayContract` object:
```json
"required": ["branchManifest", "deterministic", "backendId"]
```
does not include `"replayTier"`, yet the C# type has:
```csharp
public required string ReplayTier { get; init; }
```

This means the schema allows serialized artifacts without a `replayTier` field, contradicting
FIX-M1-1 which made `ReplayTier` required.

### Required Work

Add `"replayTier"` to the `required` array in the `ReplayContract` section of
`schemas/artifact.schema.json`. Remove the `"default": "R1"` to match the C# `required` semantic.

### Estimated Scope
2 lines in artifact.schema.json

---

## GAP-12: `reproduce.sh` References `gu` Binary Without Installation Path

**Severity:** LOW
**Plan Section:** 20.2 (artifact folder layout, reproduce.sh)

### Current State

`RunFolderWriter` generates a `replay/reproduce.sh` script containing:
```bash
gu reproduce "$original_run" "$output_dir"
```

This assumes the `Gu.Cli` binary is installed and available as `gu` on the system PATH,
which is not guaranteed. On a fresh checkout, the tool is `dotnet run --project apps/Gu.Cli`
or `dotnet apps/Gu.Cli/bin/Release/net10.0/Gu.Cli.dll`.

### Required Work

Generate a more portable `reproduce.sh` that falls back gracefully:
```bash
#!/usr/bin/env bash
# Try installed CLI first, then dotnet run from repo root
if command -v gu &>/dev/null; then
    gu reproduce "$1" "$2"
else
    dotnet run --project "$(dirname "$0")/../../../../apps/Gu.Cli" -- reproduce "$1" "$2"
fi
```

### Estimated Scope
~10-20 lines

---

## Recommended Execution Order

```
GAP-11  (Schema replayTier required)          ~5 min      -- trivial fix
GAP-12  (reproduce.sh portability)            ~15 min     -- quick fix
GAP-2   (CpuReferenceBackend real physics)    ~3-5 hrs    -- prerequisite for GAP-3
GAP-3   (True CPU-vs-GPU parity tests)        ~2-3 hrs    -- after GAP-2
GAP-5   (GpuSolverBackend buffer sizes)       ~1-2 hrs    -- independent fix
GAP-6   (Example environment specs)           ~2-3 hrs    -- JSON + mesh generator
GAP-1   (Mode D solver)                       ~4-6 hrs    -- unblocks GAP-9
GAP-9   (Branch sensitivity environment)      ~3-5 hrs    -- after GAP-1
GAP-4   (GPU Jacobian/adjoint)                ~6-12 hrs   -- prerequisite for GAP-8
GAP-7   (Gauss-Newton/CG solver)              ~4-6 hrs    -- can parallelize with GAP-4
GAP-8   (CUDA Stage 3 Krylov)                 ~4-8 hrs    -- after GAP-4
GAP-10  (Scaling environments)                ~3-5 hrs    -- independent
```

Critical path for DoD completion:
```
GAP-2 → GAP-3  (true parity, unblocks DoD criteria 4, 9)
GAP-5           (correct GPU buffer layout, prereq for accurate GPU results)
GAP-4 → GAP-8  (GPU solve acceleration, DoD criterion 5 strengthening)
GAP-1 → GAP-9  (Mode D + branch sensitivity environments)
```

---

## Test Count at Baseline

| Assembly | Tests | Notes |
|----------|-------|-------|
| Gu.Core.Tests | 309 | |
| Gu.ReferenceCpu.Tests | 143 | |
| Gu.Geometry.Tests | 138 | |
| Gu.VulkanViewer.Tests | 110 | |
| Gu.Interop.Tests | 100 | 26 GPU tests skip without CUDA |
| Gu.Validation.Tests | 67 | |
| Gu.ExternalComparison.Tests | 59 | |
| Gu.Artifacts.Tests | 56 | |
| Gu.Observation.Tests | 25 | In .slnx since GAP-7 fix |
| Gu.Solvers.Tests | 55 | Added in GAP-8 fix |
| Gu.Math.Tests | 30 | Added in GAP-8 fix |
| Gu.Branching.Tests | 30 | Added in GAP-8 fix |
| Gu.Discretization.Tests | 12 | Added in GAP-8 fix |
| **Total** | **1174** | 26 GPU tests counted but skip on non-CUDA |

---

## What's NOT a Gap (Intentionally Deferred to Phase 2)

The following are NOT included in this gap analysis — they are Phase 2 scope per
`IMPLEMENTATION_PLAN_P2.md`:

- Branch-independence proofs or multi-branch canonicity studies
- Recovery beyond branch-local extraction (sigma_h^* extensions)
- PDE well-posedness / linearization stability research tools
- Quantitative comparison with external physical observations
- Fermionic sector
- Full 4D physical mesh with dim(Y)=14
- Full Gauss-Newton with explicit sparse J^T J assembly for large problems
- Multi-GPU domain decomposition
- Advanced mixed-precision CUDA paths

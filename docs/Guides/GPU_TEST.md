# GPU Test Plan: Real CUDA Parity Verification

**Date:** 2026-03-09
**Status:** NOT IMPLEMENTED
**Hardware:** NVIDIA RTX 4080 Super (compute_89), CUDA 13.1, Vulkan 1.4.341

---

## Problem Statement

All existing interop and parity tests use `CpuReferenceBackend` (a pure C# in-memory implementation) as the underlying native backend. No test loads `libgu_cuda_core.so` via P/Invoke or runs any kernel on GPU hardware.

Specifically:
- `CudaSolverBackendTests.cs` wraps `CpuReferenceBackend` (line 93)
- `NativeParityTests.cs` compares `CpuReferenceBackend` vs `CpuReferenceBackend` (line 50)
- `GpuSolverBackendTests.cs` wraps `CpuReferenceBackend` (line 40)
- `BenchmarkRunner.cs` uses `CpuReferenceBackend` as "stand-in" for GPU (line 97)

The CUDA library compiles and links, but **zero tests verify that the native kernels produce correct physics results when called via P/Invoke**.

---

## What Needs to Be Implemented

### 1. Real CUDA Parity Test (HIGH PRIORITY)

**File:** `tests/Gu.Interop.Tests/CudaGpuParityTests.cs`

A test class that loads `CudaNativeBackend` (which P/Invokes into `libgu_cuda_core.so`) and compares kernel outputs against the C# reference implementations.

#### Prerequisites
- `libgu_cuda_core.so` must be built with `-DGU_ENABLE_CUDA=ON` (or at minimum the CPU-fallback C path)
- The `.so` must be discoverable by the .NET runtime (copy to test output dir, set `LD_LIBRARY_PATH`, or use a `NativeLibrary.SetDllImportResolver`)
- Tests should be skipped (not failed) if the library isn't available

#### Test Setup (per-test or per-class)
```
1. Create a ToyGeometryFactory mesh (e.g., CreateToy2D())
2. Create LieAlgebra via LieAlgebraFactory.CreateSu2WithTracePairing()
3. Pack mesh via MeshTopologyData.FromMesh(mesh, dimG: 3)
4. Pack algebra via AlgebraUploadData.FromLieAlgebra(algebra)
5. Initialize CudaNativeBackend with ManifestSnapshot
6. Upload mesh topology via backend.UploadMeshTopology(topologyData)
7. Upload algebra via backend.UploadAlgebraData(algebraData)
8. Upload background connection A0 (zero or known values)
9. Verify backend.HasPhysicsData == true
```

#### Test Cases

**T1: Curvature kernel parity**
```
- Create omega (edge_count * dimG doubles, known values or random with fixed seed)
- CPU: Run CurvatureAssembler.Assemble(omega, mesh, algebra)
- GPU: Allocate buffers, upload omega, call EvaluateCurvature, download result
- Compare element-wise via ParityChecker.CompareResults()
- Tolerance: 1e-12 (same precision, no floating-point reordering expected)
```

**T2: Augmented torsion kernel parity**
```
- Create omega and A0 (both edge_count * dimG doubles)
- CPU: Run AugmentedTorsionCpu.Evaluate(omega, a0, mesh, algebra)
- GPU: Upload omega + A0, call EvaluateTorsion, download result
- Compare via ParityChecker.CompareResults()
- Verify bracket term has NO 0.5 factor (compare against curvature to confirm difference)
```

**T3: Identity Shiab kernel parity**
```
- GPU: Upload omega, call EvaluateShiab, download result
- Verify output == curvature output (S = F for identity Shiab)
```

**T4: Residual kernel parity**
```
- GPU: Compute full pipeline (curvature -> torsion -> shiab -> residual)
- CPU: Compute same pipeline via C# reference
- Compare residual Upsilon = S - T
```

**T5: Objective kernel parity**
```
- GPU: Compute objective I2_h = (1/2) sum(r_i^2) from residual buffer
- CPU: Compute same from C# reference residual
- Compare scalar via ParityChecker.CompareScalar()
```

**T6: Solver primitives (axpy, inner_product, scale)**
```
- Allocate GPU buffers with known data
- Run Axpy(y, alpha, x, n): verify y_i = y_i + alpha * x_i
- Run InnerProduct(u, v, n): verify result = sum(u_i * v_i)
- Run Scale(x, alpha, n): verify x_i = alpha * x_i
- Compare against hand-computed expected values
```

**T7: Full solver parity (Mode A)**
```
- Run SolverOrchestrator.Solve with CudaNativeBackend in Mode A
- Run same with CpuSolverPipeline
- Compare: residual, objective, all derived fields
```

**T8: Full solver parity (Mode B)**
```
- Use su(2) with trace pairing (positive-definite metric required)
- Run Mode B solver on both backends for 5-10 iterations
- Compare convergence history: objective values at each iteration
- Verify objective decreases monotonically on both
```

---

### 2. Library Discovery and Skip Logic

**File:** `tests/Gu.Interop.Tests/CudaTestFixture.cs` (or similar)

Tests must gracefully handle environments without the CUDA-built library:

```
- Attempt to create CudaNativeBackend and call Initialize
- If DllNotFoundException or EntryPointNotFoundException: skip all GPU tests
- If native library loads but gu_initialize returns error: skip with message
- Use [Trait("Category", "GPU")] to allow filtering: dotnet test --filter Category=GPU
```

This prevents CI failures on machines without CUDA while allowing explicit GPU test runs.

---

### 3. Native Library Deployment

The CUDA-built `libgu_cuda_core.so` must be available to the test runner.

Options (implement at least one):
1. **Copy to output:** Post-build step in `Gu.Interop.Tests.csproj` that copies from `native/build/libgu_cuda_core.so` to test output directory
2. **NativeLibrary resolver:** Register a `DllImportResolver` in test assembly initialization that searches `native/build/`
3. **LD_LIBRARY_PATH:** Document that tests require `export LD_LIBRARY_PATH=<repo>/native/build:$LD_LIBRARY_PATH`

---

### 4. Benchmark Update (MEDIUM PRIORITY)

**File:** `apps/Gu.Benchmarks/BenchmarkRunner.cs`

The parity benchmark at line 96-97 uses two `CpuReferenceBackend` instances. Update to:
```
- reference: CpuReferenceBackend (C# in-memory)
- target: CudaNativeBackend (real native library)
- Upload mesh topology + algebra to target before running parity
- Report actual CPU vs GPU timings
```

Also update `RunSolveBenchmark` (line 56) to optionally use `CudaNativeBackend` instead of `CpuReferenceBackend`.

---

### 5. CUDA-Specific Kernel Tests (LOWER PRIORITY)

**File:** `tests/Gu.Interop.Tests/CudaKernelEdgeCaseTests.cs`

Edge cases specific to GPU execution:

**T9: Large mesh scaling**
```
- Create meshes with 1K, 10K, 100K faces
- Verify curvature kernel produces correct results at scale
- This exercises GPU thread dispatch (blockDim, gridDim)
```

**T10: Zero omega**
```
- omega = 0 everywhere
- Verify F = 0 (d(0) + (1/2)[0,0] = 0)
- Verify T = 0, S = 0, Upsilon = 0, objective = 0
```

**T11: Flat connection (A0 = omega)**
```
- Set A0 = omega (connection equals background)
- Augmented torsion: alpha = omega - A0 = 0, so T^aug = 0
- Curvature still non-zero (depends only on omega)
```

**T12: su(3) algebra**
```
- 8-dimensional Lie algebra
- Larger structure constant array (8^3 = 512 entries)
- Tests that kernel handles dimG > 3 correctly
```

**T13: Memory stress**
```
- Allocate maximum buffers, run kernels, free, repeat
- Verify no memory leaks via buffer handle tracking
```

---

## Implementation Notes

### Physics Data Gate in Native Code

The native kernels (`gu_cuda_core.c` lines 285-407) check physics data availability before dispatching:

```c
if (p->has_topology && p->has_structure_constants) {
    // Real physics kernel
    gu_curvature_assemble_physics(...);
} else {
    // Fallback stub: F = omega (identity)
    memcpy(...);
}
```

**If physics data is not uploaded, the kernels silently fall back to stubs.** Tests MUST verify `HasPhysicsData == true` before asserting physics correctness.

### Buffer Layout

- omega is edge-valued: length = `edge_count * dimG`
- curvature/torsion/shiab are face-valued: length = `face_count * dimG`
- Indexed as `[entity_index * dimG + algebra_index]`

The native API uses buffer handles (int32), not raw pointers. Workflow:
```
handle = AllocateBuffer(totalElements, bytesPerElement=8)
UploadBuffer(handle, data, byteCount)
EvaluateCurvature(omegaHandle, curvatureHandle)
DownloadBuffer(curvatureHandle, resultArray, byteCount)
FreeBuffer(handle)
```

### Critical Physics Constants

- Curvature: `F = d(omega) + 0.5 * [omega, omega]` -- the 0.5 is CRITICAL
- Torsion: `T^aug = d(alpha) + [A0 wedge alpha]` -- NO 0.5 on bracket (unlike curvature)
- su(2) structure constants: `f^c_{ab} = epsilon_{abc}` (Levi-Civita)
- Trace pairing metric: `g_{ab} = delta_{ab}` (use this, not Killing form, for Mode B)

### Test Mesh Details

`ToyGeometryFactory.CreateToy2D()` produces:
- 5 vertices in 2D (unit square corners + center)
- 8 edges (boundary + diagonals)
- 4 triangular faces
- All face boundary edges and orientations pre-computed
- Small enough for hand-verification

For su(2) on this mesh: omega has `8 * 3 = 24` coefficients, curvature has `4 * 3 = 12` coefficients.

---

## Execution Order

```
1. Implement CudaTestFixture with library detection + skip logic
2. Implement T1 (curvature parity) -- this validates the core physics kernel
3. Implement T2-T5 (remaining kernel parity)
4. Implement T6 (solver primitives)
5. Implement T7-T8 (full solver parity)
6. Update benchmark runner
7. Implement T9-T13 (edge cases)
```

**T1 is the critical path.** If curvature parity passes between `CurvatureAssembler.cs` and `gu_curvature_assemble_physics()` on a real mesh with real structure constants, the entire interop pipeline is validated end-to-end.

---

## Definition of Done

- [ ] At least T1-T6 implemented and passing on RTX 4080
- [ ] Tests skip gracefully on machines without CUDA
- [ ] Tests filterable via `dotnet test --filter Category=GPU`
- [ ] Native library automatically found by test runner
- [ ] Benchmark runner can use real CUDA backend
- [ ] All existing 1148 tests still pass (no regressions)

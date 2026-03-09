# Phase 1 Gaps Analysis

**Date:** 2026-03-09
**Baseline:** Commit 7e39180 (main), 982 tests passing (0 warnings, 0 errors)
**Reference:** IMPLEMENTATION_PLAN.md (1830 lines, 13 milestones)

---

## Summary

Phase 1 (M0-M12) C# orchestration layer is **complete and well-tested**. All CPU-side
physics, solver modes, observation pipeline, artifact serialization, validation, and
external comparison work end-to-end in tests. The gaps fall into two categories:

1. **CLI workflow gaps** -- the solver works programmatically but the CLI can't drive it
2. **Native/GPU gaps** -- interop plumbing exists but native kernels are stubs

The Vulkan viewer and CUDA backends are structurally complete (interfaces, P/Invoke,
CMake build, data preparation) but the actual compute kernels and rendering pipeline
are identity/zero stubs, not real physics implementations.

---

## GAP-1: CLI Missing `run` / `solve` Command

**Severity:** HIGH (DoD criterion 12: "System can run EnvironmentSpec objects")
**Milestones:** M6, M8, M12

### Current State
The CLI (`apps/Gu.Cli/Program.cs`) has 5 commands:
- `create-branch` -- writes empty BranchManifest JSON
- `create-environment` -- writes empty EnvironmentSpec JSON
- `init-run` -- creates canonical run folder structure
- `validate-replay` -- compares two run folders at R0/R1/R2/R3
- `verify-integrity` -- computes/verifies SHA-256 hashes

### What's Missing
No command executes the solver. A user cannot:
```bash
gu run <run-folder>                    # execute solver from initialized run folder
gu solve <branch.json> -o <run-folder> # one-shot: create + solve + write artifacts
```

### Required Work
1. Add `run` command that:
   - Reads BranchManifest + EnvironmentSpec from run folder
   - Creates geometry from spec (ToyGeometryFactory or mesh file)
   - Instantiates correct branch operators (torsion, Shiab) from manifest
   - Runs SolverOrchestrator with configured SolverOptions
   - Runs ObservationPipeline on solver output
   - Writes all artifacts via RunFolderWriter
   - Computes integrity hashes
   - Emits solver log
2. Wire up `--backend cpu|cuda` flag
3. Wire up `--mode A|B|C` flag
4. Add `--lie-algebra su2|su3` flag
5. Consider `solve` convenience command (init + run in one step)

### Dependencies
- Gu.Cli needs references to: Gu.ReferenceCpu, Gu.Solvers, Gu.Geometry,
  Gu.Observation, Gu.Math, Gu.Branching (currently only references Core, Artifacts, Validation)
- EnvironmentSpec → geometry mapping needs factory or loader

### Estimated Scope
~200-300 lines of CLI wiring + csproj reference additions. No new physics code needed;
all building blocks exist and are tested (EndToEndPipelineTests proves the chain works).

---

## GAP-2: CUDA Kernels Are Physics Stubs

**Severity:** HIGH (DoD criteria 4, 5, 9)
**Milestones:** M9, M10

### Current State
Native code in `native/` builds via CMake and produces `libgu_cuda_core.so`. The C
implementation (`gu_cuda_core.c`) has proper lifecycle, buffer management, and error
handling. But all compute kernels are stubs:

| Kernel | Stub Behavior | Required Behavior |
|--------|---------------|-------------------|
| `gu_evaluate_curvature` | `F = omega` (memcpy) | `F = d(omega) + (1/2)[omega, omega]` |
| `gu_evaluate_torsion` | `T = 0` (memset) | Branch-dependent torsion operator |
| `gu_evaluate_shiab` | `S = omega` (memcpy) | Branch-dependent Shiab operator |
| `gu_evaluate_residual` | `Upsilon = S - T` | Correct (production-ready) |
| `gu_evaluate_objective` | `I2 = (1/2) sum(r^2)` | Correct (production-ready) |

CUDA kernel file (`gu_cuda_kernels.cu`) has proper kernel signatures and dispatch
functions but the kernel bodies are identity/zero operations. The M10 solver primitives
(axpy, inner_product, scale) return `-1` in CUDA mode (not implemented).

### What's Missing
1. **Mesh topology transfer**: Native layer receives only dimensions via
   `gu_manifest_snapshot_t` (BaseDimension, AmbientDimension, etc.) but not:
   - Face-edge incidence arrays
   - Boundary orientation arrays
   - Vertex coordinates
   - Edge-vertex connectivity
2. **Lie algebra data transfer**: No mechanism to send structure constants or invariant
   metric to native layer
3. **Real curvature kernel**: Needs face-edge incidence + orientations + structure
   constants to compute `d(omega)` and bracket terms
4. **Real torsion kernel**: Needs background connection A0 + branch operator
5. **Real Shiab kernel**: Depends on curvature output + branch operator
6. **CUDA solver primitives**: axpy, inner_product, scale via cuBLAS or custom kernels

### Required Work
1. Extend `gu_manifest_snapshot_t` or add separate upload functions for:
   - Face-edge incidence + orientations (int arrays)
   - Structure constants (double array, dim^3 elements)
   - Invariant metric (double array, dim^2 elements)
   - Background connection A0 (double array)
2. Implement real curvature kernel matching `CurvatureAssembler.cs` logic
3. Implement at least trivial + augmented torsion kernels
4. Implement identity Shiab kernel
5. Implement M10 solver primitives (consider cuBLAS linkage)
6. Add CPU-fallback implementations that do real physics (not just stubs) for
   testing on machines without CUDA

### Dependencies
- CUDA toolkit for GPU compilation
- Corresponding updates to C# NativeBindings.cs and CudaNativeBackend.cs
- Update FieldPacker to transmit mesh topology

### Estimated Scope
~800-1200 lines of C/CUDA code. The C# side (CurvatureAssembler.cs) is the
authoritative reference implementation -- kernels must match it.

---

## GAP-3: Vulkan Rendering Pipeline Not Implemented

**Severity:** MEDIUM (DoD criterion 11)
**Milestone:** M11

### Current State
The data preparation layer is fully functional:
- `ColorMapper` -- maps scalars to color palettes (viridis, plasma, coolwarm, etc.)
- `ScalarFieldVisualizer` -- converts field tensors to colored mesh data
- `MeshExporter` -- exports meshes to OBJ/PLY/STL formats
- `ConvergencePlotter` -- exports convergence history to CSV
- `ViewPayloadBuilder` -- prepares GPU-ready vertex/index data
- `ArtifactViewerService` -- loads and navigates artifact run folders
- `WorkbenchSession` -- manages viewer lifecycle

All 110 VulkanViewer tests pass. But `VulkanContext` and `gu_vulkan_bridge.c` are
complete stubs -- no actual Vulkan instance, device, swapchain, render pass, or
pipeline is created.

### What's Missing
1. VkInstance creation with Vulkan 1.4
2. Physical device selection + logical device creation
3. Swapchain or offscreen framebuffer
4. Render pass + graphics pipeline (triangle mesh + wireframe + 2D overlay)
5. Vertex/index buffer upload from VisualizationData
6. Command buffer recording and submission
7. Image readback for RenderToFile

### Required Work
1. Implement `VulkanContext` methods using Vulkan C API via P/Invoke or Silk.NET
2. Implement `gu_vulkan_bridge.c` with real Vulkan calls
3. Add render pass for colored triangle mesh + optional wireframe overlay
4. Add 2D overlay pass for convergence plots
5. Add offscreen rendering for image export

### Dependencies
- Vulkan SDK/runtime
- Decision on managed Vulkan bindings (raw P/Invoke vs Silk.NET)
- GPU with Vulkan 1.4 support for testing

### Estimated Scope
~1500-2500 lines (C + C#). Well-scoped since data preparation is done and the
bridge API is already defined.

---

## GAP-4: No Replay Reproduction Command

**Severity:** MEDIUM (Plan M8 deliverable)
**Milestone:** M8

### Current State
- `validate-replay` CLI command compares two existing run folders
- `ReplayContractValidator` checks consistency at R0/R1/R2/R3 tiers
- Artifact bundles include `ReplayContract` with `BackendId`, `ReplayTier`, `Deterministic`

### What's Missing
1. No `reproduce` CLI command that re-executes a run from stored artifacts
2. No `reproduce.sh` script generation (plan lists this in M8 artifact structure)
3. No way to load a completed run folder and re-run the solver with identical parameters

### Required Work
1. Add `reproduce` CLI command:
   - Reads BranchManifest, EnvironmentSpec, InitialState from existing run folder
   - Re-executes solver with identical configuration
   - Writes output to new run folder
   - Runs `validate-replay` between original and reproduction
2. Add `reproduce.sh` generation to RunFolderWriter (shell script that calls `gu reproduce`)
3. Depends on GAP-1 (`run` command) being implemented first

### Estimated Scope
~100-150 lines on top of GAP-1.

---

## GAP-5: Schema Validation Not Runtime-Enforced

**Severity:** LOW
**Milestone:** M0

### Current State
Five JSON schemas exist in `schemas/`:
- `branch.schema.json` (4286 bytes)
- `geometry.schema.json` (2203 bytes)
- `artifact.schema.json` (4286 bytes)
- `observed.schema.json` (1954 bytes)
- `validation.schema.json` (1191 bytes)

No C# code validates artifacts against these schemas at runtime. The schemas are
documentation-only.

### Required Work
1. Add NJsonSchema or JsonSchema.Net NuGet package
2. Add `SchemaValidator` utility class
3. Validate on artifact write (RunFolderWriter) and read (RunFolderReader)
4. Add CLI `validate-schema <file> <schema>` command

### Estimated Scope
~100-150 lines + NuGet dependency.

---

## GAP-6: Gu.Symbolic Empty Project

**Severity:** LOW
**Milestone:** N/A (not in plan milestones)

### Current State
`src/Gu.Symbolic/Gu.Symbolic.csproj` exists in solution with an empty `<Project>` tag.
Zero source files. Unclear purpose -- possibly intended for symbolic algebra checks or
expression manipulation.

### Required Work
Either:
- Remove from solution if not needed for Phase 1
- Define scope and implement if needed (e.g., symbolic Lie bracket verification,
  structure constant derivation, or expression-level cross-checks)

### Estimated Scope
Depends on scope decision. Removal: 1 line change. Implementation: TBD.

---

## GAP-7: Gu.Observation.Tests Not Discovered by Solution-Level Test Run

**Severity:** LOW
**Milestone:** M7

### Current State
`Gu.Observation.Tests` (25 tests) passes when run directly:
```bash
dotnet test tests/Gu.Observation.Tests/ --no-build  # 25 passed
```
But does not appear in `dotnet test` output when run at solution level. Likely the
project is not referenced in `GeometricUnity.slnx`.

### Required Work
1. Add `Gu.Observation.Tests` to `GeometricUnity.slnx` test folder
2. Verify it runs in the full test suite

### Estimated Scope
1-2 line change in .slnx file.

---

## GAP-8: Missing Dedicated Test Projects

**Severity:** LOW (tested indirectly, no untested code paths identified)

### Current State
Four source projects lack dedicated test projects:
- `Gu.Math` (2 files) -- tested via Gu.Core.Tests (LieAlgebraTests, LieAlgebraFactoryTests)
- `Gu.Branching` (8 files) -- tested via Gu.Core.Tests (BranchManifestValidatorTests, etc.)
- `Gu.Solvers` (5 files) -- tested via Gu.Core.Tests (SolverAcceptanceTests) and Gu.ReferenceCpu.Tests
- `Gu.Discretization` (3 files) -- tested via Gu.Geometry.Tests

### Required Work
Create dedicated test projects for higher isolation. Priority order:
1. `Gu.Solvers.Tests` -- SolverOrchestrator unit tests, options validation
2. `Gu.Math.Tests` -- vector/matrix ops, algebra factory edge cases
3. `Gu.Branching.Tests` -- validator edge cases, registry isolation
4. `Gu.Discretization.Tests` -- basis family, quadrature rule unit tests

### Estimated Scope
~200-400 lines per test project. Not blocking but improves isolation.

---

## GAP-9: Native Interop Data Transfer Incomplete

**Severity:** HIGH (prerequisite for GAP-2)
**Milestone:** M9

### Current State
`ManifestSnapshot` / `NativeManifestSnapshot` only transmits:
```csharp
public int BaseDimension;
public int AmbientDimension;
public int LieAlgebraDimension;
public int MeshCellCount;
public int MeshVertexCount;
```

The native layer has no way to receive:
- Edge count, face count
- Face-edge incidence arrays + boundary orientations
- Vertex coordinates
- Structure constants (dim^3 doubles)
- Invariant metric (dim^2 doubles)
- Background connection A0

### Required Work
1. Extend `gu_manifest_snapshot_t` or add new upload functions:
   ```c
   gu_error_code_t gu_upload_mesh_topology(
       const int32_t* face_boundary_edges, const int32_t* face_boundary_orientations,
       int32_t face_count, int32_t max_edges_per_face);
   gu_error_code_t gu_upload_structure_constants(
       const double* f_abc, int32_t dim);
   gu_error_code_t gu_upload_invariant_metric(
       const double* g_ab, int32_t dim);
   ```
2. Corresponding C# P/Invoke bindings in NativeBindings.cs
3. Update CudaNativeBackend.Initialize() to upload all topology+algebra data
4. Update FieldPacker to handle topology arrays

### Estimated Scope
~200-300 lines C + ~100 lines C#. Must be done before GAP-2 CUDA kernels.

---

## Definition of Done Checklist

| # | Criterion | Status | Blocking Gap |
|---|-----------|--------|-------------|
| 1 | Branch manifest frozen and serialized | DONE | -- |
| 2 | All required lowering maps exist | DONE | -- |
| 3 | CPU reference backend evaluates residuals and solves | DONE | -- |
| 4 | CUDA residual parity passes for benchmarks | NOT MET | GAP-2, GAP-9 |
| 5 | At least one CUDA solve path exists | NOT MET | GAP-2 |
| 6 | Observed outputs exclusively through sigma_h* | DONE | -- |
| 7 | Validation bundles generated for all runs | DONE | -- |
| 8 | Artifact packages replay at R2 on CPU | DONE | -- |
| 9 | Cross-backend parity benchmark reaches R3 | NOT MET | GAP-2, GAP-9 |
| 10 | Failure cases preserved as artifacts | DONE | -- |
| 11 | Visualization is artifact-driven and read-only | PARTIAL | GAP-3 |
| 12 | System can run EnvironmentSpec and emit typed outputs | NOT MET (CLI) | GAP-1 |

**Score: 8/12 fully met, 1 partial, 3 not met**

---

## Recommended Execution Order

```
GAP-7  (Observation tests in .slnx)          ~5 min      -- quick fix
GAP-6  (Remove or scope Gu.Symbolic)         ~5 min      -- decision
GAP-1  (CLI run command)                     ~2-4 hrs    -- unblocks GAP-4
GAP-4  (Replay reproduction)                 ~1-2 hrs    -- after GAP-1
GAP-5  (Schema validation)                   ~1-2 hrs    -- independent
GAP-9  (Native data transfer)               ~3-5 hrs    -- prerequisite for GAP-2
GAP-2  (CUDA kernels)                        ~8-16 hrs   -- largest item
GAP-3  (Vulkan rendering)                    ~8-16 hrs   -- independent of GAP-2
GAP-8  (Dedicated test projects)             ~4-6 hrs    -- can parallelize
```

GAP-1 is the highest-impact fix: it completes the CLI workflow with minimal code
since all building blocks are already tested. GAP-9 + GAP-2 are the largest items
and require native C/CUDA development.

---

## Updated Test Count

With VulkanViewer tests now building and passing:

| Assembly | Tests | Notes |
|----------|-------|-------|
| Gu.Core.Tests | 309 | |
| Gu.ReferenceCpu.Tests | 143 | |
| Gu.Geometry.Tests | 138 | |
| Gu.VulkanViewer.Tests | 110 | Previously excluded (M11 build errors now resolved) |
| Gu.Interop.Tests | 100 | |
| Gu.Validation.Tests | 67 | |
| Gu.ExternalComparison.Tests | 59 | |
| Gu.Artifacts.Tests | 56 | |
| Gu.Observation.Tests | 25 | Not in .slnx (GAP-7) |
| **Total** | **1007** | **Up from 897** |

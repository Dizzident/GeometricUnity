# CUDA_GAPS.md

# CUDA Implementation Gaps

**Date:** 2026-03-13  
**Scope:** Remaining CUDA-related gaps after the Phase II and Phase IV native parity-path work.  
**Engineering rule:** CPU reference before CUDA trust (IA-5). No GPU result is trusted until it is shown to match the C# reference on real CUDA hardware.

## Current Status

The repository is no longer in the state described by the original gap list.

Implemented since the last review:

- Phase IV `gu_dirac_*` native exports now exist in [gu_cuda_kernels.cu](/home/josh/Documents/GitHub/GeometricUnity/native/gu_cuda_kernels/src/gu_cuda_kernels.cu).
- `Gu.Phase4.Dirac.GpuDiracKernel` is now wired through P/Invoke via [DiracNativeBindings.cs](/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Dirac/DiracNativeBindings.cs).
- Phase II `gu_phase2_cuda` now has non-zero native implementations and explicit upload APIs for topology, structure constants, and background connection.
- Native build wiring now produces both `gu_cuda_core` and `gu_phase2_cuda`.
- Targeted managed tests pass for:
  - [Gu.Phase2.CudaInterop.Tests](/home/josh/Documents/GitHub/GeometricUnity/tests/Gu.Phase2.CudaInterop.Tests/Phase2CudaInteropTests.cs)
  - [Gu.Phase4.Dirac.Tests](/home/josh/Documents/GitHub/GeometricUnity/tests/Gu.Phase4.Dirac.Tests/DiracParityCheckerTests.cs)
  - [Gu.Phase4.Dirac.Gpu.Tests](/home/josh/Documents/GitHub/GeometricUnity/tests/Gu.Phase4.Dirac.Gpu.Tests/DiracParityTests.cs)

What is still missing is real device acceleration, production integration of the Phase II path, and end-to-end validation on CUDA hardware.

---

## CUDA-GAP-1 (High): Phase IV Dirac Path Is Native, But Not Yet Real CUDA Device Execution

### What is true now

The Phase IV Dirac surface is implemented and linkable:

- `gu_dirac_upload_gammas`
- `gu_dirac_gamma_action_gpu`
- `gu_dirac_apply_gpu`
- `gu_dirac_mass_apply_gpu`
- `gu_dirac_chirality_project_gpu`
- `gu_dirac_coupling_proxy_gpu`

Managed dispatch exists in [GpuDiracKernel.cs](/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Dirac/GpuDiracKernel.cs), and `GpuDiracKernel.IsCudaActive` now becomes `true` when the native library and entry points are available.

### What is still missing

These functions currently execute as host-side native parity code inside the CUDA library, not as real GPU kernels:

- gamma matrices are stored in native heap memory, not device `__constant__` memory
- `gu_dirac_gamma_action_gpu` does not launch a CUDA kernel
- `gu_dirac_apply_gpu` does not run on device buffers and does not use the uploaded spin connection as a device-side operator
- `gu_dirac_mass_apply_gpu` and `gu_dirac_chirality_project_gpu` are host implementations
- `gu_dirac_coupling_proxy_gpu` performs a host reduction

So the current state is:

- native parity path: implemented
- real CUDA acceleration: not implemented
- real CUDA hardware parity: not verified

### Additional mismatch to resolve

The current native `gu_dirac_apply_gpu` signature was adjusted to consume `edge_direction_coeff` and `edge_vertices` so it can reproduce the CPU reference matrix-free operator. That is enough for parity, but it is not yet the final device-oriented interface implied by the original document.

### Remaining work

1. Move Phase IV Dirac data to actual device storage.
2. Implement real device kernels for gamma action, Dirac apply, mass apply, chirality project, and coupling reduction.
3. Decide whether the current parity-oriented `edge_direction_coeff` interface is the long-term GPU contract or just an intermediate bridge.
4. Run real CPU-vs-GPU parity on CUDA hardware at the intended tolerance.

### Acceptance criteria

- `gu_dirac_*` functions execute on device memory, not host arrays
- parity holds on real CUDA hardware against [CpuDiracKernel.cs](/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Dirac/CpuDiracKernel.cs)
- `GpuDiracKernel.IsCudaActive` implies actual device execution, not merely native-library availability

---

## CUDA-GAP-2 (Medium): Legacy `Gu.Phase4.CudaAcceleration` Facade Is Still Stub-Only

### What is true now

The newer Phase IV path in [src/Gu.Phase4.Dirac](/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Dirac) is wired.

### What is still missing

The older compatibility layer in [GpuDiracKernelStub.cs](/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.CudaAcceleration/GpuDiracKernelStub.cs) remains a pure CPU-backed stub:

- `ComputedWithCuda => false`
- `VerificationStatus == "stub-unverified"`
- `DiracKernelFactory.CreateGpu()` in `Gu.Phase4.CudaAcceleration` still returns the stub

This means there are now two Phase IV GPU stories in the repo:

- `Gu.Phase4.Dirac.GpuDiracKernel`: native parity path exists
- `Gu.Phase4.CudaAcceleration.GpuDiracKernelStub`: still stub/unverified

### Remaining work

1. Either wire `Gu.Phase4.CudaAcceleration` to the new native Phase IV path.
2. Or explicitly deprecate/remove that facade so there is only one Phase IV GPU abstraction.
3. Update any downstream code that still keys off `ComputedWithCuda` / `VerificationStatus` from the stub layer.

### Acceptance criteria

- no production-facing Phase IV GPU path remains CPU-stub-only
- `DiracKernelFactory` no longer returns a fake GPU implementation for the accelerated path

---

## CUDA-GAP-3 (High): Phase II Native Kernels Exist, But Production Integration Is Still Missing

### What is true now

The Phase II native library is no longer a zero-stub:

- [jacobian_actions.cu](/home/josh/Documents/GitHub/GeometricUnity/native/gu_phase2_cuda/src/jacobian_actions.cu) computes non-zero Jacobian and adjoint actions
- [hessian_actions.cu](/home/josh/Documents/GitHub/GeometricUnity/native/gu_phase2_cuda/src/hessian_actions.cu) computes a Hessian-style action
- [branch_kernels.cu](/home/josh/Documents/GitHub/GeometricUnity/native/gu_phase2_cuda/src/branch_kernels.cu) computes non-zero batched residuals
- [Phase2CudaBackend.cs](/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase2.CudaInterop/Phase2CudaBackend.cs) can now upload the physics data these kernels require

### What is still missing

This path is not yet integrated into the higher-level production CUDA flow:

- `Phase2CudaBackend.UploadPhysicsData(...)` exists, but no non-test caller uses it today
- the main `Gu.Interop` / `gu_cuda_core` path used by `GpuSpectralKernel` and other higher-level components is still separate from `gu_phase2_cuda`
- there is no nontrivial end-to-end parity suite exercising the Phase II native backend with uploaded topology/algebra/background data

### Important semantic limitations still present

The current Phase II implementation is still partial relative to the original design:

- `branch_flags` handling is incomplete:
  - torsion variant bit is used
  - shiab and bi-connection variant bits are not implemented
- the Hessian path currently computes `J^T J v + lambda v`, not the full `J^T M_R J v + lambda C^T M_0 C v`
- there is still no true device-kernel implementation in `gu_phase2_cuda`; the current work establishes native parity behavior and buildability

### Remaining work

1. Integrate `Phase2CudaBackend` into the actual Phase II/III GPU call path.
2. Add production upload of mesh topology, structure constants, and background connection before Phase II kernel use.
3. Implement full branch semantics for shiab and bi-connection variants.
4. Replace the simplified Hessian penalty with the intended gauge-operator form.
5. Add CPU-vs-native and CPU-vs-real-GPU parity coverage for nonzero `omega`.

### Acceptance criteria

- higher-level spectral / solver code can use the Phase II native backend on real nontrivial inputs
- full branch semantics are honored
- Hessian matches the intended mathematical operator, not `J^T J + lambda I`

---

## CUDA-GAP-4 (Low): Phase I and Future Device Paths Still Need Real Hardware Verification

### What is true now

The native build succeeds with CUDA enabled, and Phase I kernels already have device code in [gu_cuda_kernels.cu](/home/josh/Documents/GitHub/GeometricUnity/native/gu_cuda_kernels/src/gu_cuda_kernels.cu).

### What is still missing

There is still no evidence in the repository that the device paths have been validated on actual NVIDIA hardware for:

- Phase I curvature/torsion/solver primitives
- future true-device Phase II kernels
- future true-device Phase IV kernels

### Remaining work

1. Run the GPU parity suites on a CUDA-enabled machine.
2. Record whether the device kernels meet the intended tolerance.
3. Tighten any tolerance or implementation drift discovered under real hardware execution.

---

## Summary

| Gap | Priority | Current state | Remaining blocker |
|-----|----------|---------------|-------------------|
| Phase IV Dirac device execution | High | Native parity path implemented and wired | real device kernels + hardware parity |
| Legacy `Gu.Phase4.CudaAcceleration` stub | Medium | Still CPU-stub-only | wire to native path or remove |
| Phase II production integration | High | Native parity path implemented | not integrated into higher-level runtime, partial semantics |
| Real hardware verification | Low | Native CUDA build succeeds | no end-to-end proof on actual GPU |

The repository has moved from “missing symbols and zero stubs” to “native parity path exists.” The remaining work is now mostly about real device execution, runtime integration, and proof on actual CUDA hardware.

# Phase III — Open Issues for Phase IV

**Date:** 2026-03-12
**Phase III baseline:** 2252 tests passing, all 10 milestones (M23-M32) complete + 12 gaps (PHASE_3_GAPS_01.md) + 11 gaps (PHASE_3_GAPS_02.md) closed.

This document captures known limitations, deferred work, and open questions from Phase III,
as required by §16 deliverable #10 of IMPLEMENTATION_PLAN_P3.md. These are the primary
inputs to the Phase IV planning process.

---

## Summary Table

| Issue    | Area                        | Status         | Plan Reference |
|----------|-----------------------------|----------------|----------------|
| ISSUE-1  | Fermionic Spectrum          | Out of Scope   | §15.1          |
| ISSUE-2  | Full Physical Particle Dict | Out of Scope   | §15.2          |
| ISSUE-3  | QuotientAware (P3) Formul.  | Deferred       | §4.3, §6       |
| ISSUE-4  | Dispersion-Relation Mass    | Deferred       | §4.5, §7.5     |
| ISSUE-5  | GPU CUDA Kernel Linkage     | Deferred       | §8             |
| ISSUE-6  | Cosmological Environments   | Out of Scope   | §15.7          |
| ISSUE-7  | Interaction Proxy Beyond Cubic | Deferred    | §7.6           |
| ISSUE-8  | Global Well-Posedness Proofs| Out of Scope   | §15.6          |
| ISSUE-9  | Symplectic / Canonical Quant| Out of Scope   | §15.3          |
| ISSUE-10 | Background-Independent Conv.| Deferred       | §7, §11        |
| ISSUE-11 | Scattering / S-Matrix       | Out of Scope   | §15.4          |
| ISSUE-12 | Symbolic Derivation Engine  | Out of Scope   | §15.8          |

---

## ISSUE-1: Fermionic Spectrum
**Area:** Spinor / Fermionic sector
**Plan reference:** §15.1
**Status:** Out of Scope
**Description:** Geometric Unity predicts fermions from the geometry of the observerse, but the Phase III system is boson-focused only. Fermionic mode extraction requires spinor bundle infrastructure, a Dirac operator discretization, and chirality-sensitive physical identification — none of which are implemented.
**Why deferred:** Phase III scope was explicitly limited to the bosonic sector (connections, curvature, and their linearized modes). Fermions are a qualitatively different type of field requiring separate bundle and operator infrastructure.
**Phase IV input:** Implement spinor bundles on `Y_h`, discretize the Dirac operator (or its GU-specific analog), add fermionic mode solve pipeline, and extend the candidate registry to include fermionic claim classes (C0-C5 analog for half-integer spin).

---

## ISSUE-2: Full Physical Particle Dictionary
**Area:** Physical identification / registry
**Plan reference:** §15.2
**Status:** Out of Scope
**Description:** Phase III produces candidate boson families ranked by claim class C0-C5, but a full dictionary that uniquely matches all Standard Model bosons (photon, W/Z, gluons, Higgs) to geometric sectors requires significantly higher confidence fits, larger environment campaigns, and resolution of the fermionic sector.
**Why deferred:** The Phase III discipline explicitly forbids collapsing candidate identifications into unique physical truth. Only partial matching with acknowledged ambiguity is allowed.
**Phase IV input:** Run large-scale campaigns across cosmologically realistic environments (see ISSUE-6), incorporate fermionic sector data (see ISSUE-1), and design demotion-resistant claim escalation criteria that can promote C1-C3 candidates toward C4/C5 confidence.

---

## ISSUE-3: QuotientAware (P3) Spectral Formulation
**Area:** `Gu.Phase3.Spectra`
**Plan reference:** §4.3, §6
**Status:** Deferred
**Description:** `PhysicalModeFormulation.QuotientAware` is defined in the codebase and guarded with `NotSupportedException`. The P3 formulation is the mathematically cleanest treatment of gauge-invariant modes: it constructs the spectral operator directly on the quotient space `Omega / Gamma_*` rather than projecting into a physical complement (P2). Currently, `--formulation p3` from the CLI throws a documented exception.
**Why deferred:** The P3 path requires new infrastructure: a `QuotientBasis` type (column-null-space complement of the gauge generator), a `QuotientProjectedOperator` restricting H and M to the quotient space, and a restricted eigensolver dispatch branch in `EigensolverPipeline`. This was scoped out to avoid blocking the P2 delivery.
**Phase IV input:** Implement `QuotientBasis` construction, `QuotientProjectedOperator`, and dispatch in `EigensolverPipeline`. Verify that P3 eigenvalues match P2 eigenvalues on toy systems where both are applicable. Remove the `NotSupportedException` guard.

---

## ISSUE-4: True Dispersion-Relation Mass Extraction
**Area:** `Gu.Phase3.Properties`
**Plan reference:** §4.5, §7.5
**Status:** Deferred
**Description:** `DispersionFitMassExtractor` was implemented with a linear interpolation method (fitting mass-like scale as a linear function of background parameter) rather than a true relativistic dispersion relation fit (`omega^2 = m^2 + k^2`). The `ComputeFromDispersion` path exists but requires multiple momentum values `k_j` (from different mesh sizes or periodic wavevectors) as input, which the current `EigensolverPipeline` does not expose.
**Why deferred:** A proper dispersion fit requires multi-wavevector or multi-mesh runs that are not orchestrated by the current pipeline. Single-mesh eigenvalue data does not uniquely determine the dispersion relation.
**Phase IV input:** Expose a wavevector parameter in `EigensolverPipeline`. Run spectra at multiple momenta (varying periodic boundary conditions or mesh refinement levels), collect `(k_j, omega_j^2)` pairs, and pass them to `DispersionFitMassExtractor.ComputeFromDispersion()`. Handle massless (`m^2 ~ 0`) and tachyonic (`m^2 < 0`) cases with documented notes.

---

## ISSUE-5: GPU CUDA Kernel Linkage
**Area:** `Gu.Phase3.CudaSpectra` / native CUDA library
**Plan reference:** §8
**Status:** Deferred
**Description:** `IsCudaAvailable()` always returns `false`. The `CudaSpectra` project defines the native library interface and C# P/Invoke bindings, but the native CUDA kernel `.so`/`.dll` is not built and not linked. All Phase III spectral solves run on CPU. `SpectrumBundle.ComputedWithBackend` is always `"cpu"`, which caps candidates at claim class C1 per the GPU verification enforcement added in PHASE_3_GAPS_01.md GAP-12.
**Why deferred:** Building the native CUDA library requires a GPU build environment (nvcc, CUDA toolkit) and validated kernel implementations. The CUDA spectral kernels (Lanczos on GPU, GPU Jacobi) are non-trivial to implement correctly and test.
**Phase IV input:** Build the native CUDA kernel library (`libgu_cuda_spectra.so`), implement GPU Lanczos and GPU-side Jacobi tridiagonal solve, wire up P/Invoke in `CudaSpectra`, and update `IsCudaAvailable()` to actually probe for the library. Add CPU-vs-GPU parity tests for spectral operations analogous to the Phase I/II parity tests.

---

## ISSUE-6: Cosmological / High-Fidelity Environments
**Area:** Background atlas / environment campaigns
**Plan reference:** §15.7
**Status:** Out of Scope
**Description:** All Phase III campaigns use toy 2D/3D/4D environments with small meshes (O(100) vertices) and flat or analytically simple backgrounds. Cosmologically realistic backgrounds require much larger meshes, actual GR initial-data solutions as background inputs, and physically motivated gauge groups matching those of the Standard Model embedding.
**Why deferred:** Phase III is explicitly limited to structured toy environments to validate the pipeline end-to-end. Realistic environments are a later-phase concern.
**Phase IV input:** Integrate with GR initial-data solvers (e.g., spectral methods or finite-element GR codes) to produce physically motivated background connection fields. Scale mesh generators to O(10^5-10^6) vertices. Run multi-week campaigns across these environments using the `BosonCampaignRunner`.

---

## ISSUE-7: Interaction Proxy Beyond Cubic
**Area:** `Gu.Phase3.Properties` / interaction proxy
**Plan reference:** §7.6
**Status:** Deferred
**Description:** The interaction proxy in `CandidateBosonRecord.InteractionProxy` uses a cubic finite-difference approximation of the bracket structure (`[omega, [omega, omega]]` third-order term). Quartic and higher-order interaction terms — which would be relevant for strong-coupling regimes and Higgs-like self-interactions — are not approximated.
**Why deferred:** Higher-order bracket terms require more evaluations of the non-linear operators and are expensive on current mesh sizes. The cubic proxy was sufficient for C0-level claim classification.
**Phase IV input:** Implement quartic bracket operator approximation. Extend `InteractionProxyRecord` to carry order and magnitude for each term separately. Use these for C3/C4 claim escalation criteria that distinguish self-interacting from non-self-interacting bosonic sectors.

---

## ISSUE-8: Global Well-Posedness and Uniqueness Proofs
**Area:** Mathematical foundations
**Plan reference:** §15.6
**Status:** Out of Scope
**Description:** The core GU equation `D_omega* Upsilon_omega = 0` is solved numerically, but existence and uniqueness of solutions in full generality are open mathematical questions. Phase III provides numerical evidence that solutions exist on the toy environments tested, but makes no claims about global branches, uniqueness of the physical sector, or convergence in the continuum limit.
**Why deferred:** These are hard open problems in geometric analysis (elliptic regularity on non-compact manifolds, global gauge-fixing, infinite-dimensional Morse theory). They are outside the scope of a numerical implementation project.
**Phase IV input:** Engage with mathematicians specializing in elliptic regularity and geometric analysis. The numerical platform (particularly the convergence diagnostics and mode tracking) can provide computational evidence to guide analytical work. Implement Richardson extrapolation to provide numerical convergence estimates.

---

## ISSUE-9: Symplectic / Canonical Quantization
**Area:** Quantum structure
**Plan reference:** §15.3
**Status:** Out of Scope
**Description:** Phase III is entirely classical. The passage to quantum field theory requires implementing a symplectic structure on the space of connections (or its linearization), constructing creation/annihilation operators from the normal modes, and establishing a Fock space representation. None of these are implemented.
**Why deferred:** Canonical quantization requires the classical theory to be sufficiently understood, which is Phase III's contribution. The quantum extension is a qualitatively different layer.
**Phase IV input:** Implement the symplectic 2-form on the tangent space to the space of admissible connections. Use the normal mode data from Phase III (`ModeRecord.Eigenvalue`, `ModeRecord.EigenvectorCoefficients`) to define creation/annihilation operators. Compute one-loop corrections and compare with known QFT results on toy models.

---

## ISSUE-10: Background-Independent Convergence Criteria
**Area:** Numerical analysis / spectral solver
**Plan reference:** §7, §11
**Status:** Deferred
**Description:** Phase III uses fixed mesh refinements (typically 2-3 levels) to assess whether spectral results are stable. Systematic convergence of eigenvalues and mode properties as mesh size `h -> 0` (continuum limit) has not been verified using a principled numerical analysis method. The current refinement sweep in `RefinementSweepRunner` records eigenvalue changes but does not extrapolate to the continuum.
**Why deferred:** Implementing continuum-limit analysis (Richardson extrapolation, Cauchy convergence tests) was not required for Phase III claim classification. The current refinement data is sufficient for C0 numerical stability claims.
**Phase IV input:** Implement Richardson extrapolation on eigenvalue sequences across mesh refinement levels. Compute estimated continuum eigenvalues and error bounds. Use these for claim class escalation: a mode whose continuum-extrapolated eigenvalue is stable to within tolerance could qualify for C2 or C3 promotion.

---

## ISSUE-11: Scattering / S-Matrix Program
**Area:** Interaction amplitudes
**Plan reference:** §15.4
**Status:** Out of Scope
**Description:** Phase III includes cubic interaction proxies but has no machinery for computing actual scattering amplitudes, transition rates, or the S-matrix between candidate boson sectors. These require time-evolution or frequency-domain scattering boundary conditions, neither of which are implemented.
**Why deferred:** The scattering program requires the quantum completion (ISSUE-9) as a prerequisite and is substantially more complex than the linearized normal mode analysis in Phase III.
**Phase IV input:** Once canonical quantization is established (ISSUE-9), implement LSZ reduction formulas for the free field modes found in Phase III. Use the interaction proxies to compute tree-level scattering amplitudes as a first approximation. Validate against known QFT results for scalar and gauge boson scattering.

---

## ISSUE-12: Fully Automatic Symbolic Derivation
**Area:** Operator generation / symbolic support
**Plan reference:** §15.8
**Status:** Out of Scope
**Description:** Phase III generates operators (curvature, Shiab, torsion, Jacobians) numerically from mesh data. A symbolic derivation engine — one that could automatically compute structure constants, wedge product tables, covariant derivative rules, and Jacobian formulas from symbolic manifold/bundle specifications — is not implemented. Currently, operators are hard-coded in C# based on the mathematical derivations in the plan documents.
**Why deferred:** Symbolic computation requires either a CAS integration (Mathematica, SymPy, SageMath) or a purpose-built symbolic engine, both of which are major independent projects. Phase III's scope was purely numerical.
**Phase IV input:** Integrate a symbolic computation backend (e.g., via Python/SymPy interop or a native C# CAS library). Use it to automatically verify operator correctness (compare symbolically-derived Jacobians against finite-difference checks), generate new operator implementations for higher-dimensional gauge groups, and validate the physical content of the GU equations symbolically.

# PHASE_4_GAPS_01.md

# Phase IV Gap Analysis — Round 1

**Date:** 2026-03-12
**Scope:** Gaps between IMPLEMENTATION_PLAN_P4.md requirements and the current state of the Phase IV codebase (M33–M45, P4-C1/C2/C3, CLI, and integration).
**Status at time of analysis:** 2839 tests pass, 0 errors, 0 warnings. Phase IV declared complete per QA sign-off #26.

This document captures gaps that remain despite the QA pass — areas where the implementation diverges from the plan's stated requirements, DoD criteria, or engineering rules. All gaps are completable end-to-end without new physics research; they are implementation/wiring gaps only.

---

## GAP-1 (Critical): 8 Phase IV CLI Commands Missing

### What the plan requires

IMPLEMENTATION_PLAN_P4.md §8 provides concrete `dotnet run --project apps/Gu.Cli` usage examples for 9 CLI commands:

```
build-spin-spec <runFolder> <outSpec>
assemble-dirac <runFolder> --spin-spec <spec> --out <bundle>
solve-fermion-modes <runFolder> --dirac <bundle> --target lowest --count 64
analyze-chirality <runFolder> --modes <modesJson>
analyze-conjugation <runFolder> --modes <modesJson>
extract-couplings <runFolder> --boson-registry <json> --fermion-modes <json>
build-family-clusters <runFolder>
build-unified-registry <runFolder>
report-phase4 <runFolder>
```

### What is implemented

Only one Phase IV CLI command exists in `apps/Gu.Cli/Program.cs`:
- `generate-phase4-report` (line 64)

All other Phase IV pipeline steps are accessible only by running the in-process study (`studies/phase4_fermion_family_atlas_001/Phase4FermionFamilyAtlasStudy.cs`) or writing test harnesses. There is no CLI path for an operator to build a spinor spec, assemble a Dirac operator, or solve fermionic modes against an arbitrary run folder.

### Impact

- The full Phase IV pipeline is not operable as a composed CLI workflow.
- Provenance notes for CLI-driven runs ("consumed from persisted run folder") cannot be generated.
- The Phase IV feature set is inaccessible to the study runner script (`run.sh`) or CI workflows without hard-coded in-process paths.

### What to implement

Add 8 new `case` blocks to `apps/Gu.Cli/Program.cs`. All underlying service classes already exist:

| Command | Primary service class |
|---|---|
| `build-spin-spec` | `GammaMatrixBuilder`, `SpinorRepresentationSpec` |
| `assemble-dirac` | `CpuSpinConnectionBuilder`, `CpuDiracOperatorAssembler` |
| `solve-fermion-modes` | `FermionSpectralSolver`, `FermionSpectralBundleBuilder` |
| `analyze-chirality` | `ChiralityAnalyzer` |
| `analyze-conjugation` | `ConjugationAnalyzer` |
| `extract-couplings` | `CouplingProxyEngine` |
| `build-family-clusters` | `FamilyClusterReportBuilder`, `FermionFamilyAtlasBuilder` |
| `build-unified-registry` | `RegistryMergeEngine` |

Each command should:
1. Accept a `<runFolder>` as the primary input/output path.
2. Load any needed persisted artifacts from the run folder sub-directories (`fermions/`, `particle_registry/`, etc.).
3. Write output artifacts to canonical sub-paths inside the run folder.
4. Emit P4-C1 provenance notes onto the artifact (manifest source, omega source, background ID).
5. Validate output against the relevant `schemas/phase4/*.schema.json`.

The `report-phase4` command can alias to `generate-phase4-report` or be added as a separate case that calls the same handler.

### Acceptance criteria

- `dotnet run --project apps/Gu.Cli -- build-spin-spec <runFolder> <outSpec>` writes a valid `spinor_representation.json`.
- Each command writes its artifact to the canonical sub-path and the artifact validates against its schema.
- Chaining all 8 commands in sequence against a Phase III run folder reproduces the Phase4FermionFamilyAtlas001 study outputs.

---

## GAP-2 (High): FermionObservationSummary Never Fed Into Registry — ObservationConfidence Always 0.0

### What the plan requires

M43 DoD: "At least one comparison campaign involving fermionic candidates completes."
M42 requires claim class promotion/demotion to use observation confidence.
`RegistryMergeEngine`'s `LowObservation` demotion rule fires when `observationConfidence < MinObservationConfidence` for C3+ candidates.

### What is implemented

`RegistryMergeEngine.cs` hardcodes `ObservationConfidence = 0.0` for all non-boson records:

- `BuildFromFamilyCluster()` line 234: `ObservationConfidence = 0.0,`
- `BuildFromFermionFamily()` line 279: `ObservationConfidence = 0.0,`
- `BuildFromCoupling()` line 320: `ObservationConfidence = 0.0,`

`UnifiedRegistryBuilder.cs` line 164 has comment: `ObservationConfidence = 0.0, // not yet through M43`

In the M45 reference study (`Phase4FermionFamilyAtlasStudy.cs`), the `FermionObservationPipeline` is called at line 146–148 and produces `FermionObservationSummary` objects with real `BranchPersistenceScore` values — but these are stored separately in `Phase4StudyResult.FermionObservations` and are **never passed to `UnifiedRegistryBuilder`**. The registry is built before observations are run (line 136–138) and the two are never reconciled.

### Impact

- The `LowObservation` demotion rule (C3+ → C2) never fires for fermion records regardless of actual observation stability. Every fermion cluster is exempt from this check.
- `FermionObservationSummary.IsTrivial`, `BranchPersistenceScore`, and `ObservationNotes` are computed but ignored by the registry.
- This makes `ObservationConfidence`-based claim promotion/demotion a dead code path for the entire fermion sector.

### What to implement

**Option A (minimal, recommended):** Add `IReadOnlyList<FermionObservationSummary>? fermionObservations` parameter to `RegistryMergeEngine.Build()` and `UnifiedRegistryBuilder.Build()`. Build a lookup `clusterId → FermionObservationSummary`. In `BuildFromFamilyCluster()` / `BuildFromFermionFamily()`, set:

```csharp
ObservationConfidence = obs?.BranchPersistenceScore ?? 0.0,
```

Then apply `LowObservation` demotion if this confidence is below threshold.

**Option B:** Re-order the M45 study so registry is built after observations, passing the observation summaries into the builder.

Similarly for `InteractionObservationSummary` → `BuildFromCoupling()`.

### Acceptance criteria

- A fermion cluster with `BranchPersistenceScore = 0.3` (below `MinObservationConfidence = 0.5`) is demoted from C3 to C2 with a `LowObservation` demotion record.
- A fermion cluster with `BranchPersistenceScore = 0.9` is not demoted by the LowObservation rule.
- New unit test in `RegistryMergeEngineTests.cs` verifying both cases.
- The M45 study's registry now reflects observation confidence from the observation pipeline.

---

## GAP-3 (High): No FermionComparisonCampaign in M45 Reference Study

### What the plan requires

M43 DoD: "At least one comparison campaign involving fermionic candidates completes."
M45 requires a complete end-to-end Phase IV pipeline demonstration including step 8 ("Comparison campaign execution").

### What is implemented

`FermionComparisonAdapter`, `FermionComparisonCampaign`, and `FermionComparisonCampaignRunner` all exist and are tested in `tests/Gu.Phase4.Comparison.Tests/`. However:

- The M45 reference study (`Phase4FermionFamilyAtlasStudy.cs`) does not call `FermionComparisonCampaignRunner`.
- Integration tests (`Phase4FermionFamilyAtlas001Tests.cs`) do not assert that any comparison campaign was run or that `FermionComparisonCampaign` records exist in the study output.
- `Phase4StudyResult` does not contain a `FermionComparisonCampaign` field.

### Impact

- The complete M43 DoD is not met in any executed study.
- The end-to-end report (`Phase4Report`) does not include comparison campaign results.
- The design path from "observed fermion cluster → compared against references → result in report" is implemented but never exercised.

### What to implement

1. Add placeholder `FermionCandidateReference` records to the M45 study config (e.g., one "left-chiral cluster reference", one "right-chiral cluster reference").
2. Add a `FermionComparisonCampaignRunner.Run()` call as step 11 in `Phase4FermionFamilyAtlasStudy.cs`, after the observation pipeline.
3. Store `FermionComparisonCampaign` result in `Phase4StudyResult`.
4. Add assertion in `Phase4FermionFamilyAtlas001Tests.cs`:
   - `ComparisonCampaign_HasAtLeastOneRecord` — verifies the campaign produced at least one comparison record.
   - Outcomes are one of: `compatible`, `incompatible`, `underdetermined`, `not-applicable` (never empty).

### Acceptance criteria

- The M45 study runs a `FermionComparisonCampaignRunner` and serializes its output.
- Integration test asserts campaign record count >= 1.
- `Phase4Report` includes a summary of comparison outcomes (already scaffolded).

---

## GAP-4 (Medium): CUDA Dispatch Not Implemented — M44 DoD Not Met

### What the plan requires

M44 DoD: "CPU / CUDA parity is established for key operator paths and diagnostics."

### What is implemented

`GpuDiracKernel.cs` (`src/Gu.Phase4.Dirac/`) delegates all 5 operations to the CPU reference kernel with `TODO(M44-GPU)` markers:

```csharp
// CPU fallback — TODO(M44-GPU): dispatch gu_dirac_gamma_action_gpu when available
// CPU fallback — TODO(M44-GPU): dispatch gu_dirac_apply_gpu when available
// CPU fallback — TODO(M44-GPU): dispatch gu_dirac_mass_apply_gpu when available
// CPU fallback — TODO(M44-GPU): dispatch gu_dirac_chirality_project_gpu when available
// CPU fallback — TODO(M44-GPU): dispatch gu_dirac_coupling_proxy_gpu when available
```

`GpuDiracKernelStub.cs` (`src/Gu.Phase4.CudaAcceleration/`) similarly delegates to CPU.

Current parity tests in `Gu.Phase4.Dirac.Gpu.Tests/DiracParityTests.cs` compare CPU-vs-CPU (not CPU-vs-CUDA).

### Impact

- M44's stated DoD is CPU/CUDA parity; what was delivered is CPU/CPU parity.
- Native kernel declarations in `gu_cuda_kernels.h` are referenced but not linked.
- Any run using `GpuDiracKernel` with `IsCudaActive` produces results indistinguishable from CPU but without the performance benefit CUDA would provide.

### What to implement

This gap is partially by design (CUDA requires hardware) and is addressed in the engineering notes with `TODO(M44-GPU)` markers. The deliverable gap is:

1. **`GpuDiracKernelStub`**: Rename `GpuDiracKernelStub.VerificationStatus` checking: downstream code that creates records using the stub should set `ComputedWithUnverifiedGpu = true`. Currently `GpuDiracKernelStub.ComputedWithCuda = true` even though it's a CPU stub (see GAP-6).
2. **CI annotation**: Add a build note documenting that M44 GPU dispatch is deferred pending CUDA library availability, and that current parity tests are CPU-reference-vs-CPU-reference.
3. **Native stub headers**: Ensure `gu_cuda_kernels.h` function signatures match the current `IDiracKernel` interface, so that when CUDA is wired in, the P/Invoke surface matches.

Full CUDA implementation is out of scope for this gap round (requires CUDA hardware and native library).

### Acceptance criteria for partial closure

- `GpuDiracKernelStub` sets `ComputedWithCuda = false` OR propagates `VerificationStatus = "stub-unverified"` so that `FermionModeRecord.ComputedWithUnverifiedGpu = true` when the stub is used.
- Parity test infrastructure documented clearly as "CPU-vs-CPU until real CUDA library is linked."

---

## GAP-5 (Medium): RegistryMergeEngine Bypassed in M45 Study — Phase III Bosons Not in Unified Registry

### What the plan requires

M42: "Candidate fermion / boson / interaction merges" into a single `UnifiedParticleRegistry`.
M45 step 8: "Unified particle registry emission" — implies bosons from Phase III and fermions from Phase IV are merged.
`RegistryMergeEngine.Build(bosonRegistry, familyClusters, fermionAtlas, couplingAtlas, provenance)` is the designed integration point.

### What is implemented

The M45 reference study (`Phase4FermionFamilyAtlasStudy.cs` line 136–142) uses `UnifiedRegistryBuilder.Build()` which accepts only:
- `fermionClusters` (from Phase IV)
- `bosonCandidates` (hardcoded to `[]` or empty)
- `couplingAtlases`

`RegistryMergeEngine` — which properly accepts a `BosonRegistry` from Phase III and performs the full cross-phase merge — is never called in the study or integration pipeline. It is tested in isolation but not exercised end-to-end.

The `UnifiedParticleRegistry_HasAtLeastOneCandidate` integration test passes but the registry contains only fermion candidates, not bosons. The "unified" claim is not satisfied.

### Impact

- The M45 registry is not truly unified: Phase III bosonic candidates are absent.
- `RegistryMergeEngine`'s boson demotion rules and boson claim class mapping are tested but never run in a real pipeline.
- A researcher examining the unified registry from the M45 study would see no bosons alongside the fermion candidates.

### What to implement

1. In `Phase4FermionFamilyAtlasStudy.cs`, load the Phase III boson registry (or construct a minimal stub boson registry with at least one `CandidateBosonRecord`) before building the unified registry.
2. Replace `UnifiedRegistryBuilder.Build()` call with `RegistryMergeEngine.Build()`, passing the boson registry, family clusters, fermion atlas, and coupling atlas.
3. Update `Phase4StudyResult` to track which Phase III boson registry was consumed.
4. Add integration test assertion: `UnifiedRegistry_ContainsBothBosonsAndFermions`.

### Acceptance criteria

- The unified registry in the M45 study contains at least one boson record (from Phase III source) and at least one fermion record (from Phase IV clustering).
- `RegistryMergeEngine.Build()` is called with a real or minimal-stub boson registry in the study.
- Integration test `UnifiedRegistry_ContainsBothBosonsAndFermions` passes.

---

## GAP-6 (Low): GpuDiracKernelStub Incorrectly Claims ComputedWithCuda = true

### What the plan requires

Engineering rule: CPU reference before CUDA trust.
`CandidateFermionRecord.UnverifiedGpu = true` triggers the `UnverifiedGpu` demotion rule (caps claim class at C1).
`FermionModeRecord.ComputedWithUnverifiedGpu` propagates this flag.

### What is implemented

`GpuDiracKernelStub` (`src/Gu.Phase4.CudaAcceleration/GpuDiracKernelStub.cs`):

```csharp
public bool ComputedWithCuda => true;  // claims GPU
public string VerificationStatus => "stub-unverified";
```

It delegates all operations to `CpuDiracKernel` but returns `ComputedWithCuda = true`. If downstream code checks `kernel.ComputedWithCuda` to decide whether to mark `FermionModeRecord.ComputedWithUnverifiedGpu`, it will incorrectly set it to `false` (trusting a claimed-GPU stub), bypassing the C1 cap.

### What to implement

**Option A (simplest):** Change `GpuDiracKernelStub.ComputedWithCuda` to return `false`:

```csharp
public bool ComputedWithCuda => false;  // CPU fallback stub
```

**Option B:** Keep `ComputedWithCuda = true` but add `IsCudaVerified = false` and ensure mode records set `ComputedWithUnverifiedGpu = !(kernel.IsCudaVerified)`.

The `DiracKernelFactory.CreateGpu()` and `Create(bundle, useCuda: true, modes)` should propagate stub status to the resulting mode records.

### Acceptance criteria

- `GpuDiracKernelStub` usage results in `FermionModeRecord.ComputedWithUnverifiedGpu = true`.
- Fermion modes computed via the stub are capped at claim class C1 in the registry.
- New unit test: `GpuDiracKernelStub_SetsUnverifiedGpuFlag_OnModeRecords`.

---

## GAP-7 (Low): CpuSpinConnectionBuilder Only Supports dimG = 1 and dimG = 3

### What the plan requires

The plan describes a general spin connection builder that handles the gauge algebra dimension for the full GU gauge group. The architecture supports su(2) as the primary test case but also notes su(3) as a target.

### What is implemented

`CpuSpinConnectionBuilder.cs` `AssembleGaugeCoupling()`:

```csharp
if (dimG == 1)  // trivial gauge → identity coupling
if (dimG == 3)  // su(2) → structure constants from Levi-Civita3()
// else: zero gauge coupling (silent fallback)
```

All other gauge dimensions (e.g., `dimG = 8` for su(3)) silently fall back to zero coupling with a comment. This means any Dirac operator built on a su(3) background will have zero gauge coupling regardless of the background state.

### What to implement

Add `dimG == 8` case to `AssembleGaugeCoupling()` using su(3) structure constants `f^{abc}` (already implemented elsewhere in the codebase via `SuNLieAlgebra.GetStructureConstants()` or equivalent). Pattern matches the su(2) implementation:

```csharp
else if (dimG == 8) // su(3)
{
    var f = BuildSu3StructureConstants(); // f^{abc} totally antisymmetric
    // assemble gauge coupling block-by-block
}
```

Add `else { throw new NotSupportedException($"Gauge dimension {dimG} not supported."); }` to make unsupported cases explicit rather than silent.

### Acceptance criteria

- `CpuSpinConnectionBuilder.Build()` with `dimG = 8` produces a non-zero `GaugeCouplingCoefficients` array for a nonzero bosonic background state.
- New unit test: `CpuSpinConnectionBuilder_Su3Gauge_ProducesNonzeroGaugeCoupling`.
- Explicit `NotSupportedException` for all other `dimG` values (prevents silent zero).

---

## GAP-8 (Low): Bosonic Validation Study (P4-C3) Missing Committed Artifact Output

### What the plan requires

P4-C3 Completion Criteria:
- "Study produces nonzero intermediate fields and nontrivial artifact chain"
- "Replayed runs preserve same loaded branch/background identity"
- Suggested deliverables: "Committed study config under `examples/` or `studies/`, scripted runner, Markdown report, regression tests ensuring study keeps producing nontrivial outputs"

### What is implemented

`studies/bosonic_validation_001/` contains:
- `branch.json` ✓
- `environment.json` ✓
- `REPORT.md` ✓
- `run_study.sh` — calls `dotnet test --filter BosonicValidationStudy001` only

The `run_study.sh` does **not** call any `dotnet run --project apps/Gu.Cli` commands and does not produce or commit serialized artifact files. There is no `artifacts/` or `output/` subdirectory with committed run outputs.

The regression test lives in `tests/Gu.Phase3.Spectra.Tests/BosonicValidationStudy001Tests.cs` and runs in-process assertions. However P4-C1's CLI-level criteria ("CLI no longer silently substitutes toy runtime branch for persisted state") can only be verified by running CLI commands against a stored artifact.

### What to implement

1. Add a second section to `run_study.sh` that runs CLI commands:
   ```bash
   dotnet run --project apps/Gu.Cli -- run <study_dir>/branch.json <study_dir>/environment.json --output <study_dir>/output/run1/
   dotnet run --project apps/Gu.Cli -- compute-spectrum <study_dir>/output/run1/ ...
   ```
2. Commit representative output artifacts (or a `reproduce.sh` that generates them deterministically) to `studies/bosonic_validation_001/artifacts/`.
3. Add regression test asserting the CLI-invoked run produces the same background ID / manifest identity as the in-process test (P4-C1 parity check).

### Acceptance criteria

- `run_study.sh` executes at least one CLI command that writes a serialized artifact to `studies/bosonic_validation_001/output/`.
- The written artifact passes schema validation.
- A regression test asserts CLI-driven and in-process-driven runs produce the same background record ID.

---

## Summary Table

| GAP | Priority | Component | Gap Type | Effort |
|-----|----------|-----------|----------|--------|
| GAP-1 | Critical | `apps/Gu.Cli/Program.cs` | 8 Phase IV CLI commands missing | Medium (all logic exists, routing only) |
| GAP-2 | High | `RegistryMergeEngine`, `UnifiedRegistryBuilder`, M45 study | `ObservationConfidence` always 0.0; observation pipeline never feeds registry | Small (add parameter, wire up) |
| GAP-3 | High | M45 study, integration tests | `FermionComparisonCampaign` not run in any study or integration test | Small (add campaign runner call + test) |
| GAP-4 | Medium | `GpuDiracKernel`, `GpuDiracKernelStub` | CUDA dispatch not implemented; M44 DoD not fully met | Large (requires CUDA hardware) |
| GAP-5 | Medium | M45 study, `RegistryMergeEngine` | Phase III bosons absent from unified registry; `RegistryMergeEngine` bypassed | Small (replace `UnifiedRegistryBuilder` call in study) |
| GAP-6 | Low | `GpuDiracKernelStub` | Stub claims `ComputedWithCuda = true` despite CPU-only execution | Trivial (one field change + propagation) |
| GAP-7 | Low | `CpuSpinConnectionBuilder` | Only su(2) and trivial gauge supported; su(3) silently falls back to zero | Small (add su(3) structure constant block) |
| GAP-8 | Low | `studies/bosonic_validation_001/` | P4-C3 study produces no committed CLI-level artifacts | Small (extend run_study.sh + commit outputs) |

## Definition of Done for This Gap Round

A gap round is closed when all Critical and High gaps are resolved:

- [ ] GAP-1: All 8 Phase IV CLI commands implemented in `apps/Gu.Cli/Program.cs` and tested against a Phase III run folder.
- [ ] GAP-2: `ObservationConfidence` populated from `FermionObservationSummary.BranchPersistenceScore`; `LowObservation` demotion verified in new unit test.
- [ ] GAP-3: `FermionComparisonCampaignRunner` invoked in M45 study; integration test asserts campaign record count ≥ 1.
- [ ] GAP-4 (partial): `GpuDiracKernelStub` correctly propagates unverified status; parity tests documented as CPU-vs-CPU.
- [ ] GAP-5: `RegistryMergeEngine.Build()` replaces `UnifiedRegistryBuilder.Build()` in M45 study; registry contains both bosons and fermions.

Medium/Low gaps (GAP-6, GAP-7, GAP-8) can be addressed in the same pass or deferred to a round 2.

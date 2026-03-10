# PHASE_3_GAPS_01.md

# Geometric Unity Phase III — Gap Analysis Round 1

## Summary

This document records gaps found between the Phase III implementation
(M23–M32, commits through 7e29d98) and the requirements in
`IMPLEMENTATION_PLAN_P3.md`.

**M23 (Backgrounds) and M24 (GaugeReduction) are fully complete with no gaps.**

The remaining 8 milestones have gaps of varying severity, listed below
in priority order (critical first).

Each gap entry includes:
- Exact location in the codebase
- Specification reference (section numbers)
- Required deliverable
- Completion criteria / acceptance test

---

## GAP-1: ModeRecord missing required fields

**Severity:** High
**Milestone:** M25/M26 — Spectral operator bundle and CPU spectral solver
**File:** `src/Gu.Phase3.Spectra/ModeRecord.cs`
**Spec ref:** §6.3 ModeRecord

### Problem

`ModeRecord` is missing four fields required by the spec:

| Field | Spec requirement |
|---|---|
| `TensorEnergyFractions` | Block/tensor energy distribution of the mode |
| `BlockEnergyFractions` | Per-state-block energy fraction dictionary |
| `ModeVectorArtifactRef` | Serializable reference to the stored mode vector |
| `ObservedSignatureRef` | Reference to the computed observed mode signature |

These fields are consumed by M28 (Properties) and M29 (Observables) and
referenced in the spec as mandatory in the `ModeRecord` schema.

### Required deliverable

1. Add all four fields to `ModeRecord` with JSON serialization:
   ```csharp
   public Dictionary<string, double>? TensorEnergyFractions { get; init; }
   public Dictionary<string, double>? BlockEnergyFractions { get; init; }
   public string? ModeVectorArtifactRef { get; init; }
   public string? ObservedSignatureRef { get; init; }
   ```
2. Populate `BlockEnergyFractions` in `EigensolverPipeline` by calling
   `PolarizationExtractor` on each eigenvector immediately after solve.
3. Populate `TensorEnergyFractions` — distribute energy per tensor
   signature label from the branch field layout.
4. Set `ModeVectorArtifactRef` to the artifact path when mode vectors
   are written to disk.
5. `ObservedSignatureRef` is set by the Observables pipeline (M29) after
   observation; leave nullable and populate downstream.

### Acceptance criteria

- `ModeRecord` round-trip JSON test includes all four fields.
- `EigensolverPipeline` test verifies `BlockEnergyFractions` sums to 1.0
  (within tolerance) for each returned mode.
- `TensorEnergyFractions` values are non-negative.

---

## GAP-2: ModeTracking missing O2 (observed-signature overlap) metric

**Severity:** High
**Milestone:** M27 — Mode tracking and family construction
**File:** `src/Gu.Phase3.ModeTracking/ModeMatchMetricSet.cs`,
`src/Gu.Phase3.ModeTracking/ModeMatchingEngine.cs`
**Spec ref:** §4.9, §7.6

### Problem

The spec requires three overlap metrics for mode matching:

- O1: native inner product `|v1^T M_state v2|` — **implemented**
- O2: observed signature overlap `similarity(Obs(v1), Obs(v2))` — **missing**
- O3: invariant feature distance — **implemented**

`ModeMatchMetricSet` has no field for O2. The matching aggregate score
therefore cannot incorporate observed-space agreement, which the spec
treats as essential for cross-context matching.

### Required deliverable

1. Add `ObservedSignatureOverlap` (nullable `double`) to `ModeMatchMetricSet`.
2. In `ModeMatchingEngine`, when `ObservedModeSignature` objects are
   provided alongside modes (new optional parameter), compute:
   ```text
   O2 = ObservedOverlapMetrics.L2Overlap(sig_i, sig_j)
   ```
3. Update aggregate score formula when O2 is available:
   ```text
   score = w1*O1 + w2*O2 + w3*O3_feature_score
   ```
   Default weights: `w1=0.3, w2=0.4, w3=0.3`.
4. Add `ObservedOverlapWeight` to `TrackingConfig`.
5. When O2 is absent (signatures not provided), fall back to
   `w1/(w1+w3)*O1 + w3/(w1+w3)*O3_feature_score`.

### Acceptance criteria

- Test: two modes with identical observed signatures get O2=1.0.
- Test: modes with orthogonal observed signatures get O2≈0.0.
- Test: aggregate score changes when O2 is supplied vs absent.
- `ModeMatchMetricSet` serializes O2 field (nullable, omitted when null).

---

## GAP-3: ModeTracking missing split, merge, avoided-crossing detection

**Severity:** High
**Milestone:** M27 — Mode tracking and family construction
**File:** `src/Gu.Phase3.ModeTracking/ModeMatchingEngine.cs`
**Spec ref:** §7.6 "detect births/deaths/splits/merges/avoided crossings"

### Problem

`ModeMatchingEngine.Match()` uses the Hungarian algorithm (one-to-one
matching), which structurally cannot detect splits or merges.
`ModeAlignmentRecord.AlignmentType` supports the values "split",
"merge", and "avoided-crossing" but these are never created.

### Required deliverable

**Splits (one source → multiple targets):**
1. After Hungarian matching, for each unmatched target mode compute its
   overlap score against all source modes.
2. If a target t is unmatched but its best source s already matched
   to another target t', AND both `Score(s,t) >= SplitThreshold` and
   `Score(s,t') >= SplitThreshold`, record both as `AlignmentType="split"`
   with source `s`.
3. Add `SplitThreshold` (default 0.4) to `TrackingConfig`.

**Merges (multiple sources → one target):**
1. Symmetric: if two unmatched sources both score above threshold against
   one matched target, record both as `AlignmentType="merge"`.

**Avoided crossings:**
1. Require a list of at least two consecutive matching steps (continuation
   context).
2. Detect avoided crossing when: at step k, mode A has eigenvalue ≈ mode B,
   AND at step k+1 mode A's overlap is higher with the previous B and
   vice versa (track-swap event).
3. Record both alignments as `AlignmentType="avoided-crossing"` with a note.
4. This requires `BuildFamilies()` to run avoided-crossing post-processing
   over the built continuation track.

**Note:** Ambiguity must always be recorded when split/merge/avoided-crossing
is detected. Do not force one-to-one resolution.

### Acceptance criteria

- Test: two target modes from one source → both get `AlignmentType="split"`.
- Test: two source modes converging to one target → both get
  `AlignmentType="merge"`.
- Test: two modes whose eigenvalues cross between steps → both get
  `AlignmentType="avoided-crossing"`.
- Test: split/merge events appear in `CrossBranchModeMap` summary counts.
- Test: split/merge result in `AmbiguityCount > 0` in the family record.

---

## GAP-4: CandidateBosonRecord missing three required envelope fields

**Severity:** High
**Milestone:** M30 — Candidate boson registry
**File:** `src/Gu.Phase3.Registry/CandidateBosonRecord.cs`,
`src/Gu.Phase3.Registry/CandidateBosonBuilder.cs`
**Spec ref:** §6.6 `CandidateBosonRecord`

### Problem

Three fields required by the spec are absent from `CandidateBosonRecord`:

| Field | Type | Purpose |
|---|---|---|
| `PolarizationEnvelope` | `(string DominantClass, double MinFraction, double MaxFraction)` | Aggregated polarization across contributing modes |
| `SymmetryEnvelope` | `(int? MinParity, int? MaxParity, List<string> UnionLabels)` | Aggregated symmetry across contributing modes |
| `InteractionProxyEnvelope` | `(double MinCubicResponse, double MaxCubicResponse, int ProxyCount)` | Aggregated interaction proxies |

### Required deliverable

1. Add the three fields to `CandidateBosonRecord` (nullable, JSON-serializable).
2. In `CandidateBosonBuilder`, aggregate envelopes from contributing
   `BosonPropertyVector` objects:
   - `PolarizationEnvelope.DominantClass` = most frequent dominant class;
     fraction range = min/max of `DominanceFraction`.
   - `SymmetryEnvelope.UnionLabels` = union of all `SymmetryLabels`;
     parity range = min/max of `ParityEigenvalue` (null if mixed).
   - `InteractionProxyEnvelope` = min/max of `|CubicResponse|` over all
     `InteractionProxyRecord`s; count = total proxy records.
3. When `BosonPropertyVector` has no interaction proxies, set
   `InteractionProxyEnvelope = null`.

### Acceptance criteria

- `CandidateBosonRecord` JSON round-trip includes all three envelope fields.
- Test: two modes with different polarization classes → `DominantClass` set
  to the majority class; fraction range spans both.
- Test: no proxies → `InteractionProxyEnvelope` is null/omitted in JSON.

---

## GAP-5: Registry missing three demotion rules and registry diffing

**Severity:** Medium
**Milestone:** M30 — Candidate boson registry
**File:** `src/Gu.Phase3.Registry/DemotionEngine.cs`,
`src/Gu.Phase3.Registry/BosonRegistry.cs`
**Spec ref:** §4.13 claim demotion, §11.6 "registry diffing between reruns"

### Problem

`DemotionEngine` implements four rules (gauge leak, refinement fragility,
branch fragility, backend fragility) but the spec also requires:

1. **ObservationInstability** demotion: if `ObservationStabilityScore < threshold`,
   demote by 1.
2. **ComparisonMismatch** demotion: after a comparison campaign returns
   `Incompatible` for all targets, demote to at most C1.
3. **AmbiguousMatching** demotion: if `AmbiguityCount > threshold` in the
   contributing family, demote by 1.

Additionally, `BosonRegistry` has no `Diff()` / `Compare()` capability,
which the spec requires for test coverage: "registry diffing between reruns."

### Required deliverable

**Demotion rules:**
1. Add `ObservationInstabilityThreshold` (default 0.4) to demotion config.
2. Add `AmbiguousMatchingThreshold` (default 2) to demotion config.
3. In `DemotionEngine.ApplyRules()`, check all five conditions.
4. ComparisonMismatch rule is applied externally by the campaign runner
   after running campaigns; add a method
   `ApplyComparisonMismatch(record, allResultsIncompatible)` to
   `DemotionEngine`.

**Registry diffing:**
1. Add `BosonRegistry.Diff(BosonRegistry other)` returning a
   `RegistryDiff` record:
   ```csharp
   record RegistryDiff(
       IReadOnlyList<string> NewCandidateIds,
       IReadOnlyList<string> RemovedCandidateIds,
       IReadOnlyList<ClaimClassChange> ClaimClassChanges,
       IReadOnlyList<DemotionChange> DemotionChanges
   );
   ```
2. `ClaimClassChange`: `(string CandidateId, BosonClaimClass Before, BosonClaimClass After)`.
3. `DemotionChange`: `(string CandidateId, List<BosonDemotionRecord> Added)`.

### Acceptance criteria

- Test: candidate with low observation stability score is demoted.
- Test: candidate with all-incompatible campaign results is capped at C1.
- Test: candidate with many ambiguous matches is demoted.
- Test: `Diff(same_registry, same_registry)` returns empty diff.
- Test: `Diff(old, new)` correctly identifies new, removed, and reclassified
  candidates.

---

## GAP-6: Missing Phase III CLI commands

**Severity:** Medium
**Milestone:** M32 / overall Phase III
**File:** `apps/Gu.Cli/Program.cs`
**Spec ref:** §9.1 New CLI commands

### Problem

The spec requires 7 CLI commands. Only 2 are wired:

| Command | Status |
|---|---|
| `create-background-study` | ✓ wired |
| `solve-backgrounds` | ✓ wired |
| `compute-spectrum` | **missing** |
| `track-modes` | **missing** |
| `build-boson-registry` | **missing** |
| `run-boson-campaign` | **missing** |
| `export-boson-report` | **missing** |

### Required deliverable

For each missing command, add a `case` in `Program.cs` with a handler
method. Handlers should:

1. **`compute-spectrum <runFolder> <backgroundId> [--num-modes N] [--formulation p1|p2]`**
   - Load background from `<runFolder>/backgrounds/<backgroundId>.json`.
   - Load the branch manifest and environment.
   - Build `LinearizedOperatorBundle` via `OperatorBundleBuilder`.
   - Solve for `N` modes (default 10) via `EigensolverPipeline`.
   - Write `SpectrumBundle` to `<runFolder>/spectra/<backgroundId>_spectrum.json`.
   - Write individual `ModeRecord`s to `<runFolder>/spectra/modes/`.

2. **`track-modes <runFolder> [--context continuation|branch|refinement]`**
   - Load all spectrum bundles from `<runFolder>/spectra/`.
   - Run `ModeMatchingEngine.BuildFamilies()` or `CrossBranchModeMap`.
   - Write `mode_families.json` to `<runFolder>/modes/`.

3. **`build-boson-registry <runFolder>`**
   - Load mode families from `<runFolder>/modes/mode_families.json`.
   - Load property vectors (if present).
   - Build `CandidateBosonRecord`s via `CandidateBosonBuilder`.
   - Apply demotion rules.
   - Write `registry.json` to `<runFolder>/bosons/`.

4. **`run-boson-campaign <runFolder> [--campaign <campaignSpec.json>]`**
   - Load registry from `<runFolder>/bosons/registry.json`.
   - Load or generate campaign spec.
   - Run `BosonCampaignRunner`.
   - Write results to `<runFolder>/campaigns/boson_campaigns/`.

5. **`export-boson-report <runFolder> [--output <path>]`**
   - Load registry, campaign results, spectrum sheets.
   - Run `BosonAtlasReportGenerator.Generate()`.
   - Write Markdown report to `<runFolder>/reports/boson_atlas.md`.
   - Write JSON summary to `<runFolder>/reports/boson_atlas.json`.

### Acceptance criteria

- `dotnet run -- compute-spectrum` on a study folder with a valid B1 background
  produces a `SpectrumBundle` file.
- `dotnet run -- build-boson-registry` on a folder with mode families produces
  a `registry.json`.
- `dotnet run -- export-boson-report` on a complete folder produces both `.md`
  and `.json` report files.
- Help text (printed when no args given) lists all 7 commands.

---

## GAP-7: Missing Phase III JSON schemas

**Severity:** Medium
**Milestone:** M32 / overall Phase III
**Files:** `schemas/` directory
**Spec ref:** §9.2 New schemas

### Problem

The following 6 schemas required by the spec are absent from the `schemas/`
directory:

| Schema | Status |
|---|---|
| `background-study.schema.json` | ✓ exists (as `background_study.schema.json`) |
| `background-record.schema.json` | ✓ exists (as `background_record.schema.json`) |
| `spectrum-bundle.schema.json` | **missing** |
| `mode-record.schema.json` | **missing** |
| `mode-family.schema.json` | **missing** |
| `boson-registry.schema.json` | **missing** |
| `boson-campaign.schema.json` | **missing** |
| `boson-report.schema.json` | **missing** |

### Required deliverable

Create all 6 missing schemas following the same structure as existing schemas
in the directory. Each schema must validate the primary serialized type:

1. **`spectrum-bundle.schema.json`** — validates `SpectrumBundle`.
   Required properties: `spectrumId`, `backgroundId`, `operatorBundleId`,
   `operatorType`, `formulation`, `modes`, `clusters`, `convergenceStatus`.

2. **`mode-record.schema.json`** — validates `ModeRecord`.
   Required properties: `modeId`, `backgroundId`, `operatorType`, `eigenvalue`,
   `normalizationConvention`, `gaugeLeakScore`, `nullModeDiagnosis`.
   Optional: `tensorEnergyFractions`, `blockEnergyFractions`,
   `modeVectorArtifactRef`, `observedSignatureRef`.

3. **`mode-family.schema.json`** — validates `ModeFamilyRecord`.
   Required properties: `familyId`, `memberModeIds`, `contextIds`,
   `meanEigenvalue`, `eigenvalueSpread`, `isStable`, `ambiguityCount`.

4. **`boson-registry.schema.json`** — validates `BosonRegistry` (serialized form).
   Required properties: `registryVersion`, `candidates` (array of
   `CandidateBosonRecord`).

5. **`boson-campaign.schema.json`** — validates `BosonCampaignSpec`.
   Required properties: `campaignId`, `targets`, `minClaimClass`,
   `excludeDemoted`.

6. **`boson-report.schema.json`** — validates `BosonAtlasReport`.
   Required properties: `reportId`, `studyId`, `registryVersion`,
   `spectrumSheets`, `stabilitySummaries`, `negativeResults`, `generatedAt`.

### Acceptance criteria

- All 6 schema files exist in `schemas/`.
- The existing `validate-schema` CLI command can validate a real
  `SpectrumBundle` JSON against `spectrum-bundle.schema.json`.
- The existing `validate-schema` CLI command can validate a real
  `BosonAtlasReport` JSON against `boson-report.schema.json`.

---

## GAP-8: Spectra test coverage gaps

**Severity:** Medium
**Milestone:** M25/M26 — Spectral operator bundle and CPU spectral solver
**File:** `tests/Gu.Phase3.Spectra.Tests/`
**Spec ref:** §11.3 Spectral tests

### Problem

The spec mandates 6 categories of spectral tests; several are missing:

| Test category | Status |
|---|---|
| Tiny analytic toy spectra | **missing** |
| CPU matrix-free vs explicit sparse agreement | **missing** |
| Generalized eigen residual checks | **missing** |
| Null-mode detection tests | **missing** |
| Degeneracy clustering behavior | ✓ (SpectralClustererTests) |
| GN vs full-Hessian difference reporting | **missing** |

### Required deliverable

Add a new test file `SpectrumValidationTests.cs`:

1. **Analytic toy spectrum test**
   - Construct a 4×4 H and M analytically with known eigenvalues (e.g.,
     diagonal H = diag(0.1, 1.0, 2.0, 5.0), M = identity).
   - Solve via `EigensolverPipeline`.
   - Assert all 4 eigenvalues match analytic values within tolerance 1e-6.
   - Assert eigenvectors satisfy `||H v - lambda M v|| / ||v|| < 1e-6`.

2. **Generalized eigen residual check test**
   - Use the analytic toy problem.
   - For each returned `ModeRecord`, verify `ResidualNorm < 1e-6`.

3. **Null-mode detection test**
   - Construct H with a known zero eigenvalue (one row/column = 0).
   - Run `NullModeDiagnoser`.
   - Assert returned `NullModeDiagnosis.NullModeCount == 1`.
   - Assert that mode is classified as `ExactSymmetry` or `Unresolved`.

4. **GN vs full-Hessian difference test**
   - Build both a GN bundle and a full-Hessian bundle for the same background.
   - Run `EigensolverPipeline` on both.
   - Assert they return `SpectrumBundle`s with different `OperatorType`.
   - Assert at least one eigenvalue differs between the two (or assert
     that both produce valid output with recorded difference).

5. **Lanczos vs dense agreement test** (covers "CPU matrix-free vs explicit")
   - For a random symmetric 20×20 problem, compare eigenvalues from
     `DenseEigensolver` and `LanczosSolver`.
   - Assert max relative eigenvalue difference < 1e-4.

### Acceptance criteria

- All 5 new test methods pass with no flaky behaviour.
- `NullModeDiagnoser` has dedicated test coverage.
- GN and full-Hessian produce distinct bundles and both serialize without error.

---

## GAP-9: Lanczos M-orthogonality correctness uncertainty

**Severity:** Medium
**Milestone:** M26 — CPU spectral solver stack
**File:** `src/Gu.Phase3.Spectra/LanczosSolver.cs` (lines 68–86)
**Spec ref:** §7.4 Spectral solve, §11.3 Spectral tests

### Problem

`LanczosSolver.cs` contains comments (lines 68–86) expressing uncertainty
about whether the M-inner-product Lanczos recurrence is correctly
implemented. The comments suggest the implementation may be simplified and
the M-orthogonality of the Krylov basis may not be properly maintained.

For the generalized eigenproblem `H v = lambda M v`, Lanczos requires that
each new Krylov vector is orthogonal to all previous vectors under the
M-inner product: `<v_i, v_j>_M = v_i^T M v_j = delta_ij`. A standard
approach is to use the "M-Lanczos" variant where the three-term recurrence
explicitly uses M:

```text
beta_{j+1} u_{j+1} = H q_j - alpha_j M q_j - beta_j M q_{j-1}
```

where `<q_i, q_j>_M = delta_ij` and `beta_{j+1} = ||u_{j+1}||_M`.

### Required deliverable

1. Audit `LanczosSolver.cs` for correctness of the M-Lanczos recurrence:
   - Confirm that the three-term recurrence uses `M q_{j-1}` correctly.
   - Confirm that `alpha_j = q_j^T H q_j` (Rayleigh quotient).
   - Confirm that `beta_{j+1} = sqrt(u_{j+1}^T M u_{j+1})`.
   - Confirm that full reorthogonalization uses M-inner product:
     `h = q_k^T M w` for each previous vector `q_k`.
2. If any step is wrong, fix it.
3. Remove the uncertainty comments and replace with a brief mathematical
   reference comment.
4. Add to `SpectrumValidationTests.cs` (from GAP-8):
   - A test where M is a non-trivial diagonal (not identity) with
     known eigenvalues, solved by Lanczos.
   - Assert agreement with DenseEigensolver to 1e-4 relative error.

### Acceptance criteria

- `LanczosSolver` passes the Lanczos-vs-dense agreement test from GAP-8
  with non-trivial M.
- Comments in `LanczosSolver.cs` no longer express mathematical uncertainty.

---

## GAP-10: ModeTracking test coverage gaps

**Severity:** Low–Medium
**Milestone:** M27 — Mode tracking and family construction
**File:** `tests/Gu.Phase3.ModeTracking.Tests/ModeTrackingTests.cs`
**Spec ref:** §11.4 Mode tracking tests

### Problem

The spec requires four test categories for mode tracking; two are missing:

| Test category | Status |
|---|---|
| Persistence across tiny continuation steps | ✓ basic |
| Branch match stability | **missing** |
| Ambiguity handling for crossings | ✓ basic (one test) |
| Split/merge bookkeeping | **missing** (feature not implemented) |

After implementing GAP-3 (split/merge/avoided-crossing), the following
tests must be added.

### Required deliverable

Add to `ModeTrackingTests.cs`:

1. **Branch match stability test**
   - Build 3 branch variant spectra with slightly perturbed eigenvalues.
   - Run `ModeMatchingEngine.Match()` for each pair.
   - Assert mode family IDs are consistent across all 3 variants.
   - Assert `IsStable=true` on families with full persistence.

2. **Split bookkeeping test** (requires GAP-3)
   - Build source spectrum with 2 modes, target with 3 modes where one
     source clearly splits (both targets overlap source > SplitThreshold).
   - Assert `ModeAlignmentRecord` with `AlignmentType="split"` is produced.
   - Assert `AmbiguityCount > 0` in the resulting family record.

3. **Merge bookkeeping test** (requires GAP-3)
   - Symmetric to split: 3 source modes, 2 target modes.
   - Assert `AlignmentType="merge"` records are produced.

4. **Avoided-crossing test** (requires GAP-3)
   - Provide a 3-step continuation path where mode A and mode B swap
     eigenvalue ordering between step 1 and step 2.
   - Assert that `BuildFamilies` detects the swap and marks both
     alignments at that step as `AlignmentType="avoided-crossing"`.

### Acceptance criteria

- All 4 new tests pass.
- `CrossBranchModeMap` summary counts include split and merge events.

---

## GAP-11: Properties and Observables minor gaps

**Severity:** Low
**Milestone:** M28/M29
**Files:** `src/Gu.Phase3.Properties/`, `src/Gu.Phase3.Observables/`

### Problem

Two minor deferred items not yet wired:

**M28 — Dispersion-fit and observed-space-fit mass extraction**
`MassLikeScaleExtractor` only implements eigenvalue-derived mass
(`ExtractionMethod = "eigenvalue"`). The spec requires three methods:
(a) eigenvalue, (b) dispersion fit over background parameter variation,
(c) observed-space fit. The `ExtractionMethod` field exists but (b) and (c)
are not computed anywhere.

**M29 — Normalization-variant stability tests**
`ObservationLinearizationValidator` is implemented but no test verifies
that observed signatures remain stable under different normalization
conventions (e.g., the same mode normalized with `L2Unit` vs `MaxBlockNorm`
produces proportional observed signatures).

### Required deliverable

**M28 — Dispersion-fit mass (stub + provenance):**
1. Add `DispersionFitMassExtractor` class (stub) with a `Compute()` method
   that takes a list of `(backgroundParameter, massLikeScale)` pairs and
   returns a `MassLikeScaleRecord` with `ExtractionMethod="dispersion-fit"`.
2. Initially implement as linear interpolation / slope fit.
3. Document that full dispersion fit requires campaign-level continuation
   data (Phase IV).

**M29 — Normalization stability test:**
1. In `LinearizedObservationOperatorTests.cs`, add:
   - Normalize a mode with `L2Unit`, compute signature → sig_a.
   - Normalize the same mode with `MaxBlockNorm`, compute signature → sig_b.
   - Assert `L2Overlap(sig_a, sig_b) > 0.99` (they should be proportional
     after normalization of signatures themselves).

### Acceptance criteria

- `DispersionFitMassExtractor` stub exists, compiles, and has at least
  one unit test verifying it returns a record with the correct
  `ExtractionMethod` tag.
- Normalization stability test passes.

---

## GAP-12: CudaSpectra GPU kernel stubs require documented interface contract

**Severity:** Low (GPU is a known stub; CPU path fully works)
**Milestone:** M31 — CUDA spectral acceleration
**File:** `src/Gu.Phase3.CudaSpectra/GpuSpectralKernel.cs`,
`src/Gu.Phase3.CudaSpectra/SpectralKernelFactory.cs`
**Spec ref:** §8, §11.7

### Problem

`GpuSpectralKernel` is entirely stubbed. `IsCudaAvailable()` always returns
`false`. The spec §14.2 ("CPU reference before CUDA trust") is satisfied for
now, but there is no enforcement that prevents future contributors from
inadvertently enabling GPU paths without parity verification, and the spec
requirement "no high-claim candidate depends on unverified GPU-only logic"
is not enforced in code.

### Required deliverable

1. Add `CudaVerificationStatus` enum:
   ```csharp
   public enum CudaVerificationStatus {
       NotAvailable,
       AvailableUnverified,
       AvailableParityPassed,
       AvailableParityFailed
   }
   ```
2. Add `SpectralKernelFactory.GetCudaStatus()` returning
   `CudaVerificationStatus`.
3. In `DemotionEngine`, add a rule: if a candidate's defining modes were
   computed via a kernel with status `AvailableUnverified`, cap claim class
   at `C1`.
4. Add a property `ComputedWithBackend` (`"cpu"` or `"cuda"`) to
   `SpectrumBundle`.
5. `SpectralBenchmarkRunner` sets `Backend` in the artifact (already
   present in `SpectralBenchmarkArtifact`; verify it is set correctly).
6. Add a test in `SpectralKernelFactoryTests.cs` asserting that calling
   `GetCudaStatus()` when CUDA is unavailable returns `NotAvailable`.

### Acceptance criteria

- `DemotionEngine` test: candidate built from an unverified GPU spectrum
  is capped at C1 when the new rule fires.
- `SpectrumBundle` serialization test includes `computedWithBackend` field.
- `GetCudaStatus()` method exists and compiles.

---

## Definition of Done for this gap round

All 12 gaps are closed when:

1. All new/modified source files build with 0 errors and 0 warnings.
2. `dotnet build && dotnet test --no-build` passes all tests.
3. Each gap's specific acceptance criteria (above) are satisfied.
4. No existing test is broken.
5. CLI commands in GAP-6 are functional end-to-end on the toy 2D
   environment from Phase I/II.
6. All 6 schemas in GAP-7 exist and pass validation against real artifacts.

---

## Gap Priority Table

| Gap | Severity | Milestone affected | Blocking? |
|-----|----------|--------------------|-----------|
| GAP-1: ModeRecord missing fields | High | M25/M26/M28 | Yes — M28 Properties consumes energy fractions |
| GAP-2: Missing O2 metric | High | M27 | Yes — cross-context matching incomplete |
| GAP-3: Missing split/merge/crossing | High | M27 | Yes — spec explicitly requires |
| GAP-4: Missing CandidateBosonRecord envelopes | High | M30 | Yes — spec required fields |
| GAP-5: Missing demotion rules + registry diff | Medium | M30 | No |
| GAP-6: Missing CLI commands | Medium | Overall Phase III | No |
| GAP-7: Missing schemas | Medium | Overall Phase III | No |
| GAP-8: Spectra test gaps | Medium | M25/M26 | No |
| GAP-9: Lanczos M-orthogonality | Medium | M26 | No |
| GAP-10: ModeTracking test gaps | Low–Medium | M27 | No (needs GAP-3 first) |
| GAP-11: Properties/Observables minor | Low | M28/M29 | No |
| GAP-12: CudaSpectra enforcement | Low | M31 | No |

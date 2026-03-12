# PHASE_3_GAPS_02.md

# Phase III — Second Gap Round

**Date:** 2026-03-12
**Status:** Open
**Baseline:** All 12 gaps from PHASE_3_GAPS_01.md closed; 2252 tests passing (0 errors, 0 warnings).

This document identifies gaps between the IMPLEMENTATION_PLAN_P3.md requirements and the current
codebase found during a second code-review pass. Gaps are ordered by priority.

---

## Summary Table

| Gap   | Priority | Area             | Title                                                              |
|-------|----------|------------------|--------------------------------------------------------------------|
| GAP-1 | Critical | Registry         | `BosonRegistry.FromJson()` silently drops all candidates          |
| GAP-2 | High     | Workbench        | Section 10 Vulkan diagnostic views not implemented                |
| GAP-3 | High     | Spectra          | QuotientAware (P3) formulation defined but not implemented         |
| GAP-4 | High     | Observables/CLI  | `compute-spectrum` does not write observed mode signatures         |
| GAP-5 | High     | Observables      | `Gu.Phase3.Observables.ObservationPipeline` has no test coverage  |
| GAP-6 | Medium   | ModeTracking     | HungarianAlgorithm is greedy, not globally optimal                |
| GAP-7 | Medium   | GaugeReduction   | SpectralGap computation uses SVD cutoff as proxy, not actual gap  |
| GAP-8 | Medium   | Properties       | DispersionFitMassExtractor uses linear interpolation, not dispersion fit |
| GAP-9 | Low      | Spectra          | LanczosSolver Jacobi solve hardcoded to 200 iterations, no convergence warning |
| GAP-10| Low      | Registry         | RegistryDiff compares demotions by reason only                    |
| GAP-11| Low      | Deliverables     | Section 16 deliverable #10 — unresolved-issues document absent    |

---

## GAP-1 — `BosonRegistry.FromJson()` silently drops all candidates

**Priority:** Critical
**Area:** `Gu.Phase3.Registry`
**Plan reference:** §9.2 schema round-trip, §12 M30 completion criteria ("registry is generated from real mode families"), §16 deliverable #6

### Symptom

Running `run_pipeline.sh` produces a `run/reports/boson_atlas.md` reporting **"Total Candidates: 0"**
and an empty claim-class distribution, even though `run/bosons/registry.json` contains 10 `C0_NumericalMode`
candidates. The JSON file serializes correctly but round-trips back as empty.

### Root cause

`BosonRegistry.Candidates` is declared as:

```csharp
// src/Gu.Phase3.Registry/BosonRegistry.cs:20
[JsonPropertyName("candidates")]
public IReadOnlyList<CandidateBosonRecord> Candidates => _candidates;
```

`System.Text.Json` uses the **declared property type** (`IReadOnlyList<T>`) — not the runtime type
(`List<T>`) — to decide whether it can populate a read-only property. `IReadOnlyList<T>` does not
implement `ICollection<T>`, so `System.Text.Json` silently skips populating it during deserialization.
The private `_candidates` list is never filled, and every deserialized registry returns `Count == 0`.

The `ToJson()` path is fine because it serializes the `_candidates` list through the getter.

### Acceptance criteria

1. `BosonRegistry.FromJson(registry.ToJson())` round-trips with identical candidate count and all field values.
2. `run_pipeline.sh` produces a report with the correct candidate count (≥ the number of mode families found).
3. Existing `BosonRegistryTests` extend to include a `FromJson_RoundTrip_PreservesCandidates` test that
   registers N candidates, serializes, deserializes, and asserts `Count == N` and spot-checks `ClaimClass`.

### Implementation

Change `Candidates` from a computed property to an `init`-settable property and populate `_candidates`
in a constructor or remove `_candidates` entirely:

```csharp
// Option A — remove backing field, use init list
[JsonPropertyName("candidates")]
public List<CandidateBosonRecord> Candidates { get; init; } = new();

public int Count => Candidates.Count;

public CandidateBosonRecord Register(CandidateBosonRecord candidate)
{
    ArgumentNullException.ThrowIfNull(candidate);
    Candidates.Add(candidate);
    return candidate;
}
```

All query methods (`QueryByClaimClass`, `QueryByBackground`, `QueryByFamily`) work unchanged since
they already operate on `Candidates`. Update `Diff()` accordingly. The `ToJson()` / `FromJson()`
serialization options (including `JsonStringEnumConverter`) remain unchanged.

---

## GAP-2 — Section 10 Vulkan diagnostic views not implemented

**Priority:** High
**Area:** `apps/Gu.Workbench` / `Gu.VulkanViewer`
**Plan reference:** §10 "Workbench additions"

### Description

Section 10 requires 8 new read-only diagnostic views in the Vulkan workbench:

1. Background atlas browser
2. Spectral ladder plots
3. Eigenmode amplitude overlays
4. Gauge leak visualization
5. Branch/refinement mode tracks
6. Boson family cards
7. Observed-signature comparisons
8. Ambiguity heatmaps

The current `apps/Gu.Workbench/Program.cs` (`ArtifactViewerService`) loads Phase I/II run folders
(solver convergence, comparison records) but has no knowledge of Phase III artifacts:
`BackgroundAtlas`, `SpectrumBundle`, `ModeFamilyRecord`, `BosonRegistry`, or `BosonAtlasReport`.
The VulkanViewer has no Phase III view types.

### Acceptance criteria

1. `ArtifactViewerService.LoadRunFolder()` (or a new `Phase3RunFolderLoader`) detects and loads
   all Phase III artifacts from the run folder structure defined in §9.3.
2. Each of the 8 view types is implemented as a read-only console/text or structured output
   (full Vulkan rendering is not required — artifact-driven ASCII / JSON diagnostic output satisfies
   the plan's "diagnostic only, must not alter scientific state" constraint).
3. A `Gu.Workbench.Tests` project (or integration tests in VulkanViewer tests) verifies that
   loading a run folder with Phase III artifacts produces non-empty view payloads for each of the 8 types.

### Implementation sketch

Add a `Phase3ArtifactLoader` to `Gu.VulkanViewer` that reads:
- `backgrounds/atlas.json` → `BackgroundAtlas`
- `spectra/*_spectrum.json` → list of `SpectrumBundle`
- `modes/mode_families.json` → list of `ModeFamilyRecord`
- `bosons/registry.json` → `BosonRegistry`
- `reports/boson_atlas.json` → `BosonAtlasReport`

Add corresponding view payload types (one per view) and a text/JSON printer for each.
`Gu.Workbench/Program.cs` should call `LoadPhase3Folder()` and print structured output.

---

## GAP-3 — QuotientAware (P3) formulation defined but not implemented

**Priority:** High
**Area:** `Gu.Phase3.Spectra`
**Plan reference:** §4.3, §6, §12 M25

### Description

`PhysicalModeFormulation.QuotientAware` is defined:

```csharp
// src/Gu.Phase3.Spectra/PhysicalModeFormulation.cs:24
QuotientAware,
```

But `LinearizedOperatorBundle.ApplySpectral()` and `ApplyMass()` contain no case for it:

```csharp
// src/Gu.Phase3.Spectra/LinearizedOperatorBundle.cs:78-103
if (Formulation == PhysicalModeFormulation.ProjectedComplement && PhysicalProjector is not null)
{ /* P2 path */ }
return SpectralOperator.Apply(v);   // fallthrough — also used for QuotientAware
```

`OperatorBundleBuilder` never sets `Formulation = PhysicalModeFormulation.QuotientAware`.
Passing `--formulation p3` through the CLI would build a P1 bundle silently.

The plan describes P3 as the "true quotient-space" formulation: instead of projecting H into the
physical complement, one constructs an operator on the quotient space Ω / Γ_* directly. This is
the mathematically cleanest treatment but requires additional infrastructure
(quotient-space basis, quotient-space inner product, restricted eigensolver).

### Acceptance criteria

1. `OperatorBundleBuilder` rejects or explicitly scaffolds `QuotientAware` without silently using P1 logic.
   At minimum, throw `NotSupportedException("QuotientAware (P3) is not yet implemented")` or emit a
   clear warning rather than silently falling through.
2. Either: implement the full P3 path (quotient basis + restricted solve), or document P3 as explicitly
   deferred with a plan reference, and guard `--formulation p3` in the CLI with a clear error.
3. Add a unit test asserting that `OperatorBundleBuilder.Build()` with `Formulation = QuotientAware`
   either throws a documented exception or returns a bundle that identifies itself as QuotientAware
   and whose `ApplySpectral` produces a P3-correct result.

### Implementation sketch (minimal — guard only)

```csharp
// OperatorBundleBuilder.cs — add guard after formulation is selected
if (spec.Formulation == PhysicalModeFormulation.QuotientAware)
    throw new NotSupportedException(
        "PhysicalModeFormulation.QuotientAware (P3) is not yet implemented. " +
        "Use PenaltyFixed (P1) or ProjectedComplement (P2).");
```

Full P3 implementation requires:
1. `QuotientBasis` type — a basis for Ω / ker(Γ_*) constructed via column-null-space complement
2. `QuotientProjectedOperator` — restricts H and M to the quotient space
3. `EigensolverPipeline` — adds QuotientAware dispatch branch

---

## GAP-4 — `compute-spectrum` does not write observed mode signatures

**Priority:** High
**Area:** CLI / `Gu.Phase3.Observables`
**Plan reference:** §9.3 run folder structure (`observables/mode_signatures/`), §12 M29 completion criteria ("at least one environment can export observed mode signatures")

### Description

Section 9.3 specifies that a run folder must have an `observables/mode_signatures/` directory
containing `ObservedModeSignature` artifacts for each computed mode. The `compute-spectrum` CLI
command (`Program.cs:1128–1292`) builds an `EigensolverPipeline`, runs it, writes `SpectrumBundle`
and individual `ModeRecord` files, but never invokes the Phase III observation pipeline
(`Gu.Phase3.Observables.ObservedModeMapper` / `LinearizedObservationOperator`).

The run folder from `run_pipeline.sh` contains:
```
run/spectra/    ← present
run/modes/      ← present
run/bosons/     ← present
run/reports/    ← present
run/observables/ ← ABSENT
```

Downstream components (`ModeMatchingEngine` O2 metric, `CandidateBosonRecord.ObservedStabilityScore`,
`BosonCampaignRunner` observed-signature comparisons) all degrade to stub/fallback behavior when
observed signatures are missing.

### Acceptance criteria

1. `compute-spectrum` writes an `ObservedModeSignature` JSON file per mode to
   `<runFolder>/observables/mode_signatures/<modeId>.json`.
2. `track-modes` reads these files when available and computes the O2 overlap metric
   (`ModeMatchMetricSet.ObservedSignatureOverlap`) rather than leaving it null.
3. A test in `Gu.Phase3.Observables.Tests` or `Gu.Phase3.ModeTracking.Tests` verifies the full
   spectrum→signature→O2-matching path end-to-end on a toy 2D system.

### Implementation sketch

After the `EigensolverPipeline.Solve()` call in `SolveSpectrumForBackground()`:

```csharp
// Build linearized observation operator
var pullback = new Phase3PullbackOperator(bundle, geometry);
var obsOp = new LinearizedObservationOperator(bundle.Jacobian, pullback, backgroundId);
var mapper = new ObservedModeMapper(obsOp);

var signaturesDir = Path.Combine(runFolder, "observables", "mode_signatures");
Directory.CreateDirectory(signaturesDir);

foreach (var mode in spectrumBundle.Modes)
{
    var sig = mapper.Map(mode);
    File.WriteAllText(
        Path.Combine(signaturesDir, $"{mode.ModeId}.json"),
        GuJsonDefaults.Serialize(sig));
}
```

`track-modes` loads these files and sets `ModeMatchMetricSet.ObservedSignatureOverlap` if both
source and target signatures are available.

---

## GAP-5 — `Gu.Phase3.Observables.ObservationPipeline` has no test coverage

**Priority:** High
**Area:** `Gu.Phase3.Observables`
**Plan reference:** §11.5 "Observation tests"

### Description

`ObservationPipeline.cs` (in `Gu.Phase3.Observables`) orchestrates:
1. `ObservedModeMapper.MapAll()` — maps all modes to signatures
2. `ObservedOverlapComputer.Compute()` — computes overlap matrix
3. Returns `ObservationPipelineResult`

No test in `Gu.Phase3.Observables.Tests` exercises `ObservationPipeline` directly:

```
tests/Gu.Phase3.Observables.Tests/
├── FiniteDifferenceObservationTests.cs          ← tested
├── LinearizedObservationOperatorTests.cs        ← tested
├── ObservationLinearizationValidatorTests.cs    ← tested
├── ObservedModeMapperTests.cs                   ← tested
├── ObservedModeSignatureSerializationTests.cs   ← tested
├── ObservedOverlapComputerTests.cs              ← tested
├── ObservedOverlapMetricsTests.cs               ← tested
└── (no ObservationPipelineTests.cs)             ← MISSING
```

Section 11.5 also requires "no direct Y-space bypass in comparison paths" — a test that verifies
comparisons are always routed through the observation pipeline, not direct Y-space norms.

### Acceptance criteria

New `ObservationPipelineTests.cs` with ≥ 5 tests:

1. `Run_MultipleModesProducesSignatures_CorrectCount` — N modes in → N signatures out.
2. `Run_ReturnsNonNullOverlapResult` — overlap matrix present and symmetric.
3. `Run_SingleMode_OverlapPairIsNull` — degenerate case (no pair for 1 mode).
4. `Run_DifferentModes_OverlapIsLessThanOne` — orthogonal modes produce off-diagonal < 1.
5. `Run_ResultModeCount_MatchesInput` — `ObservationPipelineResult.ModeCount == input.Count`.

Additionally, add a negative-path test:
6. `NoYSpaceBypass_DirectNormVsObservationPipeline_Differ` — verifies that a direct Y-space
   L2 comparison of two mode vectors differs from the observation-pipeline overlap (enforcing §14.3).

---

## GAP-6 — HungarianAlgorithm is greedy, not globally optimal

**Priority:** Medium
**Area:** `Gu.Phase3.ModeTracking`
**Plan reference:** §7.4 mode matching algorithm, §11.4 "branch match stability"

### Description

`HungarianAlgorithm.cs` (line 47) is documented as a "0.5-greedy implementation":

```csharp
// src/Gu.Phase3.ModeTracking/HungarianAlgorithm.cs:47
// Greedy 0.5-approximation: assign each row to best available column
```

It iterates rows, greedily assigning each to the best unassigned column. This does NOT give
globally optimal assignments. The true Hungarian algorithm uses augmenting paths and runs in
O(n³). For spectra with many near-degenerate modes (common in the flat-background trivial case),
greedy assignment can produce:
- Wrong split/merge classification (a split may be misclassified as a match + birth)
- Incorrect family boundaries downstream
- Non-reproducible results if eigenvalue ordering changes slightly

### Acceptance criteria

1. Replace `HungarianAlgorithm` with a correct O(n³) implementation (Kuhn-Munkres):
   - `Solve(costMatrix)` → `int[]` assignment array, minimizing total cost
   - Accepts rectangular matrices (pad with max cost to make square)
   - Returns correct optimal assignment verified by the existing 2 tests
2. Add a test `Solve_NearDegenerateSpectrum_AssignmentIsOptimal` that constructs a 4×4 cost
   matrix where greedy gives suboptimal cost and verifies the correct minimum-cost assignment.
3. Verify that `ModeTrackingTests.cs` split/merge/avoided-crossing tests still pass after the swap.

### Implementation sketch

Standard Jonker-Volgenant or classical Kuhn-Munkres Hungarian. A clean C# implementation is ~150
lines. Key methods:
```csharp
public static int[] Solve(double[,] costMatrix)   // minimize total cost
public static int[] SolveMaximization(double[,] scoreMatrix)  // maximize (negate, then minimize)
```

`ModeMatchingEngine` calls `HungarianAlgorithm.SolveMaximization(scoreMatrix)` — the call site is
unchanged; only the internals need replacing.

---

## GAP-7 — SpectralGap computation uses SVD cutoff as proxy, not actual gap

**Priority:** Medium
**Area:** `Gu.Phase3.GaugeReduction`
**Plan reference:** §4.2 gauge-sector separation, §11.2 "gauge basis rank tests"

### Description

`GaugeReductionWorkbench.ComputeSpectralGap()` (lines 209–226) approximates the spectral gap
between retained and discarded singular values as:

```csharp
// src/Gu.Phase3.GaugeReduction/GaugeReductionWorkbench.cs:220
double largestDiscarded = basis.SingularValues[0] * basis.SvdCutoff;  // proxy
```

`GaugeBasis` stores only the retained singular values (those above the cutoff). The largest
discarded singular value (just below the cutoff) is never stored, so the true ratio:

```
spectral_gap = min(retained σ) / max(discarded σ)
```

cannot be computed. A large `SvdCutoff` (e.g., 1e-6) with many near-threshold singular values
will produce a reported gap of exactly 1.0 (since `SingularValues.Last() / (SingularValues.First() * cutoff)`
is not meaningful), while the true gap may be near 1.

This makes the diagnostic report misleading when:
- The gauge subspace has near-zero singular values (rank-deficient backgrounds)
- The cutoff is not well-chosen for the problem

### Acceptance criteria

1. `GaugeBasis` stores the full sorted list of singular values including discarded ones:
   ```csharp
   public required IReadOnlyList<double> AllSingularValues { get; init; }  // all, sorted desc
   public required IReadOnlyList<double> SingularValues { get; init; }     // retained only
   ```
2. `ComputeSpectralGap()` uses:
   ```csharp
   double minRetained = basis.SingularValues.Last();
   double maxDiscarded = basis.AllSingularValues
       .SkipWhile(s => s > minRetained).Skip(1).FirstOrDefault(double.Epsilon);
   return minRetained / maxDiscarded;
   ```
3. `ConstraintDefectReport` includes `SpectralGap` (exact value) and `SpectralGapApproximate` (old
   proxy value) to allow comparison.
4. Add a test `ComputeSpectralGap_KnownMatrix_EqualsExpected` with a hand-crafted gauge action
   that has known singular values.

---

## GAP-8 — DispersionFitMassExtractor uses linear interpolation, not dispersion relation

**Priority:** Medium
**Area:** `Gu.Phase3.Properties`
**Plan reference:** §4.5 "mass-like scales", §7.5 "property extraction algorithm", §12 M28

### Description

`DispersionFitMassExtractor.Compute()` fits:

```
mass_like_scale = a + b * backgroundParameter
```

(linear regression, intercept = mass-like scale at parameter = 0). This is documented as a
placeholder but the method's name and signature imply dispersion relation fitting, defined as:

```
omega²(k) = m² + k²   ⟹   m = sqrt(omega² - k²)
```

The correct implementation requires:
- Multiple momentum values `k_j` (from different mesh sizes or periodic boundary wavevectors)
- Corresponding eigenvalue samples `omega²_j`
- Least-squares fit to extract mass `m` from the dispersion relation

The current implementation:
- Takes `(BackgroundParameter, MassLikeScale)` pairs, not `(k, omega)` pairs
- Fits a line through these pairs — has no physical meaning as a dispersion fit
- Is described in the test `Compute_LinearData_FitsIntercept()` which tests a clearly non-physical scenario

### Acceptance criteria

Provide two methods with clearly distinct purposes:

1. **Keep** `DispersionFitMassExtractor.Compute(samples)` but rename to
   `LinearInterpolationMassExtractor.Compute(backgroundParamSamples)` to reflect what it
   actually does (interpolation in background parameter space, not dispersion fitting).

2. **Add** `DispersionFitMassExtractor.ComputeFromDispersion(kValues, eigenvalueSquareds)` that:
   - Takes momentum magnitudes `k_j` and corresponding eigenvalues `omega²_j`
   - Fits `omega² = m² + k²` via least-squares to extract `m²`
   - Returns `MassLikeScaleRecord` with `ExtractionMethod = "dispersion-fit"`
   - Handles the massless case (`m² ≈ 0`) and tachyonic case (`m² < 0`) with appropriate notes
   - Single-point fallback: if only one `(k, omega²)` is available, cannot fit — return with a
     warning in the record

3. Update `EigensolverPipeline` or `PropertyExtractor` to call the correct extractor based on
   available data, and document the expected input format.

### Tests to add

- `ComputeFromDispersion_ToyRelativistic_RecoversMass` — omega²(k) = 4 + k², expect m = 2.0
- `ComputeFromDispersion_Massless_NearZero` — omega²(k) = k², expect |m| < tolerance
- `ComputeFromDispersion_Tachyonic_NegativeMass` — omega²(k) = -1 + k², expect m < 0 with note
- `ComputeFromDispersion_SinglePoint_ThrowsOrWarns`

---

## GAP-9 — LanczosSolver Jacobi solve hardcoded to 200 iterations, no convergence warning

**Priority:** Low
**Area:** `Gu.Phase3.Spectra`
**Plan reference:** §7.3 spectral solve algorithm, §11.3 "generalized eigen residual checks"

### Description

`LanczosSolver.cs` hardcodes `maxJacobiIter = 200` for the internal tridiagonal eigenvalue solve:

```csharp
// src/Gu.Phase3.Spectra/LanczosSolver.cs  (approx line 285)
const int maxJacobiIter = 200;
```

When the Krylov dimension is large (approaching or exceeding 100), the Jacobi iteration on the
tridiagonal matrix may not converge in 200 sweeps, producing silently inaccurate Ritz values.
No convergence warning is emitted; `LobpcgSolver` also hardcodes 200 with no check.

### Acceptance criteria

1. Make `maxJacobiIter` configurable (defaulting to `max(200, 10 * tridiagDim)`) or scale with
   Krylov dimension.
2. After the Jacobi loop, check convergence (max off-diagonal element < `1e-12 * ||T||_F`) and
   emit a diagnostic to the `SpectrumBundle.DiagnosticNotes` field if convergence was not achieved.
3. Add a test `LanczosSolver_LargeKrylovDim_JacobiConverges` that uses a 50-dimensional Krylov
   space and verifies residuals are below tolerance.

---

## GAP-10 — RegistryDiff compares demotions by `DemotionReason` only

**Priority:** Low
**Area:** `Gu.Phase3.Registry`
**Plan reference:** §11.6 "registry diffing between reruns"

### Description

`BosonRegistry.Diff()` detects new demotion records by checking whether `DemotionReason` appears
in the base registry:

```csharp
// src/Gu.Phase3.Registry/BosonRegistry.cs:128-132
var baseReasons = new HashSet<DemotionReason>(
    baseRecord.Demotions.Select(d => d.Reason));
var addedDemotions = otherRecord.Demotions
    .Where(d => !baseReasons.Contains(d.Reason))
    .ToList();
```

If a candidate was demoted twice for `ComparisonMismatch` (e.g., from two different campaign runs),
the second demotion would not appear in the diff. This silently loses demotion events, violating
the "preserve negative results" principle (§14.4).

### Acceptance criteria

1. Change the comparison to detect new demotion events by comparing full demotion records, not
   just reasons. A simple approach: if `other.Demotions.Count > base.Demotions.Count`, the
   difference in count is new demotions. More precisely, compare by `(Reason, Details)` tuple or
   use a positional comparison.
2. Add a test `RegistryDiff_DuplicateDemotionReason_BothDetected` that creates a candidate with
   two `ComparisonMismatch` demotions, diffs against one `ComparisonMismatch`, and asserts
   `DemotionChanges[0].Added.Count == 1`.

---

## GAP-11 — Section 16 deliverable #10: unresolved-issues document absent

**Priority:** Low
**Area:** Documentation / deliverables
**Plan reference:** §16 deliverable #10 "a clear list of unresolved issues for the next phase"

### Description

Section 16 of the plan explicitly requires:

> 10. a clear list of unresolved issues for the next phase.

No such document exists in the repository. This deliverable captures the known limitations,
open questions, and deferred work identified during Phase III, and is the primary input to a
Phase IV plan.

### Acceptance criteria

Create `PHASE_3_OPEN_ISSUES.md` containing at minimum:

1. **Fermionic spectrum** — out of scope per §15.1; captured as open for Phase IV
2. **Full physical particle dictionary** — out of scope per §15.2
3. **QuotientAware (P3) spectral formulation** — deferred; see GAP-3 above
4. **True dispersion-relation mass extraction** — deferred; see GAP-8 above
5. **GPU CUDA kernel linkage** — native library interface not yet linked;
   `IsCudaAvailable()` always returns false; activation path undocumented
6. **Cosmological / high-fidelity environments** — out of scope per §15.7
7. **Interaction proxy beyond cubic** — v1 proxy is cubic FD; quartic and higher deferred
8. **Global well-posedness and uniqueness proofs** — out of scope per §15.6
9. **Symplectic / canonical quantization** — out of scope per §15.3
10. Each open issue should include: description, plan-section reference, why deferred,
    suggested Phase IV input

---

## Definition of Done (all gaps)

For each gap:

- `dotnet build` → 0 errors, 0 warnings
- `dotnet build && dotnet test --no-build` → all existing tests pass + new tests pass
- No `TODO` or `NotImplementedException` in changed code paths
- Serialization round-trips verified where applicable

### GAP-1 (Critical — registry deserialization)
- `BosonRegistry.FromJson(registry.ToJson())` round-trips with correct `Count`
- `run_pipeline.sh` produces report with `totalCandidates > 0`

### GAP-2 (High — workbench views)
- Each of the 8 view types produces non-empty output for the sample `run/` folder
- All operations are read-only (no writes to run artifacts)

### GAP-3 (High — QuotientAware guard)
- `OperatorBundleBuilder` with `QuotientAware` throws a documented exception or implements full P3
- CLI `--formulation p3` produces a clear error or correct behavior

### GAP-4 (High — observation signatures in CLI)
- `run/observables/mode_signatures/` populated after `compute-spectrum`
- `track-modes` reads signatures and sets O2 metric

### GAP-5 (High — ObservationPipeline tests)
- 6 new tests in `ObservationPipelineTests.cs`, all passing
- §14.3 no-bypass test present and passing

### GAP-6 (Medium — Hungarian algorithm)
- Optimal assignment verified by `Solve_NearDegenerateSpectrum_AssignmentIsOptimal`
- All existing ModeTracking tests pass unchanged

### GAP-7 (Medium — spectral gap)
- `GaugeBasis.AllSingularValues` stores all singular values
- `ComputeSpectralGap_KnownMatrix_EqualsExpected` passing

### GAP-8 (Medium — dispersion fit)
- `DispersionFitMassExtractor.ComputeFromDispersion` implemented and tested with 4 new tests
- Old linear interpolation method renamed to `LinearInterpolationMassExtractor` (or kept with clear doc)

### GAP-9 (Low — Lanczos Jacobi)
- Convergence warning emitted to `DiagnosticNotes` when Jacobi does not converge
- `LanczosSolver_LargeKrylovDim_JacobiConverges` passing

### GAP-10 (Low — RegistryDiff)
- `RegistryDiff_DuplicateDemotionReason_BothDetected` passing

### GAP-11 (Low — unresolved issues doc)
- `PHASE_3_OPEN_ISSUES.md` present in repo root with ≥ 10 items

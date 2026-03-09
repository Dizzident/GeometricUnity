# PHASE_2_GAPS_01.md

# Geometric Unity Phase II — Gap Analysis (Round 1)

## Methodology

This document was produced by cross-referencing:
- `IMPLEMENTATION_PLAN_P2.md` (full plan, including §5–§16 math and DoD criteria)
- `ARCHITECTURE_P2.md` (project structure, type specifications)
- Source code in `src/Gu.Phase2.*` (10 projects, 80+ source files)
- Test code in `tests/Gu.Phase2.*` (10 test projects, 34 test files)
- `schemas/` directory
- Build output (0 errors, 0 warnings, all 1616 tests passing)

All 12 Phase II DoD criteria from §16 are partially or fully met at the code-structure level. However, 14 gaps exist between plan specification and actual implementation. These range from missing typed records to absent CUDA interop to missing schemas.

Each gap is described with: what the plan requires, what currently exists, the impact, and a concrete implementation prescription.

---

## Gap Index

| ID | Title | Severity | DoD Impact |
|----|-------|----------|------------|
| GAP-1 | PairwiseStabilityDistanceMatrix (D_stab) missing | High | DoD-3 |
| GAP-2 | FailureModeMatrix missing | High | DoD-3 |
| GAP-3 | BranchRunRecord lacks per-branch stability diagnostics | High | DoD-3 |
| GAP-4 | RecoveryStudySpec type missing | Medium | DoD-9 |
| GAP-5 | BifurcationIndicatorRecord is untyped (uses ContinuationEvent) | Medium | DoD-6 |
| GAP-6 | GaugeFixedLinearizationRecord missing | Medium | DoD-4 |
| GAP-7 | QualitativeClass missing extraction/comparison variants | Medium | DoD-3 |
| GAP-8 | LobpcgSpectrumProbe is a non-functional stub | Medium | DoD-4 |
| GAP-9 | No Arnoldi/randomized-SVD probe for asymmetric L_tilde | Medium | DoD-4 |
| GAP-10 | Four required JSON schemas missing | Low | DoD-3 |
| GAP-11 | Phase II CUDA interop absent (Gu.Phase2.CudaInterop) | Critical | DoD-4 |
| GAP-12 | Vulkan Phase II visualization absent (Gu.Phase2.Viz) | Low | — |
| GAP-13 | Report templates directory missing | Low | DoD-12 |
| GAP-14 | Integration and benchmark test projects missing | Medium | DoD-3,4 |

---

## GAP-1: PairwiseStabilityDistanceMatrix (D_stab) missing

### Plan requirement

§5.4 defines four branch-sensitivity metrics:
- `D_obs` — observed outputs
- `D_dyn` — dynamical diagnostics
- **`D_stab` — linearization/Hessian/spectrum diagnostics**
- `D_conv` — solver convergence

§9.6 "Minimum comparison summary" explicitly requires:
```
PairwiseObservedDistanceMatrix       ✓ implemented
PairwiseDynamicDistanceMatrix        ✓ implemented
PairwiseStabilityDistanceMatrix      ✗ MISSING
QualitativeClassificationAgreementMatrix  ✓ implemented
FailureModeMatrix                    ✗ MISSING (see GAP-2)
```

### What exists

`CanonicityAnalyzer` (src/Gu.Phase2.Canonicity/CanonicityAnalyzer.cs) implements:
- `ComputeObservedDistances()` → D_obs
- `ComputeDynamicDistances()` → D_dyn
- `ComputeConvergenceDistances()` → D_conv

There is no `ComputeStabilityDistances()` method and no `PairwiseStabilityDistanceMatrix`.

### Impact

DoD-3 requires "branch sweeps emit pairwise comparison matrices." Without D_stab, the stability dimension of branch sensitivity is completely absent from the canonicity evidence output. This means branch variants that differ only in their Hessian low-spectrum cannot be distinguished.

### Implementation prescription

**1. Add `HessianSummary` to `BranchRunRecord`** (see GAP-3 — prerequisite).

**2. In `CanonicityAnalyzer`, add:**

```csharp
/// <summary>
/// D_stab metric: pairwise stability distance based on Hessian low-spectrum and
/// smallest singular values of L_tilde.
/// </summary>
public PairwiseDistanceMatrix ComputeStabilityDistances(
    Phase2BranchSweepResult sweepResult,
    EquivalenceSpec equivalence);
```

D_stab for a pair (b_i, b_j) should measure:
- absolute difference in smallest eigenvalue of H
- absolute difference in NegativeModeCount
- absolute difference in SoftModeCount
- absolute difference in smallest singular value of L_tilde (if available)

Normalize by declared tolerance in `equivalence.Tolerances["stability"]` (or a default). Return a `PairwiseDistanceMatrix` with `MetricId = "D_stab"`.

**3. Expose in `CanonicityEvidenceRecord`** by including D_stab summary in the Evaluate() output alongside D_obs and D_dyn.

**4. Tests:** At least 3 tests:
- identical stability → zero distance
- different NegativeModeCount → nonzero distance
- NaN handling when stability data absent (BranchRunRecord has no HessianSummary)

---

## GAP-2: FailureModeMatrix missing

### Plan requirement

§9.6 minimum comparison summary requires a `FailureModeMatrix`. This is distinct from the qualitative classification agreement matrix and captures *how* branches fail rather than *whether* they converged.

### What exists

`QualitativeClassificationAgreementMatrix` captures whether branches agree on Converged/Failed/Stalled. There is no matrix capturing failure modes (solver divergence, extractor failure, gauge slice breakdown, comparison inadmissibility).

### Impact

DoD-3 incomplete: branches that diverge for different physical reasons (extractor failure vs. gauge breakdown vs. solver divergence) look identical in the current agreement matrix. Negative outliers cannot be properly characterized.

### Implementation prescription

**1. Add `FailureModeMatrix` sealed class** (in `Gu.Phase2.Canonicity`):

```csharp
public sealed class FailureModeMatrix
{
    [JsonPropertyName("branchIds")]
    public required IReadOnlyList<string> BranchIds { get; init; }

    /// <summary>Primary failure mode per branch. Null if converged.</summary>
    [JsonPropertyName("primaryFailureModes")]
    public required IReadOnlyList<string?> PrimaryFailureModes { get; init; }

    /// <summary>
    /// Boolean matrix: Modes[i,j] = true if branches i and j failed for the same reason.
    /// </summary>
    [JsonPropertyName("sameFailureMode")]
    public required bool[,] SameFailureMode { get; init; }
}
```

Failure mode strings should include at minimum:
- `"solver-diverged"` — FinalResidualNorm growing
- `"solver-stagnated"` — residual plateau
- `"max-iterations"` — hit iteration cap
- `"extractor-failed"` — ObservedState is null despite convergence
- `"gauge-breakdown"` — explicit gauge-related termination
- `"not-attempted"` — skipped
- `null` — converged normally

**2. Add `ComputeFailureModes()` to `CanonicityAnalyzer`:**

```csharp
public FailureModeMatrix ComputeFailureModes(Phase2BranchSweepResult sweepResult);
```

Classification logic (from `BranchRunRecord`):
- Converged = true → null (no failure)
- TerminationReason contains "diverge" → `"solver-diverged"`
- TerminationReason contains "stagnate" → `"solver-stagnated"`
- TerminationReason contains "iteration" → `"max-iterations"`
- Converged = true, ObservedState = null → `"extractor-failed"`
- Otherwise → `"solver-diverged"` (default)

**3. Tests:** Same-failure mode detection, cross-failure-mode distinction, all-converged matrix (all null primaries).

---

## GAP-3: BranchRunRecord lacks per-branch stability diagnostics

### Plan requirement

§10.1 pseudocode:
```
stabOut = ComputeStabilityDiagnostics(solveResult, environment, b)
store RunRecord(b, solveResult, nativeOut, obsOut, stabOut)
```

§14.2 branch sweep artifact contents must include "per-branch solve traces" implying per-branch stability summaries are part of the sweep record.

### What exists

`BranchRunRecord` (src/Gu.Phase2.Execution/BranchRunRecord.cs) has:
- Variant, Manifest, Converged, TerminationReason, FinalObjective, FinalResidualNorm, Iterations, SolveMode, ObservedState, ArtifactBundle

No stability diagnostics field. `Phase2BranchSweepRunner` does not call any linearization or Hessian workbench.

### Impact

Without per-branch stability data in `BranchRunRecord`:
- D_stab (GAP-1) cannot be computed
- The stability atlas cannot be populated from sweep results
- DoD-3 is incomplete

### Implementation prescription

**1. Add `HessianSummary` record** in `Gu.Phase2.Stability`:

```csharp
public sealed class HessianSummary
{
    [JsonPropertyName("smallestEigenvalue")]
    public required double SmallestEigenvalue { get; init; }

    [JsonPropertyName("negativeModeCount")]
    public required int NegativeModeCount { get; init; }

    [JsonPropertyName("softModeCount")]
    public required int SoftModeCount { get; init; }

    [JsonPropertyName("nearKernelCount")]
    public required int NearKernelCount { get; init; }

    [JsonPropertyName("stabilityClassification")]
    public required string StabilityClassification { get; init; }

    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }
}
```

**2. Add `StabilityDiagnostics` optional field to `BranchRunRecord`:**

```csharp
/// <summary>
/// Per-branch stability summary computed by LinearizationWorkbench, if enabled.
/// Null if stability diagnostics were not requested for this sweep.
/// </summary>
[JsonPropertyName("stabilityDiagnostics")]
public HessianSummary? StabilityDiagnostics { get; init; }
```

**3. Update `Phase2BranchSweepRunner.Sweep()`** to accept an optional `ILinearOperator? jacobian` or `LinearizationWorkbench? workbench` parameter. When provided, call the workbench after each branch solve and populate `StabilityDiagnostics`.

**4. Tests:** Sweep with workbench provided → StabilityDiagnostics populated. Sweep without → null. Two branches with different stability → different summaries.

---

## GAP-4: RecoveryStudySpec type missing

### Plan requirement

§8.5 "Study specification types" lists:
```
BranchSweepSpec        ✓ exists
StabilityStudySpec     ✓ exists
RecoveryStudySpec      ✗ MISSING
ComparisonCampaignSpec ✓ exists
ResearchBatchSpec      ✓ exists
```

### What exists

`StudySpecs.cs` (src/Gu.Phase2.Semantics/StudySpecs.cs) contains BranchSweepSpec, StabilityStudySpec, ResearchBatchSpec. No RecoveryStudySpec.

`ResearchBatchSpec` has `IReadOnlyList<BranchSweepSpec> Sweeps` and `IReadOnlyList<StabilityStudySpec> StabilityStudies` but no recovery study collection.

### Impact

Recovery is managed ad hoc rather than as a typed study specification. The ResearchBatchRunner cannot compose or track recovery studies by spec, making batch recovery runs non-replayable.

### Implementation prescription

**1. Add `RecoveryStudySpec` to `StudySpecs.cs`:**

```csharp
public sealed class RecoveryStudySpec
{
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Branch sweep result ID to use as input native state.</summary>
    [JsonPropertyName("sweepResultId")]
    public required string SweepResultId { get; init; }

    /// <summary>Recovery graph definition ID.</summary>
    [JsonPropertyName("recoveryGraphId")]
    public required string RecoveryGraphId { get; init; }

    /// <summary>Whether to enforce physical-identification gate on all terminal nodes.</summary>
    [JsonPropertyName("enforceIdentificationGate")]
    public required bool EnforceIdentificationGate { get; init; }

    /// <summary>
    /// Maximum allowed claim class for gate-passing outputs.
    /// Outputs exceeding this are automatically demoted.
    /// </summary>
    [JsonPropertyName("maxAllowedClaimClass")]
    public required string MaxAllowedClaimClass { get; init; }
}
```

**2. Add `RecoveryStudies` collection to `ResearchBatchSpec`:**

```csharp
[JsonPropertyName("recoveryStudies")]
public required IReadOnlyList<RecoveryStudySpec> RecoveryStudies { get; init; }
```

**3. Update `ResearchBatchRunner`** to iterate recovery studies and call the recovery graph pipeline per study.

**4. Tests:** Construction, JSON round-trip, ResearchBatchSpec with recovery studies.

---

## GAP-5: BifurcationIndicatorRecord is untyped

### Plan requirement

§8.2 lists `BifurcationIndicatorRecord` as a required typed runtime type for stability and deformation studies.

### What exists

`StabilityAtlas` (src/Gu.Phase2.Continuation/StabilityAtlas.cs, line 53):
```csharp
public required IReadOnlyList<ContinuationEvent> BifurcationIndicators { get; init; }
```

`ContinuationEvent` has Kind, Lambda, Description — but is named and designed as a continuation step event, not a classified bifurcation indicator with structured metadata.

### Impact

Bifurcation indicators lack required metadata: confidence level, which mode was involved, which branch the bifurcation occurred on, classification (fold/branch point/Hopf/unknown), and theorem-dependency status.

### Implementation prescription

**1. Add `BifurcationIndicatorRecord` sealed class** in `Gu.Phase2.Stability`:

```csharp
public sealed class BifurcationIndicatorRecord
{
    [JsonPropertyName("indicatorId")]
    public required string IndicatorId { get; init; }

    [JsonPropertyName("lambda")]
    public required double Lambda { get; init; }

    [JsonPropertyName("kind")]
    public required string Kind { get; init; }  // "fold", "branch-point", "hopf-candidate", "sign-change", "unknown"

    [JsonPropertyName("triggeringEvent")]
    public required ContinuationEventKind TriggeringEvent { get; init; }

    [JsonPropertyName("modeIndex")]
    public int? ModeIndex { get; init; }  // which eigenmode, if known

    [JsonPropertyName("eigenvalueAtDetection")]
    public double? EigenvalueAtDetection { get; init; }

    [JsonPropertyName("confidence")]
    public required string Confidence { get; init; }  // "numerical-only", "strong-numerical", "theorem-supported"

    [JsonPropertyName("theoremDependencyStatus")]
    public required string TheoremDependencyStatus { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }
}
```

**2. Update `StabilityAtlas`:**

```csharp
// Replace:
public required IReadOnlyList<ContinuationEvent> BifurcationIndicators { get; init; }
// With:
public required IReadOnlyList<BifurcationIndicatorRecord> BifurcationIndicators { get; init; }
```

**3. Update `StabilityAtlasBuilder`** to convert detected `ContinuationEvent` objects to `BifurcationIndicatorRecord` objects (with Kind inferred from `ContinuationEventKind`, Confidence = "numerical-only" by default).

**4. Tests:** Construction, kind inference from ContinuationEventKind, JSON round-trip.

---

## GAP-6: GaugeFixedLinearizationRecord missing

### Plan requirement

§8.2 lists `GaugeFixedLinearizationRecord` as a distinct required type alongside `LinearizationRecord`.

This separation is physically meaningful: the plain linearization record captures J alone; the gauge-fixed record captures L_tilde = (J, C) with explicit gauge handling metadata, lambda, and validation against the gauge null space.

### What exists

`LinearizationRecord` (src/Gu.Phase2.Stability/LinearizationRecord.cs) has `GaugeHandlingMode` as a string field, but is not split into two types. There is no dedicated `GaugeFixedLinearizationRecord`.

### Impact

Consumers cannot distinguish a bare Jacobian linearization from a gauge-augmented one by type alone. The lambda (gauge strength) and gauge-null-space verification status have no dedicated home.

### Implementation prescription

**1. Add `GaugeFixedLinearizationRecord` sealed class** in `Gu.Phase2.Stability`:

```csharp
public sealed class GaugeFixedLinearizationRecord
{
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    [JsonPropertyName("baseLinearizationId")]
    public required string BaseLinearizationId { get; init; }  // ref to LinearizationRecord

    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }  // "coulomb-slice", "explicit-slice", "gauge-free"

    [JsonPropertyName("gaugeLambda")]
    public required double GaugeLambda { get; init; }

    /// <summary>
    /// Dimension of gauge-null space of J before fixing.
    /// Expected to equal the dimension of the gauge group.
    /// </summary>
    [JsonPropertyName("gaugeNullDimension")]
    public required int GaugeNullDimension { get; init; }

    /// <summary>
    /// Whether the gauge null space was suppressed to within tolerance by C.
    /// </summary>
    [JsonPropertyName("gaugeNullSuppressed")]
    public required bool GaugeNullSuppressed { get; init; }

    /// <summary>
    /// Smallest singular value of L_tilde on the Coulomb slice.
    /// </summary>
    [JsonPropertyName("smallestSliceSingularValue")]
    public double? SmallestSliceSingularValue { get; init; }

    [JsonPropertyName("validationStatus")]
    public required string ValidationStatus { get; init; }
}
```

**2. Emit `GaugeFixedLinearizationRecord`** from `LinearizationWorkbench` when gauge-fixing is active (i.e., when `CoulombSliceOperator` or similar is used).

**3. Add `GaugeFixedLinearizationRecords` to `StabilityAtlas`.**

**4. Tests:** Construction, JSON round-trip, ValidationStatus propagation from workbench.

---

## GAP-7: QualitativeClass missing extraction/comparison variants

### Plan requirement

§9.6 specifies qualitative classifications to compare across branches:
```
converged / failed / stalled                       ✓ QualitativeClass
stable / soft / singular / saddle                  ✓ HessianRecord.StabilityClassification
extraction-succeeded / extraction-failed           ✗ MISSING
comparison-admissible / comparison-inadmissible    ✗ MISSING
```

### What exists

`QualitativeClass` (src/Gu.Phase2.Canonicity/QualitativeClass.cs) has only: Converged, Failed, Stalled.

`QualitativeClassificationAgreementMatrix` only compares convergence quality. Extraction and comparison admissibility are not tracked.

### Impact

The agreement matrix cannot capture that two branches both converged but one produced comparison-admissible output while the other's extraction failed. Canonicity evidence is coarser than the plan requires.

### Implementation prescription

**Option A (simpler):** Add two separate typed fields to `BranchRunRecord`:

```csharp
/// <summary>Whether the observation extraction pipeline produced valid output.</summary>
[JsonPropertyName("extractionSucceeded")]
public required bool ExtractionSucceeded { get; init; }

/// <summary>Whether the output meets comparison admissibility criteria (typed, falsifier present, etc.).</summary>
[JsonPropertyName("comparisonAdmissible")]
public required bool ComparisonAdmissible { get; init; }
```

**Option B (fuller):** Add two more values to `QualitativeClass` and expand the agreement matrix:

```csharp
public enum QualitativeClass
{
    Converged,
    Failed,
    Stalled,
    ExtractionSucceeded,   // converged AND observed output produced
    ExtractionFailed,      // converged but ObservedState is null
    ComparisonAdmissible,  // all prediction validation rules pass
    ComparisonInadmissible // at least one validation rule fails
}
```

Recommendation: **Option A** — keep QualitativeClass for convergence only, use explicit bool fields for extraction/comparison status. Update `CanonicityAnalyzer` to report extraction and admissibility agreement separately.

**Tests:** Sweep where one branch extracts, one doesn't → different extraction status. Sweep where one branch has a falsifier gap → comparison-inadmissible.

---

## GAP-8: LobpcgSpectrumProbe is a non-functional stub

### Plan requirement

§10.3 specifies LOBPCG as a distinct spectrum probe method for symmetric H. The plan says "Lanczos / LOBPCG for symmetric H."

### What exists

`LobpcgSpectrumProbe` (src/Gu.Phase2.Stability/LobpcgSpectrumProbe.cs) has `MethodId = "lobpcg"` but internally delegates entirely to `LanczosSpectrumProbe`. It is a label-only stub.

### Impact

A `StabilityStudySpec` requesting `SpectrumProbeMethod = "lobpcg"` silently executes Lanczos instead. This produces a misleading `SpectrumRecord.GaugeHandlingMode` and `NormalizationConvention`. Results labeled "lobpcg" are actually "lanczos" results.

### Implementation prescription

Implement a real block LOBPCG for symmetric positive semi-definite operators. A self-contained C# implementation:

1. Initialize a block of `k` orthonormal random trial vectors X (k = requestedModes).
2. Compute residuals R = H*X - X * (X^T * H * X).
3. Optionally apply preconditioner to R.
4. Form search space S = [X, R, P] (P = previous search direction).
5. Project H onto S: H_small = S^T * H * S.
6. Solve small symmetric eigenproblem (use explicit dense matrix up to S.Length × S.Length).
7. Update X, update P, check convergence (residual norm < tol).
8. Return converged eigenvalues and eigenvectors.

A minimal but functional LOBPCG does not need the preconditioner in Phase II — the unpreconditioned version is sufficient for correctness.

Label output `MethodId = "lobpcg"` and set `NormalizationConvention = "H-orthonormal"`.

**Tests:** LOBPCG and Lanczos agree on eigenvalues of a 5×5 symmetric test operator. LOBPCG handles near-degenerate eigenvalues.

---

## GAP-9: No Arnoldi/randomized-SVD probe for asymmetric L_tilde

### Plan requirement

§10.3: "implicitly restarted Arnoldi or randomized SVD for L_tilde."

L_tilde = (J, sqrt(λ)·C) is a non-square, non-symmetric operator. Its singular values are needed for:
- smallest singular value collapse detection in continuation
- branch-fragility via σ_min(L_tilde)
- PDE character studies

### What exists

`LanczosSpectrumProbe` and `LobpcgSpectrumProbe` both only target symmetric operators. Both internally use L_tilde^T * L_tilde to compute singular values, which works but:
1. Squares the condition number
2. Does not separate left/right singular vectors
3. Cannot be relabeled as "arnoldi" with integrity

### Impact

σ_min(L_tilde) via the normal equations A^T A may suffer severe numerical cancellation when σ_min is small (which is exactly when it matters most for singularity detection).

### Implementation prescription

Add `RandomizedSvdProbe` in `Gu.Phase2.Stability` (simpler than Arnoldi, sufficient for Phase II):

```csharp
public sealed class RandomizedSvdProbe : ISpectrumProbe
{
    public string MethodId => "randomized-svd";

    // Computes k smallest singular values of op (non-square allowed)
    // Algorithm: randomized range finder + power iteration + SVD of small matrix
    public SpectrumProbeResult Probe(ILinearOperator op, int requestedModes);
}
```

Algorithm sketch:
1. Draw Ω ∈ R^{n × (k+oversampling)} random Gaussian.
2. Form Y = op * Ω, optionally with q power iterations: Y = (op * op^T)^q * Y.
3. QR-decompose Y = Q * R.
4. Form B = Q^T * op (size k × m).
5. SVD of small B → singular values and left/right vectors in original space.

This is numerically robust for σ_min even in ill-conditioned cases.

Register in `ComparisonStrategyFactory`-equivalent dispatch so `SpectrumProbeMethod = "randomized-svd"` routes here.

**Tests:** Singular values of a known matrix. σ_min detection for nearly-rank-deficient L_tilde.

---

## GAP-10: Four required JSON schemas missing

### Plan requirement

§7 specifies schemas under `/schemas/`:

| Schema | Status |
|--------|--------|
| `branch_family.schema.json` | ✅ exists |
| `branch_variant.schema.json` | ✅ exists |
| `canonicity_docket.schema.json` | ✅ exists |
| `equivalence_spec.schema.json` | ⚠️ not listed (covered by branch_family?) |
| `study_spec.schema.json` | ❌ MISSING |
| `prediction_test_record.schema.json` | ❌ MISSING |
| `comparison_campaign.schema.json` | ❌ MISSING |
| `branch_sweep_result.schema.json` | ❌ MISSING |
| `stability_record.schema.json` | ✅ covered by stability_atlas.schema.json |
| `continuation_record.schema.json` | ✅ exists |

### Implementation prescription

Add four JSON schemas following the same style as existing schemas (inferred from `GuJsonDefaults` and the C# types):

**`schemas/study_spec.schema.json`** — covers BranchSweepSpec, StabilityStudySpec, RecoveryStudySpec, ResearchBatchSpec.

**`schemas/prediction_test_record.schema.json`** — covers PredictionTestRecord, ComparisonAsset, UncertaintyRecord, CalibrationRecord.

**`schemas/comparison_campaign.schema.json`** — covers ComparisonCampaignSpec, ComparisonCampaignResult, ComparisonRunRecord, ComparisonFailureRecord, NegativeResultArtifact.

**`schemas/branch_sweep_result.schema.json`** — covers Phase2BranchSweepResult, BranchRunRecord.

These should be JSON Schema Draft 7 compatible, using `$defs` for shared sub-objects and `required` arrays matching the C# `required` init properties.

---

## GAP-11: Phase II CUDA interop absent (Gu.Phase2.CudaInterop)

### Plan requirement

§7 specifies `/src/Gu.Phase2.CudaInterop` and `/native/gu_phase2_cuda/` with:
- `branch_kernels.cu` — branch-parameterized residual assembly
- `jacobian_actions.cu` — Jv and J^T v
- `hessian_actions.cu` — H = L_tilde^T * M * L_tilde actions
- `spectrum_probes.cu` — spectral diagnostics
- `continuation_kernels.cu` — inner solves

§11.4 "CUDA parity rules" and DoD-4 state: "Linearization/Hessian workbench exists with CPU/CUDA parity."

§11 priority order:
1. batched multi-branch residual evaluation
2. Jv and J^T v actions
3. Hessian-style operator actions
4. spectral probes
5. continuation inner solves

### What exists

The existing CUDA infrastructure (`native/gu_cuda_kernels/`, `src/Gu.Interop/`) covers Phase I residual and solver kernels. No Phase II CUDA project exists.

### Impact

DoD-4 explicitly says "CPU/CUDA parity." Without Phase II CUDA interop, no research report produced by Phase II can certify GPU-path parity. All Phase II stability and linearization runs are CPU-only.

### Implementation prescription

This is a multi-file implementation. Scope it as follows:

**Step 1: Gu.Phase2.CudaInterop C# project** in `src/Gu.Phase2.CudaInterop/`

Define interop contracts:

```csharp
// Branch-parameterized residual (Phase II extension of Phase I kernel)
public sealed class Phase2ResidualKernelArgs
{
    public required BranchVariantManifest Variant { get; init; }
    public required int FieldDof { get; init; }
    // ... packed geometry refs
}

// Jv action kernel
public interface IPhase2JacobianKernel
{
    void ApplyJv(ReadOnlySpan<double> u, ReadOnlySpan<double> v, Span<double> result, BranchVariantManifest variant);
    void ApplyJtw(ReadOnlySpan<double> u, ReadOnlySpan<double> w, Span<double> result, BranchVariantManifest variant);
}

// Hessian action kernel
public interface IPhase2HessianKernel
{
    void ApplyHv(ReadOnlySpan<double> u, ReadOnlySpan<double> v, Span<double> result, BranchVariantManifest variant, double lambda);
}
```

**Step 2: native/gu_phase2_cuda/** CUDA kernels — implement as thin wrappers around Phase I kernels with branch-parameterized dispatch tables.

**Step 3: Parity test suite** in `tests/Gu.Phase2.CudaInterop.Tests/`:
- Jv CPU vs GPU agreement: max relative error < 1e-9 on benchmark geometry
- J^T w CPU vs GPU agreement
- Hv CPU vs GPU agreement
- Batch sweep: N branches evaluated in one CUDA launch vs N sequential CPU evaluations

**Priority order for implementation:** Jv/J^T w first (enabling Newton-Krylov and spectral probes), then Hv, then batched sweeps.

---

## GAP-12: Vulkan Phase II visualization absent (Gu.Phase2.Viz)

### Plan requirement

§6.3 and §7 specify `/src/Gu.Phase2.Viz` with:
- branch-to-branch visual overlays
- stability spectrum dashboards
- continuation path visualization
- observed-vs-native diagnostics
- canonicity heatmaps
- comparison campaign summaries

### What exists

`Gu.VulkanViewer` (Phase I) exists for single-branch diagnostic visualization. No Phase II extension exists.

### Impact

Low severity per §18.6: "Full large-scale HPC optimization" and visual tooling are "valuable but secondary to correctness and auditability." However, continuation paths and stability spectra are currently invisible without external tooling.

### Implementation prescription

Add `Gu.Phase2.Viz` project depending on `Gu.VulkanViewer` and Phase II result types:

**Minimum viable scope:**

1. **BranchOverlayView** — render two branches' observed outputs side-by-side with computed D_obs highlighted on mesh faces.
2. **SpectrumDashboard** — render eigenvalue spectrum bar chart from `SpectrumRecord` as a HUD overlay.
3. **ContinuationPathView** — render lambda vs. objective or lambda vs. smallest eigenvalue as a 2D line plot on the HUD.
4. **CanonicityHeatmap** — render PairwiseDistanceMatrix as a colored NxN grid.

Each view should be a separate `IVulkanDiagnosticPanel` implementor (following Phase I pattern).

---

## GAP-13: Report templates directory missing

### Plan requirement

§7 specifies `/reports/templates/` with:
- `canonicity_docket.md`
- `branch_sweep.md`
- `stability_atlas.md`
- `comparison_campaign.md`

### What exists

`/reports/templates/` directory does not exist. `ResearchReportGenerator` produces structured C# objects only.

### Impact

DoD-12 requires "reports explicitly distinguish branch-local evidence, theorem-level status, and empirical status." Without templates, the C# report objects cannot be surfaced as human-readable handoff documents. The plan says Phase II should support "a real next-session handoff without the original manuscript."

### Implementation prescription

Create `/reports/templates/` and add 4 Markdown templates. Each template should use `{{placeholder}}` tokens matching the C# `ResearchReport` field names and be self-documenting.

**`canonicity_docket.md`** template sections:
- Object class and representative
- Equivalence relation
- Current evidence table (StudyId, Verdict, MaxDeviation, Tolerance)
- Counterexamples
- Pending theorems
- Docket status

**`branch_sweep.md`** template sections:
- Environment spec summary
- Branch family table (VariantId, A0Variant, TorsionVariant, ShiabVariant)
- Per-branch results (QualitativeClass, FinalObjective, ExtractionSucceeded)
- Pairwise distance matrix tables (D_obs, D_dyn, D_stab)
- Canonicity evidence summary

**`stability_atlas.md`** template sections:
- Background family description
- Linearization records table
- Hessian mode classification (coercive/soft/near-kernel/negative counts)
- Continuation path summary (lambda range, steps, events)
- Bifurcation indicators

**`comparison_campaign.md`** template sections:
- Campaign ID and mode (Structural/SemiQuantitative/Quantitative)
- Prediction test matrix (TestId, ClaimClass, Score, Passed)
- Uncertainty decomposition table
- Negative results / failures
- Conclusion claim class summary

---

## GAP-14: Integration and benchmark test projects missing

### Plan requirement

§7 specifies:
- `/tests/Gu.Phase2.IntegrationTests` — cross-module integration tests
- `/tests/Gu.Phase2.GoldenArtifacts` — golden artifact regression tests
- `/tests/Gu.Phase2.ManufacturedSolutions` — manufactured solution benchmarks

§10.4 specifies 5 benchmark classes (A–E).

### What exists

10 per-module unit test projects exist. No integration project, no golden artifact project, no manufactured solutions project.

### Impact

DoD criteria 3, 4, and 10 are validated only within module boundaries. The full pipeline — sweep → canonicity → stability → recovery → prediction → comparison → report — has no end-to-end integration test. Regression against known-good artifacts is not automated.

### Implementation prescription

**Gu.Phase2.IntegrationTests** (highest priority):

Add `tests/Gu.Phase2.IntegrationTests/` with tests covering:

1. **Full pipeline A0-sweep test** (Appendix B Study S1):
   - Create BranchFamilyManifest with 2 variants differing only in A0Variant
   - Run Phase2BranchSweepRunner on 2D test geometry
   - Assert both produce BranchRunRecord
   - Run CanonicityAnalyzer → assert PairwiseDistanceMatrix dimensions
   - Assert CanonicityEvidenceRecord is produced

2. **Full pipeline linearization + spectrum** (Study S4):
   - Solve single branch → get background state
   - Run LinearizationWorkbench → get J, L_tilde, H
   - Run LanczosSpectrumProbe → get SpectrumRecord
   - Assert stability classification is non-null

3. **Comparison dry-run end-to-end** (Study S6):
   - Build PredictionTestRecord with InMemoryDatasetAdapter
   - Run CampaignRunner.RunWithStrategy()
   - Assert ComparisonCampaignResult has RunRecords
   - Assert NegativeResultArtifact produced for failing prediction

4. **Recovery DAG + gate enforcement**:
   - Build valid 4-node RecoveryGraph (Native → Obs → Ext → Interp)
   - Run IdentificationGate with missing falsifier
   - Assert Inadmissible result
   - Run ResearchReportGenerator → assert OpenItems non-empty

**Gu.Phase2.ManufacturedSolutions** (medium priority):

Benchmark class B (linearization benchmark) and C (gauge/slice benchmark) from §10.4:

- Class B: Flat connection omega = 0, known J = d (exterior derivative). Verify `LinearizationWorkbench` produces J that agrees with analytical d on all edge DOFs.
- Class C: Connection with known gauge group dimension n. Verify raw Jacobian nullity = n, gauge-fixed operator suppresses null space to within 1e-8.

**Gu.Phase2.GoldenArtifacts** (lower priority):

Once IntegrationTests pass, run them once to produce serialized JSON output artifacts. Store them as embedded resources. Add a test that deserializes and asserts field values match to within tolerance.

---

## Summary Table

| GAP | Affected Files / Projects | Estimated Complexity |
|-----|--------------------------|---------------------|
| GAP-1 | CanonicityAnalyzer.cs, PairwiseDistanceMatrix.cs | Low |
| GAP-2 | CanonicityAnalyzer.cs + new FailureModeMatrix.cs | Low |
| GAP-3 | BranchRunRecord.cs, Phase2BranchSweepRunner.cs + new HessianSummary.cs | Medium |
| GAP-4 | StudySpecs.cs, ResearchBatchRunner.cs | Low |
| GAP-5 | StabilityAtlas.cs, StabilityAtlasBuilder.cs + new BifurcationIndicatorRecord.cs | Low |
| GAP-6 | LinearizationWorkbench.cs, StabilityAtlas.cs + new GaugeFixedLinearizationRecord.cs | Low |
| GAP-7 | CanonicityAnalyzer.cs + extraction/admissibility fields in BranchRunRecord | Low |
| GAP-8 | LobpcgSpectrumProbe.cs (full replacement) | Medium |
| GAP-9 | New RandomizedSvdProbe.cs | Medium |
| GAP-10 | 4 new .schema.json files | Low |
| GAP-11 | New Gu.Phase2.CudaInterop + native/gu_phase2_cuda/ CUDA kernels | High |
| GAP-12 | New Gu.Phase2.Viz + Vulkan panels | High |
| GAP-13 | 4 new Markdown templates in reports/templates/ | Low |
| GAP-14 | 3 new test projects (Integration, ManufacturedSolutions, GoldenArtifacts) | Medium |

---

## Recommended Implementation Order

**Batch 1 — Type completeness (all Low complexity, unblock downstream):**
- GAP-4 (RecoveryStudySpec)
- GAP-5 (BifurcationIndicatorRecord)
- GAP-6 (GaugeFixedLinearizationRecord)
- GAP-10 (missing schemas)
- GAP-13 (report templates)

**Batch 2 — Canonicity engine completeness (DoD-3 full closure):**
- GAP-3 (HessianSummary in BranchRunRecord) — prerequisite for GAP-1
- GAP-1 (D_stab metric)
- GAP-2 (FailureModeMatrix)
- GAP-7 (extraction/comparison QualitativeClass)

**Batch 3 — Spectrum probe correctness (DoD-4 strengthening):**
- GAP-8 (real LOBPCG)
- GAP-9 (RandomizedSvdProbe for L_tilde)

**Batch 4 — Integration testing (DoD verification):**
- GAP-14 (IntegrationTests first, then ManufacturedSolutions, then GoldenArtifacts)

**Batch 5 — CUDA parity (DoD-4 strict compliance):**
- GAP-11 (Phase II CUDA interop)

**Batch 6 — Visualization (non-blocking):**
- GAP-12 (Gu.Phase2.Viz)

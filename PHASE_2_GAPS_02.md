# PHASE_2_GAPS_02.md

## Geometric Unity Phase II — Round 2 Gap Analysis

**Date:** 2026-03-09
**Build status:** 0 errors, 2 warnings (xUnit2012), all tests pass
**Prior round:** PHASE_2_GAPS_01.md (all 14 gaps closed, committed a519524 + 804d521)

---

## Summary

All Phase I and Phase II code compiles cleanly and all tests pass. The Phase II C# layer (Semantics, Branches, Execution, Canonicity, Stability, Recovery, Continuation, Predictions, Comparison, Reporting, Viz) is a sophisticated, production-quality research platform with real numerical algorithms (Lanczos, LOBPCG, Jacobi, pseudo-arclength continuation, chi-squared comparison). No `NotImplementedException` is present in any Phase II source file.

However, 7 gaps remain relative to the IMPLEMENTATION_PLAN_P2.md's full specification and Definition of Done (Section 16):

| GAP | Severity | DoD Risk | Area |
|-----|----------|----------|------|
| GAP-1 | HIGH | DoD #4 | CUDA kernels are zero-returning stubs |
| GAP-2 | MEDIUM | DoD #6 | 3 of 6 continuation event detectors not wired |
| GAP-3 | MEDIUM | DoD #4 | CoulombSliceOperator uses flat d^* not covariant d_{A0}^* |
| GAP-4 | MEDIUM | DoD #5 | PrincipalSymbolSampler covector-norm scaling is wrong order |
| GAP-5 | MEDIUM | DoD #5,#6 | Manufactured-solution benchmark classes A, D, E missing |
| GAP-6 | LOW | DoD #10 | No file-backed external comparison dataset test |
| GAP-7 | LOW | none | 2 xUnit2012 build warnings in PipelineIntegrationTests.cs |

---

## GAP-1 (HIGH): CUDA Kernels Are Zero-Returning Stubs

### Problem

The native `gu_phase2_cuda` library in `native/gu_phase2_cuda/src/` consists of 6 `.cu` files totaling ~245 lines. All kernel functions are CPU fallbacks that call `memset(..., 0, ...)` and return immediately, ignoring all input:

```c
// jacobian_actions.cu:
memset(result, 0, result_size * sizeof(double));
/* CPU fallback: Jacobian action stub. ... This stub returns zero. */
(void)u; (void)v; (void)edge_count; (void)branch_flags;
return 0;

// hessian_actions.cu:
memset(result, 0, field_size * sizeof(double));
/* CPU fallback: Hessian action stub. ... This stub returns zero. */
(void)u; (void)v; (void)face_count; (void)lambda; (void)branch_flags;
return 0;

// spectrum_probes.cu: entirely commented-out future implementation

// continuation_kernels.cu: entirely commented-out future implementation
```

The C# `Phase2CudaBackend` P/Invoke calls these stubs correctly, but since they return zeros, `ApplyJv`, `ApplyJtw`, `ApplyHv`, and `BatchResidual` always return zero vectors regardless of input.

The parity checker tests in `Gu.Phase2.CudaInterop.Tests` correctly compare CPU vs GPU output — but since both the "CPU" physics path and the GPU stub path both return zeros on the benchmark cases used (flat connection, zero field), the parity tests pass trivially.

### DoD Impact

- **DoD criterion #4**: "Linearization/Hessian workbench exists — with CPU/CUDA parity" — CUDA side is not functional.
- Plan Section 11.4: "No Phase II research report may consume GPU-only path lacking parity certification" — currently no GPU path computes real physics.

### What Needs to Be Done

**Native layer (C/CUDA):**

Implement real Jacobian-vector product in `jacobian_actions.cu`. The Jacobian of the curvature residual at background `u` acting on perturbation `v` is (from plan Section 9.1 and Phase I physics memory):

```
J(u)*v = d(v) + 0.5 * sum_{i<j} ([u_i, v_j] + [v_i, u_j])
```

where `d` is the simplicial coboundary operator (face-edge incidence), `u` and `v` are edge-valued Lie-algebra fields, and `[·,·]` is the Lie bracket using structure constants.

This requires uploading to the native layer:
- The face-to-edge incidence matrix (boundary operator)
- The structure constants f_{abc} for su(2) or su(3)

**Kernel family 1 — `gu_phase2_jacobian_action`:** Compute `y_a^f = sum_e d_{fe} v_a^e + 0.5 * sum_{e1,e2} c_{e1,e2}^f f_{abc} u_b^{e1} v_c^{e2}` where `c` encodes face-edge-edge incidence for bracket terms.

**Kernel family 2 — `gu_phase2_adjoint_action` (J^T w):** The adjoint acts on face-valued `w` and produces edge-valued output:
```
(J^T w)_a^e = sum_f d_{fe} w_a^f + bracket-adjoint terms
```

**Kernel family 3 — `gu_phase2_hessian_action`:** Compose Jv and J^Tw:
```
H*v = J^T (M_R (J*v)) + lambda * C^T (M_0 (C*v))
```
where `C` is the Coulomb slice operator. Can be implemented by sequentially calling Jv then J^Tw with identity mass matrices as a starting point, then adding the gauge penalty term.

**Kernel family 4 — `gu_phase2_batch_residual`:** Loop over branch variants and call per-branch residual assembly. Use the Phase I CPU residual logic ported to C.

**Parity verification:** The parity checker must be exercised on a **non-trivial** state (non-zero connection field on a test mesh), not the flat connection, to confirm the GPU path matches the CPU reference. Add a `Phase2CudaInteropTests.Jv_NonFlatConnection_MatchesCpuReference` test.

### Minimum Acceptable Scope (for research report safety)

If full CUDA implementation is out of scope for this phase, the `Phase2CudaBackend` should throw `NotSupportedException` when called with a non-zero state, rather than silently returning zeros. This would prevent any downstream code from consuming incorrect GPU results.

---

## GAP-2 (MEDIUM): 3 of 6 Continuation Event Detectors Not Wired

### Problem

`ContinuationEventKind` defines 6 required event kinds (per plan Section 10.2):

```csharp
public enum ContinuationEventKind
{
    SingularValueCollapse,     // ✓ detected in ContinuationRunner.cs line ~155
    HessianSignChange,         // ✓ detected in ContinuationRunner.cs line ~143
    StepRejectionBurst,        // ✓ detected in ContinuationRunner.cs line ~103
    ExtractorFailure,          // ✗ defined but never emitted
    BranchMergeSplitCandidate, // ✗ defined but never emitted
    GaugeSliceBreakdown,       // ✗ defined but never emitted
}
```

The runner detects `StepRejectionBurst`, `HessianSignChange`, and `SingularValueCollapse`. The remaining three are in the enum and handled by `StabilityAtlasBuilder.InferKind()` in switch cases, but `ContinuationRunner.Run()` never emits them.

### What Needs to Be Done

**ExtractorFailure:** After each corrector step that produces an `ObservedState`, attempt the extraction pipeline. If extraction throws or returns null/empty, emit:
```csharp
stepEvents.Add(new ContinuationEvent
{
    Kind = ContinuationEventKind.ExtractorFailure,
    StepNumber = stepNumber,
    Lambda = nextLambda,
    Description = "Extraction pipeline failed at corrector solution"
});
```

The `ContinuationRunner` already accepts an optional `ISpectrumProbe`; add an optional `Func<double[], bool>` extractorDelegate that returns false on failure.

**BranchMergeSplitCandidate:** Detect when the tangent vector angle changes sharply between consecutive steps (dot product of consecutive normalized tangents drops below threshold, e.g. 0.5). This indicates the path is turning sharply, a common indicator of an approaching branch point:
```csharp
if (prevTangent != null && Vector.Dot(tangent, prevTangent) < _branchMergeAngleThreshold)
    stepEvents.Add(new ContinuationEvent { Kind = ContinuationEventKind.BranchMergeSplitCandidate, ... });
```

**GaugeSliceBreakdown:** Detect when the Coulomb slice constraint norm `||C(u)||` grows above a threshold during the corrector iteration (indicates the corrector solution is drifting off the gauge slice). If a gauge residual norm delegate is provided, check it after each corrector step:
```csharp
if (gaugeResidualNorm > _gaugeSliceBreakdownThreshold)
    stepEvents.Add(new ContinuationEvent { Kind = ContinuationEventKind.GaugeSliceBreakdown, ... });
```

**Tests to add:** In `ContinuationRunnerTests`, add:
- `Run_WithExtractorFailure_EmitsExtractorFailureEvent()`
- `Run_WithSharpTangentChange_EmitsBranchMergeSplitCandidate()`
- `Run_WithGaugeDrift_EmitsGaugeSliceBreakdown()`

---

## GAP-3 (MEDIUM): CoulombSliceOperator Uses Flat d^* Not Covariant d_{A0}^*

### Problem

`CoulombSliceOperator.cs` line 81 has:

```csharp
// TODO: generalize to covariant d_{A0}^* for non-zero A0
```

The current implementation uses the flat codifferential `d^*` (the adjoint of the simplicial coboundary). The physically correct Coulomb gauge condition for a connection on a principal bundle is:

```
d_{A0}^*(omega - omega_ref) = 0
```

where `d_{A0}^* = *d_{A0}*` uses the **covariant** exterior derivative twisted by the background connection `A0`. For non-zero `A0`, the covariant codifferential differs from the flat `d^*` by bracket terms:

```
d_{A0}^* beta = d^* beta - [A0, *beta]   (schematically)
```

Using flat `d^*` is an approximation valid only at `A0 = 0`.

### DoD Impact

- **DoD criterion #4**: The Hessian `H = J^T M_R J + lambda*C_{A0}^T M_0 C_{A0}` uses `C = d_{A0}^*`, not `d^*`. Using flat `d^*` gives incorrect Hessian eigenvalues for non-flat backgrounds.
- Memory note says "Flat d* used for M16; covariant d_{A0}^* deferred to later" — this is confirmed deferred work.

### What Needs to Be Done

In `CoulombSliceOperator`, accept the background connection `A0` (edge-valued, `double[]`) and implement:

```csharp
// Covariant codifferential: d_{A0}^* beta = d^* beta - sum_e [A0_e, beta_e]
// where the bracket contributes vertex-valued Lie algebra terms
public FieldTensor Apply(FieldTensor beta)
{
    var flatResult = ApplyFlatCodifferential(beta);     // existing d^* term
    var bracketResult = ApplyBracketCorrection(beta);   // -[A0, beta] term
    return Add(flatResult, bracketResult);
}
```

The bracket correction term for vertex `v` and generator `a` is:
```
(bracket term)_a^v = -sum_{e incident to v} orientation(v,e) * f_{abc} * A0_b^e * beta_c^e
```

where `f_{abc}` are structure constants and `orientation(v,e)` is ±1 from the vertex-edge incidence matrix.

**Tests to add:**
- `CoulombSliceOperator_NonFlatA0_DifferentFromFlatCodifferential()`: Verify that for a non-zero `A0`, the covariant result differs from the flat result by the bracket term.
- `CoulombSliceOperator_FlatA0_AgreesWithFlatCodifferential()`: Regression test that zero `A0` gives the same result as current implementation.
- Update the `C_* R = -Delta_gauge` identity test to verify the covariant Laplacian identity holds at non-flat `A0`.

---

## GAP-4 (MEDIUM): PrincipalSymbolSampler Covector-Norm Scaling Is Wrong Order

### Problem

In `PrincipalSymbolSampler.Sample()`, the probe perturbation is scaled by `covectorNorm` and the response is divided by `covectorNorm^2`:

```csharp
// Scale by covector norm to probe the principal symbol level
delta[globalIdx] = covectorNorm > 0 ? covectorNorm : 1.0;

// ...later...
symbolMatrix[i][j] = response.Coefficients[outIdx];
// Normalize by covector norm squared for principal symbol scaling
if (covectorNorm > 0)
    symbolMatrix[i][j] /= (covectorNorm * covectorNorm);
```

For a **first-order** differential operator (such as the Jacobian J, which includes the exterior derivative `d` and bracket terms), the principal symbol `σ(ξ)` satisfies `σ(tξ) = t·σ(ξ)`. The sampler computes:

```
σ_measured(ξ) = Op(|ξ|·δ_j) / |ξ|^2 = (|ξ| · Op(δ_j)) / |ξ|^2 = Op(δ_j) / |ξ|
```

which scales as `1/|ξ|`, not `|ξ|`. This inverts the expected homogeneity degree for a first-order operator.

For a **second-order** operator (like the Hessian H), dividing by `|ξ|^2` would be correct. The current code is correct for H but wrong for J and L_tilde.

### Impact

- Symbol records for J and L_tilde have inverted covector-norm dependence.
- PDE classification based on these records may classify correctly at unit covectors but give misleading results at non-unit covectors.
- `SymbolStudyReport` outputs are numerically incorrect for multi-covector sweeps.

### What Needs to Be Done

Add a `OperatorOrder` parameter (1 or 2) to `PrincipalSymbolSampler.Sample()`:

```csharp
public PrincipalSymbolRecord Sample(
    ILinearOperator op,
    int cellIndex,
    double[] covector,
    int localDim,
    string branchManifestId,
    GaugeStudyMode gaugeStudyMode,
    string operatorId,
    int operatorOrder = 1)   // <-- NEW
```

Then normalize consistently:

```csharp
// For order-k operator: σ(tξ) = t^k * σ(ξ)
// Probe with unit-scaled perturbation (δ[j] = 1.0) and normalize by |ξ|^k
delta[globalIdx] = 1.0;   // unit probe, covector-norm handled in normalization

// ...response is linear in probe magnitude, so:
double normFactor = System.Math.Pow(covectorNorm, operatorOrder);
symbolMatrix[i][j] = (covectorNorm > 0)
    ? response.Coefficients[outIdx] / normFactor
    : response.Coefficients[outIdx];
```

Also add the covector direction (normalized ξ/|ξ|) to `PrincipalSymbolRecord` so callers can verify the angular dependence separately from the magnitude dependence.

**Tests to add:**
- `PrincipalSymbolSampler_FirstOrderOp_SymbolScalesLinearlyWithCovectorNorm()`: Verify `σ(2ξ) ≈ 2·σ(ξ)` for a known first-order operator (e.g., the exterior derivative on a 2-simplex mesh).
- `PrincipalSymbolSampler_SecondOrderOp_SymbolScalesQuadraticallyWithCovectorNorm()`: Verify `σ(2ξ) ≈ 4·σ(ξ)` for a known second-order operator.

---

## GAP-5 (MEDIUM): Manufactured-Solution Benchmark Classes A, D, E Missing

### Problem

`IMPLEMENTATION_PLAN_P2.md` Section 10.4 specifies 5 benchmark classes (A through E). The `Gu.Phase2.ManufacturedSolutions` test project contains only Classes B and C:

| Class | Description | Status |
|-------|-------------|--------|
| A | Branch parity benchmark | **MISSING** |
| B | Linearization benchmark | ✓ (ClassB_LinearizationRecord_*, ClassB_HessianRecord_*, ClassB_StabilityAtlas_*) |
| C | Gauge/slice benchmark | ✓ (ClassC_GaugeFixedLinearization_*) |
| D | Extraction benchmark | **MISSING** |
| E | Comparison dry-run benchmark | **MISSING** |

### What Needs to Be Done

Add the following to `ManufacturedSolutionTests.cs`:

**Class A — Branch Parity Benchmark**
Use a 2-simplex mesh (6 edges, 4 faces), flat connection (all zeros), and two branch variants that differ only in `torsion variant` (trivial vs. augmented with zero `A0`). For zero `A0`, both torsion variants should produce identical residuals.

```csharp
[Fact]
public void ClassA_BranchParity_IdenticalTorsionOutput_ForZeroA0()
{
    // Two BranchVariantManifests differing in TorsionVariant only
    // Flat connection (omega = 0), A0 = 0
    // Expected: D_obs(b1, b2) = 0
}

[Fact]
public void ClassA_BranchParity_DifferentTorsionOutput_ForNonZeroA0()
{
    // Same setup but A0 ≠ 0
    // Expected: D_obs(b1, b2) > 0 (measures branch sensitivity scale)
}
```

**Class D — Extraction Benchmark**
Construct a known curvature 2-form on a 3-simplex mesh, apply the extraction projector (`TensorField` kind), and verify that `ObservedOutputRecord.Values` matches the projected face values within tolerance.

```csharp
[Fact]
public void ClassD_ExtractionProjector_RecoversFaceValues_ForKnownCurvature()
{
    // Construct ExtractionProjectorRecord with identity projection
    // Apply to known curvature coefficients
    // Verify ObservedOutputRecord matches expected projected values
}

[Fact]
public void ClassD_ExtractionProjector_ObservedOutputKind_IsTensorField()
{
    // Verify Kind = ObservedOutputKind.TensorField for curvature extraction
}
```

**Class E — Comparison Dry-Run Benchmark**
Create a `PredictionTestRecord` with a synthetic `ComparisonAsset` loaded from `InMemoryDatasetAdapter`, run a full `CampaignRunner` cycle, and verify that negative results are preserved when the comparison fails.

```csharp
[Fact]
public void ClassE_ComparisonDryRun_SyntheticAsset_ProducesRunRecord()
{
    // Setup InMemoryDatasetAdapter with a synthetic comparison asset
    // Create PredictionTestRecord with matching ComparisonAsset
    // Run CampaignRunner
    // Verify ComparisonRunRecord produced with explicit score and pass/fail
}

[Fact]
public void ClassE_ComparisonDryRun_MismatchedData_ProducesNegativeArtifact()
{
    // Same setup but with data that fails structural comparison
    // Verify NegativeResultArtifact produced as first-class artifact
}
```

---

## GAP-6 (LOW): No File-Backed External Comparison Dataset Test

### Problem

**DoD criterion #10**: "At least one external comparison campaign runs end-to-end — with uncertainty decomposition & falsifier handling."

The integration tests (`S6_ComparisonCampaign_ProducesRunRecords_And_NegativeArtifacts`) use `InMemoryDatasetAdapter`. The `JsonFileDatasetAdapter` exists and loads comparison assets from JSON files on disk, but there are no tests that:

1. Write a JSON comparison asset file to a temp directory
2. Point `JsonFileDatasetAdapter` at it
3. Run a full `CampaignRunner` end-to-end
4. Verify the comparison variables are loaded and the result traces back to the file path

This means the file-based external data path is untested, and any breaking changes to the JSON schema or adapter would go undetected.

### What Needs to Be Done

Add `JsonFileDatasetAdapterIntegrationTests.cs` to `Gu.Phase2.IntegrationTests`:

```csharp
[Fact]
public void S6_FileBackedCampaign_LoadsAssetFromDisk_ProducesRunRecord()
{
    // 1. Create a temp directory and write a valid ComparisonAsset JSON file
    //    with AssetId, SourceCitation, ComparisonVariables, UncertaintyModel
    // 2. Construct JsonFileDatasetAdapter pointing at temp directory
    // 3. Create PredictionTestRecord referencing the AssetId
    // 4. Run CampaignRunner with StructuralComparison mode
    // 5. Assert ComparisonRunRecord produced, adapter path in provenance
    // Cleanup temp directory
}

[Fact]
public void S6_FileBackedCampaign_MissingFile_ProducesFailureRecord()
{
    // Point adapter at temp directory, reference a nonexistent AssetId
    // Assert ComparisonFailureRecord produced, not exception
}
```

---

## GAP-7 (LOW): 2 xUnit2012 Build Warnings in PipelineIntegrationTests.cs

### Problem

The build produces 2 warnings:

```
tests/Gu.Phase2.IntegrationTests/PipelineIntegrationTests.cs(97,9): warning xUnit2012:
  Do not use Assert.True() to check if a value exists in a collection.
  Use Assert.Contains instead.

tests/Gu.Phase2.IntegrationTests/PipelineIntegrationTests.cs(291,9): warning xUnit2012:
  Do not use Assert.True() to check if a value exists in a collection.
  Use Assert.Contains instead.
```

### What Needs to Be Done

At lines 97 and 291 of `PipelineIntegrationTests.cs`, replace:

```csharp
// line 97:
Assert.True(collection.Any(x => predicate(x)), message);
// →
Assert.Contains(collection, x => predicate(x));

// line 291:
Assert.True(collection.Any(x => predicate(x)), message);
// →
Assert.Contains(collection, x => predicate(x));
```

After the fix, `dotnet build` must produce 0 warnings, 0 errors.

---

## Definition of Done

Phase II Round 2 complete when:

- [ ] **GAP-1 resolved**: Either (a) CUDA kernels implement real Jv/J^Tw/Hv for a non-zero connection on a test mesh with parity-verified results, or (b) `Phase2CudaBackend` throws `NotSupportedException` for non-zero states with a clear "stub only" message, and the build notes this explicitly.
- [ ] **GAP-2 resolved**: `ContinuationRunner` emits `ExtractorFailure`, `BranchMergeSplitCandidate`, and `GaugeSliceBreakdown` events in appropriate conditions; 3 new tests pass.
- [ ] **GAP-3 resolved**: `CoulombSliceOperator` implements covariant `d_{A0}^*`; flat-A0 regression test + non-flat new test pass.
- [ ] **GAP-4 resolved**: `PrincipalSymbolSampler` uses correct homogeneity normalization with `operatorOrder` parameter; 2 new scaling tests pass.
- [ ] **GAP-5 resolved**: Manufactured-solution classes A, D, E each have ≥2 tests in `Gu.Phase2.ManufacturedSolutions`; all pass.
- [ ] **GAP-6 resolved**: `JsonFileDatasetAdapter` exercised in integration test with actual temp file; 2 new tests pass.
- [ ] **GAP-7 resolved**: 0 build warnings (`dotnet build` output clean).
- [ ] `dotnet build && dotnet test --no-build` — 0 errors, 0 warnings, all tests pass.

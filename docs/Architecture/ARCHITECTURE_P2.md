# Phase II Architecture Plan

## 1. Project Structure

### New Projects (under `/src`)

| Project | Purpose | Dependencies (Phase I) | Dependencies (Phase II) |
|---------|---------|----------------------|------------------------|
| `Gu.Phase2.Semantics` | Core Phase II types: BranchVariantManifest, EquivalenceSpec, study specs, field layout | Gu.Core | -- |
| `Gu.Phase2.Branches` | Branch family orchestration, variant dispatch, sweep runner | Gu.Core, Gu.Branching, Gu.Solvers | Gu.Phase2.Semantics |
| `Gu.Phase2.Canonicity` | Canonicity dockets, evidence records, pairwise comparison matrices | Gu.Core | Gu.Phase2.Semantics, Gu.Phase2.Branches |
| `Gu.Phase2.Stability` | Linearization records, Hessian operator, spectrum probes, gauge-fixed operators | Gu.Core, Gu.Branching, Gu.Solvers | Gu.Phase2.Semantics |
| `Gu.Phase2.Continuation` | Pseudo-arclength continuation, event detection, stability atlas | Gu.Core, Gu.Solvers | Gu.Phase2.Semantics, Gu.Phase2.Stability |
| `Gu.Phase2.Recovery` | Recovery DAG, interpretation nodes, physical-identification gate | Gu.Core, Gu.Observation | Gu.Phase2.Semantics |
| `Gu.Phase2.Predictions` | PredictionTestRecord, ComparisonCampaignSpec, calibration, uncertainty | Gu.Core, Gu.ExternalComparison | Gu.Phase2.Semantics, Gu.Phase2.Recovery |
| `Gu.Phase2.Comparison` | Campaign runner, comparison modes, negative-result artifacts | Gu.Core, Gu.ExternalComparison, Gu.Observation | Gu.Phase2.Semantics, Gu.Phase2.Predictions |
| `Gu.Phase2.Reporting` | Report generation, docket aggregation, dashboard exports | Gu.Core | Gu.Phase2.Semantics, Gu.Phase2.Canonicity, Gu.Phase2.Stability, Gu.Phase2.Predictions |

### New Test Projects (under `/tests`)

| Project | Tests For |
|---------|-----------|
| `Gu.Phase2.Semantics.Tests` | All Gu.Phase2.Semantics types |
| `Gu.Phase2.Branches.Tests` | Branch family orchestration, variant dispatch |
| `Gu.Phase2.Canonicity.Tests` | Dockets, evidence, comparison matrices |
| `Gu.Phase2.Stability.Tests` | Linearization, Hessian, spectrum probes |
| `Gu.Phase2.Continuation.Tests` | Continuation, event detection |
| `Gu.Phase2.Recovery.Tests` | Recovery DAG, gate enforcement |
| `Gu.Phase2.Predictions.Tests` | Prediction records, campaigns |
| `Gu.Phase2.Comparison.Tests` | Campaign runner, comparison modes |
| `Gu.Phase2.Reporting.Tests` | Report generation |

### Schemas (under `/schemas`)

- `branch_family.schema.json`
- `branch_variant.schema.json`
- `canonicity_docket.schema.json`
- `equivalence_spec.schema.json`
- `study_spec.schema.json`
- `prediction_test_record.schema.json`
- `comparison_campaign.schema.json`
- `stability_record.schema.json`
- `continuation_record.schema.json`

---

## 2. Key Types by Project

### Gu.Phase2.Semantics

This is the foundational project. All other Phase II projects depend on it. It holds pure data types with no behavior beyond validation.

```csharp
// Branch family types (Section 5.1, 8.1)
public sealed class BranchFamilyManifest
{
    public required string FamilyId { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<BranchVariantManifest> Variants { get; init; }
    public required EquivalenceSpec DefaultEquivalence { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class BranchVariantManifest
{
    public required string Id { get; init; }
    public required string ParentFamilyId { get; init; }
    // Each variant field maps to a Phase I BranchManifest field
    public required string A0Variant { get; init; }
    public required string BiConnectionVariant { get; init; }
    public required string TorsionVariant { get; init; }
    public required string ShiabVariant { get; init; }
    public required string ObservationVariant { get; init; }
    public required string ExtractionVariant { get; init; }
    public required string GaugeVariant { get; init; }
    public required string RegularityVariant { get; init; }
    public required string PairingVariant { get; init; }
    public required string ExpectedClaimCeiling { get; init; }
    public string? Notes { get; init; }
}

public sealed class EquivalenceSpec
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<string> ComparedObjectClasses { get; init; }
    public required string NormalizationProcedure { get; init; }
    public required IReadOnlyList<string> AllowedTransformations { get; init; }
    public required IReadOnlyList<string> Metrics { get; init; }
    public required IReadOnlyDictionary<string, double> Tolerances { get; init; }
    public required string InterpretationRule { get; init; }
}

// Field layout abstraction (Section 2.3)
public sealed class BranchFieldLayout
{
    public required IReadOnlyList<FieldBlockDescriptor> ConnectionBlocks { get; init; }
    public required IReadOnlyList<FieldBlockDescriptor> AuxiliaryBosonicBlocks { get; init; }
    public required IReadOnlyList<string> GaugeActionRules { get; init; }
    public required IReadOnlyList<string> ObservationEligibility { get; init; }
}

public sealed class FieldBlockDescriptor
{
    public required string BlockId { get; init; }
    public required string FieldType { get; init; }  // "connection", "auxiliary-scalar", etc.
    public required int DofCount { get; init; }
    public required TensorSignature Signature { get; init; }
}

// Study specification types (Section 8.5)
public sealed class BranchSweepSpec
{
    public required string StudyId { get; init; }
    public required string EnvironmentId { get; init; }
    public required string FamilyId { get; init; }
    public required IReadOnlyList<string> VariantIds { get; init; }
    public required SolveMode InnerSolveMode { get; init; }
    public required EquivalenceSpec Equivalence { get; init; }
}

public sealed class StabilityStudySpec
{
    public required string StudyId { get; init; }
    public required string BackgroundStateId { get; init; }
    public required string BranchManifestId { get; init; }
    public required string GaugeHandlingMode { get; init; }  // "gauge-free", "coulomb-slice", etc.
    public required int RequestedModeCount { get; init; }
    public required string SpectrumProbeMethod { get; init; }  // "lanczos", "lobpcg", "arnoldi"
    // PHYSICIST-RECOMMENDED: stability thresholds declared per-study, never hardcoded
    public required double SoftModeThreshold { get; init; }       // eigenvalues below this are "soft"
    public required double NearKernelThreshold { get; init; }     // eigenvalues below this are "near-kernel"
    public required double NegativeModeThreshold { get; init; }   // eigenvalues below this are "negative" (should be ~0 or small negative)
}

public sealed class ResearchBatchSpec
{
    public required string BatchId { get; init; }
    public required IReadOnlyList<BranchSweepSpec> Sweeps { get; init; }
    public required IReadOnlyList<StabilityStudySpec> StabilityStudies { get; init; }
    public required IReadOnlyList<string> ComparisonCampaignIds { get; init; }
}

// Claim class enum (Section 5.7)
public enum ClaimClass
{
    ExactStructuralConsequence,
    ApproximateStructuralSurrogate,
    PostdictionTarget,
    SpeculativeInterpretation,
    Inadmissible,   // auto-demotion target
}

// Comparison mode (Section 13.3)
public enum ComparisonMode
{
    Structural,
    SemiQuantitative,
    Quantitative,
}
```

**Key pattern**: `BranchVariantManifest` maps 1:1 to a Phase I `BranchManifest` instance. A utility method `ToPhase1Manifest(BranchVariantManifest variant, BranchManifest baseManifest) -> BranchManifest` creates a concrete Phase I manifest by overriding variant-specific fields. This preserves backward compatibility.

### Gu.Phase2.Branches

Depends on: Gu.Phase2.Semantics, Gu.Core, Gu.Branching, Gu.Solvers

```csharp
// Converts BranchVariantManifest + base manifest -> Phase I BranchManifest
public static class BranchVariantResolver
{
    public static BranchManifest Resolve(
        BranchVariantManifest variant,
        BranchManifest baseManifest);
}

// Orchestrates multi-branch sweep using Phase I SolverOrchestrator
public sealed class Phase2BranchSweepRunner
{
    // Runs each variant through Phase I SolverOrchestrator
    // Produces Phase2BranchSweepResult (extends Phase I BranchSweepResult)
    public Phase2BranchSweepResult Sweep(
        BranchSweepSpec spec,
        BranchFamilyManifest family,
        BranchManifest baseManifest,
        FieldTensor initialOmega,
        FieldTensor a0,
        GeometryContext geometry,
        ISolverBackend backend,
        ObservationPipeline? observationPipeline);
}

public sealed class Phase2BranchSweepResult
{
    public required BranchSweepSpec Spec { get; init; }
    public required IReadOnlyList<Phase2BranchRunRecord> RunRecords { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}

public sealed class Phase2BranchRunRecord
{
    public required BranchVariantManifest Variant { get; init; }
    public required BranchManifest ResolvedManifest { get; init; }
    public required SolverResult SolverResult { get; init; }
    public required DerivedState DerivedState { get; init; }
    public ObservedState? ObservedState { get; init; }
    public required ArtifactBundle ArtifactBundle { get; init; }
}
```

**Key pattern**: Reuses Phase I `SolverOrchestrator` and `ISolverBackend`. The Phase 2 runner is a thin wrapper that iterates over variants and collects results.

### Gu.Phase2.Canonicity

Depends on: Gu.Phase2.Semantics, Gu.Phase2.Branches, Gu.Core

```csharp
public sealed class CanonicityDocket
{
    public required string ObjectClass { get; init; }  // "A0", "torsion", "shiab", etc.
    public required string ActiveRepresentative { get; init; }
    public required string EquivalenceRelationId { get; init; }
    public required string AdmissibleComparisonClass { get; init; }
    public required IReadOnlyList<string> DownstreamClaimsBlockedUntilClosure { get; init; }
    public required IReadOnlyList<CanonicityEvidenceRecord> CurrentEvidence { get; init; }
    public required IReadOnlyList<string> KnownCounterexamples { get; init; }
    public required IReadOnlyList<string> PendingTheorems { get; init; }
    public required IReadOnlyList<string> StudyReports { get; init; }
    public required DocketStatus Status { get; init; }
}

public enum DocketStatus { Open, EvidenceAccumulating, ClosedByTheorem, ClosedByClassification, Falsified }

public sealed class CanonicityEvidenceRecord
{
    public required string EvidenceId { get; init; }
    public required string StudyId { get; init; }
    public required string Verdict { get; init; }  // "consistent", "inconsistent", "inconclusive"
    public required double MaxObservedDeviation { get; init; }
    public required double Tolerance { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}

// Pairwise comparison matrices (Section 9.6)
public sealed class PairwiseDistanceMatrix
{
    public required string MetricId { get; init; }
    public required IReadOnlyList<string> BranchIds { get; init; }
    public required double[,] Distances { get; init; }
}

// Builds pairwise matrices from sweep results
public sealed class CanonicityAnalyzer
{
    public PairwiseDistanceMatrix ComputeObservedDistances(
        Phase2BranchSweepResult sweepResult,
        EquivalenceSpec equivalence);

    public PairwiseDistanceMatrix ComputeDynamicDistances(
        Phase2BranchSweepResult sweepResult);

    public CanonicityEvidenceRecord Evaluate(
        Phase2BranchSweepResult sweepResult,
        EquivalenceSpec equivalence,
        string objectClass);

    public CanonicityDocket UpdateDocket(
        CanonicityDocket existing,
        CanonicityEvidenceRecord newEvidence);
}
```

### Gu.Phase2.Stability

Depends on: Gu.Phase2.Semantics, Gu.Core, Gu.Branching, Gu.Solvers

```csharp
// Gauge-fixed linearized operator L_tilde (Section 9.3)
// IMPORTANT: L_tilde is the algebraic operator only. ApplyTranspose is the
// algebraic transpose (matching Phase I ILinearOperator convention).
// Mass matrix weighting is handled EXTERNALLY by the Hessian/caller.
public sealed class GaugeFixedLinearOperator : ILinearOperator
{
    // Wraps J (from Phase I) and C (gauge/slice constraint)
    // Apply: delta_u -> (J delta_u, C delta_u)  [stacked output]
    // ApplyTranspose: (w1, w2) -> J^T w1 + C^T w2  [split input]
    public GaugeFixedLinearOperator(
        ILinearOperator jacobian,
        ILinearOperator gaugeConstraint);
}

// Hessian operator H = L_tilde^T M L_tilde (Section 9.3)
// PHYSICIST-CONFIRMED: Mass matrices are REQUIRED. Without them, eigenvalue
// magnitudes and mode ordering are mesh-dependent rather than physical.
//
// H(v) = J^T M_R J v + C^T M_0 C v
//
// where M_R is the residual-space mass matrix and M_0 is the gauge-constraint-space
// mass matrix. Apply pattern: L_tilde.Apply(v) -> productMass(Lv) -> L_tilde.ApplyTranspose(MLv)
public sealed class HessianOperator : ILinearOperator
{
    // productMass applies the block-diagonal mass matrix on the product space:
    // (w1, w2) -> (M_R w1, M_0 w2)
    public HessianOperator(
        GaugeFixedLinearOperator lTilde,
        ILinearOperator residualMass,    // M_R on residual space
        ILinearOperator gaugeMass);      // M_0 on gauge constraint space

    // Apply(v) = L_tilde^T * blockMass * L_tilde * v
}

// Infinitesimal gauge map R_{z_*} (Section 2.7 / 9.3)
// PHYSICIST-CONFIRMED: This is DISTINCT from the gauge constraint C.
// R maps gauge parameters -> connection perturbations:
//   R_{z_*}(xi) = -d_{A0}(xi) + [A_* - A0, xi]
// Needed for: null space verification, gauge Laplacian R^T M R,
// gauge vs physical mode identification, continuation event detection.
public sealed class InfinitesimalGaugeMap : ILinearOperator
{
    public InfinitesimalGaugeMap(
        FieldTensor a0,           // background connection A0
        FieldTensor aStar,        // background state A_*
        GeometryContext geometry,
        BranchManifest manifest);

    // Apply(xi) = -d_{A0}(xi) + [A_* - A0, xi]
    // ApplyTranspose(w) = algebraic transpose (for gauge Laplacian R^T M R)
    // InputDimension = gauge parameter DOFs
    // OutputDimension = connection perturbation DOFs
}

// Gauge constraint operator (Section 9.2)
public interface IGaugeConstraintOperator : ILinearOperator
{
    string GaugeHandlingMode { get; }
}

// Coulomb gauge constraint: C(u) = d_{A0}^*(omega - omega_ref)
public sealed class CoulombGaugeConstraint : IGaugeConstraintOperator { ... }

// Spectrum probe interface (Section 10.3)
public interface ISpectrumProbe
{
    string MethodId { get; }  // "lanczos", "lobpcg", "arnoldi"
    SpectrumResult Probe(ILinearOperator op, int requestedModes);
}

public sealed class SpectrumResult
{
    public required IReadOnlyList<double> Eigenvalues { get; init; }
    public required IReadOnlyList<double[]>? Eigenvectors { get; init; }
    public required int RequestedModes { get; init; }
    public required int ObtainedModes { get; init; }
    public required bool Converged { get; init; }
    public required string NormalizationConvention { get; init; }
    public required string GaugeHandlingMode { get; init; }
}

// Lanczos implementation for symmetric H
public sealed class LanczosProbe : ISpectrumProbe { ... }

// Record types (Section 8.2)
public sealed class BackgroundStateRecord
{
    public required string BackgroundId { get; init; }
    public required string BranchManifestId { get; init; }
    public required FieldTensor State { get; init; }
    public required double ObjectiveValue { get; init; }
    public required double ResidualNorm { get; init; }
}

public sealed class LinearizationRecord
{
    public required string BackgroundStateId { get; init; }
    public required string BranchManifestId { get; init; }
    public required string LinearizedOperatorDefId { get; init; }
    public required string DerivativeMode { get; init; }  // "analytic", "finite-difference", "hybrid"
    public required TensorSignature DomainSignature { get; init; }
    public required TensorSignature CodomainSignature { get; init; }
    public required string GaugeHandlingMode { get; init; }
    public required string AssemblyMode { get; init; }  // "matrix-free", "explicit-sparse", "block-hybrid"
    public required string ValidationStatus { get; init; }
}

public sealed class HessianRecord
{
    public required string BackgroundStateId { get; init; }
    public required string BranchManifestId { get; init; }
    public required SpectrumResult? Spectrum { get; init; }
    public required string StabilityClassification { get; init; }  // "coercive", "soft", "singular", "saddle"
    // PHYSICIST-RECOMMENDED: mode counts per stability category
    public required int CoerciveModeCount { get; init; }
    public required int SoftModeCount { get; init; }       // small positive eigenvalues
    public required int NearKernelCount { get; init; }     // near-zero eigenvalues
    public required int NegativeModeCount { get; init; }   // saddle directions
}

// Principal-symbol sampler (Section 9.4)
public sealed class PrincipalSymbolRecord
{
    public required int CellIndex { get; init; }
    public required double[] Covector { get; init; }
    public required double[,] SymbolMatrix { get; init; }
    public required bool IsSymmetric { get; init; }
    public required string DefinitenessIndicator { get; init; }
    public required int RankDeficiency { get; init; }
    public required int GaugeNullDimension { get; init; }
    public required string BranchManifestId { get; init; }
}

public sealed class PrincipalSymbolSampler
{
    // Probe frozen-coefficient symbol at (cell, covector)
    public PrincipalSymbolRecord Sample(
        ILinearOperator op,
        GeometryContext geometry,
        int cellIndex,
        double[] covector,
        BranchManifest manifest);
}
```

### Gu.Phase2.Continuation

Depends on: Gu.Phase2.Semantics, Gu.Phase2.Stability, Gu.Core, Gu.Solvers

```csharp
// Pseudo-arclength continuation (Section 10.2)
public sealed class ContinuationRunner
{
    // Track solution family G(u, lambda) = 0
    public ContinuationResult Run(ContinuationSpec spec, ...);
}

public sealed class ContinuationSpec
{
    public required string ParameterName { get; init; }
    public required double LambdaStart { get; init; }
    public required double LambdaEnd { get; init; }
    public required double InitialStepSize { get; init; }
    public required int MaxSteps { get; init; }
    public required double MinStepSize { get; init; }
    public required double MaxStepSize { get; init; }
}

public sealed class ContinuationStep
{
    public required double Lambda { get; init; }
    public required FieldTensor State { get; init; }
    public required double ResidualNorm { get; init; }
    public required SpectrumResult? StabilityDiagnostics { get; init; }
    public required IReadOnlyList<ContinuationEvent> Events { get; init; }
}

public enum ContinuationEventKind
{
    SingularValueCollapse,
    HessianSignChange,
    StepRejectionBurst,
    ExtractorFailure,
    BranchMergeSplitCandidate,
    GaugeSliceBreakdown,
}

public sealed class ContinuationEvent
{
    public required ContinuationEventKind Kind { get; init; }
    public required double Lambda { get; init; }
    public required string Description { get; init; }
}

public sealed class ContinuationResult
{
    public required IReadOnlyList<ContinuationStep> Path { get; init; }
    public required string TerminationReason { get; init; }
}

// Stability atlas = collection of continuation paths + spectrum data
public sealed class StabilityAtlas
{
    public required string AtlasId { get; init; }
    public required IReadOnlyList<ContinuationResult> Paths { get; init; }
    public required IReadOnlyList<HessianRecord> HessianRecords { get; init; }
    public required IReadOnlyList<PrincipalSymbolRecord> SymbolSamples { get; init; }
}
```

### Gu.Phase2.Recovery

Depends on: Gu.Phase2.Semantics, Gu.Core, Gu.Observation

```csharp
// Recovery DAG (Section 12.1)
public sealed class RecoveryGraph
{
    public required IReadOnlyList<RecoveryNode> Nodes { get; init; }
    public required IReadOnlyList<RecoveryEdge> Edges { get; init; }

    // Validate: every path NativeNode -> ObservationNode -> ExtractionNode -> InterpretationNode
    public bool Validate();
}

public enum RecoveryNodeKind { Native, Observation, Extraction, Interpretation }

public sealed class RecoveryNode
{
    public required string NodeId { get; init; }
    public required RecoveryNodeKind Kind { get; init; }
    public required string SourceObjectType { get; init; }
    public TensorSignature? Signature { get; init; }
    public required string BranchProvenanceId { get; init; }
    public required bool GaugeDependent { get; init; }
    public required string NumericalDependencyStatus { get; init; }
    public required string TheoremDependencyStatus { get; init; }
}

public sealed class RecoveryEdge
{
    public required string FromNodeId { get; init; }
    public required string ToNodeId { get; init; }
    public required string OperatorId { get; init; }  // e.g., "sigma_h_star", "trace-projector"
}

// Observed output classes (Section 12.2)
public enum ObservedOutputKind
{
    TensorField,
    ScalarInvariant,
    ModeSpectrum,
    ResponseCurve,
    StructuralPattern,
    ComparisonQuantity,
}

public sealed class ObservedOutputRecord
{
    public required string OutputId { get; init; }
    public required ObservedOutputKind Kind { get; init; }
    public required ObservableSnapshot Snapshot { get; init; }
    public required string RecoveryNodeId { get; init; }
    public required ClaimClass ClaimCeiling { get; init; }
}

// Physical-identification gate (Section 9.8)
public sealed class PhysicalIdentificationRecord
{
    public required string IdentificationId { get; init; }
    public required string FormalSource { get; init; }
    public required string ObservationExtractionMap { get; init; }
    public required string SupportStatus { get; init; }     // "theorem-supported", "numerical-only", "conjectural"
    public required string ApproximationStatus { get; init; }
    public required string ComparisonTarget { get; init; }
    public required string Falsifier { get; init; }
    public required ClaimClass ResolvedClaimClass { get; init; }
}

// Gate enforcement
public static class IdentificationGate
{
    // Returns Inadmissible if any required field is missing
    public static ClaimClass Evaluate(PhysicalIdentificationRecord record);
}
```

### Gu.Phase2.Predictions

Depends on: Gu.Phase2.Semantics, Gu.Phase2.Recovery, Gu.Core, Gu.ExternalComparison

```csharp
public sealed class PredictionTestRecord
{
    public required string TestId { get; init; }
    public required ClaimClass ClaimClass { get; init; }
    public required string FormalSource { get; init; }
    public required string BranchManifestId { get; init; }
    public required string ObservableMapId { get; init; }
    public required string TheoremDependencyStatus { get; init; }
    public required string NumericalDependencyStatus { get; init; }
    public required string ApproximationStatus { get; init; }
    public ComparisonAsset? ExternalComparisonAsset { get; init; }
    public required string Falsifier { get; init; }
    public required IReadOnlyList<string> ArtifactLinks { get; init; }
    public string? Notes { get; init; }
}

public sealed class ComparisonAsset
{
    public required string AssetId { get; init; }
    public required string SourceCitation { get; init; }
    public required DateTimeOffset AcquisitionDate { get; init; }
    public required string PreprocessingDescription { get; init; }
    public required string AdmissibleUseStatement { get; init; }
    public required string DomainOfValidity { get; init; }
    public required UncertaintyRecord UncertaintyModel { get; init; }
    public required IReadOnlyDictionary<string, string> ComparisonVariables { get; init; }
}

public sealed class UncertaintyRecord
{
    public required double Discretization { get; init; }
    public required double Solver { get; init; }
    public required double Branch { get; init; }
    public required double Extraction { get; init; }
    public required double Calibration { get; init; }
    public required double DataAsset { get; init; }
    // -1 = Unestimated
}

public sealed class ComparisonCampaignSpec
{
    public required string CampaignId { get; init; }
    public required IReadOnlyList<string> EnvironmentIds { get; init; }
    public required IReadOnlyList<string> BranchSubsetIds { get; init; }
    public required IReadOnlyList<string> ObservedOutputClassIds { get; init; }
    public required IReadOnlyList<string> ComparisonAssetIds { get; init; }
    public required ComparisonMode Mode { get; init; }
    public required string CalibrationPolicy { get; init; }  // "fixed", "fitted", "inverse", "sensitivity-only"
}

// Validation rules (Section 9.9)
public static class PredictionValidator
{
    // Enforces: missing falsifier -> invalid, open theorem -> not theorem-level, etc.
    public static PredictionTestRecord Validate(PredictionTestRecord record);
}
```

### Gu.Phase2.Comparison

Depends on: Gu.Phase2.Predictions, Gu.Phase2.Semantics, Gu.Core, Gu.ExternalComparison, Gu.Observation

```csharp
public sealed class CampaignRunner
{
    public ComparisonCampaignResult Run(
        ComparisonCampaignSpec spec,
        IReadOnlyDictionary<string, ComparisonAsset> assets,
        IReadOnlyDictionary<string, ObservedOutputRecord> outputs);
}

public sealed class ComparisonCampaignResult
{
    public required string CampaignId { get; init; }
    public required IReadOnlyList<ComparisonRunRecord> RunRecords { get; init; }
    public required IReadOnlyList<ComparisonFailureRecord> Failures { get; init; }
}

public sealed class ComparisonRunRecord
{
    public required string TestId { get; init; }
    public required ComparisonMode Mode { get; init; }
    public required double Score { get; init; }
    public required UncertaintyRecord Uncertainty { get; init; }
    public required ClaimClass ResolvedClaimClass { get; init; }
}

public sealed class ComparisonFailureRecord
{
    public required string TestId { get; init; }
    public required string FailureReason { get; init; }
    public required string FailureLevel { get; init; }  // "numerical", "branch-local", "extraction", "empirical"
    public required bool FalsifiesRecord { get; init; }
    public required bool BlocksCampaign { get; init; }
    public required ClaimClass DemotedClaimClass { get; init; }
}
```

### Gu.Phase2.Reporting

Depends on: most Phase II projects

```csharp
public sealed class ResearchReportGenerator
{
    public ResearchReport Generate(ResearchBatchSpec batch, ...);
}

public sealed class ResearchReport
{
    public required string ReportId { get; init; }
    public required IReadOnlyList<string> BranchLocalConclusions { get; init; }
    public required IReadOnlyList<string> ComparisonReadyConclusions { get; init; }
    public required IReadOnlyList<string> OpenItems { get; init; }
    public required IReadOnlyList<string> NumericalOnlyResults { get; init; }
    public required IReadOnlyList<string> UninterpretedOutputs { get; init; }
    public required IReadOnlyList<string> RuledOutClaims { get; init; }
    public required IReadOnlyList<CanonicityDocket> Dockets { get; init; }
}
```

---

## 3. Milestone-to-Project Mapping and Parallelization

### Dependency Graph

```
M13 (Semantics + Branches + Canonicity scaffolding)
  ├──> M14 (Branches: sweep runner + variant dispatch)
  │       ├──> M15 (Canonicity: evidence engine)
  │       └──> M19 (Recovery: DAG + gates)
  │               └──> M20 (Predictions: test matrix + campaigns)
  │                       └──> M21 (Comparison: expansion)
  │                               └──> M22 (Reporting: batch + reports)
  └──> M16 (Stability: linearization + Hessian)
          ├──> M17 (Stability: principal symbol + PDE)
          └──> M18 (Continuation: atlas)
```

### Parallelization Opportunities

Two independent tracks after M13:

- **Track A (Branch/Canonicity/Recovery)**: M14 -> M15, M19 -> M20 -> M21 -> M22
- **Track B (Stability/PDE/Continuation)**: M16 -> M17, M18

M13 must be completed first as it provides the core types.

### Implementer Assignment Recommendation

| Implementer | Milestones | Track |
|-------------|-----------|-------|
| impl-1 | M13 (types + project scaffolding) | Foundation |
| impl-2 | M14 (variant dispatch + sweep runner) | Track A |
| impl-3 | M16 (linearization + Hessian + spectrum) | Track B |
| impl-4 | M19 (recovery DAG + gates) | Track A (after M14) |
| impl-5 | M15 (canonicity engine) | Track A (after M14) |

Later milestones (M17, M18, M20, M21, M22) assigned after dependencies complete.

---

## 4. Conventions to Follow

### Matching Phase I Patterns

1. **Sealed classes, not records**: All types use `public sealed class` with `required` init-only properties
2. **JSON serialization**: `[JsonPropertyName("camelCase")]` on all serializable properties
3. **Namespace convention**: `Gu.Phase2.{ProjectName}` (e.g., `Gu.Phase2.Semantics`)
4. **Interface location**: Interfaces in the project that defines the abstraction (e.g., `ISpectrumProbe` in `Gu.Phase2.Stability`)
5. **CPU implementations**: In the same project as the interface (Phase II doesn't need separate ReferenceCpu since it builds on Phase I's ISolverBackend)
6. **Factory pattern**: Follow `BranchOperatorRegistry` pattern for any extensible dispatch
7. **Test naming**: `[MethodName]_[Scenario]_[ExpectedResult]` or similar xUnit convention
8. **System.Math**: Use `System.Math.Sqrt()` etc. in files that import `Gu.Math`

### Project File Template

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Directory.Build.props provides TFM, nullable, etc. -->
  <ItemGroup>
    <ProjectReference Include="../../src/Gu.Core/Gu.Core.csproj" />
    <!-- other references -->
  </ItemGroup>
</Project>
```

### Test Project Template

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/Gu.Phase2.Semantics/Gu.Phase2.Semantics.csproj" />
  </ItemGroup>
</Project>
```

---

## 5. Key Integration Points with Phase I

### BranchVariantManifest -> BranchManifest Bridge

The most critical integration: `BranchVariantResolver.Resolve()` creates a Phase I `BranchManifest` from a variant by overriding fields in a base manifest. This means all Phase I solver/operator infrastructure works unchanged.

### ISolverBackend Reuse

Phase II does NOT create a new solver backend. It reuses `ISolverBackend` and `SolverOrchestrator` from Phase I. The Phase II sweep runner is an outer loop calling Phase I's solve.

### ILinearOperator Extension

Phase II adds `GaugeFixedLinearOperator` and `HessianOperator` which implement the Phase I `ILinearOperator` interface. The existing `ILinearOperator.Apply`/`ApplyTranspose` contract works for Hessian and L_tilde.

### ObservationPipeline Reuse

Phase II reuses `ObservationPipeline.Extract()` per-branch. The recovery graph wraps this with explicit provenance tracking but doesn't replace the pipeline.

### BranchOperatorRegistry Extension

Phase II may register additional torsion/shiab variants via the existing `BranchOperatorRegistry`. No new registry needed.

### Artifact Extension

Phase II adds new artifact types (BranchSweepArtifact, CanonicityDocketArtifact, StabilityAtlasArtifact, etc.) as new sealed classes in `Gu.Phase2.Semantics`. These are separate from Phase I's `ArtifactBundle` but follow the same serialization patterns.

---

## 6. "Do Not" Rules (from Plan Section 17)

1. Do NOT silently hardcode one Shiab operator as canonical
2. Do NOT collapse Y into X
3. Do NOT compare raw native-space field coefficients to external data
4. Do NOT treat numerical convergence as proof of well-posedness
5. Do NOT erase branch metadata during GPU execution
6. Do NOT hide failed runs or unstable branches
7. Do NOT promote structural outputs to physics without the identification gate
8. Do NOT create comparison campaigns without explicit falsifiers
9. Do NOT allow CUDA-only features to bypass CPU parity checks
10. Do NOT add fermionic coupled solver into bosonic core without separate branch family

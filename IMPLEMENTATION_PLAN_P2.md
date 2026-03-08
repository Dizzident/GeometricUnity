# IMPLEMENTATION_PLAN_P2.md

# Geometric Unity Phase II Research Implementation Plan

## Purpose

This document is a **standalone Phase II implementation handoff** for Claude Code.

It assumes **no access to the original Geometric Unity completion manuscript**.
All mathematical objects, architectural rules, and implementation expectations needed for Phase II are restated here.

This plan continues the work of the Phase I implementation plan by defining the **first research-oriented extension** of the executable Geometric Unity platform.

Phase I was the **Minimal GU v1** build:

- a typed bosonic branch,
- an observerse-aware lowering pipeline,
- a CPU reference backend,
- a CUDA backend for residual and solver work,
- a typed observation pipeline,
- a replayable artifact contract,
- and a simulation environment framework.

Phase II is **not** “the whole theory.”
It is the next layer needed to use the software to **attack the actual remaining research frontier**.

The four Phase II research lanes are:

1. **branch-independence and canonicity studies**,
2. **recovery beyond branch-local extraction**,
3. **PDE classification / linearization / stability research**,
4. **quantitative comparison against external observations using typed evidence only**.

The goal is to turn the Phase I engine into a **research platform capable of producing structured evidence** about these frontier questions.

This plan must be read with one discipline in mind:

> Phase II is allowed to produce branch-local results, numerical evidence, invariance studies, counterexamples, and comparison records.
> It is **not allowed** to silently promote those into proofs, branch-independent physical claims, or final theory validation.

---

# 1. What Phase II is and is not

## 1.1 What Phase II is

Phase II is the implementation of the **research instrumentation layer** on top of Minimal GU v1.

It must provide:

1. a **multi-branch execution framework** for admissible branch variants,
2. a **canonicity / branch-sensitivity laboratory**,
3. a **linearization, deformation, and stability workbench**,
4. a **branch-traceable observed-sector recovery engine**,
5. a **typed physical-identification gate and prediction/test matrix**,
6. a **comparison campaign framework** for external data,
7. reproducible reports that preserve negative results, branch dependence, and theorem status,
8. explicit continuation of the artifact/replay contract so every research result is rerunnable.

## 1.2 What Phase II is not

Phase II is **not**:

- a proof that Geometric Unity is correct,
- a proof of branch-independence,
- a proof of global observerse existence or uniqueness,
- a complete fermionic implementation,
- a proof of local/global well-posedness for all bosonic or coupled branches,
- a final physical decomposition into Standard Model sectors,
- a production-grade large-scale HPC system,
- a full quantum completion.

Phase II may support these later, but it does not claim to complete them.

---

# 2. Minimal recap of the Phase I formal core

This section restates the minimum mathematics Claude needs in order to understand Phase II.

## 2.1 Spaces

The active executable framework uses two spaces:

- `X`: the observed/base manifold,
- `Y`: the native/observerse total space where the main dynamical fields live.

Implementation target:

- the engine should remain dimension-generic where practical,
- the default physical target remains `dim(X)=4`,
- `Y` may have different dimension and must not be collapsed into `X`.

## 2.2 Projection and observation

There are two distinct maps:

- `pi : Y -> X`, the structural projection,
- `sigma`, the observation section / observation map used to produce observed outputs on `X`.

Discrete versions:

- `pi_h`,
- `sigma_h`,
- `sigma_h^*` for pullback / descent / extraction.

No empirical comparison may bypass `sigma_h` and the declared extraction chain.

## 2.3 Principal bundle and dynamical field content

The active bosonic branch uses a principal bundle `P_main -> Y` and a connection-centered dynamics.

There are two implementation-compatible ways to represent the active bosonic state:

1. **single-block connection branch**: the primary field is a single connection-like object `omega`,
2. **two-block bosonic state branch**: `z = (A, omega_aux)` where `A` is the main connection block and `omega_aux` is an auxiliary bosonic block that may be trivial or gauge-inert.

### Mandatory compatibility rule

Phase II must not assume one representation only.

Implement a field-layout abstraction:

```text
BranchFieldLayout = {
  ConnectionBlocks: [ ... ],
  AuxiliaryBosonicBlocks: [ ... ],
  GaugeActionRules: [ ... ],
  ObservationEligibility: [ ... ]
}
```

If the Phase I codebase only implemented a single connection block, Phase II must remain backward compatible by treating:

- `A ≡ omega`,
- auxiliary blocks as empty.

## 2.4 Distinguished connection and bi-connection map

The executable branch includes a distinguished connection `A0` and a bi-connection construction:

```text
(A_omega, B_omega) = Mu_A0(state, branchParameters, geometryContext)
```

This is branch data.
It is **not** assumed canonical.

## 2.5 Curvature, torsion, Shiab, residual

The executable bosonic branch uses:

- curvature `F_{A_omega}`,
- augmented / displaced torsion `T_aug`,
- Shiab-family swerved-curvature term `S`,
- combined bosonic residual `Upsilon`.

### Active torsion abstraction

Use a branch-defined corrected torsion extractor:

```text
T_aug = TorsionBranch(A_omega, B_omega, geometryContext, branchParameters)
```

### Active Shiab abstraction

The active Shiab term is not canonical; it is chosen from an admissible family.
The minimal compatible branch is:

```text
Sigma_mc(Xi)
  = Pi_T( K_A0( d_{B_omega} Xi ) )
    + L_A0( T_aug, Xi )
```

where:

- `d_{B_omega}` is the covariant exterior derivative induced by `B_omega`,
- `K_A0` is a fixed algebraic contraction built from background metric/projection data and `A0`-dependent identifications,
- `Pi_T` projects into the corrected torsion target,
- `L_A0` is a lower-order correction restoring covariance/self-adjointness or symmetrizability requirements.

Swerved curvature is then:

```text
S = Sigma(F_{A_omega})
```

Residual:

```text
Upsilon = S - T_aug
```

## 2.6 Variational objective and stationarity

The core Phase I objective is:

```text
I2 = (1/2) ∫_Y <Upsilon, Upsilon> dmu_Y
```

Discrete form:

```text
I2_h = (1/2) Upsilon_h^T M_Upsilon Upsilon_h
```

The solver-relevant stationarity equation is represented in discrete form by:

```text
J_h^T M_Upsilon Upsilon_h = 0
```

where `J_h = ∂Upsilon_h / ∂state_h`.

## 2.7 Linearization and deformation package

Phase I already established the minimum deformation package around a declared background state `z_*`.
Phase II must build on that directly.

### Infinitesimal gauge map

```text
R_{z_*} : g^{s+1} -> T_{z_*} C_B^s
```

On the connection component:

```text
R_{z_*}(xi, nu)_A = -d_{A0} xi + nu + [A_* - A0, xi]
```

with branch-declared rules for any auxiliary bosonic blocks.

### Linearized field operator

```text
L_{z_*} = D E_B |_{z_*}
```

where `E_B` is the branch field operator / residual / gauge-fixed Euler–Lagrange operator.

### Gauge-fixed linearized operator

```text
L_tilde_{z_*}(delta z) = ( L_{z_*}(delta z), S_{z_*}(delta z) )
```

where `S_{z_*}` is the slice / gauge-fixing operator.

### Hessian-style operator

```text
H_{z_*} = L_tilde_{z_*}^* L_tilde_{z_*}
```

This operator governs:

- local stability semantics,
- soft modes,
- near-kernel structure,
- preconditioner design,
- continuation diagnostics,
- branch-fragility detection.

## 2.8 Observation and recovery discipline

Observed-sector recovery is **gated**.
Every observed quantity must pass four gates:

1. **Native-source gate**: identify the source object on `Y`,
2. **Observation gate**: identify `sigma_h`, pullback/descent, and observerse assumptions,
3. **Extraction gate**: identify the typed projector / extractor,
4. **Interpretation gate**: classify the result as structural output, approximate surrogate, postdiction target, or speculative interpretation.

This remains mandatory in Phase II.

---

# 3. Phase II mission statement

Phase II exists to turn the executable branch into a platform that can address the following research questions without pretending to settle them prematurely.

## 3.1 Research Gap RG.1 — Branch-independence and canonicity

The executable system uses branch-selected objects:

- `A0`,
- augmented torsion extractor,
- Shiab operator branch,
- observation/extraction projectors,
- analytic regularity branch,
- gauge-fixing realization.

Phase II must implement a framework to test whether important downstream outputs are:

- stable across admissible branch choices,
- unstable but classifiable,
- or completely branch-fragile.

## 3.2 Research Gap RG.2 — Recovery beyond branch-local extraction

Phase I can produce branch-local structural outputs on `X`.
Phase II must support studying whether those outputs are:

- reproducible across branch families,
- projector-invariant,
- geometry-invariant within declared environment classes,
- and strong enough to support a tighter recovery theorem program.

## 3.3 Research Gap RG.3 — PDE classification, well-posedness, and stability

Phase I already gave the minimal linearization package.
Phase II must implement the computational and symbolic infrastructure required to study:

- principal-symbol structure,
- gauge-fixed versus gauge-free behavior,
- conditioning,
- local coercivity or saddle behavior,
- singular directions,
- continuation and bifurcation indicators,
- benchmark classes relevant to eventual well-posedness results.

## 3.4 Research Gap RG.4 — Quantitative comparison with external observations

Phase I provided typed observed outputs and artifact contracts.
Phase II must add:

- a formal prediction/test matrix,
- campaign-level comparison runs,
- uncertainty accounting,
- calibratable but explicit approximation status,
- preservation of negative and inconclusive outcomes.

---

# 4. Core Phase II design principle

Phase II must preserve a strict separation among:

1. **branch-local mathematics**,
2. **comparison among branches**,
3. **numerical evidence**,
4. **observed-sector extraction**,
5. **physical-identification language**,
6. **external empirical comparison**.

No layer may collapse into another.

In particular:

- raw native `Y`-space field coefficients are **not** empirical outputs,
- branch-local stability is **not** branch-independent stability,
- numerical convergence is **not** PDE well-posedness,
- extractor output is **not** physical identification by itself,
- visually similar patterns are **not** validation.

The code must enforce this by type system, artifact schema, and report structure.

---

# 5. Phase II mathematical objects to implement

## 5.1 Branch family formalism

Phase II introduces a **branch family** rather than a single active branch.

Define a branch manifest:

```text
b = (
  A0_b,
  Mu_b,
  T_b,
  Sigma_b,
  PiObs_b,
  Reg_b,
  Gauge_b,
  Pairing_b,
  Extraction_b
)
```

where:

- `A0_b` is the distinguished-connection representative,
- `Mu_b` is the bi-connection constructor,
- `T_b` is the augmented torsion extractor branch,
- `Sigma_b` is the Shiab operator branch,
- `PiObs_b` is the observation/extraction projector family,
- `Reg_b` is the analytic regularity branch,
- `Gauge_b` is the gauge-fixing/slice realization,
- `Pairing_b` fixes adjoint/Hessian conventions,
- `Extraction_b` fixes observed-sector extraction maps.

### Mandatory rule

Every run in Phase II must carry a complete `BranchVariantManifest`.
No default branch settings may remain implicit.

## 5.2 Branch equivalence relation

Phase II must support a declared branch-equivalence specification:

```text
b_i ~ b_j  if and only if  they are considered equivalent under a chosen study relation
```

This relation is not universal.
It depends on the study.

Examples:

- equivalence up to gauge transformation,
- equivalence up to observation projector renaming,
- equivalence up to declared `A0`-family action,
- equivalence only if observed outputs coincide within tolerance on a study suite.

Implementation object:

```text
EquivalenceSpec {
  Id,
  Name,
  ComparedObjectClasses,
  NormalizationProcedure,
  AllowedTransformations,
  Metrics,
  Tolerances,
  InterpretationRule
}
```

## 5.3 Branch-local observed output

For an environment `e` and branch `b`, define:

```text
O_b(e) = Extract_b( Observe_b( Solve_b(e) ) )
```

Expanded form:

```text
z_b(e)                = branch-local solved or benchmark state on Y_h
Q_b(z_b(e))           = declared native quantity / quantities on Y_h
Obs_b(Q_b)            = sigma_h^*(...) or other declared descent / projection chain
Extract_b(Obs_b(Q_b)) = typed observed output on X_h
```

Only `O_b(e)` and its declared derived diagnostics may enter comparison campaigns.

## 5.4 Branch-sensitivity metrics

For two branch variants `b_i`, `b_j` in the same environment `e`, define:

```text
D_obs(b_i, b_j; e)
D_dyn(b_i, b_j; e)
D_stab(b_i, b_j; e)
D_conv(b_i, b_j; e)
```

where:

- `D_obs` compares observed outputs on `X_h`,
- `D_dyn` compares declared branch-invariant dynamical diagnostics,
- `D_stab` compares linearization/Hessian/spectrum diagnostics,
- `D_conv` compares solver/convergence behavior.

### Minimum required ingredients

`D_obs` must be built only from:

- typed observed outputs,
- gauge-invariant or slice-normalized reduced quantities,
- coordinate-normalized comparison domains.

`D_dyn` may include:

- `I2_h`,
- residual norms,
- constraint violation norms,
- gauge-slice violation norms,
- declared conserved/invariant quantities when available.

`D_stab` may include:

- smallest singular values of `L_tilde_h`,
- lowest eigenvalues of `H_h`,
- nullity/near-nullity estimates,
- pseudospectral or condition-number proxies.

`D_conv` may include:

- iteration counts,
- step rejections,
- trust-region radius histories,
- nonlinear residual stagnation classes.

## 5.5 Canonicity docket

For every branch-sensitive object class, Phase II must maintain a canonicity docket.

```text
CanonicityDocket {
  ObjectClass,
  ActiveRepresentative,
  EquivalenceRelation,
  AdmissibleComparisonClass,
  DownstreamClaimsBlockedUntilClosure,
  CurrentEvidence,
  KnownCounterexamples,
  PendingTheorems,
  StudyReports
}
```

Object classes include at minimum:

- distinguished connection `A0`,
- augmented torsion branch,
- Shiab branch,
- observation/extraction projector family,
- analytic regularity branch,
- gauge-fixing branch.

### Critical rule

A docket closes only by:

- actual uniqueness theorem,
- actual classification theorem,
- or explicit invariance evidence strong enough to support a declared claim class.

The software may produce evidence **for** or **against** closure.
It must not mark a docket as closed just because one branch behaves well numerically.

## 5.6 PDE/stability study objects

For every background state `z_*`, implement:

- `R_{z_*}`: infinitesimal gauge map,
- `L_{z_*}`: linearized residual / field operator,
- `L_tilde_{z_*}`: gauge-fixed linearized operator,
- `H_{z_*}`: Hessian-style operator,
- principal-symbol probes,
- continuation metadata.

### Background family

A single background state is not enough for Phase II.
Implement:

```text
z_*(lambda) for lambda in parameter family
```

where `lambda` may be:

- geometry parameter,
- boundary parameter,
- forcing amplitude,
- branch-control parameter,
- discretization/refinement parameter,
- continuation parameter.

This is required for stability and bifurcation studies.

## 5.7 Prediction/test matrix object

Every comparison-facing record must be typed.

```text
PredictionTestRecord {
  TestId,
  ClaimClass,
  FormalSource,
  BranchManifestId,
  ObservableMapId,
  TheoremDependencyStatus,
  NumericalDependencyStatus,
  ApproximationStatus,
  ExternalComparisonAsset,
  Falsifier,
  ArtifactLinks,
  Notes
}
```

### Allowed claim classes

Exactly these classes must be supported:

- `ExactStructuralConsequence`,
- `ApproximateStructuralSurrogate`,
- `PostdictionTarget`,
- `SpeculativeInterpretation`.

No other promoted language is allowed.

---

# 6. Phase II software architecture delta

Phase I already separated:

- C# semantics/orchestration,
- CUDA numerical execution,
- Vulkan diagnostics.

Phase II must preserve that split and extend it.

## 6.1 C# responsibilities in Phase II

C# must own:

- branch family manifests,
- canonicity dockets,
- study specifications,
- prediction/test records,
- theorem dependency status,
- comparison campaign orchestration,
- artifact assembly and replay,
- report generation.

C# must also own the **research semantics** of every run.

## 6.2 CUDA responsibilities in Phase II

CUDA must own heavy numerical kernels for:

- branch-parameterized residual assembly,
- Jacobian-vector and adjoint-vector actions,
- gauge/slice operator application,
- Hessian-style operator application,
- singular value / eigenvalue probes where worthwhile,
- branch sweep batched evaluation,
- continuation step inner solves.

## 6.3 Vulkan responsibilities in Phase II

Vulkan remains diagnostic-only, but Phase II expands its role to include:

- branch-to-branch visual overlays,
- stability spectrum dashboards,
- continuation path visualization,
- observed-vs-native diagnostics,
- canonicity heatmaps,
- comparison campaign summaries.

Nothing visual may be treated as validation unless backed by the typed artifact/report layer.

---

# 7. Recommended repository additions for Phase II

Assume the Phase I repository already exists.
Add the following projects/modules.

```text
/src
  /Gu.Phase2.Semantics
  /Gu.Phase2.Branches
  /Gu.Phase2.Canonicity
  /Gu.Phase2.Stability
  /Gu.Phase2.Continuation
  /Gu.Phase2.Recovery
  /Gu.Phase2.Predictions
  /Gu.Phase2.Comparison
  /Gu.Phase2.Reporting
  /Gu.Phase2.CudaInterop
  /Gu.Phase2.Viz

/native
  /gu_phase2_cuda
    branch_kernels.cu
    jacobian_actions.cu
    hessian_actions.cu
    spectrum_probes.cu
    continuation_kernels.cu

/tests
  /Gu.Phase2.UnitTests
  /Gu.Phase2.IntegrationTests
  /Gu.Phase2.GoldenArtifacts
  /Gu.Phase2.ManufacturedSolutions

/schemas
  branch_family.schema.json
  canonicity_docket.schema.json
  study_spec.schema.json
  prediction_test_record.schema.json
  comparison_campaign.schema.json
  branch_sweep_result.schema.json
  stability_record.schema.json
  continuation_record.schema.json

/reports
  /templates
    canonicity_docket.md
    branch_sweep.md
    stability_atlas.md
    comparison_campaign.md
```

---

# 8. Required Phase II runtime types

## 8.1 Branch family and branch variant types

```text
BranchFamilyManifest
BranchVariantManifest
BranchComparisonClass
EquivalenceSpec
CanonicityDocket
CanonicityEvidenceRecord
```

### BranchVariantManifest fields

Must include at minimum:

- `Id`
- `ParentFamilyId`
- `A0Variant`
- `BiConnectionVariant`
- `TorsionVariant`
- `ShiabVariant`
- `ObservationVariant`
- `ExtractionVariant`
- `GaugeVariant`
- `RegularityVariant`
- `PairingVariant`
- `ExpectedClaimCeiling`
- `Notes`

## 8.2 Stability and deformation types

```text
BackgroundStateRecord
GaugeJacobianRecord
LinearizationRecord
GaugeFixedLinearizationRecord
HessianRecord
SpectrumRecord
ContinuationPathRecord
BifurcationIndicatorRecord
```

### LinearizationRecord fields

Must include:

- background state ID,
- branch manifest ID,
- linearized operator definition ID,
- derivative mode used (`analytic`, `AD`, `finite-difference`, `hybrid`),
- domain/codomain signatures,
- gauge handling mode,
- assembly mode (`explicit sparse`, `matrix-free`, `block hybrid`),
- validation status.

## 8.3 Recovery and observation types

```text
RecoveryPipelineRecord
ObservedOutputClass
ObservedOutputRecord
ExtractionProjectorRecord
PhysicalIdentificationRecord
```

### PhysicalIdentificationRecord fields

Must include the six gate fields:

1. formal source object,
2. observation/extraction map,
3. support status,
4. approximation status,
5. comparison target,
6. falsifier.

## 8.4 Prediction/comparison types

```text
PredictionTestRecord
ComparisonAsset
ComparisonCampaignSpec
ComparisonRunRecord
CalibrationRecord
UncertaintyRecord
```

## 8.5 Study specification types

```text
BranchSweepSpec
StabilityStudySpec
RecoveryStudySpec
ComparisonCampaignSpec
ResearchBatchSpec
```

A `ResearchBatchSpec` may compose multiple studies but must preserve per-study branch and artifact boundaries.

---

# 9. Phase II mathematics implementation guidance

This section is the most important part of the handoff.
It tells Claude what mathematics to implement, not just what classes to create.

## 9.1 Multi-branch residual formulation

For each branch variant `b`, implement the branch-local field operator:

```text
E_b(z) = Upsilon_b(z)
```

or, for variational/gauge-fixed execution mode,

```text
E_b(z) = G_b(z)
```

where `G_b` is the gauge-fixed Euler–Lagrange operator or the adjoint-residual stationarity operator.

At minimum:

```text
Upsilon_b = S_b - T_b
S_b      = Sigma_b(F_{A_omega})
T_b      = Torsion_b(A_omega, B_omega, ...)
```

If the branch uses the minimal compatible Shiab family,

```text
Sigma_b(Xi)
  = Pi_T_b( K_A0_b( d_{B_omega} Xi ) )
    + L_A0_b( T_b, Xi )
```

### Implementation rule

Do not treat `Sigma_mc` as globally unique.
Every operator call site must accept a `ShiabVariant`.

## 9.2 Gauge-fixed stationarity formulation

For a discrete state vector `u`, define:

```text
R(u)   = Upsilon_h(u)
J(u)   = dR/du
C(u)   = gauge/slice constraint operator
```

A practical Phase II solve operator may use one of these equivalent study forms:

### Form A — adjoint-residual stationarity

```text
G(u) = J(u)^T M_R R(u)
```

### Form B — gauge-augmented stationarity

```text
G(u) = J(u)^T M_R R(u) + lambda_g C(u)^T M_C C(u)
```

### Form C — explicit slice solve

```text
Solve:
  R(u) = 0
  C(u) = 0
```

### Rule

Every run must declare which study form was used.
No mixing without an explicit solver mode record.

## 9.3 Linearization package to implement exactly

At a declared background `u_*`, implement:

```text
R_lin(delta u)       = J_* delta u
C_lin(delta u)       = C_* delta u
L_tilde(delta u)     = ( J_* delta u, C_* delta u )
H(delta u)           = L_tilde^* L_tilde (delta u)
```

This package is not optional in Phase II.
It is the foundation for:

- branch-fragility analysis,
- stability diagnostics,
- continuation,
- principal-symbol estimation,
- preconditioner design,
- near-nullspace detection,
- theorem-liaison reports.

## 9.4 Principal symbol sampler

The manuscript’s open PDE questions require an implementable symbol probe even before full theorem closure.
Implement a **principal-symbol sampler**.

For local covector `xi`, compute or approximate:

```text
sigma_principal( J_* )(x, xi)
sigma_principal( L_tilde )(x, xi)
sigma_principal( H )(x, xi)
```

### Recommended implementation strategy

1. derive analytic principal-symbol blocks for terms where possible,
2. where analytic derivation is not yet implemented, use a symbolic/local linearization layer or automatic differentiation over frozen-coefficient local stencils,
3. store the resulting symbol in a typed `PrincipalSymbolRecord`.

### Required outputs

For each sampled `(x, xi)`:

- symbol matrix / matrix-free action,
- symmetry/hermiticity flags,
- definiteness indicators when meaningful,
- rank deficiency estimate,
- gauge-null direction estimate,
- branch metadata.

This is essential for PDE classification studies.

## 9.5 Stability semantics to enforce

For each background state, interpret `H` using the following minimum semantics:

- **strictly positive on slice** → locally coercive branch behavior,
- **small positive modes** → soft / weakly constrained directions,
- **near-zero kernel** → moduli candidate or unresolved gauge degeneracy,
- **negative modes** → saddle behavior,
- **wildly branch-dependent low spectrum** → canonicity risk.

These are numerical/study semantics only.
They do not by themselves prove a theorem.

## 9.6 Branch comparison mathematics

Given a study environment `e` and a branch family `B = {b_1, ..., b_n}`:

1. solve or construct `z_{b_i}(e)` for each branch,
2. compute observed outputs `O_{b_i}(e)`,
3. compute dynamical diagnostics `DYN_{b_i}(e)`,
4. compute stability diagnostics `STAB_{b_i}(e)`,
5. compare all pairs using a declared comparison relation.

### Minimum comparison summary

For each study, produce:

```text
PairwiseObservedDistanceMatrix
PairwiseDynamicDistanceMatrix
PairwiseStabilityDistanceMatrix
QualitativeClassificationAgreementMatrix
FailureModeMatrix
```

### Qualitative classifications to compare

At minimum:

- converged / failed / stalled,
- stable / soft / singular / saddle,
- extraction-succeeded / extraction-failed,
- comparison-admissible / comparison-inadmissible.

### Branch-independence evidence rule

A study may claim **branch-robust numerical evidence** only when:

- observed outputs remain within declared tolerance over an admissible comparison class,
- qualitative classifications agree,
- no hidden reparameterization was needed beyond the declared equivalence spec,
- negative outliers were preserved rather than filtered away.

That is still not a theorem.
It is only numerical evidence relevant to a canonicity docket.

## 9.7 Observed-sector recovery implementation rule

The recovery pipeline must remain explicit:

```text
NativeSource -> ObservationMap -> ExtractionProjector -> ObservedOutput -> InterpretationGate
```

### Native source examples

Allowed native source classes include:

- residual-derived tensors,
- curvature-derived tensors,
- torsion-derived tensors,
- spectrum-derived invariants,
- linear-response quantities,
- branch-local structural surrogates.

### Forbidden shortcut

Do not let comparison campaigns consume:

- raw connection coefficients,
- raw internal-basis amplitudes,
- raw gauge-dependent fields,
- untyped visualization exports.

## 9.8 Physical-identification gate

Every attempt to say “this looks like gravity,” “this looks gauge-like,” or “this corresponds to a family/charge/mass-like quantity” must go through a typed gate.

For each identification candidate, require:

- formal source,
- observation/extraction map,
- support status,
- approximation status,
- comparison target,
- falsifier.

### Implementation rule

If any of these are missing, the system must automatically demote the claim class or mark it `inadmissible`.

## 9.9 Prediction/test matrix discipline

Every prediction-facing output must become a `PredictionTestRecord`.

Reading rules to enforce programmatically:

1. open theorem dependency → not theorem-level support,
2. missing external asset → internal result only,
3. missing falsifier → invalid prediction record,
4. incomplete branch manifest → invalid reproducible evidence,
5. exploratory-only numerical status → not comparison-ready.

---

# 10. Required numerical algorithms for Phase II

## 10.1 Branch sweep runner

Implement a `BranchSweepRunner` that:

1. loads one `StudyEnvironmentSpec`,
2. loads a branch family or branch subset,
3. normalizes shared geometry/discretization assumptions,
4. executes all branches using declared solver modes,
5. extracts observed outputs,
6. computes branch comparison matrices,
7. emits a complete branch-sweep artifact package.

### Pseudocode

```text
for each branch b in BranchSet:
    state0      = InitializeState(environment, b)
    solveResult = SolveBranch(environment, b, state0)
    nativeOut   = ComputeNativeOutputs(solveResult, b)
    obsOut      = RecoverObservedOutputs(nativeOut, environment, b)
    stabOut     = ComputeStabilityDiagnostics(solveResult, environment, b)
    store RunRecord(b, solveResult, nativeOut, obsOut, stabOut)

compare all stored RunRecords under EquivalenceSpec
emit BranchSweepResult + CanonicityEvidenceRecord
```

## 10.2 Continuation / branch path tracking

Implement pseudo-arclength continuation as the default Phase II continuation method.

For a parameter `lambda`, seek a solution family:

```text
G(u, lambda) = 0
```

with predictor-corrector steps:

1. tangent predictor from previous converged states,
2. augmented corrector solve,
3. spectrum and conditioning probe,
4. branch event detection,
5. artifact emission.

### Required event detectors

- smallest singular value collapse,
- Hessian sign change,
- step rejection bursts,
- qualitative extractor failure,
- branch merge/split candidate,
- gauge-slice breakdown.

### Why this matters

Continuation is necessary for:

- stability studies,
- bifurcation hints,
- branch-fragility maps,
- environment parameter sweeps,
- calibration studies.

## 10.3 Spectral diagnostics

Implement at least three spectrum probes:

1. **smallest singular values of `L_tilde`**,
2. **smallest eigenvalues of `H`**,
3. **dominant unstable/soft mode vectors on the slice**.

### Recommended methods

- Lanczos / LOBPCG for symmetric `H`,
- implicitly restarted Arnoldi or randomized SVD for `L_tilde`,
- shift-invert when conditioning permits,
- matrix-free operator actions where global assembly is too large.

### Mandatory outputs

- mode count requested / obtained,
- convergence status,
- normalization convention,
- slice or gauge handling mode,
- basis ordering metadata,
- whether modes are native-space or observed-space projected.

## 10.4 Manufactured-solution and benchmark families

Phase II must add benchmark families specifically for the open research lanes.

### Benchmark class A — branch parity benchmark

Use a controlled geometry/problem where multiple admissible branch variants should behave similarly enough to reveal whether branch sensitivity is small or large.

### Benchmark class B — linearization benchmark

Use a manufactured background with known perturbation response or at least a controlled synthetic tangent model.

### Benchmark class C — gauge/slice benchmark

Construct cases with known gauge redundancy so the software can verify that:

- raw Jacobian has expected null directions,
- gauge-fixed operator suppresses them,
- observed outputs remain stable.

### Benchmark class D — extraction benchmark

Construct cases where the extraction projector should recover a known structural surrogate on `X_h`.

### Benchmark class E — comparison dry-run benchmark

Use internal or synthetic data assets to verify the prediction/test matrix and campaign plumbing before real external datasets are used.

## 10.5 Calibration and uncertainty

Phase II needs a basic uncertainty engine.

Required uncertainty classes:

- discretization uncertainty,
- solver tolerance uncertainty,
- branch-choice uncertainty,
- extraction/projector uncertainty,
- comparison-asset preprocessing uncertainty,
- calibration parameter uncertainty.

### Implementation rule

Every quantitative comparison output must include a decomposition of uncertainty sources.

At minimum:

```text
TotalUncertainty = {
  Discretization,
  Solver,
  Branch,
  Extraction,
  Calibration,
  DataAsset
}
```

If a component is unknown, store it as `Unestimated` rather than hiding it.

---

# 11. Required CUDA work for Phase II

## 11.1 Phase II CUDA principle

CUDA is not just for faster residuals anymore.
It must support the research workflows directly.

Priority order:

1. batched multi-branch residual evaluation,
2. Jacobian-vector and adjoint-vector actions,
3. Hessian-style operator actions,
4. spectral probes,
5. continuation inner solves,
6. batched observation/extraction kernels where justified.

## 11.2 New kernel families

### Kernel family 1 — branch-parameterized residual assembly

A single kernel family should accept branch parameter blocks rather than requiring one kernel per branch.

Inputs:

- packed geometry,
- packed field coefficients,
- branch parameter struct,
- operator metadata,
- quadrature metadata.

Outputs:

- residual field,
- residual norm contributions,
- optional intermediate diagnostics.

### Kernel family 2 — Jv and J^T v actions

Implement matrix-free actions:

```text
y = J(u) v
x = J(u)^T w
```

These are mandatory for:

- Newton-Krylov,
- adjoint-residual solves,
- spectral probes,
- continuation correctors.

### Kernel family 3 — gauge/slice operator application

Implement:

```text
c = C(u)
q = C_* v
r = C_*^T y
```

These are required for gauge-aware stability diagnostics.

### Kernel family 4 — Hessian-style operator actions

Implement matrix-free:

```text
y = H v = L_tilde^* L_tilde v
```

This can be done without assembling dense matrices.

### Kernel family 5 — batched branch sweeps

For small-to-medium branch families, add a batched mode where different branch variants are evaluated in the same launch family using a branch-indexed parameter array.

This is especially useful when geometry/discretization are shared.

## 11.3 Library recommendations

Use the CUDA ecosystem pragmatically:

- `cuSPARSE` for sparse linear algebra,
- `cuSOLVER` for factorization/eigensolver support when applicable,
- custom kernels for element/local operator assembly and matrix-free actions,
- keep branch manifests and provenance in C# and pass only packed numerical data across the interop boundary.

## 11.4 CUDA parity rules

Every Phase II CUDA feature must first match the CPU reference on:

- residual values,
- Jv actions,
- J^T v actions,
- gauge/slice operator values,
- Hessian actions,
- smallest spectral diagnostics in benchmark cases,
- extracted observed outputs when extraction runs on GPU.

No Phase II research report may consume a GPU-only path that lacks parity certification.

---

# 12. Phase II observation and recovery engine

## 12.1 Recovery pipeline must become a first-class graph

Represent the observed-sector recovery process as a graph/DAG:

```text
RecoveryGraph:
  NativeNode -> ObservationNode -> ExtractionNode -> InterpretationNode
```

This should not be hardcoded into ad hoc postprocessing.

### Required node metadata

Every node must carry:

- source object type,
- tensor/signature metadata,
- branch provenance,
- gauge dependence flag,
- dimensionality metadata,
- numerical dependency status,
- theorem dependency status.

## 12.2 Observed output classes

At minimum implement these observed output classes:

- `ObservedTensorField`,
- `ObservedScalarInvariant`,
- `ObservedModeSpectrum`,
- `ObservedResponseCurve`,
- `ObservedStructuralPattern`,
- `ObservedComparisonQuantity`.

### Why this matters

Not all observed outputs are pointwise fields.
Some are:

- mode spectra,
- invariants,
- continuation curves,
- fit parameters,
- integrated structural diagnostics.

The type system must support all of them.

## 12.3 Interpretation layer

The final node of recovery is not “physics.”
It is a typed interpretation layer that may be one of:

- structural only,
- approximate structural surrogate,
- postdiction target,
- speculative interpretation.

No other categories should be created informally.

---

# 13. Comparison campaign framework

## 13.1 Campaign-level rather than run-level comparisons

Phase II should introduce `ComparisonCampaignSpec`.
A campaign may reference:

- one or more environments,
- one or more branch subsets,
- one or more observed output classes,
- one or more external comparison assets,
- one or more calibration strategies.

This is needed because meaningful comparison usually spans multiple runs.

## 13.2 Required comparison asset metadata

Every external asset must carry:

- asset ID,
- source citation / origin metadata,
- acquisition date,
- preprocessing description,
- admissible use statement,
- regime / domain of validity,
- uncertainty model,
- comparison variable definitions.

## 13.3 Allowed comparison modes

Implement three explicit modes:

1. `StructuralComparison`
2. `SemiQuantitativeComparison`
3. `QuantitativeComparison`

### Structural comparison

Used when output matches only pattern / topology / qualitative structure.

### Semi-quantitative comparison

Used when scaling or surrogate matching exists but exact physical identification remains approximate.

### Quantitative comparison

Used only when:

- branch manifest is complete,
- extraction is typed,
- approximation status is declared,
- numerical status is strong enough,
- uncertainty decomposition exists,
- falsifier is present.

## 13.4 Calibration policy

Calibration is allowed only if it is explicit.

Each campaign must declare whether parameters are:

- fixed a priori,
- fitted on calibration subset,
- estimated by inverse solve,
- or left free for sensitivity analysis only.

No hidden tuning is allowed.

## 13.5 Negative-result preservation

A failed comparison must still produce a first-class artifact:

```text
ComparisonFailureRecord
```

with:

- reason for failure,
- whether the failure is numerical, branch-local, extraction-level, or empirical,
- whether it falsifies a record, blocks a campaign, or merely demotes a claim class.

---

# 14. Reporting and artifact requirements for Phase II

## 14.1 New required artifact packages

In addition to Phase I artifacts, Phase II must emit:

- `BranchSweepArtifact`
- `CanonicityDocketArtifact`
- `StabilityAtlasArtifact`
- `ContinuationArtifact`
- `RecoveryGraphArtifact`
- `PredictionMatrixArtifact`
- `ComparisonCampaignArtifact`

## 14.2 Branch sweep artifact contents

Must include:

- environment spec,
- branch manifests,
- shared discretization spec,
- per-branch solve traces,
- per-branch observed outputs,
- pairwise distance matrices,
- qualitative agreement matrices,
- failure mode summary,
- canonicity evidence summary.

## 14.3 Stability atlas contents

Must include:

- background family definition,
- linearization records,
- Hessian records,
- spectrum probes,
- continuation path data,
- singularity/bifurcation indicators,
- discretization sensitivity notes,
- theorem-status notes.

## 14.4 Report generation rule

Every research report must state explicitly:

- what is branch-local,
- what is comparison-ready,
- what remains open,
- what is numerical only,
- what is physically uninterpreted,
- what is ruled out by the study.

The reporting layer must make overclaiming difficult.

---

# 15. Recommended milestone sequence for Phase II

Continue the milestone numbering from Phase I.

## Milestone 13 — Branch family framework

### Build

- `BranchFamilyManifest`
- `BranchVariantManifest`
- `EquivalenceSpec`
- `CanonicityDocket`
- branch-parameterized operator dispatch

### Completion criteria

- more than one admissible branch variant can be executed on the same environment,
- all branch choices serialize into artifacts,
- no solver path depends on hidden global defaults.

## Milestone 14 — Multi-branch residual and extraction execution

### Build

- branch-parameterized `TorsionVariant` dispatch,
- branch-parameterized `ShiabVariant` dispatch,
- branch-parameterized observation/extraction dispatch,
- common branch sweep runner.

### Completion criteria

- one environment can be run across a branch family,
- observed outputs are produced for each branch,
- per-branch artifacts are replayable.

## Milestone 15 — Canonicity evidence engine

### Build

- pairwise comparison matrices,
- canonicity evidence reports,
- branch-fragility classification rules,
- docket updater.

### Completion criteria

- at least one full branch sweep emits a canonicity docket update,
- outlier branches are preserved, not silently dropped.

## Milestone 16 — Full linearization / Hessian workbench

### Build

- `Jv`, `J^T v`, `L_tilde`, `H` actions,
- CPU reference and CUDA parity,
- linearization validation suite,
- spectrum probe interface.

### Completion criteria

- background-state diagnostics are reproducible,
- smallest singular values / eigenvalues are available on benchmark problems,
- slice/gauge handling is explicit in every record.

## Milestone 17 — Principal-symbol and PDE study layer

### Build

- principal-symbol sampler,
- symbol report objects,
- gauge-fixed versus gauge-free study mode,
- PDE classification dashboards.

### Completion criteria

- symbol probes run on declared benchmark families,
- outputs distinguish elliptic-like, hyperbolic-like, mixed, degenerate, or unresolved local behavior in a typed way.

## Milestone 18 — Continuation and stability atlas

### Build

- pseudo-arclength continuation,
- event detection,
- stability atlas generation,
- bifurcation candidate tagging.

### Completion criteria

- at least one parameter family can be tracked through multiple continuation steps,
- stability diagnostics evolve along the path,
- failure/singularity events are recorded.

## Milestone 19 — Recovery graph and physical-identification gate

### Build

- recovery DAG,
- interpretation node types,
- physical-identification gate enforcement,
- demotion rules.

### Completion criteria

- extraction outputs cannot be mislabeled as physics without complete gate fields,
- invalid identifications are automatically demoted or rejected.

## Milestone 20 — Prediction/test matrix and campaign runner

### Build

- `PredictionTestRecord`,
- `ComparisonCampaignSpec`,
- calibration and uncertainty records,
- campaign result summaries.

### Completion criteria

- at least one dry-run campaign using synthetic or internal benchmark assets succeeds,
- every comparison record has an explicit falsifier and claim class.

## Milestone 21 — External comparison expansion

### Build

- real dataset adapter interfaces,
- uncertainty decomposition pipeline,
- structural / semi-quantitative / quantitative comparison modes,
- negative-result artifact support.

### Completion criteria

- at least one real campaign can run end-to-end,
- reports remain branch-traceable and uncertainty-aware.

## Milestone 22 — Research-batch automation and reporting

### Build

- batch orchestration across environments and branch sets,
- final report templates,
- dashboard exports,
- docket aggregation.

### Completion criteria

- Phase II can run reproducible research batches,
- outputs support a real next-session handoff without the original manuscript.

---

# 16. Definition of done for Phase II

Phase II is complete only if all of the following are true.

1. **Multi-branch support exists** and hidden branch defaults are eliminated.
2. **Canonicity dockets exist** for all branch-sensitive object classes.
3. **Branch sweeps are reproducible** and emit pairwise comparison matrices.
4. **Linearization/Hessian workbench exists** with CPU/CUDA parity.
5. **Principal-symbol studies exist** in typed artifact form.
6. **Continuation/stability atlas exists** for at least one nontrivial parameter family.
7. **Recovery is graph-typed** and cannot bypass observation/extraction gates.
8. **Physical-identification gate is enforced programmatically**.
9. **Prediction/test matrix exists** and is used by all comparison-facing outputs.
10. **At least one external comparison campaign runs end-to-end** with uncertainty decomposition and falsifier handling.
11. **Negative and inconclusive results are preserved** as first-class artifacts.
12. **Reports explicitly distinguish** branch-local evidence, theorem-level status, and empirical status.

If any of these are missing, Phase II is not complete.

---

# 17. Explicit “do not improvise” rules for Claude

1. Do **not** silently hardcode one Shiab operator as canonical.
2. Do **not** collapse `Y` into `X`.
3. Do **not** compare raw native-space field coefficients directly to external data.
4. Do **not** treat numerical convergence as proof of well-posedness.
5. Do **not** erase branch metadata during GPU execution.
6. Do **not** hide failed runs, extractor failures, or unstable branches.
7. Do **not** promote structural outputs to physical claims without the physical-identification gate.
8. Do **not** create comparison campaigns without explicit falsifiers.
9. Do **not** allow CUDA-only features to bypass CPU parity checks.
10. Do **not** add a fermionic coupled solver into the bosonic core unless it is declared as a separate branch family with its own manifests and proof-status discipline.

---

# 18. Remaining items intentionally NOT included in Phase II

This section is the handoff for Phase III or later.
These are intentionally beyond Phase II even if Phase II should prepare for them.

## 18.1 Full fermionic sector and coupled boson–fermion execution

Not included:

- complete topological/chimeric spinor realization in executable form,
- full topological-to-metric spinor transition theorem implementation,
- full fermionic square-root / Dirac-type operator closure,
- fully coupled boson–fermion residuals and deformation blocks,
- fermionic observation/extraction closure.

Phase II may add semantic scaffolding only if it does not destabilize the bosonic research engine.

## 18.2 Branch-independent recovery theorems

Not included:

- actual proof that extracted observed sectors are branch-independent,
- actual theorem-level recovery of gravitational/gauge/fermionic/scalar/family structures,
- final closure of canonicity dockets.

Phase II may only produce evidence, counterexamples, or narrowing studies.

## 18.3 Full PDE theorem closure

Not included:

- local existence/uniqueness theorem,
- global well-posedness theorem,
- nonlinear stability theorem,
- optimal gauge theorem,
- full boundary-value classification theorem.

Phase II produces symbol/stability evidence and benchmark infrastructure, not the proofs themselves.

## 18.4 Final physical decomposition and prediction program

Not included:

- final Standard Model decomposition claims,
- definitive mass/coupling predictions,
- final family/charge assignments,
- broad physical validation claims.

Phase II only supports typed, claim-classed comparison records.

## 18.5 Quantum / UV / anomaly / renormalization closure

Not included:

- quantization strategy,
- anomaly cancellation analysis,
- renormalization structure,
- UV completion claims.

## 18.6 Full large-scale HPC optimization

Not included as required completion criteria:

- multi-GPU domain decomposition,
- distributed memory runs,
- out-of-core giant state support,
- mixed-precision research tuning,
- final production UX polish.

Those are valuable but secondary to correctness and auditability.

---

# 19. Recommended next pickup order after Phase II

Once this plan is implemented, the strongest next moves are:

1. **fermionic semantic onboarding as a separate branch family**,
2. **coupled boson–fermion deformation package**,
3. **theorem liaison work based on Phase II stability and canonicity evidence**,
4. **narrow-scope quantitative campaigns against carefully chosen external assets**,
5. **selective scale-up and performance optimization only after the research semantics are stable**.

---

# 20. Final implementation instruction

Build Phase II as a **research evidence engine**.

It must make it possible to say, honestly and reproducibly:

- which conclusions are branch-local,
- which are branch-robust numerically,
- which are still open mathematically,
- which extractor chains succeed or fail,
- which branches are unstable or fragile,
- which empirical comparisons are admissible,
- and which claims were actually weakened by the evidence.

That honesty is not overhead.
It is the core purpose of this phase.

---

# Appendix A — If Phase I is missing or incomplete

If the existing codebase does **not** already contain the full Phase I contracts, then Claude must implement the following minimum prerequisites before attempting Phase II:

1. typed `X_h`, `Y_h`, `pi_h`, `sigma_h` geometry,
2. typed branch manifest and artifact package,
3. corrected torsion branch interface,
4. Shiab branch interface,
5. residual `Upsilon_h`,
6. CPU reference backend,
7. CUDA parity-tested residual path,
8. observation/extraction pipeline,
9. replayable environment spec,
10. linearization package (`J_h`, `L_tilde_h`, `H_h`) at least in CPU reference form.

If these do not exist, do not fake Phase II.
Backfill the missing Phase I contracts first.

---

# Appendix B — Suggested first concrete Phase II study suite

To keep implementation honest, begin with this exact study order.

## Study S1 — A0 branch sweep

Hold torsion/Shiab/extraction fixed.
Vary the distinguished connection representative within the admissible family.
Measure:

- residual stability,
- observed-output drift,
- Hessian low-spectrum drift,
- convergence class changes.

## Study S2 — Torsion branch sweep

Hold `A0` and extraction fixed.
Compare multiple admissible torsion extractors.
Measure:

- `Upsilon` sensitivity,
- observed-output drift,
- failure modes.

## Study S3 — Shiab branch sweep

Hold `A0` and torsion fixed.
Compare `Sigma_mc` against other admissible operator branches or parameterized variants.
Measure:

- symbol behavior,
- stability spectrum changes,
- observed-output robustness.

## Study S4 — Gauge/slice study

Use one fixed branch.
Compare slice/constraint realizations.
Measure:

- raw Jacobian nullity,
- gauge-fixed singular values,
- continuation robustness.

## Study S5 — Extraction projector study

Hold native branch fixed.
Compare admissible extraction projectors.
Measure:

- observed-output robustness,
- claim-class changes,
- physical-identification gate outcomes.

## Study S6 — External comparison dry run

Use synthetic or internally generated comparison assets first.
Verify:

- campaign plumbing,
- uncertainty decomposition,
- falsifier handling,
- negative-result preservation.

Only after S1–S6 should Claude expand to broader research campaigns.

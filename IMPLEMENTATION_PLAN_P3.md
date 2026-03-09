# IMPLEMENTATION_PLAN_P3.md

# Geometric Unity Phase III Boson Spectrum and Property Extraction Implementation Plan

## Purpose

This document is a **standalone Phase III implementation handoff** for Claude Code.

It assumes **no access to the original Geometric Unity completion manuscript**.
It also assumes that:

- **Phase I is complete**: minimal executable bosonic branch, typed artifacts, CPU reference backend, CUDA backend, observation pipeline, validation and replay.
- **Phase II is complete**: branch families, canonicity studies, linearization/stability workbench, continuation, recovery graph, identification gate, comparison campaigns, and reporting.

Phase III is the first layer intended to move from “a research engine for branch and stability studies” to “a system that can extract, organize, and compare candidate bosonic sectors and their properties in a disciplined way.”

This is **not** the same as proving the final boson dictionary of Geometric Unity.
The Phase III system must be able to:

1. find admissible background states,
2. linearize the bosonic theory around those backgrounds,
3. reduce away gauge directions and other unphysical directions,
4. solve for normal modes / spectra,
5. extract mode properties,
6. track modes across branch variants, parameter continuation, and refinement,
7. push candidate modes through the observation pipeline,
8. build an evidence-ranked candidate boson registry,
9. compare candidate signatures against external targets,
10. preserve negative results and ambiguity instead of over-claiming.

The central discipline for this phase is:

> Phase III is allowed to produce candidate boson sectors, candidate properties, stability statements, branch-stability statements, and evidence-ranked physical identifications.
> Phase III is **not allowed** to silently promote those into unique physical truth, a complete Standard Model derivation, or a proof that all bosons are settled.

---

# 1. What Phase III is and is not

## 1.1 What Phase III is

Phase III is the implementation of the **boson spectrum and property extraction layer** on top of the Phase I + Phase II platform.

It must provide:

1. a **background atlas builder** for stationary or low-residual bosonic states,
2. a **gauge-aware linearization framework** around each background,
3. a **spectral solver stack** for normal modes, generalized eigenmodes, and stability modes,
4. a **mode family tracker** across branch variants, continuation paths, and discretization levels,
5. a **property extraction engine** for mass-like scales, multiplicities, polarizations, symmetry content, and interaction proxies,
6. an **observed-signature builder** that maps native mode data into X-space comparison-ready outputs,
7. a **candidate boson registry** with evidence ranking and ambiguity tracking,
8. a **boson comparison campaign engine** that evaluates candidate sectors against external targets,
9. a replayable artifact contract for all of the above.

## 1.2 What Phase III is not

Phase III is **not**:

- a proof that Geometric Unity is correct,
- a proof that all bosons in nature are uniquely derived,
- a proof of branch-independence in full generality,
- a complete fermionic sector,
- a quantum field theoretic completion,
- a renormalization program,
- a scattering amplitude engine,
- a final Standard Model match,
- a proof of exact observed masses or couplings,
- a proof that every extracted mode corresponds to a physical particle.

Phase III can provide **candidate boson sectors** and **evidence-ranked physical identifications**.
It cannot collapse all ambiguity unless the computations themselves actually demonstrate canonicity.

---

# 2. Minimal recap of the formal core Claude must assume

This section restates the minimum mathematical framework that Phase III extends.

## 2.1 Spaces

The executable framework uses two spaces:

- `X`: observed/base manifold,
- `Y`: native/observerse total space where the main dynamical fields live.

Discrete forms:

- `X_h`,
- `Y_h`.

Phase III must never collapse `Y` into `X` by convenience.
All mode computation happens natively on `Y_h` and all physical comparison must pass through the declared observation chain to `X_h`.

## 2.2 Projection and observation

There are two distinct maps:

- `pi : Y -> X`, structural projection,
- `sigma`, observation section / observation map.

Discrete forms:

- `pi_h`,
- `sigma_h`,
- `sigma_h^*` for pullback / descent / extraction.

The observation chain is not optional.
No candidate boson may be compared to external targets using raw Y-space tensors alone.

## 2.3 Bosonic state

The active implementation supports a bosonic state that may be represented either as:

1. a single primary connection-like field `omega`, or
2. a multi-block bosonic state `z = (A, omega_aux, ...)`.

Phase III must remain layout-agnostic.
All linearization, spectral, and property extraction code must operate on a generalized state layout.

Required abstraction:

```text
BranchFieldLayout = {
  StateBlocks: [ ... ],
  ConnectionBlocks: [ ... ],
  AuxiliaryBosonicBlocks: [ ... ],
  GaugeActionRules: [ ... ],
  ConstraintRules: [ ... ],
  ObservationEligibility: [ ... ]
}
```

## 2.4 Distinguished connection and branch-defined operators

The active bosonic branch uses a branch-defined map from the state to a distinguished pair of effective connections:

```text
(A_state, B_state) = Mu_A0(state, branchParameters, geometryContext)
```

This is not assumed canonical.
It remains branch-dependent.

## 2.5 Curvature, torsion, Shiab, residual

Native bosonic residual construction:

- curvature `F_A`,
- corrected/augmented torsion `T_aug`,
- Shiab-family term `S`,
- residual `Upsilon = S - T_aug`.

Abstract interfaces:

```text
T_aug = TorsionBranch(A_state, B_state, geometryContext, branchParameters)
S     = ShiabBranch(F_A, T_aug, A_state, B_state, geometryContext, branchParameters)
Upsilon = S - T_aug
```

## 2.6 Objective and first variation

The core discrete objective is:

```text
I2_h(state) = (1/2) Upsilon_h(state)^T M_Upsilon Upsilon_h(state)
```

with Jacobian:

```text
J_h = dUpsilon_h / dstate_h
```

and stationarity operator:

```text
G_h(state) = J_h(state)^T M_Upsilon Upsilon_h(state)
```

A background state is ideally a solution of:

```text
G_h(state_*) = 0
```

and preferably also has small residual norm:

```text
||Upsilon_h(state_*)|| small
```

The platform must support exact stationary backgrounds, approximate stationary backgrounds, and low-residual diagnostic backgrounds.

## 2.7 Phase II assets that Phase III depends on

Phase III must assume the following already exist:

- branch family manifest system,
- variant resolution and branch sweep runner,
- canonicity analyzer,
- linearization workbench,
- PDE classifier,
- continuation runner,
- stability atlas builder,
- recovery graph,
- identification gate,
- prediction validator,
- comparison campaign runner,
- reporting layer.

Phase III does not replace these.
It adds a **boson-specific layer** on top of them.

---

# 3. Operational definition of a candidate boson in this software

Claude needs a precise implementation target.
The software cannot wait for a final philosophical definition of “what a boson is.”
It must define an operational one.

## 3.1 Candidate boson: working definition

A **candidate boson** is an evidence-bearing equivalence class of bosonic linearized modes around one or more admissible backgrounds, equipped with:

1. a native-space mode representation,
2. a gauge-reduced physical representative,
3. one or more spectral invariants,
4. one or more symmetry / polarization descriptors,
5. one or more observed-space signatures,
6. branch/refinement/backend stability records,
7. a claim class indicating how strong the identification is.

This is intentionally weaker than “physical particle.”

## 3.2 Candidate boson record must be background-relative

No mode is absolute in the implementation.
Every mode comes from a declared background:

```text
background_* = {
  environment,
  branch manifest,
  geometry,
  gauge choice,
  continuation coordinates,
  solver tolerances,
  achieved residual/stationarity metrics
}
```

Therefore all candidate boson records must carry background provenance.

## 3.3 Candidate boson equivalence class

Two mode records may be grouped into the same candidate boson family only if there is evidence of persistence under one or more of:

- branch variation,
- refinement,
- continuation along a parameter path,
- backend change,
- observed-signature comparison.

Equivalence is not string matching.
It must be built from explicit matching metrics.

---

# 4. Mathematics Claude must implement for Phase III

This section contains the core mathematical target.

## 4.1 Background states

A background state `z_*` is a declared state satisfying one of three levels:

### Level B0 — diagnostic background

```text
||Upsilon_h(z_*)|| <= tol_residual_diagnostic
```

This is weak and only sufficient for exploratory spectral work.

### Level B1 — stationary background

```text
||G_h(z_*)|| <= tol_stationary
```

This is the default target for Phase III.

### Level B2 — stationary + low residual background

```text
||G_h(z_*)|| <= tol_stationary
and
||Upsilon_h(z_*)|| <= tol_residual_strict
```

This is the strongest target and should be preferred when possible.

Backgrounds may arise from:

- direct solves,
- continuation from known backgrounds,
- branch sweeps,
- symmetry-ansatz initialization,
- refinement transfer from coarser grids.

## 4.2 Linearization of the residual

For a perturbation `delta z`, define the first variation:

```text
delta Upsilon = J_* delta z
```

where:

```text
J_* = J_h(z_*) = dUpsilon_h / dz evaluated at z_*
```

This is the first mandatory linearization product.
All later spectral construction depends on it.

## 4.3 Linearization of the stationarity equation

The first variation of the stationarity operator is:

```text
delta G = H_* delta z
```

where the full Hessian-like operator is:

```text
H_* = d/dz [ J^T M_Upsilon Upsilon ] at z_*
```

Expanding formally:

```text
H_*[delta z]
  = (dJ^T/dz [delta z]) M_Upsilon Upsilon_*
    + J_*^T M_Upsilon (J_* delta z)
```

If `Upsilon_*` is sufficiently small, the **Gauss-Newton approximation** is allowed:

```text
H_GN = J_*^T M_Upsilon J_*
```

### Mandatory rule

Implement both:

1. **Gauss-Newton spectral operator** `H_GN`,
2. **full Hessian correction path** `H_full = H_GN + H_residualCorrection`.

The software must never silently replace one with the other.
Every spectrum must declare which operator produced it.

## 4.4 Gauge directions and physical mode space

Not every perturbation `delta z` is physical.
Some are infinitesimal gauge directions.

Implement a gauge action differential:

```text
GaugeInfinitesimal : eta -> delta z_gauge = Gamma_* eta
```

where `eta` is an infinitesimal gauge parameter and `Gamma_*` is the linearized gauge action at the background.

The tangent space decomposes into:

```text
T_{z_*} State_h = Im(Gamma_*) ⊕ physical_complement ⊕ possible_constraint_defect
```

Phase III must support three physical-mode formulations:

### Formulation P1 — penalty-fixed

Add gauge penalty to the spectral operator and compute modes in the full tangent space.
Fastest to implement, weakest physically.

### Formulation P2 — projected complement

Construct a projector `P_phys` onto a complement of the gauge image and compute spectra on:

```text
H_phys = P_phys^T H_* P_phys
```

This is the default Phase III target.

### Formulation P3 — quotient-aware generalized eigenproblem

Compute modes modulo gauge equivalence using an explicit reduced basis or nullspace quotient.
Harder, but preferred for later refinement.

Claude should implement P1 and P2 in Phase III, and leave P3 scaffolded if needed.

## 4.5 Mass matrix / inner product on state perturbations

A spectral problem needs an inner product on perturbations.
Define a state-space mass matrix:

```text
M_state
```

induced by declared block pairings and quadrature.

The default generalized eigenproblem is:

```text
H_phys v = lambda M_state v
```

Interpretation:

- `lambda >= 0` in a stable positive sector,
- negative `lambda` indicates instability directions,
- near-zero `lambda` may indicate gauge leakage, exact symmetries, flat directions, or unresolved discretization artifacts.

All spectral records must include a null-mode diagnosis.

## 4.6 Time-free nature of the executable branch

The current executable branch is not automatically a Lorentzian time-evolution PDE system.
Therefore the software must **not** over-interpret `lambda` as literal mass squared without a declared extraction rule.

Instead use the term:

```text
massLikeScale
```

with one of the following meanings:

1. eigenvalue-derived stiffness scale,
2. dispersion-fit proxy,
3. observed-signature fit proxy.

Every mass-like value must carry a provenance tag saying which interpretation was used.

## 4.7 PDE type and principal symbol support

Phase II already includes PDE classification work.
Phase III must reuse it to classify the linearized operator by principal symbol.

For a mode family or background, record:

- elliptic / hyperbolic / mixed / degenerate classification,
- symbol rank defects,
- branch/gauge dependence of classification,
- localization of defects in geometry or parameter space.

This is crucial because candidate bosons from ill-posed or gauge-leaking sectors must be demoted in claim strength.

## 4.8 Mode normalization

All extracted modes must be normalized under a declared convention.
Support at least:

1. `v^T M_state v = 1`,
2. `maxBlockNorm(v)=1`,
3. observed-output normalization.

The normalization convention must be serialized because it affects amplitudes, overlap metrics, and coupling proxies.

## 4.9 Mode overlap and matching

Mode matching across backgrounds or branches requires a declared overlap metric.
Implement at least three:

### O1 — native inner product overlap

```text
Overlap_native(v1, v2) = | v1^T M_state v2 |
```

### O2 — observed signature overlap

```text
Overlap_observed(v1, v2) = similarity( Obs(v1), Obs(v2) )
```

### O3 — invariant feature distance

Distance between feature vectors made of:

- eigenvalue band,
- block energy fractions,
- symmetry descriptors,
- polarization descriptors,
- observed-moment descriptors.

Matching must not rely on any single metric.
Use a weighted aggregate with tunable thresholds.

## 4.10 Property extraction

Phase III must extract properties from each mode or mode family.
At minimum implement the following properties.

### 4.10.1 Multiplicity

Count dimension of numerically clustered eigenspaces under a declared spectral gap tolerance.

### 4.10.2 Mass-like scale

Compute from one of:

- `lambda`,
- a dispersion fit over background parameter variation,
- observed-space fit.

### 4.10.3 Polarization / tensor class

Measure how the mode energy distributes among state blocks and tensor signatures.
Examples:

- scalar-like,
- vector-like,
- 2-form-like,
- mixed,
- connection-dominant,
- auxiliary-block-dominant.

This classification is operational, not metaphysical.

### 4.10.4 Symmetry content

Given a background symmetry group or approximate isotropy group, compute the mode’s transformation behavior under the discrete/continuous symmetry actions available in the environment.

For implementation, support at least:

- parity-like involutions,
- discrete rotational symmetries of the mesh/environment,
- branch-manifest algebra decomposition labels.

### 4.10.5 Gauge contamination score

Compute fraction of the mode lying in or near the estimated gauge image:

```text
gaugeLeak(v) = || P_gauge v || / ||v||
```

This must be a first-class property because it controls claim demotion.

### 4.10.6 Branch stability score

For matched modes across branch variants:

```text
branchStability = function(
  eigenvalue drift,
  overlap persistence,
  observed-signature drift,
  multiplicity stability,
  property stability
)
```

### 4.10.7 Refinement stability score

Measure persistence under mesh / discretization refinement.

### 4.10.8 Backend stability score

Measure persistence between CPU and CUDA pipelines.

### 4.10.9 Interaction proxy

Phase III must compute at least one nonlinear interaction proxy.
Use a third-order residual derivative or finite-difference cubic response proxy:

```text
C(v_i, v_j, v_k) ≈ directional multilinear response of Upsilon or G
```

This is not a full scattering amplitude.
It is a first nonlinear coupling descriptor.

## 4.11 Observed signature of a mode

A native-space mode is not yet comparison-ready.
For each mode `v`, build an observed-space signature.

Define:

```text
ObsMode(v) = D(ObservationPipeline)_{z_*}[v]
```

This means:

1. linearize the observation pipeline around the background,
2. apply the linearized observation map to the mode,
3. produce typed X-space outputs.

Observed outputs may include:

- scalar fields on `X_h`,
- vector/tensor fields on `X_h`,
- low-order moments,
- spectral densities,
- derived invariants,
- signature summaries for comparison campaigns.

The mode observation map must be implemented explicitly.
Do not approximate it by simply observing `z_* + eps v` unless the nonlinear finite-difference path is intentionally selected and recorded.

## 4.12 Candidate boson family

A candidate boson family is a collection of matched modes across contexts.
Implement:

```text
CandidateBosonFamily = {
  FamilyId,
  MemberModes: [...],
  MatchingEvidence: [...],
  StablePropertyEnvelope,
  BranchStabilityScore,
  RefinementStabilityScore,
  BackendStabilityScore,
  ObservationStabilityScore,
  ClaimClass,
  AmbiguityNotes
}
```

## 4.13 Claim classes

The registry must classify boson candidates by claim strength.
Use at least:

- `C0_NumericalMode`: raw computed mode, no matching yet,
- `C1_LocalPersistentMode`: persistent under local continuation/refinement,
- `C2_BranchStableBosonicCandidate`: persists across branch family,
- `C3_ObservedStableCandidate`: also stable in observed-space signatures,
- `C4_PhysicalAnalogyCandidate`: comparable to an external target but not unique,
- `C5_StrongIdentificationCandidate`: strong evidence, still not proof.

The system must be able to **demote** claims automatically when gauge leak, refinement fragility, branch fragility, or comparison mismatch appears.

---

# 5. New modules to add in Phase III

Add the following source projects.
These names are recommendations; equivalent names are acceptable if the responsibilities remain the same.

## 5.1 `src/Gu.Phase3.Backgrounds`

Responsible for:

- background specifications,
- background solve orchestration,
- continuation-derived background sets,
- background admissibility grading,
- background atlas serialization.

## 5.2 `src/Gu.Phase3.GaugeReduction`

Responsible for:

- linearized gauge action,
- gauge image estimation,
- physical complement projectors,
- quotient-aware basis construction,
- gauge leak diagnostics.

## 5.3 `src/Gu.Phase3.Spectra`

Responsible for:

- Gauss-Newton and full Hessian operator assembly,
- generalized eigenproblems,
- sparse iterative eigensolvers,
- nullspace handling,
- mode normalization,
- spectral clustering.

## 5.4 `src/Gu.Phase3.ModeTracking`

Responsible for:

- mode overlap metrics,
- matching across branches/backgrounds/refinement,
- family assignment,
- continuation mode transport,
- avoided crossing / bifurcation handling.

## 5.5 `src/Gu.Phase3.Properties`

Responsible for:

- multiplicity extraction,
- mass-like scale extraction,
- polarization/tensor-class extraction,
- symmetry content extraction,
- gauge leak score,
- branch/refinement/backend stability scores,
- interaction proxies.

## 5.6 `src/Gu.Phase3.Observables`

Responsible for:

- linearized observation operator,
- mode observation transforms,
- observed signature bundles,
- observed overlap metrics,
- exportable comparison descriptors.

## 5.7 `src/Gu.Phase3.Registry`

Responsible for:

- candidate boson records,
- family records,
- claim classes,
- demotion rules,
- boson registry serialization,
- query and reporting APIs.

## 5.8 `src/Gu.Phase3.Campaigns`

Responsible for:

- boson-focused comparison campaigns,
- target profile definitions,
- parameter scans,
- branch-family boson studies,
- report generation hooks.

## 5.9 `src/Gu.Phase3.Reporting`

Responsible for:

- boson atlas reports,
- spectrum sheets,
- stability summaries,
- ambiguity maps,
- negative-result summaries,
- registry export.

## 5.10 Native additions under `native/`

Add CUDA/native support for:

- Jacobian-vector products,
- transposed Jacobian-vector products,
- Hessian-vector approximations,
- state mass matrix actions,
- iterative eigensolver primitives,
- overlap reductions,
- mode-feature reductions.

---

# 6. New data types Claude must implement

This section lists the most important types.

## 6.1 Background types

```text
BackgroundSpec
BackgroundSeed
BackgroundSolveOptions
BackgroundMetrics
BackgroundRecord
BackgroundAtlas
```

### `BackgroundRecord`

Must include:

- `BackgroundId`
- `EnvironmentId`
- `BranchManifestId`
- `ContinuationCoordinates`
- `GeometryFingerprint`
- `GaugeChoice`
- `StateArtifactRef`
- `ResidualNorm`
- `StationarityNorm`
- `AdmissibilityLevel` (`B0`/`B1`/`B2`)
- `SolveTraceRef`
- `ReplayTierAchieved`

## 6.2 Gauge reduction types

```text
GaugeActionLinearization
GaugeBasis
GaugeProjector
PhysicalProjector
ConstraintDefectReport
GaugeLeakReport
```

## 6.3 Spectral types

```text
LinearizedOperatorSpec
LinearizedOperatorBundle
GeneralizedEigenproblemSpec
ModeRecord
ModeCluster
SpectrumRecord
SpectrumBundle
NullModeDiagnosis
```

### `ModeRecord`

Must include:

- `ModeId`
- `BackgroundId`
- `OperatorType` (`GaussNewton`, `FullHessian`, etc.)
- `Eigenvalue`
- `NormalizationConvention`
- `MultiplicityClusterId`
- `GaugeLeakScore`
- `TensorEnergyFractions`
- `BlockEnergyFractions`
- `ModeVectorArtifactRef`
- `ObservedSignatureRef`
- `NullModeDiagnosis`

## 6.4 Mode tracking types

```text
ModeMatchMetricSet
ModeAlignmentRecord
ModeFamilyRecord
CrossBranchModeMap
ContinuationModeTrack
```

## 6.5 Property extraction types

```text
MassLikeScaleRecord
PolarizationDescriptor
SymmetryDescriptor
InteractionProxyRecord
StabilityScoreCard
BosonPropertyVector
```

## 6.6 Boson registry types

```text
CandidateBosonRecord
CandidateBosonFamily
BosonClaimClass
BosonDemotionRecord
BosonRegistry
BosonQuery
```

### `CandidateBosonRecord`

Must include:

- `CandidateId`
- `PrimaryFamilyId`
- `ContributingModeIds`
- `BackgroundSet`
- `BranchVariantSet`
- `MassLikeEnvelope`
- `MultiplicityEnvelope`
- `PolarizationEnvelope`
- `SymmetryEnvelope`
- `GaugeLeakEnvelope`
- `BranchStabilityScore`
- `RefinementStabilityScore`
- `BackendStabilityScore`
- `ObservationStabilityScore`
- `InteractionProxyEnvelope`
- `ComparisonLinks`
- `ClaimClass`
- `Demotions`
- `AmbiguityNotes`
- `RegistryVersion`

---

# 7. Algorithms Claude must implement

This section gives the concrete algorithmic work.

## 7.1 Background atlas construction

Goal: generate a curated set of admissible backgrounds to linearize around.

### Algorithm

1. Select environments and branch variants.
2. Choose seeds:
   - trivial/near-trivial,
   - symmetric ansatz,
   - continuation from previous branches,
   - coarse-grid solves.
3. Solve for `B1` or `B2` backgrounds using existing solver modes plus continuation.
4. Deduplicate background records by structural invariants and state distance threshold.
5. Grade each background:
   - residual norm,
   - stationarity norm,
   - gauge defect,
   - PDE class,
   - refinement consistency.
6. Persist into `BackgroundAtlas`.

### Mandatory outputs

- atlas index,
- background ranking,
- rejected background list with reasons,
- reproducible state artifacts.

## 7.2 Gauge basis and physical projector

Goal: obtain a usable physical-mode space.

### Algorithm

1. Implement `Gamma_*` for the active branch field layout.
2. Sample or build basis vectors for infinitesimal gauge directions.
3. Assemble gauge Gram matrix under `M_state`.
4. Orthonormalize gauge basis.
5. Construct projector:

```text
P_gauge = Q_gauge Q_gauge^T M_state
P_phys  = I - P_gauge
```

6. Measure leakage by projecting trial vectors onto the gauge image.
7. Serialize projector diagnostics.

### Notes

- If the gauge basis is rank deficient, keep the defect explicit.
- Do not silently invert ill-conditioned Gram matrices.
- For unstable ranks, fall back to SVD-based pseudoinverse with serialized cutoffs.

## 7.3 Linearized operator assembly

Goal: build operator actions for mode solves.

### Required operators

1. `ApplyJ(v)`
2. `ApplyJT(w)`
3. `ApplyH_GN(v) = ApplyJT( M_Upsilon ApplyJ(v) )`
4. `ApplyH_full(v)` including residual correction
5. `ApplyM_state(v)`
6. `ApplyP_phys(v)`

### Assembly strategy

Prefer matrix-free first, explicit sparse assembly optional.
Support:

- CPU reference implementation,
- CUDA accelerated operator actions,
- optional explicit sparse export for debugging.

## 7.4 Spectral solve

Goal: compute the low-lying and unstable spectrum.

### Recommended methods

Implement in this order:

1. block power / inverse iteration for tiny debug systems,
2. Lanczos for symmetric generalized eigenproblems,
3. LOBPCG or Jacobi-Davidson for larger sparse problems,
4. shift-invert paths for interior eigenvalues when stable.

### Default problem

```text
P_phys^T H P_phys v = lambda P_phys^T M_state P_phys v
```

### Mandatory outputs

- converged eigenpairs,
- residual norm per eigenpair,
- orthogonality diagnostics,
- null-mode diagnostics,
- CPU/GPU parity status.

## 7.5 Spectral clustering and multiplicity

Goal: classify near-degenerate eigenspaces.

### Algorithm

1. sort eigenvalues,
2. cluster using relative and absolute spectral gap tolerances,
3. verify with overlap of eigenvectors under perturbation/refinement,
4. assign a `ModeClusterId`.

Multiplicity must never be inferred from one run alone when continuation/refinement data is available.

## 7.6 Mode tracking across contexts

Goal: track which modes persist across changes.

### Contexts

- branch variant changes,
- background continuation steps,
- mesh refinement steps,
- backend changes.

### Algorithm

1. compute overlap matrix under native and observed metrics,
2. compute feature-vector distances,
3. solve assignment problem (Hungarian algorithm or stable matching with thresholds),
4. detect births/deaths/splits/merges/avoided crossings,
5. update `ModeFamilyRecord`.

### Mandatory rule

If matching is ambiguous, record ambiguity.
Do not force one-to-one matches without evidence.

## 7.7 Property extraction

For each mode or mode family, compute:

- multiplicity,
- mass-like scale,
- block/tensor energy fractions,
- polarization descriptor,
- symmetry descriptor,
- gauge leak score,
- branch/refinement/backend stability scores,
- interaction proxy.

### Interaction proxy implementation

Start with finite-difference directional third response around the background:

```text
C(v_i, v_j, v_k)
  ≈ finite-difference evaluation of G or Upsilon under combined perturbations
```

CPU reference first.
CUDA optimization later.

## 7.8 Linearized observation map

Goal: compute observed signatures of modes without abusing nonlinear finite differences.

### Required operator

```text
ApplyDObs(v) = linearized observation pipeline at background applied to v
```

### Implementation stages

1. analytic / chain-rule linearization where available,
2. validated finite-difference fallback,
3. compare both where possible.

Every observed signature record must declare whether it came from:

- analytic linearization,
- finite-difference approximation,
- hybrid path.

## 7.9 Candidate boson family construction

Goal: turn mode families into registry entries.

### Algorithm

1. collect related mode families,
2. aggregate property envelopes,
3. evaluate stability scores,
4. run claim demotion rules,
5. generate `CandidateBosonRecord`,
6. link to observed signatures and comparison targets.

## 7.10 Boson comparison campaigns

Goal: compare candidate bosons to external targets or internal target profiles.

Support two comparison modes.

### Mode BC1 — internal target profile comparison

Compare against abstract target descriptors such as:

- massless vector-like,
- massive scalar-like,
- branch-stable mixed tensor family,
- two-polarization low-leak family.

### Mode BC2 — external analogy comparison

Compare candidate families to external data profiles using typed descriptors and uncertainty budgets.

### Mandatory rule

External comparison may produce:

- compatibility,
- incompatibility,
- underdetermination,
- not enough evidence.

Never force a unique match.

---

# 8. CUDA and native acceleration plan

Phase III must preserve the Phase I rule:

> CPU reference before CUDA trust.

## 8.1 CPU-first requirements

All of the following must exist on CPU first:

- `ApplyJ`,
- `ApplyJT`,
- `ApplyH_GN`,
- `ApplyM_state`,
- `ApplyP_phys`,
- spectral solve for small systems,
- observed mode map,
- property extraction reductions.

## 8.2 CUDA priorities

Prioritize CUDA work in this order:

1. Jacobian-vector products,
2. transposed Jacobian-vector products,
3. state-mass actions,
4. projected Hessian-vector products,
5. eigensolver primitives,
6. overlap and feature reductions,
7. interaction-proxy batch evaluation.

## 8.3 GPU spectral strategy

Prefer matrix-free iterative methods.
Do not build giant dense Hessians.

Recommended GPU path:

- LOBPCG / Lanczos with custom operator callbacks,
- batched Rayleigh quotient kernels,
- batched orthogonalization reductions,
- selective shift-invert only when a stable sparse factorization path exists.

## 8.4 CPU/GPU parity requirements

For every spectrum bundle, record:

- eigenvalue drift,
- mode overlap drift,
- property drift,
- observed-signature drift,
- registry effect.

No candidate boson may receive a high claim class if its defining mode family is backend-fragile.

---

# 9. CLI, schemas, and artifact additions

## 9.1 New CLI commands

Add commands along these lines:

```bash
dotnet run --project apps/Gu.Cli -- create-background-study [outputPath]
dotnet run --project apps/Gu.Cli -- solve-backgrounds [studyPath]
dotnet run --project apps/Gu.Cli -- compute-spectrum [runFolder] [backgroundId]
dotnet run --project apps/Gu.Cli -- track-modes [studyPath]
dotnet run --project apps/Gu.Cli -- build-boson-registry [studyPath]
dotnet run --project apps/Gu.Cli -- run-boson-campaign [campaignPath]
dotnet run --project apps/Gu.Cli -- export-boson-report [studyPath] [outputPath]
```

## 9.2 New schemas

Add schemas for:

- `background-study.schema.json`
- `background-record.schema.json`
- `spectrum-bundle.schema.json`
- `mode-record.schema.json`
- `mode-family.schema.json`
- `boson-registry.schema.json`
- `boson-campaign.schema.json`
- `boson-report.schema.json`

## 9.3 Run folder additions

Extend the canonical run/study structure:

```text
run/
  backgrounds/
    atlas.json
    background_records/
  spectra/
    spectrum_bundle.json
    eigenpairs/
    gauge/
  modes/
    mode_families.json
    alignment/
  bosons/
    registry.json
    candidates/
    demotions/
  observables/
    mode_signatures/
  campaigns/
    boson_campaigns/
    results/
  reports/
    boson_atlas.md
    spectrum_summary.md
```

Everything must remain replayable and integrity-hashed.

---

# 10. Workbench additions

The Vulkan workbench remains read-only and artifact-driven.
Add diagnostic views for:

- background atlas browser,
- spectral ladder plots,
- eigenmode amplitude overlays,
- gauge leak visualization,
- branch/refinement mode tracks,
- boson family cards,
- observed-signature comparisons,
- ambiguity heatmaps.

Workbench output must remain diagnostic only.
It must not alter any scientific state.

---

# 11. Testing strategy

Phase III requires a large test suite.
The most important tests are below.

## 11.1 Background tests

- background admissibility grading,
- background deduplication,
- continuation-generated background persistence,
- replay stability of background artifacts.

## 11.2 Gauge reduction tests

- gauge basis rank tests,
- projector idempotence,
- projector symmetry under `M_state`,
- gauge leak near zero for known gauge directions,
- defect handling for rank-deficient gauges.

## 11.3 Spectral tests

- tiny analytic toy spectra,
- CPU matrix-free vs explicit sparse agreement,
- generalized eigen residual checks,
- null-mode detection,
- degeneracy clustering behavior,
- Gauss-Newton vs full-Hessian branch difference reporting.

## 11.4 Mode tracking tests

- persistence across tiny continuation steps,
- branch match stability,
- ambiguity handling for crossings,
- split/merge bookkeeping.

## 11.5 Observation tests

- analytic vs finite-difference linearized observation agreement,
- mode signature stability under normalization changes,
- no direct Y-space bypass in comparison paths.

## 11.6 Registry tests

- claim class assignment,
- automatic demotion,
- serialization round-trip,
- registry diffing between reruns.

## 11.7 Backend parity tests

- CPU/GPU operator-action parity,
- CPU/GPU low-spectrum parity,
- property parity,
- observed-signature parity,
- campaign result parity thresholds.

## 11.8 Negative-result preservation tests

- failed background solve persists,
- failed eigensolve persists,
- ambiguous mode match persists,
- demoted boson candidate persists,
- comparison mismatch persists.

---

# 12. Milestone plan

This is the concrete implementation order.

## M23 — Background atlas core

Deliver:

- `Gu.Phase3.Backgrounds`
- background schemas
- atlas builder
- admissibility grading
- CLI commands for background studies

Completion criteria:

- can generate a study with multiple backgrounds,
- can serialize/replay backgrounds,
- can rank backgrounds by admissibility.

## M24 — Gauge reduction core

Deliver:

- linearized gauge action,
- gauge basis builder,
- physical projector,
- gauge leak diagnostics.

Completion criteria:

- projector works on toy 2D/3D cases,
- known gauge directions are detected,
- defect reports serialize cleanly.

## M25 — Spectral operator bundle

Deliver:

- `ApplyJ`, `ApplyJT`, `ApplyH_GN`, `ApplyM_state`, `ApplyP_phys`,
- CPU reference implementation,
- explicit operator bundle artifacts.

Completion criteria:

- operator bundle builds for at least one `B1` background,
- operator self-checks pass,
- tiny debug spectra compute successfully.

## M26 — CPU spectral solver stack

Deliver:

- generalized eigensolver pipeline,
- mode normalization,
- null-mode diagnostics,
- clustering and multiplicity logic.

Completion criteria:

- can compute low-lying spectrum for toy 2D/3D examples,
- residual norms and orthogonality diagnostics are reported,
- multiplicity clustering is stable under repeated runs.

## M27 — Mode tracking and family construction

Deliver:

- overlap metrics,
- matching engine,
- continuation/branch/refinement trackers,
- family records.

Completion criteria:

- mode families persist across at least one continuation path and one branch sweep,
- ambiguous crossings are reported rather than collapsed.

## M28 — Property extraction engine

Deliver:

- mass-like scales,
- polarization descriptors,
- symmetry descriptors,
- gauge leak scores,
- branch/refinement/backend stability scores,
- interaction proxy v1.

Completion criteria:

- each computed mode has a property vector,
- property vectors serialize and compare,
- interaction proxy works on at least small debug systems.

## M29 — Linearized observation of modes

Deliver:

- `ApplyDObs`,
- observed mode signatures,
- observed overlap metrics,
- analytic vs finite-difference validation.

Completion criteria:

- at least one environment can export observed mode signatures,
- signature stability under rerun is verified.

## M30 — Candidate boson registry

Deliver:

- candidate family construction,
- claim classes,
- demotion rules,
- registry serialization and querying.

Completion criteria:

- at least one registry is generated from real mode families,
- ambiguous candidates are represented explicitly,
- demotion logic triggers correctly.

## M31 — CUDA spectral acceleration

Deliver:

- GPU operator actions,
- GPU eigensolver primitives,
- parity tests,
- performance benchmarks.

Completion criteria:

- GPU spectrum agrees with CPU within declared thresholds,
- benchmark results are stored in artifacts,
- no high-claim candidate depends on unverified GPU-only logic.

## M32 — Boson campaign and reporting layer

Deliver:

- boson-focused campaigns,
- internal target profiles,
- external analogy comparison,
- boson atlas report generation.

Completion criteria:

- can generate a full boson atlas report from a study,
- negative/ambiguous results appear in the report,
- campaign outputs are replayable and integrity-checked.

---

# 13. Recommended first study after implementation

Once M23-M30 are complete, run the first serious Phase III study.

## Study: Bosonic persistence atlas

### Goal

Determine which low-lying bosonic mode families persist across:

- at least 3 torsion variants,
- at least 3 Shiab variants,
- at least 3 gauge settings,
- at least 2 environments,
- at least 3 discretization levels.

### Record for each family

- existence frequency,
- eigenvalue band,
- multiplicity envelope,
- gauge leak envelope,
- branch stability score,
- refinement stability score,
- observed-signature stability,
- best-fit target profile,
- claim class,
- demotions.

### Why this is the right first study

It gives the first disciplined answer to:

- whether candidate bosonic sectors are branch-stable,
- whether the mode story survives refinement,
- whether observation stabilizes or destabilizes identifications,
- whether any family is strong enough to treat as a serious candidate boson sector.

---

# 14. Engineering rules Claude must follow

## 14.1 Never hide ambiguity

If mode matching, gauge reduction, or identification is ambiguous, serialize the ambiguity.
Do not choose silently.

## 14.2 CPU reference before CUDA trust

All GPU logic must have a CPU reference path and parity tests.

## 14.3 No direct Y-space comparison

Physical comparison must go through the declared linearized observation pipeline.

## 14.4 Preserve negative results

Background failures, unstable modes, branch-fragile candidates, and demotions are first-class outputs.

## 14.5 Every strong claim needs provenance

Any mass-like scale, symmetry tag, or candidate identification must include:

- background provenance,
- branch provenance,
- operator provenance,
- normalization provenance,
- observation provenance,
- replay tier,
- uncertainty notes.

## 14.6 Do not over-interpret eigenvalues

Until a stronger physical dictionary exists, treat eigenvalues as spectral invariants or mass-like proxies, not automatically physical particle masses.

---

# 15. What remains intentionally out of scope after Phase III

Claude must leave these out of scope unless explicitly asked in a later phase.

## 15.1 Fermionic spectrum completion

Phase III is boson-focused.
Fermionic mode extraction, spinor bundles, Dirac sectors, and chirality-sensitive identification remain for a later phase.

## 15.2 Full physical particle dictionary

Phase III may produce candidate boson families and physical analogies.
It does not complete a unique final mapping from geometric sectors to known particles.

## 15.3 Quantum completion

No path integral, canonical quantization, operator algebra quantization, loop corrections, renormalization, or anomaly program is required here.

## 15.4 Scattering / S-matrix program

Interaction proxies are included, but full scattering amplitude machinery is out of scope.

## 15.5 Final mass/coupling prediction program

Mass-like scales and coupling proxies are allowed.
Exact physical masses and couplings are not yet expected.

## 15.6 Global mathematical closure

Global well-posedness, uniqueness, branch-independence proofs, and global observerse existence remain unresolved.

## 15.7 Cosmological or fully realistic environment campaigns

Phase III should begin with toy and structured environments.
Large-scale or high-fidelity physical environments belong in a later phase.

## 15.8 Fully automatic symbolic derivation

If symbolic support is improved, it should help operator generation and verification.
A complete symbolic theorem engine is out of scope.

---

# 16. Suggested deliverables Claude should leave at the end of Phase III

By the end of this phase, Claude should leave the codebase with:

1. a working background atlas subsystem,
2. a gauge-aware spectral solver stack,
3. mode tracking across branches and refinement,
4. property extraction for bosonic modes,
5. observed-space mode signature generation,
6. a candidate boson registry with claim demotion,
7. at least one bosonic persistence atlas report,
8. CPU/GPU parity tests for core spectral operations,
9. updated schemas, CLI commands, benchmarks, and tests,
10. a clear list of unresolved issues for the next phase.

If these are complete, the platform will have moved from “branch/stability research engine” to “candidate boson discovery and classification engine.”
That is the correct Phase III target.


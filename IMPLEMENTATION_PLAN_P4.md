# IMPLEMENTATION_PLAN_P4.md

# Geometric Unity Phase IV Fermionic Sector, Coupling Extraction, and Unified Particle Registry Implementation Plan

## Purpose

This document is a **standalone Phase IV implementation handoff** for Claude Code.

It assumes **no access to the original Geometric Unity completion manuscript**.
It is written to continue the implementation after the bosonic-spectrum-focused Phase III plan.

It assumes the following prior phases are already in place:

- **Phase I**: minimal executable bosonic branch, typed artifacts, CPU reference backend, CUDA backend, observation pipeline, validation, replay, integrity.
- **Phase II**: branch family manifests, canonicity studies, continuation, linearization / PDE classification, recovery graph, comparison campaigns, reporting.
- **Phase III**: background atlas construction, gauge-aware bosonic spectral solver, property extraction, candidate boson registry, observed-signature generation, and evidence-ranked boson identification workflow.

Phase IV is the first phase that aims to move from:

> “candidate bosonic sectors and their observed signatures”

into:

> “fermionic sector implementation, boson–fermion interaction extraction, chirality-aware identification, generation-like clustering, and a unified candidate particle registry.”

This is **not** yet a final proof of the complete physical particle dictionary.
It is the next executable research layer.

Phase IV must be able to:

1. implement a discrete fermionic state model on top of the existing observerse / branch framework,
2. define and compute a branch-aware Dirac-type operator and its linearized action,
3. represent chirality, spin content, conjugation rules, and admissible fermionic bilinears,
4. couple fermionic states to the Phase III bosonic backgrounds,
5. extract candidate fermionic modes and their mass-like / symmetry-like properties,
6. compute boson–fermion coupling proxies from background-dependent and fluctuation-dependent operators,
7. trace all candidate sectors through the existing observation / recovery / comparison pipeline,
8. build a unified candidate particle registry that contains bosons, fermions, and interaction records,
9. preserve ambiguity, demote weak claims, and prevent premature physical over-identification,
10. leave behind enough structure for a later quantum / scattering / renormalization phase.

The central discipline for this phase is:

> Phase IV is allowed to produce candidate fermion sectors, candidate boson–fermion couplings, candidate generation structure, chirality statements, stability statements, and evidence-ranked physical identifications.
> Phase IV is **not allowed** to silently promote these into a complete Standard Model derivation, exact measured particle masses, or a proof that the final particle dictionary is uniquely settled.

---

# 1. What Phase IV is and is not

## 1.1 What Phase IV is

Phase IV is the implementation of the **fermionic sector and unified particle extraction layer** on top of the Phase I + Phase II + Phase III platform.

It must provide:

1. a **fermionic field layout framework** compatible with branch manifests,
2. a **spin / spinor / Clifford data model** suitable for discrete computation,
3. a **branch-aware Dirac operator stack** on `Y_h`,
4. a **chirality and conjugation framework**,
5. a **fermionic background and mode solver**,
6. a **boson–fermion coupling extractor**,
7. a **generation / family clustering workflow**,
8. an **observed fermion signature builder** on `X_h`,
9. a **unified particle registry** combining bosons, fermions, and interactions,
10. replayable artifacts, schemas, tests, and reports for all of the above.

## 1.2 What Phase IV is not

Phase IV is **not**:

- a proof that Geometric Unity is correct,
- a proof that all known fermions are uniquely derived,
- a final Standard Model derivation,
- a full quantum field theory completion,
- a renormalization program,
- a scattering-amplitude engine,
- a loop-correction engine,
- a final CKM / PMNS prediction engine,
- a proof of anomaly cancellation in every branch,
- a proof of exact measured masses or couplings,
- a proof that every extracted candidate corresponds to a physical particle.

Phase IV can provide **candidate fermion families**, **candidate couplings**, **chirality-aware identifications**, and **evidence-ranked unified particle registries**.
It cannot collapse all ambiguity unless the computations actually demonstrate canonicity.

---

# 2. Minimal recap Claude must assume from prior phases

This section restates the minimum mathematical framework Phase IV extends.

## 2.1 Two-space discipline

The executable framework uses two spaces:

- `X`: observed / base manifold,
- `Y`: native / observerse total space where the main dynamical fields live.

Discrete forms:

- `X_h`,
- `Y_h`.

All primary fermionic and bosonic mode computation happens on `Y_h`.
All physical comparison must pass through the declared observation pipeline onto `X_h`.

## 2.2 Structural and observational maps

There are two distinct maps:

- `pi : Y -> X`, structural projection,
- `sigma`, observation section / observation map.

Discrete forms:

- `pi_h`,
- `sigma_h`,
- `sigma_h^*` for pullback / descent / extraction.

No candidate particle may be compared to external targets using raw `Y_h` tensors or raw `Y_h` spinor blocks alone.
All such quantities must move through an allowed recovery / observation chain.

## 2.3 Bosonic core already available

The active bosonic system already supports a branch-defined distinguished connection and residual construction:

- connection-like state blocks,
- curvature `F`,
- torsion branch `T`,
- Shiab branch `S`,
- residual `Upsilon = S - T`,
- objective `I2_h = (1/2) Upsilon^T M_Upsilon Upsilon`,
- Jacobian `J_h`,
- stationarity operator `G_h = J_h^T M_Upsilon Upsilon`,
- bosonic background atlas,
- bosonic spectral and property extraction layer from Phase III.

Phase IV must not reimplement these.
It must consume them.

## 2.4 Branch dependence remains explicit

Nothing about the fermionic sector may be silently hardcoded as “the one true form” if the mathematics is not uniquely fixed.
Whenever there are multiple plausible constructions, the choice must become:

- a branch-defined operator family,
- a manifest-declared convention,
- a serialized provenance record,
- or an evidence-ranked ambiguity.

## 2.5 Phase II and Phase III assets Phase IV depends on

Phase IV assumes the following exist and are reusable:

- branch family manifests,
- canonicity analyzer,
- continuation runner,
- PDE classifier,
- stability atlas builder,
- recovery graph,
- identification gate,
- prediction validator,
- comparison campaigns,
- research reporting,
- background atlas subsystem,
- gauge-aware spectral solver,
- boson property extraction engine,
- candidate boson registry.

Phase IV builds on these rather than replacing them.

---

# 3. Operational target for Phase IV

Claude needs a precise target.

The Phase IV system should compute and serialize the following research objects:

1. **Candidate fermionic background states**
2. **Candidate fermionic modes** around selected bosonic backgrounds
3. **Chirality-tagged fermionic sectors**
4. **Conjugation-paired sectors**
5. **Mass-like spectral invariants** and refinement / branch stability scores
6. **Boson–fermion interaction records**
7. **Generation-like or family-like clustering records**
8. **Observed fermionic signatures** on `X_h`
9. **Unified particle candidates** that may aggregate boson, fermion, and coupling evidence
10. **Negative results** where sectors fail stability, fail observation, or remain branch-fragile

The unifying product of Phase IV is:

```text
UnifiedParticleRegistry = {
  CandidateBosons,
  CandidateFermions,
  CandidateInteractions,
  MatchingEdges,
  ClaimDemotions,
  StabilitySummaries,
  ObservationSummaries,
  ComparisonSummaries,
  AmbiguityNotes
}
```

---

# 4. Mathematical framework Claude must implement

This section provides the minimum math specification Phase IV needs.

## 4.1 Fermionic state layout

The bosonic state from earlier phases remains present.
Phase IV adds a fermionic block.

Use the generalized state layout:

```text
state = (z_bos, psi, psi_bar, eta_aux)
```

where:

- `z_bos` is the bosonic state already supported,
- `psi` is a discrete spinor-like fermionic field on `Y_h`,
- `psi_bar` is the dual / conjugate representation used for bilinears,
- `eta_aux` holds optional auxiliary fermionic blocks if needed by a branch.

Not every branch must include all blocks.
The layout must be driven by the manifest.

Required abstraction:

```text
FermionFieldLayout = {
  SpinorBlocks: [ ... ],
  DualBlocks: [ ... ],
  AuxiliaryBlocks: [ ... ],
  ChiralityBlocks: [ ... ],
  ConjugationRules: [ ... ],
  AllowedBilinears: [ ... ],
  ObservationEligibility: [ ... ]
}
```

## 4.2 Spinor representation model

Do not hardcode a single representation prematurely.
Represent spinor content abstractly, then provide explicit discrete realizations.

Use:

```text
SpinorRepresentationSpec = {
  CliffordSignature,
  GammaBasisConvention,
  SpinBundleConvention,
  ChiralityConvention,
  ConjugationConvention,
  InnerProductConvention,
  RealComplexMode,
  BlockDimensions,
  ReductionRules
}
```

The implementation must support at least:

- complex spinor realization,
- a chirality decomposition,
- discrete gamma-action,
- conjugation and dual pairing,
- bilinear forms with explicit symmetry annotations.

## 4.3 Discrete gamma system

On each local patch / cell / element, implement gamma-action operators:

```text
Gamma_h(mu): SpinorBlock -> SpinorBlock
```

with algebraic validation checks based on the chosen metric signature convention.
Do not assume the continuum symbol `gamma^mu` is enough; implement a discrete operator bundle that can be applied cellwise, blockwise, and in assembled operator form.

Required validation targets:

- anticommutation consistency up to tolerance,
- basis-convention reproducibility,
- conjugation compatibility,
- chirality-operator compatibility,
- backend parity.

## 4.4 Spin connection and Dirac-type operator

The key Phase IV construction is a branch-aware Dirac-type operator coupled to the bosonic background.

Define a branch-provided effective spin connection:

```text
Omega_spin = Mu_spin(z_bos, branchParameters, geometryContext)
```

and use it to build a discrete covariant derivative on spinor blocks:

```text
nabla_spin,h = d_h + Omega_spin,h
```

Then construct a discrete Dirac-type operator:

```text
D_h = Sum_mu Gamma_h(mu) * nabla_spin,h(mu) + M_branch + C_branch
```

where:

- `Gamma_h(mu)` are discrete gamma actions,
- `nabla_spin,h(mu)` are directional or basis-indexed covariant derivatives,
- `M_branch` is an optional branch-defined mass-like / mixing / torsion-dependent block,
- `C_branch` is an optional correction term capturing branch-specific geometric couplings.

Important discipline:

- `M_branch` is not automatically a physical mass matrix,
- `C_branch` is not automatically negligible,
- every such term must be manifest-declared and serialized.

## 4.5 Fermionic action / residual

Phase IV should support two compatible formulations:

### Formulation A: operator-centric mode analysis

Treat fermions primarily through spectral analysis of `D_h` and related reduced operators.

### Formulation B: variational fermionic residual

If the branch supports it, also define a fermionic residual:

```text
R_psi(psi; z_bos) = D_h(z_bos) psi + N_psi(psi, z_bos)
```

with optional nonlinear fermionic terms `N_psi`.
The minimal required version for Phase IV is linear or linearized mode extraction around bosonic backgrounds.

## 4.6 Chirality and projection operators

Implement branch-aware chirality projectors:

```text
P_L = (1/2) (I - Gamma_chi)
P_R = (1/2) (I + Gamma_chi)
```

with the sign convention manifest-declared.

Use these to decompose modes into:

- left-chiral content,
- right-chiral content,
- mixed content,
- chirality leakage diagnostics.

These diagnostics must be tracked under:

- branch variation,
- refinement,
- continuation,
- observation.

## 4.7 Conjugation structure

Implement a conjugation / duality framework explicit enough for bilinears and interaction terms.

Required operations:

```text
psi -> psi_bar
ChargeConjugate(psi)
DualPair(psi_bar, psi)
```

Serialize which conjugation convention is used.
If multiple plausible conjugation maps exist, they must become branch variants.

## 4.8 Fermionic spectral problem

Around a chosen bosonic background `z_*`, define the linear fermionic mode problem:

```text
D_h(z_*) phi = lambda M_psi phi
```

where `M_psi` is the fermionic inner-product / mass matrix / pairing operator.

This is the central eigenproblem for candidate fermionic sectors.

Compute and serialize:

- eigenvalues / singular values / generalized spectra,
- eigenvectors / eigenspaces,
- chirality decomposition,
- conjugation pairing,
- stability across refinement,
- stability across branch families,
- observed-space signatures.

## 4.9 Boson–fermion coupling extraction

Phase IV must extract interaction proxies from the dependence of `D_h` and other fermionic operators on bosonic modes.

If bosonic background is perturbed by a bosonic fluctuation `delta z_bos`, define:

```text
delta D_h = (dD_h / dz_bos)[delta z_bos]
```

Then define coupling proxy amplitudes between fermionic modes `phi_i`, `phi_j` and bosonic mode `b_k` by pairings such as:

```text
g_ijk = <phi_i_bar, delta D_h[b_k] phi_j>
```

This is not yet a full scattering amplitude.
It is a structured coupling proxy.

Serialize:

- raw coupling matrix entries,
- normalized coupling proxies,
- symmetry / selection-rule metadata,
- branch stability scores,
- observation-linked interaction signatures if available.

## 4.10 Generation / family clustering

Do not assume Standard Model generations emerge automatically.
Instead, implement a clustering layer.

Given fermionic candidates with properties:

- chirality profile,
- conjugation class,
- mass-like scale,
- symmetry tags,
- branch stability,
- observed signature,
- coupling profile,

construct a clustering / matching process that groups candidates into family-like structures.

Use conservative labels such as:

- `FamilyLikeCluster-001`,
- `GenerationCandidate-A`,
- `ConjugationPair-B`,
- `MixedChiralitySector-C`.

Only later may those be mapped to physical names through evidence gates.

## 4.11 Unified candidate particle object

Define:

```text
CandidateParticle = {
  CandidateId,
  ParticleType,              # Boson | Fermion | CompositeCandidate | InteractionCandidate
  BackgroundId,
  BranchVariantId,
  ModeId,
  SpectralData,
  ChiralityData,
  ConjugationData,
  SymmetryData,
  CouplingData,
  ObservationData,
  ComparisonData,
  StabilityData,
  ClaimClass,
  Demotions,
  Ambiguities,
  Provenance
}
```

This is the main serialized Phase IV object.

---

# 5. What Phase IV should add to the existing architecture

The project should be treated as an existing research engine with CPU, CUDA, Vulkan, branch-family, stability, recovery, and comparison infrastructure already in place.
Phase IV should therefore be implemented as **new modules on top of that architecture**, not by rewriting the existing core.

## 5.1 New C# projects / modules

Claude should add modules with names approximately like:

```text
src/
  Gu.Phase4.Spin/
  Gu.Phase4.Fermions/
  Gu.Phase4.Dirac/
  Gu.Phase4.Chirality/
  Gu.Phase4.Couplings/
  Gu.Phase4.FamilyClustering/
  Gu.Phase4.Registry/
  Gu.Phase4.Observation/
  Gu.Phase4.Comparison/
  Gu.Phase4.Reporting/
```

### Module responsibilities

#### `Gu.Phase4.Spin`

- Clifford / gamma conventions
- spinor representation specs
- conjugation conventions
- basis transforms
- validation helpers

#### `Gu.Phase4.Fermions`

- fermion field layouts
- discrete fermion state containers
- dual blocks
- bilinear evaluators
- serialization

#### `Gu.Phase4.Dirac`

- spin connection builders
- Dirac operator assembly
- reduced operator assembly
- fermionic spectral solvers
- CPU reference implementation

#### `Gu.Phase4.Chirality`

- chirality operators
- projectors
- leakage diagnostics
- conjugation pairing
- parity / handedness reports

#### `Gu.Phase4.Couplings`

- boson–fermion coupling proxies
- derivative-of-operator machinery
- mode-to-mode coupling records
- normalization and symmetry metadata

#### `Gu.Phase4.FamilyClustering`

- generation-like clustering
- conjugation-pair discovery
- spectral family grouping
- ambiguity scoring

#### `Gu.Phase4.Registry`

- unified particle registry
- particle claim records
- demotion engine
- provenance aggregation

#### `Gu.Phase4.Observation`

- fermion signature extraction to `X_h`
- observed chirality summaries
- observed interaction summaries

#### `Gu.Phase4.Comparison`

- particle-target comparison
- signature matching policies
- uncertainty notes
- campaign extensions for fermions and couplings

#### `Gu.Phase4.Reporting`

- family atlas reports
- coupling atlas reports
- unified candidate registry reports
- negative-result dashboards

## 5.2 Native / CUDA additions

Claude should extend the native layer only after CPU reference paths exist.
Add native / CUDA support for:

- gamma action kernels,
- spin-covariant derivative assembly,
- Dirac operator application,
- fermionic residual evaluation if supported,
- eigen / Arnoldi / Lanczos support if native backend already has compatible building blocks,
- coupling-proxy matrix accumulation,
- chirality projection diagnostics.

Do not move all spectral logic to CUDA immediately.
Start with:

1. CPU reference path,
2. CUDA operator application,
3. CUDA reductions,
4. optional iterative eigensolver acceleration,
5. parity tests,
6. only then full production use.

## 5.3 Artifact contract additions

Extend the canonical run folder with Phase IV directories:

```text
run/
  fermions/
    fermion_layout.json
    spinor_representation.json
    dirac_operator_bundle.json
    fermion_backgrounds/
    fermion_modes/
    chirality/
    conjugation/
    couplings/
  particle_registry/
    candidate_fermions.json
    candidate_interactions.json
    unified_particle_registry.json
    family_clusters.json
  reports/
    fermion_family_atlas.md
    coupling_atlas.md
    unified_particle_report.md
```

All new files must participate in replay, integrity, and schema validation.

---

# 6. Detailed mathematical-to-computational lowering

Claude needs unambiguous discrete targets.

## 6.1 Discrete spinor field representation

A spinor field on `Y_h` should lower to a typed block vector:

```text
psi_h = [psi_cell_1, psi_cell_2, ..., psi_cell_N]
```

where each local block carries:

- local basis index,
- spinor component index,
- optional chirality split,
- optional internal symmetry index,
- numeric field type (real / complex).

Use a structure-of-arrays or block-CSR-compatible layout for performance.
C# owns the semantic layout metadata; native code owns optimized packed buffers.

## 6.2 Discrete gamma operator lowering

Each gamma operator should lower to one of:

- local dense block action,
- sparse block operator,
- tensor-product representation,
- precomputed basis action table.

Prefer:

```text
GammaOperatorBundle = {
  ConventionId,
  MetricSignatureId,
  BlockMatrices,
  CellwiseTransforms,
  ValidationResiduals,
  Provenance
}
```

## 6.3 Discrete spin connection lowering

Lower the branch-defined spin connection to:

```text
SpinConnectionBundle = {
  BackgroundId,
  BranchVariantId,
  ConnectionCoefficients,
  CellwiseForms,
  AssemblyMetadata,
  Provenance
}
```

This bundle must be derivable from a bosonic background artifact.

## 6.4 Dirac operator lowering

Lower the discrete Dirac operator to:

```text
DiracOperatorBundle = {
  LayoutId,
  MatrixShape,
  BlockPattern,
  ApplyOnlyOperator,
  OptionalExplicitMatrix,
  InnerProductBundle,
  GaugeReductionBundle,
  Provenance
}
```

Support both:

- matrix-free apply,
- explicit assembled sparse matrix when size permits.

## 6.5 Gauge / redundancy reduction

Some fermionic directions may be unphysical or redundant depending on branch conventions and gauge structure.

Implement:

```text
PhysicalFermionReduction = {
  NullspaceProjector,
  GaugeLeakDiagnostics,
  ConstraintProjector,
  ReducedOperator,
  ReducedInnerProduct,
  Provenance
}
```

This reduction step is mandatory before strong spectral claims.

## 6.6 Coupling derivative lowering

For each bosonic mode `b_k`, implement the derivative operator:

```text
dD_dz[b_k]
```

as a first-class operator object.

Serialize:

```text
DiracVariationBundle = {
  BosonModeId,
  VariationOperator,
  Normalization,
  SymmetryNotes,
  Provenance
}
```

---

# 7. Data types Claude must implement

Below is the minimum type inventory.

## 7.1 Spinor representation types

```text
SpinorRepresentationSpec
GammaConventionSpec
ChiralityConventionSpec
ConjugationConventionSpec
CliffordValidationResult
```

## 7.2 Fermionic state types

```text
FermionFieldLayout
DiscreteFermionState
DiscreteDualFermionState
FermionBackgroundRecord
FermionModeRecord
FermionModeFamily
```

## 7.3 Operator types

```text
GammaOperatorBundle
SpinConnectionBundle
DiracOperatorBundle
ReducedDiracOperatorBundle
FermionResidualBundle
DiracVariationBundle
```

## 7.4 Diagnostic types

```text
ChiralityDecomposition
ConjugationPairRecord
GaugeLeakSummary
FermionStabilityRecord
FermionObservationSummary
```

## 7.5 Coupling / clustering / registry types

```text
BosonFermionCouplingRecord
CouplingAtlas
FamilyClusterRecord
UnifiedParticleRecord
UnifiedParticleRegistry
ParticleClaimRecord
ParticleClaimDemotion
```

All of these need JSON schemas and binary / sparse storage where appropriate.

---

# 8. Algorithms Claude must implement

## 8.1 Background selection algorithm

Phase IV should not search fermionic modes in arbitrary backgrounds.
It should consume the best backgrounds from Phase III.

Algorithm:

1. query Phase III background atlas,
2. filter to branch-stable or at least branch-explicit bosonic backgrounds,
3. require replay tier >= R2,
4. require acceptable residual and stationarity diagnostics,
5. attach background provenance to all downstream fermionic work.

Output:

```text
SelectedBackgroundSet
```

## 8.2 Dirac assembly algorithm

For each selected background:

1. resolve spinor representation spec,
2. assemble local gamma actions,
3. assemble spin connection from bosonic background,
4. assemble covariant derivative blocks,
5. add manifest-declared correction / mass-like / torsion-dependent blocks,
6. validate operator structure,
7. serialize `DiracOperatorBundle`.

## 8.3 Fermionic spectral solve algorithm

For each background and branch variant:

1. reduce operator using gauge / redundancy projector,
2. choose spectral target region,
3. solve generalized eigenproblem or singular problem,
4. compute left / right / mixed chirality components,
5. compute conjugation-pair relations,
6. record refinement behavior,
7. record branch sensitivity,
8. serialize `FermionModeRecord` entries.

## 8.4 Mode tracking across branches and refinement

This mirrors Phase III boson tracking but adds chirality and conjugation invariants.

Use a matching score combining:

- eigenvalue band similarity,
- eigenspace overlap,
- chirality profile similarity,
- conjugation relation consistency,
- observed-signature similarity,
- coupling-profile similarity.

Outputs:

```text
FermionModeFamily
BranchPersistenceScore
RefinementPersistenceScore
AmbiguityNotes
```

## 8.5 Boson–fermion coupling extraction algorithm

For each bosonic mode `b_k` and fermionic modes `phi_i`, `phi_j`:

1. compute or approximate `delta D_h[b_k]`,
2. evaluate coupling pairings,
3. normalize by chosen conventions,
4. classify symmetry / selection-rule hints,
5. record branch and refinement stability,
6. add to `BosonFermionCouplingRecord`.

## 8.6 Family clustering algorithm

Input candidate fermions and couplings.

Construct feature vectors from:

- mass-like invariants,
- chirality summaries,
- conjugation-pair patterns,
- branch stability,
- observed signatures,
- boson coupling profiles.

Perform clustering with deterministic reproducible algorithms first.
Suggested order:

1. rule-based grouping by conjugation and chirality,
2. stable hierarchical clustering,
3. optional spectral embedding for visualization only,
4. ambiguity scoring for unstable assignments.

Output conservative family labels, not physical names.

## 8.7 Unified registry construction algorithm

Merge:

- candidate boson registry,
- candidate fermion registry,
- coupling atlas,
- observation summaries,
- comparison summaries.

For each merged candidate record:

1. aggregate provenance,
2. aggregate branch stability,
3. aggregate observation confidence,
4. aggregate comparison evidence,
5. assign claim class,
6. attach demotions and ambiguity reasons.

---

# 9. CLI, configs, and schemas Claude must add

## 9.1 New CLI commands

Add commands approximately like:

```text
gu-cli build-spin-spec
gu-cli assemble-dirac
gu-cli solve-fermion-modes
gu-cli analyze-chirality
gu-cli analyze-conjugation
gu-cli extract-couplings
gu-cli build-family-clusters
gu-cli build-unified-registry
gu-cli report-phase4
```

Concrete `dotnet run` examples should eventually look like:

```bash
# Build spinor representation spec

dotnet run --project apps/Gu.Cli -- build-spin-spec runs/bg-001 phase4/spinor_spec.json

# Assemble Dirac operator from a bosonic background

dotnet run --project apps/Gu.Cli -- assemble-dirac runs/bg-001 --spin-spec phase4/spinor_spec.json --out runs/bg-001/fermions/dirac_operator_bundle.json

# Solve fermion modes around that background

dotnet run --project apps/Gu.Cli -- solve-fermion-modes runs/bg-001 --dirac runs/bg-001/fermions/dirac_operator_bundle.json --target lowest --count 64

# Analyze chirality

dotnet run --project apps/Gu.Cli -- analyze-chirality runs/bg-001 --modes runs/bg-001/fermions/fermion_modes/modes.json

# Extract couplings to bosonic modes from Phase III

dotnet run --project apps/Gu.Cli -- extract-couplings runs/bg-001 --boson-registry runs/bg-001/particle_registry/candidate_bosons.json --fermion-modes runs/bg-001/fermions/fermion_modes/modes.json

# Build family clusters

dotnet run --project apps/Gu.Cli -- build-family-clusters runs/bg-001

# Build unified particle registry

dotnet run --project apps/Gu.Cli -- build-unified-registry runs/bg-001

# Emit final report

dotnet run --project apps/Gu.Cli -- report-phase4 runs/bg-001
```

## 9.2 Config files to add

At minimum define:

```text
phase4/
  spinor_spec.json
  chirality_policy.json
  conjugation_policy.json
  fermion_mode_study.json
  coupling_campaign.json
  family_clustering_policy.json
  particle_identification_policy.json
```

### Example `spinor_spec.json`

```json
{
  "spinorSpecId": "spinor-complex-v1",
  "schemaVersion": "1.0.0",
  "cliffordSignatureId": "lorentz-like-v1",
  "gammaBasisConventionId": "gamma-basis-standard-v1",
  "chiralityConventionId": "chirality-standard-v1",
  "conjugationConventionId": "dirac-adjoint-v1",
  "innerProductConventionId": "fermion-innerproduct-v1",
  "numericField": "complex64",
  "blockDimensions": {
    "spinorComponents": 4,
    "chiralitySplit": 2
  },
  "insertedAssumptionIds": ["P4-IA-001", "P4-IA-002"],
  "provenance": {
    "phase": "IV"
  }
}
```

### Example `fermion_mode_study.json`

```json
{
  "studyId": "fermion-modes-bg-001",
  "backgroundSelector": {
    "source": "phase3-background-atlas",
    "minimumReplayTier": "R2"
  },
  "spectralTarget": {
    "region": "lowest-magnitude",
    "count": 64,
    "solver": "Arnoldi"
  },
  "reductionPolicy": {
    "gaugeReduction": true,
    "nullspaceDeflation": true
  },
  "stabilityPolicy": {
    "requireRefinementCheck": true,
    "requireBranchSweep": true
  }
}
```

## 9.3 Schemas to add

Add schemas approximately like:

```text
schemas/phase4/
  spinor_representation.schema.json
  chirality_policy.schema.json
  conjugation_policy.schema.json
  dirac_operator_bundle.schema.json
  fermion_mode.schema.json
  fermion_mode_family.schema.json
  boson_fermion_coupling.schema.json
  family_cluster.schema.json
  unified_particle_registry.schema.json
  phase4_report.schema.json
```

All must integrate with existing schema validation and replay contracts.

---

# 10. Detailed implementation milestones

The repo documentation currently treats the earlier implementation layers as the formal foundation, so this Phase IV plan should be implemented as a **new continuation layer** following the in-progress Phase III boson work rather than as a rewrite of the earlier engine.

Use milestone numbering after Phase III.

## M33 — Spinor convention and validation substrate

Deliver:

- `SpinorRepresentationSpec`
- `GammaConventionSpec`
- `ChiralityConventionSpec`
- `ConjugationConventionSpec`
- CPU validation suite for gamma algebra and conjugation compatibility

Completion criteria:

- gamma operators assemble and validate on toy cases,
- conventions serialize cleanly,
- parity tests exist for core algebraic actions.

## M34 — Fermionic field layout and storage

Deliver:

- `FermionFieldLayout`
- `DiscreteFermionState`
- binary storage
- JSON metadata
- interop-friendly packed layout

Completion criteria:

- fermionic states can be created, serialized, loaded, and replayed.

## M35 — Spin connection builder

Deliver:

- `Mu_spin` branch interface
- spin connection builder from bosonic background artifacts
- validation and provenance

Completion criteria:

- selected bosonic backgrounds yield reproducible `SpinConnectionBundle` artifacts.

## M36 — Dirac operator CPU reference

Deliver:

- `DiracOperatorBundle`
- matrix-free apply
- optional explicit assembly for small systems
- reduction hooks

Completion criteria:

- toy examples run end-to-end,
- operator validation and parity tests pass.

## M37 — Chirality and conjugation analysis

Deliver:

- chirality projectors
- leakage diagnostics
- conjugation operators
- pair matching logic

Completion criteria:

- fermion modes can be decomposed and paired on at least one toy and one structured background.

## M38 — Fermionic spectral solver

Deliver:

- generalized eigen solve / singular solve pipeline
- mode records
- refinement diagnostics
- branch diagnostics

Completion criteria:

- candidate fermion modes can be computed on selected backgrounds and serialized.

## M39 — Fermion mode tracking and persistence atlas

Deliver:

- tracking across branch variants,
- tracking across refinement,
- persistence scores,
- ambiguity records,
- first `fermion_family_atlas.md` report.

Completion criteria:

- at least one atlas exists for a nontrivial branch family.

## M40 — Boson–fermion coupling proxy engine

Deliver:

- `DiracVariationBundle`
- `BosonFermionCouplingRecord`
- normalization policies
- symmetry notes
- coupling atlas report

Completion criteria:

- at least one boson family and one fermion family have a serialized coupling study.

## M41 — Generation / family clustering

Deliver:

- `FamilyClusterRecord`
- deterministic clustering pipeline
- ambiguity scoring
- family report

Completion criteria:

- candidate family-like clusters are emitted with stable IDs and provenance.

## M42 — Unified particle registry

Deliver:

- `UnifiedParticleRegistry`
- candidate fermion / boson / interaction merges
- claim demotion logic
- provenance aggregator

Completion criteria:

- a unified registry is emitted for at least one run and passes schema validation.

## M43 — Observation and comparison extension

Deliver:

- fermionic observation summaries,
- interaction observation summaries,
- comparison adapters for candidate fermions and couplings,
- campaign and report extensions.

Completion criteria:

- at least one comparison campaign involving fermionic candidates completes.

## M44 — CUDA acceleration and parity closure

Deliver:

- CUDA gamma actions,
- CUDA Dirac apply,
- CUDA reductions,
- optional eigensolver acceleration,
- parity tests,
- benchmark extensions.

Completion criteria:

- CPU / CUDA parity is established for key operator paths and diagnostics.

## M45 — End-to-end Phase IV reference study

Deliver one canonical study that:

1. selects a bosonic background family,
2. builds Dirac operators,
3. solves fermionic modes,
4. analyzes chirality and conjugation,
5. computes couplings to bosonic modes,
6. builds family clusters,
7. emits unified particle registry,
8. runs comparison campaign,
9. generates reports,
10. preserves negative results.

Completion criteria:

- the codebase contains a reproducible example demonstrating the full Phase IV pipeline.

---

# 11. Testing strategy Claude must follow

## 11.1 Unit tests

Add tests for:

- gamma algebra identities,
- chirality projectors,
- conjugation maps,
- bilinear symmetry properties,
- spin connection assembly,
- Dirac operator apply,
- coupling proxy evaluation,
- clustering determinism,
- registry merge logic,
- schema validation.

## 11.2 Integration tests

Add end-to-end tests that:

- consume a bosonic background from Phase III,
- assemble the fermionic stack,
- solve small spectral problems,
- produce chirality summaries,
- produce at least one coupling record,
- build a minimal unified registry.

## 11.3 Replay tests

All new artifacts must satisfy at least:

- R0 schema checks,
- R1 structural replay,
- R2 numerical replay for CPU,
- R3 only where bit-exact parity is actually achievable.

## 11.4 CPU / CUDA parity tests

Before any strong claim on GPU outputs:

- compare Dirac operator apply,
- compare chirality diagnostics,
- compare coupling accumulation,
- compare selected eigenpairs or invariant summaries.

---

# 12. First reference study Claude should implement

Phase IV needs a concrete anchor study.

## Study name

`Phase4-FermionFamily-Atlas-001`

## Inputs

- one Phase III bosonic background family,
- one spinor representation convention,
- one chirality convention,
- one conjugation convention,
- one branch family for fermionic operator variants,
- one refinement ladder,
- one coupling campaign against a selected boson family.

## Workflow

1. select 3 to 5 bosonic backgrounds from Phase III,
2. assemble Dirac operators for each,
3. solve the lowest 32 or 64 fermionic modes,
4. compute chirality and conjugation diagnostics,
5. track modes across branch variants and refinement,
6. compute couplings to the strongest Phase III boson candidates,
7. cluster into family-like groups,
8. build unified registry,
9. emit report and atlas.

## Expected outputs

- `fermion_family_atlas.md`
- `coupling_atlas.md`
- `unified_particle_registry.json`
- `phase4_report.md`
- reproducible run artifacts

## Why this is the right first study

It produces the first disciplined answer to:

- whether candidate fermionic sectors are stable,
- whether chirality survives branch choice,
- whether boson–fermion interaction structure is persistent,
- whether family-like structures appear at all,
- whether the unified particle story becomes more or less canonical after adding fermions.

---

# 13. Engineering rules Claude must follow

## 13.1 Never hide ambiguity

If spinor conventions, chirality assignments, conjugation maps, or family clustering are ambiguous, serialize the ambiguity.
Do not choose silently.

## 13.2 CPU reference before CUDA trust

All GPU logic must have a CPU reference path and parity tests.

## 13.3 No direct Y-space physical comparison

Physical comparison must go through the declared observation / recovery chain.

## 13.4 Preserve negative results

Failed fermion solves, unstable chirality assignments, fragile couplings, and broken family clusters are first-class outputs.

## 13.5 Every strong claim needs provenance

Any mass-like scale, chirality tag, family label, or candidate identification must include:

- background provenance,
- branch provenance,
- spinor convention provenance,
- operator provenance,
- observation provenance,
- replay tier,
- uncertainty notes.

## 13.6 Do not over-interpret spectral quantities

Until a stronger physical dictionary exists, treat spectral invariants as candidate mass-like or family-like features, not automatically physical measured particle properties.

## 13.7 Coupling proxies are not yet scattering amplitudes

Do not label coupling records as final physical interaction strengths.
They are structured interaction proxies.

---

# 14. What remains intentionally out of scope after Phase IV

Claude must leave these out of scope unless explicitly asked in a later phase.

## 14.1 Full quantum completion

No path integral, canonical quantization, BRST / BV completion, operator algebra quantization, loop corrections, or renormalization program is required here.

## 14.2 Full scattering / S-matrix program

Coupling proxies are included, but full on-shell scattering amplitudes and cross sections are out of scope.

## 14.3 Exact measured particle dictionary

Phase IV may produce strong candidate boson / fermion / coupling structures.
It does not complete a unique exact final dictionary of known particles.

## 14.4 Neutrino-oscillation-quality flavor program

Family clustering is included, but a fully realistic flavor / mixing matrix prediction program remains for a later phase.

## 14.5 Full anomaly / consistency closure

Partial consistency checks are allowed.
A complete anomaly-cancellation or quantum consistency program is out of scope.

## 14.6 Cosmological or large-scale physical environment campaigns

Phase IV should begin with toy, structured, and controlled environments.
Large-scale or highly realistic campaigns belong later.

## 14.7 Final symbolic theorem engine

Symbolic assistance is allowed for validation and operator generation.
A full theorem prover for the theory is out of scope.

---

# 15. Suggested deliverables Claude should leave at the end of Phase IV

By the end of this phase, Claude should leave the codebase with:

1. a working spinor convention and validation substrate,
2. a working fermionic field layout and Dirac assembly path,
3. chirality and conjugation diagnostics,
4. a fermionic spectral solver stack,
5. boson–fermion coupling extraction,
6. generation / family clustering,
7. a unified particle registry,
8. observation and comparison extensions for fermions and couplings,
9. CPU / CUDA parity for key operator paths,
10. at least one end-to-end Phase IV reference study,
11. updated schemas, CLI commands, tests, benchmarks, and reports,
12. a clear list of unresolved issues for the next phase.

If these are complete, the platform will have moved from:

> “candidate boson discovery and classification engine”

into:

> “candidate unified particle-sector extraction engine with explicit fermionic structure and interaction proxies.”

That is the correct Phase IV target.

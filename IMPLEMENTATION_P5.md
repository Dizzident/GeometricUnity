# IMPLEMENTATION_P5.md

# Geometric Unity Phase V Branch-Independence, High-Fidelity Environment, and Quantitative Validation Implementation Plan

## Purpose

This document is a **standalone Phase V implementation handoff** for Claude Code.

It assumes:

- Phase I is complete: minimal bosonic executable branch, CPU reference backend,
  CUDA/backend parity scaffolding, observation pipeline, artifacts, replay,
  validation.
- Phase II is complete: branch families, canonicity studies, continuation,
  recovery graph, comparison campaigns, reporting.
- Phase III is complete: background atlas, bosonic spectra, mode tracking,
  property extraction, candidate boson registry.
- Phase IV is complete: fermionic sector, unified particle registry, and the
  corrective prerequisites added to `IMPLEMENTATION_PLAN_P4.md`.

It also assumes the current branch-local decisions are recorded in
`ASSUMPTIONS.md`.

Phase V is the first phase whose primary job is not to add another major field
sector.

Its primary job is to answer:

> How much of the currently executable Geometric Unity branch survives when we
> reduce hardcoded toy assumptions, compare admissible branches systematically,
> move toward higher-fidelity environments, and confront the resulting outputs
> quantitatively with external targets?

Phase V therefore moves the program from:

> "an executable branch with bosonic and fermionic machinery"

to:

> "a branch-audited, refinement-aware, environment-aware, quantitatively tested
> research platform with explicit falsification pathways."

This phase is still **not** a proof that the original theory is correct.
It is the phase that makes stronger theory-testing possible.

---

# 1. What Phase V is and is not

## 1.1 What Phase V is

Phase V is the implementation of the **validation-strengthening layer**.

It must provide:

1. a branch-independence and branch-fragility measurement framework,
2. a continuum/refinement convergence framework,
3. a high-fidelity environment ingestion and campaign framework,
4. a quantitative observable extraction and calibration layer,
5. a registry-grade falsification engine,
6. a negative-result preservation and comparison framework,
7. a claim-escalation and claim-demotion system tied to actual evidence,
8. reproducible validation dossiers for each serious study.

Phase V is the first phase that may legitimately try to promote some branch-local
results from:

- executable,
- internally consistent,
- numerically stable on toy systems

toward:

- branch-robust,
- refinement-aware,
- quantitatively compared,
- or explicitly falsified.

## 1.2 What Phase V is not

Phase V is **not**:

- a proof of Geometric Unity,
- a final canonical choice of all branches,
- a complete symbolic theorem engine,
- a quantum completion,
- a scattering or renormalization program,
- a guarantee that realistic environments will validate the branch,
- permission to overstate low-signal matches as particle discoveries.

Phase V may produce:

- branch-robust or branch-fragile statements,
- convergence evidence or convergence failure,
- quantitatively admissible or quantitatively ruled-out sectors,
- stronger candidate identifications,
- stronger negative results.

Phase V may **not** silently convert these into global theory truth.

---

# 2. Core thesis of Phase V

The executable branch already supports:

- branch manifests,
- typed artifacts,
- residual/Jacobian/Hessian machinery,
- bosonic and fermionic mode extraction,
- observation/recovery pipelines,
- candidate registries,
- comparison campaigns.

But these are still limited by:

- toy geometry defaults,
- branch-selected operator choices,
- weak branch-independence evidence,
- limited refinement evidence,
- low-fidelity environments,
- weak quantitative confrontation with external data.

Phase V exists to reduce those limitations systematically.

The central discipline is:

> No claim may be promoted because the code merely runs.
> Claims may be promoted only when they survive declared branch variation,
> refinement checks, environment changes, observation-chain validation, and
> quantitative comparison rules.

---

# 3. Main research targets

Phase V should compute and serialize the following objects:

1. branch-independence studies,
2. convergence studies across refinement and discretization variants,
3. environment campaign results across toy, structured, and imported
   high-fidelity backgrounds,
4. quantitative observable tables with uncertainties and provenance,
5. falsification records,
6. claim-escalation records,
7. validation dossiers per study,
8. a Phase V evidence-ranked registry overlay for the unified particle registry.

The unifying Phase V product is:

```text
ValidationDossier = {
  StudyManifest,
  BranchFamilySummary,
  RefinementSummary,
  EnvironmentSummary,
  ObservationChainSummary,
  QuantitativeComparisonSummary,
  FalsifierSummary,
  ClaimEscalations,
  ClaimDemotions,
  NegativeResults,
  ReproducibilityBundle
}
```

---

# 4. Mathematical and computational framework

## 4.1 Branch-independence is now a first-class object

Phase V must treat branch dependence quantitatively rather than rhetorically.

For every important output object `Q`, define:

```text
BranchRobustness(Q) = summary over admissible branch family variations
```

Minimum required tracked objects:

- bosonic residual observables,
- bosonic spectral quantities,
- fermionic spectral quantities,
- coupling proxies,
- observed signatures,
- unified registry candidate properties.

Required outputs:

- pairwise branch-distance matrices,
- equivalence classes under declared tolerances,
- fragility scores,
- invariance candidates,
- explicit ambiguity records.

No quantity is allowed to be called "physical" if it is only stable on a single
operator branch and unstable on the admissible family.

## 4.2 Refinement and continuum evidence must be explicit

Phase V must implement refinement studies that go beyond simple "two meshes look
similar."

Required minimum tools:

- mesh refinement sweep runner,
- basis/quadrature variant sweep runner where applicable,
- Richardson extrapolation,
- Cauchy-style convergence diagnostics,
- failure classification for non-convergent sequences.

For any quantity `Q_h`, Phase V should compute:

```text
ContinuumEstimate(Q) = extrapolated limit estimate + error band + confidence note
```

If convergence is absent, that is a result and must be recorded.

## 4.3 Environment realism becomes a controlled ladder

Phase V should not jump from toy meshes straight to grand physical claims.

It must define an environment ladder:

1. toy environments,
2. structured analytic environments,
3. imported high-fidelity environments,
4. campaign-scale environment families.

Each environment level must preserve:

- provenance,
- geometry metadata,
- branch compatibility,
- replayability,
- admissibility grading.

Imported environments may come from:

- externally generated mesh/geometry data,
- GR initial-data style background datasets,
- curated benchmark environments,
- physically motivated gauge/background templates.

## 4.4 Observation-chain validity must be testable

Phase V must validate not only native `Y_h` calculations but also the full path:

```text
native state -> derived state -> observed state -> extracted observable -> comparison object
```

Every major claim must identify where in that chain uncertainty or failure enters.

Phase V must provide:

- observation-chain verification artifacts,
- extraction sensitivity reports,
- auxiliary-model sensitivity reports,
- invalid-comparison flags when observation is too branch-sensitive.

## 4.5 Quantitative comparison becomes a primary engine

Phase V must add a stronger quantitative comparison layer over Phase II/IV
campaigns.

This layer should support:

- tolerance policies with explicit decomposition,
- calibration records,
- uncertainty propagation,
- confidence scoring,
- structural falsifiers before numerical fit,
- multi-target scoring,
- cross-study consistency checks.

Quantitative comparison must always preserve:

- branch identity,
- refinement level,
- environment identity,
- backend identity,
- extraction policy,
- auxiliary-model identity.

## 4.6 Falsification is a first-class outcome

Phase V is successful even if it falsifies major executable branches.

It must provide explicit falsifier classes such as:

- branch-fragility falsifier,
- non-convergence falsifier,
- observation-instability falsifier,
- environment-instability falsifier,
- quantitative mismatch falsifier,
- representation-content falsifier,
- coupling inconsistency falsifier.

These falsifiers must be serializable and reportable.

## 4.7 Claim escalation must be rule-based

Phase V may promote a candidate only if declared evidence gates are passed.

Example escalation gates:

1. survives admissible branch variations,
2. survives refinement with bounded uncertainty,
3. survives more than one environment family,
4. passes observation-chain validity checks,
5. passes quantitative comparison within declared tolerances,
6. has no active high-severity falsifier.

If any gate fails, the candidate is demoted or held.

---

# 5. Architecture additions

Phase V should extend the existing architecture with new modules rather than
rewriting prior phases.

## 5.1 New C# projects/modules

Recommended additions:

```text
src/
  Gu.Phase5.BranchIndependence/
  Gu.Phase5.Convergence/
  Gu.Phase5.Environments/
  Gu.Phase5.QuantitativeValidation/
  Gu.Phase5.Falsification/
  Gu.Phase5.Dossiers/
  Gu.Phase5.Reporting/

tests/
  Gu.Phase5.BranchIndependence.Tests/
  Gu.Phase5.Convergence.Tests/
  Gu.Phase5.Environments.Tests/
  Gu.Phase5.QuantitativeValidation.Tests/
  Gu.Phase5.Falsification.Tests/
  Gu.Phase5.Dossiers.Tests/
```

### `Gu.Phase5.BranchIndependence`

Responsibilities:

- branch sweep normalization,
- pairwise distance matrices,
- invariance and fragility scoring,
- branch-family summaries,
- equivalence clustering.

### `Gu.Phase5.Convergence`

Responsibilities:

- refinement campaign runner,
- discretization-variant runner,
- Richardson extrapolation,
- continuum estimate artifacts,
- convergence failure diagnostics.

### `Gu.Phase5.Environments`

Responsibilities:

- environment import,
- structured environment generators,
- environment admissibility grading,
- environment ladder policies,
- campaign orchestration over environment sets.

### `Gu.Phase5.QuantitativeValidation`

Responsibilities:

- observable normalization,
- uncertainty propagation,
- calibration policies,
- target matching,
- consistency scoring,
- claim escalation inputs.

### `Gu.Phase5.Falsification`

Responsibilities:

- typed falsifier records,
- evaluation rules,
- severity ranking,
- registry demotions.

### `Gu.Phase5.Dossiers`

Responsibilities:

- study manifests,
- dossier assembly,
- reproducibility bundle integration,
- artifact linking.

### `Gu.Phase5.Reporting`

Responsibilities:

- validation reports,
- negative-result summaries,
- branch-independence atlases,
- convergence atlases,
- falsification dashboards.

## 5.2 Artifact contract additions

Extend the canonical run folder with Phase V directories:

```text
run/
  phase5/
    branch_independence/
    convergence/
    environments/
    quantitative_validation/
    falsifiers/
    dossiers/
    reports/
```

Each major Phase V study must be reproducible from artifacts alone.

---

# 6. Data types Claude must implement

## 6.1 Branch-independence types

Required types:

```text
BranchRobustnessStudySpec
BranchRobustnessRecord
BranchDistanceMatrix
BranchEquivalenceClass
FragilityRecord
InvarianceCandidateRecord
```

## 6.2 Convergence types

Required types:

```text
RefinementStudySpec
RefinementRunRecord
ContinuumEstimateRecord
RichardsonFitRecord
ConvergenceFailureRecord
```

## 6.3 Environment types

Required types:

```text
EnvironmentImportSpec
StructuredEnvironmentSpec
EnvironmentCampaignSpec
EnvironmentRecord
EnvironmentAdmissibilityReport
```

## 6.4 Quantitative validation types

Required types:

```text
QuantitativeObservableRecord
CalibrationPolicy
UncertaintyPropagationRecord
TargetMatchRecord
ConsistencyScoreCard
ClaimEscalationRecord
```

## 6.5 Falsification and dossier types

Required types:

```text
FalsifierRecord
FalsifierSummary
ValidationDossier
DossierIndex
NegativeResultLedger
```

---

# 7. Algorithms Claude must implement

## 7.1 Branch-robustness study algorithm

For each study object:

1. select a branch family,
2. select one or more environments,
3. run the target bosonic/fermionic/coupling pipeline for each branch,
4. align outputs under the declared comparison relation,
5. compute pairwise distances,
6. compute invariance/fragility summaries,
7. emit branch-local and family-level artifacts.

## 7.2 Refinement/continuum study algorithm

For each target quantity:

1. generate or load refinement ladder,
2. recompute quantity on each level,
3. compute successive deltas,
4. attempt extrapolation,
5. compute uncertainty/error band,
6. classify as convergent, weakly convergent, or non-convergent,
7. preserve failures.

## 7.3 Environment campaign algorithm

1. select environment ladder tier,
2. validate import/generation metadata,
3. run admissibility checks,
4. execute branch family and refinement studies as required,
5. aggregate by environment family,
6. detect environment-sensitive instability.

## 7.4 Quantitative validation algorithm

1. select comparison targets,
2. map observed outputs to target quantities,
3. propagate uncertainty,
4. apply structural falsifiers first,
5. compute quantitative mismatch metrics,
6. emit target-match and consistency records,
7. drive escalation/demotion.

## 7.5 Dossier assembly algorithm

1. gather all study artifacts,
2. verify branch/refinement/environment identity consistency,
3. link replay contracts,
4. link negative results,
5. summarize promotion/demotion reasoning,
6. emit a single dossier artifact and markdown report.

---

# 8. CLI, configs, and schemas Claude must add

## 8.1 New CLI commands

Suggested commands:

```bash
# Run branch-independence study
dotnet run --project apps/Gu.Cli -- branch-robustness <study.json>

# Run refinement/continuum study
dotnet run --project apps/Gu.Cli -- refinement-study <study.json>

# Import or generate environments
dotnet run --project apps/Gu.Cli -- import-environment <spec.json>
dotnet run --project apps/Gu.Cli -- build-structured-environment <spec.json>

# Run quantitative validation
dotnet run --project apps/Gu.Cli -- validate-quantitative <study.json>

# Build validation dossier
dotnet run --project apps/Gu.Cli -- build-validation-dossier <run-folder>
```

## 8.2 Config files to add

```text
studies/
  branch_robustness_study.json
  refinement_study.json
  environment_campaign.json
  quantitative_validation_study.json
```

## 8.3 Schemas to add

```text
schemas/
  branch_robustness_study.schema.json
  branch_robustness_record.schema.json
  branch_distance_matrix.schema.json
  refinement_study.schema.json
  continuum_estimate.schema.json
  environment_campaign.schema.json
  environment_record.schema.json
  quantitative_validation.schema.json
  falsifier_record.schema.json
  validation_dossier.schema.json
```

---

# 9. Detailed implementation milestones

Use milestone numbering after Phase IV.

## M46 — Branch-independence substrate

Deliver:

- branch-robustness study spec,
- pairwise branch distance matrices,
- fragility records,
- invariance-candidate classification,
- initial reports.

Completion criteria:

- branch-family studies can be run on at least one bosonic and one fermionic
  target quantity.

## M47 — Refinement and continuum framework

Deliver:

- refinement study runner,
- continuum estimate records,
- Richardson extrapolation,
- non-convergence diagnostics.

Completion criteria:

- at least one spectral quantity and one observed quantity can be analyzed across
  a refinement ladder.

## M48 — Structured and imported environments

Deliver:

- structured environment generator,
- imported environment reader,
- environment admissibility grading,
- replayable environment metadata.

Completion criteria:

- at least one imported or structured non-toy environment runs through the
  admissibility pipeline.

## M49 — Quantitative validation engine

Deliver:

- calibrated quantitative comparison records,
- uncertainty propagation,
- consistency scorecards,
- target-match summaries.

Completion criteria:

- at least one registry candidate can be quantitatively compared against an
  external target with explicit uncertainty and demotion rules.

## M50 — Falsification engine

Deliver:

- typed falsifier classes,
- severity policy,
- registry demotion integration,
- falsification reports.

Completion criteria:

- at least one study emits an explicit falsifier artifact when a branch or
  quantity fails.

## M51 — Claim escalation / demotion framework

Deliver:

- escalation gates,
- promotion/demotion records,
- registry overlay updates,
- report integration.

Completion criteria:

- candidates cannot be promoted without passing declared gates.

## M52 — Validation dossier system

Deliver:

- dossier assembly,
- dossier schema,
- markdown and JSON outputs,
- replay linkage.

Completion criteria:

- one complete study can be summarized by a single dossier artifact.

## M53 — First high-value Phase V campaign

Deliver:

- one end-to-end study spanning branch family, refinement, environment, and
  quantitative validation,
- one positive or mixed result dossier,
- one negative-result dossier.

Completion criteria:

- the campaign produces registry-grade evidence rather than only intermediate
  diagnostics.

---

# 10. Testing strategy Claude must follow

## 10.1 Unit tests

Must cover:

- branch-distance math,
- fragility scoring,
- extrapolation logic,
- uncertainty propagation,
- falsifier classification,
- dossier assembly consistency.

## 10.2 Integration tests

Must cover:

- branch family -> comparison -> report,
- refinement ladder -> continuum estimate -> report,
- environment import -> admissibility -> pipeline execution,
- quantitative validation -> escalation/demotion.

## 10.3 Replay tests

Each major Phase V artifact must replay with the same:

- branch identity,
- environment identity,
- refinement identity,
- comparison policy identity.

## 10.4 Negative-result tests

Claude must add tests that prove:

- non-convergent studies remain serialized,
- falsified branches remain in the ledger,
- reports preserve failure reasoning.

---

# 11. First reference study Claude should implement

## Study name

`phase5_su2_branch_refinement_environment_validation`

## Inputs

- one bosonic branch family with at least one nontrivial torsion/Shiab variation,
- one fermionic extraction path from Phase IV,
- one toy environment,
- one structured environment,
- one refinement ladder,
- one small external target table for quantitative comparison.

## Workflow

1. run the same candidate extraction under multiple admissible branches,
2. run each branch across a refinement ladder,
3. run the surviving branch candidates on two environment tiers,
4. compute observed outputs and quantitative comparisons,
5. emit falsifiers, promotions, demotions, and final dossiers.

## Expected outputs

- branch robustness report,
- continuum estimate report,
- environment sensitivity report,
- quantitative validation report,
- one positive/mixed dossier,
- one negative dossier.

## Why this is the right first study

It forces Phase V to prove its value.

If the result is fragile, Phase V should expose that fragility cleanly.
If the result is robust, Phase V should be able to explain why with actual
evidence.

---

# 12. Engineering rules Claude must follow

## 12.1 Never hide branch fragility

If branch dependence is material, it must appear in artifacts and reports.

## 12.2 Never hide non-convergence

A failed continuum story is still a scientific result.

## 12.3 Never compare raw native quantities directly to external targets

All comparison objects must come through the declared observation and extraction
chain.

## 12.4 Preserve negative results as first-class artifacts

Phase V loses value if it only keeps good-looking runs.

## 12.5 Promotion must be rule-based, not narrative-based

Every promotion needs explicit gates and recorded evidence.

## 12.6 Update `ASSUMPTIONS.md` whenever a major branch assumption is removed or reduced

Phase V should shrink the assumptions ledger where possible, not let it drift out
of sync with the implementation.

---

# 13. What remains intentionally out of scope after Phase V

Even after successful Phase V completion, the following remain out of scope:

- full canonical theory proof,
- full symbolic theorem closure,
- quantum completion,
- scattering/S-matrix program,
- renormalization and loop corrections,
- final cosmological-scale campaign program,
- final unique Standard Model dictionary.

Phase V is the validation-strengthening phase, not the last phase.

---

# 14. Suggested deliverables Claude should leave at the end of Phase V

Claude should leave behind:

1. complete source modules and tests for M46-M53,
2. CLI commands and schemas,
3. at least one committed reference study,
4. at least one positive or mixed validation dossier,
5. at least one negative-result dossier,
6. updated documentation explaining what Phase V actually validated,
7. an updated `ASSUMPTIONS.md` reflecting any assumptions that were removed,
8. a short `PHASE_5_OPEN_ISSUES.md` describing what still blocks stronger
   theory-level claims.

The success criterion for Phase V is not "the theory is proven."

The success criterion is:

> the executable Geometric Unity branch can now be tested in a way that is
> branch-aware, refinement-aware, environment-aware, quantitatively disciplined,
> and scientifically falsifiable.

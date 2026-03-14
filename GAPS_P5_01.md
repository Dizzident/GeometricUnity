# GAPS_P5_01.md

## Purpose

This document proposes a concrete follow-up plan for unresolved Phase V work.
It is based on:

- `PHASE_5_OPEN_ISSUES.md`
- `IMPLEMENTATION_P5.md`
- `ARCH_P5.md`
- `ASSUMPTIONS.md`
- the current Phase V source tree under `src/Gu.Phase5.*`
- the checked-in reference study under `studies/phase5_su2_branch_refinement_env_validation/`

The goal is not to reopen Phase V broadly. The goal is to identify what still
blocks stronger Phase V evidence and to sequence that work in a way that
produces real upgrades rather than more placeholder infrastructure.

---

## Executive summary

The eight open issues in `PHASE_5_OPEN_ISSUES.md` are real, but they are not
the whole remaining gap surface.

Three additional implementation-level gaps should be tracked explicitly:

1. The checked-in M53 reference study is not yet a full M46-M53 end-to-end
   campaign. It currently runs environment generation, branch robustness,
   quantitative validation, and provenance dossiers, but not the full
   refinement/falsification/reporting path described in `IMPLEMENTATION_P5.md`.
2. Observation-chain validation is still under-modeled. The implementation plan
   calls for observation-chain verification artifacts, extraction sensitivity,
   and auxiliary-model sensitivity, but the current escalation gate is a simple
   `ObservationConfidence > 0` proxy.
3. The rich typed Phase V dossier (`Phase5ValidationDossier`) is not yet the
   primary output of the checked-in campaign path. The checked-in study and
   campaign runner still center the simpler provenance dossier
   (`ValidationDossier`), which means the technical evidence bundle is only
   partially integrated into the actual end-to-end path.

Recommended order:

1. Fix campaign integrity and output wiring first.
2. Add the missing evidence carriers needed to activate the deferred falsifiers.
3. Correct the convergence model (`h_X`, `h_F`) and Phase III bridge points.
4. Upgrade environment and target realism.
5. Expand the branch family only after the evidence path is complete.

---

## Gap inventory

| Gap ID | Source | Priority | Why it matters |
|---|---|---:|---|
| P5-G01 | Open issue 1 | P0 | Four falsifier types exist but are inactive, so Phase V can miss real failures. |
| P5-G02 | Open issue 2 | P1 | Convergence is computed with a single mesh parameter, not the architecture's `max(h_X, h_F)`. |
| P5-G03 | Open issue 3 | P2 | Quantitative validation still uses synthetic toy placeholders. |
| P5-G04 | Open issue 4 | P1 | "Imported" environments are not backed by an external data contract. |
| P5-G05 | Open issue 5 | P1 | Reference-study branch variants are still analytic `A0` stand-ins, not Phase III outputs. |
| P5-G06 | Open issue 6 | P2 | Shiab variation is effectively untested because the family is identity-only. |
| P5-G07 | Open issue 8 | P2 | Pull statistics assume Gaussian tails only. |
| P5-G08 | Implementation review | P0 | The checked-in M53 study is not wired as a full M46-M53 campaign and appears to reference CLI commands not wired in `apps/Gu.Cli`. |
| P5-G09 | Implementation review | P0 | Observation-chain evidence is reduced to a placeholder gate instead of explicit chain artifacts and sensitivity reports. |
| P5-G10 | Implementation review | P1 | The rich Phase V dossier/report path is not the default reproducible campaign output. |
| P5-G11 | Architecture review | P3 | Branch-family coverage remains narrow beyond the explicitly tracked Shiab limitation, including the known lack of non-adjoint Dirac representation coverage. |

Notes:

- GPU work from open issue 7 stays out of scope unless profiling shows that
  Phase V analysis is now a runtime bottleneck.
- P5-G11 is a true limitation, but it should not be tackled before the P0/P1
  integrity and evidence gaps are closed.

---

## Workstream 1: Make the M53 path real

### Scope

Close P5-G08 and most of P5-G10.

### Problems to fix

- `studies/phase5_su2_branch_refinement_env_validation/run_study.sh` does not
  execute the full refinement, falsification, typed dossier, and report flow
  described by `IMPLEMENTATION_P5.md`.
- `src/Gu.Phase5.Reporting/Phase5CampaignRunner.cs` contains the intended
  orchestration logic, but that logic is not the checked-in study path.
- The checked-in study appears to depend on CLI commands that are not presently
  wired in `apps/Gu.Cli/Program.cs`.

### Deliverables

1. Add actual CLI wiring for the Phase V commands promised by the Phase V docs.
2. Add a single top-level Phase V campaign command that executes M46-M53
   through `Phase5CampaignRunner`.
3. Update `run_study.sh` to call that top-level campaign entry point instead of
   stitching together a partial workflow.
4. Emit all of the following from the same run:
   - branch robustness artifact
   - refinement study result
   - falsifier summary
   - `Phase5ValidationDossier`
   - provenance `ValidationDossier`
   - `Phase5Report`
   - markdown report output

### Acceptance criteria

- The reference study can be reproduced from one documented command sequence.
- The run produces both technical evidence artifacts and provenance/evidence-tier
  artifacts.
- The checked-in reference study no longer bypasses M47, M50, or reporting.
- Add an integration test that invokes the top-level campaign command and
  asserts all expected artifacts are produced.

---

## Workstream 2: Make observation-chain evidence first-class

### Scope

Close P5-G09 and provide the substrate needed for P5-G01.

### Problems to fix

- `IMPLEMENTATION_P5.md` requires observation-chain verification artifacts,
  extraction sensitivity reports, and auxiliary-model sensitivity reports.
- `Phase5DossierAssembler` currently treats observation-chain validity as
  `candidate.ObservationConfidence > 0`, which is a placeholder rather than a
  Phase V evidence gate.
- Without explicit observation-chain records, `ObservationInstability` cannot be
  activated meaningfully.

### Deliverables

1. Add explicit observation-chain record types, for example:
   - `ObservationChainRecord`
   - `ExtractionSensitivityRecord`
   - `AuxiliaryModelSensitivityRecord`
2. Extend `Phase5ValidationDossier` to include `ObservationChainSummary`.
3. Replace the placeholder escalation gate with a gate derived from actual
   observation-chain completeness and sensitivity thresholds.
4. Feed observation-chain instability into `FalsifierEvaluator`.

### Acceptance criteria

- A candidate can fail `ObservationChainValid` for a specific recorded reason.
- At least one test proves that observation sensitivity now generates an active
  `ObservationInstability` falsifier.
- Dossiers preserve both positive and negative observation-chain evidence.

---

## Workstream 3: Activate all deferred falsifiers

### Scope

Close P5-G01.

### Problems to fix

- Four of seven falsifier types are still placeholders:
  `ObservationInstability`, `EnvironmentInstability`,
  `RepresentationContent`, and `CouplingInconsistency`.
- The missing problem is not just evaluator code. The upstream record types and
  study outputs needed to drive these falsifiers are absent or not yet bridged.

### Deliverables

1. Define and serialize the missing input records:
   - observation-chain sensitivity outputs
   - environment variance summary records
   - representation diff / content mismatch records
   - coupling consistency records across branch variants
2. Extend `FalsifierEvaluator.Evaluate(...)` to accept these records.
3. Add threshold policy fields for each deferred falsifier type.
4. Update reports and dashboards so these falsifiers surface in M53 outputs.

### Acceptance criteria

- All seven falsifier types are exercised by unit tests.
- At least one end-to-end study can emit each formerly deferred falsifier type.
- Claim escalation and registry demotion use the newly active falsifiers.

---

## Workstream 4: Fix the convergence model at the type level

### Scope

Close P5-G02.

### Problems to fix

- `RefinementLevel` still stores one `MeshParameter`.
- `ARCH_P5.md` explicitly requires `h = max(h_X, h_F)` for the product mesh.
- The current representation prevents separate X-space and fiber-space
  refinement studies.

### Deliverables

1. Replace `MeshParameter` with:
   - `MeshParameterX`
   - `MeshParameterF`
2. Add a computed or explicit effective `h = max(h_X, h_F)` path in the
   Richardson logic.
3. Update JSON schema, config files, and study specs.
4. Add mixed-refinement tests, not just uniform-halving tests.

### Acceptance criteria

- Existing uniform refinement still works by setting `h_X == h_F`.
- A test with anisotropic refinement produces the expected effective `h`.
- The convergence report states whether a run used joint or anisotropic
  refinement.

---

## Workstream 5: Bridge Phase III solved backgrounds into Phase V

### Scope

Close P5-G05 and reduce the placeholder character of P5-G03/P5-G04.

### Problems to fix

- The reference branch family still uses analytic `A0` profiles.
- Phase V studies are therefore branch-aware but not yet tightly coupled to the
  solved Phase III background atlas.

### Deliverables

1. Define a bridge type from Phase III background artifacts to Phase V branch
   variant specs.
2. Update study config so branch variants can be sourced from persisted
   backgrounds rather than inline analytic forms.
3. Add at least one reference study variant set that originates from real Phase
   III background outputs.

### Acceptance criteria

- Two different persisted Phase III backgrounds produce measurably different
  Phase V branch-study inputs.
- The reference study provenance records the upstream background IDs and hashes.
- Remove the current analytic `A0` family from the default evidence path.

---

## Workstream 6: Make imported environments genuinely imported

### Scope

Close P5-G04.

### Problems to fix

- `EnvironmentImporter` can deserialize mesh files, but the "imported" tier is
  not yet tied to a defined external dataset contract or curated evidence tier.
- Without a real import contract, the imported tier is structurally present but
  scientifically weak.

### Deliverables

1. Define an external environment schema and versioned import contract.
2. Add dataset provenance fields:
   - dataset ID
   - source URI or local provenance string
   - source hash
   - conversion version
3. Add at least one curated imported benchmark dataset checked into a reproducible
   location or documented acquisition path.
4. Run admissibility and campaign tests on that imported dataset.

### Acceptance criteria

- An imported environment can be traced to an external source and re-imported
  deterministically.
- The imported tier is no longer produced by reusing structured generators.
- The dossier cleanly distinguishes toy, structured, and imported evidence.

---

## Workstream 7: Upgrade quantitative targets and uncertainty modeling

### Scope

Close P5-G03 and P5-G07.

### Problems to fix

- External targets are still synthetic placeholders.
- Pull scoring assumes Gaussian uncertainty only.
- Until the environment/background bridge is real, target realism cannot be
  upgraded responsibly.

### Deliverables

1. Add a physical-target ingestion layer with explicit provenance and evidence
   tiering.
2. Add `DistributionModel` to targets and observables.
3. Support at least:
   - Gaussian
   - asymmetric Gaussian
   - Student-t
4. Update target matching to dispatch on distribution model.
5. Add cross-checks that prevent physical-looking labels from being attached to
   toy-placeholder targets.

### Acceptance criteria

- Quantitative matching can run with at least one non-Gaussian target.
- The report shows which uncertainty model was used for each match.
- The default reference study still labels placeholder targets honestly until
  real targets are available.

---

## Workstream 8: Expand branch-family coverage after the evidence path is solid

### Scope

Close P5-G06 and address P5-G11.

### Problems to fix

- Current Shiab variation is identity-only.
- The declared branch family still spans a narrow subset of admissible GU
  operator choices.
- `ASSUMPTIONS.md` already records that branch-invariant means only invariant
  within the declared family.

### Deliverables

1. Implement at least one non-identity Shiab operator in the main pipeline.
2. Add `ShiabVariantId` to refinement and campaign specs.
3. Extend the reference family beyond the current four variants.
4. Evaluate whether non-adjoint Dirac representation variation should be added
   as a separate branch-family axis or deferred explicitly to Phase VI.

### Acceptance criteria

- At least one convergence and one branch-robustness study include non-identity
  Shiab variation.
- Dossiers report the branch-family envelope explicitly.
- `ASSUMPTIONS.md` is updated to reflect the reduced branch-local scope.

---

## Recommended implementation order

### Phase A: integrity first

1. Workstream 1
2. Workstream 2
3. Workstream 3

Rationale:
Before adding more physics-facing variation, the repository needs one honest,
reproducible Phase V path that emits the full intended evidence bundle.

### Phase B: numerical and cross-phase correctness

4. Workstream 4
5. Workstream 5
6. Workstream 6

Rationale:
These are the changes that turn Phase V from a mostly self-contained analysis
layer into a true continuation of the solved-background and environment ladders.

### Phase C: stronger evidence claims

7. Workstream 7
8. Workstream 8

Rationale:
Real targets, richer uncertainty models, and broader branch families only pay
off after the pipeline and upstream data contracts are already credible.

---

## Suggested issue breakdown

If this work is tracked as implementation tickets, the clean split is:

1. `P5-CLI-001` Full Phase V campaign CLI and study runner wiring
2. `P5-DOS-001` Typed dossier/report integration for campaign outputs
3. `P5-OBS-001` Observation-chain artifact model and gating
4. `P5-FAL-001` Activate deferred falsifier inputs and policies
5. `P5-CONV-001` Dual mesh parameters (`h_X`, `h_F`) and Richardson update
6. `P5-BRIDGE-001` Phase III background to Phase V branch-variant bridge
7. `P5-ENV-001` External environment import schema and benchmark dataset
8. `P5-QV-001` Real target ingestion and non-Gaussian scoring
9. `P5-BRANCH-001` Non-identity Shiab and expanded branch-family coverage

---

## Success condition for this plan

This plan is complete when Phase V can do all of the following from one
reproducible campaign path:

- run a true M46-M53 study,
- consume persisted Phase III backgrounds,
- distinguish toy, structured, and imported environments honestly,
- evaluate all intended falsifier classes,
- gate escalation on actual observation-chain evidence,
- compute convergence with the correct mesh model,
- compare to targets with explicit provenance and uncertainty-model semantics,
- and emit both provenance-grade and technically rich dossiers.

Until that point, Phase V is implemented enough to demonstrate architecture, but
not yet complete enough to support its strongest evidence claims.

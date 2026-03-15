# IMPLEMENTATION_P11.md

## Purpose

This is the implementation handoff for Phase XI, starting from the Phase X run:

- summary: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/summary.md`
- artifacts: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/campaign_artifacts`
- bridge comparison: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/bridge_comparison`
- Shiab companion: `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion`

Phase XI should treat those artifacts as the binding starting state.

## Read First

Read these before making any Phase XI change:

- `/home/josh/Documents/GitHub/GeometricUnity/ARCH_P11.md`
- `/home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P11.md`
- `/home/josh/Documents/GitHub/GeometricUnity/PHASE_11_OPEN_ISSUES.md`
- `/home/josh/Documents/GitHub/GeometricUnity/PHASE11_SUMMARY.md`
- `/home/josh/Documents/GitHub/GeometricUnity/ASSUMPTIONS.md`
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/summary.md`
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/shiab_companion/summary.md`
- `/home/josh/Documents/Geometric_Unity-Draft-April-1st-2021.pdf`

Then inspect these code surfaces before deciding implementation scope:

- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.ReferenceCpu/AugmentedTorsionCpu.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.ReferenceCpu/IdentityShiabCpu.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.ReferenceCpu/FirstOrderShiabOperator.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Observation/FermionObservationPipeline.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/apps/Gu.Cli/Program.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_l0.json`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase5_su2_branch_refinement_env_validation/config/background_study_phase10_direct_refinement_first_order_l0.json`

## Execution Order

Execute Phase XI in this order unless repository evidence forces a different dependency order:

1. `P11-M8` Add a draft-alignment check for augmented torsion.
2. `P11-M10` Add a full-pullback observation path or narrow observation claims.
3. `P11-M9` Expand Shiab evidence beyond the current minimal branch pair.
4. `P11-M7` Add a draft-aligned Observerse evidence contract.
5. `P11-M3` Add a nontrivial direct solver-backed refinement path.
6. `P11-M4` Resolve or stabilize branch fragility.
7. `P11-M1` Exercise candidate-linked branch and background provenance in a real campaign.
8. `P11-M2` Add genuine external imported evidence.
9. `P11-M5` Resolve or stabilize the representation-content fatal.
10. `P11-M6` Rerun and evaluate the paired Shiab path after any relevant change.

If an earlier milestone changes interpretation, artifacts, manifests, or copied inputs for
later milestones, rerun the affected later milestones rather than carrying forward stale results.

## Binding Decisions

### D-P11-001 Do Not Relabel Mixed Branch Studies As Robust

The standard and paired branch studies are still `mixed`.
Do not summarize that as robustness.

### D-P11-002 Do Not Relabel Repo-Internal Imported Provenance As External

The copied imported environment record still uses `datasetId = repo-internal-phase8-import-mesh-v1`.
Do not call that external data.

### D-P11-003 Keep Candidate Gates Fail-Closed Until Real Candidate Links Exist

The executed Phase X dossier still has empty `evidenceRecordIds` for `branch-robust`
and `refinement-bounded`. Do not infer those links globally or synthesize them from
campaign-wide outputs.

### D-P11-004 Preserve Branch Fragility And Representation Fatal As Negative Evidence

`gauge-violation` and `solver-iterations` remain active branch-fragility evidence,
and representation-content still contributes an active fatal falsifier.
Do not smooth those away in summaries or scorecards.

### D-P11-005 Preserve The Distinction Between Direct And Bridge Refinement Evidence

Phase X now has both forms of refinement evidence. Keep direct solver-backed and
bridge-derived ladders separately visible in reports, dossiers, and copied inputs.

### D-P11-006 Do Not Overstate The Current Direct Refinement Family

The executed direct ladders are real solver-backed evidence, but they are exact
zero-invariant control ladders. Do not describe them as a nontrivial direct refinement
closure on the mixed admitted atlas family.

### D-P11-007 Do Not Treat The Current Toy Geometry Path As Draft-Level `X^4` / Observerse Recovery

The April 1, 2021 draft starts from a topological `X^4` and treats observation as
recovery from the Observerse via pullback. The current executable evidence path still
uses toy/structured low-dimensional geometry and should not be summarized as direct
evidence for draft-level `X^4` recovery.

### D-P11-008 Do Not Treat The Current Augmented Torsion Operator As Draft-Exact

The draft's Section 7 defines augmented torsion at group level as
`T_g = $ - ε^{-1} d_{A0} ε`. The current executable branch still uses the lower-level
operator `T^aug = d_{A0}(omega - A0)`. Until equivalence is derived and tested, or a
draft-form branch is implemented, do not describe the current operator as a literal
implementation of the draft formula.

### D-P11-009 Do Not Treat The Current Shiab Branches As Canonical Draft Closure

The draft's Section 8 explicitly treats Shiab as a family of operators and leaves
operator choice underdetermined. The current runtime still relies mainly on
`identity-shiab` in the standard path, with `first-order-curvature` as the paired
comparison branch. Do not summarize either as the uniquely selected draft operator.

### D-P11-010 Do Not Treat Simplified Fermion Observation Proxies As Full Pullback Evidence

The draft's Sections 3-4 frame observed field content as pullback from `Y` to `X`
through the Observerse. The current `FermionObservationPipeline` still states that full
`sigma_h` pullback is out of scope and only extracts proxy observables. Do not treat
current fermion observation summaries as full draft-aligned observation evidence.

## Milestones

### P11-M1 Exercise Candidate-Linked Branch And Background Provenance In A Real Campaign

Required behavior:

- add real candidate-linked branch/background provenance to the registry or a real auxiliary input,
- ensure the Phase XI campaign spec copies that input forward,
- confirm that executed dossier `branch-robust` and `refinement-bounded` gates carry
  non-empty `evidenceRecordIds` for candidates whose evidence exists,
- keep mixed populations supported: candidates with links and candidates without links
  must still be distinguished correctly.

Definition of done:

- executed campaign artifacts contain the copied candidate-link input,
- at least one real executed candidate has non-empty `evidenceRecordIds` at both relevant gates if honest evidence exists,
- candidates without evidence still fail closed rather than inheriting campaign-wide links,
- tests cover the mixed linked/unlinked population contract.

### P11-M2 Add Genuine External Imported Evidence

Required behavior:

- integrate at least one imported environment record backed by an external dataset,
- preserve the external dataset provenance in copied inputs and summaries,
- keep internal benchmark imported records distinct from external imported records.

Definition of done:

- at least one executed copied environment record cites an external dataset rather than a repo-internal dataset id,
- campaign summaries and scorecards keep internal and external imported evidence explicitly separate,
- any affected quantitative or dossier logic is covered by tests and rerun artifacts.

### P11-M3 Add A Nontrivial Direct Solver-Backed Refinement Path

Required behavior:

- produce at least one nontrivial refinement ladder from direct solver-backed runs,
- keep the existing bridge-derived ladder and the current zero-invariant direct ladder available for comparison,
- update reports and dossiers so the executed artifacts state clearly which refinement
  path is direct nontrivial evidence, which is direct control evidence, and which remains bridge-derived,
- add tests for any new direct refinement contract.

Definition of done:

- at least one executed direct refinement ladder has non-constant values across levels,
- the copied refinement evidence manifest records `direct-solver-backed`,
- campaign outputs and comparison artifacts distinguish direct control, direct nontrivial, and bridge-derived ladders,
- relevant convergence/reporting/falsification tests are run after the change.

### P11-M4 Resolve Or Stabilize Branch Fragility

Required behavior:

- investigate whether the mixed branch result can be improved under unchanged thresholds,
- if a stronger family exists, demonstrate it with executed artifacts,
- if no stronger family exists in repository context, document that the mixed result is
  the stable scientific boundary and not a missing implementation.

Definition of done:

- either an executed stronger family improves the mixed result under unchanged thresholds,
- or the executed artifact set and broader repo-context search are explicitly cited as the reason the mixed result is stabilized,
- summaries and open-issues docs preserve any remaining fragility as negative evidence.

### P11-M5 Resolve Or Stabilize The Representation-Content Fatal

Required behavior:

- determine whether stronger candidate content can close the active fatal representation-content falsifier,
- if not, preserve it as an explicit blocker in the executed Phase XI artifacts and handoff.

Definition of done:

- either the executed falsifier summary no longer contains the active fatal representation-content blocker,
- or the executed Phase XI artifacts explicitly preserve it as an unresolved blocker with source evidence,
- affected tests are run if code or dossier logic changes.

### P11-M6 Keep The Paired Shiab Path Scientifically Comparable

Required behavior:

- rerun the paired `first-order-curvature` path whenever Phase XI changes affect branch,
  refinement, imported evidence, or candidate-evidence interpretation,
- preserve direct comparability between the standard and paired paths,
- state explicitly whether broader Shiab scope changes any conclusion or only reproduces
  the same mixed branch story.

Definition of done:

- the paired path is rerun after every relevant standard-path change,
- the final summary explicitly compares standard and paired results on the same evidence categories,
- no paired conclusion is inferred from stale or pre-change artifacts.

### P11-M7 Add A Draft-Aligned Observerse Evidence Contract

Required behavior:

- add explicit artifacts that separate toy control geometry from any draft-aligned
  `X^4` / Observerse evidence path,
- if repository context permits, introduce at least one non-toy or higher-fidelity
  observerse-backed environment path with explicit observation metadata rather than
  only implicit pullback naming,
- if repository context does not permit that, gate summaries and dossiers so they
  state mechanically that current evidence is toy-control only and not `X^4` recovery.

Definition of done:

- executed artifacts explicitly label geometry/observation evidence as either toy-control or draft-aligned,
- if no higher-fidelity path is available, the final dossier/report language mechanically blocks `X^4` recovery claims,
- tests are added or updated for any new labeling or gating logic.

### P11-M8 Add A Draft-Alignment Check For Augmented Torsion

Required behavior:

- either implement a branch that is materially closer to the draft's group-level
  augmented torsion `T_g = $ - ε^{-1} d_{A0} ε`,
- or derive and test the exact restricted conditions under which the current
  `d_{A0}(omega - A0)` lowering is an acceptable executable surrogate,
- ensure reports and manifests distinguish draft-form evidence from surrogate
  branch-local evidence if both remain in the repository.

Definition of done:

- either a draft-form or closer-to-draft executable torsion branch is implemented and tested,
- or a documented, tested equivalence/surrogate boundary is added and propagated into reports/manifests,
- no final summary text implies draft-exact closure unless the executed artifacts actually support it.

### P11-M9 Expand Shiab Evidence Beyond The Current Minimal Branch Pair

Required behavior:

- keep `identity-shiab` and `first-order-curvature` explicitly distinguishable,
- add at least one stronger nontrivial Shiab comparison path in the main evidence
  campaign if repository context supports it, or document artifact-backed reasons why
  the current repository cannot yet support a broader Shiab family study,
- do not infer canonicity from the current small operator family without executed
  comparison evidence.

Definition of done:

- either at least one additional executed Shiab comparison path exists,
- or the final artifacts document why repo context blocks honest expansion beyond the current pair,
- reports and docs continue to treat Shiab choice as a family question rather than a closed canonical selection.

### P11-M10 Add A Full-Pullback Observation Path Or Keep Observation Claims Narrow

Required behavior:

- close the gap between the draft's pullback-based observation story and the current
  simplified fermion observation proxies,
- if feasible, implement and exercise a full `sigma_h` pullback path for the relevant
  observed quantities and record the exact artifacts used,
- if not feasible in repository context, keep all candidate/observation summaries
  explicitly labeled as proxy observations rather than full draft-aligned pullback results.

Definition of done:

- either an executed full-pullback path exists for the relevant observed quantities,
- or all affected summaries/records are explicitly labeled as proxy observation outputs,
- tests cover the label/contract distinction and any new observation pipeline behavior.

## Mandatory Workflow

1. Read the required files and the draft PDF.
2. Inspect the current Phase X code paths, configs, copied inputs, dossiers, and paired artifacts.
3. Close the highest-priority Phase XI item that repository context honestly allows.
4. Run relevant tests after each substantive fix.
5. Rerun the standard campaign whenever a change affects artifacts, copied inputs, or interpretation.
6. Rerun the paired/companion path whenever a change affects branch, refinement, imported evidence, or draft-alignment interpretation.
7. Verify final artifacts, manifests, copied inputs, dossiers, and reproduced inputs.
8. Evaluate the results strictly against the draft-sensitive and evidence-sensitive categories below.
9. Update `/home/josh/Documents/GitHub/GeometricUnity/ARCH_P11.md`, `/home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P11.md`, `/home/josh/Documents/GitHub/GeometricUnity/PHASE_11_OPEN_ISSUES.md`, and `/home/josh/Documents/GitHub/GeometricUnity/PHASE11_SUMMARY.md` to reflect what was actually closed or proven blocked.

## Required Final Evaluation

The next evaluation must distinguish:

- contracts implemented and exercised,
- contracts implemented but not exercised,
- nontrivial upstream evidence,
- real external evidence,
- internal benchmark evidence,
- bridge-derived evidence,
- direct solver-backed control evidence,
- direct solver-backed nontrivial refinement evidence,
- draft-aligned observerse evidence versus toy-control evidence,
- draft-form operator evidence versus branch-local surrogate operator evidence,
- full-pullback observation evidence versus proxy observation evidence,
- Shiab family comparison evidence versus single-branch default evidence,
- and remaining scientific limitations.

If candidate-linked branch/refinement evidence IDs are still absent in the executed
dossier, imported provenance is still only repo-internal, direct refinement is still
only zero-invariant control evidence, current geometry is still only toy-control,
current augmented torsion remains only surrogate-form, current observation results are
still proxy pullback summaries, or the standard/paired branch studies remain mixed
without stronger family evidence, those items remain open.

## Final Handoff Requirement

If any draft-sensitive item remains unclosed, the final handoff must state exactly which
draft section or concept remains unmatched by the executable branch and which executed
artifact-backed reason prevented closure.

# IMPLEMENTATION_P12.md

## Purpose

This is the implementation handoff for Phase XII.

The new goal is narrower and more executable than "match the Standard Model against real values":

- get the repository to the point where it can **perform nontrivial boson and fermion calculations**
  from the same persisted background artifacts,
- remove the known runtime placeholders that currently prevent those calculations from being honest,
- and produce a reproducible end-to-end calculation campaign that yields bosonic spectra,
  boson candidates, fermion modes, chirality/conjugation outputs, and boson-fermion
  coupling artifacts from the same run family.

Phase XII is **not** a claim that the repository can already reproduce the known Standard
Model values. It is the phase that should make the boson and fermion calculation pipelines
actually runnable, artifact-backed, and internally consistent enough to support later
comparison work.

## Binding Goal

At the end of Phase XII, another agent should be able to point at one concrete run folder
and say:

- these boson artifacts were computed from persisted solved backgrounds,
- these fermion artifacts were computed from the same persisted solved backgrounds,
- these boson-fermion couplings were computed from actual Dirac-operator variation artifacts,
- the outputs are no longer primarily toy fallback, zero-state fallback, or placeholder coupling matrices,
- and the entire chain is reproducible from checked-in configs and code.

## Read First

Read these before making any Phase XII change:

- `/home/josh/Documents/GitHub/GeometricUnity/ASSUMPTIONS.md`
- `/home/josh/Documents/GitHub/GeometricUnity/ARCH_P11.md`
- `/home/josh/Documents/GitHub/GeometricUnity/IMPLEMENTATION_P11.md`
- `/home/josh/Documents/GitHub/GeometricUnity/PHASE_11_OPEN_ISSUES.md`
- `/home/josh/Documents/GitHub/GeometricUnity/PHASE11_SUMMARY.md`
- `/home/josh/Documents/Geometric_Unity-Draft-April-1st-2021.pdf`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/bosonic_validation_001/REPORT.md`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase4_fermion_family_atlas_001/STUDY.md`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase4_fermion_family_atlas_001/output/REPORT.md`
- `/home/josh/Documents/GitHub/GeometricUnity/reports/post_phase10_evidence_campaign/20260315T154620Z/summary.md`

Then inspect these code surfaces before deciding implementation scope:

- `/home/josh/Documents/GitHub/GeometricUnity/apps/Gu.Cli/Program.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.ReferenceCpu/AugmentedTorsionCpu.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.ReferenceCpu/IdentityShiabCpu.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.ReferenceCpu/FirstOrderShiabOperator.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase3.Observables/ObservationPipeline.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Observation/FermionObservationPipeline.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Couplings/CouplingProxyEngine.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase3.Registry/CandidateBosonBuilder.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase3.Campaigns/BosonCampaignRunner.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.FamilyClustering/FermionFamilyAtlasBuilder.cs`
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Registry/RegistryMergeEngine.cs`

## Verified Starting State

From current repository state, all of the following are true:

- the Phase V evidence campaign now has direct solver-backed refinement plumbing, but it is still
  primarily a validation/provenance campaign rather than a boson/fermion calculation campaign,
- Phase III boson artifacts can be produced through `compute-spectrum`, `build-boson-registry`,
  `run-boson-campaign`, and `export-boson-report`,
- Phase IV fermion artifacts can be produced through `assemble-dirac`, `solve-fermion-modes`,
  `analyze-chirality`, `analyze-conjugation`, `extract-couplings`, `build-family-clusters`,
  and `build-unified-registry`,
- the current reference boson study is a nontrivial but still toy 3D/su2 validation path,
- the current reference fermion family study is Toy2D/su2 and explicitly not a physical validation study,
- `assemble-dirac` still hardcodes `ToyGeometryFactory.CreateToy2D()` and a default `Cl(5,0)` spinor
  spec even when persisted background artifacts exist,
- `assemble-dirac` still falls back to a zero bosonic state when it cannot find a stored omega state,
- `extract-couplings` still builds placeholder zero variation matrices instead of actual `delta_D`
  finite-difference matrices,
- boson campaign execution still defaults to internal target profiles and does not yet provide
  a real external-descriptor calculation path,
- fermion observation is still proxy-level rather than full pullback-level observation.

## Non-Negotiable Rules

1. Do not describe Phase XII as Standard Model reproduction.
2. Do not replace missing calculations with labels, heuristics, or synthetic values and call them "computed."
3. Do not keep zero-state fallbacks or zero coupling placeholders on the main calculation path without emitting explicit blocking artifacts.
4. Do not weaken existing negative-result reporting, demotion logic, falsifiers, or provenance rules to make calculation outputs look cleaner.
5. Keep generated artifacts as the primary evidence for what was actually computed.
6. If a boson or fermion output still depends on a placeholder path, label it explicitly as placeholder-derived in artifacts and summaries.

## Primary Objective

Complete as much of the work as repository context honestly allows toward a **real executable
boson/fermion calculation pipeline**, including:

- implementation,
- tests,
- end-to-end execution,
- strict evaluation,
- and the identification of what remains impossible or scientifically unsupported.

The outcome should be an artifact-backed answer to:

- what boson calculations can now be performed,
- what fermion calculations can now be performed,
- what boson-fermion coupling calculations can now be performed,
- and which remaining gaps are still preventing later comparison against real known values.

## Execution Order

Execute Phase XII in this order unless repository evidence forces a different dependency order:

1. `P12-M1` Make Dirac assembly background-aware and environment-aware.
2. `P12-M2` Make spinor specs and fermion layout selection explicit and run-derived rather than hardcoded defaults.
3. `P12-M3` Replace placeholder coupling extraction with real finite-difference Dirac variation.
4. `P12-M4` Produce a joined boson/fermion calculation campaign from one persisted background family.
5. `P12-M5` Strengthen boson calculation outputs beyond internal-profile-only reporting.
6. `P12-M6` Strengthen fermion observation outputs beyond proxy-only ambiguity where repository context allows.
7. `P12-M7` Build a reproducible calculation dossier and summary for the combined boson/fermion run.
8. `P12-M8` Rerun draft-sensitive operator and observation checks if any earlier milestone changes their interpretation.

If an earlier milestone changes background selection, geometry, operator choice, observation provenance,
or coupling construction, rerun all downstream boson/fermion calculations rather than carrying stale artifacts forward.

## Current Blocking Gaps To Close

These are the concrete implementation blockers to "perform the calculations":

### G1. `assemble-dirac` Is Still On A Toy2D / Default-Spec Fallback Path

Current evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/apps/Gu.Cli/Program.cs`
  still builds Dirac geometry with `ToyGeometryFactory.CreateToy2D()`,
- the same command still uses `BuildDefaultSpinorSpec()` as the main fallback,
- and still emits "No background omega found; using zero bosonic state."

Impact:

- fermion calculations are not yet honestly tied to the same persisted environment/background
  artifacts that boson calculations use,
- and a user can get a fermion result that is partly disconnected from the actual run folder.

### G2. Coupling Extraction Still Uses Placeholder Variation Matrices

Current evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/apps/Gu.Cli/Program.cs`
  explicitly says "Build empty variation matrices for now",
- and `extract-couplings` constructs zero matrices for boson candidates instead of actual `delta_D`.

Impact:

- the coupling atlas can exist structurally without being an honest boson-fermion calculation,
- which blocks any serious interpretation of interaction outputs.

### G3. Boson Campaigning Is Still Mostly Internal-Profile Comparison

Current evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/apps/Gu.Cli/Program.cs`
  seeds `run-boson-campaign` with internal target profiles,
- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase3.Campaigns/BosonCampaignRunner.cs`
  supports external analogy mode, but current CLI defaults do not provide a real external calculation campaign.

Impact:

- boson calculations can be performed internally, but they are not yet packaged as a later-ready comparison path.

### G4. Fermion Observation Remains Proxy-Level

Current evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/src/Gu.Phase4.Observation/FermionObservationPipeline.cs`
  says full `sigma_h` pullback is out of scope.

Impact:

- fermion mode solving exists,
- but interpretation and identification remain limited by proxy observation summaries.

### G5. Current Studies Are Still Toy / Control Studies

Current evidence:

- `/home/josh/Documents/GitHub/GeometricUnity/studies/bosonic_validation_001/REPORT.md`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase4_fermion_family_atlas_001/STUDY.md`
- `/home/josh/Documents/GitHub/GeometricUnity/studies/phase4_fermion_family_atlas_001/output/REPORT.md`

Impact:

- the code can demonstrate branches and artifacts,
- but Phase XII must explicitly separate "calculation now executable" from "physical validation achieved."

## Milestones

### P12-M1 Make Dirac Assembly Background-Aware And Environment-Aware

Required behavior:

- make `assemble-dirac` consume persisted background geometry, omega, A0, and manifest context from the selected run folder,
- remove the main-path dependence on `ToyGeometryFactory.CreateToy2D()` when persisted geometry is available,
- ensure the assembled Dirac bundle records enough provenance to trace back to the exact background record and environment artifact,
- preserve an explicit fallback only as a clearly labeled control path, not as the main executable calculation path.

Definition of done:

- a nontrivial run folder can produce a Dirac bundle without using Toy2D fallback when persisted geometry exists,
- the Dirac bundle provenance names the actual background/environment artifacts used,
- tests cover the environment-aware and fallback-separated behavior,
- a rerun proves the command uses the real run artifacts.

### P12-M2 Make Spinor Spec Selection Run-Derived Rather Than Hardcoded

Required behavior:

- make the spinor representation spec depend on declared run/config context rather than only `BuildDefaultSpinorSpec()`,
- allow persisted or configured spinor specs to flow through the fermion pipeline cleanly,
- ensure mismatches between geometry, spinor dimension, and gauge dimension fail loudly rather than silently reverting to defaults.

Definition of done:

- the main fermion path can consume an explicit run-derived spinor spec,
- incompatible specs fail with explicit errors,
- tests cover successful custom-spec and invalid-spec paths.

### P12-M3 Replace Placeholder Coupling Extraction With Real Finite-Difference Dirac Variation

Required behavior:

- implement a real boson-mode-to-`delta_D` path for coupling extraction,
- build actual variation matrices from perturbed Dirac assemblies rather than zero placeholders,
- ensure the coupling atlas records whether each coupling came from a real finite-difference calculation or from a blocked path,
- preserve negative/zero couplings honestly when the calculation yields them.

Definition of done:

- `extract-couplings` no longer needs placeholder zero matrices on the main path,
- at least one executed coupling atlas contains records derived from actual `delta_D` variation,
- tests cover finite-difference construction and provenance labeling,
- a nontrivial coupling artifact is produced from the same background family used by boson and fermion calculations.

### P12-M4 Produce A Joined Boson/Fermion Calculation Campaign From One Persisted Background Family

Required behavior:

- define one checked-in study/config path that runs:
  - background solve,
  - bosonic spectrum,
  - boson registry,
  - Dirac assembly,
  - fermion mode solve,
  - chirality/conjugation analysis,
  - coupling extraction,
  - family clustering,
  - unified registry,
- ensure all outputs point back to the same background family rather than unrelated toy defaults,
- record reproduction commands and copied inputs.

Definition of done:

- one output root contains the full joined calculation chain,
- boson and fermion artifacts in that root share the same persisted background provenance,
- a summary file states exactly which calculations were performed and which remained blocked.

### P12-M5 Strengthen Boson Calculation Outputs Beyond Internal-Profile-Only Reporting

Required behavior:

- keep internal profile comparison available,
- add a more calculation-focused boson output path that emphasizes computed spectral families,
  candidate envelopes, stability, and observation outputs rather than only campaign verdicts,
- if possible from repository context, add descriptor-driven external-style comparison scaffolding
  without claiming real external validation.

Definition of done:

- boson outputs can be consumed as calculations even without running an internal campaign,
- at least one report/dossier distinguishes raw computed boson artifacts from interpretive campaign verdicts,
- tests or executed artifacts show the distinction.

### P12-M6 Strengthen Fermion Observation Outputs Beyond Proxy-Only Ambiguity Where Possible

Required behavior:

- improve the fermion observation path as far as repository context allows,
- if full pullback observation is still not feasible, explicitly split:
  - solved fermion calculations,
  - proxy observation summaries,
  - and blocked full-observation items,
- preserve ambiguity as negative evidence rather than smoothing it away.

Definition of done:

- the final calculation outputs distinguish solved fermion modes from later-stage interpretation,
- any remaining proxy observation path is labeled explicitly,
- tests cover new labeling/contract behavior if code changes.

### P12-M7 Build A Reproducible Boson/Fermion Calculation Dossier

Required behavior:

- create one top-level Phase XII output summary that lists:
  - run root,
  - boson artifacts,
  - fermion artifacts,
  - coupling artifacts,
  - registry artifacts,
  - blocked items,
- distinguish:
  - genuinely computed fields/modes/couplings,
  - placeholder-derived outputs,
  - proxy observation outputs,
  - and interpretation-only outputs.

Definition of done:

- a single summary can tell a future agent exactly what was really calculated,
- no later user has to infer from scattered files whether a coupling or mode was placeholder-derived,
- dossier/report generation is rerun after all substantive changes.

### P12-M8 Rerun Draft-Sensitive Operator And Observation Checks After Any Relevant Change

Required behavior:

- if Phase XII changes torsion, Shiab choice, observation provenance, or geometry/observerse interpretation,
  rerun the draft-sensitive checks introduced in Phase XI,
- keep the executable branch honest about draft alignment versus surrogate/operator-placeholder status.

Definition of done:

- final Phase XII artifacts state which calculations are branch-local executable outputs,
- and which still fall short of draft-form closure.

## Mandatory Workflow

1. Read the required files.
2. Inspect the current boson and fermion CLI paths, study configs, and known reports.
3. Close the highest-priority calculation blocker that repository context honestly allows.
4. Run relevant tests after each substantive fix.
5. After each meaningful boson-side change, rerun the affected boson calculation path.
6. After each meaningful fermion-side or coupling-side change, rerun the affected fermion/coupling path.
7. Once enough pieces exist, run the joined boson/fermion calculation campaign end to end.
8. Verify the final artifacts strictly.
9. Update the top-level docs that need to reflect what Phase XII actually closed or proved blocked.

## Minimum Tests To Run When Relevant

At minimum, rerun the relevant suites touching changed surfaces. Likely candidates include:

- bosonic spectrum / background / observation tests
- fermion solver / chirality / conjugation / family clustering tests
- coupling atlas tests
- CLI tests if command behavior changes
- registry/reporting tests if artifact semantics change

If a relevant suite is not run, the affected area is not closed.

## Required Final Evaluation

The final evaluation must distinguish:

- boson calculations that are actually executed from persisted run artifacts,
- fermion calculations that are actually executed from persisted run artifacts,
- couplings derived from real `delta_D` variation versus placeholder matrices,
- raw computed artifacts versus interpretive comparison outputs,
- proxy observation outputs versus full-observation outputs,
- toy/control geometry evidence versus stronger environment evidence,
- internal benchmark evidence versus real external evidence,
- and remaining scientific limitations.

If `assemble-dirac` still depends on Toy2D fallback for the main path, couplings still depend on
placeholder zero matrices, boson outputs still only exist as internal-profile comparison verdicts,
or fermion observation is still only proxy-level without explicit labeling, those items remain open.

## Final Handoff Requirement

When Phase XII is complete, the final handoff must answer all of the following plainly:

1. What boson calculations can now be performed end to end?
2. What fermion calculations can now be performed end to end?
3. Are boson-fermion couplings now computed from actual variation matrices or still placeholder-derived?
4. What exact run folder reproduces the combined calculation pipeline?
5. What remains missing before later comparison against real known boson and fermion values becomes scientifically honest?

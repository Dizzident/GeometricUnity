# IMPLEMENTATION_P15.md

## Purpose

Phase XV turns the Phase XIV campaign from "all targets are covered" into
"remaining failures are explained or repaired."

Phase XIV closed the target-coverage blocker by replacing the non-computable
placeholder external lattice-gauge target with a DOI-backed Zenodo SU(2)
plaquette-chain benchmark. The reference campaign now compares every active
target against a computed observable.

The campaign is still not a particle-prediction proof. It has one active
quantitative mismatch and active fatal/high falsifiers. Phase XV should resolve
those claim blockers without weakening the evidence rules.

## Baseline

Latest Phase XIV campaign result:

- total targets: 9;
- matched targets: 9;
- missing targets: 0;
- passed matches: 8;
- failed matches: 1;
- score: 8 / 9 = 0.8888888888888888.

The remaining quantitative failure is:

- target: `bosonic-mode-2-imported-repo-benchmark`;
- observable: `bosonic-eigenvalue-ratio-2`;
- environment: `env-imported-repo-benchmark`;
- computed value: `0.98`;
- target value: `0.6`;
- computed uncertainty: `0.05`;
- target upper uncertainty: `0.05`;
- pull: `5.37401153701776`;
- threshold: `5.0`.

Phase XIV diagnosed this as not being a campaign wiring issue. The target
selector matches exactly one computed observable. The next question is whether
the target value has the wrong definition/normalization or whether this is a
real model disagreement.

## Phase XV Goal

At the end of Phase XV, the campaign should have:

- no missing quantitative targets;
- a provenance-backed decision on the imported repo benchmark mismatch;
- fatal/high falsifiers either repaired or carried as explicit claim blockers;
- a reproducible external-source workflow for the Zenodo benchmark that does not
  require committing generated downloads.

## Work Items

### P15-M1 Resolve Imported Benchmark Provenance

Find the source of target value `0.6` for
`bosonic-mode-2-imported-repo-benchmark`.

Definition of done:

- identify the artifact or calculation that produced `0.6`;
- verify whether it uses the same normalization as the computed observable
  `0.98`;
- if the target is wrong, replace it with a corrected target and record the
  correction;
- if the target is right, preserve the mismatch as a model disagreement and keep
  the scorecard failure active.

Do not widen uncertainty just to pass.

### P15-M2 Add Benchmark Mismatch Evidence Record

Move the current Markdown-only mismatch diagnosis into a structured checked-in
record that can be read by future reports.

Definition of done:

- add a structured mismatch/waiver/blocker artifact for active quantitative
  mismatches;
- include computed value, target value, uncertainty fields, pull, selector, and
  closure requirement;
- surface that artifact in the Phase V report or dossier path.

### P15-M3 Automate Zenodo Benchmark Reproduction

The Zenodo SU(2) benchmark is now checked in as an eigenvalue fixture with
source provenance. Phase XV should make reproduction less manual while still
keeping downloads and Python dependencies out of git.

Definition of done:

- add a script that downloads `su2lgt-main.zip` into `study-runs/`;
- verifies md5 and sha256;
- installs transient Python dependencies under `study-runs/`;
- regenerates the eigenvalue list and extracted observable;
- compares regenerated values to the checked-in fixture within a tight numeric
  tolerance.

### P15-M4 Reduce Or Preserve Active Falsifiers

The current campaign still has active fatal/high falsifiers. These block any
claim that the particle-prediction math is validated.

Definition of done:

- list every active fatal/high falsifier in the report;
- for each falsifier, classify it as repaired, expected negative evidence, or
  unresolved blocker;
- keep candidate claim escalation fail-closed while any fatal/high falsifier is
  active.

### P15-M5 Run A Clean Campaign And Summarize In Plain English

Run the full reference campaign after P15 changes.

Definition of done:

- campaign preflight passes;
- full campaign runs under `study-runs/`;
- generated outputs remain ignored;
- final summary states plainly:
  - which targets passed;
  - which targets failed;
  - whether any blockers remain;
  - whether the result is ready for real particle-data comparison.

## Recommended Execution Order

1. P15-M1: resolve the imported benchmark target provenance.
2. P15-M2: structure the mismatch/blocker artifact.
3. P15-M3: automate the Zenodo reproduction path.
4. P15-M4: classify active falsifiers.
5. P15-M5: run and summarize the campaign.

## Plain-English Success Criteria

Phase XV succeeds if a developer can run one campaign and see whether the
remaining failure is a bad target, a real disagreement, or a known blocker. It
does not succeed by hiding the failure.

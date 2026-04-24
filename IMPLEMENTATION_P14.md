# IMPLEMENTATION_P14.md

## Purpose

Phase XIV turns the Phase XIII evidence gate into a repair and evidence
production phase.

The Phase XIII test campaign did not validate particle predictions. It proved
that the application can now fail closed: missing computed observables and target
mismatches appear as explicit failures instead of disappearing from the score.

Phase XIV should use that stricter behavior to close the concrete gaps exposed
by the campaign.

## Test Campaign Baseline

Test run:

```bash
dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json \
  --out-dir study-runs/phase13_test_campaign_001
```

Preflight validation:

```bash
dotnet run --project apps/Gu.Cli -- validate-phase5-campaign-spec \
  --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json \
  --require-reference-sidecars
```

The preflight validator correctly failed because the target table declares
`bosonic-mode-1-external-lattice-gauge-benchmark`, but the observables artifact
does not contain a computed `bosonic-eigenvalue-ratio-1` for
`env-external-lattice-gauge-su2-2d-v1`.

The campaign can still be run without preflight validation for diagnosis. With
fail-closed target coverage enabled, the scorecard records:

- total targets: 9;
- matched targets: 8;
- missing targets: 1;
- passed matches: 7;
- failed matches: 2;
- overall score: 7 / 9 = 0.7777777777777778.

The two failures are:

- `bosonic-mode-2-imported-repo-benchmark`: computed value `0.98`, target value
  `0.6`, pull `5.374`, failed at the 5-sigma threshold;
- `bosonic-mode-1-external-lattice-gauge-benchmark`: no computed observable was
  available for the requested external imported environment, recorded as a
  target coverage failure.

Other campaign status:

- branch study is mixed: 3 robust quantities, 2 fragile quantities;
- refinement study reports 5 convergent quantities;
- falsification summary has 1 active fatal falsifier and 4 active high
  falsifiers;
- geometry evidence tier remains `toy-control`;
- the report explicitly says this is not a real-world experimental measurement
  or physical prediction.

## Phase XIV Goal

At the end of Phase XIV, the reference campaign should either:

- pass preflight validation with every external target covered by a computed
  observable; or
- fail preflight with explicit waiver/blocker artifacts that explain why a target
  is not yet computable.

Success is not "matching particles." Success is a clean, reproducible answer
about what the pipeline can and cannot compare.

## Implemented Repository Slice

Phase XIV now implements the repository-side repair work from the test campaign:

- `validate-quantitative` accepts
  `--environment-records <env1.json,env2.json,...>` so standalone quantitative
  validation can resolve `targetEnvironmentTier` and `targetEnvironmentId`
  selectors the same way the full campaign does;
- `validate-quantitative --out <path>` creates the output directory if needed,
  which keeps trial outputs easy to write under `study-runs/`;
- Phase V Markdown reports include total targets, matched targets, missing
  targets, missing target labels, and an explicit claim-blocked line when fatal
  or high falsifiers are active;
- campaign specs support `targetCoverageBlockersPath`, a checked-in blocker
  table for targets that are intentionally not computable yet;
- the reference campaign now includes
  `config/target_coverage_blockers.json` for targets that are explicitly blocked;
- after the Zenodo replacement target was added, that blocker table is empty
  because every active target has a matching computed observable;
- `config/imported_benchmark_mismatch.md` records the imported benchmark
  mismatch as a preserved negative result to investigate, not something to hide
  by widening uncertainty.

Verified commands:

```bash
dotnet run --project apps/Gu.Cli -- validate-phase5-campaign-spec \
  --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json \
  --require-reference-sidecars

dotnet run --project apps/Gu.Cli -- validate-quantitative \
  --observables studies/phase5_su2_branch_refinement_env_validation/config/observables.json \
  --targets studies/phase5_su2_branch_refinement_env_validation/config/external_targets.json \
  --environment-records studies/phase5_su2_branch_refinement_env_validation/config/env_toy_record.json,studies/phase5_su2_branch_refinement_env_validation/config/env_structured_4x4_record.json,studies/phase5_su2_branch_refinement_env_validation/config/env_imported_repo_benchmark.json,studies/phase5_su2_branch_refinement_env_validation/config/env_zenodo_su2_plaquette_chain.json \
  --fail-closed-target-coverage \
  --out study-runs/phase14_test_campaign_001/standalone_scorecard.json

dotnet run --project apps/Gu.Cli -- run-phase5-campaign \
  --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json \
  --out-dir study-runs/phase14_test_campaign_001/full_campaign \
  --validate-first
```

After the Zenodo replacement target, the full campaign reports 9 total targets,
9 matched targets, 0 missing targets, 8 passed matches, and 1 failed match. The
remaining quantitative failure is the imported repo benchmark mismatch.

## Next Real Work Results

Phase XIV then investigated the two remaining quantitative blockers.

### P14-M1 External Lattice Gauge Observable

The target still cannot be covered honestly from checked-in artifacts. The
campaign contains `env_external_lattice_gauge.json`, but that file is a summarized
`EnvironmentRecord`, not the external mesh or field dataset. It has topology
counts and provenance strings, but no local source mesh path, vertex/face data,
field data, or spectrum data. The `sourceSpec` value is
`external:lattice-gauge-su2-2d-v1`, which is not a re-importable repository file.

Because of that, adding a computed `bosonic-eigenvalue-ratio-1` for
`env-external-lattice-gauge-su2-2d-v1` would fabricate a result. The detailed
investigation is recorded in `config/external_lattice_observable_gap.md`.

The active campaign now supersedes that placeholder target with a DOI-backed
external benchmark from Zenodo record `10.5281/zenodo.16739090`. The checked-in
benchmark records:

- source archive: `su2lgt-main.zip`;
- md5: `7c09478c1d2b2816d416e564695d0bc0`;
- sha256:
  `f1d566b97ad3fee275afd36784edbf8f2067e4bed4d63bcc09abfedd8db0eb8c`;
- benchmark case: periodic SU(2) plaquette chain, `P=4`, `jmax=0.5`, `k=0`,
  `g^2=1.5`;
- observable: adjacent low-energy gap ratio at index `0`;
- value: `0.3889179657576827`.

The target coverage blocker table is now empty because all active targets have
matching computed observables.

### P14-M2 Imported Benchmark Mismatch

The imported benchmark mismatch is not caused by target matching or environment
selection. The target requests `bosonic-eigenvalue-ratio-2` on
`env-imported-repo-benchmark` with tier `imported`, and exactly one computed
observable matches it:

- computed value: `0.98`;
- computed uncertainty: `0.05`;
- target value: `0.6`;
- upper target uncertainty: `0.05`;
- pull: `abs(0.98 - 0.6) / sqrt(0.05^2 + 0.05^2) = 5.37401153701776`.

That means the failure is either a target-definition/normalization problem or a
genuine model disagreement. It should not be repaired by widening uncertainty.
The diagnosis is recorded in `config/imported_benchmark_mismatch.md`.

### P14-M7 DOI-Backed Spectrum Extraction

The CLI now includes:

```bash
dotnet run --project apps/Gu.Cli -- extract-spectrum-observable \
  --eigenvalues studies/phase5_su2_branch_refinement_env_validation/config/zenodo_su2_plaquette_chain_eigenvalues.json \
  --observable-id bosonic-eigenvalue-ratio-1 \
  --environment-id env-zenodo-su2-plaquette-chain-p4-j0.5-g1.5-v1 \
  --branch-id zenodo-su2lgt-periodic-k0 \
  --refinement-level P4-j0.5-g1.5 \
  --gap-index 0 \
  --uncertainty 0.001
```

This converts an externally generated eigenvalue list into a
`QuantitativeObservableRecord` using
`min(E[i+1]-E[i], E[i+2]-E[i+1]) / max(E[i+1]-E[i], E[i+2]-E[i+1])`.

## Work Items

### P14-M1 Cover The External Lattice Gauge Target

Generate or import the missing computed observable for:

- observable: `bosonic-eigenvalue-ratio-1`;
- environment: `env-external-lattice-gauge-su2-2d-v1`;
- target: `bosonic-mode-1-external-lattice-gauge-benchmark`.

Definition of done:

- `observables.json` contains the matching computed observable with uncertainty;
- `validate-phase5-campaign-spec --require-reference-sidecars` passes this
  target coverage check;
- the scorecard no longer contains a missing-computed-observable record for this
  target.

Status: superseded in the active campaign. The placeholder target remains
documented as non-computable from checked-in artifacts; the active external
benchmark is now the DOI-backed Zenodo SU(2) plaquette-chain target.

### P14-M2 Investigate The Imported Benchmark Mismatch

The imported repo benchmark for `bosonic-eigenvalue-ratio-2` fails with pull
`5.374`.

Definition of done:

- determine whether the mismatch comes from the target value, environment
  selection, observable extraction, uncertainty budget, or real model
  disagreement;
- preserve a negative result if the mismatch is genuine;
- do not widen uncertainty just to make the comparison pass.

### P14-M3 Add Environment Records To Standalone Quantitative Validation

The full campaign path passes environment records into quantitative validation.
The standalone `validate-quantitative` command currently does not, so
environment-tier selectors cannot match there.

Required CLI addition:

```bash
gu validate-quantitative \
  --observables <observables.json> \
  --targets <external_targets.json> \
  --environment-records <env1.json,env2.json,...> \
  --fail-closed-target-coverage
```

Definition of done:

- standalone quantitative validation produces the same target coverage result as
  the full campaign for the same inputs;
- tests cover targetEnvironmentTier and targetEnvironmentId selectors from the
  CLI path.

### P14-M4 Surface Target Coverage In Human Reports

The JSON scorecard now contains target coverage counts, but the Markdown report
only shows pass/fail match counts.

Definition of done:

- `phase5_report.md` includes total targets, matched targets, missing targets,
  and missing target labels;
- missing target coverage appears in the report as a failure, not an omission.

### P14-M5 Reduce Active Falsifiers Or Preserve Them As Blockers

The test campaign reports:

- 1 active fatal falsifier;
- 4 active high falsifiers.

Definition of done:

- each active falsifier is either repaired by better upstream evidence or carried
  into the report as a blocking condition;
- candidate claim escalation remains fail-closed while fatal/high falsifiers are
  active.

### P14-M6 Keep Generated Evidence Out Of Git

Continue writing trial campaign outputs to `study-runs/`.

Definition of done:

- no generated campaign output is committed;
- any checked-in study file is a config, fixture, or documentation file;
- large transient result folders remain ignored.

## Recommended Execution Order

1. P14-M3: make standalone quantitative validation environment-aware.
2. P14-M4: expose target coverage in Markdown reports.
3. P14-M1: compute or explicitly block the external lattice-gauge observable.
4. P14-M2: diagnose the imported benchmark mismatch.
5. P14-M5: repair or preserve active falsifiers.
6. P14-M6: keep the study output hygiene intact.

## Plain-English Success Criteria

Phase XIV succeeds when a developer can run the reference campaign and see, in
plain language:

- which targets were actually compared;
- which target was missing and why, if any;
- which computed values missed their targets;
- which falsifiers still block claims;
- why the result is or is not ready for external particle-data comparison.

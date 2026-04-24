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

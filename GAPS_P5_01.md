# GAPS_P5_01.md

## Purpose

This file is the implementation handoff for closing the remaining practical
Phase V gaps. Claude Code should treat this document as the binding design
specification for the work described here. Do not invent alternate contracts if
this file already makes a decision.

Primary source documents reviewed:

- `PHASE_5_OPEN_ISSUES.md`
- `IMPLEMENTATION_P5.md`
- `ARCH_P5.md`
- `ASSUMPTIONS.md`
- current code under `apps/Gu.Cli/Program.cs`
- current code under `src/Gu.Phase3.*` and `src/Gu.Phase5.*`

This document intentionally resolves design branches left open in the other
docs. Where `PHASE_5_OPEN_ISSUES.md` says "either X or Y", this file chooses
one.

---

## What is already true

These points are already true in the current tree and must not be regressed:

1. Individual Phase V CLI subcommands exist:
   - `branch-robustness`
   - `refinement-study`
   - `import-environment`
   - `build-structured-environment`
   - `validate-quantitative`
   - `build-validation-dossier`
2. `Phase5CampaignRunner` exists in code.
3. `SolveRunClassification` exists and is persisted for `gu run` / `gu solve`.
4. `compute-spectrum` already persists and reloads more background context than
   the older Phase III path did.

Do not spend time re-implementing those from scratch. Focus on the gaps below.

---

## Binding decisions

These decisions are final for this gap-closure pass.

### D-001 Manifest resolution contract for `solve-backgrounds`

Choose `ManifestSearchPaths` on `BackgroundStudySpec`.

Do not implement `manifest-registry.json`.

Resolution order for each `BackgroundSpec.BranchManifestId`:

1. `--manifest <path>`:
   valid only when all specs in the study use the same `BranchManifestId`
2. `--manifest-dir <dir>`:
   search for `<branchManifestId>.branch.json`, then `<branchManifestId>.json`
3. `BackgroundStudySpec.ManifestSearchPaths`:
   each path is resolved relative to the study JSON file if relative
4. hard error

No implicit inline default manifest is allowed once this work is complete.

### D-002 Persist run classification in `BackgroundRecord`

Choose embedded persistence, not a standalone `classifications.json`.

Add:

- `BackgroundRecord.RunClassification`
- `BackgroundRecord.ConsumedManifestArtifactRef`

Do not add a second source of truth for the same classification data.

### D-003 Per-background manifest artifacts are authoritative

`solve-backgrounds` must write:

- `background_states/{backgroundId}_manifest.json`

`compute-spectrum` must load that file first. Only if it is absent may it fall
back to:

1. run-folder manifest
2. legacy default behavior for backward compatibility

New tests must cover this precedence explicitly.

### D-004 Phase V CLI is analysis-first, not live-solver-first

Do not try to make Phase V CLI commands directly solve new physics states inside
the command handlers. The CLI should analyze persisted or explicitly supplied
artifacts.

This means:

- `branch-robustness` continues to consume explicit values JSON
- `refinement-study` must be changed to consume explicit values JSON
- `run-phase5-campaign` must consume explicit artifact paths from its spec

This matches the existing architectural role of Phase V as a validation layer.

### D-005 Add a top-level `run-phase5-campaign` CLI command

Add:

- `gu run-phase5-campaign --spec <campaign.json> --out-dir <dir>`

This is the only approved top-level reproducible campaign command for M53.

`Phase5CampaignRunner` reproduction strings must use this exact command form.

### D-006 Keep two dossier types and emit both

Do not merge the two dossier types.

Use them as follows:

- `ValidationDossier`:
  provenance/evidence-tier envelope, G-006 freshness enforcement
- `Phase5ValidationDossier`:
  technical Phase V evidence bundle with branch/refinement/environment/
  falsification/escalation content

`run-phase5-campaign` must emit both.

### D-007 Observation-chain evidence will use one summary record type

Do not add three separate top-level projects or a large observation subsystem.

Add one typed summary record in `Gu.Phase5.Dossiers`:

- `ObservationChainRecord`

It must carry:

- `candidateId`
- `primarySourceId`
- `observableId`
- `nativeArtifactRef`
- `observedArtifactRef`
- `extractionArtifactRef`
- `auxiliaryModelId`
- `completenessStatus`
- `sensitivityScore`
- `auxiliaryModelSensitivity`
- `passed`
- `notes`
- `provenance`

`Phase5ValidationDossier` will store a collection of these records under
`ObservationChainSummary`.

Join rule is binding:

1. `candidateId` must equal `UnifiedParticleRecord.ParticleId`
2. `primarySourceId` must equal `UnifiedParticleRecord.PrimarySourceId`

Escalation and falsifier logic must join on `candidateId` first. If needed,
`primarySourceId` is the secondary join key for observable-level evidence.

### D-008 Deferred falsifier inputs stay in `Gu.Phase5.Falsification`

Add these record types there:

- `EnvironmentVarianceRecord`
- `RepresentationContentRecord`
- `CouplingConsistencyRecord`

Do not create a new Phase V project for them.

### D-009 Fix `refinement-study` by requiring values input

Do not keep the empty executor placeholder.

New command shape:

- `gu refinement-study --spec <spec.json> --values <values.json> [--out <result.json>]`

The values file is required.

### D-010 Extend `Phase5CampaignSpec` instead of inventing another campaign spec

Add these fields to `Phase5CampaignSpec`:

- `branchQuantityValuesPath`
- `refinementValuesPath`
- `observablesPath`
- `environmentRecordPaths`
- `registryPath`
- `observationChainPath`
- `environmentVariancePath`
- `representationContentPath`
- `couplingConsistencyPath`

Keep existing fields.

### D-011 Non-Gaussian support uses string models

Add `DistributionModel` as a string field.

Allowed values in this pass:

- `gaussian`
- `gaussian-asymmetric`
- `student-t`

### D-012 Use `ShiabVariantId`, not `ShibaVariantId`

If documentation contains the typo `ShibaVariantId`, do not reproduce it in
code. Use `ShiabVariantId` everywhere.

---

## Gap order

Claude should implement the gaps in this order:

1. G-001 and G-002 partial closures in `solve-backgrounds`
2. stale M53 study script and top-level campaign CLI
3. real `refinement-study` values contract
4. dual dossier emission
5. observation-chain summary
6. deferred falsifier activation
7. dual mesh parameters for convergence
8. Phase III background bridge
9. imported environment contract
10. target realism and non-Gaussian scoring
11. Shiab expansion

This order is mandatory because later steps depend on earlier artifact
contracts.

---

## Exact work packages

### WP-1 `solve-backgrounds` manifest and classification closure

#### Files to change

- `src/Gu.Phase3.Backgrounds/BackgroundStudySpec.cs`
- `src/Gu.Phase3.Backgrounds/BackgroundRecord.cs`
- `src/Gu.Phase3.Backgrounds/BackgroundAtlasSerializer.cs`
- `apps/Gu.Cli/Program.cs`
- any relevant JSON schemas under `schemas/`

#### Required code changes

1. Add to `BackgroundStudySpec`:

```csharp
[JsonPropertyName("manifestSearchPaths")]
public IReadOnlyList<string>? ManifestSearchPaths { get; init; }
```

2. Add to `BackgroundRecord`:

```csharp
[JsonPropertyName("runClassification")]
public SolveRunClassification? RunClassification { get; init; }

[JsonPropertyName("consumedManifestArtifactRef")]
public string? ConsumedManifestArtifactRef { get; init; }
```

3. Update `solve-backgrounds` usage to:

```text
gu solve-backgrounds <study.json> [--output <dir>] [--lie-algebra su2|su3] [--manifest <path>] [--manifest-dir <dir>]
```

4. For each spec in the study:
   - resolve the manifest using D-001
   - classify the solve attempt with `SolveRunClassification`
   - persist `background_states/{backgroundId}_manifest.json`
   - store `RunClassification` and `ConsumedManifestArtifactRef` in the record

5. Remove inline default-manifest fallback from the success path.
   Legacy fallback may remain behind an explicit compatibility switch only if
   needed, but default behavior must be hard error.

#### Required tests

- add tests under `tests/Gu.Phase3.Backgrounds.Tests/`
- add CLI-level tests if existing test style supports it

Must cover:

1. `--manifest` works for single-manifest studies
2. `--manifest` hard-fails for mixed-manifest studies
3. `--manifest-dir` resolves `<id>.branch.json`
4. `ManifestSearchPaths` resolves relative to the study JSON file
5. missing manifest hard-fails
6. each `BackgroundRecord` contains `RunClassification`
7. each `BackgroundRecord` contains `ConsumedManifestArtifactRef`
8. per-background manifest file is actually written

---

### WP-2 `compute-spectrum` manifest precedence closure

#### Files to change

- `apps/Gu.Cli/Program.cs`
- tests under `tests/Gu.Phase3.Spectra.Tests/`

#### Required code changes

Manifest load precedence for `compute-spectrum`:

1. `background_states/{backgroundId}_manifest.json`
2. `RunFolderLayout.BranchManifestFile`
3. legacy generated default, with diagnostic note saying fallback was used

Diagnostic notes must explicitly report which manifest source was consumed.

#### Required tests

1. a run folder with two different per-background manifests yields different
   operator branch choices for different background IDs
2. per-background manifest overrides run-folder manifest
3. missing per-background manifest falls back to run-folder manifest
4. missing both manifests emits diagnostic fallback note

---

### WP-3 Repair and modernize the M53 study path

#### Files to change

- `apps/Gu.Cli/Program.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignSpec.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignRunner.cs`
- add `src/Gu.Phase5.Reporting/Phase5CampaignArtifactLoader.cs`
- `studies/phase5_su2_branch_refinement_env_validation/run_study.sh`
- optionally add `studies/phase5_su2_branch_refinement_env_validation/config/campaign.json`

#### Required code changes

Add CLI dispatch:

```text
gu run-phase5-campaign --spec <campaign.json> --out-dir <dir>
```

Extend `Phase5CampaignSpec` with:

```csharp
[JsonPropertyName("branchQuantityValuesPath")]
public required string BranchQuantityValuesPath { get; init; }

[JsonPropertyName("refinementValuesPath")]
public required string RefinementValuesPath { get; init; }

[JsonPropertyName("observablesPath")]
public required string ObservablesPath { get; init; }

[JsonPropertyName("environmentRecordPaths")]
public required IReadOnlyList<string> EnvironmentRecordPaths { get; init; }

[JsonPropertyName("registryPath")]
public required string RegistryPath { get; init; }

[JsonPropertyName("observationChainPath")]
public string? ObservationChainPath { get; init; }

[JsonPropertyName("environmentVariancePath")]
public string? EnvironmentVariancePath { get; init; }

[JsonPropertyName("representationContentPath")]
public string? RepresentationContentPath { get; init; }

[JsonPropertyName("couplingConsistencyPath")]
public string? CouplingConsistencyPath { get; init; }
```

Add a loader class that:

1. loads the branch values file
2. loads the refinement values file
3. loads environment records
4. loads observables
5. loads external targets
6. loads the unified registry
7. loads observation-chain records when `observationChainPath` is present
8. loads environment-variance records when `environmentVariancePath` is present
9. loads representation-content records when `representationContentPath` is present
10. loads coupling-consistency records when `couplingConsistencyPath` is present

`run_study.sh` must stop using stale positional command forms and must call the
top-level `run-phase5-campaign`.

#### Campaign artifact layout

`run-phase5-campaign --out-dir <dir>` must emit:

```text
<dir>/
  branch/branch_robustness_record.json
  convergence/refinement_study_result.json
  quantitative/consistency_scorecard.json
  falsification/falsifier_summary.json
  dossiers/phase5_validation_dossier.json
  dossiers/validation_dossier.json
  dossiers/study_manifest.json
  reports/phase5_report.json
  reports/phase5_report.md
```

Optional copies of source input files may also be placed under:

```text
<dir>/inputs/
```

but these are not authoritative outputs.

`dossiers/study_manifest.json` is a JSON array of `StudyManifest`, not a single
object. For the standard Phase V campaign it must contain exactly two entries:

1. the positive or mixed study manifest
2. the negative-result study manifest

#### Required tests

Under `tests/Gu.Phase5.Reporting.Tests/` add:

1. campaign CLI integration test producing the full output tree
2. reproduction command in manifest/report uses `run-phase5-campaign`
3. typed dossier and provenance dossier are both written

---

### WP-4 Replace placeholder `refinement-study` CLI behavior

#### Files to change

- `apps/Gu.Cli/Program.cs`
- add `src/Gu.Phase5.Convergence/RefinementQuantityValueTable.cs`
- optionally add helper loader in `src/Gu.Phase5.Convergence/`
- `schemas/refinement_study.schema.json`
- tests under `tests/Gu.Phase5.Convergence.Tests/`

#### Required code changes

New CLI usage:

```text
gu refinement-study --spec <spec.json> --values <values.json> [--out <result.json>]
```

Add a values-file type:

```csharp
public sealed class RefinementQuantityValueTable
{
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    [JsonPropertyName("levels")]
    public required IReadOnlyList<RefinementQuantityValueLevel> Levels { get; init; }
}

public sealed class RefinementQuantityValueLevel
{
    [JsonPropertyName("levelId")]
    public required string LevelId { get; init; }

    [JsonPropertyName("solverConverged")]
    public bool SolverConverged { get; init; } = true;

    [JsonPropertyName("quantities")]
    public required IReadOnlyDictionary<string, double> Quantities { get; init; }
}
```

`refinement-study` must load this file and feed the runner using level-matched
quantity dictionaries.

Do not keep the empty executor placeholder.

#### Required tests

1. CLI rejects missing `--values`
2. level IDs in values file must match spec levels
3. non-empty values produce non-empty continuum estimates
4. `solverConverged = false` produces failure records

---

### WP-5 Emit both dossier types

#### Files to change

- `src/Gu.Phase5.Dossiers/Phase5ValidationDossier.cs`
- `src/Gu.Phase5.Dossiers/Phase5DossierAssembler.cs`
- `src/Gu.Phase5.Dossiers/DossierAssembler.cs`
- `src/Gu.Phase5.Reporting/Phase5CampaignRunner.cs`
- tests under `tests/Gu.Phase5.Dossiers.Tests/`

#### Required code changes

`run-phase5-campaign` must emit:

1. `dossiers/phase5_validation_dossier.json`
2. `dossiers/validation_dossier.json`
3. `dossiers/study_manifest.json`

The simple dossier remains the freshness/evidence-tier gate.
The typed dossier remains the scientific content bundle.

Do not collapse them.

#### Required tests

1. both dossier files exist after a campaign run
2. `validation_dossier.json` is acceptable only when manifests carry
   reproducibility metadata
3. `phase5_validation_dossier.json` contains branch/refinement/falsifier content

---

### WP-6 Add observation-chain summary to the typed dossier

#### Files to change

- add `src/Gu.Phase5.Dossiers/ObservationChainRecord.cs`
- `src/Gu.Phase5.Dossiers/Phase5ValidationDossier.cs`
- `src/Gu.Phase5.Dossiers/Phase5DossierAssembler.cs`
- tests under `tests/Gu.Phase5.Dossiers.Tests/`

#### Required code changes

Add to `Phase5ValidationDossier`:

```csharp
[JsonPropertyName("observationChainSummary")]
public IReadOnlyList<ObservationChainRecord>? ObservationChainSummary { get; init; }
```

Observation gate rule:

`ObservationChainValid` passes for a candidate if at least one corresponding
`ObservationChainRecord` satisfies all of:

1. `CompletenessStatus == "complete"`
2. `Passed == true`
3. `SensitivityScore <= 0.3`
4. `AuxiliaryModelSensitivity <= 0.3`

If no record exists, the gate fails.

Record-shape requirements for `ObservationChainRecord`:

```csharp
[JsonPropertyName("candidateId")]
public required string CandidateId { get; init; }

[JsonPropertyName("primarySourceId")]
public required string PrimarySourceId { get; init; }

[JsonPropertyName("observableId")]
public required string ObservableId { get; init; }
```

Join rule:

- `CandidateId` joins to `UnifiedParticleRecord.ParticleId`
- `PrimarySourceId` joins to `UnifiedParticleRecord.PrimarySourceId`

If both keys are present but disagree with the registry candidate, treat the
record as invalid input and fail fast.

#### Required tests

1. no observation record -> gate fails
2. complete low-sensitivity record -> gate passes
3. high sensitivity record -> gate fails

---

### WP-7 Activate the deferred falsifiers

#### Files to change

- add `src/Gu.Phase5.Falsification/EnvironmentVarianceRecord.cs`
- add `src/Gu.Phase5.Falsification/RepresentationContentRecord.cs`
- add `src/Gu.Phase5.Falsification/CouplingConsistencyRecord.cs`
- `src/Gu.Phase5.Falsification/FalsificationPolicy.cs`
- `src/Gu.Phase5.Falsification/FalsifierEvaluator.cs`
- tests under `tests/Gu.Phase5.Falsification.Tests/`

#### Required code changes

Add policy fields:

```csharp
[JsonPropertyName("observationInstabilityThreshold")]
public double ObservationInstabilityThreshold { get; init; } = 0.3;

[JsonPropertyName("representationContentThreshold")]
public double RepresentationContentThreshold { get; init; } = 0.2;

[JsonPropertyName("couplingInconsistencyThreshold")]
public double CouplingInconsistencyThreshold { get; init; } = 0.3;
```

Extend `FalsifierEvaluator.Evaluate(...)` to accept:

- `IReadOnlyList<ObservationChainRecord>? observationRecords`
- `IReadOnlyList<EnvironmentVarianceRecord>? environmentVarianceRecords`
- `IReadOnlyList<RepresentationContentRecord>? representationRecords`
- `IReadOnlyList<CouplingConsistencyRecord>? couplingRecords`

Trigger rules:

1. `ObservationInstability`:
   `SensitivityScore > ObservationInstabilityThreshold`
2. `EnvironmentInstability`:
   `RelativeStdDev > EnvironmentInstabilityThreshold`
3. `RepresentationContent`:
   `MissingRequiredCount > 0` or
   `StructuralMismatchScore > RepresentationContentThreshold`
4. `CouplingInconsistency`:
   `RelativeSpread > CouplingInconsistencyThreshold`

Severity:

- `ObservationInstability`: high
- `EnvironmentInstability`: high
- `RepresentationContent`: fatal if `MissingRequiredCount > 0`, else high
- `CouplingInconsistency`: high

#### Required tests

Add one positive and one negative test for each newly active falsifier type.

---

### WP-8 Correct the convergence parameterization

#### Files to change

- `src/Gu.Phase5.Convergence/RefinementLevel.cs`
- `src/Gu.Phase5.Convergence/RichardsonExtrapolator.cs`
- `src/Gu.Phase5.Convergence/RefinementStudyRunner.cs`
- `schemas/refinement_study.schema.json`
- tests under `tests/Gu.Phase5.Convergence.Tests/`

#### Required code changes

Replace:

- `meshParameter`

with:

```csharp
[JsonPropertyName("meshParameterX")]
public required double MeshParameterX { get; init; }

[JsonPropertyName("meshParameterF")]
public required double MeshParameterF { get; init; }

[JsonIgnore]
public double EffectiveMeshParameter => Math.Max(MeshParameterX, MeshParameterF);
```

Backward compatibility:

- legacy `meshParameter` must be accepted during deserialization
- when legacy `meshParameter` is present, set both X and F to that value
- new serialization must emit only `meshParameterX` and `meshParameterF`

Use `EffectiveMeshParameter` everywhere Richardson currently uses `MeshParameter`.

#### Required tests

1. legacy single-h input still works
2. anisotropic refinement uses `max(h_X, h_F)`
3. reported continuum values are unchanged for `h_X == h_F`

---

### WP-9 Bridge Phase III backgrounds into Phase V branch studies

#### Files to change

- add `src/Gu.Phase5.Reporting/BackgroundRecordBranchVariantBridge.cs`
- `studies/phase5_su2_branch_refinement_env_validation/config/*`
- optionally add a new reference-study input file for bridged variants
- tests under `tests/Gu.Phase5.Reporting.Tests/`

#### Required code changes

Add a bridge that produces Phase V branch-study values from persisted Phase III
background artifacts.

Minimum responsibility:

1. load a `BackgroundRecord`
2. load its persisted `omega`, `A0`, and manifest
3. derive a stable branch-variant identity from the artifact set
4. export branch-study quantity values

Do not keep the analytic `A0` family as the default evidence path in the main
reference study once the bridge is available.

---

### WP-10 Make imported environments real

#### Files to change

- `src/Gu.Phase5.Environments/EnvironmentImportSpec.cs`
- `src/Gu.Phase5.Environments/EnvironmentImporter.cs`
- `src/Gu.Phase5.Environments/EnvironmentRecord.cs`
- environment schemas
- tests under `tests/Gu.Phase5.Environments.Tests/`

#### Required code changes

Add to `EnvironmentImportSpec`:

- `datasetId`
- `sourceHash`
- `conversionVersion`

Add corresponding persisted provenance to `EnvironmentRecord`.

Supported source formats in this pass:

- `gu-json`
- `simplicial-json`

Keep those formats, but require external provenance fields so imported evidence
is not just a relabeled structured environment.

#### Required tests

1. imported environment records preserve dataset provenance
2. source hash round-trips
3. missing provenance fields fail validation for imported tier

---

### WP-11 Upgrade quantitative targets and uncertainty models

#### Files to change

- `src/Gu.Phase5.QuantitativeValidation/ExternalTarget.cs`
- `src/Gu.Phase5.QuantitativeValidation/QuantitativeObservableRecord.cs`
- `src/Gu.Phase5.QuantitativeValidation/QuantitativeUncertainty.cs`
- `src/Gu.Phase5.QuantitativeValidation/TargetMatcher.cs`
- `src/Gu.Phase5.QuantitativeValidation/TargetMatchRecord.cs`
- tests under `tests/Gu.Phase5.QuantitativeValidation.Tests/`

#### Required code changes

Add to `ExternalTarget`:

```csharp
[JsonPropertyName("distributionModel")]
public string DistributionModel { get; init; } = "gaussian";

[JsonPropertyName("uncertaintyLower")]
public double? UncertaintyLower { get; init; }

[JsonPropertyName("uncertaintyUpper")]
public double? UncertaintyUpper { get; init; }

[JsonPropertyName("studentTDegreesOfFreedom")]
public double? StudentTDegreesOfFreedom { get; init; }
```

Add to `QuantitativeObservableRecord`:

```csharp
[JsonPropertyName("distributionModel")]
public string DistributionModel { get; init; } = "gaussian";
```

Add to `QuantitativeUncertainty`:

```csharp
[JsonPropertyName("totalUncertaintyLower")]
public double? TotalUncertaintyLower { get; init; }

[JsonPropertyName("totalUncertaintyUpper")]
public double? TotalUncertaintyUpper { get; init; }
```

Target-matching rules:

1. `gaussian`:
   existing pull
2. `gaussian-asymmetric`:
   choose lower or upper sigma based on sign of residual
3. `student-t`:
   compute Student-t normalized residual using declared degrees of freedom

#### Required tests

1. existing Gaussian tests still pass
2. asymmetric target uses the correct side sigma
3. Student-t model serializes and scores deterministically

---

### WP-12 Expand Shiab variation after evidence contracts are stable

#### Files to change

- `src/Gu.Phase5.Convergence/RefinementStudySpec.cs`
- `studies/phase5_su2_branch_refinement_env_validation/config/*`
- any branch operator registry wiring required upstream
- tests under the relevant Phase V projects

#### Required code changes

Add to `RefinementStudySpec`:

```csharp
[JsonPropertyName("shiabVariantId")]
public string? ShiabVariantId { get; init; }
```

Once a non-identity Shiab operator exists upstream, use this field in the
reference study and convergence tests.

Do not spell the property `shibaVariantId`.

---

## Campaign spec contract

After WP-3, `Phase5CampaignSpec` is authoritative and must contain:

```json
{
  "campaignId": "phase5-su2-branch-refinement-env-validation",
  "schemaVersion": "1.1.0",
  "branchFamilySpec": { "...": "existing shape" },
  "refinementSpec": { "...": "existing shape" },
  "environmentCampaignSpec": { "...": "existing shape" },
  "externalTargetTablePath": "config/external_targets.json",
  "branchQuantityValuesPath": "config/branch_quantity_values.json",
  "refinementValuesPath": "config/refinement_values.json",
  "observablesPath": "config/observables.json",
  "environmentRecordPaths": [
    "artifacts/environments/env-toy-2d-trivial.json",
    "artifacts/environments/env-structured-4x4.json"
  ],
  "registryPath": "artifacts/registry/unified_registry.json",
  "observationChainPath": "config/observation_chain.json",
  "environmentVariancePath": "config/environment_variance.json",
  "representationContentPath": "config/representation_content.json",
  "couplingConsistencyPath": "config/coupling_consistency.json",
  "calibrationPolicy": { "...": "existing shape" },
  "falsificationPolicy": { "...": "existing shape" },
  "provenance": { "...": "existing shape" }
}
```

Paths may be relative to the campaign spec file.

The four new evidence-sidecar paths are optional for early campaign runs, but
once WP-6 and WP-7 are implemented the standard checked-in Phase V campaign
must supply them.

---

## Refinement values contract

After WP-4, `refinementValuesPath` must point to:

```json
{
  "studyId": "phase5-refinement-study",
  "levels": [
    {
      "levelId": "level-0",
      "solverConverged": true,
      "quantities": {
        "boson-eigenvalue-ratio-1": 0.12,
        "fermion-dirac-eigenvalue-1": 0.07
      }
    },
    {
      "levelId": "level-1",
      "solverConverged": true,
      "quantities": {
        "boson-eigenvalue-ratio-1": 0.11,
        "fermion-dirac-eigenvalue-1": 0.06
      }
    }
  ]
}
```

This is the only approved CLI refinement input contract for this pass.

---

## Concrete JSON examples

Claude should use the following example shapes as the authoritative JSON
contracts for the new campaign sidecar files.

### `observation_chain.json`

This file is a JSON array of `ObservationChainRecord`.

```json
[
  {
    "candidateId": "boson-candidate-001",
    "primarySourceId": "mode-family-boson-001",
    "observableId": "boson-eigenvalue-ratio-1",
    "nativeArtifactRef": "artifacts/spectra/bg-001_spectrum.json",
    "observedArtifactRef": "artifacts/observables/mode_signatures/mode-0001.json",
    "extractionArtifactRef": "artifacts/quantitative/observables.json#boson-eigenvalue-ratio-1",
    "auxiliaryModelId": "ratio-extractor-v1",
    "completenessStatus": "complete",
    "sensitivityScore": 0.12,
    "auxiliaryModelSensitivity": 0.08,
    "passed": true,
    "notes": "Observation chain complete; low sensitivity under extractor perturbations.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  },
  {
    "candidateId": "fermion-candidate-002",
    "primarySourceId": "mode-family-fermion-002",
    "observableId": "fermion-dirac-eigenvalue-1",
    "nativeArtifactRef": "artifacts/fermions/fermion_modes.json",
    "observedArtifactRef": "artifacts/observables/mode_signatures/mode-0102.json",
    "extractionArtifactRef": "artifacts/quantitative/observables.json#fermion-dirac-eigenvalue-1",
    "auxiliaryModelId": "dirac-eigenvalue-extractor-v1",
    "completenessStatus": "complete",
    "sensitivityScore": 0.44,
    "auxiliaryModelSensitivity": 0.35,
    "passed": false,
    "notes": "High observation sensitivity; should fail ObservationChainValid.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  }
]
```

### `environment_variance.json`

This file is a JSON array of `EnvironmentVarianceRecord`.

```json
[
  {
    "targetId": "boson-eigenvalue-ratio-1",
    "branchId": "V1",
    "environmentIds": [
      "env-toy-2d-trivial",
      "env-structured-4x4"
    ],
    "meanValue": 0.115,
    "standardDeviation": 0.006,
    "relativeStdDev": 0.0522,
    "passed": true,
    "notes": "Stable across toy and structured tiers.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  },
  {
    "targetId": "fermion-dirac-eigenvalue-1",
    "branchId": "V2",
    "environmentIds": [
      "env-toy-2d-trivial",
      "env-structured-4x4"
    ],
    "meanValue": 0.42,
    "standardDeviation": 0.19,
    "relativeStdDev": 0.4524,
    "passed": false,
    "notes": "Environment instability exceeds threshold.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  }
]
```

### `representation_content.json`

This file is a JSON array of `RepresentationContentRecord`.

```json
[
  {
    "targetId": "boson-candidate-001",
    "branchId": "V1",
    "expectedModeCount": 3,
    "actualModeCount": 3,
    "missingRequiredCount": 0,
    "structuralMismatchScore": 0.05,
    "passed": true,
    "notes": "Expected and actual representation content agree.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  },
  {
    "targetId": "fermion-candidate-002",
    "branchId": "V4",
    "expectedModeCount": 2,
    "actualModeCount": 1,
    "missingRequiredCount": 1,
    "structuralMismatchScore": 0.60,
    "passed": false,
    "notes": "Missing required mode; should trigger fatal RepresentationContent falsifier.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  }
]
```

### `coupling_consistency.json`

This file is a JSON array of `CouplingConsistencyRecord`.

```json
[
  {
    "targetId": "coupling-proxy-00",
    "branchIds": [
      "V1",
      "V2",
      "V3",
      "V4"
    ],
    "meanValue": 0.83,
    "minValue": 0.79,
    "maxValue": 0.87,
    "relativeSpread": 0.0964,
    "passed": true,
    "notes": "Coupling proxy consistent across branch family.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  },
  {
    "targetId": "coupling-proxy-fermion-01",
    "branchIds": [
      "V1",
      "V2",
      "V3",
      "V4"
    ],
    "meanValue": 0.31,
    "minValue": 0.10,
    "maxValue": 0.55,
    "relativeSpread": 1.4516,
    "passed": false,
    "notes": "Branch-family coupling inconsistency exceeds threshold.",
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  }
]
```

### `dossiers/study_manifest.json`

This file is a JSON array of `StudyManifest`.

```json
[
  {
    "studyId": "phase5-su2-branch-refinement-env-validation-positive",
    "description": "Positive or mixed evidence dossier for the Phase V SU(2) reference campaign.",
    "runFolder": "studies/phase5_su2_branch_refinement_env_validation/artifacts",
    "reproducibility": {
      "evidenceTier": "RegeneratedCurrentCodeCpu",
      "codeRevision": "phase5-campaign-cli",
      "reproductionCommands": [
        "dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir studies/phase5_su2_branch_refinement_env_validation/artifacts"
      ]
    },
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  },
  {
    "studyId": "phase5-su2-branch-refinement-env-validation-negative",
    "description": "Negative-result dossier for the Phase V SU(2) reference campaign.",
    "runFolder": "studies/phase5_su2_branch_refinement_env_validation/artifacts",
    "reproducibility": {
      "evidenceTier": "RegeneratedCurrentCodeCpu",
      "codeRevision": "phase5-campaign-cli",
      "reproductionCommands": [
        "dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec studies/phase5_su2_branch_refinement_env_validation/config/campaign.json --out-dir studies/phase5_su2_branch_refinement_env_validation/artifacts"
      ]
    },
    "provenance": {
      "createdAt": "2026-03-14T00:00:00Z",
      "codeRevision": "phase5-campaign-cli",
      "branch": {
        "branchId": "minimal-gu-v1",
        "schemaVersion": "1.0.0"
      },
      "backend": "cpu-reference"
    }
  }
]
```

---

## Definition of done for this document

The work defined here is complete only when all of the following are true:

1. `solve-backgrounds` hard-resolves manifests and persists run classification
   in each `BackgroundRecord`
2. `compute-spectrum` prefers per-background manifests
3. `run-phase5-campaign` exists and is the real reproduction entry point
4. `run_study.sh` for the Phase V reference study uses current CLI syntax
5. `refinement-study` requires and consumes a real values file
6. the campaign emits both dossier types
7. observation-chain data appears in the typed dossier and gates escalation
8. all seven falsifier types are active in code and tests
9. convergence uses `max(h_X, h_F)`
10. imported environments carry real provenance metadata
11. quantitative matching supports the declared distribution models

If a change does not move one of those eleven conditions forward, it is not part
of this gap-closure pass.

---

## Instructions to Claude Code

When implementing from this file:

1. Follow the work-package order.
2. Do not choose alternate contracts unless the repository makes the chosen one
   impossible.
3. If a chosen contract proves impossible, stop and report the exact conflict.
4. Keep all new JSON fields STJ-serialized with explicit `[JsonPropertyName]`.
5. Preserve backward compatibility only where this document explicitly requires
   it.
6. Add tests in the same pass as code changes. A gap is not closed if the test
   proving closure is missing.

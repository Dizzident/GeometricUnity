# Wiring P454 — exact snippets for the main-checkout checkpoint

Phase454 (`studies/phase454_beyond_ray_quadratic_certificate_probe_001/`) was
built and run in a worktree while a full validation pass occupied the main
checkout. This file holds the EXACT snippets to wire it across all validation
surfaces in one checkpoint on main, per the restart-prompt wiring list.
Apply, run a `--incremental` pass, force-add the phase output, commit.

## 1. Generator run line — `scripts/generate_validated_boson_predictions.sh`

Insert AFTER the phase453 line (phase454 reads the phase448/450/453 summary
outputs, so it must run after them) and BEFORE the phase101 line:

```
dotnet run --no-build -c Release --project studies/phase454_beyond_ray_quadratic_certificate_probe_001/Phase454BeyondRayQuadraticCertificateProbe.csproj
```

## 2. Traversal item — `scripts/BosonPhasesTraversal.proj`

Insert after the phase453 `PhaseProject` item:

```xml
    <PhaseProject Include="../studies/phase454_beyond_ray_quadratic_certificate_probe_001/Phase454BeyondRayQuadraticCertificateProbe.csproj" />
```

## 3. Scanner exclusion lists (same 8 audit scanners phase453 registered in)

Per the standing hygiene rule the scanners are named by PHASE NUMBER only;
each lives at `studies/phase<NNN>_*/Program.cs`. In every case the new line
goes directly after the corresponding phase453 line/entry.

phase204, in `IsGeneratedAuditOrImplementationJson`:

```csharp
    || path.Contains("studies/phase454_", StringComparison.Ordinal)
```

phase205, in `IsGeneratedAuditOrImplementationText`:

```csharp
    || path.Contains("studies/phase454_", StringComparison.Ordinal)
```

phase207, in `ClassifyBlockers` (after the phase453 `AddIf`):

```csharp
    AddIf(blockers, file.Contains("phase454_beyond_ray_quadratic_certificate_probe", StringComparison.Ordinal), "generated-diagnostic-artifact");
```

phase253, in `excludedPathFragments`:

```csharp
    "studies/phase454_beyond_ray_quadratic_certificate_probe_001/",
```

phase279, in `IsGeneratedOrCurrentPhaseFile`:

```csharp
    || normalizedPath.Contains("studies/phase454_beyond_ray_quadratic_certificate_probe_001/", StringComparison.Ordinal)
```

phase281, in `IsGeneratedOrCurrentPhaseFile`:

```csharp
    || normalizedPath.Contains("studies/phase454_beyond_ray_quadratic_certificate_probe_001/", StringComparison.Ordinal)
```

phase295, in `IsExcluded`:

```csharp
    || normalizedPath.StartsWith("studies/phase454_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase454_", StringComparison.Ordinal)
```

phase296, in `IsExcluded`:

```csharp
    || normalizedPath.StartsWith("studies/phase454_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase454_", StringComparison.Ordinal)
```

Doc-text note: `IMPLEMENTATION_P454.md` and this file deliberately avoid the
scanner topic terms (the study's own files are covered by the directory
exclusions above), so no doc-level exclusion entries are needed. If a scanner
flips on the docs anyway during the validation pass, register the doc path in
that scanner's blocker list following the `IMPLEMENTATION_P278.md` pattern in
phase207 — never weaken a scanner.

## 4. Phase101 package block — `studies/phase101_boson_prediction_package_001/Program.cs`

(a) Constant, after `Phase453WhamParityErrorModelRepairPath`:

```csharp
const string Phase454BeyondRayQuadraticCertificateProbePath = "studies/phase454_beyond_ray_quadratic_certificate_probe_001/output/beyond_ray_quadratic_certificate_probe_summary.json";
```

(b) Parse, after `using var phase453 = ...`:

```csharp
using var phase454 = TryParseJson(Phase454BeyondRayQuadraticCertificateProbePath);
```

(c) Package block, after the `whamParityErrorModelRepair` block:

```csharp
    beyondRayQuadraticCertificateProbe = phase454 is not null
        ? new
        {
            status = JsonString(phase454.RootElement, "terminalStatus"),
            beyondRayQuadraticCertificateProbePassed = JsonBool(phase454.RootElement, "beyondRayQuadraticCertificateProbePassed"),
            targetBlindConstruction = JsonBool(phase454.RootElement, "targetBlindConstruction"),
            applicationSubjectKind = JsonString(phase454.RootElement, "applicationSubjectKind"),
            verdictKind = JsonString(phase454.RootElement, "verdictKind"),
            certificateConventionsAreWorkbenchConventions = JsonBool(phase454.RootElement, "certificateConventionsAreWorkbenchConventions"),
            physicistReviewPending = JsonBool(phase454.RootElement, "physicistReviewPending"),
            scaleIsWorkbenchRelativeCandidateOnly = JsonBool(phase454.RootElement, "scaleIsWorkbenchRelativeCandidateOnly"),
            noGevPromotion = JsonBool(phase454.RootElement, "noGevPromotion"),
            sourceContractApplicationAllowed = JsonBool(phase454.RootElement, "sourceContractApplicationAllowed"),
            canFillPhase201WzContract = JsonBool(phase454.RootElement, "canFillPhase201WzContract"),
            canFillPhase201HiggsContract = JsonBool(phase454.RootElement, "canFillPhase201HiggsContract"),
            canFillPhase256ObservedFieldExtractionContract = JsonBool(phase454.RootElement, "canFillPhase256ObservedFieldExtractionContract"),
            routePromotesWzMasses = JsonBool(phase454.RootElement, "routePromotesWzMasses"),
            routePromotesHiggsMass = JsonBool(phase454.RootElement, "routePromotesHiggsMass"),
            routeCompletesBosonPredictions = JsonBool(phase454.RootElement, "routeCompletesBosonPredictions"),
            decision = JsonString(phase454.RootElement, "decision"),
        }
        : null,
```

## 5. Phase202 checklist — `studies/phase202_boson_objective_completion_audit_001/Program.cs`

(a) Constant, after `Phase453Path`:

```csharp
const string Phase454Path = "studies/phase454_beyond_ray_quadratic_certificate_probe_001/output/beyond_ray_quadratic_certificate_probe_summary.json";
```

(b) Parse, after the phase453 parse line:

```csharp
using var phase454 = File.Exists(Phase454Path) ? JsonDocument.Parse(File.ReadAllText(Phase454Path)) : null;
```

(c) Completion booleans, after the `whamParityErrorModelRepairPassed` block:

```csharp
var beyondRayQuadraticCertificateProbeMaterialized = phase454 is not null;
var beyondRayQuadraticCertificateProbePassed = beyondRayQuadraticCertificateProbeMaterialized
    && JsonBool(phase454!.RootElement, "beyondRayQuadraticCertificateProbePassed") is true
    && JsonBool(phase454.RootElement, "targetBlindConstruction") is true
    && JsonString(phase454.RootElement, "applicationSubjectKind") == "beyond-ray-quadratic-certificate-probe"
    && JsonBool(phase454.RootElement, "precursorsPassed") is true
    && JsonBool(phase454.RootElement, "certificateConventionsAreWorkbenchConventions") is true
    && JsonBool(phase454.RootElement, "physicistReviewPending") is true
    && JsonBool(phase454.RootElement, "scaleIsWorkbenchRelativeCandidateOnly") is true
    && JsonBool(phase454.RootElement, "noGevPromotion") is true
    && JsonBool(phase454.RootElement, "sourceContractApplicationAllowed") is false
    && JsonBool(phase454.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase454.RootElement, "canFillPhase201HiggsContract") is false
    && JsonBool(phase454.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false
    && JsonBool(phase454.RootElement, "routePromotesWzMasses") is false
    && JsonBool(phase454.RootElement, "routePromotesHiggsMass") is false
    && JsonBool(phase454.RootElement, "routeCompletesBosonPredictions") is false;
```

(d) Checklist item, after the phase453 `ObjectiveChecklistItem`:

```csharp
    new ObjectiveChecklistItem(
        "beyond-ray-quadratic-certificate-probe-materialized",
        "Execute Team B's Wave-1 rank-2 Arms A/B (phase454): exact beyond-invariant-ray certificates at the trivial and audited invariant-ray backgrounds. Arm A eigensolves the joint (omega,theta) Hessian's 48x48 momentum blocks on the exact complement of the per-background-recomputed gauge kernel ker d (252 = 12 + 3x80 verified at the n=3 trivial background); Arm B solves the exact directional cubic a3(v) on the pre-registered lowest-momentum-shell menu with the degeneracy classifier a3^2 vs 4 a2 a4. Fail-closed batteries include the exact-count phase448 lineage match; pre-registered three-terminal taxonomy; finite-menu/n=3/quadratic+cubic scope recorded; nothing promoted.",
        beyondRayQuadraticCertificateProbePassed ? "passed" : "failed",
        beyondRayQuadraticCertificateProbeMaterialized
            ? $"beyondRayQuadraticCertificateProbePassed={JsonBool(phase454!.RootElement, "beyondRayQuadraticCertificateProbePassed")}; verdictKind={JsonString(phase454.RootElement, "verdictKind")}; physicistReviewPending={JsonBool(phase454.RootElement, "physicistReviewPending")}; noGevPromotion={JsonBool(phase454.RootElement, "noGevPromotion")}; canFillPhase201WzContract={JsonBool(phase454.RootElement, "canFillPhase201WzContract")}; decision={JsonString(phase454.RootElement, "decision")}"
            : "Phase454 artifact not materialized",
        Phase454Path),
```

## 6. Integrity asserts — `scripts/verify_boson_claim_integrity.sh`

(a) Path map entry, after the `phase453:` line:

```js
  phase454: "studies/phase454_beyond_ray_quadratic_certificate_probe_001/output/beyond_ray_quadratic_certificate_probe_summary.json",
```

(b) Loader, after `const phase453 = requireFile(paths.phase453);`:

```js
const phase454 = requireFile(paths.phase454);
```

(c) Assert block, after the phase453 asserts (values wired to the ACTUAL
committed run — see `IMPLEMENTATION_P454.md` for the run record):

```js
  // Phase454 -- Team B Wave-1 rank-2 Arms A/B: beyond-invariant-ray quadratic/cubic certificates.
  // Wired to the ACTUAL committed run: trivial background clean; ray-point
  // transverse negatives real (non-gauge) and named; Arm B degeneracy-free.
  assert(phase454.beyondRayQuadraticCertificateProbePassed === true, "Phase454 beyond-ray certificate probe must pass on internal consistency.");
  assert(phase454.applicationSubjectKind === "beyond-ray-quadratic-certificate-probe", "Phase454 must classify its subject as the beyond-ray quadratic certificate probe.");
  assert(phase454.precursorsPassed === true, "Phase454 must confirm the phase448/450/453 precursors.");
  assert(phase454.verdictKind === "beyond-ray-instability-candidate-found", "Phase454 must carry the committed pre-registered verdict (named ray-background transverse candidates; trivial background clean).");
  assert(phase454.armA?.allNonnegativeOnGaugeComplement === false && phase454.armA?.negativeComplementDirectionCount === 2016, "Phase454 Arm A must carry the committed 2016 named negative gauge-complement directions (ray backgrounds only).");
  assert(phase454.armA?.kernelRestrictedNegativeCount === 0, "Phase454 must record zero negatives inside the projected-out kernel sub-blocks (nothing hidden by the projection).");
  assert(phase454.armA?.backgrounds?.find(b => b.id === "trivial")?.members?.every(m => m.gaugeComplement?.negative === 0) === true, "Phase454 must certify ZERO negative complement directions at the trivial background for every member.");
  assert(phase454.armA?.kernelTrivialTotalComplexDim === 252 && phase454.armA?.kernelTrivialPatternOk === true, "Phase454 must verify the trivial-background gauge-kernel dimension 252 with the 12 + 3x80 sector pattern.");
  assert(phase454.armB?.noDegenerateCubicOnMenu === true && phase454.armB?.degenerateCapableDirectionCount === 0, "Phase454 Arm B must certify no degeneracy-capable cubic on the pre-registered menu.");
  assert(phase454.armB?.nonzeroA3RowCount === 270, "Phase454 Arm B must carry the committed 270 nonzero-cubic rows (the audited-ray-modulated directions).");
  assert(phase454.batteries?.batteriesAllPassed === true, "Phase454 batteries must all pass.");
  assert(phase454.batteries?.lineage448Ok === true && phase454.armA?.lineage448?.comparisons === 24 && phase454.armA?.lineage448?.allMatch === true, "Phase454 must reproduce the committed phase448 mode counts exactly (24/24 lineage comparisons).");
  assert(phase454.physicistReviewPending === true, "Phase454 must carry physicistReviewPending explicitly (Wave-0 item 0.3 open).");
  assert(phase454.certificateConventionsAreWorkbenchConventions === true && phase454.scaleIsWorkbenchRelativeCandidateOnly === true && phase454.noGevPromotion === true, "Phase454 must keep the convention and no-promotion boundaries.");
  assert(phase454.sourceContractApplicationAllowed === false && phase454.canFillPhase201WzContract === false && phase454.canFillPhase201HiggsContract === false && phase454.canFillPhase256ObservedFieldExtractionContract === false, "Phase454 cannot fill Phase201 or Phase256 contracts.");
  assert(phase454.routePromotesWzMasses === false && phase454.routePromotesHiggsMass === false && phase454.routeCompletesBosonPredictions === false, "Phase454 cannot promote boson predictions.");
  assert(phase101Package?.beyondRayQuadraticCertificateProbe?.beyondRayQuadraticCertificateProbePassed === true, "Phase101 must include the Phase454 block.");
```

NOTE: the `verdictKind` / Arm A / Arm B asserts above are wired to the ACTUAL
values of the committed run (verdict `beyond-ray-instability-candidate-found`;
2016 named candidates; 270 nonzero-cubic rows; trivial background clean;
24/24 lineage). If a future legitimate re-run on changed platform code shifts
these, the asserts must be re-pinned to the new committed record in the same
checkpoint — never loosened.

## 7. Restart prompt + registry + journal

- `docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md`: add phase454 to the wired
  phase list / gate table; next free Team B number becomes 455.
- `docs/Phases/PHASE_NUMBER_REGISTRY.md`: no edit needed — 454 is inside
  Team B's 453-458 block and is the lowest free number (rule 1 satisfied).
- `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`: append the phase454 entry
  (verdict + per-arm numbers from `IMPLEMENTATION_P454.md`).

## 8. Output artifacts

Force-add at checkpoint time:

```
git add -f studies/phase454_beyond_ray_quadratic_certificate_probe_001/output/beyond_ray_quadratic_certificate_probe.json \
           studies/phase454_beyond_ray_quadratic_certificate_probe_001/output/beyond_ray_quadratic_certificate_probe_summary.json
```

Then: `./scripts/run_boson_phases_incremental.sh --incremental` must be green
before commit; `--full` before any promotion-relevant citation.

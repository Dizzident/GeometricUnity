# Wiring P461 - main-checkout validation surfaces

Apply on main after the running pass completes, in the SAME checkpoint as
the phase460 wiring, then validate with
`./scripts/run_boson_phases_incremental.sh --incremental` before committing
(`--full` required before any promotion-relevant citation; it reseeds
`scripts/boson_incremental_manifest.json` - never edit the manifest by
hand). Force-add the output directory at checkpoint time:

```
git add -f studies/phase461_dimensional_transmutation_reading_menu_001/output
```

## 1. Generator run line - `scripts/generate_validated_boson_predictions.sh`

Insert AFTER the phase460 run line and BEFORE the phase101 run line
(phase461 consumes phase460's routing decision conceptually; both are
standalone reads of committed files, so ordering is documentation only):

```
dotnet run --no-build -c Release --project studies/phase461_dimensional_transmutation_reading_menu_001/Phase461DimensionalTransmutationReadingMenu.csproj
```

## 2. Traversal item - `scripts/BosonPhasesTraversal.proj`

Insert after the phase460 `PhaseProject` item:

```xml
    <PhaseProject Include="../studies/phase461_dimensional_transmutation_reading_menu_001/Phase461DimensionalTransmutationReadingMenu.csproj" />
```

## 3. Scanner exclusions (same surfaces and pattern as WIRING_P460.md)

`studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs`:

```csharp
    || path.Contains("studies/phase461_", StringComparison.Ordinal)
```

`studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs`:

```csharp
    || path.Contains("studies/phase461_", StringComparison.Ordinal)
```

`studies/phase207_higgs_quartic_self_coupling_source_scan_001/Program.cs`:

```csharp
    AddIf(blockers, file.Contains("phase461_dimensional_transmutation_reading_menu", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P461.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/WIRING_P461.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
```

`studies/phase253_global_observed_sector_vacuum_scan_001/Program.cs`
(`excludedPathFragments`):

```csharp
    "studies/phase461_dimensional_transmutation_reading_menu_001/",
```

`studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/Program.cs`:

```csharp
    || normalizedPath.Contains("studies/phase461_dimensional_transmutation_reading_menu_001/", StringComparison.Ordinal)
```

`studies/phase281_geometric_refractive_unification_source_audit_001/Program.cs`:

```csharp
    || normalizedPath.Contains("studies/phase461_dimensional_transmutation_reading_menu_001/", StringComparison.Ordinal)
```

and in its doc list:

```csharp
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P461.md"
    || normalizedPath == "docs/Phases/Implementation/WIRING_P461.md"
```

`studies/phase295_observed_field_extraction_contract_candidate_scan_001/Program.cs`:

```csharp
    || normalizedPath.StartsWith("studies/phase461_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase461_", StringComparison.Ordinal)
```

`studies/phase296_source_lineage_contract_field_candidate_scan_001/Program.cs`:

```csharp
    || normalizedPath.StartsWith("studies/phase461_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase461_", StringComparison.Ordinal)
```

## 4. Phase101 block - `studies/phase101_boson_prediction_package_001/Program.cs`

Const:

```csharp
const string Phase461DimensionalTransmutationReadingMenuPath = "studies/phase461_dimensional_transmutation_reading_menu_001/output/dimensional_transmutation_reading_menu_summary.json";
```

Parse:

```csharp
using var phase461 = TryParseJson(Phase461DimensionalTransmutationReadingMenuPath);
```

Package block:

```csharp
    dimensionalTransmutationReadingMenu = phase461 is not null
        ? new
        {
            status = JsonString(phase461.RootElement, "terminalStatus"),
            dimensionalTransmutationReadingMenuPassed = JsonBool(phase461.RootElement, "dimensionalTransmutationReadingMenuPassed"),
            targetBlindConstruction = JsonBool(phase461.RootElement, "targetBlindConstruction"),
            applicationSubjectKind = JsonString(phase461.RootElement, "applicationSubjectKind"),
            verdictKind = JsonString(phase461.RootElement, "verdictKind"),
            physicistReviewPending = JsonBool(phase461.RootElement, "physicistReviewPending"),
            noGevPromotion = JsonBool(phase461.RootElement, "noGevPromotion"),
            sourceContractApplicationAllowed = JsonBool(phase461.RootElement, "sourceContractApplicationAllowed"),
            canFillPhase201WzContract = JsonBool(phase461.RootElement, "canFillPhase201WzContract"),
            canFillPhase201HiggsContract = JsonBool(phase461.RootElement, "canFillPhase201HiggsContract"),
            canFillPhase256ObservedFieldExtractionContract = JsonBool(phase461.RootElement, "canFillPhase256ObservedFieldExtractionContract"),
            routePromotesWzMasses = JsonBool(phase461.RootElement, "routePromotesWzMasses"),
            routePromotesHiggsMass = JsonBool(phase461.RootElement, "routePromotesHiggsMass"),
            routeCompletesBosonPredictions = JsonBool(phase461.RootElement, "routeCompletesBosonPredictions"),
            decision = JsonString(phase461.RootElement, "decision"),
        }
        : null,
```

## 5. Phase202 checklist - `studies/phase202_boson_objective_completion_audit_001/Program.cs`

Const:

```csharp
const string Phase461Path = "studies/phase461_dimensional_transmutation_reading_menu_001/output/dimensional_transmutation_reading_menu_summary.json";
```

Parse:

```csharp
using var phase461 = File.Exists(Phase461Path) ? JsonDocument.Parse(File.ReadAllText(Phase461Path)) : null;
```

Vars:

```csharp
var dimensionalTransmutationReadingMenuMaterialized = phase461 is not null;
var dimensionalTransmutationReadingMenuPassed = dimensionalTransmutationReadingMenuMaterialized
    && JsonBool(phase461!.RootElement, "dimensionalTransmutationReadingMenuPassed") is true
    && JsonBool(phase461.RootElement, "targetBlindConstruction") is false
    && JsonString(phase461.RootElement, "applicationSubjectKind") == "dimensional-transmutation-reading-menu"
    && JsonBool(phase461.RootElement, "physicistReviewPending") is true
    && JsonBool(phase461.RootElement, "noGevPromotion") is true
    && JsonBool(phase461.RootElement, "sourceContractApplicationAllowed") is false
    && JsonBool(phase461.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase461.RootElement, "canFillPhase201HiggsContract") is false
    && JsonBool(phase461.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false
    && JsonBool(phase461.RootElement, "routePromotesWzMasses") is false
    && JsonBool(phase461.RootElement, "routePromotesHiggsMass") is false
    && JsonBool(phase461.RootElement, "routeCompletesBosonPredictions") is false;
```

Checklist item:

```csharp
    new ObjectiveChecklistItem(
        "dimensional-transmutation-reading-menu-materialized",
        "Execute Team A's Wave-1 phase461 (A2): the finite reading menu of the routed CC-as-a-VEV slot statement with look-elsewhere control - 17 pre-registered power readings x 7 live declared-comparison windows (per-window imports-observed flags declared before any value is computed; the 246-GeV electroweak VEV BANNED as circular and gauge-variant, kept only as the referee-reconstruction window), exact-rational band decisions, Bonferroni trials correction, and the fail-closed O6 reconstruction gate that reproduced the task-force referee's ~460x kill (committed factor 451.1). Committed verdict: declared-comparison-consistency-only - no import-clean trials-surviving anchor; nothing promoted.",
        dimensionalTransmutationReadingMenuPassed ? "passed" : "failed",
        dimensionalTransmutationReadingMenuMaterialized
            ? $"dimensionalTransmutationReadingMenuPassed={JsonBool(phase461!.RootElement, "dimensionalTransmutationReadingMenuPassed")}; verdictKind={JsonString(phase461.RootElement, "verdictKind")}; physicistReviewPending={JsonBool(phase461.RootElement, "physicistReviewPending")}; noGevPromotion={JsonBool(phase461.RootElement, "noGevPromotion")}; canFillPhase201WzContract={JsonBool(phase461.RootElement, "canFillPhase201WzContract")}; decision={JsonString(phase461.RootElement, "decision")}"
            : "Phase461 artifact not materialized",
        Phase461Path),
```

## 6. Integrity verifier - `scripts/verify_boson_claim_integrity.sh`

Path entry:

```javascript
  phase461: "studies/phase461_dimensional_transmutation_reading_menu_001/output/dimensional_transmutation_reading_menu_summary.json",
```

Require:

```javascript
const phase461 = requireFile(paths.phase461);
```

Asserts (pinned to the ACTUAL committed output values):

```javascript
  // Phase461 -- Team A Wave-1 A2: dimensional-transmutation reading menu.
  assert(phase461.dimensionalTransmutationReadingMenuPassed === true, "Phase461 reading menu must pass fail-closed.");
  assert(phase461.targetBlindConstruction === false, "Phase461 is a declared-comparison phase (429/451-style separation), never blind.");
  assert(phase461.applicationSubjectKind === "dimensional-transmutation-reading-menu", "Phase461 must classify its subject as the dimensional-transmutation reading menu.");
  assert(phase461.verdictKind === "declared-comparison-consistency-only", "Phase461 must carry the committed declared-comparison-consistency-only verdict.");
  assert(phase461.reconstructionGate?.found === true, "Phase461 O6 reconstruction gate must be green (menu-incomplete otherwise, with NO verdict).");
  const p461Rec = phase461.reconstructionGate?.reconstructedRefereeReading;
  assert(p461Rec?.readingId === "M(1,1)-full" && p461Rec?.windowId === "vevEw246" && p461Rec?.convention === "cc-plane-squared", "Phase461 must commit the reconstructed referee reading (M(1,1)-full vs the banned VEV window, cc-plane-squared).");
  assert(p461Rec?.killFactor > 300 && p461Rec?.killFactor < 700 && Math.abs(p461Rec?.killFactor - 451.105) < 0.01, "Phase461 reconstructed kill factor must reproduce the referee's ~460x figure (committed 451.105).");
  assert(p461Rec?.windowBannedFromLiveVerdicts === true, "Phase461 reconstruction window must stay banned from live verdicts.");
  assert(phase461.liveRowCount === 119 && phase461.primaryBandHitCount === 4 && phase461.secondaryBandOnlyHitCount === 11 && phase461.importCleanTrialsSurvivingHitCount === 0, "Phase461 must carry the committed row census (119 live; 4 import-laden primary hits; 0 import-clean trials-surviving).");
  assert((phase461.primaryBandHits ?? []).every((h) => (h.windowId === "qcdScaleNf5" || h.windowId === "massElectron") && h.importsObserved === true && h.trialsSurviving === false), "Phase461 primary hits must all be import-laden and trials-failing.");
  assert(phase461.lookElsewhereControl?.trialsSurvivalThreshold === 0.05 && phase461.lookElsewhereControl?.minCorrectedP === 1, "Phase461 look-elsewhere control must carry the pre-registered threshold and the committed minimum corrected p.");
  assert(phase461.physicistReviewPending === true, "Phase461 must carry physicistReviewPending explicitly (Wave-0 item 0.3 open).");
  assert(phase461.noGevPromotion === true, "Phase461 must keep the no-GeV boundary (all GeV numbers are test-only imports).");
  assert(phase461.sourceContractApplicationAllowed === false && phase461.canFillPhase201WzContract === false && phase461.canFillPhase201HiggsContract === false && phase461.canFillPhase256ObservedFieldExtractionContract === false, "Phase461 cannot fill Phase201 or Phase256 contracts.");
  assert(phase461.routePromotesWzMasses === false && phase461.routePromotesHiggsMass === false && phase461.routeCompletesBosonPredictions === false, "Phase461 cannot promote boson predictions.");
  assert(phase101Package?.dimensionalTransmutationReadingMenu?.dimensionalTransmutationReadingMenuPassed === true, "Phase101 must include the Phase461 block.");
```

## 7. Restart prompt / journal

- `docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md`: add phase461 to the
  wiring list and queue state; Wave-1 items 2 and 3 (A1/A2) move to DONE;
  next free Team A number becomes 462.
- `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`: checkpoint entry with the
  committed verdicts; note that A6's first two conjuncts are now consumed
  (A1 breaking + A2 declared-comparison-only rule-out) pending A3.

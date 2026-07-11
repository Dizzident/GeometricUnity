# Wiring P460 - main-checkout validation surfaces

Apply on main after the running pass completes, all in ONE checkpoint,
then validate with `./scripts/run_boson_phases_incremental.sh --incremental`
before committing (a `--full` pass is required before any promotion-relevant
citation and reseeds `scripts/boson_incremental_manifest.json`; never edit
the manifest by hand). Force-add the output directory at checkpoint time:

```
git add -f studies/phase460_source_corpus_units_equivariance_kernel_001/output
```

## 1. Generator run line - `scripts/generate_validated_boson_predictions.sh`

Insert AFTER the phase453 run line and BEFORE the phase101 run line:

```
dotnet run --no-build -c Release --project studies/phase460_source_corpus_units_equivariance_kernel_001/Phase460SourceCorpusUnitsEquivarianceKernel.csproj
```

## 2. Traversal item - `scripts/BosonPhasesTraversal.proj`

Insert after the phase453 `PhaseProject` item (keep numeric order):

```xml
    <PhaseProject Include="../studies/phase460_source_corpus_units_equivariance_kernel_001/Phase460SourceCorpusUnitsEquivarianceKernel.csproj" />
```

## 3. Scanner exclusions (six scripts-root scanners + 204/205/207)

Follow the committed phase453 pattern exactly.

`studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs` - in
the path-exclusion chain, after the `phase453_` line:

```csharp
    || path.Contains("studies/phase460_", StringComparison.Ordinal)
```

`studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs` -
same chain, after the `phase453_` line:

```csharp
    || path.Contains("studies/phase460_", StringComparison.Ordinal)
```

`studies/phase207_higgs_quartic_self_coupling_source_scan_001/Program.cs` -
after the phase453 `AddIf` line:

```csharp
    AddIf(blockers, file.Contains("phase460_source_corpus_units_equivariance_kernel", StringComparison.Ordinal), "generated-diagnostic-artifact");
```

and (doc registration, P450/P451 pattern) after the `IMPLEMENTATION_P451.md`
`AddIf` line:

```csharp
    AddIf(blockers, file.Contains("docs/Phases/Implementation/IMPLEMENTATION_P460.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
    AddIf(blockers, file.Contains("docs/Phases/Implementation/WIRING_P460.md", StringComparison.Ordinal), "generated-diagnostic-artifact");
```

`studies/phase253_global_observed_sector_vacuum_scan_001/Program.cs` - in
`excludedPathFragments`, after the phase453 entry:

```csharp
    "studies/phase460_source_corpus_units_equivariance_kernel_001/",
```

`studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/Program.cs`
- in the known-file chain, after the phase453 line:

```csharp
    || normalizedPath.Contains("studies/phase460_source_corpus_units_equivariance_kernel_001/", StringComparison.Ordinal)
```

`studies/phase281_geometric_refractive_unification_source_audit_001/Program.cs`
- in the known-file chain, after the phase453 line:

```csharp
    || normalizedPath.Contains("studies/phase460_source_corpus_units_equivariance_kernel_001/", StringComparison.Ordinal)
```

and after the `IMPLEMENTATION_P451.md` line:

```csharp
    || normalizedPath == "docs/Phases/Implementation/IMPLEMENTATION_P460.md"
    || normalizedPath == "docs/Phases/Implementation/WIRING_P460.md"
```

`studies/phase295_observed_field_extraction_contract_candidate_scan_001/Program.cs`
- after the phase453 pair:

```csharp
    || normalizedPath.StartsWith("studies/phase460_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase460_", StringComparison.Ordinal)
```

`studies/phase296_source_lineage_contract_field_candidate_scan_001/Program.cs`
- after the phase453 pair:

```csharp
    || normalizedPath.StartsWith("studies/phase460_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase460_", StringComparison.Ordinal)
```

If the ~3-min `--incremental` validation still flags a scanner, the failing
scanner's output JSON names the matching file/line; register or reword per
the CLAUDE.md hazard rules (never weaken a scanner).

## 4. Phase101 block - `studies/phase101_boson_prediction_package_001/Program.cs`

Const (with the other phase paths):

```csharp
const string Phase460SourceCorpusUnitsEquivarianceKernelPath = "studies/phase460_source_corpus_units_equivariance_kernel_001/output/source_corpus_units_equivariance_kernel_summary.json";
```

Parse (with the other `TryParseJson` lines):

```csharp
using var phase460 = TryParseJson(Phase460SourceCorpusUnitsEquivarianceKernelPath);
```

Package block (next to `whamParityErrorModelRepair`):

```csharp
    sourceCorpusUnitsEquivarianceKernel = phase460 is not null
        ? new
        {
            status = JsonString(phase460.RootElement, "terminalStatus"),
            unitsEquivarianceKernelAuditPassed = JsonBool(phase460.RootElement, "unitsEquivarianceKernelAuditPassed"),
            targetBlindConstruction = JsonBool(phase460.RootElement, "targetBlindConstruction"),
            applicationSubjectKind = JsonString(phase460.RootElement, "applicationSubjectKind"),
            verdictKind = JsonString(phase460.RootElement, "verdictKind"),
            physicistReviewPending = JsonBool(phase460.RootElement, "physicistReviewPending"),
            noGevPromotion = JsonBool(phase460.RootElement, "noGevPromotion"),
            sourceContractApplicationAllowed = JsonBool(phase460.RootElement, "sourceContractApplicationAllowed"),
            canFillPhase201WzContract = JsonBool(phase460.RootElement, "canFillPhase201WzContract"),
            canFillPhase201HiggsContract = JsonBool(phase460.RootElement, "canFillPhase201HiggsContract"),
            canFillPhase256ObservedFieldExtractionContract = JsonBool(phase460.RootElement, "canFillPhase256ObservedFieldExtractionContract"),
            routePromotesWzMasses = JsonBool(phase460.RootElement, "routePromotesWzMasses"),
            routePromotesHiggsMass = JsonBool(phase460.RootElement, "routePromotesHiggsMass"),
            routeCompletesBosonPredictions = JsonBool(phase460.RootElement, "routeCompletesBosonPredictions"),
            decision = JsonString(phase460.RootElement, "decision"),
        }
        : null,
```

## 5. Phase202 checklist - `studies/phase202_boson_objective_completion_audit_001/Program.cs`

Const:

```csharp
const string Phase460Path = "studies/phase460_source_corpus_units_equivariance_kernel_001/output/source_corpus_units_equivariance_kernel_summary.json";
```

Parse:

```csharp
using var phase460 = File.Exists(Phase460Path) ? JsonDocument.Parse(File.ReadAllText(Phase460Path)) : null;
```

Vars (next to the phase453 vars):

```csharp
var sourceCorpusUnitsEquivarianceKernelMaterialized = phase460 is not null;
var sourceCorpusUnitsEquivarianceKernelPassed = sourceCorpusUnitsEquivarianceKernelMaterialized
    && JsonBool(phase460!.RootElement, "unitsEquivarianceKernelAuditPassed") is true
    && JsonBool(phase460.RootElement, "targetBlindConstruction") is true
    && JsonString(phase460.RootElement, "applicationSubjectKind") == "source-corpus-units-equivariance-kernel"
    && JsonBool(phase460.RootElement, "physicistReviewPending") is true
    && JsonBool(phase460.RootElement, "noGevPromotion") is true
    && JsonBool(phase460.RootElement, "sourceContractApplicationAllowed") is false
    && JsonBool(phase460.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase460.RootElement, "canFillPhase201HiggsContract") is false
    && JsonBool(phase460.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false
    && JsonBool(phase460.RootElement, "routePromotesWzMasses") is false
    && JsonBool(phase460.RootElement, "routePromotesHiggsMass") is false
    && JsonBool(phase460.RootElement, "routeCompletesBosonPredictions") is false;
```

Checklist item (next to the phase453 item):

```csharp
    new ObjectiveChecklistItem(
        "source-corpus-units-equivariance-kernel-materialized",
        "Execute Team A's Wave-1 phase460 (A1): the units-equivariance kernel theorem over the committed phase330-345 audit outputs plus the committed reference snapshot - exact BigInteger Smith-normal-form kernels per admissible reading, pre-registered part-12d routing gate (coefficient-free slot statement routed to the phase461 anchor menu, never a silent kernel row), 31-item dimensionally-ambiguous blocking set never dropped. Committed verdict: the prescribed reading BREAKS on the snapshot's literal curvature-mass statement and every cc-anchor re-grading breaks pinned corpus relations - elimination content only; nothing promoted.",
        sourceCorpusUnitsEquivarianceKernelPassed ? "passed" : "failed",
        sourceCorpusUnitsEquivarianceKernelMaterialized
            ? $"unitsEquivarianceKernelAuditPassed={JsonBool(phase460!.RootElement, "unitsEquivarianceKernelAuditPassed")}; verdictKind={JsonString(phase460.RootElement, "verdictKind")}; physicistReviewPending={JsonBool(phase460.RootElement, "physicistReviewPending")}; noGevPromotion={JsonBool(phase460.RootElement, "noGevPromotion")}; canFillPhase201WzContract={JsonBool(phase460.RootElement, "canFillPhase201WzContract")}; decision={JsonString(phase460.RootElement, "decision")}"
            : "Phase460 artifact not materialized",
        Phase460Path),
```

## 6. Integrity verifier - `scripts/verify_boson_claim_integrity.sh`

Path entry (in `paths`, after phase453):

```javascript
  phase460: "studies/phase460_source_corpus_units_equivariance_kernel_001/output/source_corpus_units_equivariance_kernel_summary.json",
```

Require (after `const phase453 = ...`):

```javascript
const phase460 = requireFile(paths.phase460);
```

Asserts (pinned to the ACTUAL committed output values; place next to the
Phase453 assert block):

```javascript
  // Phase460 -- Team A Wave-1 A1: source-corpus units-equivariance kernel.
  assert(phase460.unitsEquivarianceKernelAuditPassed === true, "Phase460 units-equivariance kernel audit must pass fail-closed.");
  assert(phase460.targetBlindConstruction === true, "Phase460 must be target-blind (no observed value enters).");
  assert(phase460.applicationSubjectKind === "source-corpus-units-equivariance-kernel", "Phase460 must classify its subject as the source-corpus units-equivariance kernel.");
  assert(phase460.verdictKind === "breaking-relation-found-under-prescribed-reading", "Phase460 must carry the committed breaking-relation verdict.");
  assert(phase460.snfBatteryPassed === true, "Phase460 exact SNF battery must be green.");
  assert(phase460.part12dRoutingGate?.passed === true && phase460.part12dRoutingGate?.actualClassification === "routed-to-anchor-menu", "Phase460 pre-registered part-12d gate must route the coefficient-free slot statement to the phase461 anchor menu.");
  assert(phase460.pinnedRowCount === 34 && phase460.corpusDimensionallyAmbiguousBlockingSetCount === 31, "Phase460 must carry 34 pinned rows and the 31-item blocking set (never dropped).");
  const p460r0 = (phase460.readings ?? []).find((r) => r.readingId === "R0-prescribed-literal");
  assert(p460r0?.verdict === "equivariance-breaking-relation-found" && (p460r0?.breakingRelationIds ?? []).join(",") === "sp12c-lit", "Phase460 prescribed reading must break exactly on the literal curvature-mass row.");
  const p460r1 = (phase460.readings ?? []).find((r) => r.readingId === "R1-prescribed-squared");
  assert(p460r1?.verdict === "corpus-dimensionally-ambiguous" && p460r1?.scaleVectorInKernel === true && p460r1?.kernelRankValue === 24 && p460r1?.kernelNullityValue === 9, "Phase460 squared-repair reading must close on pinned rows (rank 24 / nullity 9) but stay blocked by the ambiguous set.");
  const p460r3 = (phase460.readings ?? []).find((r) => r.readingId === "R3-ccMass-squared");
  assert((p460r3?.breakingRelationIds ?? []).join(",") === "p335-r2,p339-r1", "Phase460 cc-mass re-grading must break the corpus's pinned cc-bearing relations.");
  assert(phase460.physicistReviewPending === true, "Phase460 must carry physicistReviewPending explicitly (Wave-0 item 0.3 open).");
  assert(phase460.noGevPromotion === true, "Phase460 must keep the no-GeV boundary.");
  assert(phase460.sourceContractApplicationAllowed === false && phase460.canFillPhase201WzContract === false && phase460.canFillPhase201HiggsContract === false && phase460.canFillPhase256ObservedFieldExtractionContract === false, "Phase460 cannot fill Phase201 or Phase256 contracts.");
  assert(phase460.routePromotesWzMasses === false && phase460.routePromotesHiggsMass === false && phase460.routeCompletesBosonPredictions === false, "Phase460 cannot promote boson predictions.");
  assert(phase101Package?.sourceCorpusUnitsEquivarianceKernel?.unitsEquivarianceKernelAuditPassed === true, "Phase101 must include the Phase460 block.");
```

## 7. Restart prompt / journal

- `docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md`: add phase460 to the
  wiring list and queue state; next free Team A number becomes 462
  (459 stays reserved for the attestation phase).
- `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`: checkpoint entry with the
  committed verdicts (this file and IMPLEMENTATION_P460.md hold the
  numbers).

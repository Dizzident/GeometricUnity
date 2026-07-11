# WIRING_P459 — main-checkout wiring for phase459 (apply after the running pass completes)

Every snippet below is EXACT text. Insertion anchors are quoted from the
committed tree at d63ce0b5; apply all surfaces in the SAME checkpoint, then
validate with `./scripts/run_boson_phases_incremental.sh --incremental`
before committing (do not commit red).

Smoke-verified in the phase459 worktree (2026-07-10): terminal
`spectroscopy-record-attestation-passed-record-reconciled-canonical`,
`verdictKind=record-reconciled-canonical`, 31/31 checks green, ~0.03 s.
All numeric literals in the verify-script asserts below are the ACTUAL
values from that run's output.

---

## 1. Generator run line — `scripts/generate_validated_boson_predictions.sh`

Insert AFTER the phase453 run line (currently line 337, immediately before
the phase101 run line):

```sh
dotnet run --no-build -c Release --project studies/phase459_spectroscopy_record_attestation_001/Phase459SpectroscopyRecordAttestation.csproj
```

(Ordering matters: phase459 re-reads the phase452 outputs that the pass
regenerates at line 336, and must run before phase101/phase202.)

## 2. Traversal item — `scripts/BosonPhasesTraversal.proj`

Insert AFTER the phase453 `PhaseProject` item (currently line 323, the last
item in the ItemGroup):

```xml
    <PhaseProject Include="../studies/phase459_spectroscopy_record_attestation_001/Phase459SpectroscopyRecordAttestation.csproj" />
```

## 3. Phase101 block — `studies/phase101_boson_prediction_package_001/Program.cs`

(a) Path const — insert after the `Phase453WhamParityErrorModelRepairPath`
const (currently line 303):

```csharp
const string Phase459SpectroscopyRecordAttestationPath = "studies/phase459_spectroscopy_record_attestation_001/output/spectroscopy_record_attestation_summary.json";
```

(b) Parse — insert after `using var phase453 = TryParseJson(...)` (currently
line 644):

```csharp
using var phase459 = TryParseJson(Phase459SpectroscopyRecordAttestationPath);
```

(c) Package block — insert after the `whamParityErrorModelRepair = ...` block
(currently ends line 8277 with `: null,`), before
`branchLocalDirectInvariantCensus`:

```csharp
    spectroscopyRecordAttestation = phase459 is not null
        ? new
        {
            status = JsonString(phase459.RootElement, "terminalStatus"),
            spectroscopyRecordAttestationPassed = JsonBool(phase459.RootElement, "spectroscopyRecordAttestationPassed"),
            targetBlindConstruction = JsonBool(phase459.RootElement, "targetBlindConstruction"),
            applicationSubjectKind = JsonString(phase459.RootElement, "applicationSubjectKind"),
            verdictKind = JsonString(phase459.RootElement, "verdictKind"),
            checkCount = JsonInt(phase459.RootElement, "checkCount"),
            passedCheckCount = JsonInt(phase459.RootElement, "passedCheckCount"),
            physicistReviewPending = JsonBool(phase459.RootElement, "physicistReviewPending"),
            scaleIsWorkbenchRelativeCandidateOnly = JsonBool(phase459.RootElement, "scaleIsWorkbenchRelativeCandidateOnly"),
            noGevPromotion = JsonBool(phase459.RootElement, "noGevPromotion"),
            sourceContractApplicationAllowed = JsonBool(phase459.RootElement, "sourceContractApplicationAllowed"),
            canFillPhase201WzContract = JsonBool(phase459.RootElement, "canFillPhase201WzContract"),
            canFillPhase201HiggsContract = JsonBool(phase459.RootElement, "canFillPhase201HiggsContract"),
            canFillPhase256ObservedFieldExtractionContract = JsonBool(phase459.RootElement, "canFillPhase256ObservedFieldExtractionContract"),
            routePromotesWzMasses = JsonBool(phase459.RootElement, "routePromotesWzMasses"),
            routePromotesHiggsMass = JsonBool(phase459.RootElement, "routePromotesHiggsMass"),
            routeCompletesBosonPredictions = JsonBool(phase459.RootElement, "routeCompletesBosonPredictions"),
            decision = JsonString(phase459.RootElement, "decision"),
        }
        : null,
```

## 4. Phase202 checklist — `studies/phase202_boson_objective_completion_audit_001/Program.cs`

(a) Path const — insert after the `Phase453Path` const (currently line 212):

```csharp
const string Phase459Path = "studies/phase459_spectroscopy_record_attestation_001/output/spectroscopy_record_attestation_summary.json";
```

(b) Parse — insert after the phase453 parse (currently line 473):

```csharp
using var phase459 = File.Exists(Phase459Path) ? JsonDocument.Parse(File.ReadAllText(Phase459Path)) : null;
```

(c) Materialized/passed vars — insert after the
`whamParityErrorModelRepairPassed` block (currently ends line 6366):

```csharp
var spectroscopyRecordAttestationMaterialized = phase459 is not null;
var spectroscopyRecordAttestationPassed = spectroscopyRecordAttestationMaterialized
    && JsonBool(phase459!.RootElement, "spectroscopyRecordAttestationPassed") is true
    && JsonBool(phase459.RootElement, "targetBlindConstruction") is true
    && JsonString(phase459.RootElement, "applicationSubjectKind") == "spectroscopy-record-attestation"
    && JsonString(phase459.RootElement, "verdictKind") == "record-reconciled-canonical"
    && JsonBool(phase459.RootElement, "physicistReviewPending") is true
    && JsonBool(phase459.RootElement, "scaleIsWorkbenchRelativeCandidateOnly") is true
    && JsonBool(phase459.RootElement, "noGevPromotion") is true
    && JsonBool(phase459.RootElement, "sourceContractApplicationAllowed") is false
    && JsonBool(phase459.RootElement, "canFillPhase201WzContract") is false
    && JsonBool(phase459.RootElement, "canFillPhase201HiggsContract") is false
    && JsonBool(phase459.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false
    && JsonBool(phase459.RootElement, "routePromotesWzMasses") is false
    && JsonBool(phase459.RootElement, "routePromotesHiggsMass") is false
    && JsonBool(phase459.RootElement, "routeCompletesBosonPredictions") is false;
```

(d) Checklist item — insert after the
`"wham-parity-error-model-repair-materialized"` ObjectiveChecklistItem
(currently ends line 8966 with `Phase453Path),`):

```csharp
    new ObjectiveChecklistItem(
        "spectroscopy-record-attestation-materialized",
        "Execute Team A's rank-1 A0 (Wave-0 item 0.2 made standing): the machine-checked phase452 record-reconciliation attestation, zero physics compute, re-run by every generator pass. Attests: committed canonical configuration 16000/10000 trajectories, seed 20260705, equal to the committed phase452 source default literals; canonical gap record to 1e-9 (identity a*m = 2.7132 +- 0.1846, single-window inconclusive-by-construction as a measurement; sd2 combined 2.5260 +- 0.0712 re-derived via the pre-registered rho=1 rule, interpolator cross-check 0.56 sigma; exact free gaps 2.5509/2.5320/2.3570); cross-action ratio 0.931 +- 0.069 computed from the stored gaps, within 1.5 sigma of the free ratio 0.9926 (FREE-FIELD-COMPATIBLE label binding); superseded 2.4352/2.4547 absent from the output JSONs (tamper protection, volatile-scrubbed); claim boundary intact. Any mismatch => config-mismatch-quarantine naming the failing checks, quarantining all spectroscopy rows fail-closed; nothing promoted.",
        spectroscopyRecordAttestationPassed ? "passed" : "failed",
        spectroscopyRecordAttestationMaterialized
            ? $"spectroscopyRecordAttestationPassed={JsonBool(phase459!.RootElement, "spectroscopyRecordAttestationPassed")}; verdictKind={JsonString(phase459.RootElement, "verdictKind")}; checkCount={JsonInt(phase459.RootElement, "checkCount")}; passedCheckCount={JsonInt(phase459.RootElement, "passedCheckCount")}; physicistReviewPending={JsonBool(phase459.RootElement, "physicistReviewPending")}; noGevPromotion={JsonBool(phase459.RootElement, "noGevPromotion")}; canFillPhase201WzContract={JsonBool(phase459.RootElement, "canFillPhase201WzContract")}; decision={JsonString(phase459.RootElement, "decision")}"
            : "Phase459 artifact not materialized",
        Phase459Path),
```

## 5. Integrity verifier — `scripts/verify_boson_claim_integrity.sh`

(a) Paths entry — insert after the `phase453:` entry (currently line 226):

```js
  phase459: "studies/phase459_spectroscopy_record_attestation_001/output/spectroscopy_record_attestation_summary.json",
```

(b) requireFile — insert after `const phase453 = requireFile(paths.phase453);`
(currently line 521):

```js
const phase459 = requireFile(paths.phase459);
```

(c) Assert block — insert after the phase453 assert block (currently ends
line 5402 with the `Phase101 must include the Phase453 block.` assert).
The numeric literals are the ACTUAL values of the smoke-verified output:

```js
  // Phase459 -- Team A rank-1 A0: the phase452 record-reconciliation attestation (Wave-0 item 0.2, standing).
  assert(phase459.spectroscopyRecordAttestationPassed === true, "Phase459 record-reconciliation attestation must pass.");
  assert(phase459.applicationSubjectKind === "spectroscopy-record-attestation", "Phase459 must classify its subject as the spectroscopy record attestation.");
  assert(phase459.verdictKind === "record-reconciled-canonical", "Phase459 must attest the reconciled canonical phase452 record; config-mismatch-quarantine fail-closes the pass and quarantines all spectroscopy rows.");
  assert(Array.isArray(phase459.failingChecks) && phase459.failingChecks.length === 0, "Phase459 must report zero failing attestation checks.");
  assert(phase459.checkCount === 31 && phase459.passedCheckCount === 31, "Phase459 must run its full pre-registered 31-check battery green.");
  assert(phase459.canonicalRecord?.configuration?.trajProduction === 16000 && phase459.canonicalRecord?.configuration?.trajControl === 10000 && phase459.canonicalRecord?.configuration?.rngSeed === 20260705, "Phase459 must pin the committed canonical phase452 configuration.");
  assert(phase459.canonicalRecord?.identity?.mO1 === 2.7132465417703235 && phase459.canonicalRecord?.identity?.mO1Sigma === 0.18463902186135914, "Phase459 must pin the canonical identity gap.");
  assert(phase459.canonicalRecord?.sd2?.combined === 2.526042101792096 && phase459.canonicalRecord?.sd2?.combinedSigma === 0.07123324126755279 && phase459.canonicalRecord?.sd2?.crossCheckSigma === 0.5623628195265302, "Phase459 must pin the canonical sd2 combined gap and cross-check.");
  assert(phase459.canonicalRecord?.exactFreeGaps?.identity === 2.550880306794233 && phase459.canonicalRecord?.exactFreeGaps?.sd2O1 === 2.532028376864343 && phase459.canonicalRecord?.exactFreeGaps?.sd2O2 === 2.3569501678347393, "Phase459 must pin the exact analytic free gaps.");
  assert(phase459.canonicalRecord?.derived?.crossActionRatio === 0.9310035276570129 && phase459.canonicalRecord?.derived?.crossActionRatioSigma === 0.06857994094256506 && phase459.canonicalRecord?.derived?.freeCrossActionRatio === 0.9926096375907257, "Phase459 must pin the derived cross-action ratio and its free-field comparator.");
  assert(phase459.canonicalRecord?.supersededValues?.values?.length === 2, "Phase459 must carry the superseded-value tamper-protection record.");
  assert(phase459.envKnobsRead === false && phase459.zeroPhysicsCompute === true, "Phase459 must be env-clean and zero-physics-compute by construction.");
  assert(phase459.physicistReviewPending === true, "Phase459 must carry physicistReviewPending explicitly (Wave-0 item 0.3 open).");
  assert(phase459.scaleIsWorkbenchRelativeCandidateOnly === true && phase459.noGevPromotion === true, "Phase459 must keep the workbench-relative and no-promotion boundaries.");
  assert(phase459.sourceContractApplicationAllowed === false && phase459.canFillPhase201WzContract === false && phase459.canFillPhase201HiggsContract === false && phase459.canFillPhase256ObservedFieldExtractionContract === false, "Phase459 cannot fill Phase201 or Phase256 contracts.");
  assert(phase459.routePromotesWzMasses === false && phase459.routePromotesHiggsMass === false && phase459.routeCompletesBosonPredictions === false, "Phase459 cannot promote boson predictions.");
  assert(phase101Package?.spectroscopyRecordAttestation?.spectroscopyRecordAttestationPassed === true, "Phase101 must include the Phase459 block.");
```

## 6. Scanner exclusion registrations

Required (my Program.cs and output JSONs carry the standard boundary-field
key names, which these four scanners classify; same profile as phase452,
which is registered in exactly these four):

(a) `studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs` —
in `IsGeneratedAuditOrImplementationJson`, insert after
`|| path.Contains("studies/phase453_", StringComparison.Ordinal)` (currently
line 455; keep the trailing `;` on the new last line):

```csharp
    || path.Contains("studies/phase459_", StringComparison.Ordinal)
```

(b) `studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs`
— same helper, insert after the `studies/phase453_` line (currently line
476), same form:

```csharp
    || path.Contains("studies/phase459_", StringComparison.Ordinal)
```

(c) `studies/phase295_observed_field_extraction_contract_candidate_scan_001/Program.cs`
— insert after the `studies/phase453_` pair (currently lines 855-856; keep
the trailing `;`):

```csharp
    || normalizedPath.StartsWith("studies/phase459_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase459_", StringComparison.Ordinal)
```

(d) `studies/phase296_source_lineage_contract_field_candidate_scan_001/Program.cs`
— insert after the `studies/phase453_` pair (currently lines 863-864), same
form:

```csharp
    || normalizedPath.StartsWith("studies/phase459_", StringComparison.Ordinal)
    || normalizedPath.Contains("/studies/phase459_", StringComparison.Ordinal)
```

NOT required (verified by matching the phase459 sources and output against
each scanner's committed term list — zero hits, the same profile as
phase452, which is likewise unregistered there): phase207 (line-level
potential-term scan), phase253 (dimension-4 regex), phase279 and phase281
(topic term lists), and the six scripts-root scanner helpers (phase459
writes nothing under `scripts/`). `docs/Phases/Implementation/IMPLEMENTATION_P459.md`
needs no registration (no scanner terms; same profile as
IMPLEMENTATION_P452/P453.md). If the post-wiring `--incremental` pass
disagrees, the failing scanner's output JSON names the match — register
deliberately, never weaken.

## 7. Docs (already in this worktree branch — carried by the merge, no action)

- `docs/Phases/Implementation/IMPLEMENTATION_P459.md`
- `studies/phase459_spectroscopy_record_attestation_001/STUDY.md`

## 8. Checkpoint notes (program-state edits at the wiring checkpoint)

- `docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md`: next A number becomes 460
  (registry line "next B number 453, next A number 459, next C number 465");
  the Phase452 gate bullet's "formal attestation phase queued as A-team's
  first registry number" becomes "formal attestation STANDING as phase459";
  add a Phase459 gate-status bullet
  (`spectroscopyRecordAttestationPassed=True`,
  `verdictKind=record-reconciled-canonical`, 31/31 checks).
- `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`: journal entry for the
  phase459 checkpoint (A0 discharged; O3 of the program document closes —
  the canonical pole numbers are now a committed machine-checked
  attestation, not session-level verification).
- `docs/Phases/PHASE_NUMBER_REGISTRY.md`: no edit needed (459's first
  planned use is exactly this phase).
- `scripts/boson_incremental_manifest.json`: never hand-edited; the next
  `--full` pass re-seeds it (and `--incremental` will simply run the new
  phase until then).
- Phase outputs are gitignored: force-add
  `studies/phase459_spectroscopy_record_attestation_001/output/*.json` at
  checkpoint time, per standing practice.
- OPTIONAL alignment fix, flagged during phase459 construction: the
  committed `docs/Phases/Implementation/IMPLEMENTATION_P452.md` and
  `studies/phase452_scalar_channel_spectroscopy_probe_001/STUDY.md` still
  carry the superseded 2.4352/2.4547 prose and the reduced-budget run
  description (the Wave-0 reconciliation rewrote the journal/restart prompt
  but not these two). Phase459 deliberately does NOT scan docs (only the
  output JSONs), so nothing is red, but reconciling those two docs (or
  annotating them as superseded history) would complete the record. The
  phase202 checklist description for phase452 ("a*m ~ 2.44") has the same
  stale number; snippet 4(d) above does not touch it — fix in the same
  checkpoint if desired.

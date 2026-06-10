# Boson Prediction Agent Restart Prompt

Use this prompt to restart the GeometricUnity boson-prediction investigation
from the current checkpoint.

## Prompt For The Next Agent

You are working in `/home/josh/Documents/GitHub/GeometricUnity` on the
Geometric Unity boson-prediction/source-law investigation. Continue in the same
style: read the repo first, keep edits scoped, preserve scientific caution, keep
the journal and reference ledger current, validate before committing, and do not
promote W/Z/H masses unless the source-lineage contracts are genuinely filled.

### Operating Rules

- Use `rg`/`rg --files` first for searches.
- Use `apply_patch` for manual edits.
- Do not revert user or unrelated work. The worktree may be dirty.
- Avoid destructive git commands.
- If broad research or diagnosis is needed and multi-agent tooling is
  available, launch a read-only explorer/subagent and incorporate its findings.
- Keep a running diagnosis journal in
  `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.
- Track references through `ExperimentReferences.md` plus detailed notes under
  `docs/Reference/ExperimentReferences/`.
- Before treating any source as promotion evidence, require exact source
  lineage, observed-field extraction, target independence, pole extraction, and
  GeV/unit normalization.
- If external research is needed, use current primary/official sources where
  possible and add/update reference detail notes before using the source.

### Current Scientific Status

No successful physical W/Z/H prediction has been achieved. The current package
still blocks physical comparison because the source-lineage and observed-field
contracts are empty.

Current gate status after the Phase396 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=189`,
  `checklistFailedCount=3`
- Claim integrity:
  `boson-claim-integrity-verified`,
  `sourceLineageMissing=true`, `wzMissingFieldCount=15`,
  `higgsMissingFieldCount=14`, `promotedPhysicalMassClaimCount=0`
- Phase389:
  `vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed=True`,
  `discreteGaugeCompatibilityIdentityExact=True`,
  `canFillPhase201WzContract=False`
- Phase390:
  `convergedControlBranchFermionModeRebuildPassed=True`,
  `persistedPhase12ModeBranchUnconverged=True`,
  `wardZeroCurrentSharplyTested=True`
- Phase391:
  `denseReplayVerdict=confirmed` (Gram invariants solver-independent)
- Phase392:
  `actionDerivedResponseStructureVerdict=diverges-from-gram-structure`
  (suppressed axis is metric-dependent)
- Phase393:
  `shellAggregatedSourceCancels=True`, `perModeSourceLiesInGramImage=True`
- Phase394:
  `firstOrderAsymmetricBackreactionConstructed=True`,
  `tripletClusteringObserved=True`
- Phase395:
  `suppressedAxisIsGaugeCovariantNotCanonical=True`,
  `blockGramEffectivelyRankOne=True`
- Phase396:
  `gaugeInvariantNeutralChargedSectorSeparationProbePassed=True`,
  `extractionGaugeInvarianceVerified=True`,
  `tripletNeutralChargedSplitObserved=True`,
  `totalTripletClusterCount=68`,
  `canFillPhase256ObservedFieldExtractionContract=False`

Interpretation: the gauge-invariant sector skeleton is COMPLETE on the
control branch. Phase395 explained the axis structure (rank-one response in
the charged plane orthogonal to the symmetric-ansatz axis n_omega;
gauge-covariant, not canonical). Phase396 then materialized the simplest
gauge-invariant extraction (neutral fraction relative to n_omega; exactly
invariant at 7.8e-16) and found the residual-U(1) multiplet structure is
EXACT: all 68 bosonic su(2) triplets split as one neutral plus one charged
pair (deviation <= 1.9e-7), and the 18-dim kernel splits 6+12. This is the
discrete skeleton of {Z-like, W-pair-like} classification, built from
invariants only. The remaining gap to Phase256/Phase201 is irreducibly
physical: an electroweak embedding (su(2) x U(1)_Y with hypercharge) on a
four-dimensional observed-vacuum branch, plus coupling/VEV/pole/GeV lineage.
These cannot be synthesized from the toy branch; they require the completed
GU derivation chain (VO-6/VO-7 solved physically) or new theorem-level
source material.

### Most Recent Implemented Work

The latest work added Phase396:

- Study:
  `studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001`
- Project: `Phase396GaugeInvariantNeutralChargedSectorSeparationProbe.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P396.md`
- Outputs:
  `studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/output/gauge_invariant_neutral_charged_sector_separation_probe.json`
  and `..._summary.json`
- Precursor: reads the Phase394 working directory (run Phase394 first).

Phase396 materialized the extraction f(b) = sum_e (b_e . n_omega)^2 / |b|^2
and the basis-invariant cluster neutral content:

- All 68 bosonic triplets: neutral content 1.0 (deviation <= 1.9e-7);
  kernel splits exactly 6 neutral + 12 charged.
- Extraction exactly gauge-invariant (7.8e-16).
- Honest per-field Phase256 audit: zero of the 20 fields fillable on the
  su(2)-only toy branch (recorded reasons: no U(1)_Y, no photon/Z mixing,
  no weak angle, no scalar sector, no 4D vacuum, no scale/pole/GeV).

### Integration Points Already Updated

Phase396 (like Phase388-395) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `gauge-invariant-neutral-charged-sector-separation-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/DRESSING-FIELD-ELECTROWEAK-OBSERVED-VARIABLES.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001/Phase396GaugeInvariantNeutralChargedSectorSeparationProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase396 line, the Phase202 incomplete
status (`checklistPassedCount=189`, `checklistFailedCount=3`), and the same
claim-integrity status (`promotedPhysicalMassClaimCount=0`). All seven broad
scanners still report zero intake-ready evidence.

### Current Reference Structure

`ExperimentReferences.md` is the top-level source ledger. Each row should have
a linked detail note under `docs/Reference/ExperimentReferences/`.

When adding or revisiting a source:

1. Add/update the row in `ExperimentReferences.md`.
2. Add/update the linked detail note.
3. Record how the source was used.
4. State exactly what source-lineage fields it can and cannot supply.
5. Update `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md` when it changes the
   diagnosis or confirms a negative boundary.

Important current local detail notes:

- `LOCAL-COMPLETION-V29-FERMIONIC-YUKAWA.md`
- `LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION.md`
- `DIRAC-SHELL-RESPONSE-BOUNDARY.md`
- `COX-FIRST-PRINCIPLES-I-19800512.md`

### Best Next Work

Do not try to promote another numerical near-pass. The next useful work must
either find or derive a theorem-level artifact that can fill the missing
contract fields, or remove a physical blocker on the VO-7 branch.

The most useful next branches are:

1. An electroweak embedding branch: extend the control construction from
   su(2)-only to su(2) x U(1)_Y with hypercharge assignments (the repo has
   su(2)/su(3) algebra factories; a u(1) extension plus a mixed embedding
   would let the Phase396 neutral sector become a genuine photon/Z mixing
   problem with a weak angle). This is the only route by which the now-exact
   sector skeleton can address Phase256's electroweakGaugeEmbeddingId and
   photonEigenstateProjectionId fields. Fail-closed: an embedding without
   derived hypercharges and scales fills nothing.
2. A complete W/Z/H source package: separate W/Z source rows, Higgs scalar
   source row, weak-angle/coupling lineage, VEV/source scale, pole
   extraction, and GeV normalization - requires the completed GU derivation
   chain (VO-6/VO-7 solved physically) or new theorem-level sources.
3. Optional internal hygiene: re-express the Phase307/381/383 suppressed-axis
   blockers in gauge-invariant language so future selector audits are
   frame-independent.

If a source or new derivation appears to satisfy any of these, create a new
fail-closed phase rather than editing Phase201/Phase256 directly. The phase
should prove target independence, check every required contract field, and then
let the existing gates decide whether promotion is allowed.

### Start-Up Checklist

Run these first:

```bash
git status --short
git log -3 --oneline
tail -120 docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md
rg -n "Phase396|tripletNeutralChargedSplitObserved|sector separation" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase396_gauge_invariant_neutral_charged_sector_separation_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase396 output JSON files, and commit a checkpoint
after validation.

Suggested checkpoint message:

```text
Add phase396 gauge-invariant sector separation probe
```

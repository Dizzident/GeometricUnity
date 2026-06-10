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

Current gate status after the Phase394 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=187`,
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
  `positiveBosonicSpectrumBackreactionConstructionPassed=True`,
  `positiveBosonicSpectrumRecomputed=True`,
  `bosonicGaussNewtonPsdVerified=True`,
  `firstOrderAsymmetricBackreactionConstructed=True`,
  `tripletClusteringObserved=True`,
  `canFillPhase201WzContract=False`

Interpretation: the coupled-critical-point toolkit is complete at first and
second order on the control branch. Phase394 recomputed the full positive
bosonic Gauss-Newton spectrum through the production compute-spectrum
pipeline on a study-local family copy: PSD, kernel dimension exactly 18
(persisted 12 contained at 1.000000), spectral gap 0.062942, max eigenvalue
6.017, and EXACT su(2) triplet clustering of every positive eigenvalue. The
first-order asymmetric backreaction is now constructed (norms ~0.44 per unit
coupling; relaxation energies ~0.029 per coupling squared; unabsorbable
kernel fractions 0.12-0.13). Diagnosis refinement: the backreaction
DIRECTION re-exhibits the suppressed gauge axis (fractions
[0.5426, 0.0008, 0.4566] / [0.5296, 0.0007, 0.4697], nearly identical to the
Phase379 Gram) because the source currents lie in the rank-3 Gram image
(Phase393). So the suppression is absent from the fermion-loop response
operator (Phase392) but present in the source-driven boson displacement -
an action-derived dynamical anchor. The sharpest open internal question:
WHY do the fermionic source currents avoid gauge axis 1?

### Most Recent Implemented Work

The latest work added Phase394:

- Study:
  `studies/phase394_positive_bosonic_spectrum_backreaction_construction_001`
- Project: `Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P394.md`
- Outputs:
  `studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/positive_bosonic_spectrum_backreaction_construction.json`
  and `..._summary.json` (the recomputed spectrum bundles live under
  `output/family_workdir/` and are reproducible rather than committed)

Phase394 stages a study-local copy of the Phase12 family and re-runs the
production `Gu.Cli compute-spectrum <workdir> <bg> --num-modes 156`, then:

- Verifies PSD, kernel dimension 18, spectral gap 0.062942, exact su(2)
  triplet clustering, and persisted-kernel containment 1.000000.
- Constructs `delta_omega^(s) = -sum m_i (m_i . J^(s)) / mu_i` per unit
  coupling (norms 0.4425 / 0.4349) with relaxation energies
  0.0280 / 0.0295 per coupling squared.
- Finds the backreaction direction re-exhibits the suppressed gauge axis
  (fractions [0.5426, 0.0008, 0.4566] / [0.5296, 0.0007, 0.4697]).

### Integration Points Already Updated

Phase394 (like Phase388-393) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `positive-bosonic-spectrum-backreaction-construction-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/DIRAC-SHELL-RESPONSE-BOUNDARY.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/Phase394PositiveBosonicSpectrumBackreactionConstruction.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase394 line, the Phase202 incomplete
status (`checklistPassedCount=187`, `checklistFailedCount=3`), and the same
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

1. A target-blind structural decomposition of the fermionic source currents
   by edge/axis/representation content, to answer the sharpest open internal
   question: WHY the source currents avoid gauge axis 1. This is the
   dynamical anchor of the suppressed-axis structure (Phase394) and the
   closest internal route to the still-missing suppressed-axis W-row
   theorem.
2. A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem
   filling Phase256 observed-field extraction fields. Any axis-structure
   theorem must fix the response metric: the suppression lives in the Gram
   image and the backreaction direction, not the loop response operator.
3. A complete W/Z/H source package: separate W/Z source rows, Higgs scalar
   source row, weak-angle/coupling lineage, VEV/source scale, pole extraction,
   and GeV normalization.

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
rg -n "Phase394|positiveBosonicSpectrumRecomputed|tripletClusteringObserved" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase394_positive_bosonic_spectrum_backreaction_construction_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase394 summary and full output JSON files (NOT the
`family_workdir` tree, which is reproducible and timestamp-volatile), and
commit a checkpoint after validation.

Suggested checkpoint message:

```text
Add phase394 positive bosonic spectrum and backreaction
```

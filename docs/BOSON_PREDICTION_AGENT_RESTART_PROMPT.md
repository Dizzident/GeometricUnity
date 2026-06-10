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

Current gate status after the Phase391 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=184`,
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
  `denseConvergedShellResponseReplayAuditPassed=True`,
  `denseReplayVerdict=confirmed`,
  `denseReplayConfirmsRankThree=True`,
  `denseReplayConfirmsSuppressedAxis=True`,
  `canFillPhase201WzContract=False`

Interpretation: the artifact question is settled. Phase391 replayed the
Phase378/379 shell-response pipeline on the exact dense generalized
eigensolve and CONFIRMED the invariants to ~1e-10: the rank-3 carrier image
and the suppressed gauge axis 1 are solver-independent properties of the
discretized control branch, not artifacts of the iterative weighted solver
(and the Phase12 persisted-mode defect found by Phase390 never touched the
Phase378 shell, which was solved fresh in-study). The Phase381/383/384
suppressed-axis blockers against the Phase307 W near-pass stand on
solver-independent ground.

### Most Recent Implemented Work

The latest work added Phase391:

- Study:
  `studies/phase391_dense_converged_shell_response_replay_audit_001`
- Project: `Phase391DenseConvergedShellResponseReplayAudit.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P391.md`
- Outputs:
  `studies/phase391_dense_converged_shell_response_replay_audit_001/output/dense_converged_shell_response_replay_audit.json`
  and `..._summary.json`

Phase391 replayed the exact Phase378/379 pipeline (shell rule, coordinate
blocks, dual-Gram rank rule, axis fractions, transport) on the Phase390
dense eigensolve and found:

- Shell eigenvalues match the Phase378 weighted-solver shell to 1.5e-10.
- Positive rank 3 and suppressed gauge axis 1 on both backgrounds (axis
  fractions match Phase379 to 1.7e-10).
- Inter-background minimum transport singular value 0.79970408362 (matches
  Phase379 to 2.2e-11); strict transport still fails.
- Side result: the Phase374-repaired weighted solver is validated to ~1e-10.

### Integration Points Already Updated

Phase391 (like Phase388/389/390) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `dense-converged-shell-response-replay-audit-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/DIRAC-SHELL-RESPONSE-BOUNDARY.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase391_dense_converged_shell_response_replay_audit_001/Phase391DenseConvergedShellResponseReplayAudit.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase391 line, the Phase202 incomplete
status (`checklistPassedCount=184`, `checklistFailedCount=3`), and the same
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

1. A coupled boson-fermion second-variation (mixed Hessian) construction on
   the Phase390 rebuilt converged branch, using the Phase389 identity as the
   gauge-compatibility template, replacing the study-defined shell-response
   Gram with an action-derived source operator. After Phase391, this is the
   only repo-local route that could change the carrier-image structure.
2. A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem
   filling Phase256 observed-field extraction fields.
3. A theorem explaining why the physical W row must use the
   Phase379-suppressed carrier axis (now confirmed solver-independent by
   Phase391).
4. A complete W/Z/H source package: separate W/Z source rows, Higgs scalar
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
rg -n "Phase391|denseReplayVerdict|denseReplayConfirmsSuppressedAxis" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase391_dense_converged_shell_response_replay_audit_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add ignored Phase391 output JSON files, and commit a checkpoint after
validation. The output directory under `studies/**/output/` is generally
ignored, so use `git add -f` for Phase391 output files if they are meant to be
committed.

Suggested checkpoint message:

```text
Add phase391 dense converged-shell response replay
```

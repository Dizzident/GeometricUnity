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

Current gate status after the Phase390 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=183`,
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
  `mPsiCompatibleGeneralizedControlBranchMaterialized=True`,
  `persistedPhase12ModeBranchUnconverged=True`,
  `wardZeroCurrentSharplyTested=True`,
  `canFillPhase201WzContract=False`

Interpretation: the VO-7 branch now has converged control-branch fermion
modes on both the identity-weight and mesh-volume `M_psi` branches, the
gauge-compatibility identity (Phase389) carries over exactly to the `M_psi`
branch because `[M_psi, X_hat] = 0`, and the pure-gauge Ward zero-current
statement is sharply verified. Critically, Phase390 proved the persisted
Phase12 fermion modes are non-eigen mixtures (relative residuals 12-18, best
overlap with any true eigenvector 0.376). The Phases 376-388 shell-response
chain used those modes as fixed test vectors, so the Phase378 rank-3 carrier
image and Phase379 suppressed axis may be artifacts of unconverged modes.

### Most Recent Implemented Work

The latest work added Phase390:

- Study:
  `studies/phase390_converged_control_branch_fermion_mode_rebuild_001`
- Project:
  `Phase390ConvergedControlBranchFermionModeRebuild.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P390.md`
- Outputs:
  `studies/phase390_converged_control_branch_fermion_mode_rebuild_001/output/converged_control_branch_fermion_mode_rebuild.json`
  and `..._summary.json`

Phase390 verified, on both persisted Phase12 backgrounds:

- Converged eigenpairs on both branches (in-study complex Hermitian Jacobi;
  eigen-residuals <= 3.1e-13; M-orthonormality at solver precision).
- Exact 48-dimensional structural kernel per branch from the toy mesh's 4
  isolated vertices; target-blind selection takes the 12 smallest-|lambda|
  NONZERO eigenpairs.
- `[M_psi, X_hat] = 0` exactly, conjugating the Phase389 identity onto the
  M_psi-compatible generalized branch.
- Sharp pure-gauge Ward zero-current across 4032 rows (max current 9.3e-13).
- Persisted Phase12 modes confirmed unconverged (non-eigen mixtures).

### Integration Points Already Updated

Phase390 (like Phase388/389) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `converged-control-branch-fermion-mode-rebuild-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase390_converged_control_branch_fermion_mode_rebuild_001/Phase390ConvergedControlBranchFermionModeRebuild.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase390 line, the Phase202 incomplete
status (`checklistPassedCount=183`, `checklistFailedCount=3`), and the same
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

1. **Converged-mode shell-response replay (cheap and decisive)**: re-run the
   Phase376-379 shell-response Gram and carrier-axis characterization using
   the Phase390 converged modes instead of the unconverged persisted modes.
   This decides whether the Phase378 rank-3 carrier image and the Phase379
   suppressed axis (which block the Phase307 W near-pass) are properties of
   the discretized physics or artifacts of mode unconvergence. Either outcome
   is a major diagnostic update.
2. A coupled boson-fermion second-variation (mixed Hessian) construction on
   the rebuilt converged branch, using the Phase389 identity as the
   gauge-compatibility template, replacing the study-defined shell-response
   Gram with an action-derived source operator.
3. A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem
   filling Phase256 observed-field extraction fields.
4. A theorem explaining why the physical W row must use the
   Phase379-suppressed carrier axis (if the suppressed axis survives the
   converged-mode replay of branch 1).
5. A complete W/Z/H source package: separate W/Z source rows, Higgs scalar
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
rg -n "Phase390|convergedControlBranchFermionModeRebuild|persistedPhase12ModeBranchUnconverged" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase390_converged_control_branch_fermion_mode_rebuild_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add ignored Phase390 output JSON files, and commit a checkpoint after
validation. The output directory under `studies/**/output/` is generally
ignored, so use `git add -f` for Phase390 output files if they are meant to be
committed.

Suggested checkpoint message:

```text
Add phase390 converged control-branch mode rebuild
```

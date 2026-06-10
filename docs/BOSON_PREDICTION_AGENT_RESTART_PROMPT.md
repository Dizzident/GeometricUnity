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

Current gate status after the Phase392 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=185`,
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
  `coupledMixedHessianFermionInducedResponseAuditPassed=True`,
  `actionDerivedResponseStructureVerdict=diverges-from-gram-structure`,
  `actionDerivedResponseSharesRankThree=False`,
  `actionDerivedResponseSharesSuppressedAxis=False`,
  `canFillPhase201WzContract=False`

Interpretation: the carrier-image question is now resolved into a sharp
metric statement. Phase391 proved the study-defined Hilbert-Schmidt Gram
invariants (rank 3, suppressed gauge axis 1) are solver-independent.
Phase392 then built the ACTION-DERIVED fermion-induced response (Schur
complement of the coupled candidate mixed Hessian on the converged shell)
and found it diverges: near-full rank (146/141 of 156), mixed signature,
nearly isotropic gauge axes (~[0.33,0.33,0.33], argmin unstable). The
suppressed-axis obstruction is therefore METRIC-DEPENDENT: decisive against
the Gram-route promotion of the Phase307 near-pass, but not a physical
statement about the coupled dynamics. Scope limits: the background is not a
coupled critical point (omega solved bosonic-only) and the action is the
candidate bilinear.

### Most Recent Implemented Work

The latest work added Phase392:

- Study:
  `studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001`
- Project: `Phase392CoupledMixedHessianFermionInducedResponseAudit.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P392.md`
- Outputs:
  `studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001/output/coupled_mixed_hessian_fermion_induced_response_audit.json`
  and `..._summary.json`

Phase392 materialized the VO-7 candidate mixed-Hessian blocks
`2 delta_D[e_k] psi_s` on the Phase390 converged shell and Schur-complemented
the fermion fluctuation exactly in the dense generalized eigenbasis:

- `R_kl = sum_s Re<delta_D[e_k] psi_s, (D - lambda_s M)^+ delta_D[e_l] psi_s>`
- Significant rank 146 (bg-a, 70+/76-) and 141 (bg-b, 69+/72-) of 156.
- Gauge-axis fractions ~[0.334, 0.328, 0.337] and [0.332, 0.336, 0.332]:
  no suppressed axis, argmin unstable across backgrounds.
- Exact response symmetry; retained denominators >= 8.4e-4; shell residuals
  <= 2.9e-13; pure-gauge response up to ~30x the generic scale (candidate
  action not gauge-invariant off the coupled critical point).

### Integration Points Already Updated

Phase392 (like Phase388-391) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `coupled-mixed-hessian-fermion-induced-response-audit-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/DIRAC-SHELL-RESPONSE-BOUNDARY.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001/Phase392CoupledMixedHessianFermionInducedResponseAudit.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase392 line, the Phase202 incomplete
status (`checklistPassedCount=185`, `checklistFailedCount=3`), and the same
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

1. A coupled-critical-point construction: re-solve the background with the
   fermionic backreaction included (coupled residual), so the Phase392
   second variation becomes a genuine coupled Hessian and the gauge sector
   can be handled consistently (Phase389 identity as the gauge-compatibility
   template; a Coulomb-type slice exists in Gu.Phase2). This upgrades the
   action-derived response from fixed-background to self-consistent and is
   the remaining constructive VO-7 step on the control branch.
2. A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem
   filling Phase256 observed-field extraction fields. Note after Phase392:
   any axis-structure theorem must first fix the response metric (the
   Gram shows suppression; the action-derived response does not).
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
rg -n "Phase392|actionDerivedResponseStructureVerdict|metric-dependent" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add ignored Phase392 output JSON files, and commit a checkpoint after
validation. The output directory under `studies/**/output/` is generally
ignored, so use `git add -f` for Phase392 output files if they are meant to be
committed.

Suggested checkpoint message:

```text
Add phase392 action-derived fermion-induced response audit
```

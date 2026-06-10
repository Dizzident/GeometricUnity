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

Current gate status after the Phase393 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=186`,
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
  `coupledStationarityFermionicSourceResidualProbePassed=True`,
  `shellAggregatedSourceCancels=True`,
  `perModeSourceLiesInGramImage=True`,
  `persistedBosonicSpectrumIsNumericalKernelOnly=True`,
  `firstOrderBackreactionConstructibleFromPersistedArtifacts=False`,
  `canFillPhase201WzContract=False`

Interpretation: the coupled-critical-point program now has its first-order
picture. Phase393 showed the shell-aggregated fermionic source cancels
EXACTLY between plus/minus eigenvalue partners, so under symmetric shell
occupation the persisted background is already first-order
coupled-stationary and the Phase392 second-order response operator is the
leading backreaction object (softening Phase392's non-critical-point caveat
at first order). Each per-mode source lies identically in the rank-3 Gram
image, with 0.61-0.68 pure-gauge content and 0.08-0.11 in the persisted
bosonic numerical kernel. The persisted Phase12 bosonic Gauss-Newton
spectrum consists entirely of ~1e-15 kernel directions, so asymmetric
first-order backreaction is NOT constructible from persisted artifacts -
the positive bosonic spectrum at these backgrounds is the concrete missing
artifact. Earlier: Phase391 proved the Gram invariants solver-independent;
Phase392 proved the suppressed axis is metric-dependent (the action-derived
response is near-full-rank and isotropic).

### Most Recent Implemented Work

The latest work added Phase393:

- Study:
  `studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001`
- Project: `Phase393CoupledStationarityFermionicSourceResidualProbe.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P393.md`
- Outputs:
  `studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/output/coupled_stationarity_fermionic_source_residual_probe.json`
  and `..._summary.json`

Phase393 characterized the coupled stationarity residual
`J_k = Re<psi_s, delta_D[e_k] psi_s>` on the converged shell:

- Exact plus/minus cancellation of the shell-aggregated source
  (ratio ~4e-11; per-mode norms identical: 0.1129 / 0.1212).
- Per-mode sources lie exactly in the rank-3 Gram image (fraction 1.0);
  pure-gauge content 0.61-0.68; persisted-bosonic-kernel content 0.08-0.11.
- All 12 persisted bosonic Gauss-Newton eigenvalues per background ~1e-15
  (kernel-only): backreaction `-kappa H_B^+ J` not constructible from
  persisted artifacts.
- Unit-source shell splitting: doubly degenerate +-6.49e-3 / +-7.22e-3 per
  unit coupling.

### Integration Points Already Updated

Phase393 (like Phase388-392) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `coupled-stationarity-fermionic-source-residual-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/DIRAC-SHELL-RESPONSE-BOUNDARY.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/Phase393CoupledStationarityFermionicSourceResidualProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase393 line, the Phase202 incomplete
status (`checklistPassedCount=186`, `checklistFailedCount=3`), and the same
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

1. Recompute the positive bosonic Gauss-Newton spectrum at the Phase12
   backgrounds (the persisted spectrum is kernel-only). This is the concrete
   missing artifact for asymmetric backreaction and for any second-order
   coupled self-consistency check. The Phase3 OperatorBundleBuilder /
   spectra machinery is the natural route.
2. Formalize the second-order coupled expansion around symmetric shell
   occupation, where Phase393 proved first-order stationarity and the
   Phase392 response operator is the leading fermion-induced boson operator.
   Fail-closed: no physical coupling, no contract fields.
3. A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem
   filling Phase256 observed-field extraction fields. Note after Phase392:
   any axis-structure theorem must first fix the response metric.
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
rg -n "Phase393|shellAggregatedSourceCancels|persistedBosonicSpectrumIsNumericalKernelOnly" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add ignored Phase393 output JSON files, and commit a checkpoint after
validation. The output directory under `studies/**/output/` is generally
ignored, so use `git add -f` for Phase393 output files if they are meant to be
committed.

Suggested checkpoint message:

```text
Add phase393 coupled stationarity residual probe
```

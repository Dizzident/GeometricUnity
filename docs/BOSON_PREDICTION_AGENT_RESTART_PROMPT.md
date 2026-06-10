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

Current gate status after the Phase395 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=188`,
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
  `sourceCurrentAxisStructureGaugeCovarianceProbePassed=True`,
  `globalGaugeCovarianceVerified=True`,
  `suppressedAxisIsGaugeCovariantNotCanonical=True`,
  `backgroundOmegaNearSymmetricAnsatz=True`,
  `blockGramEffectivelyRankOne=True`,
  `canFillPhase201WzContract=False`

Interpretation: the axis question is ANSWERED. The background omega is the
symmetric ansatz (invariant axis n_omega ~ (1,1,1)/sqrt(3), fraction ~0.97).
The shell-response block Gram is effectively rank-ONE (dominant fraction
>= 0.9992) along a single direction d lying in the charged plane orthogonal
to n_omega (87-89 degrees). The Phase379 "two-axis dominance with suppressed
coordinate axis 1" is exactly the coordinate shadow of d (d^2 components
[0.5433, 0.0009, 0.4558] / [0.5197, 0.0002, 0.4802] reproduce the persisted
fractions). Exact global gauge covariance was verified at ~1e-10 using
rotated backgrounds built from persisted artifacts alone
(D' = D - delta_D[omega] + delta_D[R omega]): rotating the background
rotates d. Consequences: raw-coordinate axis statements (Phase307 selectors,
Phase381/383/384 blockers) are gauge-frame statements and cannot become
theorems; any observed photon/W/Z/H namespace map must be built from
gauge-invariant data (component along n_omega vs the charged plane,
rotation-invariant magnitudes). The internal diagnostic program on this
control branch is essentially complete; what remains is genuinely
theorem-level (Phase256 namespace map, Phase201 source package).

### Most Recent Implemented Work

The latest work added Phase395:

- Study:
  `studies/phase395_source_current_axis_structure_gauge_covariance_probe_001`
- Project: `Phase395SourceCurrentAxisStructureGaugeCovarianceProbe.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P395.md`
- Outputs:
  `studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/output/source_current_axis_structure_gauge_covariance_probe.json`
  and `..._summary.json`

Phase395 computed the basis-invariant per-edge block Gram on the converged
shell, the omega second-moment invariants, and exact global gauge
covariance checks (rotated backgrounds via Phase389 linearity):

- Omega is the symmetric ansatz: invariant axis ~(1,1,1)/sqrt(3),
  dominant fraction 0.969/0.971.
- Block Gram effectively rank-one (>= 0.9992) along d = (0.737, 0.031,
  -0.675) / (0.721, 0.013, -0.693), orthogonal to the omega axis (87-89
  degrees); the d^2 coordinate shadow reproduces the Phase379 fractions.
- Covariance residuals <= 9.5e-11 (spectrum invariance and T' = R T R^T)
  for a quarter turn and a generic rotation.

### Integration Points Already Updated

Phase395 (like Phase388-394) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `source-current-axis-structure-gauge-covariance-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/DIRAC-SHELL-RESPONSE-BOUNDARY.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/Phase395SourceCurrentAxisStructureGaugeCovarianceProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase395 line, the Phase202 incomplete
status (`checklistPassedCount=188`, `checklistFailedCount=3`), and the same
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

1. A GAUGE-INVARIANT observed-field extraction route for Phase256: after
   Phase395, any defensible namespace map must be built from invariants
   relative to the background invariant axis n_omega (invariant-axis
   component vs charged plane, rotation-invariant magnitudes) - a
   dressing-style construction. Survey primary literature (FMS/dressing
   field already catalogued in Phase385) for a theorem matching exactly this
   discrete structure, or derive a fail-closed candidate and let the gates
   decide.
2. A complete W/Z/H source package: separate W/Z source rows, Higgs scalar
   source row, weak-angle/coupling lineage, VEV/source scale, pole
   extraction, and GeV normalization. Internal diagnostics cannot
   substitute.
3. Optional internal hygiene: re-express the Phase307/381/383 suppressed-axis
   blockers in gauge-invariant language (component along n_omega vs charged
   plane) so future selector audits are frame-independent.

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
rg -n "Phase395|suppressedAxisIsGaugeCovariantNotCanonical|blockGramEffectivelyRankOne" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase395_source_current_axis_structure_gauge_covariance_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase395 output JSON files, and commit a checkpoint
after validation.

Suggested checkpoint message:

```text
Add phase395 axis structure gauge covariance probe
```

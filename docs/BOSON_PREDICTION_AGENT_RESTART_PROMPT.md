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

Current gate status after the Phase400 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=193`,
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
  `tripletNeutralChargedSplitObserved=True` (all 68 triplets, exact)
- Phase397:
  `neutralMixingElementVanishesInFermionBilinearChannel=True`,
  `photonZSeparationUnderdetermined=True`
- Phase398:
  `vo6ControlBranchComponentsComplete=True` (5/5),
  `vo7ControlBranchComponentsComplete=True` (4/4),
  `physicalCompletionStillMissing=True` (8-item gap ledger)
- Phase399:
  `quadraticModelCoupledCriticalPointSolvePassed=True`,
  `quadraticModelCoupledFixedPointConverged=True` (8/8 runs, gradient <= 9.5e-11),
  `flatDirectionObstructionPresent=True` (0.047 per unit kappa),
  `kappaScalingConsistentWithFirstOrder=True`,
  `vo7CoupledStationarityControlComponentDischarged=True`
- Phase400:
  `fullBosonicActionFlatDirectionLiftProbePassed=True`,
  `liftVerdict=all-lifted-with-saddle-directions` (36/36 kernel directions
  lifted, 0 exactly flat, quartic norms 1.2e-3..5.8e-2, saddle depth
  <= 4.5e-19 residual-scale),
  `kernelObstructionFullyRelaxableAtHigherOrder=True`,
  `minObstructionLiftedFraction=1`

Interpretation: the control-branch program has traced every
electroweak-shaped gap to its physical root. The sector skeleton is exact
(Phase396: all 68 triplets split 1 neutral + 1 charged pair). Phase397's
u(1) extension then found: (1) the Z-like channel is SOURCELESS (su(2)-
neutral source fraction <= 0.0023; the coupled residual is purely
charged-sector); (2) the fermion-bilinear neutral mixing element VANISHES
identically (trace selection rule) - photon/Z mixing requires a
symmetry-breaking scalar/VEV sector, welding the Phase256 photon-projection
gap to the Phase201 Higgs scalar-source gap, exactly as in the SM where
W3-B mixing enters through the Higgs; (3) the photon/Z separation is a
one-parameter family blocked by the named gap {hypercharge lineage,
coupling-ratio lineage, scalar sector}. Internal toy-branch construction
has reached its honest limit: the remaining requirements (scalar sector,
hypercharge/coupling lineage, 4D observed vacuum, scale/pole/GeV) all live
in the unsolved GU derivation chain (VO-6/VO-7 physically completed) or new
theorem-level sources.

### Most Recent Implemented Work

The latest work added Phase400, the full-bosonic-action flat-direction lift
probe, which answers the residual question Phase399 recorded: because the
toy bosonic objective is exactly QUARTIC (Upsilon = F - T^aug is quadratic
in omega), the expansion along Gauss-Newton kernel directions terminates
exactly, and the GN-dropped curvature form B_ij = <U0, M Q(k_i,k_j)> was
assembled by polarization (second differences of the production residual
assembly, no truncation error) and diagonalized. ALL 36 kernel directions
(18 x 2 backgrounds) are LIFTED (quartic norms 1.2e-3..5.8e-2, zero exactly
flat; quadratic coefficients are residual-scale, Cauchy-Schwarz-bounded,
saddle depth <= 4.5e-19); the fermionic source kernel component lies
entirely in lifted directions, so THE PHASE399 OBSTRUCTION IS FULLY
RELAXABLE AT HIGHER ORDER - the coupled critical point exists as a genuine
critical point of the full toy action. Cross-check: source kernel
norm-fractions square to the Phase394 energy-fractions (0.121/0.134). The
kernel is not gauge-dominated (gauge fractions <= 0.46). Study:
`studies/phase400_full_bosonic_action_flat_direction_lift_probe_001`
(IMPLEMENTATION_P400.md; reads the Phase394 working directory). The
COUPLED-STATIONARITY COMPONENT IS NOW CLOSED AT THE FULL TOY-ACTION
CONTROL-BRANCH LEVEL. Before that, Phase399 solved the quadratic-model
self-consistent coupled critical point (8/8 runs, projected gradient
<= 9.5e-11, obstruction 0.047 per unit kappa recorded, kappa = 0.1
perturbative boundary documented), and Phase398 was the v29 VO-6/VO-7
completion ledger audit (VO-6 5/5, VO-7 4/4, EW chain 3/3, 8-item physical
gap ledger headed by the symmetry-breaking scalar/VEV sector and the
hypercharge/coupling lineage).

### Integration Points Already Updated

Phase400 (like Phase388-399) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `full-bosonic-action-flat-direction-lift-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/LOCAL-COMPLETION-V29-FERMIONIC-YUKAWA.md`
(Phase399/Phase400 coupled-stationarity closure section).
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase400_full_bosonic_action_flat_direction_lift_probe_001/Phase400FullBosonicActionFlatDirectionLiftProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase400 line, the Phase202 incomplete
status (`checklistPassedCount=193`, `checklistFailedCount=3`), and the same
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

1. The physical VO-6/VO-7 derivation against the Phase398 8-item gap
   ledger, headed by the symmetry-breaking scalar/VEV sector (welded to
   photon/Z mixing by Phase397) and the hypercharge/coupling lineage. Each
   component solved should arrive through a new fail-closed phase that
   names the Phase398 ledger row it discharges, proves target independence,
   and lets the existing gates decide promotion.
2. (Closed by Phase399 + Phase400 at the full toy-action control-branch
   level: the quadratic-model coupled critical point is solved and the
   full quartic action lifts all 18 flat directions, so the obstruction is
   relaxable at higher order.) One optional constructive completion
   remains: a Phase401 that actually SOLVES the coupled critical point in
   the full quartic action (extending the Phase399 fixed point with
   kernel-direction relaxation) and verifies the cube-root scaling the
   quartic lift predicts for the relaxed kernel amplitude,
   d_ker ~ (kappa |P_ker J| / quartic-scale)^(1/3). This would upgrade the
   component from "critical point exists" to "constructed and verified".
3. Periodic external literature monitoring at checkpoint cadence (the
   2026-06-10 survey and the 2026-06-11 post-Phase400 sweep found no
   GU-native scalar-sector source; the 2026-06-11 sweep catalogued
   arXiv:2503.14578, G2-RICCI-FLOW-TORSION-EWSB, which imports the 246 GeV
   scale and weak angle - the negative boundary stands; the internal
   toy-branch construction otherwise reached its honest limit).

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
rg -n "Phase400|fullBosonicActionFlatDirectionLiftProbe|kernelObstructionFullyRelaxable" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase400_full_bosonic_action_flat_direction_lift_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase400 output JSON files, and commit a checkpoint
after validation.

Suggested checkpoint message:

```text
Add phase400 flat-direction lift probe
```

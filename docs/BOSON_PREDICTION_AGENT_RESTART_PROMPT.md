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

Current gate status after the Phase397 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=190`,
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
  `parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed=True`,
  `su2NeutralSourceChannelEmpty=True`,
  `neutralMixingElementVanishesInFermionBilinearChannel=True`,
  `photonZSeparationUnderdetermined=True`,
  `hyperchargeAssignmentsDerived=False`,
  `canFillPhase256ObservedFieldExtractionContract=False`

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

The latest work (2026-06-10, after Phase397) was the external scalar-sector
source survey recorded in the journal: no GU-native scalar-sector or
hypercharge publication exists (the Cox Geometric Unity I paper remains the
only GU-native primary source, already fully audited by Phase386/387); the
new external preprint arXiv:2602.19151 (SO(3,3) BF gravity+electroweak,
2026) was catalogued in the ledger (SO33-BF-GRAVITY-ELECTROWEAK.md) and
found to IMPORT the Higgs mechanism and hypercharge rather than derive
them - externally corroborating the Phase397 closing diagnosis. Before
that, the latest phase was Phase397:

- Study:
  `studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001`
- Project:
  `Phase397ParametrizedU1ExtensionNeutralMixingUnderdeterminationProbe.csproj`
- Study note: `STUDY.md`
- Implementation note: `docs/Phases/Implementation/IMPLEMENTATION_P397.md`
- Outputs:
  `studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/output/parametrized_u1_extension_neutral_mixing_underdetermination_probe.json`
  and `..._summary.json`

Phase397 materialized per-edge u(1) variation blocks (charge q explicitly
underived; Hermiticity exact), the extended 4-dim carrier with
residual-U(1) charges {0, +-1, 0}, the extended 4x4 block Gram, and the
sector source channels:

- Z-like channel sourceless (fraction <= 0.0023); u(1) channel nonzero.
- Fermion-bilinear neutral mixing vanishes (ratio <= 4.5e-9).
- Photon/Z separation underdetermined; named gap {hypercharge,
  coupling ratio, scalar sector}.

### Integration Points Already Updated

Phase397 (like Phase388-396) is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `parametrized-u1-extension-neutral-mixing-underdetermination-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

Reference tracking was updated in `ExperimentReferences.md` and
`docs/Reference/ExperimentReferences/DRESSING-FIELD-ELECTROWEAK-OBSERVED-VARIABLES.md`.
The diagnosis journal entry is near the end of
`docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`.

### Validation Already Run

```bash
dotnet run --project studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/Phase397ParametrizedU1ExtensionNeutralMixingUnderdeterminationProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
./scripts/generate_validated_boson_predictions.sh
```

The full generator ended with the Phase397 line, the Phase202 incomplete
status (`checklistPassedCount=190`, `checklistFailedCount=3`), and the same
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

1. The scalar/symmetry-breaking sector: after Phase397, BOTH the photon/Z
   mixing (Phase256) and any W/Z mass-asymmetry mechanism on this skeleton
   must act through a scalar/VEV sector, which is exactly the missing
   Phase201 Higgs scalar source row. The honest route is the physically
   completed GU fermionic/scalar derivation (v29 obligations VO-6/VO-7) or
   new theorem-level sources for the GU scalar sector - not further toy
   construction.
2. A complete W/Z/H source package: separate W/Z source rows, Higgs scalar
   source row, weak-angle/coupling lineage, VEV/source scale, pole
   extraction, and GeV normalization.
3. Periodic external literature monitoring at checkpoint cadence (the
   2026-06-10 survey found no GU-native scalar-sector source and catalogued
   arXiv:2602.19151 as a non-promotable boundary source). The internal
   toy-branch construction has reached its honest limit (Phase397 journal
   entry records the closing diagnosis).

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
rg -n "Phase397|neutralMixingElementVanishes|photonZSeparationUnderdetermined" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add the ignored Phase397 output JSON files, and commit a checkpoint
after validation.

Suggested checkpoint message:

```text
Add phase397 u(1) extension underdetermination probe
```

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

Current gate status after the Phase388 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=181`,
  `checklistFailedCount=3`
- Claim integrity:
  `boson-claim-integrity-verified`,
  `sourceLineageMissing=true`, `wzMissingFieldCount=15`,
  `higgsMissingFieldCount=14`, `promotedPhysicalMassClaimCount=0`
- Phase388:
  `vo7ObservedNamespaceSourceTheoremProbePassed=True`,
  `candidateTheoremPresent=False`,
  `missingTheoremRequirementCount=13`,
  `canFillPhase201WzContract=False`

Interpretation: Phase388 is a fail-closed result. It confirms that the
v29/VO-7 shell-response branch remains the strongest repo-local diagnostic
lead, but it cannot be promoted to a W/Z/H source law from current artifacts.

### Most Recent Implemented Work

The latest work added Phase388:

- Study:
  `studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001`
- Project:
  `Phase388Vo7ObservedElectroweakNamespaceSourceTheoremProbe.csproj`
- Source:
  `Program.cs`
- Study note:
  `STUDY.md`
- Implementation note:
  `docs/Phases/Implementation/IMPLEMENTATION_P388.md`
- Full output:
  `studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001/output/vo7_observed_electroweak_namespace_source_theorem_probe.json`
- Summary output:
  `studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001/output/vo7_observed_electroweak_namespace_source_theorem_probe_summary.json`

Phase388 reads and cross-checks:

- Phase201 source-lineage intake contract
- Phase213 blocker matrix
- Phase256 observed-field extraction contract
- Phase307 target-independent decoupled W/Z row selection law
- Phase370 v29/VO-7 mixed-linearization audit
- Phase372 reciprocal discrete fermionic bilinear block
- Phase378 full connection-carrier shell-response Gram
- Phase379 response-image carrier-axis characterization
- Phase381 Phase302/307 response-image selector compatibility
- Phase382 observed projection requirement audit
- Phase383 suppressed-axis counterfactual selector audit
- Phase384 basis-energy proxy audit
- Phase385 observed electroweak namespace-map intake audit
- Phase387 Cox First Principles I full-text contract audit

Phase388 theorem requirements are all unmet:

- physical `M_psi`-compatible branch
- completed VO-7 mixed linearization
- physical effective-action Hessian
- carrier-axis-to-observed photon/W/Z/H map
- canonical gauge-axis or observed-namespace selector
- theorem explaining why the W row physically uses the Phase379-suppressed
  carrier axis
- Phase307 selector escape from the suppressed axis
- basis-energy proxy escape
- separate W/Z source rows
- Higgs scalar-source row
- weak-angle or coupling lineage
- VEV/source-scale lineage
- pole extraction and GeV normalization

### Integration Points Already Updated

Phase388 is wired into:

- `scripts/generate_validated_boson_predictions.sh`
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
- `scripts/verify_boson_claim_integrity.sh`
- Broad scanner exclusions:
  - `studies/phase204_boson_source_lineage_candidate_scan_001/Program.cs`
  - `studies/phase205_boson_source_lineage_text_evidence_scan_001/Program.cs`
  - `studies/phase207_higgs_quartic_self_coupling_source_scan_001/Program.cs`
  - `studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/Program.cs`
  - `studies/phase281_geometric_refractive_unification_source_audit_001/Program.cs`
  - `studies/phase295_observed_field_extraction_contract_candidate_scan_001/Program.cs`
  - `studies/phase296_source_lineage_contract_field_candidate_scan_001/Program.cs`

Reference tracking was updated in:

- `ExperimentReferences.md`
- `docs/Reference/ExperimentReferences/LOCAL-COMPLETION-V29-FERMIONIC-YUKAWA.md`
- `docs/Reference/ExperimentReferences/LOCAL-ARCH-P4-FERMION-MASS-REPRESENTATION.md`
- `docs/Reference/ExperimentReferences/DIRAC-SHELL-RESPONSE-BOUNDARY.md`

Diagnosis journal entry was added near the end of:

- `docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md`

### Validation Already Run

Targeted validation passed:

```bash
dotnet run --project studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001/Phase388Vo7ObservedElectroweakNamespaceSourceTheoremProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
```

Scanner/audit validation passed:

```bash
dotnet run --project studies/phase204_boson_source_lineage_candidate_scan_001/Phase204BosonSourceLineageCandidateScan.csproj
dotnet run --project studies/phase205_boson_source_lineage_text_evidence_scan_001/Phase205BosonSourceLineageTextEvidenceScan.csproj
dotnet run --project studies/phase207_higgs_quartic_self_coupling_source_scan_001/Phase207HiggsQuarticSelfCouplingSourceScan.csproj
dotnet run --project studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/Phase279TechnicolorWalkingElectroweakScaleSourceAudit.csproj
dotnet run --project studies/phase281_geometric_refractive_unification_source_audit_001/Phase281GeometricRefractiveUnificationSourceAudit.csproj
dotnet run --project studies/phase295_observed_field_extraction_contract_candidate_scan_001/Phase295ObservedFieldExtractionContractCandidateScan.csproj
dotnet run --project studies/phase296_source_lineage_contract_field_candidate_scan_001/Phase296SourceLineageContractFieldCandidateScan.csproj
```

Full generator validation also completed:

```bash
./scripts/generate_validated_boson_predictions.sh
```

It ended with:

```text
vo7-observed-electroweak-namespace-source-theorem-probe-failed-closed-new-theorem-required
vo7ObservedNamespaceSourceTheoremProbePassed=True
candidateTheoremPresent=False
missingTheoremRequirementCount=13
canFillPhase201WzContract=False
boson-objective-completion-audit-incomplete
objectiveAchieved=False
checklistPassedCount=181
checklistFailedCount=3
boson-claim-integrity-verified
sourceLineageMissing=true
wzMissingFieldCount=15
higgsMissingFieldCount=14
promotedPhysicalMassClaimCount=0
```

After the full run, generated timestamp-only JSON churn was stripped by
comparing tracked JSON files against `HEAD` while ignoring `generatedAt`.
One tracked generated output remained semantically changed:

- `studies/phase382_response_image_observed_projection_requirement_audit_001/output/response_image_observed_projection_requirement_audit.json`

That change is expected: the new docs increased the
`targetBlindConstructionHash` candidate line count from `541` to `552`.

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
contract fields.

The most useful next branches are:

1. A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem.
   This would have to show how the Phase378/379 carrier image maps to observed
   electroweak fields and must fill Phase256 observed-field extraction fields.
2. A theorem explaining why the physical W row must use the Phase379-suppressed
   carrier axis, despite Phase381/383/384 showing the current selected W route
   lives on that suppressed axis.
3. A physical `M_psi`-compatible GU fermionic branch with completed VO-7
   mixed-linearization blocks and gauge identities.
4. A physical effective-action Hessian or equivalent source operator replacing
   the study-defined shell-response Gram.
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
rg -n "Phase388|vo7Observed|candidateTheoremPresent|missingTheoremRequirementCount" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add ignored Phase388 output JSON files, and commit a checkpoint after
validation. The output directory under `studies/**/output/` is generally
ignored, so use `git add -f` for Phase388 output files if they are meant to be
committed.

Suggested checkpoint message:

```text
Add phase388 VO-7 theorem probe
```

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

Current gate status after the Phase389 work:

- Phase101:
  `internal-boson-prediction-package-built-physical-comparison-blocked`
- Phase202:
  `objectiveAchieved=False`, `checklistPassedCount=182`,
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
- Phase389:
  `vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed=True`,
  `discreteGaugeCompatibilityIdentityExact=True`,
  `discreteControlBranchGaugeCompatibilityIdentityCompletesVo7=False`,
  `canFillPhase201WzContract=False`

Interpretation: Phase389 materialized the first VO-7 gauge-compatibility
artifact. The discrete identity `[D(omega), X_hat] = delta_D[v(X)] + R(X)`
holds exactly on the identity-weight control branch, with
`v(X)_e = DeltaX_e + [omega_e, Xbar_e]` and the obstruction exactly
characterized by `S_e = {rho(omega_e), rho(DeltaX_e)}/2` (zero for global
parameters). This sharpens the VO-7 gap to physical blockers: there is still
no physical `M_psi`-compatible branch, no completed fermionic action, no
coupled physical mixed Hessian, and no observed electroweak namespace map.

### Most Recent Implemented Work

The latest work added Phase389:

- Study:
  `studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001`
- Project:
  `Phase389Vo7MixedLinearizationGaugeCompatibilityIdentityProbe.csproj`
- Source:
  `Program.cs`
- Study note:
  `STUDY.md`
- Implementation note:
  `docs/Phases/Implementation/IMPLEMENTATION_P389.md`
- Full output:
  `studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/output/vo7_mixed_linearization_gauge_compatibility_identity_probe.json`
- Summary output:
  `studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/output/vo7_mixed_linearization_gauge_compatibility_identity_probe_summary.json`

Phase389 verified, on both persisted Phase12 backgrounds:

- Exact linearity of the persisted Dirac assembly:
  `D(omega) = D_kin + delta_D[omega]` (reconstruction residual exactly 0 from
  the persisted `background_states/{bg}_omega.json` coefficients).
- The exact discrete gauge-compatibility identity over 168 vertex-local and
  global su(2) gauge directions (residual exactly 0).
- Exact global equivariance `[D, X_hat] = delta_D[[omega, X]]` with zero
  obstruction for constant gauge parameters.
- The contracted pure-gauge Ward identity at machine precision over 2016
  direction-mode rows.
- Boundary finding: the persisted Phase12 fermion modes are not tight
  eigenmodes of the persisted explicit base Dirac matrix (mode artifacts
  record `residualNorm ~ 12`), so Ward zero-current statements are not
  sharply testable on this branch (`wardEigenBoundSharp=False`).

### Integration Points Already Updated

Phase389 is wired into:

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `vo7-mixed-linearization-gauge-compatibility-identity-probe-materialized`)
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
dotnet run --project studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/Phase389Vo7MixedLinearizationGaugeCompatibilityIdentityProbe.csproj
dotnet run --project studies/phase101_boson_prediction_package_001/Phase101BosonPredictionPackage.csproj
dotnet run --project studies/phase202_boson_objective_completion_audit_001/Phase202BosonObjectiveCompletionAudit.csproj
./scripts/verify_boson_claim_integrity.sh
```

Scanner/audit validation passed (all still report zero intake-ready
evidence):

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

It ended with the Phase389 line, the Phase202 incomplete status
(`checklistPassedCount=182`, `checklistFailedCount=3`), and the same
claim-integrity status (`promotedPhysicalMassClaimCount=0`).

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

1. An `M_psi`-compatible Dirac rebuild with converged fermion modes on a
   non-degenerate mesh. Phase389 showed the persisted Phase12 modes have
   `residualNorm ~ 12`, so Ward zero-current statements and any
   spectral-shell physics on this branch are not sharply testable. This is
   the cheapest physical blocker to remove and is prerequisite to a coupled
   mixed Hessian.
2. A coupled boson-fermion second-variation (mixed Hessian) construction on
   that rebuilt branch, using the Phase389 identity as the
   gauge-compatibility template, replacing the study-defined shell-response
   Gram with an action-derived source operator.
3. A target-blind carrier-axis-to-observed photon/W/Z/H namespace theorem
   filling Phase256 observed-field extraction fields.
4. A theorem explaining why the physical W row must use the
   Phase379-suppressed carrier axis, despite Phase381/383/384 showing the
   current selected W route lives on that suppressed axis.
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
rg -n "Phase389|gaugeCompatibilityIdentity|discreteGaugeCompatibilityIdentityExact" \
  docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md \
  ExperimentReferences.md \
  studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001 \
  studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json
```

Then verify the gate if needed:

```bash
./scripts/verify_boson_claim_integrity.sh
```

### Commit Guidance

If this prompt file is present in an uncommitted worktree, inspect all diffs,
force-add ignored Phase389 output JSON files, and commit a checkpoint after
validation. The output directory under `studies/**/output/` is generally
ignored, so use `git add -f` for Phase389 output files if they are meant to be
committed.

Suggested checkpoint message:

```text
Add phase389 VO-7 gauge-compatibility identity probe
```

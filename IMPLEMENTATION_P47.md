# Phase XLVII - W/Z Physical Claim Falsifier Relevance Audit

## Goal

Phase XLVI cleared the W/Z physical target comparison:

- computed W/Z ratio: `0.8796910570948282`;
- target ratio: `0.88136`;
- pull: `1.0879885044906925`;
- target comparison passed: true.

The physical claim gate still blocked prediction language because it applies
all active fatal/high falsifiers globally. Phase XLVII audits whether those
remaining severe falsifiers directly target the W/Z physical claim.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WzPhysicalClaimFalsifierRelevanceAudit.cs`
- `tests/Gu.Phase5.Reporting.Tests/WzPhysicalClaimFalsifierRelevanceAuditTests.cs`
- `studies/phase47_wz_physical_claim_falsifier_relevance_001/physical_claim_falsifier_relevance_audit.json`
- `studies/phase47_wz_physical_claim_falsifier_relevance_001/STUDY.md`

The audit evaluates:

- active fatal/high falsifiers from the Phase XLVI falsifier summary;
- the Phase XLVI quantitative scorecard for `physical-w-z-mass-ratio`;
- the W/Z selector-variation diagnostic;
- validated physical W/Z mode records.

It classifies a severe falsifier as target-relevant only when its target id
matches the W/Z target observable, selected W/Z mode ids, selected W/Z source
candidate ids, or cannot be proven unrelated. Branch-fragility falsifiers are
classified as global sidecars only when the W/Z target comparison and selector
variation both pass. Representation-content falsifiers that target unrelated
fermion registry candidates are classified as global sidecars.

## Result

Generated:

- `studies/phase47_wz_physical_claim_falsifier_relevance_001/physical_claim_falsifier_relevance_audit.json`

Result summary:

- terminal status: `wz-physical-claim-target-clear-global-sidecars-blocked`;
- target comparison passed: true;
- selector variation passed: true;
- active severe falsifiers: 3;
- target-relevant severe falsifiers: 0;
- global-sidecar severe falsifiers: 3.

Classified global sidecars:

- `falsifier-0001`: high branch-fragility target `gauge-violation`;
- `falsifier-0002`: high branch-fragility target `solver-iterations`;
- `falsifier-0003`: fatal representation-content target `fermion-registry-phase4-toy-v1-0000`.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed: 194/194.
- `jq -e . studies/phase47_wz_physical_claim_falsifier_relevance_001/physical_claim_falsifier_relevance_audit.json`
  passed.

## Next Step

Phase XLVII establishes that the remaining Phase XLVI severe falsifiers do not
directly target the W/Z physical ratio claim. The next phase should update the
physical claim gate policy to support a documented target-scoped mode:

- unrestricted campaign-level physical prediction remains blocked while global
  sidecars are active;
- W/Z target-scoped physical comparison language is allowed only when there are
  zero target-relevant severe falsifiers, the quantitative target comparison
  passes, and the selector-variation diagnostic passes;
- reports must display the remaining global sidecars next to any target-scoped
  physical claim status.

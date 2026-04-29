# Phase XLV - Selector-Eigen Operator Term Audit

## Goal

Phase XLIV proved the physical W/Z comparison path is now wired, but the
Phase XLIII selector-eigen source spectra still miss the physical W/Z target by
about `2.1088818212155%`. Phase XLV audits whether the emitted selector-eigen
spectra contain any target-independent electroweak, mixing, normalization, or
nontrivial mass-operator term evidence that could legitimately carry that
remaining ratio shift.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/WzSelectorEigenOperatorTermAudit.cs`
- CLI command `audit-wz-selector-eigen-operator-terms`
- `tests/Gu.Phase5.Reporting.Tests/WzSelectorEigenOperatorTermAuditTests.cs`

The diagnostic consumes:

- the Phase XLIV W/Z ratio failure diagnostic;
- the Phase XLIV selector variation diagnostic;
- the Phase XLIII selector-eigen spectrum root.

It reports separately:

- solver-backed spectrum count;
- observed operator types;
- observed solver methods;
- observed emitted mode energy blocks;
- nontrivial operator-term evidence count;
- closure requirements for physical W/Z prediction unblocking.

## Audit Result

Command:

```bash
dotnet run --project apps/Gu.Cli -- audit-wz-selector-eigen-operator-terms \
  --ratio-diagnostic studies/phase44_selector_eigen_wz_physical_prediction_001/wz_ratio_failure_diagnostic.json \
  --selector-variation studies/phase44_selector_eigen_wz_physical_prediction_001/wz_selector_variation_diagnostic.json \
  --spectra-root studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/spectra \
  --out studies/phase45_selector_eigen_operator_term_audit_001/selector_eigen_operator_term_audit.json
```

Result:

- terminal status: `wz-selector-eigen-operator-term-blocked`;
- selected pair: `phase22-phase12-candidate-0/phase22-phase12-candidate-2`;
- required scale to target: `1.021088818212155`;
- required ratio shift fraction: `0.021088818212154914`;
- selector ratio envelope: `0.8617415263399019` to `0.8654919922785442`;
- inspected selected W/Z spectra: 72;
- solver-backed spectra: 72;
- nontrivial operator-term evidence count: 0;
- observed operator type: `FullHessian`;
- observed solver method: `explicit-dense`;
- observed emitted mode block: `connection`.

Closure requirements:

- no target-independent electroweak, mixing, or nontrivial mass-operator term
  evidence was emitted for the selected W/Z spectra;
- selected W/Z mode energy is entirely in the connection block, so no
  electroweak or mixing block participates in the emitted modes.

## Interpretation

Phase XLV confirms that Phase XLIII did solve selector-specific eigenproblems,
but those eigenproblems are still too structurally simple for physical W/Z
prediction. The current operator path is solver-backed, not proxy-only, yet the
emitted spectra show only a connection-block FullHessian solve with no
target-independent electroweak/mixing/mass-term derivation.

This is why physical boson predictions remain blocked after Phase XLIV: the
pipeline can now compare to real values, but the source operator has not yet
included the internal structure needed to explain or falsify the remaining
`2.1088818212155%` W/Z ratio shift.

## Validation

Completed:

- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  passed: 191/191.
- `jq -e . studies/phase45_selector_eigen_operator_term_audit_001/selector_eigen_operator_term_audit.json`
  passed.

The audit command exits nonzero for the generated artifact because the terminal
status is intentionally blocked.

## Next Step

Implement a target-independent selector-cell operator upgrade that can emit
nontrivial electroweak or mixing block participation into the selected W/Z
spectra. The upgrade must be derived from existing internal charge-sector,
identity-feature, coupling, or operator artifacts; it must not tune coefficients
from the external W/Z target.

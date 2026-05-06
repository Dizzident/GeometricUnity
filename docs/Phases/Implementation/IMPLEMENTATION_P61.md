# Phase LXI - Normalized Weak-Coupling Input Audit

## Goal

Start the normalized weak-coupling bridge-source lane by making admissible weak-coupling inputs explicit.

## Implementation

Added:

- `src/Gu.Phase5.Reporting/NormalizedWeakCouplingInputAuditor.cs`
- `tests/Gu.Phase5.Reporting.Tests/NormalizedWeakCouplingInputAuditorTests.cs`
- `studies/phase61_normalized_weak_coupling_input_audit_001/weak_coupling_input_audit.json`
- `studies/phase61_normalized_weak_coupling_input_audit_001/STUDY.md`

The audit accepts a weak-coupling candidate only when it provides:

- source kind `normalized-internal-weak-coupling`;
- a physical weak-coupling normalization convention;
- finite positive coupling value and finite non-negative uncertainty;
- non-proxy derivation method;
- branch stability above the configured threshold;
- explicit exclusion of the W/Z mass target observables used downstream for validation.

## Finding

The current Phase12-style coupling records remain blocked as normalized weak-coupling inputs. They are finite-difference coupling proxies under unit-mode normalization, carry no physical weak-coupling uncertainty, and have zero branch stability in the inspected records.

## Next Step

Phase LXII should generate a real `normalized-internal-weak-coupling` candidate from internal operator artifacts instead of finite-difference coupling proxies, then pass it through this audit.

## Validation

Completed:

- `jq -e . studies/phase61_normalized_weak_coupling_input_audit_001/weak_coupling_input_audit.json`
- `dotnet test tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj`
  - Passed: 243, Failed: 0, Skipped: 0
- `git diff --check`

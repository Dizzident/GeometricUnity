# Theory Conformance Report: {{branchId}}

**Conformance ID:** {{conformanceId}}
**Branch:** {{branchId}}
**Evaluated At:** {{evaluatedAt}}
**Overall Pass:** {{overallPass}}

---

## Scope Declaration

**Validation Scope:** {{validationScope}}

> {{scopeDisclaimer}}

This report documents branch-local conformance only. A passing result means the runtime
execution was internally consistent with its declared branch. It does **not** mean:
- The branch is the canonical realization of Geometric Unity
- The lowering choices are uniquely determined by the original draft
- The outputs match physical reality

See `ASSUMPTIONS.md` for the full list of branch-local assumptions in effect.

---

## Branch Identity Checks

These checks confirm that the runtime operators and artifact provenance match the declared
branch manifest. A failure here means a silent mismatch occurred — the artifact would be
mislabeled if emitted.

| Check ID | Passed | Detail |
|----------|--------|--------|
{{#branchIdentityChecks}}
| {{checkId}} | {{passed}} | {{detail}} |
{{/branchIdentityChecks}}

---

## Trivial State Flags

These checks are **informational only** — they do not cause overall failure. They flag
when the runtime execution is on the simplest possible branch (zero omega, trivial
torsion, identity Shiab). Runs on these branches produce valid branch-local results but
are the weakest possible mathematical validation tests (see A-009, A-011).

| Check ID | Flag | Detail |
|----------|------|--------|
{{#trivialStateChecks}}
| {{checkId}} | {{trivialFlag}} | {{detail}} |
{{/trivialStateChecks}}

---

## Scope Boundary Checks

These checks confirm that the manifest properly declares its inserted assumptions,
establishing the branch-local scope boundary.

| Check ID | Passed | Detail |
|----------|--------|--------|
{{#scopeBoundaryChecks}}
| {{checkId}} | {{passed}} | {{detail}} |
{{/scopeBoundaryChecks}}

---

## Summary

| Category | Total | Passed | Failed |
|----------|-------|--------|--------|
| branch-identity | {{branchIdentityTotal}} | {{branchIdentityPassed}} | {{branchIdentityFailed}} |
| trivial-state | {{trivialStateTotal}} | {{trivialStateTotal}} | 0 (informational) |
| scope-boundary | {{scopeBoundaryTotal}} | {{scopeBoundaryPassed}} | {{scopeBoundaryFailed}} |

---

_Generated from Gu.TheoryConformance. All results are branch-local unless explicitly noted otherwise._

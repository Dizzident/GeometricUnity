# Canonicity Docket: {{objectClass}}

**Status:** {{status}}
**Active Representative:** {{activeRepresentative}}
**Equivalence Relation:** {{equivalenceRelationId}}
**Admissible Comparison Class:** {{admissibleComparisonClass}}

---

## Current Evidence

| EvidenceId | StudyId | Verdict | MaxObservedDeviation | Tolerance | Timestamp |
|------------|---------|---------|----------------------|-----------|-----------|
{{#currentEvidence}}
| {{evidenceId}} | {{studyId}} | {{verdict}} | {{maxObservedDeviation}} | {{tolerance}} | {{timestamp}} |
{{/currentEvidence}}

---

## Known Counterexamples

{{#knownCounterexamples}}
- {{.}}
{{/knownCounterexamples}}
{{^knownCounterexamples}}
_No counterexamples recorded._
{{/knownCounterexamples}}

---

## Pending Theorems

{{#pendingTheorems}}
- {{.}}
{{/pendingTheorems}}
{{^pendingTheorems}}
_No pending theorems._
{{/pendingTheorems}}

---

## Downstream Claims Blocked Until Closure

{{#downstreamClaimsBlockedUntilClosure}}
- {{.}}
{{/downstreamClaimsBlockedUntilClosure}}
{{^downstreamClaimsBlockedUntilClosure}}
_No downstream claims blocked._
{{/downstreamClaimsBlockedUntilClosure}}

---

## Study Reports

{{#studyReports}}
- {{.}}
{{/studyReports}}

---

_Generated from CanonicityDocket. This docket closes only by actual uniqueness theorem, classification theorem, or explicit invariance evidence._

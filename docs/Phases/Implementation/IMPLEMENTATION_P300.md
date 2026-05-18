# Implementation P300: Identity-Split Common Normalization Audit

Phase300 audits the last obvious normalization loophole opened by Phase299. Phase299 materialized separate source-backed analytic replay rows for the Phase27 identity-selected W and Z candidates, but both rows failed raw promotion. Phase300 asks whether a single target-independent common normalization factor could repair both identity rows at once.

Inputs:

- Phase299 identity-split source-backed replay summary.
- Phase120 finite/analytic common-scale diagnostic.
- Phase221 SU(2) Casimir/RMS normalization lead.
- Phase213 source-lineage blocker matrix.
- Phase12 spinor metadata and the Phase299 source mode vectors referenced by variation evidence ids.

Result:

- `terminalStatus=identity-split-common-normalization-audit-common-scale-blocked`
- `identitySplitCommonNormalizationAuditPassed=true`
- W required target-implied scale: `409.66232282442104`
- Z required target-implied scale: `157.61447685448672`
- Required-scale relative spread: `0.8886238468155209`
- Source-declared common scale candidate pass count: `0`
- The source perturbation-vector length scale `156` is a Z-only near miss: it lifts the Z mean row near the raw gate but leaves the W mean row far below it.

Decision:

Do not repair the Phase27 identity split by applying a common normalization factor. The W and Z production replay rows require incompatible target-implied scales, and audited target-independent scale candidates do not satisfy both raw and common-bridge gates.

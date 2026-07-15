# O4 conventions overlay register tooling

Non-phase tooling (plan item 6, `docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`).

`generate.js` recursively discovers every `physicistReviewPending` property in
`studies/*/output/*_summary.json` and emits
`docs/Phases/Adjudication/O4_CONVENTIONS_REGISTER.md`, grouping every
review-pending phase output by the 13 mandatory O4 ruling IDs. Exact artifact
paths, pending-property pointers, dispositions, and typed evidence predicates
are committed in `coverage_contract.json`; no phase-name or prose keyword
classification is permitted.

```
node scripts/o4_register/generate.js          # (re)generate the register
node scripts/o4_register/generate.js --check   # assert the register is current
node scripts/o4_register/coverage_test.js      # adversarial + live coverage tests
```

The currency check is also a mandatory precondition of
`scripts/verify_boson_claim_integrity.sh`; a stale overlay now fails the main
claim-integrity gate before any claim assertions run.

Strict invariants:

- **Read-only** over committed artifacts — the committed phase JSONs stay
  byte-identical; this tool only reads them.
- Coverage fails closed on malformed JSON, an unmapped pending artifact,
  duplicate phase IDs, unknown or contradictory pending pointers, or typed
  predicate drift.
- The coverage review-item ID set must exactly equal the memo schema's 13
  mandatory ruling IDs.

This directory is registered in the scripts-root scanner exclusion helpers per
the CLAUDE.md non-phase-tooling rule (phases 278 / 279 / 281 / 289 / 295 / 296).

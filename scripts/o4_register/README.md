# O4 conventions overlay register tooling

Non-phase tooling (plan item 6, `docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`).

`generate.js` scans `studies/*/output/*_summary.json` for the
`physicistReviewPending` field and emits
`docs/Phases/Adjudication/O4_CONVENTIONS_REGISTER.md`, grouping every
review-pending phase output by convention family (invariant rays, positive-mode
IR rule, theta-Haar, saddle backgrounds, plus the phase447 soft-floor and
phase453 uniform-ladder) so the O4 blast radius is machine-enumerable before the
physicist session.

```
node scripts/o4_register/generate.js          # (re)generate the register
node scripts/o4_register/generate.js --check   # assert the register is current
```

Strict invariants:

- **Read-only** over committed artifacts — the committed phase JSONs stay
  byte-identical; this tool only reads them.
- Family classification is a **DERIVED** heuristic overlay; it mutates nothing
  and the committed outputs remain authoritative.

This directory is registered in the scripts-root scanner exclusion helpers per
the CLAUDE.md non-phase-tooling rule (phases 278 / 279 / 281 / 289 / 295 / 296).

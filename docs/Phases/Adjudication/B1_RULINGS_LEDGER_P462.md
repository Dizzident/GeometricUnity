# B1 Rulings Ledger (phase462)

Empty rulings ledger created in the STEP 0 wave wiring checkpoint
(`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, §Step-0 item 2). It records the
Stage-2 physicist adjudication rulings for the phase462 blocking-set resolution.

**Privacy rule (binding):** this ledger stores **identifiers and hashes only —
never quoted corpus text.** Each ruling references a pinned statement by its
`pinnedQuoteSha256` (computed by phase462 Stage P over the pinned preimage); the
quote text itself lives only in the pinned-preimage record inside the phase462
tooling, never here. This keeps the ledger scanner-neutral and prevents any
corpus prose from being duplicated into a walked doc.

## Schema (one row per ruling)

| Field | Meaning |
|-------|---------|
| `id` | ruling id (stable, e.g. `R-001`) |
| `pinnedQuoteSha256` | sha256 of the Stage-P normalized pinned passage (no text) |
| `ruling` | the adjudication outcome for that statement |
| `signer` | ruling signer (primary or second signer) |
| `sessionIndex` | adjudication session index |
| `date` | ISO-8601 date of the ruling |

## Rulings

_(none — no adjudication session has been held; phase462 is a STEP 0 skeleton
emitting `awaiting-adjudication` with k = 31 pending.)_

| id | pinnedQuoteSha256 | ruling | signer | sessionIndex | date |
|----|-------------------|--------|--------|--------------|------|

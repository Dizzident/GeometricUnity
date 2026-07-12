# Phase462: Blocking Set Resolution (STEP 0 skeleton)

Team A, plan item 2 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`). STEP 0
**skeleton**: emits the pre-registered interim terminal `awaiting-adjudication`
(k = 31 pending) and the standing claim boundary. The staged resolution is not
yet implemented. This file pre-registers the full staged design.

## Interim terminal

`awaiting-adjudication` with **k = 31** prose-only statements pending under the
committed 33-symbol grading. Skeleton only; zero physics compute; nothing graded
or promoted; `physicistReviewPending = true`.

## Staged design (pre-registered; to be implemented)

- **Stage P — quote-pinning.** PA mirror tier (whitespace-normalized exact match
  against the committed mirror snapshot, byte-offset + sha256(normalized
  passage) + drift gate; normalization is load-bearing). PB PDF tier (re-fetch
  the official April-2021 draft PDF: URL + retrieval date + document sha256 +
  excerpt + page locator + drift gate). Non-refetchable items commit
  UNPINNABLE. sha12 = sha256(auditKeyName)[:12] with unique-preimage
  verification over the 331–345 audit JSON key sets.
- **Stage 0 — exact closure-sensitivity certificates** (Smith normal form over
  Z).
- **Stage 1 — conjunction-gated machine excision** (ex-ante yield ≤ 1, likely
  0).
- **Stage 2 — physicist adjudication:** **≥ 40 paraphrase decoys**, a **sealed
  redo**, and a named **second signer**.

## Committed rules

- **AUDIT-AUTHORED-NOT-CORPUS.** Audit-authored "finding" strings are labelled
  AUDIT-AUTHORED-NOT-CORPUS and are **never gradable**.
- **Discard: extractor-NLP.** An automated extractor/NLP route is deliberately
  discarded — the grading is over pinned corpus preimages, not machine-extracted
  paraphrase; an NLP extractor would introduce an ungradable authorship layer.

## Terminals (pre-registered)

`awaiting-adjudication(k)` (green interim), `pinning-insufficient` (> 7/31
UNPINNABLE), `closure-resolved(-with-excision)(k)`,
`corpus-supplies-scale-breaking-relation(namedItem)`, `still-blocked(k)`, plus
the fail-closed guards `input-hash-mismatch` / `symbol-outside-33-table` /
`snf-nonconvergent` / `mirror-or-pdf-drift` / `decoy-battery-failed(sessionIndex)`.

## Scanner note

The real phase's output JSONs will embed verbatim draft excerpts (registered in
the scanners in this checkpoint); the skeleton embeds none.

## Framing

Zero physics computation at STEP 0. Nothing promoted;
`promotedPhysicalMassClaimCount = 0`.

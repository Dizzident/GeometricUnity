# Implementation P462 - Blocking Set Resolution (Team A, Wave-2)

Phase462 executes Team A plan item 2 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`,
§2(A)): resolution of the 31-item corpus-dimensionally-ambiguous blocking set
decided by the committed phase460 units-equivariance kernel theorem. Registry
number 462 per `docs/Phases/PHASE_NUMBER_REGISTRY.md`. Deterministic,
target-blind, fail-closed; runs in well under a second.

## What this phase computes

This session implements **Stage P** (quote-pinning), **Stage 0**
(closure-sensitivity certificates + exact SNF kernel-monotonicity) and
**Stage 1** (conjunction-gated machine excision). **Stage 2** (the physicist
adjudication session) does NOT run here.

- **Stage P.** The 1 mirror-resident item pins at PA tier (whitespace-normalized
  exact match; normalized byte-offset + sha256(passage) + mirror sha256 drift
  gate). The 30 audit-key items link by `sha12` UNIQUE-preimage to
  AUDIT-AUTHORED-NOT-CORPUS finding strings (all 30 unique); those strings are
  never gradable. A PB (draft-PDF) pin needs a committed verbatim excerpt + page
  locator - none is committed and the automated extractor/NLP route is
  deliberately discarded - so all 30 commit **UNPINNABLE** (no manufactured
  pins). The official April-2021 draft drift gate is pinned; an opt-in live
  refetch (`PHASE462_PDF_REFETCH=1`) verifies it, and the deterministic verdict
  never depends on network.
- **Stage 0.** All 31 default CLOSURE-SENSITIVE; CLOSURE-IRRELEVANT is granted
  only by the exact zero-mass-dimension certificate over the committed reading
  menu. Kernel-monotonicity (BigInteger SNF ported from phase460) is proven
  exactly, and a synthetic-overturn battery flips the squared reading and shrinks
  the kernel by one.
- **Stage 1.** `excision = CLOSURE-IRRELEVANT AND syntactic-non-relation AND
  pinned-corpus-text`; ex-ante yield <= 1, asserted.
- **Decoy battery + sealed-redo escrow.** A deterministic engine emits >= 40
  paraphrase decoys (committed seed, no RNG); a second set's plaintext is
  escrowed outside the scanned roots, only its seal sha256 committed.

Fail-closed guards: `input-hash-mismatch`, `symbol-outside-33-table`,
`snf-nonconvergent`, `mirror-or-pdf-drift`, `decoy-battery-failed`.

## Committed verdict

`blocking-set-resolution-pinning-insufficient` with
`blockingSetResolutionPassed = true` (internal consistency all green). PA=1,
PB=0, UNPINNABLE=30 (> 7/31 tripwire); Stage 0 = 31 CLOSURE-SENSITIVE; Stage 1
excision yield 0 (<= 1); pending after Stage 1 = 31; decoys = 48.

This honestly proves the machine resolution route dead - the 30 audit-key
statements cannot be pinned to gradable corpus text without human Stage-2
locator work - and pins phase464 to BLOCKED-UPSTREAM-AMBIGUOUS with the
pre-committed sentence. The output JSONs embed the audit-authored preimage
strings and the one PA-pinned mirror passage (registered in the scanners in the
wave checkpoint); no other corpus text is stored.

## Framing

No GeV content; nothing measured, filled, or promoted;
`promotedPhysicalMassClaimCount = 0`; `physicistReviewPending = true`. Lattice-unit
quantities stay in lattice units.

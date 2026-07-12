# Phase462: Blocking Set Resolution (Stage P / 0 / 1)

Team A, plan item 2 (`docs/Phases/WAVE2_ACTION_PLAN_2026-07-12.md`, §2(A)).
Deterministic, target-blind, fail-closed. This session implements **Stage P**
(quote-pinning), **Stage 0** (closure-sensitivity certificates, exact SNF
kernel-monotonicity) and **Stage 1** (conjunction-gated machine excision).
**Stage 2** (the physicist adjudication session) does NOT run here; the phase
lands at the pre-registered terminal determined by the actual pinning coverage.

The blocking set is the **31** corpus-dimensionally-ambiguous items decided by
the committed phase460 units-equivariance kernel theorem: **1** mirror-resident
item (PA tier) and **30** audit-key items (PB tier).

## Stage P - quote-pinning

- **PA (mirror) tier - 1 item.** Whitespace-normalized exact match against the
  committed mirror snapshot; commit normalized byte-offset + sha256(normalized
  passage) + mirror sha256 drift gate. Result: **PINNED** (the one gradable
  corpus row).
- **PB (draft PDF) tier - 30 items.** `sha12 = sha256(auditKeyName)[:12]` with
  **UNIQUE-preimage** verification over the referenced literature-audit output's
  true-key set (all 30 unique). The preimage is an **AUDIT-AUTHORED-NOT-CORPUS**
  finding string - **never gradable**. A PB pin needs a committed verbatim draft
  excerpt + page locator; none is committed and the automated extractor/NLP
  auto-locate route is **deliberately discarded** (it would introduce an
  ungradable authorship layer). The official April-2021 draft drift gate is
  pinned (document sha256). Every PB item therefore commits **UNPINNABLE** -
  no manufactured pins.

Pinned corpus text lives ONLY in the phase output JSON (PA row only).

## Stage 0 - closure-sensitivity certificates

Default all 31 **CLOSURE-SENSITIVE**. CLOSURE-IRRELEVANT is granted ONLY by the
exact certificate: a committed non-empty symbol support all of whose symbols
have mass-dimension 0 under EVERY grading in the committed reading menu
(cc in {-2,-1,-4}). A prose-only item (empty committed support) can never earn
it. Kernel-monotonicity is proven exactly in-phase (BigInteger SNF ported from
phase460): rulings only ADD constraint rows, so the integer kernel can only
shrink; an unadjudicated ambiguous item can never help the closure verdict. The
synthetic-overturn battery adds a synthetic RELATION row that flips the squared
reading from closes to breaking and shrinks the kernel by exactly one. Injection
battery (consistency-check): a grade-0-only support certifies irrelevant, a
mass-bearing support stays sensitive.

## Stage 1 - conjunction-gated machine excision

`excision = CLOSURE-IRRELEVANT (Stage 0) AND syntactic-non-relation AND
pinned-corpus-text`. Committed ex-ante yield **<= 1** (likely 0); the actual
yield is asserted against that bound.

## Decoy battery + sealed-redo escrow

A deterministic template engine (committed seed, no RNG) emits **>= 40**
paraphrase-level decoy renderings of the pinned-row relation statements; the
decoy sha256 list is committed and the plaintext lives only in the output JSON.
A second decoy set's plaintext is escrowed OUTSIDE the scanned roots
(scratchpad); only its seal sha256 is committed. `decoy-battery-failed` on
< 40 unique or any collision with a real statement.

## Terminals (pre-registered)

`awaiting-adjudication(k)`, `pinning-insufficient` (> 7/31 UNPINNABLE),
`closure-resolved(-with-excision)(k)`,
`corpus-supplies-scale-breaking-relation(namedItem)`, `still-blocked(k)`, plus
the fail-closed guards `input-hash-mismatch` / `symbol-outside-33-table` /
`snf-nonconvergent` / `mirror-or-pdf-drift` / `decoy-battery-failed`.

## Committed result

`blocking-set-resolution-pinning-insufficient` (`blockingSetResolutionPassed = true`;
internal consistency all green). PA-pinned **1**, PB-pinned **0**, UNPINNABLE
**30** (> 7 tripwire). Stage 0: **31** CLOSURE-SENSITIVE (kernel-monotonicity and
synthetic-overturn proven exactly). Stage 1 excision yield **0** (<= 1 ex-ante
bound); pending after Stage 1 = 31. Decoys: 48 (>= 40).

This honestly proves the machine resolution route dead: the 30 audit-key
statements cannot be pinned to gradable corpus text without human Stage-2
locator work (the NLP auto-locate route is discarded). It pins phase464 to
**BLOCKED-UPSTREAM-AMBIGUOUS** with the pre-committed sentence. Nothing is
promoted; `promotedPhysicalMassClaimCount = 0`; `physicistReviewPending = true`.

## Env knobs

`PHASE462_OUTPUT_DIR`, `PHASE462_ESCROW_DIR`, and `PHASE462_PDF_REFETCH=1` (opt-in
live draft-PDF drift verification; the deterministic verdict never depends on
network - a fetched-but-mismatched PDF is a hard drift failure, a network failure
is an honest UNPINNABLE reason).

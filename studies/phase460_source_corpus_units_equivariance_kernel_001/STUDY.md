# Phase460 - Source-Corpus Units-Equivariance Kernel (Team A Wave-1, A1)

Team A rank-2 of the 2026-07-10 three-team elimination program
(`docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md`, Wave-1 item 2).
ELIMINATION computation: exact integer arithmetic, deterministic,
target-blind, sub-second.

## Question

Is the audited source corpus units-equivariant - i.e., does the
one-parameter scale-transformation family lie in the full integer kernel of
the grading constraints imposed by every pinned relation the committed
literature-audit outputs (phase330-345, referred to strictly by phase
number) and the committed reference snapshot record?

## Method

1. Every pinned relation is extracted into an integer grading-constraint row
   (exponent differences over 33 symbol grades). Prescribed reading:
   metric +2, connection -1, curvature -2, mass -1; canonical extensions for
   scalars/VEVs (-1), potential density (-4), radii (+1), dimensionless
   couplings (0).
2. The FULL integer kernel is computed per admissible reading via a Smith
   normal form written in-phase over BigInteger (exact; no floating point in
   any decision), unit-tested by an in-phase battery (divisibility chain,
   brute-force determinantal-divisor cross-check, kernel annihilation).
3. The kernel is intersected with the scale family; per-reading verdicts on
   the pre-registered taxonomy {scaling-symmetry-closes,
   equivariance-breaking-relation-found(ids),
   corpus-dimensionally-ambiguous(ids)}.
4. Admissible reading family (pre-registered): literal vs squared-repair
   variants of the snapshot's curvature-mass statement, crossed with cc-term
   re-grades {-2 curvature-class (prescribed), -1 mass-class, -4
   density-class}.

Binding pre-registration: the snapshot's "cosmological constant as a VEV"
slot statement is the EXPECTED breaking hit, decided by the machine
criterion (equation glyph + numeric coefficient => pinned row;
coefficient-free slot => routed to the phase461 anchor menu, never silently
a kernel row). The extractor fail-closes unless it breaks or routes.
Prose-only relations enter a corpus-dimensionally-ambiguous BLOCKING set
(31 items) and are never dropped.

Coverage gate: each committed audit output is pinned by its true-key count
and sorted-true-key-list SHA-256 (freshness), and every equation-indicator
key (generic regex net) must be inventory-mapped; keys are referenced by
hash, not by name.

## Committed result (smoke-verified)

- 34 pinned rows, 33 symbols; kernel rank 24 / nullity 9 on both matrix
  variants; every kernel basis vector annihilated exactly.
- R0 (prescribed, literal): BREAKING - the single breaking row is the
  snapshot's literal first-power curvature-mass statement (residual +1),
  reproducing the committed phase420 dimensional-invalidity finding.
- R1 (prescribed, squared repair): all pinned rows close and the scale
  family lies in the kernel, but the 31-item blocking set forbids the
  closure verdict (corpus-dimensionally-ambiguous).
- R2/R3/R4 (cc re-graded): the corpus's own pinned cc-bearing relations
  (p335-r2, p339-r1) BREAK - the anchor re-grading demanded by the
  CC-as-a-VEV reading is units-inconsistent with the audited corpus.
- The CC-as-a-VEV statement is coefficient-free under the machine criterion
  and ROUTES to phase461 (gate green).

Terminal:
`source-corpus-units-equivariance-kernel-breaking-relation-found-under-prescribed-reading`.

## Run

```
dotnet run -c Release --project \
  studies/phase460_source_corpus_units_equivariance_kernel_001/Phase460SourceCorpusUnitsEquivarianceKernel.csproj
```

Outputs: `output/source_corpus_units_equivariance_kernel.json` (+ summary).

## Boundaries

Elimination content only. `physicistReviewPending = true` (grading
conventions are workbench conventions). No GeV content; no contract filled;
nothing promoted; `promotedPhysicalMassClaimCount = 0` unchanged. Feeds the
A6 conjunction adjudication (first conjunct) together with phase461 (A2).

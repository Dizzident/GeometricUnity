# Phase 575 implementation: registered fold-convention re-adjudication

## Status

Phase575 v1 executed under Amendment A39 and returned
`evidence-independently-confirmed-under-registered-fold-convention` on its
first frozen execution. It is a deterministic, zero-replay, zero-RNG
adjudicator over committed bytes only.

## Method

The contract exact-binds 25 artifacts across the executed
Phase570/571/572/573/574 chains and the Phase548 lineage. The row
convention is frozen before any audited read and its evaluator is shared
verbatim between the known-answer battery and production: per table row,
bulk and tail ESS must be bit-identical across sides; pass/fail
classifications must agree; any R-hat difference must lie in Phase574's
measured five-row fold-flip set; and on differing rows the smaller distance
to the frozen 1.01 threshold must exceed ten times the scaled cross-side
difference. The battery exercises healthy, fold-flip, threshold-straddle,
unexplained-difference, tight-margin, and ESS-mismatch fixtures plus the
terminal truth table, and was deterministically rehearsed outside the
repository before the freeze.

Side consistency recomputes all 36 rows per side from the Phase574-retained
raw traces through the shared kernel (justified by Phase573's parity proof)
and requires bit-for-bit agreement with the committed Phase570 and Phase572
diagnostics. Confirmation additionally requires Phase572's committed
replay and 96-row transition adjudications to have agreed in full.

## Result

All checks passed: 72/72 committed diagnostic rows reproduced bit-for-bit,
exactly five differing rows, every difference explained and margin-safe
(minimum margin ratio above thirteen times the frozen factor-ten rule), and
the committed replay/transition confirmations present. The favorable
terminal opens `prospectiveChainPackPlanningGateOpen`, which authorizes
only the registration and prospective freezing of a separate disjoint-seed
chain-pack design phase. Phase571's scope is unchanged, Phase572's terminal
and tolerance stand, and no sampling, execution, production, or launch
authority follows.

## Hashes

- Program.cs:
  `39e701ad21b411ccc34dc87f883df0534fada16e216b10763af88f4682205a90`
- csproj:
  `de3a625c1165cfd59a28e857eada74b547cc8e17f617a3f2e87d7d9f0b754819`
- contract v1:
  `800b5baa128798d52ab9a98f6d0a065417f4c05f00c612bb0a0b4dd098105397`
- v1 full and summary outputs:
  `21e94c9e292804c4b810785747d10c345b30b0b4578b5f45b04ac1c80a20ff10`

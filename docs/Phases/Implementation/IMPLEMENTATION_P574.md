# Phase 574 implementation: directional raw-trace fold-order localization

## Status

Phase574 v1 executed under Amendment A38 and returned
`fold-order-flip-confirmed-source-attributed` on its first frozen execution.
It is a deterministic, retrospective audit that replays the six committed
Phase548 chains once and recovers the raw directional series that Phase570
and Phase572 computed but did not retain.

## Method

The contract exact-binds 36 artifacts: the Phase548 contract, program,
summary, six telemetry files, and six checkpoints; the executed Phase570 v7,
Phase572 v9, and Phase573 v2 contract/program/project/output chains; and the
six registered platform sources. The known-answer battery runs before any
audited numeric row is read and was deterministically rehearsed outside the
repository before the contract freeze. Its fold-flip fixture plants an
ulp-scale value change at a designed fold tie and requires ordinary ranks,
bulk ESS, tail ESS, and the ranked R-hat component to stay bit-identical
while the folded component moves; its stability fixture requires a one-ulp
change far from any fold tie to leave every output bit-identical; its
eigensolver-pair fixture bounds the two ported Jacobi variants at 1e-12
scaled agreement while recording their bit differences.

One replay produces the shared position stream (all six chains verified
against telemetry decisions and bit-identical final checkpoints). Each
retained draw is measured twice: through the Phase570 v7 scalar edge-space
basis and 40-sweep descending unclamped Jacobi, and through the Phase572
full-dof sign-canonicalized directions and 32-sweep ascending zero-clamped
Jacobi, both ported verbatim. A single estimator kernel - justified by
Phase573's bit-for-bit kernel-parity proof - reproduces each side's
committed diagnostics.

## Result

All four H-fold clauses held and the terminal taxonomy's final branch was
reached. 30 of 36 table rows differ at raw ulp scale (maximum scaled
difference 2.9604386036136494e-15); ordinary ranks agree everywhere; the
five committed mismatch rows each show 2-3 flipped folded-rank entries; and
both sides' committed R-hat, bulk ESS, tail ESS, and pass flags reproduce
bit-for-bit. Measured attribution: `eNormSquared`, `eMovementSquared`, and
`closedMovementSquared` diverge at projection accumulation;
`closedGramLargest` and `closedGramSmallest` diverge at the assembled Gram
matrix. Thirteen fold flips in non-mismatch rows were masked by the R-hat
max rule. The recovered raw series are retained in the output for both
sides.

The Phase570/572 disagreement is therefore fully explained as an ulp-scale
numerical-representation effect on near-tied folded ranks, not an estimator
or replay defect. Phase572's fail-closed terminal was correct and stands;
no threshold changed, the Phase571 lever remains unconfirmed, and
prospective sampler-pack planning remains closed.

## Hashes

- Program.cs:
  `54de2eb408539268302868513769f308811ce1870c6beb35a0cfc87062f61cd1`
- csproj:
  `dc9904ef746331d48d4111bb2bc52d5cfec3ec3094fe9ee008e42dae9e3a1eff`
- contract v1:
  `db2c78f3b910131b3375a69034d6bd30560e8a3368fbb63a7cd6ed1c8c2ab90e`
- v1 full and summary outputs:
  `74e4d2e6c2a0933879a6f67aa3cba65abb92cfee86363b81238d1451c325364e`

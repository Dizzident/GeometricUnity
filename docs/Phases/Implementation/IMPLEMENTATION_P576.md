# Phase 576 implementation: disjoint-seed chain-pack design

## Status

Phase576 v1 executed under Amendment A40 and returned
`chain-pack-design-frozen-execution-unauthorized` on its first frozen
execution. It is a deterministic, zero-sampling, zero-RNG design phase.

## Method

The contract exact-binds 12 artifacts across the Phase548/550 records and
the executed Phase570-575 chain. The known-answer battery (derivation
arithmetic, planted seed-collision detection, over-ceiling refusal, and the
terminal truth table) runs before any audited numeric read. The upstream
gate requires Phase575's favorable terminal with the planning gate open and
the Phase571 lever confirmed under the registered convention.

Every pack quantity is then derived from committed bytes: the proposal arm
is the Phase571 long arm verbatim; retained trajectories per chain follow
`ceil(1.25 * 2835.7692118162986 / 1.92) = 1847` from Phase548's committed
spectral bound; warmup is `ceil(0.15 * 1847) = 278`. The 900xxx seed
namespace is machine-verified disjoint from every committed Phase548 seed
(including excluded seeds) and disclosed Phase571 momentum seeds; protected
Phase554 seeds are never read. Ceilings: 561,000 estimated force
evaluations against a frozen 620,000 maximum.

## Result

The frozen pack `a40-disjoint-seed-chain-pack-v1`: 8 chains in two tables,
2,125 trajectories per chain at step 0.06 with 32 leapfrog steps, retained
integration time 3,546.24 per chain, mandatory raw directional series
retention, checkpoints every 250 trajectories plus final, hardened
R-hat 1.01 / ESS 100 gates, and the A39 registered fold convention named
for adjudication. Execution is NOT authorized and requires a separately
registered phase plus explicit written user sampling authorization.

## Hashes

- Program.cs:
  `7884f0abdb20b95abb98f75b0f66a7d50563c45938f5612b7d9697b879aba889`
- csproj:
  `de3a625c1165cfd59a28e857eada74b547cc8e17f617a3f2e87d7d9f0b754819`
- contract v1:
  `8c67c5e80c32833b8c5f1443871bc6b92a81ad8242ae7b861144c5af9f23a74a`
- v1 full and summary outputs:
  `6d178b1cd0738b3655cd5c031cafec9943ebab66d0902a54399c00a5cd60f202`

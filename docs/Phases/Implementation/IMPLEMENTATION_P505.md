# Implementation P505 — Phase503 Frozen-Failure Localization

Phase505 exact-binds the Phase503 summary and freezes its localization rules
before reading it. All eleven failed rows are retained: two coverage-only
misses, two false decisive single-to-pair calls, and seven pair ambiguities.
Four observed coverage misses are finite-24 grid misses; seven selection
failures are structural under Phase503's own expected-probability model.

The terminal is `complete-negative-preserved`. The phase changes no Phase503
threshold or verdict, recommends no repair terminal, runs no sampler, and
preserves `promotedPhysicalMassClaimCount=0`.

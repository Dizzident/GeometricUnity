# Phase578: consolidated conditional electroweak-sector ledger

Phase578 executes Amendment A42 item 1 as a deterministic, zero-sampling
consolidation of already committed evidence. It exact-binds the Phase404/429
ratio chain, Phase433/434 conditional running and extraction templates,
Phase451 running comparison, Phase461/464 anchor records, and Phase213
source-lineage deficits.

Every ledger row is labeled `derived` or `conditional`, carries its complete
assumption identifiers at entry, names its exact source bindings, and has
`promotedPhysicalMassClaimCount=0`. There are exactly two electroweak
comparison misses: the conditional tree-level `m_W/m_Z=sqrt(5/8)` row against
the Phase461 declared measured-mass ratio, and the conditional Phase451
two-loop `sin^2(theta_W)=0.210637...` row against `0.23122`, missing by
`115.125...` honest bands. Phase461's `451.105...` referee reconstruction is
retained separately as non-promotional anchor-adjudication evidence and is not
counted as an electroweak comparison miss.

The terminal is the finished honest form of this computation: ratios,
conditional templates, and misses are consolidated, but absolute masses remain
blocked because no unit anchor, observed-field extraction map/theorem, or
quartic/breaking-sector source content exists. Declared measured values remain
comparison-only imports and introduce no unit anchor.

Run:

```text
dotnet run -c Release --project studies/phase578_consolidated_conditional_electroweak_sector_ledger_001/Phase578ConsolidatedConditionalElectroweakSectorLedger.csproj
```

The full and summary outputs are intentionally byte-identical.

The first developmental run stopped fail-closed in the pre-audited-read
known-answer battery because two hand-transcribed binary64 fixture literals
for `80.377/91.1876` and its scaled miss were wrong. No audited numeric JSON
was parsed. That stop is preserved as
`output/developmental_known_answer_failure_v1.json`; the corrected fixture
uses the values computed by the same production helpers.

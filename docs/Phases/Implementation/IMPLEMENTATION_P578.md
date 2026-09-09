# Phase 578 implementation: consolidated conditional electroweak-sector ledger

## Status

Phase578 executed under Amendment A42 and returned
`conditional-electroweak-sector-ledger-complete-absolute-masses-blocked-source-content-absent`.
It is deterministic, zero-sampling, zero-RNG consolidation over committed bytes
only.

## Method

The phase exact-binds nine committed scientific inputs: Phase404 and Phase429 for
the `tan^2(theta_W)=3/5`, `sin^2(theta_W)=3/8`, and conditional
`m_W/m_Z=sqrt(5/8)` chain; Phase433 and Phase434 for the conditional content and
six extraction templates; Phase451 for the one-/two-loop running comparison;
Phase464 for the `blocked-upstream-ambiguous` no-claim record; a canonical
timestamp-free projection of Phase461's zero surviving anchors and separate
reconstruction evidence; and the Phase201 W/Z and Higgs intake templates from
which the phase independently recomputes the 15 W/Z and 14 Higgs missing
source-lineage fields.

All 14 ledger rows name their complete assumption list at entry, identify their
exact source bindings, carry a `derived` or `conditional` classification, set
`predictional=false`, and preserve `promotedPhysicalMassClaimCount=0`. A
pre-audited-read known-answer battery verifies the exact ratio arithmetic,
declared measured-mass ratio arithmetic, terminal precedence, and checksum
tamper detection.

## Result

The two electroweak comparison misses are machine-separated and explicit:

- conditional tree `m_W/m_Z`: `0.7905694150420949` versus the declared measured
  ratio `80.377/91.1876 = 0.8814466001956406`, signed miss
  `-0.0908771851535457` (`0.10310004614389022` scaled absolute);
- conditional two-loop `sin^2(theta_W)`: `0.21063705618289308` versus
  `0.23122`, signed miss `-0.02058294381710693`, or
  `115.1250020940215` honest bands.

Phase461's `451.10527021047886` referee reconstruction is retained in a
separate non-promotional anchor-evidence category and is not counted among the
two electroweak misses. No import-clean trials-surviving anchor exists, Phase434
provides conditional templates but zero source-defined extraction fields, the
breaking sector/quartic is absent, the deficits remain 15/14, and every branch
has zero promoted physical-mass claims.

The terminal therefore records the finished honest form of the computation:
absolute masses still require source content adjudicated absent - a unit anchor,
an observed-field extraction map/theorem, and quartic/breaking-sector content.
Declared measured values remain comparison-only imports and introduce no unit
anchor.

The first developmental execution stopped fail-closed before any audited
numeric JSON was parsed because two binary64 known-answer fixture literals were
transcribed incorrectly. Its record is preserved as
`output/developmental_known_answer_failure_v1.json`; correcting those literals
to the production-helper results yielded the terminal above.

The first integrated v1 replay later stopped fail-closed after the final A42
handoff note changed the scanner-derived Phase213 bytes and triggered a
deterministic Phase461 regeneration. The v1 contract and its exact drift record
remain preserved under `output/lineage/v1-post-documentation-drift/`. Contract
v2 changes only those two exact bindings, identifies v1 as its predecessor, and
was frozen before its first execution against the final documentation corpus.
The integration audit then found that only the implementation notes—not the
three generated study directories—had been excluded from all nine source
scanners. After those directory exclusions stabilized the corpus, v3 changed
only the Phase213 exact binding and preserved the successful v2 output under
`output/lineage/v2-pre-scanner-directory-exclusion/`.
Phase207 then required an enumeration-time filter—not merely a blocker label—
for the verifier and registered generated diagnostics. Contract v4 changes
only the resulting Phase213 binding and preserves v3 under
`output/lineage/v3-pre-phase207-scan-filter/`. A final replay demonstrated that
binding the complete Phase461 artifact retained a volatile `generatedAt` field,
while binding the global Phase213 inventory made the ledger recursively depend
on unrelated repository enumeration. Contract v5 therefore replaces those two
bindings with stable exact scientific inputs: the canonical Phase461 projection
and the two Phase201 intake templates. The phase recomputes the 15/14 deficits
from those templates, and the v4 result is preserved under
`output/lineage/v4-pre-stable-source-projection/`.

## Hashes

- Program.cs:
  `c8375e5369dff17a51d8bf38e142fe0bffa42051b7e489bb3bf833213a6b7e91`
- csproj:
  `de3a625c1165cfd59a28e857eada74b547cc8e17f617a3f2e87d7d9f0b754819`
- contract v1:
  `ce4d11d585570a3acc561600c92445a9f3566f64668f72961a86076177dc6cd0`
- contract v2:
  `661f792d79dec6a189f80d73b662863b5c6a83d1af7c9e86e8cfb03b658c0458`
- contract v3:
  `51d662ea1f1b35a0c06b0ed1700311c725bb85938be1ad967fc6e890deab6f5c`
- contract v4:
  `365eade46f40e2f7863d277cdc62d920fbb77e024e1f3bb6a5385b8d750cfa1f`
- contract v5:
  `37f3aaf0e68bdc0477c5b8b639aadacbac9620ddb14b672a8b3e64eca1fd1e6b`
- stable Phase461 scientific projection:
  `b828b709447065ac82a82c8dbf3f15e7801992b4d7b7d1c6547fa1179472b25f`
- full and summary outputs:
  `4be51d01a358c8d3afb69c5eca4b21cdea4798a693288d95227810cc134dadd3`
- preserved developmental failure record:
  `f9c59025d0762e0985e9d91bc89e469def44612699d18c41575fa1e838b93773`
- preserved post-documentation v1 drift record (full and summary):
  `d7ba0d1eaf652fdabcec7b31d959906618f076f25406ca85f5d6081931ca52ed`
- preserved pre-directory-exclusion v2 output (full and summary):
  `701662a434d552fb21de963aef79c8bf3ed52ff4199ddd4131d35502666f231e`
- preserved pre-Phase207-filter v3 output (full and summary):
  `ad32275edb649a792ed555bb767132c794f0313adb6dc7f3571bbbaddcdd3b02`
- preserved pre-stable-source-projection v4 output (full and summary):
  `f5667ae82078afff38a1433df2f09a13b19f6692b83b08ccfca324cd5dbcedd0`

## Run

```text
dotnet run -c Release --project studies/phase578_consolidated_conditional_electroweak_sector_ledger_001/Phase578ConsolidatedConditionalElectroweakSectorLedger.csproj
```

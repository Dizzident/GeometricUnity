# Phase531: O4 G4 Disposition-Resolution Semantics

This deterministic, zero-physics phase implements Amendment A21's generic
content-resolution contract. It exact-binds Phase530 and the unchanged O4 memo
schema and dependency map. It does not read, create, amend, or interpret a
human memo.

The frozen taxonomy keeps authentication, ruling-set completeness, scientific
resolution, and adverse disposition separate. Missing or invalid
authentication cannot satisfy the consumer. `insufficient-basis` or `defer`
remains unresolved. A fully resolved supporting set is distinct from any set
containing a resolved adverse disposition. `not-applicable` is never treated
as support.

Synthetic cases exercise all-defer, one-defer, all-supporting, one-adverse,
mixed-adverse, duplicate and missing rulings, invalid authentication, and
`not-applicable`. They test only the consumer semantics and are not human
evidence. Current G4 remains missing, external review remains pending, every
execution or promotion authority remains false, and
`promotedPhysicalMassClaimCount=0`.

Run:

```bash
dotnet run -c Release --project studies/phase531_o4_g4_disposition_resolution_semantics_001/Phase531O4G4DispositionResolutionSemantics.csproj
```

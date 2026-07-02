# Phase427: Hofseth GU-RVG Superluminal Source Audit

Discharges the second NEW-LEAD from the 2026-07-01 literature sweep. The
originally-catalogued Zenodo record `21056575` was DELETED on 2026-07-01
(tombstone reason: "duplicate", hours after the sweep found it); the live
successor is record `21117379` (same title, same claimed authorship,
published 2026-06-18; PDF md5 `90be901bc227bc90e493c295aa276046`,
pdftotext 6465 lines).

Result:

- The paper is 95.4 GeV dilaton / Koide / warp-condensate phenomenology
  that IMPORTS the electroweak VEV `v = 246 GeV` as an explicit input
  (its own derived/input/open bookkeeping marks it "I") to set a 27.2 TeV
  dilaton decay constant, and marks its condensate amplitude as "fixed by
  observation rather than computation".
- The claimed Weinstein-Harvard co-authorship is externally unverified
  and matches the fabricated-attribution pattern documented in
  arXiv:2606.02184; the audit records the attribution without endorsing
  it.
- No W/Z/H source rows, observed-field projection, weak-angle lineage,
  pole extraction, or GeV-from-geometry normalization exists in the text.

No Phase201 or Phase256 field is filled; no W/Z/H mass is promoted.

Run:

```bash
dotnet run --project studies/phase427_hofseth_gu_rvg_superluminal_source_audit_001/Phase427HofsethGuRvgSuperluminalSourceAudit.csproj
```

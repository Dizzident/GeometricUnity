# Phase425: Cross-Carrier Bilinear SM-Doublet Completion Audit

Phase424 closed the same-chirality vector-spinor `144` bilinear sector.
Phase425 completes the ENTIRE bilinear composite layer on the source-pinned
carrier menu of GU-DRAFT-2021 (Phase416 census): observed `S = 2 x 16`,
dark vector-spinor `Z = 2 x 144`, dark Rarita-Schwinger-type `Q = 6 x 16`,
and the dark mirror Weyl half.

New construction: the `Q_{3/2}` spacetime factor `6` is materialized exactly
as the gamma-traceless remainder in `4 x 2 = 2 + 6` - the 4D analog of
Phase417's `10 x 16 = 16 + 144` split - with `A4 A4^dag = 4I` exact and
welded content exactly `(1/2,1)` / `(1,1/2)`. The chiral `Q` carriers have
no linear welded scalar.

Results (exact, machine-verified; capacities in the complex convention):

- Mixed-parity channels (`Z_L x S_R`, `Q_L x S_L`, `Q_L x Z_L`,
  `Q_L x Q_R`, and chirality mirrors) have EXACTLY ZERO welded-scalar
  capacity by character arithmetic - closed with no numeric step.
- Same-parity channels all have nonzero capacity but ZERO SM-doublet
  content by the ambient intersection:
  `Z x S` (capacity 13, top Gram eigenvalue 0.022),
  `Q x Q` (14, 0.444), `Q x S` (6, 0.444), `Q x Z` (29, 0.034) -
  intersection real dimension 0 in every channel, both chiralities.
- Mirror-sector channels transfer from decided channels by representation
  identity (Phase416 pinning).

Combined with Phases 409/411/412/422/424: NO BILINEAR COMPOSITE OF ANY
SOURCE-PINNED CARRIER PAIR CARRIES A WELDED-SCALAR SM-DOUBLET. No source
supplies a bosonic projection map, action, VEV selection, observed-field
rows, weak-angle lineage, pole extraction, or GeV normalization; no
Phase201 or Phase256 field is filled and no W/Z/H mass is promoted.

Run (Release; ~1 min):

```bash
dotnet run -c Release --project studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001/Phase425CrossCarrierBilinearSmDoubletCompletionAudit.csproj
```

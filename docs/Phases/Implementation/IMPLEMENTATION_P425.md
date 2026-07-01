# Implementation P425 - Cross-Carrier Bilinear SM-Doublet Completion Audit

Phase425 adds
`studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001`,
completing the bilinear composite layer on the full source-pinned carrier
menu of GU-DRAFT-2021 eq. 11.6 / 12.20 / 12.22: observed `S = 2 x 16`,
dark vector-spinor `Z = 2 x 144`, dark Rarita-Schwinger-type
`Q_{3/2} = 6 x 16`, and the dark mirror Weyl half.

## What It Computes

- Constructs the `Q_{3/2}` spacetime factor exactly: `6` = the
  gamma-traceless remainder in `4 x 2 = 2 + 6` (the 4D analog of the
  `10 x 16 = 16 + 144` split), with `A4 A4^dag = 4 I` exact, kernel
  dimension 6 per chirality, and welded Casimir content exactly
  `(1/2,1)` / `(1,1/2)` (residual 1.3e-15). The chiral `Q` carriers carry
  no linear welded scalar.
- Enumerates the fifteen unprobed two-carrier bilinear channels among
  `S/Q/Z` (both chiralities):
  - the seven mixed-parity channels have EXACTLY ZERO welded-scalar
    capacity by character arithmetic ((int,half) x (half,int) content
    admits no equal-label pairing) and are closed with no numeric step;
  - the eight same-parity channels are decided by the Phase412/424
    ambient intersection on exact SM-diagonal complex bases.
- Verifies every computed capacity against recorded expectations
  (13/13 `Z x S`, 14/14 `Q x Q`, 6/6 `Q x S`, 29/29 `Q x Z`, zeros
  elsewhere), and the `144` complex content against Phase417's record.

## Exact Results

| channel | capacity | V_w | doublet isotypic | intersection | top eigenvalue |
|---|---|---|---|---|---|
| `Z_L x S_L` / `Z_R x S_R` | 13 | 136 | 128 real | 0 | 0.022 / 0.017 |
| `Q_L x Q_L` / `Q_R x Q_R` | 14 | 1152 | 1152 real | 0 | 0.444444 |
| `Q_L x S_R` / `Q_R x S_L` | 6 | 384 | 384 real | 0 | 0.444444 |
| `Q_L x Z_R` / `Q_R x Z_L` | 29 | 408 | 384 real | 0 | 0.033628 |

Mirror channels transfer from decided channels by representation identity
(Phase416 census pinning). `crossCarrierWeldedScalarSmDoubletCount = 0`,
`bilinearCompositeLayerClosedOnAllSourcePinnedCarriers = True`.

## Scientific Boundary

Combined with Phases 409 (frame-cross), 411 (16 bilinears), 412 (16^4
quartic), 422/424 (vector-spinor bilinears), NO BILINEAR COMPOSITE OF ANY
SOURCE-PINNED CARRIER PAIR CARRIES A WELDED-SCALAR SM-DOUBLET. The ambient
intersection is a necessary-condition test over the full tensor product
(conservative superset of all statistics projections); zeros close
channels outright. No reviewed source supplies a bosonic projection map,
carrier action, VEV selection, observed photon/W/Z/H rows, weak-angle
lineage, pole extraction, or GeV normalization, so:

- `canFillPhase201WzContract=False`
- `canFillPhase201HiggsContract=False`
- `canFillPhase256ObservedFieldExtractionContract=False`
- `routePromotesWzMasses=False`
- `routePromotesHiggsMass=False`

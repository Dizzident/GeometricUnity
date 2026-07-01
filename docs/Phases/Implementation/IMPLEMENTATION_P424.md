# Implementation P424 - Vector-Spinor 144 Bilinear SM-Doublet Intersection Analysis

Phase424 adds
`studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001`,
deciding the SM-stable question Phase422 deferred for the 528-dimensional
same-chirality Majorana-like welded-scalar sector of the `Z_{1/2}`
vector-spinor carriers. It also discharges, and regression-guards, the
2026-07-01 `|Y| = 1/2` calibration defect found in the Phase411/417
informational SM censuses.

## What It Computes

- Rebuilds the Phase417 construction with an exact complex kernel basis:
  the gamma-trace map `10 x 16+ -> 16-` satisfies `A A^dag = 10 I`
  exactly, so `P = I - A^dag A / 10` is the exact kernel projector; the
  144 basis simultaneously diagonalizes the four SM Cartans
  (Y, L3, and an exactly-commuting color Cartan pair) with residual
  `1.2e-10`.
- For each same-chirality bilinear (`Z_L x Z_L`, `Z_R x Z_R` on the
  `2 x 144` legs, plus `S_L x S_L`, `S_R x S_R` on the observed `2 x 16`
  legs as the Phase411 re-check), runs the Phase412 ambient-intersection
  pipeline:
  - doublet-candidate weight census (`|Y| = 1/2`, `|m_L| = 1/2`, color
    weight zero), split into exact sign classes;
  - SM-doublet isotypic as the kernel of
    `C_color + (C_L - (3/4) kappa_L)^2` per class;
  - welded-spin `(0,0)` isotypic via exact polynomial spectral projectors
    on the integer Casimir grid `j = 0..6` (idempotency residual
    `<= 1e-6`);
  - intersection Gram with eigenvalue-1 counting.
- Cross-checks the welded-scalar capacities against Phase422 by character
  arithmetic (264/264/0, exact match).

## Exact Results

- `Z_L x Z_L`: `Vw = 3584`, doublet isotypic `1344` real dims,
  `intersectionRealDim = 0`, top Gram eigenvalue `0.059259`.
- `Z_R x Z_R`: identical counts, `intersectionRealDim = 0`.
- `S_L x S_L` / `S_R x S_R`: `Vw = 128`, doublet isotypic `128` real dims,
  `intersectionRealDim = 0` (corrected Phase411 verdict confirmed by the
  strictly stronger ambient method).
- `sameChiralityWeldedScalarSmDoubletAbsent = True`,
  `vectorSpinor144BilinearCompositeRouteClosed = True`.

## Calibration Defect Context (2026-07-01)

The pre-fix Phase411/417 SM censuses calibrated `|Y| = 1/2` as the
"smallest Y^2 eigenvalue above 0.05" on the 16, which selects the
`|Y| = 1/3` family value `1/9`, not the lepton-doublet value `1/4`.
Phase417's corrected census reports
`internalSmHiggsPatternComplexDimension = 6` (a fermionic representation
block, not a welded scalar; the linear no-go is unchanged), and Phase411's
corrected joint-eigenbasis census keeps
`majoranaSpinZeroSmDoubletCount = 0`. Phase424's precursor gates assert
both corrected calibrations, and its `S x S` ambient re-check
independently confirms the Phase411 verdict.

## Scientific Boundary

The ambient intersection is a necessary-condition test over the full
tensor square (a conservative superset of every statistics projection); a
zero closes the channel outright. No reviewed source supplies a bosonic
projection map, carrier action, VEV selection, observed photon/W/Z/H
rows, weak-angle lineage, pole extraction, or GeV normalization, so:

- `canFillPhase201WzContract=False`
- `canFillPhase201HiggsContract=False`
- `canFillPhase256ObservedFieldExtractionContract=False`
- `routePromotesWzMasses=False`
- `routePromotesHiggsMass=False`

The vector-spinor `144` branch is now closed at linear order (Phase417),
mixed-chirality bilinear order (Phase422), and same-chirality bilinear
order (Phase424).

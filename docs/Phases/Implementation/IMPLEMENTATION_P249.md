# Phase 249: Invariant Origin Search for Near-Miss Constants

## Purpose

Search for target-independent invariant origins of the two active near-miss constants:

- W/Z: the P221 `2/sqrt(3)` SU(2) adjoint RMS normalization factor.
- Higgs: the P223 `3/10` quartic factor.

## Inputs

- Phase12 geometry and spinor representation metadata.
- Phase63, Phase64, and Phase65 weak-coupling normalization artifacts.
- Phase221 and Phase223 numerical lead artifacts.
- Phase225, Phase247, and Phase248 repairability/source-application audits.
- Phase213, Phase245, and Phase246 blocker/unlock artifacts.

## Result

The search finds exact invariant arithmetic for the current numerical leads, but not a source-backed application law.

- `sqrt(C2(adj)/dim(su2)) / sqrt(1/2) = 2/sqrt(3)` exactly reproduces the P221 W/Z normalization lead.
- `dim(su2)/(dim(Y_h)+dim(X_h)+dim(su2)) = 3/(5+2+3)` and `2*C2(fund)/(C2(adj)+dim(su2)) = 3/10` exactly reproduce the P223 Higgs factor.

These are not promoted. P225/P247 still block the W/Z adjoint-RMS application to the Phase64 fermion-current matrix element, and P248 still blocks the Higgs `3/10` factor because no scalar source/operator or quartic source derives it.

## Artifacts

- `studies/phase249_invariant_origin_search_for_near_miss_constants_001/output/invariant_origin_search_for_near_miss_constants.json`
- `studies/phase249_invariant_origin_search_for_near_miss_constants_001/output/invariant_origin_search_for_near_miss_constants_summary.json`

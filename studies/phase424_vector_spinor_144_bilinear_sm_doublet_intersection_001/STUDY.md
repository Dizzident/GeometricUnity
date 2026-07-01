# Phase424: Vector-Spinor 144 Bilinear SM-Doublet Intersection Analysis

Phase422 established the same-chirality Majorana-like welded-scalar CAPACITY
of the chiral vector-spinor carriers (`Z_L x Z_L = 264`, `Z_R x Z_R = 264`,
`Z_L x Z_R = 0`) and explicitly deferred the direct SM-stable analysis of the
528-dimensional sector. Phase424 decides that deferred question with the
Phase412 ambient-intersection method, which is strictly stronger than a
stable-subspace census and covers all statistics projections.

Result (exact, machine-verified):

- `Z_L x Z_L`: candidate weight sector 3584, SM-doublet isotypic 1344 real
  dimensions, intersection with the welded-spin (0,0) isotypic ZERO
  (top Gram eigenvalue 0.059259 vs the required 1.0).
- `Z_R x Z_R`: identical counts, intersection ZERO.
- The 528-dimensional same-chirality welded-scalar capacity therefore
  contains NO SM-doublet state. Together with Phase422's mixed-chirality
  closure and Phase417's linear closure, the vector-spinor 144 bilinear
  composite route is CLOSED.
- The same pipeline re-checks the observed `2 x 16` Majorana channels
  (`S_L x S_L`, `S_R x S_R`): intersection ZERO in both, confirming the
  corrected Phase411 verdict with a stronger method.

This phase also carries the 2026-07-01 calibration-defect regression: the
pre-fix Phase411/417 census heuristic ("smallest Y^2 above 0.05") selected
the |Y| = 1/3 family value 1/9 instead of the exact lepton-doublet value
1/4. Phase424 asserts the corrected exact calibration
(`sixteenCarriesLeptonDoubletYQuarter`, Phase417 `yHalfCalibrationExact`,
Phase411 `majoranaYHalfCalibrationExact`) in its precursor gates.

Character cross-checks reproduce Phase422's capacities exactly
(264/264/0). No source defines a bosonic projection map, action, VEV
selection, observed photon/W/Z/H rows, weak-angle lineage, pole extraction,
or GeV normalization; no Phase201 or Phase256 field is filled and no W/Z/H
mass is promoted.

Run (Release: the dense complex spectral projectors are ~25 min under the
Debug JIT vs ~7 min optimized):

```bash
dotnet run -c Release --project studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001/Phase424VectorSpinor144BilinearSmDoubletIntersection.csproj
```

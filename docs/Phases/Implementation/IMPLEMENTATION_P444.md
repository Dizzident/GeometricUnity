# Implementation P444 - Mode-Volume-Scaled Saturation Probe (Recorded Findings)

Phase444 pursued the phase443-named mode-volume lever (the same joint
effective-potential analysis on the 81-vertex M1b torus, 16x the joint
DOF) and honestly records that the verdict is UNDETERMINED -
TOOLING-BLOCKED, with the blocking measured at three levels:

1. The pinned lowest-index vertex->face rule is not translation-covariant
   (56.8% of face-translation pairs violate covariance).
2. Face bivectors were built from raw coordinate differences, violating
   the M1b mesh's documented minimal-image contract (75.1% seam faces,
   orbit norm-difference 4.84 raw vs exactly 0.0 minimal-image).
3. DEFINITIVE (signed-S_B covariance test, superseding an earlier
   confounded measurement - honesty note recorded): all four
   rule x bivector conventions fail covariance (best 4.8e-3 vs the 1e-8
   bar), and the pure curvature ||F||^2 alone fails at 2.5e-4 - the root
   cause is Gu.Geometry's global-index orientation conventions, which do
   not commute with lattice translation. Exact momentum
   block-diagonalization is NOT viable without a geometry-layer change.
4. The SLQ fallback is measured infeasible (~60 s per Hessian-vector
   product without a platform adjoint; ~2 h per composite point).

AUTHORIZED ADDITIVE PLATFORM CHANGES (kept; leader-reviewed;
open-mesh byte-identical - hard-gated by tests): VertexFaceRule
{LowestIndex default, IncidentAverage} on the family spec (BranchId
suffix only for the new rule) and a latticePeriod option applying
minimal-image bivectors on periodic meshes; 8 new tests including one
that DOCUMENTS the measured residual non-covariance as a known
limitation. These are the physically-correct periodic conventions -
necessary but not sufficient for the block path.

TWO NAMED UNLOCK PROJECTS (user decisions, measurement-scoped, NOT
started): (i) lattice-canonical orientation conventions through
Gu.Geometry (MeshTopologyBuilder + CurvatureAssembler + Shiab assembly)
=> exact torus block-diagonalization; (ii) an adjoint/joint-gradient
platform path => feasible stochastic Lanczos quadrature. The mode-volume
heuristic (more modes rescale the subleading log; a mechanism, not
volume, is likely what is missing) is recorded, LABELED, non-verdict.
physicistReviewPending=true on the minimal-image contraction semantics
(session expired; documented rationale carried). Nothing promoted.
Runtime ~5 s.

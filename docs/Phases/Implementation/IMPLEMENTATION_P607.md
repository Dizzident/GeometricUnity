# Phase607 — source-induced vertical curvature audit

Status: completed. The first MAIN-approved frozen Release execution passed
unchanged after full independent and MAIN code/proof/contract reviews.

The standalone exact audit compares Koszul-recovered metric-fiber connection
and its differentiated coefficients against the full commutator curvature
law. Four rational contexts cover two congruent Lorentz fiber points and
two nondegenerate trace weights. The full10-direction basis, curvature
symmetries, congruence, inertia, nonzero transported witness and planted
flat-connection errors are checked without sampling or fitting.

The ambient argument uses orthogonal blocks, flat downstairs splitting and
base-independent vertical metric to establish totally geodesic fibers; it
does not assume a product metric. This separates explicit flat14D controls
from the declared source-induced reference even with flat downstairs metric.
It does not infer a nonzero Shiab contraction, reject a stationary background,
choose a source normalization, or calculate a full metric Euler equation.

Study: `studies/phase607_source_induced_vertical_curvature_audit_001`.
Project: `Phase607SourceInducedVerticalCurvatureAudit.csproj`.
Contract: `phase607-a57-source-induced-vertical-curvature-v1`.
Expected terminal:
`source-induced-vertical-curvature-controls-pass-flat-reference-not-induced`.

Release build passed with0 warnings and0 errors. MAIN executed the first
scientific run in2.045 seconds wall time. Independent read-only inspection
confirmed the success terminal, all controls/counts,16 unique exact bindings,
and live726-file core closure. No scientific file was changed after freezing.

The calculation checked400 connection outputs,4000 connection derivatives,
4000 curvature outputs,40000 independent second metric jets, and800 complete
ambient vertical-pair connection outputs. All four transported witnesses
gave R(A,B)B=-A, lowered curvature-2 and sectional curvature-1/2; each flat
connection decoy had metric-compatibility defect12. The nonzero curvature
census was1344 full outputs and1536 coordinate coefficients. Instrumented
matrix arithmetic recorded2282563 products, below its frozen ceiling.

Frozen contract SHA256:
`001cdcd42da3acfb024e226739d83fa292fabb78bd04f3928f7ee4cd3dc0dccd`.
Byte-identical full and summary artifact SHA256:
`a458c05fbda8d568d9509856955dcbab3c089bbed941077c9c288ea0b89e2348`.

All14 authority flags remain false, external review/O4 pending, physical
mass claims0. No core or previously frozen scientific files were changed.

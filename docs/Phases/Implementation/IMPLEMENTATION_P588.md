# Phase588: fixed-domain action and directional-force consistency

Amendment A47 pack, frozen before its first scientific Release execution.
The [frozen study and proof](../../../studies/phase588_fixed_domain_action_force_consistency_001/STUDY.md)
extends the signed spatial dictionary through the actual 4D action, default
face pairing and analytic gradient on fixed-domain open n=1,2,4 meshes.

An independent face census predicts the finite-mesh quadratic form, including
its self-dual restriction. Direct basis residual controls supplement action
and directional tests. Two affine noncommuting profiles test independently
integrated curvature and a proved global O(h) error bound that includes
cell contraction, averaging, Riemann sums and boundary faces. Separate
fixed-topology homotheties remain local scaling controls only.

First frozen Release execution passed (2026-09-08):
`fixed-domain-action-force-controls-pass-induced-pairing-scoped`.

All 12 bindings and the 726-file source tree match. All 126 constant rows,
36 direct residual rows, 252 cross directions, 12 anchors, 16 homothety
rows and 24 smooth rows pass. The type-census/metric and moment identity
errors are zero. Maximum constant action scaled error is 2.22e-16;
directional error is 2.35e-15; both direct contraction and full signed
assembled residual errors are at most 2.22e-16. All 48 Richardson pairs
pass, with maximum scaled error 6.07e-10 against the frozen 1e-8 tolerance.

The default unit-face pairing has the derived finite-mesh quadratic form
`G_h=(1.5+2h+0.5h^2)I+(1.5+0.5h)B^T B`. Its full-space limit has eigenvalues
1.5 and 7.5, while the self-dual restriction has the scalar factor
`4.5+3h+0.5h^2`. Thus full-form anisotropy does not invalidate this particular
self-dual smooth restriction. It does not select the source pairing.

Representative action values show the remaining finite-mesh error:

| Profile / member | n=1 | n=4 | Analytic smooth limit |
|---|---:|---:|---:|
| Witness / identity | 18 | 11.47021484375 | 9.75 |
| Four-component / identity | 29.279296875 | 19.41245236992836 | 16.7841796875 |
| Witness / SD2 | 2.234375 | 1.4324340820312382 | 1.21875 |
| Four-component / SD2 | 3.0761132812500005 | 1.853248104328933 | 1.5692138671875 |

The proved global error bounds pass but are deliberately conservative:
observed errors are below roughly 4.7e-8 of their bounds. This ratio measures
the looseness of the bounds, not small relative discretization error.
No fitted slope gates were used. In particular, the zero-limit independent
direction on the SD2 witness is nonmonotone from n=1 to n=2, as allowed by
the prospective design; no row or mesh was selected after execution.
The h-to-zero theorem concerns the ideal exact-arithmetic discretization.
Actual float64 controls cover only the frozen meshes/scales; the absolute
core pivot floor prevents arbitrary numerical refinement.

The own-project Release build has zero warnings/errors. No scientific repair
or frozen-input edit was made. The full [output](../../../studies/phase588_fixed_domain_action_force_consistency_001/output/fixed_domain_action_force_consistency.json)
and summary both have SHA256
`7dec161d84073ff72fdfadbacd63534538b7f829690edaa7ce775d2227caeef4`.
Frozen contract SHA256:
`6fb8daa6abe002d5a67e0d5df605ca1e259e65e85f7c9f12e3c7c1144cce5061`.

No source pairing, kinetic metric, rough-field limit or physical particle
interpretation is established. All authority flags remain false, external
review remains pending and promotedPhysicalMassClaimCount=0.

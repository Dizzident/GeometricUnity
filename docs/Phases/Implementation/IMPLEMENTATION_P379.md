# Phase379 Response-Image Carrier-Axis Characterization

Phase379 characterizes the positive eigenspace of the Phase378 full
connection-carrier response Gram matrix on both persisted backgrounds.

Input object:

```text
Q_ij = Re Tr(G_i^dagger G_j)
```

where `i,j` run over the `156 = 52 x 3` edge-major connection-`1`-form carrier
coordinates.

Validation:

- Recomputes the positive carrier eigenspace from the Phase378 `156 x 156`
  response Gram matrices.
- Verifies the rank remains `3` on both backgrounds.
- Computes gauge-axis trace/projector capture in the edge-major carrier.
- Computes top edge-support diagnostics.
- Computes principal angles between the two background carrier images.
- Imports Phase307 W/Z selector status only as non-promotional context.

Observed result:

- The positive image is dominated by carrier gauge axes `0` and `2`.
- Carrier gauge axis `1` is suppressed below `0.2%` of the rank-`3` projector
  trace on both backgrounds.
- The rank-`3` images are not strictly identical across backgrounds; the
  minimum inter-background singular value is about `0.8`.

Boundary:

Phase379 is a target-blind discrete characterization of a study-defined
response image. It does not provide W/Z source rows, an observed electroweak
field map, a physical action Hessian, a Higgs scalar source, GeV
normalization, or a W/Z/H prediction.

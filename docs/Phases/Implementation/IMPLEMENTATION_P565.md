# Phase565 implementation

Phase565 implements Amendment A34's conditional finite-transport control. It
uses unit quaternions for SU(2), the exact registered edge orientation and
`Faces[f]` tuples, and a composable `v0 -> v1 -> v2 -> v0` face loop. Untraced
holonomy conjugation, reverse-loop inversion, basepoint transport, and the
class-function check all run before the weak-field comparison.

The exact finite covariance residual is `6.741092193e-16`. The logarithm of the
composable loop differs from registered curvature at second order (slope
`2.000016`), while exponentiating the registered boundary-array order has only
a third-order remainder (slope `3.000003`). The signed coefficient mismatch is
the preregistered `[x2,x1]` tensor with relative error `1.842677e-4`. The
terminal is `registered-curvature-continuous-holonomy-second-order-mismatch`:
the registered order is `e01, reverse(e02), e12`, whereas a composable loop is
`e01, e12, reverse(e02)`. Exact covariance itself is treated only as a control.

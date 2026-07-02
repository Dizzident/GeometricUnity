# Phase441: Toy-Branch Family Universality Sweep

The last named internal experiment. Phases 435/436/440 pinned the bosonic scale
gap on the CONTROL branch (identity Shiab, trivial torsion, `A0 = 0`), three
independent ways: the exact control-branch Hessian is degree-2 in the background
amplitude with a positive-definite `t^2` term (no log-saturation, Phase436); the
growing-mode counts 64/96 confirm Phase430's direction selection; the coupled
boson-fermion system has no interior fixed point (Phase440). This phase asks
whether that structure is UNIVERSAL across the toy-realizable branch family.

The platform admits a family of toy completions:

- **Shiab** in `{ identity-shiab, first-order-curvature, metric-scaled-shiab(lambda=2) }`
- **Torsion** in `{ trivial, augmented-torsion, local-algebraic }`
- **A0** in `{ 0, const lambda_8 (amp 0.5, 1.0), mixed lambda_8+lambda_4 constant }`

The full `3 x 3 x 4 = 36`-member Cartesian product is swept on the 2x2 su(3)
fiber-bundle mesh (52 edges, 24 faces; carrier 416, Upsilon 192), reusing the
Phase436 finite-difference machinery per member. Everything below is measured,
not assumed.

## Key structural fact (verified member by member)

The residual is `Upsilon = S(F) - T`, `F = d omega + (1/2)[omega wedge omega]`.
Every quadratic-in-`omega` piece is a Lie bracket. Along a rank-1 background
`t*u` with `u` a SINGLE Lie direction (exact-1-form profile), every bracket
`[u,u] = 0`, so `Upsilon(t*u)` is AFFINE in `t` for EVERY family member. Hence
the exact Hessian `H(t) = J(t)^T J(t) + <U(t), Hess U>` is exactly degree-2 in
`t`, and its `t^2` coefficient reduces to `H2 = J2^T J2` with `J2 = J(u) - J(0)`
- exactly Phase436's operator. Moreover `A0` enters `J(omega)` only additively
and constant, so it cancels from `J2`: **the growing-mode structure is
`A0`-independent.**

## Family table (verdicts, measured)

| verdict | result |
|---|---|
| `upsilonDegreeTwoUniversalAcrossFamily` | **true** (max third field-difference `1.5e-15`) |
| `hessianQuadraticDecompositionUniversalAcrossFamily` | **true** (max third `t`-difference `2.8e-14`) |
| `noSaturationTheoremExtendsToEntireToyFamily` | **true** |
| `growingModeCountUniversalAcrossFamily` | **true** (every member 64/96) |
| `runawayVerdictUniversalAcrossFamily` | **true** (every member net `-640 < 0`) |
| `canonicalPhysicalShiabRealizableOnToy` | **false** |
| `scaleGapRequiresDimFourSpinorShiabOrSourceAnchor` | **true** |

Per distinct `(shiab, torsion)` signature (counts are `A0`-independent):

| shiab | torsion | counts (l8/l4) | max H2 (l8/l4) | net8 | runaway |
|---|---|---|---|---|---|
| identity-shiab | trivial | 64/96 | 0.203/0.271 | -640 | yes |
| identity-shiab | augmented-torsion | 64/96 | 0.203/0.271 | -640 | yes |
| identity-shiab | local-algebraic | 64/96 | 1.829/2.438 | -640 | yes |
| first-order-curvature | (same as identity) | 64/96 | (identical) | -640 | yes |
| metric-scaled-shiab | trivial | 64/96 | 0.813/1.084 | -640 | yes |
| metric-scaled-shiab | local-algebraic | 64/96 | 3.251/4.334 | -640 | yes |

The count is invariant family-wide; only mass VALUES rescale. `identity-shiab`
and `first-order-curvature` are numerically identical (both `S = F`).
`metric-scaled-shiab(lambda=2)` scales `H2` by `lambda^2 = 4`. `local-algebraic`
torsion multiplies `H2` (different `J2`) but preserves the count.

## Degree-0 / degree-1 components (honest, from A0)

- **Trivial** torsion: inhomogeneity exactly `0` for all A0 (`S(0)=0`, `T=0`).
- **Augmented** torsion: inhomogeneity `0` at `A0=0`, but `2.449 / 4.899 / 6.928`
  for the nonzero A0 backgrounds (the `-d_{A0}(A0)` term, scaling with amplitude).
- **Local-algebraic** torsion: inhomogeneity `0` even at nonzero *constant* A0,
  because `[A0,A0] = 0` for a constant field - a genuine distinction from
  augmented torsion, recorded faithfully.

The degree-1 free-kinetic linear part is `||d|| = 24` (identity/first-order) and
`48` (metric-scaled, `= 2 x 24`).

## Balance (workbench convention reused)

Net one-loop slope on the hypercharge (`lambda_8`) axis for derived content:

```
netDerivedSlope = 2 (polarizations) * lambda8GrowingCount - 768 (fermionic)
                = 2 * 64 - 768 = -640   (matches Phase440 netSDerived)
```

Negative for every member -> the fermionic runaway persists family-wide. This
reuses the Phase430/440 polarization and derived fermion-count convention and is
a per-volume slope SIGN only.

## Canonical-Shiab impossibility (a key recorded output)

The draft's canonical active Shiab branch `Sigma_mc` (draft v29 Section 32.2,
Inserted Choice IX.32.2.1) is a Ricci/Weyl-like background algebraic contraction
`K_{A0}` built from the Einsteinian projection and metric/bundle data. It CANNOT
be realized on the 2D toy: it requires `dimX >= 4` and a metric-spinor
(`Cl(7,7)/128`) invariant basis. `MetricScaledShiabOperator`'s own comment
records that on `dimX=2`, `Lambda^2(T*X)` is 1-dimensional, so scalar scaling is
the only distinguishing Shiab variant. Recorded requirements:
`dimXAtLeastFour = false`, `spinorRealizedInvariantBasis = false`.

## Batteries

- Control member (identity, trivial, A0=0) reproduces Phase436 exactly: counts
  64/96; odd-in-`t` term `|A1|/|A0| = 0` (Cartan `lambda_8`) and `0.153` (root
  `lambda_4`).
- `BranchOperatorRegistry.ValidateCarrierMatch` passes for all 36 members.
- Augmented-torsion + nonzero A0 inhomogeneity nonzero; trivial-torsion
  inhomogeneity exactly zero.
- Third-difference residuals tabulated (degree `<= 1.5e-15`, decomposition
  `<= 2.8e-14`).

## Fail-closed

Toy family only; workbench polarization convention reused; no GeV promotion. No
scale, pole, or coupling lineage; no Phase201/Phase256 contract field filled;
nothing promoted. `toyBranchFamilyUniversalitySweepPassed` gates on internal
consistency and passes regardless of how the (honest) count/runaway verdicts
fall. Runtime ~21 s.

## Terminal frontier statement

The no-saturation theorem extends to the entire toy family: no toy-realizable
branch completion (any Shiab, torsion, or constant A0 in this family) can produce
a dynamical bosonic scale at one loop. A finite scale requires structure BEYOND
the toy family - a `dimX >= 4` spinor-realized canonical Shiab (`Sigma_mc`) or a
source anchor - which no reviewed source realizes on the toy.

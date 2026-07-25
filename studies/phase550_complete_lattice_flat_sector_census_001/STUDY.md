# Phase550 - Complete-Lattice Flat-Sector and Spectral Census

Amendment A30. A deterministic, zero-sampling census of the second-order form of
the registered complete-lattice action at the origin, at the six preserved
Phase548 checkpoint positions, and along one flat ray. No RNG is constructed, no
registered seed is touched, and the two remaining blind seeds stay blind.

## Why a deterministic census, and not a sampler change

Four facts re-read from the committed sources, not from the A29 narrative,
decide that the next measurement is deterministic.

The Phase548 smallest-eigenvalue number is the Rayleigh quotient of a
shifted-power iterate, which for a symmetric operator is an UPPER bound. The
committed record therefore establishes only `lambdaMin <= 1.227e-06`, the
condition number is a lower bound, and the derived mixing-length figure is pi
over the square root of an upper bound on a quantity that may be exactly zero.
This phase consumes none of those numbers as an input, a threshold, or a target.

Extent three is the enforced floor, `beta` is a recorded label that never enters
the executed value, and the frozen pack cannot buy the missing path length. So
the decidable question is what the second-order form actually looks like.

## What the operator's own primitives buy

The dense form is assembled exactly, with no finite differences:

```
H(x)u = LinCurvT(x, K(LinCurv(x,u))) + LinCurvT(u, w0) - LinCurvT(0, w0)
w0    = K(F(x)),  K(z) = C^T (M (C z))
```

Arm A cross-checks it against a four-point antisymmetric extraction from the
registered gradient (worst relative deviation `2.3e-15`) and tests the cubic
premise itself with a six-point prediction (worst residual `3.7e-15`) instead of
assuming it. One dense form costs about 35 seconds rather than the 330 a
finite-difference route would need, which is why all ten base points fit inside
the frozen 1800 CPU-second ceiling; the measured cost was 557 seconds.

## What was measured

**The flat sector at the origin is exactly 252-dimensional, and the bracket
closes.** The lower bound is threshold-free: the discrete `d.d = 0` identity
holds on all 81 vertex functions and 4050 faces in exact integer arithmetic, the
scalar coboundary has finite-field rank `1131` over both frozen primes, and an
exhibited set of 85 exactly closed integer generators (81 exact 1-forms plus 4
lattice winding forms) has rank `84`. Rank plus exhibited nullity close the
integer, so `84 * 3 = 252` directions lie exactly in the kernel. The
threshold-conditional inertia count at the smallest rung above the roundoff
floor is also `252`, with a plateau across the three lowest rungs and zero
negative inertia.

Both recorded falsifiers are therefore refuted: the nullity is neither below 252
(the identity holds) nor above it (the registered contraction annihilates
nothing beyond the image of `d`).

**The flat directions are exactly flat, and unbounded.** Five frozen closed
single-algebra-axis directions evaluate to exactly `0.0` at every rung of a
ladder up to `t = 1000`. The two negative controls - closed but spread over two
algebra axes, so with a non-vanishing self-bracket - are strictly positive and
scale as the exact fourth power, which is what keeps the arm from being
fail-open.

**At the pilot's own configurations the form is indefinite.** At all six
preserved positions the count below `+1e-9` equals the count below `-1e-9`
(105 to 117 depending on the chain), so those directions are not near-null: they
are negative. The origin flat block is strongly lifted there, with restricted
eigenvalues in `[0.26, 2.2]`, and the largest eigenvalue rises from `1.976` at
the origin to about `10` to `12`.

**Along a flat ray the transverse scale grows exactly quadratically.** At
`t = 0.5, 5, 47` on a unit-normalized exact 1-form, 200 of the 252 origin flat
directions remain exactly flat and 52 acquire curvature, with the largest
restricted eigenvalue `1.071e-04`, `1.071e-02`, `9.460e-01` - ratios of exactly
`(5/0.5)^2` and `(47/5)^2`. The flat-block log-determinant is undefined
precisely because 200 directions stay flat; the full log-determinant above the
roundoff floor is reported instead and is labelled model-based.

**The value at real configurations is a genuine quartic mixture.** The exact
homogeneous decomposition `S(tx) = t^2 S2 + t^3 S3 + t^4 S4`, solved from three
evaluations with no fitting, gives fractions near `0.62`, `-0.07`, `0.44` at all
six positions, with consistency residuals at `1e-15`.

**The declared observable classes do not survive measurement off the origin.**
At the origin `actionDensity` and `forceNormSquared` are invariant along the
measured flat sector to `1e-35` and `1e-31`. At the six preserved positions they
move by `6e-04` and `4.4e-03`. The Phase548 declaration was made prospectively
and matches at the origin only.

This is invariance along the MEASURED flat sector of the second-order form. No
gauge group, orbit, or quotient is constructed anywhere in this phase, so it is
not a gauge-invariance statement and may not be read as one.

## Known limitation of the deflated iterative bound

Arm D's deflated iterative bound at the origin returns an interval around zero
rather than the gap above the flat sector. Deflation removes the 252 constructed
null vectors exactly, but the assembled form's numerically-zero eigenvectors
differ from them at roundoff level, so the deflated operator genuinely retains an
eigenvalue near `1e-13`, and the unshifted Lanczos converges to it rather than to
the gap. The interval is honest and does bracket a real eigenvalue of the
deflated operator; the gap above the measured flat sector is the
tridiagonal-route value `0.18688216`, which carries the trace and
squared-Frobenius consistency checks behind it.

Phase551 sharpened this. Its shifted block subspace iteration, deflated against
an independently rebuilt basis, converges instead to `0.186888 +/- 0.000371`,
which contains the tridiagonal reference. So both routes report valid
a-posteriori intervals around genuine eigenvalues of the deflated operator - they
simply reach different ones, and "the smallest eigenvalue on the complement of
the measured null basis" is not a well-posed comparison target. Phase551's
contract was repaired to compare the well-posed quantity, with its v1 preserved
and non-citable. This phase's frozen bytes are unchanged: its gate required the
interval to bracket an eigenvalue, which it does, and re-running arm D with a
refined deflation after seeing the number would be the retrospective tuning this
program forbids.

## What this does not establish

Nothing about stationarity, sampling correctness, mixing, convergence, transfer
to a larger extent, any gauge INTERPRETATION of a measured null direction, a
quotient, a measure normalization, a production default, a Phase458 gate, an O4
discharge, or any physical or unit-carrying quantity. A measured nullity is a
fact about a matrix; calling it gauge volume is a ruling this phase does not
make. Phase548 and Phase549 are untouched, and
`promotedPhysicalMassClaimCount = 0`.

The terminal is keyed to certification quality and never to which outcome the
data support, so there was no favorable answer to steer toward. Phase551 is the
required independent adjudicator.

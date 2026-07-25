# Implementation P550: Complete-Lattice Flat-Sector and Spectral Census

Amendment A30. A deterministic, zero-sampling census of the second-order form of
the registered complete-lattice action, at the origin, at the six preserved
Phase548 checkpoint positions, and along one flat ray. No RNG is constructed, no
registered seed is touched, and the two remaining blind seeds stay blind.

The Phase548 smallest-eigenvalue number is a Rayleigh quotient of a
shifted-power iterate and therefore an UPPER bound on the smallest eigenvalue.
No arm here consumes it as an input, a threshold, or a target, and the contract
records that it may not be cited as a measurement.

## Exact assembly

The dense form is built from the operator's own linearization primitives, with
no finite differences:

```
H(x)u = LinCurvT(x, K(LinCurv(x,u))) + LinCurvT(u, w0) - LinCurvT(0, w0)
w0    = K(F(x)),  K(z) = C^T (M (C z))
```

Arm A cross-checks this against a four-point antisymmetric extraction from the
registered joint gradient (worst relative deviation `2.3e-15`) and tests the
cubic premise with a six-point prediction rather than assuming it (worst
residual `3.7e-15`). One dense form costs about 35 CPU-seconds instead of the
330 a finite-difference route needs, which is why ten base points fit inside the
frozen 1800 CPU-second ceiling. Measured cost: 557 CPU-seconds.

## The flat sector, bracketed by a threshold-free integer

The discrete `d.d = 0` identity holds on all 81 vertex functions and 4050 faces
in exact integer arithmetic. The scalar face-by-edge coboundary has finite-field
rank `1131` over both frozen primes, so the scalar nullity is at most `84`. An
exhibited set of 85 exactly closed integer generators - 81 exact 1-forms and 4
lattice winding forms - has rank `84`, so the scalar nullity is at least `84`.
The integer closes: `84`, and `84 * 3 = 252` directions lie exactly in the
kernel of the second-order form at the origin.

The threshold-conditional inertia count at the smallest rung above the roundoff
floor is also `252`, with a plateau across the three lowest rungs and zero
negative inertia. Both recorded falsifiers are refuted: the nullity is neither
below 252 nor above it, so the registered contraction annihilates nothing beyond
the image of `d`.

## Exact flatness, with the control that keeps it honest

Five frozen closed single-algebra-axis directions evaluate to exactly `0.0` at
every rung of a ladder up to `t = 1000`. The two negative controls - closed but
spread over two algebra axes, so with a non-vanishing self-bracket - are
strictly positive and scale as the exact fourth power. Without them a zero could
have meant only that the constructed vector was zero.

## Off the origin

At all six preserved positions the count below `+1e-9` equals the count below
`-1e-9` (105 to 117 by chain), so those directions are not near-null: they are
negative. The origin flat block is strongly lifted there, with restricted
eigenvalues in `[0.26, 2.2]`, and the largest eigenvalue rises from `1.976` to
about `10` to `12`. The exact homogeneous decomposition of the value, solved
from three evaluations with no fitting, gives degree-2, degree-3 and degree-4
fractions near `0.62`, `-0.07` and `0.44` at all six, with consistency residuals
at `1e-15`.

Along a flat ray at `t = 0.5, 5, 47`, 200 of the 252 origin flat directions
remain exactly flat and 52 acquire curvature whose largest restricted eigenvalue
grows as exactly the square of the ray parameter. The flat-block log-determinant
is undefined precisely because 200 directions stay flat; the full
log-determinant above the roundoff floor is reported instead and labelled
model-based.

## Measured, not declared

At the origin `actionDensity` and `forceNormSquared` are invariant along the
measured flat sector to `1e-35` and `1e-31`; at the six preserved positions they
move by `6e-04` and `4.4e-03`. The Phase548 classification was recorded
prospectively and matches at the origin only.

This is invariance along the MEASURED flat sector of the second-order form. No
gauge group, orbit, or quotient is constructed, so it is not a gauge-invariance
statement.

## Limitation recorded rather than repaired

The deflated iterative bound at the origin returns an interval around zero
rather than the gap above the flat sector. Deflation removes the constructed null
basis exactly, but the assembled form's numerically-zero eigenvectors differ from
it at roundoff level, so the deflated operator genuinely retains an eigenvalue
near `1e-13` and the unshifted Lanczos converges to that one. The interval is
honest and brackets a real eigenvalue; the gap above the measured flat sector is
the tridiagonal-route value `0.18688216`, which carries the trace and
squared-Frobenius consistency checks.

Phase551's shifted block iteration reaches `0.186888 +/- 0.000371` instead, which
contains that reference. Both are valid a-posteriori intervals around genuine
eigenvalues of the deflated operator, so the quantity itself is not a well-posed
comparison target; Phase551's contract was repaired to compare the well-posed one
and its v1 is preserved and non-citable. These frozen bytes are unchanged, since
re-running arm D with a refined deflation after seeing the number would be
retrospective tuning.

## Scope

No stationarity, sampling correctness, mixing, convergence, transfer to a larger
extent, gauge interpretation of a measured null direction, quotient, measure
normalization, production default, Phase458 gate, O4 discharge, or physical or
unit-carrying quantity. Phase548 and Phase549 are untouched, external review
remains pending, and `promotedPhysicalMassClaimCount = 0`. The terminal is keyed
to certification quality and never to which outcome the data support.

# Implementation P448 - Torus Mode-Volume Saturation Probe

Phase448 answers the question phase444 left undetermined-tooling-blocked
- does the phase443 one-loop no-saturation verdict change with mode
volume? - using BOTH completed unlock projects together: the
lattice-canonical torus (exact translation covariance, commit 82d43559)
and the adjoint/joint-gradient path (ms-scale Hessian-vector products,
commit 7a7e397d).

## Construction

Translation-INVARIANT unit rays (the constant-field Coleman-Weinberg
analogue; per-type coefficients shared across volumes) on canonical
tori n in {3,4}. Covariance makes the joint Hessian BLOCK-CIRCULANT:
its exact spectrum comes from 48 orbit-representative columns (15
minimal-image edge types + 1 vertex type, x su(2)), each ONE
Hessian-vector product, DFT-assembled into n^4 Hermitian 48x48
momentum blocks ({k,-k} pairs via the real symmetric embedding). The
3888-dim (n=3) and 12288-dim (n=4) spectra are exact - no dense FD, no
SLQ variance. A LATTICE GAUGE converts stored index-ordered edge
components to base->tip orientation (the stored direction does not
commute with translation; the covariance battery caught the missing
signs at 1.6e-2 in the first smoke run - fixed to 9.5e-15).
theta* by projected Newton in the invariant sector with a full-gradient
equivariance battery. V_eff classification on raw curves, no fits.

Batteries: per-member covariance (1e-10 gate), block reconstruction
(1e-4), objective consistency (1e-10), analytic-vs-FD gradient (1e-6),
identity theta-independence, trace consistency (recorded), theta gate
with the phase446 absolute-floor convention.

## Verdict: no-saturation persists across mode volumes

modeVolumeVerdict=no-saturation-persists-across-mode-volumes;
anySaturationAnyVolume=false; volumeTrendSeedStable=true. Covariance
9.5e-15 (n=3) / 1.4e-14 (n=4); block reconstruction 2.9e-11; theta
equivariance battery green; runtime ~13 min for both volumes. Every
Einsteinian member remains trivial-origin at 5x and 16x the minimal
mesh's mode count: PHASE443'S MODE-VOLUME LEVER IS DECIDED NEGATIVE -
the one-loop no-saturation verdict is not a small-mesh artifact.
Recorded conventions (invariant rays, positive-mode IR rule, saddle
backgrounds) carry physicistReviewPending. Nothing promoted.

## Named Next

Depends on the verdict: if no-saturation persists across volumes, the
one-loop mode-volume hypothesis (phase443's named lever) is decided
negative and the frontier rests on beyond-one-loop-at-volume, the
physicist review queue, or a source anchor; if saturation appears with
volume, the trend demands larger tori (n=5+, cheap with the block
machinery) and the convention/physicist review before any candidate
language. Nothing promoted either way.

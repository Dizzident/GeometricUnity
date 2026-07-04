# Phase447 Two-Loop Saturation Probe — Design Note (2026-07-03, post-Phase446)

Feasibility scouting delivered after Phase446 closed the RG-improved
potential-fit route. VERDICT: FEASIBLE with existing machinery; no platform
work required; cost does not gate. The one open item is a physics convention
(saddle/negative modes), to be recorded with physicistReviewPending=true per
the Phase442-446 pattern.

## Structure result (resolves the Phase442 tension)

S_B = (1/2)||S_h||^2 with S_h = M . Ad(theta) . F(omega), F = d omega +
(1/2)[omega, omega]:

- In omega (theta fixed): EXACT QUARTIC — the pure-omega block of T4 is a
  CONSTANT tensor and the pure-omega T3 is affine in omega.
- In theta: TRANSCENDENTAL (theta enters only through Ad = exp(ad_theta); for
  su(2) the Rodrigues form in sin/cos|theta|). No finite polynomial degree.
- Phase442's "joint Hessian degree > 2 in the ray parameter" is a
  third-difference test that can only certify greater-than; the
  transcendental theta-dependence exceeds every finite degree — the ">2" is
  the shadow of theta-transcendentality, not a finite joint polynomial.
- Consequence: the exact-polynomial shortcut cannot rescue the Einsteinian
  vertices (FD stencils required), but the IDENTITY control (theta absent,
  pure omega-quartic) has an ENTIRELY EXACT two-loop — the anchor battery
  that validates the Einsteinian estimators.

## Recommended computational route

Per-S_B-eval cost ~50-100 us (from recorded phase446 runtimes). n = 243;
corrected mode counts at the composite points: n_pos ~ 167-198, n_neg ~
31-59, n_zero ~ 15-48 (a deeper saddle than earlier estimates).

Contract onto the positive-subspace propagator G+ = sum_{lambda_i>0}
v_i v_i^T / lambda_i (eigenpairs already computed for the one-loop step):

- figure-eight = (1/8) sum_{ij} T4[v_i,v_i,v_j,v_j]/(lambda_i lambda_j),
  each T4[...] a 9-point mixed 4th-derivative stencil (4 new corners/pair):
  deterministic-exact over all pairs ~72k evals ~ 5 s/point.
- sunset = -(1/12) sum_{ijk} T3[v_i,v_j,v_k]^2/(lambda_i lambda_j lambda_k),
  each T3[...] an 8-point stencil. Deterministic-exact ~9.1M evals ~ 11
  min/point; SOFT-MODE TRUNCATED (K = 60 softest positive modes, physical
  because the 1/(lambda^3) weight is IR-dominated) ~274k evals ~ 20 s/point;
  stochastic Hutchinson cross-check ~0.3 s/point.
- Use UNIT eigenvectors with h ~ 1e-4 and carry 1/lambda analytically (avoid
  the 1/sqrt(lambda) probe-blowup trap).

Recommended: exact figure-eight + soft-mode sunset with a convergence-in-K
battery (K = 20/30/40/60), a stochastic cross-check, and a full-n_pos
deterministic sunset at 1-2 anchor points as the truncation/variance control.
~25 s/point of two-loop work; ~5-7 min per 16-point ray per member-seed;
full phase447 at phase443 scope ~1.5-3 h.

## The physics convention to record (physicistReviewPending)

1. Propagator: the consistent extension of the existing IR rule (one-loop =
   half-sum log lambda over positive modes) is G+ on the positive subspace,
   excluding negative AND zero modes — a defined, recorded object, but a
   POSITIVE-SUBSPACE-PROJECTED two-loop, not the physical one (about a saddle
   the true Gaussian integral is divergent/complex). Flag as loudly as
   phase445/446 flagged scheme dependence.
2. Off-shell omega rays are fine (the 1PI effective potential is defined for
   all field values; 1PI-ness excludes reducible tadpoles, leaving exactly
   figure-eight + sunset). The NEGATIVE MODES are the substantive question.
3. Zero modes excluded at the existing ZeroModeRelTol; the propagator
   soft-mode floor is a recorded design parameter (near-zero modes are
   simultaneously IR-dominant and FD-dangerous).

## Fail-closed gate structure

Precursors: phase442 (degree-lift), phase443 (no one-loop saturation),
phase446 (RG candidate resolved as fit artifact).

Batteries: exact identity control (deterministic-exact vs estimator to
machine level; must show NO two-loop minimum); stochastic-vs-deterministic
agreement on >= 1 Einsteinian point; FD-stencil Richardson stability under
h-variation (LOAD-BEARING: 3rd/4th differences on an O(10^2) one-loop term);
convergence-in-K of the truncated sunset; offset immunity (constant added to
S_B leaves T3/T4 invariant); T3/T4 index-symmetry residuals.

Decision (phase passes on internal consistency regardless of verdict):
V^{2loop}(t) = S_B + V_1loop + V_2loop along rays. CANDIDATE iff a finite
interior minimum appears AND survives the convention/estimator/robustness
batteries AND is absent for the exact identity control. FRONTIER-SHARPENED
iff no minimum (two loops also insufficient). FAIL-CLOSED iff stencils do
not converge, variance cannot decide, the identity control manufactures a
spurious minimum, or the verdict flips under the positive-subspace
convention (record convention-dependent, NOT a candidate — the phase445/446
discipline). Workbench-relative only; no GeV/pole/VEV; no Phase201/Phase256
contract filled.

Anchors: studies/phase443_.../Program.cs (SB ~183, BuildJointHessian ~424,
SolveThetaStar ~208); studies/phase446_.../Program.cs (same machinery);
src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs (EvaluateWithTheta ~435,
LinearizeTheta ~462, the transcendental Ad/MatrixExp path ~414).

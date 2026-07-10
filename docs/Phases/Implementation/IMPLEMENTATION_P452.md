# Implementation P452 - Scalar-Channel Spectroscopy Probe (Task-Force WS2)

Phase452 executes the 2026-07-05 task-force's WS2: the program's FIRST
MEASURED POLE. HMC correlator spectroscopy of two strictly
gauge-invariant scalar interpolators - the local action density
tr(F^2) and the Mode-B invariant Tr(Upsilon dagger Upsilon) - on the
lattice-canonical n=3 torus, with theta sampled by Haar-invariant
per-vertex rotation Metropolis interleaved with omega-HMC (the
phase450 binding conventions), cosh-corrected effective masses ONLY,
and the exact block-spectrum free-field control as the load-bearing
gate (analytic Gaussian correlator from the phase448 momentum blocks;
sampler large-beta column must match within statistics - measured
z = 0.7-1.1).

## Verdict: THE SCALAR CHANNEL IS GAPPED (measured)

a*m_0++ = 2.4352 +- 0.1682 (identity control, 14.5 sigma) and
2.4547 +- 0.1242 (sd2 Einsteinian, 19.8 sigma; O1-vs-O2 interpolator
cross-check 1.04 sigma). terminalStatus =
scalar-channel-spectroscopy-probe-passed-scalar-channel-gapped-measured-
workbench-relative-no-gev. All batteries green (exactness ~1e-15;
zero-mode count 252 = dim ker d EXACTLY; <e^-dH> ~ 1 and virial on all
five columns; N_eff 176-2144). This CONFIRMS the 2026-07-04 review
board's convex/gapped picture BY MEASUREMENT - the quantity the
retired saddle-V_eff arc could never reach. LABEL CAVEAT (binding):
this is the scalar glueball-like gap of THIS action in lattice units -
never m_H; the W/Z/H labels attach only in a Higgs phase.

Limitations (recorded): T=3 gives one informative cosh point (masses
upper-bound-flavored; n=4 adds the second window point at ~3.2x cost);
committed run used a reduced 8000/6000-trajectory budget (78 min)
after the original background run was killed by the harness.

## Named Next

n=4 for the second window point; the charged/neutral vector channels
and GEVP multi-pole analysis await Team A's dynamical scalar (the WS3
Upsilon-portal probe); every mass ratio against this measured gap is
now available anchor-free. Nothing promoted.

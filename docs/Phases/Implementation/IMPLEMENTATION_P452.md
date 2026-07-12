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

## Verdict: scalar-channel-spectroscopy-probe-passed (committed record)

CANONICAL COMMITTED NUMBERS (default-budget 16000/10000 trajectories,
rngSeed 20260705): identity a*m = 2.7132 +- 0.1846 (plateauChi2Dof
null, single window {0} - inconclusive-by-construction as a
measurement); sd2 combined 2.5260 +- 0.0712 (mO1 2.5553 +- 0.0725,
mO2 2.4986 +- 0.0701; O1-vs-O2 interpolator cross-check 0.56 sigma);
exact analytic free gaps 2.5509/2.5320/2.3570; cross-action ratio
sd2/identity = 0.931 +- 0.069 (CROSS-ACTION deliverable class,
0.9 sigma from the free ratio 0.9926 - FREE-FIELD-COMPATIBLE label
binding today). terminalStatus =
scalar-channel-spectroscopy-probe-passed-scalar-channel-gapped-measured-
workbench-relative-no-gev. All batteries green (exactness ~1e-15;
zero-mode count 252 = dim ker d EXACTLY; <e^-dH> ~ 1 and virial on all
five columns). LABEL CAVEAT (binding): this is the scalar
glueball-like gap of THIS action in lattice units - never m_H; the
W/Z/H labels attach only in a Higgs phase.

SUPERSEDED (note per the diagnosis journal, 2026-07-10 reconciliation):
the earlier a*m = 2.4352 +- 0.1682 / 2.4547 +- 0.1242 (quoted at
14.5 / 19.8 sigma) figures came from a never-committed reduced-budget
(8000/6000) env-override run; they are superseded by the canonical
committed default-budget numbers above and are deliberately absent
from the committed output JSONs. The formal machine-checked
attestation of the canonical record is phase459.

Limitations (recorded): T=3 gives one informative cosh point (masses
upper-bound-flavored; n=4 adds the second window point at ~3.2x cost);
the committed run is the default-budget 16000/10000-trajectory
regeneration from the full generator pass (the earlier never-committed
run used a reduced 8000/6000-trajectory budget after the original
background run was killed by the harness).

## Named Next

n=4 for the second window point; the charged/neutral vector channels
and GEVP multi-pole analysis await Team A's dynamical scalar (the WS3
Upsilon-portal probe); every mass ratio against this measured gap is
now available anchor-free. Nothing promoted.

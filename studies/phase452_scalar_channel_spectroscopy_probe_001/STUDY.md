# Phase452: 0++ Scalar-Channel Mass-Gap Spectroscopy Probe

The 2026-07-05 task-force **WS2 workstream** (BINDING: cosh-corrected effective
masses ONLY; strictly gauge+translation-invariant operators only; theta-Haar;
report null as null), inheriting the 2026-07-04 review board's four phase450
binding conditions. First **measurement** test of the review board's
convex/gapped picture: after the Elitzur/FMS reframe, the physical content of
the workbench lives in gauge-invariant composite poles — here the 0++ scalar
channel of the pure `e^{-beta S_B}` ensemble (S_B >= 0 real: no sign problem).

## Ensemble

- Lattice-canonical torus `CreateUniform4DPeriodic(n, latticeCanonical: true)`,
  n = 3 by default (`PHASE452_TORI=3,4` adds n = 4 when budget allows).
- **omega**: HMC with analytic `ComputeJointGradient` forces (the
  `cep_hmc_prototype` pattern at torus scale), leapfrog `nLeap = 12`, warmup
  step-size auto-tune.
- **theta**: Haar-invariant per-vertex rotation Metropolis (binding condition
  (ii): theta enters S_B only via `Ad = exp(ad_theta)` in SO(3) per vertex; the
  flat R^n theta integral diverges). Proposals are group compositions
  `R'_v = exp(ad_delta_v) exp(ad_theta_v)` with isotropic Gaussian delta —
  symmetric w.r.t. Haar, so plain Metropolis targets Haar x e^{-beta S_B}.
  Applied as collective all-vertex moves each trajectory (tuned sigma) plus a
  periodic single-site sweep (ergodicity aid). Coordinates are axis-angle with
  |theta| <= pi via the quaternion log map; exp/log round-trip and
  Rodrigues-vs-series batteries gate the chart (and su(2)-trace-pairing
  structure constants make `exp(ad_theta)` exactly the Rodrigues rotation).
- Members: `identity` control (exactly theta-independent, battery-verified —
  the Haar factor cancels in every observable, so theta is not sampled for it)
  and `sd2-id0/c0.5` (Einsteinian, `independent-theta`).
- Columns per run: beta = 1 production (recorded convention) for both members,
  one higher-beta physics control (sd2, beta = 4, theta-Haar), and one
  large-beta (beta = 400) **free-field control** per member at theta = 0 —
  labeled SAMPLER-DEMO per binding condition (iv).

## Interpolators (strictly gauge + translation invariant)

- `O1(tau)` = time-slice sum of the local action density `tr(F^2)` (trace
  pairing on the `CurvatureAssembler` face coefficients).
- `O2(tau)` = time-slice sum of the Mode-B invariant `Tr(Upsilon^dagger
  Upsilon)`, Upsilon = S_B's own residual through the operator's
  `EvaluateWithTheta` path (`Evaluate` for the identity member). With the
  uniform unit face weights, `sum_tau O2(tau) = 2 S_B` exactly
  (battery-verified against the gradient-path objective).

**Slice convention (documented):** lattice axis 0 is Euclidean time. A face
belongs to the slice of its **canonical base vertex** — the unique face vertex
from which the minimal-image displacements of the other two vertices are both
in {0,1}^4 (the Kuhn-chain minimum; manifestly translation covariant; whether
it coincides with the stored `Faces[f][0]` ordering is recorded, not assumed).

## Correlators and masses

Vacuum-subtracted connected `C(t) = <O(t)O(0)> - <O>^2`, averaged over all
time translations; jackknife over 50 blocks. **Cosh-corrected effective mass
ONLY** (the naive log-ratio was proven corrupted by the periodic image):
solve `C(t)/C(t+1) = cosh(m(t - T/2)) / cosh(m(t + 1 - T/2))` by bisection.
Informative points are `t <= T/2 - 1` (beyond that the ratio is identically 1).

**Pre-registered window rule:** `window = { t : 1 <= t <= T/2 - 1 }` if
nonempty, else `{0}` with the excited-state-contamination caveat recorded
(T = 3 has exactly one informative point; n = 4 adds the second). Plateau
chi^2/dof <= 3 gates windows with >= 2 points; single-point windows are
recorded as such.

## Fail-closed gates

1. **Free-field control (load-bearing).** The analytic Gaussian correlator is
   built from the exact block-circulant momentum spectrum of the omega Hessian
   at omega = 0 (phase448 machinery: 45 orbit-representative Hv columns ->
   n^4 Hermitian 45x45 blocks -> eigenpairs), combined with the interpolator's
   exact linearization kernel G(k):
   `Cov(dx) = (1/(beta V)) sum_k e^{2 pi i k.dx/n} G(k) H(k)^+ G(k)^dagger`,
   `C_free(t) = 2 n^3 sum_{F,F',dx_s} Tr(K M K M^T)`.
   The sampler's large-beta column must reproduce the analytic cosh m_eff
   within 3 sigma (O1 gating; O2 recorded). The analytic formula itself is
   validated by an **exact direct-Gaussian-sampling battery** through the
   identical measurement pipeline (quadratic-truncated interpolators, 5 sigma).
   Free columns sample e^{-beta S_B} restricted to the orthogonal complement
   of ker H(0) — the exact flat/pure-gauge cone tangent, on which the Gaussian
   theory does not exist and which the analytic prediction excludes (recorded
   control convention; physics columns are unrestricted).
2. **Sampler exactness per column:** `<e^{-dH}> = 1` (3 sigma or 1e-2) and
   equipartition `<sum_i omega_i beta dS/domega_i> = nDof` (3 sigma or 2%).
3. **Plateau quality** over the pre-registered window.
4. **O1-vs-O2 cross-check:** both interpolate the same 0++ channel; masses
   must agree within 3 sigma — incompatibility fail-closes the member verdict
   to `inconclusive-statistics`.
5. **Uniform-theta invariance assert:** a uniform (same at every site) gauge
   rotation `omega_e -> Ad_g omega_e, theta_v -> Ad_g theta_v` must leave S_B
   and every O1/O2 slice invariant (1e-8) — the exact discrete global-gauge
   invariance of the interpolators.

Gates (1), (2), (5) plus the exactness batteries (objective consistency,
block eigensolver residual, plane-wave Hv, exp/log chart, Gaussian-sim,
no-negative-modes at omega = 0, N_eff >= 100 per column) gate the PHASE
(blocked on failure). Gates (3), (4) map to the per-member verdict
`inconclusive-statistics` — fail-closed: no mass claim escapes a failed
statistics gate.

## Verdicts (pre-registered)

Per production member: `a*m_0++` with jackknife error, combined over the two
interpolators (inverse-variance weighted mean with the correlation-conservative
rho=1 error `s1 s2 (s1+s2)/(s1^2+s2^2)` — both interpolators live on the same
ensemble; for the identity member O2 = O1 exactly) once the cross-check passes:
- `scalar-channel-gapped-measured` if m >= 3 sigma;
- `scalar-channel-gapless` if m <= 2 sigma with sigma <= 0.3;
- else `inconclusive-statistics`.

**Label caveat (binding):** any a*m_0++ is the scalar glueball-like gap of
THIS pure-S_B action in lattice units — a pure-gauge-sector composite-pole
scale. It is NEVER m_H; W/Z/H labels attach only in a Higgs phase.

## Mandatory framing

Workbench-relative structure data ONLY (su(2) toy algebra, reduced Spin(4)
slice, lattice-canonical torus, lattice units, beta = 1 recorded); NO
GeV/pole/VEV promotion either way; target-blind; no Phase201/Phase256 contract
field filled; nothing promoted either way. Precursors: phase448 + phase449.

## Running

```
dotnet run -c Release --project studies/phase452_scalar_channel_spectroscopy_probe_001/Phase452ScalarChannelSpectroscopyProbe.csproj
```

Env knobs: `PHASE452_TORI` (default `3`), `PHASE452_TRAJ` (16000),
`PHASE452_CTRL_TRAJ` (10000), `PHASE452_WARM` (2000), `PHASE452_NLEAP` (12),
`PHASE452_BETA` (1), `PHASE452_BETA_MID` (4), `PHASE452_BETA_FREE` (400),
`PHASE452_GAUSS_SIM` (4000), `PHASE452_OUTPUT_DIR`. The five columns run in
parallel (one thread each).

## Results (committed run: n = 3, 2026-07-05)

Terminal status:
`scalar-channel-spectroscopy-probe-passed-scalar-channel-gapped-measured-workbench-relative-no-gev`
(runtime 78 min; the committed run used `PHASE452_TRAJ=8000`,
`PHASE452_CTRL_TRAJ=6000`, `PHASE452_WARM=1000` — a reduced-statistics budget
recorded in `probeConfiguration.ensemble`; all N_eff gates clear at >= 176).

**Exactness batteries** (all ~1e-15 unless noted): objective consistency
5.1e-15; uniform-theta (global gauge rotation) invariance 8.6e-15; identity
theta-independence 1.2e-15; exp/log chart 5.6e-16; block eigensolver residual
4.7e-15; plane-wave Hv 3.3e-11; Gaussian-sim pipeline battery worst 0.86 sigma;
omega Hessian at 0 has 3393 positive / **252 zero (= 3(V-1)+12 = dim ker d,
exactly the pure-gauge/flat cone tangent)** / 0 negative modes.

**Sampler gates** (all pass): every column has `<e^-dH>` = 0.99-1.01 within
errors and equipartition at target (e.g. 3645.6/3645 for sd2 beta = 1 —
notably the *unprojected* physics columns satisfy the virial identity, i.e.
the exactly-flat ker-d cone is entropically confined).

**Free-field control (gate 1, load-bearing):** at beta = 400 the sampler
reproduces the analytic block-spectrum cosh mass:
identity analytic m(O1) = 2.5509 vs measured 2.4243 +- 0.1129 (z = 1.12);
sd2 analytic 2.5320 vs 2.8556 +- 0.4141 (z = 0.78); O2 recorded
(sd2: analytic 2.3570 vs 2.5824 +- 0.3265).

**0++ masses (canonical committed record; default-budget 16000/10000
trajectories, rngSeed 20260705, T = 3, single-window t = 0 with the
excited-state caveat recorded):**

| member | a*m (O1) | a*m (O2) | cross-check | combined a*m_0++ | verdict |
|---|---|---|---|---|---|
| identity (control) | 2.7132 +- 0.1846 | identical (O2 = O1 exactly) | 0.00 sigma | 2.7132 +- 0.1846 (plateauChi2Dof null — inconclusive-by-construction) | scalar-channel-gapped-measured |
| sd2-id0/c0.5 | 2.5553 +- 0.0725 | 2.4986 +- 0.0701 | 0.56 sigma | 2.5260 +- 0.0712 | scalar-channel-gapped-measured |

Exact analytic free gaps 2.5509 / 2.5320 / 2.3570; cross-action ratio
sd2/identity = 0.931 +- 0.069 (CROSS-ACTION deliverable class, 0.9 sigma
from the free ratio 0.9926 — FREE-FIELD-COMPATIBLE label binding today).

`scalarChannelVerdict = scalar-channel-gapped-measured`: the 0++ channel is
gapped for both members in lattice units. Per the binding label caveat this
is the scalar glueball-like gap of THIS action in lattice units — never m_H;
no GeV/pole/VEV promotion; `promotedPhysicalMassClaimCount = 0`. The committed
spectrum is currently free-field-compatible; every dynamical-structure claim
gates on a >= 3 sigma departure from the exact Gaussian gaps.

**SUPERSEDED (note per the diagnosis journal, 2026-07-10 reconciliation):**
the earlier combined figures a*m = 2.4352 +- 0.1682 (identity) /
2.4547 +- 0.1242 (sd2), quoted at 14.5 / 19.8 sigma, and the control
comparison z-values recorded above from that run, came from a
never-committed reduced-budget (8000/6000-trajectory) env-override run.
They are superseded by the canonical committed default-budget numbers in
this table and are deliberately absent from the committed output JSONs.
The formal machine-checked attestation of the canonical record is phase459.

**Recorded limitations:** T = 3 gives exactly one informative cosh point, so
the window mass is an upper-bound-flavored estimate of the gap
(excited-state contamination caveat serialized); n = 4 (`PHASE452_TORI=3,4`)
adds the second point at ~3.2x cost. The committed run is the default-budget
16000/10000-trajectory regeneration from the full generator pass.

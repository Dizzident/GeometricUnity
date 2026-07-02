# Phase440: Coupled Background-Condensate Fixed-Point Probe

**The final link of the candidate chain 430 → 439 → 438.**

Three prior results set up the question this phase decides:

- **Phase430/435/436/437** proved the one-loop landscape *alone* has **no finite
  minimum** along the `lambda_8` background `t8`: the net (bosonic + fermionic)
  large-`t8` log slope is negative (fundamental `-4` per site, derived
  hypercharge `-40` per site), driven by fermion modes that stay light as `t8`
  grows, so `W` runs away.
- **Phase438** showed the Gross-Neveu-style gap equation **does** generate a
  dynamical condensate scale `Sigma`.
- **Phase439** showed a `lambda_8` background **steers** that condensate into an
  su(2)×u(1)-aligned per-color pattern (`Sigma_1 = Sigma_2 != Sigma_3`).

**The question this phase decides:** treat background `t8` and condensate
`Sigma` as a **joint variational system**. Does the condensate's gapping of the
fermion modes **saturate** the fermionic log-runaway and produce a finite
**interior** minimum `(t8*, Sigma*)` — i.e. does the candidate chain close into
full internal self-consistency?

## Construction

2D `L × L` lattice, `L ∈ {4, 8, 16}`; 4-dim spinors; naive central-difference
Dirac; su(3) Gell-Mann; contents fundamental (1 copy) and derived-4x (4 copies);
8 log-spaced couplings `g2 ∈ [0.3, 8]`. Doubler exclusion (momenta with
`s1²+s2² ≤ 1e-12`) with the **same excluded set at every `t8`** for **both** the
bosonic and fermionic sums, so the `t8=0` limit is exactly Phase438/439.

**Joint free energy per volume** (union of the Phase430 bosonic workbench model
and the Phase439 free-diagonal fermionic gap functional, each relative to
`(t8=0, Sigma=0)` so `W_total(0,0)=0`):

```
W_total(t8, Sigma_vec)
  = W_B(t8)
    + sum_c [ Sigma_c²/(2 g2)
              - copies*(Ns/2)/Vol * sum_k' ( log(eps_c(k,t8)² + Sigma_c²)
                                             - log(eps_c(k,0)²) ) ]
W_B(t8) = (1/(2 Vol)) sum_k' sum_i 2 * log( (eps_k² + t8² m_i²) / eps_k² )
```

- `m_i²` = eigenvalues of `-ad_{lambda_8/2}²` on su(3) = **four 0, four 3/4**
  (only the four broken modes contribute); 2 polarizations.
- `eps_c(k,t8)² = (s1 + t8 u_c)² + (s2 + t8 u_c)²`, `u_c` = eigenvalues of
  `lambda_8/2 = {1/(2√3), 1/(2√3), -1/√3}`.

Because the fermionic term decouples per color and `W_B` is `Sigma`-independent,
the joint-optimal `Sigma_c` at fixed `t8` is exactly the per-color gap solution
`1 = g2·copies·(Ns/Vol) Σ_k' 1/(eps_c² + Sigma_c²)`.

## Result — honest negative: the chain does NOT close

**No interior fixed point exists anywhere.** Across all 48 (lattice, content,
coupling) configurations the joint minimum is **trivial** (`t8*=0`, condensate
as in Phase438) at strong coupling and **runaway** (`t8*` escapes to the grid
boundary) at weak coupling — **29 trivial, 19 runaway, 0 interior**.

- **Saturation is real but insufficient.** At fixed `Sigma` the gap shifts the
  fermionic IR cutoff `eps → sqrt(eps² + Sigma²)`, depressing the moderate-`t8`
  runaway slope (Σ=0 window slope `-2.65` → Σ=4 window slope `-0.42`) and
  delaying the onset to `t8 ~ Sigma`. **But** the large-`t8` slope resumes toward
  the same integer count (`-7.28` at Σ=4), because at large `t8` the background
  gaps out the very IR modes that drive condensation — the joint-optimal `Sigma`
  → 0 there. The asymptotic net slope is therefore unchanged and negative.
- **Runaway persists** in the weak-coupling window: the `dW/dt8` decomposition at
  the boundary is bosonic `> 0`, fermionic `< 0`, net `< 0` (fundamental
  `-0.50`, derived `-4.80` at `t8=7.9`, L=8).
- **Background does not self-generate**: at strong coupling the condensate wins
  and pins `t8*=0`.

For the derived content in the runaway window, the su(2) colors still condense
at the boundary (`Sigma_1 = Sigma_2 > 0`) while color 3 is gapped out
(`Sigma_3 = 0`) — the Phase439 alignment survives, but with `t8` running away.

## Batteries (all pass)

| Battery | Residual |
|---|---|
| γ anticommutation exact | `0` |
| bosonic masses {four 0, four 3/4} | exact |
| **Phase430 cross-anchor** (fermion −192/−768, boson +128, net −64/−640, **−40 per site**) | `3.1e-4` |
| `t8=0` reproduces Phase438 gap (singlet + hyper) | `0` |
| closed form vs dense L=4 Dirac **with background** | `1.6e-13` |
| `Sigma_1 = Sigma_2` symmetry | `0` |
| verified descent (final W ≤ grid W) | all configs |
| reduced-gradient consistency at the minima | `4.5e-4` |

## Verdicts (top-level booleans)

- `coupledBackgroundCondensateFixedPointProbePassed = True` (passes on internal
  consistency regardless of outcome)
- `condensateSaturatesFermionicRunaway = True` (quantified: delays, does not stop)
- `jointFixedPointExists = False`, `jointFixedPointIsInterior = False`,
  `backgroundSelfGenerates = False`
- `runawayPersistsInCoupledSystem = True`
- `jointMinimumIsTrivialOrRunaway = True`

## Honest boundaries (fail-closed)

`t8` is a recorded candidate parameter, **not** dynamically derived (this phase
decides it does **not** self-generate); the gap equation is mean-field; the
four-fermion coupling normalization and the bosonic mode-mass model are recorded
workbench conventions; all `Sigma`/`t8` scales and ratios are candidate-only.
`scaleRatiosAreCandidateOnly = bosonicSectorIsWorkbenchModel =
gapEquationIsMeanFieldApproximation = couplingNormalizationIsRecordedConvention =
noGevPromotion = True`. Nothing is promoted to GeV; every `canFill*` /
`routePromotes*` / `route*` field is `False`; no Phase201 or Phase256 field is
filled.

## Outputs

- `output/coupled_background_condensate_fixed_point_probe.json`
- `output/coupled_background_condensate_fixed_point_probe_summary.json`

Runtime ~0.8 s. Precursors: phase430
(`netOneLoopDirectionSelectionProbePassed && noFiniteMinimumOnRays`), phase438
(`selfConsistentCondensateGapEquationProbePassed &&
dynamicalScaleGenerationObserved`), phase439
(`gapEquationLambda8BackgroundChannelSteeringProbePassed &&
backgroundInducesChannelSteering && dynamicalMassPatternAlignsWithSu2U1`).

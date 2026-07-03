# Phase445: RG-Improved Joint Effective-Potential Probe

The named NO-PLATFORM alternative to Phase444 (mode volume was tooling-blocked) and
the RG-improvement follow-up to Phase443 (which found **no one-loop saturation** on
the minimal `CreateUniform4D(1)` mesh — trivial-origin for every Einsteinian member).

## The question

Phase443 measured the **fixed-coupling one-loop** joint effective potential and found
it has no interior minimum. The standard Coleman–Weinberg mechanism, however, lives in
the **RG-improved** potential: the one-loop logs are resummed into a **running effective
coupling**, and a minimum forms where the running balances the tree term — a
structurally different question from the fixed-coupling landscape. **Does RG improvement
of the measured joint one-loop structure produce saturation (an interior minimum) that
the unimproved potential lacks?**

## Construction (honest workbench version — every convention recorded)

0. **Recompute** the Phase443-style `V_eff(t)` rays with the **hardened machinery**
   (verbatim Newton `theta*`-stationarity solver, joint FD Hessian, Jacobi eigenvalues,
   IR zero-mode convention) on `CreateUniform4D(1)`:
   `V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i(H_joint > 0)`.
1. **Extract the effective quartic.** `S_B` is an exact quartic polynomial in the ray
   parameter `t`; fit it locally to `{t^2, t^3, t^4}` over a sliding window →
   `lambda_tree(t)` (its `t^4` coefficient) plus the **measured subleading** terms
   `(a t^2 + b t^3)`. Fit the one-loop correction `V_1loop(t) = V_eff - S_B` locally to
   `c(t) t^4` (the leading-log resummation target) → `delta_lambda(t)`.
   `lambda_eff(t) = lambda_tree(t) + delta_lambda(t)`. The **workbench beta function** is
   `beta(t) = d lambda_eff / d ln t` (central difference over window centers) — **MEASURED
   from the potential, not assumed**.
2. **RG-improve:** `V_RG(t) = lambda_eff(t) * t^4 + [measured subleading a t^2 + b t^3]`.
3. **Saturation analysis** on `V_RG` per member: interior-minimum search with **verified
   descent** (strict local minimum — both window neighbours strictly above); classification
   `{trivial-origin / interior-finite / runaway / interior-inflection-not-verified}`; if
   interior, the dimensionless `t*/latticeScale` ratio recorded **candidate-only**.
4. **Control discipline.** Identity control `{id0, none, trivial}`: `theta` absent,
   `theta*=0` exact, `S_B` pure quartic, one-loop **positive** (the old no-go). Its
   `lambda_tree` must be window-**constant** (pure quartic) and its `V_RG` must **not**
   develop a spurious interior minimum. A spurious control minimum means the RG scheme
   manufactures artifacts — it **kills the treatment verdicts and blocks the phase**.
5. **Scheme honesty.** The window width, fit basis, and resummation prescription are
   **workbench conventions**. The window width is scanned over `{3, 5, 7}` and the verdict
   is required to be **scheme-stable** (`verdictSchemeStable`); a scheme-dependent verdict
   is recorded as exactly that. Per-window fit quality is recorded (uncentered `R^2` for
   the through-origin one-loop `t^4` fit; centered `R^2` for the tree fit).

## Setup

`CreateUniform4D(1)` (`V=16, E=65, F=110, C=24`, `nOmega = 195`, `nTheta = 48`, joint DOF
`243`), `su(2)` with the positive-definite trace pairing, `Upsilon = S_h` (trivial
torsion). Members: identity control + `sd2-id0/c0.5` + `asd2-id0/c0.5` (independent-theta).
Uniform `t`-grid over `(0, TMax]` (`GridN` points; a uniform grid is required so the
window-width scan is apples-to-apples). Two random `omega`-ray seeds (seed stability).

## Batteries

- **theta\*-stationarity gate** — the Phase443 hardened solver reused **verbatim**,
  relative residual `<= 1e-8`; identity `theta*=0` exact.
- **LinearizeTheta FD-vs-analytic** — `<= 1e-6`.
- **identity-control beta consistency** — `lambda_tree` window-constant (relative
  variation `<= 1e-3`) and `lambda_eff > 0` (no sign flip that would fake a minimum).
- **no-spurious-minimum control** — identity never classifies `interior-finite`.
- **scheme stability** — Einsteinian classification agrees across window widths.
- **seed stability** — classification agrees across ray seeds.
- **fit-quality floor** — min uncentered one-loop `R^2` recorded (soft floor).

## Results (filled from the committed run)

<!-- RESULTS_PLACEHOLDER -->

## Mandatory framing (verbatim intent)

Any scale is a **workbench-relative candidate ONLY** — `su(2)` toy algebra on the reduced
Spin(4) slice, lattice units, one loop; the RG prescription is a **workbench convention
pending physicist review** (`rgPrescriptionIsWorkbenchConvention = true`,
`physicistReviewPending = true`; the physicist-4d session expired). **NO GeV/pole/VEV
promotion** (`scaleIsWorkbenchRelativeCandidateOnly = true`, `noGevPromotion = true`). A
no-saturation or scheme-dependent result is a legitimate frontier-sharpening outcome. The
phase **PASSES on internal consistency** (precursors + hardened theta solve + control
discipline + honesty batteries) **regardless of how the saturation verdict falls**.

## Fail-closed

Target-blind construction (`targetBlindConstructionHash` recorded); reduced-spin4-slice
only (the six verbatim recorded-boundary keys: `definition81Scope = reduced-spin4-slice`,
`ambientSevenSevenRealized = internalGaugeContentRealized = weldRealized = false`,
`canFillPhase201WzContract = canFillPhase256Contract = false`). No scale/pole/GeV lineage;
no Phase201/Phase256 contract field filled; `sourceContractApplicationAllowed =
phase201TemplateMutated = false`; `acceptedContractFieldCount = 0`. Nothing is promoted
either way.

Precursors: Phase443 (`jointEffectivePotentialSaturationProbePassed &&
einsteinianLogSaturationObserved === false`) and Phase444 (`phase444Passed`).

## Run

```bash
dotnet run -c Release --project studies/phase445_rg_improved_joint_potential_probe_001/Phase445RgImprovedJointPotentialProbe.csproj
```

Ends `Passed=True`, 0 warnings. Outputs `output/rg_improved_joint_potential_probe.json`
and `_summary.json`.

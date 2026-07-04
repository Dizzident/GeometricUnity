# Phase446: RG Scheme-Dependence Resolution Probe

The named follow-up to Phase445, which found **the first interior finite minima in the
program's history** under RG improvement (`t* ~ 1.5–2.25`, Einsteinian members) but
recorded them as a **scheme-dependent candidate** because the classification flipped at
the widest fit window (`verdictSchemeStable = false`).

## The question

Are the Phase445 interior minima genuine RG-resummation structure, or an artifact of the
fit prescription?

## Pre-registered artifact mechanism under test

Phase445 fit the one-loop correction `V_1loop(t)` to the single-basis model `c t^4`. The
Einsteinian one-loop curves are approximately a **deep negative constant** (their `t -> 0`
limit) plus a slow log-like rise. A `t`-independent constant is pure vacuum-energy
normalization — it **cannot** move the minimum of the true `V_eff` — yet it completely
controls the fitted `c`: fitting `c t^4` through "constant + slow rise" leaks the constant
into `c`, forces the reconstructed `V_RG = c(t) t^4` to `0` at `t -> 0`, and manufactures
an interior dip for **any** deeply negative, slowly rising curve. The identity control is
structurally **blind** to this artifact class because its one-loop curve is positive. A
prototype replay of the Phase445 prescription on the committed Phase443 raw rays confirmed
the mechanism (constant subtraction kills every interior minimum; enriched bases give
chaotic classifications; the constant-immune direct measurement finds a negative log
coefficient). This phase decides it with the full recomputed machinery, fail-closed.

## Construction (four decisive arms)

0. **Recompute** the `V_eff(t)` rays with the **verbatim** Phase443/445 hardened machinery
   (Newton `theta*`-stationarity, joint FD Hessian, Jacobi eigenvalues, IR zero-mode
   convention) on `CreateUniform4D(1)`, with the **same members, seeds, and ray
   directions** as Phase445 (same `RngSeed`), on a **denser** uniform grid (`GridN = 48`,
   `TMax = 3`) whose `GridN/16`-strided subset is **exactly** the Phase445 16-point grid.
1. **Replication arm** — rerun the verbatim Phase445 prescription (one-loop basis `{t^4}`,
   windows `{3,5,7}`) on the even subset; compare classifications against the recorded
   Phase445 verdicts (parsed from its committed summary).
2. **Offset-invariance arm** — repeat with the one-loop curve shifted by constants
   (subtract its smallest-`t` value; subtract its mean; add `+1000`). The argmin of the
   raw `V_eff` is **exactly** invariant under every such shift (asserted as a battery);
   any classification change under a constant shift is an unphysical normalization
   artifact of the fit.
3. **Fit-basis-menu arm** — repeat with enriched one-loop bases (`{1, t^4}`,
   `{1, ln t, t^4}`, and the canonical CW form `{t^4, t^4 ln t}`) across windows
   (`{3,5,7}` on the even subset; `{3,5,7,9,13}` on the dense grid), raw and
   constant-subtracted. Classification stability across the full menu is the honest
   measurement of fit-scheme dependence. The identity control's behavior under enriched
   bases is **recorded** (a manufactured control minimum under some scheme is direct
   evidence of prescription fragility).
4. **Direct (constant-immune) arm** — the log-derivative operator `L = t d/dt`
   annihilates constants **exactly**. Measure `g(t) = dV_1loop/d ln t` by midpoint
   differences on the dense grid, **jump-aware** (only intervals with unchanged Hessian
   mode counts enter; excluded intervals are counted), then extract the genuine CW log
   coefficient `cL` from the L-image basis `{1, 2t^2, 3t^3, 4t^4, 4 t^4 ln t + t^4}` —
   the constant column is the L-image of `s ln t`, the known fixed-coupling one-loop
   asymptotic; without it that content aliases into the polynomial columns and biases
   `cL` (verified on a synthetic pure-log curve, where omitting it turns a true
   `cL = 0` into a spurious `-22.5`). The CW resummed potential `(c4 + cL ln t) t^4`
   has an interior minimum **only if `cL > 0`** with the implied `t*` in range. An
   **offset-immunity battery** re-derives `cL` with `V_1loop + 1000` and requires
   agreement to `1e-9` relative; fit quality is gated on **centered** `R^2` (with an
   exact-fit escape hatch), not the vacuous through-origin uncentered `R^2`.

5. **Synthetic artifact-sensitive control** — the identity control is structurally
   blind to the constant-leakage artifact (its one-loop is positive), so a hand-built
   curve with the member shape — positive-definite tree `0.05 t^2 + 0.006 t^4` plus
   one-loop `-150 + 40 ln t` — is added. It is **provably monotone increasing**
   (`dV/dt = 2at + 4 lam t^3 + s/t > 0`), so it has no interior minimum by
   construction. Any scheme that classifies it `interior-finite` is manufacturing
   minima (recorded evidence); if the **direct arm** supports a minimum on it, the
   direct arm itself is broken and the phase **fail-closes**.

## Verdict logic

The Phase445 candidate **survives scheme control** only if the interior minima persist
under constant subtraction **and** across the full basis menu **and** the direct
constant-immune log coefficient supports a minimum. Otherwise the minima are resolved as
a **fit-normalization artifact** (or **grid-fragile** if the replication itself fails on
the refined ray data), and the frontier sharpens honestly back to the named levers
(mode-volume unlock projects, beyond-one-loop structure, or a source anchor). The phase
**passes on internal consistency regardless of which way the resolution falls**.

## Setup

`CreateUniform4D(1)` (`V=16, E=65, F=110, C=24`, `nOmega = 195`, `nTheta = 48`, joint DOF
`243`), `su(2)` with the positive-definite trace pairing, `Upsilon = S_h` (trivial
torsion). Members: identity control + `sd2-id0/c0.5` + `asd2-id0/c0.5` (independent-theta),
same order/RNG as Phase445. Two random `omega`-ray seeds.

## Batteries

- **theta\*-stationarity gate** — Phase443 hardened solver verbatim, relative residual
  `<= 1e-8`; identity `theta* = 0` exact.
- **LinearizeTheta FD-vs-analytic** — `<= 1e-6`.
- **identity-control beta consistency** — `lambda_tree` window-constant and
  `lambda_eff > 0` under the verbatim scheme.
- **no-spurious-minimum control (verbatim scheme)** — identity never `interior-finite`
  under the Phase445 prescription (blocker; its menu behavior is recorded evidence, not a
  blocker).
- **raw-argmin offset invariance** — exact, asserted for every probed constant.
- **direct offset-immunity** — `cL` agrees under a `+1000` shift (`<= 1e-9` relative).
- **direct interval floor** — `>= 10` jump-free intervals per member/seed (the
  5-parameter L-image fit needs real residuals, not interpolation).
- **direct fit quality** — centered `R^2 > 0` or an essentially exact fit.
- **synthetic-control monotonicity + direct-arm cleanliness** — the provably monotone
  synthetic stays monotone on the grid, and the direct arm must NOT support a minimum
  on it (blocker).

## Results (filled from the committed run)

Terminal status:
`rg-scheme-dependence-resolution-probe-passed-phase445-minima-resolved-as-fit-normalization-artifact-frontier-sharpened`
(`resolutionKind = fit-normalization-artifact`, seed-stable, runtime ~15 min).

- **Replication**: `replicationMatchesPhase445 = true` — the Phase445 classifications
  reproduce exactly on the strided 48-point recomputation (interior-finite at w=3,5 and
  runaway at w=7 for both Einsteinian members; identity trivial-origin everywhere).
- **Offset invariance**: the raw `V_eff` argmin is exactly shift-invariant (asserted);
  the verbatim fitted scheme is offset-sensitive, and
  `constantSubtractionKillsInteriorMinima = true` at every window on both grids.
- **Fit-basis menu**: `fitSchemeClassificationIrreducible = true`; no member survives
  the menu; `identityEnrichedSchemeManufacturesMinimum = true` — even the identity
  control receives a manufactured minimum under some enriched scheme.
- **Synthetic control**: the verbatim scheme manufactures an interior minimum on the
  provably-monotone synthetic at every window; constant subtraction kills it; the
  direct arm stays clean on it.
- **Direct (constant-immune)**: admissible (30–40 usable intervals per member;
  offset-immunity exact; centered-`R^2` gate met). `sd2-id0/c0.5` `cL = -0.79`
  (nonpositive — no minimum possible); `asd2-id0/c0.5` `cL = +4.45` with implied
  `t*` far outside the grid; identity `cL = +20.0`, `t*` out of range.
  `directRgMinimumSupported = false`.
- **Theta gate**: all points pass relative `<= 1e-8` or the `1e-10` absolute-gradient
  floor (2 smallest-t points via the floor; max relative `2.8e-8` at `t = 0.1875`);
  identity `theta* = 0` exact; LinearizeTheta FD residual `2.2e-9`.

**Resolution**: the Phase445 interior minima are normalization leakage of the deep
negative one-loop constant into the `{t^4}` fit basis. `einsteinianRgSaturationObserved
= false`, `candidateSurvivesSchemeControl = false`. The RG-improved potential-fit route
on the minimal mesh is **closed**; the frontier returns to the Phase444 unlock projects,
genuine beyond-one-loop structure, or a source anchor.

## Mandatory framing (verbatim intent)

All scales are **workbench-relative candidate data ONLY** — `su(2)` toy algebra on the
reduced Spin(4) slice, lattice units, one loop; every RG prescription probed here is a
**workbench convention pending physicist review**
(`rgPrescriptionIsWorkbenchConvention = true`, `physicistReviewPending = true`). **NO
GeV/pole/VEV promotion either way** (`scaleIsWorkbenchRelativeCandidateOnly = true`,
`noGevPromotion = true`). An artifact or unresolved outcome is a legitimate
frontier-sharpening result; a survives-scheme-control outcome would be a
workbench-relative candidate only.

## Fail-closed

Target-blind construction (`targetBlindConstructionHash` recorded); reduced-spin4-slice
only (the six verbatim recorded-boundary keys: `definition81Scope = reduced-spin4-slice`,
`ambientSevenSevenRealized = internalGaugeContentRealized = weldRealized = false`,
`canFillPhase201WzContract = canFillPhase256Contract = false`). No scale/pole/GeV lineage;
no Phase201/Phase256 contract field filled; `sourceContractApplicationAllowed =
phase201TemplateMutated = false`; `acceptedContractFieldCount = 0`. Nothing is promoted
either way.

Precursors: Phase443 (`jointEffectivePotentialSaturationProbePassed &&
einsteinianLogSaturationObserved === false`) and Phase445
(`rgImprovedJointPotentialProbePassed && verdictSchemeStable === false &&
einsteinianRgSaturationObservedAny === true`).

## Run

```bash
dotnet run -c Release --project studies/phase446_rg_scheme_dependence_resolution_probe_001/Phase446RgSchemeDependenceResolutionProbe.csproj
```

Ends `Passed=True`, 0 warnings. Outputs `output/rg_scheme_dependence_resolution_probe.json`
and `_summary.json` (including the raw recomputed ray data under `rayData` so later phases
can post-process without recompute).

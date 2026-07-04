# Phase448: Torus Mode-Volume Saturation Probe

The physics study both phase444 unlock projects were built for, answering the
question Phase444 left `undetermined-tooling-blocked`: **does the Phase443
one-loop no-saturation verdict change when the mode volume grows?**

## The two unlocks used together

- **Lattice-canonical conventions** (commit `82d43559`): the discrete action on
  `CreateUniform4DPeriodic(n, latticeCanonical: true)` is exactly
  translation-covariant (measured 7.8e-15).
- **Adjoint/joint-gradient path** (commit `7a7e397d`): O(mesh) analytic
  gradients and Hessian-vector products (~ms).

On a **translation-invariant** background (the constant-field Coleman–Weinberg
analogue), covariance makes the joint (ω,θ) Hessian **block-circulant** over
(Z_n)⁴: its entire spectrum is determined by the **48 orbit-representative
columns** (15 minimal-image edge-displacement types + 1 vertex type, × su(2)),
each one Hv product, assembled by DFT into n⁴ Hermitian 48×48 momentum blocks
({k,−k} pairs eigensolved via the real symmetric embedding). The 3888-dim (n=3)
and 12288-dim (n=4) Hessians are eigensolved **exactly** in seconds — no dense
FD assembly, no SLQ variance. A **lattice gauge** converts stored index-ordered
edge components to the canonical base→tip orientation (the stored `v0<v1`
direction does not commute with translation — the first smoke run's covariance
battery caught this at 1.6e-2; fixed, covariance 9.5e-15).

## Construction

0. Per volume n ∈ {3,4}: canonical torus, edge→(base, displacement-type) orbit
   map, translation-invariant unit rays (same per-type coefficients across
   volumes, seeded identically — like-for-like volume comparison).
1. θ\* by projected Newton in the invariant sector (3 DOF) on the analytic
   joint gradient; the **equivariance battery** checks the FULL θ gradient
   vanishes at θ\* (covariance ⇒ an invariant configuration has an invariant
   gradient).
2. `V_eff(t) = S_B + ½Σ log λ⁺` (verbatim IR convention) from the momentum
   blocks; classification on the raw curve by the strict-local-minimum rule
   (no fits — the Phase446 lesson).
3. Batteries: exact S_B translation covariance for every member on every torus
   (gate 1e-10); block reconstruction (H·v via translated columns vs direct Hv,
   gate 1e-4); objective consistency (ComputeJointGradient vs EvaluateWithTheta,
   1e-10); analytic-vs-FD gradient (1e-6); identity θ-independence; trace
   consistency (recorded); θ gate with the Phase446 absolute-floor convention.

## Results (filled from the committed run)

Terminal status:
`torus-mode-volume-saturation-probe-passed-no-saturation-persists-across-mode-volumes-frontier-sharpened`
(runtime ~13 min for both volumes — the pre-unlock estimate was ~2 h *per point*).

| | n=3 (jointDof 3888) | n=4 (jointDof 12288) |
|---|---|---|
| covariance | 9.5e-15 | 1.4e-14 |
| block reconstruction | 2.9e-11 | 2.9e-11 |
| Einsteinian classification | trivial-origin (both members, both seeds) | trivial-origin (both members, both seeds) |

`modeVolumeVerdict = no-saturation-persists-across-mode-volumes`;
`anySaturationAnyVolume = false`; `volumeTrendSeedStable = true`. Every
Einsteinian `V_eff` is monotone rising along the invariant rays at 5× and 16×
the minimal mesh's mode count. **Phase443's mode-volume lever is decided
negative: the one-loop no-saturation verdict is not a small-mesh artifact.**

Build lesson (fail-closed smoke): the stored index-ordered edge direction does
not commute with translation — the covariance battery caught the missing
orientation signs at 1.6e-2; the lattice-gauge fix collapsed it to 9.5e-15.

## Mandatory framing (verbatim intent)

Workbench-relative structure data ONLY — su(2) toy algebra on the reduced
Spin(4) slice, lattice units, one loop. The invariant-ray convention is
**recorded** (`invariantRayConventionRecorded = true`): it differs from
Phase443's random open-mesh rays, so the controlled statement is the saturation
**trend across tori**, not a numeric comparison to the 16-vertex values.
**NO GeV/pole/VEV promotion either way** (`physicistReviewPending = true`).

## Fail-closed

Target-blind construction (hash recorded); reduced-spin4-slice only; no
scale/pole/GeV lineage; no Phase201/Phase256 contract field filled; nothing
promoted either way. Precursors: Phase443, Phase444 (`phase444Passed`), and
Phase447 (`twoLoopSaturationProbePassed`).

## Run

```bash
dotnet run -c Release --project studies/phase448_torus_mode_volume_saturation_probe_001/Phase448TorusModeVolumeSaturationProbe.csproj
```

Ends `Passed=True`, 0 warnings. Outputs `output/torus_mode_volume_saturation_probe.json`
and `_summary.json` (full per-point ray data per volume/member/seed). Runtime
minutes (the unlocks turned the formerly ~2 h/point computation into seconds).

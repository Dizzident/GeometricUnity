# Phase444: Mode-Volume-Scaled Saturation Probe

The named follow-up to Phase443 (which found NO one-loop log-saturation on the
16-vertex `CreateUniform4D(1)` mesh, and named "the SAME probe on a larger mesh"
as its first lever, citing the Phase437 lesson that the genuine Coleman–Weinberg
`t^4 log t` regime needs MODE VOLUME).

## The question

Does the Phase443 no-saturation verdict change with **mode volume** — i.e. does
running the same joint effective-potential analysis on a LARGER mesh (the periodic
4-torus `CreateUniform4DPeriodic(3)`, 81 vertices, joint DOF 3888 — **16×** Phase443's
243) flip any member's classification or the octave-slope structure?

## Outcome: RECORDED-FINDINGS study — no physics verdict

`modeVolumeChangesVerdict = "undetermined-tooling-blocked"`. The mandated engineering
unlock — block-diagonalizing the joint Hessian by lattice momentum on the torus — was
**prototyped first** and found **not viable** on this platform, and the SLQ fallback is
~2 orders of magnitude over budget. The phase **passes on internal consistency**
(precursors + the honest, reproduced diagnostic findings + fail-closed framing); it
reports what IS and names the future-work unlocks. It carries **no physics verdict**.

## Evidence chain (recomputed on the n=3 torus; real measured numbers in the JSON)

- **F1 — lowest-index vertex→face rule is not translation-covariant.** The Einsteinian
  operator attaches theta per face via `Faces[f][0]` (lowest GLOBAL index). The row-major
  index wrap reorders which incident vertex is smallest, so **56.8%** of (face, translation)
  pairs violate `v(f+R)=v(f)+R`.
- **F2 — raw bivectors are seam-inflated; minimal-image restores orbit-invariance.** The
  operator builds face bivectors from RAW coordinate differences; on the torus wrapped edges
  have coordinate jumps of magnitude `n-1` (the mesh's own documented consumer contract), so
  **75.1%** of faces are inflated (raw orbit-norm spread **4.84**). The minimal-image
  reduction restores **exact** bivector-norm orbit-invariance (**0.0**).
- **F3 — DEFINITIVE: even incident-average + minimal-image is not translation-covariant.**
  Convention-agnostic test `S_B(T_R x) == S_B(x)` under the correctly **signed** edge
  permutation (a connection 1-form negates when its edge reverses) + unsigned vertex
  permutation. All four rule/bivector conventions fail the `1e-8` block-diagonalization bar
  (best `4.8e-3`), and the **curvature `‖F‖²` alone** (pure Gu.Geometry, upstream of the
  Shiab) is already non-covariant (`2.5e-4`). Root cause: **global-index-sorted orientation
  conventions** (mesh face-boundary orientation signs, bivector vertex-sort parity, cell-face
  ordering) do not commute with lattice translation on a periodic mesh.
  - *Superseded-measurement honesty note:* earlier theta-Hessian residuals (`1.6e-2`
    lowest-index / `8.7e-3` averaged) used a background that ignored edge-orientation signs,
    so `H(x)` and `H(T_R x)` were Hessians at different physical points — a confound. The
    signed-`S_B` test is the clean measurement and supersedes them; the conclusion is unchanged.
- **F4 — the SLQ fallback is over budget.** The platform exposes only O(full-mesh) forward
  evaluations (no adjoint/joint-gradient), so every gradient costs O(nDOF × mesh). Measured:
  joint objective eval ~6 ms; full theta-gradient ~3.5 s; one J_omega column ~7 ms → a faithful
  Hessian-vector product ~**60 s** → a single composite point's SLQ tr-log ~**2 h** (dozens of
  points needed).

## Authorized platform change (option A' — verified-correct, necessary-but-insufficient)

Additive and open-mesh-inert:

- `EinsteinianShiabFamilySpec.cs`: `VertexFaceRule {LowestIndex (default), IncidentAverage}`
  on the member; `/avg` BranchId suffix only for the non-default rule (all prior BranchIds
  byte-identical).
- `EinsteinianShiabOperator.cs`: additive `latticePeriod` ctor flag (0 = off = raw = byte-identical;
  `>0` applies the minimal-image convention in `PrecomputeCellMaps`); `FaceTheta()` threads the
  rule into `EvaluateWithTheta`/`LinearizeTheta` (chain-rule weight `1/|incident|`).

Verified: open-mesh theta=0 low-vs-avg byte-identity `0.0` (exact); incident-average
LinearizeTheta-vs-FD `4e-10`; minimal-image bivector-norm orbit-invariance `0.0`. Tests in
`tests/Gu.ReferenceCpu.Tests/EinsteinianShiabVertexFaceRuleTests.cs` assert exactly what holds,
and a recorded-limitation test documents the measured non-covariance (`~2.5e-3`).
`minimalImageBivectorsOnPeriodicMeshes = true`, `physicistReviewPending = true` (physicist-4d
session expired; the mesh-contract rationale is documented).

## Named future-work unlocks (recorded; not started)

1. **Lattice-canonical geometry conventions** — a USER-decision major platform investment
   (scope: `MeshTopologyBuilder` orientation/sort conventions + `CurvatureAssembler` + the
   Shiab per-cell assembly in Gu.Geometry). Demanded by F3. Expected outcome: exact
   block-diagonalization on tori (81 blocks of 48×48 at n=3), eigenvalues matching Phase443's
   Jacobi convention. **Not started** — named for the user to decide.
2. **Adjoint / joint-gradient platform path** — expose an analytic joint (omega, theta)
   gradient / contraction-adjoint so a Hessian-vector product costs O(mesh). Demanded by F4.
   Makes SLQ feasible (variance-bounded, not exact counts).

## Mode-volume heuristic (LABELED, non-verdict)

At one loop with a quartic tree action `S_B ~ t^4`, additional modes rescale only the subleading
`~N log t` one-loop term and cannot overturn the `t^4`-dominated runaway asymptotics. This
LABELED expectation suggests mode volume alone would NOT flip Phase443's verdict — consistent with
the Phase437 lesson that a new MECHANISM, not just mode volume, is what is missing. It is **not** a
computed verdict; the block-path or SLQ computation would supersede it either way.

## Fail-closed

Target-blind construction (`targetBlindConstructionHash` recorded); reduced spin-4 slice only
(the six recorded-boundary keys: `definition81Scope = reduced-spin4-slice`,
`ambientSevenSevenRealized = internalGaugeContentRealized = weldRealized = false`,
`canFillPhase201WzContract = canFillPhase256Contract = false`). No scale/pole/GeV lineage; no
Phase201/Phase256 contract field filled; `acceptedContractFieldCount = 0`. Nothing is promoted.

Precursors: Phase443 (`jointEffectivePotentialSaturationProbePassed`, no-saturation) and
Phase442 (`einsteinianJointHessianDegreeExceedsTwo`).

## Run

```bash
dotnet run -c Release --project studies/phase444_mode_volume_scaled_saturation_probe_001/Phase444ModeVolumeScaledSaturationProbe.csproj
```

Ends `Passed=True`, 0 warnings, ~5 s runtime. Outputs
`output/mode_volume_scaled_saturation_probe.json` and `_summary.json`.

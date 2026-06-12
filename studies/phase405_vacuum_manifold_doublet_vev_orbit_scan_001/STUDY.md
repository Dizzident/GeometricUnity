# Phase405: Vacuum-Manifold Doublet-VEV Orbit Scan (Brute Force #2)

## Question

USER DIRECTIVE (2026-06-11), brute force #2: does the Upsilon = 0 vacuum
manifold of the control-branch objective (= the draft-claimed Higgs
potential, Phase402) PERMIT doublet-block VEVs, and does its geometry
SELECT them over other directions? Run on the GPU where warranted.

## Construction

su(3)-valued connections (minimal doublet-bearing algebra, Phase403) on a
6x6 structured fiber-bundle mesh. On the trivial-torsion identity-Shiab
branch at A0 = 0 the torsion is zero on both backends and
Upsilon = F = d omega + (1/2)[omega wedge omega]; the scan uses CLOSED
(exact 1-form) profiles dx, dy so d omega = 0 for every sample and the
landscape is purely bracket-driven: omega = t (cos phi u dA + sin phi v dB)
over all 36 unordered su(3) direction pairs x 12 angles x 6 magnitudes
(2592 samples). Exact prediction, machine-verified: flat iff [u, v] = 0,
lifted landscape = K t^4 sin^2 phi cos^2 phi ||[u,v]||^2. Objectives use
the explicit plain norm on both backends (su(3) Killing pairing is
negative-definite; metric convention recorded).

## Results

- **The vacuum manifold PERMITS doublet-block VEVs**: every rank-1
  direction (any single Lie direction with any profile - doublet block
  included) lies exactly on the Upsilon = 0 locus.
- **Flatness coincides exactly with commutativity** (11 flat pairs = 11
  commuting pairs) and every lifted pair matches the exact quartic shape.
- **NO SELECTION MECHANISM exists at this level**: triplet, doublet, and
  singlet rank-1 directions are treated identically (all exactly flat).
  Scalar-sector sub-gap (b) - vacuum-manifold breaking geometry - is
  confirmed open with sharpened evidence: selection must come from
  structure beyond the bare control-branch bosonic objective.
- **PLATFORM FINDING (GPU)**: the run machine-detected a real-mesh
  topology defect in the native CUDA curvature kernel's LINEAR (d omega)
  part - single-direction probe |cpu| = 3.4176 vs |gpu| = 3.5165 with
  |diff| = 2.0356 unchanged when brackets are added; with closed profiles
  the CPU correctly returns 0 while the GPU returns up to 17.5 (0/27
  parity samples agree). The platform's CUDA parity tests used synthetic
  topology only and never caught this. Per the CPU-reference-first rule
  (IA-5) the science ran on the CPU (8.1 s for the full scan); the defect
  is recorded as a named platform follow-up. A second limitation is also
  recorded: the native buffer-handle table is monotonic (no recycling,
  MAX_BUFFERS = 4096), requiring periodic GPU session recycling.
- **PLATFORM FINDING RESOLVED (2026-06-12)**: root-cause isolation (single
  triangle, then the full real 420-edge/216-face mesh, through a minimal C
  harness and a C# bisect probe) exonerated the native curvature kernel -
  it is exact on real mesh topology at both sizes. The defect was an API
  lifecycle bug: `GpuSolverBackend.Initialize` re-initialized the
  already-prepared native backend, and `gu_initialize` performs a full
  shutdown that discards the uploaded topology/algebra/A0, silently
  downgrading every physics kernel to its identity-stub fallback
  (F = omega, T = 0 - exactly the recorded signature: |gpu F| = |omega|,
  gpu objective = (1/2)||omega||^2). The fix makes
  `GpuSolverBackend.Initialize` adopt a prepared backend instead of
  re-initializing it, and real-mesh parity tests
  (`tests/Gu.Interop.Tests/RealMeshPhysicsParityTests.cs`) now guard the
  prepare-then-wrap session pattern on both the managed reference and real
  CUDA. Re-run after the fix: 27/27 parity samples agree
  (maxAbsDev 3.9e-34); all science verdicts unchanged (the scan always ran
  on the CPU reference per IA-5).

## Status

Fail-closed. Control-branch objective only; constant/separable profiles;
su(3) is a study algebra; no scales; nothing promoted; zero contract
fields.

## Reproduce

```bash
LD_LIBRARY_PATH=native/build dotnet run --project studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/Phase405VacuumManifoldDoubletVevOrbitScan.csproj
```

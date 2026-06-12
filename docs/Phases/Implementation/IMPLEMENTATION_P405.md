# IMPLEMENTATION_P405: Vacuum-Manifold Doublet-VEV Orbit Scan

## Scope

Brute force #2 of the 2026-06-11 user directive: the su(3) orbit scan of
the Upsilon = 0 vacuum manifold (the draft-claimed Higgs potential's zero
locus) for doublet-VEV permission and selection, with the GPU engaged per
the directive.

## Artifacts

- Study: `studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001`
- Project: `Phase405VacuumManifoldDoubletVevOrbitScan.csproj`
- Outputs: `output/vacuum_manifold_doublet_vev_orbit_scan.json` and
  `..._summary.json`
- Precursors: Phase403 (doublet block), Phase404 (chain enumeration).

## Method and results

| Item | Result |
| --- | --- |
| Scan | 36 su(3) pairs x 12 angles x 6 magnitudes = 2592 samples, closed profiles (d omega = 0) |
| Rank-1 flatness | ALL flat (doublet included) - vacuum manifold permits doublet VEVs |
| Flatness = commutativity | exact (11 = 11); quartic landscape shape verified |
| Selection mechanism | NONE at this level (all blocks treated identically) - sub-gap (b) open, sharpened |
| GPU | CUDA active; real-mesh parity defect machine-characterized (0/27 parity, maxAbsDev 17.5); science on CPU per IA-5 (8.1 s full scan) |
| Native limitations recorded | monotonic buffer-handle table (session recycling required); Killing-vs-plain objective metric convention |
| Defect RESOLVED (2026-06-12) | root cause was NOT the native kernel (exact on real mesh in isolation): `GpuSolverBackend.Initialize` re-initialized the prepared backend, wiping uploaded physics data and silently engaging identity-stub fallbacks (F = omega, T = 0); fixed in `GpuSolverBackend.Initialize` + guarded by `tests/Gu.Interop.Tests/RealMeshPhysicsParityTests.cs`; post-fix re-run: 27/27 parity (maxAbsDev 3.9e-34), science verdicts unchanged |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`vacuumManifoldDoubletVevOrbitScan` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `vacuum-manifold-doublet-vev-orbit-scan-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase405 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase405 run passes.
- Phase101, Phase202 (checklist 197 -> 198 passed), claim-integrity
  verifier re-run with Phase405 included; objective remains incomplete by
  design.

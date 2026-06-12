# IMPLEMENTATION_P410: Curvature-Coupled VEV-Selection Probe

## Scope

Machine-test the TOE-GU-40YEARS-20250602 curvature-coaxing VEV claim in
its simplest faithful bosonic realization: a uniform curvature-coupled
quadratic term added to the Phase405 vacuum landscape. Fail-closed,
target-blind, qualitative-source-motivated (GU-DRAFT-2021 primary).

## Artifacts

- Study: `studies/phase410_curvature_coupled_vev_selection_probe_001`
- Project: `Phase410CurvatureCoupledVevSelectionProbe.csproj`
- Outputs: `output/curvature_coupled_vev_selection_probe.json` and
  `..._summary.json`
- Precursors: Phase405 (flat vacuum manifold, no selection), Phase409
  (pairing menu closure).

## Method and results

| Item | Result |
| --- | --- |
| Augmented objective | S_aug = S_B + (kappaR/2) R_eff \|\|omega\|\|^2; R_eff/kappaR arbitrary-sign external parameters; VEV needs kappaR R_eff < 0 |
| C1 flat rays | exactly flat in S_B (max 0.0) -> augmented landscape UNBOUNDED BELOW along every rank-1 ray: runaway, not a finite VEV |
| C2 quadratic invariant | direction-blind (c2 identical on all 8 directions, rel dev 0.0) - no block ordering at quadratic level |
| C3 lifted sector | flatness = commutativity (3/25); S_B = K t^4 exact; K/\|\|[u,v]\|\|^2 constant (cv 1.2e-15) |
| C4 depth ordering | inverse bracket-norm; deepest stratum (\|\|[u,v]\|\|^2 = 1/4, 16 pairs) block-DEGENERATE across DD and DT pairs - `doubletVevSelectedByCurvatureCoupling=False` |
| Verdict | uniform-curvature-coupling-produces-runaway-not-doublet-selection; sub-gap (b) open, sharpened: selection requires direction-dependent coupling or the Dirac-sector realization (Phase411) |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`curvatureCoupledVevSelectionProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `curvature-coupled-vev-selection-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase410 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase410 run passes (all exactness checks).
- Phase101, Phase202 (checklist 202 -> 203 passed expected),
  claim-integrity verifier re-run with Phase410 included; objective
  remains incomplete by design.

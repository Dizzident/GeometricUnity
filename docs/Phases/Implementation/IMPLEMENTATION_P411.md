# IMPLEMENTATION_P411: Quartic / Dirac-Squared Spinor-Sector Composite Probe

## Scope

Machine-characterize the Dirac-squared composite route (the convergence
point of Phase409's lowest-open-order theorem, Phase410's bosonic
closure, and the "quartic Higgs from Dirac squaring" primary heuristic):
the welded spinor bilinear channels of the chimeric chiral carriers.

## Artifacts

- Study: `studies/phase411_quartic_dirac_squared_spinor_composite_probe_001`
- Project: `Phase411QuarticDiracSquaredSpinorCompositeProbe.csproj`
- Outputs: `output/quartic_dirac_squared_spinor_composite_probe.json`
  and `..._summary.json`
- Precursors: Phase409 (pairing-menu closure), Phase410
  (curvature-coupling closure).

## Method and results

| Item | Result |
| --- | --- |
| Carriers | S_L/R = 2_L/R (x) 16: Cl(4) Weyl halves (homomorphism residual <= 1e-10, labels machine-discovered) x Phase404 Cl(10) chiral 16, welded by the Phase408 Sym^2 pi |
| Welded 16 branching | (1/2,3/2) + (3/2,1/2) (realified x2), recovered by joint sub-Casimirs, character-cross-checked |
| Dirac mass channel | S_L (x) S_R has ZERO welded scalars ((half,int) x (int,half) types cannot pair to a singlet) - the Yukawa channel cannot carry a welded-scalar VEV |
| Majorana channels | 16 welded scalars each (direct kernel = character count); SM-stable subspace contains ZERO SM-doublet states |
| Quartic | spin-0 dimensions (LR)^2 = 9632, (LL)^2 = 9856 by character arithmetic; SM-stable analysis = named remaining order |
| Caveats | complex/compact arithmetic (Nguyen-Polya complexification caveat carried); Pin-parity not attempted; unobserved-phase fields unprobed |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`quarticDiracSquaredSpinorCompositeProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `quartic-dirac-squared-spinor-composite-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase411 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase411 run passes (all consistency checks: Cl(4)/Cl(10)
  residuals, content recovery, character matches, direct-kernel match).
- Phase101, Phase202 (checklist 203 -> 204 passed expected),
  claim-integrity verifier re-run with Phase411 included; objective
  remains incomplete by design.

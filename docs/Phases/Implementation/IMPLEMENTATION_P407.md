# IMPLEMENTATION_P407: Chimeric-Adjoint SM-Content Probe

## Scope

First probe inside the Phase406 surviving region: complete branching of
the chimeric algebra's adjoint (so(14) compact arithmetic for spin(7,7),
91 states) under {spacetime so(4)} x {the Phase404 SM chain}.

## Artifacts

- Study: `studies/phase407_chimeric_adjoint_sm_content_probe_001`
- Project: `Phase407ChimericAdjointSmContentProbe.csproj`
- Outputs: `output/chimeric_adjoint_sm_content_probe.json` and
  `..._summary.json`
- Precursors: Phase404 (adjoint exclusion), Phase406 (surviving region).

## Method and results

| Item | Result |
| --- | --- |
| Dimension accounting | 91 = 6 + 45 + 40 exact |
| SM-Higgs-pattern doublets (color-singlet, j=1/2, abs Y=1/2) | EXIST: 16 states, all in the frame-cross-internal (4 x 10) block |
| Mechanism | PS vector 4 = (2_L,2_R), B-L = 0, Y = su(2)_R Cartan = +-1/2 (machine-verified) |
| Spacetime-scalar sector | NO Higgs-pattern doublet (Phase404 re-confirmed in the bigger frame) |
| Honest tag | found doublets carry a spacetime-vector index; spin-0 extraction via the Y14 vertical-form structure is the named next step |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`chimericAdjointSmContentProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `chimeric-adjoint-sm-content-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase407 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase407 run passes.
- Phase101, Phase202 (checklist 199 -> 200 passed), claim-integrity
  verifier re-run with Phase407 included; objective remains incomplete by
  design.

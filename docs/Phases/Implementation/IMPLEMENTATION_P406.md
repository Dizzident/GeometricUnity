# IMPLEMENTATION_P406: Choice-Space Falsification Sweep

## Scope

Brute force #3 of the 2026-06-11 user directive: the falsification map of
GU's open discrete choices against the program's machine-verified
structural filters, with two new exact computations (su(5)-path ratio;
Cl(6,4)/Cl(7,3) signature axis).

## Artifacts

- Study: `studies/phase406_choice_space_falsification_sweep_001`
- Project: `Phase406ChoiceSpaceFalsificationSweep.csproj`
- Outputs: `output/choice_space_falsification_sweep.json` and
  `..._summary.json`
- Precursors: Phases 396/397/403/404/405 summaries (filter outcomes read
  from their JSONs).

## Method and results

| Item | Result |
| --- | --- |
| su(5)-path ratio | tan^2 = 3/5 exactly = Pati-Salam value (path independence) |
| Signature axis | Cl(6,4) and Cl(7,3) verified; chiral halves 16/16 (signature independence) |
| Sweep | 16 combinations x 5 filters; 4 survive |
| Survivors | exactly {larger algebra} x {non-adjoint scalar} x {either path} x {either signature} |
| su(2) toy | falsified for the doublet route everywhere |
| VEV selection | absent in ALL combinations - binding gaps choice-independent |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`choiceSpaceFalsificationSweep` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `choice-space-falsification-sweep-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase406 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase406 run passes.
- Phase101, Phase202 (checklist 198 -> 199 passed), claim-integrity
  verifier re-run with Phase406 included; objective remains incomplete by
  design.

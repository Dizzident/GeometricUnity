# IMPLEMENTATION_P413: Noncompact Real-Form Transfer Probe

## Scope

Decide the "noncompact-only invariant" loophole (the most plausible
evasion per DEEP-RESEARCH-20260612) for the Phase408-412 composite
no-gos.

## Artifacts

- Study: `studies/phase413_noncompact_real_form_transfer_probe_001`
- Project: `Phase413NoncompactRealFormTransferProbe.csproj`
- Outputs: `output/noncompact_real_form_transfer_probe.json` and
  `..._summary.json`
- Precursors: Phase409 (compact counts 0/7), Phase412 (quartic closure).

## Method and results

| Item | Result |
| --- | --- |
| N1 Lorentzian weld | pi_eta: so(1,3) -> gl(10) on Sym^2(R^{1,3}): exact homomorphism (2.2e-16); preserves B(S,T)=Tr(eta S eta T) (2.2e-16); machine signature of B = **(7,3)** (matches the Phase406 Cl(7,3) axis) |
| N2 keystone | complexified welds coincide exactly under T4 = diag(i,1,1,1) (residual 2.2e-16) -> every complex-linear kernel dimension transfers verbatim |
| N3 recomputation | direct noncompact kernels: linear singlet 0 (= compact), bilinear spin-0 7 (= compact) |
| Verdict | no-gos are REAL-FORM INDEPENDENT; the noncompact evasion is closed on finite-dimensional carriers |
| Named residuals | real/Majorana structure bookkeeping (cannot create complex invariants); unitary representation category out of scope |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`noncompactRealFormTransferProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `noncompact-real-form-transfer-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase413 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase413 run passes (all batteries at 2.2e-16 residuals).
- Phase101, Phase202 (checklist 205 -> 206 passed expected),
  claim-integrity verifier re-run with Phase413 included; objective
  remains incomplete by design.

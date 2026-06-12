# IMPLEMENTATION_P409: Invariant-Pairing-Menu Spin-Zero Extraction Probe

## Scope

Extend the Phase408 spin-0 extraction obstruction from the single
vertical-trace pairing to the COMPLETE machine-enumerated invariant
menu on the welded frame-cross block, explicitly including the
parity-odd (Levi-Civita epsilon-built) sector, through bilinear order.
Source-independent: motivated by (but not relying on) an unverified
relayed "Shiab uniqueness/pairing count" summary (journal 2026-06-12).

## Artifacts

- Study: `studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001`
- Project: `Phase409InvariantPairingMenuSpinZeroExtractionProbe.csproj`
- Outputs: `output/invariant_pairing_menu_spin_zero_extraction_probe.json`
  and `..._summary.json`
- Precursor: Phase408 (trace-slot obstruction, weld entanglement).

## Method and results

| Item | Result |
| --- | --- |
| Pairing menu | 4x4: 1 (even); 10x10: 2 (both even); 4x10: 0 - no parity-odd fiber pairing exists |
| Linear order | V = 4x10 has ZERO so(4)-singlet content (labels (1/2,1/2)^2+(1/2,3/2)+(3/2,1/2)+(3/2,3/2)) - every linear extraction closed, any parity |
| Bilinear order | spin-0 subspace exactly 7-dim (6 even + 1 epsilon-built odd; character cross-check 7 = 7; annihilation residual 2.6e-15); largest SM-stable subspace 1-dim; doubletPatternStateCount = 0 in both parity sectors |
| Odd orders | ALL closed exactly (every welded irrep is half-integer x half-integer; trilinear singlet count = 0) - next open order is QUARTIC |
| Verdict | `obstructionMenuCompleteThroughBilinearOrder=True`; named remaining: quartic+ even composites, epsilon/Shiab on content beyond the frame-cross block, or a different welded carrier |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`invariantPairingMenuSpinZeroExtractionProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `invariant-pairing-menu-spin-zero-extraction-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase409 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase409 run passes with all consistency checks (precursor,
  color dim 8, character cross-check, annihilation, parity counts,
  odd-order consistency).
- Phase101, Phase202 (checklist 201 -> 202 passed expected),
  claim-integrity verifier re-run with Phase409 included; objective
  remains incomplete by design.

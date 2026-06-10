# IMPLEMENTATION_P397: Parametrized u(1) Extension and Neutral-Mixing Underdetermination Probe

## Scope

Phase397 materializes the u(1) gauge-coupling machinery on the control
branch with the charge explicitly underived, measures the electroweak-like
sector channels, and converts the "missing embedding" boundary into three
precise findings.

## Artifacts

- Study: `studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001`
- Project: `Phase397ParametrizedU1ExtensionNeutralMixingUnderdeterminationProbe.csproj`
- Outputs: `output/parametrized_u1_extension_neutral_mixing_underdetermination_probe.json`
  and `..._summary.json`

## Results

| Quantity | Value |
| --- | --- |
| u(1) variation Hermiticity residual | 0 (exact) |
| su(2)-neutral source fraction (max, per mode) | 0.0023 |
| u(1) source channel | nonzero per unit charge |
| Neutral mixing ratio T[n,u1]/sqrt(T[n,n] T[u1,u1]) | <= 4.5e-9 (vanishes) |
| Hypercharge / coupling ratio / weak angle | underived / underived / unselected |

Diagnosis consequences:

1. The Z-like (su(2)-neutral) channel is sourceless on this branch; the
   sources live in the charged pair (W-like) channel - quantifying the
   sector asymmetry of the coupled residual.
2. The vanishing fermion-bilinear neutral mixing (trace selection rule)
   means photon/Z mixing requires the symmetry-breaking scalar sector -
   linking Phase256's photonEigenstateProjectionId directly to Phase201's
   missing Higgs scalar source row.
3. The photon/Z separation is a one-parameter family; the named blocking
   gap is {hypercharge lineage, coupling-ratio lineage, scalar sector}.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`parametrizedU1ExtensionNeutralMixingUnderdeterminationProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `parametrized-u1-extension-neutral-mixing-underdetermination-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase397 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase397 run passes.
- Phase101, Phase202 (checklist 189 -> 190 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase397 included; objective remains
  incomplete by design.

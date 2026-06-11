# IMPLEMENTATION_P404: GU Embedding-Chain Coupling-Ratio Enumeration

## Scope

Brute force #1 of the 2026-06-11 user directive: exhaustive enumeration of
the GU-named embedding chain (so(10) compact level = D5 complexification
of spin(6,4)) with the embedding-derived coupling-ratio menu and the
adjoint/spinor doublet inventories.

## Artifacts

- Study: `studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001`
- Project: `Phase404GuEmbeddingChainCouplingRatioEnumeration.csproj`
- Outputs: `output/gu_embedding_chain_coupling_ratio_enumeration.json`
  and `..._summary.json`
- Precursors: Phase402 (dictionary audit), Phase403 (ratio mechanism).

## Method and results

| Quantity | Value |
| --- | --- |
| Standard-direction ratio | tan^2 = 3/5 exactly; sin^2 = 3/8 (derived blind, internal normalization) |
| Scan | 224 hypercharge directions; tan^2 menu [0.027, 1.5] |
| Family pattern from the 16 | derived: 1/6, 1/2, 2/3, 1/3, 1, 0 (lepton-normalized) |
| Adjoint color-singlet charged doublet | ABSENT for all 224 directions |
| Battery | su(2) closure (auto-resolved signs), [J,su(2)]=0, u(3)/su(3) dims 9/8, color closure 7e-16, spinor homomorphism 0, 16-dim exact |
| GPU | not used (objects 10x10/32x32; seconds on CPU - recorded honestly per directive) |

Consequence: the scalar-sector doublet cannot come from the
gauge-algebra-adjoint part of the connection on this chain - the
non-adjoint (vertical symmetric-2-tensor) components are the only
remaining GU-native location. The ratio menu is the theorem-level
coupling-ratio lineage the Phase397 one-parameter family was missing -
still without scale/pole/GeV, so no contract fields are fillable.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`guEmbeddingChainCouplingRatioEnumeration` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `gu-embedding-chain-coupling-ratio-enumeration-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase404 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase404 run passes.
- Phase101, Phase202 (checklist 196 -> 197 passed), claim-integrity
  verifier re-run with Phase404 included; objective remains incomplete by
  design.

# IMPLEMENTATION_P403: Adjoint Doublet-Substructure Branching Probe

## Scope

Machine-characterizes the Phase402 doublet-equivalent sub-gap at the
algebra level: whether and how an adjoint-valued scalar can contain an
SM-pattern-capable doublet, from the repo's own structure constants.

## Artifacts

- Study: `studies/phase403_adjoint_doublet_substructure_branching_probe_001`
- Project: `Phase403AdjointDoubletSubstructureBranchingProbe.csproj`
- Outputs: `output/adjoint_doublet_substructure_branching_probe.json`
  and `..._summary.json`
- Precursors: Phase397 (underdetermination), Phase402 (doublet requirement).

## Method and results

| Check | Result |
| --- | --- |
| su(2) adjoint branching | pure j=1 triplet; doublet blocks: 0 |
| su(3) adjoint branching | 3_0 + 1_0 + conjugate doublet pair (4-real-dim j=1/2, abs Y = sqrt(3)/2) |
| Custodial pattern from doublet-block VEV | EXACT (massless count 1, charged degenerate, T3-Y mixing, identity residual 0.0) |
| Embedding-derived ratio | tan^2(theta_emb) = 3 (derived, recorded blind) |
| Phase397 underdetermination explained | toy su(2) algebra too small to contain the doublet route |

Structural conclusions: the doublet-equivalent substructure exists
generically in larger-adjoint routes (gauge-Higgs-unification mechanism);
the coupling ratio arrives DERIVED from the embedding (theorem-level
mechanism for the hypercharge/coupling-ratio lineage row); the named open
gaps are the GU-specific embedding chain, the vacuum-manifold VEV
selection, and the quantitative chain.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`adjointDoubletSubstructureBranchingProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `adjoint-doublet-substructure-branching-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase403 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase403 run passes (branching + custodial battery exact).
- Phase101, Phase202 (checklist 195 -> 196 passed), claim-integrity
  verifier re-run with Phase403 included; objective remains incomplete by
  design.

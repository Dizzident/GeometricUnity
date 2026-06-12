# IMPLEMENTATION_P412: Quartic SM-Doublet Intersection Analysis

## Scope

Execute the quartic SM-stable analysis named by Phase411 (user
directive 2026-06-12), via the strictly stronger ambient-intersection
formulation on 16^(x4).

## Artifacts

- Study: `studies/phase412_quartic_sm_doublet_intersection_analysis_001`
- Project: `Phase412QuarticSmDoubletIntersectionAnalysis.csproj`
- Outputs: `output/quartic_sm_doublet_intersection_analysis.json` and
  `..._summary.json`
- Precursor: Phase411 (bilinear-channel closure; quartic singlet counts).

## Method and results

| Item | Result |
| --- | --- |
| Formulation | ambient intersection: (welded labels allowed by the channel's 2-leg factors) vs (SM-doublet isotypic of 16^(x4)); zero closes the quartic order outright, covering all statistics projections |
| Candidate sector | V_w = 896 of 65536 (SM Cartans diagonal by basis choice; \|Y\|=1/2, color weight 0, \|mL\|=1/2) |
| Doublet isotypic | 480 real dims (joint kernel of C_color and (C_L-(3/4)kappaL)^2 on V_w; residual 8.0e-15) |
| Welded side | product-form spectral projectors in commuting C_A, C_B (j(j+1)kappa grid, idempotency 9.4e-15); 9 labels from exact 2-leg character arithmetic |
| Intersection | ZERO in every channel; top Gram eigenvalues 0.146/0.000/0.479/0.000/0.113, union 0.604 (eigenvalue 1 required) |
| New data | odd-mixed channels LLLR/LRRR have zero welded singlets entirely; LLLL=9856, LLRR=9632 reproduce Phase411 |
| Verdict | quartic-order-closed-no-welded-scalar-sm-doublet-in-any-channel |
| Engineering | first run timed out at 1h single-threaded; final run ~8 min (parallel leg-matvecs, dense Jacobi kernel finder after a pivoted-Cholesky attempt proved numerically inadequate - residual 2.2 vs 8e-15, and undercounted the kernel 338 vs 480) |

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`quarticSmDoubletIntersectionAnalysis` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item `quartic-sm-doublet-intersection-analysis-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase412 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279,
  phase281, phase295, phase296

## Validation

- Targeted Phase412 run passes (all exactness checks + Phase411
  character cross-checks).
- Phase101, Phase202 (checklist 204 -> 205 passed expected),
  claim-integrity verifier re-run with Phase412 included; objective
  remains incomplete by design.

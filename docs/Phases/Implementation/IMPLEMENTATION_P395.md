# IMPLEMENTATION_P395: Source-Current Axis Structure and Gauge Covariance Probe

## Scope

Phase395 answers the sharpest open internal question after Phase394 - why
the source currents, Gram image, and backreaction direction avoid gauge
axis 1 - and proves the answer is structural and gauge-covariant.

## Artifacts

- Study: `studies/phase395_source_current_axis_structure_gauge_covariance_probe_001`
- Project: `Phase395SourceCurrentAxisStructureGaugeCovarianceProbe.csproj`
- Outputs:
  - `output/source_current_axis_structure_gauge_covariance_probe.json`
  - `output/source_current_axis_structure_gauge_covariance_probe_summary.json`

## Findings

| Object | bg-a | bg-b |
| --- | --- | --- |
| Omega dominant axis fraction (along ~(1,1,1)/sqrt 3) | 0.9686 | 0.9708 |
| Block Gram eigenvalues | {2.4e-5, 4.7e-5, 8.8e-2} | {1.4e-5, 6.5e-5, 1.0e-1} |
| Dominant fraction | 0.9992 | 0.9994 |
| Dominant direction `d` | (0.737, 0.031, -0.675) | (0.721, 0.013, -0.693) |
| Coordinate shadow `d_a^2` | [0.543, 0.0009, 0.456] | [0.520, 0.0002, 0.480] |
| Angle `d` to omega axis | 87.05 deg | 89.09 deg |
| Covariance residuals (2 rotations) | <= 9.5e-11 | <= 5.6e-11 |

Structural chain: the background omega is the symmetric ansatz (invariant
axis `n_omega ~ (1,1,1)/sqrt(3)`); the shell-response block Gram is
effectively rank-one along a single direction `d` lying in the charged plane
orthogonal to `n_omega`; the Phase379 "two-axis dominance with suppressed
coordinate axis 1" is exactly the coordinate shadow of `d` (squared
components reproduce the persisted fractions); and the whole structure
transforms covariantly under global gauge rotations, verified exactly via
`D' = D - delta_D[omega] + delta_D[R omega]` (Phase389 linearity) with
spectrum invariance and `T' = R T R^T` at ~1e-10.

Interpretive consequences recorded in the journal:

- "Suppressed axis 1" was never a suppression: it is rank-one concentration
  plus a coordinate shadow that happens to nearly miss e_1.
- The direction `d` is gauge-frame-dependent. Raw-coordinate axis statements
  (Phase307 selectors, Phase381/383 blockers) are gauge-frame statements;
  only the geometry relative to `n_omega` is invariant.
- A target-blind canonical photon/W/Z/H namespace map cannot be built from
  raw carrier axes - it must use gauge-invariant combinations. This is a
  constructive sharpening of the Phase385 boundary and of Phase388 theorem
  requirement "canonical-gauge-axis-or-observed-namespace-selector".

## Caveats

The two smallest block-Gram eigenvalues are nearly degenerate, so the
individual "minimum direction" is unstable within the near-null plane; the
robust invariants are the dominant direction and the near-null plane (the
output records this caveat explicitly). Toy control branch; no physical
scale.

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`sourceCurrentAxisStructureGaugeCovarianceProbe` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `source-current-axis-structure-gauge-covariance-probe-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase395 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase395 run passes with covariance verified.
- Phase101, Phase202 (checklist 187 -> 188 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase395 included; objective remains
  incomplete by design.

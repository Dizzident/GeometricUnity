# IMPLEMENTATION_P392: Coupled Mixed-Hessian Fermion-Induced Response Audit

## Scope

Phase392 builds the first action-derived carrier response operator from the
coupled boson-fermion second variation on the Phase390 converged branch and
compares its structure to the study-defined Phase378/379 Gram. Verdict:
**diverges-from-gram-structure** - the rank-3 image and suppressed gauge
axis do not persist in the action-derived response; they are properties of
the Hilbert-Schmidt pullback metric.

## Artifacts

- Study: `studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001`
- Project: `Phase392CoupledMixedHessianFermionInducedResponseAudit.csproj`
- Outputs:
  - `output/coupled_mixed_hessian_fermion_induced_response_audit.json`
  - `output/coupled_mixed_hessian_fermion_induced_response_audit_summary.json`

## Method

Per persisted Phase12 background:

1. Dense Jacobi solve of `B = M^{-1/2} D M^{-1/2}` (Phase390 solver);
   lowest-nonzero shell by the Phase378 grouping rule (4 modes, eigenvalues
   +-8.4e-4 / +-9.1e-4).
2. Mixed blocks `y_{k,s} = delta_D[e_k] psi_s` for all 156 carrier
   coordinates and 4 shell modes (`DiracVariationComputer.ComputeAnalytical`
   on unit basis vectors); eigenbasis coefficients
   `c_{k,s} = W^dagger M^{-1/2} y_{k,s}`.
3. Response operator
   `R_kl = sum_s sum_{j retained} Re(c_{k,s}[j]* c_{l,s}[j]) / (lambda_j - lambda_s)`,
   retaining all eigenmodes outside the degenerate shell group (the exact
   `(D - lambda_s M)^+` on the M-orthogonal complement).
4. Spectrum/signature/rank of `R` (Phase378 tolerance formula on |eig|),
   gauge-axis fractions from the significant eigenvectors (Phase379 rule),
   comparison against the persisted Phase379 fractions.
5. Pure-gauge diagnostic `v(X)^T R v(X)` over the 84 Phase389
   covariant-differential directions, compared to the mean diagonal scale.

## Results

| Quantity | bg-a | bg-b | Phase378/379 Gram |
| --- | --- | --- | --- |
| Significant rank | 146 (70+/76-) | 141 (69+/72-) | 3 (PSD) |
| Axis fractions | [0.334, 0.328, 0.337] | [0.332, 0.336, 0.332] | [0.54, 0.002, 0.46] |
| Suppressed axis | none (argmin 1, ~isotropic) | none (argmin 2) | 1 (stable, <= 0.002) |
| Asymmetry residual | 0 | 0 | - |
| Min retained denominator | 8.4e-4 | 9.1e-4 | - |
| Pure-gauge/generic ratio | 29.6 | 12.0 | - |

Diagnosis update: the suppressed-axis obstruction is metric-dependent. The
Phase381/383/384 blockers stand as statements about the study-defined Gram
(solver-independent per Phase391) but do NOT transfer to the action-derived
second-order response. Any future suppressed-axis W-row theorem must specify
which response metric it concerns; conversely, the Phase307 selector conflict
cannot be declared physical without an action-derived (and ultimately
gauge-consistent, coupled-critical-point) carrier image.

## Honest scope limits (declared fail-closed)

- `backgroundIsCoupledCriticalPoint=false`: omega was solved bosonic-only.
- Candidate bilinear action; no omega-omega bosonic Hessian block.
- Toy control branch; small absolute response scale (~1e-11), well-resolved
  relative to rank tolerance; gauge directions are not flat (candidate action
  not gauge-invariant off the coupled critical point).

## Integration

- `scripts/generate_validated_boson_predictions.sh` (both invocation blocks)
- `studies/phase101_boson_prediction_package_001/Program.cs`
  (`coupledMixedHessianFermionInducedResponseAudit` block)
- `studies/phase202_boson_objective_completion_audit_001/Program.cs`
  (checklist item
  `coupled-mixed-hessian-fermion-induced-response-audit-materialized`)
- `scripts/verify_boson_claim_integrity.sh` (phase392 path + assertion block)
- Broad scanner exclusions: phase204, phase205, phase207, phase279, phase281,
  phase295, phase296

## Validation

- Targeted Phase392 run passes with verdict `diverges-from-gram-structure`.
- Phase101, Phase202 (checklist 184 -> 185 passed), claim-integrity verifier,
  and the full generator gate re-run with Phase392 included; objective remains
  incomplete by design.

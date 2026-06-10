# Phase392: Coupled Mixed-Hessian Fermion-Induced Carrier Response Audit

## Question

After Phase391 confirmed the study-defined shell-response Gram invariants
(rank 3, suppressed gauge axis 1) are solver-independent, one question
remained for the carrier-image structure: are those invariants properties of
the underlying coupled dynamics, or properties of the chosen diagnostic
metric (the Hilbert-Schmidt pullback of the projected blocks)? The only
repo-local way to decide is an ACTION-DERIVED response operator built from
the coupled boson-fermion second variation.

## Construction

Around the candidate fermionic action `S_F(omega, psi) = Re<psi, D(omega) psi>`
with M_psi-normalized converged generalized eigenmodes
`D psi_s = lambda_s M psi_s` (Phase390 dense branch, Phase378 shell rule),
the coupled second variation in `(b, chi)` has the mixed blocks
`2 delta_D[b] psi_s` (the VO-7 candidate mixed linearization blocks on
converged modes). Schur-complementing the fermion fluctuation gives the
fermion-induced carrier response operator (degenerate second-order
perturbation theory):

```
R^(s)_kl = Re< delta_D[e_k] psi_s, (D - lambda_s M)^+ delta_D[e_l] psi_s >
R        = sum over the 4-mode lowest nonzero shell
```

with the pseudo-inverse computed exactly in the dense generalized eigenbasis,
excluding the degenerate shell group. Rank, signature, and gauge-axis
fractions are computed with the same rules as Phase378/379 and compared to
the persisted Gram results. A pure-gauge diagnostic evaluates `v(X)^T R v(X)`
on all 84 Phase389 covariant-differential directions.

## Result: DIVERGES FROM THE GRAM STRUCTURE

- The action-derived response is near-FULL-rank: significant rank 146 (bg-a)
  and 141 (bg-b) of 156, with mixed signature (70+/76- and 69+/72-), versus
  the Gram's stable rank 3.
- The gauge-axis fractions are nearly ISOTROPIC: bg-a [0.334, 0.328, 0.337],
  bg-b [0.332, 0.336, 0.332], versus the Gram's [0.54, 0.002, 0.46]. There is
  no suppressed axis, and the argmin axis is not even stable across
  backgrounds (1 vs 2).
- Computation quality: response symmetry exact (asymmetry residual 0),
  retained energy denominators >= 8.4e-4 (well-conditioned), shell
  eigen-residuals <= 2.9e-13.
- Pure-gauge directions carry response up to ~30x the generic carrier scale,
  consistent with the candidate action not being gauge-invariant at this
  non-coupled background (the Phase389 obstruction and the bosonic-only
  background solve both contribute).

Interpretation: the rank-3 image and the suppressed gauge axis are
METRIC-DEPENDENT - they characterize the Hilbert-Schmidt pullback Gram, not
the second-order response of the candidate action. The Phase381/383/384
suppressed-axis blockers against the Phase307 W near-pass therefore describe
a property of the diagnostic metric, and the carrier-image question is
reopened on the action-derived side.

## Honest scope limits

- `(omega, psi_s)` is NOT a coupled critical point (omega was solved by the
  bosonic objective alone); this is a fixed-background response.
- `S_F` is the candidate bilinear, not a completed GU fermionic action; the
  omega-omega bosonic Hessian block is not assembled.
- Toy control branch; the response operator's absolute scale is small
  (~1e-11), though well-resolved relative to its own rank tolerance.

## Status

Fail-closed. No physical Hessian, observed namespace map, canonical axis
selector, W/Z/H source rows, or Phase201/Phase256 contract fields.
`canFillPhase201WzContract=False`.

## Reproduce

```bash
dotnet run --project studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001/Phase392CoupledMixedHessianFermionInducedResponseAudit.csproj
```

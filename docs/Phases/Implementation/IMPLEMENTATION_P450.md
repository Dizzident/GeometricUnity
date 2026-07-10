# Implementation P450 - Constraint-Effective-Potential HMC Probe

Phase450 executes the 2026-07-04 review board's ansatz-free experiment
(the successor made critical by phase449's diagonal-Hartree
gap-equation breakdown): the umbrella-sampled, WHAM-reconstructed
constraint effective potential U(Phi) of e^{-beta S_B} on the
lattice-canonical n=3 torus, under all FOUR binding conditions:
(i) gauge-invariant translation-invariant collective coordinate
(oSign lattice gauge; global-su(2) directions projected out to
2.9e-17); (ii) theta-Haar measure (per-vertex rotations, symmetric
Metropolis, axis-angle chart batteries at 1e-15); (iii) ergodicity
gates (tau_int-based N_eff >= 100 per window); (iv) theta=0 never a
verdict. Self-consistent window auto-placement from the per-member
tree curvature; per-arm unconstrained tadpole diagnostic; large-beta
tree-shape control column; two production runs (the second after a
recorded spring-rule softening 2.2 -> 2.0 that fixed the identity's
marginal 0.149-vs-0.15 overlap; overlaps then 0.28-0.30).

## Verdict: honestly INCONCLUSIVE-GATES-FAILED (single-well everywhere, null NOT claimed)

Both production runs agree on the observation: EVERY classification is
single-well-at-zero (identity control clean; sd2 Einsteinian
single-well in both runs; large-beta column single-well, tree-shape
Pearson 0.9996; no flat Maxwell bottom anywhere; hard batteries
2.8e-15/2.9e-15/4.0e-7 green; <e^-dH> ~ 1, virial ~ 0, acceptance
0.92-0.96 on all windows). But the parity-antisymmetry gate trips at
5.06 sigma on sd2 in run 2 (2.19 sigma in run 1) while the INDEPENDENT
unconstrained tadpole is consistent with zero (-0.04 +- 0.10) - an
UNATTRIBUTED WHAM stitching-error-model artifact class the build's own
smoke iterations had already encountered and partially fixed. Per the
phase445/446 discipline the phase records
verdictKind=inconclusive-gates-failed and does NOT claim the
symmetric-phase null. Runtime ~2.5 h per run.

## Named Next (queued; program halted at user instruction 2026-07-05)

ONE named item stands between this record and the non-perturbative
symmetric-phase null: the WHAM parity-asymmetry ERROR MODEL (the
stitching-accumulation variance is still understated at soft springs;
candidates: block bootstrap over windows, or an antisymmetrized
estimator U_odd built within single windows). When fixed and the gate
passes, the null upgrades the review-board frontier statement from
"no perturbative CW scale" to "no non-perturbative scale along
invariant rays at n=3" - the strongest internal statement available.
Nothing promoted.

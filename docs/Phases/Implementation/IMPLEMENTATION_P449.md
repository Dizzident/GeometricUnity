# Implementation P449 - Variational Gaussian Effective-Potential Probe

Phase449 executes the 2026-07-04 review board's #1 ranked experiment
(binding record: the restart prompt's READ-THIS-FIRST section): the
variational Gaussian (CJT-Hartree) effective potential - a rigorous
Feynman-Jensen UPPER BOUND on the constrained free energy, replacing
the retired positive-subspace saddle V_eff. Built by a worktree dev
agent from a full spec; smoke-validated fail-closed before production.

## Construction

Verbatim phase447 machinery (hardened theta solver with the
relative-or-absolute-floor point gate; joint FD Hessian + Jacobi with
eigenvectors; large-h directional T4 stencils at h=0.1 with Richardson
and offset-immunity batteries; same members/seeds/grid). New layer:
the T4 pair table over ALL modes above zeroTol (positive AND negative
- the Hartree lift of the negative modes is the mechanism under test),
the diagonal-Gaussian functional
V_G = S_B + (1/2) sum lambda_a sigma_a + (1/8) sum T4[a,b] sigma_a
sigma_b - (1/2) sum log sigma_a (beta=1 recorded), and the damped
fixed-point gap solver 1/sigma_a = lambda_a + (1/2) sum_b T4[a,b]
sigma_b with NO clamping - self-consistency failures are recorded
honestly per point (pre-registered verdict kind
gap-equation-breakdown). The diagonal restriction only loosens the
minimization, so the bound stays rigorous on the omega sector (exact
quartic); the theta-sector quartic truncation is a recorded workbench
convention. A basis-rotation honesty check at anchor points records
(ungated) the ansatz-basis dependence.

## Verdict: GAP-EQUATION BREAKDOWN (pre-registered outcome, recorded)

The diagonal-Gaussian family FAILS TO EXIST on 32 of 64 Einsteinian ray
points (m_a nonpositive at the prescribed init; converged points reach
residual <= 9.9e-9); the identity control converges everywhere
(trivial-origin); the anchor rotation check shows GENUINE ansatz-basis
dependence (a rotated basis converges where the eigenbasis start fails
at asd2's anchor). All numerical batteries green: theta gate 1.4e-9;
identity exact-quartic anchor 1.2e-7; Einsteinian Richardson 8.8e-3;
offset immunity entrywise 1.3e-6; gap-solver plumbing exact (1.1e-16);
descent battery monotone fraction 1.000. Runtime 21 min.

Physics reading: the cheapest rung of the non-perturbative ladder (the
diagonal CJT-Hartree bound) cannot hold the Einsteinian negative-mode
structure - the Hartree lift fails on half the ray. NOT a null, NOT a
candidate: the pre-registered third outcome. The scale question passes
intact to the phase450 constraint-EP HMC (ansatz-free, four binding
conditions), exactly as the review board's ranking anticipated.
verdictKind=gap-equation-breakdown;
einsteinianGaussianSaturationObserved=false;
hartreeSelfConsistentSolutionExistsEverywhere=false. Nothing promoted.

## Named Next

Per the review board and the 2026-07-05 task-force work plan: a
Gaussian NULL is strong rigorous-bound evidence against a scale; a
positive is mean-field-only and must be confirmed by the phase450
constraint-EP HMC (four binding conditions); a gap-equation breakdown
sharpens where the Gaussian family fails to exist. The parallel
workstreams WS1-WS4 proceed independently. Nothing promoted either
way.

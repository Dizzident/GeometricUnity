# Implementation P447 - Two-Loop Saturation Probe

Phase447 executes the beyond-one-loop lever named after phase446: the
genuine two-loop vacuum terms (figure-eight + sunset) built from the
joint action's third/fourth derivative tensors, which phase442's
degree-lift proved nontrivial. Design note committed post-phase446 at
studies/phase446_rg_scheme_dependence_resolution_probe_001/
PHASE447_TWO_LOOP_DESIGN.md.

## Construction

Verbatim phase443/446 machinery (same members/seeds/rays; Phase445
16-point grid); joint FD Hessian eigen-decomposed WITH eigenvectors;
two-loop terms contracted onto the positive-subspace propagator (the
recorded convention extending the one-loop positive-mode rule about
saddle backgrounds - physicistReviewPending). Deterministic
figure-eight over all positive pairs; sunset soft-mode-truncated (K=40,
IR-dominated 1/lambda^3 weight) with a K-sweep and full-deterministic
anchor evaluations; a RECORDED propagator soft-mode floor (default
1e-4 relative, swept {1e-5,1e-4,1e-3} - assembled free from cached
stencil tables) because unfloored near-zero modes make the 1/lambda^3
sum uncontrolled; stencil step h=0.1 LARGE BY DESIGN (S_B is exactly
quartic in omega so stencils are h-independent there; the first smoke
run proved h=5e-3 noise-dominated via the offset-immunity battery;
theta truncation controlled by the h-vs-h/2 Richardson battery).
Classification on the RAW V = S_B + V_1loop + V_2loop curve by the
strict-local-minimum rule - no fits (the phase446 lesson). An
alternative-convention arm (absolute-value propagator over all nonzero
modes) reruns one full Einsteinian ray; a convention-flipped verdict is
recorded as convention-dependent, not a candidate.

Structure result encoded in the batteries: S_B is an exact quartic in
omega (identity stencils exact to roundoff - the anchor battery) and
transcendental in theta (Ad = exp(ad_theta)) - resolving phase442's
"degree > 2" finding.

## Verdict: the two-loop lever is NON-PERTURBATIVE at the minimal scope

All numerical batteries green (theta 1.4e-9; identity exact-quartic
anchor 1.3e-7; Einsteinian Richardson 7.6e-4; offset immunity 2.9e-7);
every physics admissibility axis fails: max |V_2loop|/|V_1loop| =
1.6e3 (median 155), floor-sweep unstable, seed unstable,
propagator-arm unstable, identity control acquires a spurious minimum
(breakdown evidence, not an estimator defect).
resolutionKind=non-perturbative-or-convention-bound;
twoLoopVerdictAdmissible=false; twoLoopCandidate=false. With
phase443 (one loop) and phase446 (RG-fit artifact), the internal
no-platform program at the minimal mesh is EXHAUSTED THROUGH TWO
LOOPS. Two fail-closed smoke runs preceded production: the first
proved the scouted h=5e-3 stencil step noise-dominated (offset
battery failed at 135%), fixed via the omega-quartic large-h
exactness; the second forced the soft-floor convention + sweep and
the perturbativity gate. Runtime 42 min.

## Named Next

If no two-loop saturation: the no-platform internal program at the
minimal mesh is exhausted through two loops; the frontier is the two
phase444 unlock projects (scoping memo:
studies/phase444_mode_volume_scaled_saturation_probe_001/UNLOCK_SCOPING.md,
recommendation: adjoint path first), or a source anchor. If a
convention/seed-stable candidate: the negative-mode/saddle convention
requires physicist adjudication before any further step. Nothing
promoted either way.

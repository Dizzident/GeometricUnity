# Implementation P440 - Coupled Background-Condensate Fixed-Point Probe

Phase440 decides the final link of the candidate chain (430 -> 439 ->
438): does the joint (t8, Sigma_vec) variational system have a
self-consistent interior fixed point? IT DOES NOT (honest negative):

- The condensate SATURATES the fermionic runaway (the gap shifts the IR
  cutoff eps -> sqrt(eps^2 + Sigma^2); moderate-t8 slope depressed from
  -2.65 to -0.42 at Sigma=4) but CANNOT STOP it: at large t8 the
  background gaps out the condensing IR modes, the joint-optimal Sigma
  falls to zero, and the asymptotic negative slope resumes (-7.28).
- Joint minimization over 48 (L, content, g2) configurations: 29 trivial
  (t8*=0), 19 runaway (boundary escape), 0 interior.
  jointFixedPointExists=False; backgroundSelfGenerates=False.
- The boundary decomposition pins the failure: dW/dt8 = bosonic(+) +
  fermionic(-) with net negative (fundamental -0.50, derived -4.80) -
  the WORKBENCH BOSONIC SECTOR is too weak. This is the third
  independent convergence (with Phases 435 and 436) onto the physical
  VO-6/VO-7 bosonic structure or a source anchor as the requirement.

Batteries: Phase430 cross-anchor exact to 3.1e-4; t8=0 reproduces
Phase438; dense vs closed form 1.6e-13; verified descent; symmetry
exact. Mean-field, workbench-model, convention, and candidate-only
boundaries recorded; no GeV promotion; no contract fill. Runtime ~0.8 s.

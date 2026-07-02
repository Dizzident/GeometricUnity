# Implementation P439 - Gap-Equation-in-Lambda_8-Background Channel-Steering Probe

Phase439 combines the Phase431 background machinery with the Phase438 gap
equation to decide whether a lambda_8 background steers the condensate
channel that Phase438 found singlet-dominated. IT DOES:

- STRICT free-energy steering margins at every t8 > 0 (2.2e-5 at t8=0.5
  rising monotonically to 5.6e-3 at t8=2; zero at t8=0 as required, since
  the singlet is in the feasible set).
- The free-per-color gap equation decouples exactly (closed form
  lambda^2 = eps_c(k, t8)^2 + Sigma_c^2 verified dense; the same doubler
  exclusion as Phase438 so the t8=0 limit reproduces Phase438 exactly),
  and the induced pattern is Sigma_1 = Sigma_2 != Sigma_3 - AUTOMATICALLY
  su(2)xu(1)-ALIGNED because lambda_8's color eigenvalues are (x, x, -2x).
- Splitting ratio monotone in t8 (0 / 0.0016 / 0.0064 / 0.0274); the
  per-color critical couplings shift with the background (color-3
  g2_crit falls to 0.084 at t8=1 - the background makes condensation
  EASIER in the split channel at moderate t8).
- The exponential transmutation signature survives the background
  (fundamental R^2 in [0.81, 0.97]).

CANDIDATE-LEVEL LOOP CLOSED: loop landscape selects the lambda_8
direction (Phase430) -> lambda_8 background steers the gap equation
(this phase) -> su(2)xu(1)-aligned dynamical mass pattern with a
transmutation scale (Phase438 mechanism). HONEST BOUNDARIES: t8 remains
a recorded candidate background (its dynamical derivation is the open
link); mean-field; coupling normalization is a workbench convention;
scale and pattern candidate-only; no GeV promotion; no contract fill.
Runtime ~0.4 s.

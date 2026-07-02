# Implementation P438 - Self-Consistent Condensate Gap-Equation Probe

Phase438 tests the one mechanism class never probed by Phases
405/410/418/428/430-436: self-consistent backreaction (Gross-Neveu-style
gap equation), where the condensate the fermion loop prefers feeds back
into the spectrum that generates it. Workbench: 2D lattices L in
{4,8,16}, doubler-excluded convention, mass channel via Gamma = sz (x) I2
(anticommutes with the kinetic term exactly, so lambda^2 = eps_k^2 +
Sigma^2 in closed form), su(3) singlet and hypercharge (lambda_8)
channels, fundamental and derived contents.

## Results

- FIRST INTERNAL DYNAMICAL-SCALE GENERATION OBSERVED: nontrivial gap
  solutions exist above g2_crit(L), and g2_crit FALLS WITH VOLUME
  (0.133 -> 0.078 -> 0.055 for L = 4/8/16; 1/ln L fit R^2 = 0.998) - the
  transmutation trend (condensation at arbitrarily weak coupling as
  L -> infinity). ln Sigma* vs 1/g2 is approximately linear (slope
  ~ -0.27 at L=16, R^2 ~ 0.83-0.90) - the exponential scale-law
  signature, moderate fit quality recorded honestly.
- CHANNEL COMPETITION: the scalar singlet condensate has lower free
  energy than the hypercharge-direction condensate at EVERY sampled
  coupling (hyperchargeChannelCompetitiveWithSinglet=False). The
  mechanism supplies a SCALE but not the BREAKING DIRECTION - direction
  selection (Phase430) and scale generation live in different
  mechanisms.

## Boundaries

Mean-field approximation; the four-fermion coupling normalization is a
recorded workbench convention (not source-derived); every scale ratio is
candidate-only; no GeV promotion; no contract fill. Batteries: gamma
anticommutation exact; dense vs closed form 1.5e-14; gap-derivative
identity 4.4e-16; free energy lowered by condensation; Sigma -> 0
continuity.

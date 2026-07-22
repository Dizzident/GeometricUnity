# Implementation P537 — deterministic leapfrog correctness and stability audit

Phase537 is Amendment A23's deterministic, RNG-free audit of the scalar
leapfrog used by the reduced interacting Phase534 control. The study binds the
Phase533 contract and result, the Phase534 contract, implementation, and
result, and the Phase535 adjudicator result by exact hash.

The preregistered polynomial is the exact binary64 coefficient triple emitted
by Phase534. The implementation checks those bit patterns against the bound
Phase534 result and checks that the bound implementation still contains the
frozen potential and analytic-gradient formulas.

Three independent deterministic checks then run:

1. Symmetric finite differences test the analytic gradient on eleven frozen
   states.
2. A fixed state/momentum grid is integrated forward and backward on four
   step sizes at fixed trajectory length.
3. RMS absolute energy error is required to improve with the preregistered
   order and ratio bounds under three successive step halvings.

The implementation independently hardcodes and exact-validates every frozen
state, momentum, finite-difference step, tolerance, trajectory length,
step-size/count ladder, and energy-order bound before treating the contract as
valid. Every terminal in the frozen taxonomy is serialized; a negative result
is not suppressed merely because it differs from the preregistered expected
current outcome.

The positive terminal means only that these deterministic checks pass on this
one-dimensional polynomial and frozen grid. It does not explain Phase534's
stochastic failures or validate sampling, mixing, the full registered action,
or production. Every downstream authority remains false, external review is
pending, and `promotedPhysicalMassClaimCount=0`.

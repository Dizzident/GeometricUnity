# Phase439: Gap-Equation-in-lambda_8-Background Channel-Steering Probe

Phase438 solved the mean-field gap equation on the 2D doubler-excluded lattice
and found it generates a dynamical scale, but the **scalar singlet** condensate
beat the hypercharge-direction condensate at every coupling: scale and breaking
direction lived in different mechanisms. Phase431 found that a constant lambda_8
background `omega_bg = t8*(lambda_8/2)` breaks the su(3) degeneracy of the
fermion spectrum.

**The question this phase decides:** does solving the gap equation *inside* the
lambda_8 background **steer the condensate channel** - making a
symmetry-breaking (non-singlet) channel preferred, and splitting the condensate
**by color** (a color-dependent gap = dynamically generated su(3)-breaking mass
structure)?

## Construction

Workbench conventions exactly as Phase438: 2D `L x L` lattice, `L` in
`{4, 8, 16}`; 4-dim spinors; naive central-difference Dirac; su(3) Gell-Mann;
contents fundamental (1 copy) and derived-4x (4 copies). The Phase431 background
`omega_bg = t8*(lambda_8/2)` is minimally coupled on **both** lattice
directions, so the color-`c` block is `gamma_mu (x) (s_mu + t8 u_c)` and the
color-`c` dispersion is `eps_c(k,t8)^2 = (s1+t8 u_c)^2 + (s2+t8 u_c)^2`. Colors
1,2 have `u_c = 1/(2 sqrt3)`; color 3 has `u_c = -1/sqrt3`.

The mass insertion `Gamma = sz (x) I2 (x) C` anticommutes with the kinetic
**and** background gamma_mu terms, and diagonal `C` commutes with `U8`, so
`H^2 = (kinetic+background)^2 + Sigma^2 C^2` exactly, giving
`lambda^2 = eps_c(k,t8)^2 + Sigma^2 c_c^2` per color.

**IR convention:** exclude momenta with `s1=s2=0` (the `t8=0` zero-dispersion
sector); the **same** excluded set is kept at every `t8`, so the `t8=0` limit is
exactly Phase438.

**The key novelty - the free-diagonal minimization:** minimize the free energy
over **independent per-color gaps** `(Sigma_1, Sigma_2, Sigma_3)`,
`W(Sigma_vec) = sum_c [ Sigma_c^2/(2 g2) - copies*(Ns/2)/Vol sum_k' log(eps_c^2 + Sigma_c^2) ]`.
This decouples per color: each `Sigma_c` solves its own gap equation with the
background-shifted dispersion. Because colors 1,2 share `u_c` while color 3
differs, the induced pattern `Sigma_1 = Sigma_2 != Sigma_3` is an
su(3)->su(2)xu(1)-aligned dynamical mass.

## Results

- **Channel steering (`backgroundInducesChannelSteering = True`).** The
  free-diagonal optimum beats the singlet-constrained (all-equal) optimum by a
  strictly positive free-energy margin at every `t8 > 0` (min sampled margin
  `1.6e-6`), while the margin is exactly zero at `t8 = 0` (max `5.0e-14`). The
  singlet is feasible for the free-diagonal, so the margin is `<= 0` by
  construction; the strict positive margin at `t8 > 0` is the steering verdict.
- **su(2)xu(1) alignment (`dynamicalMassPatternAlignsWithSu2U1 = True`).** The
  free minimum has `Sigma_1 = Sigma_2 != Sigma_3` to machine precision
  (`|Sigma_1 - Sigma_2| = 0`), with color-splitting ratio up to `0.153` over the
  grid.
- **Monotone splitting (`colorGapSplittingMonotoneInBackground = True`).** The
  ratio `(Sigma_1-Sigma_3)/(Sigma_1+Sigma_3)` rises monotonically with `t8`
  across every content/lattice/coupling (fundamental L=8 g2=5:
  `0 -> 0.0016 -> 0.0064 -> 0.0274`).
- **Transmutation survives (`transmutationSignatureSurvivesBackground = True`).**
  `ln Sigma_c ~ -c/g2` per color: negative slope in every config, fundamental
  `R^2` in `[0.810, 0.968]`.
- **Critical coupling (recorded).** The larger-`|u_c|` color (color 3) reaches a
  lower critical coupling than color 1 at moderate `t8`
  (`backgroundLowersColorCriticalCouplingAtModerateT8 = True`): fundamental L=8
  `gc3 = 0.233 -> 0.195 -> 0.084` for `t8 = 0, 0.5, 1.0`. At large `t8 = 2.0`
  `gc3` rises again to `0.246` (the shift moves modes away from the IR) - an
  honest non-monotonicity, recorded in `criticalCouplingRows`. There is an exact
  internal symmetry `Sigma_3(t8) = Sigma_1(2 t8)` because `u_3 = -2 u_1`.

## Batteries (all pass)

- Gamma anticommutation exact (residual 0); `sum u_c^2 = 1/2`.
- Closed form `eps_c^2 + Sigma^2 c_c^2` vs a dense L=4 Dirac solve **with the
  background**: max residual `1.6e-13` across singlet, hypercharge, and general
  diagonal `C` at `t8 = 0, 1, 2`.
- `t8 = 0` Phase438 consistency: singlet and hypercharge single-Sigma channels
  reproduce the Phase438 gap solutions to `1.8e-15` / `7.1e-15` (`<= 1e-10`).
- Free-diagonal alignment `Sigma_1 = Sigma_2` exact (`|S1-S2| = 0`).

## Honest boundaries

`t8` is a **recorded candidate** background parameter, not dynamically derived
(the Phase430-chain caveat); `backgroundParameterT8DynamicallyDerived = False`.
`gapEquationIsMeanFieldApproximation = True`;
`fourFermionCouplingNormalizationIsRecordedConvention = True`;
`scaleAndPatternAreCandidateOnly = True`; `noGevPromotion = True`. This phase
decides only that a candidate lambda_8 background **can** steer the condensate
channel and align its pattern on the workbench; it closes no GeV scale gap and
promotes nothing. No Phase201 or Phase256 field is filled
(`canFillPhase201WzContract = canFillPhase201HiggsContract = canFillPhase256ObservedFieldExtractionContract = False`).

## Run

```bash
dotnet run -c Release --project studies/phase439_gap_equation_lambda8_background_channel_steering_probe_001/Phase439GapEquationLambda8BackgroundChannelSteeringProbe.csproj
```

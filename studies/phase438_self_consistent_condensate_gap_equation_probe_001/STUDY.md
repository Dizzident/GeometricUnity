# Phase438: Self-Consistent Condensate Gap-Equation Probe

Every prior landscape phase (405/410/418/428/430-436) treated backgrounds as
EXTERNAL rays and asked whether the one-loop landscape has a finite minimum.
This phase tests the one mechanism class never probed: **dimensional
transmutation via a self-consistent gap equation** (Gross-Neveu-style), where
the condensate the fermion loop prefers feeds back into the fermion spectrum
that generates it.

## Construction

Workbench conventions as Phases 428/430/435: 2D `L x L` lattice, `L` in
`{4, 8, 16}`; 4-dim spinors; naive central-difference Dirac; IR
doubler-exclusion convention (zero-dispersion modes dropped); su(3) Gell-Mann;
contents fundamental (1 copy) and derived-4x (4 copies).

The condensate uses a gamma_5-like mass insertion `Gamma = sz (x) I2` that
anticommutes with the kinetic `gamma1 = sx (x) I2`, `gamma2 = sy (x) I2`, so
`H = D_kin + Sigma*Gamma` has `H^2 = D_kin^2 + Sigma^2` exactly on every mode:
eigenvalues `lambda^2 = eps_k^2 + Sigma^2` (scalar singlet) or
`eps_k^2 + Sigma^2 u_c^2` (lambda_8 channel, `u_c` the color eigenvalues of
`lambda_8/2`, `sum u_c^2 = 1/2`). The gap equation follows from `dW/dSigma = 0`
with `W(Sigma) = Sigma^2/(2 g2) - (1/Vol) sum_k' logdet H(Sigma)`. The
four-fermion coupling `g2` is scanned over 20 log-spaced values in `[0.1, 10]`;
its physical normalization is fixed only up to the workbench `kappa_B` and is a
recorded convention.

## Results

- **The gap equation has nontrivial solutions** above a finite critical
  coupling `g2_crit(L)` for every content and channel. This is the first
  mechanism class that generates a scale on the workbench at all.
- **`g2_crit(L)` falls with volume** (singlet/fundamental
  `0.1333 -> 0.0777 -> 0.0549`), fitting `~1/lnL` with `R^2 = 0.998` and a
  near-zero intercept (`-0.026`), and a power law `~L^-0.64` - the
  transmutation trend expected as the 2D IR sum diverges. It stays finite on
  the doubler-excluded lattice.
- **`ln Sigma*` is approximately linear in `1/g2`** (the exponential scale law
  `Sigma* ~ Lambda*exp(-c/g2)`) with negative slope in all 12 configs and
  `R^2 = 0.83-0.99` for the fundamental-content channels; quality degrades to
  `R^2 ~ 0.77` for derived-4x, where the negligible `g2_crit` puts the whole
  grid deep in the broken phase (`Sigma* ~ sqrt(g2)`).
- **Channel competition:** the scalar singlet wins the free-energy competition
  over the lambda_8 hypercharge channel at every sampled coupling
  (e.g. L=16, fundamental, g2=5: singlet `dW = -19.18` vs hyper `dW = -8.02`).
  `hyperchargeChannelCompetitiveWithSinglet = False`.

## Batteries (all pass)

- Gamma anticommutation exact (residual 0).
- Closed-form spectrum vs dense operator, L=4: singlet `1.5e-14`, hyper
  `8.0e-15` (<= 1e-10).
- Gap function = normalized `dW/dSigma` identity residual `4e-16`, and
  bisected `Sigma*` is a zero of `f` (<= 1e-8).
- Condensation lowers the free energy `W(Sigma*) < W(0)`.
- `Sigma -> 0` continuity: monotone onset from `g2_crit`, zero below.

## Honest boundaries

`gapEquationIsMeanFieldApproximation = true`;
`fourFermionCouplingNormalizationIsRecordedConvention = true`;
`scaleRatioIsCandidateOnly = true`; `noGevPromotion = true`. This phase
establishes only that the self-consistent-backreaction MECHANISM CLASS can
generate a scale on the workbench; it does NOT close the GeV scale gap. No
Phase201 or Phase256 field is filled; nothing is promoted.

## Run

```bash
dotnet run -c Release --project studies/phase438_self_consistent_condensate_gap_equation_probe_001/Phase438SelfConsistentCondensateGapEquationProbe.csproj
```

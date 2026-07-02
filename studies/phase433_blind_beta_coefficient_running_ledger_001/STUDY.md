# Phase433: Blind Beta-Coefficient Running Ledger

Running extension of the Phase429 target-blind dimensionless-ratio ledger.
Phase429 recorded the scale-FREE surface fixed blind by the internal
embedding chain (`tan^2 theta_W = 3/5`, `sin^2 theta_W = 3/8` at the
unification point). One-loop beta coefficients are the next scale-free
datum: pure group theory of the derived matter content, with no measured
value, no dimensionful scale, and no comparison to observation anywhere.
Derivation is strictly separated from comparison exactly as in Phase429
(`measuredElectroweakValuesConsulted=false`, no residual computed, no
Phase201/Phase256 fill).

## Convention (stated once, used consistently)

- RG equation: `d g_i / d ln mu = - b_i g_i^3 / (16 pi^2)`.
- Sign meaning: **positive `b_i` means asymptotic freedom** (the coupling
  weakens toward the ultraviolet).
- Integrated running (derived from the above, not assumed):
  `1/g_i^2(mu) = 1/g_GUT^2 - (b_i/(8 pi^2)) ln(M/mu)`. For an
  asymptotically free group the coupling grows toward the infrared. (A "+"
  running term would correspond to the opposite beta-function sign
  convention; the asymptotic-freedom-positive convention is fixed
  throughout and the running relation is derived from it.)
- One-loop coefficient formula:
  `b = (11/3) C_2(G) - (2/3) sum_Weyl T(R) - (1/3) sum_complex-scalar T(R)`.
- GUT normalization `g_1^2 = (5/3) g_Y^2`, so the U(1) index is
  `(3/5) sum Y^2`.

## Derived matter content (Phase404 pattern)

Per-family Weyl content of the 16, with the derived hypercharges:
`Q:(3,2,1/6)`, `u^c:(3bar,1,-2/3)`, `d^c:(3bar,1,1/3)`, `L:(1,2,-1/2)`,
`e^c:(1,1,1)`, `nu^c:(1,1,0)`. `nu^c` is a total singlet and contributes
nothing. The explicit index sums per family come out uniform:

- SU(3)_c fermion index sum = `2`.
- SU(2)_L fermion index sum = `2`.
- `sum Y^2 = 10/3`, so the GUT-normalized U(1) index `= (3/5)(10/3) = 2`.

Hence every gauge group receives the same family-universal fermion
contribution `(2/3)*2 = 4/3`.

## Beta coefficients (exact rationals)

Gauge (pure Yang-Mills) part: `b_3` gets `(11/3)*3 = 11`, `b_2` gets
`(11/3)*2 = 22/3`, `b_1` gets `0`. So without the Higgs, for `n_f`
families:

- `b_3 = 11 - (4/3) n_f`
- `b_2 = 22/3 - (4/3) n_f`
- `b_1 = -(4/3) n_f`

Conditional Higgs (one complex doublet `(1,2,1/2)`): adds `-1/6` to `b_2`
and `-(3/5)(1/6) = -1/10` to `b_1`, nothing to `b_3`. Recorded separately
and marked conditional (Phases 405/410/418/428 closed the internal
selection mechanisms; the phase430-chain experiments are pending).

| n_f | Higgs | b_3 | b_2 | b_1 |
|-----|-------|------|------|-------|
| 1 (derived) | no | 29/3 | 6 | -4/3 |
| 1 (derived) | yes | 29/3 | 35/6 | -43/30 |
| 3 (imported) | no | 7 | 10/3 | -4 |
| 3 (imported) | yes | 7 | 19/6 | -41/10 |

`n_f = 3` with Higgs reproduces the standard SM one-loop set
`(b_3, b_2, b_1) = (7, 19/6, -41/10)` in this asymptotic-freedom-positive
convention. **Family multiplicity is NOT derived**: `n_f = 1` is the
derived single family; `n_f = 3` is an imported observed structural
parameter, marked as such on every row.

## Running ledger (still no measured values)

With `1/g_i^2(mu) = (1/g_GUT^2)(1 - b_i x)` where the dimensionless running
variable is `x = (g_GUT^2/(16 pi^2)) ln(M^2/mu^2) = (g_GUT^2/(8 pi^2)) ln(M/mu)`,
and `sin^2 theta_W = g'^2/(g_2^2 + g'^2)` with `g'^2 = (3/5) g_1^2`, the
one-parameter family is the exact rational function

```
sin^2(x) = 3 (1 - b_2 x) / [8 - (5 b_1 + 3 b_2) x].
```

At `x = 0` this is `3/8` on every row. The exact linear slope is
`15 (b_1 - b_2) / 64`, which is **family-independent** (each family
contributes equally to `b_1` and `b_2`, so `b_1 - b_2` is fixed only by the
Higgs conditional):

- without Higgs: `b_1 - b_2 = -22/3`, slope `= -55/32`;
- with Higgs: `b_1 - b_2 = -109/15`, slope `= -109/64`;

i.e. `sin^2(x) = 3/8 - (55/32) x + O(x^2)` (no Higgs) or
`3/8 - (109/64) x + O(x^2)` (with Higgs). The surface is symbolic in `x`
only; it is NOT evaluated at any physical scale because no unification
scale or input coupling exists in any source.

## Missing comparison lineage (fail-closed)

Every row lists what it lacks before any comparison would be legal: the
unification scale `M`, two-loop and higher running, threshold/matching
corrections, the scheme, the actual inter-scale matter content, and an
input coupling `g_GUT`. Zero of these are source-defined. No measured value
was consulted, no comparison performed, no Phase201/Phase256 field filled.

## Precursors

- Phase404 summary `guEmbeddingChainCouplingRatioEnumerationPassed=true`
  and `familyPatternDerived=true`.
- Phase429 summary `targetBlindDimensionlessRatioLedgerPassed=true` and
  `derivationComparisonSeparationMaintained=true`.

## Run

```bash
dotnet run -c Release --project studies/phase433_blind_beta_coefficient_running_ledger_001/Phase433BlindBetaCoefficientRunningLedger.csproj
```

Outputs `output/blind_beta_coefficient_running_ledger.json` and
`output/blind_beta_coefficient_running_ledger_summary.json`;
`applicationSubjectKind = "blind-beta-coefficient-running-ledger"`.

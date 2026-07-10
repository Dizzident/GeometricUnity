# Phase451: Two-Loop Gauge-Coupling Unification Ledger

2026-07-05 task-force **WS1** (referee-certified step 1): predict
`sin^2 theta_W(m_Z)` from the DERIVED `3/8` boundary condition
(`alpha_1 = alpha_2` at the unification point — the blind Phase404 kernel)
plus the standard RG running of the SM content row. The observed
`sin^2(m_Z)` is **never** imported into the derivation: the referee ruled
the earlier one-loop "gap closure" (`sqrt(5/8) -> 0.877` vs `0.881`)
CIRCULAR because it used the measured `sin^2(m_Z)` as its IR endpoint. The
honest inputs are `alpha_em(m_Z)` (electroweak normalization) plus exactly
ONE additional low-energy coupling, `alpha_s(m_Z)`; the unification system
is then solved for `{mu*/m_Z, sin^2(m_Z)}`.

## Derived inputs (no measured values)

- Boundary: `sin^2 = 3/8` at `mu*`, equivalent to `alpha_1(mu*) = alpha_2(mu*)`
  (Phase404 blind kernel; Phase429 ledger row). Verified as a **battery**,
  not assumed: `sin^2(mu) = 3 y_2/(5 y_1 + 3 y_2)` evaluated at the found
  crossing equals `3/8` to `< 1e-10` at both loop orders.
- One-loop b's recomputed as exact rationals from the Phase404 family
  content (verbatim Phase433 group theory) and matched **exactly** against
  Phase433's committed witness row (`n_f = 3` + Higgs, AF-positive):
  `(b_3, b_2, b_1) = (7, 19/6, -41/10)`.
- Two-loop textbook SM gauge `b_ij` (standard convention `b_i^std = -b_i^AF`),
  GUT-normalized U(1), every coefficient documented in the JSON:

  |        | g1      | g2    | g3   |
  |--------|---------|-------|------|
  | **b1j** | 199/50 | 27/10 | 44/5 |
  | **b2j** | 27/50  | 35/6  | 12   |
  | **b3j** | 11/10  | 9/2   | -26  |

  Witnesses: `-b_33 = 26 = 102 - 38*6/3` (QCD beta_1 at n_f = 6);
  GUT-normalization ratios `b_12/b_21 = 5`, `b_13/b_31 = 8` exact.
  Two-loop Yukawa traces are omitted (declared truncation).

## Declared comparison imports (Phase429-style strict separation)

`alpha_em^{-1}(m_Z) = 127.955` and `alpha_s(m_Z) = 0.1179` (MS-bar,
PDG-style, declared exact, source-labeled) are the ONLY measured numbers the
computation consumes; `measuredElectroweakValuesConsulted = true` only
inside the `comparisonImports` block. `sin^2(m_Z)_observed = 0.23122`
appears ONLY as the falsification target and in the comparison-side
`alpha_3` non-closure diagnostic — the predictions are computed strictly
before it is declared.

## Computation

RG system in `y_i = 1/alpha_i`, `t = ln(mu/m_Z)` (AF-positive one-loop
convention per Phase433):

```
dy_i/dt = + b_i^AF/(2 pi) - sum_j b_ij^std / (8 pi^2 y_j)
```

Triple closure `y_1(t*) = y_2(t*) = y_3(t*)` with the two IR inputs, solved
for `{sin^2(m_Z), t*}` (bisection over `s2`, crossing refined to machine
precision; RK4 fixed step `h = 1/256`, segment-aligned at content
breakpoints).

- **One loop, closed form**: `s2 = (3/5 + k S/A)/(8/5 + k)` with
  `k = (b_2 - b_1)/(b_3 - b_2) = 218/115` exactly.
- **Two loops, numeric**: RK4, both directions cross-checked.

## Results (all batteries green; runtime < 1 s)

| quantity | one loop | two loops |
|----------|----------|-----------|
| `sin^2(m_Z)` predicted | **0.207589** | **0.210637** |
| `mu*/m_Z` (dimensionless) | 7.42e12 | 4.58e12 |
| `1/alpha_GUT` | 41.498 | 41.364 |

- Referee correctness witness reproduced: one-loop `0.2076` (`0.208` at 3
  figures). Two-loop shift `+0.003049`.
- Threshold/matching band (matching point varied in `[m_Z/2, 2 m_Z]` with
  one-order-lower input evolution + step EW thresholds decoupling the top
  Weyl pair / Higgs doublet below `mu_th <= 2 m_Z`; no particle mass
  imported): **1.79e-4** at two loops (quadrature 2.02e-4). The strict
  one-loop prediction is matching-scale invariant (autonomous relabel), so
  its truncation proxy is the two-loop shift itself.
- **Falsification comparison**: predicted − observed =
  `0.210637 - 0.23122 = -0.02058`, i.e. **|gap| ≈ 115x the threshold band**.
  Comparison-side triple-unification tension at the observed-`sin^2`
  crossing: `1/alpha_3 - 1/alpha_(1=2) = -5.576` (one loop) / `-4.907`
  (two loops) in `1/alpha` units.

## Verdict

`tension-persists-quantified` — the SM content row does NOT unify to the
observed weak angle at two loops; the two-loop correction moves 0.2076 to
0.2106, closing only ~13% of the 0.0236 one-loop gap. Per the referee's
framing this quantifies the textbook GUT miss for the derived-plus-imported
SM content row; reaching ~0.231 within threshold error would require
intermediate content the observerse does not yet define.

## Mandatory nonclaims

- **GUT-generic**: even a within-band closure would confirm generic grand
  unification of the SM content row, NOT Geometric Unity — nothing here
  distinguishes GU until the observerse's intermediate matter content is
  defined.
- No absolute scale: the unification scale is the dimensionless ratio
  `mu*/m_Z`; the GeV display is illustrative-with-declared-anchor
  (`m_Z = 91.1876 GeV`) and gated off from promotion.
- `n_f = 3` is an imported structural parameter; the Higgs doublet is a
  conditional content assumption (Phase433 provenance rows apply).
- Referee-corrected hierarchy figure recorded as context only:
  `b*alpha_GUT = 2 pi/ln(M_Pl/m_W) = 0.159` (used nowhere).
- No Phase201/Phase256 fill; `canFill*`/`routePromotes*` all false.

## Batteries (all gated, fail-closed)

1. Phase433 committed SM witness row matched exactly (string-level).
2. Group-theory recomputation of SM / no-Higgs / top-removed rows exact
   (`b_3(top-removed) = 23/3` = QCD `n_f = 5` witness).
3. Two-loop matrix witnesses (QCD beta_1, GUT-normalization ratios).
4. Referee 0.2076 witness.
5. One-loop closed form vs numeric: `6e-14` (gate 1e-10).
6. RK4 step-halving Richardson: `2.1e-13` on couplings, `0` on `s2`
   (gate 1e-8).
7. Up-down round trip `3.6e-15`; exact-unified downward recovery of the
   imposed IR inputs `3e-16` (gate 1e-9).
8. Boundary identity `sin^2(mu*) = 3/8` dev `0` (gate 1e-10).

## Precursors

- Phase404 `guEmbeddingChainCouplingRatioEnumerationPassed` + `familyPatternDerived`.
- Phase429 `targetBlindDimensionlessRatioLedgerPassed` + separation flag.
- Phase433 `blindBetaCoefficientRunningLedgerPassed` + `exactRationalArithmeticVerified`.

## Run

```bash
dotnet run -c Release --project studies/phase451_two_loop_unification_ledger_001/Phase451TwoLoopUnificationLedger.csproj
```

Outputs `output/two_loop_unification_ledger.json` and
`output/two_loop_unification_ledger_summary.json`;
`applicationSubjectKind = "two-loop-unification-ledger"`; terminal status
`two-loop-unification-ledger-tension-persists-quantified`.

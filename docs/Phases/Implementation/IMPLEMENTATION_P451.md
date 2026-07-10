# Implementation P451 - Two-Loop Unification Ledger (Task-Force WS1)

Phase451 executes the 2026-07-05 task-force's WS1 (binding record: the
restart prompt's PARALLEL WORK PLAN): the two-loop gauge-coupling
unification ledger, the referee-certified falsifiable test that
replaced the circular one-loop "gap closure".

## Construction

DERIVED INPUTS (no measured values): the blind Phase404 kernel boundary
sin^2 = 3/8 (== alpha_1 = alpha_2 at mu*) and the Phase433
exact-rational SM content row (b3,b2,b1) = (7, 19/6, -41/10)
(witness-matched string-exactly against the committed Phase433 JSON)
plus the textbook SM two-loop gauge b_ij (GUT-normalized; per-
coefficient documentation; QCD beta_1 witness). DECLARED COMPARISON
IMPORTS (phase429-style strict separation): alpha_em(m_Z) = 1/127.955,
alpha_s(m_Z) = 0.1179; the observed sin^2(m_Z) = 0.23122 enters ONLY
the falsification comparison. Solve {mu*, sin^2(m_Z)} from the derived
boundary + the two IR couplings: closed-form at one loop, RK4 at two
loops (step-halving/round-trip/downward-recovery batteries at 1e-13
to 1e-16). Honest threshold band via one-order-lower input evolution
(the naive matching-scale variation is an exact relabel of the
autonomous system - delta == 0, a dishonest band; documented).

## Verdict: TENSION PERSISTS, QUANTIFIED

sin^2(m_Z) predicted = 0.207589 (one loop; reproduces the referee's
0.2076 witness) / 0.210637 (two loops). Gap to observed 0.23122 =
-0.02058 = ~115x the honest threshold band (1.79e-4):
twoLoopClosesWithinThresholdBand=false. Triple-unification tension
(comparison side): 1/alpha_3 mismatch -5.576 (1L) / -4.907 (2L).
mu*/m_Z = 4.58e12 (dimensionless; GeV display gated
illustrative-with-declared-anchor). The two-loop correction moves the
prediction the right direction but closes only ~13% of the gap: the
DERIVED SM-content running from the 3/8 boundary is quantitatively
falsified as-is, sharpening the demand for observerse INTERMEDIATE
CONTENT (new thresholds between m_Z and mu*) - which is exactly what
the task force predicted this ledger would measure. Even a closure
would be GUT-generic (mandatory nonclaim). Corrected hierarchy figure
b*alpha_GUT = 0.159 recorded context-only. Nothing promoted; no
Phase201/Phase256 field filled. Runtime ~0.8 s.

## Named Next

The ledger is re-runnable the moment any observerse intermediate
content row (new fermion/scalar thresholds) is source-defined: add the
row, rerun, and the same falsification comparison decides it. This is
now the standing quantitative filter for ANY proposed GU matter
content.

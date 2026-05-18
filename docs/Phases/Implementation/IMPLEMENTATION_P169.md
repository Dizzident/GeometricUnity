# Implementation P169

P169 checks whether the source-shape law route from P167 can become scientifically defensible.

The experiment evaluates source-shape features on two target-independent Phase12 source backgrounds:

- `bg-phase12-bg-a-20260315212202`
- `bg-phase12-bg-b-20260315212202`

It checks whether the W/Z correction ratio for each predeclared law is stable across the two backgrounds. Passing this stability check is necessary but not sufficient for prediction promotion, because a law also needs analytic derivation and downstream target validation.

## Result

Terminal status: `source-shape-law-stability-failed-for-p167-law`.

The P167 best law was `l1`. Its W/Z correction ratio is not stable across the sibling backgrounds:

- bg-a ratio: `0.8903652892765141`
- bg-b ratio: `0.9741149229972588`
- relative spread: `0.0898369778015613`
- tolerance: `0.05`

The best stable law was `inverse-sqrt-linf`, with relative spread `0.016929899914731133`, but it was not the P167 best predictive diagnostic law and is not derivation-backed. The source-shape route therefore remains blocked.

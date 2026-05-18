# Implementation P170

P170 attempts W/Z absolute predictions using only P169-stable source-shape laws.

The phase excludes P167's unstable `l1` law even though it was the best selected-background diagnostic. This prevents promotion from a target-like feature search and tests whether a stable law can produce a validated prediction.

## Result

Terminal status: `stable-source-shape-prediction-failed-not-promoted`.

Stable laws tested:

- `inverse-sqrt-linf`
- `sqrt-l1`

Best stable-law attempt: `sqrt-l1`.

- raw-amplitude gate: passed
- common-scale spread: `0.06928198950410719`
- common-scale gate: failed
- target comparison: failed
- derivation-backed: `false`
- promotion allowed: `false`

Diagnostic predictions:

- W: `87.29249661562896 GeV`, sigma residual `520.5486177164629`
- Z: `92.41109791672241 GeV`, sigma residual `611.5489583612045`

No P169-stable source-shape law produced a validated W/Z absolute-mass prediction.

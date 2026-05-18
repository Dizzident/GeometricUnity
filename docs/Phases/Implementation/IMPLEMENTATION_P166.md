# Implementation P166

P166 performs the source-level amplitude-normalized W/Z prediction attempt selected by P165.

It applies the best P118 dimensional normalization candidate to the repaired W/Z raw matrix-element ratios and checks raw-amplitude, W/Z common-scale, and target-comparison gates. The attempt is explicitly nonpromotional unless all gates pass.

## Result

Terminal status: `source-normalized-wz-prediction-attempt-failed-not-promoted`.

- selected candidate: `boson-vector-length-times-sqrt-fermion-coefficient-length`
- candidate value: `3971.1116831436507`
- raw-amplitude gate: passed
- minimum scaled raw-to-target ratio: `0.9844156166262811`
- W/Z common-scale spread: `0.12719953192138267`
- common-scale tolerance: `0.05`
- promotion allowed: `false`

Diagnostic mass attempts:

- W: `89.86381660219678 GeV`, target `80.3692 GeV`, sigma residual `713.8809475335919`
- Z: `89.76689124891732 GeV`, target `91.188 GeV`, sigma residual `710.5543755413421`

The selected normalization candidate reaches the raw-amplitude scale but fails W/Z common-scale and target-comparison gates, so it is not a validated boson prediction.

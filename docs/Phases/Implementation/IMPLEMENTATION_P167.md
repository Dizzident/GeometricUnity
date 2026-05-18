# Implementation P167

P167 tests source-shape normalization candidates after P166 showed that a common dimensional lift reaches raw-amplitude scale but fails W/Z common-scale spread.

The phase evaluates predeclared target-independent corrections built from source-mode features:

- no shape correction;
- max-amplitude correction;
- square-root max-amplitude correction;
- L1 norm correction;
- square-root L1 norm correction;
- participation-ratio correction;
- square-root participation-ratio correction;
- entropy participation correction;
- square-root entropy participation correction.

The phase rejects promotion unless the candidate passes raw-amplitude, W/Z common-scale, physical target-comparison, and derivation-backed gates.

## Result

Terminal status: `source-shape-normalized-wz-attempt-common-scale-only-not-promoted`.

Best candidate: `l1`, using the W/Z source-vector L1 norm.

- raw-amplitude gate: passed
- minimum scaled raw-to-target ratio: `1.0432643912763202`
- common-scale spread: `0.011247872112175638`
- common-scale tolerance: `0.05`
- target comparison: failed
- derivation-backed: `false`
- promotion allowed: `false`

Diagnostic mass attempts:

- W: `84.79475114128772 GeV`, target `80.3692 GeV`, sigma residual `332.7482061118581`
- Z: `95.13319331170509 GeV`, target `91.188 GeV`, sigma residual `1972.5966558525413`

P167 fixes the P166 common-scale blocker for a diagnostic candidate, but it does not produce a validated boson prediction because the L1 rule is not derivation-backed and the physical target comparison still fails.

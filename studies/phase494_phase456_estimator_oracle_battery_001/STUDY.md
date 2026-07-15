# Phase494: Phase456 estimator oracle battery

This A8 exploration freezes every estimator, synthetic channel, covariance,
mass grid, threshold, and invalid-row rule before reading Phase493. It uses
only analytic periodic correlators and exact specified Gaussian covariance;
it runs no HMC and no performance benchmark.

The battery tests the original T=4 `C(1)/C(2)` cosh estimator on its positive
single-pole domain, demonstrates that the same ratio does not identify
multi-pole spectral content, and compares covariance-aware full-correlator
single- and two-pole fits. Positive, sign-indefinite, overall-negative, and
massless-boundary channels make all sign and domain assumptions explicit. A
synthetic invalid family row must withhold the family threshold and propagate
invalidity to every row.

No result reinterprets or edits Phase456, changes its terminal, constructs a
repair pack, evaluates Phase458, authorizes sampling, or supports promotion.

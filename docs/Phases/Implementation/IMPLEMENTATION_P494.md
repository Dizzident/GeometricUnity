# Implementation P494 — Phase456 estimator oracle battery

Phase494 is a reduced deterministic A8 oracle battery. Its executable freezes
the T=4 original cosh-ratio estimator, covariance-aware full-correlator fits,
a target-blind fixed two-pole mass grid, sign/positivity tests, covariance,
thresholds, and invalid-row propagation before it reads the Phase493 summary.

Analytic periodic correlators cover a positive single pole, a positive
two-pole mixture, a sign-indefinite mixture, an overall-negative single pole,
and the massless boundary. The covariance is an exact specified Gaussian input;
there is no Markov-chain sampling and no performance benchmark.

The top-level taxonomy is `original-estimator-identifiable`,
`alternative-estimator-feasible`, `channel-nonidentifiable`,
`mixed-estimator-outcome`, or `invalid-oracle-battery`. Structured results are
under `batteries` and `oracleCases`, while `decisionContractSha256` binds the
pre-consumption estimator contract.

This phase is exploration-only. It does not read or reinterpret the Phase456
artifact, change the Phase456 terminal, discharge O4, satisfy Phase458, build a
repair pack, authorize sampling or production, fill a source contract, or
support physical-unit promotion. `promotedPhysicalMassClaimCount=0` throughout.

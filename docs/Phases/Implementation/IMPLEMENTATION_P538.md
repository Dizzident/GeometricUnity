# Implementation P538: Fixed-Grid Interacting-HMC Retuning

Phase538 exact-binds the immutable Phase533–535 evidence and prospectively
tests whether fixed, non-adaptive leapfrog rows can sample the Phase534 reduced
interacting target without its recorded non-finite and divergent trajectories.

The exact pre-review v1 contract is reconstructed at
`studies/phase538_fixed_grid_interacting_hmc_retuning_001/preregistration/pre_review_non_citable_phase538_fixed_grid_interacting_hmc_retuning_contract_v1.json`
with SHA-256
`b33f473ac2cd41d34ebb02876ddba234173f19c186260bc1c80089eee74a948a`.
Although v1 was frozen before the first run, its diagnostic implementation did
not survive adversarial review and it is retained only as non-citable lineage.

The corrected v2 contract is
`studies/phase538_fixed_grid_interacting_hmc_retuning_001/preregistration/phase538_fixed_grid_interacting_hmc_retuning_contract_v2.json`.
It declares `frozenBeforeFirstRun=false`, `frozenBeforeCorrectedRun=true`, and
`postReviewDiagnosticHardening=true`. The eight step-size/trajectory-length
rows, two original seed tables, disjoint third seed family, seed integers and
initial scales, quadrature and diagnostic numerical thresholds, target-blind
row order, and two-hour/one-GiB refusal ceilings are unchanged from v1. The
diagnostic definitions and reporting were strengthened after review. The
third family participates in selection and is not an independent
post-selection validation set. Every passing row must satisfy every gate
independently on all three seed families and separately for the state, second
moment, and x-times-gradient convergence scalars. The hardened implementation
uses split rank-normalized R-hat and paired initial-positive/monotone effective
sample size.

Runtime CPU, wall-clock, kernel, and peak-working-set measurements remain
active and enforce the v2 resource ceilings before adjudication. The volatile
measurement values are emitted to the console only. Citable JSON serializes
the deterministic estimates, limits, and resource-gate booleans, preventing
ordinary scheduling and process-memory variation from changing its bytes.

The pre-review first-run full and summary bytes are preserved as
`output/pre_review_non_citable_fixed_grid_interacting_hmc_retuning.json` and
`output/pre_review_non_citable_fixed_grid_interacting_hmc_retuning_summary.json`.
Each has SHA-256
`f4dd6c92cf4dbaa3e08a40d10f835faf8f6584d45de12ff1098a3a4aff6754ef`.
They are not evidence for the corrected terminal. Runtime lineage checks bind
the reconstructed v1 contract and both preserved first-run outputs to these
hashes before a v2 run can be input-valid.

The positive terminal is explicitly
`post-review-hardened-stable-fixed-grid-row-reduced-target-feasible`; it is not
a pristine-preregistration terminal. The output is reduced-target integrator
feasibility evidence only. It does not validate the complete registered
lattice operator, select a production configuration, reopen Phase535, or alter
any Phase458, O4, source-lineage, or physical-claim boundary.
`promotedPhysicalMassClaimCount=0`.

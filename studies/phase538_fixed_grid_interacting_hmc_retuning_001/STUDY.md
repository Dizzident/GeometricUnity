# Phase538: Fixed-Grid Interacting-HMC Retuning

Phase538 is the A23 retuning experiment on the coercive scalar polynomial
already reconstructed and quadrature-checked by Phase534. The exact original
v1 contract is preserved as
`preregistration/pre_review_non_citable_phase538_fixed_grid_interacting_hmc_retuning_contract_v1.json`
(SHA-256 `b33f473ac2cd41d34ebb02876ddba234173f19c186260bc1c80089eee74a948a`).
It was frozen before the first run, but its diagnostic implementation required
adversarial-review correction, so neither that contract nor its first-run
outputs are citable Phase538 evidence.

The corrected
`preregistration/phase538_fixed_grid_interacting_hmc_retuning_contract_v2.json`
is explicitly post-review hardened. It was frozen before the corrected run,
not before the first run. The finite step-size/trajectory-length grid, seed
integers and initial scales, numerical thresholds, target-blind row order, and
resource ceilings are unchanged from v1; only diagnostic definitions and
their reporting were strengthened. Adaptation remains disabled. The
preregistered disjoint third seed family participates in row selection and is
not independent post-selection validation.

CPU, wall-clock, kernel, and peak-working-set measurements are still taken and
enforced against the frozen resource ceilings, and are printed to the console
for operational inspection. Their volatile values are deliberately excluded
from the citable JSON; it contains only deterministic resource estimates,
limits, and pass/fail booleans so identical inputs produce byte-stable output.

A row passes only when every seed family has zero non-finite and divergent
trajectories and passes the frozen acceptance, energy, R-hat, effective-sample-
size, quadrature-moment, and integration-by-parts gates separately for the
state, second moment, and x-times-gradient scalar. R-hat is split and rank
normalized, and effective sample size uses paired initial-positive/monotone
autocorrelation truncation. All rows run before the lexicographically first
passing row is selected, so observations cannot reorder the menu.

The original first-run full and summary bytes are retained as
`output/pre_review_non_citable_fixed_grid_interacting_hmc_retuning.json` and
`output/pre_review_non_citable_fixed_grid_interacting_hmc_retuning_summary.json`.
Both have SHA-256
`f4dd6c92cf4dbaa3e08a40d10f835faf8f6584d45de12ff1098a3a4aff6754ef`.
They are historical review inputs only and must not be cited as the corrected
Phase538 result.

A passing row produces only the explicit
`post-review-hardened-stable-fixed-grid-row-reduced-target-feasible` terminal.
This is not a pristine-preregistration result and demonstrates feasibility
only for this one-dimensional reduced target. It does not reinterpret
Phase534, reopen the Phase535 complete-lattice pilot, choose a production
default, validate a complete lattice action, create a Phase481 pack, satisfy
Phase458 or O4, or authorize a physical-unit claim.
`promotedPhysicalMassClaimCount=0`.

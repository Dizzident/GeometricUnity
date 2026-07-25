# Phase545 injectable deterministic HMC kernel

This A28 construction phase materializes a reusable deterministic single-proposal
kernel. The original v1 positive result failed adversarial review and is
preserved byte-for-byte under `pre_review/v1`; it is not citable. An interim v2
repair is likewise preserved under `pre_review/v2_interim` and is not citable
because its contract history was not prospective. The frozen v3 repair adds
non-divergent acceptance, pre-evaluator finite checks, memory and work refusal,
strict contract validation, and adversarial boundary fixtures.

The memory fixture establishes early refusal before proposal-array allocation
or an evaluator call, not a comprehensive peak-memory bound. The work fixture
exceeds both frozen caps and establishes combined refusal with step-cap
precedence, not independent force-cap branch coverage.

No random-number generator is instantiated. No chain is advanced, no warmup,
adaptation, sampling, benchmark, pilot execution, or configuration retention
occurs. Fixture results are implementation evidence only and establish no HMC
evidence, acceptance rate, stationarity, detailed balance, mixing, convergence,
observable estimate, production, launch, physical-unit, or GeV authority.

External review remains pending and `promotedPhysicalMassClaimCount=0`.

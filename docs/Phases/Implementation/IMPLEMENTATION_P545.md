# Implementation P545: Injectable Deterministic HMC Kernel

Phase545 implements Amendment A28's reusable bounded-pilot proposal kernel.
The original v1 result is preserved and non-citable because adversarial review
found five implementation and evidence defects. An execution-preceded interim
v2 is also preserved and non-citable. The prospectively frozen v3 exact-binds
both histories and the immutable upstream inputs.

The v3 kernel accepts explicit position, momentum, and log-uniform threshold
inputs; performs velocity leapfrog, Hamiltonian accounting, finite/divergence
telemetry, deterministic accept/reject selection, and preallocation/work
refusal. All ten deterministic fixtures pass. They cover accept, reject,
reversal, corrupt action/gradient, intermediate overflow, finite divergence,
the exact acceptance boundary, memory refusal, and a request exceeding both
frozen work caps. The terminal is
`injectable-deterministic-hmc-kernel-v3-repaired`.

The memory fixture proves early refusal before proposal-array allocation or an
evaluator call; it is not a comprehensive peak-memory bound. The work fixture
proves combined-cap refusal with step-cap precedence, not independent coverage
of the force-evaluation-only branch.

These injected single-proposal fixtures validate implementation semantics
only. They provide no stochastic RNG, Markov-chain, HMC acceptance-rate,
stationarity, detailed-balance, warmup, mixing, convergence, benchmark, or
observable evidence. No pilot execution, sampling, configuration retention,
production authority, physical-unit claim, or GeV claim follows.

# Phase478: Phase458 Gate-Specification Closure

Phase478 freezes the machine contract for the later Phase458 Binder gate. Its
terminal is `gate-specification-closed-phase458-inputs-incomplete`: the
specification is complete, but Phase458 is not evaluated and no launch is
authorized.

The hash-pinned contract is
`preregistration/phase458_gate_contract_v1.json`. It defines four gate-input
states (`missing`, `invalid-or-drifted`, `available-false`, and
`available-true`), exact G1-G6 sources and terminal allowlists, and an ordered,
mutually exclusive outcome taxonomy. Missing and invalid evidence remain
distinct and neither can be coerced into an available negative.

## Frozen gates

- G1 reads the exact Phase456 A5 obstruction record. That record is available
  and says no theorem was established. Only a later fully gated Phase482
  theorem terminal may close L8.
- G2 consumes only verified operational cost fields from the committed Phase456
  production artifact. The science rows are invalid, but the measured
  `0.11371015556660466` CPU-week cost remains valid operational evidence.
- G3 is motivation only, never evidence. It requires a valid Phase456 analysis
  with an absolute Binder or susceptibility significance at least 2, or an
  O4-resolved Phase455 T2 terminal. Invalid Phase456 diagnostics and the current
  convention-fragile Phase455 terminal cannot satisfy it.
- G4 requires Phase477 infrastructure plus a genuine validated signed external
  Phase480 memo and machine-derived admissibility. Register currency or the
  inert memo template alone never satisfies it.
- G5 is a conjunction of exact resolved A5, Phase455, Phase456, and Phase457
  states. Skeleton, invalid, refused, convention-fragile, awaiting, and
  verdict-withheld states do not count.
- G6 pins a scheduling projection, not a physics result. It projects one
  Phase456-default-equivalent five-column workload at each n=3,4,5,6 with n^4
  volume scaling and a 1.5 safety factor. The resulting projection is
  `1.5044386597816015` CPU-weeks, below the inclusive 2.0 CPU-week CPU-route
  boundary. A projection above 2.0 would fund Inc 0+1 only; production-operator
  bit-exact parity would still be required before any CUDA trajectory.

The current readiness snapshot is two available-true gates (G2/G6), one
available-false gate (G1: no theorem), three missing gates (G3/G4/G5), and zero
invalid gates. Phase458 therefore retains its resting state
`blocked-inputs-incomplete`.

Phase478 performs zero physics computation, reads or authors no human ruling,
fills no source contract, and preserves `promotedPhysicalMassClaimCount=0`.

# Implementation P489 — Reduced Sampler Restart Equivalence

Phase489 implements the Amendment A6 reduced deterministic control. It uses a
bounded synthetic rotation target and three independently coded Haar-style
proposal paths: uniform independence, left-local composition, and right-local
composition.

The phase owns its serializable xoshiro256** RNG. A checkpoint contains all
four RNG words, the canonical quaternion state, proposal/acceptance/measurement
counters, accumulated sums, batch sums and counts, and the complete acceptance
sequence. An uninterrupted run and a JSON-serialized/resumed run must match
bit-for-bit for each proposal family. Any mismatch reaches the explicit
`restart-equivalence-failed` terminal.

Cross-proposal agreement is deliberately weaker and separate: bounded
synthetic observable means must agree within the prospectively frozen absolute
tolerance. Disagreement reaches its own fail-closed terminal and is never
hidden by restart success.

The exact Phase487 and Phase488 summaries are mandatory precursors. Phase489
therefore tests restart behavior only after the independent measure and
proposal controls are green, while retaining its own local implementation.

The output is exploration-lane evidence only. It records
`humanRulingAuthored=false`, `o4Discharged=false`,
`phase458EvaluationAuthorized=false`, `productionAuthorized=false`,
`noGevPromotion=true`, and `promotedPhysicalMassClaimCount=0`.

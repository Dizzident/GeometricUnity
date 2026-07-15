# Phase489: Reduced Sampler Restart-Equivalence Control

This A6 exploration phase runs a reduced synthetic rotation-chain workload.
It compares uniform independence, left-local composition, and right-local
composition proposal families on three bounded, nonphysical observables.

The workload freezes its RNG algorithm and complete seeds, step count, burn-in,
checkpoint split, proposal scale, target strength, batches, and tolerances in
code before execution. It uses a local xoshiro256** implementation whose four
state words are serialized explicitly. No `System.Random` state is involved.

For each proposal family, the phase compares an uninterrupted run with a run
serialized to JSON and resumed at the frozen split. Equality is bit-exact and
includes the quaternion state, full RNG state, accept/reject sequence,
counters, accumulated observable sums, final observables, and checksums.
Cross-proposal comparison is a separate statistical agreement test; different
families are not expected to produce bit-identical chains.

The exact green Phase487 and Phase488 summaries are mandatory precursors;
their implementations are not imported into this phase.

This is exploration-only. It does not validate a production sampler, author a
human ruling, discharge O4, evaluate Phase458, authorize production, fill a
source contract, or support a physical-unit claim.

# Implementation P477 — O4 Adjudication Infrastructure

Phase477 is the zero-physics-compute A4 infrastructure phase at execution
priority 1. It emits `infrastructure-ready-pending-human-ruling` to both output
JSON files.

The implementation replaces the former phase-name/prose heuristic with an
exact 31-artifact coverage contract. Recursive pending-marker discovery fails
on malformed JSON, an unknown marker pointer, contradictory booleans, missing
or duplicate phases, predicate drift, or any unmapped pending artifact. The
generated Markdown register is now display-only; exact JSON contracts drive
coverage and dependency behavior.

The production memo schema, inert unsigned template, coverage contract, and
dependency map agree on 13 mandatory ruling IDs. The 94-edge synthetic
overturn battery runs in memory, mutates every declared dependent and no
unrelated branch, snapshots 19 canonical artifacts, writes no Phase457 ruling
candidate path, and rejects both synthetic and legacy-minimal envelopes as
real. The main integrity verifier independently runs both coverage and
overturn tests on every pass.

Infrastructure success cannot supply or approve a physicist ruling. Phase477
keeps `physicistReviewPending=true`, preserves every no-contract/no-promotion
boundary, and fixes `promotedPhysicalMassClaimCount=0`.

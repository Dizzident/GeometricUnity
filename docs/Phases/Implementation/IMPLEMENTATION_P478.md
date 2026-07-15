# Implementation P478 — Phase458 Gate-Specification Closure

Phase478 implements Amendment A4 execution-priority item 2. It upgrades the
initial skeleton to
`gate-specification-closed-phase458-inputs-incomplete` while leaving Phase458
unevaluated and unexecutable.

The committed contract
`studies/phase478_phase458_gate_specification_closure_001/preregistration/phase458_gate_contract_v1.json`
has SHA-256
`d2bd1aa491ec7a9fc680c8f367fbed353b4abde040d7511671fe417dfba894f0`.
It freezes:

1. The exact four-state input model and G1-G6 order.
2. Hash/path/field bindings and exact admissible upstream terminal sets.
3. The distinction between a missing input, drift, an available negative, and
   an available positive.
4. Outcome precedence: invalid, incomplete, O4-dependent invalidation, theorem
   closure, absent motivation, CUDA Inc 0+1 hold, CPU eligibility, then
   CUDA-parity-green eligibility.
5. A G6 projection of `1.5044386597816015` CPU-weeks from the committed n=4
   cost, four n=3..6 volume factors, and a 1.5 safety factor. The inclusive CPU
   boundary is 2.0 CPU-weeks; only a strict exceedance triggers Inc 0+1.

The executable recomputes the projection, checks exact contract structure and
the A5 byte hash, verifies that the invalid Phase456 science analysis cannot
leak into G3, and exercises all eight outcome branches plus numeric threshold
edge cases. Current states are G1 available-false, G2/G6 available-true, and
G3/G4/G5 missing. Phase458 remains `blocked-inputs-incomplete`.

This is zero physics compute and no human adjudication. It authorizes neither
Binder sampling nor acceleration or production, fills no source contract, and
keeps `promotedPhysicalMassClaimCount=0`.

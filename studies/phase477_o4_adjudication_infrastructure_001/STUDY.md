# Phase477: O4 Adjudication Infrastructure

Phase477 is execution-priority 1 of labeled Wave-2 amendment A4. Its terminal
is `infrastructure-ready-pending-human-ruling`.

The phase recursively finds every `physicistReviewPending` marker and requires
all 31 current artifacts, their exact JSON pointers, structured evidence, and
their dispositions to match `scripts/o4_register/coverage_contract.json`.
Classification is exact and typed; prose and phase-name heuristics cannot drive
a ruling. Direct physics rows, transitive dependents, separate pending packets,
and administrative zero-compute rows are distinct, so a memo cannot
blanket-clear the program.

The inert human-memo template and production schema share exactly 13 ruling
IDs with the coverage and dependency contracts. The template is deliberately
production-invalid: it is unsigned, marked template-only, and every ruling is
`insufficient-basis` / `defer`. The synthetic-overturn battery exercises all
94 declared dependency edges, proves unrelated branches do not change, rejects
synthetic and legacy two-field ruling envelopes, and writes none of Phase457's
candidate ruling paths. It snapshots 19 canonical artifacts before and after.

This is tooling readiness only. No human ruling is read, authored, inferred,
approved, or simulated into production state; all pending flags remain true.
No source contract is filled, no ledger limb is closed, no sampling is
authorized, and `promotedPhysicalMassClaimCount=0`.

# Phase480: O4 Physicist-Adjudication Intake

Phase480 is execution-priority 4 after Phases477-479. The implemented intake
rests at `awaiting-external-physicist-ruling` while its exact input path is
absent or any authentication check fails.

The preregistered contract accepts only a strict-JSON, externally authored memo
bound to an ancestor commit and the byte-exact dossier, coverage contract,
schema, reviewer registry, dependency map, and reviewed phase artifacts. The
signed payload is RFC8785-JCS canonical UTF-8 and must pass SHA-256 plus a real
Ed25519 verification against a pre-pinned active human-reviewer SPKI key.
Shape validation is explicitly insufficient.

The single input path is
`studies/phase480_o4_physicist_adjudication_intake_001/input/o4_human_physicist_memo_v1.json`.
No input is committed now, and the reviewer registry is empty, so the current
terminal remains awaiting. The phase cannot infer a choice from numerical
outputs or user risk acceptance. Even a future positive authentication records
the human's supplied bytes only; it does not choose a disposition or lift a
downstream hold.

No source contract is filled and `promotedPhysicalMassClaimCount=0`.

# Implementation P480 — O4 Physicist-Adjudication Intake

Phase480 is the human-input-gated A4 skeleton at execution priority 4. It emits
`awaiting-external-physicist-ruling` because no genuine signed external memo is
supplied by the phase.

The future implementation may validate only a memo with identity, date, scope,
explicit rulings, role or qualifications, caveats, and signature provenance.
It may never generate or infer ruling content. The skeleton fills no contract
and preserves `promotedPhysicalMassClaimCount=0`.

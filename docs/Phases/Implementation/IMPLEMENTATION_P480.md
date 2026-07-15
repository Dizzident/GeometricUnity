# Implementation P480 — O4 Physicist-Adjudication Intake

Phase480 is the implemented human-input-gated A4 intake at execution priority
4. It emits `awaiting-external-physicist-ruling` because no genuine signed
external memo is currently supplied.

The verifier rejects duplicate-key JSON and enforces the exact 13-ruling schema,
reviewer registry, ancestor-commit and byte-level repository bindings, exact
per-ruling reviewed-artifact hash sets, RFC8785-JCS payload hashing, signature
artifact hashing, and Ed25519 verification. Unsupported signature modes fail
closed. Missing or invalid input remains at the same awaiting terminal with
machine-readable reasons. It may never generate or infer ruling content, lift
a hold, or fill a contract, and preserves `promotedPhysicalMassClaimCount=0`.

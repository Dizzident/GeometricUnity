# Convention-Robustness Tranche Plan (Amendment A6)

## Scope

This batched exploratory tranche tests the highest-ranked machine-checkable
fragility from Phase486. It does not replace external review or consume an O4
or Phase458 gate.

## Phase487 — independent SO(3) Haar measure control

- Derive the axis-angle radial law independently of the Phase450 utilities.
- Check normalization, analytic character moments, and left/right composition
  invariance using deterministic quadrature and a separately implemented
  quaternion construction.
- Exercise the identity and fundamental-domain boundary cases.

## Phase488 — proposal invariance control

- Freeze deterministic seeds, sample counts, moments, and tolerances before
  comparing uniform independence, left-composition, right-composition, and
  direct axis-angle proposal families.
- Check stationary analytic moments, proposal inversion symmetry, and binned
  detailed balance. Any family outside tolerance fails closed.

## Phase489 — reduced restart-equivalence control

- Use only reduced deterministic workbench observables with no physical
  target values.
- Compare the validated proposal families and uninterrupted versus
  serialized/resumed execution, including RNG state, acceptance sequence,
  state checksum, and observables.
- Preserve every negative branch and distinguish exact restart equality from
  cross-proposal statistical agreement.

Outputs remain exploration-only. They cannot discharge O4, authorize an
expensive sampler, or support a physical-unit claim. A later confirmation
phase requires a new prospective checkpoint.

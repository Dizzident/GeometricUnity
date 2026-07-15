# Implementation P491 — committed bosonic-model family sensitivity

Phase491 implements Amendment A7's exploration-only sensitivity table without
using the Phase455 verdict as an answer. It reconstructs the reduced fermionic
spectrum, bosonic mode masses, relative potentials, and derivative directly
from the committed Phase428/430/455 formulas. It then locates stationary points
independently by a frozen dense sign scan plus bisection and applies the
Phase455 target-blind negative-depth-minimum definition.

The admitted bosonic models are the normalized Phase430 transverse determinant
and the rank-one restriction of Phase430's mixed tree quartic. The latter is
exactly zero because the committed expression is proportional to `(a*b)^2`.
Seductive alternatives without a committed coefficient or implemented Hessian
map are rejected and serialized with provenance.

The valid table crosses both admitted models, the independently expressible
Za and Zc zero-mode treatments, the committed `-1/2` and explicitly recorded
`-1` fermionic normalizations, and T/D/S axes. All three advertised Zb floor
values are retained as invalid rows: Phase455's value code does not apply a
floor, while the cited floor belongs to a different Hessian spectrum and has no
committed determinant-factor map. No replacement rule is invented.

The exact top-level taxonomy is `stable-null`, `stable-candidate-well`,
`model-convention-fragile`, or `invalid-inputs`. The frozen contract hash is
serialized as `frozenSensitivityContractSha256`; the full branch array is
`sensitivityTable`; and admissibility is under `modelAdmissibility`.

The phase is exploration-only and cannot alter Phase455, discharge O4,
evaluate Phase458, authorize production, fill a source contract, or support a
physical-unit promotion. `promotedPhysicalMassClaimCount=0` on every branch.

# Implementation P380 - Response-Image W/Z Contract Application Audit

Phase380 tries the Phase379 carrier-image result against the Phase201 W/Z
source-lineage intake contract without mutating the template.

Inputs:

- Phase379 response-image carrier-axis characterization.
- Phase201 W/Z intake contract summary.
- Phase209 W/Z evidence request.
- Phase210 evidence application gate.
- Phase213 blocker matrix.
- Phase295/307/311 observed-row and selector blockers.

Decision rule:

- Accept no Phase201 field unless the candidate supplies the complete
  source-lineage artifact required by Phase201/P209.
- Preserve `canFillPhase201WzContract=false` when the response image lacks a
  theorem, observed electroweak field map, strict background identity, separate
  W/Z source rows, raw-amplitude gates, common bridge, GeV normalization, and
  derivation ids.

Boundary:

- This is a contract application audit, not a prediction.
- It must not use physical target values for construction.
- It must not mutate the Phase201 template.

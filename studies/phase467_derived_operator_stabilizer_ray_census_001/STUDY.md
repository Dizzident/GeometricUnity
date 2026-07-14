# Phase467: Derived-Operator Stabilizer Ray Census (C-STABILIZER v3)

This phase implements item 7 of the binding Wave-2 plan. It reconstructs the
centralizer of the Phase404 `su(3)_c + su(2)_L` embedding inside `so(10)` by
exact commutator rank, verifies the true four-dimensional commutant
`u(1)_(B-L) + su(2)_R`, and scores all 224 committed Phase404 rays by exact
adjoint stabilizer dimension.

The pre-registered discriminator is 13 versus 25: the standard Y ray must have
the generic `u(3)+u(2)` stabilizer of dimension 13, while the enhanced control
ray has `u(5)` stabilizer dimension 25. A direct `so(6,4)` field-of-definition
arm recomputes the four-dimensional commutant under
`eta=diag(+^6,-^4)`; it is a computational substitute for the compact-form
proxy at this finite-dimensional surface only and records no human O4 ruling.

The phase is fail-closed and target-blind. It selects Y only if the standard
projective ray is unique under the committed invariant. Otherwise it emits the
scoped negative terminal and forwards every surviving generic ray to C-LIFT.
No scale, pole, coupling normalization, or physical prediction is produced;
`promotedPhysicalMassClaimCount = 0`.

Run:

```bash
dotnet run -c Release --project studies/phase467_derived_operator_stabilizer_ray_census_001/Phase467DerivedOperatorStabilizerRayCensus.csproj
```

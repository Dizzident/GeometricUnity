# Phase422: Vector-Spinor 144 Bilinear Scalar-Capacity Audit

Phase417 closed the source-pinned `Z_{1/2}` vector-spinor `144` branch at
linear order: the chiral `2 x 144` carrier has no welded scalar. Phase422
tests the next representation-level possibility without inventing a source
map: bilinears of the same chiral carriers.

Result:

- The mixed-chirality, Dirac-like `Z_L x Z_R` channel has zero welded-scalar
  capacity.
- The same-chirality, Majorana-like channels have welded-scalar capacity:
  `Z_L x Z_L = 264`, `Z_R x Z_R = 264`, total `528`.
- This is only a representation-capacity result. The 528-dimensional
  same-chirality scalar sector is too large for the small-sector SM-stable
  decomposition shortcut used in older phases, and no current source supplies a
  bosonic projection map, action, VEV selection, observed-field rows,
  weak-angle lineage, pole extraction, or GeV normalization.

No Phase201 or Phase256 fields are filled, and no W/Z/H mass is promoted.

Run:

```bash
dotnet run --project studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001/Phase422VectorSpinor144BilinearScalarCapacityAudit.csproj
```

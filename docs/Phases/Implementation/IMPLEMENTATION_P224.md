# Implementation P224: Electroweak Parameter Dependency Audit

P224 maps the known W/Z/H mass formulas to the source-lineage parameters that the GU pipeline would need before an absolute boson mass prediction can be promoted.

External physics context is the PDG 2025 electroweak review. At tree level, W/Z masses depend on the electroweak VEV and gauge couplings:

```text
MW = g v / 2
MZ = sqrt(g^2 + g'^2) v / 2
M_gamma = 0
```

The Higgs mass depends on the scalar potential parameter and the VEV. P224 treats these formulas as dependency structure only. They are not GU source evidence.

Current repo mapping:

- `v`: available as external/Fermi-derived diagnostic context, not a GU source-lineage prediction.
- `g`: P221 gives a close SU(2) Casimir numerical lead, but `sourceLineagePromotable=false`.
- `g'` or weak angle: the W/Z ratio is the current dimensionless defensible electroweak relation, but it does not provide the absolute coupling magnitude or VEV source.
- Higgs scalar self-coupling: P215 and P223 provide target-implied/numerical diagnostics, but no scalar source/operator derives the value.

Therefore W, Z, and Higgs absolute masses remain blocked. P224 does not add a prediction; it records the minimum missing parameter lineages that must be filled before P201/P209/P210/P213 can be promoted.

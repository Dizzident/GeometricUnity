# Phase434: Conditional Observed-Field Extraction Row Ledger

Phase419 materialized the FMS/dressing-style symbolic template for the
photon/W/Z/Higgs observed-field extraction with **zero** source-defined
Phase256 fields. This phase derives the extraction rows **as exact functions of
a candidate doublet VEV**, from the internal embedding alone, so that any
future dynamically- or source-supplied VEV mechanically completes them.

The single blind input is the tree-level kernel relation
`tan^2 theta_W = 3/5` (`sin^2 = 3/8`, `cos^2 = 5/8`) fixed at the unification
point by Phase404 and reused by Phase429. No measured electroweak value appears
anywhere in the program; derivation is strictly separated from comparison.

## Gauge-boson mass matrix

From `|D_mu Phi|^2` with `Phi = (0, v/sqrt(2))` in the `(W1,W2,W3,B)` basis and
`gPrime^2/g^2 = 3/5`:

```
M^2 = (v^2/4) * [[g^2,0,0,0],[0,g^2,0,0],[0,0,g^2,-g gPrime],[0,0,-g gPrime, gPrime^2]]
```

Eigenvalues `(v^2/4){g^2, g^2, g^2+gPrime^2, 0}` (the neutral 2x2 block has
determinant exactly 0). Hence `m_W^2/m_Z^2 = g^2/(g^2+gPrime^2) = cos^2 = 5/8`
exactly — dimensionless, VEV-independent, and cross-checked against Phase429's
`sqrt(5/8)` ratio row.

## Extraction rows (exact, verified to 1e-15)

| Row | Field | Coefficients |
|-----|-------|--------------|
| photon | `A = sin W3 + cos B` | `sin = sqrt(6)/4`, `cos = sqrt(10)/4` |
| Z | `Z = cos W3 - sin B` | `cos = sqrt(10)/4`, `-sin = -sqrt(6)/4` |
| W+ | `W+ = (W1 - i W2)/sqrt(2)` | `1/sqrt(2) = sqrt(2)/2` |
| W- | `W- = (W1 + i W2)/sqrt(2)` | `1/sqrt(2) = sqrt(2)/2` |
| charge | `e = g sin` (`e^2 = (3/8) g^2`) | `e/g = sqrt(6)/4` |
| mass ratio | `m_W^2/m_Z^2 = cos^2` | `5/8` (rational) |

## Conditional-completion ledger (20 Phase419/Phase256 template fields)

- **(a) conditionally determined by these rows given any doublet VEV — 7:**
  `electroweakGaugeEmbeddingId`, `photonEigenstateProjectionId`,
  `zBosonSourceRowId`, `wBosonSourceRowId`, `wzCommonBridgeGatePassed`,
  `quadraticElectroweakMassOperatorId`, `targetBlindConstructionHash`
  (mixing angles, eigenstate directions, charge relation, mass ratio, operator form).
- **(b) still requires the VEV amplitude/scale — 4:**
  `fourDimensionalObservedVacuumArtifactId`, `branchNormalizationSourceId`,
  `wBosonRawAmplitudeGatePassed`, `zBosonRawAmplitudeGatePassed`
  (absolute masses).
- **(c) still requires independent lineage — 9:**
  `observedFieldExtractionTheoremId`, `sourceReferenceIds`,
  `higgsScalarSourceOperatorId`, `higgsMassiveScalarProfileId`,
  `higgsPotentialSelfCouplingRelationId`, `canonicalOrDeclaredShiabBranchId`,
  `stabilitySidecarIds`, `targetComparisonAfterConstructionGatePassed`,
  `phase201And209ApplicationReady`
  (pole extraction, GeV normalization, running/scheme, Higgs potential).

## Honest boundaries

The doublet VEV is a **candidate** parameter — its existence is not established
(Phases 405/410/418/428 closed the named internal mechanisms; the phase430-chain
experiments are pending). The rows are tree-level in the custodial limit.
Nothing here is an observed-field extraction **theorem**; it is the conditional
algebra such a theorem must reproduce. Every `canFill*`/`routePromotes*` flag is
false — in particular `canFillPhase256ObservedFieldExtractionContract=false`,
because the VEV existence/amplitude fields and the pole/GeV lineage fields
remain unfilled. No Phase201 or Phase256 field is filled.

## Run

```bash
dotnet run -c Release --project studies/phase434_conditional_observed_field_extraction_row_ledger_001/Phase434ConditionalObservedFieldExtractionRowLedger.csproj
```

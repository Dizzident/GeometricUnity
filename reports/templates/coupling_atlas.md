# Coupling Atlas: {{atlasId}}

**Study ID:** {{studyId}}
**Fermion Background ID:** {{fermionBackgroundId}}
**Boson Registry Version:** {{bosonRegistryVersion}}
**Normalization Convention:** {{normalizationConvention}}
**Generated:** {{timestamp}}

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Total Couplings | {{totalCouplings}} |
| Nonzero Couplings (above noise floor) | {{nonzeroCouplings}} |
| Max Coupling Magnitude | {{maxCouplingMagnitude}} |
| Boson Modes with Nonzero Couplings | {{activeBosons}} |
| Boson Modes with All-Zero Couplings | {{inactiveBosons}} |

> **Interpretation note:** Coupling proxy values are inner-product-based proxies derived from the Dirac
> operator variation. They are NOT physical coupling constants or Yukawa couplings. A nonzero coupling proxy
> indicates that the boson mode and fermion mode pair share spectral support under the operator variation,
> not that they couple physically.

---

## Coupling Matrices

{{#couplingMatrices}}
### Boson Mode: {{bosonModeId}}

| Fermion Pairs | Max Entry | Frobenius Norm |
|--------------|-----------|----------------|
| {{fermionPairCount}} | {{maxEntry}} | {{frobeniusNorm}} |

{{/couplingMatrices}}
{{^couplingMatrices}}
_No coupling matrices generated._
{{/couplingMatrices}}

---

## Fragile Couplings (Negative Results)

The following boson modes produced coupling matrices at or below the noise floor. These are negative
results: no evidence of significant boson-fermion spectral overlap was found.

| Boson Mode ID | Max Magnitude | Frobenius Norm | Fermion Pairs | Fragility Reason |
|--------------|---------------|----------------|---------------|-----------------|
{{#fragileCouplings}}
| {{bosonModeId}} | {{maxMagnitude}} | {{frobeniusNorm}} | {{fermionPairCount}} | {{fragilityReason}} |
{{/fragileCouplings}}
{{^fragileCouplings}}
_No fragile couplings detected._
{{/fragileCouplings}}

---

## Physical Interpretation Constraints

- Coupling proxy values reflect numerical operator structure, not measured interaction strengths.
- Branch-consistent execution (structural tests pass) does NOT imply physical coupling is present.
- Physical coupling validation requires comparison with observed particle masses and decay widths (Phase IV M43).
- Gauge leakage in the coupling proxy indicates possible contamination from pure-gauge modes.

---

_Generated from CouplingAtlas. All coupling values are numerical proxies pending physical validation._

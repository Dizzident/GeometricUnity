# Fermion Family Atlas: {{atlasId}}

**Study ID:** {{studyId}}
**Registry Version:** {{registryVersion}}
**Generated:** {{timestamp}}

---

## Executive Summary

| Metric | Value |
|--------|-------|
| Total Families | {{totalFamilies}} |
| Stable Families (persistence > 0.5) | {{stableFamilies}} |
| Ambiguous Families | {{ambiguousFamilies}} |
| Definite-Left Families | {{definiteLefCount}} |
| Definite-Right Families | {{definiteRightCount}} |
| Conjugate Pairs Found | {{conjugatePairCount}} |

> **Interpretation note:** Family stability is a branch-tracking claim, NOT a physical particle identification.
> Stable families persist across branch variants. Physical particle status requires a full observation and
> comparison campaign (Phase IV M43).

---

## Family Spectral Sheets

{{#familySheets}}
### Family: {{familyId}}

- **Mean Eigenvalue:** {{meanEigenvalue}}
- **Eigenvalue Spread:** {{eigenvalueSpread}}
- **Member Mode Count:** {{memberCount}}
- **Branch-Stable:** {{isStable}}
- **Claim Class:** {{claimClass}}
- **Member Mode IDs:** {{memberModeIds}}

{{/familySheets}}
{{^familySheets}}
_No family spectral sheets generated._
{{/familySheets}}

---

## Chirality Summary

| Family ID | Chirality Status | Left Projection | Right Projection | Leakage Norm |
|-----------|-----------------|-----------------|------------------|--------------|
{{#chiralitySummaries}}
| {{familyId}} | {{chiralityStatus}} | {{leftProjection}} | {{rightProjection}} | {{leakageNorm}} |
{{/chiralitySummaries}}
{{^chiralitySummaries}}
_No chirality data available._
{{/chiralitySummaries}}

> **Chirality note:** "trivial" and "mixed" statuses are negative results — they indicate the spectral analysis
> could not assign a definite chirality label. These are reported in the Negative Result Dashboard.

---

## Conjugation Pairs

| Family ID | Has Conjugate Pair | Paired Family ID | Pairing Score |
|-----------|-------------------|-----------------|---------------|
{{#conjugationSummaries}}
| {{familyId}} | {{hasConjugatePair}} | {{pairedFamilyId}} | {{pairingScore}} |
{{/conjugationSummaries}}
{{^conjugationSummaries}}
_No conjugation data available._
{{/conjugationSummaries}}

---

## Negative Results

See the [Negative Result Dashboard](negative_result_dashboard.md) for:
- Families with unstable or ambiguous chirality
- Families with branch persistence below threshold
- Cluster split/merge/avoided-crossing events

---

_Generated from FermionFamilyAtlas. All mode families are spectral tracking objects, not physical particles._
_Physical particle identification requires downstream observation and comparison campaigns._

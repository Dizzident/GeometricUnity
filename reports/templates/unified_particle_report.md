# Unified Particle Report: {{reportId}}

**Study ID:** {{studyId}}
**Registry Version:** {{registryVersion}}
**Schema Version:** {{schemaVersion}}
**Generated:** {{generatedAt}}

---

## Executive Summary

| Particle Type | Count |
|--------------|-------|
| Fermion Candidates | {{totalFermions}} |
| Boson Candidates | {{totalBosons}} |
| Interaction Candidates | {{totalInteractions}} |
| **Total** | **{{totalCandidates}}** |

### Claim Class Distribution

| Claim Class | Count | Meaning |
|------------|-------|---------|
| C0 (Numerical Mode) | {{c0Count}} | Proxy or test mode — no physical claim |
| C1 (Local Persistent Mode) | {{c1Count}} | Persists locally, not branch-stable |
| C2 (Branch-Stable Candidate) | {{c2Count}} | Stable across branch variants |
| C3 (Observed Stable Candidate) | {{c3Count}} | Stable with observation evidence |
| C4 (Physical Analogy Candidate) | {{c4Count}} | Analogous to known physics patterns |
| C5 (Strong Identification Candidate) | {{c5Count}} | Strong multi-evidence identification |

> **Important:** All claim classes below C4 are spectral/structural claims only. Physical identification
> of particles requires comparison with measured spectra, decay widths, and quantum numbers (Phase IV M43).

---

## Top Candidates (by Claim Class)

| Candidate ID | Type | Claim Class | Mass-Like Value | Demotion Count |
|-------------|------|------------|----------------|----------------|
{{#topCandidates}}
| {{candidateId}} | {{particleType}} | {{claimClass}} | {{massLikeValue}} | {{demotionCount}} |
{{/topCandidates}}
{{^topCandidates}}
_No candidates registered._
{{/topCandidates}}

---

## Negative Result Dashboard Summary

### Unstable Chirality

{{unstableChiralityCount}} families could not be assigned definite chirality.

{{#unstableChiralityEntries}}
- **{{familyId}}** ({{chiralityStatus}}): {{reason}}
{{/unstableChiralityEntries}}
{{^unstableChiralityEntries}}
_No unstable chirality entries._
{{/unstableChiralityEntries}}

### Fragile Couplings

{{fragileCouplingCount}} boson modes have coupling matrices at or below the noise floor.

{{#fragileCouplingEntries}}
- **{{bosonModeId}}**: max={{maxMagnitude}}, norm={{frobeniusNorm}} — {{fragilityReason}}
{{/fragileCouplingEntries}}
{{^fragileCouplingEntries}}
_No fragile couplings detected._
{{/fragileCouplingEntries}}

### Broken Family Clusters

{{brokenClusterCount}} family clusters failed persistence or coherence tests.

{{#brokenFamilyClusterEntries}}
- **{{clusterId}}** ({{failureMode}}): persistence={{meanPersistence}}, ambiguity={{ambiguityScore}} — {{demotionReason}}
{{/brokenFamilyClusterEntries}}
{{^brokenFamilyClusterEntries}}
_No broken family cluster entries._
{{/brokenFamilyClusterEntries}}

---

## Demotion Log

The following demotions were recorded during registry construction:

{{#demotions}}
- [{{reason}}] {{candidateId}} ({{particleType}}): {{details}} ({{fromClaimClass}} → {{toClaimClass}})
{{/demotions}}
{{^demotions}}
_No demotions recorded._
{{/demotions}}

---

## Physical Interpretation Constraints

All results in this report are branch-execution outputs, not physical particle identifications:

1. **Branch-consistent ≠ physical**: A candidate that passes all structural tests (persistence,
   non-demoted claim class) is evidence that the numerical solution is self-consistent. It does
   not constitute evidence for a physical particle.

2. **Claim classes C0–C2** are purely numerical/spectral claims. They say nothing about whether
   the corresponding mode has a physical counterpart.

3. **Chirality labels** ("definite-left", "definite-right") reflect the spectral projector analysis.
   Physical left/right chirality assignment requires connecting to the Y-space geometry explicitly.

4. **Coupling proxies** are inner-product-based estimates. They are not Yukawa couplings or
   S-matrix elements.

5. **Negative results are first-class**: Absent, unstable, or ambiguous features constrain the
   theory space and are reported with equal prominence to positive findings.

---

_This report was generated from the UnifiedParticleRegistry. All claim classes carry explicit uncertainty._
_Physical validation requires downstream observation campaigns and comparison with measured particle properties._

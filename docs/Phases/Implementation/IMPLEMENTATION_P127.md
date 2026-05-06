# Implementation P127: Fermion Gauge-Basis Content Observable

## Status

Implemented `studies/phase127_fermion_gauge_basis_content_observable_001`.

## Purpose

P126 showed that existing artifacts do not provide target-blind fermion charge or weak-sector labels. P127 tests whether the repaired quality fermion eigenvectors themselves contain a usable sector proxy by measuring their SU(2) gauge-basis energy content.

## Method

The study reads the P125 source-family-enriched Phase95 modes and Phase12 spinor representation, then computes gauge-basis and spinor energy fractions from the real/imag interleaved eigenvector coefficients. It uses the production Dirac layout convention:

`((vertex * dimG + gauge) * spinorComponents + spinor)`

Promotion is intentionally conservative: a basis label requires a dominant gauge fraction of at least `0.80` and a dominance gap of at least `0.20`.

## Result

Terminal status:

`fermion-gauge-basis-content-observable-mixed-sector-blocked`

The observable is materialized, but it is not sufficient to define a W/Z fermion transition rule. The repaired quality modes are mixed across SU(2) gauge-basis components, and Phase126 still lacks target-blind fermion charge/weak-sector labels.

## Next Work

Implement a nontrivial fermion sector identity observable beyond gauge-basis energy fractions. It must emit charge/weak-sector labels, or an equivalent chirality/conjugation transition table, before the corrected W/Z transition sweep can be rerun under a physical transition rule.

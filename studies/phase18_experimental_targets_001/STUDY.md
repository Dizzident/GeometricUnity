# Phase XVIII Experimental Target Integration

## Purpose

This study adds authoritative physical boson target values without activating
them in the benchmark campaign. The targets are ready for a future physical
comparison campaign once validated mappings, physical classifications, and
calibrations exist.

## Source Policy

- Source: Particle Data Group 2025 listings.
- Citation: S. Navas et al. (Particle Data Group), Phys. Rev. D 110, 030001
  (2024) and 2025 update.
- Retrieval date: 2026-04-25.
- Active campaign use: blocked until physical mappings and calibrations are
  validated.

## Targets

- W boson mass: 80.3692 +/- 0.0133 GeV.
- Z boson mass: 91.1880 +/- 0.0020 GeV.
- Higgs mass: 125.20 +/- 0.11 GeV.
- W/Z mass ratio: 0.88136 +/- 0.00015.

## Status

These are physical target inputs, not successful predictions. They intentionally
do not appear in the current Phase V benchmark campaign target table. The first
physical comparison campaign should reference this table only after it can
produce the corresponding physical observables.

## Prediction Projection Contract

Phase XVIII also adds a report-level physical prediction projection path. A
record can become `predicted` only when all of the following are true:

- The mapping is `validated` and declares `targetPhysicalObservableId`.
- The source observable is classified as `physical-observable` with physical
  claims allowed.
- A validated calibration exists for the mapping.

The active reference campaign still emits only a blocked diagnostic projection
for the photon placeholder. That is expected: its source quantity remains a
benchmark eigenvalue ratio, not a physical boson observable.

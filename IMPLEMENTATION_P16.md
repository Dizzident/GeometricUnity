# IMPLEMENTATION_P16.md

## Purpose

Phase XVI starts the transition from benchmark validation to real boson-property
comparison.

The current application can compute internal observables and compare them to
targets. It cannot yet claim predictions for physical bosons because the project
does not have a defensible mapping from theory outputs to measured quantities
such as W/Z/Higgs masses, widths, branching ratios, or electroweak couplings.

This phase is intentionally split into sub-phases. A direct jump to PDG-style
particle comparisons would risk comparing numbers with no validated physical
meaning.

## Current Capability

The Phase XIV/XV campaign supports:

- quantitative observable records with provenance and uncertainty;
- target tables with evidence tiers and environment selectors;
- fail-closed target coverage;
- DOI-backed external benchmark comparison;
- campaign reports that expose missing targets and active blockers.

Current observables include internal benchmark quantities such as:

- `bosonic-eigenvalue-ratio-1`;
- `bosonic-eigenvalue-ratio-2`;
- `bosonic-eigenvalue-ratio-3`;
- `fermionic-eigenvalue-ratio-1`;
- `fermionic-eigenvalue-ratio-2`.

These are not yet physical boson observables.

## Phase Split

### Phase XVI: Physical Observable Contract

Define what a physical boson observable means in this codebase.

Phase XVI should not try to match PDG values yet. It should build the contract
that makes such a comparison meaningful.

### Phase XVII: Unit And Scale Calibration

Add the scale-setting and unit-conversion machinery needed to turn dimensionless
internal quantities into physical units or standard dimensionless couplings.

### Phase XVIII: Experimental Target Integration

Add PDG or similarly authoritative experimental target tables only after the
observable mapping and scale calibration are explicit.

### Phase XIX: First Physical Boson Comparison Campaign

Run the first fail-closed campaign against real boson properties and report
whether the theory output matches, fails, or is not yet computable.

## Phase XVI Goal

At the end of Phase XVI, the repository should be able to say:

- which computed theory artifacts are candidates for physical boson properties;
- which particle each candidate is allowed to claim;
- which observable type is being claimed, such as mass, width, coupling, or
  branching ratio;
- what assumptions connect the computed quantity to that physical property;
- whether the mapping is validated, provisional, or blocked.

## Work Items

### P16-M1 Add Physical Observable Mapping Schema

Create a checked-in schema and model for physical observable mappings.

The mapping should include:

- mapping id;
- particle id, for example `photon`, `w-boson`, `z-boson`, `higgs`;
- physical observable type, for example `mass`, `width`, `coupling`,
  `branching-ratio`;
- source computed observable id;
- required environment or branch selectors;
- unit family, for example `dimensionless`, `mass-energy`, `inverse-time`;
- status: `validated`, `provisional`, or `blocked`;
- assumptions;
- closure requirements for blocked mappings.

Definition of done:

- schema file exists;
- C# model exists;
- tests cover serialization and required fields;
- at least one blocked placeholder mapping is checked in to demonstrate honest
  fail-closed behavior.

### P16-M2 Add Mapping Validator

Add validation that prevents a physical target from being compared unless a
valid mapping exists.

Definition of done:

- campaign validation fails when a physical target lacks a mapping;
- campaign validation fails when a mapping is blocked;
- campaign validation passes for non-physical benchmark targets without a
  physical mapping;
- errors are plain English.

### P16-M3 Classify Existing Observables

Add a checked-in classification table for current observables.

Each observable should be classified as one of:

- internal-control;
- internal-benchmark;
- external-lattice-benchmark;
- physical-candidate;
- physical-observable.

Definition of done:

- existing `bosonic-eigenvalue-ratio-*` observables are explicitly classified as
  benchmark quantities, not physical boson properties;
- no current observable is silently treated as a W/Z/Higgs/photon property;
- report output includes this classification.

### P16-M4 Add Physical Claim Gate

Add a gate that blocks any report language implying real particle prediction
unless the required mapping, calibration, and target evidence are present.

Definition of done:

- reports can say "benchmark comparison passed";
- reports cannot say "boson prediction passed" unless physical claim gates pass;
- active fatal/high falsifiers still block physical claims;
- tests cover both blocked and allowed language.

### P16-M5 Create Physical Mapping Study File

Add a study document that lists candidate physical boson mappings and why each
one is not yet validated.

Initial candidates:

- photon: masslessness / gauge mode identification;
- W boson: charged weak-vector mass candidate;
- Z boson: neutral weak-vector mass candidate;
- Higgs: scalar excitation mass candidate;
- electroweak mixing angle: coupling/mixing candidate.

Definition of done:

- each candidate has a status and closure requirement;
- none are marked validated without supporting code and tests;
- next phase inputs are clear.

## Phase XVII Preview: Unit And Scale Calibration

Phase XVII should add:

- scale-setting artifacts;
- conversion from internal spectral units to GeV or dimensionless physical
  constants;
- uncertainty propagation through scale setting;
- calibration provenance;
- tests showing that uncalibrated quantities cannot be compared to physical
  targets.

## Phase XVIII Preview: Experimental Target Integration

Phase XVIII should add:

- PDG-derived or otherwise authoritative target tables;
- citations and retrieval date;
- target uncertainty and correlation handling;
- licensing/citation notes;
- tests preventing stale or uncited target values.

Because experimental constants can change, Phase XVIII should verify values
against current sources when implemented.

## Phase XIX Preview: First Physical Comparison Campaign

Phase XIX should run the first honest particle-property campaign.

Possible first targets:

- photon mass upper bound, if the theory produces a protected massless mode;
- W/Z mass ratio, if calibrated vector modes exist;
- Higgs-to-Z mass ratio, if scalar/vector mode identification exists.

The campaign should report:

- pass;
- fail;
- not computable;
- blocked by missing mapping;
- blocked by missing calibration;
- blocked by active falsifier.

## Plain-English Success Criteria

Phase XVI succeeds if the repository can no longer confuse benchmark success
with physical particle prediction. It should make the missing scientific bridge
explicit and enforce it in code.

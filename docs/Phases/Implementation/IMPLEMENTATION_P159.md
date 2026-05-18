# Implementation P159: Mode-Branch Projector Derivation Experiment

## Status

Implemented `studies/phase159_mode_branch_projector_derivation_experiment_001`.

## Purpose

P159 creates a concrete target-independent calculation for the missing mode-to-branch map. It computes gauge-sector reduced density matrices from repaired fermion eigenvectors, diagonalizes them, and gates whether a canonical projector is sufficiently pure and stable to become a P140 intake candidate.

## Promotion Rule

A projector-derived branch map is promotable only if every repaired quality row has:

- gauge-density purity at or above `0.80`
- leading eigenvalue gap at or above `0.20`
- enough transition evidence to combine the projector with a W/Z bridge

## Result

The phase output records whether the calculation produced a promotable artifact or a sharper blocker for the current repaired modes.

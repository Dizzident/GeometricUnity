# Phase502: Phase456 adaptive-calibration protocol specification

Phase502 implements the A12 specification step without launching HMC,
benchmarking, or reprocessing. Its preregistration contract is read and
validated before its exact-bound Phase501 and Phase456 precursors.

The protocol retains configuration-level, per-site and per-face observables
at every temporal separation, with explicit chain, batch, trajectory,
measurement, RNG-stream, schema, and fingerprint identities. Four independent
fixed-seed chains are analyzed only at whole-batch checkpoints. Covariance,
autocorrelation, ESS, split-R-hat, checkpoint stability, invalid-data, and
cost rules are frozen rather than inferred from the inadequate retained T=4
artifact.

The first stage calibrates T=16. T=32 is conditional on a valid, stable T=16
stage exposing a frozen temporal-boundary trigger. No synthetic covariance or
ESS is called empirical. A future executable must independently freeze and
exact-bind the implementation, defaults, RNG, topology, schema, this contract,
and both precursor hashes, refusing to run on any mismatch.

This is a planning specification, not a Phase481 repair pack or acquisition.
It does not satisfy Phase458, discharge O4, authorize production, fill a source
contract, or promote a lattice quantity to a physical mass.
`promotedPhysicalMassClaimCount=0`.

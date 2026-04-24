# External Lattice Gauge Observable Gap

Phase XIV investigated whether the campaign can compute
`bosonic-eigenvalue-ratio-1` for
`env-external-lattice-gauge-su2-2d-v1`.

Current result: it cannot be computed from the checked-in artifacts.

The repository contains an `EnvironmentRecord` for this environment, but that
record is only a summarized environment descriptor. It has:

- environment id: `env-external-lattice-gauge-su2-2d-v1`;
- tier: `imported`;
- dimensions: base `2`, ambient `2`;
- topology counts: `12` edges and `4` faces;
- provenance fields: `datasetId`, `sourceHash`, and `conversionVersion`.

It does not contain:

- the imported source mesh file;
- vertex coordinates, face incidence, edge orientation, or lattice field data;
- a checked-in source path that can be re-imported;
- enough numeric spectrum data to compute an eigenvalue ratio.

The `sourceSpec` value is `external:lattice-gauge-su2-2d-v1`, not a local file
path. The checked-in `sourceHash` is also not verifiable against a source file in
this repository.

Because of that, adding a matching observable for this environment to
`observables.json` would be fabricating a computed result.

The active reference campaign no longer uses this placeholder target. It has
been superseded by the DOI-backed Zenodo SU(2) plaquette-chain benchmark
recorded in `zenodo_su2_plaquette_chain_benchmark.md`.

Closure requirement:

1. Check in or reference an accessible immutable external mesh/field artifact for
   `external-lattice-gauge-su2-2d-v1`.
2. Verify its hash against `sourceHash` or replace the placeholder hash with the
   actual content hash.
3. Run an observable extraction path that produces a
   `QuantitativeObservableRecord` for `bosonic-eigenvalue-ratio-1` on
   `env-external-lattice-gauge-su2-2d-v1`.
4. Add that record to `observables.json` with an uncertainty budget and
   provenance.
5. Reintroduce an explicit target for this environment only after those data
   exist.

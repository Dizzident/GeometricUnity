# Phase417 - Vector-Spinor 144 Decomposition Probe

## Purpose

Materialize the Phase416 next branch: the source-pinned `Z_{1/2}` dark carrier
uses the vector-spinor `144` remainder in `10 x 16 = 16 + 144`. This phase
computes that remainder directly as the gamma-trace kernel and decomposes it
under the same SM and welded-spin actions used by the previous phases.

## Result

- The gamma-trace split is exact at current tolerance: real rank `32`, real
  kernel dimension `288`, i.e. complex dimension `144`.
- The kernel is invariant under the probed welded and SM generators.
- The actual chiral `2 x 144` carrier contains no linear welded spin-zero
  component in the computed representation content.
- The source still supplies no bosonic projection map, action, VEV selection,
  observed photon/W/Z/H projection rows, weak-angle lineage, pole extraction,
  or GeV/unit normalization.

## Boundary

This is representation bookkeeping and a fail-closed source-boundary probe. It
does not test arbitrary non-source-defined composites, fills no Phase201 or
Phase256 contract field, and promotes no W/Z/H mass claim.

## Validation

Run:

```bash
dotnet run --project studies/phase417_vector_spinor_144_decomposition_probe_001/Phase417VectorSpinor144DecompositionProbe.csproj
```

# Phase IV Fermion Family Atlas Study 001

**Study ID:** Phase4-FermionFamily-Atlas-001
**Milestone:** M45 — End-to-end Phase IV reference study
**Status:** Scaffold (pending M44 CUDA parity closure)

## Purpose

This study runs the complete Phase IV fermionic pipeline end-to-end on a Toy2D mesh with
su(2) gauge group, producing a `FermionFamilyAtlas` that can serve as a canonical reference
artifact for Phase IV.

The study exercises:
1. Spin connection assembly (M35: `CpuDiracOperatorAssembler`)
2. Dirac operator construction (M36)
3. Chirality and conjugation analysis (M37)
4. Fermionic spectral solving (M38: `FermionSpectralSolver`)
5. Fermion mode tracking and persistence atlas (M39: `FermionFamilyAtlasBuilder`)
6. Boson-fermion coupling proxy engine (M40: `CouplingAtlas`)
7. Generation/family clustering (M41: `FamilyClusteringEngine`)
8. Unified particle registry (M42: `RegistryMergeEngine`)
9. Observation and comparison extension (M43)

## Geometry

- **Base space X:** Toy2D mesh (2D simplicial, dimX=2)
- **Ambient space Y:** dimY=5 (minimal odd-dimensional case with nontrivial chirality on X)
- **Gauge group:** su(2), dimG=3
- **Spinor dimension:** 4 (Cl(5,0), Riemannian signature)
- **DOF per cell:** spinorDim * dimG = 12

## Omega Profile

The bosonic background uses a **cos/sin edge-varying profile** to ensure spatially varying
curvature in all su(2) directions:

```
omega_e = cos(pi * mx_e) * T_1 + sin(pi * mx_e) * T_2
```

where `mx_e` is the x-coordinate of the midpoint of edge `e`.

**Rationale:** This generates curvature in all 3 su(2) directions via [T_1, T_2] = i*T_3,
breaking boundary-operator cancellation and ensuring a nontrivial Dirac gauge coupling.
A constant or purely linear omega produces trivially zero Jacobian contributions.
Profile recorded as `omegaProfile: "cos-sin-su2-v1"`.

## Expected Outputs

- `FermionFamilyAtlas` with at least 2 families (left/right chiral pair expected from su(2))
- `FamilyClusterRecord` list with at least 1 conjugate-pair cluster
- `CouplingAtlas` with nonzero coupling proxy magnitudes
- `UnifiedParticleRegistry` with Fermion + Interaction entries at C1 or above
- Chirality summary: left/right/conjugate-pair families identified
- Branch persistence score > 0.5 for stable families

## Definition of Done (M45)

Per P4-IA §10.1:
1. FermionFamilyAtlas produced with ≥2 families and nonzero branch persistence scores
2. At least one conjugate pair identified by FamilyClusteringEngine
3. CouplingAtlas produced with ≥1 above-threshold coupling record
4. UnifiedParticleRegistry serializes and deserializes round-trip (JSON)
5. All pipeline stages emit ProvenanceMeta with consistent CodeRevision
6. Study passes schema validation (registry schema version 1.0.0)
7. Results committed to studies/phase4_fermion_family_atlas_001/output/

## Files

```
studies/phase4_fermion_family_atlas_001/
├── STUDY.md                    (this file)
├── config/
│   └── study_config.json       (study parameters and pipeline config)
└── run.sh                      (pipeline runner — implemented after M44 lands)
```

## Prerequisites

- M44 (CUDA parity closure) must be complete before running
- dotnet build must be clean (0 errors, 0 warnings)

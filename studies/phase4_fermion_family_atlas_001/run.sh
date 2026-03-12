#!/usr/bin/env bash
# M45: Phase IV Fermion Family Atlas Study 001 Runner
# Study: Phase4-FermionFamily-Atlas-001
#
# Prerequisites: M44 (CUDA parity closure) must be complete.
#
# Pipeline stages:
#   1. Build Toy2D simplicial mesh (dimX=2, dimY=5, su(2))
#   2. Assemble spin connection bundle (flat LC + cos-sin-su2-v1 gauge coupling)
#   3. Assemble Dirac operator (CpuDiracOperatorAssembler)
#   4. Analyze chirality and conjugation (ChiralityAnalyzer, ConjugationAnalyzer)
#   5. Solve fermionic spectrum (FermionSpectralSolver, 6 eigenvalues)
#   6. Build fermion family atlas (FermionFamilyAtlasBuilder, M39)
#   7. Compute boson-fermion coupling atlas (M40)
#   8. Cluster families into generations (FamilyClusteringEngine, M41)
#   9. Build unified particle registry (RegistryMergeEngine, M42)
#  10. Run observation and comparison (M43)
#  11. Emit study report
#
# TODO: Implement runner after M44 lands.
# The runner will call `dotnet run --project apps/Gu.Cli -- run-phase4-study \
#   --config studies/phase4_fermion_family_atlas_001/config/study_config.json \
#   --output studies/phase4_fermion_family_atlas_001/output/`

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
STUDY_DIR="${REPO_ROOT}/studies/phase4_fermion_family_atlas_001"
CONFIG="${STUDY_DIR}/config/study_config.json"
OUTPUT_DIR="${STUDY_DIR}/output"

echo "=== M45: Phase IV Fermion Family Atlas Study 001 ==="
echo "Study dir: ${STUDY_DIR}"
echo "Config:    ${CONFIG}"
echo "Output:    ${OUTPUT_DIR}"
echo ""

# Build
echo "--- Building solution ---"
cd "${REPO_ROOT}"
dotnet build --configuration Release -nologo 2>&1

echo ""
echo "--- Running M45 study tests ---"
dotnet test --no-build --configuration Release \
  --filter "FullyQualifiedName~Phase4FermionFamilyAtlas001" \
  -nologo 2>&1

echo ""
echo "--- Study complete ---"
echo "Outputs written to: ${OUTPUT_DIR}"
echo ""
echo "Key assertions verified:"
echo "  [ ] FermionFamilyAtlas produced with >=2 families"
echo "  [ ] At least one conjugate-pair cluster identified"
echo "  [ ] CouplingAtlas produced with >=1 above-threshold coupling"
echo "  [ ] UnifiedParticleRegistry JSON round-trip passes"
echo "  [ ] All provenance CodeRevision fields consistent"
echo "  [ ] Registry schema validation passes (v1.0.0)"
echo ""
echo "NOTE: Runner not yet implemented — awaiting M44 completion."
echo "      Remove this note and implement the dotnet CLI call above when M44 lands."

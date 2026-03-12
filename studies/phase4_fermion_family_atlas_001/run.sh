#!/usr/bin/env bash
# M45: Phase IV Fermion Family Atlas Study 001 Runner
# Study: Phase4-FermionFamily-Atlas-001
#
# Prerequisites: dotnet build must succeed (all Phase IV milestones M33-M44 complete).
#
# Pipeline stages executed in-process by Phase4FermionFamilyAtlasStudy.Run():
#   1. Build Toy2D simplicial mesh (dimX=2, dimY=5, su(2))
#   2. Assemble spin connection bundle (cos-sin-su2-v1 profile)
#   3. Assemble Dirac operator (CpuDiracOperatorAssembler)
#   4. Solve fermionic spectrum (FermionSpectralBundleBuilder, 6 eigenvalues)
#   5. Build fermion family atlas (FermionFamilyAtlasBuilder, M39)
#   6. Cluster families (FamilyClusteringEngine, M41)
#   7. Compute boson-fermion coupling atlas (CouplingProxyEngine, M40)
#   8. Build unified particle registry (UnifiedRegistryBuilder, M42)
#   9. Run fermionic observation pipeline (FermionObservationPipeline, M43)
#  10. Generate Phase IV report (Phase4ReportGenerator)
#  11. Write artifacts to output directory

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
STUDY_DIR="${REPO_ROOT}/studies/phase4_fermion_family_atlas_001"
OUTPUT_DIR="${STUDY_DIR}/output"

echo "=== M45: Phase IV Fermion Family Atlas Study 001 ==="
echo "Repo root:  ${REPO_ROOT}"
echo "Study dir:  ${STUDY_DIR}"
echo "Output dir: ${OUTPUT_DIR}"
echo ""

# Build the full solution (Release configuration)
echo "--- Building solution (Release) ---"
cd "${REPO_ROOT}"
dotnet build --configuration Release -nologo

echo ""
echo "--- Running Phase4FermionFamilyAtlas001 integration tests ---"
dotnet test --no-build --configuration Release \
  --filter "FullyQualifiedName~Phase4FermionFamilyAtlas001" \
  tests/Gu.Phase4.IntegrationTests/Gu.Phase4.IntegrationTests.csproj \
  -nologo

echo ""
echo "--- Study complete ---"
echo "Outputs written to: ${OUTPUT_DIR}"
echo ""
echo "Artifacts produced:"
echo "  dirac_bundle.json"
echo "  fermion_spectral_bundle.json"
echo "  fermion_family_atlas.json"
echo "  family_clusters.json"
echo "  coupling_atlas.json"
echo "  unified_registry.json"
echo "  observation_summaries.json"
echo "  phase4_report.json"
echo "  REPORT.md"
echo ""
echo "NOTICE: This study demonstrates branch-consistent pipeline execution."
echo "        It does NOT constitute physical validation."

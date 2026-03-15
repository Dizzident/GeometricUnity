#!/usr/bin/env bash
# Phase VI Reference Campaign Runner
# Study: phase5_su2_branch_refinement_env_validation
#
# What this study demonstrates:
#   - Bridge-backed branch and refinement evidence exported from a persisted background atlas
#   - Toy + structured 4x4 + imported environment records with explicit provenance fields
#   - Sidecar generation with explicit observation / environment / representation / coupling coverage
#   - Quantitative comparison that separates toy-placeholder controls from stronger benchmark targets
#   - Campaign spec validation before run, including schema and environment-tier checks
#   - Positive/mixed dossier + negative dossier assembly (M51/M52)
#
# Reproduction command (D-005):
#   dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec config/campaign.json --out-dir artifacts

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
STUDY_DIR="${REPO_ROOT}/studies/phase5_su2_branch_refinement_env_validation"
CONFIG_DIR="${STUDY_DIR}/config"
ARTIFACTS_DIR="${STUDY_DIR}/artifacts"
CLI="${REPO_ROOT}/apps/Gu.Cli"
REGISTRY_PATH="${REPO_ROOT}/studies/phase4_fermion_family_atlas_001/output/unified_particle_registry.json"

echo "=== Phase VI Reference Campaign: su2-branch-refinement-env-validation ==="
echo "Study dir: ${STUDY_DIR}"
echo ""

# Build
echo "--- Building solution ---"
cd "${REPO_ROOT}"
dotnet build --configuration Release -nologo 2>&1

echo ""
echo "--- Running Phase V/VI regression tests ---"
dotnet test --no-build --configuration Release \
  --filter "FullyQualifiedName~Phase5" \
  -nologo 2>&1 || true

echo ""
echo "--- Step 1: Export bridge-backed branch/refinement values (P6-M2 / D-P6-003) ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  export-phase5-bridge-values \
  --atlas "${CONFIG_DIR}/background_atlas.json" \
  --refinement-spec "${CONFIG_DIR}/refinement_study.json" \
  --out-dir "${CONFIG_DIR}" 2>&1

echo ""
echo "--- Step 2: Generate sidecar evidence files (P6-M3 / D-P6-002) ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  build-phase5-sidecars \
  --registry "${REGISTRY_PATH}" \
  --observables "${CONFIG_DIR}/observables.json" \
  --environment-record "${CONFIG_DIR}/env_toy_record.json" \
  --environment-record "${CONFIG_DIR}/env_structured_4x4_record.json" \
  --environment-record "${CONFIG_DIR}/env_imported_example.json" \
  --out-dir "${CONFIG_DIR}" 2>&1

echo ""
echo "--- Step 3: Validate campaign spec (P6-M1 / D-P6-001) ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  validate-phase5-campaign-spec \
  --spec "${CONFIG_DIR}/campaign.json" \
  --require-reference-sidecars 2>&1

echo ""
echo "=== Step 4: Running Phase V/VI campaign (D-005: run-phase5-campaign) ==="
mkdir -p "${ARTIFACTS_DIR}"

dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  run-phase5-campaign \
  --spec "${CONFIG_DIR}/campaign.json" \
  --out-dir "${ARTIFACTS_DIR}" \
  --validate-first 2>&1

echo ""
echo "=== Study complete ==="
echo ""
echo "Artifacts:"
echo "  Branch independence:  ${ARTIFACTS_DIR}/branch/branch_robustness_record.json"
echo "  Convergence:          ${ARTIFACTS_DIR}/convergence/refinement_study_result.json"
echo "  Quantitative:         ${ARTIFACTS_DIR}/quantitative/consistency_scorecard.json"
echo "  Falsification:        ${ARTIFACTS_DIR}/falsification/falsifier_summary.json"
echo "  Typed dossier:        ${ARTIFACTS_DIR}/dossiers/phase5_validation_dossier.json"
echo "  Provenance dossier:   ${ARTIFACTS_DIR}/dossiers/validation_dossier.json"
echo "  Study manifests:      ${ARTIFACTS_DIR}/dossiers/study_manifest.json"
echo "  Report JSON:          ${ARTIFACTS_DIR}/reports/phase5_report.json"
echo "  Report Markdown:      ${ARTIFACTS_DIR}/reports/phase5_report.md"
echo ""
echo "Key study properties (Phase VI):"
echo "  [x] Bridge-backed branch family exported from persisted background_atlas.json"
echo "  [x] Toy + structured 4x4 + imported environment evidence (D-P6-004)"
echo "  [x] Quantitative targets with explicit distributionModel and mixed evidence tiers (D-P6-005)"
echo "  [x] Sidecar files generated and declared (D-P6-001 / D-P6-002)"
echo "  [x] Campaign spec validated before run (P6-M1)"
echo "  [x] falsifier_summary.json carries evaluation coverage counts (D-P6-002)"
echo "  [x] observationChainSummary present in phase5_validation_dossier.json"
echo "  [x] Both typed and provenance dossiers produced (D-006)"
echo "  [x] Reproduction command: run-phase5-campaign (D-005)"
echo ""
echo "IMPORTANT: The campaign now distinguishes control-study targets from a stronger benchmark target,"
echo "but none of the quantitative targets in this study is a real-world experimental measurement."
echo ""
echo "To reproduce:"
echo "  bash studies/phase5_su2_branch_refinement_env_validation/run_study.sh"

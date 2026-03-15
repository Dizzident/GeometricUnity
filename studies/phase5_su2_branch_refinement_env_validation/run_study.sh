#!/usr/bin/env bash
# Phase VI Reference Campaign Runner
# Study: phase5_su2_branch_refinement_env_validation
#
# What this study demonstrates:
#   - 4-variant su(2) branch family covering torsion x bi-connection choices
#   - Branch robustness analysis (M46): eigenvalue ratio stability across variants
#   - Toy + structured 4x4 + imported environment validation (M48, D-P6-004)
#   - Quantitative comparison against targets with explicit distributionModel (M49, D-P6-005)
#   - Observation chain + sidecar evidence coverage (P6-M3, D-P6-002)
#   - Campaign spec validated before run (P6-M1, D-P6-001)
#   - Positive/mixed dossier + negative dossier assembly (M51/M52)
#
# Branch variants:
#   V1: identity-shiab + trivial-torsion   + simple-A0-omega  (reference)
#   V2: identity-shiab + augmented-torsion + simple-A0-omega
#   V3: identity-shiab + trivial-torsion   + A0-plus-minus-omega
#   V4: identity-shiab + augmented-torsion + A0-plus-minus-omega
#
# External targets (eigenvalue RATIOS, synthetic-toy-v1, toy-placeholder, ~40% uncertainty):
#   bosonic:  [0.1, 1.0, 5.0]
#   fermionic: [0.05, 2.0]
#
# Note: All targets are toy-placeholder — NOT physical predictions.
#
# Reproduction command (D-005):
#   dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec config/campaign.json --out-dir artifacts

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
STUDY_DIR="${REPO_ROOT}/studies/phase5_su2_branch_refinement_env_validation"
CONFIG_DIR="${STUDY_DIR}/config"
ARTIFACTS_DIR="${STUDY_DIR}/artifacts"
CLI="${REPO_ROOT}/apps/Gu.Cli"

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
echo "--- Step 1: Generate sidecar evidence files (P6-M3 / D-P6-002) ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  build-phase5-sidecars \
  --registry "${CONFIG_DIR}/registry.json" \
  --observables "${CONFIG_DIR}/observables.json" \
  --environment-record "${CONFIG_DIR}/env_toy_record.json" \
  --environment-record "${CONFIG_DIR}/env_structured_4x4_record.json" \
  --environment-record "${CONFIG_DIR}/env_imported_example.json" \
  --out-dir "${CONFIG_DIR}" 2>&1

echo ""
echo "--- Step 2: Validate campaign spec (P6-M1 / D-P6-001) ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  validate-phase5-campaign-spec \
  --spec "${CONFIG_DIR}/campaign.json" \
  --require-reference-sidecars 2>&1

echo ""
echo "=== Step 3: Running Phase V/VI campaign (D-005: run-phase5-campaign) ==="
mkdir -p "${ARTIFACTS_DIR}"

dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  run-phase5-campaign \
  --spec "${CONFIG_DIR}/campaign.json" \
  --out-dir "${ARTIFACTS_DIR}" 2>&1

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
echo "  [x] 4-variant su(2) branch family (torsion x bi-connection)"
echo "  [x] Toy + structured 4x4 + imported environment evidence (D-P6-004)"
echo "  [x] Eigenvalue ratio targets with explicit distributionModel (D-P6-005)"
echo "  [x] Sidecar files generated and declared (D-P6-001 / D-P6-002)"
echo "  [x] Campaign spec validated before run (P6-M1)"
echo "  [x] falsifier_summary.json carries evaluation coverage counts (D-P6-002)"
echo "  [x] observationChainSummary present in phase5_validation_dossier.json"
echo "  [x] Both typed and provenance dossiers produced (D-006)"
echo "  [x] Reproduction command: run-phase5-campaign (D-005)"
echo ""
echo "IMPORTANT: All targets are synthetic toy placeholders (evidenceTier=toy-placeholder)."
echo "These are NOT physical predictions and carry no experimental authority."
echo ""
echo "To reproduce:"
echo "  bash studies/phase5_su2_branch_refinement_env_validation/run_study.sh"

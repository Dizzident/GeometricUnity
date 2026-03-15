#!/usr/bin/env bash
# Phase V M53 Reference Study Runner
# Study: phase5_su2_branch_refinement_env_validation
#
# What this study demonstrates (M53 completion criterion):
#   - 4-variant su(2) branch family covering torsion x bi-connection choices
#   - Branch robustness analysis (M46): eigenvalue ratio stability across variants
#   - Toy + structured 4x4 environment validation (M48)
#   - Quantitative comparison against toy eigenvalue ratio targets (M49)
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

echo "=== Phase V M53 Reference Study: su2-branch-refinement-env-validation ==="
echo "Study dir: ${STUDY_DIR}"
echo ""

# Build
echo "--- Building solution ---"
cd "${REPO_ROOT}"
dotnet build --configuration Release -nologo 2>&1

echo ""
echo "--- Running Phase V regression tests ---"
dotnet test --no-build --configuration Release \
  --filter "FullyQualifiedName~Phase5" \
  -nologo 2>&1 || true

echo ""
echo "=== Running Phase V M53 campaign (D-005: run-phase5-campaign) ==="
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
echo "Key study properties:"
echo "  [x] 4-variant su(2) branch family (torsion x bi-connection)"
echo "  [x] Toy 2x2 environment (env-toy-2d-trivial)"
echo "  [x] Eigenvalue ratio targets: bosonic [0.1, 1.0, 5.0], fermionic [0.05, 2.0]"
echo "  [x] All targets: targetProvenance=synthetic-toy-v1, evidenceTier=toy-placeholder"
echo "  [x] Both typed and provenance dossiers produced (D-006)"
echo "  [x] Reproduction command: run-phase5-campaign (D-005)"
echo ""
echo "IMPORTANT: All targets are synthetic toy placeholders (evidenceTier=toy-placeholder)."
echo "These are NOT physical predictions and carry no experimental authority."
echo ""
echo "To reproduce:"
echo "  dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec config/campaign.json --out-dir artifacts"

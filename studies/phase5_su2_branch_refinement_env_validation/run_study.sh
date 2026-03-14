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
echo "=== Step 1: Generate structured environments (M48) ==="
mkdir -p "${ARTIFACTS_DIR}/environments"

echo "--- Toy 2x2 environment ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  build-structured-environment "${CONFIG_DIR}/env_toy_2d.json" \
  --out "${ARTIFACTS_DIR}/environments/env-toy-2d-trivial.json" 2>&1

echo ""
echo "--- Structured 4x4 environment ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  build-structured-environment "${CONFIG_DIR}/env_structured_4x4.json" \
  --out "${ARTIFACTS_DIR}/environments/env-structured-4x4.json" 2>&1

echo ""
echo "=== Step 2: Branch robustness study (M46) ==="
mkdir -p "${ARTIFACTS_DIR}/branch_independence"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  branch-robustness "${CONFIG_DIR}/branch_robustness_study.json" \
  --values "${CONFIG_DIR}/branch_quantity_values.json" \
  --out "${ARTIFACTS_DIR}/branch_independence/branch_robustness_record.json" 2>&1

echo ""
echo "=== Step 3: Quantitative validation (M49) ==="
mkdir -p "${ARTIFACTS_DIR}/quantitative_validation"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  validate-quantitative "${CONFIG_DIR}/observables.json" \
  --targets "${CONFIG_DIR}/external_targets.json" \
  --study-id "phase5-su2-branch-refinement-env-validation" \
  --out "${ARTIFACTS_DIR}/quantitative_validation/consistency_scorecard.json" 2>&1

echo ""
echo "=== Step 4: Build validation dossiers (M51/M52) ==="
mkdir -p "${ARTIFACTS_DIR}/dossiers"

echo "--- Positive/mixed dossier ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  build-validation-dossier "${ARTIFACTS_DIR}" \
  --study "phase5-su2-positive:${ARTIFACTS_DIR}:phase5-m53-ref-study-v1:bash run_study.sh" \
  --title "Phase V Positive/Mixed Dossier: su2-branch-refinement-env-validation" \
  --out "${ARTIFACTS_DIR}/dossiers/positive_dossier.json" 2>&1

echo ""
echo "--- Negative result dossier ---"
dotnet run --no-build --configuration Release \
  --project "${CLI}" -- \
  build-validation-dossier "${ARTIFACTS_DIR}" \
  --study "phase5-su2-negative:${ARTIFACTS_DIR}:phase5-m53-ref-study-v1:bash run_study.sh" \
  --title "Phase V Negative Result Dossier: su2-branch-refinement-env-validation" \
  --out "${ARTIFACTS_DIR}/dossiers/negative_dossier.json" 2>&1

echo ""
echo "=== Study complete ==="
echo ""
echo "Artifacts:"
echo "  Toy environment:      ${ARTIFACTS_DIR}/environments/env-toy-2d-trivial.json"
echo "  Structured env:       ${ARTIFACTS_DIR}/environments/env-structured-4x4.json"
echo "  Branch independence:  ${ARTIFACTS_DIR}/branch_independence/branch_robustness_record.json"
echo "  Quantitative:         ${ARTIFACTS_DIR}/quantitative_validation/consistency_scorecard.json"
echo "  Positive dossier:     ${ARTIFACTS_DIR}/dossiers/positive_dossier.json"
echo "  Negative dossier:     ${ARTIFACTS_DIR}/dossiers/negative_dossier.json"
echo ""
echo "Key study properties:"
echo "  [x] 4-variant su(2) branch family (torsion x bi-connection)"
echo "  [x] Toy 2x2 + structured 4x4 environments"
echo "  [x] Eigenvalue ratio targets: bosonic [0.1, 1.0, 5.0], fermionic [0.05, 2.0]"
echo "  [x] All targets: targetProvenance=synthetic-toy-v1, evidenceTier=toy-placeholder"
echo "  [x] Positive/mixed dossier + negative dossier produced"
echo ""
echo "IMPORTANT: All targets are synthetic toy placeholders (evidenceTier=toy-placeholder)."
echo "These are NOT physical predictions and carry no experimental authority."
echo ""
echo "To reproduce: bash ${ARTIFACTS_DIR}/reproduce.sh"

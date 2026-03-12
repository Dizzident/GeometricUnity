#!/usr/bin/env bash
# P4-C3: Nontrivial Bosonic Validation Study Runner
# Study: bosonic_validation_001
#
# What this study tests:
#   - augmented-torsion branch: T^aug = d_A0(omega - A0)
#   - identity-shiab branch: S = F (curvature)
#   - nonzero initial omega (constant 0.3 on first su(2) generator per edge)
#   - nonzero background A0 (constant 0.2 on second su(2) generator per edge)
#   - su(2) gauge group on a single tetrahedron (4 vertices, 6 edges, 4 faces)
#
# Expected nonzero artifacts:
#   - Residual Upsilon_h = S_h - T_h (nonzero because both terms are nonzero)
#   - Curvature F_h (nonzero because omega is nonzero and has bracket contribution)
#   - Torsion T_h (nonzero because A0 != 0 and omega != A0)
#   - Jacobian J (nontrivial because torsion Jacobian dT/domega = d_A0 is nonzero)
#   - Gradient G = J^T M Upsilon (nonzero because Upsilon is nonzero)
#   - Spectrum (nontrivial eigenvalues because Hessian H = J^T M J + lambda*C^T C)
#
# The study preserves the negative result if the branch is unstable:
#   solver may not converge (Mode A only evaluates, does not descend).

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
STUDY_DIR="${REPO_ROOT}/studies/bosonic_validation_001"

echo "=== P4-C3: Bosonic Validation Study 001 ==="
echo "Study dir: ${STUDY_DIR}"
echo ""

# Build
echo "--- Building solution ---"
cd "${REPO_ROOT}"
dotnet build --configuration Release -nologo 2>&1

echo ""
echo "--- Running regression tests for this study ---"
dotnet test --no-build --configuration Release \
  --filter "FullyQualifiedName~BosonicValidationStudy001" \
  -nologo 2>&1

echo ""
echo "--- Study complete ---"
echo "Branch config:      ${STUDY_DIR}/branch.json"
echo "Environment config: ${STUDY_DIR}/environment.json"
echo "Report:             ${STUDY_DIR}/REPORT.md"
echo ""
echo "Key assertions verified by regression tests:"
echo "  [x] Initial omega is nonzero (max coeff > 0)"
echo "  [x] Curvature F is nonzero (bracket contribution from nonzero omega)"
echo "  [x] Torsion T is nonzero (augmented torsion with nonzero A0, omega != A0)"
echo "  [x] Residual Upsilon = S - T is nonzero"
echo "  [x] Jacobian has nonzero entries"
echo "  [x] Gradient G = J^T M Upsilon is nonzero"
echo "  [x] Spectrum eigenvalues are nontrivial (not all zero)"
echo "  [x] Artifacts are distinct from omega=0 / A0=0 baseline"

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
OUTPUT_DIR="${STUDY_DIR}/output/run1"
BG_STUDY_JSON="${OUTPUT_DIR}/bg-study.json"
BACKGROUNDS_DIR="${OUTPUT_DIR}/backgrounds"

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

# -------------------------------------------------------------------------
# Section 2: CLI-level artifact generation (P4-C3 CLI parity check)
# -------------------------------------------------------------------------
echo ""
echo "--- CLI artifact generation (P4-C3 CLI parity) ---"
echo "Output dir: ${OUTPUT_DIR}"
echo ""

# Step 1: Initialize the run folder and write the branch manifest from the study config.
echo "  Step 1: Initializing run folder..."
dotnet run --no-build --configuration Release \
  --project "${REPO_ROOT}/apps/Gu.Cli" -- \
  init-run "${OUTPUT_DIR}" 2>&1

# Copy the study's branch manifest into the run folder (override the default).
cp "${STUDY_DIR}/branch.json" "${OUTPUT_DIR}/manifest/branch.json"
echo "  Copied study branch manifest to run folder."

# Step 2: Run the solver (Mode A: residual-only, 1 iteration) using the persisted branch manifest.
echo ""
echo "  Step 2: Running solver (Mode A) via CLI..."
dotnet run --no-build --configuration Release \
  --project "${REPO_ROOT}/apps/Gu.Cli" -- \
  run "${OUTPUT_DIR}" \
  --backend cpu \
  --mode A \
  --lie-algebra su2 \
  --max-iter 1 2>&1

echo ""
echo "  Artifacts written by 'run' command:"
echo "    manifest/branch.json  — branch identity"
echo "    state/final_state.json — final omega"
echo "    observed/observed_state.json — observables"

# Step 3: Create a background study spec pinned to the bosonic-validation-001 branch.
echo ""
echo "  Step 3: Creating background study spec..."
dotnet run --no-build --configuration Release \
  --project "${REPO_ROOT}/apps/Gu.Cli" -- \
  create-background-study "${BG_STUDY_JSON}" \
  --lie-algebra su2 \
  --mode A \
  --seeds 1 2>&1

# Step 4: Solve backgrounds (produces background_records/<bgId>.json).
echo ""
echo "  Step 4: Solving backgrounds..."
dotnet run --no-build --configuration Release \
  --project "${REPO_ROOT}/apps/Gu.Cli" -- \
  solve-backgrounds "${BG_STUDY_JSON}" \
  --output "${BACKGROUNDS_DIR}" \
  --lie-algebra su2 2>&1

# Discover the first background record ID from the atlas.
BG_ID="$(dotnet script /dev/stdin <<'DOTNET_SCRIPT' 2>/dev/null || true
using System.Text.Json;
var atlas = JsonDocument.Parse(File.ReadAllText(args[0]));
var bgs = atlas.RootElement.GetProperty("backgrounds");
if (bgs.GetArrayLength() > 0)
    Console.Write(bgs[0].GetProperty("backgroundId").GetString());
DOTNET_SCRIPT
)"

# Fallback: extract via grep if dotnet-script is unavailable.
if [ -z "${BG_ID}" ]; then
  BG_ID="$(grep -o '"backgroundId":"[^"]*"' "${BACKGROUNDS_DIR}/atlas.json" | head -1 | cut -d'"' -f4 || true)"
fi

if [ -z "${BG_ID}" ]; then
  echo "  WARNING: Could not determine background ID from atlas. Skipping compute-spectrum step."
else
  echo "  Background ID: ${BG_ID}"

  # Step 5: Compute spectrum for the first solved background.
  echo ""
  echo "  Step 5: Computing spectrum via CLI..."
  dotnet run --no-build --configuration Release \
    --project "${REPO_ROOT}/apps/Gu.Cli" -- \
    compute-spectrum "${OUTPUT_DIR}" "${BG_ID}" \
    --lie-algebra su2 \
    --num-modes 4 2>&1
fi

echo ""
echo "--- CLI artifact summary ---"
echo "  Run folder:         ${OUTPUT_DIR}"
echo "  manifest/branch.json contains branchId from study config"
echo "  state/final_state.json contains solver output"
echo "  observed/observed_state.json contains observables"
if [ -n "${BG_ID}" ]; then
  echo "  spectra/${BG_ID}_spectrum.json contains bosonic spectrum"
fi
echo ""
echo "To reproduce these CLI artifacts:"
echo "  bash ${STUDY_DIR}/artifacts/reproduce.sh [output-dir]"

#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_PROJECT="$REPO_ROOT/apps/Gu.Cli/Gu.Cli.csproj"
DEFAULT_CAMPAIGN_SPEC="$REPO_ROOT/studies/phase5_su2_branch_refinement_env_validation/config/campaign.json"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"

CAMPAIGN_SPEC="${1:-$DEFAULT_CAMPAIGN_SPEC}"
OUT_ROOT="${2:-$REPO_ROOT/reports/phase5_gap_closure_batch/$TIMESTAMP}"
ARTIFACTS_DIR="$OUT_ROOT/campaign_artifacts"
LOG_DIR="$OUT_ROOT/logs"
SUMMARY_MD="$OUT_ROOT/summary.md"

TEST_PROJECTS=(
  "$REPO_ROOT/tests/Gu.Artifacts.Tests/Gu.Artifacts.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase3.Backgrounds.Tests/Gu.Phase3.Backgrounds.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase3.Spectra.Tests/Gu.Phase3.Spectra.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase5.BranchIndependence.Tests/Gu.Phase5.BranchIndependence.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase5.Convergence.Tests/Gu.Phase5.Convergence.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase5.Dossiers.Tests/Gu.Phase5.Dossiers.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase5.Environments.Tests/Gu.Phase5.Environments.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase5.Falsification.Tests/Gu.Phase5.Falsification.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase5.QuantitativeValidation.Tests/Gu.Phase5.QuantitativeValidation.Tests.csproj"
  "$REPO_ROOT/tests/Gu.Phase5.Reporting.Tests/Gu.Phase5.Reporting.Tests.csproj"
)

REQUIRED_ARTIFACTS=(
  "branch/branch_robustness_record.json"
  "convergence/refinement_study_result.json"
  "quantitative/consistency_scorecard.json"
  "falsification/falsifier_summary.json"
  "dossiers/phase5_validation_dossier.json"
  "dossiers/validation_dossier.json"
  "dossiers/study_manifest.json"
  "reports/phase5_report.json"
  "reports/phase5_report.md"
)

mkdir -p "$LOG_DIR" "$ARTIFACTS_DIR"

if [[ ! -f "$CAMPAIGN_SPEC" ]]; then
  echo "Campaign spec not found: $CAMPAIGN_SPEC" >&2
  exit 1
fi

cd "$REPO_ROOT"

run_and_capture() {
  local name="$1"
  shift
  local log_file="$LOG_DIR/${name}.log"
  echo ""
  echo "=== $name ==="
  "$@" 2>&1 | tee "$log_file"
}

write_summary() {
  local test_count="${#TEST_PROJECTS[@]}"
  local artifact_count="${#REQUIRED_ARTIFACTS[@]}"
  cat > "$SUMMARY_MD" <<EOF
# Phase V Gap Closure Batch Summary

- Timestamp (UTC): $TIMESTAMP
- Repository: $REPO_ROOT
- Campaign spec: $CAMPAIGN_SPEC
- Output root: $OUT_ROOT
- CLI project: $CLI_PROJECT
- Test projects run: $test_count
- Required artifacts verified: $artifact_count

## Logs

- Build log: $LOG_DIR/build.log
- Campaign log: $LOG_DIR/run_phase5_campaign.log
- Test logs directory: $LOG_DIR

## Campaign Artifacts

- Artifacts root: $ARTIFACTS_DIR

EOF

  for rel in "${REQUIRED_ARTIFACTS[@]}"; do
    echo "- $ARTIFACTS_DIR/$rel" >> "$SUMMARY_MD"
  done

  cat >> "$SUMMARY_MD" <<EOF

## Evaluation Prompt

Use:

\`$REPO_ROOT/PHASE_6_EVALUATION_PROMPT.md\`

with these concrete values:

- batch summary: \`$SUMMARY_MD\`
- artifacts root: \`$ARTIFACTS_DIR\`
- logs root: \`$LOG_DIR\`
- gap handoff: \`$REPO_ROOT/GAPS_P5_01.md\`
- open issues: \`$REPO_ROOT/PHASE_5_OPEN_ISSUES.md\`

EOF
}

run_and_capture "build" dotnet build "$REPO_ROOT/GeometricUnity.slnx" -c Release -nologo

for project in "${TEST_PROJECTS[@]}"; do
  project_name="$(basename "$(dirname "$project")")"
  run_and_capture "test_${project_name}" dotnet test "$project" --no-build -c Release -nologo
done

run_and_capture "run_phase5_campaign" \
  dotnet run --project "$CLI_PROJECT" -c Release --no-build -- \
  run-phase5-campaign --spec "$CAMPAIGN_SPEC" --out-dir "$ARTIFACTS_DIR"

echo ""
echo "=== verify artifacts ==="
for rel in "${REQUIRED_ARTIFACTS[@]}"; do
  full="$ARTIFACTS_DIR/$rel"
  if [[ ! -f "$full" ]]; then
    echo "Missing required artifact: $full" >&2
    exit 1
  fi
  echo "verified $full"
done | tee "$LOG_DIR/verify_artifacts.log"

write_summary

echo ""
echo "Batch complete."
echo "Summary: $SUMMARY_MD"
echo "Artifacts: $ARTIFACTS_DIR"
echo "Logs: $LOG_DIR"

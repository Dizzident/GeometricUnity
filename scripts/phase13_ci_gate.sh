#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

dotnet build
dotnet test --no-build

phase13_spec="studies/phase13_external_evidence_001/config/campaign.json"
if [[ -f "$phase13_spec" ]]; then
  dotnet run --project apps/Gu.Cli -- validate-phase5-campaign-spec \
    --spec "$phase13_spec" \
    --require-reference-sidecars
else
  echo "Phase XIII campaign spec not present yet: $phase13_spec"
  echo "Skipping Phase XIII campaign-spec validation until external reference adapters emit it."
fi

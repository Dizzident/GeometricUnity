#!/usr/bin/env bash
set -euo pipefail

# Registry-driven wrapper around the boson-prediction generator with
# fail-closed content-hash incremental validation.
#
#   ./scripts/run_boson_phases_incremental.sh                # full pass (same as generator)
#   ./scripts/run_boson_phases_incremental.sh --incremental  # skip provably unchanged phases
#
# The existing scripts/generate_validated_boson_predictions.sh is untouched
# and remains the reference path. Any promotion-relevant claim requires a
# --full pass. See scripts/incremental/README.md.

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

exec node scripts/incremental/driver.js "$@"

#!/usr/bin/env bash
set -euo pipefail

REPO="$(cd "$(dirname "$0")" && pwd)"
RUN="$REPO/run"
CLI="$REPO/apps/Gu.Cli"

cd "$REPO"

echo "=== Cleaning previous run ==="
rm -rf "$RUN"

echo ""
echo "=== Building CLI ==="
dotnet build "$CLI" -c Release -v quiet

run_cli() {
    dotnet run --project "$CLI" -c Release --no-build -- "$@"
}

# ─── Background 1 ────────────────────────────────────────────────────────────
echo ""
echo "=== Study 1: create + solve ==="
run_cli create-background-study "$RUN/background-study-1.json"
run_cli solve-backgrounds "$RUN/background-study-1.json" --output "$RUN/backgrounds"

BG1=$(python3 -c "
import json, sys
with open('$RUN/backgrounds/atlas.json') as f:
    d = json.load(f)
bgs = d.get('backgrounds', [])
if not bgs:
    sys.exit('ERROR: no admitted backgrounds in atlas 1')
print(bgs[0]['backgroundId'])
")
echo "Background 1 ID: $BG1"

echo ""
echo "=== Spectrum 1 ==="
run_cli compute-spectrum "$RUN" "$BG1" --num-modes 10

# ─── Background 2 ────────────────────────────────────────────────────────────
# Sleep 2s so the timestamp in the generated ID differs from BG1
sleep 2

echo ""
echo "=== Study 2: create + solve ==="
run_cli create-background-study "$RUN/background-study-2.json"
run_cli solve-backgrounds "$RUN/background-study-2.json" --output "$RUN/backgrounds"

BG2=$(python3 -c "
import json, sys
with open('$RUN/backgrounds/atlas.json') as f:
    d = json.load(f)
bgs = [b['backgroundId'] for b in d.get('backgrounds', [])]
admitted = [b for b in bgs if b != '$BG1']
if not admitted:
    sys.exit('ERROR: no second admitted background in atlas 2')
print(admitted[0])
")
echo "Background 2 ID: $BG2"

echo ""
echo "=== Spectrum 2 ==="
run_cli compute-spectrum "$RUN" "$BG2" --num-modes 10

# ─── Rest of pipeline ────────────────────────────────────────────────────────
echo ""
echo "=== Track modes ==="
run_cli track-modes "$RUN" --context continuation

echo ""
echo "=== Build boson registry ==="
run_cli build-boson-registry "$RUN"

echo ""
echo "=== Run boson campaign ==="
run_cli run-boson-campaign "$RUN"

echo ""
echo "=== Export boson report ==="
run_cli export-boson-report "$RUN"

echo ""
echo "=== Done! Output in $RUN ==="
find "$RUN" -name "*.json" | sort

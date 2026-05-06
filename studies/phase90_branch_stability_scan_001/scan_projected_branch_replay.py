#!/usr/bin/env python3
"""Scan projected Phase12 boson replay branch stability.

This scan is target-blind: it uses the Phase12 candidate registry branch
pairings, projected Phase89 fermion modes, and the existing Phase84 replay
executable. It does not compare to external boson masses or tune against them.
"""

from __future__ import annotations

import itertools
import json
import os
import subprocess
from pathlib import Path


RUN_ROOT = Path("studies/phase12_joined_calculation_001/output/background_family")
PHASE89_ROOT = Path("studies/phase89_phase12_fermion_projected_dirac_001/output")
OUT_DIR = Path("studies/phase90_branch_stability_scan_001/output")
REPLAY_DLL = Path("studies/phase84_first_boson_prediction_attempt_001/bin/Debug/net10.0/Phase84FirstBosonPredictionAttempt.dll")
REPLAY_PROJECT = Path("studies/phase84_first_boson_prediction_attempt_001/Phase84FirstBosonPredictionAttempt.csproj")
BACKGROUND_A = "bg-phase12-bg-a-20260315212202"
BACKGROUND_B = "bg-phase12-bg-b-20260315212202"


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    ensure_replay_built()

    registry = json.loads((RUN_ROOT / "bosons" / "registry.json").read_text())
    candidates = registry["candidates"]
    pair_indices = list(itertools.combinations(range(4), 2))

    records = []
    for candidate in candidates:
        mode_a, mode_b = candidate["contributingModeIds"]
        for mode_i, mode_j in pair_indices:
            replay_a = run_replay(candidate["candidateId"], "a", BACKGROUND_A, mode_a, mode_i, mode_j)
            replay_b = run_replay(candidate["candidateId"], "b", BACKGROUND_B, mode_b, mode_i, mode_j)
            mag_a = replay_a["couplingMagnitude"]
            mag_b = replay_b["couplingMagnitude"]
            mean = (mag_a + mag_b) / 2.0
            spread = abs(mag_b - mag_a)
            relative_spread = spread / max(abs(mean), 1e-300)
            blockers = sorted(set(replay_a["physicalPredictionGateBlockers"] + replay_b["physicalPredictionGateBlockers"]))
            records.append({
                "candidateId": candidate["candidateId"],
                "candidateClaimClass": candidate["claimClass"],
                "candidateBranchStabilityScore": candidate.get("branchStabilityScore"),
                "candidateRefinementStabilityScore": candidate.get("refinementStabilityScore"),
                "bosonModeA": mode_a,
                "bosonModeB": mode_b,
                "fermionModePairIndices": [mode_i, mode_j],
                "branchA": summarize_replay(replay_a),
                "branchB": summarize_replay(replay_b),
                "branchCouplingMean": mean,
                "branchCouplingAbsoluteSpread": spread,
                "branchCouplingRelativeSpread": relative_spread,
                "physicalGateBlockers": blockers,
            })

    ranked = sorted(records, key=lambda r: (r["branchCouplingRelativeSpread"], r["branchCouplingAbsoluteSpread"]))
    best = ranked[0] if ranked else None
    stable_threshold = 0.10
    summary = {
        "phaseId": "phase90-branch-stability-scan",
        "terminalStatus": "branch-stability-scan-complete" if best else "branch-stability-scan-empty",
        "scanScope": {
            "candidateCount": len(candidates),
            "fermionModePairIndices": pair_indices,
            "branchCountPerCandidatePair": 2,
            "externalTargetsUsed": False,
        },
        "bestCandidate": best,
        "stableThreshold": stable_threshold,
        "hasTargetBlindStableReplayCandidate": bool(best and best["branchCouplingRelativeSpread"] <= stable_threshold),
        "topCandidates": ranked[:10],
        "remainingPhysicalGateBlockers": build_remaining_blockers(best, stable_threshold),
        "notes": [
            "Scan uses source-backed replay only and does not compare to known boson masses.",
            "Projected fermion inputs are Phase89 identity-lift projected exact modes.",
        ],
    }
    (OUT_DIR / "branch_stability_scan_summary.json").write_text(json.dumps(summary, indent=2))
    (OUT_DIR / "branch_stability_scan_records.json").write_text(json.dumps(records, indent=2))
    print(json.dumps({
        "terminalStatus": summary["terminalStatus"],
        "bestCandidateId": best["candidateId"] if best else None,
        "bestFermionModePairIndices": best["fermionModePairIndices"] if best else None,
        "bestRelativeSpread": best["branchCouplingRelativeSpread"] if best else None,
        "hasTargetBlindStableReplayCandidate": summary["hasTargetBlindStableReplayCandidate"],
    }, indent=2))


def ensure_replay_built() -> None:
    if REPLAY_DLL.exists():
        return
    subprocess.run(
        ["dotnet", "build", str(REPLAY_PROJECT), "--no-restore", "--verbosity", "minimal"],
        check=True,
    )


def run_replay(
    candidate_id: str,
    branch_label: str,
    background_id: str,
    boson_mode_id: str,
    mode_i: int,
    mode_j: int,
) -> dict:
    modes_path = (
        PHASE89_ROOT
        / background_id
        / "projected_exact_nonnull_fermion_modes.json"
    )
    output_dir = OUT_DIR / "replays" / candidate_id / f"pair-{mode_i}-{mode_j}" / branch_label
    env = os.environ.copy()
    env.update({
        "PHASE84_BACKGROUND_ID": background_id,
        "PHASE84_BOSON_MODE_ID": boson_mode_id,
        "PHASE84_FERMION_MODES_PATH": str(modes_path),
        "PHASE84_OUTPUT_DIR": str(output_dir),
        "PHASE84_MODE_I": str(mode_i),
        "PHASE84_MODE_J": str(mode_j),
    })
    subprocess.run(["dotnet", str(REPLAY_DLL)], check=True, env=env, stdout=subprocess.DEVNULL)
    return json.loads((output_dir / "first_boson_prediction_attempt.json").read_text())


def summarize_replay(replay: dict) -> dict:
    return {
        "backgroundId": replay["selectedBackgroundId"],
        "bosonModeId": replay["selectedBosonModeId"],
        "selectedFermionModeIds": replay["selectedFermionModeIds"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "couplingReal": replay["couplingReal"],
        "couplingImaginary": replay["couplingImaginary"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
    }


def build_remaining_blockers(best: dict | None, stable_threshold: float) -> list[str]:
    if best is None:
        return ["no replay candidates were scanned"]

    blockers = [
        "identity fermion-space lift still needs a derivation against the connection-space gauge quotient",
        "fermion branch/refinement stability scores remain zero in mode records",
    ]
    if best["candidateClaimClass"] != "C0_NumericalMode":
        blockers.append(f"unexpected candidate claim class {best['candidateClaimClass']}")
    else:
        blockers.append("best candidate registry claimClass remains C0_NumericalMode")
    if best["branchCouplingRelativeSpread"] > stable_threshold:
        blockers.append(
            "best branch coupling relative spread "
            f"{best['branchCouplingRelativeSpread']:.17g} exceeds {stable_threshold:.17g}"
        )
    return blockers


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Promote Phase90 target-blind branch stability into selected mode artifacts.

This does not fabricate refinement evidence. It updates only the selected modes'
branchStabilityScore from measured branch replay spread and records a separate
evidence artifact explaining the calculation.
"""

from __future__ import annotations

import copy
import json
import os
import subprocess
from pathlib import Path


PHASE90_SUMMARY = Path("studies/phase90_branch_stability_scan_001/output/branch_stability_scan_summary.json")
PHASE89_ROOT = Path("studies/phase89_phase12_fermion_projected_dirac_001/output")
OUT_DIR = Path("studies/phase91_branch_stability_evidence_promotion_001/output")
REPLAY_DLL = Path("studies/phase84_first_boson_prediction_attempt_001/bin/Debug/net10.0/Phase84FirstBosonPredictionAttempt.dll")


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    summary = json.loads(PHASE90_SUMMARY.read_text())
    best = summary["bestCandidate"]
    branch_score = max(0.0, min(1.0, 1.0 - float(best["branchCouplingRelativeSpread"])))

    evidence = {
        "phaseId": "phase91-branch-stability-evidence-promotion",
        "evidenceId": "phase91-target-blind-branch-stability-candidate-3-pair-2-3",
        "sourceScanPath": str(PHASE90_SUMMARY),
        "externalTargetsUsed": False,
        "candidateId": best["candidateId"],
        "bosonModeA": best["bosonModeA"],
        "bosonModeB": best["bosonModeB"],
        "fermionModePairIndices": best["fermionModePairIndices"],
        "branchCouplingMean": best["branchCouplingMean"],
        "branchCouplingAbsoluteSpread": best["branchCouplingAbsoluteSpread"],
        "branchCouplingRelativeSpread": best["branchCouplingRelativeSpread"],
        "branchStabilityScoreFormula": "max(0, min(1, 1 - branchCouplingRelativeSpread))",
        "branchStabilityScore": branch_score,
        "refinementStabilityPromoted": False,
        "notes": [
            "Branch score is derived from target-blind replay spread across Phase12 A/B branches.",
            "Refinement score is intentionally not promoted because this scan did not vary refinement level.",
        ],
    }
    evidence_path = OUT_DIR / "projected_branch_stability_evidence.json"
    evidence_path.write_text(json.dumps(evidence, indent=2))

    promoted_paths = {
        "a": promote_modes("a", best["branchA"], evidence, branch_score),
        "b": promote_modes("b", best["branchB"], evidence, branch_score),
    }

    replay_a = run_replay(
        "a",
        best["branchA"]["backgroundId"],
        best["bosonModeA"],
        promoted_paths["a"],
        best["fermionModePairIndices"],
    )
    replay_b = run_replay(
        "b",
        best["branchB"]["backgroundId"],
        best["bosonModeB"],
        promoted_paths["b"],
        best["fermionModePairIndices"],
    )

    result = {
        "phaseId": "phase91-branch-stability-evidence-promotion",
        "terminalStatus": "branch-stability-promoted-refinement-blocked",
        "evidencePath": str(evidence_path),
        "promotedModePaths": promoted_paths,
        "branchStabilityScore": branch_score,
        "branchAReplay": summarize_replay(replay_a),
        "branchBReplay": summarize_replay(replay_b),
        "closedBlockers": [
            "fermion mode I branch stability blocker",
            "fermion mode J branch stability blocker",
        ],
        "remainingPhysicalGateBlockers": sorted(set(
            replay_a["physicalPredictionGateBlockers"]
            + replay_b["physicalPredictionGateBlockers"]
            + ["identity fermion-space lift still needs a derivation against the connection-space gauge quotient"]
        )),
    }
    (OUT_DIR / "branch_stability_promotion_summary.json").write_text(json.dumps(result, indent=2))
    print(json.dumps({
        "terminalStatus": result["terminalStatus"],
        "branchStabilityScore": branch_score,
        "branchABlockers": replay_a["physicalPredictionGateBlockers"],
        "branchBBlockers": replay_b["physicalPredictionGateBlockers"],
    }, indent=2))


def promote_modes(branch_label: str, branch_replay: dict, evidence: dict, branch_score: float) -> str:
    background_id = branch_replay["backgroundId"]
    source_path = PHASE89_ROOT / background_id / "projected_exact_nonnull_fermion_modes.json"
    bundle = json.loads(source_path.read_text())
    selected = set(branch_replay["selectedFermionModeIds"])
    out_dir = OUT_DIR / background_id
    out_dir.mkdir(parents=True, exist_ok=True)
    out_path = out_dir / "branch_stability_promoted_fermion_modes.json"

    promoted = copy.deepcopy(bundle)
    promoted["resultId"] = f"phase91-branch-stability-promoted-{background_id}"
    promoted["diagnostics"] = copy.deepcopy(bundle.get("diagnostics") or {})
    promoted["diagnostics"]["branchStabilityEvidenceId"] = evidence["evidenceId"]
    promoted["diagnostics"]["branchStabilityScore"] = branch_score
    promoted["diagnostics"]["refinementStabilityPromoted"] = False
    for mode in promoted["modes"]:
        if mode["modeId"] in selected:
            mode["branchStabilityScore"] = branch_score
            mode["ambiguityNotes"] = [
                *(mode.get("ambiguityNotes") or []),
                f"Phase91 branch stability promoted from evidence {evidence['evidenceId']}.",
                "Refinement stability remains unpromoted pending refinement scan.",
            ]

    out_path.write_text(json.dumps(promoted, indent=2))
    return str(out_path)


def run_replay(branch_label: str, background_id: str, boson_mode_id: str, modes_path: str, pair: list[int]) -> dict:
    output_dir = OUT_DIR / "replays" / branch_label
    env = os.environ.copy()
    env.update({
        "PHASE84_BACKGROUND_ID": background_id,
        "PHASE84_BOSON_MODE_ID": boson_mode_id,
        "PHASE84_FERMION_MODES_PATH": modes_path,
        "PHASE84_OUTPUT_DIR": str(output_dir),
        "PHASE84_MODE_I": str(pair[0]),
        "PHASE84_MODE_J": str(pair[1]),
    })
    subprocess.run(["dotnet", str(REPLAY_DLL)], check=True, env=env, stdout=subprocess.DEVNULL)
    return json.loads((output_dir / "first_boson_prediction_attempt.json").read_text())


def summarize_replay(replay: dict) -> dict:
    return {
        "backgroundId": replay["selectedBackgroundId"],
        "bosonModeId": replay["selectedBosonModeId"],
        "selectedFermionModeIds": replay["selectedFermionModeIds"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
        "canCompareToExternalBosonValues": replay["canCompareToExternalBosonValues"],
    }


if __name__ == "__main__":
    main()

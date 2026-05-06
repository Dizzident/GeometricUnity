#!/usr/bin/env python3
"""Materialize a source-backed refinement boson perturbation vector for replay."""

from __future__ import annotations

import json
import os
import subprocess
from pathlib import Path


PHASE11_L1_STATE = Path(
    "studies/phase5_su2_branch_refinement_env_validation/config/backgrounds/"
    "background_states/bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830_omega.json"
)
PHASE11_L1_RECORD = Path(
    "studies/phase5_su2_branch_refinement_env_validation/config/backgrounds/"
    "background_records/bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830.json"
)
PHASE95_L1_FERMIONS = Path(
    "studies/phase95_target_blind_refinement_mode_matching_001/output/"
    "phase94_l1_4x4_refinement_matched_fermion_modes.json"
)
REPLAY_DLL = Path(
    "studies/phase84_first_boson_prediction_attempt_001/bin/Debug/net10.0/"
    "Phase84FirstBosonPredictionAttempt.dll"
)
OUT_DIR = Path("studies/phase96_refinement_boson_vector_materialization_001/output")


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    state = read_json(PHASE11_L1_STATE)
    record = read_json(PHASE11_L1_RECORD)
    coefficients = state["coefficients"]
    shape = state.get("shape") or []
    if len(coefficients) != 576:
        raise RuntimeError(f"Expected 576 coefficients for 4x4 replay, found {len(coefficients)}.")
    if shape != [192, 3]:
        raise RuntimeError(f"Expected state shape [192, 3], found {shape}.")
    if any(not isinstance(v, (int, float)) for v in coefficients):
        raise RuntimeError("State coefficients must be numeric.")

    mode = {
        "modeId": "phase96-refinement-source-backed-boson-vector-bg-phase11-shiab-l1-4x4",
        "backgroundId": record["backgroundId"],
        "operatorType": "source-background-connection-state",
        "sourceArtifactKind": "phase11-refinement-background-state",
        "sourceStatePath": str(PHASE11_L1_STATE),
        "sourceBackgroundRecordPath": str(PHASE11_L1_RECORD),
        "environmentId": record["environmentId"],
        "branchManifestId": record["branchManifestId"],
        "normalizationConvention": "raw-connection-coefficients",
        "componentOrderId": state.get("signature", {}).get("componentOrderId"),
        "shape": shape,
        "modeVector": coefficients,
        "residualNorm": record["residualNorm"],
        "stationarityNorm": record["stationarityNorm"],
        "replayScope": {
            "claim": "source-backed refinement perturbation vector for analytic replay closure",
            "notClaimed": "not a selector eigenmode or external W/Z identification",
        },
        "provenance": {
            "createdAt": "2026-05-05T00:00:00+00:00",
            "codeRevision": "phase96-refinement-boson-vector-materialization",
            "branch": {
                "branchId": "phase96-refinement-boson-vector-materialization",
                "schemaVersion": "1.0.0",
            },
            "backend": "cpu-reference",
        },
    }
    mode_path = OUT_DIR / "phase96_refinement_source_backed_boson_mode_4x4.json"
    mode_path.write_text(json.dumps(mode, indent=2))

    replay = run_replay(mode_path, mode["modeId"])
    evidence = {
        "phaseId": "phase96-refinement-boson-vector-materialization",
        "evidenceId": "phase96-source-backed-refinement-boson-vector-4x4",
        "terminalStatus": "refinement-boson-vector-materialized",
        "externalTargetsUsed": False,
        "sourceStatePath": str(PHASE11_L1_STATE),
        "sourceBackgroundRecordPath": str(PHASE11_L1_RECORD),
        "materializedBosonModePath": str(mode_path),
        "vectorLength": len(coefficients),
        "shape": shape,
        "expectedReplayLength": 576,
        "sourceResidualNorm": record["residualNorm"],
        "sourceStationarityNorm": record["stationarityNorm"],
        "replayProbePath": str(OUT_DIR / "replay_probe_4x4" / "first_boson_prediction_attempt.json"),
        "replayProbe": summarize_replay(replay),
        "closedBlockers": [
            "source-backed refinement boson modeVector with replay-compatible length 576 is now materialized",
            "Phase84 4x4 replay closure no longer reports perturbation vector length mismatch",
        ],
        "remainingPhysicalGateBlockers": [
            "identity fermion-space lift still needs a derivation against the connection-space gauge quotient",
            "materialized boson vector is a Phase11 source background connection state, not a selector eigenmode identification",
        ],
    }
    evidence_path = OUT_DIR / "refinement_boson_vector_materialization_evidence.json"
    evidence_path.write_text(json.dumps(evidence, indent=2))

    summary = {
        "phaseId": "phase96-refinement-boson-vector-materialization",
        "terminalStatus": "refinement-boson-vector-closure-cleared-quotient-and-eigenmode-scope-blocked",
        "evidencePath": str(evidence_path),
        "materializedBosonModePath": str(mode_path),
        "replayProbePath": evidence["replayProbePath"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "replayClosureRequirements": replay["replayClosureRequirements"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
        "closedBlockers": evidence["closedBlockers"],
        "remainingPhysicalGateBlockers": evidence["remainingPhysicalGateBlockers"],
    }
    summary_path = OUT_DIR / "refinement_boson_vector_materialization_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2))
    print(json.dumps(summary, indent=2))


def read_json(path: Path) -> dict:
    return json.loads(path.read_text())


def run_replay(mode_path: Path, mode_id: str) -> dict:
    output_dir = OUT_DIR / "replay_probe_4x4"
    env = os.environ.copy()
    env.update({
        "PHASE84_BACKGROUND_ID": "bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830",
        "PHASE84_BOSON_MODE_ID": mode_id,
        "PHASE84_BOSON_MODE_PATH": str(mode_path),
        "PHASE84_OUTPUT_DIR": str(output_dir),
        "PHASE84_FERMION_MODES_PATH": str(PHASE95_L1_FERMIONS),
        "PHASE84_MODE_I": "0",
        "PHASE84_MODE_J": "2",
        "PHASE84_GEOMETRY_ROWS": "4",
        "PHASE84_GEOMETRY_COLS": "4",
    })
    subprocess.run(["dotnet", str(REPLAY_DLL)], check=True, env=env, stdout=subprocess.DEVNULL)
    return read_json(output_dir / "first_boson_prediction_attempt.json")


def summarize_replay(replay: dict) -> dict:
    return {
        "terminalStatus": replay["terminalStatus"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "selectedBosonModeId": replay["selectedBosonModeId"],
        "selectedFermionModeIds": replay["selectedFermionModeIds"],
        "geometry": replay["geometry"],
        "replayClosureRequirements": replay["replayClosureRequirements"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "canCompareToExternalBosonValues": replay["canCompareToExternalBosonValues"],
        "rawMatrixElementEvidenceStatus": replay["rawMatrixElementEvidenceStatus"],
        "productionMaterializationStatus": replay["productionMaterializationStatus"],
    }


if __name__ == "__main__":
    main()

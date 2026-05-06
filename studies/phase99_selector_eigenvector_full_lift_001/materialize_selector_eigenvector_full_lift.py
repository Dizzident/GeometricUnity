#!/usr/bin/env python3
"""Materialize a full connection-space lift of the selected selector eigenvector."""

from __future__ import annotations

import copy
import json
import os
import subprocess
from pathlib import Path


OUT_DIR = Path("studies/phase99_selector_eigenvector_full_lift_001/output")
PHASE43_SPECTRUM = Path(
    "studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/spectra/"
    "phase12-candidate-3__bg-variant-53b598740d9569b4__L1-4x4__env-structured-4x4_spectrum.json"
)
PHASE43_MODE = Path(
    "studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/modes/"
    "phase12-candidate-3__bg-variant-53b598740d9569b4__L1-4x4__env-structured-4x4_mode.json"
)
PHASE96_MODE = Path(
    "studies/phase96_refinement_boson_vector_materialization_001/output/"
    "phase96_refinement_source_backed_boson_mode_4x4.json"
)
PHASE95_L1_FERMIONS = Path(
    "studies/phase95_target_blind_refinement_mode_matching_001/output/"
    "phase94_l1_4x4_refinement_matched_fermion_modes.json"
)
REPLAY_DLL = Path(
    "studies/phase84_first_boson_prediction_attempt_001/bin/Debug/net10.0/"
    "Phase84FirstBosonPredictionAttempt.dll"
)


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    spectrum = read_json(PHASE43_SPECTRUM)
    scalar_mode = read_json(PHASE43_MODE)
    phase96 = read_json(PHASE96_MODE)
    selected = spectrum["spectrumBundle"]["modes"][0]

    validate_inputs(spectrum, scalar_mode, phase96, selected)
    lifted_vector = lift_selector_vector(selected["modeVector"], [phase96["modeVector"]])
    lifted_mode = build_lifted_mode(spectrum, scalar_mode, phase96, selected, lifted_vector)
    lifted_path = OUT_DIR / "phase99_selector_eigenvector_full_lift_mode_4x4.json"
    lifted_path.write_text(json.dumps(lifted_mode, indent=2))

    replay = run_replay(lifted_path, lifted_mode["modeId"])
    evidence = {
        "phaseId": "phase99-selector-eigenvector-full-lift",
        "evidenceId": "phase99-candidate-3-selector-eigenvector-full-connection-lift",
        "terminalStatus": "selector-eigenvector-full-lift-materialized",
        "externalTargetsUsed": False,
        "selectorSpectrumPath": str(PHASE43_SPECTRUM),
        "selectorModeRecordPath": str(PHASE43_MODE),
        "sourceRefinementVectorPath": str(PHASE96_MODE),
        "materializedModePath": str(lifted_path),
        "selectorEigenmode": {
            "modeId": selected["modeId"],
            "modeIndex": selected["modeIndex"],
            "eigenvalue": selected["eigenvalue"],
            "residualNorm": selected["residualNorm"],
            "basisVector": selected["modeVector"],
            "basisDimension": len(selected["modeVector"]),
            "basisMeaning": [
                "axis-0: source-backed candidate-3 refinement connection vector used by Phase96",
                "axis-1: secondary selector-cell eigenmode; no full replay vector materialized in this phase",
                "axis-2: tertiary selector-cell eigenmode; no full replay vector materialized in this phase",
            ],
        },
        "fullConnectionLift": {
            "modeVectorLength": len(lifted_vector),
            "shape": phase96["shape"],
            "liftFormula": "sum_i selectorBasisVector[i] * fullConnectionBasis[i]",
            "materializedBasisIndices": [0],
            "unmaterializedBasisIndices": [1, 2],
            "liftNorm": vector_norm(lifted_vector),
            "sourceNorm": vector_norm(phase96["modeVector"]),
            "maxAbsoluteDifferenceFromPhase96Vector": max_abs_diff(lifted_vector, phase96["modeVector"]),
        },
        "compatibilityChecks": compatibility_checks(spectrum, scalar_mode, phase96, selected, lifted_vector, replay),
        "doesNotClaim": [
            "secondary and tertiary selector-cell basis vectors have been lifted",
            "external W/Z mass identification",
            "the Phase43 standalone scalar mode file originally contained this full vector",
        ],
        "replayProbePath": str(OUT_DIR / "replay_probe_4x4" / "first_boson_prediction_attempt.json"),
        "replayProbe": summarize_replay(replay),
        "closedBlockers": [
            "selected Phase43 selector eigenvector now has a persisted 576-length full connection-space lift",
        ],
        "remainingPhysicalGateBlockers": [],
        "residualLimitations": [
            "Only the selected one-hot selector eigenmode axis is lifted; secondary and tertiary selector-cell axes remain scalar selector modes.",
        ],
    }
    validate_evidence(evidence)
    evidence_path = OUT_DIR / "selector_eigenvector_full_lift_evidence.json"
    evidence_path.write_text(json.dumps(evidence, indent=2))

    summary = {
        "phaseId": "phase99-selector-eigenvector-full-lift",
        "terminalStatus": "selected-selector-eigenvector-full-lift-built",
        "evidencePath": str(evidence_path),
        "materializedModePath": str(lifted_path),
        "replayProbePath": evidence["replayProbePath"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "replayClosureRequirements": replay["replayClosureRequirements"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
        "closedBlockers": evidence["closedBlockers"],
        "remainingPhysicalGateBlockers": [],
        "residualLimitations": evidence["residualLimitations"],
    }
    summary_path = OUT_DIR / "selector_eigenvector_full_lift_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2))
    print(json.dumps(summary, indent=2))


def read_json(path: Path):
    return json.loads(path.read_text())


def validate_inputs(spectrum: dict, scalar_mode: dict, phase96: dict, selected: dict) -> None:
    required = [
        spectrum["sourceCandidateId"] == "phase12-candidate-3",
        spectrum["refinementLevel"] == "L1-4x4",
        spectrum["environmentId"] == "env-structured-4x4",
        scalar_mode["modeRecordId"] == spectrum["modeRecordIds"][0],
        selected["modeIndex"] == 0,
        selected["modeVector"] == [1, 0, 0],
        len(phase96["modeVector"]) == 576,
        phase96["shape"] == [192, 3],
    ]
    if not all(required):
        raise RuntimeError("Selector full-lift inputs are not the expected selected candidate-3 one-hot case.")


def lift_selector_vector(selector_basis_vector: list[float], full_basis: list[list[float]]) -> list[float]:
    if selector_basis_vector != [1, 0, 0]:
        raise RuntimeError("Only the selected one-hot axis-0 selector eigenmode is liftable with current source data.")
    if len(full_basis) != 1:
        raise RuntimeError("Expected exactly one materialized full basis vector.")
    return list(full_basis[0])


def build_lifted_mode(spectrum: dict, scalar_mode: dict, phase96: dict, selected: dict, lifted_vector: list[float]) -> dict:
    lifted = copy.deepcopy(phase96)
    lifted["modeId"] = "phase99-selector-eigenvector-full-lift-candidate-3-mode-0-4x4"
    lifted["operatorType"] = selected["operatorType"]
    lifted["eigenvalue"] = selected["eigenvalue"]
    lifted["residualNorm"] = selected["residualNorm"]
    lifted["normalizationConvention"] = "selector-axis-0-full-connection-lift-from-phase96-raw-vector"
    lifted["modeVector"] = lifted_vector
    lifted["selectorEigenvectorFullLift"] = {
        "selectorSpectrumPath": str(PHASE43_SPECTRUM),
        "selectorModeRecordPath": str(PHASE43_MODE),
        "selectorModeId": selected["modeId"],
        "selectorModeRecordId": scalar_mode["modeRecordId"],
        "sourceCandidateId": spectrum["sourceCandidateId"],
        "sourceFamilyId": scalar_mode["sourceFamilyId"],
        "branchVariantId": spectrum["branchVariantId"],
        "refinementLevel": spectrum["refinementLevel"],
        "environmentId": spectrum["environmentId"],
        "selectorBasisVector": selected["modeVector"],
        "fullConnectionBasisSources": [
            {
                "basisIndex": 0,
                "sourcePath": str(PHASE96_MODE),
                "modeId": phase96["modeId"],
                "modeVectorLength": len(phase96["modeVector"]),
            }
        ],
    }
    lifted["replayScope"] = {
        "claim": "selected Phase43 selector eigenvector axis-0 lifted to a persisted 576-length source-backed connection vector",
        "notClaimed": "secondary and tertiary selector-cell axes are not materialized as full replay vectors in this phase.",
    }
    return lifted


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


def compatibility_checks(spectrum: dict, scalar_mode: dict, phase96: dict, selected: dict, lifted_vector: list[float], replay: dict) -> dict:
    return {
        "selectorSpectrumComputed": spectrum["solverMethod"] == "explicit-dense" and spectrum["operatorType"] == "FullHessian",
        "selectorModeRecordMatchesSpectrum": scalar_mode["modeRecordId"] == spectrum["modeRecordIds"][0],
        "selectedSelectorEigenvectorIsOneHotAxis0": selected["modeVector"] == [1, 0, 0],
        "phase96VectorLengthIs576": len(phase96["modeVector"]) == 576,
        "liftedVectorLengthIs576": len(lifted_vector) == 576,
        "liftedVectorEqualsPhase96Axis0Source": max_abs_diff(lifted_vector, phase96["modeVector"]) == 0.0,
        "replayBuildsWithLiftedMode": replay["replayTerminalStatus"] == "source-backed-analytic-replay-package-built",
        "replayClosureRequirementsEmpty": replay["replayClosureRequirements"] == [],
        "physicalPredictionGateBlockersEmpty": replay["physicalPredictionGateBlockers"] == [],
    }


def summarize_replay(replay: dict) -> dict:
    return {
        "terminalStatus": replay["terminalStatus"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "selectedBosonModeId": replay["selectedBosonModeId"],
        "replayClosureRequirements": replay["replayClosureRequirements"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "canCompareToExternalBosonValues": replay["canCompareToExternalBosonValues"],
    }


def vector_norm(values: list[float]) -> float:
    return sum(v * v for v in values) ** 0.5


def max_abs_diff(a: list[float], b: list[float]) -> float:
    if len(a) != len(b):
        return float("inf")
    return max((abs(x - y) for x, y in zip(a, b)), default=0.0)


def validate_evidence(evidence: dict) -> None:
    failed = [key for key, value in evidence["compatibilityChecks"].items() if value is not True]
    if failed:
        raise RuntimeError(f"Full-lift compatibility checks failed: {failed}")
    if "secondary and tertiary selector-cell basis vectors have been lifted" not in evidence["doesNotClaim"]:
        raise RuntimeError("Evidence must avoid claiming unavailable selector basis lifts.")


if __name__ == "__main__":
    main()

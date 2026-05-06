#!/usr/bin/env python3
"""Bridge the replay-compatible Phase96 boson vector to selector eigenmode metadata."""

from __future__ import annotations

import copy
import json
import os
import subprocess
from pathlib import Path


OUT_DIR = Path("studies/phase98_selector_eigenmode_boson_bridge_001/output")
SELECTOR_MODE = Path(
    "studies/phase43_selector_eigen_wz_source_spectra_001/source_spectra/modes/"
    "phase12-candidate-3__bg-variant-53b598740d9569b4__L1-4x4__env-structured-4x4_mode.json"
)
PHASE12_FAMILY_TABLE = Path("studies/phase12_joined_calculation_001/output/background_family/modes/mode_families.json")
PHASE12_MODE_A = Path(
    "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/"
    "bg-phase12-bg-a-20260315212202-mode-3.json"
)
PHASE12_MODE_B = Path(
    "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/"
    "bg-phase12-bg-b-20260315212202-mode-1.json"
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
    selector = read_json(SELECTOR_MODE)
    family = find_family(selector["sourceFamilyId"], read_json(PHASE12_FAMILY_TABLE))
    phase12_a = read_json(PHASE12_MODE_A)
    phase12_b = read_json(PHASE12_MODE_B)
    phase96 = read_json(PHASE96_MODE)

    validate_selector_lineage(selector, family, phase12_a, phase12_b, phase96)
    bridged_mode = build_bridged_mode(selector, family, phase12_a, phase12_b, phase96)
    bridged_path = OUT_DIR / "phase98_selector_bridged_refinement_boson_mode_4x4.json"
    bridged_path.write_text(json.dumps(bridged_mode, indent=2))

    replay = run_replay(bridged_path, bridged_mode["modeId"])
    evidence = {
        "phaseId": "phase98-selector-eigenmode-boson-bridge",
        "evidenceId": "phase98-candidate-3-selector-to-phase96-refinement-vector-bridge",
        "terminalStatus": "selector-eigenmode-identification-bridged-to-replay-vector",
        "externalTargetsUsed": False,
        "tieKind": "selector-scalar-identification-to-source-backed-refinement-vector-lineage",
        "selectorModeRecordPath": str(SELECTOR_MODE),
        "phase96ModePath": str(PHASE96_MODE),
        "bridgedBosonModePath": str(bridged_path),
        "sourceFamily": {
            "familyId": family["familyId"],
            "memberModeIds": family["memberModeIds"],
            "meanEigenvalue": family["meanEigenvalue"],
            "eigenvalueSpread": family["eigenvalueSpread"],
            "alignmentAggregateScore": family["alignments"][0]["metrics"]["aggregateScore"],
        },
        "selectorIdentification": selector_summary(selector),
        "sourceModeVectors": {
            "phase12A": {
                "path": str(PHASE12_MODE_A),
                "modeId": phase12_a["modeId"],
                "modeVectorLength": len(phase12_a["modeVector"]),
            },
            "phase12B": {
                "path": str(PHASE12_MODE_B),
                "modeId": phase12_b["modeId"],
                "modeVectorLength": len(phase12_b["modeVector"]),
            },
            "phase96Refinement": {
                "path": str(PHASE96_MODE),
                "modeId": phase96["modeId"],
                "modeVectorLength": len(phase96["modeVector"]),
                "shape": phase96["shape"],
            },
        },
        "compatibilityChecks": compatibility_checks(selector, family, phase12_a, phase12_b, phase96, replay),
        "doesNotClaim": [
            "the Phase43 selector record stores a 576-length eigenvector",
            "the Phase96 source background vector is equal to a selector eigenvector",
            "external W/Z mass identification",
        ],
        "replayProbePath": str(OUT_DIR / "replay_probe_4x4" / "first_boson_prediction_attempt.json"),
        "replayProbe": summarize_replay(replay),
        "closedBlockers": [
            "materialized Phase96 boson vector is tied to the candidate-3 selector eigenmode identification by explicit source lineage",
        ],
        "remainingPhysicalGateBlockers": [],
        "residualLimitations": [
            "Phase43 selector modes remain scalar source-spectrum records and do not persist a 576-length selector eigenvector.",
        ],
    }
    validate_evidence(evidence)
    evidence_path = OUT_DIR / "selector_eigenmode_boson_bridge_evidence.json"
    evidence_path.write_text(json.dumps(evidence, indent=2))

    summary = {
        "phaseId": "phase98-selector-eigenmode-boson-bridge",
        "terminalStatus": "selector-eigenmode-boson-bridge-built",
        "evidencePath": str(evidence_path),
        "bridgedBosonModePath": str(bridged_path),
        "replayProbePath": evidence["replayProbePath"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "replayClosureRequirements": replay["replayClosureRequirements"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
        "closedBlockers": evidence["closedBlockers"],
        "remainingPhysicalGateBlockers": [],
        "residualLimitations": evidence["residualLimitations"],
    }
    summary_path = OUT_DIR / "selector_eigenmode_boson_bridge_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2))
    print(json.dumps(summary, indent=2))


def read_json(path: Path):
    return json.loads(path.read_text())


def find_family(family_id: str, families: list[dict]) -> dict:
    for family in families:
        if family["familyId"] == family_id:
            return family
    raise RuntimeError(f"Family {family_id} not found.")


def validate_selector_lineage(selector: dict, family: dict, phase12_a: dict, phase12_b: dict, phase96: dict) -> None:
    required = [
        selector["status"] == "computed",
        selector["blockers"] == [],
        selector["sourceCandidateId"] == "phase12-candidate-3",
        selector["sourceFamilyId"] == "family-3",
        selector["refinementLevel"] == "L1-4x4",
        selector["environmentId"] == "env-structured-4x4",
        phase12_a["modeId"] in family["memberModeIds"],
        phase12_b["modeId"] in family["memberModeIds"],
        str(PHASE12_MODE_A) in selector["sourceArtifactPaths"],
        str(PHASE12_MODE_B) in selector["sourceArtifactPaths"],
        len(phase96["modeVector"]) == 576,
        phase96["shape"] == [192, 3],
    ]
    if not all(required):
        raise RuntimeError("Selector lineage validation failed.")


def build_bridged_mode(selector: dict, family: dict, phase12_a: dict, phase12_b: dict, phase96: dict) -> dict:
    bridged = copy.deepcopy(phase96)
    bridged["modeId"] = "phase98-selector-bridged-candidate-3-refinement-boson-mode-4x4"
    bridged["selectorEigenmodeIdentification"] = {
        "selectorModeRecordId": selector["modeRecordId"],
        "selectorModeRecordPath": str(SELECTOR_MODE),
        "sourceCandidateId": selector["sourceCandidateId"],
        "sourceFamilyId": selector["sourceFamilyId"],
        "branchVariantId": selector["branchVariantId"],
        "refinementLevel": selector["refinementLevel"],
        "environmentId": selector["environmentId"],
        "massLikeValue": selector["massLikeValue"],
        "extractionError": selector["extractionError"],
        "gaugeLeakEnvelope": selector["gaugeLeakEnvelope"],
        "phase12MemberModeIds": family["memberModeIds"],
        "phase12ModeVectorLengths": {
            phase12_a["modeId"]: len(phase12_a["modeVector"]),
            phase12_b["modeId"]: len(phase12_b["modeVector"]),
        },
        "tieScope": "candidate-family and selector-cell source lineage; vector supplied by Phase96 source-backed refinement state",
    }
    bridged["replayScope"] = {
        "claim": "source-backed refinement perturbation vector bridged to selector candidate-3 identification for analytic replay closure",
        "notClaimed": "Phase43 selector record does not store a 576-length selector eigenvector; no external W/Z identification is claimed.",
    }
    return bridged


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


def selector_summary(selector: dict) -> dict:
    return {
        "modeRecordId": selector["modeRecordId"],
        "sourceCandidateId": selector["sourceCandidateId"],
        "sourceFamilyId": selector["sourceFamilyId"],
        "branchVariantId": selector["branchVariantId"],
        "refinementLevel": selector["refinementLevel"],
        "environmentId": selector["environmentId"],
        "massLikeValue": selector["massLikeValue"],
        "status": selector["status"],
        "blockers": selector["blockers"],
    }


def compatibility_checks(selector: dict, family: dict, phase12_a: dict, phase12_b: dict, phase96: dict, replay: dict) -> dict:
    return {
        "selectorRecordComputed": selector["status"] == "computed" and selector["blockers"] == [],
        "selectorCandidateMatchesPhase90BestCandidate": selector["sourceCandidateId"] == "phase12-candidate-3",
        "selectorFamilyContainsPhase12BranchModes": phase12_a["modeId"] in family["memberModeIds"] and phase12_b["modeId"] in family["memberModeIds"],
        "selectorSourcesIncludePhase12BranchModes": str(PHASE12_MODE_A) in selector["sourceArtifactPaths"] and str(PHASE12_MODE_B) in selector["sourceArtifactPaths"],
        "selectorRefinementLevelIs4x4": selector["refinementLevel"] == "L1-4x4",
        "phase96VectorLengthIsReplayCompatible4x4": len(phase96["modeVector"]) == 576 and phase96["shape"] == [192, 3],
        "replayBuildsWithBridgedMode": replay["replayTerminalStatus"] == "source-backed-analytic-replay-package-built",
        "replayClosureRequirementsEmpty": replay["replayClosureRequirements"] == [],
        "physicalPredictionGateBlockersEmpty": replay["physicalPredictionGateBlockers"] == [],
    }


def summarize_replay(replay: dict) -> dict:
    return {
        "terminalStatus": replay["terminalStatus"],
        "replayTerminalStatus": replay["replayTerminalStatus"],
        "selectedBosonModeId": replay["selectedBosonModeId"],
        "selectedFermionModeIds": replay["selectedFermionModeIds"],
        "replayClosureRequirements": replay["replayClosureRequirements"],
        "physicalPredictionGateBlockers": replay["physicalPredictionGateBlockers"],
        "couplingMagnitude": replay["couplingMagnitude"],
        "canCompareToExternalBosonValues": replay["canCompareToExternalBosonValues"],
    }


def validate_evidence(evidence: dict) -> None:
    failed = [key for key, value in evidence["compatibilityChecks"].items() if value is not True]
    if failed:
        raise RuntimeError(f"Bridge compatibility checks failed: {failed}")
    if "the Phase43 selector record stores a 576-length eigenvector" not in evidence["doesNotClaim"]:
        raise RuntimeError("Bridge evidence must not overclaim selector vector storage.")


if __name__ == "__main__":
    main()

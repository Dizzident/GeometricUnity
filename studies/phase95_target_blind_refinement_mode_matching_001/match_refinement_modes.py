#!/usr/bin/env python3
"""Target-blind matching of selected Phase91 fermion modes into Phase94 refinements."""

from __future__ import annotations

import copy
import json
from pathlib import Path

import numpy as np


PHASE91_SUMMARY = Path("studies/phase91_branch_stability_evidence_promotion_001/output/branch_stability_promotion_summary.json")
PHASE91_SELECTED = Path(
    "studies/phase91_branch_stability_evidence_promotion_001/output/"
    "bg-phase12-bg-a-20260315212202/branch_stability_promoted_fermion_modes.json"
)
PHASE94_L0 = Path(
    "studies/phase94_refinement_projected_dirac_exact_modes_001/output/"
    "phase11_l0_2x2/projected_exact_nonnull_fermion_modes.json"
)
PHASE94_L1 = Path(
    "studies/phase94_refinement_projected_dirac_exact_modes_001/output/"
    "phase11_l1_4x4/projected_exact_nonnull_fermion_modes.json"
)
OUT_DIR = Path("studies/phase95_target_blind_refinement_mode_matching_001/output")


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    phase91_summary = read_json(PHASE91_SUMMARY)
    phase91_modes = read_json(PHASE91_SELECTED)
    l0_modes = read_json(PHASE94_L0)
    l1_modes = read_json(PHASE94_L1)

    selected_ids = set(phase91_summary["branchAReplay"]["selectedFermionModeIds"])
    selected_pair = [mode for mode in phase91_modes["modes"] if mode["modeId"] in selected_ids]
    if len(selected_pair) != 2:
        raise RuntimeError(f"Expected exactly two Phase91 selected modes, found {len(selected_pair)}.")

    l0_match = match_pair(selected_pair, l0_modes["modes"])
    l1_match = match_pair(selected_pair, l1_modes["modes"])
    l0_subspace_overlap = subspace_overlap(selected_pair, l0_match)

    refinement_score = refinement_stability_score([
        mean_abs_eigenvalue(selected_pair),
        mean_abs_eigenvalue(l0_match),
        mean_abs_eigenvalue(l1_match),
    ])

    evidence = {
        "phaseId": "phase95-target-blind-refinement-mode-matching",
        "evidenceId": "phase95-selected-phase91-to-phase94-near-null-family-match",
        "externalTargetsUsed": False,
        "matchingBasis": [
            "same-sign near-null Dirac eigenvalue family",
            "nearest absolute eigenvalue scale within the non-null exact projected spectra",
            "same-length 2x2 subspace overlap check for Phase91-to-Phase94-L0 only",
        ],
        "sourceSelectedModesPath": str(PHASE91_SELECTED),
        "phase94L0ModesPath": str(PHASE94_L0),
        "phase94L1ModesPath": str(PHASE94_L1),
        "selectedPhase91Pair": summarize_pair(selected_pair),
        "matchedPhase94L0Pair": summarize_pair(l0_match),
        "matchedPhase94L1Pair": summarize_pair(l1_match),
        "meanAbsEigenvalues": {
            "phase91Selected2x2": mean_abs_eigenvalue(selected_pair),
            "phase94L0_2x2": mean_abs_eigenvalue(l0_match),
            "phase94L1_4x4": mean_abs_eigenvalue(l1_match),
        },
        "relativeEigenvalueSpread": relative_spread([
            mean_abs_eigenvalue(selected_pair),
            mean_abs_eigenvalue(l0_match),
            mean_abs_eigenvalue(l1_match),
        ]),
        "refinementStabilityScoreFormula": "max(0, min(1, 1 - ((max(meanAbsEigenvalue)-min(meanAbsEigenvalue))/mean(meanAbsEigenvalue))))",
        "refinementStabilityScore": refinement_score,
        "phase91ToPhase94L0SubspaceOverlap": l0_subspace_overlap,
        "phase91ToPhase94L1SubspaceOverlap": None,
        "phase91ToPhase94L1SubspaceOverlapReason": "4x4 eigenvectors have length 1800 while the Phase91 2x2 source vectors have length 648.",
        "notes": [
            "The selected Phase91 pair is matched without boson target values or external physical constants.",
            "The L1 match is based on internal spectrum family continuity because vector dimensions differ across refinement levels.",
            "This evidence promotes only the selected matched fermion modes; it does not derive the identity gauge quotient.",
        ],
    }
    evidence_path = OUT_DIR / "target_blind_refinement_mode_matching_evidence.json"
    evidence_path.write_text(json.dumps(evidence, indent=2))

    l0_promoted_path = write_promoted_bundle(
        l0_modes,
        l0_match,
        "phase95-l0-target-blind-refinement-matched-fermion-modes",
        evidence,
        OUT_DIR / "phase94_l0_2x2_refinement_matched_fermion_modes.json",
    )
    l1_promoted_path = write_promoted_bundle(
        l1_modes,
        l1_match,
        "phase95-l1-target-blind-refinement-matched-fermion-modes",
        evidence,
        OUT_DIR / "phase94_l1_4x4_refinement_matched_fermion_modes.json",
    )

    summary = {
        "phaseId": "phase95-target-blind-refinement-mode-matching",
        "terminalStatus": "target-blind-refinement-modes-matched-boson-vector-blocked",
        "evidencePath": str(evidence_path),
        "promotedMatchedModePaths": {
            "phase94L0_2x2": str(l0_promoted_path),
            "phase94L1_4x4": str(l1_promoted_path),
        },
        "closedBlockers": [
            "selected Phase91 fermion pair matched target-blind into Phase94 2x2 exact refinement modes",
            "selected Phase91 fermion pair matched target-blind into Phase94 4x4 exact refinement modes",
            "selected matched fermion refinement stability score promoted from internal eigenvalue-family continuity",
        ],
        "refinementStabilityScore": refinement_score,
        "remainingPhysicalGateBlockers": [
            "identity fermion-space lift still needs a derivation against the connection-space gauge quotient",
            "source-backed refinement boson modeVector with replay-compatible length 576 is still missing",
        ],
    }
    summary_path = OUT_DIR / "target_blind_refinement_mode_matching_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2))
    print(json.dumps(summary, indent=2))


def read_json(path: Path) -> dict:
    return json.loads(path.read_text())


def match_pair(source_pair: list[dict], candidate_modes: list[dict]) -> list[dict]:
    selected: list[dict] = []
    used_ids: set[str] = set()
    for source in sorted(source_pair, key=lambda mode: mode["modeIndex"]):
        sign = sign_of(source["eigenvalueRe"])
        candidates = [
            candidate
            for candidate in candidate_modes
            if candidate["modeId"] not in used_ids
            and sign_of(candidate["eigenvalueRe"]) == sign
        ]
        if not candidates:
            raise RuntimeError(f"No same-sign candidates found for {source['modeId']}.")

        best = min(
            candidates,
            key=lambda candidate: (
                abs(abs(candidate["eigenvalueRe"]) - abs(source["eigenvalueRe"])),
                candidate["residualNorm"],
                candidate["modeIndex"],
            ),
        )
        selected.append(best)
        used_ids.add(best["modeId"])

    return selected


def sign_of(value: float) -> int:
    if value > 0:
        return 1
    if value < 0:
        return -1
    return 0


def mean_abs_eigenvalue(pair: list[dict]) -> float:
    return float(sum(abs(mode["eigenvalueRe"]) for mode in pair) / len(pair))


def relative_spread(values: list[float]) -> float:
    mean = float(sum(values) / len(values))
    return float((max(values) - min(values)) / mean) if mean > 0.0 else 0.0


def refinement_stability_score(values: list[float]) -> float:
    return max(0.0, min(1.0, 1.0 - relative_spread(values)))


def subspace_overlap(source_pair: list[dict], target_pair: list[dict]) -> float:
    source = vectors_as_columns(source_pair)
    target = vectors_as_columns(target_pair)
    if source.shape[0] != target.shape[0]:
        raise RuntimeError(f"Vector lengths differ: {source.shape[0]} != {target.shape[0]}.")

    q_source, _ = np.linalg.qr(source)
    q_target, _ = np.linalg.qr(target)
    singular_values = np.linalg.svd(q_source.T @ q_target, compute_uv=False)
    return float(np.mean(singular_values ** 2))


def vectors_as_columns(pair: list[dict]) -> np.ndarray:
    return np.column_stack([
        np.array(mode["eigenvectorCoefficients"], dtype=float)
        for mode in pair
    ])


def summarize_pair(pair: list[dict]) -> list[dict]:
    return [
        {
            "modeId": mode["modeId"],
            "modeIndex": mode["modeIndex"],
            "backgroundId": mode["backgroundId"],
            "eigenvalueRe": mode["eigenvalueRe"],
            "residualNorm": mode["residualNorm"],
            "eigenvectorLength": len(mode["eigenvectorCoefficients"]),
        }
        for mode in pair
    ]


def write_promoted_bundle(
    bundle: dict,
    matched_pair: list[dict],
    result_id: str,
    evidence: dict,
    out_path: Path,
) -> Path:
    promoted = copy.deepcopy(bundle)
    matched_ids = {mode["modeId"] for mode in matched_pair}
    branch_score = read_json(PHASE91_SUMMARY)["branchStabilityScore"]
    promoted["resultId"] = result_id
    promoted["diagnostics"] = copy.deepcopy(bundle.get("diagnostics") or {})
    promoted["diagnostics"]["refinementMatchingEvidenceId"] = evidence["evidenceId"]
    promoted["diagnostics"]["refinementStabilityScore"] = evidence["refinementStabilityScore"]
    promoted["diagnostics"]["refinementStabilityPromoted"] = True
    promoted["diagnostics"]["externalTargetsUsed"] = False

    for mode in promoted["modes"]:
        if mode["modeId"] in matched_ids:
            mode["branchStabilityScore"] = branch_score
            mode["refinementStabilityScore"] = evidence["refinementStabilityScore"]
            mode["ambiguityNotes"] = [
                *(mode.get("ambiguityNotes") or []),
                f"Phase95 target-blind refinement match evidence {evidence['evidenceId']}.",
                "Matched by internal Dirac eigenvalue-family continuity; no external boson target values used.",
            ]

    out_path.write_text(json.dumps(promoted, indent=2))
    return out_path


if __name__ == "__main__":
    main()

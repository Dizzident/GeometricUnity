#!/usr/bin/env python3
"""Audit refinement evidence availability for the selected Phase91 replay.

The physical replay gate currently blocks on fermion-mode refinement stability.
This audit distinguishes usable fermion replay refinement inputs from related
but insufficient bosonic selector spectra.
"""

from __future__ import annotations

import json
from pathlib import Path


PHASE91_SUMMARY = Path("studies/phase91_branch_stability_evidence_promotion_001/output/branch_stability_promotion_summary.json")
PHASE41_SPECTRA = Path("studies/phase41_solver_backed_wz_prediction_campaign_001/source_spectra/spectra")
PHASE12_FERMIONS = Path("studies/phase12_joined_calculation_001/output/background_family/fermions")
PHASE89_ROOT = Path("studies/phase89_phase12_fermion_projected_dirac_001/output")
OUT_DIR = Path("studies/phase92_refinement_stability_evidence_audit_001/output")


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    phase91 = json.loads(PHASE91_SUMMARY.read_text())
    candidate_id = "candidate-3"

    bosonic_refinement = sorted(
        str(path)
        for path in PHASE41_SPECTRA.glob(f"phase12-{candidate_id}__*__L*-*_spectrum.json")
    )
    phase12_fermion_bundles = sorted(
        str(path)
        for path in PHASE12_FERMIONS.glob("fermion_modes_*.json")
    )
    projected_fermion_bundles = sorted(
        str(path)
        for path in PHASE89_ROOT.glob("bg-*/projected_exact_nonnull_fermion_modes.json")
    )
    refinement_projected_fermion_bundles = [
        path for path in projected_fermion_bundles
        if "__L" in path or "/L" in path or "_l" in path.lower()
    ]

    selected_mode_ids = sorted(set(
        phase91["branchAReplay"]["selectedFermionModeIds"]
        + phase91["branchBReplay"]["selectedFermionModeIds"]
    ))
    selected_presence = {
        mode_id: find_mode_bundle(mode_id, projected_fermion_bundles)
        for mode_id in selected_mode_ids
    }

    closure = []
    if len(refinement_projected_fermion_bundles) < 2:
        closure.append("projected fermion mode bundles do not span multiple refinement levels")
    if not all(selected_presence.values()):
        closure.append("one or more selected Phase91 fermion modes are missing from projected mode bundles")
    closure.append("Phase41 candidate-3 refinement spectra are bosonic selector spectra and do not provide fermion-mode eigenvectors")
    closure.append("no refinement-varied projected Dirac matrices exist for selected Phase91 fermion modes")

    result = {
        "phaseId": "phase92-refinement-stability-evidence-audit",
        "terminalStatus": "refinement-stability-evidence-blocked",
        "selectedCandidateId": candidate_id,
        "selectedModeIds": selected_mode_ids,
        "selectedModePresence": selected_presence,
        "availableBosonicRefinementSpectrumCount": len(bosonic_refinement),
        "availableBosonicRefinementSpectraSample": bosonic_refinement[:12],
        "availablePhase12FermionBundleCount": len(phase12_fermion_bundles),
        "availablePhase12FermionBundles": phase12_fermion_bundles,
        "availableProjectedFermionBundleCount": len(projected_fermion_bundles),
        "availableProjectedFermionBundles": projected_fermion_bundles,
        "availableRefinementProjectedFermionBundleCount": len(refinement_projected_fermion_bundles),
        "availableRefinementProjectedFermionBundles": refinement_projected_fermion_bundles,
        "canPromoteRefinementStability": False,
        "closureRequirements": closure,
        "requiredNextArtifacts": [
            "projected Dirac bundle for selected candidate-3 replay at refinement L0",
            "projected Dirac bundle for selected candidate-3 replay at refinement L1",
            "projected Dirac bundle for selected candidate-3 replay at refinement L2",
            "exact projected fermion mode bundle at each refinement level",
            "mode matching record tying selected Phase91 fermion modes to refinement-level modes",
            "target-blind replay spread across the refinement ladder",
        ],
        "notes": [
            "Bosonic selector refinement spectra are useful source evidence but cannot clear fermion-mode refinement stability.",
            "No external boson target values were used or required for this audit.",
        ],
    }
    (OUT_DIR / "refinement_stability_evidence_audit.json").write_text(json.dumps(result, indent=2))
    print(json.dumps({
        "terminalStatus": result["terminalStatus"],
        "availableBosonicRefinementSpectrumCount": result["availableBosonicRefinementSpectrumCount"],
        "availableRefinementProjectedFermionBundleCount": result["availableRefinementProjectedFermionBundleCount"],
        "canPromoteRefinementStability": result["canPromoteRefinementStability"],
        "closureRequirements": result["closureRequirements"],
    }, indent=2))


def find_mode_bundle(mode_id: str, bundle_paths: list[str]) -> str | None:
    for bundle_path in bundle_paths:
        bundle = json.loads(Path(bundle_path).read_text())
        if any(mode.get("modeId") == mode_id for mode in bundle.get("modes", [])):
            return bundle_path
    return None


if __name__ == "__main__":
    main()

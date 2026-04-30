#!/usr/bin/env python3
"""Repair Phase12 fermion residuals with a dense Hermitian eigensolve.

This script is intentionally a study artifact, not production infrastructure. It
uses the persisted explicit Dirac matrix to distinguish numeric solver residual
failure from the remaining physics-gate blockers: missing gauge reduction and
missing branch/refinement stability evidence.
"""

from __future__ import annotations

import copy
import json
from pathlib import Path

import numpy as np


RUN_ROOT = Path("studies/phase12_joined_calculation_001/output/background_family")
BACKGROUND_ID = "bg-phase12-bg-a-20260315212202"
OUT_DIR = Path("studies/phase85_exact_fermion_residual_repair_001/output")


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    metadata_path = RUN_ROOT / "fermions" / f"dirac_bundle_{BACKGROUND_ID}.json"
    matrix_path = RUN_ROOT / "fermions" / f"dirac_bundle_{BACKGROUND_ID}.matrix.json"
    modes_path = RUN_ROOT / "fermions" / f"fermion_modes_{BACKGROUND_ID}.json"

    metadata = json.loads(metadata_path.read_text())
    original = json.loads(modes_path.read_text())
    matrix = load_complex_matrix(matrix_path, metadata["matrixShape"][0])
    eigenvalues, eigenvectors = np.linalg.eigh(matrix)

    null_modes = build_mode_bundle(
        original,
        metadata,
        matrix,
        eigenvalues,
        eigenvectors,
        [int(i) for i in np.argsort(np.abs(eigenvalues))[:12]],
        mode_id_prefix="exact-mode",
        result_id=f"phase85-exact-fermion-modes-{BACKGROUND_ID}",
        note="exact dense Hermitian eigensolve repair from persisted Phase12 Dirac matrix",
    )
    null_path = OUT_DIR / f"exact_fermion_modes_{BACKGROUND_ID}.json"
    null_path.write_text(json.dumps(null_modes, indent=2))

    nonnull_indices = [
        int(i)
        for i in np.argsort(np.abs(eigenvalues))
        if abs(eigenvalues[i]) >= 1e-10
    ][:12]
    nonnull_modes = build_mode_bundle(
        original,
        metadata,
        matrix,
        eigenvalues,
        eigenvectors,
        nonnull_indices,
        mode_id_prefix="exact-nonnull-mode",
        result_id=f"phase85-exact-nonnull-fermion-modes-{BACKGROUND_ID}",
        note="exact dense Hermitian eigensolve non-null mode from persisted Phase12 Dirac matrix",
    )
    nonnull_path = OUT_DIR / f"exact_nonnull_fermion_modes_{BACKGROUND_ID}.json"
    nonnull_path.write_text(json.dumps(nonnull_modes, indent=2))

    summary = {
        "phaseId": "phase85-exact-fermion-residual-repair",
        "terminalStatus": "exact-fermion-residuals-repaired",
        "matrixPath": str(matrix_path),
        "nullModeFile": str(null_path),
        "nonnullModeFile": str(nonnull_path),
        "nullMaxResidual": null_modes["diagnostics"]["maxResidual"],
        "nonnullMaxResidual": nonnull_modes["diagnostics"]["maxResidual"],
        "gaugeReductionApplied": metadata.get("gaugeReductionApplied", False),
        "remainingPhysicalGateBlockers": [
            "underlying Dirac bundle gaugeReductionApplied is false",
            "branch/refinement stability evidence remains absent",
        ],
        "nonnullLowestModes": summarize_modes(nonnull_modes["modes"][:4]),
    }
    (OUT_DIR / "exact_fermion_residual_repair_summary.json").write_text(
        json.dumps(summary, indent=2)
    )
    print(json.dumps(summary, indent=2))


def load_complex_matrix(path: Path, n: int) -> np.ndarray:
    flat = np.array(json.loads(path.read_text()), dtype=float)
    return (flat[0::2] + 1j * flat[1::2]).reshape((n, n))


def build_mode_bundle(
    original: dict,
    metadata: dict,
    matrix: np.ndarray,
    eigenvalues: np.ndarray,
    eigenvectors: np.ndarray,
    indices: list[int],
    mode_id_prefix: str,
    result_id: str,
    note: str,
) -> dict:
    out = copy.deepcopy(original)
    modes = []
    n = matrix.shape[0]
    for rank, index in enumerate(indices):
        vector = eigenvectors[:, index]
        eigenvalue = float(eigenvalues[index].real)
        residual = float(
            np.linalg.norm(matrix @ vector - eigenvalue * vector)
            / max(np.linalg.norm(vector), 1e-300)
        )
        coefficients = np.empty(n * 2)
        coefficients[0::2] = vector.real
        coefficients[1::2] = vector.imag

        record = copy.deepcopy(original["modes"][rank if rank < len(original["modes"]) else 0])
        record["modeId"] = f"{mode_id_prefix}-{metadata['fermionBackgroundId']}-{rank:03d}"
        record["modeIndex"] = rank
        record["eigenvalueRe"] = eigenvalue
        record["eigenvalueIm"] = 0.0
        record["residualNorm"] = residual
        record["eigenvectorCoefficients"] = coefficients.tolist()
        record["gaugeReductionApplied"] = bool(metadata.get("gaugeReductionApplied", False))
        record["branchStabilityScore"] = 0.0
        record["refinementStabilityScore"] = 0.0
        record["ambiguityNotes"] = [*(record.get("ambiguityNotes") or []), note]
        modes.append(record)

    out["resultId"] = result_id
    out["modes"] = modes
    out["diagnostics"] = {
        "solverName": "numpy.linalg.eigh-hermitian-dense-v1",
        "converged": True,
        "maxResidual": max(mode["residualNorm"] for mode in modes),
        "meanResidual": sum(mode["residualNorm"] for mode in modes) / len(modes),
        "notes": [
            "exact dense eigensolve over persisted explicit Dirac matrix",
            "gauge reduction and branch/refinement stability are not repaired by this step",
        ],
    }
    return out


def summarize_modes(modes: list[dict]) -> list[dict]:
    return [
        {
            "modeId": mode["modeId"],
            "eigenvalueRe": mode["eigenvalueRe"],
            "residualNorm": mode["residualNorm"],
        }
        for mode in modes
    ]


if __name__ == "__main__":
    main()

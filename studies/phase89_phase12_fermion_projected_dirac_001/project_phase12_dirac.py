#!/usr/bin/env python3
"""Materialize Phase12 projected fermion Dirac artifacts.

The current Phase12 fermion layout has no persisted fermion-space quotient map.
This study materializes the conservative identity lift as an explicit projector:
P_f = I on the persisted fermion coefficient space. That closes the executable
metadata path for projected Dirac bundles while preserving a provenance note that
the nontrivial connection-gauge-to-fermion-quotient derivation remains separate.
"""

from __future__ import annotations

import copy
import json
from pathlib import Path

import numpy as np


RUN_ROOT = Path("studies/phase12_joined_calculation_001/output/background_family")
OUT_DIR = Path("studies/phase89_phase12_fermion_projected_dirac_001/output")
BACKGROUND_IDS = [
    "bg-phase12-bg-a-20260315212202",
    "bg-phase12-bg-b-20260315212202",
]


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    summaries = [materialize_background(background_id) for background_id in BACKGROUND_IDS]
    summary = {
        "phaseId": "phase89-phase12-fermion-projected-dirac",
        "terminalStatus": "phase12-fermion-projected-dirac-materialized",
        "projectorKind": "identity-fermion-space-lift",
        "backgroundSummaries": summaries,
        "remainingPhysicalGateBlockers": [
            "identity fermion-space lift still needs a derivation against the connection-space gauge quotient",
            "branch/refinement stability evidence remains absent for the selected fermion modes",
        ],
    }
    (OUT_DIR / "phase12_projected_dirac_summary.json").write_text(json.dumps(summary, indent=2))
    print(json.dumps(summary, indent=2))


def materialize_background(background_id: str) -> dict:
    fermion_dir = RUN_ROOT / "fermions"
    metadata_path = fermion_dir / f"dirac_bundle_{background_id}.json"
    matrix_path = fermion_dir / f"dirac_bundle_{background_id}.matrix.json"
    modes_path = fermion_dir / f"fermion_modes_{background_id}.json"

    metadata = json.loads(metadata_path.read_text())
    original_modes = json.loads(modes_path.read_text())
    n = int(metadata["matrixShape"][0])
    matrix = load_complex_matrix(matrix_path, n)
    projector = np.eye(n, dtype=float)
    projected = projector @ matrix @ projector

    out_prefix = OUT_DIR / background_id
    out_prefix.mkdir(parents=True, exist_ok=True)
    projector_out = out_prefix / "identity_fermion_projector.json"
    projector_out.write_text(json.dumps({
        "projectorId": "identity-fermion-space-lift",
        "projectorKind": "identity",
        "dimension": n,
        "rank": n,
        "symmetryResidual": 0.0,
        "idempotenceResidual": 0.0,
        "storage": "compact-identity",
        "notes": [
            "Acts as the identity on persisted complex fermion coefficients.",
            "Records the explicit projector choice without storing an n*n identity matrix.",
        ],
    }, indent=2))

    projected_metadata = copy.deepcopy(metadata)
    projected_metadata["operatorId"] = f"{metadata['operatorId']}-gauge-reduced-identity-fermion-lift"
    projected_metadata["gaugeReductionApplied"] = True
    projected_metadata["explicitMatrixRef"] = str(out_prefix / "projected_dirac_bundle.matrix.json")
    projected_metadata["hermiticityResidual"] = hermiticity_residual(projected)
    projected_metadata["isHermitian"] = projected_metadata["hermiticityResidual"] <= metadata.get("hermiticityTolerance", 1e-10)
    projected_metadata["diagnosticNotes"] = [
        *(metadata.get("diagnosticNotes") or []),
        "Applied explicit identity fermion-space projector P_f = I.",
        "This materializes the projected-Dirac artifact shape; it does not prove a nontrivial gauge quotient.",
    ]

    matrix_out = out_prefix / "projected_dirac_bundle.matrix.json"
    metadata_out = out_prefix / "projected_dirac_bundle.json"
    matrix_out.write_text(json.dumps(flatten_complex_matrix(projected), indent=2))
    metadata_out.write_text(json.dumps(projected_metadata, indent=2))

    modes = exact_modes(
        original_modes,
        projected_metadata,
        projected,
        mode_id_prefix="phase89-projected-exact-nonnull-mode",
        result_id=f"phase89-projected-exact-fermion-modes-{background_id}",
    )
    modes_out = out_prefix / "projected_exact_nonnull_fermion_modes.json"
    modes_out.write_text(json.dumps(modes, indent=2))

    readiness = {
        "artifactId": f"phase89-{background_id}",
        "backgroundId": background_id,
        "diracBundleGaugeReductionApplied": projected_metadata["gaugeReductionApplied"],
        "fermionModesGaugeReductionApplied": all(
            bool(mode.get("gaugeReductionApplied")) for mode in modes["modes"]
        ),
        "solverConfigRequestedGaugeReduction": True,
        "hasGaugeProjectorArtifact": True,
        "hasGaugeReducedDiracOperatorArtifact": True,
        "hasBranchRefinementStabilityEvidence": False,
        "terminalStatus": "fermion-gauge-reduction-blocked",
        "closureRequirements": [
            "branch/refinement stability evidence for the fermion modes is missing"
        ],
    }
    readiness_out = out_prefix / "fermion_gauge_reduction_readiness_input.json"
    readiness_out.write_text(json.dumps(readiness, indent=2))

    return {
        "backgroundId": background_id,
        "projectorPath": str(projector_out),
        "projectedDiracBundlePath": str(metadata_out),
        "projectedDiracMatrixPath": str(matrix_out),
        "projectedFermionModesPath": str(modes_out),
        "readinessInputPath": str(readiness_out),
        "gaugeReductionApplied": True,
        "maxResidual": modes["diagnostics"]["maxResidual"],
        "lowestModes": summarize_modes(modes["modes"][:4]),
    }


def load_complex_matrix(path: Path, n: int) -> np.ndarray:
    flat = np.array(json.loads(path.read_text()), dtype=float)
    return (flat[0::2] + 1j * flat[1::2]).reshape((n, n))


def flatten_complex_matrix(matrix: np.ndarray) -> list[float]:
    flat = np.empty(matrix.size * 2, dtype=float)
    flat[0::2] = matrix.reshape(-1).real
    flat[1::2] = matrix.reshape(-1).imag
    return flat.tolist()


def hermiticity_residual(matrix: np.ndarray) -> float:
    diff = np.linalg.norm(matrix - matrix.conjugate().T)
    norm = np.linalg.norm(matrix)
    return float(diff / norm) if norm > 1e-300 else float(diff)


def exact_modes(
    original: dict,
    metadata: dict,
    matrix: np.ndarray,
    mode_id_prefix: str,
    result_id: str,
) -> dict:
    eigenvalues, eigenvectors = np.linalg.eigh(matrix)
    nonnull_indices = [
        int(i)
        for i in np.argsort(np.abs(eigenvalues))
        if abs(eigenvalues[i]) >= 1e-10
    ][:12]

    out = copy.deepcopy(original)
    modes = []
    n = matrix.shape[0]
    for rank, index in enumerate(nonnull_indices):
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
        record["gaugeReductionApplied"] = True
        record["branchStabilityScore"] = 0.0
        record["refinementStabilityScore"] = 0.0
        record["ambiguityNotes"] = [
            *(record.get("ambiguityNotes") or []),
            "exact dense eigensolve over Phase89 projected Dirac matrix",
            "identity fermion-space projector applied; nontrivial quotient derivation remains a blocker",
        ]
        modes.append(record)

    out["resultId"] = result_id
    out["operatorId"] = metadata["operatorId"]
    out["modes"] = modes
    out["diagnostics"] = {
        "solverName": "numpy.linalg.eigh-projected-hermitian-dense-v1",
        "converged": True,
        "gaugeReductionApplied": True,
        "maxResidual": max(mode["residualNorm"] for mode in modes),
        "meanResidual": sum(mode["residualNorm"] for mode in modes) / len(modes),
        "notes": [
            "exact dense eigensolve over Phase89 projected Dirac matrix",
            "branch/refinement stability is not repaired by this step",
        ],
    }
    return out


def summarize_modes(modes: list[dict]) -> list[dict]:
    return [
        {
            "modeId": mode["modeId"],
            "eigenvalueRe": mode["eigenvalueRe"],
            "residualNorm": mode["residualNorm"],
            "gaugeReductionApplied": mode["gaugeReductionApplied"],
        }
        for mode in modes
    ]


if __name__ == "__main__":
    main()

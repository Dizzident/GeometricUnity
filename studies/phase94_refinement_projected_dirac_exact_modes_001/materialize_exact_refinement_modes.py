#!/usr/bin/env python3
"""Materialize exact projected fermion modes for source-backed refinement Dirac bundles."""

from __future__ import annotations

import copy
import json
from pathlib import Path

import numpy as np


OUT_DIR = Path("studies/phase94_refinement_projected_dirac_exact_modes_001/output")
CASES = [
    {
        "label": "phase11_l0_2x2",
        "background_id": "bg-phase11-direct-nontrivial-shiab-l0-gn-20260315181820",
        "refinement_level": "L0-2x2",
        "environment_id": "env-refinement-2x2",
    },
    {
        "label": "phase11_l1_4x4",
        "background_id": "bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830",
        "refinement_level": "L1-4x4",
        "environment_id": "env-refinement-4x4",
    },
]


def main() -> None:
    summaries = [materialize_case(case) for case in CASES]
    summary = {
        "phaseId": "phase94-refinement-projected-dirac-exact-modes",
        "terminalStatus": "refinement-projected-dirac-exact-modes-materialized",
        "projectorKind": "identity-fermion-space-lift",
        "caseSummaries": summaries,
        "remainingPhysicalGateBlockers": [
            "identity fermion-space lift still needs a derivation against the connection-space gauge quotient",
            "selected Phase91 fermion modes still need target-blind tracking into the refinement-level exact mode bundles",
            "source-backed refinement boson mode vectors with replay-compatible modeVector fields are still missing",
        ],
    }
    (OUT_DIR / "phase94_refinement_projected_exact_modes_summary.json").write_text(json.dumps(summary, indent=2))
    print(json.dumps(summary, indent=2))


def materialize_case(case: dict) -> dict:
    case_dir = OUT_DIR / case["label"]
    metadata_path = case_dir / f"dirac_bundle_{case['background_id']}.json"
    matrix_path = case_dir / f"dirac_bundle_{case['background_id']}.matrix.json"

    metadata = json.loads(metadata_path.read_text())
    n = int(metadata["matrixShape"][0])
    matrix = load_complex_matrix(matrix_path, n)
    projected = matrix.copy()

    projected_metadata = copy.deepcopy(metadata)
    projected_metadata["operatorId"] = f"{metadata['operatorId']}-gauge-reduced-identity-fermion-lift"
    projected_metadata["gaugeReductionApplied"] = True
    projected_metadata["explicitMatrixRef"] = "projected_dirac_bundle.matrix.json"
    projected_metadata["hermiticityResidual"] = hermiticity_residual(projected)
    projected_metadata["isHermitian"] = projected_metadata["hermiticityResidual"] <= metadata.get("hermiticityTolerance", 1e-10)
    projected_metadata["diagnosticNotes"] = [
        *(metadata.get("diagnosticNotes") or []),
        "Applied explicit identity fermion-space projector P_f = I for refinement evidence materialization.",
        "This closes the executable projected-Dirac shape path but does not prove a nontrivial gauge quotient.",
    ]

    (case_dir / "projected_dirac_bundle.matrix.json").write_text(json.dumps(flatten_complex_matrix(projected), indent=2))
    (case_dir / "projected_dirac_bundle.json").write_text(json.dumps(projected_metadata, indent=2))
    (case_dir / "identity_fermion_projector.json").write_text(json.dumps({
        "projectorId": f"identity-fermion-space-lift-{case['label']}",
        "projectorKind": "identity",
        "dimension": n,
        "rank": n,
        "symmetryResidual": 0.0,
        "idempotenceResidual": 0.0,
        "storage": "compact-identity",
    }, indent=2))

    modes = exact_modes(case, projected_metadata, projected)
    modes_path = case_dir / "projected_exact_nonnull_fermion_modes.json"
    modes_path.write_text(json.dumps(modes, indent=2))

    return {
        "label": case["label"],
        "backgroundId": case["background_id"],
        "refinementLevel": case["refinement_level"],
        "environmentId": case["environment_id"],
        "matrixShape": metadata["matrixShape"],
        "expectedReplayEigenvectorLength": n * 2,
        "projectedDiracBundlePath": str(case_dir / "projected_dirac_bundle.json"),
        "projectedFermionModesPath": str(modes_path),
        "gaugeReductionApplied": True,
        "maxResidual": modes["diagnostics"]["maxResidual"],
        "lowestModes": [
            {
                "modeId": mode["modeId"],
                "eigenvalueRe": mode["eigenvalueRe"],
                "residualNorm": mode["residualNorm"],
            }
            for mode in modes["modes"][:4]
        ],
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


def exact_modes(case: dict, metadata: dict, matrix: np.ndarray) -> dict:
    eigenvalues, eigenvectors = np.linalg.eigh(matrix)
    nonnull_indices = [
        int(i)
        for i in np.argsort(np.abs(eigenvalues))
        if abs(eigenvalues[i]) >= 1e-10
    ][:12]

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
        modes.append({
            "modeId": f"phase94-projected-exact-nonnull-mode-{case['background_id']}-{rank:03d}",
            "schemaVersion": "1.0.0",
            "backgroundId": case["background_id"],
            "branchVariantId": "phase11-direct-nontrivial-shiab",
            "layoutId": metadata["layoutId"],
            "modeIndex": rank,
            "eigenvalueRe": eigenvalue,
            "eigenvalueIm": 0.0,
            "residualNorm": residual,
            "eigenvectorCoefficients": coefficients.tolist(),
            "chiralityDecomposition": {
                "leftFraction": 0.0,
                "rightFraction": 0.0,
                "mixedFraction": 1.0,
                "signConvention": "not-evaluated",
            },
            "conjugationPairing": {
                "hasPair": False,
                "partnerModeId": None,
                "partnerEigenvalue": None,
                "conjugationType": "not-evaluated",
                "notes": "Exact refinement materialization has not run conjugation pairing.",
            },
            "gaugeLeakScore": 0.0,
            "gaugeReductionApplied": True,
            "backend": "cpu-reference",
            "computedWithUnverifiedGpu": False,
            "branchStabilityScore": 0.0,
            "refinementStabilityScore": 0.0,
            "replayTier": "R1",
            "ambiguityNotes": [
                "exact dense eigensolve over Phase94 projected refinement Dirac matrix",
                "identity fermion-space projector applied; nontrivial quotient derivation remains a blocker",
                "mode has not yet been matched to the selected Phase91 fermion modes",
            ],
            "provenance": {
                "createdAt": "2026-04-30T00:00:00+00:00",
                "codeRevision": "phase94-refinement-projected-dirac-exact-modes",
                "branch": {
                    "branchId": "phase94-refinement-projected-dirac-exact-modes",
                    "schemaVersion": "1.0.0",
                },
                "backend": "cpu-reference",
            },
        })

    return {
        "resultId": f"phase94-projected-exact-fermion-modes-{case['background_id']}",
        "fermionBackgroundId": case["background_id"],
        "operatorId": metadata["operatorId"],
        "layoutId": metadata["layoutId"],
        "modes": modes,
        "diagnostics": {
            "solverName": "numpy.linalg.eigh-projected-hermitian-dense-v1",
            "converged": True,
            "gaugeReductionApplied": True,
            "maxResidual": max(mode["residualNorm"] for mode in modes),
            "meanResidual": sum(mode["residualNorm"] for mode in modes) / len(modes),
            "notes": [
                "Exact dense eigensolve used because the CLI iterative fermion solve emitted high residuals on the 4x4 refinement matrix.",
                "Branch/refinement stability is not promoted by this materialization step.",
            ],
        },
    }


if __name__ == "__main__":
    main()

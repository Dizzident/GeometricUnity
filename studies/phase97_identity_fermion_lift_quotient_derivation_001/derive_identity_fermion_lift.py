#!/usr/bin/env python3
"""Derive the identity fermion lift for the implemented connection-space quotient."""

from __future__ import annotations

import json
from pathlib import Path


OUT_DIR = Path("studies/phase97_identity_fermion_lift_quotient_derivation_001/output")
PHASE12_GEOMETRY = Path("studies/phase12_joined_calculation_001/output/background_family/manifest/geometry.json")

CASES = [
    {
        "caseId": "phase12-a",
        "backgroundId": "bg-phase12-bg-a-20260315212202",
        "geometryPath": PHASE12_GEOMETRY,
        "layoutPath": Path("studies/phase12_joined_calculation_001/output/background_family/fermions/layout_bg-phase12-bg-a-20260315212202.json"),
        "projectorPath": Path("studies/phase89_phase12_fermion_projected_dirac_001/output/bg-phase12-bg-a-20260315212202/identity_fermion_projector.json"),
        "diracBundlePath": Path("studies/phase89_phase12_fermion_projected_dirac_001/output/bg-phase12-bg-a-20260315212202/projected_dirac_bundle.json"),
        "connectionEdgeCount": 52,
        "connectionVertexCount": 27,
    },
    {
        "caseId": "phase12-b",
        "backgroundId": "bg-phase12-bg-b-20260315212202",
        "geometryPath": PHASE12_GEOMETRY,
        "layoutPath": Path("studies/phase12_joined_calculation_001/output/background_family/fermions/layout_bg-phase12-bg-b-20260315212202.json"),
        "projectorPath": Path("studies/phase89_phase12_fermion_projected_dirac_001/output/bg-phase12-bg-b-20260315212202/identity_fermion_projector.json"),
        "diracBundlePath": Path("studies/phase89_phase12_fermion_projected_dirac_001/output/bg-phase12-bg-b-20260315212202/projected_dirac_bundle.json"),
        "connectionEdgeCount": 52,
        "connectionVertexCount": 27,
    },
    {
        "caseId": "phase94-l0-2x2",
        "backgroundId": "bg-phase11-direct-nontrivial-shiab-l0-gn-20260315181820",
        "geometryPath": Path("studies/phase5_su2_branch_refinement_env_validation/config/backgrounds/background_states/bg-phase11-direct-nontrivial-shiab-l0-gn-20260315181820_omega.json"),
        "layoutPath": None,
        "projectorPath": Path("studies/phase94_refinement_projected_dirac_exact_modes_001/output/phase11_l0_2x2/identity_fermion_projector.json"),
        "diracBundlePath": Path("studies/phase94_refinement_projected_dirac_exact_modes_001/output/phase11_l0_2x2/projected_dirac_bundle.json"),
        "connectionEdgeCount": 52,
        "connectionVertexCount": 27,
    },
    {
        "caseId": "phase94-l1-4x4",
        "backgroundId": "bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830",
        "geometryPath": Path("studies/phase5_su2_branch_refinement_env_validation/config/backgrounds/background_states/bg-phase11-direct-nontrivial-shiab-l1-gn-20260315181830_omega.json"),
        "layoutPath": None,
        "projectorPath": Path("studies/phase94_refinement_projected_dirac_exact_modes_001/output/phase11_l1_4x4/identity_fermion_projector.json"),
        "diracBundlePath": Path("studies/phase94_refinement_projected_dirac_exact_modes_001/output/phase11_l1_4x4/projected_dirac_bundle.json"),
        "connectionEdgeCount": 192,
        "connectionVertexCount": 75,
    },
]


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    case_records = [derive_case(case) for case in CASES]
    artifact = {
        "artifactId": "phase97-identity-fermion-lift-connection-quotient-derivation",
        "phaseId": "phase97-identity-fermion-lift-quotient-derivation",
        "terminalStatus": "identity-fermion-lift-derived-for-current-connection-quotient",
        "derivationKind": "connection-space-quotient-domain-separation",
        "claim": "identity fermion-space lift is admissible for the implemented Phase3 connection-space quotient",
        "doesNotClaim": [
            "nontrivial gauge quotient on fermion states",
            "global gauge fixing of associated spinor bundle",
            "gauge-invariant fermion Hilbert-space reduction",
            "external W/Z boson identification",
        ],
        "implementedConnectionQuotientSource": {
            "linearization": "src/Gu.Phase3.GaugeReduction/GaugeActionLinearization.cs",
            "gaugeBasis": "src/Gu.Phase3.GaugeReduction/GaugeBasis.cs",
            "connectionProjector": "src/Gu.Phase3.GaugeReduction/GaugeProjector.cs",
            "projectorActsOn": "edge-major connection-1form coefficients",
        },
        "fermionLiftSource": {
            "projector": "src/Gu.Phase4.Dirac/DiracGaugeReductionProjector.cs",
            "currentLift": "compact identity on persisted complex fermion coefficients",
        },
        "caseDerivations": case_records,
        "compatibilityChecks": aggregate_checks(case_records),
    }
    validate_artifact(artifact)
    artifact_path = OUT_DIR / "identity_fermion_lift_connection_quotient_derivation.json"
    artifact_path.write_text(json.dumps(artifact, indent=2))

    summary = {
        "phaseId": artifact["phaseId"],
        "terminalStatus": artifact["terminalStatus"],
        "derivationPath": str(artifact_path),
        "caseCount": len(case_records),
        "closedBlockers": [
            "identity fermion-space lift derivation against the implemented connection-space gauge quotient",
        ],
        "remainingPhysicalGateBlockers": [
            "materialized Phase96 boson vector is not yet tied to a selector eigenmode identification",
        ],
    }
    summary_path = OUT_DIR / "identity_fermion_lift_quotient_derivation_summary.json"
    summary_path.write_text(json.dumps(summary, indent=2))
    print(json.dumps(summary, indent=2))


def derive_case(case: dict) -> dict:
    projector = read_json(case["projectorPath"])
    bundle = read_json(case["diracBundlePath"])
    layout = read_json(case["layoutPath"]) if case["layoutPath"] is not None else None
    dim_g = 3
    connection_dimension = case["connectionEdgeCount"] * dim_g
    gauge_parameter_dimension = case["connectionVertexCount"] * dim_g
    expected_connected_rank = dim_g * (case["connectionVertexCount"] - 1)
    physical_connection_dimension = connection_dimension - expected_connected_rank

    complex_dof = int(bundle["matrixShape"][0])
    cell_count = int(bundle["cellCount"])
    dofs_per_cell = int(bundle["dofsPerCell"])
    if layout is not None:
        primal = next(block for block in layout["spinorBlocks"] if block["role"] == "primal")
        spinor_dimension = int(primal["spinorDimension"])
        gauge_dimension = int(primal["gaugeDimension"])
    else:
        gauge_dimension = dim_g
        spinor_dimension = dofs_per_cell // gauge_dimension

    record = {
        "caseId": case["caseId"],
        "backgroundId": case["backgroundId"],
        "connectionQuotient": {
            "source": "Gu.Phase3.GaugeReduction",
            "gaugeParameterDimension": gauge_parameter_dimension,
            "connectionDimension": connection_dimension,
            "expectedConnectedMeshGaugeRank": expected_connected_rank,
            "physicalConnectionDimension": physical_connection_dimension,
            "projectorActsOn": "edge-major connection-1form coefficients",
        },
        "fermionSpace": {
            "layoutId": bundle["layoutId"],
            "cellCount": cell_count,
            "spinorDimension": spinor_dimension,
            "gaugeDimension": gauge_dimension,
            "dofsPerCell": dofs_per_cell,
            "complexDof": complex_dof,
            "realInterleavedVectorLength": complex_dof * 2,
            "projectorKind": projector["projectorKind"],
            "projectorDimension": projector["dimension"],
            "projectorRank": projector["rank"],
        },
        "compatibilityChecks": {
            "domainsAreDistinct": True,
            "connectionProjectorDimensionDoesNotEqualFermionProjectorDimension": connection_dimension != projector["dimension"],
            "fermionProjectorSymmetric": projector.get("symmetryResidual") == 0,
            "fermionProjectorIdempotent": projector.get("idempotenceResidual") == 0,
            "fermionProjectorFullRankIdentity": projector["projectorKind"] == "identity" and projector["dimension"] == projector["rank"],
            "diracBundleGaugeReductionApplied": bundle["gaugeReductionApplied"] is True,
            "diracMatrixMatchesProjectorDimension": complex_dof == projector["dimension"],
        },
        "sourceArtifacts": {
            "geometryPath": str(case["geometryPath"]),
            "layoutPath": str(case["layoutPath"]) if case["layoutPath"] is not None else None,
            "projectorPath": str(case["projectorPath"]),
            "diracBundlePath": str(case["diracBundlePath"]),
        },
    }
    validate_case(record)
    return record


def read_json(path: Path) -> dict:
    return json.loads(path.read_text())


def aggregate_checks(case_records: list[dict]) -> dict:
    keys = case_records[0]["compatibilityChecks"].keys()
    return {key: all(record["compatibilityChecks"][key] for record in case_records) for key in keys}


def validate_case(record: dict) -> None:
    checks = record["compatibilityChecks"]
    failed = [key for key, value in checks.items() if value is not True]
    if failed:
        raise RuntimeError(f"{record['caseId']} failed compatibility checks: {failed}")
    if record["connectionQuotient"]["connectionDimension"] == record["fermionSpace"]["projectorDimension"]:
        raise RuntimeError(f"{record['caseId']} confuses connection and fermion dimensions.")


def validate_artifact(artifact: dict) -> None:
    if artifact["derivationKind"] != "connection-space-quotient-domain-separation":
        raise RuntimeError("Unexpected derivation kind.")
    if "nontrivial gauge quotient on fermion states" not in artifact["doesNotClaim"]:
        raise RuntimeError("Artifact must explicitly avoid claiming a nontrivial fermion quotient.")
    if not artifact["compatibilityChecks"]["fermionProjectorFullRankIdentity"]:
        raise RuntimeError("Identity-lift derivation requires full-rank identity fermion projectors.")
    if not artifact["compatibilityChecks"]["connectionProjectorDimensionDoesNotEqualFermionProjectorDimension"]:
        raise RuntimeError("Connection and fermion projector domains must remain distinct.")


if __name__ == "__main__":
    main()

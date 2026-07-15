"use strict";

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

function rulingIndex(map) {
  return new Map(map.rulingIds.map((r) => [r.id, r]));
}

function validateDependencyMap(map) {
  assert(map?.schemaVersion === "o4-dependency-map-v1", "unexpected dependency-map schema");
  assert(map.syntheticEvaluationOnly === true, "dependency map must be synthetic-evaluation-only");
  assert(Array.isArray(map.rulingIds) && map.rulingIds.length > 0, "rulingIds missing");
  assert(Array.isArray(map.branches) && map.branches.length > 0, "branches missing");
  const ids = new Set();
  for (const r of map.rulingIds) {
    assert(typeof r.id === "string" && r.id.length > 0 && !ids.has(r.id), `invalid/duplicate ruling id: ${r.id}`);
    ids.add(r.id);
    assert(Array.isArray(r.allowedDecisions) && r.allowedDecisions.includes(r.acceptedDecision), `accepted decision missing for ${r.id}`);
    assert(r.allowedDecisions.includes(r.mutationDecision), `mutation decision missing for ${r.id}`);
    assert(r.acceptedDecision !== r.mutationDecision, `mutation must differ for ${r.id}`);
  }
  const branchIds = new Set();
  for (const b of map.branches) {
    assert(typeof b.id === "string" && b.id.length > 0 && !branchIds.has(b.id), `invalid/duplicate branch id: ${b.id}`);
    branchIds.add(b.id);
    assert(Array.isArray(b.dependsOn) && b.dependsOn.length > 0, `branch ${b.id} has no dependencies`);
    assert(new Set(b.dependsOn).size === b.dependsOn.length, `branch ${b.id} repeats a dependency`);
    for (const id of b.dependsOn) assert(ids.has(id), `branch ${b.id} names unknown ruling ${id}`);
  }
  for (const id of ids) assert(map.branches.some((b) => b.dependsOn.includes(id)), `ruling ${id} has no dependent branch`);
  return true;
}

function acceptedRulings(map) {
  validateDependencyMap(map);
  return Object.fromEntries(map.rulingIds.map((r) => [r.id, r.acceptedDecision]));
}

function pendingRulings(map) {
  validateDependencyMap(map);
  return Object.fromEntries(map.rulingIds.map((r) => [r.id, "pending"]));
}

function validateRulingSet(map, rulings) {
  const idx = rulingIndex(map);
  assert(rulings && typeof rulings === "object" && !Array.isArray(rulings), "ruling set must be an object");
  assert(Object.keys(rulings).length === idx.size, "ruling set must be complete and contain no extras");
  for (const [id, spec] of idx) {
    assert(Object.prototype.hasOwnProperty.call(rulings, id), `missing ruling ${id}`);
    assert(spec.allowedDecisions.includes(rulings[id]), `invalid decision for ${id}: ${rulings[id]}`);
  }
  return true;
}

function isPending(decision) {
  return decision === "pending";
}

function isAdverse(id, decision) {
  if (id === "O4-P455-ZERO-MODE") return decision === "overturned";
  if (id === "O4-P455-SB-MODEL" || id === "O4-C3-WS3-MPROBE-SCOPE") return decision === "rejected";
  return decision === "overturned";
}

function phase455State(rulings) {
  const zero = rulings["O4-P455-ZERO-MODE"];
  const sb = rulings["O4-P455-SB-MODEL"];
  if (isPending(zero) || isPending(sb)) return "pending-o4";
  if (isAdverse("O4-P455-ZERO-MODE", zero) || isAdverse("O4-P455-SB-MODEL", sb)) return "invalidated-no-verdict";
  return zero === "za" ? "t1-fermionic-backreaction-null" : "t2-radiative-well-candidate";
}

function genericState(branch, rulings) {
  const pending = branch.dependsOn.filter((id) => isPending(rulings[id]));
  if (pending.length) return `pending:${pending.join(",")}`;
  const adverse = branch.dependsOn.filter((id) => isAdverse(id, rulings[id]));
  if (adverse.length) return `invalidated-by:${adverse.join(",")}`;
  return "eligible-under-accepted-conventions";
}

function evaluateBranch(branch, rulings) {
  switch (branch.model) {
    case "generic-validity":
      return genericState(branch, rulings);
    case "phase455-terminal":
      return phase455State(rulings);
    case "phase455-tstar":
      return phase455State(rulings) === "t2-radiative-well-candidate"
        ? "prospective-tstar-available"
        : `not-forwarded:${phase455State(rulings)}`;
    case "phase458-g3":
      return phase455State(rulings) === "t2-radiative-well-candidate"
        ? "true-via-phase455-t2"
        : `false-or-unavailable:${phase455State(rulings)}`;
    case "phase471-l5": {
      const state = phase455State(rulings);
      return state.startsWith("t1-") ? "closed-negative" : state.startsWith("t2-") ? "closed-with-positive-inclusion" : "open";
    }
    case "ws3-conjunct": {
      const d = rulings["O4-C3-WS3-MPROBE-SCOPE"];
      return d === "accepted" ? "true" : d === "pending" ? "pending" : "false-rejected";
    }
    case "ws3-firewall": {
      const d = rulings["O4-C3-WS3-MPROBE-SCOPE"];
      return d === "accepted" ? "open-schema-pin-plus-ruling-only" : "closed";
    }
    case "phase471-l7":
      return rulings["O4-C3-WS3-MPROBE-SCOPE"] === "accepted" ? "withheld-awaiting-measurement" : "withheld-o4";
    case "all-rulings-resolved":
      return genericState(branch, rulings) === "eligible-under-accepted-conventions" ? "resolved" : genericState(branch, rulings);
    default:
      throw new Error(`unknown branch model: ${branch.model}`);
  }
}

function evaluateShadow(map, rulings) {
  validateDependencyMap(map);
  validateRulingSet(map, rulings);
  return Object.fromEntries(map.branches.map((b) => [b.id, evaluateBranch(b, rulings)]));
}

function nonempty(value) {
  return typeof value === "string" && value.trim().length > 0;
}

function sha256(value) {
  return typeof value === "string" && /^[0-9a-f]{64}$/.test(value);
}

function validateProductionRulingArray(map, rulings) {
  assert(Array.isArray(rulings) && rulings.length === map.rulingIds.length, "rulings must be the exact mandatory array");
  const expected = new Set(map.rulingIds.map((r) => r.id));
  const seen = new Set();
  for (const ruling of rulings) {
    assert(ruling && typeof ruling === "object" && !Array.isArray(ruling), "ruling entry must be an object");
    assert(expected.has(ruling.rulingId) && !seen.has(ruling.rulingId), `unknown/duplicate rulingId: ${ruling.rulingId}`);
    seen.add(ruling.rulingId);
    assert(["accept-at-declared-scope", "accept-with-registered-caveats", "reject", "insufficient-basis", "not-applicable"].includes(ruling.disposition), `invalid disposition for ${ruling.rulingId}`);
    assert(nonempty(ruling.selectedOption), `missing selectedOption for ${ruling.rulingId}`);
    assert(Array.isArray(ruling.registeredCaveatIds), `missing registeredCaveatIds for ${ruling.rulingId}`);
    assert(nonempty(ruling.rationale), `missing rationale for ${ruling.rulingId}`);
    assert(Array.isArray(ruling.reviewedArtifactSha256) && ruling.reviewedArtifactSha256.length > 0 && ruling.reviewedArtifactSha256.every(sha256), `invalid reviewedArtifactSha256 for ${ruling.rulingId}`);
  }
  assert(seen.size === expected.size, "rulingId set is incomplete");
}

function validateRealRulingEnvelope(map, envelope) {
  try {
    validateDependencyMap(map);
    if (!envelope || typeof envelope !== "object" || Array.isArray(envelope)) return { valid: false, reason: "not-object" };
    if (envelope.synthetic === true || envelope.testOnly === true) return { valid: false, reason: "synthetic-or-test-only" };
    assert(envelope.schemaId === "o4-human-physicist-memo-v1" && envelope.schemaVersion === 1, "schema identity mismatch");
    assert(envelope.templateOnly === false && envelope.documentKind === "final-physicist-ruling", "not a final ruling");
    assert(/^o4memo-[0-9]{8}-[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/.test(envelope.memoId), "invalid memoId");
    assert(nonempty(envelope.issuedAt) && Number.isFinite(Date.parse(envelope.issuedAt)), "invalid issuedAt");
    const reviewer = envelope.reviewer;
    assert(reviewer && nonempty(reviewer.reviewerRegistryId) && nonempty(reviewer.fullName), "missing reviewer identity");
    assert(["theoretical-physicist", "lattice-field-theorist", "mathematical-physicist", "other-qualified-physicist"].includes(reviewer.professionalRole), "invalid reviewer role");
    assert(Array.isArray(reviewer.qualifications) && reviewer.qualifications.length > 0 && reviewer.qualifications.every(nonempty), "missing reviewer qualifications");
    const binding = envelope.repositoryBinding;
    assert(binding && /^[0-9a-f]{40}$/.test(binding.commitSha), "invalid repository commit binding");
    assert(binding.dossierPath === "docs/Phases/Adjudication/O4_CONVENTIONS_REGISTER.md", "invalid dossier path");
    assert(binding.coverageManifestPath === "scripts/o4_register/o4_exact_coverage_manifest_v1.json", "invalid coverage manifest path");
    for (const key of ["dossierSha256", "coverageManifestSha256", "schemaSha256", "reviewerRegistryRecordSha256"]) assert(sha256(binding[key]), `invalid ${key}`);
    const attest = envelope.authorshipAttestations;
    assert(attest?.humanAuthoredRulings === true && attest.machineGeneratedRulings === false && attest.independentProfessionalJudgment === true, "invalid human-authorship attestations");
    assert(attest.riskAcceptanceIsNotPhysicistRuling === true && attest.noPhysicalMassOrGevClaim === true, "invalid scope attestations");
    validateProductionRulingArray(map, envelope.rulings);
    assert(typeof envelope.globalCaveats === "string", "missing globalCaveats");
    assert(sha256(envelope.signedPayloadSha256), "invalid signedPayloadSha256");
    const signature = envelope.signatureEnvelope;
    assert(signature && ["openpgp-detached", "ed25519-detached", "minisign-detached", "witnessed-signed-document"].includes(signature.mode), "invalid signature mode");
    assert(nonempty(signature.signerRegistryId) && nonempty(signature.signatureArtifactPath) && sha256(signature.signatureArtifactSha256), "invalid signature envelope fields");
    if (signature.mode === "witnessed-signed-document") {
      assert(nonempty(signature.witnessRegistryId) && sha256(signature.witnessAttestationSha256), "invalid witnessed signature envelope");
    } else {
      assert(nonempty(signature.publicKeyFingerprint) && nonempty(signature.signatureValue), "invalid detached signature envelope");
    }
    return { valid: true, reason: "schema-envelope-shape-valid-signature-unverified", cryptographicVerificationPerformed: false };
  } catch (error) {
    return { valid: false, reason: `schema-envelope-invalid:${error.message}`, cryptographicVerificationPerformed: false };
  }
}

module.exports = {
  acceptedRulings,
  evaluateShadow,
  pendingRulings,
  validateDependencyMap,
  validateRealRulingEnvelope,
  validateRulingSet,
};

#!/usr/bin/env node
"use strict";

const crypto = require("crypto");
const fs = require("fs");
const path = require("path");
const { execFileSync } = require("child_process");

const ROOT = path.resolve(__dirname, "..", "..");
const CONTRACT_PATH = path.join(__dirname, "preregistration", "o4_intake_contract_v1.json");

function fail(message) { throw new Error(message); }
function assert(ok, message) { if (!ok) fail(message); }
function sha256(bytes) { return crypto.createHash("sha256").update(bytes).digest("hex"); }
function exactKeys(object, allowed, label) {
  assert(object && typeof object === "object" && !Array.isArray(object), `${label}:object-required`);
  const actual = Object.keys(object).sort();
  const expected = [...allowed].sort();
  assert(JSON.stringify(actual) === JSON.stringify(expected), `${label}:property-set-invalid`);
}
function nonempty(value, label) { assert(typeof value === "string" && value.trim().length > 0, `${label}:nonempty-string-required`); }
function hex(value, count, label) { assert(typeof value === "string" && new RegExp(`^[0-9a-f]{${count}}$`).test(value), `${label}:invalid-hex`); }

// Strict JSON parser: JSON.parse silently accepts duplicate object keys, which
// is unsuitable for signed input. This parser rejects them before returning.
function parseStrict(text) {
  let i = 0;
  const ws = () => { while (/\s/.test(text[i] || "")) i++; };
  function string() {
    assert(text[i] === '"', "json:string-expected");
    const start = i++;
    while (i < text.length) {
      if (text[i] === '"') { i++; return JSON.parse(text.slice(start, i)); }
      if (text[i] === "\\") { i += 2; continue; }
      assert(text.charCodeAt(i) >= 0x20, "json:unescaped-control");
      i++;
    }
    fail("json:unterminated-string");
  }
  function value() {
    ws();
    if (text[i] === "{") {
      i++; const out = {}; const seen = new Set(); ws();
      if (text[i] === "}") { i++; return out; }
      for (;;) {
        ws(); const key = string(); assert(!seen.has(key), `json:duplicate-key:${key}`); seen.add(key);
        ws(); assert(text[i++] === ":", "json:colon-expected"); out[key] = value(); ws();
        if (text[i] === "}") { i++; return out; }
        assert(text[i++] === ",", "json:comma-expected");
      }
    }
    if (text[i] === "[") {
      i++; const out = []; ws(); if (text[i] === "]") { i++; return out; }
      for (;;) { out.push(value()); ws(); if (text[i] === "]") { i++; return out; } assert(text[i++] === ",", "json:comma-expected"); }
    }
    if (text[i] === '"') return string();
    for (const [token, result] of [["true", true], ["false", false], ["null", null]])
      if (text.startsWith(token, i)) { i += token.length; return result; }
    const match = text.slice(i).match(/^-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?/);
    assert(match, "json:value-expected"); i += match[0].length;
    const number = Number(match[0]); assert(Number.isFinite(number), "json:nonfinite-number"); return number;
  }
  const result = value(); ws(); assert(i === text.length, "json:trailing-content"); return result;
}

function validateUnicode(value) {
  for (let i = 0; i < value.length; i++) {
    const c = value.charCodeAt(i);
    if (c >= 0xd800 && c <= 0xdbff) { assert(i + 1 < value.length && value.charCodeAt(++i) >= 0xdc00 && value.charCodeAt(i) <= 0xdfff, "jcs:unpaired-surrogate"); }
    else assert(c < 0xdc00 || c > 0xdfff, "jcs:unpaired-surrogate");
  }
}
function canonicalize(value) {
  if (value === null || typeof value === "boolean") return JSON.stringify(value);
  if (typeof value === "number") { assert(Number.isFinite(value), "jcs:nonfinite-number"); return JSON.stringify(value); }
  if (typeof value === "string") { validateUnicode(value); return JSON.stringify(value); }
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  assert(value && typeof value === "object", "jcs:unsupported-value");
  return `{${Object.keys(value).sort().map((key) => { validateUnicode(key); return `${JSON.stringify(key)}:${canonicalize(value[key])}`; }).join(",")}}`;
}

const OPTIONS = {
  "O4-F1-INVARIANT-RAYS": ["translation-invariant-rays-valid-at-declared-constant-field-scope", "random-rays-valid-at-declared-diagnostic-scope", "both-declared-scopes", "neither-admissible", "defer"],
  "O4-F1-COLLECTIVE-COORDINATE": ["gauge-invariant-ray-coordinate-admissible", "diagnostic-only", "inadmissible", "defer"],
  "O4-F1-FP-NORMALIZATION": ["within-member-only-caveat-sufficient", "cross-member-comparisons-forbidden", "specified-normalization-required", "defer"],
  "O4-F2-POSITIVE-MODE-IR": ["positive-nonzero-subspace-diagnostic-only", "positive-nonzero-subspace-verdict-admissible", "absolute-nonzero-prescription-required", "contour-prescription-required", "none-admissible", "defer"],
  "O4-F3-THETA-HAAR": ["unit-quaternion-haar-axis-angle-boundary-admissible", "inadmissible", "defer"],
  "O4-F4-SADDLE-BACKGROUNDS": ["local-diagnostic-only", "projected-loop-calculation-admissible-at-declared-scope", "inadmissible", "defer"],
  "O4-E1-P447-SOFT-FLOOR": ["default-1e-4-plus-sweep-diagnostic-only", "alternative-floor-required", "no-floor-prescription-admissible", "defer"],
  "O4-E2-P453-UNIFORM-LADDER": ["uniform-stiffness-required", "mixed-stiffness-allowed-only-with-junction-covariance", "either-at-declared-error-model", "inadmissible", "defer"],
  "O4-P455-ZERO-MODE": ["Za-symmetric-k0-exclusion", "Zb-soft-floor-k0", "Zc-exact-zero-only", "none-admissible", "defer"],
  "O4-P455-SB-MODEL": ["accept-w430-as-phase455-workbench-only", "diagnostic-only", "reject-w430-model", "defer"],
  "O4-C1-COMPACT-REAL-FORM": ["phase467-direct-so64-arm-discharges-finite-scope", "additional-human-ruling-required", "reject-transfer", "defer"],
  "O4-C2-YHALF-BOOKKEEPING": ["accept-phase404-lepton-doublet-normalization-as-bookkeeping-only", "reject-bookkeeping", "defer"],
  "O4-C3-WS3-MPROBE-SCOPE": ["accept-labeled-probe-only-no-lineage", "reject-probe-scope", "defer"],
};
const CAVEATS = new Set(["DECLARED-SCOPE-ONLY", "DIAGNOSTIC-ONLY", "NO-CROSS-MEMBER-COMPARISON", "NO-RETIRED-OBJECT-REINSTATEMENT", "NO-SOURCE-LINEAGE", "NO-PHYSICAL-NORMALIZATION", "NO-STAGE-B-AUTHORIZATION"]);
const DISPOSITIONS = new Set(["accept-at-declared-scope", "accept-with-registered-caveats", "reject", "insufficient-basis", "not-applicable"]);
const NEGATIVE_OPTIONS = new Set(["neither-admissible", "inadmissible", "none-admissible", "reject-w430-model", "reject-transfer", "reject-bookkeeping", "reject-probe-scope"]);

function validateMemoShape(memo) {
  exactKeys(memo, ["schemaId", "schemaVersion", "templateOnly", "documentKind", "memoId", "issuedAt", "reviewer", "repositoryBinding", "authorshipAttestations", "rulings", "globalCaveats", "signedPayloadSha256", "signatureEnvelope"], "memo");
  assert(memo.schemaId === "o4-human-physicist-memo-v1" && memo.schemaVersion === 1, "memo:schema-identity");
  assert(memo.templateOnly === false && memo.documentKind === "final-physicist-ruling", "memo:not-final");
  assert(/^o4memo-[0-9]{8}-[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/.test(memo.memoId), "memo:id-invalid");
  assert(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z$/.test(memo.issuedAt) && Number.isFinite(Date.parse(memo.issuedAt)), "memo:issued-at-invalid");
  assert(Date.parse(memo.issuedAt) <= Date.now() + 300000, "memo:issued-in-future");
  assert(memo.memoId.slice(7, 15) === memo.issuedAt.slice(0, 10).replace(/-/g, ""), "memo:id-date-mismatch");
  exactKeys(memo.reviewer, ["reviewerRegistryId", "fullName", "professionalRole", "institution", "qualifications", "persistentIdentifier"], "reviewer");
  nonempty(memo.reviewer.reviewerRegistryId, "reviewer:id"); nonempty(memo.reviewer.fullName, "reviewer:name");
  assert(["theoretical-physicist", "lattice-field-theorist", "mathematical-physicist", "other-qualified-physicist"].includes(memo.reviewer.professionalRole), "reviewer:role");
  assert(memo.reviewer.institution === null || (typeof memo.reviewer.institution === "string" && memo.reviewer.institution.trim()), "reviewer:institution");
  assert(Array.isArray(memo.reviewer.qualifications) && memo.reviewer.qualifications.length > 0 && memo.reviewer.qualifications.every((x) => typeof x === "string" && x.trim()), "reviewer:qualifications");
  assert(memo.reviewer.persistentIdentifier === null || (typeof memo.reviewer.persistentIdentifier === "string" && memo.reviewer.persistentIdentifier.trim()), "reviewer:persistent-id");
  exactKeys(memo.repositoryBinding, ["commitSha", "dossierPath", "dossierSha256", "coverageManifestPath", "coverageManifestSha256", "schemaSha256", "reviewerRegistryRecordSha256"], "binding");
  hex(memo.repositoryBinding.commitSha, 40, "binding:commit");
  for (const key of ["dossierSha256", "coverageManifestSha256", "schemaSha256", "reviewerRegistryRecordSha256"]) hex(memo.repositoryBinding[key], 64, `binding:${key}`);
  exactKeys(memo.authorshipAttestations, ["humanAuthoredRulings", "machineGeneratedRulings", "independentProfessionalJudgment", "riskAcceptanceIsNotPhysicistRuling", "noPhysicalMassOrGevClaim"], "attestations");
  assert(memo.authorshipAttestations.humanAuthoredRulings === true && memo.authorshipAttestations.machineGeneratedRulings === false && memo.authorshipAttestations.independentProfessionalJudgment === true && memo.authorshipAttestations.riskAcceptanceIsNotPhysicistRuling === true && memo.authorshipAttestations.noPhysicalMassOrGevClaim === true, "attestations:invalid");
  assert(Array.isArray(memo.rulings) && memo.rulings.length === 13, "rulings:exact-count");
  const seen = new Set();
  for (const ruling of memo.rulings) {
    const allowedKeys = ["rulingId", "disposition", "selectedOption", "registeredCaveatIds", "rationale", "reviewedArtifactSha256"];
    if (Object.prototype.hasOwnProperty.call(ruling, "scopeAssertions")) allowedKeys.push("scopeAssertions");
    exactKeys(ruling, allowedKeys, "ruling");
    assert(OPTIONS[ruling.rulingId] && !seen.has(ruling.rulingId), "ruling:id-unknown-or-duplicate"); seen.add(ruling.rulingId);
    assert(DISPOSITIONS.has(ruling.disposition) && OPTIONS[ruling.rulingId].includes(ruling.selectedOption), `ruling:${ruling.rulingId}:choice-invalid`);
    nonempty(ruling.rationale, `ruling:${ruling.rulingId}:rationale`);
    assert(Array.isArray(ruling.registeredCaveatIds) && new Set(ruling.registeredCaveatIds).size === ruling.registeredCaveatIds.length && ruling.registeredCaveatIds.every((x) => CAVEATS.has(x)), `ruling:${ruling.rulingId}:caveats`);
    assert(ruling.disposition !== "accept-with-registered-caveats" || ruling.registeredCaveatIds.length > 0, `ruling:${ruling.rulingId}:empty-accepted-caveats`);
    assert(Array.isArray(ruling.reviewedArtifactSha256) && ruling.reviewedArtifactSha256.length > 0 && new Set(ruling.reviewedArtifactSha256).size === ruling.reviewedArtifactSha256.length, `ruling:${ruling.rulingId}:reviewed-hashes`);
    ruling.reviewedArtifactSha256.forEach((x) => hex(x, 64, `ruling:${ruling.rulingId}:reviewed-hash`));
    assert((ruling.selectedOption === "defer") === (ruling.disposition === "insufficient-basis"), `ruling:${ruling.rulingId}:defer-disposition-mismatch`);
    assert(ruling.disposition !== "not-applicable", `ruling:${ruling.rulingId}:not-applicable-unmapped`);
    assert(NEGATIVE_OPTIONS.has(ruling.selectedOption) === (ruling.disposition === "reject"), `ruling:${ruling.rulingId}:negative-disposition-mismatch`);
    if (Object.prototype.hasOwnProperty.call(ruling, "scopeAssertions")) {
      exactKeys(ruling.scopeAssertions, ["mIsSourceDefined", "singletGapOnly", "noNamedScalarIdentification", "noLineageFill", "latticeUnitsOnly"], "scope");
      assert(ruling.scopeAssertions.mIsSourceDefined === false && ruling.scopeAssertions.singletGapOnly === true && ruling.scopeAssertions.noNamedScalarIdentification === true && ruling.scopeAssertions.noLineageFill === true && ruling.scopeAssertions.latticeUnitsOnly === true, "scope:invalid");
    }
    if (ruling.rulingId === "O4-C3-WS3-MPROBE-SCOPE" && ruling.selectedOption === "accept-labeled-probe-only-no-lineage") {
      assert(ruling.scopeAssertions !== undefined, "c3-scope:required");
    }
  }
  assert(seen.size === Object.keys(OPTIONS).length, "rulings:id-set-incomplete");
  assert(typeof memo.globalCaveats === "string", "memo:global-caveats");
  hex(memo.signedPayloadSha256, 64, "memo:signed-payload");
  exactKeys(memo.signatureEnvelope, ["mode", "signerRegistryId", "publicKeyFingerprint", "signatureArtifactPath", "signatureArtifactSha256", "signatureValue"], "signature");
  assert(memo.signatureEnvelope.mode === "ed25519-detached", "signature:unsupported-mode");
  nonempty(memo.signatureEnvelope.signerRegistryId, "signature:signer"); nonempty(memo.signatureEnvelope.publicKeyFingerprint, "signature:fingerprint"); nonempty(memo.signatureEnvelope.signatureArtifactPath, "signature:path"); nonempty(memo.signatureEnvelope.signatureValue, "signature:value");
  hex(memo.signatureEnvelope.signatureArtifactSha256, 64, "signature:artifact-hash");
}

function gitBytes(commit, relativePath) { return execFileSync("git", ["show", `${commit}:${relativePath}`], { cwd: ROOT, maxBuffer: 20 * 1024 * 1024 }); }
function currentBytes(relativePath) { return fs.readFileSync(path.join(ROOT, relativePath)); }
function safeInputPath(relativePath, prefix) {
  assert(typeof relativePath === "string" && relativePath.startsWith(prefix) && !relativePath.includes("\\") && !path.posix.isAbsolute(relativePath), "signature:path-outside-prefix");
  assert(!relativePath.split("/").includes(".."), "signature:path-traversal");
  const full = path.resolve(ROOT, relativePath); assert(full.startsWith(path.resolve(ROOT, prefix) + path.sep), "signature:path-escape");
  assert((fs.lstatSync(full).mode & fs.constants.S_IFMT) === fs.constants.S_IFREG, "signature:not-regular-file"); return full;
}

function verify(repoRoot = ROOT) {
  const contract = parseStrict(fs.readFileSync(CONTRACT_PATH, "utf8"));
  const memoPath = path.join(repoRoot, contract.memoInputPath);
  if (!fs.existsSync(memoPath)) return { valid: false, inputPresent: false, terminal: contract.missingOrInvalidTerminal, reasons: ["memo-input-absent"] };
  const reasons = [];
  try {
    assert((fs.lstatSync(memoPath).mode & fs.constants.S_IFMT) === fs.constants.S_IFREG, "memo:not-regular-file");
    const memoBytes = fs.readFileSync(memoPath); assert(memoBytes.length <= 1024 * 1024, "memo:too-large");
    const memo = parseStrict(memoBytes.toString("utf8")); validateMemoShape(memo);
    const binding = memo.repositoryBinding;
    assert(binding.dossierPath === contract.dossierPath && binding.coverageManifestPath === contract.coverageContractPath, "binding:path-mismatch");
    execFileSync("git", ["cat-file", "-e", `${binding.commitSha}^{commit}`], { cwd: repoRoot });
    execFileSync("git", ["merge-base", "--is-ancestor", binding.commitSha, "HEAD"], { cwd: repoRoot });
    const bound = {
      dossier: gitBytes(binding.commitSha, contract.dossierPath),
      coverage: gitBytes(binding.commitSha, contract.coverageContractPath),
      schema: gitBytes(binding.commitSha, contract.memoSchemaPath),
      registry: gitBytes(binding.commitSha, contract.reviewerRegistryPath),
      dependency: gitBytes(binding.commitSha, contract.dependencyMapPath),
      intakeContract: gitBytes(binding.commitSha, path.relative(ROOT, CONTRACT_PATH).replace(/\\/g, "/")),
      verifier: gitBytes(binding.commitSha, "studies/phase480_o4_physicist_adjudication_intake_001/verify_intake.js"),
    };
    assert(sha256(bound.dossier) === binding.dossierSha256 && sha256(bound.coverage) === binding.coverageManifestSha256 && sha256(bound.schema) === binding.schemaSha256, "binding:artifact-hash-mismatch");
    for (const [name, relativePath] of [["dossier", contract.dossierPath], ["coverage", contract.coverageContractPath], ["schema", contract.memoSchemaPath], ["registry", contract.reviewerRegistryPath], ["dependency", contract.dependencyMapPath], ["intakeContract", path.relative(ROOT, CONTRACT_PATH).replace(/\\/g, "/")], ["verifier", "studies/phase480_o4_physicist_adjudication_intake_001/verify_intake.js"]])
      assert(sha256(bound[name]) === sha256(currentBytes(relativePath)), `binding:${name}-drift-since-commit`);
    const registry = parseStrict(bound.registry.toString("utf8"));
    exactKeys(registry, ["schemaVersion", "registryId", "purpose", "records"], "registry"); assert(registry.schemaVersion === 1 && Array.isArray(registry.records), "registry:invalid");
    const records = registry.records.filter((x) => x.reviewerRegistryId === memo.reviewer.reviewerRegistryId); assert(records.length === 1, "registry:reviewer-not-unique");
    const record = records[0];
    exactKeys(record, ["reviewerRegistryId", "fullName", "professionalRole", "institution", "qualifications", "persistentIdentifier", "active", "humanReviewerAttested", "publicKeyAlgorithm", "publicKeySpkiPem", "publicKeyFingerprintSha256"], "registry-record");
    assert(record.active === true && record.humanReviewerAttested === true && record.publicKeyAlgorithm === "Ed25519", "registry:reviewer-not-active-human-ed25519");
    for (const key of ["reviewerRegistryId", "fullName", "professionalRole", "institution", "persistentIdentifier"]) assert(JSON.stringify(record[key]) === JSON.stringify(memo.reviewer[key]), `registry:reviewer-${key}-mismatch`);
    assert(JSON.stringify(record.qualifications) === JSON.stringify(memo.reviewer.qualifications), "registry:reviewer-qualifications-mismatch");
    assert(sha256(Buffer.from(canonicalize(record))) === binding.reviewerRegistryRecordSha256, "registry:record-hash-mismatch");
    const publicKey = crypto.createPublicKey(record.publicKeySpkiPem); assert(publicKey.asymmetricKeyType === "ed25519", "registry:key-not-ed25519");
    const fingerprint = sha256(publicKey.export({ type: "spki", format: "der" }));
    assert(fingerprint === record.publicKeyFingerprintSha256 && fingerprint === memo.signatureEnvelope.publicKeyFingerprint, "signature:fingerprint-mismatch");
    assert(memo.signatureEnvelope.signerRegistryId === record.reviewerRegistryId, "signature:signer-reviewer-mismatch");
    const expectedSignaturePath = `${contract.signatureArtifactPathPrefix}${memo.memoId}.ed25519.sig`;
    assert(memo.signatureEnvelope.signatureArtifactPath === expectedSignaturePath, "signature:unexpected-artifact-path");
    const signatureBytes = fs.readFileSync(safeInputPath(expectedSignaturePath, contract.signatureArtifactPathPrefix));
    assert(signatureBytes.length === 64 && sha256(signatureBytes) === memo.signatureEnvelope.signatureArtifactSha256, "signature:artifact-invalid");
    const embeddedSignature = Buffer.from(memo.signatureEnvelope.signatureValue, "base64"); assert(embeddedSignature.length === 64 && embeddedSignature.toString("base64") === memo.signatureEnvelope.signatureValue && embeddedSignature.equals(signatureBytes), "signature:embedded-artifact-mismatch");
    const payload = { ...memo }; delete payload.signedPayloadSha256; delete payload.signatureEnvelope;
    const canonicalPayload = Buffer.from(canonicalize(payload), "utf8");
    assert(sha256(canonicalPayload) === memo.signedPayloadSha256, "signature:payload-hash-mismatch");
    assert(crypto.verify(null, canonicalPayload, publicKey, signatureBytes), "signature:cryptographic-verification-failed");
    const coverage = parseStrict(bound.coverage.toString("utf8"));
    for (const ruling of memo.rulings) {
      const required = coverage.entries.filter((entry) => entry.reviewItems.includes(ruling.rulingId)).map((entry) => sha256(gitBytes(binding.commitSha, entry.summaryPath))).sort();
      assert(required.length > 0 && JSON.stringify([...ruling.reviewedArtifactSha256].sort()) === JSON.stringify(required), `ruling:${ruling.rulingId}:reviewed-artifact-set-mismatch`);
    }
    return {
      valid: true,
      inputPresent: true,
      terminal: contract.validatedTerminal,
      reasons: [],
      memoId: memo.memoId,
      memoSha256: sha256(memoBytes),
      signedPayloadSha256: memo.signedPayloadSha256,
      reviewerRegistryId: record.reviewerRegistryId,
      rulingCount: memo.rulings.length,
      externalMemoValidated: true,
      humanAuthorshipValidated: true,
      repositoryBindingValidated: true,
      signedPayloadHashValidated: true,
      signatureProvenanceValidated: true,
      signatureVerificationStatus: "cryptographically-verified",
      memoSchemaSha256: binding.schemaSha256,
      syntheticOrTemplateInput: false,
      rulingContentMachineAuthoredOrInferred: false,
      normalizedRulings: memo.rulings,
      cryptographicVerificationPerformed: true,
      repositoryBindingVerified: true,
      reviewedArtifactSetsVerified: true,
    };
  } catch (error) { reasons.push(String(error.message || error)); }
  return { valid: false, inputPresent: true, terminal: contract.missingOrInvalidTerminal, reasons };
}

if (require.main === module) process.stdout.write(JSON.stringify(verify()) + "\n");
module.exports = { canonicalize, parseStrict, validateMemoShape, verify };

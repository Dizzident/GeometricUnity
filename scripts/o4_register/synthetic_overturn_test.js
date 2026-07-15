#!/usr/bin/env node
"use strict";

const crypto = require("crypto");
const fs = require("fs");
const path = require("path");
const {
  acceptedRulings,
  evaluateShadow,
  pendingRulings,
  validateDependencyMap,
  validateRealRulingEnvelope,
} = require("./rulings.js");

const repoRoot = path.resolve(__dirname, "..", "..");
const mapPath = path.join(__dirname, "dependency_map.json");
const schemaPath = path.join(__dirname, "o4_human_memo_schema_v1.json");
const templatePath = path.join(__dirname, "o4_human_memo_template_v1.json");

function sha256(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function artifactSnapshot(map) {
  return Object.fromEntries(map.canonicalArtifactPaths.map((rel) => {
    const file = path.join(repoRoot, rel);
    if (!fs.existsSync(file)) throw new Error(`canonical artifact missing: ${rel}`);
    return [rel, sha256(fs.readFileSync(file))];
  }));
}

function candidateSnapshot(map) {
  return Object.fromEntries(map.candidateRulingPaths.map((rel) => {
    const file = path.join(repoRoot, rel);
    return [rel, fs.existsSync(file) ? { exists: true, sha256: sha256(fs.readFileSync(file)) } : { exists: false, sha256: null }];
  }));
}

function stable(value) {
  if (Array.isArray(value)) return `[${value.map(stable).join(",")}]`;
  if (value && typeof value === "object") return `{${Object.keys(value).sort().map((k) => `${JSON.stringify(k)}:${stable(value[k])}`).join(",")}}`;
  return JSON.stringify(value);
}

function diffCount(before, after) {
  const keys = new Set([...Object.keys(before), ...Object.keys(after)]);
  return [...keys].filter((key) => stable(before[key]) !== stable(after[key])).length;
}

function exactIdSetEqual(...idLists) {
  const normalized = idLists.map((ids) => [...ids].sort());
  return normalized.slice(1).every((ids) => stable(ids) === stable(normalized[0]));
}

function productionShapeFixture(map) {
  const zero = "0".repeat(64);
  return {
    schemaId: "o4-human-physicist-memo-v1",
    schemaVersion: 1,
    templateOnly: false,
    documentKind: "final-physicist-ruling",
    memoId: "o4memo-20260715-12345678-1234-4123-8123-123456789abc",
    issuedAt: "2026-07-15T12:00:00Z",
    reviewer: {
      reviewerRegistryId: "shape-fixture-reviewer",
      fullName: "Shape Fixture Only",
      professionalRole: "other-qualified-physicist",
      institution: null,
      qualifications: ["shape fixture only"],
      persistentIdentifier: null,
    },
    repositoryBinding: {
      commitSha: "0".repeat(40),
      dossierPath: "docs/Phases/Adjudication/O4_CONVENTIONS_REGISTER.md",
      dossierSha256: zero,
      coverageManifestPath: "scripts/o4_register/o4_exact_coverage_manifest_v1.json",
      coverageManifestSha256: zero,
      schemaSha256: zero,
      reviewerRegistryRecordSha256: zero,
    },
    authorshipAttestations: {
      humanAuthoredRulings: true,
      machineGeneratedRulings: false,
      independentProfessionalJudgment: true,
      riskAcceptanceIsNotPhysicistRuling: true,
      noPhysicalMassOrGevClaim: true,
    },
    rulings: map.rulingIds.map(({ id }) => ({
      rulingId: id,
      disposition: "insufficient-basis",
      selectedOption: "defer",
      registeredCaveatIds: [],
      rationale: "Shape fixture only; not a physicist ruling.",
      reviewedArtifactSha256: [zero],
    })),
    globalCaveats: "Shape fixture only; no scientific or identity verification.",
    signedPayloadSha256: zero,
    signatureEnvelope: {
      mode: "witnessed-signed-document",
      signerRegistryId: "shape-fixture-reviewer",
      signatureArtifactPath: "shape-fixture/not-a-signature.pdf",
      signatureArtifactSha256: zero,
      witnessRegistryId: "shape-fixture-witness",
      witnessAttestationSha256: zero,
    },
  };
}

function run() {
  const mapBytes = fs.readFileSync(mapPath);
  const map = JSON.parse(mapBytes.toString("utf8"));
  const schema = JSON.parse(fs.readFileSync(schemaPath, "utf8"));
  const template = JSON.parse(fs.readFileSync(templatePath, "utf8"));
  validateDependencyMap(map);
  const mapRulingIds = map.rulingIds.map((r) => r.id);
  const schemaRulingIds = schema.$defs.ruling.properties.rulingId.enum;
  const schemaMandatoryRulingIds = schema.properties.rulings.allOf.map(
    (requirement) => requirement.contains.properties.rulingId.const,
  );
  const templateRulingIds = template.rulings.map((r) => r.rulingId);
  const schemaTemplateMapRulingIdsEqual = exactIdSetEqual(
    mapRulingIds,
    schemaRulingIds,
    schemaMandatoryRulingIds,
    templateRulingIds,
  );
  if (!schemaTemplateMapRulingIdsEqual) throw new Error("dependency-map/schema/template rulingId sets differ");
  const artifactsBefore = artifactSnapshot(map);
  const candidatesBefore = candidateSnapshot(map);

  const baselineRulings = acceptedRulings(map);
  const baseline = evaluateShadow(map, baselineRulings);
  const edgeCases = [];
  let unintendedBranchMutationCount = 0;
  for (const ruling of map.rulingIds) {
    const mutatedRulings = { ...baselineRulings, [ruling.id]: ruling.mutationDecision };
    const mutated = evaluateShadow(map, mutatedRulings);
    for (const branch of map.branches) {
      const changed = baseline[branch.id] !== mutated[branch.id];
      const declared = branch.dependsOn.includes(ruling.id);
      if (changed && !declared) unintendedBranchMutationCount++;
      if (declared) edgeCases.push({
        rulingId: ruling.id,
        branchId: branch.id,
        before: baseline[branch.id],
        after: mutated[branch.id],
        passed: changed,
      });
    }
  }

  const za = evaluateShadow(map, baselineRulings);
  const zb = evaluateShadow(map, { ...baselineRulings, "O4-P455-ZERO-MODE": "zb" });
  const pending = evaluateShadow(map, pendingRulings(map));
  const ws3Rejected = evaluateShadow(map, { ...baselineRulings, "O4-C3-WS3-MPROBE-SCOPE": "rejected" });

  const syntheticEnvelope = {
    synthetic: true,
    testOnly: true,
    physicistIdentity: "SYNTHETIC-NOT-A-PHYSICIST",
    date: "2026-07-15",
    scope: "synthetic overturn battery only",
    qualificationsOrRole: "test fixture",
    caveats: ["not a real ruling"],
    signatureProvenance: "synthetic-none",
    rulings: baselineRulings,
  };
  const legacyMinimalEnvelope = { wsThreeMProbeScopeSignedOff: true, signer: "SYNTHETIC" };
  const syntheticValidation = validateRealRulingEnvelope(map, syntheticEnvelope);
  const legacyValidation = validateRealRulingEnvelope(map, legacyMinimalEnvelope);
  const templateValidation = validateRealRulingEnvelope(map, template);
  const productionShapeValidation = validateRealRulingEnvelope(map, productionShapeFixture(map));

  const artifactsAfter = artifactSnapshot(map);
  const candidatesAfter = candidateSnapshot(map);
  const failedEdgeCases = edgeCases.filter((c) => !c.passed);
  const canonicalArtifactMutationCount = diffCount(artifactsBefore, artifactsAfter);
  const candidatePathMutationCount = diffCount(candidatesBefore, candidatesAfter);
  const expectedEdgeCount = map.branches.reduce((sum, b) => sum + b.dependsOn.length, 0);

  const semanticCases = {
    pendingKeepsPhase458G4Unresolved: pending["phase458-g4"].startsWith("pending:"),
    zaProducesPhase455T1: za["phase455-terminal"] === "t1-fermionic-backreaction-null",
    zaDoesNotMotivateG3: za["phase458-g3"].startsWith("false-or-unavailable:"),
    zbProducesPhase455T2: zb["phase455-terminal"] === "t2-radiative-well-candidate",
    zbMotivatesG3: zb["phase458-g3"] === "true-via-phase455-t2",
    zbClosesL5WithPositiveInclusion: zb["phase471-l5"] === "closed-with-positive-inclusion",
    acceptedMProbeOpensOnlyRulingConjunction: baseline["phase457-firewall"] === "open-schema-pin-plus-ruling-only",
    rejectedMProbeClosesFirewall: ws3Rejected["phase457-firewall"] === "closed",
    acceptedMProbeDoesNotCloseL7: baseline["phase471-l7"] === "withheld-awaiting-measurement",
    rejectedMProbeKeepsL7Withheld: ws3Rejected["phase471-l7"] === "withheld-o4",
    syntheticEnvelopeRejectedAsReal: syntheticValidation.valid === false && syntheticValidation.reason === "synthetic-or-test-only",
    legacyTwoFieldEnvelopeRejectedAsReal: legacyValidation.valid === false,
    templateRejectedAsReal: templateValidation.valid === false,
    schemaTemplateMapRulingIdsEqual,
    productionShapeRecognizedWithoutCryptoClaim:
      productionShapeValidation.valid === true &&
      productionShapeValidation.reason === "schema-envelope-shape-valid-signature-unverified" &&
      productionShapeValidation.cryptographicVerificationPerformed === false,
  };

  const report = {
    schemaVersion: "o4-synthetic-overturn-report-v1",
    dependencyMapSha256: sha256(mapBytes),
    rulingCount: map.rulingIds.length,
    schemaRulingIdCount: schemaRulingIds.length,
    schemaMandatoryRulingIdCount: schemaMandatoryRulingIds.length,
    templateRulingIdCount: templateRulingIds.length,
    schemaTemplateMapRulingIdsEqual,
    branchCount: map.branches.length,
    declaredEdgeCount: expectedEdgeCount,
    exercisedEdgeCount: edgeCases.length,
    failedEdgeMutationCount: failedEdgeCases.length,
    unintendedBranchMutationCount,
    semanticCaseCount: Object.keys(semanticCases).length,
    failedSemanticCaseCount: Object.values(semanticCases).filter((x) => !x).length,
    canonicalArtifactCount: map.canonicalArtifactPaths.length,
    canonicalSnapshotSha256: sha256(Buffer.from(stable(artifactsBefore))),
    canonicalArtifactMutationCount,
    candidatePathCount: map.candidateRulingPaths.length,
    candidatePathMutationCount,
    candidatePathWriteCount: 0,
    syntheticRulingAcceptedAsRealCount: syntheticValidation.valid ? 1 : 0,
    legacyMinimalRulingAcceptedAsRealCount: legacyValidation.valid ? 1 : 0,
    exactEdgeMutationCoverage: edgeCases.length === expectedEdgeCount && failedEdgeCases.length === 0,
    syntheticOverturnBatteryPassed:
      edgeCases.length === expectedEdgeCount &&
      failedEdgeCases.length === 0 &&
      unintendedBranchMutationCount === 0 &&
      Object.values(semanticCases).every(Boolean) &&
      canonicalArtifactMutationCount === 0 &&
      candidatePathMutationCount === 0 &&
      !syntheticValidation.valid &&
      !legacyValidation.valid,
    semanticCases,
    failedEdgeCases,
    safetyNote: "Synthetic rulings are evaluated only in memory. No canonical artifact or Phase457 candidate ruling path is written.",
  };
  return report;
}

function main() {
  const json = process.argv.includes("--json");
  const check = process.argv.includes("--check");
  if (!json && !check) {
    process.stderr.write("usage: synthetic_overturn_test.js --check | --json\n");
    process.exit(2);
  }
  try {
    const report = run();
    if (json) process.stdout.write(`${JSON.stringify(report, null, 2)}\n`);
    else process.stdout.write(`O4 synthetic-overturn battery: ${report.syntheticOverturnBatteryPassed ? "passed" : "FAILED"} (${report.exercisedEdgeCount}/${report.declaredEdgeCount} edges)\n`);
    if (!report.syntheticOverturnBatteryPassed) process.exit(1);
  } catch (error) {
    if (json) process.stdout.write(`${JSON.stringify({ syntheticOverturnBatteryPassed: false, error: error.message }, null, 2)}\n`);
    else process.stderr.write(`O4 synthetic-overturn battery FAILED: ${error.message}\n`);
    process.exit(1);
  }
}

main();

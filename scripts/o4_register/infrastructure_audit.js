#!/usr/bin/env node
"use strict";

// Machine-only Phase477 infrastructure audit. This validates coverage,
// schema/template alignment, and the synthetic overturn protocol. It never
// reads or writes a real physicist ruling.

const crypto = require("crypto");
const fs = require("fs");
const path = require("path");
const { spawnSync } = require("child_process");
const { collectCoverage, loadJsonStrict, validateContract } = require("./lib");

const repoRoot = path.resolve(__dirname, "..", "..");
const coveragePath = path.join(__dirname, "coverage_contract.json");
const dependencyPath = path.join(__dirname, "dependency_map.json");
const schemaPath = path.join(__dirname, "o4_human_memo_schema_v1.json");
const templatePath = path.join(__dirname, "o4_human_memo_template_v1.json");

function sha256(file) {
  return crypto.createHash("sha256").update(fs.readFileSync(file)).digest("hex");
}

function fail(message) {
  process.stderr.write(`O4 infrastructure audit failed: ${message}\n`);
  process.exit(1);
}

try {
  const coverage = loadJsonStrict(coveragePath);
  validateContract(coverage);
  const rows = collectCoverage(repoRoot, coverage);
  const schema = loadJsonStrict(schemaPath);
  const template = loadJsonStrict(templatePath);

  const batteryRun = spawnSync(
    process.execPath,
    [path.join(__dirname, "synthetic_overturn_test.js"), "--json"],
    { cwd: repoRoot, encoding: "utf8" }
  );
  if (batteryRun.status !== 0) fail((batteryRun.stderr || batteryRun.stdout).trim());
  const battery = JSON.parse(batteryRun.stdout);

  const coverageIds = coverage.reviewItems.map((item) => item.id).sort();
  const templateIds = template.rulings.map((ruling) => ruling.rulingId).sort();
  const idSetsEqual = JSON.stringify(coverageIds) === JSON.stringify(templateIds)
    && battery.schemaTemplateMapRulingIdsEqual === true;
  const templateInert = template.templateOnly === true
    && template.documentKind === "template"
    && !Object.prototype.hasOwnProperty.call(template, "signatureEnvelope")
    && template.rulings.every((ruling) =>
      ruling.disposition === "insufficient-basis" && ruling.selectedOption === "defer");

  const dispositionCounts = Object.fromEntries(
    [...new Set(rows.map((row) => row.disposition))]
      .sort()
      .map((disposition) => [disposition, rows.filter((row) => row.disposition === disposition).length])
  );
  const infrastructureReady = rows.length === coverage.entries.length
    && idSetsEqual
    && templateInert
    && battery.syntheticOverturnBatteryPassed === true
    && battery.canonicalArtifactMutationCount === 0
    && battery.candidatePathWriteCount === 0
    && battery.syntheticRulingAcceptedAsRealCount === 0
    && battery.legacyMinimalRulingAcceptedAsRealCount === 0;
  if (!infrastructureReady) fail("one or more fail-closed readiness conditions are false");

  const report = {
    schemaVersion: "o4-infrastructure-audit-v1",
    infrastructureReady,
    exactCoveragePassed: true,
    recursivePendingArtifactCount: rows.length,
    coverageEntryCount: coverage.entries.length,
    unmappedPendingArtifactCount: 0,
    ambiguousCoverageArtifactCount: 0,
    dispositionCounts,
    mandatoryRulingIdCount: coverageIds.length,
    coverageSchemaTemplateRulingIdsEqual: idSetsEqual,
    memoTemplateInertAndProductionInvalid: templateInert,
    humanRulingReadOrAuthored: false,
    coverageContractSha256: sha256(coveragePath),
    dependencyMapSha256: sha256(dependencyPath),
    memoSchemaSha256: sha256(schemaPath),
    memoTemplateSha256: sha256(templatePath),
    syntheticOverturn: battery,
    coveredArtifacts: rows.map((row) => ({
      phaseId: row.phaseId,
      summaryPath: row.summaryPath,
      disposition: row.disposition,
      reviewItems: row.reviewItems,
      pendingPointers: row.pendingPointers,
      evidencePointers: row.evidencePointers,
    })),
  };
  process.stdout.write(`${JSON.stringify(report, null, 2)}\n`);
} catch (error) {
  fail(error.message);
}

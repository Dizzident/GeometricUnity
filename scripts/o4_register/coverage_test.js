#!/usr/bin/env node
"use strict";

const assert = require("assert");
const fs = require("fs");
const os = require("os");
const path = require("path");
const {
  MANDATORY_RULING_IDS,
  collectCoverage,
  evaluatePredicate,
  findNamedPointers,
  loadJsonStrict,
  validateContract,
} = require("./lib");

const repoRoot = path.resolve(__dirname, "..", "..");

function baseContract(overrides = {}) {
  return {
    schemaVersion: 1,
    reviewItems: MANDATORY_RULING_IDS.map((id) => ({ id, title: id })),
    entries: [{
      phaseId: "phase-test",
      summaryPath: "studies/phase_test/output/test_summary.json",
      disposition: "direct-review",
      reviewItems: ["O4-F3-THETA-HAAR"],
      pendingPointers: ["/nested/physicistReviewPending"],
      predicates: [{ pointer: "/evidence/haar", op: "equals", value: true }],
      note: "fixture",
      ...overrides,
    }],
  };
}

function withFixture(document, contract, callback) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "o4-coverage-"));
  try {
    const file = path.join(root, "studies", "phase_test", "output", "test_summary.json");
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, typeof document === "string" ? document : JSON.stringify(document));
    callback(root, contract);
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
}

const validDocument = {
  phaseId: "phase-test",
  nested: { physicistReviewPending: true },
  evidence: { haar: true },
};

assert.deepStrictEqual(
  findNamedPointers({ outer: { physicistReviewPending: true } }, "physicistReviewPending"),
  [{ pointer: "/outer/physicistReviewPending", value: true }]
);
assert.strictEqual(evaluatePredicate({ x: true }, { pointer: "/x", op: "equals", value: true }), true);
assert.strictEqual(evaluatePredicate({ x: "true" }, { pointer: "/x", op: "equals", value: true }), false);
assert.strictEqual(evaluatePredicate({ prose: "theta Haar" }, { pointer: "/evidence/haar", op: "equals", value: true }), false);
assert.strictEqual(evaluatePredicate({ x: ["a", "a"] }, { pointer: "/x", op: "array-exact-set", value: ["a", "a"] }), false);
assert.strictEqual(evaluatePredicate({ x: [16, 16, 15] }, { pointer: "/x", op: "array-all-equal", value: 16, count: 3 }), false);

withFixture(validDocument, baseContract(), (root, contract) => {
  const rows = collectCoverage(root, contract);
  assert.strictEqual(rows.length, 1);
  assert.deepStrictEqual(rows[0].pendingPointers, ["/nested/physicistReviewPending"]);
});

withFixture("{", baseContract(), (root, contract) =>
  assert.throws(() => collectCoverage(root, contract), /malformed JSON/));

withFixture(validDocument, { ...baseContract(), entries: [] }, (root, contract) =>
  assert.throws(() => collectCoverage(root, contract), /unmapped pending phase/));

withFixture(validDocument, baseContract({ pendingPointers: ["/physicistReviewPending"] }), (root, contract) =>
  assert.throws(() => collectCoverage(root, contract), /pending pointer drift/));

withFixture({ ...validDocument, evidence: { haar: false } }, baseContract(), (root, contract) =>
  assert.throws(() => collectCoverage(root, contract), /coverage predicate drift/));

withFixture({ ...validDocument, physicistReviewPending: false }, baseContract({ pendingPointers: [
  "/nested/physicistReviewPending", "/physicistReviewPending",
] }), (root, contract) =>
  assert.throws(() => collectCoverage(root, contract), /contradictory pending values/));

withFixture({ nested: { physicistReviewPending: true }, evidence: { haar: true } }, baseContract(), (root, contract) =>
  assert.throws(() => collectCoverage(root, contract), /missing phaseId/));

withFixture(validDocument, baseContract(), (root, contract) => {
  const duplicate = path.join(root, "studies", "phase_z", "output", "z_summary.json");
  fs.mkdirSync(path.dirname(duplicate), { recursive: true });
  fs.writeFileSync(duplicate, JSON.stringify(validDocument));
  assert.throws(() => collectCoverage(root, contract), /duplicate pending phaseId/);
});

assert.throws(
  () => validateContract({ ...baseContract(), reviewItems: baseContract().reviewItems.slice(1) }),
  /exactly equal the 13 mandatory/
);

const liveContract = loadJsonStrict(path.join(__dirname, "coverage_contract.json"));
const liveRows = collectCoverage(repoRoot, liveContract);
assert.strictEqual(liveRows.length, liveContract.entries.length);
assert(liveRows.some((row) => row.phaseId === "phase444-mode-volume-scaled-saturation-probe"));
assert.deepStrictEqual(
  liveContract.reviewItems.map((item) => item.id).sort(),
  [...MANDATORY_RULING_IDS].sort()
);
assert.deepStrictEqual(
  liveRows.find((row) => row.phaseId === "phase457-upsilon-portal-stage-a").reviewItems,
  ["O4-C3-WS3-MPROBE-SCOPE"]
);

process.stdout.write(`O4 exact coverage tests passed (${liveRows.length} live pending artifacts).\n`);

"use strict";
// Filesystem/provenance metadata only; no study or tensor operation is run.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { readPinnedJson, PINS } = require("../a68-retained-inputs");
const bytes = Buffer.from('{"synthetic":true}\n');
const sha256 = crypto.createHash("sha256").update(bytes).digest("hex");
function fixture(t) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-input-pin-"));
  t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  fs.mkdirSync(path.join(root, "input")); fs.writeFileSync(path.join(root, "input", "fixed.json"), bytes);
  return { root, pin: { path: "input/fixed.json", bytes: bytes.length, sha256 } };
}
test("retained inputs: exact source buffer is hash and size bound", t => {
  const { root, pin } = fixture(t);
  assert.deepEqual(readPinnedJson(root, pin, bytes.length), { synthetic: true });
  assert.throws(() => readPinnedJson(root, { ...pin, sha256: "0".repeat(64) }, bytes.length), /exact parsed source buffer hash/);
  assert.throws(() => readPinnedJson(root, { ...pin, bytes: bytes.length - 1 }, bytes.length), /source size before allocation/);
});
test("retained inputs: unsafe paths and invalid ceilings fail before filesystem reads", t => {
  const { root, pin } = fixture(t);
  for (const unsafe of ["/absolute.json", "../fixed.json", "input/../fixed.json", "input//fixed.json", "./input/fixed.json", "input\\fixed.json", "input/é.json"]) {
    assert.throws(() => readPinnedJson(root, { ...pin, path: unsafe }, bytes.length), /safe source path/);
  }
  for (const ceiling of [0, -1, 1.5, Infinity, Number.MAX_SAFE_INTEGER + 1, bytes.length - 1]) {
    assert.throws(() => readPinnedJson(root, pin, ceiling), /prospective source byte ceiling/);
  }
});
test("retained inputs: symlinked parent, symlinked leaf and nonregular leaf rejected", t => {
  const { root, pin } = fixture(t);
  fs.symlinkSync(path.join(root, "input"), path.join(root, "alias"));
  fs.symlinkSync(path.join(root, "input", "fixed.json"), path.join(root, "input", "link.json"));
  for (const file of ["alias/fixed.json", "input/link.json", "input"]) {
    assert.throws(() => readPinnedJson(root, { ...pin, path: file }, bytes.length), /regular source path components/);
  }
});
test("retained inputs: changed content cannot reuse a bound pin", t => {
  const { root, pin } = fixture(t);
  fs.writeFileSync(path.join(root, pin.path), bytes.toString().replace("true", "null"));
  assert.throws(() => readPinnedJson(root, pin, bytes.length), /exact parsed source buffer hash/);
});
test("retained inputs: upstream anchor descriptors are immutable", () => {
  assert.equal(Object.keys(PINS).length, 2);
  assert.throws(() => { PINS.geometry.sha256 = "0".repeat(64); }, TypeError);
  assert.throws(() => { PINS.stationary = {}; }, TypeError);
});

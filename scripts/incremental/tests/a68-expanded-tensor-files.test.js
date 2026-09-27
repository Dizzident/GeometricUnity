"use strict";
// Manufactured files only; no source geometry, coefficients or phase outputs.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { createExpandedTensorVerifier } = require("../a68-expanded-tensor-files");
const wire = rows => Buffer.from(JSON.stringify(rows) + "\n", "ascii");
const hash = rows => crypto.createHash("sha256").update(JSON.stringify(rows)).digest("hex");
const tensor = () => [{ form: 1, blade: 1, k0: 0, k1: 0, real: "1/3", imaginary: "-2" },
  { form: 2, blade: 16383, k0: 0, k1: 0, real: "0", imaginary: "7/5" }];
const caps = () => ({ marks: 10, fileBytes: 10000, totalReadBytes: 100000, totalRecords: 100, totalStringCharacters: 100000,
  totalSerializedBytes: 1000000, rationalCharacters: 128, snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } });
function fixture(t, retained = true) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-expanded-")); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const rows = tensor(), mark = { name: "full", degree: 1, node: 0, expanded: retained, sha256: hash(rows) };
  const declaration = { contextId: "point1", marks: [{ name: mark.name, degree: 1, expanded: retained, relativePath: retained ? "tensor.json" : null }], reservedPaths: ["graph.json", "metadata.json"] };
  const file = path.join(root, "tensor.json"); if (retained) fs.writeFileSync(file, wire(rows));
  return { root, file, rows, mark, declaration, create: (limits = caps()) => createExpandedTensorVerifier(root, declaration, limits) };
}
test("all full-support retained marks and DAG-only marks must be checked before completion", t => {
  const f = fixture(t); f.declaration.marks.push({ name: "zero", degree: 14, expanded: true, relativePath: "zero.json" },
    { name: "dag", degree: 1, expanded: false, relativePath: null }); fs.writeFileSync(path.join(f.root, "zero.json"), wire([]));
  const verifier = f.create();
  assert.equal(verifier.compareMark({ ...f.mark, name: "dag", expanded: false }, f.rows), true);
  assert.equal(verifier.compareMark({ name: "zero", degree: 14, node: 1, expanded: true, sha256: hash([]) }, []), true);
  assert.equal(verifier.compareMark(f.mark, f.rows), true);
  const report = verifier.finish(); assert.equal(report.marks, 3); assert.equal(report.expandedFiles, 2);
  assert.equal(report.readBytes, wire(f.rows).length + 1 + wire([]).length + 1); assert.equal(report.records, 4);
  assert.equal(report.expandedTensorArtifactsVerified, true); assert.ok(Object.values(report.scope).every(v => v === false));
  assert.throws(() => verifier.finish(), /closed/); assert.equal(verifier.snapshot().failed, true);
});
test("DAG-only tensors require correct full hashes but no retained file", t => {
  const f = fixture(t, false), verifier = f.create(); assert.equal(fs.existsSync(f.file), false);
  verifier.compareMark(f.mark, f.rows); assert.equal(verifier.finish().expandedFiles, 0);
  const bad = f.create(); assert.throws(() => bad.compareMark({ ...f.mark, sha256: "0".repeat(64) }, f.rows), /full tensor hash/);
});
test("missing changed reordered duplicate noncanonical and wrong-LF files fail closed", t => {
  const variants = [null, wire([{ ...tensor()[0], real: "2/3" }, tensor()[1]]), wire(tensor().reverse()),
    wire([...tensor(), tensor()[1]]), Buffer.from(JSON.stringify(tensor())), Buffer.from(" " + JSON.stringify(tensor()) + "\n"),
    Buffer.concat([wire(tensor()), Buffer.from("\n")]), Buffer.from(wire(tensor()).toString().replace('"form":1', '"form":1,"form":1'))];
  for (const bytes of variants) {
    const f = fixture(t), verifier = f.create(); if (bytes === null) fs.unlinkSync(f.file); else fs.writeFileSync(f.file, bytes);
    assert.throws(() => verifier.compareMark(f.mark, f.rows)); assert.equal(verifier.snapshot().failed, true);
    fs.writeFileSync(f.file, wire(f.rows)); assert.throws(() => verifier.compareMark(f.mark, f.rows), /failed/);
  }
});
test("file agreement with a false producer hash cannot replace reconstructed values", t => {
  const f = fixture(t), changed = [{ ...f.rows[0], real: "2/3" }, f.rows[1]]; fs.writeFileSync(f.file, wire(changed));
  assert.throws(() => f.create().compareMark({ ...f.mark, sha256: hash(changed) }, f.rows), /full tensor hash/);
});
test("declaration snapshots preserve retention paths and quotas against caller mutation", t => {
  const f = fixture(t), limits = caps(), verifier = f.create(limits);
  f.declaration.marks[0].relativePath = "missing.json"; f.declaration.marks[0].expanded = false; limits.totalReadBytes = 1;
  verifier.compareMark(f.mark, f.rows); assert.equal(verifier.finish().expandedFiles, 1);
});
test("path traversal aliases reserved files directory collisions and invalid retention fail before I/O", t => {
  const changes = [d => d.marks[0].relativePath = "../escape", d => d.marks[0].relativePath = "/tmp/tensor.json",
    d => d.marks[0].relativePath = "x\\y", d => d.marks[0].relativePath = "graph.json", d => d.marks[0].relativePath = "graph.json/tensor.json",
    d => d.marks.push({ ...d.marks[0], name: "second" }), d => d.marks[0].relativePath = null,
    d => d.marks[0].expanded = false, d => d.reservedPaths.push("graph.json")];
  for (const change of changes) { const f = fixture(t); change(f.declaration); assert.throws(() => f.create()); }
});
test("unknown duplicate omitted mistyped and weakened marks poison the comparison", t => {
  for (const change of [m => m.name = "unknown", m => m.degree = 2, m => m.expanded = false, m => m.node = -1,
    m => m.sha256 = "X".repeat(64), m => m.extra = true]) {
    const f = fixture(t), verifier = f.create(); change(f.mark); assert.throws(() => verifier.compareMark(f.mark, f.rows)); assert.throws(() => verifier.finish(), /failed/);
  }
  const f = fixture(t), duplicate = f.create(); duplicate.compareMark(f.mark, f.rows); assert.throws(() => duplicate.compareMark(f.mark, f.rows), /one exact/);
  const early = f.create(); assert.throws(() => early.finish(), /complete declared/); assert.throws(() => early.compareMark(f.mark, f.rows), /failed/);
});
test("own-data snapshots reject accessors and ignore inherited serialization hooks", t => {
  const f = fixture(t); let calls = 0;
  Object.defineProperty(f.rows[0], "toJSON", { get() { calls++; throw Error("getter"); } });
  assert.throws(() => f.create().compareMark(f.mark, f.rows), /closed ordered/); assert.equal(calls, 0);
  const g = fixture(t); Object.setPrototypeOf(g.rows[0], { toJSON() { calls++; return {}; } });
  const verifier = g.create(); verifier.compareMark(g.mark, g.rows); verifier.finish(); assert.equal(calls, 0);
  const h = fixture(t); Object.defineProperty(h.declaration.marks[0], "relativePath", { get() { calls++; return "tensor.json"; }, enumerable: true });
  assert.throws(() => h.create(), /own data/); assert.equal(calls, 0);
});
test("canonical reconstructed support cannot silently accept zeros nonlocal fields or unreduced rationals", t => {
  const changes = [r => r[0].real = "2/6", r => r[0].real = "-0", r => r[0].k0 = 1,
    r => r[0].k1 = -0, r => r[0].form = 3, r => r[0].blade = 16384,
    r => { r[0].real = "0"; r[0].imaginary = "0"; }, r => r.reverse(), r => r.push(r[1]), r => delete r[1]];
  for (const change of changes) { const f = fixture(t); change(f.rows); assert.throws(() => f.create().compareMark(f.mark, f.rows)); }
});
test("pre-read quotas include the EOF probe and pre-serialization cumulative work", t => {
  for (const change of [c => c.totalReadBytes = wire(tensor()).length, c => c.totalRecords = 1,
    c => c.fileBytes = wire(tensor()).length - 1, c => c.totalSerializedBytes = 2 * (wire(tensor()).length - 1),
    c => c.totalStringCharacters = 1]) {
    const f = fixture(t), limits = caps(); change(limits); fs.unlinkSync(f.file);
    assert.throws(() => f.create(limits).compareMark(f.mark, f.rows), e => e.code !== "ENOENT");
  }
  const f = fixture(t), limits = caps(), length = wire(f.rows).length;
  limits.fileBytes = length; limits.totalReadBytes = length + 1; limits.totalRecords = f.rows.length; limits.totalSerializedBytes = 2 * length - 1;
  const verifier = f.create(limits); verifier.compareMark(f.mark, f.rows); assert.equal(verifier.finish().serializedBytes, 2 * length - 1);
});
test("cumulative admission applies across marks, not just individual tensors", t => {
  const f = fixture(t); f.declaration.marks.push({ name: "again", degree: 1, expanded: true, relativePath: "absent.json" });
  for (const key of ["totalRecords", "totalReadBytes", "totalSerializedBytes"]) {
    const limits = caps(); limits[key] = key === "totalRecords" ? f.rows.length : key === "totalReadBytes" ? wire(f.rows).length + 1 : 2 * wire(f.rows).length - 1;
    const verifier = f.create(limits); verifier.compareMark(f.mark, f.rows);
    assert.throws(() => verifier.compareMark({ ...f.mark, name: "again" }, f.rows), e => e.code !== "ENOENT");
  }
});
test("symlink files and symlink parent directories cannot satisfy retained-file verification", t => {
  const f = fixture(t); fs.renameSync(f.file, path.join(f.root, "target.json")); fs.symlinkSync("target.json", f.file);
  assert.throws(() => f.create().compareMark(f.mark, f.rows), /symlink/);
  const g = fixture(t); fs.symlinkSync(g.root, path.join(g.root, "alias"), "dir"); g.declaration.marks[0].relativePath = "alias/tensor.json";
  assert.throws(() => g.create().compareMark(g.mark, g.rows), /symlink/);
});
test("short reads and growth during a read preserve failure instead of accepting a partial comparison", t => {
  const original = fs.readSync;
  for (const mode of ["short", "grow"]) {
    const f = fixture(t), verifier = f.create(); let touched = false;
    try {
      fs.readSync = (...args) => { if (!touched) { touched = true; if (mode === "short") return 0; const n = original(...args); fs.appendFileSync(f.file, "x"); return n; } return original(...args); };
      assert.throws(() => verifier.compareMark(f.mark, f.rows), mode === "short" ? /shortened/ : /grew/); assert.equal(verifier.snapshot().failed, true);
    } finally { fs.readSync = original; }
  }
});

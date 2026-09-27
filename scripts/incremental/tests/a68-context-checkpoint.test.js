"use strict";
// Tiny manufactured graph/metadata files only. Reading a computational
// checkpoint is NOT a source or numerical acceptance test.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { readContextCheckpoint, contextCheckpointIdentity } = require("../a68-context-checkpoint");
const wire = value => Buffer.from(JSON.stringify(value) + "\n");
const hash = bytes => crypto.createHash("sha256").update(bytes).digest("hex");
const limits = () => ({ graphBytes: 10000, metadataBytes: 10000, totalReadBytes: 20002,
  snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } });
function fixture(t, context = "point0/m0_j0") {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-context-pin-")); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const declaration = { contextId: context, graphPath: "graph.json", metadataPath: "metadata.json", marks: [{ name: "tiny", degree: 1, expanded: false, relativePath: null }] };
  const graph = { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: [], nodes: [], marks: [] };
  const artifact = (name, bytes) => { fs.writeFileSync(path.join(root, name), bytes); return { path: name, bytes: bytes.length, sha256: hash(bytes) }; };
  const graphPin = artifact(declaration.graphPath, wire(graph));
  const envelope = { schema: "phase627-context-computational-evidence-v1", context, status: "producer-complete", independentValidationComplete: false, graph: graphPin, metadata: { "check/tiny": true } };
  const observed = { context, graph: graphPin, metadata: artifact(declaration.metadataPath, wire(envelope)) };
  return { root, declaration, observed, graph, envelope, artifact,
    read: (caps = limits()) => readContextCheckpoint(root, declaration, observed, caps),
    rewrite: () => { observed.metadata = artifact(declaration.metadataPath, wire(envelope)); } };
}
test("fixed nonpoint context boundaries and all three diagnostics can be byte-pinned without scientific acceptance", t => {
  for (const id of ["point0/m0_j0", "point1/m9_j34", "diagnostic/grade10", "diagnostic/acceleration", "diagnostic/secondJets"]) {
    const f = fixture(t, id), pinned = contextCheckpointIdentity(f.read());
    assert.equal(pinned.declaration.contextId, id); assert.deepEqual(pinned.graph, f.graph); assert.deepEqual(pinned.envelope, f.envelope);
    assert.equal(pinned.scope.sameBufferHashesChecked, true); assert.ok(Object.entries(pinned.scope).filter(([k]) => k !== "sameBufferHashesChecked").every(([, v]) => v === false));
    assert.ok(Object.isFrozen(pinned.envelope.metadata)); assert.ok(Object.isFrozen(pinned.declaration.marks[0]));
  }
});
test("point checkpoints and noncanonical or out-of-domain context IDs cannot use the computational reader", t => {
  for (const context of ["point0", "point1", "point2/m0_j0", "point0/m10_j0", "point0/m0_j35", "point0/m0_j01", "point0/m0_j-1", "diagnostic/other", "diagnostic/secondjets", "diagnostic/grade10 "]) {
    const f = fixture(t, context); fs.unlinkSync(path.join(f.root, f.declaration.graphPath)); assert.throws(() => f.read(), /exact nonpoint/);
  }
});
test("copied or handmade checkpoint tokens cannot acquire the private read identity", t => {
  const f = fixture(t), token = f.read();
  for (const fake of [{ ...token }, {}, null]) assert.throws(() => contextCheckpointIdentity(fake), /private read-pinned/);
  f.declaration.marks[0].name = "changed"; f.envelope.metadata["check/tiny"] = false;
  assert.equal(contextCheckpointIdentity(token).declaration.marks[0].name, "tiny"); assert.equal(contextCheckpointIdentity(token).envelope.metadata["check/tiny"], true);
});
test("producer self-certification wrong status wrong schema extra fields and point envelopes are rejected", t => {
  for (const change of [e => e.status = "complete", e => e.independentValidationComplete = true, e => e.independentValidationComplete = "false",
    e => e.schema = "phase627-full-context-evidence-v1", e => e.extra = false, e => delete e.independentValidationComplete,
    e => e.context = "point1/m0_j0", e => e.metadata = [], e => e.pointTraversalComplete = false]) {
    const f = fixture(t); change(f.envelope); f.rewrite(); assert.throws(() => f.read());
  }
});
test("fresh valid metadata hashes cannot hide a different linked graph", t => {
  for (const change of [e => e.graph = { ...e.graph, sha256: "f".repeat(64) }, e => e.graph = { ...e.graph, bytes: e.graph.bytes + 1 },
    e => e.graph = { ...e.graph, path: "other.json" }]) {
    const f = fixture(t); change(f.envelope); f.rewrite(); assert.throws(() => f.read(), /exact same read-pinned graph/);
  }
});
test("same-buffer hashes exact sizes canonical fields and one LF are mandatory", t => {
  for (const mode of ["hash", "size", "duplicate", "whitespace", "extraLF", "order", "badUTF8"]) {
    const f = fixture(t); let bytes = wire(f.envelope);
    if (mode === "hash") { f.observed.metadata.sha256 = "0".repeat(64); assert.throws(() => f.read(), /same-buffer/); continue; }
    if (mode === "size") { f.observed.metadata.bytes++; assert.throws(() => f.read(), /exact artifact size/); continue; }
    if (mode === "duplicate") bytes = Buffer.from(bytes.toString().replace('"independentValidationComplete":false', '"independentValidationComplete":false,"independentValidationComplete":false'));
    if (mode === "whitespace") bytes = Buffer.concat([Buffer.from(" "), bytes]);
    if (mode === "extraLF") bytes = Buffer.concat([bytes, Buffer.from("\n")]);
    if (mode === "order") { const { metadata, ...rest } = f.envelope; bytes = wire({ metadata, ...rest }); }
    if (mode === "badUTF8") bytes = Buffer.concat([Buffer.from([255]), bytes]);
    f.observed.metadata = f.artifact(f.declaration.metadataPath, bytes); assert.throws(() => f.read());
  }
});
test("combined read quotas include both probes and declaration snapshots are admitted before file reads", t => {
  const f = fixture(t), total = f.observed.graph.bytes + f.observed.metadata.bytes + 2;
  assert.equal(contextCheckpointIdentity(f.read({ ...limits(), totalReadBytes: total })).scope.sameBufferHashesChecked, true);
  fs.unlinkSync(path.join(f.root, f.declaration.graphPath));
  for (const cap of [{ ...limits(), totalReadBytes: total - 1 }, { ...limits(), graphBytes: 1 }, { ...limits(), metadataBytes: 1 },
    { ...limits(), snapshot: { ...limits().snapshot, nodes: 1 } }]) assert.throws(() => f.read(cap), e => e.code !== "ENOENT");
});
test("wrong declarations traversal symlink files and symlink parents cannot be read as pinned artifacts", t => {
  const f = fixture(t); f.observed.graph.path = "../graph.json"; assert.throws(() => f.read(), /artifact descriptor/);
  f.observed.graph.path = "different.json"; assert.throws(() => f.read(), /frozen declaration/);
  const g = fixture(t); fs.renameSync(path.join(g.root, "graph.json"), path.join(g.root, "target.json")); fs.symlinkSync("target.json", path.join(g.root, "graph.json")); assert.throws(() => g.read(), /symlink/);
  const h = fixture(t); fs.symlinkSync(h.root, path.join(h.root, "alias"), "dir");
  assert.throws(() => readContextCheckpoint(path.join(h.root, "alias"), h.declaration, h.observed, limits()), /symlink/);
});
test("accessors are not invoked and the whole declaration is frozen before the first file read", t => {
  const f = fixture(t); let calls = 0; Object.defineProperty(f.observed.metadata, "bytes", { enumerable: true, get() { calls++; return 1; } });
  assert.throws(() => f.read(), /own data/); assert.equal(calls, 0);
  const g = fixture(t), original = fs.readSync; let touched = false;
  try {
    fs.readSync = (...args) => { if (!touched) { touched = true; g.declaration.marks[0].name = "changed-during-read"; } return original(...args); };
    assert.equal(contextCheckpointIdentity(g.read()).declaration.marks[0].name, "tiny"); assert.equal(touched, true);
  } finally { fs.readSync = original; }
});
test("short or growing reads cannot mint a complete pinned identity", t => {
  const original = fs.readSync;
  for (const mode of ["short", "grow"]) {
    const f = fixture(t); let touched = false;
    try {
      fs.readSync = (...args) => { if (!touched) { touched = true; if (mode === "short") return 0; const n = original(...args); fs.appendFileSync(path.join(f.root, "graph.json"), "x"); return n; } return original(...args); };
      assert.throws(() => f.read(), mode === "short" ? /shortened/ : /grew/);
    } finally { fs.readSync = original; }
  }
});

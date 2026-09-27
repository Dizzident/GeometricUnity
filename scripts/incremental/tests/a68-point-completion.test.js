"use strict";
// Manufactured file/linkage and lifecycle tests only. Generic ledger records
// below are NOT authenticated child replays and cannot mint source adapter state.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { readPointCompletion, pointCompletionIdentity, createPointCompletionLedger } = require("../a68-point-completion");
const wire = value => Buffer.from(JSON.stringify(value) + "\n");
const hash = bytes => crypto.createHash("sha256").update(bytes).digest("hex");
const snapshot = () => ({ nodes: 20000, arraySlots: 20000, stringCharacters: 500000, maxDepth: 16 });
const limits = () => ({ backgroundBytes: 10000, metadataBytes: 10000, totalReadBytes: 20002, snapshot: snapshot() });
function fixture(t, id = "point0") {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-point-final-"));t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  fs.mkdirSync(path.join(root, id));
  const declaration = { contextId: id, graphPath: id + "/graph.json", backgroundPath: id + "/background-checkpoint.json", metadataPath: id + "/metadata.json" };
  const artifact = (file, bytes) => { fs.writeFileSync(path.join(root, file), bytes); return { path: file, bytes: bytes.length, sha256: hash(bytes) }; };
  const graph = artifact(declaration.graphPath, wire({ manufactured: true }));
  const background = { schema: "phase627-point-background-checkpoint-v1", context: id, status: "background-sealed", pointTraversalComplete: false,
    graph, metadata: { "check/manufactured": true } };
  const backgroundPin = artifact(declaration.backgroundPath, wire(background));
  const final = { schema: "phase627-point-computational-completion-v1", context: id, status: "producer-complete", independentValidationComplete: false,
    backgroundCheckpoint: backgroundPin, metadata: { ...background.metadata } };
  const observed = { context: id, background: { context: id, graph, metadata: backgroundPin }, metadata: artifact(declaration.metadataPath, wire(final)) };
  const seed = { root, contextId: id, graph, backgroundMetadata: backgroundPin, metadataPath: declaration.metadataPath };
  return { root, id, declaration, observed, background, final, seed, artifact,
    read: (cap = limits()) => readPointCompletion(root, declaration, observed, cap),
    rewrite: () => { observed.metadata = artifact(declaration.metadataPath, wire(final)); } };
}
function children(f, ledger, count = 350) {
  const health = Array.from({ length: count }, () => ({ failed: false }));
  for (let n = 0; n < count; n++) {
    const id = f.id + "/m" + Math.floor(n / 35) + "_j" + n % 35, pin = name => ({ path: id + "/" + name + ".json", bytes: 3, sha256: hash("{}\n") });
    ledger.registerChild({ root: f.root, contextId: id, graph: pin("graph"), metadata: pin("metadata") }, () => health[n]);
  }
  return health;
}
const ledgerFor = (f, parent = { failed: false }, cap = snapshot()) => createPointCompletionLedger(f.seed, cap, () => parent);

test("both point endings read exact background/final bytes without claiming source or child acceptance", t => {
  for (const id of ["point0", "point1"]) {
    const f = fixture(t, id); fs.unlinkSync(path.join(f.root, f.declaration.graphPath)); // no redundant numerical DAG read
    const token = f.read(), value = pointCompletionIdentity(token);
    assert.deepEqual(value.background, f.background); assert.deepEqual(value.completion, f.final);
    assert.ok(Object.isFrozen(value.completion.metadata)); assert.equal(value.scope.sameBufferHashesChecked, true);
    assert.equal(value.scope.backgroundMetadataUnchanged, true);
    for (const k of ["numericalReplayComplete", "sourceAuthenticityEstablished", "childReplaysAccepted", "pointTraversalComplete", "scientificExecutionAuthorized"])
      assert.equal(value.scope[k], false);
    for (const fake of [{ ...token }, {}, null]) assert.throws(() => pointCompletionIdentity(fake), /private read-pinned/);
  }
});
test("old completed producer claims altered background metadata and wrong pin linkage reject", t => {
  for (const change of [f => f.final.schema = "phase627-full-context-evidence-v1", f => f.final.status = "complete",
    f => f.final.independentValidationComplete = true, f => f.final.context = "point1", f => f.final.extra = false,
    f => f.final.metadata["check/manufactured"] = false, f => f.final.backgroundCheckpoint = { ...f.final.backgroundCheckpoint, sha256: "0".repeat(64) }]) {
    const f = fixture(t); change(f); f.rewrite(); assert.throws(() => f.read());
  }
});
test("background seal must retain its original incomplete status graph pin and exact point identity", t => {
  for (const change of [f => f.background.pointTraversalComplete = true, f => f.background.context = "point1",
    f => f.background.status = "complete", f => f.background.graph = { ...f.background.graph, sha256: "f".repeat(64) }]) {
    const f = fixture(t); change(f); f.observed.background.metadata = f.artifact(f.declaration.backgroundPath, wire(f.background));
    f.final.backgroundCheckpoint = f.observed.background.metadata; f.rewrite(); assert.throws(() => f.read(), /background checkpoint linkage/);
  }
});
test("fixed paths same-buffer sizes hashes canonical wire and symlinks are enforced", t => {
  for (const mode of ["path", "size", "hash", "duplicate", "LF", "symlink"]) {
    const f = fixture(t);
    if (mode === "path") f.declaration.metadataPath = "other.json";
    if (mode === "size") f.observed.metadata.bytes++;
    if (mode === "hash") f.observed.metadata.sha256 = "0".repeat(64);
    if (mode === "duplicate") f.observed.metadata = f.artifact(f.declaration.metadataPath, Buffer.from(wire(f.final).toString().replace('"status":"producer-complete"', '"status":"producer-complete","status":"producer-complete"')));
    if (mode === "LF") f.observed.metadata = f.artifact(f.declaration.metadataPath, Buffer.from(JSON.stringify(f.final)));
    if (mode === "symlink") { fs.renameSync(path.join(f.root, f.declaration.metadataPath), path.join(f.root, "target.json"));fs.symlinkSync("../target.json", path.join(f.root, f.declaration.metadataPath)); }
    assert.throws(() => f.read());
  }
});
test("combined read probes and cumulative header/artifact snapshots reject before file IO", t => {
  const f = fixture(t), exact = f.observed.background.metadata.bytes + f.observed.metadata.bytes + 2;
  assert.ok(f.read({ ...limits(), totalReadBytes: exact }));fs.unlinkSync(path.join(f.root, f.declaration.backgroundPath));
  for (const cap of [{ ...limits(), totalReadBytes: exact - 1 }, { ...limits(), backgroundBytes: 1 },
    { ...limits(), metadataBytes: 1 }, { ...limits(), snapshot: { ...snapshot(), nodes: 1 } }])
    assert.throws(() => f.read(cap), e => e.code !== "ENOENT");
});
test("own-data capture refuses getters and detaches caller metadata before reads", t => {
  const f = fixture(t);let getter = 0;
  Object.defineProperty(f.observed, "context", { enumerable: true, get() { getter++;return f.id; } });
  assert.throws(() => f.read(), /own data/);assert.equal(getter, 0);
  const g = fixture(t), original = fs.readSync;let once = false;
  try { fs.readSync = (...args) => { if (!once) { once = true;g.observed.background.graph.sha256 = "f".repeat(64); }return original(...args); };
    assert.notEqual(pointCompletionIdentity(g.read()).observed.background.graph.sha256, "f".repeat(64));
  } finally { fs.readSync = original; }
});
test("generic compact ledger requires all350 ordered registrations and private final token", t => {
  const f = fixture(t), ledger = ledgerFor(f);children(f, ledger);
  const result = ledger.complete(f.read());assert.equal(result.children, 350);assert.equal(result.scope.sourceAuthenticityEstablished, false);
  assert.equal(ledger.snapshot().completed, true);assert.equal(ledger.snapshot().retainedFullGraphs, 0);assert.equal(ledger.snapshot().retainedSourceAdapters, 0);
  assert.throws(() => ledger.complete(f.read()), /completed ledger/);assert.equal(ledger.snapshot().completed, false);
});
test("missing child out-of-order wrong-point wrong-root and repeat registrations poison the ledger", t => {
  const f = fixture(t), incomplete = ledgerFor(f);children(f, incomplete, 349);
  assert.throws(() => incomplete.complete(f.read()), /all350/);assert.equal(incomplete.snapshot().failed, true);
  for (const mode of ["order", "point", "root", "repeat"]) {
    const ledger = ledgerFor(f);if (mode === "repeat") children(f, ledger, 1);
    const id = mode === "order" ? "point0/m0_j1" : mode === "point" ? "point1/m0_j0" : "point0/m0_j0";
    const pin = file => ({ path: id + "/" + file + ".json", bytes: 3, sha256: hash("{}\n") });
    assert.throws(() => ledger.registerChild({ root: mode === "root" ? f.root + "/other" : f.root, contextId: id, graph: pin("graph"), metadata: pin("metadata") }, () => ({ failed: false })), /ordered/);
    assert.equal(ledger.snapshot().failed, true);
  }
});
test("late child or parent failure invalidates terminal acceptance without retaining adapter graphs", t => {
  for (const after of [false, true]) {
    const f = fixture(t), parent = { failed: false }, ledger = ledgerFor(f, parent), health = children(f, ledger);
    if (after) ledger.complete(f.read());health[177].failed = true;
    if (!after) assert.throws(() => ledger.complete(f.read()), /healthy accepted child/);
    assert.equal(ledger.snapshot().completed, false);assert.equal(ledger.snapshot().failed, true);
  }
  const f = fixture(t), parent = { failed: false }, ledger = ledgerFor(f, parent);children(f, ledger);ledger.complete(f.read());parent.failed = true;
  assert.equal(ledger.snapshot().completed, false);
});
test("another valid point-final file cannot substitute a different accepted root or background", t => {
  for (const mode of ["root", "background", "forged"]) {
    const f = fixture(t), ledger = ledgerFor(f);children(f, ledger);let token;
    if (mode === "root") token = fixture(t).read();
    else if (mode === "background") {
      f.background.metadata.extra = true;f.final.metadata.extra = true;
      f.observed.background.metadata = f.artifact(f.declaration.backgroundPath, wire(f.background));f.final.backgroundCheckpoint = f.observed.background.metadata;f.rewrite();token = f.read();
    } else token = { ...f.read() };
    assert.throws(() => ledger.complete(token));assert.equal(ledger.snapshot().failed, true);
  }
});
test("compact ledger snapshot quotas and swallowed health callback reentry fail closed", t => {
  const f = fixture(t);assert.throws(() => ledgerFor(f, { failed: false }, { ...snapshot(), nodes: 1 }), /ceiling/);
  const small = ledgerFor(f, { failed: false }, { ...snapshot(), nodes: 100 });assert.throws(() => children(f, small), /ceiling|positive limits/);assert.equal(small.snapshot().failed, true);
  const ledger = ledgerFor(f), id = f.id + "/m0_j0", pin = file => ({ path: id + "/" + file + ".json", bytes: 3, sha256: hash("{}\n") });
  const row = { root: f.root, contextId: id, graph: pin("graph"), metadata: pin("metadata") };
  assert.throws(() => ledger.registerChild(row, () => { assert.throws(() => ledger.registerChild(row, () => ({ failed: false })), /reentrant/);return { failed: false }; }));
  assert.equal(ledger.snapshot().failed, true);
});

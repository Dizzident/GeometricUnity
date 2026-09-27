"use strict";

// Tiny manufactured artifacts only. Empty geometry placeholders below are
// deliberately NOT source geometry and cannot mint a source adapter brand.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { readVerifiedReplayReceipt } = require("../a68-tensor-replay");
const { pointCheckNames, readPointCheckpoint, pointCheckpointIdentity, replayPointCheckpoint } = require("../a68-point-checkpoint");
const { validateGeometryWireIdentity } = require("../a68-geometry-metadata");
const hash = bytes => crypto.createHash("sha256").update(bytes).digest("hex");
const wire = x => Buffer.from(JSON.stringify(x) + "\n");
const snapshot = () => ({ nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 });
const readLimits = () => ({ graphBytes: 100000, metadataBytes: 100000, totalReadBytes: 200002, snapshot: snapshot() });
function replayLimits() {
  return { tensor: { nodes: 100, marks: 100, tensorRecords: 3, rationalCharacters: 32, tensorBytes: 10000, graphBytes: 100000, pairVisits: 10000, slotVisits: 10000, liveRecords: 100 },
    scalarSchedule: { tensorNodes: 100, tensorEdges: 1000, tensorMarks: 100, scalarNodes: 1000, scalarEdges: 1000, roots: 100,
      geometryMatrices: 100, geometryEntries: 1000, maxDepth: 64, stringCharacters: 100000, rationalCharacters: 100 },
    scalarArithmetic: { maxBits: 128, scalarOperations: 100000, rationalObjects: 100000, matrixObjects: 1, matrixCells: 1, slotVisits: 1000 },
    scalarReplay: { tensorRecords: 100, tensorResolutions: 1000, recordVisits: 10000, coefficientReads: 10000, readCharacters: 100000, outputCharacters: 100000, rationalCharacters: 100, liveScalars: 1000 },
    wire: { liveWireRecords: 100, sortReferences: 100, liveWireCharacters: 100000, serializationRecordVisits: 10000, serializationCharacters: 1000000 },
    consumerSchedule: { consumers: 100, expressionNodes: 1000, expressionEdges: 1000, tensorReferences: 1000, scalarReferences: 100, stringCharacters: 100000, maxDepth: 64, rationalCharacters: 100, retainedRootValues: 100 },
    consumerArithmetic: { maxBits: 128, scalarOperations: 100000, rationalObjects: 100000, matrixObjects: 100, matrixCells: 10000, slotVisits: 100000 },
    consumerReplay: { tensorRecords: 100, tensorResolutions: 1000, parsedTensorRecords: 10000, recordVisits: 100000, geometryResolutions: 100,
      geometryCoordinates: 100000, arraySlots: 100000, coefficientReads: 100000, readCharacters: 100000, outputCharacters: 100000,
      rationalCharacters: 100, predicateNodes: 10000, rootValues: 100, errorValues: 100, resultValues: 100, consumerComparisons: 100, sourceResolutions: 100 },
    exports: { tensors: 1, records: 3, characters: 1000, recordVisits: 3 },
    expanded: { marks: 100, fileBytes: 10000, totalReadBytes: 100000, totalRecords: 1000, totalStringCharacters: 100000, totalSerializedBytes: 1000000, rationalCharacters: 32, snapshot: snapshot() }, snapshot: snapshot() };
}
function fixture(t, point = 1, expanded = false) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-checkpoint-")); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const declaration = { contextId: "point" + point, graphPath: "graph.json", metadataPath: "checkpoint.json",
    marks: [{ name: "tiny", degree: 1, expanded, relativePath: expanded ? "tensor.json" : null }] };
  const r = new MixedRecipe(), value = r.Unit(1, 1); r.Mark("tiny", 1, value, expanded); const plan = r.Finish();
  const tensor = [{ form: 1, blade: 1, k0: 0, k1: 0, real: "1", imaginary: "0" }], tensorWire = JSON.stringify(tensor), digest = hash(tensorWire);
  const graph = { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: [], nodes: plan.nodes.map(n => ({ ...n, records: 1, bytes: tensorWire.length, sha256: digest })), marks: plan.marks.map(m => ({ ...m, sha256: digest })) };
  const background = { point, frame: [], inverseFrame: [], metric: {}, connection: {}, frameLift: [], framePartial: [], omega: [] };
  const checks = pointCheckNames(point).map(name => ({ name, metadataPath: "check/" + name,
    predicate: { kind: "tensorEqual", left: { kind: "tensorNode", node: 0 }, right: { kind: "tensorNode", node: 0 } } }));
  const metadata = { "background/geometry": background, ...Object.fromEntries(checks.map(c => [c.metadataPath, true])) };
  const artifact = (file, bytes) => { fs.writeFileSync(path.join(root, file), bytes); return { path: file, bytes: bytes.length, sha256: hash(bytes) }; };
  if (expanded) artifact("tensor.json", wire(tensor));
  const graphPin = artifact(declaration.graphPath, wire(graph));
  const checkpoint = { schema: "phase627-point-background-checkpoint-v1", context: declaration.contextId, status: "background-sealed", pointTraversalComplete: false, graph: graphPin, metadata };
  const pins = { context: declaration.contextId, graph: graphPin, metadata: artifact(declaration.metadataPath, wire(checkpoint)) };
  const expected = { contextId: declaration.contextId, tensorPlan: plan, namedRoots: [], geometry: [], checks,
    domainChecks: [{ name: "input/X", node: 0, degree: 1, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true }], error: null,
    requests: [{ id: "tiny", degree: 1, node: 0 }] };
  return { root, declaration, graph, checkpoint, pins, expected, tensor, artifact,
    read: (limits = readLimits()) => readPointCheckpoint(root, declaration, pins, limits),
    rewrite: () => { pins.metadata = artifact(declaration.metadataPath, wire(checkpoint)); } };
}

test("pinned checkpoint retains immutable exact bytes but does not claim numerical or source proof", t => {
  const f = fixture(t), token = f.read(), identity = pointCheckpointIdentity(token);
  assert.deepEqual(identity.graph, f.graph); assert.deepEqual(identity.checkpoint, f.checkpoint);
  assert.ok(Object.isFrozen(identity.checkpoint.metadata["background/geometry"]));
  assert.equal(identity.scope.numericalReplayComplete, false); assert.equal(identity.scope.sourceAuthenticityEstablished, false);
  assert.equal(identity.scope.pointTraversalComplete, false); assert.equal(identity.scope.totalProcessMemoryProved, false);
  for (const fake of [{ ...token }, {}, null]) assert.throws(() => pointCheckpointIdentity(fake), /private read-pinned/);
  f.checkpoint.status = "corrupt"; assert.equal(identity.checkpoint.status, "background-sealed");
});

test("read-pinned manufactured point performs actual numerical replay before receipt", t => {
  const f = fixture(t), result = replayPointCheckpoint(f.read(), f.expected, replayLimits(), () => { throw Error("no leaves"); });
  assert.equal(result.scalarReplay.complete, true); assert.equal(result.consumerReplay.complete, true);
  assert.equal(result.consumerReplay.consumersExecuted, 16);
  const identity = readVerifiedReplayReceipt(result.exportReceipt);
  assert.deepEqual(identity.exports[0].tensor, f.tensor); assert.deepEqual(identity.tensorPlan, f.expected.tensorPlan);
  assert.equal(result.expandedArtifacts.expandedTensorArtifactsVerified, true); assert.equal(result.expandedArtifacts.expandedFiles, 0);
});

test("expanded point file is compared with actual tiny numerical replay before export receipt", t => {
  const f = fixture(t, 1, true), result = replayPointCheckpoint(f.read(), f.expected, replayLimits(), () => []);
  assert.equal(result.expandedArtifacts.expandedFiles, 1);
  assert.equal(result.expandedArtifacts.expandedTensorArtifactsVerified, true);
  assert.equal(result.expandedArtifacts.scope.sourceAuthenticityEstablished, false);
  assert.deepEqual(readVerifiedReplayReceipt(result.exportReceipt).exports[0].tensor, f.tensor);
});

test("correct graph and predicates cannot hide missing or changed expanded files", t => {
  for (const mode of ["missing", "coefficient", "whitespace", "noLF"]) {
    const f = fixture(t, 1, true), token = f.read();
    if (mode === "missing") fs.unlinkSync(path.join(f.root, "tensor.json"));
    else f.artifact("tensor.json", mode === "coefficient" ? wire([{ ...f.tensor[0], real: "2" }]) :
      mode === "whitespace" ? Buffer.from(" " + JSON.stringify(f.tensor) + "\n") : Buffer.from(JSON.stringify(f.tensor)));
    let result = null; assert.throws(() => { result = replayPointCheckpoint(token, f.expected, replayLimits(), () => []); }); assert.equal(result, null);
  }
});

test("retention paths are frozen and cannot collide with checkpoint artifacts", t => {
  const f = fixture(t, 1, true), token = f.read(); f.declaration.marks[0].relativePath = "absent.json";
  assert.equal(replayPointCheckpoint(token, f.expected, replayLimits(), () => []).expandedArtifacts.expandedFiles, 1);
  const g = fixture(t, 1, true); g.declaration.marks[0].relativePath = g.declaration.graphPath;
  assert.throws(() => replayPointCheckpoint(g.read(), g.expected, replayLimits(), () => []), /distinct expanded path/);
  const h = fixture(t, 1, true); h.declaration.marks[0].expanded = false; h.declaration.marks[0].relativePath = null;
  assert.throws(() => replayPointCheckpoint(h.read(), h.expected, replayLimits(), () => []), /frozen retention menu/);
});

test("point0 requires all three hand anchors in addition to the15 common checks", t => {
  const f = fixture(t, 0), identity = pointCheckpointIdentity(f.read());
  assert.equal(Object.keys(identity.checkpoint.metadata).length, 19);
  delete f.checkpoint.metadata["check/nativeExteriorHandAnchor"]; f.rewrite(); assert.throws(() => f.read(), /closed ordered/);
});

test("wrong status complete traversal foreign context or linked graph fail despite fresh valid hashes", t => {
  const changes = [f => f.checkpoint.status = "complete", f => f.checkpoint.pointTraversalComplete = true,
    f => f.checkpoint.context = "point0", f => f.checkpoint.graph = { ...f.pins.graph, sha256: "f".repeat(64) }];
  for (const change of changes) { const f = fixture(t); change(f); f.rewrite(); assert.throws(() => f.read()); }
});

test("missing extra reordered nonboolean or false producer checks are rejected", t => {
  for (const change of [m => delete m["check/covariantExterior"], m => m.extra = true, m => m["check/covariantExterior"] = false,
    m => m["check/covariantExterior"] = "true", m => { delete m["check/nativeFirstJet_0"]; m["check/nativeFirstJet_0"] = true; }]) {
    const f = fixture(t); change(f.checkpoint.metadata); f.rewrite(); assert.throws(() => f.read());
  }
});

test("self-consistent reported true flags cannot conceal false independent numerical predicates", t => {
  const f = fixture(t), copy = JSON.parse(JSON.stringify(f.expected)); copy.checks.at(-1).predicate = { kind: "tensorZero", tensor: { kind: "tensorNode", node: 0 } };
  let result = null; assert.throws(() => { result = replayPointCheckpoint(f.read(), copy, replayLimits(), () => []); }, /independent consumer comparison rejected covariantExterior/); assert.equal(result, null);
});

test("numerical bridge rejects forged read identity changed recipes and missing independent checks", t => {
  const f = fixture(t), token = f.read();
  assert.throws(() => replayPointCheckpoint({ ...token }, f.expected, replayLimits(), () => []), /private read-pinned/);
  const wrong = JSON.parse(JSON.stringify(f.expected)); wrong.tensorPlan.nodes[0].parameters.real = "2";
  assert.throws(() => replayPointCheckpoint(token, wrong, replayLimits(), () => []), /independent primitive/);
  assert.throws(() => replayPointCheckpoint(token, { ...f.expected, checks: [] }, replayLimits(), () => []), /all independent point checks/);
});

test("pre-read per-file and combined quotas reject before touching absent artifacts", t => {
  const f = fixture(t); fs.unlinkSync(path.join(f.root, f.declaration.graphPath));
  for (const limits of [{ ...readLimits(), graphBytes: f.pins.graph.bytes - 1 }, { ...readLimits(), metadataBytes: 1 },
    { ...readLimits(), totalReadBytes: f.pins.graph.bytes + f.pins.metadata.bytes + 1 }]) assert.throws(() => f.read(limits), /prospective combined/);
});

test("exact same-buffer hashes sizes and canonical JSON reject tampered artifacts", t => {
  for (const mode of ["hash", "size", "duplicate", "whitespace", "twoLF", "utf8"]) {
    const f = fixture(t); let bytes = wire(f.checkpoint);
    if (mode === "hash") { f.pins.metadata.sha256 = "f".repeat(64); assert.throws(() => f.read(), /same-buffer/); continue; }
    if (mode === "size") { f.pins.metadata.bytes++; assert.throws(() => f.read(), /exact artifact size/); continue; }
    if (mode === "duplicate") bytes = Buffer.from(bytes.toString().replace('"status":"background-sealed"', '"status":"background-sealed","status":"background-sealed"'));
    if (mode === "whitespace") bytes = Buffer.from(" " + bytes);
    if (mode === "twoLF") bytes = Buffer.concat([bytes, Buffer.from("\n")]);
    if (mode === "utf8") bytes = Buffer.concat([Buffer.from([255]), bytes]);
    f.pins.metadata = f.artifact(f.declaration.metadataPath, bytes); assert.throws(() => f.read());
  }
});

test("declared path mismatches traversal and symlink parents fail closed", t => {
  const f = fixture(t); f.pins.graph.path = "../outside"; assert.throws(() => f.read(), /artifact descriptor/);
  f.pins.graph.path = "different.json"; assert.throws(() => f.read(), /frozen declaration/);
  const g = fixture(t), nested = path.join(g.root, "link"); fs.symlinkSync(g.root, nested, "dir");
  assert.throws(() => readPointCheckpoint(nested, g.declaration, g.pins, readLimits()), /symlink/);
  fs.unlinkSync(path.join(g.root, g.declaration.graphPath)); fs.symlinkSync(path.join(g.root, g.declaration.metadataPath), path.join(g.root, g.declaration.graphPath));
  assert.throws(() => g.read(), /symlink/);
});

test("descriptor accessors are rejected without invocation and snapshot limits are enforced", t => {
  const f = fixture(t); let invoked = false;
  Object.defineProperty(f.pins.graph, "path", { enumerable: true, get() { invoked = true; return "graph.json"; } });
  assert.throws(() => f.read(), /own data/); assert.equal(invoked, false);
  const g = fixture(t); assert.throws(() => g.read({ ...readLimits(), snapshot: { ...snapshot(), nodes: 1 } }), /nodes/);
});

test("geometry identity reader accepts C# camel-case baseline and germ fields only", t => {
  const f = fixture(t); validateGeometryWireIdentity("baseline", { point: 1 }, f.checkpoint.metadata["background/geometry"]);
  const old = { Point: 1, ...f.checkpoint.metadata["background/geometry"] }; delete old.point;
  assert.throws(() => validateGeometryWireIdentity("baseline", { point: 1 }, old), /ordered/);
  const identity = { metricBasis: 2, jetIndex: 3, multiindex: [0, 1, 0, 0], order: 1 };
  const germ = { ...identity, shear: {}, deltaMetric: {}, blockMetric: {}, deltaConnection: {}, palatini: {}, motion: [], motionPartial: [], motionCovariant: [], deltaOmega: [], deltaOmegaPartial: [], deltaFrame: [] };
  validateGeometryWireIdentity("germ", identity, germ);
  assert.throws(() => validateGeometryWireIdentity("germ", { ...identity, jetIndex: 4 }, germ), /complete germ identity/);
  const capital = { MetricBasis: 2, JetIndex: 3, Multiindex: [0, 1, 0, 0], Order: 1, ...Object.fromEntries(Object.entries(germ).slice(4)) };
  assert.throws(() => validateGeometryWireIdentity("germ", identity, capital), /ordered/);
});

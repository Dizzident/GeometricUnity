"use strict";

// Extract ONLY pure arithmetic declarations and the uncalled A68 block.
// Fixtures are hand-enumerated tiny synthetic tensors; no source geometry,
// phase Program, full integrity verifier, or production coefficients execute.
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");
const crypto = require("node:crypto");
const { createRequire } = require("node:module");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { TOP_SCHEMA } = require("../a68-original-action-recipe");
const { readVerifiedReplayReceipt } = require("../a68-tensor-replay");
const { createContextMetadataComparator } = require("../a68-context-metadata");
const { compileAuditConsumerSchedule, ERROR_FIELDS } = require("../a68-audit-consumer-schedule");
const repository = path.resolve(__dirname, "../../..");
const source = fs.readFileSync(path.join(repository, "scripts/verify_boson_claim_integrity.sh"), "utf8");
const begin = source.indexOf("// Begin A68 prospective DAG replay core."), end = source.indexOf("// End A68 prospective DAG replay core.", begin);
assert.ok(begin >= 0 && end > begin);
const dependencies = ["a61Equal", "a62Rat", "a62Text", "a62Degree", "a64Rat", "a64Tensor"].map(name => {
  const lines = source.split("\n").filter(line => new RegExp("^  const " + name + "\\s*=").test(line));
  assert.equal(lines.length, 1, "one pure dependency declaration: " + name); return lines[0];
}).join("\n");
const context = vm.createContext({ assert, Buffer, crypto, require: createRequire(path.join(repository, "package.json")) });
vm.runInContext(dependencies + "\n" + source.slice(begin, end) + "\nthis.verify = verifyA68MixedDag;", context,
  { filename: "a68-scalar-tensor-replay-extraction.js", timeout: 1000 });
const verify = context.verify;
const hash = value => crypto.createHash("sha256").update(JSON.stringify(value)).digest("hex");
const clone = value => JSON.parse(JSON.stringify(value));
const row = (form, blade, real, imaginary = "0") => ({ form, blade, k0: 0, k1: 0, real, imaginary });
const pair = (left, right) => ({ op: "pair", left, right });
const root = (name, expression) => ({ name, expression });
const top = node => ({ schemaVersion: TOP_SCHEMA, tensor: {}, node, degree: 14, form: 16383,
  blade: 0, k0: 0, k1: 0, absentCoefficient: "0", imaginaryRequired: "0", realFactor: "-1", weight: "1/3" });
const limits = overrides => ({ nodes: 100, marks: 100, tensorRecords: 20, rationalCharacters: 32,
  tensorBytes: 10000, graphBytes: 100000, pairVisits: 10000, slotVisits: 10000, liveRecords: 100, ...overrides });
const scalarConfig = (tensorPlan, namedRoots, compareRoot) => ({ tensorPlan, namedRoots, geometry: [],
  scheduleLimits: { tensorNodes: 100, tensorEdges: 1000, tensorMarks: 100, scalarNodes: 1000, scalarEdges: 1000,
    roots: 100, geometryMatrices: 100, geometryEntries: 1000, maxDepth: 64, stringCharacters: 100000, rationalCharacters: 100 },
  arithmeticLimits: { maxBits: 128, scalarOperations: 100000, rationalObjects: 100000, matrixObjects: 1, matrixCells: 1, slotVisits: 1000 },
  replayLimits: { tensorRecords: 100, tensorResolutions: 1000, recordVisits: 10000, coefficientReads: 10000,
    readCharacters: 100000, outputCharacters: 100000, rationalCharacters: 100, liveScalars: 1000 },
  wireLimits: { liveWireRecords: 100, sortReferences: 100, liveWireCharacters: 100000,
    serializationRecordVisits: 10000, serializationCharacters: 1000000 }, compareRoot });
const consumerConfig = overrides => ({ checks: [], domainChecks: [], error: null,
  scheduleLimits: { consumers: 100, expressionNodes: 1000, expressionEdges: 1000, tensorReferences: 1000, scalarReferences: 100,
    stringCharacters: 100000, maxDepth: 64, rationalCharacters: 100, retainedRootValues: 100 },
  arithmeticLimits: { maxBits: 128, scalarOperations: 100000, rationalObjects: 100000, matrixObjects: 100, matrixCells: 10000, slotVisits: 100000 },
  replayLimits: { tensorRecords: 100, tensorResolutions: 1000, parsedTensorRecords: 10000, recordVisits: 100000,
    geometryResolutions: 100, geometryCoordinates: 100000, arraySlots: 100000, coefficientReads: 100000, readCharacters: 100000,
    outputCharacters: 100000, rationalCharacters: 100, predicateNodes: 10000, rootValues: 100, errorValues: 100,
    resultValues: 100, consumerComparisons: 100, sourceResolutions: 100 },
  resolveGeometryField: () => { throw Error("unexpected manufactured geometry request"); },
  resolveSourceScalar: () => { throw Error("unexpected manufactured source request"); }, compareConsumer: () => true, ...overrides });
function graphFor(plan, tensors) {
  assert.equal(plan.nodes.length, tensors.length);
  return { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: clone(plan.leaves),
    nodes: plan.nodes.map((node, i) => ({ ...clone(node), records: tensors[i].length,
      bytes: Buffer.byteLength(JSON.stringify(tensors[i])), sha256: hash(tensors[i]) })),
    marks: plan.marks.map(mark => ({ ...mark, sha256: hash(tensors[mark.node]) })) };
}
function run(plan, tensors, roots, options = {}) {
  const events = [], graph = options.graph ?? graphFor(plan, tensors), tensorLimits = options.limits ?? limits();
  const config = options.scalarConfig ?? scalarConfig(plan, roots, (name, value) => { events.push(["scalar", name, value]); return true; });
  const result = verify(graph, options.expectedLeaves ?? plan.leaves, plan.marks, tensorLimits,
    options.resolveLeaf ?? (() => { throw new Error("no manufactured leaf supplied"); }),
    options.validateNode ?? (() => true), options.compareMark ?? ((mark, value) => {
      assert.ok(Object.isFrozen(value)); value.forEach(row => assert.ok(Object.isFrozen(row)));
      assert.equal(JSON.stringify(value), JSON.stringify(tensors[mark.node])); events.push(["mark", mark.name]); return true;
    }), config, options.consumerConfig ?? consumerConfig(), options.exportConfig ?? null);
  return { result, events, graph };
}
function delayedFixture() {
  const r = new MixedRecipe({ constants: ["0", "-1", ...Array.from({ length: 11 }, (_, i) => String(i + 1))] }), tensors = [];
  for (let i = 0; i < 11; i++) {
    const t = r.Unit(1, 0, String(i + 1)); tensors.push([row(1, 0, String(i + 1))]);
    if (i === 0 || i === 10) r.Mark("tensor" + i, 1, t, true);
  }
  return { plan: r.Finish(), tensors, roots: [root("early", pair(0, 1)), root("late0", pair(0, 10)), root("late1", pair(1, 10))] };
}

const exportConfig = requests => ({ contextId: "manufactured-point", requests,
  limits: { tensors: 30, records: 600, characters: 100000, recordVisits: 600 } });

test("complete replay receipts retain canonical early exports only after all consumers release", () => {
  const f = delayedFixture(), config = exportConfig([{ id: "first", degree: 1, node: 0 }]);
  const { result } = run(f.plan, f.tensors, f.roots, { exportConfig: config });
  const identity = readVerifiedReplayReceipt(result.exportReceipt);
  assert.deepEqual(identity.tensorPlan, f.plan); assert.deepEqual(identity.namedRoots, f.roots);
  assert.deepEqual(identity.requests, config.requests); assert.deepEqual(identity.exports[0].tensor, f.tensors[0]);
  assert.equal(identity.exports[0].sha256, hash(f.tensors[0]));
  assert.deepEqual(identity.completion, { tensor: true, scalar: true, consumers: true, released: true });
  assert.equal(identity.reservation.records, 20); assert.equal(identity.reservation.totalProcessMemoryProved, false);
  assert.ok(Object.isFrozen(identity)); assert.ok(Object.isFrozen(identity.exports[0].tensor[0]));
  for (const fake of [{ ...result.exportReceipt }, {}, null])
    assert.throws(() => readVerifiedReplayReceipt(fake), /private complete numerical replay receipt/);
});

test("receipts use pre-callback snapshots of plans roots checks and export requests", () => {
  const f = delayedFixture(), config = scalarConfig(clone(f.plan), clone(f.roots), () => true);
  const exports = exportConfig([{ id: "first", degree: 1, node: 0 }]);
  const consumers = consumerConfig({ checks: [{ name: "equal", metadataPath: "check/equal", predicate: {
    kind: "tensorEqual", left: { kind: "tensorNode", node: 0 }, right: { kind: "tensorNode", node: 0 } } }] });
  const { result } = run(f.plan, f.tensors, f.roots, { scalarConfig: config, consumerConfig: consumers, exportConfig: exports,
    validateNode: () => { config.tensorPlan.nodes[0].parameters.real = "99"; config.namedRoots[0].name = "changed";
      consumers.checks[0].name = "changed"; exports.requests[0].id = "changed"; return true; } });
  const identity = readVerifiedReplayReceipt(result.exportReceipt);
  assert.equal(identity.tensorPlan.nodes[0].parameters.real, "1"); assert.equal(identity.namedRoots[0].name, "early");
  assert.equal(identity.checks[0].name, "equal"); assert.equal(identity.requests[0].id, "first");
});

test("late root mark and consumer failures never return an export receipt", () => {
  for (const failure of ["root", "mark", "consumer"]) {
    const f = delayedFixture(); let returned = null, lateSeen = false;
    const options = { exportConfig: exportConfig([{ id: "early", degree: 1, node: 0 }]) };
    if (failure === "root") options.scalarConfig = scalarConfig(f.plan, f.roots, name => { if (name === "late1") { lateSeen = true; return false; } return true; });
    if (failure === "mark") options.compareMark = mark => { if (mark.node === 10) { lateSeen = true; return false; } return true; };
    if (failure === "consumer") options.consumerConfig = consumerConfig({ checks: [{ name: "late", metadataPath: "check/late", predicate: {
      kind: "tensorEqual", left: { kind: "tensorNode", node: 10 }, right: { kind: "tensorNode", node: 10 } } }],
      compareConsumer: () => { lateSeen = true; return false; } });
    assert.throws(() => { returned = run(f.plan, f.tensors, f.roots, options); });
    assert.equal(lateSeen, true); assert.equal(returned, null);
  }
});

test("export admission rejects duplicate roles nodes wrong degrees and low budgets before callbacks", () => {
  for (const change of [c => c.requests.push({ ...c.requests[0], node: 1 }),
    c => c.requests.push({ ...c.requests[0], id: "other" }), c => c.requests[0].degree = 2,
    c => c.limits.records = 19, c => c.limits.recordVisits = 19, c => c.limits.characters = 1]) {
    const f = delayedFixture(), config = exportConfig([{ id: "first", degree: 1, node: 0 }]); change(config); let calls = 0;
    assert.throws(() => run(f.plan, f.tensors, f.roots, { exportConfig: config, validateNode: () => { calls++; return true; } }), /export/);
    assert.equal(calls, 0);
  }
});

test("equal exports from distinct nodes remain distinct immutable arrays", () => {
  const r = new MixedRecipe(); r.Unit(1, 0); r.Unit(1, 0); const rows = [row(1, 0, "1")];
  const { result } = run(r.Finish(), [rows, rows], [], { exportConfig: exportConfig([
    { id: "one", degree: 1, node: 0 }, { id: "two", degree: 1, node: 1 }]) });
  const identity = readVerifiedReplayReceipt(result.exportReceipt);
  assert.equal(identity.exports[0].sha256, identity.exports[1].sha256);
  assert.notEqual(identity.exports[0].tensor, identity.exports[1].tensor);
});

test("an unknown-degree empty node can supply a declared typed zero export", () => {
  const r = new MixedRecipe(); r.Zero();
  const { result } = run(r.Finish(), [[]], [], { exportConfig: exportConfig([{ id: "typed-zero", degree: 14, node: 0 }]) });
  const identity = readVerifiedReplayReceipt(result.exportReceipt);
  assert.equal(identity.tensorPlan.nodes[0].degree, -1);
  assert.equal(identity.exports[0].degree, 14); assert.deepEqual(identity.exports[0].tensor, []);
});

test("receipt metadata rejects accessors without invoking them", () => {
  const f = delayedFixture(), config = exportConfig([{ id: "first", degree: 1, node: 0 }]); let invoked = false;
  Object.defineProperty(config.requests[0], "id", { enumerable: true, get() { invoked = true; return "first"; } });
  assert.throws(() => run(f.plan, f.tensors, f.roots, { exportConfig: config }), /own data field/);
  assert.equal(invoked, false);
});

test("integrated replay retains scalar-only tensor dependencies and releases unrelated old nodes globally", () => {
  const f = delayedFixture(), { result, events, graph } = run(f.plan, f.tensors, f.roots);
  assert.deepEqual(events, [["mark", "tensor0"], ["scalar", "early", "-2"], ["mark", "tensor10"],
    ["scalar", "late0", "-11"], ["scalar", "late1", "-22"]]);
  assert.equal(result.nodes, 11); assert.equal(result.marks, 2); assert.equal(result.maximumReplayOwnedTensorRecords, 3);
  assert.equal(result.scalarReplay.complete, true); assert.equal(result.scalarReplay.rootsCompared, 3);
  assert.equal(result.wireStorage.serializationRecordVisits, 14, "eleven current serializations plus three older operand conversions");
  assert.equal(result.wireStorage.reservedLogicalWireRecords, 60);
  assert.equal(result.wireStorage.reservedSortReferences, 20);
  assert.equal(result.wireStorage.reservedLogicalWireCharacters, 7814);
  assert.equal(result.wireStorage.totalProcessMemoryProved, false); assert.equal(result.wireStorage.callbackRetentionIncluded, false);
  assert.equal(Object.isFrozen(graph), false, "caller graph is not mutated; replay owns a frozen snapshot");
});

test("integrated replay reuses frozen current wire for repeated same-node Pair operands", () => {
  const r = new MixedRecipe(); r.Unit(0, 0, "0", "1"); const plan = r.Finish();
  const { result, events } = run(plan, [[row(0, 0, "0", "1")]], [root("central", pair(0, 0))]);
  assert.deepEqual(events, [["scalar", "central", "1"]]); assert.equal(result.wireStorage.serializationRecordVisits, 1);
  assert.equal(result.scalarReplay.usage.tensorResolutions, 2);
});

test("integrated replay reconstructs tensor primitives before TOP extraction and scalar comparison", () => {
  const r = new MixedRecipe({ constants: ["0", "1", "3"] }); const x = r.Unit(0, 0, "3"), t = r.Hodge(x); r.Mark("top", 14, t, true);
  const { result, events } = run(r.Finish(), [[row(0, 0, "3")], [row(16383, 0, "3")]], [root("density", top(1))]);
  assert.deepEqual(events, [["mark", "top"], ["scalar", "density", "-1"]]); assert.equal(result.scalarReplay.complete, true);
});

test("integrated replay runs constant roots at ready minus one and finishes empty tensor graphs", () => {
  const r = new MixedRecipe(), { result, events } = run(r.Finish(), [], [root("constant", { op: "constant", value: "2/3" })]);
  assert.deepEqual(events, [["scalar", "constant", "2/3"]]); assert.equal(result.nodes, 0);
  assert.equal(result.wireStorage.serializationRecordVisits, 0); assert.equal(result.scalarReplay.complete, true);
});

test("integrated replay rejects a self-consistent observed graph that differs from independent recipe", () => {
  const f = delayedFixture(), graph = graphFor(f.plan, f.tensors);
  graph.nodes[10].parameters.real = "99";
  const changed = [row(1, 0, "99")]; graph.nodes[10].sha256 = hash(changed); graph.nodes[10].bytes = Buffer.byteLength(JSON.stringify(changed));
  graph.marks[1].sha256 = hash(changed);
  assert.throws(() => run(f.plan, f.tensors, f.roots, { graph }), /independent primitive expression/);
  assert.throws(() => verify(graph, [], [], limits(), () => [], () => true, () => true), /closed object/);
});

test("integrated replay catches malformed full tensor hashes/support and scalar reality failures", () => {
  for (const mutate of [g => { g.nodes[0].sha256 = "f".repeat(64); }, g => { g.nodes[0].records = 2; },
    g => { g.nodes[0].bytes++; }, g => { g.marks[0].sha256 = "f".repeat(64); }]) {
    const f = delayedFixture(), graph = graphFor(f.plan, f.tensors); mutate(graph);
    assert.throws(() => run(f.plan, f.tensors, f.roots, { graph }), /intermediate|support|content/);
  }
  const r = new MixedRecipe(); r.Unit(0, 0, "1", "1");
  assert.throws(() => run(r.Finish(), [[row(0, 0, "1", "1")]], [root("nonreal", pair(0, 0))]), /imaginary trace/);
});

test("integrated replay admits wire support/sort/string reserves before callbacks or conversion", () => {
  for (const [key, value] of [["liveWireRecords", 59], ["sortReferences", 19], ["liveWireCharacters", 7813]]) {
    const f = delayedFixture(), config = scalarConfig(f.plan, f.roots, () => true); config.wireLimits[key] = value;
    let callbacks = 0;
    assert.throws(() => run(f.plan, f.tensors, f.roots, { scalarConfig: config, validateNode: () => { callbacks++; return true; } }), /wire admission/);
    assert.equal(callbacks, 0);
  }
  const f = delayedFixture(), config = scalarConfig(f.plan, f.roots, () => true); config.wireLimits.serializationRecordVisits = 1;
  assert.throws(() => run(f.plan, f.tensors, f.roots, { scalarConfig: config }), /BEFORE array/);
  const other = scalarConfig(f.plan, f.roots, () => true); other.wireLimits.serializationCharacters = 1;
  assert.throws(() => run(f.plan, f.tensors, f.roots, { scalarConfig: other }), /BEFORE array/);
});

test("integrated replay protects frozen row/graph data and snapshots limits and scalar inputs before callbacks", () => {
  const f = delayedFixture(), graph = graphFor(f.plan, f.tensors), outerLimits = limits();
  const { result } = run(f.plan, f.tensors, f.roots, { graph,
    validateNode: node => { assert.ok(Object.isFrozen(node)); assert.ok(Object.isFrozen(node.parameters)); return true; },
    compareMark: () => { graph.nodes[10].parameters.real = "99"; return true; } });
  assert.equal(result.scalarReplay.complete, true, "caller graph mutations cannot alter the independent snapshot");
  const g = delayedFixture();
  assert.throws(() => run(g.plan, g.tensors, g.roots, { compareMark: (mark, rows) => { rows[0].real = "99"; return true; } }), /read only/);
  const h = delayedFixture(), config = scalarConfig(clone(h.plan), clone(h.roots), () => true);
  config.wireLimits.serializationRecordVisits = 1;
  assert.throws(() => run(h.plan, h.tensors, h.roots, { limits: outerLimits, scalarConfig: config, validateNode: () => {
    outerLimits.liveRecords = 100000; config.wireLimits.serializationRecordVisits = 100000;
    config.tensorPlan.nodes[10].parameters.real = "99"; config.namedRoots[0].expression.left = 10; return true;
  } }), /BEFORE array/, "wire limit snapshot cannot be increased from callback");
});

test("integrated replay consumes bounded independently resolved synthetic leaves", () => {
  const leafRows = [row(1, 0, "2/3")], r = new MixedRecipe({ leaves: [{ id: "synthetic", degree: 1, source: "manufactured-only", sha256: hash(leafRows) }] });
  r.RegisterLeaf("synthetic"); const p = r.Finish();
  const { events } = run(p, [leafRows], [root("norm", pair(0, 0))], { resolveLeaf: () => leafRows });
  assert.deepEqual(events, [["scalar", "norm", "-4/9"]]);
  assert.throws(() => run(p, [leafRows], [root("norm", pair(0, 0))], { resolveLeaf: () => [row(1, 0, "1")] }), /leaf content/);
});

test("integrated leaf hash and import use the same canonical snapshot, ignoring inherited serialization", () => {
  const pinned = [row(1, 0, "1")], injected = [row(1, 0, "2")];
  const r = new MixedRecipe({ leaves: [{ id: "source", degree: 1, source: "manufactured-only", sha256: hash(pinned) }] });
  r.RegisterLeaf("source"); const plan = r.Finish(), malicious = Object.assign(Object.create({ toJSON: () => pinned[0] }), injected[0]);
  assert.equal(hash([malicious]), hash(pinned), "manufactured inherited serializer would conceal changed own fields");
  assert.throws(() => run(plan, [injected], [], { resolveLeaf: () => [malicious] }), /independent leaf content/);
  let invoked = false; const getter = { ...pinned[0] };
  Object.defineProperty(getter, "real", { enumerable: true, get() { invoked = true; return "1"; } });
  assert.throws(() => run(plan, [pinned], [], { resolveLeaf: () => [getter] }), /own data fields/);
  const array = []; Object.defineProperty(array, "0", { enumerable: true, get() { invoked = true; return pinned[0]; } });
  assert.throws(() => run(plan, [pinned], [], { resolveLeaf: () => array }), /own data indices/);
  assert.equal(invoked, false, "accessors rejected without invocation");
});

test("integrated leaf provenance is immutable even when external expected-leaf descriptors are changed by callbacks", () => {
  const pinned = [row(1, 0, "1")], changed = [row(1, 0, "2")];
  const r = new MixedRecipe({ leaves: [{ id: "source", degree: 1, source: "manufactured-only", sha256: hash(pinned) }] });
  r.RegisterLeaf("source"); const plan = r.Finish(), expectedLeaves = clone(plan.leaves);
  assert.throws(() => run(plan, [changed], [], { expectedLeaves,
    validateNode: () => { expectedLeaves[0].sha256 = hash(changed); expectedLeaves[0].source = "mutated"; return true; },
    resolveLeaf: leaf => { assert.equal(leaf.sha256, hash(pinned)); assert.equal(leaf.source, "manufactured-only"); assert.ok(Object.isFrozen(leaf)); return changed; }
  }), /independent leaf content/);
});

test("integrated leaf snapshot captures its admitted length once before copying", () => {
  const rows = [row(1, 0, "1")], r = new MixedRecipe({ leaves: [{ id: "source", degree: 1, source: "manufactured-only", sha256: hash(rows) }] });
  r.RegisterLeaf("source"); let reads = 0;
  const proxy = new Proxy(rows, { get(target, key, receiver) { return key === "length" ? (++reads === 1 ? 1 : 1000000) : Reflect.get(target, key, receiver); } });
  const { result } = run(r.Finish(), [rows], [], { resolveLeaf: () => proxy });
  assert.equal(reads, 1); assert.equal(result.slotVisits, 2, "one snapshot plus one import scan");
  assert.equal(result.wireStorage.serializationRecordVisits, 2, "one snapshot plus one result serialization");
});

test("integrated full coefficient checks keep early tensors beyond scalar-only last use", () => {
  const r = new MixedRecipe(), tensors = [];
  for (let i = 0; i < 6; i++) { r.Unit(1, 1); tensors.push([row(1, 1, "1")]); }
  const checks = [{ name: "lateEquality", metadataPath: "check/lateEquality", predicate: { kind: "tensorEqual",
    left: { kind: "tensorNode", node: 0 }, right: { kind: "tensorNode", node: 5 } } }];
  const events = [], { result } = run(r.Finish(), tensors, [], { consumerConfig: consumerConfig({ checks,
    compareConsumer: (c, value) => { events.push([c.name, value]); return true; } }) });
  assert.deepEqual(events, [["lateEquality", true]]); assert.equal(result.consumerReplay.complete, true);
  assert.equal(result.consumerReplay.consumersExecuted, 1); assert.equal(result.consumerReplay.usage.tensorResolutions, 2);
  assert.equal(result.maximumReplayOwnedTensorRecords, 2);
  assert.equal(result.wireStorage.serializationRecordVisits, 7, "six current tensors and one retained early operand");
});

test("integrated consumer roots and domain scans run after exact scalar/tensor reconstruction", () => {
  const r = new MixedRecipe(), x = r.Unit(1, 1); r.Mark("x", 1, x, true); const plan = r.Finish(), events = [];
  const roots = [root("one", { op: "constant", value: "1" })];
  const checks = [{ name: "anchor", metadataPath: "check/anchor", predicate: { kind: "scalarRootEqualsRealCoefficient", root: "one",
    coefficient: { tensor: { kind: "tensorNode", node: 0 }, form: 1, blade: 1, k0: 0, k1: 0, absent: "0" }, factor: "1" } }];
  const config = consumerConfig({ checks, domainChecks: [{ name: "domain", node: 0, degree: 1, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true }],
    compareConsumer: (c, v) => { events.push([c.kind, v]); return true; } });
  const { result } = run(plan, [[row(1, 1, "1")]], roots, { consumerConfig: config });
  assert.deepEqual(events, [["domain", true], ["check", true]]); assert.equal(result.consumerReplay.maximumRootValues, 1);
  assert.equal(result.consumerReplay.liveRootValues, 0);
});

test("integrated consumers reject false or mutated checks even with self-consistent graph hashes", () => {
  const r = new MixedRecipe({ constants: ["0", "1", "2"] }); r.Unit(1, 1); r.Unit(1, 1, "2"); const plan = r.Finish(), tensors = [[row(1, 1, "1")], [row(1, 1, "2")]];
  const checks = [{ name: "same", metadataPath: "check/same", predicate: { kind: "tensorEqual", left: { kind: "tensorNode", node: 0 }, right: { kind: "tensorNode", node: 1 } } }];
  const config = consumerConfig({ checks });
  assert.throws(() => run(plan, tensors, [], { consumerConfig: config, validateNode: () => { checks[0].predicate.right.node = 0; return true; } }), /numerical predicate failed/);
  checks[0].predicate.right.node = 1;
  assert.throws(() => run(plan, tensors, [], { consumerConfig: consumerConfig({ checks, compareConsumer: () => false }) }), /comparison rejected/);
  assert.throws(() => verify(graphFor(plan, tensors), [], [], limits(), () => [], () => true, () => true, scalarConfig(plan, [], () => true)), /closed object/);
});

function metadataFixture() {
  const recipe = new MixedRecipe(); recipe.Unit(1, 1); const plan = recipe.Finish(), tensors = [[row(1, 1, "1")]];
  const namedRoots = [root("scalars/out/0", { op: "constant", value: "2" }), root("structured/Original/a/Scalars/Value/0", { op: "constant", value: "3" }), root("check/equal/reference", { op: "constant", value: "2" })];
  const checks = [{ name: "equal", metadataPath: "check/equal", predicate: { kind: "scalarEqual", left: namedRoots[0].name, right: namedRoots[2].name } }];
  const error = { schemaVersion: "phase627-mixed-error-formula-obligations-v1", numericalReplayImplemented: false,
    fields: Object.fromEntries(ERROR_FIELDS.map(name => [name, name === "nativeFirstJetNorms" ? Array.from({ length: 14 }, () => ({ op: "constant", value: "1" })) : { op: "constant", value: "1" }])) };
  const expected = { contextId: "manufactured", tensorPlan: plan, namedRoots, geometry: [], checks, domainChecks: [], error,
    structured: [{ category: "Original", name: "a", metadataPath: "structured/Original/a", fields: [
      { category: "ScalarArray", name: "Scalars/Value", degree: null, expanded: null, length: 2 },
      { category: "Boolean", name: "Oracle", degree: null, expanded: null, length: null }],
      scalarFields: { "Scalars/Value": [namedRoots[1].name, namedRoots[1].name] }, booleanFields: { Oracle: false } }],
    scalarArrays: [{ name: "out", metadataPath: "scalars/out", roots: [namedRoots[0].name] }], geometryPaths: ["geometry/geometry"] };
  const observed = { "geometry/geometry": {}, "check/equal": true, "scalars/out": ["2"], "structured/Original/a": { "Scalars/Value": ["3", "3"], Oracle: false },
    error: Object.fromEntries(ERROR_FIELDS.map(name => [name, name === "nativeFirstJetNorms" ? Array(14).fill("1") : "1"])) };
  const scalar = scalarConfig(plan, namedRoots, () => true), consumers = consumerConfig({ checks, error });
  const caps = { snapshot: { nodes: 100000, arraySlots: 100000, stringCharacters: 1000000, maxDepth: 64 },
    scalarSchedule: scalar.scheduleLimits, consumerSchedule: consumers.scheduleLimits, results: { rationalCharacters: 100, totalCharacters: 100000 } };
  return { plan, tensors, expected, observed, caps, scalar, consumers };
}
function runMetadata(f) {
  const comparator = createContextMetadataComparator(f.expected, f.observed, f.caps);
  const replay = run(f.plan, f.tensors, f.expected.namedRoots, { scalarConfig: { ...f.scalar, compareRoot: comparator.compareRoot },
    consumerConfig: { ...f.consumers, compareConsumer: comparator.compareConsumer } });
  return { replay, result: comparator.finish(), comparator };
}
test("actual scalar/error/predicate replay binds every recorded array and literal structured field", () => {
  const f = metadataFixture(), { result, replay, comparator } = runMetadata(f);
  assert.equal(result.roots, 3); assert.equal(result.recordedRootBindings, 3); assert.equal(result.consumers, ERROR_FIELDS.length + 1);
  assert.equal(replay.result.consumerReplay.complete, true); assert.equal(replay.result.scalarReplay.complete, true);
  assert.ok(Object.values(result.scope).every(value => value === false)); assert.deepEqual(comparator.geometryMetadata, { "geometry/geometry": {} });
  assert.throws(() => comparator.finish(), /closed/);
});
test("recorded scalar or error changes cannot pass despite all producer checks being true", () => {
  for (const change of [m => m["scalars/out"][0] = "9", m => m["structured/Original/a"]["Scalars/Value"][1] = "4",
    m => m.error.radius = "2", m => m.error.nativeFirstJetNorms[13] = "2"]) {
    const f = metadataFixture(); change(f.observed); assert.throws(() => runMetadata(f), /recomputed.*metadata/);
  }
});
test("true producer flags cannot replace a recomputed false predicate", () => {
  const f = metadataFixture(); f.expected.namedRoots[2].expression.value = "4";
  assert.throws(() => runMetadata(f), /recomputed predicate/);
});
test("metadata rejects missing extra false and mis-shaped scalar route and error declarations before replay", () => {
  const changes = [m => delete m["scalars/out"], m => m.extra = true, m => m["check/equal"] = false, m => m["check/equal"] = "true",
    m => m["structured/Original/a"].Oracle = true, m => m["structured/Original/a"]["Scalars/Value"].pop(),
    m => m["structured/Original/a"].Scalars = { Value: ["3", "3"] }, m => delete m.error.radius,
    m => m.error.nativeFirstJetNorms.pop(), m => m.error.extra = "0", m => m["scalars/out"][0] = "2/1"];
  for (const change of changes) { const f = metadataFixture(); change(f.observed); assert.throws(() => createContextMetadataComparator(f.expected, f.observed, f.caps)); }
});
test("metadata comparisons use immutable own-data snapshots and ignore inherited toJSON", () => {
  const f = metadataFixture(); let calls = 0; Object.setPrototypeOf(f.observed["scalars/out"], { toJSON() { calls++; return ["99"]; } });
  const comparator = createContextMetadataComparator(f.expected, f.observed, f.caps);
  f.observed["scalars/out"][0] = "99"; f.observed["structured/Original/a"].Oracle = true;
  run(f.plan, f.tensors, f.expected.namedRoots, { scalarConfig: { ...f.scalar, compareRoot: comparator.compareRoot }, consumerConfig: { ...f.consumers, compareConsumer: comparator.compareConsumer } });
  assert.equal(comparator.finish().roots, 3); assert.equal(calls, 0);
  const g = metadataFixture(); Object.defineProperty(g.observed, "check/equal", { enumerable: true, get() { calls++; return true; } });
  assert.throws(() => createContextMetadataComparator(g.expected, g.observed, g.caps), /own data/); assert.equal(calls, 0);
});
test("omitted consumer or root callbacks and forged descriptors cannot mint complete metadata", () => {
  const f = metadataFixture(), early = createContextMetadataComparator(f.expected, f.observed, f.caps);
  assert.throws(() => early.finish(), /all independent/); assert.throws(() => early.compareRoot("x", "2", {}), /failed/);
  const forged = createContextMetadataComparator(f.expected, f.observed, f.caps);
  assert.throws(() => forged.compareRoot(f.expected.namedRoots[0].name, "2", {}), /exact unique/);
  const wrong = createContextMetadataComparator(f.expected, f.observed, f.caps);
  assert.throws(() => wrong.compareConsumer({ id: 0, kind: "error", name: "radius" }, "1"), /exact unique/);
});
test("duplicate callback failures cannot be repaired or ignored", () => {
  const f = metadataFixture(), comparator = createContextMetadataComparator(f.expected, f.observed, f.caps);
  const schedule = compileAuditConsumerSchedule({ tensorPlan: f.plan, namedRoots: f.expected.namedRoots, geometry: [], checks: f.expected.checks, domainChecks: [], error: f.expected.error,
    scheduleLimits: f.caps.scalarSchedule, limits: f.caps.consumerSchedule });
  const descriptor = schedule.scalarSchedule.rootComparisons.find(r => r.name === f.expected.namedRoots[0].name);
  assert.equal(comparator.compareRoot(descriptor.name, "2", descriptor), true);
  assert.throws(() => comparator.compareRoot(descriptor.name, "2", descriptor), /exact unique/); assert.throws(() => comparator.finish(), /failed/);
});
test("orphan roots invalid quotas and metadata path aliases fail before replay", () => {
  const f = metadataFixture(); f.expected.checks = []; delete f.observed["check/equal"];
  assert.throws(() => createContextMetadataComparator(f.expected, f.observed, f.caps), /unretained root/);
  for (const change of [f => f.caps.results.totalCharacters = 1, f => f.caps.snapshot.nodes = 1,
    f => f.expected.geometryPaths.push("scalars/out"), f => f.expected.structured[0].scalarFields["Scalars/Value"][0] = "unknown"]) {
    const g = metadataFixture(); change(g); assert.throws(() => createContextMetadataComparator(g.expected, g.observed, g.caps));
  }
});

test("callback snapshot/result quotas and swallowed reentry remain fail-closed across the complete session", () => {
  const f = metadataFixture(), baseline = createContextMetadataComparator(f.expected, f.observed, f.caps).snapshot();
  f.caps.results.totalCharacters = baseline.resultCharacters + 1; assert.throws(() => runMetadata(f), /result character quota/);
  const g = metadataFixture(), initial = createContextMetadataComparator(g.expected, g.observed, g.caps).snapshot();
  g.caps.snapshot.nodes = initial.snapshotUsage.nodes + 1; assert.throws(() => runMetadata(g), /ceiling/);
  const h = metadataFixture(), comparator = createContextMetadataComparator(h.expected, h.observed, h.caps); let invoked = false;
  const descriptor = new Proxy({}, { ownKeys(target) { invoked = true; assert.throws(() => comparator.finish(), /reentrant/); return Reflect.ownKeys(target); } });
  assert.throws(() => comparator.compareRoot(h.expected.namedRoots[0].name, "2", descriptor), /snapshot reentry/);
  assert.equal(invoked, true); assert.equal(comparator.snapshot().failed, true); assert.equal(comparator.snapshot().roots, 0);
});

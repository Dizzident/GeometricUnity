"use strict";
// Hand-enumerated tiny tensors only; no source geometry, polynomial or phase
// entry point runs. Generic positives deliberately do not establish provenance.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { ERROR_FIELDS } = require("../a68-audit-consumer-schedule");
const { readContextCheckpoint } = require("../a68-context-checkpoint");
const { replayContextCheckpoint } = require("../a68-context-replay");
const hash = bytes => crypto.createHash("sha256").update(bytes).digest("hex");
const wire = value => Buffer.from(JSON.stringify(value) + "\n");
const snapshot = () => ({ nodes: 100000, arraySlots: 100000, stringCharacters: 1000000, maxDepth: 16 });
function limits() {
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
    metadata: { snapshot: snapshot(), results: { rationalCharacters: 100, totalCharacters: 100000 } },
    expanded: { marks: 100, fileBytes: 10000, totalReadBytes: 100000, totalRecords: 1000, totalStringCharacters: 100000, totalSerializedBytes: 1000000, rationalCharacters: 32, snapshot: snapshot() }, snapshot: snapshot() };
}
function fixture(t, contextId = "point0/m0_j0") {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-context-replay-")); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const tensor = [{ form: 1, blade: 1, k0: 0, k1: 0, real: "1", imaginary: "0" }], tensorText = JSON.stringify(tensor), digest = hash(tensorText);
  const recipe = new MixedRecipe({ leaves: [{ id: "tiny", degree: 1, source: "manufactured-not-source", sha256: digest }] });
  const leaf = recipe.RegisterLeaf("tiny"), unit = recipe.Unit(1, 1); recipe.Mark("expanded", 1, leaf, true); recipe.Mark("dag", 1, unit, false);
  const plan = recipe.Finish();
  const graph = { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: plan.leaves,
    nodes: plan.nodes.map(n => ({ ...n, records: 1, bytes: tensorText.length, sha256: digest })), marks: plan.marks.map(m => ({ ...m, sha256: digest })) };
  const namedRoots = [{ name: "out", expression: { op: "constant", value: "2" } }, { name: "reference", expression: { op: "constant", value: "2" } }];
  const checks = [{ name: "scalar", metadataPath: "check/scalar", predicate: { kind: "scalarEqual", left: "out", right: "reference" } },
    { name: "tensor", metadataPath: "check/tensor", predicate: { kind: "tensorEqual", left: { kind: "tensorNode", node: 0 }, right: { kind: "tensorNode", node: 1 } } }];
  const error = { schemaVersion: "phase627-mixed-error-formula-obligations-v1", numericalReplayImplemented: false,
    fields: Object.fromEntries(ERROR_FIELDS.map(name => [name, name === "nativeFirstJetNorms" ? Array.from({ length: 14 }, () => ({ op: "constant", value: "1" })) : { op: "constant", value: "1" }])) };
  const expected = { contextId, tensorPlan: plan, namedRoots, geometry: [], checks,
    domainChecks: [{ name: "tiny", node: 0, degree: 1, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true }], error,
    structured: [], scalarArrays: [{ name: "out", metadataPath: "scalars/out", roots: ["out"] }], geometryPaths: ["geometry/geometry"] };
  const metadata = { "geometry/geometry": { manufactured: true }, "check/scalar": true, "check/tensor": true, "scalars/out": ["2"],
    error: Object.fromEntries(ERROR_FIELDS.map(name => [name, name === "nativeFirstJetNorms" ? Array(14).fill("1") : "1"])) };
  const declaration = { contextId, graphPath: "graph.json", metadataPath: "metadata.json",
    marks: [{ name: "expanded", degree: 1, expanded: true, relativePath: "tensor.json" }, { name: "dag", degree: 1, expanded: false, relativePath: null }] };
  function artifact(file, bytes) { fs.writeFileSync(path.join(root, file), bytes); return { path: file, bytes: bytes.length, sha256: hash(bytes) }; }
  artifact("tensor.json", wire(tensor)); const graphPin = artifact("graph.json", wire(graph));
  const envelope = { schema: "phase627-context-computational-evidence-v1", context: contextId, status: "producer-complete", independentValidationComplete: false, graph: graphPin, metadata };
  const pins = { context: contextId, graph: graphPin, metadata: artifact("metadata.json", wire(envelope)) }, events = [];
  const resolvers = { compareGeometryMetadata: observed => { events.push("geometry"); assert.deepEqual(observed, { "geometry/geometry": { manufactured: true } }); assert.ok(Object.isFrozen(observed)); },
    resolveLeaf: specification => { events.push("leaf"); assert.deepEqual(specification, plan.leaves[0]); return tensor; },
    resolveGeometryField: () => { throw Error("no external geometry predicate in fixture"); }, resolveSourceScalar: () => { throw Error("no source scalar in fixture"); } };
  return { root, tensor, graph, expected, metadata, declaration, envelope, pins, events, resolvers, artifact,
    rewrite: () => { pins.metadata = artifact("metadata.json", wire(envelope)); },
    read: () => readContextCheckpoint(root, declaration, pins, { graphBytes: 100000, metadataBytes: 100000, totalReadBytes: 200002, snapshot: snapshot() }) };
}
const run = (f, token = f.read(), cap = limits()) => replayContextCheckpoint(token, f.expected, cap, f.resolvers);

test("combined pinned replay checks numerical tensors scalar error predicates and expanded files without source authority", t => {
  const f = fixture(t), result = run(f);
  assert.deepEqual(f.events, ["geometry", "leaf"]); assert.equal(result.replay.nodes, 2); assert.equal(result.replay.marks, 2);
  assert.equal(result.metadataComparison.roots, 2); assert.equal(result.metadataComparison.consumers, ERROR_FIELDS.length + 3);
  assert.equal(result.expandedArtifacts.expandedFiles, 1); assert.equal(result.scope.numericalReplayComplete, true);
  for (const key of ["sourceAuthenticityEstablished", "independentContextAccepted", "upstreamCertificateProofEstablished", "totalProcessMemoryProved", "scientificExecutionAuthorized"]) assert.equal(result.scope[key], false);
  assert.ok(Object.isFrozen(result.replay)); assert.equal(f.envelope.independentValidationComplete, false);
});
test("same bridge handles fixed diagnostic context identity but does not certify the diagnostic source recipe", t => {
  for (const name of ["grade10", "acceleration", "secondJets"]) { const f = fixture(t, "diagnostic/" + name); assert.equal(run(f).scope.sourceAuthenticityEstablished, false); }
});
test("correct graph and true flags cannot hide altered scalar or any error metadata", t => {
  for (const change of [m => m["scalars/out"][0] = "3", m => m.error.radius = "2", m => m.error.nativeFirstJetNorms[13] = "2"]) {
    const f = fixture(t); change(f.metadata); f.rewrite(); assert.throws(() => run(f), /recomputed.*metadata/);
  }
});
test("true recorded checks cannot hide false independently recomputed predicates or domains", t => {
  const f = fixture(t); f.expected.namedRoots[1].expression.value = "3"; assert.throws(() => run(f), /recomputed predicate/);
  const g = fixture(t); g.expected.checks[1].predicate = { kind: "tensorZero", tensor: { kind: "tensorNode", node: 0 } }; assert.throws(() => run(g), /recomputed predicate/);
  const h = fixture(t); h.expected.domainChecks[0].degree = 2; assert.throws(() => run(h));
});
test("complete replay requires actual retained file bytes not matching graph hashes alone", t => {
  for (const mode of ["missing", "coefficient", "noLF", "whitespace"]) {
    const f = fixture(t), token = f.read();
    if (mode === "missing") fs.unlinkSync(path.join(f.root, "tensor.json"));
    else f.artifact("tensor.json", mode === "coefficient" ? wire([{ ...f.tensor[0], real: "2" }]) : Buffer.from((mode === "whitespace" ? " " : "") + JSON.stringify(f.tensor)));
    assert.throws(() => run(f, token));
  }
});
test("independent expression source leaves marks and frozen retention cannot be replaced", t => {
  for (const change of [f => { f.expected.tensorPlan = JSON.parse(JSON.stringify(f.expected.tensorPlan)); f.expected.tensorPlan.nodes[1].parameters.real = "2"; },
    f => { f.resolvers.resolveLeaf = () => [{ ...f.tensor[0], real: "2" }]; }, f => { f.declaration.marks[0].expanded = false; f.declaration.marks[0].relativePath = null; },
    f => { f.declaration.marks[0].relativePath = "graph.json"; }, f => f.declaration.marks.pop()]) {
    const f = fixture(t); change(f); assert.throws(() => run(f));
  }
});
test("missing extra and malformed complete metadata fail before geometry or numerical leaf callbacks", t => {
  for (const change of [m => delete m["check/scalar"], m => m.extra = true, m => m["check/tensor"] = false, m => delete m.error.radius]) {
    const f = fixture(t); change(f.metadata); f.rewrite(); assert.throws(() => run(f)); assert.deepEqual(f.events, []);
  }
});
test("geometry comparison is mandatory before leaf resolution and must complete synchronously", t => {
  for (const value of [true, false, {}, Promise.resolve(), { then() {} }]) {
    const f = fixture(t); f.resolvers.compareGeometryMetadata = () => value;
    assert.throws(() => run(f), /synchronous void/); assert.deepEqual(f.events, []);
  }
  const f = fixture(t); f.resolvers.compareGeometryMetadata = () => { throw Error("geometry mismatch"); };
  assert.throws(() => run(f), /geometry mismatch/); assert.deepEqual(f.events, []);
});
test("failed complete and swallowed reentrant attempts cannot reuse the same pinned checkpoint", t => {
  const f = fixture(t), token = f.read(); run(f, token); assert.throws(() => run(f, token), /completed checkpoint/);
  const g = fixture(t), failed = g.read(); g.resolvers.compareGeometryMetadata = () => { throw Error("reject"); }; assert.throws(() => run(g, failed), /reject/);
  g.resolvers.compareGeometryMetadata = () => {}; assert.throws(() => run(g, failed), /failed/);
  for (const stage of ["compareGeometryMetadata", "resolveLeaf"]) {
    const h = fixture(t), reentrant = h.read(), original = h.resolvers[stage];
    h.resolvers[stage] = (...args) => { assert.throws(() => run(h, reentrant), /reentrant/); return original(...args); };
    assert.throws(() => run(h, reentrant), /swallowed checkpoint reentry/);
  }
});
test("expected plan limits and resolver functions are captured before geometry callbacks", t => {
  const f = fixture(t), cap = limits(), original = f.resolvers.compareGeometryMetadata;
  f.resolvers.compareGeometryMetadata = value => { original(value); f.expected.namedRoots[0].expression.value = "999"; cap.tensor.nodes = 1; f.resolvers.resolveLeaf = () => []; };
  assert.equal(run(f, f.read(), cap).scope.numericalReplayComplete, true);
});
test("forged checkpoint wrong context accessors and resource exhaustion cannot trigger geometry callbacks", t => {
  const f = fixture(t); assert.throws(() => run(f, { ...f.read() }), /private read-pinned/);
  const g = fixture(t); g.expected.contextId = "point1/m0_j0"; assert.throws(() => run(g), /same independent context/);
  const h = fixture(t); let called = false; Object.defineProperty(h.resolvers, "resolveLeaf", { enumerable: true, get() { called = true; return () => []; } });
  assert.throws(() => run(h), /own data/); assert.equal(called, false);
  for (const change of [c => c.snapshot.nodes = 1, c => c.metadata.snapshot.nodes = 1, c => c.expanded.marks = 1]) {
    const f = fixture(t), cap = limits(); change(cap); assert.throws(() => run(f, f.read(), cap)); assert.deepEqual(f.events, []);
  }
});

function externalConsumers(f) {
  const name = "geometry/geometry.motion";
  f.expected.checks.push({ name: "geometry", metadataPath: "check/geometry", predicate: { kind: "geometryZero", field: name } });
  f.metadata["check/geometry"] = true;
  f.expected.error.fields.fieldError = { op: "sourceBoundScalar", binding: "phase626/certifiedEpsilon", positiveRequired: true };
  f.metadata.error.fieldError = "1/2";
  const matrix = Object.freeze(Array.from({ length: 14 }, () => Object.freeze(Array(14).fill("0"))));
  f.resolvers.resolveGeometryField = field => { assert.equal(field, name); f.events.push("field"); return matrix; };
  f.resolvers.resolveSourceScalar = field => { assert.equal(field, "phase626/certifiedEpsilon"); f.events.push("source"); return "1/2"; };
  f.rewrite(); return f;
}
test("external geometry and positive source-bound error consumers run after pinned geometry comparison", t => {
  const f = externalConsumers(fixture(t)), result = run(f);
  assert.equal(f.events[0], "geometry"); assert.ok(f.events.includes("field")); assert.ok(f.events.includes("source"));
  assert.equal(result.metadataComparison.consumers, ERROR_FIELDS.length + 4);
  const g = externalConsumers(fixture(t)); g.resolvers.resolveSourceScalar = () => "0"; assert.throws(() => run(g), /positive source-bound epsilon/);
  const h = externalConsumers(fixture(t)); h.resolvers.resolveGeometryField = () => Object.freeze(Array.from({ length: 14 }, () => Object.freeze(Array(14).fill("1"))));
  assert.throws(() => run(h), /recomputed predicate/);
});
test("reentry from geometry-field or source-scalar resolution poisons even when swallowed", t => {
  for (const key of ["resolveGeometryField", "resolveSourceScalar"]) {
    const f = externalConsumers(fixture(t)), token = f.read(), original = f.resolvers[key];
    f.resolvers[key] = name => { assert.throws(() => run(f, token), /reentrant/); return original(name); };
    assert.throws(() => run(f, token), /swallowed checkpoint reentry/);
  }
});
test("reentry during expected-plan snapshot is detected before geometry or leaf work", t => {
  const f = fixture(t), token = f.read(), expected = f.expected; let reentered = false;
  f.expected = new Proxy(expected, { ownKeys(target) {
    if (!reentered) { reentered = true; assert.throws(() => run(f, token), /reentrant/); }
    return Reflect.ownKeys(target);
  } });
  assert.throws(() => run(f, token), /healthy snapshot/); assert.deepEqual(f.events, []);
});

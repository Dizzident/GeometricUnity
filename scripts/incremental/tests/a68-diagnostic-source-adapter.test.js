"use strict";
// Source geometry remains uncalled. Positive adapter tests construct ONLY the
// symbolic second-jet DAG; geometry-bearing adapters are identity-rejection
// tests. Existing separate recipe tests cover manufactured diagnostic geometry.
const test = require("node:test"), assert = require("node:assert/strict");
const { diagnosticSourceLayout, createDiagnosticSourceAdapter, diagnosticAdapterIdentity } = require("../a68-diagnostic-source-adapter");
const { sourceContextMenu } = require("../a68-source-context-menu");
const { secondJetMarkMenu } = require("../a68-second-jet-recipe");
const { GeometryAlgebra } = require("../a68-geometry-algebra");
const limits = () => ({
  leaf: { leaves: 10, records: 10000, stringCharacters: 1000000, wireBytes: 10000000, rationalCharacters: 128, resolutions: 20 },
  export: { matrixSlots: 2000, coordinateVisits: 400000, entryRecords: 400000 },
  metadata: { nodes: 100000, arraySlots: 100000, stringCharacters: 1000000, maxDepth: 32 },
  comparison: { coordinateVisits: 2000000, sparseRecords: 1, tensorRecords: 1, rationalCharacters: 10000000 }
});
const options = expanded => ({ retention: Object.fromEntries(secondJetMarkMenu().map(m => [m.name, expanded ?? false])), nodeLimit: 4000, markLimit: 420 });
const second = caps => createDiagnosticSourceAdapter(null, "secondJets", { baseline: null, germs: [] }, caps ?? limits());

test("all diagnostic leaf declarations match the separately implemented source menus without hashes or X inputs", () => {
  for (const name of ["grade10", "acceleration", "secondJets"]) {
    const layout = diagnosticSourceLayout(name), expected = sourceContextMenu(layout.contextId);
    assert.deepEqual(layout.leaves, expected.leaves); assert.ok(Object.isFrozen(layout.leaves));
    assert.ok(layout.leaves.every(l => l.degree === 2 && !Object.hasOwn(l, "sha256") && !l.id.endsWith("input/X")));
    assert.equal(layout.geometries.length, name === "grade10" ? 5 : name === "acceleration" ? 2 : 0);
  }
});
test("fixed point germ order source paths and complete geometry resource censuses are explicit", () => {
  const grade = diagnosticSourceLayout("grade10"), acceleration = diagnosticSourceLayout("acceleration"), empty = diagnosticSourceLayout("secondJets");
  assert.equal(grade.point, 0); assert.deepEqual(grade.geometries.slice(1).map(g => [g.point, g.metricBasis, g.jetIndex, g.metadataPath]),
    Array.from({ length: 4 }, (_, i) => [0, i, 4, `basis${i}/geometry`]));
  assert.deepEqual(grade.census, { geometries: 5, leaves: 9, matrixSlots: 1033, exportCoordinateVisits: 202468, comparisonCoordinateVisits: 1450596 });
  assert.equal(acceleration.point, 1); assert.deepEqual(acceleration.geometries.slice(1).map(g => [g.metricBasis, g.jetIndex, g.metadataPath]), [[0, 10, "germ/geometry"]]);
  assert.deepEqual(acceleration.census, { geometries: 2, leaves: 3, matrixSlots: 313, exportCoordinateVisits: 61348, comparisonCoordinateVisits: 459816 });
  assert.equal(empty.point, null); assert.deepEqual(empty.census, { geometries: 0, leaves: 0, matrixSlots: 0, exportCoordinateVisits: 0, comparisonCoordinateVisits: 0 });
  assert.deepEqual(diagnosticSourceLayout("grade10"), grade); assert.notEqual(diagnosticSourceLayout("grade10"), grade);
  assert.ok(Object.isFrozen(grade.geometries[1].leafIds));
});
test("geometry-bearing source adapters reject all copied source-looking identities before algebra work", () => {
  for (const name of ["grade10", "acceleration"]) {
    const a = new GeometryAlgebra({ maxBits: 128, scalarOperations: 10, rationalObjects: 10, matrixObjects: 1, matrixCells: 1, slotVisits: 1 });
    const bindings = { baseline: { kind: "baseline", point: name === "grade10" ? 0 : 1, source: { path: "fake", sha256: "0".repeat(64) } },
      germs: Array.from({ length: name === "grade10" ? 4 : 1 }, () => ({ kind: "germ" })) };
    const before = a.snapshot(); assert.throws(() => createDiagnosticSourceAdapter(a, name, bindings, limits()), /verified geometry binding identity/);
    assert.deepEqual(a.snapshot(), before);
  }
});
test("complete prospective diagnostic admission precedes any source identity or export work", () => {
  for (const [family, field, limit] of [["leaf", "leaves", 8], ["export", "matrixSlots", 1032],
    ["export", "coordinateVisits", 202467], ["comparison", "coordinateVisits", 1450595]]) {
    const cap = limits(); cap[family][field] = limit;
    assert.throws(() => createDiagnosticSourceAdapter(null, "grade10", { baseline: null, germs: [null, null, null, null] }, cap), /prospective resource census/);
  }
});
test("unknown diagnostic names wrong binding menus and accessors cannot manufacture source inputs", () => {
  for (const name of ["Grade10", "diagnostic/grade10", "grade10 ", "", null]) assert.throws(() => diagnosticSourceLayout(name), /exact diagnostic/);
  for (const bindings of [{ baseline: null, germs: [null] }, { baseline: {}, germs: [] }, { baseline: null, germs: [], extra: true }])
    assert.throws(() => createDiagnosticSourceAdapter(null, "secondJets", bindings, limits()));
  assert.throws(() => createDiagnosticSourceAdapter({}, "secondJets", { baseline: null, germs: [] }, limits()), /no geometry/);
  let calls = 0; const bindings = { baseline: null, germs: [] }; Object.defineProperty(bindings, "baseline", { get() { calls++; return null; } });
  assert.throws(() => createDiagnosticSourceAdapter(null, "secondJets", bindings, limits()), /own data/); assert.equal(calls, 0);
  const cap = limits(); Object.defineProperty(cap.leaf, "records", { get() { calls++; return 1; } }); assert.throws(() => second(cap), /own data/); assert.equal(calls, 0);
});
test("second-jet adapter constructs the complete symbolic control recipe without source geometry or coefficients", () => {
  for (const expanded of [false, true]) {
    const adapter = second(), result = adapter.buildRecipe(options(expanded));
    assert.equal(result.context.contextId, "diagnostic/secondJets"); assert.equal(result.tensorPlan.nodes.length, 3502);
    assert.equal(result.tensorPlan.marks.length, 420); assert.equal(result.context.checks.length, 420); assert.equal(result.context.domainChecks.length, 420);
    assert.deepEqual(result.tensorPlan.leaves, []); assert.deepEqual(result.recipeMatrices, []); assert.deepEqual(result.replayIdentity.requests, []);
    assert.deepEqual(result.replayIdentity.tensorPlan, result.tensorPlan); assert.deepEqual(result.replayIdentity.checks, result.context.checks);
    assert.deepEqual(result.metadataPlan.checks, result.context.checks); assert.deepEqual(result.metadataPlan.geometryPaths, []);
    assert.deepEqual(result.metadataPlan.scalarArrays, []); assert.equal(result.metadataPlan.error, null); assert.ok(Object.isFrozen(result.metadataPlan));
    assert.ok(result.tensorPlan.marks.every(m => m.expanded === expanded)); assert.equal(result.scope.numericalReplayComplete, false);
    assert.equal(adapter.scope.arbitraryEtaEstablishedByFiniteControls, false); assert.equal(adapter.scope.scientificExecutionAuthorized, false);
    assert.equal(adapter.compareGeometryMetadata({}).geometries, 0); assert.equal(adapter.snapshot().recordedGeometryCompared, true);
  }
});
test("independent planning must precede geometry comparison and repeated completion poisons the adapter", () => {
  const early = second(); assert.throws(() => early.compareGeometryMetadata({}), /independent recipe/); assert.throws(() => early.buildRecipe(options()), /failed/);
  const built = second(); built.buildRecipe(options()); assert.throws(() => built.buildRecipe(options()), /one diagnostic/); assert.equal(built.snapshot().failed, true);
  const twice = second(); twice.buildRecipe(options()); twice.compareGeometryMetadata({}); assert.throws(() => twice.compareGeometryMetadata({}), /one complete/);
  const extra = second(); extra.buildRecipe(options()); assert.throws(() => extra.compareGeometryMetadata({ fakeGeometry: {} }), /closed ordered/);
});
test("retention and resource profiles are own-data frozen snapshots with cumulative metadata admission", () => {
  const cap = limits(), adapter = second(cap); cap.metadata.nodes = 1; cap.leaf.leaves = 1;
  const input = options(), result = adapter.buildRecipe(input); input.retention[Object.keys(input.retention)[0]] = true;
  assert.ok(result.tensorPlan.marks.every(m => !m.expanded)); const before = adapter.snapshot().metadataUsage.nodes;
  adapter.compareGeometryMetadata({}); assert.ok(adapter.snapshot().metadataUsage.nodes > before);
  const small = limits(); small.metadata.stringCharacters = 1; const reject = second(small); assert.throws(() => reject.buildRecipe(options()), /ceiling/); assert.equal(reject.snapshot().recipeBuilt, false);
  let calls = 0; const bad = options(); Object.defineProperty(bad.retention, Object.keys(bad.retention)[0], { get() { calls++; return true; }, enumerable: true });
  assert.throws(() => second().buildRecipe(bad), /own data/); assert.equal(calls, 0);
});
test("missing retention undersized node or mark limits and foreign leaf resolutions fail closed", () => {
  for (const input of [{ ...options(), retention: {} }, { ...options(), nodeLimit: 1 }, { ...options(), markLimit: 419 }, { ...options(), nodeLimit: 0 }]) {
    const adapter = second(); assert.throws(() => adapter.buildRecipe(input)); assert.equal(adapter.snapshot().failed, true);
  }
  const adapter = second(); adapter.buildRecipe(options()); adapter.compareGeometryMetadata({});
  assert.throws(() => adapter.resolveLeaf({ id: "invented", degree: 1, source: "observed", sha256: "0".repeat(64) }), /complete declared leaf/);
  assert.equal(adapter.snapshot().failed, true);
});

test("swallowed snapshot reentry poisons before recipe work and ordinary proxy reads are not invoked", () => {
  const adapter = second(), input = options(); let reentered = false;
  const proxy = new Proxy(input, { ownKeys(target) {
    if (!reentered) { reentered = true; assert.throws(() => adapter.buildRecipe(options()), /reentrant/); }
    return Reflect.ownKeys(target);
  } });
  assert.throws(() => adapter.buildRecipe(proxy), /snapshot reentry/);
  assert.equal(adapter.snapshot().failed, true); assert.equal(adapter.snapshot().recipeBuilt, false);
  let reads = 0; const germs = new Proxy([], { get() { reads++; throw Error("ordinary getter"); } });
  const clean = createDiagnosticSourceAdapter(null, "secondJets", { baseline: null, germs }, limits());
  clean.buildRecipe(options()); assert.equal(reads, 0);
});

test("combined diagnostic replay requires independent planning before inspecting a checkpoint", () => {
  const adapter = second(); assert.throws(() => adapter.replayContextCheckpoint({}, {}), /fresh independently planned/);
  assert.equal(adapter.snapshot().failed, true); assert.equal(adapter.snapshot().checkpointNumericallyReplayed, false);
  assert.throws(() => adapter.buildRecipe(options()), /failed/);
});
test("standalone unpinned geometry comparison cannot be reused as combined checkpoint acceptance", () => {
  const adapter = second(); adapter.buildRecipe(options()); adapter.compareGeometryMetadata({});
  assert.throws(() => adapter.replayContextCheckpoint({}, {}), /fresh independently planned/);
  assert.equal(adapter.snapshot().failed, true); assert.equal(adapter.snapshot().checkpointNumericallyReplayed, false);
});
test("forged diagnostic checkpoint poisons the adapter without executing the symbolic plan numerically", () => {
  const adapter = second(); adapter.buildRecipe(options());
  assert.throws(() => adapter.replayContextCheckpoint({ schemaVersion: "phase627-pinned-context-checkpoint-v1", contextId: "diagnostic/secondJets" }, {}), /private read-pinned/);
  assert.equal(adapter.snapshot().failed, true); assert.equal(adapter.snapshot().recordedGeometryCompared, false);
  assert.equal(adapter.snapshot().checkpointNumericallyReplayed, false);
  assert.throws(() => adapter.replayContextCheckpoint({}, {}), /failed/);
});

const liveLimits = () => ({ metadata: { nodes: 1000, arraySlots: 1000, stringCharacters: 10000, maxDepth: 16 },
  comparison: { coordinateVisits: 2000000, sparseRecords: 1, tensorRecords: 1, rationalCharacters: 10000000 },
  wireBytes: 1000000, totalWireBytes: 10000000 });
test("explicit empty live geometry configuration preserves authentic prepared state without authorizing coefficients", () => {
  const adapter = second(); adapter.buildRecipe(options()); adapter.configureLiveGeometry(liveLimits());
  const state = adapter.snapshot(), identity = diagnosticAdapterIdentity(adapter);
  assert.equal(state.liveGeometry.slots, 0); assert.equal(state.liveGeometry.prechecked, 0);
  assert.equal(state.liveGeometry.completed, false); assert.equal(state.recordedGeometryCompared, false);
  assert.equal(identity.prepared, true); assert.equal(identity.replayed, false); assert.equal(identity.health().failed, false);
  assert.equal(adapter.scope.liveParentImmutabilityProved, false); assert.equal(adapter.scope.scientificExecutionAuthorized, false);
});
test("live configuration is mandatory for precheck, one-shot, and cannot follow standalone recorded comparison", () => {
  const early = second(); assert.throws(() => early.configureLiveGeometry(liveLimits()), /after diagnostic recipe/);
  const missing = second(); missing.buildRecipe(options()); assert.throws(() => missing.precheckGeometry("background/geometry", {}), /configured pending/);
  const twice = second(); twice.buildRecipe(options()); twice.configureLiveGeometry(liveLimits());
  assert.throws(() => twice.configureLiveGeometry(liveLimits()), /one live geometry/); assert.equal(twice.snapshot().liveGeometry.failed, true);
  const late = second(); late.buildRecipe(options()); late.compareGeometryMetadata({});
  assert.throws(() => late.configureLiveGeometry(liveLimits()), /before observations/);
  for (const adapter of [early, missing, twice, late]) assert.equal(adapter.snapshot().failed, true);
});
test("configured diagnostics cannot fall back to standalone comparison or invent zero-slot geometry", () => {
  for (const operation of [a => a.compareGeometryMetadata({}), a => a.precheckGeometry("background/geometry", {})]) {
    const adapter = second(); adapter.buildRecipe(options()); adapter.configureLiveGeometry(liveLimits());
    assert.throws(() => operation(adapter));
    assert.equal(adapter.snapshot().failed, true); assert.equal(adapter.snapshot().liveGeometry.failed, true);
    assert.equal(diagnosticAdapterIdentity(adapter).health().failed, true);
  }
});
test("configuration accessors and swallowed constructor reentry cannot grant live authority", () => {
  const accessor = second(); accessor.buildRecipe(options()); let reads = 0;
  const cap = liveLimits(); Object.defineProperty(cap, "wireBytes", { enumerable: true, get() { reads++; return 100; } });
  assert.throws(() => accessor.configureLiveGeometry(cap), /own data/); assert.equal(reads, 0); assert.equal(accessor.snapshot().failed, true);
  const adapter = second(); adapter.buildRecipe(options()); let nested = false;
  const proxy = new Proxy(liveLimits(), { ownKeys(target) {
    if (!nested) { nested = true; assert.throws(() => adapter.configureLiveGeometry(liveLimits()), /reentrant/); }
    return Reflect.ownKeys(target);
  } });
  assert.throws(() => adapter.configureLiveGeometry(proxy), /reentry|reentrant/);
  assert.equal(adapter.snapshot().failed, true); assert.equal(adapter.snapshot().liveGeometry?.failed ?? true, true);
});
test("live acceptance does not bypass actual read-pinned checkpoint identity and failure aborts helper", () => {
  const adapter = second(); adapter.buildRecipe(options()); adapter.configureLiveGeometry(liveLimits());
  assert.throws(() => adapter.replayContextCheckpoint({}, {}), /private read-pinned/);
  assert.equal(adapter.snapshot().failed, true); assert.equal(adapter.snapshot().liveGeometry.failed, true);
  assert.equal(adapter.snapshot().liveGeometry.completed, false); assert.equal(adapter.snapshot().checkpointNumericallyReplayed, false);
});
test("VM-manufactured replay control flow finishes empty helper only after replay returns successfully", () => {
  // ONLY the checkpoint replay function is replaced. This proves integration
  // ordering, not a genuine pinned file, numerical replay or scientific result.
  const fs = require("node:fs"), path = require("node:path"), vm = require("node:vm"), { createRequire } = require("node:module");
  const file = path.resolve(__dirname, "../a68-diagnostic-source-adapter.js"), native = createRequire(file);
  for (const fail of [false, true]) {
    let adapter, calls = 0; const module = { exports: {} };
    vm.runInNewContext(fs.readFileSync(file, "utf8"), { module, require(name) {
      if (name === "./a68-context-replay") return { replayContextCheckpoint(_checkpoint, _plan, _limits, resolvers) {
        calls++; assert.equal(adapter.snapshot().liveGeometry.completed, false);
        resolvers.compareGeometryMetadata({}); assert.equal(adapter.snapshot().recordedGeometryCompared, true);
        assert.equal(adapter.snapshot().liveGeometry.completed, false);
        if (fail) throw Error("manufactured late replay failure"); return { manufactured: true };
      } };
      return native(name);
    } }, { filename: file });
    adapter = module.exports.createDiagnosticSourceAdapter(null, "secondJets", { baseline: null, germs: [] }, limits());
    adapter.buildRecipe(options()); adapter.configureLiveGeometry(liveLimits());
    if (fail) assert.throws(() => adapter.replayContextCheckpoint({}, {}), /manufactured late replay failure/);
    else assert.equal(adapter.replayContextCheckpoint({}, {}).replay.manufactured, true);
    const state = adapter.snapshot(); assert.equal(calls, 1); assert.equal(state.failed, fail);
    assert.equal(state.liveGeometry.failed, fail); assert.equal(state.liveGeometry.completed, !fail);
    assert.equal(state.checkpointNumericallyReplayed, !fail);
  }
});

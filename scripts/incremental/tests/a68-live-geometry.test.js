"use strict";
// Exact helper with explicitly manufactured private binding/comparison doubles.
// No genuine retained input, GU reconstruction, adapter or scientific run.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), vm = require("node:vm"), crypto = require("node:crypto");
const { createRequire } = require("node:module");
const { snapshotCanonicalMetadata } = require("../a68-canonical-metadata");
const file = path.resolve(__dirname, "../a68-live-geometry.js"), realRequire = createRequire(file);
const caps = () => ({ metadata: { nodes: 10000, arraySlots: 10000, stringCharacters: 10000, maxDepth: 16 },
  comparison: { coordinateVisits: 100, sparseRecords: 100, tensorRecords: 100, rationalCharacters: 100 }, wireBytes: 10000, totalWireBytes: 100000 });
const clone = value => JSON.parse(JSON.stringify(value));
const digest = value => crypto.createHash("sha256").update(JSON.stringify(value) + "\n", "ascii").digest("hex");
function fixture(values = [{ x: "1" }]) {
  const identities = new Map(), state = { failed: false, comparisons: 0, hashes: 0, observer: null, onHealth: null, onHash: null };
  const a = { snapshot() { state.onHealth?.();return { failed: state.failed }; } };
  const slots = values.map((value, i) => { const binding = Object.freeze({ manufactured: i });identities.set(binding, clone(value));return { metadataPath: "slot" + i + "/geometry", binding }; });
  const module = { exports: {} }, mocks = {
    "./a68-geometry-binding": { boundGeometryIdentity(actual, binding) { assert.equal(actual, a);assert.ok(identities.has(binding), "manufactured private brand");return {}; } },
    "./a68-geometry-metadata": { compareRecordedGeometry(actual, binding, observed, limits) {
      state.comparisons++;assert.equal(actual, a);assert.ok(Object.isFrozen(observed));state.observer?.(observed);
      assert.equal(JSON.stringify(observed), JSON.stringify(identities.get(binding)));
      const usage = { coordinateVisits: 2, sparseRecords: 0, tensorRecords: 0, rationalCharacters: 1 };
      for (const key of Object.keys(usage)) assert.ok(usage[key] <= limits[key], "comparison quota");return usage;
    } },
    "node:crypto": { createHash(...args) { state.hashes++;state.onHash?.();return crypto.createHash(...args); } }
  };
  vm.runInNewContext(fs.readFileSync(file, "utf8"), { module, require: name => mocks[name] || realRequire(name) }, { filename: file });
  const limits = caps();return { a, slots, values, state, limits, create: () => module.exports.createLiveGeometryValidation(values.length ? a : null, slots, limits) };
}
test("full ordered lifecycle compares source once and binds recorded bytes without replacing later replay", () => {
  const f = fixture([{ x: "1" }, { y: [true, null, -2] }]), helper = f.create();
  f.limits.wireBytes = 1;f.slots.reverse();
  let bytes = 0;
  for (let i = 0; i < 2; i++) {
    const value = clone(f.values[i]), reply = helper.precheck("slot" + i + "/geometry", value);
    assert.equal(reply.sha256, digest(value));assert.equal(reply.bytes, Buffer.byteLength(JSON.stringify(value) + "\n"));bytes += reply.bytes;
    assert.equal(reply.scientificExecutionAuthorized, false);assert.ok(Object.isFrozen(reply));value.extra = "later mutation";
  }
  helper.requireComplete();assert.equal(helper.snapshot().retainedCommitments, 2);
  for (let i = 0; i < 2; i++) helper.bindRecorded("slot" + i + "/geometry", f.values[i]);
  assert.equal(f.state.comparisons, 2);assert.equal(helper.snapshot().wireBytes, bytes * 2);
  helper.finish();const state = helper.snapshot();assert.equal(state.completed, true);assert.equal(state.failed, false);assert.equal(state.retainedCommitments, 0);
  assert.equal(state.scope.laterFullSourceComparisonRequired, true);assert.equal(state.scope.scientificExecutionAuthorized, false);
});
test("empty lifecycle requires no algebra comparisons or placeholder bindings", () => {
  const f = fixture([]), helper = f.create();helper.requireComplete();helper.finish();
  assert.equal(helper.snapshot().completed, true);assert.equal(f.state.hashes, 0);assert.equal(f.state.comparisons, 0);
});
test("canonical byte census handles ASCII escaping empty containers booleans and safe integer edges exactly", () => {
  const values = [{ 'a"\\': ['"\\', "", true, false, null, {}, [], Number.MAX_SAFE_INTEGER, -Number.MAX_SAFE_INTEGER, 0] }];
  const bytes = Buffer.byteLength(JSON.stringify(values[0]) + "\n"), f = fixture(values);f.limits.wireBytes = bytes;f.limits.totalWireBytes = bytes * 2;
  const helper = f.create();assert.equal(helper.precheck("slot0/geometry", values[0]).bytes, bytes);
  helper.bindRecorded("slot0/geometry", values[0]);helper.finish();assert.equal(helper.snapshot().wireBytes, 2 * bytes);
});
test("wire ceilings reject prospectively before stringify hash or source comparison", () => {
  for (const mode of ["single", "total", "recorded"]) {
    const value = { x: '"\\' }, bytes = Buffer.byteLength(JSON.stringify(value) + "\n"), f = fixture([value]);
    if (mode === "single") f.limits.wireBytes = bytes - 1;
    else f.limits.totalWireBytes = mode === "total" ? bytes - 1 : 2 * bytes - 1;
    const helper = f.create();if (mode === "recorded") helper.precheck("slot0/geometry", value);
    assert.throws(() => mode === "recorded" ? helper.bindRecorded("slot0/geometry", value) : helper.precheck("slot0/geometry", value), /wire byte ceiling/);
    assert.equal(f.state.hashes, mode === "recorded" ? 1 : 0);assert.equal(f.state.comparisons, mode === "recorded" ? 1 : 0);
    assert.equal(helper.snapshot().failed, true);assert.equal(helper.snapshot().retainedCommitments, 0);
  }
});
test("missing duplicate wrong-path out-of-order partial and post-finish operations poison permanently", () => {
  for (const mode of ["missing", "wrong", "duplicate", "partial", "record-order", "duplicate-record", "early-finish", "after-finish"]) {
    const f = fixture([{ x: "1" }, { y: "2" }]), h = f.create();
    if (["duplicate", "partial"].includes(mode)) h.precheck("slot0/geometry", f.values[0]);
    if (["record-order", "duplicate-record", "after-finish"].includes(mode)) { h.precheck("slot0/geometry", f.values[0]);h.precheck("slot1/geometry", f.values[1]); }
    if (["duplicate-record", "after-finish"].includes(mode)) h.bindRecorded("slot0/geometry", f.values[0]);
    if (mode === "after-finish") { h.bindRecorded("slot1/geometry", f.values[1]);h.finish(); }
    assert.throws(() => {
      if (mode === "missing") h.requireComplete();
      else if (mode === "wrong") h.precheck("slot1/geometry", f.values[1]);
      else if (mode === "duplicate") h.precheck("slot0/geometry", f.values[0]);
      else if (mode === "record-order") h.bindRecorded("slot1/geometry", f.values[1]);
      else if (["partial", "duplicate-record"].includes(mode)) h.bindRecorded("slot0/geometry", f.values[0]);
      else h.finish();
    });
    assert.equal(h.snapshot().failed, true);assert.equal(h.snapshot().retainedCommitments, 0);assert.throws(() => h.requireComplete());
  }
});
test("source mismatch and changed recorded fields or ordering cannot mint acceptance", () => {
  for (const mode of ["source", "recorded", "order"]) {
    const f = fixture([{ x: "1", y: "2" }]), h = f.create(), wrong = mode === "order" ? { y: "2", x: "1" } : { x: "3", y: "2" };
    if (mode !== "source") h.precheck("slot0/geometry", f.values[0]);
    assert.throws(() => mode === "source" ? h.precheck("slot0/geometry", wrong) : h.bindRecorded("slot0/geometry", wrong));
    assert.equal(h.snapshot().failed, true);assert.equal(h.snapshot().retainedCommitments, 0);
  }
});
test("both snapshot phases and multiple source comparisons use cumulative quotas", () => {
  const value = { x: ["1".repeat(30), "2"] }, usage = snapshotCanonicalMetadata(value, caps().metadata).usage;
  for (const field of ["nodes", "arraySlots", "stringCharacters"]) {
    const f = fixture([value]);f.limits.metadata[field] = 2 * usage[field] - 1;const h = f.create();h.precheck("slot0/geometry", value);
    assert.throws(() => h.bindRecorded("slot0/geometry", value), /ceiling/);assert.equal(h.snapshot().failed, true);
  }
  const f = fixture([{ x: "1" }, { x: "1" }]);f.limits.comparison.coordinateVisits = 3;
  const h = f.create();h.precheck("slot0/geometry", f.values[0]);assert.throws(() => h.precheck("slot1/geometry", f.values[1]), /comparison quota/);
  assert.equal(h.snapshot().failed, true);
});
test("accessors malformed scalars cycles and extra slots are rejected without getter execution", () => {
  for (const mode of ["accessor", "unicode", "negative-zero", "unsafe", "cycle"]) {
    const f = fixture(), h = f.create();let reads = 0, actual = { x: "1" };
    if (mode === "accessor") Object.defineProperty(actual, "x", { enumerable: true, get() { reads++;return "1"; } });
    if (mode === "unicode") actual.x = "é";
    if (mode === "negative-zero") actual.x = -0;
    if (mode === "unsafe") actual.x = Number.MAX_SAFE_INTEGER + 1;
    if (mode === "cycle") actual.x = actual;
    assert.throws(() => h.precheck("slot0/geometry", actual));assert.equal(reads, 0);assert.equal(f.state.comparisons, 0);assert.equal(h.snapshot().failed, true);
  }
  const f = fixture();f.slots.extra = true;assert.throws(() => f.create(), /dense slots/);
});
test("snapshot comparison hash and health reentry plus late algebra failure are sticky", () => {
  for (const mode of ["snapshot", "comparison", "hash", "health", "late", "compare-failure"]) {
    const f = fixture(), h = f.create();let actual = { x: "1" };
    const reenter = () => { assert.equal(h.snapshot().failed, true); };
    if (mode === "snapshot") actual = new Proxy(actual, { ownKeys(target) { reenter();return Reflect.ownKeys(target); } });
    if (mode === "comparison") f.state.observer = reenter;
    if (mode === "hash") f.state.onHash = reenter;
    if (mode === "health") f.state.onHealth = reenter;
    if (mode === "compare-failure") f.state.observer = () => { f.state.failed = true; };
    if (mode === "late") { h.precheck("slot0/geometry", actual);f.state.failed = true; }
    assert.throws(() => mode === "late" ? h.requireComplete() : h.precheck("slot0/geometry", actual));
    assert.equal(h.snapshot().failed, true);assert.equal(h.snapshot().retainedCommitments, 0);
  }
});
test("completed helper still depends on healthy source algebra and never claims live parent immutability", () => {
  const f = fixture(), h = f.create();h.precheck("slot0/geometry", f.values[0]);h.bindRecorded("slot0/geometry", f.values[0]);h.finish();
  assert.equal(h.snapshot().completed, true);assert.equal(h.snapshot().scope.liveParentImmutabilityEstablished, false);
  f.state.failed = true;assert.equal(h.snapshot().failed, true);assert.equal(h.snapshot().completed, false);assert.equal(h.snapshot().retainedCommitments, 0);
});
test("abort releases commitments and cannot be healed; invalid setup and forged genuine bindings reject", () => {
  const f = fixture(), h = f.create();h.precheck("slot0/geometry", f.values[0]);h.abort();assert.equal(h.snapshot().retainedCommitments, 0);assert.throws(() => h.requireComplete());
  for (const mutate of [f => { f.limits.wireBytes = 0; }, f => { f.limits.metadata.maxDepth = 65; }, f => { f.slots.push(f.slots[0]); }, f => { f.slots[0].binding = {}; }]) {
    const bad = fixture();mutate(bad);assert.throws(() => bad.create());
  }
  const { createLiveGeometryValidation } = realRequire("./a68-live-geometry");
  assert.throws(() => createLiveGeometryValidation({}, [{ metadataPath: "background/geometry", binding: {} }], caps()), /binding identity/);
  assert.throws(() => createLiveGeometryValidation({}, [], caps()), /no algebra/);
});

test("manufactured full14 geometry uses the REAL comparison and algebra through the last coordinate", () => {
  // Only source authentication is doubled in these isolated modules. The full
  // metadata comparator, rational parser, coordinate census and live helper
  // execute unchanged. These zero/sentinel matrices are NOT GU reconstructions.
  const { GeometryAlgebra } = realRequire("./a68-geometry-algebra");
  const metadataFile = path.resolve(__dirname, "../a68-geometry-metadata.js");
  for (const kind of ["baseline", "germ"]) for (const mode of ["valid", "last-coordinate", "comparison-quota"]) {
    const a = new GeometryAlgebra({ maxBits: 128, scalarOperations: 2000000, rationalObjects: 2000000,
      matrixObjects: 10, matrixCells: 2000, slotVisits: 2000 });
    const matrix = a.matrix(14, () => a.zero), sentinel = a.matrix(14, (i, j) => i === 13 && j === 12 ? a.parse("-3/2") : a.zero);
    const vector = Array(14).fill(matrix), grid = Array(14).fill(vector), metric = { Value: matrix, D: vector, DD: grid };
    const connection = { Gamma: vector, DGamma: grid, Curvature: grid };
    const omega = [...vector];omega[13] = sentinel;
    const rebuilt = kind === "baseline" ? {
      frame: { Frame: matrix, InverseFrame: matrix, FrameLift: vector, FramePartial: vector, Omega: omega },
      source: { metric }, connection
    } : {
      germ: { metricBasis: 0, jetIndex: 0, order: 0, multiindex: [0, 0, 0, 0], shear: metric, metricVariation: metric },
      matrices: { Motion: matrix, DeltaFrame: sentinel, MotionPartial: vector, MotionCovariant: vector,
        DeltaOmega: vector, DeltaOmegaPartial: grid }, connectionVariation: connection
    };
    const binding = Object.freeze({ kind, point: 0 });
    const privateDouble = { boundGeometryIdentity(actual, candidate) {
      assert.equal(actual, a);assert.equal(candidate, binding);return { rebuilt };
    } };
    const metadataModule = { exports: {} };
    vm.runInNewContext(fs.readFileSync(metadataFile, "utf8"), { module: metadataModule,
      require: name => name === "./a68-geometry-binding" ? privateDouble : realRequire(name) }, { filename: metadataFile });
    const helperModule = { exports: {} };
    vm.runInNewContext(fs.readFileSync(file, "utf8"), { module: helperModule, require: name =>
      name === "./a68-geometry-binding" ? privateDouble : name === "./a68-geometry-metadata" ? metadataModule.exports : realRequire(name) }, { filename: file });
    const text = Array.from({ length: 14 }, () => Array(14).fill("0")), textVector = Array(14).fill(text), textGrid = Array(14).fill(textVector);
    const textMetric = { g: text, d: textVector, dd: textGrid }, textConnection = { gamma: textVector, dGamma: textGrid, curvature: textGrid };
    const last = text.map(row => [...row]);last[13][12] = "-3/2";
    const textOmega = [...textVector];textOmega[13] = last;
    const observed = kind === "baseline" ? { point: 0, frame: text, inverseFrame: text, metric: textMetric,
      connection: textConnection, frameLift: textVector, framePartial: textVector, omega: textOmega } : {
      metricBasis: 0, jetIndex: 0, multiindex: [0, 0, 0, 0], order: 0, shear: textMetric,
      deltaMetric: textMetric, blockMetric: textMetric, deltaConnection: textConnection, palatini: textConnection,
      motion: text, motionPartial: textVector, motionCovariant: textVector, deltaOmega: textVector,
      deltaOmegaPartial: textGrid, deltaFrame: last
    };
    const coordinates = kind === "baseline" ? 129556 : 330260;
    const limits = { metadata: { nodes: 2000000, arraySlots: 2000000, stringCharacters: 2000000, maxDepth: 16 },
      comparison: { coordinateVisits: coordinates - (mode === "comparison-quota" ? 1 : 0), sparseRecords: 1,
        tensorRecords: 1, rationalCharacters: 2000000 }, wireBytes: 2000000, totalWireBytes: 4000000 };
    const h = helperModule.exports.createLiveGeometryValidation(a, [{ metadataPath: "manufactured/geometry", binding }], limits);
    if (mode === "last-coordinate") last[13][13] = "1";
    if (mode !== "valid") {
      assert.throws(() => h.precheck("manufactured/geometry", observed), /mismatch|ceiling/);
      assert.equal(h.snapshot().failed, true);assert.equal(h.snapshot().retainedCommitments, 0);
    } else {
      const reply = h.precheck("manufactured/geometry", observed);
      assert.equal(reply.sha256, digest(observed));assert.equal(h.snapshot().comparisonUsage.coordinateVisits, coordinates);
      h.bindRecorded("manufactured/geometry", observed);h.finish();assert.equal(h.snapshot().completed, true);
    }
  }
});

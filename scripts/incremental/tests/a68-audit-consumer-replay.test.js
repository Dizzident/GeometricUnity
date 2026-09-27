"use strict";
// Manufactured exact arithmetic only. No retained polynomial/geometry or GU run.
const test = require("node:test"), assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { createAuditConsumerReplay } = require("../a68-audit-consumer-replay");
const { createScalarReplay } = require("../a68-scalar-replay");
const { ERROR_FIELDS } = require("../a68-audit-consumer-schedule");
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const row = (form = 1, blade = 1, real = "2", imaginary = "0") => ({ form, blade, k0: 0, k1: 0, real, imaginary });
const tensor = node => ({ kind: "tensorNode", node }), literal = () => ({ kind: "literalZero" });
const selector = (node, form = 1, blade = 1) => ({ tensor: tensor(node), form, blade, k0: 0, k1: 0, absent: "0" });
const check = (name, predicate) => ({ name, metadataPath: "check/" + name, predicate });
const domain = (node, name = "domain") => ({ name, node, degree: 1, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true });
const constant = value => ({ op: "constant", value });
const plan = (count = 6) => { const r = new MixedRecipe(); for (let i = 0; i < count; i++) r.Unit(1, 1); return r.Finish(); };
const scheduleLimits = () => ({ tensorNodes: 100, tensorEdges: 1000, tensorMarks: 100, scalarNodes: 1000, scalarEdges: 1000,
  roots: 100, geometryMatrices: 100, geometryEntries: 10000, maxDepth: 64, stringCharacters: 100000, rationalCharacters: 100 });
const consumerScheduleLimits = () => ({ consumers: 100, expressionNodes: 1000, expressionEdges: 1000, tensorReferences: 1000,
  scalarReferences: 100, stringCharacters: 100000, maxDepth: 64, rationalCharacters: 100, retainedRootValues: 100 });
const arithmetic = () => ({ maxBits: 256, scalarOperations: 1000000, rationalObjects: 1000000, matrixObjects: 1000, matrixCells: 1000000, slotVisits: 1000000 });
const limits = extra => ({ tensorRecords: 1000, tensorResolutions: 1000, parsedTensorRecords: 100000, recordVisits: 1000000,
  geometryResolutions: 100, geometryCoordinates: 1000000, arraySlots: 1000000, coefficientReads: 1000000, readCharacters: 1000000,
  outputCharacters: 100000, rationalCharacters: 100, predicateNodes: 100000, rootValues: 100, errorValues: 100, resultValues: 100,
  consumerComparisons: 1000, sourceResolutions: 100, ...extra });
function options(extra = {}) {
  return { tensorPlan: plan(), namedRoots: [], geometry: [], checks: [], domainChecks: [], error: null,
    scheduleLimits: scheduleLimits(), consumerScheduleLimits: consumerScheduleLimits(), arithmeticLimits: arithmetic(), replayLimits: limits(),
    resolveTensor: () => freeze([row()]), resolveGeometryField: () => { throw Error("unexpected geometry resolver"); },
    resolveSourceScalar: () => { throw Error("unexpected source resolver"); }, compareConsumer: () => true, ...extra };
}
function execute(opts) {
  const r = createAuditConsumerReplay(opts);
  for (let step = -1; step < opts.tensorPlan.nodes.length; step++) r.advance(step);
  return r.finish();
}
const error = () => ({ schemaVersion: "phase627-mixed-error-formula-obligations-v1", numericalReplayImplemented: false,
  fields: Object.fromEntries(ERROR_FIELDS.map(name => [name, name === "nativeFirstJetNorms" ? Array.from({ length: 14 }, () => constant("0")) : constant("0")])) });
const mat = name => ({ op: "geometryMatrix", name });
const dense = (rank, value = "0") => freeze(rank === 1 ? Array(14).fill(value) : Array.from({ length: 14 }, () => dense(rank - 1, value)));

test("complete late tensor equality preserves operands through the combined release step", () => {
  const live = new Map(), opts = options({ checks: [check("late", { kind: "tensorEqual", left: tensor(0), right: tensor(5) })],
    resolveTensor: id => { assert.ok(live.has(id)); return live.get(id); } });
  const r = createAuditConsumerReplay(opts); r.advance(-1);
  for (let step = 0; step < 6; step++) {
    live.set(step, freeze([row()])); const release = r.advance(step);
    if (step < 5) assert.ok(live.has(0)); release.forEach(id => assert.ok(live.delete(id)));
  }
  assert.equal(live.size, 0); const result = r.finish(); assert.equal(result.complete, true);
  assert.equal(result.consumersExecuted, 1); assert.equal(result.usage.tensorResolutions, 2);
});

test("equality compares every canonical coefficient including imaginary, missing and extra records", () => {
  for (const other of [[row(1, 1, "2", "1")], [row(), row(2, 31, "3")], []]) {
    const r = createAuditConsumerReplay(options({ checks: [check("different", { kind: "tensorEqual", left: tensor(0), right: tensor(1) })],
      resolveTensor: id => freeze(id === 0 ? [row()] : other) }));
    r.advance(-1); r.advance(0); assert.throws(() => r.advance(1), /numerical predicate failed/);
    assert.equal(r.snapshot().failed, true); assert.throws(() => r.advance(1), /poisoned/);
    assert.equal(r.snapshot().cachedTensorArrays, 0); assert.equal(r.snapshot().cachedGeometryArrays, 0);
  }
});

test("literal zeros never resolve tensors and all conjunction branches are evaluated", () => {
  let resolutions = 0;
  const opts = options({ checks: [check("all", { kind: "all", predicates: [
    { kind: "tensorZero", tensor: tensor(0) }, { kind: "tensorZero", tensor: tensor(1) }, { kind: "tensorZero", tensor: literal() }
  ] })], resolveTensor: () => { resolutions++; return freeze([row()]); } });
  const r = createAuditConsumerReplay(opts); r.advance(-1); r.advance(0);
  assert.throws(() => r.advance(1), /numerical predicate failed/); assert.equal(resolutions, 2);
  assert.equal(execute(options({ tensorPlan: plan(0), checks: [check("zero", { kind: "tensorZero", tensor: literal() })] })).consumersExecuted, 1);
});

test("H-anti is enforced only for domain checks, with grade parity and central imaginary direction", () => {
  for (const value of [row(1, 0, "0", "3"), row(1, 31, "-4"), row(128, 1, "2")])
    assert.equal(execute(options({ domainChecks: [domain(0)], resolveTensor: () => freeze([value]) })).complete, true);
  const notAnti = row(1, 0, "3");
  assert.equal(execute(options({ checks: [check("same", { kind: "tensorEqual", left: tensor(0), right: tensor(1) })], resolveTensor: () => freeze([notAnti]) })).complete, true);
  assert.throws(() => execute(options({ domainChecks: [domain(0)], resolveTensor: () => freeze([notAnti]) })), /numerical predicate failed/);
});

test("coefficient anchors compare both complex components, use absent zero and require strict lower bound", () => {
  const checks = [check("scaled", { kind: "complexCoefficientScaledEqual", left: selector(0), right: selector(1), factor: "-1/2" }),
    check("absent", { kind: "complexCoefficientScaledEqual", left: selector(0, 1, 7), right: selector(1, 1, 7), factor: "3" }),
    check("lower", { kind: "realCoefficientStrictLowerBound", coefficient: selector(2), imaginaryRequired: "0", lowerBound: "1" })];
  const rows = [row(1, 1, "-2", "-3"), row(1, 1, "4", "6"), row()];
  assert.equal(execute(options({ checks, resolveTensor: id => freeze([rows[id] ?? row()]) })).complete, true);
  rows[0] = row(1, 1, "-2", "3"); assert.throws(() => execute(options({ checks, resolveTensor: id => freeze([rows[id] ?? row()]) })), /numerical predicate failed/);
  assert.throws(() => execute(options({ checks: [checks[2]], resolveTensor: () => freeze([row(1, 1, "1")]) })), /numerical predicate failed/);
});

test("scalar replay and consumer replay compose with cached early roots and only combined tensor releases", () => {
  const live = new Map(), namedRoots = [{ name: "early", expression: constant("2") }, { name: "pair", expression: { op: "pair", left: 0, right: 5 } }];
  const opts = options({ namedRoots, checks: [check("rootAtLateTensor", { kind: "scalarRootEqualsRealCoefficient", root: "early", coefficient: selector(4), factor: "1" }),
    check("pair", { kind: "scalarRootEqualsConstant", root: "pair", expected: "-4" })], resolveTensor: id => { assert.ok(live.has(id)); return live.get(id); } });
  const r = createAuditConsumerReplay(opts), s = createScalarReplay({ tensorPlan: opts.tensorPlan, namedRoots, geometry: [], scheduleLimits: opts.scheduleLimits,
    arithmeticLimits: arithmetic(), replayLimits: { tensorRecords: 100, tensorResolutions: 100, recordVisits: 1000, coefficientReads: 1000,
      readCharacters: 10000, outputCharacters: 10000, rationalCharacters: 100, liveScalars: 100 }, resolveTensor: opts.resolveTensor,
    compareRoot: (name, text) => r.captureRoot(name, text) });
  s.advance(-1); r.advance(-1);
  for (let step = 0; step < 6; step++) {
    live.set(step, freeze([row()])); s.advance(step); r.advance(step).forEach(id => assert.ok(live.delete(id)));
  }
  assert.equal(s.finish().complete, true); const result = r.finish(); assert.equal(result.rootsObserved, 2);
  assert.equal(result.liveRootValues, 0); assert.equal(result.maximumRootValues, 1); assert.equal(live.size, 0);
});

test("real-coefficient scalar anchor intentionally ignores imaginary part, unlike strict real lower bound", () => {
  const opts = options({ namedRoots: [{ name: "x", expression: constant("2") }], resolveTensor: () => freeze([row(1, 1, "2", "7")]),
    checks: [check("realOnly", { kind: "scalarRootEqualsRealCoefficient", root: "x", coefficient: selector(0), factor: "1" })] });
  const r = createAuditConsumerReplay(opts); r.captureRoot("x", "2"); for (let i = -1; i < 6; i++) r.advance(i);
  assert.equal(r.finish().complete, true);
});

test("Ward scalar-array and ordered-name predicates evaluate all exact ordered values", () => {
  const namedRoots = ["a", "b"].map(name => ({ name, expression: constant("3/2") }));
  const checks = [check("ward", { kind: "all", predicates: [{ kind: "scalarEqual", left: "a", right: "b" },
    { kind: "scalarArrayEqual", left: ["a", "b"], right: ["b", "a"] }, { kind: "orderedNamesEqual", left: ["a", "b"], right: ["a", "b"] }] })];
  const r = createAuditConsumerReplay(options({ tensorPlan: plan(0), namedRoots, checks })); r.captureRoot("a", "3/2"); r.captureRoot("b", "3/2"); r.advance(-1);
  assert.equal(r.finish().maximumRootValues, 2);
  checks[0].predicate.predicates[2].right = ["b", "a"];
  const bad = createAuditConsumerReplay(options({ tensorPlan: plan(0), namedRoots, checks })); bad.captureRoot("a", "3/2"); bad.captureRoot("b", "3/2");
  assert.throws(() => bad.advance(-1), /numerical predicate failed/);
});

test("complete dense geometry validates every rank-four coefficient, not only packed halves", () => {
  const zero = dense(4), changed = JSON.parse(JSON.stringify(zero)); changed[13][0][0][13] = "1"; freeze(changed);
  const path = "geometry/geometry.deltaConnection.curvature";
  assert.equal(execute(options({ checks: [check("curvature", { kind: "geometryZero", field: path })], resolveGeometryField: () => zero })).usage.geometryCoordinates, 38416);
  assert.throws(() => execute(options({ checks: [check("curvature", { kind: "geometryZero", field: path })], resolveGeometryField: () => changed })), /numerical predicate failed/);
  const corrupt = JSON.parse(JSON.stringify(zero)); corrupt[13][13][13][13] = "2/4";
  assert.throws(() => execute(options({ checks: [check("curvature", { kind: "geometryZero", field: path })], resolveGeometryField: () => freeze(corrupt) })), /canonical/);
});

test("geometry matrix equality and trace use full dense entries and signed diagonal sum", () => {
  const m = JSON.parse(JSON.stringify(dense(2))); m[0][0] = "-2"; m[13][13] = "2"; m[0][13] = "5"; freeze(m);
  const checks = [check("trace", { kind: "geometryTraceZero", matrix: "geometry/geometry.motion" }),
    check("equal", { kind: "geometryEqual", left: "geometry/geometry.deltaMetric.g", right: "geometry/geometry.blockMetric.g" })];
  assert.equal(execute(options({ checks, resolveGeometryField: () => m })).complete, true);
  const mismatch = JSON.parse(JSON.stringify(m)); mismatch[0][13] = "4";
  assert.throws(() => execute(options({ checks, resolveGeometryField: path => path.includes("blockMetric") ? freeze(mismatch) : m })), /numerical predicate failed/);
});

test("all19 error fields evaluate exact norms with signed matrix products before absolute values", () => {
  const e = error(), seen = new Map(), geometry = [{ id: "frame", entries: [
    { row: 0, column: 0, value: "1" }, { row: 0, column: 1, value: "-1" }, { row: 1, column: 0, value: "2" }] },
  { id: "lift", entries: [{ row: 0, column: 0, value: "1" }, { row: 1, column: 0, value: "1" }] }];
  e.fields.fieldError = { op: "sourceBoundScalar", binding: "phase626/certifiedEpsilon", positiveRequired: true };
  e.fields.motionNorm = { op: "entryL1", value: mat("frame") };
  e.fields.referenceNorm = { op: "tensorL1", tensor: tensor(0) };
  e.fields.nativeOneFormDualNorm = { op: "maximumColumnL1", matrix: mat("frame") };
  e.fields.nativeFieldNorm = { op: "maximumRowL1", matrix: mat("frame") };
  e.fields.raw0Majorant = { op: "sum", values: [constant("1"), constant("1/2")] };
  e.fields.raw0Error = { op: "product", values: [constant("3/2"), constant("1/10")] };
  e.fields.nativeFirstJetNorms = Array.from({ length: 14 }, () => ({ op: "maximumRowL1", matrix: { op: "matrixProduct", left: mat("frame"), right: mat("lift") } }));
  const result = execute(options({ error: e, geometry, resolveTensor: () => freeze([row(1, 0, "-2", "3")]), resolveSourceScalar: () => "1/10",
    compareConsumer: (c, value) => { seen.set(c.name, value); return true; } }));
  assert.equal(seen.size, 19); assert.equal(seen.get("fieldError"), "1/10"); assert.equal(seen.get("motionNorm"), "4");
  assert.equal(seen.get("referenceNorm"), "5"); assert.equal(seen.get("nativeOneFormDualNorm"), "3"); assert.equal(seen.get("nativeFieldNorm"), "2");
  assert.equal(seen.get("raw0Majorant"), "3/2"); assert.equal(seen.get("raw0Error"), "3/20");
  assert.deepEqual(seen.get("nativeFirstJetNorms"), Array(14).fill("2")); assert.equal(result.complete, true);
});

test("error results cache until their final zero check then release; epsilon must stay positive", () => {
  const e = error(); e.fields.fieldError = { op: "sourceBoundScalar", binding: "phase626/certifiedEpsilon", positiveRequired: true };
  e.fields.raw0Error = { op: "tensorL1", tensor: tensor(2) };
  const opts = options({ error: e, resolveTensor: () => freeze([]), resolveSourceScalar: () => "1/7", checks: [
    check("zeros", { kind: "all", predicates: [{ kind: "metadataScalarsZero", fields: ["error/raw0Error", "error/raw2Error", "error/eulerError"] }, { kind: "tensorZero", tensor: tensor(5) }] })] });
  const result = execute(opts); assert.equal(result.maximumErrorValues, 3); assert.equal(result.liveErrorValues, 0);
  assert.throws(() => execute({ ...opts, resolveSourceScalar: () => "0" }), /positive source-bound epsilon/);
  assert.throws(() => execute({ ...opts, replayLimits: limits({ errorValues: 2 }) }), /error cache ceiling/);
  assert.throws(() => execute({ ...opts, replayLimits: limits({ resultValues: 13 }) }), /result slots/);
});

test("strict root capture and step census reject unknown, duplicate, early, omitted and late values", () => {
  const opts = options({ namedRoots: [{ name: "early", expression: constant("1") }, { name: "late", expression: { op: "pair", left: 0, right: 5 } }] });
  for (const action of [r => r.captureRoot("absent", "1"), r => r.captureRoot("late", "1"), r => r.advance(-1),
    r => { r.captureRoot("early", "1"); r.captureRoot("early", "1"); }, r => r.finish(), r => r.advance(0)]) {
    const r = createAuditConsumerReplay(opts); assert.throws(() => action(r)); assert.equal(r.snapshot().failed, true); assert.throws(() => r.finish(), /poisoned/);
  }
});

test("tensor accessors, extra keys, wrong degree, frequency and explicit zero fail before comparison", () => {
  for (const bad of [[row(3)], [{ ...row(), k0: 1 }], [row(1, 1, "0")], [row(1, 1, "2/4")], [row(), row()]])
    assert.throws(() => execute(options({ domainChecks: [domain(0)], resolveTensor: () => freeze(bad) })), /degree|domain|zero|canonical|ordered/);
  let invoked = false; const getter = { ...row() }; Object.defineProperty(getter, "real", { enumerable: true, get() { invoked = true; return "2"; } }); Object.freeze(getter);
  assert.throws(() => execute(options({ domainChecks: [domain(0)], resolveTensor: () => Object.freeze([getter]) })), /own-data/); assert.equal(invoked, false);
  const array = []; Object.defineProperty(array, "0", { enumerable: true, get() { invoked = true; return row(); } }); Object.freeze(array);
  assert.throws(() => execute(options({ domainChecks: [domain(0)], resolveTensor: () => array })), /own-data/); assert.equal(invoked, false);
});

test("resolver and comparator reentrancy poisons permanently even if the callback swallows it", () => {
  for (const callback of ["resolveTensor", "compareConsumer", "resolveGeometryField", "resolveSourceScalar"]) {
    let r; const opts = options({ checks: [check("same", { kind: "tensorEqual", left: tensor(0), right: tensor(0) })] });
    if (callback === "resolveGeometryField") opts.checks = [check("geometry", { kind: "geometryZero", field: "geometry/geometry.motion" })];
    if (callback === "resolveSourceScalar") { opts.error = error(); opts.error.fields.fieldError = { op: "sourceBoundScalar", binding: "phase626/certifiedEpsilon", positiveRequired: true }; }
    opts[callback] = () => { try { r.finish(); } catch {} return callback === "compareConsumer" ? true : callback === "resolveSourceScalar" ? "1" : callback === "resolveGeometryField" ? dense(2) : freeze([row()]); };
    r = createAuditConsumerReplay(opts); assert.throws(() => { r.advance(-1); r.advance(0); }, /callback poisoned/); assert.equal(r.snapshot().failed, true);
  }
});

test("admission rejects work and output before excess allocation or parsing", () => {
  const base = options({ domainChecks: [domain(0)], resolveTensor: () => freeze([row(), row(2)]) });
  for (const cap of [{ tensorRecords: 1 }, { parsedTensorRecords: 1 }, { recordVisits: 1 }, { arraySlots: 1 }, { coefficientReads: 1 }, { readCharacters: 1 }])
    assert.throws(() => execute({ ...base, replayLimits: limits(cap) }), /ceiling/);
  assert.throws(() => execute(options({ error: error(), replayLimits: limits({ outputCharacters: 1 }) })), /outputCharacters ceiling/);
  assert.throws(() => execute(options({ checks: [check("g", { kind: "geometryZero", field: "geometry/geometry.motion" })], replayLimits: limits({ geometryCoordinates: 195 }) })), /geometryCoordinates ceiling/);
  assert.throws(() => createAuditConsumerReplay({ ...base, replayLimits: limits({ predicateNodes: Infinity }) }), /finite consumer limits/);
});

test("comparison rejection and completed-lifecycle reuse fail closed", () => {
  assert.throws(() => execute(options({ domainChecks: [domain(0)], compareConsumer: () => false })), /comparison rejected/);
  const r = createAuditConsumerReplay(options({ tensorPlan: plan(0) })); r.advance(-1); assert.equal(r.finish().complete, true);
  assert.throws(() => r.advance(-1), /completed consumer lifecycle/); assert.equal(r.snapshot().failed, true);
});

test("geometry length and array-scan admission precede frozen-object enumeration", () => {
  let scanned = false;
  const value = new Proxy(Object.freeze(Array(15).fill("0")), { ownKeys(target) { scanned = true; return Reflect.ownKeys(target); } });
  const opts = options({ checks: [check("shape", { kind: "geometryZero", field: "geometry/geometry.motion" })], resolveGeometryField: () => value });
  assert.throws(() => execute(opts), /exact array shape/); assert.equal(scanned, false);
  const validLength = new Proxy(Object.freeze(Array(14).fill("0")), { ownKeys(target) { scanned = true; return Reflect.ownKeys(target); } });
  assert.throws(() => execute({ ...opts, replayLimits: limits({ arraySlots: 13 }), resolveGeometryField: () => validLength }), /arraySlots ceiling/);
  assert.equal(scanned, false);
});

test("failed geometry comparison clears every parsed geometry cache", () => {
  const r = createAuditConsumerReplay(options({ checks: [check("nonzero", { kind: "geometryZero", field: "geometry/geometry.motion" })], resolveGeometryField: () => dense(2, "1") }));
  assert.throws(() => r.advance(-1), /numerical predicate failed/);
  assert.equal(r.snapshot().cachedGeometryArrays, 0); assert.equal(r.snapshot().cachedTensorArrays, 0);
});

test("diagnostic blade grade scans inspect every complex coefficient without projecting the tensor", () => {
  const opts = options({ checks: [check("grades", { kind: "tensorBladeGrades", tensor: tensor(0), allowed: [2, 5] })],
    resolveTensor: () => freeze([row(1, 3), { ...row(1, 31), real: "0", imaginary: "3" }]) });
  assert.equal(execute(opts).complete, true, "grade checks alone impose no H-anti domain restriction");
  assert.throws(() => execute({ ...opts, resolveTensor: () => freeze([row(1, 3), row(1, 63)]) }), /numerical predicate failed/);
  assert.equal(execute({ ...opts, resolveTensor: () => freeze([]) }).complete, true);
  assert.throws(() => execute({ ...opts, replayLimits: limits({ recordVisits: 7 }),
    checks: [check("grades", { kind: "tensorBladeGrades", tensor: tensor(0), allowed: [2, 5] })] }), /recordVisits ceiling/);
});

test("diagnostic coefficient constants require both real and imaginary equality and include absent zero", () => {
  const opts = options({ checks: [check("constant", { kind: "complexCoefficientEqualsConstant", coefficient: selector(0), real: "-24", imaginary: "0" })],
    resolveTensor: () => freeze([{ ...row(), real: "-24" }]) });
  assert.equal(execute(opts).complete, true);
  assert.throws(() => execute({ ...opts, resolveTensor: () => freeze([{ ...row(), real: "-24", imaginary: "1" }]) }), /numerical predicate failed/);
  assert.throws(() => execute({ ...opts, resolveTensor: () => freeze([{ ...row(), real: "24" }]) }), /numerical predicate failed/);
  assert.equal(execute({ ...opts, checks: [check("absent", { kind: "complexCoefficientEqualsConstant", coefficient: selector(0, 1, 2), real: "0", imaginary: "0" })] }).complete, true);
});

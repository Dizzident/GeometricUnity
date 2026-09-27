"use strict";
// Manufactured expression metadata only; no coefficients or GU source factory.
const test = require("node:test"), assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { compileAuditConsumerSchedule, ERROR_FIELDS } = require("../a68-audit-consumer-schedule");
const tensor = node => ({ kind: "tensorNode", node }), literal = () => ({ kind: "literalZero" });
const coefficient = node => ({ tensor: tensor(node), form: 1, blade: 1, k0: 0, k1: 0, absent: "0" });
const check = (name, predicate) => ({ name, metadataPath: "check/" + name, predicate });
const scheduleLimits = () => ({ tensorNodes: 100, tensorEdges: 100, tensorMarks: 100, scalarNodes: 100,
  scalarEdges: 100, roots: 100, geometryMatrices: 100, geometryEntries: 100, maxDepth: 32, stringCharacters: 100000, rationalCharacters: 100 });
const limits = extra => ({ consumers: 100, expressionNodes: 1000, expressionEdges: 1000, tensorReferences: 1000,
  scalarReferences: 100, stringCharacters: 100000, maxDepth: 32, rationalCharacters: 100, retainedRootValues: 100, ...extra });
const plan = (count = 6) => { const r = new MixedRecipe(); for (let i = 0; i < count; i++) r.Unit(1, 1); return r.Finish(); };
const constant = value => ({ op: "constant", value });
const matrix = name => ({ op: "geometryMatrix", name });
const norms = () => ({ schemaVersion: "phase627-mixed-error-formula-obligations-v1", numericalReplayImplemented: false,
  fields: Object.fromEntries(ERROR_FIELDS.map(name => [name, name === "nativeFirstJetNorms" ? Array.from({ length: 14 }, () => ({ op: "maximumRowL1", matrix: matrix("frame") })) : constant("1")])) });
const run = (options = {}, cap = {}) => compileAuditConsumerSchedule({ tensorPlan: plan(), namedRoots: [], geometry: [], checks: [], domainChecks: [],
  scheduleLimits: scheduleLimits(), limits: limits(cap), ...options });

test("audit equality retains an otherwise dead early tensor through its late counterpart", () => {
  const result = run({ checks: [check("late", { kind: "tensorEqual", left: tensor(0), right: tensor(5) })] });
  assert.deepEqual(result.scalarSchedule.tensorLastUse, [0, 1, 2, 3, 4, 5]);
  assert.deepEqual(result.tensorLastUse, [5, 1, 2, 3, 4, 5]);
  assert.deepEqual(result.releaseByStep[0], []); assert.deepEqual(result.releaseByStep[5], [0, 5]);
  assert.equal(result.consumers[0].readyAfter, 5); assert.deepEqual(result.consumersByStep[6], [0]);
  assert.equal(result.scope.runtimeIntegrated, false); assert.equal(result.scope.numericPredicatesEvaluated, false);
});

test("nested conjunction collects all tensor and coefficient operands without creating literal-zero nodes", () => {
  const result = run({ checks: [check("all", { kind: "all", predicates: [
    { kind: "tensorZero", tensor: literal() },
    { kind: "complexCoefficientScaledEqual", left: coefficient(1), right: coefficient(4), factor: "-1/2" },
    { kind: "realCoefficientStrictLowerBound", coefficient: coefficient(3), imaginaryRequired: "0", lowerBound: "1/7" }
  ] })] });
  assert.deepEqual(result.consumers[0].tensorInputs, [1, 4, 3]); assert.equal(result.consumers[0].readyAfter, 4);
  assert.deepEqual(result.releaseByStep[4], [1, 3, 4]); assert.equal(result.tensorLastUse.length, 6);
  assert.equal(result.usage.tensorReferences, 3);
});

test("scalar roots are separately captured before scalar replay release and kept for later coefficient checks", () => {
  const namedRoots = [{ name: "early", expression: constant("2") }, { name: "late", expression: { op: "pair", left: 0, right: 5 } }];
  const result = run({ namedRoots, checks: [
    check("earlyRootLateCoefficient", { kind: "scalarRootEqualsRealCoefficient", root: "early", coefficient: coefficient(4), factor: "3" }),
    check("lateRootEarlyCoefficient", { kind: "scalarRootEqualsRealCoefficient", root: "late", coefficient: coefficient(1), factor: "-1" })
  ] });
  assert.deepEqual(result.captureRootsByStep[0], ["early"]); assert.deepEqual(result.releaseRootsByStep[5], ["early"]);
  assert.deepEqual(result.captureRootsByStep[6], ["late"]); assert.deepEqual(result.releaseRootsByStep[6], ["late"]);
  assert.equal(result.tensorLastUse[1], 5); assert.equal(result.maximumRetainedRootValues, 1);
  assert.deepEqual(result.scalarSchedule.scalarReleaseByExecution, [[0], [1]]);
});

test("root-cache peak includes simultaneous captures before consumer evaluation and release", () => {
  const namedRoots = ["a", "b"].map(name => ({ name, expression: constant("1") }));
  const checks = namedRoots.map(({ name }) => check(name, { kind: "scalarRootEqualsConstant", root: name, expected: "1" }));
  assert.equal(run({ namedRoots, checks }).maximumRetainedRootValues, 2);
  assert.throws(() => run({ namedRoots, checks }, { retainedRootValues: 1 }), /root value ceiling/);
});

test("complete error AST retains B and deltaB for a late majorant and schedules checks after error fields", () => {
  const error = norms();
  error.fields.fieldError = { op: "sourceBoundScalar", binding: "phase626/certifiedEpsilon", positiveRequired: true };
  error.fields.raw0Error = { op: "product", values: [{ op: "tensorL1", tensor: tensor(0) }, { op: "tensorL1", tensor: tensor(4) }, constant("3/2")] };
  error.fields.motionNorm = { op: "entryL1", value: matrix("frame") };
  error.fields.nativeFirstJetNorms[0] = { op: "maximumRowL1", matrix: { op: "matrixProduct", left: matrix("frame"), right: matrix("frame") } };
  const result = run({ error, geometry: [{ id: "frame", entries: [] }], checks: [check("errorZero", { kind: "metadataScalarsZero", fields: ["error/raw0Error"] })] });
  assert.equal(result.consumers.length, 20); assert.equal(result.tensorLastUse[0], 4);
  const norm = result.consumers.find(c => c.kind === "error" && c.name === "raw0Error"), checkRow = result.consumers.at(-1);
  assert.equal(norm.readyAfter, 4); assert.deepEqual(checkRow.consumerInputs, [norm.id]); assert.equal(checkRow.readyAfter, 4);
  assert.ok(result.executionOrder.indexOf(norm.id) < result.executionOrder.indexOf(checkRow.id));
  assert.equal(result.scope.errorFormulasEvaluated, false); assert.equal(result.scope.consumerResultStorageBounded, false);
});

test("geometry comparisons are explicit pre-tensor obligations, not passed booleans", () => {
  const left = "geometry/geometry.deltaMetric.dd[13][0]", right = "geometry/geometry.blockMetric.dd[13][0]";
  const result = run({ checks: [check("metric", { kind: "geometryEqual", left, right }),
    check("trace", { kind: "geometryTraceZero", matrix: "geometry/geometry.motion" }),
    check("curvature", { kind: "geometryZero", field: "geometry/geometry.deltaConnection.curvature" })] });
  assert.deepEqual(result.consumersByStep[0], [0, 1, 2]); assert.deepEqual(result.consumers[0].geometryFields, [left, right]);
  assert.ok(!JSON.stringify(result.consumers).includes('"passed":true'));
  for (const bad of ["geometry/geometry.deltaMetric.dd[14][0]", "geometry/geometry.deltaMetric.dd[01][0]", "arbitrary"])
    assert.throws(() => run({ checks: [check("bad", { kind: "geometryZero", field: bad })] }), /closed geometry/);
  assert.throws(() => run({ checks: [check("rank4trace", { kind: "geometryTraceZero", matrix: "geometry/geometry.deltaConnection.curvature" })] }), /trace requires/);
  assert.throws(() => run({ checks: [check("mixedRanks", { kind: "geometryEqual", left, right: "geometry/geometry.deltaConnection.curvature" })] }), /same geometry field shape/);
});

test("domain checks keep complete degree and H-anti obligations at tensor availability", () => {
  const domain = { name: "input/X", node: 2, degree: 1, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true };
  const result = run({ domainChecks: [domain] }); assert.equal(result.consumers[0].readyAfter, 2);
  assert.equal(result.consumers[0].kind, "domain");
  for (const change of [{ degree: 2 }, { hAntiHermitian: false }, { canonicalNonzeroRecords: false }])
    assert.throws(() => run({ domainChecks: [{ ...domain, ...change }] }), /domain obligation/);
});

test("closed menus reject missing dependencies, wrong schemas, weak selectors and incomplete errors", () => {
  for (const predicate of [{ kind: "unknown" }, { kind: "tensorZero", tensor: tensor(6) },
    { kind: "tensorZero", tensor: { kind: "literalZero", node: 0 } },
    { kind: "scalarRootEqualsConstant", root: "absent", expected: "1" },
    { kind: "complexCoefficientScaledEqual", left: coefficient(0), right: coefficient(1), factor: "2/4" },
    { kind: "realCoefficientStrictLowerBound", coefficient: { ...coefficient(0), k0: -0 }, imaginaryRequired: "0", lowerBound: "1" },
    { kind: "metadataScalarsZero", fields: ["error/raw0Error"] }]) assert.throws(() => run({ checks: [check("bad", predicate)] }), /menu|reference|descriptor|root|canonical|field|selector/);
  const error = norms(); delete error.fields.radius;
  assert.throws(() => run({ error, geometry: [{ id: "frame", entries: [] }] }), /descriptor/);
  assert.throws(() => run({ checks: [{ ...check("x", { kind: "tensorZero", tensor: tensor(0) }), metadataPath: "check/other" }] }), /binding/);
  const c = check("x", { kind: "tensorZero", tensor: tensor(0) }); assert.throws(() => run({ checks: [c, c] }), /unique consumer/);
});

test("consumer descriptor snapshots are immutable and reject accessor or cyclic expressions", () => {
  const source = check("eq", { kind: "tensorEqual", left: tensor(0), right: tensor(5) }), result = run({ checks: [source] });
  source.predicate.left.node = 4; assert.equal(result.consumers[0].descriptor.predicate.left.node, 0);
  assert.ok(Object.isFrozen(result.consumers[0].descriptor.predicate.left));
  const cycle = { kind: "all", predicates: [] }; cycle.predicates.push(cycle);
  assert.throws(() => run({ checks: [check("cyclic", cycle)] }), /acyclic/);
  let invoked = false; const bad = { kind: "tensorZero", get tensor() { invoked = true; return tensor(0); } };
  assert.throws(() => run({ checks: [check("getter", bad)] }), /descriptor/); assert.equal(invoked, false);
});

test("logical admission limits reject excess scans, references, consumers and strings", () => {
  const checks = [check("eq", { kind: "tensorEqual", left: tensor(0), right: tensor(5) })];
  assert.throws(() => run({ checks }, { tensorReferences: 1 }), /tensorReferences ceiling/);
  assert.throws(() => run({ checks }, { stringCharacters: 1 }), /stringCharacters ceiling/);
  assert.throws(() => run({ checks: [...checks, check("zero", { kind: "tensorZero", tensor: literal() })] }, { consumers: 1 }), /bounded dense|consumers ceiling/);
  assert.throws(() => run({}, { maxDepth: 129 }), /bounded admission/);
  const nested = { kind: "all", predicates: [{ kind: "tensorZero", tensor: tensor(0) }] };
  assert.throws(() => run({ checks: [check("deep", nested)] }, { maxDepth: 1 }), /bounded acyclic/);
  assert.deepEqual(run({ tensorPlan: plan(0) }).consumersByStep, [[]]);
});

test("inherited array map cannot skip native-jet formula validation or hide tensor dependencies", () => {
  const error = norms(); let invoked = false;
  error.fields.nativeFirstJetNorms[0] = { op: "tensorL1", tensor: tensor(0) };
  error.fields.nativeFirstJetNorms[13] = { op: "tensorL1", tensor: tensor(5) };
  const prototype = Object.create(Array.prototype);
  prototype.map = () => { invoked = true; return []; };
  Object.setPrototypeOf(error.fields.nativeFirstJetNorms, prototype);
  const result = run({ error, geometry: [{ id: "frame", entries: [] }] });
  const consumer = result.consumers.find(c => c.name === "nativeFirstJetNorms");
  assert.equal(invoked, false); assert.equal(consumer.descriptor.length, 14);
  assert.deepEqual(consumer.tensorInputs, [0, 5]); assert.equal(result.tensorLastUse[0], 5);
});

test("Ward scalar-array and ordered-name comparisons preserve all roots through cross-route checks", () => {
  const namedRoots = [{ name: "early", expression: constant("1") }, { name: "late", expression: { op: "pair", left: 0, right: 5 } }];
  const result = run({ namedRoots, checks: [check("routes", { kind: "all", predicates: [
    { kind: "scalarEqual", left: "early", right: "late" },
    { kind: "scalarArrayEqual", left: ["early", "late"], right: ["late", "early"] },
    { kind: "orderedNamesEqual", left: ["epsilon", "inverse"], right: ["epsilon", "inverse"] }
  ] })] });
  assert.equal(result.consumers[0].readyAfter, 5); assert.deepEqual(result.consumers[0].scalarRoots, ["early", "late"]);
  assert.deepEqual(result.releaseRootsByStep[6], ["early", "late"]); assert.equal(result.maximumRetainedRootValues, 2);
  assert.equal(result.usage.scalarReferences, 6, "every array reference charged before deduplication");
  for (const p of [{ kind: "scalarArrayEqual", left: ["early"], right: [] },
    { kind: "scalarArrayEqual", left: ["early"], right: ["unknown"] },
    { kind: "orderedNamesEqual", left: ["epsilon"], right: [true] }])
    assert.throws(() => run({ namedRoots, checks: [check("bad", p)] }), /array shape|root|string/);
});

test("diagnostic grade and complex constant checks preserve dependencies without invented tensor nodes", () => {
  const source = { kind: "tensorBladeGrades", tensor: tensor(0), allowed: [2, 5] };
  const result = run({ checks: [check("diagnostic", { kind: "all", predicates: [source,
    { kind: "complexCoefficientEqualsConstant", coefficient: coefficient(5), real: "-24", imaginary: "0" }] })] });
  assert.deepEqual(result.consumers[0].tensorInputs, [0, 5]); assert.equal(result.consumers[0].readyAfter, 5);
  assert.equal(result.tensorLastUse[0], 5); assert.equal(result.scalarSchedule.tensorLastUse.length, 6);
  source.allowed[0] = 1; assert.deepEqual(result.consumers[0].descriptor.predicate.predicates[0].allowed, [2, 5]);
  assert.ok(Object.isFrozen(result.consumers[0].descriptor.predicate.predicates[0].allowed));
  for (const allowed of [[], [5, 2], [2, 2], [-0], [-1], [15], [1.5], ["2"]])
    assert.throws(() => run({ checks: [check("bad", { kind: "tensorBladeGrades", tensor: tensor(0), allowed })] }), /grade/);
  assert.throws(() => run({ checks: [check("bad", { kind: "complexCoefficientEqualsConstant", coefficient: coefficient(0), real: "2/4", imaginary: "0" })] }), /canonical/);
});

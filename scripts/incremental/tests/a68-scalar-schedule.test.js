"use strict";

// Synthetic expression metadata only: no scalar/tensor evaluation or source
// geometry reconstruction. Hand counts verify the augmented release protocol.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { TOP_SCHEMA } = require("../a68-original-action-recipe");
const { compileScalarSchedule, SCHEMA } = require("../a68-scalar-schedule");
const limits = overrides => ({ tensorNodes: 1000, tensorEdges: 1000, tensorMarks: 100,
  scalarNodes: 1000, scalarEdges: 1000, roots: 100, geometryMatrices: 100,
  geometryEntries: 1000, maxDepth: 64, stringCharacters: 100000,
  rationalCharacters: 100, ...overrides });
const constant = value => ({ op: "constant", value });
const pair = (left, right) => ({ op: "pair", left, right });
const add = (left, right) => ({ op: "add", left, right });
const multiply = (left, right) => ({ op: "multiply", left, right });
const root = (name, expression) => ({ name, expression });
const plan = (count = 11) => {
  const r = new MixedRecipe(); for (let i = 0; i < count; i++) r.Unit(1, 0); return r.Finish();
};
const compile = (p, roots, geometry = [], overrides = {}) => compileScalarSchedule(p, roots, geometry, limits(overrides));
const clone = x => JSON.parse(JSON.stringify(x));
const top = (node, weight = "1/2") => ({ schemaVersion: TOP_SCHEMA, tensor: Object.freeze({}), node,
  degree: 14, form: 16383, blade: 0, k0: 0, k1: 0, absentCoefficient: "0",
  imaginaryRequired: "0", realFactor: "-1", weight });

test("scalar Pair keeps early unrelated node zero live until late node ten and releases ALL expired tensors", () => {
  const out = compile(plan(), [root("late", pair(0, 10))]);
  assert.equal(out.schemaVersion, SCHEMA);
  assert.deepEqual(out.tensorLastUse, [10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
  assert.deepEqual(out.releaseByStep[0], []); assert.deepEqual(out.releaseByStep[10], [0, 10]);
  assert.deepEqual(out.nodes, [{ id: 0, op: "pair", inputs: [], tensorInputs: [0, 10], parameters: {}, readyAfter: 10 }]);
  assert.deepEqual(out.rootComparisons, [{ root: 0, name: "late", scalar: 0, readyAfter: 10, afterScalarExecution: 0 }]);
  assert.deepEqual(out.scalarReleaseByExecution, [[0]]);
  assert.match(out.executionContract, /ALL tensors/);
  assert.equal(out.scope.totalProcessMemoryProved, false); assert.equal(out.scope.numericReplayImplemented, false);
});

test("scalar constants and declared matrix entries execute before tensor zero", () => {
  const a = constant("-2/3"), b = { op: "matrixEntry", matrix: "frame", row: 2, column: 3 };
  const out = compile(plan(0), [root("sum", add(a, b))], [{ id: "frame", entries: [{ row: 2, column: 3, value: "5/7" }] }]);
  assert.deepEqual(out.executionOrder, [0, 1, 2]); assert.deepEqual(out.nodes.map(x => x.readyAfter), [-1, -1, -1]);
  assert.deepEqual(out.nodes[2].inputs, [0, 1]); assert.deepEqual(out.tensorLastUse, []);
  assert.deepEqual(out.scalarLastUse, [2, 2, 2]); assert.deepEqual(out.scalarReleaseByExecution, [[], [], [0, 1, 2]]);
  assert.equal(out.maximumLiveScalarValues, 3); assert.equal(out.rootComparisons[0].afterScalarExecution, 2);
  assert.deepEqual(out.geometry[0].entries, [{ row: 2, column: 3, value: "5/7" }]);
});

test("scalar shared current identity is preserved without value-based deduplication", () => {
  const current = multiply(pair(0, 1), constant("1/2"));
  const out = compile(plan(2), [root("current", current), root("twice", add(current, current)),
    root("sameExpressionNewIdentity", multiply(pair(0, 1), constant("1/2")))]);
  assert.equal(out.nodes.length, 7); assert.equal(out.roots[0].node, 2); assert.equal(out.roots[1].node, 3); assert.equal(out.roots[2].node, 6);
  assert.deepEqual(out.nodes[3].inputs, [2, 2]); assert.deepEqual(out.executionOrder, [1, 5, 0, 2, 3, 4, 6]);
  assert.equal(out.scalarLastUse[2], 4, "execution position, not scalar ID");
  assert.deepEqual(out.scalarReleaseByExecution[4], [2, 3]);
  assert.equal(out.rootComparisons[0].afterScalarExecution, 3, "root comparison does not wait for later consumer");
  assert.equal(out.usage.scalarEdges, 6); assert.equal(out.maximumLiveScalarValues, 4);
});

test("scalar ready sorting reorders independent roots but retains child order and all root comparisons", () => {
  const late = pair(0, 10), early = pair(0, 2), out = compile(plan(), [root("late", late), root("early", early), root("lateAgain", late)]);
  assert.deepEqual(out.executionOrder, [1, 0]);
  assert.deepEqual(out.rootComparisons.map(x => x.name), ["early", "late", "lateAgain"]);
  assert.deepEqual(out.rootComparisons.map(x => x.afterScalarExecution), [0, 1, 1]);
  assert.deepEqual(out.scalarLastUse, [1, 0]); assert.equal(out.maximumLiveScalarValues, 1);
  assert.equal(out.tensorLastUse[0], 10);
});

test("scalar tensor DAG dependencies and typed marks remain included in liveness", () => {
  const r = new MixedRecipe(), x = r.Unit(1, 0); r.Unit(1, 0); const sum = r.Sum(x, x);
  r.Mark("sum", 1, sum, true); const p = r.Finish(), out = compile(p, [root("early", pair(0, 1))]);
  assert.deepEqual(out.tensorLastUse, [2, 1, 2]); assert.deepEqual(out.releaseByStep, [[], [1], [0, 2]]);
  assert.equal(out.usage.tensorEdges, 2); assert.equal(out.usage.tensorMarks, 1);
});

test("scalar TOP descriptor compiles selected imaginary check then minus sign and fixed weight without opaque handle", () => {
  const r = new MixedRecipe(); r.Unit(16383, 0); const out = compile(r.Finish(), [root("density", top(0, "453856"))]);
  assert.equal(out.nodes[0].op, "top"); assert.deepEqual(out.nodes[0].tensorInputs, [0]);
  assert.equal(out.nodes[0].parameters.realFactor, "-1"); assert.equal(out.nodes[0].parameters.weight, "453856");
  assert.equal(out.nodes[0].parameters.imaginaryRequired, "0");
  assert.ok(!JSON.stringify(out).includes('"tensor":'));
  assert.equal(out.scope.topHandleIdentityAuthenticated, false); assert.match(out.numericContract, /complete imaginary trace cancellation/);
  assert.ok(Object.isFrozen(out.nodes[0].parameters)); assert.ok(Object.isFrozen(out.releaseByStep[0]));
});

test("scalar untyped zero Pair and TOP follow typed-zero boundary semantics", () => {
  const r = new MixedRecipe(); r.Zero(); r.Unit(1, 0);
  const out = compile(r.Finish(), [root("zeroPair", pair(0, 1)), root("zeroTop", top(0))]);
  assert.equal(out.nodes.length, 2); assert.deepEqual(out.executionOrder, [1, 0]);
});

test("scalar rejects foreign tensor IDs, degree disagreement and malformed TOP selectors", () => {
  for (const expression of [pair(-1, 0), pair(0, 11), pair("0", 0), pair(0.5, 0), top(0)])
    assert.throws(() => compile(plan(), [root("bad", expression)]), /foreign tensor|TOP tensor degree/);
  const r = new MixedRecipe(); r.Unit(1, 0); r.Unit(3, 0);
  assert.throws(() => compile(r.Finish(), [root("bad", pair(0, 1))]), /Pair degree/);
  const t = new MixedRecipe(); t.Unit(16383, 0); const p = t.Finish();
  for (const mutation of [{ schemaVersion: "wrong" }, { realFactor: "1" }, { weight: "2" },
    { imaginaryRequired: "ignore" }, { k0: 1 }, { degree: 13 }, { blade: 1 }, { extra: true }])
    assert.throws(() => compile(p, [root("bad", { ...top(0), ...mutation })]), /TOP|closed descriptor/);
});

test("scalar closed shapes, canonical constants, declared geometry and unique names are mandatory", () => {
  for (const expression of [{ op: "unknown" }, { op: "constant", value: "1", extra: 1 },
    constant("2/4"), constant("-0"), constant("01"), constant("1e3"), constant("1/1"),
    { op: "pair", left: 0 }, { op: "add", left: constant("1"), right: null },
    { op: "matrixEntry", matrix: "missing", row: 0, column: 0 }])
    assert.throws(() => compile(plan(1), [root("bad", expression)]), /scalar|canonical|closed|declared/);
  assert.throws(() => compile(plan(1), [root("x", constant("1")), root("x", constant("2"))]), /unique scalar root/);
  assert.throws(() => compile(plan(), [root("\u00e9", constant("1"))]), /ASCII/);
  assert.throws(() => compile(plan(), [], [{ id: "x", entries: [] }, { id: "x", entries: [] }]), /unique declared geometry/);
  assert.throws(() => compile(plan(), [], [{ id: "x", entries: [{ row: 0, column: 0, value: "0" }] }]), /zero omitted/);
  assert.throws(() => compile(plan(), [], [{ id: "x", entries: new Array(1) }]), /dense/);
  assert.throws(() => compile(plan(), new Array(1)), /dense/);
  let invoked = false; const accessor = { get op() { invoked = true; return "constant"; }, value: "1" };
  assert.throws(() => compile(plan(), [root("bad", accessor)]), /closed scalar/); assert.equal(invoked, false);
});

test("scalar cycles and longest shared-reference path depth are rejected", () => {
  const cycle = add(constant("1"), null); cycle.right = cycle;
  assert.throws(() => compile(plan(0), [root("cycle", cycle)]), /cyclic/);
  const shared = add(constant("1"), constant("2"));
  assert.throws(() => compile(plan(0), [root("first", shared), root("deep", add(constant("0"), shared))], [], { maxDepth: 2 }), /shared scalar depth/);
  assert.throws(() => compile(plan(0), [root("deep", add(add(constant("1"), constant("2")), constant("0")))], [], { maxDepth: 2 }), /depth ceiling/);
});

test("scalar explicit finite admission caps cover nodes, edges, roots, geometry, strings and depth", () => {
  assert.throws(() => compileScalarSchedule(plan(), [], [], undefined), /closed descriptor/);
  assert.throws(() => compile(plan(), [], [], { maxDepth: 513 }), /bounded depth/);
  assert.throws(() => compile(plan(), [], [], { scalarNodes: Infinity }), /explicit positive/);
  assert.throws(() => compile(plan(), [], [], { extra: 1 }), /closed descriptor/);
  assert.throws(() => compile(plan(2), [], [], { tensorNodes: 1 }), /tensorNodes ceiling/);
  assert.throws(() => compile(plan(0), [root("x", add(constant("1"), constant("2")))], [], { scalarNodes: 2 }), /scalarNodes ceiling/);
  assert.throws(() => compile(plan(0), [root("x", add(constant("1"), constant("2")))], [], { scalarEdges: 1 }), /scalarEdges ceiling/);
  assert.throws(() => compile(plan(0), [root("a", constant("1")), root("b", constant("1"))], [], { roots: 1 }), /roots ceiling/);
  assert.throws(() => compile(plan(0), [], [{ id: "a", entries: [] }, { id: "b", entries: [] }], { geometryMatrices: 1 }), /geometryMatrices ceiling/);
  assert.throws(() => compile(plan(0), [], [{ id: "a", entries: [{ row: 0, column: 0, value: "1" }, { row: 1, column: 1, value: "1" }] }], { geometryEntries: 1 }), /geometryEntries ceiling/);
  assert.throws(() => compile(plan(0), [root("long", constant("1"))], [], { stringCharacters: 2 }), /stringCharacters ceiling/);
  assert.throws(() => compile(plan(0), [root("x", constant("123"))], [], { rationalCharacters: 2 }), /bounded canonical/);
});

test("scalar tensor plan rejects malformed topology, degrees, primitive shapes and marks", () => {
  for (const mutate of [p => { p.nodes[0].id = 1; }, p => { p.nodes[0].degree = 2; },
    p => { p.nodes[0].op = "opaque"; }, p => { p.nodes[0].parameters.extra = 1; },
    p => { p.nodes[0].inputs = [0]; }, p => { p.nodes[0].inputs = new Array(1); },
    p => { p.marks = [{ name: "bad", degree: 2, node: 0, expanded: true }]; }]) {
    const p = clone(plan(1)); mutate(p); assert.throws(() => compile(p, []), /topology|degree|primitive|closed|dependency|dense|mark/);
  }
});

test("scalar plan boundary accepts the complete independently generated primitive menu", () => {
  const r = new MixedRecipe({ leaves: [{ id: "x", degree: 1, source: "synthetic-only", sha256: "0".repeat(64) }],
    matrices: [{ name: "matrix", matrix: [{ row: 0, column: 1, value: "1/2" }] }] });
  const x = r.RegisterLeaf("x"), matrix = r.Matrix("matrix"), z = r.Zero(), unit = r.Unit(1, 2);
  r.Sum(x, z); r.Scale(unit, "-1", "1"); const product = r.P(x, unit, "C");
  r.Transpose(x, product, "A"); const star = r.Hodge(x); r.HodgeAdjoint(star, 1);
  r.Motion(matrix, x); r.Pullback(matrix, x); r.Spin(matrix); r.Raise(x); r.Contract(x, 0); r.Component(x, 1);
  r.Mark("typed", 1, x, false);
  const p = r.Finish(), out = compile(p, [root("pair", pair(0, 0))]);
  assert.equal(out.tensorLastUse.length, p.nodes.length);
  assert.equal(out.usage.tensorNodes, 15);
  assert.deepEqual(new Set(p.nodes.map(n => n.op)), new Set(["leaf", "zero", "unit", "sum", "scale",
    "product", "transpose", "star", "starAdjoint", "motion", "pullback", "spin", "raise", "contract", "component"]));
});

test("scalar complete release protocol closes every symbolic value without reading an expired operand", () => {
  const p = plan(), shared = multiply(pair(0, 10), constant("1/2"));
  const out = compile(p, [root("late", shared), root("double", add(shared, shared)), root("early", pair(0, 3))]);
  const tensors = new Set(), scalars = new Set(), compared = [];
  let position = 0, peak = 0;
  for (let tensorStep = -1; tensorStep < p.nodes.length; tensorStep++) {
    if (tensorStep >= 0) tensors.add(tensorStep);
    while (position < out.executionOrder.length && out.nodes[out.executionOrder[position]].readyAfter === tensorStep) {
      const node = out.nodes[out.executionOrder[position]];
      node.tensorInputs.forEach(id => assert.ok(tensors.has(id), "live tensor " + id));
      node.inputs.forEach(id => assert.ok(scalars.has(id), "live scalar " + id));
      scalars.add(node.id); peak = Math.max(peak, scalars.size);
      for (const comparison of out.rootComparisons.filter(x => x.afterScalarExecution === position)) {
        assert.ok(scalars.has(comparison.scalar)); compared.push(comparison.name);
      }
      out.scalarReleaseByExecution[position].forEach(id => assert.ok(scalars.delete(id)));
      position++;
    }
    if (tensorStep >= 0) out.releaseByStep[tensorStep].forEach(id => assert.ok(tensors.delete(id)));
  }
  assert.deepEqual(compared, ["early", "late", "double"]);
  assert.equal(position, out.nodes.length); assert.equal(scalars.size, 0); assert.equal(tensors.size, 0);
  assert.equal(peak, out.maximumLiveScalarValues);
});

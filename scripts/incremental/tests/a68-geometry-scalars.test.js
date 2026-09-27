"use strict";

// Synthetic exact metadata only. No metric reconstruction, scientific tensor
// coefficients, evaluator or retained numerical replay is invoked.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");

const matrices = [
  { name: "a", matrix: [
    { row: 0, column: 0, value: "1/2" }, { row: 0, column: 7, value: "-2/3" },
    { row: 7, column: 0, value: "3/5" }, { row: 7, column: 7, value: "-1/6" }
  ] },
  { name: "b", matrix: [
    { row: 0, column: 0, value: "3/4" }, { row: 0, column: 7, value: "1/3" },
    { row: 2, column: 2, value: "-5/7" }, { row: 7, column: 0, value: "-1/5" },
    { row: 7, column: 7, value: "1/2" }
  ] },
  { name: "zero", matrix: [] }
];
const create = () => new MixedRecipe({ matrices, constants: ["0", "1", "-1", "1/2", "2/3", "-3/2"] });

test("geometry scalars: declared constants and matrix coefficients remain exact canonical metadata", () => {
  const recipe = create(), a = recipe.Matrix("a");
  const half = recipe.ScalarConstant("1/2"), negative = recipe.MatrixCoefficient(a, 0, 7);
  assert.equal(recipe.ScalarValue(half), "1/2"); assert.equal(recipe.ScalarValue(negative), "-2/3");
  assert.equal(recipe.ScalarValue(recipe.MatrixCoefficient(a, 13, 13)), "0");
  assert.ok(Object.isFrozen(half)); assert.deepEqual(Object.keys(half), []);
  assert.throws(() => recipe.ScalarConstant("3/7"), /undeclared/);
  assert.throws(() => recipe.ScalarConstant("2/4"), /canonical/);
  assert.throws(() => recipe.MatrixCoefficient(a, 14, 0), /indices/);
  assert.equal(recipe.Finish().nodes.length, 0, "scalar metadata creates no tensor DAG nodes");
});

test("geometry scalars: addition, multiplication and negative trace use exact reduced rationals", () => {
  const recipe = create(), a = recipe.Matrix("a"), half = recipe.MatrixCoefficient(a, 0, 0), minusTwoThirds = recipe.MatrixCoefficient(a, 0, 7);
  assert.equal(recipe.ScalarValue(recipe.ScalarAdd(half, minusTwoThirds)), "-1/6");
  assert.equal(recipe.ScalarValue(recipe.ScalarMultiply(half, minusTwoThirds)), "-1/3");
  assert.equal(recipe.ScalarValue(recipe.NegativeMatrixTrace(a)), "-1/3");
  assert.equal(recipe.ScalarValue(recipe.NegativeMatrixTrace(recipe.Matrix("zero"))), "0");
  const opposite = recipe.ScalarMultiply(minusTwoThirds, recipe.ScalarConstant("-1"));
  assert.equal(recipe.ScalarValue(recipe.ScalarAdd(minusTwoThirds, opposite)), "0");
  assert.equal(recipe.ScalarValue(recipe.ScalarMultiply(minusTwoThirds, recipe.ScalarConstant("0"))), "0");
  assert.equal(recipe.Finish().nodes.length, 0);
});

test("geometry scalar scaling emits exactly one typed scale node with independently derived rational", () => {
  const recipe = create(), value = recipe.Unit(1, 1), scalar = recipe.NegativeMatrixTrace(recipe.Matrix("a"));
  recipe.Mark("scaled", 1, recipe.ScaleWithScalar(value, scalar), true);
  const plan = recipe.Finish();
  assert.deepEqual(plan.nodes[1], { id: 1, op: "scale", degree: 1, inputs: [0], parameters: { real: "-1/3", imaginary: "0" } });
  assert.equal(plan.nodes.length, 2); assert.equal(plan.marks[0].node, 1);
});

test("geometry combination: nontrivial rational matrices match a hand-derived exact result", () => {
  const recipe = create(), a = recipe.Matrix("a"), b = recipe.Matrix("b"), zero = recipe.Matrix("zero");
  const factors = Array.from({ length: 14 }, () => recipe.ScalarConstant("0"));
  factors[0] = recipe.ScalarConstant("2/3"); factors[13] = recipe.ScalarConstant("-3/2");
  const terms = Array(14).fill(zero); terms[0] = a; terms[13] = b;
  const combined = recipe.CombineMatrices(terms, factors);
  assert.deepEqual(recipe.MatrixEntries(combined), [
    { row: 0, column: 0, value: "-19/24" }, { row: 0, column: 7, value: "-17/18" },
    { row: 2, column: 2, value: "15/14" }, { row: 7, column: 0, value: "7/10" },
    { row: 7, column: 7, value: "-31/36" }
  ]);
  assert.ok(Object.isFrozen(recipe.MatrixEntries(combined)));
  assert.throws(() => { recipe.MatrixEntries(combined)[0].value = "0"; }, TypeError);
  assert.equal(recipe.Finish().nodes.length, 0, "matrix arithmetic remains metadata, not hidden tensor operations");
});

test("geometry combination: all fourteen terms participate, with canonical exact cancellation", () => {
  const recipe = create(), a = recipe.Matrix("a");
  const sevenA = recipe.CombineMatrices(Array(14).fill(a), Array.from({ length: 14 }, () => recipe.ScalarConstant("1/2")));
  assert.deepEqual(recipe.MatrixEntries(sevenA), [
    { row: 0, column: 0, value: "7/2" }, { row: 0, column: 7, value: "-14/3" },
    { row: 7, column: 0, value: "21/5" }, { row: 7, column: 7, value: "-7/6" }
  ]);
  const cancelled = recipe.CombineMatrices(Array(14).fill(a), Array.from({ length: 14 }, (_, i) => recipe.ScalarConstant(i < 7 ? "1" : "-1")));
  assert.deepEqual(recipe.MatrixEntries(cancelled), [], "sparse matrix omits exact zero entries");
});

test("geometry scalar identities cannot cross sessions or be replaced by raw numbers/tensors", () => {
  const recipe = create(), other = create(), value = recipe.Unit(1, 1), one = recipe.ScalarConstant("1"), foreign = other.ScalarConstant("1");
  for (const fake of [foreign, {}, "1", value, other.Matrix("a")]) {
    assert.throws(() => recipe.ScalarValue(fake), /foreign geometry scalar/);
    assert.throws(() => recipe.ScalarAdd(one, fake), /foreign geometry scalar/);
    assert.throws(() => recipe.ScalarMultiply(fake, one), /foreign geometry scalar/);
    assert.throws(() => recipe.ScaleWithScalar(value, fake), /foreign geometry scalar/);
  }
  assert.throws(() => recipe.MatrixCoefficient(other.Matrix("a"), 0, 0), /undeclared geometry matrix/);
  assert.throws(() => recipe.NegativeMatrixTrace(other.Matrix("a")), /undeclared geometry matrix/);
  assert.throws(() => recipe.ScaleWithScalar(other.Unit(1, 1), one), /foreign\/unregistered tensor/);
});

test("geometry combinations reject incomplete arrays and wrong-session matrix/scalar members", () => {
  const recipe = create(), other = create(), a = recipe.Matrix("a"), one = recipe.ScalarConstant("1");
  assert.throws(() => recipe.CombineMatrices(Array(13).fill(a), Array(14).fill(one)), /fourteen-slot/);
  assert.throws(() => recipe.CombineMatrices(Array(14).fill(a), Array(15).fill(one)), /fourteen-slot/);
  assert.throws(() => recipe.CombineMatrices(Array(14).fill(other.Matrix("a")), Array(14).fill(one)), /undeclared geometry matrix/);
  assert.throws(() => recipe.CombineMatrices(Array(14).fill(a), Array(14).fill(other.ScalarConstant("1"))), /foreign geometry scalar/);
  assert.throws(() => recipe.CombineMatrices(Array(14).fill(a), Array(14).fill("1")), /foreign geometry scalar/);
});

test("geometry scalar and matrix APIs remain closed after recipe finalization", () => {
  const recipe = create(), a = recipe.Matrix("a"), one = recipe.ScalarConstant("1"), value = recipe.Unit(1, 1); recipe.Finish();
  for (const operation of [
    () => recipe.ScalarValue(one), () => recipe.ScalarConstant("1"), () => recipe.MatrixCoefficient(a, 0, 0),
    () => recipe.ScalarAdd(one, one), () => recipe.ScalarMultiply(one, one), () => recipe.NegativeMatrixTrace(a),
    () => recipe.ScaleWithScalar(value, one), () => recipe.CombineMatrices(Array(14).fill(a), Array(14).fill(one))
  ]) assert.throws(operation, /closed recipe/);
});

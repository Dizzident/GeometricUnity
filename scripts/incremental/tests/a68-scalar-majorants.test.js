"use strict";
// Analytic metadata fixtures only: no tensor/scalar replay, source geometry,
// observed coefficient support or physical study evaluation.
const test = require("node:test"), assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { TOP_SCHEMA } = require("../a68-original-action-recipe");
const { deriveScalarResourceMajorants, SCHEMA } = require("../a68-scalar-majorants");
const limits = overrides => ({ maxBits: 256, tensorRecordPeak: "1000000", tensorDictionaryPeak: "1000000",
  scalarValuePeak: "1000000", combinedRationalSlotPeak: "10000000", scalarArithmeticIntegerBits: "1000000", ...overrides });
const scheduleLimits = { tensorNodes: 1000, tensorEdges: 10000, tensorMarks: 1000, scalarNodes: 1000,
  scalarEdges: 10000, roots: 1000, geometryMatrices: 100, geometryEntries: 1000, maxDepth: 64,
  stringCharacters: 1000000, rationalCharacters: 100 };
const hash = "0".repeat(64), leaf = (id, degree) => ({ id, degree, source: "synthetic/" + id, sha256: hash });
const lb = (id, support, denominator, norm) => ({ id, source: "synthetic/" + id, sha256: hash, support, denominator, norm });
const constant = value => ({ op: "constant", value }), pair = (left, right) => ({ op: "pair", left, right });
const add = (left, right) => ({ op: "add", left, right }), multiply = (left, right) => ({ op: "multiply", left, right });
const root = (name, expression) => ({ name, expression });
const top = (node, weight = "1/3") => ({ schemaVersion: TOP_SCHEMA, tensor: {}, node, degree: 14,
  form: 16383, blade: 0, k0: 0, k1: 0, absentCoefficient: "0", imaginaryRequired: "0", realFactor: "-1", weight });
function options(plan, namedRoots, leafBounds = [], geometry = [], overrides = {}) {
  return { tensorPlan: plan, namedRoots, geometry, leafBounds,
    validateLeafBound: (actual, declared) => {
      assert.ok(Object.isFrozen(actual)); assert.equal(actual.id, declared.id);
      assert.deepEqual(actual, leafBounds.find(b => b.id === actual.id)); return true;
    }, scheduleLimits, limits: limits(overrides) };
}
const run = (r, roots, bounds = [], geometry = [], overrides = {}) => deriveScalarResourceMajorants(options(r.Finish(), roots, bounds, geometry, overrides));

test("constant/matrix-entry/add bounds use absolute magnitudes, lcm denominator and exact numerator heights", () => {
  const r = new MixedRecipe(), geometry = [{ id: "frame", entries: [{ row: 2, column: 3, value: "5/7" }] }];
  const result = run(r, [root("sum", add(constant("-2/3"), { op: "matrixEntry", matrix: "frame", row: 2, column: 3 }))], [], geometry);
  assert.equal(result.schemaVersion, SCHEMA);
  assert.deepEqual(result.nodes.map(n => [n.norm, n.denominator, n.numeratorBound, n.rationalCharacters]),
    [["2/3", "3", "2", "4"], ["5/7", "7", "5", "4"], ["29/21", "21", "29", "6"]]);
  assert.deepEqual(result.peaks, { tensorRecordPeak: "0", tensorDictionaryPeak: "0", scalarValuePeak: "3", combinedRationalSlotPeak: "8" });
  assert.equal(result.nodes[2].arithmeticIntegerBits, "11"); assert.deepEqual(result.scalarExecutionOrder, [0, 1, 2]);
  assert.equal(result.nodes[1].recordVisits, "2", "declared matrix lookup and its sparse entries are both scanned");
  assert.equal(result.scalarReducedRationalCharacterPeak, "6"); assert.ok(Object.isFrozen(result.nodes[2]));
});

test("multiplication denominator remains a common product even when analytic magnitude cancels factors", () => {
  const result = run(new MixedRecipe(), [root("product", multiply(constant("2/3"), constant("-3/5")))]);
  const n = result.nodes[2];
  assert.deepEqual([n.norm, n.denominator, n.numeratorBound, n.rationalCharacters], ["2/5", "15", "6", "5"]);
  assert.equal(n.reducedIntegerBits, "4"); assert.equal(n.arithmeticIntegerBits, "9");
});

test("Pair uses tensor L1-product magnitude and product denominator, with complex scratch not a real-only shortcut", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 1), leaf("b", 1)] }); r.RegisterLeaf("a"); r.RegisterLeaf("b");
  const result = run(r, [root("pair", pair(0, 1))], [lb("a", "2", "6", "3/2"), lb("b", "3", "10", "5/3")]);
  const n = result.nodes[0];
  assert.deepEqual([n.norm, n.denominator, n.numeratorBound, n.reducedIntegerBits], ["5/2", "60", "150", "8"]);
  assert.equal(n.scratchRationalSlots, "32"); assert.equal(n.arithmeticIntegerBits, "144");
  assert.equal(n.recordVisits, "10"); assert.equal(n.matchedPairProducts, "2");
  assert.equal(n.coefficientReads, "18"); assert.equal(n.arithmeticOperations, "40");
  assert.equal(n.tensorResolutions, "2"); assert.equal(result.scalarReplayWork.rationalObjects, "42");
  assert.deepEqual(result.peaks, { tensorRecordPeak: "5", tensorDictionaryPeak: "2", scalarValuePeak: "1", combinedRationalSlotPeak: "44" });
  assert.equal(result.scope.fullImaginaryPairAccumulatorIncluded, true);
});

test("complex tensor norm feeds Pair even when real scalar output is required", () => {
  const r = new MixedRecipe({ constants: ["0", "1", "3/2", "5/3"] }); r.Unit(1, 0, "0", "3/2"); r.Unit(1, 0, "1", "5/3");
  const result = run(r, [root("complex", pair(0, 1))]);
  assert.equal(result.nodes[0].norm, "4"); assert.equal(result.nodes[0].denominator, "6");
  assert.equal(result.nodes[0].numeratorBound, "24"); assert.equal(result.nodes[0].scratchRationalSlots, "32");
  assert.equal(result.scope.scientificCoefficientsEvaluated, false);
});

test("Top includes exact weight, real sign, selected imaginary check and full tensor lifetime", () => {
  const r = new MixedRecipe({ leaves: [leaf("top", 14)] }); r.RegisterLeaf("top");
  const result = run(r, [root("weighted", top(0))], [lb("top", "4", "6", "7/3")]);
  const n = result.nodes[0];
  assert.deepEqual([n.norm, n.denominator, n.numeratorBound, n.reducedIntegerBits], ["7/9", "18", "14", "5"]);
  assert.equal(n.arithmeticIntegerBits, "11"); assert.equal(n.scratchRationalSlots, "8");
  assert.equal(result.scalarReplayWork.recordVisits, "8"); assert.equal(result.scalarReplayWork.coefficientReads, "11");
  assert.equal(result.scalarReplayWork.arithmeticOperations, "13"); assert.equal(result.peaks.combinedRationalSlotPeak, "18");
});

test("zero bounds normalize denominator but do not suppress full Pair scratch or declared geometry validation", () => {
  const r = new MixedRecipe(); r.Zero(); r.Unit(1, 0);
  const result = run(r, [root("zero", pair(0, 1)), root("absent", { op: "matrixEntry", matrix: "m", row: 1, column: 2 })], [], [{ id: "m", entries: [] }]);
  assert.deepEqual(result.nodes.map(n => [n.norm, n.denominator, n.numeratorBound]), [["0", "1", "0"], ["0", "1", "0"]]);
  assert.equal(result.nodes[0].scratchRationalSlots, "32"); assert.deepEqual(result.scalarExecutionOrder, [1, 0]);
});

test("zero Top still includes parsed weight height and alias Pair still validates each operand", () => {
  const z = new MixedRecipe(); z.Zero();
  const zero = run(z, [root("zero", top(0, "453856"))]);
  assert.equal(zero.nodes[0].rationalCharacters, "1"); assert.equal(zero.nodes[0].parameterRationalCharacters, "6");
  assert.equal(zero.scalarReducedRationalCharacterPeak, "6"); assert.equal(zero.nodes[0].arithmeticIntegerBits, "39");
  const r = new MixedRecipe({ leaves: [leaf("a", 1)] }); r.RegisterLeaf("a");
  const alias = run(r, [root("alias", pair(0, 0))], [lb("a", "3", "1", "2")]);
  assert.equal(alias.scalarReplayWork.tensorResolutions, "2"); assert.equal(alias.scalarReplayWork.recordVisits, "12");
  assert.equal(alias.scalarReplayWork.coefficientReads, "24");
  assert.equal(alias.peaks.tensorRecordPeak, "3", "aliases are two validations but one retained tensor dictionary");
});

test("an early tensor survives unrelated tensor nodes until a late scalar Pair consumes it", () => {
  const r = new MixedRecipe(); for (let i = 0; i < 11; i++) r.Unit(1, 0);
  const result = run(r, [root("late", pair(0, 10))]);
  assert.equal(result.tensorMajorants.replayTensorRecordPeak, "1", "old tensor-only liveness is insufficient");
  assert.equal(result.peaks.tensorRecordPeak, "2"); assert.equal(result.peaks.tensorDictionaryPeak, "2");
  assert.equal(result.peaks.combinedRationalSlotPeak, "38");
  assert.deepEqual(result.tensorLastUse, [10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
  assert.deepEqual(result.peakWitnesses.combinedRationalSlotPeak, { type: "scalar", id: 0, tensorAfter: 10 });
});

test("constants and shared scalar results stay live by scalar execution position, not scalar node ID", () => {
  const r = new MixedRecipe(); r.Unit(1, 0); r.Unit(1, 0);
  const current = multiply(pair(0, 1), constant("1/2"));
  const result = run(r, [root("current", current), root("twice", add(current, current)), root("another", multiply(pair(0, 1), constant("1/2")))]);
  assert.deepEqual(result.scalarExecutionOrder, [1, 5, 0, 2, 3, 4, 6]);
  assert.equal(result.scalarLastUse[2], 4); assert.equal(result.peaks.scalarValuePeak, "4");
  assert.equal(result.peaks.combinedRationalSlotPeak, "40", "two constants coexist with first Pair scratch, both tensors and two arithmetic context values");
});

test("tensor pullback scratch combines with already-live scalar constants", () => {
  const matrix = { name: "m", matrix: [{ row: 0, column: 0, value: "1/2" }, { row: 0, column: 1, value: "-3/2" }, { row: 7, column: 0, value: "2" }] };
  const r = new MixedRecipe({ leaves: [leaf("a", 2)], matrices: [matrix] }), a = r.RegisterLeaf("a"); r.Pullback(r.Matrix("m"), a);
  const result = run(r, [root("weighted", multiply(pair(0, 1), constant("1/2")))], [lb("a", "3", "6", "5")]);
  assert.equal(result.peaks.tensorRecordPeak, "31"); assert.equal(result.peaks.tensorDictionaryPeak, "7");
  // At tensor1: (3input+28primitive scratch)*2+1 waiting scalar+2context=65.
  // At Pair:15 retained records*2+1 waiting scalar+32scratch+2context=65.
  assert.equal(result.peaks.combinedRationalSlotPeak, "65");
  assert.deepEqual(result.peakWitnesses.combinedRationalSlotPeak, { type: "tensor", id: 1 });
});

test("ceil numerator bound handles a norm bound whose denominator does not divide the tensor denominator", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 14)] }); r.RegisterLeaf("a");
  const result = run(r, [root("top", top(0, "1"))], [lb("a", "1", "2", "4/3")]);
  assert.equal(result.nodes[0].numeratorBound, "3"); assert.equal(result.nodes[0].denominator, "2");
});

test("two arithmetic context rationals remain live even for an empty schedule", () => {
  const empty = run(new MixedRecipe(), []);
  assert.equal(empty.peaks.combinedRationalSlotPeak, "2");
  assert.deepEqual(empty.peakWitnesses.combinedRationalSlotPeak, { type: "arithmeticContext" });
  assert.equal(empty.scalarReplayWork.rationalObjects, "2");
  const scalar = run(new MixedRecipe(), [root("constant", constant("2"))]);
  assert.equal(scalar.peaks.combinedRationalSlotPeak, "3");
  assert.throws(() => run(new MixedRecipe(), [], [], [], { combinedRationalSlotPeak: "1" }), /combinedRationalSlotPeak ceiling/);
});

test("all explicit ceilings fail closed and analytic multiplication is guarded before oversized BigInts", () => {
  for (const [key, value] of [["tensorRecordPeak", "1"], ["tensorDictionaryPeak", "1"], ["combinedRationalSlotPeak", "35"], ["scalarArithmeticIntegerBits", "31"]]) {
    const r = new MixedRecipe(); r.Unit(1, 0); r.Unit(1, 0);
    assert.throws(() => run(r, [root("pair", pair(0, 1))], [], [], { [key]: value }), new RegExp(key + " ceiling"));
  }
  assert.throws(() => run(new MixedRecipe(), [root("sum", add(constant("1"), constant("1")))], [], [], { scalarValuePeak: "2" }), /scalarValuePeak ceiling/);
  assert.throws(() => run(new MixedRecipe(), [root("large", multiply(constant("4294967296"), constant("4294967296")))], [], [], { maxBits: 64 }), /pre-product analytic maxBits/);
  const empty = new MixedRecipe().Finish(), original = options(empty, []);
  for (const bad of [{ ...original, limits: {} }, { ...original, limits: limits({ maxBits: 63 }) },
    { ...original, limits: limits({ tensorRecordPeak: "0" }) }, { ...original, limits: limits({ scalarValuePeak: "01" }) }])
    assert.throws(() => deriveScalarResourceMajorants(bad), /descriptor|ceiling|canonical/);
});

test("no observed tensor report or schedule is accepted and source-bound leaf proof remains mandatory", () => {
  const r = new MixedRecipe({ leaves: [leaf("a", 1)] }); r.RegisterLeaf("a");
  const input = options(r.Finish(), [root("pair", pair(0, 0))], [lb("a", "1", "1", "1")]);
  assert.throws(() => deriveScalarResourceMajorants({ ...input, validateLeafBound: () => false }), /source proof/);
  assert.throws(() => deriveScalarResourceMajorants({ ...input, observedTensorReport: {} }), /closed/);
  assert.throws(() => deriveScalarResourceMajorants({ ...input, schedule: {} }), /closed/);
  const result = deriveScalarResourceMajorants(input);
  assert.equal(result.scope.tensorBoundsIndependentlyRederived, true);
  for (const key of ["tensorPrimitiveArithmeticScratchIncluded", "metadataMemoryProved", "wireRowParserStorageIncluded", "bigintAndGcdInternalMemoryProved", "wholeProcessMemoryProved", "completeRuntimeResourceProof"])
    assert.equal(result.scope[key], false);
});

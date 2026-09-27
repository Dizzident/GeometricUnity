"use strict";

// Integer count identities only. No source geometry module, GeometryAlgebra
// instance, coefficient matrix or scientific point is evaluated by this file.
const test = require("node:test"), assert = require("node:assert/strict");
const { geometryPrimitiveCost, deriveConnectionVariationCosts, SCHEMA, COUNTERS } = require("../a68-geometry-costs");
const limits = overrides => ({ maxBits: 128, scalarOperations: "100000000000000", rationalObjects: "100000000000000",
  matrixObjects: "100000000000000", matrixCells: "100000000000000", slotVisits: "100000000000000", ...overrides });
const derive = (dimension, calls = 1, includeArithmeticContext = true, overrides = {}) =>
  deriveConnectionVariationCosts({ dimension, calls, includeArithmeticContext, limits: limits(overrides) });
const values = row => COUNTERS.map(key => row[key]);

test("primitive arithmetic charges include subtract two, sum's zero seed, and no scalar work for transpose", () => {
  assert.deepEqual(values(geometryPrimitiveCost("subtract", 2)), ["2", "2", "0", "0", "0"]);
  assert.deepEqual(values(geometryPrimitiveCost("sum", 2, 3)), ["3", "3", "0", "0", "0"]);
  assert.deepEqual(values(geometryPrimitiveCost("sum", 2, 0)), ["0", "0", "0", "0", "0"]);
  assert.deepEqual(values(geometryPrimitiveCost("matAdd", 2, 2)), ["8", "8", "1", "4", "4"]);
  assert.deepEqual(values(geometryPrimitiveCost("transpose", 2)), ["0", "0", "1", "4", "4"]);
  assert.deepEqual(values(geometryPrimitiveCost("matScale", 2)), ["4", "4", "1", "4", "4"]);
});

test("matrix products count all dense arithmetic possibilities and reserve n cubed visits before any pruning", () => {
  assert.deepEqual(values(geometryPrimitiveCost("matMultiply", 2)), ["16", "16", "1", "4", "12"]);
  assert.deepEqual(values(geometryPrimitiveCost("product", 2, 3)), ["32", "32", "2", "8", "24"]);
  assert.deepEqual(values(geometryPrimitiveCost("product", 2, 1)), ["0", "0", "0", "0", "0"]);
  assert.deepEqual(values(geometryPrimitiveCost("commutator", 2)), ["45", "45", "4", "16", "32"]);
  assert.deepEqual(values(geometryPrimitiveCost("trace", 2)), ["2", "2", "0", "0", "2"]);
  assert.deepEqual(values(geometryPrimitiveCost("traceProduct", 2)), ["8", "8", "0", "0", "4"]);
});

test("inverse includes both scratch matrices, four n-cubed charged slots and every possible elimination row", () => {
  assert.deepEqual(values(geometryPrimitiveCost("inverse", 1)), ["3", "3", "2", "2", "6"]);
  assert.deepEqual(values(geometryPrimitiveCost("inverse", 2)), ["34", "34", "2", "8", "40"]);
  assert.deepEqual(values(geometryPrimitiveCost("inverse", 14)), ["15694", "15694", "2", "392", "11368"]);
  assert.equal(geometryPrimitiveCost("inverse", 2).coefficientHeightsProved, false);
});

test("complete two-dimensional connectionVariation stages match independently hand-enumerated charges", () => {
  const result = derive(2);
  assert.equal(result.schemaVersion, SCHEMA);
  assert.deepEqual(result.stages.map(row => [row.name, ...values(row)]), [
    ["constants", "2", "2", "0", "0", "0"],
    ["validateBothMetricJets", "0", "0", "0", "0", "8"],
    ["inverseMetric", "34", "34", "2", "8", "40"],
    ["inverseD", "72", "72", "6", "24", "56"],
    ["Q", "104", "104", "12", "48", "80"],
    ["DQ", "368", "368", "36", "144", "272"],
    ["S", "24", "24", "2", "8", "8"],
    ["C", "40", "40", "4", "16", "32"],
    ["DS", "48", "48", "4", "16", "16"],
    ["DC", "176", "176", "16", "64", "128"],
    ["curvature", "456", "456", "44", "176", "304"]
  ]);
  assert.deepEqual(values(result.perCall), ["1324", "1324", "126", "504", "944"]);
  assert.deepEqual(values(result.aggregate), ["1324", "1326", "126", "504", "944"]);
  assert.equal(result.validationScalarComparisonsPerCall, "22");
  assert.ok(Object.isFrozen(result.stages[0])); assert.ok(Object.isFrozen(result.omissions));
});

test("all dimensions satisfy closed polynomial totals, rather than only the contraction subtotal", () => {
  for (let n = 1; n <= 14; n++) {
    const r = derive(n, 1, false), N = BigInt(n);
    assert.equal(r.perCall.scalarOperations, String(20n * N ** 5n + 35n * N ** 4n + 16n * N ** 3n - 2n * N ** 2n + N + 2n));
    assert.equal(r.perCall.rationalObjects, r.perCall.scalarOperations);
    assert.equal(r.perCall.matrixObjects, String(25n * N ** 2n + 12n * N + 2n));
    assert.equal(r.perCall.matrixCells, String(25n * N ** 4n + 12n * N ** 3n + 2n * N ** 2n));
    assert.equal(r.perCall.slotVisits, String(10n * N ** 5n + 31n * N ** 4n + 15n * N ** 3n + 2n * N ** 2n));
    assert.equal(r.validationScalarComparisonsPerCall, String(N * (N - 1n) * (1n + N + 2n * N ** 2n)));
    for (const key of COUNTERS) assert.equal(r.perCall[key], String(r.stages.reduce((sum, row) => sum + BigInt(row[key]), 0n)));
  }
});

test("fixed n14 metadata count covers Q,DQ,curvature and validation in addition to accepted S/C and DS/DC bounds", () => {
  const r = derive(14), stages = Object.fromEntries(r.stages.map(stage => [stage.name, stage]));
  assert.equal(BigInt(stages.S.scalarOperations) + BigInt(stages.C.scalarOperations), 87808n);
  assert.equal(BigInt(stages.DS.scalarOperations) + BigInt(stages.DC.scalarOperations), 2381792n);
  assert.deepEqual(values(r.perCall), ["12144568", "12144568", "5070", "993720", "6610688"]);
  assert.ok(BigInt(r.perCall.scalarOperations) > 2469600n);
  assert.equal(r.scope.completeConnectionVariationChargedPath, true);
  assert.equal(r.scope.fullBaselineOrGermFactory, false);
  assert.equal(r.scope.geometryCoefficientsEvaluated, false);
});

test("700-call aggregate counts one persistent arithmetic context, with conservative no-GC logical storage", () => {
  const one = derive(14), many = derive(14, 700);
  assert.equal(many.aggregate.scalarOperations, "8501197600");
  assert.equal(many.aggregate.rationalObjects, "8501197602");
  assert.equal(many.aggregate.matrixObjects, "3549000");
  assert.equal(many.aggregate.matrixCells, "695604000");
  assert.equal(many.aggregate.slotVisits, "4627481600");
  assert.equal(many.logicalStorage.inputMatrixSlotsPerCall, "632");
  assert.equal(many.logicalStorage.inputMatrixCellsPerCall, "123872");
  assert.equal(many.logicalStorage.outputMatrixSlotsPerCall, "406");
  assert.equal(many.logicalStorage.outputMatrixCellsPerCall, "79576");
  assert.equal(many.logicalStorage.inputPlusAllAllocatedMatrixCells, String((123872n + 993720n) * 700n));
  assert.equal(many.logicalStorage.inputPlusAllAllocatedRationalSlots, String(123872n * 700n + 8501197602n));
  assert.equal(many.logicalStorage.assumesGarbageCollection, false);
  assert.equal(BigInt(many.contextCost.rationalObjects), 2n); assert.equal(BigInt(one.contextCost.rationalObjects), 2n);
});

test("all finite ledger caps reject excess before admitting aggregate counts", () => {
  const baseline = derive(2);
  for (const key of COUNTERS) {
    const cap = String(BigInt(baseline.aggregate[key]) - 1n);
    assert.throws(() => derive(2, 1, true, { [key]: cap }), new RegExp(key + " ceiling"));
  }
  const large = Object.fromEntries(COUNTERS.map(key => [key, "18446744073709551615"]));
  assert.throws(() => derive(14, Number.MAX_SAFE_INTEGER, true, { ...large, maxBits: 64 }), /pre-product ledger maxBits/);
  const noContext = derive(1, 1, false);
  assert.equal(noContext.aggregate.rationalObjects, "72");
  assert.equal(derive(1).aggregate.rationalObjects, "74");
});

test("closed metadata domains reject invalid dimensions, arities, counts and undeclared scope", () => {
  for (const n of [0, 15, 1.5, NaN, Infinity, "14"]) assert.throws(() => geometryPrimitiveCost("matrix", n), /dimension/);
  for (const a of [-1, 197, 1.5, undefined]) assert.throws(() => geometryPrimitiveCost("matAdd", 2, a), /arity/);
  assert.throws(() => geometryPrimitiveCost("product", 2, 0), /arity/);
  assert.throws(() => geometryPrimitiveCost("inverse", 2, 1), /arity/);
  assert.throws(() => geometryPrimitiveCost("unknown", 2), /primitive/);
  for (const count of [0, -1, 1.5, Number.MAX_SAFE_INTEGER + 1, Infinity]) assert.throws(() => derive(2, count), /call count/);
  assert.throws(() => derive(2, 1, 1), /context inclusion/);
  for (const override of [{ maxBits: 63 }, { maxBits: 1048577 }, { scalarOperations: "01" }, { matrixCells: "0" }, { slotVisits: 10 }])
    assert.throws(() => derive(2, 1, true, override), /maxBits|ceiling/);
  assert.throws(() => deriveConnectionVariationCosts({ dimension: 2, calls: 1, includeArithmeticContext: true, limits: limits(), source: {} }), /closed/);
});

test("partial factory scope and unsolved coefficient/inverse/memory obligations remain explicit", () => {
  const r = derive(14);
  for (const key of ["fullBaselineOrGermFactory", "coefficientBitHeightsProved", "inverseIntermediateHeightsProved", "outerArrayAndObjectStorageProved",
    "engineInternalMemoryProved", "wholeProcessMemoryProved", "sourceCodeClosureBound", "completeRuntimeResourceProof"]) assert.equal(r.scope[key], false);
  assert.equal(r.scope.inverseScratchMatrixCellsIncluded, true);
  assert.ok(r.omissions.some(s => s.includes("buildSourceMetric")));
  assert.ok(r.omissions.some(s => s.includes("verifyLinearizedMetricConnection")));
  assert.ok(r.omissions.some(s => s.includes("spin packing")));
  assert.ok(r.omissions.some(s => s.includes("inverse-pivot")));
});

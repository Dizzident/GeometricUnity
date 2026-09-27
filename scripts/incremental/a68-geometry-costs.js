"use strict";

// STATIC integer ledger only. This module neither imports GeometryAlgebra nor
// invokes a geometry factory, callback, matrix operation or coefficient test.
// Counts compose the currently reviewed factored connectionVariation source.
// They are not measured counters and never use observed zero/support patterns.
const SCHEMA = "phase627-static-connection-variation-costs-v1";
const COUNTERS = Object.freeze(["scalarOperations", "rationalObjects", "matrixObjects", "matrixCells", "slotVisits"]);
const need = (ok, message) => { if (!ok) throw new Error("A68 geometry costs: " + message); };
const shape = (x, keys) => need(x && typeof x === "object" && !Array.isArray(x) &&
  Reflect.ownKeys(x).length === keys.length && keys.every(key => Object.hasOwn(x, key) &&
    Object.hasOwn(Object.getOwnPropertyDescriptor(x, key), "value")), "closed plain descriptor");
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const dimension = n => need(Number.isSafeInteger(n) && n >= 1 && n <= 14, "dimension 1..14");
const zero = () => Object.fromEntries(COUNTERS.map(key => [key, 0n]));
const sum = (...costs) => Object.fromEntries(COUNTERS.map(key => [key, costs.reduce((total, cost) => total + cost[key], 0n)]));
const scale = (cost, repetitions) => Object.fromEntries(COUNTERS.map(key => [key, cost[key] * repetitions]));
const wire = cost => Object.fromEntries(COUNTERS.map(key => [key, cost[key].toString()]));

function primitive(operation, n, arity) {
  // n<=14 and arity<=196 bound every expression in this primitive model;
  // its largest integer fits in a safe JS integer, before conversion to a
  // BigInt ledger. No arbitrary-size coefficient integer is constructed.
  dimension(n); const N = BigInt(n), N2 = N * N, N3 = N2 * N;
  let scalar = 0n, matrices = 0n, slots = 0n;
  const matrix = (count = 1n) => { matrices = count; slots = count * N2; };
  const countArity = (minimum = 1) => need(Number.isSafeInteger(arity) && arity >= minimum && arity <= 196, "bounded variadic arity");
  if (!["sum", "matAdd", "product"].includes(operation)) need(arity === undefined, "arity only belongs to variadic primitives");
  switch (operation) {
    case "parse": case "number": case "add": case "multiply": case "negate": case "reciprocal": scalar = 1n; break;
    case "subtract": scalar = 2n; break;
    case "sum": countArity(0); scalar = BigInt(arity); break; // Includes zero+first.
    case "matrix": case "identity": case "transpose": matrix(); break;
    case "matScale": matrix(); scalar = N2; break;
    case "matAdd": countArity(); matrix(); scalar = BigInt(arity) * N2; break;
    case "matMultiply": matrix(); slots += N3; scalar = 2n * N3; break;
    case "product": countArity(); return scale(primitive("matMultiply", n), BigInt(arity - 1));
    case "commutator": return sum(scale(primitive("matMultiply", n), 2n), primitive("matScale", n), primitive("matAdd", n, 2), primitive("number", n));
    case "trace": slots = N; scalar = N; break;
    case "traceProduct": slots = N2; scalar = 2n * N2; break;
    case "matrixEqual": case "matrixText": slots = N2; break;
    case "inverse":
      // Two scratch matrices, then the implementation's full4*n^3 charge.
      // n reciprocals;2*n^2 pivot-row multiplies; at most n*(n-1) rows
      // each with2*n (multiply+subtract) updates, subtract costing2 ops.
      matrix(2n); slots += 4n * N3; scalar = N + 2n * N2 + 6n * N2 * (N - 1n); break;
    default: need(false, "closed primitive operation menu");
  }
  return { scalarOperations: scalar, rationalObjects: scalar, matrixObjects: matrices, matrixCells: matrices * N2, slotVisits: slots };
}

function geometryPrimitiveCost(operation, n, arity) {
  return freeze({ operation, dimension: n, ...(arity === undefined ? {} : { arity }), ...wire(primitive(operation, n, arity)),
    denseUpperArithmetic: true, coefficientHeightsProved: false, engineMemoryProved: false });
}

function deriveConnectionVariationCosts(options) {
  shape(options, ["dimension", "calls", "includeArithmeticContext", "limits"]);
  const { dimension: n, calls, includeArithmeticContext, limits } = options; dimension(n);
  need(Number.isSafeInteger(calls) && calls >= 1, "positive safe-integer call count");
  need(typeof includeArithmeticContext === "boolean", "explicit arithmetic-context inclusion");
  shape(limits, ["maxBits", ...COUNTERS]);
  need(Number.isSafeInteger(limits.maxBits) && limits.maxBits >= 64 && limits.maxBits <= 1048576, "finite ledger maxBits 64..1048576");
  const bits = value => BigInt(value.toString(2).length), maxBits = BigInt(limits.maxBits);
  const cap = text => {
    need(typeof text === "string" && text.length <= Math.ceil(limits.maxBits / 3) && /^[1-9][0-9]*$/.test(text), "bounded positive canonical ledger ceiling");
    const value = BigInt(text); need(bits(value) <= maxBits, "ledger ceiling maxBits"); return value;
  };
  const ceilings = Object.fromEntries(COUNTERS.map(key => [key, cap(limits[key])]));
  const checkedAdd = (a, b) => { need((bits(a) > bits(b) ? bits(a) : bits(b)) + 1n <= maxBits, "pre-addition ledger maxBits"); return a + b; };
  const checkedMultiply = (a, b) => {
    if (a === 0n || b === 0n) return 0n;
    need(bits(a) + bits(b) <= maxBits, "pre-product ledger maxBits"); return a * b;
  };
  const N = BigInt(n), N2 = N * N, N3 = N2 * N, N4 = N2 * N2, pairs = N * (N - 1n) / 2n;
  const p = (operation, arity) => primitive(operation, n, arity), stages = [];
  const stage = (name, cost) => { stages.push({ name, ...wire(cost) }); return cost; };
  const validation = scale(p("matrixEqual"), 2n * pairs);
  const costs = [
    stage("constants", scale(p("parse"), 2n)), // half and minus.
    stage("validateBothMetricJets", validation),
    stage("inverseMetric", p("inverse")),
    stage("inverseD", scale(sum(p("product", 3), p("matScale")), N)),
    stage("Q", scale(sum(p("transpose"), scale(p("matMultiply"), 2n), p("matAdd", 2), p("matScale"), p("matAdd", 2)), N)),
    stage("DQ", scale(sum(scale(p("transpose"), 2n), scale(p("matMultiply"), 4n), p("matAdd", 4), p("matScale"), p("matAdd", 2)), N2)),
    stage("S", scale(sum(p("matrix"), scale(sum(p("add"), p("subtract")), N2)), N)),
    stage("C", scale(sum(p("matMultiply"), p("matScale")), N)),
    stage("DS", scale(sum(p("matrix"), scale(sum(p("add"), p("subtract")), N2)), N2)),
    stage("DC", scale(sum(scale(p("matMultiply"), 2n), p("matAdd", 2), p("matScale")), N2)),
    stage("curvature", scale(sum(p("matScale"), scale(p("commutator"), 2n), p("matScale"), p("matAdd", 4)), N2))
  ];
  const perCall = sum(...costs), contextCost = zero();
  if (includeArithmeticContext) contextCost.rationalObjects = 2n; // a.zero,a.one once, not once per call.
  const aggregate = Object.fromEntries(COUNTERS.map(key => {
    const value = checkedAdd(checkedMultiply(perCall[key], BigInt(calls)), contextCost[key]);
    need(value <= ceilings[key], key + " ceiling"); return [key, value];
  }));
  // The direct per-entry symmetry comparisons do not increment the algebra's
  // scalarOperations counter. Report them separately rather than overlooking
  // them or changing the meaning of an existing admission counter.
  const validationScalarComparisons = 2n * pairs * (1n + N + 2n * N2);
  const inputMatrixSlots = 2n + 3n * N + 3n * N2; // metric+changed jets and Gamma,DGamma, with no assumed aliases.
  const outputMatrixSlots = N + 2n * N2; // C,DC,curvature, all entries.
  const inputMatrixCells = inputMatrixSlots * N2;
  const totalInputCells = checkedMultiply(inputMatrixCells, BigInt(calls));
  // Conservative cumulative allocation is also an upper bound on logical
  // simultaneous retention, WITHOUT relying on GC reclaiming temporaries.
  // Only referenced input matrices are included, not other caller-owned data.
  const logicalMatrixCells = checkedAdd(totalInputCells, aggregate.matrixCells);
  const logicalRationalSlots = checkedAdd(totalInputCells, aggregate.rationalObjects);
  return freeze({ schemaVersion: SCHEMA, dimension: n, calls, includeArithmeticContext, stages,
    perCall: wire(perCall), contextCost: wire(contextCost), aggregate: wire(aggregate),
    formulas: { scalarOperations: "20*n^5+35*n^4+16*n^3-2*n^2+n+2", matrixObjects: "25*n^2+12*n+2",
      matrixCells: "25*n^4+12*n^3+2*n^2", slotVisits: "10*n^5+31*n^4+15*n^3+2*n^2",
      SplusC: "2*n^4+4*n^3", DSplusDC: "4*n^5+6*n^4" },
    validationScalarComparisonsPerCall: validationScalarComparisons.toString(),
    validationScalarComparisonsAggregate: checkedMultiply(validationScalarComparisons, BigInt(calls)).toString(),
    logicalStorage: { inputMatrixSlotsPerCall: inputMatrixSlots.toString(), inputMatrixCellsPerCall: inputMatrixCells.toString(),
      outputMatrixSlotsPerCall: outputMatrixSlots.toString(), outputMatrixCellsPerCall: (outputMatrixSlots * N2).toString(),
      inputPlusAllAllocatedMatrixCells: logicalMatrixCells.toString(), inputPlusAllAllocatedRationalSlots: logicalRationalSlots.toString(),
      correspondingBigIntegerComponentSlots: checkedMultiply(2n, logicalRationalSlots).toString(),
      assumesGarbageCollection: false },
    scope: { completeConnectionVariationChargedPath: true, fullBaselineOrGermFactory: false,
      geometryCoefficientsEvaluated: false, observedSparsityUsed: false, allOutputCoordinatesRetained: true,
      denseArithmeticUpperBounds: true, inputMatrixAliasingAssumed: false, coefficientBitHeightsProved: false,
      inverseIntermediateHeightsProved: false, inverseScratchMatrixCellsIncluded: true,
      outerArrayAndObjectStorageProved: false, engineInternalMemoryProved: false, wholeProcessMemoryProved: false,
      sourceCodeClosureBound: false, completeRuntimeResourceProof: false },
    omissions: ["buildSourceMetric and buildSourceConnection", "buildSourceGermMetric", "verifyLinearizedMetricConnection outside connectionVariation",
      "buildSourceFrameGeometry and buildSourceGermFrame", "lowered curvature transforms and spin packing/validation",
      "buildSourceGermGeometry's final inverse and inverseVariation", "geometry binding/export/string serialization",
      "caller-owned matrices outside the referenced metric/changed/Gamma/DGamma inputs", "outer lists/tables/row arrays, WeakMap metadata and runtime headers",
      "unmetered index/shape/ownership checks and engine BigInt/GCD internals", "coefficient height/denominator and inverse-pivot magnitude proof",
      "production ceilings and externally enforced process containment"] });
}

module.exports = { geometryPrimitiveCost, deriveConnectionVariationCosts, SCHEMA, COUNTERS };

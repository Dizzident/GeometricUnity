"use strict";

// Prospective ANALYTIC envelopes, never measured coefficient statistics.
// Both the scalar schedule and tensor majorants are independently regenerated
// from the supplied semantic recipes and source-proved leaf bounds. No
// observed schedule/report is accepted. This is NOT a whole-process bound.
const { compileScalarSchedule } = require("./a68-scalar-schedule");
const { deriveMixedResourceMajorants } = require("./a68-resource-majorants");
const need = (ok, message) => { if (!ok) throw new Error("A68 scalar majorants: " + message); };
const shape = (x, keys) => need(x && typeof x === "object" && !Array.isArray(x) &&
  Reflect.ownKeys(x).length === keys.length && keys.every(k => Object.hasOwn(x, k) &&
    !Object.getOwnPropertyDescriptor(x, k).get && !Object.getOwnPropertyDescriptor(x, k).set), "closed plain descriptor");
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const gcd = (a, b) => { while (b) [a, b] = [b, a % b]; return a; };
const max = (...values) => values.reduce((a, b) => a > b ? a : b, 0n);
const bits = n => BigInt((n < 0n ? -n : n).toString(2).length);
const SCHEMA = "phase627-analytic-scalar-resource-majorants-v1";

function deriveScalarResourceMajorants(options) {
  shape(options, ["tensorPlan", "namedRoots", "geometry", "leafBounds", "validateLeafBound", "scheduleLimits", "limits"]);
  const { tensorPlan, namedRoots, geometry, leafBounds, validateLeafBound, scheduleLimits, limits } = options;
  const ceilings = ["tensorRecordPeak", "tensorDictionaryPeak", "scalarValuePeak", "combinedRationalSlotPeak", "scalarArithmeticIntegerBits"];
  shape(limits, ["maxBits", ...ceilings]);
  need(Number.isSafeInteger(limits.maxBits) && limits.maxBits >= 64 && limits.maxBits <= 1048576, "finite analytic bit ceiling");
  const maxBits = BigInt(limits.maxBits), check = n => { need(bits(n) <= maxBits, "analytic maxBits exceeded"); return n; };
  const natural = (text, positive = false) => {
    need(typeof text === "string" && text.length <= Math.ceil(limits.maxBits / 3) && /^(0|[1-9][0-9]*)$/.test(text), "bounded canonical natural string");
    const n = check(BigInt(text)); need(!positive || n > 0n, "positive admission ceiling/denominator"); return n;
  };
  const caps = Object.fromEntries(ceilings.map(key => [key, natural(limits[key], true)]));
  const add = (a, b) => { need(max(bits(a), bits(b)) + 1n <= maxBits, "pre-addition analytic maxBits ceiling"); return check(a + b); };
  const multiply = (a, b) => {
    if (a === 0n || b === 0n) return 0n;
    if (a === 1n) return check(b); if (b === 1n) return check(a);
    need(bits(a) + bits(b) <= maxBits, "pre-product analytic maxBits ceiling"); return check(a * b);
  };
  const lcm = (a, b) => multiply(a / gcd(a, b), b);
  const rat = (n, d) => { const g = gcd(n, d); return { n: check(n / g), d: check(d / g) }; };
  const rationalMagnitude = text => {
    need(typeof text === "string" && text.length <= Math.ceil(limits.maxBits / 3) && /^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(text), "bounded canonical rational string");
    const [ns, ds] = text.split("/"), signed = check(BigInt(ns)), n = signed < 0n ? -signed : signed, d = ds === undefined ? 1n : check(BigInt(ds));
    need(text !== "-0" && gcd(n, d) === 1n && (ds === undefined || d !== 1n), "reduced rational required"); return { n, d };
  };
  const plus = (a, b) => { const d = lcm(a.d, b.d); return rat(add(multiply(a.n, d / a.d), multiply(b.n, d / b.d)), d); };
  const times = (a, b) => { const g = gcd(a.n, b.d), h = gcd(b.n, a.d); return rat(multiply(a.n / g, b.n / h), multiply(a.d / h, b.d / g)); };
  const text = q => q.n.toString() + (q.d === 1n ? "" : "/" + q.d.toString());
  const bound = (M, D) => {
    if (M.n === 0n) return { M: { n: 0n, d: 1n }, D: 1n, N: 0n, printed: 1n, integerBits: 1n };
    const numerator = multiply(M.n, D), quotient = numerator / M.d;
    const N = numerator % M.d === 0n ? quotient : add(quotient, 1n);
    return { M, D, N, printed: BigInt(N.toString().length) + (D === 1n ? 1n : BigInt(D.toString().length) + 2n), integerBits: max(bits(N), bits(D)) };
  };

  // Structural limits are enforced by the scheduler before this module
  // allocates its O(tensorNodes+scalarNodes+geometryEntries) metadata arrays.
  const schedule = compileScalarSchedule(tensorPlan, namedRoots, geometry, scheduleLimits);
  const tensors = deriveMixedResourceMajorants(tensorPlan, leafBounds, validateLeafBound, { maxBits: limits.maxBits });
  const tb = tensors.nodes.map(row => ({ ...bound(rationalMagnitude(row.norm), natural(row.denominator, true)),
    S: natural(row.support), scratchRecords: natural(row.primitiveScratchRecords), scratchTensors: natural(row.primitiveScratchTensors),
    primitiveCharacters: natural(row.primitiveRationalCharacters) }));
  const matrices = new Map(schedule.geometry.map(matrix => [matrix.id, { length: BigInt(matrix.entries.length),
    entries: new Map(matrix.entries.map(entry => [entry.row * 14 + entry.column, entry.value])) }]));
  const scalarBounds = [], rows = [];
  let scalarArithmeticIntegerBits = 0n, scalarRationalCharacters = 1n, coefficientVisits = 0n, matchedPairProducts = 0n;
  let coefficientReads = 0n, readCharacters = 0n, tensorResolutions = 0n, arithmeticOperations = 0n;
  for (const node of schedule.nodes) {
    const inputs = node.inputs.map(id => scalarBounds[id]), ts = node.tensorInputs.map(id => tb[id]);
    let result, scratchSlots, visits = 0n, products = 0n, L = 1n, arithmeticBits;
    let reads = 0n, characters = 0n, resolutions = 0n, operations = 0n, parameterCharacters = 0n;
    switch (node.op) {
      case "constant": case "matrixEntry": {
        const matrix = node.op === "matrixEntry" ? matrices.get(node.parameters.matrix) : null;
        const value = node.op === "constant" ? node.parameters.value : matrix.entries.get(node.parameters.row * 14 + node.parameters.column) ?? "0";
        const q = rationalMagnitude(value);
        reads = 1n; characters = BigInt(value.length); operations = 1n;
        visits = matrix ? add(BigInt(schedule.geometry.length), matrix.length) : 0n;
        parameterCharacters = BigInt(value.length);
        result = bound(q, q.d); scratchSlots = 1n; L = result.integerBits; arithmeticBits = L; break;
      }
      case "add":
        result = bound(plus(inputs[0].M, inputs[1].M), lcm(inputs[0].D, inputs[1].D)); scratchSlots = 4n; operations = 1n; break;
      case "multiply":
        result = bound(times(inputs[0].M, inputs[1].M), multiply(inputs[0].D, inputs[1].D)); scratchSlots = 4n; operations = 1n; break;
      case "pair":
        result = bound(times(ts[0].M, ts[1].M), multiply(ts[0].D, ts[1].D));
        products = ts[0].S < ts[1].S ? ts[0].S : ts[1].S;
        // Standalone scalar replay validates BOTH full wire tensors and then
        // merge-scans. Alias operands are resolved/validated twice as well.
        resolutions = 2n; visits = multiply(2n, add(ts[0].S, ts[1].S));
        reads = add(visits, multiply(4n, products));
        characters = add(multiply(2n, add(multiply(ts[0].S, ts[0].printed), multiply(ts[1].S, ts[1].printed))),
          multiply(multiply(2n, products), add(ts[0].printed, ts[1].printed)));
        // Four multiply, one subtract(two ops), one add, two possible sign
        // negations and two accumulator additions, plus all scalar parses.
        operations = add(reads, multiply(11n, products));
        // Explicit real AND imaginary accumulators; four parsed components,
        // rr/ii and expression temporaries are covered by this conservative
        // 32-rational-slot contract. No imaginary cancellation is assumed.
        // GCD/BigInt engine-internal storage is NOT represented by slots.
        scratchSlots = 32n; break;
      case "top": {
        const weight = rationalMagnitude(node.parameters.weight);
        parameterCharacters = BigInt(node.parameters.weight.length);
        result = bound(times(ts[0].M, weight), multiply(ts[0].D, weight.d));
        L = max(bits(weight.n), bits(weight.d)); scratchSlots = 8n; resolutions = 1n;
        visits = multiply(2n, ts[0].S); reads = add(add(visits, 2n), 1n);
        characters = add(multiply(add(visits, 2n), ts[0].printed), BigInt(node.parameters.weight.length));
        operations = add(reads, 2n); break;
      }
      default: need(false, "closed scalar operation menu");
    }
    L = max(L, result.integerBits, ...inputs.map(x => x.integerBits), ...ts.map(x => x.integerBits));
    // Reduced components/partial accumulators are bounded by M,D even before
    // cancellation. 16L+16 conservatively covers the two complex products,
    // sign and accumulator arithmetic; scalar operations need at most2L+1.
    arithmeticBits ??= node.op === "pair" ? add(multiply(16n, L), 16n) : add(multiply(2n, L), 1n);
    need(arithmeticBits <= caps.scalarArithmeticIntegerBits, "scalarArithmeticIntegerBits ceiling");
    scalarArithmeticIntegerBits = max(scalarArithmeticIntegerBits, arithmeticBits);
    const primitiveCharacters = max(result.printed, parameterCharacters, ...inputs.map(x => x.printed), ...ts.map(x => x.printed));
    scalarRationalCharacters = max(scalarRationalCharacters, primitiveCharacters);
    coefficientVisits = add(coefficientVisits, visits); matchedPairProducts = add(matchedPairProducts, products);
    coefficientReads = add(coefficientReads, reads); readCharacters = add(readCharacters, characters);
    tensorResolutions = add(tensorResolutions, resolutions); arithmeticOperations = add(arithmeticOperations, operations);
    scalarBounds.push({ ...result, scratchSlots });
    rows.push({ id: node.id, op: node.op, denominator: result.D.toString(), norm: text(result.M), numeratorBound: result.N.toString(),
      rationalCharacters: result.printed.toString(), reducedIntegerBits: result.integerBits.toString(),
      primitiveRationalCharacters: primitiveCharacters.toString(), parameterRationalCharacters: parameterCharacters.toString(),
      arithmeticIntegerBits: arithmeticBits.toString(), scratchRationalSlots: scratchSlots.toString(),
      recordVisits: visits.toString(), coefficientReads: reads.toString(), readCharacters: characters.toString(),
      tensorResolutions: resolutions.toString(), arithmeticOperations: operations.toString(), matchedPairProducts: products.toString() });
  }

  const liveTensors = new Set(), liveScalars = new Set(); let liveRecords = 0n, scalarStep = 0;
  const peaks = Object.fromEntries(ceilings.filter(k => k !== "scalarArithmeticIntegerBits").map(k => [k, 0n]));
  const witnesses = {};
  function measure(records, dictionaries, scalarValues, extraScalarSlots, event) {
    const values = { tensorRecordPeak: records, tensorDictionaryPeak: dictionaries, scalarValuePeak: scalarValues,
      // GeometryAlgebra owns a.zero and a.one for the entire replay lifetime,
      // including an empty schedule. They are additional to value-map slots.
      combinedRationalSlotPeak: add(add(add(multiply(2n, records), scalarValues), extraScalarSlots), 2n) };
    for (const [key, value] of Object.entries(values)) {
      need(value <= caps[key], key + " ceiling");
      if (value > peaks[key]) { peaks[key] = value; witnesses[key] = { ...event }; }
    }
  }
  function scalarsReadyAfter(tensorId) {
    while (scalarStep < schedule.executionOrder.length && schedule.nodes[schedule.executionOrder[scalarStep]].readyAfter === tensorId) {
      const id = schedule.executionOrder[scalarStep], node = schedule.nodes[id], b = scalarBounds[id];
      need(node.inputs.every(input => liveScalars.has(input)) && node.tensorInputs.every(input => liveTensors.has(input)), "scheduled consumers retain all inputs");
      measure(liveRecords, BigInt(liveTensors.size), BigInt(liveScalars.size + 1), b.scratchSlots - 1n, { type: "scalar", id, tensorAfter: tensorId });
      liveScalars.add(id);
      // Root comparisons occur here, BEFORE releasing the freshly produced
      // value. Their external callback may not retain unbudgeted values.
      for (const expired of schedule.scalarReleaseByExecution[scalarStep]) { need(liveScalars.delete(expired), "scalar release census"); }
      scalarStep++;
    }
  }
  measure(0n, 0n, 0n, 0n, { type: "arithmeticContext" });
  scalarsReadyAfter(-1);
  for (let id = 0; id < tensorPlan.nodes.length; id++) {
    const b = tb[id]; need(tensorPlan.nodes[id].inputs.every(input => liveTensors.has(input)), "tensor consumer liveness");
    measure(add(liveRecords, b.scratchRecords), add(BigInt(liveTensors.size), b.scratchTensors), BigInt(liveScalars.size), 0n, { type: "tensor", id });
    liveTensors.add(id); liveRecords = add(liveRecords, b.S);
    scalarsReadyAfter(id);
    // ALL expired tensors, including ones held solely for scalar consumers,
    // are released. Restricting this loop to current tensor inputs is wrong.
    for (const expired of schedule.releaseByStep[id]) {
      need(liveTensors.delete(expired), "tensor release census"); liveRecords -= tb[expired].S;
    }
  }
  need(scalarStep === schedule.nodes.length && liveTensors.size === 0 && liveScalars.size === 0 && liveRecords === 0n, "complete combined release census");
  return freeze({ schemaVersion: SCHEMA, nodes: rows, tensorMajorants: tensors,
    tensorLastUse: schedule.tensorLastUse, scalarExecutionOrder: schedule.executionOrder, scalarLastUse: schedule.scalarLastUse,
    peaks: Object.fromEntries(Object.entries(peaks).map(([key, value]) => [key, value.toString()])), peakWitnesses: witnesses,
    scalarArithmeticIntegerBits: scalarArithmeticIntegerBits.toString(), scalarReducedRationalCharacterPeak: scalarRationalCharacters.toString(),
    scalarReplayWork: { recordVisits: coefficientVisits.toString(), coefficientReads: coefficientReads.toString(), readCharacters: readCharacters.toString(),
      tensorResolutions: tensorResolutions.toString(), arithmeticOperations: arithmeticOperations.toString(),
      rationalObjects: add(arithmeticOperations, 2n).toString(), matchedPairProducts: matchedPairProducts.toString() },
    scope: { scientificCoefficientsEvaluated: false, observedStatisticsUsed: false, tensorBoundsIndependentlyRederived: true,
      scalarBoundsAnalytic: true, fullImaginaryPairAccumulatorIncluded: true, scalarConsumerTensorRetentionIncluded: true,
      explicitScalarLocalScratchIncluded: true, tensorPrimitiveArithmeticScratchIncluded: false,
      dictionaryLookupWorkProved: false, rootComparisonCallbackMemoryProved: false, metadataMemoryProved: false,
      geometryMatrixStorageIncluded: false, wireRowParserStorageIncluded: false, bigintAndGcdInternalMemoryProved: false,
      wholeProcessMemoryProved: false, completeRuntimeResourceProof: false } });
}

module.exports = { deriveScalarResourceMajorants, SCHEMA };

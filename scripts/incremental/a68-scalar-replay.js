"use strict";

// Standalone scalar replay, NOT activation/integration of the tensor verifier.
// resolveTensor(id) must expose an already independently reconstructed and
// authenticated tensor as a frozen canonical array of frozen wire rows. This
// module checks the entire returned domain, but cannot authenticate its source.
// Outer wire conversion/storage, tensor numerical replay, and aggregate RSS
// remain separate obligations. No production resource limits are supplied.
// Read/visit counters are conservative precharged envelopes, not exact visits.
// rationalCharacters bounds input and emitted-root strings; intermediate
// numerator/denominator height is separately bounded by arithmetic maxBits.
const { compileScalarSchedule } = require("./a68-scalar-schedule");
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const need = (ok, why) => { if (!ok) throw new Error("A68 scalar replay: " + why); };
const integer = (x, lo, hi) => Number.isSafeInteger(x) && !Object.is(x, -0) && x >= lo && x <= hi;
const shape = (x, keys, ordered = false) => {
  need(x !== null && typeof x === "object" && !Array.isArray(x), "closed object shape");
  const actual = Reflect.ownKeys(x);
  need(actual.length === keys.length && keys.every((key, i) => Object.hasOwn(x, key) && (!ordered || actual[i] === key)), "closed object keys/order");
  need(keys.every(key => Object.hasOwn(Object.getOwnPropertyDescriptor(x, key), "value")), "data properties required");
};
const degree = mask => { let n = 0; while (mask) { n += mask & 1; mask >>>= 1; } return n; };

function createScalarReplay(options) {
  shape(options, ["tensorPlan", "namedRoots", "geometry", "scheduleLimits", "arithmeticLimits", "replayLimits", "resolveTensor", "compareRoot"]);
  const { tensorPlan, namedRoots, geometry, scheduleLimits, arithmeticLimits, resolveTensor, compareRoot } = options;
  need(typeof resolveTensor === "function" && typeof compareRoot === "function", "tensor resolver and immediate comparator required");
  const limitKeys = ["tensorRecords", "tensorResolutions", "recordVisits", "coefficientReads", "readCharacters",
    "outputCharacters", "rationalCharacters", "liveScalars"];
  shape(options.replayLimits, limitKeys);
  need(limitKeys.every(key => integer(options.replayLimits[key], 1, Number.MAX_SAFE_INTEGER)), "explicit positive finite replay limits");
  const limits = Object.freeze({ ...options.replayLimits });
  need(limits.rationalCharacters <= 16384, "bounded rational characters");
  const schedule = compileScalarSchedule(tensorPlan, namedRoots, geometry, scheduleLimits);
  need(schedule.maximumLiveScalarValues <= limits.liveScalars, "prospective live scalar ceiling");
  const a = new GeometryAlgebra(arithmeticLimits), maxBits = arithmeticLimits.maxBits;
  const tensorDegrees = Object.freeze(tensorPlan.nodes.map(node => node.degree));
  const values = new Map(), usage = { tensorResolutions: 0, recordVisits: 0, coefficientReads: 0, readCharacters: 0, outputCharacters: 0 };
  let failed = false, busy = false, complete = false, nextStep = -1, execution = 0, comparison = 0, maximumLiveScalars = 0;
  const charge = (key, count = 1) => {
    need(integer(count, 0, Number.MAX_SAFE_INTEGER) && count <= limits[key] - usage[key], "pre-operation " + key + " ceiling"); usage[key] += count;
  };
  const guard = operation => {
    try {
      need(!failed, "poisoned replay"); need(!busy, "reentrant replay lifecycle"); need(!complete, "completed replay lifecycle");
      busy = true; const result = operation(); need(!failed, "callback poisoned replay"); return result;
    } catch (error) { failed = true; values.clear(); throw error; }
    finally { busy = false; }
  };
  const read = (text, reserved = false) => {
    if (!reserved) charge("coefficientReads");
    need(typeof text === "string" && text.length >= 1 && text.length <= limits.rationalCharacters, "bounded coefficient string");
    charge("readCharacters", text.length); return a.parse(text);
  };
  const resolve = id => {
    charge("tensorResolutions"); const rows = resolveTensor(id);
    need(!failed, "resolver poisoned replay");
    need(Array.isArray(rows) && rows.length <= limits.tensorRecords, "frozen bounded tensor row array");
    charge("recordVisits", rows.length); charge("coefficientReads", 2 * rows.length);
    need(Object.isFrozen(rows), "frozen bounded tensor row array");
    need(Reflect.ownKeys(rows).length === rows.length + 1 && Object.keys(rows).length === rows.length, "dense tensor row array without extra keys");
    need(tensorDegrees[id] >= 0 || rows.length === 0, "untyped zero must have no records");
    let previous = -1;
    for (let i = 0; i < rows.length; i++) {
      const descriptor = Object.getOwnPropertyDescriptor(rows, String(i));
      need(descriptor && Object.hasOwn(descriptor, "value"), "tensor array indices must be own data properties");
      const row = rows[i];
      shape(row, ["form", "blade", "k0", "k1", "real", "imaginary"], true);
      need(Object.isFrozen(row) && integer(row.form, 0, 16383) && integer(row.blade, 0, 16383) && row.k0 === 0 && row.k1 === 0 && !Object.is(row.k0, -0) && !Object.is(row.k1, -0), "frozen local zero-frequency row");
      const key = row.form * 16384 + row.blade;
      need(key > previous && degree(row.form) === tensorDegrees[id], "canonical tensor ordering and degree"); previous = key;
      const real = read(row.real, true), imaginary = read(row.imaginary, true);
      need(!a.isZero(real) || !a.isZero(imaginary), "explicit zero tensor record");
    }
    return rows;
  };
  const pairing = (left, right) => {
    // Exactly one resolution of each operand per operation, including aliases.
    const x = resolve(left), y = resolve(right);
    charge("recordVisits", x.length + y.length);
    charge("coefficientReads", 4 * Math.min(x.length, y.length));
    let i = 0, j = 0, real = a.zero, imaginary = a.zero;
    while (i < x.length && j < y.length) {
      const p = x[i], q = y[j], pk = p.form * 16384 + p.blade, qk = q.form * 16384 + q.blade;
      if (pk < qk) { i++; continue; } if (qk < pk) { j++; continue; }
      const ar = read(p.real, true), ai = read(p.imaginary, true), br = read(q.real, true), bi = read(q.imaginary, true);
      let rr = a.subtract(a.multiply(ar, br), a.multiply(ai, bi));
      let ii = a.add(a.multiply(ar, bi), a.multiply(ai, br));
      const grade = degree(p.blade), parity = grade * (grade - 1) / 2 + degree(p.blade & 0x3f80) + degree(p.form & 0x3f80);
      // -CliffordSquare(blade)*exteriorMetric(form), with NO conjugation.
      if (parity % 2 === 0) { rr = a.negate(rr); ii = a.negate(ii); }
      real = a.add(real, rr); imaginary = a.add(imaginary, ii); i++; j++;
    }
    need(a.isZero(imaginary), "complete Pair imaginary trace must cancel"); return real;
  };
  const top = node => {
    const rows = resolve(node.tensorInputs[0]); charge("recordVisits", rows.length); charge("coefficientReads", 2);
    let real = a.zero, imaginary = a.zero;
    for (const row of rows) if (row.form === 16383 && row.blade === 0) {
      real = read(row.real, true); imaginary = read(row.imaginary, true); break;
    }
    need(a.isZero(imaginary), "selected TOP coefficient must be real");
    return a.multiply(a.negate(real), read(node.parameters.weight));
  };
  const evaluate = node => {
    const p = node.parameters;
    switch (node.op) {
      case "constant": return read(p.value);
      case "matrixEntry": {
        charge("recordVisits", schedule.geometry.length);
        const matrix = schedule.geometry.find(item => item.id === p.matrix);
        charge("recordVisits", matrix.entries.length);
        return read(matrix.entries.find(entry => entry.row === p.row && entry.column === p.column)?.value ?? "0");
      }
      case "pair": return pairing(...node.tensorInputs);
      case "top": return top(node);
      case "add": case "multiply": {
        need(node.inputs.every(id => values.has(id)), "released or missing scalar input");
        return node.op === "add" ? a.add(values.get(node.inputs[0]), values.get(node.inputs[1])) : a.multiply(values.get(node.inputs[0]), values.get(node.inputs[1]));
      }
      default: throw new Error("A68 scalar replay: internally compiled primitive required");
    }
  };
  const output = value => {
    // Each numerator and denominator has <=maxBits bits; their decimal
    // lengths are individually <=maxBits. Reserve before string allocation.
    charge("outputCharacters", 2 * maxBits + 2);
    const text = a.text(value); need(text.length <= limits.rationalCharacters, "scalar result serialized height ceiling"); return text;
  };
  const snapshot = () => Object.freeze({ nextStep, scalarOperationsExecuted: execution, rootsCompared: comparison,
    liveScalars: values.size, maximumLiveScalars, failed, complete, usage: Object.freeze({ ...usage }),
    arithmetic: a.snapshot(), tensorSourceAuthenticationProved: false, outerTensorConversionStorageProved: false,
    totalProcessMemoryProved: false });
  return Object.freeze({ schedule,
    advance(step) { return guard(() => {
      need(integer(step, -1, tensorDegrees.length - 1) && step === nextStep, "strict ready-step sequence");
      while (execution < schedule.executionOrder.length) {
        const node = schedule.nodes[schedule.executionOrder[execution]];
        if (node.readyAfter !== step) break;
        need(values.size < limits.liveScalars, "live scalar ceiling before evaluation");
        const value = evaluate(node); values.set(node.id, value); maximumLiveScalars = Math.max(maximumLiveScalars, values.size);
        while (comparison < schedule.rootComparisons.length && schedule.rootComparisons[comparison].afterScalarExecution === execution) {
          const descriptor = schedule.rootComparisons[comparison], text = output(value);
          need(compareRoot(descriptor.name, text, descriptor) === true, "root comparison rejected " + descriptor.name);
          need(!failed, "comparator poisoned replay"); comparison++;
        }
        for (const id of schedule.scalarReleaseByExecution[execution]) need(values.delete(id), "complete scalar release");
        execution++;
      }
      nextStep++; return step < 0 ? Object.freeze([]) : schedule.releaseByStep[step];
    }); },
    finish() { return guard(() => {
      need(nextStep === tensorDegrees.length && execution === schedule.nodes.length && comparison === schedule.roots.length && values.size === 0, "complete tensor-step/scalar/root/release census");
      complete = true; return snapshot();
    }); },
    snapshot
  });
}

module.exports = { createScalarReplay };

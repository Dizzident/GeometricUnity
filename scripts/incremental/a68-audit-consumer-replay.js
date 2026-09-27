"use strict";

// Exact numerical predicates/norms for an independently compiled audit plan.
// NOT source authentication, a phase entry point or full tensor/scalar replay.
// Resolvers must expose independently reconstructed authenticated values; two
// observed geometry derivations agreeing with one another is not sufficient.
// Dense geometry resolver contract: frozen nested arrays,14 entries per axis,
// canonical rational strings, rank2 for matrices/rank4 for curvature fields.
const { compileAuditConsumerSchedule } = require("./a68-audit-consumer-schedule");
const { GeometryAlgebra } = require("./a68-geometry-algebra");
const need = (ok, why) => { if (!ok) throw new Error("A68 consumer replay: " + why); };
const integer = (x, lo, hi = Number.MAX_SAFE_INTEGER) => Number.isSafeInteger(x) && !Object.is(x, -0) && x >= lo && x <= hi;
const degree = mask => { let n = 0; while (mask) { n += mask & 1; mask >>>= 1; } return n; };
function shape(value, keys, ordered = false) {
  need(value && typeof value === "object" && !Array.isArray(value), "own-data object"); const actual = Reflect.ownKeys(value);
  need(actual.length === keys.length && keys.every((key, i) => (!ordered || actual[i] === key) && Object.hasOwn(Object.getOwnPropertyDescriptor(value, key) ?? {}, "value")), "closed own-data fields");
}
const field = (value, key) => Object.getOwnPropertyDescriptor(value, key).value;

function createAuditConsumerReplay(options) {
  const names = ["tensorPlan", "namedRoots", "geometry", "checks", "domainChecks", "error", "scheduleLimits", "consumerScheduleLimits",
    "arithmeticLimits", "replayLimits", "resolveTensor", "resolveGeometryField", "resolveSourceScalar", "compareConsumer"];
  shape(options, names);
  const { tensorPlan, namedRoots, geometry, checks, domainChecks, error, scheduleLimits, consumerScheduleLimits,
    arithmeticLimits, resolveTensor, resolveGeometryField, resolveSourceScalar, compareConsumer } = options;
  need([resolveTensor, resolveGeometryField, resolveSourceScalar, compareConsumer].every(fn => typeof fn === "function"), "explicit resolvers and comparator");
  const limitKeys = ["tensorRecords", "tensorResolutions", "parsedTensorRecords", "recordVisits", "geometryResolutions", "geometryCoordinates",
    "arraySlots", "coefficientReads", "readCharacters", "outputCharacters", "rationalCharacters", "predicateNodes", "rootValues", "errorValues",
    "resultValues", "consumerComparisons", "sourceResolutions"];
  shape(options.replayLimits, limitKeys); need(limitKeys.every(k => integer(options.replayLimits[k], 1)) && options.replayLimits.rationalCharacters <= 16384, "explicit finite consumer limits");
  const limits = Object.freeze({ ...options.replayLimits });
  const schedule = compileAuditConsumerSchedule({ tensorPlan, namedRoots, geometry, checks, domainChecks, error, scheduleLimits, limits: consumerScheduleLimits });
  need(schedule.maximumRetainedRootValues <= limits.rootValues, "prospective root cache ceiling");
  const a = new GeometryAlgebra(arithmeticLimits), maxBits = arithmeticLimits.maxBits;
  const degrees = Object.freeze(tensorPlan.nodes.map(node => node.degree));
  const counters = limitKeys.filter(k => !["tensorRecords", "rationalCharacters", "rootValues", "errorValues", "resultValues"].includes(k));
  const used = Object.fromEntries(counters.map(k => [k, 0]));
  const roots = new Map(), errors = new Map(), matrices = new Map(), seenRoots = new Set();
  const rootMenu = new Map(schedule.scalarSchedule.rootComparisons.map(row => [row.name, row.readyAfter]));
  const neededRoots = new Set(schedule.captureRootsByStep.flat());
  const rootCensus = Array.from({ length: degrees.length + 1 }, () => []);
  rootMenu.forEach((step, name) => rootCensus[step + 1].push(name));
  const errorIds = new Map(), lastErrorUse = new Map();
  schedule.consumers.forEach(c => { if (c.kind === "error") errorIds.set("error/" + c.name, c.id); });
  schedule.executionOrder.forEach((id, position) => schedule.consumers[id].consumerInputs.forEach(input => lastErrorUse.set(input, position)));
  let failed = false, busy = false, complete = false, nextStep = -1, executed = 0, errorValues = 0, maximumErrorValues = 0, maximumRootValues = 0;
  const charge = (key, count = 1) => { need(integer(count, 0) && count <= limits[key] - used[key], "pre-operation " + key + " ceiling"); used[key] += count; };
  const guard = fn => {
    try {
      need(!failed, "poisoned consumer replay"); need(!busy, "reentrant consumer lifecycle"); need(!complete, "completed consumer lifecycle");
      busy = true; const result = fn(); need(!failed, "callback poisoned consumer replay"); return result;
    } catch (error) { failed = true; roots.clear(); errors.clear(); matrices.clear(); tensorCache.clear(); fieldCache.clear(); errorValues = 0; throw error; }
    finally { busy = false; }
  };
  const external = (fn, ...args) => { const value = fn(...args); need(!failed, "callback poisoned consumer replay"); return value; };
  const read = text => {
    charge("coefficientReads"); need(typeof text === "string" && text.length > 0 && text.length <= limits.rationalCharacters, "bounded rational string");
    charge("readCharacters", text.length); return a.parse(text);
  };
  const output = q => { charge("outputCharacters", 2 * maxBits + 2); const text = a.text(q); need(text.length <= limits.rationalCharacters, "output rational height"); return text; };
  const abs = q => q[0] < 0n ? a.negate(q) : q;
  const greater = (x, y) => a.subtract(x, y)[0] > 0n;
  function frozenArray(value, expected = null) {
    need(Array.isArray(value), "frozen dense array");
    const count = Object.getOwnPropertyDescriptor(value, "length")?.value;
    need(integer(count, 0) && (expected === null || count === expected), "exact array shape");
    charge("arraySlots", count);
    need(Object.isFrozen(value), "frozen dense array");
    need(Reflect.ownKeys(value).length === count + 1, "dense array without extra keys");
    for (let i = 0; i < count; i++) need(Object.hasOwn(Object.getOwnPropertyDescriptor(value, String(i)) ?? {}, "value"), "own-data array indices");
    return count;
  }
  let tensorCache = new Map(), fieldCache = new Map();
  function tensor(ref) {
    if (ref.kind === "literalZero") return [];
    const id = ref.node; if (tensorCache.has(id)) return tensorCache.get(id);
    charge("tensorResolutions"); const rows = external(resolveTensor, id);
    need(Array.isArray(rows), "tensor array"); const n = Object.getOwnPropertyDescriptor(rows, "length")?.value;
    need(integer(n, 0, limits.tensorRecords), "per-tensor record ceiling");
    charge("parsedTensorRecords", n); charge("recordVisits", n); frozenArray(rows, n);
    need(degrees[id] >= 0 || n === 0, "untyped zero tensor must be empty");
    const result = []; let previous = -1;
    for (let i = 0; i < n; i++) {
      const row = field(rows, String(i)); shape(row, ["form", "blade", "k0", "k1", "real", "imaginary"], true);
      need(Object.isFrozen(row), "frozen tensor row");
      const form = field(row, "form"), blade = field(row, "blade"), k0 = field(row, "k0"), k1 = field(row, "k1");
      need(integer(form, 0, 16383) && integer(blade, 0, 16383) && integer(k0, 0, 0) && integer(k1, 0, 0), "complete local tensor domain");
      const key = form * 16384 + blade; need(key > previous && degree(form) === degrees[id], "complete ordered tensor degree"); previous = key;
      const real = read(field(row, "real")), imaginary = read(field(row, "imaginary"));
      need(!a.isZero(real) || !a.isZero(imaginary), "explicit zero omitted");
      result.push({ form, blade, key, real, imaginary });
    }
    tensorCache.set(id, result); return result;
  }
  function coefficient(selector) {
    const rows = tensor(selector.tensor), key = selector.form * 16384 + selector.blade;
    charge("recordVisits", rows.length); let result = { real: a.zero, imaginary: a.zero };
    for (const row of rows) if (row.key === key) result = row;
    return result;
  }
  function equalTensor(left, right) {
    const x = tensor(left), y = tensor(right); charge("recordVisits", x.length + y.length);
    let ok = x.length === y.length;
    for (let i = 0; i < Math.max(x.length, y.length); i++) {
      if (!x[i] || !y[i]) { ok = false; continue; }
      const real = a.equal(x[i].real, y[i].real), imaginary = a.equal(x[i].imaginary, y[i].imaginary);
      if (x[i].key !== y[i].key || !real || !imaginary) ok = false;
    }
    return ok;
  }
  function geometryField(path) {
    if (fieldCache.has(path)) return fieldCache.get(path);
    const rank = path.endsWith(".curvature") ? 4 : 2, count = 14 ** rank;
    charge("geometryResolutions"); charge("geometryCoordinates", count);
    const data = external(resolveGeometryField, path), values = [];
    const descend = (node, remaining) => {
      frozenArray(node, 14);
      for (let i = 0; i < 14; i++) { const value = field(node, String(i)); if (remaining === 1) values.push(read(value)); else descend(value, remaining - 1); }
    };
    descend(data, rank); need(values.length === count, "complete geometry coordinate census"); fieldCache.set(path, values); return values;
  }
  function matrix(expression) {
    charge("predicateNodes");
    if (expression.op === "matrixProduct") return a.matMultiply(matrix(expression.left), matrix(expression.right));
    if (!matrices.has(expression.name)) {
      charge("recordVisits", schedule.scalarSchedule.geometry.length);
      const descriptor = schedule.scalarSchedule.geometry.find(item => item.id === expression.name);
      need(descriptor, "declared matrix name"); charge("geometryCoordinates", 196); charge("recordVisits", descriptor.entries.length);
      const parsed = new Map(); descriptor.entries.forEach(row => parsed.set(row.row * 14 + row.column, read(row.value)));
      matrices.set(expression.name, a.matrix(14, (i, j) => parsed.get(i * 14 + j) ?? a.zero));
    }
    return matrices.get(expression.name);
  }
  function matrixNorm(expression, kind) {
    const m = matrix(expression); charge("geometryCoordinates", 196);
    if (kind === "entryL1") { let sum = a.zero; for (let i = 0; i < 14; i++) for (let j = 0; j < 14; j++) sum = a.add(sum, abs(a.get(m, i, j))); return sum; }
    let largest = a.zero;
    for (let i = 0; i < 14; i++) {
      let sum = a.zero; for (let j = 0; j < 14; j++) sum = a.add(sum, abs(kind === "maximumColumnL1" ? a.get(m, j, i) : a.get(m, i, j)));
      if (greater(sum, largest)) largest = sum;
    }
    return largest;
  }
  function norm(expression) {
    charge("predicateNodes");
    switch (expression.op) {
      case "constant": return read(expression.value);
      case "sourceBoundScalar": {
        charge("sourceResolutions"); const value = read(external(resolveSourceScalar, expression.binding));
        need(value[0] > 0n, "positive source-bound epsilon"); return value;
      }
      case "sum": case "product": {
        let result = expression.op === "sum" ? a.zero : a.one;
        for (const value of expression.values) result = expression.op === "sum" ? a.add(result, norm(value)) : a.multiply(result, norm(value));
        return result;
      }
      case "tensorL1": {
        const rows = tensor(expression.tensor); charge("recordVisits", rows.length); let sum = a.zero;
        for (const row of rows) sum = a.add(sum, a.add(abs(row.real), abs(row.imaginary))); return sum;
      }
      case "entryL1": return matrixNorm(expression.value, expression.op);
      case "maximumColumnL1": case "maximumRowL1": return matrixNorm(expression.matrix, expression.op);
      default: need(false, "compiled norm operation");
    }
  }
  const root = name => { need(roots.has(name), "missing or released scalar root"); return roots.get(name); };
  function predicate(p) {
    charge("predicateNodes");
    switch (p.kind) {
      case "tensorEqual": return equalTensor(p.left, p.right);
      case "tensorZero": return tensor(p.tensor).length === 0;
      case "tensorBladeGrades": {
        const rows = tensor(p.tensor); charge("recordVisits", rows.length * (1 + p.allowed.length)); let ok = true;
        for (const row of rows) if (!p.allowed.includes(degree(row.blade))) ok = false; return ok;
      }
      case "all": { let ok = true; for (const child of p.predicates) { const value = predicate(child); if (!value) ok = false; } return ok; }
      case "complexCoefficientScaledEqual": {
        const x = coefficient(p.left), y = coefficient(p.right), factor = read(p.factor);
        const real = a.equal(x.real, a.multiply(y.real, factor)), imaginary = a.equal(x.imaginary, a.multiply(y.imaginary, factor)); return real && imaginary;
      }
      case "complexCoefficientEqualsConstant": {
        const x = coefficient(p.coefficient), real = read(p.real), imaginary = read(p.imaginary);
        const sameReal = a.equal(x.real, real), sameImaginary = a.equal(x.imaginary, imaginary); return sameReal && sameImaginary;
      }
      case "realCoefficientStrictLowerBound": { const x = coefficient(p.coefficient), bound = read(p.lowerBound); const above = greater(x.real, bound); return a.isZero(x.imaginary) && above; }
      case "scalarRootEqualsConstant": return a.equal(root(p.root), read(p.expected));
      case "scalarRootEqualsRealCoefficient": return a.equal(root(p.root), a.multiply(coefficient(p.coefficient).real, read(p.factor)));
      case "scalarEqual": return a.equal(root(p.left), root(p.right));
      case "scalarArrayEqual": case "orderedNamesEqual": {
        charge("recordVisits", p.left.length + p.right.length); let ok = true;
        for (let i = 0; i < p.left.length; i++) {
          const equal = p.kind === "scalarArrayEqual" ? a.equal(root(p.left[i]), root(p.right[i])) : p.left[i] === p.right[i]; if (!equal) ok = false;
        }
        return ok;
      }
      case "geometryEqual": {
        const x = geometryField(p.left), y = geometryField(p.right); charge("recordVisits", x.length + y.length); let ok = x.length === y.length;
        for (let i = 0; i < x.length; i++) if (!a.equal(x[i], y[i])) ok = false; return ok;
      }
      case "geometryZero": { const x = geometryField(p.field); charge("recordVisits", x.length); let ok = true; for (const q of x) if (!a.isZero(q)) ok = false; return ok; }
      case "geometryTraceZero": { const x = geometryField(p.matrix); charge("recordVisits", 14); let sum = a.zero; for (let i = 0; i < 14; i++) sum = a.add(sum, x[15 * i]); return a.isZero(sum); }
      case "metadataScalarsZero": {
        let ok = true; for (const name of p.fields) { const id = errorIds.get(name); need(errors.has(id), "missing or released error result"); if (!a.isZero(errors.get(id)[0])) ok = false; } return ok;
      }
      default: need(false, "compiled predicate operation");
    }
  }
  function domain(descriptor) {
    const rows = tensor({ kind: "tensorNode", node: descriptor.node }); charge("recordVisits", rows.length); let ok = true;
    for (const row of rows) { const g = degree(row.blade), realDirection = (g * (g + 1) / 2) % 2 === 1; if (!a.isZero(realDirection ? row.imaginary : row.real)) ok = false; }
    return ok;
  }
  const snapshot = () => Object.freeze({ nextStep, consumersExecuted: executed, rootsObserved: seenRoots.size, failed, complete,
    liveRootValues: roots.size, liveErrorValues: errorValues, maximumRootValues, maximumErrorValues, usage: Object.freeze({ ...used }), arithmetic: a.snapshot(),
    cachedTensorArrays: tensorCache.size, cachedGeometryArrays: fieldCache.size, cachedMatrices: matrices.size,
    tensorAndGeometrySourceAuthenticationProved: false, outerWireConversionStorageProved: false, combinedResourceMajorantsProved: false, totalProcessMemoryProved: false });
  return Object.freeze({ schedule,
    captureRoot(name, text) { return guard(() => {
      need(rootMenu.has(name) && rootMenu.get(name) === nextStep && !seenRoots.has(name), "declared unique root at current step");
      if (neededRoots.has(name)) { need(roots.size < limits.rootValues, "root cache ceiling before parse"); roots.set(name, read(text)); maximumRootValues = Math.max(maximumRootValues, roots.size); }
      else read(text); // Also validate roots not consumed by a predicate.
      seenRoots.add(name); return true;
    }); },
    advance(step) { return guard(() => {
      need(integer(step, -1, degrees.length - 1) && step === nextStep, "strict consumer ready-step sequence");
      need(rootCensus[step + 1].every(name => seenRoots.has(name)), "all same-step scalar roots must precede consumers");
      for (const id of schedule.consumersByStep[step + 1]) {
        const consumer = schedule.consumers[id]; need(schedule.executionOrder[executed] === id, "complete consumer execution order");
        tensorCache = new Map(); fieldCache = new Map();
        const slots = consumer.kind === "error" && consumer.name === "nativeFirstJetNorms" ? 14 : 1;
        need(slots <= limits.resultValues, "result slots before evaluation"); let value, result;
        if (consumer.kind === "error") {
          if (lastErrorUse.has(id)) need(slots <= limits.errorValues - errorValues, "error cache ceiling before evaluation");
          result = consumer.name === "nativeFirstJetNorms" ? consumer.descriptor.map(norm) : [norm(consumer.descriptor)];
          value = consumer.name === "nativeFirstJetNorms" ? Object.freeze(result.map(output)) : output(result[0]);
        } else value = consumer.kind === "domain" ? domain(consumer.descriptor) : predicate(consumer.descriptor.predicate);
        charge("consumerComparisons"); need(external(compareConsumer, consumer, value) === true, "independent consumer comparison rejected " + consumer.name);
        if (consumer.kind !== "error") need(value === true, "numerical predicate failed " + consumer.name);
        if (consumer.kind === "error" && lastErrorUse.has(id)) { errors.set(id, result); errorValues += slots; maximumErrorValues = Math.max(maximumErrorValues, errorValues); }
        for (const input of consumer.consumerInputs) if (lastErrorUse.get(input) === executed) { errorValues -= errors.get(input).length; errors.delete(input); }
        tensorCache.clear(); fieldCache.clear(); executed++;
      }
      for (const name of schedule.releaseRootsByStep[step + 1]) need(roots.delete(name), "complete cached root release");
      nextStep++; return step < 0 ? Object.freeze([]) : schedule.releaseByStep[step];
    }); },
    finish() { return guard(() => {
      need(nextStep === degrees.length && executed === schedule.consumers.length && seenRoots.size === rootMenu.size && roots.size === 0 && errors.size === 0 && errorValues === 0, "complete consumer/root/error census");
      matrices.clear(); tensorCache.clear(); fieldCache.clear(); complete = true; return snapshot();
    }); }, snapshot
  });
}
module.exports = { createAuditConsumerReplay };

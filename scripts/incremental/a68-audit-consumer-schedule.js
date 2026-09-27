"use strict";

// Metadata only. Extend scalar/tensor liveness to EVERY independently declared
// check, domain check and error formula. This compiler does not evaluate their
// predicates; the separate consumer replay uses these combined releases,
// never the scalar-only release buckets.
const { compileScalarSchedule } = require("./a68-scalar-schedule");
const SCHEMA = "phase627-audit-consumer-schedule-v1";
const need = (ok, why) => { if (!ok) throw new Error("A68 audit consumers: " + why); };
const safe = (x, lo, hi = Number.MAX_SAFE_INTEGER) => Number.isSafeInteger(x) && !Object.is(x, -0) && x >= lo && x <= hi;
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const shape = (x, names) => need(x && typeof x === "object" && !Array.isArray(x) && Reflect.ownKeys(x).length === names.length &&
  names.every(k => Object.hasOwn(Object.getOwnPropertyDescriptor(x, k) ?? {}, "value")), "closed own-data descriptor");
const ERROR_FIELDS = Object.freeze(["radius", "fieldError", "motionNorm", "motionDerivativeSum", "referenceNorm", "referenceVariationNorm",
  "raw0Majorant", "raw2Majorant", "eulerMajorant", "raw0Error", "raw2Error", "eulerError", "nativeOneFormDualNorm", "nativeTwoFormDualNorm",
  "nativeRaw0Error", "nativeRaw2Error", "nativeEulerError", "nativeFieldNorm", "nativeFirstJetNorms"]);

function compileAuditConsumerSchedule({ tensorPlan, namedRoots, geometry, checks, domainChecks, error = null, scheduleLimits, limits }) {
  const keys = ["consumers", "expressionNodes", "expressionEdges", "tensorReferences", "scalarReferences", "stringCharacters", "maxDepth", "rationalCharacters", "retainedRootValues"];
  shape(limits, keys); need(keys.every(k => safe(limits[k], 1)) && limits.maxDepth <= 128 && limits.rationalCharacters <= 16384, "explicit bounded admission limits");
  const caps = Object.freeze({ ...limits }), usage = Object.fromEntries(keys.filter(k => !["maxDepth", "rationalCharacters", "retainedRootValues"].includes(k)).map(k => [k, 0]));
  const charge = (kind, n = 1) => { need(safe(n, 0) && n <= caps[kind] - usage[kind], "pre-allocation " + kind + " ceiling"); usage[kind] += n; };
  function array(value, limit) {
    need(Array.isArray(value), "dense own-data array"); const n = Object.getOwnPropertyDescriptor(value, "length")?.value;
    need(safe(n, 0, limit) && Reflect.ownKeys(value).length === n + 1, "bounded dense array");
    for (let i = 0; i < n; i++) need(Object.hasOwn(Object.getOwnPropertyDescriptor(value, String(i)) ?? {}, "value"), "own-data array indices");
    return n;
  }
  const string = x => { need(typeof x === "string" && /^[\x20-\x7e]+$/.test(x) && x.trim().length > 0, "printable nonempty string"); charge("stringCharacters", x.length); return x; };
  const rational = text => {
    need(typeof text === "string" && text.length <= caps.rationalCharacters && /^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(text), "bounded rational"); string(text);
    const [ns, ds] = text.split("/"), n = BigInt(ns), d = ds === undefined ? 1n : BigInt(ds);
    let a = n < 0n ? -n : n, b = d; while (b) [a, b] = [b, a % b];
    need(text !== "-0" && a === 1n && (ds === undefined || d !== 1n), "canonical rational"); return text;
  };
  // This independently validates all tensor/scalar topology and degrees too.
  const scalar = compileScalarSchedule(tensorPlan, namedRoots, geometry, scheduleLimits);
  const degrees = tensorPlan.nodes.map(node => node.degree), lastUse = [...scalar.tensorLastUse];
  const matrixNames = new Set(scalar.geometry.map(row => row.id));
  const roots = new Map(scalar.rootComparisons.map(row => [row.name, row.readyAfter]));
  const consumers = [], errors = new Map(), names = new Set(), active = new WeakSet();
  function walk(value, depth, action) {
    need(depth <= caps.maxDepth && value && typeof value === "object" && !active.has(value), "bounded acyclic expression");
    charge("expressionNodes"); active.add(value);
    try { return action(); } finally { active.delete(value); }
  }
  function state() { return { tensors: new Set(), roots: new Set(), dependencies: new Set(), fields: new Set() }; }
  function tensor(value, s) {
    const kind = Object.getOwnPropertyDescriptor(value ?? {}, "kind")?.value;
    if (kind === "literalZero") { shape(value, ["kind"]); return { kind }; }
    shape(value, ["kind", "node"]); need(kind === "tensorNode" && safe(value.node, 0, degrees.length - 1), "declared tensor reference");
    charge("tensorReferences"); s.tensors.add(value.node); return { kind, node: value.node };
  }
  function coefficient(value, s) {
    shape(value, ["tensor", "form", "blade", "k0", "k1", "absent"]);
    need(safe(value.form, 0, 16383) && safe(value.blade, 0, 16383) && safe(value.k0, 0, 0) && safe(value.k1, 0, 0) && value.absent === "0", "complete local coefficient selector");
    return { tensor: tensor(value.tensor, s), form: value.form, blade: value.blade, k0: 0, k1: 0, absent: "0" };
  }
  function root(value, s) { string(value); need(roots.has(value), "declared scalar root"); charge("scalarReferences"); s.roots.add(value); return value; }
  function geometryField(value, s) {
    string(value);
    const matrix = /^geometry\/geometry\.(motion|(?:deltaMetric|blockMetric)\.(?:g|d\[(\d+)\]|dd\[(\d+)\]\[(\d+)\])|(?:deltaConnection|palatini)\.(?:gamma\[(\d+)\]|dGamma\[(\d+)\]\[(\d+)\]|curvature))$/;
    const match = matrix.exec(value); need(match && match.slice(2).filter(x => x !== undefined).every(x => /^(0|[1-9][0-9]*)$/.test(x) && Number(x) < 14), "closed geometry metadata field");
    s.fields.add(value); return value;
  }
  function normExpression(value, s, depth = 1) {
    return walk(value, depth, () => {
      const op = Object.getOwnPropertyDescriptor(value, "op")?.value;
      const descend = x => { charge("expressionEdges"); return normExpression(x, s, depth + 1); };
      switch (op) {
        case "constant": shape(value, ["op", "value"]); return { op, value: rational(value.value) };
        case "sourceBoundScalar": shape(value, ["op", "binding", "positiveRequired"]);
          need(value.binding === "phase626/certifiedEpsilon" && value.positiveRequired === true, "fixed certified epsilon binding");
          return { op, binding: value.binding, positiveRequired: true };
        case "sum": case "product": {
          shape(value, ["op", "values"]); const n = array(value.values, caps.expressionEdges - usage.expressionEdges); need(n > 0, "nonempty norm expression");
          return { op, values: Array.from({ length: n }, (_, i) => descend(value.values[i])) };
        }
        case "tensorL1": shape(value, ["op", "tensor"]); return { op, tensor: tensor(value.tensor, s) };
        case "entryL1": shape(value, ["op", "value"]); return { op, value: matrixExpression(value.value, s, depth + 1) };
        case "maximumColumnL1": case "maximumRowL1": shape(value, ["op", "matrix"]); return { op, matrix: matrixExpression(value.matrix, s, depth + 1) };
        default: need(false, "closed norm expression menu");
      }
    });
  }
  function matrixExpression(value, s, depth) {
    charge("expressionEdges"); return walk(value, depth, () => {
      const op = Object.getOwnPropertyDescriptor(value, "op")?.value;
      if (op === "geometryMatrix") { shape(value, ["op", "name"]); string(value.name); need(matrixNames.has(value.name), "declared geometry matrix"); return { op, name: value.name }; }
      shape(value, ["op", "left", "right"]); need(op === "matrixProduct", "closed matrix expression menu");
      return { op, left: matrixExpression(value.left, s, depth + 1), right: matrixExpression(value.right, s, depth + 1) };
    });
  }
  function predicate(value, s, depth = 1) {
    return walk(value, depth, () => {
      const kind = Object.getOwnPropertyDescriptor(value, "kind")?.value;
      switch (kind) {
        case "tensorEqual": shape(value, ["kind", "left", "right"]); return { kind, left: tensor(value.left, s), right: tensor(value.right, s) };
        case "tensorZero": shape(value, ["kind", "tensor"]); return { kind, tensor: tensor(value.tensor, s) };
        case "tensorBladeGrades": {
          shape(value, ["kind", "tensor", "allowed"]); const n = array(value.allowed, 15);
          need(n > 0, "nonempty blade grade menu"); charge("expressionEdges", n);
          const allowed = []; let previous = -1;
          for (let i = 0; i < n; i++) { const grade = value.allowed[i]; need(safe(grade, 0, 14) && grade > previous, "ordered distinct blade grades"); allowed.push(grade); previous = grade; }
          return { kind, tensor: tensor(value.tensor, s), allowed };
        }
        case "all": {
          shape(value, ["kind", "predicates"]); const n = array(value.predicates, caps.expressionEdges - usage.expressionEdges); need(n > 0, "nonempty predicate conjunction");
          return { kind, predicates: Array.from({ length: n }, (_, i) => { charge("expressionEdges"); return predicate(value.predicates[i], s, depth + 1); }) };
        }
        case "complexCoefficientScaledEqual": shape(value, ["kind", "left", "right", "factor"]);
          return { kind, left: coefficient(value.left, s), right: coefficient(value.right, s), factor: rational(value.factor) };
        case "complexCoefficientEqualsConstant": shape(value, ["kind", "coefficient", "real", "imaginary"]);
          return { kind, coefficient: coefficient(value.coefficient, s), real: rational(value.real), imaginary: rational(value.imaginary) };
        case "realCoefficientStrictLowerBound": shape(value, ["kind", "coefficient", "imaginaryRequired", "lowerBound"]); need(value.imaginaryRequired === "0", "complete real coefficient test");
          return { kind, coefficient: coefficient(value.coefficient, s), imaginaryRequired: "0", lowerBound: rational(value.lowerBound) };
        case "scalarRootEqualsConstant": shape(value, ["kind", "root", "expected"]); return { kind, root: root(value.root, s), expected: rational(value.expected) };
        case "scalarRootEqualsRealCoefficient": shape(value, ["kind", "root", "coefficient", "factor"]);
          return { kind, root: root(value.root, s), coefficient: coefficient(value.coefficient, s), factor: rational(value.factor) };
        case "scalarEqual": shape(value, ["kind", "left", "right"]); return { kind, left: root(value.left, s), right: root(value.right, s) };
        case "scalarArrayEqual": case "orderedNamesEqual": {
          shape(value, ["kind", "left", "right"]);
          const bound = kind === "scalarArrayEqual" ? caps.scalarReferences : caps.stringCharacters;
          const n = array(value.left, bound); need(array(value.right, bound) === n && n > 0, "same nonempty comparison array shape");
          const item = text => kind === "scalarArrayEqual" ? root(text, s) : string(text);
          return { kind, left: Array.from({ length: n }, (_, i) => item(value.left[i])), right: Array.from({ length: n }, (_, i) => item(value.right[i])) };
        }
        case "geometryEqual": {
          shape(value, ["kind", "left", "right"]); const left = geometryField(value.left, s), right = geometryField(value.right, s);
          need(left.endsWith(".curvature") === right.endsWith(".curvature"), "same geometry field shape"); return { kind, left, right };
        }
        case "geometryTraceZero": {
          shape(value, ["kind", "matrix"]); const matrix = geometryField(value.matrix, s);
          need(!matrix.endsWith(".curvature"), "trace requires a geometry matrix"); return { kind, matrix };
        }
        case "geometryZero": shape(value, ["kind", "field"]); return { kind, field: geometryField(value.field, s) };
        case "metadataScalarsZero": {
          shape(value, ["kind", "fields"]); const n = array(value.fields, ERROR_FIELDS.length); need(n > 0, "nonempty scalar field check");
          return { kind, fields: Array.from({ length: n }, (_, i) => {
            const field = string(value.fields[i]); need(errors.has(field) && field !== "error/nativeFirstJetNorms", "declared scalar error field"); s.dependencies.add(errors.get(field)); return field;
          }) };
        }
        default: need(false, "closed predicate menu");
      }
    });
  }
  function emit(kind, name, descriptor, s) {
    charge("consumers"); string(name); const key = kind + "/" + name; need(!names.has(key), "unique consumer name"); names.add(key);
    let readyAfter = -1;
    s.tensors.forEach(id => { readyAfter = Math.max(readyAfter, id); });
    s.roots.forEach(id => { readyAfter = Math.max(readyAfter, roots.get(id)); });
    s.dependencies.forEach(id => { readyAfter = Math.max(readyAfter, consumers[id].readyAfter); });
    s.tensors.forEach(id => { lastUse[id] = Math.max(lastUse[id], readyAfter); });
    const id = consumers.length;
    consumers.push({ id, kind, name, readyAfter, tensorInputs: [...s.tensors], scalarRoots: [...s.roots], consumerInputs: [...s.dependencies], geometryFields: [...s.fields], descriptor });
    return id;
  }
  if (error !== null) {
    shape(error, ["schemaVersion", "numericalReplayImplemented", "fields"]);
    need(error.schemaVersion === "phase627-mixed-error-formula-obligations-v1" && error.numericalReplayImplemented === false, "independent unevaluated error formulas");
    shape(error.fields, ERROR_FIELDS);
    for (const name of ERROR_FIELDS) {
      const s = state(), value = error.fields[name]; let descriptor;
      if (name === "nativeFirstJetNorms") { need(array(value, 14) === 14, "all14 native jet norm formulas"); descriptor = Array.from({ length: 14 }, (_, i) => normExpression(value[i], s)); }
      else descriptor = normExpression(value, s);
      errors.set("error/" + name, emit("error", name, descriptor, s));
    }
  }
  const domainCount = array(domainChecks, caps.consumers);
  for (let i = 0; i < domainCount; i++) {
    const item = domainChecks[i]; shape(item, ["name", "node", "degree", "local", "hAntiHermitian", "canonicalNonzeroRecords"]);
    const s = state(); tensor({ kind: "tensorNode", node: item.node }, s);
    need(safe(item.degree, 0, 14) && (degrees[item.node] === -1 || degrees[item.node] === item.degree) && item.local === true && item.hAntiHermitian === true && item.canonicalNonzeroRecords === true, "complete tensor-domain obligation");
    emit("domain", item.name, { node: item.node, degree: item.degree, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true }, s);
  }
  const checkCount = array(checks, caps.consumers);
  for (let i = 0; i < checkCount; i++) {
    const check = checks[i]; shape(check, ["name", "metadataPath", "predicate"]); string(check.metadataPath);
    need(check.metadataPath === "check/" + check.name, "exact check metadata binding");
    const s = state(); emit("check", check.name, { metadataPath: check.metadataPath, predicate: predicate(check.predicate, s) }, s);
  }
  const executionOrder = consumers.map(c => c.id).sort((a, b) => consumers[a].readyAfter - consumers[b].readyAfter || a - b);
  const consumersByStep = Array.from({ length: degrees.length + 1 }, () => []), releaseByStep = Array.from({ length: degrees.length }, () => []);
  executionOrder.forEach(id => consumersByStep[consumers[id].readyAfter + 1].push(id));
  lastUse.forEach((step, id) => releaseByStep[step].push(id));
  // Scalar replay compares roots immediately then releases its own values.
  // Copy ONLY consumer-needed roots into a separately admitted immutable cache.
  const rootLastUse = new Map();
  consumers.forEach(c => c.scalarRoots.forEach(name => rootLastUse.set(name, Math.max(rootLastUse.get(name) ?? -1, c.readyAfter))));
  const captureRootsByStep = Array.from({ length: degrees.length + 1 }, () => []), releaseRootsByStep = Array.from({ length: degrees.length + 1 }, () => []);
  rootLastUse.forEach((step, name) => { captureRootsByStep[roots.get(name) + 1].push(name); releaseRootsByStep[step + 1].push(name); });
  let liveRoots = 0, maximumRetainedRootValues = 0;
  for (let step = 0; step < captureRootsByStep.length; step++) {
    liveRoots += captureRootsByStep[step].length; need(liveRoots <= caps.retainedRootValues, "retained root value ceiling");
    maximumRetainedRootValues = Math.max(maximumRetainedRootValues, liveRoots); liveRoots -= releaseRootsByStep[step].length;
  }
  need(liveRoots === 0, "complete root cache releases");
  return freeze({ schemaVersion: SCHEMA, scalarSchedule: scalar, consumers, executionOrder, consumersByStep, tensorLastUse: lastUse, releaseByStep,
    captureRootsByStep, releaseRootsByStep, maximumRetainedRootValues, usage,
    executionContract: "At step -1 and after each tensor i: run ready scalars and capture needed roots; evaluate all consumersByStep[i+1], retaining their scalar/boolean results for metadata comparison; release root cache entries, then ALL tensors in the combined releaseByStep[i]. Never use scalar-only tensor releases.",
    scope: { allDeclaredConsumersScheduled: true, numericPredicatesEvaluated: false, errorFormulasEvaluated: false, runtimeIntegrated: false,
      scientificSourceClosureEstablished: false, consumerResultStorageBounded: false, combinedArithmeticAndStorageMajorantsProved: false, totalProcessMemoryProved: false } });
}
module.exports = { compileAuditConsumerSchedule, ERROR_FIELDS, SCHEMA };

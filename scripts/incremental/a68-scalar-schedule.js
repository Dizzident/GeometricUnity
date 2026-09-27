"use strict";

// Metadata scheduler ONLY. Inputs must be independently generated semantic
// recipes, never descriptors taken from an observed result graph. This does
// not establish source/physical authenticity and does not evaluate scalars.
// TOP.tensor is opaque NONAUTHORITATIVE metadata: only its validated node ID
// is compiled. No closed-session handle identity is inferred from that token.
const { PLAN_SCHEMA } = require("./a68-mixed-recipe");
const { TOP_SCHEMA, WEIGHTS } = require("./a68-original-action-recipe");
const SCHEMA = "phase627-scalar-earliest-ready-schedule-v1";
const need = (ok, why) => { if (!ok) throw new Error("A68 scalar schedule: " + why); };
const integer = (n, lo, hi) => Number.isSafeInteger(n) && n >= lo && n <= hi;
const shape = (x, keys) => {
  need(x !== null && typeof x === "object" && !Array.isArray(x) &&
    Reflect.ownKeys(x).length === keys.length && keys.every(k => Object.hasOwn(x, k)), "closed descriptor shape");
  need(keys.every(k => Object.getOwnPropertyDescriptor(x, k).get === undefined &&
    Object.getOwnPropertyDescriptor(x, k).set === undefined), "plain data properties required");
};
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const gcd = (a, b) => { a = a < 0n ? -a : a; while (b) [a, b] = [b, a % b]; return a; };
const popcount = n => { let d = 0; while (n) { d += n & 1; n >>>= 1; } return d; };

function compileScalarSchedule(tensorPlan, namedRoots, declaredGeometry, limits) {
  const limitNames = ["tensorNodes", "tensorEdges", "tensorMarks", "scalarNodes", "scalarEdges", "roots",
    "geometryMatrices", "geometryEntries", "maxDepth", "stringCharacters", "rationalCharacters"];
  shape(limits, limitNames);
  need(limitNames.every(k => integer(limits[k], 1, Number.MAX_SAFE_INTEGER)), "explicit positive finite limits");
  need(limits.maxDepth <= 512 && limits.rationalCharacters <= 16384, "bounded depth/rational limits");
  const used = Object.fromEntries(limitNames.filter(k => !["maxDepth", "rationalCharacters"].includes(k)).map(k => [k, 0]));
  const charge = (kind, count = 1) => {
    need(integer(count, 0, Number.MAX_SAFE_INTEGER) && count <= limits[kind] - used[kind], "pre-allocation " + kind + " ceiling");
    used[kind] += count;
  };
  const string = (text, nonempty = true) => {
    need(typeof text === "string" && /^[\x20-\x7e]*$/.test(text) && (!nonempty || text.trim().length > 0), "printable ASCII string");
    charge("stringCharacters", text.length); return text;
  };
  const rational = text => {
    need(typeof text === "string" && text.length <= limits.rationalCharacters && /^-?(0|[1-9][0-9]*)(\/[1-9][0-9]*)?$/.test(text), "bounded canonical rational");
    string(text); const [ns, ds] = text.split("/"), n = BigInt(ns), d = ds === undefined ? 1n : BigInt(ds);
    need(text !== "-0" && gcd(n, d) === 1n && (ds === undefined || d !== 1n), "reduced canonical rational"); return text;
  };
  const matrix = entries => {
    need(Array.isArray(entries) && entries.length <= 196, "sparse geometry matrix"); charge("geometryEntries", entries.length);
    need(Object.keys(entries).length === entries.length && Array.from(entries).every(x => x !== undefined), "dense matrix entry array");
    let previous = -1;
    return entries.map(e => {
      shape(e, ["row", "column", "value"]);
      need(integer(e.row, 0, 13) && integer(e.column, 0, 13) && e.row * 14 + e.column > previous, "canonical matrix ordering");
      previous = e.row * 14 + e.column; rational(e.value); need(e.value !== "0", "sparse matrix zero omitted");
      return { row: e.row, column: e.column, value: e.value };
    });
  };
  shape(tensorPlan, ["schemaVersion", "leaves", "nodes", "marks"]);
  need(tensorPlan.schemaVersion === PLAN_SCHEMA && Array.isArray(tensorPlan.nodes) && Array.isArray(tensorPlan.leaves) && Array.isArray(tensorPlan.marks), "independent tensor recipe schema");
  charge("tensorNodes", tensorPlan.nodes.length); charge("tensorMarks", tensorPlan.marks.length);
  need(tensorPlan.leaves.length <= tensorPlan.nodes.length, "leaf census bounded by nodes");
  const leaves = new Map(), registered = new Set(), degrees = [], tensorLastUse = [];
  for (const leaf of tensorPlan.leaves) {
    shape(leaf, ["id", "degree", "source", "sha256"]); string(leaf.id); string(leaf.source); string(leaf.sha256);
    need(integer(leaf.degree, 0, 14) && /^[0-9a-f]{64}$/.test(leaf.sha256) && !leaves.has(leaf.id), "unique typed source leaf"); leaves.set(leaf.id, leaf.degree);
  }
  const ref = id => { need(integer(id, 0, tensorPlan.nodes.length - 1), "foreign tensor node ID"); return id; };
  for (const node of tensorPlan.nodes) {
    shape(node, ["id", "op", "degree", "inputs", "parameters"]);
    const id = degrees.length; need(node.id === id && Array.isArray(node.inputs), "sequential tensor topology");
    charge("tensorEdges", node.inputs.length);
    need(Object.keys(node.inputs).length === node.inputs.length && Array.from(node.inputs).every(Number.isSafeInteger), "dense tensor input array");
    const ds = node.inputs.map(input => {
      need(integer(input, 0, id - 1), "earlier tensor dependency required"); tensorLastUse[input] = id; return degrees[input];
    });
    const p = node.parameters, arity = n => need(ds.length === n, "tensor primitive arity"); let d;
    switch (node.op) {
      case "leaf": shape(p, ["id"]); arity(0); need(leaves.has(p.id) && !registered.has(p.id), "registered source leaf"); registered.add(p.id); d = leaves.get(p.id); break;
      case "zero": shape(p, []); arity(0); d = -1; break;
      case "unit": shape(p, ["form", "blade", "real", "imaginary"]); arity(0); need(integer(p.form, 0, 16383) && integer(p.blade, 0, 16383), "unit masks"); rational(p.real); rational(p.imaginary); d = popcount(p.form); break;
      case "sum": {
        shape(p, []); need(ds.length > 0, "nonempty tensor sum"); const known = [...new Set(ds.filter(x => x >= 0))];
        need(known.length <= 1, "tensor sum degree agreement"); d = known.length ? known[0] : -1; break;
      }
      case "scale": shape(p, ["real", "imaginary"]); arity(1); rational(p.real); rational(p.imaginary); d = ds[0]; break;
      case "product": case "transpose":
        shape(p, ["kind"]); arity(2); need((node.op === "product" ? ["W", "C", "A"] : ["C", "A"]).includes(p.kind), "product kind");
        d = ds.some(x => x < 0) ? -1 : node.op === "product" ? (ds[0] + ds[1] > 14 ? -1 : ds[0] + ds[1]) : (ds[1] < ds[0] ? -1 : ds[1] - ds[0]); break;
      case "star": shape(p, []); arity(1); d = ds[0] < 0 ? -1 : 14 - ds[0]; break;
      case "starAdjoint": shape(p, ["degree"]); arity(1); need(integer(p.degree, 0, 14) && (ds[0] < 0 || ds[0] === 14 - p.degree), "star adjoint degree"); d = p.degree; break;
      case "motion": case "pullback": case "spin":
        shape(p, ["matrix"]); arity(node.op === "spin" ? 0 : 1); matrix(p.matrix); d = node.op === "spin" ? 0 : ds[0]; break;
      case "raise": shape(p, []); arity(1); d = ds[0]; break;
      case "contract": shape(p, ["axis"]); arity(1); need(integer(p.axis, 0, 13), "contract axis"); d = ds[0] <= 0 ? -1 : ds[0] - 1; break;
      case "component": shape(p, ["form"]); arity(1); need(integer(p.form, 0, 16383), "component mask"); d = 0; break;
      default: need(false, "closed tensor primitive menu");
    }
    need(node.degree === d, "tensor primitive degree mismatch"); degrees.push(d); tensorLastUse.push(id);
  }
  need(registered.size === leaves.size, "complete registered leaf census");
  const markNames = new Set();
  for (const mark of tensorPlan.marks) {
    shape(mark, ["name", "degree", "node", "expanded"]); string(mark.name); ref(mark.node);
    need(!markNames.has(mark.name) && integer(mark.degree, 0, 14) && (degrees[mark.node] < 0 || degrees[mark.node] === mark.degree) && typeof mark.expanded === "boolean", "unique typed tensor mark"); markNames.add(mark.name);
  }
  need(Array.isArray(declaredGeometry), "declared geometry array"); charge("geometryMatrices", declaredGeometry.length);
  const geometry = [], geometryNames = new Set();
  for (const item of declaredGeometry) {
    shape(item, ["id", "entries"]); string(item.id); need(!geometryNames.has(item.id), "unique declared geometry"); geometryNames.add(item.id);
    geometry.push({ id: item.id, entries: matrix(item.entries) });
  }
  need(Array.isArray(namedRoots), "named roots array"); charge("roots", namedRoots.length);
  need(Object.keys(namedRoots).length === namedRoots.length && Array.from(namedRoots).every(x => x !== undefined), "dense named roots array");
  const nodes = [], identities = new WeakMap(), active = new WeakSet(), heights = [], rootNames = new Set();
  function visit(expression, depth) {
    need(expression !== null && typeof expression === "object" && !Array.isArray(expression), "scalar expression object");
    need(depth <= limits.maxDepth, "scalar depth ceiling"); need(!active.has(expression), "cyclic scalar expression");
    if (identities.has(expression)) {
      const id = identities.get(expression); need(depth + heights[id] - 1 <= limits.maxDepth, "shared scalar depth ceiling"); return id;
    }
    charge("scalarNodes"); active.add(expression);
    let op, inputs = [], tensorInputs = [], parameters = {}, readyAfter = -1;
    if (Object.hasOwn(expression, "schemaVersion")) {
      shape(expression, ["schemaVersion", "tensor", "node", "degree", "form", "blade", "k0", "k1", "absentCoefficient", "imaginaryRequired", "realFactor", "weight"]);
      need(expression.schemaVersion === TOP_SCHEMA && expression.degree === 14 && expression.form === 16383 && expression.blade === 0 && expression.k0 === 0 && expression.k1 === 0 && expression.absentCoefficient === "0" && expression.imaginaryRequired === "0" && expression.realFactor === "-1" && WEIGHTS.includes(expression.weight), "exact TOP extraction descriptor");
      ref(expression.node); need(degrees[expression.node] === -1 || degrees[expression.node] === 14, "TOP tensor degree");
      op = "top"; tensorInputs = [expression.node]; readyAfter = expression.node;
      parameters = { degree: 14, form: 16383, blade: 0, k0: 0, k1: 0, absentCoefficient: "0", imaginaryRequired: "0", realFactor: "-1", weight: rational(expression.weight) };
    } else {
      op = Object.getOwnPropertyDescriptor(expression, "op")?.value;
      switch (op) {
        case "constant": shape(expression, ["op", "value"]); parameters = { value: rational(expression.value) }; break;
        case "matrixEntry":
          shape(expression, ["op", "matrix", "row", "column"]); string(expression.matrix);
          need(geometryNames.has(expression.matrix) && integer(expression.row, 0, 13) && integer(expression.column, 0, 13), "declared matrix entry");
          parameters = { matrix: expression.matrix, row: expression.row, column: expression.column }; break;
        case "pair":
          shape(expression, ["op", "left", "right"]); ref(expression.left); ref(expression.right);
          need(degrees[expression.left] < 0 || degrees[expression.right] < 0 || degrees[expression.left] === degrees[expression.right], "Pair degree agreement");
          tensorInputs = [expression.left, expression.right]; readyAfter = Math.max(...tensorInputs); break;
        case "add": case "multiply":
          shape(expression, ["op", "left", "right"]); charge("scalarEdges", 2);
          inputs = [visit(expression.left, depth + 1), visit(expression.right, depth + 1)];
          readyAfter = Math.max(nodes[inputs[0]].readyAfter, nodes[inputs[1]].readyAfter); break;
        default: need(false, "closed scalar primitive menu");
      }
    }
    tensorInputs.forEach(id => { tensorLastUse[id] = Math.max(tensorLastUse[id], readyAfter); });
    const id = nodes.length; nodes.push({ id, op, inputs, tensorInputs, parameters, readyAfter });
    heights.push(1 + (inputs.length ? Math.max(...inputs.map(i => heights[i])) : 0)); identities.set(expression, id); active.delete(expression); return id;
  }
  const roots = namedRoots.map(root => {
    shape(root, ["name", "expression"]); string(root.name); need(!rootNames.has(root.name), "unique scalar root name"); rootNames.add(root.name);
    return { name: root.name, node: visit(root.expression, 1) };
  });
  const executionOrder = nodes.map(node => node.id).sort((a, b) => nodes[a].readyAfter - nodes[b].readyAfter || a - b);
  const position = Array(nodes.length), scalarLastUse = Array(nodes.length);
  executionOrder.forEach((id, step) => { position[id] = step; scalarLastUse[id] = step; });
  executionOrder.forEach((id, step) => nodes[id].inputs.forEach(input => {
    need(position[input] < step, "scalar execution topology"); scalarLastUse[input] = Math.max(scalarLastUse[input], step);
  }));
  const scalarReleaseByExecution = Array.from({ length: nodes.length }, () => []);
  scalarLastUse.forEach((step, id) => scalarReleaseByExecution[step].push(id));
  const releaseByStep = Array.from({ length: degrees.length }, () => []);
  tensorLastUse.forEach((step, id) => releaseByStep[step].push(id));
  const rootComparisons = roots.map((root, index) => ({ root: index, name: root.name, scalar: root.node,
    readyAfter: nodes[root.node].readyAfter, afterScalarExecution: position[root.node] }))
    .sort((a, b) => a.afterScalarExecution - b.afterScalarExecution || a.root - b.root);
  let live = 0, maximumLiveScalarValues = 0;
  executionOrder.forEach((id, step) => { live++; maximumLiveScalarValues = Math.max(maximumLiveScalarValues, live); live -= scalarReleaseByExecution[step].length; });
  need(live === 0, "all scalar values released");
  return freeze({ schemaVersion: SCHEMA, nodes, geometry, executionOrder, roots, rootComparisons,
    tensorLastUse, releaseByStep, scalarLastUse, scalarReleaseByExecution, maximumLiveScalarValues,
    usage: used, executionContract: "Run readyAfter=-1 scalars before tensors; after each tensor i, compare tensor marks and run all readyAfter=i scalars in executionOrder; compare scalar roots immediately after their scalar operation, then scalar releases; finally release ALL tensors in releaseByStep[i], not merely current tensor inputs.",
    numericContract: "Pair must test complete imaginary trace cancellation; Top must test the selected coefficient imaginary part, use absent zero, then apply realFactor=-1 before weight. No such numerical checks occur here.",
    scope: { numericReplayImplemented: false, scientificAuthenticityEstablished: false,
      topHandleIdentityAuthenticated: false, scalarLifetimesScheduled: true,
      scalarValueHeightsBounded: false, metadataMemoryProved: false, totalProcessMemoryProved: false } });
}

module.exports = { compileScalarSchedule, SCHEMA };

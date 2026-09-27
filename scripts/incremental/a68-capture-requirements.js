"use strict";

// Conditional copy requirements from COMPLETE independently supplied symbolic
// plans, never observed producer graphs. Counting does not authenticate those
// plans, establish their physical premises or authorize scientific execution.
// No source geometry, coefficient evaluation, I/O or production caps occur here.
const { PLAN_SCHEMA } = require("./a68-mixed-recipe");
const { sourceContextIds } = require("./a68-source-context-menu");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const FIELDS = Object.freeze(["graphs", "nodeObjects", "arrays", "slots", "elementCopies"]);
const SCHEMA = "phase627-capture-requirements-v1";
const scope = Object.freeze({ symbolicTopologyCounted: true, sourceAuthenticityEstablished: false,
  primitiveSemanticsValidated: false, productionResourceSufficiencyProved: false,
  totalProcessMemoryProved: false, scientificExecutionAuthorized: false });
const need = (ok, why) => { if (!ok) throw new Error("A68 capture requirements: " + why); };
const natural = n => Number.isSafeInteger(n) && n >= 0;
const named = s => typeof s === "string" && /^[\x20-\x7e]+$/.test(s) && s.trim().length > 0;
const zero = () => Object.fromEntries(FIELDS.map(k => [k, 0]));
function shape(value, keys) {
  need(value && typeof value === "object" && !Array.isArray(value), "closed object required");
  const actual = Object.keys(value);
  need(actual.length === keys.length && keys.every(k => actual.includes(k)), "exact fields " + keys.join(","));
}
function add(a, b) { need(natural(a) && natural(b) && b <= Number.MAX_SAFE_INTEGER - a, "safe cumulative count"); return a + b; }
function multiply(a, b) {
  need(natural(a) && natural(b) && (a === 0 || b <= Math.floor(Number.MAX_SAFE_INTEGER / a)), "safe multiplied count");
  const result = a * b; need(natural(result), "safe multiplied result"); return result;
}
const plus = (a, b) => Object.fromEntries(FIELDS.map(k => [k, add(a[k], b[k])]));
const times = (a, count) => Object.fromEntries(FIELDS.map(k => [k, multiply(a[k], count)]));
const maximum = (a, b) => Object.fromEntries(FIELDS.map(k => [k, Math.max(a[k], b[k])]));
const freezeCounts = value => Object.freeze({ ...value });
function copyLimits(value) {
  shape(value, ["normal", "inspection", "failure"]);
  for (const lane of Object.values(value)) { shape(lane, FIELDS); need(FIELDS.every(k => natural(lane[k])), "nonnegative safe copy ceilings"); }
  return value;
}
function count(plan) {
  shape(plan, ["schemaVersion", "leaves", "nodes", "marks"]);
  need(plan.schemaVersion === PLAN_SCHEMA, "independent symbolic plan schema, not producer evidence");
  need(Array.isArray(plan.leaves) && Array.isArray(plan.nodes) && Array.isArray(plan.marks), "complete plan arrays");
  const declared = new Map(), registered = new Set(), marks = new Set();
  for (const leaf of plan.leaves) {
    shape(leaf, ["id", "degree", "source", "sha256"]);
    need(named(leaf.id) && named(leaf.source) && natural(leaf.degree) && leaf.degree <= 14 &&
      typeof leaf.sha256 === "string" && /^[0-9a-f]{64}$/.test(leaf.sha256) && !declared.has(leaf.id), "unique complete typed leaf declaration");
    declared.set(leaf.id, leaf.degree);
  }
  let edges = 0;
  for (let i = 0; i < plan.nodes.length; i++) {
    const node = plan.nodes[i]; shape(node, ["id", "op", "degree", "inputs", "parameters"]);
    need(node.id === i && named(node.op) && Number.isInteger(node.degree) && node.degree >= -1 && node.degree <= 14 &&
      Array.isArray(node.inputs) && node.parameters && typeof node.parameters === "object" && !Array.isArray(node.parameters), "ordered typed symbolic node");
    // Input multiplicity matters: [0,0] allocates/copies TWO integers.
    for (const input of node.inputs) need(natural(input) && input < i, "strict earlier-node input topology");
    edges = add(edges, node.inputs.length);
    if (node.op === "leaf") {
      shape(node.parameters, ["id"]); const id = node.parameters.id;
      need(node.inputs.length === 0 && declared.get(id) === node.degree && !registered.has(id), "one typed registration per declared leaf");
      registered.add(id);
    }
  }
  need(registered.size === declared.size, "all declared leaves registered in complete plan");
  for (const mark of plan.marks) {
    shape(mark, ["name", "degree", "node", "expanded"]);
    need(named(mark.name) && !marks.has(mark.name) && natural(mark.degree) && mark.degree <= 14 &&
      natural(mark.node) && mark.node < plan.nodes.length && typeof mark.expanded === "boolean", "unique typed complete mark");
    need(plan.nodes[mark.node].degree < 0 || plan.nodes[mark.node].degree === mark.degree, "mark target degree"); marks.add(mark.name);
  }
  const leaves = plan.leaves.length, nodes = plan.nodes.length, markCount = plan.marks.length;
  const slots = add(add(add(leaves, nodes), markCount), edges);
  return Object.freeze({ shape: Object.freeze({ leaves, nodes, marks: markCount, inputReferences: edges }),
    copies: freezeCounts({ graphs: 1, nodeObjects: nodes, arrays: add(nodes, 3), slots, elementCopies: slots }), scope });
}
function deriveCaptureShape(plan, metadataLimits) {
  return count(snapshotCanonicalMetadata(plan, metadataLimits).value);
}
function createCaptureRequirementCensus(inspectionPolicy, metadataLimits) {
  // One cumulative metadata allowance across policy, all705 plans and limits.
  const caps = snapshotCanonicalMetadata(metadataLimits, { nodes: 20, arraySlots: 20, stringCharacters: 200, maxDepth: 2 }).value;
  shape(caps, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(caps).every(n => Number.isSafeInteger(n) && n > 0) && caps.maxDepth <= 64, "explicit cumulative metadata ceilings");
  const used = { nodes: 0, arraySlots: 0, stringCharacters: 0 }, ids = sourceContextIds();
  let failed = false, busy = false, finished = false, index = 0, point = null;
  let normal = zero(), inspection = zero(), failure = zero();
  function capture(value) {
    const remaining = { ...caps };
    for (const key of Object.keys(used)) { remaining[key] -= used[key]; need(remaining[key] > 0, "remaining metadata allowance"); }
    const result = snapshotCanonicalMetadata(value, remaining);
    for (const key of Object.keys(used)) used[key] = add(used[key], result.usage[key]);
    need(!failed, "swallowed metadata snapshot reentry"); return result.value;
  }
  const policy = capture(inspectionPolicy);
  need(Array.isArray(policy) && policy.length === ids.length, "explicit all705 inspection policy");
  policy.forEach((entry, i) => {
    shape(entry, ["id", "copies"]); need(entry.id === ids[i] && natural(entry.copies), "ordered context and finite inspection count");
  });
  function guard(action) {
    if (failed || busy || finished) { failed = true; need(false, "failed reentrant or finished census"); }
    busy = true;
    try { const result = action(); need(!failed, "swallowed census failure"); return result; }
    catch (error) { failed = true; throw error; } finally { busy = false; }
  }
  return Object.freeze({
    add: (id, independentPlan) => guard(() => {
      need(index < ids.length && id === ids[index], "exact complete context sequence");
      const current = count(capture(independentPlan)), cost = current.copies;
      const nextNormal = plus(normal, cost), nextInspection = plus(inspection, times(cost, policy[index].copies));
      // One terminal failure can retain a suspended point AND its active child.
      // Componentwise maxima are conservative even if attained at different
      // failure locations. Diagnostics run alone. Full shapes bound all prefixes.
      let terminal = cost, nextPoint = point;
      if (/^point[01]$/.test(id)) nextPoint = cost;
      else if (id.startsWith("point")) { need(point !== null, "suspended point before child"); terminal = plus(point, cost); }
      else nextPoint = null;
      const nextFailure = maximum(failure, terminal);
      normal = nextNormal; inspection = nextInspection; failure = nextFailure; point = nextPoint; index++;
      return current;
    }),
    finish: proposedLimits => guard(() => {
      need(index === ids.length, "all705 independent plans before completion");
      const limits = copyLimits(capture(proposedLimits)), requirements = { normal, inspection, failure };
      for (const lane of Object.keys(requirements)) for (const key of FIELDS)
        need(limits[lane][key] >= requirements[lane][key], "insufficient " + lane + "/" + key + " copy ceiling");
      const result = Object.freeze({ schema: SCHEMA, contexts: index,
        requirements: Object.freeze(Object.fromEntries(Object.entries(requirements).map(([k, v]) => [k, freezeCounts(v)]))),
        limits, inspectionPolicy: policy, scope });
      finished = true; return result;
    }),
    snapshot: () => Object.freeze({ contexts: index, failed, finished, metadata: Object.freeze({ ...used }), scope })
  });
}
module.exports = { SCHEMA, deriveCaptureShape, createCaptureRequirementCensus };

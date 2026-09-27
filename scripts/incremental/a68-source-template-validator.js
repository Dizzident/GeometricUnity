"use strict";

// Source STRUCTURE preflight, never coefficient/source authentication. The
// caller supplies an independently frozen profile, not one inferred from the
// candidate template. Transport must separately pin/parse canonical wire bytes.
// Explicit snapshot quotas bound copied JSON-content accounting, not total RSS,
// serializer buffers or the source-menu builder's temporary live storage.
const { sourceContextIds, sourceContextMenu } = require("./a68-source-context-menu");
const { snapshotCanonicalMetadata } = require("./a68-canonical-metadata");
const { deriveProspectiveCaptureRequirements, REVIEWED_SOURCE_SHA256 } = require("./a68-context-topology-envelope");
const IDS = sourceContextIds(), ALLOWED = new Set(IDS);
const need = (ok, why) => { if (!ok) throw new Error("A68 source template validator: " + why); };
const integer = n => Number.isSafeInteger(n) && n > 0;
const ordinal = (a, b) => a < b ? -1 : a > b ? 1 : 0;
const scope = Object.freeze({ structureOnly: true, numericalReplayComplete: false, sourceAuthenticityEstablished: false,
  resourceSufficiencyProved: false, totalProcessMemoryProved: false, scientificExecutionAuthorized: false });
function own(value, name) {
  const d = value && Object.getOwnPropertyDescriptor(value, name);
  need(d && Object.hasOwn(d, "value") && d.enumerable, "own data field " + name); return d.value;
}
function shape(value, names) {
  need(value && typeof value === "object" && !Array.isArray(value), "closed object");
  const keys = Reflect.ownKeys(value);
  need(keys.length === names.length && names.every(name => keys.includes(name)), "exact field set " + names.join(","));
  return Object.fromEntries(names.map(name => [name, own(value, name)]));
}
function equal(actual, expected, where) {
  if (expected === null || typeof expected !== "object") { need(actual === expected, "exact " + where); return; }
  if (Array.isArray(expected)) {
    need(Array.isArray(actual) && actual.length === expected.length, "exact array census " + where);
    expected.forEach((value, i) => equal(actual[i], value, where + "/" + i)); return;
  }
  const keys = Object.keys(expected); shape(actual, keys);
  keys.forEach(key => equal(actual[key], expected[key], where + "/" + key));
}
function validateResources(resources) {
  shape(resources, ["trace", "fileBytes", "contextBytes", "metadataBytes", "failureGraphBytes"]);
  const trace = shape(resources.trace, ["nodes", "marks", "tensorRecords", "rationalCharacters", "tensorBytes", "graphBytes", "pairVisits", "slotVisits", "liveRecords"]);
  need(Object.values(trace).every(integer) && [trace.nodes, trace.marks, trace.tensorRecords].every(n => n <= 2147483647) && trace.rationalCharacters <= 16384,
    "positive cross-runtime trace ceilings");
  need([resources.fileBytes, resources.contextBytes, resources.metadataBytes, resources.failureGraphBytes].every(integer), "positive cross-runtime file/context ceilings");
}
function createSourceTemplateValidator(profile, limits, admitConstruction, admitTopology) {
  limits = shape(limits, ["nodes", "arraySlots", "stringCharacters", "maxDepth"]);
  need(Object.values(limits).every(integer) && limits.maxDepth <= 16, "explicit snapshot quotas and depth16");
  limits = Object.freeze({ ...limits }); need(typeof admitConstruction === "function", "mandatory external construction admission");
  need(typeof admitTopology === "function", "mandatory external topology planning admission");
  const used = { nodes: 0, arraySlots: 0, stringCharacters: 0 }, seen = new Set();
  let failed = false, busy = false, finished = false, captureBound = false, topologyChecked = false, constructions = 0, marks = 0, leaves = 0, callbacks = 0;
  function capture(value) {
    const remaining = { ...limits };
    for (const name of Object.keys(used)) { remaining[name] -= used[name]; need(remaining[name] > 0, "remaining snapshot quota " + name); }
    // The shared copier charges before every copied node/slot/string. Passing
    // remaining quotas makes the accounting cumulative across the profile and
    // all candidate templates; a new context cannot reset it.
    const result = snapshotCanonicalMetadata(value, remaining);
    for (const name of Object.keys(used)) used[name] += result.usage[name];
    return result.value;
  }
  profile = capture(profile); shape(profile, ["retention", "budgets", "capture"]);
  shape(profile.capture, ["limits", "inspectionPolicy"]);
  shape(profile.capture.limits, ["normal", "inspection", "failure"]);
  for (const lane of Object.values(profile.capture.limits)) {
    shape(lane, ["graphs", "nodeObjects", "arrays", "slots", "elementCopies"]);
    need(Object.values(lane).every(n => Number.isSafeInteger(n) && n >= 0), "explicit nonnegative safe copy ceilings");
  }
  need(Array.isArray(profile.capture.inspectionPolicy) && profile.capture.inspectionPolicy.length === IDS.length, "all705 inspection policy entries");
  profile.capture.inspectionPolicy.forEach((row, i) => {
    shape(row, ["id", "copies"]);
    need(row.id === IDS[i] && Number.isSafeInteger(row.copies) && row.copies >= 0, "exact ordered finite inspection policy");
  });
  shape(profile.retention, ["background", "geometry", "structured", "secondJets"]);
  need(Object.values(profile.retention).every(v => typeof v === "boolean"), "four explicit boolean retention choices");
  need(Array.isArray(profile.budgets) && profile.budgets.length === IDS.length, "all705 explicit budget records");
  const budgets = new Map();
  for (const budget of profile.budgets) {
    shape(budget, ["id", "resources", "checkpointBytes"]);
    need(ALLOWED.has(budget.id) && !budgets.has(budget.id), "exact unique budget context"); validateResources(budget.resources);
    if (/^point[01]$/.test(budget.id)) need(integer(budget.checkpointBytes) && budget.checkpointBytes <= budget.resources.fileBytes && budget.checkpointBytes <= budget.resources.contextBytes,
      "point checkpoint admission inside file/context ceilings");
    else need(budget.checkpointBytes === null, "no checkpoint budget outside baseline point");
    budgets.set(budget.id, budget);
  }
  function guard(action) {
    if (failed || busy || finished) { failed = true; need(false, "failed, reentrant or completed validator"); }
    busy = true;
    try { const result = action(); need(!failed, "swallowed callback failure"); return result; }
    catch (error) { failed = true; throw error; } finally { busy = false; }
  }
  function expectedTemplate(id, menu, budget) {
    const retention = profile.retention;
    return { schema: "phase627-context-structure-v1", id, graphPath: id + "/graph.json", metadataPath: id + "/metadata.json",
      resources: budget.resources, pointCheckpoint: budget.checkpointBytes === null ? null : { relativePath: id + "/background-checkpoint.json", bytes: budget.checkpointBytes },
      leaves: menu.leaves, leafRoles: menu.leafRoles,
      marks: menu.marks.map((mark, i) => {
        const expanded = mark.requiredExpanded ?? (mark.name.startsWith("structured/") ? retention.structured :
          mark.name.startsWith("secondJet/") ? retention.secondJets : mark.name.startsWith("background/") ? retention.background : retention.geometry);
        return { name: mark.name, degree: mark.degree, expanded, relativePath: expanded ? `${id}/tensors/t${String(i).padStart(6, "0")}.json` : null };
      }), callbacks: { id, callbacks: menu.callbacks } };
  }
  return Object.freeze({
    validateCaptureDeclaration: candidate => guard(() => {
      need(!captureBound && seen.size === 0, "one capture binding before templates");
      const actual = capture(candidate); need(!failed, "swallowed capture snapshot failure");
      equal(actual, profile.capture, "capture declaration");
      // Mandatory BEFORE planning, and therefore before the transport's Begin
      // ACK. The host must admit planner temporaries and independently bind
      // these revisions; merely returning void is NOT source authentication.
      const admitted = admitTopology(Object.freeze({ operation: "prospective-tensor-topology-v1", contexts: IDS.length,
        profile, sourceRevisions: REVIEWED_SOURCE_SHA256 }));
      need(!failed, "topology admission reentry");
      need(admitted === undefined, "synchronous void topology admission required");
      const topology = deriveProspectiveCaptureRequirements(profile.capture.inspectionPolicy);
      need(!failed, "healthy topology planning");
      // Charge the retained report snapshot to the existing cumulative metadata
      // quota. The external hook, not this post-construction snapshot, must
      // account for planner temporary allocations/source-menu construction.
      const checked = capture(topology); need(!failed, "topology snapshot reentry");
      for (const context of checked.contexts) {
        const trace = budgets.get(context.id).resources.trace;
        need(trace.nodes >= context.shape.nodes && trace.marks >= context.shape.marks,
          "prospective topology nodes/marks for " + context.id);
      }
      for (const [lane, requirements] of Object.entries(checked.requirements)) for (const [field, value] of Object.entries(requirements))
        need(profile.capture.limits[lane][field] >= value, "prospective topology capture " + lane + "/" + field);
      topologyChecked = true; captureBound = true;
    }),
    validateTemplate: candidate => guard(() => {
      need(captureBound, "capture declaration must be bound before templates");
      // Snapshot BEFORE any admission callback can mutate caller-owned input.
      const actual = capture(candidate); shape(actual, ["schema", "id", "graphPath", "metadataPath", "resources", "pointCheckpoint", "leaves", "leafRoles", "marks", "callbacks"]);
      need(ALLOWED.has(actual.id) && !seen.has(actual.id), "one declared context before construction");
      const admission = admitConstruction(actual.id);
      need(admission === undefined, "synchronous void construction admission required");
      need(!failed, "construction admission reentry"); constructions++;
      const menu = sourceContextMenu(actual.id), expected = expectedTemplate(actual.id, menu, budgets.get(actual.id));
      shape(actual.callbacks, ["id", "callbacks"]);
      need(Array.isArray(actual.callbacks.callbacks) && actual.callbacks.callbacks.length === menu.callbacks.length, "complete callback census");
      for (const callback of actual.callbacks.callbacks) {
        shape(callback, ["category", "name", "degree", "expanded", "length"]);
        need(typeof callback.category === "string" && typeof callback.name === "string", "named callback census entries");
      }
      // Callback menus are SETS, not execution order. The catalog's separate
      // structural fingerprint still freezes their original wire order.
      const ordered = [...actual.callbacks.callbacks].sort((a, b) => ordinal(a.category, b.category) || ordinal(a.name, b.name));
      equal({ ...actual, callbacks: { id: actual.callbacks.id, callbacks: ordered } }, expected, actual.id);
      need(menu.leaves.length <= expected.resources.trace.nodes && menu.marks.length <= expected.resources.trace.marks,
        "structural declarations fit independently frozen node/mark ceilings");
      seen.add(actual.id); marks += menu.marks.length; leaves += menu.leaves.length; callbacks += menu.callbacks.length;
      return Object.freeze({ contextId: actual.id, marks: menu.marks.length, leaves: menu.leaves.length, callbacks: menu.callbacks.length, scope });
    }),
    finish: () => {
      const result = guard(() => {
        need(captureBound, "capture declaration must be bound before completion");
        need(seen.size === IDS.length && IDS.every(id => seen.has(id)), "all705 contexts required before structural completion");
        need(marks === 291199 && leaves === 20316 && callbacks === 940365, "complete fixed source mark/leaf/callback census");
        return Object.freeze({ contexts: seen.size, marks, leaves, callbacks, scope });
      });
      finished = true; return result;
    },
    snapshot: () => Object.freeze({ contexts: seen.size, constructions, captureBound, topologyChecked, failed, finished, metadata: Object.freeze({ ...used }), scope })
  });
}
module.exports = { createSourceTemplateValidator };

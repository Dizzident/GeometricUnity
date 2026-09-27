"use strict";
// Explicit TEST-ONLY values covering the prospective tensor topology envelope;
// NOT scientifically sufficient caps, memory feasibility or production defaults.
// Source menus contain declarations only, with no coefficient/source hashes.
const { sourceContextIds, sourceContextMenu } = require("../a68-source-context-menu");
const clone = value => JSON.parse(JSON.stringify(value));
const limits = Object.freeze({ nodes: 30000000, arraySlots: 30000000, stringCharacters: 500000000, maxDepth: 16 });
function profile() {
  const resources = { trace: { nodes: 3000000, marks: 5000, tensorRecords: 100000, rationalCharacters: 128,
    tensorBytes: 100000000, graphBytes: 100000000, pairVisits: 100000000, slotVisits: 100000000, liveRecords: 100000000 },
  fileBytes: 100000000, contextBytes: 1000000000, metadataBytes: 100000000, failureGraphBytes: 100000000 };
  return { retention: { background: false, geometry: false, structured: false, secondJets: false },
    budgets: sourceContextIds().map(id => ({ id, resources: clone(resources), checkpointBytes: /^point[01]$/.test(id) ? 100000000 : null })),
    capture: { limits: {
      normal: { graphs: 705, nodeObjects: 422622614, arrays: 422624729, slots: 999971128, elementCopies: 999971128 },
      inspection: { graphs: 0, nodeObjects: 0, arrays: 0, slots: 0, elementCopies: 0 },
      failure: { graphs: 2, nodeObjects: 2399809, arrays: 2399812, slots: 5681242, elementCopies: 5681242 } },
    inspectionPolicy: sourceContextIds().map(id => ({ id, copies: 0 })) } };
}
function template(id, frozen) {
  const menu = sourceContextMenu(id), budget = frozen.budgets.find(b => b.id === id), policy = frozen.retention;
  const marks = menu.marks.map((mark, i) => {
    let expanded = mark.requiredExpanded;
    if (expanded === null) {
      if (mark.name.startsWith("background/")) expanded = policy.background;
      else if (mark.name.startsWith("structured/")) expanded = policy.structured;
      else if (mark.name.startsWith("secondJet/")) expanded = policy.secondJets;
      else expanded = policy.geometry;
    }
    return { name: mark.name, degree: mark.degree, expanded, relativePath: expanded ? id + "/tensors/t" + String(i).padStart(6, "0") + ".json" : null };
  });
  return clone({ schema: "phase627-context-structure-v1", id, graphPath: id + "/graph.json", metadataPath: id + "/metadata.json", resources: budget.resources,
    pointCheckpoint: budget.checkpointBytes === null ? null : { relativePath: id + "/background-checkpoint.json", bytes: budget.checkpointBytes },
    leaves: menu.leaves, leafRoles: menu.leafRoles, marks, callbacks: { id, callbacks: menu.callbacks } });
}
function checkpointPolicy() {
  // Routing fixtures only: replay sentinel is intentionally NOT executable
  // scientific admission. No file reader or numerical replay uses these caps.
  return { outputRoot: "/tmp/a68-manufactured-configured-output",
    profiles: [{ read: { graphBytes: 100000000, metadataBytes: 100000000, totalReadBytes: 200000002, snapshot: { ...limits } }, replay: { manufactured: true } },
      { read: { backgroundBytes: 100000000, metadataBytes: 100000000, totalReadBytes: 200000002, snapshot: { ...limits } }, replay: null }],
    routes: sourceContextIds().map(contextId => ({ contextId, profile: 0, completionProfile: /^point[01]$/.test(contextId) ? 1 : null })) };
}
function preparationPolicy(repositoryRoot) {
  // Constructor/schema fixture only; these deliberately tiny arithmetic caps
  // are NOT admitted for genuine source construction and must never run it.
  const algebra = { maxBits: 16, scalarOperations: 1, rationalObjects: 1, matrixObjects: 1, matrixCells: 1, slotVisits: 1 };
  const binding = { coordinateVisits: 1, sparseRecords: 1, tensorRecords: 1, rationalCharacters: 1 };
  const adapter = { leaf: { leaves: 1, records: 1, stringCharacters: 1, wireBytes: 1, rationalCharacters: 1, resolutions: 1 },
    export: { matrixSlots: 1, coordinateVisits: 1, entryRecords: 1 }, metadata: limits, comparison: binding };
  const recipe = { nodeLimit: 1, markLimit: 1 };
  return { repositoryRoot, profiles: [
    { kind: "point", algebra, binding, polynomialArithmetic: algebra,
      polynomial: { inputRecords: 1, outputRecords: 1, recordVisits: 1, readCharacters: 1, outputCharacters: 1, rationalCharacters: 1 }, adapter, recipe },
    { kind: "germ", binding, adapter: { ...adapter, fields: { fields: 1, coordinateVisits: 1, arrayObjects: 1, arraySlots: 1, formatCharacters: 1, rationalCharacters: 1 } }, recipe },
    { kind: "diagnostic", algebra, binding, adapter, recipe }, { kind: "diagnostic", algebra: null, binding: null, adapter, recipe }],
    routes: sourceContextIds().map(contextId => ({ contextId, profile: /^point[01]$/.test(contextId) ? 0 : contextId.startsWith("point") ? 1 : contextId.endsWith("secondJets") ? 3 : 2 })) };
}
module.exports = { clone, limits, profile, template, checkpointPolicy, preparationPolicy };

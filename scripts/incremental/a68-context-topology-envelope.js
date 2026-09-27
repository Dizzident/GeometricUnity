"use strict";

// SOURCE-EXPRESSION tensor topology envelope, not a coefficient calculation.
// Fixed ledgers were derived by expanding the primitive macros in the reviewed
// JS/C# sources; branch maxima follow finite index domains, NOT a dense fixture.
// Charge every FreshZero once at its CREATION site, even if unconsumed. This
// bounds later Mark/operand materialization without double charging callers.
// The source commitments below require external binding to compiler inputs;
// this pure arithmetic module performs no I/O and does not authenticate them.
const { prospectiveBranchTopologyEnvelope } = require("./a68-branch-topology-envelope");
const { sourceContextIds, sourceContextMenu } = require("./a68-source-context-menu");
const { wardControlsMenu } = require("./a68-ward-controls-recipe");
const freeze = x => { if (x && typeof x === "object") { Object.values(x).forEach(freeze); Object.freeze(x); } return x; };
const need = (ok, why) => { if (!ok) throw new Error("A68 context topology: " + why); };
const natural = n => Number.isSafeInteger(n) && n >= 0;
const add = (a, b) => { need(natural(a) && natural(b) && b <= Number.MAX_SAFE_INTEGER - a, "safe count sum"); return a + b; };
const multiply = (a, b) => { need(natural(a) && natural(b) && (a === 0 || b <= Math.floor(Number.MAX_SAFE_INTEGER / a)), "safe count product"); return a * b; };
const ids = sourceContextIds(), allowed = new Set(ids);
const ward = new Set(wardControlsMenu().contexts.map(c => `point${c.point}/m${c.metricBasis}_j${c.jetIndex}`));
const branch = prospectiveBranchTopologyEnvelope().blocks;

// (ordinary nodes, ordered edges, owned lazy handles), excluding Phi cache.
// Background/germ include1/2 geometry leaves; germWrapper owns27 imports.
const FIXED = freeze({
  background: [793, 1264, 3], germ: [217, 8, 2], variation: [4788, 6117, 853],
  acceleration: [942, 625, 43], original: [511, 801, 20],
  wardUncompensated: [1393, 2417, 3], wardCompensated: [1400, 2431, 1],
  pointWrapper: [1, 0, 0], germWrapper: [980, 1356, 16],
  germAnchorMaximum: [4, 3, 0], wardWrapper: [23, 28, 0],
  grade10Wrapper: [1879, 2183, 91], accelerationWrapper: [20, 20, 1],
  secondJets: [3502, 3864, 0], phiCache: [107, 105, 0]
});
// Transparent fixed recurrences; sums equal FIXED entries (before zero handles).
const FIXED_LEDGERS = freeze({
  background: [[1,1],[168,210],[210,392],[56,70],[3,5],[1,0],[1,2],[10,14],[27,39],[70,84],[210,392],[29,42],[7,13]],
  germ: [[210,0],[2,4],[2,0],[3,4]],
  variation: [[9,14],[183,258],[104,148],[9,15],[16,30],[43,64],[99,148],[1176,1652],[4,7],[242,341],[10,10],[284,288],[1624,1764],[980,1372],[5,6]],
  acceleration: [[8,14],[392,0],[56,98],[56,70],[3,5],[3,6],[18,26],[406,406]],
  grade10Wrapper: [[5,3],[4*382,4*412],[4*68,4*102],[8,32],[66,92]],
  accelerationWrapper: [[2,0],[2*5,2*7],[2*4,2*3]]
});
const SCOPE = freeze({
  completeRecipeTensorTopologyEnvelope: true, denseFixtureExtremumAssumed: false,
  sourceRevisionBindingEstablished: false, coefficientGeometryEvaluated: false,
  completeRuntimeResourceProof: false, productionProfileAdmitted: false,
  scientificExecutionAuthorized: false
});

function component(name) {
  const [fixedNodes, fixedEdges, ownedLazyZeros] = FIXED[name];
  const branches = name === "background" ? [branch.backgroundBAndDX]
    : name === "germ" ? [branch.germDeltaBAndExterior]
    : name === "variation" ? [branch.movingEuler, branch.nativeCurrent]
    : name === "acceleration" ? [branch.accelerationPartialB, branch.accelerationCovariantWY] : [];
  // Do NOT add branch.lazyZeroNodes: all zeros already belong to FIXED.Z.
  return { nodes: fixedNodes + ownedLazyZeros + branches.reduce((s, b) => s + b.bodyNodes, 0),
    inputReferences: fixedEdges + branches.reduce((s, b) => s + b.inputReferences, 0) };
}
function deriveProspectiveContextTopology(id) {
  need(typeof id === "string" && allowed.has(id), "exact declared context ID");
  const composition = [];
  const use = (name, times = 1) => composition.push({ name, times, ...component(name) });
  if (/^point[01]$/.test(id)) { use("pointWrapper"); use("background"); }
  else if (id.startsWith("point")) {
    use("germWrapper"); use("germAnchorMaximum"); use("germ"); use("variation");
    if (ward.has(id)) {
      use("wardWrapper"); use("wardUncompensated", 4); use("wardCompensated", 4);
      use("acceleration", 4); use("original", 4);
    }
  } else if (id === "diagnostic/grade10") {
    use("grade10Wrapper"); use("background"); use("germ", 4); use("variation", 4); use("original", 8);
  } else if (id === "diagnostic/acceleration") {
    use("accelerationWrapper"); use("background"); use("germ"); use("wardUncompensated", 2);
    use("wardCompensated", 2); use("acceleration", 2); use("original", 2);
  } else use("secondJets");
  if (id !== "diagnostic/secondJets") use("phiCache");
  const menu = sourceContextMenu(id);
  const shape = { leaves: menu.leaves.length, nodes: composition.reduce((s, c) => add(s, multiply(c.nodes, c.times)), 0),
    marks: menu.marks.length, inputReferences: composition.reduce((s, c) => add(s, multiply(c.inputReferences, c.times)), 0) };
  const slots = add(add(shape.leaves, shape.nodes), add(shape.marks, shape.inputReferences));
  const copies = { graphs: 1, nodeObjects: shape.nodes, arrays: add(shape.nodes, 3), slots, elementCopies: slots };
  return freeze({ schemaVersion: "phase627-prospective-context-topology-v1", id, composition, shape, copies, scope: SCOPE });
}

const COPY_FIELDS = ["graphs", "nodeObjects", "arrays", "slots", "elementCopies"];
const zero = () => Object.fromEntries(COPY_FIELDS.map(k => [k, 0]));
const plus = (a, b) => Object.fromEntries(COPY_FIELDS.map(k => [k, add(a[k], b[k])]));
function data(value, key) {
  const d = value && Object.getOwnPropertyDescriptor(value, key);
  need(d && Object.hasOwn(d, "value") && d.enumerable, "own data " + key); return d.value;
}
function deriveProspectiveCaptureRequirements(inspectionPolicy) {
  need(Array.isArray(inspectionPolicy) && inspectionPolicy.length === ids.length &&
    Reflect.ownKeys(inspectionPolicy).length === ids.length + 1, "dense all705 inspection policy");
  // Snapshot the complete policy BEFORE constructing any context report.
  const policy = ids.map((id, i) => {
    const row = data(inspectionPolicy, String(i));
    need(row && typeof row === "object" && !Array.isArray(row) && Reflect.ownKeys(row).length === 2, "closed policy row");
    const actual = data(row, "id"), copies = data(row, "copies");
    need(actual === id && natural(copies), "ordered context/finite inspection count"); return { id, copies };
  });
  let normal = zero(), inspection = zero(), failure = zero(), point = null;
  const contexts = policy.map(row => {
    const context = deriveProspectiveContextTopology(row.id), cost = context.copies;
    normal = plus(normal, cost);
    inspection = plus(inspection, Object.fromEntries(COPY_FIELDS.map(k => [k, multiply(cost[k], row.copies)])));
    let terminal = cost;
    if (/^point[01]$/.test(row.id)) point = cost;
    else if (row.id.startsWith("point")) { need(point !== null, "point before child"); terminal = plus(point, cost); }
    else point = null;
    failure = Object.fromEntries(COPY_FIELDS.map(k => [k, Math.max(failure[k], terminal[k])]));
    return context;
  });
  return freeze({ schemaVersion: "phase627-prospective-capture-requirements-v1", contexts, inspectionPolicy: policy,
    requirements: { normal, inspection, failure }, scope: SCOPE });
}

// Exact reviewed revisions, not self-authenticating declarations. Tests bind
// these to files; production must bind them to admitted compiler/replay inputs.
const REVIEWED_SOURCE_SHA256 = freeze(Object.fromEntries([
  ["scripts/incremental/", {
    "a68-mixed-recipe.js": "aeb95711a55e36065ea1d2a33acf1d78fd1c2495567f9e40f8b053fbd5f05990",
    "a68-background-recipe.js": "f8a98b1e570b7f0bfdca91431d4fd9ce4aef713459f26bc902eb7204fa24d582",
    "a68-variation-recipe.js": "54197463ba1b3892c124f97ab1a0a0ab66a284d35db35313f97d182954ea8a71",
    "a68-acceleration-recipe.js": "095f87f705d7ee10e6017aaca83e624285524fcde3f3b08c17820bffffe4a39b",
    "a68-original-action-recipe.js": "b74292bed180dae241d5d8f444413d48851a82a04e57b6f0bc86b4362782fc5d",
    "a68-ward-recipe.js": "9ffd45a66311e2e1b7e73c295662030d42b69638e7701639565d7a7b2a321aaf",
    "a68-audit-context-recipe.js": "948a79fb9a313901860a41662516e0ac0e28ea68d45b180da3f18fd7e260b5d2",
    "a68-ward-controls-recipe.js": "542789fa8f6a5efc8f7869c3898ced26a70b6491ef1b36a4c69d574345bfa5e6",
    "a68-diagnostics-recipe.js": "b7386980ed2404bdcb86467cb14219cabafb842f1bedd1d7df1fdda901b233fe",
    "a68-second-jet-recipe.js": "9dcc41f4d1f461615a1fc58f0bd539d086065080f952a00a345517697b947505",
    "a68-source-context-menu.js": "0e13bc152cdd0cbc45ea83237341591a5e9ed4240c54f4c95aec0f6742e78feb",
    "a68-branch-topology-envelope.js": "7a21a1e5b2fe8b4f70b000afeaf142424c7c4fd079d8259bb2118650020a0ffd"
  }],
  ["studies/phase627_full_mixed_metric_native_field_variation_audit_001/", {
    "MixedAlgebra.cs": "b1616388550b70d31f8537adae97747e40ce312f471b0e7fa8c0bc982e1bcc1c",
    "MixedGeometry.cs": "c39c41d2a92ffcc92fa662c3ae7d7c575ff52c595f3e7d4374f0ee23e45ae2b3",
    "MixedVariation.cs": "70955f329235d407d45a9c3e71a11c4180072abf6e6eba8e0b04e88477173f64",
    "MixedWard.cs": "eab0fc52b60b7f236e0550e473a261e7ae8caa906b4779d0b29f6a3b21686371",
    "OriginalMixedAction.cs": "d82dff40a65371e2ce4cfe6a288e48e6423dd44d6edc54066f2cdde5ed395cdd",
    "MixedAudit.cs": "e73a92e849f49f8cfaf1b15294818ea8733d6d4cb74b80f83bed6bf8d99ca497",
    "MixedDiagnostics.cs": "e4996ff748b53dce1af4b9f5c9a7489516cadd7294bef1512080ef5b0bbf1091",
    "MixedWardControls.cs": "9fc23e46aeee7e9e86c0756eda1ee3062aae1183edc27ef808c95baa2c6d25c7",
    "MixedEvidence.cs": "30c8c26b79f90d4e8e6af5f44aaaf094af88242be3bfad34988dd15f36ff3528",
    "MixedAuditSink.cs": "942bc0f6b3ff22b86db3da4ccd996f00317b8d2a8548b1ec566f1967b0b6926f",
    "MixedStructuredFields.cs": "6dc87bc35bab0ba0815a01110627f908f7be7f937ee11b21f131fa50bacb51c3"
  }]
].flatMap(([prefix, files]) => Object.entries(files).map(([name, hash]) => [prefix + name, hash]))));

module.exports = { FIXED, FIXED_LEDGERS, REVIEWED_SOURCE_SHA256, deriveProspectiveContextTopology, deriveProspectiveCaptureRequirements };

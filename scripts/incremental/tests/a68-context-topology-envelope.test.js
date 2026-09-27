"use strict";
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), crypto = require("node:crypto");
const { FIXED, FIXED_LEDGERS, REVIEWED_SOURCE_SHA256, deriveProspectiveContextTopology, deriveProspectiveCaptureRequirements } = require("../a68-context-topology-envelope");
const { sourceContextIds } = require("../a68-source-context-menu");
const policy = copies => sourceContextIds().map(id => ({ id, copies }));

test("static fixed ledgers and reviewed JS/C# source commitments cannot silently drift", () => {
  for (const [name, terms] of Object.entries(FIXED_LEDGERS)) {
    assert.equal(terms.reduce((s, t) => s + t[0], 0), FIXED[name][0], name + " nodes");
    assert.equal(terms.reduce((s, t) => s + t[1], 0), FIXED[name][1], name + " edges");
  }
  assert.equal(Object.keys(REVIEWED_SOURCE_SHA256).length, 23);
  for (const [file, hash] of Object.entries(REVIEWED_SOURCE_SHA256)) {
    const bytes = fs.readFileSync(path.resolve(__dirname, "../../..", file));
    assert.equal(crypto.createHash("sha256").update(bytes).digest("hex"), hash, "topology requires re-audit: " + file);
  }
});

test("whole-context composition covers all705 IDs including Ward tails and three distinct diagnostics", () => {
  const report = deriveProspectiveCaptureRequirements(policy(0)), totals = {};
  assert.equal(report.contexts.length, 705);
  for (const c of report.contexts) for (const p of c.composition) totals[p.name] = (totals[p.name] ?? 0) + p.times;
  assert.deepEqual(totals, { pointWrapper: 2, background: 4, phiCache: 704,
    germWrapper: 700, germAnchorMaximum: 700, germ: 705, variation: 704,
    wardWrapper: 6, wardUncompensated: 26, wardCompensated: 26, acceleration: 26, original: 34,
    grade10Wrapper: 1, accelerationWrapper: 1, secondJets: 1 });
  assert.deepEqual(report.requirements.normal, { graphs: 705, nodeObjects: 422622614, arrays: 422624729, slots: 999971128, elementCopies: 999971128 });
  assert.deepEqual(report.requirements.inspection, { graphs: 0, nodeObjects: 0, arrays: 0, slots: 0, elementCopies: 0 });
  assert.equal(report.scope.completeRecipeTensorTopologyEnvelope, true);
  for (const key of ["sourceRevisionBindingEstablished", "coefficientGeometryEvaluated", "completeRuntimeResourceProof", "productionProfileAdmitted", "scientificExecutionAuthorized"])
    assert.equal(report.scope[key], false);
  assert.ok(Object.isFrozen(report) && Object.isFrozen(report.contexts[0].shape));
});

test("failure allowance is componentwise maximum of suspended-point/child or lone diagnostic", () => {
  const report = deriveProspectiveCaptureRequirements(policy(0)), byId = new Map(report.contexts.map(c => [c.id, c]));
  const expected = { graphs: 0, nodeObjects: 0, arrays: 0, slots: 0, elementCopies: 0 };
  for (const c of report.contexts) for (const key of Object.keys(expected)) {
    const point = c.id.includes("/") && c.id.startsWith("point") ? byId.get(c.id.slice(0, 6)).copies[key] : 0;
    expected[key] = Math.max(expected[key], c.copies[key] + point);
  }
  assert.deepEqual(report.requirements.failure, expected);
  assert.equal(expected.graphs, 2);
  assert.equal(expected.nodeObjects, byId.get("diagnostic/grade10").shape.nodes);
  assert.equal(expected.nodeObjects, 2399809);
});

test("per-context inspection counts are explicit detached and cumulative", () => {
  const input = policy(0); input[0].copies = 2; input[1].copies = 3;
  const report = deriveProspectiveCaptureRequirements(input); input[0].copies = 100;
  assert.equal(report.inspectionPolicy[0].copies, 2);
  for (const key of Object.keys(report.requirements.inspection))
    assert.equal(report.requirements.inspection[key], 2 * report.contexts[0].copies[key] + 3 * report.contexts[1].copies[key]);
});

test("missing reordered accessor negative and unsafe inspection declarations fail closed", () => {
  for (const input of [null, [], policy(0).slice(1)]) assert.throws(() => deriveProspectiveCaptureRequirements(input), /policy/);
  const reordered = policy(0); [reordered[0], reordered[1]] = [reordered[1], reordered[0]];
  assert.throws(() => deriveProspectiveCaptureRequirements(reordered), /ordered/);
  for (const copies of [-1, 0.5, Infinity, "1", Number.MAX_SAFE_INTEGER + 1]) {
    const bad = policy(0); bad[0].copies = copies; assert.throws(() => deriveProspectiveCaptureRequirements(bad), /finite/);
  }
  const overflow = policy(0); overflow[0].copies = Number.MAX_SAFE_INTEGER;
  assert.throws(() => deriveProspectiveCaptureRequirements(overflow), /safe count/);
  let reads = 0; const getter = policy(0);
  Object.defineProperty(getter[0], "copies", { get() { reads++; return 0; }, enumerable: true });
  assert.throws(() => deriveProspectiveCaptureRequirements(getter), /own data/); assert.equal(reads, 0);
  const extra = policy(0); extra[0].extra = 0; assert.throws(() => deriveProspectiveCaptureRequirements(extra), /closed/);
  for (const id of ["point2", "point0/m00_j0", "point0/m0_j35", "diagnostic/missing", null])
    assert.throws(() => deriveProspectiveContextTopology(id), /declared context/);
});

test("secondJets bound is its full fixed graph, not a geometry-branch placeholder", () => {
  const { MixedRecipe } = require("../a68-mixed-recipe");
  const { buildSecondJetControlsRecipe, secondJetMarkMenu } = require("../a68-second-jet-recipe");
  const recipe = new MixedRecipe();
  buildSecondJetControlsRecipe(recipe, { retention: Object.fromEntries(secondJetMarkMenu().map(m => [m.name, false])) });
  const plan = recipe.Finish(), bound = deriveProspectiveContextTopology("diagnostic/secondJets");
  assert.deepEqual(bound.shape, { leaves: 0, nodes: plan.nodes.length, marks: plan.marks.length,
    inputReferences: plan.nodes.reduce((s, n) => s + n.inputs.length, 0) });
  assert.deepEqual(bound.shape, { leaves: 0, nodes: 3502, marks: 420, inputReferences: 3864 });
});

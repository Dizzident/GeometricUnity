"use strict";
// Manufactured symbolic plans only. The geometry validators here check fixture
// metadata, NOT source provenance; these tests grant no scientific permission.
const test = require("node:test"), assert = require("node:assert/strict");
const { EVENT_LIMITS, deriveBranchTopologyContributions, prospectiveBranchTopologyEnvelope } = require("../a68-branch-topology-envelope");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { GERM_SCHEMA, WedgeCoordinate, buildMixedMetricGerm } = require("../a68-background-recipe");
const zeroCounts = () => Object.fromEntries(Object.keys(EVENT_LIMITS).map(k => [k, 0]));
const counts = frameNonzeros => ({ ...zeroCounts(), frameNonzeros });
const leaf = (id, degree) => ({ id, degree, source: "synthetic/" + id, sha256: "0".repeat(64) });
const entries = n => Array.from({ length: n }, (_, i) => ({ row: Math.floor(i / 14), column: i % 14, value: "1" }));
const edges = plan => plan.nodes.reduce((sum, node) => sum + node.inputs.length, 0);

test("fixed-domain upper contributions remain explicitly partial and immutable", () => {
  const report = prospectiveBranchTopologyEnvelope(), b = report.blocks;
  assert.deepEqual(report.events, EVENT_LIMITS);
  assert.deepEqual(b.wedgeCoordinate, { bodyNodes: 784, inputReferences: 980, lazyZeroNodes: 1, nodeUpperBound: 785 });
  assert.equal(b.germDeltaBAndExterior.bodyNodes, 231280);
  assert.equal(b.germDeltaBAndExterior.inputReferences, 269892);
  assert.equal(b.nativeCurrent.bodyNodes, 356720);
  assert.equal(b.nativeCurrent.inputReferences, 535080);
  assert.equal(b.accelerationPartialB.bodyNodes, 21952);
  assert.equal(b.accelerationCovariantWY.bodyNodes, 2940);
  for (const value of [report, report.events, report.blocks, ...Object.values(b), report.scope]) assert.ok(Object.isFrozen(value));
  assert.equal(report.scope.completeContextEnvelope, false);
  assert.equal(report.scope.sourceCountsAuthenticated, false);
  assert.equal(report.scope.productionCaptureSufficiency, false);
});

test("count declarations reject malformed, unbounded and accessor inputs without reading getters", () => {
  for (const key of Object.keys(EVENT_LIMITS)) {
    for (const value of [-1, 0.5, NaN, Infinity, "1", EVENT_LIMITS[key] + 1])
      assert.throws(() => deriveBranchTopologyContributions({ ...zeroCounts(), [key]: value }), /bounded count/);
    const missing = zeroCounts(); delete missing[key];
    assert.throws(() => deriveBranchTopologyContributions(missing), /exact count fields/);
  }
  let reads = 0; const getter = zeroCounts();
  Object.defineProperty(getter, "frameNonzeros", { get() { reads++; return 1; } });
  assert.throws(() => deriveBranchTopologyContributions(getter), /own data/); assert.equal(reads, 0);
  for (const value of [null, [], { ...zeroCounts(), extra: 1 }, { ...zeroCounts(), [Symbol("hidden")]: 0 }])
    assert.throws(() => deriveBranchTopologyContributions(value), /record|fields/);
  const original = counts(1), report = deriveBranchTopologyContributions(original); original.frameNonzeros = 196;
  assert.equal(report.events.frameNonzeros, 1);
});

test("every frame support cardinality0..196 matches the real wedge recipe including lazy seed", () => {
  for (let f = 0; f <= 196; f++) {
    const recipe = new MixedRecipe({ leaves: [leaf("d", 1)], matrices: [{ name: "frame", matrix: entries(f) }] });
    const derivative = recipe.RegisterLeaf("d");
    const value = WedgeCoordinate(recipe, Array(14).fill(derivative), recipe.Matrix("frame"));
    recipe.Mark("result", 2, value, false); // Forces the zero seed even at f=0.
    const plan = recipe.Finish(), expected = deriveBranchTopologyContributions(counts(f)).blocks.wedgeCoordinate;
    assert.equal(plan.nodes.length, 1 + expected.nodeUpperBound, "nodes at support " + f);
    assert.equal(edges(plan), expected.inputReferences, "edges at support " + f);
    assert.equal(plan.nodes.filter(n => n.op === "zero").length, 1);
  }
});

test("actual germ recipe retains the square ordered-pair count, even numerically zero wedges", () => {
  for (const f of [0, 1, 2, 14, 15, 32, 196]) {
    const recipe = new MixedRecipe({ leaves: [leaf("B", 1), leaf("F", 2), leaf("adapted", 2), leaf("oracle", 2)],
      matrices: [{ name: "frame", matrix: entries(f) }, { name: "zero", matrix: [] }] });
    const background = { Point: 0, Frame: recipe.Matrix("frame"), B: recipe.RegisterLeaf("B"), F: recipe.RegisterLeaf("F") };
    const fourteen = () => Array(14).fill("zero");
    const geometry = { schemaVersion: GERM_SCHEMA, point: 0, metricBasis: 0, jetIndex: 0,
      provenance: [{ id: "manufactured", source: "synthetic/not-source-proof", sha256: "0".repeat(64) }],
      matrices: { Motion: "zero", DeltaFrame: "zero", MotionPartial: fourteen(), MotionCovariant: fourteen(), DeltaOmega: fourteen(), DeltaOmegaPartial: Array.from({ length: 14 }, fourteen) } };
    buildMixedMetricGerm(recipe, background, { geometry, adaptedLeaf: "adapted", oracleLeaf: "oracle",
      validateGeometry(actual, values) { assert.deepEqual(actual, geometry); assert.deepEqual(values, { zero: [] }); return true; } });
    const plan = recipe.Finish(), expected = deriveBranchTopologyContributions(counts(f)).blocks.germDeltaBAndExterior;
    // Four registered leaves,210 unconditional spins,5 fixed tail nodes/8edges.
    assert.equal(plan.nodes.length, 219 + expected.nodeUpperBound, "nodes at support " + f);
    assert.equal(edges(plan), 8 + expected.inputReferences, "edges at support " + f);
    assert.equal(plan.nodes.filter(n => n.op === "spin").length, 210);
    assert.equal(plan.nodes.filter(n => n.op === "unit").length, f + 2 * f * f);
  }
});

test("independent native predicates and lazy seeds are not collapsed into frame support", () => {
  const report = deriveBranchTopologyContributions({ ...zeroCounts(), nativeChangedEvents: 1, nativeMetricEvents: 1 });
  assert.deepEqual(report.blocks.nativeCurrent, { bodyNodes: 4, inputReferences: 6, lazyZeroNodes: 588, nodeUpperBound: 592 });
  assert.equal(report.blocks.movingEuler.lazyZeroNodes, 14);
  assert.equal(report.blocks.accelerationPartialB.lazyZeroNodes, 14);
  assert.equal(report.blocks.accelerationCovariantWY.lazyZeroNodes, 28);
});

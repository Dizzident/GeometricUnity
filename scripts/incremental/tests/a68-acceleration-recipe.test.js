"use strict";
// Hand-counted synthetic metadata fixtures; no Ward scalar/tensor values run.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { SCALAR_SCHEMA, buildMixedWardAcceleration } = require("../a68-acceleration-recipe");
const hash = "0".repeat(64), all14 = value => Array(14).fill(value);

function fixture({ oracle = false, framePartial = false, partialLabels = false, frameEntries = null } = {}) {
  const roles = [["eta", 0], ["nativeDeta", 1], ["DeltaB", 1], ["DeltaBExterior", 2], ["X", 1], ["B", 1], ["F", 2], ["DX", 2], ["Q", 2], ["AdjointX", 2], ["K0", 1], ["K1", 1], ["K2", 1], ["G1", 1], ["G2", 1], ["G3", 1]];
  const matrices = [
    { name: "identity", matrix: Array.from({ length: 14 }, (_, a) => ({ row: a, column: a, value: "1" })) },
    { name: "zero", matrix: [] }, { name: "partial", matrix: [{ row: 0, column: 0, value: "2/3" }] }
  ];
  if (frameEntries) matrices.push({ name: "frame", matrix: frameEntries });
  const partialNames = Array.from({ length: 14 }, () => Array(14));
  for (let nu = 0; nu < 14; nu++) for (let mu = 0; mu < 14; mu++) {
    const name = partialLabels ? "d" + nu + "_" + mu : "zero"; partialNames[nu][mu] = name;
    if (partialLabels) matrices.push({ name, matrix: [{ row: 7, column: 0, value: String(1 + 14 * nu + mu) }] });
  }
  const recipe = new MixedRecipe({ leaves: roles.map(([id, degree]) => ({ id, degree, source: "synthetic/" + id, sha256: hash })), matrices });
  const tensors = Object.fromEntries(roles.map(([id]) => [id, recipe.RegisterLeaf(id)]));
  const b = { X: tensors.X, B: tensors.B, F: tensors.F, DX: tensors.DX, Q: tensors.Q, AdjointX: tensors.AdjointX,
    KInputs: [tensors.K0, tensors.K1, tensors.K2], GradientPieces: [tensors.K0, tensors.G1, tensors.G2, tensors.G3],
    Frame: recipe.Matrix(frameEntries ? "frame" : "identity"), Identity: recipe.Matrix("identity"), FramePartial: all14(recipe.Matrix("zero")), Omega: all14(recipe.Matrix("zero")) };
  if (framePartial) b.FramePartial[0] = recipe.Matrix("partial");
  const g = { DeltaB: tensors.DeltaB, DeltaBExterior: tensors.DeltaBExterior, DeltaOmega: all14(recipe.Matrix("zero")), DeltaOmegaPartial: partialNames.map(row => row.map(name => recipe.Matrix(name))) };
  const result = buildMixedWardAcceleration(recipe, b, g, { eta: tensors.eta, nativeDeta: tensors.nativeDeta, oracle });
  const ids = Object.fromEntries(["W", "Dw", "DBw", "DwFromJet", "DBwFromJet", "CrossQ"].map(key => [key, recipe.RecordedNode(result[key])]));
  return { recipe, b, g, result, ids, tensors };
}

test("acceleration metadata follows the hand-enumerated initial derivative and native-jet order", () => {
  const { recipe, result, ids } = fixture(), plan = recipe.Finish();
  assert.equal(plan.nodes.length, 2102);
  assert.deepEqual(plan.nodes.slice(16, 31).map(n => [n.op, n.inputs]), [
    ["pullback", [1]], ["product", [2, 0]], ["product", [3, 0]], ["product", [2, 16]], ["scale", [19]], ["sum", [18, 20]],
    ["product", [5, 17]], ["sum", [21, 22]], ["spin", []], ["spin", []], ["unit", []], ["product", [26, 25]],
    ["scale", [27]], ["zero", []], ["sum", [29, 28]]
  ]);
  assert.deepEqual(plan.nodes.slice(109, 113).map(n => [n.op, n.inputs]), [["product", [108, 0]], ["component", [1]], ["product", [2, 110]], ["sum", [109, 111]]]);
  assert.deepEqual(ids, { W: 17, Dw: 21, DBw: 23, DwFromJet: 1567, DBwFromJet: 1564, CrossQ: 1570 });
  assert.equal(result.Stages.length, 2); assert.ok(result.Stages.every(s => s.length === 8));
});

test("acceleration retains all14 native/covariant arrays and all196 delta-connection partials", () => {
  const { recipe, result } = fixture({ partialLabels: true });
  for (const field of ["PartialW", "CovariantW", "DeltaBPartialInFrame", "CovariantAdjointX"]) assert.equal(result[field].length, 14);
  assert.deepEqual(result.PartialW.map(t => recipe.RecordedNode(t)), Array.from({ length: 14 }, (_, nu) => 112 + 89 * nu));
  assert.deepEqual(result.CovariantW.map(t => recipe.RecordedNode(t)), Array.from({ length: 14 }, (_, a) => 1278 + 17 * a));
  assert.deepEqual(result.CovariantAdjointX.map(t => recipe.RecordedNode(t)), Array.from({ length: 14 }, (_, a) => 1286 + 17 * a));
  const plan = recipe.Finish(), spin = plan.nodes.filter(n => n.op === "spin"); assert.equal(spin.length, 420);
  assert.deepEqual(spin.slice(0, 392).filter((_, i) => i % 2 === 1).map(n => n.parameters.matrix[0].value), Array.from({ length: 196 }, (_, i) => String(i + 1)));
});

test("frame-partial contribution occurs before changing-spin contribution and keeps its rational coefficient", () => {
  const { recipe } = fixture({ framePartial: true }), plan = recipe.Finish();
  assert.equal(plan.nodes.length, 2106);
  assert.deepEqual(plan.nodes.slice(24, 35).map(n => [n.op, n.inputs]), [
    ["spin", []], ["spin", []], ["unit", []], ["product", [26, 24]], ["scale", [27]], ["zero", []], ["sum", [29, 28]],
    ["unit", []], ["product", [31, 25]], ["scale", [32]], ["sum", [30, 33]]
  ]);
  assert.deepEqual(plan.nodes[28].parameters, { real: "2/3", imaginary: "0" });
  assert.deepEqual(plan.nodes[33].parameters, { real: "1", imaginary: "0" });
});

test("acceleration partial branch adds four nodes and five ordered edges per extra entry", () => {
  const { EVENT_LIMITS, deriveBranchTopologyContributions } = require("../a68-branch-topology-envelope");
  const plain = fixture().recipe.Finish(), partial = fixture({ framePartial: true }).recipe.Finish();
  const counts = Object.fromEntries(Object.keys(EVENT_LIMITS).map(k => [k, 0])); counts.frameNonzeros = 14;
  const base = deriveBranchTopologyContributions(counts).blocks.accelerationPartialB;
  counts.framePartialNonzeros = 1;
  const changed = deriveBranchTopologyContributions(counts).blocks.accelerationPartialB;
  const edges = plan => plan.nodes.reduce((sum, n) => sum + n.inputs.length, 0);
  assert.equal(partial.nodes.length - plain.nodes.length, changed.bodyNodes - base.bodyNodes);
  assert.equal(edges(partial) - edges(plain), changed.inputReferences - base.inputReferences);
  assert.equal(changed.bodyNodes - base.bodyNodes, 4);
  assert.equal(changed.inputReferences - base.inputReferences, 5);
});

test("acceleration branch envelope covers empty, concentrated and dense frame support with all lazy seeds", () => {
  const { EVENT_LIMITS, deriveBranchTopologyContributions } = require("../a68-branch-topology-envelope");
  const entry = (row, column) => ({ row, column, value: "1" });
  const supports = [[], [entry(0, 0)], Array.from({ length: 14 }, (_, i) => entry(i, 0)),
    Array.from({ length: 14 }, (_, i) => entry(0, i)), Array.from({ length: 196 }, (_, i) => entry(Math.floor(i / 14), i % 14))];
  let residual;
  for (const frameEntries of supports) for (const framePartial of [false, true]) {
    const plan = fixture({ frameEntries, framePartial }).recipe.Finish();
    const counts = { ...Object.fromEntries(Object.keys(EVENT_LIMITS).map(k => [k, 0])),
      frameNonzeros: frameEntries.length, framePartialNonzeros: framePartial ? 1 : 0 };
    const b = deriveBranchTopologyContributions(counts).blocks;
    const actual = { nodes: plan.nodes.length - b.accelerationPartialB.nodeUpperBound - b.accelerationCovariantWY.nodeUpperBound,
      edges: plan.nodes.reduce((sum, n) => sum + n.inputs.length, 0) - b.accelerationPartialB.inputReferences - b.accelerationCovariantWY.inputReferences };
    if (!residual) residual = actual; else assert.deepEqual(actual, residual);
  }
  //942 fixed-body nodes/625 edges +16 input leaves +Phi107/105 +identity seed.
  assert.deepEqual(residual, { nodes: 1066, edges: 730 });
});

test("original/Euler scalar descriptors reference exact nodes without inventing Pair DAG nodes", () => {
  const { recipe, result } = fixture();
  assert.equal(result.scalarPlan.schemaVersion, SCALAR_SCHEMA);
  assert.deepEqual(result.Original[0], { op: "pair", left: 17, right: 10 });
  assert.deepEqual(result.Original[1], { op: "multiply", left: { op: "add", left: { op: "pair", left: 17, right: 11 }, right: { op: "pair", left: 4, right: 1686 } }, right: { op: "constant", value: "1/2" } });
  assert.deepEqual(result.Original[2].right, { op: "constant", value: "1/3" });
  assert.deepEqual(result.Original[3], { op: "multiply", left: { op: "pair", left: 4, right: 17 }, right: { op: "constant", value: "907712" } });
  assert.deepEqual(result.Euler.map(p => [p.left, p.right]), [[17, 10], [17, 13], [17, 14], [17, 15]]);
  const plan = recipe.Finish(); assert.equal(plan.nodes.filter(n => n.op === "pair").length, 0);
  assert.equal(result.scalarPlan.scope.scalarReplayImplementedHere, false); assert.equal(result.scalarPlan.scope.scalarOperandLivenessProvedHere, false);
});

test("every current and all196 current-derivative slots retain their distinct Contract nodes in exact order", () => {
  const { recipe, result } = fixture(), plan = recipe.Finish();
  const contracts = plan.nodes.filter(n => n.op === "contract"); assert.equal(contracts.length, 406);
  for (let a = 0; a < 14; a++) {
    assert.deepEqual(contracts[a], { id: 1696 + a, op: "contract", degree: 1, inputs: [9], parameters: { axis: a } });
    assert.deepEqual(result.Current[a], { op: "multiply", left: { op: "pair", left: 1696 + a, right: 17 }, right: { op: "constant", value: a < 7 ? "1/2" : "-1/2" } });
  }
  assert.equal(result.CurrentCovariantDerivative.length, 14);
  for (let z = 0; z < 14; z++) for (let a = 0; a < 14; a++) {
    const id = 1710 + 28 * z + 2 * a, expression = result.CurrentCovariantDerivative[z][a];
    assert.deepEqual(plan.nodes[id], { id, op: "contract", degree: 1, inputs: [1286 + 17 * z], parameters: { axis: a } });
    assert.deepEqual(plan.nodes[id + 1], { id: id + 1, op: "contract", degree: 1, inputs: [9], parameters: { axis: a } });
    assert.deepEqual(expression.left, { op: "add", left: { op: "pair", left: id, right: 17 }, right: { op: "pair", left: id + 1, right: 1278 + 17 * z } });
    assert.equal(expression.right.value, a < 7 ? "1/2" : "-1/2");
  }
});

test("coordinate-current and divergence descriptors preserve the entire ordered sums, including zero frame entries", () => {
  const { result } = fixture();
  for (let mu = 0; mu < 14; mu++) {
    let expression = result.CoordinateCurrent[mu];
    for (let a = 13; a >= 0; a--) {
      assert.equal(expression.op, "add"); assert.equal(expression.right.op, "multiply");
      assert.deepEqual(expression.right.left, { op: "matrixEntry", matrix: "background.Frame", row: mu, column: a });
      assert.equal(expression.right.right, result.Current[a]); expression = expression.left;
    }
    assert.deepEqual(expression, { op: "constant", value: "0" });
  }
  let divergence = result.Divergence;
  for (let z = 13; z >= 0; z--) { assert.equal(divergence.op, "add"); assert.equal(divergence.right, result.CurrentCovariantDerivative[z][z]); divergence = divergence.left; }
  assert.deepEqual(divergence, { op: "constant", value: "0" });
  assert.equal(result.scalarPlan.geometry[0].id, "background.Frame"); assert.equal(result.scalarPlan.geometry[0].entries.length, 14);
  assert.ok(Object.isFrozen(result.scalarPlan)); assert.ok(Object.isFrozen(result.CurrentCovariantDerivative[13][13]));
});

test("word/oracle mode changes no semantic trace or scalar formula and retains mode identity", () => {
  const normal = fixture(), oracle = fixture({ oracle: true });
  assert.equal(normal.result.Oracle, false); assert.equal(oracle.result.Oracle, true);
  assert.deepEqual(normal.recipe.Finish(), oracle.recipe.Finish()); assert.deepEqual(normal.result.scalarPlan, oracle.result.scalarPlan);
});

test("acceleration fails closed on wrong input types, incomplete jet menus or missing identity matrix", () => {
  const { recipe, b, g, tensors } = fixture();
  assert.throws(() => buildMixedWardAcceleration(recipe, b, g, { eta: tensors.X, nativeDeta: tensors.nativeDeta }), /input degree/);
  assert.throws(() => buildMixedWardAcceleration(recipe, b, g, { eta: tensors.eta, nativeDeta: tensors.nativeDeta, oracle: "yes" }), /oracle/);
  assert.throws(() => buildMixedWardAcceleration(recipe, b, { ...g, DeltaOmega: g.DeltaOmega.slice(1) }, { eta: tensors.eta, nativeDeta: tensors.nativeDeta }), /all14/);
  assert.throws(() => buildMixedWardAcceleration(recipe, { ...b, Identity: recipe.Matrix("zero") }, g, { eta: tensors.eta, nativeDeta: tensors.nativeDeta }), /identity/);
});

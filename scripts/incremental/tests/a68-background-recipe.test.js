"use strict";
// Synthetic metadata only; no actual geometry or Clifford tensors are evaluated.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { BACKGROUND_SCHEMA, GERM_SCHEMA, Derivative, WedgeCoordinate, Divergence, CoordinateDual, FrameDual, buildMixedBackground, baselineRoles, canonicalBaselineImports, importMixedBaseline, buildMixedMetricGerm, multiindices } = require("../a68-background-recipe");
const hash = "0".repeat(64), leaf = (id, degree) => ({ id, degree, source: "synthetic/" + id, sha256: hash });
const fourteen = value => Array.from({ length: 14 }, () => value);
const constants = ["0", "1", "-1", "-1/2", "1/2", "1/3", "907712"];
const baseMatrices = [
  { name: "identity", matrix: Array.from({ length: 14 }, (_, i) => ({ row: i, column: i, value: "1" })) },
  { name: "zero", matrix: [] },
  { name: "asymmetric", matrix: [{ row: 0, column: 7, value: "2" }] }
];
const provenance = [{ id: "synthetic-geometry", source: "synthetic/metadata-fixture", sha256: hash }];
function backgroundDescriptor(point = 0) {
  return { schemaVersion: BACKGROUND_SCHEMA, point, provenance, matrices: {
    Frame: "identity", InverseFrame: "identity", Identity: "identity", FrameLift: fourteen("zero"), FramePartial: fourteen("zero"), Omega: fourteen("zero"), InversePartial: fourteen("zero"), ConnectionInFrame: fourteen("zero")
  } };
}
function validator(expected, matrices = baseMatrices) { return (actual, values) => {
  assert.deepEqual(actual, expected); assert.ok(Object.isFrozen(actual)); assert.ok(Object.isFrozen(values));
  const names = [...new Set(Object.values(expected.matrices).flat(2))].sort();
  assert.deepEqual(values, Object.fromEntries(names.map(id => [id, matrices.find(m => m.name === id).matrix]))); return true;
}; }
function builtBackground() {
  const geometry = backgroundDescriptor(), recipe = new MixedRecipe({ leaves: [leaf("X", 1), leaf("F", 2)], constants, matrices: baseMatrices });
  const b = buildMixedBackground(recipe, { x: recipe.RegisterLeaf("X"), curvatureLeaf: "F", geometry, validateGeometry: validator(geometry) });
  return { recipe, b, geometry };
}
function importFixture(extraLeaves = [], extraMatrices = [], point = 0) {
  const roles = baselineRoles(), canonical = [...new Set(roles.map(r => r.canonicalId))], leafIds = Object.fromEntries(canonical.map(id => [id, "baseline/" + id]));
  const leaves = canonical.map(id => leaf(leafIds[id], roles.find(r => r.canonicalId === id).degree));
  const recipe = new MixedRecipe({ leaves: [...leaves, ...extraLeaves], constants, matrices: [...baseMatrices, ...extraMatrices] }), geometry = backgroundDescriptor(point);
  const b = importMixedBaseline(recipe, { geometry, validateGeometry: validator(geometry), leafIds });
  return { recipe, b, geometry, leafIds };
}

test("background trace matches hand-enumerated first native jet and midstream curvature boundary", () => {
  const { recipe, b } = builtBackground();
  recipe.Mark("NativePartial0", 1, b.NativePartial[0], false); recipe.Mark("NativeOracle0", 1, b.NativePartialOracle[0], false);
  const plan = recipe.Finish();
  assert.equal(plan.nodes.length, 1016);
  assert.deepEqual(plan.nodes.slice(0, 19).map(n => [n.op, n.inputs]), [
    ["leaf", []], ["pullback", [0]], ["spin", []], ["unit", []], ["product", [3, 2]], ["scale", [4]], ["zero", []], ["sum", [6, 5]],
    ["pullback", [0]], ["spin", []], ["product", [9, 0]], ["motion", [0]], ["scale", [11]], ["sum", [10, 12]],
    ["motion", [0]], ["product", [2, 0]], ["scale", [15]], ["sum", [13, 14, 16]], ["pullback", [17]]
  ]);
  assert.deepEqual(plan.nodes.filter(n => n.op === "leaf").map(n => [n.id, n.parameters.id]), [[0, "X"], [554, "F"]]);
  assert.deepEqual(plan.nodes[555].inputs, [0, 0]);
  assert.deepEqual(plan.marks.map(m => m.node), [8, 18]);
  assert.deepEqual(plan.nodes.at(-1).inputs, [681, 1010, 1013, 1014]);
});

test("full native/covariant jets and all27 canonical background exports preserve the sole alias", () => {
  const { recipe, b } = builtBackground(), canonical = canonicalBaselineImports(recipe, b);
  for (const name of ["NativePartial", "NativePartialOracle", "CovariantCoordinate", "CovariantFrame"]) assert.equal(b[name].length, 14);
  assert.equal(b.GradientPieces[0], b.KInputs[0]); assert.equal(canonical.length, 27);
  assert.equal(new Set(canonical.map(x => x.value)).size, 27);
  canonical.forEach(entry => recipe.Mark(entry.id, entry.degree, entry.value, false));
  const plan = recipe.Finish();
  assert.deepEqual(plan.marks.slice(0, 9).map(m => [m.name, m.node]), [["X", 0], ["B", 215], ["F", 554], ["DX", 493], ["Q", 555], ["AdjointX", 672], ["KInputs[0]", 681], ["KInputs[1]", 690], ["KInputs[2]", 699]]);
  assert.deepEqual(plan.marks.slice(-14).map(m => m.node), Array.from({ length: 14 }, (_, z) => 241 + 15 * z));
});

test("27 imported baseline identities are explicit registrations, not equal-value deduplication", () => {
  const { recipe, b } = importFixture();
  const imports = canonicalBaselineImports(recipe, b); assert.equal(imports.length, 27);
  assert.equal(b.GradientPieces[0], b.KInputs[0]); assert.notEqual(b.KInputs[0], b.KInputs[1]);
  const malformed = { ...b, GradientPieces: [b.KInputs[1], ...b.GradientPieces.slice(1)] };
  assert.throws(() => canonicalBaselineImports(recipe, malformed), /alias/);
  const plan = recipe.Finish(); assert.equal(plan.nodes.length, 27); assert.ok(plan.nodes.every(n => n.op === "leaf"));
});

test("germ trace includes all196 derivative-spin slots, exact nu/mu indexing and late independent curvature leaves", () => {
  const matrices = [], partials = Array.from({ length: 14 }, () => Array(14));
  for (let nu = 0; nu < 14; nu++) for (let mu = 0; mu < 14; mu++) {
    const name = "partial-" + nu + "-" + mu; partials[nu][mu] = name;
    matrices.push({ name, matrix: [{ row: 0, column: 1, value: String(1 + nu * 14 + mu) }] });
  }
  const { recipe, b } = importFixture([leaf("adapted", 2), leaf("oracle", 2)], matrices);
  const geometry = { schemaVersion: GERM_SCHEMA, point: 0, metricBasis: 7, jetIndex: 34, provenance, matrices: {
    Motion: "zero", DeltaFrame: "zero", MotionPartial: fourteen("zero"), MotionCovariant: fourteen("zero"), DeltaOmega: fourteen("zero"), DeltaOmegaPartial: partials
  } };
  const germ = buildMixedMetricGerm(recipe, b, { geometry, validateGeometry: validator(geometry, [...baseMatrices, ...matrices]), adaptedLeaf: "adapted", oracleLeaf: "oracle" });
  assert.equal(germ.Order, 3); assert.deepEqual(germ.Multiindex, [3, 0, 0, 0]);
  recipe.Mark("DeltaBExterior", 2, germ.DeltaBExterior, true); recipe.Mark("DeltaFFixed", 2, germ.DeltaFFixed, true);
  const plan = recipe.Finish(); assert.equal(plan.nodes.length, 1478);
  assert.deepEqual(plan.nodes.slice(27, 33).map(n => [n.op, n.inputs]), [["spin", []], ["unit", []], ["product", [28, 27]], ["scale", [29]], ["zero", []], ["sum", [31, 30]]]);
  const spins = plan.nodes.filter(n => n.op === "spin"); assert.equal(spins.length, 210);
  assert.deepEqual(spins.slice(14).map(n => n.parameters.matrix[0].value), Array.from({ length: 196 }, (_, i) => String(1 + (i % 14) * 14 + Math.floor(i / 14))));
  assert.deepEqual(plan.nodes.filter(n => n.op === "leaf").slice(-2).map(n => [n.id, n.parameters.id]), [[1473, "adapted"], [1474, "oracle"]]);
  assert.deepEqual(plan.nodes.slice(-5).map(n => [n.op, n.inputs]), [["leaf", []], ["leaf", []], ["motion", [2]], ["scale", [1475]], ["sum", [1473, 1476]]]);
  assert.equal(plan.marks[0].node, 1470);
});

test("derivative, divergence and coordinate dual macros emit real primitive operations", () => {
  const recipe = new MixedRecipe({ leaves: [leaf("x", 1), leaf("y", 2)], matrices: baseMatrices });
  const x = recipe.RegisterLeaf("x"), y = recipe.RegisterLeaf("y"), a = recipe.Matrix("asymmetric");
  const d = Derivative(recipe, a, x), native = CoordinateDual(recipe, a, x), frame = FrameDual(recipe, a, native);
  const divergence = Divergence(recipe, fourteen(y)), wedge = WedgeCoordinate(recipe, fourteen(x), recipe.Matrix("identity"));
  for (const [name, degree, value] of [["d", 1, d], ["native", 1, native], ["frame", 1, frame], ["divergence", 1, divergence], ["wedge", 2, wedge]]) recipe.Mark(name, degree, value, false);
  const plan = recipe.Finish(); assert.deepEqual(plan.nodes.slice(2, 7).map(n => n.op), ["spin", "product", "motion", "scale", "sum"]);
  assert.deepEqual(plan.nodes[8].parameters.matrix, [{ row: 7, column: 0, value: "2" }], "ordinary transpose, not signed metric transpose");
  assert.deepEqual(plan.nodes.slice(11, 39).filter(n => n.op === "scale").map(n => n.parameters.real), [...Array(7).fill("-1"), ...Array(7).fill("1")]);
});

test("closed geometry APIs require independent validation and reject incomplete or mistyped menus", () => {
  const make = () => { const recipe = new MixedRecipe({ leaves: [leaf("X", 1), leaf("F", 2)], constants, matrices: baseMatrices }); return { recipe, x: recipe.RegisterLeaf("X") }; };
  for (const validateGeometry of [undefined, () => false]) {
    const { recipe, x } = make(); assert.throws(() => buildMixedBackground(recipe, { x, curvatureLeaf: "F", geometry: backgroundDescriptor(), validateGeometry }), /independent/);
  }
  const { recipe, x } = make(), bad = structuredClone(backgroundDescriptor()); bad.matrices.Omega.pop();
  assert.throws(() => buildMixedBackground(recipe, { x, curvatureLeaf: "F", geometry: bad, validateGeometry: () => true }), /all14/);
  const { recipe: other, x: otherX } = make(), wrong = structuredClone(backgroundDescriptor()); wrong.matrices.Identity = "zero";
  assert.throws(() => buildMixedBackground(other, { x: otherX, curvatureLeaf: "F", geometry: wrong, validateGeometry: () => true }), /identity/);
});

test("fixed combinatorial germ domain is35 jets per10 metrics at each of2 points", () => {
  const jets = multiindices(); assert.equal(jets.length, 35); assert.equal(new Set(jets.map(JSON.stringify)).size, 35);
  assert.deepEqual([0, 1, 2, 3].map(order => jets.filter(j => j.reduce((a, b) => a + b, 0) === order).length * 10 * 2), [20, 80, 200, 400]);
  assert.equal(baselineRoles().length, 28); assert.equal(new Set(baselineRoles().map(r => r.canonicalId)).size, 27);
});

test("geometry validation receives actual resolved values, not just trusted-looking names", () => {
  const geometry = backgroundDescriptor();
  const wrong = baseMatrices.map(m => m.name === "zero" ? { name: "zero", matrix: [{ row: 0, column: 0, value: "2" }] } : m);
  const recipe = new MixedRecipe({ leaves: [leaf("X", 1), leaf("F", 2)], constants, matrices: wrong });
  assert.throws(() => buildMixedBackground(recipe, { x: recipe.RegisterLeaf("X"), curvatureLeaf: "F", geometry, validateGeometry: validator(geometry) }), assert.AssertionError);
});

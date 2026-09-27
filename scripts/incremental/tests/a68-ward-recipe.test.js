"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { BiTensorRecipe, FIELDS, TOP_SCHEMA } = require("../a68-original-action-recipe");
const { buildMixedWardAction, buildWardChain, buildWardDensity, wardConjugate } = require("../a68-ward-recipe");
const leaf = (id, degree) => ({ id, degree, source: "synthetic/" + id, sha256: "0".repeat(64) });
const matrices = [{ name: "frame", matrix: [{ row: 0, column: 7, value: "2" }] },
  { name: "motion", matrix: [{ row: 1, column: 2, value: "-3" }] }];
function fixture() {
  const leaves = [leaf("eta", 0), leaf("nativeDeta", 1), leaf("x", 1), leaf("b", 1), leaf("f", 2), leaf("dx", 2),
    leaf("db", 1), leaf("dff", 2), leaf("dbExterior", 2), leaf("connectionCurvature", 2)];
  const r = new MixedRecipe({ leaves, matrices });
  const t = Object.fromEntries(leaves.map(d => [d.id, r.RegisterLeaf(d.id)]));
  return { r, eta: t.eta, nativeDeta: t.nativeDeta,
    b: { Frame: r.Matrix("frame"), X: t.x, B: t.b, F: t.f, DX: t.dx },
    g: { Motion: r.Matrix("motion"), DeltaB: t.db, DeltaFFixed: t.dff, DeltaBExterior: t.dbExterior, DeltaCurvatureFromConnection: t.connectionCurvature } };
}
const primitiveMap = result => Object.fromEntries(result.Primitives.map(p => [p.Name, p.Tensor]));
function evaluate(compensated = true, oracle = false) {
  const f = fixture(), result = buildMixedWardAction(f.r, f.b, f.g, f.eta, f.nativeDeta, compensated, oracle);
  return { ...f, result, primitives: primitiveMap(result) };
}
const primitiveNames = ["epsilon", "inverse", "epsilonInverse", "inverseEpsilon", "referenceCurvature", "DBepsilon", "DBinverse", "DBSquaredEpsilon",
  "DBSquaredEpsilonExpanded", "curvatureVariationCommutator", "gaugeDifference", "DBgaugeDifference", "varpi", "DBvarpi", "T", "DBT", "rotatedCurvature",
  "covariantT", "quadraticT", "unrotatedPhiFirst", "unrotatedPhiOuter", "unrotatedPhiInner", "phiFirst", "phiOuter", "phiInner", "descendedT",
  "descendedDerivative", "descendedDerivativeOracle", "descendedCurvature", "descendedQuadratic", "descendedPhiFirst", "descendedPhiOuter", "descendedPhiInner"];

test("Ward chain uses separately supplied first/outer/inner tensors in the exact three positions", () => {
  const groups = [["input", 2], ["first", 1], ["outer", 1], ["inner", 2]];
  const r = new MixedRecipe({ leaves: groups.flatMap(([name, d]) => FIELDS.map(f => leaf(name + f, d))), matrices }), bi = new BiTensorRecipe(r);
  const values = groups.map(([name]) => bi.Tensor(...FIELDS.map(f => r.RegisterLeaf(name + f))));
  const result = buildWardChain(r, ...values, r.Matrix("motion"));
  result.forEach((stage, i) => FIELDS.forEach(f => assert.equal(r.TensorDegree(stage[f]), [2, 12, 13, 14, 0, 1, 13, 1][i])));
  const plan = r.Finish();
  // 4 Stars*18 + 3 Products*12 + Scale*4 + Add*4 =116; no solder construction.
  assert.equal(plan.nodes.length, 132); assert.deepEqual(plan.marks, []);
  assert.deepEqual(plan.nodes[34], { id: 34, op: "product", degree: 13, inputs: [4, 16], parameters: { kind: "C" } });
  assert.deepEqual(plan.nodes[46], { id: 46, op: "product", degree: 14, inputs: [12, 16], parameters: { kind: "A" } });
  assert.deepEqual(plan.nodes[76], { id: 76, op: "product", degree: 1, inputs: [8, 58], parameters: { kind: "A" } });
  assert.equal(plan.nodes.filter(n => n.op === "unit").length, 0);
});

test("conjugation is left-associated and does not identify literal conjugations", () => {
  const groups = [["left", 0], ["x", 1], ["right", 0]];
  const r = new MixedRecipe({ leaves: groups.flatMap(([name, d]) => FIELDS.map(f => leaf(name + f, d))) }), bi = new BiTensorRecipe(r);
  const values = groups.map(([name]) => bi.Tensor(...FIELDS.map(f => r.RegisterLeaf(name + f))));
  const first = wardConjugate(r, ...values), second = wardConjugate(r, ...values);
  assert.notEqual(first.Value, second.Value);
  const plan = r.Finish(); assert.equal(plan.nodes.length, 60);
  assert.deepEqual(plan.nodes[12].inputs, [0, 4]); assert.deepEqual(plan.nodes[24].inputs, [12, 8]);
  assert.deepEqual(plan.nodes[36].inputs, [0, 4]); assert.deepEqual(plan.nodes[48].inputs, [36, 8]);
});

test("all four Ward route/compensation combinations retain all33 primitives and both full densities", () => {
  for (const compensated of [false, true]) for (const oracle of [false, true]) {
    const { r, result } = evaluate(compensated, oracle);
    assert.equal(result.Compensated, compensated); assert.equal(result.Oracle, oracle);
    assert.deepEqual(result.Primitives.map(p => p.Name), primitiveNames); assert.ok(Object.isFrozen(result.Primitives));
    assert.ok(Object.isFrozen(result)); result.Primitives.forEach(p => assert.ok(Object.isFrozen(p)));
    for (const density of [result.Literal, result.Descended]) {
      assert.equal(density.Stages.length, 3); assert.equal(density.TopForms.length, 4); assert.ok(Object.isFrozen(density));
      density.Stages.forEach(chain => { assert.equal(chain.length, 8); chain.forEach((stage, s) => FIELDS.forEach(f => {
        const degree = r.TensorDegree(stage[f]); assert.ok(degree === -1 || degree === [2, 12, 13, 14, 0, 1, 13, 1][s]);
      })); });
      for (const [array, f] of [["Value", "Value"], ["Metric", "H"], ["Field", "U"], ["Mixed", "HU"]]) {
        assert.equal(density[array].length, 4);
        density[array].forEach((d, p) => {
          assert.deepEqual(d, { schemaVersion: TOP_SCHEMA, tensor: density.TopForms[p][f], node: r.RecordedNode(density.TopForms[p][f]),
            degree: 14, form: 16383, blade: 0, k0: 0, k1: 0, absentCoefficient: "0", imaginaryRequired: "0", realFactor: "-1",
            weight: ["1", "1/2", "1/3", "453856"][p] });
          assert.ok(Object.isFrozen(d));
        });
      }
    }
    const plan = r.Finish(); assert.deepEqual(plan.marks, []);
    // Hand census: 59 BiProducts,32 BiStars,9 BiScales,12 BiAdds;
    // cached solder105 units+2 sums;3 Phi motions/scales; shared empty1.
    // Compensated direct:15 products,4 sums,5 scales,identity,pullback.
    // Uncompensated removes4 products,2 sums,1 scale and adds2 distinct zeros.
    assert.equal(plan.nodes.length, compensated ? 1518 : 1513);
    const counts = {}; for (const node of plan.nodes) counts[node.op] = (counts[node.op] ?? 0) + 1;
    assert.deepEqual(counts, { leaf: 10, unit: 106, pullback: 1, product: compensated ? 546 : 542,
      sum: compensated ? 359 : 357, scale: compensated ? 108 : 107, zero: compensated ? 1 : 3, motion: 131, star: 256 });
  }
});

test("native tangent HU is shared zero, while d2Expanded independently uses connection and native eta jet", () => {
  const { r, result, primitives: p } = evaluate();
  assert.equal(p.varpi.HU, p.epsilon.H); assert.notEqual(p.varpi.HU, p.DBepsilon.HU);
  assert.equal(p.varpi.U, result.Tangent); assert.equal(p.DBvarpi.U, result.DBTangent);
  assert.equal(p.DBSquaredEpsilonExpanded.U, p.DBSquaredEpsilon.U);
  assert.notEqual(p.DBSquaredEpsilonExpanded.HU, p.DBSquaredEpsilon.HU);
  const expandedNode = r.RecordedNode(p.DBSquaredEpsilonExpanded.HU), connectionNode = r.RecordedNode(p.curvatureVariationCommutator.HU);
  const plan = r.Finish();
  assert.deepEqual(plan.nodes.slice(10, 34).map(n => [n.op, n.inputs]), [
    ["unit", []], ["pullback", [1]], ["product", [3, 0]], ["sum", [11, 12]], ["product", [6, 0]],
    ["product", [2, 0]], ["sum", [13, 15]], ["product", [4, 0]], ["product", [5, 0]], ["product", [2, 13]],
    ["scale", [19]], ["sum", [17, 18, 20]], ["scale", [0]], ["scale", [13]], ["scale", [14]],
    ["product", [4, 0]], ["product", [7, 0]], ["product", [8, 0]], ["product", [6, 11]], ["scale", [28]],
    ["product", [6, 13]], ["product", [3, 14]], ["sum", [27, 29, 30, 31]], ["product", [9, 0]]
  ]);
  assert.equal(expandedNode, 32); assert.equal(connectionNode, 33);
  const ancestors = id => {
    const ids = new Set([id]); for (const input of plan.nodes[id].inputs) for (const a of ancestors(input)) ids.add(a); return ids;
  };
  assert.ok(ancestors(expandedNode).has(8)); assert.ok(ancestors(expandedNode).has(1));
  assert.ok(!ancestors(expandedNode).has(7)); assert.ok(!ancestors(connectionNode).has(7));
  assert.deepEqual(plan.nodes[76].inputs, [6, 16]); // Only DBvarpi, not varpi, includes [dotB,V].
});

test("oracle changes HU construction order without changing ordered public primitive names", () => {
  for (const oracle of [false, true]) {
    const { r, primitives: p } = evaluate(true, oracle);
    const id = r.RecordedNode(p.gaugeDifference.HU), plan = r.Finish();
    const pairs = plan.nodes[id].inputs.map(input => plan.nodes[input].inputs);
    assert.deepEqual(pairs, oracle ? [[22, 34], [10, 14], [34, 34], [34, 13]] : [[34, 34], [34, 13], [22, 34], [10, 14]]);
  }
});

test("first and outer Phi have separate variations and conjugations; controls execute after both densities", () => {
  const { r, result, primitives: p } = evaluate();
  assert.equal(p.unrotatedPhiFirst.Value, p.unrotatedPhiOuter.Value);
  assert.notEqual(p.unrotatedPhiFirst.H, p.unrotatedPhiOuter.H);
  FIELDS.forEach(f => assert.notEqual(p.phiFirst[f], p.phiOuter[f]));
  assert.equal(r.RecordedNode(p.phiFirst.Value), 262); assert.equal(r.RecordedNode(p.phiOuter.Value), 286);
  assert.equal(r.RecordedNode(p.phiInner.Value), 310);
  assert.equal(r.RecordedNode(p.descendedDerivative.Value), 890);
  assert.equal(r.RecordedNode(p.descendedDerivativeOracle.Value), 906);
  assert.equal(r.RecordedNode(p.epsilonInverse.Value), 1422); assert.equal(r.RecordedNode(p.inverseEpsilon.Value), 1434);
  assert.equal(r.RecordedNode(p.descendedPhiFirst.Value), 1458); assert.equal(r.RecordedNode(p.descendedPhiOuter.Value), 1482);
  assert.equal(r.RecordedNode(p.descendedPhiInner.Value), 1506);
  for (const [density, start] of [[result.Literal, 688], [result.Descended, 1320]]) {
    FIELDS.forEach((f, i) => density.TopForms.forEach((top, piece) =>
      assert.equal(r.RecordedNode(top[f]), start + piece * 30 + [0, 3, 6, 11][i])));
  }
  const plan = r.Finish();
  assert.deepEqual(plan.nodes.slice(137, 156).map(n => n.op), [...Array(14).fill("unit"), "sum", "motion", "scale", "motion", "scale"]);
  assert.deepEqual(plan.nodes[248].inputs, [247]);
  assert.deepEqual(plan.nodes[890].inputs, [826, 850, 886]); // Three product-rule terms, last negated.
  assert.equal(plan.nodes[886].parameters.real, "-1");
});

test("uncompensated tangent zeros are distinct from the common empty and each other", () => {
  const { r, result, primitives: p } = evaluate(false);
  assert.notEqual(result.Tangent, result.DBTangent); assert.notEqual(result.Tangent, p.epsilon.H);
  assert.notEqual(result.DBTangent, p.epsilon.H); assert.equal(p.varpi.U, result.Tangent);
  assert.equal(p.DBvarpi.U, result.DBTangent);
  assert.equal(r.TensorDegree(result.Tangent), -1); assert.equal(r.TensorDegree(result.DBTangent), -1);
  const plan = r.Finish(); assert.equal(plan.nodes.filter(n => n.op === "zero").length, 3);
});

test("standalone Density accepts all distinct boundary fields and emits no solder or marks", () => {
  const groups = [["t", 1], ["f", 2], ["d", 2], ["q", 2], ["first", 1], ["outer", 1], ["inner", 2]];
  const r = new MixedRecipe({ leaves: groups.flatMap(([name, d]) => FIELDS.map(f => leaf(name + f, d))), matrices }), bi = new BiTensorRecipe(r);
  const values = groups.map(([name]) => bi.Tensor(...FIELDS.map(f => r.RegisterLeaf(name + f))));
  const density = buildWardDensity(r, ...values, r.Matrix("motion"), true);
  assert.equal(density.Stages[0][0], values[1]); assert.equal(density.Stages[1][0], values[2]); assert.equal(density.Stages[2][0], values[3]);
  const plan = r.Finish(); assert.equal(plan.nodes.length, 28 + 468);
  assert.deepEqual(plan.marks, []); assert.equal(plan.nodes.filter(n => n.op === "unit").length, 0);
});

test("ill-typed/foreign inputs and nonboolean route flags fail closed", () => {
  const { r, b, g, eta, nativeDeta } = fixture();
  assert.throws(() => buildMixedWardAction(r, b, g, eta, nativeDeta, 0), /boolean/);
  assert.throws(() => buildMixedWardAction(r, b, g, eta, nativeDeta, true, "false"), /boolean/);
  assert.throws(() => buildMixedWardAction(r, b, g, nativeDeta, nativeDeta, true), /degree/);
  assert.throws(() => buildMixedWardAction(r, b, { ...g, DeltaBExterior: eta }, eta, nativeDeta, true), /degree/);
  assert.throws(() => buildMixedWardAction(r, b, { ...g, Motion: {} }, eta, nativeDeta, true), /geometry/);
  const other = new MixedRecipe();
  assert.throws(() => buildMixedWardAction(r, b, g, other.FreshZero(), nativeDeta, true), /foreign/);
  const bi = new BiTensorRecipe(r);
  assert.throws(() => buildWardChain(r, bi.Fixed(eta), bi.Fixed(nativeDeta), bi.Fixed(nativeDeta), bi.Fixed(b.F), g.Motion), /degree/);
});

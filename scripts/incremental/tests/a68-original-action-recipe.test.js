"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { BiTensorRecipe, buildOriginalMixedAction, FIELDS, WEIGHTS, TOP_SCHEMA } = require("../a68-original-action-recipe");
const hash = "0".repeat(64);
const leaf = (id, degree) => ({ id, degree, source: "synthetic/" + id, sha256: hash });
const matrices = [{ name: "frame", matrix: [{ row: 0, column: 7, value: "2" }] },
  { name: "motion", matrix: [{ row: 1, column: 2, value: "-3" }] }];
function fixture() {
  const descriptors = [leaf("x", 1), leaf("b", 1), leaf("exterior", 2), leaf("f", 2),
    leaf("db", 1), leaf("dff", 2), leaf("u", 1), leaf("du", 2)];
  const r = new MixedRecipe({ leaves: descriptors, matrices });
  const handles = Object.fromEntries(descriptors.map(d => [d.id, r.RegisterLeaf(d.id)]));
  return { r, b: { Frame: r.Matrix("frame"), X: handles.x, B: handles.b, NativeExterior: handles.exterior, F: handles.f },
    g: { Motion: r.Matrix("motion"), DeltaB: handles.db, DeltaFFixed: handles.dff }, u: handles.u, du: handles.du };
}
function productFixture(reverseOrder, kind = "C") {
  const descriptors = ["x", "y"].flatMap(prefix => FIELDS.map(field => leaf(prefix + field, 1)));
  const r = new MixedRecipe({ leaves: descriptors }); descriptors.forEach(d => r.RegisterLeaf(d.id));
  const bi = new BiTensorRecipe(r), x = bi.Tensor(...FIELDS.map(f => r.Leaf("x" + f))), y = bi.Tensor(...FIELDS.map(f => r.Leaf("y" + f)));
  const result = bi.Product(x, y, kind, reverseOrder);
  FIELDS.forEach(f => r.Mark(f, 2, result[f], true)); return r.Finish();
}

test("BiTensor product has the hand-enumerated nine products and three sums, with no factorial", () => {
  for (const kind of ["W", "C", "A"]) for (const reverseOrder of [false, true]) {
    const plan = productFixture(reverseOrder, kind);
    assert.equal(plan.nodes.length, 20);
    const normalHU = [[3, 4], [1, 6], [2, 5], [0, 7]];
    const reverseHU = [[2, 5], [0, 7], [3, 4], [1, 6]];
    const pairs = [[0, 4], [1, 4], [0, 5], [2, 4], [0, 6], ...(reverseOrder ? reverseHU : normalHU)];
    const products = plan.nodes.filter(n => n.op === "product");
    assert.deepEqual(products.map(n => n.inputs), pairs);
    products.forEach(n => assert.deepEqual(n.parameters, { kind }));
    assert.deepEqual(plan.nodes.slice(8).map(n => n.op), ["product", "product", "product", "sum", "product", "product", "sum", "product", "product", "product", "product", "sum"]);
    assert.deepEqual(plan.nodes.filter(n => n.op === "sum").map(n => n.inputs), [[9, 10], [12, 13], [15, 16, 17, 18]]);
    assert.deepEqual(plan.marks.map(m => m.node), [8, 11, 14, 19]);
  }
});

test("Fixed empties are fresh, lazy and reused by identity; Add and Scale preserve field order", () => {
  const r = new MixedRecipe({ leaves: [leaf("x", 1)] }), x = r.RegisterLeaf("x"), bi = new BiTensorRecipe(r);
  const unused = bi.Fixed(x), fixed = bi.Fixed(x);
  assert.notEqual(fixed.H, fixed.U); assert.notEqual(fixed.U, fixed.HU); assert.notEqual(unused.H, fixed.H);
  const sum = bi.Add(fixed, fixed), scaled = bi.Scale(sum, "-1/2");
  FIELDS.forEach(f => r.Mark(f, 1, scaled[f], true));
  const plan = r.Finish();
  assert.deepEqual(plan.nodes.map(n => n.op), ["leaf", "sum", "zero", "sum", "zero", "sum", "zero", "sum", "scale", "scale", "scale", "scale"]);
  assert.deepEqual(plan.nodes.filter(n => n.op === "sum").map(n => n.inputs), [[0, 0], [2, 2], [4, 4], [6, 6]]);
  assert.deepEqual(plan.nodes.filter(n => n.op === "scale").map(n => n.inputs), [[1], [3], [5], [7]]);
  assert.ok(Object.isFrozen(scaled));
});

test("BiTensor Star preserves the complete hand-enumerated mixed Hodge derivative order", () => {
  const r = new MixedRecipe({ leaves: FIELDS.map(f => leaf(f, 2)), matrices });
  const bi = new BiTensorRecipe(r), input = bi.Tensor(...FIELDS.map(f => r.RegisterLeaf(f)));
  const result = bi.Star(input, r.Matrix("motion"), true);
  FIELDS.forEach(f => r.Mark(f, 12, result[f], true)); const plan = r.Finish();
  // 1 Value star; 8 H operations; 1 U star; 8 HU operations.
  assert.equal(plan.nodes.length, 22);
  assert.deepEqual(plan.nodes.slice(4).map(n => [n.op, n.inputs]), [
    ["star", [0]], ["star", [1]], ["motion", [0]], ["star", [6]], ["star", [0]],
    ["motion", [8]], ["scale", [9]], ["sum", [7, 10]], ["sum", [5, 11]],
    ["star", [2]], ["star", [3]], ["motion", [2]], ["star", [15]], ["star", [2]],
    ["motion", [17]], ["scale", [18]], ["sum", [16, 19]], ["sum", [14, 20]]
  ]);
  assert.deepEqual(plan.marks.map(m => m.node), [4, 12, 13, 21]);
  plan.nodes.filter(n => n.op === "motion").forEach(n => assert.deepEqual(n.parameters.matrix, matrices[1].matrix));
  plan.nodes.filter(n => n.op === "scale").forEach(n => assert.equal(n.parameters.real, "-1"));
});

test("standalone chain has all eight stages and initializes solder caches at the C# boundaries", () => {
  const r = new MixedRecipe({ leaves: FIELDS.map(f => leaf(f, 2)), matrices });
  const bi = new BiTensorRecipe(r), input = bi.Tensor(...FIELDS.map(f => r.RegisterLeaf(f)));
  const chain = bi.Chain(input, r.Matrix("motion"));
  assert.equal(chain[0], input); assert.equal(chain.length, 8);
  const degrees = [2, 12, 13, 14, 0, 1, 13, 1];
  chain.forEach((stage, i) => FIELDS.forEach(f => {
    assert.equal(r.TensorDegree(stage[f]), degrees[i]); r.Mark(i + "/" + f, degrees[i], stage[f], false);
  }));
  const plan = r.Finish();
  // 4 leaves + 107 cached-solder nodes + 120 chain operations + 4 lazy p1/p2 empties.
  assert.equal(plan.nodes.length, 235);
  assert.deepEqual(plan.nodes.slice(4, 23).map(n => n.op), [...Array(14).fill("unit"), "sum", "motion", "scale", "unit", "unit"]);
  assert.equal(plan.nodes[19].inputs[0], 18); assert.equal(plan.nodes[20].parameters.real, "-1");
  assert.deepEqual(plan.nodes.slice(113, 119).map(n => n.op), ["motion", "scale", "star", "star", "motion", "star"]);
  assert.equal(plan.nodes[113].inputs[0], 112);
  assert.equal(plan.nodes.filter(n => n.op === "zero").length, 4);
});

test("full original action retains three complete chains and four full top tensors without hidden marks", () => {
  for (const reverseOrder of [false, true]) {
    const { r, b, g, u, du } = fixture(), result = buildOriginalMixedAction(r, b, g, u, du, reverseOrder);
    assert.equal(result.Stages.length, 3); assert.equal(result.TopForms.length, 4);
    assert.ok(Object.isFrozen(result)); assert.ok(Object.isFrozen(result.Stages)); assert.ok(Object.isFrozen(result.TopForms));
    const degrees = [2, 12, 13, 14, 0, 1, 13, 1];
    result.Stages.forEach(chain => chain.forEach((stage, i) => FIELDS.forEach(f => {
      const d = r.TensorDegree(stage[f]); assert.ok(d === -1 || d === degrees[i]);
    })));
    for (const [array, field] of [["Value", "Value"], ["Metric", "H"], ["Field", "U"], ["Mixed", "HU"]]) {
      assert.equal(result[array].length, 4); assert.ok(Object.isFrozen(result[array]));
      result[array].forEach((descriptor, p) => {
        assert.deepEqual(descriptor, { schemaVersion: TOP_SCHEMA, tensor: result.TopForms[p][field], node: r.RecordedNode(result.TopForms[p][field]), degree: 14,
          form: 16383, blade: 0, k0: 0, k1: 0, absentCoefficient: "0", imaginaryRequired: "0", realFactor: "-1", weight: WEIGHTS[p] });
        assert.ok(Object.isFrozen(descriptor)); assert.equal(r.TensorDegree(descriptor.tensor), 14);
        assert.equal(descriptor.node, 544 + 30 * p + [0, 3, 6, 11][FIELDS.indexOf(field)]);
      });
    }
    assert.deepEqual(WEIGHTS, ["1", "1/2", "1/3", "453856"]);
    const plan = r.Finish(); assert.deepEqual(plan.marks, []);
    // Hand census: prefix 3 pullbacks + 2*12 products + 4 Add + 6 zeros;
    // 3 chains*(120 ops + 4 p1/p2 zeros), 2 curvature zeros, 107 solder;
    // final 4*(18 Star + 12 Product), plus 8 fixture leaves = 646.
    assert.equal(plan.nodes.length, 646);
    const counts = {}; for (const node of plan.nodes) counts[node.op] = (counts[node.op] ?? 0) + 1;
    assert.deepEqual(counts, { leaf: 8, pullback: 3, product: 135, zero: 20, sum: 127, unit: 105, motion: 70, scale: 50, star: 128 });
    assert.deepEqual(plan.nodes.slice(8, 11).map(n => [n.op, n.inputs, n.parameters.matrix]), [
      ["pullback", [6], matrices[0].matrix], ["pullback", [7], matrices[0].matrix], ["pullback", [2], matrices[0].matrix]
    ]);
    assert.equal(plan.nodes[45].op, "unit"); // First Phi1 getter, not before the initial covariant/quadratic products.
    assert.equal(plan.nodes.at(-1).op, "sum");
    assert.deepEqual(plan.nodes.at(-1).inputs, [641, 642, 643, 644]);
  }
});

test("preexisting session solder caches are reused, never recreated or silently marked", () => {
  const { r, b, g, u, du } = fixture(); const phi1 = r.Phi1, phi2 = r.Phi2;
  buildOriginalMixedAction(r, b, g, u, du);
  assert.equal(r.Phi1, phi1); assert.equal(r.Phi2, phi2);
  const plan = r.Finish();
  assert.equal(plan.nodes.length, 646); assert.equal(plan.nodes.filter(n => n.op === "unit").length, 105);
  assert.equal(plan.nodes[115].op, "pullback"); assert.deepEqual(plan.nodes[115].inputs, [6]);
  assert.deepEqual(plan.marks, []);
});

test("normal action prefix binds the original native exterior and spin product, not a gradient substitute", () => {
  const { r, b, g, u, du } = fixture(); buildOriginalMixedAction(r, b, g, u, du); const plan = r.Finish();
  assert.deepEqual(plan.nodes.slice(11, 33).map(n => [n.op, n.inputs]), [
    ["product", [1, 0]], ["product", [4, 0]], ["zero", []], ["product", [1, 13]], ["sum", [12, 14]],
    ["zero", []], ["product", [16, 0]], ["product", [1, 8]], ["sum", [17, 18]],
    ["zero", []], ["product", [20, 0]], ["product", [4, 8]], ["product", [16, 13]],
    ["zero", []], ["product", [1, 24]], ["sum", [21, 22, 23, 25]],
    ["sum", [10, 11]], ["zero", []], ["sum", [28, 15]], ["sum", [9, 19]], ["zero", []], ["sum", [31, 26]]
  ]);
});

test("caller marks are explicit, post-Evaluate, and preserve the already computed top and stage nodes", () => {
  const { r, b, g, u, du } = fixture(), result = buildOriginalMixedAction(r, b, g, u, du);
  result.Stages.forEach((chain, p) => chain.forEach((stage, s) => FIELDS.forEach(field =>
    r.Mark(`stage/${p}/${s}/${field}`, [2, 12, 13, 14, 0, 1, 13, 1][s], stage[field], true))));
  result.TopForms.forEach((top, p) => FIELDS.forEach(field => r.Mark(`top/${p}/${field}`, 14, top[field], true)));
  const plan = r.Finish(); assert.equal(plan.nodes.length, 646); assert.equal(plan.marks.length, 112);
  assert.deepEqual(plan.marks.slice(-4).map(m => m.node), [634, 637, 640, 645]);
});

test("lazy native derivative is accepted without eager materialization and foreign/ill-typed inputs fail closed", () => {
  const { r, b, g, u } = fixture(); const result = buildOriginalMixedAction(r, b, g, u, r.FreshZero());
  assert.equal(result.Field.length, 4); const plan = r.Finish();
  assert.deepEqual(plan.nodes.slice(8, 12).map(n => [n.op, n.inputs]), [["pullback", [6]], ["zero", []], ["pullback", [9]], ["pullback", [2]]]);
  const a = fixture(), bi = new BiTensorRecipe(a.r), other = new MixedRecipe();
  assert.throws(() => new BiTensorRecipe({}), /MixedRecipe/);
  assert.throws(() => bi.Tensor(a.u, a.du, a.u, a.u), /degree mismatch/);
  assert.throws(() => bi.Fixed(other.FreshZero()), /foreign/);
  assert.throws(() => bi.Add({ Value: a.u, H: a.u, U: a.u, HU: a.u, extra: true }), /descriptor/);
  assert.throws(() => bi.Product(bi.Fixed(a.u), bi.Fixed(a.u), "W", 1), /boolean/);
  assert.throws(() => bi.Chain(bi.Fixed(a.u), a.g.Motion), /degree/);
  assert.throws(() => buildOriginalMixedAction(a.r, a.b, a.g, a.du, a.du), /degree/);
  assert.throws(() => buildOriginalMixedAction(a.r, a.b, a.g, a.u, a.du, "false"), /boolean/);
  assert.throws(() => buildOriginalMixedAction(a.r, { ...a.b, Frame: {} }, a.g, a.u, a.du), /geometry/);
});

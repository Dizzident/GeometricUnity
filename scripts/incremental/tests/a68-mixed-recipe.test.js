"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe, validateMixedRecipe, SCHEMA } = require("../a68-mixed-recipe");
const hash = "0".repeat(64);
const leaf = (id, degree) => ({ id, degree, source: "synthetic/" + id, sha256: hash });
const matrix = { name: "a", matrix: [{ row: 0, column: 0, value: "1/2" }, { row: 0, column: 7, value: "2" }, { row: 7, column: 0, value: "-3" }] };
const create = degree => { const recipe = new MixedRecipe({ leaves: [leaf("x", degree)], matrices: [matrix] }); recipe.RegisterLeaf("x"); return recipe; };
const graphFrom = plan => ({ schemaVersion: SCHEMA, leaves: structuredClone(plan.leaves), nodes: plan.nodes.map(n => ({ ...structuredClone(n), records: 0, bytes: 2, sha256: hash })), marks: plan.marks.map(m => ({ ...m, sha256: hash })) });

test("symbolic node lookup is pure and cannot materialize an unused zero or foreign handle", () => {
  const recipe = create(1), x = recipe.Leaf("x"), zero = recipe.FreshZero();
  assert.equal(recipe.RecordedNode(x), 0);
  assert.throws(() => recipe.RecordedNode(zero), /already have a recorded node/);
  assert.throws(() => recipe.RecordedNode(create(1).Leaf("x")), /already have a recorded node/);
  const sum = recipe.Sum(x, zero); assert.equal(recipe.RecordedNode(zero), 1); assert.equal(recipe.RecordedNode(sum), 2);
  const plan = recipe.Finish(); assert.equal(plan.nodes.length, 3); assert.equal(plan.marks.length, 0);
  assert.throws(() => recipe.RecordedNode(x), /closed/);
});

test("symbolic identities: explicit leaves, lazy distinct zeros, aliases and exact mark nodes", () => {
  const recipe = new MixedRecipe({ leaves: [leaf("x", 1), leaf("y", 2)] });
  recipe.RegisterLeaf("x"); recipe.RegisterLeaf("y");
  const a = recipe.FreshZero(), b = recipe.FreshZero();
  recipe.Mark("zero-first", 1, a, false);
  recipe.Mark("zero-alias", 2, a, true);
  recipe.Mark("zero-distinct", 0, b, false);
  const plan = recipe.Finish();
  assert.deepEqual(plan.nodes.map(n => [n.id, n.op, n.degree]), [[0, "leaf", 1], [1, "leaf", 2], [2, "zero", -1], [3, "zero", -1]]);
  assert.deepEqual(plan.marks.map(m => m.node), [2, 2, 3]);
  assert.throws(() => recipe.Zero(), /closed/);
});

test("symbolic geometry leaves register midstream without reordering earlier operations", () => {
  const recipe = new MixedRecipe({ leaves: [leaf("x", 1), leaf("curvature", 2)] });
  assert.throws(() => recipe.Leaf("x"), /unregistered/);
  const x = recipe.RegisterLeaf("x"); recipe.Times(x, "-1");
  assert.throws(() => recipe.Finish(), /all declared leaves/);
  assert.throws(() => recipe.RegisterLeaf("x"), /repeated/);
  recipe.RegisterLeaf("curvature");
  const plan = recipe.Finish();
  assert.deepEqual(plan.nodes.map(n => [n.id, n.op]), [[0, "leaf"], [1, "scale"], [2, "leaf"]]);
  assert.equal(plan.nodes[2].parameters.id, "curvature");
});

test("symbolic zeros: unused zero is absent; repeated operand reuses identity without value deduplication", () => {
  const recipe = new MixedRecipe(); recipe.FreshZero();
  const zero = recipe.FreshZero(), total = recipe.Sum(zero, zero);
  recipe.Mark("total", 14, total, true);
  const plan = recipe.Finish();
  assert.deepEqual(plan.nodes, [
    { id: 0, op: "zero", degree: -1, inputs: [], parameters: {} },
    { id: 1, op: "sum", degree: -1, inputs: [0, 0], parameters: {} }
  ]);
});

test("symbolic primitive degree rules preserve typed zero units and reject invalid domains", () => {
  const recipe = create(2), x = recipe.Leaf("x");
  const zeroUnit = recipe.Unit(3, 0, "0"), scalar = recipe.Component(x, 3), contraction = recipe.Contract(x, 0);
  recipe.Mark("typed-zero", 2, zeroUnit, true); recipe.Mark("scalar", 0, scalar, true); recipe.Mark("contract", 1, contraction, true);
  assert.throws(() => recipe.Sum(x, scalar), /degree/);
  assert.throws(() => recipe.HodgeAdjoint(x, 2), /input degree/);
  assert.throws(() => recipe.P(x, x, "bad"), /kind/);
  assert.throws(() => recipe.Unit(16384, 0), /masks/);
  assert.throws(() => recipe.Contract(x, 14), /axis/);
  assert.throws(() => recipe.Times(x, "2"), /undeclared fixed/);
  assert.throws(() => recipe.Hodge({}), /foreign/);
  assert.throws(() => recipe.Motion({}, x), /undeclared geometry/);
});

test("symbolic descriptors: noncanonical rational, duplicate and unknown descriptors fail closed", () => {
  for (const constant of ["-0", "01", "1/1", "2/4", "0/2", "1/0", "1e0"]) assert.throws(() => new MixedRecipe({ constants: [constant] }));
  assert.throws(() => new MixedRecipe({ leaves: [leaf("x", 1), leaf("x", 1)] }), /unique/);
  assert.throws(() => new MixedRecipe({ leaves: [{ ...leaf("x", 1), unexpected: true }] }), /descriptor/);
  assert.throws(() => new MixedRecipe({ matrices: [{ name: "a", matrix: [{ row: 0, column: 0, value: "0" }] }] }), /zero/);
  assert.throws(() => new MixedRecipe({ matrices: [{ name: "a", matrix: [{ row: 1, column: 0, value: "1" }, { row: 0, column: 0, value: "1" }] }] }), /ordering/);
  assert.throws(() => new MixedRecipe({ leaves: [leaf("nonascii-é", 1)] }), /unique/);
});

test("symbolic geometry: metric transpose signs and pairing trace are independent metadata", () => {
  const recipe = create(1), a = recipe.Matrix("a"), x = recipe.Leaf("x");
  const result = recipe.PairingMotion(a, x); recipe.Mark("pairing", 1, result, true);
  const plan = recipe.Finish();
  assert.deepEqual(plan.nodes.map(n => n.op), ["leaf", "motion", "motion", "scale", "sum"]);
  assert.deepEqual(plan.nodes[2].parameters.matrix, [{ row: 0, column: 0, value: "1/2" }, { row: 0, column: 7, value: "3" }, { row: 7, column: 0, value: "-2" }]);
  assert.deepEqual(plan.nodes[3].parameters, { real: "-1/2", imaginary: "0" });
  assert.deepEqual(plan.nodes[4].inputs, [1, 2, 3]);
});

test("symbolic solder: per-session lazy caches preserve exact unit/sum order", () => {
  const recipe = new MixedRecipe(), one = recipe.Phi1;
  assert.equal(recipe.Phi1, one); const two = recipe.Phi2; assert.equal(recipe.Phi2, two);
  recipe.Mark("phi1", 1, one, false); recipe.Mark("phi2", 2, two, false);
  const plan = recipe.Finish(); assert.equal(plan.nodes.length, 107);
  for (let a = 0; a < 14; a++) assert.deepEqual(plan.nodes[a].parameters, { form: 1 << a, blade: 1 << a, real: "1", imaginary: "0" });
  assert.deepEqual(plan.nodes[14].inputs, Array.from({ length: 14 }, (_, i) => i));
  assert.deepEqual(plan.nodes[15].parameters, { form: 3, blade: 3, real: "1", imaginary: "0" });
  assert.deepEqual(plan.nodes[105].parameters, { form: 12288, blade: 12288, real: "1", imaginary: "0" });
  assert.deepEqual(plan.marks.map(m => m.node), [14, 106]);
  const other = new MixedRecipe(); assert.notEqual(other.Phi1, one);
});

test("symbolic matrix coefficients retain their independently declared geometry role", () => {
  const recipe = create(1), a = recipe.Matrix("a"), x = recipe.Leaf("x");
  assert.equal(recipe.MatrixValue(a, 1, 1), "0");
  assert.throws(() => recipe.MatrixValue(a, -1, 0), /indices/);
  recipe.TimesMatrixEntry(x, a, 0, 7);
  recipe.TimesMatrixProduct(x, a, 0, 7, a, 7, 0);
  recipe.Pullback(recipe.TransposeMatrix(a), x);
  const plan = recipe.Finish();
  assert.deepEqual(plan.nodes[1].parameters, { real: "2", imaginary: "0" });
  assert.deepEqual(plan.nodes[2].parameters, { real: "-6", imaginary: "0" });
  assert.deepEqual(plan.nodes[3].parameters.matrix, [{ row: 0, column: 0, value: "1/2" }, { row: 0, column: 7, value: "-3" }, { row: 7, column: 0, value: "2" }]);
});

test("symbolic FixedForward: hand-listed node order, dependencies and stages", () => {
  const recipe = create(2), stages = recipe.FixedForward(recipe.Leaf("x"));
  const expectedIds = [0, 1, 17, 110, 111, 112, 115, 116];
  stages.forEach((t, i) => recipe.Mark("stage" + i, [2, 12, 13, 14, 0, 1, 13, 1][i], t, false));
  const plan = recipe.Finish(); assert.equal(plan.nodes.length, 117); assert.deepEqual(plan.marks.map(m => m.node), expectedIds);
  assert.deepEqual(plan.nodes[1], { id: 1, op: "star", degree: 12, inputs: [0], parameters: {} });
  assert.deepEqual(plan.nodes[17], { id: 17, op: "product", degree: 13, inputs: [16, 1], parameters: { kind: "C" } });
  assert.deepEqual(plan.nodes[110].inputs, [109, 1]);
  assert.deepEqual(plan.nodes.slice(111).map(n => [n.op, n.inputs]), [["star", [110]], ["product", [16, 111]], ["star", [112]], ["scale", [113]], ["sum", [17, 114]], ["star", [115]]]);
});

test("symbolic FixedAdjoint: explicit reverse chain ordering, not a forward rewrite", () => {
  const recipe = create(1); recipe.Mark("adjoint", 2, recipe.FixedAdjoint(recipe.Leaf("x")), true);
  const plan = recipe.Finish();
  const nonSolder = plan.nodes.filter(n => !["unit", "leaf"].includes(n.op) && !(n.op === "sum" && [14, 91].includes(n.inputs.length)));
  assert.deepEqual(nonSolder.map(n => n.op), ["starAdjoint", "transpose", "starAdjoint", "starAdjoint", "transpose", "starAdjoint", "transpose", "starAdjoint", "scale", "sum"]);
  assert.deepEqual(nonSolder.filter(n => n.op === "starAdjoint").map(n => n.parameters.degree), [13, 2, 1, 14, 2]);
  assert.deepEqual(nonSolder.filter(n => n.op === "transpose").map(n => n.parameters.kind), ["C", "A", "A"]);
  assert.equal(plan.nodes.length, 118);
});

test("symbolic dual jets: complete Forward/Reverse stage degrees and lazy delta identity", () => {
  for (const [method, inputDegree, stageDegrees] of [["Forward", 2, [2, 12, 13, 14, 0, 1, 13, 1]], ["Reverse", 1, [1, 13, 2, 1, 0, 14, 12, 2, 2]]]) {
    const recipe = create(inputDegree), fixed = recipe.Fixed(recipe.Leaf("x"));
    const stages = recipe[method](fixed, recipe.Matrix("a"));
    stages.forEach((j, i) => { recipe.Mark("value" + i, stageDegrees[i], j.Value, false); recipe.Mark("delta" + i, stageDegrees[i], j.Delta, false); });
    const plan = recipe.Finish();
    assert.equal(stages.length, stageDegrees.length); assert.equal(plan.nodes.filter(n => n.op === "zero").length, 1);
    assert.equal(plan.nodes.filter(n => n.op === "unit").length, 105);
    assert.equal(plan.nodes.filter(n => n.op === "motionAdjoint").length, 0);
    assert.equal(plan.marks[1].node, plan.nodes.find(n => n.op === "zero").id);
    assert.equal(plan.nodes[1].op, "unit", "dual macros prepare Phi before Hodge");
    // Hand-enumerated C# left-to-right call order through the first dual
    // product. These expected edges are not generated by a second macro call.
    const firstDual = method === "Forward" ? [
      ["star", [0]], ["zero", []], ["star", [113]], ["motion", [0]],
      ["star", [115]], ["star", [0]], ["motion", [117]], ["scale", [118]],
      ["sum", [116, 119]], ["sum", [114, 120]], ["product", [15, 112]],
      ["product", [17, 112]], ["product", [15, 121]], ["sum", [123, 124]]
    ] : [
      ["starAdjoint", [0]], ["zero", []], ["starAdjoint", [113]],
      ["motion", [112]], ["motion", [0]], ["starAdjoint", [116]],
      ["scale", [117]], ["sum", [114, 115, 118]], ["transpose", [15, 112]],
      ["transpose", [17, 112]], ["transpose", [15, 119]], ["sum", [121, 122]]
    ];
    assert.deepEqual(plan.nodes.slice(112, 112 + firstDual.length).map(n => [n.op, n.inputs]), firstDual);
  }
});

test("symbolic C/CAdjoint: independent geometry traces and a single Phi cache", () => {
  for (const [method, inputDegree, outputDegree] of [["C", 2, 1], ["CAdjoint", 1, 2]]) {
    const recipe = create(inputDegree); recipe.Mark("answer", outputDegree, recipe[method](recipe.Leaf("x"), recipe.Matrix("a")), true);
    const plan = recipe.Finish(); assert.equal(plan.nodes.filter(n => n.op === "unit").length, 105);
    assert.equal(plan.nodes.at(-1).op, "sum"); assert.equal(plan.nodes.at(-1).inputs.length, 3);
    assert.deepEqual(plan.nodes.at(-2).parameters, { real: "-1/2", imaginary: "0" });
  }
});

test("symbolic validation rejects changed semantics, ordering, count and mark-node substitutions", () => {
  const recipe = create(1), x = recipe.Leaf("x"); recipe.Mark("answer", 1, recipe.Times(x, "-1"), true);
  const plan = recipe.Finish(), graph = graphFrom(plan); assert.equal(validateMixedRecipe(graph, plan), true);
  const changes = [
    g => { g.nodes[1].parameters.real = "0"; }, g => { g.nodes[1].inputs = [1]; },
    g => { g.nodes[1].degree = 0; }, g => { g.nodes[1].op = "sum"; },
    g => { g.nodes.push(structuredClone(g.nodes[1])); }, g => { g.nodes.reverse(); },
    g => { g.marks[0].node = 0; }, g => { g.marks[0].expanded = false; },
    g => { g.marks[0].name = "other"; }, g => { g.leaves[0].source = "observed-graph"; }
  ];
  for (const change of changes) { const bad = structuredClone(graph); change(bad); assert.throws(() => validateMixedRecipe(bad, plan)); }
  graph.nodes[1].records = 999; graph.nodes[1].sha256 = "deliberately not checked here";
  assert.equal(validateMixedRecipe(graph, plan), true, "numeric replay, not this metadata helper, validates payload evidence");
});

test("symbolic plans are immutable and reject duplicate marks, forged tensors and ceilings", () => {
  const recipe = create(1), x = recipe.Leaf("x"); recipe.Mark("x", 1, x, true);
  assert.throws(() => recipe.Mark("x", 1, x, false), /unique/);
  assert.throws(() => recipe.Mark("bad", 2, x, false), /degree/);
  const other = create(1); assert.throws(() => recipe.Sum(other.Leaf("x")), /foreign/);
  const plan = recipe.Finish(); assert.throws(() => { plan.nodes[0].parameters.id = "mutated"; }, TypeError);
  const bounded = new MixedRecipe({ nodeLimit: 1 }); bounded.Zero(); assert.throws(() => bounded.Zero(), /ceiling/);
});

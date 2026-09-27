"use strict";
// Manufactured expression metadata only. No scientific tensor, source metric,
// coefficient contraction, geometry factory or Phase627 entry point is run.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { multiindices } = require("../a68-background-recipe");
const { compileScalarSchedule } = require("../a68-scalar-schedule");
const { compileAuditConsumerSchedule } = require("../a68-audit-consumer-schedule");
const { SCHEMA, wardControlsMenu, structuredFieldMenu, wardMarkMenu, wardInputDomains, accelerationInputDomains, buildWardControlsRecipe } = require("../a68-ward-controls-recipe");
const hash = "0".repeat(64), all14 = value => Array(14).fill(value);
const jetIndex = multiindices().findIndex(m => JSON.stringify(m) === "[1,0,0,0]");
function fixture(point = 0, jet = jetIndex) {
  const roles = [["DeltaB", 1], ["DeltaBExterior", 2], ["DeltaFFixed", 2], ["DeltaCurvatureFromConnection", 2],
    ["X", 1], ["B", 1], ["F", 2], ["DX", 2], ["Q", 2], ["AdjointX", 2], ["NativeExterior", 2],
    ["K0", 1], ["K1", 1], ["K2", 1], ["G1", 1], ["G2", 1], ["G3", 1]];
  const recipe = new MixedRecipe({ leaves: roles.map(([id, degree]) => ({ id, degree, source: "manufactured/" + id, sha256: hash })),
    matrices: [{ name: "identity", matrix: Array.from({ length: 14 }, (_, row) => ({ row, column: row, value: "1" })) },
      { name: "zero", matrix: [] }], nodeLimit: 40000, markLimit: 4000 });
  const t = Object.fromEntries(roles.map(([id]) => [id, recipe.RegisterLeaf(id)])), identity = recipe.Matrix("identity"), z = recipe.Matrix("zero");
  const b = { Point: point, Frame: identity, InverseFrame: identity, Identity: identity, FramePartial: all14(z), Omega: all14(z),
    X: t.X, B: t.B, F: t.F, DX: t.DX, Q: t.Q, AdjointX: t.AdjointX, NativeExterior: t.NativeExterior,
    KInputs: [t.K0, t.K1, t.K2], GradientPieces: [t.K0, t.G1, t.G2, t.G3] };
  const g = { MetricBasis: 0, JetIndex: jet, Multiindex: multiindices()[jet], Motion: z,
    DeltaB: t.DeltaB, DeltaBExterior: t.DeltaBExterior, DeltaFFixed: t.DeltaFFixed,
    DeltaCurvatureFromConnection: t.DeltaCurvatureFromConnection, DeltaOmega: all14(z), DeltaOmegaPartial: all14(null).map(() => all14(z)) };
  const retention = Object.fromEntries(wardMarkMenu(point, jet).map((m, i) => [m.name, i % 2 === 0]));
  return { recipe, b, g, retention, leafCount: roles.length };
}
let cached;
function complete() {
  if (!cached) {
    const f = fixture(), result = buildWardControlsRecipe(f.recipe, f.b, f.g, { retention: f.retention });
    cached = { ...f, result, plan: f.recipe.Finish() };
  }
  return cached;
}
const nodeOfMark = (result, name) => result.marks.find(m => m.name === name).node;
const checkOf = (result, name) => result.checks.find(c => c.name === name).predicate;
const expressionOf = (result, name) => result.scalarRoots.find(r => r.name === name).expression;

test("fixed menu has six germs, twelve parameter contexts and forty-eight Ward evaluations", () => {
  const m = wardControlsMenu(); assert.equal(m.contexts.length, 6);
  assert.equal(m.contexts.reduce((n, c) => n + c.parameters.length, 0), 12);
  assert.equal(m.contexts.reduce((n, c) => n + c.parameters.reduce((v, p) => v + 2 * p.routes.length, 0), 0), 48);
  assert.equal(m.accelerationEvaluations, 24); assert.equal(m.originalEvaluations, 24);
  assert.deepEqual(m.contexts.map(c => [c.point, c.jetIndex, c.multiindex]), [
    [0, 4, [1, 0, 0, 0]], [0, 10, [0, 2, 0, 0]], [0, 24, [0, 3, 0, 0]],
    [1, 4, [1, 0, 0, 0]], [1, 10, [0, 2, 0, 0]], [1, 24, [0, 3, 0, 0]] ]);
  assert.ok(Object.isFrozen(m.contexts[0].parameters[0]));
});

test("all six manufactured germ headers assemble the complete fixed menu without source evaluation", () => {
  let ward = 0, acceleration = 0, original = 0, marks = 0, checks = 0;
  const names = new Set();
  for (const context of wardControlsMenu().contexts) {
    const f = fixture(context.point, context.jetIndex), result = buildWardControlsRecipe(f.recipe, f.b, f.g, { retention: f.retention });
    const plan = f.recipe.Finish();
    assert.equal(result.contextId, `point${context.point}/m0_j${context.jetIndex}`);
    assert.deepEqual(plan.marks.map(m => [m.name, m.degree]), wardMarkMenu(context.point, context.jetIndex).map(m => [m.name, m.degree]));
    for (const callback of result.callbacks) {
      const key = callback.category + "/" + callback.name; assert.ok(!names.has(key)); names.add(key);
      if (callback.category === "Ward") ward++;
      if (callback.category === "Acceleration") acceleration++;
      if (callback.category === "Original") original++;
    }
    marks += result.marks.length; checks += result.checks.length;
  }
  assert.deepEqual({ ward, acceleration, original, marks, checks }, { ward: 48, acceleration: 24, original: 24, marks: 21600, checks: 3684 });
});

test("closed structured menus hand-count all marks, scalar fields and full196 derivative slots", () => {
  for (const [category, tensors, scalars, bools] of [["Ward", 362, 32, 2], ["Acceleration", 80, 233, 1], ["Original", 96, 16, 0]]) {
    const fields = structuredFieldMenu(category);
    assert.equal(fields.filter(f => f.category === "Tensor").length, tensors);
    assert.equal(fields.reduce((n, f) => n + (f.category === "ScalarArray" ? f.length : f.category === "Scalar" ? 1 : 0), 0), scalars);
    assert.equal(fields.filter(f => f.category === "Boolean").length, bools);
  }
  const original = structuredFieldMenu("Original"); assert.ok(!original.some(f => f.name.startsWith("TopForms")));
  assert.throws(() => structuredFieldMenu("other"), /closed structured/);
  const { result, plan } = complete(); assert.equal(result.schemaVersion, SCHEMA);
  assert.equal(result.marks.length, 4 * (2 * 362 + 80 + 96));
  assert.equal(result.scalarRoots.length, 4 * (2 * 32 + 233 + 16) + 4 * 12 * 2);
  assert.equal(result.scalarRoots.filter(r => /CurrentCovariantDerivative\/\d+\/\d+$/.test(r.name)).length, 4 * 196);
  assert.deepEqual(result.marks, plan.marks); assert.equal(new Set(result.marks.map(m => m.name)).size, 3600);
  assert.ok(result.marks.some(m => m.expanded)); assert.ok(result.marks.some(m => !m.expanded));
  assert.equal(result.scope.numericChecksEvaluated, false); assert.equal(result.scope.sourceGeometryValidatedHere, false);
  assert.equal(result.scope.fullAuditLifecycle, false); assert.equal(result.scope.symmetricSecondJetsIncluded, false);
});

test("callback order is epsilon, compensated, acceleration, original then epsilon-first checks", () => {
  const { result } = complete(), n = `p0_j${jetIndex}_eta0_route0`;
  assert.equal(result.checks.length, 2 * (33 + 274)); assert.equal(result.callbacks.length, 614 + 16);
  assert.deepEqual(result.callbacks.slice(0, 8).map(c => [c.category, c.name]), [
    ["Ward", n + "_epsilon"], ["Ward", n + "_compensated"], ["Acceleration", n], ["Original", n + "_fixedNativeTangent"],
    ["Check", n + "_epsilon_densityValue"], ["Check", n + "_epsilon_densityMetric"],
    ["Check", n + "_epsilon_densityField"], ["Check", n + "_epsilon_densityMixed"] ]);
  assert.deepEqual(result.checks.slice(0, 10).map(c => c.name.replace(n + "_epsilon_", "")), ["densityValue", "densityMetric", "densityField", "densityMixed", "descendedDerivative", "curvatureSquare", "curvatureVariation", "solderFirst", "solderOuter", "solderInner"]);
  assert.deepEqual(result.structured.map(s => s.category), Array(4).fill(["Ward", "Ward", "Acceleration", "Original"]).flat());
  assert.equal(result.checks.at(-1).name, `p0_j${jetIndex}_eta1_route1_wordAcceleration_p1_s7`);
  for (const s of result.structured) assert.equal(s.metadataPath, `structured/${s.category}/${s.name}`);
});

test("all148 repeated Local obligations preserve exact Ward and acceleration role order without extra trace nodes", () => {
  const { result, plan } = complete();
  const wardRoles = [["eta", 0], ["nativeDeta", 1], ["X", 1], ["B", 1], ["F", 2], ["DX", 2],
    ["DeltaB", 1], ["DeltaFFixed", 2], ["DeltaBExterior", 2], ["DeltaCurvatureFromConnection", 2]];
  const accelerationRoles = [["eta", 0], ["nativeDeta", 1], ["DeltaB", 1], ["DeltaBExterior", 2],
    ["X", 1], ["B", 1], ["F", 2], ["DX", 2], ["Q", 2], ["AdjointX", 2],
    ["KInputs[0]", 1], ["KInputs[1]", 1], ["KInputs[2]", 1],
    ["GradientPieces[0]", 1], ["GradientPieces[1]", 1], ["GradientPieces[2]", 1], ["GradientPieces[3]", 1]];
  const expected = result.evaluations.flatMap(e => [
    ...wardRoles.map(([role, degree]) => ["wardInput/" + e.name + "_epsilon/" + role, degree]),
    ...wardRoles.map(([role, degree]) => ["wardInput/" + e.name + "_compensated/" + role, degree]),
    ...accelerationRoles.map(([role, degree]) => ["accelerationInput/" + e.name + "/" + role, degree])
  ]);
  assert.deepEqual(result.domainChecks.map(d => [d.name, d.degree]), expected);
  assert.equal(result.domainChecks.length, 148); assert.equal(result.census.localDomainObligations, 148);
  assert.equal(new Set(result.domainChecks.map(d => d.name)).size, 148);
  for (const d of result.domainChecks) {
    assert.equal(d.local, true); assert.equal(d.hAntiHermitian, true); assert.equal(d.canonicalNonzeroRecords, true);
    assert.ok(plan.nodes[d.node].degree === -1 || plan.nodes[d.node].degree === d.degree);
    assert.ok(Object.isFrozen(d));
  }
  for (let i = 0; i < 4; i++) {
    const slice = result.domainChecks.slice(i * 37, (i + 1) * 37);
    assert.equal(slice[30].node, slice[33].node, "KInputs[0]/GradientPieces[0] alias retains two Local calls");
    assert.notEqual(slice[30].name, slice[33].name);
  }
  assert.equal(result.callbacks.length, 630, "domain declarations invent no evidence callbacks");
  assert.equal(result.marks.length, 3600, "domain declarations invent no marks");
});

test("reusable domain helpers require recorded handles and never register a lazy zero", () => {
  const f = fixture(), eta = f.recipe.Unit(0, 16), deta = f.recipe.Unit(1, 2), before = f.leafCount + 2;
  assert.equal(wardInputDomains(f.recipe, f.b, f.g, eta, deta, "diagnostic/ward").length, 10);
  assert.equal(accelerationInputDomains(f.recipe, f.b, f.g, eta, deta, "diagnostic/acceleration").length, 17);
  assert.equal(f.recipe.Finish().nodes.length, before);
  const fresh = fixture(), empty = fresh.recipe.FreshZero();
  const sourceEta = fresh.recipe.Unit(0, 16), sourceDeta = fresh.recipe.Unit(1, 2), count = fresh.leafCount + 2;
  assert.throws(() => wardInputDomains(fresh.recipe, fresh.b, { ...fresh.g, DeltaB: empty }, sourceEta, sourceDeta, "unrecorded"), /recorded node/);
  assert.throws(() => wardInputDomains(fresh.recipe, fresh.b, fresh.g, sourceDeta, sourceDeta, "wrongDegree"), /form degree/);
  assert.equal(fresh.recipe.Finish().nodes.length, count, "failed domain lookup adds no node");
});

test("both eta units precede all action work and native probes preserve exact extra node sequence", () => {
  const { result, plan, leafCount } = complete();
  assert.deepEqual(plan.nodes.slice(leafCount, leafCount + 3).map(n => [n.op, n.parameters]), [
    ["unit", { form: 0, blade: 16, real: "1", imaginary: "0" }],
    ["unit", { form: 0, blade: 3, real: "1", imaginary: "0" }],
    ["unit", { form: 1, blade: 2, real: "1", imaginary: "0" }] ]);
  for (const evaluation of result.evaluations) {
    const prefix = `structured/Ward/${evaluation.name}_compensated/`, native = evaluation.nativeTangent.node;
    const tangent = nodeOfMark(result, prefix + "Tangent"), dbt = nodeOfMark(result, prefix + "DBTangent");
    assert.deepEqual(plan.nodes.slice(native, native + 5).map(n => [n.op, n.inputs]), [
      ["pullback", [tangent]], ["product", [5, tangent]], ["scale", [native + 1]], ["sum", [dbt, native + 2]], ["pullback", [native + 3]] ]);
    assert.equal(plan.nodes[native + 1].parameters.kind, "C"); assert.equal(plan.nodes[native + 2].parameters.real, "-1");
    assert.equal(evaluation.nativeDTangent.node, native + 4);
    assert.equal(plan.nodes[native + 5].op, "pullback"); assert.deepEqual(plan.nodes[native + 5].inputs, [native]);
    assert.deepEqual(plan.nodes[native + 6].inputs, [native + 4]);
    // Final acceleration current Contract operations precede the first native
    // probe. Marks between them may register lazy empties, never move probes.
    const derivative = evaluation.acceleration.CurrentCovariantDerivative[13][13];
    assert.ok(derivative.left.right.left < native);
  }
  assert.equal(result.evaluations[0].epsilon.Eta, result.evaluations[1].epsilon.Eta);
  assert.notEqual(result.evaluations[0].epsilon.Eta, result.evaluations[2].epsilon.Eta);
});

test("check descriptors bind exact marked node IDs and preserve four BiTensor components", () => {
  const { result } = complete(), n = `p0_j${jetIndex}_eta0_route0`, prefix = `structured/Ward/${n}_epsilon/Primitives/`;
  const equality = checkOf(result, n + "_epsilon_descendedDerivative");
  assert.deepEqual(equality, { kind: "all", predicates: ["Value", "H", "U", "HU"].map(field => ({ kind: "tensorEqual",
    left: { kind: "tensorNode", node: nodeOfMark(result, prefix + "descendedDerivative/" + field) },
    right: { kind: "tensorNode", node: nodeOfMark(result, prefix + "descendedDerivativeOracle/" + field) } })) });
  const word = checkOf(result, `p0_j${jetIndex}_eta0_route1_wordAccelerationJet13`);
  assert.equal(word.predicates.length, 5); assert.equal(word.predicates[4].left.length, 14);
  assert.equal(word.predicates[4].right.length, 14);
  assert.ok(result.checks.every(c => c.metadataPath === "check/" + c.name && !Object.hasOwn(c, "passed")));
});

test("scalar check AST preserves left-associated Ward sums and kinetic-only divergence", () => {
  const { result } = complete();
  for (const e of result.evaluations) for (let p = 0; p < 4; p++) {
    const mixed = checkOf(result, e.name + "_epsilonMixedWard_" + p), expression = expressionOf(result, mixed.left);
    assert.equal(expression.op, "add"); assert.equal(expression.left.op, "add");
    assert.equal(expression.left.left, e.epsilon.Literal.Mixed[p]); assert.equal(expression.left.right, e.field.Mixed[p]);
    assert.equal(expression.right, e.acceleration.Original[p]); assert.deepEqual(expressionOf(result, mixed.right), { op: "constant", value: "0" });
    const green = checkOf(result, e.name + "_accelerationGreen_" + p), right = expressionOf(result, green.right);
    assert.equal(expressionOf(result, green.left), e.acceleration.Original[p]); assert.equal(right.left, e.acceleration.Euler[p]);
    if (p === 1) assert.equal(right.right, e.acceleration.Divergence); else assert.deepEqual(right.right, { op: "constant", value: "0" });
  }
  assert.equal(result.geometry.length, 1); assert.equal(result.geometry[0].id, "background.Frame");
});

test("all retained and check scalar expressions compile independently with global release scheduling", () => {
  const { result, plan } = complete();
  const schedule = compileScalarSchedule(plan, result.scalarRoots, result.geometry, {
    tensorNodes: 40000, tensorEdges: 100000, tensorMarks: 4000, scalarNodes: 20000, scalarEdges: 40000, roots: 2000,
    geometryMatrices: 1, geometryEntries: 100000, maxDepth: 128, stringCharacters: 500000, rationalCharacters: 100 });
  assert.equal(schedule.roots.length, 1348); assert.equal(schedule.rootComparisons.length, 1348);
  assert.equal(schedule.releaseByStep.flat().length, plan.nodes.length);
  // Original pieces use 1+2+2+1 Pair expressions; Euler four,
  // current fourteen, derivative fourteen-by-fourteen with two per slot.
  assert.equal(schedule.nodes.filter(n => n.op === "pair").length, 4 * (6 + 4 + 14 + 392));
  assert.equal(schedule.scope.numericReplayImplemented, false);
});

test("complete Ward consumer plan retains cross-route tensors and cached roots for all614 checks", () => {
  const { result, plan } = complete();
  const schedule = compileAuditConsumerSchedule({ tensorPlan: plan, namedRoots: result.scalarRoots, geometry: result.geometry,
    checks: result.checks, domainChecks: result.domainChecks, error: null,
    scheduleLimits: { tensorNodes: 40000, tensorEdges: 100000, tensorMarks: 4000, scalarNodes: 20000, scalarEdges: 40000, roots: 2000,
      geometryMatrices: 1, geometryEntries: 100000, maxDepth: 128, stringCharacters: 500000, rationalCharacters: 100 },
    limits: { consumers: 1000, expressionNodes: 10000, expressionEdges: 10000, tensorReferences: 10000, scalarReferences: 10000,
      stringCharacters: 1000000, maxDepth: 64, rationalCharacters: 100, retainedRootValues: 2000 } });
  assert.equal(schedule.consumers.length, 762); assert.equal(schedule.consumers.filter(c => c.kind === "domain").length, 148);
  assert.equal(schedule.scalarSchedule.roots.length, 1348);
  assert.ok(schedule.maximumRetainedRootValues > 100, "complete early route values survive for late comparisons");
  assert.equal(schedule.captureRootsByStep.flat().length, new Set(schedule.consumers.flatMap(c => c.scalarRoots)).size);
  assert.deepEqual([...schedule.captureRootsByStep.flat()].sort(), [...schedule.releaseRootsByStep.flat()].sort());
  assert.ok(schedule.tensorLastUse.some((last, i) => last > schedule.scalarSchedule.tensorLastUse[i]));
  assert.equal(schedule.scope.runtimeIntegrated, false); assert.equal(schedule.scope.numericPredicatesEvaluated, false);
});

test("outside-menu headers, inconsistent jet identity and foreign handles fail closed", () => {
  for (const mutate of [f => { f.b.Point = 2; }, f => { f.g.MetricBasis = 1; }, f => { f.g.JetIndex = 0; },
    f => { f.g.Multiindex = [0, 2, 0, 0]; }, f => { f.g.DeltaOmegaPartial = []; },
    f => { f.g.DeltaB = new MixedRecipe().FreshZero(); }]) {
    const f = fixture(); mutate(f); assert.throws(() => buildWardControlsRecipe(f.recipe, f.b, f.g, { retention: f.retention }));
  }
});

test("retention is exact own-data policy with no missing, extra, nonboolean or getter entries", () => {
  for (const mutate of [f => { delete f.retention[Object.keys(f.retention)[0]]; }, f => { f.retention.extra = true; },
    f => { f.retention[Object.keys(f.retention)[0]] = 1; },
    f => { Object.defineProperty(f.retention, Object.keys(f.retention)[0], { enumerable: true, get() { throw Error("must not read getter"); } }); }]) {
    const f = fixture(); mutate(f);
    assert.throws(() => buildWardControlsRecipe(f.recipe, f.b, f.g, { retention: f.retention }), /retention|own data/);
    assert.equal(f.recipe.Finish().nodes.length, f.leafCount, "retention rejection precedes all Ward node creation");
  }
  const f = fixture(); let called = false;
  Object.defineProperty(f.g, "Multiindex", { enumerable: true, get() { called = true; return [1, 0, 0, 0]; } });
  assert.throws(() => buildWardControlsRecipe(f.recipe, f.b, f.g, { retention: f.retention }), /own data/); assert.equal(called, false);
});

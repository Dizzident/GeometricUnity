"use strict";
// Entirely manufactured symbolic plans. No GU coefficient, numerical wedge,
// source geometry, scientific factory or phase executable is evaluated.
const test = require("node:test"), assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { compileAuditConsumerSchedule } = require("../a68-audit-consumer-schedule");
const { SCHEMA, secondJetMenu, secondJetMarkMenu, secondJetDiagnosticMenu, buildSymmetricSecondJetImage,
  buildSecondJetControlsRecipe, buildSecondJetDiagnosticRecipe, secondJetProofNotes } = require("../a68-second-jet-recipe");
const policy = () => Object.fromEntries(secondJetMarkMenu().map((item, i) => [item.name, i % 2 === 0]));
function fixture(options = {}) {
  const recipe = new MixedRecipe({ nodeLimit: 3600, markLimit: 420, ...options });
  return { recipe, retention: policy() };
}
let cached;
function complete() {
  if (!cached) { const f = fixture(), result = buildSecondJetControlsRecipe(f.recipe, { retention: f.retention }); cached = { result, plan: f.recipe.Finish() }; }
  return cached;
}

test("closed menu is two controls by all105 symmetric slots by two innermost routes", () => {
  const menu = secondJetMenu(), marks = secondJetMarkMenu();
  assert.equal(menu.length, 420); assert.equal(new Set(menu.map(item => item.name)).size, 420);
  assert.deepEqual(menu.slice(0, 4), [
    { name: "eta0_second_0_0_route0", coefficient: 0, first: 0, second: 0, route: 0, diagonal: true },
    { name: "eta0_second_0_0_route1", coefficient: 0, first: 0, second: 0, route: 1, diagonal: true },
    { name: "eta0_second_0_1_route0", coefficient: 0, first: 0, second: 1, route: 0, diagonal: false },
    { name: "eta0_second_0_1_route1", coefficient: 0, first: 0, second: 1, route: 1, diagonal: false } ]);
  assert.equal(menu[209].name, "eta0_second_13_13_route1"); assert.equal(menu[210].name, "eta1_second_0_0_route0");
  assert.equal(menu.at(-1).name, "eta1_second_13_13_route1");
  for (let coefficient = 0; coefficient < 2; coefficient++) for (let route = 0; route < 2; route++) {
    const group = menu.filter(item => item.coefficient === coefficient && item.route === route);
    assert.equal(group.length, 105); assert.equal(group.filter(item => item.diagonal).length, 14);
    assert.equal(group.filter(item => !item.diagonal).length, 91);
    assert.deepEqual(group.filter(item => item.diagonal).map(item => item.first), Array.from({ length: 14 }, (_, i) => i));
  }
  assert.deepEqual(marks, menu.map(item => ({ name: "secondJet/" + item.name, degree: 2, requiredExpanded: null })));
  assert.ok(Object.isFrozen(menu[0])); assert.ok(Object.isFrozen(marks));
});

test("hand node census retains all literal products, even diagonal or cancelling images", () => {
  const { result, plan } = complete(); assert.equal(result.schemaVersion, SCHEMA);
  // 56 diagonal images each2 units+2 products;364 off-diagonal images each
  //4 units+4 products+1 sum; the two eta units are constructed first.
  assert.equal(plan.nodes.length, 2 + 56 * 4 + 364 * 9);
  const counts = {}; for (const node of plan.nodes) counts[node.op] = (counts[node.op] ?? 0) + 1;
  assert.deepEqual(counts, { unit: 2 + 56 * 2 + 364 * 4, product: 56 * 2 + 364 * 4, sum: 364 });
  assert.deepEqual(plan.nodes.slice(0, 2), [
    { id: 0, op: "unit", degree: 0, inputs: [], parameters: { form: 0, blade: 16, real: "1", imaginary: "0" } },
    { id: 1, op: "unit", degree: 0, inputs: [], parameters: { form: 0, blade: 3, real: "1", imaginary: "0" } } ]);
  assert.deepEqual(result.coefficients, [{ kind: "tensorNode", node: 0 }, { kind: "tensorNode", node: 1 }]);
  assert.equal(result.census.emittedNodes, 3502); assert.equal(result.census.emittedUnitNodes, counts.unit);
  assert.equal(result.census.emittedProductNodes, counts.product); assert.equal(result.census.emittedSumNodes, counts.sum);
  assert.ok(result.images.every(image => plan.nodes[image.node].degree === 2));
  assert.ok(!plan.nodes.some(node => node.op === "zero" || node.op === "scale"));
});

test("every node follows exact ordered source operands without diagonal doubling or route deduplication", () => {
  const { result, plan } = complete(); let start = 2;
  const unit = (id, axis) => ({ id, op: "unit", degree: 1, inputs: [], parameters: { form: 1 << axis, blade: 0, real: "1", imaginary: "0" } });
  const product = (id, left, right) => ({ id, op: "product", degree: 2, inputs: [left, right], parameters: { kind: "W" } });
  for (const image of result.images) {
    const coefficient = image.coefficient;
    const expected = [unit(start, image.first), unit(start + 1, image.second), product(start + 2, start, start + 1), product(start + 3, start + 2, coefficient)];
    if (!image.diagonal) expected.push(unit(start + 4, image.second), unit(start + 5, image.first),
      product(start + 6, start + 4, start + 5), product(start + 7, start + 6, coefficient),
      { id: start + 8, op: "sum", degree: 2, inputs: [start + 3, start + 7], parameters: {} });
    assert.deepEqual(plan.nodes.slice(start, start + expected.length), expected);
    assert.equal(image.node, start + expected.length - 1); assert.equal(image.coefficientNode, coefficient);
    start += expected.length;
  }
  assert.equal(start, plan.nodes.length);
  for (let i = 0; i < result.images.length; i += 2) {
    assert.equal(result.images[i].route, 0); assert.equal(result.images[i + 1].route, 1);
    assert.notEqual(result.images[i].node, result.images[i + 1].node);
  }
});

test("420 complete-tensor marks alternate with exact same-node tensorZero check callbacks", () => {
  const { result, plan } = complete();
  assert.equal(result.callbacks.length, 840); assert.equal(result.checks.length, 420); assert.deepEqual(result.marks, plan.marks);
  for (let i = 0; i < 420; i++) {
    const image = result.images[i], mark = result.marks[i], check = result.checks[i];
    assert.deepEqual(result.callbacks[2 * i], { category: "SecondJet", name: image.name, degree: 2, expanded: null, length: null });
    assert.deepEqual(result.callbacks[2 * i + 1], { category: "Check", name: image.name, degree: null, expanded: null, length: null });
    assert.deepEqual(mark, { name: "secondJet/" + image.name, degree: 2, node: image.node, expanded: i % 2 === 0 });
    assert.deepEqual(check, { name: image.name, metadataPath: "check/" + image.name,
      predicate: { kind: "tensorZero", tensor: { kind: "tensorNode", node: image.node } } });
    assert.ok(!Object.hasOwn(check, "passed"));
  }
  assert.equal(result.scope.numericalChecksEvaluated, false); assert.equal(result.scope.arbitraryEtaEstablishedByFiniteControls, false);
  assert.equal(result.scope.coefficientDomainChecksEvaluated, false); assert.equal(result.scope.outerLifecycleClosed, false);
});

test("dedicated secondJets diagnostic owns Begin/End and the unchanged full control DAG", () => {
  const f = fixture(), result = buildSecondJetDiagnosticRecipe(f.recipe, { retention: f.retention }), plan = f.recipe.Finish();
  const interior = complete(); assert.deepEqual(plan, interior.plan);
  assert.equal(result.contextId, "diagnostic/secondJets"); assert.equal(result.callbacks.length, 842);
  assert.deepEqual(result.callbacks[0], { category: "Begin", name: "$", degree: null, expanded: null, length: null });
  assert.deepEqual(result.callbacks.at(-1), { category: "End", name: "$", degree: null, expanded: null, length: null });
  assert.deepEqual(result.callbacks.slice(1, -1), interior.result.callbacks);
  assert.equal(result.scope.completeDiagnosticLifecycleRecipe, true); assert.equal(result.scope.outerLifecycleClosed, false);
  assert.equal(result.scope.numericalChecksEvaluated, false);
  const names = secondJetMenu().map(item => item.name), menu = secondJetDiagnosticMenu();
  assert.deepEqual(menu, { id: "secondJets", tensors: [], scalarArrays: [], originalActions: [], wardActions: [], accelerations: [],
    secondJets: names, checks: names, backgrounds: 0, germs: 0, variationRows: 0 });
  assert.deepEqual(result.diagnosticMenu, menu); assert.notEqual(menu.secondJets, menu.checks);
  assert.equal(plan.leaves.length, 0); assert.equal(result.geometry.length, 0); assert.equal(plan.nodes.length, 3502);
});

test("every Local invocation remains an unevaluated canonical real-domain coefficient obligation", () => {
  const { result } = complete(); assert.equal(result.domainChecks.length, 420);
  result.domainChecks.forEach((domain, i) => assert.deepEqual(domain, {
    name: "secondJetCoefficient/" + result.images[i].name, node: result.images[i].coefficientNode,
    degree: 0, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true
  }));
  assert.equal(new Set(result.domainChecks.map(d => d.name)).size, 420);
  assert.deepEqual(result.scalarRoots, []); assert.deepEqual(result.geometry, []);
});

test("all zero and domain descriptors compile under the current closed consumer language", () => {
  const { result, plan } = complete();
  const out = compileAuditConsumerSchedule({ tensorPlan: plan, namedRoots: result.scalarRoots, geometry: result.geometry,
    checks: result.checks, domainChecks: result.domainChecks,
    scheduleLimits: { tensorNodes: 3600, tensorEdges: 7000, tensorMarks: 420, scalarNodes: 1, scalarEdges: 1, roots: 1,
      geometryMatrices: 1, geometryEntries: 1, maxDepth: 16, stringCharacters: 500000, rationalCharacters: 10 },
    limits: { consumers: 840, expressionNodes: 500, expressionEdges: 1, tensorReferences: 840, scalarReferences: 1,
      stringCharacters: 500000, maxDepth: 16, rationalCharacters: 10, retainedRootValues: 1 } });
  assert.equal(out.consumers.length, 840); assert.equal(out.usage.tensorReferences, 840);
  assert.equal(out.consumers.filter(c => c.kind === "domain").length, 420);
  assert.equal(out.consumers.filter(c => c.kind === "check").length, 420);
  for (let i = 0; i < 420; i++) {
    const c = out.consumers[420 + i]; assert.deepEqual(c.tensorInputs, [result.images[i].node]);
    assert.equal(c.readyAfter, result.images[i].node); assert.equal(out.tensorLastUse[result.images[i].node], result.images[i].node);
  }
  assert.equal(out.releaseByStep.flat().length, plan.nodes.length); assert.equal(out.maximumRetainedRootValues, 0);
  assert.equal(out.scope.numericPredicatesEvaluated, false);
});

test("standalone macro preserves arbitrary zero-form identity and lazy empty registration timing", () => {
  const r = new MixedRecipe(), coefficient = r.Unit(0, 0, "0", "1");
  const literal = buildSymmetricSecondJetImage(r, 2, 9, coefficient), oracle = buildSymmetricSecondJetImage(r, 2, 9, coefficient, true);
  const literalId = r.RecordedNode(literal), oracleId = r.RecordedNode(oracle), plan = r.Finish();
  assert.equal(literalId, 9); assert.equal(oracleId, 18); assert.deepEqual(plan.nodes[4].inputs, [3, 0]);
  assert.deepEqual(plan.nodes[8].inputs, [7, 0]); assert.deepEqual(plan.nodes[13].inputs, [12, 0]);
  assert.deepEqual(plan.nodes[17].inputs, [16, 0]);
  // This is a manufactured descriptor for a central imaginary coefficient,
  // not numerical validation of its real-domain sign or of the wedge output.
  const emptyRecipe = new MixedRecipe(), empty = emptyRecipe.FreshZero();
  const image = buildSymmetricSecondJetImage(emptyRecipe, 3, 3, empty), imageId = emptyRecipe.RecordedNode(image), emptyPlan = emptyRecipe.Finish();
  assert.equal(imageId, 4); assert.deepEqual(emptyPlan.nodes.map(n => n.op), ["unit", "unit", "product", "zero", "product"]);
  assert.equal(emptyPlan.nodes[4].degree, -1); assert.deepEqual(emptyPlan.nodes[4].inputs, [2, 3]);
});

test("building within an existing session uses absolute nodes and does not finish the caller session", () => {
  const f = fixture({ nodeLimit: 3700 }); for (let i = 0; i < 5; i++) f.recipe.Unit(1, 1);
  const result = buildSecondJetControlsRecipe(f.recipe, { retention: f.retention });
  assert.deepEqual(result.coefficients, [{ kind: "tensorNode", node: 5 }, { kind: "tensorNode", node: 6 }]);
  assert.equal(result.images[0].node, 10); const tail = f.recipe.Unit(2, 2); assert.equal(f.recipe.RecordedNode(tail), 3507);
  assert.equal(f.recipe.Finish().nodes.length, 3508);
});

test("indices, route flags, degree and session identity fail before appending macro work", () => {
  for (const [first, second, oracle] of [[-1, 1, false], [0, 14, false], [2, 1, false], [0.5, 1, false], [-0, 0, false], [0, 0, 1]]) {
    const r = new MixedRecipe(), coefficient = r.Unit(0, 16);
    assert.throws(() => buildSymmetricSecondJetImage(r, first, second, coefficient, oracle)); assert.equal(r.Finish().nodes.length, 1);
  }
  const r = new MixedRecipe(), bad = r.Unit(1, 16);
  assert.throws(() => buildSymmetricSecondJetImage(r, 0, 0, bad), /zero-form/); assert.equal(r.Finish().nodes.length, 1);
  const foreign = new MixedRecipe(), other = new MixedRecipe();
  assert.throws(() => buildSymmetricSecondJetImage(foreign, 0, 0, other.FreshZero()), /foreign/); assert.equal(foreign.Finish().nodes.length, 0);
});

test("exact own-data retention rejects holes, extras, symbols, nonbooleans and accessors before units", () => {
  for (const mutate of [p => { delete p[Object.keys(p)[0]]; }, p => { p.extra = true; }, p => { p[Symbol("extra")] = false; },
    p => { p[Object.keys(p)[0]] = "true"; },
    p => { Object.defineProperty(p, Object.keys(p)[0], { enumerable: true, get() { throw Error("getter ran"); } }); },
    p => { Object.defineProperty(p, Object.keys(p)[0], { enumerable: false, value: false }); }]) {
    const f = fixture(); mutate(f.retention);
    assert.throws(() => buildSecondJetControlsRecipe(f.recipe, { retention: f.retention }), /retention/);
    assert.equal(f.recipe.Finish().nodes.length, 0);
  }
  for (const [options, error] of [[{ nodeLimit: 3501 }, /node ceiling/], [{ markLimit: 419 }, /unique typed mark descriptor/]]) {
    const f = fixture(options); assert.throws(() => buildSecondJetControlsRecipe(f.recipe, { retention: f.retention }), error);
  }
});

test("top-level retention must be an own data option, never inherited or getter-evaluated", () => {
  for (const wrapper of [buildSecondJetControlsRecipe, buildSecondJetDiagnosticRecipe]) {
    const f = fixture(); let invoked = false;
    const accessor = { get retention() { invoked = true; throw Error("getter invoked"); } };
    assert.throws(() => wrapper(f.recipe, accessor), /own-data retention option/); assert.equal(invoked, false);
    assert.throws(() => wrapper(f.recipe, Object.create({ retention: f.retention })), /own-data retention option/);
    assert.equal(f.recipe.Finish().nodes.length, 0);
  }
});

test("conditional proof states full ordinary Hessian cancellation without claiming zero covariant Hessian", () => {
  const notes = secondJetProofNotes(); assert.ok(Object.isFrozen(notes.steps));
  assert.match(notes.status, /not inferred from the finite controls/);
  assert.match(notes.assumptions.join(" "), /holonomic native/); assert.match(notes.assumptions.join(" "), /characteristic zero/);
  assert.match(notes.steps[0], /\(1\/2\)/); assert.match(notes.steps[1], /once, not twice/);
  assert.match(notes.steps[2], /same right-hand coefficient/); assert.match(notes.steps[3], /central imaginary/);
  assert.match(notes.steps[4], /graded commutator/); assert.match(notes.steps[4], /\[F,eta\]/);
  assert.match(notes.notEstablished.join(" "), /does not imply.*zero covariant Hessian/);
  assert.match(notes.notEstablished.join(" "), /nonholonomic/); assert.match(notes.notEstablished.join(" "), /not a claim.*constant/);
});

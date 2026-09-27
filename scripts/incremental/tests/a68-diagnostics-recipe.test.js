"use strict";
// Manufactured identity/zero geometry descriptors and opaque curvature leaves.
// No source geometry factories, scientific tensor values or phase entry run.
const test = require("node:test"), assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { BACKGROUND_SCHEMA, GERM_SCHEMA } = require("../a68-background-recipe");
const { compileAuditConsumerSchedule } = require("../a68-audit-consumer-schedule");
const { SCHEMA, diagnosticMarkMenu, diagnosticCheckMenu, buildGradeTenDiagnosticRecipe, buildAccelerationDiagnosticRecipe } = require("../a68-diagnostics-recipe");
const { contextMetadataPlan, createContextMetadataComparator } = require("../a68-context-metadata");
const hash = "0".repeat(64), fourteen = value => Array.from({ length: 14 }, () => value), clone = x => JSON.parse(JSON.stringify(x));
const constants = ["0", "1", "-1", "-1/2", "1/2", "1/3", "907712", "-2", "-24", "-3/32"];
const matrices = [{ name: "identity", matrix: Array.from({ length: 14 }, (_, row) => ({ row, column: row, value: "1" })) }, { name: "zero", matrix: [] }];
const provenance = [{ id: "manufactured", source: "synthetic-metadata-only", sha256: hash }];
const backgroundGeometry = point => ({ schemaVersion: BACKGROUND_SCHEMA, point, provenance, matrices: {
  Frame: "identity", InverseFrame: "identity", Identity: "identity", FrameLift: fourteen("zero"), FramePartial: fourteen("zero"), Omega: fourteen("zero"), InversePartial: fourteen("zero"), ConnectionInFrame: fourteen("zero") } });
const germGeometry = (point, metricBasis, jetIndex) => ({ schemaVersion: GERM_SCHEMA, point, metricBasis, jetIndex, provenance, matrices: {
  Motion: "zero", DeltaFrame: "zero", MotionPartial: fourteen("zero"), MotionCovariant: fourteen("zero"), DeltaOmega: fourteen("zero"), DeltaOmegaPartial: fourteen(null).map(() => fourteen("zero")) } });
const leaf = id => ({ id, degree: 2, source: "manufactured/" + id, sha256: hash });
function validator(expected) {
  const snapshot = clone(expected);
  return (descriptor, actual) => {
    assert.deepEqual(descriptor, snapshot); assert.ok(Object.isFrozen(descriptor)); assert.ok(Object.isFrozen(actual));
    for (const [name, entries] of Object.entries(actual)) assert.deepEqual(entries, matrices.find(m => m.name === name).matrix);
    return true;
  };
}
function fixture(diagnostic = "grade10") {
  const point = diagnostic === "grade10" ? 0 : 1, geometry = backgroundGeometry(point), count = diagnostic === "grade10" ? 4 : 1;
  const background = { geometry, validateGeometry: validator(geometry), curvatureLeaf: "curvature" }, germs = [];
  for (let basis = 0; basis < count; basis++) {
    const geometry = germGeometry(point, basis, diagnostic === "grade10" ? 4 : 10);
    germs.push({ geometry, validateGeometry: validator(geometry), adaptedLeaf: "adapted" + basis, oracleLeaf: "oracle" + basis });
  }
  const recipe = new MixedRecipe({ matrices, constants, leaves: [leaf("curvature"), ...germs.flatMap(g => [leaf(g.adaptedLeaf), leaf(g.oracleLeaf)])], nodeLimit: 100000, markLimit: 5000 });
  const retention = Object.fromEntries(diagnosticMarkMenu(diagnostic).map(m => [m.name, m.requiredExpanded ?? false]));
  const options = diagnostic === "grade10" ? { background, germs, retention } : { background, germ: germs[0], retention };
  return { recipe, options, diagnostic };
}
const build = f => (f.diagnostic === "grade10" ? buildGradeTenDiagnosticRecipe : buildAccelerationDiagnosticRecipe)(f.recipe, f.options);
const cache = new Map();
function complete(diagnostic) {
  if (!cache.has(diagnostic)) { const f = fixture(diagnostic), result = build(f); cache.set(diagnostic, { result, plan: f.recipe.Finish() }); }
  return cache.get(diagnostic);
}
const check = (r, name) => r.checks.find(c => c.name === name).predicate;
const expression = (r, name) => r.scalarRoots.find(v => v.name === name).expression;
const mark = (r, name) => r.marks.find(v => v.name === name);

test("complete manufactured diagnostics match fixed-edge composition and owned-zero node bounds", () => {
  const { checkFixtureTopology } = require("./a68-topology-fixture-check");
  const counts = { frameNonzeros: 14, deltaFrameNonzeros: 0, framePartialNonzeros: 0,
    nativeCoefficientEvents: 182, nativeChangedEvents: 0, nativeMetricEvents: 0 };
  for (const diagnostic of ["grade10", "acceleration"]) {
    const { result, plan } = complete(diagnostic); checkFixtureTopology(plan, result.contextId, counts);
  }
});

test("complete diagnostic scalar/flag/check metadata declarations bind without orphan roots", () => {
  for (const diagnostic of ["grade10", "acceleration"]) {
    const { result, plan } = complete(diagnostic), expected = contextMetadataPlan(result, plan), observed = {};
    for (const name of expected.geometryPaths) observed[name] = {}; // NOT source geometry or authentication.
    for (const check of expected.checks) observed[check.metadataPath] = true;
    for (const array of expected.scalarArrays) observed[array.metadataPath] = array.roots.map(() => "0");
    for (const row of expected.structured) observed[row.metadataPath] = {
      ...Object.fromEntries(Object.entries(row.scalarFields).map(([name, roots]) => [name, Array.isArray(roots) ? roots.map(() => "0") : "0"])), ...row.booleanFields
    };
    const comparator = createContextMetadataComparator(expected, observed, {
      snapshot: { nodes: 5000000, arraySlots: 5000000, stringCharacters: 50000000, maxDepth: 64 },
      scalarSchedule: { tensorNodes: 100000, tensorEdges: 250000, tensorMarks: 5000, scalarNodes: 20000, scalarEdges: 40000, roots: 5000,
        geometryMatrices: 4, geometryEntries: 1000000, maxDepth: 128, stringCharacters: 2000000, rationalCharacters: 100 },
      consumerSchedule: { consumers: 2000, expressionNodes: 20000, expressionEdges: 40000, tensorReferences: 20000, scalarReferences: 20000,
        stringCharacters: 2000000, maxDepth: 128, rationalCharacters: 100, retainedRootValues: 5000 },
      results: { rationalCharacters: 100, totalCharacters: 1000000 }
    });
    assert.equal(comparator.snapshot().roots, 0); assert.equal(comparator.snapshot().consumers, 0);
    assert.equal(Object.keys(comparator.geometryMetadata).length, diagnostic === "grade10" ? 5 : 2);
    assert.throws(() => comparator.finish(), /all independent roots/); // Shape is not a numerical result.
  }
});

test("complete source menu and sink mark census for both manufactured diagnostics", () => {
  for (const [id, tensorCallbacks, marks, checks, originals, wards, accelerations, scalars] of [
    ["grade10", 3082, 3948, 1019, 8, 0, 0, 10], ["acceleration", 3, 1883, 310, 2, 4, 2, 3] ]) {
    const { result, plan } = complete(id);
    assert.equal(result.schemaVersion, SCHEMA); assert.equal(result.contextId, "diagnostic/" + id);
    assert.deepEqual(result.census, { tensorCallbacks, marks, checks, originalActions: originals, wardActions: wards, accelerations, scalarArrays: scalars });
    assert.deepEqual(plan.marks, result.marks); assert.equal(new Set(plan.marks.map(m => m.name)).size, marks);
    assert.deepEqual([...result.checks.map(c => c.name)].sort(), [...diagnosticCheckMenu(id)].sort());
    assert.deepEqual(plan.marks.map(m => [m.name, m.degree]), diagnosticMarkMenu(id).map(m => [m.name, m.degree]));
    assert.deepEqual(result.callbacks[0], { category: "Begin", name: "$", degree: null, expanded: null, length: null });
    assert.equal(result.callbacks.at(-1).category, "End"); assert.equal(result.callbacks.at(-1).name, "$");
    assert.equal(result.geometryArtifacts.length, id === "grade10" ? 5 : 2);
    assert.equal(result.scope.numericChecksEvaluated, false); assert.equal(result.scope.scientificSourceClosureEstablished, false);
    assert.equal(result.scope.geometryArtifactsValidatedHere, false);
  }
});

test("grade-ten traced X and full grade-ten probe precede late background curvature registration", () => {
  const { result, plan } = complete("grade10");
  assert.deepEqual(plan.nodes.slice(0, 4).map(n => [n.op, n.parameters, n.inputs]), [
    ["unit", { form: 2, blade: 1026, real: "1", imaginary: "0" }, []],
    ["unit", { form: 8, blade: 992, real: "1", imaginary: "0" }, []], ["sum", {}, [0, 1]],
    ["unit", { form: 16, blade: 2043, real: "1", imaginary: "0" }, []] ]);
  assert.equal(mark(result, "tensor/input/X").node, 2); assert.equal(mark(result, "tensor/input/UFrame").node, 3);
  const leaves = plan.nodes.filter(n => n.op === "leaf"); assert.equal(leaves.length, 9); assert.ok(leaves[0].id > 4);
  assert.deepEqual(leaves.map(n => n.parameters.id), ["curvature", "adapted0", "oracle0", "adapted1", "oracle1", "adapted2", "oracle2", "adapted3", "oracle3"]);
  const grade = check(result, "background/gradePreservingJets"); assert.equal(grade.predicates.length, 14);
  grade.predicates.forEach((p, mu) => assert.deepEqual(p, { kind: "tensorBladeGrades", tensor: { kind: "tensorNode", node: mark(result, `background/NativePartial[${mu}]`).node }, allowed: [2, 5] }));
});

test("each grade-ten variation retains108 intermediates, all current slots and node-producing checks", () => {
  const { result, plan } = complete("grade10");
  for (let basis = 0; basis < 4; basis++) {
    const prefix = "basis" + basis;
    assert.equal(result.marks.filter(m => m.name.startsWith(`tensor/${prefix}/intermediate/`)).length, 108);
    for (const family of ["current", "currentPartial", "green"]) assert.equal(result.marks.filter(m => m.name.startsWith(`tensor/${prefix}/${family}/`)).length, 196);
    assert.equal(result.checks.filter(c => c.name.startsWith(prefix + "/current/")).length, 196);
    for (let p = 0; p < 4; p++) {
      const n = check(result, `${prefix}/nativeEuler/${p}`).right.node; assert.equal(plan.nodes[n].op, "pullback"); assert.equal(plan.nodes[plan.nodes[n].inputs[0]].op, "raise");
    }
    const diagonal = check(result, `${prefix}/current/7/7`); assert.deepEqual(diagonal.predicates[1].right, { kind: "literalZero" });
    for (const [mu, nu, sign] of [[0, 7, "1"], [7, 0, "-1"]]) {
      const n = check(result, `${prefix}/current/${mu}/${nu}`).predicates[1].right.node;
      assert.equal(plan.nodes[n].op, "scale"); assert.equal(plan.nodes[n].parameters.real, sign);
      assert.equal(plan.nodes[plan.nodes[n].inputs[0]].op, "component");
    }
    assert.equal(check(result, prefix + "/covariantDerivative").predicates.length, 14);
  }
  const pos = name => result.callbacks.findIndex(c => c.name === name);
  assert.ok(pos("basis0/current/13/13") < pos("basis0/original0")); assert.ok(pos("basis0/originalEquality/scalars") < pos("basis0/rawOriginal/0"));
});

test("weighted grade-ten scalar sums preserve source update order and degree-two projections remain literal zero", () => {
  const { result } = complete("grade10");
  for (let route = 0; route < 2; route++) for (let p = 0; p < 4; p++) {
    let e = expression(result, `scalars/combined/originalRoute${route}/${p}`);
    for (let basis = 3; basis >= 0; basis--) {
      assert.equal(e.op, "add"); assert.equal(e.right.op, "multiply");
      assert.equal(e.right.left, result.originals[basis][route].Mixed[p]); assert.equal(e.right.right.value, basis === 1 ? "1/2" : "-1/2"); e = e.left;
    }
    assert.deepEqual(e, { op: "constant", value: "0" });
  }
  for (const family of ["raw2", "fieldFirst2", "word2"]) for (let p = 0; p < 4; p++) assert.deepEqual(expression(result, `scalars/combined/projection/${family}/${p}`), { op: "constant", value: "0" });
  const motion = check(result, "combined/realizableMotion"); assert.equal(motion.predicates.length, 196);
  motion.predicates.forEach((p, i) => { assert.equal(p.expected, i === 14 ? "1" : "0"); assert.deepEqual(expression(result, p.root), {
    op: "matrixEntry", matrix: "diagnostic.grade10.combinedMotion", row: Math.floor(i / 14), column: i % 14 }); });
  assert.deepEqual(result.combinedMotion.entries, [], "the zero manufactured geometry does not satisfy the nonzero scientific forecast; no pass is fabricated");
});

test("nonzero manufactured matrix combination is reconstructed from all four signed geometry inputs", () => {
  const f = fixture("grade10");
  for (const germ of f.options.germs) { germ.geometry.matrices.Motion = "identity"; germ.validateGeometry = validator(germ.geometry); }
  const result = build(f);
  // (-1/2 +1/2 -1/2 -1/2) I = -I; no tensor coefficient is evaluated.
  assert.deepEqual(result.combinedMotion.entries, Array.from({ length: 14 }, (_, row) => ({ row, column: row, value: "-1" })));
  assert.equal(check(result, "combined/realizableMotion").predicates[14].expected, "1", "forecast stays fixed rather than fitted to input geometry");
  assert.equal(f.recipe.Finish().marks.length, 3948);
});

test("all late grade-ten hand anchors retain complex equality and their otherwise unmarked nodes", () => {
  const { result, plan } = complete("grade10");
  for (const [name, form, blade, real] of [["reverseCoefficient", 24, 1051, "-2"], ["forwardCoefficient", 1, 1026, "2"], ["nonzeroV2", 5, 6, "-24"]]) {
    const c = check(result, "hand/" + name); assert.equal(c.kind, "complexCoefficientEqualsConstant");
    assert.equal(c.real, real); assert.equal(c.imaginary, "0"); assert.equal(c.coefficient.form, form); assert.equal(c.coefficient.blade, blade);
  }
  for (const [name, op, scalar] of [["motionX", "unit", null], ["crossQ", "scale", "-2"], ["movingAdjointPairing", "sum", null], ["adjointPhi", "scale", "-24"]]) {
    const c = check(result, "hand/" + name), n = plan.nodes[c.right.node]; assert.equal(n.op, op); if (scalar) assert.equal(n.parameters.real, scalar);
  }
  assert.equal(check(result, "hand/quadraticZero").kind, "tensorZero"); assert.equal(check(result, "hand/motionCrossZero").kind, "tensorZero");
  const last = result.checks.slice(-11).map(c => c.name); assert.deepEqual(last, ["probeNorm", "quadraticZero", "motionX", "crossQ", "motionCrossZero", "reverseWord", "reverseCoefficient", "forwardCoefficient", "movingAdjointPairing", "adjointPhi", "nonzeroV2"].map(s => "hand/" + s));
});

test("acceleration diagnostic retains native-zero input, both routes, full expected W and all74 Local domains", () => {
  const { result, plan } = complete("acceleration");
  assert.deepEqual(plan.nodes.slice(0, 3).map(n => [n.op, n.parameters]), [
    ["unit", { form: 128, blade: 1, real: "1", imaginary: "0" }], ["unit", { form: 0, blade: 16, real: "1", imaginary: "0" }], ["zero", {}] ]);
  assert.equal(result.domainChecks.length, 2 * (10 + 10 + 17));
  assert.equal(result.scalarRoots.filter(r => /CurrentCovariantDerivative\/\d+\/\d+$/.test(r.name)).length, 392);
  for (let route = 0; route < 2; route++) {
    const e = result.evaluations[route], n = check(result, `route${route}/accelerationCoefficient`).right.node;
    assert.deepEqual(plan.nodes.slice(n - 3, n + 1).map(v => [v.op, v.parameters]), [
      ["unit", { form: 1, blade: 128, real: "1", imaginary: "0" }], ["unit", { form: 128, blade: 1, real: "1", imaginary: "0" }], ["sum", {}], ["scale", { real: "-3/32", imaginary: "0" }] ]);
    assert.equal(e.nativeDu.node, e.nativeU.node + 4);
    for (let p = 0; p < 4; p++) {
      const mixed = expression(result, check(result, `route${route}/epsilonWard/${p}`).left); assert.equal(mixed.left.left, e.epsilon.Literal.Mixed[p]);
      assert.equal(mixed.left.right, e.original.Mixed[p]); assert.equal(mixed.right, e.acceleration.Original[p]);
      const green = expression(result, check(result, `route${route}/green/${p}`).right);
      if (p === 1) assert.equal(green.right, e.acceleration.Divergence); else assert.deepEqual(green.right, { op: "constant", value: "0" });
    }
  }
  assert.equal(check(result, "word/acceleration/scalars").predicates.length, 5);
  assert.equal(check(result, "word/acceleration/scalars").predicates[3].left.length, 14, "coordinate current comparison retained");
  assert.equal(result.checks.at(-1).name, "word/acceleration/stage/1/7");
});

test("all diagnostic consumers compile with retained roots and complete global releases", () => {
  for (const id of ["grade10", "acceleration"]) {
    const { result, plan } = complete(id);
    const schedule = compileAuditConsumerSchedule({ tensorPlan: plan, namedRoots: result.scalarRoots, geometry: result.geometry,
      checks: result.checks, domainChecks: result.domainChecks,
      scheduleLimits: { tensorNodes: 100000, tensorEdges: 250000, tensorMarks: 5000, scalarNodes: 20000, scalarEdges: 40000, roots: 5000,
        geometryMatrices: 4, geometryEntries: 1000000, maxDepth: 128, stringCharacters: 2000000, rationalCharacters: 100 },
      limits: { consumers: 2000, expressionNodes: 20000, expressionEdges: 40000, tensorReferences: 20000, scalarReferences: 20000,
        stringCharacters: 2000000, maxDepth: 128, rationalCharacters: 100, retainedRootValues: 5000 } });
    assert.equal(schedule.consumers.length, result.checks.length + result.domainChecks.length);
    assert.equal(schedule.releaseByStep.flat().length, plan.nodes.length);
    assert.equal(schedule.scope.numericPredicatesEvaluated, false); assert.ok(schedule.maximumRetainedRootValues > 0);
  }
});

test("wrong diagnostic point/basis/jet/leaf alias and weakened retention fail before node creation", () => {
  for (const mutate of [o => { o.background.geometry.point = 1; }, o => { o.germs[2].geometry.metricBasis = 1; },
    o => { o.germs[0].geometry.jetIndex = 10; }, o => { o.germs[1].adaptedLeaf = o.germs[0].adaptedLeaf; },
    o => { o.retention["tensor/input/X"] = false; }, o => { delete o.retention["tensor/input/X"]; }, o => { o.retention.extra = true; }]) {
    const f = fixture(); mutate(f.options); assert.throws(() => build(f));
    // Declared leaves intentionally remain unregistered after early rejection;
    // the first fresh node must still receive ID zero.
    assert.equal(f.recipe.RecordedNode(f.recipe.Unit(1, 0)), 0);
  }
});

test("source validators cannot mutate later descriptor/leaf/policy snapshots and getters never run", () => {
  const f = fixture("acceleration"), old = f.options.background.validateGeometry;
  f.options.background.validateGeometry = (...args) => {
    f.options.germ.geometry.matrices.Motion = "unknown"; f.options.germ.adaptedLeaf = "unknown";
    f.options.retention["tensor/input/X"] = false; return old(...args);
  };
  const result = build(f); assert.equal(result.geometryArtifacts[1].descriptor.matrices.Motion, "zero");
  assert.equal(mark(result, "tensor/input/X").expanded, true); assert.equal(f.recipe.Finish().leaves[1].id, "adapted0");
  const g = fixture(); let invoked = false;
  Object.defineProperty(g.options.germs, "0", { enumerable: true, get() { invoked = true; throw Error("getter ran"); } });
  assert.throws(() => build(g), /own data/); assert.equal(invoked, false);
  const h = fixture(); h.options.background.validateGeometry = () => false;
  assert.throws(() => build(h), /source closure rejected/);
});

"use strict";

// Synthetic identity/zero MATRIX metadata and opaque manufactured leaf IDs.
// No numerical geometry, tensor coefficients, source P5, or study is executed.
const test = require("node:test");
const assert = require("node:assert/strict");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { BACKGROUND_SCHEMA, GERM_SCHEMA, baselineRoles, multiindices } = require("../a68-background-recipe");
const { pointMarkMenu, germMarkMenu, buildAuditPointContext, buildAuditGermContext, exteriorComponent } = require("../a68-audit-context-recipe");
const { compileAuditConsumerSchedule } = require("../a68-audit-consumer-schedule");
const { wardControlsMenu, wardMarkMenu } = require("../a68-ward-controls-recipe");
const { contextMetadataPlan, createContextMetadataComparator } = require("../a68-context-metadata");
const { deriveCaptureShape } = require("../a68-capture-requirements");
const hash = "0".repeat(64), fourteen = value => Array.from({ length: 14 }, () => value);
const constants = ["0", "1", "-1", "-1/2", "1/2", "1/3", "907712"];
const matrices = [{ name: "identity", matrix: Array.from({ length: 14 }, (_, i) => ({ row: i, column: i, value: "1" })) }, { name: "zero", matrix: [] }];
const provenance = [{ id: "manufactured", source: "synthetic-metadata-only", sha256: hash }];
const leaf = (id, degree) => ({ id, degree, source: "manufactured/" + id, sha256: hash });
const retention = menu => Object.fromEntries(menu.map(m => [m.name, m.requiredExpanded ?? false]));
const backgroundGeometry = point => ({ schemaVersion: BACKGROUND_SCHEMA, point, provenance, matrices: {
  Frame: "identity", InverseFrame: "identity", Identity: "identity", FrameLift: fourteen("zero"), FramePartial: fourteen("zero"), Omega: fourteen("zero"), InversePartial: fourteen("zero"), ConnectionInFrame: fourteen("zero") } });
const germGeometry = (point, metricBasis, jetIndex) => ({ schemaVersion: GERM_SCHEMA, point, metricBasis, jetIndex, provenance, matrices: {
  Motion: "zero", DeltaFrame: "zero", MotionPartial: fourteen("zero"), MotionCovariant: fourteen("zero"), DeltaOmega: fourteen("zero"), DeltaOmegaPartial: fourteen(fourteen("zero")) } });
const validate = expected => (descriptor, actual) => {
  assert.deepEqual(descriptor, expected); assert.ok(Object.isFrozen(descriptor)); assert.ok(Object.isFrozen(actual));
  for (const [name, values] of Object.entries(actual)) assert.deepEqual(values, matrices.find(m => m.name === name).matrix);
  return true;
};
function pointFixture(point = 0, policy = retention(pointMarkMenu())) {
  const geometry = backgroundGeometry(point), recipe = new MixedRecipe({ leaves: [leaf("X", 1), leaf("curvature", 2)], constants, matrices });
  return { recipe, options: { xLeaf: "X", curvatureLeaf: "curvature", geometry, validateGeometry: validate(geometry), retention: policy } };
}
function germFixture(point = 0, metric = 5, jet = 0, policy = retention(germMarkMenu()), extraMatrices = []) {
  const roles = baselineRoles(), canonical = [...new Set(roles.map(r => r.canonicalId))], leafIds = Object.fromEntries(canonical.map(id => [id, "baseline/" + id]));
  const leaves = canonical.map(id => leaf(leafIds[id], roles.find(r => r.canonicalId === id).degree));
  const bg = backgroundGeometry(point), gg = germGeometry(point, metric, jet);
  const recipe = new MixedRecipe({ leaves: [...leaves, leaf("adapted", 2), leaf("oracle", 2)], constants, matrices: [...matrices, ...extraMatrices] });
  return { recipe, options: { background: { geometry: bg, validateGeometry: validate(bg), leafIds },
    germ: { geometry: gg, validateGeometry: validate(gg), adaptedLeaf: "adapted", oracleLeaf: "oracle" }, retention: policy,
    wardRetention: wardControlsMenu().contexts.some(c => c.point === point && c.metricBasis === metric && c.jetIndex === jet)
      ? retention(wardMarkMenu(point, jet)) : null } };
}
const cached = new Map();
function complete(point = 0, metric = 5, multi = [0, 0, 0, 0]) {
  const jet = multiindices().findIndex(m => JSON.stringify(m) === JSON.stringify(multi)), key = [point, metric, jet].join("/");
  if (!cached.has(key)) {
    const { recipe, options } = germFixture(point, metric, jet), result = buildAuditGermContext(recipe, options);
    const variationEnd = recipe.RecordedNode(result.variation.NativeEuler[1]);
    cached.set(key, { result, plan: recipe.Finish(), variationEnd });
  }
  return cached.get(key);
}

test("complete manufactured point and germ graphs fit the static composed topology ledger", () => {
  const { checkFixtureTopology } = require("./a68-topology-fixture-check");
  const counts = { frameNonzeros: 14, deltaFrameNonzeros: 0, framePartialNonzeros: 0,
    nativeCoefficientEvents: 182, nativeChangedEvents: 0, nativeMetricEvents: 0 };
  for (const point of [0, 1]) {
    const { recipe, options } = pointFixture(point); buildAuditPointContext(recipe, options);
    checkFixtureTopology(recipe.Finish(), "point" + point, counts);
  }
  for (const tuple of [[0, 5, [0, 0, 0, 0]], [0, 0, [1, 0, 0, 0]], [1, 0, [0, 3, 0, 0]]]) {
    const { result, plan } = complete(...tuple); checkFixtureTopology(plan, result.contextId, counts);
  }
});

test("complete ordinary and Ward germ metadata plans bind all roots checks flags and error fields", () => {
  for (const tuple of [[0, 5, [0, 0, 0, 0]], [0, 0, [1, 0, 0, 0]], [1, 0, [0, 3, 0, 0]]]) {
    const { result, plan } = complete(...tuple), expected = contextMetadataPlan(result, plan), observed = { "geometry/geometry": {} };
    for (const check of result.checks) observed[check.metadataPath] = true;
    for (const row of result.structured) observed[row.metadataPath] = {
      ...Object.fromEntries(Object.entries(row.scalarFields).map(([name, roots]) => [name, Array.isArray(roots) ? roots.map(() => "0") : "0"])), ...row.booleanFields
    };
    observed.error = Object.fromEntries(Object.keys(result.error.fields).map(name => [name, name === "nativeFirstJetNorms" ? Array(14).fill("0") : "0"]));
    const comparator = createContextMetadataComparator(expected, observed, {
      snapshot: { nodes: 5000000, arraySlots: 5000000, stringCharacters: 50000000, maxDepth: 64 },
      scalarSchedule: { tensorNodes: 100000, tensorEdges: 250000, tensorMarks: 5000, scalarNodes: 20000, scalarEdges: 40000, roots: 5000,
        geometryMatrices: 1000, geometryEntries: 1000000, maxDepth: 128, stringCharacters: 2000000, rationalCharacters: 100 },
      consumerSchedule: { consumers: 10000, expressionNodes: 100000, expressionEdges: 200000, tensorReferences: 200000, scalarReferences: 20000,
        stringCharacters: 2000000, maxDepth: 128, rationalCharacters: 100, retainedRootValues: 5000 },
      results: { rationalCharacters: 100, totalCharacters: 1000000 }
    });
    assert.equal(comparator.snapshot().roots, 0); assert.equal(comparator.snapshot().consumers, 0);
    assert.throws(() => comparator.finish(), /all independent roots/); // Metadata assembly is not a numerical replay.
  }
});

test("audit point assembly preserves74 sink marks, native checks, anchors and pending350 child contexts", () => {
  for (const point of [0, 1]) {
    const { recipe, options } = pointFixture(point), result = buildAuditPointContext(recipe, options), plan = recipe.Finish();
    assert.equal(plan.nodes.length, 1016, "outer background sink/check descriptors emit no extra primitive");
    assert.equal(result.marks.length, 74); assert.deepEqual(plan.marks, result.marks);
    assert.equal(result.checks.length, point === 0 ? 18 : 15); assert.equal(result.canonicalBaselineExports.length, 27);
    assert.deepEqual(result.callbacks.slice(0, 3).map(x => [x.category, x.name]), [["BeginPoint", "$"], ["GeometryLeaf", "curvature"], ["Background", "$"]]);
    assert.equal(result.childContexts.length, 350); assert.equal(result.childContexts[0].id, `point${point}/m0_j0`); assert.equal(result.childContexts.at(-1).id, `point${point}/m9_j34`);
    assert.ok(!result.callbacks.some(c => c.category === "EndPoint")); assert.equal(result.continuation.pending, true);
    assert.equal(result.callbacks.at(-1).category, "SealPointBackground");
    assert.equal(result.callbacks.filter(c => c.category === "SealPointBackground").length, 1);
    assert.equal(result.callbacks.length, point === 0 ? 22 : 19, "computational seal precedes all350 child callbacks");
    if (point === 0) {
      const anchor = result.checks.find(c => c.name === "nativeVectorAnchorPositive");
      assert.equal(anchor.predicate.lowerBound, "181/203327488"); assert.equal(anchor.predicate.coefficient.tensor.node, 0);
    }
    assert.equal(result.scope.numericChecksReplayed, false); assert.equal(result.scope.fullAuditRecipe, false);
  }
});

test("frame sparsity changes point copy requirements so identity fixtures cannot supply production caps", () => {
  const base = pointFixture(); buildAuditPointContext(base.recipe, base.options); const identityPlan = base.recipe.Finish();
  const frame = [...matrices[0].matrix, { row: 0, column: 1, value: "1" }].sort((a, b) => a.row * 14 + a.column - b.row * 14 - b.column);
  const geometry = backgroundGeometry(0); geometry.matrices.Frame = "manufacturedShear";
  const recipe = new MixedRecipe({ leaves: [leaf("X", 1), leaf("curvature", 2)], constants,
    matrices: [...matrices, { name: "manufacturedShear", matrix: frame }] });
  buildAuditPointContext(recipe, { xLeaf: "X", curvatureLeaf: "curvature", geometry,
    validateGeometry: (d, actual) => { assert.deepEqual(d, geometry); assert.deepEqual(actual.manufacturedShear, frame); return true; },
    retention: retention(pointMarkMenu()) });
  const limits = { nodes: 100000, arraySlots: 100000, stringCharacters: 1000000, maxDepth: 16 };
  const baseline = deriveCaptureShape(identityPlan, limits), changed = deriveCaptureShape(recipe.Finish(), limits);
  assert.equal(baseline.shape.nodes, 1016); assert.equal(changed.shape.marks, baseline.shape.marks);
  assert.ok(changed.shape.nodes > baseline.shape.nodes); assert.ok(changed.shape.inputReferences > baseline.shape.inputReferences);
  assert.ok(changed.copies.slots > baseline.copies.slots); assert.equal(changed.scope.sourceAuthenticityEstablished, false);
});

test("audit germ sink/check execution order differs correctly from set-only callback census", () => {
  const { result, plan } = complete();
  assert.deepEqual(result.callbacks.slice(0, 4).map(c => [c.category, c.name]), [["BeginGerm", "$"], ["GeometryLeaf", "deltaCurvatureAdapted"], ["GeometryLeaf", "deltaCurvatureOracle"], ["Geometry", "$"]]);
  const at = name => result.callbacks.findIndex(c => c.name === name);
  assert.ok(at("referenceCurvatureDerivative") < at("intermediate_fixed_p0_s0_value"));
  assert.ok(at("intermediate_partial_mu13") < at("nonInvariantCovariantDerivative"));
  assert.ok(at("currentPartial_13_13") < at("current")); assert.ok(at("greenCurrent") < at("completeCurrent"));
  assert.equal(result.callbacks.at(-1).category, "EndGerm"); assert.equal(result.scope.completeContextLifecycle, true);
  assert.equal(result.checks.length, 951); assert.equal(result.marks.length, 376); assert.deepEqual(plan.marks, result.marks);
  assert.equal(result.callbacks.filter(c => c.category === "Tensor").length, 370);
  assert.equal(result.domainChecks.length, 66); assert.equal(result.error.numericalReplayImplemented, false);
});

test("audit outer tail emits exactly964 independently hand-counted non-Ward primitives", () => {
  const { result, plan, variationEnd } = complete();
  // 5 moving-adjoint check +12 family totals +26 coordinate-dual primitives
  // +364 ExteriorComponent +91 symmetric sums +455 current/green assembly
  // +2 assembly seed zeros +9 first-marked family zeros =964.
  assert.equal(plan.nodes.length - variationEnd - 1, 964);
  const check = result.checks.find(c => c.name === "movingAdjointPairingMotion").predicate;
  const sum = plan.nodes[check.right.node]; assert.equal(sum.op, "sum");
  assert.equal(plan.nodes[sum.inputs[1]].op, "sum");
  assert.deepEqual(plan.nodes[sum.inputs[1]].inputs.map(id => plan.nodes[id].op), ["motion", "motion", "scale"]);
  for (const piece of ["source", "kinetic", "cubic", "mass"]) {
    const predicate = result.checks.find(c => c.name === "nativeEuler_" + piece).predicate;
    const pullback = plan.nodes[predicate.right.node]; assert.equal(pullback.op, "pullback"); assert.equal(plan.nodes[pullback.inputs[0]].op, "raise");
  }
});

test("audit current checks retain all196 nonsymmetric derivatives and14 literal diagonal zeros", () => {
  const { result, plan } = complete(), derivativeMarks = plan.marks.filter(m => m.name.startsWith("tensor/currentPartial_"));
  assert.equal(derivativeMarks.length, 196); assert.equal(new Set(derivativeMarks.map(m => m.node)).size, 196);
  assert.equal(result.checks.filter(c => c.name.startsWith("nativeJetSlot_")).length, 196);
  assert.equal(result.checks.filter(c => c.name.startsWith("symmetricNull_")).length, 91);
  for (let mu = 0; mu < 14; mu++) {
    assert.deepEqual(result.checks.find(c => c.name === `nativeJetSlot_${mu}_${mu}`).predicate.left, { kind: "literalZero" });
    assert.deepEqual(result.checks.find(c => c.name === `diagonalNull_${mu}`).predicate.tensor, { kind: "literalZero" });
  }
  for (const [mu, nu, sign] of [[0, 7, "1"], [7, 0, "-1"]]) {
    const ref = result.checks.find(c => c.name === `nativeJetSlot_${mu}_${nu}`).predicate.left;
    const scale = plan.nodes[ref.node], component = plan.nodes[scale.inputs[0]];
    assert.equal(scale.op, "scale"); assert.equal(scale.parameters.real, sign);
    assert.equal(component.op, "component"); assert.equal(component.parameters.form, 129);
  }
  assert.equal(result.census.diagonalNullControls, 14); assert.equal(result.census.symmetricNullControls, 91);
});

test("audit original family totals, native transforms and reconstructed currents are named expanded marks", () => {
  const { result, plan } = complete();
  const totals = plan.marks.filter(m => m.name.endsWith("_total")); assert.equal(totals.length, 12);
  totals.forEach(m => { assert.equal(m.expanded, true); assert.equal(plan.nodes[m.node].op, "sum"); assert.equal(plan.nodes[m.node].inputs.length, 4); });
  const current = plan.marks.find(m => m.name === "tensor/current"), green = plan.marks.find(m => m.name === "tensor/greenCurrent");
  assert.equal(current.degree, 2); assert.equal(green.degree, 2); assert.equal(current.expanded, true); assert.equal(green.expanded, true);
  assert.notEqual(current.node, green.node);
  const completeCheck = result.checks.find(c => c.name === "completeCurrent").predicate;
  assert.equal(completeCheck.predicates.length, 2); assert.equal(completeCheck.predicates[1].left.node, current.node); assert.equal(completeCheck.predicates[1].right.node, green.node);
});

test("audit third-order and pure-diffeomorphism checks are explicit predicates, not manufactured pass values", () => {
  const { result } = complete(1, 0, [3, 0, 0, 0]);
  assert.equal(result.checks.length, 956); assert.equal(result.census.thirdGermLowerPieceChecks, 3); assert.equal(result.census.pureDiffeomorphismControls, 1);
  assert.equal(result.scope.geometryPredicatesReplayed, false); assert.equal(result.scope.errorPredicatesReplayed, false);
  assert.deepEqual(result.checks.find(c => c.name === "thirdGermZeroBackgroundError").predicate.fields, ["error/raw0Error", "error/raw2Error", "error/eulerError"]);
  const pure = result.checks.find(c => c.name === "pureBaseDiffeomorphismPrincipalZero").predicate;
  assert.equal(pure.predicates[0].kind, "geometryZero"); assert.equal(pure.predicates.slice(1).every(x => x.kind === "tensorZero"), true);
  assert.ok(!JSON.stringify(result.checks).includes('"passed":true'));
});

test("audit hand-anchor branches precede complete selected Ward continuation and EndGerm", () => {
  const mass = complete(0, 0, [1, 0, 0, 0]), third = complete(1, 0, [0, 3, 0, 0]);
  for (const [built, extra, name, lastOps] of [[mass, 4, "nonzeroMassHandAnchor", ["unit", "unit", "sum", "pullback"]],
    [third, 2, "thirdSourceHandAnchor", ["unit", "pullback"]]]) {
    const { result, plan, variationEnd } = built;
    const anchorNode = result.nonWard.scalarRoots[0].expression.left;
    assert.equal(anchorNode - variationEnd, 964 + extra); assert.deepEqual(plan.nodes.slice(anchorNode - extra + 1, anchorNode + 1).map(n => n.op), lastOps);
    assert.equal(result.scalarRoots.length, 1349); assert.equal(result.nonWard.scalarRoots.length, 1);
    assert.equal(result.scalarRoots[0].name, "check/" + name + "/actual");
    assert.equal(result.scalarRoots[0].expression.op, "pair");
    assert.equal(result.wardControls.contextId, result.contextId);
    assert.deepEqual(plan.nodes.slice(anchorNode + 1, anchorNode + 4).map(n => n.op), ["unit", "unit", "unit"]);
    assert.equal(result.callbacks.filter(c => c.category === "EndGerm").length, 1);
    assert.equal(result.callbacks.at(-1).category, "EndGerm"); assert.equal(result.scope.completeContextLifecycle, true);
    assert.deepEqual(result.callbacks.slice(result.nonWard.callbacks.length, -1), result.wardControls.callbacks);
    assert.equal(result.callbacks[result.nonWard.callbacks.length - 1].name, name);
    assert.equal(result.marks.length, 3976); assert.deepEqual(plan.marks, result.marks);
    assert.equal(result.structured.length, 16); assert.equal(result.checks.length, result.nonWard.checks.length + 614);
  }
  assert.equal(mass.result.checks.find(c => c.name === "nonzeroMassHandAnchor").predicate.factor, "-907712");
  assert.equal(mass.result.checks.find(c => c.name === "nonzeroMassHandAnchor").predicate.coefficient.tensor.node, 0);
  assert.equal(third.result.checks.find(c => c.name === "thirdSourceHandAnchor").predicate.expected, "3/16");
});

test("audit error descriptor retains all19 fixed fields and all14 first-jet norm formulas without evaluation", () => {
  const { fields } = complete().result.error;
  assert.equal(Object.keys(fields).length, 19); assert.equal(fields.radius.value, "15/113464");
  assert.equal(fields.fieldError.op, "sourceBoundScalar"); assert.equal(fields.fieldError.positiveRequired, true);
  assert.equal(fields.nativeFirstJetNorms.length, 14); assert.ok(fields.nativeFirstJetNorms.every(x => x.matrix.op === "matrixProduct"));
  assert.equal(fields.motionDerivativeSum.values.length, 14); assert.equal(fields.referenceNorm.op, "tensorL1");
  assert.ok(Object.isFrozen(fields.raw0Majorant)); assert.ok(Object.isFrozen(fields.nativeFirstJetNorms));
});

test("audit retention is exact and cannot weaken mandatory expanded outputs or add unrelated marks", () => {
  for (const mutate of [x => { delete x["tensor/current"]; }, x => { x["tensor/current"] = false; },
    x => { x["tensor/currentPartial_0_0"] = true; }, x => { x.extra = false; }]) {
    const policy = retention(germMarkMenu()); mutate(policy); const { recipe, options } = germFixture(0, 5, 0, policy);
    assert.throws(() => buildAuditGermContext(recipe, options), /retention census/);
  }
  const { recipe, options } = pointFixture(); options.retention.extra = false;
  assert.throws(() => buildAuditPointContext(recipe, options), /retention census/);
});

test("audit ExteriorComponent diagonal description emits no phantom zero node", () => {
  const r = new MixedRecipe(), tensor = r.Unit(3, 0);
  const diagonal = exteriorComponent(r, tensor, 0, 0); assert.equal(r.TensorDegree(diagonal), -1);
  assert.throws(() => r.RecordedNode(diagonal), /already have a recorded node/);
  exteriorComponent(r, tensor, 1, 0); const p = r.Finish();
  assert.deepEqual(p.nodes.map(n => n.op), ["unit", "component", "scale"]); assert.equal(p.nodes[2].parameters.real, "-1");
});

test("audit callbacks cannot replace validated geometry roles or late leaf IDs used by error formulas", () => {
  const { recipe, options } = germFixture();
  const originalBackground = options.background, originalGerm = options.germ;
  const validateBackground = originalBackground.validateGeometry;
  const expectedBackground = JSON.parse(JSON.stringify(originalBackground.geometry));
  const expectedGerm = JSON.parse(JSON.stringify(originalGerm.geometry));
  originalBackground.validateGeometry = (descriptor, values) => {
    validateBackground(descriptor, values);
    originalBackground.geometry.matrices.Frame = "unvalidated-frame";
    originalBackground.geometry.matrices.FrameLift[0] = "unvalidated-lift";
    originalBackground.leafIds.X = "unvalidated-leaf";
    originalGerm.geometry.matrices.Motion = "unvalidated-motion";
    originalGerm.adaptedLeaf = "unvalidated-curvature";
    assert.deepEqual(descriptor, expectedBackground);
    return true;
  };
  originalGerm.validateGeometry = validate(expectedGerm);
  // The original background fixture validator closes over the mutable object;
  // it is invoked before the mutations above. All later uses must be snapshots.
  const result = buildAuditGermContext(recipe, options), plan = recipe.Finish();
  assert.equal(result.error.fields.nativeOneFormDualNorm.matrix.name, "identity");
  assert.equal(result.error.fields.nativeFirstJetNorms[0].matrix.right.name, "zero");
  assert.equal(result.error.fields.motionNorm.value.name, "zero");
  assert.ok(plan.leaves.some(leaf => leaf.id === "baseline/X"));
  assert.ok(plan.leaves.some(leaf => leaf.id === "adapted"));
  assert.ok(!JSON.stringify(result.error).includes("unvalidated"));
});

test("audit snapshots reject metadata and retention accessors without invoking them", () => {
  let invoked = false;
  for (const target of ["geometry", "retention"]) {
    const { recipe, options } = pointFixture();
    const object = target === "geometry" ? options.geometry.matrices : options.retention;
    const key = target === "geometry" ? "Frame" : "background/X";
    Object.defineProperty(object, key, { enumerable: true, get() { invoked = true; return target === "geometry" ? "identity" : false; } });
    assert.throws(() => buildAuditPointContext(recipe, options), /own metadata data fields/);
  }
  assert.equal(invoked, false);
});

test("complete manufactured outer context schedules all checks, domains and error consumers", () => {
  const { result, plan } = complete(0, 0, [1, 0, 0, 0]);
  const scheduled = compileAuditConsumerSchedule({ tensorPlan: plan, namedRoots: result.scalarRoots,
    geometry: result.geometry, checks: result.checks, domainChecks: result.domainChecks, error: result.error,
    scheduleLimits: { tensorNodes: 100000, tensorEdges: 500000, tensorMarks: 4000, scalarNodes: 100000, scalarEdges: 100000, roots: 2000,
      geometryMatrices: 100000, geometryEntries: 1000000, maxDepth: 64, stringCharacters: 10000000, rationalCharacters: 100 },
    limits: { consumers: 3000, expressionNodes: 100000, expressionEdges: 100000, tensorReferences: 100000, scalarReferences: 10000,
      stringCharacters: 10000000, maxDepth: 64, rationalCharacters: 100, retainedRootValues: 2000 } });
  assert.equal(scheduled.consumers.length, result.checks.length + result.domainChecks.length + 19);
  assert.equal(scheduled.consumers.filter(c => c.kind === "check").length, 952 + 614);
  assert.equal(scheduled.consumers.filter(c => c.kind === "domain").length, result.domainChecks.length);
  assert.equal(result.domainChecks.length, 214);
  assert.equal(scheduled.consumers.filter(c => c.kind === "error").length, 19);
  const mass = scheduled.consumers.find(c => c.name === "nonzeroMassHandAnchor");
  assert.equal(mass.readyAfter, result.nonWard.scalarRoots[0].expression.left);
  assert.ok(scheduled.tensorLastUse[0] >= mass.readyAfter, "X remains live through its late mass coefficient comparison and Ward uses");
  assert.ok(scheduled.tensorLastUse.some((last, i) => last > scheduled.scalarSchedule.tensorLastUse[i]));
  assert.equal(scheduled.scope.runtimeIntegrated, false);
});

test("all six selected germ lifecycles preserve exact combined counts and ordered Ward tails", () => {
  const expectedChecks = [1566, 1565, 1569, 1565, 1565, 1570]; let at = 0;
  for (const context of wardControlsMenu().contexts) {
    const f = germFixture(context.point, context.metricBasis, context.jetIndex);
    const result = buildAuditGermContext(f.recipe, f.options), plan = f.recipe.Finish();
    assert.equal(result.checks.length, expectedChecks[at++]); assert.equal(result.domainChecks.length, 214);
    assert.equal(result.marks.length, 3976); assert.equal(result.scalarRoots.length, result.nonWard.scalarRoots.length + 1348);
    assert.equal(result.structured.length, 16); assert.deepEqual(plan.marks, result.marks);
    assert.equal(result.callbacks.at(-1).category, "EndGerm");
    assert.deepEqual(result.callbacks.slice(result.nonWard.callbacks.length, -1), result.wardControls.callbacks);
    assert.deepEqual(result.geometry.map(g => g.id), ["identity", "zero", "background.Frame"]);
    assert.deepEqual(result.geometry.at(-1).entries, matrices[0].matrix);
    assert.equal(result.scope.numericChecksReplayed, false); assert.equal(result.scope.sourceClosureEstablishedHere, false);
  }
});

test("selected Ward policy is mandatory and snapshotted before validators; arbitrary hooks fail closed", () => {
  for (const mutate of [o => { delete o.wardRetention; }, o => { delete o.wardRetention[Object.keys(o.wardRetention)[0]]; },
    o => { o.wardRetention.extra = false; }, o => { o.wardRetention[Object.keys(o.wardRetention)[0]] = 1; },
    o => { o.wardHook = () => { throw Error("must not invoke"); }; }]) {
    const f = germFixture(0, 0, 4); let invoked = false;
    f.options.background.validateGeometry = () => { invoked = true; return true; }; mutate(f.options);
    assert.throws(() => buildAuditGermContext(f.recipe, f.options), /retention|Ward hooks/);
    assert.equal(invoked, false); assert.equal(f.recipe.RecordedNode(f.recipe.RegisterLeaf("baseline/X")), 0, "rejection precedes all graph nodes");
  }
  const f = germFixture(0, 0, 4), first = Object.keys(f.options.wardRetention)[0], validateBackground = f.options.background.validateGeometry;
  f.options.background.validateGeometry = (descriptor, values) => {
    f.options.wardRetention[first] = true; return validateBackground(descriptor, values);
  };
  const result = buildAuditGermContext(f.recipe, f.options);
  assert.equal(result.wardControls.marks[0].expanded, false);
  const other = germFixture(); other.options.wardRetention = {};
  assert.throws(() => buildAuditGermContext(other.recipe, other.options), /outside fixed selected menu/);
});

test("germ options never invoke inherited or own accessor fields", () => {
  let invoked = false;
  for (const target of ["required", "optional", "background", "germ"]) {
    const f = germFixture(), key = target === "required" ? "background" : target === "optional" ? "wardRetention" : "geometry";
    const object = target === "background" ? f.options.background : target === "germ" ? f.options.germ : f.options;
    delete object[key]; Object.setPrototypeOf(object, { get [key]() { invoked = true; throw Error("must not invoke"); } });
    if (target === "optional") { const result = buildAuditGermContext(f.recipe, f.options); assert.equal(result.wardControls, null); }
    else assert.throws(() => buildAuditGermContext(f.recipe, f.options), /own data option/);
  }
  const f = germFixture(0, 0, 4), key = Object.keys(f.options.wardRetention)[0];
  Object.defineProperty(f.options.wardRetention, key, { enumerable: true, get() { invoked = true; return false; } });
  assert.throws(() => buildAuditGermContext(f.recipe, f.options), /own metadata data fields/);
  assert.equal(invoked, false);
});

test("Ward scalar alias cannot overwrite a differently valued source-named error matrix", () => {
  const f = germFixture(0, 0, 4, retention(germMarkMenu()), [{ name: "background.Frame", matrix: [] }]);
  f.options.germ.geometry.matrices.Motion = "background.Frame";
  f.options.germ.validateGeometry = (descriptor, values) => {
    assert.equal(descriptor.matrices.Motion, "background.Frame"); assert.deepEqual(values["background.Frame"], []); return true;
  };
  assert.throws(() => buildAuditGermContext(f.recipe, f.options), /same scalar geometry for aliased name/);
});

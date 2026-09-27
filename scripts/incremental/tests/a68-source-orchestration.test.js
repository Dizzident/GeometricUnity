"use strict";

// Manufactured local tensors/JSON only. No source14D geometry factory, genuine
// retained polynomial reconstruction, physics driver or scientific gate runs.
const test = require("node:test"), assert = require("node:assert/strict"), crypto = require("node:crypto");
const { createCanonicalLeafClosure, snapshotCanonicalMetadata, bindVerifiedReplayExports, createVerifiedBaselineLeafBindings, createSourcePointAdapter, createSourceGermAdapter } = require("../a68-source-orchestration");
const { GeometryAlgebra } = require("../a68-geometry-algebra");
const { reconstructPolynomial } = require("../a68-polynomial-reconstruction");
const { MixedRecipe } = require("../a68-mixed-recipe");
const { baselineRoles, importMixedBaseline, canonicalBaselineImports, BACKGROUND_SCHEMA } = require("../a68-background-recipe");
const leafCaps = overrides => ({ leaves: 10, records: 100, stringCharacters: 10000, wireBytes: 10000, rationalCharacters: 100, resolutions: 10, ...overrides });
const metadataCaps = overrides => ({ nodes: 1000, arraySlots: 1000, stringCharacters: 10000, maxDepth: 16, ...overrides });
const row = (form = 1, blade = 31, real = "2/3", imaginary = "-5/7") => ({ form, blade, k0: 0, k1: 0, real, imaginary });
const leaf = (tensor = [row()], id = "manufactured/X", degree = 1) => ({ id, degree, source: "manufactured/independent-input", tensor });
const digest = value => crypto.createHash("sha256").update(JSON.stringify(value)).digest("hex");
const algebraLimits = () => ({ maxBits: 128, scalarOperations: 10000, rationalObjects: 10000, matrixObjects: 100, matrixCells: 1000, slotVisits: 10000 });

test("manufactured leaf closure preserves complete support, grade5 and both complex components", () => {
  const input = [row(1, 0, "0", "1"), row(1, 31), row(2, 16383, "-17", "3/11")];
  const c = createCanonicalLeafClosure([leaf(input)], leafCaps());
  assert.deepEqual(c.resolveLeaf(c.leaves[0]), input); assert.equal(c.leaves[0].sha256, digest(input));
  assert.equal(c.snapshot().records, 3); assert.equal(c.snapshot().wireBytes, JSON.stringify(input).length);
  assert.equal(c.scope.sourceAuthenticityEstablished, false); assert.equal(c.scope.totalProcessMemoryProved, false);
  assert.ok(Object.isFrozen(c.leaves)); assert.ok(Object.isFrozen(c.resolveLeaf(c.leaves[0])));
  input[0].imaginary = "99"; input.push(row(4)); assert.equal(c.resolveLeaf(c.leaves[0]).length, 3);
  assert.equal(c.resolveLeaf(c.leaves[0])[0].imaginary, "1");
});

test("identical content keeps adapted and oracle leaf identities distinct; empty [] hashes canonically", () => {
  const c = createCanonicalLeafClosure([leaf([], "adapted", 2), leaf([], "oracle", 2)], leafCaps());
  assert.equal(c.leaves.length, 2); assert.equal(c.leaves[0].sha256, digest([]));
  assert.equal(c.leaves[0].sha256, c.leaves[1].sha256); assert.notEqual(c.leaves[0].id, c.leaves[1].id);
  assert.notEqual(c.resolveLeaf(c.leaves[0]), c.resolveLeaf(c.leaves[1])); assert.equal(c.snapshot().wireBytes, 4);
});

test("snapshot defeats inherited toJSON; hash and import see the same plain own-data rows", () => {
  let invoked = 0; const r = Object.assign(Object.create({ toJSON() { invoked++; return row(1, 1, "999", "0"); } }), row());
  const input = [r]; Object.setPrototypeOf(input, Object.assign(Object.create(Array.prototype), { toJSON() { invoked++; return []; } }));
  const c = createCanonicalLeafClosure([leaf(input)], leafCaps()), output = c.resolveLeaf(c.leaves[0]);
  assert.equal(invoked, 0); assert.equal(c.leaves[0].sha256, digest([row()]));
  assert.equal(Object.getPrototypeOf(output), Array.prototype); assert.equal(Object.getPrototypeOf(output[0]), Object.prototype);
});

test("array or row getters are rejected without invoking their code", () => {
  let reads = 0; const r = row(); Object.defineProperty(r, "real", { enumerable: true, get() { reads++; return "1"; } });
  assert.throws(() => createCanonicalLeafClosure([leaf([r])], leafCaps()), /own data/);
  const rows = []; Object.defineProperty(rows, "0", { enumerable: true, get() { reads++; return row(); } }); Object.freeze(rows);
  assert.throws(() => createCanonicalLeafClosure([leaf(rows)], leafCaps()), /own data/); assert.equal(reads, 0);
});

test("closed shapes reject sparse/extra/symbol arrays and row fields", () => {
  for (const rows of [Array(1), Object.assign([row()], { extra: 1 }), Object.assign([row()], { [Symbol("extra")]: 1 })])
    assert.throws(() => createCanonicalLeafClosure([leaf(rows)], leafCaps()), /array|own data/);
  assert.throws(() => createCanonicalLeafClosure([leaf([{ ...row(), extra: 1 }])], leafCaps()), /closed/);
  assert.throws(() => createCanonicalLeafClosure([leaf(), leaf()], leafCaps()), /distinct/);
});

test("every invalid local degree/support/mode/zero and rational form is rejected", () => {
  for (const r of [row(3), row(1, 16384), { ...row(), k0: 1 }, { ...row(), k1: -0 }, row(1, 0, "0", "0")])
    assert.throws(() => createCanonicalLeafClosure([leaf([r])], leafCaps()));
  for (const text of ["-0", "01", "1/1", "2/4", "0/2", "1/-2", "1e3", "+1", "NaN"])
    assert.throws(() => createCanonicalLeafClosure([leaf([row(1, 31, text)])], leafCaps()), /rational/);
  assert.throws(() => createCanonicalLeafClosure([leaf([row(), row()])], leafCaps()), /ordering/);
  assert.throws(() => createCanonicalLeafClosure([leaf([row(2), row(1)])], leafCaps()), /ordering/);
});

test("leaf quotas are mandatory and charged before copying/formatting the next value", () => {
  const valid = leaf(), length = JSON.stringify(valid.tensor).length;
  const c = createCanonicalLeafClosure([valid], leafCaps({ wireBytes: length })); assert.equal(c.snapshot().wireBytes, length);
  assert.throws(() => createCanonicalLeafClosure([valid], leafCaps({ wireBytes: length - 1 })), /wireBytes/);
  assert.throws(() => createCanonicalLeafClosure([valid], leafCaps({ stringCharacters: 1 })), /stringCharacters/);
  assert.throws(() => createCanonicalLeafClosure([valid], leafCaps({ rationalCharacters: 2 })), /rational/);
  assert.throws(() => createCanonicalLeafClosure([valid, leaf([], "second")], leafCaps({ leaves: 1 })), /length/);
  assert.throws(() => createCanonicalLeafClosure([leaf([row(), row(2)])], leafCaps({ records: 1 })), /length/);
  for (const cap of [0, -1, Infinity, Number.MAX_SAFE_INTEGER + 1]) assert.throws(() => createCanonicalLeafClosure([valid], leafCaps({ records: cap })), /finite/);
  assert.throws(() => createCanonicalLeafClosure([valid], undefined));
});

test("resolver binds full declaration and poisons on mismatch or exhausted quota", () => {
  for (const replacement of [{ degree: 2 }, { source: "untrusted" }, { sha256: "0".repeat(64) }, { id: "other" }]) {
    const c = createCanonicalLeafClosure([leaf()], leafCaps());
    assert.throws(() => c.resolveLeaf({ ...c.leaves[0], ...replacement }), /specification/);
    assert.equal(c.snapshot().failed, true); assert.throws(() => c.resolveLeaf(c.leaves[0]), /poisoned/);
  }
  const c = createCanonicalLeafClosure([leaf()], leafCaps({ resolutions: 1 })); c.resolveLeaf(c.leaves[0]);
  assert.throws(() => c.resolveLeaf(c.leaves[0]), /resolutions/); assert.equal(c.snapshot().failed, true);
});

test("own descriptor values are captured rather than invoking proxy get or own accessor limits", () => {
  let reads = 0; const proxy = new Proxy(row(), { get() { reads++; throw new Error("get should not execute"); } });
  const c = createCanonicalLeafClosure([leaf([proxy])], leafCaps()); assert.equal(c.resolveLeaf(c.leaves[0])[0].real, "2/3"); assert.equal(reads, 0);
  const limits = leafCaps(); Object.defineProperty(limits, "records", { enumerable: true, get() { reads++; return 100; } });
  assert.throws(() => createCanonicalLeafClosure([leaf()], limits), /own data/); assert.equal(reads, 0);
});

test("swallowed nested resolver rejection poisons the active outer resolution permanently", () => {
  const c = createCanonicalLeafClosure([leaf()], leafCaps()); let nested = false;
  const specification = new Proxy(c.leaves[0], { getOwnPropertyDescriptor(target, key) {
    if (!nested) {
      nested = true;
      assert.throws(() => c.resolveLeaf({ ...c.leaves[0], id: "bad" }), /reentrant/);
    }
    return Reflect.getOwnPropertyDescriptor(target, key);
  } });
  assert.throws(() => c.resolveLeaf(specification), /reentrant/); assert.equal(c.snapshot().failed, true);
  assert.throws(() => c.resolveLeaf(c.leaves[0]), /poisoned/);
});

test("metadata snapshot keeps complete ranked arrays, ignores inherited toJSON, detaches and freezes", () => {
  let calls = 0; const input = Object.assign(Object.create({ toJSON() { calls++; return {}; } }), { matrix: [["0", "1/3"], ["-2", "0"]], identity: { point: 1, passed: false } });
  const result = snapshotCanonicalMetadata(input, metadataCaps());
  assert.deepEqual(result.value, { matrix: [["0", "1/3"], ["-2", "0"]], identity: { point: 1, passed: false } });
  input.matrix[0][1] = "8"; assert.equal(result.value.matrix[0][1], "1/3"); assert.ok(Object.isFrozen(result.value.matrix[0])); assert.equal(calls, 0);
  const special = JSON.parse('{"__proto__":{"x":1}}'); assert.equal(Object.getPrototypeOf(snapshotCanonicalMetadata(special, metadataCaps()).value), Object.prototype);
});

test("metadata snapshot rejects cycles, noncanonical values and accessors without execution", () => {
  let calls = 0; const getter = {}; Object.defineProperty(getter, "x", { enumerable: true, get() { calls++; return 0; } });
  const cycle = []; cycle.push(cycle);
  for (const value of [getter, cycle, { value: -0 }, { value: 1.1 }, { value: Infinity }, { value: "\n" }, { value: undefined }, { value: () => 1 }, { [Symbol("x")]: 1 }])
    assert.throws(() => snapshotCanonicalMetadata(value, metadataCaps()));
  assert.equal(calls, 0);
});

test("metadata snapshots have explicit depth, copied slots, nodes and string ceilings", () => {
  assert.throws(() => snapshotCanonicalMetadata([[[0]]], metadataCaps({ maxDepth: 2 })), /depth/);
  assert.throws(() => snapshotCanonicalMetadata([0, 1], metadataCaps({ arraySlots: 1 })), /length|arraySlots/);
  assert.throws(() => snapshotCanonicalMetadata([0], metadataCaps({ nodes: 1 })), /nodes/);
  assert.throws(() => snapshotCanonicalMetadata({ abc: "de" }, metadataCaps({ stringCharacters: 4 })), /stringCharacters/);
  assert.throws(() => snapshotCanonicalMetadata({}, metadataCaps({ maxDepth: 65 })), /depth/);
});

test("source point wrapper rejects copied binding/proof claims rather than accepting hashes/flags", () => {
  const a = new GeometryAlgebra(algebraLimits());
  for (const binding of [{ kind: "baseline", point: 0, comparison: { passed: true } }, Object.freeze({ kind: "baseline", point: 0, fullSourceRecipeBound: true })])
    assert.throws(() => createSourcePointAdapter(a, binding, { point: 0, tensorSha256: digest([]), scientificExecutionAuthorized: true }, {}), /binding identity/);
});

test("manufactured positive polynomial is not a source identity or numerical baseline replay receipt", () => {
  const empty = reconstructPolynomial([[], [], [], [], []], "1", [], algebraLimits(), {
    inputRecords: 10, outputRecords: 10, recordVisits: 100, readCharacters: 100, outputCharacters: 1000, rationalCharacters: 100
  });
  const a = new GeometryAlgebra(algebraLimits()), fakePoint = Object.freeze({ point: 0, polynomial: empty, numericalReplayPassed: true, baselineSha256: digest([]) });
  assert.throws(() => createSourceGermAdapter(a, fakePoint, { point: 0, metricBasis: 0, jetIndex: 0 }, {}), /authentic same-session/);
  assert.throws(() => createSourceGermAdapter(a, createCanonicalLeafClosure([leaf()], leafCaps()), {}, {}), /authentic same-session/);
});

// Actual standalone numerical replay of27 manufactured ONE-record units, not
// source background tensors. This exercises the real private receipt factory;
// no test backdoor, monkeypatch, skipped assertion or fake success token.
function manufacturedReceipt() {
  const { verifyMixedDag } = require("../a68-tensor-replay");
  const roles = baselineRoles().filter((r, i, all) => all.findIndex(s => s.canonicalId === r.canonicalId) === i);
  const recipe = new MixedRecipe(), tensors = [], requests = [];
  for (const role of roles) {
    const form = role.degree === 1 ? 1 : 3, t = recipe.Unit(form, 1);
    requests.push({ id: role.canonicalId, degree: role.degree, node: recipe.RecordedNode(t) }); tensors.push([row(form, 1, "1", "0")]);
  }
  const plan = recipe.Finish(), roots = [{ name: "one", expression: { op: "constant", value: "1" } }];
  const checks = [{ name: "selfEqual", metadataPath: "check/selfEqual", predicate: { kind: "tensorEqual", left: { kind: "tensorNode", node: 0 }, right: { kind: "tensorNode", node: 0 } } }];
  const domains = [{ name: "manufacturedDomain", node: 0, degree: 1, local: true, hAntiHermitian: true, canonicalNonzeroRecords: true }];
  const graph = { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: plan.leaves,
    nodes: plan.nodes.map((n, i) => ({ ...n, records: 1, bytes: JSON.stringify(tensors[i]).length, sha256: digest(tensors[i]) })), marks: [] };
  const scalar = { tensorPlan: plan, namedRoots: roots, geometry: [],
    scheduleLimits: { tensorNodes: 100, tensorEdges: 1000, tensorMarks: 100, scalarNodes: 1000, scalarEdges: 1000, roots: 100,
      geometryMatrices: 100, geometryEntries: 1000, maxDepth: 64, stringCharacters: 100000, rationalCharacters: 100 },
    arithmeticLimits: algebraLimits(),
    replayLimits: { tensorRecords: 100, tensorResolutions: 1000, recordVisits: 10000, coefficientReads: 10000,
      readCharacters: 100000, outputCharacters: 100000, rationalCharacters: 100, liveScalars: 1000 },
    wireLimits: { liveWireRecords: 100, sortReferences: 100, liveWireCharacters: 100000, serializationRecordVisits: 10000, serializationCharacters: 1000000 },
    compareRoot: (name, value) => { assert.equal(name, "one"); assert.equal(value, "1"); return true; } };
  const consumers = { checks, domainChecks: domains, error: null,
    scheduleLimits: { consumers: 100, expressionNodes: 1000, expressionEdges: 1000, tensorReferences: 1000, scalarReferences: 100,
      stringCharacters: 100000, maxDepth: 64, rationalCharacters: 100, retainedRootValues: 100 },
    arithmeticLimits: algebraLimits(),
    replayLimits: { tensorRecords: 100, tensorResolutions: 1000, parsedTensorRecords: 10000, recordVisits: 100000, geometryResolutions: 100,
      geometryCoordinates: 100000, arraySlots: 100000, coefficientReads: 100000, readCharacters: 100000, outputCharacters: 100000,
      rationalCharacters: 100, predicateNodes: 10000, rootValues: 100, errorValues: 100, resultValues: 100, consumerComparisons: 100, sourceResolutions: 100 },
    resolveGeometryField: () => { throw Error("no source geometry in manufactured replay"); },
    resolveSourceScalar: () => { throw Error("no source scalar in manufactured replay"); }, compareConsumer: (_, value) => { assert.equal(value, true); return true; } };
  const limits = { nodes: 100, marks: 100, tensorRecords: 3, rationalCharacters: 32, tensorBytes: 10000, graphBytes: 100000,
    pairVisits: 10000, slotVisits: 10000, liveRecords: 100 };
  const result = verifyMixedDag(graph, plan.leaves, plan.marks, limits, () => { throw Error("no leaves"); }, () => true, () => true,
    scalar, consumers, { contextId: "manufactured-point", requests, limits: { tensors: 27, records: 81, characters: 20000, recordVisits: 81 } });
  return { receipt: result.exportReceipt, expected: { contextId: "manufactured-point", tensorPlan: plan, namedRoots: roots, geometry: [],
    checks, domainChecks: domains, error: null, requests }, tensors };
}
const receiptCaps = () => metadataCaps({ nodes: 10000, arraySlots: 10000, stringCharacters: 100000 });
const baselineCaps = () => ({ leaves: 27, stringCharacters: 10000, resolutions: 100 });

test("real manufactured numerical receipt binds every plan/root/check/domain and retains27 full tensors", () => {
  const { receipt, expected, tensors } = manufacturedReceipt(), bound = bindVerifiedReplayExports(receipt, expected, receiptCaps());
  assert.equal(bound.exports.length, 27); assert.equal(bound.scope.sourceAuthenticityEstablished, false);
  bound.exports.forEach((e, i) => { assert.deepEqual(e.tensor, tensors[i]); assert.equal(e.sha256, digest(tensors[i])); assert.ok(Object.isFrozen(e.tensor)); });
  assert.notEqual(bound.exports[0].node, bound.exports[1].node); assert.equal(bound.exports[0].sha256, bound.exports[1].sha256, "equal hashes never merge node identity");
});

test("copied receipts and any changed identity component cannot bind", () => {
  const { receipt, expected } = manufacturedReceipt();
  assert.throws(() => bindVerifiedReplayExports({ ...receipt }, expected, receiptCaps()), /private complete/);
  const variants = {
    contextId: "other-point", tensorPlan: { ...expected.tensorPlan, nodes: [] }, namedRoots: [], geometry: [{ id: "untrusted", entries: [] }],
    checks: [], domainChecks: [], error: {}, requests: expected.requests.slice(1)
  };
  for (const [key, value] of Object.entries(variants)) assert.throws(() => bindVerifiedReplayExports(receipt, { ...expected, [key]: value }, receiptCaps()), new RegExp("identity " + key));
  assert.throws(() => bindVerifiedReplayExports(receipt, expected, metadataCaps({ nodes: 1 })), /nodes/);
});

test("two germ import bindings share27 immutable tensors but use distinct per-context IDs", () => {
  const { receipt, expected } = manufacturedReceipt(), bound = bindVerifiedReplayExports(receipt, expected, receiptCaps());
  const a = createVerifiedBaselineLeafBindings(bound, "manufactured/germA", baselineCaps());
  const b = createVerifiedBaselineLeafBindings(bound, "manufactured/germB", baselineCaps());
  assert.equal(a.leaves.length, 27); assert.equal(Object.keys(a.leafIds).length, 27);
  assert.ok(!Object.hasOwn(a.leafIds, "GradientPieces[0]")); assert.ok(Object.hasOwn(a.leafIds, "KInputs[0]"));
  for (let i = 0; i < 27; i++) {
    assert.notEqual(a.leaves[i].id, b.leaves[i].id);
    assert.equal(a.resolveLeaf(a.leaves[i]), b.resolveLeaf(b.leaves[i])); assert.equal(a.resolveLeaf(a.leaves[i]), bound.exports[i].tensor);
  }
  assert.equal(a.scope.onlyAlias, "GradientPieces[0]=KInputs[0]"); assert.equal(a.scope.sourceAuthenticityEstablished, false);
});

test("verified baseline declarations import through real MixedRecipe with only the declared alias", () => {
  const { receipt, expected } = manufacturedReceipt(), bound = bindVerifiedReplayExports(receipt, expected, receiptCaps());
  const imports = createVerifiedBaselineLeafBindings(bound, "manufactured/germ", baselineCaps());
  const identity = Array.from({ length: 14 }, (_, i) => ({ row: i, column: i, value: "1" })), zeros = Array(14).fill("zero");
  const matrices = [{ name: "identity", matrix: identity }, { name: "zero", matrix: [] }];
  const geometry = { schemaVersion: BACKGROUND_SCHEMA, point: 0, provenance: [{ id: "manufactured", source: "manufactured", sha256: "0".repeat(64) }],
    matrices: { Frame: "identity", InverseFrame: "identity", Identity: "identity", FrameLift: zeros, FramePartial: zeros, Omega: zeros, InversePartial: zeros, ConnectionInFrame: zeros } };
  const r = new MixedRecipe({ leaves: imports.leaves, matrices });
  const background = importMixedBaseline(r, { geometry, validateGeometry: () => true, leafIds: imports.leafIds });
  assert.equal(background.KInputs[0], background.GradientPieces[0]); assert.equal(canonicalBaselineImports(r, background).length, 27);
  const plan = r.Finish(); assert.equal(plan.nodes.length, 27); assert.equal(new Set(plan.nodes.map(n => n.parameters.id)).size, 27);
  assert.deepEqual(plan.leaves, imports.leaves);
});

test("baseline importer rejects copied generic brands, incomplete menus, limits and mismatched leaf specs", () => {
  const { receipt, expected } = manufacturedReceipt(), bound = bindVerifiedReplayExports(receipt, expected, receiptCaps());
  assert.throws(() => createVerifiedBaselineLeafBindings({ ...bound }, "germ", baselineCaps()), /private verified/);
  assert.throws(() => createVerifiedBaselineLeafBindings(bound, "germ", { ...baselineCaps(), leaves: 26 }), /27/);
  assert.throws(() => createVerifiedBaselineLeafBindings(bound, "germ", { ...baselineCaps(), stringCharacters: 1 }), /character/);
  const imports = createVerifiedBaselineLeafBindings(bound, "germ", baselineCaps());
  assert.throws(() => imports.resolveLeaf({ ...imports.leaves[0], sha256: "0".repeat(64) }), /exact verified/);
  assert.throws(() => imports.resolveLeaf(imports.leaves[0]), /poisoned/);
  const a = new GeometryAlgebra(algebraLimits()); assert.throws(() => createSourceGermAdapter(a, bound, {}, {}), /authentic same-session/);
});

// VM-manufactured adapter CONTROL FLOW only. Replace source brands, geometry,
// recipes, checkpoint/replay readers and live helper explicitly; no genuine
// geometry, source certificate, pinned checkpoint or numerical result is made.
function manufacturedAdapterFlow() {
  const fs = require("node:fs"), path = require("node:path"), vm = require("node:vm"), { createRequire } = require("node:module");
  const file = path.resolve(__dirname, "../a68-source-orchestration.js"), native = createRequire(file), module = { exports: {} };
  const events = [], helpers = [], roles = baselineRoles().filter((r, i, all) => all.findIndex(x => x.canonicalId === r.canonicalId) === i);
  const baseline = { identity: { kind: "baseline", point: 0 }, spinCurvature: [] }, binding = { source: { path: "manufactured/baseline" }, rebuilt: baseline };
  const germ = { identity: { kind: "germ", point: 0, metricBasis: 0, jetIndex: 0, baseline }, spinCurvatureVariation: [] };
  const germBinding = { source: { path: "manufactured/germ" }, rebuilt: germ };
  const a = {}, controls = { failLate: false, latePoint: 0, lateGerm: 0, replayPoint: 0, replayGerm: 0, children: 0 };
  const context = id => ({ contextId: id, scalarRoots: [], geometry: [], checks: [], domainChecks: [], error: null });
  const mocks = {
    "./a68-geometry-binding": { boundGeometryIdentity: (_a, b) => ({ rebuilt: b.rebuilt }) },
    "./a68-source-geometry": { sourceGeometryIdentity: (_a, rebuilt) => rebuilt.identity },
    "./a68-polynomial-reconstruction": { reconstructedPolynomialIdentity: () => ({ point: 0, polynomial: { X: { source: { path: "manufactured/X" } } } }) },
    "./a68-geometry-recipe-binding": { exportRecipeGeometry: () => ({ matrices: [], descriptor: {}, validateGeometry() {}, usage: {} }) },
    "./a68-source-field-resolver": { createSourceGermFieldResolver: () => ({ snapshot: () => ({ failed: false }) }) },
    "./a68-geometry-metadata": { compareRecordedGeometry(_a, b) {
      const kind = b === binding ? "point" : "germ"; events.push("late-compare/" + kind);
      controls[kind === "point" ? "latePoint" : "lateGerm"]++;
      if (controls.failLate) throw Error("manufactured late source mismatch"); return { manufactured: true };
    } },
    "./a68-mixed-recipe": { MixedRecipe: class {
      constructor(options) { this.leaves = options.leaves; } RecordedNode(value) { return value.node; }
      Finish() { return { nodes: [], marks: [], leaves: this.leaves }; }
    } },
    "./a68-audit-context-recipe": {
      buildAuditPointContext: () => ({ ...context("point0"), canonicalBaselineExports: roles.map((r, i) => ({ id: r.canonicalId, degree: r.degree, value: { node: i } })) }),
      buildAuditGermContext: () => context("point0/m0_j0")
    },
    "./a68-context-metadata": { contextMetadataPlan: () => ({ manufactured: true }) },
    "./a68-point-checkpoint": {
      pointCheckpointIdentity: checkpoint => ({ declaration: { contextId: "point0" }, root: "/manufactured", pins: { graph: {}, metadata: {} }, checkpoint: { metadata: { "background/geometry": checkpoint.geometry } } }),
      replayPointCheckpoint(_checkpoint, expected) {
        controls.replayPoint++; events.push("numerical/point");
        return { expandedArtifacts: { expandedTensorArtifactsVerified: false }, exportReceipt: { ...expected,
          completion: { tensor: true, scalar: true, consumers: true, released: true },
          exports: expected.requests.map(r => ({ ...r, tensor: Object.freeze([]), sha256: digest([]) })) } };
      }
    },
    "./a68-tensor-replay": { readVerifiedReplayReceipt: receipt => receipt },
    "./a68-point-completion": { createPointCompletionLedger: () => ({ snapshot: () => ({ failed: false, completed: false }), registerChild() { controls.children++; } }) },
    "./a68-context-checkpoint": { contextCheckpointIdentity: () => ({ root: "/manufactured", pins: { graph: {}, metadata: {} } }) },
    "./a68-context-replay": { replayContextCheckpoint(checkpoint, _plan, _limits, resolvers) {
      resolvers.compareGeometryMetadata({ "geometry/geometry": checkpoint.geometry });
      events.push("numerical/germ"); controls.replayGerm++; return { manufactured: true };
    } },
    "./a68-live-geometry": { createLiveGeometryValidation(_a, slots, cap) {
      assert.equal(_a, a); assert.deepEqual(cap, { explicitlyManufactured: true });
      let failed = false, completed = false, checked = false, bound = false, hash = null;
      const name = slots[0].metadataPath, kind = name === "background/geometry" ? "point" : "germ";
      assert.equal(slots.length, 1); assert.equal(slots[0].binding, kind === "point" ? binding : germBinding);
      const helper = { precheck(p, observed) { assert.equal(p, name); assert.equal(checked, false); hash = digest(observed); checked = true; events.push("precheck/" + kind); return { metadataPath: p }; },
        requireComplete() { assert.equal(checked, true, "all live prechecks before pinned comparison"); },
        bindRecorded(p, observed) { assert.equal(p, name); assert.equal(checked, true); assert.equal(bound, false); assert.equal(digest(observed), hash); bound = true; events.push("bind/" + kind); },
        finish() { assert.equal(bound, true); assert.equal(failed, false); completed = true; hash = null; events.push("finish/" + kind); },
        abort() { failed = true; hash = null; },
        snapshot: () => ({ failed, completed, retainedCommitments: hash === null ? 0 : 1 }) };
      helpers.push(helper); return helper;
    } }
  };
  vm.runInNewContext(fs.readFileSync(file, "utf8"), { module, require: name => mocks[name] ?? native(name) }, { filename: file });
  const caps = () => ({ leaf: leafCaps({ leaves: 40, stringCharacters: 100000 }),
    export: { matrixSlots: 100, coordinateVisits: 100, entryRecords: 100 }, metadata: receiptCaps(),
    comparison: { coordinateVisits: 100, sparseRecords: 100, tensorRecords: 100, rationalCharacters: 1000 } });
  const proof = { point: 0, gamma: 1, kappa: 907712, lambda: "1/907712", epsilon: native("./a68-retained-inputs").EPSILON, tensor: [], tensorSha256: digest([]) };
  const point = module.exports.createSourcePointAdapter(a, binding, proof, caps());
  const buildPoint = () => point.buildPointRecipe({ retention: {}, nodeLimit: 100, markLimit: 100 });
  const child = () => {
    const adapter = module.exports.createSourceGermAdapter(a, point, germBinding, { ...caps(), fields: { fields: 1, coordinateVisits: 1, arrayObjects: 1, arraySlots: 1, formatCharacters: 1, rationalCharacters: 1 } });
    adapter.buildGermRecipe({ retention: {}, wardRetention: null, nodeLimit: 100, markLimit: 100 }); return adapter;
  };
  return { point, buildPoint, child, controls, helpers, events, identity: module.exports.sourceAdapterIdentity };
}
test("VM-manufactured point and germ live wiring preserves private prepared state and full late comparison", () => {
  const f = manufacturedAdapterFlow(); f.buildPoint(); f.point.configureLiveGeometry({ explicitlyManufactured: true });
  const p = { pointValue: "1" }; f.point.precheckGeometry("background/geometry", p);
  assert.equal(f.identity(f.point).prepared, true); assert.equal(f.point.snapshot().recordedGeometryCompared, false);
  f.point.replayPointCheckpoint({ geometry: p }, {}); const child = f.child(); child.configureLiveGeometry({ explicitlyManufactured: true });
  const g = { germValue: "2" }; child.precheckGeometry("geometry/geometry", g);
  assert.equal(f.identity(child).prepared, true); child.replayContextCheckpoint({ geometry: g }, {});
  assert.deepEqual(f.events, ["precheck/point", "bind/point", "late-compare/point", "numerical/point", "finish/point",
    "precheck/germ", "bind/germ", "late-compare/germ", "numerical/germ", "finish/germ"]);
  assert.equal(f.controls.latePoint, 1); assert.equal(f.controls.lateGerm, 1); assert.equal(f.controls.children, 1);
  for (const adapter of [f.point, child]) { assert.equal(adapter.snapshot().liveGeometry.completed, true); assert.equal(f.identity(adapter).health().failed, false); }
});
test("VM-manufactured skipped live checks and late source mismatch abort helper without numerical acceptance", () => {
  for (const skip of [false, true]) {
    const f = manufacturedAdapterFlow(); f.buildPoint(); f.point.configureLiveGeometry({ explicitlyManufactured: true });
    if (!skip) f.point.precheckGeometry("background/geometry", {});
    f.controls.failLate = true; assert.throws(() => f.point.replayPointCheckpoint({ geometry: {} }, {}));
    assert.equal(f.controls.replayPoint, 0); assert.equal(f.controls.latePoint, skip ? 0 : 1);
    assert.equal(f.point.snapshot().failed, true); assert.equal(f.point.snapshot().liveGeometry.failed, true);
    assert.equal(f.point.snapshot().liveGeometry.retainedCommitments, 0); assert.equal(f.identity(f.point).health().failed, true);
  }
});
test("VM-manufactured helper failure revokes retained point and germ private health capabilities", () => {
  const f = manufacturedAdapterFlow(); f.buildPoint(); f.point.configureLiveGeometry({ explicitlyManufactured: true });
  f.point.precheckGeometry("background/geometry", {}); f.point.replayPointCheckpoint({ geometry: {} }, {});
  const child = f.child(); child.configureLiveGeometry({ explicitlyManufactured: true }); child.precheckGeometry("geometry/geometry", {});
  const pointHealth = f.identity(f.point).health, childHealth = f.identity(child).health;
  f.helpers[1].abort(); assert.equal(childHealth().failed, true); assert.equal(child.snapshot().failed, true);
  f.helpers[0].abort(); assert.equal(pointHealth().failed, true); assert.equal(f.point.snapshot().failed, true);
});

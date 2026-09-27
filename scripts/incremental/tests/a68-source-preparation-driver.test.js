"use strict";
// Exact preparation-driver AND dispatcher code, real pinned file readers.
// Source constructors/brands are VM-only manufactured doubles. This exercises
// orchestration, NOT genuine geometry, polynomial or numerical source replay.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto"), vm = require("node:vm");
const { createRequire } = require("node:module");
const { sourceContextIds, sourceContextMenu, diagnosticMenu } = require("../a68-source-context-menu");
const { pointCheckNames, pointCheckpointIdentity } = require("../a68-point-checkpoint");
const { contextCheckpointIdentity } = require("../a68-context-checkpoint");
const { pointCompletionIdentity } = require("../a68-point-completion");
const { checkpointEvent } = require("../a68-source-checkpoint-dispatcher");
const { clone, limits: structureLimits, profile: makeProducerProfile, template: producerTemplate } = require("./a68-source-template-fixture");
const { snapshotCanonicalMetadata } = require("../a68-canonical-metadata");
const driverFile = path.resolve(__dirname, "../a68-source-preparation-driver.js"), actualRequire = createRequire(driverFile);
const wire = value => JSON.stringify(value) + "\n", hash = value => crypto.createHash("sha256").update(value).digest("hex");
// TEST-ONLY copied metadata quotas, not prospective scientific resource caps.
const quota = () => ({ ...structureLimits });
const driverLimits = () => ({ snapshot: quota(), dispatcher: { configuration: quota(), event: quota(), total: quota() } });
const algebra = () => ({ maxBits: 128, scalarOperations: 100000, rationalObjects: 100000, matrixObjects: 1000, matrixCells: 10000, slotVisits: 100000 });
const comparison = () => ({ coordinateVisits: 1, sparseRecords: 1, tensorRecords: 1, rationalCharacters: 1 });
function adapterLimits(germ = false) {
  return { leaf: { leaves: 100, records: 1, stringCharacters: 100000, wireBytes: 100000, rationalCharacters: 128, resolutions: 1000 },
    export: { matrixSlots: 1, coordinateVisits: 1, entryRecords: 1 }, metadata: quota(), comparison: comparison(),
    ...(germ ? { fields: { fields: 1, coordinateVisits: 1, arrayObjects: 1, arraySlots: 1, formatCharacters: 1, rationalCharacters: 1 } } : {}) };
}
function policy(root) {
  const recipe = { nodeLimit: 10000000, markLimit: 5000 };
  return { repositoryRoot: root, profiles: [
    { kind: "point", algebra: algebra(), binding: comparison(), polynomialArithmetic: algebra(),
      polynomial: { inputRecords: 1, outputRecords: 1, recordVisits: 1, readCharacters: 1, outputCharacters: 1, rationalCharacters: 1 }, adapter: adapterLimits(), recipe },
    { kind: "germ", binding: comparison(), adapter: adapterLimits(true), recipe },
    { kind: "diagnostic", algebra: algebra(), binding: comparison(), adapter: adapterLimits(), recipe },
    { kind: "diagnostic", algebra: null, binding: null, adapter: adapterLimits(), recipe }],
    routes: sourceContextIds().map(contextId => ({ contextId, profile: /^point[01]$/.test(contextId) ? 0 : contextId.startsWith("point") ? 1 : contextId.endsWith("secondJets") ? 3 : 2 })) };
}
function declaration(index) {
  const { kind, contextId: id } = checkpointEvent(index);
  if (kind === "final") return { contextId: id, graphPath: id + "/graph.json", backgroundPath: id + "/background-checkpoint.json", metadataPath: id + "/metadata.json" };
  return { contextId: id, graphPath: id + "/graph.json", metadataPath: id + (kind === "background" ? "/background-checkpoint.json" : "/metadata.json"),
    marks: sourceContextMenu(id).marks.map((m, i) => ({ name: m.name, degree: m.degree, expanded: m.requiredExpanded ?? false,
      relativePath: m.requiredExpanded ? id + "/tensors/t" + String(i).padStart(6, "0") + ".json" : null })) };
}
// Reused compact digests, never a retained all-context mark/menu set.
const events = Array.from({ length: 707 }, (_, i) => ({ ...checkpointEvent(i), declarationSha256: hash(wire(declaration(i))), profile: checkpointEvent(i).kind === "final" ? 1 : 0 }));
const originalProducerProfile = makeProducerProfile();
const fullTemplate = index => producerTemplate(checkpointEvent(index).contextId, originalProducerProfile);
const producerCommitments = Object.freeze({ profileSha256: hash(wire(originalProducerProfile)), captureSha256: hash(wire(originalProducerProfile.capture)),
  templates: Object.freeze(sourceContextIds().map(contextId => Object.freeze({ contextId, templateSha256: hash(wire(producerTemplate(contextId, originalProducerProfile))) }))) });
function fixture(t) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-preparation-"));t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const brands = new WeakMap(), calls = [], admissions = [], states = [], algebras = [], backgrounds = new Map();
  const failure = { operation: null }, configurationHealth = { failed: false };let observer = null, leafMutation = null;
  const stageFor = { inputs: "retained-inputs", algebra: "geometry-algebra", "read-baseline": "retained-baseline", baseline: "source-baseline",
    "read-germ": "retained-germ", germ: "source-germ", bind: "geometry-binding", "read-polynomial": "retained-polynomial", "read-certificate": "retained-certificate",
    polynomial: "polynomial-reconstruction", "point-adapter": "source-adapter", "germ-adapter": "source-adapter", "diagnostic-adapter": "source-adapter", recipe: "source-recipe",
    replay: "checkpoint-read-replay", final: "point-completion" };
  const record = (operation, value = null) => {
    if (stageFor[operation]) assert.equal(admissions.at(-1)?.operation, stageFor[operation], "explicit admission precedes " + operation);
    calls.push({ operation, value });if (failure.operation === operation) throw Error("manufactured " + operation + " failure");
  };
  class Algebra {
    constructor(limits) { record("algebra", limits);assert.ok(Object.isFrozen(limits));algebras.push(this);this.failed = false; }
    snapshot() { return { failed: this.failed }; }
  }
  class Inputs {
    constructor(repositoryRoot) { record("inputs", repositoryRoot);assert.equal(repositoryRoot, root); }
    baseline(point) { record("read-baseline", point);return { point, retained: true }; }
    geometry(point, metricBasis, jetIndex) { record("read-germ", { point, metricBasis, jetIndex });return { point, metricBasis, jetIndex, retained: true }; }
    polynomialInputs(point) { record("read-polynomial", point);return { point }; }
    certificate(point) { record("read-certificate", point);return { point }; }
  }
  function baseline(a, point) { record("baseline", { a, point });assert.ok(a instanceof Algebra);return { a, point }; }
  function germ(a, b, metricBasis, jetIndex) { record("germ", { a, baseline: b, metricBasis, jetIndex });assert.equal(a, b.a);return { a, point: b.point, baseline: b, metricBasis, jetIndex }; }
  function bind(a, rebuilt, retained, limits) {
    record("bind", { a, rebuilt, retained, limits });assert.equal(rebuilt.a, a);assert.equal(rebuilt.point, retained.point);
    assert.equal(rebuilt.metricBasis, retained.metricBasis);assert.equal(rebuilt.jetIndex, retained.jetIndex);return { a, rebuilt };
  }
  function adapter(kind, id, a, parent = null) {
    const state = { kind, contextId: id, identity: Object.freeze({}), parentIdentity: parent ? brands.get(parent).identity : null,
      prepared: false, replayed: false, completed: false, failed: false, children: 0, a, skipReplay: false };
    state.health = () => ({ failed: state.failed });states.push(state);
    const leaves = sourceContextMenu(id).leaves.map(l => ({ ...l, sha256: "a".repeat(64) }));
    const build = options => {
      record("recipe", { id, options });assert.ok(Object.isFrozen(options.retention));
      const ordinary = id.startsWith("point") && id.includes("/") ? new Set(actualRequire("./a68-audit-context-recipe").germMarkMenu().map(m => m.name)) : null;
      const all = sourceContextMenu(id).marks;
      const expected = Object.fromEntries(all.filter(m => !ordinary || ordinary.has(m.name)).map(m => [m.name, m.requiredExpanded ?? false]));
      assert.equal(JSON.stringify(options.retention), JSON.stringify(expected));
      if (ordinary) {
        const ward = all.filter(m => !ordinary.has(m.name));
        assert.equal(JSON.stringify(options.wardRetention), JSON.stringify(ward.length ? Object.fromEntries(ward.map(m => [m.name, m.requiredExpanded ?? false])) : null));
      }
      if (leafMutation) leafMutation(leaves, id);
      assert.equal(state.prepared, false);state.prepared = true;return { tensorPlan: { leaves, nodes: [], marks: [] } };
    };
    const replay = token => {
      record("replay", id);const pinned = kind === "point" ? pointCheckpointIdentity(token) : contextCheckpointIdentity(token);
      assert.equal(pinned.declaration.contextId, id);assert.equal(pinned.root, root);assert.equal(state.prepared, true);
      if (!state.skipReplay) { state.replayed = true;state.prepared = false; }
      if (parent) brands.get(parent).children++;
    };
    const result = Object.freeze({ leaves, geometryLeaves: leaves, buildPointRecipe: build, buildGermRecipe: build, buildRecipe: build,
      replayPointCheckpoint: replay, replayContextCheckpoint: replay,
      acceptPointCompletion: token => { record("final", id);assert.equal(pointCompletionIdentity(token).declaration.contextId, id);assert.equal(state.children, 350);state.completed = true; },
      snapshot: () => ({ failed: state.failed }) });
    brands.set(result, state);return result;
  }
  const readBrand = value => {
    const s = brands.get(value);assert.ok(s, "manufactured private source brand");
    return Object.freeze({ kind: s.kind, contextId: s.contextId, parentIdentity: s.parentIdentity, identity: s.identity,
      prepared: s.prepared, replayed: s.replayed, completed: s.completed, health: s.health });
  };
  const hostResult = Object.freeze({});
  const read = { graphBytes: 100000, metadataBytes: 100000, totalReadBytes: 200002, snapshot: quota() };
  const configuration = { outputRoot: root, events, profiles: [{ read, replay: {} },
    { read: { backgroundBytes: 100000, metadataBytes: 100000, totalReadBytes: 200002, snapshot: quota() }, replay: null }] };
  const mocks = {
    "./a68-template-process-host": { validatedCheckpointConfiguration(value) { assert.equal(value, hostResult);return { configuration, producerCommitments, health: () => configurationHealth }; } },
    "./a68-retained-inputs": { RetainedMixedInputs: Inputs, PINS: Object.freeze({ geometry: { bytes: 1 }, stationary: { bytes: 1 } }) },
    "./a68-geometry-algebra": { GeometryAlgebra: Algebra },
    "./a68-source-geometry": { buildSourceBaselineGeometry: baseline, buildSourceGermGeometry: germ },
    "./a68-geometry-binding": { bindRetainedGeometry: bind },
    "./a68-polynomial-reconstruction": { reconstructRetainedPolynomial(p, c, a, limits) { record("polynomial", { p, c, a, limits });assert.equal(p.point, c.point);return { point: p.point }; } },
    "./a68-source-orchestration": { sourceAdapterIdentity: readBrand,
      createSourcePointAdapter(a, b, p, limits) { record("point-adapter", { a, b, p, limits });assert.equal(b.rebuilt.point, p.point);return adapter("point", "point" + p.point, a); },
      createSourceGermAdapter(a, parent, b, limits) { record("germ-adapter", { a, parent, b, limits });assert.equal(brands.get(parent).a, a);assert.equal(brands.get(parent).replayed, true);
        const g = b.rebuilt;return adapter("germ", `point${g.point}/m${g.metricBasis}_j${g.jetIndex}`, a, parent); } },
    "./a68-diagnostic-source-adapter": { diagnosticSourceLayout: actualRequire("./a68-diagnostic-source-adapter").diagnosticSourceLayout,
      diagnosticAdapterIdentity: readBrand,
      createDiagnosticSourceAdapter(a, name, b, limits) { record("diagnostic-adapter", { a, name, b, limits });
        if (name === "secondJets") { assert.equal(a, null);assert.equal(b.baseline, null);assert.equal(b.germs.length, 0); }
        else { assert.equal(b.baseline.a, a);assert.equal(b.germs.length, name === "grade10" ? 4 : 1);b.germs.forEach(g => assert.equal(g.a, a)); }
        return adapter("diagnostic", "diagnostic/" + name, a); } }
  };
  function load(file) { const module = { exports: {} };vm.runInNewContext(fs.readFileSync(file, "utf8"), {
    module, require: name => Object.hasOwn(mocks, name) ? mocks[name] : actualRequire(name) }, { filename: file });return module.exports; }
  mocks["./a68-source-checkpoint-dispatcher"] = load(path.resolve(__dirname, "../a68-source-checkpoint-dispatcher.js"));
  const implementation = load(driverFile), suppliedPolicy = policy(root), suppliedLimits = driverLimits();
  const producerProfile = clone(originalProducerProfile), producerCapture = clone(originalProducerProfile.capture);
  const admit = descriptor => { admissions.push(descriptor);assert.ok(Object.isFrozen(descriptor));if (observer) return observer(descriptor); };
  const artifact = (file, value) => { const bytes = wire(value);fs.mkdirSync(path.dirname(path.join(root, file)), { recursive: true });fs.writeFileSync(path.join(root, file), bytes);return { path: file, bytes: Buffer.byteLength(bytes), sha256: hash(bytes) }; };
  function observed(i) {
    const e = checkpointEvent(i), d = declaration(i), id = e.contextId;
    if (e.kind === "final") { const b = backgrounds.get(id);return { context: id, background: b.observed,
      metadata: artifact(d.metadataPath, { schema: "phase627-point-computational-completion-v1", context: id, status: "producer-complete", independentValidationComplete: false,
        backgroundCheckpoint: b.observed.metadata, metadata: b.metadata }) }; }
    const graph = artifact(d.graphPath, { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: [], nodes: [], marks: [] }), point = id === "point0" ? 0 : 1;
    const metadata = e.kind === "background" ? { "background/geometry": { point, frame: [], inverseFrame: [], metric: {}, connection: {}, frameLift: [], framePartial: [], omega: [] },
      ...Object.fromEntries(pointCheckNames(point).map(n => ["check/" + n, true])) } : { "check/manufactured": true };
    const envelope = e.kind === "background" ? { schema: "phase627-point-background-checkpoint-v1", context: id, status: "background-sealed", pointTraversalComplete: false, graph, metadata } :
      { schema: "phase627-context-computational-evidence-v1", context: id, status: "producer-complete", independentValidationComplete: false, graph, metadata };
    const result = { context: id, graph, metadata: artifact(d.metadataPath, envelope) };
    if (e.kind === "background") backgrounds.set(id, { observed: result, metadata });return result;
  }
  return { root, calls, admissions, states, algebras, failure, configurationHealth, suppliedPolicy, suppliedLimits, observed, producerProfile, producerCapture,
    setObserver: f => { observer = f; }, setLeafMutation: f => { leafMutation = f; },
    create: (begin = true) => { const driver = implementation.createSourcePreparationDriver(hostResult, suppliedPolicy, suppliedLimits, admit);
      if (begin) driver.beginProducer(producerProfile, producerCapture);return driver; },
    step(driver, i) { if (checkpointEvent(i).kind === "final") return driver.completePoint(declaration(i), observed(i));
      if (checkpointEvent(i).kind === "diagnostic") {
        const name = checkpointEvent(i).contextId.slice("diagnostic/".length);
        driver.validateDiagnosticMenu(name, diagnosticMenu(name));
      }
      const result = driver.prepareNext(declaration(i), fullTemplate(i));assert.equal(result.contextId, checkpointEvent(i).contextId);assert.ok(Object.isFrozen(result.leaves));
      driver.acceptCheckpoint(observed(i));return result; } };
}
test("full manufactured preparation feeds real dispatcher/readers with four cumulative algebra sessions", t => {
  const f = fixture(t), driver = f.create();assert.equal(f.calls.length, 0);
  for (let i = 0; i < 707; i++) {
    f.step(driver, i);
    if (i === 351 || i === 703) assert.equal(driver.snapshot().retainedActivePoints, 0);
  }
  assert.equal(driver.snapshot().completed, true);assert.equal(f.algebras.length, 4);
  assert.equal(driver.snapshot().producerProfileMatched, true);assert.equal(driver.snapshot().producerTemplatesMatched, 705);
  assert.equal(driver.snapshot().menusMatched, 3);
  assert.equal(f.admissions.filter(s => s.operation === "diagnostic-menu").length, 3);
  assert.equal(driver.snapshot().retainedInputReaders, 0);assert.equal(driver.snapshot().pendingContextId, null);
  const count = op => f.calls.filter(c => c.operation === op).length;
  assert.equal(count("inputs"), 1);assert.equal(count("baseline"), 4);assert.equal(count("germ"), 705);
  assert.equal(count("polynomial"), 2);assert.equal(count("point-adapter"), 2);assert.equal(count("germ-adapter"), 700);
  assert.equal(count("diagnostic-adapter"), 3);assert.equal(count("recipe"), 705);assert.equal(count("replay"), 705);assert.equal(count("final"), 2);
  assert.equal(f.admissions.filter(s => s.operation === "retention-policy").length, 705);
  assert.equal(f.admissions.filter(s => s.operation === "leaf-declaration-export").length, 705);
  assert.deepEqual(f.algebras.map(a => f.calls.filter(c => c.operation === "germ" && c.value.a === a).length), [350, 350, 4, 1]);
  assert.equal(driver.snapshot().scope.scientificExecutionAuthorized, false);
  f.states[7].failed = true;assert.equal(driver.snapshot().completed, false);
});
function atDiagnostic(t) {
  const f = fixture(t), driver = f.create();
  for (let i = 0; i < 704; i++) f.step(driver, i);
  return { f, driver };
}
test("diagnostic menus reject missing early wrong repeated and pending-context submissions before source work", t => {
  for (const mode of ["missing", "early", "unbegun", "wrong", "repeated", "pending"]) {
    const pair = ["early", "unbegun"].includes(mode) ? (() => { const f = fixture(t);return { f, driver: f.create(mode !== "unbegun") }; })() : atDiagnostic(t);
    const { f, driver } = pair, menu = diagnosticMenu("grade10");
    if (["repeated", "pending"].includes(mode)) driver.validateDiagnosticMenu("grade10", menu);
    if (mode === "pending") driver.prepareNext(declaration(704), fullTemplate(704));
    const calls = f.calls.length, admissions = f.admissions.length;
    assert.throws(() => mode === "missing" ? driver.prepareNext(declaration(704), fullTemplate(704)) :
      driver.validateDiagnosticMenu(mode === "wrong" ? "acceleration" : "grade10", menu), /diagnostic menu/);
    assert.equal(f.calls.length, calls);assert.equal(f.admissions.length, admissions);
    assert.equal(driver.snapshot().failed, true);assert.equal(driver.snapshot().retainedActivePoints, 0);
    assert.equal(driver.snapshot().retainedInputReaders, 0);
  }
});
test("diagnostic validation compares every complete ordered menu field and list order", t => {
  for (const field of [...Object.keys(diagnosticMenu("grade10")), "field-order", "list-order", "accessor"]) {
    const { f, driver } = atDiagnostic(t);let actual = clone(diagnosticMenu("grade10")), reads = 0;
    if (field === "field-order") actual = Object.fromEntries(Object.entries(actual).reverse());
    else if (field === "list-order") actual.tensors.reverse();
    else if (field === "accessor") Object.defineProperty(actual, "id", { enumerable: true, get() { reads++;return "grade10"; } });
    else if (Array.isArray(actual[field])) actual[field].push("manufactured-drift");
    else if (typeof actual[field] === "number") actual[field]++;
    else actual[field] += "-drift";
    const calls = f.calls.length;
    assert.throws(() => driver.validateDiagnosticMenu("grade10", actual), /menu|own data/);
    assert.equal(reads, 0);assert.equal(f.calls.length, calls);assert.equal(f.admissions.at(-1).operation, "diagnostic-menu");
    assert.equal(driver.snapshot().menusMatched, 0);assert.equal(driver.snapshot().failed, true);
  }
});
test("diagnostic admission is prospective synchronous and fail-closed on swallowed reentry", t => {
  for (const mode of ["throw", "async", "reentry"]) {
    const { f, driver } = atDiagnostic(t), before = driver.snapshot().snapshotUsage, calls = f.calls.length;
    f.setObserver(stage => {
      assert.equal(stage.operation, "diagnostic-menu");assert.equal(stage.contextId, "diagnostic/grade10");assert.ok(Object.isFrozen(stage.details.caps));
      if (mode === "throw") throw Error("manufactured menu refusal");
      if (mode === "async") return Promise.resolve();
      assert.throws(() => driver.validateDiagnosticMenu("grade10", diagnosticMenu("grade10")));
    });
    assert.throws(() => driver.validateDiagnosticMenu("grade10", diagnosticMenu("grade10")));
    assert.equal(f.calls.length, calls);assert.deepEqual(driver.snapshot().snapshotUsage, before);
    assert.equal(driver.snapshot().menusMatched, 0);assert.equal(driver.snapshot().failed, true);
  }
});
test("diagnostic snapshots charge both independent and submitted menus and do not retain caller authority", t => {
  const { f, driver } = atDiagnostic(t), actual = clone(diagnosticMenu("grade10")), before = driver.snapshot().snapshotUsage;
  const usage = snapshotCanonicalMetadata(actual, quota()).usage;
  assert.equal(JSON.stringify(driver.validateDiagnosticMenu("grade10", actual)), JSON.stringify({ contextId: "diagnostic/grade10", scientificExecutionAuthorized: false }));
  const after = driver.snapshot().snapshotUsage;
  for (const key of Object.keys(before)) assert.equal(after[key] - before[key], 2 * usage[key]);
  actual.tensors.length = 0;
  driver.prepareNext(declaration(704), fullTemplate(704));driver.acceptCheckpoint(f.observed(704));
  assert.equal(driver.snapshot().menusMatched, 1);
  const g = fixture(t);g.suppliedLimits.snapshot.stringCharacters = before.stringCharacters + usage.stringCharacters - 1;
  const bounded = g.create();for (let i = 0; i < 704; i++) g.step(bounded, i);
  const calls = g.calls.length;assert.throws(() => bounded.validateDiagnosticMenu("grade10", diagnosticMenu("grade10")), /quota|ceiling/);
  assert.equal(g.calls.length, calls);assert.equal(bounded.snapshot().failed, true);assert.equal(bounded.snapshot().menusMatched, 0);
});
test("declaration drift fails before source reads construction or admission", t => {
  const f = fixture(t), driver = f.create(), d = declaration(0);d.marks[0].expanded = !d.marks[0].expanded;
  assert.throws(() => driver.prepareNext(d, fullTemplate(0)), /commitment|declaration/);assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);
  assert.equal(driver.snapshot().failed, true);assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)), /failed|poison/);
});
test("pending context cannot be replaced skipped or checkpointed before preparation", t => {
  const f = fixture(t), driver = f.create();assert.throws(() => driver.acceptCheckpoint({}));assert.equal(f.calls.length, 0);
  const g = fixture(t), d = g.create();d.prepareNext(declaration(0), fullTemplate(0));const n = g.calls.length;
  assert.throws(() => d.prepareNext(declaration(0), fullTemplate(0)));assert.equal(g.calls.length, n);assert.equal(d.snapshot().failed, true);
});
test("refused asynchronous and reentrant admission stop before first source constructor", t => {
  for (const mode of ["throw", "async", "reentry", "snapshot"]) {
    const f = fixture(t), driver = f.create();f.setObserver(() => {
      if (mode === "throw") throw Error("manufactured refused admission");
      if (mode === "async") return Promise.resolve();
      if (mode === "snapshot") driver.snapshot();
      else assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)));
    });
    assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)));assert.equal(f.calls.length, 0);assert.equal(driver.snapshot().failed, true);
  }
});
test("frozen preparation caps and declarations survive caller mutation during admission", t => {
  const f = fixture(t), driver = f.create(), d = declaration(0), candidate = fullTemplate(0);
  f.setObserver(() => { f.suppliedPolicy.profiles[0].recipe.nodeLimit = 1;f.suppliedLimits.snapshot.nodes = 1;d.graphPath = "mutated";
    candidate.resources.trace.nodes = 1;candidate.leaves[0].source = "mutated"; });
  driver.prepareNext(d, candidate);assert.equal(f.calls.find(c => c.operation === "recipe").value.options.nodeLimit, 10000000);
  driver.acceptCheckpoint(f.observed(0));assert.equal(driver.snapshot().failed, false);
});
test("source construction failure is sticky and never healed by retry", t => {
  for (const operation of ["inputs", "algebra", "baseline", "bind", "polynomial", "point-adapter", "recipe"]) {
    const f = fixture(t), driver = f.create();f.failure.operation = operation;
    assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)), /manufactured/);const n = f.calls.length;
    f.failure.operation = null;assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)));assert.equal(f.calls.length, n);assert.equal(driver.snapshot().failed, true);
  }
});
test("bad producer checkpoint and false replay reports cannot advance owner", t => {
  for (const mode of ["hash", "transition"]) {
    const f = fixture(t), driver = f.create();driver.prepareNext(declaration(0), fullTemplate(0));const pins = f.observed(0);
    if (mode === "hash") pins.graph.sha256 = "0".repeat(64);else f.states[0].skipReplay = true;
    assert.throws(() => driver.acceptCheckpoint(pins));assert.equal(driver.snapshot().failed, true);
  }
});
test("late configured provenance failure stops preparation before new source work", t => {
  const f = fixture(t), driver = f.create();f.step(driver, 0);const n = f.calls.length;f.configurationHealth.failed = true;
  assert.throws(() => driver.prepareNext(declaration(1), fullTemplate(1)));assert.equal(f.calls.length, n);assert.equal(driver.snapshot().failed, true);
});
test("invalid policy routes roles root and metadata quotas reject without source work", t => {
  for (const mutate of [f => f.suppliedPolicy.routes.pop(), f => f.suppliedPolicy.routes.reverse(),
    f => { f.suppliedPolicy.routes[0].profile = 1; }, f => { f.suppliedPolicy.repositoryRoot = "/"; },
    f => { f.suppliedPolicy.profiles[3].algebra = algebra(); }, f => { f.suppliedLimits.snapshot.nodes = 1; }]) {
    const f = fixture(t);mutate(f);assert.throws(() => f.create());assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);
  }
});
test("own-data policy and declaration capture never invokes accessors", t => {
  const f = fixture(t);let gets = 0;
  Object.defineProperty(f.suppliedPolicy, "repositoryRoot", { enumerable: true, get() { gets++;return f.root; } });
  assert.throws(() => f.create(), /own data/);assert.equal(gets, 0);assert.equal(f.calls.length, 0);
  const g = fixture(t), driver = g.create(), d = declaration(0);
  Object.defineProperty(d, "marks", { enumerable: true, get() { gets++;return []; } });
  assert.throws(() => driver.prepareNext(d, fullTemplate(0)), /own data/);assert.equal(gets, 0);assert.equal(g.calls.length, 0);
});
test("early point final cannot skip unprepared or unreplayed children", t => {
  const f = fixture(t), driver = f.create();f.step(driver, 0);const n = f.calls.length;
  assert.throws(() => driver.completePoint(declaration(351), f.observed(351)));
  assert.equal(f.calls.length, n);assert.equal(driver.snapshot().failed, true);
});
test("checkpoint pins are detached before replay admission can mutate caller input", t => {
  const f = fixture(t), driver = f.create();driver.prepareNext(declaration(0), fullTemplate(0));const pins = f.observed(0);
  f.setObserver(s => { if (s.operation === "checkpoint-read-replay") pins.graph.sha256 = "0".repeat(64); });
  driver.acceptCheckpoint(pins);assert.equal(driver.snapshot().failed, false);assert.equal(driver.snapshot().acceptedEvents, 1);
});
test("replay or metadata-export admission refusal prevents that stage and releases active ownership", t => {
  for (const denied of ["retention-policy", "leaf-declaration-export", "checkpoint-read-replay"]) {
    const f = fixture(t), driver = f.create();f.setObserver(s => { if (s.operation === denied) throw Error("stage denied " + denied); });
    assert.throws(() => f.step(driver, 0), /stage denied/);
    assert.equal(f.calls.filter(c => c.operation === "replay").length, 0);
    assert.equal(driver.snapshot().retainedActivePoints, 0);assert.equal(driver.snapshot().retainedInputReaders, 0);
    assert.equal(driver.snapshot().pendingContextId, null);assert.equal(driver.snapshot().failed, true);
  }
});
test("producer abort releases pending and active sources and cannot be healed", t => {
  for (const accepted of [false, true]) {
    const f = fixture(t), driver = f.create();driver.prepareNext(declaration(0), fullTemplate(0));if (accepted) driver.acceptCheckpoint(f.observed(0));
    driver.abort();driver.abort();const s = driver.snapshot();assert.equal(s.failed, true);assert.equal(s.completed, false);
    assert.equal(s.retainedActivePoints, 0);assert.equal(s.retainedInputReaders, 0);assert.equal(s.pendingContextId, null);
    const n = f.calls.length;assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)));assert.equal(f.calls.length, n);
  }
});
test("production driver refuses caller-made preflight results without touching retained sources", t => {
  const f = fixture(t), { createSourcePreparationDriver } = actualRequire("./a68-source-preparation-driver");
  assert.throws(() => createSourcePreparationDriver({}, f.suppliedPolicy, f.suppliedLimits, () => {}), /private clean configured process/);
});
test("producer handshake is mandatory single-use and binds both full profile and capture", t => {
  for (const mode of ["missing", "profile", "capture", "repeat"]) {
    const f = fixture(t), driver = f.create(false);
    if (mode === "missing") assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)), /handshake/);
    else {
      if (mode === "profile") f.producerProfile.budgets[0].resources.trace.nodes--;
      if (mode === "capture") f.producerCapture.limits.normal.graphs--;
      if (mode === "repeat") driver.beginProducer(f.producerProfile, f.producerCapture);
      assert.throws(() => driver.beginProducer(f.producerProfile, f.producerCapture), /profile|handshake|capture/);
    }
    assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);assert.equal(driver.snapshot().failed, true);
    assert.throws(() => driver.beginProducer(clone(originalProducerProfile), clone(originalProducerProfile.capture)), /failed/);
  }
});
test("producer profile capture and full-template accessors reject without invoking getters", t => {
  for (const mode of ["profile", "capture", "template"]) {
    const f = fixture(t), driver = f.create(mode === "template");let reads = 0;
    const target = mode === "profile" ? f.producerProfile : mode === "capture" ? f.producerCapture : fullTemplate(0);
    const key = mode === "profile" ? "retention" : mode === "capture" ? "inspectionPolicy" : "resources";
    Object.defineProperty(target, key, { enumerable: true, get() { reads++;return {}; } });
    assert.throws(() => mode === "template" ? driver.prepareNext(declaration(0), target) : driver.beginProducer(f.producerProfile, f.producerCapture), /own data/);
    assert.equal(reads, 0);assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);assert.equal(driver.snapshot().failed, true);
  }
});
test("full-template changes omitted from compact checkpoints reject before all source admissions", t => {
  for (const mutate of [candidate => { candidate.resources.trace.nodes--; }, candidate => { candidate.pointCheckpoint.bytes--; },
    candidate => { candidate.callbacks.callbacks.pop(); }, candidate => { candidate.leafRoles = []; },
    candidate => { candidate.leaves[0].source += "#changed"; }]) {
    const f = fixture(t), driver = f.create(), candidate = fullTemplate(0), expected = declaration(0);
    const originalHash = hash(wire(candidate));mutate(candidate);
    assert.notEqual(hash(wire(candidate)), originalHash);
    assert.deepEqual({ contextId: candidate.id, graphPath: candidate.graphPath, metadataPath: candidate.pointCheckpoint.relativePath, marks: candidate.marks }, expected);
    assert.throws(() => driver.prepareNext(expected, candidate), /full regenerated producer template/);
    assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);assert.equal(driver.snapshot().failed, true);
    assert.equal(driver.snapshot().producerTemplatesMatched, 0);
  }
});
test("missing or out-of-order full template cannot be substituted by a compact declaration", t => {
  for (const candidate of [undefined, null, declaration(0), fullTemplate(1)]) {
    const f = fixture(t), driver = f.create();
    assert.throws(() => driver.prepareNext(declaration(0), candidate));
    assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);assert.equal(driver.snapshot().failed, true);
  }
});
test("source leaf export binds complete ordered triples and canonical hash syntax", t => {
  for (const mutate of [leaves => leaves.reverse(), leaves => leaves.pop(), leaves => { leaves[0].id += "/other"; },
    leaves => { leaves[0].degree = 0; }, leaves => { leaves[0].source += "#other"; },
    leaves => { leaves[0].sha256 = "A".repeat(64); }, leaves => { delete leaves[0].sha256; }]) {
    const f = fixture(t), driver = f.create();f.setLeafMutation(mutate);
    assert.throws(() => driver.prepareNext(declaration(0), fullTemplate(0)), /leaf|ordered fields/);
    assert.equal(f.admissions.at(-1).operation, "leaf-declaration-export");
    assert.equal(f.calls.filter(c => c.operation === "replay").length, 0);
    const status = driver.snapshot();assert.equal(status.failed, true);assert.equal(status.producerTemplatesMatched, 0);
    assert.equal(status.pendingContextId, null);assert.equal(status.retainedActivePoints, 0);assert.equal(status.retainedInputReaders, 0);
  }
});
test("full-template snapshot quota exhaustion happens before retained reads or source admission", t => {
  for (const field of ["nodes", "arraySlots", "stringCharacters"]) {
    const f = fixture(t), measuring = f.create(), used = measuring.snapshot().snapshotUsage[field];measuring.abort();
    const candidate = fullTemplate(0), d = declaration(0);
    const required = snapshotCanonicalMetadata(d, quota()).usage[field] + snapshotCanonicalMetadata(candidate, quota()).usage[field];
    f.suppliedLimits.snapshot[field] = used + required - 1;
    const driver = f.create();assert.throws(() => driver.prepareNext(d, candidate), /ceiling|quota/);
    assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);assert.equal(driver.snapshot().failed, true);
  }
});
test("swallowed producer-handshake and template-snapshot reentry cannot authorize source work", t => {
  for (const mode of ["handshake", "template"]) {
    const f = fixture(t), driver = f.create(mode === "template");
    const value = new Proxy(mode === "handshake" ? f.producerProfile : fullTemplate(0), { ownKeys(target) {
      assert.throws(() => driver.beginProducer(f.producerProfile, f.producerCapture), /reentrant/);return Reflect.ownKeys(target);
    } });
    assert.throws(() => mode === "handshake" ? driver.beginProducer(value, f.producerCapture) : driver.prepareNext(declaration(0), value), /reentry|failure|failed/);
    assert.equal(f.calls.length, 0);assert.equal(f.admissions.length, 0);assert.equal(driver.snapshot().failed, true);
  }
});
test("handshake snapshots resist later caller mutation while capturing the sibling declaration", t => {
  const f = fixture(t), driver = f.create(false);
  const capture = new Proxy(f.producerCapture, { ownKeys(target) { f.producerProfile.retention.background = true;return Reflect.ownKeys(target); } });
  driver.beginProducer(f.producerProfile, capture);
  assert.equal(driver.snapshot().producerProfileMatched, true);assert.equal(driver.snapshot().failed, false);
  driver.prepareNext(declaration(0), fullTemplate(0));assert.equal(driver.snapshot().producerTemplatesMatched, 1);
  driver.acceptCheckpoint(f.observed(0));assert.equal(driver.snapshot().failed, false);
});

"use strict";
// Exact dispatcher code + REAL pinned readers, but VM-only manufactured
// adapter identities. No source brands, scientific coefficients or positive
// scientific replay are created by this lifecycle/routing test seam.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto"), vm = require("node:vm");
const { createRequire } = require("node:module");
const production = require("../a68-source-checkpoint-dispatcher");
const { pointCheckNames, pointCheckpointIdentity } = require("../a68-point-checkpoint");
const { contextCheckpointIdentity } = require("../a68-context-checkpoint");
const { pointCompletionIdentity } = require("../a68-point-completion");
const filename = path.resolve(__dirname, "../a68-source-checkpoint-dispatcher.js"), actualRequire = createRequire(filename);
const wire = x => JSON.stringify(x) + "\n", hash = x => crypto.createHash("sha256").update(x).digest("hex");
const quota = () => ({ nodes: 100000, arraySlots: 100000, stringCharacters: 1000000, maxDepth: 16 });
const limits = () => ({ configuration: quota(), event: quota(), total: { ...quota(), nodes: 1000000, arraySlots: 1000000, stringCharacters: 10000000 } });
function environment(t) {
  const processResults = new WeakMap(), catalogHealth = { failed: false };
  const brands = new WeakMap(), lookup = kind => adapter => {
    const b = brands.get(adapter); assert.ok(b && (kind === "diagnostic" ? b.kind === kind : b.kind !== "diagnostic"), "private manufactured adapter identity");
    return Object.freeze({ kind: b.kind, contextId: b.id, parentIdentity: b.parent, identity: b.identity,
      prepared: b.prepared, replayed: b.replayed, completed: b.completed, health: b.health });
  };
  const module = { exports: {} };
  const requireFixture = name => name === "./a68-source-orchestration" ? { sourceAdapterIdentity: lookup("source") } :
    name === "./a68-diagnostic-source-adapter" ? { diagnosticAdapterIdentity: lookup("diagnostic") } :
    name === "./a68-template-process-host" ? { validatedCheckpointConfiguration: value => {
      const found = processResults.get(value); assert.ok(found, "private manufactured process result"); return found;
    } } : actualRequire(name);
  vm.runInNewContext(fs.readFileSync(filename, "utf8"), { require: requireFixture, module }, { filename });
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-dispatch-")); t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const read = { graphBytes: 100000, metadataBytes: 100000, totalReadBytes: 200002, snapshot: quota() };
  const finalRead = { backgroundBytes: 100000, metadataBytes: 100000, totalReadBytes: 200002, snapshot: quota() };
  const declaration = i => {
    const e = production.checkpointEvent(i), id = e.contextId;
    return e.kind === "final" ? { contextId: id, graphPath: id + "/graph.json", backgroundPath: id + "/background-checkpoint.json", metadataPath: id + "/metadata.json" } :
      { contextId: id, graphPath: id + "/graph.json", metadataPath: id + (e.kind === "background" ? "/background-checkpoint.json" : "/metadata.json"), marks: [] };
  };
  const config = { outputRoot: root, events: Array.from({ length: 707 }, (_, i) => ({ ...production.checkpointEvent(i), declarationSha256: hash(wire(declaration(i))), profile: production.checkpointEvent(i).kind === "final" ? 1 : 0 })),
    profiles: [{ read, replay: { manufactured: true } }, { read: finalRead, replay: null }] };
  const backgrounds = new Map(), states = [], calls = []; let active = null;
  const artifact = (file, value) => { const bytes = wire(value); fs.mkdirSync(path.dirname(path.join(root, file)), { recursive: true });fs.writeFileSync(path.join(root, file), bytes);return { path: file, bytes: Buffer.byteLength(bytes), sha256: hash(bytes) }; };
  function input(i) {
    const e = production.checkpointEvent(i), id = e.contextId, d = declaration(i);
    if (e.kind === "final") {
      const background = backgrounds.get(id), final = { schema: "phase627-point-computational-completion-v1", context: id, status: "producer-complete", independentValidationComplete: false,
        backgroundCheckpoint: background.observed.metadata, metadata: background.metadata };
      return { adapter: active, declaration: d, observed: { context: id, background: background.observed, metadata: artifact(d.metadataPath, final) }, state: brands.get(active) };
    }
    const graph = artifact(d.graphPath, { schemaVersion: "phase627-typed-mixed-dag-v1", leaves: [], nodes: [], marks: [] });
    const point = id === "point0" ? 0 : 1;
    const metadata = e.kind === "background" ? { "background/geometry": { point, frame: [], inverseFrame: [], metric: {}, connection: {}, frameLift: [], framePartial: [], omega: [] },
      ...Object.fromEntries(pointCheckNames(point).map(name => ["check/" + name, true])) } : { "check/manufactured": true };
    const envelope = e.kind === "background" ? { schema: "phase627-point-background-checkpoint-v1", context: id, status: "background-sealed", pointTraversalComplete: false, graph, metadata } :
      { schema: "phase627-context-computational-evidence-v1", context: id, status: "producer-complete", independentValidationComplete: false, graph, metadata };
    const observed = { context: id, graph, metadata: artifact(d.metadataPath, envelope) };
    const state = { kind: e.kind === "background" ? "point" : e.kind, id, identity: Object.freeze({}), parent: e.kind === "germ" ? brands.get(active).identity : null,
      prepared: true, replayed: false, completed: false, failed: false, callback: null, skip: false };
    state.health = () => ({ failed: state.failed }); // TEST ONLY, not production lifetime evidence
    function replay(token, cap) {
      const pinned = e.kind === "background" ? pointCheckpointIdentity(token) : contextCheckpointIdentity(token);
      assert.equal(pinned.declaration.contextId, id);assert.equal(pinned.root, root);assert.equal(cap.manufactured, true);assert.ok(Object.isFrozen(cap));
      calls.push(id);if (state.callback) state.callback();if (!state.skip) state.replayed = true;
      return { scope: { checkpointNumericallyReplayed: true } }; // ignored by dispatcher
    }
    const adapter = Object.freeze({ replayPointCheckpoint: replay, replayContextCheckpoint: replay, acceptPointCompletion: token => {
      assert.equal(pointCompletionIdentity(token).declaration.contextId, id);calls.push(id + "/final");if (state.callback) state.callback();if (!state.skip) state.completed = true;
    } });
    brands.set(adapter, state);states.push(state);
    if (e.kind === "background") { active = adapter;backgrounds.set(id, { observed, metadata }); }
    return { adapter, declaration: d, observed, state };
  }
  const dispatch = (driver, f) => driver.dispatch(f.adapter, f.declaration, f.observed);
  const preflight = Object.freeze({});processResults.set(preflight, { configuration: config, health: () => catalogHealth });
  return { module: module.exports, config, input, dispatch, states, calls, catalogHealth,
    create: (cap = limits()) => module.exports.createSourceCheckpointDispatcher(preflight, cap) };
}
test("fixed707 order has705 distinct contexts and350 children before each final", () => {
  const events = Array.from({ length: 707 }, (_, i) => production.checkpointEvent(i));
  assert.equal(new Set(events.map(e => e.contextId)).size, 705);
  assert.deepEqual(events.filter(e => e.kind === "final").map(e => e.contextId), ["point0", "point1"]);
  assert.equal(events.filter(e => e.kind === "germ").length, 700);
  for (const i of [-1, 707, 0.5, NaN]) assert.throws(() => production.checkpointEvent(i));
});
test("all707 manufactured events use actual pinned readers and release active point at each final", t => {
  const f = environment(t), d = f.create();
  for (let i = 0; i < 707; i++) {
    const result = f.dispatch(d, f.input(i));assert.equal(result.event, i);assert.equal(result.scientificExecutionAuthorized, false);
    if ([351, 703].includes(i)) assert.equal(d.snapshot().retainedActivePointAdapters, 0);
  }
  const s = d.snapshot();assert.equal(s.completed, true);assert.equal(s.acceptedEvents, 707);assert.equal(s.retainedHealthChecks, 705);
  assert.ok(Object.values(s.scope).every(x => x === false));assert.equal(f.calls.length, 707);
  f.states[88].failed = true;assert.equal(d.snapshot().completed, false);assert.equal(d.snapshot().failed, true);
});
test("production rejects raw or copied configuration instead of accepting caller hashes", t => {
  const f = environment(t);
  for (const value of [f.config, { configuration: f.config, health: () => ({ failed: false }) }, {}, null])
    assert.throws(() => production.createSourceCheckpointDispatcher(value, limits()), /private clean configured process/);
  assert.equal(f.calls.length, 0);
});
test("a plain success report without a live replay transition cannot advance", t => {
  const f = environment(t), d = f.create(), first = f.input(0);first.state.skip = true;
  assert.throws(() => f.dispatch(d, first), /postcondition/);assert.equal(d.snapshot().acceptedEvents, 0);
  first.state.skip = false;assert.throws(() => f.dispatch(d, first), /failed/);
});
test("same point number with different private parent identity is rejected", t => {
  const f = environment(t), d = f.create();f.dispatch(d, f.input(0));const child = f.input(1);child.state.parent = Object.freeze({});
  assert.throws(() => f.dispatch(d, child), /exact active parent/);assert.equal(d.snapshot().acceptedEvents, 1);
});
test("out of order unprepared pre-replayed or unhealthy adapters poison dispatcher", t => {
  for (const change of [s => s.id = "point1", s => s.prepared = false, s => s.replayed = true, s => s.failed = true]) {
    const f = environment(t), d = f.create(), first = f.input(0);change(first.state);assert.throws(() => f.dispatch(d, first));assert.equal(d.snapshot().failed, true);
  }
});
test("frozen commitments and limits cannot be changed after construction", t => {
  const f = environment(t), cap = limits(), d = f.create(cap), first = f.input(0);
  f.config.events[0].declarationSha256 = "0".repeat(64);f.config.profiles[0].read.graphBytes = 1;cap.event.nodes = 1;
  f.dispatch(d, first);assert.equal(d.snapshot().acceptedEvents, 1);
  const child = f.input(1);child.declaration.marks.push({ unexpected: true });assert.throws(() => f.dispatch(d, child), /commitment/);
});
test("corrupt pinned file stops before adapter callback and cannot be retried", t => {
  const f = environment(t), d = f.create(), first = f.input(0);first.observed.graph.sha256 = "0".repeat(64);
  assert.throws(() => f.dispatch(d, first), /same-buffer/);assert.equal(f.calls.length, 0);assert.equal(d.snapshot().failed, true);
});
test("swallowed dispatch or snapshot reentry still poisons the outer event", t => {
  for (const mode of ["dispatch", "snapshot"]) {
    const f = environment(t), d = f.create(), first = f.input(0);
    first.state.callback = () => { if (mode === "snapshot") d.snapshot();else assert.throws(() => f.dispatch(d, first)); };
    assert.throws(() => f.dispatch(d, first), /failure|reentry/);assert.equal(d.snapshot().acceptedEvents, 0);
  }
});
test("own-data capture rejects getters without invoking them and meters cumulative snapshots", t => {
  const f = environment(t), d = f.create(), first = f.input(0);let reads = 0;
  Object.defineProperty(first.observed, "context", { get() { reads++;return "point0"; } });
  assert.throws(() => f.dispatch(d, first), /own data/);assert.equal(reads, 0);
  const g = environment(t), cap = limits();cap.event.nodes = 1;const e = g.create(cap);
  assert.throws(() => g.dispatch(e, g.input(0)), /ceiling/);assert.equal(g.calls.length, 0);
  const h = environment(t), tiny = limits();tiny.total.nodes = 1;assert.throws(() => h.create(tiny), /ceiling/);
});
test("incomplete or reordered configuration cannot create dispatcher", t => {
  for (const mode of ["missing", "order", "digest", "profile", "finalReplay"]) {
    const f = environment(t);
    if (mode === "missing") f.config.events.pop();
    if (mode === "order") [f.config.events[0], f.config.events[1]] = [f.config.events[1], f.config.events[0]];
    if (mode === "digest") f.config.events[0].declarationSha256 = "bogus";
    if (mode === "profile") f.config.events[0].profile = 999;
    if (mode === "finalReplay") f.config.profiles[1].replay = {};
    assert.throws(() => f.create());
  }
});
test("late accepted-parent failure is detected before the next child replay", t => {
  const f = environment(t), d = f.create(), first = f.input(0);f.dispatch(d, first);first.state.failed = true;
  assert.throws(() => f.dispatch(d, f.input(1)), /healthy accepted/);assert.equal(f.calls.length, 1);assert.equal(d.snapshot().retainedActivePointAdapters, 0);
});
test("point final requires the exact active adapter and live completion transition", t => {
  for (const mode of ["replacement", "noCompletion", "early"]) {
    const f = environment(t), d = f.create();
    const count = mode === "early" ? 350 : 351;
    for (let i = 0; i < count; i++) f.dispatch(d, f.input(i));
    const final = f.input(351);
    if (mode === "replacement") final.adapter = f.input(0).adapter;
    if (mode === "noCompletion") final.state.skip = true;
    assert.throws(() => f.dispatch(d, final));assert.equal(d.snapshot().failed, true);assert.equal(d.snapshot().acceptedEvents, count);
  }
});
test("dispatcher cannot accept extra events after terminal traversal", t => {
  const f = environment(t), d = f.create();let last;
  for (let i = 0; i < 707; i++) { last = f.input(i);f.dispatch(d, last); }
  assert.equal(d.snapshot().completed, true);assert.throws(() => f.dispatch(d, last), /exhausted/);
  assert.equal(d.snapshot().completed, false);assert.equal(d.snapshot().failed, true);
});
test("all cumulative configuration caps reject before visiting event records", t => {
  for (const mode of ["nodes", "arraySlots", "stringCharacters", "maxDepth"]) {
    const f = environment(t), cap = limits();cap.total[mode] = 1;let visits = 0;
    f.config.events[0] = new Proxy(f.config.events[0], { ownKeys(value) { visits++;return Reflect.ownKeys(value); } });
    assert.throws(() => f.create(cap), /ceiling/);assert.equal(visits, 0);
  }
});
test("late configuration provenance failure invalidates an otherwise healthy dispatcher", t => {
  const f = environment(t), d = f.create();f.dispatch(d, f.input(0));f.catalogHealth.failed = true;
  assert.throws(() => f.dispatch(d, f.input(1)), /healthy validated checkpoint configuration/);
  assert.equal(d.snapshot().failed, true);assert.equal(d.snapshot().acceptedEvents, 1);
});
test("owner abort releases the active point and permanently prevents replay or completion", t => {
  const f = environment(t), d = f.create();f.dispatch(d, f.input(0));assert.equal(d.snapshot().retainedActivePointAdapters, 1);
  d.abort();assert.equal(d.snapshot().retainedActivePointAdapters, 0);assert.equal(d.snapshot().failed, true);assert.equal(d.snapshot().completed, false);
  d.abort();assert.throws(() => f.dispatch(d, f.input(1)), /failed/);assert.equal(f.calls.length, 1);
});

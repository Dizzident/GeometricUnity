"use strict";
// Exact production transport with explicit VM-only private host/driver doubles.
// These tiny manufactured templates are NOT independent source validation;
// no GU source constructor, retained reader or scientific entry point runs.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), crypto = require("node:crypto"), vm = require("node:vm");
const { createRequire } = require("node:module");
const { Readable, Writable, PassThrough } = require("node:stream");
const { sourceContextIds, diagnosticMenu } = require("../a68-source-context-menu");
const { checkpointEvent } = require("../a68-source-checkpoint-dispatcher");
const { snapshotCanonicalMetadata } = require("../a68-canonical-metadata");
const file = path.resolve(__dirname, "../a68-source-preparation-service.js"), realRequire = createRequire(file);
const REQUEST = "phase627-source-preparation-request-v1", REPLY = "phase627-source-preparation-reply-v1";
const wire = value => Buffer.from(JSON.stringify(value) + "\n", "ascii"), digest = value => crypto.createHash("sha256").update(value).digest("hex");
const clone = value => JSON.parse(JSON.stringify(value));
const snapshotLimits = { nodes: 1000000, arraySlots: 1000000, stringCharacters: 20000000, maxDepth: 16 };
const limits = { frameBytes: 200000, totalInputBytes: 20000000, replyBytes: 20000, totalOutputBytes: 4000000, timeoutMs: 10000 };
const ids = sourceContextIds(), profile = { manufactured: true, capture: { manufactured: true } };
const template = id => ({ id, graphPath: id + "/graph.json", metadataPath: id + "/metadata.json",
  pointCheckpoint: /^point[01]$/.test(id) ? { relativePath: id + "/background-checkpoint.json", bytes: 10 } : null,
  marks: [], leaves: [{ id: id + "/leaf", degree: 1, source: "manufactured" }], resources: { manufactured: true } });
const producerCommitments = { profileSha256: digest(wire(profile)), captureSha256: digest(wire(profile.capture)),
  templates: ids.map(contextId => ({ contextId, templateSha256: digest(wire(template(contextId))) })) };
const request = (sequence, operation, payload = null) => ({ schema: REQUEST, sequence, operation, payload });
const begin = () => request(0, "begin", { profile, capture: profile.capture });
function prepass() { return [begin(), ...ids.map((id, i) => request(i + 1, "validate", template(id))), request(706, "freeze")]; }
function allRequests() {
  const requests = prepass();
  for (let i = 0; i < 707; i++) {
    const event = checkpointEvent(i);
    if (event.kind === "diagnostic") {
      const diagnostic = event.contextId.slice("diagnostic/".length);
      requests.push(request(requests.length, "diagnostic-menu", { diagnostic, menu: diagnosticMenu(diagnostic) }));
    }
    if (event.kind !== "final") requests.push(request(requests.length, "prepare", template(event.contextId)));
    requests.push(request(requests.length, event.kind === "final" ? "pointfinal" : "checkpoint", { context: event.contextId }));
  }
  requests.push(request(requests.length, "finish"));assert.equal(requests.length, 2123);return requests;
}
function fixture() {
  const result = Object.freeze({}), configHealth = { failed: false }, calls = [];
  const state = { failed: false, begun: false, prepared: 0, events: 0, menusMatched: 0, acceptedMenu: null, pending: null, active: null, failOperation: null, failError: null, onConstruct: null };
  const preparationPolicy = { manufactured: true }, preparationLimits = { manufactured: true };
  const record = operation => { calls.push(operation);if (state.failOperation === operation) throw state.failError ?? Error("manufactured driver " + operation + " failure"); };
  const driver = {
    abort() { calls.push("abort");state.failed = true;state.pending = null;state.active = null; },
    beginProducer(p, c) { record("begin");assert.equal(digest(wire(p)), producerCommitments.profileSha256);assert.equal(digest(wire(c)), producerCommitments.captureSha256);state.begun = true; },
    validateDiagnosticMenu(diagnostic, actual) {
      record("diagnostic-menu");assert.equal(state.events, 704 + state.menusMatched);assert.equal(state.acceptedMenu, null);
      const contextId = "diagnostic/" + diagnostic;assert.equal(checkpointEvent(state.events).contextId, contextId);
      assert.equal(JSON.stringify(actual), JSON.stringify(diagnosticMenu(diagnostic)));
      state.acceptedMenu = contextId;state.menusMatched++;return { contextId, scientificExecutionAuthorized: false };
    },
    prepareNext(declaration, value) {
      record("prepare");assert.equal(state.begun, true);assert.equal(state.pending, null);
      const event = checkpointEvent(state.events);assert.notEqual(event.kind, "final");assert.equal(value.id, event.contextId);
      if (event.kind === "diagnostic") { assert.equal(state.acceptedMenu, event.contextId);state.acceptedMenu = null; }
      assert.equal(digest(wire(value)), producerCommitments.templates[state.prepared].templateSha256);
      assert.equal(JSON.stringify(declaration), JSON.stringify({ contextId: value.id, graphPath: value.graphPath,
        metadataPath: event.kind === "background" ? value.pointCheckpoint.relativePath : value.metadataPath, marks: value.marks }));
      state.prepared++;state.pending = value.id;if (event.kind === "background") state.active = value.id;
      return { contextId: value.id, leaves: value.leaves.map(l => ({ ...l, sha256: "a".repeat(64) })) };
    },
    acceptCheckpoint(observed) {
      record("checkpoint");const event = checkpointEvent(state.events);assert.equal(observed.context, state.pending);
      state.pending = null;return { event: state.events++, kind: event.kind, contextId: event.contextId, scientificExecutionAuthorized: false };
    },
    completePoint(declaration, observed) {
      record("pointfinal");const event = checkpointEvent(state.events);assert.equal(event.kind, "final");assert.equal(state.pending, null);assert.equal(state.active, event.contextId);
      assert.equal(observed.context, event.contextId);
      assert.equal(JSON.stringify(declaration), JSON.stringify({ contextId: event.contextId, graphPath: event.contextId + "/graph.json",
        backgroundPath: event.contextId + "/background-checkpoint.json", metadataPath: event.contextId + "/metadata.json" }));
      state.active = null;return { event: state.events++, kind: event.kind, contextId: event.contextId, scientificExecutionAuthorized: false };
    },
    snapshot() { return { failed: state.failed, acceptedEvents: state.events, producerTemplatesMatched: state.prepared, producerProfileMatched: state.begun, menusMatched: state.menusMatched,
      completed: !state.failed && state.events === 707, pendingContextId: state.pending, retainedActivePoints: state.active ? 1 : 0,
      retainedInputReaders: state.events > 0 && state.events < 707 ? 1 : 0 }; }
  };
  const mocks = {
    "./a68-template-process-host": { validatedCheckpointConfiguration(value) { assert.equal(value, result);return { producerCommitments, health: () => configHealth }; } },
    "./a68-source-preparation-driver": { createSourcePreparationDriver(value, policy, caps, admitStage) {
      assert.equal(value, result);assert.equal(Object.isFrozen(policy), true);assert.equal(Object.isFrozen(caps), true);assert.equal(typeof admitStage, "function");
      state.onConstruct?.();record("construct");return driver;
    } }
  };
  const module = { exports: {} };vm.runInNewContext(fs.readFileSync(file, "utf8"), { module, Buffer, setTimeout, clearTimeout,
    require: name => Object.hasOwn(mocks, name) ? mocks[name] : realRequire(name) }, { filename: file });
  function run(frames, overrides = {}) {
    const replies = [], waiters = new Map(), input = overrides.input || Readable.from(frames);
    const output = overrides.output || new Writable({ write(chunk, encoding, callback) {
      replies.push(Buffer.from(chunk));overrides.onReply?.(replies.length, replies.at(-1));waiters.get(replies.length)?.();callback();
    } });
    const promise = module.exports.serveSourcePreparation({ input, output, preflightResult: result, preparationPolicy, preparationLimits,
      snapshotLimits: { ...snapshotLimits, ...overrides.snapshotLimits }, limits: { ...limits, ...overrides.limits }, admitStage() {} });
    return { input, output, replies, promise, waitForReplies(n) { return replies.length >= n ? Promise.resolve() : new Promise(resolve => waiters.set(n, resolve)); } };
  }
  return { run, calls, state, configHealth, preparationPolicy, preparationLimits };
}
test("all2123 manufactured requests bind second catalog freeze and private driver traversal", async () => {
  const f = fixture(), requests = allRequests(), session = f.run(requests.map(wire)), result = await session.promise;
  assert.equal(result.requests, 2123);assert.equal(session.replies.length, 2123);assert.equal(f.calls.includes("abort"), false);
  for (let i = 0; i < requests.length; i++) {
    const reply = JSON.parse(session.replies[i]);assert.equal(reply.schema, REPLY);assert.equal(reply.sequence, i);
    assert.equal(reply.operation, requests[i].operation);assert.equal(reply.requestSha256, digest(wire(requests[i])));assert.equal(reply.status, "accepted");
  }
  assert.deepEqual(JSON.parse(session.replies[1]).report, { contextId: "point0" });
  assert.deepEqual(JSON.parse(session.replies[706]).report, { contexts: 705, scientificExecutionAuthorized: false });
  assert.equal(JSON.parse(session.replies[707]).report.leaves[0].sha256, "a".repeat(64));
  assert.deepEqual(JSON.parse(session.replies[2122]).report, { contexts: 705, checkpoints: 707, scientificExecutionAuthorized: false });
  assert.equal(f.calls.filter(c => c === "diagnostic-menu").length, 3);assert.equal(f.state.menusMatched, 3);
  assert.equal(f.calls.filter(c => c === "prepare").length, 705);assert.equal(f.calls.filter(c => c === "checkpoint").length, 705);
  assert.equal(f.calls.filter(c => c === "pointfinal").length, 2);assert.equal(result.scope.scientificExecutionAuthorized, false);
  assert.equal(result.scope.fullPrerequisitesImplemented, false);assert.equal(result.scope.producerProcessIdentityEstablished, false);
});
test("diagnostic transport requires one matching menu before each prepare and rejects altered payloads", async () => {
  for (const mode of ["missing", "wrong", "repeated", "altered", "ordered-fields", "extra-field", "early"]) {
    const f = fixture(), requests = allRequests(), index = requests.findIndex(r => r.operation === "diagnostic-menu");
    let frames = requests.slice(0, index), candidate = clone(requests[index]);
    if (mode === "missing") candidate = { ...requests[index + 1], sequence: index };
    if (mode === "wrong") candidate.payload.diagnostic = "acceleration";
    if (mode === "altered") candidate.payload.menu.backgrounds++;
    if (mode === "ordered-fields") candidate.payload = { menu: candidate.payload.menu, diagnostic: candidate.payload.diagnostic };
    if (mode === "extra-field") candidate.payload.extra = true;
    if (mode === "early") { frames = prepass();candidate.sequence = frames.length; }
    frames.push(candidate);
    if (mode === "repeated") frames.push({ ...candidate, sequence: candidate.sequence + 1 });
    const session = f.run(frames.map(wire));await assert.rejects(session.promise);
    assert.equal(f.state.prepared, mode === "early" ? 0 : 702);assert.equal(f.state.failed, true);
    assert.equal(f.calls.filter(c => c === "diagnostic-menu").length, ["repeated", "altered"].includes(mode) ? 1 : 0);
    assert.equal(session.replies.length, frames.length - 1);
  }
});
test("second-producer template mismatch and early freeze fail before source preparation", async () => {
  for (const mode of ["mutated", "wrong-id", "skip", "early-freeze", "prepare-before-freeze"]) {
    const f = fixture(), candidate = template("point0");
    if (mode === "mutated") candidate.resources.manufactured = false;
    if (mode === "wrong-id") candidate.id = "point1";
    const second = request(mode === "skip" ? 2 : 1, mode === "early-freeze" ? "freeze" : mode === "prepare-before-freeze" ? "prepare" : "validate", candidate);
    const s = f.run([wire(begin()), wire(second)]);await assert.rejects(s.promise);
    assert.equal(s.replies.length, 1);assert.equal(f.calls.includes("prepare"), false);assert.equal(f.state.failed, true);
  }
});
test("missing handshake repeated begin and extra frame fields reject before driver work", async () => {
  for (const frame of [request(0, "validate", template("point0")), { ...begin(), extra: true }, { ...begin(), schema: "wrong" }, { ...begin(), sequence: 1 }]) {
    const f = fixture(), s = f.run([wire(frame)]);await assert.rejects(s.promise);assert.equal(s.replies.length, 0);assert.equal(f.calls.includes("begin"), false);
  }
  const f = fixture(), s = f.run([wire(begin()), wire(request(1, "begin", begin().payload))]);await assert.rejects(s.promise);assert.equal(s.replies.length, 1);
});
test("source operation order rejects checkpoint before prepare duplicates and premature final or finish", async () => {
  for (const operation of ["checkpoint", "pointfinal", "finish", "freeze"]) {
    const f = fixture(), frames = prepass();frames.push(request(frames.length, operation));
    const s = f.run(frames.map(wire));await assert.rejects(s.promise);assert.equal(f.calls.includes("prepare"), false);assert.equal(f.state.failed, true);
  }
  const f = fixture(), frames = prepass();frames.push(request(707, "prepare", template("point0")), request(708, "prepare", template("point0")));
  const s = f.run(frames.map(wire));await assert.rejects(s.promise, /checkpoint required/);assert.equal(f.state.pending, null);assert.equal(f.state.active, null);
});
test("canonical fragments and coalesced frames retain request identities", async () => {
  const first = wire(begin()), second = wire(request(1, "validate", template("point0")));
  for (const frames of [[Buffer.concat([first, second])], [first.subarray(0, 7), first.subarray(7), second.subarray(0, 8), second.subarray(8)]]) {
    const f = fixture(), s = f.run(frames);await assert.rejects(s.promise, /clean EOF/);assert.equal(s.replies.length, 2);
    assert.equal(JSON.parse(s.replies[1]).requestSha256, digest(second));assert.equal(f.state.failed, true);
  }
});
test("duplicate keys alternate numeric encoding whitespace and partial tails are rejected", async () => {
  const first = wire(begin());
  for (const frame of [Buffer.from(first.toString().replace('"sequence":0', '"sequence":0,"sequence":0')),
    Buffer.from(first.toString().replace('"sequence":0', '"sequence":0.0')), Buffer.concat([Buffer.from(" "), first]),
    first.subarray(0, first.length - 1), Buffer.concat([first, Buffer.from("{")])]) {
    const f = fixture(), s = f.run([frame]);await assert.rejects(s.promise);assert.equal(f.state.failed, true);
  }
});
test("bounded input and reply quotas fail before excess assembly or writes", async () => {
  const first = wire(begin());
  for (const cap of [{ frameBytes: first.length - 1 }, { totalInputBytes: first.length - 1 }, { replyBytes: 1 }, { totalOutputBytes: 1 }]) {
    const f = fixture(), s = f.run([first], { limits: cap });await assert.rejects(s.promise, /quota/);
    assert.equal(s.replies.length, 0);assert.equal(f.state.failed, true);
  }
});
test("second-producer template copies consume cumulative service snapshot quota", async () => {
  for (const field of ["nodes", "arraySlots", "stringCharacters"]) {
    const f = fixture(), config = { preparationPolicy: f.preparationPolicy, preparationLimits: f.preparationLimits, snapshotLimits, limits, producerCommitments };
    const need = snapshotCanonicalMetadata(config, snapshotLimits).usage[field] + snapshotCanonicalMetadata(template("point0"), snapshotLimits).usage[field];
    const s = f.run([wire(begin()), wire(request(1, "validate", template("point0")))], { snapshotLimits: { [field]: need - 1 } });
    await assert.rejects(s.promise, /ceiling|quota/);assert.equal(s.replies.length, 1);assert.equal(f.calls.includes("prepare"), false);assert.equal(f.state.failed, true);
  }
});
test("finish ACK is withheld until clean input EOF and output drain", async () => {
  const f = fixture(), input = new PassThrough(), s = f.run([], { input });
  for (const frame of allRequests().map(wire)) input.write(frame);
  await s.waitForReplies(2122);assert.equal(s.replies.length, 2122);assert.equal(f.state.events, 707);input.end();await s.promise;
  assert.equal(s.replies.length, 2123);
  const g = fixture();let release, notify;const enteredFinal = new Promise(resolve => { notify = resolve; });
  const output = new Writable({ write(_chunk, _encoding, callback) { callback(); }, final(callback) { release = callback;notify(); } });
  const held = g.run(allRequests().map(wire), { output });let settled = false;held.promise.then(() => { settled = true; }, () => { settled = true; });
  await enteredFinal;assert.equal(settled, false);release();await held.promise;
});
test("trailing frame partial tail or missing finish forbids terminal acceptance", async () => {
  for (const mode of ["extra", "partial", "missing"]) {
    const f = fixture(), frames = allRequests().map(wire);
    if (mode === "extra") frames.push(wire(request(2123, "finish")));else if (mode === "partial") frames.push(Buffer.from("{"));else frames.pop();
    const s = f.run(frames);await assert.rejects(s.promise);assert.equal(s.replies.length, 2122);assert.equal(f.state.failed, true);
  }
});
test("driver failure and late authenticated health failure abort without success promotion", async () => {
  const f = fixture();f.state.failOperation = "prepare";
  const frames = prepass();frames.push(request(707, "prepare", template("point0")));
  const s = f.run(frames.map(wire));await assert.rejects(s.promise, /manufactured driver prepare/);assert.equal(s.replies.length, 707);assert.equal(f.state.failed, true);
  const g = fixture(), late = g.run(allRequests().map(wire), { onReply(n) { if (n === 2123) g.configHealth.failed = true; } });
  await assert.rejects(late.promise, /healthy/);assert.equal(g.state.failed, true);
});
test("stream failures and whole-session deadline abort owned preparation", async () => {
  const f = fixture(), input = new PassThrough(), s = f.run([], { input, limits: { timeoutMs: 20 } });
  await assert.rejects(s.promise, /deadline/);assert.equal(input.destroyed, true);assert.equal(f.state.failed, true);
  const g = fixture(), output = new Writable({ write(_chunk, _encoding, callback) { callback(Error("manufactured output failure")); } });
  const broken = g.run([wire(begin())], { output });await assert.rejects(broken.promise, /manufactured output failure/);assert.equal(g.state.failed, true);
});
test("input errors and a stalled reply write reject rather than outliving the deadline", async () => {
  const f = fixture(), input = new PassThrough(), broken = f.run([], { input });input.destroy(Error("manufactured input failure"));
  await assert.rejects(broken.promise, /manufactured input failure/);assert.equal(f.state.failed, true);
  const g = fixture(), output = new Writable({ write() { /* Deliberately withhold the write callback. */ } });
  const stalled = g.run([wire(begin())], { output, limits: { timeoutMs: 20 } });
  await assert.rejects(stalled.promise, /deadline/);assert.equal(g.state.failed, true);assert.equal(output.destroyed, true);
});
test("production service rejects an unbranded receipt without calling source admission", async () => {
  const { serveSourcePreparation } = realRequire("./a68-source-preparation-service");let calls = 0;
  const input = new Readable({ read() {}, destroy(_error, callback) { queueMicrotask(() => callback(Error("manufactured input cleanup error"))); } });
  const output = new Writable({ write(_chunk, _encoding, callback) { callback(); }, destroy(_error, callback) { queueMicrotask(() => callback(Error("manufactured output cleanup error"))); } });
  const closed = Promise.all([input, output].map(stream => new Promise(resolve => stream.once("close", resolve))));
  await assert.rejects(serveSourcePreparation({ input, output, preflightResult: {}, preparationPolicy: {}, preparationLimits: {},
    snapshotLimits, limits, admitStage() { calls++; } }), /private clean configured process/);
  assert.equal(calls, 0);assert.equal(input.destroyed, true);assert.equal(output.destroyed, true);await closed;
  assert.equal(input.listenerCount("error"), 0);assert.equal(output.listenerCount("error"), 0);
});
test("malformed setup limits destroy both supplied streams before driver construction", async () => {
  for (const overrides of [{ limits: { frameBytes: 0 } }, { snapshotLimits: { nodes: 0 } }]) {
    const f = fixture(), s = f.run([], overrides);await assert.rejects(s.promise, /quota/);
    assert.equal(f.calls.length, 0);assert.equal(s.input.destroyed, true);assert.equal(s.output.destroyed, true);assert.equal(s.replies.length, 0);
  }
});
test("driver constructor failure destroys streams without replacing its original error", async () => {
  const f = fixture(), original = Error("original manufactured constructor failure");f.state.failOperation = "construct";f.state.failError = original;
  const output = new Writable({ write(_chunk, _encoding, callback) { callback(); }, destroy(_error, callback) { callback(Error("secondary cleanup failure")); } });
  const closed = new Promise(resolve => output.once("close", resolve));
  const s = f.run([], { output });await assert.rejects(s.promise, error => error === original);
  assert.equal(s.input.destroyed, true);assert.equal(output.destroyed, true);assert.deepEqual(f.calls, ["construct"]);await closed;
  assert.equal(output.listenerCount("error"), 0);
});
test("earlier setup stream errors and active driver errors retain their respective original cause", async () => {
  const f = fixture(), setupError = Error("first setup stream error"), output = new PassThrough();
  f.state.onConstruct = () => output.emit("error", setupError);f.state.failOperation = "construct";
  const setup = f.run([], { output });await assert.rejects(setup.promise, error => error === setupError);
  assert.equal(setup.input.destroyed, true);assert.equal(output.destroyed, true);
  const g = fixture(), original = Error("first active driver error");g.state.failOperation = "prepare";g.state.failError = original;
  const peer = new Writable({ write(_chunk, _encoding, callback) { callback(); }, destroy(_error, callback) { callback(Error("later active cleanup error")); } });
  const frames = prepass();frames.push(request(707, "prepare", template("point0")));
  const active = g.run(frames.map(wire), { output: peer });await assert.rejects(active.promise, error => error === original);
  assert.equal(g.state.failed, true);assert.equal(peer.destroyed, true);
});

"use strict";
// Real source-STRUCTURE validation of manufactured metadata profiles only.
// No file reader, source geometry, coefficient or scientific replay runs.
const test = require("node:test"), assert = require("node:assert/strict"), crypto = require("node:crypto");
const { createCheckpointCatalog, checkpointCatalogIdentity } = require("../a68-checkpoint-catalog");
const { snapshotCanonicalMetadata } = require("../a68-canonical-metadata");
const { sourceContextIds } = require("../a68-source-context-menu");
const { clone, limits, profile, template, checkpointPolicy } = require("./a68-source-template-fixture");
const digest = value => crypto.createHash("sha256").update(JSON.stringify(value) + "\n").digest("hex");
const caps = () => ({ validator: { ...limits }, snapshot: { ...limits } });
const make = (p = profile(), policy = checkpointPolicy(), budget = caps(), admit = () => {}, topology = () => {}) =>
  createCheckpointCatalog(p, policy, budget, admit, topology);
const row = (kind, contextId, declaration, profileIndex) => ({ kind, contextId, declarationSha256: digest(declaration), profile: profileIndex });

test("real705 validation produces exact707 same-candidate commitments and immutable private configuration", () => {
  const p = profile(), originalProfile = clone(p), policy = checkpointPolicy(), originalPolicy = clone(policy), budget = caps();
  let candidate = null, constructions = 0, topology = 0;
  const catalog = make(p, policy, budget, () => {
    constructions++;
    if (candidate) { candidate.graphPath = "caller-mutation/graph.json"; candidate.marks[0].name = "caller-mutated"; }
  }, () => { topology++; p.retention.background = true; p.capture.limits.normal.graphs = 1; });
  policy.outputRoot = "/tmp/changed-after-construction"; policy.profiles[0].replay.manufactured = false;
  policy.routes[0].profile = 1; budget.snapshot.nodes = 1; budget.validator.nodes = 1;
  catalog.validateCaptureDeclaration(p.capture);
  const expected = [], expectedTemplates = []; let final = null;
  for (const id of sourceContextIds()) {
    candidate = template(id, originalProfile);
    if (id === "point0") {
      const originalHash = digest(candidate);
      const before = digest({ contextId: id, graphPath: candidate.graphPath, metadataPath: candidate.pointCheckpoint.relativePath, marks: candidate.marks });
      // The structure validator accepts object field reordering, while exact
      // producer bytes differ. The compact checkpoint commitment cannot see
      // this full-template difference; the new commitment must preserve it.
      candidate.resources = Object.fromEntries(Object.entries(candidate.resources).reverse());
      assert.notEqual(digest(candidate), originalHash);
      assert.equal(digest({ contextId: id, graphPath: candidate.graphPath, metadataPath: candidate.pointCheckpoint.relativePath, marks: candidate.marks }), before);
    }
    expectedTemplates.push({ contextId: id, templateSha256: digest(candidate) });
    const point = /^point[01]$/.test(id), declaration = { contextId: id, graphPath: candidate.graphPath,
      metadataPath: point ? candidate.pointCheckpoint.relativePath : candidate.metadataPath, marks: candidate.marks };
    expected.push(row(point ? "background" : id.startsWith("diagnostic/") ? "diagnostic" : "germ", id, declaration, 0));
    if (point) final = row("final", id, { contextId: id, graphPath: candidate.graphPath,
      backgroundPath: candidate.pointCheckpoint.relativePath, metadataPath: candidate.metadataPath }, 1);
    if (/^point[01]\/m9_j34$/.test(id)) { expected.push(final); final = null; }
    const report = catalog.validateTemplate(candidate); assert.equal(report.contextId, id); assert.equal(report.scope.scientificExecutionAuthorized, false);
  }
  const report = catalog.finish();
  assert.deepEqual(report, { contexts: 705, marks: 291199, leaves: 20316, callbacks: 940365,
    scope: { structureOnly: true, numericalReplayComplete: false, sourceAuthenticityEstablished: false,
      resourceSufficiencyProved: false, totalProcessMemoryProved: false, scientificExecutionAuthorized: false } });
  assert.equal(constructions, 705); assert.equal(topology, 1);
  const token = catalog.configurationToken(), identity = checkpointCatalogIdentity(token), { configuration, producerCommitments, health } = identity;
  assert.equal(catalog.configurationToken(), token); assert.equal(checkpointCatalogIdentity(token), identity);
  assert.deepEqual(configuration, { outputRoot: originalPolicy.outputRoot, events: expected, profiles: originalPolicy.profiles });
  assert.deepEqual(producerCommitments, { profileSha256: digest(originalProfile), captureSha256: digest(originalProfile.capture), templates: expectedTemplates });
  assert.notEqual(producerCommitments.profileSha256, digest(p)); assert.notEqual(producerCommitments.captureSha256, digest(p.capture));
  assert.equal(producerCommitments.templates.length, 705);
  assert.ok(Object.isFrozen(producerCommitments)); assert.ok(Object.isFrozen(producerCommitments.templates)); assert.ok(Object.isFrozen(producerCommitments.templates[0]));
  assert.deepEqual(Object.keys(identity), ["configuration", "producerCommitments", "health"]);
  assert.throws(() => { producerCommitments.templates[0].templateSha256 = "0".repeat(64); }, TypeError);
  assert.equal(configuration.events[351].kind, "final"); assert.equal(configuration.events[703].kind, "final");
  assert.equal(configuration.events[704].contextId, "diagnostic/grade10"); assert.equal(configuration.events[706].contextId, "diagnostic/secondJets");
  assert.ok(Object.isFrozen(identity)); assert.ok(Object.isFrozen(token)); assert.ok(Object.isFrozen(configuration.events));
  assert.ok(Object.isFrozen(configuration.events[0])); assert.ok(Object.isFrozen(configuration.profiles[0].read.snapshot));
  assert.deepEqual(Object.keys(configuration.events[0]), ["kind", "contextId", "declarationSha256", "profile"]);
  assert.deepEqual(health(), { failed: false, finished: true });
  assert.throws(() => { configuration.events[0].profile = 1; }, TypeError);
  assert.throws(() => checkpointCatalogIdentity({ ...token }), /private completed catalog/);
  assert.throws(() => catalog.finish(), /invalid terminal/);
  assert.deepEqual(health(), { failed: true, finished: true });
  assert.throws(() => checkpointCatalogIdentity(token), /healthy completed/);
  assert.throws(() => catalog.configurationToken(), /failed/);
});

test("catalog identity accessor rejects unknown data without invoking getters", () => {
  let calls = 0; const fake = new Proxy({}, { get() { calls++; throw Error("getter"); } });
  for (const value of [null, undefined, 1, {}, fake]) assert.throws(() => checkpointCatalogIdentity(value), /private completed catalog/);
  assert.equal(calls, 0);
});

test("policy requires fixed705 routes canonical root and appropriate context/final profile roles", () => {
  for (const change of [
    p => { p.outputRoot = "/"; }, p => { p.outputRoot = "relative"; }, p => { p.outputRoot += "/../alias"; },
    p => p.routes.pop(), p => p.routes.reverse(), p => { p.routes[1].contextId = "point0"; },
    p => { p.routes[0].profile = -1; }, p => { p.routes[0].profile = 1; },
    p => { p.routes[0].completionProfile = null; }, p => { p.routes[0].completionProfile = 0; },
    p => { p.routes[1].completionProfile = 1; }, p => { p.profiles[1].replay = {}; },
    p => { p.profiles[0].read = null; }, p => { p.profiles[0].replay = []; },
    p => { p.profiles = []; }, p => { p.routes[0].extra = false; }
  ]) {
    const policy = checkpointPolicy(); change(policy); assert.throws(() => make(profile(), policy));
  }
});

test("policy and candidate getters reject without execution and failures cannot mint configuration", () => {
  let reads = 0; const policy = checkpointPolicy();
  Object.defineProperty(policy, "outputRoot", { enumerable: true, get() { reads++; return "/tmp/untrusted"; } });
  assert.throws(() => make(profile(), policy), /own data/); assert.equal(reads, 0);
  const badProfile = profile();
  Object.defineProperty(badProfile, "capture", { enumerable: true, get() { reads++; return {}; } });
  assert.throws(() => make(badProfile), /own data/); assert.equal(reads, 0);
  const p = profile(), catalog = make(p); catalog.validateCaptureDeclaration(p.capture);
  const candidate = template("point0", p);
  Object.defineProperty(candidate, "marks", { enumerable: true, get() { reads++; return []; } });
  assert.throws(() => catalog.validateTemplate(candidate), /own data/); assert.equal(reads, 0);
  assert.throws(() => catalog.configurationToken(), /failed/);
});

test("early token completion unbound skipped and repeated calls are permanently rejected", () => {
  for (const action of [c => c.configurationToken(), c => c.finish(), c => c.validateTemplate({ id: "point0" })]) {
    const p = profile(), c = make(p); assert.throws(() => action(c)); assert.throws(() => c.validateCaptureDeclaration(p.capture), /failed/);
  }
  const p = profile();
  for (const action of [c => c.validateCaptureDeclaration(p.capture), c => c.validateTemplate(template("point1", p)),
    c => { c.validateTemplate(template("point0", p)); c.validateTemplate(template("point0", p)); }]) {
    const c = make(p); c.validateCaptureDeclaration(p.capture); assert.throws(() => action(c)); assert.throws(() => c.finish(), /failed/);
  }
});

test("real validator rejection cannot be replaced by a caller hash or corrected retry", () => {
  const p = profile(), c = make(p); c.validateCaptureDeclaration(p.capture);
  const candidate = template("point0", p); candidate.marks[0].degree = 14;
  assert.throws(() => c.validateTemplate(candidate), /exact/);
  assert.throws(() => c.validateTemplate(template("point0", p)), /failed/);
  assert.throws(() => c.configurationToken(), /failed/);
});

test("swallowed callback and snapshot reentry poison wrapper despite inner-validator progress", () => {
  const p = profile(); let c;
  c = make(p, checkpointPolicy(), caps(), () => { assert.throws(() => c.configurationToken(), /reentrant/); });
  c.validateCaptureDeclaration(p.capture);
  assert.throws(() => c.validateTemplate(template("point0", p)), /healthy|swallowed/); assert.throws(() => c.finish(), /failed/);
  const d = make(p); d.validateCaptureDeclaration(p.capture);
  const candidate = new Proxy(template("point0", p), { ownKeys(target) {
    assert.throws(() => d.finish(), /reentrant/); return Reflect.ownKeys(target);
  } });
  assert.throws(() => d.validateTemplate(candidate), /healthy|swallowed/); assert.throws(() => d.configurationToken(), /failed/);
});

test("snapshot quotas are cumulative across policy full profile capture hashes and candidates before admission", () => {
  const p = profile(), policy = checkpointPolicy(), candidate = template("point0", p);
  const copies = [policy, p, digest(p), p.capture, digest(p.capture), candidate].map(value => snapshotCanonicalMetadata(value, limits).usage);
  for (const field of ["nodes", "arraySlots", "stringCharacters"]) {
    const cap = caps(); cap.snapshot[field] = copies.reduce((n, used) => n + used[field], 0) - 1;
    let admitted = 0; const c = make(p, policy, cap, () => { admitted++; }); c.validateCaptureDeclaration(p.capture);
    assert.throws(() => c.validateTemplate(candidate), /ceiling|quota/); assert.equal(admitted, 0);
    assert.throws(() => c.configurationToken(), /failed/);
  }
});

test("new full-template commitment rows consume cumulative snapshot quota before retention", () => {
  const p = profile(), policy = checkpointPolicy(), candidate = template("point0", p);
  const row = [{ contextId: candidate.id, templateSha256: digest(candidate) }];
  const copies = [policy, p, digest(p), p.capture, digest(p.capture), candidate, row].map(value => snapshotCanonicalMetadata(value, limits).usage);
  for (const field of ["nodes", "arraySlots", "stringCharacters"]) {
    const cap = caps(); cap.snapshot[field] = copies.reduce((n, used) => n + used[field], 0) - 1;
    let admitted = 0; const c = make(p, policy, cap, () => { admitted++; }); c.validateCaptureDeclaration(p.capture);
    assert.throws(() => c.validateTemplate(candidate), /ceiling|quota/); assert.equal(admitted, 1);
    assert.throws(() => c.configurationToken(), /failed/);
  }
});

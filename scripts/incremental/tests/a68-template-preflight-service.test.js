"use strict";
const test = require("node:test"), assert = require("node:assert/strict"), crypto = require("node:crypto");
const { Readable, Writable, PassThrough } = require("node:stream");
const { REQUEST, REPLY, profileCommitment, serveTemplatePreflight, templatePreflightConfiguration } = require("../a68-template-preflight-service");
const fixture = require("./a68-source-template-fixture");
const limits = { frameBytes: 2000000, totalInputBytes: 536870912, replyBytes: 4096, totalOutputBytes: 4194304, timeoutMs: 60000 };
const wire = value => Buffer.from(JSON.stringify(value) + "\n", "ascii");
function request(sequence, operation, profileSha256 = null, template = null) { return { schema: REQUEST, sequence, operation, profileSha256, template }; }
function fixtureSession(frames, overrides = {}) {
  const profile = overrides.profile || fixture.profile(), replies = [];
  const input = overrides.input || Readable.from(frames);
  const output = new Writable({ write(chunk, _encoding, callback) { replies.push(Buffer.from(chunk)); callback(); } });
  return { replies, promise: serveTemplatePreflight({ input, output, profile, snapshotLimits: fixture.limits,
    limits: { ...limits, ...overrides.limits }, admitConstruction: overrides.admitConstruction || (() => {}),
    admitTopology: overrides.admitTopology || (() => {}), checkpointPolicy: fixture.checkpointPolicy(),
    checkpointLimits: { validator: fixture.limits, snapshot: fixture.limits } }) };
}
function begin(p) { return request(0, "begin", profileCommitment(p, fixture.limits), p.capture); }
test("matching undersized topology profiles receive no Begin acknowledgement", async () => {
  for (const change of [p => p.budgets[1].resources.trace.nodes = 100000,
    p => p.capture.limits.normal.nodeObjects = 100000000,
    p => p.capture.limits.failure.slots = 2000000]) {
    const p = fixture.profile(); change(p);
    const s = fixtureSession([wire(begin(p))], { profile: p });
    await assert.rejects(s.promise, /prospective topology/); assert.equal(s.replies.length, 0);
  }
});
test("planner refusal or asynchronous admission precedes any acknowledgement", async () => {
  for (const admitTopology of [() => { throw new Error("planner budget"); }, async () => {}]) {
    const p = fixture.profile(), s = fixtureSession([wire(begin(p))], { profile: p, admitTopology });
    await assert.rejects(s.promise, /planner budget|synchronous void topology/); assert.equal(s.replies.length, 0);
  }
});
test("v2 handshake refuses old schema or absent declarations despite a matching profile hash", async () => {
  for (const mutate of [r => r.schema = "phase627-template-preflight-request-v1", r => r.template = null, r => delete r.template]) {
    const p = fixture.profile(), r = begin(p); mutate(r); const s = fixtureSession([wire(r)], { profile: p });
    await assert.rejects(s.promise); assert.equal(s.replies.length, 0);
  }
});
test("matching opaque profile hashes cannot hide actual cap or inspection policy mismatches", async () => {
  for (const mutate of [d => d.limits.normal.graphs++, d => d.limits.inspection.slots++, d => d.limits.failure.arrays++, d => d.inspectionPolicy[704].copies++]) {
    const p = fixture.profile(), r = begin(p); r.template = fixture.clone(p.capture); mutate(r.template);
    const s = fixtureSession([wire(r)], { profile: p }); await assert.rejects(s.promise, /capture declaration/); assert.equal(s.replies.length, 0);
  }
});
test("handshake binds a separately configured profile, with a byte-bound acknowledgement but no completion", async () => {
  const p = fixture.profile(), frame = wire(begin(p)), s = fixtureSession([frame], { profile: p });
  await assert.rejects(s.promise, /clean EOF/); assert.equal(s.replies.length, 1);
  assert.deepEqual(JSON.parse(s.replies[0]), { schema: REPLY, sequence: 0, operation: "begin", requestSha256: crypto.createHash("sha256").update(frame).digest("hex"), status: "accepted", report: null });
});
test("wrong profile commitments and peer-supplied configuration fail before any acknowledgement", async () => {
  for (const change of [r => r.profileSha256 = "0".repeat(64), r => r.profile = {}, r => r.template = {}, r => r.sequence = 1]) {
    const p = fixture.profile(), value = begin(p); change(value); const s = fixtureSession([wire(value)], { profile: p });
    await assert.rejects(s.promise); assert.equal(s.replies.length, 0);
  }
});
test("sequence omissions duplicate frames early finish and wrong context cannot advance admission", async () => {
  const p = fixture.profile();
  for (const second of [request(2, "validate", null, fixture.template("point0", p)), begin(p), request(1, "finish"),
    request(1, "validate", null, fixture.template("point1", p)), request(1, "validate", "0".repeat(64), fixture.template("point0", p))]) {
    let calls = 0; const s = fixtureSession([wire(begin(p)), wire(second)], { profile: p, admitConstruction() { calls++; } });
    await assert.rejects(s.promise); assert.equal(s.replies.length, 1); assert.equal(calls, 0);
  }
});
test("fragmented or coalesced canonical frames preserve exact request identities", async () => {
  const p = fixture.profile(), first = wire(begin(p)), second = wire(request(1, "validate", null, fixture.template("point0", p)));
  for (const frames of [[Buffer.concat([first, second])], [first.subarray(0, 3), first.subarray(3), second.subarray(0, 10), second.subarray(10)]]) {
    const s = fixtureSession(frames, { profile: p }); await assert.rejects(s.promise, /clean EOF/);
    assert.equal(s.replies.length, 2); const reply = JSON.parse(s.replies[1]);
    assert.equal(reply.requestSha256, crypto.createHash("sha256").update(second).digest("hex")); assert.equal(reply.report.marks, 74);
    assert.equal(reply.report.scope.scientificExecutionAuthorized, false);
  }
});
test("wire duplicates alternate encodings whitespace missing LF and partial tails are rejected", async () => {
  const p = fixture.profile(), original = wire(begin(p));
  for (const bad of [Buffer.from(original.toString().replace('"sequence":0', '"sequence":0,"sequence":0')),
    Buffer.from(original.toString().replace('"sequence":0', '"sequence":0.0')), Buffer.concat([Buffer.from(" "), original]), original.subarray(0, original.length - 1),
    Buffer.concat([original, Buffer.from("{")])]) {
    const s = fixtureSession([bad], { profile: p }); await assert.rejects(s.promise);
  }
});
test("input per-frame and cumulative quotas reject before excess frame assembly", async () => {
  const p = fixture.profile(), first = wire(begin(p));
  for (const quota of [{ frameBytes: first.length - 1 }, { totalInputBytes: first.length - 1 }]) {
    const s = fixtureSession([first], { profile: p, limits: quota }); await assert.rejects(s.promise, /quota/); assert.equal(s.replies.length, 0);
  }
});
test("reply per-frame and cumulative quotas are checked before output writes", async () => {
  const p = fixture.profile();
  for (const quota of [{ replyBytes: 1 }, { totalOutputBytes: 1 }]) {
    const s = fixtureSession([wire(begin(p))], { profile: p, limits: quota }); await assert.rejects(s.promise, /prewrite reply/); assert.equal(s.replies.length, 0);
  }
});
test("semantic validation or asynchronous admission failure cannot emit a context acceptance", async () => {
  const p = fixture.profile(), bad = fixture.template("point0", p); bad.marks.pop();
  for (const [candidate, admitConstruction] of [[bad, () => {}], [fixture.template("point0", p), async () => {}]]) {
    const s = fixtureSession([wire(begin(p)), wire(request(1, "validate", null, candidate))], { profile: p, admitConstruction });
    await assert.rejects(s.promise); assert.equal(s.replies.length, 1);
  }
});
test("a stalled input reaches a real whole-session deadline without retry", async () => {
  const input = new PassThrough(), s = fixtureSession([], { input, limits: { timeoutMs: 30 } });
  await assert.rejects(s.promise, /deadline/); assert.equal(input.destroyed, true); assert.equal(s.replies.length, 0);
});
test("ACK reports and plain terminal-looking objects do not carry private catalog provenance", async () => {
  const p = fixture.profile(), s = fixtureSession([wire(begin(p))], { profile: p });
  await assert.rejects(s.promise, /clean EOF/);
  for (const fake of [JSON.parse(s.replies[0]), { requests: 707, report: { contexts: 705 } }, {}, null])
    assert.throws(() => templatePreflightConfiguration(fake), /private configured preflight/);
});

"use strict";
const test = require("node:test"), assert = require("node:assert/strict"), path = require("node:path");
const { runTemplatePreflightProcess, runConfiguredTemplatePreflightProcess, validatedCheckpointConfiguration } = require("../a68-template-process-host");
const fixture = require("./a68-source-template-fixture");
const { createLaunchFileAdmission } = require("../a68-launch-file-admission");
const { profileCommitment } = require("../a68-template-preflight-service");
const root = path.resolve(__dirname, "../../.."), child = path.join(__dirname, "a68-template-process-child.js");
function options(mode = "stall") {
  return { launch: { executable: process.execPath, arguments: [child, mode], workingDirectory: root, environment: {} },
    profile: fixture.profile(), snapshotLimits: fixture.limits,
    transportLimits: { frameBytes: 2000000, totalInputBytes: 536870912, replyBytes: 4096, totalOutputBytes: 4194304, timeoutMs: 1500 },
    processLimits: { timeoutMs: 2000, killGraceMs: 200, stdoutBytes: 128, stderrBytes: 128 }, admitLaunch() {}, admitConstruction() {}, admitTopology() {} };
}
async function rejection(input, pattern) {
  let error;
  try { await runTemplatePreflightProcess(input); } catch (caught) { error = caught; }
  assert.ok(error, "never accept failed child"); if (pattern) assert.match(error.message, pattern);
  if (error.evidence) { assert.equal(error.evidence.scientificExecutionAuthorized, false); assert.equal(error.evidence.protocolCompleted, false); }
  return error;
}
test("host requires explicit launch environment callbacks positive quotas and complete profile before admission", async () => {
  for (const change of [o => o.launch.executable = "node", o => o.launch.workingDirectory = ".", o => o.launch.environment = null,
    o => o.launch.arguments = "shell command", o => o.processLimits.timeoutMs = 0, o => o.transportLimits.timeoutMs = 3000,
    o => o.profile.budgets.pop(), o => o.admitConstruction = null, o => o.admitTopology = null]) {
    const o = options(); let admitted = false; o.admitLaunch = () => { admitted = true; }; change(o);
    const error = await rejection(o); assert.equal(admitted, false); assert.equal(error.evidence, undefined);
  }
});
test("launch admission must synchronously return void and no async or value acceptance launches a child", async () => {
  for (const value of [true, false, {}, Promise.resolve(), { then() {} }]) {
    const o = options(); o.admitLaunch = () => value;
    assert.equal((await rejection(o, /synchronous void/)).evidence, undefined);
  }
  const o = options(); o.admitLaunch = () => { throw Error("unreviewed executable closure"); };
  assert.equal((await rejection(o, /unreviewed/)).evidence, undefined);
});
test("own-data launch snapshots reject getters without invoking them", async () => {
  const o = options(); let invoked = false;
  Object.defineProperty(o.launch, "executable", { enumerable: true, get() { invoked = true; return process.execPath; } });
  await rejection(o, /own data/); assert.equal(invoked, false);
});
test("zero or nonzero exit without the complete protocol cannot pass and owned process is reaped", async () => {
  for (const mode of ["exit0", "exit7", "closed", "malformed"]) {
    const error = await rejection(options(mode)); assert.ok(error.evidence.childPid > 0);
    assert.equal(error.evidence.closed, true); assert.equal(error.evidence.cleanupConfirmed, true);
  }
});
test("stdout and stderr limits reject before retaining an oversized chunk", async () => {
  for (const mode of ["stdout", "stderr"]) {
    const error = await rejection(options(mode), new RegExp(mode + " quota"));
    assert.equal(error.evidence[mode + "Bytes"], 0); assert.equal(error.evidence[mode + "Base64"], "");
    assert.equal(error.evidence.cleanupConfirmed, true);
  }
});
test("bounded diagnostic evidence survives a failed session without promoting success", async () => {
  const o = options("diagnostic"); o.transportLimits.timeoutMs = 250; o.processLimits.timeoutMs = 1000;
  const error = await rejection(o, /deadline/);
  assert.equal(Buffer.from(error.evidence.stderrBase64, "base64").toString(), "manufactured rejection\n");
  assert.equal(error.evidence.cleanupConfirmed, true);
});
test("deadline cleanup terminates an owned child that refuses SIGTERM", async () => {
  const o = options("ignore-term"); o.transportLimits.timeoutMs = 250; o.processLimits.timeoutMs = 400;
  const error = await rejection(o, /deadline/);
  assert.equal(error.evidence.signal, "SIGKILL"); assert.equal(error.evidence.cleanupConfirmed, true);
});
test("spawn failures carry bounded evidence and cannot leave a successful session", async () => {
  const o = options(); o.launch.executable = "/tmp/a68-deliberately-absent-executable";
  const error = await rejection(o, /spawn failure ENOENT/); assert.equal(error.evidence.spawnError, "ENOENT");
  assert.equal(error.evidence.childPid, null); assert.equal(error.evidence.closed, true); assert.equal(error.evidence.cleanupConfirmed, true);
});
test("admission receives frozen launch and independent profile commitment before caller mutation", async () => {
  const o = options("exit7"); o.admitLaunch = captured => {
    assert.ok(Object.isFrozen(captured.launch.arguments)); assert.match(captured.profileSha256, /^[0-9a-f]{64}$/);
    assert.deepEqual(captured.launch.environment, {}); o.launch.arguments[1] = "stall"; o.processLimits.timeoutMs = 1;
  };
  const error = await rejection(o); assert.equal(error.evidence.exitCode, 7); assert.equal(error.evidence.cleanupConfirmed, true);
});
test("concrete file admission rejects changed executable bytes before the host can spawn", async () => {
  const fs = require("node:fs"), o = options("exit0"); o.launch.executable = fs.realpathSync(o.launch.executable);
  const bytes = fs.statSync(o.launch.executable).size;
  const admission = createLaunchFileAdmission({ expected: { launch: o.launch, profileSha256: profileCommitment(o.profile, o.snapshotLimits),
    processLimits: o.processLimits, transportLimits: o.transportLimits },
    pins: [{ path: o.launch.executable, bytes, sha256: "0".repeat(64) }], argumentFiles: [],
    limits: { files: 1, fileBytes: bytes, totalReadBytes: bytes + 1, chunkBytes: 65536, snapshot: fixture.limits } });
  o.admitLaunch = admission.admitLaunch;
  const error = await rejection(o, /exact independently expected file bytes/);
  assert.equal(error.evidence, undefined); assert.equal(admission.snapshot().failed, true);
});
test("configured host rejects missing malformed or accessor policy before launch admission", async () => {
  for (const mode of ["missing", "route", "getter", "quota"]) {
    const o = options(), policy = fixture.checkpointPolicy(), cap = { validator: fixture.limits, snapshot: fixture.limits };let admitted = false, getters = 0;
    o.admitLaunch = () => { admitted = true; };
    if (mode === "route") policy.routes[0].contextId = "point1";
    if (mode === "getter") Object.defineProperty(policy, "outputRoot", { enumerable: true, get() { getters++;return "/tmp/should-not-read"; } });
    if (mode === "quota") cap.snapshot = { ...fixture.limits, nodes: 1 };
    await assert.rejects(runConfiguredTemplatePreflightProcess(o, mode === "missing" ? null : policy, cap));
    assert.equal(admitted, false);assert.equal(getters, 0);
  }
});
test("plain configured-looking process reports cannot mint checkpoint provenance", () => {
  for (const result of [null, {}, { process: { exitCode: 0 }, preflight: { requests: 707 }, configuration: fixture.checkpointPolicy() }])
    assert.throws(() => validatedCheckpointConfiguration(result), /private clean configured process/);
});

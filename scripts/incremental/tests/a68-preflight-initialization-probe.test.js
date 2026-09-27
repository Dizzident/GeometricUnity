"use strict";

// Metadata-only engineering tests. No phase entrypoint, source geometry,
// coefficients, production host/admission factory or scientific sink runs.
const test = require("node:test"), assert = require("node:assert/strict");
const fs = require("node:fs"), path = require("node:path"), os = require("node:os"), crypto = require("node:crypto");
const { runPreflightInitializationProbe: probe } = require("../a68-preflight-initialization-probe");
const { expectedRuntime, sourcePins } = require("../a68-preflight-bootstrap-policy");
const { PREFLIGHT_SOURCE_PINS: pins } = require("../a68-preflight-module-manifest");
const vm = require("node:vm"), Module = require("node:module");
const root = path.resolve(__dirname, "../../.."), source = path.join(root, "scripts/incremental");
const digest = data => crypto.createHash("sha256").update(data).digest("hex");
function options() {
  return { repositoryRoot: root, profile: { label: "manufactured-metadata-only", counts: [2, 700, 3] },
    snapshotLimits: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 },
    workerLimits: { maxOldGenerationSizeMb: 64, maxYoungGenerationSizeMb: 16, codeRangeSizeMb: 32,
      stackSizeMb: 4, timeoutMs: 10000, replyBytes: 8192, stdioBytes: 1024 } };
}
function stopped(error, pattern) {
  assert.match(error.message, pattern); assert.equal(error.evidence.terminationObserved, true);
  assert.equal(error.evidence.messages, 0); assert.equal(error.evidence.stdoutBytes, 0);
  assert.equal(error.evidence.stderrBytes, 0); return true;
}
function copiedSources(t) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), "a68-init-probe-"));
  t.after(() => fs.rmSync(dir, { recursive: true, force: true }));
  const target = path.join(dir, "scripts/incremental"); fs.mkdirSync(target, { recursive: true });
  for (const pin of pins) fs.copyFileSync(path.join(source, pin.id), path.join(target, pin.id));
  return { dir, target };
}
function manufacturedWorkerBoundary({ ambientImport, cached = false, wrongRuntime = false, corruptBootstrap = false }) {
  // Execute the exact supervisor source with a manufactured worker API. This
  // exercises failure branches only; it is NOT evidence of actual worker limits.
  // No aggregate is compiled: each injected failure occurs before loader.load.
  const filename = path.join(source, "a68-preflight-initialization-probe.js"), localRequire = Module.createRequire(filename);
  const config = options(), cap = config.workerLimits; let loaded = false;
  const actualLoader = localRequire("./a68-trusted-module-loader");
  function injectedRequire(request) {
    if (request === "node:worker_threads") return { isMainThread: false,
      workerData: { operation: "a68-metadata-initialization-v1", config },
      resourceLimits: { maxOldGenerationSizeMb: cap.maxOldGenerationSizeMb, maxYoungGenerationSizeMb: cap.maxYoungGenerationSizeMb,
        codeRangeSizeMb: cap.codeRangeSizeMb, stackSizeMb: cap.stackSizeMb },
      parentPort: { postMessage() { assert.fail("negative boundary must not post success"); } } };
    if (request === "node:fs" && corruptBootstrap) return { ...fs, readFileSync(file, ...args) {
      const bytes = fs.readFileSync(file, ...args);
      if (file === path.join(source, "a68-canonical-metadata.js")) bytes[0] ^= 1;
      return bytes;
    } };
    if (request === "./a68-trusted-module-loader") return { ...actualLoader, createTrustedModuleLoader(opts) {
      const loader = actualLoader.createTrustedModuleLoader(opts);
      return { ...loader, capture() { if (ambientImport) localRequire(ambientImport); loader.capture(); },
        load() { loaded = true; assert.fail("negative boundary must precede aggregate evaluation"); } };
    } };
    return localRequire(request);
  }
  const runtime = Object.create(process);
  if (wrongRuntime) Object.defineProperty(runtime, "version", { value: "v0.0.0-unreviewed" });
  const compiled = vm.compileFunction(fs.readFileSync(filename, "utf8"),
    ["require", "module", "exports", "__dirname", "__filename", "process"], { filename });
  const original = Module._load, module = { exports: {} };
  const cachedPath = cached ? localRequire.resolve(ambientImport) : null;
  if (ambientImport && !cached) assert.equal(require.cache[localRequire.resolve(ambientImport)], undefined, "uncached negative target");
  const previousCache = cachedPath && require.cache[cachedPath];
  if (cachedPath) require.cache[cachedPath] = { exports: { manufacturedCachedModule: true } };
  try { compiled(injectedRequire, module, module.exports, source, filename, runtime); }
  finally {
    if (cachedPath) { if (previousCache) require.cache[cachedPath] = previousCache; else delete require.cache[cachedPath]; }
    assert.equal(Module._load, original, "ambient hook restored even after failure"); assert.equal(loaded, false);
  }
}

test("reviewed immutable bootstrap commitments and runtime identifiers match independently", () => {
  assert.deepEqual(expectedRuntime, { node: process.version, v8: process.versions.v8, platform: process.platform, arch: process.arch });
  assert.ok(Object.isFrozen(expectedRuntime)); assert.ok(Object.isFrozen(sourcePins));
  assert.equal(sourcePins.length, 5);
  for (const pin of sourcePins) {
    assert.ok(Object.isFrozen(pin)); const bytes = fs.readFileSync(path.join(source, pin.id));
    assert.equal(bytes.length, pin.bytes, pin.id); assert.equal(digest(bytes), pin.sha256, pin.id);
  }
  assert.throws(() => { expectedRuntime.node = "unreviewed"; }, TypeError);
  assert.throws(() => { sourcePins[0].sha256 = "0".repeat(64); }, TypeError);
});

test("actual same-byte aggregate initializes under checked engine limits and uses its private profile factory", async () => {
  const config = options(), expected = digest(JSON.stringify(config.profile) + "\n");
  const ambientBefore = pins.filter(p => require.cache[path.join(source, p.id)]).map(p => p.id);
  const { report, worker } = await probe(config);
  assert.equal(report.profileSha256, expected);
  assert.deepEqual(report.engineLimits, { maxOldGenerationSizeMb: 64, maxYoungGenerationSizeMb: 16, codeRangeSizeMb: 32, stackSizeMb: 4 });
  assert.equal(report.nodeVersion, expectedRuntime.node); assert.equal(report.v8Version, expectedRuntime.v8);
  assert.equal(report.verifiedModules, 23); assert.equal(report.initializedModules, 23);
  assert.equal(report.requireCalls, 66); assert.equal(report.readBytes, 261072); assert.equal(report.ambientModuleAttempts, 0);
  assert.deepEqual(report.factoryNames, ["runTemplatePreflightProcess", "runConfiguredTemplatePreflightProcess", "validatedCheckpointConfiguration", "createTopologySourceAdmission", "profileCommitment"]);
  assert.deepEqual(report.scope, { sameByteMetadataEntryEvaluated: true, privateProfileFactoryUsed: true,
    actualHostOrAdmissionInvoked: false, engineEnvelopeObserved: true, parentDeadlineIsHardCpuProof: false,
    runtimeIdentityProved: false, totalProcessMemoryProved: false, numericalReplayComplete: false, scientificExecutionAuthorized: false });
  assert.deepEqual(worker, { exitCode: 0, terminationObserved: true, stdoutBytes: 0, stderrBytes: 0 });
  assert.ok(Object.isFrozen(report)); assert.ok(Object.isFrozen(report.scope));
  assert.deepEqual(pins.filter(p => require.cache[path.join(source, p.id)]).map(p => p.id), ambientBefore);
});

test("caller mutations after invocation cannot change private worker configuration or profile", async () => {
  const config = options(), expected = digest(JSON.stringify(config.profile) + "\n"), running = probe(config);
  config.profile.counts[0] = 900; config.repositoryRoot = "/missing";
  config.workerLimits.replyBytes = 1; config.snapshotLimits.nodes = 1;
  assert.equal((await running).report.profileSha256, expected);
});

test("accessor and malformed options reject before worker construction without evaluating getters", async () => {
  let reads = 0; const config = options();
  Object.defineProperty(config, "profile", { get() { reads++; throw Error("getter ran"); }, enumerable: true });
  await assert.rejects(probe(config), /own data field/); assert.equal(reads, 0);
  const nested = options(); Object.defineProperty(nested.profile, "label", { get() { reads++; return "bad"; }, enumerable: true });
  await assert.rejects(probe(nested), /own data property/); assert.equal(reads, 0);
  const extra = options(); extra.unreviewed = true;
  await assert.rejects(probe(extra), /closed ordered fields/);
  const cyclic = options(); cyclic.profile.self = cyclic.profile;
  await assert.rejects(probe(cyclic), /acyclic JSON metadata/);
});

test("nonpositive unsafe and oversized engineering limits reject without a worker", async () => {
  for (const [key, value] of [["timeoutMs", 0], ["timeoutMs", 60001], ["replyBytes", 65537],
    ["stdioBytes", 65537], ["maxOldGenerationSizeMb", 257], ["stackSizeMb", 1.5]]) {
    const config = options(); config.workerLimits[key] = value;
    await assert.rejects(probe(config), error => { assert.equal(error.evidence, undefined); return /limits|integer/.test(error.message); });
  }
});

test("deadline failure is returned only after worker exit, not as a successful receipt", async () => {
  const config = options(); config.workerLimits.timeoutMs = 1;
  await assert.rejects(probe(config), error => stopped(error, /parent deadline/));
});

test("pre-post reply quota rejects and observes worker termination", async () => {
  const config = options(); config.workerLimits.replyBytes = 1;
  await assert.rejects(probe(config), error => stopped(error, /pre-post reply quota/));
});

test("changed source bytes reject fixed commitments without ambient fallback", async t => {
  const { dir, target } = copiedSources(t), file = path.join(target, pins[0].id);
  const bytes = fs.readFileSync(file); bytes[0] = bytes[0] === 32 ? 33 : 32; fs.writeFileSync(file, bytes);
  const config = options(); config.repositoryRoot = dir;
  await assert.rejects(probe(config), error => stopped(error, /exact retained source hash/));
});

test("missing source and symlink source refuse initialization without fallback", async t => {
  const { dir, target } = copiedSources(t), file = path.join(target, pins[0].id);
  fs.unlinkSync(file); const config = options(); config.repositoryRoot = dir;
  await assert.rejects(probe(config), error => stopped(error, /ENOENT/));
  fs.symlinkSync(path.join(source, pins[0].id), file);
  await assert.rejects(probe(config), error => stopped(error, /nonsymlink regular source components/));
});

test("negative ambient host and source-admission imports trip before aggregate evaluation", () => {
  for (const ambientImport of ["./a68-template-process-host", "./a68-topology-source-admission"])
    for (const cached of [false, true])
      assert.throws(() => manufacturedWorkerBoundary({ ambientImport, cached }), /ambient production module forbidden/);
});

test("unreviewed runtime and corrupted bootstrap reject before aggregate evaluation", () => {
  assert.throws(() => manufacturedWorkerBoundary({ wrongRuntime: true }), /reviewed runtime identifiers before loading/);
  assert.throws(() => manufacturedWorkerBoundary({ corruptBootstrap: true }), /reviewed bootstrap source commitment/);
});

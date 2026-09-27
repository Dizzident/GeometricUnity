"use strict";
// Cross-language METADATA comparison only. The Release harness describes the
// actual C# source menus and constructor-free manufactured geometry containers;
// never scientific background/germ construction, an operator or scientific sink.
// Hashes here compact fully regenerated metadata, NOT coefficient evidence.
const test = require("node:test"), assert = require("node:assert/strict");
const { execFileSync, spawn } = require("node:child_process"), path = require("node:path"), crypto = require("node:crypto");
const { sourceContextIds, sourceContextMenu, diagnosticMenu } = require("../a68-source-context-menu");
const { createSourceTemplateValidator } = require("../a68-source-template-validator");
const { parseCanonicalWire } = require("../a68-canonical-wire");
const fixture = require("./a68-source-template-fixture");
const { Writable } = require("node:stream");
const { profileCommitment, serveTemplatePreflight } = require("../a68-template-preflight-service");
const { readContextCheckpoint, contextCheckpointIdentity } = require("../a68-context-checkpoint");
const { runTemplatePreflightProcess, runConfiguredTemplatePreflightProcess, validatedCheckpointConfiguration } = require("../a68-template-process-host");
const { createSourceCheckpointDispatcher } = require("../a68-source-checkpoint-dispatcher");
const { createSourcePreparationDriver } = require("../a68-source-preparation-driver");
const { serveSourcePreparation } = require("../a68-source-preparation-service");
const { createLaunchFileAdmission } = require("../a68-launch-file-admission");
const ordinal = (a, b) => a < b ? -1 : a > b ? 1 : 0;
const hash = value => crypto.createHash("sha256").update(JSON.stringify(value)).digest("hex");
const ids = sourceContextIds();
let rows, attempted = false, failure;
function observed() {
  if (!attempted) {
    attempted = true;
    const root = path.resolve(__dirname, "../../..");
    try {
      const text = execFileSync("dotnet", ["run", "-c", "Release", "--no-build", "--no-restore", "--project",
        path.join(__dirname, "a68-template-catalog/A68TemplateCatalog.csproj"), "--", "--source-structures"],
      { cwd: root, encoding: "utf8", maxBuffer: 16 * 1024 * 1024, timeout: 60000 });
      rows = text.trimEnd().split("\n").map(line => JSON.parse(line));
      assert.deepEqual(rows.map(row => row.id), ids, "compile the Release metadata harness first; require exact ordered705 IDs");
    } catch (error) {
      // Never retry a failed/timed-out child from a later test, nor dump its
      // multi-megabyte partial metadata as if it were accepted evidence.
      failure = new Error("C# metadata comparison unavailable: " + (error.code || error.message));
    }
  }
  if (failure) throw failure;
  return rows;
}
const expectedMarks = id => sourceContextMenu(id).marks;
const expectedLeaves = id => { const { leaves, leafRoles } = sourceContextMenu(id); return { leaves, leafRoles }; };
const expectedCallbacks = id => sourceContextMenu(id).callbacks;
test("admitted C# geometry writer matches independently assembled full14 manufactured wire and JSON+LF hashes", () => {
  // Constructor-free C# containers hold zero rationals only. This is a wire
  // contract test, not a source reconstruction or a scientific background.
  const text = execFileSync("dotnet", ["run", "-c", "Release", "--no-build", "--no-restore", "--project",
    path.join(__dirname, "a68-template-catalog/A68TemplateCatalog.csproj"), "--", "--geometry-wire-fixtures"],
  { cwd: path.resolve(__dirname, "../../.."), encoding: "utf8", maxBuffer: 8 * 1024 * 1024, timeout: 60000 });
  const rows = text.trimEnd().split("\n").map(line => JSON.parse(line));
  assert.deepEqual(rows.map(row => row.kind), ["background", "germ"]);
  const matrix = Array.from({ length: 14 }, () => Array(14).fill("0"));
  const vector = Array(14).fill(matrix), grid = Array(14).fill(vector);
  const metric = { g: matrix, d: vector, dd: grid }, connection = { gamma: vector, dGamma: grid, curvature: grid };
  const expected = [
    { point: 0, frame: matrix, inverseFrame: matrix, metric, connection, frameLift: vector, framePartial: vector, omega: vector },
    { metricBasis: 0, jetIndex: 0, multiindex: [0, 0, 0, 0], order: 0, shear: metric, deltaMetric: metric,
      blockMetric: metric, deltaConnection: connection, palatini: connection, motion: matrix,
      motionPartial: vector, motionCovariant: vector, deltaOmega: vector, deltaOmegaPartial: grid, deltaFrame: matrix }
  ];
  for (let i = 0; i < rows.length; i++) {
    assert.deepEqual(rows[i].value, expected[i], "every nested coordinate, identity and declared field");
    const wire = JSON.stringify(expected[i]) + "\n";
    assert.equal(JSON.stringify(rows[i].value) + "\n", wire, "exact ordered schema without normalization");
    assert.equal(rows[i].bytes, [538292, 1372171][i]);assert.equal(rows[i].bytes, Buffer.byteLength(wire));
    assert.equal(rows[i].sha256, crypto.createHash("sha256").update(wire, "ascii").digest("hex"));
    assert.notEqual(rows[i].sha256, hash(expected[i]), "geometry commitment includes LF, unlike tensor fingerprint");
  }
});
test("full independent diagnostic menus match all11 C# fields in declared order, not sorted derivative sets", () => {
  const text = execFileSync("dotnet", ["run", "-c", "Release", "--no-build", "--no-restore", "--project",
    path.join(__dirname, "a68-template-catalog/A68TemplateCatalog.csproj"), "--", "--diagnostic-menus"],
  { cwd: path.resolve(__dirname, "../../.."), encoding: "utf8", maxBuffer: 1024 * 1024, timeout: 60000 });
  const menus = text.trimEnd().split("\n").map(line => JSON.parse(line));
  assert.deepEqual(menus.map(menu => menu.id), ["grade10", "acceleration", "secondJets"]);
  const fields = ["tensors", "scalarArrays", "originalActions", "wardActions", "accelerations", "secondJets", "checks"];
  const categories = ["Tensor", "ScalarArray", "Original", "Ward", "Acceleration", "SecondJet", "Check"];
  const census = [[3082, 10, 8, 0, 0, 0, 1019], [3, 3, 2, 4, 2, 0, 310], [0, 0, 0, 0, 0, 420, 420]];
  for (const [i, actual] of menus.entries()) {
    const expected = diagnosticMenu(actual.id), callbacks = sourceContextMenu("diagnostic/" + actual.id).callbacks;
    assert.deepEqual(Object.keys(actual), ["id", ...fields, "backgrounds", "germs", "variationRows"]);
    assert.deepEqual(actual, expected, "all ordered arrays and counters, without normalization");
    assert.deepEqual(fields.map(field => actual[field].length), census[i]);
    assert.deepEqual([actual.backgrounds, actual.germs, actual.variationRows], [[1, 4, 4], [1, 1, 0], [0, 0, 0]][i]);
    fields.forEach((field, j) => {
      assert.equal(new Set(actual[field]).size, actual[field].length, "unique " + field);
      assert.deepEqual([...actual[field]].sort(ordinal), callbacks.filter(c => c.category === categories[j]).map(c => c.name), "complete derivative name set " + field);
      assert.ok(Object.isFrozen(expected[field]));
    });
    assert.ok(Object.isFrozen(expected));
  }
  const grade = menus[0];
  assert.equal(grade.tensors[4], "combined/motionX"); assert.equal(grade.tensors[14], "basis0/intermediate/fixed_p0_s0_value");
  assert.notDeepEqual(grade.tensors, sourceContextMenu("diagnostic/grade10").callbacks.filter(c => c.category === "Tensor").map(c => c.name));
  assert.throws(() => diagnosticMenu("diagnostic/grade10"), /exact declared diagnostic ID/);
  assert.throws(() => diagnosticMenu(null), /exact declared diagnostic ID/);
  assert.notEqual(diagnosticMenu("grade10"), diagnosticMenu("grade10"), "no shared mutable or eager menu cache");
});
test("all705 C# source mark menus match independent JS names degrees and required retention", () => {
  let total = 0;
  for (const row of observed()) {
    const expected = [...expectedMarks(row.id)].sort((a, b) => ordinal(a.name, b.name));
    assert.equal(row.markCount, expected.length, row.id); assert.equal(row.marks, hash(expected), row.id); total += expected.length;
  }
  assert.equal(total, 291199); assert.equal(observed().filter(row => row.markCount === 3976).length, 6);
});
test("all705 source leaf declarations preserve exact lineage and sole27-export alias", () => {
  let total = 0;
  for (const row of observed()) {
    const expected = expectedLeaves(row.id); assert.deepEqual(row.leaves, expected.leaves, row.id); assert.deepEqual(row.leafRoles, expected.leafRoles, row.id); total += row.leaves.length;
    assert.ok(row.leaves.every(l => !Object.hasOwn(l, "sha256")), "no fabricated coefficient hash");
  }
  assert.equal(total, 20316);
});
test("all705 complete callback censuses match separate JS metadata including diagnostics and Ward routes", () => {
  for (const row of observed()) {
    const expected = expectedCallbacks(row.id);
    assert.equal(new Set(expected.map(c => c.category + "/" + c.name)).size, expected.length);
    assert.equal(row.callbackCount, expected.length, row.id); assert.equal(row.callbacks, hash(expected), row.id);
  }
});
async function actualSourceTemplates(accept) {
  let count = 0, bytes = 0;
  await new Promise((resolve, reject) => {
    // The child emits metadata only. Read one bounded canonical line at a time;
    // do not retain hundreds of megabytes of all-context templates in a fixture.
    const child = spawn("dotnet", ["run", "-c", "Release", "--no-build", "--no-restore", "--project",
      path.join(__dirname, "a68-template-catalog/A68TemplateCatalog.csproj"), "--", "--source-templates"],
    { cwd: path.resolve(__dirname, "../../.."), stdio: ["ignore", "pipe", "pipe"] });
    let pending = Buffer.alloc(0), failure = null, stderr = "";
    const fail = error => { if (!failure) { failure = error; child.kill(); } };
    const timeout = setTimeout(() => fail(new Error("metadata-only child timeout; no retry")), 60000);
    child.once("error", error => { clearTimeout(timeout); reject(new Error("metadata-only child unavailable: " + error.code)); });
    child.stderr.on("data", data => { if (stderr.length + data.length > 8192) fail(new Error("metadata child stderr quota")); else stderr += data.toString("utf8"); });
    child.stdout.on("data", data => {
      if (failure) return;
      try {
        bytes += data.length; assert.ok(bytes <= 512 * 1024 * 1024, "prospective total metadata transport ceiling");
        assert.ok(pending.length + data.length <= 2000000, "prospective partial-line transport ceiling"); pending = Buffer.concat([pending, data]);
        let lf;
        while ((lf = pending.indexOf(10)) >= 0) {
          const line = pending.subarray(0, lf + 1); pending = pending.subarray(lf + 1);
          const candidate = parseCanonicalWire(line, { graphBytes: 2000000 });
          assert.equal(candidate.id, ids[count], "exact complete producer context sequence");
          accept(candidate, count); count++;
        }
      } catch (error) { fail(error); }
    });
    child.once("close", code => {
      clearTimeout(timeout);
      if (failure) { reject(failure); return; }
      try { assert.equal(code, 0, stderr); assert.equal(stderr, ""); assert.equal(pending.length, 0, "complete LF-terminated metadata"); assert.equal(count, 705); resolve(); }
      catch (error) { reject(error); }
    });
  });
  return { count, bytes };
}
test("production validator accepts all705 ACTUAL C# template wire records against a separately supplied profile", async () => {
  const profile = fixture.profile(), validator = createSourceTemplateValidator(profile, fixture.limits, id => assert.ok(ids.includes(id)), () => {});
  validator.validateCaptureDeclaration(profile.capture);
  const { bytes } = await actualSourceTemplates(candidate => validator.validateTemplate(candidate));
  const result = validator.finish(); assert.equal(result.contexts, 705); assert.equal(result.marks, 291199); assert.equal(result.leaves, 20316); assert.equal(result.callbacks, 940365);
  assert.equal(result.scope.scientificExecutionAuthorized, false); assert.ok(bytes > 0);
});
async function duplexPreflight(mutateReply = null, mutateProfile = null) {
  const profile = fixture.profile(), replies = []; if (mutateProfile) mutateProfile(profile);
  const sha = profileCommitment(profile, fixture.limits);
  // Execute the BUILT Release assembly directly so the test owns one client
  // process, without a `dotnet run` launcher in its EOF/exit lifecycle.
  // Dedicated descriptors avoid the runtime's retained stdout alias. The
  // client must close its request pipe before receiving the terminal ACK.
  const child = spawn("dotnet", ["exec", path.join(__dirname, "a68-template-catalog/bin/Release/net10.0/A68TemplateCatalog.dll"), "--preflight-client", sha],
  { cwd: path.resolve(__dirname, "../../.."), stdio: ["ignore", "ignore", "pipe", "pipe", "pipe"] });
  const requests = child.stdio[3], replyInput = child.stdio[4];
  let stderr = "", spawnError = null, timedOut = false;
  replyInput.on("error", () => {});
  child.stderr.on("data", data => { if (stderr.length + data.length > 8192) child.kill(); else stderr += data.toString("utf8"); });
  const exit = new Promise(resolve => { child.once("error", error => { spawnError = error; resolve(null); }); child.once("close", resolve); });
  const timer = setTimeout(() => { timedOut = true; child.kill(); }, 60000);
  const output = new Writable({
    write(chunk, _encoding, callback) {
      try {
        const reply = JSON.parse(chunk); replies.push({ sequence: reply.sequence, operation: reply.operation });
        replyInput.write(mutateReply ? mutateReply(reply, chunk) : chunk, callback);
      } catch (error) { callback(error); }
    },
    final(callback) { replyInput.end(callback); },
    destroy(error, callback) { replyInput.destroy(); callback(error); }
  });
  let result, failure;
  try {
    result = await serveTemplatePreflight({ input: requests, output, profile, snapshotLimits: fixture.limits,
      limits: { frameBytes: 2000000, totalInputBytes: 536870912, replyBytes: 4096, totalOutputBytes: 4194304, timeoutMs: 60000 }, admitConstruction() {}, admitTopology() {} });
  } catch (error) { failure = error; }
  const code = await exit; clearTimeout(timer);
  assert.equal(timedOut, false, "fixture child completed without retry"); assert.equal(spawnError, null);
  return { code, stderr, result, failure, replies };
}
test("actual C# coordinator joins full705 catalog freeze to all707 production JS exchanges without leaf binding", async () => {
  const result = await duplexPreflight(); assert.equal(result.code, 0, JSON.stringify({ stderr: result.stderr, lastReplies: result.replies.slice(-2), serviceFailure: result.failure?.message })); assert.equal(result.failure, undefined);
  assert.equal(result.replies.length, 707); assert.equal(result.result.requests, 707);
  assert.equal(result.result.report.contexts, 705); assert.equal(result.result.report.callbacks, 940365);
  assert.equal(result.result.report.scope.scientificExecutionAuthorized, false);
});
test("actual C# client refuses an acknowledgement for different request bytes", async () => {
  const result = await duplexPreflight(reply => Buffer.from(JSON.stringify({ ...reply, requestSha256: "0".repeat(64) }) + "\n"));
  assert.equal(result.code, 1); assert.match(result.stderr, /exact independently expected bound acknowledgement/);
  assert.equal(result.replies.length, 1); assert.ok(result.failure); assert.equal(result.result, undefined);
});
test("actual C# capture declaration cannot hide behind a matching independently changed profile hash", async () => {
  const result = await duplexPreflight(null, p => p.capture.limits.normal.graphs++);
  assert.equal(result.code, 1); assert.equal(result.replies.length, 0); assert.equal(result.result, undefined);
  assert.match(result.failure.message, /capture declaration/);
});
test("actual C# matching capture declaration cannot obtain Begin ACK with undersized trace topology", async () => {
  const result = await duplexPreflight(null, p => p.budgets.find(b => b.id === "diagnostic/grade10").resources.trace.nodes = 100000);
  assert.equal(result.code, 1); assert.equal(result.replies.length, 0); assert.equal(result.result, undefined);
  assert.match(result.failure.message, /prospective topology nodes\/marks/);
});
test("actual C# client refuses trailing output even after an otherwise valid full-set acknowledgement", async () => {
  const result = await duplexPreflight((reply, bytes) => reply.operation === "finish" ? Buffer.concat([bytes, Buffer.from("{}\n")]) : bytes);
  assert.equal(result.code, 1); assert.match(result.stderr, /queued data|terminal reply EOF/); assert.equal(result.replies.length, 707);
});

function hostedPreflight(tail = null, configured = false) {
  const fs = require("node:fs"), profile = fixture.profile(), sha = profileCommitment(profile, fixture.limits);
  const dotnet = (process.env.PATH ?? "").split(path.delimiter).map(dir => path.resolve(dir, "dotnet")).find(file => fs.existsSync(file));
  assert.ok(dotnet, "explicit installed dotnet path required for metadata-only fixture");
  const executable = fs.realpathSync(dotnet), timeoutMs = tail === "linger" ? 15000 : 60000;
  const options = {
    launch: { executable, arguments: ["exec", path.join(__dirname, "a68-template-catalog/bin/Release/net10.0/A68TemplateCatalog.dll"),
      tail === null ? "--preflight-client" : "--preflight-client-tail", sha, ...(tail === null ? [] : [tail])],
      workingDirectory: path.resolve(__dirname, "../../.."), environment: { DOTNET_ROOT: path.dirname(executable) } },
    profile, snapshotLimits: fixture.limits,
    transportLimits: { frameBytes: 2000000, totalInputBytes: 536870912, replyBytes: 4096, totalOutputBytes: 4194304, timeoutMs },
    processLimits: { timeoutMs, killGraceMs: 500, stdoutBytes: 8192, stderrBytes: 8192 },
    admitLaunch() {}, admitConstruction() {}, admitTopology() {}
  };
  // Pin the explicitly declared test executable/managed artifacts. These pins
  // are manufactured from this built fixture, NOT a reviewed complete runtime
  // closure or proof that an independently frozen source produced the binary.
  const directory = path.join(__dirname, "a68-template-catalog/bin/Release/net10.0");
  const files = [executable, ...["A68TemplateCatalog.dll", "A68TemplateCatalog.deps.json", "A68TemplateCatalog.runtimeconfig.json"].map(name => path.join(directory, name)),
    path.join(options.launch.workingDirectory, "studies/phase627_full_mixed_metric_native_field_variation_audit_001/bin/Release/net10.0/Phase627FullMixedMetricNativeFieldVariationAudit.dll")].sort();
  const pins = files.map(file => { const bytes = fs.readFileSync(file); return { path: file, bytes: bytes.length, sha256: crypto.createHash("sha256").update(bytes).digest("hex") }; });
  const admission = createLaunchFileAdmission({ expected: { launch: options.launch, profileSha256: sha,
    processLimits: options.processLimits, transportLimits: options.transportLimits }, pins, argumentFiles: [1],
    limits: { files: pins.length, fileBytes: Math.max(...pins.map(pin => pin.bytes)), totalReadBytes: pins.reduce((n, pin) => n + pin.bytes + 1, 0), chunkBytes: 65536, snapshot: fixture.limits } });
  options.admitLaunch = admission.admitLaunch;
  const policy = fixture.checkpointPolicy();
  const run = configured ? runConfiguredTemplatePreflightProcess(options, policy, { validator: fixture.limits, snapshot: fixture.limits }) : runTemplatePreflightProcess(options);
  // Host detached all configuration before its first external callback/await.
  if (configured) policy.routes[0].contextId = "caller-mutated-after-launch";
  return run.then(result => {
    assert.equal(admission.snapshot().completed, true); assert.equal(admission.snapshot().scope.sourceToBinaryCorrespondenceProved, false); return result;
  });
}
test("production metadata process host requires all707 exchanges AND clean C# process exit", async () => {
  const result = await hostedPreflight(); assert.equal(result.preflight.requests, 707);
  assert.deepEqual(result.process, { exitCode: 0, signal: null, stdoutBytes: 0, stderrBytes: 0, ownedProcessGroupGone: true });
  assert.equal(result.scope.scientificExecutionAuthorized, false); assert.equal(result.scope.executableClosurePinnedHere, false);
  assert.equal(result.scope.allDescendantsContained, false); assert.equal(result.scope.hardCpuDeadlineProved, false);
  assert.throws(() => validatedCheckpointConfiguration(result), /private clean configured process/);
});
test("configured actual C# catalog yields private707 declaration commitments only after clean process completion", async () => {
  const result = await hostedPreflight(null, true), identity = validatedCheckpointConfiguration(result), config = identity.configuration;
  assert.equal(config.events.length, 707);assert.equal(identity.health().failed, false);
  assert.ok(Object.isFrozen(config.events));assert.ok(Object.isFrozen(config.profiles));
  const profile = fixture.profile();
  const digest = value => crypto.createHash("sha256").update(JSON.stringify(value) + "\n").digest("hex");
  const commitments = identity.producerCommitments;
  assert.equal(commitments.profileSha256, digest(profile));assert.equal(commitments.captureSha256, digest(profile.capture));
  assert.equal(commitments.templates.length, 705);let firstTemplate;
  // A separate metadata-only C# process regenerates the FULL templates. No
  // normalization/sorting or fixture-produced template can stand in for it.
  const regenerated = await actualSourceTemplates((candidate, index) => {
    assert.deepEqual(commitments.templates[index], { contextId: candidate.id, templateSha256: digest(candidate) });
    if (index === 0) firstTemplate = candidate;
  });
  assert.equal(regenerated.count, 705);assert.ok(regenerated.bytes > 0);
  for (const i of [0, 1, 350, 351, 352, 702, 703, 704, 705, 706]) {
    const row = config.events[i], template = fixture.template(row.contextId, profile);
    const declaration = row.kind === "final" ? { contextId: template.id, graphPath: template.graphPath,
      backgroundPath: template.pointCheckpoint.relativePath, metadataPath: template.metadataPath } :
      { contextId: template.id, graphPath: template.graphPath, metadataPath: row.kind === "background" ? template.pointCheckpoint.relativePath : template.metadataPath, marks: template.marks };
    assert.equal(row.declarationSha256, crypto.createHash("sha256").update(JSON.stringify(declaration) + "\n").digest("hex"));
  }
  for (const fake of [{ ...result }, result.preflight, identity, config])
    assert.throws(() => validatedCheckpointConfiguration(fake), /private clean configured process/);
  const driver = createSourceCheckpointDispatcher(result, { configuration: fixture.limits, event: fixture.limits, total: fixture.limits });
  assert.equal(driver.snapshot().acceptedEvents, 0);assert.equal(driver.snapshot().completed, false);
  // No source adapter, leaf or file replay is run. Configuration alone cannot
  // substitute a caller-created source adapter or authorize science.
  assert.throws(() => driver.dispatch({}, {}, {}), /private source adapter identity/);
  assert.equal(driver.snapshot().failed, true);assert.equal(result.scope.scientificExecutionAuthorized, false);
  let stages = 0;
  const newOwner = () => createSourcePreparationDriver(result, fixture.preparationPolicy(path.resolve(__dirname, "../../..")),
    { snapshot: fixture.limits, dispatcher: { configuration: fixture.limits, event: fixture.limits, total: fixture.limits } }, () => { stages++;throw Error("no scientific preparation authorized"); });
  const owner = newOwner();
  assert.equal(owner.snapshot().acceptedEvents, 0);assert.equal(owner.snapshot().retainedInputReaders, 0);assert.equal(stages, 0);
  assert.throws(() => owner.prepareNext({ contextId: "point0", graphPath: "wrong", metadataPath: "wrong", marks: [] }), /committed declaration/);
  assert.equal(stages, 0);assert.equal(owner.snapshot().failed, true);assert.equal(owner.snapshot().scope.scientificExecutionAuthorized, false);
  const declaration = { contextId: firstTemplate.id, graphPath: firstTemplate.graphPath,
    metadataPath: firstTemplate.pointCheckpoint.relativePath, marks: firstTemplate.marks };
  for (const mutate of [t => { t.resources.fileBytes++; }, t => { t.pointCheckpoint.bytes++; },
    t => { t.leafRoles[Object.keys(t.leafRoles)[0]] = "changed"; }, t => t.leaves.reverse(),
    t => { t.callbacks.callbacks[0].name = "changed"; }]) {
    const changed = fixture.clone(firstTemplate);mutate(changed);
    const rejected = newOwner();rejected.beginProducer(profile, profile.capture);
    assert.throws(() => rejected.prepareNext(declaration, changed), /full regenerated producer template/);
    assert.equal(rejected.snapshot().failed, true);assert.equal(stages, 0);
  }
  const acceptedMetadata = newOwner();acceptedMetadata.beginProducer(profile, profile.capture);
  // Correct full metadata reaches the FIRST admission, which deliberately
  // throws before RetainedMixedInputs or any scientific constructor is called.
  assert.throws(() => acceptedMetadata.prepareNext(declaration, firstTemplate), /no scientific preparation authorized/);
  assert.equal(stages, 1);assert.equal(acceptedMetadata.snapshot().retainedInputReaders, 0);
  assert.equal(acceptedMetadata.snapshot().failed, true);assert.equal(acceptedMetadata.snapshot().scope.producerIdentityEstablished, false);
  await actualProducerPrefix(result);
});
async function actualProducerPrefix(preflightResult) {
  // Separate actual C# catalog/client/coordinator, real JS transport/driver.
  // Stop at first admission BEFORE retained/source construction, never a sink.
  const child = spawn("dotnet", ["exec", path.join(__dirname, "a68-template-catalog/bin/Release/net10.0/A68TemplateCatalog.dll"), "--source-preparation"],
    { cwd: path.resolve(__dirname, "../../.."), stdio: ["ignore", "pipe", "pipe", "pipe", "pipe"] });
  let stderr = "", stdout = "", spawnError = null, timedOut = false, stages = 0;
  const replies = [];
  child.stdio[4].on("error", () => {});
  for (const [stream, label] of [[child.stdout, "stdout"], [child.stderr, "stderr"]]) stream.on("data", data => {
    if (stdout.length + stderr.length + data.length > 8192) { child.kill(); return; }
    if (label === "stdout") stdout += data.toString("utf8");else stderr += data.toString("utf8");
  });
  const exited = new Promise(resolve => { child.once("error", error => { spawnError = error; resolve(null); });child.once("close", resolve); });
  const timer = setTimeout(() => { timedOut = true;child.kill("SIGKILL"); }, 60000);
  const output = new Writable({
    write(chunk, _encoding, callback) {
      try { const reply = JSON.parse(chunk);replies.push({ sequence: reply.sequence, operation: reply.operation });child.stdio[4].write(chunk, callback); }
      catch (error) { callback(error); }
    }, final(callback) { child.stdio[4].end(callback); }, destroy(error, callback) { child.stdio[4].destroy();callback(error); }
  });
  let failure, result;
  try {
    result = await serveSourcePreparation({ input: child.stdio[3], output, preflightResult,
      preparationPolicy: fixture.preparationPolicy(path.resolve(__dirname, "../../..")),
      preparationLimits: { snapshot: fixture.limits, dispatcher: { configuration: fixture.limits, event: fixture.limits, total: fixture.limits } },
      snapshotLimits: fixture.limits,
      limits: { frameBytes: 2000000, totalInputBytes: 536870912, replyBytes: 100000, totalOutputBytes: 8388608, timeoutMs: 60000 },
      admitStage(descriptor) { stages++;assert.equal(descriptor.operation, "retained-inputs");throw Error("FIRST source preparation intentionally denied"); } });
  } catch (error) { failure = error; }
  const code = await exited;clearTimeout(timer);
  assert.equal(timedOut, false);assert.equal(spawnError, null);assert.equal(stdout, "");assert.equal(code, 1, stderr);
  assert.match(failure?.message ?? "", /FIRST source preparation intentionally denied/);assert.equal(result, undefined);
  assert.equal(stages, 1);assert.equal(replies.length, 707);
  assert.deepEqual(replies[0], { sequence: 0, operation: "begin" });assert.deepEqual(replies.at(-1), { sequence: 706, operation: "freeze" });
  assert.match(stderr, /reply stream ended before acknowledgement/);
}
test("completed C# protocol cannot hide a later nonzero process exit", async () => {
  await assert.rejects(hostedPreflight("exit7", true), error => {
    assert.match(error.message, /clean zero child exit/); assert.equal(error.evidence.exitCode, 7);
    assert.equal(error.evidence.protocolCompleted, true);
    assert.equal(error.evidence.cleanupConfirmed, true); return true;
  });
});
test("completed C# protocol cannot hide undeclared stdout even with zero exit", async () => {
  await assert.rejects(hostedPreflight("stdout", true), error => {
    assert.match(error.message, /no undeclared stdout/); assert.equal(error.evidence.exitCode, 0);
    assert.equal(error.evidence.protocolCompleted, true);
    assert.equal(Buffer.from(error.evidence.stdoutBase64, "base64").toString(), "unexpected metadata fixture stdout\n");
    assert.equal(error.evidence.cleanupConfirmed, true); return true;
  });
});
test("host deadline still applies after C# protocol completion while process remains live", async () => {
  await assert.rejects(hostedPreflight("linger", true), error => {
    assert.match(error.message, /whole-process deadline/); assert.equal(error.evidence.cleanupConfirmed, true);
    assert.equal(error.evidence.protocolCompleted, true);
    assert.ok(error.evidence.signal); return true;
  });
});

test("actual C# computational envelope is read-pinned by JS without claiming independent acceptance", t => {
  const fs = require("node:fs"), os = require("node:os"), root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-csharp-envelope-"));
  t.after(() => fs.rmSync(root, { recursive: true, force: true }));
  const bytes = execFileSync("dotnet", ["exec", path.join(__dirname, "a68-template-catalog/bin/Release/net10.0/A68TemplateCatalog.dll"), "--context-envelope"], { maxBuffer: 10000, timeout: 10000 });
  const graph = Buffer.from(JSON.stringify({ schemaVersion: "phase627-typed-mixed-dag-v1", leaves: [], nodes: [], marks: [] }) + "\n"), envelope = JSON.parse(bytes);
  fs.writeFileSync(path.join(root, "graph.json"), graph); fs.writeFileSync(path.join(root, "metadata.json"), bytes);
  const declaration = { contextId: "diagnostic/secondJets", graphPath: "graph.json", metadataPath: "metadata.json", marks: [] };
  const observed = { context: declaration.contextId, graph: envelope.graph,
    metadata: { path: "metadata.json", bytes: bytes.length, sha256: crypto.createHash("sha256").update(bytes).digest("hex") } };
  const token = readContextCheckpoint(root, declaration, observed, { graphBytes: 10000, metadataBytes: 10000, totalReadBytes: 20002,
    snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } });
  const pinned = contextCheckpointIdentity(token); assert.deepEqual(pinned.envelope, envelope);
  assert.equal(pinned.envelope.independentValidationComplete, false); assert.equal(pinned.scope.independentContextAccepted, false);
});

test("actual C# point-final envelope binds the sealed metadata in JS without asserting child replay", t => {
  const fs = require("node:fs"), os = require("node:os"), root = fs.mkdtempSync(path.join(os.tmpdir(), "a68-csharp-point-final-"));
  const { readPointCompletion, pointCompletionIdentity } = require("../a68-point-completion");
  t.after(() => fs.rmSync(root, { recursive: true, force: true }));fs.mkdirSync(path.join(root, "point0"));
  const text = execFileSync("dotnet", ["exec", path.join(__dirname, "a68-template-catalog/bin/Release/net10.0/A68TemplateCatalog.dll"), "--point-completion-envelope"], { maxBuffer: 10000, timeout: 10000 });
  const { background, final } = JSON.parse(text), wire = value => Buffer.from(JSON.stringify(value) + "\n");
  const backgroundBytes = wire(background), finalBytes = wire(final);
  fs.writeFileSync(path.join(root, "point0/background-checkpoint.json"), backgroundBytes);
  fs.writeFileSync(path.join(root, "point0/metadata.json"), finalBytes);
  const declaration = { contextId: "point0", graphPath: "point0/graph.json", backgroundPath: "point0/background-checkpoint.json", metadataPath: "point0/metadata.json" };
  const observed = { context: "point0", background: { context: "point0", graph: background.graph, metadata: final.backgroundCheckpoint },
    metadata: { path: declaration.metadataPath, bytes: finalBytes.length, sha256: crypto.createHash("sha256").update(finalBytes).digest("hex") } };
  const identity = pointCompletionIdentity(readPointCompletion(root, declaration, observed, { backgroundBytes: 10000, metadataBytes: 10000, totalReadBytes: 20002,
    snapshot: { nodes: 10000, arraySlots: 10000, stringCharacters: 100000, maxDepth: 16 } }));
  assert.deepEqual(identity.background, background);assert.deepEqual(identity.completion, final);
  assert.equal(identity.scope.pointTraversalComplete, false);assert.equal(identity.scope.childReplaysAccepted, false);
});
